'use strict';
taskDetailController.$inject = ['$scope', '$http', 'baseService'];
function taskDetailController($scope, $http, baseService) {

    //#region task detail 
    $scope.TaskManagerMasterId = null;
    $scope.ToDoModel = {};
    $scope.getTaskDetail = function (args) {

        try {

            $scope.TaskManagerMasterId = args.data.Id;
            if (baseService.isUndefinedOrNull($scope.TaskManagerMasterId))
                $scope.TaskManagerMasterId = args.data.TaskMasterId;

            if (baseService.isUndefinedOrNull($scope.TaskManagerMasterId))
                $scope.TaskManagerMasterId = args;
        } catch (e) {

        }
        $scope.getTask();
        $scope.getcommentlist();
        $scope.getSubTaskList();
        $scope.getFileList();

        var eDialog = $("#taskDetails").data("ejDialog");
        eDialog.open();

    }
    $scope.getTask = function () {
        $http({
            method: 'post', url: 'taskmanagement/tasklist/getTask',
            datatype: 'json',
            data: { ToDoId: $scope.TaskManagerMasterId }

        }).then(function successcallback(response) {
            if (response.data.error == true) {
                showresult('error', 'failure');
            }
            else {
                $scope.ToDoModel = response.data[0];

            }
        }, function errorcallback(response) {
            showresult('failed', 'failure');
        });
    }
    $scope.CommentText = '';
    $scope.CommentsList = [];
    $scope.getcommentlist = function () {

        $http({
            method: 'post', url: 'taskmanagement/tasklist/GetAllCommentsForDashboard', datatype: 'json',
            data: { todoid: $scope.TaskManagerMasterId }

        }).then(function successcallback(response) {
            if (response.data.error == true) {
                showresult('error', 'failure');
            }
            else {
                $scope.CommentsList = response.data;
            }
        }, function errorcallback(response) {
            showresult('failed', 'failure');
        });
    }

    $scope.SubTaskText = '';
    $scope.SubTasksList = [];
    $scope.getSubTaskList = function () {

        $http({
            method: 'POST', url: 'taskmanagement/tasklist/GetAllSubTasks', dataType: 'JSON',
            data: { ToDoId: $scope.TaskManagerMasterId }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.SubTasksList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }

    $scope.FilesList = [];
    $scope.getFileList = function () {

        $http({
            method: 'POST', url: 'taskmanagement/tasklist/GetAllFiles', dataType: 'JSON',
            data: { ToDoId: $scope.TaskManagerMasterId }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.FilesList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }

    //#endRegion task detail 
}