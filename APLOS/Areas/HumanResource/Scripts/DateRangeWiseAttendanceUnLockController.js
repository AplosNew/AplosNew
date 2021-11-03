'use strict';
DateRangeWiseAttendanceUnLockController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function DateRangeWiseAttendanceUnLockController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = ' UnLock';
    $scope.Action = 'Save';
    $scope.path = 'humanresource/HrmsSettings/';

    $scope.saveUnLockUrl = $scope.path + 'CreateUnLockData';
    $scope.saveUnLockEmployeeListUrl = $scope.path + 'CreateUnLockDataEmployeeWise';
    $scope.saveReLockEmployeeListUrl = $scope.path + 'CreateReLockDataEmployeeWise';

    $scope.UnLockDateListUrl = $scope.path + 'GetUnLockDateList';
    $scope.GetLockEmployeeListUrl = $scope.path + 'GetLockEmployeeList';
    $scope.GetReLockEmployeeListUrl = $scope.path + 'GetReLockEmployeeList';
    $scope.GetOutPunchMissingDataForAlertUrl = $scope.path + 'GetOutPunchMissingDataForAlert';

    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';



    $scope.DisabledDates = [];
    $scope.OutPunchMissingDataForAlert = [];

    $scope.getLockDateList = function () {

        $.ajax({
            type: "GET",
            url: $scope.UnLockDateListUrl,

            dataType: "json",
            success: function (data) {

                $scope.DisabledDates = data.LastLockDate;

            }

        });

    };
    $scope.getLockDateList();




    //var today = new Date();
    //var today_formatted = today.getFullYear() + '-' + (today.getMonth() + 1) + '-' + ('0' + today.getDate()).slice(-2);
    //var user_busy_days = ['2019-06-09', '2019-06-16', '2019-06-19'];
    // An array of dates






    $('.datepicker').datepicker({
        startDate: '-36m',
        endDate: '-0d',
        datesDisabled: $scope.DisabledDates,
        format: 'dd-M-yyyy',
        todayHighlight: true,
        //minDate: 0,
        autoclose: true,
        inline: true,
        changeMonth: true,
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

   



    $scope.OTUnConfirmedEmployees = [];
    $scope.UnApprovedEmployees = [];
    $scope.ShiftNotAssignEmployees = [];
    $scope.AttdencenotNotProcEmployees = [];
    $scope.employees = [];
    $scope.customPara = {
        FromDate: null,
        ToDate: null
    };





    $scope.OTUnConfirmedEmployeesCount = null;
    $scope.UnApprovedEmployeesCount = null;
    $scope.ShiftNotAssignEmployeesCount = null;
    $scope.AttdencenotNotProcEmployeesCount = null;
    $scope.LastLockDate = null;
    $scope.DatePickerEnable = true;
    $scope.LoadButtonShow = false;
    $scope.LockButtonShow = false;







    $scope.messageText = "";

    //$scope.SaveUnLockData = function () {



    //    try {
    //        if (baseService.isUndefinedOrNull($scope.customPara.lockDate)) {
    //            $scope.ShowResultCustom("Select Date...", 'failure');
    //        }



    //        $.ajax({
    //            type: "POST",
    //            url: $scope.saveUnLockUrl,
    //            data:
    //            {
    //                'lockDate': $scope.customPara.lockDate
    //            },
    //            dataType: "json",
    //            success: function (data) {
    //                $scope.ShowResultCustom($scope.customPara.lockDate + " is Un-Loked...", "success");

    //            }

    //        });
    //    } catch (e) {
    //        $scope.ShowResultCustom(e.Message, 'failure');
    //    }



    //};





    $scope.employees = [];
    $scope.LockEmpList = [];
    $scope.TobeLockEmpList = [];

    //$scope.customPara = {
    //    lockDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy')
    //};


    $scope.LockEmpListCount = null;

    $scope.LastLockDate = null;
    $scope.DatePickerEnable = true;



    //#region Tab




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


    // #endregion Tab
    $scope.actionCompleteSelected7 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridLockEmployeeList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container
                gridObj.clearFiltering();
                $("#GridLockEmployeeList").children('.e-grid.e-headercell').css('height', '100px');
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






    //$scope.customPara = {
    //    lockDate: null
    //};


    $scope.LastLockDate = null;
    $scope.DatePickerEnable = true;
    $scope.LoadButtonShow = false;
    $scope.LockButtonShow = false;







    $scope.messageText = "";

    $scope.SaveUnLockData = function () {



        try {


            if (baseService.isUndefinedOrNull($scope.customPara.FromDate)) {
                throw "Please Enter From Date.";
            }
            if (baseService.isUndefinedOrNull($scope.customPara.ToDate)) {
                throw "Please Enter To Date.";
            }





            $.ajax({
                type: "POST",
                url: $scope.saveUnLockUrl,
                data:
                {
                    'FromDate': $scope.customPara.FromDate,
                    'ToDate': $scope.customPara.ToDate
                },
                dataType: "json",
                success: function (data) {
                    ShowResult($scope.customPara.ToDate + 'to'+$scope.customPara.ToDate + " is Un-Loked...", "success");

                }

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }



    };















    //#region Employee wise

    $scope.EmployeeLockData = [];
    $scope.EmployeeReLockData = [];
    $scope.getUnLockDateList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.customPara.FromDate)) {
                throw "Please Enter From Date.";
            }
            if (baseService.isUndefinedOrNull($scope.customPara.ToDate)) {
                throw "Please Enter To Date.";
            }


            $http({
                method: "GET",
                dataType: 'JSON',               
                url: $scope.GetLockEmployeeListUrl + '?FromDate=' + $scope.customPara.FromDate + '&ToDate=' + $scope.customPara.ToDate

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                }
                else {
                    $scope.EmployeeReLockData = [];
                    $scope.EmployeeLockData = [];
                    $scope.EmployeeLockData = response.data.LockEmployees;
                    $scope.EmployeeReLockData = response.data.ReLockEmployees;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.getReLockDateList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.customPara.FromDate)) {
                throw "Please Enter From Date.";
            }
            if (baseService.isUndefinedOrNull($scope.customPara.ToDate)) {
                throw "Please Enter To Date.";
            }

            $http({
                method: "GET",
                dataType: 'JSON',
                //data: { 'lockDate': $scope.customPara.lockDate},
                url: $scope.GetReLockEmployeeListUrl +  '?FromDate=' + $scope.customPara.FromDate + '&ToDate=' + $scope.customPara.ToDate

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                }
                else {
                    $scope.EmployeeReLockData = [];
                    $scope.EmployeeLockData = [];
                    $scope.EmployeeLockData = response.data.LockEmployees;
                    $scope.EmployeeReLockData = response.data.ReLockEmployees;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



    $scope.SaveEmployeeWiseUnLockData = function () {

        try {
            var UnLockEmployeeList = [];
            for (var i = 0; i < $scope.EmployeeLockData.length; i++) {

                if ($scope.EmployeeLockData[i].CheckBoxSelect === true) {
                    UnLockEmployeeList.push($scope.EmployeeLockData[i].SystemID);
                }

            }
            if (UnLockEmployeeList.length == 0) {
                throw "Please Select Employee.";
            }

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'FromDate': $scope.customPara.FromDate, 'ToDate': $scope.customPara.ToDate, 'UnLockEmployeeList': UnLockEmployeeList },
                url: $scope.saveUnLockEmployeeListUrl

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //$scope.EmployeeLockData = response.data.Employees;

                    ShowResult(response.data.Message, "success");

                    //var gridObj = $("#GridEmpWise").data("ejGrid");
                    //gridObj.refreshContent();
                    $scope.EmployeeLockData = [];
                    $scope.EmployeeReLockData = [];
                    $scope.getUnLockDateList();
                    $scope.getReLockDateList();
                }
            }, function errorCallback(response) {
                ShowResult(response.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }


    };
    $scope.SaveEmployeeWiseReLockData = function () {

        try {
            var ReLockEmployeeList = [];
            for (var i = 0; i < $scope.EmployeeReLockData.length; i++) {

                if ($scope.EmployeeReLockData[i].CheckBoxSelect === true) {
                    ReLockEmployeeList.push($scope.EmployeeReLockData[i].SystemID);
                }

            }
            if (ReLockEmployeeList.length == 0) {
                throw "Please Select Employee.";
            }



            $scope.OutPunchMissingDataForAlert = [];

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'FromDate': $scope.customPara.FromDate, 'ToDate': $scope.customPara.ToDate, 'ReLockEmployeeList': ReLockEmployeeList },
                url: $scope.GetOutPunchMissingDataForAlertUrl

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.OutPunchMissingDataForAlert = response.data;

                    if ($scope.OutPunchMissingDataForAlert.length > 0) {
                        var eDialog = $("#dialogMessageAlert").data("ejDialog");
                        eDialog.open();
                    } else {
                        $scope.SaveData();
                    }
                    
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });






          




        } catch (e) {
            ShowResult(e, 'failure');
        }


    };

    $scope.SaveData = function () {
        if ($scope.OutPunchMissingDataForAlert.length > 0) {
            var eDialog = $("#dialogMessageAlert").data("ejDialog");
            eDialog.close();
        }
        var ReLockEmployeeList = [];
        for (var i = 0; i < $scope.EmployeeReLockData.length; i++) {

            if ($scope.EmployeeReLockData[i].CheckBoxSelect === true) {
                ReLockEmployeeList.push($scope.EmployeeReLockData[i].SystemID);
            }

        }
        if (ReLockEmployeeList.length == 0) {
            throw "Please Select Employee.";
        }
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'FromDate': $scope.customPara.FromDate, 'ToDate': $scope.customPara.ToDate, 'ReLockEmployeeList': ReLockEmployeeList },
            url: $scope.saveReLockEmployeeListUrl

        }).then(function successCallback(response) {

            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //$scope.EmployeeLockData = response.data.Employees;

                ShowResult(response.data.Message, "success");
                //$scope.EmployeeLockData = [];
                //var gridObj = $("#GridEmpWise").data("ejGrid");
                //gridObj.refreshContent();
                $scope.EmployeeLockData = [];
                $scope.EmployeeReLockData = [];
                $scope.getUnLockDateList();
                $scope.getReLockDateList();
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
        

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

    $scope.actionCompleteSelected5 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridEmpWiseReLock").ejGrid("instance");
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







    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {



        //if (e.model.checkState === "check") {

        //    for (var i = 0; i < $scope.EmployeeLockData.length; i++) {
        //        //$scope.EmployeeLockData[i].CheckBoxSelect = false;
        //        //if ($scope.EmployeeLockData[i].IsLock === false)
        //        $scope.EmployeeLockData[i].CheckBoxSelect = true;
        //    }
        //}
        //else {

        //    for (var i = 0; i < $scope.EmployeeLockData.length; i++) {
        //        $scope.EmployeeLockData[i].CheckBoxSelect = false;
        //    }
        //}
        //var gridObj = $("#GridEmpWise").data("ejGrid");
        //gridObj.refreshContent();


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridEmpWise").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeLockData.length; i++) {
                $scope.EmployeeLockData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridEmpWise").data("ejGrid");
        gridObj.refreshContent();



    };




    $scope.refreshTemplateemployee5 = function (args) {
        $("#headchk5").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise5 });
    };

    function CheckBoxSelectAllEmolyeeWise5(e) {



        //if (e.model.checkState === "check") {

        //    for (var i = 0; i < $scope.EmployeeReLockData.length; i++) {
           
        //        $scope.EmployeeReLockData[i].CheckBoxSelect = true;
        //    }
        //}
        //else {

        //    for (var i = 0; i < $scope.EmployeeReLockData.length; i++) {
        //        $scope.EmployeeReLockData[i].CheckBoxSelect = false;
        //    }
        //}
        //var gridObj = $("#GridEmpWiseReLock").data("ejGrid");
        //gridObj.refreshContent();



        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridEmpWiseReLock").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeReLockData.length; i++) {
                $scope.EmployeeReLockData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridEmpWiseReLock").data("ejGrid");
        gridObj.refreshContent();

    };
    //#endregion





}

