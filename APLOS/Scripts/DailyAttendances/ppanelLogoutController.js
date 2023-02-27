'use strict';
ppanelLogoutController.$inject = ['$scope', '$location', '$http', '$window','signalR'];
function ppanelLogoutController($scope, $location, $http, $window, signalR) {
    $scope.Logout = function () {
        $http({
            method: 'GET',
            url: 'DailyAttendances/logout'
        }).then(function (result) {
            var path = $location.protocol() + '://' + $location.host() + ':' + $location.port() + result.data.BasePath + '/DailyAttendances';
            $window.location.href = path;
            signalR.DisconnectUser();
        });
    };
    $scope.Logout();
}