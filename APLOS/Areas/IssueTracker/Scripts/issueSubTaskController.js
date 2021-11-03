'use strict';
issueSubTaskController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function issueSubTaskController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'issueSubTask';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.issueSubTasks = [];
    $scope.path = 'issueTracker/issueSubTask/';
    $scope.getListUrl = $scope.path + 'getlist';
    //$scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
   // $controller("employeeBaseController", { $scope: $scope, $http: $http });

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.issueSubTasks = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.issueSubTask = {
        Id: null,
        IssueTransactionId: null,
        RequiredDate: null,
        TaskDetail: null,
        IsDone: null,
        ResponsiblePersonId: null,
        Remarks: null
    };

    $scope.issueSubTaskNew = Object.assign({}, $scope.issueSubTask);
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.issueSubTask = $scope.issueSubTasks[$scope.index];
        $scope.issueSubTaskNew = Object.assign({}, $scope.issueSubTask);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.issueSubTaskNew, $scope.issueSubTask);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.issueSubTaskNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.issueSubTask,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.issueSubTasks.push(response.data.IssueSubTask);
                        
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
                    data: $scope.issueSubTask,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.issueSubTasks[$scope.index] = $scope.issueSubTask;
                            //$scope.issueSubTasks = $filter('orderBy')($scope.issueSubTasks, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.issueSubTaskNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.issueSubTaskNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.issueSubTasks.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.issueSubTask = {};
        $scope.issueSubTaskNew = {};
        $scope.taskTypeNew.Active = true;
        //$scope.issueSubTaskNew.Sequence = seq;
    }
}