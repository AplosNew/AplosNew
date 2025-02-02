'use strict';
accountStatusBIController.$inject = ['commonMessage', '$scope', '$rootScope', 'cboService', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function accountStatusBIController(commonMessage, $scope, $rootScope, cboService, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Account Status";
    $scope.comInfo = {};
    cboService.getCboInterCompany(null, function (result) {
        $scope.companyList = result;
        $scope.comInfo.CompanyId = $scope.companyList[0].CompanyId;
        $scope.comInfo.CompanyName = $scope.companyList[0].CompanyName;
    });
}