'use strict';
function SystemAdminAuthtokenController(commonMessage, $scope, $routeParams, $location, $http, $filter, $window) {
    Get($routeParams.id);
    function Get(id) {
        $http.get('Securities/systemadmin/get?id=' + id)
            .then(function (response) {
                $scope.system = response.data;
                $scope.system.AuthToken = null;
            });
    }

    $scope.system = {
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
        if ($scope.systemAdminAuthForm.$valid) {
            $scope.system.DateOfBirth = $filter('dateFilter')($scope.system.DateOfBirth);
            $scope.system.EmailVerifiedDate = $filter('dateFilter')($scope.system.LastPwdChangedDay);
            $scope.system.UserLockedDate = $filter('dateFilter')($scope.system.LastPwdChangedDay);
            $scope.system.UserUnlockDate = $filter('dateFilter')($scope.system.LastPwdChangedDay);
            $scope.system.AuthTokenLockedDate = $filter('dateFilter')($scope.system.LastPwdChangedDay);
            $scope.system.AuthTokenUnlockDate = $filter('dateFilter')($scope.system.LastPwdChangedDay);
            $scope.system.LastPwdChangedDay = $filter('dateFilter')($scope.system.LastPwdChangedDay);

            $http({
                method: 'POST',
                url: 'Securities/systemadmin/authtokenchange',
                data: $scope.system,
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
    }

    $scope.Back = function () {
        $window.history.back();
    }
}
SystemAdminAuthtokenController.$inject = ['commonMessage', '$scope', '$routeParams', '$location', '$http', '$filter', '$window'];