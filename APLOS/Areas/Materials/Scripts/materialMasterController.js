'use strict';
MaterialMasterController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function MaterialMasterController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
    $rootScope.title = "Material Master";
    $scope.Action = 'Save';
    $scope.AltUomAction = 'Add Alternative UOM';
    $scope.materialTypeCheck = false;
    $scope.index = -1;
    $scope.altUomIndex = -1;
    $scope.materialTypeList = [];
    $scope.materialMasters = [];
    $scope.path = 'Materials/materialmaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'MaterialTypeName, MaterialGroupMasterName, Code, UserName', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.materialMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.materialMaster = {
        Id: null
        , CompanyGroupId: null
        , MaterialTypeId: null
        , MaterialCategoryId: null
        , MaterialSubCategoryId: null
        , TestingStandardId: null
        , MaterialGroupMasterId: null
        , MaterialGroupMasterName: null
        , HSNCodeId: null
        , MaterialGridId: null
        , ProductMasterId: null
        , PurchaseOrderUOMId: null
        , SalesOrderUOMId: null
        , BaseUOMId: null
        , StockUOMId: null
        , ProcessId: null
        , ActivityId: null
        , Sequence: 0
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Image: null
        , WithSKU: false
        , Description: null
        , IsAsset: false
        , AssetMasterId: null
        , AssetMasterName: null
        , BudgetMasterId: null
        , AssetBudgetCode: null
        , IsOriginApplicable: false
        , Active: true
        , IsProduct: false
        , IsRevenue: false
        , IsInventory: false
        , IsExpenseOut: false
        , AssetType: null
        , MachineAllowance: null
        , SkillId: null
        , FixedAssetMasterId: null
        , IsRegular: true
        , IssueByUoM: false
        , MaterialMasterTypeId: null
        , IsReplacement:false
        , IsAlternativeQty:false
    };
    $scope.materialMasterNew = angular.copy($scope.materialMaster);
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

    // #region Image Upload
    $scope.filedata = null;
    $("#uploadImage").change(function () {
        $scope.filedata = this.files[0];
    });
    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };
    $scope.clearImage = function () {
        $scope.filedata = null;
        $scope.imageSrc = '';
        document.getElementById("uploadImage").value = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);
    };
    // #endregion

    // #region *********Material Master*********//
    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.materialMasterNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();
    GetBusinessProcessList();
    function GetBusinessProcessList() {
        $http({
            method: 'GET',
            url: 'Setups/businessprocess/getbusinessprocesslist/',
            params: { materialMasterId: $scope.materialMasterNew.Id }
        }).then(function successCallback(response) {
            $scope.businessProcesses = response.data;
        });
    }

    $scope.materialMasterTypeList = [];
    cboService.getMaterialMasterTypeCbo(function (result) {
        $scope.materialMasterTypeList = result;
    });

    // #region ddl
    $http({
        method: 'GET',
        url: 'Materials/materialtype/getcbo/',
    }).then(function successCallback(response) {
        $scope.materialTypeList = response.data;
    });

    $scope.materialCategoryList = [];
    $http({
        method: 'GET',
        url: 'Materials/materialcategory/getcbo/',
    }).then(function successCallback(response) {
        $scope.materialCategoryList = response.data;
    });

    $scope.materialSubCategoryList = [];
    $http({
        method: 'GET',
        url: 'Materials/materialsubcategory/getcbo/',
    }).then(function successCallback(response) {
        $scope.materialSubCategoryList = response.data;
    });
    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });
    $scope.hsnCodeList = [];
    cboService.getHNSCbo(function (response) {
        $scope.hsnCodeList = response;
    });
    cboService.getTestinStdCbo(null, function (response) {
        $scope.testingStdList = response;
    });
    $scope.activityList = [];
    $scope.getActivityList = function () {
        cboService.GetBudgetMasterActivityLevelCbo($scope.materialMasterNew.BudgetMasterId, null, function (response) {
            $scope.activityList = response;
        });
    }
    $scope.oirUoMList = [];
    $scope.createUomList = function () {
        var PurchaseOrderUOMId = $scope.materialMasterNew.PurchaseOrderUOMId;
        var StockUOMId = $scope.materialMasterNew.StockUOMId;
        var SalesOrderUOMId = $scope.materialMasterNew.SalesOrderUOMId;
        var OIRUoMId = $scope.materialMasterNew.OIRUoMId;
        $scope.oirUoMList = oirUoMListCreate($scope.materialMasterNew.BaseUOMId, $scope.materialMasterAlternativeUOMs);
        //selectValueInUomList(PurchaseOrderUOMId, $scope.oirUoMList);
        //selectValueInUomList(StockUOMId, $scope.oirUoMList);
        //selectValueInUomList(SalesOrderUOMId, $scope.oirUoMList);
        //selectValueInUomList(OIRUoMId, $scope.oirUoMList);
    }
    $scope.createNewUomList = function () {
        $scope.materialMasterNew.PurchaseOrderUOMId = null;
        $scope.materialMasterNew.StockUOMId = null;
        $scope.materialMasterNew.SalesOrderUOMId = null;
        var OIRUoMId = $scope.materialMasterNew.OIRUoMId;
        $scope.oirUoMList = oirUoMListCreate($scope.materialMasterNew.BaseUOMId, $scope.materialMasterAlternativeUOMs);
        //selectValueInUomList(PurchaseOrderUOMId, $scope.oirUoMList);
        //selectValueInUomList(StockUOMId, $scope.oirUoMList);
        //selectValueInUomList(SalesOrderUOMId, $scope.oirUoMList);
        //selectValueInUomList(OIRUoMId, $scope.oirUoMList);
    }
    function oirUoMListCreate(baseUoM, altList) {
        var list = [];
        if (!baseService.isUndefinedOrNull(baseUoM)) {
            list.push({
                Value: baseUoM,
                Text: $scope.baseUOM
            });
            for (var i = 0; i < baseService.arrayLength(altList); i++) {
                list.push({
                    Value: altList[i].AlternativeUOMId,
                    Text: altList[i].AlternativeUOMName
                });
            }
        }
        return list;
    }
    function selectValueInUomList(value, list) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].Value == value)
                return value
        }
        return;
    }

    $scope.GetToUoMFactor = function () {
        cboService.getToUoMFactor($scope.materialMasterAlternativeUOMNew.AlternativeUOMId, $scope.materialMasterNew.BaseUOMId, function (response) {
            $scope.materialMasterAlternativeUOMNew.BaseUOMFactor = response.data
        });
    }

    $scope.attributePropertyList = [];
    cboService.getEnumCbo("enum/GetAttributePropertiesCbo", function (result) {
        $scope.attributePropertyList = result;
    });
    // #endregion

    function uomValidation(id, name) {
        if (baseService.isUndefinedOrNull(id)) {
            return;
        }
        var flag = false;
        for (var i = 0; i < $scope.materialMasterAlternativeUOMs.length; i++) {
            if ($scope.materialMasterAlternativeUOMs[i].AlternativeUOMId == id) {
                flag = true;
                break;
            }
        }
        if (!flag && $scope.materialMasterNew.BaseUOMId != id) {
            $scope.materialMasterNew[name] = null;
            ShowResult('This value is not exist in base or alternative uom grid.!', 'failure');
        }
    }
    $scope.putValueInAltUom = function () {
        $scope.materialMasterNew.PurchaseOrderUOMId = $scope.materialMasterNew.BaseUOMId;
        $scope.materialMasterNew.StockUOMId = $scope.materialMasterNew.BaseUOMId;
        $scope.materialMasterNew.SalesOrderUOMId = $scope.materialMasterNew.BaseUOMId;
        $scope.materialMasterNew.OIRUoMId = $scope.materialMasterNew.BaseUOMId;
        $scope.baseUOM = document.getElementById("baseUOMId").options[document.getElementById('baseUOMId').selectedIndex].text;
    }

    $scope.Get = function (id, index) {
        $scope.clearImage();
        $scope.setTab(1);
        $scope.gethierarchyList = null;
        $scope.attributeList = [];
        $scope.materialAttributeMasters = [];
        $scope.productMasters = [];
        $scope.prdNameList = null;
        $scope.productMastertbl = false;
        $scope.processRoutingList = [];
        $scope.processRoutingTbl = false;
        $scope.index = index;
        angular.copy($scope.materialMasters[$scope.index], $scope.materialMaster);
        angular.copy($scope.materialMaster, $scope.materialMasterNew);
        GetBusinessProcessList();
        GetRevenueBudget();
        $scope.getActivityList();
        $scope.GetAlternativeUomListByMaterialMaster($scope.materialMasterNew.Id);
        $scope.baseUOM = $scope.materialMasters[$scope.index].BaseUom;
        //$scope.ChangeOnMaterialType();
        if ($scope.materialMasterNew.WithSKU) {
            getMaterialMasterCharacteristics();
            GetCharacteristicsValueListByMaterialMaster(id);
        }
        else {
            $scope.dimensionList = [];
            $scope.characteristicsValueList = [];
        }
        getMaterialMasterAttribute();
        getMaterialMstProcess();
        //$scope.createUomList();
        if (!baseService.isUndefinedOrNull($scope.materialMasterNew.Image))
            $scope.imageSrc = virtualPath.MaterialsImage + $scope.materialMasterNew.Image;

        GetMaterialUsedData($scope.materialMasterNew.Id);

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.UsedInPO = false;
    $scope.UsedInGRN = false;
    $scope.UsedInBOM = false;
    $scope.UsedSKUInBOM = false;

    function GetMaterialUsedData(masterId) {
        $scope.UsedInPO = false;
        $scope.UsedInGRN = false;
        $scope.UsedInBOM = false;
        $scope.UsedSKUInBOM = false;

        $http.get($scope.path + 'GetMaterialUsedData?masterId=' + masterId)
            .then(function (response) {
                if (baseService.arrayLength(response.data.PO) > 0) {
                    $scope.UsedInPO = true;
                }
                if (baseService.arrayLength(response.data.GRN) > 0) {
                    $scope.UsedInGRN = true;
                }
                if (baseService.arrayLength(response.data.BOM) > 0) {
                    $scope.UsedInBOM = true;
                }
                if (baseService.arrayLength(response.data.CharacteristicsUsingInBOM) > 0) {
                    $scope.UsedSKUInBOM = true;
                }

            });
    }


    function GetRevenueBudget() {
        $scope.revenuList = [];
        $http({
            method: 'GET',
            url: 'Materials/materialmaster/getrevenuebudget/',
            params: { materialMasterId: $scope.materialMasterNew.Id }
        }).then(function successCallback(response) {
            $scope.revenuList = response.data;
        });
    }


    $scope.materialAttributeMasterListForSave = [];
    function combinematerialAttributeMasterList(list) {
        angular.forEach(list, function (item, key) {
            item.Sequence = key + 1;
            $scope.materialAttributeMasterListForSave.push(item);
        });
    }

    $scope.Save = function () {
        try {
            $scope.materialAttributeMasterListForSave = [];
            uomValidation($scope.materialMasterNew.PurchaseOrderUOMId, 'PurchaseOrderUOMId');
            uomValidation($scope.materialMasterNew.SalesOrderUOMId, 'SalesOrderUOMId');
            uomValidation($scope.materialMasterNew.StockUOMId, 'StockUOMId');
            combinematerialAttributeMasterList($scope.materialAttributeMasters);
            if ($scope.materialMasterNew.IsRevenue && baseService.arrayLength($scope.revenuList) === 0)
                return ShowResult('Please insert revenue budget.');
            if (baseService.arrayLength($scope.masterProcessSetList) > 0) {
                isJobWorkType($scope.masterProcessSetList);
                var isBaseProcess = false;
                for (var i = 0; i < baseService.arrayLength($scope.masterProcessSetList); i++) {
                    if ($scope.masterProcessSetList[i].IsBaseProcess) {
                        isBaseProcess = true;
                        break;
                    }
                    isBaseProcess = false;
                }
                if (!isBaseProcess) throw 'Please select base process';
            }

            $scope.$broadcast('show-errors-check-validity');
            reDirectToRequiredTab();
            if ($scope.materialMasterNew.WithSKU && baseService.arrayLength($scope.dimensionList) === 0)
                throw 'Please insert characteristics';
            if ($scope.materialMasterNewForm.$valid) {
                angular.copy($scope.materialMasterNew, $scope.materialMaster);
                var formData = new FormData();
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        headers: { 'Content-Type': undefined },
                        data: {
                            'materialMaster': $scope.materialMaster
                            , 'materialMasterAlternativeUOM': $scope.materialMasterAlternativeUOMs
                            , 'materialMasterAttribute': $scope.materialAttributeMasterListForSave
                            , 'attributeValueList': $scope.materialValues
                            , 'materialMasterCharacteristics': $scope.dimensionList
                            , 'characteristicsValue': $scope.characteristicsValueList
                            , 'materialMasterProcessRouting': $scope.processRoutingList
                            , 'masterProcessSetList': $scope.masterProcessSetList
                            , 'businessProcesses': $scope.businessProcesses
                            , 'revenuList': $scope.revenuList

                            , 'file': $scope.filedata
                        },
                        transformRequest: function (data) {
                            formData.append('materialMaster', JSON.stringify(data.materialMaster));
                            formData.append('materialMasterAlternativeUOM', JSON.stringify(data.materialMasterAlternativeUOM));
                            formData.append('materialMasterAttribute', JSON.stringify(data.materialMasterAttribute));
                            formData.append('attributeValueList', JSON.stringify(data.attributeValueList));
                            formData.append('materialMasterCharacteristics', JSON.stringify(data.materialMasterCharacteristics));
                            formData.append('characteristicsValue', JSON.stringify(data.characteristicsValue));
                            formData.append('materialMasterProcessRouting', JSON.stringify(data.materialMasterProcessRouting));
                            formData.append('masterProcessSetList', JSON.stringify(data.masterProcessSetList));
                            formData.append('businessProcesses', JSON.stringify(data.businessProcesses));
                            formData.append('revenuList', JSON.stringify(data.revenuList));

                            formData.append('file', data.file);
                            return formData;
                        }
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getData();
                            ClearFields(response.data.Sequence);
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
                else if ($scope.Action === "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        headers: { 'Content-Type': undefined },
                        data: {
                            'materialMaster': $scope.materialMaster
                            , 'materialMasterAlternativeUOM': $scope.materialMasterAlternativeUOMs
                            , 'materialMasterAttribute': $scope.materialAttributeMasterListForSave
                            , 'attributeValueList': $scope.materialValues
                            , 'materialMasterCharacteristics': $scope.dimensionList
                            , 'characteristicsValue': $scope.characteristicsValueList
                            , 'materialMasterProcessRouting': $scope.processRoutingList
                            , 'masterProcessSetList': $scope.masterProcessSetList
                            , 'businessProcesses': $scope.businessProcesses
                            , 'revenuList': $scope.revenuList
                            , 'file': $scope.filedata
                        },
                        transformRequest: function (data) {
                            formData.append('materialMaster', JSON.stringify(data.materialMaster));
                            formData.append('materialMasterAlternativeUOM', JSON.stringify(data.materialMasterAlternativeUOM));
                            formData.append('materialMasterAttribute', angular.toJson(data.materialMasterAttribute));
                            formData.append('attributeValueList', JSON.stringify(data.attributeValueList));
                            formData.append('materialMasterCharacteristics', angular.toJson(data.materialMasterCharacteristics));
                            formData.append('characteristicsValue', JSON.stringify(data.characteristicsValue));
                            formData.append('materialMasterProcessRouting', JSON.stringify(data.materialMasterProcessRouting));
                            formData.append('masterProcessSetList', JSON.stringify(data.masterProcessSetList));
                            formData.append('businessProcesses', JSON.stringify(data.businessProcesses));
                            formData.append('revenuList', JSON.stringify(data.revenuList));
                            formData.append('file', data.file);
                            return formData;
                        }
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                //$scope.materialMasters[$scope.index] = $scope.materialMaster;
                                $scope.getData();
                            }
                            ClearFields(response.data.Sequence);
                        }
                    }, function (response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, '')
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.materialMaster.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.materialMaster.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.materialMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    // #endregion *********Material Master End*********//

    // #region ProductMaster
    $scope.productMastertbl = false;
    $scope.productMasterList = [];
    $scope.productMasters = [];
    $http({
        method: 'GET',
        url: 'Products/productmaster/getcbo/',
    }).then(function successCallback(response) {
        $scope.productMasterList = response.data.Rows;
    });
    $scope.ChangeOnProductMaster = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $http({
                method: 'GET',
                url: 'Products/productmaster/ProductMasterWithDetails?productMasterId=' + id,
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.productMasters = response.data;
                    $scope.prdNameList = $scope.getUniqueColumn($scope.productMasters);
                    $scope.productMastertbl = true;
                }
                else {
                    productMasterCombinationData(id);
                    $scope.productMastertbl = false;
                }
            });
        }
    }
    function productMasterCombinationData(id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $http({
                method: 'GET',
                url: 'Products/productmaster/ProductMasterComminationData?productMasterId=' + id,
            }).then(function successCallback(response) {
                $scope.prdNameList = response.data;
            });
        }
    }
    // #endregion

    // #region Material Group
    function gethierarchy(id) {
        if (!baseService.isUndefinedOrNull(id)) {
            //$http({
            //    method: 'GET',
            //    url: 'Materials/materialgroupmaster/gethierarchy?id=' + id,
            //}).then(function successCallback(response) {
            //    if (response.data.Rows.length > 0) {
            //        $scope.gethierarchyList = response.data.Rows[0].Hierarchy;
            //    }
            //    else
            //        $scope.gethierarchyList = null;
            //});
        }
    }
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
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
    $scope.popUp = function () {
        if (baseService.isUndefinedOrNull($scope.materialMasterNew.MaterialTypeId))
            return ShowResult('Please select at first material type.!', '')
        $scope.popUpUrl = 'Materials/materialgroupmaster/getlistbymaterialtype?materialTypeId=' + $scope.materialMasterNew.MaterialTypeId;
        baseService.setCurrentPage('popUpDataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    }
    $scope.selectDoubleClick = function (data) {
        $scope.materialMasterNew.MaterialGroupMasterId = data.Id;
        $scope.materialMasterNew.MaterialGroupMasterName = data.UserName;
        $scope.materialMasterNew.HSNCodeId = data.HSNCodeId !== '' ? data.HSNCodeId : $scope.materialMasterNew.HSNCodeId;
        //getAttributeByMaterialGroup();
        $scope.closePopUp();
    };
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    }
    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        }
        $scope.selectDoubleClick($scope.valueData)
        $scope.closePopUp();
    }
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    }
    $scope.materialGroupClear = function () {
        $scope.materialMasterNew.MaterialGroupMasterId = null;
        $scope.materialMasterNew.MaterialGroupMasterName = null;
        $scope.materialAttributeMasters = [];
    }
    // #endregion

    // #region ChangeOnMaterialType
    //$scope.ChangeOnMaterialType = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'Materials/materialmaster/getvalidation?materialTypeId=' + $scope.materialMasterNew.MaterialTypeId,
    //    }).then(function successCallback(response) {
    //        if (response.data[0].IsCodeAutoGenerate) {
    //            $scope.materialTypeCheck = true;
    //            $scope.byCode = false;
    //        }
    //        else {
    //            $scope.materialTypeCheck = false;
    //            $scope.byCode = true;
    //        }
    //        if (!response.data[0].IsProductMstRequired)
    //            $scope.materialMasterNew.ProductMasterId = null;
    //        //$scope.ChangeOnProductMaster($scope.materialMasterNew.ProductMasterId)
    //        $scope.productMst = response.data[0].IsProductMstRequired;
    //        $scope.productMstMandatory = response.data[0].IsProductMstMandatory;
    //        if (!response.data[0].IsProcessRequired)
    //            $scope.materialMasterNew.ProcessId = null;
    //        $scope.process = response.data[0].IsProcessRequired;
    //        $scope.processMandatory = response.data[0].IsProcessMandatory;
    //        $scope.processRoutingTabe = response.data[0].IsProcessRouting;
    //        if ($scope.processRoutingTabe) {
    //            getProcessRouting();
    //        }
    //    });
    //};

    $scope.ChangeMaterialType = function () {
        $scope.materialMasterNew.MaterialGroupMasterId = null;
        $scope.materialMasterNew.MaterialGroupMasterName = null;
    };

    // #endregion

    // #region Process Routing
    $scope.processRoutingTbl = false;
    $scope.processRoutingList = [];
    function getProcessRouting() {
        $http({
            method: 'GET',
            url: 'Materials/materialmaster/getprocessroutinglist?materialMasterId=' + $scope.materialMasterNew.Id,
        }).then(function successCallback(response) {
            if (response.data.length != 0) {
                $scope.processRoutingTbl = true;
                $scope.processRoutingList = response.data;
            }
        });
    }

    // #endregion

    // #region materialGrid
    $scope.skuChange = function () {
        if (!$scope.materialMasterNew.WithSKU) {
            $scope.dimensionList = [];
            ClearMaterialDimension()
        }
    }
    // #endregion

    // #region MaterialMasterAlternativeUOM
    $scope.altUOMtbl = false;
    $scope.baseUOMDisable = false;
    $scope.materialMasterAlternativeUOMs = [];
    $scope.materialMasterAlternativeUOM = {
        Id: null,
        MaterialMasterId: null,
        AlternativeUOMId: null,
        AlternativeUOMName: null,
        AlternativeUOMFactor: 1,
        BaseUOMId: null,
        BaseUOMName: null,
        BaseUOMFactor: null,
        Active: true,
        Archive: false,
        UsedUomInBOM: null,
        UsedUomInGRN: null,
        UsedUomInPO:null
    };
    $scope.materialMasterAlternativeUOMNew = angular.copy($scope.materialMasterAlternativeUOM);
    $scope.GetAlternativeUomListByMaterialMaster = function (id) {
        $http({
            method: 'GET',
            url: 'Materials/materialmaster/getmaterialmasteraltuomlist?materialmasterid=' + id,
        }).then(function successCallback(response) {
            $scope.materialMasterAlternativeUOMs = response.data;
            if (response.data.length > 0) {
                $scope.altUOMtbl = true;
                $scope.baseUOMDisable = true;
            }
            else
                $scope.altUOMtbl = false;
            $scope.createUomList();
        });
    }
    $scope.GetMaterialMasterAlternativeUom = function (id, index) {
        $scope.altUomIndex = index;
        $scope.materialMasterAlternativeUOM = $scope.materialMasterAlternativeUOMs[$scope.altUomIndex];
        $scope.materialMasterAlternativeUOMNew = angular.copy($scope.materialMasterAlternativeUOM);
        $scope.AltUomAction = 'Update Alternative UOM';
    }
    $scope.addRow = function () {
        try {
            if ($scope.materialMasterNew.BaseUOMId == null) {
                throw 'Please select base uom from uom tab';
            }
            if ($scope.materialMasterAlternativeUOMNew.AlternativeUOMId == null) {
                throw 'Please select alternative uom';
            }
            if ($scope.materialMasterNew.BaseUOMId == $scope.materialMasterAlternativeUOMNew.AlternativeUOMId) {
                throw 'Base uom and alternative uom can not be same. Please select another alternative uom.';
            }
            var isAvailable = false;
            $scope.altUomName = document.getElementById("altUOMId").options[document.getElementById('altUOMId').selectedIndex].text;
            $scope.baseUOM = document.getElementById("baseUOMId").options[document.getElementById('baseUOMId').selectedIndex].text;
            for (var i = 0; i < $scope.materialMasterAlternativeUOMs.length; i++) {
                isAvailable = listValidation($scope.materialMasterAlternativeUOMs[i].AlternativeUOMId,
                    $scope.materialMasterAlternativeUOMNew.AlternativeUOMId,
                    $scope.materialMasterAlternativeUOMs[i].Archive, i);
                if (isAvailable) {
                    throw 'This alternative uom : [' + $scope.altUomName + '] has been already taken. Please select another alternative uom';
                }
            }
            if ($scope.materialMasterAlternativeUOMs > -1) {
                $scope.baseUOMDisable = true;
            }
           // if ($scope.materialMasterAlternativeUOMNew.BaseUOMFactor > 0) {
                angular.copy($scope.materialMasterAlternativeUOMNew, $scope.materialMasterAlternativeUOM);
                // isAvailable true == add new
                if (!isAvailable) {
                    if ($scope.altUomIndex == -1) {
                        this.materialMasterAlternativeUOM.Id = null;
                        this.materialMasterAlternativeUOM.AlternativeUOMId = $scope.materialMasterAlternativeUOMNew.AlternativeUOMId;
                        this.materialMasterAlternativeUOM.AlternativeUOMName = $scope.altUomName;
                        this.materialMasterAlternativeUOM.BaseUOMId = $scope.materialMasterNew.BaseUOMId;
                        this.materialMasterAlternativeUOM.BaseUOMName = $scope.baseUOM;
                        this.materialMasterAlternativeUOM.Active = true;
                        this.materialMasterAlternativeUOM.UsedUomInPO = null;
                        this.materialMasterAlternativeUOM.UsedUomInGRN = null;
                        this.materialMasterAlternativeUOM.UsedUomInBOM = null;
                        $scope.materialMasterAlternativeUOMs.push($scope.materialMasterAlternativeUOM);
                        clearAltUOM();
                        $scope.altUOMtbl = true;
                    }
                    else {
                        $scope.materialMasterAlternativeUOMs[$scope.altUomIndex] = this.materialMasterAlternativeUOM;
                        $scope.materialMasterAlternativeUOMs[$scope.altUomIndex].AlternativeUOMName = $scope.altUomName;
                        $scope.materialMasterAlternativeUOMs[$scope.altUomIndex].BaseUOMName = $scope.baseUOM;
                        $scope.altUomIndex = -1;
                        clearAltUOM();
                    }
                    $scope.AltUomAction = 'Add Alternative UOM';
                    $scope.index = -1;
                }
            //} //else
                //throw 'Please insert base uom factor';
        } catch (err) {
            ShowResult(err, 'failure');
        }
    }

    //Check Alt UOM List
    function listValidation(oldValue, newValue, archive, index) {
        var isAvailable = false;
        // Id
        if ($scope.altUomIndex == -1) {
            if (!archive) {
                if (oldValue == newValue) {
                    isAvailable = true;
                    return isAvailable;
                }
            }
        }
        else {
            if ($scope.altUomIndex != index) {
                if (archive) {
                    if (oldValue == newValue) {
                        isAvailable = true;
                        return isAvailable;
                    }
                }
            }
        }
        return isAvailable;
    }
    $scope.valuePassInDelModal = function (id, index, altUomName) {
        $scope.mauid = id;
        $scope.mauindex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + altUomName + ' ]';
        angular.element(document.querySelector('#mmaltuom')).modal('show');
    };
    $scope.removeRow = function () {
        $scope.materialMasterAlternativeUOMs.splice($scope.mauindex, 1);
        $scope.createUomList();
        $scope.AltUomAction = 'Add Alternative UOM';
        $scope.mauid = null;
        $scope.mauindex = -1;
    };

    function clearAltUOM() {
        $scope.materialMasterAlternativeUOMNew.AlternativeUOMId = null;
        $scope.materialMasterAlternativeUOMNew.BaseUOMFactor = null;
        $scope.materialMasterAlternativeUOM = {};
    }
    // #endregion

    // #region DocumentDelete

    $scope.DocumentRemove = function (id) {
        $scope.idd = id;
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmDocDelete')).modal('show');
        $scope.filedata = {};
    };
    $scope.removeDoc = function () {
        angular.element(document.querySelector('#confirmDocDelete')).modal('hide');
        $http({
            method: 'POST',
            url: 'Materials/MaterialMaster/deletedocument?Id=' + $scope.idd,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.materialMasterNew.Image = "";
                $scope.clearImage();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };
    $scope.confirmCloseDocDelete = function () {
        angular.element(document.querySelector('#confirmDocDelete')).modal('hide');
    };

    // #endregion

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.materialMaster = {};
        $scope.materialMasterNew = {
            Sequence: seq
            , IsAsset: false
            , IsOriginApplicable: false
            , Active: true
            , IsProduct: false
            , IsRevenue: false
            , IsInventory: false
            , IsExpenseOut: false
            , WithSKU: false
            , IsRegular: true
        };
        $scope.materialMasterAlternativeUOMs = [];
        $scope.baseUOM = null;
        $scope.baseUOMDisable = false;
        $scope.altUOMtbl = false;
        $scope.productMst = false;
        $scope.attributeList = [];
        $scope.gethierarchyList = null;
        $scope.processRoutingTbl = false;
        $scope.processRoutingList = [];
        $scope.productMasters = [];
        $scope.prdNameList = null;
        $scope.productMastertbl = false;
        $scope.processRoutingTabe = false;
        $scope.subMaterialTblList = [];
        $scope.materialAttributetbl = false;
        $scope.processSetList = [];
        $scope.process = false;
        $scope.isSet(1);
        $scope.setTab(1);
        GetBusinessProcessList();
        $scope.businessProcessList = [];
        $scope.revenuList = [];
        $scope.materialAttributeMasters = [];
        $scope.materialValues = [];
        $scope.dimensionList = [];
        $scope.ClearMaterialMasterAttribute();
        $scope.clearImage();
        $rootScope.tempList = [];
        $scope.filedata = null;
        $scope.materialValues = [];
        $scope.characteristicsValueList = [];
        $scope.UsedInPO = false;
        $scope.UsedInGRN = false;
        $scope.UsedInBOM = false;
        $scope.UsedSKUInBOM = false;
    }

    function reDirectToRequiredTab() {
        if ($scope.materialMasterFormTab1.$invalid) $scope.setTab(1);
        else if ($scope.materialMasterFormTab2.$invalid) $scope.setTab(2);
        else if ($scope.materialMasterFormTab3.$invalid) $scope.setTab(3);
        else if ($scope.materialMasterFormTab4.$invalid) $scope.setTab(4);
        else if ($scope.materialMasterFormTab5.$invalid) $scope.setTab(5);
        else if ($scope.materialMasterFormTab6.$invalid) $scope.setTab(6);
        else if ($scope.materialMasterFormTab7.$invalid) $scope.setTab(7);
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

    // #region Process Set
    $scope.masterProcessSetList = [];
    $scope.processPetParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'Entity,ProcessCategory,ProcessCriteria,Code,Description'
        , searchBy: "Code"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.processSetPopUp = function () {
        $scope.popUpList = [];
        $scope.popUpUrl = 'Processes/ProcessSet/GetDataList';
        baseService.setCurrentPage('dataList');
        $scope.getProcessSetList = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.processPetParameters)
                .then(function (result) {
                    $scope.processSetList = result.Rows;
                    $scope.processPetParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processSetPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processSetPopUp')).modal('show');
        $scope.getProcessSetList();
    }
    $scope.selectProcessSet = function (data) {
        getProcessSetList(data.Id)
        angular.element(document.querySelector('#processSetPopUp')).modal('hide');
    }

    function getProcessSetList(id) {
        $scope.masterProcessSetList = [];
        $http({
            method: 'GET',
            url: 'Processes/processset/GetProcessSetList?processSetId=' + id
        }).then(function successCallback(response) {
            $scope.masterProcessSetList = response.data;
        });
    }

    // #endregion

    // #region Process
    $scope.processSearchList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
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
            'name': 'Local Name',
            'value': 'LocalName'
        },
        {
            'name': 'Alias',
            'value': 'Alias'
        }
    ];
    $scope.processPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Sequence',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.processPopUp = function () {
        $scope.popUpProcessUrl = 'Processes/Process/GetProductionProcessList';
        $scope.getProcessData = function (pageno) {
            baseService.paginationBase($scope.popUpProcessUrl, pageno, $scope.processPopUpParameters)
                .then(function (result) {
                    $scope.processPopUpDataList = result.Rows;
                    $scope.processPopUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processPopUp')).modal('show');
        $scope.getProcessData();
    }
    $scope.closeProcessPopUp = function () {
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    }

    $scope.processDelModal = function (data, index) {
        $scope.processIndex = index;
        $scope.processMessage = 'Are you sure want to permanently delete [ ' + data.ProcessName + ' ]?';
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
    };
    $scope.removeProcessRow = function () {
        $scope.masterProcessSetList.splice($scope.processIndex, 1);
        $scope.processIndex = -1;
    };
    $scope.processAdd = function (data) {
        $scope.masterProcessSetList.push({
            Id: null
            , MaterialMasterId: $scope.materialMasterNew.Id
            , ProcessId: data.Id
            , ProcessName: data.UserName
            , Sequence: $scope.masterProcessSetList.length + 1
            , IsBaseProcess: false
            , Days: 0
            , Symbol: '+'
            , ProductionCycleTime: 1
            , JobWorkApplicable: false
            , JobWorkType: null
            , EntityOrVendorId: null
            , EntityOrVendorName: null
            , Archive: false
            , class: 'new'
            , setDisable: true

        });
    };
    $scope.setPlusOrMinus = function (event, index) {
        for (var i = 0; i <= $scope.masterProcessSetList.length - 1; i++) {
            if (i < index) {
                $scope.masterProcessSetList[i].Symbol = '-';
                $scope.masterProcessSetList[i].IsBaseProcess = false;
            }
            else if (i > index) {
                $scope.masterProcessSetList[i].Symbol = '+';
                $scope.masterProcessSetList[i].IsBaseProcess = false;
            }
            else if (i === index) {
                $scope.masterProcessSetList[i].Symbol = null;
                $scope.masterProcessSetList[i].Days = 0;
                $scope.masterProcessSetList[i].IsBaseProcess = true;
            }
        }
    };
    function daysSortValidation(list) {
        try {
            var seq = 0;
            var seqNeg = 0;
            var isNeg = true;
            if (list[0].Days === 0) {
                isNeg = false;
            } else {
                seqNeg = parseInt(list[0].Days);
                seqNeg += 1;
            }
            for (var i = 0; i < list.length; i++) {
                if (isNeg === false) {//0,1,2
                    if (list[i].Days >= seq) {
                        seq = list[i].Days;
                    }
                    else//0,1,3,2
                        throw "Lag days sequence is not valid.....!";
                }
                else //2,1,0,1,2 or2,1,0
                {
                    if (list[i].Days <= seqNeg) {//2,1,0
                        seqNeg = list[i].Days;
                        if (list[i].Days === 0) {
                            isNeg = false;
                            seq = 0;
                        }
                    }
                    else {
                        //2,3,1,0,1,2
                        throw "Lag days sequence is not valid.....!";
                    }
                }
            }
        } catch (e) {
            throw e;
        }
    }
    function isJobWorkType(list) {
        try {
            for (var i = 0; i < list.length; i++) {
                if (list[i].JobWorkApplicable && list[i].JobWorkType === null
                    //&& (list[i].EntityIdWithinCompany === null || list[i].EntityIdWithinGroup === null || list[i].PartyId === null)
                ) {
                    throw 'Please select job work type!';
                }
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.clearEntityOrVendor = function (list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id && !list[i].Archive) {
                list[i].EntityIdWithinCompany = null;
                list[i].EntityIdWithinGroup = null;
                list[i].PartyId = null;
                list[i].EntityOrVendorName = null;
                break;
            }
        }
    };
    $scope.clearJobType = function (list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                list[i].JobWorkType = null;
                break;
            }
        }
    };
    $scope.SetDisable = function (id) {
        for (var i = 0; i < $scope.masterProcessSetList.length; i++) {
            if ($scope.masterProcessSetList[i].Id === id) {
                if ($scope.masterProcessSetList[i].JobWorkApplicable)
                    return $scope.masterProcessSetList[i].setDisable = false;
                else
                    return $scope.masterProcessSetList[i].setDisable = true;
            }
        }
    };
    function getMaterialMstProcess() {
        $http({
            method: 'GET',
            url: 'Materials/materialmaster/getmaterialmstprocess?materialMasterId=' + $scope.materialMasterNew.Id,
        }).then(function successCallback(response) {
            $scope.masterProcessSetList = response.data;
        });
    }
    // #endregion

    //#region Job Work PopUp
    cboService.getEnumCbo("enum/GetJobWorkTypeListCbo", function (result) {
        $scope.jobWorkTypeList = result;
    });

    //#endregion

    // #region
    $scope.searchbyixedAssetMasterList = [];
    $scope.fixedAssetMasterListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'FixedAssetMasterName',
        searchBy: "FixedAssetMasterName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getFixedAssetMasterData = function () {
        baseService.setCurrentPage('fixedAssetMasterList');
        $scope.loadFixedAssetMasterData = function (pageno) {
            $scope.fixedAssetMasterListParameters.ids = JSON.stringify([]);
            baseService.paginationBase('fixedassets/fixedassetmaster/getlistfordynamicpopup', pageno, $scope.fixedAssetMasterListParameters)
                .then(function (result) {
                    $scope.fixedAssetMasterList = result.Rows;
                    $scope.fixedAssetMasterListParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.searchbyixedAssetMasterList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyixedAssetMasterList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#fixedAssetMasterModal')).modal('show');
        $scope.loadFixedAssetMasterData();
    };
    $scope.selectAssetMaster = function (data) {
        $scope.materialMasterNew.AssetMasterId = null;
        $scope.materialMasterNew.AssetMasterName = null;
        $scope.materialMasterNew.BudgetMasterId = null;
        $scope.materialMasterNew.AssetBudgetCode = null;
        $scope.materialMasterNew.ActivityId = null;

        $scope.materialMasterNew.AssetMasterId = data.FixedAssetMasterId;
        $scope.materialMasterNew.FixedAssetMasterId = data.FixedAssetMasterId;
        $scope.materialMasterNew.AssetMasterName = data.FixedAssetMasterName;
        $rootScope.tempList = [];
        angular.element(document.querySelector('#fixedAssetMasterModal')).modal('hide');
    }
    $scope.isAsset = function (event) {
        if (!event.currentTarget.checked) {
            $scope.materialMasterNew.AssetMasterId = null;
            $scope.materialMasterNew.AssetMasterName = null;
            $scope.materialMasterNew.BudgetMasterId = null;
            $scope.materialMasterNew.AssetBudgetCode = null;
            $scope.materialMasterNew.ActivityId = null;
        }
    }
    //#endregion

    //var move = function (origin, destination) {
    //    var temp = $scope.masterProcessSetList[destination];
    //    var symbolIndex = null;
    //    $scope.masterProcessSetList[destination] = $scope.masterProcessSetList[origin];
    //    $scope.masterProcessSetList[origin] = temp;
    //    //$scope.masterProcessSetList[origin].Sequence = destination + 1;
    //    for (var i = 0; i < $scope.masterProcessSetList.length; i++) {
    //        $scope.masterProcessSetList[i].Sequence = i + 1;
    //        if ($scope.masterProcessSetList[i].IsBaseProcess) {
    //            symbolIndex = i;
    //        }
    //    }
    //    $scope.setPlusOrMinus(null, symbolIndex);
    //};

    var move = function (origin, destination) {
        var temp = $scope.materialAttributeMasters[destination];
        var symbolIndex = null;
        $scope.materialAttributeMasters[destination] = $scope.materialAttributeMasters[origin];
        $scope.materialAttributeMasters[origin] = temp;
        //$scope.masterProcessSetList[origin].Sequence = destination + 1;
        for (var i = 0; i < $scope.materialAttributeMasters.length; i++) {
            $scope.materialAttributeMasters[i].Sequence = i + 1;
            //if ($scope.materialAttributeMasters[i].IsBaseProcess) {
            //    symbolIndex = i;
            //}
        }
        //$scope.setPlusOrMinus(null, symbolIndex);
    };

    $scope.moveUp = function (index) {
        move(index, index - 1);
    };
    $scope.moveDown = function (index) {
        move(index, index + 1);
    };


    var movesku = function (origin, destination) {
        var temp = $scope.dimensionList[destination];
        var symbolIndex = null;
        $scope.dimensionList[destination] = $scope.dimensionList[origin];
        $scope.dimensionList[origin] = temp;
        for (var i = 0; i < $scope.dimensionList.length; i++) {
            $scope.dimensionList[i].Sequence = i + 1;
        }
    };

    $scope.moveskuUp = function (index) {
        movesku(index, index - 1);
    };
    $scope.moveskuDown = function (index) {
        movesku(index, index + 1);
    };


    $scope.reverseIsNotRevenue = function () {
        if (!$scope.materialMasterNew.IsRevenue) {
            $scope.materialMasterNew.IsInventory = $scope.materialMasterNew.IsRevenue;
            $scope.materialMasterNew.IsExpenseOut = $scope.materialMasterNew.IsRevenue;
            $scope.revenuList = [];
        }
    };

    $scope.budgetPopUpList = [];
    $scope.assetBudgetPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode,GLGeneralInfoName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.assetBudgetPopUp = function () {
        $scope.valueData = '';
        $scope.assetBudgetPopUpUrl = 'accounts/glitem/GetATypeAssetAndReconAssetGLWithFixedAssetMaster?fixedAssetMasterId=' + $scope.materialMasterNew.AssetMasterId;
        baseService.setCurrentPage('dataList');
        $scope.getAssetBudgetPopUpData = function (pageno) {
            baseService.paginationBase($scope.assetBudgetPopUpUrl, pageno, $scope.assetBudgetPopUpParameters)
                .then(function (result) {
                    $scope.assetBudgetPopUpDataList = result.Rows;
                    $scope.assetBudgetPopUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.budgetPopUpList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.budgetPopUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'assetBudgetPopUpId');
                }).finally(function () {
                });
        };
        $scope.getAssetBudgetPopUpData();
        angular.element(document.querySelector('#assetBudgetPopUpId')).modal('show');
    };
    $scope.selectAssetBudgetDoubleClick = function (data) {
        $scope.materialMasterNew.BudgetMasterId = data.BudgetMasterId;
        $scope.materialMasterNew.AssetBudgetCode = data.BudgetName;
        $scope.getActivityList();
        $scope.closeAssetBudgetPopUp();
    };
    $scope.closeAssetBudgetPopUp = function () {
        $scope.valueData = '';
        $scope.assetBudgetPopUpParameters.search = null;
        angular.element(document.querySelector('#assetBudgetPopUpId')).modal('hide');
    };

    $scope.budgetPopUpList = [];
    $scope.assetBudgetPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode,GLGeneralInfoName',
        searchBy: "BudgetCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.revenueBudgetPopUp = function () {
        $scope.valueData = '';
        $scope.assetBudgetPopUpUrl = 'accounts/glitem/getatypeexpensegl';
        baseService.setCurrentPage('budgetList');
        $scope.getAssetBudgetPopUpData = function (pageno) {
            baseService.paginationBase($scope.assetBudgetPopUpUrl, pageno, $scope.assetBudgetPopUpParameters)
                .then(function (result) {
                    $scope.budgetList = result.Rows;
                    $scope.assetBudgetPopUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.budgetPopUpList) == 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.budgetPopUpList);
                    for (var a = 0; a < baseService.arrayLength($scope.revenuList); a++) {
                        for (var t = 0; t < baseService.arrayLength($scope.budgetList); t++) {
                            if ($scope.budgetList[t].BudgetMasterId === $scope.revenuList[a].BudgetMasterId)
                                $scope.budgetList.splice(t, 1);
                        }
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'revenueBudgetPopUpId');
                }).finally(function () {
                });
        };
        $scope.getAssetBudgetPopUpData();
        angular.element(document.querySelector('#revenueBudgetPopUpId')).modal('show');
    };
    $scope.revenuList = [];
    var tempList = [];
    $scope.pushTempList = function (data, event) {
        if (event.currentTarget.checked) tempList.push(data);
        else {
            for (var t = 0; t < baseService.arrayLength(tempList); t++) {
                if (tempList[t].BudgetMasterId === data.BudgetMasterId) return tempList.spalice(t, 1);
            }
        }
    };
    $scope.selectRevenue = function () {
        for (var t = 0; t < baseService.arrayLength(tempList); t++) {
            $scope.revenuList.push(tempList[t]);
        }
        tempList = [];
        angular.element(document.querySelector('#revenueBudgetPopUpId')).modal('hide');
    };
    $scope.removeRevenueRowModal = function (data) {
        $scope.data = data;
        $scope.revenueDelPopMsg = 'Are you sure want to delete this data.';
        angular.element(document.querySelector('#revenueDelPop')).modal('show');
    };
    $scope.removeRevenueRow = function () {
        for (var t = 0; t < baseService.arrayLength($scope.revenuList); t++) {
            if ($scope.revenuList[t].BudgetMasterId === $scope.data.BudgetMasterId) {
                $scope.revenuList.splice(t, 1);
                return;
            }
        }
    };
    $scope.closeRevenuePopUp = function () {
        $scope.valueData = '';
        $scope.assetBudgetPopUpParameters.search = null;
        angular.element(document.querySelector('#revenueBudgetPopUpId')).modal('hide');
    };
    // #endregion


    // #region Material Master Attribute
    $scope.joiningParameterList = [
        { Value: ", ", Text: "Comma(,)" },
        { Value: ", ", Text: "Comma Space(, )" },
        { Value: " ", Text: "Space()" },
        { Value: "/", Text: "Slash(/)" },
        { Value: "-", Text: "Hyphen(-)" },
        { Value: ":", Text: "Colon(:)" },
        { Value: "x", Text: "Multiply" }
    ];

    $scope.materialAttributeMasters = [];
    $scope.ChAction = 'Add Row';
    $scope.vindex = -1;
    $scope.materialAttributeList = [];
    $http({
        method: 'GET',
        url: 'Materials/materialattribute/getcbo',
        params: { 'valueAssignment': null }
    }).then(function successCallback(response) {
        $scope.materialAttributeList = response.data;
    });
    $scope.materialAttributeMaster = {
        Id: null
        , MaterialMasterId: null
        , MaterialGroupMasterId: null
        , MaterialAttributeId: null
        , MaterialAttributeName: null
        , Sequence: null
        , ValueAssignmentLevel: 'Specific'
        , AttributeProperty: null
        , IsFixedNoOfCharacter: null
        , NoOfCharacter: null
        , IsFreeField: true
        , IsPreDefinedField: true
        , IsMandatory: true
        , Active: true
        , CreationLevel: ''
        , JoiningSequence: 0
        , JoiningParameter:null
        , MaterialMasterAttributeValues: []
    };
    $scope.materialAttributeMasterNew = Object.assign({}, $scope.materialAttributeMaster);
    $scope.change = function () {
        var obj = $.grep($scope.materialAttributeList, function (item) {
            return item.Value === $scope.materialAttributeMasterNew.MaterialAttributeId;
        })[0];
        $scope.materialAttributeMasterNew.UserName = obj.MaterialAttributeName;
        $scope.materialAttributeMasterNew.ValueAssignmentLevel = obj.ValueAssignmentLevel;
        $scope.materialAttributeMasterNew.IsFreeField = obj.IsFreeField;
        $scope.materialAttributeMasterNew.IsPreDefinedField = obj.IsPreDefinedField;
        $scope.materialAttributeMasterNew.IsMandatory = obj.IsMandatory;
        $scope.materialAttributeMasterNew.IsFixedNoOfCharacter = obj.IsFixedNoOfCharacter;
        $scope.materialAttributeMasterNew.NoOfCharacter = obj.NoOfCharacter;
        $scope.materialAttributeMasterNew.AttributeProperty = obj.AttributeProperty;
    };
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
        angular.element(document.querySelector('#materialAttributeCreatePopUp')).modal('show');

    };

    $scope.createMatarialAttribute = function () {
        try {
            CloseShowResult();
            if ($scope.materialAttributeMasters.length > 19)
                throw 'Total no of material attribute can not be more than 20!';

            if ($scope.manualValidationAddRemove('div_attr_1', 'Sequence', $scope.materialAttributeMasterNew)) return;
            if ($scope.manualValidationAddRemove('div_attrjs_1', 'Joining Sequence', $scope.materialAttributeMasterNew)) return;
            if ($scope.manualValidationAddRemove('div_attr_2', 'Code', $scope.materialAttributeMasterNew)) return;
            if ($scope.manualValidationAddRemove('div_attr_3', 'Short Name', $scope.materialAttributeMasterNew)) return;
            if ($scope.manualValidationAddRemove('div_attr_4', 'Standard Name', $scope.materialAttributeMasterNew)) return;
            if ($scope.manualValidationAddRemove('div_attr_5', 'User Name', $scope.materialAttributeMasterNew)) return;
            if ($scope.manualValidationAddRemove('div_attr_6', 'Attribute Property', $scope.materialAttributeMasterNew, 'Property is required.')) return;

            var isAvailable = false;
            for (var i = 0; i < $scope.materialAttributeMasters.length; i++) {
                if (baseService.isAvailableInList($scope.materialAttributeMasters[i].Code, $scope.materialAttributeMasterNew.Code, i, $scope.chindex))
                    throw 'Code : [' + $scope.materialAttributeMasterNew.Code + '] has been already taken';
                isAvailable = baseService.isAvailableInList($scope.materialAttributeMasters[i].UserName, $scope.materialAttributeMasterNew.UserName, i, $scope.chindex);
                if (isAvailable) throw 'User name : [' + $scope.materialAttributeMasterNew.UserName + '] has been already taken';
            }
            angular.copy($scope.materialAttributeMasterNew, $scope.materialAttributeMaster);
            // isAvailable true == add new
            if (!isAvailable) {
                if ($scope.chindex === -1) {
                    $scope.materialAttributeMaster.Id = null;
                    $scope.materialAttributeMaster.MaterialMasterId = $scope.materialMasterNew.Id;
                    //$scope.materialAttributeMaster.MaterialGroupMasterId = $scope.materialMasterNew.MaterialGroupMasterId;
                    $scope.materialAttributeMaster.TempMaterialAttributeId = baseService.pk();
                    $scope.materialAttributeMaster.MaterialAttributeId = $scope.materialAttributeMaster.TempMaterialAttributeId;
                    $scope.materialAttributeMaster.CreationLevel = 'Material';
                    $scope.materialAttributeMaster.ValueAssignmentLevel = 'Specific';
                    $scope.materialAttributeMasters.push($scope.materialAttributeMaster);
                }
                else
                    $scope.materialAttributeMasters[$scope.chindex] = $scope.materialAttributeMaster;
                $scope.chindex = -1;
                $scope.ClearMaterialMasterAttribute();
            }
        } catch (err) {
            ShowResult(err, 'failure', 'materialAttributeCreatePopUp');
        }
    };

    $scope.editAttribute = function (index) {
        $scope.chindex = index;
        angular.copy($scope.materialAttributeMasters[$scope.chindex], $scope.materialAttributeMaster);
        angular.copy($scope.materialAttributeMaster, $scope.materialAttributeMasterNew);
        $scope.attrAction = 'Update Row';
        angular.element(document.querySelector('#materialAttributeCreatePopUp')).modal('show');
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

    function getAttributeByMaterialGroup() {
        $http({
            method: 'GET',
            url: 'Materials/materialattributemaster/GetListForMaterialMaster',
            params: { materialGroupMasterId: $scope.materialMasterNew.MaterialGroupMasterId }
        }).then(function successCallback(response) {
            $scope.materialAttributeMasters = [];
            $scope.materialValues = [];
            $scope.materialAttributeMasters = response.data;
        });
    }

    $scope.ClearMaterialMasterAttribute = function () {
        $scope.ChAction = 'Add Row';
        $scope.attrAction = 'Add Row';
        $scope.materialAttributeMaster = {};
        $scope.materialAttributeMasterNew = {
            ValueAssignmentLevel: null
            , Active: true
            , IsFreeField: true
            , IsPreDefinedField: true
            , IsMandatory: true
            , MaterialMasterAttributeValues: []
            , Sequence: $scope.materialAttributeMasters.length + 1
        };
        CloseModalShowResult('materialAttributeCreatePopUp');
    };

    $scope.CloseMaterialMasterAttribute = function () {
        $scope.ClearMaterialMasterAttribute();
        angular.element(document.querySelector('#materialAttributeCreatePopUp')).modal('hide');
    };

    function getMaterialMasterAttribute() {
        $http({
            method: 'GET'
            , url: $scope.path + 'GetMaterialMasterAttribute?masterId=' + $scope.materialMasterNew.Id
        }).then(function successCallback(response) {
            $scope.materialAttributeMasters = response.data;
            if (baseService.arrayLength(response.data) > 0)
                getMaterialMasterAttributeValue();
        }), function errorCallBack(response) {
        };
    }

    // #endregion Material Master Attribute

    // #region Material Master Attribute Value

    $scope.materialValues = [];
    $scope.mvindex = -1;
    $scope.chindex = -1;
    $scope.materialAttributeValuePoUp = function (charId, index) {
        $scope.materialValueAction = 'Add Row';
        $scope.charId = charId;
        CloseModalShowResult();
        $scope.chindex = index;
        $scope.attributeName = $scope.materialAttributeMasters[$scope.chindex].UserName;
        $scope.materialValue = {
            Id: baseService.pk()
            , CompanyGroupId: $window.companyGroupId
            , MaterialMasterId: $scope.materialMasterNew.Id
            , MaterialAttributeId: $scope.charId
            , Sequence: null
            , Code: null
            , ShortName: null
            , StandardName: null
            , UserName: null
            , SourceType: 'Specific'
            , Description: null
            , Remarks: null
            , IsDefault: false
            , Active: true
        };
        $scope.materialValueNew = angular.copy($scope.materialValue);
        $scope.GetMaterialMasterAttributeValueSequence();
        angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('show');
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
    $scope.addMasterAttributeValue = function () {
        try {
            CloseModalShowResult();
            if ($scope.manualValidationAddRemove('div_1', 'Sequence', $scope.materialValueNew)) return;
            if ($scope.manualValidationAddRemove('div_2', 'Code', $scope.materialValueNew)) return;
            if ($scope.manualValidationAddRemove('div_3', 'Short Name', $scope.materialValueNew)) return;
            if ($scope.manualValidationAddRemove('div_4', 'Standard Name', $scope.materialValueNew)) return;
            if ($scope.manualValidationAddRemove('div_5', 'User Name', $scope.materialValueNew)) return;
            var chList = $filter("filter")($scope.materialValues, { MaterialAttributeId: $scope.charId });
            for (var t = 0; t < baseService.arrayLength($scope.materialValues); t++) {
                if ($scope.materialValues[t].MaterialAttributeId === $scope.charId) {
                    duplicateCheck($scope.materialValues[t].Code, $scope.materialValueNew.Code, t, $scope.mvindex, 'Code');
                    duplicateCheck($scope.materialValues[t].ShortName, $scope.materialValueNew.ShortName, t, $scope.mvindex, 'ShortName');
                    duplicateCheck($scope.materialValues[t].StandardName, $scope.materialValueNew.StandardName, t, $scope.mvindex, 'StandardName');
                    duplicateCheck($scope.materialValues[t].UserName, $scope.materialValueNew.UserName, t, $scope.mvindex, 'UserName');
                    if ($scope.materialValueNew.IsDefault)
                        duplicateCheck($scope.materialValues[t].IsDefault, $scope.materialValueNew.IsDefault, t, $scope.mvindex, null, 'Default value already exist for ' + $scope.materialValues[t].UserName);
                }
            }

            checkPropertiesAndCharLength($scope.materialAttributeMasters[$scope.chindex], $scope.materialValueNew);
            angular.copy($scope.materialValueNew, $scope.materialValue);

            if ($scope.mvindex === -1) {
                $scope.materialValue.MaterialMasterId = $scope.materialMasterNew.Id;
                $scope.materialValue.MaterialAttributeId = $scope.charId;
                $scope.materialValues.push($scope.materialValue);
            }
            else
                $scope.materialValues[$scope.mvindex] = $scope.materialValue;


            if (!baseService.isUndefinedOrNull($scope.materialAttributeMasters[$scope.chindex].Id)) {
                $http({
                    method: 'POST'
                    , url: $scope.path + 'CreateValue'
                    , data: $scope.materialValue
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'materialAttributeValuePoUp');
                    else {
                        ShowResult(response.data.Message, 'success', 'materialAttributeValuePoUp');
                        if ($scope.mvindex === -1) {
                            $scope.materialValues[$scope.materialValues.length - 1].Id = response.data.Id;
                        }
                        else
                            $scope.materialValues[$scope.mvindex].Id = response.data.Id;
                        $scope.mvindex = -1;
                        ClearMasterAttributeValueFields(getMaxNumberFromList($filter("filter")($scope.materialValues, { MaterialAttributeId: $scope.charId }), 'Sequence'));
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'materialAttributeValuePoUp');
                };
            } else {
                $scope.mvindex = -1;
                ClearMasterAttributeValueFields(getMaxNumberFromList($filter("filter")($scope.materialValues, { MaterialAttributeId: $scope.charId }), 'Sequence'));
            }
        } catch (e) {
            ShowResult(e, '', 'materialAttributeValuePoUp');
        }
    };

    $scope.GetMaterialMasterAttributeValueSequence = function () {
        if (baseService.arrayLength($scope.materialValues) > 0)
            $scope.materialValueNew.Sequence = getMaxNumberFromList($filter("filter")($scope.materialValues, { MaterialAttributeId: $scope.charId }), 'Sequence');
        else {
            $http.get('Materials/MaterialAttributeValue/getautosequence?materialAttributeId=' + $scope.charId + '&materialId=' + $scope.materialMasterNew.Id)
                .then(function (response) {
                    $scope.materialValueNew.Sequence = response.data;
                });
        }
    };
    $scope.GetMaterialMasterAttributeValue = function (id, index) {
        $scope.materialValueAction = 'Update Row';
        $scope.mvindex = baseService.getIndexOf($scope.materialValues, id, 'Id');
        $scope.materialValue = $scope.materialValues[$scope.mvindex];
        $scope.materialValueNew = angular.copy($scope.materialValue);
    };
    $scope.CloseMaterialMasterAttributeValue = function () {
        $scope.materialAttributeMasters[$scope.chindex].MaterialMasterAttributeValues = [];
        $scope.chindex = - 1;
        CloseModalShowResult();
        angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('hide');
    };

    $scope.clearMasterAttributeValue = function () {
        ClearMasterAttributeValueFields(getMaxNumberFromList($scope.materialValues, 'Sequence'));
        return true;
    };
    function ClearMasterAttributeValueFields(seq) {
        $scope.mvindex = -1;
        $scope.materialValueAction = 'Add Row';
        $scope.materialValue = {};
        $scope.materialValueNew = {
            Id: baseService.pk()
            , MaterialMasterId: $scope.materialMasterNew.Id
            , MaterialAttributeId: $scope.charId
            , Sequence: seq, Active: true
            , IsDefault: false
            , SourceType: 'Specific'
            , CompanyGroupId: $window.companyGroupId
        };
    }
    function getMaterialMasterAttributeValue() {
        $http({
            method: 'GET'
            , url: $scope.path + 'GetMaterialMasterAttributeValue?masterId=' + $scope.materialMasterNew.Id
        }).then(function successCallback(response) {
            $scope.materialValues = response.data;
        }), function errorCallBack(response) {
        };
    }

    // #endregion Material Master Attribute Value

    // #region SKU

    $scope.dimensionList = [];
    $scope.characteristicsList = [];
    $http({
        method: 'GET',
        url: 'Materials/characteristics/getcbo',
        params: { 'valueAssignment': null }
    }).then(function successCallback(response) {
        $scope.characteristicsList = response.data;
    });

    $scope.materialDimension = {
        Id: null
        , MaterialMasterId: null
        , CharacteristicsId: null
        , CharacteristicsName: null
        , Sequence: null
        , ValueAssignmentLevel: null
        , AttributeProperty: null
        , IsFixedNoOfCharacter: null
        , NoOfCharacter: null
        , IsFreeField: true
        , IsPreDefinedField: true
        , IsMandatory: true
        , Active: true
        , MaterialMasterCharacteristicsValues: []
    };
    $scope.materialDimensionNew = Object.assign({}, $scope.materialDimension);
    $scope.dimensionChange = function () {
        var obj = $.grep($scope.characteristicsList, function (item) {
            return item.Value === $scope.materialDimensionNew.CharacteristicsId;
        })[0];
        $scope.materialDimensionNew.CharacteristicsName = obj.CharacteristicsName;
        $scope.materialDimensionNew.ValueAssignmentLevel = obj.ValueAssignmentLevel;
        $scope.materialDimensionNew.IsFreeField = obj.IsFreeField;
        $scope.materialDimensionNew.IsPreDefinedField = obj.IsPreDefinedField;
        $scope.materialDimensionNew.IsMandatory = obj.IsMandatory;
        $scope.materialDimensionNew.IsFixedNoOfCharacter = obj.IsFixedNoOfCharacter;
        $scope.materialDimensionNew.NoOfCharacter = obj.NoOfCharacter;
        $scope.materialDimensionNew.AttributeProperty = obj.AttributeProperty;
    };
    $scope.addMDimensionListRow = function () {
        try {
            if (baseService.arrayLength($scope.dimensionList) > 2)
                throw 'Total no of dimension can not be more than 3!';
            if (baseService.isUndefinedOrNull($scope.materialDimensionNew.CharacteristicsId))
                return manualValidation('div_sku', true, 'Dimension is required.');
            var isAvailable = false;
            for (var i = 0; i < $scope.dimensionList.length; i++) {
                isAvailable = baseService.isAvailableInList($scope.dimensionList[i].CharacteristicsId, $scope.materialDimensionNew.CharacteristicsId, i, $scope.chindex);
                if (isAvailable) throw 'This dimension : [' + $scope.materialDimensionNew.CharacteristicsName + '] has been already taken';
            }
            angular.copy($scope.materialDimensionNew, $scope.materialDimension);
            if (!isAvailable) {
                if ($scope.chindex === -1) {
                    $scope.materialDimension.MaterialMasterId = $scope.materialMasterNew.Id;
                    $scope.materialDimension.Sequence = $scope.dimensionList.length + 1;
                    $scope.dimensionList.push($scope.materialDimension);
                }
                else {
                    $scope.dimensionList[$scope.chindex] = $scope.materialDimension;
                }
                $scope.chindex = -1;
                ClearMaterialDimension();
                CloseShowResult();
            }
        } catch (err) {
            ShowResult(err, 'failure');
        }
    };
    function ClearMaterialDimension() {
        $scope.ChAction = 'Add Row';
        $scope.materialDimension = {};
        $scope.materialDimensionNew = {
            ValueAssignmentLevel: null, Active: true, IsFreeField: true, IsPreDefinedField: true, IsMandatory: true
        };
        $scope.characteristicsValueList = [];
    }
    function getMaterialMasterCharacteristics() {
        $http({
            method: 'GET'
            , url: $scope.path + 'GetMaterialMasterCharacteristics?masterId=' + $scope.materialMasterNew.Id
        }).then(function successCallback(response) {
            $scope.dimensionList = response.data;
        }), function errorCallBack(response) {
        };
    }

    // #endregion SKU

    // #region Material Master Characteristics Value

    $scope.characteristicsValueList = [];
    $scope.characteristicsValuePoUp = function (charId, index) {
        $scope.charValueBtn = 'Add Row';
        $scope.charId = charId;
        CloseModalShowResult();
        $scope.chindex = index;
        $scope.characteristicsValue = {
            Id: baseService.pk()
            , MaterialMasterId: $scope.materialMasterNew.Id
            , CharacteristicsId: $scope.charId
            , Sequence: null
            , Code: null
            , ShortName: null
            , StandardName: null
            , UserName: null
            , SourceType: 'Specific'
            , Description: null
            , Remarks: null
            , IsDefault: false
            , Active: true
        };
        $scope.characteristicsValueNew = angular.copy($scope.characteristicsValue);
        $scope.GetMaterialMasterCharacteristicsValueSequence();
        angular.element(document.querySelector('#characteristicsValuePoUp')).modal('show');
    };
    $scope.addMasterCharacteristicsValue = function () {
        try {
            CloseModalShowResult();
            $scope.manualValidationAddRemove('div_c1', 'Sequence', $scope.characteristicsValueNew);
            $scope.manualValidationAddRemove('div_c2', 'Code', $scope.characteristicsValueNew);
            $scope.manualValidationAddRemove('div_c3', 'Short Name', $scope.characteristicsValueNew);
            $scope.manualValidationAddRemove('div_c4', 'Standard Name', $scope.characteristicsValueNew);
            $scope.manualValidationAddRemove('div_c5', 'User Name', $scope.characteristicsValueNew);
            var chList = $filter("filter")($scope.characteristicsValueList, { CharacteristicsId: $scope.charId });
            //for (var t = 0; t < baseService.arrayLength(chList); t++) {
            for (var t = 0; t < baseService.arrayLength($scope.characteristicsValueList); t++) {
                if ($scope.characteristicsValueList[t].CharacteristicsId === $scope.charId) {
                    //duplicateCheck(chList[t].Code, $scope.characteristicsValueNew.Code, t, $scope.mvindex, 'Code');
                    //duplicateCheck(chList[t].ShortName, $scope.characteristicsValueNew.ShortName, t, $scope.mvindex, 'ShortName');
                    //duplicateCheck(chList[t].StandardName, $scope.characteristicsValueNew.StandardName, t, $scope.mvindex, 'StandardName');
                    //duplicateCheck(chList[t].UserName, $scope.characteristicsValueNew.Code, t, $scope.mvindex, 'UserName');
                    //if ($scope.characteristicsValueNew.IsDefault)
                    //    duplicateCheck(chList[t].IsDefault, $scope.characteristicsValueNew.IsDefault, t, $scope.mvindex, null, 'Default value already exist for ' + chList[t].UserName);
                    duplicateCheck($scope.characteristicsValueList[t].Code, $scope.characteristicsValueNew.Code, t, $scope.mvindex, 'Code');
                    duplicateCheck($scope.characteristicsValueList[t].ShortName, $scope.characteristicsValueNew.ShortName, t, $scope.mvindex, 'ShortName');
                    duplicateCheck($scope.characteristicsValueList[t].StandardName, $scope.characteristicsValueNew.StandardName, t, $scope.mvindex, 'StandardName');
                    duplicateCheck($scope.characteristicsValueList[t].UserName, $scope.characteristicsValueNew.UserName, t, $scope.mvindex, 'UserName');
                    if ($scope.characteristicsValueNew.IsDefault)
                        duplicateCheck($scope.characteristicsValueList[t].IsDefault, $scope.characteristicsValueNew.IsDefault, t, $scope.mvindex, null, 'Default value already exist for ' + chList[t].UserName);
                }
            }
            checkPropertiesAndCharLength($scope.dimensionList[$scope.chindex], $scope.characteristicsValueNew);
            angular.copy($scope.characteristicsValueNew, $scope.characteristicsValue);
            if ($scope.mvindex === -1) {
                $scope.characteristicsValue.MaterialMasterId = $scope.materialMasterNew.Id;
                $scope.characteristicsValue.CharacteristicsId = $scope.charId;
                $scope.characteristicsValueList.push($scope.characteristicsValue);
            }
            else
                $scope.characteristicsValueList[$scope.mvindex] = $scope.characteristicsValue;
            $scope.mvindex = -1;
            ClearMasterCharacteristicsValueFields(getMaxNumberFromList(($filter("filter")($scope.characteristicsValueList, { CharacteristicsId: $scope.charId })), 'Sequence'))
        } catch (e) {
            ShowResult(e, '', 'characteristicsValuePoUp');
        }
    };
    $scope.GetMaterialMasterCharacteristicsValueSequence = function () {
        if (baseService.arrayLength($scope.characteristicsValueList) > 0)
            $scope.characteristicsValueNew.Sequence = getMaxNumberFromList(($filter("filter")($scope.characteristicsValueList, { CharacteristicsId: $scope.charId })), 'Sequence');
        else {
            $http.get('Materials/characteristicsvalue/getautosequence?characteristicsId=' + $scope.charId + '&materialId=' + $scope.materialMasterNew.Id)
                .then(function (response) {
                    $scope.characteristicsValueNew.Sequence = response.data;
                });
        }
    };
    $scope.GetMaterialMasterCharacteristicsValue = function (id, index) {
        $scope.charValueBtn = 'Update Row'
        $scope.mvindex = baseService.getIndexOf($scope.characteristicsValueList, id, 'Id');
        $scope.characteristicsValue = $scope.characteristicsValueList[$scope.mvindex];
        angular.copy($scope.characteristicsValue, $scope.characteristicsValueNew);
    };
    $scope.CloseMaterialMasterCharacteristicsValue = function () {
        $scope.dimensionList[$scope.chindex].MaterialMasterCharacteristicsValues = [];
        $scope.chindex = - 1;
        CloseModalShowResult();
        angular.element(document.querySelector('#characteristicsValuePoUp')).modal('hide');
    }
    $scope.clearMasterCharacteristicsValue = function () {
        ClearMasterCharacteristicsValueFields(getMaxNumberFromList($scope.characteristicsValueList, 'Sequence'));
        return true;
    }
    function ClearMasterCharacteristicsValueFields(seq) {
        $scope.charValueBtn = 'Add Row'
        $scope.mvindex = -1;
        $scope.characteristicsValue = {};
        $scope.characteristicsValueNew = {
            Id: baseService.pk()
            , MaterialMasterId: $scope.materialMasterNew.Id
            , CharacteristicsId: $scope.charId
            , Sequence: seq, Active: true, IsDefault: false
        };
    }
    function GetCharacteristicsValueListByMaterialMaster(id) {
        $scope.characteristicsValueList = [];
        $http.get('Materials/CharacteristicsValue/GetCharacteristicsValueListByMaterialMaster?materialMasterId=' + id)
            .then(function (response) {
                $scope.characteristicsValueList = response.data;
            });
    }

    // #endregion Characteristics Value

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
                url: 'Materials/MaterialMaster/DeleteCharacteristicsValues?id=' + $scope.LCChargesId
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

    // #region get Define Enum
    $scope.EnumList = [];
    $scope.getEnum = function () {
        $http({
            method: 'POST',
            url: "Materials/IssueControl/GetEnum",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EnumList = response.data;
        });
    }
    $scope.getEnum();
     // #endregion get Define Enum
}