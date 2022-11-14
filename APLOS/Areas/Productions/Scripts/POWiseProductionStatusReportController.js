'use strict';
POWiseProductionStatusReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window'];
function POWiseProductionStatusReportController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $scope.title = 'PO Wise Production Status Report';
    $scope.path = 'Productions/POWiseProductionStatusReport/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';

    //The Filters 
    $scope.filters = [];
    $scope.loadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
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

    $scope.ProductionData = function () {
        $scope.filterComplete();
        $scope.fileName = "ProductionDataReport.xlsx";

        $http({
            method: 'POST',
            url: $scope.path + "ProductionDataXls",
            data: { 'parameters': $scope.parameters },
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