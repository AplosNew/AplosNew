'use strict';
interPlantLoanTakenOpeningBalanceController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function interPlantLoanTakenOpeningBalanceController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Inter Plant Loan Taken Opening Balance';
    $scope.url = 'accounts/OpeningBalance';
    $scope.listUrl = $scope.url + '/GetInterPlantLoanTakenList';
    $scope.updateUrl = $scope.url + '/UpdateInterPlantLoanTaken';
    $scope.interplantList = [];


    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $controller('baseOpeningBalanceController', { $scope: $scope, $http: $http });
    $scope.openingBalance.PartyType = 'Plant';
    $scope.partyType = 'Plant';
    $scope.sourceType = 'Loan';

    cboService.getCboInterPlantFinancingType($scope.sourceType, function (result) {
        $scope.financingTypeList = result;
    });
}