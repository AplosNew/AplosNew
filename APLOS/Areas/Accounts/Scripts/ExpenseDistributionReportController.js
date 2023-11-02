"use strict";
ExpenseDistributionReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function ExpenseDistributionReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.ExpenseDistributionList = [];
    $scope.GetExpenseDistribution = function () {
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation("div_FromDate", true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
            manualValidation("div_ToDate", true, "To Date is required.");
        }
        else if (new Date($scope.report.FromDate) > new Date($scope.report.ToDate)) {
            manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.report.ToDate) < new Date($scope.report.FromDate)) {
            manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
        } else {

            $http({
                method: 'POST',
                url: 'Accounts/VoucherReport/GetExpenseDistribution',
                data: {
                    fromDate: $scope.report.FromDate,
                    toDate: $scope.report.ToDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.ExpenseDistributionList = response.data;
                $scope.getEDReport();
            });
        }
    };

    $scope.getEDReport = function () {
        $scope.fileName = 'Expense Distribution Report';

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'reportFileName': $scope.fileName, 'data': $scope.ExpenseDistributionList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                //$rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };
}