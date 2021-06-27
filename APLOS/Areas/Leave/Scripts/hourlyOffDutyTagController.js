'use strict';
hourlyOffDutyTagController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function hourlyOffDutyTagController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Off Duty Hours Tag';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.maternityLeaveTransactions = [];
    $scope.path = 'Leave/HourlyOffDutyTag/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.WorkDate = new Date();
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

    $scope.setTab33 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet33 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab44 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet44 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab55 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet55 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab66 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet66 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab66 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet66 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    // #endregion Tab
    
    $scope.AttendanceInfoExtra = [];
    $scope.LUNCHOUTisnullisnotnull = [];
    $scope.LATEIN = [];
    $scope.WithOutIn = [];
    $scope.EARLYOUT = [];
    $scope.LUNCHOUTisnullisnull = [];
    $scope.LUNCHOUTisnotnullisnotnull = [];
    $scope.LUNCHOUTisnotnullisnull = [];
    $scope.GetAttendanceInfoExtra = function (workdate) {

        $http({
            method: 'GET',
            url: $scope.path + '/GetAttendanceInfoExtra?workdate=' + workdate
        }).then(function successCallback(response) {
           $scope.AttendanceInfoExtra = [];
            $scope.AttendanceInfoExtra = response.data;

            $scope.LUNCHOUTisnullisnotnull = [];
            $scope.LATEIN = [];
            $scope.WithOutIn = [];
            $scope.EARLYOUT = [];
            $scope.LUNCHOUTisnullisnull = [];
            $scope.LUNCHOUTisnotnullisnotnull = [];
            $scope.LUNCHOUTisnotnullisnull = [];

            for (var i = 0; i < $scope.AttendanceInfoExtra.length; i++) {
                if ($scope.AttendanceInfoExtra[i].InfoType == 'LUNCHOUT' && $scope.AttendanceInfoExtra[i].LunchInTime == null && $scope.AttendanceInfoExtra[i].LunchOutTime != null) {
                    $scope.LUNCHOUTisnullisnotnull.push($scope.AttendanceInfoExtra[i]);
                }
                if ($scope.AttendanceInfoExtra[i].InfoType == 'LATEIN') {
                    $scope.LATEIN.push($scope.AttendanceInfoExtra[i]);
                }
                if ($scope.AttendanceInfoExtra[i].InfoType == 'LUNCHOUT_OM') {
                    $scope.WithOutIn.push($scope.AttendanceInfoExtra[i]);
                }
                if ($scope.AttendanceInfoExtra[i].InfoType == 'EARLYOUT') {
                    $scope.EARLYOUT.push($scope.AttendanceInfoExtra[i]);
                }
                if ($scope.AttendanceInfoExtra[i].InfoType == 'LUNCHOUT' && $scope.AttendanceInfoExtra[i].LunchInTime == null && $scope.AttendanceInfoExtra[i].LunchOutTime == null) {
                    $scope.LUNCHOUTisnullisnull.push($scope.AttendanceInfoExtra[i]);
                }
                if ($scope.AttendanceInfoExtra[i].InfoType == 'LUNCHOUT' && $scope.AttendanceInfoExtra[i].LunchInTime != null && $scope.AttendanceInfoExtra[i].LunchOutTime != null) {
                    $scope.LUNCHOUTisnotnullisnotnull.push($scope.AttendanceInfoExtra[i]);
                }
                if ($scope.AttendanceInfoExtra[i].InfoType == 'LUNCHOUT' && $scope.AttendanceInfoExtra[i].LunchInTime != null && $scope.AttendanceInfoExtra[i].LunchOutTime == null) {
                    $scope.LUNCHOUTisnotnullisnull.push($scope.AttendanceInfoExtra[i]);
                }
            }
        });
    };



    //$scope.recorddoubleclick = function () {
    //    var gridObj = $("#Grid").data("ejGrid");
    //    $scope.OffDutyHoursModel = gridObj.getSelectedRecords()[0];
    //    $scope.GetPreData($scope.employeeInfo.EmpSystemID);
    //    $scope.Action = 'Update';
    //};

    $scope.GetHourlyOffDutyTag = function () {
        var ReportFormat = 'Excel';
        location.href = 'Leave/HourlyOffDutyTag/GetHourlyOffDutyTag?reportFormat=' + ReportFormat + '&WorkDate=' + $scope.WorkDate;
    };



    $scope.HourlyLeaveReasonIdList = [];
    $scope.GetCbo = function () {
        $http.get('Leave/HourlyOffDutyTag/GetCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.HourlyLeaveReasonIdList = [];
                        $scope.HourlyLeaveReasonIdList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetCbo();
    $scope.ApproveModel = {
        ApproveType: null,
        LeaveTypeId: null,
        Duration: null,
        OrginalDuration: null,
        HourlyLeaveReasonId: null
    };
    $scope.ApproveTypeList = [{ Text: '--Select--' }, { Text: 'Waive' }, { Text: 'Leave' }, { Text: 'Deducation' }]


    $scope.LeaveTypeList = [];
    $scope.ApproveTypeChange = function () {
        try {

            $scope.LeaveTypeList = [];
            if ($scope.ApproveModel.ApproveType == 'Leave') {
                $http({
                    method: 'GET',
                    url: $scope.path + '/GetLeaveTypeInfo?EmpsystemId=' + $scope.ApproveInfoModel.SystemId
                }).then(function successCallback(response) {
                    $scope.LeaveTypeList = [];
                    $scope.LeaveTypeList = response.data;

                   
                });
            }
            else {
                $scope.LeaveTypeList = [];
            }


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
   


    $scope.ApproveInfoModel = {};



    $scope.OpendialogActionLUNCHOUT = function (arg) {
        try {


            var eDialog = $("#dialogAction").data("ejDialog");
            eDialog.open();
            var gridObj = $("#Grid").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];
            //$("#dialogDesignation").ejDialog("setTitle", modeldata.UserName);

            $scope.ApproveInfoModel = {};
            $scope.ApproveInfoModel = modeldata;
            $scope.ApproveModel.Duration = modeldata.Duration;
            $scope.ApproveModel.OrginalDuration = modeldata.OrginalDuration;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.OpendialogActionWithoutIn = function (arg) {
        try {


            var eDialog = $("#dialogAction").data("ejDialog");
            eDialog.open();
            var gridObj = $("#Grid6").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];
            //$("#dialogDesignation").ejDialog("setTitle", modeldata.UserName);

            $scope.ApproveInfoModel = {};
            $scope.ApproveInfoModel = modeldata;
            $scope.ApproveModel.Duration = modeldata.Duration;
            $scope.ApproveModel.OrginalDuration = modeldata.OrginalDuration;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.OpendialogActionLATEIN = function (arg) {
        try {


            var eDialog = $("#dialogAction").data("ejDialog");
            eDialog.open();
            var gridObj = $("#Grid1").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];
            //$("#dialogDesignation").ejDialog("setTitle", modeldata.UserName);

            $scope.ApproveInfoModel = {};
            $scope.ApproveInfoModel = modeldata;
            $scope.ApproveModel.Duration = modeldata.Duration;
            $scope.ApproveModel.OrginalDuration = modeldata.OrginalDuration;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.OpendialogActionEARLYOUT = function (arg) {
        try {


            var eDialog = $("#dialogAction").data("ejDialog");
            eDialog.open();
            var gridObj = $("#Grid2").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];
            //$("#dialogDesignation").ejDialog("setTitle", modeldata.UserName);

            $scope.ApproveInfoModel = {};
            $scope.ApproveInfoModel = modeldata;
            $scope.ApproveModel.Duration = modeldata.Duration;
            $scope.ApproveModel.OrginalDuration = modeldata.OrginalDuration;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };







    $scope.TakeAction = function (arg) {
        try {

            if (baseService.isUndefinedOrNull($scope.ApproveModel.HourlyLeaveReasonId)) {
                throw 'Please select Hourly Leave Reason.';
            }

            if (baseService.isUndefinedOrNull($scope.ApproveModel.ApproveType)) {
                throw 'Please select Approval Type.';
            }

            if (parseInt($scope.ApproveModel.Duration) > parseInt($scope.ApproveModel.OrginalDuration)) {
                throw 'Duration must be less than or equal to ' + $scope.ApproveModel.OrginalDuration +' minutes.';
            }

            if ($scope.ApproveModel.ApproveType == 'Leave') {
                if (baseService.isUndefinedOrNull($scope.ApproveModel.LeaveTypeId)) {
                    throw 'Please select Leave Type.';
                }
            }


            $http({
                method: 'Post',
                url: $scope.path + '/Save',
                data: {
                    'DutyHour': $scope.ApproveInfoModel
                    ,'Duration': $scope.ApproveModel.Duration
                    ,'ApproveType': $scope.ApproveModel.ApproveType
                    ,'LeaveTypeId': $scope.ApproveModel.LeaveTypeId
                    ,'HourlyLeaveReasonId': $scope.ApproveModel.HourlyLeaveReasonId
                }
            }).then(function successCallback(response) {
                var eDialog = $("#dialogAction").data("ejDialog");
                eDialog.close();
                $scope.LeaveTypeList = [];
                $scope.LeaveTypeList = response.data;


            });

     

            //$scope.ApproveInfoModel = {};
            //$scope.ApproveInfoModel = modeldata;


        } catch (e) {
            ShowResult(e, "failure");
        }
    };



    //$scope.ChangeDuration = function () {       

    //    if (!baseService.isUndefinedOrNull($scope.ApproveInfoModel.FromDate) && !baseService.isUndefinedOrNull($scope.ApproveInfoModel.Duration)) {
    //        //Date then minite get get new date//
    //        var dt = new Date($scope.ApproveInfoModel.FromDate);
    //        var minutes = $scope.ApproveInfoModel.Duration;
    //        var d = dt.setTime(dt.getTime() + minutes * 60000);
    //        $scope.ApproveInfoModel.ToDate = dt;
    //    }
    //}
};



