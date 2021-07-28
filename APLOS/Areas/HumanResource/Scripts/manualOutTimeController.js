'use strict';
manualOutTimeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller','$window'];
function manualOutTimeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Manual Out Time';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    var yesterday = new Date();

    var datee = yesterday.setDate(yesterday.getDate() - 1);

    $scope.ToDate = $filter('dateFiltering')(new Date(datee), 'dd-MM-yyyy');

    $scope.ManualOutTimeDateWise = {        
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(new Date(datee), 'dd-MM-yyyy'),
        //EmployeeId: null,
        ReportFormat: 'Excel',
        //chkAdditionInfo: false
    };
   
    $scope.ManualOutTimeDateWiseData = function () {
        try {
        
            if (baseService.isUndefinedOrNull($scope.ManualOutTimeDateWise.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("From Date is required.", 'failure');
            }
            else if (baseService.isUndefinedOrNull($scope.ManualOutTimeDateWise.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
                ShowResult("To Date is required.", 'failure');
            }
            else if (new Date($scope.ManualOutTimeDateWise.FromDate) > new Date($scope.ManualOutTimeDateWise.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
                ShowResult("From date must be below or equal to To Date", 'failure');
            }
            else if (new Date($scope.ManualOutTimeDateWise.ToDate) < new Date($scope.ManualOutTimeDateWise.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To date must be above or equal to From Date.", 'failure');

            }
            else {

                var url = 'HumanResource/AttendanceManagement/GetManualOutTimeDateWiseReport?reportFormat=Excel' + ' &FromDate=' + $scope.ManualOutTimeDateWise.FromDate + ' &ToDate=' + $scope.ManualOutTimeDateWise.ToDate;

                $rootScope.report(url);

            }

           
        } catch (e) {
            ShowResult(e, 'failure');

        }
    };
}