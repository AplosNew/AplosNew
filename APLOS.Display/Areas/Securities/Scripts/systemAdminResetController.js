'use strict';
function SystemAdminResetController(commonMessage, $scope, $routeParams, $location, $http, $filter, $window) {
    $scope.Action = 'New';
    Get($routeParams.id);
    function Get(id) {
        $http.get("Securities/SystemAdmin/Get?id=" + id)
            .then(function (response) {
                $scope.system = response.data;
                $scope.system.Password = null;
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
        AddedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        UpdatedDate: $filter("date")(Date.now(), 'yyyy-MM-dd')
    };
    $scope.compare = function (f1, f2) {
        $scope.result = angular.equals(f1, f2);
        if (!$scope.result)
            $scope.compareTo = true;
        else
            $scope.compareTo = false;
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.systemAdminResetForm.$valid) {
            if (!angular.equals($scope.system.Password, $scope.system.ConfirmPassword))
                return ShowResult('Confirm password does not match.', 'failure');
            $scope.system.DateOfBirth = $filter('dateFilter')($scope.system.DateOfBirth);
            $scope.system.EmailVerifiedDate = $filter('dateFilter')($scope.system.LastPwdChangedDay);
            $scope.system.UserLockedDate = $filter('dateFilter')($scope.system.LastPwdChangedDay);
            $scope.system.UserUnlockDate = $filter('dateFilter')($scope.system.LastPwdChangedDay);
            $scope.system.AuthTokenLockedDate = $filter('dateFilter')($scope.system.LastPwdChangedDay);
            $scope.system.AuthTokenUnlockDate = $filter('dateFilter')($scope.system.LastPwdChangedDay);
            $scope.system.LastPwdChangedDay = $filter('dateFilter')($scope.system.LastPwdChangedDay);
            $http({
                method: 'POST',
                url: "Securities/SystemAdmin/Reset",
                data: $scope.system,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                }
            });
        }
        else
            $scope.compareTo = false;
    }

    $scope.Back = function () {
        $window.history.back();
    }
}
SystemAdminResetController.$inject = ['commonMessage', "$scope", "$routeParams", "$location", "$http", "$filter", "$window"];