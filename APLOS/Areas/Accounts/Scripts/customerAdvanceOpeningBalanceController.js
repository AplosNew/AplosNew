'use strict';
customerAdvanceOpeningBalanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$controller'];
function customerAdvanceOpeningBalanceController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $controller) {
    $rootScope.title = 'Customer Advanced Opening Balance';
    $scope.url = 'accounts/OpeningBalance';
    $scope.listUrl = $scope.url + '/GetCustomerAdvanceList';
    $scope.saveUrl = $scope.url + '/InsertCustomerAdvance';
    $scope.updateUrl = $scope.url + '/UpdateCustomerAdvance';
    $scope.interplantList = [];

    $scope.sort = 'PartyName';
    $scope.partyType = 'Customer';
    $scope.partyGLType = "DownPayment";
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $controller('baseOpeningBalanceController', { $scope: $scope, $http: $http });
}