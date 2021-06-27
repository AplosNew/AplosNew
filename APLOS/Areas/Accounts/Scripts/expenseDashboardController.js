'use strict';
expenseDashboardController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$window'];
function expenseDashboardController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $window) {
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
    $scope.dataGrid = null;
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

    $scope.GetCompanyInformation = function () {

        $http({

            method: 'GET',
            url: 'Accounts/ExpenseDashboard/GetCompanyInformation/',
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.companyInformation = response.data;
            $scope.BaseCurrencyCode = $scope.companyInformation[0].BaseCurrencyCode;
        });
    };

    $scope.GetCompanyInformation();
    $scope.GetVoucherLatestDate = function () {

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
            $scope.GetExpenseList();
        });
    };

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


    //--------------End of Update 5-September-2019

    $scope.GetExpenseList = function () {

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
            url: 'accounts/ExpenseDashboard/ExpenseList',
            data: {
                'factDate': $scope.expFactDate.factDate,
                'fromDate': $scope.dateRange.fromDate,
                'toDate': $scope.dateRange.toDate
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setList(response.data);
            createColList();
        });
        $http({
            method: 'POST',
            url: 'accounts/ExpenseDashboard/ExpenseListLineChart',
            data: {
                'factDate': $scope.expFactDate.factDate,
                'fromDate': $scope.dateRange.fromDate,
                'toDate': $scope.dateRange.toDate
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.expensePeriodList = response.data;
            $scope.ExpenseListTotal = $scope.expensePeriodList.reduce(function (sum, expense) {
                return sum + expense.Amount;
            }, 0);
            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/RevenueListLineChart',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.revenuePeriodList = response.data;
                $scope.RevenueListTotal = $scope.revenuePeriodList.reduce(function (sum, expense) {
                    return sum + expense.Amount;
                }, 0);
                if ($scope.expensePeriodList.length > 0 || $scope.revenuePeriodList.length > 0) {
                    setLineList($scope.fiscalYearList, $scope.expensePeriodList, $scope.revenuePeriodList);
                    //createLineChart();
                }
            });
        });

        $http({
            method: 'POST',
            url: 'accounts/ExpenseDashboard/PeriodWiseExpenseBarChart',
            data: {
                'factDate': $scope.expFactDate.factDate,
                'fromDate': $scope.dateRange.fromDate,
                'toDate': $scope.dateRange.toDate
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BudgetedExpenseList = response.data;


            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/PeriodWiseRevenueBarChart',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.BudgetedRevenueList = response.data;

                setPeriodicBudgetList($scope.BudgetedExpenseList, $scope.BudgetedRevenueList);
                createPeriodicalBudgetedBarChart();
            });


        });


        $http({
            method: 'POST',
            url: 'accounts/ExpenseDashboard/MonthlyExpenseVSBudgetBarChart',
            data: {
                'factDate': $scope.expFactDate.factDate,
                'fromDate': $scope.dateRange.fromDate,
                'toDate': $scope.dateRange.toDate
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MonthlyExpenseVSBudget = response.data;


            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/MonthlyRevenueVSBudgetBarChart',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.MonthlyRevenueVSBudget = response.data;

                setMonthlyBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
                createLineChart();
            });
        });
        $http({
            method: 'POST',
            url: 'accounts/ExpenseDashboard/PeriodExpenseVSBudgetBarChart',
            data: {
                'factDate': $scope.expFactDate.factDate,
                'fromDate': $scope.dateRange.fromDate,
                'toDate': $scope.dateRange.toDate
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MonthlyExpenseVSBudget = response.data;


            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/PeriodRevenueVSBudgetBarChart',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.MonthlyRevenueVSBudget = response.data;

                setPeriodWiseBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
                createPeriodBarChart();
            });
        });

    };

    $scope.GetVoucherLatestDate();

    $scope.GetDetailDrillDownTableJS = function (data) {
        $scope.DDList = [];
        $scope.expenseList = [];
        $scope.revenueList = [];
        $scope.ExpenseListTotal = 0;
        $scope.RevenueListTotal = 0;


        if ($scope.index + 3 < $scope.ColList.length) {
            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/DymnamicExpenseList/',
                data: {
                    'ChartColumnList': $scope.ColList,
                    'seq': $scope.index,
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.DDList = response.data;
                setList(response.data);
                $scope.index += 1;
                $scope.stIndex = $scope.index - 1;

            });

            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/DymnamicExpenseListLineChart/',
                data: {
                    'ChartColumnList': $scope.ColList,
                    'seq': $scope.index,
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                var seque = $scope.index;
                $scope.expensePeriodList = response.data;

                for (var i = 0; i < $scope.expensePeriodList.length; i++) {
                    $scope.ExpenseListTotal += $scope.revenuePeriodList.Amount;
                }

                $http({
                    method: 'POST',
                    url: 'accounts/ExpenseDashboard/DymnamicRevenueListLineChart/',
                    data: {
                        'ChartColumnList': $scope.ColList,
                        'seq': seque,
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.revenuePeriodList = response.data;

                    for (var i = 0; i < $scope.revenuePeriodList.length; i++) {
                        $scope.RevenueListTotal += $scope.revenuePeriodList.Amount;
                    }

                    //if ($scope.expensePeriodList.length > 0 || $scope.revenuePeriodList.length > 0) {
                    setLineList($scope.fiscalYearList, $scope.expensePeriodList, $scope.revenuePeriodList);

                    //createLineChart();
                    //}
                });

            });
            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/DynamicPeriodWiseExpenseBarChart',
                data: {
                    'ChartColumnList': $scope.ColList,
                    'seq': $scope.index,
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.BudgetedExpenseList = response.data;


                $http({
                    method: 'POST',
                    url: 'accounts/ExpenseDashboard/DynamicPeriodWiseRevenueBarChart',
                    data: {
                        'ChartColumnList': $scope.ColList,
                        'seq': $scope.index,
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.BudgetedRevenueList = response.data;
                    setMonthlyBudgetVSExpenseList($scope.BudgetedExpenseList, $scope.BudgetedRevenueList);
                    createPeriodicalBudgetedBarChart();


                });

            });

            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/MonthlyDynamicExpenseVSBudgetBarChart',
                data: {
                    'ChartColumnList': $scope.ColList,
                    'seq': $scope.index,
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.MonthlyExpenseVSBudget = response.data;


                $http({
                    method: 'POST',
                    url: 'accounts/ExpenseDashboard/MonthlyDynamicRevenueVSBudgetBarChart',
                    data: {
                        'ChartColumnList': $scope.ColList,
                        'seq': $scope.index,
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.MonthlyRevenueVSBudget = response.data;


                    setPeriodicBudgetList($scope.BudgetedExpenseList, $scope.BudgetedRevenueList);
                    createPeriodicalBudgetedBarChart();
                });
            });
            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/PeriodDynamicExpenseVSBudgetBarChart',
                data: {
                    'ChartColumnList': $scope.ColList,
                    'seq': $scope.index,
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.PeriodExpenseVSBudget = response.data;

                $http({
                    method: 'POST',
                    url: 'accounts/ExpenseDashboard/PeriodDynamicRevenueVSBudgetBarChart',
                    data: {
                        'ChartColumnList': $scope.ColList,
                        'seq': $scope.index,
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.PeriodRevenueVSBudget = response.data;

                    setPeriodWiseBudgetVSExpenseList($scope.PeriodExpenseVSBudget, $scope.PeriodRevenueVSBudget);
                    createPeriodBarChart();
                });
            });
        }
    };

    $scope.budgetedExpenseList = [];

    $scope.budgetedExpenseChartLabel = [];

    var unique_array = [];


    $scope.dFunction = function () {
        if (new Date($scope.dateRange.toDate) < new Date($scope.dateRange.fromDate)) {
            throw ShowResult("From date can not be greater then to date", 'failure');
        }
        else {
            $scope.expensePeriodList = [];
            $scope.ExpenseListTotal = 0;
            $scope.revenuePeriodList = [];
            $scope.RevenueListTotal = 0;
            $scope.GetFiscalYear();
            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/ExpenseList/',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
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
            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/ExpenseListLineChart',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.expensePeriodList = response.data;
                $scope.ExpenseListTotal = $scope.expensePeriodList.reduce(function (sum, expense) {
                    return sum + expense.Amount;
                }, 0);
                $http({
                    method: 'POST',
                    url: 'accounts/ExpenseDashboard/RevenueListLineChart',
                    data: {
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.revenuePeriodList = response.data;
                    $scope.RevenueListTotal = $scope.revenuePeriodList.reduce(function (sum, expense) {
                        return sum + expense.Amount;
                    }, 0);
                    if ($scope.expensePeriodList.length > 0 || $scope.revenuePeriodList.length > 0) {
                        setLineList($scope.fiscalYearList, $scope.expensePeriodList, $scope.revenuePeriodList);
                        //createLineChart();
                    }
                });

            });
            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/PeriodWiseExpenseBarChart',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.BudgetedExpenseList = response.data;

                $http({
                    method: 'POST',
                    url: 'accounts/ExpenseDashboard/PeriodWiseRevenueBarChart',
                    data: {
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.BudgetedRevenueList = response.data;
                    setPeriodicBudgetList($scope.BudgetedExpenseList, $scope.BudgetedRevenueList);
                    createPeriodicalBudgetedBarChart();
                });

            });

            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/MonthlyExpenseVSBudgetBarChart',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.MonthlyExpenseVSBudget = response.data;


                $http({
                    method: 'POST',
                    url: 'accounts/ExpenseDashboard/MonthlyRevenueVSBudgetBarChart',
                    data: {
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.MonthlyRevenueVSBudget = response.data;

                    setMonthlyBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
                    createLineChart();
                });
            });
            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/PeriodExpenseVSBudgetBarChart',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.MonthlyExpenseVSBudget = response.data;


                $http({
                    method: 'POST',
                    url: 'accounts/ExpenseDashboard/PeriodRevenueVSBudgetBarChart',
                    data: {
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.MonthlyRevenueVSBudget = response.data;

                    setPeriodWiseBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
                    createPeriodBarChart();
                });
            });
        }
    };

    function setList(list) {
        $scope.ExpenseList = [];
        $scope.ExpenseList = list;
        $scope.chartLabel = [];
        $scope.chartList = [];
    }
    //-----------------RemoveDuplicates------------------------
    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }
    //-----------------RemoveDuplicatesEnd------------------------

    $scope.exceptionList = {
        entryPeriod: null,
        postingPeriod: null,
        normalPostedAmout: null,
        delayPostedAmount: null
    };
    $scope.listForChart = [];


    function setLineList(fiscalYearList, Expenselist, RevenueList) {
        $scope.ExpenseLineList = [];
        $scope.listForChart = [];
        $scope.chartLabel = [];
        $scope.chartExpenseList = [];
        $scope.chartRevenueList = [];
        $scope.chartDelayExpenseList = [];
        $scope.chartDelayRevenueList = [];

        angular.forEach(fiscalYearList, function (item, j) {
            var row = {
                fiscalPeriod: null,
                ExpenseAmount: 0,
                ExpenseDelayAmount: 0,
                RevenueAmount: 0,
                RevenueDelayAmount: 0
            };
            var expenseObj = Expenselist.filter(x => x.PostingPeriodId === item.FiscalYearPeriodId);
            var revenueObj = RevenueList.filter(x => x.PostingPeriodId === item.FiscalYearPeriodId);

            row.fiscalPeriod = item.PeriodName;

            if (expenseObj.length > 0) {
                for (var ei = 0; ei < expenseObj.length; ei++) {
                    if (expenseObj[ei].PostingPeriod !== expenseObj[ei].EntryPeriod) {
                        if (new Date(expenseObj[ei].EntryPeriodEndDate) > new Date(expenseObj[ei].PostingPeriodEndDate))
                            row.ExpenseDelayAmount += expenseObj[ei].Amount;
                    }
                    else {
                        row.ExpenseAmount += expenseObj[ei].Amount;
                    }

                }
            }
            if (revenueObj.length > 0) {
                for (var ri = 0; ri < revenueObj.length; ri++) {
                    if (revenueObj[ri].PostingPeriod !== revenueObj[ri].EntryPeriod) {
                        if (new Date(revenueObj[ri].EntryPeriodEndDate) > new Date(revenueObj[ri].PostingPeriodEndDate))
                            row.RevenueDelayAmount += revenueObj[ri].Amount;
                    }
                    else {
                        row.RevenueAmount += revenueObj[ri].Amount;
                    }

                }
            }
            $scope.listForChart.push(row);
        });

        $scope.ExpenseLineList = $scope.listForChart;

        angular.forEach($scope.ExpenseLineList, function (item, i) {

            $scope.chartLabel.push(item.fiscalPeriod);
            $scope.chartExpenseList.push(item.ExpenseAmount);
            $scope.chartDelayExpenseList.push(item.ExpenseDelayAmount);
            $scope.chartRevenueList.push(item.RevenueAmount);
            $scope.chartDelayRevenueList.push(item.RevenueDelayAmount);
        });
    }

    function setPeriodicBudgetList(Expenselist, RevenueList) {
        $scope.ExpenseLineList = [];
        $scope.listForChart = [];
        $scope.chartLabel = [];
        $scope.chartExpenseList = [];
        $scope.chartBudgetedExpenseList = [];

        $scope.chartRevenueList = [];
        $scope.chartBudgetedRevenueList = [];

        $scope.BudgetedChartLabel = [];
        $scope.BudgetedchartExpenseList = [];
        $scope.BudgetedChartBudgeted = [];

        $scope.ExpenseLineList = $scope.listForChart;

        angular.forEach(Expenselist, function (item, i) {

            $scope.BudgetedChartLabel.push(item.PostingPeriod);
            $scope.BudgetedchartExpenseList.push(item.Amount);
            $scope.BudgetedChartBudgeted.push(item.BudgetAmount);
        });


        angular.forEach(RevenueList, function (item, i) {

            $scope.chartRevenueList.push(item.Amount);
            $scope.chartBudgetedRevenueList.push(item.BudgetAmount);
        });
    }

    function setMonthlyBudgetVSExpenseList(Expenselist, RevenueList) {

        $scope.MonthlyBudgetVSExpense = [];

        $scope.MonthlyBudgetVSExpense.push(Expenselist[0].BudgetAmount);
        $scope.MonthlyBudgetVSExpense.push(Expenselist[0].Amount);

        $scope.MonthlyBudgetVSExpense.push(RevenueList[0].BudgetAmount);
        $scope.MonthlyBudgetVSExpense.push(RevenueList[0].Amount);
    }
    function setPeriodWiseBudgetVSExpenseList(Expenselist, RevenueList) {
        $scope.PeriodicBudgetVSExpense = [];

        $scope.PeriodicBudgetVSExpense.push(Expenselist[0].BudgetAmount);
        $scope.PeriodicBudgetVSExpense.push(Expenselist[0].Amount);

        $scope.PeriodicBudgetVSExpense.push(RevenueList[0].BudgetAmount);
        $scope.PeriodicBudgetVSExpense.push(RevenueList[0].Amount);
    }


    function getDrillDownList() {
        $http({
            method: 'POST',
            url: 'accounts/expenseDashboard/OrgStructureList',
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
    console.log("ColList Length", $scope.ColList[$scope.ColList.length]);

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
        if (x.Sequence !== -2) {
            $scope.setIndexHead(x);
            $scope.GetDetailDrillDownTableJS(x.Id);
        }
        else {
            $scope.setIndexHead(x);
            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/ExpenseList',
                data: {

                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                setList(response.data);
                $scope.index = -1;
                $scope.stIndex = $scope.index - 1;
            });
            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/ExpenseListLineChart',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.expensePeriodList = response.data;
                $http({
                    method: 'POST',
                    url: 'accounts/ExpenseDashboard/RevenueListLineChart',
                    data: {
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.revenuePeriodList = response.data;
                });
                if ($scope.expensePeriodList.length > 0 || $scope.revenuePeriodList.length > 0) {
                    setLineList($scope.fiscalYearList, $scope.expensePeriodList, $scope.revenuePeriodList);

                    //createLineChart();
                }
            });
            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/PeriodWiseExpenseBarChart',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.BudgetedExpenseList = response.data;

                $http({
                    method: 'POST',
                    url: 'accounts/ExpenseDashboard/PeriodWiseRevenueBarChart',
                    data: {
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.BudgetedRevenueList = response.data;
                    setPeriodicBudgetList($scope.BudgetedExpenseList, $scope.BudgetedRevenueList);
                    createPeriodicalBudgetedBarChart();
                });
            });

            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/MonthlyExpenseVSBudgetBarChart',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.MonthlyExpenseVSBudget = response.data;


                $http({
                    method: 'POST',
                    url: 'accounts/ExpenseDashboard/MonthlyRevenueVSBudgetBarChart',
                    data: {
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.MonthlyRevenueVSBudget = response.data;

                    setMonthlyBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
                    createLineChart();
                });
            });
            $http({
                method: 'POST',
                url: 'accounts/ExpenseDashboard/PeriodExpenseVSBudgetBarChart',
                data: {
                    'factDate': $scope.expFactDate.factDate,
                    'fromDate': $scope.dateRange.fromDate,
                    'toDate': $scope.dateRange.toDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.MonthlyExpenseVSBudget = response.data;


                $http({
                    method: 'POST',
                    url: 'accounts/ExpenseDashboard/PeriodRevenueVSBudgetBarChart',
                    data: {
                        'factDate': $scope.expFactDate.factDate,
                        'fromDate': $scope.dateRange.fromDate,
                        'toDate': $scope.dateRange.toDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.MonthlyRevenueVSBudget = response.data;

                    setPeriodWiseBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
                    createPeriodBarChart();
                });
            });
        }
    };

    function createLineChart() {
        Chart.defaults.global.legend.display = false;
        var MPctx = null;
        if ($scope.MonthlyBudgetVSExpense[0] === 0 && $scope.MonthlyBudgetVSExpense[1] === 0 && $scope.MonthlyBudgetVSExpense[2] === 0 && $scope.MonthlyBudgetVSExpense[3] === 0) {
            expensebarChart.destroy();
        }
        else {

            MPctx = document.getElementById("expenseBarChart").getContext('2d');
            if (expensebarChart !== undefined && typeof expensebarChart === 'object' && typeof expensebarChart.destroy === 'function') expensebarChart.destroy();
            expensebarChart = new Chart(MPctx, {
                // The type of chart we want to create
                type: 'bar',

                // The data for our dataset
                data: {
                    labels: ['Expense Budget', 'Expense Actual', 'Revenue Budget', 'Revenue Actual'],
                    datasets: [{

                        data: $scope.MonthlyBudgetVSExpense,//$scope.chartExpenseList,
                        backgroundColor: [window.chartColors.yellow, window.chartColors.blue, window.chartColors.orange, window.chartColors.green],
                        borderColor: [window.chartColors.yellow, window.chartColors.blue, window.chartColors.orange, window.chartColors.green],
                        fill: true,
                        borderWidth: 2
                    }

                    ]
                },

                // Configuration options go here
                options: {
                    legend: {
                        display: false,
                        labels: {
                            border: 1
                        }
                    },
                    title: {
                        display: true,
                        text: 'Budget VS Actual(' + $scope.dateRange.toDate.substring(3, 11) + ')',
                        position: 'top'
                    },
                    hover: {
                        mode: 'nearest',
                        intersect: true
                    },
                    tooltips: {
                        mode: 'index',
                        intersect: false,
                        label: function (tooltipItem) {
                            return tooltipItem.yLabel;
                        }
                    },
                    scales: {
                        yAxes: [{
                            stacked: false,
                            ticks: {
                                beginAtZero: true,
                                userCallback: function (value, index, values) {
                                    // Convert the number to a string and splite the string every 3 charaters from the end
                                    value = value.toString();
                                    value = value.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
                                    return value;
                                }
                            },
                            scaleLabel: {
                                display: true,
                                labelString: $scope.BaseCurrencyCode
                            }

                        }],
                        xAxes: [{
                            stacked: false,
                            ticks: {
                                beginAtZero: true,
                                autoSkip: false,
                                maxRotation: 90,
                                minRotation: 90
                            }
                        }]
                    },
                    elements: {
                        line: {
                            tension: 1
                        }
                    }
                }
            });

        }


    }



    function createPeriodBarChart() {//Function for generating periodic bar chart.
        Chart.defaults.global.legend.display = false;

        if ($scope.PeriodicBudgetVSExpense[0] === 0 && $scope.PeriodicBudgetVSExpense[1] === 0 && $scope.PeriodicBudgetVSExpense[2] === 0 && $scope.PeriodicBudgetVSExpense[3] === 0) {

            periodExpenseBarChart.destroy();


        }
        else {

            var MPctx = document.getElementById("periodExpenseBarChart").getContext('2d');
            if (periodExpenseBarChart !== undefined && typeof periodExpenseBarChart === 'object' && typeof periodExpenseBarChart.destroy === 'function') periodExpenseBarChart.destroy();
            periodExpenseBarChart = new Chart(MPctx, {
                type: 'bar',
                data: {
                    labels: ['Expense Budget', 'Expense Actual', 'Revenue Budget', 'Revenue Actual'],
                    datasets: [{

                        data: $scope.PeriodicBudgetVSExpense,//$scope.chartExpenseList,
                        backgroundColor: [window.chartColors.yellow, window.chartColors.blue, window.chartColors.orange, window.chartColors.green],
                        borderColor: [window.chartColors.yellow, window.chartColors.blue, window.chartColors.orange, window.chartColors.green],
                        fill: true,
                        borderWidth: 2
                    }]
                },
                // Configuration options go here
                options: {
                    legend: {
                        display: false,
                        labels: {
                            border: 1
                        }
                    },
                    title: {
                        display: true,
                        text: 'Budget VS Actual For the Period',
                        position: 'top'
                    },
                    hover: {
                        mode: 'nearest',
                        intersect: true
                    },
                    tooltips: {
                        mode: 'index',
                        intersect: false,
                        label: function (tooltipItem) {
                            return tooltipItem.yLabel;
                        }
                    },
                    scales: {
                        yAxes: [{
                            stacked: false,
                            ticks: {
                                beginAtZero: true,
                                userCallback: function (value, index, values) {
                                    // Convert the number to a string and splite the string every 3 charaters from the end
                                    value = value.toString();
                                    value = value.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
                                    return value;
                                }
                            },
                            scaleLabel: {
                                display: true,
                                labelString: $scope.BaseCurrencyCode
                            }
                        }],
                        xAxes: [{
                            stacked: false,
                            ticks: {
                                beginAtZero: true,
                                autoSkip: false,
                                maxRotation: 90,
                                minRotation: 90
                            }
                        }]
                    },
                    elements: {
                        line: {
                            tension: 1
                        }
                    }
                }
            });
        }

    }


    function createPeriodicalBudgetedBarChart() {
        Chart.defaults.global.legend.display = false;
        var MPctx = document.getElementById("periodicBudgetedAountChart").getContext('2d');
        if (periodWiseExpenseChart !== undefined && typeof periodWiseExpenseChart === 'object' && typeof periodWiseExpenseChart.destroy === 'function') periodWiseExpenseChart.destroy();
        periodWiseExpenseChart = new Chart(MPctx, {
            type: 'bar',
            data: {
                labels: $scope.BudgetedChartLabel,
                datasets: [{
                    label: 'Budgeted Expense Amount',
                    data: $scope.BudgetedChartBudgeted,
                    backgroundColor: window.chartColors.yellow,
                    borderColor: window.chartColors.yellow,
                    fill: true,
                    borderWidth: 2
                },
                {
                    label: 'Expense Amount',
                    data: $scope.BudgetedchartExpenseList,
                    backgroundColor: window.chartColors.blue,
                    borderColor: window.chartColors.blue,
                    fill: true,
                    borderWidth: 2
                },

                {
                    label: 'Budgeted Revenue Amount',
                    data: $scope.chartBudgetedRevenueList,
                    backgroundColor: window.chartColors.green,
                    borderColor: window.chartColors.green,
                    fill: true,
                    borderWidth: 2
                },
                {
                    label: 'Revenue Amount',
                    data: $scope.chartRevenueList,
                    backgroundColor: window.chartColors.orange,
                    borderColor: window.chartColors.orange,
                    fill: true,
                    borderWidth: 2
                }]
            },
            options: {
                legend: {
                    display: true,
                    labels: {
                        border: 1
                    }//,
                    //onClick: (e) => e.stopPropagation()
                },
                title: {
                    display: true,
                    text: 'Period Wise Budget VS Actual (Expense and Revenue)',
                    position: 'bottom'
                },
                hover: {
                    mode: 'nearest',
                    intersect: true
                },
                tooltips: {
                    mode: 'index',
                    intersect: true
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,
                            userCallback: function (value, index, values) {
                                // Convert the number to a string and splite the string every 3 charaters from the end
                                value = value.toString();
                                value = value.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
                                return value;
                            }
                        },
                        scaleLabel: {
                            display: true,
                            labelString: $scope.BaseCurrencyCode
                        }
                    }],
                    xAxes: [{
                        //stacked: true,
                        ticks: {
                            beginAtZero: true,
                            autoSkip: false,
                            maxRotation: 90,
                            minRotation: 90
                        }
                    }]
                },
                elements: {
                    line: {
                        tension: 0
                    }
                }
            }
        });
    }
    var fill;


    //----------------------------------------Modal---------------------------------------------//
    $scope.budgetWiseExpenseTotal = 0;
    $scope.GetBudgetWiseExpenseDetailJS = function (data, expenseType, periodType) {
        $scope.expenseType = expenseType;
        $scope.periodType = periodType;
        $scope.budgetWiseExpenseTotal = 0;
        $scope.BudgetWiseExpenseDetailList = [];

        if (periodType === '') {
            var fromDate = null;
            var toDate = null;
            if ($scope.expFactDate.factDate === 'postingDate') {
                fromDate = data.PostingPeriodStartDate;
                toDate = data.PostingPeriodEndDate;
            }
            if ($scope.expFactDate.factDate === 'AddedDate') {
                fromDate = data.EntryPeriodStartDate;
                toDate = data.EntryPeriodEndDate;
            }
            $http({
                method: 'POST',
                url: 'Accounts/ExpenseDashboard/ModalBudgetWiseExpense',
                data: {
                    'chartColumnList': $scope.ColList
                    , 'seq': $scope.stIndex
                    , 'budgetId': data.BudgetId
                    , 'factDate': $scope.expFactDate.factDate
                    //, 'fromDate': fromDate
                    //, 'toDate': toDate 
                    , 'fromDate': $scope.dateRange.fromDate
                    , 'toDate': $scope.dateRange.toDate
                    , 'expenseRevenue': $scope.expenseType
                    , 'periodType': $scope.periodType
                    , 'postingPeriodId': data.PostingPeriodId
                    , 'entryPeriodId': data.EntryPeriodId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.BudgetWiseExpenseDetailList = response.data;

                $scope.dataGrid = "#BudgetAmountDetailList";

                $scope.budgetWiseExpenseTotal = $scope.BudgetWiseExpenseDetailList.reduce(function (sum, expense) {
                    return sum + expense.Amount;
                }, 0);
                angular.element(document.querySelector('#BudgetWiseExpenseListModal')).modal('show');
            });
        }
        else {
            $scope.setModal(data);

            $http({
                method: 'POST',
                url: 'Accounts/ExpenseDashboard/ModalBudgetWiseExpense',
                data: {
                    'chartColumnList': $scope.ColList
                    , 'seq': $scope.index
                    , 'budgetId': data.BudgetId
                    , 'factDate': $scope.expFactDate.factDate
                    , 'fromDate': $scope.dateRange.fromDate
                    , 'toDate': $scope.dateRange.toDate
                    , 'expenseRevenue': $scope.expenseType
                    , 'periodType': $scope.periodType
                    , 'postingPeriodId': ''
                    , 'entryPeriodId': ''
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.BudgetWiseExpenseDetailList = response.data;
                $scope.dataGrid = "#BudgetAmountDetailList";
                $scope.budgetWiseExpenseTotal = $scope.BudgetWiseExpenseDetailList.reduce(function (sum, expense) {
                    return sum + expense.Amount;
                }, 0);
                angular.element(document.querySelector('#BudgetWiseExpenseListModal')).modal('show');
            });
        }
    };

    $scope.GetExpenseDetailJS = function (data) {
        $scope.ExpenseDetailList = [];
        //$scope.setModal(data.data);

        $http({
            method: 'POST',
            url: 'Accounts/ExpenseDashboard/ModalExpenseDetail',
            data: {
                'chartColumnList': $scope.ColList
                , 'seq': $scope.index
                , 'budgetId': data.data.BudgetId
                , 'factDate': $scope.expFactDate.factDate
                , 'fromDate': $scope.dateRange.fromDate
                , 'toDate': $scope.dateRange.toDate
                , 'entryPeriodId': data.data.EntryPeriodId
                , 'postingPeriodId': data.data.PostingPeriodId
                , 'expenseORRevenue': $scope.expenseType
                , 'periodType': $scope.periodType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ExpenseDetailList = response.data;

            $scope.dataGrid = "#ExpenseDetailList";
            $scope.ExpenseDetailListTotal = $scope.ExpenseDetailList.reduce(function (sum, expense) {
                return sum + expense.Amount;
            }, 0);
            angular.element(document.querySelector('#ExpenseListModal')).modal('show');
        });
    };

    $scope.VoucharParameters = {
        limit: 20,
        offset: 0,
        order: 'asc',
        sort: '',
        searchBy: "",
        pageSize: 20,
        total_count: 0,
        search: null,
        serverPagination: false
    };
    $scope.voucharNo = "";
    $scope.voucharId = "";
  
    $scope.GetVoucharDetailJS = function (data) {
        var reportFormat = "Pdf";
        var file_src = "";
        if (baseService.isUndefinedOrNull(data.data.VoucherId))
            return ShowResult('No Id found', 'failure');
        else {
            file_src = 'Accounts/VoucherReport/GetCommonVoucherReport?reportFormat=' + 'Pdf' + '&compnayGroupId=' + data.data.CompanyGroupId + '&companyId=' + data.data.CompanyId + '&plantId=' + data.data.PlantId + '&sourceType=' + data.data.SourceType + '&voucherId=' + data.data.VoucherId;

            $window.open(file_src, '_blank');
        }
    };

    $scope.setModal = function (x) {
        for (var i = 0; i < $scope.ColList.length; i++) {
            if ($scope.ColList[i].Sequence === $scope.index) {
                $scope.ColList[i].Id = x.CompanyId;
                $scope.ColList[i].Text = x.UId;
            }
        }
    };
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.PrintGRDes = function () {
        var gridObj = $($scope.dataGrid).data("ejGrid");
        var data = gridObj.model.dataSource();
        //data = ej.DataManager(data).executeLocal(ej.Query().select(["EmployeeName", "EmployeeCode", "Shift", "Designation", "EmpCategory", "DOJ", "OperationActivityName", "OperationMasterName", "OperationCode", "CompanyName", "Plant", "Department", "Line", "CellPhnNo"]));
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': data }
            // dataType: 'JSON'
            //, contentType: "application/json charset=utf-8"
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


