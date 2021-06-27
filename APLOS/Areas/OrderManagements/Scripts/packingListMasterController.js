'use strict';
packingListMasterController.$inject = ['factoryService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window', 'cboService', '$controller'];
function packingListMasterController(factoryService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $window, cboService, $controller) {
    $rootScope.title = "Packing List";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.modelList = [];
    $scope.path = 'OrderManagements/packingListMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.partyType = 'Customer';
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.searchMasterFilterList = [
        {
            'value': 'PartyCode'
            , 'name': 'Party Code'
        },
        {
            'value': 'PartyName'
            , 'name': 'Party'
        },
        {
            'value': 'InvoicingPartyPlant'
            , 'name': 'Bill TO'
        },
        {
            'value': 'DeliveryPartyPlant'
            , 'name': 'Ship TO'
        }
    ];

    $scope.getData = function () {
        baseService.init($scope.getListUrl, null, null, null, 'PartyName', 'PartyName');
        $rootScope.parameters.entityId = $scope.modelNew.EntityId;
        $scope.getModelList = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.modelList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getModelList();

        var filterEntityList = $filter('filter')($scope.entityList, { Value: $scope.modelNew.EntityId }, true);
        $scope.IsDispatchGrpApplicable = filterEntityList[0].IsDispatchGrpApplicable;
        $scope.DispatchUoM = filterEntityList[0].DispatchUoM;
        $scope.PackingUoM = filterEntityList[0].PackingUoM;

    };

    $scope.model = {
        Id: null
        , PlantId: $window.plantId
        , EntityId: null
        , PartyId: null
        , PartyCode: null
        , PartyName: null
        , InvoicingPartyPlantId: null
        , DeliveryPartyPlantId: null
        , InvoicingByAddress: null
        , DeliveryByAddress: null
        , TotalQty: null
        , TotalQtyUOMId: null
        , TotalQtyBaseUoMId: null
        , Remarks: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    // #region Ddl

    $scope.entityList = [];
    $http.get($scope.path + 'GetEntityCboByPlant?plantId=' + $window.plantId)
        .then(function (response) {
            $scope.entityList = response.data;
        });

    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });

    // #endregion Ddl

    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
    }).then(function successCallback(response) {
        $scope.baseCurrencyId = response.data[0].Value;
        factoryService.getCurrencyPrecision($scope.baseCurrencyId);
    });

    $scope.Get = function (index) {
        $scope.index = index;
        angular.copy($scope.modelList[$scope.index], $scope.model);
        angular.copy($scope.model, $scope.modelNew);
        getPartyPlantList();
        getDispatchMasterArticleList();
        getDispatchAllSKUList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.modelForm.$valid) {
            angular.copy($scope.modelNew, $scope.model);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: { 'entity': $scope.model }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.modelNew.Id = response.data.Id;
                        $scope.Action = 'Update';
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                for (var i = 0; i < baseService.arrayLength($scope.itemList); i++) {
                    if (baseService.isUndefinedOrNull($scope.itemList[i].MaterialMasterId))
                        return ShowResult('Material master need in row number ' + (i + 1), 'failure');
                }
                $http({
                    method: 'POST'
                    , url: $scope.updateUrl
                    , data: { 'entity': $scope.model }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl + $scope.modelNew.Id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.modelList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        $scope.ClearFields();
        $scope.modelNew.EntityId = null;
    };
    $scope.ClearFields = function () {
        $scope.Action = "Save";
        $scope.model = {};
        $scope.modelNew = { PlantId: $scope.modelNew.PlantId, EntityId: $scope.modelNew.EntityId };
        $scope.partyPlantList = [];
    };

    //#region Party

    $scope.partySearchByList = [
        {
            'name': $scope.partyType + ' Code',
            'value': 'Code'
        },
        {
            'name': $scope.partyType + ' Name',
            'value': 'PartyName'
        },
        {
            'name': 'Account Group',
            'value': 'PartyAccountGroupName'
        },
        {
            'name': 'Country',
            'value': 'CountryName'
        },
        {
            'name': 'State',
            'value': 'StateName'
        },
        {
            'name': 'Currency',
            'value': 'CurrencyCode'
        }
    ];

    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'PartyName, PartyAccountGroupName'
        , searchBy: 'PartyName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };

    $scope.showPartyPopUp = function () {
        baseService.setCurrentPage('partyList');
        $scope.getPartyList = function (pageno) {
            $scope.partyUrl = $scope.path + 'GetCompanyPartyDataList?plantId=' + $window.plantId + '&entityId=' + $scope.modelNew.EntityId;
            baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
                .then(function (result) {
                    $scope.partyList = result.Rows;
                    $scope.partyParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#partyPopUp')).modal('show');
        $scope.getPartyList();
    };

    $scope.selectPartyPopUpRow = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedParty = id;
    };

    $scope.selectCustomerPopUp = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedCustomer = id;
    };

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.modelNew.PartyCode = party.Code;
            $scope.modelNew.PartyName = party.UserName;
            $scope.modelNew.PartyId = party.Id;
            $scope.modelNew.CurrencyId = party.CurrencyId;
            $scope.modelNew.PartyAccountGroupId = party.PartyAccountGroupId;
            $scope.modelNew.InvoicingPartyPlantId = party.InvoicingPartyPlantId;
            $scope.modelNew.InvoicingByAddress = party.InvoicingByAddress;
            $scope.modelNew.DeliveryPartyPlantId = party.DeliveryPartyPlantId;
            $scope.modelNew.DeliveryByAddress = party.DeliveryByAddress;

            $scope.modelNew.TotalQty = party.TotalQty;
            $scope.modelNew.TotalQtyUOMId = party.TotalQtyUOMId;
        }
        getPartyPlantList();
        $scope.hidePartyPopUp();
    };


    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
            if (flag === 'billTo') {
                $scope.modelNew.InvoicingState = state;
                $scope.modelNew.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.modelNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.modelNew.DeliveryState = state;
                $scope.modelNew.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.modelNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.modelNew.InvoicingState = null;
                $scope.modelNew.InvoicingGSTIN = null;
                return $scope.modelNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.modelNew.DeliveryState = null;
                $scope.modelNew.DeliveryGSTIN = null;
                return $scope.modelNew.DeliveryByAddress = null;
            }
        }
    };

    function getPartyPlantList() {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.modelNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
            });
        });
    }

    //#endregion Party

    //#region Dispatch

    $scope.dispatchPopUp = function () {
        $scope.qtyName = angular.element("#totalQtyId :selected").text();
        $scope.dispatchArticleList = [];

        $scope.detailModel = {
            Id: null
            , PackingListMasterId: $scope.modelNew.Id
            , SalesOrderId: null
            , SalesOrderNo: null
            //, DispatchMasterCodeLable: $scope.IsDispatchGrpApplicable ? $scope.DispatchUoM : $scope.PackingUoM
            , DispatchUnitCode: null
            , Qty: null
            , QtyUOMId: $scope.modelNew.TotalQtyUOMId
            , QtyBaseUoMId: null
            , NetWeight: null
            , GrossWeight: null
            , Remarks: null
            , MaterialMasterId: null
            , MaterialMasterName: null
            , ArticleId: null
            , ArticleName: null
        };
        angular.element(document.querySelector('#dispatchMasterPoUp')).modal('show');
    };

    $scope.saveDispatch = function () {
        $http({
            method: 'POST'
            , url: $scope.path + 'CreateDispatch'
            , data: {
                'dispatch': $scope.detailModel
                , 'articleList': $scope.dispatchArticleList
            }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'dispatchMasterPoUp');
            }
            else {
                ShowResult(response.data.Message, 'success', 'dispatchMasterPoUp');
                $scope.detailModel.Id = response.data.dispatchUnitId;
                $scope.dispatchArticleList = [];
                $scope.dispatchArticleList = response.data.articleList;
                getDispatchMasterArticleList();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'dispatchMasterPoUp');
        };
    };
    $scope.closeDispatchPopUp = function () {
        $scope.detailModel = {};
        $scope.dispatchArticleList = [];
        angular.element(document.querySelector('#dispatchMasterPoUp')).modal('hide');
    };

    $scope.addDispatchArticle = function () {
        $scope.dispatchArticleList.unshift({
            Id: null
            , PackingListMasterId: $scope.modelNew.Id
            , DispatchUnitMasterId: $scope.detailModel.Id
            , SalesOrderId: null
            , MaterialMasterId: null
            , ArticleId: null
            , Qty: null
            , QtyUOMId: $scope.modelNew.TotalQtyUOMId
            , QtyBaseUoMId: null
        });
    };

    $scope.deleteDispatchArticle = function () {
        if (!baseService.isUndefinedOrNull($scope.id)) {
            $http({
                method: 'POST'
                , url: $scope.path + 'DeleteDispatchArticleGraph?id=' + $scope.id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    if ($scope.isPopUp === 'popUp') ShowResult(response.data.Message, 'failure', 'dispatchMasterPoUp');
                    else if ($scope.isPopUp === null) ShowResult(response.data.Message, 'failure');
                }
                else {
                    if ($scope.isPopUp === 'popUp') {
                        ShowResult(response.data.Message, 'success', 'dispatchMasterPoUp');
                        $scope.dispatchArticleList.splice($scope.dispatchIndex, 1);
                    }
                    else if ($scope.isPopUp === null) {
                        ShowResult(response.data.Message, 'success');
                        $scope.dispatchMasterArticleList.splice($scope.dispatchIndex, 1);
                    }
                }
                $scope.id = null;
                $scope.dispatchIndex = -1;
                $scope.isPopUp = null;
                function errorCallBack(response) {
                    if ($scope.isPopUp === 'popUp') ShowResult(response.data.Message, 'failure', 'dispatchMasterPoUp');
                    else if ($scope.isPopUp === null) ShowResult(response.data.Message, 'failure');
                }
            });
            getDispatchMasterArticleList();
        }
        else {
            if ($scope.isPopUp === 'popUp') {
                $scope.dispatchArticleList.splice($scope.dispatchIndex, 1);
            }
            $scope.id = null;
            $scope.dispatchIndex = -1;
            $scope.isPopUp = null;
        }
    };

    function getDispatchMasterArticleList() {
        $scope.dispatchSKUList = [];
        $http.get($scope.path + 'GetDispatchMasterArticleList?packingId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.dispatchMasterArticleList = response.data;
            });
    }

    //#endregion Dispatch

    //#region Dispatch SKU

    $scope.skuPopUp = function (id, salesOrderId) {
        $scope.dispatchArticleId = id;
        $scope.salesOrderId = salesOrderId;
        $scope.dispatchSKUPopUpList = [];
        $http.get($scope.path + 'GetDispatchSKUListByArticle?dispatchArticleId=' + $scope.dispatchArticleId)
            .then(function (response) {
                $scope.dispatchSKUPopUpList = response.data;
            });
        angular.element(document.querySelector('#dispatchSKUPopUp')).modal('show');
    };

    $scope.salesOrderSkuPopUp = function () {
        $scope.salesOrderSkuList = [];
        $http.get($scope.path + 'GetSalesOrderSKUList?salesOrderId=' + $scope.salesOrderId)
            .then(function (response) {
                $scope.salesOrderSkuList = response.data;
            });
        angular.element(document.querySelector('#salesOrderSkuPopUp')).modal('show');
    };

    $scope.selectSku = function (data) {
        $scope.dispatchSKUPopUpList.unshift({
            Id: null
            , DispatchUnitArticleId: $scope.dispatchArticleId
            , SalesOrderFirstCharacteristicsId: data.FirstCharacteristicsId
            , FirstCharacteristicsId: data.CHId1
            , CH1Name: data.CH1Name
            , CH1Value: data.CH1Value

            , SalesOrderSecondCharacteristicsId: data.SecondCharacteristicsId
            , SecondCharacteristicsId: data.CHId2
            , CH2Name: data.CH2Name
            , CH2Value: data.CH2Value

            , SalesOrderThirdCharacteristicsId: data.ThirdCharacteristicsId
            , ThirdCharacteristicsId: data.CHId3
            , CH3Name: data.CH3Name
            , CH3Value: data.CH3Value

            , NoOfPackingUnit: null
            , QtyPerPackingUnit: null
            , Qty: data.Qty
            , QtyUOMId: $scope.modelNew.TotalQtyUOMId
            , QtyBaseUoMId: null
            , disabledQty: true
        });

    };

    $scope.addSku = function () {
        $scope.dispatchSKUPopUpList.unshift({
            Id: null
            , DispatchUnitArticleId: $scope.dispatchArticleId
            , SalesOrderFirstCharacteristicsId: null
            , FirstCharacteristicsId: null
            , CH1Name: null
            , CH1Value: null

            , SalesOrderSecondCharacteristicsId: null
            , SecondCharacteristicsId: null
            , CH2Name: null
            , CH2Value: null

            , SalesOrderThirdCharacteristicsId: null
            , ThirdCharacteristicsId: null
            , CH3Name: null
            , CH3Value: null

            , NoOfPackingUnit: null
            , QtyPerPackingUnit: null
            , Qty: null
            , QtyUOMId: $scope.modelNew.TotalQtyUOMId
            , QtyBaseUoMId: null
            , disabledQty: false
        });

    };

    function getDispatchAllSKUList() {
        $scope.dispatchSKUList = [];
        $http.get($scope.path + 'GetDispatchAllSKUList?packingId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.dispatchSKUList = response.data;
            });
    }

    $scope.GetDispatchAllData = function (id) {
        $http.get($scope.path + 'GetDispatchAllData?dispatchUnitMasterId=' + id)
            .then(function (response) {
                $scope.detailModel = response.data.dispatch[0];
                $scope.dispatchArticleList = response.data.dispatchArticleList;
                angular.element(document.querySelector('#dispatchMasterPoUp')).modal('show');
            });
    };

    $scope.saveDispatchSku = function () {
        $http({
            method: 'POST'
            , url: $scope.path + 'CreateDispatchSku'
            , data: { 'skuList': $scope.dispatchSKUPopUpList }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'dispatchSKUPopUp');
            }
            else {
                ShowResult(response.data.Message, 'success', 'dispatchSKUPopUp');
                $scope.dispatchArticleId = null;
                $scope.salesOrderId = null;
                $scope.dispatchSKUPopUpList = [];

                angular.element(document.querySelector('#dispatchSKUPopUp')).modal('hide');
                getDispatchAllSKUList();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'dispatchSKUPopUp');
        };
    };

    $scope.deleteDispatchSku = function () {
        if (!baseService.isUndefinedOrNull($scope.id)) {
            $http({
                method: 'POST'
                , url: $scope.path + 'DeleteDispatchSkuGraph?id=' + $scope.id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    if ($scope.isPopUp === 'popUp') ShowResult(response.data.Message, 'failure', 'dispatchSKUPopUp');
                    else if ($scope.isPopUp === null) ShowResult(response.data.Message, 'failure');
                }
                else {
                    if ($scope.isPopUp === 'popUp') {
                        ShowResult(response.data.Message, 'success', 'dispatchSKUPopUp');
                        $scope.dispatchSKUPopUpList.splice($scope.dispatchIndex, 1);
                    }
                    else if ($scope.isPopUp === null) {
                        ShowResult(response.data.Message, 'success');
                        $scope.dispatchSKUList.splice($scope.dispatchIndex, 1);
                    }
                }
                $scope.id = null;
                $scope.dispatchIndex = -1;
                $scope.isPopUp = null;
                function errorCallBack(response) {
                    if ($scope.isPopUp === 'popUp') ShowResult(response.data.Message, 'failure', 'dispatchSKUPopUp');
                    else if ($scope.isPopUp === null) ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            if ($scope.isPopUp === 'popUp') {
                $scope.dispatchSKUPopUpList.splice($scope.dispatchIndex, 1);
            }
            else if ($scope.isPopUp === null) {
                $scope.dispatchSKUList.splice($scope.dispatchIndex, 1);
            }
            $scope.id = null;
            $scope.dispatchIndex = -1;
            $scope.isPopUp = null;
        }
    };

    //#endregion Dispatch SKU

    //#region Sales Order

    $scope.salesOrderFilterList = [
        {
            'name': 'Sales Order No',
            'value': 'SalesOrderNo'
        },
        {
            'name': 'Delivery Date',
            'value': 'DeliveryDate'
        },
        {
            'name': 'PO Number',
            'value': 'PONumber'
        },
        {
            'name': 'Material Master',
            'value': 'MaterialMasterName'
        },
        {
            'name': 'Article',
            'value': 'ArticleName'
        }
    ];

    $scope.salesOrderParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'MaterialMasterName, ArticleName'
        , searchBy: 'SalesOrderNo'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.salesOrderPopUp = function (index) {
        $scope.salesOrderIndex = index;
        $scope.salesOrderList = [];
        baseService.setCurrentPage('salesOrderList');
        $scope.getSalesOrderList = function (pageno) {
            baseService.paginationBase($scope.path + 'GetSalesOrderList', pageno, $scope.salesOrderParameters)
                .then(function (result) {
                    $scope.salesOrderList = result.Rows;
                    $scope.salesOrderParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#salesOrderPoUp')).modal('show');
        $scope.getSalesOrderList();
    };

    $scope.selectSalesOrder = function (data) {
        for (var i = 0; i < baseService.arrayLength($scope.dispatchArticleList); i++) {
            if (baseService.isAvailableInList($scope.dispatchArticleList[i].SalesOrderId, data.Id, i, $scope.salesOrderIndex))
                return ShowResult('Already taken, sales order :' + data.Id, 'failure', 'salesOrderPoUp');
        }

        $scope.dispatchArticleList[$scope.salesOrderIndex].SalesOrderId = data.Id;
        $scope.dispatchArticleList[$scope.salesOrderIndex].SalesOrderNo = data.SalesOrderNo;
        $scope.dispatchArticleList[$scope.salesOrderIndex].MaterialMasterId = data.MaterialMasterId;
        $scope.dispatchArticleList[$scope.salesOrderIndex].MaterialMasterName = data.MaterialMasterName;
        $scope.dispatchArticleList[$scope.salesOrderIndex].ArticleId = data.ArticleId;
        $scope.dispatchArticleList[$scope.salesOrderIndex].ArticleName = data.ArticleName;
        $scope.dispatchArticleList[$scope.salesOrderIndex].Qty = data.Qty;
        $scope.salesOrderPopUpClose();
    };

    $scope.salesOrderPopUpClose = function () {
        angular.element(document.querySelector('#salesOrderPoUp')).modal('hide');
    };

    //#endregion Sales Order

    $scope.genericDelete = function (id, index, flag, isPopUp) {
        $scope.isPopUp = isPopUp;
        $scope.id = id;
        $scope.dispatchIndex = index;
        $scope.message_confirmation = "Are you sure want to permanently delete ";
        angular.element(document.querySelector('#genericConfirm')).modal('show');
        $scope.flag = flag;
    };

    $scope.genericRemove = function () {
        if ($scope.flag === 'sku')
            $scope.deleteDispatchSku();
        else if ($scope.flag === 'article')
            $scope.deleteDispatchArticle();
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}

