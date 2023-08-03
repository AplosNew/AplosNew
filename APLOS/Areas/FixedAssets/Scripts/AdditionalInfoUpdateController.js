'use strict';
AdditionalInfoUpdateController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function AdditionalInfoUpdateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'AdditionalInfoUpdate';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'FixedAssets/FixedAssetMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.model = {
        Id: null,
        FixedAssetItemId: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };

    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.assetItemList = [];
    $scope.GetAssetItemData = function () {
        $http({
            method: "Get",
            url: 'fixedassets/fixedassetmaster/getFAMIlist',
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.assetItemList = response.data.Rows;
        });
        angular.element(document.querySelector('#AssetItemPopUp')).modal('show');
    }
    $scope.CloseAssetItemPopUp = function () {
        angular.element(document.querySelector('#AssetItemPopUp')).modal('hide');
    }

    $scope.SetAssetItem = function (e) {
        $scope.modelNew.FixedAssetItemId = e.data.Id;
        $scope.modelNew.FixedAssetItem = e.data.UserName;
        $scope.getAdditionalData($scope.modelNew.FixedAssetItemId);
        angular.element(document.querySelector('#AssetItemPopUp')).modal('hide');
    }

    $scope.AdditionalInfoItemList = [];
    $scope.getAdditionalData = function (masterId) {
        $http({
            method: 'GET',
            url: 'FixedAssets/FixedAssetMaster/getAdditionalData?masterId=' + masterId
        }).then(function successCallback(response) {
            $scope.AdditionalInfoItemList = response.data;
        });
    }

    $scope.AdditionalInfoUpdateList = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: 'FixedAssets/FixedAssetMaster/GetAdditionallInfoUpdateData'
        }).then(function successCallback(response) {
            $scope.AdditionalInfoUpdateList = response.data;
        });
    }
    $scope.getData();

    $scope.Get = function (args) {
        $scope.modelNew = Object.assign({}, args);
        $scope.getAdditionalData($scope.modelNew.FixedAssetItemId);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.XSave = function () {
        angular.copy($scope.modelNew, $scope.model);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.AdditionalInfoUpdateNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'FixedAssets/FixedAssetMaster/CreateAdditionallInfoUpdate',
                    data: $scope.model,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'FixedAssets/FixedAssetMaster/CreateAdditionallInfoUpdate',
                    data: $scope.model,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Save = function () {
        angular.copy($scope.modelNew, $scope.model);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.AdditionalInfoUpdateNewForm.$valid) {
            if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'FixedAssets/FixedAssetMaster/CreateAdditionallInfoUpdate',
                    data: { 'data': $scope.model },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ClearFAMI();
                        $scope.getFAMIData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.AdditionalInfoUpdates.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.modelNew = {};
    }
}