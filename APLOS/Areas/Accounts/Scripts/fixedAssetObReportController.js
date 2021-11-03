'use strict';
FixedAssetObReportController.$inject = ['$scope', '$rootScope', '$http'];
function FixedAssetObReportController($scope, $rootScope, $http) {
    $rootScope.title = 'Fixed Asset';

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

    $scope.fixedAssetObReport = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            if ($scope.report.FiscalYearId === null)
                return ShowResult('Fiscal Year required', 'failure');
            location.href = 'accounts/voucher/FixedAssetReport?FiscalYearId=' + $scope.report.FiscalYearId;
        }
    };
}