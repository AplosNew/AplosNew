'use strict';
atAGlanceBIController.$inject = ['commonMessage', '$scope', 'cboService','$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function atAGlanceBIController(commonMessage, $scope, cboService, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "At A Glance";

    $scope.comInfo = {};
    cboService.getCboInterCompany(null, function (result) {
        $scope.companyList = result;
        $scope.comInfo.CompanyId = $scope.companyList[0].CompanyId;
        $scope.comInfo.CompanyName = $scope.companyList[0].CompanyName;
    });

}