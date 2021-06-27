'use strict';
employeeLedgerReportController.$inject = ['$scope', '$rootScope', '$http', '$filter', '$controller', 'baseService', "$window"];
function employeeLedgerReportController($scope, $rootScope, $http, $filter, $controller, baseService, $window) {
    $rootScope.title = 'Employee Ledger';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $scope.ledgerReport = {
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        EmployeeId: null,
        ReportFormat: 'Pdf'
    };
    $scope.showEmployeeListPopUp = function () {
        baseService.setCurrentPage('employeeList');
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'accounts/EmployeePayable/GetEmployeeListAllPlant';
            }
            else {
                url = $scope.employeeUrl;
            }
            baseService.paginationBase(url, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();
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
            var url = 'Employees/EmployeeReport/GetEmployeeLedgerReport?reportFormat=' + $scope.ledgerReport.ReportFormat + '&fromDate=' + $scope.ledgerReport.FromDate + '&toDate=' + $scope.ledgerReport.ToDate + '&employeeId=' + $scope.ledgerReport.EmployeeId;
            $window.open(url, '_blank');
        }
    };
}