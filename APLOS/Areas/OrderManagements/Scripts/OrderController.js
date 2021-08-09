'use strict';
OrderController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OrderController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Order';
    $scope.path = "OrderManagements/Order/";

    $scope.fromDate = null;
    $scope.toDate = null;


   //The Selection Criteria   
    $scope.Datelist = [
        { text: "Ex Factory Date", value: "ExFactoryD" },
        { text: "Delivery Date", value: "DeliveryD" },
        { text: "Commitment Date", value: "CommitmentD" }        
    ];
    $rootScope.selected = "Delivery Month";
    $rootScope.group = "Delivery";
    $rootScope.valueC = "NO. Of SO";
    $rootScope.valueCgroup = "SO";
    $rootScope.dateC = "Deliver Date";
    $rootScope.dateCgroup = "DeliveryD";
    $rootScope.chartTypeS = "Based on Production Date";
    $rootScope.chartTypeG = "ProductionD";
    $scope.dateValueChange = function () {
        var obj = $('#dropdownDate').data("ejDropDownList");
        $rootScope.dateC = obj.option("text");
        $rootScope.dateCgroup = obj.option("value");
    }

    $scope.setAll = function () {
        $scope.filterComplete();
    }

  

    //The Filters 
    $scope.filters=[];
    $scope.loadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var columnList = [
                { field: 'Plant', width: 20, headerText: "Plant", type: "string" },
                { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                { field: 'Customer', width: 20, headerText: "Customer", type: "string" },
                { field: 'MResP', width: 20, headerText: "Mark. Responsible Person", type: "string" },
                { field: 'EResp', width: 20, headerText: "Entity Responsible Person", type: "string" },
                { field: 'Status', width: 20, headerText: "SO Status", type: "string" },
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
        destroyGrid();
        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "PlantId", "Value": getString(fl, "PlantId") });
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "CustomerId", "Value": getString(fl, "CustomerId") });
        parameters.push({ "Key": "MResId", "Value": getString(fl, "MResId") });
        parameters.push({ "Key": "ERespId", "Value": getString(fl, "ERespId") });
        parameters.push({ "Key": "Status", "Value": getString(fl, "Status") });

        $scope.parameters = parameters;
        $scope.slabGrid();
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
    function destroyGrid() {
        var g = $("#slabGrid").data("ejGrid");
        g.destroy();
    }

    $scope.clearFilters = function () {

        var gridObj = $("#filters").data("ejGrid");
        gridObj.clearFiltering();
    }

    //The Cell Functions For the Grid
    $scope.Detail1;
    $scope.Detail2;
    $scope.Detail3;
    $scope.DetailList = [];
    $scope.cellDoubleClick = function (e) {
        var d = 0;
        if ($scope.group == 'Delivery') {
            d = 1;
        }
        if (e.cellIndex > d) {


            $scope.Detail1 = "";
            $scope.Detail2 = "";
            $scope.Detail3 = "";
            $scope.Detail1 = Object.values(e.data)[0];
            $scope.Detail2 = Object.values(e.data)[1];
            $scope.Detail3 = e.columnName;
            var two = Object.values(e.data)[0] + ":" + Object.values(e.data)[1];
            $http({
                method: 'POST',
                url: $scope.path + 'getClickData',
                data: { 'parameters': $scope.parameters, 'group': $scope.group, 'col': two, 'range': e.columnName, 'analysis': $scope.dateCgroup, 'type': $scope.chartTypeG }
            }).then(function success(response) {
               // console.log(response);
                $scope.DetailList = response.data;
                angular.element(document.querySelector('#clickModal')).modal('show');
            });
        }

    }

    $scope.closeModal = function () {
        var gridObj = $("#slabClickGrid").data("ejGrid");
        gridObj.destroy();
        angular.element(document.querySelector('#clickModal')).modal('hide');
    }
    
    var negs = ["LN30", "LN30T20","LN20T10","LN10T5","LN5T0","E0","G0T5","G5T10","G10T15","G15T20","G20T30","G30"];

    
    $scope.cellDetails = function (e) {
        try {
            if (e.data[e.column.field] === 0) {
                e.data[e.column.field] = "-";
                e.rowData[e.column.field] = "-";
            }
            for (var i = 0; i < negs.length; i++) {

                if (e.column.field == negs[i]) {

                    if (e.data[e.column.field] == '-') {
                        e.rowData[e.column.field] = "-";
                        e.cell.bgColor = '#D3D3D3';
                        break;
                    }
                    if (e.data[e.column.field] != 0 && e.data[e.column.field] !='-') {
                        if (i < 5) {
                            e.cell.bgColor = '#FF6347';
                        }
                        if (i == 5 || i == 6) {
                            e.cell.bgColor = '#FFFF00';
                        }
                        if (i > 6) {
                            e.cell.bgColor = '#00ff00';
                        }
                        break;
                    }
                    
                }
            }
        }
        catch (ex) {

        }
    }

    $scope.refreshPage = function (e) {
        if (e.requestType == "paging") {
            var gridObj = $("#slabGrid").data("ejGrid");
       gridObj.refreshContent(true);
        gridObj.refreshTemplate();
        }
        var k = 100;
    }

    // The Slab Grid Data
    $scope.negs = [];
    $scope.zeros = [];
    $scope.tens = [];
    $scope.poss = [];
    $scope.slabData = [];
    $scope.Chart = [];
    $scope.ControlList = [];
    $scope.orderControlShow = function (e) {
       
        var pr = e.data.PRNo;
        $http({
            method: 'POST',
            url: $scope.path + 'getControlList',
            data: {'pr':pr}
        }).then(function succ(resp) {
            $scope.ControlList = [];
            $scope.ControlList = resp.data;
            angular.element(document.querySelector('#orderControl')).modal('show');
        });
    }
    /// Charts Section

    Object.size = function (obj) {
        var size = 0, key;
        for (key in obj) {
            if (obj.hasOwnProperty(key)) size++;
        }
        return size;
    };
  
}



   