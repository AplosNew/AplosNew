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

    $scope.SOItemList = [];

    $scope.SearchSOItemList = [];
    $scope.AddSO = function () {
        $scope.itemList = [];
        $http.get('Materials/MaterialIssueControl/GetSOItemList?entityid=' + $scope.productionSummaryNew.EntityId + '&ProductionOrderId=' + $scope.productionSummaryNew.ProductionOrderId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SearchSOItemList = response.data;

                        if (baseService.arrayLength($scope.SOItemList) > 0) {
                            for (var i = 0; i < $scope.SOItemList.length; i++) {
                                for (var j = 0; j < $scope.SearchSOItemList.length; j++) {
                                    if ($scope.SOItemList[i].LineItemId == $scope.SearchSOItemList[j].LineItemId) {
                                        $scope.SearchSOItemList.splice(j, 1);
                                    }
                                }
                            }
                        }
                        var ob = { Value: null, Text: null };
                        for (var i = 0; i < $scope.SearchSOItemList.length; i++) {
                            ob.Value = $scope.SearchSOItemList[i].LineItemId;
                            ob.Text = $scope.SearchSOItemList[i].LineItemId;
                            $scope.itemList.push(ob);
                            ob = {};
                        }
                    }


                    angular.element(document.querySelector('#SOpopUp')).modal('show');
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.itemList = [];
    $scope.closeSOPopUp = function () {
        try {
            for (var i = 0; i < $scope.SearchSOItemList.length; i++) {

                if ($scope.SearchSOItemList[i].Flag) {
                    if (checkExistsItem($scope.SOItemList, $scope.SearchSOItemList[i].LineItemId)) {
                        $scope.SOItemList.push($scope.SearchSOItemList[i]);
                    }
                    else {
                        $scope.SOItemList = [];
                        throw "Select same Line Item";
                    }
                }
            }
            $scope.GetQBOQCostingData();
            angular.element(document.querySelector('#SOpopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
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










}