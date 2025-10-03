'use strict';
DefectMarkerController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DefectMarkerController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.defecttitle = 'Defect Marker';
    $scope.Action = 'Save';
    $scope.DefectModelList = [];
    $scope.path = 'QMS/QualityProcess/';
    $scope.saveUrl = $scope.path + 'createdefect';
    $scope.deleteUrl = $scope.path + 'deletedefect/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.productionSummaryNew = { EntityId: null, WorkCenterMasterId: null, MarkDate: null, ProductionOrderId: null, BuyerItem: null, OwnItem: null, BuyerOrder: null, OwnOrder: null, Remarks: null, ProductionShiftId: null, SalesOrderId: null, ResponsiblePersonId: null, ResponsiblePersonName: null }

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.productionSummaryNew.EntityId = $scope.entityList[0].Value;
            }
        });
    }
    $scope.getAllEntities();

    $scope.wcList = [];
    $scope.loadWC = function () {
        $http.get('Productions/Productionsummary/GetWCCbo?entityId=' + $scope.productionSummaryNew.EntityId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.wcList = response.data;
                }
            });
    };

    $scope.shiftList = [];
    $scope.GetShiftList = function () {
        $scope.shiftList = [];
        $http.get('Productions/Productionsummary/GetShiftCbo?wcId=' + $scope.productionSummaryNew.WorkCenterMasterId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.shiftList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.productionSummaryNew.ProductionShiftId = $scope.shiftList[0].Value;
                    }
                }
            });
    }

    $scope.modelFilterByList = [
        { 'name': 'Prod. Order#', 'value': 'Id' },
        { 'name': 'Prod. Status', 'value': 'ProductionStatus' },
        { 'name': 'Material', 'value': 'Material' },
        { 'name': 'Product', 'value': 'Product' },
        { 'name': 'Product Category', 'value': 'ProductCategory' },
        { 'name': 'Master Order No', 'value': 'MasterOrderId' },
        { 'name': 'Buyer Order#', 'value': 'BuyerRefNo' },
        { 'name': 'Own Order#', 'value': 'OwnRefNo' },
        { 'name': 'Buyer Item#', 'value': 'StyleNo' },
        { 'name': 'Own Item#', 'value': 'OwnStyleNo' },
        { 'name': 'SO No', 'value': 'SONo' },
        { 'name': 'SO Desc', 'value': 'SODesc' },
        { 'name': 'Buyer', 'value': 'buyer' },
        { 'name': 'Customer', 'value': 'Customer' },
    ];

    $scope.PRSearchColumn = 'Id';
    $scope.PRSearchValue = null;
    $scope.modelList = [];
    $scope.getPOData = function () {
        try {
            $scope.modelList = [];
            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.EntityId)) {
                throw "Entity is required.";
            }
            $http({
                method: 'POST',
                data: {
                    'entityid': $scope.productionSummaryNew.EntityId, 'column': $scope.PRSearchColumn, 'value': $scope.PRSearchValue
                },
                url: 'Materials/MaterialIssueControl/getlist'
            }).then(function successCallback(response) {
                $scope.modelList = response.data;
                angular.element(document.querySelector('#POItemPopup')).modal('show');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.rowDataBound = function rowDataBound(e) {
        if (e.data.Balance != 0) {
            e.row.css("background-color", '#FFFF00')
        }

    }

    $scope.SetPO = function ($event) {
        $scope.productionSummaryNew.ProductionOrderId = $event.data.Id;
        $scope.productionSummaryNew.BuyerItem = $event.data.BuyerItem;
        $scope.productionSummaryNew.OwnItem = $event.data.OwnItem;
        $scope.productionSummaryNew.BuyerOrder = $event.data.BuyerOrder;
        $scope.productionSummaryNew.OwnOrder = $event.data.OwnOrder;
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SOId === id) {
                return true;
            }
        }
        return false;
    }

    function checkExistsItem(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].LineItemId !== id) {
                return false;
            }
        }
        return true;
    }

    $scope.ShowDefectMarkingpopUp = function () {
        angular.element(document.querySelector('#DefectMarkingPopup')).modal('show');
    }

    $scope.CloseDefectMarkingpopUp = function () {
        angular.element(document.querySelector('#DefectMarkingPopup')).modal('hide');
    }

    $scope.SalesOrderListForProductionOrderId = [];
    $scope.getSalesOrderByProdOrderList = function () {
        /*$scope.openPopup('dialogSOItemsFromProductionOrder');*/
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetProductionRecipeMaterialList?productionOrderId=' + $scope.productionSummaryNew.ProductionOrderId
        }).then(function successCallback(response) {
            $scope.SalesOrderListForProductionOrderId = response.data;
            angular.element(document.querySelector('#SOItemPopup')).modal('show');

        });
    }
    $scope.SetSO = function ($event) {
        $scope.productionSummaryNew.SalesOrderId = $event.data.SalesOrderId;
        $scope.getSalesOrderColorSizeList();
        angular.element(document.querySelector('#SOItemPopup')).modal('hide');
    }
    $scope.CloseSOpopUp = function () {
        angular.element(document.querySelector('#SOItemPopup')).modal('hide');
    }

    $scope.colorList = [];
    $scope.sizeList = [];
    $scope.getSalesOrderColorSizeList = function () {
        $http({
            method: 'GET',
            url: 'QMS/QualityProcess/GetColorSizeCbo?soId=' + $scope.productionSummaryNew.SalesOrderId
        }).then(function successCallback(response) {
            $scope.colorList = response.data.colorItem;
            $scope.sizeList = response.data.sizeItem;

        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.productionSummaryNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };


}