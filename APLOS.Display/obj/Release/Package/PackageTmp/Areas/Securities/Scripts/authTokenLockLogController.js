'use strict';
AuthTokenLockLogController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function AuthTokenLockLogController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "AuthToken Lock Log";
    $scope.authLockLogs = [];
    $scope.path = 'Securities/authtokenlocklog/';
    $scope.getListUrl = $scope.path + 'authtokenlockdatedetailswithoutsyadmin?id=' + $routeParams.id;
    $scope.getUrl = $scope.path + 'get';
    baseService.init($scope.getListUrl, null, null, null, 'UserId', 'UserId');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.authLockLogs = result;
                $scope.UserId = result[0]['UserId'];
                $scope.FullName = result[0]['FullName'];
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $rootScope.searchByList = [
        {
            'name': 'Username',
            'value': 'UserId'
        }
    ];

    $scope.Back = function () {
        $window.history.back();
    }
}