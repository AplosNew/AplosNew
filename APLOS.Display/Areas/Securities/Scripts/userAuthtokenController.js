'use strict';
function UserAuthtokenController(commonMessage, $scope, $routeParams, $location, $http, $filter, $window) {
    $scope.Action = 'New';
    Get($routeParams.id);
    function Get(id) {
        $http.get('Securities/user/get?id=' + id)
            .then(function (response) {
                $scope.user = response.data;
                $scope.user.AuthToken = null;
            });
    }

    $scope.user = {
        Id: null,
        CompanyGroupId: null,
        UserId: null,
        FullName: null,
        Password: null,
        ConfirmPassword: null,
        LastPwdChangedDay: null,
        DateOfBirth: null,
        Phone: null,
        Email: null,
        EmailVerified: null,
        EmailVerifiedDate: null,
        EmailVerificationCode: null,
        PasswordFailCount: 0,
        UserLocked: false,
        UserLockedDate: null,
        UserUnlockDate: null,
        AuthToken: null,
        AuthTokenFailCount: 0,
        AuthTokenLocked: false,
        AuthTokenLockedDate: null,
        AuthTokenUnlockDate: null,
        SysAdmin: true,
        PowerUser: false,
        PwdChangeOnFirstLogin: true,
        PasswordNeverExpired: true,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter('date')(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        UpdatedDate: $filter('date')(Date.now(), 'yyyy-MM-dd')
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.userAuthForm.$valid) {
            $scope.user.DateOfBirth = $filter('dateFilter')($scope.user.DateOfBirth);
            $scope.user.EmailVerifiedDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
            $scope.user.UserLockedDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
            $scope.user.UserUnlockDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
            $scope.user.AuthTokenLockedDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
            $scope.user.AuthTokenUnlockDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
            $scope.user.LastPwdChangedDay = $filter('dateFilter')($scope.user.LastPwdChangedDay);

            $http({
                method: 'POST',
                url: 'Securities/user/authtokenchange',
                data: $scope.user,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    setTimeout(function () {
                        $window.history.back();
                    }, 3000);
                }
            });
            return true;
        }
    }

    $scope.Back = function () {
        $window.history.back();
    }
}
UserAuthtokenController.$inject = ['commonMessage', '$scope', '$routeParams', '$location', '$http', '$filter', '$window'];