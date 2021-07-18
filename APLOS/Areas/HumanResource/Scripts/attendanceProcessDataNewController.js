'use strict';
attendanceProcessDataNewController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function attendanceProcessDataNewController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'Manual Attendance';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'HumanResource/attendanceProcessDataNew/';

    $scope.FromDateSingleDate = '';
    $scope.FromDate = '';
    $scope.ToDate = '';
    $scope.SetAs = 'In';
    $scope.Intime = null;
    function nullrecorder(val) {
        if (baseService.isUndefinedOrNull(val))
            return "";

        return val;
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



    $scope.employeeAttendanceBySingleDateSelection = [];
    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.employeeAttendanceBySingleDateSelection, { 'Id': e.model.value });
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
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.employeeAttendanceBySingleDateSelection.length; i++) {
                    $scope.employeeAttendanceBySingleDateSelection[i].Active = true;
                }
            }
            else {
                for (var i = 0; i < $scope.employeeAttendanceBySingleDateSelection.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.employeeAttendanceBySingleDateSelection[i].Id == filtered[j].Id)
                            $scope.employeeAttendanceBySingleDateSelection[i].Active = true;
                    }

                }
            }

            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.employeeAttendanceBySingleDateSelection.length; i++) {
                    $scope.employeeAttendanceBySingleDateSelection[i].Active = false;
                }
            }
            else {
                for (var i = 0; i < $scope.employeeAttendanceBySingleDateSelection.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.employeeAttendanceBySingleDateSelection[i].Id == filtered[j].Id)
                            $scope.employeeAttendanceBySingleDateSelection[i].Active = false;
                    }

                }
            }
            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#Gridemployee .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    }
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.employeeAttendanceBySingleDateSelection, { 'Id': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Active == true)
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    }
    $scope.saveemployeedata = function () {

        var row = $filter('filter')($scope.employeeAttendanceBySingleDateSelection, { 'Active': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.employeeAttendanceBySingleDate = row;
        }
        $scope.Back();
    }
    $scope.showEmployeeFilterScreen = function () {
        try {

            var gridObj = $("#Gridemployee").data("ejGrid");
            gridObj.clearFiltering();
            angular.element(document.querySelector('#recipeMaterialPopUp')).modal('show');


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.Back = function () {
        angular.element(document.querySelector('#recipeMaterialPopUp')).modal('hide');
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
                    scrollerwidth = $("#Tab").width();//Obtain the width of the container
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
            try {
                if (
                    nullrecorder($scope.employeeAttendance[i].ShiftSystemID) != nullrecorder($scope.employeeAttendance[i].ShiftSystemIDOriginal)
                    || nullrecorder($scope.employeeAttendance[i].InDate) != nullrecorder($scope.employeeAttendance[i].InDateOriginal)
                    || nullrecorder($scope.employeeAttendance[i].InTime) != nullrecorder($scope.employeeAttendance[i].InTimeOriginal)
                    || nullrecorder($scope.employeeAttendance[i].OutDate) != nullrecorder($scope.employeeAttendance[i].OutDateOriginal)
                    || nullrecorder($scope.employeeAttendance[i].OutTime) != nullrecorder($scope.employeeAttendance[i].OutTimeOriginal)
                ) {
                    DataToBeSaved.push($scope.employeeAttendance[i]);

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

                $scope.selectSignleEmployee();

            }


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

    $scope.SaveSingleDay = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
            $scope.employeeAttendanceBySingleDate[i].ErrorMessage = "";
            try {
                if (
                    nullrecorder($scope.employeeAttendanceBySingleDate[i].ShiftSystemID) != nullrecorder($scope.employeeAttendanceBySingleDate[i].ShiftSystemIDOriginal)
                    || nullrecorder($scope.employeeAttendanceBySingleDate[i].InDate) != nullrecorder($scope.employeeAttendanceBySingleDate[i].InDateOriginal)
                    || nullrecorder($scope.employeeAttendanceBySingleDate[i].InTime) != nullrecorder($scope.employeeAttendanceBySingleDate[i].InTimeOriginal)
                    || nullrecorder($scope.employeeAttendanceBySingleDate[i].OutDate) != nullrecorder($scope.employeeAttendanceBySingleDate[i].OutDateOriginal)
                    || nullrecorder($scope.employeeAttendanceBySingleDate[i].OutTime) != nullrecorder($scope.employeeAttendanceBySingleDate[i].OutTimeOriginal)
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

                $scope.selectSigleDate();

            }

        });


    }
}