'use strict';
SampleOrderPendingController.$inject = ['$controller', '$window', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', 'cboService'];
function SampleOrderPendingController($controller, $window, commonMessage, $scope, $rootScope, baseService, $http, cboService) {
    $rootScope.title = "Sample Order Pending";
    $scope.path = 'OrderManagements/sampleorderpending/';
    $scope.confirmUrl = $scope.path + '';
    $scope.attachUrl = $scope.path + '';
    $scope.deleteUrl = $scope.path + '';
    $scope.getSampleOrderListUrl = $scope.path + 'getpendingsampleorderlist';
    $scope.pendingList = [];
    var tempPendingList = [];
    $scope.sampleOrderList = [];

    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $scope.sampleOrderPending = {
        Id: null
        , PlantId: null
        , EntityId: null
    };
    $scope.sampleOrderPendingNew = Object.assign({}, $scope.sampleOrderPending);

    $scope.entityList = [];
    cboService.getCboProductionEntityByPlant(null, null, $window.plantId, function (result) {
        $scope.entityList = result;
    });


    // #region Sample order
    $rootScope.searchSampleOrderList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Material Group (Mst)',
            'value': 'MaterialGroupMasterName'
        },
        {
            'name': 'Material Master',
            'value': 'MaterialMasterName'
        },
        {
            'name': 'Article',
            'value': 'ArticleName'
        },
        {
            'name': 'Unit of measurement',
            'value': 'UoM'
        },
        {
            'name': 'Currency',
            'value': 'CurrencyName'
        },
        {
            'name': 'Delivery date',
            'value': 'DeliveryDate'
        },
        {
            'name': 'ReferenceDocNo',
            'value': 'ReferenceDocNo'
        },
        {
            'name': 'PartyName',
            'value': 'PartyName'
        }
    ];
    $scope.sampleOrderPopUp = function () {
        $scope.sampleOrderParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'Code',
            searchBy: "MaterialGroupMasterName",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        baseService.setCurrentPage('sampleOrderList');
        $scope.getSampleOrderData = function (pageno) {
            $scope.sampleOrderParameters.entityId = $scope.sampleOrderPendingNew.EntityId;
            baseService.paginationBase($scope.getSampleOrderListUrl, pageno, $scope.sampleOrderParameters)
                .then(function (result) {
                    $scope.sampleOrderList = result.Rows;
                    $scope.sampleOrderParameters.total_count = result.Total;
                    for (var i = 0; i < baseService.arrayLength($scope.sampleOrderList); i++) {
                        $scope.sampleOrderList[i].Flag = tempPendingList.includes($scope.sampleOrderList[i].Id)
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'sampleOrderId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#sampleOrderId')).modal('show');
        $scope.getSampleOrderData();
    }

    $scope.selectSampleOrder = function () {
        if (baseService.arrayLength(tempPendingList) === 0)
            return ShowResult('Please first select at least one row', 'failure', 'sampleOrderId');
        $http.get('OrderManagements/sampleorderpending/getpendinglist?ids=' + JSON.stringify(tempPendingList))
            .then(function (response) {
                $scope.pendingList = response.data;
                $scope.closeSampleOrder();
            });
    }
    $scope.closeSampleOrder = function () {
        refreshTempList($scope.pendingList, tempPendingList)
        angular.element(document.querySelector('#sampleOrderId')).modal('hide');
        $scope.sampleOrderList = [];
    }
    $scope.pushTempList = function (id, event) {
        if (event.currentTarget.checked)
            tempPendingList.push(id);
        else
            tempPendingList.splice(tempPendingList.indexOf(id), 1);
    }
    function refreshTempList(list, list2) {
        list2 = [];
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (!list2.includes(list[i].Id))
                list2.push(list[i].Id);
        }
    }

    // #endregion Sample order

    $scope.confirmation = function (id, event) {
        try {
            $http({
                method: "POST",
                url: $scope.path + 'Confirmation',
                data: {
                    'id': id,
                    'flag': event.currentTarget.checked,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Id = null;
    $scope.MaterialGroupId = null;
    $scope.Index = null;

    $scope.materialAttach = function (id, mgId, uom, index) {
        try {
            $scope.sampleOrderMaterial = {
                Id: id
                , MaterialGroupMasterId: mgId
                , MaterialGroupMaster: null
                , MaterialMasterId: null
                , MaterialMaster: null
                , ArticleId: null
                , Article: null
                , MaterialGridId: null
                , OurStyle: null
                , Name: null
                , UoM: null
                , Currency: null
                , FirstCharacteristicsId: null
                , FirstCharacteristicsValueId: null

                , SecondCharacteristicsId: null
                , SecondCharacteristicsValueId: null

                , ThirdCharacteristicsId: null
                , ThirdCharacteristicsValueId: null
            };
            $scope.clearCharNames();
            $scope.Id = id;
            $scope.MaterialGroupId = mgId;
            $scope.UoMId = uom;
            $scope.Index = index;
            angular.element(document.querySelector('#materialAttachId')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.MaterialAttachSave = function () {
        try {
            if ($scope.hasSku) {
                if (!baseService.isUndefinedOrNull($scope.char1.CharacteristicsId))
                    $scope.IsMandatoryButNull($scope.char1.IsMandatory, $scope.char1.FreeText);
                else if (!baseService.isUndefinedOrNull($scope.char2.CharacteristicsId))
                    $scope.IsMandatoryButNull($scope.char2.IsMandatory, $scope.char2.FreeText);
                else if (!baseService.isUndefinedOrNull($scope.char3.CharacteristicsId))
                    $scope.IsMandatoryButNull($scope.char3.IsMandatory, $scope.char3.FreeText);
                else throw 'Please insert SKU.';
            }
            $scope.sampleOrderMaterial.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
            $scope.sampleOrderMaterial.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
            $scope.sampleOrderMaterial.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
            $scope.sampleOrderMaterial.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;
            $scope.sampleOrderMaterial.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
            $scope.sampleOrderMaterial.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;
            $http({
                method: "POST",
                url: $scope.path + 'MaterialAttach',
                data: $scope.sampleOrderMaterial,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'materialAttachId');
                else {
                    $scope.selectSampleOrder();
                    $scope.CloseMaterialAttach();
                    ShowResult(response.data.Message, 'success');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'materialAttachId');
            };
        } catch (e) {
            ShowResult(e, 'failure', 'materialAttachId');
        }
    }

    $scope.materialDetachedPopUp = function (id, name, mmName, index) {
        if (baseService.isUndefinedOrNull(mmName))
            return ShowResult('This order has no material', 'failure');
        $scope.id = id;
        $rootScope.rowIndex = index;
        $rootScope.confirmationMessage = 'Are you sure want to material detached from this [ ' + name + ' ]';
        angular.element(document.querySelector('#detachedPopUp')).modal('show');
    };
    $scope.materialDetached = function () {
        $http({
            method: "POST",
            url: $scope.path + 'MaterialDetached?id=' + $scope.id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                $scope.pendingList[$rootScope.rowIndex].MaterialMasterId = null;
                $scope.pendingList[$rootScope.rowIndex].MaterialMasterName = null;
                $scope.pendingList[$rootScope.rowIndex].ArticleId = null;
                $scope.pendingList[$rootScope.rowIndex].ArticleName = null;
                $scope.pendingList[$rootScope.rowIndex].Detail = null;
                $scope.id = null;
                $rootScope.rowIndex = -1;
                $rootScope.confirmationMessage = '';
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.errorText = '';
    $scope.dispatchDatePopUp = function (id, name, index) {
        $scope.date = null;
        $scope.id = id;
        $rootScope.rowIndex = index;
        $rootScope.confirmationMessage = 'Are you sure want to set dispatch date for [ ' + name + ' ]';
        angular.element(document.querySelector('#dispatchDatePopUp')).modal('show');
    };
    $scope.dispatchDate = function () {
        if (baseService.isUndefinedOrNull($scope.date))
            return $scope.errorText = 'Please select date';
        $http({
            method: "POST",
            url: $scope.path + 'DispatchDate',
            data: {
                'id': $scope.id,
                'date': $scope.date,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                $scope.pendingList[$rootScope.rowIndex].DeliveryDate = $scope.date;
                $scope.id = null;
                $rootScope.rowIndex = -1;
                $rootScope.confirmationMessage = '';
                $scope.errorText = '';
                angular.element(document.querySelector('#dispatchDatePopUp')).modal('hide');
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    function IsCharacteristicsValue(chId, chValue, chName) {
        if (!baseService.isUndefinedOrNull(chId)) {
            if (baseService.isUndefinedOrNull(chValue))
                throw 'Value of ' + chName + ' can not be null!';
        }
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

    //#region Material Master

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
        $scope.popUpUrl = $scope.path + 'GetMaterialList';
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
        $scope.clearCharNames();
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

    //#endregion Material Master

    $scope.ClearSampleOrder = function () {
        $scope.sampleOrderList = [];
        $scope.pendingList = [];
        tempPendingList = [];
        $scope.sampleOrderPendingNew = {};
        $scope.sampleOrderPending = {};
    }
}