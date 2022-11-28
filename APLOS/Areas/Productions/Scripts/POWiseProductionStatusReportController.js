'use strict';
POWiseProductionStatusReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window'];
function POWiseProductionStatusReportController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $scope.title = 'PO Wise Production Status Report';
    $scope.path = 'Productions/POWiseProductionStatusReport/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.ProductionDataSumReportList = [];
    $scope.ProductionDataReportList = [];
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //The Filters 
    $scope.filters = [];
    $scope.sumfilters = [];
    $scope.loadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            $scope.sumfilters = response.data;
            var columnList = [
                { field: 'Customer', width: 20, headerText: "Customer", type: "string" },
                { field: 'ProductCode', width: 20, headerText: "ProductCode", type: "string" },
                { field: 'ProductionOrderId', width: 20, headerText: "PONo", type: "string" },
                { field: 'LotNumber', width: 20, headerText: "LotNumber", type: "string" },
                { field: 'ProductionStatus', width: 20, headerText: "PO Status", type: "string" },
                { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                { field: 'ResponsiblePerson', width: 20, headerText: "Responsible Person", type: "string" },

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

            $("#summaryfilters").ejGrid({
                dataSource: $scope.sumfilters,
                minWidth: 450, minHeight: 400,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                filterSettings: { filterType: "excel" },
                columns: columnList
            });

            var gridSumObj = $("#summaryfilters").data("ejGrid");
            gridSumObj.refreshContent(true);
            gridSumObj.refreshTemplate();
            $("#summaryfilters").children('.e-pager.e-js.e-pager').hide();
            $("#summaryfilters").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#summaryfilters").children('.e-gridcontent').hide();
        });
    }
    $scope.loadfilters();

    // THe Generate Filters
    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "CustomerId", "Value": getString(fl, "CustomerId") });
        parameters.push({ "Key": "ResponsiblePersonId", "Value": getString(fl, "ResponsiblePersonId") });
        parameters.push({ "Key": "ProductionStatusId", "Value": getString(fl, "ProductionStatusId") });
        parameters.push({ "Key": "ProductLibraryId", "Value": getString(fl, "ProductLibraryId") });
        parameters.push({ "Key": "LotNumber", "Value": getString(fl, "LotNumber") });

        $scope.parameters = parameters;

    }


    var getString = function (data, column) {
        var string = "''";
        var collection = [];

        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                /* var replace = data[i][column].replace(",", "','");*/
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }

    //Destroy The Grid Before ReBuilding And Clearing of the Filters
    $scope.clearFilters = function () {

        var gridObj = $("#filters").data("ejGrid");
        gridObj.clearFiltering();
    }

    $scope.refreshPage = function (e) {
        if (e.requestType == "paging") {
            var gridObj = $("#slabGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        }
        var k = 100;
    }
    $scope.ProductionDataReportList = [];
    $scope.ViewData = function () {
        $scope.filterComplete();
      //  $scope.fileName = "ProductionDataReport.xlsx";

        $http({
            method: 'POST',
            url: $scope.path + "GetViewData",
            data: { 'parameters': $scope.parameters },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.ProductionDataReportList = response.data;
                console.log($scope.ProductionDataReportList);
                //$window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.ProductionData = function () {
        var dataList = [];
        var g = $("#GridEmp").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ProductionDataReportList;
        }
        console.log('dataList',dataList);
        $scope.fileName = "ProductionDataReport.xlsx";

        $http({
            method: 'POST',
            url: $scope.path + "ProductionDataXls",
            //data: { 'parameters': $scope.parameters },
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //$scope.ProductionDataReportList = response.data;
                $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                //$rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    //$scope.ProductionDataReport = function () {
    //    var dataList = [];
    //    var g = $("#GridEmp").data("ejGrid");
    //    dataList = g.getFilteredRecords();

    //    if (dataList.length == 0) {
    //        dataList = $scope.ProductionDataReportList;
    //    }

    //    $scope.fileName = "Production Data Report";

    //    $http({
    //        method: 'POST',
    //        url: $scope.exportgriddataUrlUpd,
    //        data: {'reportFileName': $scope.fileName,'data': dataList},
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error == true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
    //        }
    //    }, function errorCallback(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    });
    //}

    $scope.sumparameters = [];
    $scope.SumfilterComplete = function () {

        var g = $("#summaryfilters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.sumfilters;
        }


        var sumparameters = [];
        sumparameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        sumparameters.push({ "Key": "CustomerId", "Value": getString(fl, "CustomerId") });
        sumparameters.push({ "Key": "ResponsiblePersonId", "Value": getString(fl, "ResponsiblePersonId") });
        sumparameters.push({ "Key": "ProductionStatusId", "Value": getString(fl, "ProductionStatusId") });
        sumparameters.push({ "Key": "ProductLibraryId", "Value": getString(fl, "ProductLibraryId") });
        sumparameters.push({ "Key": "LotNumber", "Value": getString(fl, "LotNumber") });

        $scope.sumparameters = sumparameters;

    }


    $scope.SummeryViewData = function () {
        $scope.SumfilterComplete();

        $http({
            method: 'POST',
            url: $scope.path + "GetSummaryViewData",
            data: { 'parameters': $scope.sumparameters },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.ProductionDataSumReportList = response.data;
                console.log($scope.ProductionDataSumReportList);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.ProSumDataReport = function () {
        var dataLists = [];
         var g = $("#GridSum").data("ejGrid");
        dataLists = g.getFilteredRecords();

        if (dataLists.length == 0) {
            dataLists = $scope.ProductionDataSumReportList;
        }

        $scope.fileName = "ProductionDataSummaryReport";

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpd,
            data: {'reportFileName': $scope.fileName,'data': dataLists},
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

}