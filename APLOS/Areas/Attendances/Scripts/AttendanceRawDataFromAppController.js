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
            var gridObj = $("#GridChangeAttendanceBySingleDates").data("ejGrid");
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

            var gridObj = $("#GridChangeAttendances").data("ejGrid");
            gridObj.refreshContent();
        });
    }
    $scope.SetTime = null;
    $scope.SetIn = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.SetTime)) {
                throw "Select Time..";
            }
            var gridObj = $("#GridChangeAttendanceBySingleDates").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.employeeAttendanceBySingleDate
            }
            for (var i = 0; i < filteredRecords.length; i++) {
                if (filteredRecords[i].isApprovedIN) {
                    filteredRecords[i].InTimeApp = $scope.SetTime;
                }

            }
            $scope.employeeAttendanceBySingleDate = filteredRecords;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SetOut = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.SetTime)) {
                throw "Select Time..";
            }
            var gridObj = $("#GridChangeAttendanceBySingleDates").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.employeeAttendanceBySingleDate
            }
            for (var i = 0; i < filteredRecords.length; i++) {
                if (filteredRecords[i].isApprovedOUT) {
                    filteredRecords[i].OutTimeApp = $scope.SetTime;
                }
            }
            $scope.employeeAttendanceBySingleDate = filteredRecords;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    

    function nullrecorder(val) {
        if (baseService.isUndefinedOrNull(val))
            return "";

        return val;
    }

    $scope.SaveSingleEmployee = function () {
        try {
            var DataToBeSaved = [];
            for (var i = 0; i < $scope.employeeAttendance.length; i++) {
                $scope.employeeAttendance[i].ErrorMessage = "";
                try {
                    if (nullrecorder($scope.employeeAttendance[i].InDateApp) != nullrecorder($scope.employeeAttendance[i].InDateOriginal)
                        || nullrecorder($scope.employeeAttendance[i].InTimeApp) != nullrecorder($scope.employeeAttendance[i].InTimeOriginal)
                        || nullrecorder($scope.employeeAttendance[i].OutDateApp) != nullrecorder($scope.employeeAttendance[i].OutDateOriginal)
                        || nullrecorder($scope.employeeAttendance[i].OutTimeApp) != nullrecorder($scope.employeeAttendance[i].OutTimeOriginal))
                    {
                        DataToBeSaved.push($scope.employeeAttendance[i]);
                    }
                    else {

                    }
                } catch (e) {

                }
            }
            for (var i = 0; i < DataToBeSaved.length; i++) {
                if (DataToBeSaved[i].InTimeApp == "" || DataToBeSaved[i].InTimeApp == null) {
                    throw "Insert InTimeApp For Date: " + DataToBeSaved[i].WorkDate;
                }
                if (DataToBeSaved[i].OutTimeApp == "" || DataToBeSaved[i].OutTimeApp == null) {
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

                    var gridObj = $("#GridChangeAttendanceBySingleDates").data("ejGrid");
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
                if (nullrecorder($scope.employeeAttendanceBySingleDate[i].InDateApp) != nullrecorder($scope.employeeAttendanceBySingleDate[i].InDateOriginal)
                    || nullrecorder($scope.employeeAttendanceBySingleDate[i].InTimeApp) != nullrecorder($scope.employeeAttendanceBySingleDate[i].InTimeOriginal)
                    || nullrecorder($scope.employeeAttendanceBySingleDate[i].OutDateApp) != nullrecorder($scope.employeeAttendanceBySingleDate[i].OutDateOriginal)
                    || nullrecorder($scope.employeeAttendanceBySingleDate[i].OutTimeApp) != nullrecorder($scope.employeeAttendanceBySingleDate[i].OutTimeOriginal))
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

      //  var sorteddata = ej.DataManager(DataToBeSaved).executeLocal(ej.Query().select(["Id", "WorkDate", "InDateApp", "InTimeApp", "OutDateApp", "OutTimeApp"]));

        
        var filtered = $("#GridChangeAttendanceBySingleDates").data("ejGrid");
        var sorteddata = filtered.getFilteredRecords();
        if (baseService.arrayLength(sorteddata) == 0) {
            sorteddata = ej.DataManager(DataToBeSaved).executeLocal(ej.Query().select(["Id", "WorkDate", "InDateApp", "InTimeApp", "OutDateApp", "OutTimeApp"]));
        }


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

                var gridObj = $("#GridChangeAttendanceBySingleDates").data("ejGrid");
                gridObj.refreshContent();
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.selectSigleDate();
            }
        });
    }
    $scope.queryCellInfo = function (args) {
        try {
            if (args.data.IsManualDayStatus == true) {
                if (args.column.field == "IsManualDayStatus" || args.column.field == "DayStatus") {
                    args.cell.bgColor = "#FF911D";
                }
            }
        } catch (e) {

        }

    }
    $scope.rowDataBoundSingleEmployee = function (e) {

        if (!baseService.isUndefinedOrNull(e.data.ErrorMessage) && e.data.ErrorMessage != "")
            e.row.css("background-color", "#ff0000");
    }
    $scope.rowDataBoundSingleDate = function (e) {

        if (!baseService.isUndefinedOrNull(e.data.ErrorMessage) && e.data.ErrorMessage != "")
            e.row.css("background-color", "#ff0000");

    }
}