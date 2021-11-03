'use strict';
EmployeeWiseAttendanceLockController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function EmployeeWiseAttendanceLockController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Employee Lock And UnLock';
    $scope.Action = 'Save';
    $scope.path = 'humanresource/HrmsSettings/';
    $scope.getEmpListUrl = $scope.path + 'GetEmployeeData';
    $scope.saveLockUrl = $scope.path + 'CreateLockData';
    $scope.LastLockDateUrl = $scope.path + 'GetLastLockDate';
    $scope.LockDateListUrl = $scope.path + 'GetLockDateList';
    $scope.AllEmployeeListUrl = $scope.path + 'GetAllEmployeeList';
    //$scope.GetLockEmployeeListDataUrl = $scope.path + 'GetLockEmployeeListData';
    $scope.GetEmployeeWiseLockDataUrl = $scope.path + 'GetEmployeeWiseLockData';
    $scope.SaveEmployeeWiseLockDataUrl = $scope.path + 'CreateLockDataEmpWise'; 
    $scope.DisabledDates = [];

    $scope.CheckBoxValidation = {
        AllNewJoinEntered: false,
        EmpProfileApproved: false,
        AttendanceProcessed: false,
        ShiftAssignedorchanged: false,
        OTconfirmed: false,
        Leaveentered: false,
        SalaryStructueDefined: false,
        SalaryStructueApproved: false
    };

    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;
        $scope.employees = [];

    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab22 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet22 = function (tabNum) {
        return $scope.tabh === tabNum;
    };






    $scope.OpenTabname = 'EA';
    $scope.getLockDateList = function () {

        $.ajax({
            type: "GET",
            url: $scope.LockDateListUrl,

            dataType: "json",
            success: function (data) {

                $scope.DisabledDates = data.LastLockDate;

            }

        });

    };
    $scope.getLockDateList();

    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.loadstatus = false;

    $scope.DownloadData = function () {
        if ($scope.OpenTabname === 'EA') {
            var gridObj = $("#GridUnApprovedEmployeeList").ejGrid("instance");
            $scope.Print(gridObj);
            //$rootScope.report(gridObj);
        }
        else if ($scope.OpenTabname === 'OT') {
            var gridObj1 = $("#GridOTUnConfirmedEmployeeList").ejGrid("instance");
            $scope.Print(gridObj1);
        }
        else if ($scope.OpenTabname === 'AP') {
            var gridObj2 = $("#GridAttdencenotNotProcEmployeeList").ejGrid("instance");
            $scope.Print(gridObj2);
        }
        else if ($scope.OpenTabname === 'SA') {
            var gridObj3 = $("#GridShiftNotAssignEmployeeList").ejGrid("instance");
            $scope.Print(gridObj3);
        }

    };
    $scope.Print = function (gridObj) {
        //var gridObj = $("#DetailGrid").data("ejGrid");
        var data = gridObj.model.currentViewData;
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': data }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                // ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

            }
            else {

                location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };
    //var today = new Date();
    //var today_formatted = today.getFullYear() + '-' + (today.getMonth() + 1) + '-' + ('0' + today.getDate()).slice(-2);
    //var user_busy_days = ['2019-06-09', '2019-06-16', '2019-06-19'];
    // An array of dates






    $('.datepicker').datepicker({
        startDate: '-2m',
        endDate: '-0d',
       // datesDisabled: $scope.DisabledDates,
        format: 'dd-M-yyyy',
        todayHighlight: true,
        //minDate: 0,
        autoclose: true,
        inline: true,
        //sideBySide: true,
        //beforeShowDay: function (date) {

        //    var  calender_date = date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + ('0' + date.getDate()).slice(-2);

        //    var search_index = $.inArray(calender_date, user_busy_days);

        //    if (search_index > -1) {
        //        return { classes: 'non-highlighted-cal-dates', tooltip: 'User available on this day.' };
        //    } else {
        //        return { classes: 'highlighted-cal-dates', tooltip: 'User not available on this day.' };
        //    }
        //}
        beforeShowDay: function (date) {
            var eventDates = {};
            eventDates[new Date('12/04/2014')] = new Date('12/04/2014');
            eventDates[new Date('12/06/2014')] = new Date('12/06/2014');
            eventDates[new Date('12/20/2014')] = new Date('12/20/2014');

            var highlight = eventDates[date];
            if (highlight) {
                return [true, "event", highlight];
            } else {
                return [true, '', ''];
            }
        }

    });

    //$('.datepicker').on('dp.show', function (e) {
    //    $scope.highlight();
    //});
    //$('.datepicker').on('dp.update', function (e) {
    //    $scope.highlight();
    //});
    //$('.datepicker').on('dp.change', function (e) {
    //    $scope.highlight();
    //});

    //$scope.highlight = function () {
    //    var dateToHilight = ["03-Jul-2019"];
    //    var array = $(".datepicker").find(".day").toArray();
    //    for (var i = 0; i < array.length; i++) {
    //        var date = array[i].getAttribute("data-day");
    //        if (dateToHilight.indexOf(date) > -1) {
    //            array[i].style.color = "#090";
    //            array[i].style.fontWeight = "bold";
    //        }
    //    }
    //};


    //$scope.highlight();



    $scope.OTUnConfirmedEmployees = [];
    $scope.UnApprovedEmployees = [];
    $scope.ShiftNotAssignEmployees = [];
    $scope.AttdencenotNotProcEmployees = [];
    $scope.employees = [];
    $scope.LockEmpList = [];
    $scope.TobeLockEmpList = [];

    $scope.customPara = {
        lockDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy') 
    };

    $scope.OTUnConfirmedEmployeesCount = null;
    $scope.UnApprovedEmployeesCount = null;
    $scope.ShiftNotAssignEmployeesCount = null;
    $scope.AttdencenotNotProcEmployeesCount = null;
    $scope.LockEmpListCount = null;
    $scope.TobeLockEmpListCount = null;
    $scope.LastLockDate = null;
    $scope.DatePickerEnable = true;
    $scope.LoadButtonShow = false;
    $scope.LockButtonShow = false;


    //#region Tab




    $scope.tab = 1;
    $scope.setTab1 = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet1 = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.setTab2 = function (newTab) {
        $scope.tab = newTab;
        $scope.OpenTabname = 'EA';

    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.setTab3 = function (newTab) {
        $scope.tab = newTab;
        $scope.OpenTabname = 'OT';
        try {
            $scope.actionCompleteSelected1();
        } catch (e) {
            //
        }
        try {
            $scope.actionCompleteSelected2();
        } catch (e) {
            //
        }
        try {
            $scope.actionCompleteSelected3();
        } catch (e) {
            //
        }

    };
    $scope.isSet3 = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTab4 = function (newTab) {
        $scope.tab = newTab;
        $scope.OpenTabname = 'AP';
        try {
            $scope.actionCompleteSelected1();
        } catch (e) {
            //
        }
        try {
            $scope.actionCompleteSelected2();
        } catch (e) {
            //
        }
        try {
            $scope.actionCompleteSelected3();
        } catch (e) {
            //
        }

    };
    $scope.isSet4 = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.setTab5 = function (newTab) {
        $scope.tab = newTab;
        $scope.OpenTabname = 'SA';
        try {
            $scope.actionCompleteSelected1();
        } catch (e) {
            //
        }
        try {
            $scope.actionCompleteSelected2();
        } catch (e) {
            //
        }
        try {
            $scope.actionCompleteSelected3();
        } catch (e) {
            //
        }

    };
    $scope.isSet5 = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTab6 = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet6 = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion Tab
    $window.onload = function (event) {
        try {
            $scope.actionCompleteSelected();
        } catch (e) {
            //
        }

        try {
            $scope.actionCompleteSelected1();
        } catch (e) {
            //
        }
        try {
            $scope.actionCompleteSelected2();
        } catch (e) {
            //
        }
        try {
            $scope.actionCompleteSelected3();
        } catch (e) {
            //
        }
        try {
            $scope.actionCompleteSelected6();
        } catch (e) {
            //
        }
        try {
            $scope.actionCompleteSelected7();
        } catch (e) {
            //
        }
        //$scope.actionCompleteSelected1();
        //$scope.actionCompleteSelected2();
        //$scope.actionCompleteSelected3();

    };
    $window.onresize = function (event) {

        try {
            $scope.actionCompleteSelected();
        } catch (e) {
            //
        }

        try {
            $scope.actionCompleteSelected1();
        } catch (e) {
            //
        }
        try {
            $scope.actionCompleteSelected2();
        } catch (e) {
            //
        }
        try {
            $scope.actionCompleteSelected3();
        } catch (e) {
            //
        }

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridUnApprovedEmployeeList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                gridObj.clearFiltering();
                //$("#GridUnApprovedEmployeeList").children('.e-grid.e-headercell').css('height', '100px');
                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    $scope.actionCompleteSelected1 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridOTUnConfirmedEmployeeList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container
                gridObj.clearFiltering();
                $("#GridOTUnConfirmedEmployeeList").children('.e-grid.e-headercell').css('height', '100px');
                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    $scope.actionCompleteSelected2 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridAttdencenotNotProcEmployeeList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container
                gridObj.clearFiltering();
                $("#GridAttdencenotNotProcEmployeeList").children('.e-grid.e-headercell').css('height', '100px');
                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    $scope.actionCompleteSelected3 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridShiftNotAssignEmployeeList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container
                gridObj.clearFiltering();
                $("#GridShiftNotAssignEmployeeList").children('.e-grid.e-headercell').css('height', '100px');
                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    $scope.actionCompleteSelected6 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridLockEmpList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container
                gridObj.clearFiltering();
                $("#GridLockEmpList").children('.e-grid.e-headercell').css('height', '100px');
                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    $scope.actionCompleteSelected7 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridTobeLockEmployeeList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container
                gridObj.clearFiltering();
                $("#GridTobeLockEmployeeList").children('.e-grid.e-headercell').css('height', '100px');
                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };

    //#region selectall
    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === "";
    };
    function checkChangeemployee7(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.TobeLockEmpList, { 'SystemID': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState === "check")
                row[0].CheckBoxSelect = true;
            else
                row[0].CheckBoxSelect = false;
        }

    }
    function headCheckChangeemployee7(e) {
        if (e.model.checkState === "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#GridTobeLockEmployeeList").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.TobeLockEmpList.length; i++) {
                    $scope.TobeLockEmpList[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.TobeLockEmpList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.TobeLockEmpList[i].SystemID === filtered[j].SystemID)
                            $scope.TobeLockEmpList[i].CheckBoxSelect = true;
                    }

                }
            }

            var checkbox = $("#GridTobeLockEmployeeList .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridTobeLockEmployeeList .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridTobeLockEmployeeList .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridTobeLockEmployeeList .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee7 });
            }
        }
        else {
            var filtered = $("#GridTobeLockEmployeeList").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.TobeLockEmpList.length; i++) {
                    $scope.TobeLockEmpList[i].CheckBoxSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.TobeLockEmpList[i].SystemID == filtered[j].SystemID)
                            $scope.TobeLockEmpList[i].CheckBoxSelect = false;
                    }

                }
            }
            var checkbox = $("#GridTobeLockEmployeeList .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridTobeLockEmployeeList .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridTobeLockEmployeeList .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridTobeLockEmployeeList .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee7 });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee7 = function (args) {
        $("#GridTobeLockEmployeeList .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk7").ejCheckBox({ "change": headCheckChangeemployee7 });

    }
    $scope.refreshTemplateemployee7 = function (args) {
        $("#headchk7").ejCheckBox({ "change": CheckBoxSelectAll });
        //if (args.rowIndex == 0) {
        //    $("#headchk7").ejCheckBox({ "change": CheckBoxSelectAll });
        //}

        //var valobj = $($("#GridTobeLockEmployeeList .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        //var val = $($("#GridTobeLockEmployeeList .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        //$($("#GridTobeLockEmployeeList .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        //var row = $filter('filter')($scope.TobeLockEmpList, { 'SystemID': val });
        //if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
        //    if (row[0].CheckBoxSelect == true)
        //        $($("#GridTobeLockEmployeeList .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
        //    else
        //        $($("#GridTobeLockEmployeeList .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        //}
        //$($("#GridTobeLockEmployeeList .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee7 });
    };

   function CheckBoxSelectAll(e) {
       console.log('ok');


       if (e.model.checkState === "check") {

           console.log('c-ok');
           for (var i = 0; i < $scope.TobeLockEmpList.length; i++) {
              
                  
                       $scope.TobeLockEmpList[i].CheckBoxSelect = true;
              

           }
       }
       else {
           console.log('co-ok');
           for (var i = 0; i < $scope.TobeLockEmpList.length; i++) {


               $scope.TobeLockEmpList[i].CheckBoxSelect = false;


           }
       }
       var gridObj = $("#GridTobeLockEmployeeList").data("ejGrid");
       gridObj.refreshContent();
    };
    // #endregion Tab



    $scope.getEmpListData = function () {

        $.ajax({
            type: "GET",
            url: $scope.getEmpListUrl,
            data:
            {
                'lockDate': $scope.customPara.lockDate
            },
            dataType: "json",
            success: function (data) {
                $scope.employees = data.UnApprovedEmployees;

                $scope.OTUnConfirmedEmployees = data.OTUnConfirmedEmployees;
                $scope.UnApprovedEmployees = data.UnApprovedEmployees;

                $scope.ShiftNotAssignEmployees = data.ShiftNotAssignEmployees;
                $scope.AttdencenotNotProcEmployees = data.AttdencenotNotProcEmployees;



                $scope.OTUnConfirmedEmployeesCount = data.OTUnConfirmedEmployees.length;
                $scope.UnApprovedEmployeesCount = data.UnApprovedEmployees.length;
                $scope.ShiftNotAssignEmployeesCount = data.ShiftNotAssignEmployees.length;
                $scope.AttdencenotNotProcEmployeesCount = data.AttdencenotNotProcEmployees.length;

                $scope.LockEmpList = data.LockEmpList;
                $scope.TobeLockEmpList = data.ToBeLockEmpList;

                $scope.LockEmpListCount = data.LockEmpList.length;
                $scope.TobeLockEmpListCount = data.ToBeLockEmpList.length;



                //if (baseService.isUndefinedOrNull(data.LastLockDate)) {
                //    $scope.DatePickerEnable = false;
                //} else {


                //    //var myDate = new Date();

                //    var lockDay = new Date(data.LastLockDate);
                //    lockDay.setDate(lockDay.getDate() + 1);

                //    $scope.customPara.lockDate = $filter('dateFiltering')(lockDay, 'dd-M-yyyy');
                //    $scope.DatePickerEnable = true;

                //}
                $scope.LoadButtonShow = false;
                $scope.LockButtonShow = true;
                //$scope.getLockDateList();
            }

        });

    };
    $scope.getLastLockDate = function () {

        $.ajax({
            type: "GET",
            url: $scope.LastLockDateUrl,

            dataType: "json",
            success: function (data) {



                if (baseService.isUndefinedOrNull(data.LastLockDate)) {
                    $scope.DatePickerEnable = false;
                    $scope.LoadButtonShow = true;

                } else {


                    //var myDate = new Date();

                    var lockDay = new Date(data.LastLockDate);
                    lockDay.setDate(lockDay.getDate() + 1);

                    $scope.customPara.lockDate = $filter('dateFiltering')(lockDay, 'dd-M-yyyy');
                    $scope.DatePickerEnable = true;
                    $scope.getEmpListData();
                    $scope.LockButtonShow = true;
                }
            }

        });

    };
    //$scope.getLastLockDate();

    $scope.messageText = "";

    $scope.SaveLockData = function () {

        try {

            var LockDateWiseEmployeeList = [];
            for (var i = 0; i < $scope.TobeLockEmpList.length; i++) {

                if ($scope.TobeLockEmpList[i].CheckBoxSelect === true) {
                    LockDateWiseEmployeeList.push($scope.TobeLockEmpList[i].SystemID);
                }

            }

            if ($scope.CheckBoxValidation.AllNewJoinEntered === false ||
                $scope.CheckBoxValidation.EmpProfileApproved === false ||
                $scope.CheckBoxValidation.AttendanceProcessed === false ||
                $scope.CheckBoxValidation.ShiftAssignedorchanged === false ||
                $scope.CheckBoxValidation.OTconfirmed === false ||
                $scope.CheckBoxValidation.Leaveentered === false ||
                $scope.CheckBoxValidation.SalaryStructueDefined === false ||
                $scope.CheckBoxValidation.SalaryStructueApproved === false) {
                throw "Please select all options.";
            }
            if ($scope.OTUnConfirmedEmployeesCount > 0) {
                throw "Please Confirmed all Employees OT.";
                //$scope.ShowResultCustom('Please Confirmed all Employees OT.', 'failure');
            }
            if ($scope.UnApprovedEmployeesCount > 0) {
                throw "Please Confirmed all Employees  Approved.";
                //$scope.ShowResultCustom('Please Confirmed all Employees Approved.', 'failure');
            }
            if ($scope.ShiftNotAssignEmployeesCount > 0) {
                throw "Please Confirmed all Employees Shift Assign.";
                //$scope.ShowResultCustom('Please Confirmed all Employees Shift Assign.', 'failure');
            }
            if ($scope.AttdencenotNotProcEmployeesCount > 0) {
                throw "Please Confirmed all Employees Attdence Proc.";
                //$scope.ShowResultCustom('Please Confirmed all Employees Attdence Proc', 'failure');
            }



            $.ajax({
                type: "POST",
                url: $scope.saveLockUrl,
                data:
                {
                    'lockDate': $scope.customPara.lockDate
                   ,'LockDateWiseEmployeeList': LockDateWiseEmployeeList
                },
                dataType: "json",
                success: function (data) {
                    ShowResult(data.Message, "success");
                    //$scope.getLockDateList();
                    $scope.getEmpListData();

                },
                error: function (data) {
                    ShowResult(data.Message, "failure");

                }

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }



    };
    $scope.xSaveLockData = function () {

        ////#validation Start
        //if ($scope.CheckBoxValidation.AllNewJoinEntered === false
        //    || $scope.CheckBoxValidation.EmpProfileApproved === false
        //    || $scope.CheckBoxValidation.AttendanceProcessed === false
        //    || $scope.CheckBoxValidation.ShiftAssignedorchanged === false
        //    || $scope.CheckBoxValidation.OTconfirmed === false
        //    || $scope.CheckBoxValidation.Leaveentered === false
        //    || $scope.CheckBoxValidation.SalaryStructueDefined === false
        //    || $scope.CheckBoxValidation.SalaryStructueApproved === false)
        //{
        //    throw ex;
        //}

        //#validation End




        try {

            var EmployeeWiseLockDateList = [];
            for (var i = 0; i < $scope.EmployeeLockData.length; i++) {

                if ($scope.EmployeeLockData[i].CheckBoxSelect === true) {
                    EmployeeWiseLockDateList.push($scope.EmployeeLockData[i].WorkDate);
                }

            }

            if ($scope.CheckBoxValidation.AllNewJoinEntered === false ||
                $scope.CheckBoxValidation.EmpProfileApproved === false ||
                $scope.CheckBoxValidation.AttendanceProcessed === false ||
                $scope.CheckBoxValidation.ShiftAssignedorchanged === false ||
                $scope.CheckBoxValidation.OTconfirmed === false ||
                $scope.CheckBoxValidation.Leaveentered === false ||
                $scope.CheckBoxValidation.SalaryStructueDefined === false ||
                $scope.CheckBoxValidation.SalaryStructueApproved === false) {
                throw "Please select all options.";
            }
            if ($scope.OTUnConfirmedEmployeesCount > 0) {
                throw "Please Confirmed all Employees OT.";
                //$scope.ShowResultCustom('Please Confirmed all Employees OT.', 'failure');
            }
            if ($scope.UnApprovedEmployeesCount > 0) {
                throw "Please Confirmed all Employees  Approved.";
                //$scope.ShowResultCustom('Please Confirmed all Employees Approved.', 'failure');
            }
            if ($scope.ShiftNotAssignEmployeesCount > 0) {
                throw "Please Confirmed all Employees Shift Assign.";
                //$scope.ShowResultCustom('Please Confirmed all Employees Shift Assign.', 'failure');
            }
            if ($scope.AttdencenotNotProcEmployeesCount > 0) {
                throw "Please Confirmed all Employees Attdence Proc.";
                //$scope.ShowResultCustom('Please Confirmed all Employees Attdence Proc', 'failure');
            }



            $.ajax({
                type: "POST",
                url: $scope.saveLockUrl,
                data:
                {
                    'lockDate': $scope.customPara.lockDate
                },
                dataType: "json",
                success: function (data) {
                    ShowResult(data.Message, "success");
                    $scope.getLockDateList();

                },
                error: function (data) {
                    ShowResult(data.Message, "failure");

                }

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }



    };
    $scope.SaveData = function () {

        $.ajax({
            type: "POST",
            url: $scope.OTConfirmationSaveUrl,
            data:
            {
                'employeeOTInformation': $scope.employees,
                'ProcDate': $scope.customPara.procdate
            },
            dataType: "json",
            success: function (data) {

                if (data.Error === true) {
                    ShowResult(data.Message, 'failure');
                }
                else {
                    ShowResult(data.Message, 'success');
                }

            }

        });

    };

    $scope.ShowResultCustom = function (message, type) {

        $scope.messageText = message;
        $scope.messageTitle = "Message";
        if (type === "success")
            $("#dialogMessage").ejDialog("setTitle", "Success");
        if (type === "failure")
            $("#dialogMessage").ejDialog("setTitle", "Error");

        var eDialog = $("#dialogMessage").data("ejDialog");
        eDialog.open();

    };

    $scope.ShowDate = function () {
        $('.datepicker').datepicker({
            //startDate: '-3d',
            datesDisabled: ['07/11/2019', '07/15/2019']

            //multidate: true,
            //todayHighlight: true,
            //minDate: 0,
            //beforeShowDay: function (date) {
            //    var hilightedDays = [1, 3, 8, 20, 21, 16, 26, 30];
            //    if (~hilightedDays.indexOf(date.getDate())) {
            //        return { classes: 'highlight', tooltip: 'Title' };
            //    }
            //}

        });
        $("td[data-day='08/23/2016']").css('background', 'blue');

    };

    //$scope.CheckBoxValidation = {
    //    AllNewJoinEntered: false,
    //    EmpProfileApproved: false,
    //    AttendanceProcessed: false,
    //    ShiftAssignedorchanged: false,
    //    OTconfirmed: false,
    //    Leaveentered: false,
    //    SalaryStructueDefined: false,
    //    SalaryStructueApproved: false
    //};



    //#region Employee wise
    $scope.FromDate = null;
    $scope.ToDate = null;
    $scope.selectSignleEmployee = null;
    $scope.EmployeeLockData = [];
    $scope.allEmployeeList = [];
    $scope.getAllEmployee = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "Please Date.";
            }
            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "Please Date.";
            }
            var eDialog = $("#dialogEmployeeSelect").data("ejDialog");


            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'fromdate': $scope.FromDate, 'todate': $scope.ToDate },
                url: $scope.AllEmployeeListUrl

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                }
                else {
                    eDialog.open();
                    $scope.allEmployeeList = [];
                    $scope.allEmployeeList = response.data.Employees;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.selectSignleEmployee = function (args) {
        var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
        eDialog.close();
        if (baseService.isUndefinedOrNull(args) === false)
            $scope.selectedSinglemployee = args.data;

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'empsystemid': $scope.selectedSinglemployee.SystemID, 'fromdate': $scope.FromDate, 'todate': $scope.ToDate },
            url: $scope.GetEmployeeWiseLockDataUrl

        }).then(function successCallback(response) {
           $scope.EmployeeLockData = response.data.Employees;



        });


    };


    //$scope.EmployeeWiseLockDate = [];
    $scope.SaveEmployeeWiseLockData = function () {
       
        var EmployeeWiseLockDateList = [];
        for (var i = 0; i < $scope.EmployeeLockData.length; i++) {

            if ($scope.EmployeeLockData[i].CheckBoxSelect === true) {
                EmployeeWiseLockDateList.push($scope.EmployeeLockData[i].WorkDate);
            }

        }
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'EmpSystemId': $scope.selectedSinglemployee.SystemID, 'EmployeeWiseLockDateList': EmployeeWiseLockDateList },
            url: $scope.SaveEmployeeWiseLockDataUrl

        }).then(function successCallback(response) {

            if (response.data.Error === true) {
                ShowResult(response.Message, 'failure');
            }
            else {
                //$scope.EmployeeLockData = response.data.Employees;
                
                ShowResult(response.data.Message, "success");
                $scope.EmployeeLockData = [];
                var gridObj = $("#GridEmpWise").data("ejGrid");
                gridObj.refreshContent();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });


    };


    //$scope.testFunc = function () {
    //    try {
    //        var dates = $scope.getDates(new Date('01-sep-2019'), new Date('30-sep-2019'));
    //        dates.forEach(function (date) {
    //            console.log(date);
    //        });
    //    } catch (e) {
    //        ///
    //        console.log(e);
    //    }


    //};

    //$scope.testFunc();
    //$scope.getDates = function (startDate, endDate) {
    //    var dates = [],
    //        currentDate = startDate,
    //        addDays = function (days) {
    //            var date = new Date(this.valueOf());
    //            date.setDate(date.getDate() + days);
    //            return date;
    //        };
    //    while (currentDate <= endDate) {
    //        dates.push(currentDate);
    //        currentDate = addDays.call(currentDate, 1);
    //    }
    //    return dates;
    //};

    // Usage



    $scope.actionCompleteSelected4 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridEmpWise").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            // $scope.ShowResult(e, 'failure');
        }
    };
    function checkChangeemployee4(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.EmployeeLockData, { 'WorkDate': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState === "check")
                if (row[0].IsLock === false)
                    row[0].CheckBoxSelect = true;
                else
                    row[0].CheckBoxSelect = false;
        }

    }
    function headCheckChangeemployee4(e) {
        if (e.model.checkState === "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#GridEmpWise").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.EmployeeLockData.length; i++) {
                    $scope.EmployeeLockData[i].CheckBoxSelect = false;
                    if ($scope.EmployeeLockData[i].IsLock === false)
                        $scope.EmployeeLockData[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeLockData.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        $scope.EmployeeLockData[i].CheckBoxSelect = false;
                        if ($scope.EmployeeLockData[i].WorkDate === filtered[j].WorkDate && $scope.EmployeeLockData[i].IsLock === false)
                            $scope.EmployeeLockData[i].CheckBoxSelect = true;
                    }

                }
            }

            //var checkbox = $("#GridEmpWise .rowCheckbox").ejCheckBox();
            //for (var i = 0; i < checkbox.length; i++) {

            //    $($("#GridEmpWise .rowCheckbox")[i]).ejCheckBox({ "change": null });
            //    $($("#GridEmpWise .rowCheckbox")[i]).ejCheckBox({ "checked": true });
            //    $($("#GridEmpWise .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee4 });

            //}
        }
        else {
            



            for (var i = 0; i < $scope.EmployeeLockData.length; i++) {
                $scope.EmployeeLockData[i].CheckBoxSelect = false;
            }

            
        }
        var _gridObj = $("#GridEmpWise").data("ejGrid");
        _gridObj.refreshContent(true);
        //header level check
    }
    $scope.dataBoundemployee4 = function (args) {
        $("#GridEmpWise .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk4").ejCheckBox({ "change": headCheckChangeemployee4 });
        //$("#EntityFilterGrid").children('.e-pager.e-js.e-pager').hide();
        //$("#EntityFilterGrid").children('.e-gridcontent.e-droppable.e-js').hide();
        //$("#EntityFilterGrid").children('.e-gridcontent').hide();
        //$("#EntityFilterGrid").children('.e-grid.e-headercell').css('background-color', 'red');
    }
    $scope.refreshTemplateemployee4 = function (args) {
        //if (args.rowIndex == 0) {
        //    $("#headchk4").ejCheckBox({ "change": headCheckChangeemployee4 });
        //}
        $("#headchk4").ejCheckBox({ "change": headCheckChangeemployee4 });
        //var valobj = $($("#GridEmpWise .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        //var val = $($("#GridEmpWise .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        //$($("#GridEmpWise .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        //var row = $filter('filter')($scope.EmployeeLockData, { 'WorkDate': val });
        //if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
        //    if (row[0].CheckBoxSelect === true)
        //        $($("#GridEmpWise .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
        //    else
        //        $($("#GridEmpWise .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        //}
        //$($("#GridEmpWise .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee4 });
    }








    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });        
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        console.log('ok');


        if (e.model.checkState === "check") {

            for (var i = 0; i < $scope.EmployeeLockData.length; i++) {
                $scope.EmployeeLockData[i].CheckBoxSelect = false;
                if ($scope.EmployeeLockData[i].IsLock === false)
                    $scope.EmployeeLockData[i].CheckBoxSelect = true;
            }
        }
        else {
            console.log('co-ok');
            for (var i = 0; i < $scope.EmployeeLockData.length; i++) {

                $scope.EmployeeLockData[i].CheckBoxSelect = false;


            }
        }
        var gridObj = $("#GridEmpWise").data("ejGrid");
        gridObj.refreshContent();
    };
    //#endregion

}

