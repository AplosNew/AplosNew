'use strict';
UserLockLogController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function UserLockLogController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "User Lock Log";
    $scope.userLockLogs = [];
    $scope.path = 'Securities/userlocklog/';
    $scope.getListUrl = $scope.path + 'userlockdatedetailswithoutsyadmin?id=' + $routeParams.id;
    $scope.getUrl = $scope.path + 'get';
    baseService.init($scope.getListUrl, null, null, null, 'UserId', 'UserId');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.userLockLogs = result;
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