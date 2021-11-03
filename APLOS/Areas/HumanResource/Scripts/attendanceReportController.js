'use strict';
attendanceReportController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window', 'toaster'];
function attendanceReportController(fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window, toaster)
{
    $rootScope.title = 'Employee Attendance';
    $scope.index = -1;
   
    // #region ****Scope Ledger Report***
    $scope.AttndReport = {
        FromDate: null,
        ToDate: null
    };
    $scope.EmpAttndReportPrint = function () {
        try {
            CheckField($scope.AttndReport.FromDate, "From Date");
            CheckField($scope.AttndReport.ToDate, "To Date");
            var _fromdate = new Date($scope.AttndReport.FromDate);
            var _todate = new Date($scope.AttndReport.ToDate);
           /* var empcount = ($scope.EmpEncashReport.selectedemployeeList.EmployeeCode)*/;
            var fromdate = $filter('dateFiltering')(_fromdate, 'dd-MMM-yyyy');
            var todate = $filter('dateFiltering')(_todate, 'dd-MMM-yyyy');
            if (_fromdate >= _todate) {
                throw "From Date [" + fromdate + "] can not be greater than To Date [" + todate + "]";
            }
            location.href =  'humanresource/AttendanceReport/AttndReport?fromDate=' + $scope.AttndReport.FromDate + '&toDate=' + $scope.AttndReport.ToDate + '&plantId=' + $scope.AttndReport.PlantId;


        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw '[' + fieldName + '] is required.';
            }
        } catch (e) {
            throw e;
        }
    }
}