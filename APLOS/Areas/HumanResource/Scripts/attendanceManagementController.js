'use strict';
attendanceManagementController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller','$window'];
function attendanceManagementController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Shift Report';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.ShiftReport = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        EmployeeId: null,
        ReportFormat: 'Pdf'
    };

    $scope.dataList = [];
        $scope.employeeInfo = {};
    $scope.GetEmployeeDeleteInfo = function () {
        $scope.dataList = [];
        $http({
            method: 'GET',
            url: 'employees/EmployeeDelete/getemployeeDelete'
        }).then(function successCallback(response) {
            $scope.dataList = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }


    $scope.employeeInfo = {};
    $scope.SetData = function (obj) {
        var emp = obj.data;
        $scope.employeeInfo.EmpSystemID = emp.SystemID; 
        $scope.employeeInfo.EmployeeCode = emp.EmployeeCode;
        $scope.employeeInfo.EmployeeName = emp.EmployeeName;
        $scope.employeeInfo.DOJ = emp.DOJ;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');

    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }
    
    $scope.report = function () {
        try {
            if ($scope.employeeInfo.EmpSystemID == null) {
                throw "Please Select Employee..";
            }
            if (baseService.isUndefinedOrNull($scope.ShiftReport.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
            }
            else if (baseService.isUndefinedOrNull($scope.ShiftReport.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
            }
            else if (new Date($scope.ShiftReport.FromDate) > new Date($scope.ShiftReport.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
            }
            else if (new Date($scope.ShiftReport.ToDate) < new Date($scope.ShiftReport.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
            }
            else {
                var url = 'HumanResource/AttendanceManagement/GetShiftReport?reportFormat=' + $scope.ShiftReport.ReportFormat + '&fromDate=' + $scope.ShiftReport.FromDate + '&toDate=' + $scope.ShiftReport.ToDate + '&employeeId=' + $scope.employeeInfo.EmpSystemID + '&EmpDoj=' + $scope.employeeInfo.DOJ;

                $window.open(url, '_blank');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };




    $scope.shiftinfo = [];
    $scope.GetShiftInfo = function () {

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'HumanResource/AttendanceManagement/getShift'

        }).then(function successCallback(response) {
            $scope.shiftinfo = response.data;
        });


    }
    $scope.GetShiftInfo();
    $scope.CustomPara = {
        FromDate: null,
        ToDate: null,
        ShiftId: null,      
        Hr: null,
        Min: null       
    };
    $scope.ShiftName = [];

    $scope.tiffinBillreport = function () {
        try {
            $scope.ShiftReport.ReportFormat = 'Excell';

            if (baseService.isUndefinedOrNull($scope.CustomPara.FromDate)) {
                throw "Enter valid From Date.";
            }
            if (baseService.isUndefinedOrNull($scope.CustomPara.ToDate)) {
                throw "Enter valid To Date.";
            }
            if (baseService.isUndefinedOrNull($scope.CustomPara.Hr)) {
                throw "Enter valid  Hr.";
            }
            if (baseService.isUndefinedOrNull($scope.CustomPara.Min)) {
                throw "Enter valid min.";
            }
            if (baseService.isUndefinedOrNull($scope.CustomPara.ShiftId)) {
                throw "Enter valid Shift.";
            }
            var url = 'HumanResource/AttendanceManagement/GetTifineReport?fromDate=' + $scope.CustomPara.FromDate + '&toDate=' + $scope.CustomPara.ToDate + '&ShiftId=' + $scope.CustomPara.ShiftId + '&Hr=' + $scope.CustomPara.Hr + '&Min=' + $scope.CustomPara.Min;

            $window.open(url, '_blank');
        } catch (e) {
            ShowResult(e, 'failure');
        }
       
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
                    otcd+="'"+$scope.shiftinfo[i].SystemID+"'";
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
            url:  'HumanResource/AttendanceManagement/getShift'

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