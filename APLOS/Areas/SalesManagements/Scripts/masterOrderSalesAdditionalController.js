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

    $scope.model = { Id: null, SalesId: null, PostCode: null, ShippingDate: null, ShippingBill: null, RodTepAmount: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.SalesId = null;
    $scope.ShowAdditionalPopup = function (obj) {
        $scope.model = { Id: null, SalesId: null, PostCode: null, ShippingDate: null, ShippingBill: null, RodTepAmount: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
        $scope.modelNew = Object.assign({}, $scope.model);
        $scope.SalesAdditionalInfoDataList = [];
        $scope.SalesId = obj.data.Id;
        $scope.GetSalesAdditionalInfoData();
        angular.element(document.querySelector('#detailpopup')).modal('show');
    }

    $scope.EditData = function (data) {
        $scope.modelNew = Object.assign({}, data);
    }

    $scope.ClosePopUp = function () {
        $scope.model = { Id: null, SalesId: null, PostCode: null, ShippingDate: null, ShippingBill: null, RodTepAmount: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
        $scope.modelNew = Object.assign({}, $scope.model);
        angular.element(document.querySelector('#detailpopup')).modal('hide');
    }
    $scope.Action = "Save";
    $scope.Save = function () {
        try {
            $scope.modelNew.SalesId = $scope.SalesId;
            if (baseService.isUndefinedOrNull($scope.modelNew.PostCode)) {
                throw "Post Code is required.";
            }
            if (baseService.isUndefinedOrNull($scope.modelNew.ShippingDate)) {
                throw "Shipping Date is required.";
            }
            if (baseService.isUndefinedOrNull($scope.modelNew.ShippingBill)) {
                throw "Shipping Bill is required.";
            }
            if (baseService.isUndefinedOrNull($scope.modelNew.RodTepAmount)) {
                throw "RodTep Amount is required.";
            }

            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: 'SalesManagements/Sales/CreateSalesAdditionalInfo',
                    data: {
                        'data': $scope.modelNew
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetSalesAdditionalInfoData();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure",'detailpopup');
        }
    };

    $scope.SalesAdditionalInfoDataList = [];
    $scope.GetSalesAdditionalInfoData = function () {
        $scope.SalesAdditionalInfoDataList = [];
        $http.get("SalesManagements/Sales/GetSalesAdditionalInfoData?salesId=" + $scope.SalesId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SalesAdditionalInfoDataList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.Clear = function () {
        $scope.model = { Id: null, SalesId: null, PostCode: null, ShippingDate: null, ShippingBill: null, RodTepAmount: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
        $scope.modelNew = Object.assign({}, $scope.model);
    }

    $scope.message_detailconfirmation = null;
    $scope.removeLineItem = function (data) {
        $scope.modelNew = Object.assign({}, data);
        if (!baseService.isUndefinedOrNull($scope.modelNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.modelNew.PostCode + ' ]';
        angular.element(document.querySelector('#confirmBoMDetailPopUp')).modal('show');
    }

    $scope.DeleteItem = function () {
        $http({
            method: 'POST',
            url: 'SalesManagements/Sales/DeleteItem?id=' + $scope.modelNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSalesAdditionalInfoData();
                $scope.Clear();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };
}