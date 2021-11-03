'use strict';
offDutyApproveController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function offDutyApproveController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Off Duty Approval';
    $scope.path = 'Leave/OffDutyApprove/';
    $scope.getOffDutyApproveUrl = $scope.path + 'GetOffDutyApproveInfo';
    $scope.getLeaveTypeUrl = $scope.path + 'GetLeaveTypeInfo';
    $scope.SaveDailyAllowanceUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.OffDutyInfoList = [];
    $scope.GetOffDutyApproveInfo = function () {
        try {
            $http.get($scope.getOffDutyApproveUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.OffDutyInfoList = response.data;
                        $scope.GetLeaveInfo();
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetOffDutyApproveInfo();


    $scope.GetLeaveList = [];
    $scope.MainLeaveList = [];
    $scope.GetLeaveInfo = function () {
        try {
            $http.get($scope.getLeaveTypeUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.MainLeaveList = response.data;
                        for (var i = 0; i < $scope.OffDutyInfoList.length; i++) {
                            $scope.OffDutyInfoList[i].LeaveTypeList = [];
                            $scope.OffDutyInfoList[i].LeaveTypeListBlank = $scope.MainLeaveList;
                        }
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        try {
            var NewOffDutyDelete = [];
            for (var i = 0; i < $scope.OffDutyInfoList.length; i++) {
                if ($scope.OffDutyInfoList[i].CheckBoxSelect == true) {
                    NewOffDutyDelete.push($scope.OffDutyInfoList[i].Id)
                }
            }

            if (NewOffDutyDelete.length == 0) {
                throw 'Please Check First...';
            }           

            $.ajax({
                type: "POST",
                url: $scope.deleteUrl,
                data: { 'Id': NewOffDutyDelete },
                dataType: "json",
                success: function (data) {
                    if (data.Error === true) {
                        ShowResult(data.Message, "failure");
                    }
                    else {
                        ShowResult(data.Message, "success");
                        $scope.GetOffDutyApproveInfo();
                        var gridObj = $("#GridShiftInfo").data("ejGrid");
                        gridObj.refreshContent();
                    }
                }
            });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveDailyAllowanceData = function () {
        try {

            var OffDutyInFoListNew = [];
            for (var i = 0; i < $scope.OffDutyInfoList.length; i++) {
                if ($scope.OffDutyInfoList[i].CheckBoxSelect == true) {
                    if (baseService.isUndefinedOrNull($scope.OffDutyInfoList[i].ApproveType)) {
                        throw 'Please Select ApproveType';
                    } else {
                        OffDutyInFoListNew.push($scope.OffDutyInfoList[i]);
                    }
                }
            }

            if (OffDutyInFoListNew.length == 0) {
                throw 'Please Check..';
            }

            $.ajax({
                type: "POST",
                url: $scope.SaveDailyAllowanceUrl,
                data: { 'OffDutyApprove': OffDutyInFoListNew },
                dataType: "json",
                success: function (data) {
                    if (data.Error === true) {
                        ShowResult(data.Message, "failure");
                    }
                    else {
                        ShowResult(data.Message, "success");
                        $scope.OffDutyInfoList = [];
                        $scope.GetOffDutyApproveInfo();
                    }
                }
            });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.ApproveTypeList = [{ Text: '--Select--' }, { Text: 'Waive' }, { Text: 'Leave' }, { Text: 'Deducation' }]

    $scope.changeLeave = function (obj) {
        var gridObj = $("#GridShiftInfo").ejGrid("instance");
        var currRow = gridObj.model.currentViewData[this.element.closest("tr").index()];

        if (obj.text == 'Leave') {
            currRow.LeaveTypeList = currRow.LeaveTypeListBlank;
        }
        else {
            currRow.LeaveTypeList = [];
        }
        gridObj.refreshContent(true);
    }

    $scope.ChangeDuration = function () {

        //TWO DATE SELECT GET MINITE//
        //var diff = Math.abs(new Date($scope.OffDutyHoursModel.FromDate) - new Date($scope.OffDutyHoursModel.ToDate));
        //var minutes = Math.floor((diff / 1000) / 60);
        //$scope.OffDutyHoursModel.Duration = minutes;

        if (!baseService.isUndefinedOrNull($scope.OffDutyHoursModel.FromDate) && !baseService.isUndefinedOrNull($scope.OffDutyHoursModel.Duration)) {
            //Date then minite get get new date//
            var dt = new Date($scope.OffDutyHoursModel.FromDate);
            var minutes = $scope.OffDutyHoursModel.Duration;
            var d = dt.setTime(dt.getTime() + minutes * 60000);
            $scope.OffDutyHoursModel.ToDate = dt;
        }
    }

}