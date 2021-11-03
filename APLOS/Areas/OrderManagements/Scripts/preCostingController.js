'use strict';
PreCostingController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "cboService", "$window"];
function PreCostingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preCostings = [];
    $scope.getListUrl = 'OrderManagements/preCosting/getList';
    $scope.searchByPreCostingList = [
        {
            'name': 'Buyer Name',
            'value': 'BuyerName'
        },
        {
            'name': 'Finished Goods',
            'value': 'FinishedGoods'
        },
        {
            'name': 'Criticality',
            'value': 'CriticalName'
        }
    ]
    baseService.init($scope.getListUrl, null, 10, null, 'BuyerName', 'BuyerName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.preCostings = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.buyerList = [];
    cboService.getCboBuyer(function (result) {
        $scope.buyerList = result;
    })
    $scope.styleCategoryList = [];
    function getCriticality() {
        $http({
            method: 'GET',
            url: 'OrderManagements/Critical/GetCbo',
        }).then(function successCallback(response) {
            $scope.styleCategoryList = response.data.Rows;
        });
    }
    getCriticality();
    $scope.currencyList = [];
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = result;
    });

    $scope.preCosting = {
        Id: null,
        IsInquiryLinked: false,
        CompanyGroupId: $window.companyGroupId,
        BuyerId: null,
        MaterialMasterId: null,
        FinishedGoods: null,
        SPT: null,
        CriticalId: null,
        CurrencyId: null,
        SellingPrice: null,
        Remarks: null,
        InquiryFG: null,
        ArticleCode: null,
        ArticleStandardName: null,
        MaterialGroupMasterName: null
    };

    $scope.preCostingNew = Object.assign({}, $scope.preCosting);
    $scope.getGenericPopUpForInquiyLinkedForm = function () {
        $scope.message_confirmation = 'Do you want to link with inquiry?';
        angular.element(document.querySelector('#confirmgenericPopUpForInquiyLinkedForm')).modal('show');
    }
    $scope.inquiryShow = function () {
        $scope.preCostingNew.IsInquiryLinked = true;
        $scope.inquiryPopUp();
    }
    $scope.inquiryHide = function () {
        $scope.preCostingNew.IsInquiryLinked = false;
        angular.element(document.querySelector('#confirmgenericPopUpForInquiyLinkedForm')).modal('hide');
        $scope.showFinishGoodsModal();
    }
    //
    //*************Inquiry*********************/
    //-------------
    $scope.inquiryDataList = [];
    $scope.searchbyInquiryList = [
        {
            'name': 'Finished Goods',
            'value': 'FinishedGoods'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupName'
        },
        {
            'name': 'Product Master',
            'value': 'ProductMasterName'
        }
    ]
    $scope.inquiryListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'FinishedGoods',
        searchBy: 'FinishedGoods',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getInquiyData = function () {
        $scope.inquiryDataList = [];
        baseService.setCurrentPage('inquiryDataList');
        $scope.loadInquiryData = function (pageno) {
            baseService.paginationBase('OrderManagements/Inquiry/QueryForIsPreCostingInquiry', pageno, $scope.inquiryListParameters)
                .then(function (result) {
                    $scope.inquiryDataList = result.Rows;
                    $scope.inquiryListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadInquiryData();
    };
    $scope.inquiryPopUp = function () {
        $scope.getInquiyData();
        angular.element(document.querySelector('#inquiryPopUp')).modal('show');
    }
    $scope.inquirySelectdCloseListPopUp = function () {
        angular.element(document.querySelector('#inquiryPopUp')).modal('hide');
    }
    $scope.selectInquiryInfo = function (data) {
        $scope.preCostingNew.MaterialMasterId = data.MaterialMasterId;
        $scope.GetPreCostingCalculation($scope.preCostingNew.PlantId)
        //$scope.preCostingNew.InquiryFG = data.FinishedGoods;
        $scope.preCostingNew.FinishedGoods = data.FinishedGoods;
        $scope.preCostingNew.BuyerId = data.BuyerId;
        if (data.MaterialMasterArticleId === null) {
            getArticle();
        } else {
            getArticleInfoOnEdit(data.MaterialMasterArticleId);
        }
        angular.element(document.querySelector('#inquiryPopUp')).modal('hide');
    }
    //
    //******************Material Group Master**************/
    $scope.getALtUomList = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/PreCosting/GetMaterialGroupAltUOMList',
        }).then(function successCallback(response) {
            $scope.alterNativeUomList = response.data;
            $scope.getMaterialGroupMasterSavedList();
        })
    };
    $scope.materialGroupMasterListForSave = [];
    $scope.materialGroupMasterFormSearchPopup = function () {
        if ($scope.preCostingNew.Id != null) {
            $scope.PreCostingId = $scope.preCostingNew.Id;
            $scope.getALtUomList();
        }
        //angular.element(document.querySelector('#materialGroupMasterFormPopUp')).modal('show');
    };
    $scope.getMaterialGroupMasterSavedList = function () {
        $scope.materialGroupMasterListForSave = [];
        $http({
            method: 'GET',
            url: 'OrderManagements/PreCosting/GetPreCostingDetailList?preCostingId=' + $scope.PreCostingId,
        }).then(function successCallback(response) {
            var obList = response.data;
            for (var i = 0; i < obList.length; i++) {
                obList[i].AlernativeUomLists = buildUomDropDown($scope.alterNativeUomList, obList[i].MaterialGroupMasterId);
                $scope.materialGroupMasterListForSave.push(obList[i]);
            }
        });
    }
    $scope.materialGroupMasterFormModalCloseListPopUp = function () {
        angular.element(document.querySelector('#materialGroupMasterFormPopUp')).modal('hide');
    }
    //-------------
    $scope.showMaterialGroupMasterPopUp = function () {
        $scope.getMaterialGroupMasterData();
    };
    $scope.searchByMaterialGroupMasterList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },

        {
            'name': 'Material Type',
            'value': 'MaterialTypeName'
        },
        {
            'name': 'BaseUom',
            'value': 'BaseUom'
        }
    ]
    $scope.materialGroupMasterListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.materialGroupMasterDataList = [];
    $scope.getMaterialGroupMasterData = function () {
        baseService.setCurrentPage('materialGroupMasterDataList');
        $scope.loadMaterialMasterData = function (pageno) {
            baseService.paginationBase('Materials/MaterialGroupMaster/GetList', pageno, $scope.materialGroupMasterListParameters)
                .then(function (result) {
                    $scope.materialGroupMasterDataList = result.Rows;
                    $scope.materialGroupMasterListParameters.total_count = result.Total;
                    angular.element(document.querySelector('#materialGroupMasterPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMaterialMasterData();
    };
    $scope.materialGroupMasterSearchPopup = function () {
        $scope.getMaterialGroupMasterData();
    };
    function checkMaterialGroupMasterExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MaterialMasterId === id) {
                return true;
            }
        }
        return false;
    }
    function getMaterialGroupMasterDataSaveList() {
        angular.forEach($scope.materialGroupMasterDataList, function (item) {
            if (item.Flag) {
                if (checkMaterialGroupMasterExist($scope.materialGroupMasterListForSave, item.Id) === false) {
                    $scope.materialGroupMasterListForSave.push(
                        {
                            MaterialGroupMasterId: item.Id,
                            Id: null,
                            PreCostingId: $scope.PreCostingId,
                            BaseUOMId: item.BaseUoMId,
                            MaterialTypeName: item.MaterialTypeName,
                            UomValue: null,
                            Rate: null,
                            MaterialGroupMasterName: item.UserName,
                            Code: item.Code,
                            BaseUOM: item.BaseUOM,
                            AlernativeUomLists: buildUomDropDown($scope.alterNativeUomList, item.Id),
                            AlternativeUOMId: item.BaseUoMId,
                        }
                    );
                }
            }
        })
    }
    $scope.materialGroupMasterSelectdCloseListPopUp = function () {
        getMaterialGroupMasterDataSaveList();
        angular.element(document.querySelector('#materialGroupMasterPopUp')).modal('hide');
    }
    //-----------------
    /***UOM************/
    var finalUomDropDownList = [];
    function buildUomDropDown(list, id) {
        finalUomDropDownList = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                if (finalUomDropDownList.length > 0) {
                    if (getIsExistsgUOM(finalUomDropDownList, list[i].UoMID) === false) {
                        finalUomDropDownList.push({
                            Text: list[i].UoM,
                            Value: list[i].UoMID,
                            Id: list[i].Id,
                        });
                    }
                } else {
                    finalUomDropDownList.push({
                        Text: list[i].UoM,
                        Value: list[i].UoMID,
                        Id: list[i].Id,
                    });
                }
            }
        }

        return finalUomDropDownList;
    }
    function getIsExistsgUOM(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Value === id) {
                return true;
            }
        }
        return false;
    }
    //
    //*********************************FinishGoods*****************//

    function getProductSavedList(id) {
        var url = '/OrderManagements/Inquiry/GetProductInquiryList?inquiryId=' + id;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            $scope.productInquirySelectedList = response.data;
        });
    }
    //ProductList for modal
    $scope.finishGoodsDataList = [];
    $scope.ShowFinishGoodItemList = function () {
        $scope.searchByFinishGoodList = [
            {
                'name': 'Finished Goods',
                'value': 'FinishedGoods'
            },
            {
                'name': 'Material Group',
                'value': 'MaterialGroupName'
            },
            {
                'name': 'Product Master',
                'value': 'ProductMasterName'
            }
        ];
        baseService.init('OrderManagements/PreCosting/GetFinishGoodsWithCompanyGroup', null, null, null, 'FinishedGoods', 'FinishedGoods');
        $scope.getFinishGoodData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.finishGoodsDataList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getFinishGoodData();
    };
    $scope.showFinishGoodsModal = function () {
        $scope.ShowFinishGoodItemList();
        angular.element(document.querySelector('#finishGoodsModal')).modal('show');
    };
    //Passing Data For IntermediateItemEntity List
    $scope.getSelectFinishGoods = function (data) {
        $scope.preCostingNew.FinishedGoods = data.FinishedGoods;
        $scope.preCostingNew.MaterialMasterId = data.Id;
        getArticle();
        $scope.GetPreCostingCalculation($scope.preCostingNew.PlantId)
        angular.element(document.querySelector('#finishGoodsModal')).modal('hide');
    };
    $scope.hasProductDuplicate = function (list) {
        for (var i = 0; i < list.length; i++) {
            for (var x = i + 1; x < list.length; x++) {
                if (list[i].MaterialMasterId == list[x].MaterialMasterId) {
                    throw list[i].FinishedGoods + " has duplicate row";
                }
            }
        }
    };
    //
    //*************Article*******************************//
    $scope.assetRegisterCharactreristicsList = [];
    $scope.materialArticleInfo = {
        ArticleCode: null,
        ArticleStandardName: null
    }
    $scope.preCostingNew.MaterialMasterArticleId = null;
    function getArticle() {
        $scope.articleHead = [];
        $scope.articleList = [];
        $http({
            method: 'GET',
            url: 'Materials/materialmasterarticle/getarticlvaluehead?materialMasterId=' + $scope.preCostingNew.MaterialMasterId,
            contentType: 'application/json; charset=utf-8',
        }).then(function successCallback(response) {
            $scope.articleHead = response.data;
            $http({
                method: 'GET',
                url: 'Materials/materialmasterarticle/getlist?materialMasterId=' + $scope.preCostingNew.MaterialMasterId,
                contentType: 'application/json; charset=utf-8',
            }).then(function successCallback(response) {
                $scope.articalesTempList = response.data;
                var articles = response.data;
                if ($scope.articleHead.length > 0 && $scope.articalesTempList.length === 0) {
                    return ShowResult("Article is required for this asset item", 'failure', 'assetmodal');
                }
                if (articles.length > 0) {
                    $http({
                        method: 'GET',
                        url: 'Materials/materialmasterarticle/GetArticleValueList?materialMasterId=' + $scope.preCostingNew.MaterialMasterId,
                        contentType: 'application/json; charset=utf-8',
                    }).then(function successCallback(response) {
                        if (baseService.arrayLength(response.data)) {
                            var valueData = response.data
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
                                            , MaterialMasterAttributeValueId: null
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
                            if ($scope.articleList.length > 0 && $scope.Action == "Update")
                                getArticleInfoOnEdit($scope.preCostings[$scope.index].MaterialMasterArticleId);
                            if ($scope.articleList.length > 0 && $scope.Action === "Save") {
                                angular.element(document.querySelector('#materialMasterArticlemodal')).modal('show');
                            }
                        }
                    })
                }
                //getAttribute();
            });
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
    $scope.selectMaterialMasterArticleInfo = function (data) {
        $scope.preCostingNew.ArticleCode = data.Code;
        $scope.preCostingNew.ArticleStandardName = data.StandardName;
        $scope.preCostingNew.MaterialMasterArticleId = data.Id;
        angular.forEach($scope.articleHead, function (item) {
            angular.forEach(data.MaterialMasterArticleValues, function (itemx) {
                if (item.MaterialAttributeId === itemx.MaterialAttributeId) {
                    $scope.preCostingNew[item.MaterialAttributeName] = itemx.MaterialAttributeValueFreeText;
                }
            })
        });
        angular.element(document.querySelector('#materialMasterArticlemodal')).modal('hide');
    }
    function getArticleInfoOnEdit(articleMasterId) {
        var ob;
        angular.forEach($scope.articleList, function (item) {
            if (item.Id === articleMasterId) {
                return ob = item;
            }
        });
        $scope.selectMaterialMasterArticleInfo(ob);
    }

    //*************************End******************//
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.preCosting = $scope.preCostings[$scope.index];
        $scope.preCostingNew = Object.assign({}, $scope.preCosting);
        $scope.Action = "Update";
        $scope.materialGroupMasterFormSearchPopup();
        getArticle();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.preCostingNewForm.$valid) {
            angular.copy($scope.preCostingNew, $scope.preCosting);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "OrderManagements/preCosting/create",
                    data: $scope.preCosting,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.preCostingNew.Id = response.data.PreCosting.Id;
                        $scope.Action = "Update";
                        $scope.materialGroupMasterFormSearchPopup();
                        getArticle();
                        $scope.getData();
                        $scope.preCostings = $filter('orderBy')($scope.preCostings, 'BuyerName');
                        baseService.paginationAdd();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: "OrderManagements/preCosting/edit",
                    data: $scope.preCosting,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.preCostings[$scope.index] = $scope.preCosting;
                            $scope.preCostings = $filter('orderBy')($scope.preCostings, 'Sequence');
                        }
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.preCostingNew.Id)) {
            $http({
                method: 'POST',
                url: "OrderManagements/preCosting/delete/" + $scope.preCostingNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.preCostings.splice($scope.index, 1);
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
    $scope.MaterialGroupMasterSave = function () {
        try {
            $http({
                method: 'POST',
                url: "OrderManagements/preCosting/PreCostingDetailCreate",
                data: $scope.materialGroupMasterListForSave,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    angular.element(document.querySelector('#materialGroupMasterFormPopUp')).modal('hide');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    //Deleting Rows from MaterialFormList
    $scope.valuePassInMaterialFormDelModal = function (index, Id) {
        $scope.MaterialGroupMasterId = Id;
        $scope.mTIndex = index;
        if (baseService.isUndefinedOrNull($scope.MaterialGroupMasterId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.MaterialGroupMasterId + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUpForMaterialForm')).modal('show');
    };

    $scope.DeleteMaterialGroupMasterSavedItem = function () {
        if (baseService.isUndefinedOrNull($scope.MaterialGroupMasterId)) {
            $scope.materialGroupMasterListForSave.splice($scope.mTIndex, 1);
        } else {
            for (var i = 0; i < $scope.materialGroupMasterListForSave.length; i++) {
                if ($scope.materialGroupMasterListForSave[i].Id == $scope.MaterialGroupMasterId) {
                    $http({
                        method: 'POST',
                        url: 'OrderManagements/PreCosting/DeletePreCostingDetail?id=' + $scope.MaterialGroupMasterId,
                    }).then(function successCallback(response) {
                        ShowResult(response.data.Message, 'success');
                        $scope.materialGroupMasterListForSave.splice($scope.mTIndex, 1);
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    })
                }
            }
        }
        $scope.MaterialGroupMasterId = null;
        $scope.mTIndex = -1;
    };
    /****Calculation************/
    $scope.plantList = [];
    $scope.GetPlantList = function () {
        var url = '/OrderManagements/PreCosting/GetPlantWithWorkCenter';
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            $scope.plantList = response.data;
        });
    }
    $scope.GetPlantList();
    $scope.preCostingCalculationEntityList = [];
    $scope.totalWorkStationOfEntity = 0;
    $scope.totalHourlyCostOfEntity = 0;
    $scope.perHourWSCost = 0;
    $scope.noOfWorkStationRequiredForFg = 0;
    $scope.hourlyCostForFG = 0;
    $scope.totalMinuteOfTheWorkStation = 0;
    $scope.finishGoodSPT = 0;
    $scope.finishGoodEfficiency = 0;
    $scope.timeRequiredAtEfficiency = 0;
    $scope.outputPerHour = 0;
    $scope.costOutputPerHour = 0;
    $scope.GetPreCostingCalculation = function (id) {
        var url = '/OrderManagements/PreCosting/GetPreCostingCalculationWithEntity?plantId=' + id + '&fgId=' + $scope.preCostingNew.MaterialMasterId;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            $scope.preCostingCalculationEntityList = response.data;
            getCalculation(id);
        });
    }
    function getCalculation(id) {
        var url = '/OrderManagements/PreCosting/GetPreCostingCalculation?plantId=' + id + '&fgId=' + $scope.preCostingNew.MaterialMasterId;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            $scope.totalWorkStationOfEntity = response.data[0].TotalWorkStationi;
            $scope.totalHourlyCostOfEntity = response.data[0].TotalHourlyCost;
            $scope.perHourWSCost = $scope.totalWorkStationOfEntity / $scope.totalHourlyCostOfEntity;
            GetFGNoOfWorkStation(id);
        });
    }
    function GetFGNoOfWorkStation(id) {
        var url = '/OrderManagements/PreCosting/GetFGNoOfWorkStation?finishGoodId=' + $scope.preCostingNew.MaterialMasterId;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            $scope.noOfWorkStationRequiredForFg = response.data[0].FGNoOfWorkStation;
            $scope.hourlyCostForFG = $scope.noOfWorkStationRequiredForFg * $scope.perHourWSCost;
            $scope.totalMinuteOfTheWorkStation = $scope.noOfWorkStationRequiredForFg * 60;
            $scope.finishGoodSPT = response.data[0].FGSPT;
            $scope.finishGoodEfficiency = response.data[0].EfficencyPercentage;
            $scope.timeRequiredAtEfficiency = $scope.finishGoodSPT / $scope.finishGoodEfficiency;
            $scope.outputPerHour = $scope.totalMinuteOfTheWorkStation / $scope.timeRequiredAtEfficiency;
            $scope.costOutputPerHour = $scope.hourlyCostForFG / $scope.outputPerHour;
        });
    }
    //
    //## Material Group Article Production Process//
    $scope.materialGroupArticleProductionProcessList = [];
    $scope.getMaterialGroupArticleProductionProcessPopUp = function (index, data) {
        $scope.materialGroupMasterTempId = data.MaterialGroupMasterId;
        $scope.productionProcessGroupOutPutValue = data.UomValue;
        angular.element(document.querySelector('#mgProductionProcessGroupArticleFormPopUp')).modal('show');
    }
    function getMGAProductionProcess(id) {
        var url = '/OrderManagements/PreCosting/GetMaterialGroupArticlePrdProcessGroupList?materialGroupArticleId=' + id;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            angular.forEach(response.data, function (item) {
                if (item.Sequence === 1) {
                    item.InputValue = $scope.productionProcessGroupOutPutValue;
                    item.OutputValue = item.InputValue + item.Wastage;
                } else if (item.Sequence > 1) {
                    angular.forEach(response.data, function (x) {
                        if (x.ProductionProcessGroupId === x.InputId) {
                            item.InputValue = item.OutputValue;
                            item.OutputValue = item.InputValue + item.Wastage;
                        }
                    })
                }
            });
            //for (var t = 0; t < baseService.arrayLength(response.data); t++) {
            //    var row = response.data;
            //    if (t === 1 && row.Sequence === 1) {
            //        // input = consumtion value
            //        row.InputValue = $scope.productionProcessGroupOutPutValue;
            //        row.OutputValue = row.InputValue + row.Wastage;
            //    }
            //    else {
            //        var inputValue = $filter("filter")(response.data, { ProductionProcessGroupId: row.InputId });
            //        row.InputValue = inputValue;
            //        row.OutputValue = row.InputValue + row.Wastage;
            //    }
            //}
            $scope.materialGroupArticleProductionProcessList = response.data;
            angular.forEach($scope.materialGroupArticleProductionProcessList, function (item) {
                item.Rate = calculateCriteria($scope.materialGroupProcessCritia, item.Id, item.Output + item.Wastage);
            });
        });
    }
    //## Material Group Article//
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
            'name': 'Material Group Master',
            'value': 'MaterialGroupMasterName'
        }
    ];
    $scope.materialGroupArticlePopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'MaterialGroupMasterName',
        searchBy: "MaterialGroupMasterName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.valueData = '';
    $scope.materialGroupArticlePopUpTitle = 'Material Group Article';

    $scope.showMaterialGroupArticlePopUp = function () {
        try {
            $scope.materialGroupArticleDatasList = [];
            $scope.GLUrl = 'Materials/MaterialGroupMaster/GetArticleList?mGroupId=' + $scope.materialGroupMasterTempId;
            //baseService.setCurrentPage('materialGroupArticleDatasList');
            $scope.getMaterialGroupArticlePopUpData = function (pageno) {
                baseService.paginationBase($scope.GLUrl, pageno, $scope.materialGroupArticlePopUpParameters)
                    .then(function (data) {
                        $scope.materialGroupArticleDatasList = data.Rows;
                        $scope.materialGroupArticlePopUpParameters.total_count = data.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#materialGroupMasterArticlemodal')).modal('show');
            $scope.getMaterialGroupArticlePopUpData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectDoubleClick = function (data) {
        //$scope.getMaterialGroupArticleProductionProcessPopUp();
        $scope.preCostingNew.MaterialGroupArticleName = data.StandardName;
        getMGAProductionProcess(data.Id);
        angular.element(document.querySelector('#materialGroupMasterArticlemodal')).modal('hide');
    };
    function calculateCriteria(list, id, output) {
        var rate = 0;
        angular.forEach(list, function (item) {
            if (item.MaterialGroupArticlePrdProcessGroupId === id) {
                rate = rate + (output + item.Wastage) * item.Rate;
            }
        });
        return rate;
    }
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
    // #endregion
    //ProductionProcessCriterria PopUp
    $scope.materialGroupProductionProcessCriteriaList = [];
    $scope.showMaterialGroupProductionCriteriaPop = function (data) {
        var url = 'Materials/MaterialGroupMaster/getProcessCriteriaList?id=' + data.Id;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            $scope.materialGroupProductionProcessCriteriaList = response.data;
        });
        angular.element(document.querySelector('#criteriaEntryPopUp')).modal('show');
    }
    $scope.closeEntryProcessPopUp = function () {
        angular.element(document.querySelector('#criteriaEntryPopUp')).modal('hide');
    }
    //
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = "Save";
        $scope.preCosting = {};
        $scope.preCostingNew = {};
        $scope.preCostingNew.Active = true;
    }
    // #region setTab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion
}