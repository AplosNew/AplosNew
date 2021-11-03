'use strict';
BalanceSheetDetailsReportController.$inject = ['$scope', '$rootScope', '$http'];
function BalanceSheetDetailsReportController($scope, $rootScope, $http) {
    $rootScope.title = 'Balance Sheet Detail';

    $scope.report = {
        FiscalYearId: null,
    };
    $scope.fiscalYearList = [];
    $http({
        method: 'GET',
        url: 'accounts/FiscalYear/GetCbo'
    }).then(function successCallback(response) {
        $scope.fiscalYearList = response.data;
    });

    $scope.balanceSheetDetailsReport = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            if ($scope.report.FiscalYearId === null)
                return ShowResult('Fiscal Year required', 'failure');
            location.href = 'accounts/voucher/BalanceSheetDetailReport?FiscalYearId=' + $scope.report.FiscalYearId;
        }
    };
}