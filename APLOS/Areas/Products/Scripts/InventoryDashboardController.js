'use strict';
InventoryDashboardController.$inject = ['cboService', 'commonMessage', '$window', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function InventoryDashboardController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
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
    $scope.path = 'Products/InventoryDashboard/';

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
    $scope.chartExpenseList = [];
    $scope.chartRevenueList = [];
    $scope.chartDelayExpenseList = [];
    $scope.chartDelayRevenueList = [];
    var BudgetedBarChart;

    $scope.ExpenseList = [];
    $scope.ExpenseListGraph = [];
    
    $scope.RevenueList = [];
    $scope.ExpenseListTotal = 0;
    $scope.RevenueListTotal = 0;

    $scope.chartList = [];
    $scope.ExpenseList = [];
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
    //GetCompanyInformation

    //---------------Update 5-September-2019----------------



    //--------------End of Update 5-September-2019

    $scope.budgetedExpenseList = [];

    $scope.budgetedExpenseChartLabel = [];

    var unique_array = [];

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

    $scope.setIndex = function (x) {
        for (var i = 0; i < $scope.ColList.length; i++) {
            if ($scope.ColList[i].Sequence === $scope.index) {
                $scope.ColList[i].Id = x.CompanyId;
                $scope.ColList[i].Text = x.UId;
                $scope.ColList[i].Name = x.ColumnName;
            }
        }
    };
    // console.log("ColList Length", $scope.ColList[$scope.ColList.length]);

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
                url: 'Products/InventoryDashboard/DelayList',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate,
                    'groupName': $scope.groupName

                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                setList(response.data);
                $scope.index = -1;
                $scope.stIndex = $scope.index - 1;
            });
            
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
        $scope.CompanyId = $scope.CompanyId;
        if ($scope.index + 2 < $scope.ColList.length) {
            $http({
                method: 'POST',
                url: 'Products/InventoryDashboard/DymnamicExpenseList/',
                data: {
                    'ChartColumnList': $scope.ColList,
                    'seq': $scope.index,
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate,
                    'CompanyWisePlant': $scope.dateRange.toDate,
                    'CompanyId': data.CompanyId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.DDList = response.data;
                setList(response.data);
                $scope.index += 1;
                $scope.stIndex = $scope.index - 1;

            });
        }
    };

    $scope.exceptionList = {
        entryPeriod: null,
        postingPeriod: null,
        normalPostedAmout: null,
        delayPostedAmount: null
    };
    $scope.listForChart = [];

    function setList(list) {
        //debugger;
        $scope.ExpenseList = [];
        $scope.ExpenseList = list;
        $scope.chartLabel = [];
        $scope.chartList = [];
        $scope.stackingGraph();
       
    }
    function setListGraph(list1) {
        //debugger;
        $scope.ExpenseListGraph = [];      
        $scope.ExpenseListGraph = list1;
        $scope.stackingGraph1();
        $scope.chartLabel = [];
        $scope.chartList = [];

    }
    $scope.groupName = 'groupName';
    


    //#region Company Plant Filter

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
                  
                    columns: [
                        { headerText: "Company", field: "Company", width: 30 },
                        { headerText: "Plant", field: "PlantName", width: 30 } ]


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

    //#endregion




    $scope.GetDelayList = function () {
        //debugger;
        //$scope.ShowLoader = true;       
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
        $scope.expensePeriodList = [];
        $scope.revenuePeriodList = [];
        $scope.ExpenseListTotal = null;
        $scope.RevenueListTotal = null;

        $http({
            method: 'POST',
            url: 'Products/InventoryDashboard/DelayList',
            data: {
                'factDate': $scope.expFactDate.factDate,
                'fromDate': $scope.dateRange.fromDate,
                'toDate': $scope.dateRange.toDate,
                'groupName': $scope.groupName,
                'Companywiseplantdata': $scope.Companywiseplantdata,
                'ValueOrNumber': $scope.data,
                'queryString': queryString,
                'queryStringProcess': queryStringProcess,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setList(response.data);
            createColList();
           
           
        });
        $scope.GetExpenseListGraph();
      

    };
    $scope.GetExpenseListGraph = function () {
        //debugger;
        //$scope.ShowLoader = true;       
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
        $scope.expensePeriodList = [];
        $scope.revenuePeriodList = [];
        $scope.ExpenseListTotal = null;
        $scope.RevenueListTotal = null;

        $http({
            method: 'POST',
            url: 'Products/InventoryDashboard/DelayListGraph',
            data: {
                'factDate': $scope.expFactDate.factDate,
                'fromDate': $scope.dateRange.fromDate,
                'toDate': $scope.dateRange.toDate,
                'groupName': $scope.groupName,
                'Companywiseplantdata': $scope.Companywiseplantdata,
                'ValueOrNumber': $scope.data,
                'queryString': queryString,
                'queryStringProcess': queryStringProcess,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setListGraph(response.data);
            createColList();
            //$scope.graph();
            
        });

    };


    $scope.dFunction = function () {
        //debugger;
        if (new Date($scope.dateRange.toDate) < new Date($scope.dateRange.fromDate)) {
            throw ShowResult("From date can not be greater then to date", 'failure');
        }
        else {
            $scope.ExpenseList = [];
            //$scope.ShowLoader = true;       
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
                //queryStringForSum = [];
                var queryString1 = [];
                var arrqueryStringProcess = [];
                var arrqueryStringSkill = [];


                var value = "";
                var queryString = "''";
                //$scope.queryString1 = "''";
                var queryStringCaption = "''";
                var queryStringProcess = "''";
                var queryStringSkill = "''";
                var queryStringOperationCode = "''";

                var queryStringGrouping = "''";
                var queryStringMachineCategory = "''";
                var queryStringMachineSubCategoryCode = "''";
                var queryStringCaption = "''";
                var queryStringOperationCategoryId = "''";

                var queryStringOnRoll = "''";
                var queryStringTotalPresent = "0";
                var queryStringOnRollShort = "0";
                var queryStringOnRollExcess = "0";
                var queryStringPresentShort = "0";
                var queryStringPresentExcess = "0";
                var skillList = [];
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

                $scope.expensePeriodList = [];
                $scope.ExpenseListTotal = 0;
                $scope.revenuePeriodList = [];
                $scope.RevenueListTotal = 0;
                $scope.GetFiscalYear();
                $http({
                    method: 'POST',
                    url: 'Products/InventoryDashboard/DelayList/',
                    data: {
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate,
                        'queryString': queryString,
                        'queryStringProcess': queryStringProcess,
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    setList(response.data);
                    if ($scope.ColList.length === 0) {
                        createColList();
                    }
                    else {
                        $scope.index = -1;
                        $scope.stIndex = $scope.index - 1;
                    }


                });
                $scope.dFunctionGraph();
       
            }
           
        }
    };
    $scope.dFunctionGraph = function () {
        //debugger;
        if (new Date($scope.dateRange.toDate) < new Date($scope.dateRange.fromDate)) {
            throw ShowResult("From date can not be greater then to date", 'failure');
        }
        else {
            //$scope.ShowLoader = true;       
            var obj = $("#Grid2").ejGrid("instance");
            var sd = obj.getFilteredRecords();
            if (sd.length == 0) {
                sd = obj.model.dataSource;
            }
            var arr = [];
            var queryString = [];
            //queryStringForSum = [];
            var queryString1 = [];
            var arrqueryStringProcess = [];
            var arrqueryStringSkill = [];
            

            var value = "";
            var queryString = "''";
            //$scope.queryString1 = "''";
            var queryStringCaption = "''";
            var queryStringProcess = "''";
            var queryStringSkill = "''";
            var queryStringOperationCode = "''";

            var queryStringGrouping = "''";
            var queryStringMachineCategory = "''";
            var queryStringMachineSubCategoryCode = "''";
            var queryStringCaption = "''";
            var queryStringOperationCategoryId = "''";

            var queryStringOnRoll = "''";
            var queryStringTotalPresent = "0";
            var queryStringOnRollShort = "0";
            var queryStringOnRollExcess = "0";
            var queryStringPresentShort = "0";
            var queryStringPresentExcess = "0";
            var skillList = [];
            var index = 0;
            for (var i = 0; i < sd.length; i++) {
                var x = sd[i];
                //var yOnRoll = x["OnRoll"];
                //var yTotalPresent = x["TotalPresent"];
                //var yRollShort = x["RollShort"];
                //var yOnRollExcess = x["OnRollExcess"];
                //var yPresentShort = x["PresentShort"];
                //var yPresentExcess = x["PresentExcess"];

                var yEntityName = x["CompanyId"];
                var yProcess = x["PlantId"];
                //var ySkill = x["SkillId"];
                //var yOperationCode = x["OperationCode"];

                ////var ySkillId = x["SkillId"];
                //var yGrouping = x["SkillGroupId"];

                //var yMachineCategory = x["MachineCategoryId"];
                //var yMachineSubCategoryCode = x["MachineSubCategoryId"];
                //var yOperationCategoryId = x["OperationCategoryId"];

                ////var yCaption = x["MachineSubCategoryCode"];
                //var yOnRoll = x["OnRoll"];
                //var yTotalPresent = x["TotalPresent"];
                //var yRollShort = x["OnRollShort"];
                //var yOnRollExcess = x["OnRollExcess"];
                //var yPresentShort = x["PresentShort"];
                //var yPresentExcess = x["PresentExcess"];


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

                //if (!arrqueryStringSkill.includes(ySkill)) {
                //    queryStringSkill += ",'" + ySkill + "'";
                //    arrqueryStringSkill.push(ySkill);

                //}
                //if (!arrqueryStringOperationCode.includes(yOperationCode)) {
                //    queryStringOperationCode += ",'" + yOperationCode + "'";
                //    arrqueryStringOperationCode.push(yOperationCode);

                //}

                //if (!arrqueryStringGrouping.includes(yGrouping)) {
                //    queryStringGrouping += ",'" + yGrouping + "'";
                //    arrqueryStringGrouping.push(yGrouping);
                //}
                //if (!arrqueryStringMachineCategory.includes(yMachineCategory)) {
                //    queryStringMachineCategory += ",'" + yMachineCategory + "'";
                //    arrqueryStringMachineCategory.push(yMachineCategory);
                //}
                //if (!arrqueryStringMachineSubCategoryCode.includes(yMachineSubCategoryCode)) {
                //    queryStringMachineSubCategoryCode += ",'" + yMachineSubCategoryCode + "'";
                //    arrqueryStringMachineSubCategoryCode.push(yMachineSubCategoryCode);
                //}
                //if (!arrqueryStringCaption.includes(yposition)) {
                //    queryStringCaption += ",'" + yposition + "'";
                //    arrqueryStringCaption.push(yposition);
                //}
                //if (!arrqueryStringOperationCategoryId.includes(yOperationCategoryId)) {
                //    queryStringOperationCategoryId += ",'" + yOperationCategoryId + "'";
                //    arrqueryStringOperationCategoryId.push(yOperationCategoryId);
                //}



                //if (!arrqueryStringOnRoll.includes(yOnRoll)) {
                //    queryStringOnRoll += ",'" + yOnRoll + "'";
                //    arrqueryStringOnRoll.push(yOnRoll);
                //}

                //if (!arrqueryStringTotalPresent.includes(yTotalPresent)) {
                //    queryStringTotalPresent += "," + yTotalPresent + "";
                //    arrqueryStringTotalPresent.push(yTotalPresent);
                //}
                //if (!arrqueryStringOnRollShort.includes(yRollShort)) {
                //    queryStringOnRollShort += "," + yRollShort + "";
                //    arrqueryStringOnRollShort.push(yRollShort);
                //}
                //if (!arrqueryStringOnRollExcess.includes(yOnRollExcess)) {
                //    queryStringOnRollExcess += "," + yOnRollExcess + "";
                //    arrqueryStringOnRollExcess.push(yOnRollExcess);
                //}
                //if (!arrqueryStringPresentShort.includes(yPresentShort)) {
                //    queryStringPresentShort += "," + yPresentShort + "";
                //    arrqueryStringPresentShort.push(yPresentShort);
                //}
                //if (!arrqueryStringPresentExcess.includes(yPresentExcess)) {
                //    queryStringPresentExcess += "," + yPresentExcess + "";
                //    arrqueryStringPresentExcess.push(yPresentExcess);
                //}

            }






            $scope.expensePeriodList = [];
            $scope.ExpenseListTotal = 0;
            $scope.revenuePeriodList = [];
            $scope.RevenueListTotal = 0;
            $scope.GetFiscalYear();
            $http({
                method: 'POST',
                url: 'Products/InventoryDashboard/DelayListGraph/',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate,
                    'queryString': queryString,
                    'queryStringProcess': queryStringProcess,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                setListGraph(response.data);
                if ($scope.ColList.length === 0) {
                    createColList();
                }
                else {
                    $scope.index = -1;
                    $scope.stIndex = $scope.index - 1;
                }


            });
           
        }
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
    $scope.data;
    //alert('dd'+$scope.data);
    $scope.valuenumber = function (data) {
        debugger;

        $scope.data;
        $scope.GetExpenseListGraph();
    }

    $scope.PoPopUp = function (data) {
        //debugger;
        angular.element(document.querySelector('#DetailModal')).modal('show');
    }
    $scope.DetailList = [];
    $scope.GetBudgetWiseExpenseDetailJS = function (data, days, periodType) {
        debugger;
        $scope.headerstatus = data.Category;
     
        if (data.Category === 'Pending For GRN') {
            data.Total3Value = 0;
            data.Total5Value = 0;
            data.Total10Value = 0;
            data.Total15Value = 0;
            data.Total20Value = 0;
            data.Total25Value = 0;
            data.Total30Value = 0;
            data.Total31Value = 0;
            data.Total32Value = 0;
           
        }
        if (data.Category === 'Pending  Issue Request  For Approval') {
            data.Total3Value = 0;
            data.Total5Value = 0;
            data.Total10Value = 0;
            data.Total15Value = 0;
            data.Total20Value = 0;
            data.Total25Value = 0;
            data.Total30Value = 0;
            data.Total31Value = 0;
            data.Total32Value = 0;

        }
        $scope.days = "";
        if (days === '3') {
            if (data.ThreeDaysCount === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            if (data.Total3Value === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            else {
                $scope.days = '3';
            }
        }
        else if (days === '5') {
            if (data.FiveDaysCount === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            if (data.Total5Value === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            else {
                $scope.days = '5';

            }
        }
        else if (days === '10') {
            if (data.TenDaysCount === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            if (data.Total10Value === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            else {
                $scope.days = '10';
            }
        }
        else if (days === '15') {
            if (data.FifteenDaysCount === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            if (data.Total15Value === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            else {
                $scope.days = '15';
            }
        }
        else if (days === '20') {
            if (data.TwentyFiveyDaysCount === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            if (data.Total20Value === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            else {
                $scope.days = '20';
            }
        }
        else if (days === '25') {
            if (data.TwentyFiveyDaysCount === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            if (data.Total25Value === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            else {
                $scope.days = '25';
            }
        }
        else if (days === '30') {
            if (data.ThirtyDaysCount === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            if (data.Total30Value === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            else {
                $scope.days = '30';
            }
        }
        else if (days === '31') {
            if (data.GraterThirtyDaysCount === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            if (data.Total31Value === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            else {
                $scope.days = '31';
            }
        }
        else if (days === '32') {
            if (data.AllCount === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            if (data.Total32Value === null) {
                ShowResult("Information is not available", 'failure');
                return false;
            }
            else {
                $scope.days = '32';
            }
        }
        $scope.BudgetWiseExpenseDetailList = [];
        $http({
            method: 'POST',
            url: 'Products/InventoryDashboard/ModalCompanyWiseDetails',
            data: {
                'Category': data.Category,
                'days': $scope.days,
                'companyId': data.CompanyId,
                'PlantId': data.PlantId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DetailList = response.data;
            angular.element(document.querySelector('#DetailModal')).modal('show');
        });
       // }
    };



    //$scope.sum=function() {
    //    var row = 0,
    //        col = 0,
    //        ncol = 0;
    //    var sum;
    //    // sum by row
    //    $("tr").each(function (rowindex) {
    //        sum = 0;
    //        col = 0;
    //        $(this).find("td").each(function (colindex) {
    //            col++;
    //            newval = $(this).find("input").val();
    //            if (isNaN(newval)) {
    //                $(this).html(sum);
    //                if (col > ncol) {
    //                    ncol = col - 1
    //                }
    //            } else {
    //                sum += parseInt(newval);
    //            }
    //        });
    //    });

    //    // sum by col
    //    for (col = 1; col < ncol + 1; col++) {
    //        console.log("column: " + col);
    //        sum = 0;
    //        $("tr").each(function (rowindex) {
    //            $(this).find("td:nth-child(" + col + ")").each(function (rowindex) {
    //                newval = $(this).find("input").val();
    //                console.log(newval);
    //                if (isNaN(newval)) {
    //                    $(this).html(sum);
    //                } else {
    //                    sum += parseInt(newval);
    //                }
    //            });
    //        });
    //    }
    //}
    //$scope.sum();

    //#endregion


    $scope.valuePassInDelModal = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.Id = data.RequisitionNo;
        $scope.message = 'Are you sure want to InActive this Requisition?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
    };
    $scope.UpdateInActive = function (x) {
        //debugger;
        try {
            $http({
                method: 'POST',
                url: 'Products/InventoryDashboard/UpdateInActive?ReqId=' + $scope.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetExpenseList();

                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            throw e;
        }
    };

    $scope.valuePassInDelModal1 = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.Id = data.PONo;
        $scope.message = 'Are you sure want to Close this PO?';
        angular.element(document.querySelector('#removerPopUp1')).modal('show');
    };
    $scope.UpdateInActivePO = function (x) {
        //debugger;

        try {
            $http({
                method: 'POST',
                url: 'Products/InventoryDashboard/UpdateInActivePO?POId=' + $scope.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');


                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            throw e;
        }
    };


    //#region  Req Detail
    $scope.lst = [];
    $scope.ReqListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/Requisition/GetAllReqdataDetails'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            window.lst = response.data;

        });
    }
    $scope.ReqListDetails();


    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["RequisitionNo"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("MaterialReqqusitionMasterId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            //columns: ["MaterialGroupName", "MaterialName", "ArticleName", "SKU1", "SKU2", "SKU3","MaterialDetail", "TransactionQty", "TransactionUoM", "EstimatedRate", "CurrencyName", "TotalAmount" ]
            //
            columns: [{ field: "BudgetType", headerText: "Budget Type", width: 100 },
            { field: "ActivityName", headerText: "Activity Name", width: 150 },
            { field: "MaterialGroupName", headerText: "Material Group", width: 100 },
            { field: "MaterialName", headerText: "Material Name", width: 150 },
            { field: "ArticleName", headerText: "Article Name", width: 150 },
            { field: "SKU1", headerText: "SKU1", width: 50 },
            { field: "SKU2", headerText: "SKU2", width: 50 },
            { field: "SKU3", headerText: "SKU3", width: 50 },
            { field: "MaterialDetail", headerText: "MaterialDetail", width: 80 },
            { field: "TransactionQty", headerText: "Qty", width: 70 },
            { field: "PORaisedQty", headerText: "PORaisedQty", width: 80 },
            //{ field: "GRNRcvQty", headerText: "GRNRcvQty", width: 80 },                
            { field: "Balance", headerText: "Balance", width: 80 },
            { field: "TransactionUoM", headerText: "UoM", width: 50 },
            { field: "EstimatedRate", headerText: "Estimated Rate", width: 120 },
                { field: "CurrencyName", headerText: "Currency Name", width: 120 },
            { field: "TotalAmount", headerText: "Amount", width: 100 },           
            { field: "RequisitionStatus", headerText: "Status", width: 80 }
            ],
            recordDoubleClick: rowSelected
            
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    //#endregion

    //#region  PO Detail
    //$scope.lst = [];
    $scope.POListDetails = function () {
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/GetInventoryMaterialListPoByReqDetail'
        }).then(function successCallback(response) {
            $scope.lstPO = response.data;
            window.lstPO = response.data;

        });
    }
    $scope.POListDetails();

    $scope.data1 = $scope.lstPO;
    $scope.detailTempPO = "#tabGridContentsPO";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgridPO = function detailGridData(e) {
        //debugger;
        var filteredData = e.data["PONo"];
        var data = ej.DataManager(window.lstPO).executeLocal(ej.Query().where("POmasterId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGridPO").ejGrid({
            dataSource: data,
            columns: [{ field: "MaterialGroupName", headerText: "Material Group", width: 100 },
            { field: "MaterialName", headerText: "Material Name", width: 100 },
            { field: "Article", headerText: "Article", width: 100 },
            { field: "Sku1", headerText: "Sku1", width: 80 },
            { field: "Sku2", headerText: "Sku2", width: 80 },
            { field: "Sku3", headerText: "Sku3", width: 80 },
            { field: "MaterialDetail", headerText: "Material Detail", width: 80 },
            { field: "TransactionQty", headerText: "Qty", width: 80 },
            { field: "GRNRcvQty", headerText: "GRN Rcv Qty", width: 100 },
            { field: "Balance", headerText: "Balance", width: 80 },
            { field: "TransactionUoM", headerText: "UoM", width: 80 },
            { field: "TransactionRate", headerText: "Transaction Rate", width: 100 },
            { field: "CurrencyName", headerText: "Currency Name", width: 100 },
            { field: "TotalAmount", headerText: "Amount", width: 100 },
            { field: "RequisitionId", headerText: "Requisition No", width: 100 }],
            recordDoubleClick: rowSelectedforGRN
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    //#endregion

    //#region GRN Detail
    $scope.lstGRN = [];
    $scope.GRNListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/GoodsReceiveNote/GRNDetailsData'
        }).then(function successCallback(response) {
            $scope.lstGRN = response.data;
            //$scope.detailgrid($scope.lst);
            window.lstGRN = response.data;

        });
    }
    $scope.GRNListDetails();


    $scope.data1 = $scope.lst;
    $scope.detailTempGRN = "#tabGridContentsGRN";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgridGRN = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["GRNNo"];
        var data = ej.DataManager(window.lstGRN).executeLocal(ej.Query().where("InventoryReceiveId", "equal", parseInt(filteredData), true).take(105));
        e.detailsElement.find("#detailGridGRN").ejGrid({

            dataSource: data,
            columns: ["MaterialGroupName", "MaterialName", "Article", "SKU1", "SKU2", "SKU3", "MaterialDetail", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalMaterialTranAmount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    //#endregion
    //#region Inventory Issue Material
    $scope.lstEXPBooking = [];
    $scope.GRNListDetailsEXPBooking = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/InventoryIssue/MaterialIssueDetailsData'
        }).then(function successCallback(response) {
            $scope.lstEXPBooking = response.data;
            //$scope.detailgrid($scope.lst);
            window.lstEXPBooking = response.data;

        });
    }
    //$scope.GRNListDetailsEXPBooking();


    $scope.data1 = $scope.lstEXPBooking;
    $scope.detailTempEXPBooking = "#tabGridContentsEXPBooking";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgridEXPBooking = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["IssueNo"];
        var data = ej.DataManager(window.lstEXPBooking).executeLocal(ej.Query().where("IssueNo", "equal", parseInt(filteredData), true).take(200));
        e.detailsElement.find("#detailGridEXPBooking").ejGrid({

            dataSource: data,
            columns: ["CostCenter", "Materials", "Article", "SKU1", "SKU2", "SKU3", "Qty", "UOM", "TransactionRate", "CurrencyName", "TotalMaterialTranAmount", "Comments"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }


    //#endregion


    $scope.DetailSubModal = [];
    function rowSelected(args) {
        //debugger;
        this.preventClick = true;
        //alert("double click");      
       // $scope.operationCode = args.data.OperationCode;
        if (args.data.PORaisedQty === 0) {

        }
        else {
            $http({
                method: 'GET',
                url: 'Products/InventoryDashboard/GetReqForPoDetail?Id=' + args.data.Id
            }).then(function successCallback(response) {
                $scope.DetailSubModal = response.data;
              

            });
            angular.element(document.querySelector('#DetailSubModal')).modal('show');
        }
    }
    $scope.DetailSubModal1 = [];
    function rowSelectedforGRN(args) {
        //debugger;
        this.preventClick = true;
        //alert("double click");      
        // $scope.operationCode = args.data.OperationCode;
        if (args.data.GRNRcvQty === 0) {           
        }
        else {
            $http({
                method: 'GET',
                url: 'Products/InventoryDashboard/GetPOForGRNDetail?Id=' + args.data.InventoryReceiveDetailId
            }).then(function successCallback(response) {
                $scope.DetailSubModal1 = response.data;


            });
            angular.element(document.querySelector('#DetailSubSubModal')).modal('show');
        }
    }
   
    $scope.GRNDetails=function (args) {
        //debugger;
        this.preventClick = true;
        //alert("double click");      
        // $scope.operationCode = args.data.OperationCode;
        if (args.data.GRNRcvQty === 0) {

        }
        else {
            $http({
                method: 'GET',
                url: 'Products/InventoryDashboard/GetPOForGRNDetail?Id=' + args.data.PoDetailId
            }).then(function successCallback(response) {
                $scope.DetailSubModal1 = response.data;


            });
            angular.element(document.querySelector('#DetailSubSubModal')).modal('show');
        }
    }

    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObjAllTabPrint = $(x).data("ejGrid");
        var data = gridObjAllTabPrint.getSelectedRecords()[0];
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.RequisitionNo;

    };
    $scope.AllTabPOPrint = function (z) {

        var x = "#" + z;
        var gridObjAllTabPOPrint = $(x).data("ejGrid");
        var data = gridObjAllTabPOPrint.getSelectedRecords()[0];
        location.href = "Products/PurchaseOrder/GePurchaseOrderReport?purchaseOrderId=" + data.Id;
        //location.href = "Products/PurchaseOrder/GePurchaseOrderReportByReq?purchaseOrderId=" + data.Id;

    };
    //#endregion

//    $("#container").ejChart({
//        primaryXAxis:
//        {
//           // title: { text: 'Countries' },
//            //Rotating labels by 45 degrees
//            //labelRotation: 45,
//            font: {
//                color: "transparent"
//            }

//        }
//});

  
    
    
    $("#container1").ejChart({
        primaryXAxis:
        {
            // title: { text: 'Countries' },
            //Rotating labels by 45 degrees
           labelRotation: 45
            //font: {
            //    color: "transparent"
            //}

        }//,
    
    });   
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    //$scope.exportgriddataUrl = 'GridReports/ExcelExportJson';
    //$scope.downloadgriddataUrl = 'GridReports/Download';
    //$scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';


    //$scope.PrintRequisition = function () {
    //    var gridObj = $("#PendingRequisition").data("ejGrid");
    //    var data = gridObj.model.dataSource();//columns
    //    //var data = gridObj.model.columns;//columns

    //    $http({
    //        method: 'POST',
    //        url: $scope.exportgriddataUrl,
    //        data: { 'data': data }
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');
    //        }
    //        else {
    //            window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
    //        }
    //    });
    //};
    $scope.PrintRequisition = function () {
        var gridObj = $("#PendingReqForPO").data("ejGrid");
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
    $scope.PrintRequisition1 = function () {
        var gridObj = $("#PendingRequisitionForApproval").data("ejGrid");
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
    $scope.PrintPO = function () {
        var gridObj = $("#POPendingForApproval").data("ejGrid");
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
    $scope.PrintPOPurchase = function () {
        var gridObj = $("#POPendingForPurchase").data("ejGrid");
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

    $scope.PrintRequisitionDetails = function () {
        try {
            var RequisitionDetailsRow = [];
            for (var i = 0; i < $scope.DetailList.length; i++) {
                if (RequisitionDetailsRow, $scope.DetailList[i].RequisitionNo) {
                    RequisitionDetailsRow.push($scope.DetailList[i].RequisitionNo);
                }
            }
            var file_src = $scope.path + "RequisitionDetailsReport?RequisitionDetailsRow=" + RequisitionDetailsRow;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }



    $scope.POPurchaseId = [];
    $scope.PrintPOPurchaseDetails = function () {
        try {
            var POPurchaseId = [];
            for (var i = 0; i < $scope.DetailList.length; i++) {
                if (POPurchaseId, $scope.DetailList[i].PONo) {
                    POPurchaseId.push($scope.DetailList[i].PONo);
                }
            }
            var file_src = $scope.path + "POPurchaseDetailsReport?POPurchaseDetailsId=" + POPurchaseId;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    

    $scope.PrintPO1 = function () {
        //debugger;
        var gridObj = $("#POPendingForLCTaging").data("ejGrid");
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
    $scope.PrintPO2 = function () {
        var gridObj = $("#POPendingForAcceptance").data("ejGrid");
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
    $scope.PrintPO3 = function () {
        var gridObj = $("#AcceptancePendingForGRN").data("ejGrid");
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
    $scope.PrintPO4 = function () {
        var gridObj = $("#UnTagGateEntry").data("ejGrid");
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
    $scope.PrintPO5 = function () {
        var gridObj = $("#PendingGRNPosting").data("ejGrid");
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
    $scope.PrintPO6 = function () {
        var gridObj = $("#PendingInventoryIssue").data("ejGrid");
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
    $scope.PrintPO7 = function () {
        var gridObj = $("#PendingExpenseBooking").data("ejGrid");
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
    $scope.PrintPO8 = function () {
        var gridObj = $("#InvoicePendingForAcceptance").data("ejGrid");
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
    $scope.PrintPO81 = function () {
        var gridObj = $("#PendingGRNForApproval").data("ejGrid");
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
 
    $scope.Points1Y3 = '';
    $scope.Points1Y5 = '';
    $scope.Points1Y10 = '';
    $scope.Points1Y15 = '';
    $scope.Points1Y20 = '';
    $scope.Points1Y25 = '';
    $scope.Points1Y30 = '';
    $scope.Points1Y31 = '';
    $scope.stackingGraph = function () {
        for (var i = 0; i < $scope.ExpenseList.length; i++) {
            if ($scope.ExpenseList[i].Category === 'Pending Requisition For Approval') {
                $scope.Points1Y3 = $scope.ExpenseList[i].ThreeDaysCount;
                $scope.Points1Y5 = $scope.ExpenseList[i].FiveDaysCount;
                $scope.Points1Y10 = $scope.ExpenseList[i].TenDaysCount;
                $scope.Points1Y15 = $scope.ExpenseList[i].FifteenDaysCount;
                $scope.Points1Y20 = $scope.ExpenseList[i].TweentyDaysCount;
                $scope.Points1Y25 = $scope.ExpenseList[i].TwentyFiveyDaysCount;
                $scope.Points1Y30 = $scope.ExpenseList[i].ThirtyDaysCount;
                $scope.Points1Y31 = $scope.ExpenseList[i].GraterThirtyDaysCount;

            }
            else if ($scope.ExpenseList[i].Category === 'Pending Requisition For PO') {
                $scope.Points2Y3 = $scope.ExpenseList[i].ThreeDaysCount;
                $scope.Points2Y5 = $scope.ExpenseList[i].FiveDaysCount;
                $scope.Points2Y10 = $scope.ExpenseList[i].TenDaysCount;
                $scope.Points2Y15 = $scope.ExpenseList[i].FifteenDaysCount;
                $scope.Points2Y20 = $scope.ExpenseList[i].TweentyDaysCount;
                $scope.Points2Y25 = $scope.ExpenseList[i].TwentyFiveyDaysCount;
                $scope.Points2Y30 = $scope.ExpenseList[i].ThirtyDaysCount;
                $scope.Points2Y31 = $scope.ExpenseList[i].GraterThirtyDaysCount;

            }
            else if ($scope.ExpenseList[i].Category === 'PO Pending For Approval') {

                $scope.Points3Y3 = $scope.ExpenseList[i].ThreeDaysCount;
                $scope.Points3Y5 = $scope.ExpenseList[i].FiveDaysCount;
                $scope.Points3Y10 = $scope.ExpenseList[i].TenDaysCount;
                $scope.Points3Y15 = $scope.ExpenseList[i].FifteenDaysCount;
                $scope.Points3Y20 = $scope.ExpenseList[i].TweentyDaysCount;
                $scope.Points3Y25 = $scope.ExpenseList[i].TwentyFiveyDaysCount;
                $scope.Points3Y30 = $scope.ExpenseList[i].ThirtyDaysCount;
                $scope.Points3Y31 = $scope.ExpenseList[i].GraterThirtyDaysCount;
            }
            else if ($scope.ExpenseList[i].Category === 'PO Pending For Purchase') {
                
                $scope.Points4Y3 = $scope.ExpenseList[i].ThreeDaysCount;
                $scope.Points4Y5 = $scope.ExpenseList[i].FiveDaysCount;
                $scope.Points4Y10 = $scope.ExpenseList[i].TenDaysCount;
                $scope.Points4Y15 = $scope.ExpenseList[i].FifteenDaysCount;
                $scope.Points4Y20 = $scope.ExpenseList[i].TweentyDaysCount;
                $scope.Points4Y25 = $scope.ExpenseList[i].TwentyFiveyDaysCount;
                $scope.Points4Y30 = $scope.ExpenseList[i].ThirtyDaysCount;
                $scope.Points4Y31 = $scope.ExpenseList[i].GraterThirtyDaysCount;
            }
            else if ($scope.ExpenseList[i].Category === 'PO Pending For LC Taging') {
                
                $scope.Points5Y3 = $scope.ExpenseList[i].ThreeDaysCount;
                $scope.Points5Y5 = $scope.ExpenseList[i].FiveDaysCount;
                $scope.Points5Y10 = $scope.ExpenseList[i].TenDaysCount;
                $scope.Points5Y15 = $scope.ExpenseList[i].FifteenDaysCount;
                $scope.Points5Y20 = $scope.ExpenseList[i].TweentyDaysCount;
                $scope.Points5Y25 = $scope.ExpenseList[i].TwentyFiveyDaysCount;
                $scope.Points5Y30 = $scope.ExpenseList[i].ThirtyDaysCount;
                $scope.Points5Y31 = $scope.ExpenseList[i].GraterThirtyDaysCount;
            }
            else if ($scope.ExpenseList[i].Category === 'Invoice Pending For Acceptance') {
                $scope.Points6Y3 = $scope.ExpenseList[i].ThreeDaysCount;
                $scope.Points6Y5 = $scope.ExpenseList[i].FiveDaysCount;
                $scope.Points6Y10 = $scope.ExpenseList[i].TenDaysCount;
                $scope.Points6Y15 = $scope.ExpenseList[i].FifteenDaysCount;
                $scope.Points6Y20 = $scope.ExpenseList[i].TweentyDaysCount;
                $scope.Points6Y25 = $scope.ExpenseList[i].TwentyFiveyDaysCount;
                $scope.Points6Y30 = $scope.ExpenseList[i].ThirtyDaysCount;
                $scope.Points6Y31 = $scope.ExpenseList[i].GraterThirtyDaysCount;
            }
            else if ($scope.ExpenseList[i].Category === 'PO Pending For Acceptance') {
                
                $scope.Points7Y3 = $scope.ExpenseList[i].ThreeDaysCount;
                $scope.Points7Y5 = $scope.ExpenseList[i].FiveDaysCount;
                $scope.Points7Y10 = $scope.ExpenseList[i].TenDaysCount;
                $scope.Points7Y15 = $scope.ExpenseList[i].FifteenDaysCount;
                $scope.Points7Y20 = $scope.ExpenseList[i].TweentyDaysCount;
                $scope.Points7Y25 = $scope.ExpenseList[i].TwentyFiveyDaysCount;
                $scope.Points7Y30 = $scope.ExpenseList[i].ThirtyDaysCount;
                $scope.Points7Y31 = $scope.ExpenseList[i].GraterThirtyDaysCount;
            }
            else if ($scope.ExpenseList[i].Category === 'Acceptance Pending For GRN	') {
                $scope.Points8Y3 = $scope.ExpenseList[i].ThreeDaysCount;
                $scope.Points8Y5 = $scope.ExpenseList[i].FiveDaysCount;
                $scope.Points8Y10 = $scope.ExpenseList[i].TenDaysCount;
                $scope.Points8Y15 = $scope.ExpenseList[i].FifteenDaysCount;
                $scope.Points8Y20 = $scope.ExpenseList[i].TweentyDaysCount;
                $scope.Points8Y25 = $scope.ExpenseList[i].TwentyFiveyDaysCount;
                $scope.Points8Y30 = $scope.ExpenseList[i].ThirtyDaysCount;
                $scope.Points8Y31 = $scope.ExpenseList[i].GraterThirtyDaysCount;
            }
            else if ($scope.ExpenseList[i].Category === 'Pending For GRN') {
                $scope.Points9Y3 = $scope.ExpenseList[i].ThreeDaysCount;
                $scope.Points9Y5 = $scope.ExpenseList[i].FiveDaysCount;
                $scope.Points9Y10 = $scope.ExpenseList[i].TenDaysCount;
                $scope.Points9Y15 = $scope.ExpenseList[i].FifteenDaysCount;
                $scope.Points9Y20 = $scope.ExpenseList[i].TweentyDaysCount;
                $scope.Points9Y25 = $scope.ExpenseList[i].TwentyFiveyDaysCount;
                $scope.Points9Y30 = $scope.ExpenseList[i].ThirtyDaysCount;
                $scope.Points9Y31 = $scope.ExpenseList[i].GraterThirtyDaysCount;
            }
            else if ($scope.ExpenseList[i].Category === 'Pending GRN Posting') {
                $scope.Points10Y3 = $scope.ExpenseList[i].ThreeDaysCount;
                $scope.Points10Y5 = $scope.ExpenseList[i].FiveDaysCount;
                $scope.Points10Y10 = $scope.ExpenseList[i].TenDaysCount;
                $scope.Points10Y15 = $scope.ExpenseList[i].FifteenDaysCount;
                $scope.Points10Y20 = $scope.ExpenseList[i].TweentyDaysCount;
                $scope.Points10Y25 = $scope.ExpenseList[i].TwentyFiveyDaysCount;
                $scope.Points10Y30 = $scope.ExpenseList[i].ThirtyDaysCount;
                $scope.Points10Y31 = $scope.ExpenseList[i].GraterThirtyDaysCount;
            }
            else if ($scope.ExpenseList[i].Category === 'Pending  Issue Request  For Approval') {
                $scope.Points11Y3 = $scope.ExpenseList[i].ThreeDaysCount;
                $scope.Points11Y5 = $scope.ExpenseList[i].FiveDaysCount;
                $scope.Points11Y10 = $scope.ExpenseList[i].TenDaysCount;
                $scope.Points11Y15 = $scope.ExpenseList[i].FifteenDaysCount;
                $scope.Points11Y20 = $scope.ExpenseList[i].TweentyDaysCount;
                $scope.Points11Y25 = $scope.ExpenseList[i].TwentyFiveyDaysCount;
                $scope.Points11Y30 = $scope.ExpenseList[i].ThirtyDaysCount;
                $scope.Points11Y31 = $scope.ExpenseList[i].GraterThirtyDaysCount;
            }
            else if ($scope.ExpenseList[i].Category === 'Pending Inventory Issue Posting') {
                $scope.Points12Y3 = $scope.ExpenseList[i].ThreeDaysCount;
                $scope.Points12Y5 = $scope.ExpenseList[i].FiveDaysCount;
                $scope.Points12Y10 = $scope.ExpenseList[i].TenDaysCount;
                $scope.Points12Y15 = $scope.ExpenseList[i].FifteenDaysCount;
                $scope.Points12Y20 = $scope.ExpenseList[i].TweentyDaysCount;
                $scope.Points12Y25 = $scope.ExpenseList[i].TwentyFiveyDaysCount;
                $scope.Points12Y30 = $scope.ExpenseList[i].ThirtyDaysCount;
                $scope.Points12Y31 = $scope.ExpenseList[i].GraterThirtyDaysCount;
            }
            
            
        }
        $("#containerStacking").ejChart(
            {
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false }
                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    range: { min: 0, max: 400, interval: 25 }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingcolumn',//stackingcolumn
                    enableAnimation: true,
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },

                //Initializing Series
                series:
                    [
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points1Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points1Y10 }, { x: '<=15', y: $scope.Points1Y15 },
                                    { x: '<=20', y: $scope.Points1Y20 }, { x: '<=25', y: $scope.Points1Y25 }, { x: '<=30', y: $scope.Points1Y30 }, { x: '>30', y: $scope.Points1Y31 }
                                ],
                            name: 'Pending Requisition For PO'
                        },
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points2Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points2Y10 }, { x: '<=15', y: $scope.Points2Y15 },
                                    { x: '<=20', y: $scope.Points2Y20 }, { x: '<=25', y: $scope.Points2Y25 }, { x: '<=30', y: $scope.Points2Y30 }, { x: '>30', y: $scope.Points2Y31 }
                                ],
                            name: 'PO Pending For GRN'
                        },
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points3Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points3Y10 }, { x: '<=15', y: $scope.Points3Y15 },
                                    { x: '<=20', y: $scope.Points3Y20 }, { x: '<=25', y: $scope.Points3Y25 }, { x: '<=30', y: $scope.Points3Y30 }, { x: '>30', y: $scope.Points3Y31 }
                                ],
                            name: 'PO Pending For LC Taging'
                        },
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points4Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points4Y10 }, { x: '<=15', y: $scope.Points4Y15 },
                                    { x: '<=20', y: $scope.Points4Y20 }, { x: '<=25', y: $scope.Points4Y25 }, { x: '<=30', y: $scope.Points4Y30 }, { x: '>30', y: $scope.Points4Y31 }
                                ],
                            name: 'PO Pending For Acceptance'
                        },
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points5Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points5Y10 }, { x: '<=15', y: $scope.Points5Y15 },
                                    { x: '<=20', y: $scope.Points5Y20 }, { x: '<=25', y: $scope.Points1525 }, { x: '<=30', y: $scope.Points5Y30 }, { x: '>30', y: $scope.Points5Y31 }
                                ],
                            name: 'Acceptance Pending For GRN'
                        },
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points6Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points6Y10 }, { x: '<=15', y: $scope.Points6Y15 },
                                    { x: '<=20', y: $scope.Points6Y20 }, { x: '<=25', y: $scope.Points6Y25 }, { x: '<=30', y: $scope.Points6Y30 }, { x: '>30', y: $scope.Points6Y31 }
                                ],
                            name: 'UnTag Gate Entry'
                        },
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points7Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points7Y10 }, { x: '<=15', y: $scope.Points7Y15 },
                                    { x: '<=20', y: $scope.Points7Y20 }, { x: '<=25', y: $scope.Points7Y25 }, { x: '<=30', y: $scope.Points7Y30 }, { x: '>30', y: $scope.Points7Y31 }
                                ],
                            name: 'Pending GRN Posting'
                        },
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points8Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points8Y10 }, { x: '<=15', y: $scope.Points8Y15 },
                                    { x: '<=20', y: $scope.Points8Y20 }, { x: '<=25', y: $scope.Points8Y25 }, { x: '<=30', y: $scope.Points8Y30 }, { x: '>30', y: $scope.Points8Y31 }
                                ],
                            name: 'Pending Inventory Issue'
                        },
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points9Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points9Y10 }, { x: '<=15', y: $scope.Points9Y15 },
                                    { x: '<=20', y: $scope.Points9Y20 }, { x: '<=25', y: $scope.Points9Y25 }, { x: '<=30', y: $scope.Points9Y30 }, { x: '>30', y: $scope.Points9Y31 }
                                ],
                            name: 'Pending Inventory Issue Posting'
                        }


                        //{
                        //    points:
                        //        [
                        //            { x: 'Pending Requisition For PO', y: $scope.Points1Y3 }, { x: 'PO Pending For GRN', y: $scope.Points2Y3 }, { x: 'PO Pending For LC Taging', y: $scope.Points3Y3 }
                        //            , { x: 'PO Pending For Acceptance', y: $scope.Points4Y3 }, { x: 'Acceptance Pending For GRN', y: $scope.Points5Y3 }, { x: 'UnTag Gate Entry', y: $scope.Points6Y3 }
                        //            , { x: 'Pending GRN Posting', y: $scope.Points7Y3 }, { x: 'Pending Inventory Issue', y: $scope.Points8Y3 }, { x: 'Pending Inventory Issue Posting', y: $scope.Points9Y3 }
                        //        ],
                        //    name: '<=3'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: 'Pending Requisition For PO', y: $scope.Points1Y5 }, { x: 'PO Pending For GRN', y: $scope.Points2Y5 }, { x: 'PO Pending For LC Taging', y: $scope.Points3Y5 }
                        //            , { x: 'PO Pending For Acceptance', y: $scope.Points4Y5 }, { x: 'Acceptance Pending For GRN', y: $scope.Points5Y5 }, { x: 'UnTag Gate Entry', y: $scope.Points6Y5 }
                        //            , { x: 'Pending GRN Posting', y: $scope.Points7Y5 }, { x: 'Pending Inventory Issue', y: $scope.Points8Y5 }, { x: 'Pending Inventory Issue Posting', y: $scope.Points9Y5 }
                        //        ],
                        //    name: '<=5'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: 'Pending Requisition For PO', y: $scope.Points1Y10 }, { x: 'PO Pending For GRN', y: $scope.Points2Y10 }, { x: 'PO Pending For LC Taging', y: $scope.Points3Y10 }
                        //            , { x: 'PO Pending For Acceptance', y: $scope.Points4Y5 }, { x: 'Acceptance Pending For GRN', y: $scope.Points5Y10 }, { x: 'UnTag Gate Entry', y: $scope.Points6Y10 }
                        //            , { x: 'Pending GRN Posting', y: $scope.Points7Y10 }, { x: 'Pending Inventory Issue', y: $scope.Points8Y10 }, { x: 'Pending Inventory Issue Posting', y: $scope.Points9Y10 }
                        //        ],
                        //    name: '<=10'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: 'Pending Requisition For PO', y: $scope.Points1Y15 }, { x: 'PO Pending For GRN', y: $scope.Points2Y15 }, { x: 'PO Pending For LC Taging', y: $scope.Points3Y15 }
                        //            , { x: 'PO Pending For Acceptance', y: $scope.Points4Y15 }, { x: 'Acceptance Pending For GRN', y: $scope.Points5Y15 }, { x: 'UnTag Gate Entry', y: $scope.Points6Y15 }
                        //            , { x: 'Pending GRN Posting', y: $scope.Points7Y15 }, { x: 'Pending Inventory Issue', y: $scope.Points8Y15 }, { x: 'Pending Inventory Issue Posting', y: $scope.Points9Y15}
                        //        ],
                        //    name: '<=15'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: 'Pending Requisition For PO', y: $scope.Points1Y20 }, { x: 'PO Pending For GRN', y: $scope.Points2Y20 }, { x: 'PO Pending For LC Taging', y: $scope.Points3Y20 }
                        //            , { x: 'PO Pending For Acceptance', y: $scope.Points4Y20 }, { x: 'Acceptance Pending For GRN', y: $scope.Points5Y20 }, { x: 'UnTag Gate Entry', y: $scope.Points6Y20 }
                        //            , { x: 'Pending GRN Posting', y: $scope.Points7Y20 }, { x: 'Pending Inventory Issue', y: $scope.Points8Y20 }, { x: 'Pending Inventory Issue Posting', y: $scope.Points9Y20 }
                        //        ],
                        //    name: '<=20'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: 'Pending Requisition For PO', y: $scope.Points1Y25 }, { x: 'PO Pending For GRN', y: $scope.Points2Y25 }, { x: 'PO Pending For LC Taging', y: $scope.Points3Y25 }
                        //            , { x: 'PO Pending For Acceptance', y: $scope.Points4Y25 }, { x: 'Acceptance Pending For GRN', y: $scope.Points5Y25 }, { x: 'UnTag Gate Entry', y: $scope.Points6Y25 }
                        //            , { x: 'Pending GRN Posting', y: $scope.Points7Y25 }, { x: 'Pending Inventory Issue', y: $scope.Points8Y25 }, { x: 'Pending Inventory Issue Posting', y: $scope.Points9Y25 }
                        //        ],
                        //    name: '<=25'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: 'Pending Requisition For PO', y: $scope.Points1Y30 }, { x: 'PO Pending For GRN', y: $scope.Points2Y30 }, { x: 'PO Pending For LC Taging', y: $scope.Points3Y30 }
                        //            , { x: 'PO Pending For Acceptance', y: $scope.Points4Y30 }, { x: 'Acceptance Pending For GRN', y: $scope.Points5Y30 }, { x: 'UnTag Gate Entry', y: $scope.Points6Y30 }
                        //            , { x: 'Pending GRN Posting', y: $scope.Points7Y30 }, { x: 'Pending Inventory Issue', y: $scope.Points8Y30 }, { x: 'Pending Inventory Issue Posting', y: $scope.Points9Y30 }
                        //        ],
                        //    name: '<=30'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: 'Pending Requisition For PO', y: $scope.Points1Y31 }, { x: 'PO Pending For GRN', y: $scope.Points2Y31 }, { x: 'PO Pending For LC Taging', y: $scope.Points3Y31 }
                        //            , { x: 'PO Pending For Acceptance', y: $scope.Points4Y31 }, { x: 'Acceptance Pending For GRN', y: $scope.Points5Y31 }, { x: 'UnTag Gate Entry', y: $scope.Points6Y31 }
                        //            , { x: 'Pending GRN Posting', y: $scope.Points7Y31 }, { x: 'Pending Inventory Issue', y: $scope.Points8Y31 }, { x: 'Pending Inventory Issue Posting', y: $scope.Points9Y31 }
                        //        ],
                        //    name: '>30'
                        //}

                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'Ageing Wise Activity Status' },
                size: { height: "350" },
                legend: { visible: true }
            });


        //#region Individual Graph for Pending Requisition For Approval
        $("#containerStacking11").ejChart(
            {
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false }
                    //font: {
                    //        color: "transparent"
                    //    }

                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    range: { min: 0, max: 400, interval: 25 }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingcolumn',//stackingcolumn
                    enableAnimation: true,
                    //specifying series fill color as rgb value
                    fill: 'rgb(0,0,255)',//#F6B53F
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },

                //Initializing Series
                series:
                    [
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points1Y3, fill: '#2ECC71' },
                                    { x: '<=5', y: $scope.Points1Y5, fill: '#F1C40F' },
                                    { x: '<=10', y: $scope.Points1Y10, fill: '#E74C3C' },
                                    { x: '<=15', y: $scope.Points1Y15, fill: '#52B2D9' },
                                    { x: '<=20', y: $scope.Points1Y20, fill: '#FDE3A7' },
                                    { x: '<=25', y: $scope.Points1Y25, fill: 'LIGHT PURPLE' },
                                    { x: '<=30', y: $scope.Points1Y30, fill: 'BROWN' },
                                    { x: '>30', y: $scope.Points1Y31, fill: '#E74C3C' }
                                ],
                            name: 'Pending Requisition For PO'
                        }


                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'Pending Requisition For Approval' },
                size: { height: "350" },
                legend: { visible: false }
            });

        //#endregion
        //#region Individual Graph for Pending Requisition For PO
        $("#containerStacking1").ejChart(
            {
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false }
                    //font: {
                    //        color: "transparent"
                    //    }
                    
                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    range: { min: 0, max: 400, interval: 25 }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingcolumn',//stackingcolumn
                    enableAnimation: true,
                    //specifying series fill color as rgb value
                    fill: 'rgb(0,0,255)',//#F6B53F
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },
                
                //Initializing Series
                series:
                    [
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points2Y3, fill: '#2ECC71' },
                                    { x: '<=5', y: $scope.Points2Y5, fill: '#F1C40F' },
                                    { x: '<=10', y: $scope.Points2Y10, fill: '#E74C3C' },
                                    { x: '<=15', y: $scope.Points2Y15, fill: '#52B2D9' },
                                    { x: '<=20', y: $scope.Points2Y20, fill: '#FDE3A7' },
                                    { x: '<=25', y: $scope.Points2Y25, fill: 'LIGHT PURPLE' },
                                    { x: '<=30', y: $scope.Points2Y30, fill: 'BROWN' },
                                    { x: '>30', y: $scope.Points2Y31, fill: '#E74C3C' }
                                ],
                            name: 'Pending Requisition For PO'
                        }
                       

                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'Pending Requisition For PO' },
                size: { height: "350" },
                legend: { visible: false }                
            });

        //#endregion
        //#region Individual Graph PO Pending For Approval
        $("#containerStacking22").ejChart(
            {
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false }
                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    range: { min: 0, max: 400, interval: 25 }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingcolumn',//stackingcolumn
                    enableAnimation: true,
                    fill: '#F6B53F',//,'rgb(0,0,255)',
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },

                //Initializing Series
                series:
                    [

                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points3Y3, fill: '#2ECC71' },
                                    { x: '<=5', y: $scope.Points3Y5, fill: '#F1C40F' },
                                    { x: '<=10', y: $scope.Point3Y10, fill: '#E74C3C' },
                                    { x: '<=15', y: $scope.Points3Y15, fill: '#52B2D9' },
                                    { x: '<=20', y: $scope.Points3Y20, fill: '#FDE3A7' },
                                    { x: '<=25', y: $scope.Points3Y25, fill: 'LIGHT PURPLE' },
                                    { x: '<=30', y: $scope.Points3Y30, fill: 'BROWN' },
                                    { x: '>30', y: $scope.Points3Y31, fill: '#E74C3C' }
                                ],
                            name: 'PO Pending For GRN'
                        }





                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'PO Pending For Approval' },
                size: { height: "350" },
                legend: { visible: false }
            });

        //#endregion

        //#region Individual Graph PO Pending For Purchase
        $("#containerStacking2").ejChart(
            {
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false }
                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    range: { min: 0, max: 400, interval: 25 }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingcolumn',//stackingcolumn
                    enableAnimation: true,
                    fill: '#F6B53F',//,'rgb(0,0,255)',
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },

                //Initializing Series
                series:
                    [
                        
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points4Y3, fill: '#2ECC71' },
                                    { x: '<=5', y: $scope.Points4Y5, fill: '#F1C40F' },
                                    { x: '<=10', y: $scope.Points4Y10, fill: '#E74C3C' },
                                    { x: '<=15', y: $scope.Points4Y15, fill: '#52B2D9' },
                                    { x: '<=20', y: $scope.Points4Y20, fill: '#FDE3A7' },
                                    { x: '<=25', y: $scope.Points4Y25, fill: 'LIGHT PURPLE' },
                                    { x: '<=30', y: $scope.Points4Y30, fill: 'BROWN' },
                                    { x: '>30', y: $scope.Points4Y31, fill: '#E74C3C' }
                                ],
                            name: 'PO Pending For Purchase'
                        }
                       




                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'PO Pending For Purchase' },
                size: { height: "350" },
                legend: { visible: false }
            });

        //#endregion

        //#region Individual Graph
        $("#containerStacking3").ejChart(
            {
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false }
                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    range: { min: 0, max: 400, interval: 25 }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingcolumn',//stackingcolumn
                    enableAnimation: true,
                    fill: '#6FAAB0',//,'rgb(0,0,255)',
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },

                //Initializing Series
                series:
                    [
                        
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points5Y3, fill: '#2ECC71' },
                                    { x: '<=5', y: $scope.Points5Y5, fill: '#F1C40F' },
                                    { x: '<=10', y: $scope.Points5Y10, fill: '#E74C3C' },
                                    { x: '<=15', y: $scope.Points5Y15, fill: '#52B2D9' },
                                    { x: '<=20', y: $scope.Points5Y20, fill: '#FDE3A7' },
                                    { x: '<=25', y: $scope.Points5Y25, fill: 'LIGHT PURPLE' },
                                    { x: '<=30', y: $scope.Points5Y30, fill: 'BROWN' },
                                    { x: '>30', y: $scope.Points5Y31, fill: '#E74C3C' }
                                ],
                            name: 'PO Pending For LC Taging'
                        }
                       



                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'PO Pending For LC Taging' },
                size: { height: "350" },
                legend: { visible: false }
            });

        //#endregion

        //#region Individual Graph Invoice Pending For Acceptance
        $("#containerStacking10").ejChart(
            {
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false }
                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    range: { min: 0, max: 400, interval: 25 }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingcolumn',//stackingcolumn
                    enableAnimation: true,
                    fill: '#E27F2D',//,'rgb(0,0,255)',
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },
                legend: {
                    //Visible chart legend
                    visible: false
                },
                //Initializing Series
                series:
                    [

                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points6Y3, fill: '#2ECC71' },
                                    { x: '<=5', y: $scope.Points6Y5, fill: '#F1C40F' },
                                    { x: '<=10', y: $scope.Points6Y10, fill: '#E74C3C' },
                                    { x: '<=15', y: $scope.Points6Y15, fill: '#52B2D9' },
                                    { x: '<=20', y: $scope.Points6Y20, fill: '#FDE3A7' },
                                    { x: '<=25', y: $scope.Points6Y25, fill: 'LIGHT PURPLE' },
                                    { x: '<=30', y: $scope.Points6Y30, fill: 'BROWN' },
                                    { x: '>30', y: $scope.Points6Y31, fill: '#E74C3C' }
                                ],
                            name: 'Invoice Pending For Acceptance'
                        }




                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'Invoice Pending For Acceptance' },
                size: { height: "350" },
                legend: { visible: false }
            });

        //#endregion
        //#region Individual Graph PO Pending For Acceptance
        $("#containerStacking4").ejChart(
            {
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false }
                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    range: { min: 0, max: 400, interval: 25 }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingcolumn',//stackingcolumn
                    enableAnimation: true,
                    fill: '#C4C24A',//,'rgb(0,0,255)',
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },

                //Initializing Series
                series:
                    [
               
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points7Y3, fill: '#2ECC71' },
                                    { x: '<=5', y: $scope.Points7Y5, fill: '#F1C40F' },
                                    { x: '<=10', y: $scope.Points7Y10, fill: '#E74C3C' },
                                    { x: '<=15', y: $scope.Points7Y15, fill: '#52B2D9' },
                                    { x: '<=20', y: $scope.Points7Y20, fill: '#FDE3A7' },
                                    { x: '<=25', y: $scope.Points7Y25, fill: 'LIGHT PURPLE' },
                                    { x: '<=30', y: $scope.Points7Y30, fill: 'BROWN' },
                                    { x: '>30', y: $scope.Points7Y31, fill: '#E74C3C' }
                                ],
                            name: 'PO Pending For Acceptance'
                        }
                      



                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'PO Pending For Acceptance' },
                size: { height: "350" },
                legend: { visible: false }
            });

        //#endregion
        //#region Individual Graph Acceptance Pending For GRN
        $("#containerStacking5").ejChart(
            {
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false }
                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    range: { min: 0, max: 400, interval: 25 }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingcolumn',//stackingcolumn
                    enableAnimation: true,
                    fill: 'gray',//,'rgb(0,0,255)',
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },

                //Initializing Series
                series:
                    [
                       
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points8Y3, fill: '#2ECC71' },
                                    { x: '<=5', y: $scope.Points8Y5, fill: '#F1C40F' },
                                    { x: '<=10', y: $scope.Points8Y10, fill: '#E74C3C' },
                                    { x: '<=15', y: $scope.Points8Y15, fill: '#52B2D9' },
                                    { x: '<=20', y: $scope.Points8Y20, fill: '#FDE3A7' },
                                    { x: '<=25', y: $scope.Points8Y25, fill: 'LIGHT PURPLE' },
                                    { x: '<=30', y: $scope.Points8Y30, fill: 'BROWN' },
                                    { x: '>30', y: $scope.Points8Y31, fill: '#E74C3C' }
                                ],
                            name: 'Acceptance Pending For GRN'
                        }
                    


                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'Acceptance Pending For GRN' },
                size: { height: "350" },
                legend: { visible: false }
            });

        //#endregion
        //#region Individual Graph Pending For GRN
        $("#containerStacking6").ejChart(
            {
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false }
                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    range: { min: 0, max: 400, interval: 25 }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingcolumn',//stackingcolumn
                    enableAnimation: true,
                    fill: '#005277',//,'rgb(0,0,255)',
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },

                //Initializing Series
                series:
                    [
                       
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points9Y3, fill: '#2ECC71' },
                                    { x: '<=5', y: $scope.Points9Y5, fill: '#F1C40F' },
                                    { x: '<=10', y: $scope.Points9Y10, fill: '#E74C3C' },
                                    { x: '<=15', y: $scope.Points9Y15, fill: '#52B2D9' },
                                    { x: '<=20', y: $scope.Points9Y20, fill: '#FDE3A7' },
                                    { x: '<=25', y: $scope.Points9Y25, fill: 'LIGHT PURPLE' },
                                    { x: '<=30', y: $scope.Points9Y30, fill: 'BROWN' },
                                    { x: '>30', y: $scope.Points9Y31, fill: '#E74C3C' }
                                ],
                            name: 'UnTag Gate Entry'
                        }
                       



                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'Pending For GRN' },
                size: { height: "350" },
                legend: { visible: false }
            });

        //#endregion
        //#region Individual Graph Pending GRN Posting
        $("#containerStacking7").ejChart(
            {
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false }
                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    range: { min: 0, max: 400, interval: 25 }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingcolumn',//stackingcolumn
                    enableAnimation: true,
                    fill: '#005277',//,'rgb(0,0,255)',
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },

                //Initializing Series
                series:
                    [
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points10Y3, fill: '#2ECC71' },
                                    { x: '<=5', y: $scope.Points10Y5, fill: '#F1C40F' },
                                    { x: '<=10', y: $scope.Points10Y10, fill: '#E74C3C' },
                                    { x: '<=15', y: $scope.Points10Y15, fill: '#52B2D9' },
                                    { x: '<=20', y: $scope.Points10Y20, fill: '#FDE3A7' },
                                    { x: '<=25', y: $scope.Points10Y25, fill: 'LIGHT PURPLE' },
                                    { x: '<=30', y: $scope.Points10Y30, fill: 'BROWN' },
                                    { x: '>30', y: $scope.Points10Y31, fill: '#E74C3C' }
                                ],
                            name: 'Pending GRN Posting'
                        }
                       



                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'Pending GRN Posting' },
                size: { height: "350" },
                legend: { visible: false }
            });

        //#endregion
        //#region Individual Graph Pending Inventory Issue
        $("#containerStacking8").ejChart(
            {
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false }
                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    range: { min: 0, max: 400, interval: 25 }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingcolumn',//stackingcolumn
                    enableAnimation: true,
                    fill: '#282828',//,'rgb(0,0,255)',
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },

                //Initializing Series
                series:
                    [
                        
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points11Y3, fill: '#2ECC71' },
                                    { x: '<=5', y: $scope.Points11Y5, fill: '#F1C40F' },
                                    { x: '<=10', y: $scope.Points11Y10, fill: '#E74C3C' },
                                    { x: '<=15', y: $scope.Points11Y15, fill: '#52B2D9' },
                                    { x: '<=20', y: $scope.Points11Y20, fill: '#FDE3A7' },
                                    { x: '<=25', y: $scope.Points11Y25, fill: 'LIGHT PURPLE' },
                                    { x: '<=30', y: $scope.Points11Y30, fill: 'BROWN' },
                                    { x: '>30', y: $scope.Points11Y31, fill: '#E74C3C' }
                                ],
                            name: 'Pending Inventory Issue'
                        }
                       




                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'Pending Inventory Issue' },
                size: { height: "350" },
                legend: { visible: false }
            });

        //#endregion
        //#region Individual Graph Pending Inventory Issue Posting
        $("#containerStacking9").ejChart(
            {
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false }
                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    range: { min: 0, max: 400, interval: 25 }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingcolumn',//stackingcolumn
                    enableAnimation: true,
                    fill: '#69D2E7',//,'rgb(0,0,255)',
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },

                //Initializing Series
                series:
                    [
                        
                        {
                            points:
                                [
                                    { x: '<=3', y: $scope.Points12Y3, fill: '#2ECC71' },
                                    { x: '<=5', y: $scope.Points12Y5, fill: '#F1C40F' },
                                    { x: '<=10', y: $scope.Points12Y10, fill: '#E74C3C' },
                                    { x: '<=15', y: $scope.Points12Y15, fill: '#52B2D9' },
                                    { x: '<=20', y: $scope.Points12Y20, fill: '#FDE3A7' },
                                    { x: '<=25', y: $scope.Points12Y25, fill: 'LIGHT PURPLE' },
                                    { x: '<=30', y: $scope.Points12Y30, fill: 'BROWN' },
                                    { x: '>30', y: $scope.Points12Y31, fill: '#E74C3C' }
                                ],
                            name: 'Pending Inventory Issue Posting'
                        }
                       




                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'Pending Inventory Issue Posting' },
                size: { height: "350" },
                legend: { visible: false }
            });

        //#endregion
       
    }
    $scope.pPoints1Y3 = '';
    $scope.pPoints1Y5 = '';
    $scope.pPoints1Y10 = '';
    $scope.pPoints1Y15 = '';
    $scope.pPoints1Y20 = '';
    $scope.pPoints1Y25 = '';
    $scope.pPoints1Y30 = '';
    $scope.pPoints1Y31 = '';
    $scope.stackingGraph1 = function () {
        
        for (var i = 0; i < $scope.ExpenseListGraph.length; i++) {

            if ($scope.data === false || $scope.data === undefined) {
                if ($scope.ExpenseListGraph[i].Category === 'Pending Requisition For Approval') {
                    $scope.pPoints1Y3 = $scope.ExpenseListGraph[i].ThreeDaysCount;
                    $scope.pPoints1Y5 = $scope.ExpenseListGraph[i].FiveDaysCount;
                    $scope.pPoints1Y10 = $scope.ExpenseListGraph[i].TenDaysCount;
                    $scope.pPoints1Y15 = $scope.ExpenseListGraph[i].FifteenDaysCount;
                    $scope.pPoints1Y20 = $scope.ExpenseListGraph[i].TweentyDaysCount;
                    $scope.pPoints1Y25 = $scope.ExpenseListGraph[i].TwentyFiveyDaysCount;
                    $scope.pPoints1Y30 = $scope.ExpenseListGraph[i].ThirtyDaysCount;
                    $scope.pPoints1Y31 = $scope.ExpenseListGraph[i].GraterThirtyDaysCount;


                }
                else if ($scope.ExpenseListGraph[i].Category === 'Pending Requisition For PO') {
                    $scope.pPoints2Y3 = $scope.ExpenseListGraph[i].ThreeDaysCount;
                    $scope.pPoints2Y5 = $scope.ExpenseListGraph[i].FiveDaysCount;
                    $scope.pPoints2Y10 = $scope.ExpenseListGraph[i].TenDaysCount;
                    $scope.pPoints2Y15 = $scope.ExpenseListGraph[i].FifteenDaysCount;
                    $scope.pPoints2Y20 = $scope.ExpenseListGraph[i].TweentyDaysCount;
                    $scope.pPoints2Y25 = $scope.ExpenseListGraph[i].TwentyFiveyDaysCount;
                    $scope.pPoints2Y30 = $scope.ExpenseListGraph[i].ThirtyDaysCount;
                    $scope.pPoints2Y31 = $scope.ExpenseListGraph[i].GraterThirtyDaysCount;


                }
                else if ($scope.ExpenseListGraph[i].Category === 'PO Pending For Approval') {

                    $scope.pPoints3Y3 = $scope.ExpenseListGraph[i].ThreeDaysCount;
                    $scope.pPoints3Y5 = $scope.ExpenseListGraph[i].FiveDaysCount;
                    $scope.pPoints3Y10 = $scope.ExpenseListGraph[i].TenDaysCount;
                    $scope.pPoints3Y15 = $scope.ExpenseListGraph[i].FifteenDaysCount;
                    $scope.pPoints3Y20 = $scope.ExpenseListGraph[i].TweentyDaysCount;
                    $scope.pPoints3Y25 = $scope.ExpenseListGraph[i].TwentyFiveyDaysCount;
                    $scope.pPoints3Y30 = $scope.ExpenseListGraph[i].ThirtyDaysCount;
                    $scope.pPoints3Y31 = $scope.ExpenseListGraph[i].GraterThirtyDaysCount;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'PO Pending For Purchase') {

                    $scope.pPoints4Y3 = $scope.ExpenseListGraph[i].ThreeDaysCount;
                    $scope.pPoints4Y5 = $scope.ExpenseListGraph[i].FiveDaysCount;
                    $scope.pPoints4Y10 = $scope.ExpenseListGraph[i].TenDaysCount;
                    $scope.pPoints4Y15 = $scope.ExpenseListGraph[i].FifteenDaysCount;
                    $scope.pPoints4Y20 = $scope.ExpenseListGraph[i].TweentyDaysCount;
                    $scope.pPoints4Y25 = $scope.ExpenseListGraph[i].TwentyFiveyDaysCount;
                    $scope.pPoints4Y30 = $scope.ExpenseListGraph[i].ThirtyDaysCount;
                    $scope.pPoints4Y31 = $scope.ExpenseListGraph[i].GraterThirtyDaysCount;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'PO Pending For LC Taging') {

                    $scope.pPoints5Y3 = $scope.ExpenseListGraph[i].ThreeDaysCount;
                    $scope.pPoints5Y5 = $scope.ExpenseListGraph[i].FiveDaysCount;
                    $scope.pPoints5Y10 = $scope.ExpenseListGraph[i].TenDaysCount;
                    $scope.pPoints5Y15 = $scope.ExpenseListGraph[i].FifteenDaysCount;
                    $scope.pPoints5Y20 = $scope.ExpenseListGraph[i].TweentyDaysCount;
                    $scope.pPointsY25 = $scope.ExpenseListGraph[i].TwentyFiveyDaysCount;
                    $scope.pPoints53Y30 = $scope.ExpenseListGraph[i].ThirtyDaysCount;
                    $scope.pPoints5Y31 = $scope.ExpenseListGraph[i].GraterThirtyDaysCount;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'Invoice Pending For Acceptance') {
                    $scope.pPoints6Y3 = $scope.ExpenseListGraph[i].ThreeDaysCount;
                    $scope.pPoints6Y5 = $scope.ExpenseListGraph[i].FiveDaysCount;
                    $scope.pPoints6Y10 = $scope.ExpenseListGraph[i].TenDaysCount;
                    $scope.pPoints6Y15 = $scope.ExpenseListGraph[i].FifteenDaysCount;
                    $scope.pPoints6Y20 = $scope.ExpenseListGraph[i].TweentyDaysCount;
                    $scope.pPoints6Y25 = $scope.ExpenseListGraph[i].TwentyFiveyDaysCount;
                    $scope.pPoints6Y30 = $scope.ExpenseListGraph[i].ThirtyDaysCount;
                    $scope.pPoints6Y31 = $scope.ExpenseListGraph[i].GraterThirtyDaysCount;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'PO Pending For Acceptance') {

                    $scope.pPoints7Y3 = $scope.ExpenseListGraph[i].ThreeDaysCount;
                    $scope.pPoints7Y5 = $scope.ExpenseListGraph[i].FiveDaysCount;
                    $scope.pPoints7Y10 = $scope.ExpenseListGraph[i].TenDaysCount;
                    $scope.pPoints7Y15 = $scope.ExpenseListGraph[i].FifteenDaysCount;
                    $scope.pPoints7Y20 = $scope.ExpenseListGraph[i].TweentyDaysCount;
                    $scope.pPoints7Y25 = $scope.ExpenseListGraph[i].TwentyFiveyDaysCount;
                    $scope.pPoints7Y30 = $scope.ExpenseListGraph[i].ThirtyDaysCount;
                    $scope.pPoints7Y31 = $scope.ExpenseListGraph[i].GraterThirtyDaysCount;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'Acceptance Pending For GRN	') {
                    $scope.pPoints8Y3 = $scope.ExpenseListGraph[i].ThreeDaysCount;
                    $scope.pPoints8Y5 = $scope.ExpenseListGraph[i].FiveDaysCount;
                    $scope.pPoints8Y10 = $scope.ExpenseListGraph[i].TenDaysCount;
                    $scope.pPoints8Y15 = $scope.ExpenseListGraph[i].FifteenDaysCount;
                    $scope.pPoints8Y20 = $scope.ExpenseListGraph[i].TweentyDaysCount;
                    $scope.pPoints8Y25 = $scope.ExpenseListGraph[i].TwentyFiveyDaysCount;
                    $scope.pPoints8Y30 = $scope.ExpenseListGraph[i].ThirtyDaysCount;
                    $scope.pPoints8Y31 = $scope.ExpenseListGraph[i].GraterThirtyDaysCount;
                }

                else if ($scope.ExpenseListGraph[i].Category === 'Pending For GRN') {
                    $scope.pPoints9Y3 = $scope.ExpenseListGraph[i].ThreeDaysCount;
                    $scope.pPoints9Y5 = $scope.ExpenseListGraph[i].FiveDaysCount;
                    $scope.pPoints9Y10 = $scope.ExpenseListGraph[i].TenDaysCount;
                    $scope.pPoints9Y15 = $scope.ExpenseListGraph[i].FifteenDaysCount;
                    $scope.pPoints9Y20 = $scope.ExpenseListGraph[i].TweentyDaysCount;
                    $scope.pPoints9Y25 = $scope.ExpenseListGraph[i].TwentyFiveyDaysCount;
                    $scope.pPoints9Y30 = $scope.ExpenseListGraph[i].ThirtyDaysCount;
                    $scope.pPoints9Y31 = $scope.ExpenseListGraph[i].GraterThirtyDaysCount;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'Pending GRN For Approval') {
                    $scope.pPoints10Y3 = $scope.ExpenseListGraph[i].ThreeDaysCount;
                    $scope.pPoints10Y5 = $scope.ExpenseListGraph[i].FiveDaysCount;
                    $scope.pPoints10Y10 = $scope.ExpenseListGraph[i].TenDaysCount;
                    $scope.pPoints10Y15 = $scope.ExpenseListGraph[i].FifteenDaysCount;
                    $scope.pPoints10Y20 = $scope.ExpenseListGraph[i].TweentyDaysCount;
                    $scope.pPoints10Y25 = $scope.ExpenseListGraph[i].TwentyFiveyDaysCount;
                    $scope.pPoints10Y30 = $scope.ExpenseListGraph[i].ThirtyDaysCount;
                    $scope.pPoints10Y31 = $scope.ExpenseListGraph[i].GraterThirtyDaysCount;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'Pending GRN Posting') {
                    $scope.pPoints11Y3 = $scope.ExpenseListGraph[i].ThreeDaysCount;
                    $scope.pPoints11Y5 = $scope.ExpenseListGraph[i].FiveDaysCount;
                    $scope.pPoints11Y10 = $scope.ExpenseListGraph[i].TenDaysCount;
                    $scope.pPoints11Y15 = $scope.ExpenseListGraph[i].FifteenDaysCount;
                    $scope.pPoints11Y20 = $scope.ExpenseListGraph[i].TweentyDaysCount;
                    $scope.pPoints11Y25 = $scope.ExpenseListGraph[i].TwentyFiveyDaysCount;
                    $scope.pPoints11Y30 = $scope.ExpenseListGraph[i].ThirtyDaysCount;
                    $scope.pPoints11Y31 = $scope.ExpenseListGraph[i].GraterThirtyDaysCount;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'Pending Inventory Issue') {
                    $scope.pPoints12Y3 = $scope.ExpenseListGraph[i].ThreeDaysCount;
                    $scope.pPoints12Y5 = $scope.ExpenseListGraph[i].FiveDaysCount;
                    $scope.pPoints12Y10 = $scope.ExpenseListGraph[i].TenDaysCount;
                    $scope.pPoints12Y15 = $scope.ExpenseListGraph[i].FifteenDaysCount;
                    $scope.pPoints12Y20 = $scope.ExpenseListGraph[i].TweentyDaysCount;
                    $scope.pPoints12Y25 = $scope.ExpenseListGraph[i].TwentyFiveyDaysCount;
                    $scope.pPoints12Y30 = $scope.ExpenseListGraph[i].ThirtyDaysCount;
                    $scope.pPoints12Y31 = $scope.ExpenseListGraph[i].GraterThirtyDaysCount;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'Pending Inventory Issue Posting') {
                    $scope.pPoints13Y3 = $scope.ExpenseListGraph[i].ThreeDaysCount;
                    $scope.pPoints13Y5 = $scope.ExpenseListGraph[i].FiveDaysCount;
                    $scope.pPoints13Y10 = $scope.ExpenseListGraph[i].TenDaysCount;
                    $scope.pPoints13Y15 = $scope.ExpenseListGraph[i].FifteenDaysCount;
                    $scope.pPoints13Y20 = $scope.ExpenseListGraph[i].TweentyDaysCount;
                    $scope.pPoints13Y25 = $scope.ExpenseListGraph[i].TwentyFiveyDaysCount;
                    $scope.pPoints13Y30 = $scope.ExpenseListGraph[i].ThirtyDaysCount;
                    $scope.pPoints13Y31 = $scope.ExpenseListGraph[i].GraterThirtyDaysCount;
                }
                
			}
            
            else {
                if ($scope.ExpenseListGraph[i].Category === 'Pending Requisition For Approval') {
                    $scope.pPoints1Y3 = $scope.ExpenseListGraph[i].Total3Value;
                    $scope.pPoints1Y5 = $scope.ExpenseListGraph[i].Total5Value;
                    $scope.pPoints1Y10 = $scope.ExpenseListGraph[i].Total10Value;
                    $scope.pPoints1Y15 = $scope.ExpenseListGraph[i].Total15Value;
                    $scope.pPoints1Y20 = $scope.ExpenseListGraph[i].Total20Value;
                    $scope.pPoints1Y25 = $scope.ExpenseListGraph[i].Total25Value;
                    $scope.pPoints1Y30 = $scope.ExpenseListGraph[i].Total30Value;
                    $scope.pPoints1Y31 = $scope.ExpenseListGraph[i].Total31Value;


                }
                else if ($scope.ExpenseListGraph[i].Category === 'Pending Requisition For PO') {
                    $scope.pPoints2Y3 = $scope.ExpenseListGraph[i].Total3Value;
                    $scope.pPoints2Y5 = $scope.ExpenseListGraph[i].Total5Value;
                    $scope.pPoints2Y10 = $scope.ExpenseListGraph[i].Total10Value;
                    $scope.pPoints2Y15 = $scope.ExpenseListGraph[i].Total15Value;
                    $scope.pPoints2Y20 = $scope.ExpenseListGraph[i].Total20Value;
                    $scope.pPoints2Y25 = $scope.ExpenseListGraph[i].Total25Value;
                    $scope.pPoints2Y30 = $scope.ExpenseListGraph[i].Total30Value;
                    $scope.pPoints2Y31 = $scope.ExpenseListGraph[i].Total31Value;


                }
                else if ($scope.ExpenseListGraph[i].Category === 'PO Pending For Approval') {

                    $scope.pPoints3Y3 = $scope.ExpenseListGraph[i].Total3Value;
                    $scope.pPoints3Y5 = $scope.ExpenseListGraph[i].Total5Value;
                    $scope.pPoints3Y10 = $scope.ExpenseListGraph[i].Total10Value;
                    $scope.pPoints3Y15 = $scope.ExpenseListGraph[i].Total15Value;
                    $scope.pPoints3Y20 = $scope.ExpenseListGraph[i].Total20Value;
                    $scope.pPoints3Y25 = $scope.ExpenseListGraph[i].Total25Value;
                    $scope.pPoints3Y30 = $scope.ExpenseListGraph[i].Total30Value;
                    $scope.pPoints3Y31 = $scope.ExpenseListGraph[i].Total31Value;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'PO Pending For Purchase') {

                    $scope.pPoints4Y3 = $scope.ExpenseListGraph[i].Total3Value;
                    $scope.pPoints4Y5 = $scope.ExpenseListGraph[i].Total5Value;
                    $scope.pPoints4Y10 = $scope.ExpenseListGraph[i].Total10Value;
                    $scope.pPoints4Y15 = $scope.ExpenseListGraph[i].Total15Value;
                    $scope.pPoints4Y20 = $scope.ExpenseListGraph[i].Total20Value;
                    $scope.pPoints4Y25 = $scope.ExpenseListGraph[i].Total25Value;
                    $scope.pPoints4Y30 = $scope.ExpenseListGraph[i].Total30Value;
                    $scope.pPoints4Y31 = $scope.ExpenseListGraph[i].Total31Value;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'PO Pending For LC Taging') {

                    $scope.pPoints5Y3 = $scope.ExpenseListGraph[i].Total3Value;
                    $scope.pPoints5Y5 = $scope.ExpenseListGraph[i].Total5Value;
                    $scope.pPoints5Y10 = $scope.ExpenseListGraph[i].Total10Value;
                    $scope.pPoints5Y15 = $scope.ExpenseListGraph[i].Total15Value;
                    $scope.pPoints5Y20 = $scope.ExpenseListGraph[i].Total20Value;
                    $scope.pPointsY25 = $scope.ExpenseListGraph[i].Total25Value;
                    $scope.pPoints53Y30 = $scope.ExpenseListGraph[i].Total30Value;
                    $scope.pPoints5Y31 = $scope.ExpenseListGraph[i].Total31Value;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'Invoice Pending For Acceptance') {
                    $scope.pPoints6Y3 = $scope.ExpenseListGraph[i].Total3Value;
                    $scope.pPoints6Y5 = $scope.ExpenseListGraph[i].Total5Value;
                    $scope.pPoints6Y10 = $scope.ExpenseListGraph[i].Total10Value;
                    $scope.pPoints6Y15 = $scope.ExpenseListGraph[i].Total15Value;
                    $scope.pPoints6Y20 = $scope.ExpenseListGraph[i].Total20Value;
                    $scope.pPoints6Y25 = $scope.ExpenseListGraph[i].Total25Value;
                    $scope.pPoints6Y30 = $scope.ExpenseListGraph[i].Total30Value;
                    $scope.pPoints6Y31 = $scope.ExpenseListGraph[i].Total31Value;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'PO Pending For Acceptance') {

                    $scope.pPoints7Y3 = $scope.ExpenseListGraph[i].Total3Value;
                    $scope.pPoints7Y5 = $scope.ExpenseListGraph[i].Total5Value;
                    $scope.pPoints7Y10 = $scope.ExpenseListGraph[i].Total10Value;
                    $scope.pPoints7Y15 = $scope.ExpenseListGraph[i].Total15Value;
                    $scope.pPoints7Y20 = $scope.ExpenseListGraph[i].Total20Value;
                    $scope.pPoints7Y25 = $scope.ExpenseListGraph[i].Total25Value;
                    $scope.pPoints7Y30 = $scope.ExpenseListGraph[i].Total30Value;
                    $scope.pPoints7Y31 = $scope.ExpenseListGraph[i].Total31Value;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'Acceptance Pending For GRN	') {
                    $scope.pPoints8Y3 = $scope.ExpenseListGraph[i].Total3Value;
                    $scope.pPoints8Y5 = $scope.ExpenseListGraph[i].Total5Value;
                    $scope.pPoints8Y10 = $scope.ExpenseListGraph[i].Total10Value;
                    $scope.pPoints8Y15 = $scope.ExpenseListGraph[i].Total15Value;
                    $scope.pPoints8Y20 = $scope.ExpenseListGraph[i].Total20Value;
                    $scope.pPoints8Y25 = $scope.ExpenseListGraph[i].Total25Value;
                    $scope.pPoints8Y30 = $scope.ExpenseListGraph[i].Total30Value;
                    $scope.pPoints8Y31 = $scope.ExpenseListGraph[i].Total31Value;
                }

                else if ($scope.ExpenseListGraph[i].Category === 'Pending For GRN') {
                    $scope.pPoints9Y3 = $scope.ExpenseListGraph[i].Total3Value;
                    $scope.pPoints9Y5 = $scope.ExpenseListGraph[i].Total5Value;
                    $scope.pPoints9Y10 = $scope.ExpenseListGraph[i].Total10Value;
                    $scope.pPoints9Y15 = $scope.ExpenseListGraph[i].Total15Value;
                    $scope.pPoints9Y20 = $scope.ExpenseListGraph[i].Total20Value;
                    $scope.pPoints9Y25 = $scope.ExpenseListGraph[i].Total25Value;
                    $scope.pPoints9Y30 = $scope.ExpenseListGraph[i].Total30Value;
                    $scope.pPoints9Y31 = $scope.ExpenseListGraph[i].Total31Value;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'Pending GRN For Approval') {
                    $scope.pPoints10Y3 = $scope.ExpenseListGraph[i].Total3Value;
                    $scope.pPoints10Y5 = $scope.ExpenseListGraph[i].Total5Value;
                    $scope.pPoints10Y10 = $scope.ExpenseListGraph[i].Total10Value;
                    $scope.pPoints10Y15 = $scope.ExpenseListGraph[i].Total15Value;
                    $scope.pPoints10Y20 = $scope.ExpenseListGraph[i].Total20Value;
                    $scope.pPoints10Y25 = $scope.ExpenseListGraph[i].Total25Value;
                    $scope.pPoints10Y30 = $scope.ExpenseListGraph[i].Total30Value;
                    $scope.pPoints10Y31 = $scope.ExpenseListGraph[i].Total31Value;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'Pending GRN Posting') {
                    $scope.pPoints11Y3 = $scope.ExpenseListGraph[i].Total3Value;
                    $scope.pPoints11Y5 = $scope.ExpenseListGraph[i].Total5Value;
                    $scope.pPoints11Y10 = $scope.ExpenseListGraph[i].Total10Value;
                    $scope.pPoints11Y15 = $scope.ExpenseListGraph[i].Total15Value;
                    $scope.pPoints11Y20 = $scope.ExpenseListGraph[i].Total20Value;
                    $scope.pPoints11Y25 = $scope.ExpenseListGraph[i].Total25Value;
                    $scope.pPoints11Y30 = $scope.ExpenseListGraph[i].Total30Value;
                    $scope.pPoints11Y31 = $scope.ExpenseListGraph[i].Total31Value;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'Pending Inventory Issue') {
                    $scope.pPoints12Y3 = $scope.ExpenseListGraph[i].Total3Value;
                    $scope.pPoints12Y5 = $scope.ExpenseListGraph[i].Total5Value;
                    $scope.pPoints12Y10 = $scope.ExpenseListGraph[i].Total10Value;
                    $scope.pPoints12Y15 = $scope.ExpenseListGraph[i].Total15Value;
                    $scope.pPoints12Y20 = $scope.ExpenseListGraph[i].Total20Value;
                    $scope.pPoints12Y25 = $scope.ExpenseListGraph[i].Total25Value;
                    $scope.pPoints12Y30 = $scope.ExpenseListGraph[i].Total30Value;
                    $scope.pPoints12Y31 = $scope.ExpenseListGraph[i].Total31Value;
                }
                else if ($scope.ExpenseListGraph[i].Category === 'Pending Inventory Issue Posting') {
                    $scope.pPoints13Y3 = $scope.ExpenseListGraph[i].Total3Value;
                    $scope.pPoints13Y5 = $scope.ExpenseListGraph[i].Total5Value;
                    $scope.pPoints13Y10 = $scope.ExpenseListGraph[i].Total10Value;
                    $scope.pPoints13Y15 = $scope.ExpenseListGraph[i].Total15Value;
                    $scope.pPoints13Y20 = $scope.ExpenseListGraph[i].Total20Value;
                    $scope.pPoints13Y25 = $scope.ExpenseListGraph[i].Total25Value;
                    $scope.pPoints13Y30 = $scope.ExpenseListGraph[i].Total30Value;
                    $scope.pPoints13Y31 = $scope.ExpenseListGraph[i].Total31Value;
                }
            }

        }
        $("#container").ejChart(
            {
                  
                //Initializing Primary X Axis	 
                primaryXAxis:
                {
                    //labelRotation: 45,
                    //title: { text: 'Month' },
                    majorGridLines: { visible: false },
                    //font: {
                    //        color: "transparent"
                    //    }
                    font: {
                        color: 'black',
                        fontWeight: 'bold',
                        size: '12px'
                    }
                },

                //Initializing Primary Y Axis	
                primaryYAxis:
                {
                    //title: { text: 'Number of visitors in Millions' },
                    //range: { min: 0, max: 400, interval: 25 },
                    range: { min: 0, max: $scope.pPoints13Y31},
                    font: {
                        color: 'black',
                        fontWeight: 'bold',
                        size: '12px'
                    }
                },

                //Initializing Common Properties for all the series
                commonSeriesOptions:
                {
                    type: 'stackingbar',//stackingcolumn
                    enableAnimation: true,                  
                    tooltip:
                    {
                        visible: true,
                        format: " #series.name#  <br/> #point.x# : #point.y#"
                    }
                },
                //size: {
                //    width: '500px',
                //    height: '550px'
                //},
                //x: {
                //    tick: {
                //        format: function (x) {
                //            return -x;
                //        }
                //    }
                //},
                //Initializing Series
                series:
                    [
                        //{
                        //    points:
                        //        [
                        //            { x: '<=3', y: $scope.Points1Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points1Y10 }, { x: '<=15', y: $scope.Points1Y15 },
                        //            { x: '<=20', y: $scope.Points1Y20 }, { x: '<=25', y: $scope.Points1Y25 }, { x: '<=30', y: $scope.Points1Y30 }, { x: '>30', y: $scope.Points1Y31 }
                        //        ],
                        //    name: 'Pending Requisition For PO'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: '<=3', y: $scope.Points2Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points2Y10 }, { x: '<=15', y: $scope.Points2Y15 },
                        //            { x: '<=20', y: $scope.Points2Y20 }, { x: '<=25', y: $scope.Points2Y25 }, { x: '<=30', y: $scope.Points2Y30 }, { x: '>30', y: $scope.Points2Y31 }
                        //        ],
                        //    name: 'PO Pending For GRN'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: '<=3', y: $scope.Points3Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points3Y10 }, { x: '<=15', y: $scope.Points3Y15 },
                        //            { x: '<=20', y: $scope.Points3Y20 }, { x: '<=25', y: $scope.Points3Y25 }, { x: '<=30', y: $scope.Points3Y30 }, { x: '>30', y: $scope.Points3Y31 }
                        //        ],
                        //    name: 'PO Pending For LC Taging'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: '<=3', y: $scope.Points4Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points4Y10 }, { x: '<=15', y: $scope.Points4Y15 },
                        //            { x: '<=20', y: $scope.Points4Y20 }, { x: '<=25', y: $scope.Points4Y25 }, { x: '<=30', y: $scope.Points4Y30 }, { x: '>30', y: $scope.Points4Y31 }
                        //        ],
                        //    name: 'PO Pending For Acceptance'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: '<=3', y: $scope.Points5Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points5Y10 }, { x: '<=15', y: $scope.Points5Y15 },
                        //            { x: '<=20', y: $scope.Points5Y20 }, { x: '<=25', y: $scope.Points1525 }, { x: '<=30', y: $scope.Points5Y30 }, { x: '>30', y: $scope.Points5Y31 }
                        //        ],
                        //    name: 'Acceptance Pending For GRN'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: '<=3', y: $scope.Points6Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points6Y10 }, { x: '<=15', y: $scope.Points6Y15 },
                        //            { x: '<=20', y: $scope.Points6Y20 }, { x: '<=25', y: $scope.Points6Y25 }, { x: '<=30', y: $scope.Points6Y30 }, { x: '>30', y: $scope.Points6Y31 }
                        //        ],
                        //    name: 'UnTag Gate Entry'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: '<=3', y: $scope.Points7Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points7Y10 }, { x: '<=15', y: $scope.Points7Y15 },
                        //            { x: '<=20', y: $scope.Points7Y20 }, { x: '<=25', y: $scope.Points7Y25 }, { x: '<=30', y: $scope.Points7Y30 }, { x: '>30', y: $scope.Points7Y31 }
                        //        ],
                        //    name: 'Pending GRN Posting'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: '<=3', y: $scope.Points8Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points8Y10 }, { x: '<=15', y: $scope.Points8Y15 },
                        //            { x: '<=20', y: $scope.Points8Y20 }, { x: '<=25', y: $scope.Points8Y25 }, { x: '<=30', y: $scope.Points8Y30 }, { x: '>30', y: $scope.Points8Y31 }
                        //        ],
                        //    name: 'Pending Inventory Issue'
                        //},
                        //{
                        //    points:
                        //        [
                        //            { x: '<=3', y: $scope.Points9Y3 }, { x: '<=5', y: $scope.Points1Y5 }, { x: '<=10', y: $scope.Points9Y10 }, { x: '<=15', y: $scope.Points9Y15 },
                        //            { x: '<=20', y: $scope.Points9Y20 }, { x: '<=25', y: $scope.Points9Y25 }, { x: '<=30', y: $scope.Points9Y30 }, { x: '>30', y: $scope.Points9Y31 }
                        //        ],
                        //    name: 'Pending Inventory Issue Posting'
                        //}


                        {
                            points:
                                [    
                                    { x: 'Pending Inventory Issue Posting', y: $scope.pPoints13Y3 },
                                    { x: 'Pending Inventory Issue', y: $scope.pPoints12Y3 },
                                    { x: 'Pending GRN Posting', y: $scope.pPoints11Y3 },
                                    { x: 'Pending GRN For Approval', y: $scope.pPoints10Y3 },
                                    { x: 'Pending For GRN', y: $scope.pPoints9Y3 },

                                    { x: 'Acceptance Pending For GRN', y: $scope.pPoints8Y3 },
                                    { x: 'PO Pending For Acceptance', y: $scope.pPoints7Y3 },
                                    { x: 'Invoice Pending For Acceptance', y: $scope.pPoints6Y3 },

                                    { x: 'PO Pending For LC Taging', y: $scope.pPoints5Y3 },
                                    { x: 'PO Pending For Purchase', y: $scope.pPoints4Y3 },
                                    { x: 'PO Pending For Approval', y: $scope.pPoints3Y3 },
                                    { x: 'Pending Requisition For PO', y: $scope.pPoints2Y3 },
                                    { x: 'Pending Requisition For Approval', y: $scope.pPoints1Y3 }
                                    

                                ],
                            name: '<=3',
                            fill: '#2ECC71',
                        },
                        {
                            points:
                                [
                                    { x: 'Pending Requisition For Approval', y: $scope.pPoints1Y5 },
                                    { x: 'Pending Requisition For PO', y: $scope.pPoints2Y5 },
                                    { x: 'PO Pending For Approval', y: $scope.pPoints3Y5 },
                                    { x: 'PO Pending For Purchase', y: $scope.pPoints4Y5 },
                                    { x: 'PO Pending For LC Taging', y: $scope.pPoints5Y5 },
                                    { x: 'Invoice Pending For Acceptance', y: $scope.pPoints6Y5 },
                                    { x: 'PO Pending For Acceptance', y: $scope.pPoints7Y5 },
                                    { x: 'Acceptance Pending For GRN', y: $scope.pPoints8Y5 },
                                    { x: 'Pending For GRN', y: $scope.pPoints9Y5 },
                                    { x: 'Pending GRN For Approval', y: $scope.pPoints10Y5 },
                                    { x: 'Pending GRN Posting', y: $scope.pPoints11Y5 },
                                    { x: 'Pending Inventory Issue', y: $scope.pPoints12Y5 },
                                    { x: 'Pending Inventory Issue Posting', y: $scope.pPoints13Y5 }
                                ],
                            name: '<=5',
                            fill:'#F1C40F'
                        },
                        {
                            points:
                                [
                                    { x: 'Pending Requisition For Approval', y: $scope.pPoints1Y10 },
                                    { x: 'Pending Requisition For PO', y: $scope.pPoints2Y10 },
                                    { x: 'PO Pending For Approval', y: $scope.pPoints3Y10 },
                                    { x: 'PO Pending For Purchase', y: $scope.pPoints4Y10 },
                                    { x: 'PO Pending For LC Taging', y: $scope.pPoints5Y10 },
                                    { x: 'Invoice Pending For Acceptance', y: $scope.pPoints6Y10 },
                                    { x: 'PO Pending For Acceptance', y: $scope.pPoints7Y5 },
                                    { x: 'Acceptance Pending For GRN', y: $scope.pPoints8Y10 },
                                    { x: 'Pending For GRN', y: $scope.pPoints9Y10 },
                                    { x: 'Pending GRN For Approval', y: $scope.pPoints10Y10 },
                                    { x: 'Pending GRN Posting', y: $scope.pPoints11Y10 },
                                    { x: 'Pending Inventory Issue', y: $scope.pPoints12Y10 },
                                    { x: 'Pending Inventory Issue Posting', y: $scope.pPoints13Y10 }
                                ],
                            name: '<=10',
                            //fill:'#E74C3C'
                            fill:'#e0918e'
                        },
                        {
                            points:
                                [
                                    { x: 'Pending Requisition For Approval', y: $scope.pPoints1Y15 },
                                    { x: 'Pending Requisition For PO', y: $scope.pPoints2Y15 },
                                    { x: 'PO Pending For Approval', y: $scope.pPoints3Y15 },
                                    { x: 'PO Pending For Purchase', y: $scope.pPoints4Y15 },
                                    { x: 'PO Pending For LC Taging', y: $scope.pPoints5Y15 },
                                    { x: 'Invoice Pending For Acceptance', y: $scope.pPoints6Y5 },
                                    { x: 'PO Pending For Acceptance', y: $scope.pPoints7Y15 },
                                    { x: 'Acceptance Pending For GRN', y: $scope.pPoints8Y15 },
                                    { x: 'Pending For GRN', y: $scope.pPoints9Y15 },
                                    { x: 'Pending GRN For Approval', y: $scope.pPoints10Y15 },
                                    { x: 'Pending GRN Posting', y: $scope.pPoints11Y15 },
                                    { x: 'Pending Inventory Issue', y: $scope.pPoints12Y15 },
                                    { x: 'Pending Inventory Issue Posting', y: $scope.pPoints13Y15 }
                                ],
                            name: '<=15',
                            //fill:'#52B2D9'
                            fill: '#008000'
                        },
                        {
                            points:
                                [
                                    { x: 'Pending Requisition For Approval', y: $scope.pPoints1Y20 },
                                    { x: 'Pending Requisition For PO', y: $scope.pPoints2Y20 },
                                    { x: 'PO Pending For Approval', y: $scope.pPoints3Y20 },
                                    { x: 'PO Pending For Purchase', y: $scope.pPoints4Y20 },
                                    { x: 'PO Pending For LC Taging', y: $scope.pPoints5Y20 },
                                    { x: 'Invoice Pending For Acceptance', y: $scope.pPoints6Y20 },
                                    { x: 'PO Pending For Acceptance', y: $scope.pPoints7Y20 },
                                    { x: 'Acceptance Pending For GRN', y: $scope.pPoints8Y20 },
                                    { x: 'Pending For GRN', y: $scope.pPoints9Y20 },
                                    { x: 'Pending GRN For Approval', y: $scope.pPoints10Y20 },
                                    { x: 'Pending GRN Posting', y: $scope.pPoints11Y20 },
                                    { x: 'Pending Inventory Issue', y: $scope.pPoints12Y20 },
                                    { x: 'Pending Inventory Issue Posting', y: $scope.pPoints13Y20 }
                                ],
                            name: '<=20',
                            fill:'#FDE3A7'
                        },
                        {
                            points:
                                [
                                    { x: 'Pending Requisition For Approval', y: $scope.pPoints1Y25 },
                                    { x: 'Pending Requisition For PO', y: $scope.pPoints2Y25 },
                                    { x: 'PO Pending For Approval', y: $scope.pPoints3Y25 },
                                    { x: 'PO Pending For Purchase', y: $scope.pPoints4Y25 },
                                    { x: 'PO Pending For LC Taging', y: $scope.pPoints5Y25 },
                                    { x: 'Invoice Pending For Acceptance', y: $scope.pPoints6Y25 },
                                    { x: 'PO Pending For Acceptance', y: $scope.pPoints7Y25 },
                                    { x: 'Acceptance Pending For GRN', y: $scope.pPoints8Y25 },
                                    { x: 'Pending For GRN', y: $scope.pPoints9Y25 },
                                    { x: 'Pending GRN For Approval', y: $scope.pPoints10Y25 },
                                    { x: 'Pending GRN Posting', y: $scope.pPoints11Y25 },
                                    { x: 'Pending Inventory Issue', y: $scope.pPoints12Y25 },
                                    { x: 'Pending Inventory Issue Posting', y: $scope.pPoints13Y25 }
                                ],
                            name: '<=25',
                            //fill:'LIGHT PURPLE'
                            fill:'#008b8b'
                        },
                        {
                            points:
                                [
                                    { x: 'Pending Requisition For Approval', y: $scope.pPoints1Y30 },
                                    { x: 'Pending Requisition For PO', y: $scope.pPoints2Y30 },
                                    { x: 'PO Pending For Approval', y: $scope.pPoints3Y30 },
                                    { x: 'PO Pending For Purchase', y: $scope.pPoints4Y30 },
                                    { x: 'PO Pending For LC Taging', y: $scope.pPoints5Y30 },
                                    { x: 'Invoice Pending For Acceptance', y: $scope.pPoints6Y30 },
                                    { x: 'PO Pending For Acceptance', y: $scope.pPoints7Y30 },
                                    { x: 'Acceptance Pending For GRN', y: $scope.pPoints8Y30 },
                                    { x: 'Pending For GRN', y: $scope.pPoints9Y30 },
                                    { x: 'Pending GRN For Approval', y: $scope.pPoints10Y30 },
                                    { x: 'Pending GRN Posting', y: $scope.pPoints11Y30 },
                                    { x: 'Pending Inventory Issue', y: $scope.pPoints12Y30 },
                                    { x: 'Pending Inventory Issue Posting', y: $scope.pPoints13Y30 }
                                ],
                            name: '<=30',
                            //fill:'BROWN'
                            fill:'#52B2D9'
                        },
                        {
                            points:
                                [
                                    { x: 'Pending Requisition For Approval', y: $scope.pPoints1Y31 },
                                    { x: 'Pending Requisition For PO', y: $scope.pPoints2Y31 },
                                    { x: 'PO Pending For Approval', y: $scope.pPoints3Y31 },
                                    { x: 'PO Pending For Purchase', y: $scope.pPoints4Y31 },
                                    { x: 'PO Pending For LC Taging', y: $scope.pPoints5Y31 },
                                    { x: 'Invoice Pending For Acceptance', y: $scope.pPoints6Y31 },
                                    { x: 'PO Pending For Acceptance', y: $scope.pPoints7Y31 },
                                    { x: 'Acceptance Pending For GRN', y: $scope.pPoints8Y31 },
                                    { x: 'Pending For GRN', y: $scope.pPoints9Y31 },
                                    { x: 'Pending GRN For Approval', y: $scope.pPoints10Y31 },
                                    { x: 'Pending GRN Posting', y: $scope.pPoints11Y31 },
                                    { x: 'Pending Inventory Issue', y: $scope.pPoints12Y31 },
                                    { x: 'Pending Inventory Issue Posting', y: $scope.pPoints13Y31 }
                                ],
                            name: '>30',
                            fill:'#E74C3C'
                        }

                    ],

                isResponsive: true,
                load: "loadTheme",
                title: { text: 'Material Management Activity Status' },
                size: { height: "600" },               
                legend: { visible: true }
              
            });

    }


    $scope.onrowdataboundColor = function (e) {
        if (e.data.AuthorizedByStatus === 'Hold' || e.data.CheckedByStatus==='Hold')
            e.row.css("background-color", "#f27935");//orange
        if (e.data.AuthorizedByStatus === 'Approved')
            e.row.css("background-color", "#c8f7c5");// very soft line green//#2ECC71
        if (e.data.AuthorizedByStatus === 'To be approved')
            e.row.css("background-color", "#f1c40f");//vivid yellow//FDE3A7
        if (e.data.AuthorizedByStatus === 'To be Checked')
            e.row.css("background-color", "#e08283");//vivid yellow//FDE3A7
    };

    $scope.onrowdataboundColor1 = function (e) {
        //debugger;
        if (e.data.AuthorizedByStatus === 'Hold' || e.data.CheckedByStatus === 'Hold')
            e.row.css("background-color", "#f27935");//orange
        else if (e.data.AuthorizedByStatus === 'Approved')
            e.row.css("background-color", "#c8f7c5");// very soft line green//#2ECC71
        else if (e.data.AuthorizedByStatus === 'To be approved')
            e.row.css("background-color", "#f1c40f");//vivid yellow//FDE3A7
        else if (e.data.AuthorizedByStatus === 'To be Checked')
            e.row.css("background-color", "#e08283");//vivid yellow//FDE3A7
    };
}


