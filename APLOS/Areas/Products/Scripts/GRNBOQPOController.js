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

    $scope.saveUrl = $scope.path + 'createGRNBYPO';
    $scope.updateUrl1 = $scope.path + 'UpdateGRNBYPO';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'deleteGRNBYPO/';
    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete?receiveDetailId=';
    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
    $scope.updateUrlForSRValue = $scope.path + 'UpdateShortageRejectionValueMap';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

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
        //}
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
    $scope.plantList = [];
    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.productNew.PartyCode = party.Code;
        $scope.productNew.PartyName = party.UserName;
        $scope.productNew.PartyId = party.Id;
        $scope.productNew.PaymentTermId = party.PaymentTermId;
        $scope.productNew.CurrencyId = party.CurrencyId;
        getPartyPlantList();
        $scope.GetCurrencyExchangeRateList();
        $scope.hidePartyPopUp();
    };

    function getPartyPlantList() {
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
    $scope.GetDetails = function () {
        try {
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
            var ArticleIds = parameters[1].Value;
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
            var parameters = [];
            var filteredRecords = [];
            for (var i = 0; i < $scope.DetailList.length; i++) {
                if ($scope.DetailList[i].IsActives) {
                    filteredRecords.push($scope.DetailList[i]);
                }
            }
            parameters.push({ "Key": "POId", "Value": getString(filteredRecords, "POId") });
            parameters.push({ "Key": "ContractId", "Value": getString(filteredRecords, "ContractId") });

            var POId = parameters[0].Value;
            var ContractId = parameters[1].Value;

            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Products/GoodsReceiveNote/GetSelectedItemListDetailsByList',
                data: {
                    'POId': POId,
                    'ContractId': ContractId,
                }
            }).then(function successCallback(response) {
                $scope.MasterList = response.data;
            });
        } catch (e) {
            ShowResult(e, 'info')
        }
    }
}