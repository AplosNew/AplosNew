'use strict';
NewEarnLeaveReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function NewEarnLeaveReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'New Earn Leave Report';
    $scope.path = 'Leave/NewEarnLeaveReport/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    //#region From-Date To-Date
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.FromDate = $filter('dateFiltering')(firstDay);
    $scope.ToDate = $filter('dateFiltering')(new Date(), 'dd-MM-yyyy');
    //#endregion

    //#region Report Part
    $scope.ReportForEarnLeave = function () {
        try {

            $http({
                method: 'POST',
                url: 'Leave/NewEarnLeaveReport/NewEarnReport',
                data: {
                    'FromDate': $scope.FromDate,
                    'ToDate': $scope.ToDate
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