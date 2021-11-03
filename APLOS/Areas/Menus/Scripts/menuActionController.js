'use strict';
MenuActionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function MenuActionController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Menu";
    $scope.Action = 'Save';
    $scope.menuActions = [];
    $scope.menuList = [];
    $scope.path = 'Menus/menuaction/';
    $scope.getListUrl = $scope.path + 'getallmenuactionlist';
    baseService.init($scope.getListUrl, null, null, null, 'Area, Controller, UserName', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.menuActions = result.Rows;
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
        },
        {
            'name': 'Menu Name',
            'value': 'MenuName'
        }
    ];
}