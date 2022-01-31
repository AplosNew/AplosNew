'use strict';
ProductionGeneralReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProductionGeneralReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Production Report';
    $scope.path = 'Productions/ProductionGeneralReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    //Variables 
    $scope.filtersList = [];
    $scope.masterData = [];
    $scope.masterDetailData = [];
    $scope.ColName = 'Master';
    $scope.ChosenPRID = null;
    $scope.ProcessList = [];
    $scope.ProcessId = null;

    // The Tab Switching Code

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    // Getting the Filters 
    //- $http({}).then(function () { });

    $scope.getFilters = function () {

        $http({
            method: 'POST',
            url: $scope.path + "getProcess",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + 'getFilters'
        }).then(function (resp) {
            $scope.filtersList = resp.data;
            var gridObj = $("#FilterList").data("ejGrid");
            gridObj.refreshContent(true);
            $("#FilterList").children('.e-gridcontent').hide();
        });
    }

   
    // Filling in the Filters as Parameters
    $scope.AllFilters = [];

    $scope.ViewAll = function () {
        var gridObj = $("#MasterGrid").data("ejGrid");
        if (!angular.isUndefinedOrNull(gridObj)) {
            gridObj.destroy();
        }
        $scope.fillFilters();
    }

    $scope.fillFilters = function () {

        if (angular.isUndefinedOrNull($scope.ProcessId)) {
            ShowResult('Please Select a Process!!', 'failure');
            throw ("Invalid Request");
        }

        var gridObj = $("#FilterList").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        
        if (filteredRecords.length == 0) {
            filteredRecords = $scope.filtersList;
        }

        var parameters = [];
        parameters.push({ "Key": "CustomerId", "Value": getString(filteredRecords, "CustomerId") });
        parameters.push({ "Key": "BuyerRef", "Value": getString(filteredRecords, "BuyerRef") });
        parameters.push({ "Key": "OwnRef", "Value": getString(filteredRecords, "OwnRef") });
        parameters.push({ "Key": "MOId", "Value": getString(filteredRecords, "MOId") });
        parameters.push({ "Key": "LineItem", "Value": getString(filteredRecords, "LineItem") });
        parameters.push({ "Key": "SO", "Value": getString(filteredRecords, "SO") });
        parameters.push({ "Key": "PRStatId", "Value": getString(filteredRecords, "PRStatId") });
        parameters.push({ "Key": "SOOrderId", "Value": getString(filteredRecords, "SOOrderId") });
        parameters.push({ "Key": "PSLibId", "Value": getString(filteredRecords, "PSLibId") });


        $scope.AllFilters = parameters;

        $scope.getMaster(parameters);
    }

    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        if (data.length > 0) {
            for (var i = 0; i < data.length; i++) {
                if (collection.includes(data[i][column]) == false) {
                    string += ",'" + data[i][column] + "'";
                    collection.push(data[i][column]);
                }
            }
        }
       
        return string;
    }

    // Getting the Master Grid

    $scope.getMaster = function (param) {
        $http({
            method: 'POST',
            url: $scope.path + 'getMasterGrid',
            data: {'filters': param , 'ProcessId':$scope.ProcessId}
        }).then(function (resp) {
            if (resp.data.Error == false) {
                $scope.masterData = [];
                $scope.masterData = resp.data.Data;
                var ColumnList = [
                    { field: 'PRStatus', width: 80, headerText: "PO Status", type: "string" , width: 80},
                    { field: 'ProductionOrderId', width: 80, headerText: "PO", type: "string" , width: 80},
                    { field: 'Customer', width: 80, headerText: "Customer", type: "string" , width: 80},
                    { field: 'ProductCode', width: 80, headerText: "ProductCode", type: "string" , width: 80},
                    { field: 'BuyerRef', width: 80, headerText: "Buyer Ref", type: "string" , width: 80},
                    { field: 'OwnRef', width: 80, headerText: "Own Ref", type: "string" , width: 80},
                    { field: 'LineItem', width: 80, headerText: "Line Item", type: "string" , width: 80},
                    { field: 'OrderQty', width: 80, headerText: "Order Qty", type: "number" , width: 80},
                    { field: 'PlanQty', width: 80, headerText: "Plan Qty", type: "number" , width: 80},
                    { field: 'ProdQty', width: 80, headerText: "Producted Qty", type: "number" , width: 80},
                    { field: 'ToProduce', width: 80, headerText: "To Produce", type: "number" , width: 80},
                    { field: 'ExcessProduce', width: 80, headerText: "Excess Produce", type: "number" , width: 80},
                ];
                $("#MasterGrid").ejGrid({
                    dataSource: $scope.masterData,
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

    //Double Click in Master Grid
    $scope.detailClick = function (e) {
        if (e.cellIndex > 7) {
            $scope.ColName = e.columnName;
        }
        else {
            $scope.ColName = 'Master';
        }
        
        var PRId = e.data.ProductionOrderId;
        $scope.ChosenPRID = PRId;
        $http({
            method: 'POST',
            url: $scope.path + 'masterDetail',
            data: { 'PRId': PRId, 'Col': $scope.ColName, 'Filters': $scope.AllFilters, 'ProcessId': $scope.ProcessId},
        }).then(function (resp) {
            $scope.masterDetailData = [];
            $scope.masterDetailData = resp.data;
            var ColumnList = [
                { field: 'SalesOrderId', width: 80, headerText: "SO", type: "string", width: 80 },
                { field: 'CharV', width: 80, headerText: "SKU1", type: "string", width: 80 },
                { field: 'Char2V', width: 80, headerText: "SKU2", type: "string", width: 80 },
                { field: 'OrderQty', width: 80, headerText: "Order Qty", type: "number", width: 80 },
                { field: 'PlanQty', width: 80, headerText: "Plan Qty", type: "number", width: 80 },
                { field: 'ProducedQty', width: 80, headerText: "Producted Qty", type: "number", width: 80 },
                { field: 'ShortExcess', width: 80, headerText: "Short Excess", type: "number", width: 80 },
            ];
            $("#detailMaster").ejGrid({
                dataSource: $scope.masterDetailData,
                minWidth: 450, minHeight: 400,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true, allowResizing: true,
                filterSettings: { filterType: "excel" },
                columns: ColumnList
            });
            angular.element(document.querySelector('#masterDetail')).modal('show');
        });
    }

    $scope.closeModal = function () {
        var gridObj = $("#detailMaster").data("ejGrid");
        gridObj.destroy();
        angular.element(document.querySelector('#masterDetail')).modal('hide');
    }

    //Downloading Of the Reports
    $scope.getReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getReports",
            data: { 'PRId': $scope.ChosenPRID, 'Filters': $scope.AllFilters, 'ProcessId': $scope.ProcessId},
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

    //Initial Loading Functions
    $scope.getFilters();
}