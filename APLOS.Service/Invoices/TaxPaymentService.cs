#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Banks;
using Library.Model.Currencies;
using Library.Model.Enums;
using Library.Model.Invoices;
using Library.Model.Organizations;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Vouchers;
using Library.Service.Calendars;
using Library.Service.Core;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Taxations;
using Library.Service.Vouchers;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Invoices
{
    public class TaxPaymentService : Service<InvoiceTaxWriteOff>, ITaxPaymentService
    {
        #region Cotr

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IVoucherTypeService _voucherTypeService;
        private readonly IRepositoryAsync<Voucher> _voucherRepository;
        private readonly IRepositoryAsync<VoucherDetail> _voucherDetailRepository;
        private readonly IRepositoryAsync<VoucherDetailCurrency> _voucherDetailCurrencyRepository;
        private readonly IRepositoryAsync<GLTransactionDetail> _glTransactionDetailRepository;
        private readonly IRepositoryAsync<Entity> _entityRepository;
        private readonly IRepositoryAsync<InvoiceTax> _invoiceTaxRepository;
        private readonly IRepositoryAsync<InvoiceTaxDetail> _invoiceTaxDetailRepository;
        private readonly IRepositoryAsync<InvoiceTaxWriteOffDetail> _invoiceTaxWriteOffDetailRepository;
        private readonly ISqlRepository _sqlRepository;
        private readonly IVoucherService _voucherService;
        private readonly ICompanyFiscalYearService _companyFiscalYearService;
        private readonly ICompanyTaxYearService _companyTaxYearService;
        private readonly IRepositoryAsync<BankMaster> _bankMasterRepository;

        public TaxPaymentService(
            IRepositoryAsync<InvoiceTaxWriteOff> taxPaymentService,
            IPKGeneratorService pkGeneratorService,
            ICompanyParallelCurrencyService companyParallelCurrencyService,
            IVoucherTypeService voucherTypeService,
            IRepositoryAsync<Voucher> voucherRepository,
            IRepositoryAsync<VoucherDetail> voucherDetailRepository,
            IRepositoryAsync<VoucherDetailCurrency> voucherDetailCurrencyRepository,
            IRepositoryAsync<Entity> entityRepository,
            IRepositoryAsync<GLTransactionDetail> glTransactionDetailRepository,
            IRepositoryAsync<InvoiceTax> invoiceTaxRepository,
            IRepositoryAsync<InvoiceTaxDetail> invoiceTaxDetailRepository,
            IRepositoryAsync<InvoiceTaxWriteOffDetail> invoiceTaxWriteOffDetailRepository,
            ICompanyFiscalYearService companyFiscalYearService,
            ICompanyTaxYearService companyTaxYearService,
            IVoucherService voucherService,
            IRepositoryAsync<BankMaster> bankMasterRepository,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(taxPaymentService, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _voucherTypeService = voucherTypeService;
            _voucherRepository = voucherRepository;
            _voucherDetailRepository = voucherDetailRepository;
            _voucherDetailCurrencyRepository = voucherDetailCurrencyRepository;
            _entityRepository = entityRepository;
            _glTransactionDetailRepository = glTransactionDetailRepository;
            _invoiceTaxRepository = invoiceTaxRepository;
            _invoiceTaxDetailRepository = invoiceTaxDetailRepository;
            _invoiceTaxWriteOffDetailRepository = invoiceTaxWriteOffDetailRepository;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
            _voucherService = voucherService;
            _companyFiscalYearService = companyFiscalYearService;
            _companyTaxYearService = companyTaxYearService;
            _bankMasterRepository = bankMasterRepository;
        }

        #endregion Cotr

        public override void Insert(InvoiceTaxWriteOff entity)
        {
            try
            {
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                entity.AddedBy, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public string GetPK()
        {
            return "GTD" + _pkGeneratorService.GetMaxNumber("GLTransactionDetail", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public List<Dictionary<string, object>> GetInvoiceTaxPayableList(string companyGroupId, string companyId, string taxCategoryId, DateTime fromDate, DateTime todate, string partyType, string partyId, string partyPlantId)
        {
            try
            {
                var sql = @"SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName, IVD.BudgetMasterId, B.UserName AS BudgetName, IVD.ActivityId, A.UserName AS ActivityName,
                                           V.VoucherNo, Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11),
	                                       V.PostingDate, 106), ' ', '-') PostingDate, VD.DocRefNo, VD.Narration, V.SourceType, IV.TaxCategoryId, IV.Id AS InvoiceTaxId, IVD.Id AS InvoiceTaxDetailId, VD.VoucherId, VD.EntityId, EN.UserName AS EntityName, VD.PlantId,
                                           VD.Id AS VoucherDetailId, VD.CurrencyId, C.Code AS CurrencyCode, IV.PartyId,PT.UserName AS PartyName, IVD.Amount AS Payable,
                                           IVD.WrittenOffAmount AS Payment, IVD.Amount-IVD.WrittenOffAmount AS Balance,IVD.AType,
	                                    	CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion

	                                       FROM [TRN].[InvoiceTaxDetail] AS IVD
                                           LEFT JOIN [TRN].[InvoiceTax] AS IV ON IVD.InvoiceTaxId=IV.Id
                                           LEFT JOIN [TRN].[Invoice] AS I ON IV.InvoiceId=I.Id
                                           LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=IV.VoucherDetailId
                                           LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                           LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
	                                       LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=IVD.BudgetMasterId
	                                       LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
	                                       LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                           LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                                           LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=VD.EntityId
                                           LEFT JOIN [HKP].[Party] AS PT ON PT.Id=IV.PartyId
	                                       LEFT JOIN (
	                                    	SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
	                                    	VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
	                                    	FROM [TRN].[VoucherDetailCurrency] AS VDC
	                                    	JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                                    	WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
	                                     ) AS CC ON CC.VoucherDetailId=VD.Id
                                           WHERE IV.Archive=0
                                           AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND I.PartyType='"+ partyType + @"' AND IV.PartyId='" + partyId + @"'
	                                        --AND IVD.AType='Cr' 
                                            AND IV.TaxCategoryId='" + taxCategoryId + @"'
										   AND  CONVERT(DATE,V.PostingDate )  BETWEEN  CONVERT(DATE,'" + fromDate + "') AND  CONVERT(DATE,'" + todate + "')";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetTaxPaymentDataList(string column, string value, string companyId, string plantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        select top 300 * from (SELECT V.VoucherNo,ITW.PostingDate,ITW.DocDate,ITW.DocRefNo,ITW.Amount 
										   FROM  trn.InvoiceTaxWriteOff ITW 
										   LEFT JOIN TRN.Voucher V ON V.Id=ITW.VoucherId 
										   WHERE  ITW.CompanyId='" + companyId + "' AND ITW.PlantId='"+ plantId + @"'
                ) AS TEMP WHERE " + strkey + " order by PostingDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public void InsertTaxPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id

                var parallerCurrency = _companyParallelCurrencyService.Query(r => r.CompanyId == voucherVM.CompanyId).Select();
                if (null == parallerCurrency)
                    throw new CustomException("Company Parallel Currency not found!");
                var companyCurrency = parallerCurrency.FirstOrDefault(r => r.ParallelCurrencyType == ParallelCurrencyType.CompanyCurrency.ToString());
                var companyCurrencyId = companyCurrency != null ? companyCurrency.CurrencyId : throw new CustomException("Company Parallel Currency Id not found!");
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                #endregion Get Company Parallerl Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO InvoiceWriteOff TABLE
                var invoiceTaxWriteOff = new InvoiceTaxWriteOff
                {
                    Id = GetInvoiceTaxWriteOffAutoNumber(),
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = voucherVM.CurrencyId,
                    PartyId = voucherVM.PartyId,
                    PartyType = null,
                    Amount = voucherVM.Amount,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    ModelState = ModelState.Added,
                    SourceType = SourceType.TaxPayment.ToString(),
                    VoucherId = null,
                    EmployeeId = null,
                    UpdatedBy = null,
                    UpdatedDate = null,
                    UpdatedFromIP = null,
                    Archive = false,
                    SourceFrom = voucherVM.SourceFrom,
                    SourceTo = PartyType.Vendor.ToString(),
                };
                AuditService.AddedLog(invoiceTaxWriteOff);

                // INSERT INTO Voucher TABLE
                
                var voucher = _voucherService.InsertVoucher(voucherVM);
                voucher.Id = base.GetAutoNumber("Voucher", PKGeneratorEnum.Auto, null, DateTime.Now);
                voucher.VoucherNo = base.GetAutoNumber("Voucher" + voucher.CompanyId, PKGeneratorEnum.Daily, null, DateTime.Now);
                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherRepository.Insert(voucher);

                // Set to InvoiceWriteOff
                invoiceTaxWriteOff.VoucherId = voucher.Id;

                var invoiceWriteOffDetailPk = GetMaxNumber("InvoiceWriteOffDetail", PKGeneratorEnum.Auto, null, DateTime.Now);
                var voucherdetailPk = GetMaxNumber("VoucherDetail", PKGeneratorEnum.Auto, null, DateTime.Now);
                var voucherDetailCurrencyPk = GetMaxNumber("VoucherDetailCurrency", PKGeneratorEnum.Auto, null, DateTime.Now);

                var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                
                // INSERT INTO VoucherDetail
                voucherdetailPk.MaxNumber++;

                var voucherCr = new VoucherDetail
                {
                    GLGeneralInfoId = bankMaster.GLGeneralInfoId,
                    BudgetMasterId = bankMaster.BudgetMasterId,
                    ActivityId = bankMaster.ActivityId,
                    EntityId = voucherVM.EntityId,
                    PlantId = _entityRepository.Find(voucherVM.EntityId)?.PlantId,
                    CurrencyId = voucher.CurrencyId,
                    FiscalYearId = voucher.FiscalYearId,
                    FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                    AddedBy = voucher.AddedBy,
                    AddedDate = voucher.AddedDate,
                    AddedFromIP = voucher.AddedFromIP,
                    Archive = voucher.Archive,
                    DrAmount = 0,
                    CrAmount = voucherVM.Amount,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = invoiceTaxWriteOff.Narration,
                    EmployeeId = null,
                    Id = voucherdetailPk.MaxNumber.ToString(),
                    InvoiceDetailId = null,
                    InvoiceWriteOffDetailId = null,
                    ModelState = ModelState.Added,
                    PartyId = null,
                    PartyType = null,
                    PostingWithoutTaxAllow = false,
                    BankMasterId = voucherVM.BankMasterId,
                    CashMasterId = voucherVM.CashMasterId,
                    CheckLotDetailId = null,
                    AdvanceDetailId = null,
                    CostCenterId = null,
                    RefCode = null,
                    UpdatedBy = null,
                    UpdatedDate = null,
                    UpdatedFromIP = null,
                    AdvanceWriteOffDetailId = null,
                    VoucherId = voucher.Id,
                    IsPark = voucher.IsPark
                };
                _voucherDetailRepository.Insert(voucherCr);

                    if (!string.IsNullOrEmpty(companyCurrencyId))
                    {
                       
                        voucherDetailCurrencyPk.MaxNumber++;
                        _voucherDetailCurrencyRepository.Insert(new VoucherDetailCurrency
                        {
                            AddedBy = voucherCr.AddedBy,
                            AddedDate = voucherCr.AddedDate,
                            AddedFromIP = voucherCr.AddedFromIP,
                            CrAmount = voucherCr.CrAmount,
                            DrAmount = 0,
                            FromCurrencyId = voucherCr.CurrencyId,
                            Id = voucherDetailCurrencyPk.MaxNumber.ToString(),
                            ModelState = ModelState.Added,
                            ParallelCurrencyId = companyCurrencyId,
                            ToCurrencyConversion = 1 ,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = 1,
                            UpdatedBy = null,
                            UpdatedDate = null,
                            UpdatedFromIP = null,
                            VoucherDetailId = voucherCr.Id,
                            VoucherId = voucherCr.VoucherId
                        });

                        if (companyCurrencyId == voucherVM.BankCurrencyId)
                            voucherVM.BankAmount = voucherCr.CrAmount;
                    }
                   

                if (!string.IsNullOrEmpty(voucherCr.BankMasterId))
                {
                    // INSRT INTO GLTransactionDetail TABLE
                    var glTransactionDetail = new GLTransactionDetail
                    {
                        SourceType = voucherCr.PaymentSource,
                        BankMasterId = voucherVM.BankMasterId,
                        CashMasterId = voucherVM.CashMasterId
                    };
                    _voucherService.InsertGLTransactionDetail(voucherCr, glTransactionDetail);
                }
                if (!string.IsNullOrEmpty(voucherCr.CashMasterId))
                {
                    // INSRT INTO GLTransactionDetail TABLE
                    _glTransactionDetailRepository.Insert(new GLTransactionDetail
                    {
                        AddedBy = voucherCr.AddedBy,
                        AddedDate = voucherCr.AddedDate,
                        AddedFromIP = voucherCr.AddedFromIP,
                        CashMasterId = voucherCr.CashMasterId,
                        CrAmount = 0,
                        DrAmount = voucherVM.BankAmount,
                        Id = base.GetAutoNumber("GLTransactionDetail", PKGeneratorEnum.Auto, null, DateTime.Now),
                        ModelState = ModelState.Added,
                        ReconcileDate = null,
                        ReconcileId = null,
                        UpdatedBy = null,
                        UpdatedDate = null,
                        UpdatedFromIP = null,
                        VoucherDetailId = voucherCr.Id,
                        SourceType = SourceType.CashJournal.ToString()
                    });
                }

                var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceTaxId);
                var inviceTaxDbList = _invoiceTaxRepository.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                var invoiceTaxDetailIds = voucherDetailVMList.Select(r => r.InvoiceTaxDetailId);
                var inviceTaxDetailDbList = _invoiceTaxDetailRepository.Query(r => invoiceTaxDetailIds.Contains(r.Id)).Select().ToList();
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var invoiceTaxDetail = inviceTaxDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceTaxDetailId);
                    if (null == invoiceTaxDetail)
                        throw new CustomException("Invoice not found!");

                    invoiceTaxDetail.WrittenOffAmount += voucherDetailVM.Amount;

                    if (invoiceTaxDetail.Amount < invoiceTaxDetail.WrittenOffAmount)
                        throw new CustomException("Received Amount can not cross Balance Amount");

                    invoiceTaxDetail.IsWrittenOff = invoiceTaxDetail.Amount == invoiceTaxDetail.WrittenOffAmount;
                    invoiceTaxDetail.UpdatedBy = invoiceTaxWriteOff.AddedBy;
                    invoiceTaxDetail.UpdatedDate = invoiceTaxWriteOff.AddedDate;
                    invoiceTaxDetail.UpdatedFromIP = invoiceTaxWriteOff.AddedFromIP;
                    _invoiceTaxDetailRepository.Update(invoiceTaxDetail);

                    // TODO: have a gap here if invoice split
                    var invoiceTax = inviceTaxDbList.First(r => r.Id == invoiceTaxDetail.InvoiceTaxId);
                    invoiceTax.WrittenOffAmount += voucherDetailVM.Amount;
                    invoiceTax.IsWrittenOff = invoiceTax.TaxAmount == invoiceTax.WrittenOffAmount;
                    invoiceTax.UpdatedBy = invoiceTaxWriteOff.AddedBy;
                    invoiceTax.UpdatedDate = invoiceTaxWriteOff.AddedDate;
                    invoiceTax.UpdatedFromIP = invoiceTaxWriteOff.AddedFromIP;
                    _invoiceTaxRepository.Update(invoiceTax);

                    invoiceWriteOffDetailPk.MaxNumber++;
                    // INSERT INTO InvoiceDetail
                    var invoiceWriteOffDetail = new InvoiceTaxWriteOffDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        CurrencyId = voucherDetailVM.CurrencyId,
                        InvoiceTaxWriteOffId = invoiceTaxWriteOff.Id,
                        InvoiceTaxId = voucherDetailVM.InvoiceTaxId,
                        InvoiceTaxDetailId = voucherDetailVM.InvoiceTaxDetailId,

                        Amount = voucherDetailVM.Amount,
                        AddedBy = invoiceTaxWriteOff.AddedBy,
                        AddedDate = invoiceTaxWriteOff.AddedDate,
                        AddedFromIP = invoiceTaxWriteOff.AddedFromIP,
                        Archive = invoiceTaxWriteOff.Archive,
                        ModelState = invoiceTaxWriteOff.ModelState,
                        Id = invoiceWriteOffDetailPk.MaxNumber.ToString(),
                        UpdatedBy = null,
                        UpdatedDate = null,
                        UpdatedFromIP = null,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration
                    };
                    _invoiceTaxWriteOffDetailRepository.Insert(invoiceWriteOffDetail);

                    // in libility side Cr.
                    voucherdetailPk.MaxNumber++;
                    var voucherDr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        CurrencyId = voucher.CurrencyId,
                        EntityId = voucherDetailVM.EntityId,
                        PlantId = _entityRepository.Find(voucherDetailVM.EntityId)?.PlantId,
                        FiscalYearId = voucher.FiscalYearId,
                        FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                        Id = voucherdetailPk.MaxNumber.ToString(),
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP,
                        DrAmount = voucherDetailVM.ConvertedAmount,
                        CrAmount = 0,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        IsPark = voucher.IsPark,
                        Archive = voucher.Archive,
                        ModelState = voucher.ModelState,
                        PostingWithoutTaxAllow = false,
                        EmployeeId = null,
                        PartyId = invoiceTaxWriteOff.PartyId,
                        PartyType = invoiceTaxWriteOff.PartyType,
                        RefCode = null,
                        UpdatedBy = null,
                        UpdatedDate = null,
                        UpdatedFromIP = null,
                        InvoiceDetailId = null,
                        InvoiceTaxWriteOffDetailId = invoiceWriteOffDetail.Id,
                        BankMasterId = null,
                        AdvanceDetailId = null,
                        CheckLotDetailId = null,
                        CostCenterId = null,
                        VoucherId = voucher.Id,
                        AdvanceWriteOffDetailId = null,
                    };
                    _voucherDetailRepository.Insert(voucherDr);

                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        {
                            voucherDetailCurrencyPk.MaxNumber++;
                            _voucherDetailCurrencyRepository.Insert(new VoucherDetailCurrency
                            {
                                AddedBy = voucherCr.AddedBy,
                                AddedDate = voucherCr.AddedDate,
                                AddedFromIP = voucherCr.AddedFromIP,
                                CrAmount = 0,
                                DrAmount = voucherDr.DrAmount,
                                FromCurrencyId = voucherDr.CurrencyId,
                                Id = voucherDetailCurrencyPk.MaxNumber.ToString(),
                                ModelState = ModelState.Added,
                                ParallelCurrencyId = companyCurrencyId,
                                ToCurrencyConversion = 1 ,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = 1,
                                UpdatedBy = null,
                                UpdatedDate = null,
                                UpdatedFromIP = null,
                                VoucherDetailId = voucherDr.Id,
                                VoucherId = voucherDr.VoucherId
                            });
                        }
                       
                }
                base.Insert(invoiceTaxWriteOff);
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

        private string GetInvoiceTaxWriteOffAutoNumber()
        {
            return base.GetAutoNumber("InvoiceTaxWriteOff", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

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

        public IWorkbook GetTaxPayableReport(string companyGroupId, string companyId, string plantId, string plantName, string taxCategoryId, string fromDate, string toDate)
        {
            try
            {
                var row = 6;
                var colLast = 0;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";

                // Get BankMaster data
                var taxCategory = GetTaxCategoryData(taxCategoryId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "TaxCategory");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, taxCategory["TaxCategoryName"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "TaxCode");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, taxCategory["TaxCodeName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Ref No");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, taxCategory["RefNo"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Budget");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, taxCategory["BudgetName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Activity");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                var bankCurrencyId = taxCategory["ActivityName"].ToString();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankCurrencyId);

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "GL");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, taxCategory["GLGeneralInfoCode"] + " - " + taxCategory["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(6) + row].Merge();

                row++;
                sheet.Range[reportUtility.GetColumnNameForXls(6) + row + ":" + reportUtility.GetColumnNameForXls(7) + row].Merge();
                colLast = 8;

                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, 1, "Voucher No", 12);
                reportUtility.SetHeaderText(ref sheet, row, 2, "Posting Date", 10);
                reportUtility.SetHeaderText(ref sheet, row, 3, "DocRef No", 28);
                reportUtility.SetHeaderText(ref sheet, row, 4, "Party", 28);
                reportUtility.SetHeaderText(ref sheet, row, 5, "Narration", 28);
                reportUtility.SetHeaderText(ref sheet, row, 6, "Balance", 9, ExcelHAlign.HAlignRight);

                row++;
                // Get bank transaction data.
                var ledgerData = GetTaxPayableData(companyGroupId, companyId, plantId, taxCategoryId, fromDate, toDate);
                if (ledgerData.Rows.Count > 0)
                {
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        reportUtility.SetText(ref sheet, row, 1, ledgerData.Rows[i]["VoucherNo"].ToString());
                        reportUtility.SetText(ref sheet, row, 2, Convert.ToDateTime(ledgerData.Rows[i]["PostingDate"].ToString()).ToString("dd-MMM-yyyy"));
                        reportUtility.SetText(ref sheet, row, 3, ledgerData.Rows[i]["DocRefNo"].ToString());
                        reportUtility.SetText(ref sheet, row, 4, ledgerData.Rows[i]["PartyName"].ToString());
                        reportUtility.SetText(ref sheet, row, 5, ledgerData.Rows[i]["Narration"].ToString());

                        reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(ledgerData.Rows[i]["Balance"].ToString()));
                        sheet.Range[row, 7].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(5) + row + "-" + reportUtility.GetColumnNameForXls(6) + row + ")";
                        sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    }
                }

                sheet.Range[row, 8].Formula = "=" + reportUtility.GetColumnNameForXls(7) + (row - 1);
                sheet.Range[row, 8].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, 8].CellStyle.Font.Bold = true;

                sheet.Range[11, 4, row, 4].WrapText = true;
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Tax Payable Report", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Dictionary<string, object> GetTaxCategoryData(string taxCategoryId)
        {
            var sql = @"SELECT TC.Code,TC.UserName AS TaxCategoryName, TX.UserName AS TaxCodeName, TX.Id AS TaxCodeId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
						, B.UserName AS BudgetName, BM.Id AS BudgetMasterId,BM.RefNo, A.UserName AS ActivityName
						FROM MST.TaxCategory AS TC
						LEFT JOIN MST.TaxCategoryGL AS TCG ON TCG.TaxCategoryId=TC.Id
						LEFT JOIN MST.TaxCode AS TX ON TX.TaxCategoryId=TC.Id
						LEFT JOIN MST.TaxCodeGL AS TXG ON TXG.TaxCodeId=TX.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=TXG.WithholdCreditableGLId
	                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=TXG.WithholdCreditableBudgetMasterId
	                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
	                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=TXG.WithholdCreditableActivityId
						 WHERE TC.Id='" + taxCategoryId + "'";
            return _sqlRepository.GetData(sql);
        }

        private DataTable GetTaxPayableData(string companyGroupId, string companyId, string plantId, string taxCategoryId, string fromDate, string toDate)
        {
            var cmdText = @"SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName, IVD.BudgetMasterId, B.UserName AS BudgetName, IVD.ActivityId, A.UserName AS ActivityName,
                                           V.VoucherNo, Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11),
	                                       V.PostingDate, 106), ' ', '-') PostingDate, VD.DocRefNo, VD.Narration, V.SourceType, IV.TaxCategoryId, IV.Id AS InvoiceTaxId, IVD.Id AS InvoiceTaxDetailId, VD.VoucherId, VD.EntityId, EN.UserName AS EntityName, VD.PlantId,
                                           VD.Id AS VoucherDetailId, VD.CurrencyId, C.Code AS CurrencyCode, IV.PartyId,PT.UserName AS PartyName, IVD.Amount AS Payable,
                                           IVD.WrittenOffAmount AS Payment, IVD.Amount-IVD.WrittenOffAmount AS Balance,
	                                    	CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, CC.CompanyCurrencyAmount

	                                       FROM [TRN].[InvoiceTaxDetail] AS IVD
                                           LEFT JOIN [TRN].[InvoiceTax] AS IV ON IVD.InvoiceTaxId=IV.Id
                                           LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=IV.VoucherDetailId
                                           LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                           LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
	                                       LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=IVD.BudgetMasterId
	                                       LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
	                                       LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                           LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                                           LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=VD.EntityId
                                           LEFT JOIN [HKP].[Party] AS PT ON PT.Id=IV.PartyId
	                                       LEFT JOIN (
	                                    	SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
	                                    	VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
	                                    	FROM [TRN].[VoucherDetailCurrency] AS VDC
	                                    	JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                                    	WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
	                                     ) AS CC ON CC.VoucherDetailId=VD.Id
                                           WHERE IV.Archive=0
                                           AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
	                                        AND IVD.AType='Cr' AND IV.TaxCategoryId='" + taxCategoryId + @"'
										   AND  CONVERT(DATE,V.PostingDate )  BETWEEN  CONVERT(DATE,'" + fromDate + "') AND  CONVERT(DATE,'" + toDate + "')";
            return _sqlRepository.GetDataTable(cmdText);
        }
    }
}