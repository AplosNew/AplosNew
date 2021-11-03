'use strict';
intSalesOrderInvoiceEditController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function intSalesOrderInvoiceEditController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    $rootScope.title = 'Sales Order Invoice';
    $scope.Action = 'Park';
    $scope.index = -1;
    $scope.vouchers = [];
    $scope.saleOrderInvoiceEdit = [];
    $scope.CustomerReceivableReceivedList = [];
    $scope.customerreceivableList = [];

    $scope.path = 'accounts/voucher/';
    $scope.saveUrl = $scope.path + 'SalesOrderInvoicePark';
    $scope.parkUrl = $scope.path + 'customerinvoicepark';
    $scope.getListUrl = $scope.path + 'GetCustomerInvoiceSaleOrderInvoice';
    baseService.init($scope.getListUrl, null, null, 'desc', 'DocRefNo', 'DocRefNo');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.saleOrderInvoiceEdit = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.getData();
}