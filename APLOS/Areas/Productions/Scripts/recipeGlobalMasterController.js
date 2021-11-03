'use strict';
recipeGlobalMasterController.$inject = ["cboService", "$window", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function recipeGlobalMasterController(cboService, $window, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.Action = 'Save';
    declaration('Recipe Global Master', 'Productions/recipeglobalmaster/');
    allList();
    allObject();

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.recipeDetailsRawMaterialUsedList = [];

    $scope.rawMaterialList1 = [];
    $scope.MasterSubProcessId = '';

    $scope.hasArticle = false;

   

    $scope.searchvalueList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'StanderName',
            'value': 'StanderName'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        }
    ];
    $scope.attributeValuePopUpflag = '';
    $scope.attributeValuePopUp = function (data) {
        $scope.valueParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'Code'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.valueList = [];
        $scope.materialAttributeValueUrl = 'Materials/MaterialAttributeValue/GetAttributeValueList';
        baseService.setCurrentPage('valueList');
        $scope.getValueData = function (pageno) {
            $scope.valueParameters.assignment = data.ValueAssignmentLevel;
            $scope.valueParameters.materialMasterId = data.MaterialMasterId;
            $scope.valueParameters.attributeId = data.MaterialAttributeId;
            baseService.paginationBase($scope.materialAttributeValueUrl, pageno, $scope.valueParameters)
                .then(function (result) {
                    $scope.valueList = result.Rows;
                    $scope.valueParameters.total_count = result.Total;
                    angular.element(document.querySelector('#attributeValuePopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getValueData();
    };
    $scope.attributeValuePopUpNew = function (data, MaterialAttributeId, flag) {
        $scope.valueParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'Code'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.valueList = [];
        $scope.materialAttributeValueUrl = 'Materials/MaterialAttributeValue/GetAttributeValueList';
        baseService.setCurrentPage('valueList');
        $scope.getValueData = function (pageno) {
            $scope.valueParameters.assignment = data.ValueAssignmentLevel;
            $scope.valueParameters.materialMasterId = data.MaterialMasterId;
            $scope.valueParameters.attributeId = MaterialAttributeId;
            baseService.paginationBase($scope.materialAttributeValueUrl, pageno, $scope.valueParameters)
                .then(function (result) {
                    $scope.attributeValuePopUpflag = flag;
                    $scope.valueList = result.Rows;
                    $scope.valueParameters.total_count = result.Total;
                    angular.element(document.querySelector('#attributeValuePopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getValueData();
    };

    $scope.closeValuePopUp = function () {
        angular.element(document.querySelector('#attributeValuePopUp')).modal('hide');
        CloseModalShowResult('attributeValuePopUp');
    };
    $scope.attributeValueClear = function (flag) {
        //$scope.modelNew.ReipeName     = null;
        //$scope.modelNew.MaterialAttributeId = null;
        if (flag=='1') {
            $scope.modelNew.AttributeValueId = null;
            $scope.modelNew.AttributeValueName = null;
        }
        else if (flag=='2') {
            $scope.modelNew.Specification1ValueId = null;
            $scope.modelNew.Specification1ValueName = null;
        }
        else if (flag=='3') {
            $scope.modelNew.Specification2ValueId = null;
            $scope.modelNew.Specification2ValueName = null;
        }
        else {
            $scope.modelNew.AttributeValueId = null;
            $scope.modelNew.AttributeValueName = null;
            $scope.modelNew.Specification1ValueId = null;
            $scope.modelNew.Specification1ValueName = null;
            $scope.modelNew.Specification2ValueId = null;
            $scope.modelNew.Specification2ValueName = null;
        }
    };
    $scope.charValueClear = function (flag) {
        
        if (flag=='1') {
            
            $scope.modelNew.Characteristics1ValueId  = null;
            $scope.modelNew.Characteristics1Id       = null;
            $scope.modelNew.Characteristics1ValueName = null;
        }
        else if (flag=='2') {
            $scope.modelNew.Characteristics2ValueId = null;
            $scope.modelNew.Characteristics2Id = null;
            $scope.modelNew.Characteristics2ValueName = null;
        }
        else if (flag=='3') {
            $scope.modelNew.Characteristics3ValueId = null;
            $scope.modelNew.Characteristics3Id = null;
            $scope.modelNew.Characteristics3ValueName = null;
        }
        else {
            $scope.modelNew.Characteristics1ValueId   = null;
            $scope.modelNew.Characteristics1Id        = null;
            $scope.modelNew.Characteristics2ValueId  = null;
            $scope.modelNew.Characteristics2Id = null;
            $scope.modelNew.Characteristics3ValueId = null;
            $scope.modelNew.Characteristics3Id = null;
            $scope.modelNew.Characteristics1ValueName = null;
            $scope.modelNew.Characteristics2ValueName = null;
            $scope.modelNew.Characteristics3ValueName = null;
        }
    };
    $scope.searchCharFilterList = [
        {
            'Text': 'Sequence',
            'Value': 'Sequence'
        },
        {
            'Text': 'Code',
            'Value': 'Code'
        },
        {
            'Text': 'Short Name',
            'Value': 'ShortName'
        },
        {
            'Text': 'Standard Name',
            'Value': 'StandardName'
        },
        {
            'Text': 'User Name',
            'Value': 'UserName'
        }
    ];
    $scope.charValueflag = '';
    $scope.charValuePopUp = function (data, CharacteristicsId, flag) {
        $scope.charValueParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'Code'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.charDataList = [];
        $scope.charValueCharName = data.UserName;
        $scope.url = 'Materials/CharacteristicsValue/getcharacteristicsvaluesearchdata/';
        baseService.setCurrentPage('dataPlate');
        $scope.getSearchCharData = function (pageno) {
            $scope.charValueParameters.assignment = data.ValueAssignmentLevel;
            $scope.charValueParameters.materialMasterId = data.MaterialMasterId;
            $scope.charValueflag = flag;
            if ($scope.charValueflag==='1') {
                $scope.charValueParameters.charId = data.CharacteristicsId;
            }
            if ($scope.charValueflag === '2'){
                $scope.charValueParameters.charId = data.SpecificationChar1;
            }
            if ($scope.charValueflag === '3') {
                $scope.charValueParameters.charId = data.SpecificationChar2;
            }
           
            baseService.paginationBase($scope.url, pageno, $scope.charValueParameters)
                .then(function (result) {
                    $scope.dataPlate = result.Rows;
                    $scope.charDataList = result.Rows;
                    $scope.charValueflag = flag;
                    $scope.charValueParameters.total_count = result.Total;
                    angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getSearchCharData();
    };

    $scope.closeCharValuePopUp = function () {
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('hide');
        CloseModalShowResult('characteristicsValuepopup');
    };

    $scope.model = {
        Id: null
        , CompanyGroupId: null
        , CompanyId: null
        , EntityId: null
        , ProcessId: null
        , MaterialAttributeId: null
        , CharacteristicsId: null
        , AttributeValueId: null
        , AttributeValueName: null
        , CharacteristicsValueId: null
        , CharacteristicsValueName: null
        , Code: null
        , Name: null
        , IsUndulation: false
        , Description: null
        , ReipeName: null
        , SpecificationChar1: null
        , SpecificationChar2: null
        , Specification1Id: null
        , Specification2Id: null
        , Specification1ValueName: null
        , Specification2ValueName: null
        , Specification1ValueId: null
        , Specification2ValueId: null
        , RecipeLevel: null
        , ButtonRecipeMasterId: null
        , ProcessTypeId: null
        , ProcessTypeNam: null
        , Weight: null
        , LayerNo: null
        , ButtonRecipeDetailsId: null
        , MaterialMasterId: null
        , MaterialMasterName: null
        , ArticleId: null
        , ArticleName: null
        , RmValue: null
        , ValueAssignmentLevel: 'General'
        , Characteristics1ValueName: null
        , Characteristics2ValueName: null
        , Characteristics3ValueName: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.mRMNew = {
        Id: null
        , RecipeGlobalMasterId: null
        , RecipeGlobalSubprocessId: null
        , MaterialMasterId: null
        , ArticleId: null
        , QtyValue: null
    };
    $scope.mRMNewSave = Object.assign({}, $scope.mRMNew);

    $scope.selectMaterialByType = function (ob) {
        try {
            if ($scope.flag_for_mm === 'Recipe' || $scope.flag_for_mm === 'ProductDefinition') {// searching nonFG material
                $scope.modelNew.MaterialMasterId = ob.Id;
                $scope.modelNew.MaterialMasterName = ob.UserName;

                $scope.modelNew.ArticleId = null;
                $scope.modelNew.ArticleName = null;
            }

            cboService.getMeasurementCbo($scope.modelNew.MaterialMasterId, function (result) {
                $scope.uomList = result;
                if (baseService.arrayLength($scope.uomList) > 0) {
                    $scope.modelNew.UomId = $scope.uomList[0].Value;
                }
            });

            $scope.hasArticle = ob.HasAttribute;
            $scope.hasSku = ob.WithSKU;
            if (ob.HasAttribute) {
                $scope.getArticleSearchList(ob.Id);
            } else {

                //save starts during fg adding
                if ($scope.flag_for_mm === 'Recipe' || $scope.flag_for_mm === 'ProductDefinition') {// searching FG material
                    $scope.flag_for_mm = '';
                    $scope.operationNew.RecipeGlobalMasterId = $scope.mastermodal.Id;// $scope.master.Id;
                    $scope.operationNew.MaterialMasterId = ob.Id;// '153931'; // data1.Id;
                    $scope.operationNew.ArticleId = null;

                    $http({
                        method: 'POST',
                        url: 'Productions/RecipeMaterial/CreateRecipeMaterialFG/',
                        data: $scope.operationNew,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getRecipeRawMaterialListByMaster($scope.mastermodal.Id);
                        }
                    }), function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                //save ends

                $scope.closeMaterialMasterbyTypePopUp();
                return ShowResult('This material has no attribute', 'failure');
            }
            $scope.closeMaterialMasterbyTypePopUp();

            manualValidation('div_material', false);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.materialMasterbyTypeList = [];
    $scope.searchMaterialMasterList = [
        {
            'Text': 'Material Type',
            'Value': 'MaterialTypeName'
        },
        {
            'Text': 'Material Group',
            'Value': 'MaterialGroupMasterName'
        },
        {
            'Text': 'Code',
            'Value': 'Code'
        },
        {
            'Text': 'Material',
            'Value': 'UserName'
        },
        {
            'Text': 'Product',
            'Value': 'ProductMasterName'
        },
        {
            'Text': 'Id',
            'Value': 'Id'
        }
    ];

    $scope.flag_for_mm = '';
    $scope.getMaterialMasterbyTypePopUp = function (flag) {

        //if (flag === 'mm') {
        //    $scope.materialType = ['FinishedGoods'];
        //} else {
        //    $scope.materialType = ['RawMaterial'];
        //}
        $scope.materialType = flag;
       // $scope.materialType = 'Recipe';
        
        $scope.flag_for_mm = flag;
        $scope.mmPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        //$scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        //$scope.popUpUrl = 'Materials/MaterialMaster/GetMaterialListByMaterialType?materialType=' + JSON.stringify($scope.materialType);
        $scope.popUpUrl = 'Materials/MaterialMaster/MaterialSearchByBusinessProcess?type=' + $scope.materialType;
        baseService.setCurrentPage('materialMasterbyTypeList');
        $scope.getMaterialMasterbyTypeData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.mmPopUpParameters)
                .then(function (result) {
                    $scope.materialMasterbyTypeList = result.Rows;
                    $scope.mmPopUpParameters.total_count = result.Total;
                    angular.element(document.querySelector('#materialMasterbyTypePopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getMaterialMasterbyTypeData();
    };
    $scope.closeMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('materialMasterbyTypePopup');
        angular.element(document.querySelector('#materialMasterbyTypePopup')).modal('hide');
    };

    $scope.getArticleSearchList = function (id) {
        try {
            CloseShowResult();
            CloseModalShowResult();
            $scope.articlePopUpParameters = {
                limit: 10
                , offset: 0
                , order: 'asc'
                , sort: 'StandardName'
                , searchBy: "StandardName"
                , pageSize: 10
                , total_count: 0
                , search: null
                , serverPagination: true
            };
            $scope.searchList = [];
            $scope.dataPlate = [];
            baseService.setCurrentPage('dataPlate');
            $scope.articlePopUpParameters.materialMasterId = id;
            $scope.loadArticleData = function (pageno) {
                baseService.paginationBase('productions/recipeglobalmaster/getmaterialarticle', pageno, $scope.articlePopUpParameters)
                    .then(function (result) {
                        $scope.dataPlate = result.Rows;
                        $scope.articlePopUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.searchList) === 0) baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                        angular.element(document.querySelector('#articleSearchPop')).modal('show');
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.loadArticleData();
        } catch (e) {
            ShowResult(e, '');
        }
    };

    $scope.closeMaterialArticlePopUp = function () {
        $scope.searchList = [];
        $scope.dataPlate = [];
        $scope.popUpUrl = '';
        CloseModalShowResult('articleSearchPop');
        angular.element(document.querySelector('#articleSearchPop')).modal('hide');
    };

    function manualValidationAddRemove() {
        if (baseService.isUndefinedOrNull($scope.modelNew.MaterialMasterName))
            return manualValidation('div_material', true, 'Raw Material is required.');
        else manualValidation('div_material', false);

        if ($scope.hasArticle && baseService.isUndefinedOrNull($scope.modelNew.ArticleName))
            return manualValidation('div_article', true, 'Article is required.');
        else manualValidation('div_article', false);

        if (parseFloat($scope.modelNew.RmValue) === 0 || baseService.isUndefinedOrNull($scope.modelNew.RmValue) || isNaN($scope.modelNew.RmValue))
            return manualValidation('div_rmValue', true, 'Value is required.');
        else manualValidation('div_rmValue', false);
    }

    $scope.addRawMaterial = function () {
        try {
            if (manualValidationAddRemove()) return;

            var filterList = ($filter('filter')($scope.rawMaterialList1, { ButtonRecipeDetailsId: $scope.detailId }, true));

            if (baseService.arrayLength(filterList) > 0) {
                if (!$scope.hasArticle && baseService.valueCheckInList(filterList, 'MaterialMasterId', $scope.modelNew.MaterialMasterId))
                    throw 'This data already exist';
                else if ($scope.hasArticle && baseService.multipleValueCheckInList(filterList, 'MaterialMasterId', $scope.modelNew.MaterialMasterId, 'ArticleId', $scope.modelNew.ArticleId))
                    throw 'This data already exist';
            }

            if (baseService.isUndefinedOrNull($scope.modelNew.RmValue)) {
                throw "Raw Material value is required.";
            }
            if (baseService.isUndefinedOrNull($scope.modelNew.UomId)) {
                throw "Raw Material uom is required.";
            }

            //$scope.rawMaterialList1.push({
            //    Id: null
            //    , ButtonRecipeMasterId: $scope.modelNew.Id
            //    , ButtonRecipeDetailsId: $scope.detailId
            //    , MaterialMasterId: $scope.modelNew.MaterialMasterId
            //    , MaterialMasterName: $scope.modelNew.MaterialMasterName
            //    , ArticleId: $scope.modelNew.ArticleId
            //    , ArticleName: $scope.modelNew.ArticleName
            //    , RmValue: $scope.modelNew.RmValue
            //    , UomId: $scope.modelNew.UomId
            //});

            $scope.SaveRawMaterial();

            $scope.modelNew.MaterialMasterId = null;
            $scope.modelNew.MaterialMasterName = null;
            $scope.modelNew.ArticleId = null;
            $scope.modelNew.ArticleName = null;
            $scope.modelNew.RmValue = null;
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.mRMNew = {
        Id: null
        , RecipeGlobalMasterId: null
        , RecipeGlobalSubprocessId: null
        , MaterialMasterId: null
        , ArticleId: null
        , QtyValue: null
        , UomId: null
    };
    $scope.mRMNewSave = Object.assign({}, $scope.mRMNew);

    $scope.SaveRawMaterial = function () {
        try {
            $scope.getRecipeRawMaterialList($scope.MasterSubProcessId);

            //$scope.mRMNewSave.RecipeGlobalMasterId = $scope.MasterNewId;
            $scope.mRMNewSave.RecipeGlobalMasterId = $scope.mastermodal.Id;
            $scope.mRMNewSave.RecipeGlobalSubprocessId = $scope.MasterSubProcessId;
            $scope.mRMNewSave.MaterialMasterId = $scope.modelNew.MaterialMasterId;
            $scope.mRMNewSave.ArticleId = $scope.modelNew.ArticleId;
            $scope.mRMNewSave.QtyValue = $scope.modelNew.RmValue;
            $scope.mRMNewSave.UomId = $scope.modelNew.UomId;

            $http({
                method: 'POST',
                url: $scope.path + 'CreateRawMaterial',
                dataType: 'JSON',
                data: { 'RecipeGlobalRawMaterial': $scope.mRMNewSave }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {

                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getRecipeRawMaterialList($scope.MasterSubProcessId);
                    $scope.SaveDetailChildDisabled = false;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.detailindex = -1;
    $scope.rawPopUp = function (data, index) {
        $scope.uomList = [];
        $scope.detailindex = index;
        $scope.MasterSubProcessId = data.Id;
        $scope.getRecipeRawMaterialList($scope.MasterSubProcessId);
        $scope.detailId = $scope.MasterSubProcessId;
        manualValidation('div_material', false);
        manualValidation('div_article', false);
        manualValidation('div_rmValue', false);
        angular.element(document.querySelector('#rawMaterialPopup')).modal('show');
    };

    $scope.closeRawPopUp = function () {
        $scope.detailId = null;
        angular.element(document.querySelector('#rawMaterialPopup')).modal('hide');
    };
    $scope.groupindex = -1;
    $scope.RecipeMaterialGroupPopUp = function (data, index) {
        $scope.RecipeGlobalMaterialGroup = {};
        $scope.groupindex = index;
        $scope.RecipeGlobalMasterId = data.RecipeGlobalMasterId;
        $scope.RecipeGlobalSubprocessId = data.Id;
        $scope.recipeGroupList = [];
        $scope.GetRecipeGlobalMaterialGroup($scope.RecipeGlobalSubprocessId);

        angular.element(document.querySelector('#RecipeMaterialGroupPopUp')).modal('show');
    };

    $scope.GetRecipeGlobalMaterialGroup = function (recipeGlobalSubprocessId) {
        $http({
            method: 'GET',
            url: 'Productions/RecipeGlobalMaster/GetRecipeGlobalMaterialGroup?recipeGlobalSubprocessId=' + recipeGlobalSubprocessId
        }).then(function successCallback(response) {
            $scope.recipeGroupList = response.data;
        });
    };

    $scope.recipeMaterialGroupList = [];
    $scope.GetRecipeMaterialGroup = function () {
        $http({
            method: 'GET',
            url: 'Productions/RecipeGlobalMaster/GetRecipeMaterialGroup'
        }).then(function successCallback(response) {
            $scope.recipeMaterialGroupList = response.data;
            angular.element(document.querySelector('#RecipeGroupPopUp')).modal('show');
        });
    };

    $scope.RecipeGlobalMaterialGroup = {
        Id: null,
        RecipeGlobalMasterId: null,
        RecipeGlobalSubprocessId: null,
        RecipeMaterialGroupingMasterId: null,
        Value: 0,
        UserName: null
    };

    $scope.recipeGroupList = [];
    $scope.groupuomList = [];
    $scope.setGroupData = function (data) {

        $scope.RecipeGlobalMaterialGroup = {};
        //$scope.recipeGroupList.push({
        //    Id: null
        //    , RecipeGlobalMasterId: $scope.RecipeGlobalMasterId
        //    , RecipeGlobalSubprocessId: $scope.RecipeGlobalSubprocessId
        //    , RecipeMaterialGroupingMasterId: data.RecipeMaterialGroupingMasterId
        //    , Sequence: data.Sequence
        //    , Code: data.Code
        //    , StandardName: data.StandardName
        //    , UserName: data.UserName
        //    , UnitOfMeasurement: data.UnitOfMeasurement
        //});

        $scope.RecipeGlobalMaterialGroup.RecipeGlobalMasterId = $scope.RecipeGlobalMasterId;
        $scope.RecipeGlobalMaterialGroup.RecipeGlobalSubprocessId = $scope.RecipeGlobalSubprocessId;
        $scope.RecipeGlobalMaterialGroup.RecipeMaterialGroupingMasterId = data.RecipeMaterialGroupingMasterId;
        $scope.RecipeGlobalMaterialGroup.UserName = data.UserName;

        cboService.getRecipeMaterialGroupingMasterMeasurementCbo(data.RecipeMaterialGroupingMasterId, function (result) {
            $scope.groupuomList = result;
            for (var i = 0; i < $scope.groupuomList.length; i++) {
                $scope.RecipeGlobalMaterialGroup.UomId = $scope.groupuomList[0].Value;
            }
        });

        //$scope.SaveRecipeMaterialGroup();
        angular.element(document.querySelector('#RecipeGroupPopUp')).modal('hide');
    };

    $scope.SaveRecipeMaterialGroup = function () {
        $scope.GetRecipeGlobalMaterialGroup($scope.RecipeGlobalSubprocessId);
        try {
            if (baseService.isUndefinedOrNull($scope.RecipeGlobalMaterialGroup.Value)) {
                throw "Value is required.";
            }
            if (baseService.isUndefinedOrNull($scope.RecipeGlobalMaterialGroup.UomId)) {
                throw "Uom is required.";
            }
            $http({
                method: 'POST',
                url: $scope.path + 'CreateRecipeGlobalMaterialGroup',
                dataType: 'JSON',
                data: { 'recipeGlobalMaterialGroup': $scope.RecipeGlobalMaterialGroup }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'RecipeMaterialGroupPopUp');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'RecipeMaterialGroupPopUp');
                    $scope.GetRecipeGlobalMaterialGroup($scope.RecipeGlobalSubprocessId);
                    $scope.RecipeGlobalMaterialGroup = {};
                    $scope.groupuomList = [];
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure', 'RecipeMaterialGroupPopUp');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.closeSearchPopUp = function () {
        angular.element(document.querySelector('#RecipeGroupPopUp')).modal('hide');
    };

    $scope.closeMaterialGroupPopUp = function () {
        angular.element(document.querySelector('#RecipeMaterialGroupPopUp')).modal('hide');
    };

    $scope.getRecipeRawMaterialList = function (id) {
        $scope.rawMaterialList1 = [];
        $http.get('Productions/RecipeGlobalMaster/' + 'GetRecipeRawMaterialList?masterId=' + id)
            .then(function (response) {
                for (var i = 0; i < response.data.length; i++) {
                    if (baseService.isUndefinedOrNull(response.data[i].UserName)) {
                        response.data[i].UserName = '';
                    }
                }
                $scope.rawMaterialList1 = response.data;
            });
    };

    $scope.MaterialMasterCboList = [];
    $scope.MasterNewId = [];
    $scope.mastermodal.Id = null;
    $scope.UnitOfMeasurementList = [];

    cboService.getUnitOfMeasurementCbo(function (result) {
        $scope.UnitOfMeasurementList = result;

    });

    $scope.RecipeOperationList = [];

    $scope.pullRecipeOperation = function (processId) {
        cboService.getRecipeOperationCbo(processId, function (result) {
            $scope.RecipeOperationList = result;
        });
    };

    $scope.processChange = function (ProcessId) {
        $scope.pullRecipeOperation(ProcessId);
        $scope.getRecipeConfigData(ProcessId);

    };

    cboService.getMaterialMasterCbo(function (result) {
        $scope.MaterialMasterCboList = result;
    });

    $scope.getRecipeConfigData = function (processId) {
        $http({
            method: 'GET',
            url: $scope.path + 'getListOnChange?processId=' + processId
        }).then(function successCallback(response) {
            $scope.modelNew.AttributeValueId = "";
            $scope.modelNew.CharacteristicsValueId = "";
            if (baseService.arrayLength(response.data) > 0) {
                //visible
                
                $scope.modelNew.RecipeLevel = response.data[0].RecipeLevel;
                $scope.modelNew.SpecificationLevel1 = response.data[0].SpecificationLevel1;
                $scope.modelNew.SpecificationLevel2 = response.data[0].SpecificationLevel2;
                //label show
                $scope.modelNew.ReipeName = response.data[0].UserName;
                $scope.modelNew.Specification1 = response.data[0].s1;
                $scope.modelNew.Specification2 = response.data[0].s2;
                //value get
                $scope.modelNew.CharacteristicsId = response.data[0].RecipeDependCharacteristicsId;
                $scope.modelNew.MaterialAttributeId = response.data[0].RecipeDependAttributeId;
                ///-------------------------------
                //sp1
                $scope.modelNew.SpecificationChar1 = response.data[0].SpecificationCharacteristicId1;
                $scope.modelNew.Specification1Id = response.data[0].SpecificationAttributeId1;
                //sp2
                $scope.modelNew.SpecificationChar2 = response.data[0].SpecificationCharacteristicId2;
                $scope.modelNew.Specification2Id = response.data[0].SpecificationAttributeId2;

                if ($scope.modelNew.Specification1 === null) {
                    $scope.modelNew.SpecificationLevel1 = null;
                }

                if ($scope.modelNew.Specification2 === null) {
                    $scope.modelNew.SpecificationLevel2 = null;
                }

            }//if length>0
            else {
                $scope.modelNew.RecipeLevel = null;
                $scope.modelNew.SpecificationLevel1 = null;
                $scope.modelNew.SpecificationLevel2 = null;

                $scope.modelNew.ReipeName = null;
                $scope.modelNew.Specification1 = null;
                $scope.modelNew.Specification2 = null;

                $scope.modelNew.CharacteristicsId = null;
                $scope.modelNew.MaterialAttributeId = null;

                $scope.modelNew.SpecificationChar1 = null;
               
                $scope.modelNew.Specification1Id = null;

                $scope.modelNew.SpecificationChar2 = null;
                
                $scope.modelNew.Specification2Id = null;

                $scope.modelNew.SpecificationAttributeId1 = null;
                $scope.modelNew.SpecificationAttributeId2 = null;

                if ($scope.modelNew.Specification1 === null) {
                    $scope.modelNew.SpecificationLevel1 = null;
                }

                if ($scope.modelNew.Specification2 === null) {
                    $scope.modelNew.SpecificationLevel2 = null;
                }
            }
        });//success
    };

    $scope.MaterialAttributeList = [];
    cboService.getMaterialAttributeCbo(function (result) {
        $scope.MaterialAttributeList = result;

    });

    $scope.CharacteristicsList = [];
    cboService.getCharacteristicsCbo(function (result) {
        $scope.CharacteristicsList = result;

    });

    LoadProcessCriteria();

    $scope.searchByList = [
        {
            'value': 'Code'
            , 'name': 'Code'
        },
        {
            'value': 'Name'
            , 'name': 'Recipe Name'
        },
        {
            'value': 'Description'
            , 'name': 'Description'
        }
    ];

    baseService.init('Productions/RecipeGlobalMaster/RecipeGlobalMasterList', null, null, null, 'Code', 'Code');
    $scope.getDataList = function (pageno) {
        $rootScope.parameters.entityId = $scope.mastermodal.EntityId;
        $rootScope.parameters.processId = $scope.mastermodal.ProcessId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.RecipeMasterList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.mastermodal = { EntityId: $scope.mastermodal.EntityId, ProcessId: $scope.mastermodal.ProcessId};
        $scope.mastermodal.ValueAssignmentLevel= 'General';
        $scope.detailList = {};
        $scope.MasterNewId = '';
        $scope.mastermodal.Id = null;
        $scope.modelNew = {};
        $scope.modelNew.ValueAssignmentLevel = 'General';
        $scope.recipeDetailsRawMaterialUsedList = [];
        $scope.model = {};
        $scope.master = {};
        $scope.mastermodal.Id = null;
        $scope.processChange($scope.mastermodal.ProcessId);
    }

    ///========================================================================COMMON FUNCTION ANGULAR
    $scope.clearMMCode = function () {
        ClearMMCode();
    };

    $scope.showCharacteristicsGrid = function (hasCharForMM) {
        if (hasCharForMM === null || hasCharForMM === '') {
            return false;
        }
        else {
            return true;
        }
    };

    $scope.clearCharacteristics1Value = function () {
        $scope.mastermodal.Characteristics1ValueId = null;
        $scope.mastermodal.Characteristics1Value = null;
    };

    $scope.clearCharacteristics2Value = function () {
        $scope.mastermodal.Characteristics2ValueId = null;
        $scope.mastermodal.Characteristics2Value = null;
    };

    $scope.clearCharacteristics3Value = function () {
        $scope.mastermodal.Characteristics3ValueId = null;
        $scope.mastermodal.Characteristics3Value = null;
    };

    $scope.MainPageToModal = function () {
        for (var i in $scope.mastermodal) {
            $scope.mastermodal[i] = $scope.master[i];
        }
    };

    $scope.ModalToMainPage = function () {
        for (var i in $scope.mastermodal) {
            $scope.master[i] = $scope.mastermodal[i];
        }
        // char1
        if ($scope.modelNew.RecipeLevel == 'CH') {
            $scope.master.Characteristics1ValueName = $scope.modelNew.Characteristics1ValueName;
            $scope.master.Characteristics1Id = $scope.modelNew.Characteristics1Id;
            $scope.master.Characteristics1ValueId = $scope.modelNew.Characteristics1ValueId;
            $scope.master.MaterialAttributeId = null;
            $scope.master.AttributeValueId = null;
        } else {
            $scope.master.MaterialAttributeId = $scope.modelNew.MaterialAttributeId;
            $scope.master.AttributeValueId = $scope.modelNew.AttributeValueId;
            $scope.master.Characteristics1ValueName = null;
            $scope.master.Characteristics1Id = null;
            $scope.master.Characteristics1ValueId = null;
        }
       ///////////////// char2
        if ($scope.modelNew.SpecificationLevel1=='CH') {
            $scope.master.Characteristics2ValueName = $scope.modelNew.Characteristics2ValueName;
            $scope.master.Characteristics2Id        = $scope.modelNew.Characteristics2Id;
            $scope.master.Characteristics2ValueId   = $scope.modelNew.Characteristics2ValueId;
            $scope.master.Specification1Id = null;
            $scope.master.Specification1ValueId=null;
        } else {
            $scope.master.Specification1Id      = $scope.modelNew.Specification1Id;         
            $scope.master.Specification1ValueId = $scope.modelNew.Specification1ValueId;
            $scope.master.Characteristics2ValueNam = null;
            $scope.master.Characteristics2Id = null;
            $scope.master.Characteristics2ValueId = null;
        }
        ///////////////// char3
        if ($scope.modelNew.SpecificationLevel2 == 'CH') {
            $scope.master.Characteristics3ValueName = $scope.modelNew.Characteristics3ValueName;
            $scope.master.Characteristics3Id = $scope.modelNew.Characteristics3Id;
            $scope.master.Characteristics3ValueId = $scope.modelNew.Characteristics3ValueId;
            $scope.master.Specification2Id     =null;
            $scope.master.Specification2ValueId = null;
        } else {
            $scope.master.Specification2Id = $scope.modelNew.Specification2Id;
            $scope.master.Specification2ValueId = $scope.modelNew.Specification2ValueId;
            $scope.master.Characteristics3ValueName =   null;
            $scope.master.Characteristics3Id =    null;
            $scope.master.Characteristics3ValueId = null;
        }
    };

    $scope.getPlantCompanyWise = function () {
        try {
            if ($scope.mastermodal.CompanyId.length === 0) {
                throw "Select Company first...";
            }
            $scope.loadPlant($scope.mastermodal.CompanyId);
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.ClearBody = function () {
        ClearOb($scope.master = { EntityId: $scope.mastermodal.EntityId });
        ClearOb($scope.detailChildmodal);
        $scope.detailList = [];
        $scope.detailChildList = [];
        loadProcessList($scope.mastermodal.EntityId);
        $scope.modelNew.RecipeLevel = null;
    };

    $scope.ClearbyProcess = function () {
        ClearOb($scope.master = { ProcessId: $scope.mastermodal.ProcessId });
        ClearOb($scope.detailChildmodal);
        $scope.detailList = [];
        $scope.detailChildList = [];
        $scope.recipeDetailsRawMaterialUsedList = [];
        $scope.modelNew.RecipeLevel = null;
    };

    $scope.AddNewRecipeOperation = function () {
        $scope.recipeOperation.Id = null;
        $scope.recipeOperation.OperationId = null;
        $scope.recipeOperation.Sequence = null;
    };
    $scope.getAttrValue = function (data) {
        //console.log('data',data);
        if ($scope.attributeValuePopUpflag === '2') {
            $scope.modelNew.Specification1ValueId = data.MaterialAttributeValueId;//MaterialAttributeValueId
            $scope.modelNew.Specification1ValueName = data.UserName;
        }
        else if ($scope.attributeValuePopUpflag === '3') {
            $scope.modelNew.Specification2ValueId = data.MaterialAttributeValueId;//MaterialAttributeValueId
            $scope.modelNew.Specification2ValueName = data.UserName;
        }
        else//3
        {
            $scope.modelNew.AttributeValueId = data.MaterialAttributeValueId; //MaterialAttributeValueId
            $scope.modelNew.AttributeValueName = data.UserName;
        }
        manualValidation('div_arCh', false);
        $scope.attributeValuePopUpflag = '';
        $scope.closeValuePopUp();
    };


    $scope.getCharacteristicsValueCode = function (data) {

        if ($scope.charValueflag === '2')
        {
            $scope.modelNew.Characteristics2ValueId = data.CharacteristicsValueId;
            $scope.modelNew.Characteristics2Id = data.CharacteristicsId;
            $scope.modelNew.Characteristics2ValueName = data.UserName;
        }
        else if ($scope.charValueflag === '3')
        {
            $scope.modelNew.Characteristics3ValueId = data.CharacteristicsValueId;
            $scope.modelNew.Characteristics3Id = data.CharacteristicsId;
            $scope.modelNew.Characteristics3ValueName = data.UserName;
        }
        else 
        {
            $scope.modelNew.Characteristics1ValueId = data.CharacteristicsValueId;
            $scope.modelNew.Characteristics1Id = data.CharacteristicsId;
            $scope.modelNew.Characteristics1ValueName = data.UserName;
        }
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('hide');
        $scope.charValueflag = '';
        $scope.closeCharValuePopUp();
    };
    ///========================================================================LOAD LIST ANGULAR
    $scope.loadPlant = function (companyId) {
        try {
            $http.get($scope.path + "getplantcbo?companyId=" + companyId)
                .then(function (response) {
                    $scope.plantList = response.data;
                });
            $http.get($scope.path + "getunitcbo?companyId=" + companyId)
                .then(function (response) {
                    $scope.unitList = response.data;
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.loadSequence = function () {
        try {
            $http.get($scope.path + 'getautosequence')
                .then(function (response) {
                    $scope.mastermodal.Sequence = response.data;
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.loadDDL = function () {
        try {
            cboService.getCboCompanyByCompanyGroup(' ', function (result) {
                $scope.companyList = result;
            });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.loadDDLDetail = function () {
        try {
            cboService.loadUtilityCbo(function (result) { $scope.utilityList = result; });
            cboService.loadUomUtilityCbo(function (result) { $scope.utilityUomList = result; });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.loadDDLDetailChild = function () {
        try {
            $http.get($scope.path + "getmmuomcbo?materialmasterid=" + $scope.detailchildmodal.MaterialMasterId)
                .then(function (response) {
                    $scope.uomChildList = response.data;
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.getRawMaterialById = function (MaterialMasterId) {
        $http({
            method: 'GET',
            url: 'Materials/materialmaster/materialmasterbyid?MaterialMasterId=' + MaterialMasterId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data.materialMasterData) > 0) {
                SetMMData($scope.detailchildmodal, response.data.materialMasterData[0]);
            }
        });
    };

    $scope.getDetailData = function (masterid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetaillist?masterid=' + masterid
        }).then(function successCallback(response) {
            $scope.detailList = [];
            $scope.detailList = response.data;
            if (baseService.arrayLength($scope.searchbyDetaillist) === 0) {
                baseService.getDDLSearchColumn(response.data, $scope.searchbyDetaillist);
            }
        });
    };

    $scope.getUtilityData = function (recipewashsubprocessid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getutilitylist?recipeGlobalsubprocessid=' + recipewashsubprocessid,
        }).then(function successCallback(response) {
            $scope.recipeUutilityList = [];
            $scope.recipeUutilityList = response.data;
            //console.log('***', $scope.recipeUutilityList);
            if (baseService.arrayLength($scope.sbrecipeUutilityList) == 0) {
                baseService.getDDLSearchColumn(response.data, $scope.sbrecipeUutilityList);
            }
        });
    };

    $scope.getOperationData = function (subprocessid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getoperationlist?subprocessid=' + subprocessid,
        }).then(function successCallback(response) {
            $scope.recipeOperationList = [];
            //console.log('---',response);
            $scope.recipeOperationList = response.data;
            if (baseService.arrayLength($scope.sbrecipeOperationList) == 0) {
                baseService.getDDLSearchColumn(response.data, $scope.sbrecipeOperationList);
            }
        });
    };

    $scope.loadProcessAsperConfig = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getprocessasperconfigcbo?materialmasterid=' + $scope.master.MaterialMasterId,
        }).then(function successCallback(response) {
            $scope.processList = [];
            var r = response.data;
            if (baseService.arrayLength(r) > 0) {
                $scope.processList = r;
            }
        });
    };

    $scope.getCharacteristics = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getskuasperconfig/',
            params: { entityid: $scope.mastermodal.EntityId, materialmasterid: $scope.master.MaterialMasterId }
        }).then(function successCallback(response) {
            //console.log('char',response);
            ClearCharacteristics();
            if (baseService.arrayLength(response.data) > 0) {
                $scope.mastermodal.SelectedCharacteristics = response.data[0].SelectedCharacteristics;
                $scope.mastermodal.Characteristics1Selected = response.data[0].Characteristics1Selected;
                $scope.mastermodal.Characteristics2Selected = response.data[0].Characteristics2Selected;
                $scope.mastermodal.Characteristics3Selected = response.data[0].Characteristics3Selected;

                $scope.mastermodal.Characteristics1 = response.data[0].Characteristics1;
                $scope.mastermodal.Characteristics2 = response.data[0].Characteristics2;
                $scope.mastermodal.Characteristics3 = response.data[0].Characteristics3;

                $scope.mastermodal.Characteristics1Id = response.data[0].Characteristics1Id;
                $scope.mastermodal.Characteristics2Id = response.data[0].Characteristics2Id;
                $scope.mastermodal.Characteristics3Id = response.data[0].Characteristics3Id;
            }
            else {
                if ($scope.mastermodal.ProcessId != null && $scope.mastermodal.ProcessId != '') {
                    ShowResult('No data found in Recipe Config...', 'Error');
                }
            }
        });
    };

    $scope.getDetailEditData = function (pk) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetail?id=' + pk
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.detail = response.data[0];
                $scope.detailmodal = angular.copy($scope.detail);
                //cboService.loadSubprocessCbo($scope.ProcessId, function (result) {
                //$scope.subProcessList = result;
                // cboService.loadOperationCbo($scope.detailmodal.SubprocessId, function (result) { $scope.operationList = result; });
                // });
            }
        });
    };

    $scope.getSubprocessEditData = function (pk) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetail?id=' + pk
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                //console.log('ppp',response);
                $scope.recipeSubprocess = response.data[0];
            }
        });
    };

    $scope.getOperationEditData = function (pk) {
        $http({
            method: 'GET',
            url: $scope.path + 'getoperation?rwoid=' + pk,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                //console.log('ppp',response);
                $scope.recipeOperation = response.data[0];
            }
        });
    };

    //$scope.getUtilityEditData = function (pk) {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'getutility?recipewashutilityid=' + pk,
    //    }).then(function successCallback(response) {
    //        if (baseService.arrayLength(response.data) > 0) {
    //            $scope.detailmodal = response.data[0];
    //        }
    //    });
    //};
    $scope.getUtilityEditData = function (ob) {
                $scope.detailmodal = ob;
    };

    $scope.getDetailChildData = function (masterid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetailchildlist?detailid=' + masterid
        }).then(function successCallback(response) {
            for (var i in $scope.detailchildmodal) {
                $scope.detailchildmodal[i] = null;
            }
            $scope.detailchildList = [];
            $scope.detailchildList = response.data;
            if (baseService.arrayLength($scope.searchbyDetailChildlist) === 0) {
                baseService.getDDLSearchColumn(response.data, $scope.searchbyDetailChildlist);
            }
        });
    };

    $scope.getDetailChildEditData = function (pk) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetailchild?id=' + pk,
        }).then(function successCallback(response) {
            //$scope.rawMaterial = response.data[0];
            if (baseService.arrayLength(response.data) > 0) {
                ///get mm id to get uom from db and fill cbo
                //then set uom selected value
                $scope.loadMMUomList(response.data[0]);
            }
        });
    };

    $scope.uom = null;

    $scope.getMasterData = function (masterid) {
        $scope.getRecipeConfigData($scope.mastermodal.ProcessId);
        $http({
            method: 'GET',
            url: $scope.path + 'getmasterlist?masterid=' + masterid
        }).then(function successCallback(response) {
            $scope.masterList = [];
            $scope.masterList = response.data;
            if (baseService.arrayLength($scope.masterList) > 0) {
                $scope.master = $scope.masterList[0];
                $scope.mastermodal = $scope.masterList[0];

                $scope.pullRecipeOperation($scope.mastermodal.ProcessId);
                $scope.modelNew.RecipeLevel = $scope.masterList[0].RecipeLevel;
                $scope.modelNew.ReipeName = $scope.masterList[0].DependantAttribute;
                $scope.modelNew.MaterialAttributeId = $scope.masterList[0].MaterialAttributeId;
                $scope.modelNew.Specification1Id = $scope.masterList[0].Specification1Id;
                $scope.modelNew.Specification2Id = $scope.masterList[0].Specification2Id;

                //if (baseService.isUndefinedOrNull($scope.modelNew.Specification1Id)) {
                //    $scope.getRecipeConfigData($scope.mastermodal.ProcessId);
                //}
                //if (baseService.isUndefinedOrNull($scope.modelNew.Specification2Id)) {
                //    $scope.getRecipeConfigData($scope.mastermodal.ProcessId);
                //}

                $scope.modelNew.Specification1ValueId = $scope.masterList[0].Specification1ValueId;
                $scope.modelNew.Specification1ValueName = $scope.masterList[0].Specification1ValueName;

                $scope.modelNew.Specification2ValueId = $scope.masterList[0].Specification2ValueId;
                $scope.modelNew.Specification2ValueName = $scope.masterList[0].Specification2ValueName;

                $scope.modelNew.AttributeValueId = $scope.masterList[0].AttributeValueId;
                $scope.modelNew.AttributeValueName = $scope.masterList[0].AttributeValueName;

                $scope.modelNew.Characteristics1Id = $scope.masterList[0].Characteristics1Id;
                $scope.modelNew.Characteristics1ValueId = $scope.masterList[0].Characteristics1ValueId;
                $scope.modelNew.Characteristics1ValueName = $scope.masterList[0].Characteristics1ValueName;

                $scope.modelNew.Characteristics2Id = $scope.masterList[0].Characteristics2Id;
                $scope.modelNew.Characteristics2ValueId = $scope.masterList[0].Characteristics2ValueId;
                $scope.modelNew.Characteristics2ValueName = $scope.masterList[0].Characteristics2ValueName;

                $scope.modelNew.Characteristics3Id = $scope.masterList[0].Characteristics3Id;
                $scope.modelNew.Characteristics3ValueId = $scope.masterList[0].Characteristics3ValueId;
                $scope.modelNew.Characteristics3ValueName = $scope.masterList[0].Characteristics3ValueName;

                $scope.modelNew.Description = $scope.masterList[0].Description;
                $scope.mastermodal.Description = $scope.masterList[0].Description;
                //$scope.mastermodal.Thickness = $scope.masterList[0].Thickness;
                //$scope.mastermodal.Weight = $scope.masterList[0].Weight;
                $scope.uom = $scope.mastermodal.AvgUomWeight;
                $scope.uomAddItem = $scope.mastermodal.AvgUomWeight;

            }//if length>0
        });//success
    };

    $scope.loadMMUomList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getmmuomcbo?materialmasterid=' + $scope.master.MaterialMasterId
        }).then(function successCallback(response) {//getmmuomcbo
            $scope.mmUomList = response.data;
        });
    };

    $scope.loadMMUomList = function (obj) {
        $scope.uomChildList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getmmuomcbo?materialmasterid=' + obj.MaterialMasterId,
        }).then(function successCallback(response) {
            $scope.uomChildList = response.data;
            $scope.rawMaterial = obj;
            $scope.getRawMaterialById(obj.MaterialMasterId);
        });
    };
    ///========================================================================LOAD SEARCH GRID ANGULAR
    $scope.getData = function () {
        baseService.init($scope.path + 'getlist', null, 25, null, 'Description', 'Description');
        $scope.loadMasterData = function (pageno) {//loadMMData
            $rootScope.parameters.MaterialMasterId = $scope.master.MaterialMasterId;

            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.masterList = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMasterlist) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMasterlist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMasterData();
    };

    $scope.getMMData = function () {
        //baseService.init($scope.path + 'getmaterialmasterlist', null, 25, null, 'Description', 'Description');
        baseService.init('Materials/materialmaster/materialmastersearch', null, 25, null, 'UserName', 'UserName');
        $scope.loadMMData = function (pageno) {//loadProcessData
            baseService.pagination(pageno)
                .then(function (result) {
                    //console.log('kk',result);
                    $scope.mmData = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMaterialMasterDatalist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMaterialMasterDatalist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMMData();
    };

    $scope.getMMForRMData = function () {
        baseService.init('Materials/materialmaster/materialmasterrecipe', null, 25, null, 'UserName', 'UserName');
        //baseService.init($scope.path + 'MaterialMasterRecipe', null, 25, null, 'Description', 'Description');
        $scope.loadMMForRMData = function (pageno) {//loadProcessData
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.mmForRMData = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMaterialMasterForRMDatalist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMaterialMasterForRMDatalist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMMForRMData();
    };
   
    $scope.getCharacteristicsValueData = function (characteristicsid) {
        //baseService.init($scope.path + 'getcharacteristicsvaluelist', null, 25, null, 'Description', 'Description');
        baseService.init('materials/characteristicsvalue/characteristicsvaluesearh', null, 25, null, 'Code', 'Code');
        $scope.loadCharacteristicsValueData = function (pageno) {//loadProcessData
            $rootScope.parameters.CharId = characteristicsid;
            $rootScope.parameters.ids = '';
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.characteristicsValueData = result.Rows;
                    //console.log(result.Rows);
                    //if (baseService.arrayLength($scope.searchbyCharacteristicsValuelist) == 0) {
                    //    baseService.getDDLSearchColumn(result.Rows, $scope.searchbyCharacteristicsValuelist);
                    //}
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadCharacteristicsValueData();
    };
    ///######################################################################## SAVE AND DELETE ################################################################
    $scope.SaveMaster = function () {
        try {
            ValidationMaster();
            $scope.ModalToMainPage();
            $scope.master.ProcessId = $scope.mastermodal.ProcessId; // $scope.ProcessId;
            $scope.master.EntityId = $scope.mastermodal.EntityId;
            $http({
                method: 'POST',
                url: $scope.path + 'createmaster',
                dataType: 'JSON',
                data: { 'master': $scope.master }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.mastermodal.Id = response.data.id;
                    ShowResult(response.data.Message, 'success');
                    $scope.Action = 'Update';
                    $scope.getDataList();
                    $scope.uomAddItem = $("#AvgUom option:selected").text();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SaveDetail = function () {
        try {
            ValidationDetail();
            $scope.master.Id = $scope.mastermodal.Id;
            $scope.detailmodal.RecipeGlobalMasterId = $scope.master.Id;

            $scope.SaveDetailDisabled = true;
            $http({
                method: 'POST',
                url: $scope.path + 'createdetail',
                dataType: 'JSON',
                data: { 'recipeutility': $scope.detailmodal }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.SaveDetailDisabled = false;
                    ShowResult(response.data.Message, 'failure', 'detailentrypopup');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'detailentrypopup');
                    $scope.getUtilityData($scope.recipeUtility.RecipeWashSubprocessId);
                    $scope.detailmodal = {};
                    $scope.SaveDetailDisabled = false;
                }
            }, function errorCallback(response) {
                $scope.SaveDetailDisabled = false;
                ShowResult(response.status.Message, 'failure', 'detailentrypopup');
            });
            return true;
        } catch (e) {
            $scope.SaveDetailDisabled = false;
            ShowResult(e, 'Error', 'detailentrypopup');
        }
    };

    $scope.SaveRecipeSubprocess = function () {
        try {
            // ValidationDetail();
            $scope.recipeSubprocess.RecipeGlobalMasterId = $scope.mastermodal.Id;// $scope.MasterNewId; // $scope.master.Id;
            $scope.recipeSubprocess.ProcessId = $scope.ProcessId;
            //$scope.detailmodal.ProcessId = $scope.ProcessId;
            //$scope.master.EntityId = $scope.EntityId;
            //$scope.detailmodal.MaterialMasterId = $scope.master.MaterialMasterId;
            //for (var i in $scope.recipeSubprocess) {
            //    $scope.recipeSubprocess[i] = $scope.detailmodal[i];
            //}

            $scope.SaveDetailDisabled = true;
            $http({
                method: 'POST',
                url: $scope.path + 'CreateRecipeSubprocess',
                dataType: 'JSON',
                data: { 'recipesubprocess': $scope.recipeSubprocess }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'recipesubprocessentrypopup');

                    $scope.getDetailData($scope.mastermodal.Id);
                    //$scope.getDetailData($scope.MasterNewId);
                    $scope.recipeSubprocess = {};

                    $scope.gridDetailGrid = true;
                    $scope.SaveDetailDisabled = false;

                    cboService.loadSubprocessCbo($scope.mastermodal.ProcessId, function (result) {
                        $scope.subProcessList = result;

                        if (baseService.arrayLength($scope.subProcessList) > 0) {
                            cboService.subprocessCbo($scope.mastermodal.ProcessId, function (res) {
                                $scope.sprocessList = res;

                                if (baseService.arrayLength($scope.sprocessList) > 0) {
                                    $scope.recipeSubprocess.SubprocessId = $scope.sprocessList[0].Value;
                                }

                            });
                        }
                    });

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.SaveRecipeOperation = function () {
        try {
            // ValidationDetail();
            $scope.master.Id = $scope.mastermodal.Id;
            $scope.recipeOperation.RecipeGlobalMasterId = $scope.master.Id;
            $scope.recipeOperation.ProcessId = $scope.ProcessId;
            $scope.recipeOperation.RecipeGlobalSubprocessId = $scope.RecipeWashSubprocessId;

            $scope.SaveDetailDisabled = true;
            $http({
                method: 'POST',
                url: $scope.path + 'CreateRecipeOperation',
                dataType: 'JSON',
                data: { 'recipeoperation': $scope.recipeOperation }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'recipeoperationentrypopup');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'recipeoperationentrypopup');
                    $scope.getOperationData($scope.recipeOperation.SubprocessId);
                    $scope.SaveDetailDisabled = false;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure', 'recipeoperationentrypopup');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error', 'recipeoperationentrypopup');
        }
    };

    ///--------------materrial article save from popup-------------

    $scope.operationNew = {
        RecipeGlobalMasterId: null,
        MaterialMasterId: null,
        ArticleId: null
    };

    $scope.selectArticle = function (data1) {
       
        if ($scope.flag_for_mm === 'ProductDefinition') {
            $scope.flag_for_mm = '';

            $scope.operationNew.RecipeGlobalMasterId = $scope.master.Id;
            $scope.operationNew.RecipeGlobalMasterId = $scope.mastermodal.Id;
            $scope.operationNew.MaterialMasterId = data1.MaterialMasterId;// '153931'; // data1.Id;
            $scope.operationNew.ArticleId = data1.Id;

            $http({
                method: 'POST',
                url: 'Productions/RecipeMaterial/CreateRecipeMaterialFG/',
                data: $scope.operationNew,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                   
                    $scope.getRecipeRawMaterialListByMaster($scope.mastermodal.Id);
                                 }
            }), function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            };
            $scope.closeMaterialArticlePopUp();
        }
        else {
            $scope.modelNew.ArticleId = data1.Id;
            $scope.modelNew.ArticleName = data1.StandardName;
            //manualValidation('div_ar', false);
            $scope.closeMaterialArticlePopUp();
        }

    };

    $scope.DeleteMaster = function () {
        try {
            $scope.master.Id = $scope.mastermodal.Id;
            if ($scope.master.Id === null || $scope.master.Id === '') {
                throw 'No Recipe is found...';
            }
            $http({
                method: 'POST',
                url: $scope.path + 'deletemaster',
                dataType: 'JSON',
                data: { 'masterid': $scope.master.Id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
                    $scope.masterAddEditPopup('DELETE');
                    //$scope.mastermodal.ProcessId = $scope.mastermodal.ProcessId;
                    //$scope.mastermodal.ProcessId = $scope.mastermodal.ProcessId;
                    $scope.ClearBody();
                    $scope.getDataList();
                    $scope.recipeDetailsRawMaterialUsedList = [];
                    $scope.master.Id = null;
                    $scope.mastermodal.Id = null;
                    $scope.attributeValueClear('');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');

            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteDetail = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'deletedetail',
            dataType: 'JSON',
            data: { 'detailid': $scope.detailmodal.Id }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //other child
                //$scope.getDetailData($scope.master.Id);
                $scope.getDetailData($scope.mastermodal.Id);
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.DeleteOperation = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'deleterecipeoperation',
            dataType: 'JSON',
            data: { 'operationid': $scope.recipeOperation.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure', 'recipeoperationentrypopup');
            }
            else {
                ShowResult(response.data.Message, 'success', 'recipeoperationentrypopup');
                $scope.getOperationData($scope.recipeOperation.SubprocessId);
                $scope.AddNewRecipeOperation();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure', 'recipeoperationentrypopup');
        });
        return true;
    };

    $scope.DeleteUtility = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'deleterecipeutility',
            dataType: 'JSON',
            data: { 'utilityid': $scope.recipeUtility.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure', 'recipeutilityentrypopup');
            }
            else {
                ShowResult(response.data.Message, 'success', 'recipeutilityentrypopup');
                $scope.getUtilityData($scope.recipeUtility.RecipeWashSubprocessId);
                //$scope.getOperationData($scope.recipeOperation.SubprocessId);
                //$scope.AddNewRecipeOperation();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure', 'recipeutilityentrypopup');
        });
        return true;
    };

    $scope.DeleteRawMaterial = function (Id, index) {
        $http({
            method: 'POST',
            // url: $scope.deleteUrlDetailChild,
            url: $scope.path + 'deleterawmaterial',
            dataType: 'JSON',
            data: { 'rawmaterialid': Id } //$scope.rawMaterial.Id }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'recipewashrawmaterialpopup');
            }
            else {
                ShowResult(response.data.Message, 'success', 'recipewashrawmaterialpopup');
                //reload other child
                $scope.getDetailChildData($scope.rawMaterial.UtilityId);
            }
            $scope.rawMaterialList1.splice($scope.index, 1);
            $scope.index = -1;
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure', 'recipewashrawmaterialpopup');
        });
        return true;
    };

    ///========================================================================SEARCH POPUP
    $scope.masterSearchPopup = function () {
        $scope.getData();
        angular.element(document.querySelector('#mastersearchpopup')).modal('show');
    };

    $scope.showProcessModal = function () {
        $scope.getProcessData();
        angular.element(document.querySelector('#processmodal')).modal('show');
    };

    $scope.showMMRMModal = function () {
        $scope.getMMForRMData();
        angular.element(document.querySelector('#mmrmmodal')).modal('show');
    };

    $scope.showMMModal = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ProcessId)) {
                throw "Process can not be blank...";
            }
            $scope.getMMData();
            angular.element(document.querySelector('#mmmodal')).modal('show');
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.searchCharacteristics3Value = function (cvid) {
        $scope.dim = "3";
        $scope.getCharacteristicsValueData(cvid);
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };

    $scope.searchCharacteristics2Value = function (cvid) {
        $scope.dim = "2";
        $scope.getCharacteristicsValueData(cvid);
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };

    $scope.searchCharacteristics1Value = function (cvid) {
        $scope.dim = "1";
        $scope.getCharacteristicsValueData(cvid);
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };
    ///========================================================================ENTRY POPUP
    $scope.masterAddEditPopup = function (flag) {
        try {
            if (flag === 'NEW') {
                ClearMasterModal();
                LoadProcessCriteria();//by monir
                LoadUom(flag);
                $scope.getCharacteristics();
                angular.element(document.querySelector('#masteraddeditpopup')).modal('show');
            }
            else if (flag === 'DELETE') {
                ClearMaster();
                angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
            }
            else {
                if (baseService.arrayLength($scope.detailList) > 0) {
                    throw "Line Item (Subprocess) is available, so edition is not possible..."; 
                }
                LoadUom(flag);
                ClearMasterModal();
                $scope.getCharacteristics();
                LoadProcessCriteria();
                //$scope.MainPageToModal();
                angular.element(document.querySelector('#masteraddeditpopup')).modal('show');
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.detailEntryPopup = function (ob, flag) {
        $scope.master.Id = $scope.mastermodal.Id;
        if ($scope.master.Id === null || $scope.master === "") {
            ShowResult("Select a 'Master' first....");
            return;
        }

        ClearDetailModal();
        $scope.loadDDLDetail();
        //cboService.getWashOperationCbo(ob.Id, function (result) { $scope.operationList = result; });

        if (flag === 'NEW') {
            $scope.detailchildList = [];
            for (var i in $scope.detailmodal) {
                $scope.detailmodal[i] = null;
            }
            $scope.detailmodal.RecipeGlobalSubprocessId = ob.Id;
            $scope.detailmodal.SubprocessId = ob.SubprocessId;
            $scope.detailmodal.IsFixed = 'Fixed';
        }
        else {
            $scope.getDetailEditData(ob.Id);
        }
        angular.element(document.querySelector('#detailentrypopup')).modal('show');
    };

    $scope.showRawMaterialPopup = function (ob) {
        angular.element(document.querySelector('#recipewashrawmaterialpopup')).modal('show');
    };

    $scope.showSubprocessPopup = function (id, flag) {
        try {
           
            $scope.uom = $("#AvgUom option:selected").text();
            if (baseService.isUndefinedOrNull($scope.mastermodal.Id)) {
                throw "Please Select a recipe...";
            }
            
            if (flag === 'EDIT') {
                $scope.recipeSubprocess.SubprocessId = null;
                $scope.recipeSubprocess.Sequence = null;
                $scope.recipeSubprocess.RecipeWashMasterId = $scope.mastermodal.Id;// $scope.master.Id;

                //cboService.subprocessCbo($scope.mastermodal.ProcessId, function (result) {
                //    //$scope.subProcessList = result;
                //    //if ($scope.subProcessList.length === 1)
                //    //    $scope.recipeSubprocess.SubprocessId = $scope.subProcessList[0].Value;
                //    //$scope.getSubprocessEditData(id);

                //    $scope.subProcessList = result;
                //    $scope.getSubprocessEditData(id);

                cboService.loadSubprocessCbo($scope.mastermodal.ProcessId, function (result) {
                    $scope.subProcessList = result;

                    if (baseService.arrayLength($scope.subProcessList) > 0) {
                        cboService.subprocessCbo($scope.mastermodal.ProcessId, function (res) {
                            $scope.sprocessList = res;

                            if (baseService.arrayLength($scope.sprocessList) > 0) {
                                $scope.recipeSubprocess.SubprocessId = $scope.sprocessList[0].Value;
                            }

                        });
                    }
                    $scope.getSubprocessEditData(id);
                });
            }
            else {
                $scope.recipeSubprocess.Id = null;
                $scope.recipeSubprocess.SubprocessId = null;
                $scope.recipeSubprocess.Sequence = null;
                $scope.recipeSubprocess.RecipeWashMasterId = $scope.mastermodal.Id;// $scope.master.Id;

                //cboService.subprocessCbo($scope.mastermodal.ProcessId, function (result) {
                //    //$scope.subProcessList = result;
                //    //if ($scope.subProcessList.length === 1)
                //    //    $scope.recipeSubprocess.SubprocessId = $scope.subProcessList[0].Value;

                //    $scope.subProcessList = result;
                //});

                //subprocessCbo

                cboService.loadSubprocessCbo($scope.mastermodal.ProcessId, function (result) {
                    $scope.subProcessList = result;

                    if (baseService.arrayLength($scope.subProcessList) > 0) {
                        cboService.subprocessCbo($scope.mastermodal.ProcessId, function (res) {
                            $scope.sprocessList = res;

                            if (baseService.arrayLength($scope.sprocessList) > 0) {
                                $scope.recipeSubprocess.SubprocessId = $scope.sprocessList[0].Value;
                            }

                        });
                    }
                });
            }
            angular.element(document.querySelector('#recipesubprocessentrypopup')).modal('show');
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.showOperationPopup = function (ob, flag) {
        try {
            if (baseService.isUndefinedOrNull(ob.Id)) {
                throw "Please Select a Subprocess...";
            }
            $scope.getOperationData(ob.SubprocessId);
            $scope.RecipeWashSubprocessId = ob.Id;
            $scope.recipeOperation.SubprocessId = ob.SubprocessId;

            if (flag === 'EDIT') {
                $scope.recipeOperation.Sequence = null;
                $scope.recipeOperation.OperationId = null;
                $scope.recipeOperation.RecipeWashMasterId = $scope.mastermodal.Id;// $scope.master.Id;
                cboService.loadOperationCbo(ob.SubprocessId, function (result) {
                    $scope.operationList = result;
                });
            }
            else {
                $scope.recipeOperation.Id = null;
                $scope.recipeOperation.OperationId = null;
                $scope.recipeOperation.Sequence = null;
                $scope.recipeOperation.RecipeWashMasterId = $scope.mastermodal.Id;//$scope.master.Id;
                cboService.loadOperationCbo(ob.SubprocessId, function (result) {
                    $scope.operationList = result;
                });
            }
            angular.element(document.querySelector('#recipeoperationentrypopup')).modal('show');
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    ///========================================================================DELETE POPUP
    $scope.deleteMaster = function () {
        var _id = $scope.mastermodal.Id;
        $scope.message_confirmation = "Are you sure to delete [" + _id + "] ";
        angular.element(document.querySelector('#confirmmasterdelete')).modal('show');
    };

    $scope.removeMasterYes = function () {
        angular.element(document.querySelector('#confirmmasterdelete')).modal('hide');
        $scope.DeleteMaster();
    };

    $scope.removeRowYes = function () {
        $scope.DeleteDetail();
        angular.element(document.querySelector('#detailentrypopup')).modal('hide');
    };

    $scope.deleteDetailGrid = function (id) {
        $scope.detailmodal.Id = id;
        $scope.message_confirmation = "Are you sure to delete [" + id + "] ";
        angular.element(document.querySelector('#confirmdetaildelete')).modal('show');
    };

    $scope.removeRowDetailYes = function () {
        $scope.DeleteDetail();
        angular.element(document.querySelector('#confirmdetaildelete')).modal('hide');
    };

    $scope.deleteOperation = function (id) {
        $scope.recipeOperation.Id = id;
        $scope.message_confirmation = "Are you sure to delete [" + id + "] ";
        angular.element(document.querySelector('#coperationdelete')).modal('show');
    };

    $scope.removeOperationYes = function () {
        $scope.DeleteOperation();
        angular.element(document.querySelector('#coperationdelete')).modal('hide');
    };

    $scope.deleteUtility = function (ob) {
        $scope.recipeUtility.Id = ob.Id;
        $scope.recipeUtility.RecipeWashSubprocessId = ob.RecipeWashSubprocessId;
        $scope.message_confirmation = "Are you sure to delete [" + ob.Utility + "] ";
        angular.element(document.querySelector('#cutilitydelete')).modal('show');
    };

    $scope.removeUtilityYes = function () {
        $scope.DeleteUtility();
        angular.element(document.querySelector('#cutilitydelete')).modal('hide');
    };

    $scope.deleteRawMaterial = function (ob) {
        $scope.rawMaterial.Id = ob.Id;
        $scope.message_confirmation = "Are you sure to delete [" + ob.MaterialMaster + "] ";
        angular.element(document.querySelector('#crmdelete')).modal('show');
    };

    $scope.removeRawMaterialYes = function () {
        $scope.DeleteRawMaterial();
        angular.element(document.querySelector('#crmdelete')).modal('hide');
    };
    ///######################################################################### SELECTED ROW #########################################################################
    $scope.getProcessCode = function (id, code) {
        $scope.mastermodal.ProcessId = id;
        $scope.mastermodal.Process = code;
        angular.element(document.querySelector('#processmodal')).modal('hide');
    };

    $scope.clearProcessCode = function (id, code) {
        $scope.mastermodal.ProcessId = null;
        $scope.mastermodal.Process = null;
    };

    $scope.GetMasterIndex = function (x) {
        $scope.Action = 'Update';
        $scope.getMasterData(x.Id);
        $scope.getDetailData(x.Id);
        $scope.getRecipeRawMaterialListByMaster(x.Id);
        $scope.MasterNewId = x.Id;
        $scope.mastermodal.Id = x.Id;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.getRecipeConfigData(x.ProcessId);
    };

    $scope.getRecipeRawMaterialListByMaster = function (id) {
        $http.get('Productions/RecipeGlobalMaster/RecipeDetailsUsedListList?recipemasterId=' + id)
            .then(function (response) {
                $scope.recipeDetailsRawMaterialUsedList = response.data;
            });
    };

    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to permanent delete [" + name + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope[$scope.listName][$scope.popUpIndex].Id)) {
            $http({
                method: 'POST'
                , url: 'Productions/RecipeMaterial/delete/' + $scope[$scope.listName][$scope.popUpIndex].Id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, "failure");
                else {
                    ShowResult(response.data.Message, "success");
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        for (var t = 0; t < baseService.arrayLength($scope.recipeDetailsRawMaterialUsedList); t++) {
            if ($scope.recipeDetailsRawMaterialUsedList[t][$scope.tempId] === $scope[$scope.listName][$scope.popUpIndex][$scope.listId])
                $scope.recipeDetailsRawMaterialUsedList.splice(t, 1);
        }
        $scope[$scope.listName].splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };

    $scope.getMMCode = function (obj) {
        SetMMData($scope.master, obj);
        SetMMData($scope.mastermodal, obj);
        angular.element(document.querySelector('#mmmodal')).modal('hide');
    };

    $scope.rawMaterialSingle = function (id) {
        try {
            if (id === null || id === "") {
                ShowResult("Select a 'Line Item' first....");
                return;
            }
            $scope.rawMaterial.Id = id;
            $scope.getDetailChildEditData(id);
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.getDetailRow = function (ob, falg) {
        $scope.detailEntryPopup(ob, falg);
        $scope.getUtilityData(ob.Id);
        $scope.recipeUtility.RecipeWashSubprocessId = ob.Id;
    };

    $scope.getDetailChildRow = function (index) {
        $scope.detailChildEntryPopup('EDIT');
        $scope.detailchildmodal = $scope.detailChildList[index];
    };

    $scope.getMMRMCode = function (ob) {
        ClearDetailChild($scope.detailchildmodal);
        SetMMData($scope.detailchildmodal, ob);
        $scope.loadDDLDetailChild();
        angular.element(document.querySelector('#mmrmmodal')).modal('hide');
    };

  
    ///==========================================================================LOAD FROM DATABASE
    function LoadUom(flag) {
        $http({
            method: 'GET',
            url: $scope.path + 'getmmuomcbo?materialmasterid=' + $scope.master.MaterialMasterId
        }).then(function successCallback(response) {//getmmuomcbo
            $scope.uomList = response.data;
            $scope.avgUomList = response.data;
            if (flag === 'EDIT') {
                $scope.MainPageToModal();
            }
        });
    }

    function LoadProcessCriteria() {
        $http({
            method: 'GET',
            url: 'Processes/processcriteria/getcriteriacbo'
        }).then(function (response) {
            $scope.processCriteriaList = response.data;
            //console.log(' $scope.processCriteriaList', response.data);
        });
    }

    function loadProcessList(entityid) { cboService.GetEntityProductionProcessCbo(entityid, function (result) { $scope.processList = result; }); }
    ///==========================================================================COMMON FUNCTION
    function ClearOb(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
    }

    function ClearObject(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
    }

    function ClearDetailChild() {
        //list obj savebtn savetext
        ClearObject($scope.detailchildmodal);
        $scope.SaveDetailChildDisabled = false;
        $scope.ActionDetailChild = 'Save';
        //$scope.detailchildList = [];
        $scope.uomChildList = [];
    }

    function allList() {
        $scope.detailList = [];
        $scope.detailchildList = [];
        $scope.subProcessList = [];
        $scope.utilityList = [];
        $scope.recipeUutilityList = [];
        $scope.recipeOperationList = [];

        $scope.sbrecipeOperationList = [];
        $scope.searchbyDetaillist = [];
        $scope.searchbyDetailChildlist = [];
        $scope.searchbyMasterlist = [];
        $scope.searchbyMaterialMasterDatalist = [];
        $scope.searchbyMaterialMasterForRMDatalist = [];
        $scope.searchbyCharacteristicsValuelist = [];
        $scope.sbrecipeUutilityList = [];

        $scope.companyList = [];
        $scope.plantList = [];
        $scope.mmUomList = [];
        $scope.uomChildList = [];

        $scope.departmentList = [];
        $scope.lineList = [];
        $scope.subsectionList = [];
        $scope.sectionList = [];
        $scope.divisionList = [];
        $scope.characteristicsValueData = [];
    }

    function allObject() {
        $scope.recipeSubprocess = {
            Id: null,
            RecipeWashMasterId: null,
            SubprocessId: null,
            Sequence: null,
            LineItemValue: 0
        };
        $scope.recipeOperation = {
            Id: null,
            RecipeWashMasterId: null,
            RecipeWashSubprocessId: null,
            SubprocessId: null,
            OperationId: null,
            Sequence: null
        };
        $scope.recipeUtility = {
            Id: null,
            RecipeWashMasterId: null,
            RecipeWashSubprocessId: null,
            RecipeWashOperationId: null,
            SubprocessId: null,
            OperationId: null,
            UtilityId: null,
            Temperature: null,
            IsFixed: 'Fixed',
            Ph: null,
            QtyValue: null,
            Uom: null,
            Duration: null,
            Remark: null,
            Sequence: null
        };
        $scope.detailmodal = {
            Id: null,
            RecipeGlobalMasterId: null,
            RecipeGlobalSubprocessId: null,
            RecipeGlobalOperationId: null,
            SubprocessId: null,
            OperationId: null,
            UtilityId: null,
            Temperature: null,
            IsFixed: 'Fixed',
            IsPercentage: null,
            Ph: null,
            Qty: null,
            Uom: null,
            Duration: null,
            Remark: null,
            Sequence: null
        };
        $scope.rawMaterial = {
            Id: null,
            RecipeWashMasterId: null,
            RecipeWashSubprocessId: null,
            RecipeWashOperationId: null,
            RecipeWashUtilityId: null,
            SubprocessId: null,
            OperationId: null,
            UtilityId: null,
            MaterialMasterId: null,
            UomId: null,
            QtyValue: null,
            IsFixed: 'Fixed',
            IsOperationLevel: null,
            Remark: null
        };

        $scope.detailchildmodal = {
            Id: null,
            RecipeWashMasterId: null,
            RecipeWashSubprocessId: null,
            RecipeWashOperationId: null,
            RecipeWashUtilityId: null,
            SubprocessId: null,
            OperationId: null,
            UtilityId: null,
            MaterialMasterId: null,
            UomId: null,
            Qty: null,
            IsPercentage: null,
            IsOperationLevel: null,
            Remark: null
        };

        $scope.master = {
            Id: null,
            Description: null,
            MaterialMasterDescription: null,
            MaterialMasterCode: null,
            MaterialMasterId: null,
            Code: null, Uom: null, AvgUom: null, MaterialAvgWeight: null,
            UserName: null,
            BatchSize: null,
            ProcessId: null, ProcessCriteriaId: null,
            MaterialAttributeId: null,
            AttributeValueId: null,
            AttributeValueName: null,
            CharacteristicsValueId: null,
            CharacteristicsValueName: null,
            Characteristics1Selected: true,
            Characteristics2Selected: true,
            Characteristics3Selected: true,
            Characteristics1: null,
            Characteristics2: null,
            Characteristics3: null,
            Characteristics1Id: null,
            Characteristics2Id: null,
            Characteristics3Id: null,
            Characteristics1ValueId: null,
            Characteristics2ValueId: null,
            Characteristics3ValueId: null,
            Characteristics1Value: null,
            Characteristics2Value: null,
            Characteristics3Value: null,
            EndTemperature: null,
            StartTemperature: null,
            StartPressure: null,
            EndPressure: null,
            GradientTemperature: null,
            GradientPressure: null,
            Process: null, EntityId: null,
            SelectedCharacteristics: null
        };

        $scope.mastermodal = {
            Id: null,
            Description: null,
            MaterialMasterDescription: null,
            MaterialMasterCode: null,
            MaterialMasterId: null,
            Code: null, Uom: null, AvgUom: null, MaterialAvgWeight: null,
            UserName: null,
            BatchSize: null,
            ProcessId: null, ProcessCriteriaId: null,
            Characteristics1Selected: true,
            Characteristics2Selected: true,
            Characteristics3Selected: true,
            Characteristics1: null,
            Characteristics2: null,
            Characteristics3: null,
            Characteristics1Id: null,
            Characteristics2Id: null,
            Characteristics3Id: null,
            Characteristics1ValueId: null,
            Characteristics2ValueId: null,
            Characteristics3ValueId: null,
            Characteristics1Value: null,
            Characteristics2Value: null,
            Characteristics3Value: null,
            EndTemperature: null,
            StartTemperature: null,
            StartPressure: null,
            EndPressure: null,
            GradientTemperature: null,
            GradientPressure: null,
            Process: null, EntityId: null,
            SelectedCharacteristics: null
        };
    }

    function declaration(title, path) {
        $rootScope.title = title;
        $scope.path = path;
        $scope.message_confirmation = "";
        $scope.SaveDetailDisabled = false;
        $scope.SaveDetailChildDisabled = false;
        $scope.RecipeWashSubprocessId = null;
        $scope.IsProportionate = false;
    }

    function ClearMMCode() {
        ClearOb($scope.master);
        ClearOb($scope.mastermodal);
        ClearOb($scope.recipeSubprocess);
        ClearOb($scope.recipeOperation);
        ClearOb($scope.recipeUtility);
        ClearOb($scope.detailmodal);
        ClearOb($scope.rawMaterial);
        ClearOb($scope.detailchildmodal);
        allList();
        $scope.RecipeWashSubprocessId = null;
    }

    function CheckField(fieldValue, fieldName) {
        try {
            if (baseService.isUndefinedOrNull(fieldValue) || fieldValue === '') {
                throw ('[' + fieldName + '] is required...');
            }
        } catch (e) {
            throw e;
        }
    }

    function CheckFieldTime(fieldValue, fieldName) {
        try {
            CheckField(fieldValue, fieldName);
            if (fieldValue.length !== 5) {
                throw fieldName + ' is not correct format...Ex: 08:00, 15:30 (HH:mm)';
            }
            if (fieldValue.substr(2, 1) !== ':') {
                throw fieldName + ' is not correct format...Ex: 08:00, 15:30 (HH:mm)';
            }
            var a = parseInt(fieldValue.substr(0, 2));
            if (a > 23) {
                throw fieldName + ' can not be greater than 23...';
            }
            if (a < 0) {
                throw fieldName + ' can not be negetive...';
            }
            var b = parseInt(fieldValue.substr(3, 2));
            if (b > 59) {
                throw fieldName + ' can not be greater than 59...';
            }
            if (b < 0) {
                throw fieldName + ' can not be negetive...';
            }

            if (a === 0 && b === 0) {
                throw fieldName + ' can not be blank...';
            }
            //first 2 digit check integer
            //last 2 digit check integer
        } catch (e) {
            throw e;
        }
    }

    function ValidationMaster() {
        try {
            //check PORecipeTag
            CheckField($scope.mastermodal.EntityId, 'Production Entity');
            CheckField($scope.mastermodal.ProcessId, 'Process');
            CheckField($scope.mastermodal.Code, 'Code');
            CheckField($scope.mastermodal.UserName, 'User Name');
            CheckField($scope.mastermodal.MaterialAvgWeight, 'Avg Weight');
            CheckField($scope.mastermodal.AvgUom, 'Avg Weight Uom');
            //CheckField($scope.mastermodal.BatchSize, 'BatchSize');
            //CheckField($scope.mastermodal.Uom, 'Uom');
            CheckField($scope.mastermodal.ProcessCriteriaId, 'Process Criteria');
        } catch (e) {
            throw e;
        }
    }

    function ValidationDetail() {
        try {
            CheckField($scope.mastermodal.Id, 'Recipe Master');
            CheckField($scope.detailmodal.SubprocessId, 'Subprocess');
            CheckField($scope.detailmodal.RecipeGlobalOperationId, 'Operation');
            CheckField($scope.detailmodal.UtilityId, 'Utility');
            CheckField($scope.detailmodal.QtyValue, 'Value');
            CheckField($scope.detailmodal.Uom, 'UoM');
            CheckField($scope.detailmodal.Duration, 'Duration');
            CheckField($scope.detailmodal.Temperature, 'Temperature');
            CheckField($scope.detailmodal.Ph, 'Ph');
            //CheckField($scope.detailmodal.Remark, 'Remark');
        } catch (e) {
            throw e;
        }
    }

    function CheckDuplicateSubprocess(ob) {
        try {
            for (var i = 0; i < baseService.arrayLength($scope.detailList); i++) {
                if (ob.Id !== $scope.detailList[i].Id) {
                    if (ob.SubprocessId === $scope.detailList[i].SubprocessId) {
                        throw "Subprocess: [" + $scope.detailList[i].Subprocess + "] has already been taken...";
                    }//id
                }//id
            }
        } catch (e) {
            throw e;
        }
    }

    function ClearCharacteristics() {
        $scope.mastermodal.SelectedCharacteristics = null;
        $scope.mastermodal.Characteristics1Selected = null;
        $scope.mastermodal.Characteristics2Selected = null;
        $scope.mastermodal.Characteristics3Selected = null;

        $scope.mastermodal.Characteristics1 = null;
        $scope.mastermodal.Characteristics2 = null;
        $scope.mastermodal.Characteristics3 = null;

        $scope.mastermodal.Characteristics1Id = null;
        $scope.mastermodal.Characteristics2Id = null;
        $scope.mastermodal.Characteristics3Id = null;
    }

    function ValidationDetailChild() {
        try {
            //
            //CheckField($scope.detailchildmodal.RecipeSubprocessId, 'Line Item (RecipeSubprocessId) is not selected...');
            //CheckField($scope.detailchildmodal.MaterialMasterId, 'Material Master');
            //CheckField($scope.detailchildmodal.Qty, 'Qty');
            //var _qty = $scope.detailchildmodal.Qty;
            //if (_qty <= 0) {
            //    throw "Qty must be greater than Zero...";
            //}
            //CheckUOMandPerc();
            //CheckDuplicate($scope.detailchildmodal);
        } catch (e) {
            throw e;
        }
    }

    function CheckUOMandPerc() {
        try {
            if ($scope.detailchildmodal.Ispercentage) {
                if ($scope.detailchildmodal.UomId !== null && $scope.detailchildmodal.UomId !== '') {
                    throw 'UOM and Percentage both can not be taken...';
                }
            }
            else {
                CheckField($scope.detailchildmodal.UomId, 'UOM');
            }
        } catch (e) {
            throw e;
        }
    }

    function CheckDuplicate(ob) {
        try {
            for (var i = 0; i < arrayLength($scope.detailchildList); i++) {
                if (ob.Id !== $scope.detailchildList[i].Id) {
                    if (ob.MaterialMasterId === $scope.detailchildList[i].RawMaterialId) {
                        throw "Material Master: [" + ob.MaterialMasterDescription + "] has already been taken...";
                    }//id
                }//id
            }
        } catch (e) {
            throw e;
        }
    };

    function SetMMData(list, obj) {
        list.MaterialMasterId = obj.Id;
        list.MaterialMasterDescription = obj.Description;
        list.MaterialMasterCode = obj.Code;
        list.UserName = obj.UserName;
        list.MaterialType = obj.MaterialType;
        list.MaterialGroup = obj.MaterialGroupMaster;
        list.GridNO = obj.GridName;
        list.MaterialGridId = obj.MaterialGridId;
        list.BaseUOM = obj.BaseUom;
        list.BaseUOMId = obj.BaseUOMId;
    }

    function ClearMasterModal() {
        for (var i in $scope.mastermodal) {
            if (i !== 'MaterialMasterId' && i !== 'MaterialMasterDescription' && i !== 'MaterialMasterCode') {
                $scope.mastermodal[i] = null;
            }
        }
        $scope.btnDetailEntryPopup = true;
        $scope.btndeletemaster = false;
        $scope.Action = 'Save';
    }

    function ClearMaster() {
        for (var i in $scope.master) {
            if (i !== 'MaterialMasterId' && i !== 'MaterialMasterDescription' && i !== 'MaterialMasterCode') {
                $scope.master[i] = null;
            }
        }
        ClearMasterModal();
        ClearDetail();
    }

    function ClearDetail() {
        //ClearObject($scope.detailmodal);
        $scope.detailList = [];
        ClearObject($scope.detail);
        $scope.gridDetailGrid = false;
        $scope.btnDetailEntryPopup = false;
        ClearDetailModal();
        ClearDetailChild();
    }

    function ClearDetailModal() {
        ClearObject($scope.detailmodal);
        // $scope.SaveDetailDisabled = false;
        // $scope.ActionDetail = 'Save'
        $scope.subProcessList = [];
    }

    ///==========================================================================LOAD TIME CALL
    $scope.recipeMaterialGroupingDetailList = [];
    $scope.groupingDetailindex = -1;
    $scope.getRecipeMaterialGroupingDetail = function (data, index) {
        $scope.groupingDetailindex = index;
        $scope.RecipeGlobalMaterialGroupName = data.UserName;
        $http({
            method: 'GET',
            url: 'productions/recipematerialgroupingmaster/getrecipematerialgroupingdetaillist?masterid=' + data.RecipeGlobalMaterialGroupId
        }).then(function successCallback(response) {
            $scope.recipeMaterialGroupingDetailList = [];
            $scope.recipeMaterialGroupingDetailList = response.data;
            angular.element(document.querySelector('#RecipeMaterialGroupingDetailPopup')).modal('show');
        });
    };

    $scope.closeRecipeMaterialGroupingDetailPopup = function () {
        angular.element(document.querySelector('#RecipeMaterialGroupingDetailPopup')).modal('hide');
    };

    $scope.entityList = [];
    //cboService.getCboProductionEntityByPlant(null, null, $window.plantId, function (result) {
    //    $scope.entityList = result;
    //});

    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.getAllEntities();


    $scope.processCriteriaList = [];
    cboService.processCriteriaCbo($window.companyGroupId, function (result) {
        $scope.processCriteriaList = result;
    });

    $scope.Print = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.mastermodal.Id))
                throw "First select recipe.";
            else
                location.href = 'productions/recipeglobalmaster/getreport?mmId=' + $scope.mastermodal.Id;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.popUpIndex = -1;
    $scope.DeleteMaterialGroup = function (data, index) {
        try {
            $scope.popUpIndex = index;
            $scope.name = data.UserName;
            $scope.Id = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete [" + $scope.name + "] ";
            angular.element(document.querySelector('#confirmDelete')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteRow = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteRecipeGlobalMaterialGroup',
            dataType: 'JSON',
            data: { 'id': $scope.Id }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.GetRecipeGlobalMaterialGroup($scope.RecipeGlobalSubprocessId);

            }
            $scope.recipeGroupList.splice($scope.index, 1);
            $scope.index = -1;
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

}
