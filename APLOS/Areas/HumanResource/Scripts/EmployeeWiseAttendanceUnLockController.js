'use strict';
PlantWiseAttendanceUnLockController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function PlantWiseAttendanceUnLockController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = ' UnLock';
    $scope.Action = 'Save';
    $scope.path = 'humanresource/HrmsSettings/';
    
    $scope.saveUnLockUrl = $scope.path + 'CreateUnLockData';
    
    $scope.UnLockDateListUrl = $scope.path + 'GetUnLockDateList';
    $scope.DisabledDates = [];


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
















    $('.datepicker').datepicker({
        startDate: '-2m',
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


    
    $scope.employees = [];
    $scope.LockEmpList = [];
    $scope.TobeLockEmpList = [];

    $scope.customPara = {
        lockDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy')
    };

   
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
    





    $scope.customPara = {
        lockDate: null
    };

   
    $scope.LastLockDate = null;
    $scope.DatePickerEnable = true;
    $scope.LoadButtonShow = false;
    $scope.LockButtonShow = false;


    




    $scope.messageText = "";

    $scope.SaveUnLockData = function () {

       

        try {
            if(baseService.isUndefinedOrNull($scope.customPara.lockDate)) {
                $scope.ShowResultCustom("Select Date...", 'failure');
            }

            $http({
                method: 'POST',
                url: $scope.saveUnLockUrl,
                data:
                {
                    'lockDate': $scope.customPara.lockDate
                },
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    $scope.ShowResultCustom($scope.customPara.lockDate + " is Un-Loked...", "success");
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };





            //$.ajax({
            //    type: "POST",
            //    url: $scope.saveUnLockUrl,
            //    data:
            //    {
            //        'lockDate': $scope.customPara.lockDate
            //    },
            //    dataType: "json",
            //    success: function (data) {
            //        $scope.ShowResultCustom($scope.customPara.lockDate+" is Un-Loked...", "success");

            //    }

            //});
        } catch (e) {
            $scope.ShowResultCustom(e.Message, 'failure');
        }

        

    };

    

    $scope.ShowResultCustom = function (message, type) {
        $("#dialogMessage").ejDialog("setTitle", "Success");
        $scope.messageText = message;
        $scope.messageTitle = "Message";

        if (type === "failure")
            $("#dialogMessage").ejDialog("setTitle", "Error");

        var eDialog = $("#dialogMessage").data("ejDialog");
        eDialog.open();

    };











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



    //$scope.actionCompleteSelected4 = function (args) {
    //    try {
    //        if (args.requestType === "refresh") {
    //            var gridObj = $("#GridEmpWise").ejGrid("instance");
    //            var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

    //            //args.requestType: "filtering"
    //            //var filtereddata = gridObj.getFilteredRecords();
    //            //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
    //            gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
    //            gridObj.windowonresize();
    //        }
    //    } catch (e) {
    //        // $scope.ShowResult(e, 'failure');
    //    }
    //};
    //function checkChangeemployee4(e) {

    //    var val = e.model.value;
    //    //item level check
    //    var row = $filter('filter')($scope.EmployeeLockData, { 'WorkDate': e.model.value });
    //    if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
    //        if (e.model.checkState === "check")
    //            if (row[0].IsLock === false)
    //                row[0].CheckBoxSelect = true;
    //            else
    //                row[0].CheckBoxSelect = false;
    //    }

    //}
    //function headCheckChangeemployee4(e) {
    //    if (e.model.checkState === "check") {

          
    //        var filtered = $("#GridEmpWise").data("ejGrid").getFilteredRecords();
    //        if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
    //            for (var i = 0; i < $scope.EmployeeLockData.length; i++) {
    //                $scope.EmployeeLockData[i].CheckBoxSelect = false;
    //                if ($scope.EmployeeLockData[i].IsLock === false)
    //                    $scope.EmployeeLockData[i].CheckBoxSelect = true;
    //            }
    //        }
    //        else {
    //            for (var i = 0; i < $scope.EmployeeLockData.length; i++) {
    //                for (var j = 0; j < filtered.length; j++) {
    //                    $scope.EmployeeLockData[i].CheckBoxSelect = false;
    //                    if ($scope.EmployeeLockData[i].WorkDate === filtered[j].WorkDate && $scope.EmployeeLockData[i].IsLock === false)
    //                        $scope.EmployeeLockData[i].CheckBoxSelect = true;
    //                }

    //            }
    //        }

         
    //    }
    //    else {




    //        for (var i = 0; i < $scope.EmployeeLockData.length; i++) {
    //            $scope.EmployeeLockData[i].CheckBoxSelect = false;
    //        }


    //    }
    //    var _gridObj = $("#GridEmpWise").data("ejGrid");
    //    _gridObj.refreshContent(true);
    //    //header level check
    //}
    //$scope.dataBoundemployee4 = function (args) {
    //    $("#GridEmpWise .rowCheckbox").ejCheckBox({ "change": checkChange });
    //    $("#headchk4").ejCheckBox({ "change": headCheckChangeemployee4 });
    
    //}
    //$scope.refreshTemplateemployee4 = function (args) {
    //    //if (args.rowIndex == 0) {
    //    //    $("#headchk4").ejCheckBox({ "change": headCheckChangeemployee4 });
    //    //}
    //    $("#headchk4").ejCheckBox({ "change": headCheckChangeemployee4 });        
    //}








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




    $scope.refreshTemplateemployee5 = function (args) {
        $("#headchk5").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise5 });
    };

    function CheckBoxSelectAllEmolyeeWise5(e) {
     


        if (e.model.checkState === "check") {

            for (var i = 0; i < $scope.EmployeeReLockData.length; i++) {              
                    $scope.EmployeeReLockData[i].CheckBoxSelect = true;
            }
        }
        else {
           
            for (var i = 0; i < $scope.EmployeeReLockData.length; i++) {
                $scope.EmployeeReLockData[i].CheckBoxSelect = false;
            }
        }
        var gridObj = $("#GridEmpWiseReLock").data("ejGrid");
        gridObj.refreshContent();
    };
    //#endregion

























}

function xPlantWiseAttendanceUnLockController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = ' UnLock';
    $scope.Action = 'Save';
    $scope.path = 'humanresource/HrmsSettings/';

    $scope.saveUnLockUrl = $scope.path + 'CreateUnLockData';

    $scope.UnLockDateListUrl = $scope.path + 'GetUnLockDateList';
    $scope.DisabledDates = [];


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
        startDate: '-2m',
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
    $scope.customPara = {
        lockDate: null
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

    $scope.SaveUnLockData = function () {



        try {
            if (baseService.isUndefinedOrNull($scope.customPara.lockDate)) {
                $scope.ShowResultCustom("Select Date...", 'failure');
            }



            $.ajax({
                type: "POST",
                url: $scope.saveUnLockUrl,
                data:
                {
                    'lockDate': $scope.customPara.lockDate
                },
                dataType: "json",
                success: function (data) {
                    $scope.ShowResultCustom($scope.customPara.lockDate + " is Un-Loked...", "success");

                }

            });
        } catch (e) {
            $scope.ShowResultCustom(e.Message, 'failure');
        }



    };



    $scope.ShowResultCustom = function (message, type) {
        $("#dialogMessage").ejDialog("setTitle", "Success");
        $scope.messageText = message;
        $scope.messageTitle = "Message";

        if (type === "failure")
            $("#dialogMessage").ejDialog("setTitle", "Error");

        var eDialog = $("#dialogMessage").data("ejDialog");
        eDialog.open();

    };
}