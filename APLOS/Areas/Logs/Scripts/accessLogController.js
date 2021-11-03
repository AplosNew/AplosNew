'use strict';
AccessLogController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function AccessLogController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Access Logs';
    $scope.accesslog = {
        fromDate: $filter("dateFiltering")(Date.now()),
        toDate: $filter("dateFiltering")(Date.now())
    };

    $scope.accesslogs = [];
    $scope.path = 'Logs/accesslog/';
    $scope.getListUrl = $scope.path + 'get';
    baseService.init($scope.getListUrl, null, 10);
    $scope.getData = function (pageno) {
        $rootScope.parameters.fromDate = $scope.accesslog.fromDate;
        $rootScope.parameters.toDate = $scope.accesslog.toDate;
        baseService.pagination(pageno)
                   .then(function (result) {
                       $scope.accesslogs = result.Rows;
                   }, function () {
                       ShowResult(commonMessage.NetworkError, 'failure');
                   }).finally(function () {
                   });
    };

    $scope.GetByDate = function () {
        $http.get('Logs/accesslog/get?fromdate=' + $scope.accesslog.fromDate + '&todate=' + $scope.accesslog.toDate)
          .then(function (response) {
              $scope.accesslogs = response.data;
          });
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'Logs/accesslog/delete?fromdate=' + $scope.accesslog.fromDate + '&toDate=' + $scope.accesslog.toDate,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.accesslogs = [];
                $rootScope.total_count = 0;
            }
        });
        return true;
    };
}