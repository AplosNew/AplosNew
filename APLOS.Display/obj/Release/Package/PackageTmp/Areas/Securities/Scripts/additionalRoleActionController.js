'use strict';
AdditionalRoleActionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'dataShare'];
function AdditionalRoleActionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, dataShare) {
    $rootScope.title = "Additional Role Action";
    $scope.roleList = [];
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
        ModuleId: null,
        MenuFrameId: null,
        MenuActionId: null,
        Active: true
    };
    GetRoleActionByRole();

    function GetRoleActionByRole() {
        $scope.roleList = $window.RoleDetails;

        var data = dataShare.getData();
        $scope.userRoleDetail = data;
        Search();
    }
    function Search() {
        $http({
            method: 'GET',
            url: 'Securities/roledetail/getroledetaillistforaditionalrole',
            params: {
                'userId': $scope.userRoleDetail.UserId,
                'companyId': $scope.userRoleDetail.CompanyId,
                'moduleId': $scope.userRoleDetail.ModuleId,
                'menuFrameId': $scope.userRoleDetail.MenuFrameId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.roleDetails = response.data;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
    }

    $scope.Save = function () {
        $scope.menuActions = [];
        angular.forEach($scope.roleDetails, function (item, index) {
            angular.forEach(item.MenuActions, function (item2, index) {
                $scope.menuActions.push({
                    Id: null,
                    MenuMasterId: item2.MenuMasterId,
                    CompanyId: $scope.userRoleDetail.CompanyId,
                    UserId: $scope.userRoleDetail.UserId,
                    MenuActionId: item2.MenuActionId,
                    Active: item2.Active
                });
            });
        });
        $http({
            method: 'POST',
            url: 'Securities/UserRoleDetail/Save',
            data: { 'userRoleDetail': $scope.menuActions },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
    };

    $scope.Back = function () {
        //$window.UserId = $scope.userRoleDetail.UserId;
        $window.UserRoleUserId = $scope.userRoleDetail.UserId;
        $window.UserRoleFullName = $scope.userRoleDetail.FullName;
        $window.userName = $scope.userRoleDetail.UserName;
        $window.RoleDetails = $scope.roleList;

        $window.UserRoleCompanyId = $scope.userRoleDetail.CompanyId;
        $window.history.back();
    };
}