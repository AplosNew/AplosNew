'use strict';
MenuController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function MenuController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Menu";
    $scope.Action = 'Save';
    $scope.menuList = [];
    $scope.actionList = [];
    $scope.index = -1;

    $scope.path = 'Menus/menu/';
    $scope.getListUrl = $scope.path + 'getallmenulist';
    $scope.getActionListUrl = $scope.path + 'GeActionListByMenu?menuId=';

    $scope.saveUrl = $scope.path + 'Create';
    $scope.updateUrl = $scope.path + 'Edit';
    $scope.deleteUrl = $scope.path + 'Delete/';
    $scope.actionDeleteUrl = $scope.path + 'DeleteMenuAction/';

    baseService.init($scope.getListUrl, null, null, null, 'Area, Controller, UserName', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.menuList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $rootScope.searchByList = [
        {
            'name': 'Area',
            'value': 'Area'
        },
        {
            'name': 'Controller',
            'value': 'Controller'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Id',
            'value': 'Id'
        }
    ];

    $scope.model = {
        Id: null
        , Area: null
        , UserName: null
        , Controller: null
        , Href: null
        , IFCodes: null
        , Description: null
        , Active: true
    };

    $scope.get = function (index) {
        $scope.Action = 'Update';
        $scope.index = index;
        $scope.model = $scope.menuList[$scope.index];
        getRecipeMaterialList();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.modelForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: { 'entity': $scope.model, 'actionList': $scope.actionList }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST'
                    , url: $scope.updateUrl
                    , data: { 'entity': $scope.model, 'actionList': $scope.actionList }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.model.Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl + $scope.model.Id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.menuList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };

    $scope.removePopup = function (data, index) {
        $scope.id = data.Id;
        $scope.cindex = index;
        $scope.message = 'Are you sure want to permanent delete this?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
    };

    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.id)) {
            $http({
                method: 'POST'
                , url: $scope.actionDeleteUrl + $scope.id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.actionList.splice($scope.cindex, 1);
                }
            }, function errorCallback(response) {
                ShowResult(status.Message, 'failure');
            });
        }
        else
            $scope.actionList.splice($scope.cindex, 1);
        $scope.cindex = -1;
    };

    function getRecipeMaterialList() {
        $http.get($scope.getActionListUrl + $scope.model.Id)
            .then(function (response) {
                $scope.actionList = response.data;
            });
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.model = { Active: true };
        $scope.actionList = [];
        $scope.index = -1;
    }

    $scope.add = function () {
        $scope.actionList.push({
            Id: null
            , MenuId: null
            , Action: null
            , UserName: null
            , Description: null
            , Active: true
        });
    };
}