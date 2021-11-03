'use strict';
leavesChecklistReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller','$window'];
function leavesChecklistReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Manual Out Time';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.LeavesCheckListReport = {        
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        //EmployeeId: null,
        ReportFormat: 'Excel',
        //chkAdditionInfo: false
    };
   
    $scope.LeavesCheckListReportData = function () {
        try {
        
            if (baseService.isUndefinedOrNull($scope.LeavesCheckListReport.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("From Date is required.", 'failure');
            }
            else if (baseService.isUndefinedOrNull($scope.LeavesCheckListReport.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
                ShowResult("To Date is required.", 'failure');
            }
            else if (new Date($scope.LeavesCheckListReport.FromDate) > new Date($scope.LeavesCheckListReport.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
                ShowResult("From date must be below or equal to To Date", 'failure');
            }
            else if (new Date($scope.LeavesCheckListReport.ToDate) < new Date($scope.LeavesCheckListReport.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To date must be above or equal to From Date.", 'failure');

            }
            else {

                var url = 'HumanResource/AttendanceManagement/GetleavesChecklistReport?reportFormat=Excel' + ' &FromDate=' + $scope.LeavesCheckListReport.FromDate + ' &ToDate=' + $scope.LeavesCheckListReport.ToDate;

                $rootScope.report(url);

            }

           
        } catch (e) {
            ShowResult(e, 'failure');

        }
    };
}