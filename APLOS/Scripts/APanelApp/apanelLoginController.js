'use strict';
apanelLoginController.$inject = ['$scope', '$rootScope', '$routeParams', '$http', '$filter', '$window', '$cookies', '$location'];
function apanelLoginController($scope, $rootScope, $routeParams, $http, $filter, $window, $cookies, $location) {
    $rootScope.title = 'Administration::Login';
    $scope.authenticationToken = angular.element(document.querySelector('#authToken')).val();
    $scope.companyGroupId = angular.element(document.querySelector('#groupId')).val();
    $scope.companyGroupName = null;
    $scope.servicepanel = 'aPanel';
    $scope.returnUrl = $routeParams.returnUrl;
    $scope.timezoneoffset = new Date().getTimezoneOffset();
    $scope.userId = null;
    $scope.password = null;
    $scope.imageSrc = null;
    $scope.remember = null;
    // Default/alt company group logo.
    $scope.companyGroupLogo = 'images/group-alt.png';
    $scope.errorText = null;
    $http.get('Organizations/companygroup/getnameandlogo/' + $scope.companyGroupId)
        .then(function (response) {
            if (response.data[0] !== null) {
                $scope.companyGroupName = response.data[0];
            }
            else {
                $scope.companyGroupName = 'Company group name not found!';
            }
            if (response.data[1] !== null) {
                $scope.companyGroupLogo = virtualPath.LogoOrImage + response.data[1];
                $cookies.put('gImage', response.data[1]);
            }
        });

    $scope.Login = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.apanelLoginForm.$valid) {
            $scope.errorText = null;
            if (!navigator.onLine)
                return $scope.errorText = 'No internet connection.Please check your internet connection.';
            $http({
                method: 'POST',
                url: 'APanel/login',
                params: {
                    'timezoneoffset': $scope.timezoneoffset
                    , 'authToken': $scope.authenticationToken
                    , 'groupId': $scope.companyGroupId
                    , 'groupName': $scope.companyGroupName
                    , 'userId': $scope.userId
                    , 'password': $scope.password
                    //, 'password': encodeURIComponent($scope.password)
                    , 'remember': $scope.remember
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true || response.data.Status === 'Fail') {
                    $scope.errorText = response.data.ErrorText || response.data.Message;
                }
                else {
                    if (response.data.Status === 'Success') {
                        $cookies.put('panel', 'apanel');
                        $cookies.put('authToken', $scope.authenticationToken);
                        $cookies.put('groupId', $scope.companyGroupId);
                        $cookies.put('employeeId', response.data.EmployeeId);
                        if (response.data.Image === null) {
                            $cookies.put('userImage', 'altUser.jpg');
                        } else
                            $cookies.put('userImage', response.data.Image);
                        $window.location = 'administrationpanel';
                    }
                }
            }, function errorCallback(response) {
                //$scope.errorHide = false;
                $scope.errorText = response.statusText;
            });
        }
    };

    $scope.clearMsg = function () {
        //$scope.errorHide = true;
        $scope.errorText = null;
    };
}