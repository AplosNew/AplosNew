'use strict';
function ControlAdminController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.compareTo = false
    $scope.passwordShow = true;
    $scope.inactive = false;
    $scope.messaeShow = false;
    $scope.Action = 'Save';
    $scope.controlAdmins = [];
    $scope.path = 'Securities/ControlAdmin/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, "UserId", "UserId");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.controlAdmins = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $rootScope.searchByList = [
        {
            'name': 'Username',
            'value': 'UserId'
        },
        {
            'name': 'Full Name',
            'value': 'FullName'
        },
        {
            'name': 'Email',
            'value': 'Email'
        }
    ];

    $scope.controlAdmin = {
        UserId: null,
        FullName: null,
        Email: null,
        Password: null,
        ConfirmPassword: null,
        Archive: false
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.passwordShow = false;
        $scope.inactive = true;
        $scope.controlAdmin = $scope.controlAdmins[$scope.index];
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.compare = function (p1, p2) {
        $scope.result = angular.equals(p1, p2);
        if (!$scope.result)
            $scope.compareTo = true;
        else
            $scope.compareTo = false;
    };

    $scope.Save = function () {
        if ($scope.passwordShow == true) {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.controlAdminForm.$valid) {
                if (!angular.equals($scope.controlAdmin.Password, $scope.controlAdmin.ConfirmPassword))
                    return ShowResult('Confirm password does not match.', 'failure');
                if ($scope.Action == 'Save') {
                    $http({
                        method: 'POST',
                        url: 'Securities/ControlAdmin/Create',
                        data: $scope.controlAdmin,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.controlAdmin = response.data.ControlAdmin;
                            $scope.controlAdmin.AddedDate = $filter('dateFilter')($scope.controlAdmin.AddedDate);
                            $scope.controlAdmins.push($scope.controlAdmin);
                            baseService.paginationAdd();
                            ClearFields();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
            }
        }
        else if ($scope.Action == 'Update') {
            $http({
                method: 'POST',
                url: 'Securities/ControlAdmin/Edit',
                data: $scope.controlAdmin,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    if ($scope.index > -1) {
                        $scope.controlAdmins[$scope.index] = $scope.controlAdmin;
                    }
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        return true;
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.controlAdmin.UserId)) {
            $http({
                method: 'POST',
                url: 'Securities/controladmin/delete?userId=' + $scope.controlAdmin.UserId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.controlAdmins.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields()
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.controlAdmin = {};
        $scope.controlAdmin.Archive = false;
        $scope.inactive = false;
        $scope.passwordShow = true;
    }
}
ControlAdminController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];