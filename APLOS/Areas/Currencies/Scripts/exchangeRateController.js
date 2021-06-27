'use strict';
ExchangeRateController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function ExchangeRateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    $rootScope.title = "Exchange Rate";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.deleteUrl = 'currencies/exchangerate/delete?id='
    $scope.getexchangeRate = [];
    $scope.exchangeRateList = [];
    $scope.exchangeRates = [];
    $scope.path = 'currencies/exchangerate/';
    $scope.editexchangeratediv = false;
    $scope.insertexchangeratediv = true;
    $scope.deletebutton = false;
    $scope.showcurrencyAdd = false;
    $scope.exchangeEditList = [];
    $scope.exchangeEditListpush = {};
    $scope.exchange = {
        Id: null,
        CompanyId: null,
        FromCurrencyUnit: 1,
        FromCurrencyCode: null,
        ToCurrencyBankBuying: null,
        ToCurrencyBankSelling: null,
        ToCurrencyAverage: null,
        ToCurrencyCode: null,
        FromDate: $filter("dateFiltering")(Date.now()),
        Active: true
    }

    $('.datepicker').datepicker({
        format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
    });

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.GetCurrencyList = function () {
        $http({
            method: 'GET',
            url: 'currencies/TransactionCurrency/GetExchangeCurrencyCbo?companyId=' + $scope.exchange.CompanyId
        }).then(function successCallback(response) {
            $scope.currencyList = response.data;
        });
    };

    $scope.CheckExistCurrency = function (list, fromcurrencycode) {
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].FromCurrencyCode === fromcurrencycode) {
                    $scope.pop('error', 'Same Currency already have Added !');
                    return false;
                }
            }
            return true;
        }
        else
            return true;
    };

    $scope.AddCurrency = function () {
        if ($scope.baseParallelCurrency.length > 0) {
            if ($scope.CurrencyId !== null) {
                if ($scope.CheckExistCurrency($scope.exchangeRateList, $scope.CurrencyId)) {
                    $scope.exchangeRateList.push({
                        BaseCurrency: $scope.baseParallelCurrency[0].BaseCurrency,
                        CurrencyCode: angular.element("#currencyId :selected").text(),
                        FromCurrencyCode: $scope.CurrencyId,
                        FromCurrencyUnit: 1,
                        ToCurrencyAverage: 0,
                        ToCurrencyBankBuying: 0,
                        ToCurrencyBankSelling: 0,
                        ToCurrencyCode: $scope.baseParallelCurrency[0].ToCurrencyCode,
                        CompanyId: $scope.exchange.CompanyId,
                        Active: $scope.exchange.Active,
                        isExtraCurrency: true
                    });
                }
            }
            else {
                $scope.pop('error', 'Please Select Currency !');
            }
        }
        else {
            $scope.pop('error', 'This company is not set parallel currency !');
        }
    }
    $scope.GetExchangeRateList = function (item) {
        if (baseService.isUndefinedOrNull(item))
            return true;
        $http({
            method: 'GET',
            url: 'currencies/exchangeRate/GetExchangeRateList?companyId=' + item
        }).then(function successCallback(response) {
            $scope.exchangeRateList = response.data.Rows;
        });
    };

    $scope.CheckParallelCurrencySet = function (item) {
        if (baseService.isUndefinedOrNull(item))
            return true;
        $http({
            method: 'GET',
            url: 'currencies/exchangeRate/CheckParallelCurrencySet?companyId=' + item
        }).then(function successCallback(response) {
            $scope.checkParallelCurrencySet = response.data;
            if ($scope.checkParallelCurrencySet === false) {
                $scope.pop('error', 'Company Parallel Currency is not set! ');
            }
        });
    };

    $scope.BaseParallelCurrencySet = function (item) {
        $http({
            method: 'GET',
            url: 'currencies/exchangeRate/GetBaseParallelCurrency?companyId=' + item
        }).then(function successCallback(response) {
            $scope.baseParallelCurrency = response.data.Rows;
        });
    };

    $scope.exchangeRateIndex = -1;

    $scope.removeExchangeRow = function () {
        $scope.exchangeRateList.splice($scope.exchangeRateIndex, 1);
        $scope.exchangeRateIndex = -1;
    };
    $scope.searchByList = [
        {
            'name': 'From Currency',
            'value': 'FromCurrencyName'
        },
        {
            'name': 'ToCurrencyAverage',
            'value': 'ToCurrencyAverage'
        },
        {
            'name': 'ToCurrencyBankBuying',
            'value': 'ToCurrencyBankBuying'
        },
        {
            'name': 'ToCurrencyBankSelling',
            'value': 'ToCurrencyBankSelling'
        }
        ,
        {
            'name': 'ToCurrency',
            'value': 'ToCurrencyName'
        },
        {
            'name': 'FromDate',
            'value': 'FromDate'
        }
    ];
    $scope.onCompanyChange = function (item) {
        if (baseService.isUndefinedOrNull(item))
            return true;
        $scope.exchangeRateParameters = {
            limit: 10,
            offset: 0,
            order: 'DESC',
            sort: 'FromDate',
            searchBy: "FromCurrencyName",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.GetExchangeRateList(item);
        $scope.getexchangerateUrl = $scope.path + 'getdata?companyId=' + item;
        $scope.getData = function (pageno) {
            baseService.paginationBase($scope.getexchangerateUrl, pageno, $scope.exchangeRateParameters)
                .then(function (result) {
                    $scope.getexchangeRate = result.Rows;
                    $scope.exchangeRateParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    }
    $scope.getExchange = function (item) {
        $http({
            method: 'GET',
            url: 'currencies/exchangerate/getdatabyid?id=' + item
        }).then(function successCallback(response) {
            $scope.exchangeEditList = response.data.Rows;
        });
    }

    $scope.Get = function (id, index) {
        $scope.editexchangeratediv = true;
        $scope.insertexchangeratediv = false;
        $scope.deletebutton = true;
        $scope.Action = 'Update';
        $scope.index = index;
        $scope.getExchange(id);
        $scope.showcurrencyAdd = true;
        $scope.exchangeRateList = [];
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        // helperdropdowns();
    };
    $scope.checkAmountNullMsg = '';
    $scope.checkAmountNull = function () {
        try {
            for (var i = 0; i < $scope.exchangeRateList.length; i++) {
                if ($scope.exchangeRateList[i].Active === true) {
                    if ($scope.exchangeRateList[i].ToCurrencyBankBuying > 0 && $scope.exchangeRateList[i].ToCurrencyBankSelling > 0
                        && $scope.exchangeRateList[i].ToCurrencyAverage > 0) {
                    }
                    else {
                        throw $scope.pop('error', 'Currency Bank Buying and selling can not empty');
                    }
                }
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.checkAmountNullEdit = function () {
        if ($scope.exchangeEditList[0].ToCurrencyBankBuying !== 0 || $scope.exchangeEditList[0].ToCurrencyBankSelling !== 0 ||
            $scope.exchangeEditList[0].ToCurrencyAverage !== 0) {
            $scope.checkAmountNullMsg = '';
            return true;
        } else {
            $scope.pop('error', 'Currency Bank Buying and selling can not empty');
            return false
        }
    }

    $scope.checkCompany = function () {
        for (var i = 0; i < $scope.exchangeRates.length; i++) {
            if ($scope.exchangeRates[i].CompanyId === null || $scope.exchangeRates[i].CompanyId === "") {
                $scope.pop('error', 'Company is not select !');
                return false
            }
            else {
                return true;
            }
        }
    }

    $scope.Save = function () {
        $scope.checkAmountNull();
        angular.forEach($scope.exchangeRateList, function (item) {
            if (item.Active === true) {
                $scope.exchangeRates.push({
                    FromCurrencyUnit: item.FromCurrencyUnit,
                    FromCurrencyCode: item.FromCurrencyCode,
                    ToCurrencyBankBuying: item.ToCurrencyBankBuying,
                    ToCurrencyBankSelling: item.ToCurrencyBankSelling,
                    ToCurrencyAverage: item.ToCurrencyAverage,
                    ToCurrencyCode: item.ToCurrencyCode,
                    FromDate: $scope.exchange.FromDate,
                    CompanyId: $scope.exchange.CompanyId,
                    Active: $scope.exchange.Active
                })
            }
        })

        if ($scope.exchangeRateForm.$valid) {//&&
            try {
                if ($scope.Action === 'Save') {
                    //$scope.checkAmountNull();
                    $scope.checkCompany();
                    $http({
                        method: 'POST',
                        url: 'currencies/exchangerate/create',
                        data: { 'exchangeRate': $scope.exchangeRates },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                            $scope.exchangeRates = [];
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            baseService.paginationAdd();
                            ClearFields();
                            $scope.GetExchangeRateList($scope.exchange.CompanyId);
                            $scope.getData();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }

                else if ($scope.Action === 'Update' && $scope.checkAmountNullEdit()) {
                    $http({
                        method: 'POST',
                        url: 'currencies/exchangerate/update',
                        data: { 'exchangeRate': $scope.exchangeEditList[0] },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                            }
                            ClearFields();
                            $scope.showcurrencyAdd = false;
                            $scope.GetExchangeRateList($scope.exchange.CompanyId);
                            $scope.getData();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
            } catch (e) {
                ShowResult(e, 'failure');
            }
        }
    };
    $scope.pop = function (type, msg) {
        toaster.pop({
            type: type,
            body: msg,
            timeout: 3000
        });
    };
    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: $scope.deleteUrl + $scope.exchangeEditList[0].Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                baseService.paginationRemove();
                ClearFields();
                $scope.getData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    }
    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.editexchangeratediv = false;
        $scope.insertexchangeratediv = true;
        $scope.deletebutton = false;
        $scope.exchangeRateList = [];
        $scope.exchangeEditList = [];
        $scope.GetExchangeRateList($scope.exchange.CompanyId);
        $scope.exchangeRates = [];
        $scope.showcurrencyAdd = false;
    }
}