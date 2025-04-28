'use strict';
operationActivityController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function operationActivityController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Operation Activity";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.operationActions = [];
    $scope.path = 'Machines/operationActivity/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.operationActions = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.operationAction = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        ProcessId: null
    };
    $scope.operationActionNew = Object.assign({}, $scope.operationAction);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.operationActionNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.operationAction = $scope.operationActions[$scope.index];
        $scope.operationActionNew = Object.assign({}, $scope.operationAction);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetCboProcess = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboProcess'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });
    }
    $scope.GetCboProcess();

    $scope.Save = function () {
        angular.copy($scope.operationActionNew, $scope.operationAction);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.operationActionNewForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.operationAction,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.operationActions.push(response.data.OperationAction);
                        $scope.operationActions = $filter('orderBy')($scope.operationActions, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.operationAction,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.operationActions[$scope.index] = $scope.operationAction;
                            $scope.operationActions = $filter('orderBy')($scope.operationActions, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.operationActionNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.operationActionNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.operationActions.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    }
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.operationAction = {};
        $scope.operationActionNew = {};
        $scope.operationActionNew.Sequence = seq;
        $scope.operationActionNew.Active = true;
    }
};