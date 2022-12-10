'use strict';
BOQStatusReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', '$window'];
function BOQStatusReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $controller, $filter, $cboService, $window, fileReader) {
    $rootScope.title = 'BOQ Status Report';
    $scope.ModelList = [];
    $scope.path = 'Costings/BOQStatusReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.Action = 'Save';
    //$scope.searchBy = "UserName"; $scope.searchBySO = "MasterOrderId"; $scope.searchSO = ''; $scope.search = "";


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "PartyId", "Value": getString(fl, "PartyId") });
        parameters.push({ "Key": "Customer", "Value": getString(fl, "Customer") });
        parameters.push({ "Key": "BuyerReferenceNo", "Value": getString(fl, "BuyerReferenceNo") });
        parameters.push({ "Key": "OwnReferenceNo", "Value": getString(fl, "OwnReferenceNo") });
        parameters.push({ "Key": "MasterOrderId", "Value": getString(fl, "MasterOrderId") });
        parameters.push({ "Key": "LineItemId", "Value": getString(fl, "LineItemId") });
        parameters.push({ "Key": "SOId", "Value": getString(fl, "SOId") });
        parameters.push({ "Key": "PONo", "Value": getString(fl, "PONo") });

        $scope.parameters = parameters;
        $scope.getBOQStatus(parameters);
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
    //#region The Filters 

    $scope.filters = [];
    $scope.getBOQStatusFilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getBOQFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var columnList = [
                { field: 'Customer', width: 20, headerText: "Customer", type: "string" },
                { field: 'BuyerReferenceNo', width: 20, headerText: "Buyer Reference No", type: "string" },
                { field: 'OwnReferenceNo', width: 20, headerText: "Own Reference No", type: "string" },
                { field: 'MasterOrderId', width: 20, headerText: "Master Order Id", type: "string" },
                { field: 'LineItemId', width: 20, headerText: "Line Item Id", type: "string" },
                { field: 'SOId', width: 20, headerText: "SO Id", type: "string" },
                { field: 'PONo', width: 20, headerText: "PO No", type: "string" },

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
    $scope.getBOQStatusFilters();

    //new

    $scope.AllFilters = [];

    $scope.ViewAll = function () {
        var gridObj = $("#BOQStatusGrid").data("ejGrid");
        if (!angular.isUndefinedOrNull(gridObj)) {
            gridObj.destroy();
        }
        $scope.filterComplete();
    }
    // Getting the Master Grid
    $scope.BOQStausData = [];

    $scope.getBOQStatus = function (parameters) {
        $http({
            method: 'POST',
            url: $scope.path + 'getBOQStatusReportSql',
            data: { 'filters': parameters }
        }).then(function (resp) {
            if (resp.data.Error == false) {
                $scope.BOQStausData = [];
                $scope.BOQStausData = resp.data.Data;
                var ColumnList = [
                    { field: 'PRStatus', width: 80, headerText: "PO Status", type: "string", width: 80 },
                    { field: 'ProductionOrderId', width: 80, headerText: "PO", type: "string", width: 80 },
                    { field: 'Customer', width: 80, headerText: "Customer", type: "string", width: 80 },
                    { field: 'ProductCode', width: 80, headerText: "ProductCode", type: "string", width: 80 },
                    { field: 'BuyerRef', width: 80, headerText: "Buyer Ref", type: "string", width: 80 },
                    { field: 'OwnRef', width: 80, headerText: "Own Ref", type: "string", width: 80 },
                    { field: 'LineItem', width: 80, headerText: "Line Item", type: "string", width: 80 },
                    { field: 'OrderQty', width: 80, headerText: "Order Qty", type: "number", width: 80 },
                    { field: 'PlanQty', width: 80, headerText: "Plan Qty", type: "number", width: 80 },
                    { field: 'ProdQty', width: 80, headerText: "Producted Qty", type: "number", width: 80 },
                    { field: 'ToProduce', width: 80, headerText: "To Produce", type: "number", width: 80 },
                    { field: 'ExcessProduce', width: 80, headerText: "Excess Produce", type: "number", width: 80 },
                ];
                $("#BOQStatusGrid").ejGrid({
                    dataSource: $scope.BOQStausData,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true, allowResizing: true,
                    filterSettings: { filterType: "excel" },
                    recordDoubleClick: $scope.detailClick,
                    columns: ColumnList
                });
            }
            else {
                ShowResult(resp.data.Message, 'failure');
            }

        });
    }

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.getBOQStatusReport = function () {
        $scope.filterComplete();
        //var dataList = [];
        //var g = $("#filters").data("ejGrid");
        //dataList = g.getFilteredRecords();

        //if (dataList.length == 0) {
        //    dataList = $scope.parameters;
        //}

        $http({
            method: 'POST',
            url: $scope.path + "GetBOQStatusReport",
            //data: { 'parameters': dataList },
            data: { 'parameters': $scope.parameters },
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