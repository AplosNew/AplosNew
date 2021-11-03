'use strict';
TrimInTimeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function TrimInTimeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = 'Trim In Time';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'HumanResource/TrimInTime/';
    $scope.InTimeStartMargin = 15;
    $scope.FromDateSingleDate = '';
    $scope.FromDate = '';
    $scope.ToDate = '';
    $scope.ShiftSystemID = '';
    function nullrecorder(val) {
        if (baseService.isUndefinedOrNull(val))
            return "";

        return val;
    }

    $scope.shiftDefinition = {};
    $scope.getShiftDefinition = function () {
        if (nullrecorder($scope.ShiftSystemID) == "" || nullrecorder($scope.FromDateSingleDate) == "")
            return;

        $http({
            method: "POST",
            dataType: 'JSON',
            data: {
                'systemid': $scope.ShiftSystemID, 'WorkDate': $scope.FromDateSingleDate
            },
            url: $scope.path + 'getShiftDefinition'

        }).then(function successCallback(response) {
            $scope.shiftDefinition = response.data[0];

        });
    }

    $scope.enabledPanel = true;
    $scope.employeeAttendance = [];
    $scope.employeeAttendanceBySingleDate = [];
    $scope.ShiftNote = "";
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
    $scope.Clear = function () {
        $scope.employeeAttendanceBySingleDate = [];
        $scope.ShiftNote = "";
        var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
        gridObj.refreshContent();
    }

    $scope.allShiftSingleDay = [];
    $scope.InTimeMarginTemp = 1000;
    $scope.loadEmployeesToGrid = function () {
        if (nullrecorder($scope.ShiftSystemID) == "" || nullrecorder($scope.FromDateSingleDate) == "")
            return;

        $http({
            method: "POST",
            dataType: 'JSON',
            data: {
                'employeeid': '', 'shiftsystemid': $scope.ShiftSystemID, 'fromdate': $scope.FromDateSingleDate, 'todate': $scope.FromDateSingleDate, 'minutes': $scope.InTimeStartMargin
            },
            url: $scope.path + 'getAttendanceData'

        }).then(function successCallback(response) {
            $scope.employeeAttendanceBySingleDate = response.data.data;
            $scope.ShiftNote = response.data.note;
            $scope.InTimeMarginTemp = $scope.InTimeStartMargin;
            //default checking on sceen data
            for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
                $scope.employeeAttendanceBySingleDate[i].Active = false;
            }

           
            var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
            gridObj.refreshContent();

        });
    }

    $window.onload = function (event) {
        $scope.actionCompleteSingleEmployee("refresh");
        $scope.actionCompleteSingleDay("refresh");

    }
    $window.onresize = function (event) {
        $scope.actionCompleteSingleEmployee("refresh");
        $scope.actionCompleteSingleDay("refresh");

    }
    $scope.actionCompleteSingleEmployee = function (args) {
        try {
            if (args == "refresh" || args.requestType == "refresh") {
                var scrollerwidth = 0;
                var gridObj = null;
                try {
                    gridObj = $("#GridChangeAttendance").ejGrid("instance");
                    scrollerwidth = $("#APIAccordion").width();//Obtain the width of the container
                    gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth, height: 400 } });//pass the obtainer width and height to gridmodel options
                    gridObj.windowonresize();

                } catch (e) {

                }
            }
        } catch (e) {

        }
    }
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#GridChangeAttendanceBySingleDate .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridChangeAttendanceBySingleDate .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridChangeAttendanceBySingleDate .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.employeeAttendanceBySingleDate, { 'Id': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Active == true)
                $($("#GridChangeAttendanceBySingleDate .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridChangeAttendanceBySingleDate .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridChangeAttendanceBySingleDate .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    }
    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.employeeAttendanceBySingleDate, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#GridChangeAttendanceBySingleDate").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
                    $scope.employeeAttendanceBySingleDate[i].Active = true;
                }
            }
            else {
                for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.employeeAttendanceBySingleDate[i].Id == filtered[j].Id)
                            $scope.employeeAttendanceBySingleDate[i].Active = true;
                    }

                }
            }

            var checkbox = $("#GridChangeAttendanceBySingleDate .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridChangeAttendanceBySingleDate .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridChangeAttendanceBySingleDate .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridChangeAttendanceBySingleDate .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#GridChangeAttendanceBySingleDate").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
                    $scope.employeeAttendanceBySingleDate[i].Active = false;
                }
            }
            else {
                for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.employeeAttendanceBySingleDate[i].Id == filtered[j].Id)
                            $scope.employeeAttendanceBySingleDate[i].Active = false;
                    }

                }
            }
            var checkbox = $("#GridChangeAttendanceBySingleDate .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridChangeAttendanceBySingleDate .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridChangeAttendanceBySingleDate .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridChangeAttendanceBySingleDate .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
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

    $scope.shiftinfo = [];
    $scope.selectedShiftInfo = function (args) {

        $http({
            method: "POST",
            dataType: 'JSON',
            url: $scope.path + 'getShift'

        }).then(function successCallback(response) {
            $scope.shiftinfo = response.data;
        });


    }
    $scope.selectedShiftInfo();
    $scope.SaveSingleEmployee = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
            try {
                if ($scope.employeeAttendanceBySingleDate[i].Active == true)
                {
                    DataToBeSaved.push($scope.employeeAttendanceBySingleDate[i].Id);

                }
            } catch (e) {

            }

        }

        $http({
            method: "POST",
            dataType: 'JSON',
            data: {
                'employeelist': DataToBeSaved, 'shiftsystemid': $scope.ShiftSystemID, 'fromdate': $scope.FromDateSingleDate, 'minutes': $scope.InTimeStartMargin},
            url: $scope.path + 'SaveRandomTime'

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.loadEmployeesToGrid();

            }


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
}