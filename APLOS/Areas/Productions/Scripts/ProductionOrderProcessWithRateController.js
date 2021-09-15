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

    $scope.modelNew = {
        ProductionEntityId: null,
        ProcessId: null,
        ProductionOrderId: null,
    }

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
    $scope.SKUList1 = [];
    $scope.SKUList2 = [];
    $scope.getMatrixValue = function () {
        $http({
            method: 'POST',
            url: "Productions/ProductionOrderProcessWithRate/GetSKUMatrix",
            data: { ProcessId: $scope.modelNew.ProcessId, ProductionOrderId: $scope.modelNew.ProductionOrderId, SkuId: $scope.SelectedProductionOrder.SkuId }
        }).then(function successCallback(response) {
            $scope.SKUList1 = response.data;
            $scope.SKUList2 = response.data;
        });
    }
    $scope.ProductionOrderList = [];
    $scope.getData = function () {
        try {
            if ($scope.modelNew.ProductionEntityId == null) {
                throw "Select Production Entity.."
            }
            $scope.ProductionOrderList = [];
            $http.get("Productions/ProductionOrderProcessWithRate/GetProductionOrderDataList?entityId=" + $scope.modelNew.ProductionEntityId + "&ProcessId=" + $scope.modelNew.ProcessId)
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.ProductionOrderList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
    $scope.SelectedProductionOrder = {};
    $scope.ShowDiv = false;
    $scope.AddButton = function (row) {
        try {
            $scope.ShowDiv = true;

            $scope.SelectedProductionOrder = row;
            $scope.modelNew.ProductionOrderId = $scope.SelectedProductionOrder.POId;
            
            var eDialog = $("#SKUPopUp").data("ejDialog");
            eDialog.open();

            $scope.getMatrixValue();

            //if (data.IsExemption == true) {
            //    $("#General").ejDialog("setTitle", data.SalaryHead );
            //    eDialog.open();
            //}
            //else {
            //    throw "Exemption Applicable is not checked for this Taxable Income";
            //}
        } catch (e) {
            ShowResult(e, "failure");
        }

    };
}