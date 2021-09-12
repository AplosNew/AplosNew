'use strict';
ProductionOrderProcessWithRateController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function ProductionOrderProcessWithRateController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Production Order Process With Rate';
    $scope.Action = 'Save';

    $scope.path = 'Productions/ProductionOrderProcessWithRate/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.modelNew.ProductionEntityId = $scope.entityList[0].Value;
                //default                
            }
        });
    }
    $scope.getAllEntities();
    $scope.loadProcessList = function () {
        cboService.GetEntityProcessCbo($scope.modelNew.ProductionEntityId, function (result) {
            $scope.processList = result;
        });
    };
    $scope.SKUList = [];
    $scope.loadSKU = function () {
        $http({
            method: 'POST',
            url: "Productions/ProductionOrderProcessWithRate/GetSKU",
            data: { ProcessId: $scope.modelNew.ProcessId }
        }).then(function successCallback(response) {
            $scope.SKUList = response.data;
        });
    }
}