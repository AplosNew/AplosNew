'use strict';
taskManagerMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function taskManagerMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'taskManagerMaster';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.taskManagerMasters = [];
    $scope.path = 'taskmanagement/taskManagerMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.taskManagerMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.taskManagerMaster = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.taskManagerMasterNew = Object.assign({}, $scope.taskManagerMaster);
    
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.taskManagerMaster = $scope.taskManagerMasters[$scope.index];
        $scope.taskManagerMasterNew = Object.assign({}, $scope.taskManagerMaster);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.taskManagerMasterNew, $scope.taskManagerMaster);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.taskManagerMasterNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.taskManagerMaster,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.taskManagerMasters.push(response.data.TaskType);
                        $scope.taskManagerMasters = $filter('orderBy')($scope.taskManagerMasters, 'Sequence');
                        baseService.paginationAdd();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.taskManagerMaster,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.taskManagerMasters[$scope.index] = $scope.taskManagerMaster;
                            $scope.taskManagerMasters = $filter('orderBy')($scope.taskManagerMasters, 'Sequence');
                        }
                        $scope.Clear();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.taskManagerMasterNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.taskManagerMasterNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.taskManagerMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
 
    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.taskManagerMaster = {};
        $scope.taskManagerMasterNew = {};
        $scope.taskTypeNew.Active = true;
        $scope.taskManagerMasterNew.Sequence = seq;
    }
}