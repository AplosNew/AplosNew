'use strict';
entityComponentCostingController.$inject = ['$window', 'cboService', "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function entityComponentCostingController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Organizations/EntityComponentCosting/';
    $scope.getListUrl = $scope.path + 'GetList';
    $scope.mainList = [];
    $scope.exchangeRate = true;
    $scope.getData = function () {
        if (baseService.isUndefinedOrNull($scope.model.EntityId)) {
            $scope.mainList = [];
            $scope.model = {
                CompanyId: $scope.model.CompanyId
                , CompanyCurrencyId: $scope.model.CompanyCurrencyId
                , CompanyCurrency: $scope.model.CompanyCurrency
            };
            return;
        }
        $http({
            method: 'GET',
            url: $scope.getListUrl + '?entityId=' + $scope.model.EntityId
        }).then(function successCallback(response) {
            if (!baseService.isUndefinedOrNull(response.data.masterData)) {
                var companyId = $scope.model.CompanyId;
                var companyCurrencyId = $scope.model.CompanyCurrencyId;
                var companyCurrency = $scope.model.CompanyCurrency;
                $scope.model = response.data.masterData;
                $scope.model.CompanyId = companyId;
                $scope.model.CompanyCurrencyId = companyCurrencyId;
                $scope.model.CompanyCurrency = companyCurrency;
            }
            else
                $scope.model = {
                    Id: null
                    , CompanyId: $scope.model.CompanyId
                    , EntityId: $scope.model.EntityId
                    , CurrencyId: null
                    , ExchangeRate: 0
                    , CompanyCurrencyId: $scope.model.CompanyCurrencyId
                    , CompanyCurrency: $scope.model.CompanyCurrency
                    , NoOfWorkStation: 0
                };
            $scope.mainList = response.data.matrixData;
            if (!baseService.isUndefinedOrNull($scope.model.CurrencyId) && $scope.model.CurrencyId !== $scope.model.CompanyCurrencyId)
                $scope.exchangeRate = false;
        });
    };

    $scope.model = {
        Id: null
        , CompanyId: null
        , EntityId: null
        , CurrencyId: null
        , ExchangeRate: null
        , CompanyCurrencyId: null
        , CompanyCurrency: null
        , NoOfWorkStation: null
    };

    // #region DDL
    $scope.companyList = [];
    $scope.entityList = [];
    $scope.currencyList = [];
    cboService.getCboCompanyByCompanyGroup($window.companyGroupId, function (result) {
        $scope.companyList = result;
    });
    $scope.getEntityList = function () {
        if (baseService.isUndefinedOrNull($scope.model.CompanyId)) {
            $scope.entityList = [];
            $scope.mainList = [];
            $scope.model = { CompanyId: $scope.model.CompanyId };
            $scope.model.CurrencyId = null;
            $scope.model.CompanyCurrencyId = null;
            $scope.model.CompanyCurrency = null;
            return;
        }
        else {
            cboService.getCboEntityByCompanyWise($window.companyGroupId, $scope.model.CompanyId, function (result) {
                $scope.entityList = result;
            });
            getCurrencyList();
        }
    };
    function getCurrencyList() {
        cboService.getCboTransactionCurrencyByCompany($scope.model.CompanyId, function (result) {
            $scope.currencyList = result;
        });
        cboService.getCompanyCurrency($scope.model.CompanyId, function (result) {
            $scope.model.CompanyCurrencyId = result[0].Value;
            $scope.model.CompanyCurrency = result[0].Text;
        });
    }
    // #endregion

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.modelForm.$valid) {
            if (baseService.isUndefinedOrNull($scope.model.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.path + 'create',
                    data: {
                        'entity': $scope.model
                        , 'detailList': $scope.mainList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true)
                        ShowResult(response.data.Message, "failure");
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if (!baseService.isUndefinedOrNull($scope.model.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.path + 'edit',
                    data: {
                        'entity': $scope.model
                        , 'detailList': $scope.mainList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true)
                        ShowResult(response.data.Message, "failure");
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }
    }

    $scope.Clear = function () {
        $scope.model = {};
        $scope.mainList = [];
        $scope.exchangeRate = true;
    };

    $scope.currencyChange = function () {
        $scope.model.ExchangeRate = 0;
        if (baseService.isUndefinedOrNull($scope.model.CurrencyId))
            $scope.exchangeRate = true;
        else if ($scope.model.CurrencyId !== $scope.model.CompanyCurrencyId)
            $scope.exchangeRate = false;
        else if ($scope.model.CurrencyId === $scope.model.CompanyCurrencyId)
            $scope.exchangeRate = true;
    }
}