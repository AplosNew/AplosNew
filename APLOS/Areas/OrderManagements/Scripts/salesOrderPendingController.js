'use strict';
SalesOrderPendingController.$inject = ['$controller',"cboService", "$window", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function SalesOrderPendingController($controller,cboService, $window, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Pending Order";
    $scope.message_confirmation = "";
    $scope.path = 'OrderManagements/salesorderpending/';
    $scope.Index = -1;
    $scope.soPendingList = [];
    $scope.sbsoPendingList = [];
    var tempPendingList = [];
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $scope.master = {
        PlantId: $window.plantId,
        EntityId: null
    };
    $scope.getEntityDdl = function () {
        $scope.entityList = [];
        cboService.getCboProductionEntityByPlant(null, null, $scope.master.PlantId, function (result) {
            $scope.entityList = result;
        });
        $scope.pendingList = [];
        tempPendingList = [];
    }
    $scope.getEntityDdl();
    $scope.getSOPendingList = function () {
        $scope.soPendingList = [];
        baseService.init($scope.path + "getlist/", null, 20, null, 'MaterialMasterName', 'MaterialMasterName');
        $scope.loadSOPData = function (pageno) {
            $rootScope.parameters.entityid = $scope.master.EntityId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.soPendingList = result.Rows;
                    for (var t = 0; t < baseService.arrayLength($scope.soPendingList); t++) {
                        $scope.soPendingList[t].IsSelectedID = isInList($scope.pendingList, $scope.soPendingList[t].Id);
                    }
                    if (baseService.arrayLength($scope.sbsoPendingList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbsoPendingList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadSOPData();
        angular.element(document.querySelector('#soppopup')).modal('show');
    }
    function isInList(list, id) {
        for (var t = 0; t < baseService.arrayLength(list); t++) {
            if (list[t].Id === id) return true;
        }
        return false;
    }
    $scope.soClear = function () {
        $scope.pendingList = [];
    }
    ///**************************************************grid row selected event function*********************************

    $scope.pendingList = [];
    $scope.pushTempList = function (data, event) {
        if (event.currentTarget.checked)
            tempPendingList.push(data);
        else {
            for (var a = 0; a < baseService.arrayLength(tempPendingList); a++) {
                if (tempPendingList[a].Id === data.Id)
                    return tempPendingList.splice(a, 1);
            }
        }
    }
    $scope.selectPendingByButton = function () {
        if (baseService.arrayLength(tempPendingList)) {
            for (var t = 0; t < baseService.arrayLength(tempPendingList); t++) {
                if (!IsAvailablePL(tempPendingList[t], $scope.pendingList))
                    $scope.pendingList.push(tempPendingList[t]);
            }
        }
        else
            $scope.pendingList = [];
        for (var a = 0; a < baseService.arrayLength($scope.pendingList); a++) {
            if (!IsAvailablePL($scope.pendingList[a], tempPendingList))
                $scope.pendingList.splice(a, 1);
        }
        angular.element(document.querySelector('#soppopup')).modal('hide');
    };
    $scope.closePendingPopUp = function () {
        angular.element(document.querySelector('#soppopup')).modal('hide');
    };
    function IsAvailablePL(ob, list) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].Id === ob.Id) return true;
        }
        return false;
    }

    $scope.removeRowModal = function (ob, index) {
        try {
            $scope.Index = -1;
            $scope.message_confirmation = "Are you sure to delete [" + ob.MaterialGroupMasterName + "] ";
            angular.element(document.querySelector('#cpmm')).modal('show');
            $scope.Index = index;
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.removeRow = function () {
        for (var t = 0; t < baseService.arrayLength(tempPendingList); t++) {
            if (tempPendingList[t].Id === $scope.pendingList[$scope.Index].Id)
                tempPendingList.splice(t, 1);
        }
        $scope.pendingList.splice($scope.Index, 1);
        $scope.Index = -1;

        angular.element(document.querySelector('#cpmm')).modal('hide');
    };

    ///**************************************************Material Attach**************************************************

    $scope.Id = null;
    $scope.MaterialGroupId = null;
    $scope.Index = null;
    $scope.materialPopUpDataList = [];
    $scope.materialPopUpList = [];

        $scope.materialAttachPop = function (data, id, mgId, uom, index) {
        try {
            $scope.Id = id;
            $scope.MaterialGroupId = mgId;
            $scope.UoMId = uom;
            $scope.Index = index;
            $scope.materialPopUpDataList = [];
            $scope.materialPopUpList = [];
            $scope.sampleOrderMaterial = {
                Id: null
                , MaterialGroupMasterId: null
                , MaterialGroupMaster: null
                , MaterialMasterId: null
                , MaterialMaster: null
                , SubMaterialId: null
                , SubMaterial: null
                , MaterialGridId: null
                , OurStyle: null
                , Name: null
                , UoM: null
                , Currency: null
                , Characteristics1Id: null
                , Characteristics1: null
                , CharacteristicsValue1Id: null
                , CharacteristicsValue1: null
                , Characteristics2Id: null
                , Characteristics2: null
                , CharacteristicsValue2Id: null
                , Characteristics3Id: null
                , Characteristics3: null
                , CharacteristicsValue3Id: null
            };
             $scope.char1={}
             $scope.char2={}
             $scope.char3 = {}
            angular.element(document.querySelector('#materialAttachId')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.searchList = [];
    $scope.dataPlate = [];
    $scope.searchbyMaterialMasterDatalist = [
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
            'Text': 'Material Master',
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
    $scope.getMaterialMasterSearchData = function () {
        $scope.mmPopUpParameters = {
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
        $scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        $scope.popUpUrl = 'OrderManagements/sampleorderpending/getmateriallist';
        baseService.setCurrentPage('materialmasterSearchData');
        $scope.mmPopUpParameters.materialGroupId = $scope.MaterialGroupId;
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

    $scope.setMaterialMasterData = function (ob) {
        $scope.sampleOrderMaterial.MaterialMasterId = ob.Id;
        $scope.sampleOrderMaterial.MaterialMasterName = ob.UserName;
        $scope.sampleOrderMaterial.BaseUOMId = ob.BaseUOMId;
        $scope.sampleOrderMaterial.BaseUoM = ob.BaseUoM;
        $scope.sampleOrderMaterial.OurStyleName = ob.OurStyleName;
        $scope.sampleOrderMaterial.MaterialGroupMasterName = ob.MaterialGroupMasterName;
        $scope.sampleOrderMaterial.ProductMasterName = ob.ProductMasterName;
        $scope.sampleOrderMaterial.IsOurStyleRequired = ob.IsOurStyleRequired;
        $scope.sampleOrderMaterial.IsProductMstRequired = ob.IsProductMstRequired;
        $scope.sampleOrderMaterial.TransactionUoMId = ob.BaseUOMId;
        $scope.sampleOrderMaterial.ArticleId = null;
        $scope.sampleOrderMaterial.ArticleName = null;
        $scope.sampleOrderMaterial.FirstCharacteristicsValueId = null;
        $scope.sampleOrderMaterial.SecondCharacteristicsValueId = null;
        $scope.sampleOrderMaterial.ThirdCharacteristicsValueId = null;

        $scope.hasArticle = ob.HasAttribute;
        $scope.hasSku = ob.WithSKU;
        if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
        if (ob.WithSKU) $scope.getCharacteristicsList(ob.Id);
        angular.element(document.querySelector('#materialmastersearchpopup')).modal('hide');
    };
    $scope.selectarticle = function (ob) {
        try {
            $scope.sampleOrderMaterial.ArticleId = ob.Id;
            $scope.sampleOrderMaterial.ArticleName = ob.StandardName;
            manualValidation('div_ar', false);
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };
    $scope.setCharData = function (data) {
        $scope[$scope.charValueSearchFor].CharacteristicsValueId = data.CharacteristicsValueId;
        $scope[$scope.charValueSearchFor].FreeText = data.UserName;
        $scope[$scope.charValueSearchFor].FlagDisable = $scope.isSearch;
        angular.element(document.querySelector('#searchcharactervaluepopup')).modal('hide');
    };
    $scope.materialAttach = function () {
        if ($scope.hasSku) {
            if (!baseService.isUndefinedOrNull($scope.char1.CharacteristicsId))
                $scope.IsMandatoryButNull($scope.char1.IsMandatory, $scope.char1.FreeText);
            else if (!baseService.isUndefinedOrNull($scope.char2.CharacteristicsId))
                $scope.IsMandatoryButNull($scope.char2.IsMandatory, $scope.char2.FreeText);
            else if (!baseService.isUndefinedOrNull($scope.char3.CharacteristicsId))
                $scope.IsMandatoryButNull($scope.char3.IsMandatory, $scope.char3.FreeText);
            else ShowResult('Select SKU.', 'failure', 'materialAttachId');
        }
        if ($scope.hasArticle && baseService.isUndefinedOrNull($scope.sampleOrderMaterial.ArticleName)) {
            throw ShowResult('Select article.', 'failure', 'materialAttachId');
        }
        $scope.pendingList[$scope.Index].MaterialMasterId=$scope.sampleOrderMaterial.MaterialMasterId;
        $scope.pendingList[$scope.Index].MaterialMasterName=$scope.sampleOrderMaterial.MaterialMasterName;
        $scope.pendingList[$scope.Index].ArticleName = $scope.sampleOrderMaterial.ArticleName;
        $scope.pendingList[$scope.Index].ArticleId = $scope.sampleOrderMaterial.ArticleId;
        if ($scope.hasSku) {
            $scope.pendingList[$scope.Index].Characteristics1Name = $scope.char1.Name;
            $scope.pendingList[$scope.Index].CharacteristicsValue1 = $scope.char1.FreeText;
            $scope.pendingList[$scope.Index].Characteristics1Id = $scope.char1.CharacteristicsId;
            $scope.pendingList[$scope.Index].CharacteristicsValue1Id = $scope.char1.CharacteristicsValueId;
            $scope.pendingList[$scope.Index].Characteristics2Name = $scope.char2.Name;
            $scope.pendingList[$scope.Index].CharacteristicsValue2 = $scope.char2.FreeText;
            $scope.pendingList[$scope.Index].Characteristics2Id = $scope.char2.CharacteristicsId;
            $scope.pendingList[$scope.Index].CharacteristicsValue2Id = $scope.char2.CharacteristicsValueId;
            $scope.pendingList[$scope.Index].Characteristics3Name = $scope.char3.Name;
            $scope.pendingList[$scope.Index].CharacteristicsValue3 = $scope.char3.FreeText;
            $scope.pendingList[$scope.Index].Characteristics3Id = $scope.char3.CharacteristicsId;
            $scope.pendingList[$scope.Index].CharacteristicsValue3Id = $scope.char3.CharacteristicsValueId;
        }
        angular.element(document.querySelector('#materialAttachId')).modal('hide');

    };
    function getCVList(mmId, mGridId, chId, list) {
        $scope[list] = [];
        $http.get('OrderManagements/sampleorderpending/GetCharacteristicsValueCbo?mmId=' + mmId + '&mGridId=' + mGridId + '&chId=' + chId)
            .then(function (response) {
                $scope[list] = response.data;
            });
    }
    $scope.CloseMaterialAttach = function () {
        $scope.Id = null;
        $scope.MaterialGroupId = null;
        $scope.UoMId = null;
        $scope.Index = null;
        ClearMaterial();
        CloseModalShowResult('materialAttachId');
        angular.element(document.querySelector('#materialAttachId')).modal('hide');
    }
    function ClearMaterial() {
        $scope.CV2List = [];
        $scope.CV3List = [];
        $scope.sampleOrderMaterial = {};
    }
    $scope.closeMaterialPopUp = function () {
        $scope.materialPopUpDataList = [];
        $scope.materialPopUpList = [];
        $scope.valueData = '';
        CloseModalShowResult('materialPopUpId');
        angular.element(document.querySelector('#materialPopUpId')).modal('hide');
    }
    ///************************************************End Material Attach************************************************
    ///**************************************************Material Detached************************************************
    $scope.materialDetached = function (index) {
        $scope.pendingList[index].MaterialMasterId = null;
        $scope.pendingList[index].MaterialMasterName = null;
        $scope.pendingList[index].ArticleName = null;
        $scope.pendingList[index].ArticleId = null;
        $scope.pendingList[index].Characteristics1Name = null;
        $scope.pendingList[index].CharacteristicsValue1 = null;
        $scope.pendingList[index].Characteristics1Id = null;
        $scope.pendingList[index].CharacteristicsValue1Id = null;
        $scope.pendingList[index].Characteristics2Name = null;
        $scope.pendingList[index].CharacteristicsValue2 = null;
        $scope.pendingList[index].Characteristics2Id = null;
        $scope.pendingList[index].CharacteristicsValue2Id = null;
        $scope.pendingList[index].Characteristics3Name = null;
        $scope.pendingList[index].CharacteristicsValue3 = null;
        $scope.pendingList[index].Characteristics3Id = null;
        $scope.pendingList[index].CharacteristicsValue3Id = null;
    }
    ///************************************************End Material Detached**********************************************
    function validattion() {
        angular.forEach($scope.pendingList, function (item) {
            if (baseService.isUndefinedOrNull(item.MaterialMasterId)) {
                throw "Material master required.";
            }
        });
    }
    $scope.saveChange = function () {
        try {
            validattion();
            $http({
                method: 'POST',
                url: $scope.path + 'create',
                data: $scope.pendingList,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.soClear();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
};