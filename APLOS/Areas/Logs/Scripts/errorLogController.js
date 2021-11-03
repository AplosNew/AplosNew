'use strict';
ErrorLogController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ErrorLogController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Error Logs';
    $scope.errorLog = {
        fromDate: $filter("dateFiltering")(Date.now()),
        toDate: $filter("dateFiltering")(Date.now())
    };

    $scope.errorLogs = [];
    $scope.path = 'Logs/errorlog/';
    $scope.getListUrl = $scope.path + 'get';
    baseService.init($scope.getListUrl, null, 10);
    $scope.getData = function (pageno) {
        $rootScope.parameters.fromDate = $scope.errorLog.fromDate;
        $rootScope.parameters.toDate = $scope.errorLog.toDate;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.errorLogs = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: "Logs/errorlog/delete?fromDate=" + $scope.errorLog.fromDate + "&toDate=" + $scope.errorLog.toDate,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.errorLogs = [];
                $rootScope.total_count = 0;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message);
        });
        return true;
    }
}