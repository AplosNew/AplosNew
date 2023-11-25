'use strict';
PaymentAdviceController.$inject = ['accountService', 'addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function PaymentAdviceController(accountService, addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {

    $rootScope.title = "Payment Advice";
    $scope.Action = 'Save';
    $scope.path = 'Accounts/Invoice/';


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
}