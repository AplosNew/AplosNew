'use strict';
PurchaseConfirmationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window'];
function PurchaseConfirmationController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $scope.title = 'Purchase Confirmation';
    $scope.TaskManagementDataList = [];
    $scope.path = 'Products/InventoryReceiveAddition/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    $scope.ToDate = null;
    $scope.FromDate = null;
    $scope.model = { State: 'EmployeeWise', Status: 'All', Task: 'WithTask' };

    $scope.ChangeState = function () {
        $scope.TaskManagementDataList = [];
    }

    $scope.Today = new Date();
    $scope.PreviousMonth = new Date().setDate(new Date().getDate() - 31);
    $scope.NextMonth = new Date().setDate(new Date().getDate()-1);
    $scope.FromDate = $filter("dateFiltering")($scope.PreviousMonth);
    $scope.ToDate = $filter("dateFiltering")($scope.NextMonth);

    $scope.filters = [];
    $scope.getFiltersData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "From Date is required.";
            }
            else if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "To Date is required.";
            }
            else if (new Date($scope.FromDate) > new Date($scope.ToDate)) {
                throw "From date must be below or equal to To Date";
            }
            else if (new Date($scope.ToDate) < new Date($scope.FromDate)) {
                throw "To date must be above or equal to From Date.";
            }


            $http({
                method: 'GET',
                url: 'TaskManagement/TaskManagementReport/getFiltersData?fromDate=' + $scope.FromDate + '&todate=' + $scope.ToDate,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filters = response.data;
                var columnList = [
                    { field: 'DesignationGroup', width: 20, headerText: "Designation Group", type: "string" },
                    { field: 'TaskCreatedBy', width: 20, headerText: "Task CreatedBy", type: "string" },
                    { field: 'Department', width: 20, headerText: "Department", type: "string" },
                    { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                    { field: 'UserReportGroup', width: 20, headerText: "User Group2", type: "string" }

                ];
                $("#filters").ejGrid({
                    dataSource: $scope.filters,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: columnList
                });

                var gridObj = $("#filters").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                $("#filters").children('.e-pager.e-js.e-pager').hide();
                $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#filters").children('.e-gridcontent').hide();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "DesignationGroupId", "Value": getString(fl, "DesignationGroupId") });
        parameters.push({ "Key": "DepartmentId", "Value": getString(fl, "DepartmentId") });
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "UserReportGroup", "Value": getString(fl, "UserReportGroup") });
        parameters.push({ "Key": "TaskCreatedBy", "Value": getString(fl, "TaskCreatedBy") });

        $scope.parameters = parameters;
    }

    var getString = function (data, column) {
        var string = "''";
        var collection = [];

        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }

    $scope.GetTaskManagementReport = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "From Date is required.";
            }
            else if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "To Date is required.";
            }
            else if (new Date($scope.FromDate) > new Date($scope.ToDate)) {
                throw "From date must be below or equal to To Date";
            }
            else if (new Date($scope.ToDate) < new Date($scope.FromDate)) {
                throw "To date must be above or equal to From Date.";
            }

            $scope.filterComplete();
            $scope.fileName = "TaskManagementReport.xlsx";
            if ($scope.model.State == "EmployeeWise") {
                $scope.fileName = "TaskManagementReportEmployeeWise.xlsx";
            }
            else if ($scope.model.State == "DepartmentWise") {
                $scope.fileName = "TaskManagementReportDepartmentWise.xlsx";
            }
            else {
                $scope.fileName = "TaskManagementReportDesignationGroupWise.xlsx";
            }
            $http({
                method: 'POST',
                url: $scope.path + "GetTaskManagementReport",
                data: { 'parameters': $scope.parameters, 'fromDate': $scope.FromDate, 'todate': $scope.ToDate, 'model': $scope.model },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.GetTaskManagementData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "From Date is required.";
            }
            else if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "To Date is required.";
            }
            else if (new Date($scope.FromDate) > new Date($scope.ToDate)) {
                throw "From date must be below or equal to To Date";
            }
            else if (new Date($scope.ToDate) < new Date($scope.FromDate)) {
                throw "To date must be above or equal to From Date.";
            }

            $scope.TaskManagementDataList = [];
            $scope.filterComplete();

            $http({
                method: 'POST',
                url: $scope.path + "GetTaskManagementData",
                data: { 'parameters': $scope.parameters, 'fromDate': $scope.FromDate, 'todate': $scope.ToDate, 'model': $scope.model },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.TaskManagementDataList = response.data;
                    var totalTask = $filter("sumByKey")($filter("filter")($scope.TaskManagementDataList), "CreatedTask");
                    var totalTaskDue = $filter("sumByKey")($filter("filter")($scope.TaskManagementDataList), "TaskDue");
                    for (var i = 0; i < $scope.TaskManagementDataList.length; i++) {
                        if ($scope.TaskManagementDataList[i].CreatedTask == 0) {
                            $scope.TaskManagementDataList[i].TotalStoryPoint = 0;
                        }
                        $scope.TaskManagementDataList[i].OfTotalTask = Math.ceil(($scope.TaskManagementDataList[i].CreatedTask / totalTask) * 100);
                        if (isNaN($scope.TaskManagementDataList[i].OfTotalTask)) {
                            $scope.TaskManagementDataList[i].OfTotalTask = 0;
                        }
                        $scope.TaskManagementDataList[i].PerTaskDue = Math.ceil(($scope.TaskManagementDataList[i].TaskDue / totalTaskDue) * 100);
                        $scope.TaskManagementDataList[i].OverdueTask = $scope.TaskManagementDataList[i].TaskDue - $scope.TaskManagementDataList[i].OnTimeTask - $scope.TaskManagementDataList[i].LateTask;
                        if (isNaN($scope.TaskManagementDataList[i].OverdueTask)) {
                            $scope.TaskManagementDataList[i].OverdueTask = 0;
                        }
                        if (isNaN($scope.TaskManagementDataList[i].PerTaskDue)) {
                            $scope.TaskManagementDataList[i].PerTaskDue = 0;
                        }
                        $scope.TaskManagementDataList[i].Performance = ((($scope.TaskManagementDataList[i].OnTimeTask * 2) + $scope.TaskManagementDataList[i].LateTask * 1) * $scope.TaskManagementDataList[i].PerTaskDue) - $scope.TaskManagementDataList[i].UnRead;//formula
                    }
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

}