'use strict';
CompanyGroupModuleController.$inject = ['cboService', '$rootScope', '$scope', '$routeParams', '$location', '$http', '$filter', 'baseService'];
function CompanyGroupModuleController(cboService, $rootScope, $scope, $routeParams, $location, $http, $filter, baseService) {
    $rootScope.title = "Company Group Module";
    $scope.companyGroupModules = [];
    $scope.companyGroupList = [];

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.CompanyGroupId = null;

    $scope.onChange = function () {
        if (!baseService.isUndefinedOrNull($scope.CompanyGroupId))
            GetAll();
        else
            $scope.companyGroupModules = [];
    };

    function GetAll() {
        $http.get('Modules/companygroupmodule/getlist?companyGroupId=' + $scope.CompanyGroupId)
            .then(function (response) {
                $scope.companyGroupModules = response.data;
            });
    }

    $scope.Save = function () {
        $http({
            method: 'POST',
            url: 'Modules/companygroupmodule/create',
            data: $scope.companyGroupModules,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                GetAll();
                ShowResult(response.data.Message, 'success');
            }
        });
    };
}