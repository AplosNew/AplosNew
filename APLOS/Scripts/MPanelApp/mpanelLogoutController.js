"use strict";
mpanelLogoutController.$inject = ["$scope", "$rootScope", "$location", "$http", "$filter", "$window", "$cookies"];
function mpanelLogoutController($scope, $rootScope, $location, $http, $filter, $window, $cookies) {
    $scope.Logout = function () {
        $http({
            method: "GET",
            url: "cpanel/logout"
        }).then(function (result) {
            $window.location.href = $location.protocol() + "://" + $location.host() + ":" + $location.port() + result.data.BasePath + "/mpanel"
                + "?authToken=" + $cookies.get("authToken")
                + "&groupId=" + $cookies.get("groupId");
        });
    };
    $scope.Logout();
}