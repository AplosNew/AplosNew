'use strict';
recipeMaterialGroupingMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function recipeMaterialGroupingMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Recipe Material Grouping';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.recipeMaterialGroupingMasters = [];
    $scope.path = 'productions/recipeMaterialGroupingMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.recipeMaterialGroupingMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
   
    $scope.recipeMaterialGroupingMaster = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        QtyValue: null,
        UomId: null,
        Active: true
    };

    $scope.recipeMaterialGroupingMasterNew = Object.assign({}, $scope.recipeMaterialGroupingMaster);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.recipeMaterialGroupingMasterNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (data, index) {
        $scope.index = index;
        $scope.recipeMaterialGroupingMaster = $scope.recipeMaterialGroupingMasters[$scope.index];
        $scope.recipeMaterialGroupingMasterNew = Object.assign({}, $scope.recipeMaterialGroupingMaster);
        $scope.getDetailData(data.Id);

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.UnitOfMeasurementList = [];

    cboService.getUnitOfMeasurementCbo(function (result) {
        $scope.UnitOfMeasurementList = result;

    });

    $scope.Save = function () {
        angular.copy($scope.recipeMaterialGroupingMasterNew, $scope.recipeMaterialGroupingMaster);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.recipeMaterialGroupingMasterNewForm.$valid) {
            if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.recipeMaterialGroupingMaster,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.recipeMaterialGroupingMasterNew.Id = response.data.RecipeMaterialGroupingMaster.Id;
                        $scope.getDetailData($scope.recipeMaterialGroupingMasterNew.Id);
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }

        }
    };

    $scope.hasArticle = false;

    $scope.getAttrValue = function (data) {
        $scope.modelNew.AttributeValueId = data.MaterialAttributeValueId;//MaterialAttributeValueId
        $scope.modelNew.AttributeValueName = data.UserName;
        manualValidation('div_arCh', false);
        $scope.closeValuePopUp();
    };

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

    $scope.closeValuePopUp = function () {
        angular.element(document.querySelector('#attributeValuePopUp')).modal('hide');
        CloseModalShowResult('attributeValuePopUp');
    };

    $scope.modelNew = {
        Id: null,
        RecipeMaterialGroupingMasterId: null,
        MaterialMasterId: null,
        ArticleId: null,
        UomId: null,
        Value: 0
    };

    $scope.rawPopUp = function () {
        $scope.uom = $("#uomdrp option:selected").text();
        $scope.QtyValue = $scope.recipeMaterialGroupingMasterNew.QtyValue + ' ' + $scope.uom;
        $scope.modelNew = {};
        $scope.uomList = [];
        angular.element(document.querySelector('#rawMaterialPopup')).modal('show');

    };

    $scope.closeRawPopUp = function () {
        $scope.detailId = null;
        angular.element(document.querySelector('#rawMaterialPopup')).modal('hide');
    };

    $scope.selectMaterialByType = function (ob) {
        try {
            if ($scope.flag_for_mm === '') {// searching nonFG material
                $scope.modelNew.MaterialMasterId = ob.Id;
                $scope.modelNew.MaterialMasterName = ob.UserName;

                $scope.modelNew.ArticleId = null;
                $scope.modelNew.ArticleName = null;
            }

            cboService.getMeasurementCbo($scope.modelNew.MaterialMasterId, function (result) {
                $scope.uomList = result;
                for (var i = 0; i < $scope.uomList.length; i++) {
                    $scope.modelNew.UomId = $scope.uomList[0].Value;
                }
            });

            $scope.hasArticle = ob.HasAttribute;
            $scope.hasSku = ob.WithSKU;
            if (ob.HasAttribute) {
                $scope.getArticleSearchList(ob.Id);
            } else {

                //save starts during fg adding
                if ($scope.flag_for_mm === 'mm') {// searching FG material
                    $scope.flag_for_mm = '';
                    $scope.operationNew.RecipeGlobalMasterId = $scope.master.Id;
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
                            $scope.getRecipeRawMaterialListByMaster($scope.master.Id);
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

    $scope.flag_for_mm = '';
    $scope.getMaterialMasterbyTypePopUp = function (flag) {

        $scope.materialType = 'Recipe';

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
        $scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        $scope.searchMaterialMasterList = [];
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

        if (parseFloat($scope.modelNew.Value) === 0 || baseService.isUndefinedOrNull($scope.modelNew.Value) || isNaN($scope.modelNew.Value))
            return manualValidation('div_rmValue', true, 'Value is required.');
        else manualValidation('div_rmValue', false);
    }

    $scope.operationNew = {
        RecipeMaterialGroupingMasterId: null,
        MaterialMasterId: null,
        ArticleId: null
    };

    $scope.selectArticle = function (data1) {

        if ($scope.flag_for_mm === 'mm') {
            $scope.flag_for_mm = '';

            $scope.operationNew.RecipeGlobalMasterId = $scope.master.Id;
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
                    //by monir
                    $scope.getRecipeRawMaterialListByMaster($scope.master.Id);
                    //$scope.rptConfigTemplates.push(response.data.RptConfigTemplate);
                    //baseService.paginationAdd();
                    //ClearFields();
                    //$scope.getData();
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

    $scope.rawMaterialList = [];
    $scope.addRawMaterial = function () {
        try {
            if (manualValidationAddRemove()) return;
            if (baseService.isUndefinedOrNull($scope.modelNew.Value)) {
                throw "Raw Material value is required.";
            }
            if (baseService.isUndefinedOrNull($scope.modelNew.UomId)) {
                throw "Raw Material uom is required.";
            }
            $scope.uom = $("#Uom option:selected").text();

            for (var i = 0; i < $scope.rawMaterialList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.modelNew.ArticleId)) {
                    if ($scope.rawMaterialList[i].MaterialMasterId === $scope.modelNew.MaterialMasterId
                        && $scope.rawMaterialList[i].ArticleId === $scope.modelNew.ArticleId) {
                        throw "Material/Article already exist.";
                    }
                }
                else {
                    if ($scope.rawMaterialList[i].MaterialMasterId === $scope.modelNew.MaterialMasterId && $scope.rawMaterialList[i].ArticleId === $scope.modelNew.ArticleId) {
                        throw "Material already exist.";
                    }
                }
            }
            $scope.rawMaterialList.push({
                Id: null
                , MaterialMasterId: $scope.modelNew.MaterialMasterId
                , MaterialMasterName: $scope.modelNew.MaterialMasterName
                , ArticleId: $scope.modelNew.ArticleId
                , ArticleName: $scope.modelNew.ArticleName
                , Value: $scope.modelNew.Value
                , UomId: $scope.modelNew.UomId
                , Description: $scope.uom
            });

            $scope.SaveRawMaterial();

            $scope.modelNew.MaterialMasterId = null;
            $scope.modelNew.MaterialMasterName = null;
            $scope.modelNew.ArticleId = null;
            $scope.modelNew.ArticleName = null;
            $scope.modelNew.Value = null;
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.mRMNew = {
        Id: null
        , RecipeMaterialGroupingMasterId: null
        , MaterialMasterId: null
        , ArticleId: null
        , Value: null
        , UomId: null
    };
    $scope.mRMNewSave = Object.assign({}, $scope.mRMNew);

    $scope.SaveRawMaterial = function () {
        try {
            //$scope.getRecipeRawMaterialList($scope.MasterSubProcessId);

            $scope.mRMNewSave.RecipeMaterialGroupingMasterId = $scope.recipeMaterialGroupingMasterNew.Id;
            $scope.mRMNewSave.MaterialMasterId = $scope.modelNew.MaterialMasterId;
            $scope.mRMNewSave.ArticleId = $scope.modelNew.ArticleId;
            $scope.mRMNewSave.Value = $scope.modelNew.Value;
            $scope.mRMNewSave.UomId = $scope.modelNew.UomId;

            $http({
                method: 'POST',
                url: $scope.path + 'CreateRecipeMaterialGroupingDetail',
                dataType: 'JSON',
                data: { 'recipeMaterialGroupingDetail': $scope.mRMNewSave }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {

                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.uomList=[];
                    //$scope.SaveDetailChildDisabled = false;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.getDetailData = function (masterid) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetRecipeMaterialGroupingDetailList?masterid=' + masterid
        }).then(function successCallback(response) {
            $scope.rawMaterialList = [];
            $scope.rawMaterialList = response.data;
        });
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.recipeMaterialGroupingMasterNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.recipeMaterialGroupingMasterNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.recipeMaterialGroupingMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.popUpIndex = -1;
    $scope.DeleteRawMaterial = function (data, index) {
        try {
            $scope.popUpIndex = index;
            $scope.name = data.MaterialMasterName;
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
            // url: $scope.deleteUrlDetailChild,
            url: $scope.path + 'deleterawmaterial',
            dataType: 'JSON',
            data: { 'rawmaterialid': $scope.Id } //$scope.rawMaterial.Id }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //reload other child

                $scope.getDetailData($scope.recipeMaterialGroupingMasterNew.Id);

            }
            $scope.rawMaterialList.splice($scope.index, 1);
            $scope.index = -1;
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.recipeMaterialGroupingMaster = {};
        $scope.recipeMaterialGroupingMasterNew = {};
        $scope.recipeMaterialGroupingMasterNew.Sequence = seq;
        $scope.recipeMaterialGroupingMasterNew.Active = true;
        $scope.recipeMaterialGroupingMasterNew.Id = null;
        $scope.rawMaterialList = [];
    }
}