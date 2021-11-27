'use strict';
SystemAdminController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SystemAdminController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'System Admin';
    $scope.compareTo = false
    $scope.passwordShow = true;
    $scope.inactive = false;
    $scope.messaeShow = false;
    $scope.Action = 'Save';
    $scope.companyGroupList = [];
    $scope.index = -1;
    $scope.systemAdmins = [];
    $scope.path = 'Securities/systemadmin/';
    $scope.getListUrl = $scope.path + 'GetUserList';
    $scope.getUrl = $scope.path + 'get';
    $scope.getAuth = $scope.path + 'createauth';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'UserId', 'UserId');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.systemAdmins = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $rootScope.searchByList = [
        {
            'name': 'Username',
            'value': 'UserId'
        },
        {
            'name': 'Full Name',
            'value': 'FullName'
        },
        {
            'name': 'Company Group Id',
            'value': 'CompanyGroupId'
        },
        {
            'name': 'Company Group',
            'value': 'CompanyGroupName'
        },
        {
            'name': 'Auth Token',
            'value': 'AuthToken'
        },
        {
            'name': 'Id',
            'value': 'Id'
        }
    ];

    $scope.systemAdmin = {
        Id: null,
        CompanyGroupId: null,
        CompanyGroupName: null,
        UserId: null,
        FullName: null,
        Password: null,
        ConfirmPassword: null,
        LastPwdChangedDay: null,
        DateOfBirth: null,
        Phone: null,
        Email: null,
        EmailVerified: false,
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
        AddedFromIP: null
    };

    $scope.systemAdminNew = angular.copy($scope.systemAdmin);

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.compare = function (p1, p2) {
        $scope.result = angular.equals(p1, p2);
        if (!$scope.result)
            $scope.compareTo = true;
        else
            $scope.compareTo = false;
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.systemAdmin = $scope.systemAdmins[$scope.index];
        $scope.systemAdminNew = angular.copy($scope.systemAdmin);
        $scope.systemAdminNew.ConfirmPassword = $scope.systemAdminNew.Password;
        $scope.systemAdminNew.AddedDate = $filter('dateFilter')($scope.systemAdminNew.AddedDate);
        $scope.systemAdminNew.UpdatedDate = $filter('dateFilter')($scope.systemAdminNew.UpdatedDate);
        $scope.systemAdminNew.DateOfBirth = $filter('dateFiltering')($scope.systemAdminNew.DateOfBirth);

        $scope.systemAdminNew.EmailVerifiedDate = $filter('dateFilter')($scope.systemAdminNew.EmailVerifiedDate);
        $scope.systemAdminNew.UserLockedDate = $filter('dateFilter')($scope.systemAdminNew.UserLockedDate);
        $scope.systemAdminNew.UserUnlockDate = $filter('dateFilter')($scope.systemAdminNew.UserUnlockDate);
        $scope.systemAdminNew.AuthTokenLockedDate = $filter('dateFilter')($scope.systemAdminNew.AuthTokenLockedDate);
        $scope.systemAdminNew.AuthTokenUnlockDate = $filter('dateFilter')($scope.systemAdminNew.AuthTokenUnlockDate);
        $scope.systemAdminNew.LastPwdChangedDay = $filter('dateFilter')($scope.systemAdminNew.LastPwdChangedDay);
        $scope.inactive = true;
        $scope.passwordShow = false;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.companyGroupName = document.getElementById("groupId").options[document.getElementById('groupId').selectedIndex].text;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.systemAdminNewForm.$valid) {
            angular.copy($scope.systemAdminNew, $scope.systemAdmin);
            $scope.systemAdmin.DateOfBirth = $filter('dateFiltering')($scope.systemAdmin.DateOfBirth);
            $scope.systemAdmin.EmailVerifiedDate = $filter('dateFilter')($scope.systemAdmin.LastPwdChangedDay);
            $scope.systemAdmin.UserLockedDate = $filter('dateFilter')($scope.systemAdmin.LastPwdChangedDay);
            $scope.systemAdmin.UserUnlockDate = $filter('dateFilter')($scope.systemAdmin.LastPwdChangedDay);
            $scope.systemAdmin.AuthTokenLockedDate = $filter('dateFilter')($scope.systemAdmin.LastPwdChangedDay);
            $scope.systemAdmin.AuthTokenUnlockDate = $filter('dateFilter')($scope.systemAdmin.LastPwdChangedDay);
            $scope.systemAdmin.LastPwdChangedDay = $filter('dateFilter')($scope.systemAdmin.LastPwdChangedDay);
            if (!angular.equals($scope.systemAdmin.Password, $scope.systemAdmin.ConfirmPassword))
                return ShowResult('Confirm password does not match.', 'failure');
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.systemAdmin,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.systemAdmin = response.data.User;
                        $scope.systemAdmin.CompanyGroupName = $scope.companyGroupName;
                        $scope.systemAdmins.push($scope.systemAdmin);
                        $scope.systemAdmins = $filter('orderBy')($scope.systemAdmins, 'UserId');
                        baseService.paginationAdd();
                        ClearFields(response.data.AuthToken);
                    }
                })
                return true;
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.systemAdmin,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.systemAdmin.CompanyGroupName = $scope.companyGroupName;
                            $scope.systemAdmins[$scope.index] = $scope.systemAdmin;
                            $scope.systemAdmins = $filter('orderBy')($scope.systemAdmins, 'UserId');
                        }
                        ClearFields(response.data.AuthToken);
                    }
                })
                return true;
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.systemAdminNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.systemAdminNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.systemAdmins.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(data.AuthToken);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    }
    $scope.AuthToken = function () {
        $http.get($scope.getAuth)
            .then(function (response) {
                $scope.systemAdminNew.AuthToken = response.data;
            });
    }
    $scope.AuthToken();

    $scope.Clear = function () {
        ClearFields($scope.AuthToken());
        return true;
    }

    function ClearFields(auth) {
        $scope.Action = "Save";
        $scope.systemAdmin = {};
        $scope.systemAdminNew = {};
        $scope.systemAdminNew.AuthToken = auth;
        $scope.systemAdminNew.PasswordChangeOnFirstLogin = true;
        $scope.systemAdminNew.PasswordNeverExpired = true;
        $scope.systemAdminNew.Active = true;
        $scope.inactive = false;
        $scope.passwordShow = true;
    }

    //Check pin digit
    $scope.pindigit = function (event) {
        var max_chars = 6;
        if (event.target.value.length > max_chars) {
            event.target.value = event.target.value.substr(0, max_chars);
        }
    }
}