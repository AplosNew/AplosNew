'use strict';
CompanyGroupMenuMasterController.$inject = ['baseService', 'cboService', '$rootScope', '$scope', '$http', '$filter'];
function CompanyGroupMenuMasterController(baseService, cboService, $rootScope, $scope, $http, $filter) {
    $rootScope.title = "Company Group Menu Master";
    $scope.companyGroupList = [];
    $scope.companyGroupMenuMaster = {
        Id: null,
        ModuleId: null,
        CompanyGroupId: null,
        MenuFrameId: null
    };

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.getCboModuleByCompanyGroup = function () {
        cboService.getCboModuleByCompanyGroup($scope.companyGroupMenuMaster.CompanyGroupId, function (result) {
            $scope.moduleList = result;
        });
    };

    $scope.menuFarmeGet = function () {
        $http({
            method: 'GET',
            url: 'Menus/menumaster/getmenuframebymoduleidcbo?moduleId=' + $scope.companyGroupMenuMaster.ModuleId
        }).then(function successCallback(response) {
            $scope.menuFrameList = response.data;
        });
    };

    $scope.getData = function () {
        $http({
            method: 'GET',
            url: 'Menus/companygroupmenumaster/getlist?companyGroupId=' + $scope.companyGroupMenuMaster.CompanyGroupId
            + '&moduleId=' + $scope.companyGroupMenuMaster.ModuleId + '&menuFrameId=' + $scope.companyGroupMenuMaster.MenuFrameId
        }).then(function successCallback(response) {
            $scope.menuMasters = response.data;
        });
    };

    $scope.Save = function () {
        $http({
            method: 'POST',
            url: 'Menus/companygroupmenumaster/create',
            data: $scope.menuMasters,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                $scope.getData();
                ShowResult(response.data.Message, 'success');
            }
        });
        return true;
    };
}