using Library.Core;
using Library.Data.Repositories;
using Library.Model.Banks;
using Library.Model.Enums;
using Library.ViewModel.Banks;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Library.Service.Banks
{
    public interface IBankJournalNewService
    {
        BankJournal InsertBankJournal(BankJournal bankJournal);

        string InsertBankJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList);

        GridModel GetBankCashPaymentList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);

        GridModel GetBankCashPaymentDetailList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType, string bankJournalId);

        string InsertBankPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);

        string UpdateBankPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);

        string InsertBankReceipt(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string UpdateBankReceipt(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string UpdateBankJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList);

        void UpdateBankJournal(BankJournal bankJournal);

        BankJournal FindBankJournal(string bankJournalId);

        BankJournalDetail FindBankJournalDetail(string bankJournalDetailId);

        void PostBankJournal(string journalId);

        BankJournalDetail InsertBankJournalDetail(BankJournal bankJournal, BankJournalDetail bankJournalDetail, int currentId);

        int GetBankJournalDetailPK(string bankJournalId);

        void UpdateBankJournalDetail(BankJournal bankJournal, BankJournalDetail bankJournalDetail);

        IQueryFluent<BankJournal> GetBankJournalList(Expression<Func<BankJournal, bool>> query);

        IQueryFluent<BankJournalDetail> GetBankJournalDetailList(Expression<Func<BankJournalDetail, bool>> query);

        GridModel GetBankJournalList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);

        GridModel GetBankJVList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);

        GridModel GetBankJournalDetail(GridParameter parameters, string companyGroupId, string companyId, string plantId, string voucherId, string voucherDetailId);

        Dictionary<string, object> GetBankMaster(string bankMasterId);

        Dictionary<string, object> GetBankJournalHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType);

        DataTable GetBankJournalDetail(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType);

        DataTable GetBankLedgerData(string companyGroupId, string companyId, string plantId, string bankMasterId, string fromDate, string toDate);

        DataTable GetBankLedgerData(string companyGroupId, string companyId, string plantId, string bankMasterId, string fromDate, string toDate, bool isOpeningBalance, string fiscalYearId);
        DataTable GetBankReconcileData(string companyGroupId, string companyId, string plantId, string bankMasterId, string fromDate, string toDate);
        List<Dictionary<string, object>> GetBankOpeningBalanceLedgerData(string companyGroupId, string companyId, string plantId, string bankMasterId, string fromDate);

        Dictionary<string, object> GetBankJournal(string bankJournalId);

        List<Dictionary<string, object>> GetBankChargeList(string bankJournalId);

        List<Dictionary<string, object>> GetAdvanceBankChargeList(string bankChargeId);

        GridModel GetAvilabeCustomerPaymentList(GridParameter parameters, string companyGroupId, string companyId, string plantId);
        DataTable GetBankBookLedgerData(string companyGroupId, string companyId, string plantId, string bankMasterId, string fromDate, string toDate);
        void DeleteBankJournal(string bankJournalId, string voucherId);
    }
}