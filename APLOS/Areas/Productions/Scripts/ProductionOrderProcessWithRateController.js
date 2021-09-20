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
    $scope.Rate = null;
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
            else if ($scope.SelectedProductionOrder.Sequence == "Both") {
                var Check = 0;
                $scope.ColumnList = [];
                for (var i = 0; i < $scope.SKUList.length; i++) {
                    if ($scope.ColumnList.length == 0) {
                        Check = $scope.SKUList[i].FirstCharacteristicsValueId;
                        $scope.ColumnList.push({ "ColorName": $scope.SKUList[i].CharValue1, "ColumnValue": $scope.SKUList[i].FirstCharacteristicsValueId, childList: [], "FirstCharacteristicsId": $scope.SKUList[i].FirstCharacteristicsId });
                    }
                    else {
                        if (Check != $scope.SKUList[i].FirstCharacteristicsValueId) {
                            Check = $scope.SKUList[i].FirstCharacteristicsValueId;
                            $scope.ColumnList.push({ "ColorName": $scope.SKUList[i].CharValue1, "ColumnValue": $scope.SKUList[i].FirstCharacteristicsValueId, childList: [], "FirstCharacteristicsId": $scope.SKUList[i].FirstCharacteristicsId });
                        }
                    }
                }
                for (var i = 0; i < $scope.ColumnList.length; i++) {
                    if ($scope.ColumnList[i].childList.length == 0) {
                        for (var k = 0; k < $scope.SKUList.length; k++) {
                            if ($scope.SKUList[k].FirstCharacteristicsValueId == $scope.ColumnList[i].ColumnValue) {
                                $scope.ColumnList[i].childList.push({ "SizeName": $scope.SKUList[k].CharValue2, "SizeValue": $scope.SKUList[k].SecondCharacteristicsValueId, "Rate": $scope.SKUList[k].Rate, "SecondCharacteristicsId": $scope.SKUList[k].SecondCharacteristicsId });
                            }
                        }
                    }
                }
            }
            else {

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
            if ($scope.modelNew.ProcessId == null) {
                throw "Select Process.."
            }
            $scope.ProductionOrderList = [];
            $http.get("Productions/ProductionOrderProcessWithRate/GetProductionOrderDataList?entityId=" + $scope.modelNew.ProductionEntityId + "&ProcessId=" + $scope.modelNew.ProcessId)
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.ProductionOrderList = response.data;

                            for (var i = 0; i < $scope.ProductionOrderList.length; i++) {
                                for (var j = 0; j < $scope.ProductionOrderList[i].Charactaristics.length; j++) {
                                    if ($scope.ProductionOrderList[i].Charactaristics[j].Value == null && $scope.ProductionOrderList[i].Charactaristics[j].Text == null) {
                                        $scope.ProductionOrderList[i].IsDisable = true;
                                    }
                                }
                            }

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
            $scope.modelNew.SelectedDropDownValue = $scope.SelectedProductionOrder.SKUId;
            $scope.modelNew.Id = $scope.SelectedProductionOrder.Id;

            for (var i = 0; i < row.Charactaristics.length; i++) {
                if ($scope.SelectedProductionOrder.SKUId == row.Charactaristics[i].Value) {
                    $scope.SelectedProductionOrder.Sequence = row.Charactaristics[i].Sequence;
                }
            }

            if ($scope.SelectedProductionOrder.IsDisable == false && $scope.SelectedProductionOrder.SKUId == "" || $scope.SelectedProductionOrder.IsDisable == false && baseService.isUndefinedOrNull($scope.SelectedProductionOrder.SKUId)) {
                throw "Select SKU..!";
            }
            else {
                if ($scope.SelectedProductionOrder.Sequence == 2 || $scope.SelectedProductionOrder.Sequence == 1) {
                    $scope.getMatrixValue();
                    angular.element(document.querySelector('#firstPopup')).modal('show');
                }
                else if ($scope.SelectedProductionOrder.Sequence == 'Both') {
                    $scope.getMatrixValue();
                    angular.element(document.querySelector('#secondPopup')).modal('show');
                }
                else {
                    $scope.getRateData($scope.modelNew.ProductionEntityId, $scope.modelNew.ProcessId, $scope.modelNew.ProductionOrderId);
                    angular.element(document.querySelector('#thirdPopup')).modal('show');
                }
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
        for (var i = 0; i < $scope.SKUList.length; i++) {
            $scope.SKUList[i].Rate = $scope.SKUList[i].Rate == "" ? null : $scope.SKUList[i].Rate;
        }
        try {
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
                data: { 'MasterId': $scope.modelNew.Id },
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
    $scope.SaveMatrix = function () {
        try {
            for (var i = 0; i < $scope.ColumnList.length; i++) {
                for (var j = 0; j < $scope.SKUList.length; j++) {
                    for (var k = 0; k < $scope.ColumnList[i].childList.length; k++) {
                        if ($scope.SKUList[j].FirstCharacteristicsValueId == $scope.ColumnList[i].ColumnValue
                            && $scope.SKUList[j].SecondCharacteristicsValueId == $scope.ColumnList[i].childList[k].SizeValue
                            && $scope.SKUList[j].FirstCharacteristicsId == $scope.ColumnList[i].FirstCharacteristicsId
                            && $scope.SKUList[j].SecondCharacteristicsId == $scope.ColumnList[i].childList[k].SecondCharacteristicsId && $scope.ColumnList[i].childList[k].Rate != null) {
                            $scope.SKUList[j].Rate = $scope.ColumnList[i].childList[k].Rate == "" ? null : $scope.ColumnList[i].childList[k].Rate;
                        }
                    }
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

    $scope.SaveRate = function () {
        try {
            var VList = [];
            VList.push({ "Rate": $scope.Rate, "FirstCharacteristicsId": null, "FirstCharacteristicsValueId": null, "SecondCharacteristicsId": null, "SecondCharacteristicsValueId": null });
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'Master': $scope.modelNew, 'ChildData': VList, 'Sequence': $scope.SelectedProductionOrder.Sequence },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.closeCharPopUp();
                    //$scope.getRateData($scope.modelNew.ProductionEntityId,);
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.getRateData = function (ProductionEntityId, ProcessId, ProductionOrderId) {

        $http({
            method: 'GET',
            url: 'Productions/ProductionOrderProcessWithRate/GetRate?ProductionEntityId=' + ProductionEntityId + '&ProcessId=' + ProcessId + '&ProductionOrderId=' + ProductionOrderId,
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.Rate = response.data[0].Rate;
                $scope.MasterId = response.data[0].MasterId;
            }
            else {
                $scope.Rate = null;
                $scope.MasterId = null;
            }

        });
    };

    $scope.ConfirmDelete = function () {
        var eDialog = $("#DeletePopUp").data("ejDialog");
        eDialog.open();
    };
    $scope.ConfirmDeleteClose = function () {
        var eDialog = $("#DeletePopUp").data("ejDialog");
        eDialog.close();
    };
    $scope.ConfirmDeleteFirst = function () {
        var eDialog = $("#DeletePopUpFirst").data("ejDialog");
        eDialog.open();
    };
    $scope.ConfirmDeleteCloseFirst = function () {
        var eDialog = $("#DeletePopUpFirst").data("ejDialog");
        eDialog.close();
    };
    $scope.ConfirmDeleteThird = function () {
        var eDialog = $("#DeletePopUpThird").data("ejDialog");
        eDialog.open();
    };
    $scope.ConfirmDeleteCloseThird = function () {
        var eDialog = $("#DeletePopUpThird").data("ejDialog");
        eDialog.close();
    };

}