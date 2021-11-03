'use strict';
DailyDayStatusReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function DailyDayStatusReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Daily Day Status Report';
    $scope.path = 'humanresource/DailyDayStatusReport/';
    $scope.Prev = null;
    $scope.PreviousDateEnabled = false;
    $scope.PreviousDate = function () {
        var attdnDate = new Date($scope.attdnDate);
        $scope.PreviousDateEnabled = false;
        $scope.Prev = $filter('date')(new Date(attdnDate.setDate(attdnDate.getDate() - 1)), 'dd-MMM-yyyy');     
        if (!baseService.isUndefinedOrNull($scope.attdnDate)) {
            $scope.PreviousDateEnabled = true;
        }
    };
    $scope.empGridShow = function (args) {
        ShowResult('Press the Go Button  After Date Change', 'success');
        $scope.empGrid = false;
    };
    $scope.isMaternity = false;
    $scope.isSeperated = true;
    $scope.isActive = true;
    $scope.EmployeeListDefault = [];
    $scope.EmployeeList = [];
    $scope.EmployeeListTemp = [];
    $scope.GetEmployeeInformation = function () {
        var date = $filter('dateFiltering')($scope.attdnDate, 'dd-MM-yyyy');
        var previousdate = $filter('dateFiltering')($scope.Prev, 'dd-MM-yyyy');
        var parameters = {
            'workDate': date, 'PrevWorkDate': previousdate, 'isActive': $scope.isActive,
            'isSeperated': $scope.isSeperated,
            'isMaternity': $scope.isMaternity
        };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'humanresource/DailyDayStatusReport/GetEmpInfo',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.empGrid = true;
                $scope.EmployeeListDefault = response.data;//.filter(d => d.isSelect == true);
                $scope.EmployeeList = $scope.EmployeeListDefault;
                $scope.EmployeeListTemp = $scope.EmployeeListDefault;
            }
            else {
                $scope.empGrid = false;
                ShowResult("No Data Found", 'failure');
            }
        });
        //}
    };
    var empParameters = [];
    $scope.GetdailyDayStatusReport = function () {
        try {
            var reportFormat = "Excel";
            empParameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filteredRecords = gridObj.getFilteredRecords();
            var data = filteredRecords;
            data = ej.DataManager(data).executeLocal(ej.Query().select(["EmpSystemId"]));
            var res = data.toString();
    
            //empParameters.push(filteredRecords);
            var date = $filter('dateFiltering')($scope.attdnDate, 'dd-MM-yyyy');
            var previousdate = $filter('dateFiltering')($scope.Prev, 'dd-MM-yyyy');

            if (new Date(previousdate) > new Date(date)) {
                throw 'Previous Date cann\'t be greater.';
            }
            if (previousdate === date) {
                throw 'Previous Date cann\'t be Same.';
            }
            if (baseService.isUndefinedOrNull($scope.attdnDate)) {
                throw "Select Date.";
            }

            $scope.Dep = $("#Department option:selected").text();
            $scope.Sec = $("#Section option:selected").text();
            //if (filteredRecords.length > 140) {
            //    throw "Max. Download Limit is 140";
            //}
            var wcDocCatg = "(";

            wcDocCatg += Array.prototype.map.call(data, function (item) { return "'" + item.EmpSystemId + "'"; }).join(",") + ")"; /*+= Array.prototype.map.call(data, function (item) { return "'" + item.EmpSystemId + "'"; }).join(",");*/

            var file_src = 'humanresource/DailyDayStatusReport/GetDailyDayStatusReport?reportFormat=' + reportFormat + '&workDate=' + $scope.attdnDate + '&PrevWorkDate=' + $scope.Prev + '&empParameters=' + wcDocCatg;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}
