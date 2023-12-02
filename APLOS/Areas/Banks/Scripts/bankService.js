bankService.$inject = ["$http"];
function bankService($http) {
    var service = {
        base: base
        , getBankMasterHouseBankCboList: getBankMasterHouseBankCboList
        , getInvestmentBankMasterCbo: getInvestmentBankMasterCbo
        , getBankMasterLoanBankCboList: getBankMasterLoanBankCboList
        , getBankMasterHouseBankCboListByEntity: getBankMasterHouseBankCboListByEntity
        , getCashMasterCboList: getCashMasterCboList
        , getCashMasterCboListByEntity: getCashMasterCboListByEntity
        , getCboBankChargeTypeList: getCboBankChargeTypeList
        , getCboBankChargeTypeSourceDeductionList: getCboBankChargeTypeSourceDeductionList
        , getCboVoucherTypeCashJournalList: getCboVoucherTypeCashJournalList
        , getCboVoucherTypeCashExpensesList: getCboVoucherTypeCashExpensesList
        , getBankMasterCboListByPlant: getBankMasterCboListByPlant
        , getNegotiatingBankMasterCboListByPlant: getNegotiatingBankMasterCboListByPlant
    };

    function base(url, callback) {
        $http.get(url)
            .then(function successCallback(response) {
                callback(response.data);
            }, function errorCallback(response) {
                ShowResult(response, "failure");
            });
    }

    function getNegotiatingBankMasterCboListByPlant(callback) {
        base("Banks/BankMaster/GetNegotiatingBankMasterCboListByPlant", callback);
    }

    function getBankMasterCboListByPlant(callback) {
        base("Banks/BankMaster/GetBankMasterCboListByPlant", callback);
    }
    function getBankMasterHouseBankCboList(callback) {
        base("Banks/BankMaster/GetBankMasterHouseBankCboList", callback);
    }
    function getInvestmentBankMasterCbo(callback) {
        base("Banks/BankMaster/GetInvestmentBankMasterCbo", callback);
    }

    function getBankMasterLoanBankCboList(callback) {
        base("Banks/BankMaster/GetBankMasterLoanBankCboList", callback);
    }

    function getBankMasterHouseBankCboListByEntity(entityId, callback) {
        base("Banks/BankMaster/GetBankMasterCboListByEntity?entityId=" + entityId, callback);
    }

    function getCashMasterCboList(callback) {
        base("Banks/CashMaster/GetCashMasterCboList", callback);
    }

    function getCashMasterCboListByEntity(entityId, callback) {
        base("Banks/CashMaster/GetCashMasterCboListByEntity?entityId=" + entityId, callback);
    }

    function getCboBankChargeTypeList(callback) {
        base("Banks/BankChargeType/GetCboBankChargeTypeList", callback);
    }

    function getCboBankChargeTypeSourceDeductionList(callback) {
        base("Banks/BankChargeType/GetCboBankChargeTypeSourceDeductionList", callback);
    }

    function getCboVoucherTypeCashJournalList(callback) {
        base("Accounts/VoucherTypeMatrix/GetCboVoucherTypeCashJournalList", callback);
    }
    function getCboVoucherTypeCashExpensesList(callback) {
        base("Accounts/VoucherTypeMatrix/GetCboVoucherTypeCashExpensesList", callback);
    }
    return service;
}