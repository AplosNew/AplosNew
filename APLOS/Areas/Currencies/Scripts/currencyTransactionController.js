'use strict';
CurrencyTransactionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function CurrencyTransactionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Currency Transaction';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.transactionCurrencies = [];
    $scope.path = 'currencies/transactioncurrency/';
    $scope.getListUrl = $scope.path + 'gettransactioncurrencylist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Currency', 'Currency');
    $scope.getData = function (pageno) {
        $rootScope.parameters.companyId = $scope.transactionCurrency.CompanyId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.transactionCurrencies = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    //$scope.getData();
    $scope.currencyList = [];
    cboService.getCompanyGroupCurrencyCbo(null, function (result) {
        $scope.currencyList = result;
    });

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.transactionCurrency = {
        Id: null,
        CurrencyId: null,
        CompanyId: null,
        CurrencyCode: null,
        Currency: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter('date')(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        UpdatedDate: $filter('date')(Date.now(), 'yyyy-MM-dd')
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.transactionCurrency = $scope.transactionCurrencies[$scope.index];
        $scope.transactionCurrency.AddedDate = $filter('dateFilter')($scope.transactionCurrency.AddedDate);
        $scope.transactionCurrency.UpdatedDate = $filter('dateFilter')($scope.transactionCurrency.UpdatedDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $rootScope.searchtransactioncurrencyList = [
        {
            'name': 'CurrencyCode',
            'value': 'CurrencyCode'
        },
        {
            'name': 'Currency',
            'value': 'Currency'
        }
    ];
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.transactionCurrencyForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.transactionCurrency,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.transactionCurrencies.push($scope.transactionCurrency);
                        $scope.getData();
                        $scope.transactionCurrencies = $filter('orderBy')($scope.transactionCurrencies, 'Currency');
                        $scope.transactionCurrencies.Active = true;
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.transactionCurrency,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.transactionCurrencies.Active = true;
                            $scope.transactionCurrencies[$scope.index] = $scope.transactionCurrency;
                            $scope.transactionCurrencies = $filter('orderBy')($scope.transactionCurrencies, 'Currency');
                        }
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.transactionCurrency.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + 'delete?id=' + $scope.transactionCurrency.Id + '&companyId=' + $scope.transactionCurrency.CompanyId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.transactionCurrencies.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } else {
            ShowResult("Please select at list one row.", 'failure');
        }
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.Action = "Save";
        $scope.companyId = $scope.transactionCurrency.CompanyId;
        $scope.transactionCurrency = {};
        $scope.transactionCurrency.Active = true;
        $scope.transactionCurrency.CompanyId = $scope.companyId;
    }
}