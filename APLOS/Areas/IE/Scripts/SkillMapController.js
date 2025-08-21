'use strict';
SkillMapController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SkillMapController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Skill Map';


    //Setting the Dates filter 
    $scope.FirstDay;
    $scope.EndDay;
    Date.prototype.toShortFormat = function () {

        let monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug","Sep", "Oct", "Nov", "Dec"];

        let day = ('0' + this.getDate().toString()).slice(-2);

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

    $scope.resetDates = function()
    {
        $scope.startDate = (new Date()).toShortFormat();
        $scope.dateFilter();
    }
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
                url: 'IE/SkillMap/leftGridData',
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

    
   


    $scope.shiftsAll = [];
    $scope.filterComplete = function (args) {
        
        if ($scope.allData != null ) {
            //destroyTabs();
        }
        loadLeftGrid();
        var gridObj = $("#filters").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        //if (angular.isUndefinedOrNull(filteredRecords) == false) {
        // if (filteredRecords.length > 0) {
        if (filteredRecords.length == 0) {
            filteredRecords = $scope.filters;
            //$scope.allData = $scope.fullTable;
           // $scope.fillTableGrid();
        }
        
            var parameters = [];
            parameters.push({ "Key": "CompanyId", "Value": getString(filteredRecords, "CompanyId") });
            parameters.push({ "Key": "ShiftId", "Value": getString(filteredRecords, "ShiftId") });
            parameters.push({ "Key": "PlantId", "Value": getString(filteredRecords, "PlantId") });
            parameters.push({ "Key": "EntityId", "Value": getString(filteredRecords, "EntityId") });
            parameters.push({ "Key": "ProcessId", "Value": getString(filteredRecords, "ProcessId") });
            parameters.push({ "Key": "CategoryId", "Value": getString(filteredRecords, "CategoryId") });
            parameters.push({ "Key": "TypeId", "Value": getString(filteredRecords, "TypeId") });
            parameters.push({ "Key": "SkillId", "Value": getString(filteredRecords, "SkillId") });
            parameters.push({ "Key": "SkillGroupId", "Value": getString(filteredRecords, "SkillGroupId") });


           // $scope.filters = filteredRecords;
            var shifs = [];
            $scope.shiftsAll = [];
            for (var i = 0; i < filteredRecords.length; i++) {
                
                if (shifs.includes(filteredRecords[i]["Shift"]) == false) {
                    shifs.push(filteredRecords[i]["Shift"]);
                }

                
            }
            for (var i = 0; i < shifs.length; i++) {
                var str = "";
                if (i == shifs.length - 1) {
                    str = shifs[i] + ".";
                }
                else {
                    str = shifs[i] + ", ";
                }
                $scope.shiftsAll.push(str);
            }
            $scope.applyFilters(parameters);
        
            

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


    function destroyTabs() {
        var gridObj1 = $("#summaryDash").data("ejGrid");
        gridObj1.destroy();
        var grid = $("#GridSkill").data("ejGrid");
        grid.destroy();
        var gridObj11 = $("#leftGrid").data("ejGrid");
        gridObj11.destroy();
    }

    //Filter UI for the Main Filters

    $scope.filters = [];
    $scope.loadFilters = function () {
        var ColumnList = [
            { field: 'Company', width: 20, headerText: "Company", type: 'string' },
            { field: 'Plant', width: 20, headerText: "Plant", type: 'string' },
            { field: 'Entity', width: 20, headerText: "Entity", type: 'string' },
            { field: 'Shift', width: 20, headerText: "Shift", type: 'string' },
            { field: 'Process', width: 20, headerText: "Process", type: 'string' },
            { field: 'Category', width: 20, headerText: "Category", type: 'string' },
            { field: 'Type', width: 20, headerText: "Type", type: 'string' },
            { field: 'Skill', width: 20, headerText: "Skill", type: 'string' },
            { field: 'SkillGroup', width: 20, headerText: "Skill Group", type: 'string' },
        ];

        $http({
            method: 'GET',
            url: 'IE/SkillMap/allFilterLists',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
           

        //try {
        //    var gridObj = $("#filters").data("ejGrid");
        //    if (gridObj !== undefined && typeof gridObj === 'object' && typeof gridObj.destroy === 'function') gridObj.destroy();
        //} catch (e) {

        //}
        //$("#filters").ejGrid({
        //    dataSource: $scope.filters,
        //    minWidth: 450, minHeight: 400,
        //    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
        //    filterSettings: { filterType: "excel" },
        //    columns: ColumnList
        //});

        var gridObj = $("#filters").data("ejGrid");
        //gridObj.refreshTemplate();
        $("#filters").children('.e-pager.e-js.e-pager').hide();
        $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
        $("#filters").children('.e-gridcontent').hide();
        });
    }

    $scope.loadFilters();

    ///////////The Date Modifications 
    $scope.dateList = []; //String Format of Dates
    $scope.dates = []; //Dates Format of the strings
    $scope.IntervalsList = [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    $scope.showDates = [];
    $scope.Interval = 1;

    $scope.intervalSet = function () {
        $scope.showDates = [];
        
        var i = 0;
        var intervals = parseInt($scope.Interval);

        for (i = 0; i < $scope.dateList.length ; i += intervals) {
                $scope.showDates.push($scope.dateList[i]);
                
        }
        $scope.fillTableGrid();
        $scope.loadFilters();
    }


    $scope.resetIntervals = function () {
        $scope.showDates = $scope.dateList;
        $scope.Interval = 1;
        $scope.fillTableGrid();
    }

    

 

   
    //The Double Click For Chart\
    $scope.preChart;
    $scope.SkillCodeWork;
    $scope.SkillWork;
    $scope.DateWork;
    $scope.Actual;
    $scope.parameters;
    $scope.required;
    $scope.dataSourceForChart;
    $scope.seq;
    $scope.skillGridDoubleClick = function (e) {
        $scope.preChart = e.data;
        if (e.cellIndex < 5) {
            fillChart(e.data);
        }
        else if (e.cellIndex >= 5 && e.cellIndex < 8)
        {
            var seq = ''; var skillCode = ""; $scope.seq = "";
            $scope.SkillCodeWork = "";
            $scope.SkillWork = "";
            //if (e.cellIndex == 3) { seq = '1'; } 
            //if (e.cellIndex == 4) { seq = '2'; }
            //if (e.cellIndex == 5) { seq = '3'; }

            if (e.cellIndex == 5) { seq = '1'; }
            if (e.cellIndex == 6) { seq = '2'; }
            if (e.cellIndex == 7) { seq = '3'; }

            var skillCode = e.data.SkillCode;
            $scope.SkillCodeWork = skillCode;
            $scope.SkillWork = e.data.Skill;
            $scope.seq = seq;
            var shifts = $scope.parameters[1].Value;
            var companyId = $scope.parameters[0].Value;
            $http({
                method: 'POST',
                url: 'IE/SkillMap/skillwiseEmployee',
                data: { 'code': skillCode, 'shifts': shifts, 'seq': seq, 'companyId': companyId},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                var ColumnList = [                   
                    { field: 'BudgetCode', width: 80, headerText: "Budget Code", type: "string" },
                    { field: 'Entity', width: 80, headerText: "Entity", type: "string" },
                    { field: 'ShiftName', width: 80, headerText: "Shift Name", type: "string" },
                    { field: 'Line', width: 80, headerText: "Line", type: "string" },
                    { field: 'GivenDesignation', width: 80, headerText: "Given Designation", type: "string" },
                    { field: 'LegalDesignation', width: 80, headerText: "Legal Designation", type: "string" },
                    { field: 'EmployeeId', width: 80, headerText: "EmployeeId", type: "string" },
                    { field: 'EmployeeCode', width: 80, headerText: "Employee Code", type: "string" },
                    { field: 'EmployeeName', width: 180, headerText: "Name", type: "string" },
                    { field: 'EmployeeCurrentStatus', width: 180, headerText: "Employee Current Status", type: "string" },
                    { field: 'SubSection', width: 180, headerText: "SubSection", type: "string" },
                    { field: 'Section', width: 180, headerText: "Section", type: "string" },
                    { field: 'Department', width: 180, headerText: "Department", type: "string" }                  
                ];
                $("#employeeSkillGrid").ejGrid({
                    dataSource: response.data,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList
                });

                var gridObj = $("#employeeSkillGrid").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                angular.element(document.querySelector('#employeeModal')).modal('show');
            });
        }
        else {
            var skillCode = e.data.SkillCode;
            var skillId = e.data.SkillId
            var date = e.columnName;
            $scope.SkillCodeWork = "";
            $scope.SkillWork = "";
            $scope.DateWork = "";
            $scope.Actual = "";
            $scope.required = "";
            $scope.dataSourceForChart = "";
            $scope.SkillCodeWork = skillCode;
            $scope.DateWork = date;
            $scope.Actual = e.rowData.Skill1;
           
            $scope.required = parseFloat($scope.Actual) - parseFloat(e.cellValue);
            
            $scope.SkillWork = e.data.Skill;
            $http({
                method: 'POST',
                url: 'IE/SkillMap/allotedWorkCenter',
                data: {'parameters': $scope.parameters,'skillId' :skillId , 'date':date},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.dataSourceForChart = response.data;
                var ColumnList = [
                    { field: 'UserName', width: 80, headerText: "Work Center", type: "string"},
                    { field: 'Alloted', width: 80, headerText: "Required", type: "string" },
                    { field: 'buyer', width: 80, headerText: "Buyer", type: "string" },
                    { field: 'Article', width: 80, headerText: "Article", type: "string" },
                    { field: 'PRNO', width: 80, headerText: "PR No", type: "string" }


                ];
                $("#workCenterGrid").ejGrid({
                    dataSource: $scope.dataSourceForChart,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true, allowResizing: true,
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

    $scope.closedateModal = function () {
        var gridObj = $("#workCenterGrid").data("ejGrid");
        gridObj.destroy();
        angular.element(document.querySelector('#dateModal')).modal('hide');
    }

    $scope.closeemployeeModal = function () {
        var gridObj = $("#employeeSkillGrid").data("ejGrid");
        gridObj.destroy();
        angular.element(document.querySelector('#employeeModal')).modal('hide');
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
        catch (ex)
        {
            
        }
    }


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
        $http({
            method: 'POST',
            url: 'IE/SkillMap/GetScheduleDataFiltered',
            data: { 'parameters': parameters, 'fromDate' :$scope.FirstDay , 'toDate' : $scope.EndDay},
            
        }).then(function successCallback(response) {
            $scope.dateList = response.data.Columns;
            $scope.showDates = response.data.Columns;
            $scope.allData = response;
            $scope.fillTableGrid();
        })

    }

    
    function summaryDash() {
        var response = $scope.allData;
        var ColumnList = [
            { field: 'RowCaption', width: 80, headerText: "Title" }


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
            if (info[i]["Flag"] == "ActualMP") {

                $scope.actualMp.push(info[i]);
            }
            if (info[i]["Flag"] == "RequiredManPower") {

                $scope.requiredMp.push(info[i]);
            }
        }

        summaryDash();
        
        var ColumnList = [
            //{ field: 'SkillId', width: 120, headerText: "SkillId" },
            //{ field: 'SkillCategory', width: 40, headerText: "Skill Category" },
            //{ field: 'SkillCode', width: 80, headerText: "Skill Code" },
            //{ field: 'Skill', width: 180, headerText: "Skill" },
            //{ field: 'SkillGroup', width: 180, headerText: "Skill Group" },
            //{ field: 'Skill1', width: 80, headerText: "Skill 1" },
            //{ field: 'Skill2', width: 80, headerText: "Skill 2" },
            //{ field: 'Skill3', width: 80, headerText: "Skill 3" }

            { field: 'SkillGroup', width: 50, headerText: "Skill Group" },
            { field: 'SkillCategory', width: 50, headerText: "Skill Category" },
            { field: 'Skill', width: 100, headerText: "Skill" },
            { field: 'SkillMaster', width: 180, headerText: "Skill Master" },
            { field: 'SkillCode', width: 50, headerText: "Skill Code" },
            { field: 'Skill1', width: 70, headerText: "Skill 1" },
            { field: 'Skill2', width: 70, headerText: "Skill 2" },
            { field: 'Skill3', width: 70, headerText: "Skill 3" }

            //{ field: 'SkillCategory', width: 40, headerText: "Skill Category" },
            //{ field: 'SkillCode', width: 80, headerText: "Skill Code" },
            //{ field: 'Skill', width: 180, headerText: "Skill" },
            //{ field: 'Skill1', width: 80, headerText: "Skill 1" },
            //{ field: 'Skill2', width: 80, headerText: "Skill 2" },
            //{ field: 'Skill3', width: 80, headerText: "Skill 3" }

        ];

        $scope.skillDates = response.data.Columns;
        //for (var i = 0; i < response.data.Columns.length; i++) {
        //    ColumnList.push({ field: response.data.Columns[i], width: 100, headerText: response.data.Columns[i], format: "{0:N2}", type: "number" });
        //}
        for (var i = 0; i < $scope.showDates.length; i++) {
            ColumnList.push({ field: $scope.showDates[i], width: 70, headerText: $scope.showDates[i], format: "{0:N2}", type: "number" });
        }
        $("#GridSkill").ejGrid({
            dataSource: $scope.shortExcess,
            minWidth: 450, minHeight: 400,
            allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true, allowSorting: true,
            filterSettings: { filterType: "excel" },
            columns: ColumnList,
            recordDoubleClick: $scope.skillGridDoubleClick,
            queryCellInfo: $scope.cellColorChange
        });

        var gridObj = $("#GridSkill").data("ejGrid");
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
            let l = Object.size(data);
            let j = 0;
            var k = Object.values(data)[9];
            for (let i = 10; i < l; i++) {

                if (Object.keys(data)[i] == $scope.showDates[j]) {
                    labelsChart.push(Object.keys(data)[i]);
                    chartData1.push(Object.values($scope.actualMp[k])[i]);
                    chartData2.push(Object.values($scope.requiredMp[k])[i]);

                    j++;
                }

            }

            AreaChart.data.labels = labelsChart;
            AreaChart.data.datasets[0].data = chartData1;
            AreaChart.data.datasets[1].data = chartData2;
            AreaChart.options.title.text = data.Skill;
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
            for (var i = 10; i < l; i++) {
                avail.push(Object.values(data[0])[i]);
                req.push(Object.values(data[1])[i]);
                labels.push(Object.keys(data[0])[i]);

            }

            SummaryChart.data.labels = labels;
            SummaryChart.data.datasets[0].data = avail;
            SummaryChart.data.datasets[1].data = req;
            SummaryChart.update();

        }
        catch (e) {}
    }


    var AreaCharts = document.getElementById('AreaChart').getContext('2d');

    var AreaChart = new Chart(AreaCharts, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'Actual MP',
                data: [],
                backgroundColor: '#f39233',
                borderColor: '#f39233',
                hoverBorderWidth: 2,
                hoverBorderColor: '#054564',
                fill: false,
            },
            {
                label: 'Required MP',
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



    //function dateSets() {
    //    $(function () {
    //        if (!$.fn.bootstrapDP && $.fn.datepicker && $.fn.datepicker.noConflict) {
    //            var datepicker = $.fn.datepicker.noConflict();
    //            $.fn.bootstrapDP = datepicker;
    //        }
    //        var avaiable = [];
    //        avaiable = $scope.showDates;
    //        $(function () {
    //            $('#txtDate').datepicker({
    //                beforeShowDay:
    //                    function (dt) {
    //                        return [available($scope.showDates, dt)];
    //                    },
    //                changeMonth: true, changeYear: true, dateFormat: 'dd-M-y'
    //            });
    //        });
    //    });
    //}
    //function available(avaiable, date) {
    //    let monthNames = ["Jan", "Feb", "Mar", "Apr",
    //        "May", "Jun", "Jul", "Aug",
    //        "Sep", "Oct", "Nov", "Dec"];
    //    let day = ('0' + date.getDate()).slice(-2);
    //    let month = monthNames[date.getMonth()];
    //    let year = date.getFullYear().toString().substr(-2);
    //    let dmy = day + "-" + month + "-" + year;
    //    if ($.inArray(dmy, avaiable) != -1) {
    //        return true;
    //    } else {
    //        return false;
    //    }
    //}