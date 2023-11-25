'use strict';
PaymentAdviceController.$inject = ['accountService', 'addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function PaymentAdviceController(accountService, addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {

    $rootScope.title = "Payment Advice";
    $scope.Action = 'Save';
    $scope.path = 'Accounts/Invoice/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $controller("bankBaseController", { $scope: $scope, $http: $http });

    $scope.FromDate = $filter('dateFiltering')(Date.now());
    $scope.ToDate = $filter('dateFiltering')(Date.now());

    $scope.PaymentAdviceList = [];
    $scope.GetPaymentAdvice = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GatePaymentAdviceData?fromDate=' + $scope.FromDate + '&toDate=' + $scope.ToDate + '&bankMasterId=' + $scope.BankMasterId
        }).then(function successCallback(response) {
            $scope.PaymentAdviceList = response.data;
        });
    }

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            $scope.AccountTitle = bank.AccountTitle;
            $scope.BankName = bank.AccountTitle;
            $scope.BankMasterId = bank.BankMasterId;
        }
        $scope.hideBankPopUp();
    };

    $scope.PaymentAdviceReportExcel = function () {
        $scope.fileName = 'Payment Advice Report.xlsx';

        var dataList = [];
        var g = $("#GridPAPrint").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.PaymentAdviceList;
        }
        $http({
            method: 'POST',
            url: $scope.path + "GetPaymentAdviceReport",
            data: { 'data': dataList, 'reportFileName': $scope.fileName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

}