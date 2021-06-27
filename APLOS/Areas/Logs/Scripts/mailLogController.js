'use strict';
mailLogController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function mailLogController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Mail Logs';
    $scope.mailLog = {
        fromDate: $filter("dateFiltering")(Date.now()),
        toDate: $filter("dateFiltering")(Date.now())
    };

    $scope.mailLogListParameters = {
        limit: 10,
        offset: 0,
        order: 'DESC',
        sort: 'RecordTime',
        searchBy: "",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.setClickedRow = function (index) {
        $scope.selectedRow = index;
    }

    $scope.getMailLogList = function (fromDate, toDate) {
        $scope.mailLogList = [];
        $scope.getMailLogListpagin = function (pageno) {
            baseService.paginationBase('Logs/MailLog/GetMailLogList?fromDate=' + fromDate + '&toDate=' + toDate, pageno, $scope.mailLogListParameters)
                .then(function (data) {
                    $scope.mailLogList = data.Rows;
                    $scope.mailLogListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getMailLogListpagin();
    }
    $scope.getMailLogList();

 
}