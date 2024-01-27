'use strict';
LWQSummaryReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function LWQSummaryReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "LWQSummaryReport";
    $scope.Action = 'Save';
    $scope.path = 'QMS/LWQSummaryReport/';

    $scope.status = {
        Id: null,
        Customer: null,
        CustomerId: null,
        InvoiceNo: null,
        InvoiceId: null,
        ProductionOrderId: null,
        LotNo: null,
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.CustomerList = [];
    $scope.selectCustomer = function () {
        $http({
            method: 'GET',
            url: 'QMS/LWQSummaryReport/GetCustomerList'
        }).then(function successCallback(response) {
            $scope.CustomerList = response.data;
            angular.element(document.querySelector('#CustomerPopup')).modal('show');
        });
    }

    $scope.doubleCustomer = function (e) {
        $scope.statusNew.CustomerId = e.data.PartyId;
        $scope.statusNew.Customer = e.data.Customer;
        angular.element(document.querySelector('#CustomerPopup')).modal('hide');
    }

    $scope.closeCustomerPopup = function () {
        angular.element(document.querySelector('#CustomerPopup')).modal('hide');
    }

    $scope.InvoiceList = [];
    $scope.selectInvoice = function () {
        $http({
            method: 'GET',
            url: 'QMS/LWQSummaryReport/GetInvoiceList?PartyId=' + $scope.statusNew.CustomerId
        }).then(function successCallback(response) {
            $scope.InvoiceList = response.data;
            angular.element(document.querySelector('#InvoicePopup')).modal('show');
        });
    }

    $scope.doubleInvoice = function (e) {
        $scope.statusNew.InvoiceId = e.data.InvoiceId;
        $scope.selectPONo();
        angular.element(document.querySelector('#InvoicePopup')).modal('hide');
    }

    $scope.closeInvoicePopup = function () {
        angular.element(document.querySelector('#InvoicePopup')).modal('hide');
    }

    $scope.POList = [];
    $scope.selectPONo = function () {
        $scope.POList = [];
        $http({
            method: 'GET',
            url: 'QMS/LWQSummaryReport/GetPOList?InvoiceId=' + $scope.statusNew.InvoiceId
        }).then(function successCallback(response) {
            $scope.POList = response.data;
        });
    }
    $scope.selectPONo();

    //$scope.LotNumberLists = [];
    //$scope.GetLotNumberLists = function () {
    //    $scope.LotNumberLists = [];
    //    $http({
    //        method: 'GET',
    //        url: 'QMS/LWQSummaryReport/GetLotNumberLists?POId=' + $scope.statusNew.ProductionOrderId
    //    }).then(function successCallback(response) {
    //        $scope.LotNumberLists = response.data;
    //    });
    //}
    //$scope.GetLotNumberLists();

    $scope.LotNumberLists = [];
    $scope.selectLotNo = function () {
        $scope.LotNumberLists = [];
        $http({
            method: 'GET',
            url: 'QMS/LWQSummaryReport/GetLotNumberLists?POId=' + $scope.statusNew.ProductionOrderId
        }).then(function successCallback(response) {
            $scope.LotNumberLists = response.data;
            angular.element(document.querySelector('#LotNumberPopup')).modal('show');
        });
    }

    $scope.doubleLotNumber = function (e) {
        $scope.statusNew.LotNo = e.data.LotNumber;
        $scope.statusNew.ProductionOrderId = e.data.PONo;
        angular.element(document.querySelector('#LotNumberPopup')).modal('hide');
    }

    $scope.closeLotNumberPopup = function () {
        angular.element(document.querySelector('#LotNumberPopup')).modal('hide');
    }

    $scope.View = function () {
        try {
                $scope.JobCardCQReportFunc();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Clear = function () {
        LWQRClearFields();
    };

    function LWQRClearFields() {
        $scope.Action = "Save";
        $scope.statusNew = Object.assign({}, $scope.status);
        $scope.selectPONo();
        //$scope.GetLotNumberLists();
    }

    $scope.JobCardCQReportFunc = function () {
        try {
            var url = $scope.path + '/GetCustomerLWQSummaryJobCardReport?CustomerId=' + $scope.statusNew.CustomerId + '&InvoiceId=' + $scope.statusNew.InvoiceId + '&ProductionOrderId=' + $scope.statusNew.ProductionOrderId + '&LotNumber=' + $scope.statusNew.LotNo;
            $rootScope.report(url);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}

