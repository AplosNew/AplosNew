'use strict';
EmployeeDayStatusReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function EmployeeDayStatusReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Employee Day Status Report';
    $scope.Action = 'Save';
    $scope.path = 'Payrolls/EmployeeDayStatusReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';


    $scope.ShiftList = [];
    $scope.getShift = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetShift",
        }).then(function successCallback(response) {
            $scope.ShiftList = response.data;
        });
    }
    $scope.getShift();

    $scope.FromDate = null;
    $scope.shift = null;
    $scope.ToDate = null;
    $scope.Absent = false;
    $scope.Late = false;
    $scope.LvWP = false;
    $scope.LvWOP = false;
    $scope.GetReport = function () {
        var reportFormat = "Excel";
        try {
            if (new Date($scope.FromDate) > new Date($scope.ToDate) || new Date($scope.FromDate) == new Date($scope.ToDate)) {
                throw "From date must be below or equal to To Date";
            }
            var DropDownListObj = $("#shiftList").data("ejDropDownList");
            var shiftList = DropDownListObj.getSelectedValue();
            var url = 'Payrolls/EmployeeDayStatusReport/GetReport?reportFormat=' + reportFormat + "&FromDate=" + $scope.FromDate + "&Shift=" + shiftList + "&ToDate=" + $scope.ToDate + "&Absent=" + $scope.Absent + "&Late=" + $scope.Late + "&LvWP=" + $scope.LvWP + "&LvWOP=" + $scope.LvWOP ;
            $rootScope.report(url);
        } catch (e) {
            ShowResult(e, 'info');
        }
    };

}