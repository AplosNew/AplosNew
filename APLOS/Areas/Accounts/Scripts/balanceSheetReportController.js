'use strict';
balanceSheetReportController.$inject = ['$scope', '$rootScope', '$filter', "baseService", "$http", "$window"];
function balanceSheetReportController($scope, $rootScope, $filter, baseService, $http, $window) {
    $rootScope.title = 'Balance Sheet';
    $scope.report = {
        IsUpToLevel:null,
        IsBudgetLevel: false,
        IsActivityLevel: false,
        isACGroupLevel: false,
        ReportFormat: 'Pdf',
        FromDate: $filter('dateFiltering')(Date.now())
    };

    //$scope.uncheck = function () {
    //    if ($scope.report.IsBudgetLevel === false) {
    //        $scope.report.IsBudgetLevel = false;
    //        $scope.report.IsActivityLevel = false;
    //    }
    //    if ($scope.report.IsActivityLevel === false) {
    //        $scope.report.isACGroupLevel = false;
    //    }
    //    else {
    //        $scope.report.IsActivityLevel = false;
    //    }
    //};

    $scope.upToLevelList = [];

    $scope.getLevelType = function () {
        $http({
            method: "GET",
            url: "Enum/GetBalanceSheetLevelCbo/"
        }).then(function successCallback(response) {
            $scope.upToLevelList = response.data;
            $scope.report.IsUpToLevel = response.data[0].Value;
        });
    };
    $scope.getLevelType();
    $scope.LevelAssaign = function (level) {
        if (level == 'GL') {
            $scope.report.IsBudgetLevel = false;
            $scope.report.IsActivityLevel = false;
            $scope.report.isACGroupLevel = false;
        }
        if (level == 'Budget') {
            $scope.report.IsBudgetLevel = true;
            $scope.report.IsActivityLevel = false;
            $scope.report.isACGroupLevel = false;
        }
        else if (level == 'Activity') {
            $scope.report.IsBudgetLevel = true;
            $scope.report.IsActivityLevel = true;
            $scope.report.isACGroupLevel = false;
        }
        else if (level == 'AccountGroup') {
            $scope.report.isACGroupLevel = true;
            $scope.report.IsActivityLevel = true;
            $scope.report.IsBudgetLevel = true;
        }
    };

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation('div_FromDate', true, "Date is required.");
        }
        else {
            var url = 'Accounts/Voucher/BalanceSheetReport?reportFormat=' + $scope.report.ReportFormat + '&date=' + $scope.report.FromDate + '&isBudgetLevel=' + $scope.report.IsBudgetLevel + '&isActivityLevel=' + $scope.report.IsActivityLevel + '&isACGroupLevel=' + $scope.report.isACGroupLevel;
            $window.open(url, '_blank');
        }
    };
}