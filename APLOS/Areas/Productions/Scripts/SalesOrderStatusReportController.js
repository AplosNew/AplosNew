'use strict';
SalesOrderStatusReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function SalesOrderStatusReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Sales Order Status Report';
    $scope.path = 'Productions/SalesOrderStatusReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    //The Filters 
    $scope.filters = [];
    $scope.SalesOrderStatusloadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var columnList = [
                { field: 'SOId', width: 20, headerText: "SO Id", type: "string" },
                { field: 'OrderStatus', width: 20, headerText: "Order Status", type: "string" },
                { field: 'OrderCategory', width: 20, headerText: "Order Category", type: "string" },
                { field: 'Customer', width: 20, headerText: "Customer", type: "string" },
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
    $scope.SalesOrderStatusloadfilters();

    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }

        var parameters = [];

        parameters.push({ "Key": "SOId", "Value": getString(fl, "SOId") });
        parameters.push({ "Key": "OrderStatus", "Value": getString(fl, "OrderStatus") });
        parameters.push({ "Key": "CustomerId", "Value": getString(fl, "CustomerId") });
        parameters.push({ "Key": "OrderCategoryId", "Value": getString(fl, "OrderCategoryId") });
        parameters.push({ "Key": "ResponsiblePersonId", "Value": getString(fl, "ResponsiblePersonId") });


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

    $scope.OrderStatusId = null;
    $scope.orderStatusList = [];
    $http.get("OrderManagements/orderstatus/getcbo/")
        .then(function (response) {
            $scope.orderStatusList = response.data;
        });


    $scope.GetReport = function (reportType) {
        try {
            $scope.filterComplete();
            $http({
                method: 'POST',
                url: $scope.path + 'XlsSalesOrderStatusReport',
                data: { 'parameters': $scope.parameters },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



}