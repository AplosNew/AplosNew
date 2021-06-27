'use strict';
ProductLibraryController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function ProductLibraryController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
    $rootScope.title = "Product Library";
    $scope.Action = 'Save';
    $scope.AltUomAction = 'Add Alternative UOM';
    $scope.materialTypeCheck = false;
    $scope.index = -1;
    $scope.altUomIndex = -1;
    $scope.masterDataList = [];
    $scope.RecipeList = [];

    $scope.materialMasters = [];
    $scope.path = 'OrderManagements/ProductLibrary/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'GetAutoSequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.searchCol = "";
    $scope.searchVal = "";
    $scope.PRsearchBy = "UserName";
    $scope.PRsearch = "";
    $scope.PRFilterList = [
        { 'name': 'Sequence', 'value': 'Sequence' },
        { 'name': 'Code', 'value': 'Code' },
        { 'name': 'Short Name', 'value': 'ShortName' },
        { 'name': 'Standard Name', 'value': 'StandardName' },
        { 'name': 'User Name', 'value': 'UserName' },
        { 'name': 'Material Master', 'value': 'MaterialMaster' },

    ];

    $scope.getData = function () {
        $scope.masterDataList = [];
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.getListUrl + "?column=" + $scope.PRsearchBy + "&value=" + $scope.PRsearch
        }).then(function successCallback(response) {
            $scope.masterDataList = response.data;
        });
    };
    $scope.getData();


    $scope.Model = {
        Id: null
        , CompanyGroupId: null
        , Sequence: 0
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Description: null
        , RecipeOrProductionGroup: 'Recipe'
        , RecipeId: null
        , MaterialMasterId: null
        , ArticleId: null
        , ProductMasterName: null
        , ProductionGroup: null
        , Remarks: null
        , Active: true
        , AddedBy: null
        , AddedDate: null
        , AddedFromIP: null
        , UpdatedBy: null
        , UpdatedDate: null
        , UpdatedFromIP: null
    };
    $scope.ModelNew = angular.copy($scope.Model);

    $scope.searchMaterialMasterList = [
        {
            'name': 'Material Type',
            'value': 'MaterialTypeName'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMasterName'
        },
        {
            'name': 'Product',
            'value': 'ProductMasterName'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'IsAsset',
            'value': 'Asset'
        },
        {
            'name': 'Asset Master',
            'value': 'AssetMasterName'
        },
        {
            'name': 'Budget Code',
            'value': 'AssetBudgetCode'
        },
        {
            'name': 'Activity',
            'value': 'ActivityName'
        },
        {
            'name': 'Id',
            'value': 'Id'
        }
    ];
    $scope.url = null;


    // #region *********Product Library*********//
    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.ModelNew.Sequence = response.data[0].Sequence;
            });
    };
    $scope.GetSequence();

    //ProductDefinition 
    $scope.getMaterial = function (index) {

        $scope.materialType = 'ProductDefinition';
        $scope.itemIndex = index;
        $scope.getMaterialMasterbyTypePopUp();
    };


    $scope.selectMaterialByType = function (ob) {
        $scope.ModelNew.MaterialMasterId = ob.Id;
        $scope.ModelNew.MaterialMaster = ob.UserName;
        $scope.ModelNew.ProductMasterName = ob.ProductMasterName;

        $scope.ModelNew.ArticleId = null;
        $scope.ModelNew.Article = null;
        $scope.ModelNew.HasAttribute = ob.HasAttribute;

        if ($scope.ModelNew.HasAttribute) {
            $scope.materialType = null;
            $scope.getArticleSearchList(ob.Id);
        } else {
            $scope.closeMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }

        $scope.closeMaterialMasterbyTypePopUp();
    };

    $scope.MaterialClear = function () {
        $scope.ModelNew.MaterialMasterId = null;
        $scope.ModelNew.MaterialMaster = null;
    };

    $scope.getArticle = function (index) {
        $scope.itemIndex = index;
        $scope.getArticleSearchList($scope.ModelNew.MaterialMasterId);
    };

    $scope.selectarticle = function (ob) {
        try {
            $scope.ModelNew.MaterialMasterId = ob.MaterialMasterId;
            $scope.ModelNew.MaterialMaster = ob.MaterialMasterName;
            $scope.ModelNew.ArticleId = ob.Id;
            $scope.ModelNew.Article = ob.StandardName;
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.clearArticle = function () {
        $scope.ModelNew.ArticleId = null;
        $scope.ModelNew.Article = null;
    };

    $scope.GetRecipepopUp = function () {
        $http.get("OrderManagements/ProductLibrary/GetRecipeGlobalMasterList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.RecipeList = response.data;
                    }
                    angular.element(document.querySelector("#RecipePopUp")).modal("show");
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.SelectedRecipe = function ($event) {
        try {
            var soitem = $event.data;
            $scope.ModelNew.RecipeId = soitem.Id;
            $scope.ModelNew.Recipe = soitem.Name;
            angular.element(document.querySelector("#RecipePopUp")).modal("hide");
        } catch (ex) {
            ShowResult(ex, 'error');
        }
    };

    $scope.Get = function (obj) {
        $scope.setTab(1);
        $scope.Model = obj.data;
        $scope.ModelNew = Object.assign({}, $scope.Model);

        getProductLibraryAttribute();

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.materialAttributeMasterListForSave = [];
    function combinematerialAttributeMasterList(list) {
        angular.forEach(list, function (item, key) {
            item.Sequence = key + 1;
            $scope.materialAttributeMasterListForSave.push(item);
        });
    }

    $scope.confirmToCreateNewVersion = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {

            if ($scope.Action == "Save") {
                $scope.message_confirmation = "Are you sure to save? after save you can not change Code.";
                angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
            }
            else {

                angular.element(document.querySelector("#confirmPostPopUp")).modal("hide");
                try {
                    $scope.materialAttributeMasterListForSave = [];
                    combinematerialAttributeMasterList($scope.materialAttributeMasters);
                    if ($scope.ModelNew.RecipeOrProductionGroup === 'Recipe' && baseService.isUndefinedOrNull($scope.ModelNew.RecipeId)) {
                        throw "Recipe is required.";
                    }
                    if ($scope.ModelNew.RecipeOrProductionGroup === 'ProductionGroup' && baseService.isUndefinedOrNull($scope.ModelNew.ProductionGroup)) {
                        throw "Production Group is required.";
                    }

                    $scope.$broadcast('show-errors-check-validity');
                    if ($scope.ModelNewForm.$valid) {
                        $http({
                            method: 'POST',
                            url: $scope.updateUrl,
                            data: { 'entity': $scope.ModelNew, 'attributes': $scope.materialAttributeMasterListForSave },
                            dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, 'failure');
                            }
                            else {
                                ShowResult(response.data.Message, 'success');
                                $scope.ModelNew = response.data.Data;
                                $scope.getData();
                                $scope.Action = 'Update';
                            }
                        }), function errorCallBack(response) {
                            ShowResult(response.data.Message, 'failure');
                        }
                    }
                } catch (e) {
                    ShowResult(e, 'failure');
                }
            }
        }

    }

    $scope.CloseNo = function () {
        angular.element(document.querySelector("#confirmPostPopUp")).modal("hide");
    }

    $scope.Save = function () {
        try {
            $scope.materialAttributeMasterListForSave = [];
            combinematerialAttributeMasterList($scope.materialAttributeMasters);
            if ($scope.ModelNew.RecipeOrProductionGroup === 'Recipe' && baseService.isUndefinedOrNull($scope.ModelNew.RecipeId)) {
                throw "Recipe is required.";
            }
            if ($scope.ModelNew.RecipeOrProductionGroup === 'ProductionGroup' && baseService.isUndefinedOrNull($scope.ModelNew.ProductionGroup)) {
                throw "Production Group is required.";
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                if ($scope.Action == "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'entity': $scope.ModelNew, 'attributes': $scope.materialAttributeMasterListForSave },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.ModelNew = response.data.Data;
                            $scope.getData();
                            $scope.Action = 'Update';
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
                else if ($scope.Action == "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: { 'entity': $scope.ModelNew, 'attributes': $scope.materialAttributeMasterListForSave },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.ModelNew = response.data.Data;
                            $scope.getData();
                            $scope.Action = 'Update';
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: 'OrderManagements/ProductLibrary/Delete?id=' + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();

                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    // #endregion *********Material Master End*********//


    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.Model = {
            Id: null
            , CompanyGroupId: null
            , Sequence: 0
            , Code: null
            , ShortName: null
            , StandardName: null
            , UserName: null
            , Description: null
            , RecipeOrProductionGroup: 'Recipe'
            , RecipeId: null
            , MaterialMasterId: null
            , ProductionGroup: null
            , Remarks: null
            , Active: true
            , AddedBy: null
            , AddedDate: null
            , AddedFromIP: null
            , UpdatedBy: null
            , UpdatedDate: null
            , UpdatedFromIP: null
        };
        $scope.ModelNew = angular.copy($scope.Model);
        $scope.isSet(1);
        $scope.setTab(1);

        $scope.materialAttributeMasters = [];

        $scope.ClearMaterialMasterAttribute();

    }

    function reDirectToRequiredTab() {
        if ($scope.materialMasterFormTab1.$invalid) $scope.setTab(1);
        else if ($scope.materialMasterFormTab2.$invalid) $scope.setTab(2);
        else if ($scope.materialMasterFormTab3.$invalid) $scope.setTab(3);
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    function hasColumn(list, v1, v2, v3) {
        for (var i = 0; i < list.length; i++) {
            var ob = list[i];
            if (ob.ProductCategoryName == v1) {
                if (ob.ProductSubCategoryName == v2) {
                    if (ob.ProductName == v3) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    $scope.getUniqueColumn = function (fromtotable) {
        var _obj = {
            ProductCategoryName: null,
            ProductSubCategoryName: null,
            ProductName: null
        }
        var _step = [];
        //var _stepList = [];
        for (var i_cycle = 0; i_cycle < fromtotable.length; i_cycle++) {
            var hasduplicate = true;
            var _newObj = fromtotable[i_cycle];
            _obj.ProductCategoryName = _newObj.ProductCategoryName;
            _obj.ProductSubCategoryName = _newObj.ProductSubCategoryName;
            _obj.ProductName = _newObj.ProductName;
            hasduplicate = hasColumn(_step, _obj.ProductCategoryName, _obj.ProductSubCategoryName, _obj.ProductName);
            if (hasduplicate == false) {
                _step.push(_obj);
            }
        }
        return _step;
    }




    // #region ProductLibrary Attribute.
    $scope.chindex = -1;

    $scope.ScanItemList = [];
    $http({
        method: 'GET'
        , url: 'OrderManagements/ScanItem/GetCbo'
    }).then(function successCallback(response) {
        $scope.ScanItemList = response.data;

    }), function errorCallBack(response) {
    };

    $scope.materialAttributeMasters = [];
    $scope.ChAction = 'Add Row';
    $scope.vindex = -1;
    $scope.materialAttributeList = [];
    $http({
        method: 'GET',
        url: 'materials/materialattribute/getcbo',
        params: { 'valueAssignment': null }
    }).then(function successCallback(response) {
        $scope.materialAttributeList = response.data;
    });
    $scope.materialAttributeMaster = {
        Id: null, ProductLibraryId: null, Sequence: null, Code: null, ShortName: null, StandardName: null, UserName: null, ScanItemId: null, AttributeValue: null, UoMId: null, Remarks: null, Description: null, Active: true, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };
    $scope.materialAttributeMasterNew = Object.assign({}, $scope.materialAttributeMaster);
    //$scope.change = function () {
    //    var obj = $.grep($scope.materialAttributeList, function (item) {
    //        return item.Value === $scope.materialAttributeMasterNew.MaterialAttributeId;
    //    })[0];
    //    $scope.materialAttributeMasterNew.UserName = obj.MaterialAttributeName;
    //    $scope.materialAttributeMasterNew.ValueAssignmentLevel = obj.ValueAssignmentLevel;
    //    $scope.materialAttributeMasterNew.IsFreeField = obj.IsFreeField;
    //    $scope.materialAttributeMasterNew.IsPreDefinedField = obj.IsPreDefinedField;
    //    $scope.materialAttributeMasterNew.IsMandatory = obj.IsMandatory;
    //    $scope.materialAttributeMasterNew.IsFixedNoOfCharacter = obj.IsFixedNoOfCharacter;
    //    $scope.materialAttributeMasterNew.NoOfCharacter = obj.NoOfCharacter;
    //    $scope.materialAttributeMasterNew.AttributeProperty = obj.AttributeProperty;
    //};

    $scope.addMatarialAttributeRow = function () {
        try {
            CloseShowResult();
            if (baseService.isUndefinedOrNull($scope.materialMasterNew.MaterialGroupMasterId))
                return manualValidation('div_Attr', true, 'Please select MaterialGroup!');
            if ($scope.materialAttributeMasters.length > 19)
                throw 'Total no of material attribute can not be more than 20!';
            if (baseService.isUndefinedOrNull($scope.materialAttributeMasterNew.MaterialAttributeId))
                return manualValidation('div_Attr', true, 'Material attribute is required.');
            var isAvailable = false;
            for (var i = 0; i < $scope.materialAttributeMasters.length; i++) {
                isAvailable = baseService.isAvailableInList($scope.materialAttributeMasters[i].MaterialAttributeId, $scope.materialAttributeMasterNew.MaterialAttributeId, i, $scope.chindex);
                if (isAvailable) throw 'This material attribute : [' + $scope.materialAttributeMasterNew.UserName + '] has been already taken';
            }
            angular.copy($scope.materialAttributeMasterNew, $scope.materialAttributeMaster);
            // isAvailable true == add new
            if (!isAvailable) {
                if ($scope.chindex === -1) {
                    $scope.materialAttributeMaster.MaterialMasterId = $scope.materialMasterNew.Id;
                    //$scope.materialAttributeMaster.MaterialGroupMasterId = $scope.materialMasterNew.MaterialGroupMasterId;
                    $scope.materialAttributeMaster.Sequence = $scope.materialAttributeMasters.length + 1;
                    $scope.materialAttributeMasters.push($scope.materialAttributeMaster);
                }
                else
                    $scope.materialAttributeMasters[$scope.chindex] = this.materialAttributeMaster;
                $scope.chindex = -1;
                $scope.ClearMaterialMasterAttribute();
            }
        } catch (err) {
            ShowResult(err, 'failure');
        }
    };

    $scope.materialAttributeCreatePopUp = function () {
        $scope.ClearMaterialMasterAttribute();
        angular.element(document.querySelector('#ProductLibraryAttributeCreatePopUp')).modal('show');

    };

    $scope.manualValidationAddRemove = function (divId, fieldName, model, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull(model[str.replace(/\s/g, '')])) {
            if (manualValidation(divId, true, msg))
                return true;
        } else
            return manualValidation(divId, false);
    };

    $scope.uOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });

    $scope.createMatarialAttribute = function () {
        try {
            CloseShowResult();
            if ($scope.materialAttributeMasters.length > 19)
                throw 'Total no of attribute can not be more than 20!';

            if ($scope.manualValidationAddRemove('div_attr_1', 'Sequence', $scope.materialAttributeMasterNew)) return;
            if ($scope.manualValidationAddRemove('div_attr_2', 'Code', $scope.materialAttributeMasterNew)) return;
            if ($scope.manualValidationAddRemove('div_attr_3', 'Short Name', $scope.materialAttributeMasterNew)) return;
            if ($scope.manualValidationAddRemove('div_attr_4', 'Standard Name', $scope.materialAttributeMasterNew)) return;
            if ($scope.manualValidationAddRemove('div_attr_5', 'User Name', $scope.materialAttributeMasterNew)) return;
            if ($scope.manualValidationAddRemove('div_attr_7', 'AttributeValue', $scope.materialAttributeMasterNew)) return;
            $scope.materialAttributeMasterNew.ScanItem = $("#ScanItemId option:selected").text();
            var isAvailable = false;
            for (var i = 0; i < $scope.materialAttributeMasters.length; i++) {
                if (baseService.isAvailableInList($scope.materialAttributeMasters[i].Code, $scope.materialAttributeMasterNew.Code, i, $scope.chindex))
                    throw 'Code : [' + $scope.materialAttributeMasterNew.Code + '] has been already taken';
                isAvailable = baseService.isAvailableInList($scope.materialAttributeMasters[i].UserName, $scope.materialAttributeMasterNew.UserName, i, $scope.chindex);
                if (isAvailable) throw 'User name : [' + $scope.materialAttributeMasterNew.UserName + '] has been already taken';
                if (baseService.isAvailableInList($scope.materialAttributeMasters[i].ScanItemId, $scope.materialAttributeMasterNew.ScanItemId, i, $scope.chindex))
                    throw 'Scan Item : [' + $scope.materialAttributeMasterNew.ScanItem + '] has been already taken';
            }
            angular.copy($scope.materialAttributeMasterNew, $scope.materialAttributeMaster);
            // isAvailable true == add new
            if (!isAvailable) {
                if ($scope.chindex === -1) {
                    $scope.materialAttributeMaster.Id = null;
                    $scope.materialAttributeMaster.ProductLibraryId = $scope.ModelNew.Id;
                    $scope.materialAttributeMaster.ScanItem = $("#ScanItemId option:selected").text();
                    $scope.materialAttributeMaster.UoM = $("#UOM option:selected").text();
                    $scope.materialAttributeMasters.push($scope.materialAttributeMaster);
                }
                else {
                    $scope.materialAttributeMaster.ScanItem = $("#ScanItemId option:selected").text();
                    $scope.materialAttributeMaster.UoM = $("#UOM option:selected").text();

                    $scope.materialAttributeMasters[$scope.chindex] = $scope.materialAttributeMaster;
                }
                $scope.chindex = -1;
                $scope.ClearMaterialMasterAttribute();
            }
        } catch (err) {
            ShowResult(err, 'failure', 'ProductLibraryAttributeCreatePopUp');
        }
    };

    $scope.editAttribute = function (index) {
        $scope.chindex = index;
        angular.copy($scope.materialAttributeMasters[$scope.chindex], $scope.materialAttributeMaster);
        angular.copy($scope.materialAttributeMaster, $scope.materialAttributeMasterNew);
        $scope.attrAction = 'Update Row';
        angular.element(document.querySelector('#ProductLibraryAttributeCreatePopUp')).modal('show');
    };

    $scope.DeleteModal = function (index, list, childList, name, parentId) {
        $scope.vindex = index;
        $scope.chList = list;
        $scope.parentId = parentId;
        $scope.childList = childList;
        $scope.subMaterialMessage = 'Are you sure want to permanent delete ' + name + '.?';
        angular.element(document.querySelector('#materialMasterAttribute')).modal('show');
    };
    $scope.removeMaterialMasterAttributeRow = function () {
        for (var t = baseService.arrayLength($scope[$scope.childList]) - 1; t >= 0; t--) {
            if ($scope[$scope.childList][t][$scope.parentId] === $scope[$scope.chList][$scope.vindex][$scope.parentId])
                $scope[$scope.childList].splice(t, 1);
        }
        $scope[$scope.chList].splice($scope.vindex, 1);
        $scope.vindex = -1;
        $scope.chList = null;
    };



    $scope.ClearMaterialMasterAttribute = function () {
        $scope.ChAction = 'Add Row';
        $scope.attrAction = 'Add Row';
        $scope.materialAttributeMaster = {};
        $scope.materialAttributeMasterNew = {
            Id: null, ProductLibraryId: null, Sequence: $scope.materialAttributeMasters.length + 1, Code: null, ShortName: null, StandardName: null, UserName: null, ScanItemId: null, Remarks: null, Description: null, Active: true, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null

        };
        CloseModalShowResult('ProductLibraryAttributeCreatePopUp');
    };

    $scope.CloseMaterialMasterAttribute = function () {
        $scope.ClearMaterialMasterAttribute();
        angular.element(document.querySelector('#ProductLibraryAttributeCreatePopUp')).modal('hide');
    };

    function getProductLibraryAttribute() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductLibraryAttribute?masterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.materialAttributeMasters = response.data;
        });
    }

    // #endregion Material Master Attribute



    // #region Generic function

    function duplicateCheck(listValue, modelValue, listIndex, index, fieldName, msg) {
        if (listValue === modelValue && modelValue && listIndex !== index) {
            if (!baseService.isUndefinedOrNull(msg)) throw msg;
            else throw 'This ' + fieldName + ' (' + modelValue + ') already exist.';
        }
    }
    function getMaxNumberFromList(list, fieldName) {
        var max = 0;
        for (var t = 0; t < baseService.arrayLength(list); t++) {
            if (list[t][fieldName] > max)
                max = parseFloat(list[t][fieldName]);
        }
        return max + 1;
    }
    function checkPropertiesAndCharLength(parentList, model) {
        if (parentList.AttributeProperty === 'Integer') {
            if (!Number.isInteger(parseInt(model.UserName))) throw 'UserName is not Integer';
        }
        else if (parentList.AttributeProperty === 'Decimal') {
            if (!baseService.checkDecimal(model.UserName)) throw 'UserName is not Decimal';
        }
        else {
            if (parentList.IsFixedNoOfCharacter) {
                var code = model.UserName;
                if (code.length !== parseInt(parentList.NoOfCharacter))
                    throw 'UserName can not be greater than ' + parentList.NoOfCharacter;
            }
        }
    }

    //$scope.removeRowModal = function (id, name, list) {
    //    try {
    //        $scope.list = list;
    //        $scope.id = id;
    //        $scope.message_confirmation = "Are you sure want to permanently delete [" + name + "] ";
    //        angular.element(document.querySelector('#confirmRemovePopUp')).modal('show');
    //    }
    //    catch (e) {
    //        ShowResult(e, 'Error');
    //    }
    //};
    $scope.Isdeleteable = false;
    //$scope.conRemoveRow = function () {
    //    try {
    //        $http.get('Materials/MaterialMaster/CheckMaterialCVUsingInBOM?characteristicsValueId=' + $scope.id)
    //            .then(function (response) {
    //                $scope.Isdeleteable = response.data;
    //                if ($scope.Isdeleteable == false) {
    //                    for (var t = 0; t < baseService.arrayLength($scope[$scope.list]); t++) {
    //                        if ($scope[$scope.list][t].Id === $scope.id) {
    //                            $scope[$scope.list].splice(t, 1);
    //                            break;
    //                        }
    //                    }
    //                } else {
    //                    ShowResult("This Characteristics Value is used in BOM, cann't delete.", 'failure');
    //                }
    //            });

    //        $scope.list = null;
    //        $scope.id = null;
    //        angular.element(document.querySelector('#confirmRemovePopUp')).modal('hide');
    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //};

    $scope.removeRowModal = function (index, data) {
        try {

            $scope.LCChargesId = data.Id;
            $scope.bActivityIndex = index;
            if (baseService.isUndefinedOrNull($scope.LCChargesId))
                $scope.messagedelete = 'Are you sure want to delete this data....';
            else
                $scope.messagedelete = 'Are you sure want to delete permanently [ ' + data.UserName + ' ]';
            angular.element(document.querySelector('#confirmRemovePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.conRemoveRow = function () {
        if (baseService.isUndefinedOrNull($scope.LCChargesId)) {
            $scope.characteristicsValueList.splice($scope.bActivityIndex, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'OrderManagements/MaterialMaster/DeleteCharacteristicsValues?id=' + $scope.LCChargesId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.characteristicsValueList.splice($scope.bActivityIndex, 1);
                    GetCharacteristicsValueListByMaterialMaster($scope.materialMasterNew.Id);
                    $scope.GetMaterialMasterCharacteristicsValueSequence();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion Generic function

    $scope.message_detailconfirmation = null;
    $scope.removeAttr = function (obj, index) {
        $scope.Attrindex = index;
        $scope.AttrNew = obj;

        $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.AttrNew.UserName + ' ]';
        angular.element(document.querySelector('#confirmBoMDetailPopUp')).modal('show');
    }

    $scope.DeleteProductLibraryAttribute = function () {
        if (baseService.isUndefinedOrNull($scope.AttrNew.Id)) {
            $scope.materialAttributeMasters.splice($scope.Attrindex, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'OrderManagements/ProductLibrary/DeleteProductLibraryAttribute?id=' + $scope.AttrNew.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.materialAttributeMasters.splice($scope.Attrindex, 1);
                    getProductLibraryAttribute();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    $scope.removeAttrValueRow = function (id, name, list) {
        try {
            $scope.list = list;
            $scope.id = id;
            $scope.message_confirmation = "Are you sure want to permanent delete [" + name + "] ";
            angular.element(document.querySelector('#confirmValueRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.attrValueRemoveRow = function () {
        if (!baseService.isUndefinedOrNull($scope.id) && !$scope.id.startsWith('n-')) {
            $http({
                method: 'POST'
                , url: $scope.path + 'DeleteValue?valueId=' + $scope.id
                , data: $scope.materialValue
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'materialAttributeValuePoUp');
                    $scope.list = null;
                    $scope.id = null;
                }
                else {
                    ShowResult(response.data.Message, 'success', 'materialAttributeValuePoUp');
                    for (var t = 0; t < baseService.arrayLength($scope[$scope.list]); t++) {
                        if ($scope[$scope.list][t].Id === $scope.id) {
                            $scope[$scope.list].splice(t, 1);
                            break;
                        }
                    }
                    $scope.list = null;
                    $scope.id = null;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'materialAttributeValuePoUp');
            };
        }
        else {
            for (var t = 0; t < baseService.arrayLength($scope[$scope.list]); t++) {
                if ($scope[$scope.list][t].Id === $scope.id) {
                    $scope[$scope.list].splice(t, 1);
                    break;
                }
            }
            $scope.list = null;
            $scope.id = null;
        }
        angular.element(document.querySelector('#confirmValueRemovePopUp')).modal('hide');
    };

    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }

    $scope.getProductLibraryReport = function () {
        var FilterModel = null;
        var gridObj = $("#Grid").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        if (angular.isUndefinedOrNull(filteredRecords) == true || filteredRecords.length == 0) {
            filteredRecords = $scope.masterDataList;
        }

        var FilterString = getString(filteredRecords, "Id");

        try {
            $http({
                method: 'POST',
                url: $scope.path + "ProductLibraryReport",
                data: { IDs: FilterString },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
        }

    }



}
