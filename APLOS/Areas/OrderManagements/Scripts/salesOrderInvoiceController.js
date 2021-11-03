'use strict';
SalesOrderInvoiceController.$inject = ["cboService", "$window", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", '$controller'];
function SalesOrderInvoiceController(cboService, $window, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "Sales Order Invoice";
    $scope.message_confirmation = "";
    $scope.path = 'OrderManagements/salesorderinvoice/';
    $scope.maxRow = 10;
    $scope.SaveMasterDisabled = false;
    $scope.SaveDetailDisabled = false;
    $scope.salesOrderInvoiceList = [];
    $scope.sbmasterList = [];//
    $scope.sbdetailList = [];//
    $scope.sbfileList = [];
    $scope.sbplList = [];
    $scope.sbcharList = [];
    $scope.masterList = [];
    $scope.detailList = [];
    $scope.entityList = [];
    $scope.plantList = [];
    $scope.charList = [];
    $scope.charSaveList = [];
    $scope.plList = [];
    $scope.salesOrganizationList = [];
    $scope.salesgroupList = [];
    $scope.customerSearchData = [];
    $scope.sbcustomerSearchData = [];
    $scope.paymentTermList = [];

    $scope.detail = {
        Id: null,
        SalesOrderPackingListMasterId: null,
        SalesOrderMasterId: null,
        SalesOrderMaterialMasterId: null,
        MaterialMasterId: null,
        MaterialMaster: null,
        Po: null,
        DeliveryDate: null,
        PacketQty: null,
        BalanceQty: null,
        Uom: null,
        Characteristics1Id: null,
        CharacteristicsValue1Id: null,
        Characteristics2Id: null,
        CharacteristicsValue2Id: false,
        Characteristics3Id: null,
        CharacteristicsValue3Id: null,
        UomId: null,
        Qty: null,
        Rate: null
    };
    $scope.file = {
        Id: null,
        CompanyGroupId: $window.companyGroupId,
        CompanyId: $window.companyId,
        PlantId: $window.plantId,
        EntityId: null,
        SalesOrganizationId: null,
        CustomerId: null,
        CustomerName: null,
        PaymentTermId: null,
        SalesGroupId: null,
        CurrencyId: null,
        SalesTypeId: null,
        InvoiceNo: null,
        InvoiceDate: $filter('dateFiltering')(Date.now()),
        InvoiceValue: null,
        ActualDueDate: $filter('dateFiltering')(Date.now()),
        RevisedDueDate: null,
        BaseOnDueDate: null,
        BaseNoOfDays: null
    };
    $scope.fileNew = Object.assign({}, $scope.file);
    $scope.addNewShow = false;
    $scope.partyType = 'Customer';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $scope.getEntityDdl = function () {
        $scope.entityList = [];
        cboService.getCboProductionEntityByPlant(null, null, $scope.fileNew.PlantId, function (result) {
            $scope.entityList = result;
        });
    };
    $scope.getEntityDdl();
    $scope.masterSearchByList = [
        {
            'Text': 'Customer',
            'Value': 'CustomerName'
        },
        {
            'Text': 'Sales Organization',
            'Value': 'SalesOrganization'
        },
        {
            'Text': 'Sales Group',
            'Value': 'SalesGroup'
        },
        {
            'Text': 'Sales Type',
            'Value': 'SalesType'
        },
        {
            'Text': 'Payment Term',
            'Value': 'PaymentTerm'
        }
    ];
    $scope.parameters.searchBy = "Customer";
    $scope.getData = function () {
        $rootScope.parameters.plantId = $scope.fileNew.PlantId;
        $rootScope.parameters.entityId = $scope.fileNew.EntityId;
        baseService.init('OrderManagements/SalesOrderInvoice/Getmasterinfo?plantid=' + $scope.fileNew.PlantId, null, null, null, 'CustomerName', 'CustomerName');
        $scope.loadMasterData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.salesOrderInvoiceList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMasterData();
    }
    //******************Customer************//
    // #region Customer
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
            if ($scope.CustomerSearchFrom === 'MATERIAL_MASTER') {
                $scope.fileNew.CustomerId = party.Id;
                $scope.fileNew.CustomerName = party.UserName;
                $scope.fileNew.CurrencyId = party.CurrencyId;
                $scope.fileNew.PaymentTermId = party.PaymentTermId;
                $scope.fileNew.PaymentTermName = party.PaymentTermName;
                $scope.fileNew.IsChangeable = party.IsPaymentTermChangeable;
                //GetBaseLineDateSetting($scope.fileNew.PaymentTermId);//170828
            }
            else if ($scope.CustomerSearchFrom === 'PO') {
                $scope.customerPO.CustomerId = party.Id;
                $scope.customerPO.CustomerName = party.UserName;
            }
        }
        $scope.hidePartyPopUp();
    };
    $scope.findCustomerSearchData = function (searchFrom) {
        $scope.CustomerSearchFrom = searchFrom;
        //searchCustomer();
        $scope.showPartyPopUp();
        angular.element(document.querySelector('#customersearchpopup')).modal('show');
    };
    $scope.clearCustomerSearchData = function () {
        $scope.fileNew.CustomerId = "";
        $scope.fileNew.CustomerName = "";
    };
    $scope.sbCustomerList = [];
    function searchCustomer() {
        baseService.init('OrderManagements/salesordermatrix/getcustomersearchdata/', null, 10, null, 'UserName', 'UserName');
        $scope.loadCustomerData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    console.log(result);
                    $scope.customerSearchData = [];
                    $scope.customerSearchData = result.Rows;
                    if (baseService.arrayLength($scope.sbCustomerList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbCustomerList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadCustomerData();
    }
    // #endregion Customer




    $scope.getMasterData = function () {
        baseService.setCurrentPage('masterList');
        baseService.init($scope.path + "getmasterinfo/", null, $scope.maxRow, null, 'InvoiceDateId', 'InvoiceNo');
        $scope.loadMasterData = function (pageno) {//loadMMData
            $rootScope.parameters.plantid = $scope.master.PlantId;

            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.masterList = result.Rows;
                    //console.log('99*',$scope.fileList);
                    if (baseService.arrayLength($scope.sbmasterList) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbmasterList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMasterData();
    }
    $scope.sbplList = [
        {
            'Text': 'Material Master',
            'Value': 'MaterialMasterName'
        },
        {
            'Text': 'Article',
            'Value': 'ArticleName'
        },
        {
            'Text': 'Material Group',
            'Value': 'MaterialGroupName'
        }
    ];
    $scope.getPLData = function () {
        baseService.setCurrentPage('plList');
        baseService.init("OrderManagements/ButtonSalesOrderPackingList/GetPackingListWithPartyId/", null, $scope.maxRow, null, 'MaterialMasterName', 'MaterialMasterName');
        $scope.loadPLData = function (pageno) {//loadMMData
            $rootScope.parameters.partyId = $scope.fileNew.CustomerId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.plList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadPLData();
    }
    $scope.getCharData = function () {
        baseService.setCurrentPage('charList');
        baseService.init($scope.path + "getcharqty/", null, $scope.maxRow, null, 'SKU', 'cv1,cv2,cv3');
        $scope.loadCharData = function (pageno) {//loadMMData
            $rootScope.parameters.sommid = $scope.detail.SalesOrderMaterialMasterId;

            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.charList = result.Rows;
                    // console.log('88', $scope.obList);
                    if (baseService.arrayLength($scope.sbcharList) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbcharList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadCharData();
    }
    $scope.getCharSavedList = function (sommid) {
        try {
            $http.get($scope.path + "getsavedcharqty?sommid=" + sommid + "&packingmasterid=" + $scope.master.Id)
                .then(function (response) {
                    //$scope.charSaveList = response.data;
                    $scope.charSaveList = [];
                    for (var i = 0; i < baseService.arrayLength(response.data); i++) {
                        var ob = response.data[i];
                        $scope.charSaveList.push({
                            Id: ob.Id,
                            Sku: ob.Sku,
                            cv1: ob.cv1,
                            cv2: ob.cv2,
                            cv3: ob.cv3,
                            CharacteristicsValue1Id: ob.CharacteristicsValue1Id,
                            CharacteristicsValue2Id: ob.CharacteristicsValue2Id,
                            CharacteristicsValue3Id: ob.CharacteristicsValue3Id,
                            Characteristics1Id: ob.Characteristics1Id,
                            Characteristics2Id: ob.Characteristics2Id,
                            Characteristics3Id: ob.Characteristics3Id,
                            OQty: ob.OrderQty,
                            PacketQty: ob.TPackingQty,
                            BalanceQty: ob.BalanceQty,
                            Qty: ob.CurrentQty,
                            SalesOrderPackingListMasterId: $scope.detail.SalesOrderPackingListMasterId,
                            SalesOrderMasterId: $scope.detail.SalesOrderMasterId,
                            MaterialMaster: $scope.detail.MaterialMaster,
                            MaterialMasterId: $scope.detail.MaterialMasterId,
                            Po: $scope.detail.Po,
                            DeliveryDate: $scope.detail.DeliveryDate,
                            UomId: ob.UomId,
                            EntityId: $scope.master.EntityId,
                            PlantId: $scope.master.PlantId,
                            Uom: ob.Uom,
                            SalesOrderMaterialMasterId: $scope.detail.SalesOrderMaterialMasterId
                        })
                    }
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.LoadSalesGroup = function (salesorganisationid) {
        $scope.salesgroupList = [];
        $http({
            method: 'GET',
            url: 'Organizations/salesgroup/getcbo?salesorganisationid=' + salesorganisationid,
        }).then(function successCallback(response) {
            $scope.salesgroupList = response.data;
            if (baseService.arrayLength($scope.salesgroupList) == 1) {
                $scope.SalesOrderMaster.SalesGroupId = $scope.salesgroupList[0].Value;
            }
        });
    }
    $scope.LoadSalesOrganization = function () {
        $http({
            method: 'GET',
            url: 'Organizations/salesorganisation/getcbobyplant?plantId=' + $scope.fileNew.PlantId,
        }).then(function successCallback(response) {
            $scope.salesOrganizationList = response.data;
            $scope.clear();
            if (baseService.arrayLength($scope.salesOrganizationList) === 1) {
                $scope.master.SalesOrganisationId = $scope.salesOrganizationList[0].Value;
                $scope.LoadSalesGroup($scope.salesOrganizationList[0].Value);
            }
            else {
                $scope.salesgroupList = [];
            }
        });
    }
    $scope.setCustomerData = function (customerSearchIndex) {
        $scope.master.CustomerId = $scope.customerSearchData[customerSearchIndex].Id;
        $scope.master.Customer = $scope.customerSearchData[customerSearchIndex].UserName;
        $scope.master.CurrencyId = $scope.customerSearchData[customerSearchIndex].CurrencyId;
        $scope.master.PaymentTermId = $scope.customerSearchData[customerSearchIndex].PaymentTermId;
        $scope.master.IsChangeable = $scope.customerSearchData[customerSearchIndex].IsChangeable;
        GetBaseLineDateSetting($scope.master.PaymentTermId);//170828
        angular.element(document.querySelector('#customersearchpopup')).modal('hide');
    };
    $scope.getcustomerSearchData = function () {
        baseService.setCurrentPage('customerSearchData');
        baseService.init($scope.path + 'getcustomersearchdata/', null, $scope.maxRow, null, 'Code', 'Code');
        $scope.CustSearch = function (pageno) {
            $rootScope.parameters.sorgid = $scope.master.SalesOrganizationId;
            //$rootScope.parameters.mmid = $scope.SalesOrderMaterialMaster.MaterialMasterId;
            baseService.pagination(pageno)
                .then(function (result) {
                    //console.log('result', result.customerSearchData);
                    $scope.customerSearchData = result.customerSearchData.Rows;
                    if (baseService.arrayLength($scope.sbcustomerSearchData) == 0) {
                        baseService.getDDLSearchColumn(result.customerSearchData.Rows, $scope.sbcustomerSearchData);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.CustSearch();
    };
    //                if (baseService.arrayLength($scope.searchbyPOMasterlist) == 0) {
    //                    baseService.getDDLSearchColumn(result.Rows, $scope.searchbyPOMasterlist);
    //                }
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //    }; $scope.loadPOMasterData();
    //}
    ///**************************************************grid row selected event function*********************************
    function SetSelectedInSearchList(searchlist, selectedlsit) {
        for (var i = 0; i < baseService.arrayLength(selectedlsit); i++) {
            SetSelectedByID(selectedlsit[i].SalesOrderPackingListMasterId, searchlist);
        }
    }
    function SetSelectedByID(id, searchlist) {
        for (var i = 0; i < baseService.arrayLength(searchlist); i++) {
            if (searchlist[i].SalesOrderPackingListMasterId == id) {
                searchlist[i].IsSelectedId = true;
                break;
            }
        }
    }
    function createguid(prefix) {
        var d = new Date().getTime();
        d += (parseInt(Math.random() * 100)).toString();
        if (undefined === prefix) {
            prefix = 'uid-';
        }
        d = prefix + d;
        return d;
    };
    $scope.clear = function () {
        $scope.addNewShow = false;
        $scope.fileNew.Id = null;
        var _PackingDate = $filter('dateFiltering')(new Date(), 'dd-MMM-yyyy');
        $scope.master.InvoiceDate = _PackingDate;
        $scope.master.InvoiceNo = null;
        $scope.master.InvoiceValue = 0;
        $scope.master.Customer = null;
        $scope.master.CustomerId = null;
        $scope.master.SalesGroupId = null;
        $scope.master.SalesOrganizationId = null;
        $scope.master.SalesTypeId = null;
        $scope.master.CurrencyId = null;

        ClearObject($scope.detail);
        $scope.sbmasterList = [];
        $scope.sbdetailList = [];
        $scope.masterList = [];
        $scope.detailList = [];
    }
    $scope.addNew = function () {
        $scope.clear();
        $scope.addNewShow = true;
    }
    $scope.clearCustomer = function () {
        $scope.master.CustomerId = null;
        $scope.master.Customer = null;
        $scope.master.PaymentTermId = null;
        $scope.master.IsChangeable = true;
    }
    $scope.getMasterSingle = function (ob) {
        //ShowResult(e, 'Error');
        console.log(ob);
        $scope.addNew();
        $scope.master.Id = ob.Id;
        $scope.master.InvoiceDate = ob.InvoiceDate;
        $scope.master.InvoiceNo = ob.InvoiceNo;
        $scope.master.InvoiceValue = ob.InvoiceValue;
        $scope.master.Customer = ob.Customer;
        $scope.master.CustomerId = ob.CustomerId;
        $scope.master.SalesGroupId = ob.SalesGroupId;
        $scope.master.SalesOrganizationId = ob.SalesOrganizationId;
        $scope.master.SalesTypeId = ob.SalesTypeId;
        $scope.master.CurrencyId = ob.CurrencyId;
        $scope.master.EntityId = ob.EntityId;
        $scope.master.PaymentTermId = ob.PaymentTermId;

        LoadPackingListHead(ob.Id);

        angular.element(document.querySelector('#mastersearchpopup')).modal('hide');
    };

    function LoadPackingListHead(id) {
        $http.get($scope.path + "getInvoicePackingListHead?masterid=" + id)
            .then(function (response) {
                $scope.detailList = [];
                $scope.detailList = response.data;
                AmountCalculateEdit($scope.detailList);
            });
    }
    function getInvoiceMaster(id) {
        $http.get($scope.path + "getInvoiceMaster?id=" + id)
            .then(function (response) {
                //$scope.master = null;
                //if (baseService.arrayLength(response.data)>0) {
                //    $scope.master.Qty = response.data[0].Qty;
                //}
            });
    }
    function AmountCalculateEdit(list) {
        var _amount = 0;
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            //console.log('1', list[i].CurrentQty);
            //console.log('2', list[i].Rate);
            _amount += list[i].Amount;
        }
        $scope.master.InvoiceValue = _amount;
    }
    $scope.AmountCalculate = function (list) {
        var _amount = 0;
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            //console.log('1', list[i].CurrentQty);
            //console.log('2', list[i].Rate);
            _amount += list[i].CurrentQty * list[i].Rate;
        }
        $scope.detail.Amount = _amount;
    }
    function LoadPDDetail(id) {
        $scope.pdList = [];
        $http.get("/OrderManagements/ButtonSalesOrderPackingList/GetDetailList?buttonSalesOrderPackingId=" + id)
            .then(function (response) {
                $scope.pdList = response.data.Rows;
                //set packed qty
                // ViewAllMaterialSetPackingQty(id);

                $scope.AmountCalculate($scope.pdList);
            });
    }
    function LoadDetailEdit(id) {
        $http.get($scope.path + "get_invoiced_material_edit_setqty?ipackmasterid=" + id)
            .then(function (response) {
                $scope.pdList = [];
                $scope.pdList = response.data;
                // console.log('*9*9',$scope.pdList)
                $scope.AmountCalculate($scope.pdList);
            });
    }
    function ViewAllMaterialSetPackingQty(masterid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getpdlist_SetQty/',
            params: { masterid: masterid }
        }).then(function successCallback(response) {
            var res = response.data;
            if (baseService.arrayLength(res) > 0) {
                SetQty($scope.pdList, res);
            }//if count
        });
    }
    function SetQty(search_list, list) {
        for (var i = 0; i < baseService.arrayLength(search_list); i++) {
            for (var c = 0; c < baseService.arrayLength(list); c++) {
                if (IsMMFound(search_list[i], list[c])) {
                    console.log('ic', i);
                    //search_list[i].OrderQty = list[c].OrderQty;
                    search_list[i].PackingQty = list[c].PackingQty;
                    search_list[i].CurrentQty = list[c].CurrentQty;
                    //search_list[i].OrderQty = list[c].OrderQty;
                }//if
            }//for
        }//for
    }
    function IsMMFound(search_ob, ob) {
        //SalesOrderMasterId SalesOrderMaterialMasterId  SalesOrderCharacteristicsValue1stId SalesOrderCharacteristicsValue2ndId
        // var ob=list[i];
        console.log(search_ob, ob);
        if (search_ob.SalesOrderCharacteristicsValue2ndId == ob.SalesOrderCharacteristicsValue2ndId
            && search_ob.SalesOrderCharacteristicsValue1stId == ob.SalesOrderCharacteristicsValue1stId
            && search_ob.SalesOrderMaterialMasterId == ob.SalesOrderMaterialMasterId
            && search_ob.SalesOrderMasterId == ob.SalesOrderMasterId
        ) {
            console.log('88', ob);
            return true;
        }//field check
        return false;
    }
    $scope.getFileSingle = function (ob) {
        //$scope.file.Id = ob.Id;
        //$scope.file.FileNo = ob.FileNo;
        //$scope.file.CustomerId = ob.CustomerId;
        //$scope.file.Customer = ob.Customer;

        var _PackingDate = $filter('dateFiltering')(new Date(), 'dd-MMM-yyyy');

        $scope.master.Id = createguid("m");
        $scope.master.SalesOrderMasterId = ob.Id;
        $scope.master.CustomerId = ob.CustomerId;
        $scope.master.Customer = ob.Customer;
        $scope.master.FileNo = ob.FileNo;
        $scope.master.PackingDate = _PackingDate;

        angular.element(document.querySelector('#filesearchpopup')).modal('hide');
    };
    $scope.getCharSingle = function (ob) {
        console.log('5', ob);

        $scope.charSaveList.push({
            Id: createguid("c"),
            SKU: ob.SKU,
            cv1: ob.cv1,
            cv2: ob.cv2,
            cv3: ob.cv3,
            CharacteristicsValue1Id: ob.CharacteristicsValue1Id,
            CharacteristicsValue2Id: ob.CharacteristicsValue2Id,
            CharacteristicsValue3Id: ob.CharacteristicsValue3Id,
            Characteristics1Id: ob.Characteristics1Id,
            Characteristics2Id: ob.Characteristics2Id,
            Characteristics3Id: ob.Characteristics3Id,
            OQty: ob.OQty,
            PacketQty: ob.PacketQty,
            BalanceQty: ob.BalanceQty,
            Qty: ob.Qty,
            SalesOrderPackingListMasterId: $scope.detail.SalesOrderPackingListMasterId,
            SalesOrderMasterId: $scope.detail.SalesOrderMasterId,
            MaterialMaster: $scope.detail.MaterialMaster,
            MaterialMasterId: $scope.detail.MaterialMasterId,
            Po: $scope.detail.Po,
            DeliveryDate: $scope.detail.DeliveryDate,
            UomId: ob.UomId,
            EntityId: $scope.master.EntityId,
            PlantId: $scope.master.PlantId,
            Uom: ob.Uom,
            SalesOrderMaterialMasterId: $scope.detail.SalesOrderMaterialMasterId
        })
        console.log('55', $scope.charSaveList);
        angular.element(document.querySelector('#charsearchpopup')).modal('hide');
    };
    function detailOb(ob) {
        $scope.detail.SalesOrderMasterId = $scope.master.SalesOrderMasterId;
        $scope.detail.SalesOrderMaterialMasterId = ob.SalesOrderMaterialMasterId;
        $scope.detail.SalesOrderPackingListMasterId = $scope.master.Id;
        $scope.detail.MaterialMaster = ob.MaterialMaster;
        $scope.detail.MaterialMasterId = ob.MaterialMaster;
        $scope.detail.Po = ob.PoNo;
        $scope.detail.DeliveryDate = ob.DeliveryDate;
    }

    $scope.getPLSingle = function (ob) {
        //$scope.detail.Id = ob.Id;
        //if(baseSe)
        //if (IsExistSomm($scope.obSaveList, ob.SalesOrderMaterialMasterId) == false) {
        $scope.detailList.push(ob);
        // detailOb(ob);
        //$scope.obSaveList.push({
        //    //$scope.master.Id = createguid("m");
        //    Id: createguid("d"),
        //    SalesOrderPackingListMasterId: $scope.master.Id,
        //    SalesOrderMasterId: $scope.master.SalesOrderMasterId,
        //    MaterialMaster: ob.MaterialMaster,
        //    MaterialMasterId: ob.MaterialMasterId,
        //    Po: ob.PoNo,
        //    DeliveryDate: ob.DeliveryDate,
        //    Qty: ob.OrderQty,
        //    PacketQty: ob.PackingQty,
        //    BalanceQty: ob.BalanceQty,
        //    Uom: ob.Uom,
        //    UomId: ob.UomId,
        //    SalesOrderMaterialMasterId: ob.SalesOrderMaterialMasterId
        //});
        //}//if exist

        angular.element(document.querySelector('#plsearchpopup')).modal('hide');
    };
    $scope.selectPLMultiple = function () {
        angular.forEach($scope.plList, function (item) {
            if (item.Active) {
                item.SalesOrderPackingListMaterialId = item.Id;
                if (!$filter("filter")($scope.pdList, { SalesOrderPackingListMaterialId: item.Id }).length > 0) {
                    $scope.pdList.push(item);
                }
            }
        });
        angular.element(document.querySelector('#plsearchpopup')).modal('hide');

    };
    $scope.selectPLSingle = function (ob) {
        try {
            //console.log('888',ob);
            //ob.Id = createguid('id');
            if (baseService.isUndefinedOrNull(ob.InvoiceNo)) {
                $scope.detail.SalesOrderPackingListMasterId = ob.SalesOrderPackingListMasterId;
                $scope.detail.SalesOrderMaterialMasterId = ob.SalesOrderMaterialMasterId;
                $scope.detail.SalesOrderPackingListMaterialId = ob.SalesOrderPackingListMaterialId;
                $scope.detail.MaterialMasterId = ob.MaterialMasterId;
                $scope.detail.ArticleId = ob.ArticleId;
                $scope.detail.PackingQty = ob.PackingQty;
                $scope.detail.PackingDate = ob.PackingDate;
                $scope.detail.Customer = ob.Customer;
                $scope.detail.Rate = ob.Rate;
                $scope.detail.UomId = ob.UomId;
                $scope.detail.Amount = ob.MaterialQty * ob.Rate;
                LoadPDDetail(ob.SalesOrderPackingListMasterId);
                angular.element(document.querySelector('#plsearchpopup')).modal('hide');
            }
            else {
                throw "This Packinglist:[" + ob.PackingListNo + "] has already been taken in Invoice:[" + ob.InvoiceNo + "]";
            }
        } catch (e) {
            ShowResult(e, 'Error', 'plsearchpopup');
        }
    }


    function IsAvailableInDetail(id, list) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].SalesOrderPackingListMasterId == id) {
                return true;
            }
        }
        return false;
    }
    function IsExistSomm(list, id) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].SalesOrderMaterialMasterId == id) {
                return true;
            }
        }
        return false;
    }
    $scope.CalculateAmount = function (rate, qty, index) {
        $scope.detailList[index].Amount = rate * qty;
    }
    $scope.clearProcessCode = function (id, code) {
        $scope.mastermodal.ProcessId = null;
        $scope.mastermodal.Process = null;
    };
    $scope.GetMasterIndex = function (id) {
        //$scope.masterindex = index;
        //$scope.master = $scope.masterList[$scope.masterindex];
        //console.log($scope.master);
        $scope.getMasterData(id);
        $scope.getDetailData(id);
        //$scope.btnDetailEntryPopup = true;
        // $scope.bulletinmastermodal = $scope.bulletinmasterList[$scope.masterindex];
        angular.element(document.querySelector('#mastersearchpopup')).modal('hide');
    };
    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue == null || fieldValue == '') {
                throw ('[' + fieldName + '] is required...')
            }
        } catch (e) {
            throw e;
        }
    }
    function CheckFieldTime(fieldValue, fieldName) {
        try {
            CheckField(fieldValue, fieldName);
            if (fieldValue.length !== 5) {
                throw fieldName + ' is not correct format...Ex: 08:00, 15:30 (HH:mm)';
            }
            if (fieldValue.substr(2, 1) !== ':') {
                throw fieldName + ' is not correct format...Ex: 08:00, 15:30 (HH:mm)';
            }
            var a = parseInt(fieldValue.substr(0, 2));
            if (a > 23) {
                throw fieldName + ' can not be greater than 23...';
            }
            if (a < 0) {
                throw fieldName + ' can not be negetive...';
            }
            var b = parseInt(fieldValue.substr(3, 2));
            if (b > 59) {
                throw fieldName + ' can not be greater than 59...';
            }
            if (b < 0) {
                throw fieldName + ' can not be negetive...';
            }

            if (a == 0 && b == 0) {
                throw fieldName + ' can not be blank...';
            }
            //first 2 digit check integer
            //last 2 digit check integer
        } catch (e) {
            throw e;
        }
    }
    function ValidationMaster() {
        try {
            //check PORecipeTag
            CheckField($scope.mastermodal.Code, 'Code');
            CheckField($scope.mastermodal.UserName, 'UserName');
            CheckField($scope.mastermodal.ProcessId, 'Process');
            CheckField($scope.mastermodal.BatchSize, 'BatchSize');

            if ($scope.mastermodal.Characteristics1Selected) {
                CheckField($scope.mastermodal.Characteristics1ValueId, $scope.mastermodal.Characteristics1);
            }
            if ($scope.mastermodal.Characteristics2Selected) {
                CheckField($scope.mastermodal.Characteristics2ValueId, $scope.mastermodal.Characteristics2);
            }
            if ($scope.mastermodal.Characteristics3Selected) {
                CheckField($scope.mastermodal.Characteristics3ValueId, $scope.mastermodal.Characteristics3);
            }
        } catch (e) {
            throw e;
        }
    }
    function ValidationDetail() {
        try {
            //CheckField($scope.master.Id, 'Recipe Master');
            //CheckField($scope.detailmodal.SubprocessId, 'Subprocess');
            //CheckField($scope.detailmodal.Duration, 'Duration');
            //CheckField($scope.detailmodal.SectionId, 'Section');
            //CheckField($scope.detailmodal.SubsectionId, 'Subsection');
            //CheckField($scope.detailmodal.LineId, 'Line');

            //CheckDuplicateSubprocess($scope.detailmodal);
        } catch (e) {
            throw e;
        }
    }
    ///**************************************************save delete and clear function*********************************
    function getDetailSaveData() {
        $scope.SalesOrderInvoicePackingListOb = {};
        $scope.SalesOrderInvoicePackingListOb.Id = null;
        $scope.SalesOrderInvoicePackingListOb.SalesOrderInvoiceMasterId = $scope.fileNew.Id;
        $scope.SalesOrderInvoicePackingListOb.SalesOrderPackingListMasterId = $scope.detail.SalesOrderPackingListMasterId;
        $scope.SalesOrderInvoiceDetailOb = {};
        $scope.SalesOrderInvoiceDetailOb.Id = null;
        $scope.SalesOrderInvoiceDetailOb.MaterialMasterId = $scope.detail.MaterialMasterId;
        $scope.SalesOrderInvoiceDetailOb.ArticleId = $scope.detail.ArticleId;
        $scope.SalesOrderInvoiceDetailOb.UomId = $scope.detail.UomId;
        $scope.SalesOrderInvoiceDetailOb.SalesOrderInvoicePackingListId = $scope.SalesOrderInvoicePackingListOb.Id;
        $scope.SalesOrderInvoiceDetailOb.SalesOrderInvoiceMasterId = $scope.fileNew.Id;
        $scope.SalesOrderInvoiceDetailOb.SalesOrderPackingListMasterId = $scope.detail.SalesOrderPackingListMasterId;
        $scope.SalesOrderInvoiceDetailOb.SalesOrderPackingListMaterialId = $scope.detail.SalesOrderPackingListMaterialId;
        $scope.SalesOrderInvoiceDetailOb.Qty = $scope.detail.PackingQty;
        $scope.SalesOrderInvoiceDetailOb.Rate = $scope.detail.Rate;
        $scope.SalesOrderInvoiceDetailOb.Amount = $scope.detail.Amount;
    }
    $scope.SaveMaster = function () {
        try {
            // ValidationMaster();
            $scope.file = Object.assign({}, $scope.fileNew);
            $scope.SaveMasterDisabled = true;
            $http({
                method: 'POST',
                url: $scope.path + "create",
                dataType: 'JSON',
                data: { 'master': $scope.file }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    $scope.SaveMasterDisabled = false;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //get data by id
                    //$scope.getMasterData(response.data.id)
                    $scope.getData();
                    $scope.fileNew.Id = response.data.id;
                    $scope.SaveMasterDisabled = false;
                }
            }, function errorCallback(response) {
                $scope.SaveMasterDisabled = false;
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            $scope.SaveMasterDisabled = false;
            ShowResult(e, 'failure');
        }
    }
    $scope.SaveDetail = function () {
        try {
            // ValidationDetail();
            //$scope.detailmodal.RecipeMasterID = $scope.master.Id;
            //$scope.detailmodal.MaterialMasterId = $scope.master.MaterialMasterId;
            //for (var i in $scope.detailList) {
            //    $scope.detailList[i].SalesOrderInvoiceMasterId = $scope.master.Id;
            //}
            //console.log('444', $scope.detailList);

            //$scope.detail.SalesOrderInvoiceMasterId = $scope.master.Id;
            //for (var i = 0; i < baseService.arrayLength($scope.pdList); i++) {
            //    $scope.pdList[i].SalesOrderInvoiceMasterId = $scope.master.Id;
            //    // $scope.pdList[i].SalesOrderInvoicePackingListId = $scope.detail.Id;
            //}
            getDetailSaveData();
            $scope.SaveDetailDisabled = true;
            $http({
                method: 'POST',
                url: $scope.path + "createdetail",
                dataType: 'JSON',
                data: { 'masterid': $scope.fileNew.Id, 'packing': $scope.SalesOrderInvoicePackingListOb, 'detail': $scope.SalesOrderInvoiceDetailOb }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    $scope.SaveDetailDisabled = false;
                    ShowResult(response.data.Message, 'failure', 'plentrypopup');
                }
                else {
                    angular.element(document.querySelector('#plentrypopup')).modal('hide');
                    ShowResult(response.data.Message, 'success', 'plentrypopup');
                    getInvoiceMaster($scope.fileNew.Id);
                    LoadPackingListHead($scope.fileNew.Id);

                    $scope.SaveDetailDisabled = false;
                }
            }, function errorCallback(response) {
                $scope.SaveDetailDisabled = false;
                ShowResult(response.status.Message, 'failure', 'plentrypopup');
            });
            return true;
        } catch (e) {
            $scope.SaveDetailDisabled = false;
            ShowResult(e, 'Error', 'plentrypopup');
        }
    }
    $scope.DeleteMaster = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.master.Id)) {
                throw "Select an Invoice...";
            }
            $http({
                method: 'POST',
                url: $scope.path + "delete",
                dataType: 'JSON',
                data: { 'masterid': $scope.master.Id }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.clear();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.DeleteDetail = function () {
        //console.log('m',$scope.master.Id);
        //console.log('d',$scope.detail.Id);
        $http({
            method: 'POST',
            url: $scope.path + "DeleteDetailSingle/",
            dataType: 'JSON',
            data: { 'masterid': $scope.master.Id, 'detailid': $scope.detail.Id }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                getInvoiceMaster($scope.master.Id);
                LoadPackingListHead($scope.master.Id);
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    }

    function ClearDetail() {
        //ClearObject($scope.detailmodal);
        $scope.detailList = [];
        ClearObject($scope.detail);
        $scope.gridDetailGrid = false;
        $scope.btnDetailEntryPopup = false;
        ClearDetailModal();
        ClearDetailChild();
    }
    function ClearDetailModal() {
        ClearObject($scope.detailmodal);
        $scope.SaveDetailDisabled = false;
        $scope.ActionDetail = 'Save'
        $scope.subProcessList = [];
    }

    ///common function ends-------------------------------------------------------------------------------------------------
    ///**************************************************show modal*********************************

    $scope.masterSearchPopup = function () {
        $scope.getMasterData();
        angular.element(document.querySelector('#mastersearchpopup')).modal('show');
    };
    $scope.fileSearchPopup = function () {
        $scope.getFileData();
        angular.element(document.querySelector('#filesearchpopup')).modal('show');
    };
    $scope.plSearchPopup = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.fileNew.EntityId)) {
                throw "Select entity.";
            }

            if (baseService.isUndefinedOrNull($scope.fileNew.CustomerId)) {
                throw "Select customer.";
            }
            $scope.getPLData();
            angular.element(document.querySelector('#plsearchpopup')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.plEditPopup = function (ob) {
        try {
            if (baseService.isUndefinedOrNull(ob.Id)) {
                throw "Invoice PackingList Master can not be blank...";
            }
            console.log('***', ob);
            CopyObject($scope.detail, ob);
            LoadDetailEdit(ob.Id);//SalesOrderInvoicePackingListId

            angular.element(document.querySelector('#plentrypopup')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.plEntryPopup = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.fileNew.CustomerId)) {
                throw "Select Customer.";
            }
            $scope.pdList = [];
            angular.element(document.querySelector('#plentrypopup')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.skuFormPopup = function (ob) {
        detailOb(ob);
        //load already saved detaillist by ob.Id
        $scope.getCharSavedList(ob.SalesOrderMaterialMasterId);
        angular.element(document.querySelector('#charentrypopup')).modal('show');
    };
    $scope.charSearchPopup = function () {
        $scope.getCharData();
        angular.element(document.querySelector('#charsearchpopup')).modal('show');
    };
    $scope.customerPopup = function () {
        if (baseService.isUndefinedOrNull($scope.master.SalesOrganizationId)) {
            ShowResult("Select 'Sales Organisation'.....", 'failure');
        }
        else {
            $scope.getcustomerSearchData();
            angular.element(document.querySelector('#customersearchpopup')).modal('show');
        }
    };

    $scope.searchCharacteristics3Value = function (cvid) {
        $scope.dim = "3";
        $scope.getCharacteristicsValueData(cvid);
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };
    $scope.searchCharacteristics2Value = function (cvid) {
        $scope.dim = "2";
        $scope.getCharacteristicsValueData(cvid);
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };
    $scope.searchCharacteristics1Value = function (cvid) {
        $scope.dim = "1";
        $scope.getCharacteristicsValueData(cvid);
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };

    $scope.Delete = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.master.Id)) {
                throw "Select an Invoice...";
            }
            var _id = $scope.master.InvoiceNo;
            $scope.message_confirmation = "Are you sure to delete [" + _id + "] ";
            angular.element(document.querySelector('#confirmmasterdelete')).modal('show');
            //$rootScope.passValue(_id, $scope.masterindex);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.removeMasterYes = function () {
        angular.element(document.querySelector('#confirmmasterdelete')).modal('hide');
        $scope.DeleteMaster();
    };
    $scope.removeRowYes = function () {
        $scope.DeleteDetail();
        angular.element(document.querySelector('#detailentrypopup')).modal('hide');
    };
    //
    $scope.deleteDetailGrid = function (ob) {
        //$scope.detailid_delete = id;
        //$scope.message_confirmation = "Are you sure to delete [" + id + "] ";

        $scope.detail.Id = ob.Id;
        $scope.message_confirmation = "Are you sure to delete [" + ob.PackingListNo + "] ";
        angular.element(document.querySelector('#confirmdetaildelete')).modal('show');
    }
    $scope.removeRowDetailYes = function () {
        $scope.DeleteDetail();
        angular.element(document.querySelector('#confirmdetaildelete')).modal('hide');
    };

    $scope.deleteDetailChildGrid = function (id) {
        $scope.detailchildmodal.Id = id;
        $scope.message_confirmation = "Are you sure to delete [" + id + "] ";
        angular.element(document.querySelector('#confirmdetailchilddelete')).modal('show');
    }
    $scope.removeRowDetailChildYes = function () {
        $scope.DeleteDetailChild();
        angular.element(document.querySelector('#confirmdetailchilddelete')).modal('hide');
    };

    //For Detail
    $scope.getDetailRow = function (id) {
        $scope.detailEntryPopup('EDIT');
        // $scope.detailindex = index;
        //$scope.detail = $scope.detailList[$scope.detailindex];
        $scope.getDetailEditData(id);
    }
    $scope.getDetailChildRow = function (index) {
        $scope.detailChildEntryPopup('EDIT');
        $scope.detailchildmodal = $scope.detailChildList[index];
    }

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
    });

    $http.get($scope.path + "getsalestype/")
        .then(function (response) {
            $scope.salestypeList = [];
            $scope.salestypeList = response.data;
        });
    $http.get($scope.path + "loadCustomerPaymentTerm/")
        .then(function (response) {
            $scope.paymentTermList = [];
            $scope.paymentTermList = response.data;
        });
    $scope.addNewShow = false;

    $scope.GetBaseLineDateSetting = function (paymentterm) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBaseLineDateSetting/',
            params: { PaymentTermId: paymentterm }
        }).then(function successCallback(response) {
            //$scope.paymentTermList = [];
            //$scope.paymentTermList = response.data;
            $scope.fileNew.BaseNoOfDays = response.data[0].NoOfDay;
        });
    }
    $scope.Get = function (id, index) {
        $scope.fileNew = Object.assign({}, $scope.salesOrderInvoiceList[index]);
        $scope.LoadSalesGroup($scope.fileNew.SalesOrganizationId);
        getInvoiceMaster($scope.fileNew.Id);
        LoadPackingListHead($scope.fileNew.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
}
