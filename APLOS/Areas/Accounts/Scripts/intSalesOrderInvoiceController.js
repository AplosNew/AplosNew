'use strict';
intSalesOrderInvoiceController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function intSalesOrderInvoiceController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    $rootScope.title = 'Sales Order Invoice';
    $scope.Action = 'Park';
    $scope.index = -1;
    $scope.vouchers = [];
    $scope.customerInvoices = [];
    $scope.CustomerReceivableReceivedList = [];
    $scope.customerreceivableList = [];

    $scope.path = 'accounts/voucher/';
    $scope.saveUrl = $scope.path + 'SalesOrderInvoicePark';
    $scope.parkUrl = $scope.path + 'customerinvoicepark';
    baseService.init($scope.getListUrl, null, null, 'desc', 'DocRefNo', 'DocRefNo');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.saleOrderInvoices = result.Rows;
                console.log('saleOrderInvoices', $scope.saleOrderInvoices);
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.getData();

    function guid() {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
                .toString(16)
                .substring(1);
        }
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
            s4() + '-' + s4() + s4() + s4();
    };
    $scope.checkGLMsg = '';
    $scope.checkGL = function () {
        angular.forEach($scope.saleOrderInvoices, function (item) {
            if (item.Active) {
                if (item.GLGeneralInfoId != null) {
                    return true;
                }
                else {
                    $scope.pop('error', item.PartyName + '  have not set GL yet');
                    return false
                }
            }
        });
    };

    $scope.IntSaleOrderInvoie = function () {
        $scope.intSaleOrderInvoieDataList = [];
        angular.forEach($scope.saleOrderInvoices, function (item) {
            if (item.Active) {
                item.Id = guid();
                $scope.intSaleOrderInvoieDataList.push(
                    item
                );
            }
        });
    };
    $scope.Save = function () {
        try {
            $scope.checkGL();
            if ($scope.intSaleOrderInvoiceForm.$valid) {
                $scope.IntSaleOrderInvoie();
                if ($scope.Action == "Park") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'customerInvoices': $scope.intSaleOrderInvoieDataList, 'customerInvoiceDetails': $scope.intSaleOrderInvoieDataList
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.getData();
                            $location.path('UPanel/sales-order-edit-invoice');
                        }
                    });
                    return true;
                }
            }
        } catch (e) {
            ShowResult(e, 'failure')
        }
    }
}