'use strict';
ProductionOrderRateReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function ProductionOrderRateReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'ProductionOrder Rate Report';

    $scope.ModelList = [];
    $scope.path = 'Productions/ProductionOrderRateReport/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.FromDate = null;
    $scope.ToDate = null;

    $scope.DonwloadReport = function () {
        try {
            $scope.fileName = "ProductionOrderRateReport.xls";

            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "Select From Date";
            }

            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "Select To Date";
            }

            $http({
                method: 'POST',
                url: 'Productions/ProductionOrderRateReport/RReport',
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