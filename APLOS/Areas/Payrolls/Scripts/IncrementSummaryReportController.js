'use strict';
IncrementSummaryReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function IncrementSummaryReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Increment Summary Report';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/IncrementSummaryReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    baseService.init($scope.getListUrl);

   // $scope.Date = { FromDate: new Date().toJSON(), ToDate: new Date().toJSON() };

  
    $scope.getIncrementSummaryReport = function() {
        //$http({
        //    method: 'GET',
        //    url: $scope.path + "getEmployeeIncrementReport"
        //}).then(function successCallback(response) {
           
        //});
        try {
            var file_src = $scope.path + "getIncrementSummaryReport" ; 
            $rootScope.report(file_src);

        } catch (e) {

        }


    }
   



   
}