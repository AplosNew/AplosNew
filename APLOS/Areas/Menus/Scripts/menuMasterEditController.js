'use strict';
MenuMasterEditController.$inject = ['commonMessage', '$scope', '$rootScope', '$routeParams', '$http', '$filter', '$window', 'cboService', 'baseService'];
function MenuMasterEditController(commonMessage, $scope, $rootScope, $routeParams, $http, $filter, $window, cboService, baseService) {
    $rootScope.title = "Menu Master Edit";
    $scope.menuItemGroupList = [];
    $scope.moduleList = [];
    $scope.subModuleList = [];
    $scope.menuFrameList = [];
    $scope.menuGroupList = [];
    $scope.menuSubGroupList = [];
    $scope.searchSubModuleList = [];
    $scope.menuItemIdList = [];
    $scope.menuItemList = [];
    $scope.searchModuleId;
    $scope.searchSubModuleId;
    Get($routeParams.id);

    $http({
        method: 'GET',
        url: 'Menus/MenuItem/GetMenuItemGroupList'
    }).then(function successCallback(response) {
        $scope.menuItemGroupList = response.data;
    });

    $scope.onChangeMenuItem = function (item) {
        $http.get('Menus/MenuItem/GetMenuItemByMenuItemGroupList?menuItemGroup=' + item)
            .then(function (response) {
                $scope.menuItems = response.data;
            });
    };

    cboService.getCboModule(function (data) {
        $scope.moduleList = data;
    });

    $scope.onChangeModule = function (item) {
        cboService.getCboSubModuleByModule(item, function (result) {
            $scope.subModuleList = result;
        });
    };

    $http({
        method: 'GET',
        url: 'Menus/menuframe/getmenuframecbo'
    }).then(function successCallback(response) {
        $scope.menuFrameList = response.data;
    });

    $http({
        method: 'get',
        url: 'Menus/menugroup/getmenugroupcbo'
    }).then(function successCallback(response) {
        $scope.menuGroupList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Menus/menusubgroup/getmenusubgroupcbo'
    }).then(function successCallback(response) {
        $scope.menuSubGroupList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Menus/menuitem/getmenuitemcbo'
    }).then(function successCallback(response) {
        $scope.menuItemList = response.data;
    });

    $scope.menuMaster = {
        Id: null,
        ModuleId: null,
        SubModuleId: null,
        MenuItemGroup: null,
        MenuFrameId: null,
        MenuGroupId: null,
        MenuSubGroupId: null,
        MenuItemId: null,
        PanelName: null,
        Description: null,
        Remarks: null,
        Active: true,
        IsExternalMenu: false,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    function Get(id) {
        $http.get('Menus/MenuMaster/GetMenuMaster?id=' + id)
            .then(function (response) {
                $scope.menuMaster = response.data;
            });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.menuMasterForm.$valid) {
            $http({
                method: 'POST',
                url: 'Menus/menumaster/edit',
                data: { 'menuMaster': $scope.menuMaster },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        return true;
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.menuMaster.Id)) {
            $http({
                method: 'POST',
                url: 'Menus/menumaster/Delete?id=' + $scope.menuMaster.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    setTimeout(function () {
                        $window.history.back();
                    }, 3000);
                }
            }, function errorCallback(response) {
                ShowResult(status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Back = function () {
        $window.history.back();
    };
}