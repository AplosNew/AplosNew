'use strict';
manpowerStatusBIController.$inject = ['commonMessage', '$scope', '$rootScope', 'cboService', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function manpowerStatusBIController(commonMessage, $scope, $rootScope, cboService, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Manpower Status";
    $scope.comInfo = {};
    cboService.getCboInterCompany(null, function (result) {
        $scope.companyList = result;
        $scope.comInfo.CompanyId = $scope.companyList[0].CompanyId;
        $scope.comInfo.CompanyName = $scope.companyList[0].CompanyName;
    });
}