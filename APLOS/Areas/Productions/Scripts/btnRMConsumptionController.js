'use strict';
btnRMConsumptionController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function btnRMConsumptionController($window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "RM Comsumption";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.modelList = [];
    $scope.path = 'Productions/ButtonConfig/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'CreateRMConsumption';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.model = {
        Id: null
        , CompanyGroupId: $window.companyGroupId
        , CompanyId: null
        , PlantId: null
        , ProcessId: null
        , ButtonRecipeConfigId: null
        , Name: null
        , RawMaterialConsumptionAattributeId: null
        , RawMaterialConsumptionCharacteristicsId: null

        , RMConsumptionAattributeValueId: null
        , RMConsumptionCharacteristicsValueId: null

        , OutputDependAttributeValueId: null
        , OutputDependCharacteristicsValueId: null

        , UoMValue: null
        , EntryType: 'RMConsumption'
        , ConsumptionLevel: 'CH'
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.getDataList = function () {
        $http({
            method: 'GET',
            url: $scope.getListUrl + '?plantId=' + $scope.modelNew.PlantId + '&processId=' + $scope.modelNew.ProcessId + '&entryType=RMConsumption'
        }).then(function successCallback(response) {
            $scope.modelList = response.data.Rows;
        });
    };

    // #region ddl
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.getCboPlantList = function () {
        $scope.Action = "Save";
        $scope.model = {};
        $scope.modelNew = {
            CompanyGroupId: $window.companyGroupId
            , CompanyId: $scope.modelNew.CompanyId
        };
        $scope.error1 = null;
        $scope.error2 = null;
        $scope.modelList = [];
        cboService.getCboPlantByCompany($scope.modelNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };

    $scope.processList = [];
    $http({
        method: 'GET',
        url: 'Processes/process/getcbo'
    }).then(function successCallback(response) {
        $scope.processList = response.data;
    });

    $scope.getAttributeValueList = function () {
        $scope.attrValueList = [];
        $scope.charValueList = [];
        $scope.error1 = null;
        $scope.error2 = null;
        $scope.modelList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'GetHeaderList?plantId=' + $scope.modelNew.PlantId + '&processId=' + $scope.modelNew.ProcessId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.modelNew.ButtonRecipeConfigId = response.data[0].Id;
                $scope.modelNew.RawMaterialConsumptionAattributeId = response.data[0].RawMaterialConsumptionAattributeId;
                $scope.modelNew.RawMaterialConsumptionAattribute = response.data[0].RawMaterialConsumptionAattribute;
                $scope.modelNew.RawMaterialConsumptionCharacteristicsId = response.data[0].RawMaterialConsumptionCharacteristicsId;
                $scope.modelNew.RawMaterialConsumptionCharacteristics = response.data[0].RawMaterialConsumptionCharacteristics;
                $scope.modelNew.Name = baseService.isUndefinedOrNull($scope.modelNew.RawMaterialConsumptionCharacteristicsId) === false ? $scope.modelNew.RawMaterialConsumptionCharacteristics : $scope.modelNew.RawMaterialConsumptionAattribute;
                $scope.modelNew.RmConsumptionUoM = response.data[0].RmConsumptionUoM;
                $scope.modelNew.ConsumptionLevel = response.data[0].ConsumptionLevel;
                $http({
                    method: 'GET',
                    url: 'Materials/MaterialAttributeValue/GetCbo?attributeId=' + $scope.modelNew.RawMaterialConsumptionAattributeId
                }).then(function successCallback(response) {
                    $scope.attrValueList = response.data;
                });

                $http({
                    method: 'GET',
                    url: 'materials/characteristicsvalue/GetCbo?characteristicsId=' + $scope.modelNew.RawMaterialConsumptionCharacteristicsId
                }).then(function successCallback(response) {
                    $scope.charValueList = response.data;
                });
                $scope.getDataList();
            }
        });
    };

    // #endregion ddl

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        angular.copy($scope.modelNew, $scope.model);
        $http({
            method: 'POST'
            , url: $scope.saveUrl
            , data: $scope.modelList
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ClearFields();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.valuePassInDelModal = function (id, index) {
        $scope.index = index;
        $scope.id = id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        $scope.modelList.splice($scope.index, 1);
        $scope.index = -1;
        $scope.id = null;
    }

    $scope.Clear = function () {
        $scope.ClearFields();
        return true;
    }
    $scope.ClearFields = function () {
        $scope.Action = "Save";
        $scope.model = {};
        $scope.modelNew = {
            CompanyGroupId: $window.companyGroupId
            , CompanyId: $scope.modelNew.CompanyId
            , PlantId: $scope.modelNew.PlantId
        };
        $scope.error1 = null;
        $scope.error2 = null;
        $scope.modelList = [];
    };

    $scope.error1 = null;
    $scope.error2 = null;

    $scope.add = function () {
        $scope.error1 = null;
        $scope.error2 = null;
        if ($scope.modelNew.ConsumptionLevel === 'CH') {
            if (baseService.isUndefinedOrNull($scope.modelNew.RMConsumptionCharacteristicsValueId))
                return $scope.error1 = $scope.modelNew.Name + ' value is required.';
            if (baseService.valueCheckInList($scope.modelList, 'RMConsumptionCharacteristicsValueId', $scope.modelNew.RMConsumptionCharacteristicsValueId))
                return $scope.error1 = $scope.modelNew.Name + ' value is already exist.';
        }
        if ($scope.modelNew.ConsumptionLevel === 'AR') {
            if (baseService.isUndefinedOrNull($scope.modelNew.RMConsumptionAattributeValueId))
                return $scope.error1 = $scope.modelNew.Name + ' value is required.';
            if (baseService.valueCheckInList($scope.modelList, 'RMConsumptionAattributeValueId', $scope.modelNew.RMConsumptionAattributeValueId))
                return $scope.error1 = $scope.modelNew.Name + ' value is already exist.';
        }

        if (isNaN($scope.modelNew.UoMValue))
            return $scope.error2 = $scope.modelNew.RmConsumptionUoM + ' value is required.'
        //if (baseService.valueCheckInList($scope.modelList, 'UoMValue', $scope.modelNew.UoMValue))
        //    return $scope.error2 = $scope.modelNew.Name + ' value is already exist.';


        $scope.modelList.push({
            Id: null
            , CompanyGroupId: $window.companyGroupId
            , CompanyId: $scope.modelNew.CompanyId
            , PlantId: $scope.modelNew.PlantId
            , ProcessId: $scope.modelNew.ProcessId
            , ButtonRecipeConfigId: $scope.modelNew.ButtonRecipeConfigId
            , RMConsumptionAattributeValueId: $scope.modelNew.RMConsumptionAattributeValueId
            , RMConsumptionCharacteristicsValueId: $scope.modelNew.RMConsumptionCharacteristicsValueId
            , RmName: $scope.modelNew.ConsumptionLevel === 'CH' ? angular.element("#chId :selected").text() : angular.element("#attrId :selected").text()
            , UoMValue: $scope.modelNew.UoMValue
            , OutputDependAttributeValueId: null
            , EntryType: 'RMConsumption'
            , IfChange: false
        });
        $scope.modelNew.RMConsumptionAattributeValueId = null;
        $scope.modelNew.RMConsumptionCharacteristicsValueId = null;
    };

}