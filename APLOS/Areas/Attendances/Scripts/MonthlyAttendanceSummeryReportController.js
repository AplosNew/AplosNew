'use strict';
MonthlyAttendanceSummeryReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function MonthlyAttendanceSummeryReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Monthly Attendance Summary";
    $scope.path = 'Attendances/MonthlyAttendanceSummeryReport/';


    //#region Get year 
    $scope.YearList = [];
    $scope.getYear = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetYear",
        }).then(function successCallback(response) {
            $scope.YearList = response.data;            
        });
    }
    $scope.getYear();    
    //#endregion 

    $scope.GetMonthlyAttendanceSummeryReport = function () {
        var reportFormat = "Excel";
        try {
            var url = 'Attendances/MonthlyAttendanceSummeryReport/GetReport?reportFormat=' + reportFormat + "&Year=" + $scope.YearId + "&Month=" + $scope.MonthId ;

            $rootScope.report(url);
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
}