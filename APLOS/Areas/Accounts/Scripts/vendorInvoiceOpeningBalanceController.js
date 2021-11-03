'use strict';
vendorInvoiceOpeningBalanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function vendorInvoiceOpeningBalanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Vendor Invoice Opening Balance';
    $scope.url = 'accounts/OpeningBalance';
    $scope.listUrl = $scope.url + '/GetVendorInvoiceList';
    $scope.saveUrl = $scope.url + '/InsertVendorInvoice';
    $scope.updateUrl = $scope.url + '/UpdateVendorInvoice';
    $scope.interplantList = [];
    $scope.partyType = 'Vendor';
    $scope.partyGLType = "Reconciliation";
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $scope.sort = 'PartyName';
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $controller('baseOpeningBalanceController', { $scope: $scope, $http: $http });
   
}