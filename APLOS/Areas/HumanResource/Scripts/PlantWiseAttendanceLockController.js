'use strict';
PlantWiseAttendanceLockController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function PlantWiseAttendanceLockController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Lock';
    $scope.Action = 'Save';
    $scope.path = 'humanresource/HrmsSettings/';
    $scope.getEmpListUrl = $scope.path + 'GetEmployeeData';
    $scope.saveLockUrl = $scope.path + 'CreateLockData';
    $scope.LastLockDateUrl = $scope.path + 'GetLastLockDate';
    $scope.LockDateListUrl = $scope.path + 'GetLockDateList';
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
    //$scope.getLockDateList();

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
            if (response.data.Error === true) {
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



    $scope.DownloadOutPunchMissingDataForAlert = function () {
      
        var gridObj = $("#GridOutPunchMissingDataForAlert").ejGrid("instance");
        $scope.Print(gridObj);

    };



    $('.datepicker').datepicker({
        startDate: '-2m',
        endDate: '-0d',
        datesDisabled: $scope.DisabledDates,
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
    $scope.OutPunchMissingData = [];
    $scope.OutPunchMissingDataForAlert = [];
    $scope.OutPunchMissingDataForAlertEmployes = null;
    $scope.employees = [];
    $scope.customPara = {
        lockDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy')
    };

    $scope.OTUnConfirmedEmployeesCount = null;
    $scope.UnApprovedEmployeesCount = null;
    $scope.ShiftNotAssignEmployeesCount = null;
    $scope.AttdencenotNotProcEmployeesCount = null;
    $scope.OutPunchMissingDataCount = null;
    $scope.LastLockDate = null;
    $scope.DatePickerEnable = true;
    $scope.LoadButtonShow = false;
    $scope.LockButtonShow = false;
    $scope.IsOutMissingValidationRequired = false;
    $scope.IsOTConfirmationAuto = false;
    $scope.IsOTConfirmationAfterLock = false;
    $scope.ShowOTConfirmationTab = true;
    // #region Tab




    $scope.tab = 2;
    //$scope.setTab1 = function (newTab) {
    //    $scope.tab = newTab;


    //};
    //$scope.isSet1 = function (tabNum) {
    //    return $scope.tab === tabNum;
    //};


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
                var gridObj = $("#GridOutPunchMissingData").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container
                gridObj.clearFiltering();
                $("#GridOutPunchMissingData").children('.e-grid.e-headercell').css('height', '100px');
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



    $scope.getEmpListData = function () {

        if (baseService.isUndefinedOrNull($scope.customPara.lockDate)) {
            $scope.customPara.lockDate = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
                }
        $.ajax({
            type: "POST",
            url: $scope.getEmpListUrl,
            data:
            {
                'lockDate': $filter('dateFiltering')($scope.customPara.lockDate, 'dd-M-yyyy')
            },
            dataType: "json",
            success: function (data) {
                $scope.OutPunchMissingDataForAlert = [];
                $scope.employees = data.UnApprovedEmployees;

                $scope.OTUnConfirmedEmployees = data.OTUnConfirmedEmployees;
                $scope.UnApprovedEmployees = data.UnApprovedEmployees;

                $scope.ShiftNotAssignEmployees = data.ShiftNotAssignEmployees;
                $scope.AttdencenotNotProcEmployees = data.AttdencenotNotProcEmployees;
                $scope.OutPunchMissingData = data.OutPunchMissingData;

                $scope.IsOutMissingValidationRequired = data.IsOutMissingValidationRequired;
                $scope.IsOTConfirmationAuto = data.IsOTConfirmationAuto;
                $scope.IsOTConfirmationAfterLock = data.IsOTConfirmationAfterLock;
                $scope.OutPunchMissingDataForAlert = data.OutPunchMissingDataForAlert;

                if (data.IsOTConfirmationAuto) {
                    $scope.ShowOTConfirmationTab = false;
                }
                if (data.IsOTConfirmationAfterLock) {
                    $scope.ShowOTConfirmationTab = false;
                }
              


                if (baseService.isUndefinedOrNull(data.OTUnConfirmedEmployees)) {
                    $scope.OTUnConfirmedEmployeesCount = 0;
                } else {

                    $scope.OTUnConfirmedEmployeesCount = data.OTUnConfirmedEmployees.length;
                }

                if (baseService.isUndefinedOrNull(data.UnApprovedEmployees)) {
                    $scope.UnApprovedEmployeesCount = 0;
                } else {
                    $scope.UnApprovedEmployeesCount = data.UnApprovedEmployees.length;
                }

                if (baseService.isUndefinedOrNull(data.ShiftNotAssignEmployees)) {
                    $scope.ShiftNotAssignEmployeesCount = 0;
                } else {
                    $scope.ShiftNotAssignEmployeesCount = data.ShiftNotAssignEmployees.length;
                }

                if (baseService.isUndefinedOrNull(data.AttdencenotNotProcEmployees)) {
                    $scope.AttdencenotNotProcEmployeesCount = 0;
                } else {
                    $scope.AttdencenotNotProcEmployeesCount = data.AttdencenotNotProcEmployees.length;
                }





                if (baseService.isUndefinedOrNull(data.OutPunchMissingData)) {
                    $scope.OutPunchMissingDataCount = 0;
                } else {                    
                    $scope.OutPunchMissingDataCount = data.OutPunchMissingData.length;
                }

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
            },
             error: function (data) {
                ShowResult(data.Message, "failure");

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
    $scope.getLastLockDate();

    $scope.messageText = "";
    $scope.MissPunchEmployeeListAuto = [];
    $scope.SaveLockData = function () {

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
            if ($scope.OutPunchMissingDataCount > 0) {
                throw "Please Confirmed all Employees Attdence out time.";
                //$scope.ShowResultCustom('Please Confirmed all Employees Attdence Proc', 'failure');
            }
        

            if ($scope.OutPunchMissingDataForAlert.length > 0) {
                var eDialog = $("#dialogMessageAlert").data("ejDialog");
                eDialog.open();
            } else {
                $scope.SaveData();
            }







        } catch (e) {
            ShowResult(e, 'failure');
        }



    };


    $scope.SaveData = function () {
        if ($scope.OutPunchMissingDataForAlert.length > 0) {
            var eDialog = $("#dialogMessageAlert").data("ejDialog");
            eDialog.close();
        }
        $http({
            method: "POST",
            dataType: 'JSON',

            url: $scope.saveLockUrl,
            data:
            {
                'lockDate': $scope.customPara.lockDate
            },


        }).then(function successCallback(response) {

            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {

                if (response.data.IsOTConfirmationAutoException === false) {
                    ShowResult(response.data.Message, "success");
                    $scope.getLockDateList();
                } else {

                    $scope.MissPunchEmployeeListAuto = response.data.MissPunchEmployeeListAuto;
                    $scope.ShowResultCustom("failure");

                }

            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

        
    };

    //$scope.SaveData = function () {

    //    $.ajax({
    //        type: "POST",
    //        url: $scope.OTConfirmationSaveUrl,
    //        data:
    //        {
    //            'employeeOTInformation': $scope.employees,
    //            'ProcDate': $scope.customPara.procdate
    //        },
    //        dataType: "json",
    //        success: function (data) {

    //            if (data.Error === true) {
    //                ShowResult(data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(data.Message, 'success');
    //            }

    //        }

    //    });

   // };

    $scope.ShowResultCustom = function (type) {

        //$scope.messageText = message;
        //$scope.messageTitle = "Message";
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






}

