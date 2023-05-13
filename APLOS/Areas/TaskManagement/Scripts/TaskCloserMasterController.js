'use strict';
TaskCloserMasterController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'signalR'];
function TaskCloserMasterController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, signalR) {
    $rootScope.title = 'Task / Issue Closed Master';
    $scope.path = "TaskManagement/TaskCloserMaster/";
    $scope.Action = 'Save';
    $scope.ActionB = 'Save';
    $scope.OpenTaskList = [];

    document.getElementById("issuegrid").style.display = 'none';
    document.getElementById("taskgrid").style.display = 'none'

    $scope.ModelTemp = {
        Id: null,
        FromDate: null,
        TaskType:null,
        ToDate:null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetOpenTask = function () {
        
        if ($scope.ModelNew.TaskType == null) {
            throw ShowResult("Please Select Task / Issue Type");
        }
        if ($scope.ModelNew.TaskType == "Task") {
            
            document.getElementById("issuegrid").style.display = 'none';
            document.getElementById("taskgrid").style.display = 'block'
        }
        else if ($scope.ModelNew.TaskType == "Issue") {
            document.getElementById("taskgrid").style.display = 'none'
            document.getElementById("issuegrid").style.display = 'block';
        }
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
    //$scope.GetOpenTask();

    $scope.CloseOpenTask = function () {
        $scope.CheckedOpenTaskList = [];
        $scope.CheckedOpenIssueList = [];
        // #region Task block
        if ($scope.ModelNew.TaskType == "Task") {
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
        // #endregion Task block
        // #region Issue Block
        else if ($scope.ModelNew.TaskType == "Issue") {
           
                for (var i = 0; i < $scope.OpenTaskList.length; i++) {

                    if ($scope.OpenTaskList[i].isSelected) {
                        $scope.CheckedOpenIssueList.push($scope.OpenTaskList[i]);
                    }
                }

                $http({
                    method: 'POST',
                    url: $scope.path + 'CloseOpenIssue',
                    data: {
                        'chkIssueList': $scope.CheckedOpenIssueList,

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
           // #endregion Issue Block
       
    }

    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.XlsDailyAttendanceReport = function () {
        var dataList = [];
        var g = $("#Grid").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {

            dataList = $scope.OpenTaskList;
        }
        $scope.fileName = 'Open Task Detail.xlsx';
        $http({
            method: "POST",
            url: $scope.exportgriddataUrl,
            data: {

                'data': dataList,
                'reportFileName': $scope.fileName,
            },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    //$window.open($scope.downloadgriddataUrlPath + "?FileName=" + response.data.FileName);
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };

    $scope.XlsNotCloseIssueReport = function () {
        var dataList = [];
        var g = $("#gridIssue").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {

            dataList = $scope.OpenTaskList;
        }
        $scope.fileName = 'Open Issue Detail.xlsx';
        $http({
            method: "POST",
            url: $scope.exportgriddataUrl,
            data: {

                'data': dataList,
                'reportFileName': $scope.fileName,
            },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    //$window.open($scope.downloadgriddataUrlPath + "?FileName=" + response.data.FileName);
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };
}