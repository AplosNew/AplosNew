'use strict';
tpanelLogoutController.$inject = ['$scope', '$location', '$http', '$window','signalR'];
function tpanelLogoutController($scope, $location, $http, $window, signalR) {
    $scope.Logout = function () {
        $http({
            method: 'GET',
            url: 'DailyAttdStatus/logout'
        }).then(function (result) {
            var path = $location.protocol() + '://' + $location.host() + ':' + $location.port() + result.data.BasePath + '/DailyAttdStatus';
            $window.location.href = path;
            signalR.DisconnectUser();
        });
    };
    $scope.Logout();
}