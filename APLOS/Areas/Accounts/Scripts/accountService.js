accountService.$inject = ['$http'];
function accountService($http) {
    var service = {
        getCompanyGLCboList: getCompanyGLCboList
        , getBudgetMasterCboList: getBudgetMasterCboList
        , getCboVoucherTypeBankJournalList: getCboVoucherTypeBankJournalList
        , getCboVoucherTypePaymentByBankList: getCboVoucherTypePaymentByBankList
        , getCboVoucherTypePaymentByCashList: getCboVoucherTypePaymentByCashList
        , getCboVoucherTypeReceiptByBankList: getCboVoucherTypeReceiptByBankList
        , getCboVoucherTypeReceiptByCashList: getCboVoucherTypeReceiptByCashList
        , getCboVoucherTypeJournalVoucherList: getCboVoucherTypeJournalVoucherList
        , getCboVoucherTypePFESICDisbursementVoucherList: getCboVoucherTypePFESICDisbursementVoucherList
        , getCboVoucherTypeLoanList: getCboVoucherTypeLoanList
        , getCboVoucherTypeLoanPaymentList: getCboVoucherTypeLoanPaymentList
        , getCboVoucherTypeInvestmentSetOffList: getCboVoucherTypeInvestmentSetOffList
        , getCboVoucherTypeLoanInterestPayableList: getCboVoucherTypeLoanInterestPayableList
        , getCboVoucherTypeInvestmentList: getCboVoucherTypeInvestmentList
        , getCboSecurityFinancingTypeList: getCboSecurityFinancingTypeList
        , getCboCreditNoteTypeList: getCboCreditNoteTypeList
        , getCboDebitNoteTypeList: getCboDebitNoteTypeList
        , getBudgetMasterActivityCbo: getBudgetMasterActivityCbo
        , getTaxCategoryCbo: getTaxCategoryCbo
        , getTaxCategoryMaterialLevelCbo: getTaxCategoryMaterialLevelCbo
    };

    function base(url, callback) {
        $http.get(url)
            .then(function successCallback(response) {
                callback(response.data);
            }, function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }
    function getBudgetMasterCboList(glId, callback) {
        base('Accounts/BudgetMaster/GetBudgetMasterCboList?glId=' + glId, callback);
    }
    function getCompanyGLCboList(callback) {
        base('Accounts/GLItem/GetCompanyGLCboList', callback);
    }
    function getCboVoucherTypeBankJournalList(callback) {
        base('Accounts/VoucherTypeMatrix/GetCboVoucherTypeBankJournalList', callback);
    }
    function getCboVoucherTypePaymentByBankList(callback) {
        base('Accounts/VoucherTypeMatrix/GetCboVoucherTypePaymentByBankList', callback);
    }
    function getCboVoucherTypePaymentByCashList(callback) {
        base('Accounts/VoucherTypeMatrix/GetCboVoucherTypePaymentByCashList', callback);
    }
    function getCboVoucherTypeReceiptByBankList(callback) {
        base('Accounts/VoucherTypeMatrix/GetCboVoucherTypeReceiptByBankList', callback);
    }
    function getCboVoucherTypeReceiptByCashList(callback) {
        base('Accounts/VoucherTypeMatrix/GetCboVoucherTypeReceiptByCashList', callback);
    }
    function getCboVoucherTypeJournalVoucherList(callback) {
        base('Accounts/VoucherTypeMatrix/GetCboVoucherTypeJournalVoucherList', callback);
    }
    function getCboVoucherTypePFESICDisbursementVoucherList(callback) {
        base('Accounts/VoucherTypeMatrix/GetCboVoucherTypePFESICDisbursementVoucherList', callback);
    }
    function getCboVoucherTypeLoanList(callback) {
        base('Accounts/VoucherTypeMatrix/GetCboVoucherTypeLoanList', callback);
    }
    function getCboVoucherTypeLoanPaymentList(callback) {
        base('Accounts/VoucherTypeMatrix/GetCboVoucherTypeLoanPaymentList', callback);
    }
    function getCboVoucherTypeInvestmentSetOffList(callback) {
        base('Accounts/VoucherTypeMatrix/GetCboVoucherTypeInvestmentSetOffList', callback);
    }
    function getCboVoucherTypeLoanInterestPayableList(callback) {
        base('Accounts/VoucherTypeMatrix/GetCboVoucherTypeLoanInterestPayableList', callback);
    }
    function getCboVoucherTypeInvestmentList(callback) {
        base('Accounts/VoucherTypeMatrix/GetCboVoucherTypeInvestmentList', callback);
    }
    function getCboSecurityFinancingTypeList(callback) {
        base('Accounts/FinancingType/GetCboSecurityFinancingTypeList', callback);
    }
    function getCboCreditNoteTypeList(partyType, callback) {
        base('Accounts/FinancingType/GetCboCreditNoteTypeList?partyType=' + partyType, callback);
    }
    function getCboDebitNoteTypeList(partyType, callback) {
        base('Accounts/FinancingType/GetCboDebitNoteTypeList?partyType=' + partyType, callback);
    }
    function getBudgetMasterActivityCbo(budgetMasterId, callback) {
        base('Accounts/BudgetMaster/GetBudgetMasterActivityCbo?budgetMasterId=' + budgetMasterId, callback);
    }
    function getTaxCategoryCbo(countryId, callback) {
        base('Accounts/TaxCategory/GetCbo?countryId=' + countryId, callback);
    }
    function getTaxCategoryMaterialLevelCbo(countryId, callback) {
        base('Accounts/TaxCategory/GetTaxCategoryMaterialLevelCbo?countryId=' + countryId, callback);
    }
    return service;
}