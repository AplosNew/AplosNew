'use strict';
SalesOrderWiseProductionCompletionReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http"];
function SalesOrderWiseProductionCompletionReportController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http) {
    $rootScope.title = "Sales Order Wise Production Completion Report";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.baseProcess = { Id: null, UserName: null };
    $scope.modelList = [];
    $scope.productionMaterialList = [];
    $scope.prdProcessSetList = [];
    $scope.productionEntityList = [];
    $scope.productionWorkCenterList = [];
    $scope.productWorkCenterList = [];

    $scope.path = 'OrderManagements/SalesOrderWiseProductionCompletionReport/';
    $scope.FromDate = '';
    $scope.ToDate = '';
    $scope.EntityId = '';
    $scope.prdProcessSetList = [];
    $scope.ProcessID = '';
    $scope.ProcessID = '';

    $scope.Plants = [];
    $scope.Entities = [];
    $scope.SinglePlantEntity = [];
    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    };
    ////////////////////////////////////////////////REPORT//////////////////////////////////////////////////
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.GetControlChartReportXls = function () {
        try {
            var file_src = 'OrderManagements/OrderReport/GetControlChartReportXls';
            $rootScope.report(file_src);

        } catch (e) {

        }
      
    };

    $scope.getos3 = function () {

        try {
            var file_src = 'OrderManagements/SalesOrderWiseProductionCompletionReport/OS3xls?entityid=' + 118;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

}