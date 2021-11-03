'use strict';
vendorAdvanceOpeningBalanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function vendorAdvanceOpeningBalanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Vendor Advanced Opening Balance';
    $scope.url = 'accounts/OpeningBalance';
    $scope.listUrl = $scope.url + '/GetVendorAdvanceList';
    $scope.saveUrl = $scope.url + '/InsertVendorAdvance';
    $scope.updateUrl = $scope.url + '/UpdateVendorAdvance';
    $scope.interplantList = [];

    $scope.sort = 'PartyName';
    $scope.partyType = 'Vendor';
    $scope.partyGLType = "DownPayment";
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $controller('baseOpeningBalanceController', { $scope: $scope, $http: $http });
}