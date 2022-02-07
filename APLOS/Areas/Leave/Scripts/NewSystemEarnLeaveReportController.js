'use strict';
NewSystemEarnLeaveReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function NewSystemEarnLeaveReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'New Earn Leave Report';
    $scope.path = 'Leave/NewEarnLeaveReport/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    //#region From-Date To-Date
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.NewFromDate = $filter('dateFiltering')(firstDay);
    $scope.NewToDate = $filter('dateFiltering')(new Date(), 'dd-MM-yyyy');
    //#endregion

    //#region Report Part
    $scope.ReportForEarnLeaveNew = function () {
        try {

            $http({
                method: 'POST',
                url: 'Leave/NewEarnLeaveReport/NewSystemEarnReport',
                data: {
                    'FromDate': $scope.NewFromDate,
                    'ToDate': $scope.NewToDate
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.File + "&fileName=" + response.data.ReportName);//downloadgriddataUrlPath
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    //#endregion

}