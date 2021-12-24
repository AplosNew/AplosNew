'use strict';
PIInvoiceController.$inject = ['accountService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService', 'bankService', '$controller'];
function PIInvoiceController(accountService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService, bankService, $controller) {
    $rootScope.title = "PI wise sales";
    $scope.Action = 'Save';
    $scope.path = 'Commercial/PIInvoice/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveChargesUrl = $scope.path + 'CreateCharge';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.TaxOption = function (data) {
        $scope.salesVM.TaxOption = data;
    };
    $scope.TaxOptionMat = function (data) {
        $scope.salesVM.TaxOptionMat = data;

    };
    $scope.TaxOptionService = function (data) {
        $scope.salesVM.TaxOptionService = data;

    };
    $scope.TaxOptionServiceModify = function (data) {
        $scope.salesVM.TaxOptionServiceModify = data;

    };

    $scope.PackingList = [];
    $scope.GetPackingListPopUp = function () {
        $scope.PackingList = [];
        $http({
            method: 'GET',
            url: "Commercial/PIInvoice/GetPackingData"
        }).then(function (response) {
            $scope.PackingList = response.data;
        });
        angular.element(document.querySelector('#PackingListPopUp')).modal('show');
    }


}






