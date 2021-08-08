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
    $scope.allShiftSingleDay = [];
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
            $scope.allShiftSingleDay = response.data.shift;
            var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
            gridObj.refreshContent();

        });
    }

    $scope.shiftinfo = {};
    $scope.selectedShiftInfo = function (args) {
        var eDialog = $("#ViewShiftInfo").data("ejDialog");
        eDialog.open();
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'systemid': args.data.ShiftSystemIDOriginal, 'WorkDate': args.data.WorkDate },
            url: $scope.path + 'getShift'

        }).then(function successCallback(response) {
            $scope.shiftinfo = response.data[0];
        });


    }

    $scope.attendanceinfo = [];
    $scope.showAttendanceInfo = function (args) {
        var eDialog = $("#ViewAttendanceInfo").data("ejDialog");
        eDialog.open();

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'empsystemid': args.data.Id, 'WorkDate': args.data.WorkDate },
            url: $scope.path + 'getAttendance'

        }).then(function successCallback(response) {
            $scope.attendanceinfo = response.data;
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
    $scope.allShift = [];
    $scope.selectSignleEmployee = function (args) {
        var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
        eDialog.close();
        if (baseService.isUndefinedOrNull(args) == false)
            $scope.selectedSinglemployee = args.data;

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'employeeid': $scope.selectedSinglemployee.Id, 'fromdate': $scope.FromDate, 'todate': $scope.ToDate },
            url: $scope.path + 'getAttendanceData'

        }).then(function successCallback(response) {
            $scope.employeeAttendance = response.data.data;
            $scope.allShift = response.data.shift;

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
                if (filteredRecords[i].isApprovedIN) {
                    filteredRecords[i].InTime = $scope.Intime;
                }

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
                if (filteredRecords[i].isApprovedOUT) {
                    filteredRecords[i].OutTime = $scope.Intime;
                }
            }
            $scope.employeeAttendanceBySingleDate = filteredRecords;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.SaveSingleEmployee = function () {
        try {
            var DataToBeSaved = [];
            for (var i = 0; i < $scope.employeeAttendance.length; i++) {
                $scope.employeeAttendance[i].ErrorMessage = "";
                try {
                    if (baseService.isUndefinedOrNull($scope.employeeAttendance[i].InDate) || baseService.isUndefinedOrNull($scope.employeeAttendance[i].InTime)
                        || baseService.isUndefinedOrNull($scope.employeeAttendance[i].OutDate) || baseService.isUndefinedOrNull($scope.employeeAttendance[i].OutTime)) {
                        DataToBeSaved.push($scope.employeeAttendance[i]);
                    }
                    else {

                    }
                } catch (e) {

                }
            }
            for (var i = 0; i < DataToBeSaved.length; i++) {
                if (DataToBeSaved[i].InTime == "" || DataToBeSaved[i].InTime == null) {
                    throw "Insert InTime For Date: " + DataToBeSaved[i].WorkDate;
                }
                if (DataToBeSaved[i].OutTime == "" || DataToBeSaved[i].OutTime == null) {
                    throw "Insert OutTimeFor Date: " + DataToBeSaved[i].WorkDate;
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
        } catch (e) {
            ShowResult(e, 'info');
        }
    }

    $scope.SaveSingleDay = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
            $scope.employeeAttendanceBySingleDate[i].ErrorMessage = "";
            try {
                if ($scope.employeeAttendanceBySingleDate[i].InDate != null
                    || $scope.employeeAttendanceBySingleDate[i].InTime != null
                    || $scope.employeeAttendanceBySingleDate[i].OutDate != null
                    || $scope.employeeAttendanceBySingleDate[i].OutTime != null) {
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

        var sorteddata = ej.DataManager(DataToBeSaved).executeLocal(ej.Query().select(["Id", "WorkDate", "InDate", "InTime", "OutDate", "OutTime"]));

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