'use strict';
AttendanceRawDataFromAppController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function AttendanceRawDataFromAppController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Attendance Raw Data From App';
    $scope.path = 'Attendances/AttendanceRawDataFromApp/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.Action = 'Save';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.saveBP = $scope.path + 'SaveBP';
    $scope.saveLeaveUrl = $scope.path + 'SaveLeave';
    $scope.saveMUrl = $scope.path + 'SaveM';
    $scope.deleteUrl = $scope.path + 'DeleteDetails/';

    $scope.employeeAttendance = [];
    $scope.employeeAttendanceBySingleDate = [];

    $scope.selectSigleDate = function () {

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'employeeid': '', 'fromdate': $scope.FromDateSingleDate, 'todate': $scope.FromDateSingleDate },
            url: $scope.path + 'getAttendanceData'

        }).then(function successCallback(response) {
            $scope.employeeAttendanceBySingleDate = response.data.data;
            $scope.employeeAttendanceBySingleDateSelection = response.data.data;
            
            var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
            gridObj.refreshContent();

        });
    }
    $scope.selectemployee = [];
    $scope.selectedSinglemployee = {};
    $scope.getAllEmployee = function () {

        var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
        eDialog.open();

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'fromdate': $scope.FromDate, 'todate': $scope.ToDate },
            url: $scope.path + 'getAllEmployees'

        }).then(function successCallback(response) {
            $scope.selectemployee = response.data;

        });
    }
    $scope.selectSignleEmployee = function (args) {
        var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
        eDialog.close();
        if (baseService.isUndefinedOrNull(args) == false)
            $scope.selectedSinglemployee = args.data;

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'employeeid': $scope.selectedSinglemployee.Id, 'fromdate': $scope.FromDate, 'todate': $scope.ToDate },
            url: $scope.path + 'getAttendanceDataxD'

        }).then(function successCallback(response) {
            $scope.employeeAttendance = response.data.data;            
            var gridObj = $("#GridChangeAttendance").data("ejGrid");
            gridObj.refreshContent();
        });
    }

    $scope.SetIn = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.Intime)) {
                throw "Select Time..";
            }
            var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.employeeAttendanceBySingleDate
            }
            for (var i = 0; i < filteredRecords.length; i++) {
                filteredRecords[i].InTime = $scope.Intime;
            }
            $scope.employeeAttendanceBySingleDate = filteredRecords;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SetOut = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.Intime)) {
                throw "Select Time..";
            }
            var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.employeeAttendanceBySingleDate
            }
            for (var i = 0; i < filteredRecords.length; i++) {
                filteredRecords[i].OutTime = $scope.Intime;
            }
            $scope.employeeAttendanceBySingleDate = filteredRecords;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.SaveSingleEmployee = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.employeeAttendance.length; i++) {
            $scope.employeeAttendance[i].ErrorMessage = "";
            try {
                if ($scope.employeeAttendance[i].InDate != null
                    || $scope.employeeAttendance[i].InTime != null
                    || $scope.employeeAttendance[i].OutDate != null
                    || $scope.employeeAttendance[i].OutTime != null) {
                    DataToBeSaved.push($scope.employeeAttendance[i]);
                }
                else {

                }
            } catch (e) {

            }
        }
        $http({
            method: "POST",
            dataType: 'JSON',
            url: $scope.path + 'Save',
            data: { 'data': JSON.stringify(DataToBeSaved) },
            contentType: "application/json charset=utf-8"
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
                for (var i = 0; i < response.data.Data.length; i++) {
                    var row = $filter('filter')($scope.employeeAttendance, { 'Id': response.data.Data[i].Id });
                    if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
                        row[0].ErrorMessage = response.data.Data[i].ErrorMessage;
                    }
                }
                var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
                gridObj.refreshContent();
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.selectSignleEmployee();
            }
        });
    }

//    public class AttendanceFromApp {
//        public string Id { get; set; } = "";
//    public string WorkDate { get; set; } = "";
//    public string InDate { get; set; } = "";
//    public string InTime { get; set; } = "";
//    public string OutDate { get; set; } = "";
//    public string OutTime { get; set; } = "";
//    public bool IsError { get; set; } = false;
//    public string ErrorMessage { get; set; } = "";
//}

    $scope.SaveSingleDay = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
            $scope.employeeAttendanceBySingleDate[i].ErrorMessage = "";
            try {
                if ($scope.employeeAttendanceBySingleDate[i].InDate != null
                    || $scope.employeeAttendanceBySingleDate[i].InTime != null
                    || $scope.employeeAttendanceBySingleDate[i].OutDate != null
                    || $scope.employeeAttendanceBySingleDate[i].OutTime != null)
                {
                    DataToBeSaved.push($scope.employeeAttendanceBySingleDate[i]);
                }
                else {

                }
            } catch (e) {

            }
        }
        for (var i = 0; i < DataToBeSaved.length; i++) {
            DataToBeSaved[i].WorkDate = $scope.FromDateSingleDate;
        }

        var sorteddata = ej.DataManager(DataToBeSaved).executeLocal(ej.Query().select(["Id", "WorkDate", "InDate", "InTime", "OutDate","OutTime"]));

        $http({
            method: "POST",
            dataType: 'JSON',
            url: $scope.path + 'Save',
            data: { 'data': JSON.stringify(sorteddata) },
            contentType: "application/json charset=utf-8"
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
                for (var i = 0; i < response.data.Data.length; i++) {
                    var row = $filter('filter')($scope.employeeAttendanceBySingleDate, { 'Id': response.data.Data[i].Id });
                    if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
                        row[0].ErrorMessage = response.data.Data[i].ErrorMessage;
                    }
                }
                var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
                gridObj.refreshContent();
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.selectSigleDate();
            }
        });
    }
}