'use strict';
balanceSheetReportGroupWiseController.$inject = ['$scope', '$rootScope', '$filter', "baseService", "$http", "$window"];
function balanceSheetReportGroupWiseController($scope, $rootScope, $filter, baseService, $http, $window) {
    $rootScope.title = 'Balance Sheet Group Wise';
    $scope.report = {
       // IsUpToLevel:null,
       // IsBudgetLevel: false,
      //  IsActivityLevel: false,
       // isACGroupLevel: false,
        ReportFormat: 'Excel',
        //FromDate: $filter('dateFiltering')(Date.now())

        Date: $filter('dateFiltering')(Date.now()),

        Type: 'AsOnDate',
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now())

    };


    $scope.balancesheetReportDownLoad = function () {
        if ($scope.report.Type === "ForThePeriod") {
            $scope.getDateWiseTrialBalanceReport();
        }
        else {
            $scope.getReport();

        }
    };

    $scope.getDateWiseTrialBalanceReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation('div_WDFromDate', true, "From Date is required.");
        }
        if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
            manualValidation('div_WDToDate', true, "To Date is required.");
        }
        else {
            //location.href = 'accounts/voucher/balanceSheetreportDateWise? fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate /*+ '&parallelCurrency=' + JSON.stringify(listOfCurrencyId($scope.currencyIds))*/;

            //var url = 'Accounts/Voucher/balanceSheetreportDateWise?reportFormat=' + $scope.report.ReportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate /*+ '&isBudgetLevel=' + $scope.report.IsBudgetLevel + '&isActivityLevel=' + $scope.report.IsActivityLevel*/;
            var url = 'Accounts/Voucher/balanceSheetreportForThePeriod?reportFormat=' + $scope.report.ReportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate /*+ '&isBudgetLevel=' + $scope.report.IsBudgetLevel + '&isActivityLevel=' + $scope.report.IsActivityLevel*/;
            $window.open(url, '_blank');
        }
    };

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation('div_FromDate', true, "Date is required.");
        }
        else {
            var url = 'Accounts/Voucher/BalanceSheetExtentReport?reportFormat=' + $scope.report.ReportFormat + '&date=' + $scope.report.Date /*+ '&isBudgetLevel=' + $scope.report.IsBudgetLevel + '&isActivityLevel=' + $scope.report.IsActivityLevel + '&isACGroupLevel=' + $scope.report.isACGroupLevel*/;
           // var url = 'Accounts/Voucher/BalanceSheetExtentReport?date=' + $scope.report.Date + '&date=' + $scope.report.Date;
            $window.open(url, '_blank');
        }
    };
}


