'use strict';
cpanelLogoutController.$inject = ['$scope', '$rootScope', '$location', '$http', '$filter', '$window', '$cookies'];
function cpanelLogoutController($scope, $rootScope, $location, $http, $filter, $window, $cookies) {
    $scope.Logout = function () {
        $http({
            method: 'GET',
            url: 'cpanel/logout'
        }).then(function (result) {
            var path = $location.protocol() + '://' + $location.host() + ':' + $location.port() + result.data.BasePath + '/cpanel';
            $window.location.href = path;
        });
    };
    $scope.Logout();
}