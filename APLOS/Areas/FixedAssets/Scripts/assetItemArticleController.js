'use strict';
assetItemArticleController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService'];
function assetItemArticleController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $rootScope.title = 'Asset Item Article';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.assetItemArticles = [];
    $scope.path = 'fixedassets/AssetItemArticle/';
    $scope.getListUrl = 'fixedassets/AssetItemArticle/getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.assetItemCboList = [];
    cboService.getCboAssetItemMachine(function (result) {
        $scope.assetItemCboList = result;
    });

    $scope.assetItemCharacteristicsList = [];
    cboService.getCboAssetItemCharacteristics(function (result) {
        $scope.assetItemCharacteristicsList = result;
    });

    $scope.searchAssetItemList = [
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Asset Class',
            'value': 'FixedAssetClassName'
        },
        {
            'name': 'Asset SubClass',
            'value': 'FixedAssetSubClassName'
        },
        {
            'name': 'Asset Master',
            'value': 'FixedAssetMasterName'
        },
        {
            'name': 'Asset Category',
            'value': 'AssetCategory'
        },
        {
            'name': 'Asset SubCategory',
            'value': 'FixedAssetSubCategoryName'
        },
        {
            'name': 'Asset Type',
            'value': 'AssetType'
        }
    ];
    $scope.articleSku = {
        Dimension1: null,
        Dimension2: null,
        Dimension3: null
    }
    $scope.AssetItemParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.assetItemSearch = function () {
        $scope.loadAssetItem();
    }
    $scope.assetItemList = [];
    $scope.loadAssetItem = function () {
        var url = 'fixedassets/AssetItemArticle/GetAssetItemSearchList';
        baseService.setCurrentPage('assetItemList');
        $scope.assetItemModalList = function (pageno) {
            baseService.paginationBase(url, pageno, $scope.AssetItemParameters)
                .then(function (result) {
                    $scope.assetItemList = result.Rows;
                    $scope.AssetItemParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
            angular.element(document.querySelector('#AssetitemSearchPopUp')).modal('show');
        };
        $scope.assetItemModalList();
    }
    $scope.closeAssetItemPopUp = function () {
        angular.element(document.querySelector('#AssetitemSearchPopUp')).modal('hide');
    }
    $scope.selectAssetItem = function (data) {
        $scope.assetItemArticleNew.AssetItemId = data.Id;
        $scope.assetItemArticleNew.MaterialMasterName = data.UserName;
        angular.element(document.querySelector('#AssetitemSearchPopUp')).modal('hide');
        $scope.getValue();
    }
    $scope.clearAssetItemSearch = function () {
        $scope.assetItemArticleNew.AssetItemId = null;
        $scope.assetItemArticleNew.MaterialMasterName = null;
        $scope.assetItemArticleList = [];
    }

    // #region SubMaterial
    $scope.SubMaterialCaption = 'Add SubMaterial';
    $scope.assetItemArticleList = [];
    $scope.subMaterialHead = [];
    $scope.assetItemArticle = {
        Id: null,
        AssetItemId: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        AssetItemArticleValues: []
    };
    $scope.assetItemArticleNew = Object.assign({}, $scope.assetItemArticle);
    $scope.ShowSubMaterialFormPopUp = function () {
        if ($scope.Action === "Update") {
            $scope.getAttributeOnUpdate();
        } else {
            $scope.getAttribute();
        }
        angular.element(document.querySelector('#subMaterialPoUp')).modal('show');
    }
    $scope.CloseSubMaterialPopUp = function () {
        SubMaterialClear();
        angular.element(document.querySelector('#subMaterialPoUp')).modal('hide');
        CloseModalShowResult(subMaterialPoUp);
    }
    $scope.AddAssetItem = function () {
        try {
            for (var i = 0; i < $scope.attributeList.length; i++) {
                var _invalid = $scope.IsMandatoryButNull($scope.attributeList[i].IsMandatory, $scope.attributeList[i].ValueFreeText);
                if (_invalid) {
                    throw $scope.attributeList[i].FixedAssetAttributeName + ' value is required!';
                }
            }
            uniqueCheckInSubMaterialList($scope.assetItemArticleList, $scope.assetItemArticleNew);
            for (var i = 0; i < $scope.assetItemArticleList.length; i++) {
                if (!materialValueDuplecateCheck($scope.assetItemArticleList[i].AssetItemArticleValues, $scope.attributeList))
                    throw 'This combination already exist.!';
            }
            if ($scope.assetItemArticleList.length < 1) {
                $scope.subMaterialHead = [];
                getSubMaterialHed($scope.attributeList, $scope.subMaterialHead, false);
            }
            angular.forEach($scope.attributeList, function (element, i) {
                $scope.assetItemArticleNew.AssetItemArticleValues.push({
                    Id: null
                    , AssetItemArticleId: $scope.assetItemArticleNew.Id
                    , AssetItemId: $scope.assetItemArticleNew.AssetItemId
                    , FixedAssetAttributeId: element.FixedAssetAttributeId
                    , FixedAssetAttributeName: element.FixedAssetAttributeName
                    , FixedAssetAttributeValueId: element.FixedAssetAttributeValueId
                    , ValueFreeText: element.ValueFreeText
                });
            });
            $scope.assetItemArticle = Object.assign({}, $scope.assetItemArticleNew);
            $scope.assetItemArticleList.push($scope.assetItemArticle);
            CloseModalShowResult(subMaterialPoUp);
            SubMaterialClear();
        } catch (e) {
            ShowResult(e, 'failure', 'subMaterialPoUp')
        }
    }
    function uniqueCheckInSubMaterialList(mainList, model) {
        for (var i = 0; i < mainList.length; i++) {
            if (mainList[i].Code == model.Code) {
                throw 'Code is already exist in grid.!';
            }
            if (mainList[i].ShortName == model.ShortName) {
                throw 'Short name is already exist in grid.!';
            }
            if (mainList[i].StandardName == model.StandardName) {
                throw 'Standard name is already exist in grid.!';
            }
        }
    }
    function materialValueDuplecateCheck(list, tempList) {
        var hasDifferent = false;
        for (var i = 0; i < list.length; i++) {
            if (list[i].ValueFreeText !== tempList[i].ValueFreeText) {
                hasDifferent = true;
                break;
            }
        }
        return hasDifferent;
    }
    function SubMaterialClear() {
        $scope.attributeList = [];
        $scope.assetItemArticleNew = {
            Id: null,
            AssetItemId: $scope.assetItemArticleNew.AssetItemId,
            MaterialMasterName: $scope.assetItemArticleNew.MaterialMasterName,
            Code: null,
            ShortName: null,
            StandardName: null,
            AssetItemArticleValues: []
        };
        $scope.Action = "Save";
    }
    function subMaterialFieldValidation(field, fieldName) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw fieldName + ' is required.!';
            }
        } catch (e) {
            throw e;
        }
    }
    function getSubMaterialHed(list, newList, flag) {
        if (flag) {
            for (var i = 0; i < list.length; i++) {
                newList.push({ FixedAssetAttributeName: list[i].MaterialAttribute.UserName });
            }
        }
        else {
            for (var i = 0; i < list.length; i++) {
                newList.push({ FixedAssetAttributeName: list[i].FixedAssetAttributeName });
            }
        }
    }
    $scope.MaterialAttributeValueDelete = function (index, id) {
        $scope.SubMaterialIndex = index;
        $scope.assetItemArticleTempId = id;
        $scope.subMaterialMessage = 'Are you sure want to delete this.?';
        angular.element(document.querySelector('#subMaterial')).modal('show');
    };
    $scope.removeSubMaterialRow = function () {
        $scope.Delete($scope.assetItemArticleTempId);
        $scope.SubMaterialIndex = -1;
        $scope.assetItemArticleTempId = null;
    };
    function GetSubMaterial() {
        $scope.subMaterialHead = [];
        $scope.attributeList = [];
        $scope.assetItemArticleList = [];
        $http({
            method: 'GET',
            url: 'fixedassets/AssetItemArticle/GetAssetItemArticle?assetItemId=' + $scope.assetItemArticleNew.AssetItemId,
            contentType: "application/json; charset=utf-8"
        }).then(function successCallback(response) {
            //$scope.subMaterialList = response.data;
            var subMaterials = response.data;
            if (subMaterials.length > 0) {
                $http({
                    method: 'GET',
                    url: 'fixedassets/AssetItemArticle/GetAssetItemArticleValue?assetItemId=' + $scope.assetItemArticleNew.AssetItemId,
                    contentType: "application/json; charset=utf-8"
                }).then(function successCallback(response) {
                    if (baseService.arrayLength(response.data)) {
                        var valueData = response.data
                        $http({
                            method: 'GET',
                            url: 'fixedassets/AssetItemArticle/GetAssetItemArticleValueHead?assetItemId=' + $scope.assetItemArticleNew.AssetItemId,
                            contentType: "application/json; charset=utf-8"
                        }).then(function successCallback(response) {
                            $scope.subMaterialHead = response.data;
                            if (baseService.arrayLength($scope.subMaterialHead)) {
                                for (var i = 0; i < subMaterials.length; i++) {
                                    subMaterials[i].FixedAssetAttributeValues = [];
                                    for (var a = 0; a < $scope.subMaterialHead.length; a++) {
                                        subMaterials[i].FixedAssetAttributeValues.push({
                                            FixedAssetAttributeId: $scope.subMaterialHead[a].FixedAssetAttributeId,
                                            FixedAssetAttributeName: $scope.subMaterialHead[a].FixedAssetAttributeName,
                                            Id: null,
                                            AssetItemArticleId: null,
                                            ValueFreeText: null,
                                            FixedAssetAttributeValueId: null,
                                            Active: false
                                        });
                                    }
                                }
                            }
                            for (var t = 0; t < baseService.arrayLength(subMaterials); t++) {
                                var subMaterialRow = Object.assign({}, subMaterials[t]);
                                checkValueSubMaterialId(valueData, subMaterialRow);
                                $scope.assetItemArticleList.push(subMaterialRow);
                            }
                        })
                    }
                })
            }
        })
    }
    function checkValueSubMaterialId(valueData, subMaterialRow) {
        for (var v = 0; v < subMaterialRow.FixedAssetAttributeValues.length; v++) {
            var materialAttributeValuesRow = subMaterialRow.FixedAssetAttributeValues[v];//Object.assign({}, subMaterialRow.FixedAssetAttributeValues[v]);
            for (var tt = 0; tt < valueData.length; tt++) {
                if (subMaterialRow.Id === valueData[tt].AssetItemArticleId && materialAttributeValuesRow.FixedAssetAttributeId === valueData[tt].FixedAssetAttributeId) {
                    var newValue = valueData[tt];
                    materialAttributeValuesRow.Id = newValue.Id;
                    materialAttributeValuesRow.AssetItemArticleId = newValue.AssetItemArticleId;
                    materialAttributeValuesRow.FixedAssetAttributeId = newValue.FixedAssetAttributeId;
                    materialAttributeValuesRow.FixedAssetAttributeName = newValue.FixedAssetAttributeName;
                    materialAttributeValuesRow.ValueFreeText = newValue.ValueFreeText;
                    materialAttributeValuesRow.FixedAssetAttributeValueId = newValue.FixedAssetAttributeValueId;
                    materialAttributeValuesRow.Active = newValue.Active;
                    break;
                }
            }
        }
    }
    // #endregion
    $scope.assetItemArticleValueGet = function (index, data) {
        $scope.Action = "Update";
        $scope.assetItemArticleNew.Id = data.Id;
        $scope.assetItemArticleNew.Code = data.Code;
        $scope.assetItemArticleNew.ShortName = data.ShortName;
        $scope.assetItemArticleNew.StandardName = data.StandardName;
        $scope.ShowSubMaterialFormPopUp();
        $scope.getArticleSkuOnUpdate();
    }
    // #region AttributeValue
    $scope.attributeList = [];
    $scope.getAttribute = function () {
        //$scope.materialAttributetbl = false;
        $scope.attributeList = [];
        $http({
            method: 'GET',
            url: 'fixedassets/AssetItemArticle/GetAttribute?assetItemArticleId=' + $scope.assetItemArticleNew.Id + '&assetItemId=' + $scope.assetItemArticleNew.AssetItemId
        }).then(function successCallback(response) {
            $scope.searchFreeField = false;
            $scope.attributeList = response.data;
            for (var i = 0; i < $scope.attributeList.length; i++) {
                var isFree = $scope.attributeList[i].IsFreeField;
                $scope.attributeList[i].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
            }
        })
    }
    $scope.getAttributeOnUpdate = function () {
        //$scope.materialAttributetbl = false;
        $scope.attributeList = [];
        $http({
            method: 'GET',
            url: 'fixedassets/AssetItemArticle/GetAttributeUpdate?assetItemArticleId=' + $scope.assetItemArticleNew.Id + '&assetItemId=' + $scope.assetItemArticleNew.AssetItemId
        }).then(function successCallback(response) {
            $scope.searchFreeField = false;
            $scope.attributeList = response.data;
            for (var i = 0; i < $scope.attributeList.length; i++) {
                var isFree = $scope.attributeList[i].IsFreeField;
                $scope.attributeList[i].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
            }
        })
    }
    $scope.getArticleSkuOnUpdate = function () {
        //$scope.materialAttributetbl = false;
        $scope.articleSkuList = [];
        $http({
            method: 'GET',
            url: 'fixedassets/AssetItemArticle/GetArticleSkuUpdate?assetItemArticleId=' + $scope.assetItemArticleNew.Id + '&assetItemId=' + $scope.assetItemArticleNew.AssetItemId
        }).then(function successCallback(response) {
            $scope.articleSkuList = response.data;
        })
    }
    $scope.attributeSkuPassValueDelete = function (index) {
        $scope.attributeSkuIndex = index;
        $scope.attributeSkuMessage = 'Are you sure want to delete this.?';
        angular.element(document.querySelector('#attributeSku')).modal('show');
    };
    $scope.removeAttributeSkuRow = function () {
        $scope.articleSkuList.splice($scope.attributeSkuIndex, 1)
        $scope.SubMaterialIndex = -1;
    };
    $scope.mvalueindex = -1;
    $scope.materialMstGroupTitle = 'Material Group (Mst)';
    $scope.materialAttributeValueParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Description",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.materialAttributeValueList = [];
    $scope.materialAttributeValuePoUp = function (id, index) {
        $scope.materialAttributeValueUrl = 'fixedassets/FixedAssetAttributeValue/GetList?fixedAssetAttributeId=' + id;
        baseService.setCurrentPage('materialAttributeValueList');
        $scope.getAttributeValueData = function (pageno) {
            baseService.paginationBase($scope.materialAttributeValueUrl, pageno, $scope.materialAttributeValueParameters)
                .then(function (result) {
                    $scope.materialAttributeValueList = result.Rows;
                    $scope.materialAttributeValueParameters.total_count = result.Total;
                    $scope.mvalueindex = index;
                    $scope.searchFreeField = true;
                    angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getAttributeValueData();
    }
    $scope.getAttrValue = function (id, code) {
        $scope.attributeList[$scope.mvalueindex].FixedAssetAttributeValueId = id;
        $scope.attributeList[$scope.mvalueindex].ValueFreeText = code;
        $scope.attributeList[$scope.mvalueindex].FlagDisable = $scope.searchFreeField;
        $scope.mvalueindex = -1;
        angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('hide');
    }
    $scope.idNullByFreeText = function (id, index) {
        if ($scope.attributeList[index].FixedAssetAttributeId == id) {
            $scope.attributeList[index].FixedAssetAttributeValueId = null;
        }
    }
    $scope.searchFreeField = false;
    $scope.IsFreeFieldOrNot = function (IsFreeField) {
        if (IsFreeField) {
            if ($scope.searchFreeField) {
                return true;//disabled true
            }
            else
                return false;//disabled false
        }
        else {
            return true;//disabled true
        }
    }
    $scope.IsMandatoryButNull = function (isMandatory, valueFreeText) {
        if (isMandatory) {
            if (baseService.isUndefinedOrNull(valueFreeText)) {
                return true;
            }
            else
                return false;
        }
        else
            return false;
    }
    $scope.FixedAssetAttributeValueId = '';
    $scope.materialAttributeValueCode = '';
    $scope.materialAttributeValueIndex = -1;
    $scope.SelectMA = function (id, code, index) {
        $scope.FixedAssetAttributeValueId = id;
        $scope.materialAttributeValueCode = code;
        $scope.searchFreeField = true;
        $scope.materialAttributeValueIndex = index;
    }
    $scope.SelectMAButton = function () {
        if (baseService.isUndefinedOrNull($scope.FixedAssetAttributeValueId)) {
            return ShowResult('Please at first select row', 'failure', 'materialAttributeValuePoUp');
        }
        $scope.getAttrValue($scope.FixedAssetAttributeValueId, $scope.materialAttributeValueCode);
        $scope.FixedAssetAttributeValueId = '';
        $scope.materialAttributeValueCode = '';
        $scope.materialAttributeValueIndex = -1;
        angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('hide');
    }
    $scope.materialAttributeValueClear = function (index) {
        $scope.attributeList[index].FixedAssetAttributeValueId = null;
        $scope.attributeList[index].ValueFreeText = null;
        $scope.searchFreeField = false;
        var isFree = $scope.attributeList[index].IsFreeField;
        $scope.attributeList[index].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
    }
    $scope.ClosematerialAttributePopUp = function () {
        angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('hide');
        CloseModalShowResult(subMaterialPoUp);
    }
    $scope.searchMaterialAttributeValueList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Description',
            'value': 'Description'
        }
    ];
    $scope.articleSkuList = [];
    $scope.addArticleSku = function () {
        var data = {
            Dimension1: $scope.articleSku.Dimension1,
            Dimension1Name: angular.element("#Dimension1 :selected").text(),
            Dimension2: $scope.articleSku.Dimension2,
            Dimension2Name: angular.element("#Dimension2 :selected").text(),
            Dimension3: $scope.articleSku.Dimension3,
            Dimension3Name: angular.element("#Dimension3 :selected").text()
        }
        $scope.articleSkuList.push(data);
    }
    // #endregion
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.getassetItemArticle = angular.copy($scope.assetItemArticles[$scope.index]);
        $scope.assetItemArticle = $scope.getassetItemArticle;
        $scope.assetItemArticle.AddedDate = $filter('dateFilter')($scope.assetItemArticle.AddedDate);
        $scope.assetItemArticle.UpdatedDate = $filter('dateFilter')($scope.assetItemArticle.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.getValue = function () {
        GetSubMaterial();
    }
    function validateField(field, fieldName) {
        if (baseService.isUndefinedOrNull(field)) {
            throw ShowResult(fieldName + " is required!", 'failure', 'subMaterialPoUp');
        }
    }
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.assetItemArticleNewForm.$valid && $scope.assetItemArticleNewForm2.$valid) {
            validateField($scope.assetItemArticleNew.Code, 'Code');
            validateField($scope.assetItemArticleNew.ShortName, 'Short Name');
            validateField($scope.assetItemArticleNew.StandardName, 'Standard Name');
            $scope.assetItemArticle = Object.assign({}, $scope.assetItemArticleNew);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'assetItemArticle': $scope.assetItemArticle, 'assetItemArticleValues': $scope.attributeList, 'assetItemArticleSkues': $scope.articleSkuList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        GetSubMaterial();
                        angular.element(document.querySelector('#subMaterialPoUp')).modal('hide');
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: { 'assetItemArticle': $scope.assetItemArticle, 'assetItemArticleValues': $scope.attributeList, 'assetItemArticleSkues': $scope.articleSkuList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        GetSubMaterial();
                        angular.element(document.querySelector('#subMaterialPoUp')).modal('hide');
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.assetItemArticleList.splice($scope.SubMaterialIndex, 1);
                    $scope.SubMaterialIndex = -1;
                    $scope.assetItemArticleTempId = null;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.allClear = function () {
        $scope.clearAssetItemSearch();
        $scope.Action = 'Save';
        $scope.assetItemArticle = {};
        $scope.assetItemArticleNew = {};
        $scope.attributeList = [];
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.assetItemArticle = { AssetItemId: $scope.assetItemArticle.AssetItemId, MaterialMasterName: $scope.assetItemArticle.MaterialMasterName };
        $scope.assetItemArticleNew = { AssetItemId: $scope.assetItemArticleNew.AssetItemId, MaterialMasterName: $scope.assetItemArticleNew.MaterialMasterName };
        $scope.articleSkuList = [];
        if (!baseService.isUndefinedOrNull($scope.assetItemArticleNew.AssetItemId)) {
            getValue();
        }
        $scope.assetItemArticle.Active = true;
    }
}