"use strict";
bankOpeningBalanceLedgerController.$inject = ["$scope", "$rootScope", "$http"];
function bankOpeningBalanceLedgerController($scope, $rootScope, $http) {
    $rootScope.title = "Bank Opening Balance Ledger";
    $scope.report = {
        FiscalYearId: null,
        IsCompanyCurrency: true
    };

    $scope.fiscalYearList = [];
    $http({
        method: "GET",
        url: "accounts/FiscalYear/GetCbo"
    }).then(function successCallback(response) {
        $scope.fiscalYearList = response.data;
    });

    $scope.getBankLedgerReport = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form.$valid) {
            if ($scope.report.FiscalYearId === null)
                return ShowResult("Fiscal Year required", "failure");
            location.href = "Banks/BankReport/GetBankOpeningBalanceLedgerReport?fiscalYearId=" + $scope.report.FiscalYearId + "&isCompanyCurrency=" + $scope.report.IsCompanyCurrency;
        }
    };
}