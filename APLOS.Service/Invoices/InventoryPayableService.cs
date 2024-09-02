using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Accounts;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.Model.Invoices;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Systems;
using Library.Model.Taxations;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Extension.Accounts;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Taxations;
using Library.Service.Vouchers;
using Library.ViewModel.Inventory;
using Library.ViewModel.Invoices;
using Library.ViewModel.Materials;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Library.Service.Invoices
{
    public class InventoryPayableService : IInventoryPayableService
    {
        #region Constructor

        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceWriteOffService _invoiceWriteOffService;
        private readonly IAdjustmentNoteService _adjustmentNoteService;
        private readonly IInvoiceTaxService _invoiceTaxService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IVoucherService _voucherService;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IRepositoryAsync<OtherInvoice> _otherInvoiceRepository;
        private readonly IRepositoryAsync<InvoiceWriteOffDetail> _invoiceWriteOffDetailRepository;
        private readonly IRepositoryAsync<AdjustmentNoteDetail> _AdjustmentNoteDetailRepository;
        private readonly IRepositoryAsync<InvoiceTax> _invoiceTaxRepository;
        private readonly IRepositoryAsync<InvoiceTaxDetail> _invoiceTaxDetailRepository;
        private readonly IRepositoryAsync<EmployeePayableDetail> _employeePayableDetailRepository;
        private readonly IRepositoryAsync<ServiceAcknowledgementMaster> _serviceAcknowledgementMasterRepository;
        private readonly IRepositoryAsync<ServiceAcknowledgementDetail> _serviceAcknowledgementDetailRepository;
        private readonly IRepositoryAsync<InventoryReceive> _inventoryReceiveRepository;
        private readonly IRepositoryAsync<InventorySales> _inventorySalesRepository;
        private readonly IRepositoryAsync<InventorySalesDetail> _inventorySalesDetailRepository;
        private readonly IRepositoryAsync<InventoryReceiveDetail> _inventoryReceiveDetailRepository;
        private readonly IInventoryIssueJournalService _inventoryIssueService;
        private readonly IRepositoryAsync<PurchaseDocAcceptance> _purchaseDocAcceptanceRepository;
        private readonly IRepositoryAsync<PurchaseDocAcceptanceCharges> _purchaseDocAcceptanceServiceRepository;
        private readonly IRepositoryAsync<PurchaseDocAcceptanceDetail> _purchaseDocAcceptanceDetailRepository;
        private readonly IRepositoryAsync<PurchaseDocAcceptanceTax> _purchaseDocAcceptanceServiceTaxRepository;
        private readonly IEmployeePayableService _employeePayableService;
        private readonly IRepositoryAsync<AdditionalTax> _additionalTaxRepository;
        private readonly IRepositoryAsync<AdditionalTaxDetail> _additionalTaxDetailRepository;
        private readonly IRepositoryAsync<GRNAcceptanceMap> _gRNAcceptanceMapRepository;
        private readonly IRepositoryAsync<PurchaseReturn> _purchaseReturnRepository;
        private readonly IRepositoryAsync<EmployeeSubsequentTransaction> _employeeSubsequentTransactionRepository;
        private readonly IRepositoryAsync<InventoryReceiveTax> _inventoryReceiveTaxRepository;
        private readonly IRepositoryAsync<InventoryIssueReturn> _InventoryIssueReturnRepository;

        public InventoryPayableService(
            IInvoiceService invoiceService
            , IInvoiceWriteOffService invoiceWriteOffService
            , IAdjustmentNoteService adjustmentNoteService
            , IInvoiceTaxService invoiceTaxService
            , IRepositoryAsync<OtherInvoice> otherInvoiceRepository
            , IRepositoryAsync<InvoiceWriteOffDetail> invoiceWriteOffDetailRepository
            , IRepositoryAsync<AdjustmentNoteDetail> AdjustmentNoteDetailRepository
            , IRepositoryAsync<InvoiceTax> invoiceTaxRepository
            , IRepositoryAsync<InvoiceTaxDetail> invoiceTaxDetailRepository
            , IUnitOfWork unitOfWork
            , IVoucherService voucherService
            , ISqlRepository sqlRepository
            , IRepositoryAsync<InventoryReceive> inventoryReceiveRepository
            , IRepositoryAsync<InventorySales> inventorySalesRepository
            , IRepositoryAsync<InventorySalesDetail> inventorySalesDetailRepository
            , IInventoryIssueJournalService inventoryIssueService
            , IPKGeneratorService pkGeneratorService
            , IEmployeePayableService employeePayableService
            , IRepositoryAsync<EmployeePayableDetail> employeePayableDetailRepository
            , IRepositoryAsync<InventoryReceiveDetail> inventoryReceiveDetailRepository
            , IRepositoryAsync<ServiceAcknowledgementMaster> serviceAcknowledgementMasterRepository
            , IRepositoryAsync<ServiceAcknowledgementDetail> serviceAcknowledgementDetailRepository
            , IRepositoryAsync<PurchaseDocAcceptance> purchaseDocAcceptanceRepository
            , IRepositoryAsync<PurchaseDocAcceptanceCharges> purchaseDocAcceptanceServiceRepository
            , IRepositoryAsync<PurchaseDocAcceptanceDetail> purchaseDocAcceptanceDetailRepository
            , IRepositoryAsync<PurchaseDocAcceptanceTax> purchaseDocAcceptanceServiceTaxRepository
            , IRepositoryAsync<AdditionalTax> additionalTaxRepository
            , IRepositoryAsync<AdditionalTaxDetail> additionalTaxDetailRepository
            , IRepositoryAsync<GRNAcceptanceMap> gRNAcceptanceMapRepository
            , IRepositoryAsync<PurchaseReturn> purchaseReturnRepository
             , IRepositoryAsync<EmployeeSubsequentTransaction> employeeSubsequentTransactionRepository
            , IRepositoryAsync<InventoryReceiveTax> inventoryReceiveTaxRepository
            , IRepositoryAsync<InventoryIssueReturn> InventoryIssueReturnRepository

            ) //: base( unitOfWork, pkGeneratorService)
        {
            _invoiceService = invoiceService;
            _pkGeneratorService = pkGeneratorService;
              _invoiceWriteOffService = invoiceWriteOffService;
            _adjustmentNoteService = adjustmentNoteService;
            _invoiceTaxService = invoiceTaxService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _otherInvoiceRepository = otherInvoiceRepository;
            _invoiceWriteOffDetailRepository = invoiceWriteOffDetailRepository;
            _AdjustmentNoteDetailRepository = AdjustmentNoteDetailRepository;
            _invoiceTaxRepository = invoiceTaxRepository;
            _invoiceTaxDetailRepository = invoiceTaxDetailRepository;
            _voucherService = voucherService;
            _inventoryReceiveRepository = inventoryReceiveRepository;
            _inventorySalesRepository = inventorySalesRepository;
            _inventoryIssueService = inventoryIssueService;
            _employeePayableService = employeePayableService;
            _employeePayableDetailRepository = employeePayableDetailRepository;
            _inventoryReceiveDetailRepository = inventoryReceiveDetailRepository;
            _purchaseDocAcceptanceRepository = purchaseDocAcceptanceRepository;
            _purchaseDocAcceptanceServiceRepository = purchaseDocAcceptanceServiceRepository;
            _purchaseDocAcceptanceDetailRepository = purchaseDocAcceptanceDetailRepository;
            _purchaseDocAcceptanceServiceTaxRepository = purchaseDocAcceptanceServiceTaxRepository;
            _serviceAcknowledgementMasterRepository = serviceAcknowledgementMasterRepository;
            _serviceAcknowledgementDetailRepository = serviceAcknowledgementDetailRepository;
            _inventorySalesDetailRepository = inventorySalesDetailRepository;
            _additionalTaxRepository = additionalTaxRepository;
            _additionalTaxDetailRepository = additionalTaxDetailRepository;
            _gRNAcceptanceMapRepository = gRNAcceptanceMapRepository;
            _purchaseReturnRepository = purchaseReturnRepository;
            _employeeSubsequentTransactionRepository = employeeSubsequentTransactionRepository;
            _inventoryReceiveTaxRepository = inventoryReceiveTaxRepository;
            _InventoryIssueReturnRepository = InventoryIssueReturnRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetVendorPayableGLBudgetActivity(string receiveId, string companyId, string plantId, string partyAccountGroupId)
        {
            var sql = @"DECLARE @receiveId varchar(10)='" + receiveId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + "', @partyAccountGruopId varchar(10)='" + partyAccountGroupId + @"',@countryId varchar(10)

                            SELECT distinct IR.Id,IRD.Id AS InventoryReceiveDetailId, 'Vendor' AS OtherName, 'Cr' AS TrnType ,MM.MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGPGL.GLGeneralInfoId  ELSE FAG.VendorReconGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGPGL.BudgetMasterId  ELSE FAG.VendorReconBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGPGL.ActivityId  ELSE FAG.VendorReconActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id

                        LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAVGL.VendorReconGLId ,FAVGL.VendorReconBudgetMasterId,FAVGL.VendorReconActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT 
						LEFT JOIN HKP.FixedAssetMasterVendorReconGL FAVGL ON 
						FAMBT.FixedAssetMasterId=FAVGL.FixedAssetMasterId  AND FAVGL.PartyAccountGroupId=@partyAccountGruopId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId

						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.VendorReconGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.VendorReconBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.VendorReconActivityId= AF.Id

						WHERE IRD.InventoryReceiveId=@receiveId";
            return _sqlRepository.GetDataCollection(sql);
        }

        
       
        public string InsertInventoryPayable(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
            , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<InvoiceTaxViewModel> additionalTaxList
            , IEnumerable<VoucherDetailViewModel> otherVendorChargesList )
        {
            var flag = false;
            try
            {
                DataSet _adtaxDetaildataset = null;
                DataSet _adtaxdataset = null;
                string voucherNo = null;
                var receiveData = _inventoryReceiveRepository.Find(receiveId);
                voucherVM.PostingDate = receiveData.GRNDate;
                if (voucherVM.IsInvoice == true && receiveData.IsFOC == false)
                {
                    AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                    _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                    _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                    _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                    //var receiveData = _sqlRepository.GetDataTable(@"Select * from TRN.InventoryReceive where Id = '" + receiveId + @"'").Rows[0];
                   
                    if (receiveData.Status == "Posting")
                        throw new CustomException("The GRN no '" + receiveData.Id + "' already Posted!");

                    //var companyParty = _companyPartyRepository.Query(r => r.PartyId == receiveData.PartyId).Select().FirstOrDefault();

                    // var inventoryReceive=_inventoryReceiveRepository.Find();

                    _unitOfWork.BeginTransaction();
                    flag = true;
                    // INSERT INTO Invoice TABLE

                    var invoice = new Invoice
                    {
                        Amount = voucherVM.Amount,
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        CurrencyId = voucherVM.CurrencyId,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        InvoiceNo = voucherVM.InvoiceNo,
                        Narration = voucherVM.Narration,
                        EntityId = voucherVM.EntityId,
                        PlantId = voucherVM.PlantId,
                        IsExcludingTax = voucherVM.IsExcludingTax,
                        IsSplit = voucherVM.IsSplit,
                        PartyId = voucherVM.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        PartyType = voucherVM.PartyType,
                        EmployeeId = voucherVM.EmployeeId,
                        PaymentTermId = voucherVM.PaymentTermId,
                        PostingDate = receiveData.GRNDate,
                        SourceType = SourceType.InventoryPayable.ToString(),

                        VoucherTypeId = voucherVM.VoucherTypeId,
                        FiscalYearId = voucherVM.FiscalYearId,
                        FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                        TaxYearId = voucherVM.TaxYearId,
                        VoucherDate = DateTime.Now,
                        TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                        CompanyCurrencyRate = voucherVM.ToCurrencyRate
                    };

                    invoice.BaseNoOfDays = voucherVM.BaseNoOfDays;
                    invoice.BaseOnDueDate = voucherVM.BaseOnDueDate;
                    invoice.RevisedDueDate = voucherVM.MatureDate;
                    invoice.ActualDueDate = voucherVM.MatureDate;
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
                        EntityId = voucherVM.EntityId,
                        AddedBy = invoice.AddedBy,
                        AddedDate = invoice.AddedDate,
                        AddedFromIP = invoice.AddedFromIP,
                        VoucherDate = invoice.VoucherDate,
                        DocDate = invoice.DocDate,
                        DocRefNo = invoice.DocRefNo,
                        Archive = invoice.Archive,
                        IsPark = invoice.IsPark,
                        Narration = invoice.Narration,
                        PostingDate = receiveData.GRNDate,
                        SourceType = SourceType.InventoryPayable.ToString(),
                        VoucherTypeId = voucherVM.VoucherTypeId,
                    };
                    voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                    voucher.PostedBy = receiveData.AddedBy;
                    voucher.PostedFromIP = invoice.AddedFromIP;
                    voucher.PostedDate = invoice.AddedDate;
                    _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                    receiveData.VoucherId = voucher.Id;
                    receiveData.InvoiceNo = voucherVM.InvoiceNo;
                    receiveData.InvoiceDate = voucherVM.InvoiceDate;

                    receiveData.Status = "Posting";
                    receiveData.ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(receiveData);
                    invoice.InventoryReceiveId = receiveId;
                    _inventoryReceiveRepository.Update(receiveData);

                    var grnAccMap = new GRNAcceptanceMap
                    {
                        Id = "I" + _pkGeneratorService.GetAutoNumber(nameof(GRNAcceptanceMap), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        GRNId = receiveData.Id,
                        PurchaseDocumentAcceptanceId = null,
                        Qty = 0,
                        InvoiceId = invoice.Id
                    };
                    AuditService.AddedLog(grnAccMap);
                    _gRNAcceptanceMapRepository.Insert(grnAccMap);


                    // Set to Invoice
                    invoice.VoucherId = voucher.Id;

                    var currentInvoiceDetail = 0;
                    var currentVoucherDetaiRecord = 0;
                    var currentTaxRecord = 0;
                    decimal totalAmountDr = 0;
                    decimal totalAmountCr = 0;
                    var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                    foreach (var voucherDetailVM in voucherDetailVMList.Where(r => r.Amount > 0))
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                            throw new CustomException("Without Budget can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                            throw new CustomException("Without Activity can not post.");
                        if (voucherDetailVM.TrnType == "Dr")
                        {
                            // in libility side Dr.
                            var voucherDr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                DrAmount = voucherDetailVM.Amount,
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PostingWithoutTaxAllow = invoice.IsExcludingTax
                            };
                            totalAmountDr += voucherDr.DrAmount;
                            voucherDetailVM.Id = voucherDr.Id;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                            foreach (var item in inventoryReceiveDetailVMList.Where(r => r.GLGeneralInfoId == voucherDr.GLGeneralInfoId
                            && r.BudgetMasterId == voucherDr.BudgetMasterId && r.ActivityId == voucherDr.ActivityId))
                            {
                                var inventoryReceiveDetail = _inventoryReceiveDetailRepository.Find(item.InventoryReceiveDetailId);
                                var CrGLBAct = inventoryPayableVMList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                                inventoryReceiveDetail.PostDrGLGeneralInfoId = voucherDr.GLGeneralInfoId;
                                inventoryReceiveDetail.PostDrBudgetMasterId = voucherDr.BudgetMasterId;
                                inventoryReceiveDetail.PostDrActivityId = voucherDr.ActivityId;
                                inventoryReceiveDetail.IsAsset = voucherDetailVM.IsAsset;
                                inventoryReceiveDetail.PostCrGLGeneralInfoId = CrGLBAct.GLGeneralInfoId;
                                inventoryReceiveDetail.PostCrBudgetMasterId = CrGLBAct.BudgetMasterId;
                                inventoryReceiveDetail.PostCrActivityId = CrGLBAct.ActivityId;
                                inventoryReceiveDetail.ModelState = ModelState.Modified;
                                inventoryReceiveDetail.VoucherDetailId = voucherDr.Id;
                                AuditService.UpdatedLog(inventoryReceiveDetail);
                                _inventoryReceiveDetailRepository.Update(inventoryReceiveDetail);
                            }

                            if (voucherDetailVM.OtherName == "Tax" || voucherDetailVM.OtherName == "TCS")
                            {
                                //var voucherDetailDrId = voucherDetailVMList.FirstOrDefault(t => t.TrnType == "Dr" && t.OtherName == "Tax" && t.MaterialGroupMasterId == voucherDetailVM.MaterialGroupMasterId).Id;
                                currentTaxRecord++;
                                var invoiceTax = new InvoiceTax
                                {
                                    Archive = false,
                                    VoucherDetailId = voucherDr.Id,//voucherDetailDrId,
                                    VoucherId = voucher.Id,
                                    InvoiceId = invoice.Id,
                                    TaxYearId = voucher.TaxYearId,
                                    TaxYearPeriodId = voucher.TaxYearPeriodId,
                                    TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                    TaxCodeId = voucherDetailVM.TaxCodeId,
                                    TaxAmount = voucherDetailVM.Amount,
                                    TaxAutoAmount = 0,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.InventoryPayable.ToString(),
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
                                var inventoryreceivetax = _inventoryReceiveTaxRepository.Query(r=>r.InventoryReceiveId==receiveId && r.InventoryReceiveDetailId!=null && r.TaxCategoryId== voucherDetailVM.TaxCategoryId).Select().ToList();

                                foreach (var DrTax in inventoryreceivetax)
                                {
                                    DrTax.DrVoucherDetailId = voucherDr.Id;
                                    _inventoryReceiveTaxRepository.Update(DrTax);
                                }

                            }
                            #region Currency

                            foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                            {
                               
                                var voucherDetailCurrencydb = new VoucherDetailCurrency
                                {
                                    ToCurrencyRate = voucherVM.ToCurrencyRate,
                                    ToCurrencyId = companyCurrencyId,
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                    DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                    ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                                };
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                                voucherDetailCurrencydb = null;
                            }

                            #endregion Currency
                        }
                        else if (voucherDetailVM.TrnType == "Cr")
                        {
                            currentInvoiceDetail++;
                            // INSERT INTO InvoiceDetail
                            var invoiceDetail = new InvoiceDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                MaterialGroupMasterId = voucherDetailVM.MaterialGroupMasterId,
                                Amount = voucherDetailVM.Amount,
                                NetAmount = voucherDetailVM.Amount,
                                TaxAmount = 0,
                                AddedBy = invoice.AddedBy,
                                AddedDate = invoice.AddedDate,
                                AddedFromIP = invoice.AddedFromIP,
                                Archive = invoice.Archive,
                                InvoiceId = invoice.Id,
                            };
                            invoice.Amount = invoiceDetail.Amount;
                            // INSERT INTO VoucherDetail
                            var voucherCr = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceDetail.BudgetMasterId,
                                ActivityId = invoiceDetail.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                DrAmount = 0,
                                CrAmount = voucherDetailVM.Amount,
                                DocDate = voucher.DocDate,
                                DocRefNo = voucher.DocRefNo,
                                Narration = invoice.Narration,
                                EmployeeId = invoice.EmployeeId,
                                PostingWithoutTaxAllow = invoice.IsExcludingTax
                            };
                            voucherDetailVM.Id = voucherCr.Id;
                            totalAmountCr += voucherCr.CrAmount;
                            if (voucherDetailVM.OtherName == "Vendor")
                            {
                                // _invoiceDetailRepository.Insert(invoiceDetail);
                              _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail);
                                voucherCr.InvoiceDetailId = invoiceDetail.Id;
                                voucherCr.PartyId = invoice.PartyId;
                                voucherCr.PartyPlantId = invoice.PartyPlantId;
                                voucherCr.PartyType = invoice.PartyType;
                            }

                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);

                            if (voucherDetailVM.OtherName == "Tax" || voucherDetailVM.OtherName == "TCS")
                            {
                                var voucherDetailCrrId = voucherDetailVMList.FirstOrDefault(t => t.TrnType == "Cr" && t.OtherName == "Tax" && t.MaterialGroupMasterId == voucherDetailVM.MaterialGroupMasterId).Id;
                                currentTaxRecord++;
                                var invoiceTaxCr = new InvoiceTax
                                {
                                    Archive = false,
                                    VoucherDetailId = voucherCr.Id,//voucherDetailCrrId,
                                    InvoiceId = invoice.Id,
                                    TaxYearId = voucher.TaxYearId,
                                    TaxYearPeriodId = voucher.TaxYearPeriodId,
                                    TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                    TaxAmount = voucherDetailVM.Amount,
                                    TaxAutoAmount = 0,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.InventoryPayable.ToString(),
                                    AddedBy = voucher.AddedBy,
                                    AddedDate = voucher.AddedDate,
                                    AddedFromIP = voucher.AddedFromIP
                                };
                                _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTaxCr, invoiceTaxPk);
                                var invoiceTaxDetailCr = new InvoiceTaxDetail
                                {
                                    Id = invoiceTaxCr.Id + 1,
                                    InvoiceTaxId = invoiceTaxCr.Id,
                                    Amount = invoiceTaxCr.TaxAmount,
                                    GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                    BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                    ActivityId = voucherDetailVM.ActivityId,
                                    AType = "Cr",
                                    Archive = false,
                                    ModelState = ModelState.Added,
                                    AddedBy = voucher.AddedBy,
                                    AddedDate = voucher.AddedDate,
                                    AddedFromIP = voucher.AddedFromIP
                                };
                                _invoiceTaxDetailRepository.Insert(invoiceTaxDetailCr);
                                var inventoryreceivetax = _inventoryReceiveTaxRepository.Query(r => r.InventoryReceiveId == receiveId && r.InventoryReceiveDetailId != null && r.TaxCategoryId == voucherDetailVM.TaxCategoryId).Select().ToList();

                                foreach (var Crtax in inventoryreceivetax)
                                {
                                    Crtax.CrVoucherDetailId = voucherCr.Id;
                                    _inventoryReceiveTaxRepository.Update(Crtax);
                                }
                            }


                            #region Currency

                            foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                            {


                                var voucherDetailCurrencydb = new VoucherDetailCurrency
                                {
                                    ToCurrencyRate = voucherVM.ToCurrencyRate,
                                    ToCurrencyId = companyCurrencyId,
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                    CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                                    ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                                };
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);
                                voucherDetailCurrencydb = null;
                            }

                            #endregion Currency
                        }
                    }


                    if (totalAmountDr != totalAmountCr)
                        throw new CustomException("Dr and Cr amount is not equal.");
                    //if (null != additionalTaxList && additionalTaxList.Count() > 0)
                    //{

                    //    var additionalTax = new AdditionalTax
                    //    {

                    //        TaxYearId = voucher.TaxYearId,
                    //        TaxYearPeriodId = voucher.TaxYearPeriodId,
                    //        TaxAmount = additionalTaxList.Sum(r => r.TaxAmount),
                    //        TaxAutoAmount = additionalTaxList.Sum(r => r.TaxAutoAmount),
                    //        InventoryReceiveId = receiveId,
                    //        InvoiceId = invoice.Id,
                    //        Id = base.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
                    //        AddedBy = invoice.AddedBy,
                    //        AddedDate = invoice.AddedDate,
                    //        AddedFromIP = invoice.AddedFromIP
                    //    };
                    //    accountCommonExtensionService.InsertAdditionalTax(additionalTax, out  _adtaxdataset);

                    //    int addtionalTaxDetailId = 0;
                    //    foreach (var invoiceTaxVM in additionalTaxList)
                    //    {

                    //        if (null == invoiceTaxVM.TaxCodeId)
                    //            throw new CustomException("Tax code not found!");

                    //        var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == invoiceTaxVM.TaxCodeId).Select().FirstOrDefault();
                    //        if (null == taxCodeGL)
                    //            throw new CustomException("Tax code GL not found!");


                    //        addtionalTaxDetailId++;
                    //        var invoiceTaxDetail = new AdditionalTaxDetail
                    //        {
                    //            GLGeneralInfoId = taxCodeGL.WithholdCreditableGLId,
                    //            BudgetMasterId = taxCodeGL.WithholdCreditableBudgetMasterId,
                    //            ActivityId = taxCodeGL.WithholdCreditableActivityId,
                    //            Amount = invoiceTaxVM.TaxAmount,
                    //            AdditionalTaxId = additionalTax.Id,
                    //            TaxCodeId = invoiceTaxVM.TaxCodeId,
                    //            TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                    //            AType = "Cr",
                    //            Id = MakePK(additionalTax.Id, addtionalTaxDetailId, 3),
                    //            AddedBy = invoice.AddedBy,
                    //            AddedDate = invoice.AddedDate,
                    //            AddedFromIP = invoice.AddedFromIP
                    //        };
                    //        accountCommonExtensionService.InsertAdditionalTaxDetail(invoiceTaxDetail, ref _adtaxDetaildataset);
                    //    }

                    //}
                    if (null != additionalTaxList && additionalTaxList.Count() > 0)
                    {
                        var tdsTax = new AdditionalTax
                        {

                            TaxYearId = voucher.TaxYearId,
                            TaxYearPeriodId = voucher.TaxYearPeriodId,
                            TaxAmount = additionalTaxList.Sum(r => r.TaxAmount),
                            TaxAutoAmount = additionalTaxList.Sum(r => r.TaxAutoAmount),
                            InventoryReceiveId = receiveId,
                            PartyId = invoice.PartyId,
                            PartyPlantId = invoice.PartyPlantId,
                            InvoiceId = invoice.Id,
                            Id = _pkGeneratorService.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _additionalTaxRepository.Insert(tdsTax);

                        int addtionalTaxDetailId = 0;
                        foreach (var tdsTaxVM in additionalTaxList)
                        {

                            if (null == tdsTaxVM.TaxCodeId)
                                throw new CustomException("Tax code not found!");

                            var taxCodeGL = _accountsCommonService.GetTaxCodeGL(tdsTaxVM.TaxCodeId);
                           

                            addtionalTaxDetailId++;
                            var tdsTaxDetail = new AdditionalTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                                BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                                ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                                Amount = tdsTaxVM.TaxAmount,
                                AdditionalTaxId = tdsTax.Id,
                                TaxCodeId = tdsTaxVM.TaxCodeId,
                                TaxCategoryId = tdsTaxVM.TaxCategoryId,
                                AType = "Cr",
                                Id = _pkGeneratorService.MakePK(tdsTax.Id, addtionalTaxDetailId, 3),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _additionalTaxDetailRepository.Insert(tdsTaxDetail);


                        }
                    }

                    if(voucherVM.OtherPartyId != null)
                    {
                        InsertOtherVendorChargesPayable(receiveId,voucherVM, otherVendorChargesList);
                    }
                    _unitOfWork.SaveChanges();
                    //clsStaticInfo objApp = new clsStaticInfo();
                    //objApp.SaveDataSets(_adgrnAccMapset);
                    flag = false;
                    _unitOfWork.Commit();
                    voucherNo = voucher.VoucherNo;
                }
                else if (voucherVM.IsInvoice == false && receiveData.IsFOC == false)
                {
                    voucherNo = InsertInventoryPayableWithOutInvoice(receiveId, acceptanceId, voucherVM, voucherDetailVMList
                 , voucherDetailCurrencyVMList, inventoryPayableVMList, inventoryReceiveDetailVMList, additionalTaxList);
                }
                else if (voucherVM.IsInvoice == true && receiveData.IsFOC == true)
                {
                    voucherNo = InsertFOCInventoryPayable(receiveId, acceptanceId, voucherVM, voucherDetailVMList
                 , voucherDetailCurrencyVMList, inventoryPayableVMList, inventoryReceiveDetailVMList, additionalTaxList);
                }

                return voucherNo;
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
        private string InsertInventoryPayableWithOutInvoice(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
            , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<InvoiceTaxViewModel> additionalTaxList)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                var receiveData = _inventoryReceiveRepository.Find(receiveId);
                voucherVM.PostingDate = receiveData.GRNDate;
                if (receiveData.Status == "Posting")
                    throw new CustomException("The GRN no '" + receiveData.Id + "' already Posted!");

                //var companyParty = _companyPartyRepository.Query(r => r.PartyId == receiveData.PartyId).Select().FirstOrDefault();
                _unitOfWork.BeginTransaction();
                flag = true;
                // var inventoryReceive=_inventoryReceiveRepository.Find();
                #endregion Get Company Parallerl Currency Id



                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = voucherVM.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    AddedBy = voucherVM.AddedBy,
                    AddedDate = voucherVM.AddedDate,
                    AddedFromIP = voucherVM.AddedFromIP,
                    VoucherDate = voucherVM.VoucherDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Archive = false,
                    IsPark = voucherVM.IsPark,
                    Narration = voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.InventoryPayable.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };
                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);
                voucherVM.AddedBy = voucher.AddedBy;
                voucherVM.AddedDate = voucher.AddedDate;
                voucherVM.AddedFromIP = voucher.AddedFromIP;

                receiveData.Status = "Posting";
                receiveData.VoucherId = voucher.Id;
                receiveData.IsInvoice = voucherVM.IsInvoice;
                receiveData.ModelState = ModelState.Modified;
                AuditService.UpdatedLog(receiveData);
                _inventoryReceiveRepository.Update(receiveData);
                //For check Budget is applied in company or not.
                //var comdata = _companyService.Find(voucher.CompanyId);
                // Set to Invoice
                var currentVoucherDetaiRecord = 0;
                decimal totalAmountDr = 0;
                decimal totalAmountCr = 0;
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();


                foreach (var voucherDetailVM in voucherDetailVMList.Where(r => r.Amount > 0))
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                        throw new CustomException("Without Budget can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                        throw new CustomException("Without Activity can not post.");
                    if (voucherDetailVM.TrnType == "Dr")
                    {
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                        };
                        totalAmountDr += voucherDr.DrAmount;
                        voucherDetailVM.Id = voucherDr.Id;
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                        foreach (var item in inventoryReceiveDetailVMList.Where(r => r.GLGeneralInfoId == voucherDr.GLGeneralInfoId
                        && r.BudgetMasterId == voucherDr.BudgetMasterId && r.ActivityId == voucherDr.ActivityId))
                        {
                            var inventoryReceiveDetail = _inventoryReceiveDetailRepository.Find(item.InventoryReceiveDetailId);
                            var CrGLBAct = inventoryPayableVMList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                            inventoryReceiveDetail.PostDrGLGeneralInfoId = voucherDr.GLGeneralInfoId;
                            inventoryReceiveDetail.PostDrBudgetMasterId = voucherDr.BudgetMasterId;
                            inventoryReceiveDetail.PostDrActivityId = voucherDr.ActivityId;
                            inventoryReceiveDetail.IsAsset = voucherDetailVM.IsAsset;
                            inventoryReceiveDetail.PostCrGLGeneralInfoId = CrGLBAct.GLGeneralInfoId;
                            inventoryReceiveDetail.PostCrBudgetMasterId = CrGLBAct.BudgetMasterId;
                            inventoryReceiveDetail.PostCrActivityId = CrGLBAct.ActivityId;
                            inventoryReceiveDetail.ModelState = ModelState.Modified;
                            inventoryReceiveDetail.VoucherDetailId = voucherDr.Id;
                            AuditService.UpdatedLog(inventoryReceiveDetail);
                            _inventoryReceiveDetailRepository.Update(inventoryReceiveDetail);
                        }

                        if (voucherDetailVM.OtherName == "Tax" || voucherDetailVM.OtherName == "TCS")
                        {
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                VoucherDetailId = voucherDr.Id,//voucherDetailDrId,
                                VoucherId = voucher.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.InventoryPayable.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxService.InsertInvoiceTax(voucherVM, invoiceTax, invoiceTaxPk);
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
                            var inventoryreceivetax = _inventoryReceiveTaxRepository.Query(r => r.InventoryReceiveId == receiveId && r.InventoryReceiveDetailId != null && r.TaxCategoryId == voucherDetailVM.TaxCategoryId).Select().ToList();

                            foreach (var DrTax in inventoryreceivetax)
                            {
                                DrTax.DrVoucherDetailId = voucherDr.Id;
                                _inventoryReceiveTaxRepository.Update(DrTax);
                            }
                        }
                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                            
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                    else if (voucherDetailVM.TrnType == "Cr")
                    {
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = voucherVM.Narration,
                            //EmployeeId = voucherVM.EmployeeId,
                            //PartyId = voucherVM.PartyId,
                            //PartyPlantId = voucherVM.PartyPlantId,
                            PartyType = voucherVM.PartyType,
                        };
                        voucherDetailVM.Id = voucherCr.Id;
                        totalAmountCr += voucherCr.CrAmount;
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);

                        if (voucherDetailVM.OtherName == "Tax" || voucherDetailVM.OtherName == "TCS")
                        {
                            var invoiceTaxCr = new InvoiceTax
                            {
                                Archive = false,
                                VoucherDetailId = voucherCr.Id,//voucherDetailCrrId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.InventoryPayable.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxService.InsertInvoiceTax(voucherVM, invoiceTaxCr, invoiceTaxPk);
                            var invoiceTaxDetailCr = new InvoiceTaxDetail
                            {
                                Id = invoiceTaxCr.Id + 1,
                                InvoiceTaxId = invoiceTaxCr.Id,
                                Amount = invoiceTaxCr.TaxAmount,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                AType = "Cr",
                                Archive = false,
                                ModelState = ModelState.Added,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxDetailRepository.Insert(invoiceTaxDetailCr);
                            var inventoryreceivetax = _inventoryReceiveTaxRepository.Query(r => r.InventoryReceiveId == receiveId && r.InventoryReceiveDetailId != null && r.TaxCategoryId == voucherDetailVM.TaxCategoryId).Select().ToList();

                            foreach (var CrTax in inventoryreceivetax)
                            {
                                CrTax.DrVoucherDetailId = voucherCr.Id;
                                _inventoryReceiveTaxRepository.Update(CrTax);
                            }
                        }

                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                          
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                if (null != additionalTaxList && additionalTaxList.Count() > 0)
                {
                    var invoiceTax = new AdditionalTax
                    {

                        TaxYearId = voucher.TaxYearId,
                        TaxYearPeriodId = voucher.TaxYearPeriodId,
                        TaxAmount = additionalTaxList.Sum(r => r.TaxAmount),
                        TaxAutoAmount = additionalTaxList.Sum(r => r.TaxAutoAmount),
                        InventoryReceiveId = receiveId,
                        PartyId = voucherVM.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        Id = _pkGeneratorService.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP
                    };
                    _additionalTaxRepository.Insert(invoiceTax);

                    int addtionalTaxDetailId = 0;
                    foreach (var invoiceTaxVM in additionalTaxList)
                    {

                        if (null == invoiceTaxVM.TaxCodeId)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _accountsCommonService.GetTaxCodeGL(invoiceTaxVM.TaxCodeId);
                        if (null == taxCodeGL)
                            throw new CustomException("Tax code GL not found!");


                        addtionalTaxDetailId++;
                        var invoiceTaxDetail = new AdditionalTaxDetail
                        {
                            GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                            BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                            ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                            Amount = invoiceTaxVM.TaxAmount,
                            AdditionalTaxId = invoiceTax.Id,
                            TaxCodeId = invoiceTaxVM.TaxCodeId,
                            TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                            AType = "Cr",
                            Id =_pkGeneratorService.MakePK(invoiceTax.Id, addtionalTaxDetailId, 3),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _additionalTaxDetailRepository.Insert(invoiceTaxDetail);


                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
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

        private string InsertFOCInventoryPayable(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
           , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<InvoiceTaxViewModel> additionalTaxList)
        {
            var flag = false;
            try
            {


                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                var receiveData = _inventoryReceiveRepository.Find(receiveId);
                voucherVM.PostingDate = receiveData.GRNDate; //invReceive["GRNDate"].ToString();
               
                if (receiveData.Status == "Posting")
                    throw new CustomException("The GRN no '" + receiveId + "' already Posted!");

                AccountCommonExtensionService accountCommonExtensionService = new AccountCommonExtensionService();

                //var companyParty = _companyPartyRepository.Query(r => r.PartyId == receiveData.PartyId).Select().FirstOrDefault();
                _unitOfWork.BeginTransaction();

                var invoice = new Invoice
                {
                    Amount = voucherVM.Amount,
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    CurrencyId = voucherVM.CurrencyId,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    InvoiceNo = voucherVM.InvoiceNo,
                    Narration = voucherVM.Narration,
                    EntityId = voucherVM.EntityId,
                    PlantId = voucherVM.PlantId,
                    IsExcludingTax = voucherVM.IsExcludingTax,
                    IsSplit = voucherVM.IsSplit,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = PartyType.Vendor.ToString(),
                    EmployeeId = voucherVM.EmployeeId,
                    PaymentTermId = voucherVM.PaymentTermId,
                    PostingDate = receiveData.GRNDate,
                    SourceType = SourceType.InventoryPayable.ToString(),

                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    CompanyCurrencyRate = voucherVM.ToCurrencyRate
                };
                if (acceptanceId != null)
                {
                    invoice.BaseNoOfDays = 0;
                    invoice.BaseOnDueDate = null;
                    invoice.RevisedDueDate = null;
                    invoice.ActualDueDate = null;
                }
                else
                {
                    invoice.BaseNoOfDays = voucherVM.BaseNoOfDays;
                    invoice.BaseOnDueDate = voucherVM.BaseOnDueDate;
                    invoice.RevisedDueDate = voucherVM.MatureDate;
                    invoice.ActualDueDate = voucherVM.MatureDate;
                }
                _invoiceService.InsertInvoice(invoice);


                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = invoice.CompanyGroupId,
                    CompanyId = invoice.CompanyId,
                    PlantId = invoice.PlantId,
                    EntityId = voucherVM.EntityId,
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
                    Narration = invoice.Narration,
                    PostingDate = receiveData.GRNDate,
                    SourceType = SourceType.InventoryPayable.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };
                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                receiveData.VoucherId = voucher.Id;
                receiveData.Status = "Posting";
                receiveData.ModelState = ModelState.Modified;
                AuditService.UpdatedLog(receiveData);
                invoice.InventoryReceiveId = receiveId;
                _inventoryReceiveRepository.Update(receiveData);


                //invReceive.BeginEdit();
                //invReceive["VoucherId"] = voucher.Id;
                //invReceive["Status"] = "Posting";
                //invReceive["ModelState"] = ModelState.Modified;
                //invReceive["UpdatedBy"] = voucher.AddedBy;
                //invReceive["UpdatedDate"] = voucher.AddedDate;
                //invReceive["UpdatedFromIP"] = voucher.AddedFromIP;
                //invReceive.EndEdit();


                //var grnAccMap = new GRNAcceptanceMap
                //{
                //    Id = "I" + base.GetMaxNumber(nameof(GRNAcceptanceMap), PKGeneratorEnum.Yearly, null, DateTime.Now),
                //    GRNId = receiveId,
                //    PurchaseDocumentAcceptanceId = null,
                //    //PurchaseDocumentAcceptanceDetailId = null,
                //    Qty = 0,
                //    InvoiceId = invoice.Id
                //};
                //accountCommonExtensionService.InsertGRNAcceptanceMap(grnAccMap, out DataSet _adgrnAccMapset);
                var grnAccMap = new GRNAcceptanceMap
                {
                    Id = "I" + _pkGeneratorService.GetAutoNumber(nameof(GRNAcceptanceMap), PKGeneratorEnum.Yearly, null, DateTime.Now),
                    GRNId = receiveData.Id,
                    PurchaseDocumentAcceptanceId = null,
                    //PurchaseDocumentAcceptanceDetailId = null,
                    Qty = 0,
                    InvoiceId = invoice.Id
                };
                AuditService.AddedLog(grnAccMap);
                _gRNAcceptanceMapRepository.Insert(grnAccMap);


                invoice.VoucherId = voucher.Id;

                var currentInvoiceDetail = 0;
                var currentVoucherDetaiRecord = 0;
                var currentTaxRecord = 0;
                decimal totalAmountDr = 0;
                decimal totalAmountCr = 0;
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                        throw new CustomException("Without Budget can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                        throw new CustomException("Without Activity can not post.");
                    if (voucherDetailVM.TrnType == "Dr")
                    {
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax
                        };
                        totalAmountDr += voucherDr.DrAmount;
                        voucherDetailVM.Id = voucherDr.Id;
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                        foreach (var item in inventoryReceiveDetailVMList.Where(r => r.GLGeneralInfoId == voucherDr.GLGeneralInfoId
                        && r.BudgetMasterId == voucherDr.BudgetMasterId && r.ActivityId == voucherDr.ActivityId))
                        {
                            var inventoryReceiveDetail = _inventoryReceiveDetailRepository.Find(item.InventoryReceiveDetailId);
                            var CrGLBAct = inventoryPayableVMList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                            inventoryReceiveDetail.PostDrGLGeneralInfoId = voucherDr.GLGeneralInfoId;
                            inventoryReceiveDetail.PostDrBudgetMasterId = voucherDr.BudgetMasterId;
                            inventoryReceiveDetail.PostDrActivityId = voucherDr.ActivityId;
                            inventoryReceiveDetail.IsAsset = voucherDetailVM.IsAsset;
                            inventoryReceiveDetail.PostCrGLGeneralInfoId = CrGLBAct.GLGeneralInfoId;
                            inventoryReceiveDetail.PostCrBudgetMasterId = CrGLBAct.BudgetMasterId;
                            inventoryReceiveDetail.PostCrActivityId = CrGLBAct.ActivityId;
                            inventoryReceiveDetail.ModelState = ModelState.Modified;
                            inventoryReceiveDetail.VoucherDetailId = voucherDr.Id;
                            AuditService.UpdatedLog(inventoryReceiveDetail);
                            _inventoryReceiveDetailRepository.Update(inventoryReceiveDetail);
                        }

                        if (voucherDetailVM.OtherName == "Tax")
                        {
                            var voucherDetailDrId = voucherDetailVMList.FirstOrDefault(t => t.TrnType == "Dr" && t.OtherName == "Tax" && t.MaterialGroupMasterId == voucherDetailVM.MaterialGroupMasterId).Id;
                            currentTaxRecord++;
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                VoucherDetailId = voucherDr.Id,//voucherDetailDrId,
                                VoucherId = voucher.Id,
                                InvoiceId = invoice.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.InventoryPayable.ToString(),
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
                            var inventoryreceivetax = _inventoryReceiveTaxRepository.Query(r => r.InventoryReceiveId == receiveId && r.InventoryReceiveDetailId != null && r.TaxCategoryId == voucherDetailVM.TaxCategoryId).Select().ToList();

                            foreach (var DrTax in inventoryreceivetax)
                            {
                                DrTax.DrVoucherDetailId = voucherDr.Id;
                                _inventoryReceiveTaxRepository.Update(DrTax);
                            }
                        }
                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                           
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                    else if (voucherDetailVM.TrnType == "Cr")
                    {
                        currentInvoiceDetail++;
                        // INSERT INTO InvoiceDetail
                        var invoiceDetail = new InvoiceDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            MaterialGroupMasterId = voucherDetailVM.MaterialGroupMasterId,
                            Amount = voucherDetailVM.Amount,
                            NetAmount = voucherDetailVM.Amount,
                            TaxAmount = 0,
                            AddedBy = invoice.AddedBy,
                            AddedDate = invoice.AddedDate,
                            AddedFromIP = invoice.AddedFromIP,
                            Archive = invoice.Archive,
                            InvoiceId = invoice.Id,
                        };
                        invoice.Amount = invoiceDetail.Amount;
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceDetail.BudgetMasterId,
                            ActivityId = invoiceDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = invoice.Narration,
                            EmployeeId = invoice.EmployeeId,
                            InvoiceDetailId = invoiceDetail.Id,
                            PartyId = invoice.PartyId,
                            PartyPlantId = invoice.PartyPlantId,
                            PartyType = invoice.PartyType,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax
                        };
                        voucherDetailVM.Id = voucherCr.Id;
                        totalAmountCr += voucherCr.CrAmount;

                        if (voucherDetailVM.OtherName == "Tax")
                        {
                            var voucherDetailCrrId = voucherDetailVMList.FirstOrDefault(t => t.TrnType == "Cr" && t.OtherName == "Tax" && t.MaterialGroupMasterId == voucherDetailVM.MaterialGroupMasterId).Id;
                            currentTaxRecord++;
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                VoucherDetailId = voucherCr.Id,//voucherDetailCrrId,
                                InvoiceId = invoice.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.InventoryPayable.ToString(),
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
                                Archive = false,
                                ModelState = ModelState.Added,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                            var inventoryreceivetax = _inventoryReceiveTaxRepository.Query(r => r.InventoryReceiveId == receiveId && r.InventoryReceiveDetailId != null && r.TaxCategoryId == voucherDetailVM.TaxCategoryId).Select().ToList();

                            foreach (var Crtax in inventoryreceivetax)
                            {
                                Crtax.CrVoucherDetailId = voucherCr.Id;
                                _inventoryReceiveTaxRepository.Update(Crtax);
                            }
                        }

                        //_invoiceDetailRepository.Insert(invoiceDetail);
                      _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail);
                        voucherCr.InvoiceDetailId = invoiceDetail.Id;
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);

                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                           
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                }
                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                if (null != additionalTaxList && additionalTaxList.Count() > 0)
                {
                    var invoiceTax = new AdditionalTax
                    {

                        TaxYearId = voucher.TaxYearId,
                        TaxYearPeriodId = voucher.TaxYearPeriodId,
                        TaxAmount = additionalTaxList.Sum(r => r.TaxAmount),
                        TaxAutoAmount = additionalTaxList.Sum(r => r.TaxAutoAmount),
                        InventoryReceiveId = receiveId,
                        Id = _pkGeneratorService.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP
                    };
                    _additionalTaxRepository.Insert(invoiceTax);

                    int addtionalTaxDetailId = 0;
                    foreach (var invoiceTaxVM in additionalTaxList)
                    {

                        if (null == invoiceTaxVM.TaxCodeId)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _accountsCommonService.GetTaxCodeGL(invoiceTaxVM.TaxCodeId);
                       

                        addtionalTaxDetailId++;
                        var invoiceTaxDetail = new AdditionalTaxDetail
                        {
                            GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                            BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                            ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                            Amount = invoiceTaxVM.TaxAmount,
                            AdditionalTaxId = invoiceTax.Id,
                            TaxCodeId = invoiceTaxVM.TaxCodeId,
                            TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                            AType = "Cr",
                            Id =_pkGeneratorService.MakePK(invoiceTax.Id, addtionalTaxDetailId, 3),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _additionalTaxDetailRepository.Insert(invoiceTaxDetail);


                    }
                }
                _unitOfWork.SaveChanges();
                //clsStaticInfo objApp = new clsStaticInfo();
                //objApp.SaveDataSets(_adgrnAccMapset);
                flag = false;
                _unitOfWork.Commit();

                return voucher.VoucherNo;
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

        private void InsertOtherVendorChargesPayable(string receiveId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            try
            {
                #region Get Company Parallerl Currency Id
                
                    var receiveData = _inventoryReceiveRepository.Find(receiveId);
                    voucherVM.PostingDate = receiveData.GRNDate;
                    AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                    _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                    _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                    _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                    #endregion Get Company Parallerl Currency Id

                    voucherVM.CompanyCurrencyRate = voucherVM.ToCurrencyRate;

                    var invoice = new Invoice
                    {
                        Amount = voucherDetailVMList.Where(r=>r.OtherName == "Vendor").Sum(r => r.Amount),
                        BaseNoOfDays = voucherVM.BaseNoOfDays,
                        BaseOnDueDate = voucherVM.BaseOnDueDate,
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        CurrencyId = voucherVM.CurrencyId,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = receiveData.OtherPartyDocRefNo,
                        InvoiceNo = receiveData.OtherPartyDocRefNo,
                        Narration = voucherVM.Narration,
                        EntityId = voucherVM.EntityId,
                        PlantId = voucherVM.PlantId,
                        IsExcludingTax = voucherVM.IsExcludingTax,
                        IsSplit = voucherVM.IsSplit,
                        PartyId = voucherVM.OtherPartyId,
                        PartyPlantId = voucherVM.OtherPartyPlantId,
                        PartyType = PartyType.Vendor.ToString(),
                        EmployeeId = voucherVM.EmployeeId,
                        PaymentTermId = voucherVM.PaymentTermId,
                        PostingDate = receiveData.GRNDate,
                        SourceType = SourceType.InventoryPayable.ToString(),
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


                    // INSERT INTO Voucher TABLE
                    var voucherOtherCharges = new Voucher
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
                        Narration = invoice.Narration,
                        PostingDate = receiveData.GRNDate,
                        SourceType = SourceType.InventoryPayable.ToString(),
                        VoucherTypeId = voucherVM.VoucherTypeId,
                    };
                    voucherOtherCharges.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucherOtherCharges.Id;
                    _voucherService.InsertVoucher(voucherOtherCharges, voucherVM.FiscalYearPrefix);
                    
                    invoice.InventoryReceiveId = receiveId;
                 
                    invoice.VoucherId = voucherOtherCharges.Id;

                    var currentInvoiceDetail = 0;
                    var currentVoucherDetaiRecord = 0;
                    var currentTaxRecord = 0;
                    decimal totalAmountDr = 0;
                    decimal totalAmountCr = 0;
                    var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();

                    foreach (var voucherDetailVM in voucherDetailVMList.Where(r => r.Amount > 0))
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                            throw new CustomException("Without Budget can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                            throw new CustomException("Without Activity can not post.");
                        if (voucherDetailVM.TrnType == "Dr")
                        {
                            // in libility side Dr.
                            var voucherDr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                DrAmount = voucherDetailVM.Amount,
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PostingWithoutTaxAllow = invoice.IsExcludingTax
                            };
                            totalAmountDr += voucherDr.DrAmount;
                            voucherDetailVM.Id = voucherDr.Id;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucherOtherCharges, voucherDr, currentVoucherDetaiRecord);

                         

                            if (voucherDetailVM.OtherName == "Tax")
                            {
                                currentTaxRecord++;
                                var invoiceTax = new InvoiceTax
                                {
                                    Archive = false,
                                    VoucherDetailId = voucherDr.Id,//voucherDetailDrId,
                                    VoucherId = voucherOtherCharges.Id,
                                    InvoiceId = invoice.Id,
                                    TaxYearId = voucherOtherCharges.TaxYearId,
                                    TaxYearPeriodId = voucherOtherCharges.TaxYearPeriodId,
                                    TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                    TaxCodeId = voucherDetailVM.TaxCodeId,
                                    TaxAmount = voucherDetailVM.Amount,
                                    TaxAutoAmount = 0,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.InventoryPayable.ToString(),
                                    AddedBy = voucherOtherCharges.AddedBy,
                                    AddedDate = voucherOtherCharges.AddedDate,
                                    AddedFromIP = voucherOtherCharges.AddedFromIP
                                };
                                _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk);
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    Id = invoiceTax.Id + 1,
                                    InvoiceTaxId = invoiceTax.Id,
                                    Amount = invoiceTax.TaxAmount,
                                    GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                    BudgetMasterId = voucherDetailVM.BudgetId,
                                    ActivityId = voucherDetailVM.ActivityId,
                                    AType = "Dr",
                                    AddedBy = invoiceTax.AddedBy,
                                    AddedDate = invoiceTax.AddedDate,
                                    AddedFromIP = invoiceTax.AddedFromIP
                                };
                                _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                            var inventoryreceivetax = _inventoryReceiveTaxRepository.Query(r => r.InventoryReceiveId == receiveId && r.InventoryReceiveDetailId != null && r.TaxCategoryId == voucherDetailVM.TaxCategoryId).Select().ToList();

                            foreach (var DrTax in inventoryreceivetax)
                            {
                                DrTax.DrVoucherDetailId = voucherDr.Id;
                                _inventoryReceiveTaxRepository.Update(DrTax);
                            }
                        }
                            #region Currency

                            
                                var voucherDetailCurrencydb = new VoucherDetailCurrency
                                {
                                    ToCurrencyRate = voucherVM.ToCurrencyRate,
                                    ToCurrencyId = companyCurrencyId,
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDr.CurrencyId,
                                    DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                    ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                                };
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                                voucherDetailCurrencydb = null;

                            #endregion Currency
                        }
                        else if (voucherDetailVM.TrnType == "Cr")
                        {
                            currentInvoiceDetail++;
                            // INSERT INTO InvoiceDetail
                            var invoiceDetail = new InvoiceDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                Amount = voucherDetailVM.Amount,
                                NetAmount = voucherDetailVM.Amount,
                                TaxAmount = 0,
                                AddedBy = invoice.AddedBy,
                                AddedDate = invoice.AddedDate,
                                AddedFromIP = invoice.AddedFromIP,
                                Archive = invoice.Archive,
                                InvoiceId = invoice.Id,
                            };
                            invoice.Amount = invoiceDetail.Amount;
                            // INSERT INTO VoucherDetail
                            var voucherCr = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceDetail.BudgetMasterId,
                                ActivityId = invoiceDetail.ActivityId,
                                CurrencyId = voucherOtherCharges.CurrencyId,
                                DrAmount = 0,
                                CrAmount = voucherDetailVM.Amount,
                                DocDate = voucherOtherCharges.DocDate,
                                DocRefNo = voucherOtherCharges.DocRefNo,
                                Narration = invoice.Narration,
                                EmployeeId = invoice.EmployeeId,
                                InvoiceDetailId = invoiceDetail.Id,
                                PartyId = invoice.PartyId,
                                PartyPlantId = invoice.PartyPlantId,
                                PartyType = invoice.PartyType,
                                PostingWithoutTaxAllow = invoice.IsExcludingTax
                            };
                            voucherDetailVM.Id = voucherCr.Id;
                            totalAmountCr += voucherCr.CrAmount;

                            if (voucherDetailVM.OtherName == "Tax")
                            {
                                currentTaxRecord++;
                                var invoiceTax = new InvoiceTax
                                {
                                    Archive = false,
                                    VoucherDetailId = voucherCr.Id,//voucherDetailCrrId,
                                    InvoiceId = invoice.Id,
                                    TaxYearId = voucherOtherCharges.TaxYearId,
                                    TaxYearPeriodId = voucherOtherCharges.TaxYearPeriodId,
                                    TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                    TaxAmount = voucherDetailVM.Amount,
                                    TaxAutoAmount = 0,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.InventoryPayable.ToString(),
                                    AddedBy = voucherOtherCharges.AddedBy,
                                    AddedDate = voucherOtherCharges.AddedDate,
                                    AddedFromIP = voucherOtherCharges.AddedFromIP
                                };
                                _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk);
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    Id = invoiceTax.Id + 1,
                                    InvoiceTaxId = invoiceTax.Id,
                                    Amount = invoiceTax.TaxAmount,
                                    GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                    BudgetMasterId = voucherDetailVM.BudgetId,
                                    ActivityId = voucherDetailVM.ActivityId,
                                    AType = "Cr",
                                    Archive = false,
                                    ModelState = ModelState.Added,
                                    AddedBy = voucherOtherCharges.AddedBy,
                                    AddedDate = voucherOtherCharges.AddedDate,
                                    AddedFromIP = voucherOtherCharges.AddedFromIP
                                };
                                _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                            var inventoryreceivetax = _inventoryReceiveTaxRepository.Query(r => r.InventoryReceiveId == receiveId && r.InventoryReceiveDetailId != null && r.TaxCategoryId == voucherDetailVM.TaxCategoryId).Select().ToList();

                            foreach (var Crtax in inventoryreceivetax)
                            {
                                Crtax.CrVoucherDetailId = voucherCr.Id;
                                _inventoryReceiveTaxRepository.Update(Crtax);
                            }
                        }

                            _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail);
                            voucherCr.InvoiceDetailId = invoiceDetail.Id;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucherOtherCharges, voucherCr, currentVoucherDetaiRecord);
         

                            #region Currency


                                var voucherDetailCRdb = new VoucherDetailCurrency
                                {
                                    ToCurrencyRate = voucherVM.ToCurrencyRate,
                                    ToCurrencyId = companyCurrencyId,
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherOtherCharges.CurrencyId,
                                    CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                                    ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                                };
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCRdb);
                                voucherDetailCRdb = null;

                            #endregion Currency
                        }
                    }

                    // Update Inventory Received


                    if (totalAmountDr != totalAmountCr)
                        throw new CustomException("Dr and Cr amount is not equal.");

               
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public string InsertAdditionalTaxPayable(VoucherViewModel voucherVM, string additionalTaxId)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                var additionalTax = _additionalTaxRepository.Find(additionalTaxId);
                if (additionalTax.VoucherId != null)
                    throw new CustomException("Additional Tax already posted.");
                var voucherDetailVMList = _invoiceService.QueryInvoiceDetail(additionalTax.InvoiceId).Select().ToList();
                voucherVM.PartyId = additionalTax.PartyId;
                voucherVM.PartyPlantId = additionalTax.PartyPlantId;
                voucherVM.IsPark = false;
                voucherVM.Amount = additionalTax.TaxAmount;
                voucherVM.Narration = "TDS Of" + voucherVM.PartyName;
                var invoiceWriteOff = _invoiceWriteOffService.InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher

                var voucher = _voucherService.InsertVoucher(voucherVM);
                AuditService.PostedLog(voucher);
                additionalTax.VoucherId = voucher.Id;
                AuditService.UpdatedLog(additionalTax);
                _additionalTaxRepository.Update(additionalTax);
                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                decimal taxDrAmount = 0;
                var withholdgl = false;

                var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                var inviceDbList =_invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                var invoiceDetailIds = voucherDetailVMList.Select(r => r.Id);
                var inviceDetailDbList = _invoiceService.QueryInvoiceDetailEnumerable(invoiceDetailIds);
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.Id);
                    if (null == invoiceDetail)
                        throw new CustomException("Invoice not found!");

                    invoiceDetail.WrittenOffAmount += additionalTax.TaxAmount;

                    if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                        throw new CustomException("Received amount can not cross balance amount.");

                    invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                    invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                    // TODO: have a gap here if invoice split
                    var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                    invoice.WrittenOffAmount += additionalTax.TaxAmount;
                    invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                    invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.Update(invoice);

                    // INSERT INTO InvoiceDetail
                    currentInvoiceWriteOffDetailId++;
                    var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherVM.InvoicingPartyPlantId,
                        ActivityId = voucherDetailVM.ActivityId,
                        CurrencyId = voucherVM.CurrencyId,
                        InvoiceWriteOffId = invoiceWriteOff.Id,
                        InvoiceId = voucherDetailVM.InvoiceId,
                        InvoiceDetailId = voucherDetailVM.Id,
                        Amount = additionalTax.TaxAmount,
                        AddedBy = invoiceWriteOff.AddedBy,
                        AddedDate = invoiceWriteOff.AddedDate,
                        AddedFromIP = invoiceWriteOff.AddedFromIP,
                        Archive = invoiceWriteOff.Archive,
                        ModelState = invoiceWriteOff.ModelState,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration
                    };
                    _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);

                    var voucherDetailDr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        EntityId = voucherVM.EntityId,
                        DrAmount = additionalTax.TaxAmount,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        PartyType = invoiceWriteOff.PartyType,
                        InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.ToCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.ToCurrencyRate),
                        DrAmount = voucherVM.ToCurrencyRate * voucherDetailDr.DrAmount
                    });

                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalCurrencyAmountDr += voucherVM.ToCurrencyRate * voucherDetailDr.DrAmount;

                }

                var taxDetailVMList = _additionalTaxDetailRepository.Query(r => r.AdditionalTaxId == additionalTaxId).Select().ToList();
                if (voucherVM.PaymentSource == PaymentSource.Tax.ToString())
                {
                    if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                    {
                        var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                        foreach (var invoiceTaxVM in taxDetailVMList)
                        {
                            var invoiceTax = new InvoiceTax
                            {
                                VoucherDetailId = voucherDetailTempId,
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                TaxAmount = invoiceTaxVM.Amount,
                                TaxAutoAmount = 0
                            };
                            totalAmountCr += invoiceTaxVM.Amount;
                            _invoiceTaxService.InsertInvoiceTax(invoiceWriteOff, invoiceTax, invoiceTaxPk);

                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = invoiceTaxVM.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxVM.BudgetMasterId,
                                ActivityId = invoiceTaxVM.ActivityId,
                                Amount = invoiceTax.TaxAmount,
                                AType = "Cr"
                            };
                            _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                            var voucherDetailTax = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                ActivityId = invoiceTaxDetail.ActivityId,
                                InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                CrAmount = invoiceTaxDetail.Amount,
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                            var voucherDetailCurrencyTax = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = companyCurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherDetailTax.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                            totalCurrencyAmountCr += voucherVM.ToCurrencyRate * voucherDetailTax.CrAmount;
                        }
                    }
                }

                totalCurrencyAmountCr = totalCurrencyAmountDr;
                totalAmountCr += taxDrAmount;
                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                //if (totalCurrencyAmountCr != totalCurrencyAmountDr)
                //    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
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
        public string InsertShortageDebitNote(VoucherViewModel voucherVM, string grnId, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
              
                voucherVM.PartyId = voucherVM.PartyId;
                voucherVM.PartyPlantId = voucherVM.PartyPlantId;
                voucherVM.IsPark = false;
                voucherVM.Amount = voucherDetailVMList.Sum(r=>r.DrAmount);
                voucherVM.Narration = "Shortage DebitNote" + voucherVM.PartyName;

                // INSERT INTO Voucher
                voucherVM.NoteType = NoteType.VendorDebitNote.ToString();
                var adjustmentNote =_adjustmentNoteService.InsertAdjustmentNote(voucherVM);
                adjustmentNote.InventoryReceiveId = grnId;
                var voucher = _voucherService.InsertVoucher(voucherVM);
                AuditService.PostedLog(voucher);

                // Set Voucher Id to Advance
                adjustmentNote.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                decimal taxDrAmount = 0;
                var withholdgl = false;

              
                foreach (var voucherDetailVM in voucherDetailVMList)
                {

                    // INSERT INTO InvoiceDetail
                    if (voucherDetailVM.TrnType == "Dr")
                    {
                        currentInvoiceWriteOffDetailId++;
                        var adjustmentNoteDetail = new AdjustmentNoteDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            AdjustmentNoteId = adjustmentNote.Id,
                            InvoiceId = voucherDetailVM.InvoiceId,
                            InvoiceDetailId = voucherDetailVM.Id,
                            Amount = voucherDetailVM.DrAmount,
                            AddedBy = adjustmentNote.AddedBy,
                            AddedDate = adjustmentNote.AddedDate,
                            AddedFromIP = adjustmentNote.AddedFromIP,
                            Archive = adjustmentNote.Archive,
                            ModelState = adjustmentNote.ModelState,
                        };
                        _adjustmentNoteService.InsertAdjustmentNoteDetail(adjustmentNote, adjustmentNoteDetail, currentInvoiceWriteOffDetailId);

                        var voucherDetailDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            EntityId = voucherVM.EntityId,
                            DrAmount = voucherDetailVM.DrAmount,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration,
                            PartyId = adjustmentNote.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            PartyType = adjustmentNote.PartyType,
                            AdjustmentNoteDetailId = adjustmentNoteDetail.Id
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.ToCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.ToCurrencyRate),
                            DrAmount = voucherVM.ToCurrencyRate * voucherDetailDr.DrAmount
                        });

                        totalAmountDr += voucherDetailDr.DrAmount;
                        totalCurrencyAmountDr += voucherVM.ToCurrencyRate * voucherDetailDr.DrAmount;

                    }
                    else if (voucherDetailVM.TrnType == "Cr" && voucherDetailVM.OtherName=="Material")
                    {
                      
                        var voucherDetailCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            EntityId = voucherVM.EntityId,
                            CrAmount = voucherDetailVM.CrAmount,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration,
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.ToCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.ToCurrencyRate),
                            CrAmount = voucherVM.ToCurrencyRate * voucherDetailCr.CrAmount
                        });

                        totalAmountCr += voucherDetailCr.CrAmount;
                        totalCurrencyAmountCr += voucherVM.ToCurrencyRate * voucherDetailCr.CrAmount;

                    }
                    else if (voucherDetailVM.TrnType == "Cr" && voucherDetailVM.OtherName == "Tax")
                    {

                        var voucherDetailCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            EntityId = voucherVM.EntityId,
                            CrAmount = voucherDetailVM.CrAmount,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration,
                            PaymentSource="Tax"
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.ToCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.ToCurrencyRate),
                            CrAmount = voucherVM.ToCurrencyRate * voucherDetailCr.CrAmount
                        });

                        totalAmountCr += voucherDetailCr.CrAmount;
                        totalCurrencyAmountCr += voucherVM.ToCurrencyRate * voucherDetailCr.CrAmount;

                    }
                }

                //var taxDetailVMList = _additionalTaxDetailRepository.Query(r => r.AdditionalTaxId == additionalTaxId).Select().ToList();
                //if (voucherVM.PaymentSource == PaymentSource.Tax.ToString())
                //{
                //    if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                //    {
                //        var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                //        foreach (var invoiceTaxVM in taxDetailVMList)
                //        {
                //            var invoiceTax = new InvoiceTax
                //            {
                //                VoucherDetailId = voucherDetailTempId,
                //                TaxCodeId = invoiceTaxVM.TaxCodeId,
                //                TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                //                TaxAmount = invoiceTaxVM.Amount,
                //                TaxAutoAmount = 0
                //            };
                //            totalAmountCr += invoiceTaxVM.Amount;
                //            _invoiceTaxService.InsertInvoiceTax(invoiceWriteOff, invoiceTax, invoiceTaxPk);

                //            var invoiceTaxDetail = new InvoiceTaxDetail
                //            {
                //                GLGeneralInfoId = invoiceTaxVM.GLGeneralInfoId,
                //                BudgetMasterId = invoiceTaxVM.BudgetMasterId,
                //                ActivityId = invoiceTaxVM.ActivityId,
                //                Amount = invoiceTax.TaxAmount,
                //                AType = "Cr"
                //            };
                //            _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                //            var voucherDetailTax = new VoucherDetail
                //            {
                //                GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                //                BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                //                ActivityId = invoiceTaxDetail.ActivityId,
                //                InvoiceTaxDetailId = invoiceTaxDetail.Id,
                //                CrAmount = invoiceTaxDetail.Amount,
                //            };
                //            currentVoucherDetailId++;
                //            _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                //            var voucherDetailCurrencyTax = new VoucherDetailCurrency
                //            {
                //                ToCurrencyRate = voucherVM.ToCurrencyRate,
                //                ToCurrencyId = companyCurrencyId,
                //                ParallelCurrencyId = companyCurrencyId,
                //                FromCurrencyId = companyCurrencyId,
                //                CrAmount = voucherVM.ToCurrencyRate * voucherDetailTax.CrAmount,
                //                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                //            };
                //            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                //            totalCurrencyAmountCr += voucherVM.ToCurrencyRate * voucherDetailTax.CrAmount;
                //        }
                //    }
                //}

                totalCurrencyAmountCr = totalCurrencyAmountDr;
                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                //if (totalCurrencyAmountCr != totalCurrencyAmountDr)
                //    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
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
        public string InsertCreditNoteAdditionalTaxPost(VoucherViewModel voucherVM, string additionalTaxId)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                var additionalTax = _additionalTaxRepository.Find(additionalTaxId);
                if (additionalTax.VoucherId != null)
                    throw new CustomException("Additional Tax already posted.");
                var voucherDetailVMList = _adjustmentNoteService.QueryAdjustmentNoteDetail(additionalTax.AdjustmentNoteId).Select().ToList();
                voucherVM.PartyId = additionalTax.PartyId;
                voucherVM.PartyPlantId = additionalTax.PartyPlantId;
                voucherVM.IsPark = false;
                voucherVM.Amount = additionalTax.TaxAmount;
                voucherVM.Narration = "TDS Of" + voucherVM.PartyName;
                var invoiceWriteOff = _invoiceWriteOffService.InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher

                var voucher = _voucherService.InsertVoucher(voucherVM);
                AuditService.PostedLog(voucher);
                additionalTax.VoucherId = voucher.Id;
                AuditService.UpdatedLog(additionalTax);
                _additionalTaxRepository.Update(additionalTax);
                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                decimal taxDrAmount = 0;
                var withholdgl = false;

                var adjustmentNoteIds = voucherDetailVMList.Select(r => r.AdjustmentNoteId);
                var adjustmentNoteDbList = _adjustmentNoteService.Query(r => adjustmentNoteIds.Contains(r.Id)).Select().ToList();
                var adjustmentNoteDetailIds = voucherDetailVMList.Select(r => r.Id);
                var adjustmentNoteDetailDbList = _adjustmentNoteService.QueryInvoiceDetailEnumerable(adjustmentNoteDetailIds);
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var adjustmentNoteDetail = adjustmentNoteDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.Id);
                    if (null == adjustmentNoteDetail)
                        throw new CustomException("AdjustmentNote not found!");

                    adjustmentNoteDetail.WrittenOffAmount += additionalTax.TaxAmount;

                    if (adjustmentNoteDetail.Amount < adjustmentNoteDetail.WrittenOffAmount)
                        throw new CustomException("Received amount can not cross balance amount.");

                    adjustmentNoteDetail.IsWrittenOff = adjustmentNoteDetail.Amount == adjustmentNoteDetail.WrittenOffAmount;
                    adjustmentNoteDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                    adjustmentNoteDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                    adjustmentNoteDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _adjustmentNoteService.UpdateAdjustmentNoteDetail(adjustmentNoteDetail);

                    // TODO: have a gap here if invoice split
                    var adjustmentNote = adjustmentNoteDbList.First(r => r.Id == adjustmentNoteDetail.AdjustmentNoteId);
                    adjustmentNote.WrittenOffAmount += additionalTax.TaxAmount;
                    adjustmentNote.IsWrittenOff = adjustmentNote.Amount == adjustmentNote.WrittenOffAmount;
                    adjustmentNote.UpdatedBy = invoiceWriteOff.AddedBy;
                    adjustmentNote.UpdatedDate = invoiceWriteOff.AddedDate;
                    adjustmentNote.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    adjustmentNote.DocDate = adjustmentNote.DocDate;
                    adjustmentNote.ModelState = ModelState.Modified;
                    _adjustmentNoteService.Update(adjustmentNote);

                    // INSERT INTO InvoiceDetail
                    currentInvoiceWriteOffDetailId++;
                    var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherVM.InvoicingPartyPlantId,
                        ActivityId = voucherDetailVM.ActivityId,
                        CurrencyId = voucherVM.CurrencyId,
                        InvoiceWriteOffId = invoiceWriteOff.Id,
                        AdjustmentNoteDetailId = voucherDetailVM.Id,
                        AdjustmentNoteId = voucherDetailVM.AdjustmentNoteId,
                        Amount = additionalTax.TaxAmount,
                        AddedBy = invoiceWriteOff.AddedBy,
                        AddedDate = invoiceWriteOff.AddedDate,
                        AddedFromIP = invoiceWriteOff.AddedFromIP,
                        Archive = invoiceWriteOff.Archive,
                        ModelState = invoiceWriteOff.ModelState,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration
                    };
                    _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);

                    var voucherDetailDr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        EntityId = voucherVM.EntityId,
                        DrAmount = additionalTax.TaxAmount,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        PartyType = invoiceWriteOff.PartyType,
                        InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.ToCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.ToCurrencyRate),
                        DrAmount = voucherVM.ToCurrencyRate * voucherDetailDr.DrAmount
                    });

                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalCurrencyAmountDr += voucherVM.ToCurrencyRate * voucherDetailDr.DrAmount;

                }

                var taxDetailVMList = _additionalTaxDetailRepository.Query(r => r.AdditionalTaxId == additionalTaxId).Select().ToList();
                if (voucherVM.PaymentSource == PaymentSource.Tax.ToString())
                {
                    if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                    {
                        var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                        foreach (var invoiceTaxVM in taxDetailVMList)
                        {
                            var invoiceTax = new InvoiceTax
                            {
                                VoucherDetailId = voucherDetailTempId,
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                TaxAmount = invoiceTaxVM.Amount,
                                TaxAutoAmount = 0
                            };
                            totalAmountCr += invoiceTaxVM.Amount;
                            _invoiceTaxService.InsertInvoiceTax(invoiceWriteOff, invoiceTax, invoiceTaxPk);

                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = invoiceTaxVM.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxVM.BudgetMasterId,
                                ActivityId = invoiceTaxVM.ActivityId,
                                Amount = invoiceTax.TaxAmount,
                                AType = "Cr"
                            };
                            _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                            var voucherDetailTax = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                ActivityId = invoiceTaxDetail.ActivityId,
                                InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                CrAmount = invoiceTaxDetail.Amount,
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                            var voucherDetailCurrencyTax = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = companyCurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherDetailTax.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                            totalCurrencyAmountCr += voucherVM.ToCurrencyRate * voucherDetailTax.CrAmount;
                        }
                    }
                }

                totalCurrencyAmountCr = totalCurrencyAmountDr;
                totalAmountCr += taxDrAmount;
                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                //if (totalCurrencyAmountCr != totalCurrencyAmountDr)
                //    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
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

        private string GetEmployeeSubsequentTransactionPK()
        {
            return _invoiceService.GetAutoNumber("EmployeeSubsequentTransaction", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public string InsertEmployeePayable(string receiveId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id


                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                var receiveData = _inventoryReceiveRepository.Find(receiveId);
                if (receiveData.Status == "Posting")
                    throw new CustomException("The GRN no '" + receiveData.Id + "' already Posted!");
                voucherVM.PostingDate = receiveData.GRNDate;

                #endregion Get Company Parallerl Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;



                // INSERT INTO Invoice TABLE
                var employeePayable = new EmployeePayable
                {
                    Amount = voucherVM.Amount,
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    CurrencyId = voucherVM.CurrencyId,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    EntityId = voucherVM.EntityId,
                    PlantId = voucherVM.PlantId,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = PartyType.Employee.ToString(),
                    EmployeeId = voucherVM.EmployeeId,
                    PaymentTermId = voucherVM.PaymentTermId,
                    PostingDate = receiveData.GRNDate,
                    SourceType = SourceType.InventoryPayable.ToString(),
                    RevisedDueDate = voucherVM.MatureDate,
                    ActualDueDate = voucherVM.MatureDate,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    EmployeeTransactionTypeId = voucherVM.EmployeeTransactionTypeId,
                    CompanyCurrencyRate = voucherVM.ToCurrencyRate
                };
                _employeePayableService.InsertEmployeePayable(employeePayable);

                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = employeePayable.CompanyGroupId,
                    CompanyId = employeePayable.CompanyId,
                    PlantId = employeePayable.PlantId,
                    EntityId = employeePayable.EntityId,
                    CurrencyId = employeePayable.CurrencyId,
                    FiscalYearId = employeePayable.FiscalYearId,
                    FiscalYearPeriodId = employeePayable.FiscalYearPeriodId,
                    TaxYearId = employeePayable.TaxYearId,
                    TaxYearPeriodId = employeePayable.TaxYearPeriodId,
                    AddedBy = employeePayable.AddedBy,
                    AddedDate = employeePayable.AddedDate,
                    AddedFromIP = employeePayable.AddedFromIP,
                    VoucherDate = employeePayable.VoucherDate,
                    DocDate = employeePayable.DocDate,
                    DocRefNo = employeePayable.DocRefNo,
                    Archive = employeePayable.Archive,
                    IsPark = employeePayable.IsPark,
                    Narration = SourceType.InventoryPayable.ToString(),
                    PostingDate = receiveData.GRNDate,
                    SourceType = SourceType.InventoryPayable.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };
                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                //For check Budget is applied in company or not.
                //var comdata = _companyService.Find(voucher.CompanyId);
                // Set to Invoice
                employeePayable.VoucherId = voucher.Id;

                var currentInvoiceDetail = 0;
                var currentVoucherDetaiRecord = 0;
                var currentTaxRecord = 0;
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                foreach (var voucherDetailVM in voucherDetailVMList.Where(r => r.Amount > 0))
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                        throw new CustomException("Without Budget can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                        throw new CustomException("Without Activity can not post.");
                    if (voucherDetailVM.TrnType == "Dr")
                    {
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            PostingWithoutTaxAllow = voucherVM.IsExcludingTax
                        };
                        voucherDetailVM.Id = voucherDr.Id;
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                        foreach (var item in inventoryReceiveDetailVMList.Where(r => r.GLGeneralInfoId == voucherDr.GLGeneralInfoId
                        && r.BudgetMasterId == voucherDr.BudgetMasterId && r.ActivityId == voucherDr.ActivityId))
                        {
                            var inventoryReceiveDetail = _inventoryReceiveDetailRepository.Find(item.InventoryReceiveDetailId);
                            var payableCR = voucherDetailVMList.Where(r => r.OtherName == "Vendor" && r.TrnType == "Cr").FirstOrDefault();
                            inventoryReceiveDetail.PostDrGLGeneralInfoId = voucherDr.GLGeneralInfoId;
                            inventoryReceiveDetail.PostDrBudgetMasterId = voucherDr.BudgetMasterId;
                            inventoryReceiveDetail.PostDrActivityId = voucherDr.ActivityId;
                            inventoryReceiveDetail.IsAsset = voucherDetailVM.IsAsset;
                            inventoryReceiveDetail.PostCrGLGeneralInfoId = payableCR.GLGeneralInfoId;
                            inventoryReceiveDetail.PostCrBudgetMasterId = payableCR.BudgetMasterId;
                            inventoryReceiveDetail.PostCrActivityId = payableCR.ActivityId;
                            _inventoryReceiveDetailRepository.Update(inventoryReceiveDetail);
                        }

                        if (voucherDetailVM.OtherName == "Tax")
                        {
                            var voucherDetailDrId = voucherDetailVMList.FirstOrDefault(t => t.TrnType == "Dr" && t.OtherName == "Tax" && t.MaterialGroupMasterId == voucherDetailVM.MaterialGroupMasterId).Id;
                            currentTaxRecord++;
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                VoucherDetailId = voucherDr.Id,//voucherDetailDrId,
                                VoucherId = voucher.Id,
                                EmployeePayableId = employeePayable.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.InventoryPayable.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxService.InsertInvoiceTax(employeePayable, invoiceTax, invoiceTaxPk);

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
                            var inventoryreceivetax = _inventoryReceiveTaxRepository.Query(r => r.InventoryReceiveId == receiveId && r.InventoryReceiveDetailId != null && r.TaxCategoryId == voucherDetailVM.TaxCategoryId).Select().ToList();

                            foreach (var DrTax in inventoryreceivetax)
                            {
                                DrTax.DrVoucherDetailId = voucherDr.Id;
                                _inventoryReceiveTaxRepository.Update(DrTax);
                            }
                        }
                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                           
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                    else if (voucherDetailVM.TrnType == "Cr")
                    {
                        currentInvoiceDetail++;
                        // INSERT INTO InvoiceDetail
                        var employeePayableDetail = new EmployeePayableDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            //MaterialGroupMasterId = voucherDetailVM.MaterialGroupMasterId,
                            Amount = voucherDetailVM.Amount,
                            NetAmount = voucherDetailVM.Amount,
                            TaxAmount = 0,
                            AddedBy = employeePayable.AddedBy,
                            AddedDate = employeePayable.AddedDate,
                            AddedFromIP = employeePayable.AddedFromIP,
                            Archive = employeePayable.Archive,
                            Id =_pkGeneratorService.MakePK(employeePayable.Id, currentInvoiceDetail, 2),
                            EmployeePayableId = employeePayable.Id,
                        };
                        employeePayable.Amount = employeePayableDetail.Amount;
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = employeePayableDetail.GLGeneralInfoId,
                            BudgetMasterId = employeePayableDetail.BudgetMasterId,
                            ActivityId = employeePayableDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = employeePayable.Narration,
                            EmployeeId = employeePayable.EmployeeId,
                            EmployeePayableDetailId = employeePayableDetail.Id,
                            PartyId = employeePayable.PartyId,
                            PartyPlantId = employeePayable.PartyPlantId,
                            PartyType = employeePayable.PartyType,
                            PostingWithoutTaxAllow = voucherVM.IsExcludingTax
                        };
                        voucherDetailVM.Id = voucherCr.Id;


                        if (voucherDetailVM.OtherName == "Tax")
                        {
                            var voucherDetailCrrId = voucherDetailVMList.FirstOrDefault(t => t.TrnType == "Cr" && t.OtherName == "Tax").Id;
                            currentTaxRecord++;
                            var invoiceTaxCR = new InvoiceTax
                            {
                                Archive = false,
                                VoucherDetailId = voucherCr.Id,//voucherDetailCrrId,
                                EmployeePayableId = employeePayable.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.InventoryPayable.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxService.InsertInvoiceTax(employeePayable, invoiceTaxCR, invoiceTaxPk);

                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                Id = invoiceTaxCR.Id + 1,
                                InvoiceTaxId = invoiceTaxCR.Id,
                                Amount = invoiceTaxCR.TaxAmount,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                AType = "Cr",
                                Archive = false,
                                ModelState = ModelState.Added,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                            var inventoryreceivetax = _inventoryReceiveTaxRepository.Query(r => r.InventoryReceiveId == receiveId && r.InventoryReceiveDetailId != null && r.TaxCategoryId == voucherDetailVM.TaxCategoryId).Select().ToList();

                            foreach (var Crtax in inventoryreceivetax)
                            {
                                Crtax.CrVoucherDetailId = voucherCr.Id;
                                _inventoryReceiveTaxRepository.Update(Crtax);
                            }
                        }

                        _employeePayableDetailRepository.Insert(employeePayableDetail);
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);
                        var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                        {
                            CompanyGroupId = voucherVM.CompanyGroupId,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            EntityId = voucherVM.EntityId,
                            VoucherTypeId = voucherVM.VoucherTypeId,
                            AdvanceId = null,
                            EmployeeId = employeePayable.EmployeeId,
                            EmployeeTransactionTypeId = employeePayable.EmployeeTransactionTypeId,
                            AdvanceWriteOffId = null,
                            EmployeePayableId = employeePayable.Id,
                            PartyType = employeePayable.PartyType,
                            CurrencyId = employeePayable.CurrencyId,
                            Amount = voucherCr.CrAmount,
                            VoucherDate = employeePayable.VoucherDate,
                            PostingDate = receiveData.GRNDate,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            JournalType = voucherVM.JournalType,
                            TransactionType = EmployeeSubsequentTranEnum.Payable.ToString(),
                            Narration = voucherVM.Narration,
                            SourceType = SourceType.InventoryPayable.ToString(),
                            IsPark = voucherVM.IsPark,
                            Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                            VoucherId = voucher.Id,
                            VoucherDetailId = voucherCr.Id,
                            PaymentSource = voucherVM.PaymentSource,
                        };
                        AuditService.AddedLog(EmployeeSubsequentAdvance);
                        _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);

                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                           
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                }

                // Update Inventory Received

                receiveData.Status = "Posting";
                receiveData.ModelState = ModelState.Modified;
                receiveData.VoucherId = voucher.Id;
                AuditService.UpdatedLog(receiveData);
                employeePayable.InventoryReceiveId = receiveId;
                _inventoryReceiveRepository.Update(receiveData);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                return voucher.VoucherNo;
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


        private string GetInvoiceWriteOffAutoNumber()
        {
            return _invoiceService.GetAutoNumber("InvoiceWriteOff", PKGeneratorEnum.Auto, null, DateTime.Now);
        }


        public void InsertInventoryShortagePayable(string receiveId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                var receiveData = _inventoryReceiveRepository.Find(receiveId);
                #endregion Get Company Parallerl Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Invoice TABLE
                voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount / 2);
                var adjustmentNote = new AdjustmentNote
                {
                    Amount = voucherVM.Amount,
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    CurrencyId = voucherVM.CurrencyId,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    PlantId = voucherVM.PlantId,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = PartyType.Vendor.ToString(),
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.InventoryShortagePayable.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    InventoryReceiveId = voucherVM.InventoryReceiveId,
                    Id = _invoiceService.GetAutoNumber("AdjustmentNote", PKGeneratorEnum.Auto, null, DateTime.Now),
                    NoteType = NoteType.VendorDebitNote.ToString()
                };


                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = adjustmentNote.CompanyGroupId,
                    CompanyId = adjustmentNote.CompanyId,
                    PlantId = adjustmentNote.PlantId,
                    CurrencyId = adjustmentNote.CurrencyId,
                    FiscalYearId = adjustmentNote.FiscalYearId,
                    FiscalYearPeriodId = adjustmentNote.FiscalYearPeriodId,
                    TaxYearId = adjustmentNote.TaxYearId,
                    TaxYearPeriodId = adjustmentNote.TaxYearPeriodId,
                    AddedBy = adjustmentNote.AddedBy,
                    AddedDate = adjustmentNote.AddedDate,
                    AddedFromIP = adjustmentNote.AddedFromIP,
                    VoucherDate = adjustmentNote.VoucherDate,
                    DocDate = adjustmentNote.DocDate,
                    DocRefNo = adjustmentNote.DocRefNo,
                    Archive = adjustmentNote.Archive,
                    IsPark = adjustmentNote.IsPark,
                    Narration = SourceType.InventoryPayable.ToString(),
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.DebitNote.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };
                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);
                adjustmentNote.VoucherId = voucher.Id;
                _adjustmentNoteService.Insert(adjustmentNote);
                //For check Budget is applied in company or not.
                //var comdata = _companyService.Find(voucher.CompanyId);
                // Set to Invoice

                var currentInvoiceDetail = 0;
                var currentVoucherDetaiRecord = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                        throw new CustomException("Without Budget can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                        throw new CustomException("Without Activity can not post.");
                    if (voucherDetailVM.TrnType == "Cr")
                    {
                        // in libility side Dr.
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CrAmount = voucherDetailVM.Amount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                        };
                        voucherDetailVM.Id = voucherCr.Id;
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);

                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                           
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                    else if (voucherDetailVM.TrnType == "Dr")
                    {
                        //var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                        //var inviceDbList = _invoiceRepository.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                        //var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                        //var inviceDetailDbList = _invoiceDetailRepository.Query(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                        //var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                        //if (null == invoiceDetail)
                        //    throw new CustomException("Invoice not found!");

                        //invoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;

                        //if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                        //    throw new CustomException("Received amount can not cross balance amount.");

                        //invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                        //invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                        //invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                        //invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        //_invoiceDetailRepository.Update(invoiceDetail);

                        //// TODO: have a gap here if invoice split
                        //var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                        //invoice.WrittenOffAmount += voucherDetailVM.Amount;
                        //invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                        //invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                        //invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                        //invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        //_invoiceRepository.Update(invoice);

                        //var invoicedetail = _invoiceDetailRepository.Query(r => r.InvoiceId == voucherDetailVM.InvoiceId).Select().FirstOrDefault();
                        currentInvoiceDetail++;
                        // INSERT INTO InvoiceDetail
                        var adjustmentNoteDetail = new AdjustmentNoteDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            // MaterialGroupMasterId = voucherDetailVM.MaterialGroupMasterId,
                            Amount = voucherDetailVM.Amount,
                            AddedBy = adjustmentNote.AddedBy,
                            AddedDate = adjustmentNote.AddedDate,
                            AddedFromIP = adjustmentNote.AddedFromIP,
                            Archive = adjustmentNote.Archive,
                            Id =_pkGeneratorService.MakePK(adjustmentNote.Id, currentInvoiceDetail, 1),
                            AdjustmentNoteId = adjustmentNote.Id,
                            InventoryReceiveId = voucherVM.InventoryReceiveId,
                            InventoryReceiveDetailId = voucherDetailVM.InventoryReceiveDetailId
                        };
                        adjustmentNote.Amount = adjustmentNoteDetail.Amount;
                        // INSERT INTO VoucherDetail
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = adjustmentNoteDetail.GLGeneralInfoId,
                            BudgetMasterId = adjustmentNoteDetail.BudgetMasterId,
                            ActivityId = adjustmentNoteDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            CrAmount = 0,
                            DrAmount = voucherDetailVM.Amount,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = adjustmentNote.Narration,
                            AdjustmentNoteDetailId = adjustmentNoteDetail.Id,
                            PartyId = adjustmentNote.PartyId,
                            PartyPlantId = adjustmentNote.PartyPlantId,
                            PartyType = adjustmentNote.PartyType,
                        };
                        voucherDetailVM.Id = voucherDr.Id;

                        _AdjustmentNoteDetailRepository.Insert(adjustmentNoteDetail);
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                           
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
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

        public void InsertInventoryRejectPayable(string receiveId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                #endregion Get Company Parallerl Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Invoice TABLE
                voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount / 2);
                var adjustmentNote = new AdjustmentNote
                {
                    Amount = voucherVM.Amount,
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    CurrencyId = voucherVM.CurrencyId,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    PlantId = voucherVM.PlantId,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = PartyType.Vendor.ToString(),
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.InventoryRejectPayable.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    InventoryReceiveId = voucherVM.InventoryReceiveId,
                    Id = _invoiceService.GetAutoNumber("AdjustmentNote", PKGeneratorEnum.Auto, null, DateTime.Now),
                    NoteType = NoteType.VendorDebitNote.ToString()
                };


                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = adjustmentNote.CompanyGroupId,
                    CompanyId = adjustmentNote.CompanyId,
                    PlantId = adjustmentNote.PlantId,
                    CurrencyId = adjustmentNote.CurrencyId,
                    FiscalYearId = adjustmentNote.FiscalYearId,
                    FiscalYearPeriodId = adjustmentNote.FiscalYearPeriodId,
                    TaxYearId = adjustmentNote.TaxYearId,
                    TaxYearPeriodId = adjustmentNote.TaxYearPeriodId,
                    AddedBy = adjustmentNote.AddedBy,
                    AddedDate = adjustmentNote.AddedDate,
                    AddedFromIP = adjustmentNote.AddedFromIP,
                    VoucherDate = adjustmentNote.VoucherDate,
                    DocDate = adjustmentNote.DocDate,
                    DocRefNo = adjustmentNote.DocRefNo,
                    Archive = adjustmentNote.Archive,
                    IsPark = adjustmentNote.IsPark,
                    Narration = SourceType.InventoryPayable.ToString(),
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.DebitNote.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };
                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);
                adjustmentNote.VoucherId = voucher.Id;
                _adjustmentNoteService.Insert(adjustmentNote);
                //For check Budget is applied in company or not.
                //var comdata = _companyService.Find(voucher.CompanyId);
                // Set to Invoice

                var currentInvoiceDetail = 0;
                var currentVoucherDetaiRecord = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                        throw new CustomException("Without Budget can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                        throw new CustomException("Without Activity can not post.");
                    if (voucherDetailVM.TrnType == "Cr")
                    {
                        // in libility side Dr.
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CrAmount = voucherDetailVM.Amount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                        };
                        voucherDetailVM.Id = voucherCr.Id;
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);

                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                           
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                    else if (voucherDetailVM.TrnType == "Dr")
                    {
                        //var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                        //var inviceDbList = _invoiceRepository.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                        //var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                        //var inviceDetailDbList = _invoiceDetailRepository.Query(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                        //var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                        //if (null == invoiceDetail)
                        //    throw new CustomException("Invoice not found!");

                        //invoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;

                        //if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                        //    throw new CustomException("Received amount can not cross balance amount.");

                        //invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                        //invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                        //invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                        //invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        //_invoiceDetailRepository.Update(invoiceDetail);

                        //// TODO: have a gap here if invoice split
                        //var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                        //invoice.WrittenOffAmount += voucherDetailVM.Amount;
                        //invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                        //invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                        //invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                        //invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        //_invoiceRepository.Update(invoice);

                        //var invoicedetail = _invoiceDetailRepository.Query(r => r.InvoiceId == voucherDetailVM.InvoiceId).Select().FirstOrDefault();
                        currentInvoiceDetail++;
                        // INSERT INTO InvoiceDetail
                        var adjustmentNoteDetail = new AdjustmentNoteDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            // MaterialGroupMasterId = voucherDetailVM.MaterialGroupMasterId,
                            Amount = voucherDetailVM.Amount,
                            AddedBy = adjustmentNote.AddedBy,
                            AddedDate = adjustmentNote.AddedDate,
                            AddedFromIP = adjustmentNote.AddedFromIP,
                            Archive = adjustmentNote.Archive,
                            Id =_pkGeneratorService.MakePK(adjustmentNote.Id, currentInvoiceDetail, 1),
                            AdjustmentNoteId = adjustmentNote.Id,
                            InventoryReceiveId = voucherVM.InventoryReceiveId,
                            InventoryReceiveDetailId = voucherDetailVM.InventoryReceiveDetailId
                        };
                        adjustmentNote.Amount = adjustmentNoteDetail.Amount;
                        // INSERT INTO VoucherDetail
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = adjustmentNoteDetail.GLGeneralInfoId,
                            BudgetMasterId = adjustmentNoteDetail.BudgetMasterId,
                            ActivityId = adjustmentNoteDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            CrAmount = 0,
                            DrAmount = voucherDetailVM.Amount,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = adjustmentNote.Narration,
                            AdjustmentNoteDetailId = adjustmentNoteDetail.Id,
                            PartyId = adjustmentNote.PartyId,
                            PartyPlantId = adjustmentNote.PartyPlantId,
                            PartyType = adjustmentNote.PartyType,
                        };
                        voucherDetailVM.Id = voucherDr.Id;

                        _AdjustmentNoteDetailRepository.Insert(adjustmentNoteDetail);
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                           
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
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

        #region Issue Journal
        public void InsertIssueJournal(string issueId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
          , IEnumerable<InventoryMaterialViewModel> invIssueDetailList, IEnumerable<InventoryMaterialViewModel> invIssueDetailGLList)
        {
            var flag = false;
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster = null;
                string sql = "SELECT * FROM TRN.Voucher WHERE Docrefno='"+ issueId + "' and sourcetype='IssueJournal' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("Data save successfully ");
                }

                var issueData = _inventoryIssueService.FindInventoryIssue(issueId);
                voucherVM.PostingDate = issueData.IssueDate;
                voucherVM.EntityId = issueData.EntityId;

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                if (issueData.CurrencyId == companyCurrencyId)
                {
                    voucherVM.CompanyCurrencyRate = 1;
                }
                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.PostingDate,
                    DocRefNo = issueData.Id,
                    Narration = issueData.Remarks ?? "N/A",
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.IssueJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                AuditService.AddedLog(voucher);
                voucher.AddedBy = issueData.AddedBy;
                voucher.PostedBy = voucher.AddedBy;
                voucher.PostedFromIP = voucher.AddedFromIP;
                voucher.PostedDate = voucher.AddedDate;
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);
                //svoucher.PostedBy = voucher.AddedBy;
                var currentVoucherDetaiRecord = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (voucherDetailVM.TrnType == "Dr")
                    {
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration
                        };
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDr.DrAmount
                        });
                        foreach (var item in invIssueDetailList.Where(r=>r.GLGeneralInfoId== voucherDetailVM.GLGeneralInfoId 
                                && r.BudgetMasterId== voucherDetailVM.BudgetMasterId && r.ActivityId== voucherDetailVM.ActivityId))
                        {
                            var issueDetail = _inventoryIssueService.FindInventoryIssueDetail(item.InventoryIssueDetailId);
                            issueDetail.PostDrGLGeneralInfoId = item.GLGeneralInfoId;
                            issueDetail.PostDrBudgetMasterId = item.BudgetMasterId;
                            issueDetail.PostDrActivityId = item.ActivityId;
                            issueDetail.DrVoucherDetailId = voucherDr.Id;
                            issueDetail.ModelState = ModelState.Modified;
                            AuditService.UpdatedLog(issueDetail);
                            _inventoryIssueService.UpdateInventoryIssueDetail(issueDetail);
                        }

                    }
                    else if (voucherDetailVM.TrnType == "Cr")
                    {
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                            CostCenterId = voucherDetailVM.CostCenterId
                        };
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherCr.CrAmount
                        });
                        foreach (var item in invIssueDetailGLList.Where(r=>r.PostDrGLGeneralInfoId== voucherDetailVM.GLGeneralInfoId && r.PostDrBudgetMasterId== voucherDetailVM.BudgetMasterId 
                        && r.PostDrActivityId== voucherDetailVM.ActivityId))
                        {
                            var issueDetailGL = _inventoryIssueService.FindInventoryIssueDetail(item.InventoryIssueDetailId);
                            issueDetailGL.PostCrGLGeneralInfoId = item.PostDrGLGeneralInfoId;
                            issueDetailGL.PostCrBudgetMasterId = item.PostDrBudgetMasterId;
                            issueDetailGL.PostCrActivityId = item.PostDrActivityId;
                            issueDetailGL.CrVoucherDetailId = voucherCr.Id;
                            issueDetailGL.ModelState = ModelState.Modified;
                            AuditService.UpdatedLog(issueDetailGL);
                            _inventoryIssueService.UpdateInventoryIssueDetail(issueDetailGL);
                        }
                    }
                }
               
                // Update Inventory Received
                issueData.VoucherId = voucher.Id;
                issueData.Status = "Posting";
                issueData.ModelState = ModelState.Modified;
                AuditService.UpdatedLog(issueData);
                _inventoryIssueService.UpdateInventoryIssue(issueData);
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

        public void DeleteIssueJournal(string issueId, string voucherId)
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
                var inventoryIssue = _inventoryIssueService.FindInventoryIssue(issueId);
                if(inventoryIssue.CapitalizeVoucherId!=null)
                    throw new CustomException("Delete Capitalize Voucher first ! ");

                var inventoryIssueDetail = _inventoryIssueService.QueryInventoryIssueDetail(issueId).Select().ToList();

                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherService.DeleteVoucherDetailCurrency(item.Id);
                }
                foreach (var item in voucherdetail)
                {
                    var glTransactionDetail = _voucherService.QueryGLTransactionDetail(item.Id).Select().FirstOrDefault();
                    if (glTransactionDetail != null)
                    {
                        _voucherService.DeleteGLTransactionDetail(item.Id);
                    }
                    _voucherService.DeleteVoucherDetail(item.Id);
                }
                foreach (var item in inventoryIssueDetail)
                {
                    item.PostCrActivityId = null;
                    item.PostCrBudgetMasterId = null;
                    item.PostCrGLGeneralInfoId = null;
                    item.PostDrActivityId = null;
                    item.PostDrBudgetMasterId = null;
                    item.PostDrGLGeneralInfoId = null;
                    AuditService.UpdatedLog(item);
                    _inventoryIssueService.UpdateInventoryIssueDetail(item);
                }
                inventoryIssue.VoucherId = null;
                inventoryIssue.Status = null;
                AuditService.UpdatedLog(inventoryIssue);
                _inventoryIssueService.UpdateInventoryIssue(inventoryIssue);
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

        #endregion
        #region Issue Journal
        public void InsertIssueReturnJournal(string issueId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
          , IEnumerable<InventoryMaterialViewModel> invIssueDetailList, IEnumerable<InventoryMaterialViewModel> invIssueDetailGLList)
        {
            var flag = false;
            try
            {
                var issueData = _InventoryIssueReturnRepository.Find(issueId);
                voucherVM.PostingDate = issueData.IssueDate;
                voucherVM.EntityId = issueData.EntityId;

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                if (issueData.CurrencyId == companyCurrencyId)
                {
                    voucherVM.CompanyCurrencyRate = 1;
                }
                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.PostingDate,
                    DocRefNo = issueData.Id,
                    Narration = issueData.Remarks ?? "N/A",
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.IssueReturnJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);
                //svoucher.PostedBy = voucher.AddedBy;
                var currentVoucherDetaiRecord = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (voucherDetailVM.TrnType == "Dr")
                    {
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration
                        };
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDr.DrAmount
                        });
                        

                    }
                    else if (voucherDetailVM.TrnType == "Cr")
                    {
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                            CostCenterId = voucherDetailVM.CostCenterId
                        };
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherCr.CrAmount
                        });
                        
                    }
                }

                // Update Inventory Received
                issueData.VoucherId = voucher.Id;
                issueData.Status = "Posting";
                issueData.ModelState = ModelState.Modified;
                AuditService.UpdatedLog(issueData);
                _InventoryIssueReturnRepository.Update(issueData);
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

        public void DeleteIssueReturnJournal(string issueId, string voucherId)
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
                var inventoryIssue = _InventoryIssueReturnRepository.Find(issueId);
                if (inventoryIssue.CapitalizeVoucherId != null)
                    throw new CustomException("Delete Capitalize Voucher first ! ");

                

                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherService.DeleteVoucherDetailCurrency(item.Id);
                }
                foreach (var item in voucherdetail)
                {
                    var glTransactionDetail = _voucherService.QueryGLTransactionDetail(item.Id).Select().FirstOrDefault();
                    if (glTransactionDetail != null)
                    {
                        _voucherService.DeleteGLTransactionDetail(item.Id);
                    }
                    _voucherService.DeleteVoucherDetail(item.Id);
                }
                
                inventoryIssue.VoucherId = null;
                inventoryIssue.Status = null;
                AuditService.UpdatedLog(inventoryIssue);
                _InventoryIssueReturnRepository.Update(inventoryIssue);
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

        #endregion

        public void InsertGRNFixedAssetCapitalizeJournal(string issueId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.PostingDate,
                    DocRefNo = "",
                    Narration = "FixedAssetCapitalize",
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.FixedAssetCapitalizeJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                var currentVoucherDetaiRecord = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (voucherDetailVM.TrnType == "Dr")
                    {
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                        };
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDr.DrAmount * voucherVM.CompanyCurrencyRate
                        });


                    }
                    else if (voucherDetailVM.TrnType == "Cr")
                    {
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                        };
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);
                        //UPdate InventoryReceiveDetail
                        var inventoryReceiveDetail = _inventoryReceiveDetailRepository.Find(voucherDetailVM.InventoryReceiveDetailId);
                        inventoryReceiveDetail.CapitalizeVoucherDetailId = voucherCr.Id;
                        _inventoryReceiveDetailRepository.Update(inventoryReceiveDetail);
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherCr.CrAmount * voucherVM.CompanyCurrencyRate
                        });
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



        //private static string MakePK(string masterId, int currentId, int padLeft)
        //{
        //    return masterId + currentId.ToString().PadLeft(padLeft, '0');
        //}

        private static void CurrencyExchange(string transactionCurrencyId, string companyCurrencyId, string companyGroupCurrencyId, string hardCurrencyId,
            decimal companyCurrencyAmount, decimal companyGroupCurrencyAmount, decimal hardCurrencyAmount, VoucherDetailCurrencyViewModel voucherDetailCurrencyVM, decimal amount)
        {
            // Set to company currency id.
            voucherDetailCurrencyVM.ToCurrencyId = companyCurrencyId;
            if (transactionCurrencyId == companyCurrencyId)
            {
                voucherDetailCurrencyVM.CompanyCurrencyConversion = 1;

                if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                {
                    voucherDetailCurrencyVM.CompanyGroupCurrencyConversion = voucherDetailCurrencyVM.CompanyCurrencyConversion / voucherDetailCurrencyVM.CompanyGroupCurrencyRate;
                }
                if (!string.IsNullOrEmpty(hardCurrencyId))
                {
                    voucherDetailCurrencyVM.HardCurrencyConversion = voucherDetailCurrencyVM.CompanyCurrencyConversion / voucherDetailCurrencyVM.HardCurrencyRate;
                }
            }
            else if (!string.IsNullOrEmpty(companyGroupCurrencyId) && transactionCurrencyId == companyGroupCurrencyId)
            {
                voucherDetailCurrencyVM.CompanyGroupCurrencyConversion = 1;

                voucherDetailCurrencyVM.CompanyCurrencyConversion = voucherDetailCurrencyVM.CompanyGroupCurrencyConversion * voucherDetailCurrencyVM.CompanyGroupCurrencyRate;

                if (!string.IsNullOrEmpty(hardCurrencyId))
                {
                    voucherDetailCurrencyVM.HardCurrencyConversion = voucherDetailCurrencyVM.CompanyCurrencyConversion / voucherDetailCurrencyVM.HardCurrencyRate;
                }
            }
            else if (!string.IsNullOrEmpty(hardCurrencyId) && transactionCurrencyId == hardCurrencyId)
            {
                voucherDetailCurrencyVM.HardCurrencyConversion = 1;
                voucherDetailCurrencyVM.CompanyCurrencyConversion = voucherDetailCurrencyVM.HardCurrencyConversion * voucherDetailCurrencyVM.HardCurrencyRate;

                voucherDetailCurrencyVM.CompanyGroupCurrencyRate = companyCurrencyAmount / companyGroupCurrencyAmount;
                voucherDetailCurrencyVM.CompanyCurrencyConversion = voucherDetailCurrencyVM.CompanyCurrencyConversion / voucherDetailCurrencyVM.CompanyGroupCurrencyRate;
            }
            else
            {
                voucherDetailCurrencyVM.CompanyCurrencyConversion = 1 / voucherDetailCurrencyVM.CompanyCurrencyRate;
                voucherDetailCurrencyVM.CompanyFromCurrencyId = transactionCurrencyId;

                if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                {
                    voucherDetailCurrencyVM.CompanyCurrencyConversion = 1 / voucherDetailCurrencyVM.CompanyGroupCurrencyRate;
                }
                if (!string.IsNullOrEmpty(hardCurrencyId))
                {
                    voucherDetailCurrencyVM.HardCurrencyConversion = 1 / voucherDetailCurrencyVM.HardCurrencyRate;
                }
            }
        }

        public Dictionary<string, object> CheckingFiscalYearPeriod(string groupId, DateTime postingDate)
        {
            var sql = @"SELECT A.TaxYearId, A.Id AS TaxYearPeriodId, B.TaxYearCode, B.TaxYearName, A.PeriodNo, A.PeriodName FROM [SCS].[TaxYearPeriod] A
                        JOIN [SCS].[TaxYear] AS B ON A.TaxYearId=B.Id WHERE B.CompanyGroupId='" + groupId + @"' AND CAST('" + postingDate + @"' AS DATE) BETWEEN  CAST(A.StartDate AS DATE) AND  CAST(A.EndDate AS DATE)";
            var data = _sqlRepository.GetData(sql);
            if (null == data || data.Count == 0)
                throw new CustomException("Tax year not found");
            return data;
        }

        public void InsertIssueFixedAssetCapitalizeJournal(string issueId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InventoryMaterialViewModel> invIssueDetailList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Voucher TABLE

                var issue = _inventoryIssueService.FindInventoryIssue(issueId);

                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = voucherVM.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = issue.IssueDate,
                    DocRefNo = issue.Id,
                    Narration = "FixedAssetCapitalize",
                    PostingDate = issue.IssueDate,
                    SourceType = SourceType.FixedAssetCapitalizeJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                issue.CapitalizeVoucherId = voucher.Id;
                issue.VoucherId = voucher.Id;
                _inventoryIssueService.UpdateInventoryIssue(issue);

                var currentVoucherDetaiRecord = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (voucherDetailVM.TrnType == "Dr")
                    {
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            FixedAssetMasterId = voucherDetailVM.FixedAssetMasterId,
                            FAType = "AssetCapatalized"
                        };
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDr.DrAmount * voucherVM.CompanyCurrencyRate
                        });
                        foreach (var item in invIssueDetailList.Where(r => r.PostDrBudgetMasterId == voucherDetailVM.BudgetMasterId && r.PostDrActivityId == voucherDetailVM.ActivityId))
                        {
                            var inventoryIssueHistory = _inventoryIssueService.FindInventoryIssueHistory(item.InventoryIssueHistoryId);
                            inventoryIssueHistory.CapitalizeVoucherDetailId = voucherDr.Id;
                            inventoryIssueHistory.IsCapitalize = true;
                            _inventoryIssueService.UpdateInventoryIssueHistory(inventoryIssueHistory);

                            var inventoryIssueDetail = _inventoryIssueService.FindInventoryIssueDetail(inventoryIssueHistory.InventoryIssueDetailId);
                            inventoryIssueDetail.PostDrGLGeneralInfoId = voucherDr.GLGeneralInfoId;
                            inventoryIssueDetail.PostDrBudgetMasterId = voucherDr.BudgetMasterId;
                            inventoryIssueDetail.PostDrActivityId = voucherDr.ActivityId;
                            inventoryIssueDetail.PostCrGLGeneralInfoId = item.PostCrGLGeneralInfoId;
                            inventoryIssueDetail.PostCrBudgetMasterId = item.PostCrBudgetMasterId;
                            inventoryIssueDetail.PostCrActivityId = item.PostCrActivityId;
                            _inventoryIssueService.UpdateInventoryIssueDetail(inventoryIssueDetail);
                        }

                    }
                    else if (voucherDetailVM.TrnType == "Cr")
                    {
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                        };
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherCr.CrAmount * voucherVM.CompanyCurrencyRate
                        });
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
        public void InsertExpensesCapitalizeJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Voucher TABLE


                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.Id,
                    Narration = "ExpensesCapitalize",
                    PostingDate = voucherVM.PostingDate,
                    SourceType = "ExpensesCapitalizeJournal",
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                var currentVoucherDetaiRecord = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (voucherDetailVM.TrnType == "Dr")
                    {
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVMList.Sum(r => r.CrAmount),
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            FixedAssetMasterId = voucherDetailVM.FixedAssetMasterId,
                            FAType = "ExpensesCapitalized"
                        };
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDr.DrAmount * voucherVM.CompanyCurrencyRate
                        });
                    }
                    else if (voucherDetailVM.TrnType == "Cr")
                    {
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.CrAmount,
                        };
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherCr.CrAmount * voucherVM.CompanyCurrencyRate
                        });
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


        public void InsertIssueInventoryCapitalizeJournal(string issueId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<InventoryMaterialViewModel> invIssueDetailList)
        {
            var flag = false;
            try
            {
                var issue = _inventoryIssueService.FindInventoryIssue(issueId);

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Voucher TABLE


                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.PostingDate,
                    DocRefNo = issue.Id,
                    Narration = "AssetNonCapitalized",
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.FixedAssetCapitalizeJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                issue.CapitalizeVoucherId = voucher.Id;
                _inventoryIssueService.UpdateInventoryIssue(issue);

                var currentVoucherDetaiRecord = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (voucherDetailVM.TrnType == "Dr")
                    {
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            FixedAssetMasterId = voucherDetailVM.FixedAssetMasterId,
                            FAType = "AssetNonCapitalized"
                        };
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDr.DrAmount * voucherVM.CompanyCurrencyRate
                        });
                        foreach (var item in invIssueDetailList.Where(r => r.PostDrBudgetMasterId == voucherDetailVM.BudgetMasterId && r.PostDrActivityId == voucherDetailVM.ActivityId))
                        {
                            var inventoryIssueHistory = _inventoryIssueService.FindInventoryIssueHistory(item.InventoryIssueHistoryId);
                            inventoryIssueHistory.CapitalizeVoucherDetailId = voucherDr.Id;
                            inventoryIssueHistory.IsCapitalize = true;
                            _inventoryIssueService.UpdateInventoryIssueHistory(inventoryIssueHistory);

                            //var inventoryIssueDetail = _inventoryIssueDetailRepository.Find(inventoryIssueHistory.InventoryIssueDetailId);
                            //inventoryIssueDetail.PostDrGLGeneralInfoId = voucherDr.GLGeneralInfoId;
                            //inventoryIssueDetail.PostDrBudgetMasterId = voucherDr.BudgetMasterId;
                            //inventoryIssueDetail.PostDrActivityId = voucherDr.ActivityId;
                            //inventoryIssueDetail.PostCrGLGeneralInfoId = item.PostCrGLGeneralInfoId;
                            //inventoryIssueDetail.PostCrBudgetMasterId = item.PostCrBudgetMasterId;
                            //inventoryIssueDetail.PostCrActivityId = item.PostCrActivityId;
                            //_inventoryIssueDetailRepository.Update(inventoryIssueDetail);
                        }

                    }
                    else if (voucherDetailVM.TrnType == "Cr")
                    {
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                        };
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherCr.CrAmount * voucherVM.CompanyCurrencyRate
                        });
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


        private InvoiceTax InsertInvoiceTax(VoucherViewModel invoice, InvoiceTax invoiceTax, PKGenerator pKGenerator)
        {
            pKGenerator.MaxNumber++;
            invoiceTax.Id = DateTime.Now.Year + pKGenerator.MaxNumber.ToString();
            invoiceTax.InvoiceId = invoice.Id;
            invoiceTax.TaxYearId = invoice.TaxYearId;
            invoiceTax.TaxYearPeriodId = invoice.TaxYearPeriodId;
            invoiceTax.VoucherId = invoice.VoucherId;
            invoiceTax.PartyId = invoice.PartyId;
            invoiceTax.PartyPlantId = invoice.PartyPlantId;
            invoiceTax.SourceType = invoice.SourceType;
            invoiceTax.AddedBy = invoice.AddedBy;
            invoiceTax.AddedDate = invoice.AddedDate;
            invoiceTax.AddedFromIP = invoice.AddedFromIP;
            _invoiceTaxRepository.Insert(invoiceTax);
            return invoiceTax;
        }
        public void PostDocumentAcceptance(VoucherViewModel voucherVM, IEnumerable<PurchaseDocAcceptanceDetailViewModel> docAcceptanceDetails, IEnumerable<PurchaseDocAcceptanceDetailViewModel> rowDetails, bool IsNonCreditable)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id

                var purchaseDocAcceptance = _purchaseDocAcceptanceRepository.Find(voucherVM.Id);
                if (purchaseDocAcceptance.VoucherId != null)
                    throw new CustomException("The Doc Acceptance no '" + purchaseDocAcceptance.Id + "' already Posted!");
                voucherVM.PostingDate = purchaseDocAcceptance.AcceptanceDate;

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);


                #endregion Get Company Parallerl Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Invoice TABLE

                var invoice = new Invoice
                {
                    Amount = voucherVM.Amount,
                    BaseNoOfDays = voucherVM.BaseNoOfDays,
                    BaseOnDueDate = voucherVM.BaseOnDueDate,
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    CurrencyId = voucherVM.CurrencyId,
                    DocDate = voucherVM.PostingDate,
                    DocRefNo = purchaseDocAcceptance.AcceptanceNo == null ? purchaseDocAcceptance.InvoiceNo : purchaseDocAcceptance.AcceptanceNo,
                    Narration = voucherVM.Narration,
                    EntityId = voucherVM.EntityId,
                    PlantId = voucherVM.PlantId,
                    IsExcludingTax = voucherVM.IsExcludingTax,
                    IsSplit = voucherVM.IsSplit,
                    PartyId = purchaseDocAcceptance.PartyId,
                    PartyPlantId = purchaseDocAcceptance.PartyPlantId,
                    PartyType = PartyType.Vendor.ToString(),
                    EmployeeId = voucherVM.EmployeeId,
                    PaymentTermId = voucherVM.PaymentTermId,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = voucherVM.SourceType,
                    RevisedDueDate = voucherVM.MatureDate,
                    ActualDueDate = voucherVM.MatureDate,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    CompanyCurrencyRate = voucherVM.ToCurrencyRate,
                    PurchaseDocAcceptanceId = purchaseDocAcceptance.Id
                };
                _invoiceService.InsertInvoice(invoice);
                invoice.InvoiceNo = invoice.Id;

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
                    VoucherDate = invoice.VoucherDate,
                    DocDate = purchaseDocAcceptance.AcceptanceDate,
                    DocRefNo = invoice.DocRefNo,
                    Archive = invoice.Archive,
                    IsPark = invoice.IsPark,
                    Narration = invoice.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = voucherVM.SourceType,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };
                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);
                purchaseDocAcceptance.VoucherId = voucher.Id;
                purchaseDocAcceptance.AcceptanceRate = voucherVM.ToCurrencyRate;
                _purchaseDocAcceptanceRepository.Update(purchaseDocAcceptance);

                invoice.VoucherId = voucher.Id;

                var currentInvoiceDetail = 0;
                var currentVoucherDetaiRecord = 0;
                decimal totalAmountDr = 0;
                decimal totalAmountCr = 0;
                decimal totalTaxAmount = 0;
                var currentTaxRecord = 0;

                foreach (var voucherDetailVM in rowDetails.Where(r => r.TrnType == "Dr"))
                {
                    if (voucherDetailVM.ClearingAccountGLId == null && voucherDetailVM.ClearingAccountBudgetMasterId == null && voucherDetailVM.ClearingAccountActivityId == null)
                        throw new CustomException("GL not found!");
                    var voucherDr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.ClearingAccountGLId,
                        BudgetMasterId = voucherDetailVM.ClearingAccountBudgetMasterId,
                        ActivityId = voucherDetailVM.ClearingAccountActivityId,
                        DrAmount = voucherDetailVM.TrnAmount,
                        CurrencyId = voucherVM.CurrencyId,
                        DocDate = purchaseDocAcceptance.AcceptanceDate,
                        DocRefNo = purchaseDocAcceptance.AcceptanceNo,
                        Narration = purchaseDocAcceptance.Remarks,
                        PostingWithoutTaxAllow = invoice.IsExcludingTax
                    };
                    totalAmountDr += voucherDr.DrAmount;
                    voucherDetailVM.Id = voucherDr.Id;
                    currentVoucherDetaiRecord++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                    foreach (var item in docAcceptanceDetails.Where(r => r.ClearingAccountGLId == voucherDr.GLGeneralInfoId && r.ClearingAccountBudgetMasterId == voucherDr.BudgetMasterId
                     && r.ClearingAccountActivityId == voucherDr.ActivityId))
                    {
                        var docAcceptanceDetail = _purchaseDocAcceptanceDetailRepository.Find(item.AcceptenceDetailId);
                        docAcceptanceDetail.GLGeneralInfoId = voucherDr.GLGeneralInfoId;
                        docAcceptanceDetail.BudgetMasterId = voucherDr.BudgetMasterId;
                        docAcceptanceDetail.ActivityId = voucherDr.ActivityId;
                        _purchaseDocAcceptanceDetailRepository.Update(docAcceptanceDetail);
                    }
                    #region Currency
                    var voucherDetailCurrencydb = new VoucherDetailCurrency
                    {
                        ToCurrencyRate = voucherVM.ToCurrencyRate,
                        ToCurrencyId = companyCurrencyId,
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherVM.CurrencyId,
                        DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                        ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                    };
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);

                    #endregion Currency
                }

                if (!IsNonCreditable)
                {
                    //var taxCategoryList = _purchaseDocAcceptanceServiceTaxRepository.Query(r => r.PurchaseDocAcceptanceId == voucherVM.Id).Select(r=>r.TaxCategoryId).ToList();

                    var acceptanceTax = _purchaseDocAcceptanceServiceTaxRepository.Query(r => r.PurchaseDocAcceptanceId == voucherVM.Id && r.TaxAmount>0).Select().ToList();
                    var taxCategoryList = acceptanceTax.Select(t => t.TaxCategoryId).Distinct().ToArray();
                    if (acceptanceTax != null)
                    {

                        foreach (var taxCatId in taxCategoryList)
                        {
                            totalTaxAmount += acceptanceTax.Where(r => r.TaxCategoryId == taxCatId).Sum(r => r.TaxAmount);
                            var taxCategoryGL = _accountsCommonService.GetTaxCategoryInputGL(taxCatId); 
                           
                            if (null == taxCategoryGL["GLGeneralInfoId"].ToString())
                                throw new CustomException("Tax Category Expenses GL not found!");

                            var voucherDr = new VoucherDetail
                            {
                                GLGeneralInfoId = taxCategoryGL["GLGeneralInfoId"].ToString(),
                                BudgetMasterId = taxCategoryGL["BudgetMasterId"].ToString(),
                                ActivityId = taxCategoryGL["ActivityId"].ToString(),
                                DrAmount = acceptanceTax.Where(r => r.TaxCategoryId == taxCatId).Sum(r => r.TaxAmount),
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = voucherVM.Narration,
                                PostingWithoutTaxAllow = invoice.IsExcludingTax
                            };
                            totalAmountDr += voucherDr.DrAmount;


                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);
                            #region Currency
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherVM.CurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);

                            #endregion Currency

                            currentTaxRecord++;
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                Id =_pkGeneratorService.MakePK(invoice.Id, currentTaxRecord, 2),
                                VoucherDetailId = voucherDr.Id,//voucherDetailDrId,
                                VoucherId = voucher.Id,
                                InvoiceId = invoice.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = taxCatId,
                                TaxAmount = acceptanceTax.Where(r => r.TaxCategoryId == taxCatId).Sum(r => r.TaxAmount),
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.PurchaseDocAcceptance.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxRepository.Insert(invoiceTax);
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                Id = invoiceTax.Id + 1,
                                InvoiceTaxId = invoiceTax.Id,
                                Amount = invoiceTax.TaxAmount,
                                GLGeneralInfoId = voucherDr.GLGeneralInfoId,
                                BudgetMasterId = voucherDr.BudgetMasterId,
                                ActivityId = voucherDr.ActivityId,
                                AType = "Dr",
                                AddedBy = invoiceTax.AddedBy,
                                AddedDate = invoiceTax.AddedDate,
                                AddedFromIP = invoiceTax.AddedFromIP
                            };
                            _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                        }

                    }

                }
                foreach (var voucherDetailVM in rowDetails.Where(r => r.TrnType == "Cr"))
                {
                    currentInvoiceDetail++;
                    // INSERT INTO InvoiceDetail
                    var invoiceDetail = new InvoiceDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        Amount = IsNonCreditable ? voucherVM.Amount : voucherVM.Amount + totalTaxAmount,
                        NetAmount = voucherVM.Amount,
                        TaxAmount = 0,
                        AddedBy = invoice.AddedBy,
                        AddedDate = invoice.AddedDate,
                        AddedFromIP = invoice.AddedFromIP,
                        Archive = invoice.Archive,
                        InvoiceId = invoice.Id,
                    };
                    invoice.Amount = invoiceDetail.Amount;
                   _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail);
                    // INSERT INTO VoucherDetail
                    var voucherCr = new VoucherDetail
                    {
                        GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                        BudgetMasterId = invoiceDetail.BudgetMasterId,
                        ActivityId = invoiceDetail.ActivityId,
                        CurrencyId = voucher.CurrencyId,
                        DrAmount = 0,
                        CrAmount = IsNonCreditable ? voucherVM.Amount : voucherVM.Amount + totalTaxAmount,
                        DocDate = voucher.DocDate,
                        DocRefNo = voucher.DocRefNo,
                        Narration = invoice.Narration,
                        EmployeeId = invoice.EmployeeId,
                        InvoiceDetailId = invoiceDetail.Id,
                        PartyId = invoice.PartyId,
                        PartyPlantId = invoice.PartyPlantId,
                        PartyType = invoice.PartyType,
                        PostingWithoutTaxAllow = invoice.IsExcludingTax
                    };
                    voucherDetailVM.Id = voucherCr.Id;
                    totalAmountCr += voucherCr.CrAmount;


                    //_invoiceDetailRepository.Insert(invoiceDetail);

                    currentVoucherDetaiRecord++;
                    _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);

                    #region Currency
                    var voucherDetailCurrencydb = new VoucherDetailCurrency
                    {
                        ToCurrencyRate = voucherVM.ToCurrencyRate,
                        ToCurrencyId = companyCurrencyId,
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherVM.CurrencyId,
                        CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                        ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                    };
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);

                    #endregion Currency
                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

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
        public void PostDocumentAcceptanceService(VoucherViewModel voucherVM, IEnumerable<PurchaseDocAcceptanceViewModel> voucherRows
            , IEnumerable<PurchaseDocAcceptanceChargesViewModel> purchaseDocAcceptanceServiceList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            try
            {
                bool flag = false;

                if (voucherRows != null)
                {
                    var purDocAcceptance = _purchaseDocAcceptanceRepository.Find(voucherRows.Select(r => r.PurchaseDocAcceptanceId).FirstOrDefault());
                    voucherVM.PostingDate = purDocAcceptance.InvoiceDate;
                    voucherVM.DocDate = purDocAcceptance.AcceptanceDate;
                    voucherVM.DocRefNo = purDocAcceptance.AcceptanceNo;
                    voucherVM.Narration = "Being Posting Purchase Doc Acceptance Charges of Acceptance No. " + voucherVM.DocRefNo;
                    AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                    _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                    _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                    _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var currentVoucherDetailId = 0;
                    var currentTaxRecord = 0;
                    decimal totalTaxAmount = 0;
                    decimal totalDrAmount = 0;
                    decimal totalCrAmount = 0;
                    foreach (var item in voucherRows)
                    {
                        voucherVM.CurrencyId = item.CurrencyId;
                        var voucher = _voucherService.InsertVoucher(voucherVM);

                        var chargesList = purchaseDocAcceptanceServiceList.Where(r => r.OpeningBankMasterId == item.OpeningBankMasterId).ToList();
                        decimal voucherDetailCurrencyCr = 0;

                        foreach (var cList in chargesList)
                        {
                            var purDocAcceptService = _purchaseDocAcceptanceServiceRepository.Find(cList.Id);
                            if (purDocAcceptService.VoucherId != null)
                                throw new CustomException("The Doc Acceptance Charges of Acceptance  no '" + purDocAcceptService.PurchaseDocAcceptanceId + "' already Posted!");
                            currentVoucherDetailId++;
                            var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                            {
                                GLGeneralInfoId = cList.ExpensesGLId,
                                BudgetMasterId = cList.ExpensesBudgetMasterId,
                                ActivityId = cList.ExpensesActivityId,
                                DrAmount = cList.Amount,
                            }, currentVoucherDetailId);
                            totalDrAmount += voucherDetailDr.DrAmount;
                            // INSERT INTO VoucherDetailCurrency
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.ToCurrencyRate),
                                DrAmount = voucherVM.ToCurrencyRate * voucherDetailDr.DrAmount
                            });
                            voucherDetailCurrencyCr += voucherVM.ToCurrencyRate * voucherDetailDr.DrAmount;


                            purDocAcceptService.VoucherId = voucher.Id;
                            purDocAcceptService.Rate = cList.Rate;
                            _purchaseDocAcceptanceServiceRepository.Update(purDocAcceptService);
                        }

                        if (taxDetailVMList != null)
                        {
                            var invoieTaxVM = taxDetailVMList.Where(r => r.PurchaseLCId == item.Id).ToList();


                            if (null != invoieTaxVM && invoieTaxVM.Count() > 0)
                            {
                                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                                foreach (var invoiceTaxVM in invoieTaxVM)
                                {
                                    var taxCategoryGL = _accountsCommonService.GetTaxCategoryInputGL(invoiceTaxVM.TaxCategoryId);
                                  
                                    if (null == taxCategoryGL["ExpensesGLId"].ToString())
                                        throw new CustomException("Tax Category Expenses GL not found!");

                                    currentTaxRecord++;
                                    var invoiceTax = new InvoiceTax
                                    {
                                        Id =_pkGeneratorService.MakePK(purDocAcceptance.Id, currentTaxRecord, 2),
                                        TaxCodeId = invoiceTaxVM.TaxCodeId,
                                        TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                        TaxAmount = invoiceTaxVM.TaxAmount,
                                        TaxAutoAmount = invoiceTaxVM.TaxAutoAmount,
                                        PurchaseLCId = invoiceTaxVM.PurchaseLCId,
                                        TaxYearId = voucher.TaxYearId,
                                        VoucherId = voucher.Id,
                                        TaxYearPeriodId = voucher.TaxYearPeriodId,
                                        AddedBy = voucher.AddedBy,
                                        AddedDate = voucher.AddedDate,
                                        AddedFromIP = voucher.AddedFromIP
                                    };
                                    InsertInvoiceTax(voucherVM, invoiceTax, invoiceTaxPk);
                                    totalTaxAmount += invoiceTax.TaxAmount;

                                    if (!string.IsNullOrEmpty(taxCategoryGL["ExpensesGLId"].ToString()))
                                    {
                                        var invoiceTaxDetail = new InvoiceTaxDetail
                                        {
                                            GLGeneralInfoId = taxCategoryGL["ExpensesGLId"].ToString(),
                                            BudgetMasterId = taxCategoryGL["ExpensesBudgetMasterId"].ToString(),
                                            ActivityId = taxCategoryGL["ExpensesActivityId"].ToString(),
                                            Amount = invoiceTax.TaxAmount,
                                            AType = "Dr"
                                        };
                                        _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                                        var voucherDetailTax = new VoucherDetail
                                        {
                                            GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                            BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                            ActivityId = invoiceTaxDetail.ActivityId,
                                            InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                            DrAmount = invoiceTaxDetail.Amount,
                                        };
                                        currentVoucherDetailId++;
                                        _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                                        totalDrAmount += voucherDetailTax.DrAmount;
                                        var voucherDetailCurrencyTax = new VoucherDetailCurrency
                                        {
                                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                            ToCurrencyId = companyCurrencyId,
                                            ParallelCurrencyId = companyCurrencyId,
                                            FromCurrencyId = companyCurrencyId,
                                            DrAmount = voucherDetailTax.DrAmount,//voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                            ToCurrencyConversion = 1 //ToDo:
                                        };
                                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                                    }
                                }
                            }
                        }


                        var obBankData = chargesList.Where(r => r.OpeningBankMasterId == item.OpeningBankMasterId).FirstOrDefault();
                        currentVoucherDetailId++;
                        var voucherDetailCr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            GLGeneralInfoId = obBankData.GLGeneralInfoId,
                            BudgetMasterId = obBankData.BudgetMasterId,
                            ActivityId = obBankData.ActivityId,
                            CrAmount = chargesList.Where(r => r.OpeningBankMasterId == item.OpeningBankMasterId).Sum(r => r.Amount) + totalTaxAmount,
                            BankMasterId = obBankData.OpeningBankMasterId

                        }, currentVoucherDetailId);
                        totalCrAmount += voucherDetailCr.CrAmount;
                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.ToCurrencyRate,
                            ToCurrencyConversion = voucherVM.ToCurrencyRate,
                            CrAmount = voucherDetailCurrencyCr + totalTaxAmount
                        });

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailCr.PaymentSource,
                            BankMasterId = obBankData.OpeningBankMasterId,
                            CashMasterId = voucherVM.CashMasterId,
                            CrAmount = chargesList.Where(r => r.OpeningBankMasterId == item.OpeningBankMasterId).Sum(r => r.BankAmount) + totalTaxAmount
                        };
                        _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);
                        voucherDetailCurrencyCr = 0;
                    }
                    if (totalCrAmount != totalDrAmount)
                        throw new CustomException("Dr Cr Amount not match !.");
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #region Service Payable
        public void InsertServicePayable(string serviceAcknowledgementMasterId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> serviceDetailGLList
            , IEnumerable<ServiceAcknowledgementDetailViewModel> serviceAcknowledgementDetailVMList, IEnumerable<InvoiceTaxViewModel> tdsTaxList)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id
                if (voucherVM.IsInvoice == true)
                {

                    var serviceAcknowedgeData = _serviceAcknowledgementMasterRepository.Find(serviceAcknowledgementMasterId);

                    voucherVM.PostingDate = serviceAcknowedgeData.AcknowledgementDate;
                    AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                    _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                    _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                    _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                   
                    if (serviceAcknowedgeData.Status == "Posting")
                        throw new CustomException("The GRN no '" + serviceAcknowedgeData.Id + "' already Posted!");

                   // var companyParty = _companyPartyRepository.Query(r => r.PartyId == serviceAcknowedgeData.PartyId).Select().FirstOrDefault();

                    // var inventoryReceive=_inventoryReceiveRepository.Find();
                    #endregion Get Company Parallerl Currency Id

                    _unitOfWork.BeginTransaction();
                    flag = true;
                    // INSERT INTO Invoice TABLE



                    voucherVM.CompanyCurrencyRate = voucherVM.ToCurrencyRate;

                    var invoice = new Invoice
                    {
                        Amount = voucherVM.Amount,
                        BaseNoOfDays = voucherVM.BaseNoOfDays,
                        BaseOnDueDate = voucherVM.BaseOnDueDate,
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        CurrencyId = voucherVM.CurrencyId,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        InvoiceNo = voucherVM.InvoiceNo,
                        Narration = voucherVM.Narration,
                        EntityId = voucherVM.EntityId,
                        PlantId = voucherVM.PlantId,
                        IsExcludingTax = voucherVM.IsExcludingTax,
                        IsSplit = voucherVM.IsSplit,
                        PartyId = voucherVM.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        PartyType = PartyType.Vendor.ToString(),
                        EmployeeId = voucherVM.EmployeeId,
                        PaymentTermId = voucherVM.PaymentTermId,
                        PostingDate = serviceAcknowedgeData.AcknowledgementDate,
                        SourceType = SourceType.ServicePayable.ToString(),
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
                        Narration = invoice.Narration,
                        PostingDate = serviceAcknowedgeData.AcknowledgementDate,
                        SourceType = SourceType.ServicePayable.ToString(),
                        VoucherTypeId = voucherVM.VoucherTypeId,
                    };
                    voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                    _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);
                    serviceAcknowedgeData.Status = "Posting";
                    serviceAcknowedgeData.ModelState = ModelState.Modified;
                    serviceAcknowedgeData.PaymentTermId = voucherVM.PaymentTermId;
                    serviceAcknowedgeData.VoucherId = voucher.Id;

                    AuditService.UpdatedLog(serviceAcknowedgeData);
                    invoice.ServiceAcknowledgementMasterId = serviceAcknowledgementMasterId;
                    _serviceAcknowledgementMasterRepository.Update(serviceAcknowedgeData);

                    //For check Budget is applied in company or not.
                    //var comdata = _companyService.Find(voucher.CompanyId);
                    // Set to Invoice
                    invoice.VoucherId = voucher.Id;

                    var currentInvoiceDetail = 0;
                    var currentVoucherDetaiRecord = 0;
                    var currentTaxRecord = 0;
                    decimal totalAmountDr = 0;
                    decimal totalAmountCr = 0;
                    var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();

                    foreach (var voucherDetailVM in voucherDetailVMList.Where(r => r.Amount > 0))
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                            throw new CustomException("Without Budget can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                            throw new CustomException("Without Activity can not post.");
                        if (voucherDetailVM.TrnType == "Dr")
                        {
                            // in libility side Dr.
                            var voucherDr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                DrAmount = voucherDetailVM.Amount,
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PostingWithoutTaxAllow = invoice.IsExcludingTax
                            };
                            totalAmountDr += voucherDr.DrAmount;
                            voucherDetailVM.Id = voucherDr.Id;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);


                            if (serviceDetailGLList != null)
                            {
                                foreach (var item in serviceDetailGLList.Where(r => r.GLGeneralInfoId == voucherDr.GLGeneralInfoId && r.BudgetMasterId == voucherDr.BudgetMasterId
                                && r.ActivityId == voucherDr.ActivityId && r.OtherName == "Svc"))
                                {
                                    var serviceDetail = _serviceAcknowledgementDetailRepository.Find(item.ServiceAcknowledgementDetailId);
                                    serviceDetail.PostDrGLGeneralInfoId = voucherDr.GLGeneralInfoId;
                                    serviceDetail.PostDrBudgetMasterId = voucherDr.BudgetMasterId;
                                    serviceDetail.PostDrActivityId = voucherDr.ActivityId;
                                    serviceDetail.ModelState = ModelState.Modified;
                                    AuditService.UpdatedLog(serviceDetail);
                                    _serviceAcknowledgementDetailRepository.Update(serviceDetail);
                                }
                            }


                            if (voucherDetailVM.OtherName == "Tax")
                            {
                                currentTaxRecord++;
                                var invoiceTax = new InvoiceTax
                                {
                                    Archive = false,
                                    VoucherDetailId = voucherDr.Id,//voucherDetailDrId,
                                    VoucherId = voucher.Id,
                                    InvoiceId = invoice.Id,
                                    TaxYearId = voucher.TaxYearId,
                                    TaxYearPeriodId = voucher.TaxYearPeriodId,
                                    TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                    TaxCodeId = voucherDetailVM.TaxCodeId,
                                    TaxAmount = voucherDetailVM.Amount,
                                    TaxAutoAmount = 0,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.InventoryPayable.ToString(),
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
                                    BudgetMasterId = voucherDetailVM.BudgetId,
                                    ActivityId = voucherDetailVM.ActivityId,
                                    AType = "Dr",
                                    AddedBy = invoiceTax.AddedBy,
                                    AddedDate = invoiceTax.AddedDate,
                                    AddedFromIP = invoiceTax.AddedFromIP
                                };
                                _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                            }
                            #region Currency

                            foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                            {

                                var voucherDetailCurrencydb = new VoucherDetailCurrency
                                {
                                    ToCurrencyRate = voucherVM.ToCurrencyRate,
                                    ToCurrencyId = companyCurrencyId,
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                    DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                    ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                                };
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                                voucherDetailCurrencydb = null;
                            }

                            #endregion Currency
                        }
                        else if (voucherDetailVM.TrnType == "Cr")
                        {
                            currentInvoiceDetail++;
                            // INSERT INTO InvoiceDetail
                            var invoiceDetail = new InvoiceDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                Amount = voucherDetailVM.Amount,
                                NetAmount = voucherDetailVM.Amount,
                                TaxAmount = 0,
                                AddedBy = invoice.AddedBy,
                                AddedDate = invoice.AddedDate,
                                AddedFromIP = invoice.AddedFromIP,
                                Archive = invoice.Archive,
                                InvoiceId = invoice.Id,
                            };
                            invoice.Amount = invoiceDetail.Amount;
                            // INSERT INTO VoucherDetail
                            var voucherCr = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceDetail.BudgetMasterId,
                                ActivityId = invoiceDetail.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                DrAmount = 0,
                                CrAmount = voucherDetailVM.Amount,
                                DocDate = voucher.DocDate,
                                DocRefNo = voucher.DocRefNo,
                                Narration = invoice.Narration,
                                EmployeeId = invoice.EmployeeId,
                                InvoiceDetailId = invoiceDetail.Id,
                                PartyId = invoice.PartyId,
                                PartyPlantId = invoice.PartyPlantId,
                                PartyType = invoice.PartyType,
                                PostingWithoutTaxAllow = invoice.IsExcludingTax
                            };
                            voucherDetailVM.Id = voucherCr.Id;
                            totalAmountCr += voucherCr.CrAmount;

                            if (voucherDetailVM.OtherName == "Tax")
                            {
                                currentTaxRecord++;
                                var invoiceTax = new InvoiceTax
                                {
                                    Archive = false,
                                    VoucherDetailId = voucherCr.Id,//voucherDetailCrrId,
                                    InvoiceId = invoice.Id,
                                    TaxYearId = voucher.TaxYearId,
                                    TaxYearPeriodId = voucher.TaxYearPeriodId,
                                    TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                    TaxAmount = voucherDetailVM.Amount,
                                    TaxAutoAmount = 0,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.InventoryPayable.ToString(),
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
                                    BudgetMasterId = voucherDetailVM.BudgetId,
                                    ActivityId = voucherDetailVM.ActivityId,
                                    AType = "Cr",
                                    Archive = false,
                                    ModelState = ModelState.Added,
                                    AddedBy = voucher.AddedBy,
                                    AddedDate = voucher.AddedDate,
                                    AddedFromIP = voucher.AddedFromIP
                                };
                                _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                            }

                           _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail);
                            voucherCr.InvoiceDetailId = invoiceDetail.Id;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);
                            var svcAcknowledgeDetails = _serviceAcknowledgementDetailRepository.Query(r => r.ServiceAcknowledgementMasterId == serviceAcknowledgementMasterId).Select().ToList();
                            foreach (var item in svcAcknowledgeDetails.Where(r => r.ServiceAcknowledgementMasterId == serviceAcknowledgementMasterId))
                            {
                                item.PostCrGLGeneralInfoId = voucherCr.GLGeneralInfoId;
                                item.PostCrBudgetMasterId = voucherCr.BudgetMasterId;
                                item.PostCrActivityId = voucherCr.ActivityId;
                                item.ModelState = ModelState.Modified;
                                AuditService.UpdatedLog(item);
                                _serviceAcknowledgementDetailRepository.Update(item);

                            }

                            #region Currency

                            foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                            {

                                var voucherDetailCurrencydb = new VoucherDetailCurrency
                                {
                                    ToCurrencyRate = voucherVM.ToCurrencyRate,
                                    ToCurrencyId = companyCurrencyId,
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                    CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                                    ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                                };
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);
                                voucherDetailCurrencydb = null;
                            }

                            #endregion Currency
                        }
                    }

                    // Update Inventory Received


                    if (totalAmountDr != totalAmountCr)
                        throw new CustomException("Dr and Cr amount is not equal.");

                    if (null != tdsTaxList && tdsTaxList.Count() > 0)
                    {
                        var tdsTax = new AdditionalTax
                        {

                            TaxYearId = voucher.TaxYearId,
                            TaxYearPeriodId = voucher.TaxYearPeriodId,
                            TaxAmount = tdsTaxList.Sum(r => r.TaxAmount),
                            TaxAutoAmount = tdsTaxList.Sum(r => r.TaxAutoAmount),
                            InventoryReceiveId = null,
                            ServiceAcknowledgementMasterId = invoice.ServiceAcknowledgementMasterId,
                            PartyId = voucherVM.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            InvoiceId = invoice.Id,
                            Id = _invoiceService.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _additionalTaxRepository.Insert(tdsTax);

                        int addtionalTaxDetailId = 0;
                        foreach (var tdsTaxVM in tdsTaxList)
                        {

                            if (null == tdsTaxVM.TaxCodeId)
                                throw new CustomException("Tax code not found!");

                            var taxCodeGL = _accountsCommonService.GetTaxCodeGL(tdsTaxVM.TaxCodeId);
                            if (null == taxCodeGL)
                                throw new CustomException("Tax code GL not found!");


                            addtionalTaxDetailId++;
                            var tdsTaxDetail = new AdditionalTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                                BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                                ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                                Amount = tdsTaxVM.TaxAmount,
                                AdditionalTaxId = tdsTax.Id,
                                TaxCodeId = tdsTaxVM.TaxCodeId,
                                TaxCategoryId = tdsTaxVM.TaxCategoryId,
                                AType = "Cr",
                                Id =_pkGeneratorService.MakePK(tdsTax.Id, addtionalTaxDetailId, 3),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _additionalTaxDetailRepository.Insert(tdsTaxDetail);


                        }
                    }
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                else
                {
                    InsertServicePayableWithOutInvoice(serviceAcknowledgementMasterId, voucherVM, voucherDetailVMList, voucherDetailCurrencyVMList
                        , serviceDetailGLList, serviceAcknowledgementDetailVMList, tdsTaxList);
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


        private void InsertServicePayableWithOutInvoice(string serviceAcknowledgementMasterId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> serviceDetailGLList
           , IEnumerable<ServiceAcknowledgementDetailViewModel> serviceAcknowledgementDetailVMList, IEnumerable<InvoiceTaxViewModel> tdsTaxList)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id

                var serviceAcknowedgeData = _serviceAcknowledgementMasterRepository.Find(serviceAcknowledgementMasterId);
                voucherVM.PostingDate = serviceAcknowedgeData.AcknowledgementDate;
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                if (serviceAcknowedgeData.Status == "Posting")
                    throw new CustomException("The GRN no '" + serviceAcknowedgeData.Id + "' already Posted!");

                //var companyParty = _companyPartyRepository.Query(r => r.PartyId == serviceAcknowedgeData.PartyId).Select().FirstOrDefault();

                // var inventoryReceive=_inventoryReceiveRepository.Find();
                #endregion Get Company Parallerl Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Invoice TABLE



                voucherVM.CompanyCurrencyRate = voucherVM.ToCurrencyRate;

                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = voucherVM.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    AddedBy = voucherVM.AddedBy,
                    AddedDate = voucherVM.AddedDate,
                    AddedFromIP = voucherVM.AddedFromIP,
                    VoucherDate = voucherVM.VoucherDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    // Archive = voucherVM.Archive,
                    IsPark = voucherVM.IsPark,
                    Narration = voucherVM.Narration,
                    PostingDate = serviceAcknowedgeData.AcknowledgementDate,
                    SourceType = SourceType.ServicePayable.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };
                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);
                voucherVM.AddedBy = voucher.AddedBy;
                voucherVM.AddedDate = voucher.AddedDate;
                voucherVM.AddedFromIP = voucher.AddedFromIP;

                serviceAcknowedgeData.Status = "Posting";
                serviceAcknowedgeData.ModelState = ModelState.Modified;
                serviceAcknowedgeData.PaymentTermId = voucherVM.PaymentTermId;
                serviceAcknowedgeData.VoucherId = voucher.Id;

                AuditService.UpdatedLog(serviceAcknowedgeData);
                _serviceAcknowledgementMasterRepository.Update(serviceAcknowedgeData);

                //For check Budget is applied in company or not.
                //var comdata = _companyService.Find(voucher.CompanyId);
                // Set to Invoice

                var currentInvoiceDetail = 0;
                var currentVoucherDetaiRecord = 0;
                var currentTaxRecord = 0;
                decimal totalAmountDr = 0;
                decimal totalAmountCr = 0;
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();

                foreach (var voucherDetailVM in voucherDetailVMList.Where(r => r.Amount > 0))
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                        throw new CustomException("Without Budget can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                        throw new CustomException("Without Activity can not post.");
                    if (voucherDetailVM.TrnType == "Dr")
                    {
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                        };
                        totalAmountDr += voucherDr.DrAmount;
                        voucherDetailVM.Id = voucherDr.Id;
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);


                        if (serviceDetailGLList != null)
                        {
                            foreach (var item in serviceDetailGLList.Where(r => r.GLGeneralInfoId == voucherDr.GLGeneralInfoId && r.BudgetMasterId == voucherDr.BudgetMasterId
                            && r.ActivityId == voucherDr.ActivityId && r.OtherName == "Svc"))
                            {
                                var serviceDetail = _serviceAcknowledgementDetailRepository.Find(item.ServiceAcknowledgementDetailId);
                                serviceDetail.PostDrGLGeneralInfoId = voucherDr.GLGeneralInfoId;
                                serviceDetail.PostDrBudgetMasterId = voucherDr.BudgetMasterId;
                                serviceDetail.PostDrActivityId = voucherDr.ActivityId;
                                serviceDetail.ModelState = ModelState.Modified;
                                AuditService.UpdatedLog(serviceDetail);
                                _serviceAcknowledgementDetailRepository.Update(serviceDetail);
                            }
                        }

                        if (voucherDetailVM.OtherName == "Tax")
                        {
                            currentTaxRecord++;
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                VoucherDetailId = voucherDr.Id,//voucherDetailDrId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxCodeId = voucherDetailVM.TaxCodeId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.InventoryPayable.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            voucherVM.VoucherId = voucher.Id;
                            _invoiceTaxService.InsertInvoiceTax(voucherVM, invoiceTax, invoiceTaxPk);
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                Id = invoiceTax.Id + 1,
                                InvoiceTaxId = invoiceTax.Id,
                                Amount = invoiceTax.TaxAmount,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetId,
                                ActivityId = voucherDetailVM.ActivityId,
                                AType = "Dr",
                                AddedBy = invoiceTax.AddedBy,
                                AddedDate = invoiceTax.AddedDate,
                                AddedFromIP = invoiceTax.AddedFromIP
                            };
                            _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                        }
                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {

                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                    else if (voucherDetailVM.TrnType == "Cr")
                    {

                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = voucher.Narration,
                            //PartyId = voucherVM.PartyId,
                            //PartyPlantId = voucherVM.PartyPlantId,
                            //PartyType = voucherVM.PartyType,
                        };
                        voucherDetailVM.Id = voucherCr.Id;
                        totalAmountCr += voucherCr.CrAmount;


                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);
                        var svcAcknowledgeDetails = _serviceAcknowledgementDetailRepository.Query(r => r.ServiceAcknowledgementMasterId == serviceAcknowledgementMasterId).Select().ToList();
                        foreach (var item in svcAcknowledgeDetails.Where(r => r.ServiceAcknowledgementMasterId == serviceAcknowledgementMasterId))
                        {
                            item.PostCrGLGeneralInfoId = voucherCr.GLGeneralInfoId;
                            item.PostCrBudgetMasterId = voucherCr.BudgetMasterId;
                            item.PostCrActivityId = voucherCr.ActivityId;
                            item.ModelState = ModelState.Modified;
                            AuditService.UpdatedLog(item);
                            _serviceAcknowledgementDetailRepository.Update(item);

                        }
                        if (voucherDetailVM.OtherName == "Tax")
                        {
                            currentTaxRecord++;
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                VoucherDetailId = voucherCr.Id,//voucherDetailCrrId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.InventoryPayable.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            voucherVM.VoucherId = voucher.Id;
                            _invoiceTaxService.InsertInvoiceTax(voucherVM, invoiceTax, invoiceTaxPk);
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                Id = invoiceTax.Id + 1,
                                InvoiceTaxId = invoiceTax.Id,
                                Amount = invoiceTax.TaxAmount,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetId,
                                ActivityId = voucherDetailVM.ActivityId,
                                AType = "Cr",
                                Archive = false,
                                ModelState = ModelState.Added,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                        }

                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {

                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                }

                // Update Inventory Received


                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                if (null != tdsTaxList && tdsTaxList.Count() > 0)
                {
                    var tdsTax = new AdditionalTax
                    {

                        TaxYearId = voucher.TaxYearId,
                        TaxYearPeriodId = voucher.TaxYearPeriodId,
                        TaxAmount = tdsTaxList.Sum(r => r.TaxAmount),
                        TaxAutoAmount = tdsTaxList.Sum(r => r.TaxAutoAmount),
                        InventoryReceiveId = null,
                        ServiceAcknowledgementMasterId = serviceAcknowedgeData.Id,
                        PartyId = voucherVM.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        //InvoiceId = invoice.Id,
                        Id = _invoiceService.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP
                    };
                    _additionalTaxRepository.Insert(tdsTax);

                    int addtionalTaxDetailId = 0;
                    foreach (var tdsTaxVM in tdsTaxList)
                    {

                        if (null == tdsTaxVM.TaxCodeId)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _accountsCommonService.GetTaxCodeGL(tdsTaxVM.TaxCodeId);
                        if (null == taxCodeGL)
                            throw new CustomException("Tax code GL not found!");


                        addtionalTaxDetailId++;
                        var tdsTaxDetail = new AdditionalTaxDetail
                        {
                            GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                            BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                            ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                            Amount = tdsTaxVM.TaxAmount,
                            AdditionalTaxId = tdsTax.Id,
                            TaxCodeId = tdsTaxVM.TaxCodeId,
                            TaxCategoryId = tdsTaxVM.TaxCategoryId,
                            AType = "Cr",
                            Id =_pkGeneratorService.MakePK(tdsTax.Id, addtionalTaxDetailId, 3),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _additionalTaxDetailRepository.Insert(tdsTaxDetail);


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

        public void DeleteTDSPostServicePayable(string invoiceWriteOffId, string voucherId, string serviceAckId)
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
                var invoiceWriteOff = _invoiceWriteOffService.Find(invoiceWriteOffId);
                var invoiceWriteOffDetail = _invoiceWriteOffService.QueryInvoiceWriteOffDetail(invoiceWriteOffId).Select().ToList();
                var invoiceTax = _invoiceTaxRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherService.DeleteVoucherDetailCurrency(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    var glTransactionDetail = _voucherService.QueryGLTransactionDetail(item.Id).Select().FirstOrDefault();
                    if (glTransactionDetail != null)
                    {
                        _voucherService.DeleteGLTransactionDetail(item.Id);
                    }
                    _voucherService.DeleteVoucherDetail(item.Id);
                }
                if (invoiceTax != null)
                {
                    foreach (var item in invoiceTax)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = @"UPDATE [TRN].InvoiceTax SET VoucherDetailId=NULL WHERE Id='" + item.Id + "'";
                        rdBuilder.Append(builderSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                        var invoicetaxDdetail = _invoiceTaxDetailRepository.Query(r => r.InvoiceTaxId == item.Id).Select().ToList();
                        foreach (var item1 in invoicetaxDdetail)
                        {
                            _invoiceTaxDetailRepository.Delete(item1.Id);
                        }
                        _invoiceTaxRepository.Delete(item.Id);
                    }
                }
                
                foreach (var item in invoiceWriteOffDetail)
                {

                    var invoice = _invoiceService.Find(item.InvoiceId);
                    var invoiceDetail = _invoiceService.FindInvoiceDetail(item.InvoiceDetailId);
                    invoiceDetail.WrittenOffAmount -= item.Amount;
                    invoice.WrittenOffAmount -= item.Amount;
                    invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                    invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;

                    _invoiceService.UpdateInvoiceDetail(invoiceDetail);
                    _invoiceService.Update(invoice);
                    _invoiceWriteOffService.DeleteInvoiceWriteOffDetail(item.Id);
                }
                _invoiceWriteOffService.Delete(invoiceWriteOffId);

                var rdBuilderAT = new System.Text.StringBuilder();
                var builderSqlAT = @"UPDATE [TRN].AdditionalTax SET VoucherId=NULL WHERE ServiceAcknowledgementMasterId='" + serviceAckId + "'";
                rdBuilderAT.Append(builderSqlAT);
                _sqlRepository.ExecuteSqlCommand(rdBuilderAT.ToString());

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

        public void DeleteTDSServicePayable(string additionalTaxId,string voucherId)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                
                if (voucherId != null)
                    throw new CustomException("Delete is not allow after post ! ");

                var rdBuilderAT = new System.Text.StringBuilder();
                var builderSqlDetail = @"Delete [TRN].AdditionalTaxDetail  WHERE AdditionalTaxId='" + additionalTaxId + "'";
                var builderSql = @"Delete [TRN].AdditionalTax  WHERE Id='" + additionalTaxId + "'";
                rdBuilderAT.Append(builderSqlDetail);
                rdBuilderAT.Append(builderSql);
                _sqlRepository.ExecuteSqlCommand(rdBuilderAT.ToString());
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

        #endregion

        #region InventorySalesPosting
        public void PostSingleJournalSales(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
            , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, OtherInvoice otherInvoiceVM)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id

                var receiveData = _inventorySalesRepository.Find(receiveId);
                voucherVM.PostingDate = receiveData.SalesDate;
                voucherVM.DocDate = receiveData.DocDate;
                voucherVM.DocRefNo = receiveData.DocRefNo;
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                if (receiveData.Status == "Posting")
                    throw new CustomException("The Sales no '" + receiveData.Id + "' already Posted!");

                //var companyParty = _companyPartyRepository.Query(r => r.PartyId == receiveData.CustomerId).Select().FirstOrDefault();

                // var inventoryReceive=_inventoryReceiveRepository.Find();
                #endregion Get Company Parallerl Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Invoice TABLE

                var invoice = new Invoice
                {
                    Amount = voucherVM.Amount,
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
                    PostingDate = receiveData.SalesDate,
                    SourceType = SourceType.SalesInvoice.ToString(),

                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    CompanyCurrencyRate = voucherVM.ToCurrencyRate
                };
                if (acceptanceId != null)
                {
                    invoice.BaseNoOfDays = 0;
                    invoice.BaseOnDueDate = null;
                    invoice.RevisedDueDate = null;
                    invoice.ActualDueDate = null;
                }
                else
                {
                    invoice.BaseNoOfDays = voucherVM.BaseNoOfDays;
                    invoice.BaseOnDueDate = voucherVM.BaseOnDueDate;
                    invoice.RevisedDueDate = voucherVM.MatureDate;
                    invoice.ActualDueDate = voucherVM.MatureDate;
                }
                _invoiceService.InsertInvoice(invoice);

                receiveData.Status = "Posting";
                receiveData.ModelState = ModelState.Modified;
                AuditService.UpdatedLog(receiveData);
                invoice.InventorySalesId = receiveId;
                _inventorySalesRepository.Update(receiveData);

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
                    VoucherDate = invoice.VoucherDate,
                    DocDate = invoice.DocDate,
                    DocRefNo = invoice.DocRefNo,
                    Archive = invoice.Archive,
                    IsPark = invoice.IsPark,
                    Narration = invoice.Narration,
                    PostingDate = receiveData.SalesDate,
                    SourceType = SourceType.SalesInvoice.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };

                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                //For check Budget is applied in company or not.
                // var comdata = _companyService.Find(voucher.CompanyId);
                // Set to Invoice
                invoice.VoucherId = voucher.Id;

                var currentInvoiceDetail = 0;
                var currentVoucherDetaiRecord = 0;
                var currentTaxRecord = 0;
                decimal totalAmountDr = 0;
                decimal totalAmountCr = 0;
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();

                foreach (var voucherDetailVM in voucherDetailVMList.Where(r => r.Amount > 0))
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                        throw new CustomException("Without Budget can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                        throw new CustomException("Without Activity can not post.");
                    if (voucherDetailVM.TrnType == "Cr")
                    {
                        // in libility side Dr.
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CrAmount = voucherDetailVM.Amount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax
                        };
                        totalAmountCr += voucherCr.CrAmount;
                        voucherDetailVM.Id = voucherCr.Id;
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);

                        foreach (var item in inventoryReceiveDetailVMList.Where(r => r.GLGeneralInfoId == voucherCr.GLGeneralInfoId
                        && r.BudgetMasterId == voucherCr.BudgetMasterId && r.ActivityId == voucherCr.ActivityId && r.TrnType == "Cr"))
                        {
                            var inventorySalesDetail = _inventorySalesDetailRepository.Find(item.InventorySalesDetailId);
                            inventorySalesDetail.PostCrGLGeneralInfoId = voucherCr.GLGeneralInfoId;
                            inventorySalesDetail.PostCrBudgetMasterId = voucherCr.BudgetMasterId;
                            inventorySalesDetail.PostCrActivityId = voucherCr.ActivityId;
                            inventorySalesDetail.ModelState = ModelState.Modified;
                            AuditService.UpdatedLog(inventorySalesDetail);
                            _inventorySalesDetailRepository.Update(inventorySalesDetail);
                        }

                        if (voucherDetailVM.OtherName == "Tax")
                        {
                            var voucherDetailDrId = voucherDetailVMList.FirstOrDefault(t => t.TrnType == "Dr" && t.OtherName == "Tax" && t.MaterialGroupMasterId == voucherDetailVM.MaterialGroupMasterId).Id;
                            currentTaxRecord++;
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                Id =_pkGeneratorService.MakePK(invoice.Id, currentTaxRecord, 2),
                                VoucherDetailId = voucherCr.Id,//voucherDetailDrId,
                                VoucherId = voucher.Id,
                                InvoiceId = invoice.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.InventoryPayable.ToString(),
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
                                BudgetMasterId = voucherDetailVM.BudgetId,
                                ActivityId = voucherDetailVM.ActivityId,
                                AType = "Dr",
                                AddedBy = invoiceTax.AddedBy,
                                AddedDate = invoiceTax.AddedDate,
                                AddedFromIP = invoiceTax.AddedFromIP
                            };
                            _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                        }
                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                            CurrencyExchange(voucherVM.CurrencyId, companyCurrencyId, null, null,
                               voucherDetailCurrency.CompanyCurrencyCr, voucherDetailCurrency.CompanyGroupCurrencyCr,
                               voucherDetailCurrency.HardCurrencyCr, voucherDetailCurrency, voucherCr.DrAmount);

                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                    else if (voucherDetailVM.TrnType == "Dr")
                    {
                        currentInvoiceDetail++;
                        // INSERT INTO InvoiceDetail
                        var invoiceDetail = new InvoiceDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            MaterialGroupMasterId = voucherDetailVM.MaterialGroupMasterId,
                            Amount = voucherDetailVM.Amount,
                            NetAmount = voucherDetailVM.Amount,
                            TaxAmount = 0,
                            AddedBy = invoice.AddedBy,
                            AddedDate = invoice.AddedDate,
                            AddedFromIP = invoice.AddedFromIP,
                            Archive = invoice.Archive,
                            InvoiceId = invoice.Id,
                        };
                        invoice.Amount = invoiceDetail.Amount;
                        // INSERT INTO VoucherDetail
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceDetail.BudgetMasterId,
                            ActivityId = invoiceDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            CrAmount = 0,
                            DrAmount = voucherDetailVM.Amount,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = invoice.Narration,
                            EmployeeId = invoice.EmployeeId,
                            InvoiceDetailId = invoiceDetail.Id,
                            PartyId = invoice.PartyId,
                            PartyPlantId = invoice.PartyPlantId,
                            PartyType = invoice.PartyType,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax
                        };
                        voucherDetailVM.Id = voucherDr.Id;
                        totalAmountDr += voucherDr.DrAmount;
                        foreach (var item in inventoryReceiveDetailVMList.Where(r => r.GLGeneralInfoId == voucherDr.GLGeneralInfoId
                       && r.BudgetMasterId == voucherDr.BudgetMasterId && r.ActivityId == voucherDr.ActivityId && r.TrnType == "Dr"))
                        {
                            var inventorySalesDetail = _inventorySalesDetailRepository.Find(item.InventorySalesDetailId);
                            inventorySalesDetail.PostDrGLGeneralInfoId = voucherDr.GLGeneralInfoId;
                            inventorySalesDetail.PostDrBudgetMasterId = voucherDr.BudgetMasterId;
                            inventorySalesDetail.PostDrActivityId = voucherDr.ActivityId;
                            inventorySalesDetail.ModelState = ModelState.Modified;
                            AuditService.UpdatedLog(inventorySalesDetail);
                            _inventorySalesDetailRepository.Update(inventorySalesDetail);
                        }
                        if (voucherDetailVM.OtherName == "Tax")
                        {
                            var voucherDetailCrrId = voucherDetailVMList.FirstOrDefault(t => t.TrnType == "Cr" && t.OtherName == "Tax" && t.MaterialGroupMasterId == voucherDetailVM.MaterialGroupMasterId).Id;
                            currentTaxRecord++;
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                Id =_pkGeneratorService.MakePK(invoice.Id, currentTaxRecord, 2),
                                VoucherDetailId = voucherDr.Id,//voucherDetailCrrId,
                                InvoiceId = invoice.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.SalesInvoice.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxRepository.Insert(invoiceTax);
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                Id = invoiceTax.Id + 1,
                                InvoiceTaxId = invoiceTax.Id,
                                Amount = invoiceTax.TaxAmount,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetId,
                                ActivityId = voucherDetailVM.ActivityId,
                                AType = "Cr",
                                Archive = false,
                                ModelState = ModelState.Added,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                        }

                        //_invoiceDetailRepository.Insert(invoiceDetail);
                      _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail);
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                            CurrencyExchange(voucherVM.CurrencyId, companyCurrencyId, null, null,
                               voucherDetailCurrency.CompanyCurrencyCr, voucherDetailCurrency.CompanyGroupCurrencyCr,
                               voucherDetailCurrency.HardCurrencyCr, voucherDetailCurrency, voucherDr.DrAmount);

                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                }

                // Update Inventory Received


                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                if (otherInvoiceVM.PartyId != null && otherInvoiceVM.GLGeneralInfoId != null)
                {
                    var otherInvoice = new OtherInvoice
                    {

                        Amount = otherInvoiceVM.Amount,
                        PartyId = otherInvoiceVM.PartyId,
                        PartyPlantId = otherInvoiceVM.PartyPlantId,
                        InvoiceId = invoice.Id,
                        GLGeneralInfoId = otherInvoiceVM.GLGeneralInfoId,
                        BudgetMasterId = otherInvoiceVM.BudgetMasterId,
                        ActivityId = otherInvoiceVM.ActivityId,
                        SourceType = invoice.SourceType,
                        IsPark = true,
                        Id = _invoiceService.GetAutoNumber(nameof(OtherInvoice), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP
                    };
                    _otherInvoiceRepository.Insert(otherInvoice);
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

        public void PostMultipleJournalSales(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
            , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<VoucherDetailViewModel> inventoryJVList, OtherInvoice otherInvoiceVM)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id
                var receiveData = _inventorySalesRepository.Find(receiveId);
                voucherVM.PostingDate = receiveData.SalesDate;
                voucherVM.DocDate = receiveData.DocDate;
                voucherVM.DocRefNo = receiveData.DocRefNo;
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
               
                if (receiveData.Status == "Posting")
                    throw new CustomException("The Sales no '" + receiveData.Id + "' already Posted!");

                //var companyParty = _companyPartyRepository.Query(r => r.PartyId == receiveData.CustomerId).Select().FirstOrDefault();

                // var inventoryReceive=_inventoryReceiveRepository.Find();
                #endregion Get Company Parallerl Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Invoice TABLE
                voucherVM.Narration = "Sales Journal";
                var invoice = new Invoice
                {
                    Amount = voucherVM.Amount,
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
                    PostingDate = receiveData.SalesDate,
                    SourceType = SourceType.SalesInvoice.ToString(),

                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    CompanyCurrencyRate = voucherVM.ToCurrencyRate
                };
                if (acceptanceId != null)
                {
                    invoice.BaseNoOfDays = 0;
                    invoice.BaseOnDueDate = null;
                    invoice.RevisedDueDate = null;
                    invoice.ActualDueDate = null;
                }
                else
                {
                    invoice.BaseNoOfDays = voucherVM.BaseNoOfDays;
                    invoice.BaseOnDueDate = voucherVM.BaseOnDueDate;
                    invoice.RevisedDueDate = voucherVM.MatureDate;
                    invoice.ActualDueDate = voucherVM.MatureDate;
                }
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
                    VoucherDate = invoice.VoucherDate,
                    DocDate = invoice.DocDate,
                    DocRefNo = invoice.DocRefNo,
                    Archive = invoice.Archive,
                    IsPark = invoice.IsPark,
                    Narration = invoice.Narration,
                    PostingDate = receiveData.SalesDate,
                    SourceType = SourceType.SalesInvoice.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };

                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                receiveData.Status = "Posting";
                receiveData.ModelState = ModelState.Modified;
                AuditService.UpdatedLog(receiveData);
                invoice.InventorySalesId = receiveId;
                receiveData.VoucherId = voucher.Id;
                _inventorySalesRepository.Update(receiveData);
              
                invoice.VoucherId = voucher.Id;

                var currentInvoiceDetail = 0;
                var currentVoucherDetaiRecord = 0;
                var currentTaxRecord = 0;
                decimal totalAmountDr = 0;
                decimal totalAmountCr = 0;
                foreach (var voucherDetailVM in voucherDetailVMList.Where(r => r.Amount > 0))
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                        throw new CustomException("Without Budget can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                        throw new CustomException("Without Activity can not post.");
                    if (voucherDetailVM.TrnType == "Cr")
                    {
                        // in libility side Dr.
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CrAmount = voucherDetailVM.Amount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax
                        };
                        totalAmountCr += voucherCr.CrAmount;
                        voucherDetailVM.Id = voucherCr.Id;
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);

                        foreach (var item in inventoryReceiveDetailVMList.Where(r => r.GLGeneralInfoId == voucherCr.GLGeneralInfoId
                        && r.BudgetMasterId == voucherCr.BudgetMasterId && r.ActivityId == voucherCr.ActivityId && r.TrnType == "Cr"))
                        {
                            var inventorySalesDetail = _inventorySalesDetailRepository.Find(item.InventorySalesDetailId);
                            var CrGLBAct = inventoryReceiveDetailVMList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                            inventorySalesDetail.PostCrGLGeneralInfoId = voucherCr.GLGeneralInfoId;
                            inventorySalesDetail.PostCrBudgetMasterId = voucherCr.BudgetMasterId;
                            inventorySalesDetail.PostCrActivityId = voucherCr.ActivityId;
                            inventorySalesDetail.ModelState = ModelState.Modified;
                            AuditService.UpdatedLog(inventorySalesDetail);
                            _inventorySalesDetailRepository.Update(inventorySalesDetail);
                        }

                        if (voucherDetailVM.OtherName == "Tax")
                        {
                            // var voucherDetailDrId = voucherDetailVMList.FirstOrDefault(t => t.TrnType == "Cr" && t.OtherName == "Tax" ).Id;
                            currentTaxRecord++;
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                Id =_pkGeneratorService.MakePK(invoice.Id, currentTaxRecord, 2),
                                VoucherDetailId = voucherCr.Id,//voucherDetailDrId,
                                VoucherId = voucher.Id,
                                InvoiceId = invoice.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.SalesInvoice.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxRepository.Insert(invoiceTax);
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                Id = invoiceTax.Id + 1,
                                InvoiceTaxId = invoiceTax.Id,
                                Amount = invoiceTax.TaxAmount,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetId,
                                ActivityId = voucherDetailVM.ActivityId,
                                AType = "Cr",
                                AddedBy = invoiceTax.AddedBy,
                                AddedDate = invoiceTax.AddedDate,
                                AddedFromIP = invoiceTax.AddedFromIP
                            };
                            _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                        }
                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                            CurrencyExchange(voucherVM.CurrencyId, companyCurrencyId, null, null,
                               voucherDetailCurrency.CompanyCurrencyCr, voucherDetailCurrency.CompanyGroupCurrencyCr,
                               voucherDetailCurrency.HardCurrencyCr, voucherDetailCurrency, voucherCr.DrAmount);

                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                    else if (voucherDetailVM.TrnType == "Dr")
                    {
                        currentInvoiceDetail++;
                        // INSERT INTO InvoiceDetail
                        var invoiceDetail = new InvoiceDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            MaterialGroupMasterId = voucherDetailVM.MaterialGroupMasterId,
                            Amount = voucherDetailVM.Amount,
                            NetAmount = voucherDetailVM.Amount,
                            TaxAmount = 0,
                            AddedBy = invoice.AddedBy,
                            AddedDate = invoice.AddedDate,
                            AddedFromIP = invoice.AddedFromIP,
                            Archive = invoice.Archive,
                            InvoiceId = invoice.Id,
                        };
                        invoice.Amount = invoiceDetail.Amount;
                        // INSERT INTO VoucherDetail
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceDetail.BudgetMasterId,
                            ActivityId = invoiceDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            CrAmount = 0,
                            DrAmount = voucherDetailVM.Amount,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = invoice.Narration,
                            EmployeeId = invoice.EmployeeId,
                            InvoiceDetailId = invoiceDetail.Id,
                            PartyId = invoice.PartyId,
                            PartyPlantId = invoice.PartyPlantId,
                            PartyType = invoice.PartyType,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax
                        };
                        voucherDetailVM.Id = voucherDr.Id;
                        totalAmountDr += voucherDr.DrAmount;
                        foreach (var item in inventoryReceiveDetailVMList.Where(r => r.GLGeneralInfoId == voucherDr.GLGeneralInfoId
                       && r.BudgetMasterId == voucherDr.BudgetMasterId && r.ActivityId == voucherDr.ActivityId && r.TrnType == "Dr"))
                        {
                            var inventorySalesDetail = _inventorySalesDetailRepository.Find(item.InventorySalesDetailId);
                            inventorySalesDetail.PostDrGLGeneralInfoId = voucherDr.GLGeneralInfoId;
                            inventorySalesDetail.PostDrBudgetMasterId = voucherDr.BudgetMasterId;
                            inventorySalesDetail.PostDrActivityId = voucherDr.ActivityId;
                            inventorySalesDetail.ModelState = ModelState.Modified;
                            AuditService.UpdatedLog(inventorySalesDetail);
                            _inventorySalesDetailRepository.Update(inventorySalesDetail);
                        }
                        if (voucherDetailVM.OtherName == "Tax")
                        {
                            // var voucherDetailCrrId = voucherDetailVMList.FirstOrDefault(t => t.TrnType == "Cr" && t.OtherName == "Tax").Id;
                            currentTaxRecord++;
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                Id =_pkGeneratorService.MakePK(invoice.Id, currentTaxRecord, 2),
                                VoucherDetailId = voucherDr.Id,//voucherDetailCrrId,
                                InvoiceId = invoice.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.SalesInvoice.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxRepository.Insert(invoiceTax);
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                Id = invoiceTax.Id + 1,
                                InvoiceTaxId = invoiceTax.Id,
                                Amount = invoiceTax.TaxAmount,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetId,
                                ActivityId = voucherDetailVM.ActivityId,
                                AType = "Dr",
                                Archive = false,
                                ModelState = ModelState.Added,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                        }

                        //_invoiceDetailRepository.Insert(invoiceDetail);
                       _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail);
                        currentVoucherDetaiRecord++;
                        voucherDr.InvoiceDetailId = invoiceDetail.Id;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                            CurrencyExchange(voucherVM.CurrencyId, companyCurrencyId, null, null,
                               voucherDetailCurrency.CompanyCurrencyCr, voucherDetailCurrency.CompanyGroupCurrencyCr,
                               voucherDetailCurrency.HardCurrencyCr, voucherDetailCurrency, voucherDr.DrAmount);

                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                }

                // Update Inventory Received


                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                if (otherInvoiceVM.PartyId != null && otherInvoiceVM.GLGeneralInfoId != null)
                {
                    var otherInvoice = new OtherInvoice
                    {

                        Amount = otherInvoiceVM.Amount,
                        PartyId = otherInvoiceVM.PartyId,
                        PartyPlantId = otherInvoiceVM.PartyPlantId,
                        InvoiceId = invoice.Id,
                        GLGeneralInfoId = otherInvoiceVM.GLGeneralInfoId,
                        BudgetMasterId = otherInvoiceVM.BudgetMasterId,
                        ActivityId = otherInvoiceVM.ActivityId,
                        SourceType = invoice.SourceType,
                        IsPark = true,
                        Id = _invoiceService.GetAutoNumber(nameof(OtherInvoice), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP
                    };
                    _otherInvoiceRepository.Insert(otherInvoice);
                }
                _unitOfWork.SaveChanges();

                //Sales Inventory Posting



                // INSERT INTO Voucher TABLE
                var invvoucher = new Voucher
                {
                    CompanyGroupId = invoice.CompanyGroupId,
                    CompanyId = invoice.CompanyId,
                    PlantId = invoice.PlantId,
                    CurrencyId = companyCurrencyId,
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
                    Narration = "Inventory Journal",
                    PostingDate = receiveData.SalesDate,
                    SourceType = SourceType.SalesInvoice.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };

                invvoucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + invvoucher.Id;
                _voucherService.InsertVoucher(invvoucher, voucherVM.FiscalYearPrefix);

                receiveData.InventoryVoucherId = invvoucher.Id;
                _inventorySalesRepository.Update(receiveData);


                var invcurrentVoucherDetaiRecord = 0;
                decimal invtotalAmountDr = 0;
                decimal invtotalAmountCr = 0;
                foreach (var invvoucherDetailVM in inventoryJVList.Where(r => r.Amount > 0))
                {
                    if (string.IsNullOrEmpty(invvoucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (string.IsNullOrEmpty(invvoucherDetailVM.BudgetMasterId))
                        throw new CustomException("Without Budget can not post.");
                    if (string.IsNullOrEmpty(invvoucherDetailVM.ActivityId))
                        throw new CustomException("Without Activity can not post.");
                    if (invvoucherDetailVM.TrnType == "Cr")
                    {
                        // in libility side Dr.
                        var invvoucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = invvoucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = invvoucherDetailVM.BudgetMasterId,
                            ActivityId = invvoucherDetailVM.ActivityId,
                            CrAmount = invvoucherDetailVM.Amount,
                            CurrencyId = companyCurrencyId,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = invvoucherDetailVM.Narration,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax
                        };
                        invtotalAmountCr += invvoucherCr.CrAmount;
                        invvoucherDetailVM.Id = invvoucherCr.Id;
                        invcurrentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(invvoucher, invvoucherCr, invcurrentVoucherDetaiRecord);

                        foreach (var item in inventoryPayableVMList.Where(r => r.GLGeneralInfoId == invvoucherCr.GLGeneralInfoId
                        && r.BudgetMasterId == invvoucherCr.BudgetMasterId && r.ActivityId == invvoucherCr.ActivityId && r.TrnType == "Cr"))
                        {
                            var inventorySalesDetail = _inventorySalesDetailRepository.Find(item.InventorySalesDetailId);
                            
                            inventorySalesDetail.PostCrInventoryGLId = invvoucherCr.GLGeneralInfoId;
                            inventorySalesDetail.PostCrInventoryBudgetMasterId = invvoucherCr.BudgetMasterId;
                            inventorySalesDetail.PostCrInventoryActivityId = invvoucherCr.ActivityId;
                            inventorySalesDetail.ModelState = ModelState.Modified;
                            AuditService.UpdatedLog(inventorySalesDetail);
                            _inventorySalesDetailRepository.Update(inventorySalesDetail);
                        }

                        var invvoucherDetailCurrencydb = new VoucherDetailCurrency
                        {
                            ToCurrencyRate = voucherVM.ToCurrencyRate,
                            ToCurrencyId = companyCurrencyId,
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            CrAmount =  invvoucherCr.CrAmount,
                            ToCurrencyConversion = 1 
                        };
                        _voucherService.InsertVoucherDetailCompanyCurrency(invvoucherCr, invvoucherDetailCurrencydb);
                        invvoucherDetailCurrencydb = null;

                    }
                    else if (invvoucherDetailVM.TrnType == "Dr")
                    {
                        var invvoucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = invvoucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = invvoucherDetailVM.BudgetMasterId,
                            ActivityId = invvoucherDetailVM.ActivityId,
                            CurrencyId = companyCurrencyId,
                            CrAmount = 0,
                            DrAmount = invvoucherDetailVM.Amount,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = invoice.Narration,
                            EmployeeId = invoice.EmployeeId,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax
                        };
                        invvoucherDetailVM.Id = invvoucherDr.Id;
                        invtotalAmountDr += invvoucherDr.DrAmount;
                        foreach (var item in inventoryPayableVMList.Where(r => r.GLGeneralInfoId == invvoucherDr.GLGeneralInfoId
                       && r.BudgetMasterId == invvoucherDr.BudgetMasterId && r.ActivityId == invvoucherDr.ActivityId && r.TrnType == "Dr"))
                        {
                            var inventorySalesDetail = _inventorySalesDetailRepository.Find(item.InventorySalesDetailId);
                            inventorySalesDetail.PostDrInventoryGLId = invvoucherDr.GLGeneralInfoId;
                            inventorySalesDetail.PostDrInventoryBudgetMasterId = invvoucherDr.BudgetMasterId;
                            inventorySalesDetail.PostDrInventoryActivityId = invvoucherDr.ActivityId;
                            inventorySalesDetail.ModelState = ModelState.Modified;
                            AuditService.UpdatedLog(inventorySalesDetail);
                            _inventorySalesDetailRepository.Update(inventorySalesDetail);
                        }
                        invcurrentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(invvoucher, invvoucherDr, invcurrentVoucherDetaiRecord);
                        var invvoucherDetailCurrencydb = new VoucherDetailCurrency
                        {
                            ToCurrencyRate = voucherVM.ToCurrencyRate,
                            ToCurrencyId = companyCurrencyId,
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            DrAmount = invvoucherDr.DrAmount,
                            ToCurrencyConversion = 1
                        };
                        _voucherService.InsertVoucherDetailCompanyCurrency(invvoucherDr, invvoucherDetailCurrencydb);
                        invvoucherDetailCurrencydb = null;

                    }
                }

                // Update Inventory Received


                if (invtotalAmountDr != invtotalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
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


        public void PostMultipleJournalSalesReturn(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
         , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
          , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<VoucherDetailViewModel> inventoryJVList, OtherInvoice otherInvoiceVM)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id
                var receiveData = _inventorySalesRepository.Find(receiveId);
                voucherVM.PostingDate = receiveData.SalesDate;
                voucherVM.DocDate = receiveData.DocDate;
                voucherVM.DocRefNo = receiveData.DocRefNo;
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                if (receiveData.Status == "Posting")
                    throw new CustomException("The Sales no '" + receiveData.Id + "' already Posted!");

                //var companyParty = _companyPartyRepository.Query(r => r.PartyId == receiveData.CustomerId).Select().FirstOrDefault();

                // var inventoryReceive=_inventoryReceiveRepository.Find();
                #endregion Get Company Parallerl Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Invoice TABLE
                voucherVM.Narration = "Sales Return Journal";
                var invoice = new Invoice
                {
                    Amount = voucherVM.Amount,
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
                    PartyType = PartyType.Vendor.ToString(),
                    EmployeeId = voucherVM.EmployeeId,
                    PaymentTermId = voucherVM.PaymentTermId,
                    PostingDate = receiveData.SalesDate,
                    SourceType = SourceType.SalesInvoice.ToString(),

                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    CompanyCurrencyRate = voucherVM.ToCurrencyRate
                };
                if (acceptanceId != null)
                {
                    invoice.BaseNoOfDays = 0;
                    invoice.BaseOnDueDate = null;
                    invoice.RevisedDueDate = null;
                    invoice.ActualDueDate = null;
                }
                else
                {
                    invoice.BaseNoOfDays = voucherVM.BaseNoOfDays;
                    invoice.BaseOnDueDate = voucherVM.BaseOnDueDate;
                    invoice.RevisedDueDate = voucherVM.MatureDate;
                    invoice.ActualDueDate = voucherVM.MatureDate;
                }
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
                    VoucherDate = invoice.VoucherDate,
                    DocDate = invoice.DocDate,
                    DocRefNo = invoice.DocRefNo,
                    Archive = invoice.Archive,
                    IsPark = invoice.IsPark,
                    Narration = invoice.Narration,
                    PostingDate = receiveData.SalesDate,
                    SourceType = SourceType.SalesInvoice.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };

                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                receiveData.Status = "Posting";
                receiveData.ModelState = ModelState.Modified;
                AuditService.UpdatedLog(receiveData);
                invoice.InventorySalesId = receiveId;
                receiveData.VoucherId = voucher.Id;
                _inventorySalesRepository.Update(receiveData);

                invoice.VoucherId = voucher.Id;

                var currentInvoiceDetail = 0;
                var currentVoucherDetaiRecord = 0;
                var currentTaxRecord = 0;
                decimal totalAmountDr = 0;
                decimal totalAmountCr = 0;
                foreach (var voucherDetailVM in voucherDetailVMList.Where(r => r.Amount > 0))
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                        throw new CustomException("Without Budget can not post.");
                    if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                        throw new CustomException("Without Activity can not post.");
                    if (voucherDetailVM.TrnType == "Cr")
                    {
                        // in libility side Dr.
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CrAmount = voucherDetailVM.Amount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax
                        };
                        totalAmountCr += voucherCr.CrAmount;
                        voucherDetailVM.Id = voucherCr.Id;
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);

                        //foreach (var item in inventoryReceiveDetailVMList.Where(r => r.GLGeneralInfoId == voucherCr.GLGeneralInfoId
                        //&& r.BudgetMasterId == voucherCr.BudgetMasterId && r.ActivityId == voucherCr.ActivityId && r.TrnType == "Cr"))
                        //{
                        //    var inventorySalesDetail = _inventorySalesDetailRepository.Find(item.InventorySalesDetailId);
                        //    var CrGLBAct = inventoryReceiveDetailVMList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                        //    inventorySalesDetail.PostCrGLGeneralInfoId = voucherCr.GLGeneralInfoId;
                        //    inventorySalesDetail.PostCrBudgetMasterId = voucherCr.BudgetMasterId;
                        //    inventorySalesDetail.PostCrActivityId = voucherCr.ActivityId;
                        //    inventorySalesDetail.ModelState = ModelState.Modified;
                        //    AuditService.UpdatedLog(inventorySalesDetail);
                        //    _inventorySalesDetailRepository.Update(inventorySalesDetail);
                        //}

                        if (voucherDetailVM.OtherName == "Tax")
                        {
                            // var voucherDetailDrId = voucherDetailVMList.FirstOrDefault(t => t.TrnType == "Cr" && t.OtherName == "Tax" ).Id;
                            currentTaxRecord++;
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                Id = _pkGeneratorService.MakePK(invoice.Id, currentTaxRecord, 2),
                                VoucherDetailId = voucherCr.Id,//voucherDetailDrId,
                                VoucherId = voucher.Id,
                                InvoiceId = invoice.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.SalesInvoice.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxRepository.Insert(invoiceTax);
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                Id = invoiceTax.Id + 1,
                                InvoiceTaxId = invoiceTax.Id,
                                Amount = invoiceTax.TaxAmount,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetId,
                                ActivityId = voucherDetailVM.ActivityId,
                                AType = "Cr",
                                AddedBy = invoiceTax.AddedBy,
                                AddedDate = invoiceTax.AddedDate,
                                AddedFromIP = invoiceTax.AddedFromIP
                            };
                            _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                        }
                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                            CurrencyExchange(voucherVM.CurrencyId, companyCurrencyId, null, null,
                               voucherDetailCurrency.CompanyCurrencyCr, voucherDetailCurrency.CompanyGroupCurrencyCr,
                               voucherDetailCurrency.HardCurrencyCr, voucherDetailCurrency, voucherCr.DrAmount);

                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                    else if (voucherDetailVM.TrnType == "Dr")
                    {
                        currentInvoiceDetail++;
                        // INSERT INTO InvoiceDetail
                        var invoiceDetail = new InvoiceDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            MaterialGroupMasterId = voucherDetailVM.MaterialGroupMasterId,
                            Amount = voucherDetailVM.Amount,
                            NetAmount = voucherDetailVM.Amount,
                            TaxAmount = 0,
                            AddedBy = invoice.AddedBy,
                            AddedDate = invoice.AddedDate,
                            AddedFromIP = invoice.AddedFromIP,
                            Archive = invoice.Archive,
                            InvoiceId = invoice.Id,
                        };
                        invoice.Amount = invoiceDetail.Amount;
                        // INSERT INTO VoucherDetail
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceDetail.BudgetMasterId,
                            ActivityId = invoiceDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            CrAmount = 0,
                            DrAmount = voucherDetailVM.Amount,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = invoice.Narration,
                            EmployeeId = invoice.EmployeeId,
                            InvoiceDetailId = invoiceDetail.Id,
                            PartyId = invoice.PartyId,
                            PartyPlantId = invoice.PartyPlantId,
                            PartyType = invoice.PartyType,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax
                        };
                        voucherDetailVM.Id = voucherDr.Id;
                        totalAmountDr += voucherDr.DrAmount;
                       // foreach (var item in inventoryReceiveDetailVMList.Where(r => r.GLGeneralInfoId == voucherDr.GLGeneralInfoId
                       //&& r.BudgetMasterId == voucherDr.BudgetMasterId && r.ActivityId == voucherDr.ActivityId && r.TrnType == "Dr"))
                       // {
                       //     var inventorySalesDetail = _inventorySalesDetailRepository.Find(item.InventorySalesDetailId);
                       //     inventorySalesDetail.PostDrGLGeneralInfoId = voucherDr.GLGeneralInfoId;
                       //     inventorySalesDetail.PostDrBudgetMasterId = voucherDr.BudgetMasterId;
                       //     inventorySalesDetail.PostDrActivityId = voucherDr.ActivityId;
                       //     inventorySalesDetail.ModelState = ModelState.Modified;
                       //     AuditService.UpdatedLog(inventorySalesDetail);
                       //     _inventorySalesDetailRepository.Update(inventorySalesDetail);
                       // }
                        if (voucherDetailVM.OtherName == "Tax")
                        {
                            // var voucherDetailCrrId = voucherDetailVMList.FirstOrDefault(t => t.TrnType == "Cr" && t.OtherName == "Tax").Id;
                            currentTaxRecord++;
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                Id = _pkGeneratorService.MakePK(invoice.Id, currentTaxRecord, 2),
                                VoucherDetailId = voucherDr.Id,//voucherDetailCrrId,
                                InvoiceId = invoice.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.SalesInvoice.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxRepository.Insert(invoiceTax);
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                Id = invoiceTax.Id + 1,
                                InvoiceTaxId = invoiceTax.Id,
                                Amount = invoiceTax.TaxAmount,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetId,
                                ActivityId = voucherDetailVM.ActivityId,
                                AType = "Dr",
                                Archive = false,
                                ModelState = ModelState.Added,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                        }

                        //_invoiceDetailRepository.Insert(invoiceDetail);
                        _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail);
                        currentVoucherDetaiRecord++;
                        voucherDr.InvoiceDetailId = invoiceDetail.Id;
                        _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                        #region Currency

                        foreach (var voucherDetailCurrency in voucherDetailCurrencyVMList)
                        {
                            CurrencyExchange(voucherVM.CurrencyId, companyCurrencyId, null, null,
                               voucherDetailCurrency.CompanyCurrencyCr, voucherDetailCurrency.CompanyGroupCurrencyCr,
                               voucherDetailCurrency.HardCurrencyCr, voucherDetailCurrency, voucherDr.DrAmount);

                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }

                        #endregion Currency
                    }
                }

                // Update Inventory Received


                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                if (otherInvoiceVM.PartyId != null && otherInvoiceVM.GLGeneralInfoId != null)
                {
                    var otherInvoice = new OtherInvoice
                    {

                        Amount = otherInvoiceVM.Amount,
                        PartyId = otherInvoiceVM.PartyId,
                        PartyPlantId = otherInvoiceVM.PartyPlantId,
                        InvoiceId = invoice.Id,
                        GLGeneralInfoId = otherInvoiceVM.GLGeneralInfoId,
                        BudgetMasterId = otherInvoiceVM.BudgetMasterId,
                        ActivityId = otherInvoiceVM.ActivityId,
                        SourceType = invoice.SourceType,
                        IsPark = true,
                        Id = _invoiceService.GetAutoNumber(nameof(OtherInvoice), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP
                    };
                    _otherInvoiceRepository.Insert(otherInvoice);
                }
                _unitOfWork.SaveChanges();

                //Sales Inventory Posting



                // INSERT INTO Voucher TABLE
                var invvoucher = new Voucher
                {
                    CompanyGroupId = invoice.CompanyGroupId,
                    CompanyId = invoice.CompanyId,
                    PlantId = invoice.PlantId,
                    CurrencyId = companyCurrencyId,
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
                    Narration = "Inventory Journal",
                    PostingDate = receiveData.SalesDate,
                    SourceType = SourceType.SalesInvoice.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };

                invvoucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + invvoucher.Id;
                _voucherService.InsertVoucher(invvoucher, voucherVM.FiscalYearPrefix);

                receiveData.InventoryVoucherId = invvoucher.Id;
                _inventorySalesRepository.Update(receiveData);


                var invcurrentVoucherDetaiRecord = 0;
                decimal invtotalAmountDr = 0;
                decimal invtotalAmountCr = 0;
                foreach (var invvoucherDetailVM in inventoryJVList.Where(r => r.Amount > 0))
                {
                    if (string.IsNullOrEmpty(invvoucherDetailVM.GLGeneralInfoId))
                        throw new CustomException("Without GL can not post.");
                    if (string.IsNullOrEmpty(invvoucherDetailVM.BudgetMasterId))
                        throw new CustomException("Without Budget can not post.");
                    if (string.IsNullOrEmpty(invvoucherDetailVM.ActivityId))
                        throw new CustomException("Without Activity can not post.");
                    if (invvoucherDetailVM.TrnType == "Cr")
                    {
                        // in libility side Dr.
                        var invvoucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = invvoucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = invvoucherDetailVM.BudgetMasterId,
                            ActivityId = invvoucherDetailVM.ActivityId,
                            CrAmount = invvoucherDetailVM.Amount,
                            CurrencyId = companyCurrencyId,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = invvoucherDetailVM.Narration,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax
                        };
                        invtotalAmountCr += invvoucherCr.CrAmount;
                        invvoucherDetailVM.Id = invvoucherCr.Id;
                        invcurrentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(invvoucher, invvoucherCr, invcurrentVoucherDetaiRecord);

                        //foreach (var item in inventoryPayableVMList.Where(r => r.GLGeneralInfoId == invvoucherCr.GLGeneralInfoId
                        //&& r.BudgetMasterId == invvoucherCr.BudgetMasterId && r.ActivityId == invvoucherCr.ActivityId && r.TrnType == "Cr"))
                        //{
                        //    var inventorySalesDetail = _inventorySalesDetailRepository.Find(item.InventorySalesDetailId);

                        //    inventorySalesDetail.PostCrInventoryGLId = invvoucherCr.GLGeneralInfoId;
                        //    inventorySalesDetail.PostCrInventoryBudgetMasterId = invvoucherCr.BudgetMasterId;
                        //    inventorySalesDetail.PostCrInventoryActivityId = invvoucherCr.ActivityId;
                        //    inventorySalesDetail.ModelState = ModelState.Modified;
                        //    AuditService.UpdatedLog(inventorySalesDetail);
                        //    _inventorySalesDetailRepository.Update(inventorySalesDetail);
                        //}

                        var invvoucherDetailCurrencydb = new VoucherDetailCurrency
                        {
                            ToCurrencyRate = voucherVM.ToCurrencyRate,
                            ToCurrencyId = companyCurrencyId,
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            CrAmount = invvoucherCr.CrAmount,
                            ToCurrencyConversion = 1
                        };
                        _voucherService.InsertVoucherDetailCompanyCurrency(invvoucherCr, invvoucherDetailCurrencydb);
                        invvoucherDetailCurrencydb = null;

                    }
                    else if (invvoucherDetailVM.TrnType == "Dr")
                    {
                        var invvoucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = invvoucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = invvoucherDetailVM.BudgetMasterId,
                            ActivityId = invvoucherDetailVM.ActivityId,
                            CurrencyId = companyCurrencyId,
                            CrAmount = 0,
                            DrAmount = invvoucherDetailVM.Amount,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = invoice.Narration,
                            EmployeeId = invoice.EmployeeId,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax
                        };
                        invvoucherDetailVM.Id = invvoucherDr.Id;
                        invtotalAmountDr += invvoucherDr.DrAmount;
                       // foreach (var item in inventoryPayableVMList.Where(r => r.GLGeneralInfoId == invvoucherDr.GLGeneralInfoId
                       //&& r.BudgetMasterId == invvoucherDr.BudgetMasterId && r.ActivityId == invvoucherDr.ActivityId && r.TrnType == "Dr"))
                       // {
                       //     var inventorySalesDetail = _inventorySalesDetailRepository.Find(item.InventorySalesDetailId);
                       //     inventorySalesDetail.PostDrInventoryGLId = invvoucherDr.GLGeneralInfoId;
                       //     inventorySalesDetail.PostDrInventoryBudgetMasterId = invvoucherDr.BudgetMasterId;
                       //     inventorySalesDetail.PostDrInventoryActivityId = invvoucherDr.ActivityId;
                       //     inventorySalesDetail.ModelState = ModelState.Modified;
                       //     AuditService.UpdatedLog(inventorySalesDetail);
                       //     _inventorySalesDetailRepository.Update(inventorySalesDetail);
                       // }
                        invcurrentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(invvoucher, invvoucherDr, invcurrentVoucherDetaiRecord);
                        var invvoucherDetailCurrencydb = new VoucherDetailCurrency
                        {
                            ToCurrencyRate = voucherVM.ToCurrencyRate,
                            ToCurrencyId = companyCurrencyId,
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            DrAmount = invvoucherDr.DrAmount,
                            ToCurrencyConversion = 1
                        };
                        _voucherService.InsertVoucherDetailCompanyCurrency(invvoucherDr, invvoucherDetailCurrencydb);
                        invvoucherDetailCurrencydb = null;

                    }
                }

                // Update Inventory Received


                if (invtotalAmountDr != invtotalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
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


        #endregion

        public string InsertInventoryTransferPayable(string receiveId, VoucherViewModel voucherVM
           , IEnumerable<VoucherDetailViewModel> fromPlantInventoryTransferJV
           , IEnumerable<VoucherDetailViewModel> toPlantInventoryTransferJV
           , IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
           )
        {
            var flag = false;
            try
            {
                string voucherNo = null;
                var receiveData = _inventoryReceiveRepository.Find(receiveId);
                voucherVM.PostingDate = receiveData.GRNDate;

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                if (receiveData.Status == "Posting")
                    throw new CustomException("The GRN no '" + receiveData.Id + "' already Posted!");


                _unitOfWork.BeginTransaction();
                flag = true;
                if (voucherVM.CurrencyId == companyCurrencyId)
                {
                    voucherVM.ToCurrencyRate = 1;
                }
                // INSERT INTO Invoice TABLE
                var currentVoucherDetaiRecord = 0;
                decimal totalAmountDr = 0;
                decimal totalAmountCr = 0;
                string fromVoucherNo = null;
                if (fromPlantInventoryTransferJV != null)
                {
                    var voucherFrom = new Voucher
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        CurrencyId = voucherVM.CurrencyId,
                        FiscalYearId = voucherVM.FiscalYearId,
                        FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                        TaxYearId = voucherVM.TaxYearId,
                        TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                        AddedBy = voucherVM.AddedBy,
                        AddedDate = voucherVM.AddedDate,
                        AddedFromIP = voucherVM.AddedFromIP,
                        VoucherDate = voucherVM.VoucherDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Archive = false,
                        IsPark = false,
                        Narration = voucherVM.Narration,
                        PostingDate = receiveData.GRNDate,
                        SourceType = SourceType.InventoryPayable.ToString(),
                        VoucherTypeId = voucherVM.VoucherTypeId,
                    };
                    voucherFrom.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucherFrom.Id;
                    _voucherService.InsertVoucher(voucherFrom, voucherVM.FiscalYearPrefix);
                    fromVoucherNo = voucherFrom.VoucherNo;
                    receiveData.VoucherId = voucherFrom.Id;
                    receiveData.Status = "Posting";
                    receiveData.ModelState = ModelState.Modified;
                    receiveData.IsApproved = true;
                    AuditService.UpdatedLog(receiveData);
                    _inventoryReceiveRepository.Update(receiveData);
                    foreach (var voucherDetailVM in fromPlantInventoryTransferJV.Where(r => r.Amount > 0))
                    {


                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                            throw new CustomException("Without Budget can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                            throw new CustomException("Without Activity can not post.");
                        if (voucherDetailVM.TrnType == "Dr")
                        {
                            // in libility side Dr.
                            var voucherDrFrom = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                DrAmount = voucherDetailVM.Amount,
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                            };
                            totalAmountDr += voucherDrFrom.DrAmount;
                            voucherDetailVM.Id = voucherDrFrom.Id;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucherFrom, voucherDrFrom, currentVoucherDetaiRecord);


                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherVM.CurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherDrFrom.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDrFrom, voucherDetailCurrencydb);

                        }
                        else if (voucherDetailVM.TrnType == "Cr")
                        {

                            var voucherCrFrom = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CurrencyId = voucherFrom.CurrencyId,
                                DrAmount = 0,
                                CrAmount = voucherDetailVM.Amount,
                                DocDate = voucherFrom.DocDate,
                                DocRefNo = voucherFrom.DocRefNo,
                                Narration = voucherFrom.Narration,
                                EmployeeId = voucherVM.EmployeeId,
                                PartyId = voucherVM.PartyId,
                                PartyPlantId = voucherVM.PartyPlantId,
                                PartyType = voucherVM.PartyType,
                            };
                            voucherDetailVM.Id = voucherCrFrom.Id;
                            totalAmountCr += voucherCrFrom.CrAmount;

                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucherFrom, voucherCrFrom, currentVoucherDetaiRecord);
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherVM.CurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherCrFrom.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCrFrom, voucherDetailCurrencydb);
                        }
                    }

                }

                if (toPlantInventoryTransferJV != null)
                {
                    var inventoryReceivedIds = new List<InventoryReceiveDetail>();
                    var voucherTo = new Voucher
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.ToPlantId,
                        CurrencyId = voucherVM.CurrencyId,
                        FiscalYearId = voucherVM.FiscalYearId,
                        FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                        TaxYearId = voucherVM.TaxYearId,
                        TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                        AddedBy = voucherVM.AddedBy,
                        AddedDate = voucherVM.AddedDate,
                        AddedFromIP = voucherVM.AddedFromIP,
                        VoucherDate = voucherVM.VoucherDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Archive = false,
                        IsPark = false,
                        Narration = voucherVM.Narration,
                        PostingDate = receiveData.GRNDate,
                        SourceType = SourceType.InventoryPayable.ToString(),
                        VoucherTypeId = voucherVM.VoucherTypeId,
                    };
                    voucherTo.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucherTo.Id;
                    _voucherService.InsertVoucher(voucherTo, voucherVM.FiscalYearPrefix);

                    receiveData.ToVoucherId = voucherTo.Id;
                    receiveData.Status = "Posting";
                    receiveData.ModelState = ModelState.Modified;
                    receiveData.IsApproved = true;
                    AuditService.UpdatedLog(receiveData);
                    _inventoryReceiveRepository.Update(receiveData);

                    foreach (var voucherDetailVMTo in toPlantInventoryTransferJV.Where(r => r.Amount > 0))
                    {

                        if (string.IsNullOrEmpty(voucherDetailVMTo.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVMTo.BudgetMasterId))
                            throw new CustomException("Without Budget can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVMTo.ActivityId))
                            throw new CustomException("Without Activity can not post.");
                        if (voucherDetailVMTo.TrnType == "Dr")
                        {
                            // in libility side Dr.
                            var voucherDrTo = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVMTo.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVMTo.BudgetMasterId,
                                ActivityId = voucherDetailVMTo.ActivityId,
                                DrAmount = voucherDetailVMTo.Amount,
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = voucherDetailVMTo.Narration,
                            };
                            totalAmountDr += voucherDrTo.DrAmount;
                            voucherDetailVMTo.Id = voucherDrTo.Id;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucherTo, voucherDrTo, currentVoucherDetaiRecord);

                            foreach (var item in toPlantInventoryTransferJV.Where(r => r.GLGeneralInfoId == voucherDrTo.GLGeneralInfoId
                            && r.BudgetMasterId == voucherDrTo.BudgetMasterId && r.ActivityId == voucherDrTo.ActivityId && r.TrnType == "Dr"))
                            {
                                var inventoryReceiveDetail = _inventoryReceiveDetailRepository.Find(item.InventoryReceiveDetailId);
                                inventoryReceiveDetail.PostDrGLGeneralInfoId = voucherDrTo.GLGeneralInfoId;
                                inventoryReceiveDetail.PostDrBudgetMasterId = voucherDrTo.BudgetMasterId;
                                inventoryReceiveDetail.PostDrActivityId = voucherDrTo.ActivityId;
                                inventoryReceiveDetail.ModelState = ModelState.Modified;
                                AuditService.UpdatedLog(inventoryReceiveDetail);
                                _inventoryReceiveDetailRepository.Update(inventoryReceiveDetail);

                                inventoryReceivedIds.Add(inventoryReceiveDetail);
                            }


                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherVM.CurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherDrTo.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDrTo, voucherDetailCurrencydb);

                        }
                        else if (voucherDetailVMTo.TrnType == "Cr")
                        {

                            var voucherCrTo = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVMTo.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVMTo.BudgetMasterId,
                                ActivityId = voucherDetailVMTo.ActivityId,
                                CurrencyId = voucherTo.CurrencyId,
                                DrAmount = 0,
                                CrAmount = voucherDetailVMTo.Amount,
                                DocDate = voucherTo.DocDate,
                                DocRefNo = voucherTo.DocRefNo,
                                Narration = voucherTo.Narration,
                                EmployeeId = voucherVM.EmployeeId,
                                PartyId = voucherVM.PartyId,
                                PartyPlantId = voucherVM.PartyPlantId,
                                PartyType = voucherVM.PartyType,
                            };
                            voucherDetailVMTo.Id = voucherCrTo.Id;
                            totalAmountCr += voucherCrTo.CrAmount;
                            foreach (var item in inventoryReceivedIds)
                            {
                                item.PostCrGLGeneralInfoId = voucherCrTo.GLGeneralInfoId;
                                item.PostCrBudgetMasterId = voucherCrTo.BudgetMasterId;
                                item.PostCrActivityId = voucherCrTo.ActivityId;
                                item.ModelState = ModelState.Modified;
                                AuditService.UpdatedLog(item);
                                _inventoryReceiveDetailRepository.Update(item);
                            }
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucherTo, voucherCrTo, currentVoucherDetaiRecord);
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherVM.CurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherCrTo.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCrTo, voucherDetailCurrencydb);
                        }
                    }

                }


                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");


                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                voucherNo = fromVoucherNo;

                return voucherNo;
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

        private AdjustmentNote InsertAdjustmentNote(AdjustmentNote adjustmentNote)
        {
            adjustmentNote.Id = _invoiceService.GetAutoNumber(nameof(AdjustmentNote), PKGeneratorEnum.Yearly, null, DateTime.Now);
            _adjustmentNoteService.InsertGraph(adjustmentNote);
            return adjustmentNote;
        }
        private void Check(AdjustmentNote entity)
        {
            _adjustmentNoteService.CheckUniqueColumn(UniqueColumnName.DocRefNo, entity.DocRefNo, r => r.Id != entity.Id && r.PartyId == entity.PartyId && r.DocRefNo == entity.DocRefNo);
        }
        private AdjustmentNote InsertAdjustmentNote(VoucherViewModel voucherVM)
        {
            var adjustmentNote = new AdjustmentNote
            {
                CompanyGroupId = voucherVM.CompanyGroupId,
                CompanyId = voucherVM.CompanyId,
                PlantId = voucherVM.PlantId,
                EntityId = voucherVM.EntityId,
                FiscalYearId = voucherVM.FiscalYearId,
                FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                TaxYearId = voucherVM.TaxYearId,
                TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                VoucherTypeId = voucherVM.VoucherTypeId,
                CurrencyId = voucherVM.CurrencyId,
                Amount = voucherVM.Amount,
                VoucherDate = voucherVM.VoucherDate,
                PostingDate = voucherVM.PostingDate,
                DocDate = voucherVM.DocDate,
                DocRefNo = voucherVM.DocRefNo,
                Narration = voucherVM.Narration,
                PartyType = voucherVM.PartyType,
                PartyId = voucherVM.PartyId,
                PartyPlantId = voucherVM.PartyPlantId,
                SourceType = voucherVM.SourceType,
                IsPark = voucherVM.IsPark,
                NoteType = voucherVM.NoteType,
                InvoiceId = voucherVM.InvoiceId,
                Archive = false,
                SettlementType = voucherVM.SettlementType
            };
            if (adjustmentNote.SourceType == SourceType.CreditNote.ToString())
            {
                if (adjustmentNote.NoteType == NoteType.CustomerCreditNote.ToString())
                    adjustmentNote.PartyType = PartyType.Customer.ToString();
                else if (adjustmentNote.NoteType == NoteType.VendorCreditNote.ToString())
                    adjustmentNote.PartyType = PartyType.Vendor.ToString();
                else throw new CustomException("Party type is null.");
            }
            else if (adjustmentNote.SourceType == SourceType.DebitNote.ToString())
            {
                if (adjustmentNote.NoteType == NoteType.CustomerDebitNote.ToString())
                    adjustmentNote.PartyType = PartyType.Customer.ToString();
                else if (adjustmentNote.NoteType == NoteType.VendorDebitNote.ToString())
                    adjustmentNote.PartyType = PartyType.Vendor.ToString();
                else throw new CustomException("Party type is null.");
            }
            Check(adjustmentNote);
            return InsertAdjustmentNote(adjustmentNote);
        }

        private AdjustmentNoteDetail InsertAdjustmentNoteDetail(AdjustmentNote adjustmentNote, AdjustmentNoteDetail adjustmentNoteDetail, int currentId)
        {
            adjustmentNoteDetail.Id =_pkGeneratorService.MakePK(adjustmentNote.Id, currentId, 1);
            adjustmentNoteDetail.AdjustmentNoteId = adjustmentNote.Id;
            adjustmentNoteDetail.InvoiceId = adjustmentNote.InvoiceId;
            adjustmentNoteDetail.AddedBy = adjustmentNote.AddedBy;
            adjustmentNoteDetail.AddedDate = adjustmentNote.AddedDate;
            adjustmentNoteDetail.AddedFromIP = adjustmentNote.AddedFromIP;
            adjustmentNoteDetail.Archive = adjustmentNote.Archive;
            _AdjustmentNoteDetailRepository.Insert(adjustmentNoteDetail);
            return adjustmentNoteDetail;
        }
        public string InsertPurchaseReturnPayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _unitOfWork.BeginTransaction();
                flag = true;
                voucherVM.PartyType = PartyType.Vendor.ToString();
                voucherVM.NoteType = NoteType.VendorDebitNote.ToString();
                voucherVM.Amount = voucherDetailVMList.Where(r => r.OtherName == "Return").Sum(r => r.Amount);
                // INSERT INTO AdjustmentNote
                voucherVM.DocRefNo = "PR" + voucherVM.DocRefNo;

                var adjustmentNote = InsertAdjustmentNote(voucherVM);

                //invoicewriteoff
                // var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);
                voucherVM.AddedBy = voucher.AddedBy;
                voucherVM.AddedDate = voucher.AddedDate;
                voucherVM.AddedFromIP = voucher.AddedFromIP;
                adjustmentNote.VoucherId = voucher.Id;
                // Set VoucherId
                //adjustmentNote.VoucherId = voucher.Id;
                var purhcaseReturn = _purchaseReturnRepository.Find(voucherVM.Id);
                purhcaseReturn.Status = "Posting";
                purhcaseReturn.VoucherId = voucher.Id;
                purhcaseReturn.DocRefNo = voucherVM.DocRefNo;
                _purchaseReturnRepository.Update(purhcaseReturn);


                var currentVoucherDetailId = 0;
                decimal totalAmountDr = 0;
                decimal totalAmountCr = 0;
                var currentInvoiceWriteOffDetailId = 0;
                // INSERT INTO VoucherDetail


                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();

                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (voucherDetailVM.OtherName == "Return")
                    {
                        var adjustmentNoteDetail = new AdjustmentNoteDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            Amount = adjustmentNote.Amount,
                            WrittenOffAmount = 0,
                            IsWrittenOff = false
                        };
                        InsertAdjustmentNoteDetail(adjustmentNote, adjustmentNoteDetail, 1);
                        // INSERT INTO InvoiceWriteOff
                        var invoiceWriteOff = _invoiceWriteOffService.InsertInvoiceWriteOff(voucherVM);
                        
                        var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                        var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                        var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                        var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();

                        var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                        if (null == invoiceDetail)
                            throw new CustomException("Invoice not found!");

                        invoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;
                        if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                            throw new CustomException("Received amount can not cross balance amount.");

                        invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                        invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                        invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                        invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                        var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                        invoice.WrittenOffAmount = invoiceDetail.WrittenOffAmount;
                        invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                        invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                        invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                        invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        _invoiceService.Update(invoice);

                        // INSERT INTO InvoiceWriteOffDetail
                        currentInvoiceWriteOffDetailId++;
                        var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                        {
                            GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceDetail.BudgetMasterId,
                            ActivityId = invoiceDetail.ActivityId,
                            CurrencyId = invoice.CurrencyId,
                            InvoiceWriteOffId = invoiceWriteOff.Id,
                            InvoiceId = voucherDetailVM.InvoiceId,
                            InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            PartyId = invoice.PartyId,
                            PartyPlantId = invoice.PartyPlantId,
                            PartyType = invoice.PartyType,
                            Amount = voucherDetailVM.Amount,
                            AddedBy = invoiceWriteOff.AddedBy,
                            AddedDate = invoiceWriteOff.AddedDate,
                            AddedFromIP = invoiceWriteOff.AddedFromIP,
                            Archive = invoiceWriteOff.Archive,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration
                        };
                        _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);

                        var voucherDetail = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            EntityId = voucher.EntityId,
                            PartyType = voucherVM.PartyType,
                            PartyId = voucherVM.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            TrnNature = "DebitNote",
                            AdjustmentNoteDetailId = adjustmentNoteDetail.Id,
                            DrAmount = voucherVM.Amount,
                            InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id
                        };
                        totalAmountDr += voucherDetail.DrAmount;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount
                        });

                    }

                    //if (adjustmentNote.SettlementType == SettlementType.Invoice.ToString())
                    //{
                    //    var voucherDetailDb = _voucherService.FindVoucherDetail(voucherDetailVM.Id);
                    //    voucherDetailVM.GLGeneralInfoId = voucherDetailDb.GLGeneralInfoId;
                    //    voucherDetailVM.BudgetMasterId = voucherDetailDb.BudgetMasterId;
                    //    voucherDetailVM.ActivityId = voucherDetailDb.ActivityId;
                    //}
                    if (voucherDetailVM.OtherName == "Tax" && voucherDetailVM.TrnType == "Cr" || voucherDetailVM.OtherName == "TCS" && voucherDetailVM.TrnType == "Cr" || voucherDetailVM.OtherName == "Material" && voucherDetailVM.TrnType == "Cr")
                    {
                        var voucherDetailCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            TrnNature = TransactionNature.Purchases.ToString(),
                            CrAmount = voucherDetailVM.Amount
                        };
                        totalAmountCr += voucherDetailCr.CrAmount;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount
                        });


                        if (voucherDetailVM.OtherName == "Tax" && voucherDetailVM.TrnType == "Cr" || voucherDetailVM.OtherName == "TCS" && voucherDetailVM.TrnType == "Cr")
                        {
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                VoucherDetailId = voucherDetailCr.Id,//voucherDetailDrId,
                                VoucherId = voucher.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.DebitNote.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _invoiceTaxService.InsertInvoiceTax(voucherVM, invoiceTax, invoiceTaxPk);
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

                    if (voucherDetailVM.OtherName == "Tax" && voucherDetailVM.TrnType == "Dr")
                    {
                        var voucherDetailDrTax = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            TrnNature = TransactionNature.Purchases.ToString(),
                            DrAmount = voucherDetailVM.Amount
                        };
                        totalAmountDr += voucherDetailDrTax.DrAmount;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDrTax, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDrTax, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDrTax.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDrTax.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDrTax.DrAmount
                        });



                        var invoiceTax = new InvoiceTax
                        {
                            Archive = false,
                            VoucherDetailId = voucherDetailDrTax.Id,//voucherDetailDrId,
                            VoucherId = voucher.Id,
                            TaxYearId = voucher.TaxYearId,
                            TaxYearPeriodId = voucher.TaxYearPeriodId,
                            TaxCategoryId = voucherDetailVM.TaxCategoryId,
                            TaxAmount = voucherDetailVM.Amount,
                            TaxAutoAmount = 0,
                            PartyId = voucherVM.PartyId,
                            SourceType = SourceType.InventoryPayable.ToString(),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _invoiceTaxService.InsertInvoiceTax(voucherVM, invoiceTax, invoiceTaxPk);
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

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
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

        public string InventoryOSReceivedPost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> inventoryJobWorkWIPList
            , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
        , IEnumerable<VoucherDetailViewModel> changeInInventoryList
        , IEnumerable<VoucherDetailViewModel> inventoryJobWorkGIRIList, VoucherViewModel ServiceVM)
        {
            var flag = false;
            try
            {

                string voucherNo = "";

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);


                _unitOfWork.BeginTransaction();
                flag = true;
                string TempvoucherNo = "";
                string wipVoucherId = "";
                string changeInVoucherId = "";
                string giriVoucherId = "";

                if (inventoryJobWorkWIPList != null)
                {
                    var voucherWiP = new Voucher
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        CurrencyId = voucherVM.CurrencyId,
                        FiscalYearId = voucherVM.FiscalYearId,
                        FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                        TaxYearId = voucherVM.TaxYearId,
                        TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                        AddedBy = voucherVM.AddedBy,
                        AddedDate = voucherVM.AddedDate,
                        AddedFromIP = voucherVM.AddedFromIP,
                        VoucherDate = voucherVM.VoucherDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        IsPark = voucherVM.IsPark,
                        Narration = voucherVM.Narration,
                        PostingDate = voucherVM.PostingDate,
                        SourceType = SourceType.OutSourceReceived.ToString(),
                        VoucherTypeId = voucherVM.VoucherTypeId,
                    };
                    voucherWiP.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucherWiP.Id;
                    _voucherService.InsertVoucher(voucherWiP, voucherVM.FiscalYearPrefix);
                    var currentVoucherDetaiRecord = 0;
                    foreach (var voucherDetailVMWIP in inventoryJobWorkWIPList.Where(r => r.Amount > 0))
                    {
                        if (voucherDetailVMWIP.TrnType == "Dr")
                        {
                            var voucherDr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVMWIP.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVMWIP.BudgetMasterId,
                                ActivityId = voucherDetailVMWIP.ActivityId,
                                DrAmount = voucherDetailVMWIP.Amount,
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = voucherDetailVMWIP.Narration,
                            };
                            voucherDetailVMWIP.Id = voucherDr.Id;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucherWiP, voucherDr, currentVoucherDetaiRecord);
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherVM.CurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherDr.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                           

                        }
                        else if (voucherDetailVMWIP.TrnType == "Cr")
                        {
                            var voucherCr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVMWIP.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVMWIP.BudgetMasterId,
                                ActivityId = voucherDetailVMWIP.ActivityId,
                                CurrencyId = voucherWiP.CurrencyId,
                                DrAmount = 0,
                                CrAmount = voucherDetailVMWIP.Amount,
                                DocDate = voucherWiP.DocDate,
                                DocRefNo = voucherWiP.DocRefNo,
                                Narration = voucherWiP.Narration,
                            };
                            voucherDetailVMWIP.Id = voucherCr.Id;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucherWiP, voucherCr, currentVoucherDetaiRecord);
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherVM.CurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherCr.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                            
                        }
                    }
                    TempvoucherNo += "'', " + voucherWiP.VoucherNo + "";
                    wipVoucherId = voucherWiP.Id;
                }

                if (changeInInventoryList != null)
                {
                    var voucherCIInv = new Voucher
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        CurrencyId = voucherVM.CurrencyId,
                        FiscalYearId = voucherVM.FiscalYearId,
                        FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                        TaxYearId = voucherVM.TaxYearId,
                        TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                        AddedBy = voucherVM.AddedBy,
                        AddedDate = voucherVM.AddedDate,
                        AddedFromIP = voucherVM.AddedFromIP,
                        VoucherDate = voucherVM.VoucherDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        IsPark = voucherVM.IsPark,
                        Narration = voucherVM.Narration,
                        PostingDate = voucherVM.PostingDate,
                        SourceType = "InventoryJWReceipt",
                        VoucherTypeId = voucherVM.VoucherTypeId,
                    };
                    voucherCIInv.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucherCIInv.Id;
                    _voucherService.InsertVoucher(voucherCIInv, voucherVM.FiscalYearPrefix);

                    var currentVoucherDetaiRecord = 0;
                    foreach (var voucherDetailVMCIInv in changeInInventoryList.Where(r => r.Amount > 0))
                    {
                        if (voucherDetailVMCIInv.TrnType == "Dr")
                        {
                            var voucherCIInvDr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVMCIInv.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVMCIInv.BudgetMasterId,
                                ActivityId = voucherDetailVMCIInv.ActivityId,
                                DrAmount = voucherDetailVMCIInv.Amount,
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = voucherDetailVMCIInv.Narration,
                            };
                            voucherDetailVMCIInv.Id = voucherCIInvDr.Id;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucherCIInv, voucherCIInvDr, currentVoucherDetaiRecord);
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherVM.CurrencyId,
                                DrAmount = voucherVM.ToCurrencyRate * voucherCIInvDr.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCIInvDr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                            foreach (var item in inventoryReceiveDetailVMList.Where(r => r.GLGeneralInfoId == voucherCIInvDr.GLGeneralInfoId
                         && r.BudgetMasterId == voucherCIInvDr.BudgetMasterId && r.ActivityId == voucherCIInvDr.ActivityId))
                            {
                                var inventoryReceiveDetail = _inventoryReceiveDetailRepository.Find(item.InventoryReceiveDetailId);
                                inventoryReceiveDetail.PostDrGLGeneralInfoId = voucherCIInvDr.GLGeneralInfoId;
                                inventoryReceiveDetail.PostDrBudgetMasterId = voucherCIInvDr.BudgetMasterId;
                                inventoryReceiveDetail.PostDrActivityId = voucherCIInvDr.ActivityId;
                                if(voucherVM.PartyId != null)
                                {
                                    var CrGLBAct = inventoryPayableVMList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                                    inventoryReceiveDetail.PostCrGLGeneralInfoId = CrGLBAct.GLGeneralInfoId;
                                    inventoryReceiveDetail.PostCrBudgetMasterId = CrGLBAct.BudgetMasterId;
                                    inventoryReceiveDetail.PostCrActivityId = CrGLBAct.ActivityId;
                                }
                               

                                inventoryReceiveDetail.ModelState = ModelState.Modified;
                                AuditService.UpdatedLog(inventoryReceiveDetail);
                                _inventoryReceiveDetailRepository.Update(inventoryReceiveDetail);
                            }
                        }
                        else if (voucherDetailVMCIInv.TrnType == "Cr")
                        {
                            var voucherCIInvCr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVMCIInv.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVMCIInv.BudgetMasterId,
                                ActivityId = voucherDetailVMCIInv.ActivityId,
                                CurrencyId = voucherCIInv.CurrencyId,
                                DrAmount = 0,
                                CrAmount = voucherDetailVMCIInv.Amount,
                                DocDate = voucherCIInv.DocDate,
                                DocRefNo = voucherCIInv.DocRefNo,
                                Narration = voucherCIInv.Narration,
                            };
                            voucherDetailVMCIInv.Id = voucherCIInvCr.Id;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucherCIInv, voucherCIInvCr, currentVoucherDetaiRecord);
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherVM.CurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherCIInvCr.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCIInvCr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }
                    }
                    TempvoucherNo += ", " + voucherCIInv.VoucherNo + "";
                    changeInVoucherId = voucherCIInv.Id;
                }

                if (inventoryJobWorkGIRIList != null)
                {
                    var voucherGIRI = new Voucher
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        CurrencyId = ServiceVM.CurrencyId,
                        FiscalYearId = voucherVM.FiscalYearId,
                        FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                        TaxYearId = voucherVM.TaxYearId,
                        TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                        AddedBy = voucherVM.AddedBy,
                        AddedDate = voucherVM.AddedDate,
                        AddedFromIP = voucherVM.AddedFromIP,
                        VoucherDate = voucherVM.VoucherDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        IsPark = voucherVM.IsPark,
                        Narration = voucherVM.Narration,
                        PostingDate = voucherVM.PostingDate,
                        SourceType = "InventoryJWReceipt",
                        VoucherTypeId = voucherVM.VoucherTypeId,
                    };
                    voucherGIRI.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucherGIRI.Id;
                    _voucherService.InsertVoucher(voucherGIRI, voucherVM.FiscalYearPrefix);

                    var currentVoucherDetaiRecord = 0;
                    foreach (var voucherDetailVMGIRI in inventoryJobWorkGIRIList.Where(r => r.Amount > 0))
                    {
                        if (voucherDetailVMGIRI.TrnType == "Dr")
                        {
                            // in libility side Dr.
                            var voucherDr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVMGIRI.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVMGIRI.BudgetMasterId,
                                ActivityId = voucherDetailVMGIRI.ActivityId,
                                DrAmount = voucherDetailVMGIRI.Amount,
                                CurrencyId = ServiceVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = voucherDetailVMGIRI.Narration,
                            };
                            voucherDetailVMGIRI.Id = voucherDr.Id;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucherGIRI, voucherDr, currentVoucherDetaiRecord);
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = ServiceVM.CurrencyId,
                                DrAmount = ServiceVM.ToCurrencyRate * voucherDr.DrAmount,
                                ToCurrencyConversion = 1 / ServiceVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }
                        else if (voucherDetailVMGIRI.TrnType == "Cr")
                        {
                            var voucherCr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVMGIRI.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVMGIRI.BudgetMasterId,
                                ActivityId = voucherDetailVMGIRI.ActivityId,
                                CurrencyId = ServiceVM.CurrencyId,
                                DrAmount = 0,
                                CrAmount = voucherDetailVMGIRI.Amount,
                                DocDate = voucherGIRI.DocDate,
                                DocRefNo = voucherGIRI.DocRefNo,
                                Narration = voucherGIRI.Narration,
                            };
                            voucherDetailVMGIRI.Id = voucherCr.Id;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucherGIRI, voucherCr, currentVoucherDetaiRecord);
                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = ServiceVM.CurrencyId,
                                CrAmount = ServiceVM.ToCurrencyRate * voucherCr.CrAmount,
                                ToCurrencyConversion = 1 / ServiceVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, voucherDetailCurrencydb);
                            voucherDetailCurrencydb = null;
                        }
                    }
                    TempvoucherNo += ", " + voucherGIRI.VoucherNo + "";
                    giriVoucherId = voucherGIRI.Id;
                }
                var receiveData = _inventoryReceiveRepository.Find(voucherVM.InventoryReceiveId);
                receiveData.JWWIPVoucherId = wipVoucherId;
                receiveData.JWChangeInInvVoucherId = changeInVoucherId;
                receiveData.JWGRIRVoucherId = giriVoucherId;
                receiveData.Status = "Posting";
                _inventoryReceiveRepository.Update(receiveData);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                voucherNo = TempvoucherNo;

                return voucherNo;
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