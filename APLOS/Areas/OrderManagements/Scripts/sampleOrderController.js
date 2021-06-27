'use strict';
SampleOrderController.$inject = ['$window', '$controller', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService'];
function SampleOrderController($window, $controller, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $rootScope.title = "Sample Order";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.sampleOrderList = [];
    $scope.showTbl = false;
    $scope.path = 'OrderManagements/sampleOrder/';
    $scope.saveUrl = $scope.path + '/create';
    $scope.updateUrl = $scope.path + '/edit';
    $scope.deleteUrl = $scope.path + '/delete?id=';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.partyType = 'Customer';
    $scope.buyerApplicable = false;
    $scope.soMasterList = [];
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.getDataList = function () {
        baseService.init($scope.getListUrl, null, null, null, 'Plant, Entity, PartyName', 'PartyName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.plantId = $window.plantId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.soMasterList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.pk = function () {
        return 'n-' + Math.floor(Math.random() * 900000) + 100000;
    };
    $scope.sampleOrder = {
        Id: null
        , PlantId: $window.plantId
        , EntityId: null
        , SalesOrganisationId: null
        , SalesGroupId: null
        , BuyerId: null
        , CustomerId: null
        , PartyCode: null
        , PartyName: null

        , InvoicingPartyPlantId: null
        , DeliveryPartyPlantId: null
        , InvoicingByAddress: null
        , DeliveryByAddress: null
        , InvoicingState: null
        , DeliveryState: null

        , CurrencyId: null
        , PaymentTermId: null
        , RequestReferenceDate: $filter('dateFiltering')(Date.now())
        , ReferenceDocNo: null
        , DeliveryDate: null
        , IsChangeable: true
    };
    $scope.sampleOrderNew = Object.assign({}, $scope.sampleOrder);
    $scope.Get = function (index) {
        $scope.tab = 1;
        $scope.Action = 'Update';
        $scope.index = index;
        $scope.sampleOrder = $scope.soMasterList[$scope.index];
        angular.copy($scope.sampleOrder, $scope.sampleOrderNew);
        $scope.getEntityAndSalesOrg(true);
        $scope.GetSubMaterial();
        getPartyPlantList(true);
        //salesOrganizationCbo(true);
        //$scope.LoadSalesGroup(true);
        //$scope.getUoMCboByMaterialGroup();
        //$scope.GetSampleOrderPartnerFunction();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.searchsampleOrderList = [
        {
            'name': 'Plant',
            'value': 'Plant'
        },
        {
            'name': 'Entity',
            'value': 'Entity'
        },
        {
            'name': 'Sales Org.',
            'value': 'SalesOrganisation'
        },
        {
            'name': 'Sales Group',
            'value': 'SalesGroup'
        },
        {
            'name': 'Buyer',
            'value': 'Buyer'
        },
        {
            'name': 'Customer',
            'value': 'PartyName'
        },
        {
            'name': 'Currency',
            'value': 'Currency'
        },
        {
            'name': 'Ref. DocNo',
            'value': 'ReferenceDocNo'
        }
    ];

    $scope.entityList = [];
    $scope.getEntityAndSalesOrg = function (isEdit) {
        if (!isEdit)
            $scope.sampleOrderNew.EntityId = null;
        cboService.getCboProductionEntityByPlant(null, null, $window.plantId, function (result) {
            $scope.entityList = result;
        });
        salesOrganizationCbo(isEdit);
        $http({
            method: 'GET',
            url: 'Setups/plantconfig/GetPlantConfigDataByPlantId?plantid=' + $scope.sampleOrderNew.PlantId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0)
                $scope.buyerApplicable = response.data[0].BuyerApplicable;
            else $scope.buyerApplicable = false;
        });
    };
    $scope.getEntityAndSalesOrg(false);
    //Sales Org DDL
    $scope.clearChangeOnSalesOrg = function () {
        $scope.sampleOrderNew.CustomerId = null;
        $scope.sampleOrderNew.PartyName = null;
        $scope.sampleOrderNew.PaymentTermId = null;
        $scope.partnerFunctionList = [];
    };
    function salesOrganizationCbo(isEdit) {
        $http({
            method: 'GET',
            url: 'Organizations/salesorganisation/getcbobyplant?plantId=' + $scope.sampleOrderNew.PlantId
        }).then(function successCallback(response) {
            $scope.salesOrganizationList = response.data;
            if (!isEdit) {
                if (baseService.arrayLength($scope.salesOrganizationList) === 1)
                    $scope.sampleOrderNew.SalesOrganisationId = $scope.salesOrganizationList[0].Value;
            }
            $scope.LoadSalesGroup(isEdit);
        });
    }
    //Sales Group DDL
    $scope.LoadSalesGroup = function (isEdit) {
        if (baseService.isUndefinedOrNull($scope.sampleOrderNew.SalesOrganisationId)) {
            return $scope.sampleOrderNew.SalesGroupId = null;
        }
        $http({
            method: 'GET',
            url: 'Organizations/salesgroup/getcbo?salesorganisationid=' + $scope.sampleOrderNew.SalesOrganisationId
        }).then(function successCallback(response) {
            $scope.salesgroupList = response.data;
            if (!isEdit) {
                if (baseService.arrayLength($scope.salesgroupList) === 1) {
                    $scope.sampleOrderNew.SalesGroupId = $scope.salesgroupList[0].Value;
                }
            }
        });
    };
    //Buyer DDL
    $http.get('Parties/buyer/GetCbo/')
        .then(function (response) {
            $scope.buyerList = response.data;
        });

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = result;
    });

    $scope.InsertCurrencyInTbl = function () {
        if (baseService.arrayLength($scope.subMaterialList) > 0) {
            $scope.CurrencyName = $.grep($scope.currencyList, function (item) {
                return item.Value === $scope.sampleOrderNew.CurrencyId;
            })[0].Text;
            for (var i = 0; i < baseService.arrayLength($scope.subMaterialList); i++) {
                $scope.subMaterialList[i].CurrencyId = $scope.sampleOrderNew.CurrencyId;
                $scope.subMaterialList[i].CurrencyName = $scope.CurrencyName;
            }
        }
    };

    cboService.getTestinStdCbo(null, function (response) {
        $scope.testingStdList = response;
    });

    $scope.getUoMCboByMaterialGroup = function (flag) {
        cboService.getUoMCboByMaterialGroup($scope.subMaterialNew.MaterialGroupMasterId, function (response) {
            $scope.subMaterialUOMList = response;
            if (flag) {
                for (var i = 0; i < $scope.subMaterialUOMList.length; i++) {
                    if ($scope.subMaterialUOMList[i].IsBaseUom === 1) {
                        $scope.subMaterialNew.UoMId = $scope.subMaterialUOMList[i].Value;
                        break;
                    }
                }
            }
        });
    };

    $http.get('OrderManagements/salesorderlinear/loadCustomerPaymentTerm/')
        .then(function (response) {
            $scope.paymentTermList = response.data;
        });

    //#region Customer
    $scope.partySearchByList = [
        {
            'name': $scope.partyType + ' Code',
            'value': 'Code'
        },
        {
            'name': $scope.partyType + ' Name',
            'value': 'UserName'
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
    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.sampleOrderNew.PartyCode = party.Code;
            $scope.sampleOrderNew.PartyName = party.UserName;
            $scope.sampleOrderNew.PartyId = party.Id;
            $scope.sampleOrderNew.PaymentTermId = party.PaymentTermId;
            $scope.sampleOrderNew.CurrencyId = party.CurrencyId;
            getPartyPlantList(false);
        }
        $scope.hidePartyPopUp();
    };

    function getPartyPlantList(isEdit) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.sampleOrderNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (!isEdit) {
                    if (item.IsDefault) {
                        $scope.sampleOrderNew.InvoicingPartyPlantId = item.Value;
                        $scope.sampleOrderNew.DeliveryPartyPlantId = item.Value;
                        $scope.sampleOrderNew.InvoicingByAddress = item.Address1;
                        $scope.sampleOrderNew.DeliveryByAddress = item.Address1;
                        $scope.sampleOrderNew.InvoicingState = item.StateName;
                        $scope.sampleOrderNew.DeliveryState = item.StateName;
                    }
                }
            });
        });
    }

    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closeInvoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
    };
    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
            if (flag === 'billTo') {
                $scope.sampleOrderNew.InvoicingState = state;
                return $scope.sampleOrderNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.sampleOrderNew.DeliveryState = state;
                return $scope.sampleOrderNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.sampleOrderNew.InvoicingState = null;
                return $scope.sampleOrderNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.sampleOrderNew.DeliveryState = null;
                return $scope.sampleOrderNew.DeliveryByAddress = null;
            }
        }
    };
    //#endregion Customer

    //#region Material Group
    $scope.valueData = '';

    $scope.materialGroupMstPopUp = function () {
        $scope.materialGroupMstParameters = {
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
        $scope.materialGroupMstList = [];
        $scope.materialGroupMstUrl = 'Materials/materialgroupmaster/getlistbyfinishedgoods';
        baseService.setCurrentPage('materialGroupMstDataList');
        $scope.getMaterialGroupMstData = function (pageno) {
            baseService.paginationBase($scope.materialGroupMstUrl, pageno, $scope.materialGroupMstParameters)
                .then(function (result) {
                    $scope.materialGroupMstDataList = result.Rows;
                    $scope.materialGroupMstParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.materialGroupMstList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.materialGroupMstList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'materialGroupMstId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#materialGroupMstId')).modal('show');
        $scope.getMaterialGroupMstData();
    };
    $scope.selectMGMstDoubleClick = function (data) {
        $scope.subMaterialNew.MaterialGroupMasterId = data.Id;
        $scope.subMaterialNew.MaterialGroupMasterName = data.UserName;
        $scope.getAttribute();
        $scope.subMaterialNew.CurrencyId = $scope.sampleOrderNew.CurrencyId;
        $scope.subMaterialNew.DeliveryDate = $scope.sampleOrderNew.DeliveryDate;
        $scope.getUoMCboByMaterialGroup(true);
        $scope.closeMaterialGroupMst();
    };
    $scope.selectMGMstSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.selectMGMstByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'materialGroupMstId');
        }
        $scope.selectMGMstDoubleClick($scope.valueData)
        $scope.closeMaterialGroupMst();
    };
    $scope.closeMaterialGroupMst = function () {
        $scope.valueData = '';
        $scope.materialGroupMstList = [];
        $scope.materialGroupMstDataList = [];
        angular.element(document.querySelector('#materialGroupMstId')).modal('hide');
    }
    $scope.materialGroupClear = function () {
        $scope.subMaterialNew.MaterialGroupMasterId = null;
        $scope.subMaterialNew.MaterialGroupMasterName = null;
    };
    //#endregion Material Group

    // #region SubMaterial
    $scope.subMaterialCaption = 'Add';
    $scope.subMaterialList = [];
    $scope.subMaterialValue = null;
    $scope.subMaterialId = null;
    $scope.subMaterial = {
        Id: $scope.pk()
        , SampleOrderId: null
        , MaterialGroupMasterId: null
        , MaterialGroupMasterName: null
        , TestingStandardId: null
        , Qty: null
        , UoMId: null
        , Rate: null
        , CurrencyId: null
        , CurrencyName: null
        , Name: null
        , Remarks: null
        , DeliveryDate: null
        , MaterialAttributeValues: []
    };
    $scope.subMaterialNew = Object.assign({}, $scope.subMaterial);
    $scope.ShowSubMaterialFormPopUp = function () {
        $scope.attributeList = [];
        angular.element(document.querySelector('#subMaterialPoUp')).modal('show');
    }
    $scope.CloseSubMaterialPopUp = function () {
        subMaterialClear(null, null);
        angular.element(document.querySelector('#subMaterialPoUp')).modal('hide');
    }
    $scope.AddSubMaterial = function () {
        try {
            subMaterialFieldValidation($scope.subMaterialNew.Qty, 'Quantity');
            subMaterialFieldValidation($scope.subMaterialNew.UoMId, 'Quantity uom');
            subMaterialFieldValidation($scope.subMaterialNew.Rate, 'Quantity rate');

            var name = '';
            for (var j = 0; j < $scope.attributeList.length; j++) {
                if (!baseService.isUndefinedOrNull($scope.attributeList[j].MaterialAttributeValueFreeText)) {
                    if (baseService.isUndefinedOrNull(name))
                        name = $scope.attributeList[j].MaterialAttributeValueFreeText;
                    else name += '-' + $scope.attributeList[j].MaterialAttributeValueFreeText;
                }
            }
            $scope.subMaterialNew.Name = name;
            $scope.subMaterialNew.UoMName = document.getElementById("uOMId").options[document.getElementById('uOMId').selectedIndex].text;
            $scope.subMaterialNew.CurrencyName = document.getElementById("currencyId").options[document.getElementById('currencyId').selectedIndex].text;

            if (baseService.isUndefinedOrNull($scope.subMaterialId)) {
                angular.forEach($scope.attributeList, function (element, l) {
                    $scope.subMaterialNew.MaterialAttributeValues.push({
                        Id: element.Id
                        , SampleOrderId: $scope.sampleOrderNew.Id
                        , SampleOrderSubMaterialId: $scope.subMaterialNew.Id
                        , MaterialAttributeId: element.MaterialAttributeId
                        , MaterialAttributeName: element.MaterialAttributeName
                        , MaterialAttributeValueId: element.MaterialAttributeValueId
                        , MaterialAttributeValueFreeText: element.MaterialAttributeValueFreeText
                    });
                });
                $scope.subMaterial = Object.assign({}, $scope.subMaterialNew);
                $scope.subMaterialList.push($scope.subMaterial);
            }
            else {
                for (var n = 0; n < $scope.subMaterialList.length; n++) {
                    if ($scope.subMaterialList[n].Id === $scope.subMaterialId) {
                        $scope.subMaterialNew.MaterialAttributeValues = [];
                        $scope.subMaterial = Object.assign({}, $scope.subMaterialNew);
                        $scope.subMaterialList[n].Id = $scope.subMaterial.Id;
                        $scope.subMaterialList[n].MaterialMasterId = $scope.subMaterial.MaterialMasterId;
                        $scope.subMaterialList[n].MaterialGroupMasterId = $scope.subMaterial.MaterialGroupMasterId;
                        $scope.subMaterialList[n].SampleOrderId = $scope.subMaterial.SampleOrderId;
                        $scope.subMaterialList[n].Qty = $scope.subMaterial.Qty;
                        $scope.subMaterialList[n].UoMId = $scope.subMaterial.UoMId;
                        $scope.subMaterialList[n].UoMName = $scope.subMaterial.UoMName;
                        $scope.subMaterialList[n].Rate = $scope.subMaterial.Rate;
                        $scope.subMaterialList[n].CurrencyId = $scope.subMaterial.CurrencyId;
                        $scope.subMaterialList[n].CurrencyName = $scope.subMaterial.CurrencyName;
                        $scope.subMaterialList[n].Name = $scope.subMaterial.Name;
                        $scope.subMaterialList[n].Remarks = $scope.subMaterial.Remarks;
                        $scope.subMaterialList[n].DeliveryDate = $scope.subMaterial.DeliveryDate;
                        for (var m = 0; m < $scope.attributeList.length; m++) {
                            if (!baseService.isUndefinedOrNull($scope.attributeList[m].Id))
                                updateSubMaterial($scope.subMaterialList[n].MaterialAttributeValues, $scope.attributeList[m]);
                            else if (baseService.isUndefinedOrNull($scope.attributeList[m].Id) && !baseService.isUndefinedOrNull($scope.attributeList[m].MaterialAttributeValueFreeText)) {
                                $scope.subMaterialList[n].MaterialAttributeValues.push({
                                    Id: $scope.attributeList[m].Id
                                    , SampleOrderId: $scope.sampleOrderNew.Id
                                    , SampleOrderSubMaterialId: $scope.subMaterialNew.Id
                                    , MaterialAttributeId: $scope.attributeList[m].MaterialAttributeId
                                    , MaterialAttributeName: $scope.attributeList[m].MaterialAttributeName
                                    , MaterialAttributeValueId: $scope.attributeList[m].MaterialAttributeValueId
                                    , MaterialAttributeValueFreeText: $scope.attributeList[m].MaterialAttributeValueFreeText
                                });
                            }
                        }

                    }
                }
            }
            subMaterialClear($scope.subMaterialNew.MaterialGroupMasterId, $scope.subMaterialNew.MaterialGroupMasterName);
        } catch (e) {
            ShowResult(e, 'failure', 'subMaterialPoUp');
        }
    };
    function isNull(list) {
        var isNull = true;
        for (var i = 0; i < list.length; i++) {
            if (!baseService.isUndefinedOrNull(list[i].MaterialAttributeValueFreeText)) {
                isNull = false;
                return isNull;
            }
        }
        return isNull;
    }
    function updateSubMaterial(data, element) {
        for (var i = 0; i < data.length; i++) {
            if (element.MaterialAttributeId === data[i].MaterialAttributeId) {
                data[i].Id = element.Id;
                data[i].SampleOrderId = $scope.sampleOrderNew.Id;
                data[i].SampleOrderSubMaterialId = $scope.subMaterialNew.Id;
                data[i].MaterialAttributeId = element.MaterialAttributeId;
                data[i].MaterialAttributeName = element.MaterialAttributeName;
                data[i].MaterialAttributeValueId = element.MaterialAttributeValueId;
                data[i].MaterialAttributeValueFreeText = element.MaterialAttributeValueFreeText;
                return;
            }
        }
    }
    function subMaterialFieldValidation(field, fieldName) {
        if (baseService.isUndefinedOrNull(field))
            throw fieldName + ' is required...............!';
    }
    function materialValueDuplecateCheck(list, tempList) {
        try {
            var hasDifferent = false;
            for (var i = 0; i < list.length; i++) {
                if (list[i].MaterialAttributeValueFreeText !== tempList[i].MaterialAttributeValueFreeText) {
                    hasDifferent = true;
                    break;
                }
            }
            return hasDifferent;
        } catch (e) {
            throw e;
        }
    }
    function subMaterialClear(mgmId, mgmName) {
        $scope.subMaterialNew = {
            Id: $scope.pk()
            , MaterialGroupMasterId: mgmId
            , MaterialGroupMasterName: mgmName
            , MaterialMasterId: null
            , SampleOrderId: null
            , Qty: null
            , UoMId: null
            , Rate: null
            , CurrencyId: null
            , CurrencyName: null
            , Name: null
            , Remarks: null
            , DeliveryDate: null
            , MaterialAttributeValues: []
        };

        $scope.subMaterialCaption = 'Add';
        $scope.subMaterialId = null;
        $scope.subMaterialValue = null;
        $scope.subMaterialNew.CurrencyId = $scope.sampleOrderNew.CurrencyId;
        $scope.subMaterialNew.DeliveryDate = $scope.sampleOrderNew.DeliveryDate;
    }
    $scope.materialAttributeValueEdit = function (data) {
        $scope.subMaterialCaption = 'Update';
        $scope.ShowSubMaterialFormPopUp();
        $scope.subMaterialValue = data.MaterialAttributeValues;
        $scope.subMaterialId = data.Id;
        $scope.subMaterial = data;
        $scope.subMaterialNew = Object.assign({}, $scope.subMaterial);
        $scope.getUoMCboByMaterialGroup(false);
        $scope.getAttribute();
        $scope.subMaterialNew.UoMId = data.UoMId;
    };
    $scope.MaterialAttributeValueDelete = function (index) {
        $scope.SubMaterialIndex = index;
        $scope.subMaterialMessage = 'Are you sure want to delete this............?';
        angular.element(document.querySelector('#subMaterial')).modal('show');
    };
    $scope.removeSubMaterialRow = function () {
        for (var i = 0; i < $scope.subMaterialList.length; i++) {
            $scope.subMaterialList.splice($scope.SubMaterialIndex, 1);
            break;
        }
        $scope.SubMaterialIndex = -1;
    };
    $scope.GetSubMaterial = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/sampleOrder/getsubmaterial?masterId=' + $scope.sampleOrderNew.Id,
        }).then(function successCallback(response) {
            $scope.subMaterialList = response.data;
        });
    };
    // #endregion

    //#region Attribute
    $scope.attributeList = [];
    $scope.getAttribute = function () {
        $scope.attributeList = [];
        $http({
            method: 'GET',
            url: 'OrderManagements/sampleOrder/GetAttributeByMgm?materialGroupMasterId=' + $scope.subMaterialNew.MaterialGroupMasterId + '&subMaterialId=' + $scope.subMaterialNew.Id
        }).then(function successCallback(response) {
            $scope.searchFreeField = false;
            $scope.attributeList = response.data;
            for (var i = 0; i < $scope.attributeList.length; i++) {
                var isFree = $scope.attributeList[i].IsFreeField;
                $scope.attributeList[i].FlagDisable = $scope.attributeList[i].MaterialAttributeValueId !== null ? true : $scope.IsFreeFieldOrNot(isFree);
            }
            if (baseService.arrayLength($scope.subMaterialValue) > 0) {
                for (var t = 0; t < baseService.arrayLength($scope.subMaterialValue); t++) {
                    for (var a = 0; a < baseService.arrayLength($scope.attributeList); a++) {
                        if ($scope.attributeList[a].MaterialAttributeId === $scope.subMaterialValue[t].MaterialAttributeId) {
                            $scope.attributeList[a].MaterialAttributeValueId = $scope.subMaterialValue[t].MaterialAttributeValueId;
                            $scope.attributeList[a].MaterialAttributeValueFreeText = $scope.subMaterialValue[t].MaterialAttributeValueFreeText;
                            $scope.attributeList[a].FlagDisable = baseService.isUndefinedOrNull($scope.subMaterialValue[t].MaterialAttributeValueId) !== true
                                ? true : $scope.IsFreeFieldOrNot($scope.attributeList[a].IsFreeField);
                            break;
                        }
                    }
                }
                $scope.subMaterialValue = null;
            }
        });
    };
    $scope.mvalueindex = -1;
    $scope.materialAttributeValueParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Description",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.materialAttributeValuePoUp = function (id, index) {
        $scope.materialAttributeValueUrl = 'Materials/materialattributevalue/materialattributevaluesearch?materialAttributeId=' + id;
        baseService.setCurrentPage('materialAttributeValueList');
        $scope.getAttributeValueData = function (pageno) {
            baseService.paginationBase($scope.materialAttributeValueUrl, pageno, $scope.materialAttributeValueParameters)
                .then(function (result) {
                    $scope.materialAttributeValueList = result.Rows;
                    $scope.materialAttributeValueParameters.total_count = result.Total;
                    $scope.mvalueindex = index;
                    $scope.searchFreeField = true;
                    angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getAttributeValueData();
    }
    $scope.idNullByFreeText = function (id, index) {
        if ($scope.attributeList[index].MaterialAttributeId === id) {
            $scope.attributeList[index].MaterialAttributeValueId = null;
        }
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
    }

    //#endregion Attribute

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
            $scope.valueParameters.mmAttributeId = data.MaterialMasterAttributeId;
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

    //#region Partner Function

    $scope.partnerFunctionList = [];
    $scope.GetPartnerFunctionPopUp = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.sampleOrderNew.CustomerId))
                return ShowResult("Select customer first...");
            $http({
                method: 'GET',
                url: 'OrderManagements/salesorderlinear/loadpartnerfunction/',
                params: { customerid: $scope.sampleOrderNew.CustomerId }
            }).then(function successCallback(response) {
                $scope.pfPopUpList = response.data;
                setSelectedPF($scope.pfPopUpList, $scope.partnerFunctionList);
                angular.element(document.querySelector('#pfpopup')).modal('show');
            });
        } catch (e) {
            throw e;
        }
    };

    function setSelectedPF(searchlist, selectedlist) {
        for (var i = 0; i < baseService.arrayLength(selectedlist); i++) {
            setSelectedPF2(selectedlist[i], searchlist);
        }
    }
    function setSelectedPF2(ob, searchlist) {
        for (var i = 0; i < baseService.arrayLength(searchlist); i++) {
            if (searchlist[i].PartnerFunctionId === ob.PartnerFunctionId && searchlist[i].CustomerId === ob.CustomerId) {
                searchlist[i].IsSelectedID = true;
                searchlist[i].Id = ob.Id;
            }
        }
    }

    $scope.selectPFByButton = function () {
        for (var i = 0; i < baseService.arrayLength($scope.pfPopUpList); i++) {
            var ob = $scope.pfPopUpList[i];
            if (ob.IsSelectedID && !IsAvailableAT(ob, $scope.partnerFunctionList)) {
                ob['SampleOrderId'] = $scope.sampleOrderNew.Id;
                $scope.partnerFunctionList.push(ob);
            }//exists
        }//for
        angular.element(document.querySelector('#pfpopup')).modal('hide');
    };
    $scope.closePFPopUp = function () {
        angular.element(document.querySelector('#pfpopup')).modal('hide');
    };
    function clearPF() {
        $scope.partnerFunctionList = [];
        $scope.pfPopUpList = [];
    }
    function IsAvailableAT(ob, list) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].AssignmentType === ob.AssignmentType)
                return true;
        }
        return false;
    }

    $scope.removePfConfirmation = function (ob) {
        try {
            $scope.assignmentType = ob.AssignmentType;
            $scope.pf_message = "Are you sure to delete [" + $scope.assignmentType + "] ";
            angular.element(document.querySelector('#pfDelPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.removePf = function () {
        try {
            for (var i = 0; i < baseService.arrayLength($scope.partnerFunctionList); i++) {
                if ($scope.partnerFunctionList[i].AssignmentType === $scope.assignmentType) {
                    $scope.partnerFunctionList.splice(i, 1);
                    break;
                }
            }
            angular.element(document.querySelector('#pfDelPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //$scope.GetSampleOrderPartnerFunction = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'OrderManagements/sampleOrder/getSampleorderpartnerfunction?masterId=' + $scope.sampleOrderNew.Id,
    //    }).then(function successCallback(response) {
    //        $scope.partnerFunctionList = response.data;
    //    })
    //}
    //#endregion Partner Function

    var rangeDate = false;
    $scope.rangeDateValidation = function (div_id, flag) {
        var msg = '';
        if (flag && new Date($scope.sampleOrderNew.DeliveryDate) < new Date($scope.sampleOrderNew.RequestReferenceDate)) {
            $scope.isSet(1);
            $scope.setTab(1);
            rangeDate = true;
            msg = 'Delivery date can not be less then request reference date!';
        }
        else if (new Date($scope.sampleOrderNew.RequestReferenceDate) > Date.now()) {
            $scope.isSet(1);
            $scope.setTab(1);
            rangeDate = true;
            msg = 'Request reference date can not be greater then today date!';
        }
        else rangeDate = false;
        return manualValidation(div_id, rangeDate, msg);
    }

    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.sampleOrder = {};
        $scope.sampleOrderNew = {
            RequestReferenceDate: $filter('dateFiltering')(Date.now())
            , IsChangeable: true
            , PlantId: $window.plantId
        };
        $scope.subMaterialUOMList = [];
        $scope.buyerApplicable = false;
        $scope.subMaterialList = [];
        subMaterialClear(null, null);
        $scope.isSet(1);
        $scope.setTab(1);
        clearPF();
    };
    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.sampleOrderNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
            if (baseService.isUndefinedOrNull($scope.sampleOrderNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
            $scope.$broadcast('show-errors-check-validity');
            $scope.rangeDateValidation('reqRefDateId', false);
            $scope.rangeDateValidation('deliveryDateId', true);
            if ($scope.sampleOrderNewForm.$valid && !rangeDate) {
                $scope.sampleOrderNew = Object.assign({}, $scope.sampleOrderNew);
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'entity': $scope.sampleOrderNew
                            , 'details': $scope.subMaterialList
                            , 'partnerFunctions': $scope.partnerFunctionList
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getData();
                            $scope.Clear();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: {
                            'entity': $scope.sampleOrderNew
                            , 'details': $scope.subMaterialList
                            , 'partnerFunctions': $scope.partnerFunctionList
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getData();
                            $scope.Clear();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.sampleOrderNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.sampleOrderNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    function reDirectToRequiredTab() {
        if ($scope.sampleOrderFormTab1.$invalid)
            $scope.setTab(1);
        else
            $scope.setTab(2);
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.IsMandatoryButNull = function (isMandatory, value) {
        if (isMandatory) {
            if (baseService.isUndefinedOrNull(value)) return true;
            else return false;
        }
        else return false;
    };
}