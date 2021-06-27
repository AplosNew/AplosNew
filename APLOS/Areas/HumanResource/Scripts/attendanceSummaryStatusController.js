'use strict';
attendanceSummaryStatusController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller','$window'];
function attendanceSummaryStatusController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Previous Day Absent';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });

    var date = new Date(), y = date.getFullYear(), m = date.getMonth()-6;
    var firstDay = new Date(y, m, 1);

    $scope.AttendanceSummaryStatusReport = {        
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),      
        ReportFormat: 'Excel'      
    };
   
    $scope.AttendanceSummaryStatusReportData = function () {
        try {
        
            if (baseService.isUndefinedOrNull($scope.AttendanceSummaryStatusReport.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("From Date is required.", 'failure');
            }
            else if (baseService.isUndefinedOrNull($scope.AttendanceSummaryStatusReport.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
                ShowResult("To Date is required.", 'failure');
            }
            else if (new Date($scope.AttendanceSummaryStatusReport.FromDate) > new Date($scope.AttendanceSummaryStatusReport.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
                ShowResult("From date must be below or equal to To Date", 'failure');
            }
            else if (new Date($scope.AttendanceSummaryStatusReport.ToDate) < new Date($scope.AttendanceSummaryStatusReport.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To date must be above or equal to From Date.", 'failure');
            }
            else {
                var url = 'HumanResource/AttendanceManagement/GetAttendanceSummaryStatusReport?reportFormat=Excel' + ' &FromDate=' + $scope.AttendanceSummaryStatusReport.FromDate + ' &ToDate=' + $scope.AttendanceSummaryStatusReport.ToDate;
                $rootScope.report(url);
            }          
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}