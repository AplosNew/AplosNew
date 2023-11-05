'use strict';
IncomeStatementReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window'];
function IncomeStatementReportController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $rootScope.title = 'Income Statement';
    $scope.parallelCurrencyList = [];

    $scope.incomeStatementReport = {
        Date: $filter('dateFiltering')(Date.now()),
        Type: 'AsOnDate',
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        IsUpToLevel: null,
        IsBudgetLevel: false,
        IsActivityLevel: false,
        isACGroupLevel: false,
    };
    $scope.LevelAssaign = function (level) {
        $scope.incomeStatementReport.IsBudgetLevel = false;
        $scope.incomeStatementReport.IsActivityLevel = false;
        if (level == 'GL') {
            $scope.incomeStatementReport.IsBudgetLevel = false;
            $scope.incomeStatementReport.IsActivityLevel = false;

        }
        if (level == 'Budget') {
            $scope.incomeStatementReport.IsBudgetLevel = true;
            $scope.incomeStatementReport.IsActivityLevel = false;

        }
        if (level == 'Activity') {
            $scope.incomeStatementReport.IsBudgetLevel = false;
            $scope.incomeStatementReport.IsActivityLevel = true;

        }
    };
    $scope.upToLevelList = [];

    $scope.getLevelType = function () {
        $http({
            method: "GET",
            url: "Enum/GetIncomeStatementCbo/"
        }).then(function successCallback(response) {
            $scope.upToLevelList = response.data;
            $scope.incomeStatementReport.IsUpToLevel = response.data[0].Value;
            $scope.yearClosedIncomeStatementReport.IsUpToLevel = response.data[0].Value;
        });
    };
    $scope.getLevelType();
    
    $scope.currencyIds = [];
    $http({
        method: 'GET',
        url: 'currencies/companyparallelcurrency/cboparallelcurrency'
    }).then(function successCallback(response) {
        $scope.parallelCurrencyList = response.data;
        if ($scope.parallelCurrencyList.length === 1) {
            $scope.currencyIds.push($scope.parallelCurrencyList[0]);
        }
    });

    function listOfCurrencyId(ids) {
        var list = [];
        for (var i = 0; i < ids.length; i++) {
            list.push(ids[i].Value);
        }
        return list;
    }

    $scope.getReport = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            if ($scope.currencyIds.length === 0) return ShowResult('Currency required', 'failure');
            location.href = 'accounts/voucher/incomestatementreport?date=' + $scope.incomeStatementReport.Date + '&parallelCurrency=' + JSON.stringify(listOfCurrencyId($scope.currencyIds)) + '&isBudgetLevel=' + $scope.incomeStatementReport.IsBudgetLevel + '&isActivityLevel=' + $scope.incomeStatementReport.IsActivityLevel;
        }
    };

    $scope.incomeStatementReportDownLoad = function () {
        if ($scope.incomeStatementReport.Type === "ForThePeriod") {
            $scope.getDateWiseTrialBalanceReport();
        }
        else {
            $scope.getReport();

        }
    };

    $scope.getDateWiseTrialBalanceReport = function () {
        if (baseService.isUndefinedOrNull($scope.incomeStatementReport.FromDate)) {
            manualValidation('div_WDFromDate', true, "From Date is required.");
        }
        if (baseService.isUndefinedOrNull($scope.incomeStatementReport.ToDate)) {
            manualValidation('div_WDToDate', true, "To Date is required.");
        }
        else {
            location.href = 'accounts/voucher/incomestatementreportDateWise?fromDate=' + $scope.incomeStatementReport.FromDate + '&toDate=' + $scope.incomeStatementReport.ToDate + '&parallelCurrency=' + JSON.stringify(listOfCurrencyId($scope.currencyIds)) + '&isBudgetLevel=' + $scope.incomeStatementReport.IsBudgetLevel + '&isActivityLevel=' + $scope.incomeStatementReport.IsActivityLevel;

        }
    };
    $scope.yearClosedByDateList = [];
    $scope.checkYearClosedByDate = function (date) {
        $scope.yearClosedByDateList = [];
        $http({
            method: "GET",
            url: "accounts/FiscalYearClose/CheckYearClosedByDate?date=" + date
        }).then(function successCallback(response) {
            $scope.yearClosedByDateList = response.data;
            if ($scope.yearClosedByDateList.length>0) {
                $scope.incomeStatementReport.FromDate = $filter('dateFiltering')(Date.now());
                $scope.incomeStatementReport.ToDate = $filter('dateFiltering')(Date.now());
                $scope.incomeStatementReport.Date = $filter('dateFiltering')(Date.now());
                ShowResult('Fiscal Year already closed!!!', 'failure');
            }
        });
    };

    //Year Closed Income Statement
    $scope.yearClosedIncomeStatementReport = {
        FiscalYearCloseId: null,
        FiscalYearName: null,
        IsUpToLevel: null,
        IsBudgetLevel: false,
        IsActivityLevel: false,
        isACGroupLevel: false,
    };
    $scope.yearClosedLevelAssaign = function (level) {
        $scope.yearClosedIncomeStatementReport.IsBudgetLevel = false;
        $scope.yearClosedIncomeStatementReport.IsActivityLevel = false;
        if (level == 'GL') {
            $scope.yearClosedIncomeStatementReport.IsBudgetLevel = false;
            $scope.yearClosedIncomeStatementReport.IsActivityLevel = false;

        }
        if (level == 'Budget') {
            $scope.yearClosedIncomeStatementReport.IsBudgetLevel = true;
            $scope.yearClosedIncomeStatementReport.IsActivityLevel = false;

        }
        if (level == 'Activity') {
            $scope.yearClosedIncomeStatementReport.IsBudgetLevel = false;
            $scope.yearClosedIncomeStatementReport.IsActivityLevel = true;

        }
    };
    $scope.masterList = [];
    $scope.getMasterData = function () {
        $scope.masterList = [];
        $http.get("accounts/FiscalYearClose/GetFiscalYearClosedListForReporting")
            .then(
                function successCallback(response) {
                    $scope.masterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#FiscalYearClosepopUp')).modal('show');
    };
    $scope.getFiscalYearClosedData = function () {
        $scope.masterList = [];
        $http.get("accounts/FiscalYearClose/GetFiscalYearClosedListForReporting")
            .then(
                function successCallback(response) {
                    $scope.masterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getFiscalYearClosedData();

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#FiscalYearClosepopUp')).modal('hide');
    }

    $scope.SelectMaster = function (x) {
        var data = x.data;
        $scope.yearClosedIncomeStatementReport.FiscalYearCloseId = data.Id;
        $scope.yearClosedIncomeStatementReport.FiscalYearName = data.FiscalYearName;
        angular.element(document.querySelector('#FiscalYearClosepopUp')).modal('hide');
    };
    $scope.getYearClosedReport = function () {
        if (baseService.isUndefinedOrNull($scope.yearClosedIncomeStatementReport.FiscalYearName)) {
            manualValidation('div_FiscalYearName', true, "Fiscal Year is required.");
        }
        else {
            location.href = 'accounts/voucher/IncomeStatementYearClosedReport?fiscalYearCloseId=' + $scope.yearClosedIncomeStatementReport.FiscalYearCloseId + '&fiscalYearName=' + $scope.yearClosedIncomeStatementReport.FiscalYearName + '&isBudgetLevel=' + $scope.yearClosedIncomeStatementReport.IsBudgetLevel + '&isActivityLevel=' + $scope.yearClosedIncomeStatementReport.IsActivityLevel;
        }
    };
    //Year Closed Income Statement
}