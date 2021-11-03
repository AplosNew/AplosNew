'use strict';
cpanelLoginController.$inject = ['$scope', '$rootScope', '$routeParams', '$http', '$filter', '$window', '$cookies'];
function cpanelLoginController($scope, $rootScope, $routeParams, $http, $filter, $window, $cookies) {
    $rootScope.title = 'Control Panel::Login';
    $scope.servicepanel = 'cPanel';
    $scope.returnUrl = $routeParams.returnUrl;
    $scope.timezoneoffset = new Date().getTimezoneOffset();
    $scope.username = null;
    $scope.userId = null;
    $scope.remember = null;
    $scope.errorText = null;
    $scope.companyGroupLogo = 'organization-alt.png';
    $scope.Login = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.loginForm.$valid) {
            $scope.errorText = null;
            if (!navigator.onLine)
                return $scope.errorText = 'No internet connection. Please check your internet connection.';
            $http({
                method: 'POST',
                url: 'cpanel/login',
                params: {
                    'timezoneoffset': $scope.timezoneoffset,
                    'userId': $scope.userId,
                    'password': $scope.password,
                    'remember': $scope.remember
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true || response.data.Status === 'Fail') {
                    $scope.errorText = response.data.ErrorText || response.data.Message;
                }
                else {
                    if (response.data.Status === 'Success') {
                        $cookies.put('panel', 'cpanel');
                        $window.location = 'controlpanel';
                    }
                }
            }, function errorCallback(response) {
                $scope.errorText = response.statusText;
            });
        }
    };

    $scope.clearMsg = function () {
        $scope.errorText = null;
    };
}