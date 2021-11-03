'use strict';
CurrencyExchangeController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "cboService","TableName"];
function CurrencyExchangeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, TableName) {
    $scope.ExchangeTitle = 'Currency Exchange';
    $scope.ExchangePath = "OrderManagements/CurrencyExchange/";

    $scope.ExchangeRateTableName = TableName;
    $scope.ExchangeAllCurrency = [];
    $scope.ExchangeBaseCurrencyId = '';

    $scope.ExchangeDisplayCurrency = [];
    $scope.TransactionId = null;
    //get books currency and parallel currency(company)
    $scope.ExchangeReset = function () {
        $http({
            method: 'GET',
            url: $scope.ExchangePath + "GetExchangeRates?TableName=" + $scope.ExchangeRateTableName + "&TransactionId="
        }).then(function successCallback(response) {

            $scope.ExchangeAllCurrency = response.data;
            $scope.ExchangeBaseCurrencyId = response.data[0]["ToCurrencyId"];
            $scope.ExchangeDisplayCurrency = ej.DataManager(response.data).executeLocal(ej.Query().where("FromCurrencyId", "notEqual", $scope.ExchangeBaseCurrencyId));;
        });
    }
    $scope.ExchangeReset();

    $scope.ExchangeUpdateCurrencyList = function (FromCurrencyId, val) {

        for (var i = 0; i < $scope.ExchangeAllCurrency.length; i++) {
            if ($scope.ExchangeAllCurrency[i].FromCurrencyId == FromCurrencyId) {
                $scope.ExchangeAllCurrency[i].ToUnit = val;
                break;
            }
        }
    }

    //only to show on the screen
    $scope.ExchangeShowExchangeRates = function (CurrencyId) {
        //if (CurrencyId == $scope.ExchangeBaseCurrencyId)
        //    return;
        $scope.ExchangeDisplayCurrency = ej.DataManager($scope.ExchangeAllCurrency).executeLocal(ej.Query().where("FromCurrencyId", "equal", CurrencyId));;
        if ($scope.ExchangeDisplayCurrency.length > 0) {
            //open popup
            $("#dialogExchangeCurrency").data("ejDialog").open();

        }
    }
    $scope.ExchangeDisplayExchangeRates = function (TransactionId, CurrencyId) {
        if (CurrencyId == $scope.ExchangeBaseCurrencyId)
            return;
        if (!TransactionId)
            return;//the transaction not saved yet

        $scope.TransactionId = TransactionId;
        $http({
            method: 'GET',
            url: $scope.ExchangePath + "GetExchangeRates?TableName=" + $scope.ExchangeRateTableName + "&TransactionId=" + TransactionId
        }).then(function successCallback(response) {
            $scope.ExchangeAllCurrency = response.data;
            $scope.ExchangeDisplayCurrency = ej.DataManager(response.data).executeLocal(ej.Query().where("FromCurrencyId", "equal", CurrencyId));;
            if ($scope.ExchangeDisplayCurrency.length > 0) {
                if (parseFloat($scope.ExchangeDisplayCurrency[0].ToUnit) <= 0 || !$scope.ExchangeDisplayCurrency[0].ToUnit) {
                    //open popup
                    $("#dialogExchangeCurrency").data("ejDialog").open();
                }
            }
        });
    }
    $scope.ExchangeOpenExchangeRates = function (CurrencyId) {
        $scope.ExchangeDisplayCurrency = ej.DataManager($scope.ExchangeAllCurrency).executeLocal(ej.Query().where("FromCurrencyId", "equal", CurrencyId));;

        if (CurrencyId == $scope.ExchangeBaseCurrencyId)
            return true;


        if ($scope.ExchangeDisplayCurrency.length > 0) {
            //open popup if no exchange rate has been provided
            if (parseFloat($scope.ExchangeDisplayCurrency[0].ToUnit) <= 0 || !$scope.ExchangeDisplayCurrency[0].ToUnit) {
                $("#dialogExchangeCurrency").data("ejDialog").open();
                //open popup
                return false;
            }
        }
    }


    $scope.ExchangeSaveExchangeRates = function (CurrencyId) {
        try {

            return $scope.ExchangeOpenExchangeRates(CurrencyId);


        } catch (e) {

        }
        return false;
    }


}