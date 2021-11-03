'use strict';
SampleRequisitionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function SampleRequisitionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Sample Order";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.sampleRequisitionList = [];
    $scope.showTbl = false;
    $scope.path = 'OrderManagements/sampleRequisition/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete?id=';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.buyerApplicable = false;
    $scope.soMasterList = [];
    baseService.init($scope.getListUrl, null, null, null, 'Plant, Entity, CustomerName', 'CustomerName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.soMasterList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.pk = function () {
        return 'n-' + Math.floor(Math.random() * 900000) + 100000;
    };
    $scope.sampleRequisition = {
        Id: null
        , PlantId: null
        , EntityId: null
        , BuyerId: null
        , CustomerId: null
        , CustomerName: null
        , CurrencyId: null
        , PaymentTermId: null
        , RequestReferenceDate: $filter('dateFiltering')(Date.now())
        , ReferenceDocNo: null
        , BuyerRequirementDate: null
        , PaidStatus: "paid"
        , DevelopmentCategory: null
        , IsChangeable: true
    };
    $scope.sampleRequisitionNew = Object.assign({}, $scope.sampleRequisition);
    $scope.Get = function (id, index) {
        $scope.Action = 'Update';
        $scope.index = index;
        $scope.sampleRequisition = $scope.soMasterList[$scope.index];
        angular.copy($scope.sampleRequisition, $scope.sampleRequisitionNew);
        LoadSalesOrganization();
        $scope.LoadSalesGroup();
        $scope.GetFinishGood();
        $scope.getEntityAndSalesOrg();
        //$scope.getUoMCboByMaterialGroup();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.searchsampleRequisitionList = [
        {
            'name': 'Plant',
            'value': 'Plant'
        },
        {
            'name': 'Entity',
            'value': 'Entity'
        },
        {
            'name': 'Buyer',
            'value': 'Buyer'
        },
        {
            'name': 'Customer',
            'value': 'CustomerName'
        },
        {
            'name': 'Currency',
            'value': 'Currency'
        },
        {
            'name': 'Ref. DocNo',
            'value': 'ReferenceDocNo'
        }
        ,
        {
            'name': 'Paid Status',
            'value': 'PaidStatus'
        }
    ];
    $scope.plantList = [];
    $http.get('Productions/salesorderlinear/getplantlist/')
        .then(function (response) {
            $scope.plantList = response.data;
        });
    $scope.developmentCategoryList = [];
    cboService.getEnumCbo("/enum/GetDevelopmentCategoryEnumCbo", function (result) {
        $scope.developmentCategoryList = result;
    });
    $scope.entityList = [];
    $scope.getEntityAndSalesOrg = function () {
        //Entity DDL
        cboService.getCboProductionEntityByPlant(null, null, $scope.sampleRequisitionNew.PlantId, function (result) {
            $scope.entityList = result;
        });
        LoadSalesOrganization();
        $http({
            method: 'GET',
            url: 'Setups/plantconfig/GetPlantConfigDataByPlantId?plantid=' + $scope.sampleRequisitionNew.PlantId,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.buyerApplicable = response.data[0].BuyerApplicable;
            }
            else
                $scope.buyerApplicable = false;
        });
    }
    //Sales Org DDL
    $scope.clearChangeOnSalesOrg = function () {
        $scope.sampleRequisitionNew.CustomerId = null;
        $scope.sampleRequisitionNew.CustomerName = null;
        $scope.sampleRequisitionNew.PaymentTermId = null;
        $scope.partnerFunctionList = [];
    }
    function LoadSalesOrganization() {
        $http({
            method: 'GET',
            url: 'Organizations/salesorganisation/getcbobyplant?plantId=' + $scope.sampleRequisitionNew.PlantId,
        }).then(function successCallback(response) {
            $scope.salesOrganizationList = response.data;
            if (baseService.arrayLength($scope.salesOrganizationList) == 1) {
                $scope.sampleRequisitionNew.SalesOrganisationId = $scope.salesOrganizationList[0].Value;
                $scope.LoadSalesGroup()
            }
        });
    }
    //Sales Group DDL
    $scope.LoadSalesGroup = function () {
        if (baseService.isUndefinedOrNull($scope.sampleRequisitionNew.SalesOrganisationId)) return;
        $http({
            method: 'GET',
            url: 'Organizations/salesgroup/getcbo?salesorganisationid=' + $scope.sampleRequisitionNew.SalesOrganisationId,
        }).then(function successCallback(response) {
            $scope.salesgroupList = response.data;
            if (baseService.arrayLength($scope.salesgroupList) == 1) {
                $scope.sampleRequisitionNew.SalesGroupId = $scope.salesgroupList[0].Value;
            }
        });
    }
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
                return item.Value == $scope.sampleRequisitionNew.CurrencyId;
            })[0].Text;
            for (var i = 0; i < baseService.arrayLength($scope.subMaterialList); i++) {
                $scope.subMaterialList[i].CurrencyId = $scope.sampleRequisitionNew.CurrencyId;
                $scope.subMaterialList[i].CurrencyName = $scope.CurrencyName;
            }
        }
    }

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
    }
    $http.get('Productions/salesorderlinear/loadCustomerPaymentTerm/')
        .then(function (response) {
            $scope.paymentTermList = response.data;
        });

    //*************************************Customer Search***************************************************//
    $scope.customerList = [];
    $scope.customerTitle = 'Customer';
    $scope.valueData = '';
    $scope.excluedColumnList = ['IsChangeable'];
    $scope.customerParameters = {
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
    $scope.getCustomerPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.sampleRequisitionNew.SalesOrganisationId))
            return ShowResult('Please select sales organisation', 'failure');
        $scope.customerDataList = [];
        $scope.customerUrl = 'Productions/salesorderlinear/getcustomersearchdata?sorgid=' + $scope.sampleRequisitionNew.SalesOrganisationId;
        baseService.setCurrentPage('dataList');
        $scope.getCustomerData = function (pageno) {
            baseService.paginationBase($scope.customerUrl, pageno, $scope.customerParameters)
                .then(function (result) {
                    $scope.customerDataList = result.customerSearchData.Rows;
                    $scope.customerParameters.total_count = result.customerSearchData.Total;
                    if (baseService.arrayLength($scope.customerList) === 0) {
                        baseService.getDDLSearchColumn(result.customerSearchData.Rows, $scope.customerList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'customerPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#customerPopUp')).modal('show');
        $scope.getCustomerData();
    }
    $scope.customerDoubleClick = function (data) {
        $scope.sampleRequisitionNew.CustomerId = data.Id;
        $scope.sampleRequisitionNew.CustomerName = data.UserName;
        $scope.sampleRequisitionNew.CurrencyId = data.CurrencyId;
        $scope.sampleRequisitionNew.PaymentTermId = data.PaymentTermId == '' ? null : data.PaymentTermId;
        $scope.sampleRequisitionNew.IsChangeable = data.IsChangeable;
        $scope.closeCustomer();
    }
    $scope.customerSingleClick = function (data) {
        $scope.valueData = data;
    }
    $scope.customerByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            ShowResult('Please at first select row');
        }
        $scope.customerDoubleClick($scope.valueData)
        $scope.closeCustomer();
    }
    $scope.closeCustomer = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#customerPopUp')).modal('hide');
    }
    $scope.clearCustomer = function () {
        $scope.sampleRequisitionNew.CustomerId = null;
        $scope.sampleRequisitionNew.CustomerName = null;
        $scope.sampleRequisitionNew.PaymentTermId = null;
        $scope.sampleRequisitionNew.IsChangeable = true;
        $scope.partnerFunctionList = [];
    }

    //*************************************End Customer Search***********************************************//

    //*************************************Material Group Mst***********************************************//
    $scope.valueData = '';

    $scope.materialGroupMstPopUp = function () {
        $scope.materialGroupMstParameters = {
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
    }
    $scope.selectMGMstDoubleClick = function (data) {
        $scope.subMaterialNew.MaterialGroupMasterId = data.Id;
        $scope.subMaterialNew.MaterialGroupMasterName = data.UserName;
        $scope.getAttribute();
        $scope.subMaterialNew.CurrencyId = $scope.sampleRequisitionNew.CurrencyId;
        $scope.subMaterialNew.BuyerRequirementDate = $scope.sampleRequisitionNew.BuyerRequirementDate;
        $scope.getUoMCboByMaterialGroup(true);
        $scope.closeMaterialGroupMst();
    }
    $scope.selectMGMstSingleClick = function (data) {
        $scope.valueData = data;
    }
    $scope.selectMGMstByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'materialGroupMstId');
        }
        $scope.selectMGMstDoubleClick($scope.valueData)
        $scope.closeMaterialGroupMst();
    }
    $scope.closeMaterialGroupMst = function () {
        $scope.valueData = '';
        $scope.materialGroupMstList = [];
        $scope.materialGroupMstDataList = [];
        angular.element(document.querySelector('#materialGroupMstId')).modal('hide');
    }
    $scope.materialGroupClear = function () {
        $scope.subMaterialNew.MaterialGroupMasterId = null;
        $scope.subMaterialNew.MaterialGroupMasterName = null;
    }
    //*************************************End Material Group Mst*******************************************//

    // #region SubMaterial
    $scope.subMaterialCaption = 'Add';
    $scope.subMaterialList = [];
    $scope.subMaterialValue = null;
    $scope.subMaterialId = null;
    $scope.subMaterial = {
        Id: $scope.pk()
        , SampleRequisitionId: null
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
        , BuyerRequirementDate: null
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
                        , SampleRequisitionId: $scope.sampleRequisitionNew.Id
                        , SampleRequisitionSubMaterialId: $scope.subMaterialNew.Id
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
                        $scope.subMaterialList[n].SampleRequisitionId = $scope.subMaterial.SampleRequisitionId;
                        $scope.subMaterialList[n].Qty = $scope.subMaterial.Qty;
                        $scope.subMaterialList[n].UoMId = $scope.subMaterial.UoMId;
                        $scope.subMaterialList[n].UoMName = $scope.subMaterial.UoMName;
                        $scope.subMaterialList[n].Rate = $scope.subMaterial.Rate;
                        $scope.subMaterialList[n].CurrencyId = $scope.subMaterial.CurrencyId;
                        $scope.subMaterialList[n].CurrencyName = $scope.subMaterial.CurrencyName;
                        $scope.subMaterialList[n].Name = $scope.subMaterial.Name;
                        $scope.subMaterialList[n].Remarks = $scope.subMaterial.Remarks;
                        $scope.subMaterialList[n].BuyerRequirementDate = $scope.subMaterial.BuyerRequirementDate;
                        for (var m = 0; m < $scope.attributeList.length; m++) {
                            updateSubMaterial($scope.subMaterialList[n].MaterialAttributeValues, $scope.attributeList[m]);
                        };
                    }
                }
            }
            subMaterialClear($scope.subMaterialNew.MaterialGroupMasterId, $scope.subMaterialNew.MaterialGroupMasterName);
        } catch (e) {
            ShowResult(e, 'failure', 'subMaterialPoUp')
        }
    }
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
                data[i].SampleRequisitionId = $scope.sampleRequisitionNew.Id;
                data[i].SampleRequisitionSubMaterialId = $scope.subMaterial.Id;
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
            , SampleRequisitionId: null
            , Qty: null
            , UoMId: null
            , Rate: null
            , CurrencyId: null
            , CurrencyName: null
            , Name: null
            , Remarks: null
            , BuyerRequirementDate: null
            , MaterialAttributeValues: []
        };

        $scope.subMaterialCaption = 'Add';
        $scope.subMaterialId = null;
        $scope.subMaterialValue = null;
        $scope.subMaterialNew.CurrencyId = $scope.sampleRequisitionNew.CurrencyId;
        $scope.subMaterialNew.BuyerRequirementDate = $scope.sampleRequisitionNew.BuyerRequirementDate;
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
    $scope.GetFinishGood = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/sampleRequisition/getRequisitionFinishGoods?masterId=' + $scope.sampleRequisitionNew.Id,
        }).then(function successCallback(response) {
            $scope.subMaterialList = response.data;
        })
    }
    // #endregion

    //*************************************Attribute and Value********************************************************//
    $scope.attributeList = [];
    $scope.getAttribute = function () {
        $scope.attributeList = [];
        $http({
            method: 'GET',
            url: 'OrderManagements/sampleRequisition/GetAttributeByMgm?materialGroupMasterId=' + $scope.subMaterialNew.MaterialGroupMasterId
            + '&subMaterialId=' + $scope.subMaterialNew.Id,
        }).then(function successCallback(response) {
            $scope.searchFreeField = false;
            $scope.attributeList = response.data;
            for (var i = 0; i < $scope.attributeList.length; i++) {
                var isFree = $scope.attributeList[i].IsFreeField;
                $scope.attributeList[i].FlagDisable = $scope.attributeList[i].MaterialAttributeValueId != null ? true : $scope.IsFreeFieldOrNot(isFree);
            }
            if (baseService.arrayLength(($scope.subMaterialValue)) !== 0) {
                for (var t = 0; t < baseService.arrayLength($scope.subMaterialValue); t++) {
                    for (var i = 0; i < baseService.arrayLength($scope.attributeList); i++) {
                        if ($scope.attributeList[i].MaterialAttributeId === $scope.subMaterialValue[t].MaterialAttributeId) {
                            $scope.attributeList[i].MaterialAttributeValueId = $scope.subMaterialValue[t].MaterialAttributeValueId;
                            $scope.attributeList[i].MaterialAttributeValueFreeText = $scope.subMaterialValue[t].MaterialAttributeValueFreeText;
                            break;
                        }
                    }
                }
                $scope.subMaterialValue = null;
            }
        })
    }
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
    $scope.getAttrValue = function (id, code) {
        $scope.attributeList[$scope.mvalueindex].MaterialAttributeValueId = id;
        $scope.attributeList[$scope.mvalueindex].MaterialAttributeValueFreeText = code;
        $scope.attributeList[$scope.mvalueindex].FlagDisable = $scope.searchFreeField;
        $scope.mvalueindex = -1;
        angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('hide');
    }
    $scope.idNullByFreeText = function (id, index) {
        if ($scope.attributeList[index].MaterialAttributeId === id) {
            $scope.attributeList[index].MaterialAttributeValueId = null;
        }
    }
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
    $scope.materialAttributeValueId = '';
    $scope.materialAttributeValueCode = '';
    $scope.materialAttributeValueIndex = -1;
    $scope.SelectMA = function (id, code, index) {
        $scope.materialAttributeValueId = id;
        $scope.materialAttributeValueCode = code;
        $scope.searchFreeField = true;
        $scope.materialAttributeValueIndex = index;
    }
    $scope.SelectMAButton = function () {
        if (baseService.isUndefinedOrNull($scope.materialAttributeValueId))
            ShowResult('Please at first select row', 'failure', 'materialAttributeValuePoUp');
        $scope.getAttrValue($scope.materialAttributeValueId, $scope.materialAttributeValueCode);
        $scope.materialAttributeValueId = '';
        $scope.materialAttributeValueCode = '';
        $scope.materialAttributeValueIndex = -1;
        angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('hide');
    }
    $scope.materialAttributeValueClear = function (index) {
        $scope.attributeList[index].MaterialAttributeValueId = null;
        $scope.attributeList[index].MaterialAttributeValueFreeText = null;
        $scope.searchFreeField = false;
        var isFree = $scope.attributeList[index].IsFreeField;
        $scope.attributeList[index].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
    }
    $scope.ClosematerialAttributePopUp = function () {
        angular.element(document.querySelector('#materialAttributeValuePoUp')).modal('hide');
    }
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
    //*************************************End Attribute and Vale****************************************************//

    //*************************************Partner Function********************************************************//
    $scope.partnerFunctionList = [];
    $scope.GetPartnerFunctionPopUp = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.sampleRequisitionNew.CustomerId))
                return ShowResult("Select customer first...");
            $http({
                method: 'GET',
                url: 'Productions/salesorderlinear/loadpartnerfunction/',
                params: { customerid: $scope.sampleRequisitionNew.CustomerId }
            }).then(function successCallback(response) {
                $scope.pfPopUpList = response.data;
                setSelectedPF($scope.pfPopUpList, $scope.partnerFunctionList);
                angular.element(document.querySelector('#pfpopup')).modal('show');
            });
        } catch (e) {
        }
    }

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
                ob['SampleRequisitionId'] = $scope.sampleRequisitionNew.Id;
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
            if (list[i].AssignmentType == ob.AssignmentType)
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
                if ($scope.partnerFunctionList[i].AssignmentType == $scope.assignmentType) {
                    $scope.partnerFunctionList.splice(i, 1);
                    break;
                }
            }
            angular.element(document.querySelector('#pfDelPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.GetSampleRequisitionPartnerFunction = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/sampleRequisition/getSampleorderpartnerfunction?masterId=' + $scope.sampleRequisitionNew.Id,
        }).then(function successCallback(response) {
            $scope.partnerFunctionList = response.data;
        })
    }
    //*************************************End Partner Function****************************************************//

    var rangeDate = false;
    $scope.rangeDateValidation = function (div_id, flag) {
        var msg = '';
        if (flag && new Date($scope.sampleRequisitionNew.BuyerRequirementDate) < new Date($scope.sampleRequisitionNew.RequestReferenceDate)) {
            $scope.isSet(1);
            $scope.setTab(1);
            rangeDate = true;
            msg = 'BuyerRequirement date can not be less then request reference date!';
        }
        else if (new Date($scope.sampleRequisitionNew.RequestReferenceDate) > Date.now()) {
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
        $scope.sampleRequisition = {}
        $scope.sampleRequisitionNew = {
            RequestReferenceDate: $filter('dateFiltering')(Date.now())
            , IsChangeable: true
        };
        $scope.subMaterialUOMList = [];
        $scope.buyerApplicable = false;
        $scope.subMaterialList = [];
        subMaterialClear(null, null);
        $scope.isSet(1);
        $scope.setTab(1);
        clearPF();
    }
    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $scope.rangeDateValidation('reqRefDateId', false);
            $scope.rangeDateValidation('deliveryDateId', true);
            if ($scope.sampleRequisitionNewForm.$valid && !rangeDate) {
                $scope.sampleRequisitionNew = Object.assign({}, $scope.sampleRequisitionNew);
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'entity': $scope.sampleRequisitionNew
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
                    }
                }
                else if ($scope.Action == "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: {
                            'entity': $scope.sampleRequisitionNew
                            , 'details': $scope.subMaterialList
                            , 'partnerFunctions': $scope.partnerFunctionList
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
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
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.sampleRequisitionNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.sampleRequisitionNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
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
    }
    function reDirectToRequiredTab() {
        if ($scope.sampleRequisitionFormTab1.$invalid)
            $scope.setTab(1);
        else
            $scope.setTab(2);
    }
    // #endregion
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
};