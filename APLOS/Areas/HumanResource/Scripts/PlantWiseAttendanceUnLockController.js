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




    //var today = new Date();
    //var today_formatted = today.getFullYear() + '-' + (today.getMonth() + 1) + '-' + ('0' + today.getDate()).slice(-2);
    //var user_busy_days = ['2019-06-09', '2019-06-16', '2019-06-19'];
    // An array of dates




   //datesDisabled

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
                throw "Select Date...";
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
                    ShowResult($scope.customPara.lockDate + " is Un-Loked...", "success");
                    $scope.getLockDateList();
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }



    };



    //$scope.ShowResultCustom = function (message, type) {
    //    $("#dialogMessage").ejDialog("setTitle", "Success");
    //    $scope.messageText = message;
    //    $scope.messageTitle = "Message";

    //    if (type === "failure")
    //        $("#dialogMessage").ejDialog("setTitle", "Error");

    //    var eDialog = $("#dialogMessage").data("ejDialog");
    //    eDialog.open();

    //};
}

