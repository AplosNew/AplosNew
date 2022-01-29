'use strict';
StocksAgeingReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function StocksAgeingReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Finished Goods Stock Ageing ';
    $scope.path = 'Productions/StocksAgeingReport/';


    // Variables
    $scope.FromDate = null;
    $scope.ToDate = null;

    //Operations
    $scope.show = function () {
        console.log($scope.FromDate, ' ', $scope.ToDate);
    }
}