'use strict';
UserRoleDetailController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'dataShare'];
function UserRoleDetailController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, dataShare) {
    $rootScope.title = 'User Access Detail';
    $scope.Action = 'Save';
    $scope.index = -1;

    $scope.userRoleDetail = {
        Id: null,
        UserId: null,
        UserName: null,
        FullName: null,
        CompanyId: null,
        CompanyName: null,
        RoleId: null,
        RoleName: null,
        UserAccessId: null,
        MenuMasterId: null,
        MenuActionId: null,
        Active: true
    };

    GetUserInfoFromUserAccess();

    function GetUserInfoFromUserAccess() {
        if ($window.userId !== null) {
            var data = dataShare.getData();
            $scope.userRoleDetail = data;
            $scope.userRoleDetail.UserAccessId = data.Id;
            $scope.userRoleDetail.UserId = $window.userId;
            $scope.userRoleDetail.FullName = $window.fullName;
            $scope.userRoleDetail.UserName = $window.userName;
            Search();
        }
    }

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    cboService.getCboRoleByCompanyGroup(null, function (result) {
        $scope.roleList = result;
    });

    $http({
        method: 'GET',
        url: 'Securities/user/getuserlistwithoutsysadmin'
    }).then(function successCallback(response) {
        $scope.userList = response.data;
    });

    $scope.Save = function () {
        $scope.menuActions = [];
        angular.forEach($scope.roleDetails,
            function (item, index) {
                angular.forEach(item.MenuActions,
                    function (item2, index) {
                        $scope.menuActions.push({
                            Id: null,
                            MenuMasterId: item2.MenuMasterId,
                            MenuActionId: item2.MenuActionId,
                            UserId: $scope.userRoleDetail.UserId,
                            UserAccessId: $scope.userRoleDetail.UserAccessId,
                            CompanyId: $scope.userRoleDetail.CompanyId,
                            Active: item2.Active
                        });
                    });
            });
        $http({
            method: 'POST',
            url: 'Securities/UserRoleDetail/Create',
            data: { 'userRoleDetail': $scope.menuActions, 'roleId': $scope.userRoleDetail.RoleId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                ShowResult(response.data.Message, 'success');
            }
        },
            function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
    };

    // #region Role Load

    function Search() {
        $http({
            method: 'GET',
            url: 'Securities/roledetail/getmenuandactionlist',
            params: { 'roleId': $scope.userRoleDetail.RoleId, 'userAccessId': $scope.userRoleDetail.UserAccessId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.roleDetails = response.data;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
    }

    // #endregion

    $scope.Back = function () {
        $window.userId = $scope.userRoleDetail.UserId;
        $window.fullName = $scope.userRoleDetail.FullName;
        $window.userName = $scope.userRoleDetail.UserName;
        $window.history.back();
    };
}