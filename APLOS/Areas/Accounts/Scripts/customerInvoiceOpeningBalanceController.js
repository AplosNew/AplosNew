'use strict';
customerInvoiceOpeningBalanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function customerInvoiceOpeningBalanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Customer Invoice Opening Balance';
    $scope.url = 'accounts/OpeningBalance';
    $scope.listUrl = $scope.url + '/GetCustomerInvoiceList';
    $scope.saveUrl = $scope.url + '/InsertCustomerInvoice';
    $scope.updateUrl = $scope.url + '/UpdateCustomerInvoice';
    $scope.interplantList = [];

    $scope.sort = 'PartyName';
    $scope.partyType = 'Customer';
    $scope.partyGLType = "Reconciliation";
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $controller('baseOpeningBalanceController', { $scope: $scope, $http: $http });
   
}