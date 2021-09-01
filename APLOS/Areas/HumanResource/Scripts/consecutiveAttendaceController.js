'use strict';
consecutiveAttendaceController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function consecutiveAttendaceController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Consecutive Work Days';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $scope.considerInOut = false;
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.PlantIdFromUI = null;
    $scope.PlantList = [];
    $scope.getPlant = function () {
        $http({
            method: 'GET',
            url: "humanresource/payrollReports/GetPlantList",
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
                    , width: 250
                });

        });
    }
    $scope.getPlant();

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
    $scope.presentComparator = ">=";
    $scope.DateWisePresentStatusList = [];
    $scope.GetGruopWiseDateWisePresentStatus = function () {

        //var DropDownListObj = $("#AttendanceDayStatusList").data("ejDropDownList");
        //var dayStatus = DropDownListObj.getSelectedValue();
        //dayStatus = '\'' + dayStatus.split(',').join('\',\'') + '\'';
        var dayStatus = null;
        var toDate = new Date($scope.hrPresentJSToDate);
        toDate.setDate(toDate.getDate());
        $scope.dataGrid = "#";

        var oneDay = 24 * 60 * 60 * 1000;

        var formDate = new Date($scope.hrPresentJSFromDate);

        var DropDownListObj = $("#ddlPlantList").data("ejDropDownList");
        var PlantId = DropDownListObj.getSelectedValue();

        var oneDay = 24 * 60 * 60 * 1000;

        var formDate = new Date($scope.hrPresentJSFromDate);

        var diffDaysLate = Math.ceil(Math.abs((formDate.getTime() - toDate.getTime()) / (oneDay)));


        if ($scope.hrPresentJSFromDate === "" || $scope.hrPresentJSFromDate === undefined) {
            throw ShowResult("Missing From Date", 'failure');
        }
        else if ($scope.hrPresentJSToDate === "" || $scope.hrPresentJSToDate === undefined) {
            throw ShowResult("Missing To Date", 'failure');
        }
        else if (formDate > toDate) {
            throw ShowResult("From Date Can not  be greater then To Date", 'failure');
        }
        else if (diffDaysLate < $scope.dayCountPresent) {
            throw ShowResult("Given Dates are not valid", 'failure');

        }
        else {

            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'HumanResource/ConsecutiveAttendaceAndOT/ConsecutivePresentStatusDynamic',
                data: {
                    'hrFromDate': $scope.hrPresentJSFromDate,
                    'hrToDate': $scope.hrPresentJSToDate,
                    'dayCount': $scope.dayCountPresent,
                    'presentComparator': $scope.presentComparator,
                    'CompanyId': $scope.companyId,
                    'dayStatus': dayStatus,
                    'considerInOut': $scope.considerInOut
                    , 'PlantId': PlantId
                }
            }).then(function successCallback(response) {

                if (response.data.length > 0) {
                    $scope.DateWisePresentStatusList = response.data;

                }
                else {
                    ShowResult("No Data Found", 'failure');
                }
            });

        }
    };
    $scope.workingHours = 12;
    $scope.workingHoursComparator = ">=";
    $scope.workingHoursFromDate = null;
    $scope.workingHoursToDate = null;

    $scope.WorkingHoursList = [];
    $scope.GetGruopWiseDateWiseWorkingHours = function () {

        var toDate = new Date($scope.workingHoursToDate);
        toDate.setDate(toDate.getDate());
        $scope.dataGrid = "#GridWorkingHours";

        if (presentTable.style.display === "none") {
            presentTable.style.display = "block";
        }
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'HumanResource/ConsecutiveAttendaceAndOT/GetEEmpJobCardInfoWithInDateTimes',
            data: {
                'wrHrFromDate': $scope.workingHoursFromDate,
                'wrHrToDate': $scope.workingHoursToDate,

                'hours': $scope.workingHours,
                'presentComparator': $scope.workingHoursComparator,
                'companyId': $scope.companyId,
                'considerInOut': $scope.considerInOut
            }
        }).then(function successCallback(response) {

            if (response.data.length > 0) {
                $scope.WorkingHoursList = response.data;
            }
            else {
                ShowResult("No Data Found", 'failure');
            }
        });


    };

    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.PrintPresent = function () {
        try {

            //var DropDownListObj = $("#AttendanceDayStatusList").data("ejDropDownList");
            //var dayStatus = DropDownListObj.getSelectedValue();
            //dayStatus = '\'' + dayStatus.split(',').join('\',\'') + '\'';

            var DropDownListObj = $("#ddlPlantList").data("ejDropDownList");
            var PlantId = DropDownListObj.getSelectedValue();

            var dayStatus = null;
            $http({
                method: 'POST',
                url: 'HumanResource/ConsecutiveAttendaceAndOT/PrintPresent',
                data: {
                    'hrFromDate': $scope.hrPresentJSFromDate,
                    'hrToDate': $scope.hrPresentJSToDate,
                    'dayCount': $scope.dayCountPresent,
                    'presentComparator': $scope.presentComparator,
                    'companyId': $scope.companyId,
                    'dayStatus': dayStatus,
                    'considerInOut': $scope.considerInOut,
                    'PlantId': PlantId
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.presentDaysList = [];
    $scope.ModalPresentEmpWiseDate = function (data) {
        //var DropDownListObj = $("#AttendanceDayStatusList").data("ejDropDownList");
        //var dayStatus = DropDownListObj.getSelectedValue();
        //dayStatus = '\'' + dayStatus.split(',').join('\',\'') + '\'';
        var dayStatus = null;
        $scope.ADGLUrl = 'HumanResource/ConsecutiveAttendaceAndOT/ModalEmployeeWisePresentDateList?companyId=' + $scope.companyId + "&plantId=" + $scope.plantId + "&hrFromDate=" + $scope.hrPresentJSFromDate + "&hrToDate=" + $scope.hrPresentJSToDate + "&EmpSystemId=" + data.EmpSystemID + "&dayCount=" + $scope.dayCountPresent + "&comparator=" + $scope.presentComparator + "&dayStatus=" + dayStatus + "&considerInOut=" + $scope.considerInOut;
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
    $scope.lateDayList = [];
    $scope.ModalPresentEmpWiseDateList = function (data) {
        //var DropDownListObj = $("#AttendanceDayStatusList").data("ejDropDownList");
        //var dayStatus = DropDownListObj.getSelectedValue();
        //dayStatus = '\'' + dayStatus.split(',').join('\',\'') + '\'';
        var dayStatus = null;
        $scope.ADGLUrl = 'HumanResource/ConsecutiveAttendaceAndOT/ModalEmployeeWisePresentStatusDateWiseList?companyId=' + $scope.companyId + "&plantId=" + $scope.plantId + "&hrFromDate=" + data.fromDate + "&hrToDate=" + data.toDate + "&EmpSystemId=" + data.EmpSystemID + "&dayCount=" + $scope.dayCountPresent + "&comparator=" + $scope.presentComparator + " &dayStatus=" + dayStatus + " &considerInOut=" + $scope.considerInOut;
        $scope.label = data.EmployeeName;
        $scope.empCode = data.EmployeeCode;
        $http({
            method: 'GET',
            url: $scope.ADGLUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.lateDayList = response.data;
        });
        angular.element(document.querySelector('#LateDateWiseList')).modal('show');
    };

    $scope.ShiftName = [];
    $scope.selectedShif = function () {
        var eDialog = $("#dialogShiftSelect").data("ejDialog");
        eDialog.close();
        $scope.ShiftName = [];
        var otcd = '';
        for (var i = 0; i < $scope.shiftinfo.length; i++) {

            if ($scope.shiftinfo[i].CheckBoxSelect === true) {
                if (baseService.isUndefinedOrNull(otcd)) {
                    otcd += "'" + $scope.shiftinfo[i].SystemID + "'";
                } else {
                    otcd += ",'" + $scope.shiftinfo[i].SystemID + "'";
                }
                $scope.ShiftName.push($scope.shiftinfo[i].UserName);
            }
        }
        $scope.CustomPara.ShiftId = otcd;

    };
    $scope.selectemployee = [];
    $scope.selectedSinglemployee = {};
    $scope.getAllShiftinfo = function () {

        var eDialog = $("#dialogShiftSelect").data("ejDialog");
        eDialog.open();

        $http({
            method: "GET",
            dataType: 'JSON',
            //data: { 'fromdate': $scope.FromDate, 'todate': $scope.ToDate },
            url: 'HumanResource/AttendanceManagement/getShift'

        }).then(function successCallback(response) {
            $scope.shiftinfo = response.data;

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
            data: { 'EmpId': $scope.selectedSinglemployee.Id, 'FDate': $scope.FromDate, 'TDate': $scope.ToDate },
            url: $scope.path + 'GetEmployeeWiseDataForOTConfirmation'

        }).then(function successCallback(response) {
            //$scope.employees = [];
            //$scope.employees = response.data.data;



            $scope.IsPreallocationBasedOT = response.data.IsPreallocationBasedOT;
            $scope.IsPunchBasedOT = response.data.IsPunchBasedOT;
            if (response.data.OTConsiderOn === 'Hour Minute Value') {
                $scope.ShowHourMinute = true;
            }
            if (response.data.OTConsiderOn === 'Decimal Value') {
                $scope.ShowDecimal = true;
            }

            //  IsPreallocationBasedOT: false,
            //  IsPunchBasedOT: false
            if (response.data.IsPreallocationBasedOT === true || response.data.IsPunchBasedOT === true) {
                $scope.employees = [];
                $scope.employees = response.data.data;

                $scope.TobeConfirmedCount = 0;
                if ($scope.employees.length > 0) {
                    $scope.TobeConfirmedCount = $scope.employees.length;
                }



                $scope.ShowOTValue = response.data.ShowOTValue;

                $scope.customPara.MinimumOTMinute = response.data.MinimumOTMinute;
                $scope.customPara.OTConsiderOn = response.data.OTConsiderOn;
                $scope.customPara.OTFractionCalculate = response.data.OTFractionCalculate;
                $scope.customPara.IsPreallocationBasedOT = !response.data.IsPreallocationBasedOT;
                $scope.customPara.IsPunchBasedOT = !response.data.IsPunchBasedOT;
                var gridObj = $("#GridEmpWise").data("ejGrid");
                gridObj.clearFiltering();

                if (response.data.OTConsiderOn === 'Hour Minute Value') {
                    //gridObj.hideColumns("DeviceOTHrHour");
                    //gridObj.hideColumns("DeviceOTHrMinute");
                    gridObj.hideColumns("DeviceOTHrInDecimal");
                    gridObj.hideColumns("OTPreallocationDecimal");
                    //gridObj.hideColumns("NormalOTHrHour");
                    //gridObj.hideColumns("NormalOTHrMinute");
                    gridObj.hideColumns("NormalOTHrInDecimal");
                }
                if (response.data.OTConsiderOn === 'Decimal Value') {
                    gridObj.hideColumns("DeviceOTHrHour");
                    gridObj.hideColumns("DeviceOTHrMinute");

                    gridObj.hideColumns("OTPreallocationHour");
                    gridObj.hideColumns("OTPreallocationMinute");

                    //gridObj.hideColumns("DeviceOTHrInDecimal");
                    gridObj.hideColumns("NormalOTHrHour");
                    gridObj.hideColumns("NormalOTHrMinute");
                    //gridObj.hideColumns("NormalOTHrInDecimal");
                }


                if (response.data.IsPreallocationBasedOT === true) {
                    gridObj.hideColumns("DeviceOTHrHour");
                    gridObj.hideColumns("DeviceOTHrMinute");
                    gridObj.hideColumns("DeviceOTHrInDecimal");
                    //gridObj.hideColumns("EmployeeName");
                }
                if (response.data.IsPunchBasedOT === true) {
                    gridObj.hideColumns("OTPreallocationHour");
                    gridObj.hideColumns("OTPreallocationMinute");
                    gridObj.hideColumns("OTPreallocationDecimal");
                    //gridObj.hideColumns("EmployeeName");
                }

                if (response.data.IsPreallocationBasedOT === true && response.data.IsPunchBasedOT === true) {

                    if (response.data.OTConsiderOn === 'Hour Minute Value') {


                        gridObj.showColumns("DeviceOTHrHour");
                        gridObj.showColumns("DeviceOTHrMinute");
                        gridObj.showColumns("OTPreallocationHour");
                        gridObj.showColumns("OTPreallocationMinute");

                    }
                    if (response.data.OTConsiderOn === 'Decimal Value') {
                        gridObj.showColumns("OTPreallocationDecimal");
                        gridObj.showColumns("DeviceOTHrInDecimal");




                    }


                }



            }


            //var gridObj = $("#GridChangeAttendance").data("ejGrid");
            //gridObj.refreshContent();
        });


    }

    $scope.actionCompleteSelected4 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridShiftSelect").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            // $scope.ShowResultCustom(e, 'failure');
        }
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

}