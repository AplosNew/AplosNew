'use strict';
daPasswordChangeController.$inject = ['$scope', 'baseService', '$routeParams', '$http', '$filter'];
function daPasswordChangeController($scope, baseService, $routeParams, $http, $filter) {
    Get($routeParams.id);

    $scope.user = {
        Id: null
        , Password: null
        , OldPassword: null
        , PasswordCheck: null
    };

    function Get(id) {
        $http.get('DailyAttendances/getforpasswordchange?id=' + id)
            .then(function (response) {
                $scope.user = response.data;
                $scope.user.PasswordCheck = response.data.PIN;
                $scope.user.Password = null;
            });
    }

    $scope.Save = function () {
        if (!Number.isInteger(Number($scope.user.Password))) return ShowResult('Invalid pin formate.');
        if ($scope.user.Password.toString().length !== 6) return ShowResult('Input 6 numbers pin');

        if ($scope.user.Password !== $scope.user.ConfirmPassword) return;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.userPasswordChangeForm.$valid) {
            if (parseInt($scope.user.OldPassword) === parseInt($scope.user.PasswordCheck)) {
                $http({
                    method: 'POST',
                    url: 'DailyAttendances/passwordchange',
                    data: {
                        id: $scope.user.Id
                        , password: $scope.user.Password
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) ShowResult(response.data.Message, 'failure');
                    else ShowResult(response.data.Message, 'success');
                });
            }
            else
                ShowResult('Invalid old password', 'failure');
        }
    };
}