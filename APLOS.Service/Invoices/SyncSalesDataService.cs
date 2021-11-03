using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.UnitOfWorks;
using Library.Model.ChartOfAccounts;
using Library.Model.Enums;
using Library.Model.Invoices;
using Library.Model.Logs;
using Library.Model.Parties;
using Library.Model.Vouchers;
using Library.Service.Calendars;
using Library.Service.Core;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Taxations;
using Library.Service.Vouchers;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Library.Service.Invoices
{
    public class SyncSalesDataService : Service<SyncRegister>, ISyncSalesDataService
    {
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly ICompanyTaxYearService _companyTaxYearService;
        private readonly IRepositoryAsync<SyncRegister> _salesDataLogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<PartyMapping> _partyMappingRepository;
        private readonly IRepositoryAsync<GLMapping> _glMappingRepository;
        private readonly IRepositoryAsync<CompanyParty> _companyPartyRepository;
        private readonly IRepositoryAsync<CompanyPartyGL> _companyPartyGLRepository;
        private readonly ICompanyFiscalYearService _companyFiscalYearService;
        private readonly IVoucherService _voucherService;
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceTaxService _invoiceTaxService;

        public SyncSalesDataService(
             IRepositoryAsync<SyncRegister> salesDataLogRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IUnitOfWork unitOfWork
            , IRepositoryAsync<PartyMapping> partyMappingRepository
            , IRepositoryAsync<GLMapping> glMappingRepository
            , IRepositoryAsync<CompanyParty> companyPartyRepository
            , IRepositoryAsync<CompanyPartyGL> companyPartyGLRepository
            , ICompanyFiscalYearService companyFiscalYearService
            , IVoucherService voucherService
            , IInvoiceService invoiceService
            , IInvoiceTaxService invoiceTaxService
            , ICompanyTaxYearService companyTaxYearService) : base(salesDataLogRepository, unitOfWork)
        {
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _companyTaxYearService = companyTaxYearService;
            _salesDataLogRepository = salesDataLogRepository;
            _unitOfWork = unitOfWork;
            _partyMappingRepository = partyMappingRepository;
            _glMappingRepository = glMappingRepository;
            _companyPartyRepository = companyPartyRepository;
            _companyPartyGLRepository = companyPartyGLRepository;
            _companyFiscalYearService = companyFiscalYearService;
            _voucherService = voucherService;
            _invoiceService = invoiceService;
            _invoiceTaxService = invoiceTaxService;
        }

        public void Sync(string companyGroupId, string companyId, string plantId, string entityId)
        {
            #region Get Company Parallerl Currency Id

            var syncName = SyncName.Sales.ToString();
            var salesLog = _salesDataLogRepository.Query(r => r.CompanyGroupId == companyGroupId && r.CompanyId == companyId && r.PlantId == plantId && r.SyncName == syncName).Select().FirstOrDefault();
            if (null == salesLog)
                return;
            var voucherVM = new VoucherViewModel
            {
                CompanyGroupId = companyGroupId,
                CompanyId = companyId,
                PlantId = plantId,
                EntityId = entityId,
                PostingDate = salesLog.PostingDate
            };
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
            _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
            _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);
            var headerData = PullHeaderData(salesLog.DatabaseName, salesLog.PostingDate, salesLog.DocumentSeriesCode);

            #endregion Get Company Parallerl Currency Id

            var partyType = PartyType.Customer.ToString();
            var partyGLType = PartyGLType.ReconciliationGL.ToString();

            for (int i = 0; i < headerData.Rows.Count; i++)
            {
                try
                {
                    var isMappingOk = true;
                    string partyGLId = null;
                    string partyBudgetMasterId = null;
                    string partyActivityId = null;
                    string paymentTermId = null;
                    var partyId = headerData.Rows[i]["CustomerId"].ToString();
                    var party = _partyMappingRepository.Query(r => r.CompanyGroupId == companyGroupId && r.CompanyId == companyId && r.PlantId == plantId && r.PartyType == partyType && r.OldPartyId == partyId).Select().FirstOrDefault();
                    if (null == party || string.IsNullOrEmpty(party.PartyId))
                    {
                        if (null == party)
                        {
                            _partyMappingRepository.Insert(new PartyMapping
                            {
                                AddedBy = "TS",
                                AddedDate = DateTime.Now,
                                AddedFromIP = "TS",
                                CompanyGroupId = companyGroupId,
                                CompanyId = companyId,
                                PlantId = plantId,
                                PartyType = partyType,
                                ModelState = ModelState.Added,
                                OldPartyId = partyId,
                                OldPartyName = headerData.Rows[i]["CustomerName"].ToString(),
                                OldPartyAddress = headerData.Rows[i]["OldPartyAddress"].ToString(),
                                OldPartyCity = headerData.Rows[i]["OldPartyCity"].ToString(),
                                OldPartyCountry = headerData.Rows[i]["OldPartyCountry"].ToString(),
                                OldPartyGSTIN = headerData.Rows[i]["OldPartyGSTIN"].ToString(),
                                OldPartyState = headerData.Rows[i]["OldPartyState"].ToString()
                            });
                            _unitOfWork.SaveChanges();
                            Console.WriteLine($"This ({headerData.Rows[i]["CustomerName"]}) party mapping not found!");
                        }
                        else if (string.IsNullOrEmpty(party.OldPartyName) || string.IsNullOrEmpty(party.OldPartyAddress))
                        {
                            party.ModelState = ModelState.Modified;
                            party.UpdatedBy = "TS";
                            party.UpdatedDate = DateTime.Now;
                            party.UpdatedFromIP = "TS";
                            party.OldPartyName = headerData.Rows[i]["CustomerName"].ToString();
                            party.OldPartyAddress = headerData.Rows[i]["OldPartyAddress"].ToString();
                            party.OldPartyCity = headerData.Rows[i]["OldPartyCity"].ToString();
                            party.OldPartyCountry = headerData.Rows[i]["OldPartyCountry"].ToString();
                            party.OldPartyGSTIN = headerData.Rows[i]["OldPartyGSTIN"].ToString();
                            party.OldPartyState = headerData.Rows[i]["OldPartyState"].ToString();
                            _partyMappingRepository.Update(party);
                            _unitOfWork.SaveChanges();
                            Console.WriteLine($"This ({headerData.Rows[i]["CustomerName"]}) party still not mapped!");
                        }
                        Console.WriteLine($"This ({headerData.Rows[i]["CustomerName"]}) party still not mapped!");
                        isMappingOk = false;
                        break;
                    }
                    else
                    {
                        var companyParty = _companyPartyRepository.Query(r => r.CompanyId == party.CompanyId && r.PlantId == party.PlantId && r.PartyId == party.PartyId && r.PartyType == partyType).Select().FirstOrDefault();
                        if (null == companyParty)
                        {
                            Console.WriteLine("Plant party mapping not found!");
                            break;
                        }
                        else
                        {
                            var companyPartyGL = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id && r.PartyGLType == partyGLType).Select().FirstOrDefault();
                            if (null == companyPartyGL)
                            {
                                Console.WriteLine("Party reconciliation gl not found!");
                                break;
                            }
                            else
                            {
                                partyGLId = companyPartyGL.GLGeneralInfoId;
                                partyBudgetMasterId = companyPartyGL.BudgetMasterId;
                                partyActivityId = companyPartyGL.ActivityId;
                                paymentTermId = companyParty.PaymentTermId;
                            }
                        }
                    }
                    var documentId = headerData.Rows[i]["DocumentId"].ToString();
                    var postingDataList = PullPostingData(salesLog.DatabaseName, documentId);
                    var glMappingMappingList = new List<GLMapping>();
                    for (int p = 0; p < postingDataList.Rows.Count; p++)
                    {
                        if (postingDataList.Rows[p]["AmountSign"].ToString() == "C")
                        {
                            var accountId = postingDataList.Rows[p]["AccountId"].ToString();
                            var glMapping = _glMappingRepository.Query(r => r.CompanyGroupId == companyGroupId && r.CompanyId == companyId && r.PlantId == plantId && r.OldGLId == accountId).Include(r => r.BudgetMaster).Select().FirstOrDefault();
                            if (null == glMapping || string.IsNullOrEmpty(glMapping.BudgetMasterId))
                            {
                                if (null == glMapping)
                                {
                                    _glMappingRepository.Insert(new GLMapping
                                    {
                                        AddedBy = "TS",
                                        AddedDate = DateTime.Now,
                                        AddedFromIP = "TS",
                                        CompanyGroupId = companyGroupId,
                                        CompanyId = companyId,
                                        PlantId = plantId,
                                        ModelState = ModelState.Added,
                                        OldGLId = accountId,
                                        OldGLCode = postingDataList.Rows[p]["AccountCode"].ToString(),
                                        OldGLName = postingDataList.Rows[p]["AccountName"].ToString(),
                                        PartyType = partyType
                                    });
                                    _unitOfWork.SaveChanges();
                                }
                                if (isMappingOk)
                                    isMappingOk = false;
                                Console.WriteLine($"This ({postingDataList.Rows[p]["AccountName"].ToString()}) GL mapping not found!");
                                break;
                            }
                            else
                                glMappingMappingList.Add(glMapping);
                        }
                    }
                    if (isMappingOk)
                    {
                        var pDocumentNo = headerData.Rows[i]["PDocumentNo"].ToString();
                        if (!_voucherService.Any(r => r.TransactionRefNo == pDocumentNo))
                        {
                            Console.WriteLine("Sync PDocumentNo: " + pDocumentNo);
                            var invoice = new Invoice
                            {
                                CompanyGroupId = companyGroupId,
                                CompanyId = companyId,
                                PlantId = plantId,
                                EntityId = entityId,
                                PartyId = party.PartyId,
                                PartyPlantId = party.PartyPlantId,
                                PaymentTermId = paymentTermId,
                                VoucherTypeId = "18",
                                FiscalYearId = voucherVM.FiscalYearId,
                                FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                                TaxYearId = voucherVM.TaxYearId,
                                TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                                PartyType = PartyType.Customer.ToString(),
                                SourceType = SourceType.CustomerInvoice.ToString(),
                                AddedBy = headerData.Rows[i]["CreatedBy"].ToString(),
                                AddedDate = Convert.ToDateTime(headerData.Rows[i]["LastTransactionDatetime"].ToString()),
                                AddedFromIP = "TS",
                                VoucherDate = Convert.ToDateTime(headerData.Rows[i]["DocumentDate"].ToString()),
                                PostingDate = Convert.ToDateTime(headerData.Rows[i]["DocumentDate"].ToString()),
                                DocDate = Convert.ToDateTime(headerData.Rows[i]["PostingDateTime"].ToString()),
                                DocRefNo = headerData.Rows[i]["DocumentNo"].ToString(),
                                Narration = headerData.Rows[i]["Remarks"].ToString(),
                                Amount = Convert.ToDecimal(headerData.Rows[i]["DocumentAmount"].ToString()),
                                BaseNoOfDays = 30,
                                CompanyCurrencyRate = Convert.ToDecimal(headerData.Rows[i]["ConversionRate"].ToString())
                            };
                            invoice.BaseOnDueDate = invoice.PostingDate.AddDays(invoice.BaseNoOfDays);
                            invoice.ActualDueDate = invoice.BaseOnDueDate;
                            invoice.SalesOrderNo = GetPINumber(salesLog.DatabaseName, documentId);
                            var cyrrencyId = headerData.Rows[i]["CurrencyId"].ToString().ToUpper();
                            // USD
                            if (cyrrencyId == "8B5CEFCD-9E33-4991-A033-37EA8C243595" ||
                                cyrrencyId == "F6EBA5A2-414E-479A-834A-719521242364")
                                invoice.CurrencyId = "201712";
                            // INR
                            else if (cyrrencyId == "3BF9B34D-57F5-4EF0-9546-F18A2B79419D")
                                invoice.CurrencyId = "20178";
                            // EURO
                            else if (cyrrencyId == "EABE7576-D811-4090-B772-7EF2B86C056B")
                                invoice.CurrencyId = "20174";
                            // GBP/POUND
                            else if (cyrrencyId == "CD0BFC7B-3B9C-43FC-9981-8C141C4D790D")
                                invoice.CurrencyId = "20171";

                            _invoiceService.InsertInvoice(invoice);

                            // INSERT INTO Voucher TABLE
                            var voucher = new Voucher
                            {
                                CompanyGroupId = invoice.CompanyGroupId,
                                CompanyId = invoice.CompanyId,
                                PlantId = invoice.PlantId,
                                EntityId = invoice.EntityId,
                                CurrencyId = invoice.CurrencyId,
                                VoucherDate = invoice.PostingDate,
                                PostingDate = invoice.PostingDate,
                                DocDate = invoice.DocDate,
                                DocRefNo = invoice.DocRefNo,
                                Archive = invoice.Archive,
                                IsPark = invoice.IsPark,
                                AddedBy = invoice.AddedBy,
                                AddedDate = invoice.AddedDate,
                                AddedFromIP = invoice.AddedFromIP,
                                Narration = invoice.Narration,
                                SourceType = invoice.SourceType,
                                ModelState = invoice.ModelState,
                                FiscalYearId = invoice.FiscalYearId,
                                FiscalYearPeriodId = invoice.FiscalYearPeriodId,
                                TaxYearId = invoice.TaxYearId,
                                TaxYearPeriodId = invoice.TaxYearPeriodId,
                                VoucherTypeId = invoice.VoucherTypeId,
                                TransactionRefNo = headerData.Rows[i]["PDocumentNo"].ToString()
                            };
                            _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, false);
                            invoice.VoucherId = voucher.Id;

                            var currentVoucherDetailId = 0;
                            var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                            for (int p = 0; p < postingDataList.Rows.Count; p++)
                            {
                                if (postingDataList.Rows[p]["AmountSign"].ToString() == "C")
                                {
                                    var accountId = postingDataList.Rows[p]["AccountId"].ToString().ToLower();
                                    var glMapping = glMappingMappingList.FirstOrDefault(r => r.OldGLId.ToLower() == accountId);
                                    if (postingDataList.Rows[p]["Naration"].ToString() == "CGST" || postingDataList.Rows[p]["Naration"].ToString() == "SGST" || postingDataList.Rows[p]["Naration"].ToString() == "IGST")
                                    {
                                        var invoiceTax = new InvoiceTax
                                        {
                                            TaxAmount = Convert.ToDecimal(postingDataList.Rows[p]["Amount"]),
                                            TaxAutoAmount = Convert.ToDecimal(postingDataList.Rows[p]["Amount"])
                                        };
                                        if (postingDataList.Rows[p]["Naration"].ToString() != "CGST")
                                            invoiceTax.TaxCategoryId = "5";
                                        else if (postingDataList.Rows[p]["Naration"].ToString() != "SGST")
                                            invoiceTax.TaxCategoryId = "6";
                                        else if (postingDataList.Rows[p]["Naration"].ToString() != "IGST")
                                            invoiceTax.TaxCategoryId = "4";
                                        _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk);

                                        var invoiceTaxDetail = new InvoiceTaxDetail
                                        {
                                            GLGeneralInfoId = glMapping.BudgetMaster.GLGeneralInfoId,
                                            BudgetMasterId = glMapping.BudgetMasterId,
                                            ActivityId = glMapping.ActivityId,
                                            Amount = invoiceTax.TaxAmount,
                                            AType = "Cr"
                                        };
                                        _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                                        var voucherDetailCr = new VoucherDetail
                                        {
                                            InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                            GLGeneralInfoId = glMapping.BudgetMaster.GLGeneralInfoId,
                                            BudgetMasterId = glMapping.BudgetMasterId,
                                            ActivityId = glMapping.ActivityId,
                                            CrAmount = Convert.ToDecimal(postingDataList.Rows[p]["Amount"]) / invoice.CompanyCurrencyRate,
                                            Narration = postingDataList.Rows[p]["Naration"].ToString(),
                                            PostingWithoutTaxAllow = invoice.IsExcludingTax,
                                            TrnNature = TransactionNature.SalesTax.ToString()
                                        };
                                        currentVoucherDetailId++;
                                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                                        invoiceTax.VoucherDetailId = voucherDetailCr.Id;

                                        // INSERT INTO VoucherDetailCurrency
                                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                                        {
                                            ParallelCurrencyId = companyCurrencyId,
                                            FromCurrencyId = voucherDetailCr.CurrencyId,
                                            ToCurrencyId = companyCurrencyId,
                                            ToCurrencyRate = Convert.ToDecimal(headerData.Rows[i]["ConversionRate"].ToString()),
                                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, invoice.CompanyCurrencyRate),
                                            CrAmount = Convert.ToDecimal(postingDataList.Rows[p]["Amount"])
                                        });
                                    }
                                    else
                                    {
                                        var voucherDetailCr = new VoucherDetail
                                        {
                                            GLGeneralInfoId = glMapping.BudgetMaster.GLGeneralInfoId,
                                            BudgetMasterId = glMapping.BudgetMasterId,
                                            ActivityId = glMapping.ActivityId,
                                            CrAmount = Convert.ToDecimal(postingDataList.Rows[p]["Amount"]) / invoice.CompanyCurrencyRate,
                                            Narration = postingDataList.Rows[p]["Naration"].ToString(),
                                            PostingWithoutTaxAllow = invoice.IsExcludingTax,
                                            TrnNature = TransactionNature.Sales.ToString()
                                        };
                                        currentVoucherDetailId++;
                                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                                        // INSERT INTO VoucherDetailCurrency
                                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                                        {
                                            ParallelCurrencyId = companyCurrencyId,
                                            FromCurrencyId = voucherDetailCr.CurrencyId,
                                            ToCurrencyId = companyCurrencyId,
                                            ToCurrencyRate = Convert.ToDecimal(headerData.Rows[i]["ConversionRate"].ToString()),
                                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, invoice.CompanyCurrencyRate),
                                            CrAmount = Convert.ToDecimal(postingDataList.Rows[p]["Amount"])
                                        });
                                    }
                                }
                                else if (postingDataList.Rows[p]["AmountSign"].ToString() == "D")
                                {
                                    var invoiceDetail = new InvoiceDetail
                                    {
                                        GLGeneralInfoId = partyGLId,
                                        BudgetMasterId = partyBudgetMasterId,
                                        ActivityId = partyActivityId,
                                        Amount = Convert.ToDecimal(postingDataList.Rows[p]["Amount"]) / invoice.CompanyCurrencyRate,
                                        NetAmount = Convert.ToDecimal(postingDataList.Rows[p]["Amount"]) / invoice.CompanyCurrencyRate
                                    };
                                    invoice.Amount = invoiceDetail.NetAmount;
                                    _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, 1);

                                    var voucherDetailDr = new VoucherDetail
                                    {
                                        GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                                        BudgetMasterId = invoiceDetail.BudgetMasterId,
                                        ActivityId = invoiceDetail.ActivityId,
                                        DrAmount = invoiceDetail.NetAmount,
                                        Narration = invoice.Narration,
                                        EmployeeId = invoice.EmployeeId,
                                        InvoiceDetailId = invoiceDetail.Id,
                                        PartyId = invoice.PartyId,
                                        PartyPlantId = invoice.PartyPlantId,
                                        PartyType = invoice.PartyType,
                                        PostingWithoutTaxAllow = invoice.IsExcludingTax,
                                        TrnNature = TransactionNature.ToCustomer.ToString()
                                    };
                                    currentVoucherDetailId++;
                                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                                    // INSERT INTO VoucherDetailCurrency
                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                                    {
                                        ParallelCurrencyId = companyCurrencyId,
                                        FromCurrencyId = voucherDetailDr.CurrencyId,
                                        ToCurrencyId = companyCurrencyId,
                                        ToCurrencyRate = Convert.ToDecimal(headerData.Rows[i]["ConversionRate"].ToString()),
                                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, invoice.CompanyCurrencyRate),
                                        DrAmount = Convert.ToDecimal(postingDataList.Rows[p]["Amount"])
                                    });
                                }
                            }
                            _unitOfWork.SaveChanges();
                        }
                        else
                            Console.WriteLine("Skiping PDocumentNo: " + pDocumentNo);
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
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
                }
            }

            if (salesLog.PostingDate != DateTime.Today)
            {
                salesLog.PostingDate = salesLog.PostingDate.AddDays(1);
                salesLog.ProcessDate = DateTime.Now;
                base.Update(salesLog);
            }
        }

        public static DataTable PullHeaderData(string databaseName, DateTime postigDate, string documentSeriesCode)
        {
            var query = @"SELECT DH.Id AS DocumentId, DH.DocumentDate, DH.DocumentNo, DH.FYID, DH.CreatedBy, DH.LastTransactionUser, DH.LastTransactionDatetime, DH.LastTransactionType, DH.Post, DH.PostedBy
                        , DH.TempDocumentNo, DH.DocumentSeriesCode, DH.PreFix, DH.TDocumentNo, DH.PDocumentNo, DH.PostingDateTime, DH.Remarks, IH.CustomerId, IH.[CustomerName], C.GSTIN AS [OldPartyGSTIN]
                        , C.BillingCountry AS [OldPartyCountry], C.StateCode+' - '+C.BillingState AS [OldPartyState], C.BillingAddressLine2 AS [OldPartyCity], C.BillingAddress AS [OldPartyAddress], IH.CurrencyId
                        , IH.ConversionRate, IH.DocumentAmount
                        FROM DocumentHeaders AS DH
                        LEFT JOIN InvoiceHeader AS IH ON IH.Id=DH.Id
                        LEFT JOIN [OrderProcessing].[Customer] AS C ON C.CustomerID=IH.CustomerId
                        WHERE DH.DocumentSeriesCode='" + documentSeriesCode + "'" +
                        "AND CONVERT(DATE, DH.DocumentDate) = '" + postigDate.ToDbDate() + "' ORDER BY DH.DocumentNo ASC";
            return PullData(databaseName, query);
        }

        public static string GetPINumber(string databaseName, string documentId)
        {
            var query = @"SELECT TOP(1) P.PISeries, P.FYID, P.PINo FROM [dbo].[InvoiceLineItems] AS ILI
                        LEFT JOIN [SD].[PIArticles] AS PIA ON ILI.OtherRefLineId=PIA.PIArticleID
                        LEFT JOIN [SD].[PI] AS P ON P.PIID=PIA.PIID
                        where ILI.DocumentId='" + documentId + "' AND OtherRefLineId IS NOT NULL ORDER BY ILI.RefNo";
            var data = PullData(databaseName, query);
            return data.Rows.Count > 0 ? data.Rows[0]["PISeries"] + "-" + data.Rows[0]["FYID"] + "-" + data.Rows[0]["PINo"] : null;
        }

        public static DataTable PullPostingData(string databaseName, string documentId)
        {
            var query = @"SELECT DP.AccountId, DP.AmountSign, DP.Naration, SUM(DP.Amount) AS Amount, GL.AccountCode, GL.AccountName FROM DocumentPostings AS DP
                        LEFT JOIN FI.GLAccounts AS GL ON GL.Id=DP.AccountId
                        WHERE DP.DocumentId='" + documentId + @"'
                        GROUP BY DP.AccountId, DP.AmountSign, DP.Naration, GL.AccountCode, GL.AccountName
                        ORDER BY 2, 3";
            return PullData(databaseName, query);
        }

        public static DataTable PullData(string databaseName, string commandText)
        {
#if DEBUG
            var connString = @"server=RIKHAN0706\DEV17;uid=sa;pwd=Aplos123_; database=" + databaseName + " ";
#else
            var connString = @"server=192.168.2.200;uid=sa;pwd=abhi123456@; database=" + databaseName + " ";
#endif
            var dataTable = new DataTable();
            var conn = new SqlConnection(connString);
            var cmd = new SqlCommand(commandText, conn);
            conn.Open();
            var da = new SqlDataAdapter(cmd);
            da.Fill(dataTable);
            conn.Close();
            da.Dispose();
            return dataTable;
        }
    }
}