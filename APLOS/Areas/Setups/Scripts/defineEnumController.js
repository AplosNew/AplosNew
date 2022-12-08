'use strict';
defineEnumController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function defineEnumController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Define Enum";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.defineEnums = [];
    $scope.path = 'Setups/BusinessProcess/';
    $scope.saveUrl = $scope.path + 'SaveDefineEnum';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'DeleteDefineEnum/';
    $scope.getListUrl = $scope.path + 'GetDefineEnumlist';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.defineEnums = result;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.defineEnum = {
        Id: null
        , Category: null
        , EnumName: null
        , UserName: null
        , Active: true
    };

    $scope.defineEnumNameList = [];
    $http({
        method: 'GET',
        url: 'Setups/BusinessProcess/GetCboDefineEnumName/'
    }).then(function successCallback(response) {
        $scope.defineEnumNameList = response.data;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.defineEnum = $scope.defineEnums[$scope.index];
        $scope.defineEnum.AddedDate = $filter('dateFilter')($scope.defineEnum.AddedDate);
        $scope.defineEnum.UpdatedDate = $filter('dateFilter')($scope.defineEnum.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.brandForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'datas': $scope.defineEnum, },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }), function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'datas': $scope.defineEnum, },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.defineEnum.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.defineEnum.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    ClearFields();
                } function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.defineEnum = {};
        $scope.defineEnum.Active = true;
    }
}