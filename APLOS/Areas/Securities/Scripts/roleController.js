'use strict';
RoleController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function RoleController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $compile) {
    $rootScope.title = "Role";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.roles = [];
    $scope.path = 'Securities/role/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Name', 'Name');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.roles = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $rootScope.searchByList = [
        {
            'name': 'Name',
            'value': 'Name'
        }, {
            'name': 'Panel',
            'value': 'PanelName'
        }
    ];

    $scope.role = {
        Id: null,
        CompanyGroupId: null,
        Name: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.roleNew = Object.assign({}, $scope.role);

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.role = $scope.roles[$scope.index];
        $scope.roleNew = Object.assign({}, $scope.role);
        $scope.roleNew.AddedDate = $filter('dateFilter')($scope.role.AddedDate);
        $scope.roleNew.UpdatedDate = $filter('dateFilter')($scope.role.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.roleForm.$valid) {
            angular.copy($scope.roleNew, $scope.role);
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.role,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.roles.push(response.data.Role);
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.statusText.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.role,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.roles[$scope.index] = $scope.role;
                        }
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.statusText.Message, 'failure');
                });
                return true;
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.roleNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.roleNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.roles.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.statusText.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    }

    $scope.Clear = function () {
        ClearFields();
    }

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.role = {};
        $scope.roleNew = { Active: true };
    }
}