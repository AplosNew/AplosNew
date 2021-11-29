'use strict';
ProductMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProductMasterController(cboService,commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Product Master";
    $scope.Action = 'Save';
    $scope.AltUomAction = 'Add Alternative UOM';
    $scope.productMasterTypeCheck = false;
    $scope.index = -1;
    $scope.altUomIndex = -1;
    $scope.productMasterTypeList = [];
    $scope.productMasters = [];
    $scope.path = 'Products/productmaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.productMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchByProductMasterList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
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
        },
        {
            'name': 'Product Category',
            'value': 'ProductCategoryName'
        },
        {
            'name': 'Product SubCategory',
            'value': 'ProductSubCategory'
        },
        {
            'name': 'Product',
            'value': 'ProductName'
        }
    ];
    $http({
        method: 'GET',
        url: 'Products/productcategory/getcbo'
    }).then(function successCallback(response) {
        $scope.productCategoryList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Products/productsubcategory/getcbo'
    }).then(function successCallback(response) {
        $scope.productSubCategoryList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Products/product/getcbo'
    }).then(function successCallback(response) {
        $scope.productList = response.data;
        });

    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });

    // #region Product Master

    $scope.productMaster = {
        Id: null,
        ProductCategoryId: null,
        ProductCategoryName: null,
        ProductSubCategoryId: null,
        ProductSubCategoryName: null,
        ProductId: null,
        ProductNanme: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        CostAndManufacture: null
        , CostAndManufactureCurrencyId: null
        , DaysToReachTheTarget: null
        , FirstdayOutPut: null
        , IsFixed: 'Fixed'
        , IncrementValue: null
        , Active: true
        , BaseProcessId: null
        , TargetQty: null
        , CostingType: null
        ,PlanningType:null
        , BaseUOMId:null
    };
    $scope.productMasterNew = angular.copy($scope.productMaster);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.productMasterNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.currencyList = [];
    cboService.getCompanyGroupCurrencyCbo(null, function (response) {
        $scope.currencyList = response;
    });

    $scope.getEfficencyList = function () {
        $http.get($scope.path + 'getefficencylist?masterId=' + $scope.productMasterNew.Id)
            .then(function (response) {
                $scope.efficencyList = response.data;
            });
    };
    $scope.getEfficencyList();


    $scope.CostingTypeList = [];
    cboService.getCostingTypesCbo(function (response) {
        $scope.CostingTypeList = response;
    });

    $scope.planningTypesList = [];
    cboService.getPlanningTypesCbo(function (response) {
        $scope.planningTypesList = response;
    });

    // #endregion End Product Master

    // #region AttributeValue
    $scope.attributeList = [];
    $scope.getAttribute = function (id) {
        $scope.materialAttributetbl = false;
        $http({
            method: 'GET',
            url: 'Products/productsubcategoryattribute/getattribute?productSubCategoryId=' + id
                + "&&productMasterId=" + $scope.productMasterNew.Id
        }).then(function successCallback(response) {
            if (response.data.length !== 0)
                $scope.materialAttributetbl = true;
            $scope.attributeList = response.data;
            $scope.searchFreeField = false;
            for (var i = 0; i < $scope.attributeList.length; i++) {
                var isFree = $scope.attributeList[i].IsFreeField;
                $scope.attributeList[i].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
                if (!baseService.isUndefinedOrNull($scope.attributeList[i].MaterialAttributeValueId)) {
                    $scope.attributeList[i].FlagDisable = true;
                }
            }
        });
    };
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
    $scope.mvalueindex = -1;
    $scope.materialAttributeValuePoUp = function (id, index) {
        $scope.materialAttributeValueParameters = {
            limit: 20,
            offset: 0,
            order: 'asc',
            sort: 'Code',
            searchBy: "Description",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.getAttributeValueData = function (pageno) {
            $scope.materialAttributeValueUrl = 'Materials/materialattributevalue/materialattributevaluesearch?materialAttributeId=' + id;
            baseService.paginationBase($scope.materialAttributeValueUrl, pageno, $scope.materialAttributeValueParameters)
                .then(function (result) {
                    $scope.materialAttributeValueList = result.Rows;
                    $scope.materialAttributeValueParameters.total_count = result.Total;
                    $scope.mvalueindex = index;
                    angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getAttributeValueData();
    };
    $scope.getAttrValue = function (id, code) {
        $scope.attributeList[$scope.mvalueindex].MaterialAttributeValueId = id;
        $scope.attributeList[$scope.mvalueindex].MaterialAttributeValueFreeText = code;
        //Enable TextBox
        $scope.searchFreeField = true;
        var isFree = $scope.attributeList[$scope.mvalueindex].IsFreeField;
        $scope.attributeList[$scope.mvalueindex].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
        $scope.mvalueindex = -1;
        angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('hide');
    };

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
    };
    $scope.materialAttributeValueId = '';
    $scope.materialAttributeValueCode = '';
    $scope.materialAttributeValueIndex = -1;
    $scope.SelectMA = function (id, code, index) {
        $scope.materialAttributeValueId = id;
        $scope.materialAttributeValueCode = code;
        $scope.searchFreeField = true;
        $scope.materialAttributeValueIndex = index;
    };
    $scope.SelectMAButton = function () {
        if ($scope.materialAttributeValueId === '') {
            alert('Please at first select row');
            return;
        }
        $scope.getAttrValue($scope.materialAttributeValueId, $scope.materialAttributeValueCode);
        $scope.materialAttributeValueId = '';
        $scope.materialAttributeValueCode = '';
        $scope.materialAttributeValueIndex = -1;
        angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('hide');
    };
    $scope.materialAttributeValueClear = function (index) {
        $scope.attributeList[index].MaterialAttributeValueId = null;
        $scope.attributeList[index].MaterialAttributeValueFreeText = null;
        $scope.searchFreeField = false;
        var isFree = $scope.attributeList[index].IsFreeField;
        $scope.attributeList[index].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
    };
    $scope.ClosematerialAttributePopUp = function () {
        angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('hide');
    };
    // #endregion End AttributeValue

    // #region MaterialMasterAlternativeUOM

    $scope.altUOMtbl = false;
    $scope.baseUOMDisable = false;
    $scope.materialMasterAlternativeUOMs = [];
    $scope.materialMasterAlternativeUOM = {
        Id: null,
        ProductMasterId: null,
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
        UsedUomInPO: null
    };
    $scope.materialMasterAlternativeUOMNew = angular.copy($scope.materialMasterAlternativeUOM);

    $scope.putValueInAltUom = function () {
        $scope.productMasterNew.OIRUoMId = $scope.productMasterNew.BaseUOMId;
        $scope.baseUOM = document.getElementById("baseUOMId").options[document.getElementById('baseUOMId').selectedIndex].text;
    }

    $scope.GetToUoMFactor = function () {
        cboService.getToUoMFactor($scope.materialMasterAlternativeUOMNew.AlternativeUOMId, $scope.productMasterNew.BaseUOMId, function (response) {
            $scope.materialMasterAlternativeUOMNew.BaseUOMFactor = response.data
        });
    }

    $scope.GetAlternativeUomListByProductMaster = function (id) {
        $http({
            method: 'GET',
            url: 'Products/ProductMaster/GetProductMasterAltUomList?productMasterId=' + id,
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
            if ($scope.productMasterNew.BaseUOMId == null) {
                throw 'Please select base uom from uom tab';
            }
            if ($scope.materialMasterAlternativeUOMNew.AlternativeUOMId == null) {
                throw 'Please select alternative uom';
            }
            if ($scope.productMasterNew.BaseUOMId == $scope.materialMasterAlternativeUOMNew.AlternativeUOMId) {
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
            if ($scope.materialMasterAlternativeUOMNew.BaseUOMFactor > 0) {
                angular.copy($scope.materialMasterAlternativeUOMNew, $scope.materialMasterAlternativeUOM);
                // isAvailable true == add new
                if (!isAvailable) {
                    if ($scope.altUomIndex == -1) {
                        this.materialMasterAlternativeUOM.Id = null;
                        this.materialMasterAlternativeUOM.AlternativeUOMId = $scope.materialMasterAlternativeUOMNew.AlternativeUOMId;
                        this.materialMasterAlternativeUOM.AlternativeUOMName = $scope.altUomName;
                        this.materialMasterAlternativeUOM.BaseUOMId = $scope.productMasterNew.BaseUOMId;
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
            } else
                throw 'Please insert base uom factor';
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

    $scope.oirUoMList = [];
    $scope.createUomList = function () {
        var OIRUoMId = $scope.productMasterNew.OIRUoMId;
        $scope.oirUoMList = oirUoMListCreate($scope.productMasterNew.BaseUOMId, $scope.materialMasterAlternativeUOMs);
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

    $scope.createNewUomList = function () {
      
        var OIRUoMId = $scope.productMasterNew.OIRUoMId;
        $scope.oirUoMList = oirUoMListCreate($scope.productMasterNew.BaseUOMId, $scope.materialMasterAlternativeUOMs);
       
    }


    function clearAltUOM() {
        $scope.materialMasterAlternativeUOMNew.AlternativeUOMId = null;
        $scope.materialMasterAlternativeUOMNew.BaseUOMFactor = null;
        $scope.materialMasterAlternativeUOM = {};
    }
    // #endregion


    // #region Start CRUD 
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.productMaster = $scope.productMasters[$scope.index];
        $scope.productMasterNew = angular.copy($scope.productMaster);
        if (baseService.isUndefinedOrNull($scope.productMasterNew.CostingType)) {
            $scope.productMasterNew.CostingType = "N/A";
        }
        $scope.getAttribute($scope.productMasterNew.ProductSubCategoryId, $scope.productMasterNew.Id);
        $scope.getEfficencyList();

        $scope.GetAlternativeUomListByProductMaster($scope.productMasterNew.Id);
        $scope.baseUOM = $scope.productMasters[$scope.index].BaseUom;

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        if (baseService.isUndefinedOrNull($scope.productMasterNew.IsFixed)) {
            $scope.productMasterNew.IsFixed = 'Fixed';
        }
    };

    $scope.IsMandatoryButNull = function (isMandatory, materialAttributeValueFreeText) {
        if (isMandatory) {
            if (baseService.isUndefinedOrNull(materialAttributeValueFreeText)) {
                return true;
            }
            else
                return false;
        }
        else
            return false;
    };
    $scope.processList = [];
    cboService.getProductionProcessCbo(function (response) {
        $scope.processList = response;
    });
    // IncrementType, FirstDayOutPut, MinRequiredTargetHourly, StandardTime
    //$scope.GetDays = function (incType, incValue, firstDayOutPut, tQty) {
    //    try {
    //        var iv = parseInt(CalculateIncrementValue(incType, incValue, firstDayOutPut));//daily iv
    //        var _days = 1;
    //        var _cumi_output = parseInt(firstDayOutPut);
    //        while (_cumi_output < parseInt(tQty)) {
    //            _days++;
    //            _cumi_output += parseInt(firstDayOutPut) + iv;
    //            if (iv <= 0) {
    //                _days = 0;
    //                break;
    //            }
    //        }
    //        $scope.productMasterNew.DaysToReachTheTarget = _days;
    //    } catch (e) {
    //        throw e;
    //    }
    //};
    $scope.GetDays = function (incType, incValue, firstDayOutPut, tQty) {
        try {
            var iv = parseInt(CalculateIncrementValue(incType, incValue, firstDayOutPut));//daily iv
            var _days = 1;
            var _cumi_output = parseInt(firstDayOutPut);
            while (_cumi_output < parseInt(tQty)) {
                _days++;
                _cumi_output += iv;
                if (iv <= 0) {
                    _days = 0;
                    break;
                }
            }
            $scope.productMasterNew.DaysToReachTheTarget = _days;
        } catch (e) {
            throw e;
        }
    };
    function CalculateIncrementValue(isfixed, incValue, firstDayOutPut) {
        try {
            var iv = CheckNullReturnZero(incValue);
            if (isfixed === "Fixed")
                return iv;
            else
                return iv * CheckNullReturnZero(firstDayOutPut) / 100;
        } catch (e) {
            throw e;
        }
    }
    function CheckNullReturnZero(val) {
        if (baseService.isUndefinedOrNull(val)) return 0;
        else return parseInt(val);
    }

    function reDirectToRequiredTab() {
        if ($scope.formTab1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.formTab3.$invalid) {
            $scope.setTab(3);
        }
        else if ($scope.formTab4.$invalid) {
            $scope.setTab(4);
        }
       
    }

    $scope.Save = function () {
        angular.copy($scope.productMasterNew, $scope.productMaster);
        for (var i = 0; i < $scope.attributeList.length; i++) {
            var _invalid = $scope.IsMandatoryButNull($scope.attributeList[i].IsMandatory, $scope.attributeList[i].MaterialAttributeValueFreeText);
            if (_invalid) {
                ShowResult($scope.attributeList[i].MaterialAttributeName + ' value is required!', 'Error');
                return;
            }
        }
        if (baseService.isUndefinedOrNull($scope.productMasterNew.CostAndManufactureCurrencyId)) {
            ShowResult("CM UoM is required.", 'failure');
        }
        if (!baseService.isUndefinedOrNull($scope.productMasterNew.PlanningType) && $scope.productMasterNew.PlanningType !=='N/A') {
            if (baseService.isUndefinedOrNull($scope.productMasterNew.BaseProcessId)) {
                ShowResult("Base Process(Production) is required.", 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.productMasterNew.FirstdayOutPut)) {
                ShowResult("First Day Output Per Hour is required.", 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.productMasterNew.TargetQty)) {
                ShowResult("Standard Target Per Hour is required.", 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.productMasterNew.IncrementValue)) {
                ShowResult("Increment Value is required.", 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.productMasterNew.DaysToReachTheTarget)) {
                ShowResult("Days To Reach The Target is required.", 'failure');
            }
            
        }
        $scope.$broadcast('show-errors-check-validity');
        reDirectToRequiredTab();
        if ($scope.formTab1.$valid && $scope.formTab3.$valid && $scope.formTab4.$valid) {
            $scope.productCategory = document.getElementById("productCategoryId").options[document.getElementById('productCategoryId').selectedIndex].text;
            $scope.productSubCategory = document.getElementById("productSubCategoryId").options[document.getElementById('productSubCategoryId').selectedIndex].text;
            $scope.product = document.getElementById("productId").options[document.getElementById('productId').selectedIndex].text;
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'productMaster': $scope.productMaster, 'productMasterAttributeValue': $scope.attributeList, 'efficencyList': $scope.efficencyList, 'materialMasterAlternativeUOM': $scope.materialMasterAlternativeUOMs},
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.productMaster = response.data.ProductMaster;
                        $scope.productMaster.ProductCategoryName = $scope.productCategory;
                        $scope.productMaster.ProductSubCategoryName = $scope.productSubCategory;
                        $scope.productMaster.ProductName = $scope.product;

                        $scope.productMasters.push($scope.productMaster);
                        $scope.productMasters = $filter('orderBy')($scope.productMasters, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'productMaster': $scope.productMaster, 'productMasterAttributeValue': $scope.attributeList, 'efficencyList': $scope.efficencyList, 'materialMasterAlternativeUOM': $scope.materialMasterAlternativeUOMs},
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.productMaster.ProductCategoryName = $scope.productCategory;
                            $scope.productMaster.ProductSubCategoryName = $scope.productSubCategory;
                            $scope.productMaster.ProductName = $scope.product;
                            $scope.productMasters[$scope.index] = $scope.productMaster;
                            $scope.productMasters = $filter('orderBy')($scope.productMasters, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.productMasterNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.productMasterNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.productMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    // #endregion End CRUD Operations
    
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.productMaster = {};
        $scope.productMasterNew = {IsFixed: 'Fixed'};
        $scope.attributeList = [];
        $scope.materialAttributetbl = false;
        $scope.productMasterNew.Sequence = seq;
        $scope.productMasterNew.Active = true;
        $scope.productMasterNew.WithSKU = true;
        $scope.getEfficencyList();
        $scope.materialMasterAlternativeUOMs = [];
        $scope.baseUOM = null;
    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

}
