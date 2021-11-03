'use strict';
MenuMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', 'cboService'];
function MenuMasterController(commonMessage, $scope, $rootScope, baseService, $http, cboService) {
    $rootScope.title = "Menu Master";
    $scope.Action = 'Save';
    $scope.tableShow = false;
    $scope.index = -1;
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
    $scope.path = 'Menus/menumaster/';
    $scope.getListUrl = $scope.path + 'getmenumasteralllist';
    baseService.init($scope.getListUrl, null, null, null, 'MenuFrame, MenuGroup, MenuSubGroup, [Sequence]', 'MenuFrame');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.menuMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $http({
        method: 'GET',
        url: 'Menus/MenuItem/GetMenuItemGroupList'
    }).then(function successCallback(response) {
        $scope.menuItemGroupList = response.data;
    });

    $rootScope.searchByList = [
        {
            'name': 'Module',
            'value': 'Module'
        },
        {
            'name': 'SubModule',
            'value': 'SubModule'
        },
        {
            'name': 'Menu Frame',
            'value': 'MenuFrame'
        },
        {
            'name': 'Menu Group',
            'value': 'MenuGroup'
        },
        {
            'name': 'Menu SubGroup',
            'value': 'MenuSubGroup'
        },
        {
            'name': 'Menu Item',
            'value': 'MenuItem'
        }
    ];

    $scope.onShowenuItem = function (item, moduleId, menuFrameId) {
        if (baseService.isUndefinedOrNull(item) ||
            baseService.isUndefinedOrNull(moduleId) ||
            baseService.isUndefinedOrNull(menuFrameId)) {
            ShowResult('Please select MenuItem Group, Module and MenuFrame.', 'failure');
            return false;
        }
        $http.get('Menus/menumaster/getmenuitembymenuitemgrouplist?menuItemGroup=' + item + '&moduleId=' + moduleId + '&menuFrameId=' + menuFrameId)
            .then(function (response) {
                $scope.menuItems = response.data;
                if ($scope.menuItems.length > 0) {
                    $scope.tableShow = true;
                }
                else {
                    $scope.tableShow = false;
                    ShowResult(commonMessage.RecordNotFound, 'failure');
                }
            });
    };

    cboService.getCboModule(function (result) {
        $scope.moduleList = result;
    });

    $scope.onChangeModule = function (item) {
        cboService.getCboSubModuleByModule(item, function (result) {
            $scope.subModuleList = result;
        });
    };

    $http({
        method: 'GET',
        url: 'Menus/MenuFrame/GetMenuFrameCbo'
    }).then(function successCallback(response) {
        $scope.menuFrameList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Menus/MenuGroup/GetMenuGroupCbo'
    }).then(function successCallback(response) {
        $scope.menuGroupList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Menus/MenuSubGroup/GetMenuSubGroupCbo'
    }).then(function successCallback(response) {
        $scope.menuSubGroupList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Menus/MenuItem/GetMenuItemCbo'
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
        IsExternalMenu: false,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $scope.Push = function () {
        $scope.menuItemIdList = [];
        angular.forEach($scope.menuItems, function (item, index) {
            if (item.Active === true) {
                $scope.menuItemIdList.push(item.Id);
            }
        });
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.menuMasterForm.$valid) {
            $scope.Push();
            $http({
                method: 'POST',
                url: 'Menus/menumaster/create',
                data: { 'menuMaster': $scope.menuMaster, 'menuItemIds': $scope.menuItemIdList }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    ClearFields();
                }
            });
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.menuMaster = {};
        $scope.menuMaster.Active = true;
        $scope.menuItems = null;
        $scope.tableShow = false;
    }
}