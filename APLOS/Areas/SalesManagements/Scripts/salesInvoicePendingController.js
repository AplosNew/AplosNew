"use strict";
salesInvoicePendingController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "accountService"];
function salesInvoicePendingController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, accountService) {
    $rootScope.title = "Pending Sales Invoice";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherDetailList = [];
    $scope.voucherList = [];
    $scope.partyType = "Customer";
    $scope.isAdvance = false;
    $scope.salesMaterialList = [];
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("baseMaterialAndArticleController", { $scope: $scope, $http: $http });

    baseService.init("SalesManagements/Sales/GetSalesPendingList", null, null, "DESC", "InvoiceDate", "InvoiceNo");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.invoiceList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();
}