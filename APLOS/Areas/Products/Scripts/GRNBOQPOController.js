'use strict';
GRNBOQPOController.$inject = ['addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function GRNBOQPOController(addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "GRN BOQ PO"; //Inventory Receive
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/GoodsReceiveNote/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getListUrl1 = $scope.path + 'GetListForMasterData';
    $scope.getListUrl2 = $scope.path + 'GetListForMasterData2';

    $scope.saveUrl = $scope.path + 'CreatePOGRNBOQ';
    $scope.updateUrl1 = $scope.path + 'UpdateGRNBOQPO';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'deleteGRNBYPO/';
    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete?receiveDetailId=';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.updateUrlForSRValue = $scope.path + 'UpdateShortageRejectionValueMap';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $scope.soList = [];
    $scope.GetListForMasterOrder = [];
    $scope.POMaterialTaxList = [];
    $scope.POServiceTaxList = [];
    $scope.PurchaseOrderFileLocation = virtualPath.GRN;
    $scope.partyType = 'Vendor';
    $scope.isAdvance = false;
    $scope.currentDate = new Date(Date.now());
    $scope.grossTotal = 0;
    $scope.chargesList = [];
    $scope.chargesListPO = [];
    $scope.storageList = [];
    $scope.currencyList = [];
    $scope.detailModelSave = [];
    $scope.inventoryMaterialListPOnew = [];
    $scope.chargesListPOnew = [];
    $scope.partyList = [];
    $scope.storageList = [];
    $scope.currencyList = [];
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.product = {
        Id: null
        , GRNDate: $filter("dateFiltering")(Date.now())
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: $window.plantId
        , PartyId: null
        , InvoicingPartyPlantId: null
        , InvoicingByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryPartyPlantId: null
        , DeliveryByAddress: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , CutOffDate: null
        , MaterialStorageId: null
        , CurrencyId: null
        , BaseCurrencyId: $scope.baseCurrencyId
        , ToCurrencyRate: 0
        , PaymentTermId: null
        , BaseOnDueDate: null
        , BaseNoOfDays: null
        , MatureDate: null
        , DocRefNo: null
        , DocDate: null
        , GateEntryNo: null
        , EntryDate: null
        , FixedAssetOrInventory: 'Inventory'
        , PODepended: false
        , AlongwithInvoice: true
        , InvoiceNo: null
        , InvoiceDate: null
        , IsNonCreditable: false
        , TaxApplicable: null
        , IsTaxApplicable: false
        , IsTaxApplicableChangeable: false
        , PartyType: $scope.partyType
        , POId: null
        , IsApproved: 0
        , CheckedBy: null
        , CheckedByStatus: null
        , AuthorizedBy: null
        , AuthorizedByStatus: null
        , NoteForAccounts: null
        , AcceptanceDate: null
        , PurchaseDocumentAcceptanceId: null
        , VoucherId: null
        , InvoiceNo: null
        , InvoiceDate: null
        , DueDate: null
        , PurchaseLCId: null
        , ContractId: null
        , ContractNo: null
        , AcceptancePaymentSource: null
        , LCDate: null
        , PO: null
        , labelCheckAndApproved: null
        , CheckedByStatusForNoti: null
        , ApprovedByStatusForNoti: null
        , TaxOptionAddiTax: 'Yes'
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionService: 'Yes'
        , TaxOptionServiceModify: 'Yes'
        , TaxOptionService1: 'Yes'
        , msgForAllocationNeed: null
    };
    $scope.productNew = Object.assign({}, $scope.product);

    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

    $scope.cboParallelCurrency = function () {
        $http({
            method: 'GET',
            url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
        }).then(function successCallback(response) {
            $scope.baseCurrencyId = response.data[0].Value;
            $scope.productNew.BaseCurrencyId = response.data[0].Value;
            factoryService.getCurrencyPrecision($scope.baseCurrencyId);
        });
    }
    $scope.cboParallelCurrency();

    $scope.showPartyPopUpNew = function () {
        if ($scope.partyType === 'Vendor') {
            $scope.partyUrl = 'Products/GoodsReceiveNote/GetGRNBOQPartyListNew?partyType=' + $scope.partyType;
        }
        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#partyPopUp')).modal('show');
    };

    $scope.closePartyPopUpNew = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
    };
    $scope.hidePartyPopUp = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };

    $scope.NotificationSettingStatus = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/NotificationSetting',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NotificationSetting = response.data;
            $scope.CheckedByStatusForNoti = $scope.NotificationSetting[0].RequiredChecking;
            $scope.ApprovedByStatusForNoti = $scope.NotificationSetting[0].RequiredApproval;
            $scope.GetCheckedByAndApprovedBy1();
            if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === false) {
                $scope.productNew.labelCheckAndApproved = 'To be checked by';
            }
            else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be approved by';
            }
            else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be checked by';
            }

        });
    }
    $scope.NotificationSettingStatus();
    $scope.GetCheckedByAndApprovedBy1 = function () {
        if (!baseService.isUndefinedOrNull($scope.CheckedByStatusForNoti) && !baseService.isUndefinedOrNull($scope.ApprovedByStatusForNoti)) {
            $http({
                method: 'GET',
                url: 'Products/InventoryReceive/GetCheckedByAndApprovedBY?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.checkedByList = response.data;
            });
        }
    }
    $scope.plantList = [];
    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.productNew.PartyCode = party.Code;
        $scope.productNew.PartyName = party.UserName;
        $scope.productNew.PartyId = party.Id;
        $scope.productNew.PaymentTermId = party.PaymentTermId;
        $scope.productNew.CurrencyId = party.CurrencyId;

        getPartyPlantListBOQ();
        $scope.GetCurrencyExchangeRateList();
        $scope.hidePartyPopUp();
    };

    function getPartyPlantListBOQ() {
        $scope.plantList = [];
        $http.get('Products/PurchaseOrder/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId + '&Id=' + $scope.Id).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.IsDefault) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = item.Value;
                    $scope.productNew.InvoicingByAddress = item.Address1;
                    $scope.productNew.DeliveryByAddress = item.Address2;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = item.StateName;
                    $scope.productNew.DeliveryGSTIN = item.GSTIN;
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
            var address = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateName;
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = state;
                $scope.productNew.InvoicingGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.productNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.productNew.DeliveryState = state;
                $scope.productNew.DeliveryGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.productNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = null;
                $scope.productNew.InvoicingGSTIN = null;
                return $scope.productNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.productNew.DeliveryState = null;
                $scope.productNew.DeliveryGSTIN = null;
                return $scope.productNew.DeliveryByAddress = null;
            }
        }
    };


    $scope.GetCurrencyExchangeRateList = function () {

        //if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
        if (!baseService.isUndefinedOrNull(!baseService.isUndefinedOrNull($scope.productNew.CurrencyId))) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.productNew.DocDate + "&currencyId=" + $scope.productNew.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.productNew.ToCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };
    $scope.tab1 = 1;

    $scope.setTabGRNBOQList = function (newTab12) {
        $scope.GriddataSelected = [];
        $scope.Clear();
        $scope.tab1 = newTab12;
    };

    $scope.isSetGRNBOQList = function (tabNum12) {
        return $scope.tab1 === tabNum12;
    };

    $scope.itemList = [];
    $scope.GetItemVendor = function () {
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetItemListByVendor?vendorId=' + $scope.productNew.PartyId,
        }).then(function successCallback(response) {
            $scope.itemList = response.data;
            for (var i = 0; i < $scope.GriddataSelected.length; i++) {
                for (var j = 0; j < $scope.itemList.length; j++) {
                    if ($scope.GriddataSelected[i].MaterialMasterId == $scope.itemList[j].MaterialMasterId
                        && $scope.GriddataSelected[i].ArticleId == $scope.itemList[j].ArticleId
                        && $scope.GriddataSelected[i].VendorRefNo == $scope.itemList[j].VendorRefNo
                        && $scope.GriddataSelected[i].CustomerRefNo == $scope.itemList[j].CustomerRefNo
                        && $scope.GriddataSelected[i].OwnReferenceNo == $scope.itemList[j].OwnReferenceNo) {
                        $scope.itemList[j].Active = true;
                    }
                }
            }
        });
        angular.element(document.querySelector('#itemPopUp')).modal('show');
    };

    $scope.closeItemPopUp = function () {
        angular.element(document.querySelector('#itemPopUp')).modal('hide');
    };

    var getString = function (data, column) {
        var kk = "";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) === false) {
                if (kk === "") {
                    kk += "'" + data[i][column] + "'";
                }
                else {
                    kk += ",'" + data[i][column] + "'";
                }

                collection.push(data[i][column]);
            }
        }
        return kk;
    };

    $scope.MasterList = [];
    $scope.DetailList = [];
    $scope.GriddataSelected = [];
    $scope.recorddoubleclick = function () {
        try {
            $scope.GriddataSelected = [];
            for (var i = 0; i < $scope.itemList.length; i++) {
                if ($scope.itemList[i].Active) {
                    $scope.GriddataSelected.push($scope.itemList[i]);
                }
            }
            if ($scope.GriddataSelected.length == 0) {
                throw "Select atleast one item.."
            }
            $scope.GetDetails();
            angular.element(document.querySelector('#itemPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'info');
        }
    }
    $scope.MaterialMasterId = null;
    $scope.ArticleId = null;
    $scope.GetDetails = function () {
        try {
            $scope.MaterialMasterId = null;
            $scope.ArticleId = null;
            var parameters = [];
            var gridObj = $("#GriddataSelected").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.GriddataSelected;
            }
            parameters.push({ "Key": "MaterialId", "Value": getString(filteredRecords, "MaterialMasterId") });
            parameters.push({ "Key": "ArticleId", "Value": getString(filteredRecords, "ArticleId") });
            parameters.push({ "Key": "VendorRefNo", "Value": getString(filteredRecords, "VendorRefNo") });
            parameters.push({ "Key": "CustomerRefNo", "Value": getString(filteredRecords, "CustomerRefNo") });
            parameters.push({ "Key": "OwnReferenceNo", "Value": getString(filteredRecords, "OwnReferenceNo") });

            var MaterialIds = parameters[0].Value;
            $scope.MaterialMasterId = MaterialIds;
            var ArticleIds = parameters[1].Value;
            $scope.ArticleId = ArticleIds;
            var VendorRefNos = parameters[2].Value;
            var CustomerRefNos = parameters[3].Value;
            var OwnReferenceNo = parameters[4].Value;

            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Products/GoodsReceiveNote/GetItemListDetailsByList',
                data: {
                    'MaterialIds': MaterialIds,
                    'ArticleIds': ArticleIds,
                    'VendorRefNos': VendorRefNos,
                    'CustomerRefNos': CustomerRefNos,
                    'OwnReferenceNo': OwnReferenceNo,
                    'PartyId': $scope.productNew.PartyId
                }
            }).then(function successCallback(response) {
                $scope.DetailList = response.data;
            });

        } catch (e) {
            ShowResult(e, 'info')
        }
    }
    $scope.getDetailsData = function () {
        try {
            //var eDialog = $("#GRnBOQPoo").data("ejDialog");
            //$("#GRnBOQPoo").ejDialog("setTitle", " BOQ List");
            //eDialog.open();
            $scope.getDetailsDataNew();

        } catch (e) {
            ShowResult(e, 'info')
        }
    }
    $scope.MasterListNewBOQ = [];
    $scope.getDetailsDataNew = function () {
        var parameters = [];
        var filteredRecords = [];
        var currencyStatus = true;
        var temCurrency = null;
        $scope.MasterListNewBOQ = [];
        for (var i = 0; i < $scope.DetailList.length; i++) {
            if ($scope.DetailList[i].IsActives) {
                filteredRecords.push($scope.DetailList[i]);
                if (temCurrency == null) {
                    temCurrency = $scope.DetailList[i].CurrencyId;
                }
                else if (temCurrency != $scope.DetailList[i].CurrencyId) {
                    currencyStatus = false;
                    throw ('PO must be same currency!!!');
                }
            }
        }
        $scope.productNew.CurrencyId = temCurrency;
        $scope.getToCurrencyRate();

        parameters.push({ "Key": "POId", "Value": getString(filteredRecords, "POId") });
        parameters.push({ "Key": "ContractId", "Value": getString(filteredRecords, "ContractId") });
        parameters.push({ "Key": "SalesOrderIds", "Value": getString(filteredRecords, "SalesOrderId") });

        var POId = parameters[0].Value;
        var ContractId = parameters[1].Value;
        var SalesOrderId = parameters[2].Value;
        var masterOrderitemId = parameters[2].Value;
        if (POId != "" && currencyStatus) {
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Products/GoodsReceiveNote/GetPOBOQItemForGRN',
                data: {
                    'POId': POId,
                    'ContractId': ContractId,
                    'masterOrderitemId': masterOrderitemId,
                    'SalesOrderId': SalesOrderId,
                    'MaterialMasterId': $scope.MaterialMasterId,
                    'ArticleId': $scope.ArticleId,
                }
            }).then(function successCallback(response) {
                $scope.MasterListNewBOQ = response.data;
                $scope.GetPOMaterialTaxData(POId);
            });
            angular.element(document.querySelector('#GRnBOQPoo')).modal('show');
        }
        else {
            if (POId == '')
                ShowResult('Please select at least one PO!!!');
        }
    }
    $scope.boqPopClose = function () {
        angular.element(document.querySelector('#GRnBOQPoo')).modal('hide');
    }
    $http({
        method: 'GET',
        url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = result;
    });
    $scope.POMaterialTaxList = [];
    $scope.GetPOMaterialTaxData = function (poid) {
        $scope.POMaterialTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetReceiveTaxListPO?receiveDetailId=' + poid
        }).then(function (response) {
            $scope.POMaterialTaxList = response.data;
        });
    };
    $scope.GetGRNMaterialTaxData = function (grnId) {
        $scope.POMaterialTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + grnId
        }).then(function (response) {
            $scope.POMaterialTaxList = response.data;
            $scope.receiveTaxList = response.data;

            for (var i = 0; i < $scope.MasterList.length; i++) {
                var linepk = $scope.MasterList[i].InventoryReceiveDetailId;
                var list = getPOMaterialtaxlist(linepk);
                $scope.MasterList[i].POMaterialTaxList = list;
            }
            for (var j = 0; j < length; j++) {

            }
        });
    };

    function getPOMaterialtaxlist(linepk) {
        var result = [];
        for (var i = 0; i < $scope.POMaterialTaxList.length; i++) {
            if ($scope.POMaterialTaxList[i].InventoryReceiveDetailId === linepk) {
                result.push($scope.POMaterialTaxList[i]);
            }
        }
        return result;
    }

    $scope.AcceptanceId = null;
    $scope.getToCurrencyRate = function () {
        if (baseService.isUndefinedOrNull($scope.productNew.GRNDate)) {
            $scope.productNew.ToCurrencyRate = 1;
            return;
        }
        $http.get($scope.path + 'GetToCurrencyRate?currencyId=' + $scope.productNew.CurrencyId + '&baseCurrencyId=' + $scope.productNew.BaseCurrencyId + '&docDate=' + $filter('dateFiltering')($scope.productNew.GRNDate))
            .then(function (response) {
                if (parseFloat(response.data) === 0)
                    $scope.productNew.ToCurrencyRate = 1;
                else
                    $scope.productNew.ToCurrencyRate = response.data;
            });
    };
    $scope.checkedByList = [];
    $scope.GetSupervisorCboList = function () {
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/GetSupervisorCbo'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
    }
    $scope.GetSupervisorCboList();

    $scope.POPopUpGateEntry = function () {
        $scope.getalldataGateEntry();
        angular.element(document.querySelector('#POPopUpGateEntry')).modal('show');
    };
    $scope.POPopUpCloseGateEntry = function () {
        angular.element(document.querySelector('#POPopUpGateEntry')).modal('hide');
    };
    $scope.GriddataGateEntry = [];
    $scope.getalldataGateEntry = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetListOfPOGateEntry?partyCode=' + $scope.productNew.PartyId,
        }).then(function successCallback(response) {
            $scope.GriddataGateEntry = response.data;
        });
    };
    $scope.recorddoubleclickGateEntry = function ($event) {
        var x = $event;
        var Id = x.data.Id;
        $scope.productNew.GateEntryNo = x.data.Id;
        $scope.productNew.EntryDate = x.data.EntryDate;

        $scope.POPopUpCloseGateEntry();
    }

    $scope.GetPOServiceTaxData = function () {
        //debugger;
        $scope.POServiceTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetServiceTaxListPO?serviceId=' + $scope.inveReveiveId
        }).then(function (response) {
            $scope.POServiceTaxList = response.data;

            for (var i = 0; i < $scope.chargesListPO.length; i++) {
                var linepk = $scope.chargesListPO[i].Id;
                var list1 = getPOServicetaxlist(linepk);
                $scope.chargesListPO[i].POServiceTaxList = list1;
            }
        });
    };
    $scope.receiveTaxList = [];
    $scope.getReceiveTaxListPO = function (data, flag, index, Id) {
        $scope.receiveTaxindex = index;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.receiveTaxList = [];
        if (data.POMaterialTaxList.length > 0) {
            $scope.HSNCode = data.POMaterialTaxList[0].HSNCode;
            $scope.receiveTaxList = data.POMaterialTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    };

    $scope.getReceiveTaxListPOAfterSave = function (data, flag, index, Id) {
        $scope.receiveTaxindex = index;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.GRNDetailRowData = data;
        $scope.receiveTaxList = [];
        if ($scope.POMaterialTaxList.length > 0) {
            $scope.HSNCode = $scope.POMaterialTaxList[0].HSNCode;
            for (var i = 0; i < $scope.POMaterialTaxList.length; i++) {
                if ($scope.productNew.Id == null) {
                    if ($scope.POMaterialTaxList[i].PODetailId == data.PODetailsID) {
                        $scope.receiveTaxList.push($scope.POMaterialTaxList[i])
                    }
                } else {
                    if ($scope.POMaterialTaxList[i].PODetailId == data.InventoryReceiveDetailId) {
                        $scope.receiveTaxList.push($scope.POMaterialTaxList[i])
                    }
                }
            }
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    };


    $scope.Clear = function () {
        ClearFields();
        return true;
        $scope.PostButton = false;

    };

    function ClearFields() {
        $scope.SaveButtonDisable = "";
        $scope.Action = "Save";
        $scope.IsBaseOnDueDateEnable = false;
        $scope.productNew = {
            FixedAssetOrInventory: 'Inventory'
            , PODepended: false
            , AlongwithInvoice: true
            , IsNonCreditable: false
            , BaseCurrencyId: $scope.baseCurrencyId
            , ToCurrencyRate: 1
            , TaxApplicable: null
            , IsTaxApplicable: false
            , IsTaxApplicableChangeable: false
            , PartyType: $scope.partyType
            , PlantId: $window.plantId
            , GRNDate: $filter("dateFiltering")(Date.now())

        };
        $scope.DetailList = [];
        $scope.GriddataSelected = [];
        $scope.MasterList = [];
        $scope.POMaterialTaxList = [];
        $scope.POServiceTaxList = [];
        $scope.MasterListNewBOQ = [];
        $scope.chargesListPOnew = [];
        $scope.ApprovedByStatusForNoti = null;
        $scope.CheckedByStatusForNoti = null;
        $scope.NotificationSettingStatus();
        $scope.AcceptanceId = null;
        $scope.PostButton = false;
        $scope.advanceTaxesList = [];
        $scope.productDocMap = {
            UserFilename: null
            , Description: null
            , Remarks: null
        };

        $scope.Imagedata = [];
    }

    $scope.PostButton = false;
    $scope.Save = function () {
        if ($scope.Action === 'Save') {

            /*if (!$scope.checkValidation()) {*/

            try {
                if ($scope.Action === 'Update') {
                    $scope.modelValidation('div_grnNo', 'productNew', 'Id');
                    $scope.modelValidation('div_grnDate', 'productNew', 'GRNDate');
                    $scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');
                    if (baseService.isUndefinedOrNull($scope.productNew.DocRefNo)) {
                        throw ("Enter Doc Ref No", 'failure');
                        return false;
                    }
                    if (baseService.isUndefinedOrNull($scope.productNew.DocRefNo)) {
                        throw ("Enter Doc Ref No", 'failure');
                        return false;
                    }
                    if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
                        throw ("Enter Doc Date", 'failure');
                        return false;
                    }
                    if (baseService.isUndefinedOrNull($scope.productNew.GateEntryNo)) {
                        throw ("Select Gate Entry No", 'failure');
                        return false;
                    }
                    if (baseService.isUndefinedOrNull($scope.productNew.EntryDate)) {
                        throw ("Enter Gate Entry Date", 'failure');
                        return false;
                    }
                    if (baseService.isUndefinedOrNull($scope.productNew.GRNDate)) {
                        throw ("Enter GRN Date", 'failure');
                        return false;
                    }
                    if (baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
                        throw ("Please select Check By", 'failure');
                        return false;
                    }
                }

                $scope.$broadcast('show-errors-check-validity');
                if ($scope.productNewForm.$valid) {
                    if ($scope.Action === "Save") {
                        if (new Date($scope.productNew.EntryDate) < new Date($scope.productNew.DocDate)) {
                            return manualValidation('div_entryDate', true, "Gate entry date can't be less than Doc Date");
                        }

                        else if (new Date($scope.productNew.GRNDate) < new Date($scope.productNew.EntryDate)) {
                            return manualValidation('div_grnDate', true, "GRN date can't be less than gate entry date");

                        }
                        else {
                            manualValidation('div_grnDate', false);
                            manualValidation('div_entryDate', false);
                            manualValidation('div_rate', false);
                            $scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
                            $scope.modelValidation('div_docDate', 'productNew', 'DocDate');
                            $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                            $scope.product = Object.assign({}, $scope.productNew);
                            $scope.product.POId = $scope.POId;
                            $scope.product.PurchaseDocumentAcceptanceId = $scope.AcceptanceId;

                            //$scope.productNew.ToCurrencyRate = 1;
                            //$scope.product.ToCurrencyRate = 1;
                            var CheckList = [];
                            for (var i = 0; i < $scope.MasterList.length; i++) {
                                if ($scope.MasterList[i].check) {
                                    CheckList.push($scope.MasterList[i]);
                                }
                            }
                            var CheckNewBOQList = [];
                            for (var i = 0; i < $scope.MasterListNewBOQ.length; i++) {
                                if ($scope.MasterListNewBOQ[i].TransactionQty > 0) {
                                    CheckNewBOQList.push($scope.MasterListNewBOQ[i]);
                                }
                            }
                            $scope.PostButton = true;
                            //debugger;
                            $http({
                                method: 'POST',
                                url: $scope.saveUrl,
                                data:
                                {
                                    'entity': $scope.product,
                                    'entityMatAndImat': JSON.stringify(CheckList),
                                    'receiveTaxList': $scope.NewPOMaterialTaxList,
                                    'chargesListPO': $scope.chargesListPOnew,
                                    'POServiceTaxList': $scope.POServiceTaxList,
                                    'GRNType': 'GRNBYBOQ',
                                    'AcceptanceId': $scope.AcceptanceId,
                                    'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                                    'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                                    , 'BOQAllocation': JSON.stringify(CheckNewBOQList)
                                },
                                dataType: 'JSON'
                                , contentType: "application/json charset=utf-8"
                            }).then(function (response) {
                                if (response.data.Error === true) {
                                    ShowResult(response.data.Message, 'failure');
                                }
                                else {
                                    ShowResult(response.data.Message, 'success');
                                    $scope.SaveButtonDisable = true;
                                    $scope.setTabGRNList(1);

                                    $scope.productNew.Id = response.data.entity.Id;
                                    $scope.productId = response.data.Id;
                                    $scope.productNew.msgForAllocationNeed = response.data.entity.msgForAllocationNeed;
                                    //$scope.Action = 'Update';
                                    $scope.GetGRNMaterialTaxData($scope.productNew.Id);
                                }
                            }), function (response) {
                                ShowResult(response.data.Message, 'failure');
                            };
                        }


                    }

                }
            } catch (e) {
                throw e;
            }
            //}
        }
        else if ($scope.Action === "Update") {


            if (!baseService.isUndefinedOrNull($scope.AcceptanceId) && ($scope.productNew.AcceptanceDate > $scope.productNew.GRNDate)) {
                ShowResult("Acceptance Date  can not grather than GRN Date", 'failure');
                return false;
            }
            else if (!baseService.isUndefinedOrNull($scope.AcceptanceId) && ($scope.productNew.GRNDate > new Date())) {
                ShowResult("GRN Date  can not grather than Today's Date", 'failure');
                return false;
            }
            else if ($scope.productNew.NoteForAccounts === '' || $scope.productNew.NoteForAccounts === null || $scope.productNew.NoteForAccounts === undefined) {
                ShowResult("Enter Note for accounts", 'failure');
                return false;
            }
            else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
                ShowResult("Please select to be approved by", 'failure');
                return false;
            }
            else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
                ShowResult("Please select to be checked by", 'failure');
                return false;
            }
            else if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) {
                ShowResult('Invoicing by is required', 'failure');
                return false;
            }
            else if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) {
                return ShowResult('Delivery by is required', 'failure');
                return false;
            }
            //else if ($scope.productNew.CurrencyId != $scope.productNew.BaseCurrencyId) {
            //	$scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');

            //}
            else if (new Date($scope.productNew.EntryDate) < new Date($scope.productNew.DocDate)) {
                return manualValidation('div_entryDate', true, "Gate entry date can't be less than Doc Date");
            }
            else if (new Date($scope.productNew.GRNDate) < new Date($scope.productNew.EntryDate)) {
                return manualValidation('div_grnDate', true, "GRN date can't be less than gate entry date");

            }
            else {
                manualValidation('div_grnDate', false);
                manualValidation('div_entryDate', false);
                manualValidation('div_rate', false);
                $scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
                $scope.modelValidation('div_docDate', 'productNew', 'DocDate');
                $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                $scope.product = Object.assign({}, $scope.productNew);
                $scope.product.POId = $scope.POId;
                $scope.product.PurchaseDocumentAcceptanceId = $scope.AcceptanceId;
                var CheckList = [];
                for (var i = 0; i < $scope.MasterList.length; i++) {
                    if ($scope.MasterList[i].check) {
                        CheckList.push($scope.MasterList[i]);
                    }
                }
                for (var i4 = 0; i4 < $scope.chargesList.length; i4++) {
                    if ($scope.chargesList[i4].check == true) {
                        $scope.chargesListPOnew.push($scope.chargesList[i4]);
                    }

                    else {

                    }
                }
                $scope.SaveButton = true;

                $http({
                    method: 'POST',
                    url: $scope.updateUrl1,
                    data:
                    {
                        'entity': $scope.product,
                        'entityMatAndImat': CheckList,
                        'receiveTaxList': $scope.POMaterialTaxList,
                        'chargesListPO': $scope.chargesListPOnew,
                        'POServiceTaxList': $scope.ServiceTaxList,
                        'GRNType': 'GRNBYBOQ',
                        'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                        'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti,
                        'BOQAllocation': JSON.stringify($scope.MasterListNewBOQ)
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.setTabGRNList(1);
                        $scope.GRNListDetails();

                        $scope.productId = response.data.entity.Id;
                        $scope.productNew.Id = response.data.entity.Id;
                        $scope.productNew.msgForAllocationNeed = response.data.entity.msgForAllocationNeed;
                        //$scope.SaveButton = true;
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });

            }
        }
    };
    $scope.checkgridcheckornot = [];
    $scope.checkValidation = function () {
        $scope.checkgridcheckornot = $filter("filter")($scope.inventoryMaterialList, { check: true });

        if ($scope.checkgridcheckornot.length === 0) {
            ShowResult("Enter atleast one material information", 'failure');
            return true;
        }

        if (baseService.isUndefinedOrNull($scope.productNew.DocRefNo)) {
            ShowResult("Enter Doc Ref No", 'failure');
            return true;
        }

        if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
            ShowResult("Enter Doc Date", 'failure');
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.productNew.GateEntryNo)) {
            ShowResult("Select Gate Entry No", 'failure');
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.productNew.EntryDate)) {
            ShowResult("Enter Gate Entry Date", 'failure');
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.productNew.GRNDate)) {
            ShowResult("Enter GRN Date", 'failure');
            return true;
        }

        if (!baseService.isUndefinedOrNull($scope.AcceptanceId) && (new Date($scope.productNew.AcceptanceDate) > new Date($scope.productNew.GRNDate))) {
            ShowResult("Acceptance Date  can not grather than GRN Date", 'failure');
            return true;
        }
        else if (!baseService.isUndefinedOrNull($scope.AcceptanceId) && ($scope.productNew.GRNDate > new Date())) {
            ShowResult("GRN Date  can not grather than Today's Date", 'failure');
            return true;
        }
        else if ($scope.productNew.NoteForAccounts === '' || $scope.productNew.NoteForAccounts === null || $scope.productNew.NoteForAccounts === undefined) {
            ShowResult("Enter Note for accounts", 'failure');
            return true;
        }
        else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
            ShowResult("Please select to be approved by", 'failure');
            return true;
        }
        else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
            ShowResult("Please select to be checked by", 'failure');
            return true;
        }
        else if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) {
            ShowResult('Invoicing by is required', 'failure');
            return true;
        }
        else if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) {
            ShowResult('Delivery by is required', 'failure');
            return true;
        }
        $scope.inventoryMaterialListPOnew = [];
        $scope.chargesListPOnew = [];
        for (var i = 0; i < $scope.MasterList.length; i++) {
            if ($scope.MasterList[i].TransactionQty > 0 && $scope.MasterList[i].check == null) {
                ShowResult("Please check in PORowId " + $scope.MasterList[i].InventoryReceiveDetailId, 'failure');
                return true;
            }
            if ($scope.MasterList[i].check == true) {
                if (baseService.isUndefinedOrNull($scope.MasterList[i].MaterialStorageId)) {
                    ShowResult("Please select storage location in PORowId" + $scope.MasterList[i].InventoryReceiveDetailId, 'failure');
                    return true;
                }
                else if (baseService.isUndefinedOrNull($scope.MasterList[i].QualityStatus)) {
                    ShowResult("Please select quality statusin PORowId" + $scope.MasterList[i].InventoryReceiveDetailId, 'failure');
                    return true;
                }
                $scope.inventoryMaterialListPOnew.push($scope.MasterList[i]);

            }
        }
        if ($scope.chargesListPO.length > 0) {
            for (var i = 0; i < $scope.chargesListPO.length; i++) {
                if ($scope.chargesListPO[i].Amount > 0 && $scope.chargesListPO[i].check == null) {
                    ShowResult("Please check  in  " + $scope.chargesListPO[i].ServiceMasterName, 'failure');
                    return true;
                }
                if ($scope.chargesListPO[i].check == true) {
                    $scope.chargesListPOnew.push($scope.chargesListPO[i]);
                }
            }
        }
        return false;
    }
    $scope.modelValidation = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };
    function removeValidationMsg() {
        CloseModalShowResult();
        $scope.clearCharNames();
        manualValidation('div_mm', false);
        manualValidation('div_ar', false);
        manualValidation('div_qty', false);
        manualValidation('div_qty', false);
        manualValidation('div_rate', false);
    }
    $scope.manualValidationAddRemove = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else if (isNaN($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };
    $scope.calculateAmount = function (data, index) {
        if (baseService.isUndefinedOrNull(data.PurchaseDocumentAcceptanceId)) {

            var count = 0;
            for (var j = 0; j < $scope.MasterList.length; j++) {
                if ($scope.MasterList[j].TransactionQty > 0) {
                    count++;
                }
                else {
                    $scope.MasterList[j].ServiceCharge = 0;
                    $scope.MasterList[j].ServiceTax = 0;
                    $scope.MasterList[j].TrnAmount = 0;
                    $scope.MasterList[j].TotalMaterialTranAmount = 0;
                    $scope.MasterList[j].TotalMaterialTranAmount = 0;
                }
            }
            if (data.POTrnRate < data.TransactionRate) {
                data.TransactionRate = data.POTrnRate;
                ShowResult('Transaction Rate can not grater than PO Transaction Rate!', 'failure');
                return false;
            }

            $scope.PreBal = data.Balance;
            // data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
            data.TrnAmount = parseFloat(data.TransactionQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
            if (data.TrnAmount == 'NaN')
                data.TrnAmount = 0;
            data.TaxAmount = 0;
            data.BaseTaxAmount = 0;
            var TotalServiceAmount = Math.round($filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount') * 100 + Number.EPSILON) / 100;
            var TotalTrnAmount = Math.round($filter('sumByKey')($filter('filter')($scope.MasterList), 'TrnAmount') * 100 + Number.EPSILON) / 100;
            var TotalServiceTaxAmount = Math.round($filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount') * 100 + Number.EPSILON) / 100;
            var tempServiceAmount = 0;
            var tempServiceTaxAmount = 0;
            var newcount = 0;
            for (var i = 0; i < $scope.MasterList.length; i++) {
                if ($scope.MasterList[i].TransactionQty > 0) {
                    newcount++;
                    $scope.MasterList[i].Balance = '';
                    var ToleranceQty = $scope.MasterList[i].POQty * $scope.MasterList[i].Tolerance / 100;
                    var newpoQty = $scope.MasterList[i].POQty + ToleranceQty;
                    if ($scope.MasterList[i].POQty < (parseFloat($scope.MasterList[i].GRNRcvQty + $scope.MasterList[i].TransactionQty).toFixed(2)) && (baseService.isUndefinedOrNull($scope.MasterList[i].Tolerance) || $scope.MasterList[i].Tolerance === 0)) {
                        //$scope.MasterList[i].Balance = $scope.MasterList[i].POQty - ($scope.MasterList[i].GRNRcvQty + $scope.MasterList[i].TransactionQty);
                        $scope.MasterList[i].TransactionQty = '';
                        ShowResult('Current quantity can not grater than balance qty!', 'failure');
                        return false;
                    }

                    else if (newpoQty < (parseFloat($scope.MasterList[i].GRNRcvQty + $scope.MasterList[i].TransactionQty).toFixed(2)) && (!baseService.isUndefinedOrNull($scope.MasterList[i].Tolerance) || $scope.MasterList[i].Tolerance > 0)) {
                        ShowResult('Current quantity can not grater than po qty and Tolerance qty!PO + Tolerance=' + newpoQty, 'failure');
                        return false;
                    }
                    else if ($scope.MasterList[i].ShortageQty > $scope.MasterList[i].TransactionQty) {
                        ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
                        return false;
                    }
                    else if ($scope.MasterList[i].RejectionQty > $scope.MasterList[i].TransactionQty) {
                        ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
                        return false;
                    }
                    else {

                        if ($scope.MasterList[i].PODetailsID == data.PODetailsID) {
                            $scope.MasterList[i].TrnAmount = Math.round(data.TrnAmount * 100 + Number.EPSILON) / 100;
                            angular.forEach(data.POMaterialTaxList, function (item) {
                                item.TaxAmount = Math.round(((data.TrnAmount * item.Percentage) / 100) * 100 + Number.EPSILON) / 100;
                            });

                            //$scope.MasterList[i].BaseTaxAmount = Math.round($filter('sumByKey')($filter('filter')(data.POMaterialTaxList, { "PODetailId": data.PODetailsID }), 'TaxAmount') * 100 + Number.EPSILON) / 100;

                            if (TotalServiceAmount > 0) {
                                //$scope.MasterList[i].BaseTaxAmount = (($scope.MasterList[i].TotalTaxAmount / $scope.MasterList[i].POQty) * $scope.MasterList[i].TransactionQty).toFixed(2);
                                if (count > newcount) {
                                    $scope.MasterList[i].ServiceCharge = Math.round(((TotalServiceAmount / TotalTrnAmount) * $scope.MasterList[i].TrnAmount) * 100 + Number.EPSILON) / 100;
                                    $scope.MasterList[i].ServiceTax = Math.round(((TotalServiceTaxAmount / TotalTrnAmount) * $scope.MasterList[i].TrnAmount) * 100 + Number.EPSILON) / 100;

                                }
                                else if (count == newcount) {
                                    $scope.MasterList[i].ServiceCharge = 0;
                                    tempServiceAmount = Math.round($filter('sumByKey')($filter('filter')($scope.MasterList), 'ServiceCharge') * 100 + Number.EPSILON) / 100;
                                    $scope.MasterList[i].ServiceCharge = Math.round((TotalServiceAmount - tempServiceAmount) * 100 + Number.EPSILON) / 100;
                                    $scope.MasterList[i].ServiceTax = 0;
                                    tempServiceTaxAmount = Math.round($filter('sumByKey')($filter('filter')($scope.MasterList), 'ServiceTax') * 100 + Number.EPSILON) / 100;
                                    $scope.MasterList[i].ServiceTax = Math.round((TotalServiceTaxAmount - tempServiceTaxAmount) * 100 + Number.EPSILON) / 100
                                }

                            }

                            $scope.MasterList[i].Balance = ($scope.MasterList[i].POQty - ($scope.MasterList[i].GRNRcvQty + $scope.MasterList[i].TransactionQty));
                            //$scope.MasterList[i].ShortageQty = ($scope.MasterList[i].POQty - ($scope.MasterList[i].GRNRcvQty + $scope.MasterList[i].TransactionQty));
                            $scope.MasterList[i].ApprovedQty = ($scope.MasterList[i].TransactionQty - ($scope.MasterList[i].ShortageQty + $scope.MasterList[i].RejectionQty));
                            //$scope.MasterList[i].ApprovedQty = ($scope.MasterList[i].TransactionQty - $scope.MasterList[i].RejectionQty);
                            $scope.MasterList[i].NetQty = ($scope.MasterList[i].TransactionQty - $scope.MasterList[i].ShortageQty);

                        }
                        else {
                            if (TotalServiceAmount > 0) {
                                if (count > newcount) {
                                    $scope.MasterList[i].ServiceCharge = Math.round(((TotalServiceAmount / TotalTrnAmount) * $scope.MasterList[i].TrnAmount) * 100 + Number.EPSILON) / 100;
                                    $scope.MasterList[i].ServiceTax = Math.round(((TotalServiceTaxAmount / TotalTrnAmount) * $scope.MasterList[i].TrnAmount) * 100 + Number.EPSILON) / 100;

                                }
                                else if (count == newcount) {
                                    $scope.MasterList[i].ServiceCharge = 0;
                                    tempServiceAmount = Math.round($filter('sumByKey')($filter('filter')($scope.MasterList), 'ServiceCharge') * 100 + Number.EPSILON) / 100;
                                    $scope.MasterList[i].ServiceCharge = Math.round((TotalServiceAmount - tempServiceAmount) * 100 + Number.EPSILON) / 100;
                                    $scope.MasterList[i].ServiceTax = 0;
                                    tempServiceTaxAmount = Math.round($filter('sumByKey')($filter('filter')($scope.MasterList), 'ServiceTax') * 100 + Number.EPSILON) / 100;
                                    $scope.MasterList[i].ServiceTax = Math.round((TotalServiceTaxAmount - tempServiceTaxAmount) * 100 + Number.EPSILON) / 100
                                }

                            }
                            $scope.MasterList[i].Balance = ($scope.MasterList[i].POQty - ($scope.MasterList[i].GRNRcvQty + $scope.MasterList[i].TransactionQty));
                            //$scope.MasterList[i].ShortageQty = ($scope.MasterList[i].POQty - ($scope.MasterList[i].GRNRcvQty+$scope.MasterList[i].TransactionQty));
                            $scope.MasterList[i].ApprovedQty = ($scope.MasterList[i].TransactionQty - ($scope.MasterList[i].ShortageQty + $scope.MasterList[i].RejectionQty));
                            //$scope.MasterList[i].ApprovedQty = ($scope.MasterList[i].TransactionQty - $scope.MasterList[i].RejectionQty);
                            $scope.MasterList[i].NetQty = ($scope.MasterList[i].TransactionQty - $scope.MasterList[i].ShortageQty);
                        }
                        if ($scope.productNew.IsNonCreditable == 1) {
                            $scope.MasterList[i].TrnAmount = ($scope.MasterList[i].NetQty * $scope.MasterList[i].TransactionRate).toFixed(2);
                            $scope.MasterList[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].ServiceTax) + parseFloat($scope.MasterList[i].ServiceCharge) + parseFloat($scope.MasterList[i].BaseTaxAmount)) * 100 + Number.EPSILON) / 100;
                            $scope.MasterList[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].ServiceTax) + parseFloat($scope.MasterList[i].ServiceCharge) + parseFloat($scope.MasterList[i].BaseTaxAmount)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;

                        }
                        else {
                            $scope.MasterList[i].TrnAmount = Math.round(($scope.MasterList[i].NetQty * $scope.MasterList[i].TransactionRate) * 100 + Number.EPSILON) / 100;
                            $scope.MasterList[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].ServiceCharge)) * 100 + Number.EPSILON) / 100;
                            $scope.MasterList[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
                        }
                    }

                }
            }


        }
        else {
            $scope.PreBal = data.Balance;
            // data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
            data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
            if (data.TrnAmount == 'NaN')
                data.TrnAmount = 0;
            data.TaxAmount = 0;
            data.BaseTaxAmount = 0;
            var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
            var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.MasterList), 'TrnAmount');
            var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount');

            for (var i = 0; i < $scope.MasterList.length; i++) {
                if ($scope.MasterList[i].TransactionQty > 0) {
                    $scope.MasterList[i].Balance = '';
                    var ToleranceQty = $scope.MasterList[i].POQty * $scope.MasterList[i].Tolerance / 100;
                    var newpoQty = $scope.MasterList[i].POQty + ToleranceQty;

                    if ($scope.MasterList[i].ShortageQty > $scope.MasterList[i].TransactionQty) {
                        ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
                        return false;
                    }
                    else if ($scope.MasterList[i].RejectionQty > $scope.MasterList[i].TransactionQty) {
                        ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
                        return false;
                    }
                    else {
                        if ($scope.MasterList[i].PODetailsID == data.PODetailsID) {
                            $scope.MasterList[i].TrnAmount = Math.round(data.TrnAmount * 100 + Number.EPSILON) / 100;
                            if (TotalServiceAmount > 0) {
                                $scope.MasterList[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.MasterList[i].TrnAmount;
                                $scope.MasterList[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.MasterList[i].TrnAmount;
                            }

                            $scope.MasterList[i].Balance = ($scope.MasterList[i].POQty - ($scope.MasterList[i].GRNRcvQty + $scope.MasterList[i].TransactionQty));
                            //$scope.MasterList[i].ShortageQty = ($scope.MasterList[i].POQty - ($scope.MasterList[i].GRNRcvQty + $scope.MasterList[i].TransactionQty));
                            $scope.MasterList[i].ApprovedQty = ($scope.MasterList[i].TransactionQty - ($scope.MasterList[i].ShortageQty + $scope.MasterList[i].RejectionQty));
                            //$scope.MasterList[i].ApprovedQty = ($scope.MasterList[i].TransactionQty - $scope.MasterList[i].RejectionQty);
                            $scope.MasterList[i].NetQty = ($scope.MasterList[i].TransactionQty - $scope.MasterList[i].ShortageQty);

                        }
                        else {
                            //$scope.MasterList[i].BaseTaxAmount = (($scope.MasterList[i].TotalTaxAmount / $scope.MasterList[i].POQty) * $scope.MasterList[i].TransactionQty).toFixed(2);
                            if (TotalServiceAmount > 0) {
                                $scope.MasterList[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.MasterList[i].TrnAmount;
                                $scope.MasterList[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.MasterList[i].TrnAmount;
                            }
                            $scope.MasterList[i].Balance = ($scope.MasterList[i].POQty - ($scope.MasterList[i].GRNRcvQty + $scope.MasterList[i].TransactionQty));
                            //$scope.MasterList[i].ShortageQty = ($scope.MasterList[i].POQty - ($scope.MasterList[i].GRNRcvQty+$scope.MasterList[i].TransactionQty));
                            $scope.MasterList[i].ApprovedQty = ($scope.MasterList[i].TransactionQty - ($scope.MasterList[i].ShortageQty + $scope.MasterList[i].RejectionQty));
                            //$scope.MasterList[i].ApprovedQty = ($scope.MasterList[i].TransactionQty - $scope.MasterList[i].RejectionQty);
                            $scope.MasterList[i].NetQty = ($scope.MasterList[i].TransactionQty - $scope.MasterList[i].ShortageQty);
                        }
                        if ($scope.productNew.IsNonCreditable == 1) {
                            $scope.MasterList[i].TrnAmount = ($scope.MasterList[i].NetQty * $scope.MasterList[i].TransactionRate).toFixed(2);
                            $scope.MasterList[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].ServiceTax) + parseFloat($scope.MasterList[i].ServiceCharge) + parseFloat(data.ServiceTax)) * 100 + Number.EPSILON) / 100;
                            $scope.MasterList[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].ServiceTax) + parseFloat($scope.MasterList[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
                        }
                        else {
                            $scope.MasterList[i].TrnAmount = Math.round(($scope.MasterList[i].NetQty * $scope.MasterList[i].TransactionRate) * 100 + Number.EPSILON) / 100;
                            $scope.MasterList[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].ServiceCharge)) * 100 + Number.EPSILON) / 100;
                            $scope.MasterList[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
                        }
                    }
                }
            }
            angular.forEach(data.POMaterialTaxList, function (item) {
                item.TaxAmount = Math.round(((data.TrnAmount * item.Percentage) / 100) * 100 + Number.EPSILON) / 100;
            });
            for (var i1 = 0; i1 < $scope.MasterList.length; i1++) {
                if ($scope.MasterList[i1].PODetailsID == data.PODetailsID) {
                    $scope.MasterList[i1].BaseTaxAmount = Math.round($filter('sumByKey')($filter('filter')(data.POMaterialTaxList, { "PODetailId": data.PODetailsID }), 'TaxAmount') * 100 + Number.EPSILON) / 100;
                }
            }
        }
    };
    $scope.popupcalculateAmounts = function (data) {
        if (baseService.isUndefinedOrNull(data.PurchaseDocumentAcceptanceId)) {

            var count = 0;
            for (var j = 0; j < $scope.MasterListNewBOQ.length; j++) {
                if ($scope.MasterListNewBOQ[j].TransactionQty > 0) {
                    count++;
                }
                else {
                    $scope.MasterListNewBOQ[j].ServiceCharge = 0;
                    $scope.MasterListNewBOQ[j].ServiceTax = 0;
                    $scope.MasterListNewBOQ[j].TrnAmount = 0;
                    $scope.MasterListNewBOQ[j].TotalMaterialTranAmount = 0;
                    $scope.MasterListNewBOQ[j].TotalMaterialTranAmount = 0;
                }
            }

            $scope.PreBal = data.Balance;
            // data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
            data.TrnAmount = parseFloat(data.TransactionQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
            if (data.TrnAmount == 'NaN')
                data.TrnAmount = 0;
            data.TaxAmount = 0;
            data.BaseTaxAmount = 0;
            var TotalServiceAmount = Math.round($filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount') * 100 + Number.EPSILON) / 100;
            var TotalTrnAmount = Math.round($filter('sumByKey')($filter('filter')($scope.MasterListNewBOQ), 'TrnAmount') * 100 + Number.EPSILON) / 100;
            var TotalServiceTaxAmount = Math.round($filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount') * 100 + Number.EPSILON) / 100;
            var tempServiceAmount = 0;
            var tempServiceTaxAmount = 0;
            var newcount = 0;
            for (var i = 0; i < $scope.MasterListNewBOQ.length; i++) {
                if ($scope.MasterListNewBOQ[i].TransactionQty > 0) {
                    newcount++;
                    $scope.MasterListNewBOQ[i].Balance = '';
                    var ToleranceQty = $scope.MasterListNewBOQ[i].POQty * $scope.MasterListNewBOQ[i].Tolerance / 100;
                    var newpoQty = $scope.MasterListNewBOQ[i].POQty + ToleranceQty;
                    if ($scope.MasterListNewBOQ[i].POQty < (parseFloat($scope.MasterListNewBOQ[i].GRNRcvQty + $scope.MasterListNewBOQ[i].TransactionQty).toFixed(2)) && (baseService.isUndefinedOrNull($scope.MasterListNewBOQ[i].Tolerance) || $scope.MasterListNewBOQ[i].Tolerance === 0)) {
                        //$scope.MasterListNewBOQ[i].Balance = $scope.MasterListNewBOQ[i].POQty - ($scope.MasterListNewBOQ[i].GRNRcvQty + $scope.MasterListNewBOQ[i].TransactionQty);
                        $scope.MasterListNewBOQ[i].TransactionQty = '';
                        ShowResult('Current quantity can not grater than balance qty!', 'failure');
                        return false;
                    }

                    else if (newpoQty < (parseFloat($scope.MasterListNewBOQ[i].GRNRcvQty + $scope.MasterListNewBOQ[i].TransactionQty).toFixed(2)) && (!baseService.isUndefinedOrNull($scope.MasterListNewBOQ[i].Tolerance) || $scope.MasterListNewBOQ[i].Tolerance > 0)) {
                        ShowResult('Current quantity can not grater than po qty and Tolerance qty!PO + Tolerance=' + newpoQty, 'failure');
                        return false;
                    }
                    else if ($scope.MasterListNewBOQ[i].ShortageQty > $scope.MasterListNewBOQ[i].TransactionQty) {
                        ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
                        return false;
                    }
                    else if ($scope.MasterListNewBOQ[i].RejectionQty > $scope.MasterListNewBOQ[i].TransactionQty) {
                        ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
                        return false;
                    }
                    else {

                        if ($scope.MasterListNewBOQ[i].PODetailsID == data.PODetailsID) {
                            $scope.MasterListNewBOQ[i].TrnAmount = Math.round(data.TrnAmount * 100 + Number.EPSILON) / 100;
                            angular.forEach(data.POMaterialTaxList, function (item) {
                                item.TaxAmount = Math.round(((data.TrnAmount * item.Percentage) / 100) * 100 + Number.EPSILON) / 100;
                            });

                            //$scope.MasterListNewBOQ[i].BaseTaxAmount = Math.round($filter('sumByKey')($filter('filter')(data.POMaterialTaxList, { "PODetailId": data.PODetailsID }), 'TaxAmount') * 100 + Number.EPSILON) / 100;

                            if (TotalServiceAmount > 0) {
                                //$scope.MasterListNewBOQ[i].BaseTaxAmount = (($scope.MasterListNewBOQ[i].TotalTaxAmount / $scope.MasterListNewBOQ[i].POQty) * $scope.MasterListNewBOQ[i].TransactionQty).toFixed(2);
                                if (count > newcount) {
                                    $scope.MasterListNewBOQ[i].ServiceCharge = Math.round(((TotalServiceAmount / TotalTrnAmount) * $scope.MasterListNewBOQ[i].TrnAmount) * 100 + Number.EPSILON) / 100;
                                    $scope.MasterListNewBOQ[i].ServiceTax = Math.round(((TotalServiceTaxAmount / TotalTrnAmount) * $scope.MasterListNewBOQ[i].TrnAmount) * 100 + Number.EPSILON) / 100;

                                }
                                else if (count == newcount) {
                                    $scope.MasterListNewBOQ[i].ServiceCharge = 0;
                                    tempServiceAmount = Math.round($filter('sumByKey')($filter('filter')($scope.MasterListNewBOQ), 'ServiceCharge') * 100 + Number.EPSILON) / 100;
                                    $scope.MasterListNewBOQ[i].ServiceCharge = Math.round((TotalServiceAmount - tempServiceAmount) * 100 + Number.EPSILON) / 100;
                                    $scope.MasterListNewBOQ[i].ServiceTax = 0;
                                    tempServiceTaxAmount = Math.round($filter('sumByKey')($filter('filter')($scope.MasterListNewBOQ), 'ServiceTax') * 100 + Number.EPSILON) / 100;
                                    $scope.MasterListNewBOQ[i].ServiceTax = Math.round((TotalServiceTaxAmount - tempServiceTaxAmount) * 100 + Number.EPSILON) / 100
                                }

                            }

                            $scope.MasterListNewBOQ[i].Balance = ($scope.MasterListNewBOQ[i].POQty - ($scope.MasterListNewBOQ[i].GRNRcvQty + $scope.MasterListNewBOQ[i].TransactionQty));
                            //$scope.MasterListNewBOQ[i].ShortageQty = ($scope.MasterListNewBOQ[i].POQty - ($scope.MasterListNewBOQ[i].GRNRcvQty + $scope.MasterListNewBOQ[i].TransactionQty));
                            $scope.MasterListNewBOQ[i].ApprovedQty = ($scope.MasterListNewBOQ[i].TransactionQty - ($scope.MasterListNewBOQ[i].ShortageQty + $scope.MasterListNewBOQ[i].RejectionQty));
                            //$scope.MasterListNewBOQ[i].ApprovedQty = ($scope.MasterListNewBOQ[i].TransactionQty - $scope.MasterListNewBOQ[i].RejectionQty);
                            $scope.MasterListNewBOQ[i].NetQty = ($scope.MasterListNewBOQ[i].TransactionQty - $scope.MasterListNewBOQ[i].ShortageQty);

                        }
                        else {
                            if (TotalServiceAmount > 0) {
                                if (count > newcount) {
                                    $scope.MasterListNewBOQ[i].ServiceCharge = Math.round(((TotalServiceAmount / TotalTrnAmount) * $scope.MasterListNewBOQ[i].TrnAmount) * 100 + Number.EPSILON) / 100;
                                    $scope.MasterListNewBOQ[i].ServiceTax = Math.round(((TotalServiceTaxAmount / TotalTrnAmount) * $scope.MasterListNewBOQ[i].TrnAmount) * 100 + Number.EPSILON) / 100;

                                }
                                else if (count == newcount) {
                                    $scope.MasterListNewBOQ[i].ServiceCharge = 0;
                                    tempServiceAmount = Math.round($filter('sumByKey')($filter('filter')($scope.MasterListNewBOQ), 'ServiceCharge') * 100 + Number.EPSILON) / 100;
                                    $scope.MasterListNewBOQ[i].ServiceCharge = Math.round((TotalServiceAmount - tempServiceAmount) * 100 + Number.EPSILON) / 100;
                                    $scope.MasterListNewBOQ[i].ServiceTax = 0;
                                    tempServiceTaxAmount = Math.round($filter('sumByKey')($filter('filter')($scope.MasterListNewBOQ), 'ServiceTax') * 100 + Number.EPSILON) / 100;
                                    $scope.MasterListNewBOQ[i].ServiceTax = Math.round((TotalServiceTaxAmount - tempServiceTaxAmount) * 100 + Number.EPSILON) / 100
                                }

                            }
                            $scope.MasterListNewBOQ[i].Balance = ($scope.MasterListNewBOQ[i].POQty - ($scope.MasterListNewBOQ[i].GRNRcvQty + $scope.MasterListNewBOQ[i].TransactionQty));
                            //$scope.MasterListNewBOQ[i].ShortageQty = ($scope.MasterListNewBOQ[i].POQty - ($scope.MasterListNewBOQ[i].GRNRcvQty+$scope.MasterListNewBOQ[i].TransactionQty));
                            $scope.MasterListNewBOQ[i].ApprovedQty = ($scope.MasterListNewBOQ[i].TransactionQty - ($scope.MasterListNewBOQ[i].ShortageQty + $scope.MasterListNewBOQ[i].RejectionQty));
                            //$scope.MasterListNewBOQ[i].ApprovedQty = ($scope.MasterListNewBOQ[i].TransactionQty - $scope.MasterListNewBOQ[i].RejectionQty);
                            $scope.MasterListNewBOQ[i].NetQty = ($scope.MasterListNewBOQ[i].TransactionQty - $scope.MasterListNewBOQ[i].ShortageQty);
                        }
                        if ($scope.productNew.IsNonCreditable == 1) {
                            $scope.MasterListNewBOQ[i].TrnAmount = ($scope.MasterListNewBOQ[i].NetQty * $scope.MasterListNewBOQ[i].TransactionRate).toFixed(2);
                            $scope.MasterListNewBOQ[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.MasterListNewBOQ[i].TrnAmount) + parseFloat($scope.MasterListNewBOQ[i].ServiceTax) + parseFloat($scope.MasterListNewBOQ[i].ServiceCharge) + parseFloat($scope.MasterListNewBOQ[i].BaseTaxAmount)) * 100 + Number.EPSILON) / 100;
                            $scope.MasterListNewBOQ[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.MasterListNewBOQ[i].TrnAmount) + parseFloat($scope.MasterListNewBOQ[i].ServiceTax) + parseFloat($scope.MasterListNewBOQ[i].ServiceCharge) + parseFloat($scope.MasterListNewBOQ[i].BaseTaxAmount)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;

                        }
                        else {
                            $scope.MasterListNewBOQ[i].TrnAmount = Math.round(($scope.MasterListNewBOQ[i].NetQty * $scope.MasterListNewBOQ[i].TransactionRate) * 100 + Number.EPSILON) / 100;
                            $scope.MasterListNewBOQ[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.MasterListNewBOQ[i].TrnAmount) + parseFloat($scope.MasterListNewBOQ[i].ServiceCharge)) * 100 + Number.EPSILON) / 100;
                            $scope.MasterListNewBOQ[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.MasterListNewBOQ[i].TrnAmount) + parseFloat($scope.MasterListNewBOQ[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
                        }
                    }

                }
            }


        }
    };
    $scope.calculateAmounts = function (data) {
        if (data.Balance < data.TransactionQty) {
            data.TransactionQty = '';
            ShowResult("Receive Qty can not greater than Balance Qty", 'failure', 'GRnBOQPoo');
        }
        var gridObj = $("#GRnBOQPooGrid").data("ejGrid");
        gridObj.refreshContent();
    }
    $scope.GriddataMaster = [];
    $scope.GetListForGRNBYPO = function (grnbypostatus) {
        $scope.GriddataMaster = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetListForGRNBYPO?GRNbyPOCheckStatus=' + grnbypostatus + '&grnType=' + 'GRNBYBOQ',
        }).then(function successCallback(response) {
            $scope.GriddataMaster = response.data;
        });
    };
    //$scope.GetListForGRNBYPO();

    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("InventoryReceiveId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid").ejGrid({
            dataSource: data,
            columns: ["MaterialGroupName", "MaterialName", "Article", "SKU1", "SKU2", "SKU3", "MaterialDetail", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalMaterialTranAmount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
        //var filteredData1 = e.data["Id"];
        var dataImg = ej.DataManager(window.Img).executeLocal(ej.Query().where("GRNId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid1").ejGrid({
            dataSource: dataImg,
            columns: [{ field: "UserFilename", headerText: "UserFilename", width: 100 },
            { field: "Description", headerText: "Description", width: 100 },
            { field: "Remarks", headerText: "Remarks", width: 100 },

            ]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    $scope.GRN = "";
    $scope.tab = 1;
    $scope.setTabGRNList = function (newTab) {
        $scope.tab = newTab;
        $scope.GRNbyPOCheckStatus = "ForChecked";
        $scope.GetListForGRNBYPO($scope.GRNbyPOCheckStatus);
    };
    $scope.isSetGRNList = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 1;

    };
    $scope.setTabGRNList(1);
    $scope.setTabCheckedHR = function (newTab) {
        $scope.tab = newTab;
        $scope.GRNbyPOCheckStatus = "CheckedHoldReject";
        $scope.GetListForGRNBYPO($scope.GRNbyPOCheckStatus);

    };
    $scope.isSetCheckedHR = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 2;
    };

    $scope.setTabNotApprovedChecked = function (newTab) {
        $scope.tab = newTab;
        $scope.GRNbyPOCheckStatus = "Checked";
        $scope.GetListForGRNBYPO($scope.GRNbyPOCheckStatus);
    };

    $scope.isSetNotApprovedChecked = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 3;
    };


    $scope.setTabApprovedHR = function (newTab) {
        $scope.GRNbyPOApprovedStatus = "ApprovedHoldReject";
        $scope.tab = newTab;
        $scope.GetListForGRNBYPO($scope.GRNbyPOCheckStatus);
    };

    $scope.isSetApprovedHR = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 4;
    };


    $scope.setTabApprovedNP = function (newTab) {
        $scope.tab = newTab;
        $scope.GRNbyPOCheckStatus = "Approved";
        $scope.GetListForGRNBYPO($scope.GRNbyPOCheckStatus);
    };

    $scope.isSetApprovedNP = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 5;
    };

    $scope.setTabPosted = function (newTab) {
        $scope.tab = newTab;
        $scope.GRNbyPOCheckStatus = "Posted";
        $scope.GetListForGRNBYPO($scope.GRNbyPOCheckStatus);
    };

    $scope.isSetPosted = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 6;
    };

    $scope.onClickReportDownloadWord = function (args) {
        var gridObj = $("#GriddataMaster1").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandWord = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",
            click: $scope.onClickReportDownloadWord
        }
    }];

    //#region Update of BOQ GRN PO
    $scope.MasterGrid = function ($event) {
        $scope.MasterList = [];
        $scope.GriddataSelected = [];
        $scope.DetailList = [];
        var x = $event;
        var Id = x.data.Id;
        $scope.productId = Id;
        $scope.ActionForEdit = 'Update';
        $scope.POId1 = x.data.POId;
        $scope.POID = x.data.POId;
        $scope.product = $scope.products[$scope.index];
        $scope.productNew = x.data;
        $scope.productNew.NoteForAccounts = x.data.NoteForAccounts;
        $scope.productNew.GRNDate = x.data.GRNDate1;
        $scope.productNew.CheckedBy = x.data.CheckedBy;
        $scope.AcceptanceId = x.data.PurchaseDocumentAcceptanceId;
        $scope.AccDate = x.data.AcceptanceDate;
        if ($scope.AcceptanceId === null || $scope.AcceptanceId === "" || $scope.AcceptanceId === undefined) {
            $scope.status = 'PO';
            $scope.productNew.PO = $scope.status;

            $scope.tab1 = 1;
            $scope.GetSavedPOList1BOQ(Id);
        }
        else {

            $scope.status = 'Acceptance';
            $scope.productNew.PO = $scope.status;
            $scope.tab1 = 2;
        }

        getPartyPlantListBOQ();
        getInventoryMaterialListBOQ(Id);
        getServiceChargeListBOQ(Id);
        $scope.GetAdvanceTaxInfoBOQ(Id);
        $scope.productNew.TaxOptionAddiTax = 'Yes';
        // $scope.TotalSumAfterTCSBOQ();
        $scope.ImagedataLoadBOQ(Id);
        if (baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = false;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = x.data.ApprovedById;
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = true;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = x.data.CheckedById;
        }

        $scope.GetCheckedByAndApprovedBy1BoQ();


        if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {

            $scope.productNew.CheckedBy = x.data.ApprovedById;
            $scope.productNew.labelCheckAndApproved = 'To be approved by';
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && baseService.isUndefinedOrNull(x.data.ApprovedById)) {

            $scope.productNew.CheckedBy = x.data.CheckedById;
            $scope.productNew.labelCheckAndApproved = 'To be checked by';
        }
        $scope.loadAcceptanceDetailBOQ();

        if (!$rootScope.isCollapsed) $rootScope.toggle();

    }
    $scope.inventoryMaterialList = [];
    $scope.sumORnot = false;
    function getInventoryMaterialListBOQ(inveReveiveId) {
        $scope.inventoryReceiveId = inveReveiveId;
        $http.get($scope.path + 'GetInventoryMaterialListBOQ?inveReveiveId=' + inveReveiveId + '&POID=' + $scope.POID + '&AcceptanceId=' + $scope.AcceptanceId)
            .then(function (response) {
                $scope.MasterList = [];
                $scope.MasterList = response.data.Rows;
                $scope.POIDs = $scope.MasterList[0].POId;
                //$scope.productNew.CheckedBy = $scope.inventoryMaterialList[0].CheckedBy;
                $scope.productNew.PODate = $scope.MasterList[0].AddedDate;
                $scope.TotalSumAfterTCSBOQ();
                checkSameValueInColumnListBOQ($scope.MasterList, 'TransactionUoM');
                getGrossAmountBOQ($scope.MasterList, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
                //$scope.GetMaterialTaxDataBOQ();
                $scope.GetGRNMaterialTaxData($scope.inventoryReceiveId);
            });
    }
    function getGrossAmountBOQ(list, key1, key2, key3, fieldName) {
        $scope[fieldName] = 0;
        for (var t = 0; t < baseService.arrayLength(list); t++) {
            $scope[fieldName] += parseFloat(list[t][key1]);// + parseFloat(list[t][key2]) + parseFloat(list[t][key3]);
        }
    }
    function checkSameValueInColumnListBOQ(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }
    //$scope.MaterialTaxList = [];
    //$scope.GetMaterialTaxDataBOQ = function () {
    //    //debugger;
    //    $scope.MaterialTaxList = [];
    //    $http({
    //        method: "GET",
    //        url: $scope.path + 'GetReceiveTaxListBOQ?receiveDetailId=' + $scope.inventoryReceiveId
    //    }).then(function (response) {
    //        $scope.MaterialTaxList = response.data;

    //        for (var i = 0; i < $scope.MasterList.length; i++) {
    //            var linepk = $scope.MasterList[i].InventoryReceiveDetailId;
    //            var list = getMaterialtaxlistBOQ(linepk);
    //            $scope.MasterList[i].MaterialTaxList = list;
    //        }
    //    });
    //};

    //function getMaterialtaxlistBOQ(linepk) {
    //    //debugger;
    //    var result4 = [];
    //    for (var i = 0; i < $scope.MaterialTaxList.length; i++) {
    //        if ($scope.MaterialTaxList[i].PODetailId === linepk) {
    //            result4.push($scope.MaterialTaxList[i]);
    //        }
    //    }
    //    return result4;
    //}
    function getServiceChargeListBOQ(inveReveiveId) {
        $scope.masterId12 = inveReveiveId;
        //debugger;
        $http.get($scope.path + 'GetServiceChargeListBOQ?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.chargesList = [];
                $scope.chargesList = response.data;
                $scope.getServiceTaxListBOQ();

            });
    }
    $scope.getServiceTaxListBOQ = function () { //,data, flag)

        $http({
            method: 'GET',
            url: $scope.path + 'GetServiceTaxListBOQ?serviceId=' + $scope.masterId12//data.Id
        }).then(function (response) {
            $scope.ServiceTaxList = response.data;
            for (var i = 0; i < $scope.chargesList.length; i++) {
                var linepk1 = $scope.chargesList[i].Id;
                var list11 = getServicetaxlist1BOQ(linepk1);
                $scope.chargesList[i].ServiceTaxList = list11;
            }
        });
    }
    function getServicetaxlist1BOQ(linepk111) {
        //debugger;
        var result11 = [];
        for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
            if ($scope.ServiceTaxList[i].InventoryServiceId === linepk111) {
                result11.push($scope.ServiceTaxList[i]);
            }
        }
        return result11;
    }
    $scope.advanceTaxesList = [];
    $scope.GetAdvanceTaxInfoBOQ = function (Id) {

        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.path + 'GetAdvanceTaxInfoBOQ?InventoryReceiveId=' + Id,
        }).then(function successCallback(response) {
            $scope.advanceTaxesList = response.data;
            $scope.TotalSumAfterTCSBOQ();
        });

    }

    $scope.TotalSumAfterTCSBOQ = function () {

        if ($scope.inventoryMaterialListPO.length > 0) {
            $scope.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);
        }
        else {
            $scope.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);
        }
    }
    $scope.Imagedata = [];
    $scope.ImagedataLoadBOQ = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.path + 'GRNDocumentMapDataBOQ?POID=' + $scope.productNew.Id,
        }).then(function successCallback(response) { //datagatefun
            $scope.Imagedata = response.data;

        });
    };
    $scope.checkedByList = [];
    $scope.GetCheckedByAndApprovedBy1BoQ = function () {
        if (!baseService.isUndefinedOrNull($scope.CheckedByStatusForNoti) && !baseService.isUndefinedOrNull($scope.ApprovedByStatusForNoti)) {
            $http({
                method: 'GET',
                url: $scope.path + 'GetCheckedByAndApprovedBYBOQ?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.checkedByList = response.data;
            });
        }
        else {

        }

    }
    $scope.loadAcceptanceDetailList = [];
    $scope.loadAcceptanceDetailBOQ = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/LoadAcceptanceDetailsBOQ?AcceptanceId=' + $scope.AcceptanceId,
        }).then(function successCallback(response) {


            $scope.loadAcceptanceDetailList = response.data;

            if (baseService.arrayLength($scope.loadAcceptanceDetailList) > 0) {
                $scope.productNew.InvoicingPartyPlantId = $scope.loadAcceptanceDetailList[0].InvoicingPartyPlantId;
                $scope.productNew.InvoicingByName = $scope.loadAcceptanceDetailList[0].InvoicingBy;
                $scope.productNew.InvoicingPartyPlantId = $scope.loadAcceptanceDetailList[0].InvoicingPartyPlantId;
                $scope.productNew.InvoicingPartyPlantId = $scope.loadAcceptanceDetailList[0].InvoicingPartyPlantId;
                $scope.productNew.InvoicingByAddress = $scope.loadAcceptanceDetailList[0].InvoicingByAddress;

                $scope.productNew.DeliveryPartyPlantId = $scope.loadAcceptanceDetailList[0].DeliveryPartyPlantId;
                $scope.productNew.DeliveryByName = $scope.loadAcceptanceDetailList[0].DeliveryBy;
                $scope.productNew.DeliveryPartyPlantId = $scope.loadAcceptanceDetailList[0].DeliveryPartyPlantId;
                $scope.productNew.DeliveryPartyPlantId = $scope.loadAcceptanceDetailList[0].DeliveryPartyPlantId;
                $scope.productNew.DeliveryByAddress = $scope.loadAcceptanceDetailList[0].DeliveryByAddress;
                $scope.productNew.ToCurrencyRate = $scope.loadAcceptanceDetailList[0].AcceptanceRate;
                $scope.productNew.CurrencyId = $scope.loadAcceptanceDetailList[0].CurrencyId;

                $scope.productNew.PartyName = $scope.loadAcceptanceDetailList[0].PartyName;
                $scope.productNew.DocRefNo = $scope.loadAcceptanceDetailList[0].DocRefNo;
                $scope.productNew.DocDate = $scope.loadAcceptanceDetailList[0].DocDate;
                $scope.productNew.CurrencyId = $scope.loadAcceptanceDetailList[0].CurrencyId;
                $scope.productNew.ToCurrencyRate = $scope.loadAcceptanceDetailList[0].AcceptanceRate;
                $scope.productNew.VoucherId = $scope.loadAcceptanceDetailList[0].VoucherId;
                $scope.productNew.InvoiceNo = $scope.loadAcceptanceDetailList[0].InvoiceNo;
                $scope.productNew.InvoiceDate = $scope.loadAcceptanceDetailList[0].InvoiceDate;
                $scope.productNew.DueDate = $scope.loadAcceptanceDetailList[0].DueDate;
                $scope.productNew.PurchaseLCId = $scope.loadAcceptanceDetailList[0].LCANo;
                $scope.productNew.LCDate = $scope.loadAcceptanceDetailList[0].LCDate;
                $scope.productNew.ContractId = $scope.loadAcceptanceDetailList[0].ContractId;

                $scope.productNew.AcceptancePaymentSource = $scope.loadAcceptanceDetailList[0].AcceptancePaymentSource;
                $scope.productNew.PartyId = $scope.loadAcceptanceDetailList[0].PartyId;
                $scope.productNew.partySearchByList = $scope.loadAcceptanceDetailList[0].PartyId;

                if ($scope.loadAcceptanceDetailList[0].IsNonCreditable === 'Yes') {
                    $scope.productNew.IsNonCreditable = true;

                }
                else {
                    $scope.productNew.IsNonCreditable = false;


                }
            }
            //$scope.productId = "";
            $scope.AcceptanceId = $scope.AcceptanceId;
            getPartyPlantListBOQ();
            //getPartyPlantEditList();
            var id1 = null;
            GetInventoryMaterialListByPOBOQ(id1, $scope.AcceptanceId);
            getServiceChargeListPOBOQ(id1);
            $scope.productNew.PO = $scope.status;

        });
    }
    $scope.inventoryMaterialListPO = [];
    function GetInventoryMaterialListByPOBOQ(inveReveiveId) {
        //debugger;
        $scope.masterId = inveReveiveId;
        $http.get($scope.path + 'GetInventoryMaterialListByOnlyPOBOQ?inveReveiveId=' + inveReveiveId + '&AcceptanceId=' + $scope.AcceptanceId)
            .then(function (response) {
                $scope.inventoryMaterialListPO = [];
                $scope.inventoryMaterialListPO = response.data.Rows;
                $scope.POID = $scope.inventoryMaterialListPO.POID;
                $scope.PreBal = $scope.inventoryMaterialListPO.Balance;
                $scope.PODetailsID = $scope.inventoryMaterialListPO.InventoryReceiveDetailId;
                if (baseService.arrayLength($scope.inventoryMaterialListPO) > 0) {
                    $scope.productNew.InvoicingByAddress = $scope.inventoryMaterialListPO[0].InvoicingByAddress;
                    $scope.productNew.DeliveryByAddress = $scope.inventoryMaterialListPO[0].DeliveryByAddress;
                }
                $scope.inventoryMaterialListPO.BaseAmount = '0';
                checkSameValueInColumnListBOQ($scope.inventoryMaterialListPO, 'TransactionUoM');
                getGrossAmountBOQ($scope.inventoryMaterialListPO, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');

            });
    }
    $scope.chargesListPO = [];
    function getServiceChargeListPOBOQ(inveReveiveId) {
        $scope.inveReveiveId = inveReveiveId;
        $http.get($scope.path + 'GetServiceChargeListPOBOQ?receiveId=' + inveReveiveId + '&AcceptanceId=' + $scope.AcceptanceId)
            .then(function (response) {
                $scope.chargesListPO = [];
                $scope.chargesListPO = response.data;
                $scope.GetPOServiceTaxData();
            });
    }
    $scope.GetSavedPOListNew = [];
    $scope.GetSavedPOList1BOQ = function (Id) {
        //debugger;
        var PoType = 'PO';
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetSavedPOListBOQ?GRNId=' + Id,
        }).then(function successCallback(response) {
            //$scope.GetSavedPOListNew = [];
            $scope.GetSavedPOListNew = response.data;
            for (var i = 0; i < $scope.GetSavedPOListNew.length; i++) {
                $scope.GriddataSelected.push($scope.GetSavedPOListNew[i]);
            }

        });
    };
    //#endregion

    //#region Allocation Part 
    $scope.Action1 = 'Save';
    $scope.GRNAllowcationForSO = function (x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID) {
        $scope.Action1 = 'Update'
        GRNAllowcationForSOListBOQ(x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID);
        angular.element(document.querySelector('#ListOfSoBoq')).modal('show');
    };
    $scope.ListOfSo = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListOfSoBoq')).modal('hide');
    };
    $scope.soList = [];
    $scope.InventoryReceiveDetailId = '';
    function GRNAllowcationForSOListBOQ(x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID) {
        //$scope.Action1 = 'Save';
        $scope.InventoryReceiveDetailId = InventoryReceiveDetailId;
        $http.get($scope.path + 'GetGRNDetailsForSoAllocationBOQ?InventoryReceiveDetailId=' + x)
            .then(function (response) {
                $scope.soList = response.data;
                $scope.totalGRNVal = $scope.soList[0].GRNQty;
                $scope.RejectionQty = $scope.soList[0].GRNRejectionQty;
            });
    }
    $scope.soListNew = [];
    $scope.GrnRequisitionAllocationSave = function () {
        debugger;
        try {
            $scope.soListNew = [];
            var totalGRNQty = 0;
            var totalallowCatedQtyQty = 0;
            var totalGRNQty1 = 0;
            var totalallowCatedQtyQty1 = 0;
            for (var i = 0; i < $scope.soList.length; i++) {

                if ($scope.soList[i].Active === true) {
                    var TotalSOQty = $filter('sumByKey')($filter('filter')($scope.soList), 'TransactionQty');
                    var TotalRejectionQty = $filter('sumByKey')($filter('filter')($scope.soList), 'RejectionQty');
                    if (TotalSOQty > $scope.totalGRNVal) {
                        ShowResult('Allocated Qty can not grater than GRN Qty', 'failure', 'ListOfSoBoq');
                        return false;
                    }
                    else if (TotalRejectionQty > $scope.RejectionQty) {
                        ShowResult('Allocated Qty can not grater than Rejection Qty', 'failure', 'ListOfSoBoq');
                        return false;
                    }
                    else if (baseService.isUndefinedOrNull($scope.soList[i].TransactionQty) || $scope.soList[i].TransactionQty === 0) {
                        ShowResult('Enter the Qty', 'failure', 'ListOfSoBoq');
                        return false;
                    }
                    else {
                        $scope.soListNew.push($scope.soList[i]);
                    }

                    totalGRNQty += $scope.soList[i].TransactionQty;
                    totalGRNQty1 += $scope.soList[i].RejectionQty;

                }
                else {
                    totalallowCatedQtyQty += $scope.soList[i].allowCatedQty;
                    totalallowCatedQtyQty1 += $scope.soList[i].RejectQty;
                }

                var res = totalGRNQty + totalallowCatedQtyQty;
                var res1 = totalGRNQty1 + totalallowCatedQtyQty1;
                if (res > $scope.totalGRNVal) {
                    ShowResult('allocated qty can not grater than GRN Qty', 'failure', 'ListOfSoBoq');
                    return false;
                }
                if (res1 > $scope.RejectionQty) {
                    ShowResult('allocated qty can not grater than Rejection Qty', 'failure', 'ListOfSoBoq');
                    return false;
                }
            }
            if ($scope.soListNew.length === 0) {
                ShowResult('Please select atlest one item', 'failure', 'ListOfSoBoq');
                return false;
            }
            if ($scope.Action1 === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Products/GoodsReceiveNote/GrnRequisitionAllocationSaveBOQ',
                    data: {
                        entity: $scope.soListNew
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'ListOfSoBoq');
                    else {
                        ShowResult(response.data.Message, 'success', 'ListOfSoBoq');
                        GRNAllowcationForSOListBOQ($scope.InventoryReceiveDetailId);
                        $scope.Action1 = "Update";
                        //$scope.GetListForMasterOrder = [];
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'ListOfSoBoq');
                };

            }
            else if ($scope.Action1 === "Update") {
                $http({
                    method: 'POST',
                    url: 'Products/GoodsReceiveNote/GrnRequisitionAllocationSaveBOQ',
                    data: {
                        entity: $scope.soListNew
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'ListOfSoBoq');
                    else {
                        ShowResult(response.data.Message, 'success', 'ListOfSoBoq');
                        GRNAllowcationForSOListBOQ($scope.InventoryReceiveDetailId);

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'ListOfSoBoq');
                };

            }
        } catch (e) {
        }
    };
    $scope.specific = [];
    $scope.NewPOMaterialTaxList = [];
    $scope.SubmitList = function () {
        $scope.MasterList = [];
        $scope.NewPOMaterialTaxList = [];
        try {
            for (var n = 0; n < baseService.arrayLength($scope.MasterListNewBOQ); n++) { // add
                if ($scope.MasterListNewBOQ[n].TransactionQty > 0) {
                    var nRow = {};
                    nRow = $scope.MasterListNewBOQ[n];

                    $scope.MasterListNewBOQ[n].Qty = $scope.MasterListNewBOQ[n].TransactionQty;
                    $scope.MasterListNewBOQ[n].ApprovedQty = $scope.MasterListNewBOQ[n].TransactionQty;
                    $scope.MasterListNewBOQ[n].NetQty = $scope.MasterListNewBOQ[n].TransactionQty;
                    $scope.MasterListNewBOQ[n].Rate = $scope.MasterListNewBOQ[n].BaseRate;
                    nRow.BaseQty = $scope.MasterListNewBOQ[n].BaseQty;
                    nRow.BaseIssueQty = $scope.MasterListNewBOQ[n].BaseIssueQty;
                    if (!baseService.valueCheckInList($scope.MasterList, 'InventoryReceiveDetailId', nRow.InventoryReceiveDetailId) && nRow.TransactionQty > 0) {
                        var taxAmount = 0;
                        if ($scope.POMaterialTaxList.length > 0) {
                            for (var j = 0; j < $scope.POMaterialTaxList.length; j++) {
                                if (nRow.InventoryReceiveDetailId == $scope.POMaterialTaxList[j].InventoryReceiveDetailId) {
                                    $scope.POMaterialTaxList[j].TaxAmount = Math.round(($scope.MasterListNewBOQ[n].Qty * $scope.MasterListNewBOQ[n].TransactionRate) * ($scope.POMaterialTaxList[j].Percentage / 100) * 100 + Number.EPSILON) / 100;
                                    taxAmount += Math.round(($scope.MasterListNewBOQ[n].Qty * $scope.MasterListNewBOQ[n].TransactionRate) * ($scope.POMaterialTaxList[j].Percentage / 100) * 100 + Number.EPSILON) / 100;
                                    $scope.NewPOMaterialTaxList.push($scope.POMaterialTaxList[j]);
                                }
                            }
                        }
                        nRow.TrnCurrencyBaseRate = $scope.MasterListNewBOQ[n].ToCurrencyRate
                        nRow.TrnAmount = Math.round(($scope.MasterListNewBOQ[n].Qty * $scope.MasterListNewBOQ[n].TransactionRate) * 100 + Number.EPSILON) / 100;

                        nRow.BaseTaxAmount = taxAmount;
                        nRow.QualityStatus = 'Approved';
                        $scope.MasterList.push(nRow);
                        taxAmount = 0;
                    }
                    else {
                        for (var x = 0; x < $scope.MasterList.length; x++) {
                            var taxAmountUpdate = 0;
                            if ($scope.POMaterialTaxList.length > 0) {
                                for (var k = 0; k < $scope.POMaterialTaxList.length; k++) {
                                    if ($scope.MasterListNewBOQ[n].InventoryReceiveDetailId == $scope.POMaterialTaxList[k].InventoryReceiveDetailId && $scope.MasterList[x].InventoryReceiveDetailId == $scope.POMaterialTaxList[k].InventoryReceiveDetailId
                                        && $scope.MasterListNewBOQ[n].TransactionQty > 0) {
                                        for (var l = 0; l < $scope.NewPOMaterialTaxList.length; l++) {
                                            if ($scope.NewPOMaterialTaxList[l].InventoryReceiveDetailId == $scope.POMaterialTaxList[k].InventoryReceiveDetailId
                                                && $scope.NewPOMaterialTaxList[l].TaxCategoryId == $scope.POMaterialTaxList[k].TaxCategoryId) {
                                                taxAmountUpdate += Math.round(($scope.MasterListNewBOQ[n].Qty * $scope.MasterListNewBOQ[n].TransactionRate) * ($scope.NewPOMaterialTaxList[l].Percentage / 100) * 100 + Number.EPSILON) / 100
                                                $scope.NewPOMaterialTaxList[l].TaxAmount += Math.round(($scope.MasterListNewBOQ[n].Qty * $scope.MasterListNewBOQ[n].TransactionRate) * ($scope.NewPOMaterialTaxList[l].Percentage / 100) * 100 + Number.EPSILON) / 100
                                            }
                                        }
                                    }
                                }
                            }
                            if ($scope.MasterList[x].InventoryReceiveDetailId == nRow.InventoryReceiveDetailId && nRow.TransactionQty > 0) {
                                var Qty = nRow.TransactionQty;
                                $scope.MasterList[x].BaseTaxAmount += taxAmountUpdate;
                                $scope.MasterList[x].TransactionQty = $scope.MasterList[x].TransactionQty + parseFloat(Qty);
                                $scope.MasterList[x].POQty += $scope.MasterList[x].POQty;
                                $scope.MasterList[x].Balance += $scope.MasterList[x].Balance;
                                $scope.MasterList[x].GRNRcvQty += $scope.MasterList[x].GRNRcvQty;
                                $scope.MasterList[x].ApprovedQty = $scope.MasterList[x].TransactionQty;
                                $scope.MasterList[x].NetQty = $scope.MasterList[x].TransactionQty;
                                $scope.MasterList[x].TrnAmount += Math.round(($scope.MasterListNewBOQ[n].Qty * $scope.MasterListNewBOQ[n].TransactionRate) * 100 + Number.EPSILON) / 100;
                                Qty = 0;
                            }
                        }
                    }
                }
            }
            $scope.BOQPopUpClose();
        } catch (e) {
            ShowResult(e, 'failure')
        }
    }
    $scope.BOQPopUpClose = function () {
        angular.element(document.querySelector('#GRnBOQPoo')).modal('hide');
    };
    $scope.GetgrnBOQPO = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        if (data.TransactionQty == null || data.TransactionQty < 0) {
            ShowResult('Receive Quantity is 0 in this PO', 'failure');
        }
        else {

            location.href = "GoodsReceiveNote/GRNBOQPOReport?grnBOQPOId=" + data.Id;
        }
    };

    $scope.taxCodCboListWithhold = [];
    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYearWithhold = function (date) {
        $scope.productNew.TaxOptionAddiTax = 'Yes';
        $http({
            method: "Get",
            url: "accounts/TaxCode/GetAdditionalTaxCbo?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message;
                }
                else {
                    $scope.taxCodCboListWithhold = response.data;;
                }
            },
            function errorCallback(response) {
            });
    };

    $scope.getTaxCodeByTaxYearWithhold($scope.productNew.GRNDate);
    $scope.calculateTaxAmountForAdditionalTax = function (data) {
        $scope.advanceTax.TaxAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "TrnAmount") * data / 100).toFixed(2);
    };
    $scope.advanceTaxesList = [];
    $scope.additionalTax = function () {
        for (var i = 0; i < $scope.advanceTaxesList.length; i++) {
            if ($scope.advanceTaxesList[i].TaxCodeId === $scope.advanceTax.TaxCodeId) {
                ShowResult("Tax Already Added");
                return false;
            }

        }

        if (manualValidation("td_TaxCode", baseService.isUndefinedOrNull($scope.advanceTax.TaxCodeId), "Tax Code is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeAmount", baseService.isUndefinedOrNull($scope.advanceTax.TaxAmount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.advanceTax.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.advanceTax.TaxName = $.grep($scope.taxCodCboListWithhold, function (item) {
                return item.Id === $scope.advanceTax.TaxCodeId;
            })[0].UserName;

            $scope.advanceTaxesList.push($scope.advanceTax);
            $scope.advanceTax = {};
        }
        $scope.TotalSumAfterTCS();
    };

    $scope.TotalSumAfterTCS = function () {

        if ($scope.MasterList.length > 0) {
            $scope.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);
        }
        else {
            $scope.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);

        }

    }
    $scope.selectadditionalTax = function () {
        $scope.advanceTax.ValueOfFixed = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].ValueOfFixed;
        $scope.advanceTax.TaxCategoryId = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].TaxCategoryId;
        $scope.advanceTax.Type = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].Type;
        if ($scope.advanceTax.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.advanceTax.ValueOfFixed)) {//* $scope.advanceTax.ValueOfFixed / 100
            if ($scope.Action === 'Save') {
                $scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "ServiceTax"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
            }
            else {
                $scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.MasterList), "ServiceTax"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
            }
        }
        $scope.TotalSumAfterTCS();
    }

    $scope.SaveAdditinalTaxInGRNList = function () {
        $http({
            method: 'POST',
            url: 'Products/InventoryReceive/SaveAdditinalTaxInGRN',
            data:
            {
                'InventoryReceiveId': $scope.productNew.Id,
                'UserSendData': $scope.advanceTaxesList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetAdvanceTaxInfo($scope.productNew.Id);
                $scope.TotalSumAfterTCS();

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.closeReceiveTaxPopUpNew = function (data) {
        if (baseService.isUndefinedOrNull($scope.productId)) {
            $scope.MasterList[$scope.receiveTaxindex].BaseTaxAmount = $filter("sumByKey")($filter("filter")($scope.receiveTaxList), "TaxAmount");
            for (var i = 0; i < $scope.MasterList.length; i++) {


                if ($scope.productNew.IsNonCreditable == 1) {
                    $scope.MasterList[i].TotalMaterialTranAmount = (parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].BaseTaxAmount) + parseFloat($scope.MasterList[i].ServiceCharge) + parseFloat($scope.MasterList[i].ServiceTax)).toFixed(2);
                    $scope.MasterList[i].TotalMaterialBaseAmount = ((parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].BaseTaxAmount) + parseFloat($scope.MasterList[i].ServiceCharge) + parseFloat($scope.MasterList[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

                }
                else {
                    $scope.MasterList[i].TotalMaterialTranAmount = (parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].ServiceCharge)).toFixed(2);
                    $scope.MasterList[i].TotalMaterialBaseAmount = ((parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
                }
            }
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
            $scope.receiveTaxindex = null;
            $scope.GRNDetailRowData = null;
        }
        else {
            $scope.MasterList[$scope.receiveTaxindex].BaseTaxAmount = $filter("sumByKey")($filter("filter")($scope.receiveTaxList), "TaxAmount");
            for (var i = 0; i < $scope.MasterList.length; i++) {


                if ($scope.productNew.IsNonCreditable == 1) {
                    $scope.MasterList[i].TotalMaterialTranAmount = (parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].BaseTaxAmount) + parseFloat($scope.MasterList[i].ServiceCharge) + parseFloat($scope.MasterList[i].ServiceTax)).toFixed(2);
                    $scope.MasterList[i].TotalMaterialBaseAmount = ((parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].BaseTaxAmount) + parseFloat($scope.MasterList[i].ServiceCharge) + parseFloat($scope.MasterList[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

                }
                else {
                    $scope.MasterList[i].TotalMaterialTranAmount = (parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].ServiceCharge)).toFixed(2);
                    $scope.MasterList[i].TotalMaterialBaseAmount = ((parseFloat($scope.MasterList[i].TrnAmount) + parseFloat($scope.MasterList[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
                }
            }
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
            $scope.receiveTaxindex = null;
            $scope.GRNDetailRowData = null;
        }

    }

    $scope.closeReceiveTaxPopUp = function () {
        $scope.detailModel = {};
        $scope.receiveTaxList = [];

        for (var i = 0; i < $scope.MasterList.length; i++) {
            if ($scope.productNew.IsNonCreditable == 1) {
                $scope.MasterList[i].TotalMaterialTranAmount = parseFloat(parseFloat($scope.MasterList[i].TrnAmount).toFixed(2) + parseFloat($scope.MasterList[i].BaseTaxAmount).toFixed(2) + parseFloat($scope.MasterList[i].ServiceCharge).toFixed(2) + parseFloat($scope.MasterList[i].ServiceTax).toFixed(2)).toFixed(2);
                $scope.MasterList[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.MasterList[i].TrnAmount).toFixed(2) + parseFloat($scope.MasterList[i].BaseTaxAmount).toFixed(2) + parseFloat($scope.MasterList[i].ServiceCharge).toFixed(2) + parseFloat($scope.MasterList[i].ServiceTax).toFixed(2)) * $scope.productNew.ToCurrencyRate).toFixed(2);
            }
            else {
                $scope.MasterList[i].TotalMaterialTranAmount = parseFloat($scope.MasterList[i].TrnAmount).toFixed(2) + parseFloat($scope.MasterList[i].ServiceCharge).toFixed(2);
                $scope.MasterList[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.MasterList[i].TrnAmount).toFixed(2) + parseFloat($scope.MasterList[i].ServiceCharge).toFixed(2)) * $scope.productNew.ToCurrencyRate).toFixed(2);
            }
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }
    $scope.calculateTaxAmountForMat = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
        angular.forEach($scope.POMaterialTaxList, function (item) {
            if (item.InventoryReceiveDetailId === data.InventoryReceiveDetailId && item.TaxCategoryId === data.TaxCategoryId) {
                item.TaxAmount = data.TaxAmount;
                item.Percentage = data.Percentage;
            }
        });
    };

    $scope.checkRowValidationMat = function (x) {
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxAmount) || $scope.receiveTaxList[i].TaxAmount === 0) {
                ShowResult("Taxable Amount can not null or zero", 'failure', 'receiveTaxPopUp');
            }
            if ($scope.receiveTaxList[i].Id === x.Id) {
                $scope.receiveTaxList[i].Percentage = ((x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100).toFixed(2);
            }
        }
        angular.forEach($scope.POMaterialTaxList, function (item) {
            if (item.Id === x.Id) {
                item.TaxAmount = x.TaxAmount;
                item.Percentage = x.Percentage;
            }
        });
    }


    $scope.DeleteGRNBOQDetailPopUp = function (data) {
        $scope.tempData = data;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removePopUp')).modal('show');
    };
    $scope.DeleteGRNBOQDetail = function () {
        try {
            $http({
                method: 'POST',
                url: 'Products/GoodsReceiveNote/GRNBOQDetailDelete',
                data:
                {
                    'receiveId': $scope.tempData.InventoryReceiveId,
                    'receiveDetailId': $scope.tempData.InventoryReceiveDetailId,
                },
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    getInventoryMaterialListBOQ($scope.productNew.Id);
                    $scope.tempData = null;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };

    $scope.serviceChargePopUp = function () {
        if ($scope.Action === 'Update') {
            $scope.productNew.TaxOptionService1 = 'Yes';
            if (baseService.arrayLength($scope.MasterList) === 0)
                return ShowResult('Without material charges not aplicable.');
            $scope.serviceModel = {
                Id: null
                , ServiceMasterId: null
                , InventoryReceiveId: $scope.productNew.Id
                , CurrencyName: angular.element("#currency :selected").text()
                , CurrencyId: $scope.productNew.CurrencyId
                , BaseCurrencyId: $scope.baseCurrencyId
                , DocDate: $scope.productNew.DocDate
                , TransactionAmount: 0
                , BaseAmount: 0
                , TotalTaxAmount: 0
                , ToCurrencyRate: $scope.productNew.ToCurrencyRate
                , IsNonCreditable: $scope.productNew.IsNonCreditable
            };
            angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
        }
        else {
            if (baseService.arrayLength($scope.MasterList) === 0)
                return ShowResult('Without material charges not aplicable.');
            $scope.serviceModel = {
                Id: null
                , ServiceMasterId: null
                , InventoryReceiveId: $scope.productNew.Id
                , CurrencyName: angular.element("#currency :selected").text()
                , CurrencyId: $scope.productNew.CurrencyId
                , BaseCurrencyId: $scope.baseCurrencyId
                , DocDate: $scope.productNew.DocDate
                , TransactionAmount: 0
                , BaseAmount: 0
                , TotalTaxAmount: 0
                , ToCurrencyRate: $scope.productNew.ToCurrencyRate
                , IsNonCreditable: $scope.productNew.IsNonCreditable
            };
            angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
        }

    };
    $http.get('Setups/CompanyServiceMaster/GetCboList')
        .then(function (response) {
            $scope.serviceList = response.data;
        });
    $scope.closeServiceChargePopUp = function () {
        $scope.serviceModel = {};
        $scope.receiveTaxList = [];
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    };
    $scope.taxCategoryList = [];
    function getTaxCategoryList(hsnCodeId) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
        });
    }

    $scope.changeService = function () {
        if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
        getTaxCategoryList(hsnCodeId);
    };
    $scope.sumSvcTaxAmount = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
    };

    $scope.serviceSave = function () {
        try {
            $scope.manualValidationAddRemove('div_svc', 'serviceModel', 'ServiceMasterId');
            $scope.manualValidationAddRemove('div_svcRate', 'serviceModel', 'TransactionAmount', 'Amount');

            $http({
                method: 'POST',
                url: $scope.sreviceSaveUrl,
                data: {
                    entity: $scope.serviceModel
                    , taxCategoryList: $scope.taxCategoryList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'serviceChargePopUp');
                    $scope.serviceModel = {
                        Id: null
                        , ServiceMasterId: null
                        , InventoryReceiveId: $scope.productNew.Id
                        , CurrencyName: angular.element("#currency :selected").text()
                        , CurrencyId: $scope.productNew.CurrencyId
                        , BaseCurrencyId: $scope.baseCurrencyId
                        , DocDate: $scope.productNew.DocDate
                        , TransactionAmount: 0
                        , BaseAmount: 0
                        , TotalTaxAmount: 0
                        , ToCurrencyRate: $scope.productNew.ToCurrencyRate
                        , IsNonCreditable: $scope.productNew.IsNonCreditable
                    };
                    $scope.taxCategoryList = [];
                    getServiceChargeList($scope.productNew.Id);
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
            };
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };

    $scope.delModal = function (id) {
        $scope.id = id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removeServicePopUp')).modal('show');
    };
    $scope.serviceDelete = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.sreviceDeleteUrl + $scope.id
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.id = null;
                    getServiceChargeList($scope.productNew.Id);
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };


    function getServiceChargeList(inveReveiveId) {
        $scope.masterId12 = inveReveiveId;
        $http.get($scope.path + 'GetServiceChargeList?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.chargesList = [];
                $scope.chargesList = response.data;
                $scope.getServiceTaxList();

            });
    }

    $scope.calculateSvcTaxCategory = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
    };
    $scope.sumSvcTaxAmount = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
    };

    $scope.calculateTaxAmountForService1 = function (data) {

        if ($scope.Action === 'Update') {
            if (baseService.isUndefinedOrNull(data.Percentage)) {
                data.Percentage = 0;
            }
            data.TaxAmount = Math.round($scope.serviceModel.TransactionAmount * data.Percentage) / 100;
            for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                if ($scope.taxCategoryList[i].Id === data.Id) {
                    $scope.taxCategoryList[i].Percentage = data.Percentage;
                    $scope.taxCategoryList[i].TaxAmount = data.TaxAmount;
                }
            }
        }
    };
    $scope.MaterialTaxUpdate = function () {
        try {
            $http({
                method: 'POST',
                url: 'Products/GoodsReceiveNote/UpdateGRNBOQTax',
                data: {
                    entity: $scope.GRNDetailRowData
                    , taxCategoryList: $scope.receiveTaxList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'receiveTaxPopUp');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
            };
        } catch (e) {
        }
    };


    $scope.summaryUnassignRows = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TransactionQty", dataMember: "TransactionQty", format: "{0:C2}" }],
        showCaptionSummary: true,
    }];

    $scope.GetAdvanceTaxInfo = function (Id) {
        $scope.advanceTaxesList = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/InventoryReceive/GetAdvanceTaxInfo?InventoryReceiveId=' + Id,
        }).then(function successCallback(response) {
            $scope.advanceTaxesList = response.data;

        });
    }

    $scope.removeTaxesRow = function (Id, index) {
        if (baseService.isUndefinedOrNull(Id)) {
            $scope.advanceTaxesList.splice(index, 1);

        }
        else {
            $scope.DeleteAdditinalTax(Id);
        }
    };
    $scope.DeleteAdditinalTax = function (Id) {
        $http({
            method: 'POST',
            url: 'Products/InventoryReceive/AdditionalTaxDelete?Id=' + Id,
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetAdvanceTaxInfo($scope.productNew.Id);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    //#region Document Upload

    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });
    document.getElementById("uploadBtn").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };

    $scope.DocumentSave = function () { 
        if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
            throw $scope.filedata.name + ' File size must be below 2 mb';
        var fileName = null;
        if (!baseService.isUndefinedOrNull($scope.filedata))
            fileName = $scope.filedata.name;
        $scope.productDocMap.UserFilename = fileName;
        $scope.productDocMap.POId = $scope.productNew.Id;
        if (baseService.isUndefinedOrNull($scope.productDocMap.UserFilename)) {
            ShowResult('Select Attachment file');
            return false;
        }
        if (!baseService.isUndefinedOrNull($scope.productDocMap.UserFilename)) {
            if ($scope.productDocMap.UserFilename.length > 50) {
                throw "File Name must be less than 50 character.";
            }
        }
        for (var i = 0; i < $scope.Imagedata.length; i++) {
            var getRow = $filter("filter")($scope.Imagedata, { "UserFilename": $scope.productDocMap.UserFilename });
            if (getRow.length === 1) {
                ShowResult('File Already added');
                return false;
            }
        }
        if (angular.isUndefinedOrNull($scope.productNew.Id))
            ShowResult('Please select/save PO first', 'Error');
        else {
            try {

                var formData = new FormData();

                $http({
                    method: "POST",
                    url: 'Products/PurchaseOrder/GRNPODocCreate',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("PODocumentMap", angular.toJson($scope.productDocMap));
                        if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: {
                        "PODocumentMap": $scope.productDocMap,
                        "file": $scope.filedata,
                        "POId": $scope.productNew.Id,
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.ImagedataLoad();
                        $scope.productDocMap.UserFilename = "";
                        $scope.productDocMap.Description = "";
                        $scope.productDocMap.Remarks = "";
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;

            } catch (e) {
                throw ShowResult(e, "failure");
            }
        }
        return true;
    };
    $scope.Imagedata = [];
    $scope.ImagedataLoad = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseOrder/GRNPODocumentMapData?POID=' + $scope.productNew.Id,
        }).then(function successCallback(response) { //datagatefun
            $scope.Imagedata = response.data;

        });
    };

    $scope.removePopUpForDoc = function (Id) {
        $scope.DocId = Id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removePopUpForDoc')).modal('show');
    };
    $scope.DeleteGRNPOgame = function (Id) {

        if (!baseService.isUndefinedOrNull($scope.DocId)) {
            $http({
                method: 'POST',
                url: 'Products/PurchaseOrder/GRNPOImageDelete?Id=' + $scope.DocId,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ImagedataLoad();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }


    };

    $scope.PODocumentMapDataAll = function () {
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/PurchaseOrder/GRNPODocumentMapDataAll'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.Img = response.data;

        });
    }
    $scope.PODocumentMapDataAll();

    function containsSpecialChars(str) {
        //const specialChars = /[@!#$%^&*()_+\-=\[\]{};':"|,.<>\?`~]/;
        const specialChars = /[@!#$%^&*()_+\=\[\]{};':"|,.<>\?`~]/;
        return specialChars.test(str);
    }

    $scope.CheckSpecialCharecter = function () {
        try {
            if (containsSpecialChars($scope.productNew.DocRefNo)) {
                $scope.productNew.DocRefNo = $scope.productNew.DocRefNo.substring(0, $scope.productNew.DocRefNo.length - 1);
                throw "No special characters allowed for Doc Ref No.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //#endregion 
    $scope.changeCurrencyRate = function () {
        for (var i = 0; i < $scope.poBoqItemListNew.length; i++) {
            $scope.poBoqItemListNew[i].TransactionRate = $scope.poBoqItemListNew[i].TransactionRate / $scope.productNew.ToCurrencyRate;
            $scope.poBoqItemListNew[i].TrnAmount = $scope.poBoqItemListNew[i].TrnAmount / $scope.productNew.ToCurrencyRate;
        } 

    }
}