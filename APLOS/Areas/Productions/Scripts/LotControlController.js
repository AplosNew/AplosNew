'use strict';
LotControlController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function LotControlController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'LotControl';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/LotControl/';
    $scope.saveUrl = $scope.path + 'create';


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
        $scope.lotControlList = [];
        var proId = null;
        var moinc = 0;
        var moincvalue = 0;
        var soinc = 0;
        var soincvalue = 0;
        var mlot = 0;
        var gr = 0;
        var sgr = 0;
        $http({
            method: 'GET',
            url: 'Productions/LotControl/GetPOLotControlSettingData?entityId=' + $scope.ModelNew.EntityId + '&PoId=' + $scope.ModelNew.ProductionOrderId
        }).then(function successCallback(response) {
            $scope.lotControlList = response.data;
            for (var i = 0; i < $scope.lotControlList.length; i++) {
                $scope.lotControlList[i].LotNo = $scope.ModelNew.ProductionOrderId;
                $scope.lotControlList[i].ProductionOrderId = $scope.ModelNew.ProductionOrderId;
                $scope.lotControlList[i].EntityId = $scope.ModelNew.EntityId;


                if ($scope.lotControlList[i].ProductionBookingLevel == 'MasterOrderItem') {
                    if (i == 0) {
                        moinc++;
                        moincvalue = moinc;
                        $scope.lotControlList[i].LotNo = $scope.lotControlList[i].LotNo + '-' + moincvalue;
                        gr = $scope.lotControlList[i].LotNo;
                    } else {
                        $scope.lotControlList[i].LotNo = gr;
                    }

                }
                if ($scope.lotControlList[i].ProductionBookingLevel == 'SalesOrder') {

                    soinc++;
                    soincvalue = soinc;
                    $scope.lotControlList[i].LotNo = $scope.lotControlList[i].LotNo + '-S' + soincvalue;

                }

                if ($scope.lotControlList[i].ProductionBookingLevel == 'ProductionOrder') {
                    $scope.lotControlList[i].LotNo = $scope.lotControlList[i].LotNo
                }
                proId = $scope.lotControlList[i].ProcessId;

            }


        });
    }

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


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.lotControlList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

   

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = {
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
    }


}