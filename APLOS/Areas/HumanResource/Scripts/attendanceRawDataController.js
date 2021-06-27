'use strict';
attendanceRawController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller','$window'];
function attendanceRawController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Attendance Raw Data';
    
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.RawDataModel = {        
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),       
        ReportFormat: 'Excel',      
    };
   
    $scope.DownloadRawData = function () {
        try {
        
            if (baseService.isUndefinedOrNull($scope.RawDataModel.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("From Date is required.", 'failure');
            }
            else if (baseService.isUndefinedOrNull($scope.RawDataModel.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
                ShowResult("To Date is required.", 'failure');
            }
            else if (new Date($scope.RawDataModel.FromDate) > new Date($scope.RawDataModel.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
                ShowResult("From date must be below or equal to To Date", 'failure');
            }
            else if (new Date($scope.RawDataModel.ToDate) < new Date($scope.RawDataModel.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To date must be above or equal to From Date.", 'failure');
            }
            else {
                var url = 'HumanResource/AttendanceManagement/GetAttendanceRawDataReport?reportFormat=Excel' + ' &FromDate=' + $scope.RawDataModel.FromDate + ' &ToDate=' + $scope.RawDataModel.ToDate;
                $rootScope.report(url);
            }          
        } catch (e) {
            ShowResult(e, 'failure');

        }
    };
}