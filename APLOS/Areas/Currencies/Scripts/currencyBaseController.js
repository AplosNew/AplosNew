currencyBaseController.$inject = ['$scope', '$http', 'cboService', 'baseService'];
function currencyBaseController($scope, $http, cboService, baseService) {
    $scope.tranCurrencyList = [];
    if ($scope.isWriteOff) {
        cboService.getCboParallelCurrency(function (result) {
            $scope.tranCurrencyList = result;
        });
    }
    else {
        cboService.getCboTransactionCurrencyByCompany('', function (result) {
            $scope.tranCurrencyList = result;
        });
    }

    $scope.selectBaseCurrency = function () {
        var currencyId = '';
        angular.forEach($scope.tranCurrencyList, function (item, i) {
            if (item.IsBaseCurrency === 1) {
                currencyId = item.CurrencyId;
            }
        });
        return currencyId;
    };

    if (baseService.isUndefinedOrNull($scope.source)) {
        $scope.source = 'DocRef';
    }
    if (baseService.isUndefinedOrNull($scope.hideSource)) {
        $scope.hideSource = false;
    }

    // Creating parallel currency table heading.
    $scope.parallelCurrencyTableHead += '<tr>' +
        '<th rowspan="2" ng-hide=' + $scope.hideSource + '>' + $scope.source + '</th>' +
        '<th rowspan="2">GL</th>' +
        '<th rowspan="2" ng-show="companyConfig.IsVoucherFromBudget">Budget</th>' +
        '<th rowspan="2" ng-show="companyConfig.IsVoucherFromBudget">Activity</th>';
    var debitCreditHead = '</tr><tr>';
    $scope.parallelCurrencyTypeList = [];
    $scope.companyCurrencyId = null;
    $scope.companyGroupCurrencyId = null;
    $scope.hardCurrencyId = null;
    $http.get('currencies/CompanyParallelCurrency/CurrencyParallel')
        .then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.showform = true;
                angular.forEach(response.data, function (item, i) {
                    $scope.parallelCurrencyTableHead += '<th style="text-align:center" colspan="2">' + item.Code + '</th>';
                    debitCreditHead += '<th>Dr</th><th>Cr</th>';
                    if (item.ParallelCurrencyType === 'CompanyCurrency') {
                        $scope.companyCurrencyId = item.CurrencyId;
                        $scope.companyCurrencyCode = item.Code;
                        $scope.companyCurrencyName = item.Code;
                        $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyCurrency', CurrencyType: 'CompanyCurrencyDr', CurrencyId: item.CurrencyId });
                        $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyCurrency', CurrencyType: 'CompanyCurrencyCr', CurrencyId: item.CurrencyId });
                    }
                    else if (item.ParallelCurrencyType === 'CompanyGroupCurrency') {
                        $scope.companyGroupCurrencyId = item.CurrencyId;
                        $scope.companyGroupCurrencyCode = item.Code;
                        $scope.companyGroupCurrencyName = item.Code;
                        $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyGroupCurrency', CurrencyType: 'CompanyGroupCurrencyDr', CurrencyId: item.CurrencyId });
                        $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyGroupCurrency', CurrencyType: 'CompanyGroupCurrencyCr', CurrencyId: item.CurrencyId });
                    }
                    else if (item.ParallelCurrencyType === 'HardCurrency') {
                        $scope.hardCurrencyId = item.CurrencyId;
                        $scope.hardCurrencyCode = item.Code;
                        $scope.hardCurrencyName = item.Code;
                        $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'HardCurrency', CurrencyType: 'HardCurrencyDr', CurrencyId: item.CurrencyId });
                        $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'HardCurrency', CurrencyType: 'HardCurrencyCr', CurrencyId: item.CurrencyId });
                    }
                });
            }
            else {
                ShowResult('Company Parallel Currency is not set!', 'failure');
                $scope.showform = false;
            }
            $scope.parallelCurrencyTableHead += debitCreditHead + '</tr>';
        });
}