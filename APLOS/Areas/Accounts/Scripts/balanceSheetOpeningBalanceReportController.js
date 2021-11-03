'use strict';
BalanceSheetOpeningBalanceReportController.$inject = ['$scope', '$rootScope', '$http'];
function BalanceSheetOpeningBalanceReportController($scope, $rootScope, $http) {
    $rootScope.title = 'Balance Sheet Opening Balance Report';

    $scope.balanceSheet = {
        IsBudgetLevel: false,
        IsActivityLevel: false,
        FiscalYearId: null
    };

    $scope.fiscalYearList = [];
    $http({
        method: 'GET',
        url: 'accounts/FiscalYear/GetCbo'
    }).then(function successCallback(response) {
        $scope.fiscalYearList = response.data;
    });

    $scope.uncheck = function () {
        if ($scope.balanceSheet.IsBudgetLevel === false) {
            $scope.balanceSheet.IsBudgetLevel = false;
            $scope.balanceSheet.IsActivityLevel = false;
        }
        else {
            $scope.balanceSheet.IsActivityLevel = false;
        }
    };
    $scope.balanceSheetObReport = function () {
        location.href = 'accounts/voucher/balancesheetObreport?fiscalYearId=' + $scope.balanceSheet.FiscalYearId + '&isBudgetLevel=' + $scope.balanceSheet.IsBudgetLevel + '&isActivityLevel=' + $scope.balanceSheet.IsActivityLevel;
    };
}