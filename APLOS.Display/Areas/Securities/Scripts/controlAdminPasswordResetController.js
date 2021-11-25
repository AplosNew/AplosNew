'use strict';
function ControlAdminPasswordResetController(commonMessage, $scope, $rootScope, $routeParams, $location, $http, $filter, $window) {
    $scope.messaeShow = false;
    $scope.controlAdminReset = {
        UserId: null,
        FullName: null,
        Email: null,
        Password: null,
        ConfirmPassword: null,
        Archive: false
    };

    $scope.compare = function (p1, p2) {
        $scope.result = angular.equals(p1, p2);
        if (!$scope.result)
            $scope.compareTo = true;
        else
            $scope.compareTo = false;
    };

    $scope.Get = function () {
        $http.get('Securities/controladmin/get?id=' + $routeParams.userId)
            .then(function (response) {
                $scope.controlAdminReset = response.data;
                $scope.controlAdminReset.Password = null;
            });
    };

    $scope.Get();

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.controlAdminForm.$valid) {
            if (!angular.equals($scope.controlAdminReset.Password, $scope.controlAdminReset.ConfirmPassword))
                return ShowResult('Confirm password does not match.', 'failure');
            $scope.controlAdminReset.AddedDate = $filter('date')(Date.now(), 'yyyy-MM-dd');
            $scope.controlAdminReset.UpdatedDate = null;
            $http({
                method: 'POST',
                url: 'Securities/ControlAdmin/Reset',
                data: $scope.controlAdminReset,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            });
            return true;
        }
        else
            $scope.compareTo = false;
    }

    $scope.Back = function () {
        $window.history.back();
    }
}
ControlAdminPasswordResetController.$inject = ['commonMessage', '$scope', '$rootScope', '$routeParams', '$location', '$http', '$filter', '$window'];