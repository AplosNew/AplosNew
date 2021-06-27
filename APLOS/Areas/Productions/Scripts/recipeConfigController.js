'use strict';
recipeConfigController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function recipeConfigController($window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Recipe Configuration";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.modelList = [];
    $scope.path = 'Productions/RecipeConfig/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.model = {
        Id: null
        , CompanyGroupId: $window.companyGroupId
        , CompanyId: null
        , PlantId: null
        , ProcessId: null
        , ProcessName: null
        , OutputDependAttributeId: null
        , OutputDependAttribute: null
        , OutputDependCharacteristicssId: null
        , OutputDependCharacteristics: null
        , OutputDependSubprocessId: null
        , OutputDependSubprocess: null
        , OutPutUoMId: null
        , OutPutUoM: null
        , OutputLevel: 'CH'
        , RawMaterialConsumptionAattributeId: null
        , RawMaterialConsumptionAattribute: null
        , RawMaterialConsumptionCharacteristicssId: null
        , RawMaterialConsumptionCharacteristics: null
        , RmConsumptionUoMId: null
        , RmConsumptionUoM: null
        , ConsumptionLevel: 'CH'
        , RecipeDependAttributeId: null
        , RecipeDependAttribute: null
        , RecipeDependCharacteristicsId: null
        , RecipeDependCharacteristics: null
        , RecipeDependonSubprocessId: null
        , RecipeDependonSubprocess: null
        , RecipeLevel: 'CH'
        , SpecificationAttributeId1: null
        , SpecificationAttribute: null
        , SpecificationCharacteristicId1: null
        , SpecificationCharacteristics: null
        , SpecificationLevel1: 'CH'
        , SpecificationAttributeId2: null
        , SpecificationAttribute: null
        , SpecificationCharacteristicId2: null
        , SpecificationCharacteristics: null
        , SpecificationLevel2: 'CH'
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.searchByList = [
        {
            'value': 'ProcessName'
            , 'name': 'Process'
        },
        {
            'value': 'OutputDependAttribute'
            , 'name': 'Output Depend Attribute'
        },
        {
            'value': 'OutputDependCharacteristics'
            , 'name': 'Output Depend Characteristics'
        },
        {
            'value': 'OutputDependSubprocess'
            , 'name': 'Output Depend Subprocess'
        },
        {
            'value': 'RawMaterialConsumptionAattribute'
            , 'name': 'RawMaterial Consumption Aattribute'
        },
        {
            'value': 'RawMaterialConsumptionCharacteristics'
            , 'name': 'RawMaterial Consumption Characteristics'
        },
        {
            'value': 'RecipeDependAttribute'
            , 'name': 'Recipe Depend Attribute'
        },
        {
            'value': 'RecipeDependCharacteristics'
            , 'name': 'Recipe Depend Characteristics'
        },
        {
            'value': 'RecipeDependonSubprocess'
            , 'name': 'Recipe Dependon Subprocess'
        },
    ];

    $scope.getDataList = function () {
        baseService.init($scope.getListUrl, null, null, null, 'ProcessName', 'ProcessName');
        $rootScope.parameters.plantId = $scope.modelNew.PlantId;
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.modelList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    // #region ddl
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.getCboPlantList = function () {
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

    $scope.attributeList = [];
    $http({
        method: 'GET',
        url: 'Materials/materialattribute/getcbo',
        params: { 'valueAssignment': 'G' }
    }).then(function successCallback(response) {
        $scope.attributeList = response.data;
    });

    $scope.characteristicList = [];
    $http({
        method: 'GET',
        url: 'materials/characteristics/getcbo',
        params: { 'valueAssignment': 'G' }
    }).then(function successCallback(response) {
        $scope.characteristicList = response.data;
    });

    $scope.subProcessList = [];
    $scope.getProcessList = function () {
        $http({
            method: 'GET',
            url: 'processes/subprocess/getcbo?processId=' + $scope.modelNew.ProcessId
        }).then(function successCallback(response) {
            $scope.subProcessList = response.data.Rows;
        });
    }

    $scope.uomList = [];
    $http({
        method: 'GET',
        url: 'Setups/UnitOfMeasurement/getcbo',
    }).then(function successCallback(response) {
        $scope.uomList = response.data;
    });

    // #endregion ddl

    $scope.Get = function (data,index) {
        $scope.index = index;
        $scope.model = $scope.modelList[$scope.index];
        $scope.modelNew = Object.assign({}, $scope.model);
        $scope.modelNew = data;
        $scope.getProcessList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
           

            if ($scope.modelForm.$valid) {

                if ($scope.modelNew.OutputLevel === 'CH' && baseService.isUndefinedOrNull($scope.modelNew.OutputDependCharacteristicsId)) {
                    throw "Output Depend On Characteristics is required.";
                }

                if ($scope.modelNew.OutputLevel === 'AR' && baseService.isUndefinedOrNull($scope.modelNew.OutputDependAttributeId)) {
                    throw "Output Depend On Attribute is required.";
                }

                if ($scope.modelNew.RecipeLevel === 'CH' && baseService.isUndefinedOrNull($scope.modelNew.RecipeDependCharacteristicsId)) {
                    throw "Recipe Depend On Characteristics is required.";
                }

                if ($scope.modelNew.RecipeLevel === 'AR' && baseService.isUndefinedOrNull($scope.modelNew.RecipeDependAttributeId)) {
                    throw "Recipe Depend On Attribute is required.";
                }

                if (!baseService.isUndefinedOrNull($scope.modelNew.SpecificationAttributeId1) && !baseService.isUndefinedOrNull($scope.modelNew.SpecificationAttributeId2)) {
                    if ($scope.modelNew.SpecificationAttributeId1 === $scope.modelNew.SpecificationAttributeId2) {
                       throw "Attribute can't be same.";
                    }
                }

                if (!baseService.isUndefinedOrNull($scope.modelNew.SpecificationCharacteristicId1) && !baseService.isUndefinedOrNull($scope.modelNew.SpecificationCharacteristicId2)) {
                    if ($scope.modelNew.SpecificationCharacteristicId1 === $scope.modelNew.SpecificationCharacteristicId2) {
                        throw "Characteristic can't be same.";
                    }
                }
                //angular.copy($scope.modelNew, $scope.model);

                if ($scope.Action == "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.model,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getDataList();
                            baseService.paginationAdd();
                            ClearFields();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
                else if ($scope.Action == "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.model,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getDataList();
                            ClearFields();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.modelList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    }

    $scope.Clear = function () {
        ClearFields();
        $scope.modelNew.CompanyId = null;
        $scope.modelNew.PlantId = null;
        return true;
    }

    function ClearFields() {
        $scope.Action = "Save";
        $scope.model = {};
        $scope.modelNew = {
            CompanyGroupId: $window.companyGroupId
            , CompanyId: $scope.modelNew.CompanyId
            , PlantId: $scope.modelNew.PlantId
            , OutputLevel: 'CH'
            , ConsumptionLevel: 'CH'
            , RecipeLevel: 'CH'
            , SpecificationLevel1: 'CH'
            , SpecificationLevel2: 'CH'
        };
    }

    $scope.clearLevel = function (fieldName) {
        //if ($scope.modelNew.RecipeLevel === 'CH') {
        //    $scope.modelNew.OutputDependAttributeId = null;
        //    $scope.modelNew.RawMaterialConsumptionAattributeId = null;
        //    $scope.modelNew.RecipeDependAttributeId = null;
        //}
        //else {
        //    $scope.modelNew.OutputDependCharacteristicsId = null;
        //    $scope.modelNew.RawMaterialConsumptionCharacteristicsId = null;
        //    $scope.modelNew.RecipeDependCharacteristicsId = null;
        //}
        $scope.modelNew[fieldName] = null;
    }
}