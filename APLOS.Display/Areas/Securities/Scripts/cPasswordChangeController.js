'use strict';
CPasswordChangeController.$inject = ['commonMessage', '$scope', '$rootScope', '$routeParams', '$location', '$http', '$window'];
function CPasswordChangeController(commonMessage, $scope, $rootScope, $routeParams, $location, $http, $window) {
    $rootScope.title = "Control Admin Password Change";
    Get($routeParams.id);
    $scope.show = true;
    $scope.clientAdmin = {
        UserId: null,
        FullName: null,
        OldPassword: null,
        Password: null,
        ConfirmPassword: null,
        Email: null,
        Archive: null,
        PasswordCheck: null
    };

    $scope.compare = function (f1, f2) {
        $scope.result = angular.equals(f1, f2);
        if (!$scope.result)
            $scope.compareTo = true;
        else
            $scope.compareTo = false;
    };
    function Get(id) {
        $http.get('Securities/controladmin/Get?id=' + id)
            .then(
            function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.show = false;
                }
                else {
                    $scope.clientAdmin = response.data;
                    $scope.clientAdmin.PasswordCheck = $scope.clientAdmin.Password;
                    $scope.clientAdmin.Password = null;
                    $scope.show = true;
                }
            });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.cpanalePwdChangeForm.$valid) {
            if ($scope.clientAdmin.OldPassword != $scope.clientAdmin.PasswordCheck) {
                return ShowResult('Old password does not match.', 'failure');
            }
            else if (angular.equals($scope.clientAdmin.Password, $scope.clientAdmin.OldPassword)) {
                return ShowResult('Old password and new password can not be same.', 'failure');
            }
            else if (!angular.equals($scope.clientAdmin.Password, $scope.clientAdmin.ConfirmPassword))
                return ShowResult('Confirm password does not match.', 'failure');
            $http({
                method: 'POST',
                url: 'Securities/controladmin/change',
                data: $scope.clientAdmin,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.clientAdmin.PasswordCheck = $scope.clientAdmin.Password;
                    ShowResult(response.data.Message, 'success');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else
            $scope.compareTo = false;
        return true;
    }

    $scope.Back = function () {
        $location.path("cpanel/dashboard");
    }
}