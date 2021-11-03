'use strict';
btnOutputController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function btnOutputController($window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Btn Output";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.modelList = [];
    $scope.path = 'Productions/ButtonConfig/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'CreateOutput';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.model = {
        Id: null
        , CompanyGroupId: $window.companyGroupId
        , CompanyId: null
        , PlantId: null
        , ProcessId: null
        , ButtonRecipeConfigId: null
        , Name: null
        , OutputDependAttributeId: null
        , OutputDependCharacteristicsId: null

        , OutputDependAttributeValueId: null
        , OutputDependCharacteristicsValueId: null

        , UoMValue: null
        , EntryType: 'Output'
        , OutputLevel: 'CH'
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.getDataList = function () {
        $http({
            method: 'GET',
            url: $scope.getListUrl + '?plantId=' + $scope.modelNew.PlantId + '&processId=' + $scope.modelNew.ProcessId + '&entryType=Output'
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
                $scope.modelNew.OutputDependAttributeId = response.data[0].OutputDependAttributeId;
                $scope.modelNew.OutputDependAttribute = response.data[0].OutputDependAttribute;
                $scope.modelNew.OutputDependCharacteristicsId = response.data[0].OutputDependCharacteristicsId;
                $scope.modelNew.OutputDependCharacteristics = response.data[0].OutputDependCharacteristics;
                $scope.modelNew.Name = baseService.isUndefinedOrNull($scope.modelNew.OutputDependCharacteristicsId) === false ? $scope.modelNew.OutputDependCharacteristics : $scope.modelNew.OutputDependAttribute;
                $scope.modelNew.OutPutUoM = response.data[0].OutPutUoM;
                $scope.modelNew.OutputLevel = response.data[0].OutputLevel;
                $http({
                    method: 'GET',
                    url: 'Materials/MaterialAttributeValue/GetCbo?attributeId=' + $scope.modelNew.OutputDependAttributeId
                }).then(function successCallback(response) {
                    $scope.attrValueList = response.data;
                });

                $http({
                    method: 'GET',
                    url: 'materials/characteristicsvalue/GetCbo?characteristicsId=' + $scope.modelNew.OutputDependCharacteristicsId
                }).then(function successCallback(response) {
                    $scope.charValueList = response.data;
                });

                $scope.getDataList();
            }
        });
    };

    // #endregion ddl

    $scope.Save = function () {
        if (baseService.arrayLength($scope.modelList) === 0)
            return ShowResult('Insert value.', 'failure');
        $scope.$broadcast('show-errors-check-validity');
        angular.copy($scope.modelNew, $scope.model);
        $http({
            method: 'POST'
            , url: $scope.saveUrl
            , data: $scope.modelList
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ClearFields();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }

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
                if (response.data.Error === true) {
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
        if ($scope.modelNew.OutputLevel === 'CH') {
            if (baseService.isUndefinedOrNull($scope.modelNew.OutputDependCharacteristicsValueId))
                return $scope.error1 = $scope.modelNew.Name + ' value is required.';
            if (baseService.valueCheckInList($scope.modelList, 'OutputDependCharacteristicsValueId', $scope.modelNew.OutputDependCharacteristicsValueId))
                return $scope.error1 = $scope.modelNew.Name + ' value is already exist.';
        }
        if ($scope.modelNew.OutputLevel === 'AR') {
            if (baseService.isUndefinedOrNull($scope.modelNew.OutputDependAttributeValueId))
                return $scope.error1 = $scope.modelNew.Name + ' value is required.';
            if (baseService.valueCheckInList($scope.modelList, 'OutputDependAttributeValueId', $scope.modelNew.OutputDependAttributeValueId))
                return $scope.error1 = $scope.modelNew.Name + ' value is already exist.';
        }

        if (baseService.isUndefinedOrNull($scope.modelNew.UoMValue) || isNaN($scope.modelNew.UoMValue))
            return $scope.error2 = $scope.modelNew.OutPutUoM + ' value is required.';
        //if (baseService.valueCheckInList($scope.modelList, 'UoMValue', $scope.modelNew.UoMValue))
        //    return $scope.error2 = $scope.modelNew.OutPutUoM + ' value is already exist.';

        $scope.modelList.push({
            Id: null
            , CompanyGroupId: $window.companyGroupId
            , CompanyId: $scope.modelNew.CompanyId
            , PlantId: $scope.modelNew.PlantId
            , ProcessId: $scope.modelNew.ProcessId
            , ButtonRecipeConfigId: $scope.modelNew.ButtonRecipeConfigId
            , RMConsumptionAattributeValueId: null
            , RMConsumptionCharacteristicsValueId: null
            , OutputDependAttributeValueId: $scope.modelNew.OutputDependAttributeValueId
            , OutputDependCharacteristicsValueId: $scope.modelNew.OutputDependCharacteristicsValueId
            , OpName: $scope.modelNew.OutputLevel === 'CH' ? angular.element("#chId :selected").text() : angular.element("#attrId :selected").text()
            , UoMValue: $scope.modelNew.UoMValue
            , EntryType: 'Output'
            , IfChange: false
        });
        $scope.modelNew.OutputDependAttributeValueId = null;
        $scope.modelNew.OutputDependCharacteristicsValueId = null;
    };
}