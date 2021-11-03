'use strict';
TNAReportsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function TNAReportsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'TNA Reports';
    $scope.path = 'TaskManagement/TNAReports/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $controller('taskDetailController', { $scope: $scope, $http: $http });

    var _currentDate = new Date();
    var numberOfDaysToAdd = 90;
    _currentDate.setDate(_currentDate.getDate() + numberOfDaysToAdd);
    $scope.ShowFilterScreen = true;
    $scope.FilterModel = { ReportLevel: 'ALL', FromDate: new Date(), ToDate: _currentDate, ActiveStatus: 'All', DateSelection: "ASON" };
    $scope.QueryString = [];
    $scope.ElasticSearchData = [];

    $scope.filterComplete = function () {
        $scope.ShowFilterScreen = false;
        $scope.getData();
    }
    $scope.showLegends = function () {
        $("#dialogLegends").data("ejDialog").open();
    }
    $scope.taskcolorchange = function (args) {
        try {

            //today's task
            var DueDate = new Date(args.data.DueDate);
            if (DueDate.getDate() == new Date().getDate()
                && DueDate.getMonth() == new Date().getMonth()
                && DueDate.getFullYear() == new Date().getFullYear()) {
                args.cell.bgColor = "#E6F0FF";
            }

            //overdue
            if (new Date(DueDate.getFullYear(), DueDate.getMonth(), DueDate.getDate()) < new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate())) {
                args.cell.bgColor = "#FFF4E6";
            }

            //future
            if (new Date(DueDate.getFullYear(), DueDate.getMonth(), DueDate.getDate()) > new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate())) {
                args.cell.bgColor = "#F5FFE6";
            }

            if (args.data.CurrentStatus == "Closed") {
                try {


                    var ClosingDate = new Date(args.data.ClosingDate);
                    //late closed
                    var _temDueDate = new Date(DueDate.getFullYear(), DueDate.getMonth(), DueDate.getDate());
                    var _temClosingDate = new Date(ClosingDate.getFullYear(), ClosingDate.getMonth(), ClosingDate.getDate());

                    if (_temDueDate < _temClosingDate) {
                        args.cell.bgColor = "#52B3D9";
                    }
                    if (_temDueDate >= _temClosingDate) {
                        args.cell.bgColor = "#2ECC71";
                    }
                } catch (e) {

                }

            }

        } catch (e) {

        }
    }
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { filterSettings: $scope.FilterModel },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ElasticSearchData = response.data;
            $scope.filtergridonload();
        });
    }



    $scope.ResultData = [];
    $scope.GetResult = function () {
        try {
            var gridObj = $("#GridResultTNA").data("ejGrid");
            if (gridObj !== undefined && typeof gridObj === 'object' && typeof gridObj.destroy === 'function') gridObj.destroy();
        } catch (e) {

        }

        try {
            if (angular.isUndefinedOrNull($scope.FilterModel.FromDate))
                throw 'Please enter From Date';


            if (angular.isUndefinedOrNull($scope.FilterModel.ToDate))
                throw 'Please enter To Date';
        } catch (e) {
            ShowResult(e, 'failure');
            return;
        }

        $http({
            method: 'POST',
            url: $scope.path + "GetResult",
            data: { Filter: $scope.FilterModel, FilterFields: $scope.QueryString },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            //var ColumnList = [{ field: 'TNAType', width: 70, headerText: "Type" }, { field: 'Buyer', width: 120, headerText: "Buyer" }, { field: 'MasterOrderId', width: 100, headerText: "Order ID" },
            //{ field: 'StyleNo', width: 100, headerText: "Item" }, { field: 'SONo', width: 100, headerText: "SO" }, { field: 'PRNo', width: 100, headerText: "Prod. Ord#" }];

            //for (var i = 0; i < response.data.COLUMNS.length; i++) {
            //    ColumnList.push({ field: response.data.COLUMNS[i].Id, width: 130, headerText: response.data.COLUMNS[i].UserDefineTask });
            //}

            //$("#GridResultTNA").ejGrid({
            //    dataSource: $scope.ResultData,
            //    minWidth: 450, minHeight: 400,
            //    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
            //    filterSettings: { filterType: "excel" },
            //    columns: ColumnList
            //});
            for (var i = 0; i < response.data.MAINDATA.length; i++) {
                
                try { response.data.MAINDATA[i].DueDate = new Date(response.data.MAINDATA[i].DueDate); } catch (e) { }
                try { response.data.MAINDATA[i].CommitmentDate = new Date(response.data.MAINDATA[i].CommitmentDate); } catch (e) { }
                try {response.data.MAINDATA[i].ClosingDate = new Date(response.data.MAINDATA[i].ClosingDate);} catch (e) {}
                try { response.data.MAINDATA[i].TempStartDate = new Date(response.data.MAINDATA[i].TempStartDate); } catch (e) { }
                try { response.data.MAINDATA[i].TempEndDate = new Date(response.data.MAINDATA[i].TempEndDate); } catch (e) { }

            }
            $scope.ResultData = response.data.MAINDATA;

        });
    }
    $scope.ExportToExcel = function () {


        try {
            if (angular.isUndefinedOrNull($scope.FilterModel.FromDate))
                throw 'Please enter From Date';


            if (angular.isUndefinedOrNull($scope.FilterModel.ToDate))
                throw 'Please enter To Date';
        } catch (e) {
            ShowResult(e, 'failure');
            return;
        }

        $http({
            method: 'POST',
            url: $scope.path + "GetExcelReport",
            data: { Filter: $scope.FilterModel, FilterFields: $scope.QueryString },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == false) {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
            else {
                ShowResult(response.data.Message, 'failure');
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }


    $scope.GridFilter = function (args) {
        if (args.requestType == "filtering") {
            $scope.ApplyFilter();
        }
    }
    $scope.ApplyFilter = function () {
        $scope.HideGrid = true;

        $scope.QueryString = [];

        var gridObj = $("#GridElasticSearchTNA").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        if (angular.isUndefinedOrNull(filteredRecords) == false) {
            if (filteredRecords.length > 0) {
                $scope.QueryString.push({ "Key": "BuyerId", "Value": getString(filteredRecords, "BuyerId") });
                $scope.QueryString.push({ "Key": "MasterOrderId", "Value": getString(filteredRecords, "MasterOrderId") });
                $scope.QueryString.push({ "Key": "StyleNo", "Value": getString(filteredRecords, "StyleNo") });
                $scope.QueryString.push({ "Key": "SONo", "Value": getString(filteredRecords, "SONo") });
                $scope.QueryString.push({ "Key": "PRNo", "Value": getString(filteredRecords, "PRNo") });

                $scope.QueryString.push({ "Key": "ProcessId", "Value": getString(filteredRecords, "ProcessId") });
                $scope.QueryString.push({ "Key": "DepartmentId", "Value": getString(filteredRecords, "DepartmentId") });
                $scope.QueryString.push({ "Key": "AssignToId", "Value": getString(filteredRecords, "AssignToId") });
                $scope.QueryString.push({ "Key": "AssignById", "Value": getString(filteredRecords, "AssignById") });
                $scope.QueryString.push({ "Key": "TaskCategoryId", "Value": getString(filteredRecords, "TaskCategoryId") });
                $scope.QueryString.push({ "Key": "TaskSubCategoryId", "Value": getString(filteredRecords, "TaskSubCategoryId") });

            }
            else {

            }
        }
        //$scope.ClearFilter();
    }
    $scope.ClearFilter = function () {
        $scope.HideGrid = true;
        var gridObj = $("#GridElasticSearchTNA").data("ejGrid");
        gridObj.clearFiltering();

        var gridObj = $("#GridEdit").data("ejGrid");
        gridObj.clearFiltering();

    }
    $scope.filtergridonload = function () {
        try {
            $("#GridElasticSearchTNA").children('.e-pager.e-js.e-pager').hide();
            $("#GridElasticSearchTNA").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#GridElasticSearchTNA").children('.e-gridcontent').hide();
        } catch (e) {

        }

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
    $scope.PrintExcel = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetExcelTasksReport',
            data: { Filter: $scope.FilterModel, FilterFields: $scope.QueryString },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });


    }
    $scope.PrintExcelException = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetExcelTasksReportException',
            data: { Filter: $scope.FilterModel, FilterFields: $scope.QueryString },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });


    }
    $scope.HideGrid = true;
    $scope.ModelList = [];
    $scope.GetTaskForGrid = function () {

        var gridObj = $("#GridEdit").data("ejGrid");
        gridObj.clearFiltering();

        $scope.HideGrid = false;
        $http({
            method: 'POST',
            url: $scope.path + 'GetTaskListResult',
            data: { Filter: $scope.FilterModel, FilterFields: $scope.QueryString },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.ModelList = response.data.MAINDATA;
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });


    }
}