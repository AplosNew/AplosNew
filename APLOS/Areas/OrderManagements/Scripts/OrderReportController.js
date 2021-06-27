'use strict';
OrderReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http"];
function OrderReportController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http) {
    $rootScope.title = "Order Control Report";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.baseProcess = { Id: null, UserName: null };
    $scope.modelList = [];
    $scope.productionMaterialList = [];
    $scope.prdProcessSetList = [];
    $scope.productionEntityList = [];
    $scope.productionWorkCenterList = [];
    $scope.productWorkCenterList = [];

    $scope.path = 'OrderManagements/OrderReport/';
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

}