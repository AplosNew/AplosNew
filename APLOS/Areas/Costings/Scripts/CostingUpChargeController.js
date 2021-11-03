'use strict';
CostingUpChargeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', 'cboService', '$http', '$filter'];
function CostingUpChargeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, cboService, $http, $filter) {
    $rootScope.title = "Costing Up-Charge Matrix";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.costingCategoryList = [];
    $scope.path = 'Costings/CostingUpCharge/';


    $scope.CostingType = '';
    $scope.CostingTypeList = [];
    //cboService.getEnumCbo("enum/GetCostingTypeEnumCbo", function (result) {
    //    $scope.CostingTypeList = result;
    //});
    cboService.getCostingTypesCbo(function (response) {
        $scope.CostingTypeList = response;
    });
    $scope.UpchargeListMatrix = [];

    $scope.GetCostingTypeComponent = function () {
        $scope.LoadData();
    }

    $scope.LoadData = function () {

        try {

            $scope.UpchargeListMatrix = [];
            if ($scope.CostingType == null || $scope.CostingType == "")
                return;
            $http({

                method: 'POST',
                url: $scope.path + 'getData',
                data: { CostingType: $scope.CostingType },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    for (var i = 0; i < response.data.length; i++) {
                        if (response.data[i].Basic <= 0)
                            response.data[i].Basic = '';

                        if (response.data[i].SemiCritical <= 0)
                            response.data[i].SemiCritical = '';

                        if (response.data[i].Critical <= 0)
                            response.data[i].Critical = '';

                        if (response.data[i].HighlyCritical <= 0)
                            response.data[i].HighlyCritical = '';

                    }
                    $scope.UpchargeListMatrix = response.data;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    $scope.Save = function () {
        try {
           
            if ($scope.CostingType == null || $scope.CostingType == "")
                throw 'Please select costing type';

            $http({

                method: 'POST',
                url: $scope.path + 'UpdateData',
                data: { 'MatrixData': $scope.UpchargeListMatrix, 'CostingType': $scope.CostingType },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
}