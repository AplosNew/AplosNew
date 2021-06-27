'use strict';
SecretarialDocumentSubCategoryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function SecretarialDocumentSubCategoryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Secretarial Document SubCategory";
    $scope.Action = 'Save';
    $scope.index = -1;
    //  $scope.TaskSchedulerList = [];
    $scope.path = 'IssueTracker/SecretarialDocumentSubCategory/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    // subcategory
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.SDSubCagtegoryModelList = [];
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            //  url: $scope.getListUrl
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SDSubCagtegoryModelList = response.data;
            $scope.GetSequence();
        });
    }
    $scope.getData();


    $scope.GetSDSubCagtegory = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        IsUpdateRecurring: false,
        Level: null,

        RepeatType: "Daily",
        StartDate: new Date(),
        EndDate: new Date(),
        AfterNoOfAccurence: 1,
        EveryInterval: 1,
        RepeatByDayNumber: 1,
        RepeatbyNthWeek: 'First',
        RepeatByMonth: 'January',
        RepeatbyOfEarly: 'January',
        RepeatByWeek: 'Sunday',
        IsAfter: false,
        IsOn: false,
        IsNever: true,
        // WeeklyRepeatationBycommaSepDayName: "",
        WeeklyRepeatationBycommaSepDayName: null,
        Details: null,
        isWeekly: false,
        isYearly: false,
        isDaily: true,
        EveryWeekDay: false,

        isRepeatByDay: true,
        isRepeatByTheNthWeekForMonthly: false,

        isRepeatByTheMonth: true,
        isRepeatByTheNthWeekForYearly: false,
        OnPreviousAccomplishment: true,
        SecretarialDocumentSubCategoryId: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    //$scope.Get = function (args) {

    //    $scope.ModelNew = Object.assign({}, args.data);
    //    $scope.Action = 'Update';
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNew.IsUpdateRecurring) {
            $scope.CreateTaskScheduleMessage($scope.ModelNew);

        }
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Data.Id;
                    ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.SaveSchedule = function () {
        try {

            if ($scope.ModelNew.IsUpdateRecurring) {
                $scope.CreateTaskScheduleMessage($scope.ModelNew);

                if (baseService.isUndefinedOrNull($scope.ModelNew.Level)) {
                    throw "Level is required";
                }

                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'data': $scope.ModelNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ModelNew.Id = response.data.Data.Id;
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                        angular.element(document.querySelector('#taskScheduler')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            }
        } catch (e) {
            ShowResult(e, 'failure', 'taskScheduler')
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }


    //end subcategory

    // $scope.taskSchedule.RepeatType = 'Daily';


    $scope.AuditSchedulerStatement = {
        RepeatType: '',
        Details: ''
    }
    $scope.dayList = [
        { day: 'Sun', isChecked: false },
        { day: 'Mon', isChecked: false },
        { day: 'Tue', isChecked: false },
        { day: 'Wed', isChecked: false },
        { day: 'Thu', isChecked: false },
        { day: 'Fri', isChecked: false },
        { day: 'Sat', isChecked: false }
    ];

    $scope.EveryRepeatedFlag = $scope.ModelNew.RepeatType;

    $scope.checkRepeatedStatus = function () {

        if ($scope.ModelNew.RepeatType === 'Daily') {
            //data for dinamic control
            $scope.EveryRepeatedFlag = 'Day';
            $scope.ModelNew.isWeekly = false;
            $scope.ModelNew.isYearly = false;
            $scope.ModelNew.isDaily = true;
            $scope.ModelNew.EveryWeekDay = false;

            //$scope.taskSchedule.RepeatByDayNumber = null;
            //$scope.taskSchedule.RepeatbyNthWeek = null;
            //$scope.taskSchedule.RepeatByWeek = null;

            //$scope.taskSchedule.RepeatbyOfEarly = null;
            //$scope.taskSchedule.RepeatByMonth = null;

        }
        else if ($scope.ModelNew.RepeatType === 'Weekly') {
            $scope.EveryRepeatedFlag = 'Week';
            $scope.ModelNew.isWeekly = true;
            $scope.ModelNew.isYearly = false;
            $scope.ModelNew.isDaily = false;
            $scope.ModelNew.EveryWeekDay = false;

            //$scope.taskSchedule.RepeatByDayNumber = null;
            //$scope.taskSchedule.RepeatbyNthWeek = null;
            //$scope.taskSchedule.RepeatByWeek = null;

            //$scope.taskSchedule.RepeatbyOfEarly = null;
            //$scope.taskSchedule.RepeatByMonth = null;

        }
        else if ($scope.ModelNew.RepeatType === 'Monthly') {
            $scope.EveryRepeatedFlag = 'Month';
            $scope.ModelNew.isYearly = false;
            $scope.ModelNew.isWeekly = false;
            $scope.ModelNew.isDaily = false;
            $scope.ModelNew.EveryWeekDay = false;
            //$scope.taskSchedule.RepeatbyOfEarly = null;
            //$scope.taskSchedule.RepeatByMonth = null;

        }
        else if ($scope.ModelNew.RepeatType === 'Yearly') {
            $scope.EveryRepeatedFlag = 'Year';
            $scope.ModelNew.isWeekly = false;
            $scope.ModelNew.isYearly = true;
            $scope.ModelNew.isDaily = false;
            $scope.ModelNew.EveryWeekDay = false;

        }
        else if ($scope.ModelNew.RepeatType === 'Every') {
            $scope.ModelNew.isWeekly = false;
            $scope.ModelNew.isYearly = false;
            $scope.ModelNew.isDaily = true;
            $scope.ModelNew.EveryWeekDay = true;

            //$scope.taskSchedule.EveryInterval = null;
            //$scope.taskSchedule.RepeatByDayNumber = null;
            //$scope.taskSchedule.RepeatbyNthWeek = null;
            //$scope.taskSchedule.RepeatByWeek = null;

            //$scope.taskSchedule.RepeatbyOfEarly = null;
            //$scope.taskSchedule.RepeatByMonth = null;

        }
    }


    $scope.checkUpdateFrequencyTypeAndDay = function () {

        try {
            if ($scope.ModelNew.IsUpdateRecurring) {

                if (baseService.isUndefinedOrNull($scope.ModelNew.Level)) {
                    throw "Level is required";
                }

                if ($scope.ModelNew.RepeatType === null) {
                    $scope.ClearReccuringData();
                }

                $scope.showTaskSchedulerPopUp();
            }
        } catch (e) {
            ShowResult(e, 'failure')
        }

    }
    $scope.showTaskSchedulerPopUp = function () {
        angular.element(document.querySelector('#taskScheduler')).modal('show');
    }


    $scope.ClearShedule = function () {
        $scope.ModelNew.RepeatType = null;
        $scope.ModelNew.StartDate = null;
        $scope.ModelNew.EndDate = null;
        $scope.ModelNew.AfterNoOfAccurence= 0;
        $scope.ModelNew.EveryInterval= 0;
        $scope.ModelNew.RepeatByDayNumber= 0;
        $scope.ModelNew.RepeatbyNthWeek = null;
        $scope.ModelNew.RepeatByMonth = null;
        $scope.ModelNew.RepeatbyOfEarly = null;
        $scope.ModelNew.RepeatByWeek = null;
        $scope.ModelNew.IsAfter= false;
        $scope.ModelNew.IsOn= false;
        $scope.ModelNew.IsNever= true;
        // WeeklyRepeatationBycommaSepDayName: "";
        $scope.ModelNew.WeeklyRepeatationBycommaSepDayName= null;
        $scope.ModelNew.Details= null;
        $scope.ModelNew.isWeekly= false;
        $scope.ModelNew.isYearly= false;
        $scope.ModelNew.isDaily = false;
        $scope.ModelNew.EveryWeekDay= false;

        $scope.ModelNew.isRepeatByDay = false;
        $scope.ModelNew.isRepeatByTheNthWeekForMonthly= false;

        $scope.ModelNew.isRepeatByTheMonth = false;
        $scope.ModelNew.isRepeatByTheNthWeekForYearly= false;
        $scope.ModelNew.OnPreviousAccomplishment = false;
        $scope.ModelNew.SecretarialDocumentSubCategoryId = null;
    }

    $scope.ClearReccuringData = function () {
        $scope.dayList = [
            { day: 'Sun', isChecked: false },
            { day: 'Mon', isChecked: false },
            { day: 'Tue', isChecked: false },
            { day: 'Wed', isChecked: false },
            { day: 'Thu', isChecked: false },
            { day: 'Fri', isChecked: false },
            { day: 'Sat', isChecked: false }
        ];
        $scope.ModelNew.RepeatType = "Daily";
        $scope.ModelNew.isDaily = true;
        $scope.ModelNew.isWeekly = false;
        $scope.ModelNew.isYearly = false;
        $scope.ModelNew.EveryWeekDay = false;

        $scope.ModelNew.IsNever = true;
        $scope.ModelNew.IsAfter = false;
        $scope.ModelNew.IsOn = false;

        $scope.ModelNew.StartDate = new Date();
        $scope.ModelNew.EndDate = new Date();
        $scope.ModelNew.AfterNoOfAccurence = 1;
        $scope.ModelNew.EveryInterval = 1;
        $scope.ModelNew.RepeatByDayNumber = 1;
        $scope.ModelNew.RepeatByMonth = 'January';
        $scope.ModelNew.RepeatbyOfEarly = 'January';
        $scope.ModelNew.RepeatbyNthWeek = 'First';
        $scope.ModelNew.RepeatByWeek = 'Sunday';


        $scope.ModelNew.isRepeatByDay = true;
        $scope.ModelNew.isRepeatByTheNthWeekForMonthly = false;

        $scope.ModelNew.isRepeatByTheNthWeekForYearly = false;
        $scope.ModelNew.isRepeatByTheMonth = true;
        $scope.ModelNew.OnPreviousAccomplishment = true;

    }



    $scope.CreateTaskScheduleMessage = function (Schedule) {
        if ($scope.ModelNew.RepeatType === 'Daily') {

            $scope.ModelNew.Details = '';

            $scope.ModelNew.Details += 'Repeate ' + Schedule.RepeatType;
            $scope.ModelNew.Details += ' Every ' + Schedule.EveryInterval + ' Day(s) starting from ' + $filter("dateFiltering")(Schedule.StartDate);
            if (Schedule.IsNever == true) {
                $scope.ModelNew.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.ModelNew.Details += ' and End After ' + Schedule.AfterNoOfAccurence + ' occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.ModelNew.Details += 'and End On ' + Schedule.EndDate;
            }
        }
        else if (Schedule.RepeatType === 'Weekly') {


            $scope.ModelNew.Details = '';

            $scope.ModelNew.Details += 'Repeate ' + Schedule.RepeatType;
            $scope.ModelNew.Details += ' Every ' + Schedule.EveryInterval + ' Week(s) starting from ' + $filter("dateFiltering")(Schedule.StartDate);
            if (Schedule.IsNever == true) {
                $scope.ModelNew.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.ModelNew.Details += ' and End After ' + Schedule.AfterNoOfAccurence + ' occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.ModelNew.Details += 'and End On ' + Schedule.EndDate;
            }
        }
        else if (Schedule.RepeatType === 'Monthly') {

            $scope.ModelNew.Details = '';

            $scope.ModelNew.Details += 'Repeate ' + Schedule.RepeatType;
            $scope.ModelNew.Details += ' Every ' + Schedule.EveryInterval + ' Month(s) starting from ' + $filter("dateFiltering")(Schedule.StartDate);

            if (Schedule.IsNever == true) {
                $scope.ModelNew.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.ModelNew.Details += ' and End After ' + Schedule.AfterNoOfAccurence + 'occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.ModelNew.Details += 'and End On ' + $filter("dateFiltering")(Schedule.EndDate);
            }

            if ($scope.ModelNew.isRepeatByDay == true) {
                $scope.ModelNew.Details += 'Repeat On ' + Schedule.RepeatByDayNumber + ' day(s) of the month';
            }
            else if ($scope.ModelNew.isRepeatByTheNthWeekForMonthly == true) {
                $scope.ModelNew.Details += 'Repeat On ' + Schedule.RepeatbyNthWeek + ' ' + Schedule.RepeatByWeek + ' of the month';
            }
        }
        else if ($scope.ModelNew.RepeatType === 'Yearly') {
            //Schedule
            $scope.ModelNew.Details = '';

            $scope.ModelNew.Details += 'Repeate ' + $scope.ModelNew.RepeatType;
            $scope.ModelNew.Details += ' Every ' + $scope.ModelNew.EveryInterval + ' Year(s) starting from ' + $filter("dateFiltering")(Schedule.StartDate);

            if (Schedule.IsNever == true) {
                $scope.ModelNew.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.ModelNew.Details += ' and End After ' + Schedule.AfterNoOfAccurence + ' occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.ModelNew.Details += 'and End On ' + $filter("dateFiltering")(Schedule.EndDate);
            }

            if ($scope.ModelNew.isRepeatByTheMonth == true) {
                $scope.ModelNew.Details += ' Repeat On ' + Schedule.RepeatByDayNumber + ' Day(s) of ' + Schedule.RepeatByMonth;
            }
            else if ($scope.ModelNew.isRepeatByTheNthWeekForYearly == true) {
                $scope.ModelNew.Details += ' Repeat On ' + Schedule.RepeatbyNthWeek + ' ' + Schedule.RepeatByWeek + ' of ' + Schedule.RepeatbyOfEarly;
            }

        }
        else if (Schedule.RepeatType === 'Every') {

            $scope.ModelNew.Details = '';

            $scope.ModelNew.Details += 'Repeate ' + Schedule.RepeatType + ' Week Day';
            $scope.ModelNew.Details += 'Week Days starting from ' + $filter("dateFiltering")(Schedule.StartDate);
            if (Schedule.IsNever == true) {
                $scope.ModelNew.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.ModelNew.Details += ' and End After ' + Schedule.AfterNoOfAccurence + ' occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.ModelNew.Details += 'and End On ' + $filter("dateFiltering")(Schedule.EndDate);
            }
        }


    }

    $scope.hideSchedulerPopUp = function () {
        angular.element(document.querySelector('#taskScheduler')).modal('hide');
    }


    //GrideView SecretarialDocument SubCategory List getmasterData
    $scope.ModelSDSubCategoryList = [];
    $scope.getSDSubCategoryData = function () {
        $http.get("IssueTracker/SecretarialDocumentSubCategory/GetList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ModelSDSubCategoryList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getSDSubCategoryData();

    //$scope.Get = function (obj) {
    //    $scope.ModelNew = obj.data;
    //    $scope.issueTransactionNew.IsUpdateRecurring = true;
    //    $scope.checkUpdateFrequencyTypeAndDay();
    //};

}


