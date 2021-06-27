'use strict';
function ProcessConfigurationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "processConfig Configuration";
    $scope.Action = 'Save';
    $scope.tableShow = false;
    $scope.processConfigMessage = false;
    $scope.delBtn = true;
    $scope.index = -1;
    $scope.processConfigs = [];
    $scope.path = 'Processes/processconfig/';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.getData = function (id) {
        $http({
            method: 'GET',
            url: 'Processes/processconfig/getlist?materialMasterId=' + id
        }).then(function successCallback(response) {
            //result[0].MaterialGridId = null;
            if (response.data.length > 0) {
                $scope.processConfigs = response.data;
                //setDefaultProcess($scope.processConfigs);
                $scope.processConfigMessage = false;
                $scope.tableShow = true;
                if (response.data[0].Id != null) {
                    $scope.Action = 'Update';
                    $scope.delBtn = false;
                }
                if (response.data[0].MaterialGridId == null) {
                    angular.forEach($scope.levelList, function (obj, i) {
                        if (obj.Value == "Grid") {
                            obj.disabled = true;
                            characteristics();
                        }
                    })
                }
                else {
                    $scope.getCharacteristicsName(id);
                    angular.forEach($scope.levelList, function (obj, i) {
                        if (obj.Value == "Grid") {
                            obj.disabled = false;
                            characteristics();
                        }
                    })
                }
            }
            else {
                $scope.tableShow = false;
                $scope.materialGrid = null;
                $scope.processConfigMessage = true;
            }
        });
    };

    $scope.getCharacteristicsName = function (id) {
        $http({
            method: 'GET',
            url: 'Processes/processconfig/getcharacteristicsname?materialMasterId=' + id
        }).then(function successCallback(response) {
            characteristics();
            for (var i = 0; i <= response.data.length - 1; i++) {
                if (response.data[i].Sort == '1') {
                    $scope.Characteristics1 = response.data[i].Characteristics;
                    $scope.Characteristics1Id = response.data[i].Id;
                    $scope.ch1 = true;
                }
                if (response.data[i].Sort == '2') {
                    $scope.Characteristics2 = response.data[i].Characteristics;
                    $scope.Characteristics2Id = response.data[i].Id;
                    $scope.ch2 = true;
                }

                if (response.data[i].Sort == '3') {
                    $scope.Characteristics3 = response.data[i].Characteristics;
                    $scope.Characteristics3Id = response.data[i].Id;
                    $scope.ch3 = true;
                }
            }
        });
    };
    $scope.processConfig = {
        Id: null,
        CompanyGroupId: null,
        MaterialMasterId: null,
        ProcessId: null,
        DefaultPlanning: false,
        Days: null,
        BomOrRecipe: null,
        MaterialTaggingType: null,
        Level: null,
        Symbol: null,
        Characteristics1Id: null,
        Characteristics2Id: null,
        Characteristics3Id: null,
        Active: true
    };
    $scope.processConfigNew = Object.assign({}, $scope.processConfig);

    $scope.materialMasterView = {
        Id: null,
        UserName: null,
        Code: null,
        Description: null,
        MaterialType: null,
        MaterialGroup: null,
        MaterialGrid: null,
        BaseUom: null
    };

    //Function
    $scope.setPlusOrMinus = function (event, index) {
        for (var i = 0; i <= $scope.processConfigs.length - 1; i++) {
            if (i < index) {
                $scope.processConfigs[i].Symbol = '-';
                $scope.processConfigs[i].DefaultPlanning = false;
            }
            else if (i > index) {
                $scope.processConfigs[i].Symbol = '+';
                $scope.processConfigs[i].DefaultPlanning = false;
            }
            else if (i == index) {
                $scope.processConfigs[i].Symbol = null;
                $scope.processConfigs[i].Days = 0;
                $scope.processConfigs[i].DefaultPlanning = true;
            }
        }
    }

    //Function End

    // #region Enum Dropdown
    $scope.bomOrRecipeList = [];
    $http({
        method: 'GET',
        url: 'Processes/processconfig/GetProcessConfigBomOrRecipeCbo'
    }).then(function successCallback(response) {
        $scope.bomOrRecipeList = response.data;
    });
    $scope.levelList = [];
    $http({
        method: 'GET',
        url: 'Processes/processconfig/GetProcessConfigLevelCbo'
    }).then(function successCallback(response) {
        $scope.levelList = response.data;
    });
    $scope.materialTaggingTypeList = [];
    $http({
        method: 'GET',
        url: 'Processes/processconfig/GetProcessConfigMaterialTaggingTypeCbo'
    }).then(function successCallback(response) {
        $scope.materialTaggingTypeList = response.data;
    });
    // #endregion

    // #region Material Master
    $scope.materialMasterList = [];
    $scope.materialMasterParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.materialMst = 'Material Master';
    $scope.materialMasterPopUp = function () {
        $scope.materialMasterUrl = 'Materials/materialmaster/materialmastersearch';
        $scope.getMaterialMasterData = function (pageno) {
            baseService.paginationBase($scope.materialMasterUrl, pageno, $scope.materialMasterParameters)
                .then(function (result) {
                    $scope.materialMasterDataList = result.Rows;
                    $scope.materialMasterParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.materialMasterList) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.materialMasterList);
                        //$scope.materialMasterList = localValue;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#materialMasterPopUp')).modal('show');
        $scope.getMaterialMasterData();
    }
    $scope.selectMaterialMaster = function (data) {
        $scope.processConfigNew.MaterialMasterId = data.Id;
        $scope.materialMasterView.Id = data.Id;
        $scope.materialMasterView.UserName = data.UserName;
        $scope.materialMasterView.Code = data.Code;
        $scope.materialMasterView.Description = data.Description;
        $scope.materialMasterView.MaterialType = data.MaterialType;
        $scope.materialMasterView.MaterialGroup = data.MaterialGroupMaster;
        $scope.materialMasterView.MaterialGrid = data.GridName;
        $scope.materialMasterView.BaseUom = data.BaseUom;
        $scope.getData($scope.processConfigNew.MaterialMasterId);
        angular.element(document.querySelector('#materialMasterPopUp')).modal('hide');
    }
    $scope.valueData = '';
    $scope.SelectMMaster = function (data) {
        $scope.valueData = data;
    }
    $scope.SelectMMasterByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            ShowResult('Please at first select row', 'failure', 'materialMasterPopUp');
            return;
        }
        $scope.selectMaterialMaster($scope.valueData);
        $scope.valueData = '';
        angular.element(document.querySelector('#materialMasterPopUp')).modal('hide');
    }
    $scope.closematerialMasterPopUp = function () {
        angular.element(document.querySelector('#materialMasterPopUp')).modal('hide');
    }
    // #end region

    $scope.Save = function () {
        try {
            for (var i = 0; i < $scope.processConfigs.length; i++) {
                if ($scope.processConfigs[i].Characteristics1Selected == true)
                    $scope.processConfigs[i].Characteristics1Id = $scope.Characteristics1Id;
                else
                    $scope.processConfigs[i].Characteristics1Id = null;
                if ($scope.processConfigs[i].Characteristics2Selected == true)
                    $scope.processConfigs[i].Characteristics2Id = $scope.Characteristics2Id;
                else
                    $scope.processConfigs[i].Characteristics2Id = null;
                if ($scope.processConfigs[i].Characteristics3Selected == true)
                    $scope.processConfigs[i].Characteristics3Id = $scope.Characteristics3Id;
                else
                    $scope.processConfigs[i].Characteristics3Id = null;
            }
            //checkDaysValueNotNull($scope.processConfigs);
            //checkDefaultProcess($scope.processConfigs);
            daysSortValidation($scope.processConfigs);
            isLevelSeleceted($scope.processConfigs);
            //angular.copy($scope.processConfigNew, $scope.processConfigNew);
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: $scope.processConfigs,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getData($scope.processConfigNew.MaterialMasterId);
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.processConfigs)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl,
                data: $scope.processConfigs,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getData($scope.processConfigNew.MaterialMasterId);
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    }
    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.processConfig = {};
        $scope.processConfigNew = {};
        $scope.MaterialMasterName = '';
        $scope.materialGrid = '';
        $scope.processConfigs = [];
        $scope.tableShow = false;
        $scope.delBtn = true;
        $scope.selectMaterialMaster = {};
    }
    function ClearFields() {
        $scope.Action = "Save";
        $scope.processConfig = {};
        $scope.processConfigNew = { MaterialMasterId: $scope.processConfigNew.MaterialMasterId };
        $scope.delBtn = true;
    }

    // #region Common Function
    function checkDaysValueNotNull(list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Days == '') {
                throw 'Can not save without days';
            }
        }
    }
    function daysSortValidation(list) {
        try {
            var seq = 0;
            var seqNeg = 0;
            var isNeg = true;
            if (list[0].Days == 0) {
                isNeg = false;
            } else {
                seqNeg = parseInt(list[0].Days);
                seqNeg += 1;
            }
            for (var i = 0; i < list.length; i++) {
                if (isNeg == false) {//0,1,2
                    if (list[i].Days >= seq) {
                        seq = list[i].Days;
                    }
                    else//0,1,3,2
                        throw "aaaaa";
                }
                else //2,1,0,1,2 or2,1,0
                {
                    if (list[i].Days <= seqNeg) {//2,1,0
                        seqNeg = list[i].Days;
                        if (list[i].Days == 0) {
                            isNeg = false;
                            seq = 0;
                        }
                    }
                    else {
                        //2,3,1,0,1,2
                        throw "bbbb";
                    }
                }
            }
        } catch (e) {
            throw e;
        }
    }
    //function checkDefaultProcess(list) {
    //    try {
    //        for (var i = 0; i <= list.length; i++) {
    //            var d = list[i].Days;
    //            var dp = list[i].DefaultProcess;
    //            if (d == 0 && dp == true)
    //                break;
    //            if (d == 0 && dp == false)
    //                throw 'First lag days 0 set default for planning...!';
    //        }
    //    } catch (e) {
    //        throw e;
    //    }
    //}

    //function setDefaultProcess(list) {
    //    for (var i = 0; i < list.length; i++) {
    //        if (list[i].Days == 0)//DefaultPlanning
    //        {
    //            list[i].DefaultPlanning = true;
    //            break;
    //        }
    //    }
    //}
    function isLevelSeleceted(list) {
        try {
            for (var i = 0; i < list.length; i++) {
                if (list[i].Level == 'Grid' && list[i].Characteristics1Selected == false
                    && list[i].Characteristics2Selected == false && list[i].Characteristics3Selected == false) {
                    throw 'Since grid level selected, please select at least 1 characteristics...';
                }
                else if (list[i].Level != 'Grid' && (list[i].Characteristics1Selected == true
                    || list[i].Characteristics2Selected == true || list[i].Characteristics3Selected == true)) {
                    throw 'Since characteristics selected, please select grid level...';
                }
            }
        } catch (e) {
            throw e;
        }
    }
    function characteristics() {
        $scope.ch1 = false;
        $scope.ch2 = false;
        $scope.ch3 = false;
        $scope.Characteristics1 = null;
        $scope.Characteristics1Id = null;
        $scope.Characteristics2 = null;
        $scope.Characteristics2Id = null;
        $scope.Characteristics3 = null;
        $scope.Characteristics3Id = null;
    }
    // #endregion
}
ProcessConfigurationController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];