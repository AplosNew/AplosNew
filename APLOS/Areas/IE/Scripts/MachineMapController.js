'use strict';
MachineMapController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MachineMapController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Machine Map';

   
    var article = document.getElementById('slideArticle');




    //Setting the Dates filter 
    $scope.FirstDay;
    $scope.EndDay;
    Date.prototype.toShortFormat = function () {

        let monthNames = ["Jan", "Feb", "Mar", "Apr",
            "May", "Jun", "Jul", "Aug",
            "Sep", "Oct", "Nov", "Dec"];

        let day = this.getDate();

        let monthIndex = this.getMonth();
        let monthName = monthNames[monthIndex];

        let year = this.getFullYear().toString().substr(-2);

        return `${day}-${monthName}-${year}`;
    }

    Date.prototype.addDays = function (days) {
        var date = new Date(this.valueOf());
        date.setDate(date.getDate() + days);
        return date;
    }

    $scope.startDate = (new Date()).toShortFormat();
    
    $scope.datesArray = [];

    $scope.dateFilter = function () {
        $scope.datesArray = [];
        var date = new Date($scope.startDate);
        var jj = date.toShortFormat();
        $scope.FirstDay = jj;
        //$scope.datesArray.push(jj);
        for (var i = 0; i < 90; i++) {
            var today = date;
            var k = today.addDays(1);
            $scope.datesArray.push(k.toShortFormat());
            date = k;
        }
        $scope.EndDay = $scope.datesArray[89];
    }

    $scope.dateFilter();

    $scope.resetDates = function () {
        $scope.startDate = (new Date()).toShortFormat();
        $scope.dateFilter();
    }
    // The Left Grid
    $scope.left = [];
    async function loadLeftGrid() {
        var ColumnList = [
            { field: 'CompanyName', width: 20, headerText: "Company", type: "string" },
            { field: 'PlantName', width: 20, headerText: "Plant", type: "string" },
            { field: 'NoOfSO', width: 20, headerText: "No Of SO", type: "string" },
            { field: 'SOQty', width: 20, headerText: "SO Quantity", type: "string" },
            { field: 'PendingSOForPR', width: 20, headerText: "Pending", type: "string" },
            { field: 'BulletinToAttach', width: 20, headerText: "Bulletin To Attach", type: "string" },
            { field: 'BulletinToAttachW45Days', width: 20, headerText: "45 Days Bulleting Pending", type: "string" }
        ];
        if ($scope.left.length == 0 || $scope.left == null) {
            $http({
                method: 'GET',
                url: 'IE/MachineMap/leftGridData',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.left = response.data;

                $("#leftGrid").ejGrid({
                    dataSource: $scope.left,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList
                });

                var gridObj = $("#leftGrid").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            });
        }
        else {
            $("#leftGrid").ejGrid({
                dataSource: $scope.left,
                minWidth: 450, minHeight: 400,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                filterSettings: { filterType: "excel" },
                columns: ColumnList
            });

            var gridObj = $("#leftGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        }


    }





    $scope.filterComplete = function (args) {

        var gridObj = $("#filters").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        if ($scope.allData != null) {
           // destroyTabs();
        }
        loadLeftGrid();
        if (filteredRecords.length == 0) {
            filteredRecords = $scope.filters;
        }
        //if (angular.isUndefinedOrNull(filteredRecords) == false) {
        // if (filteredRecords.length > 0) {

        try {
            var parameters = [];
            parameters.push({ "Key": "CompanyId", "Value": getString(filteredRecords, "CompanyId") });
            parameters.push({ "Key": "PlantId", "Value": getString(filteredRecords, "PlantId") });
            parameters.push({ "Key": "EntityId", "Value": getString(filteredRecords, "EntityId") });
            parameters.push({ "Key": "ProcessId", "Value": getString(filteredRecords, "ProcessId") });
            $scope.applyFilters(parameters);
        }
        catch (e) {

        }
        

        //  }
        // else {
        //     console.log('else');
        // }
        //}

    }
    function destroyTabs() {
        var gridObj1 = $("#summaryDash").data("ejGrid");
        gridObj1.destroy();
        var grid = $("#GridMachine").data("ejGrid");
        grid.destroy();
        var gridObj11 = $("#leftGrid").data("ejGrid");
        gridObj11.destroy();
    }

    //Filter UI for the Main Filters

    $scope.filters = [];
    $scope.loadFilters = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMap/allFilterLists',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;


            try {
                var gridObj = $("#filters").data("ejGrid");
                if (gridObj !== undefined && typeof gridObj === 'object' && typeof gridObj.destroy === 'function') gridObj.destroy();
            } catch (e) {

            }
            var ColumnList = [
                { field: 'Company', width: 20, headerText: "Company", type:"string"},
                { field: 'Plant', width: 20, headerText: "Plant", type:"string"},
                { field: 'Entity', width: 20, headerText: "Entity", type:"string"},
                { field: 'Process', width: 20, headerText: "Process", type:"string"}
            ];



            $("#filters").ejGrid({
                dataSource: $scope.filters,
                minWidth: 450, minHeight: 400,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                filterSettings: { filterType: "excel" },
                columns: ColumnList
            });

            var gridObj = $("#filters").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $("#filters").children('.e-pager.e-js.e-pager').hide();
            $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#filters").children('.e-gridcontent').hide();
        });

    }

    $scope.loadFilters();
    ///////////The Date Modifications 
    $scope.dateList = []; //String Format of Dates
    $scope.dates = []; //Dates Format of the strings
    $scope.IntervalsList = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    $scope.showDates = [];
    $scope.Interval = 1;

    $scope.intervalSet = function () {
        $scope.showDates = [];
        var i = 0;
        var intervals = parseInt($scope.Interval);

        for (i = 0; i < $scope.dateList.length; i += intervals) {
            $scope.showDates.push($scope.dateList[i]);

        }
        $scope.fillTableGrid();
    }


    $scope.resetIntervals = function () {
        $scope.showDates = $scope.dateList;
        $scope.Interval = 1;
        $scope.fillTableGrid();
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




    //The Double Click For Chart\
    $scope.preChart;
    $scope.MachineIdWork;
    $scope.ArticleId;
    $scope.MachineWork;
    $scope.DateWork;
    $scope.Actual;
    $scope.required;
    $scope.parameters;
    $scope.skillGridDoubleClick = function (e) {
        $scope.preChart = e.data;


        if (article.checked == false) {
            if (e.cellIndex < 3) {
                fillChart(e.data);
            }
            else {

                var MachineId = e.data.MachineId;
                var date = e.columnName;
                $scope.MachineIdWork = "";
                $scope.MachineWork = "";
                $scope.DateWork = "";
                $scope.Actual = "";
                $scope.required = "";
                $scope.MachineIdWork = MachineId;
                $scope.DateWork = date;
                $scope.Actual = e.rowData.Available;
                $scope.required = parseFloat($scope.Actual) - parseFloat(e.cellValue);
                $scope.MachineWork = e.data.Machine;
                $http({
                    method: 'POST',
                    url: 'IE/MachineMap/allotedWorkCenter',
                    data: { 'parameters': $scope.parameters, 'machineId': MachineId, 'date': date },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    var ColumnList = [
                        { field: 'WorkCenter', width: 180, headerText: "Work Center", type: "string"},
                        { field: 'Allotted', width: 80, headerText: "Required", type: "string"},
                        { field: 'buyer', width: 80, headerText: "Buyer", type: "string" },
                        { field: 'Article', width: 80, headerText: "Article", type: "string" },
                        { field: 'PRNO', width: 80, headerText: "PR No", type: "string" }

                    ];
                    $("#workCenterGrid").ejGrid({
                        dataSource: response.data,
                        minWidth: 450, minHeight: 400,
                        allowFiltering: true, allowPaging: true, enableTouch: true,
                        responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true, allowResizing: true,
                        filterSettings: { filterType: "excel" },
                        columns: ColumnList
                    });

                    var gridObj = $("#workCenterGrid").data("ejGrid");
                    gridObj.refreshContent(true);
                    gridObj.refreshTemplate();
                    angular.element(document.querySelector('#dateModal')).modal('show');
                });
            }
        }
        else {
            if (e.cellIndex < 4) {
                fillChart(e.data);
            }
            else {

                var MachineId = e.data.ArticleId;
                var date = e.columnName;
                $scope.MachineIdWork = "";
                $scope.MachineWork = "";
                $scope.DateWork = "";
                $scope.Actual = "";
                $scope.required = "";
                $scope.MachineIdWork = MachineId;
                $scope.DateWork = date;
                $scope.Actual = e.rowData.Available;
                $scope.required = parseFloat($scope.Actual) - parseFloat(e.cellValue);
                $scope.MachineWork = e.data.Articles;
                $http({
                    method: 'POST',
                    url: 'IE/MachineMap/allotedArticleWorkCenter',
                    data: { 'parameters': $scope.parameters, 'machineVId': MachineId, 'date': date },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    var ColumnList = [

                        { field: 'WorkCenter', width: 180, headerText: "Work Center", type: "string"},
                        { field: 'Allotted', width: 80, headerText: "Required", type: "string" },
                        { field: 'buyer', width: 80, headerText: "Buyer", type: "string" },
                        { field: 'Article', width: 80, headerText: "Article", type: "string" },
                        { field: 'PRNO', width: 80, headerText: "PR No", type: "string" }


                    ];
                    $("#workCenterGrid").ejGrid({
                        dataSource: response.data,
                        minWidth: 450, minHeight: 400,
                        allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true,
                        allowSelection: true, allowTextWrap: true, allowScrolling: true, allowResizing:true,
                        filterSettings: { filterType: "excel" },
                        columns: ColumnList
                    });

                    var gridObj = $("#workCenterGrid").data("ejGrid");
                    gridObj.refreshContent(true);
                    gridObj.refreshTemplate();
                    angular.element(document.querySelector('#dateModal')).modal('show');
                });
            }
        }
        

    }


    $scope.closedateModal = function () {
        var gridObj = $("#workCenterGrid").data("ejGrid");
        gridObj.destroy();
        angular.element(document.querySelector('#dateModal')).modal('hide');
    }

    //The Cell Color Change in the Grid
    $scope.cellColorChange = function (e) {
        try {


            for (var i = 0; i < $scope.skillDates.length; i++) {
                if ($scope.skillDates[i] == e.column.field) {
                    if (e.data[e.column.field] < 0) {
                        e.cell.bgColor = '#ff0000';
                    }
                    if (e.data[e.column.field] > 0) {
                        e.cell.bgColor = '#00ff00';
                    }
                    break;
                }
            }

        }
        catch (ex) {

        }
    }

   /* $scope.doki = function () {

        var gridObj1 = $("#summaryDash").data("ejGrid");
        gridObj1.destroy();

        var gridObj = $("#GridMachine").data("ejGrid");
        gridObj.destroy();
    }*/

    $scope.clearFilters = function () {

        var gridObj = $("#filters").data("ejGrid");
        gridObj.clearFiltering();
    }



    $scope.pivotdatasource = [];
    $scope.shortExcess = [];
    $scope.actualMp = [];
    $scope.requiredMp = [];
    $scope.skillDates;
    $scope.allData;

    //The Grid For the Skills




    $scope.applyFilters = function (parameters) {
        $scope.dataSource = [];
        $scope.shortExcess = [];
        $scope.actualMp = [];
        $scope.requiredMp = [];
        $scope.parameters = parameters;
        if (article.checked == true) {
            console.log("YES");

            $http({
                method: 'POST',
                url: 'IE/MachineMap/GetScheduleDataArticleFiltered',
                data: { 'parameters': parameters, 'fromDate': $scope.FirstDay, 'toDate': $scope.EndDay },

            }).then(function successCallback(response) {
                $scope.dateList = response.data.Columns;
                $scope.showDates = response.data.Columns;
                $scope.allData = response;
                $scope.preChart = {};
                $scope.fillTableGrid();
            })
        }
        else {
            console.log("NO");

            $http({
                method: 'POST',
                url: 'IE/MachineMap/GetScheduleDataFiltered',
                data: { 'parameters': parameters, 'fromDate': $scope.FirstDay, 'toDate': $scope.EndDay },

            }).then(function successCallback(response) {
                $scope.dateList = response.data.Columns;
                $scope.showDates = response.data.Columns;
                $scope.allData = response;
                $scope.preChart = {};
                $scope.fillTableGrid();
            })
        }


    }


    function summaryDash() {
        console.log($scope.allData.data.Compact);
        var response = $scope.allData;
        var ColumnList = [
            { field: 'Flag', width: 80, headerText: "Title" , type:"string"}


        ];


        for (var i = 0; i < response.data.Columns.length; i++) {
            ColumnList.push({ field: response.data.Columns[i], width: 100, headerText: response.data.Columns[i], format: "{0:N2}", type: "number" });
        }

        $("#summaryDash").ejGrid({
            dataSource: response.data.Compact,
            minWidth: 450, minHeight: 400,
            allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
            filterSettings: { filterType: "excel" },
            columns: ColumnList
            //queryCellInfo: $scope.cellColorChange
        });

        var gridObj = $("#summaryDash").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
        fillSummary(response.data.Compact);
    }

    $scope.fillTableGrid = function () {
        $scope.shortExcess = [];
        $scope.actualMp = [];
        $scope.requiredMp = [];
        var response = $scope.allData;
        $scope.dataSource = response.data.DATA;
        var info = response.data.DATA;
        for (var i = 0; i < info.length; i++) {
            if (info[i]["Flag"] == "ShortExcess") {
                $scope.shortExcess.push(info[i]);
            }
            if (info[i]["Flag"] == "Available") {

                $scope.actualMp.push(info[i]);
            }
            if (info[i]["Flag"] == "Allotted") {

                $scope.requiredMp.push(info[i]);
            }
        }

        summaryDash();

        try {
            var gridObj = $("#GridMachine").data("ejGrid");
            if (gridObj !== undefined && typeof gridObj === 'object' && typeof gridObj.destroy === 'function') gridObj.destroy();
        } catch (e) {

        }
        if (article.checked == false) {
            var ColumnList = [

                { field: 'MachineId', width: 40, headerText: "Id", type: "string" },
                { field: 'Machine', width: 180, headerText: "Machine", type: "string" },
                { field: 'Available', width: 80, headerText: "Available", type: "string"}


            ];
        }
        else {
            var ColumnList = [

                { field: 'ArticleId', width: 40, headerText: "Article Id", type: "string"},
                { field: 'MachineName', width: 180, headerText: "Machine", type: "string"},
                { field: 'Articles', width: 180, headerText: "Articles", type: "string"},
                { field: 'Available', width: 80, headerText: "Available", type: "string"}


            ];
        }

        $scope.skillDates = response.data.Columns;
        //for (var i = 0; i < response.data.Columns.length; i++) {
        //    ColumnList.push({ field: response.data.Columns[i], width: 100, headerText: response.data.Columns[i], format: "{0:N2}", type: "number" });
        //}
        for (var i = 0; i < $scope.showDates.length; i++) {
            ColumnList.push({ field: $scope.showDates[i], width: 70, headerText: $scope.showDates[i], format: "{0:N2}", type: "number" });
        }
        $("#GridMachine").ejGrid({
            dataSource: $scope.shortExcess,
            minWidth: 450, minHeight: 400,
            allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true, allowSorting: true,
            filterSettings: { filterType: "excel" },
            columns: ColumnList,
            recordDoubleClick: $scope.skillGridDoubleClick,
            queryCellInfo: $scope.cellColorChange
        });

        var gridObj = $("#GridMachine").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();

        if ($scope.preChart == undefined || $scope.preChart == null) {
            $scope.preChart = response.data.DATA[0];
            fillChart(response.data.DATA[0]);
        }
        else {
            fillChart($scope.preChart);
        }
    }

    // $scope.tab = 1;
    //$scope.setTaba = function (newTab) {
    //    $scope.tab = newTab;
    //};
    //$scope.isSeta = function (tabNum) {
    //    return $scope.tab === tabNum;
    //};

    //$scope.setTabb = function (newTab) {
    //    $scope.tab = newTab;
    //};
    //$scope.isSetb = function (tabNum) {
    //    return $scope.tab === tabNum;
    //};

    //-----------------------------------Charts ---------------------

    Object.size = function (obj) {
        var size = 0, key;
        for (key in obj) {
            if (obj.hasOwnProperty(key)) size++;
        }
        return size;
    };


    function fillChart(data) {
        try {
            var labelsChart = [];
            var chartData1 = [];
            var chartData2 = [];
            var title = "";
            let l = Object.size(data);
            if (article.checked == false) {
                let j = 0;
                title = data.Machine;
                var k = Object.values(data)[4];
                for (let i = 5; i < l; i++) {

                    if (Object.keys(data)[i] == $scope.showDates[j]) {
                        labelsChart.push(Object.keys(data)[i]);
                        chartData1.push(Object.values($scope.actualMp[k])[i]);
                        chartData2.push(Object.values($scope.requiredMp[k])[i]);

                        j++;
                    }

                }
            }
            else {
                let j = 0;
                title = data.Articles;
                var k = Object.values(data)[5];
                for (let i = 6; i < l; i++) {

                    if (Object.keys(data)[i] == $scope.showDates[j]) {
                        labelsChart.push(Object.keys(data)[i]);
                        chartData1.push(Object.values($scope.actualMp[k])[i]);
                        chartData2.push(Object.values($scope.requiredMp[k])[i]);

                        j++;
                    }

                }
            }
            

            AreaChart.data.labels = labelsChart;
            AreaChart.data.datasets[0].data = chartData1;
            AreaChart.data.datasets[1].data = chartData2;
            AreaChart.options.title.text = title;
            AreaChart.update();
        }
        catch (e) {

        }

    }

    function fillSummary(data) {
        try {
            var avail = [];
            var req = [];
            var labels = [];
            var l = Object.size(data[0]);
            if (article.checked == false) {
                for (var i = 5; i < l; i++) {
                    avail.push(Object.values(data[0])[i]);
                    req.push(Object.values(data[1])[i]);
                    labels.push(Object.keys(data[0])[i]);

                }

                SummaryChart.data.labels = labels;
                SummaryChart.data.datasets[0].data = avail;
                SummaryChart.data.datasets[1].data = req;
                SummaryChart.update();
            }
            else {
                for (var i = 6; i < l; i++) {
                    avail.push(Object.values(data[0])[i]);
                    req.push(Object.values(data[1])[i]);
                    labels.push(Object.keys(data[0])[i]);

                }

                SummaryChart.data.labels = labels;
                SummaryChart.data.datasets[0].data = avail;
                SummaryChart.data.datasets[1].data = req;
                SummaryChart.update();
            }
            

        }
        catch (e) { }
    }



    var AreaCharts = document.getElementById('AreaChart').getContext('2d');

    var AreaChart = new Chart(AreaCharts, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'Available',
                data: [],
                backgroundColor: '#f39233',
                borderColor: '#f39233',
                hoverBorderWidth: 2,
                hoverBorderColor: '#054564',
                fill: false,
            },
            {
                label: 'Required',
                data: [],
                backgroundColor: '#b8de6f',
                borderColor: '#b8de6f',
                hoverBorderWidth: 2,
                hoverBorderColor: '#054564',
                fill: false,
            }
            ]
        },
        options: {

            bezierCurve: false,
            scaleShowValues: true,
            scales: {
                //yAxes: [{
                //    ticks: {
                //        beginAtZero: true
                //    }
                //}],
                xAxes: [{
                    ticks: {
                        autoSkip: true,
                        padding: 10,
                        fontSize: 10
                    }
                }]
            },
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: true,
                text: ''
            },

        },
    });



    var Summary = document.getElementById('SummaryChart').getContext('2d');

    var SummaryChart = new Chart(Summary, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'Total Available',
                data: [],
                backgroundColor: '#f39233',
                borderColor: '#f39233',
                hoverBorderWidth: 2,
                hoverBorderColor: '#054564',
                fill: false,
            },
            {
                label: 'Total Required',
                data: [],
                backgroundColor: '#b8de6f',
                borderColor: '#b8de6f',
                hoverBorderWidth: 2,
                hoverBorderColor: '#054564',
                fill: false,
            }
            ]
        },
        options: {

            bezierCurve: false,
            scaleShowValues: true,
            scales: {
                xAxes: [{
                    ticks: {
                        autoSkip: true,
                        padding: 10,
                        fontSize: 10
                    }
                }]
            },
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: true,
                text: 'Summary Chart'
            },

        },
    });
}



   