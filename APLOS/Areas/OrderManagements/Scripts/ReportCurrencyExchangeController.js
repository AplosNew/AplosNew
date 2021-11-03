'use strict';
ReportCurrencyExchangeController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "cboService"];
function ReportCurrencyExchangeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $scope.ExchangeTitle = 'Currency Exchange';
    $scope.ExchangePath = "OrderManagements/ReportCurrencyExchange/";



    $scope.CompanyList = [];
    $scope.PlantListMain = [];
    $scope.PlantList = [];
    $scope.CurrencyList = [];
    $scope.SelectedCurrency = null;
    $scope.SelectedPlant = null;
    $scope.SelectedCompany = null;
    $scope.CurrencyMatrix = [];
    $http({
        method: 'GET',
        url: 'Productions/ProductionDashboard/GetAllCompaniesAndPlants'
    }).then(function successCallback(response) {
        $scope.PlantListMain = response.data.Plant;
        $scope.CompanyList = response.data.Company;

    });

    $scope.ChangePlant = function () {
        $http({
            method: 'GET',
            url: $scope.ExchangePath + 'GetReportCurrency?PlantId=' + $scope.SelectedPlant
        }).then(function successCallback(response) {
            $scope.CurrencyList = [];
            $scope.CurrencyMatrix = [];
            $scope.SelectedCurrency = null;

            $scope.CurrencyList = response.data.TransactionCurrencyList;
            $scope.SelectedCurrency = null;
            if (response.data.BaseCurrency.length > 0) {
                $scope.SelectedCurrency = response.data.BaseCurrency[0]["Id"];
                $scope.ChangeCurrency();
            }
        });
    }


    $scope.ChangeCompany = function () {
        $scope.EntityList = [];
        $scope.PlantList = [];
        $scope.PlantList = ej.DataManager($scope.PlantListMain).executeLocal(ej.Query().where("CompanyId", "equal", $scope.SelectedCompany));
    }

    $scope.ChangeCurrency = function () {
        $scope.CurrencyMatrix = [];
        $http({
            method: 'GET',
            url: $scope.ExchangePath + 'GetRelativeCurrencyMatrix?PlantId=' + $scope.SelectedPlant + '&BaseCurrencyId=' + $scope.SelectedCurrency
        }).then(function successCallback(response) {
            $scope.CurrencyMatrix = [];
            $scope.CurrencyMatrix = response.data.Matrix;
        });
    }
    $scope.SaveReportCurrencyConversion = function () {
       
        $http({
            method: 'POST',
            url: $scope.ExchangePath + 'SaveReportCurrencyConversion', 
            data: { PlantId: $scope.SelectedPlant, BaseCurrencyId: $scope.SelectedCurrency, data: $scope.CurrencyMatrix}
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        });
    }
}