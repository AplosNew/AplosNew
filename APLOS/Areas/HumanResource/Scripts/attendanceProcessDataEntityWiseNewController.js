'use strict';
attendanceProcessDataEntityWiseNewController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function attendanceProcessDataEntityWiseNewController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'Manual Attendance Entity Wise';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'HumanResource/attendanceProcessDataEntityWiseNew/';

    $scope.FromDateSingleDate = '';
    $scope.FromDate = '';
    $scope.ToDate = '';
    function nullrecorder(val) {
        if (baseService.isUndefinedOrNull(val))
            return "";

        return val;
    }
    $scope.EntityId = null;
    $scope.entityList = null;
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: 'HumanResource/attendanceProcessDataEntityWiseNew/GetEntity'
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }


    $scope.getAllEntities();

    $scope.selectemployee = [];
    $scope.selectedSinglemployee = {};
    $scope.getAllEmployee = function () {

        try {

            if (angular.isUndefinedOrNull($scope.FromDate))
                throw "Select From Date";

            if (angular.isUndefinedOrNull($scope.ToDate))
                throw "Select To Date";


            var DropDownListObj = $("#EntityList").data("ejDropDownList");
            var EntityList = DropDownListObj.getSelectedValue();

            if (EntityList.length == 0)
                throw "Select Entity First";

          

            var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
            eDialog.open();

            $http({
                method: "POST",
                dataType: 'JSON',
                data: {
                    'fromdate': $scope.FromDate, 'todate': $scope.ToDate, 'entityids': EntityList
                },
                url: $scope.path + 'getAllEmployees'

            }).then(function successCallback(response) {
                $scope.selectemployee = response.data;

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.queryCellInfo = function (args) {
        if (args.data.IsManualDayStatus == true) {
            if (args.column.field == "IsManualDayStatus" || args.column.field == "DayStatus" ) {
                args.cell.bgColor = "#FF911D";
            }
        }
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
        try {



            var DropDownListObj = $("#EntityList").data("ejDropDownList");
            var EntityList = DropDownListObj.getSelectedValue();

            if (EntityList.length == 0)
                throw "Select Entity First";

          
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'employeeid': '', 'fromdate': $scope.FromDateSingleDate, 'todate': $scope.FromDateSingleDate, 'entityids': EntityList },
                url: $scope.path + 'getAttendanceData'

            }).then(function successCallback(response) {
                $scope.employeeAttendanceBySingleDate = response.data.data;
                $scope.allShiftSingleDay = response.data.shift;

                var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
                gridObj.refreshContent();

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $window.onload = function (event) {
        $scope.actionCompleteSingleEmployee("refresh");
        $scope.actionCompleteSingleDay("refresh");
        $scope.Remarks = null;
    }
    $window.onresize = function (event) {
        $scope.actionCompleteSingleEmployee("refresh");
        $scope.actionCompleteSingleDay("refresh");
        $scope.Remarks = null;
    }
    $scope.actionCompleteSingleEmployee = function (args) {
        try {
            if (args == "refresh" || args.requestType == "refresh") {
                var scrollerwidth = 0;
                var gridObj = null;
                $scope.Remarks = null;
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
                $scope.Remarks = null;
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
            data: { 'data': DataToBeSaved , 'Remarks':$scope.Remarks},
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
            data: { 'data': DataToBeSaved , 'Remarks' : $scope.Remarks},
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