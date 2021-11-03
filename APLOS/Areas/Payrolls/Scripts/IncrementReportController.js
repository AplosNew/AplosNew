'use strict';
IncrementReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function IncrementReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Increment Report';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/IncrementReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    baseService.init($scope.getListUrl);

    $scope.Date = {
        FromDate: null,
        ToDate: null
    };

  
    $scope.getIncrementReport = function() {
        //$http({
        //    method: 'GET',
        //    url: $scope.path + "getEmployeeIncrementReport"
        //}).then(function successCallback(response) {
           
        //});
        try {
            var FromDate = $filter('dateFiltering')($scope.Date.FromDate, 'dd-MMM-yyyy');   
            var ToDate = $filter('dateFiltering')($scope.Date.ToDate, 'dd-MMM-yyyy');   
            var file_src = $scope.path + "getIncrementReport?FromDate=" + FromDate + '&ToDate=' +ToDate; 
            $rootScope.report(file_src);

        } catch (e) {

        }


    }
   



   
}