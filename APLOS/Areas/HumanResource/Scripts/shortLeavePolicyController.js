'use strict';
shortLeavePolicyController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', '$http', '$window'];
function shortLeavePolicyController(cboService, commonMessage, $scope, $rootScope,  $http, $window) {
    $rootScope.title = 'Short Leave Policy';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.shortLeavePolicy = [];
    $scope.plantList = [];
    $scope.path = 'HumanResource/shortLeavePolicy/';
    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';


    cboService.getCboPlantByCompanyGroup(null, function (result) {
        $scope.plantList = result;
    });

    $scope.getData = function (id) {
        $http.get($scope.path + 'getlist?plantId=' + id)
            .then(function (response) {
                $scope.getListUrlByPlant = response.data.Rows[0];
                if ($scope.getListUrlByPlant) {
                    $scope.shortLeavePolicyNew.SystemId = $scope.getListUrlByPlant.SystemId;
                    $scope.shortLeavePolicyNew.PlantName = $scope.getListUrlByPlant.PlantName;
                    $scope.shortLeavePolicyNew.MaxShortLeaveInaMonth = $scope.getListUrlByPlant.MaxShortLeaveInaMonth;
                    $scope.shortLeavePolicyNew.IsHalfDayPresentAllowed = $scope.getListUrlByPlant.IsHalfDayPresentAllowed;
                    $scope.shortLeavePolicyNew.IsShortLeaveAllowed = $scope.getListUrlByPlant.IsShortLeaveAllowed;
                    $scope.shortLeavePolicyNew.IsTowShortLeaveAllowedInaDay = $scope.getListUrlByPlant.IsShortLeaveAllowed;
                    $scope.Action = 'Update';
                }
                else {
                    $scope.ClearFields();
                }
            });
      
    };


    $scope.Get = function () {
        $scope.shortLeavePolicyNew.SystemId;
        $scope.shortLeavePolicyNew.PlantName;
        $scope.shortLeavePolicyNew.MaxShortLeaveInaMonth;
        $scope.shortLeavePolicyNew.IsHalfDayPresentAllowed;
        $scope.shortLeavePolicyNew.IsShortLeaveAllowed;
        $scope.shortLeavePolicyNew.IsTowShortLeaveAllowedInaDay;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.shortLeavePolicyNew = {
        SystemId: null,
        IsShortLeaveAllowed: null,
        IsHalfDayPresentAllowed: null,
        IsTowShortLeaveAllowedInaDay: null,
        MaxShortLeaveInaMonth: null,
        PlantId: $window.plantId,
        GroupId: $window.companyGroupId,
        PlantName: null

    };

    $scope.SaveMaster = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.shortLeavePolicyNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.shortLeavePolicyNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.shortLeavePolicyNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };


    $scope.Delete = function () {
        if ($scope.shortLeavePolicyNew.SystemId !== null) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.shortLeavePolicyNew.SystemId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearFields();
                }
            },
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
            });
        }
    };

    $scope.ClearFields = function () {
        $scope.Action = 'Save';
        $scope.shortLeavePolicyNew.PlantName = null;
        $scope.shortLeavePolicyNew.MaxShortLeaveInaMonth = null;
        $scope.shortLeavePolicyNew.IsShortLeaveAllowed = false;
        $scope.shortLeavePolicyNew.IsHalfDayPresentAllowed = false;
        $scope.shortLeavePolicyNew.IsTowShortLeaveAllowedInaDay = false;
    };
}

