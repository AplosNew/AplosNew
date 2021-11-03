'use strict';
IncomeStatementReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter','$window'];
function IncomeStatementReportController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $rootScope.title = 'Income Statement';
    $scope.parallelCurrencyList = [];
     
    $scope.incomeStatementReport = {
        Date: $filter('dateFiltering')(Date.now()),
       // CutOffDate: $filter('dateFiltering')(Date.now()), //checkCutOffDate
      //  ParallelCurrencyId: null,
        Type: 'AsOnDate',
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now())
    };

    //$http({
    //    method: 'GET',
    //    url: 'accounts/OpeningBalance/GetACCCutOffDate'
    //}).then(function successCallback(response) {
    //    if (response.data !== null) {
    //        $scope.incomeStatementReport.CutOffDate = response.data.CutOffDate;
    //        $scope.incomeStatementReport.CutOffDate = $filter('dateFiltering')($scope.incomeStatementReport.CutOffDate);
    //    }
    //    else {
    //        ShowResult('Opening Balance Cut Off date not found!', 'failure');
    //    }
    //});

    //$scope.dateMessageFrom = '';
    //$scope.checkCutOffDate = function () {
    //    if (new Date($scope.incomeStatementReport.Date) < new Date($scope.incomeStatementReport.CutOffDate)) {
    //        $scope.dateMessageFrom = 'Date must be above or equal to Cut Off Date!';
    //        return false;
    //    } else {
    //        $scope.dateMessageFrom = '';
    //        return true;
    //    }
    //};

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

    //$scope.multiSelectSettings = {
    //    scrollableHeight: 'auto',
    //    smartButtonMaxItems: 3,
    //    scrollable: true,
    //    showCheckAll: false,
    //    showUncheckAll: false,
    //    enableSearch: false,
    //    dynamicTitle: true
    //};

    //$scope.CurrencyParallel = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'currencies/CompanyParallelCurrency/CurrencyParallel'
    //    }).then(function successCallback(response) {
    //        $scope.CurrencyParallel = response.data.Rows;
    //    });
    //    $scope.CheckParallelCurrencyValid();
    //};

    function listOfCurrencyId(ids) {
        var list = [];
        for (var i = 0; i < ids.length; i++) {
            list.push(ids[i].Value);
        }
        return list;
    }

    //$scope.typeChange = function (data) {

    //}

    $scope.getReport = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            if ($scope.currencyIds.length === 0) return ShowResult('Currency required', 'failure');
            location.href = 'accounts/voucher/incomestatementreport?date=' + $scope.incomeStatementReport.Date + '&parallelCurrency=' + JSON.stringify(listOfCurrencyId($scope.currencyIds));
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
            location.href = 'accounts/voucher/incomestatementreportDateWise?fromDate=' + $scope.incomeStatementReport.FromDate + '&toDate=' + $scope.incomeStatementReport.ToDate   + '&parallelCurrency=' + JSON.stringify(listOfCurrencyId($scope.currencyIds));

            //var url = 'Accounts/Voucher/DateRangeWiseTrialBalanceReport?reportFormat=' + $scope.report.ReportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&isBudgetLevel=' + $scope.report.IsBudgetLevel + '&isActivityLevel=' + $scope.report.IsActivityLevel;
            //$window.open(url, '_blank');
        }
    };

    //New Format for income statement
   // $rootScope.title = 'Trial Balance';
    //var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    //var firstDay = new Date(y, m, 1);

  
        //$scope.$broadcast('show-errors-check-validity');
        //if ($scope.form.$valid) {
        //    if ($scope.currencyIds.length === 0) return ShowResult('Currency required', 'failure');
        //    location.href = 'accounts/voucher/incomestatementreport?date=' + $scope.incomeStatementReport.Date + '&parallelCurrency=' + JSON.stringify(listOfCurrencyId($scope.currencyIds));
        //}


    //$scope.getReport = function () {
    //    if (baseService.isUndefinedOrNull($scope.report.GLGeneralInfoId)) {
    //        manualValidation("div_GL", true, "GL is required.");
    //    }
    //    else if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
    //        manualValidation("div_FromDate", true, "From Date is required.");
    //    }
    //    else if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
    //        manualValidation("div_ToDate", true, "To Date is required.");
    //    }
    //    else if (new Date($scope.report.FromDate) > new Date($scope.report.ToDate)) {
    //        manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
    //    }
    //    else if (new Date($scope.report.ToDate) < new Date($scope.report.FromDate)) {
    //        manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
    //    }
    //    else {
    //        var url = "Accounts/Voucher/GetGeneralLedgerReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&glId=" + $scope.report.GLGeneralInfoId;
    //        if (!baseService.isUndefinedOrNull($scope.report.BudgetMasterId)) {
    //            url += "&budgetMasterId=" + $scope.report.BudgetMasterId;
    //        }
    //        if (!baseService.isUndefinedOrNull($scope.report.ActivityId)) {
    //            url += "&activityId=" + $scope.report.ActivityId;
    //        }
    //        $window.open(url, "_blank");
    //    }
    //};

}