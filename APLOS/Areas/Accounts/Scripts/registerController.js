'use strict';
registerController.$inject = ['cboService', '$route', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function registerController(cboService, $route, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Register';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.modules = [];
    $scope.path = 'accounts/Register/';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init('accounts/Register/getlist');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.modules = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.module = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Image: null,
        Active: true
    };

    $scope.getSequence = function () {
        cboService.getSequence('accounts/Register/getautosequence', function (result) {
            $scope.module.Sequence = result;
        });
    };
    $scope.getSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.module = baseService.find($scope.modules, id, null);
        $scope.module.AddedDate = $filter('dateFilter')($scope.module.AddedDate);
        $scope.module.UpdatedDate = $filter('dateFilter')($scope.module.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.moduleForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.module,
                    dataType: 'JSON'
                }).then(
                    function success(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.modules.push(response.data.ModelData);
                            baseService.paginationAdd();
                            ClearFields(response.data.Sequence);
                        }
                    }, function error(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.module,
                    dataType: 'JSON'
                }).then(function success(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.modules[$scope.index] = $scope.module;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function error(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.module.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.module.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.modules.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields($scope.getSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.module = {};
        $scope.module.Sequence = seq;
        $scope.module.Active = true;
    }
}