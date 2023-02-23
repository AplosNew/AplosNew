'use strict';
SalesOrderWiseProductionCompletionReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http"];
function SalesOrderWiseProductionCompletionReportController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http) {
    $rootScope.title = "Sales Order Wise Production Completion Report";
    $scope.Action = 'Save';

    $scope.path = 'OrderManagements/SalesOrderWiseProductionCompletionReport/';
    //The Filters 
    $scope.filters = [];
    $scope.GetFilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var columnList = [
                { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                { field: 'Customer', width: 20, headerText: "Customer", type: "string" },
                { field: 'ResponsiblePerson', width: 20, headerText: "Responsible Person", type: "string" },
                { field: 'OrderStatusId', width: 20, headerText: "Order Status", type: "string" },

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
    $scope.GetFilters();

    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }

        var parameters = [];

        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "PartyId", "Value": getString(fl, "PartyId") });
        parameters.push({ "Key": "ResponsiblePersonId", "Value": getString(fl, "ResponsiblePersonId") });
        parameters.push({ "Key": "OrderStatusId", "Value": getString(fl, "OrderStatusId") });


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


    $scope.Xgetos3 = function () {
        try {
            var file_src = 'OrderManagements/SalesOrderWiseProductionCompletionReport/OS3xls?entityid=' + 118;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.getos3 = function (reportType) {
        try {
            $scope.filterComplete();
            
            $http({
                method: 'POST',
                url: $scope.path + 'GetOS3xlsReport',
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