'use strict';
bulletinReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$sce'];
function bulletinReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $sce) {
    $scope.companyList = [];
    $scope.cboPlantList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.getPlant = function (args) {
        $scope.companyId = args.value;
        cboService.getCboPlantByCompany($scope.companyId, function (result) {
            $scope.cboPlantList = result;
        });
    };


}