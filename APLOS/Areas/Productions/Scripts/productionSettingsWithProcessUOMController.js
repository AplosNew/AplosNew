'use strict';
ProductionSettingsWithProcessUOMController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProductionSettingsWithProcessUOMController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "UoM Settings";
    $scope.Action = "Save";
    $scope.path = 'Productions/productionsettingswithprocessuom/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete';

    // #region common ddl
    $scope.plantId = null;
    $scope.plantList = [];
    $http({
        method: 'GET',
        url: 'Organizations/plant/getcbo'
    }).then(function successCallback(response) {
        $scope.plantList = response.data;
    });
    $scope.processList = [];
    $http({
        method: 'GET',
        url: 'Processes/process/getcbo'
    }).then(function successCallback(response) {
        $scope.processList = response.data;
    });
    $scope.uomList = [];
    $http({
        method: 'GET',
        url: 'Setups/unitofmeasurement/getcbo'
    }).then(function successCallback(response) {
        $scope.uomList = response.data;
    });
    $scope.bomOrRecipeList = [];
    $http({
        method: 'GET',
        url: 'Processes/processconfig/GetProcessConfigBomOrRecipeCbo'
    }).then(function successCallback(response) {
        $scope.bomOrRecipeList = response.data;
        $scope.bomOrRecipeList.push({
            Value: 'BOM',
            Text: 'BOM'
        });
    });
    // #endregion

    // #region production settings
    $scope.productionSettings = {
        Id: null,
        PlantId: null,
        NeoclearProcessId: null,
        BomOrRecipe: null,
        IsMultipleOrderAllowedInBatch: false
    };
    $scope.productionSettingsDataGet = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'productionsettingsgetlist?plantId=' + $scope.plantId
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.productionSettings = {};
                $scope.productionSettings = response.data[0];
                //$scope.productionSettings.Id = result[0].Id;
                //$scope.productionSettings.PlantId = result[0].PlantId;
                $scope.Action = "Update";
            }
            else {
                $scope.productionSettings = {};
                $scope.Action = "Save";
            }
        });
    }
    // #endregion

    // #region ProcessCapacityUOM
    $scope.psUoM = 'Add Line Item';
    $scope.processUomIndex = -1;
    $scope.processUomList = [];
    $scope.processUomTableShow = false;
    $scope.processUom = {
        Id: null,
        PlantId: null,
        ProcessId: null,
        ProcessName: null,
        CapacityUOMId: null,
        CapacityUOM: null,
        UOM1Id: null,
        UOM1: null,
        UOM2Id: null,
        UOM2: null
    };
    $scope.processUomNew = Object.assign({}, $scope.processUom);
    $scope.processUomDataGet = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'processcapacityuomgetlist?plantId=' + $scope.plantId
        }).then(function successCallback(response) {
            $scope.processUomList = response.data;
            if ($scope.processUomList.length > 0)
                $scope.processUomTableShow = true;
            else
                $scope.processUomTableShow = false;
        });
    }
    $scope.processUoMPopUp = function () {
        if ($scope.plantId === null)
            ShowResult('Please select plant......!', 'failure', 'processUoMPopUp');
        angular.element(document.querySelector('#processUoMPopUp')).modal('show');
    }
    $scope.processUoMEdit = function (data) {
        $scope.processUom = data;
        $scope.processUomNew = Object.assign({}, $scope.processUom);
        for (var i = 0; i < $scope.processUomList.length; i++) {
            if ($scope.processUomList[i].Id == data.Id) {
                $scope.processUomIndex = i;
            }
        }
        $scope.psUoM = 'Update Line Item';
        $scope.processUoMPopUp();
    }
    $scope.closeProcessPopUp = function () {
        angular.element(document.querySelector('#processUoMPopUp')).modal('hide');
    };
    $scope.clearProcessUoM = function () {
        $scope.processUom = {};
        $scope.processUomNew = {};
        $scope.processUomIndex = -1;
        $scope.psUoM = 'Add Line Item';
    };
    $scope.addProcessUoM = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.processUomNew.UOM1Id) && baseService.isUndefinedOrNull($scope.processUomNew.UOM2Id))
                throw 'Please select 1st UoM or 2nd UoM or both......!';
            for (var i = 0; i < $scope.processUomList.length; i++) {
                if ($scope.processUomList[i].ProcessId == $scope.processUomNew.ProcessId
                    && !$scope.processUomList[i].Archive
                    && $scope.processUomList[$scope.processUomIndex] != $scope.processUomList[i]) {
                    throw 'This process already exist in grid......!';
                }
            }
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.processUoMForm.$valid) {
                $scope.processId = document.getElementById("processId").options[document.getElementById('processId').selectedIndex].text;
                $scope.capacityUOMId = document.getElementById("capacityUOMId").options[document.getElementById('capacityUOMId').selectedIndex].text;
                $scope.uoM1Id = document.getElementById("uoM1Id").options[document.getElementById('uoM1Id').selectedIndex].text;
                $scope.uoM2Id = document.getElementById("uoM2Id").options[document.getElementById('uoM2Id').selectedIndex].text;
                angular.copy($scope.processUomNew, $scope.processUom);
                if (baseService.isUndefinedOrNull($scope.processUomNew.Id)) {
                    $scope.processUomList.push({
                        Id: $scope.createId(),
                        PlantId: $scope.plantId,
                        ProcessId: $scope.processUom.ProcessId,
                        ProcessName: $scope.processId,
                        CapacityUOMId: $scope.processUom.CapacityUOMId,
                        CapacityUOM: $scope.capacityUOMId,
                        UOM1Id: $scope.processUom.UOM1Id,
                        UOM1: $scope.uoM1Id,
                        UOM2Id: $scope.processUom.UOM2Id,
                        UOM2: $scope.uoM2Id,
                        Archive: false,
                        'class': 'new'
                    });
                }
                else {
                    $scope.processUomList[$scope.processUomIndex].ProcessName = $scope.processId;
                    $scope.processUomList[$scope.processUomIndex].CapacityUOM = $scope.capacityUOMId;
                    $scope.processUomList[$scope.processUomIndex].UOM1 = $scope.uoM1Id;
                    $scope.processUomList[$scope.processUomIndex].UOM2 = $scope.uoM2Id;
                    $scope.processUomList[$scope.processUomIndex] = $scope.processUom;
                }
                if (!$scope.processUomTableShow)
                    $scope.processUomTableShow = true;
                $scope.clearProcessUoM();
                $scope.closeProcessPopUp();
            }
        } catch (e) {
            ShowResult(e, 'failure', 'processUoMPopUp');
        }
    };
    $scope.createId = function () {
        return 'new' + Math.floor(Math.random() * 900000) + 100000;
    };

    $scope.processUoMDel = function (data) {
        $scope.psmessage_confirmation = '';
        $scope.Id = data.Id;
        $scope.psmessage_confirmation = 'Are you sure want to delete..........?';
        angular.element(document.querySelector('#confirmPSPopUp')).modal('show');
    };
    $scope.psDeleteRow = function () {
        for (var i = 0; i < $scope.processUomList.length; i++) {
            if ($scope.processUomList[i].Id == $scope.Id && $scope.Id.startsWith('new')) {
                $scope.processUomList.splice(i, 1);
            }
            else if ($scope.processUomList[i].Id != null && $scope.processUomList[i].Id == $scope.Id)
                $scope.processUomList[i].Archive = true;
        }
        if ($scope.processUomList.length > 0) {
            $scope.processUomTableShow = true;
        }
        else {
            $scope.processUomTableShow = false;
        }
    };
    // #endregion

    // #region CRUD
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.productionSettings.NeoclearProcessId != null && $scope.productionSettings.BomOrRecipe == null) {
            ShowResult('Please select bom or recioe.......!', 'failure', 'processUoMPopUp');
        }
        for (var i = 0; i < $scope.processUomList.length; i++) {
            if ($scope.processUomList[i].Id.startsWith('new')) {
                $scope.processUomList[i].Id = null;
            }
        }
        if ($scope.Action == "Save") {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data:
                {
                    'productionSettings': $scope.productionSettings,
                    'processCapacityUOM': $scope.processUomList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.productionSettings = response.data.ProductionSettings;
                    $scope.processUomDataGet();
                    $scope.clearProcessUoM();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        else if ($scope.Action == "Update") {
            $http({
                method: 'POST',
                url: $scope.updateUrl,
                data:
                {
                    'productionSettings': $scope.productionSettings,
                    'processCapacityUOM': $scope.processUomList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.productionSettings = response.data.ProductionSettings;
                    $scope.processUomDataGet();
                    $scope.clearProcessUoM();
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.plantId)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.plantId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.Clear = function () {
        ClearFields();
        $scope.plantId = null;
        $scope.productionSettings = {};
        $scope.productionSettings.IsMultipleOrderAllowedInBatch = false;
        $scope.clearProcessUoM();
        $scope.processUomList = [];
        $scope.processUomTableShow = false;
    };
    function ClearFields() {
        $scope.Action = "Save";
    }
    // #endregion

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}