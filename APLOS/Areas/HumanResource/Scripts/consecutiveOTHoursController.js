'use strict';
consecutiveOTHoursController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function consecutiveOTHoursController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Consecutive Work Hours';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.dataBasedOn = 'Daily';
    $scope.timeShow = true;
    $scope.timeNotShow = false;
    $scope.workingHours = 12;
    $scope.isDataBasedOn = function () {
        if ($scope.dataBasedOn == 'Daily') {
            $scope.timeShow = true;
            $scope.timeNotShow = false;
            $scope.workingHours = 12;
        }
        else if ($scope.dataBasedOn == 'Period') {
            $scope.workingHours = 60;
            $scope.timeShow = false;
            $scope.timeNotShow = true;
        }
    }
    $scope.ShiftReport = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        EmployeeId: null,
        ReportFormat: 'Pdf'
    };
    $scope.AttendanceDayStatusList = [];
    cboService.getAttendanceDayStatus(function (result) {
        $scope.AttendanceDayStatusList = result;
    });

    var today = new Date();
    var last30Days = new Date(today.getFullYear(), today.getMonth(), today.getDate() - 30)
    $scope.hrPresentJSFromDate = $filter('dateFiltering')(last30Days, 'dd-MMMM-yyyy');
    $scope.hrPresentJSToDate = $filter('dateFiltering')(Date.now(), 'dd-MMMM-yyyy');

    $scope.dayCountPresent = 12;
    $scope.dateDiffPresent = function () {

        var toDate = new Date($scope.hrPresentJSToDate);
        toDate.setDate(toDate.getDate());

        var oneDay = 24 * 60 * 60 * 1000;

        var formDate = new Date($scope.hrPresentJSFromDate);

        var diffDaysPresent = Math.ceil(Math.abs((formDate.getTime() - toDate.getTime()) / (oneDay)));
        $scope.dayCountPresent = diffDaysPresent;
    };

    $scope.workingHoursComparator = ">=";
    $scope.workingHoursFromDate = $scope.hrPresentJSFromDate;
    $scope.workingHoursToDate = $scope.hrPresentJSToDate;
    $scope.WorkingHoursPeriodList  = []
    $scope.WorkingHoursList = [];
    $scope.GetGruopWiseDateWiseWorkingHours = function () {
        var toDate = new Date($scope.workingHoursToDate);
        toDate.setDate(toDate.getDate());
        $scope.dataGrid = "#GridWorkingHours";
        var DropDownListObj = $("#ddlPlantList").data("ejDropDownList");
        var PlantId = DropDownListObj.getSelectedValue();
        if ($scope.dataBasedOn == 'Daily') {
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'HumanResource/ConsecutiveAttendaceAndOT/GetWorkingHoursDaily',
                data: {
                    'wrHrFromDate': $scope.workingHoursFromDate,
                    'wrHrToDate': $scope.workingHoursToDate,

                    'hours': $scope.workingHours,
                    'presentComparator': $scope.workingHoursComparator,
                    'companyId': $scope.companyId, 'PlantId': PlantId
                }
            }).then(function successCallback(response) {

                if (response.data.length > 0) {
                    for (var i = 0; i < response.data.length; i++) {
                        try {

                            if (angular.isUndefinedOrNull(response.data[i]["DOJS"]) == false)
                                response.data[i]["DOJS"] = new Date(response.data[i]["DOJS"]);

                            if (angular.isUndefinedOrNull(response.data[i]["DOSs"]) == false)
                                response.data[i]["DOSs"] = new Date(response.data[i]["DOSs"]);


                            if (angular.isUndefinedOrNull(response.data[i]["InTime"]) == false)
                                response.data[i]["InTime"] = new Date(response.data[i]["InTime"]);


                            if (angular.isUndefinedOrNull(response.data[i]["OutTime"]) == false)
                                response.data[i]["OutTime"] = new Date(response.data[i]["OutTime"]);

                        } catch (e) {

                        }
                    }

                    $scope.WorkingHoursList = response.data;
                }
                else {
                    $scope.WorkingHoursList = [];
                    ShowResult("No Data Found", 'failure');
                }
            });
        }
        else if ($scope.dataBasedOn == 'Period') {
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'HumanResource/ConsecutiveAttendaceAndOT/GetWorkingHoursPeriod',
                data: {
                    'wrHrFromDate': $scope.workingHoursFromDate,
                    'wrHrToDate': $scope.workingHoursToDate,

                    'hours': $scope.workingHours,
                    'presentComparator': $scope.workingHoursComparator,
                    'companyId': $scope.companyId, 'PlantId': PlantId
                }
            }).then(function successCallback(response) {

                if (response.data.length > 0) {
                    for (var i = 0; i < response.data.length; i++) {
                        try {

                            if (angular.isUndefinedOrNull(response.data[i]["DOJS"]) == false)
                                response.data[i]["DOJS"] = new Date(response.data[i]["DOJS"]);
                            if (angular.isUndefinedOrNull(response.data[i]["DOSs"]) == false)
                                response.data[i]["DOSs"] = new Date(response.data[i]["DOSs"]);
                        } catch (e) {

                        }
                    }

                    $scope.WorkingHoursPeriodList = response.data;
                }
                else {
                    $scope.WorkingHoursPeriodList = [];
                    ShowResult("No Data Found", 'failure');
                }
            });
        }

        


    };
    $scope.PrintEmployeeWorkHourReport = function () {
        try {
            var DropDownListObj = $("#ddlPlantList").data("ejDropDownList");
            var PlantId = DropDownListObj.getSelectedValue();
            if ($scope.dataBasedOn == 'Daily') {
                $http({
                    method: 'POST',
                    url: 'HumanResource/ConsecutiveAttendaceAndOT/PrintEmployeeWorkHourReport',
                    data: {
                        'wrHrFromDate': $scope.workingHoursFromDate,
                        'ToDate': $scope.workingHoursToDate,
                        'comparator': $scope.workingHoursComparator,
                        'workingHour': $scope.workingHours, 'PlantId': PlantId
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    }
                });
            }
            else if ($scope.dataBasedOn == 'Period') {
                $http({
                    method: 'POST',
                    url: 'HumanResource/ConsecutiveAttendaceAndOT/PrintEmployeeWorkHourReportPeriod',
                    data: {
                        'wrHrFromDate': $scope.workingHoursFromDate,
                        'ToDate': $scope.workingHoursToDate,
                        'comparator': $scope.workingHoursComparator,
                        'workingHour': $scope.workingHours, 'PlantId': PlantId
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    }
                });
            }

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.presentDaysList = [];
    $scope.ModalPresentEmpWiseDate = function (data) {
        var DropDownListObj = $("#AttendanceDayStatusList").data("ejDropDownList");
        var dayStatus = DropDownListObj.getSelectedValue();
        dayStatus = '\'' + dayStatus.split(',').join('\',\'') + '\'';
        $scope.ADGLUrl = 'HumanResource/ConsecutiveAttendaceAndOT/ModalEmployeeWisePresentDateList?companyId=' + $scope.companyId + "&plantId=" + $scope.plantId + "&hrFromDate=" + $scope.hrPresentJSFromDate + "&hrToDate=" + $scope.hrPresentJSToDate + "&EmpSystemId=" + data.EmpSystemID + "&dayCount=" + $scope.dayCountPresent + "&comparator=" + $scope.presentComparator + "&dayStatus=" + dayStatus + "";
        $scope.label = data.EmployeeName;
        $scope.empCode = data.EmployeeCode;
        $http({
            method: 'GET',
            url: $scope.ADGLUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.presentDaysList = response.data;
        });
        angular.element(document.querySelector('#presentDaysCountList')).modal('show');
    };


    function checkChangeemployee4(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.shiftinfo, { 'SystemID': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState === "check")
                row[0].CheckBoxSelect = true;
            else
                row[0].CheckBoxSelect = false;
        }

    }
    function headCheckChangeemployee4(e) {
        if (e.model.checkState === "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#GridShiftSelect").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.shiftinfo.length; i++) {
                    $scope.shiftinfo[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.shiftinfo.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.shiftinfo[i].SystemID === filtered[j].SystemID)
                            $scope.shiftinfo[i].CheckBoxSelect = true;
                    }

                }
            }

            var checkbox = $("#GridShiftSelect .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridShiftSelect .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridShiftSelect .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridShiftSelect .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee4 });
            }
        }
        else {
            var filtered = $("#GridShiftSelect").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.shiftinfo.length; i++) {
                    $scope.shiftinfo[i].CheckBoxSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.shiftinfo[i].SystemID == filtered[j].SystemID)
                            $scope.shiftinfo[i].CheckBoxSelect = false;
                    }

                }
            }
            var checkbox = $("#GridShiftSelect .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridShiftSelect .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridShiftSelect .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridShiftSelect .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee4 });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee4 = function (args) {
        $("#GridShiftSelect .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk4").ejCheckBox({ "change": headCheckChangeemployee4 });
        //$("#EntityFilterGrid").children('.e-pager.e-js.e-pager').hide();
        //$("#EntityFilterGrid").children('.e-gridcontent.e-droppable.e-js').hide();
        //$("#EntityFilterGrid").children('.e-gridcontent').hide();
        //$("#EntityFilterGrid").children('.e-grid.e-headercell').css('background-color', 'red');
    }
    $scope.refreshTemplateemployee4 = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk4").ejCheckBox({ "change": headCheckChangeemployee4 });
        }

        var valobj = $($("#GridShiftSelect .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridShiftSelect .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridShiftSelect .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.shiftinfo, { 'SystemID': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].CheckBoxSelect == true)
                $($("#GridShiftSelect .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridShiftSelect .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridShiftSelect .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee4 });
    }
    $scope.PrintGRDes = function () {
        var gridObj = $($scope.dataGrid).data("ejGrid");
        var data = gridObj.model.dataSource();
        //data = ej.DataManager(data).executeLocal(ej.Query().select(["EmployeeName", "EmployeeCode", "LegalDesignation", "EmployeeCategorys", "DOJS", "DOSs", "InTime", "OutTime", "WorkHour"]));
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': data }
            // dataType: 'JSON'
            //, contentType: "application/json charset=utf-8"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');
            }
            else {
                window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };

    $scope.PlantList = [];
    $scope.getPlant = function () {
        $http({
            method: 'GET',
            url: 'Attendances/AttendanceProcessUI/GetPlantList',
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;

            var index = 0;
            for (var i = 0; i < $scope.PlantList.length; i++) {
                if ($scope.PlantList[i].PlantId == $window.plantId) {
                    index = i;
                }
            }

            $('#ddlPlantList').ejDropDownList(
                {
                    dataSource: $scope.PlantList,
                    fields: { text: "PlantName", value: "PlantId" },
                    selectedIndex: index, showCheckBox: true, multiSelectMode: ej.MultiSelectMode.VisualMode
                    , width: 330
                });


        });
    }
    $scope.getPlant();

}