'use strict';
CompanyWiseBankSheetController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanyWiseBankSheetController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Company Wise Bank Sheet';
    $scope.path = 'Payrolls/CompanyWiseBankSheet/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.GetList = [];

    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.path ,
        }).then(function successCallback(response) {
            $scope.GetList = response.data;
        });
    };
    //$scope.getData();
}