"use strict";
cashOpeningBalanceLedgerController.$inject = ["$scope", "$rootScope", "$http"];
function cashOpeningBalanceLedgerController($scope, $rootScope, $http) {
    $rootScope.title = "Cash Opening Balance Ledger";
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

    $scope.getCashLedgerReport = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form.$valid) {
            if ($scope.report.FiscalYearId === null)
                return ShowResult("Fiscal Year required", "failure");
            location.href = "Banks/CashReport/GetCashOpeningBalanceLedgerReport?fiscalYearId=" + $scope.report.FiscalYearId + "&isCompanyCurrency=" + $scope.report.IsCompanyCurrency;
        }
    };
}