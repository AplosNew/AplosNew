'use strict';
FNFReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function FNFReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Full&FinalReport';
   
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.FromDate = null;
    $scope.ToDate = null;

    $scope.GetReport = function () {
        try {
            $scope.fileName = "Full&FinalReport.xlsx";
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "Select From Date";
            }
            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "Select To Date";
            }
            $http({
                method: 'POST',
                url: "Payrolls/FinalSettlement/GetFNFReport",
                data: { 'reportFileName': $scope.fileName, 'fromDate': $scope.FromDate, 'toDate': $scope.ToDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

}