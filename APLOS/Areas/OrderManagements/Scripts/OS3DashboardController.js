'use strict';
OS3DashboardController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OS3DashboardController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Order Control Dashboard';
    $scope.path = "OrderManagements/OS3Dashboard/";

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';

   //The Selection Criteria
    //New
    $scope.list = [
        { text: "Delivery Month", value: "Delivery" },
        { text: "Entity", value: "Entity" },
        { text: "Customer", value: "Customers" },
        { text: "Marketing Responsible Person", value: "MResp" },
        { text: "Entity Responsible Person", value: "EResp" },


    ];
    $scope.list1 = [
        { text: "NO. Of SO", value: "SO" },
        { text: "SO Quantity", value: "SOQTY" },
        { text: "Sales Value", value: "SORT" },
        { text: "SO Contribution", value: "SOCM" },


    ];
    $scope.list2 = [
        { text: "Ex Factory Date", value: "ExFactoryD" },
        { text: "Delivery Date", value: "DeliveryD" },
        { text: "Commitment Date", value: "CommitmentD" }        
    ];

    $scope.list3 = [
        { text: "Based on Production Date", value: "ProductionD" },
        { text: "Based on  Today", value: "ToD" }
       
    ];
    $rootScope.selected = "Delivery Month";
    $rootScope.group = "Delivery";
    $rootScope.valueC = "NO. Of SO";
    $rootScope.valueCgroup = "SO";
    $rootScope.dateC = "Deliver Date";
    $rootScope.dateCgroup = "DeliveryD";
    $rootScope.chartTypeS = "Based on Production Date";
    $rootScope.chartTypeG = "ProductionD";

    $scope.change = function () {
        var obj = $('#dropdown1').data("ejDropDownList");
        $rootScope.selected = obj.option("text");
        $rootScope.group = obj.option("value");
    }
    $scope.change1 = function () {
        var obj = $('#dropdown2').data("ejDropDownList");
        $rootScope.valueC = obj.option("text");
        $rootScope.valueCgroup = obj.option("value");
    }
    $scope.change2 = function () {
        var obj = $('#dropdown3').data("ejDropDownList");
        $rootScope.dateC = obj.option("text");
        $rootScope.dateCgroup = obj.option("value");
    }
    $scope.change3 = function () {
        var obj = $('#dropdown4').data("ejDropDownList");
        $rootScope.chartTypeS = obj.option("text");
        $rootScope.chartTypeG = obj.option("value");
        $scope.slabGrid();
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
                data: { 'parameters': $scope.parameters, 'group': $scope.group, 'col': two, 'range': e.columnName, 'analysis': $scope.dateCgroup, 'type': $scope.chartTypeG, 'entityId': $scope.Detail2 }
            }).then(function success(response) {
               // console.log(response);
                $scope.DetailList = response.data;
                //var ColumnList = [
                //    { field: 'Id', width: 80, headerText: "So Id", type: "string" },
                //    { field: 'Qty', width: 80, headerText: "SO Oty", type: "number" },
                //    { field: 'Entity', width: 80, headerText: "Entity", type: "string" },
                //    { field: 'customers', width: 120, headerText: "Customer", type: "string" },
                //    { field: 'Remarks', width: 150, headerText: "Remarks", type: "string" },
                //    { field: 'Buyer', width: 120, headerText: "Buyer", type: "string" },
                //    { field: 'BuyerReferenceNo', width: 120, headerText: "Buyer Ref No", type: "string" },
                //    { field: 'OwnReferenceNo', width: 120, headerText: "Own Ref No", type: "string" },
                //    { field: 'IBuyerReferenceNo', width: 120, headerText: "Buyer Item Ref No", type: "string" },
                //    { field: 'IOwnReferenceNo', width: 120, headerText: "Own Item Ref No", type: "string" },
                //    { field: 'DeliveryDate', width: 80, headerText: "Delivery Date", type: "string" },
                //    { field: 'CommitmentDate', width: 120, headerText: "Commitment Date", type: "string" },
                //    { field: 'ProductionDate', width: 120, headerText: "Production Date", type: "string" },
                //    { field: 'DDate', width: 120, headerText: "Ex Factory Date", type: "string" },
                //    { field: 'OrderNo', width: 120, headerText: "Order No", type: "string" },
                //    { field: 'ItemNo', width: 120, headerText: "Item No", type: "string" },
                //    { field: 'PRNo', width: 120, headerText: "Production Order No", type: "string" },
                //    { field: 'MResp', width: 80, headerText: "Marketing Responsible Person", type: "string" },
                //    { field: 'EarlyOrLateBy', width: 80, headerText: "Early Or Late By", type: "string" },
                //    { field: 'OrderStatusId', width: 100, headerText: "SO Status", type: "string" },
                //    { field: 'POStatus', width: 100, headerText: "Production Order Status", type: "string" }

                //];

                //$("#slabClickGrid").ejGrid({
                //    dataSource: response.data,
                //    minWidth: 450, minHeight: 4000,
                //    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true, allowResizing: true,
                //    filterSettings: { filterType: "excel" },
                //    recordDoubleClick: $scope.orderControlShow,
                //    columns: ColumnList
                //});

                //var gridObj = $("#slabClickGrid").data("ejGrid");
                //gridObj.refreshContent(true);
                //gridObj.refreshTemplate();
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
   // var poss = [">0 TO 5", ">5 TO 10", ">10 TO 15", ">15 TO 20", ">20 TO 30", ">30"];

    
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
    $scope.slabGrid = function()
    {
        
        $http({
            method: 'POST',
            url: $scope.path + "getSlabData",
            data: { 'parameters': $scope.parameters, 'group': $rootScope.group, 'value': $rootScope.valueCgroup, 'analysis': $rootScope.dateCgroup, 'type': $scope.chartTypeG },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.slabData = response.data.DATA;
            $scope.Chart = response.data.Chart;
            for (var i = 0; i < response.data.DATA.length; i++) {
                response.data.DATA[i] = Object.assign({}, response.data.DATA[i], response.data.Total[i]);
                
            }
            $scope.slabData = response.data.DATA;
            //Filling of the Chart Data Values
           
            $scope.summaryRows = [{
                title: "Total ", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "total", dataMember: "total", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "NotAlotted", dataMember: "NotAlotted", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "daysthree", dataMember: "daysthree", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "LN30", dataMember: "LN30", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "LN30T20", dataMember: "LN30T20", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "LN20T10", dataMember: "LN20T10", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "LN10T5", dataMember: "LN10T5", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "LN5T0", dataMember: "LN5T0", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "E0", dataMember: "E0", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "G0T5", dataMember: "G0T5", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "G5T10", dataMember: "G5T10", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "G10T15", dataMember: "G10T15", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "G15T20", dataMember: "G15T20", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "G20T30", dataMember: "G20T30", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "G30", dataMember: "G30", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "RowTotal", dataMember: "RowTotal", format: "{0:N0}" }
                    , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "nodates", dataMember: "nodates", format: "{0:N0}" }],
                showCaptionSummary: true
            }];

            var columnList = [];
            //For the different Grids
            if ($scope.group == "Delivery") {
                columnList.push({ field: 'Years', width: 80, headerText: "Years", type: "string" },
                    { field: 'Months', width: 120, headerText: "Months", type: "string" });
            }
            if ($scope.group == "Entity") {
                columnList.push({ field: 'Entity', width: 120, headerText: "Entity", type: "string" });
            }
            if ($scope.group == "Customers") {
                columnList.push({ field: 'Customers', width: 120, headerText: "Customer", type: "string" });
            }
            if ($scope.group == "MResp") {
                columnList.push({ field: 'MResp', width: 120, headerText: "Marketing Responsible Person", type: "string" });
            }
            if ($scope.group == "EResp") {
                columnList.push({ field: 'EResp', width: 120, headerText: "Entity Responsible Person", type: "string" });
            }
            


            columnList.push(
                { field: 'total', width: 80, headerText: "Grand Total", type: "number", format: "{0:N0}"},
                { field: 'NotAlotted', width: 80, headerText: "SO W/O PO", type: "number", format: "{0:N0}"},
                { field: 'daysthree', width: 80, headerText: "SO Created Less Than 3 Days", type: "number", format: "{0:N0}" },
                { field: 'LN30', width: 80, headerText: "<-30", type: "number", format: "{0:N0}" },
                { field: 'LN30T20', width: 80, headerText: "<-30 TO -20", type: "number", format: "{0:N0}" },
                { field: 'LN20T10', width: 80, headerText: "<-20 TO -10", type: "number", format: "{0:N0}"},
                { field: 'LN10T5', width: 80, headerText: "<-10 TO -5", type: "number", format: "{0:N0}" },
                { field: 'LN5T0', width: 80, headerText: "<-5 TO -1", type: "number", format: "{0:N0}" },
                { field: 'E0', width: 80, headerText: "= 0", type: "number", format: "{0:N0}" },
                { field: 'G0T5', width: 80, headerText: ">0 TO 5", type: "number", format: "{0:N0}"},
                { field: 'G5T10', width: 80, headerText: ">5 TO 10", type: "number", format: "{0:N0}"},
                { field: 'G10T15', width: 80, headerText: ">10 TO 15", type: "number", format: "{0:N0}"},
                { field: 'G15T20', width: 80, headerText: ">15 TO 20", type: "number", format: "{0:N0}" },
                { field: 'G20T30', width: 80, headerText: ">20 TO 30", type: "number", format: "{0:N0}"},
                { field: 'G30', width: 80, headerText: ">30", type: "number", format: "{0:N0}" },
                { field: 'RowTotal', width: 80, headerText: "Slab Total", type: "number", format: "{0:N0}" },
                { field: 'nodates', width: 80, headerText: "SO W/O Dates", type: "number", format: "{0:N0}"},
            );

            $("#slabGrid").ejGrid({
                dataSource: $scope.slabData,
                minWidth: 450, minHeight: 800,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                filterSettings: { filterType: "excel" },
                summaryRows: $scope.summaryRows, 
                summaryColumns: $scope.summaryRows,
                recordDoubleClick: $scope.cellDoubleClick,
                queryCellInfo: $scope.cellDetails,
                actionComplete: $scope.refreshPage,
                columns: columnList
            });

            var gridObj = $("#slabGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();

            fillChart();
        });

        
    }


    $scope.slabGrid();

    // The double Click PO From Order Controls

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
    function fillChart() {
        try {
            //var labels = [];
            //var active = [];
            //var pending = [];
            //var close = [];
            //var dispatch = [];
            //var prodcom = [];
            //var l = Object.size($scope.slabData[0]);
            //for (var i = 0; i < l; i++) {
            //    active[i] = 0;
            //    pending[i] = 0;
            //    close[i] = 0;
            //    dispatch[i] = 0;
            //    prodcom[i] = 0;
            //}
            var ini = 0;
            if ($scope.group == "Delivery") {
                slabChart.data.labels = ["", "", "<-30", "<-30 To -20","<-20 TO -10", "<-10 To -5", "<-5 TO 0", "= 0", "> 0 To 5", "> 5 TO 10", ">10 TO 15", ">15 To 20", ">20 To 30", ">30"];
                ini = 2;
            }
            else {
                slabChart.data.labels = [ "", "","<-30","<-30 To -20", "<-20 TO -10", "<-10 To -5", "<-5 TO 0", "= 0", "> 0 To 5", "> 5 TO 10", ">10 TO 15", ">15 To 20", ">20 To 30", ">30"];
                ini = 1;

            }
            //for (var i = 0; i < $scope.slabData.length; i++) {
            //    //var k = 0; 
            //    var kk = ini;
            //    for (var j = 2; j < l; j++) {
            //        if (Object.values($scope.slabData[i])[j] != '-') {
            //            active[ini] = active[ini] + Object.values($scope.slabData[i])[j];
            //            ini++;
            //        }
            //        else {
            //            ini++;
            //        }
            //        //k++;
            //    }
            //    ini = kk;
            //}
            ////labels = ["<-30", "<-20 TO -10", "<-10 To -5", "<-5 TO 0", "= 0", "> 0 To 5", "> 5 TO 10", ">10 TO 15", ">15 To 20", ">20 To 30", ">30"];
            
            ////slabChart.data.labels = $scope.labels;
            slabChart.data.datasets[0].data = $scope.Chart[1];//pending
            slabChart.data.datasets[1].data = $scope.Chart[2];//ToClose
            slabChart.data.datasets[2].data = $scope.Chart[3];//ToDispatch
            slabChart.data.datasets[3].data = $scope.Chart[4];//ProductionComplete
            slabChart.data.datasets[4].data = $scope.Chart[0]; // active
            
            slabChart.update();
        }
        catch (e) { }
    }

    var slabCharter = document.getElementById('slabChart').getContext('2d');

    var slabChart = new Chart(slabCharter, {
        type: 'bar',
        data: {
        labels:  [],
        datasets: [{
                label: 'Pending',
                data: [],
            backgroundColor: '#FFFF00',
        },

            {
                label: 'To Close',
                data: [],
                backgroundColor: '#FF6347', 
            },
            {
                label: 'To Ship',
                data: [],
                backgroundColor:'#FFA500',
            },
            {
                label: 'Production Complete',
                data: [],
                backgroundColor: '#00ff00',
            },
            {
                label: 'Active',
                data: [],
                backgroundColor: '#0E86D4',
            }
        ]
    },
        options: {
            scaleShowValues: true,
            title: {
                display: true,
                text: 'Slab Chart'
            },
            tooltips: {
                mode: 'index',
                intersect: false
            },
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                xAxes: [{
                    ticks: {
                        autoSkip: true,
                        padding: 10,
                        fontSize: 10
                    },
                    stacked: true,
                }],
                yAxes: [{
                    stacked: true
                }]
            }
        }
    });

    $scope.SODetailsReport = function () {
        $scope.fileName = 'SO Details';
        var dataList = [];
        var g = $("#slabClickGrid").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.DetailList;
        }

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
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

    };

}



   