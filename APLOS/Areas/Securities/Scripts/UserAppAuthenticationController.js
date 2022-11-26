'use strict';
UserAppAuthenticationController.$inject = ['cboService', 'baseService', '$rootScope', '$scope', '$routeParams', '$location', '$http', '$filter'];
function UserAppAuthenticationController(cboService, baseService, $rootScope, $scope, $routeParams, $location, $http, $filter) {
    $rootScope.title = "User AppAuthentication";
    $scope.saveUrl = 'Securities/UserAppAuthentication/Save'
    $scope.tableShow = false;
    $scope.Action = 'Save';

    $scope.userRoleNew = {
        Id: null,
        RoleId: null,
        ModuleId: null,
        Active: true,
        IconId: null
    }
    $scope.userAccessApp = Object.assign({}, $scope.userRoleNew);

    $scope.roleList = [];
    $scope.getRole = function () {
        $http.get('Securities/UserAppAuthentication/getRole').
            then(function successCallback(response) {
                $scope.roleList = response.data;
            },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };
    $scope.getRole();

    $scope.ModuleList = [];
    $scope.getModule = function () {
        $http.get('Securities/UserAppAuthentication/getModule').
            then(function successCallback(response) {
                $scope.ModuleList = response.data;
            },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };
    $scope.getModule();

    $scope.IconList = [];
    $scope.geticon = function () {
        $http.get('Securities/UserAppAuthentication/geticon?moduleid=' + $scope.userAccessApp.ModuleId).
            then(function successCallback(response) {
                $scope.IconList = response.data;
            },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };
    
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'data': $scope.userAccessApp,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                ClearFields(response.data.Sequence);
                $scope.getData();

            }
        }), function errorCallBack(response) { ShowResult(response.data.Message, 'failure'); }
    };


    $scope.RoleList = [];
    $scope.Getlist = function () {
        $http.get('Securities/UserAppAuthentication/Getlist')
            .then(function successCallback(response) {
                $scope.RoleList = response.data;
            });
    }
    $scope.Getlist();
}

