'use strict';
taskManagerDashboardController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller','$window'];
function taskManagerDashboardController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, $window) {
    $rootScope.title = 'Taskmanager Dashboard';
    $scope.path = 'TaskManagement/TaskManagerDashboard/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $controller('taskDetailController', { $scope: $scope, $http: $http });


    $scope.filterString = { FromDate: '01-Jan-2019', ToDate: '09-Nov-2019', Status: 'All' };
    $scope.ShowFilterScreen = true;
    $scope.ModelList = [];
    $scope.Statistics = {};
    $scope.StatisticsToDo = {};
    $scope.StatisticsIssue = {};
    $scope.summaryRows = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, template: "#templateDataSummary", displayColumn: "TotalCreated", dataMember: "TotalCreated", format: "{0:N0}" },
            { summaryType: ej.Grid.SummaryType.Sum, template: "#templateDataSummary", displayColumn: "TotalOverDueUnread", dataMember: "TotalOverDueUnread", format: "{0:N0}" },
            { summaryType: ej.Grid.SummaryType.Sum, template: "#templateDataSummary", displayColumn: "TotalOverDueRead", dataMember: "TotalOverDueRead", format: "{0:N0}" },
            { summaryType: ej.Grid.SummaryType.Sum, template: "#templateDataSummary", displayColumn: "TodayTask", dataMember: "TodayTask", format: "{0:N0}" },
            { summaryType: ej.Grid.SummaryType.Sum, template: "#templateDataSummary", displayColumn: "TaskToClose", dataMember: "TaskToClose", format: "{0:N0}" }
        ],
        showCaptionSummary: true

    }];
    $scope.getTaskMmanagerDashboardList = function () {
        try {
            if (angular.isUndefinedOrNull($scope.filterString.FromDate) == true) {
                throw 'Select from date';
            }
            if (angular.isUndefinedOrNull($scope.filterString.ToDate) == true) {
                throw 'Select to date';
            }
            $http({

                dataType: 'JSON',
                method: 'POST',
                url: 'TaskManagement/TaskManagerDashboard/GetTaskManagerDashboardList',
                data: { fromDate: $scope.filterString.FromDate, ToDate: $scope.filterString.ToDate, TaskTypeGroup: $scope.filterString.Status },

            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult('error', 'failure');
                }
                else {
                    $scope.ModelList = response.data;
                    $scope.GetPieChart();
                    //$("#GridEdit").ejGrid("instance").refreshContent(true);
                }
            }, function errorCallback(response) {
                ShowResult('Failed', 'failure');
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    $scope.getMinDueDate = function () {
        $http({
            method: 'GET',
            url: 'TaskManagement/TaskManagerDashboard/GetMinDueDate',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.filterString.FromDate = response.data[0].FromDate;
                $scope.filterString.ToDate = response.data[0].ToDate;
                //$scope.filterComplete();
                //$scope.getTaskMmanagerDashboardList();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.getMinDueDate();



    $scope.filterComplete = function () {
        try {
            if (angular.isUndefinedOrNull($scope.filterString.FromDate) == true) {
                throw 'Select from date';
            }
            if (angular.isUndefinedOrNull($scope.filterString.ToDate) == true) {
                throw 'Select to date';
            }

            $scope.ShowFilterScreen = false;
            $http({
                method: 'POST',
                url: 'TaskManagement/TaskManagerDashboard/GetTaskStatistics',
                data: { fromDate: $scope.filterString.FromDate, ToDate: $scope.filterString.ToDate, TaskTypeGroup: $scope.filterString.Status },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult('error', 'failure');
                }
                else {
                    $scope.Statistics = [];
                    $scope.Statistics = response.data;

                    //for sparkline
                    for (var i = 0; i < $scope.Statistics.length; i++) {
                        $scope.Statistics[i].Sparkline = [];
                        $scope.Statistics[i].Sparkline.push($scope.Statistics[i].ToDo);
                        $scope.Statistics[i].Sparkline.push($scope.Statistics[i].Issue);
                        $scope.Statistics[i].Sparkline.push($scope.Statistics[i].TNA);
                    }
                    //$("#GridStatistics").ejGrid("instance").refreshContent(true);
                    //$scope.getTaskMmanagerDashboardList();
                }
            }, function errorCallback(response) {
                ShowResult('Failed', 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    $scope.lineData = [12];
    $scope.SelectedData = {};
    $scope.SelectedDataFieldsTemp = { flag: '', Caption: '', Total: 0, ToDo: 0, Issue: 0, TNA: 0 };
    $scope.SelectedDataFields = Object.assign({}, $scope.SelectedDataFieldsTemp);
    $scope.typeflag = '';
    $scope.showStat = function (args, flag) {
        $scope.SelectedData = args.data;
        $scope.SelectedDataFields = Object.assign({}, $scope.SelectedDataFieldsTemp);

        $scope.typeflag = flag;
        //if (flag == 'TotalCreated') {
        //    $scope.SelectedDataFields = {
        //        flag: 'TotalCreated', Caption: 'Total Created', Total: args.data.TotalCreated, ToDo: args.data.TotalCreatedToDo, Issue: args.data.TotalCreatedIssue, TNA: args.data.TotalCreatedTNA
        //    };
        //}
        //if (flag == 'TotalOverDueUnread') {
        //    $scope.SelectedDataFields = {
        //        flag: 'TotalOverDueUnread', Caption: 'Overdue (Unread)', Total: args.data.TotalOverDueUnread, ToDo: args.data.OverDueUnreadToDo, Issue: args.data.OverDueUnreadIssue, TNA: args.data.OverDueUnreadTNA
        //    };
        //}
        //if (flag == 'TotalOverDueRead') {
        //    $scope.SelectedDataFields = {
        //        flag: 'TotalOverDueRead', Caption: 'Overdue (Read)', Total: args.data.TotalOverDueRead, ToDo: args.data.OverDueReadToDo, Issue: args.data.OverDueReadIssue, TNA: args.data.OverDueReadTNA
        //    };
        //}
        //if (flag == 'TodayTask') {
        //    $scope.SelectedDataFields = {
        //        flag: 'TodayTask', Caption: 'Today Task', Total: args.data.TodayTask, ToDo: args.data.TodayTaskToDo, Issue: args.data.TodayTaskIssue, TNA: args.data.TodayTaskTNA
        //    };
        //}
        //if (flag == 'TaskToClose') {
        //    $scope.SelectedDataFields = {
        //        flag: 'TaskToClose', Caption: 'Task To Close', Total: args.data.TaskToClose, ToDo: args.data.TaskToCloseToDo, Issue: args.data.TaskToCloseIssue, TNA: args.data.TaskToCloseTNA
        //    };
        //}

        //var eDialog = $("#dialogSummaryStat").data("ejDialog");
        //eDialog.open();
        $scope.showTaskDetail("ALL");
    }
    $scope.showMainStat = function (args, tasktypegroup) {

        $http({
            method: 'POST',
            url: 'TaskManagement/TaskManagerDashboard/GetTaskDetailMain',
            data: { typeflag: args.Value, fromDate: $scope.filterString.FromDate, ToDate: $scope.filterString.ToDate, TaskTypeGroup: tasktypegroup, TaskTypeGroupFilter: $scope.filterString.Status },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {

                $scope.TaskDetail = response.data;


                $("#dialogDetailTaskMain").ejDialog("setTitle", "Task List (" + args.Particular + ") for -" + tasktypegroup);
                var eDialog = $("#dialogDetailTaskMain").data("ejDialog");
                eDialog.open();


            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });


    }

    $scope.TaskDetail = [];
    $scope.showTaskDetail = function (TaskGroupType) {
        try {

            $http({
                method: 'POST',
                url: 'TaskManagement/TaskManagerDashboard/GetTaskDetail',
                data: { typeflag: $scope.typeflag, Row: $scope.SelectedData, fromDate: $scope.filterString.FromDate, ToDate: $scope.filterString.ToDate, TaskTypeGroup: TaskGroupType },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult('error', 'failure');
                }
                else {

                    $scope.TaskDetail = response.data;
                    var eDialog = $("#dialogDetailTask").data("ejDialog");
                    eDialog.open();
                }
            }, function errorCallback(response) {
                ShowResult('Failed', 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    $scope.render = function (args) {
        try {


            for (var i = 0; i < $scope.Statistics.length; i++) {
                try {
                    $("#" + $scope.Statistics[i].Value).ejSparkline({
                        enableCanvasRendering: false,
                        dataSource: $scope.Statistics[i].Sparkline, type: "winloss",
                        rangeBandSettings: { endRange: 0 }, size: { height: 40, width: 80 }
                    });

                } catch (e) {

                }
            }

            try {
                if (args.type == "create")
                    this.getScrollObject().refresh();
            } catch (e) {

            }

        } catch (e) {

        }
    }

    $scope.PieChartMain = [{ x: '', y: 0 }];
    $scope.PieChartIssue = [{ x: '', y: 0 }];
    $scope.PieChartToDo = [{ x: '', y: 0 }];
    $scope.PieChartTNA = [{ x: '', y: 0 }];
    $scope.GetPieChart = function () {

        $http({
            method: 'POST',
            url: 'TaskManagement/TaskManagerDashboard/GetTaskStatisticsForPieChart',
            data: { fromDate: $scope.filterString.FromDate, ToDate: $scope.filterString.ToDate, TaskTypeGroup: $scope.filterString.Status },
            dataType: 'JSON'
        }).then(function successCallback(response) {


            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {

                $scope.PieChartMain = [];
                $scope.PieChartIssue = [];
                $scope.PieChartToDo = [];
                $scope.PieChartTNA = [];

                $scope.PieChartMain.push({ x: 'ToDo', y: response.data.Master[0].ToDo });
                $scope.PieChartMain.push({ x: 'Issue', y: response.data.Master[0].Issue });
                $scope.PieChartMain.push({ x: 'TNA', y: response.data.Master[0].TNA });


                $scope.PieChartIssue.push({ x: 'To Start', y: response.data.Tasks.Issue[0].ToStart });
                $scope.PieChartIssue.push({ x: 'In Progress', y: response.data.Tasks.Issue[0].InProgress });
                $scope.PieChartIssue.push({ x: 'To Close', y: response.data.Tasks.Issue[0].ToClose });
                $scope.PieChartIssue.push({ x: 'Closed', y: response.data.Tasks.Issue[0].Closed });


                $scope.PieChartToDo.push({ x: 'To Start', y: response.data.Tasks.ToDo[0].ToStart });
                $scope.PieChartToDo.push({ x: 'In Progress', y: response.data.Tasks.ToDo[0].InProgress });
                $scope.PieChartToDo.push({ x: 'To Close', y: response.data.Tasks.ToDo[0].ToClose });
                $scope.PieChartToDo.push({ x: 'Closed', y: response.data.Tasks.ToDo[0].Closed });


                $scope.PieChartTNA.push({ x: 'To Start', y: response.data.Tasks.TNA[0].ToStart });
                $scope.PieChartTNA.push({ x: 'In Progress', y: response.data.Tasks.TNA[0].InProgress });
                $scope.PieChartTNA.push({ x: 'To Close', y: response.data.Tasks.TNA[0].ToClose });
                $scope.PieChartTNA.push({ x: 'Closed', y: response.data.Tasks.TNA[0].Closed });


            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });


    }

    $scope.Print = function () {
        var data = $scope.TaskDetail;
        $scope.fileName = $filter("dateFiltering")(Date.now()) + "-Task List";
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'reportFileName': $scope.fileName,'data': data }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'dialogDetailTaskMain');
            }
            else {
                window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };


    ////#region task detail 
    //$scope.TaskManagerMasterId = null;
    //$scope.ToDoModel = {};
    //$scope.getTaskDetail = function (args) {

    //    $scope.TaskManagerMasterId = args.data.Id;
    //    $scope.getTask();
    //    $scope.getcommentlist();
    //    $scope.getSubTaskList();
    //    $scope.getFileList();

    //    var eDialog = $("#taskDetails").data("ejDialog");
    //    eDialog.open();

    //}
    //$scope.getTask = function () {
    //    $http({
    //        method: 'post', url: 'taskmanagement/tasklist/getTask',
    //        datatype: 'json',
    //        data: { ToDoId: $scope.TaskManagerMasterId }

    //    }).then(function successcallback(response) {
    //        if (response.data.error == true) {
    //            showresult('error', 'failure');
    //        }
    //        else {
    //            $scope.ToDoModel = response.data[0];

    //        }
    //    }, function errorcallback(response) {
    //        showresult('failed', 'failure');
    //    });
    //}
    //$scope.CommentText = '';
    //$scope.CommentsList = [];
    //$scope.getcommentlist = function () {

    //    $http({
    //        method: 'post', url: 'taskmanagement/tasklist/GetAllCommentsForDashboard', datatype: 'json',
    //        data: { todoid: $scope.TaskManagerMasterId }

    //    }).then(function successcallback(response) {
    //        if (response.data.error == true) {
    //            showresult('error', 'failure');
    //        }
    //        else {
    //            $scope.CommentsList = response.data;
    //        }
    //    }, function errorcallback(response) {
    //        showresult('failed', 'failure');
    //    });
    //}

    //$scope.SubTaskText = '';
    //$scope.SubTasksList = [];
    //$scope.getSubTaskList = function () {

    //    $http({
    //        method: 'POST', url: 'taskmanagement/tasklist/GetAllSubTasks', dataType: 'JSON',
    //        data: { ToDoId: $scope.TaskManagerMasterId }

    //    }).then(function successCallback(response) {
    //        if (response.data.Error == true) {
    //            ShowResult('error', 'failure');
    //        }
    //        else {
    //            $scope.SubTasksList = response.data;
    //        }
    //    }, function errorCallback(response) {
    //        ShowResult('Failed', 'failure');
    //    });
    //}

    //$scope.FilesList = [];
    //$scope.getFileList = function () {

    //    $http({
    //        method: 'POST', url: 'taskmanagement/tasklist/GetAllFiles', dataType: 'JSON',
    //        data: { ToDoId: $scope.TaskManagerMasterId }

    //    }).then(function successCallback(response) {
    //        if (response.data.Error == true) {
    //            ShowResult('error', 'failure');
    //        }
    //        else {
    //            $scope.FilesList = response.data;
    //        }
    //    }, function errorCallback(response) {
    //        ShowResult('Failed', 'failure');
    //    });
    //}

    //#endRegion task detail 
}