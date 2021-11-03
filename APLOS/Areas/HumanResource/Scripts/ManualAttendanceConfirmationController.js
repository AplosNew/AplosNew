'use strict';
ManualAttendanceConfirmationController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function ManualAttendanceConfirmationController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'Manual Attendance';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'HumanResource/ManualAttendanceConfirmation/';

    $scope.FromDateSingleDate = '';
    $scope.FromDate = '';
    $scope.ToDate = '';
    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    function nullrecorder(val) {
        if (baseService.isUndefinedOrNull(val))
            return "";

        return val;
    }
    $scope.queryCellInfo = function (args) {
        if (args.data.IsManualDayStatus == true) {
            if (args.column.field == "IsManualDayStatus" || args.column.field == "DayStatus") {
                args.cell.bgColor = "#FF911D";
            }
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
    $scope.employeeAttendanceBySingleDatePending = [];
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
    $scope.allShiftSingleDay = [];

    $scope.selectSigleDate = function () {

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'pdate': $scope.FromDateSingleDate },
            url: $scope.path + 'getAttendanceData'

        }).then(function successCallback(response) {
            $scope.employeeAttendanceBySingleDate = response.data.data;

            var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
            gridObj.refreshContent();

        });
    }
    $scope.selectSigleDatePending = function () {

        $http({
            method: "POST",
            dataType: 'JSON',
            url: $scope.path + 'getAttendanceDataPending'

        }).then(function successCallback(response) {
            $scope.employeeAttendanceBySingleDatePending = response.data.data;

            var gridObj = $("#GridChangeAttendanceBySingleDatePending").data("ejGrid");
            gridObj.refreshContent();

        });
    }

    $window.onload = function (event) {
        $scope.actionCompletePending("refresh");
        $scope.actionCompleteSingleDay("refresh");

    }
    $window.onresize = function (event) {
        $scope.actionCompletePending("refresh");
        $scope.actionCompleteSingleDay("refresh");

    }
    $scope.actionCompletePending = function (args) {
        try {
            if (args == "refresh" || args.requestType == "refresh") {
                var scrollerwidth = 0;
                var gridObj = null;
                try {
                    gridObj = $("#GridChangeAttendanceBySingleDatePending").ejGrid("instance");
                    scrollerwidth = $("#TabPending").width();//Obtain the width of the container
                    gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                    gridObj.windowonresize();

                } catch (e) {

                }
            }
        } catch (e) {

        }
    }
    $scope.actionCompleteSingleDay = function (args) {
        try {
            if (args == "refresh" || args.requestType == "refresh") {
                var scrollerwidth = 0;
                var gridObj = null;

                try {
                    gridObj = $("#GridChangeAttendanceBySingleDate").ejGrid("instance");
                    scrollerwidth = $("#TabDateRange").width();//Obtain the width of the container
                    gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                    gridObj.windowonresize();
                } catch (e) {

                }



            }
        } catch (e) {

        }
    }
    $scope.changeShift = function (args) {

        for (var i = 0; i < $scope.employeeAttendance.length; i++) {
            for (var j = 0; j < $scope.allShift.length; j++) {
                //new Date(year, month, day, hours, minutes, seconds, milliseconds)
                if ($scope.employeeAttendance[i].ShiftName == $scope.allShift[j].UserName) {
                    $scope.employeeAttendance[i].ShiftSystemID = $scope.allShift[j].SystemID;



                }


            }

        }

        return;

    }
    $scope.ActionchangeShift = function (args) {

    }

    $scope.shiftinfo = {};
    $scope.selectedShiftInfo = function (args) {
        var eDialog = $("#ViewShiftInfo").data("ejDialog");
        eDialog.open();

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'systemid': args.data.ShiftSystemID, 'WorkDate': args.data.WorkDate },
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
    $scope.rowDataBoundSingleEmployee = function rowDataBoundSingleEmployee(e) {

        if (!baseService.isUndefinedOrNull(e.data.ErrorMessage) && e.data.ErrorMessage != "")
            e.row.css("background-color", "#ff0000");

    }
    $scope.SaveSingleDay = function (args, flag) {

        $http({
            method: "POST",
            dataType: 'JSON',
            data: {
                'employeeid': args.data.Id, 'workdate': args.data.PDate, 'inOrOut': flag
            },
            url: $scope.path + 'SaveSingleEmployee'

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');

                var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
                gridObj.refreshContent();
            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.selectSigleDate();

            }

        });


    }
    $scope.SaveSingleDayPending = function (args, flag) {

        $http({
            method: "POST",
            dataType: 'JSON',
            data: {
                'employeeid': args.data.Id, 'workdate': args.data.PDate, 'inOrOut': flag
            },
            url: $scope.path + 'SaveSingleEmployee'

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');

                var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
                gridObj.refreshContent();
            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.selectSigleDatePending();

            }

        });


    }
    $scope.selectSigleDatePending();

}