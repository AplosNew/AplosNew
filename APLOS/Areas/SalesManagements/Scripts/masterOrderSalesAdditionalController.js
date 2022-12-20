"use strict";
masterOrderSalesAdditionalController.$inject = ["cboService", "commonMessage", '$window', "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "accountService", "bankService"];
function masterOrderSalesAdditionalController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $http, $filter, $controller, accountService, bankService) {
    $rootScope.title = "Master Order Sales";
    $scope.Action = "Save";
    $scope.invoiceList = [];
    $scope.postedSalesList = [];

    $scope.searchByPostedSales = "InvoiceNo"; $scope.searchSales = "";
    $scope.searchByPostedSalesList = [{ value: 'InvoiceNo', name: "Invoice No" }, { value: 'VoucherNo', name: "Voucher No" }, { value: 'PartyCode', name: "Party Code" }, { value: 'PartyName', name: "Party Name" }
        , { value: 'DocRefNo', name: "DocRef No" }
    ];

    $scope.getMasterOrderSalesPostedList = [];
    $scope.getMasterOrderSalesPosted = function () {
        $http({
            method: 'POST'
            , url: 'SalesManagements/Sales/GetPostedMasterOrderSalesList'
            , data: { column: $scope.searchByPostedSales, value: $scope.searchSales }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.getMasterOrderSalesPostedList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.getMasterOrderSalesPosted();



    $scope.InvoiceId = null;
    $scope.ShowAdditionalPopup = function (obj) {
        $scope.InvoiceId = obj.data.Id;
        angular.element(document.querySelector('#detailpopup')).modal('show');
    }


    $scope.ClosePopUp = function () {
        angular.element(document.querySelector('#detailpopup')).modal('hide');
    }

    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.bomNew.FGMaterialMasterId)) {
                throw "Finish Goods Material is required.";
            }
            if (baseService.isUndefinedOrNull($scope.bomNew.FGArticleId)) {
                throw "Finish Goods Article is required.";
            }
            if (baseService.isUndefinedOrNull($scope.bomNew.UnitOfMeasurementId)) {
                throw "Finish Goods UoM is required.";
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.bomNewForm.$valid) {
                if ($scope.Action == "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveMasterUrl,
                        data: {
                            'entity': $scope.bomNew
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.bomNew.Id = response.data.Data.Id;
                            $scope.getmasterData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action == "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: {
                            'entity': $scope.bomNew
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.bomNew.Id = response.data.Data.Id;
                            $scope.getmasterData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.masterDataList = [];
    $scope.getmasterData = function () {
        $http.get("OrderManagements/BOMMaster/GetList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.masterDataList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.Clear = function () {
        $scope.masterOrderItemList = [];
        $scope.salesMaterialList = [];
        $scope.invoiceList = [];
        $scope.postedSalesList = [];
        $scope.selectedpostedSalesList = [];
    }
}