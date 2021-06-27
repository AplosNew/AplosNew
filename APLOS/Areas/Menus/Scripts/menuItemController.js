'use strict';
MenuItemController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function MenuItemController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Menu Item";
    $scope.Action = 'Save';
    $scope.menuItems = [];
    $scope.areaList = [];
    $scope.menuList = [];
    $scope.path = 'Menus/menuitem/';
    $scope.getListUrl = $scope.path + 'getmenuitemlist';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.menuItems = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $http({
        method: 'GET',
        url: 'Menus/menu/getarealist'
    }).then(function successCallback(response) {
        $scope.areaList = response.data;
    });

    $scope.onChange = function (item) {
        $http({
            method: 'GET',
            url: 'Menus/menu/getcontrollerlist?area=' + item
        }).then(function successCallback(response) {
            $scope.menuList = response.data;
        });
    };

    $scope.menuItem = {
        Id: null,
        MenuId: null,
        Area: null,
        MenuItemGroup: null,
        Sequence: 0,
        Code: null,
        UserCode: null,
        InterfaceNo: null,
        StandardName: null,
        UserName: null,
        TooltipName: null,
        MaximumUser: 0,
        MaximumInactiveTime: 0,
        Image: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $rootScope.searchByList[2] = {
        'name': 'User Code',
        'value': 'UserCode'
    };
    $rootScope.searchByList.push({
        'name': 'InterfaceNo',
        'value': 'InterfaceNo'
    });
    $rootScope.searchByList.push({
        'name': 'Menu Id',
        'value': 'MenuId'
    });
    $rootScope.searchByList.push({
        'name': 'Menu Item Group',
        'value': 'MenuItemGroup'
    });

    $scope.GetSequence = function () {
        $http.get('Menus/menuitem/getautosequence')
            .then(function (response) {
                $scope.menuItem.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.menuItem = $scope.menuItems[$scope.index];
        $scope.menuItem.AddedDate = $filter('dateFilter')($scope.menuItem.AddedDate);
        $scope.menuItem.UpdatedDate = $filter('dateFilter')($scope.menuItem.UpdatedDate);
        $scope.onChange($scope.menuItem.Area);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.menuItemForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Menus/menuitem/create',
                    data: $scope.menuItem,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        var area = $scope.menuItem.Area;
                        $scope.menuItem = response.data.MenuItem;
                        $scope.menuItem.Area = area;
                        $scope.menuItem.AddedDate = $filter('dateFilter')($scope.menuItem.AddedDate);
                        $scope.menuItems.push($scope.menuItem);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'Menus/menuitem/edit',
                    data: $scope.menuItem,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.menuItems[$scope.index] = $scope.menuItem;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.menuItem.Id)) {
            $http({
                method: 'POST',
                url: 'Menus/menuitem/delete/' + $scope.menuItem.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.menuItems.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(data.Sequence);
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

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.menuItem = {};
        $scope.menuItem.Sequence = seq;
        $scope.menuItem.MaximumUser = 0;
        $scope.menuItem.MaximumInactiveTime = 0;
        $scope.menuItem.Active = true;
    }
}