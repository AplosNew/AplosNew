'use strict';
generalLedgerOpeningBalanceReportController.$inject = ['$scope', '$rootScope', '$http', "$window", "baseService"];
function generalLedgerOpeningBalanceReportController($scope, $rootScope, $http, $window, baseService) {
    $rootScope.title = 'General Opening Balance Ledger';
    $scope.report = {
        FiscalYearId: null,
        ReportFormat: 'Pdf'
    };

    $scope.fiscalYearList = [];
    $http({
        method: 'GET',
        url: 'accounts/FiscalYear/GetCbo'
    }).then(function successCallback(response) {
        $scope.fiscalYearList = response.data;
    });

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.GLGeneralInfoId)) {
            manualValidation('div_FY', true, "Fiscal Year is required.");
        }
        else {
            var url = 'Accounts/Voucher/GetGeneralLedgerOpeningBalanceReport?reportFormat=' + $scope.report.ReportFormat + '&fiscalYearId=' + $scope.report.FiscalYearId;
            $window.open(url, '_blank');
        }
    };
}