'use strict';
UserPasswordChangeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function UserPasswordChangeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    Get($routeParams.id);
    function Get(id) {
        $http.get('Securities/user/getforpasswordchange?id=' + id)
            .then(function (response) {
                $scope.user = response.data;
                $scope.user.PasswordCheck = $scope.user.Password;
                $scope.user.Password = null;
            });
    }

    $scope.user = {
        Id: null,
        CompanyGroupId: null,
        UserId: null,
        FullName: null,
        OldPassword: null,
        PasswordCheck: null,
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
        AddedDate: null,
        AddedFromIP: null,
        UpdatedDate: $filter('date')(Date.now(), 'yyyy-MM-dd')
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.userPasswordChangeForm.$valid) {
            if ($scope.user.OldPassword === $scope.user.PasswordCheck) {
                $scope.user.DateOfBirth = $filter('dateFilter')($scope.user.DateOfBirth);
                $scope.user.EmailVerifiedDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
                $scope.user.UserLockedDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
                $scope.user.UserUnlockDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
                $scope.user.AuthTokenLockedDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
                $scope.user.AuthTokenUnlockDate = $filter('dateFilter')($scope.user.LastPwdChangedDay);
                $scope.user.LastPwdChangedDay = $filter('dateFilter')($scope.user.LastPwdChangedDay);
                $scope.user.AddedDate = $filter('dateFilter')($scope.user.AddedDate);
                $scope.user.UpdatedDate = $filter('dateFilter')($scope.user.UpdatedDate);

                $http({
                    method: 'POST',
                    url: 'Securities/user/passwordchange',
                    data: $scope.user,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                });
            }
            else
                ShowResult('Invalid old password', 'failure');
            return true;
        }
    }

    $scope.Back = function () {
        $window.history.back();
    }
}