'use strict';
epanelLogoutController.$inject = ['$scope', '$location', '$http', '$window','signalR'];
function epanelLogoutController($scope, $location, $http, $window, signalR) {
    $scope.Logout = function () {
        $http({
            method: 'GET',
            url: 'MyApp/logout'
        }).then(function (result) {
            var path = $location.protocol() + '://' + $location.host() + ':' + $location.port() + result.data.BasePath + '/myapp';
            $window.location.href = path;
            signalR.DisconnectUser();
        });
    };
    $scope.Logout();
}