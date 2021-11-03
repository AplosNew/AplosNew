'use strict';
SubProcessController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SubProcessController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'SubProcess';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.subProcesses = [];
    $scope.path = 'Processes/subprocess/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence?processId=';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.subProcess = {
        Id: null,
        CompanyGroupId: null,
        ProcessId: null,
        SubProcessCategoryId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Remarks: null,
        Active: true
    };
    $scope.subProcessNew = angular.copy($scope.subProcess);

    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        $rootScope.parameters.processId = $scope.subProcessNew.ProcessId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.subProcesses = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $rootScope.searchBySubProcessList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        },
        {
            'name': 'SubProcess Category',
            'value': 'SubProcessCategoryName'
        }
    ];
    $scope.processList = [];
    $http({
        method: 'GET',
        url: 'Processes/process/getcbo'
    }).then(function successCallback(response) {
        $scope.processList = response.data;
    });
    $scope.subProcessCtegoryList = [];
    $http({
        method: 'GET',
        url: 'Processes/subprocesscategory/getcbo'
    }).then(function successCallback(response) {
        $scope.subProcessCtegoryList = response.data;
    });
    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl + $scope.subProcessNew.ProcessId)
            .then(function (response) {
                $scope.subProcessNew.Sequence = response.data;
            });
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        angular.copy($scope.subProcesses[$scope.index], $scope.subProcess);
        angular.copy($scope.subProcess, $scope.subProcessNew);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.subProcessNewForm.$valid) {
            angular.copy($scope.subProcessNew, $scope.subProcess);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.subProcess,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.subProcess,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.subProcessNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.subProcessNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.subProcesses.splice($scope.index, 1);
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
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.subProcess = {};
        $scope.subProcessNew = { ProcessId: $scope.subProcessNew.ProcessId };
        $scope.subProcessNew.Sequence = seq;
        $scope.subProcessNew.Active = true;
    }
}
