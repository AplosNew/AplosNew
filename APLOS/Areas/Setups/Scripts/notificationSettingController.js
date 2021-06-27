'use strict';
notificationSettingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$http', '$filter', '$window', 'cboService'];
function notificationSettingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $filter, $window, cboService) {
    $rootScope.title = "Notification Setting";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Setups/NotificationSetting/';

    $scope.notificationSetting = {
        Id: null,
        CompanyId: null,
        PlantId: null,
        BusinessFlow: null,
        NotificationAfterCreation: false,
        RequiredChecking: false,
        NotificationAfterChecking: false,
        RequiredApproval: false,
        NotificationAfterApproval: false
    }


    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.notificationSetting.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };

    $scope.ActionStatusList = [];

    function getList() {
        cboService.getEnumCbo("enum/GetNotificationEnumCbo", function (result) {
            $scope.ActionStatusList = result;

            for (var i = 0; i < $scope.ActionStatusList.length; i++) {
                $scope.ActionStatusList[i].BusinessFlow = $scope.ActionStatusList[i].Value;
                $scope.ActionStatusList[i].Id = null;
                $scope.ActionStatusList[i].PlantId = null;
                $scope.ActionStatusList[i].NotificationAfterCreation = false;
                $scope.ActionStatusList[i].RequiredChecking = false;
                $scope.ActionStatusList[i].NotificationAfterChecking = false;
                $scope.ActionStatusList[i].RequiredApproval = false;
                $scope.ActionStatusList[i].NotificationAfterApproval = false;
            }

        });
    }
    getList();

    $scope.List = [];
    $scope.getSaveList = function () {
        getList();
        $http({
            method: 'GET',
            url: 'setups/NotificationSetting/GetList',
            params: { 'plantId': $scope.notificationSetting.PlantId }
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.List = response.data;
                for (var t = 0; t < baseService.arrayLength($scope.ActionStatusList); t++) {
                    for (var i = 0; i < baseService.arrayLength($scope.List); i++) {
                        if (!baseService.isUndefinedOrNull($scope.List[i].Id) && $scope.List[i].BusinessFlow === $scope.ActionStatusList[t].Value) {

                            $scope.ActionStatusList[t].Id = $scope.List[i].Id;
                            $scope.ActionStatusList[t].BusinessFlow = $scope.List[i].BusinessFlow;
                            $scope.ActionStatusList[t].PlantId = $scope.List[i].PlantId;
                            $scope.ActionStatusList[t].NotificationAfterCreation = $scope.List[i].NotificationAfterCreation;
                            $scope.ActionStatusList[t].RequiredChecking = $scope.List[i].RequiredChecking;
                            $scope.ActionStatusList[t].NotificationAfterChecking = $scope.List[i].NotificationAfterChecking;
                            $scope.ActionStatusList[t].RequiredApproval = $scope.List[i].RequiredApproval;
                            $scope.ActionStatusList[t].NotificationAfterApproval = $scope.List[i].NotificationAfterApproval;
                        }
                    }
                }
            }
            else {
                $scope.ActionStatusList[i].Id = null;
                $scope.ActionStatusList[i].PlantId = null;
                $scope.ActionStatusList[i].NotificationAfterCreation = false;
                $scope.ActionStatusList[i].RequiredChecking = false;
                $scope.ActionStatusList[i].NotificationAfterChecking = false;
                $scope.ActionStatusList[i].RequiredApproval = false;
                $scope.ActionStatusList[i].NotificationAfterApproval = false;
            }
        });
    }



    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.notificationSetting.PlantId)) {
                throw "Select Plant.";
            }
            for (var i = 0; i < $scope.ActionStatusList.length; i++) {
                $scope.ActionStatusList[i].PlantId = $scope.notificationSetting.PlantId;

                if ($scope.ActionStatusList[i].NotificationAfterCreation) {
                    if (!$scope.ActionStatusList[i].RequiredChecking && !$scope.ActionStatusList[i].NotificationAfterChecking
                        && !$scope.ActionStatusList[i].RequiredApproval && !$scope.ActionStatusList[i].NotificationAfterApproval) {
                        throw "Only Notification After Creation is not allowed for " + $scope.ActionStatusList[i].BusinessFlow + ".";
                    }
                }
                if ($scope.ActionStatusList[i].NotificationAfterChecking) {
                    if (!$scope.ActionStatusList[i].RequiredChecking) {
                        throw "Required Checking is mandatory for " + $scope.ActionStatusList[i].BusinessFlow + ".";
                    }
                }
                if ($scope.ActionStatusList[i].RequiredChecking) {
                    if (!$scope.ActionStatusList[i].RequiredApproval) {
                        throw "Required Approval is mandatory for " + $scope.ActionStatusList[i].BusinessFlow+".";
                    }
                }
                
                if ($scope.ActionStatusList[i].NotificationAfterApproval) {
                    if (!$scope.ActionStatusList[i].RequiredApproval) {
                        throw "Required Approval is mandatory for " + $scope.ActionStatusList[i].BusinessFlow + ".";
                    }
                }

            }
            if ($scope.NotificationSettingForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.path + 'Create',
                    data: { 'entities': $scope.ActionStatusList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getSaveList();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                });
                return true;
            }
            //}
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


}