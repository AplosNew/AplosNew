'use strict';
CutPlanController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function CutPlanController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Cut Plan';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/CutPlan/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

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

    $scope.modelNew = {
        ProductionEntityId: null,
        ProductionOrderId: null
    }

    $scope.ProductionOrderList = [];
    $scope.ProdOrderList = [];
    $scope.getProductionOrderPopUp = function () {
        if ($scope.modelNew.ProductionEntityId == null) {
            throw "Select Production Entity.."
        }
        $scope.ProductionOrderList = [];
        $http.get("Productions/CutPlan/GetProductionOrderDataList?entityId=" + $scope.modelNew.ProductionEntityId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ProductionOrderList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');
    };
    $scope.SelectPOItem = function ($event) {
        $scope.modelNew.ProductionOrderId = $event.data.POId;
        //$scope.GetLineItemData();
        getProductionRecipeMaterialList();
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
    }
    $scope.SalesOrderLineItems = [];
    $scope.recipeMaterialListSelected = [];
    $scope.GetLineItemData = function () {
        $http({
            method: 'GET',
            url: 'Productions/CutPlan/GetLineItemData?entityId=' + $scope.modelNew.ProductionEntityId + '&processId=' + $scope.modelNew.ProcessId + '&productionOrderId=' + $scope.modelNew.ProductionOrderId + '&masterId=' + $scope.modelNew.Id
        }).then(function successCallback(response) {
            $scope.SalesOrderLineItems = response.data;            
        });
    }
    function getProductionRecipeMaterialList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionRecipeMaterialList?productionOrderId=' + $scope.modelNew.ProductionOrderId
        }).then(function successCallback(response) {
            $scope.recipeMaterialListSelected = response.data;
            //getProductionProcessSetList();
        });
    }
}