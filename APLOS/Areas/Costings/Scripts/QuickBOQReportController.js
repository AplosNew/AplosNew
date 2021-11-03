'use strict';
QuickBOQReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function QuickBOQReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Quick BOQ Report';
    $scope.ModelList = [];
    $scope.path = 'Costings/QuickBOQReport/';

    baseService.init($scope.getListUrl);



    $scope.getQuickBOQReport = function () {
        //});
        try {
            var file_src = $scope.path + 'GetQuickBOQReport';
            $rootScope.report(file_src);

        } catch (e) {

        }
      

    }

    

   
}