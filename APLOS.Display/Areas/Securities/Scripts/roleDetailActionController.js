'use strict';
RoleDetailActionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'dataShare'];
function RoleDetailActionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, dataShare) {
    $rootScope.title = "Role Privilege Action";
    $scope.roleList = [];
    $scope.roleDetail = {
        Id: null,
        PanelName: null,
        RoleId: null,
        RoleName: null,
        ModuleId: null,
        ModuleName: null,
        MenuFrameId: null,
        MenuFrameName: null,
        Active: true
    };
    GetRoleActionByRole();

    function GetRoleActionByRole() {
        var data = dataShare.getData();
        $scope.roleDetail = data;
        Search();
    }
    function Search() {
        $scope.roleList = $window.RoleDetails;
        $scope.roleDetails = [];
        $http({
            method: 'GET',
            url: 'Securities/roledetail/getroledetaillist',
            params: { 'roleId': $scope.roleDetail.RoleId, 'moduleId': $scope.roleDetail.ModuleId, 'menuFrameId': $scope.roleDetail.MenuFrameId },
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
    $scope.menuActions = [];
    $scope.Save = function () {
        $scope.menuActions = [];
        angular.forEach($scope.roleDetails, function (item, index) {
            angular.forEach(item.MenuActions, function (item2, index) {
                $scope.menuActions.push({
                    Id: null,
                    RoleId: $scope.roleDetail.RoleId,
                    MenuMasterId: item2.MenuMasterId,
                    MenuActionId: item2.MenuActionId,
                    Active: item2.Active
                });
            })
        });
        $http({
            method: 'POST',
            url: 'Securities/roledetail/Create',
            data: $scope.menuActions,
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
    }

    $scope.Back = function () {
        // Set user id in window element.
        $window.RoleId = $scope.roleDetail.RoleId;
        $window.history.back();
    }
}