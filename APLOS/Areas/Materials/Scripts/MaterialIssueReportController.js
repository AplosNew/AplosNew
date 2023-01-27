'use strict';
MaterialIssueReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function MaterialIssueReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Material Issue Report';
    $scope.UtilityTransactionList = [];
    $scope.path = 'Materials/MaterialIssueReport/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.filters = [];
    $scope.getFiltersData = function () {
        try {
            $http({
                method: 'GET',
                url: 'Materials/MaterialIssueReport/getFiltersData?fromDate=' + $scope.FromDate + '&todate=' + $scope.ToDate,
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

    $scope.getUtilityTransactionData = function () {
       
        if (angular.isUndefinedOrNull($scope.FromDate) || angular.isUndefinedOrNull($scope.ToDate)) {
            ShowResult("Please Select the Date Range!", 'failure');
            throw ('Invalid Request!!');
        }

        $http({
            method: 'POST',
            url: $scope.path + 'getUtilityTransactionData',
            data: {'ToDate': $scope.ToDate, 'FromDate': $scope.FromDate },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.UtilityTransactionList = resp.data;
        });

    }
    


    $scope.UtilityTransactionReport = function () {
        if (angular.isUndefinedOrNull($scope.FromDate) || angular.isUndefinedOrNull($scope.ToDate)) {
            ShowResult("Please Select the Date Range!", 'failure');
            throw ('Invalid Request!!');
        }

        //$scope.filterComplete();
        $scope.fileName = "UtilityTransactionReport.xlsx";

        $http({
            method: 'POST',
            url: $scope.path + "GetUtilityTransactionReport",
            data: {'ToDate': $scope.ToDate, 'FromDate': $scope.FromDate},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
}