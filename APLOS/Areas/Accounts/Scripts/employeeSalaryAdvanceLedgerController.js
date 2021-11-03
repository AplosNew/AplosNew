'use strict';
employeeSalaryAdvanceLedgerController.$inject = ['$scope', '$rootScope', '$http', '$filter', '$controller', 'baseService', "$window"];
function employeeSalaryAdvanceLedgerController($scope, $rootScope, $http, $filter, $controller, baseService, $window) {
    $rootScope.title = 'Employee Salary Advance Ledger';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $scope.ledgerReport = {
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        EmployeeId: null,
        ReportFormat: 'Pdf'
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.ledgerReport.EmployeeId = employee.SystemId;
            $scope.ledgerReport.EmployeeName = employee.EmployeeName;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.report = function () {
        if (baseService.isUndefinedOrNull($scope.ledgerReport.EmployeeId)) {
            manualValidation('div_Employee', true, "Employee is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.ledgerReport.FromDate)) {
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
            var url = 'Accounts/Advance/EmployeeSalaryAdvanceLedgerReport?reportFormat=' + $scope.ledgerReport.ReportFormat + '&fromDate=' + $scope.ledgerReport.FromDate + '&toDate=' + $scope.ledgerReport.ToDate + '&employeeId=' + $scope.ledgerReport.EmployeeId;
            $window.open(url, '_blank');
        }
    };
}