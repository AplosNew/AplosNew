'use strict';
OTPlanningController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function OTPlanningController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'OT Planning';
    $scope.path = 'humanresource/OTPlanning/';


    $scope.MasterModel = {
        Id: null,
        Date: '',
        ShiftId: null
    }


    $scope.ShiftList = [];
    $scope.getShift = function () {
        $http({
            method: 'GET',
            url: 'humanresource/dailydaystatus/GetShift',
        }).then(function successCallback(response) {
            $scope.ShiftList = response.data;
        });
    }
    $scope.getShift();

    $scope.EmpDataList = [];
    $scope.SaveEmpDataList = [];
    $scope.getData = function () {
        if ($scope.MasterModel.Date != '') {
            $scope.date = $filter('dateFiltering')(new Date($scope.MasterModel.Date), 'dd-MM-yyyy');
        }
        else {
            $scope.date = '';
        }
        $http({
            method: 'GET',
            url: $scope.path + 'GetEmpData?Date=' + $scope.date + '&ShiftId=' + $scope.MasterModel.ShiftId,
        }).then(function successCallback(response) {
            $scope.EmpDataList = response.data;
        });
    }
    //$scope.getData();

    //#region Save General Tax
    $scope.Save = function () {
        try {
            $scope.MasterModel.Id = $scope.EmpDataList[0].OtPlanMst;
            for (var i = 0; i < $scope.EmpDataList.length; i++) {
                if ($scope.EmpDataList[i].NoOfEmp < $scope.EmpDataList[i].AllotedMan) {
                    throw "Alloted Men Can't be greater than No Of Emp..";
                }
            }

            $scope.SaveEmpDataList = [];
            for (var i = 0; i < $scope.EmpDataList.length; i++) {
                if ($scope.EmpDataList[i].CheckBoxSelect == true) {
                    $scope.SaveEmpDataList.push($scope.EmpDataList[i]);
                }
            }
            if ($scope.SaveEmpDataList.length == []) {
                throw "Please Check the box to save..";
            }
            $http({
                method: 'POST',
                url: $scope.path + "Save",
                data: { 'MasterData': $scope.MasterModel, 'Details': $scope.SaveEmpDataList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    for (var i = 0; i < response.data.Data.length; i++) {
                        $scope.EmpDataList[i].Id = response.data.Data[i];
                    }                    
                    $scope.MasterModel.Id = response.data.Data[0].OtPlanMst;
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridAttendance").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmpDataList.length; i++) {
                $scope.EmpDataList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridAttendance").data("ejGrid");
        gridObj.refreshContent();
    };
    //#endregion
}
