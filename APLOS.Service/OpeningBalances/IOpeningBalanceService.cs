using Library.Core;
using Library.Model.Enums;
using Library.Model.OpeningBalances;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;

namespace Library.Service.OpeningBalances
{
    public interface IOpeningBalanceService : IService<OpeningBalance>
    {
        #region Journal

        List<Dictionary<string, object>> GetSummaryData(string companyGroupId, string companyId, string plantId);

        List<Dictionary<string, object>> GetAvailableForJournal(string companyGroupId, string companyId, string plantId);

        void InsertJournal(Voucher entity, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);

        GridModel GetJournalList(GridParameter parameters, string companyId, string plantId);

        #endregion Journal

        #region AdvanceJournal
        string InsertAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string UpdateAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string InsertGLAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string PostAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string PostInsertAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        List<Dictionary<string, object>> GetMaterialMasterOBGL(string openingBalanceId, string companyGroupId, string companyId, string plantId);
        #endregion

        #region FixedAsset

        List<Dictionary<string, object>> GetMaterialMasterOpeningBalanceDetailList(string companyId, string plantId, string openinngBalanceId);

        void InsertFixedAsset(OpeningBalance openingBalance, IEnumerable<MaterialMasterOpeningBalanceDetailViewModel> materialMasterOpeningBalanceDetailVMList);

        void UpdateFixedAsset(OpeningBalance openingBalance, IEnumerable<MaterialMasterOpeningBalanceDetailViewModel> materialMasterOpeningBalanceDetailVMList);

        void DeleteFixedAsset(string id);

        void DeleteOPDetail(string id);

        #endregion FixedAsset

        #region MaterialMaster
        List<Dictionary<string, object>> GetMaterialMasterOBDetailList(string companyId, string plantId, string openinngBalanceId);
        void InsertMaterialMaster(OpeningBalance openingBalance, IEnumerable<MaterialMasterOpeningBalanceDetailViewModel> materialMasterOpeningBalanceDetailVMList);

        void UpdateMaterialMaster(OpeningBalance openingBalance, IEnumerable<MaterialMasterOpeningBalanceDetailViewModel> materialMasterOpeningBalanceDetailVMList);

        void DeleteMaterialMaster(string id);
        string PostNonFinancialMaterialOB(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        GridModel GetNonFinancialMaterialPostedList(GridParameter parameters, string companyId, string plantId);
        #endregion MaterialMaster

        void UpdateInterInvestmentGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList);

        void UpdateInterCompanyTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList);

        void UpdateInterPlantTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList);

        void DeleteInter(string id);

        GridModel GetOpeningBalance(GridParameter parameters, string companyId);

        GridModel GetInterLoanGivenList(GridParameter parameters, string companyGroupId, string companyId, string plantId);

        GridModel QueryAsset(GridParameter parameters, string sourceType, string companyGroupId, string companyId, string plantId);
        GridModel GetMaterialMasterOB(GridParameter parameters, string sourceType, string companyGroupId, string companyId, string plantId);
        GridModel GetNonFinancialMaterialMasterOB(GridParameter parameters, string sourceType, string companyGroupId, string companyId, string plantId);
        GridModel Query(GridParameter parameters, string sourceType, string companyGroupId, string companyId, string plantId, string transactionType);

        GridModel GetInterPlantInvestmentTakenList(GridParameter parameters, string sourceType, string companyGroupId, string companyId, string plantId);

        GridModel GetInterInvestmentGivenList(GridParameter parameters, string companyGroupId, string companyId, string plantId);

        List<Dictionary<string, object>> GetOpeningBalanceDetailList(string companyId, string openingBalanceId, string sort);

        List<Dictionary<string, object>> GetMMOpeningBalanceDetailList(string companyId, string openingBalanceId);

        void Insert(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList);

        void InsertInterLoanGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList);

        void UpdateInterLoanGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList);

        void InsertInterInvestmentGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList);

        void UpdateInterPlantInvestmentTaken(OpeningBalance openingBalance);

        void UpdateInterCompanyInvestmentTaken(OpeningBalance openingBalance);

        void Update(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList);

        #region Report

        IWorkbook GetOpeningBalanceJournal(string companyId, string plantName, string voucherId);


        IWorkbook CreatePaybleVSpaymentReportSheet(string companyId, string plantId, string fromDate);
        IWorkbook GetOpeningBalanceReport(string companyId, string plantName, string[] parallelCurrencies);

        #endregion Report

        void InsertInterTransactionGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList);

        void UpdateInterTransactionGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList);


        List<Dictionary<string, object>> GetOBLoanTakenDetailGL(string companyGroupId, string companyId, string plantId, string openingBalanceId);
        List<Dictionary<string, object>> GetOBLoanGivenDetailGL(string companyGroupId, string companyId, string plantId, string openingBalanceId);
        List<Dictionary<string, object>> GetOBSecurityGivenDetailGL(string companyGroupId, string companyId, string plantId, string openingBalanceId);
        List<Dictionary<string, object>> GetOBSecurityTakenDetailGL(string companyGroupId, string companyId, string plantId, string openingBalanceId);
        List<Dictionary<string, object>> GetOBEquityDetailGL(string companyGroupId, string companyId, string plantId, string openingBalanceId);
        List<Dictionary<string, object>> GetOBInvestmentDetailGL(string companyGroupId, string companyId, string plantId, string openingBalanceId);

        string PostDeleteAccCutOffDateBackData(VoucherViewModel voucherVM);
        List<Dictionary<string, object>> GetCutOffBackDateData(string companyGroupId, string companyId, string plantId, DateTime cutOffDate);
        List<Dictionary<string, object>> GetEmployeePayableCutOffAfterPostingDateData(string companyGroupId, string companyId, string plantId, DateTime cutOffDate);
        List<Dictionary<string, object>> GetVendorPayableCutOffAfterPostingDateData(string companyGroupId, string companyId, string plantId, DateTime cutOffDate);
        string DeleteVendorPayableCutOffAfterPostingDateData(IEnumerable<VoucherDetailViewModel> voucherDetailVM);
        string DeleteEmployeePayableCutOffAfterPostingDateData(IEnumerable<VoucherDetailViewModel> voucherDetailVM);


        IWorkbook CreateMaterialMasterOpeningBalanceReport(string companyId, string plantId, string fromDate, string toDate);
    }
}