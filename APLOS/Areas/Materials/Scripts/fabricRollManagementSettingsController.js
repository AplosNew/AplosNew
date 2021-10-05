'use strict';
fabricRollManagementSettingsController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function fabricRollManagementSettingsController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Fabric Roll Management";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.models = [];
    $scope.path = 'Materials/fabricrollmanagementsettings/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.GetFabricList = function () {
        baseService.init($scope.getListUrl, null, null, null, 'Code, UserName', 'UserName');
        $scope.getData = function (pageno) {
            var tempParam = [];
            tempParam.push($scope.searchModel.Code);
            tempParam.push($scope.searchModel.ShortName);
            tempParam.push($scope.searchModel.StandardName);
            tempParam.push($scope.searchModel.UserName);
            tempParam.push($scope.searchModel.BaseUoM);
            tempParam.push($scope.searchModel.Ch1);
            tempParam.push($scope.searchModel.Ch2);
            tempParam.push($scope.searchModel.Ch3);
            $rootScope.parameters.tempParam = JSON.stringify(tempParam);
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.buyerStyles = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
        angular.element(document.querySelector('#fabricId')).modal('show');
    }
    $scope.model = {
        Id: null
        , MaterialMasterId: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , BaseUoM: null
        , BlanketLengthBeforeWash: 0
        , BlanketWidthBeforeWash: 0
        , Characteristics1Id: null
        , Characteristics1Name: null
        , Characteristics2Id: null
        , Characteristics2Name: null
        , Characteristics3Id: null
        , Characteristics3Name: null
        , IsDimension1: false
        , IsDimension2: false
        , IsDimension3: false
        , IsBlanketDefaultLengthValuesChangeable: false
        , IsBlanketDefaultWidthValuesChangeable: false
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.searchModel = {
        Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , BaseUoM: null
        , Ch1: null
        , Ch2: null
        , Ch3: null
    };

    $scope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.model = $scope.buyerStyles[$scope.index];
        $scope.modelNew = Object.assign({}, $scope.model);
        GetCharacteristicsList(false);
        $scope.Action = "Update";
        angular.element(document.querySelector('#fabricId')).modal('hide');
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            if (baseService.arrayLength($scope.characteristicsList) > 0) {
                if (!checkDimension())
                    return ShowResult('Please check at least one sku.', "failure");
            }
            proertiesNull($scope.modelNew, 'IsDimension1', 'Characteristics1Id');
            proertiesNull($scope.modelNew, 'IsDimension2', 'Characteristics2Id');
            proertiesNull($scope.modelNew, 'IsDimension3', 'Characteristics3Id');
            console.log($scope.modelNew);
            angular.copy($scope.modelNew, $scope.model);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.model,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, "failure");
                    else {
                        ShowResult(response.data.Message, "success");
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.model,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, "failure");
                    else {
                        ShowResult(response.data.Message, "success");
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }
    }
    function checkDimension() {
        var flag = false;
        if (!baseService.isUndefinedOrNull($scope.modelNew.Characteristics1Id) && $scope.modelNew.IsDimension1) flag = true;
        if (!baseService.isUndefinedOrNull($scope.modelNew.Characteristics2Id) && $scope.modelNew.IsDimension2) flag = true;
        if (!baseService.isUndefinedOrNull($scope.modelNew.Characteristics3Id) && $scope.modelNew.IsDimension3) flag = true;
        return flag;
    }
    function proertiesNull(model, proerty, nullProerty) {
        if (!model[proerty])
            model[nullProerty] = null;
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.buyerStyles.splice($scope.index, 1);
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
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.model = {};
        $scope.modelNew = {
            IsDimension1: false
            , IsDimension2: false
            , IsDimension3: false
        };
    }

    // #region MM
    $scope.materialModel = {
        materialTypeId: null
        , materialCategoryId: null
        , materialSubCategoryId: null
        , materialGroupMasterId: null
    }
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
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
    $scope.popUp = function () {
        $scope.popUpDataList = [];
        $scope.popUpUrl = $scope.path + 'GetMaterialMasterList';
        baseService.setCurrentPage('popUpDataList');
        $scope.getPopUpData = function (pageno) {
            var paramList = [];
            paramList.push($scope.materialModel.materialTypeId);
            paramList.push($scope.materialModel.materialGroupMasterId);
            paramList.push($scope.materialModel.materialCategoryId);
            paramList.push($scope.materialModel.materialSubCategoryId);
            $scope.popUpParameters.paramList = JSON.stringify(paramList);
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };
    $scope.selectDoubleClick = function (data) {
        $scope.modelNew = data;
        GetCharacteristicsList(true);
        $scope.modelNew.IsDimension1 = false;
        $scope.modelNew.IsDimension2 = false;
        $scope.modelNew.IsDimension3 = false;
        $scope.closePopUp();
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        $scope.materialModel = {};
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
    // #endregion MM

    function GetCharacteristicsList(flag) {
        try {
            $http({
                method: "GET",
                url: $scope.path + '/GetCharacteristicsList',
                params: { 'materialMasterId': $scope.modelNew.MaterialMasterId },
                dataType: "json"
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, "failure");
                else if (baseService.arrayLength(response.data) === 0) {
                    if (flag) {
                        $scope.modelNew = {
                            IsDimension1: false
                            , IsDimension2: false
                            , IsDimension3: false
                        };
                    }
                    return ShowResult('This material has no characteristics.', "failure");
                }
                else {
                    $scope.characteristicsList = response.data;
                    if (baseService.arrayLength($scope.characteristicsList) > 0) {
                        $scope.modelNew.Characteristics1Id = $scope.characteristicsList[0].CharacteristicsId;
                        $scope.modelNew.Characteristics1Name = $scope.characteristicsList[0].Characteristics;
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 1) {
                        $scope.modelNew.Characteristics2Id = $scope.characteristicsList[1].CharacteristicsId;
                        $scope.modelNew.Characteristics2Name = $scope.characteristicsList[1].Characteristics;
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 2) {
                        $scope.modelNew.Characteristics3Id = $scope.characteristicsList[2].CharacteristicsId;
                        $scope.modelNew.Characteristics3Name = $scope.characteristicsList[2].Characteristics;
                    }
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        } catch (e) {
            throw e;
        }
    }

    $scope.getMaterialTypeUrl = 'Materials/materialtype/getcbobymaterialmaster';
    $scope.getMaterialCategoryUrl = 'Materials/materialcategory/getcbobymaterialmaster';
    $scope.getMaterialSubCategoryUrl = 'Materials/materialsubcategory/getcbobymaterialmaster';
    $scope.getMaterialGroupMasterUrl = 'Materials/materialgroupmaster/getcbobymaterialmaster';
    // #region DDL
    function getMaterialTypeList() {
        $http.get($scope.getMaterialTypeUrl)
            .then(function (response) {
                $scope.materialTypeList = response.data;
            });
    }
    function getMaterialCategoryList() {
        $http.get($scope.getMaterialCategoryUrl)
            .then(function (response) {
                $scope.materialCategoryList = response.data;
            });
    }
    function getMaterialSubCategoryList() {
        $http.get($scope.getMaterialSubCategoryUrl)
            .then(function (response) {
                $scope.materialSubCategoryList = response.data;
            });
    }
    function getMaterialGroupMasterList() {
        $http.get($scope.getMaterialGroupMasterUrl)
            .then(function (response) {
                $scope.materialGroupMasterList = response.data;
            });
    }
    // #endregion DDL

    $scope.getBlanketData = function () {
        //$scope.modelNew = data;
        $http.get("Materials/fabricrollmanagementsettings/GetBlankeData")
                .then(
                    function successCallback(response) {
                        $scope.modelNew.BlanketLengthBeforeWash = response.data[0].BlanketDefaultLength;
                        $scope.modelNew.BlanketWidthBeforeWash = response.data[0].BlanketDefaultWidth;
                        $scope.modelNew.IsBlanketDefaultLengthValuesChangeable = response.data[0].IsBlanketDefaultLengthValuesChangeable;
                        $scope.modelNew.IsBlanketDefaultWidthValuesChangeable = response.data[0].IsBlanketDefaultWidthValuesChangeable;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
      
    };
    $scope.getBlanketData();
}