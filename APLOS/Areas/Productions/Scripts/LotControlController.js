'use strict';
LotControlController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function LotControlController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'LotControl';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/LotControl/';
    $scope.saveUrl = $scope.path + 'create';


    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.getAllEntities();

    $scope.ProductionOrderList = [];

    $scope.PRSearchColumn = 'Id';
    $scope.PRSearchValue = null;
    $scope.getProductionOrderPopUp = function () {
        $scope.ProductionOrderList = [];
        if (!baseService.isUndefinedOrNull($scope.ModelNew.EntityId)) {
            $http({
                method: 'POST',
                data: {
                    'entityid': $scope.ModelNew.EntityId, 'column': $scope.PRSearchColumn, 'value': $scope.PRSearchValue
                },
                url: 'Materials/MaterialIssueControl/getlist'
            }).then(function successCallback(response) {
                $scope.ProductionOrderList = response.data;
            });
        }
        angular.element(document.querySelector('#POItemPopup')).modal('show');
    };

    $scope.SetPrOData = function ($event) {
        $scope.ModelNew.ProductionOrderId = $event.data.Id;
        $scope.GetPOLotControlSettingData();
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
    }

    $scope.lotControlList = [];
    $scope.GetPOLotControlSettingData = function () {
        try {
            $http({
                method: 'GET',
                url: 'OrderManagements/ProductionOrder/GetPOLotContSettingsData?poId=' + $scope.ModelNew.ProductionOrderId + '&entityId=' + $scope.ModelNew.EntityId
            }).then(function (response) {
                $scope.lotControlList = response.data;

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.copymessage_detailconfirmation = null;
    $scope.copyDetail = function (obj) {
        $scope.DetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.DetailNew.Id))
            $scope.copymessage_detailconfirmation = 'Are you sure want to copy Lot No:' + $scope.DetailNew.UserLotNo+' ?';
        angular.element(document.querySelector('#confirmCopyDetailPopUp')).modal('show');
    }

    $scope.CopyDetailData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'CopyData?ProductionOrderId=' + $scope.DetailNew.ProductionOrderId + '&Id=' + $scope.DetailNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetPOLotControlSettingData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.SaveRowData = function (obj) {
        try {
           
            $http({
                method: 'POST',
                url: 'Productions/LotControl/SaveTNCRowData',
                data: { 'data': obj.data },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetPOLotControlSettingData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, "failure");
        }
    };



}