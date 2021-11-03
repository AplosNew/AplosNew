'use strict';
PasswordChangeFirstLoginController.$inject = ['$scope', '$rootScope', '$routeParams', '$http', '$filter'];
function PasswordChangeFirstLoginController($scope, $rootScope, $routeParams, $http, $filter) {
    $rootScope.title = 'Password change on first login';
    $scope.errorText = null;
    $scope.Id = $routeParams.Id;
    $scope.password = null;
    $scope.confirmPassword = null;
    $scope.changePassword = function () {
        $http({
            method: 'POST',
            url: 'account/passwordchange',
            data:
            {
                'id': $scope.Id,
                'password': $scope.password,
                'url': 'apnel'
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                $scope.errorText = response.data.Message;
            }
            else {
                $window.location = response.data.Url;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
}