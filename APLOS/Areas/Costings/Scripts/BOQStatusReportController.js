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

    //new
    $scope.BOQStausData = [];

    $scope.ViewAll = function () {
        $scope.filterComplete();
        $http({
            method: 'POST',
            url: $scope.path + "getBOQStatusData",
            data: { 'parameters': $scope.parameters },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.BOQStausData = response.data;
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
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