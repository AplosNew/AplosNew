'use strict';
entityWiseExpenseAndEarningController.$inject = ['cboService','commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter','$window'];
function entityWiseExpenseAndEarningController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $rootScope.title = 'Entity Wise Expense And Earning';
    $scope.parallelCurrencyList = [];
     
    $scope.incomeStatementReport = {
        Date: $filter('dateFiltering')(Date.now()),
       // CutOffDate: $filter('dateFiltering')(Date.now()), 
        Type: 'Statement',
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),

        EntityId: null
       // VoucherId: null
    };


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

    //$scope.typeChange = function (data) {

    //}



    $scope.getReport = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            if ($scope.currencyIds.length === 0) return ShowResult('Currency required', 'failure');
            location.href = 'accounts/voucher/incomestatementreport?date=' + $scope.incomeStatementReport.Date + '&parallelCurrency=' + JSON.stringify(listOfCurrencyId($scope.currencyIds));
        }
    };


    $scope.entityChange = function (id) {
        var entity = $.grep($scope.entityList, function (item) {
            return item.Value === id;
        })[0];

        $scope.Entiy = entity.Text;
    }



    $scope.incomeStatementReportDownLoad = function () {
        $scope.getDateWiseTrialBalanceReport();

        //if ($scope.incomeStatementReport.Type === "ForThePeriod") {
        //    $scope.getDateWiseTrialBalanceReport();
        //}
        //else {
        //    $scope.getReport();

        //}
    };

    $scope.getDateWiseTrialBalanceReport = function () {
        if (baseService.isUndefinedOrNull($scope.incomeStatementReport.FromDate)) {
            manualValidation('div_WDFromDate', true, "From Date is required.");
        }
        if (baseService.isUndefinedOrNull($scope.incomeStatementReport.ToDate)) {
            manualValidation('div_WDToDate', true, "To Date is required.");
        }
        else {
            if ($scope.incomeStatementReport.Type === "Statement") {
                location.href = 'accounts/voucher/EntityWiseExpenseAndEarningreportDateWise?fromDate=' + $scope.incomeStatementReport.FromDate + '&toDate=' + $scope.incomeStatementReport.ToDate + '&entityId=' + $scope.incomeStatementReport.EntityId + '&entity=' + $scope.Entiy + '&parallelCurrency=' + JSON.stringify(listOfCurrencyId($scope.currencyIds));
            }
            else {
                location.href = 'accounts/voucher/EntityWiseExpenseAndEarningreportDateWiseActivityLevel?fromDate=' + $scope.incomeStatementReport.FromDate + '&toDate=' + $scope.incomeStatementReport.ToDate + '&entityId=' + $scope.incomeStatementReport.EntityId + '&entity=' + $scope.Entiy + '&parallelCurrency=' + JSON.stringify(listOfCurrencyId($scope.currencyIds));
            }
            

            //var url = 'Accounts/Voucher/DateRangeWiseTrialBalanceReport?reportFormat=' + $scope.report.ReportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&isBudgetLevel=' + $scope.report.IsBudgetLevel + '&isActivityLevel=' + $scope.report.IsActivityLevel;
            //$window.open(url, '_blank');
        }
    };

    $scope.entityList = [];
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });

   
}