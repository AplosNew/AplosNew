'use strict';
TaskManagementReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function TaskManagementReportController(commonMessage,  $scope, $rootScope, baseService,  $http, $filter) {
    $scope.title = 'Task Management Report';
    $scope.UtilityTransactionList = [];
    $scope.path = 'TaskManagement/TaskManagementReport/';
    $scope.downloadgriddataUrlPath = 'Materials/UtilityTransactionReport/DownloadUsingFullPath';
    //baseService.init($scope.getListUrl);


    $scope.ToDate = null;
    $scope.FromDate = null;

    $scope.Today = new Date();
    $scope.PreviousMonth = new Date().setDate(new Date().getDate() - 31);
    $scope.NextMonth = new Date().setDate(new Date().getDate() + 31);
    $scope.FromDate = $filter("dateFiltering")($scope.PreviousMonth);
    $scope.ToDate = $filter("dateFiltering")($scope.NextMonth);


    $scope.filters = [];
    $scope.getFiltersData = function () {
        $http({
            method: 'GET',
            url: 'TaskManagement/TaskManagementReport/getFiltersData?fromDate=' + $scope.FromDate + '&todate=' + $scope.ToDate,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var columnList = [
                { field: 'DesignationGroup', width: 20, headerText: "Designation Group", type: "string" },
                { field: 'TYPE', width: 20, headerText: "Type", type: "string" },
                { field: 'Department', width: 20, headerText: "Department", type: "string" },
                { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                { field: 'UserReportGroup', width: 20, headerText: "User Group2", type: "string" },

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
        parameters.push({ "Key": "TYPE", "Value": getString(fl, "TYPE") });
        parameters.push({ "Key": "UserReportGroup", "Value": getString(fl, "UserReportGroup") });

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


    $scope.GetTaskReport = function () {
        $scope.filterComplete();
        $scope.fileName = "UtilityTransactionReport.xlsx";

        $http({
            method: 'POST',
            url: $scope.path + "GetUtilityTransactionReport",
            data: { 'ToDate': $scope.ToDate, 'FromDate': $scope.FromDate },
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
    }
}