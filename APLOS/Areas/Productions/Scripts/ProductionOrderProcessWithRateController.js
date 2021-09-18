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
        Id: null,
        ProductionEntityId: null,
        ProcessId: null,
        ProductionOrderId: null,
        SelectedDropDownValue: null,
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
    $scope.SKUList = [];
    $scope.getMatrixValue = function () {
        $http({
            method: 'POST',
            url: "Productions/ProductionOrderProcessWithRate/GetSKUMatrix",
            data: { ProcessId: $scope.modelNew.ProcessId, ProductionOrderId: $scope.modelNew.ProductionOrderId, SkuId: $scope.SelectedProductionOrder.SKUId, Sequence: $scope.SelectedProductionOrder.Sequence }
        }).then(function successCallback(response) {
            $scope.SKUList = response.data;
            
            if ($scope.SelectedProductionOrder.Sequence == 2 || $scope.SelectedProductionOrder.Sequence == 1) {

            }
            else {
                var Check = 0;
                $scope.ColumnList = [];
                for (var i = 0; i < $scope.SKUList.length; i++) {
                    if ($scope.ColumnList.length == 0) {
                        Check = $scope.SKUList[i].FirstCharacteristicsValueId;
                        $scope.ColumnList.push({ "ColorName": $scope.SKUList[i].CharValue1, "ColumnValue": $scope.SKUList[i].FirstCharacteristicsValueId, childList: [] });
                    }
                    else {
                        if (Check != $scope.SKUList[i].FirstCharacteristicsValueId) {
                            Check = $scope.SKUList[i].FirstCharacteristicsValueId;
                            $scope.ColumnList.push({ "ColorName": $scope.SKUList[i].CharValue1, "ColumnValue": $scope.SKUList[i].FirstCharacteristicsValueId, childList: [] });
                        }
                    }
                }
                for (var i = 0; i < $scope.ColumnList.length; i++) {
                    if ($scope.ColumnList[i].childList.length == 0) {
                        for (var k = 0; k < $scope.SKUList.length; k++) {
                            $scope.ColumnList[i].childList.push({ "SizeName": $scope.SKUList[k].CharValue2, "SizeValue": $scope.SKUList[k].SecondCharacteristicsValueId, "Rate": $scope.SKUList[k].Rate });
                        }
                    }
                }
            }
            $scope.FGSizeOrColor = $scope.SKUList[0].Char;
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

            for (var i = 0; i < row.Charactaristics.length; i++) {
                if ($scope.SelectedProductionOrder.SKUId == row.Charactaristics[i].Value) {
                    $scope.SelectedProductionOrder.Sequence = row.Charactaristics[i].Sequence;
                }
            }
            $scope.getMatrixValue();
            if ($scope.SelectedProductionOrder.Sequence == 2 || $scope.SelectedProductionOrder.Sequence == 1) {
                angular.element(document.querySelector('#firstPopup')).modal('show');
            }
            else {
                angular.element(document.querySelector('#secondPopup')).modal('show');
            }



        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.closeCharPopUp = function () {
        $scope.skuList = [];
        $scope.firstSKUList = [];
        $scope.salesOrderId = null;
        angular.element(document.querySelector('#firstPopup')).modal('hide');
        angular.element(document.querySelector('#secondPopup')).modal('hide');
        angular.element(document.querySelector('#thirdPopup')).modal('hide');
    };
    $scope.charSave = function () {
        try {
            for (var i = 0; i < $scope.ProductionOrderList.length; i++) {
                if ($scope.ProductionOrderList[i].SKUId != "") {
                    $scope.modelNew.SelectedDropDownValue = $scope.ProductionOrderList[i].SKUId;
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'Master': $scope.modelNew, 'ChildData': $scope.SKUList, 'Sequence': $scope.SelectedProductionOrder.Sequence },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.closeCharPopUp();
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.Delete = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.deleteUrl,
                data: { 'MasterId': $scope.SKUList[0].MasterId},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.closeCharPopUp();
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
}