'use strict';
baseMaterialAndArticleController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$http', '$filter'];
function baseMaterialAndArticleController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $filter) {
    $scope.businessProcesses = null;
    $scope.materialType = null;

    // #region Material Search By Business Process

    $scope.searchList = [];
    $scope.dataPlate = [];
    $scope.searchbyMaterialMasterDatalist = [
        {
            'name': 'Material Type',
            'value': 'MaterialTypeName'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMasterName'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Product',
            'value': 'ProductMasterName'
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
            'value': 'IsAsset'
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
    $scope.getMaterialMasterSearchData = function () {
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
        $scope.popUpUrl = 'Materials/MaterialMaster/MaterialSearchByBusinessProcess?type=' + $scope.businessProcesses;
        baseService.setCurrentPage('materialmasterSearchData');
        $scope.loadMMData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.mmPopUpParameters)
                .then(function (result) {
                    $scope.materialmasterSearchData = result.Rows;
                    $scope.mmPopUpParameters.total_count = result.Total;
                    angular.element(document.querySelector('#materialmastersearchpopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        $scope.loadMMData();
    };

    $scope.closeMaterialMasterSearchPopUp = function () {
        CloseModalShowResult('materialmastersearchpopup');
        angular.element(document.querySelector('#materialmastersearchpopup')).modal('hide');
    };

    // #endregion Material Search By Business Process

    // #region Material Article Search

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
            //$scope.popUpUrl = 'Materials/MaterialMasterArticle/GetMaterialArticle';
            baseService.setCurrentPage('dataPlate');
            $scope.articlePopUpParameters.materialMasterId = id;
           // $scope.articlePopUpParameters.materialType = JSON.stringify($scope.materialType);
            $scope.loadArticleData = function (pageno) {
                baseService.paginationBase('Materials/MaterialMasterArticle/GetMaterialArticle', pageno, $scope.articlePopUpParameters)
                    .then(function (result) {
                        //if (baseService.arrayLength(result.Rows) === 0) return ShowResult('This material has no article', 'failure');
                        $scope.dataPlate = result.Rows;
                        $scope.articlePopUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.searchList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                        if ($scope.articlePopUpParameters.total_count == 0) {
                            ShowResult("This material has no article ", 'failure');
                        }
                        else {

                            angular.element(document.querySelector('#articleSearchPop')).modal('show');
                        }

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

    // #endregion Material Article Search

    // #region Characteristics

    $scope.searchCharFilterList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        }

    ];
    $scope.clearCharNames = function () {
        $scope.char1 = { show: false };
        $scope.char2 = { show: false };
        $scope.char3 = { show: false };
    };
    $scope.getCharacteristicsList = function (id) {
        $scope.clearCharNames();
        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.characteristicsList = [];
            $scope.characteristicsList = response.data.charData;
            if (baseService.arrayLength($scope.characteristicsList) > 0) {
                $scope.isSearch = $scope.characteristicsList[0].FreeText !== null ? true : false;
                $scope.char1 = {
                    CharacteristicsId: $scope.characteristicsList[0].Value
                    , CharacteristicsValueId: $scope.characteristicsList[0].CharacteristicsValueId
                    , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                    , Name: $scope.characteristicsList[0].Text
                    , IsFreeField: $scope.characteristicsList[0].IsFreeField
                    , IsPreDefinedField: $scope.characteristicsList[0].IsPreDefinedField
                    , IsMandatory: $scope.characteristicsList[0].IsMandatory
                    , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                    , Sequence: $scope.characteristicsList[0].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[0].IsFreeField)

                    , FreeText: $scope.characteristicsList[0].FreeText
                    , show: true
                };
            }
            if (baseService.arrayLength($scope.characteristicsList) > 1) {
                $scope.isSearch = $scope.characteristicsList[1].FreeText !== null ? true : false;
                $scope.char2 = {
                    CharacteristicsId: $scope.characteristicsList[1].Value
                    , CharacteristicsValueId: $scope.characteristicsList[1].CharacteristicsValueId
                    , MaterialMasterId: $scope.characteristicsList[1].MaterialMasterId
                    , Name: $scope.characteristicsList[1].Text
                    , IsFreeField: $scope.characteristicsList[1].IsFreeField
                    , IsPreDefinedField: $scope.characteristicsList[1].IsPreDefinedField
                    , IsMandatory: $scope.characteristicsList[1].IsMandatory
                    , ValueAssignmentLevel: $scope.characteristicsList[1].ValueAssignmentLevel
                    , Sequence: $scope.characteristicsList[1].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[1].IsFreeField)
                    , FreeText: $scope.characteristicsList[1].FreeText
                    , show: true
                };
            }
            if (baseService.arrayLength($scope.characteristicsList) > 2) {
                $scope.isSearch = $scope.characteristicsList[2].FreeText !== null ? true : false;
                $scope.char3 = {
                    CharacteristicsId: $scope.characteristicsList[2].Value
                    , CharacteristicsValueId: $scope.characteristicsList[2].CharacteristicsValueId
                    , MaterialMasterId: $scope.characteristicsList[2].MaterialMasterId
                    , Name: $scope.characteristicsList[2].Text
                    , IsFreeField: $scope.characteristicsList[2].IsFreeField
                    , IsPreDefinedField: $scope.characteristicsList[2].IsPreDefinedField
                    , IsMandatory: $scope.characteristicsList[2].IsMandatory
                    , ValueAssignmentLevel: $scope.characteristicsList[2].ValueAssignmentLevel
                    , Sequence: $scope.characteristicsList[2].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[2].IsFreeField)
                    , FreeText: $scope.characteristicsList[2].FreeText
                    , show: true
                };
            }
        });
    };
    $scope.charValueSearchFor = null;
    $scope.charValueCharName = null;
    $scope.findCharValueSearchData = function (data, searchFor) {
        $scope.charValueSearchFor = searchFor;
        $scope.charValueCharName = data.Name;
        $scope.getCharData(data);
    };

    $scope.IsMandatoryButNull = function (isMandatory, value) {
        if (isMandatory) {
            if (baseService.isUndefinedOrNull(value)) return true;
            else return false;
        }
        else return false;
    };
    $scope.isSearch = false;
    $scope.IsFreeOrNot = function (IsFreeField) {
        if (IsFreeField) {
            if ($scope.isSearch) {
                return true;//disabled true
            }
            else
                return false;//disabled false
        }
        else {
            return true;//disabled true
        }
    };
    $scope.clearCharValueField = function (valueFor) {
        $scope[valueFor].CharacteristicsValueId = null;
        $scope[valueFor].FreeText = null;
        $scope[valueFor].FlagDisable = $scope.IsFreeOrNot($scope.char1.IsFreeField);
        $scope.isSearch = false;
    };
    $scope.nullByFreeText = function (valueFor) {
        $scope[valueFor].CharacteristicsValueId = null;
    };
    $scope.getCharData = function (data) {
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
        baseService.setCurrentPage('charDataList');
        $scope.url = '';
        $scope.getSearchCharData = function (pageno) {
            $scope.charValueParameters.assignment = data.ValueAssignmentLevel;
            $scope.charValueParameters.materialMasterId = data.MaterialMasterId;
            $scope.charValueParameters.charId = data.CharacteristicsId;
            baseService.paginationBase('Materials/CharacteristicsValue/getcharacteristicsvaluesearchdata/', pageno, $scope.charValueParameters)
                .then(function (result) {
                    $scope.charDataList = result.Rows;
                    $scope.charValueParameters.total_count = result.Total;
                    $scope.isSearch = true;
                    angular.element(document.querySelector('#searchcharactervaluepopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        //$scope.getSearchCharData1 = function () {
        //	debugger;
        //	$scope.charValueParameters.assignment = data.ValueAssignmentLevel;
        //	$scope.charValueParameters.materialMasterId = data.MaterialMasterId;
        //	$scope.charValueParameters.charId = data.CharacteristicsId;
        //	$http({
        //		method: 'GET',
        //		//url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
        //		url: 'Materials/CharacteristicsValue/getcharacteristicsvaluesearchdata1?assignment=' + $scope.charValueParameters.assignment + "&materialMasterId=" + $scope.charValueParameters.materialMasterId + "&charId=" + $scope.charValueParameters.charId
        //	}).then(function successCallback(response) {
        //		$scope.charDataList = response.data;
        //		//$scope.detailgrid($scope.lst);
        //		//window.lst = response.data;

        //	});
        //};
        //$scope.getSearchCharData1();
        $scope.getSearchCharData();

    };












    // #endregion Characteristics

    // #region Material by material type

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
        },
        {
            'Text': 'Base UoM',
            'Value':'BaseUoM'
        }
        
    ];
    $scope.getMaterialMasterbyTypePopUp = function () {
        //debugger;
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
        if ($scope.materialType === 'BOM') {
            //$scope.popUpUrl = 'Materials/MaterialMaster/GetMaterialListByMaterialTypeBOM?materialType=' + JSON.stringify($scope.materialType);
            $scope.popUpUrl = 'Materials/MaterialMaster/GetMaterialListByMaterialTypeBOM?materialType=' + $scope.materialType;
        }
        else if ($scope.materialType === 'ProductDefinition') {
            $scope.popUpUrl = 'Materials/MaterialMaster/MaterialSearchByBusinessProcess?type=' + $scope.materialType;
        }
        else {

            $scope.popUpUrl = 'Materials/MaterialMaster/GetMaterialListByMaterialType?materialType=' + JSON.stringify($scope.materialType);
        }
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

    // #endregion Material by material type
    $scope.searchByMaterial = "MaterialMasterName"; $scope.search = "";
    $scope.searchByMaterialList = [{ value: 'MaterialMasterName', name: "Material" }, { value: 'StandardName', name: "Article" }, { value: 'MaterialTypeName', name: "MaterialType" }
        , { value: 'MaterialGroupMasterName', name: "MaterialGroup" }, { value: 'HSNCode', name: "HSNCode" }, { value: 'BusinessProcessName', name: "Business Process" }];

    $scope.materialArticleList = [];
    $scope.InputMaterialArticlelistData = {};
    $scope.getMaterialMasterWithArticle = function (data) {
        $http({
            method: 'POST',
            url: 'Materials/MaterialMasterArticle/GetMaterialMasterWithArticlePopUpData?type=' + $scope.materialType,
            data: { column: $scope.searchByMaterial, value: $scope.search },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.materialArticleList = response.data;
        });
        $scope.InputMaterialArticlelistData = data;
        angular.element(document.querySelector('#materialarticleNewPopUp')).modal('show');

    };
    $scope.getMaterialMasterWithArticleBySearch = function () {
        $scope.getMaterialMasterWithArticle($scope.InputMaterialArticlelistData);
    }
    $scope.closeMaterialMasterWithArticle = function () {
        angular.element(document.querySelector('#materialarticleNewPopUp')).modal('show');
    }

    $scope.getMaterialMasterWithCbxArticle = function () {
        $http({
            method: 'POST',
            url: 'Materials/MaterialMasterArticle/GetMaterialMasterWithArticlePopUpData?type=' + $scope.materialType,
            data: { column: $scope.searchByMaterial, value: $scope.search },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.materialArticleList = response.data;
        });
        angular.element(document.querySelector('#materialarticleNewCbxPopUp')).modal('show');

    };

}