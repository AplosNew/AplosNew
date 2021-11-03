'use strict';
employeeLedgerOpeningBalanceReportController.$inject = ['$scope', '$rootScope', '$http', "$window", "baseService"];
function employeeLedgerOpeningBalanceReportController($scope, $rootScope, $http, $window, baseService) {
    $rootScope.title = 'Employee Opening Balance Ledger';
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
            var url = 'Employees/EmployeeReport/GetEmployeeLedgerOpeningBalanceReport?reportFormat=' + $scope.report.ReportFormat + '&fiscalYearId=' + $scope.report.FiscalYearId;
            $window.open(url, '_blank');
        }
    };
}