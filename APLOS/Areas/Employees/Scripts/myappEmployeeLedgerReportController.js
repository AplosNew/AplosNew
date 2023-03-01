'use strict';
myappEmployeeLedgerReportController.$inject = ['$scope', '$rootScope', '$http', '$filter', '$controller', 'baseService', "$window"];
function myappEmployeeLedgerReportController($scope, $rootScope, $http, $filter, $controller, baseService, $window) {
    $rootScope.title = 'Employee Ledger';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $scope.ledgerReport = {
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        EmployeeId: null,
        ReportFormat: 'Pdf'
    };

    $scope.report = function () {
        if (baseService.isUndefinedOrNull($scope.ledgerReport.FromDate)) {
            manualValidation('div_FromDate', true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.ledgerReport.ToDate)) {
            manualValidation('div_ToDate', true, "To Date is required.");
        }
        else if (new Date($scope.ledgerReport.FromDate) > new Date($scope.ledgerReport.ToDate)) {
            manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.ledgerReport.ToDate) < new Date($scope.ledgerReport.FromDate)) {
            manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
        }
        else {
            var url = 'Employees/EmployeeReport/GetMyAppEmployeeLedgerReport?reportFormat=' + $scope.ledgerReport.ReportFormat + '&fromDate=' + $scope.ledgerReport.FromDate + '&toDate=' + $scope.ledgerReport.ToDate;
            $window.open(url, '_blank');
        }
    };
}