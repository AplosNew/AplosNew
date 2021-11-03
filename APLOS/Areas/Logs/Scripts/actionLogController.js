'use strict';
ActionLogController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ActionLogController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Action Logs';
    $scope.actionLog = {
        fromDate: $filter("dateFiltering")(Date.now()),
        toDate: $filter("dateFiltering")(Date.now())
    };

    $scope.actionLogs = [];
    $scope.path = 'Logs/ActionLog/';
    $scope.getListUrl = $scope.path + 'get';
    baseService.init($scope.getListUrl, null, 10);
    $scope.getData = function (pageno) {
        $rootScope.parameters.fromDate = $scope.actionLog.fromDate;
        $rootScope.parameters.toDate = $scope.actionLog.toDate;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.actionLogs = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: "Logs/ActionLog/Delete?fromDate=" + $scope.actionLog.fromDate + "&toDate=" + $scope.actionLog.toDate,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.actionLogs = [];
                $rootScope.total_count = 0;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message);
        });
        return true;
    };
}
