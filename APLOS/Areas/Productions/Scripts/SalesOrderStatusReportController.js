'use strict';
SalesOrderStatusReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function SalesOrderStatusReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Sales Order Status Report';
    $scope.path = 'Productions/SalesOrderStatusReport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    //The Filters 
    $scope.filters = [];
    $scope.summaryfilters = [];
    $scope.wcfilters = [];
    $scope.loadfilters = function () {
        try {
            $scope.filters = [];
            $scope.ProductionDataReportList = [];
            $http({
                method: 'GET',
                url: $scope.path + 'getFilters',
                dataType: 'JSON'
            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.filters = response.data;
                    var columnList = [
                        { field: 'SOId', width: 20, headerText: "SO Id", type: "string" },
                        { field: 'OrderStatus', width: 20, headerText: "Order Status", type: "string" },
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
                }
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
        parameters.push({ "Key": "SOId", "Value": getString(fl, "SOId") });
        parameters.push({ "Key": "OrderStatus", "Value": getString(fl, "OrderStatus") });
        parameters.push({ "Key": "CustomerId", "Value": getString(fl, "CustomerId") });
        parameters.push({ "Key": "ResponsiblePersonId", "Value": getString(fl, "ResponsiblePersonId") });

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

    //$scope.Dates = new Date();
   // $scope.EntityList = null;

    //$scope.parameters = [];
    //$scope.filters = [];
    //$scope.loadfilters = function () {
    //    $http({
    //        method: 'POST',
    //        url:"Productions/ProductiveAllowanceRateSetup/getEntity",
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.EntityList = response.data;
    //    });
    //}
    //$scope.loadfilters();

    $scope.OrderStatusId = null;
    $scope.orderStatusList = [];
    $http.get("OrderManagements/orderstatus/getcbo/")
        .then(function (response) {
            $scope.orderStatusList = response.data;
        });


    //var getString = function (data, column) {
    //    var string = "''";
    //    var collection = [];

    //    for (var i = 0; i < data.length; i++) {
    //        if (collection.includes(data[i][column]) == false) {
    //            string += ",'" + data[i][column] + "'";
    //            collection.push(data[i][column]);
    //        }
    //    }
    //    return string;
    //}

    $scope.GetReport = function (reportType) {
        try {


            var DropDownJobLocationListObjE = $("#selOS").data("ejDropDownList");
            var osLists = DropDownJobLocationListObjE.getSelectedValue();
            if (angular.isUndefinedOrNull(osLists)) {
                throw "Select Order Status.";
            }


            // The Report Code
            $http({
                method: 'POST',
                url: $scope.path + '/XlsSalesOrderStatusReport',
                data: { 'orderStatusId': osLists },
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