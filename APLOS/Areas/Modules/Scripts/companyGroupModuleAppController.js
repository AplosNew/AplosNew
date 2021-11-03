'use strict';
CompanyGroupModuleAppController.$inject = ['cboService', '$rootScope', '$scope', '$routeParams', '$location', '$http', '$filter', 'baseService'];
function CompanyGroupModuleAppController(cboService, $rootScope, $scope, $routeParams, $location, $http, $filter, baseService) {
    $rootScope.title = "Company Group Module App";
    $scope.companyGroupList = [];
    $scope.tableShow = false;
    $scope.index = -1;

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });
    $scope.CompanyGroupId = null;
    $scope.onChange = function () {
        if (!baseService.isUndefinedOrNull($scope.CompanyGroupId))
            GetAll();
        else
            $scope.companyGroupModuleApps = [];
    };
    function GetAll() {
        $http.get('Modules/companygroupmoduleapp/getlist?companyGroupId=' + $scope.CompanyGroupId)
            .then(function (response) {
                $scope.companyGroupModuleApps = response.data;
            });
    }
    $scope.Save = function () {
        $http({
            method: 'POST',
            url: 'Modules/companygroupmoduleapp/create',
            data: $scope.companyGroupModuleApps,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                GetAll();
                ShowResult(response.data.Message, 'success');
            }
        });
    };
}