'use strict';
AuditReportSummeryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function AuditReportSummeryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = 'Audit Report Summary';
    //$scope.index = -1;

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.path = 'Attendances/AuditReportSummery/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.effectiveDate = $filter('dateFiltering')(firstDay);


    var yesterday = new Date();

    var datee = yesterday.setDate(yesterday.getDate() - 1);

    $scope.ToDate = $filter('dateFiltering')(new Date(datee), 'dd-MM-yyyy');


    $scope.Report = function () {
        try {
            $scope.fileName = "Audit Report Summary " + $scope.effectiveDate + " to  .xls";

            $http({
                method: 'POST',
                url: 'Attendances/AuditReportSummery/AuditReportSummery',
                data: {
                    'workDate': $scope.effectiveDate, 'ToDate': $scope.ToDate
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