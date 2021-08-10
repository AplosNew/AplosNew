'use strict';
attendanceProcessDataManualStatusNewController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function attendanceProcessDataManualStatusNewController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'Manual Attendance';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'HumanResource/attendanceProcessDataManualStatusNew/';

    $scope.FromDateSingleDate = '';
    $scope.FromDate = '';
    $scope.ToDate = '';

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
    };

   
    $scope.selectedStatus = [];
    $scope.getDayStatus = function (data) {

        angular.element(document.querySelector('#StatusModal')).modal('show');
        for (var i = 0; i < $scope.employeeAttendance.length; i++) {
            if (data.data.Id == $scope.employeeAttendance[i].Id &&
                data.data.WorkDate == $scope.employeeAttendance[i].WorkDate) {

                $scope.EmpCatId = data.data.EmployeeCategoryId;
                $scope.A = i;
            }

        }
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'EmpType': $scope.EmpCatId },
            url: $scope.path + 'GetDayStatus'

        }).then(function successCallback(response) {
            $scope.selectedStatus = response.data;

        });
    };


    $scope.doubleStatus = function (e) {
        
        $scope.changestatus = e.data.DayType;
        var x = $scope.A;
        $scope.employeeAttendance[x].DayStatusNew = $scope.changestatus;
        angular.element(document.querySelector('#StatusModal')).modal('hide');
        $scope.lastIndex = 0;
    }


    $scope.selectedStatusx = [];
    $scope.getDayStatusx = function (data) {

        angular.element(document.querySelector('#StatusModalx')).modal('show');
        for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
            if (data.data.Id == $scope.employeeAttendanceBySingleDate[i].Id &&
                data.data.WorkDate == $scope.employeeAttendanceBySingleDate[i].WorkDate) {

                $scope.EmpCatIdx = data.data.EmployeeCategoryId;
                $scope.Ax = i;
            }

        }
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'EmpType': $scope.EmpCatIdx },
            url: $scope.path + 'GetDayStatus'

        }).then(function successCallback(response) {
            $scope.selectedStatusx = response.data;

        });
    };


    $scope.doubleStatusx = function (e) {

        $scope.changestatusx = e.data.DayType;
        var x = $scope.Ax;
        $scope.employeeAttendanceBySingleDate[x].DayStatusNew = $scope.changestatusx;
        angular.element(document.querySelector('#StatusModalx')).modal('hide');
        $scope.lastIndex = 0;
    }



    $scope.employeeAttendance = [];
    $scope.employeeAttendanceBySingleDate = [];
    $scope.allShift = [];
    $scope.selectSingleEmployee = function (args) {
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
    };
    $scope.allShiftSingleDay = [];

    $scope.selectSingleDate = function () {

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'employeeid': '', 'fromdate': $scope.FromDateSingleDate, 'todate': $scope.FromDateSingleDate },
            url: $scope.path + 'getAttendanceData'

        }).then(function successCallback(response) {
            $scope.employeeAttendanceBySingleDate = response.data.data;
            $scope.allShiftSingleDay = response.data.shift;

            var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
            gridObj.refreshContent();

        });
    };

    $window.onload = function (event) {
        $scope.actionCompleteSingleEmployee("refresh");
        $scope.actionCompleteSingleDay("refresh");

    };
    $window.onresize = function (event) {
        $scope.actionCompleteSingleEmployee("refresh");
        $scope.actionCompleteSingleDay("refresh");

    };
    $scope.actionCompleteSingleEmployee = function (args) {
        try {
            if (args == "refresh" || args.requestType == "refresh") {
                var scrollerwidth = 0;
                var gridObj = null;
                try {
                    gridObj = $("#GridChangeAttendance").ejGrid("instance");
                    scrollerwidth = $("#TabEmployee").width();//Obtain the width of the container
                    gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth, height: 400 } });//pass the obtainer width and height to gridmodel options
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
                    scrollerwidth = $("#TabEmployee").width();//Obtain the width of the container
                    gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth, height: 400 } });//pass the obtainer width and height to gridmodel options
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
    $scope.SaveSingleEmployee = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.employeeAttendance.length; i++) {
            $scope.employeeAttendance[i].ErrorMessage = "";
            var m = $scope.employeeAttendance[i].DayStatus;
            var n = $scope.employeeAttendance[i].DayStatusNew;
            var jj = m +" "+ n;
         
                if (
                    nullrecorder($scope.employeeAttendance[i].DayStatus) !=
                    nullrecorder($scope.employeeAttendance[i].DayStatusNew)
                )
                {
                    DataToBeSaved.push($scope.employeeAttendance[i]);

                }
           

        }

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'data': DataToBeSaved },
            url: $scope.path + 'SaveSingleEmployee'

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');

                for (var i = 0; i < response.data.Data.length; i++) {
                    var row = $filter('filter')($scope.employeeAttendance, { 'WorkDate': response.data.Data[i].WorkDate });
                    if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
                        row[0].ErrorMessage = response.data.Data[i].ErrorMessage;
                    }
                }


                var gridObj = $("#GridChangeAttendance").data("ejGrid");
                gridObj.refreshContent();
            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.selectSingleEmployee();

            }


        });


    }
    $scope.SaveSingleDay = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
            $scope.employeeAttendanceBySingleDate[i].ErrorMessage = "";

            try {
                if (
                    nullrecorder($scope.employeeAttendanceBySingleDate[i].DayStatus) !=
                    nullrecorder($scope.employeeAttendanceBySingleDate[i].DayStatusNew)
                ) {
                    DataToBeSaved.push($scope.employeeAttendanceBySingleDate[i]);

                }
            } catch (e) {

            }


        }

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'data': DataToBeSaved },
            url: $scope.path + 'SaveSingleEmployee'

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

                $scope.selectSingleDate();

            }

        });


    }

    // Select All Check Box 



    $scope.refreshTemplateemployee = function () {
        $("#BPheadchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };


    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridChangeAttendance").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.employeeAttendance.length; i++) {
                $scope.employeeAttendance[i].isSelected = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridChangeAttendance").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.FlagRowId = "''";
    $scope.LockAttnd = function () {
        for (var i = 0; i < $scope.employeeAttendance.length; i++) {
            if ($scope.employeeAttendance[i].isSelected == true) {
                $scope.FlagRowId += ",'" + $scope.employeeAttendance[i].RowId + "'";

            }
        }
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'RowId': $scope.FlagRowId },
            url: $scope.path + 'LockAttnd'
        }).then(function successCallback(response) {

            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');


            } else {
                ShowResult(response.data.Message, 'success');
                $scope.FlagRowId = "''";
                $scope.selectSingleEmployee();
            }
        });
    }

}