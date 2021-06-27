'use strict';
interCompanyLoanTakenOpeningBalanceController.$inject = ['cboService', '$scope', '$rootScope', '$http', '$controller'];
function interCompanyLoanTakenOpeningBalanceController(cboService, $scope, $rootScope, $http, $controller) {
    $rootScope.title = 'Inter Company Loan Taken Opening Balance';
    $scope.url = 'accounts/OpeningBalance';
    $scope.listUrl = $scope.url + '/GetInterCompanyLoanTakenList';
    $scope.updateUrl = $scope.url + '/UpdateInterCompanyLoanTaken';

    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $controller('baseOpeningBalanceController', { $scope: $scope, $http: $http });
    $scope.openingBalance.PartyType = 'Company';
    $scope.partyType = 'Company';
    $scope.sourceType = 'Loan';

    cboService.getCboInterCompanyFinancingType($scope.sourceType, function (result) {
        $scope.financingTypeList = result;
    });
}