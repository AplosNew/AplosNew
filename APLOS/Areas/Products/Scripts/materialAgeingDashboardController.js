'use strict';
materialAgeingDashboardController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function materialAgeingDashboardController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    $scope.BudgetDateSuper = [];
    $scope.BudgetWiseExpenseList = [];
    $scope.BudgetWiseExpenseTillToday = [];
    $scope.BudgetWiseExpenseDetailList = [];
    $scope.ExpenseDetailList = [];
    $scope.expenseType = null;
    $scope.periodType = null;
    $scope.BudgetedChartLabel = [];
    $scope.BudgetedchartExpenseList = [];
    $scope.BudgetedChartBudgeted = [];
    $scope.MonthlyBudgetVSExpense = [];
    $scope.chartRevenueList = [];
    $scope.chartBudgetedRevenueList = [];   

    $scope.PeriodicBudgetVSExpense = [];
    window.chartColors = {
        red: 'rgba(240, 52, 52, .6)',
        orange: 'rgb(255, 159, 64)',
        yellow: 'rgb(255, 205, 86)',
        green: 'rgba(46, 204, 113,.6)',
        blue: 'rgb(54, 162, 235)',
        purple: 'rgb(153, 102, 255)',
        grey: 'rgb(201, 203, 207)',
        lightBlue: 'rgb(160, 184, 222)',
        lightGreen: 'rgb(139, 245, 137)'
    };
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.chartExpenseList = [];
    $scope.chartRevenueList = [];
    $scope.chartDelayExpenseList = [];
    $scope.chartDelayRevenueList = [];
    var BudgetedBarChart;

    //$scope.ExpenseList = [];
    $scope.RevenueList = [];
    $scope.ExpenseListTotal = 0;
    $scope.RevenueListTotal = 0;

    $scope.chartList = [];
    //$scope.ExpenseList = [];
    $scope.index = -1;
    $scope.chartLabel = [];
    $scope.chartExceptionList = [];
    $scope.ColList = [];
    $scope.stIndex = -2;
    $scope.budgetName = [];
    var factDate = [];

    $scope.budgetDate = [];
    $scope.toolTipLabel = [];
    $scope.BudgetDateSuper = [];
    $scope.BudgetAmount = [];
    var colorNames = Object.keys(window.chartColors);

    $scope.dateWisechartLabel = [];
    $scope.dateWisechartList = [];
    $scope.dateWisechartDatasetLabel = [];
    var flags = [];
    var expensebarChart;
    var periodExpenseBarChart;
    var bDateWiseExpenseChart;
    var periodWiseExpenseChart;
    var bWEChart;
    var bWEChart2;
    var chartType = 'bar';
    //$scope.report = {
    //    IsAsset: false
    //};
    $scope.report.IsAsset = false;
    var now = new Date();
    $scope.dateRange = {};

    $scope.itemGroupOption = [
        { value: "0", Name: "PL" },

        { value: "1", Name: "BS" }
    ];
    var dateWiseBudgetedDatasets = {
        label: null,
        backgroundColor: window.chartColors.green,
        borderColor: window.chartColors.green,
        fill: null,
        data: null
    };
    $scope.fiscalYearList = [];

    $scope.expFactDate = {
        factDate: 'postingDate'
    };
    $scope.BaseCurrencyCode = null;

    $scope.LoadData = function GetEntity() {
        //debugger;
        $scope.ShowLoader = true;
        $.ajax({
            type: "GET",
            contentType: "application/json; charset=utf-8",
            url: 'Products/InventoryDashboard/GetCompanyPlantInformation',
            data: {},
            async: false,
            dataType: "json",
            success: function (data) {
                //Hide loader image & process successful data.
                $scope.ShowLoader = false;
                $("#Grid2").ejGrid({

                    dataSource: data, // data must be array of json
                    allowPaging: true,
                    //allowSorting: true,
                    allowFiltering: true,
                    isResponsive: true,
                    enableResponsiveRow: true,
                    allowTextWrap: true,
                    textWrapSettings: { wrapMode: "header" },
                    cssClass: "filtered",
                    filterSettings: {
                        filterType: "excel"
                    },
                    // pageSize: 1,
                    allowScrolling: true,
                    scrollSettings: { width: "400", height: "2" },
                    //summaryRows:
                    //    [
                    //        {
                    //            title: "Total =",
                    //            color: "Red",
                    //            summaryColumns: [
                    //                {
                    //                    summaryType: ej.Grid.SummaryType.Sum
                    //                    , displayColumn: "ManpowerBudget"
                    //                    , dataMember: "ManpowerBudget"
                    //                    //, format: "{0:C1}"
                    //                },
                    //                {
                    //                    summaryType: ej.Grid.SummaryType.Sum
                    //                    , displayColumn: "OnRoll"
                    //                    , dataMember: "OnRoll"
                    //                    //, format: "{0:C1}"
                    //                },
                    //                {
                    //                    summaryType: ej.Grid.SummaryType.Sum
                    //                    , displayColumn: "TotalPresent"
                    //                    , dataMember: "TotalPresent"
                    //                    //, format: "{0:C1}"
                    //                },
                    //                {
                    //                    summaryType: ej.Grid.SummaryType.Sum
                    //                    , displayColumn: "OnRollShort"
                    //                    , dataMember: "OnRollShort"
                    //                    //, format: "{0:C1}"
                    //                },
                    //                {
                    //                    summaryType: ej.Grid.SummaryType.Sum
                    //                    , displayColumn: "OnRollExcess"
                    //                    , dataMember: "OnRollExcess"
                    //                    //, format: "{0:C1}"
                    //                },
                    //                {
                    //                    summaryType: ej.Grid.SummaryType.Sum
                    //                    , displayColumn: "PresentShort"
                    //                    , dataMember: "PresentShort"
                    //                    //, format: "{0:C1}"
                    //                },
                    //                {
                    //                    summaryType: ej.Grid.SummaryType.Sum
                    //                    , displayColumn: "PresentExcess"
                    //                    , dataMember: "PresentExcess"
                    //                    //, format: "{0:C1}"
                    //                },
                    //            ]

                    //        }
                    //    ],

                    columns: [
                        { headerText: "Company", field: "Company", width: 30 },
                        { headerText: "Plant", field: "PlantName", width: 30 }
                        //{ headerText: "Skill", field: "Skill", width: 60 },
                        //{ headerText: "Operation Name", field: "OperationName", width: 90 },
                        //{ headerText: "Skill Group", field: "SkillGroupe", width: 90 },
                        //{ headerText: "Operation Category", field: "OperationCategoryName", width: 80 },
                        //{ headerText: "Machine Category", field: "MachineCategory", width: 85 },
                        //{ headerText: "Machine Sub Category", field: "MachineSubCategory", width: 95 },
                        ////{ headerText: "Position", field: "Position", width: 80 },
                        //{ headerText: "OnRoll", field: "OnRoll", width: 70 },
                        //{ headerText: "T.Present", field: "TotalPresent", width: 80 },
                        //{ headerText: "OR.Short", field: "OnRollShort", width: 80 },
                        //{ headerText: "OR.Excess", field: "OnRollExcess", width: 90 },
                        //{ headerText: "P.Short", field: "PresentShort", width: 80 },
                        //{ headerText: "P.Excess", field: "PresentExcess", width: 80 }




                        //{ field: "OerationCode", headerText: "Operation Code", textAlign: ej.TextAlign.Right, width: 200 },
                        //{ field: "OperationName", headerText: "Operation Name", width: 200, visibility: false },
                        //{ headerText: "Type", field: "Type", width: 100 },
                        // { headerText: "Machine Master", field: "MachineMaster", width: 200 },

                        //{ title: "EntityCode", field: "EntityCode", filterable: true, width: 200, filterable: { multi: true, search: true } },

                        //{ title: "PositionCode", field: "PositionCode", filterable: true, filterable: { multi: true, search: true } },
                        //{ headerText: "PositionName", field: "PositionName", width: 200 },

                        //{ title: "MachineCode", field: "MachineCode", width: 200, filterable: true, filterable: { multi: true, search: true } },

                        //{ headerText: "MachineCode", field: "MachineCode", width: 200 },
                        //        //{ title: "MachineCategoryCode", field: "MachineCategoryCode", width: 200, filterable: true, filterable: { multi: true, search: true } },


                        // { headerText: "SkillGroupingCode", field: "SkillGroupingCode", width: 200 },

                        // { headerText: "DesignationCategory", field: "DesignationCategory", width: 200 },
                        //{ headerText: "StandardSalary", field: "StandardSalary", width: 200 },

                        //{ headerText: "LegalDesignation", field: "LegalDesignation", width: 200 }

                        //{ headerText: "ManpowerBudget", field: "ManpowerBudget", width: 200 },

                    ]//,
                    // rowDataBound: "rowDataBound"


                });
                $("#Grid2").children('.e-pager.e-js.e-pager').hide();
                $("#Grid2").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#Grid2").children('.e-gridcontent').hide();
                //$("#Grid2").children('.e-grid .e-headercell {background - color: chocolate;}').add();

                $("#Grid2").children('.e-grid.e-headercell').css('background-color', 'red'); //{background - color: chocolate;}').add();

            }
        });
    }
    $scope.LoadData();
    $scope.groupName = 'groupName';
    $scope.data = false;
    $scope.Isregulardata = true;
    $scope.ExpenseList = [];
    $scope.GetDelayList = function () {
        //debugger;              
        var obj = $("#Grid2").ejGrid("instance");
        var sd = obj.getFilteredRecords();
        var sd1 = obj.getSelectedRecords
        var value = "";
        var queryString = "''";
        var queryStringProcess = "''";
        if (sd.length > 0) {
            sd = obj.model.dataSource;
            $scope.plantvisible = 'visible';
        }
        else {
            var queryString = null;
            var queryStringProcess = null;
        }
        var arr = [];
        var queryString = [];
        var arrqueryStringProcess = [];


        var index = 0;
        for (var i = 0; i < sd.length; i++) {
            var x = sd[i];            

            var yEntityName = x["CompanyId"];
            var yProcess = x["PlantId"];            


            if (!arr.includes(yEntityName)) {
                queryString += ",'" + yEntityName + "'";
                //queryStringForSum += ",'" + yEntityName + "'";

                //queryString1 += ",'" + yEntityName + "'";
                arr.push(yEntityName);

            }
            if (!arrqueryStringProcess.includes(yProcess)) {
                queryStringProcess += ",'" + yProcess + "'";
                arrqueryStringProcess.push(yProcess);
            }
        }

        //debugger;
        var currentTotalEmp = 0;
        var proposedTotalEmp = 0;
        var Short = 0;
        var excess = 0;
        var unallocated = 0;
        //$scope.expensePeriodList = [];
        //$scope.revenuePeriodList = [];
        //$scope.ExpenseListTotal = null;
        //$scope.RevenueListTotal = null;

        $http({
            method: 'POST',
            url: 'Products/InventoryDashboard/MaterialAgeingStatusDashboard',
            data: {
                'factDate': $scope.expFactDate.factDate,
                'fromDate': $scope.dateRange.fromDate,
                'toDate': $scope.dateRange.toDate,
                'groupName': $scope.groupName,
                //'Companywiseplantdata': $scope.Companywiseplantdata,
                'ValueOrNumber': $scope.Isregulardata,
                'queryString': queryString,
                'queryStringProcess': queryStringProcess,
                'IsAsset': $scope.report.IsAsset,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ExpenseList = response.data;
            //setList(response.data);
            //createColList();
        });
        //$scope.GetExpenseListGraph();
        

    };
   // $scope.GetDelayList();


    function setList(list) {
        $scope.ExpenseList = [];
        $scope.ExpenseList = list;
        $scope.chartLabel = [];
        $scope.chartList = [];
        //$scope.stackingGraph();
    }
    
    function createColList() {
        if (baseService.arrayLength($scope.ExpenseList) >= 0) {
            var row = {
                Sequence: null,
                Id: null,
                StandardName: null,
                ColumnName: null,
                RType: null,
                Text: null,
                Name: null,
                date: ''
            };
            row.Sequence = -2;
            row.Id = $scope.ExpenseList[0].CompanyGroupId;
            row.StandardName = "Group";
            row.ColumnName = "Group";
            row.Text = $scope.ExpenseList[0].GroupName;
            row.Name = $scope.ExpenseList[0].GroupName;

            row.date = $scope.date;

            $scope.ColList.push(row);
            var rowc = {
                Sequence: null,
                Id: null,
                StandardName: null,
                ColumnName: null,
                RType: null,
                Text: null,
                Name: null,
                date: ''
            };
            rowc.Sequence = -1;
            rowc.Id = $scope.ExpenseList[0].CompanyId;
            row.StandardName = "Company";
            rowc.ColumnName = "Company";
            rowc.Text = $scope.ExpenseList[0].UserName;
            rowc.Name = $scope.ExpenseList[0].UserName;
            rowc.date = $scope.date;

            $scope.ColList.push(rowc);
            getDrillDownList();
        }
    }
   
    $scope.dFunction = function () {
        
            $scope.expensePeriodList = [];
            $scope.ExpenseListTotal = 0;
            $scope.revenuePeriodList = [];
            $scope.RevenueListTotal = 0;
            var obj = $("#Grid2").ejGrid("instance");
            var sd = obj.getFilteredRecords();
        if (sd.length == 0) {
            $scope.GetDelayList();
        }
        else {
            if (sd.length == 0) {
                sd = obj.model.dataSource;
            }
            var arr = [];
            var queryString = [];
            var arrqueryStringProcess = [];
            

            var value = "";
            var queryString = "''";
            var queryStringProcess = "''";
            
            var index = 0;
            for (var i = 0; i < sd.length; i++) {
                var x = sd[i];
                
                var yEntityName = x["CompanyId"];
                var yProcess = x["PlantId"];
                


                if (!arr.includes(yEntityName)) {
                    queryString += ",'" + yEntityName + "'";

                    arr.push(yEntityName);

                }
                if (!arrqueryStringProcess.includes(yProcess)) {
                    queryStringProcess += ",'" + yProcess + "'";
                    arrqueryStringProcess.push(yProcess);
                }

                


                $http({
                    method: 'POST',
                    url: 'Products/InventoryDashboard/MaterialAgeingStatusDashboard',
                    data: {
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate,
                        'groupName': $scope.groupName,
                        //'Companywiseplantdata': $scope.Companywiseplantdata,
                        'ValueOrNumber': $scope.Isregulardata,
                        'queryString': queryString,
                        'queryStringProcess': queryStringProcess,
                        'IsAsset': $scope.report.IsAsset,
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.ExpenseList = response.data;
                    //setList(response.data);
                    //if ($scope.ColList.length === 0) {
                    //    createColList();
                    //}
                    //else {
                    //    $scope.index = -1;
                    //    $scope.stIndex = $scope.index - 1;
                    //}


                });

            }
        }
    };  



    $scope.dateRange.fromDate = $filter("dateFiltering")(Date.now());
    $scope.dateRange.toDate = $filter("dateFiltering")(Date.now());

  

    

    $scope.budgetedExpenseList = [];

    $scope.budgetedExpenseChartLabel = [];

    function getDrillDownList() {
        $http({
            method: 'POST',
            url: 'Products/InventoryDashboard/OrgStructureList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                for (var i = 0; i < baseService.arrayLength(response.data); i++) {
                    var row = {
                        Sequence: -2,
                        Id: null,
                        StandardName: null,
                        ColumnName: null,
                        RType: null,
                        Text: null,
                        Name: null,
                        date: ''
                    };
                    row.Sequence = i;
                    row.StandardName = response.data[i].StandardName;
                    row.ColumnName = response.data[i].ColumnName;
                    row.RType = response.data[i].RType;
                    row.Text = response.data[i].UId;
                    row.date = $scope.date;
                    $scope.ColList.push(row);
                }
            }
        });
    }

   

    $scope.setIndex = function (x) {
        for (var i = 0; i < $scope.ColList.length; i++) {
            if ($scope.ColList[i].Sequence === $scope.index) {
                $scope.ColList[i].Id = x.CompanyId;
                $scope.ColList[i].Text = x.UId;
                $scope.ColList[i].Name = x.ColumnName;
            }
        }
    };
    //console.log("ColList Length", $scope.ColList[$scope.ColList.length]);

    function getCol(seq) {
        for (var i = 0; i < baseService.arrayLength($scope.ColList); i++) {
            if ($scope.ColList[i].Sequence === seq) {
                return $scope.ColList[i].ColumnName;
            }
        }
    }

    $scope.setIndexHead = function (x) {
        $scope.index = x.Sequence;
    };






    $scope.headerNav = function (x) {
        //debugger;
       
        $scope.groupName = 'groupName';
     
        if (x.Sequence !== -2) {
            $scope.setIndexHead(x);
            $scope.GetDetailDrillDownTableJS(x.Id);
        }
        else {
            $scope.setIndexHead(x);
            $http({
                method: 'POST',
                url: 'Products/InventoryDashboard/MaterialAgeingStatusDashboard',

                data: {

                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate,
                    'groupName': $scope.groupName,
                    //'Company': $scope.Company,
                    'Companywiseplantdata': $scope.Companywiseplantdata,
                    'CompanyId': $scope.CompanyId,
                    'PlantId': $scope.PlantId,
                    'ValueOrNumber': $scope.Isregulardata,
                    'IsAsset': $scope.report.IsAsset,
                    
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                setList(response.data);
                $scope.index = -1;
                $scope.stIndex = $scope.index - 1;
            });
            //$http({
            //    method: 'POST',
            //    url: 'accounts/ExpenseDashboard/ExpenseListLineChart',
            //    data: {
            //        'factDate': $scope.expFactDate.factDate,
            //        'fromDate': $scope.dateRange.fromDate,
            //        'toDate': $scope.dateRange.toDate
            //    },
            //    dataType: 'JSON'
            //}).then(function successCallback(response) {
            //    $scope.expensePeriodList = response.data;
            //    $http({
            //        method: 'POST',
            //        url: 'accounts/ExpenseDashboard/RevenueListLineChart',
            //        data: {
            //            'factDate': $scope.expFactDate.factDate,
            //            'fromDate': $scope.dateRange.fromDate,
            //            'toDate': $scope.dateRange.toDate
            //        },
            //        dataType: 'JSON'
            //    }).then(function successCallback(response) {
            //        $scope.revenuePeriodList = response.data;
            //    });
            //    if ($scope.expensePeriodList.length > 0 || $scope.revenuePeriodList.length > 0) {
            //        setLineList($scope.fiscalYearList, $scope.expensePeriodList, $scope.revenuePeriodList);

            //        //createLineChart();
            //    }
            //});
            //$http({
            //    method: 'POST',
            //    url: 'accounts/ExpenseDashboard/PeriodWiseExpenseBarChart',
            //    data: {
            //        'factDate': $scope.expFactDate.factDate,
            //        'fromDate': $scope.dateRange.fromDate,
            //        'toDate': $scope.dateRange.toDate
            //    },
            //    dataType: 'JSON'
            //}).then(function successCallback(response) {
            //    $scope.BudgetedExpenseList = response.data;

            //    $http({
            //        method: 'POST',
            //        url: 'accounts/ExpenseDashboard/PeriodWiseRevenueBarChart',
            //        data: {
            //            'factDate': $scope.expFactDate.factDate,
            //            'fromDate': $scope.dateRange.fromDate,
            //            'toDate': $scope.dateRange.toDate
            //        },
            //        dataType: 'JSON'
            //    }).then(function successCallback(response) {
            //        $scope.BudgetedRevenueList = response.data;
            //        setPeriodicBudgetList($scope.BudgetedExpenseList, $scope.BudgetedRevenueList);
            //        createPeriodicalBudgetedBarChart();
            //    });
            //});

            //$http({
            //    method: 'POST',
            //    url: 'accounts/ExpenseDashboard/MonthlyExpenseVSBudgetBarChart',
            //    data: {
            //        'factDate': $scope.expFactDate.factDate,
            //        'fromDate': $scope.dateRange.fromDate,
            //        'toDate': $scope.dateRange.toDate
            //    },
            //    dataType: 'JSON'
            //}).then(function successCallback(response) {
            //    $scope.MonthlyExpenseVSBudget = response.data;


            //    $http({
            //        method: 'POST',
            //        url: 'accounts/ExpenseDashboard/MonthlyRevenueVSBudgetBarChart',
            //        data: {
            //            'factDate': $scope.expFactDate.factDate,
            //            'fromDate': $scope.dateRange.fromDate,
            //            'toDate': $scope.dateRange.toDate
            //        },
            //        dataType: 'JSON'
            //    }).then(function successCallback(response) {
            //        $scope.MonthlyRevenueVSBudget = response.data;

            //        setMonthlyBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
            //        createLineChart();
            //    });
            //});
            //$http({
            //    method: 'POST',
            //    url: 'accounts/ExpenseDashboard/PeriodExpenseVSBudgetBarChart',
            //    data: {
            //        'factDate': $scope.expFactDate.factDate,
            //        'fromDate': $scope.dateRange.fromDate,
            //        'toDate': $scope.dateRange.toDate
            //    },
            //    dataType: 'JSON'
            //}).then(function successCallback(response) {
            //$scope.MonthlyExpenseVSBudget = response.data;
            //$http({
            //        method: 'POST',
            //        url: 'accounts/ExpenseDashboard/PeriodRevenueVSBudgetBarChart',
            //        data: {
            //            'factDate': $scope.expFactDate.factDate,
            //            'fromDate': $scope.dateRange.fromDate,
            //            'toDate': $scope.dateRange.toDate
            //        },
            //        dataType: 'JSON'
            //    }).then(function successCallback(response) {
            //        $scope.MonthlyRevenueVSBudget = response.data;

            //        setPeriodWiseBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
            //        createPeriodBarChart();
            //    });
            // });
        }
    };



    $scope.GetDetailDrillDownTableJS = function (data) {
        //debugger;
        $scope.DDList = [];
        $scope.expenseList = [];
        $scope.revenueList = [];
        $scope.ExpenseListTotal = 0;
        $scope.RevenueListTotal = 0;
        $scope.Companywiseplantdata = 'Companywiseplantdata';
       
        if ($scope.index + 2 < $scope.ColList.length) {
            $http({
                method: 'POST',
                url: 'Products/InventoryDashboard/MaterialAgeingDashboardPlant/',
                data: {
                    'ChartColumnList': $scope.ColList,
                    'seq': $scope.index,
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate,
                    'CompanyWisePlant': $scope.dateRange.toDate,
                    'CompanyId': data.CompanyId,
                    //'CompanyId': data,
                    'PlantId': '',
                    'IsRegular': $scope.Isregulardata
                      
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.DDList = response.data;
                setList(response.data);
                $scope.index += 1;
                $scope.stIndex = $scope.index - 1;

            });

            //$http({
            //    method: 'POST',
            //    url: 'accounts/ExpenseDashboard/DymnamicExpenseListLineChart/',
            //    data: {
            //        'ChartColumnList': $scope.ColList,
            //        'seq': $scope.index,
            //        'factDate': $scope.expFactDate.factDate,
            //        'fromDate': $scope.dateRange.fromDate,
            //        'toDate': $scope.dateRange.toDate
            //    },
            //    dataType: 'JSON'
            //}).then(function successCallback(response) {
            //    var seque = $scope.index;
            //    $scope.expensePeriodList = response.data;

            //    for (var i = 0; i < $scope.expensePeriodList.length; i++) {
            //        $scope.ExpenseListTotal += $scope.revenuePeriodList.Amount;
            //    }

            //    $http({
            //        method: 'POST',
            //        url: 'accounts/ExpenseDashboard/DymnamicRevenueListLineChart/',
            //        data: {
            //            'ChartColumnList': $scope.ColList,
            //            'seq': seque,
            //            'factDate': $scope.expFactDate.factDate,
            //            'fromDate': $scope.dateRange.fromDate,
            //            'toDate': $scope.dateRange.toDate
            //        },
            //        dataType: 'JSON'
            //    }).then(function successCallback(response) {
            //        $scope.revenuePeriodList = response.data;

            //        for (var i = 0; i < $scope.revenuePeriodList.length; i++) {
            //            $scope.RevenueListTotal += $scope.revenuePeriodList.Amount;
            //        }

            //        //if ($scope.expensePeriodList.length > 0 || $scope.revenuePeriodList.length > 0) {
            //        setLineList($scope.fiscalYearList, $scope.expensePeriodList, $scope.revenuePeriodList);

            //        //createLineChart();
            //        //}
            //    });

            //});
            //$http({
            //    method: 'POST',
            //    url: 'accounts/ExpenseDashboard/DynamicPeriodWiseExpenseBarChart',
            //    data: {
            //        'ChartColumnList': $scope.ColList,
            //        'seq': $scope.index,
            //        'factDate': $scope.expFactDate.factDate,
            //        'fromDate': $scope.dateRange.fromDate,
            //        'toDate': $scope.dateRange.toDate
            //    },
            //    dataType: 'JSON'
            //}).then(function successCallback(response) {
            //    $scope.BudgetedExpenseList = response.data;


            //    $http({
            //        method: 'POST',
            //        url: 'accounts/ExpenseDashboard/DynamicPeriodWiseRevenueBarChart',
            //        data: {
            //            'ChartColumnList': $scope.ColList,
            //            'seq': $scope.index,
            //            'factDate': $scope.expFactDate.factDate,
            //            'fromDate': $scope.dateRange.fromDate,
            //            'toDate': $scope.dateRange.toDate
            //        },
            //        dataType: 'JSON'
            //    }).then(function successCallback(response) {
            //        $scope.BudgetedRevenueList = response.data;
            //        setMonthlyBudgetVSExpenseList($scope.BudgetedExpenseList, $scope.BudgetedRevenueList);
            //        createPeriodicalBudgetedBarChart();


            //    });

            //});

            //$http({
            //    method: 'POST',
            //    url: 'accounts/ExpenseDashboard/MonthlyDynamicExpenseVSBudgetBarChart',
            //    data: {
            //        'ChartColumnList': $scope.ColList,
            //        'seq': $scope.index,
            //        'factDate': $scope.expFactDate.factDate,
            //        'fromDate': $scope.dateRange.fromDate,
            //        'toDate': $scope.dateRange.toDate
            //    },
            //    dataType: 'JSON'
            //}).then(function successCallback(response) {
            //    $scope.MonthlyExpenseVSBudget = response.data;


            //    $http({
            //        method: 'POST',
            //        url: 'accounts/ExpenseDashboard/MonthlyDynamicRevenueVSBudgetBarChart',
            //        data: {
            //            'ChartColumnList': $scope.ColList,
            //            'seq': $scope.index,
            //            'factDate': $scope.expFactDate.factDate,
            //            'fromDate': $scope.dateRange.fromDate,
            //            'toDate': $scope.dateRange.toDate
            //        },
            //        dataType: 'JSON'
            //    }).then(function successCallback(response) {
            //        $scope.MonthlyRevenueVSBudget = response.data;


            //        setPeriodicBudgetList($scope.BudgetedExpenseList, $scope.BudgetedRevenueList);
            //        createPeriodicalBudgetedBarChart();
            //    });
            //});
            //$http({
            //    method: 'POST',
            //    url: 'accounts/ExpenseDashboard/PeriodDynamicExpenseVSBudgetBarChart',
            //    data: {
            //        'ChartColumnList': $scope.ColList,
            //        'seq': $scope.index,
            //        'factDate': $scope.expFactDate.factDate,
            //        'fromDate': $scope.dateRange.fromDate,
            //        'toDate': $scope.dateRange.toDate
            //    },
            //    dataType: 'JSON'
            //}).then(function successCallback(response) {
            //    $scope.PeriodExpenseVSBudget = response.data;

            //    $http({
            //        method: 'POST',
            //        url: 'accounts/ExpenseDashboard/PeriodDynamicRevenueVSBudgetBarChart',
            //        data: {
            //            'ChartColumnList': $scope.ColList,
            //            'seq': $scope.index,
            //            'factDate': $scope.expFactDate.factDate,
            //            'fromDate': $scope.dateRange.fromDate,
            //            'toDate': $scope.dateRange.toDate
            //        },
            //        dataType: 'JSON'
            //    }).then(function successCallback(response) {
            //        $scope.PeriodRevenueVSBudget = response.data;

            //        setPeriodWiseBudgetVSExpenseList($scope.PeriodExpenseVSBudget, $scope.PeriodRevenueVSBudget);
            //        createPeriodBarChart();
            //    });
            //});
        }
    };

    $scope.exceptionList = {
        entryPeriod: null,
        postingPeriod: null,
        normalPostedAmout: null,
        delayPostedAmount: null
    };
    $scope.listForChart = [];

    
   
    $scope.GetExpenseList = function () {
        //debugger;
        var currentTotalEmp = 0;
        var proposedTotalEmp = 0;
        var Short = 0;
        var excess = 0;
        var unallocated = 0;
        $scope.expensePeriodList = [];
        $scope.revenuePeriodList = [];
        $scope.ExpenseListTotal = null;
        $scope.RevenueListTotal = null;

        $http({
            method: 'POST',
            url: 'Products/InventoryDashboard/MaterialAgeingStatusDashboard',
            data: {
                'factDate': $scope.expFactDate.factDate,
                'fromDate': $scope.dateRange.fromDate,
                'toDate': $scope.dateRange.toDate,
                'groupName': $scope.groupName,
                'Companywiseplantdata': $scope.Companywiseplantdata,
                'ValueOrNumber': $scope.Isregulardata,
                'IsAsset': $scope.report.IsAsset,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setList(response.data);
           // createColList();
        });
        //$http({
        //    method: 'POST',
        //    url: 'accounts/ExpenseDashboard/ExpenseListLineChart',
        //    data: {
        //        'factDate': $scope.expFactDate.factDate,
        //        'fromDate': $scope.dateRange.fromDate,
        //        'toDate': $scope.dateRange.toDate
        //    },
        //    dataType: 'JSON'
        //}).then(function successCallback(response) {
        //    $scope.expensePeriodList = response.data;
        //    $scope.ExpenseListTotal = $scope.expensePeriodList.reduce(function (sum, expense) {
        //        return sum + expense.Amount;
        //    }, 0);
        //    $http({
        //        method: 'POST',
        //        url: 'accounts/ExpenseDashboard/RevenueListLineChart',
        //        data: {
        //            'factDate': $scope.expFactDate.factDate,
        //            'fromDate': $scope.dateRange.fromDate,
        //            'toDate': $scope.dateRange.toDate
        //        },
        //        dataType: 'JSON'
        //    }).then(function successCallback(response) {
        //        $scope.revenuePeriodList = response.data;
        //        $scope.RevenueListTotal = $scope.revenuePeriodList.reduce(function (sum, expense) {
        //            return sum + expense.Amount;
        //        }, 0);
        //        if ($scope.expensePeriodList.length > 0 || $scope.revenuePeriodList.length > 0) {
        //            setLineList($scope.fiscalYearList, $scope.expensePeriodList, $scope.revenuePeriodList);
        //            //createLineChart();
        //        }
        //    });
        //});

        //$http({
        //    method: 'POST',
        //    url: 'accounts/ExpenseDashboard/PeriodWiseExpenseBarChart',
        //    data: {
        //        'factDate': $scope.expFactDate.factDate,
        //        'fromDate': $scope.dateRange.fromDate,
        //        'toDate': $scope.dateRange.toDate
        //    },
        //    dataType: 'JSON'
        //}).then(function successCallback(response) {
        //    $scope.BudgetedExpenseList = response.data;


        //    $http({
        //        method: 'POST',
        //        url: 'accounts/ExpenseDashboard/PeriodWiseRevenueBarChart',
        //        data: {
        //            'factDate': $scope.expFactDate.factDate,
        //            'fromDate': $scope.dateRange.fromDate,
        //            'toDate': $scope.dateRange.toDate
        //        },
        //        dataType: 'JSON'
        //    }).then(function successCallback(response) {
        //        $scope.BudgetedRevenueList = response.data;

        //        setPeriodicBudgetList($scope.BudgetedExpenseList, $scope.BudgetedRevenueList);
        //        createPeriodicalBudgetedBarChart();
        //    });


        //});


        //$http({
        //    method: 'POST',
        //    url: 'accounts/ExpenseDashboard/MonthlyExpenseVSBudgetBarChart',
        //    data: {
        //        'factDate': $scope.expFactDate.factDate,
        //        'fromDate': $scope.dateRange.fromDate,
        //        'toDate': $scope.dateRange.toDate
        //    },
        //    dataType: 'JSON'
        //}).then(function successCallback(response) {
        //    $scope.MonthlyExpenseVSBudget = response.data;


        //    $http({
        //        method: 'POST',
        //        url: 'accounts/ExpenseDashboard/MonthlyRevenueVSBudgetBarChart',
        //        data: {
        //            'factDate': $scope.expFactDate.factDate,
        //            'fromDate': $scope.dateRange.fromDate,
        //            'toDate': $scope.dateRange.toDate
        //        },
        //        dataType: 'JSON'
        //    }).then(function successCallback(response) {
        //        $scope.MonthlyRevenueVSBudget = response.data;

        //        setMonthlyBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
        //        createLineChart();
        //    });
        //});
        //$http({
        //    method: 'POST',
        //    url: 'accounts/ExpenseDashboard/PeriodExpenseVSBudgetBarChart',
        //    data: {
        //        'factDate': $scope.expFactDate.factDate,
        //        'fromDate': $scope.dateRange.fromDate,
        //        'toDate': $scope.dateRange.toDate
        //    },
        //    dataType: 'JSON'
        //}).then(function successCallback(response) {
        //    $scope.MonthlyExpenseVSBudget = response.data;


        //    $http({
        //        method: 'POST',
        //        url: 'accounts/ExpenseDashboard/PeriodRevenueVSBudgetBarChart',
        //        data: {
        //            'factDate': $scope.expFactDate.factDate,
        //            'fromDate': $scope.dateRange.fromDate,
        //            'toDate': $scope.dateRange.toDate
        //        },
        //        dataType: 'JSON'
        //    }).then(function successCallback(response) {
        //        $scope.MonthlyRevenueVSBudget = response.data;

        //        setPeriodWiseBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
        //        createPeriodBarChart();
        //    });
        //});
       
    };


    $scope.GetCompanyInformation = function () {

        $http({

            method: 'GET',

            url: 'Accounts/ExpenseDashboard/GetCompanyInformation/',

            //params: { 'dateType': $scope.expFactDate.factDate, 'itemType': 0 },

            dataType: 'JSON'

        }).then(function successCallback(response) {

            $scope.companyInformation = response.data;
            $scope.BaseCurrencyCode = $scope.companyInformation[0].BaseCurrencyCode;
        });
    };

  //  $scope.GetCompanyInformation();
    $scope.GetCompanyGroupData = function () {
        $scope.GetDelayList();
  // $scope.GetExpenseList();
        //$http({

        //    method: 'GET',

        //    url: 'Products/InventoryDashboard/GetVoucherLatestDate/',

        //    params: { 'dateType': $scope.expFactDate.factDate, 'itemType': 0 },

        //    dataType: 'JSON'

        //}).then(function successCallback(response) {

        //    $scope.getFromDate = response.data;

        //    $scope.dateRange.fromDate = $filter("dateFiltering")($scope.getFromDate[0].PostingDate);

        //    $scope.dateRange.toDate = $filter("dateFiltering")($scope.getFromDate[0].PostingDate);
        //    //$scope.GetFiscalYear();
            
        //});
    };

    
    $scope.GetCompanyGroupData();
    $scope.factDateChange = function () {

        $http({

            method: 'GET',

            url: 'Accounts/ExpenseDashboard/GetVoucherLatestDate/',

            params: { 'dateType': $scope.expFactDate.factDate, 'itemType': 0 },

            dataType: 'JSON'

        }).then(function successCallback(response) {

            $scope.getFromDate = response.data;

            $scope.dateRange.fromDate = $filter("dateFiltering")($scope.getFromDate[0].PostingDate);

            $scope.dateRange.toDate = $filter("dateFiltering")($scope.getFromDate[0].PostingDate);
            $scope.GetFiscalYear();
            $scope.dFunction($filter("dateFiltering")($scope.getFromDate[0].PostingDate), $filter("dateFiltering")($scope.getFromDate[0].PostingDate));
        });
    };

    $scope.GetFiscalYear = function () {
        $scope.fiscalYearList = [];
        $http({
            method: 'GET',
            url: 'Accounts/ExpenseDashboard/GetFiscalYearForBarChart/',
            params: { 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate },
            dataType: 'JSON'

        }).then(function successCallback(response) {
            $scope.fiscalYearList = response.data;
        });
    };
    
    
    $scope.valuenumber=function(data)
    {
        //debugger;
       
        $scope.data;
    }

    $scope.AssetStatus = 'Regular';
    $scope.Isregulardata = true;
    $scope.Isregular = function () {
        //debugger;
        if ($scope.Isregulardata) {
            $scope.AssetStatus = 'Regular';
        }
        else
            $scope.AssetStatus = 'Non-Regular';
        $scope.report.IsAsset = false;
    }
    

    $scope.PoPopUp = function (data) {
        //debugger;
        angular.element(document.querySelector('#DetailModal')).modal('show');
    }
    $scope.MaterialGroupList = [];
    $scope.GetBudgetWiseExpenseDetailJS = function (data, days, periodType) {
        //debugger;
       // $scope.headerstatus = data.Category;
        $scope.days = "";
        if (days === '30') {
            $scope.days = '30';
        }
        else if (days === '45') {
            $scope.days = '45';
        }
        else if (days === '60') {
            $scope.days = '60';
        }
        else if (days === '120') {
            $scope.days = '120';
        }
        else if (days === '120') {
            $scope.days = '120';
        }
        else if (days === '365') {
            $scope.days = '365';
        }
        else if (days === '365') {
            $scope.days = '365';
        }
        else if (days === '9000000') {
            $scope.days = '9000000';
        }
        
            $http({
                method: 'POST',
                url: 'Products/InventoryDashboard/MaterialAgeingMGDataByType',
                data: {
                    'Id': data.MaterialTypeId,
                    'days': $scope.days,
                    'companyId': data.CompanyId,
                    'PlantId': data.PlantId,
                    'ValueOrNumber': $scope.Isregulardata,
                    'IsAsset': $scope.report.IsAsset,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.MaterialGroupList = response.data;
                angular.element(document.querySelector('#DetailModal')).modal('show');
            });
    };
    $scope.MaterialList = [];
    $scope.GetGroupWiseMaterialDetailJS = function ($event, data, days, periodType) {
        //debugger;
        var x = $event;
        var Id = x.data.MaterialGroupId;
        $scope.days = "";
        if (days === '30') {
            $scope.days = '30';
        }
        else if (days === '45') {
            $scope.days = '45';
        }
        else if (days === '60') {
            $scope.days = '60';
        }
        else if (days === '120') {
            $scope.days = '120';
        }
        else if (days === '120') {
            $scope.days = '120';
        }
        else if (days === '365') {
            $scope.days = '365';
        }
        else if (days === '365') {
            $scope.days = '365';
        }
        else if (days === '9000000') {
            $scope.days = '9000000';
        }

        $http({
            method: 'POST',
            url: 'Products/InventoryDashboard/MaterialAgeingMaterialataByMG',
            data: {
                'Id': Id,
                'days': $scope.days,
                'companyId': x.data.CompanyId,
                'PlantId': x.data.PlantId,
                'ValueOrNumber': $scope.Isregulardata,
                'IsAsset': $scope.report.IsAsset,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialList = response.data;
            angular.element(document.querySelector('#DetailModalMaterial')).modal('show');
        });
    };
    $scope.ArticleList = [];
    $scope.GetMaterialWiseArticleDetailJS = function ($event, data, days, periodType) {
        //debugger;
        var x = $event;
        var Id = x.data.Id;
        $scope.days = "";
        if (days === '30') {
            $scope.days = '30';
        }
        else if (days === '45') {
            $scope.days = '45';
        }
        else if (days === '60') {
            $scope.days = '60';
        }
        else if (days === '120') {
            $scope.days = '120';
        }
        else if (days === '120') {
            $scope.days = '120';
        }
        else if (days === '365') {
            $scope.days = '365';
        }
        else if (days === '365') {
            $scope.days = '365';
        }
        else if (days === '9000000') {
            $scope.days = '9000000';
        }

        $http({
            method: 'POST',
            url: 'Products/InventoryDashboard/MaterialAgeingArticleDataByMaterial',
            data: {
                'Id': Id,
                'days': $scope.days,
                'companyId': x.data.CompanyId,
                'PlantId': x.data.PlantId,
                'ValueOrNumber': $scope.Isregulardata,
                'IsAsset': $scope.report.IsAsset,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ArticleList = response.data;
            angular.element(document.querySelector('#DetailModalArticle')).modal('show');
        });
    };
    //#endregion

    $scope.Print1 = function () {
        var gridObj = $("#Group").data("ejGrid");
        var data = gridObj.getFilteredRecords();
        if (data.length === 0) {
            data = gridObj.model.dataSource();
            $scope.plantvisible = 'visible';
        }
        //var data = gridObj.model.dataSource();//columns
        //var data = gridObj.model.columns;//columns
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': data }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');
            }
            else {
                window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };
    $scope.Print2 = function () {
        var gridObj = $("#Material").data("ejGrid");
        var data = gridObj.getFilteredRecords();
        if (data.length === 0) {
            data = gridObj.model.dataSource();
            $scope.plantvisible = 'visible';
        }
        var data = gridObj.model.dataSource();//columns
        //var data = gridObj.model.columns;//columns
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': data }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');
            }
            else {
                window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };
    $scope.Print3 = function () {
        var gridObj = $("#Article").data("ejGrid");
        var data = gridObj.getFilteredRecords();
        if (data.length === 0) {
            data = gridObj.model.dataSource();
            $scope.plantvisible = 'visible';
        }
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': data }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');
            }
            else {
                window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };

   
}


