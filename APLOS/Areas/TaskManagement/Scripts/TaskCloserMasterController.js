'use strict';
TaskCloserMasterController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'signalR'];
function TaskCloserMasterController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, signalR) {
    $rootScope.title = 'Task Closed Master';
    $scope.path = "TaskManagement/TaskCloserMaster/";
    $scope.Action = 'Save';
    $scope.OpenTaskList = [];

    $scope.ModelTemp = {
        Id: null,
        FromDate: null,
        TaskType:null,
        ToDate:null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetOpenTask = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetOpenTask",
            data: {
                'fromdate': $scope.ModelNew.FromDate,
                'todate': $scope.ModelNew.ToDate,
                'taskType': $scope.ModelNew.TaskType,

},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OpenTaskList = response.data;
           
        });
    }
    $scope.GetOpenTask();

    $scope.CloseOpenTask = function () {
        $scope.CheckedOpenTaskList = [];
        
            for (var i = 0; i < $scope.OpenTaskList.length; i++) {

                if ($scope.OpenTaskList[i].isSelected) {
                    $scope.CheckedOpenTaskList.push($scope.OpenTaskList[i]);
                }
            }
           
            $http({
                method: 'POST',
                url: $scope.path + 'CloseOpenTask',
                data: {
                    'chkBgtList': $scope.CheckedOpenTaskList,
                    
                },
                dataType: 'JSON',
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetOpenTask();
                }
            });
       
        
    }
}