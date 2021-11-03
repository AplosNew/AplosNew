'use strict';
SalesOrderPackingListController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService','$window'];
function SalesOrderPackingListController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService,$window) {
    $rootScope.title = "Sales Order Packing List";
    $scope.Action = 'Save';
    $scope.SecondAction = 'Save';
    $scope.index = -1;
    $scope.samplePackingList = [];
    $scope.showTbl = false;
    $scope.packLess = false;
    $scope.packLessCol = true;
    $scope.path = 'OrderManagements/salesorderpackinglist/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getPackingFormListUrl = $scope.path + 'GetPackingFormList';
    $scope.getFirstPackingFormListUrl = $scope.path + 'GetFirstPackingForm?id=';
    $scope.getPackingMaterialListUrl = $scope.path + 'GetPackingMaterial?firstFormId=';
    $scope.getSecondPackingListUrl = $scope.path + 'GetSecondPackByFirstPackId?firstFormId=';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete?id=';
    $scope.savePackingFormUrl = $scope.path + 'packingFormCreate';
    $scope.updatePackingFormUrl = $scope.path + 'packingFormEdit';
    $scope.deletePackingFormUrl = $scope.path + 'packingFormDelete?id=';
    $scope.saveSecondPackingFormUrl = $scope.path + 'secondPackingFormCreate';
    $scope.masterList = [];
    baseService.init($scope.getListUrl, null, null, null, 'CustomerName', 'CustomerName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.masterList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.searchMasterList = [
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
            'name': 'Customer',
            'value': 'CustomerName'
        },
        {
            'name': 'PendingQty',
            'value': 'PendingQty'
        }
    ];
    $scope.samplePacking = {
        Id: null
        , CompanyGroupId: $window.companyGroupId
        , CompanyId: $window.companyId
        , PlantId: $window.plantId
        , EntityId: null
        , SalesOrganisationId: null
        , CustomerId: null
        , CustomerName: null
        , BillingAddressId: null
        , BillingAddress: null
        , ShippingAddressId: null
        , ShippingAddress: null
        , PackingDate: $filter('dateFiltering')(Date.now())
        , PendingQty: 0
    };
    $scope.samplePackingNew = Object.assign({}, $scope.samplePacking);

    $scope.billingAddressList = [];
    $scope.shippingAddressList = [];
    function loadAddress() {
        $http({
            method: 'GET',
            url: 'OrderManagements/salesorderpackinglist/getcbobilltopartyaddress',
            params: { customerid: $scope.samplePackingNew.CustomerId }
        }).then(function successCallback(response) {
            $scope.billingAddressList = response.data;
            if (baseService.arrayLength($scope.billingAddressList) === 1) {
                $scope.samplePackingNew.BillingAddressId = response.data[0].Value
                $scope.getAddress($scope.billingAddressList, $scope.samplePackingNew.BillingAddressId, 'BillingAddress', false);
            }
        });
        $http({
            method: 'GET',
            url: 'OrderManagements/salesorderpackinglist/getcboshiptopartyaddress',
            params: { customerid: $scope.samplePackingNew.CustomerId }
        }).then(function successCallback(response) {
            $scope.shippingAddressList = response.data;
            if (baseService.arrayLength($scope.shippingAddressList) === 1) {
                $scope.samplePackingNew.ShippingAddressId = response.data[0].Value
                $scope.getAddress($scope.shippingAddressList, $scope.samplePackingNew.ShippingAddressId, 'ShippingAddress', false);
            }
        });
    }

    $scope.getAddress = function (list, value, fieldName, flag) {
        if (!baseService.isUndefinedOrNull(value) && flag)
            $scope.samplePackingNew[fieldName] = $.grep(list, function (item) { return item.Value === value; })[0].Address;
        else if (baseService.isUndefinedOrNull(value) && flag)
            $scope.samplePackingNew[fieldName] = null;
    }
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.samplePacking = $scope.masterList[$scope.index];
        angular.copy($scope.samplePacking, $scope.samplePackingNew);
        $scope.GetPackLessMaterial();
        $scope.getPackingFormList();
        $scope.getAllMaterial();
        loadAddress();
        $scope.samplePackingNew.BillingAddressId = $scope.samplePacking.BillingAddressId;
        $scope.samplePackingNew.ShippingAddressId = $scope.samplePacking.ShippingAddressId;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    //*************************************Customer Search***************************************************//
    $scope.excluedColumnList = [];
    $scope.customerList = [];
    $scope.customerTitle = 'Customer';
    $scope.valueData = '';
    $scope.customerParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Customer',
        searchBy: "Customer",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getCustomerPopUp = function () {
        $scope.excluedColumnList = [];
        $scope.customerDataList = [];
        $scope.customerUrl = 'OrderManagements/salesorderpackinglist/getcustomerbyspf/';
        baseService.setCurrentPage('dataList');
        $scope.getCustomerData = function (pageno) {
            baseService.paginationBase($scope.customerUrl, pageno, $scope.customerParameters)
                .then(function (result) {
                    $scope.customerDataList = result.Rows;
                    $scope.customerParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.customerList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.customerList);
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
        $scope.samplePackingNew.Id = null;
        $scope.samplePackingNew.EntityId = data.EntityId;
        $scope.samplePackingNew.CustomerId = data.CustomerId;
        $scope.samplePackingNew.CustomerName = data.Customer;
        $scope.samplePackingNew.InvoicingPartyPlantId = null;
        $scope.samplePackingNew.InvoicingByAddress = null;
        $scope.samplePackingNew.DeliveryPartyPlantId = null;
        getPartyPlantList(false);
//        loadAddress();
        $scope.closeCustomer();
    }
    $scope.customerSingleClick = function (data) {
        $scope.valueData = data;
    }
    $scope.customerByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            ShowResult('Please at first select row', 'failure', 'customerPopUp');
        }
        $scope.customerDoubleClick($scope.valueData)
        $scope.closeCustomer();
    }
    $scope.closeCustomer = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#customerPopUp')).modal('hide');
    }
    $scope.clearCustomer = function () {
        $scope.samplePackingNew.CustomerId = null;
        $scope.samplePackingNew.CustomerName = null;
        $scope.samplePackingNew.BillingAddress = null;
        $scope.samplePackingNew.ShippingAddress = null;
        $scope.excluedColumnList = [];
    }
    function getPartyPlantList(isEdit) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.samplePackingNew.CustomerId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (!isEdit) {
                    if (item.IsDefault) {
                        $scope.samplePackingNew.InvoicingPartyPlantId = item.Value;
                        $scope.samplePackingNew.DeliveryPartyPlantId = item.Value;
                        $scope.samplePackingNew.InvoicingByAddress = item.Address1;
                        $scope.samplePackingNew.DeliveryByAddress = item.Address1;
                        $scope.samplePackingNew.InvoicingState = item.StateName;
                        $scope.samplePackingNew.DeliveryState = item.StateName;
                    }
                }
            });
        });
    }

    //*************************************End Customer Search***********************************************//

    //*************************************Material Master By Customer***************************************************//
    $scope.materialMasterDataList = [];
    //$scope.packingList = [];
    $scope.materialMasterTitle = 'Material Master';
    $scope.valueData = '';
    $scope.searchMaterialMaster = [
        {
            'name': 'MaterialGroup (Mst)',
            'value': 'MaterialGroupMaster'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        },
        {
            'name': 'StandardName',
            'value': 'StandardName'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'SubMaterial',
            'value': 'SubMaterial'
        },
        {
            'name': 'Detail',
            'value': 'Detail'
        },
        {
            'name': 'Sample SubMaterial',
            'value': 'SalesOrderMaterial'
        },
        {
            'name': 'Ref.DocNo',
            'value': 'PONumber'
        },
        {
            'name': 'Req.Ref.Date',
            'value': 'RequestReferenceDate'
        }

    ];
    $scope.materialMasterParameters = {
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
    $scope.getMaterialMasterPopUp = function () {
        $scope.excluedColumnList = ['UoMName', 'Count'];
        $scope.materialMasterDataList = [];
        $scope.materialMasterUrl = 'OrderManagements/salesorderpackinglist/getmaterialmasterbycustomer?customerId=' + $scope.samplePackingNew.CustomerId
            + '&sampleOrderSubMaterialIds=' + baseService.getColumnValueList($scope.materialList, 'SalesOrderMaterialId');
        baseService.setCurrentPage('dataList');
        $scope.getMaterialMasterData = function (pageno) {
            baseService.paginationBase($scope.materialMasterUrl, pageno, $scope.materialMasterParameters)
                .then(function (result) {
                    for (var a = 0; a < baseService.arrayLength(result.Rows); a++) {
                        result.Rows[a].Flag = ifSelectedInMaterialPopUp(result.Rows[a].SalesOrderMaterialId);
                    }
                    $scope.materialMasterDataList = result.Rows;
                    $scope.materialMasterParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'materialMasterPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#materialMasterPopUp')).modal('show');
        $scope.getMaterialMasterData();
    }
    function ifSelectedInMaterialPopUp(id) {
        for (var t = 0; t < baseService.arrayLength($scope.materialList); t++) {
            if ($scope.materialList[t].SalesOrderMaterialId === id)
                return true;
        }
        return false;
    }
    $scope.materialList = [];

    $scope.selectMaterial = function (event, data, index) {
        try {
            if (event.currentTarget.checked && !isSamePackingStyle($scope.materialList, data.PackingFormId1, data.PackingFormId2, data.IsSingleEntry)) {
                $scope.materialMasterDataList[index].Flag = false;
                throw 'Packing form is different.';
            }
            var materialMasterId = [];
            materialMasterId.push(data.MaterialMasterId);
            cboService.getUomCboByMaterialMaster(JSON.stringify(materialMasterId), function (response) {
                $scope.materialUoMList = response;
                if (event.currentTarget.checked) {
                    $scope.materialList.push({
                        Id: null
                        , MaterialGroupMasterId: data.MaterialGroupMasterId
                        , MaterialGroupMasterName: data.MaterialGroupMaster
                        , MaterialMasterId: data.MaterialMasterId
                        , MaterialMasterName: data.UserName
                        , SubMaterialId: data.SubMaterialId
                        , SubMaterialName: data.SubMaterial

                        , Characteristics1Id: data.Characteristics1Id
                        , Characteristics2Id: data.Characteristics2Id
                        , Characteristics3Id: data.Characteristics3Id

                        , CharacteristicsValue1Id: data.CharacteristicsValue1Id
                        , CharacteristicsValue2Id: data.CharacteristicsValue2Id
                        , CharacteristicsValue3Id: data.CharacteristicsValue3Id

                        , Detail: data.Detail

                        , SalesOrderPackingListId: $scope.samplePackingNew.Id
                        , SalesOrderId: data.SalesOrderId
                        , SalesOrderMaterialId: data.SalesOrderMaterialId
                        , SalesOrderMaterial: data.SalesOrderMaterial

                        , SamplePackingListMaterialId: $scope.SamplePackingListMaterialId

                        , PONumber: data.PONumber
                        , RequestReferenceDate: data.RequestReferenceDate
                        , DeliveryDate: data.DeliveryDate
                        , Description: null
                        , UoMId: data.UoMId
                        , OrderQty: data.OrderQty
                        , OrderUoMId: data.UoMId
                        , PendingQty: data.PendingQty
                        , OrderUoM: data.UoMName
                        , Rate: data.Rate
                        , CurrencyName: data.CurrencyName
                        , Qty: data.PendingQty
                        , PackingFormId1: data.PackingFormId1
                        , IsSingleEntry: data.IsSingleEntry
                        , PackingFormId2: data.PackingFormId2
                        , Count: data.Count
                        , materialUoMList: $scope.materialUoMList
                    });
                    if (data.Count !== 0 && baseService.isUndefinedOrNull($scope.firstPackingNew.PackingFormId))
                        getFirstForm();
                }
                else
                    materialSplice($scope.materialList, data.SalesOrderMaterialId);
                $scope.materialUoMList = [];
            });
        } catch (e) {
            ShowResult(e, '', 'materialMasterPopUp');
        }
    }
    function isSamePackingStyle(list, id1, id2, isSingleEntry) {
        if (baseService.arrayLength(list) > 0) {
            if (list[0].PackingFormId1 === id1 && list[0].PackingFormId2 === id2 && list[0].IsSingleEntry === isSingleEntry)
                return true;
            else
                return false;
        }
        return true;
    }
    function materialSplice(list, id) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].SalesOrderMaterialId === id)
                list.splice(i, 1);
        }
    }
    $scope.closeMaterialPopUp = function () {
        $scope.clearMaterial();
        angular.element(document.querySelector('#materialMasterPopUp')).modal('hide');
    }
    $scope.clearMaterial = function () {
        $scope.materialMasterDataList = [];
    }
    $scope.packingPopUp = function () {
        try {
            $scope.samplePackingListMaterialId = null;
            materialPackTab();
            angular.element(document.querySelector('#firstPackingPopUp')).modal('show');
        } catch (e) {
            ShowResult(e);
        }
    }

    //*************************************End Material Master By Customer***********************************************//

    //*************************************End PackLessMaterial***********************************************//
    $scope.packLessMaterialList = [];
    $scope.GetPackLessMaterial = function () {
        $http.get($scope.path + 'GetPackLessMaterialList?masterId=' + $scope.samplePackingNew.Id)
            .then(function successCallback(response) {
                $scope.packLessMaterialList = response.data;
            });
    }

    $scope.deletePackLessMaterialModal = function (data, index) {
        $scope.delIndex = index;
        $scope.Id = data.Id;
        $scope.confirm_msg = 'Are you sure want to permanent delete [ ' + data.MaterialGroupMasterName + ' ]';
        angular.element(document.querySelector('#confirm_PackLessMaterial')).modal('show');
    };
    $scope.removeRowFromPackLessMaterialList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'PackLessMaterialDelete?id=' + $scope.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.packLessMaterialList.splice($scope.delIndex, 1);
                $scope.Id = '';
                $scope.delIndex = -1;
                $scope.GetPackLessMaterial();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };
    $scope.editPackLessMaterial = function (data, index) {
        $scope.packLessCol = false;
        $scope.packLess = true;
        $scope.materialList.push(data);
        var materialMasterId = [];
        materialMasterId.push(data.MaterialMasterId);
        cboService.getUomCboByMaterialMaster(JSON.stringify(materialMasterId), function (response) {
            $scope.materialUoMList = response;
            getUoMFromList(data.MaterialMasterId, 0);
            $scope.materialUoMList = [];
        });
        $scope.PackingAction = 'Update';
        materialPackTab();
        angular.element(document.querySelector('#firstPackingPopUp')).modal('show');
    }
    //*************************************End PackLessMaterial***********************************************//

    //**********************************1st Packing Form****************************************************************//
    $scope.PackingAction = 'Save';
    $scope.firstPackingListIndex = -1;
    $scope.firstPackingList = [];
    $scope.firstPacking = {
        Id: null
        , SalesOrderPackingListId: null
        , SamplePackingListMaterialId: $scope.samplePackingListMaterialId
        , FirstFormId: null
        , PackingFormId: null
        , PackingForm: null
        , PackingFormNo: null
        , ContainerQty: 0
        , ContentQty: 0
        , UoMId: null
        , UoMName: null
        , Sequence: null
        , IsSingleEntry: false
        , PackFormType: 'First'
        , Count: 0
    };
    $scope.firstPackingNew = Object.assign({}, $scope.firstPacking);
    function getFirstForm() {
        try {
            $http.get($scope.path + '/GetPackingListByMaterialGroupMaster?materialGroupMasterId=' + $scope.materialList[0].MaterialGroupMasterId)
                .then(function (response) {
                    $scope.firstPacking = response.data[0];
                    $scope.firstPackingNew = Object.assign({}, $scope.firstPacking);
                    $scope.firstPackingNew.Id = null;
                });
        } catch (e) {
            ShowResult(e);
        }
    }
    $scope.add1stList = function () {
        try {
            //if ($scope.firstPackingNew.IsSingleEntry)
            //    throw 'Unique entry is not allowed.!';
            for (var i = 0; i < $scope.materialList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.materialList[i].Qty) || $scope.materialList[i].Qty === 0)
                    throw 'Please insert material quantity.!';
            }
            if (baseService.isUndefinedOrNull($scope.firstPackingNew.ContainerQty) || parseInt($scope.firstPackingNew.ContainerQty) == 0)
                throw 'Please insert ' + $scope.firstPackingNew.PackingForm + ' quantity';
            if ($scope.firstPackingNew.IsSingleEntry && baseService.isUndefinedOrNull($scope.firstPackingNew.PackingFormNo))
                throw 'Please insert ' + $scope.firstPackingNew.PackingForm + ' no!';
            if ($scope.firstPackingNew.IsSingleEntry) {
                for (var t = 0; t < $scope.firstPackingList.length; t++) {
                    var row = $scope.firstPackingList[t];
                    if (baseService.isAvailableInList(row.PackingFormNo, $scope.firstPackingNew.PackingFormNo, t, $scope.firstPackingListIndex))
                        throw '[' + $scope.firstPackingNew.PackingFormNo + '] already exist in grid!';
                }
            }
            if ((baseService.isUndefinedOrNull($scope.firstPackingNew.ContentQty) || (parseInt($scope.firstPackingNew.ContentQty) === 0)))
                throw 'Please insert total quantity.!';
            if (!$scope.firstPackingNew.IsSingleEntry && baseService.arrayLength($scope.firstPackingList) === 1 && $scope.firstPackingListIndex === -1)
                throw 'Multiple ' + $scope.firstPackingNew.PackingForm + ' can not allowed as per material group master configuration!';
            $scope.firstPacking = Object.assign({}, $scope.firstPackingNew);
            if ($scope.firstPackingListIndex === -1) {
                $scope.firstPackingList.push({
                    Id: baseService.pk()
                    , SalesOrderPackingListId: $scope.samplePackingNew.Id
                    , SamplePackingListMaterialId: $scope.SamplePackingListMaterialId
                    , FirstFormId: null
                    , PackingFormId: $scope.firstPacking.PackingFormId
                    , PackingForm: $scope.firstPacking.PackingForm
                    , PackingFormNo: $scope.firstPacking.PackingFormNo
                    , ContainerQty: $scope.firstPacking.ContainerQty
                    , ContentQty: $scope.firstPacking.ContentQty
                    , MaterialGroupPackingFormId: $scope.firstPacking.MaterialGroupPackingFormId
                    , Sequence: $scope.firstPacking.Sequence
                    , IsSingleEntry: $scope.firstPacking.IsSingleEntry
                    , PackFormType: $scope.firstPacking.PackFormType
                    , Count: $scope.firstPacking.Count
                    , UoMId: $scope.firstPacking.UoMId
                    , UoMName: $scope.firstPacking.UoMName
                });
            }
            else {
                $scope.firstPackingList[$scope.firstPackingListIndex] = $scope.firstPacking;
                $scope.firstPackingListIndex = -1;
            }
            $scope.firstPacking = {};
            $scope.firstPackingNew.Id = null;
            //$scope.firstPackingNew.ContainerQty = null;
            $scope.firstPackingNew.ContentQty = null;
            $scope.firstPackingNew.ContainerQty = $scope.firstPackingNew.IsSingleEntry ? 1 : null;
        } catch (e) {
            ShowResult(e, '', 'firstPackingPopUp');
        }
    }
    $scope.editFirstPackingList = function (data, index) {
        try {
            $scope.firstPackingListIndex = index;
            $scope.firstPacking = $scope.firstPackingList[$scope.firstPackingListIndex];
            $scope.firstPackingNew = Object.assign({}, $scope.firstPacking);
        } catch (e) {
            ShowResult(e, '', 'firstPackingPopUp');
        }
    }
    function clearFirstPacking() {
        $scope.samplePackingListMaterialId = null;
        $scope.materialList = [];
        $scope.firstPacking = {
            Id: null
            , SalesOrderPackingListId: null
            , FirstFormId: null
            , SamplePackingListMaterialId: $scope.SamplePackingListMaterialId
            , PackingFormId: null
            , PackingForm: null
            , PackingFormNo: null
            , ContainerQty: 0
            , ContentQty: 0
            , Sequence: null
            , IsSingleEntry: false
            , PackFormType: 'First'
            , Count: 0
        };
        $scope.firstPackingNew = {};
        $scope.PackingAction = 'Save';
        $scope.firstPackingList = [];
        $scope.firstPackingListIndex = -1;
    }
    $scope.closePackingPopUp = function () {
        try {
            $scope.packLessCol = true;
            $scope.packLess = false;
            clearFirstPacking();
            $scope.SamplePackingListMaterialId = null;
            angular.element(document.querySelector('#firstPackingPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'firstPackingPopUp');
        }
    }
    $scope.sumQty = function (list, qty) {
        if (!baseService.isUndefinedOrNull($scope.firstPackingNew.PackingFormId)) {
            var totalQty = 0;
            for (var i = 0; i < baseService.arrayLength(list); i++) {
                totalQty += parseInt(list[i].Qty);
            }
            $scope.firstPackingNew.ContentQty = totalQty;
        }
    }
    $scope.editFirstForm = function (id, samplePackingListMaterialId) {
        try {
            $scope.SamplePackingListMaterialId = samplePackingListMaterialId
            materialPackTab();
            getFirstPackingMaterial(id, samplePackingListMaterialId)
        } catch (e) {
            ShowResult(e, '', 'firstPackingPopUp');
        }
    }
    function getFirstPackingMaterial(id, samplePackingListMaterialId) {
        try {
            $http.get($scope.getPackingMaterialListUrl + samplePackingListMaterialId)
                .then(function (response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.materialList = response.data;
                        var materialMasterId = [];
                        if (baseService.arrayLength($scope.materialList) > 0) {
                            for (var i = 0; i < baseService.arrayLength($scope.materialList); i++) {
                                materialMasterId.push($scope.materialList[i].MaterialMasterId);
                            }
                            cboService.getUomCboByMaterialMaster(JSON.stringify(materialMasterId), function (response) {
                                $scope.materialUoMList = response;
                                setUoMInMaterialList();
                                $scope.materialUoMList = [];
                            });
                        }
                        getFirstPackingForm(id, samplePackingListMaterialId, response.data[0].MaterialGroupMasterId);
                        getFirstForm();
                        angular.element(document.querySelector('#firstPackingPopUp')).modal('show');
                        $scope.PackingAction = 'Update';
                    }
                });
        } catch (e) {
            ShowResult(e, '');
        }
    }
    function setUoMInMaterialList() {
        for (var t = 0; t < baseService.arrayLength($scope.materialList); t++) {
            getUoMFromList($scope.materialList[t].MaterialMasterId, t)
        }
    }
    function getUoMFromList(mmId, i) {
        $scope.materialList[i].materialUoMList = [];
        for (var t = 0; t < baseService.arrayLength($scope.materialUoMList); t++) {
            if ($scope.materialUoMList[t].MaterialMasterId === mmId) {
                $scope.materialList[i].materialUoMList.push($scope.materialUoMList[t]);
            }
        }
    }
    function getFirstPackingForm(id, samplePackingMaterialId, mgId) {
        $http.get($scope.getFirstPackingFormListUrl + id + '&samplePackingMaterialId=' + samplePackingMaterialId + '&materialGroupMstId=' + mgId)
            .then(function (response) {
                $scope.firstPackingList = response.data;
            });
    }
    //********************************End 1st Packing Form**************************************************************//

    //**********************************Second Packing Form****************************************************************//
    $scope.secondFormPopup = function (data) {
        try {
            $scope.firstContainerQty = data.ContainerQty;
            $scope.firstTotalQty = data.ContentQty;
            $scope.fUoM = data.UoMName;
            $scope.SamplePackingListMaterialId = data.SamplePackingListMaterialId
            $scope.secondPackingListIndex = -1;
            $scope.secondPackingList = [];
            $scope.secondPacking = {
                Id: null
                , SalesOrderPackingListId: null
                , SamplePackingListMaterialId: null
                , FirstFormId: data.Id
                , PackingFormId: null
                , PackingForm: null
                , PackingFormNo: null
                , ContainerQty: 0
                , ContentQty: 0
                , UoMId: null
                , UoMName: null
                , Sequence: null
                , IsSingleEntry: false
                , PackFormType: 'Second'
                , Count: 0
            };

            $scope.secondPackingUrl = 'Get2ndPackingListByMaterialGroupMaster?firstFormId=' + data.SamplePackingListMaterialId;
            $http.get($scope.path + $scope.secondPackingUrl)
                .then(function (response) {
                    $scope.secondPacking = response.data[0];
                    $scope.secondPackingNew = Object.assign({}, $scope.secondPacking);
                    $scope.secondPackingNew.Id = null;
                    $scope.secondPackingNew.FirstFormId = data.Id;
                    $scope.getSecondPackingList(data);
                    if (!$scope.secondPackingNew.IsSingleEntry)
                        $scope.secondPackingNew.ContentQty = $scope.firstTotalQty;
                    $scope.secondPackingNew.ContainerQty = $scope.firstContainerQty;
                    $scope.secondPackingNew.UoMId = data.UoMId;
                    $scope.secondPackingNew.UoMName = data.UoMName;
                    angular.element(document.querySelector('#secondPackingPopUp')).modal('show');
                });
        } catch (e) {
            ShowResult(e);
        }
    }
    $scope.closeSecondPacking = function () {
        try {
            $scope.secondPacking = {
                Id: null
                , SalesOrderPackingListId: null
                , FirstFormId: null
                , PackingFormId: null
                , PackingForm: null
                , PackingFormNo: null
                , ContainerQty: 0
                , ContentQty: 0
                , Sequence: null
                , IsSingleEntry: false
                , PackFormType: 'Second'
                , Count: 0
            };
            $scope.SecondAction = 'Save';
            $scope.secondPackingList = [];
            $scope.firstTotalQty = null;
            $scope.secondPackingListIndex = -1;
            angular.element(document.querySelector('#secondPackingPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'secondPackingPopUp');
        }
    }
    $scope.addSecondList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.secondPackingNew.ContainerQty) || parseInt($scope.secondPackingNew.ContainerQty) === 0)
                throw 'Please insert ' + $scope.secondPackingNew.PackingForm + ' quantity';
            if ($scope.secondPackingNew.IsSingleEntry && baseService.isUndefinedOrNull($scope.secondPackingNew.PackingFormNo))
                throw 'Please insert ' + $scope.secondPackingNew.PackingForm + ' no!';
            if ($scope.secondPackingNew.IsSingleEntry) {
                for (var i = 0; i < baseService.arrayLength($scope.secondPackingList); i++) {
                    var row = $scope.secondPackingList[i];
                    if (baseService.isAvailableInList(row.PackingFormNo, $scope.secondPackingNew.PackingFormNo, i, $scope.secondPackingListIndex))
                        throw $scope.secondPackingNew.PackingForm + 'No : [' + $scope.secondPackingNew.PackingFormNo + '] already exist in grid!';
                }
            }
            if (baseService.isUndefinedOrNull($scope.secondPackingNew.ContentQty) || (parseInt($scope.secondPackingNew.ContentQty) === 0))
                throw 'Please insert total quantity.!';
            if (!$scope.secondPackingNew.IsSingleEntry && baseService.arrayLength($scope.secondPackingList) === 1 && $scope.secondPackingListIndex === -1)
                throw 'Multiple ' + $scope.secondPackingNew.PackingForm + ' can not allowed as per material group master configuration!';
            $scope.secondPacking = Object.assign({}, $scope.secondPackingNew);
            if ($scope.secondPackingListIndex === -1) {
                $scope.secondPackingList.push({
                    Id: baseService.pk()
                    , SalesOrderPackingListId: $scope.samplePackingNew.Id
                    , FirstFormId: $scope.secondPacking.FirstFormId
                    , PackingFormId: $scope.secondPacking.PackingFormId
                    , PackingForm: $scope.secondPacking.PackingForm
                    , PackingFormNo: $scope.secondPacking.PackingFormNo
                    , ContainerQty: $scope.secondPacking.ContainerQty
                    , ContentQty: $scope.secondPacking.ContentQty
                    , MaterialGroupPackingFormId: $scope.secondPacking.MaterialGroupPackingFormId
                    , Sequence: $scope.secondPacking.Sequence
                    , IsSingleEntry: $scope.secondPacking.IsSingleEntry
                    , PackFormType: $scope.secondPacking.PackFormType
                    , UoMId: $scope.secondPacking.UoMId
                    , UoMName: $scope.secondPacking.UoMName
                    , SamplePackingListMaterialId: $scope.SamplePackingListMaterialId
                });
            }
            else {
                $scope.secondPackingList[$scope.secondPackingListIndex] = $scope.secondPacking;
                $scope.secondPackingListIndex = -1;
            }
            $scope.secondPacking = {};
            $scope.secondPackingNew.Id = null;
            //$scope.secondPackingNew.ContainerQty = null;
            $scope.secondPackingNew.ContainerQty = $scope.secondPackingNew.IsSingleEntry ? 1 : null;
            $scope.secondPackingNew.ContentQty = null;
        } catch (e) {
            ShowResult(e, '', 'secondPackingPopUp');
        }
    }
    $scope.edit2ndPackingList = function (data, index) {
        try {
            $scope.secondPackingListIndex = index;
            $scope.secondPacking = $scope.secondPackingList[$scope.secondPackingListIndex];
            $scope.secondPackingNew = Object.assign({}, $scope.secondPacking);
        } catch (e) {
            ShowResult(e, '', 'firstPackingPopUp');
        }
    }

    //********************************End Second Packing Form**************************************************************//

    //*****************************************All Materials**************************************************************//
    $scope.getAllMaterial = function () {
        $http.get($scope.path + 'GetAllMaterialList?masterId=' + $scope.samplePackingNew.Id)
            .then(function successCallback(response) {
                $scope.allMaterialList = response.data;
            });
    }
    //**************************************End All Materials*************************************************************//

    //*****************************************View Materials**************************************************************//
    $scope.getViewMaterial = function (id, smpMaterialId) {
        $http.get($scope.path + 'GetViewMaterialList?firstFormId=' + id + '&smpMaterialId=' + smpMaterialId)
            .then(function successCallback(response) {
                $scope.viewMaterialList = response.data;
                angular.element(document.querySelector('#viewMaterialMasterPopUp')).modal('show');
            });
    }
    $scope.closeViewMaterialPopUp = function () {
        $scope.viewMaterialList = [];
        angular.element(document.querySelector('#viewMaterialMasterPopUp')).modal('hide');
    }
    //**************************************End View Materials*************************************************************//
    function materialPackTab() {
        $scope.tab = 1;
        $scope.setTab = function (newTab) {
            $scope.tab = newTab;
        };
        $scope.isSet = function (tabNum) {
            return $scope.tab === tabNum;
        };
    }
    $scope.mainTab = (baseService.arrayLength($scope.packLessMaterialList) > 0) ? 1 : 2;
    $scope.setTabMain = function (newTab) {
        $scope.mainTab = newTab;
    };
    $scope.isSetMain = function (tabNum) {
        return $scope.mainTab === tabNum;
    };
    $scope.Clear = function () {
        $scope.packingList = [];
        $scope.samplePacking = {};
        $scope.samplePackingNew = {
            PackingDate: $filter('dateFiltering')(Date.now())
            , CompanyGroupId: $scope.samplePackingNew.CompanyGroupId
            , CompanyId: $scope.samplePackingNew.CompanyId
            , PlantId: $scope.samplePackingNew.PlantId
        };
        $scope.excluedColumnList = [];
        $scope.billingAddressList = [];
        $scope.shippingAddressList = [];
        $scope.allMaterialList = [];
        $scope.viewMaterialList = [];
        $scope.packLessMaterialList = [];
    }

    $scope.deleteModal = function (name, listName, index) {
        $scope.delIndex = index;
        $scope.list = listName;
        $scope.confirm_msg = 'Are you sure want to permanent delete [ ' + name + ' ]';
        angular.element(document.querySelector('#confirm_PopUp')).modal('show');
    };
    $scope.removeRowFromList = function () {
        $scope[$scope.list].splice($scope.delIndex, 1);
        if ($scope.list === 'materialList')
            $scope.materialLengthCheck();
        $scope.list = '';
        $scope.delIndex = -1;
    };

    $scope.materialLengthCheck = function () {
        if (baseService.arrayLength($scope.materialList) == 0) {
            $scope.firstPackingList = [];
            clearFirstPacking();
        }
    }
    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.samplePackingNewForm.$valid) {
                $scope.samplePacking = Object.assign({}, $scope.samplePackingNew);
                if (baseService.isUndefinedOrNull($scope.samplePacking.Id)) {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { entity: $scope.samplePacking },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true)
                            ShowResult(response.data.Message, 'failure');
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.samplePacking.Id = response.data.Id;
                            $scope.samplePackingNew.Id = $scope.samplePacking.Id;
                            $scope.getData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
                else {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: { 'entity': $scope.samplePacking },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true)
                            ShowResult(response.data.Message, 'failure');
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getData();
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
    $scope.SavePackingForm = function () {
        try {
            if (baseService.arrayLength($scope.firstPackingList) == 0) {
                if ($scope.materialList[0].Count > 0)
                    throw 'Please insert packing form.!';
            }
            checkUoMInMaterial($scope.materialList)
            checkUoMInFirstPacking($scope.materialList[0].UoMId, $scope.firstPackingList)
            var mQty = 0, fpQty = 0;
            for (var i = 0; i < baseService.arrayLength($scope.materialList); i++) {
                mQty += parseFloat($scope.materialList[i].Qty);
            }
            for (var t = 0; t < baseService.arrayLength($scope.firstPackingList); t++) {
                fpQty += parseFloat($scope.firstPackingList[t].ContentQty);
            }
            if (parseFloat(fpQty) !== parseFloat(mQty) && $scope.materialList[0].Count > 0)
                throw 'Material and packing form quantity are not same.!';
            if ($scope.PackingAction === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.savePackingFormUrl,
                    data: {
                        'materialList': $scope.materialList,
                        'firstPackingList': $scope.firstPackingList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'firstPackingPopUp');
                    else {
                        ShowResult(response.data.Message, 'success', 'firstPackingPopUp');
                        $scope.closePackingPopUp();
                        $scope.getPackingFormList();
                        $scope.getAllMaterial();
                        $scope.GetPackLessMaterial();
                        angular.element(document.querySelector('#firstPackingPopUp')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'firstPackingPopUp');
                }
            }
            else {
                $http({
                    method: 'POST',
                    url: $scope.updatePackingFormUrl,
                    data: {
                        'materialList': $scope.materialList,
                        'firstPackingList': $scope.firstPackingList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'firstPackingPopUp');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getPackingFormList();
                        $scope.getAllMaterial();
                        $scope.GetPackLessMaterial();
                        clearFirstPacking();
                        angular.element(document.querySelector('#firstPackingPopUp')).modal('hide');
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'firstPackingPopUp');
                });
            }
        } catch (e) {
            ShowResult(e, 'failure', 'firstPackingPopUp');
        }
    }
    function checkUoMInMaterial(list) {
        for (var i = 1; i < baseService.arrayLength(list); i++) {
            if (list[0].UoMId !== list[i].UoMId)
                throw 'UoM is not same in material tab.';
        }
    }
    function checkUoMInFirstPacking(uomId, list) {
        for (var i = 1; i < baseService.arrayLength(list); i++) {
            if (list[0].UoMId !== uomId)
                throw 'UoM is not same material and packing form.';
        }
    }
    $scope.checkUoMInMaterial = function (list, list2) {
        try {
            CloseModalShowResult('firstPackingPopUp');
            for (var i = 1; i < baseService.arrayLength(list); i++) {
                if (list[0].UoMId !== list[i].UoMId)
                    throw 'UoM is not same in material tab.';
            }
            $scope.firstPackingNew.UoMName = $.grep(list[0].materialUoMList, function (item) { return item.Value === list[0].UoMId; })[0].Text;
            $scope.firstPackingNew.UoMId = (baseService.isUndefinedOrNull($scope.firstPackingNew.UoMId) === false) ? $scope.firstPackingNew.UoMId : list[0].UoMId;
            for (var t = 0; t < baseService.arrayLength(list2); t++) {
                list2[t].UoMId = list[0].UoMId;
                list2[t].UoMName = $scope.firstPackingNew.UoMName;
            }
        } catch (e) {
            $scope.setTab(1);
            $scope.isSet(1);
            ShowResult(e, 'failure', 'firstPackingPopUp');
        }
    }
    $scope.saveSecondPackingForm = function () {
        try {
            var tqty = 0;
            var tContainerqty = 0;
            for (var i = 0; i < baseService.arrayLength($scope.secondPackingList); i++) {
                tContainerqty += parseFloat($scope.secondPackingList[i].ContainerQty);
                tqty += parseFloat($scope.secondPackingList[i].ContentQty);
            }
            if (parseFloat(tqty) !== parseFloat($scope.firstTotalQty))
                throw 'Total content quantity does not match first form';
            if (parseFloat(tContainerqty) < parseFloat($scope.firstContainerQty))
                throw 'Total container quantity can not be less than first form container quantity';
            $http({
                method: 'POST',
                url: $scope.saveSecondPackingFormUrl,
                data: { 'secondPackingList': $scope.secondPackingList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'secondPackingPopUp');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.closeSecondPacking();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'secondPackingPopUp');
            }
        } catch (e) {
            ShowResult(e, 'failure', 'secondPackingPopUp');
        }
    }
    $scope.UpdatePackLessMaterial = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + 'PackLessMaterialEdit',
                data: {
                    'materialList': $scope.materialList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'firstPackingPopUp');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getAllMaterial();
                    $scope.closePackingPopUp();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'firstPackingPopUp');
            }
        } catch (e) {
            ShowResult(e, 'failure', 'firstPackingPopUp');
        }
    }

    $scope.Delete = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.samplePackingNew.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.deleteUrl + $scope.samplePackingNew.Id,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.deleteFirstForm = function (data, listName, index) {
        try {
            $scope.delId = data.Id;
            $scope.list = listName;
            $scope.firstPackIndex = index;
            $scope.firstPack_msg = 'Are you sure want to permanent delete [ ' + data.ContainerQty + ' ' + data.PackingForm + ' ]';
            angular.element(document.querySelector('#confirm_FirstPack')).modal('show');
        } catch (e) {
            ShowResult(e);
        }
    }
    $scope.deleteFirstPack = function () {
        $http({
            method: 'POST',
            url: $scope.deletePackingFormUrl + $scope.delId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getPackingFormList();
                $scope.getAllMaterial();
                $scope.list = '';
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    $scope.packingList = [];
    $scope.getPackingFormList = function () {
        if (!baseService.isUndefinedOrNull($scope.samplePackingNew.Id)) {
            $http.get($scope.getPackingFormListUrl + '?masterId=' + $scope.samplePackingNew.Id)
                .then(function (response) {
                    $scope.packingList = response.data;
                });
        }
        else
            $scope.packingList = [];
    }
    $scope.getSecondPackingList = function (data) {
        if (!baseService.isUndefinedOrNull(data.Id)) {
            $http.get($scope.getSecondPackingListUrl + data.Id + '&samplePackingListMaterialId=' + data.SamplePackingListMaterialId)
                .then(function (response) {
                    $scope.secondPackingList = response.data;
                    if (baseService.arrayLength(response.data) > 0)
                        $scope.SecondAction = 'Update';
                    else
                        $scope.SecondAction = 'Save';
                });
        }
        else
            $scope.secondPackingList = [];
    }
};