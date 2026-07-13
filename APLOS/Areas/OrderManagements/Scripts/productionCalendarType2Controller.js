'use strict';
productionCalendarType2Controller.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http"];
function productionCalendarType2Controller(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http) {
    $rootScope.title = "Product Calendar";
    $scope.Action = 'Save';
    $scope.baseProcess = { Id: null, UserName: null };
    $scope.modelList = [];
    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }

    $scope.path = 'OrderManagements/ProductionCalendar/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';


    $scope.EntityId = '';
    $scope.prdProcessSetList = [];
    $scope.ProcessID = '';
    $scope.getProcessData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetPlanningType2ProcessCbo?entityid=" + $scope.EntityId
        }).then(function successCallback(response) {
            $scope.prdProcessSetList = response.data;

        });

    };
    $scope.entityList = null;
    $scope.getAllEntities = function () {
        $http({
            method: 'GET',
            url: "OrderManagements/ProductionCalendar/GetPlanningType2EntityCbo"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.getAllEntities();
    $scope.HolidayType = [{ 'type': 'Week-Off', 'Id': 'W' }, { 'type': 'Holiday', 'Id': 'H' }];
    $scope.HolidayCategorycbo = [];
    $scope.HolidayCategoryId = '';
    $scope.loadHolidayCategory = function () {
        $http({
            method: 'POST',
            url: 'Employees/CompensatoryOff/HolidayCategory',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {

            }
            else {
                $scope.HolidayCategorycbo = response.data;
            }
        }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    }
    $scope.loadHolidayCategory();
    $scope.workigHours = 0;
    $scope.OTHours = 0;




    $scope.Create = function () {
        if ($scope.EntityId != '' && $scope.ProcessID != '')

            $http({
                method: 'POST',
                url: $scope.path + 'CreateType2',
                data: { entityid: $scope.EntityId, processid: $scope.ProcessID },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true)
                    ShowResult(response.data.Message, 'failure');
                else
                    $scope.navigation(null);
            }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
    }
    $scope.WeekoffAssign = function () {

        var dt = $scope.contextmenuargs.targetInfo.ProductionDate;
        $http({
            method: 'POST',
            url: $scope.path + 'WeekoffAssignType2',
            data: { entityid: $scope.EntityId, processid: $scope.ProcessID, wdate: dt },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            ShowResult(response.data.Message, 'success');
            $scope.navigation($scope.navigationargs);
        }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    }
    $scope.HolidayAssign = function () {
        var fromdate = $scope.contextmenuargs.targetInfo.ProductionDate;
        var todate = $scope.contextmenuargs.targetInfo.ProductionDate;

        if (angular.isUndefinedOrNull(fromdate) == true || angular.isUndefinedOrNull(todate) == true) {
            var date = $scope.contextmenuargs.targetInfo.startTime.getDate();
            var month = $scope.contextmenuargs.targetInfo.startTime.getMonth();
            var year = $scope.contextmenuargs.targetInfo.startTime.getFullYear();
            fromdate = new Date(year, month, date);

            date = $scope.contextmenuargs.targetInfo.endTime.getDate();
            month = $scope.contextmenuargs.targetInfo.endTime.getMonth();
            year = $scope.contextmenuargs.targetInfo.endTime.getFullYear();
            todate = new Date(year, month, date);
        }

        try {
            if (angular.isUndefinedOrNull($scope.HolidayCategoryId) == true) {
                throw 'please select holiday';
            }

            $http({
                method: 'POST',
                url: $scope.path + 'HolidayAssignType2',
                data: { 'entityid': $scope.EntityId, 'processid': $scope.ProcessID, 'holidayid': $scope.HolidayCategoryId, 'fromdate': fromdate, 'todate': todate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.navigation($scope.navigationargs);
                ShowResult(response.data.Message, 'success');
            }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            var eDialog = $("#setholiday").data("ejDialog");
            eDialog.close();
        } catch (e) {
            ShowResult(response.data.Message, 'failure', '#setholiday');
        }

    }
    $scope.WorkDayAssign = function () {
        var fromdate = $scope.contextmenuargs.targetInfo.ProductionDate;
        var todate = $scope.contextmenuargs.targetInfo.ProductionDate;

        if (angular.isUndefinedOrNull(fromdate) == true || angular.isUndefinedOrNull(todate) == true) {
            var date = $scope.contextmenuargs.targetInfo.startTime.getDate();
            var month = $scope.contextmenuargs.targetInfo.startTime.getMonth();
            var year = $scope.contextmenuargs.targetInfo.startTime.getFullYear();
            fromdate = new Date(year, month, date);

            date = $scope.contextmenuargs.targetInfo.endTime.getDate();
            month = $scope.contextmenuargs.targetInfo.endTime.getMonth();
            year = $scope.contextmenuargs.targetInfo.endTime.getFullYear();
            todate = new Date(year, month, date);
        }

        //var dt = $scope.contextmenuargs.targetInfo.ProductionDate;
        try {

            if ($scope.workigHours <= 0) {
                throw 'please provide working hour(s)';
            }
            if ($scope.OTHours < 0) {
                throw 'Negative values are not allowed for OT hours';
            }

            $http({
                method: 'POST',
                url: $scope.path + 'WorkDayAssignType2',
                data: { 'entityid': $scope.EntityId, 'processid': $scope.ProcessID, 'fromdate': fromdate, 'todate': todate, 'hours': $scope.workigHours, 'OT': $scope.OTHours },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.navigation($scope.navigationargs);
                ShowResult(response.data.Message, 'success');

            }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }


            var eDialog = $("#setworkingday").data("ejDialog");
            eDialog.close();

        } catch (e) {
            ShowResult(e.me, 'failure', '#setworkingday');
        }

    }


    $scope.selectedDayStatus = {};
    $scope.getDayStatus = function () {
        var dt = $scope.contextmenuargs.targetInfo.ProductionDate;

        $http({
            method: 'POST',
            url: $scope.path + 'getDayStatusType2',
            data: { entityid: $scope.EntityId, processid: $scope.ProcessID, wdate: dt },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.selectedDayStatus = response.data.DATA[0];

            var eDialog = $("#show").data("ejDialog");
            eDialog.open();

        }), function errorCallBack(response) {

            }
    }

    ///////////////////////////////SCHEDULE////////////////////////////////
    $scope.FromDate = null;
    $scope.ToDate = null;

    $scope.navigationargs = null;
    $scope.appointments = [];
    $scope.workweek = ["Saturday", "Sunday", "Friday", "Monday", "Tuesday", "Wednesday", "Thursday"];
    $scope.setDate = new Date();
    $scope.plancolorchange = function (args) {
        try {

            if (args.requestType == "appointment") {

                args.element.css("background", args.appointment.Color);
                args.element.css("border-color", args.appointment.Color);

            }
        } catch (e) {

        }

    }
    $scope.navigation = function (args) {

        var dt = new Date();
        if (args != null) {
            $scope.navigationargs = args;
            var date = args.currentDate.getDate();
            var month = args.currentDate.getMonth();
            var year = args.currentDate.getFullYear();

            var dt = new Date(year, month, date);
        }
        $http({
            method: 'POST',
            url: $scope.path + 'getDayStatusRangeType2',
            data: { entityid: $scope.EntityId, processid: $scope.ProcessID, wdate: dt },
            dataType: 'JSON'
        }).then(function successCallback(res) {
            try {
                for (var i = 0; i < res.data.DATA.length; i++) {
                    res.data.DATA[i].AllDay = true;
                    res.data.DATA[i].Recurrence = false;
                }
                $scope.appointments = angular.copy(res.data.DATA);


                var schObj = $("#ResourceGroupSchedule").data("ejSchedule");

                schObj.refresh(); // To refresh the Schedule control within the client side event
                schObj.refreshAppointments();
            } catch (e) {

            }


        });
    }

    $scope.contextmenuargs = null;
    $scope.menuitemclick = function (args) {
        $scope.contextmenuargs = args;
        switch (args.events.ID) {
            case 'xopen':
                $scope.getDayStatus(args)
                break;
            case 'xedit':
                var eDialog = $("#setworkingday").data("ejDialog");
                eDialog.open();
                break;
            case 'xweekoff':
                $scope.WeekoffAssign(args)
                break;
            case 'xholiday':
                var eDialog = $("#setholiday").data("ejDialog");
                eDialog.open();
                break;
            case 'yholiday':
                var eDialog = $("#setholiday").data("ejDialog");
                eDialog.open();
                break;
            case 'yedit':
                var eDialog = $("#setworkingday").data("ejDialog");
                eDialog.open();
                break;
            default:
                break;
        }
    }
    $scope.OpenSimulatedData = function () {
        try {

            $scope.SimulateVisual();

        } catch (e) {

        }
        //$scope.SimulateVisual();
    }
    $scope.clickonschedule = function (args) {
        args.cancel = true;


    }
    $scope.clickonscheduledouble = function (args) {
        args.cancel = true;


    }
    $scope.beforecontextmenuopen = function (args) {
        if ($scope.EntityId == '' || $scope.ProcessID == '') {
            args.cancel = true;
        }

    }


    $scope.OpenSchedule = function (args) {

        //$scope.GetProductionPlanningData(args.appointment.Id);
        args.cancel = true;

    }
    $scope.viewtype = ["Month"];
    $scope.contextMenu = {
        appointment: [
            { id: "xopen", text: "Show Detail" },
            { id: "xaa", text: "Assign as" },
            { id: "xedit", text: " Working Day", parentId: "xaa" },
            { id: "xweekoff", text: "Week-Off", parentId: "xaa" },
            { id: "xholiday", text: "Holiday", parentId: "xaa" }
        ],
        cells: [
            { id: "yholiday", text: "Assign as Holiday" },
            { id: "yedit", text: "Assign as Working Day" }
        ]
    }

    $scope.SimulateVisual = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetScheduleData?entityid=" + $scope.EntityId + "&processid=" + $scope.baseProcess.Id
            }).then(function successCallback(res) {

                if (res.data.DATA.length > 0) {
                    $scope.resourcedata2 = {
                        dataSource: res.data.GROUPDATA,
                        text: "text", id: "id", groupId: "groupId", color: "color"
                    };
                    for (var i = 0; i < res.data.DATA.length; i++) {
                        res.data.DATA[i].AllDay = true;
                        res.data.DATA[i].Recurrence = false;
                    }
                    $scope.workweek = res.data.WORKDAYDATA;
                    $scope.appointments = angular.copy(res.data.DATA);

                    var schObj = $("#ResourceGroupSchedule").data("ejSchedule");

                    schObj.refresh(); // To refresh the Schedule control within the client side event
                    schObj.refreshAppointments();

                }
            });
        } catch (e) {

        }
        //var schObj = $("#ResourceGroupSchedule").data("ejSchedule");
        //var appointments = schObj.getAppointments();


    }




}