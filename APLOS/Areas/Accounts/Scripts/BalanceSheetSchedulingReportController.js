'use strict';
BalanceSheetSchedulingReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function BalanceSheetSchedulingReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Balance Sheet Scheduling Report';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.FromDate = null;
    $scope.ToDate = null;

    $scope.ReportDownload = function () {
        try {
            $scope.fileName = "BalanceSheetSchedulingReport.xls";

            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "Select From Date";
            }

            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "Select To Date";
            }

            $http({
                method: 'POST',
                url: 'Accounts/BalanceSheetScheduling/GetReport',
                data: {
                    'FromDate': $scope.FromDate, 'ToDate': $scope.ToDate
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}