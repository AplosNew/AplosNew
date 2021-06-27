'use strict';
ManualAttendanceWithShiftController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', 'fileReader', '$filter'];
function ManualAttendanceWithShiftController($scope, $http, $location, $rootScope, $window, $compile, baseService, fileReader, $filter) {
    $scope.path = 'Attendances/ManualAttendanceWithShift/';
    $rootScope.title = 'Future Manual Attendance With Shift';
    $scope.dateTime = null;
    $scope.ModelList = [];

    //#region -- E M P. T A B

    $scope.DateValid = function () {
        try {
            if ($scope.FromDate > $scope.ToDate) {
                throw "Todate Cannot be greater than Fromdate..";
            }
            $scope.TodaysDate = $filter('dateFiltering')(new Date(), 'dd-MM-yyyy');
            if ($scope.ToDate <= $scope.TodaysDate) {
                throw "Select FutureDate..";
            }
            if ($scope.FromDate <= $scope.TodaysDate) {
                throw "Select FutureDate..";
            }
        } catch (e) {
            ShowResult(e, 'info');
        }
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

    $scope.employeeAttendance = [];
    $scope.employeeAttendanceBySingleDate = [];
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
            url: $scope.path + 'getAttendanceDataxD'

        }).then(function successCallback(response) {
            $scope.employeeAttendance = response.data.data;
            $scope.allShift = response.data.shift;

            var gridObj = $("#GridChangeAttendance").data("ejGrid");
            gridObj.refreshContent();
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
    //#endregion

    //#region - - - S I N G L E D A T E T A B - - -

    $scope.employeeAttendanceBySingleDate = [];
    $scope.employeeAttendanceBySingleDateSelection = [];
    $scope.allShiftSingleDay = [];
    $scope.selectSigleDate = function () {
        try {
            $scope.TodaysDate = $filter('dateFiltering')(new Date(), 'dd-MM-yyyy');

            if ($scope.FromDateSingleDate < $scope.TodaysDate) {

                $scope.FromDateSingleDate = null;
                $scope.employeeAttendanceBySingleDate = [];
                $scope.employeeAttendanceBySingleDateSelection = [];

                throw "Select Future Date..";

            }

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'employeeid': '', 'fromdate': $scope.FromDateSingleDate, 'todate': $scope.FromDateSingleDate },
                url: 'Attendances/ManualAttendanceWithShift/getAttendanceData'
            }).then(function successCallback(response) {
                for (var i = 0; i < response.data.data.length; i++) {
                    response.data.data[i].pindate = response.data.data[i].pindateG;
                    response.data.data[i].poutdate = response.data.data[i].poutdateG;
                }
                $scope.employeeAttendanceBySingleDate = response.data.data;
                $scope.employeeAttendanceBySingleDateSelection = response.data.data;
                $scope.allShiftSingleDay = response.data.shift;
                var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
                gridObj.refreshContent();
            });
        } catch (e) {
            ShowResult(e, 'info');
        }
    }

    $scope.shiftinfo = {};
    $scope.selectedShiftInfo = function (args) {
        var eDialog = $("#ViewShiftInfo").data("ejDialog");
        eDialog.open();
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'systemid': args.data.ShiftSystemID, 'WorkDate': args.data.WorkDate },
            url: 'Attendances/ManualAttendanceWithShift/getShift'
        }).then(function successCallback(response) {
            $scope.shiftinfo = response.data[0];
        });
    }

    function nullrecorder(val) {
        if (baseService.isUndefinedOrNull(val))
            return "";

        return val;
    }

    $scope.Save = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
            $scope.employeeAttendanceBySingleDate[i].ErrorMessage = "";
            try {
                if ($scope.employeeAttendanceBySingleDate[i].ShiftSystemID != null || $scope.employeeAttendanceBySingleDate[i].InDate != null
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
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'data': DataToBeSaved },
            url: $scope.path + 'Save'
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

    $scope.SaveEmp = function () {
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
            data: { 'data': DataToBeSaved, },
            url: $scope.path + 'Save'
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


    //#endregion
}





