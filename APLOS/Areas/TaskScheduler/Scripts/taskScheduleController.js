'use strict';
taskScheduleController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function taskScheduleController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Task Scheduler";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.TaskSchedulerList = [];
    $scope.path = 'TaskScheduler/TaskSchedule/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    //baseService.init($scope.getListUrl);

    //  $scope.taskScheduler = function () {

    //  }


    //  $scope.getData = function (pageno) {
    //      baseService.pagination(pageno)
    //          .then(function (result) {
    //              $scope.TaskSchedulerList = result.Rows;
    //          }, function () {
    //              ShowResult(commonMessage.NetworkError, 'failure');
    //          }).finally(function () {
    //          });
    //  };
    ////  $scope.getData();

    //  $scope.taskSchedulerNew = Object.assign({}, $scope.taskScheduler);


    //  $scope.Get = function (index) {
    //      $scope.index = index;
    //      $scope.taskScheduler = $scope.TaskSchedulerList[$scope.index];
    //      $scope.taskSchedulerNew = Object.assign({}, $scope.taskScheduler);
    //      $scope.Action = 'Update';
    //      if (!$rootScope.isCollapsed) {
    //          $rootScope.toggle();
    //      }
    //  };

    //  $scope.Save = function () {
    //      angular.copy($scope.taskSchedulerNew, $scope.taskScheduler);
    //      $scope.$broadcast('show-errors-check-validity');
    //      if ($scope.taskSchedulerNewForm.$valid) {
    //          if ($scope.Action === "Save") {
    //              $http({
    //                  method: 'POST'
    //                  , url: $scope.saveUrl
    //                  , data: $scope.taskScheduler
    //                  , dataType: 'JSON'
    //              }).then(function successCallback(response) {
    //                  if (response.data.Error === true) {
    //                      ShowResult(response.data.Message, 'failure');
    //                  }
    //                  else {
    //                      ShowResult(response.data.Message, 'success');
    //                      $scope.TaskSchedulerList.push(response.data.taskScheduler);
    //                      $scope.TaskSchedulerList = $filter('orderBy')($scope.TaskSchedulerList, 'Sequence');
    //                      baseService.paginationAdd();
    //                      ClearFields(response.data.Sequence);
    //                  }
    //              }), function errorCallBack(response) {
    //                  ShowResult(response.data.Message, 'failure');
    //              };
    //          }
    //          else if ($scope.Action === "Update") {
    //              $http({
    //                  method: 'POST'
    //                  , url: $scope.updateUrl
    //                  , data: $scope.taskScheduler
    //                  , dataType: 'JSON'
    //              }).then(function successCallback(response) {
    //                  if (response.data.Error === true) {
    //                      ShowResult(response.data.Message, 'failure');
    //                  }
    //                  else {
    //                      ShowResult(response.data.Message, 'success');
    //                      if ($scope.index > -1) {
    //                          $scope.TaskSchedulerList[$scope.index] = $scope.taskScheduler;
    //                          $scope.TaskSchedulerList = $filter('orderBy')($scope.TaskSchedulerList, 'Sequence');
    //                      }
    //                      ClearFields(response.data.Sequence);
    //                  }
    //              }, function errorCallBack(response) {
    //                  ShowResult(response.data.Message, 'failure');
    //              });
    //          }
    //      }
    //  };

    //  $scope.Delete = function () {
    //      if (!baseService.isUndefinedOrNull($scope.taskSchedulerNew.Id)) {
    //          $http({
    //              method: 'POST'
    //              , url: $scope.deleteUrl + $scope.taskSchedulerNew.Id
    //              , dataType: 'JSON'
    //          }).then(function successCallback(response) {
    //              if (response.data.Error === true) {
    //                  ShowResult(response.data.Message, 'failure');
    //              }
    //              else {
    //                  ShowResult(response.data.Message, 'success');
    //                  $scope.TaskSchedulerList.splice($scope.index, 1);
    //                  baseService.paginationRemove();
    //                  ClearFields(response.data.Sequence);
    //              }
    //              function errorCallBack(response) {
    //                  ShowResult(response.data.Message, 'failure');
    //              }
    //          });
    //      }
    //  };

    //  $scope.Clear = function () {
    //      ClearFields($scope.GetSequence());
    //      return true;
    //  };

    //  function ClearFields(seq) {
    //      $scope.Action = "Save";
    //      $scope.taskScheduler = {};
    //      $scope.taskSchedulerNew = { Sequence: seq, Active: true };
    //  }
}