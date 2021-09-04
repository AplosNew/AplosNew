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

    $scope.MarkerId = null;
    $scope.CharacteristicsName = null;
    $scope.CharacteristicsId = null;

    $scope.FGCharacteristicsValueList = [];
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
            GetMarker(response.data[0].MaterialMasterId);
            
        });
    }

    //#region MarkerList
    $scope.MarkerList = [];
    function GetMarker(MaterialId) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMarker?MaterialId=' + MaterialId
        }).then(function successCallback(response) {
            $scope.MarkerList = response.data;
            //getProductionProcessSetList();
        });
    }
    $scope.getSKU = function () {
        for (var i = 0; i < $scope.MarkerList.length; i++) {
            if ($scope.MarkerList[i].Value == $scope.MarkerId) {
                $scope.CharacteristicsName = $scope.MarkerList[i].SKU;
                $scope.CharacteristicsId = $scope.MarkerList[i].SKUId;
            }
        }
        $scope.getFGCharacteristicsLists($scope.recipeMaterialListSelected[0].MaterialMasterId);
    };
    $scope.getFGCharacteristics = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMarkerDetails?MarkerId=' + $scope.MarkerId
        }).then(function successCallback(response) {
            $scope.FGCharacteristicsValueList = response.data;
            //getProductionProcessSetList();
        });
    };
    $scope.getFGCharacteristicsLists = function (id) {
        //$scope.clearCharNames();
        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.characteristicsList = [];
            $scope.characteristicsList = response.data.charData;
            for (var i = 0; i < $scope.characteristicsList.length; i++) {
                if ($scope.characteristicsList[i].Value === $scope.CharacteristicsId ) {
                    $scope.characteristicsList.splice(i,1);
                }
            }
            $scope.getOtherFGCharacteristics($scope.characteristicsList[0].Value, $scope.recipeMaterialListSelected[0].MaterialMasterId);
        });
    };
    $scope.SkuValueList = [];
    $scope.getOtherFGCharacteristics = function (skuId, materialMasterId) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetSkuDetails?OtherSku=' + skuId + '&MaterialMasterId=' + materialMasterId
        }).then(function successCallback(response) {
            $scope.SkuValueList = response.data;
        });
    };
}