'use strict';
materialMasterArticleController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$controller','$window'];
function materialMasterArticleController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $controller, $window) {
    $rootScope.title = "Material Master Article";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.buyerStyles = [];
    $scope.showTbl = false;
    $scope.path = 'Materials/materialmasterarticle/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.savePGUrl = $scope.path + 'CreateProductionGrouping';
    $scope.deletePGUrl = $scope.path + 'DeleteProductionGrouping/';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getSeqUrl = $scope.path + 'GetAutoSequence/';

    $scope.partyType = "Party";
    $controller("partyBaseController", { $scope: $scope, $http: $http });

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.model = {
        Id: null
        , Code: null
        , ShortName: null
        , StanderName: null
        , UserName: null
        , BaseUoM: null
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.model = {};
        $scope.articleList = [];
    }

    // #region ddl
    getOurStyle();
    $scope.styleList = [];
    function getOurStyle() {
        $http({
            method: 'GET',
            url: 'Materials/ourstyle/getcbo/'
        }).then(function successCallback(response) {
            $scope.styleList = response.data;
        });
    }
    // #endregion ddl

    // #region MM

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

    $scope.columnExcluedList = ['WithSKU', 'Description', 'Active', 'IsInventory', 'IsExpenseOut', 'IsAsset	', 'AssetMasterName', 'AssetType', 'IsRevenue'];
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
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
    $scope.popUp = function () {
        $scope.popUpUrl = 'Materials/materialmaster/GetMaterialMasterActiveItemPopUp';
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };
    $scope.selectDoubleClick = function (data) {
        $scope.model = data;
        $scope.MaterialHSNCodeId = data.HSNCodeId;
        getAttribute();
        getArticle();
        $scope.closePopUp();
    };
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData))
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        $scope.selectDoubleClick($scope.valueData);
        $scope.closePopUp();
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    // #endregion MM

    // #region article

    $scope.searchFreeField = false;
    $scope.attributeList = [];
    $scope.articleList = [];
    $scope.articleHead = [];

    $scope.article = {
        Id: null
        , MaterialMasterId: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , HSNCodeId: null
        , MaterialMasterArticleValues: []
        , IsWorkCenterApplicable: false
        , IsMachineApplicable: false
        , OrderLevel: null
        , ProductionGroupingId: null
        , ProcessSetId: null
    };
    $scope.articleNew = Object.assign({}, $scope.article);

    $scope.articleFormPopUp = function () {
        $scope.articleNew = {
            Id: null
            , MaterialMasterId: null
            , Code: null
            , ShortName: null
            , StandardName: null
            , HSNCodeId: null
            , Active: true
            , MaterialMasterArticleValues: []
            , IsWorkCenterApplicable: false
            , IsMachineApplicable: false
            , OrderLevel: null
        };
        getAttribute();
        angular.element(document.querySelector('#articlePoUp')).modal('show');
    };

    function getAttribute() {
        $scope.attributeList = [];
        $http({
            method: 'GET',
            url: 'Materials/materialmaster/getmaterialmasterattributelist?materialMasterId=' + $scope.model.Id
        }).then(function successCallback(response) {
            $scope.attributeList = response.data;
            if (baseService.arrayLength(response.data) === 0)
                return ShowResult('This material has no attribute', 'failure');
            for (var i = 0; i < $scope.attributeList.length; i++) {
                $scope.searchFreeField = $scope.attributeList[i].MaterialAttributeValueFreeText !== null ? true : false;
                var isFree = $scope.attributeList[i].IsFreeField;
                $scope.attributeList[i].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
            }
        });
    }

    $scope.CloseArticlePopUp = function () {
        articleClear();
        angular.element(document.querySelector('#articlePoUp')).modal('hide');
        CloseModalShowResult('articlePoUp');
    };
    $scope.id = null
    $scope.AddArticle = function () {
        try {

            if (baseService.arrayLength($scope.attributeList) === 0)
                throw 'This material has no attribute';

            $scope.id = $scope.articleNew.Id;
            //if (!baseService.isUndefinedOrNull($scope.articleNew.Id) && !baseService.isUndefinedOrNull(id.startsWith('n-')))
            //    articleFieldValidation($scope.articleNew.Code, 'Code');
            articleFieldValidation($scope.articleNew.ShortName, 'ShortName');
            articleFieldValidation($scope.articleNew.StandardName, 'StandardName');

            for (var i = 0; i < $scope.attributeList.length; i++) {
                var _invalid = $scope.IsMandatoryButNull($scope.attributeList[i].IsMandatory, $scope.attributeList[i].MaterialAttributeValueFreeText);
                if (_invalid)
                    throw $scope.attributeList[i].MaterialAttributeName + ' value is required!';
            }
            uniqueCheckInArticleList($scope.articleList, $scope.articleNew);






            //var getRow = $filter("filter")($scope.articleList[t].MaterialMasterArticleValues, {
            //    "MaterialMasterArticleId": $scope.attributeList[i].MaterialMasterArticleId,"MaterialAttributeValueFreeText": $scope.attributeList[i].MaterialAttributeValueFreeText, "MaterialAttributeName": $scope.attributeList[i].MaterialAttributeName
            //});
            //if (getRow.length===1) {
            //    throw 'This combination already exist.!';
            //}

            //$scope.MaterialMasterArticleValueslist = [];
            //for (var t = 0; t < $scope.articleList.length; t++) {
            //    $scope.MaterialMasterArticleValueslist = $scope.articleList[t].MaterialMasterArticleValues;

            //    //if (!materialValueDuplecateCheck($scope.articleList[t].MaterialMasterArticleValues, $scope.attributeList))
            //    //    throw 'This combination already exist.!';

            //    if (!$scope.MaterialMasterArticleValueslist.includes($scope.attributeList[i])) {
            //        //do nothing........
            //    } else {
            //        throw 'This combination already exist.!';
            //    }

            //}
            //$scope.MainList=[];
            //for (var t = 0; t < $scope.articleList.length; t++) {

            //    for (var M = 0; M < $scope.articleList[t].MaterialMasterArticleValues.length; M++) {
            //        $scope.MainList.push($scope.articleList[t].MaterialMasterArticleValues[M]);
            //    }

            //}
            //var tempList;
            //for (var i = 0; i< $scope.attributeList.length; i++) {

            //    var getRow = $filter("filter")($scope.MainList, {
            //        "MaterialAttributeValueFreeText": $scope.attributeList[i].MaterialAttributeValueFreeText, "MaterialAttributeName": $scope.attributeList[i].MaterialAttributeName
            //    });

            //    $scope.MainList = getRow;
            //    //if (getRow.length === 1) {
            //    //    throw 'This combination already exist.!';
            //    //}

            //}

            //if (MainList.length === 1) {
            //        throw 'This combination already exist.!';
            //    }

            $.ajax({
                type: "POST",
                url: 'Materials/materialmasterarticle/Comapre',
                data: { 'allArticles': $scope.articleList, 'currentArticles': $scope.attributeList },
                dataType: "json",
                success: function (data) {

                    if (data.Error === true) {

                        ShowResult(data.Message, 'failure', 'articlePoUp');
                    }
                    else {

                        $scope.articleHead = [];
                        getarticleHed($scope.attributeList, $scope.articleHead, false);
                        $scope.articleNew.MaterialMasterId = $scope.model.Id;

                        angular.forEach($scope.attributeList, function (element, i) {
                            $scope.articleNew.MaterialMasterArticleValues.push({
                                //Id: baseService.pk()
                                Id: null
                                , MaterialAttributeId: element.MaterialAttributeId
                                , MaterialAttributeName: element.MaterialAttributeName
                                , MaterialAttributeValueFreeText: element.MaterialAttributeValueFreeText
                                , MaterialAttributeValueId: element.MaterialAttributeValueId
                                , MaterialMasterArticleId: $scope.articleNew.Id
                                , MaterialMasterAttributeId: element.MaterialMasterAttributeId
                                , MaterialMasterAttributeValueId: baseService.isUndefinedOrNull(element.MaterialMasterAttributeValueId) ? 0 : element.MaterialMasterAttributeValueId
                                , MaterialMasterId: $scope.model.Id
                            });
                        });

                        $scope.article = Object.assign({}, $scope.articleNew);

                        $scope.articleList.push($scope.article);

                        CloseModalShowResult('articlePoUp');
                        articleClear();
                        angular.element(document.querySelector('#articlePoUp')).modal('hide');

                    }

                }

            });



        } catch (e) {
            ShowResult(e, 'failure', 'articlePoUp');
        }

    };

    function uniqueCheckInArticleList(mainList, model) {
        for (var i = 0; i < mainList.length; i++) {
            if ($scope.index !== i) {
                if (!baseService.isUndefinedOrNull(model.Id) && mainList[i].Code === model.Code)
                    throw 'Code is already exist in grid.!';
                else if (mainList[i].ShortName === model.ShortName)
                    throw 'Short name is already exist in grid.!';
                else if (mainList[i].StandardName === model.StandardName)
                    throw 'Standard name is already exist in grid.!';
            }
        }
    }

    //function materialValueDuplecateCheck(list, tempList) {
    //    var hasDifferent = false;
    //    for (var i = 0; i < list.length; i++) {
    //        if (list[i].MaterialAttributeValueFreeText !== tempList[i].MaterialAttributeValueFreeText) {
    //            hasDifferent = true;
    //            break;
    //        }
    //    }
    //    return hasDifferent;
    //} //// tanvir

    function materialValueDuplecateCheck(list, tempList) {
        var hasDifferent = false;
        for (var i = 0; i < tempList.length; i++) {
            for (var j = 0; j < list.length; j++) {
                if (list[i].Id !== tempList[j].Id && list[i].MaterialAttributeValueFreeText !== tempList[j].MaterialAttributeValueFreeText && list[i].MaterialAttributeName !== tempList[j].MaterialAttributeName) {
                    hasDifferent = true;
                    break;
                }
            }

        }
        return hasDifferent;
    }/// mizan

    //function materialValueDuplecateCheck(list, tempList) {
    //    var IsAvailable = false;
    //    for (var i = 1; i < list.length; i++) {
    //        var vv = list[i].MaterialAttributeValueFreeText;
    //        var name = list[i].MaterialAttributeName;
    //        IsAvailable = HasValue(tempList, vv, name);
    //        //IsAvailable = HasValue(tempList, name);
    //        if (IsAvailable) {
    //            break;
    //        }
    //    }
    //    return IsAvailable;
    //}
    //function HasValue(templist, savedone, name) {
    //    var IsAvailable = false;
    //    if (!baseService.isUndefinedOrNull(savedone) && !baseService.isUndefinedOrNull(name)) {
    //        for (var i = 0; i < templist.length; i++) {
    //            if (templist[i].MaterialAttributeValueFreeText === savedone && templist[i].MaterialAttributeName === name) {
    //                IsAvailable = true;
    //                break;
    //            }
    //        }
    //    }
    //    return IsAvailable;
    //}
    //function HasValue(templist, name) {
    //    var IsAvailable = false;
    //    if (!baseService.isUndefinedOrNull(name)) {
    //        for (var i = 0; i < templist.length; i++) {
    //            if (templist[i].MaterialAttributeName === name) {
    //                IsAvailable = true;
    //                break;
    //            }
    //        }
    //    }
    //    return IsAvailable;
    //}
    function articleClear() {
        $scope.articleNew = {
            Id: null
            , MaterialMasterId: null
            , Code: null
            , ShortName: null
            , StandardName: null
            , HSNCodeId: null
            , MaterialMasterArticleValues: []
        };
        $scope.index = -1;
    }
    $scope.hsnCodeList = [];
    cboService.getHNSCbo(function (response) {
        $scope.hsnCodeList = response;
    });
    function articleFieldValidation(field, fieldName) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw fieldName + ' is required.!';
            }
        } catch (e) {
            throw e;
        }
    }

    function getarticleHed(list, newList, flag) {
        if (flag) {
            for (var i = 0; i < list.length; i++) {
                newList.push({ MaterialAttributeName: list[i].MaterialAttribute.UserName });
            }
        }
        else {
            for (var t = 0; t < list.length; t++) {
                newList.push({ MaterialAttributeName: list[t].MaterialAttributeName });
            }
        }
    }

    $scope.articleIndex = -1;
    $scope.deleteModal = function (data, index) {
        $scope.articleIndex = index;
        $scope.deleteId = data.Id;
        $scope.articleMessage = 'Are you sure want to permanently delete ' + data.Code + '.?';
        angular.element(document.querySelector('#deleteModal')).modal('show');
    };

    //$scope.removeArticleRow = function () {
    //    $scope.articleList.splice($scope.articleIndex, 1);
    //    $scope.articleIndex = -1;
    //};


    $scope.removeArticleRow = function () {
        if (baseService.isUndefinedOrNull($scope.deleteId)) {
            $scope.articleList.splice($scope.articleIndex, 1);
            $scope.articleIndex = -1;
        } else {
            $http({
                method: 'POST',
                url: 'materials/materialmasterarticle/deletemaster',
                dataType: 'JSON',
                data: { 'Id': $scope.deleteId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    getArticle();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
    };

    $scope.edit = function (data, index) {
        $scope.attributeList = [];
        $scope.articleNew.Id = data.Id;
        $scope.articleNew.Code = data.Code;
        $scope.articleNew.ShortName = data.ShortName;
        $scope.articleNew.StandardName = data.StandardName;
        $scope.articleNew.UserName = data.UserName;
        $scope.articleNew.Active = data.Active;
        $scope.articleNew.IsWorkCenterApplicable = data.Active;
        $scope.articleNew.IsWorkCenterApplicable = data.IsWorkCenterApplicable;
        $scope.articleNew.IsMachineApplicable = data.IsMachineApplicable;
        $scope.articleNew.OrderLevel = data.OrderLevel;
        $scope.articleNew.ProductionGroupingId = data.ProductionGroupingId;
        $scope.articleNew.Description = data.Description;
        $scope.articleNew.ProcessSetId = data.ProcessSetId;
        if (baseService.isUndefinedOrNull(data.HSNCodeId))
            $scope.articleNew.HSNCodeId = $scope.MaterialHSNCodeId;
        else
            $scope.articleNew.HSNCodeId = data.HSNCodeId;
        $scope.index = index;
        $http({
            method: 'GET',
            url: 'Materials/materialmaster/getmaterialmasterattributelist?materialMasterId=' + $scope.model.Id
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) === 0)
                return ShowResult('This material has no attribute', 'failure', 'articlePoUp');
            $scope.attributeList = response.data;
            for (var i = 0; i < $scope.attributeList.length; i++) {
                for (var t = 0; t < baseService.arrayLength(data.MaterialMasterArticleValues); t++) {
                    if (data.MaterialMasterArticleValues[t].MaterialAttributeId === $scope.attributeList[i].MaterialAttributeId) {
                        $scope.attributeList[i].Id = data.MaterialMasterArticleValues[t].Id;
                        $scope.attributeList[i].MaterialMasterArticleId = data.MaterialMasterArticleValues[t].MaterialMasterArticleId;
                        $scope.attributeList[i].MaterialAttributeId = data.MaterialMasterArticleValues[t].MaterialAttributeId;
                        $scope.attributeList[i].MaterialAttributeValueId = data.MaterialMasterArticleValues[t].MaterialAttributeValueId;
                        $scope.attributeList[i].MaterialAttributeValueFreeText = data.MaterialMasterArticleValues[t].MaterialAttributeValueFreeText;
                        $scope.attributeList[i].MaterialMasterAttributeId = data.MaterialMasterArticleValues[t].MaterialMasterAttributeId;
                        $scope.attributeList[i].MaterialMasterAttributeValueId = data.MaterialMasterArticleValues[t].MaterialMasterAttributeValueId;
                    }
                }
                $scope.searchFreeField = $scope.attributeList[i].MaterialAttributeValueFreeText !== null ? true : false;
                var isFree = $scope.attributeList[i].IsFreeField;
                $scope.attributeList[i].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
            }
        });
        angular.element(document.querySelector('#articlePoUp')).modal('show');
    };



    $scope.updateArticle = function () {
        try {
            //if (!baseService.isUndefinedOrNull($scope.articleNew.Id) && !baseService.isUndefinedOrNull($scope.id.startsWith('n-')))
            if (!baseService.isUndefinedOrNull($scope.articleNew.Id))
                articleFieldValidation($scope.articleNew.Code, 'Code');
            articleFieldValidation($scope.articleNew.ShortName, 'ShortName');
            articleFieldValidation($scope.articleNew.StandardName, 'StandardName');


            uniqueCheckInArticleList($scope.articleList, $scope.articleNew);

            $scope.articleList[$scope.index].Code = $scope.articleNew.Code;
            $scope.articleList[$scope.index].ShortName = $scope.articleNew.ShortName;
            $scope.articleList[$scope.index].StandardName = $scope.articleNew.StandardName;
            $scope.articleList[$scope.index].UserName = $scope.articleNew.UserName;
            $scope.articleList[$scope.index].HSNCodeId = $scope.articleNew.HSNCodeId;
            $scope.articleList[$scope.index].Active = $scope.articleNew.Active;
            $scope.articleList[$scope.index].ProductionGroupingId = $scope.articleNew.ProductionGroupingId;
            $scope.articleList[$scope.index].Description = $scope.articleNew.Description;
            $scope.articleList[$scope.index].ProcessSetId = $scope.articleNew.ProcessSetId;

            for (var i = 0; i < $scope.attributeList.length; i++) {
                var _invalid = $scope.IsMandatoryButNull($scope.attributeList[i].IsMandatory, $scope.attributeList[i].MaterialAttributeValueFreeText);
                if (_invalid) throw $scope.attributeList[i].MaterialAttributeName + ' value is required!';
            }
            //for (var t = 0; t < $scope.articleList.length; t++) {
            //    if ($scope.index !== t && !materialValueDuplecateCheck($scope.articleList[t].MaterialMasterArticleValues, $scope.attributeList))
            //        throw 'This combination already exist.!';
            //}


            for (var m = 0; m < $scope.attributeList.length; m++) {
                for (var n = 0; n < baseService.arrayLength($scope.articleList[$scope.index].MaterialMasterArticleValues); n++) {
                    if (!baseService.isUndefinedOrNull($scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialAttributeId)) {
                        if ($scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialAttributeId === $scope.attributeList[m].MaterialAttributeId) {
                            $scope.articleList[$scope.index].MaterialMasterArticleValues[n].Id = $scope.attributeList[m].Id;
                            $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialMasterId = $scope.attributeList[m].MaterialMasterId;
                            $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialMasterArticleId = $scope.attributeList[m].MaterialMasterArticleId;
                            $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialAttributeId = $scope.attributeList[m].MaterialAttributeId;
                            $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialAttributeValueId = $scope.attributeList[m].MaterialAttributeValueId;
                            $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialAttributeValueFreeText = $scope.attributeList[m].MaterialAttributeValueFreeText;
                            $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialMasterAttributeId = $scope.attributeList[m].MaterialMasterAttributeId;
                            $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialMasterAttributeValueId = $scope.attributeList[m].MaterialMasterAttributeValueId;
                        }
                    } else {
                        $scope.articleList[$scope.index].MaterialMasterArticleValues[n].Id = $scope.attributeList[m].Id;
                        $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialMasterId = $scope.attributeList[m].MaterialMasterId;
                        $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialMasterArticleId = $scope.attributeList[m].MaterialMasterArticleId;
                        $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialAttributeId = $scope.attributeList[m].MaterialAttributeId;
                        $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialAttributeValueId = $scope.attributeList[m].MaterialAttributeValueId;
                        $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialAttributeValueFreeText = $scope.attributeList[m].MaterialAttributeValueFreeText;
                        $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialMasterAttributeId = $scope.attributeList[m].MaterialMasterAttributeId;
                        $scope.articleList[$scope.index].MaterialMasterArticleValues[n].MaterialMasterAttributeValueId = $scope.attributeList[m].MaterialMasterAttributeValueId;
                    }
                }
            }
            articleClear();
            angular.element(document.querySelector('#articlePoUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure', 'articlePoUp');
        }
    };

    // #endregion article

    // #region value

    $scope.valueindex = -1;
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
    $scope.valueParameters = {
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
    $scope.valuePoUp = function (data, index) {
        $scope.materialAttributeValueUrl = 'Materials/MaterialMasterArticle/GetAttributeValueList';
        baseService.setCurrentPage('valueList');
        $scope.getValueData = function (pageno) {
            $scope.valueParameters.assignment = data.ValueAssignmentLevel;
            $scope.valueParameters.materialMasterId = data.MaterialMasterId;
            $scope.valueParameters.attributeId = data.MaterialAttributeId;
            baseService.paginationBase($scope.materialAttributeValueUrl, pageno, $scope.valueParameters)
                .then(function (result) {
                    $scope.valueList = result.Rows;
                    $scope.valueParameters.total_count = result.Total;
                    $scope.valueindex = index;
                    $scope.searchFreeField = true;
                    angular.element(document.querySelector('#attributeValuePopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getValueData();
    };
    $scope.getAttrValue = function (data) {
        $scope.attributeList[$scope.valueindex].MaterialAttributeValueId = data.MaterialAttributeValueId;
        $scope.attributeList[$scope.valueindex].MaterialMasterAttributeValueId = data.MaterialMasterAttributeValueId;
        $scope.attributeList[$scope.valueindex].MaterialAttributeValueFreeText = data.UserName;
        $scope.attributeList[$scope.valueindex].FlagDisable = $scope.searchFreeField;
        $scope.valueindex = -1;
        angular.element(document.querySelector('#attributeValuePopUp')).modal('hide');
    };

    $scope.Generate = function () {
        var un = "";
        $scope.stndName = "";
        $scope.srtName = "";
        var finalCon = "";
        var fcon = "";
        for (var i = 0; i < $scope.attributeList.length; i++) {

            finalCon = (baseService.isUndefinedOrNull($scope.attributeList[i].MaterialAttributeValueFreeText) == true ? "" : $scope.attributeList[i].MaterialAttributeValueFreeText) + (baseService.isUndefinedOrNull($scope.attributeList[i].JoiningParameter) == true ? "" : $scope.attributeList[i].JoiningParameter);
            $scope.stndName = $scope.stndName + (finalCon == null ? "" : finalCon);
            $scope.srtName = $scope.srtName + (finalCon == null ? "" : finalCon);
            un = un + (finalCon == null ? "" : finalCon);
        }

        $scope.articleNew.ShortName = $scope.stndName;
        $scope.articleNew.StandardName = $scope.stndName;
        $scope.articleNew.UserName = un;
    }

    $scope.materialAttributeValueClear = function (index) {
        $scope.attributeList[index].MaterialAttributeValueId = null;
        $scope.attributeList[index].MaterialMasterAttributeValueId = null;
        $scope.attributeList[index].MaterialAttributeValueFreeText = null;
        $scope.searchFreeField = false;
        var isFree = $scope.attributeList[index].IsFreeField;
        $scope.attributeList[index].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
    };
    $scope.closeValuePopUp = function () {
        angular.element(document.querySelector('#attributeValuePopUp')).modal('hide');
        CloseModalShowResult('attributeValuePopUp');
    };

    // #endregion value

    $scope.idNullByFreeText = function (id, index) {
        if ($scope.attributeList[index].MaterialAttributeId === id) {
            $scope.attributeList[index].MaterialAttributeValueId = null;
            $scope.attributeList[index].MaterialMasterAttributeValueId = null;
        }
    };
    $scope.IsFreeFieldOrNot = function (IsFreeField) {
        if (IsFreeField) {
            if ($scope.searchFreeField)
                return true;//disabled true
            else
                return false;//disabled false
        }
        else
            return true;//disabled true
    };
    $scope.IsMandatoryButNull = function (isMandatory, materialAttributeValueFreeText) {
        if (isMandatory) {
            if (baseService.isUndefinedOrNull(materialAttributeValueFreeText)) return true;
            else return false;
        }
        else return false;
    };

    // #region Artilce & Value

    function getArticle() {
        $scope.articleHead = [];
        $scope.articleList = [];
        $http({
            method: 'GET'
            , url: 'Materials/materialmasterarticle/getlist?materialMasterId=' + $scope.model.Id
            , contentType: "application/json; charset=utf-8"
        }).then(function successCallback(response) {
            var articles = response.data;
            if (articles.length > 0) {
                $http({
                    method: 'GET',
                    url: 'Materials/materialmasterarticle/GetArticleValueList?materialMasterId=' + $scope.model.Id,
                    contentType: "application/json; charset=utf-8"
                }).then(function successCallback(response) {
                    if (baseService.arrayLength(response.data)) {
                        var valueData = response.data;
                        $http({
                            method: 'GET',
                            url: 'Materials/materialmasterarticle/getarticlvaluehead?materialMasterId=' + $scope.model.Id,
                            contentType: "application/json; charset=utf-8"
                        }).then(function successCallback(response) {
                            $scope.articleHead = response.data;
                            if (baseService.arrayLength($scope.articleHead)) {
                                for (var i = 0; i < articles.length; i++) {
                                    articles[i].MaterialMasterArticleValues = [];
                                    for (var a = 0; a < $scope.articleHead.length; a++) {
                                        articles[i].MaterialMasterArticleValues.push({
                                            Id: null
                                            , MaterialMasterId: null
                                            , MaterialMasterAttributeId: null
                                            , MaterialAttributeId: $scope.articleHead[a].MaterialAttributeId
                                            , MaterialAttributeName: $scope.articleHead[a].MaterialAttributeName
                                            , MaterialMasterArticleId: null
                                            , MaterialAttributeValueId: null
                                            , MaterialMasterAttributeValueId: 0
                                            , MaterialAttributeValueFreeText: null
                                        });
                                    }
                                }
                            }
                            for (var t = 0; t < baseService.arrayLength(articles); t++) {
                                var articleRow = Object.assign({}, articles[t]);
                                checkValueSubMaterialId(valueData, articleRow);
                                $scope.articleList.push(articleRow);
                            }
                        });
                    }//if
                    else {
                        for (var t = 0; t < baseService.arrayLength(articles); t++) {
                            articles[t].MaterialMasterArticleValues = [];
                            articles[t].MaterialMasterArticleValues.push({
                                Id: null
                                , MaterialMasterId: null
                                , MaterialMasterAttributeId: null
                                , MaterialAttributeId: null
                                , MaterialAttributeName: null
                                , MaterialMasterArticleId: null
                                , MaterialAttributeValueId: null
                                , MaterialMasterAttributeValueId: 0
                                , MaterialAttributeValueFreeText: null
                            });
                            var articleRow = Object.assign({}, articles[t]);
                            checkValueSubMaterialId(valueData, articleRow);
                            $scope.articleList.push(articleRow);
                        }
                    }
                });
            }
        });
    }
    function checkValueSubMaterialId(valueData, articleRow) {
        for (var v = 0; v < baseService.arrayLength(articleRow.MaterialMasterArticleValues); v++) {
            var valueRow = articleRow.MaterialMasterArticleValues[v];
            for (var tt = 0; tt < baseService.arrayLength(valueData); tt++) {
                if (articleRow.Id === valueData[tt].MaterialMasterArticleId
                    && valueRow.MaterialAttributeId === valueData[tt].MaterialAttributeId) {
                    var newValue = valueData[tt];
                    valueRow.Id = newValue.Id;
                    valueRow.MaterialMasterId = newValue.MaterialMasterId;
                    valueRow.MaterialMasterAttributeId = newValue.MaterialMasterAttributeId;
                    valueRow.MaterialAttributeId = newValue.MaterialAttributeId;
                    valueRow.MaterialAttributeName = newValue.MaterialAttributeName;
                    valueRow.MaterialMasterArticleId = newValue.MaterialMasterArticleId;
                    valueRow.MaterialAttributeValueId = newValue.MaterialAttributeValueId;
                    valueRow.MaterialMasterAttributeValueId = newValue.MaterialMasterAttributeValueId;
                    valueRow.MaterialAttributeValueFreeText = newValue.MaterialAttributeValueFreeText;
                    break;
                }
            }
        }
    }

    // #endregion Artilce & Value

    $scope.Save = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'articles': JSON.stringify($scope.articleList),
                'materialCode': $scope.model.Code
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.model = {};
                $scope.articleList = {};
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.model.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.model.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.fgzones.splice($scope.index, 1);
                    $scope.model = {};
                    $scope.articleList = {};
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.articleId = null;
    $scope.ArticleAliasData = function (data, index) {
        $scope.articleId = data.Id;
        $scope.articleAliasModel = {
            Id: null
            , ArticleId: $scope.articleId
            , Code: null
            , PartyId: null
            , PartyName: null
            , ArticlePartyName: null
            , UserGroup: null
            , Remark: null
        };
        $scope.articleAlias = Object.assign({}, $scope.articleAliasModel);

        $scope.GetArticleAliasDatas();
        angular.element(document.querySelector('#ArticleAliasPoUp')).modal('show');
    };

    $scope.aliasList = [];
    $scope.GetArticleAliasDatas = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getArticleAliaslist?articleId=' + $scope.articleId
        }).then(function successCallback(response) {
            //  $scope.aliasList = response.data;
            $scope.articleAlias = Object.assign({}, response.data[0]);
        });
    }
    $scope.articleAliasModel = {
        Id: null
        , Code: null
        , PartyId: null
        , PartyName: null
        , ArticlePartyName: null
        , UserGroup: null
        , Remark: null
    };
    $scope.articleAlias = Object.assign({}, $scope.articleAliasModel);

    $scope.articleAliasClear = function () {
        $scope.articleAlias = Object.assign({}, $scope.articleAliasModel);
    }

    $scope.closePartyPopUp = function (x) {
        var party = x.data;

        $scope.articleAlias.PartyName = party.UserName;
        $scope.articleAlias.PartyId = party.Id;
        $scope.articleAlias.Code = party.Code;

        $scope.hidePartyPopUp();
    };

    $scope.GetArticleAlias = function (args) {

        $scope.articleAlias = Object.assign({}, args);
        $scope.GetArticleAliasDatas(args.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.SaveArticleAlias = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.articleAlias.PartyId)) {
                throw "Party is required.";
            }
            if (baseService.isUndefinedOrNull($scope.articleAlias.ArticlePartyName)) {
                throw "Article Party Name is required.";
            }
            if (baseService.isUndefinedOrNull($scope.articleAlias.UserGroup)) {
                throw "User Group is required.";
            }


            $http({
                method: 'POST',
                url: $scope.path + 'CreateArticleAlias',
                data: { 'data': $scope.articleAlias },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetArticleAliasDatas();
                    //$scope.articleAliasClear();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure', 'ArticleAliasPoUp');
        }
    };


    $scope.deleteArticleAlias = function (data) {
        $scope.deleteArticleAliasId = data.Id;
        $scope.articleMessage = 'Are you sure want to permanently delete ' + data.Code + '.?';
        angular.element(document.querySelector('#deleteArticleAlias')).modal('show');
    };

    $scope.removeArticlAliaseRow = function () {
        $http({
            method: 'POST',
            url: 'materials/materialmasterarticle/deleteArticleAliasData',
            dataType: 'JSON',
            data: { 'Id': $scope.deleteArticleAliasId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetArticleAliasDatas();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
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
        $scope.popUpUrl = 'Processes/ProcessSet/GetProcessSetListByCompany';
        baseService.setCurrentPage('dataList');
        $scope.processPetParameters.companyId = $window.companyId;
        $scope.getProcessSetList = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.processPetParameters)
                .then(function (result) {
                    $scope.processSetList = result.Rows;
                    $scope.processPetParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processSetPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processSetPopUp')).modal('show');
        $scope.getProcessSetList();
    };

    $scope.selectProcessSet = function (data) {
        $scope.articleNew.ProcessSet = data.Description;
        $scope.articleNew.ProcessSetId = data.Id;
        angular.element(document.querySelector('#processSetPopUp')).modal('hide');
    };


    //#region ProductionGrouping
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.PGModelList = [];

    $scope.getProductionGroupingData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPGList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PGModelList = response.data;
            $scope.GetSequence();
        });
    }
    $scope.getProductionGroupingData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.GetProductionGrouping = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    function containsSpecialChars(str) {
        const specialChars = /[`!@#$%^&*()_+\=\[\]{};':"\\|,.<>\/?~]/;
        return specialChars.test(str);
    }

    $scope.CheckSpecialCharecter = function () {
        try {
            if (containsSpecialChars($scope.ModelNew.UserName)) {
                $scope.ModelNew.UserName = $scope.ModelNew.UserName.substring(0, $scope.ModelNew.UserName.length - 1);
                throw "No special characters allowed for Production Group User Name.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SaveProductionGrouping = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.savePGUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearPGFields(response.data.Sequence);
                    $scope.getProductionGroupingData();
                    $scope.GetProductionGroupingCbo();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };
    $scope.ProductionGroupingCboList = [];
    $scope.GetProductionGroupingCbo = function () {
        $http.get('Materials/materialmasterarticle/GetProductionGroupingCbo')
            .then(function (response) {
                $scope.ProductionGroupingCboList = response.data;
            });
    };
    $scope.GetProductionGroupingCbo();


    $scope.DeleteProductionGrouping = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deletePGUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearPGFields(response.data.Sequence);
                    $scope.getProductionGroupingData();
                    $scope.GetProductionGroupingCbo();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearPG = function () {
        ClearPGFields($scope.GetSequence());
        return true;
    };

    function ClearPGFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }
    //#endregion


}