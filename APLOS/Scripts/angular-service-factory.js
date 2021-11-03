factoryService.$inject = ['$http', '$window', '$rootScope', 'baseService'];
function factoryService($http, $window, $rootScope, baseService) {
    var service = {
        getCurrencyPrecision: getCurrencyPrecision
        , getBankMasterGL: getBankMasterGL
        , getCashMasterGL: getCashMasterGL
    };

    function base(url, callback) {
        $http.get(url)
            .then(function successCallback(response) {
                callback(response.data);
            }, function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCurrencyPrecision(currencyId) {
        $rootScope.currencyPrecision = 0;
        if (baseService.isUndefinedOrNull(currencyId)) return ShowResult('Base currency not found.', 'failure');
        $http.get('Currencies/CompanyParallelCurrency/getCurrencyPrecision?currencyId=' + currencyId)
            .then(function successCallback(response) {
                $rootScope.currencyPrecision = response.data;
            }, function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getBankMasterGL(bankMasterId, callback) {
        base('Banks/BankMaster/GetBankMasterGL?bankMasterId=' + bankMasterId, callback);
    }

    function getCashMasterGL(cashMasterId, callback) {
        base('Banks/CashMaster/GetCashMasterGL?cashMasterId=' + cashMasterId, callback);
    }

    //function getCurrencyPrecision(companyId, callback) {
    //    if (baseService.isUndefinedOrNull(companyId)) {
    //        if (!baseService.isUndefinedOrNull($window.companyId)) companyId = $window.companyId;
    //        else companyId = null;
    //    }
    //    base('Currencies/CompanyParallelCurrency/getCurrencyPrecision?companyId=' + companyId, callback);
    //};

    return service;
}