'use strict';
LCPendingReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function LCPendingReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'LC Pending Report';

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.GetReport = function (reportType) {
        try {

            $http({
                method: 'POST',
                url: 'Commercial/Contract/GetLCPendingReport',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}