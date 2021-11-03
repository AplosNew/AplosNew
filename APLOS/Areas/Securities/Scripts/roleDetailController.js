'use strict';
RoleDetailController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'dataShare', '$window'];
function RoleDetailController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, dataShare, $window) {
    $rootScope.title = "Role Privilege";
    $scope.Action = 'Save';
    $scope.roleId = null;
    $scope.moduleId = null;
    $scope.menuFrameId = null;
    $scope.moduleList = [];
    $scope.menuFrameList = [];
    $scope.roleList = [];
    $scope.roleDetails = [];
    $scope.roleDetailNew = {
        Id: null,
        RoleId: null,
        RoleName: null,
        ModuleId: null,
        ModuleName: null,
        MenuFrameId: null,
        MenuFrameName: null,
        Active: true
    };

    $scope.root = $rootScope.bootPoint;
    console.log("Root",$scope.root);

    $scope.roleDetail = Object.assign({}, $scope.roleDetailNew);
    $http({
        method: 'GET',
        url: 'Modules/companygroupmodule/getmodulebycompanygroupcbo'
    }).then(function (response) {
        $scope.moduleList = response.data;
    });

    $scope.menuFarmeGet = function (id) {
        $http({
            method: 'GET',
            url: 'Menus/menumaster/getmenuframebymoduleidcbo?moduleId=' + id
        }).then(function successCallback(response) {
            $scope.menuFrameList = response.data;
        });
    }

    cboService.getCboRoleByCompanyGroup(null, function (result) {
        $scope.roleList = result;
    });

    $scope.getDataByRole = function () {
        $scope.roleDetails = [];
        $http({
            method: 'GET',
            url: 'Securities/roledetail/getmenuframelistbyrole',
            params: { 'roleId': $scope.roleDetail.RoleId },
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

    if (!baseService.isUndefinedOrNull($window.RoleId)) {
        $scope.roleDetail.RoleId = $window.RoleId;
        $scope.getDataByRole();
        $window.RoleId = null;
    }

    $scope.Add = function () {
        try {
            //$scope.$broadcast('show-errors-check-validity');
            if ($scope.roleDetailForm.$valid) {
                //var getRow = $filter('filter')($scope.roleDetails, { 'ModuleId': $scope.roleDetail.ModuleId, 'MenuFrameId': $scope.roleDetail.MenuFrameId });
                for (var i = 0; i < $scope.roleDetails.length; i++) {
                    if ($scope.roleDetails[i].ModuleId === $scope.roleDetail.ModuleId && $scope.roleDetails[i].MenuFrameId === $scope.roleDetail.MenuFrameId) {
                        throw 'This combination has already been  taken for this role';
                    }
                }


                $scope.roleDetail.ModuleName = angular.element("#moduleId :selected").text();
                $scope.roleDetail.MenuFrameName = angular.element("#menuFrameId :selected").text();

                $scope.roleDetailNew = Object.assign({}, $scope.roleDetail);

                $scope.roleDetails.push({
                    RoleId: $scope.roleDetailNew.RoleId
                    , RoleName: $scope.roleDetailNew.RoleName
                    , ModuleId: $scope.roleDetailNew.ModuleId
                    , ModuleName: $scope.roleDetailNew.ModuleName
                    , MenuFrameId: $scope.roleDetailNew.MenuFrameId
                    , MenuFrameName: $scope.roleDetailNew.MenuFrameName
                });
                $scope.roleDetailNew = {};
                $scope.roleDetail.MenuFrameId = null;
                CloseShowResult();
            }
        } catch (err) {
            ShowResult(err, 'failure');
        }
    };

    $scope.send = function (data) {
        data.RoleName = document.getElementById("roleId").options[document.getElementById('roleId').selectedIndex].text
        $window.RoleDetails = $scope.roleDetails;
        dataShare.sendData(data);
        $window.location = $rootScope.bootPoint + 'role-detail-action';
    };
}