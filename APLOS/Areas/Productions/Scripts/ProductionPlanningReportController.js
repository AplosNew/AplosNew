
'use strict';
ProductionPlanningReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProductionPlanningReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Production Planning Report";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Productions/ProductionPlanningReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.fromDate= $filter('dateFiltering')(Date.now());
    $scope.toDate= $filter('dateFiltering')(Date.now());

    $scope.ProductionPlanningReport = function () {

        try {

            if (angular.isUndefinedOrNull($scope.fromDate))
                throw 'Plase select from date.';

            if (angular.isUndefinedOrNull($scope.toDate))
                throw 'Plase select to date.';            

            var file_src = $scope.path + 'GetProductionPlanningReport?fromDate=' + $scope.fromDate + '&toDate=' + $scope.toDate;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }


}