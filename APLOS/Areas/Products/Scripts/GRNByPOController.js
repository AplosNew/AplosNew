'use strict';
GRNByPOController.$inject = ['addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function GRNByPOController(addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "GRN By PO"; //Inventory Receive
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/GoodsReceiveNote/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getListUrl1 = $scope.path + 'GetListForMasterData';
    $scope.getListUrl2 = $scope.path + 'GetListForMasterData2';

    $scope.saveUrl = $scope.path + 'createGRNBYPO';
    $scope.updateUrl1 = $scope.path + 'UpdateGRNBYPOMaster';
    // $scope.updateUrl1 = $scope.path + 'UpdateGRNBYPO';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'deleteGRNBYPO/';
    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete?receiveDetailId=';
    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
    $scope.updateUrlForSRValue = $scope.path + 'UpdateShortageRejectionValueMap';
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

    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.productId = null;
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
        //debugger;

        if (!baseService.isUndefinedOrNull($scope.CheckedByStatusForNoti) && !baseService.isUndefinedOrNull($scope.ApprovedByStatusForNoti)) {
            $http({
                method: 'GET',
                url: 'Products/InventoryReceive/GetCheckedByAndApprovedBY?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.checkedByList = response.data;
            });

        }
        else {

        }

    }
    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;
    };
    $scope.lst = [];
    $scope.GRNListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/GRNDetailsData'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            window.lst = response.data;

        });
    }
    $scope.GRNListDetails();
    $scope.GRNListDetails();
    $scope.lst1 = [];

    $scope.GRNDocumentMapDataAll = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/GRNDocumentMapDataAll'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            window.Img = response.data;

        });
    }
    $scope.GRNDocumentMapDataAll();

    $scope.data1 = $scope.lst;
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


    $scope.getDataList = function () {
        baseService.init($scope.getListUrl, null, null, "DESC", "GRNDate", "PartyName");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.products = [];
                    $scope.products = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };
    $scope.getDataList();
    $http({
        method: 'GET',
        url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });

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
    $scope.advanceTax = {
        TaxCodeId: null,
        Text: null,
        TaxAmount: null,
        ValueOfFixed: null,
        CompanyCurrencyAmount: null,
        Type: null,
        TaxCategoryId: null,
        TotalSumAfterTCSVal: null
    };
    $scope.productDocMap = {
        Id: null
        , CompanyGroupId: null
        , FileName: null
        , UserFilename: null
        , SystemFileName: null
        , Description: null
        , Remarks: null
    };

    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
    $scope.partyUrl = "";
    $scope.showPartyByGateEntryPopUp = function () {

        if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListByGateEntryANDPO?partyType=' + $scope.partyType;
        }
        else if ($scope.partyType === 'Party') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataByGateEntryListNew';
        }
        else if ($scope.partyType === 'Other') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataByGateEntryListNew';
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


    $scope.productNew = Object.assign({}, $scope.product);

    addressService.getCountryCbo(function (result) {
        $scope.countryList = result;
    });

    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
    }).then(function successCallback(response) {
        $scope.baseCurrencyId = response.data[0].Value;
        $scope.productNew.BaseCurrencyId = response.data[0].Value;
        factoryService.getCurrencyPrecision($scope.baseCurrencyId);
    });

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = result;
    });

    $http.get('accounts/OpeningBalance/GetACCCutOffDate')
        .then(function (response) {
            if (response.data !== null && !baseService.isUndefinedOrNull(response.data.CutOffDate)) {
                $scope.productNew.CutOffDate = response.data.CutOffDate;
                $('#cutOffDate').datepicker('setStartDate', new Date($scope.productNew.CutOffDate));
            }
            else
                ShowResult('Cut Off date not found!', 'failure');
        });

    function containsSpecialChars(str) {
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


    $scope.Get = function (index) {

        $scope.index = index;
        $scope.product = $scope.products[$scope.index];
        $scope.productNew = Object.assign({}, $scope.product);
        // $scope.productNew.GRNDate = data.GRNDate;
        getPartyPlantList();
        getInventoryMaterialList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
       
        $scope.productId = $scope.productNew.Id;
        //$scope.getToCurrencyRate();
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate')
                    $scope.IsBaseOnDueDateEnable = true;
                else
                    $scope.IsBaseOnDueDateEnable = false;
        }
        $scope.Action = 'Save';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.ReqAllocation = function (podetail) {
        $scope.RowLength = $filter("filter")($scope.requisitionListByPo, { 'PODetailId': podetail.PODetailsID });

        for (var j = 0; j < $scope.requisitionListByPo.length; j++) {
            if ($scope.requisitionListByPo[j].PODetailId == podetail.PODetailsID) {
                if ($scope.RowLength.length == 1) {
                    $scope.requisitionListByPo[j].TransactionQty = podetail.TransactionQty;
                    $scope.requisitionListByPoForSave.push($scope.requisitionListByPo[j]);
                }
                else {
                    var tempreqQty = Math.round($filter("sumByKey")($filter("filter")($scope.requisitionListByPo, { PODetailId: podetail.PODetailsID, MaterialMasterId: podetail.MaterialMasterId, 'ArticleId': podetail.ArticleId }), "TransactionQty") * 1000 + Number.EPSILON) / 1000;

                    if (podetail.TransactionQty != tempreqQty) {
                        ShowResult("Please input Requsition Qty of Row Id : " + podetail.PODetailsID + " Material " + podetail.UserName, 'failure');
                        return true;
                        break;
                    } else {

                        $scope.requisitionListByPoForSave.push($scope.requisitionListByPo[j]);
                    }
                }
            }
        }
        return false;
    }

    $scope.AutoBinAllocation = function (podetail) {
        $scope.RowLength = $filter("filter")($scope.binMasterPOList, { 'PODetailId': podetail.PODetailsID });
        if ($scope.RowLength.length == 1 && $scope.RowLength[0].Qty == 0) {
            for (var b = 0; b < $scope.binMasterPOList.where('PODetailId' == podetail.PODetailsID); b++) {
                $scope.binMasterPOList[b].Qty = podetail.TransactionQty;
                $scope.binMasterList.push($scope.binMasterPOList[b]);
            }
        }
        else {
            $scope.totalBinQty = Math.round($filter("sumByKey")($filter("filter")($scope.binMasterPOList, { PODetailId: podetail.PODetailsID, 'MaterialMasterId': podetail.MaterialMasterId, 'ArticleId': podetail.ArticleId }), "Qty") * 1000 + Number.EPSILON) / 1000;
            if ($scope.totalBinQty != podetail.TransactionQty) {
                ShowResult("Please allocate Bin Qty ", 'failure');
                return true;
            }
        }
        return false;
    }
    $scope.binMasterPOList = []
    function GetBinAllocationForPO(inveReveiveId) {
        $scope.masterId = inveReveiveId;
        $http({
            method: 'Post'
            , url: 'Materials/StorageBinAllocation/GetBinAllocationForPO?poId=' + inveReveiveId
        }).then(function (response) {
            $scope.binMasterPOList = response.data;
        });
    }
    $scope.requisitionListByPoForSave = [];
    $scope.checkValidation = function () {
        $scope.checkgridcheckornot = $filter("filter")($scope.inventoryMaterialListPO, { check: true });
        $scope.requisitionListByPoForSave = [];
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
        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            if ($scope.inventoryMaterialListPO[i].TransactionQty > 0 && $scope.inventoryMaterialListPO[i].check == null) {
                ShowResult("Please check in PORowId " + $scope.inventoryMaterialListPO[i].InventoryReceiveDetailId, 'failure');
                return true;
            }
            if ($scope.inventoryMaterialListPO[i].check == true) {
                $scope.inventoryMaterialListPO[i].MaterialStorageId = $scope.productNew.MaterialStorageId;
                if (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].QualityStatus)) {
                    ShowResult("Please select quality statusin PORowId" + $scope.inventoryMaterialListPO[i].InventoryReceiveDetailId, 'failure');
                    return true;
                }
                if ($scope.requisitionListByPo.length > 0) {
                    $scope.ReqAllocation($scope.inventoryMaterialListPO[i]);
                    //Bin Allocation
                    if ($scope.binMasterPOList.length > 0) {
                        $scope.BinRowLength = $filter("filter")($scope.binMasterPOList, { 'PODetailId': $scope.inventoryMaterialListPO[i].PODetailsID });
                        if ($scope.BinRowLength.length > 0) {
                            if ($scope.BinRowLength.length == 1 && $scope.BinRowLength[0].Qty == 0) {
                                for (var b = 0; b < $scope.BinRowLength.where('PODetailId' == $scope.inventoryMaterialListPO[i].PODetailsID); b++) {
                                    $scope.BinRowLength[b].Qty = $scope.inventoryMaterialListPO[i].TransactionQty;
                                    $scope.binMasterList.push($scope.BinRowLength[b]);
                                }
                            }
                        }
                    }

                    $scope.RowLength = $filter("filter")($scope.requisitionListByPoForSave, { PODetailId: $scope.inventoryMaterialListPO[i].PODetailsID, MaterialMasterId: $scope.inventoryMaterialListPO[i].MaterialMasterId, ArticleId: $scope.inventoryMaterialListPO[i].ArticleId });

                    if ($scope.RowLength.length > 0) {
                        $scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialListPO[i]);
                    }
                    else {
                        return true;
                        break;
                    }
                }
                else {
                    //Bin Allocation
                    if ($scope.binMasterPOList.length > 0) {
                        $scope.BinRowLength = $filter("filter")($scope.binMasterPOList, { 'PODetailsID': $scope.inventoryMaterialListPO[i].PODetailsID });
                        if ($scope.BinRowLength.length > 0) {
                            if ($scope.BinRowLength.length == 1 && $scope.BinRowLength[0].Qty == 0) {
                                $scope.BinRowLength[0].Qty = $scope.inventoryMaterialListPO[i].TransactionQty;
                                $scope.binMasterList.push($scope.BinRowLength[0]);
                            }
                        }
                    }
                    $scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialListPO[i]);
                }
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
        if ($scope.binMasterPOList.length > 0) {
            $scope.totalBinQty = Math.round($filter("sumByKey")($filter("filter")($scope.binMasterPOList), "Qty") * 1000 + Number.EPSILON) / 1000;
            $scope.totalTransactionQty = Math.round($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPOnew), "TransactionQty") * 1000 + Number.EPSILON) / 1000;
            if ($scope.totalBinQty != $scope.totalTransactionQty) {
                ShowResult("Please allocate Bin Qty ", 'failure');
                return true;
            }
        }
        return false;
    }

    $scope.TrancastionTypeCboList = [];
    function GetTrancastionTypeCboList() {
        $http({
            method: 'GET',
            url: 'Productions/SalesPurchaseTransactionType/GetPurchaseTypeCbo'
        }).then(function (response) {
            $scope.TrancastionTypeCboList = response.data;
        });
    }
    GetTrancastionTypeCboList();

    $scope.Save = function () {
        //$scope.detailModel.BaseUOMId = $filter("filter")($scope.inventoryMaterialListPO, { check: 1 })[0].Value;
        $scope.product = {};
        if ($scope.Action === 'Save') {
            if (!$scope.checkValidation()) {

                try {
                    if ($scope.Action === 'Update') {
                        $scope.modelValidation('div_grnNo', 'productNew', 'Id');
                        $scope.modelValidation('div_grnDate', 'productNew', 'GRNDate');
                        $scope.modelValidation('div_TT', 'productNew', 'Trancastion Type');
                        $scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');
                        if (baseService.isUndefinedOrNull($scope.productNew.DocRefNo)) {
                            ShowResult("Enter Doc Ref No", 'failure');
                            return false;
                        }
                        if (baseService.isUndefinedOrNull($scope.productNew.DocRefNo)) {
                            ShowResult("Enter Doc Ref No", 'failure');
                            return false;
                        }
                        if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
                            ShowResult("Enter Doc Date", 'failure');
                            return false;
                        }
                        if (baseService.isUndefinedOrNull($scope.productNew.GateEntryNo)) {
                            ShowResult("Select Gate Entry No", 'failure');
                            return false;
                        }
                        if (baseService.isUndefinedOrNull($scope.productNew.EntryDate)) {
                            ShowResult("Enter Gate Entry Date", 'failure');
                            return false;
                        }
                        if (baseService.isUndefinedOrNull($scope.productNew.GRNDate)) {
                            ShowResult("Enter GRN Date", 'failure');
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
                                $scope.modelValidation('div_TT', 'productNew', 'Trancastion Type');
                                manualValidation('div_rate', false);
                                $scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
                                $scope.modelValidation('div_docDate', 'productNew', 'DocDate');
                                $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                                $scope.product = Object.assign({}, $scope.productNew);
                                $scope.product.POId = $scope.POId;
                                $scope.product.PurchaseDocumentAcceptanceId = $scope.AcceptanceId;



                                //debugger;
                                $http({
                                    method: 'POST',
                                    url: $scope.saveUrl,
                                    data:
                                    {
                                        'entity': $scope.product,
                                        'entityMatAndImat': JSON.stringify($scope.inventoryMaterialListPOnew),
                                        'receiveTaxList': $scope.POMaterialTaxList,
                                        'chargesListPO': $scope.chargesListPOnew,
                                        'POServiceTaxList': $scope.POServiceTaxList,
                                        'requisitionDetailList': $scope.requisitionListByPoForSave,
                                        'GRNType': 'GRNBYPO',
                                        'AcceptanceId': $scope.AcceptanceId,
                                        'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                                        'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti,
                                        'grnBinAllocationMap': $scope.binMasterList
                                        //'inventoryMaterialList': $scope.inventoryMaterialList
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
                                        $scope.getDataList();
                                        $scope.GRNListDetails();

                                        $scope.productId = response.data.entity.Id;
                                        $scope.productNew.Id = response.data.entity.Id;
                                        $scope.productNew.msgForAllocationNeed = response.data.entity.msgForAllocationNeed;
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
            }



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
                return ShowResult('Invoicing by is required', 'failure');
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
                for (var i3 = 0; i3 < $scope.inventoryMaterialList.length; i3++) {
                    if ($scope.inventoryMaterialList[i3].check == true) {
                        $scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialList[i3]);
                    }
                    else {

                    }
                }
                for (var i4 = 0; i4 < $scope.chargesList.length; i4++) {
                    if ($scope.chargesList[i4].check == true) {
                        $scope.chargesListPOnew.push($scope.chargesList[i4]);
                    }

                    else {

                    }
                }
                $http({
                    method: 'POST',
                    url: $scope.updateUrl1,
                    data:
                    {
                        'entity': $scope.product,
                        'entityMatAndImat': $scope.inventoryMaterialListPOnew,
                        'receiveTaxList': $scope.MaterialTaxList,
                        'chargesListPO': $scope.chargesListPOnew,
                        'POServiceTaxList': $scope.ServiceTaxList,
                        'GRNType': 'GRNBYPO',
                        'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                        'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.setTabGRNList(1);
                        $scope.getDataList();
                        $scope.GRNListDetails();

                        $scope.productId = response.data.entity.Id;
                        $scope.productNew.Id = response.data.entity.Id;
                        $scope.productNew.msgForAllocationNeed = response.data.entity.msgForAllocationNeed;

                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });




            }
        }
    };

    $scope.Delete = function () {
        //debugger;
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0 && baseService.arrayLength($scope.chargesList) === 0) {
            if (!baseService.isUndefinedOrNull($scope.productNew.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.deleteUrl + $scope.productNew.Id,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult('Data Deleted Successfully', 'success');
                        $scope.getDataList();
                        ClearFields();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
            }
        }
        else
            ShowResult('First delete all line item.', 'failure');
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.SaveButtonDisable = "";
        $scope.Action = "Save";
        $scope.GriddataSelected = [];
        // $scope.product = { POId: $scope.product.POId };
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
            //, POId: $scope.product.POId 

            , GRNDate: $filter("dateFiltering")(Date.now())

        };
        $scope.AcceptanceId = '';
        $scope.inventoryMaterialListPOnew = [];
        $scope.MaterialTaxList = [];
        $scope.chargesListPOnew = [];
        $scope.ServiceTaxList = [];
        $scope.advanceTaxesList = [];
        $scope.TotalSumAfterTCSVal = "";

        // $scope.POId1 = '';
        $scope.NotificationSettingStatus();
        $scope.inventoryMaterialListPO = [];
        $scope.chargesListPO = [];
        $scope.inventoryMaterialList = [];
        $scope.chargesList = [];
        $scope.grossTotal = 0;
        baseService.removeErrorClasses();
        $scope.productDocMap = {
            UserFilename: null
            , Description: null
            , Remarks: null
        };

        $scope.Imagedata = [];
    }
    $scope.changeAllInvoice = function () {
        $scope.productNew.InvoiceNo = null;
        $scope.productNew.InvoiceDate = null;
    };

    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.productNew.PartyCode = party.Code;
        $scope.productNew.PartyName = party.UserName;
        $scope.productNew.PartyId = party.Id;
        $scope.productNew.PaymentTermId = party.PaymentTermId;
        $scope.productNew.CurrencyId = party.CurrencyId;
        $scope.IsBaseOnDueDateEnable = false;
        $scope.productNew.BaseOnDueDate = null;
        $scope.productNew.BaseNoOfDays = null;
        $scope.productNew.MatureDate = null;

        $scope.productNew.TaxApplicable = party.TaxApplicable;
        $scope.productNew.IsTaxApplicableChangeable = party.IsTaxApplicableChangeable;
        if (party.TaxApplicable === 'Mandatory')
            $scope.productNew.IsTaxApplicable = true;
        else
            $scope.productNew.IsTaxApplicable = false;

        if (!baseService.isUndefinedOrNull($scope.productNew.DocDate))
            $scope.changePaymentTerm();
        getPartyPlantList();
        $scope.hidePartyPopUp();
    };

    function getPartyPlantList() {
    }
    function getPartyPlantListPO() {
        $scope.plantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.partyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.IsDefault) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = item.Value;
                    $scope.productNew.InvoicingByAddress = item.Address1;
                    $scope.productNew.DeliveryByAddress = item.Address1;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = item.StateName;
                    $scope.productNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }
    $scope.getToCurrencyRate = function () {
        if (!baseService.isUndefinedOrNull(AcceptanceId)) {
            if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
                $scope.productNew.ToCurrencyRate = 1;
                return;
            }
            $http.get($scope.path + 'GetToCurrencyRate?currencyId=' + $scope.productNew.CurrencyId + '&baseCurrencyId=' + $scope.productNew.BaseCurrencyId + '&docDate=' + $filter('dateFiltering')($scope.productNew.DocDate))
                .then(function (response) {
                    if (parseFloat(response.data) === 0)
                        $scope.productNew.ToCurrencyRate = 1;
                    else
                        $scope.productNew.ToCurrencyRate = response.data;
                });
        }


    };
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

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #region Details
    $scope.detailModelSave = {
        Id: null
        , CountryId: null
        , InventoryReceiveId: $scope.productNew.Id
        , MaterialStorageId: $scope.productNew.MaterialStorageId
        , CurrencyName: angular.element("#currency :selected").text()
        , CurrencyId: $scope.productNew.CurrencyId
        , BaseCurrencyId: $scope.baseCurrencyId
        , DocDate: $scope.productNew.DocDate
        , InventoryMaterialId: null
        , MaterialMasterId: null
        , MaterialMasterName: null
        , ArticleId: null
        , ArticleName: null
        , MaterialType: null
        , OurStyleName: null
        , Description: null
        , MaterialGroupMasterName: null
        , ProductMasterName: null
        , IsOurStyleRequired: false
        , IsProductMstRequired: false

        , FirstCharacteristicsId: null
        , FirstCharacteristicsValueId: null

        , SecondCharacteristicsId: null
        , SecondCharacteristicsValueId: null

        , ThirdCharacteristicsId: null
        , ThirdCharacteristicsValueId: null

        , TransactionQty: null
        , TransactionUoMId: null
        , TransactionRate: 0
        , TransactionAmount: 0
        , BaseQty: null
        , BaseUOMId: null
        , BaseUoM: null
        , BaseUoMFactor: null

        , TotalQty: null
        , TotalAmount: 0
        , TotalTaxAmount: 0
        , AvgRate: null
        , ToCurrencyRate: $scope.productNew.ToCurrencyRate
        , IsNonCreditable: $scope.productNew.IsNonCreditable
        , IsOriginApplicable: false
    };
    $scope.businessProcesses = '';//"BP.BusinessProcessName IN('MaintenanceSpare','BOM','WetProcess','Consumable')";
    $scope.detailPopUp = function () {
        $scope.detailModel = {
            Id: null
            , CountryId: null
            , InventoryReceiveId: $scope.productNew.Id
            , MaterialStorageId: $scope.productNew.MaterialStorageId
            , CurrencyName: angular.element("#currency :selected").text()
            , CurrencyId: $scope.productNew.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.productNew.DocDate
            , InventoryMaterialId: null
            , MaterialMasterId: null
            , MaterialMasterName: null
            , ArticleId: null
            , ArticleName: null
            , MaterialType: null
            , OurStyleName: null
            , Description: null
            , MaterialGroupMasterName: null
            , ProductMasterName: null
            , IsOurStyleRequired: false
            , IsProductMstRequired: false

            , FirstCharacteristicsId: null
            , FirstCharacteristicsValueId: null

            , SecondCharacteristicsId: null
            , SecondCharacteristicsValueId: null

            , ThirdCharacteristicsId: null
            , ThirdCharacteristicsValueId: null

            , TransactionQty: null
            , TransactionUoMId: null
            , TransactionRate: 0
            , TransactionAmount: 0
            , BaseQty: null
            , BaseUOMId: null
            , BaseUoM: null
            , BaseUoMFactor: null

            , TotalQty: null
            , TotalAmount: 0
            , TotalTaxAmount: 0
            , AvgRate: null
            , ToCurrencyRate: $scope.productNew.ToCurrencyRate
            , IsNonCreditable: $scope.productNew.IsNonCreditable
            , IsOriginApplicable: false
        };
        $scope.clearCharNames();
        angular.element(document.querySelector('#detailPopUp')).modal('show');
    };
    $scope.closeDetaiPopUp = function () {
        $scope.detailModel = {};
        $scope.taxCategoryList = [];
        removeValidationMsg();
        angular.element(document.querySelector('#detailPopUp')).modal('hide');
    };

    $scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];
    //$scope.setMaterialMasterData
    $scope.selectMaterialByType = function (ob) {
        $scope.detailModel.MaterialMasterId = ob.Id;
        $scope.detailModel.MaterialMasterName = ob.UserName;
        $scope.detailModel.BaseUOMId = ob.BaseUOMId;
        $scope.detailModel.BaseUoM = ob.BaseUoM;
        $scope.detailModel.OurStyleName = ob.OurStyleName;
        $scope.detailModel.MaterialGroupMasterName = ob.MaterialGroupMasterName;
        $scope.detailModel.ProductMasterName = ob.ProductMasterName;
        $scope.detailModel.IsOurStyleRequired = ob.IsOurStyleRequired;
        $scope.detailModel.IsProductMstRequired = ob.IsProductMstRequired;
        $scope.detailModel.TransactionUoMId = ob.BaseUOMId;
        $scope.detailModel.ArticleId = null;
        $scope.detailModel.ArticleName = null;
        $scope.detailModel.FirstCharacteristicsValueId = null;
        $scope.detailModel.SecondCharacteristicsValueId = null;
        $scope.detailModel.ThirdCharacteristicsValueId = null;
        $scope.detailModel.IsOriginApplicable = ob.IsOriginApplicable;
        $scope.detailModel.CountryId = null;

        $scope.hasArticle = ob.HasAttribute;
        $scope.hasSku = ob.WithSKU;
        $scope.clearCharNames();
        if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
        if (ob.WithSKU) $scope.getCharacteristicsList(ob.Id);

        getTaxCategoryList(ob.HSNCodeId);
        var mmId = []; mmId.push(ob.Id);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
            $scope.uoMList = result;
            //$scope.detailModel.BaseUOMId = $filter("filter")($scope.uoMList, { IsBaseUom: 1 })[0].Value;
        });
        manualValidation('div_mm', false);
        manualValidation('div_country', false);
        $scope.closeMaterialMasterbyTypePopUp();
    };

    $scope.selectarticle = function (ob) {
        try {
            $scope.detailModel.ArticleId = ob.Id;
            $scope.detailModel.ArticleName = ob.StandardName;
            getTaxCategoryList(ob.HSNCodeId);
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

    $scope.detailSave = function () {
        try {
            $scope.validation();
            $scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
            $scope.detailModel.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
            $scope.detailModel.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
            $scope.detailModel.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
            $scope.detailModel.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;
            $scope.detailModel.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
            $scope.detailModel.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;

            for (var i = 0; i < baseService.arrayLength($scope.inventoryMaterialList); i++) {
                if ($scope.detailModel.MaterialMasterId === $scope.inventoryMaterialList[i].MaterialMasterId &&
                    $scope.detailModel.ArticleId === $scope.inventoryMaterialList[i].ArticleId &&
                    $scope.detailModel.FirstCharacteristicsId === $scope.inventoryMaterialList[i].FirstCharacteristicsId &&
                    $scope.detailModel.FirstCharacteristicsValueId === $scope.inventoryMaterialList[i].FirstCharacteristicsValueId &&
                    $scope.detailModel.SecondCharacteristicsId === $scope.inventoryMaterialList[i].SecondCharacteristicsId &&
                    $scope.detailModel.SecondCharacteristicsValueId === $scope.inventoryMaterialList[i].SecondCharacteristicsValueId &&
                    $scope.detailModel.ThirdCharacteristicsId === $scope.inventoryMaterialList[i].ThirdCharacteristicsId &&
                    $scope.detailModel.ThirdCharacteristicsValueId === $scope.inventoryMaterialList[i].ThirdCharacteristicsValueId) {
                    return ShowResult('This material already received');
                }
            }

            $http({
                method: 'POST',
                url: $scope.detailSaveUrl,
                data: {
                    entity: $scope.detailModel
                    , taxCategoryList: $scope.taxCategoryList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'detailPopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'detailPopUp');
                    $scope.detailModel.Id = null;
                    $scope.detailModel = {
                        InventoryReceiveId: $scope.productNew.Id
                        , MaterialStorageId: $scope.productNew.MaterialStorageId
                        , CurrencyName: angular.element("#currency :selected").text()
                        , CurrencyId: $scope.productNew.CurrencyId
                        , BaseCurrencyId: $scope.baseCurrencyId
                        , DocDate: $scope.productNew.DocDate
                        , TotalAmount: 0
                        , TransactionAmount: 0
                        , ToCurrencyRate: $scope.productNew.ToCurrencyRate
                        , IsNonCreditable: $scope.productNew.IsNonCreditable
                        , IsOriginApplicable: false
                    };
                    $scope.taxCategoryList = [];
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                    $scope.clearCharNames();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'detailPopUp');
            };
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };


    $scope.valuePassInDelModal = function (id) {
        $scope.id = id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
    };

    $scope.detailDelete = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.detailDeleteUrl + $scope.id
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.id = null;
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

    $scope.validation = function () {
        $scope.modelValidation('div_mm', 'detailModel', 'MaterialMasterName', 'Material Master');
        if ($scope.hasArticle) $scope.modelValidation('div_ar', 'detailModel', 'ArticleName');
        $scope.manualValidationAddRemove('div_qty', 'detailModel', 'TransactionQty');
        $scope.modelValidation('div_qty', 'detailModel', 'TransactionUoMId', 'UoM is required');
        if ($scope.detailModel.TransactionAmount === 0)
            throw manualValidation('div_tamnt', true, 'Total amount is required.');
        $scope.manualValidationAddRemove('div_tamnt', 'detailModel', 'TransactionAmount');
        if ($scope.detailModel.IsOriginApplicable)
            $scope.manualValidationAddRemove('div_country', 'detailModel', 'CountryId');

        var isSku = false;
        if ($scope.hasSku) {
            if (!baseService.isUndefinedOrNull($scope.char1.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char1.IsMandatory, $scope.char1.FreeText);
            }
            else if (!baseService.isUndefinedOrNull($scope.char2.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char2.IsMandatory, $scope.char2.FreeText);
            }
            else if (!baseService.isUndefinedOrNull($scope.char3.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char3.IsMandatory, $scope.char3.FreeText);
            }
            if (isSku) throw ShowResult('Please insert SKU.', 'failure', 'detailPopUp');
        }
    };
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
    //manualDateValidation
    $scope.modelValidation = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };

    $scope.sumORnot = false;
    function getInventoryMaterialList(inveReveiveId) {
        $scope.masterId5 = inveReveiveId;
        $http.get($scope.path + 'GetInventoryMaterialList?inveReveiveId=' + inveReveiveId + '&POID=' + $scope.POID + '&AcceptanceId=' + $scope.AcceptanceId)
            .then(function (response) {
                $scope.inventoryMaterialList = [];
                $scope.inventoryMaterialList = response.data.Rows;
                $scope.POIDs = $scope.inventoryMaterialList.POId;
                //$scope.productNew.CheckedBy = $scope.inventoryMaterialList[0].CheckedBy;
                $scope.productNew.PODate = $scope.inventoryMaterialList[0].AddedDate;
                checkSameValueInColumnList($scope.inventoryMaterialList, 'TransactionUoM');
                getGrossAmount($scope.inventoryMaterialList, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
                $scope.GetMaterialTaxData();
            });
    }



    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }
    function getTaxCategoryList(hsnCodeId) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
        });
    }

    $scope.calculateTaxCategory = function () {
        $scope.detailModel.TotalTaxAmount = 0;
        var tQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) ? 0 : parseFloat($scope.detailModel.TransactionQty);
        var tAmount = baseService.isUndefinedOrNull($scope.detailModel.TransactionAmount) ? 0 : parseFloat($scope.detailModel.TransactionAmount);
        if (tQty > 0 && tAmount > 0)
            $scope.detailModel.TransactionRate = tAmount / tQty;
        else
            $scope.detailModel.TransactionRate = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.detailModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.detailModel.TotalTaxAmount)) $scope.detailModel.TotalTaxAmount = 0;
    };
    $scope.sumTaxAmount = function () {
        $scope.detailModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
    };
    $scope.getReceiveTaxList = function (data, flag, index, Id) {
        $scope.productNew.TaxOptionAddiTax = 'Yes';
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
        if (data.MaterialTaxList.length > 0) {
            $scope.HSNCode = data.MaterialTaxList[0].HSNCode;
            $scope.receiveTaxList = data.MaterialTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');

    };



    $scope.getTotalReceiveTaxList = function (amount, flag) {
        $scope.taxAbleAmnt = amount;
        $scope.percentageColumn = flag;
        $http({
            method: 'GET',
            url: $scope.path + 'GetTotalReceiveTaxList?receiveId=' + $scope.productNew.Id
        }).then(function (response) {
            $scope.receiveTaxList = response.data;
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
        });
    };
    $scope.closeReceiveTaxPopUp = function () {
        $scope.detailModel = {};
        $scope.receiveTaxList = [];

        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            if ($scope.productNew.IsNonCreditable == 1) {
                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat(parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2)).toFixed(2);
                $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2)) * $scope.productNew.ToCurrencyRate).toFixed(2);
            }
            else {
                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2);
                $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)) * $scope.productNew.ToCurrencyRate).toFixed(2);
            }
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }


    $scope.closeReceiveTaxPopUpValue = function (x) {
        if ($scope.Action === 'Save') {
            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                var row = $filter('filter')($scope.new, { 'PODetailsID': $scope.inventoryMaterialListPO[i].PODetailsID });
                if (row.length != 0) {
                    if ($scope.inventoryMaterialListPO[i].PODetailsID === row[0].PODetailsID) {
                        $scope.inventoryMaterialListPO[i].ShortageRate = row[0].ShortageRate;
                        $scope.inventoryMaterialListPO[i].ShortageValue = row[0].ShortageValue;
                        $scope.inventoryMaterialListPO[i].RejectionRate = row[0].RejectionRate;
                        $scope.inventoryMaterialListPO[i].RejectionValue = row[0].RejectionValue;
                        $scope.inventoryMaterialListPO[i].RejectionClamRate = row[0].RejectionClamRate;
                    }
                    angular.element(document.querySelector('#ValueSet')).modal('hide');
                }
                else {
                    angular.element(document.querySelector('#ValueSet')).modal('hide');
                }

            }
            angular.element(document.querySelector('#ValueSet')).modal('hide');
        }
        else {
            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                var row = $filter('filter')($scope.new1, { 'PODetailsID': $scope.inventoryMaterialList[i].PODetailsID });
                if (row.length != 0) {
                    if ($scope.inventoryMaterialList[i].PODetailsID === row[0].PODetailsID) {
                        $scope.inventoryMaterialList[i].ShortageRate = row[0].ShortageRate;
                        $scope.inventoryMaterialList[i].ShortageValue = row[0].ShortageValue;
                        $scope.inventoryMaterialList[i].RejectionRate = row[0].RejectionRate;
                        $scope.inventoryMaterialList[i].RejectionValue = row[0].RejectionValue;
                        $scope.inventoryMaterialList[i].RejectionClamRate = row[0].RejectionClamRate;
                    }
                    angular.element(document.querySelector('#ValueSet')).modal('hide');
                }
                else {
                    angular.element(document.querySelector('#ValueSet')).modal('hide');
                }
                angular.element(document.querySelector('#ValueSet')).modal('hide');
            }
            angular.element(document.querySelector('#ValueSet')).modal('hide');
        }

    }


    function removeValidationMsg() {
        CloseModalShowResult();
        $scope.clearCharNames();
        manualValidation('div_mm', false);
        manualValidation('div_ar', false);
        manualValidation('div_qty', false);
        manualValidation('div_qty', false);
        manualValidation('div_rate', false);
    }
    function getGrossAmount(list, key1, key2, key3, fieldName) {
        $scope[fieldName] = 0;
        for (var t = 0; t < baseService.arrayLength(list); t++) {
            $scope[fieldName] += parseFloat(list[t][key1]);// + parseFloat(list[t][key2]) + parseFloat(list[t][key3]);
        }
    }

    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getvendorcbo'
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });

    $scope.changePaymentTerm = function () {
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            $scope.productNew.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.productNew.BaseNoOfDays = paymentTerm.NoOfDay;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate') {
                    $scope.productNew.BaseOnDueDate = $filter('dateFiltering')($scope.productNew.DocDate);
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else {
                    $scope.productNew.BaseOnDueDate = null;
                    $scope.IsBaseOnDueDateEnable = false;
                }
            $scope.getMatureDate($scope.productNew.BaseOnDueDate, $scope.productNew.BaseNoOfDays);
        }
    };
    $scope.getMatureDate = function (date, days) {
        if (baseService.isUndefinedOrNull(date)) return $scope.productNew.MatureDate = null;
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.productNew.MatureDate = $filter('date')(date, 'dd-MMM-yyyy');
    };

    $scope.serviceChargePopUp = function () {
        if ($scope.Action === 'Update') {
            $scope.productNew.TaxOptionService1 = 'Yes';
            if ($scope.inventoryMaterialList.length==0)
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
            if ($scope.inventoryMaterialList.length === 0)
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
    $scope.changeService = function () {
        if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
        getTaxCategoryList(hsnCodeId);
    };

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
        angular.element(document.querySelector('#removePopUp')).modal('show');
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

    $scope.inventoryReceiveReport = function (id, reportFormat) {
        if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + id + '&plantId=' + $scope.productNew.PlantId);
    };
    $scope.Griddata = [];
    $scope.getalldata = function () {
        var PoType = 'PO';
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetListOfPO?PoType=' + PoType + '&Status=' + $scope.status + '&vendorId=' + $scope.productNew.PartyId,
        }).then(function successCallback(response) {
            $scope.Griddata = response.data;
            $scope.productNew.GRNDate = $filter("dateFiltering")(Date.now());
        });
    };

    $scope.GetSavedPOListNew = [];
    $scope.GetSavedPOList1 = function (Id) {
        var PoType = 'PO';
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetSavedPOList?GRNId=' + Id,
        }).then(function successCallback(response) {
            //$scope.GetSavedPOListNew = [];
            $scope.GetSavedPOListNew = response.data;
            for (var i = 0; i < $scope.GetSavedPOListNew.length; i++) {
                $scope.GriddataSelected.push($scope.GetSavedPOListNew[i]);
            }

        });
    };

    $scope.status = 'PO';

    $scope.POPopUp = function () {
        $scope.status = 'PO';
        if ($scope.status === 'PO') {
            $scope.status = 'PO';
            //alert('1');
            $scope.productNew.PO = 'PO';
            $scope.getalldata();
        }
        else if ($scope.status === 'Acceptance') {
            $scope.status = 'Acceptance';
            $scope.productNew.PO = 'Acceptance';
            $scope.getalldata();
        }
        angular.element(document.querySelector('#POPopUp')).modal('show');

    };
    $scope.POPopUpNew = function () {
        $scope.getalldata();
        $scope.status === 'PO';
        if ($scope.status === 'PO') {
            $scope.status === 'PO';
            $scope.getalldata();
        }
        else if ($scope.status === 'Acceptance') {
            $scope.status === 'Acceptance';
            $scope.getalldata();
        }
        angular.element(document.querySelector('#POPopUp1')).modal('show');

    };

    $scope.POPopUpCloseNew = function () {
        angular.element(document.querySelector('#POPopUp1')).modal('hide');
    };

    $scope.change = function (e) {
        $scope.status = e;
        $scope.productNew.PO = $scope.status;

    }
    $scope.POPopUpClose = function () {
        angular.element(document.querySelector('#POPopUp')).modal('hide');
    };

    $scope.CheckAll = function (event) {


        var _isselected = event.target.checked;
        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            $scope.inventoryMaterialListPO[i].check = _isselected;
        }
    };

    $scope.CheckAll1 = function (event) {
        var _isselected = event.target.checked;

        for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {

            $scope.inventoryMaterialList[i].check = _isselected;
        }
    };
    $scope.GriddataSelected = [];
    $scope.recorddoubleclick = function ($event) {

        $scope.Griddatatemp = [];
        $scope.Griddatatemp1 = [];
        var partyId = null;
        $scope.tempList = [];
        for (var j = 0; j < $scope.Griddata.length; j++) {
            if ($scope.Griddata[j].Active === true) {
                $scope.Griddata[j].QualityStatus = 'Approved';
                $scope.tempList.push($scope.Griddata[j]);
            }
        }
        var flagTemp = false;
        if ($scope.tempList.length > 0) {
            for (var k = 0; k < $scope.tempList.length; k++) {
                if ($scope.tempList[k].PartyId != $scope.tempList[0].PartyId) {// ||  && $scope.tempList[k].CurrencyId != $scope.tempList[0].CurrencyId
                    flagTemp = true;
                    ShowResult('Have you selected Same vendor?', 'failure', 'POPopUp');
                    return;
                }
                else if ($scope.tempList[k].InvoicingPartyPlantId != $scope.tempList[0].InvoicingPartyPlantId) {// ||  && $scope.tempList[k].CurrencyId != $scope.tempList[0].CurrencyId
                    flagTemp = true;
                    ShowResult('Have you selected Same Invoicing Party?', 'failure', 'POPopUp');
                    return;
                }
                else if ($scope.tempList[k].POType != $scope.tempList[0].POType) {
                    ShowResult('Have you selected Same Types of PO?', 'failure', 'POPopUp');
                    return;
                }
                else if ($scope.tempList[k].CurrencyId != $scope.tempList[0].CurrencyId) {
                    ShowResult('Have you selected Same Currency of PO?', 'failure', 'POPopUp');
                    return;
                }

            }
        }


        if (flagTemp == false) {

            $scope.load();
            for (var x = 0; x < $scope.tempList.length; x++) {
                $scope.GriddataSelected.push($scope.tempList[x]);
            }


        }

        angular.element(document.querySelector('#POPopUp')).modal('hide');
        // }


    }
    $scope.recorddoubleclickforAcceptance = function ($event) {
        $scope.Clear();
        if ($event.data.AcceptanceId != null || $event.data.AcceptanceId != undefined) {
            $scope.AcceptanceId = $event.data.AcceptanceId;
            $scope.productNew.LCDate = $event.data.LCDate;
            $scope.productNew.ContractNo = $event.data.ContractNo;
            if ($event.data.IsNonCreditable === 'Yes') {
                $scope.productNew.IsNonCreditable = true;

            }
            else {
                $scope.productNew.IsNonCreditable = false;

            }
            $scope.load();
            $scope.loadAcceptanceDetail();
            $scope.productNew.TaxOptionAddiTax = 'Yes';
            angular.element(document.querySelector('#POPopUp1')).modal('hide');
        }
    }
    $scope.loadAcceptanceDetailList = [];
    $scope.loadAcceptanceDetail = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/LoadAcceptanceDetails?AcceptanceId=' + $scope.AcceptanceId,
        }).then(function successCallback(response) {


            $scope.loadAcceptanceDetailList = response.data;

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
            //$scope.productId = "";
            $scope.AcceptanceId = $scope.AcceptanceId;
            getPartyPlantList();
            //getPartyPlantEditList();
            GetInventoryMaterialListByPO(id1, $scope.AcceptanceId);

            getServiceChargeListPO(id1);
            $scope.productNew.PO = $scope.status;
            if ($scope.loadAcceptanceDetailList[0].IsNonCreditable === 'Yes') {
                $scope.productNew.IsNonCreditable = true;

            }
            else {
                $scope.productNew.IsNonCreditable = false;


            }
        });
    }

    $scope.requisitionDetailList = [];
    $scope.tempPoDetailId = null;
    $scope.ViewRequisitionDetail = function (poDatailId, poqty, materialmasterIdId, articleId) {
        $scope.requisitionDetailList = [];
        $scope.requisitionDetailList = $filter("filter")($scope.requisitionListByPo, { 'PODetailId': poDatailId, 'MaterialMasterId': materialmasterIdId, 'ArticleId': articleId });
        $scope.tempPoDetailId = poDatailId;
        $scope.tempPoqty = poqty;
        $scope.tempMaterialId = materialmasterIdId;
        $scope.tempArticleId = articleId;
        angular.element(document.querySelector('#ListOfRequisitionPopUP')).modal('show');

    }
    $scope.CloseRequisitionPopUP = function () {


        var qty = Math.round($filter("sumByKey")($filter("filter")($filter("filter")($scope.requisitionListByPo, { 'PODetailId': $scope.tempPoDetailId, 'MaterialMasterId': $scope.tempMaterialId, 'ArticleId': $scope.tempArticleId })), "TransactionQty") * 1000 + Number.EPSILON) / 1000;

        if ($scope.tempPoqty != qty) {
            ShowResult('Requisition Allocation Qty is not equal with GRN Qty?', 'failure', 'ListOfRequisitionPopUP');
        } else {
            angular.element(document.querySelector('#ListOfRequisitionPopUP')).modal('hide');
            $scope.tempPoDetailId = null;
            $scope.tempPoqty = null;
            $scope.tempMaterialId = null;
            $scope.tempArticleId = null;
        }

    }

    $scope.requisitionListByPo = [];
    function GetRequisitionListByPO(poIds) {
        //debugger;
        $http.get($scope.path + 'GetRequsitionQtyListByPO?poIds=' + poIds)
            .then(function (response) {
                $scope.requisitionListByPo = [];
                $scope.requisitionListByPo = response.data.Rows;
            });
    }

    $scope.load = function () {

        if ($scope.AcceptanceId === null || $scope.AcceptanceId === "" || $scope.AcceptanceId === undefined) {

            //#region load
            var id1 = "''";
            var PartyId = '';
            //var PartyCode = '';
            $scope.productNew.DocRefNo = '';
            $scope.productNew.DocDate = '';
            $scope.productNew.DocDate = '';
            $scope.productNew.CurrencyId = '';
            $scope.productNew.DocDate = '';
            $scope.productNew.PartyName = '';
            $scope.GriddataSelected = [];
            for (var i = 0; i < $scope.Griddata.length; i++) {
                if ($scope.Griddata[i].Active === true) {
                    id1 += ",'" + $scope.Griddata[i].Id + "'";
                    //AcceptanceId = $scope.Griddata[i].AcceptanceId;
                    PartyId = $scope.Griddata[i].PartyId;
                    //PartyCode = $scope.Griddata[i].PartyCode;
                    $scope.productNew.PartyName = $scope.Griddata[i].PartyName;
                    //$scope.productNew.DocRefNo = '';//$scope.Griddata[i].DocRefNo;
                    //$scope.productNew.DocDate = '';//$scope.Griddata[i].DocDate;
                    $scope.productNew.CurrencyId = $scope.Griddata[i].CurrencyId;
                    $scope.productNew.ToCurrencyRate = $scope.Griddata[i].ToCurrencyRate;
                    $scope.productNew.IsNonCreditable = $scope.Griddata[i].IsNonCreditable;
                    $scope.productNew.AcceptanceDate = $scope.Griddata[i].AcceptanceDate;

                    $scope.productNew.PaymentTermId = $scope.Griddata[i].PaymentTermId;
                    $scope.productNew.BaseNoOfDays = $scope.Griddata[i].BaseNoOfDays;
                    $scope.productNew.BaseOnDueDate = $scope.Griddata[i].BaseOnDueDate;
                    $scope.productNew.MatureDate = $scope.Griddata[i].MatureDate;

                    $scope.productNew.InvoicingPartyPlantId = $scope.Griddata[i].InvoicingPartyPlantId;
                    $scope.productNew.InvoicingState = $scope.Griddata[i].InvoicingState;
                    $scope.productNew.InvoicingGSTIN = $scope.Griddata[i].GSTIN;
                    $scope.productNew.InvoicingByAddress = $scope.Griddata[i].InvoicingByAddress;

                    $scope.productNew.DeliveryState = $scope.Griddata[i].DeliveryState;
                    $scope.productNew.DeliveryState = $scope.Griddata[i].DeliveryState;
                    $scope.productNew.DeliveryGSTIN = $scope.Griddata[i].GSTIN;
                    $scope.productNew.DeliveryPartyPlantId = $scope.Griddata[i].DeliveryPartyPlantId;
                }
            }
            $scope.productNew.PartyId = PartyId;
            $scope.productNew.partySearchByList = PartyId;
            $scope.productId = "";

            $scope.AcceptanceId = $scope.AcceptanceId;

            getPartyPlantList();
            GetInventoryMaterialListByPO(id1, $scope.AcceptanceId);
            GetBinAllocationForPO(id1);
            GetRequisitionListByPO(id1);
            getServiceChargeListPO(id1);
            $scope.productNew.PO = $scope.status;

        }
        else {
            var gridObj = $("#AcceptanceList").data("ejGrid");
            var data = gridObj.getSelectedRecords()[0];
            var id1 = "''";
            var PartyId = '';
            $scope.productNew.DocRefNo = '';
            $scope.productNew.DocDate = '';
            $scope.productNew.DocDate = '';
            $scope.productNew.CurrencyId = '';
            $scope.productNew.DocDate = '';
            $scope.productNew.PartyName = '';

            $scope.productNew.PartyName = data.PartyName;
            $scope.productNew.DocRefNo = data.DocRefNo;
            $scope.productNew.DocDate = data.DocDate;
            $scope.productNew.CurrencyId = data.CurrencyId;
            $scope.productNew.ToCurrencyRate = data.AcceptanceRate;

            $scope.productNew.AcceptanceDate = data.AcceptanceDate;
            $scope.productNew.VoucherId = data.VoucherId;
            $scope.productNew.InvoiceNo = data.InvoiceNo;
            $scope.productNew.InvoiceDate = data.InvoiceDate;
            $scope.productNew.DueDate = data.DueDate;
            $scope.productNew.PurchaseLCId = data.PurchaseLCId;
            $scope.productNew.ContractId = data.ContractId;
            $scope.productNew.AcceptancePaymentSource = data.AcceptancePaymentSource;
            $scope.productNew.PartyId = data.PartyId;
            $scope.productNew.partySearchByList = data.PartyId;
            $scope.productId = "";
            $scope.AcceptanceId = $scope.AcceptanceId;
            getPartyPlantList();
            //getPartyPlantEditList();
            GetInventoryMaterialListByPO(id1, $scope.AcceptanceId);
            getServiceChargeListPO(null, $scope.AcceptanceId);
            $scope.productNew.PO = $scope.status;
            if (data.IsNonCreditable === 'Yes') {
                $scope.productNew.IsNonCreditable = true;

            }
            else {
                $scope.productNew.IsNonCreditable = false;


            }

        }

    }


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

    $scope.getReceiveTaxListPOValueSet = function (data, flag, index, Id) {

        if ($scope.Action === 'Save') {
            $scope.ShortageRate = '';
            $scope.ShortageValue = '';
            $scope.RejectionRate = '';
            $scope.RejectionValue = '';
            $scope.RejectionClamRate = '';
            $scope.MaterialGroupMasterName = data.MaterialGroupMasterName;
            $scope.UserName = data.UserName;
            $scope.StandardName = data.StandardName;
            $scope.FirstCharacteristicsValue = data.FirstCharacteristicsValue;
            $scope.SecondCharacteristicsValue = data.SecondCharacteristicsValue;
            $scope.ThirdCharacteristicsValue = data.ThirdCharacteristicsValue;

            $scope.TransactionRate = data.TransactionRate;
            $scope.ShortageQty = data.ShortageQty;
            $scope.RejectionQty = data.RejectionQty;

            $scope.PODetailsID = data.PODetailsID;
            $scope.ShortageRate = data.ShortageRate;
            $scope.ShortageValue = data.ShortageValue;
            $scope.RejectionRate = data.RejectionRate;
            $scope.RejectionValue = data.RejectionValue;
            $scope.RejectionClamRate = data.RejectionClamRate;

            angular.element(document.querySelector('#ValueSet')).modal('show');
        }
        else {
            $scope.ShortageRate = '';
            $scope.ShortageValue = '';
            $scope.RejectionRate = '';
            $scope.RejectionValue = '';
            $scope.RejectionClamRate = '';
            $scope.MaterialGroupMasterName = data.MaterialGroupMasterName;
            $scope.UserName = data.UserName;
            $scope.StandardName = data.StandardName;
            $scope.FirstCharacteristicsValue = data.FirstCharacteristicsValue;
            $scope.SecondCharacteristicsValue = data.SecondCharacteristicsValue;
            $scope.ThirdCharacteristicsValue = data.ThirdCharacteristicsValue;

            $scope.TransactionRate = data.TransactionRate;
            $scope.ShortageQty = data.ShortageQty;
            $scope.RejectionQty = data.RejectionQty;

            $scope.PODetailsID = data.InventoryReceiveDetailId;
            $scope.ShortageRate = data.ShortageRate;
            $scope.ShortageValue = data.ShortageValue;
            $scope.RejectionRate = data.RejectionRate;
            $scope.RejectionValue = data.RejectionValue;
            $scope.RejectionClamRate = data.RejectionClamRate;

            angular.element(document.querySelector('#ValueSet')).modal('show');
        }


    };
    $scope.getReceiveTaxListPOValueSet1 = function (data, flag, index, Id) {
        if ($scope.Action === 'Save') {
            $scope.new = [];
            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                if ($scope.inventoryMaterialListPO[i].check === true) {
                    if ($scope.inventoryMaterialListPO[i].ShortageQty > 0 || $scope.inventoryMaterialListPO[i].RejectionQty > 0) {
                        $scope.new.push($scope.inventoryMaterialListPO[i]);
                    }
                }
            }

            for (var i = 0; i < $scope.new.length; i++) {
                if ($scope.new[i].check == true) {
                    if ($scope.new[i].ShortageQty > 0 || $scope.new[i].RejectionQty > 0) {
                        $scope.new[i].ShortageRate = 110;
                        $scope.new[i].ShortageValue = (($scope.new[i].ShortageQty * $scope.new[i].ShortageRate) / 100) * $scope.new[i].TransactionRate;
                        $scope.new[i].RejectionRate = 50;
                        $scope.new[i].RejectionValue = (($scope.new[i].RejectionQty * $scope.new[i].RejectionRate) / 100) * $scope.new[i].TransactionRate;
                        $scope.new[i].RejectionClamRate = (100 - $scope.new[i].RejectionRate);

                    }
                }
            }

            angular.element(document.querySelector('#ValueSet')).modal('show');
        }
        else {
            $scope.new1 = [];

            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                if ($scope.inventoryMaterialList[i].check === true) {
                    if ($scope.inventoryMaterialList[i].ShortageQty > 0 || $scope.inventoryMaterialList[i].RejectionQty > 0) {
                        $scope.new1.push($scope.inventoryMaterialList[i]);
                    }
                }
            }

            for (var i = 0; i < $scope.new1.length; i++) {
                if ($scope.new1[i].check == true) {
                    if ($scope.new1[i].ShortageQty > 0 || $scope.new1[i].RejectionQty > 0) {
                        $scope.new1[i].ShortageRate = 110;
                        $scope.new1[i].ShortageValue = (($scope.new1[i].ShortageQty * $scope.new1[i].ShortageRate) / 100) * $scope.new1[i].TransactionRate;
                        $scope.new1[i].RejectionRate = 50;
                        $scope.new1[i].RejectionValue = (($scope.new1[i].RejectionQty * $scope.new1[i].RejectionRate) / 100) * $scope.new1[i].TransactionRate;
                        $scope.new1[i].RejectionClamRate = (100 - $scope.new1[i].RejectionRate);
                    }
                }
            }

            angular.element(document.querySelector('#ValueSet')).modal('show');
        }


    };
    $scope.CalculateShortageVal = function (x) {
        for (var i = 0; i < $scope.newList.length; i++) {
            $scope.newList[i].ShortageValue = (($scope.newList[i].ShortageQty * $scope.newList[i].ShortageRate) / 100) * $scope.newList[i].TransactionRate;
        }


    }
    $scope.CalculateRejectionVal = function () {
        for (var i = 0; i < $scope.newList.length; i++) {
            $scope.newList[i].RejectionValue = (($scope.newList[i].RejectionQty * $scope.newList[i].RejectionRate) / 100) * $scope.newList[i].TransactionRate;
            $scope.newList[i].RejectionClamRate = (100 - $scope.newList[i].RejectionRate);

        }
    }
    function GetInventoryMaterialListByPO(inveReveiveId) {
        $scope.masterId = inveReveiveId;
        $http.get($scope.path + 'GetInventoryMaterialListByOnlyPO?inveReveiveId=' + inveReveiveId + '&AcceptanceId=' + $scope.AcceptanceId)
            .then(function (response) {
                $scope.inventoryMaterialListPO = [];
                $scope.inventoryMaterialListPO = response.data.Rows;
                //$scope.POID = $scope.inventoryMaterialListPO.POID;
                //$scope.PreBal = $scope.inventoryMaterialListPO.Balance;
                //$scope.PODetailsID = $scope.inventoryMaterialListPO.InventoryReceiveDetailId;
                $scope.productNew.InvoicingByAddress = $scope.inventoryMaterialListPO[0].InvoicingByAddress;
                $scope.productNew.DeliveryByAddress = $scope.inventoryMaterialListPO[0].DeliveryByAddress;
                $scope.inventoryMaterialListPO.BaseAmount = '0';
                checkSameValueInColumnList($scope.inventoryMaterialListPO, 'TransactionUoM');
                getGrossAmount($scope.inventoryMaterialListPO, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
                $scope.GetPOMaterialTaxData();
            });
    }


    $scope.binMasterList = [];
    $scope.GetbinAllocationPopUp = function (data) {
        $scope.PODetailsID = data.InventoryReceiveDetailId;
        $scope.POData = data;
        for (var i = 0; i < $scope.binMasterPOList.length; i++) {
            if ($scope.binMasterPOList[i].PODetailsID == $scope.PODetailsID) {
                var getRow = $filter("filter")($scope.binMasterList, { "PODetailsID": $scope.PODetailsID, "BinCode": $scope.binMasterPOList[i].BinCode, "BinReference": $scope.binMasterPOList[i].BinReference });
                var getbinRow = $filter("filter")($scope.binMasterPOList, { "PODetailsID": $scope.PODetailsID });

                if (getRow.length == 0 && getbinRow.length == 1) {
                    $scope.binMasterPOList[i].Qty = data.TransactionQty;
                    $scope.binMasterList.push($scope.binMasterPOList[i])
                }
                else if (getRow.length == 0) {
                    $scope.binMasterList.push($scope.binMasterPOList[i])
                }
            }
        }
        angular.element(document.querySelector('#binAllocationPopUp')).modal('show');
    }

    $scope.selectedBinAllocationList = [];
    $scope.CloseBinAllocationPopUp = function () {
        if ($scope.binMasterList.length > 0) {
            for (var b = 0; b < $scope.binMasterList.length; b++) {
                if ($scope.binMasterList[b].PODetailsID == $scope.PODetailsID && $scope.POData.TransactionQty > 0) {
                    $scope.totalBinQty = Math.round($filter("sumByKey")($filter("filter")($scope.binMasterList, { 'PODetailsID': $scope.PODetailsID }), "Qty") * 1000 + Number.EPSILON) / 1000;
                    if ($scope.POData.TransactionQty != $scope.totalBinQty) {
                        ShowResult("Bin Qty cann't less than Transaction Qty", 'failure', 'binAllocationPopUp');
                        angular.element(document.querySelector('#binAllocationPopUp')).modal('show');
                    }
                    else {
                        angular.element(document.querySelector('#binAllocationPopUp')).modal('hide');
                    }
                }
            }
        }
        else {
            angular.element(document.querySelector('#binAllocationPopUp')).modal('hide');
        }
    }

    $scope.GetPOMaterialTaxData = function () {
        $scope.POMaterialTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetReceiveTaxListPO?receiveDetailId=' + $scope.masterId
        }).then(function (response) {
            $scope.POMaterialTaxList = response.data;

            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                var linepk = $scope.inventoryMaterialListPO[i].InventoryReceiveDetailId;
                var list = getPOMaterialtaxlist(linepk);
                $scope.inventoryMaterialListPO[i].POMaterialTaxList = list;
            }
        });
    };
    function getPOMaterialtaxlist(linepk) {
        var result = [];
        for (var i = 0; i < $scope.POMaterialTaxList.length; i++) {
            if ($scope.POMaterialTaxList[i].PODetailId === linepk) {
                result.push($scope.POMaterialTaxList[i]);
            }
        }
        return result;
    }

    $scope.GetMaterialTaxData = function () {
        $scope.MaterialTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + $scope.masterId5
        }).then(function (response) {
            $scope.MaterialTaxList = response.data;

            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                var linepk = $scope.inventoryMaterialList[i].InventoryReceiveDetailId;
                var list = getMaterialtaxlist(linepk);
                $scope.inventoryMaterialList[i].MaterialTaxList = list;
            }
        });
    };
    function getMaterialtaxlist(linepk) {
        var result4 = [];
        for (var i = 0; i < $scope.MaterialTaxList.length; i++) {
            if ($scope.MaterialTaxList[i].PODetailId === linepk) {
                result4.push($scope.MaterialTaxList[i]);
            }
        }
        return result4;
    }

    function getServiceChargeListPO(inveReveiveId) {
        $scope.inveReveiveId = inveReveiveId;
        $http.get($scope.path + 'GetServiceChargeListPO?receiveId=' + inveReveiveId + '&AcceptanceId=' + $scope.AcceptanceId)
            .then(function (response) {
                $scope.chargesListPO = [];
                $scope.chargesListPO = response.data;
                $scope.GetPOServiceTaxData();
            });
    }
    $scope.GetPOServiceTaxData = function () {
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
    function getPOServicetaxlist(linepk1) {
        var result1 = [];
        for (var i = 0; i < $scope.POServiceTaxList.length; i++) {
            if ($scope.POServiceTaxList[i].InventoryServiceId === linepk1) {
                result1.push($scope.POServiceTaxList[i]);
            }
        }
        return result1;
    }
    function getServicetaxlist1(linepk111) {
        var result11 = [];
        for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
            if ($scope.ServiceTaxList[i].InventoryServiceId === linepk111) {
                result11.push($scope.ServiceTaxList[i]);
            }
        }
        return result11;
    }

    $scope.getServiceTaxList = function () { //,data, flag)

        $http({
            method: 'GET',
            url: $scope.path + 'GetServiceTaxList?serviceId=' + $scope.masterId12//data.Id
        }).then(function (response) {
            $scope.ServiceTaxList = response.data;
            for (var i = 0; i < $scope.chargesList.length; i++) {
                var linepk1 = $scope.chargesList[i].Id;
                var list11 = getServicetaxlist1(linepk1);
                $scope.chargesList[i].ServiceTaxList = list11;
            }
        });
        
    }

    $scope.getServiceTaxListPOPOP1 = function (data, flag, Id, index) {
        $scope.productNew.TaxOptionService = 'Yes';
        $scope.ServiceAddindex = index;
        $scope.taxAbleAmnt = data.Amount;
        $scope.percentageColumn = flag;
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        //$scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.ServiceTaxList = [];
        if (data.ServiceTaxList.length > 0) {
            $scope.HSNCode = data.ServiceTaxList[0].HSNCode;
            $scope.ServiceTaxList = data.ServiceTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.ServiceTaxList.length; j++) {
            $scope.total = $scope.total + $scope.ServiceTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#ServiceTaxPopUp')).modal('show');
    }

    $scope.index = -1;
    $scope.staus = true;
    $scope.enableid = true;
    $scope.Tabshow = false;
    $scope.TabChabge = function () {
        var getRow = $filter("filter")($scope.inventoryMaterialListPO, { "check": true });
        if (getRow.length > 0) {
            $scope.Tabshow = true;
        }
        else {
            $scope.Tabshow = false;
        }
    }
    $scope.Tabshow1 = false;
    $scope.TabChabge1 = function () {
        var getRow = $filter("filter")($scope.inventoryMaterialList, { "check": true });
        if (getRow.length > 0) {
            $scope.Tabshow1 = true;
        }
        else {
            $scope.Tabshow1 = false;
        }
    }
    $scope.Change = function (event, index, x) {
        if (baseService.isUndefinedOrNull(x.TransactionQty)) {
            ShowResult('Enter the current qty', 'failure');
        }
        else {
            if (event.currentTarget.checked) {
                $scope.inventoryMaterialListPO[index].check = true;
                $scope.index = index;
                //$scope.staus = false;
                x.enableid = false;

                if (x.POQty === (x.GRNRcvQty + x.TransactionQty)) {
                    x.POClosStatus = true;
                }
                else if (x.POQty < (x.GRNRcvQty + x.TransactionQty) && x.Tolerance > 0) {// Condition is added for if receive more qty
                    x.POClosStatus = true;
                }

                else if (x.POQty > (x.GRNRcvQty + x.TransactionQty)) {
                    $scope.PODetailId = x.PODetailId;
                    $scope.message = 'Are you want to close this PO line item?';
                    angular.element(document.querySelector('#ConfirmationForReqClosePopUp')).modal('show');
                }

            }
            else {
                $scope.inventoryMaterialListPO[index].check = false;
                x.enableid = true;
                //$scope.index = index;
                x.POClosStatus = false;
                x.TransactionQty = "";
                x.Balance = x.POQty - x.GRNRcvQty;//parseFloat(x.POQty - x.GRNRcvQty).toFixed(2);
            }


        }
        $scope.TabChabge();

    }
    $scope.YesMessageForClosed = function ($event) {
        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            if ($scope.inventoryMaterialListPO[i].check === true) {
                if ($scope.inventoryMaterialListPO[i].PODetailId === $scope.PODetailId) {
                    $scope.inventoryMaterialListPO[i].POClosStatus = true;
                }
            }
            else {
                $scope.inventoryMaterialListPO[i].POClosStatus = false;
            }
        }
    }
    $scope.NoMessageForClosed = function ($event) {
        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            if ($scope.GetListForMasterOrder[i].check === true) {
                if ($scope.GetListForMasterOrder[i].PODetailId === $scope.PODetailId) {
                    $scope.inventoryMaterialListPO[i].POClosStatus = false;
                }
            }
            else {
                $scope.GetListForMasterOrder[i].WantToClose = false;
            }
        }
    }

    $scope.calculateRate = function (data, event) {
        data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
        if (data.TransactionRate === 'NaN')
            data.TransactionRate = 0;
        data.BaseTaxAmount = 0;
        angular.forEach(data.POMaterialTaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;

            data.BaseTaxAmount += item.TaxAmount;
        });
        data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;


    };
    $scope.calculateAmount = function (data, index) {
        if (baseService.isUndefinedOrNull(data.PurchaseDocumentAcceptanceId)) {

            var count = 0;
            for (var j = 0; j < $scope.inventoryMaterialListPO.length; j++) {
                if ($scope.inventoryMaterialListPO[j].TransactionQty > 0) {
                    count++;
                }
                else {
                    $scope.inventoryMaterialListPO[j].ServiceCharge = 0;
                    $scope.inventoryMaterialListPO[j].ServiceTax = 0;
                    $scope.inventoryMaterialListPO[j].TrnAmount = 0;
                    $scope.inventoryMaterialListPO[j].TotalMaterialTranAmount = 0;
                    $scope.inventoryMaterialListPO[j].TotalMaterialTranAmount = 0;
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
            var TotalTrnAmount = Math.round($filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount') * 100 + Number.EPSILON) / 100;
            var TotalServiceTaxAmount = Math.round($filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount') * 100 + Number.EPSILON) / 100;
            var tempServiceAmount = 0;
            var tempServiceTaxAmount = 0;
            var newcount = 0;
            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                if ($scope.inventoryMaterialListPO[i].TransactionQty > 0) {
                    newcount++;
                    $scope.inventoryMaterialListPO[i].Balance = '';
                    var ToleranceQty = $scope.inventoryMaterialListPO[i].POQty + $scope.inventoryMaterialListPO[i].ToleranceQty;
                    var newpoQty = $scope.inventoryMaterialListPO[i].POQty + $scope.inventoryMaterialListPO[i].ToleranceQty;
                    if ($scope.inventoryMaterialListPO[i].POQty < (parseFloat($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2)) && (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].ToleranceQty) || $scope.inventoryMaterialListPO[i].ToleranceQty === 0)) {
                        //$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty);
                        $scope.inventoryMaterialListPO[i].TransactionQty = '';
                        ShowResult('Current quantity can not grater than balance qty!', 'failure');
                        return false;
                    }

                    else if (newpoQty < (parseFloat($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2)) && (!baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].Tolerance) || $scope.inventoryMaterialListPO[i].Tolerance > 0)) {
                        $scope.inventoryMaterialListPO[i].TransactionQty = '';
                        $scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty + $scope.inventoryMaterialListPO[i].ToleranceQty;
                        ShowResult('Current quantity can not grater than  Balance Qty !', 'failure');
                        return false;
                    }
                    else if ($scope.inventoryMaterialListPO[i].ShortageQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
                        ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
                        return false;
                    }
                    else if ($scope.inventoryMaterialListPO[i].RejectionQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
                        ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
                        return false;
                    }
                    else {

                        if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
                            $scope.inventoryMaterialListPO[i].TrnAmount = Math.round(data.TrnAmount * 100 + Number.EPSILON) / 100;
                            angular.forEach(data.POMaterialTaxList, function (item) {
                                item.TaxAmount = Math.round(((data.TrnAmount * item.Percentage) / 100) * 100 + Number.EPSILON) / 100;
                            });

                            $scope.inventoryMaterialListPO[i].BaseTaxAmount = Math.round($filter('sumByKey')($filter('filter')(data.POMaterialTaxList, { "PODetailId": data.PODetailsID }), 'TaxAmount') * 100 + Number.EPSILON) / 100;

                            if (TotalServiceAmount > 0) {
                                //$scope.inventoryMaterialListPO[i].BaseTaxAmount = (($scope.inventoryMaterialListPO[i].TotalTaxAmount / $scope.inventoryMaterialListPO[i].POQty) * $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2);
                                if (count > newcount) {
                                    $scope.inventoryMaterialListPO[i].ServiceCharge = Math.round(((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount) * 100 + Number.EPSILON) / 100;
                                    $scope.inventoryMaterialListPO[i].ServiceTax = Math.round(((TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount) * 100 + Number.EPSILON) / 100;

                                }
                                else if (count == newcount) {
                                    $scope.inventoryMaterialListPO[i].ServiceCharge = 0;
                                    tempServiceAmount = Math.round($filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'ServiceCharge') * 100 + Number.EPSILON) / 100;
                                    $scope.inventoryMaterialListPO[i].ServiceCharge = Math.round((TotalServiceAmount - tempServiceAmount) * 100 + Number.EPSILON) / 100;
                                    $scope.inventoryMaterialListPO[i].ServiceTax = 0;
                                    tempServiceTaxAmount = Math.round($filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'ServiceTax') * 100 + Number.EPSILON) / 100;
                                    $scope.inventoryMaterialListPO[i].ServiceTax = Math.round((TotalServiceTaxAmount - tempServiceTaxAmount) * 100 + Number.EPSILON) / 100
                                }

                            }

                            $scope.inventoryMaterialListPO[i].Balance = (($scope.inventoryMaterialListPO[i].POQty + $scope.inventoryMaterialListPO[i].ToleranceQty) - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                            //$scope.inventoryMaterialListPO[i].ShortageQty = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                            $scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                            //$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].RejectionQty);
                            $scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);

                        }
                        else {
                            if (TotalServiceAmount > 0) {
                                if (count > newcount) {
                                    $scope.inventoryMaterialListPO[i].ServiceCharge = Math.round(((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount) * 100 + Number.EPSILON) / 100;
                                    $scope.inventoryMaterialListPO[i].ServiceTax = Math.round(((TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount) * 100 + Number.EPSILON) / 100;

                                }
                                else if (count == newcount) {
                                    $scope.inventoryMaterialListPO[i].ServiceCharge = 0;
                                    tempServiceAmount = Math.round($filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'ServiceCharge') * 100 + Number.EPSILON) / 100;
                                    $scope.inventoryMaterialListPO[i].ServiceCharge = Math.round((TotalServiceAmount - tempServiceAmount) * 100 + Number.EPSILON) / 100;
                                    $scope.inventoryMaterialListPO[i].ServiceTax = 0;
                                    tempServiceTaxAmount = Math.round($filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'ServiceTax') * 100 + Number.EPSILON) / 100;
                                    $scope.inventoryMaterialListPO[i].ServiceTax = Math.round((TotalServiceTaxAmount - tempServiceTaxAmount) * 100 + Number.EPSILON) / 100
                                }

                            }
                            $scope.inventoryMaterialListPO[i].Balance = (($scope.inventoryMaterialListPO[i].POQty + $scope.inventoryMaterialListPO[i].ToleranceQty) - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                            //$scope.inventoryMaterialListPO[i].ShortageQty = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty+$scope.inventoryMaterialListPO[i].TransactionQty));
                            $scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                            //$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].RejectionQty);
                            $scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);
                        }
                        if ($scope.productNew.IsNonCreditable == 1) {
                            $scope.inventoryMaterialListPO[i].TrnAmount = ($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate).toFixed(2);
                            $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount)) * 100 + Number.EPSILON) / 100;
                            $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
                        }
                        else {
                            $scope.inventoryMaterialListPO[i].TrnAmount = Math.round(($scope.inventoryMaterialListPO[i].TransactionQty * $scope.inventoryMaterialListPO[i].TransactionRate) * 100 + Number.EPSILON) / 100;
                            $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * 100 + Number.EPSILON) / 100;
                            $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
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
            var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
            var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount');

            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                if ($scope.inventoryMaterialListPO[i].TransactionQty > 0) {
                    $scope.inventoryMaterialListPO[i].Balance = '';
                    var ToleranceQty = $scope.inventoryMaterialListPO[i].POQty * $scope.inventoryMaterialListPO[i].Tolerance / 100;
                    var newpoQty = $scope.inventoryMaterialListPO[i].POQty + ToleranceQty;

                    if ($scope.inventoryMaterialListPO[i].ShortageQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
                        ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
                        return false;
                    }
                    else if ($scope.inventoryMaterialListPO[i].RejectionQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
                        ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
                        return false;
                    }
                    else {
                        if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
                            $scope.inventoryMaterialListPO[i].TrnAmount = Math.round(data.TrnAmount * 100 + Number.EPSILON) / 100;
                            if (TotalServiceAmount > 0) {
                                $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                                $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                            }

                            $scope.inventoryMaterialListPO[i].Balance = (($scope.inventoryMaterialListPO[i].POQty + $scope.inventoryMaterialListPO[i].ToleranceQty) - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                            //$scope.inventoryMaterialListPO[i].ShortageQty = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                            $scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                            //$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].RejectionQty);
                            $scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);

                        }
                        else {
                            //$scope.inventoryMaterialListPO[i].BaseTaxAmount = (($scope.inventoryMaterialListPO[i].TotalTaxAmount / $scope.inventoryMaterialListPO[i].POQty) * $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2);
                            if (TotalServiceAmount > 0) {
                                $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                                $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                            }
                            $scope.inventoryMaterialListPO[i].Balance = (($scope.inventoryMaterialListPO[i].POQty + $scope.inventoryMaterialListPO[i].ToleranceQty) - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                            //$scope.inventoryMaterialListPO[i].ShortageQty = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty+$scope.inventoryMaterialListPO[i].TransactionQty));
                            $scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                            //$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].RejectionQty);
                            $scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);
                        }
                        if ($scope.productNew.IsNonCreditable == 1) {
                            $scope.inventoryMaterialListPO[i].TrnAmount = ($scope.inventoryMaterialListPO[i].TransactionQty * $scope.inventoryMaterialListPO[i].TransactionRate).toFixed(2);
                            $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * 100 + Number.EPSILON) / 100;
                            $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
                        }
                        else {
                            $scope.inventoryMaterialListPO[i].TrnAmount = Math.round(($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate) * 100 + Number.EPSILON) / 100;
                            $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * 100 + Number.EPSILON) / 100;
                            $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
                        }
                    }
                }
            }
            angular.forEach(data.POMaterialTaxList, function (item) {
                item.TaxAmount = Math.round(((data.TrnAmount * item.Percentage) / 100) * 100 + Number.EPSILON) / 100;
            });

            for (var i1 = 0; i1 < $scope.inventoryMaterialListPO.length; i1++) {
                if ($scope.inventoryMaterialListPO[i1].PODetailsID == data.PODetailsID) {
                    $scope.inventoryMaterialListPO[i1].BaseTaxAmount = Math.round($filter('sumByKey')($filter('filter')(data.POMaterialTaxList, { "PODetailId": data.PODetailsID }), 'TaxAmount') * 100 + Number.EPSILON) / 100;
                }
            }
        }

    };

    $scope.calculateAmount1 = function (data) {
        data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesList), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TrnAmount');
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');
        for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
            if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId == data.InventoryReceiveDetailId) {
                $scope.inventoryMaterialList[i].TrnAmount = data.TrnAmount;
                $scope.inventoryMaterialList[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount;
                $scope.inventoryMaterialList[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount;
                $scope.inventoryMaterialList[i].Balance = (($scope.inventoryMaterialList[i].POQty + $scope.inventoryMaterialList[i].ToleranceQty) - ($scope.inventoryMaterialList[i].OtherReceived + $scope.inventoryMaterialList[i].TransactionQty));
                //$scope.inventoryMaterialList[i].ShortageQty = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].OtherReceived + $scope.inventoryMaterialList[i].TransactionQty));
                //$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                //$scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].RejectionQty);
                $scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
                $scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty));
                data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
                if (data.TrnAmount == 'NaN')
                    data.TrnAmount = 0;
                data.TaxAmount = 0;
                data.BaseTaxAmount = 0;
                angular.forEach(data.MaterialTaxList, function (item) {
                    item.TaxAmount = data.TrnAmount * item.Percentage / 100;
                    data.BaseTaxAmount += item.TaxAmount;
                });
            }
            else {
                $scope.inventoryMaterialList[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount;
                $scope.inventoryMaterialList[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount;
                $scope.inventoryMaterialList[i].Balance = (($scope.inventoryMaterialList[i].POQty + $scope.inventoryMaterialList[i].ToleranceQty) - ($scope.inventoryMaterialList[i].OtherReceived + $scope.inventoryMaterialList[i].TransactionQty));
                //$scope.inventoryMaterialList[i].ShortageQty = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].OtherReceived + $scope.inventoryMaterialList[i].TransactionQty));
                //$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                //$scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].RejectionQty);
                $scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
                $scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty));
                data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
                if (data.TrnAmount == 'NaN')
                    data.TrnAmount = 0;
                data.TaxAmount = 0;
                data.BaseTaxAmount = 0;
                angular.forEach(data.MaterialTaxList, function (item) {
                    item.TaxAmount = data.TrnAmount * item.Percentage / 100;
                    data.BaseTaxAmount += item.TaxAmount;
                });
            }
            if ($scope.productNew.IsNonCreditable == 1) {
                //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);               
                //$scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax);
                $scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax);
                $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate);

            }
            else {
                //data.BaseAmount = parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge);
                data.TotalMaterialTranAmount = parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge);
                data.TotalMaterialBaseAmount = ((parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.productNew.ToCurrencyRate);
            }
        }

    };

    $scope.enableid1 = true;
    $scope.enableid3 = true;
    $scope.Change1 = function (event, index, x) {

        if (event.currentTarget.checked) {
            $scope.index = index;
            x.enableid1 = false;
            x.check == true;
        }


        else {
            x.enableid1 = true;
            x.check == false;
            //$scope.index = index;
        }
    }
    $scope.enableid2 = true;
    $scope.Change2 = function (event, index, x) {

        if (event.currentTarget.checked) {
            $scope.index = index;
            $scope.enableid2 = false;
            x.check == true;
        }


        else {
            $scope.enableid2 = true;
            x.check == false;
        }
    }

    $scope.recalculateMaterialByServiceAmount = function () {

        var count = 0;
        for (var j = 0; j < $scope.inventoryMaterialListPO.length; j++) {
            if ($scope.inventoryMaterialListPO[j].TransactionQty > 0) {
                count++;
            }
            else {
                $scope.inventoryMaterialListPO[j].ServiceCharge = 0;
                $scope.inventoryMaterialListPO[j].ServiceTax = 0;
                $scope.inventoryMaterialListPO[j].TrnAmount = 0;
            }
        }
        var TotalServiceAmount = Math.round($filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount') * 100 + Number.EPSILON) / 100;
        var TotalTrnAmount = Math.round($filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount') * 100 + Number.EPSILON) / 100;
        var TotalServiceTaxAmount = Math.round($filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount') * 100 + Number.EPSILON) / 100;
        var tempServiceAmount = 0;
        var tempServiceTaxAmount = 0;
        var newcount = 0;
        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            if ($scope.inventoryMaterialListPO[i].TransactionQty > 0) {
                newcount++;
                if (TotalServiceAmount > 0) {
                    if (count > newcount) {
                        $scope.inventoryMaterialListPO[i].ServiceCharge = Math.round(((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount) * 100 + Number.EPSILON) / 100;
                        $scope.inventoryMaterialListPO[i].ServiceTax = Math.round(((TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount) * 100 + Number.EPSILON) / 100;

                    }
                    else if (count == newcount) {
                        $scope.inventoryMaterialListPO[i].ServiceCharge = 0;
                        tempServiceAmount = Math.round($filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'ServiceCharge') * 100 + Number.EPSILON) / 100;
                        $scope.inventoryMaterialListPO[i].ServiceCharge = Math.round((TotalServiceAmount - tempServiceAmount) * 100 + Number.EPSILON) / 100;
                        $scope.inventoryMaterialListPO[i].ServiceTax = 0;
                        tempServiceTaxAmount = Math.round($filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'ServiceTax') * 100 + Number.EPSILON) / 100;
                        $scope.inventoryMaterialListPO[i].ServiceTax = Math.round((TotalServiceTaxAmount - tempServiceTaxAmount) * 100 + Number.EPSILON) / 100
                    }
                }
                if ($scope.productNew.IsNonCreditable == 1) {
                    $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount)) * 100 + Number.EPSILON) / 100;
                    $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;

                }
                else {
                    $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * 100 + Number.EPSILON) / 100;
                    $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
                }
            }
        }
    };
    $scope.calculateAmountForServiceCharge = function (data) {

        data.TotalTaxAmount = 0;
        for (var i = 0; i < $scope.chargesListPO.length; i++) {
            if ($scope.chargesListPO[i].Amount > parseFloat($scope.chargesListPO[i].POAmount) + parseFloat($scope.chargesListPO[i].GRNServiceAmount)) {

                ShowResult('Amount can not grater than PO Service Amount');
                $scope.chargesListPO[i].Amount = 0;
                return false;
            }
        }
        for (var i = 0; i < $scope.POServiceTaxList.length; i++) {

            if ($scope.POServiceTaxList[i].InventoryServiceId == data.Id) {
                $scope.POServiceTaxList[i].TaxAmount = Math.round((data.Amount * $scope.POServiceTaxList[i].Percentage / 100) * 100 + Number.EPSILON) / 100;
                data.TotalTaxAmount += $scope.POServiceTaxList[i].TaxAmount;
            }
        }
        var TotalServiceTaxAmount = Math.round($filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount') * 100 + Number.EPSILON) / 100;
        $scope.recalculateMaterialByServiceAmount();

    };


    $scope.calculateAmountForServiceCharge1 = function (data) {

        data.TotalTaxAmount = 0;
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesList), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TrnAmount');

        for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
            if ($scope.ServiceTaxList[i].InventoryServiceId == data.Id) {
                $scope.ServiceTaxList[i].TaxAmount = data.Amount * $scope.ServiceTaxList[i].Percentage / 100;
                data.TotalTaxAmount += $scope.ServiceTaxList[i].TaxAmount;
            }
        }
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');

        for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
            $scope.inventoryMaterialList[i].ServiceCharge = (parseFloat(TotalServiceAmount).toFixed(2) / parseFloat(TotalTrnAmount).toFixed(2)) * parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2);
            $scope.inventoryMaterialList[i].ServiceTax = (parseFloat(TotalServiceTaxAmount).toFixed(2) / parseFloat(TotalTrnAmount).toFixed(2)) * parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2);

            if ($scope.productNew.IsNonCreditable == 1) {

                $scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)).toFixed(2);
                $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

            }
            else {
                $scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge)).toFixed(2);
                $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
            }

        }
    };

    $scope.GRNReport = function (data) {

        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;
    };

    $scope.calculateMaterialTax = function (data, index) {
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
        var TotalMaterialTaxAmount = $filter('sumByKey')($filter('filter')($scope.receiveTaxList), 'TaxAmount');

        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailId) {
                $scope.inventoryMaterialListPO[i].BaseTaxAmount = TotalMaterialTaxAmount;

                if ($scope.productNew.IsNonCreditable == 1) {
                    $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax))).toFixed(2);

                    $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

                }
                else {
                    $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge))).toFixed(2);
                    $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);

                }
            }
        }
    };

    $scope.calculateSerciceTax = function (data) {
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
        var ServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'TotalTaxAmount');

        for (var i = 0; i < $scope.chargesListPO.length; i++) {
            if ($scope.chargesListPO[i].Id == data.InventoryServiceId) {
                $scope.chargesListPO[i].TotalTaxAmount = ServiceTaxAmount;
            }
        }

        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            //if ($scope.inventoryMaterialListPO[i].PODetailsID == data.Id) {
            //$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;  
            //$scope.inventoryMaterialListPO[i].ServiceTax = Math.round(Math$scope.chargesListPO[i].TotalTaxAmount);
            $scope.inventoryMaterialListPO[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
            $scope.inventoryMaterialListPO[i].ServiceTax = parseFloat((TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
            if ($scope.productNew.IsNonCreditable == 1) {
                //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat(parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2)).toFixed(2);
                $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

            }
            else {
                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat(parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)).toFixed(2);
                $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)) * $scope.productNew.ToCurrencyRate).toFixed(2);
            }

        }

    };
    $scope.onClickReportDownloadExcel = function (args) {
        var gridObj = $("#GriddataMaster1").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);

    };

    $scope.commandExcel = [{
        type: "details", buttonOptions: {
            text: "Excel",
            BackgroundColor: "Black",
            Color: "White",
            width: "50",
            height: "20",
            contentType: "imageonly",
            prefixIcon: "e-icon e-dataexport",

            click: $scope.onClickReportDownloadExcel
        }
    }];
    $scope.onClickReportDownloadPdf = function (args) {
        var gridObj = $("#GriddataMaster1").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);

    };
    $scope.commandPdf = [{
        type: "details", buttonOptions: {
            text: "Pdf",
            width: "50",
            height: "20",
            contentType: "imageonly",
            prefixIcon: "e-icon e-dataexport",

            click: $scope.onClickReportDownloadPdf
        }
    }];


    $scope.Get = function (index) {

        $scope.index = index;
        $scope.product = $scope.products[$scope.index];
        $scope.productNew = Object.assign({}, $scope.product);
        getPartyPlantList();
        getInventoryMaterialList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
        $scope.productId = $scope.productNew.Id;
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate')
                    $scope.IsBaseOnDueDateEnable = true;
                else
                    $scope.IsBaseOnDueDateEnable = false;
        }
        $scope.Action = 'Save';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    $scope.recorddoubleclickFromMasterGrid = function ($event) {
        var x = $event;
        var Id = x.data.Id;

        ClearFields();
        $scope.productId = Id;
        $scope.Action = 'Save';
        $scope.ActionForEdit = 'Update';
        $scope.POId1 = x.data.POID;
        $scope.POID = x.data.POID;
        $scope.product = $scope.products[$scope.index];
        $scope.productNew = x.data;
        $scope.productNew.NoteForAccounts = x.data.NoteForAccounts;
        $scope.productNew.GRNDate = x.data.GRNDate1;
        $scope.productNew.CheckedBy = x.data.CheckedBy;
        $scope.AcceptanceId = x.data.PurchaseDocumentAcceptanceId;
        $scope.AccDate = x.data.AcceptanceDate;
        $scope.loadAcceptanceDetail();
        if ($scope.AcceptanceId === null || $scope.AcceptanceId === "" || $scope.AcceptanceId === undefined) {
            $scope.status = 'PO';
            $scope.productNew.PO = $scope.status;

            $scope.tab1 = 1;
            $scope.GetSavedPOList1(Id);
        }
        else {

            $scope.status = 'Acceptance';
            $scope.productNew.PO = $scope.status;
            $scope.tab1 = 2;
        }

        getPartyPlantList();
        getInventoryMaterialList(Id);
        getServiceChargeList(Id);
        $scope.GetAdvanceTaxInfo(Id);
        $scope.productNew.TaxOptionAddiTax = 'Yes';
        $scope.TotalSumAfterTCS();
        $scope.ImagedataLoad(Id);
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

        $scope.GetCheckedByAndApprovedBy1();

        $scope.GetGRNAdditionalInfoList();
        if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {

            $scope.productNew.CheckedBy = x.data.ApprovedById;
            $scope.productNew.labelCheckAndApproved = 'To be approved by';
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && baseService.isUndefinedOrNull(x.data.ApprovedById)) {

            $scope.productNew.CheckedBy = x.data.CheckedById;
            $scope.productNew.labelCheckAndApproved = 'To be checked by';
        }

        if (!$rootScope.isCollapsed) $rootScope.toggle();

    }


    $scope.MasterOrderListHide = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListOfRequisition')).modal('hide');
    };
    $scope.rowDataBound = function rowDataBound(e) {

        if ($scope.RowColor != e.data.MaterialGroupMasterName + e.data.UserName + e.data.StandardName + e.data.FirstCharacteristicsValue + e.data.SecondCharacteristicsValue + e.data.ThirdCharacteristicsValue) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.RowColor = e.data.MaterialGroupMasterName + e.data.UserName + e.data.StandardName + e.data.FirstCharacteristicsValue + e.data.SecondCharacteristicsValue + e.data.ThirdCharacteristicsValue;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", '#D3D3D3');
        else
            e.row.css("background-color", '#ffffff');


    }
    $scope.PODetailsUpdatePOPUp = function (x, MaterialMasterId, InventoryReceiveDetailId) {
        $scope.Action1 = 'Update'
        getInventoryMaterialListForUpdate(x, MaterialMasterId, InventoryReceiveDetailId);
        angular.element(document.querySelector('#ListOfRequisition')).modal('show');
    };
    $scope.GetListForMasterOrder = [];
    function getInventoryMaterialListForUpdate(inveReveiveId, MaterialMasterId, InventoryReceiveDetailId) {
        $scope.Action1 = 'Save';
        $scope.masterId = inveReveiveId;
        $http.get($scope.path + 'GetInventoryMaterialListForPOUpdate?inveReveiveId=' + inveReveiveId + '&InventoryReceiveId=' + $scope.productNew.Id + '&MaterialMasterId=' + MaterialMasterId + '&InventoryReceiveDetailId=' + InventoryReceiveDetailId)
            .then(function (response) {
                $scope.GetListForMasterOrder = response.data;
                $scope.totalGRNVal = $scope.GetListForMasterOrder[0].GRNQty;
                $scope.RejectionQty = $scope.GetListForMasterOrder[0].RejectionQty;
            });


    }


    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    function getTaxList(inveReveiveId) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetTaxCategoryListPO?receiveDetailId=' + inveReveiveId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
        });
    }
    function checkChangeemployee(e) {
        var val = e.model.value;
        var hsnCodeId = $scope.GetListForMasterOrder[0].HSNCodeId;
        var row = $filter('filter')($scope.GetListForMasterOrder, { 'RequisitionDetailId': e.model.value });

        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check") {
                row[0].CheckedStatus = true;

            }
            else
                row[0].CheckedStatus = false;
        }

    }
    function headCheckChangeemployee(e) {
        var val = e.model.value;
        var hsnCodeId = $scope.GetListForMasterOrder[0].HSNCodeId;
        var row = $filter('filter')($scope.GetListForMasterOrder, { 'RequisitionDetailId': e.model.value });

        if (e.model.checkState == "check") {
            var filtered = $("#GridReq").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    $scope.GetListForMasterOrder[i].CheckedStatus = true;
                }
            }
            else {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.GetListForMasterOrder[i].RequisitionDetailId == filtered[j].RequisitionDetailId)
                            $scope.GetListForMasterOrder[i].CheckedStatus = true;
                    }

                }
            }

            var checkbox = $("#GridReq .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#GridReq").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    $scope.GetListForMasterOrder[i].CheckedStatus = false;
                }
            }
            else {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.GetListForMasterOrder[i].RequisitionDetailId == filtered[j].RequisitionDetailId)
                            $scope.GetListForMasterOrder[i].CheckedStatus = false;
                    }

                }
            }
            var checkbox = $("#GridReq .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
    }
    $scope.dataBoundemployee = function (args) {
        $("#GridReq .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    }
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.GetListForMasterOrder, { 'RequisitionDetailId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].CheckedStatus == true)
                $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    }

    $scope.tab1 = 1;
    $scope.setTabIndex = function (newTab) {
        $scope.tab1 = newTab;
        $scope.getalldata();
    };
    $scope.isSetIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTabIndex1 = function (newTab) {
        $scope.tab1 = newTab;
        $scope.getalldataIndexApp();
    };
    $scope.isSetIndex1 = function (tabNum) {
        return $scope.tab1 === tabNum;
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

    $scope.GRNbyPOCheckStatus = "ForChecked";
    $scope.GriddataMaster = [];
    $scope.GetListForGRNBYPO = function () {
        if ($scope.GRNbyPOCheckStatus === "ForChecked") {
            $scope.GRNbyPOCheckStatus = "ForChecked";
        }
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetListForGRNBYPO?GRNbyPOCheckStatus=' + $scope.GRNbyPOCheckStatus + '&grnType=' + 'GRNBYPO',
        }).then(function successCallback(response) {
            $scope.GriddataMaster = response.data;
        });
    };
    $scope.GetListForGRNBYPO();


    $scope.GriddataMaster2 = [];
    $scope.getalldataMaster2 = function () {
        $scope.GriddataMaster2 = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetListForMasterData2?GRNbyPOApprovedStatus=' + $scope.GRNbyPOApprovedStatus,
        }).then(function successCallback(response) {
            $scope.GriddataMaster2 = response.data;
        });
    };


    $scope.searchByPostedGRN = "Id"; $scope.searchGRN = "";
    $scope.searchByPostedGRNList = [{ value: 'Id', name: "GRN No" }, { value: 'GRNDate', name: "GRN Date" }, { value: 'PartyName', name: "Vendor" }
        , { value: 'GateEntryNo', name: "Gate EntryNo" }, { value: 'DocRefNo', name: "DocRef No" }
        , { value: 'DocDate', name: "Doc Date" }];

    $scope.GetSearchPostedGRNPOList = function () {
        $http({
            method: 'POST',
            url: 'Products/GoodsReceiveNote/GetSearchPostedGRNPOList',
            data: { column: $scope.searchByPostedGRN, value: $scope.searchGRN },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.GriddataMaster2 = response.data;
        });
    };


    $scope.GRN = "";
    $scope.tab = 1;
    $scope.GRNbyPOCheckStatus = "ForChecked";
    $scope.setTabGRNList = function (newTab) {
        $scope.tab = newTab;
        $scope.GRNbyPOCheckStatus = "ForChecked";
        $scope.getDataList();
        $scope.GetListForGRNBYPO();
    };
    $scope.isSetGRNList = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 1;

    };



    $scope.setTabCheckedHR = function (newTab) {
        $scope.tab = newTab;
        $scope.GRNbyPOCheckStatus = "CheckedHoldReject";
        $scope.GetListForGRNBYPO();

    };
    $scope.isSetCheckedHR = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 2;
    };

    $scope.setTabNotApprovedChecked = function (newTab) {

        $scope.tab = newTab;
        $scope.GRNbyPOCheckStatus = "Checked";
        $scope.GetListForGRNBYPO();

    };
    $scope.isSetNotApprovedChecked = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 3;
    };

    $scope.GRNbyPOApprovedStatus = "ApprovedHoldReject";
    $scope.setTabApprovedHR = function (newTab) {
        $scope.GRNbyPOApprovedStatus = "ApprovedHoldReject";
        $scope.tab = newTab;
        $scope.getalldataMaster2();

    };
    $scope.isSetApprovedHR = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 4;
    };



    $scope.isSetApprovedNP = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 5;
    };
    $scope.setTabApprovedNP = function (newTab) {
        $scope.tab = newTab;
        $scope.GRNbyPOApprovedStatus = "Approved";
        $scope.getalldataMaster2();

    };
    $scope.setTabPosted = function (newTab) {
        $scope.tab = newTab;
        $scope.GRNbyPOApprovedStatus = "Posted";
        $scope.getalldataMaster2();

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


    $scope.onClickReportCheckedHR = function (args) {
        var gridObj = $("#GriddataCheckedHR").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandCheckedHR = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportCheckedHR
        }
    }];

    $scope.onClickReportApprovedChecked = function (args) {
        var gridObj = $("#GriddataApprovedChecked").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandApprovedCK = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportApprovedChecked
        }
    }];

    $scope.onClickReportApprovedHR = function (args) {
        var gridObj = $("#GriddataApprovedHR").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandApprovedHRGRN = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",
            click: $scope.onClickReportApprovedHR
        }
    }];


    $scope.onClickReportDownloadWord2 = function (args) {
        var gridObj = $("#GriddataMaster2").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;
    };

    $scope.commandWord2 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportDownloadWord2
        }
    }];


    $scope.onClickcommandPosted = function (args) {
        var gridObj = $("#GriddataPosted").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;
    };

    $scope.commandPosted = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickcommandPosted
        }
    }];

    $scope.tab1 = 1;
    $scope.productNew.PO = "PO";
    $scope.status = "PO";
    $scope.setTabGRNPOList = function (newTab12) {
        $scope.GriddataSelected = [];

        $scope.AcceptanceId = '';
        $scope.Clear();
        $scope.productId = "";
        $scope.productNew.PO = "PO";
        $scope.status = "PO";
        $scope.tab1 = newTab12;
    };
    $scope.isSetGRNLPOist = function (tabNum12) {
        return $scope.tab1 === tabNum12;
    };

    $scope.setTabGRNAcceptance = function (newTab12) {

        $scope.Clear();
        $scope.productId = '';
        $scope.productNew.PO = "Acceptance";
        $scope.status = "Acceptance";
        $scope.GriddataSelected = [];
        $scope.tab1 = newTab12;
        $scope.productNew.TaxOptionAddiTax = 'Yes';
    };
    $scope.isSetGRNAcceptance = function (tabNum12) {
        return $scope.tab1 === tabNum12;
    };

    $scope.contractList = [];
    $scope.GetPopUpContract = function () {
        $scope.contractList = [];
        $http.get("Products/PurchaseOrder/GetLCContractList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.contractList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#ContractPopUp')).modal('show');
    };

    $scope.SelectedContract = function (obj) {
        var data = obj.data.ContractId;
        $scope.productNew.ContractId = data;
        $scope.productNew.CustomerName = obj.data.CustomerName;
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }

    $scope.ClearFields = function () {
        $scope.productNew.ContractId = null;

    }
    $scope.CloseContractPopUp = function () {
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }
    $scope.masterOrderCustomerList = [];
    $scope.GetMasterOrderByContractList = function () {
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetMasterOrderListbyContract?contractId=" + $scope.productNew.ContractId
        }).then(function (response) {
            $scope.masterOrderCustomerList = response.data;
        });
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
    }

    $scope.CloseMasterOrder = function () {
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
    }


    $scope.GriddataPOWithLC = [];
    $scope.getalldataPOWithLC = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetalldataPOWithLCMap',
        }).then(function successCallback(response) {
            $scope.GriddataPOWithLC = response.data;
        });
    };
    $scope.getalldataPOWithLC();

    $scope.GriddataPOWithOutLC = [];
    $scope.getalldataPOWithOutLC = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetalldataPOWithoutLCMap',
        }).then(function successCallback(response) {
            $scope.GriddataPOWithOutLC = response.data;
        });
    };
    $scope.getalldataPOWithOutLC();

    $scope.POTypeStatus = '';
    $scope.tab1 = 1;
    $scope.setTabPOLCMapIndex = function (newTab) {
        $scope.tab1 = newTab;
        $scope.getalldataPOWithLC();
    };
    $scope.isSetPOLCMapIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };
    $scope.setTabPOLCMap = function (newTab) {

        $scope.tab1 = newTab;
        $scope.getalldataPOWithOutLC();
    };
    $scope.isSetPOLCMap = function (tabNum) {
        return $scope.tab1 === tabNum;
    };
    $scope.LcList = [];
    $scope.GetLCByContract = function () {

        $http({
            method: 'GET',//?id=' + id+' & name='+name
            url: "Products/PurchaseOrder/GetLCListByCotract?ContractId=" + $scope.data.ContractId + "&VendorId=" + $scope.data.PartyId
        }).then(function successCallback(response) {
            $scope.LcList = response.data;
            angular.element(document.querySelector('#ContractPopUp')).modal('show');

        });

    }

    $scope.CurrencyId = null;
    $scope.a = function (args) {
        var gridObj = $("#Grid123").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.rowID = $scope.data.Id;
        $scope.CurrencyId = $scope.data.CurrencyId;
        $scope.GetLCByContract();
    };


    $scope.recorddoubleclickContract = function ($event) {

        var x = $event;
        var Id = x.data.Id;

        for (var i = 0; i < $scope.GriddataPOWithOutLC.length; i++) {
            if ($scope.GriddataPOWithOutLC[i].Id === $scope.rowID) {

                if ($scope.CurrencyId === x.data.CurrencyId) {
                    $scope.GriddataPOWithOutLC[i].PurchaseLCId = x.data.Value;
                    angular.element(document.querySelector('#ContractPopUp')).modal('hide');
                } else {
                    ShowResult("Purchase Order Currency and PurchaseLC Currency is not same!!!", 'failure', 'ContractPopUp');
                }
            }
        }

    };
    $scope.UpdatePOforLCdata = function () {

        if ($scope.data.PurchaseLCId === null || $scope.data.PurchaseLCId === '' || $scope.data.PurchaseLCId === undefined) {
            ShowResult('Please select Purchase LC');
            return false;
        }


        $http({
            method: 'POST',
            url: "Products/PurchaseOrder/UpdatePOforLC",
            data:
            {
                POId: $scope.rowID,
                PurchaseLCId: $scope.data.PurchaseLCId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldataPOWithOutLC();

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });





    }

    $scope.GetShortageRejectionValue = function () {
        $scope.newList = [];
        $http({
            method: 'GET',
            url: "Products/InventoryReceive/GetShortageRejectionValue?InventoryReceiveId=" + $scope.productNew.Id
        }).then(function (response) {
            $scope.newList = response.data;
        });
        angular.element(document.querySelector('#ValueSet')).modal('show');
    }

    $scope.UpdateUrlForSRValue = function () {
        $http({
            method: 'POST',
            url: $scope.updateUrlForSRValue,
            data:
            {
                'InventoryReceiveId': $scope.productNew.Id,
                'UserSendData': $scope.newList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                angular.element(document.querySelector('#ValueSet')).modal('hide');

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.closeReceiveTaxPopUpValueNew = function (x) {
        angular.element(document.querySelector('#ValueSet')).modal('hide');
    }


    $scope.GRNAllowcationForSO = function (x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID) {
        $scope.Action1 = 'Update'
        GRNAllowcationForSOList(x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID);
        angular.element(document.querySelector('#ListOfSo')).modal('show');
    };

    $scope.GRNAllowcationForSOInSavingTime = function (x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID) {
        $scope.Action1 = 'Save'

        GRNAllowcationForSOList1(x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID);
        angular.element(document.querySelector('#ListOfRequisition')).modal('show');


    };
    $scope.soList = [];
    $scope.InventoryReceiveDetailId = '';
    function GRNAllowcationForSOList(x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID) {
        $scope.Action1 = 'Save';
        $scope.InventoryReceiveDetailId = InventoryReceiveDetailId;
        $http.get($scope.path + 'GetGRNDetailsForSoAllocation?InventoryReceiveDetailId=' + x)
            .then(function (response) {
                $scope.soList = response.data;
                $scope.totalGRNVal = $scope.soList[0].GRNQty;
                $scope.RejectionQty = $scope.soList[0].GRNRejectionQty;
            });
    }

    $scope.soList = [];
    $scope.InventoryReceiveDetailId = '';
    function GRNAllowcationForSOList1(x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID) {
        $scope.Action1 = 'Save';
        $scope.InventoryReceiveDetailId = InventoryReceiveDetailId;
        $http.get($scope.path + 'GetGRNDetailsForSoAllocation?PODetailId=' + PODetailsID)
            .then(function (response) {
                $scope.soList = response.data;
                for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                    $scope.totalGRNVal = $scope.inventoryMaterialListPO[0].TransactionQty;
                    $scope.RejectionQty = $scope.inventoryMaterialListPO[0].RejectionQty;
                    if ($scope.soList.length === 1) {
                        for (var i1 = 0; i1 < $scope.soList.length; i1++) {
                            $scope.soList[i1].TransactionQty = $scope.inventoryMaterialListPO[0].TransactionQty;
                            $scope.soList[i1].RejectionQty = $scope.inventoryMaterialListPO[0].RejectionQty;

                        }

                    }
                }

            });
    }
    $scope.ListOfSo = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListOfSo')).modal('hide');
    };

    $scope.GrnRequisitionAllocationSave = function () {
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
                        ShowResult('Allocated Qty can not grater than GRN Qty', 'failure', 'ListOfSo');
                        return false;
                    }
                    else if (TotalRejectionQty > $scope.RejectionQty) {
                        ShowResult('Allocated Qty can not grater than Rejection Qty', 'failure', 'ListOfSo');
                        return false;
                    }
                    else if (baseService.isUndefinedOrNull($scope.soList[i].TransactionQty) || $scope.soList[i].TransactionQty === 0) {
                        ShowResult('Enter the Qty', 'failure', 'ListOfSo');
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
                    ShowResult('allocated qty can not grater than GRN Qty', 'failure', 'ListOfSo');
                    return false;
                }
                if (res1 > $scope.RejectionQty) {
                    ShowResult('allocated qty can not grater than Rejection Qty', 'failure', 'ListOfSo');
                    return false;
                }


            }
            if ($scope.soListNew.length === 0) {
                ShowResult('Please select atlest one item', 'failure', 'ListOfSo');
                return false;
            }
            if ($scope.Action1 === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Products/GoodsReceiveNote/GrnRequisitionAllocationSave',
                    data: {
                        entity: $scope.soListNew
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'ListOfSo');
                    else {
                        ShowResult(response.data.Message, 'success', 'ListOfSo');
                        GRNAllowcationForSOList($scope.InventoryReceiveDetailId);
                        $scope.Action1 = "Update";
                        //$scope.GetListForMasterOrder = [];
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'ListOfSo');
                };

            }
            else if ($scope.Action1 === "Update") {
                $http({
                    method: 'POST',
                    url: 'Products/GoodsReceiveNote/GrnRequisitionAllocationSave',
                    data: {
                        entity: $scope.soListNew
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'ListOfSo');
                    else {
                        ShowResult(response.data.Message, 'success', 'ListOfSo');
                        GRNAllowcationForSOList($scope.InventoryReceiveDetailId);

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'ListOfSo');
                };

            }
        } catch (e) {
        }
    };

    $scope.GRNAllowcationForRequisition = function (x, InventoryReceiveDetailId) {
        $scope.Action1 = 'Update'
        GRNAllowcationForRequisitionList(x, InventoryReceiveDetailId);
        angular.element(document.querySelector('#ListOfRequisition')).modal('show');
    };
    $scope.GRNAllowcationForRequisitionLst = [];
    function GRNAllowcationForRequisitionList(data, InventoryReceiveDetailId) {
        $scope.totalGRNVal = '';
        $scope.RejectionQty = '';
        $scope.Action1 = 'Save';
        $scope.masterId = data.POId;
        $http.get($scope.path + 'GetInventoryMaterialListForPOUpdate?inveReveiveId=' + data.POId + '&InventoryReceiveId=' + $scope.productNew.Id + '&MaterialMasterId=' + data.MaterialMasterId + '&InventoryReceiveDetailId=' + InventoryReceiveDetailId)
            .then(function (response) {
                $scope.GRNAllowcationForRequisitionLst = response.data;
                $scope.totalGRNVal = $scope.GRNAllowcationForRequisitionLst[0].GRNQty;
                $scope.RejectionQty = $scope.GRNAllowcationForRequisitionLst[0].RejectionQty;
            });


    }

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
            if ($scope.inventoryMaterialListPO.length > 0) {
                $scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceTax"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);

            }
            else {
                $scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);

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
                $scope.TotalSumAfterTCS();

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.GetAdvanceTaxInfo = function (Id) {

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
            $scope.GetAdvanceTaxInfo($scope.productNew.Id);
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
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.TaxOptionAdditax = function (data) {
        $scope.productNew.TaxOptionAddiTax = data;
    };

    $scope.calculateTaxAmountForAdditionalTax = function (data) {
        $scope.advanceTax.TaxAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseAmount") * data / 100).toFixed(2);

    };
    $scope.checkRowValidationSdditionalTax = function (data) {
        if ($scope.inventoryMaterialListPO.length>0) {
            var netAmount = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceTax"))).toFixed(2);

            $scope.advanceTax.ValueOfFixed = ((data / netAmount).toFixed(4) * 100);
        }
        else {
            var netAmount1 = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax"))).toFixed(2);

            $scope.advanceTax.ValueOfFixed = ((data / netAmount1).toFixed(4) * 100);
        }
    }

    $scope.TotalSumAfterTCS = function () {

        if ($scope.inventoryMaterialListPO.length>0) {
            $scope.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);

        }
        else {
            $scope.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);

        }

    }


    $scope.calculateAmountAfterDiscount = function (data, index) {
        $scope.PreBal = data.Balance;
        data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
        if (data.TrnAmount == 'NaN')
            data.TrnAmount = 0;

        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            $scope.inventoryMaterialListPO[i].Balance = '';
            if ($scope.inventoryMaterialListPO[i].POQty < (parseFloat($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2))) {
                $scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
                ShowResult('Current quantity can not grater than balance qty!', 'failure');
            }
            else if ($scope.inventoryMaterialListPO[i].ShortageQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
                ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
            }
            else if ($scope.inventoryMaterialListPO[i].RejectionQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
                ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
            }
            else if ($scope.inventoryMaterialListPO[i].DiscountAmount > $scope.inventoryMaterialListPO[i].TrnAmount) {
                ShowResult('Discount Amount  can not grater than Material Amount!', 'failure');
            }
            else {
                if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
                    $scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
                    $scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                    $scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                    $scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);

                }
                else {
                    $scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                    $scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                    $scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);
                }
                if ($scope.productNew.IsNonCreditable == 1) {
                    if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
                        $scope.inventoryMaterialListPO[i].TrnAmount = (($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate) - data.DiscountAmount).toFixed(2);
                        $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)).toFixed(2);
                        $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

                    }
                }
                else {
                    if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
                        $scope.inventoryMaterialListPO[i].TrnAmount = (($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate) - data.DiscountAmount).toFixed(2);
                        $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.ServiceCharge)).toFixed(2);
                        $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
                    }
                }
            }
        }

    };
    $scope.calculateAmountAfterDiscountEdit = function (data, index) {
        $scope.PreBal = data.Balance;
        data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
        if (data.TrnAmount == 'NaN')
            data.TrnAmount = 0;

        for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
            $scope.inventoryMaterialList[i].Balance = '';
            if ($scope.inventoryMaterialList[i].POQty < (parseFloat($scope.inventoryMaterialList[i].GRNRcvQty + $scope.inventoryMaterialList[i].TransactionQty).toFixed(2))) {
                $scope.inventoryMaterialList[i].Balance = $scope.inventoryMaterialList[i].POQty - $scope.inventoryMaterialList[i].GRNRcvQty;
                ShowResult('Current quantity can not grater than balance qty!', 'failure');
            }
            else if ($scope.inventoryMaterialList[i].ShortageQty > $scope.inventoryMaterialList[i].TransactionQty) {
                ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
            }
            else if ($scope.inventoryMaterialList[i].RejectionQty > $scope.inventoryMaterialList[i].TransactionQty) {
                ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
            }
            else if ($scope.inventoryMaterialList[i].DiscountAmount > $scope.inventoryMaterialList[i].TrnAmount) {
                ShowResult('Discount Amount  can not grater than Material Amount!', 'failure');
            }
            else {
                if ($scope.inventoryMaterialList[i].PODetailsID == data.PODetailsID) {
                    $scope.inventoryMaterialList[i].TrnAmount = data.TrnAmount;
                    $scope.inventoryMaterialList[i].Balance = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].GRNRcvQty + $scope.inventoryMaterialList[i].TransactionQty));
                    $scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
                    $scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].ShortageQty);

                }
                else {
                    $scope.inventoryMaterialList[i].Balance = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].GRNRcvQty + $scope.v[i].TransactionQty));
                    $scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
                    $scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].ShortageQty);
                }
                if ($scope.productNew.IsNonCreditable == 1) {
                    if ($scope.inventoryMaterialList[i].PODetailsID == data.PODetailsID) {
                        $scope.inventoryMaterialList[i].TrnAmount = (($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate) - data.DiscountAmount).toFixed(2);
                        $scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax)).toFixed(2);
                        $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

                    }
                }
                else {
                    if ($scope.inventoryMaterialList[i].PODetailsID == data.PODetailsID) {
                        $scope.inventoryMaterialList[i].TrnAmount = (($scope.inventoryMaterialList[i].NetQty * $scope.inventoryMaterialList[i].TransactionRate) - data.DiscountAmount).toFixed(2);
                        $scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.ServiceCharge)).toFixed(2);
                        $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.ServiceCharge) * $scope.productNew.ToCurrencyRate).toFixed(2);
                    }
                }
            }
        }

    };
    $scope.TaxOptionAdditax = function (data) {
        $scope.productNew.TaxOptionAddiTax = data;
    };

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


    $scope.closeReceiveTaxPopUpNew = function (data) {
        if (baseService.isUndefinedOrNull($scope.productId)) {
            $scope.inventoryMaterialListPO[$scope.receiveTaxindex].BaseTaxAmount = $filter("sumByKey")($filter("filter")($scope.receiveTaxList), "TaxAmount");
            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {


                if ($scope.productNew.IsNonCreditable == 1) {
                    $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)).toFixed(2);
                    $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

                }
                else {
                    $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)).toFixed(2);
                    $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
                }
            }
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
            $scope.receiveTaxindex = null;
        }
        else {
            $scope.inventoryMaterialList[$scope.receiveTaxindex].BaseTaxAmount = $filter("sumByKey")($filter("filter")($scope.receiveTaxList), "TaxAmount");
            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {


                if ($scope.productNew.IsNonCreditable == 1) {
                    $scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)).toFixed(2);
                    $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

                }
                else {
                    $scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge)).toFixed(2);
                    $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
                }
            }
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
            $scope.receiveTaxindex = null;
        }

    }

    $scope.GetAdvanceTaxInfo = function (Id) {

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/InventoryReceive/GetAdvanceTaxInfo?InventoryReceiveId=' + Id,
        }).then(function successCallback(response) {
            $scope.advanceTaxesList = response.data;

        });
    }
    $scope.DocDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.UserFilename;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ExpensesDocument + '/' + data.Id + extention;
    };

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
                    url: 'Products/InventoryReceive/GRNDocCreate',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("GRNDocumentMap", angular.toJson($scope.productDocMap));
                        if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: {
                        "GRNDocumentMap": $scope.productDocMap,
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
            url: 'Products/InventoryReceive/GRNDocumentMapData?POID=' + $scope.productNew.Id,
        }).then(function successCallback(response) { //datagatefun
            $scope.Imagedata = response.data;

        });
    };
    $scope.removePopUpForDoc = function (Id) {
        $scope.DocId = Id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removePopUpForDoc')).modal('show');
    };
    $scope.DeletePOIgame = function (Id) {

        if (!baseService.isUndefinedOrNull($scope.DocId)) {
            $http({
                method: 'POST',
                url: 'Products/InventoryReceive/GRNImageDelete?Id=' + $scope.DocId,
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
    $scope.CalculateRateAndAmount = function () {
        $scope.detailModel.TransactionRate = parseFloat(($scope.detailModel.GrossAmount - $scope.detailModel.DiscountAmount) / $scope.detailModel.TransactionQty).toFixed(4);
        $scope.detailModel.TransactionAmount = parseFloat($scope.detailModel.GrossAmount - $scope.detailModel.DiscountAmount).toFixed(2);
    }

    $scope.getServiceTaxListPOPOP = function (data, flag, Id, index) {
        $scope.ServiceAddindex = index;
        $scope.taxAbleAmnt = data.Amount;
        $scope.percentageColumn = flag;
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.Amount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.ServiceTaxList = [];
        if (data.POServiceTaxList.length > 0) {
            $scope.HSNCode = data.POServiceTaxList[0].HSNCode;
            $scope.ServiceTaxList = data.POServiceTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.ServiceTaxList.length; j++) {
            $scope.total = $scope.total + $scope.ServiceTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#ServiceTaxPopUp')).modal('show');
    }

    $scope.closeReceiveTaxPopUp1 = function () {

        if ($scope.ActionForEdit === 'Update') {
            $scope.chargesList[$scope.ServiceAddindex].TotalTaxAmount = $filter("sumByKey")($filter("filter")($scope.ServiceTaxList), "TaxAmount");
            var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesList), 'Amount');
            var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TrnAmount');
            var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');
            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                //if ($scope.inventoryMaterialListPO[i].PODetailsID == data.Id) {
                //$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
                $scope.inventoryMaterialList[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount).toFixed(2);
                $scope.inventoryMaterialList[i].ServiceTax = parseFloat((TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount).toFixed(2);
                //}
                //else {
                //    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                //    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                //}
                if ($scope.productNew.IsNonCreditable == 1) {
                    //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
                    //  $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount + $scope.inventoryMaterialListPO[i].ServiceCharge + $scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2);
                    $scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax))).toFixed(2);

                    $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

                }
                else {
                    //$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat(parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)).toFixed(2);
                    $scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge))).toFixed(2);

                    $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
                }

            }
        }

        else {
            $scope.chargesListPO[$scope.ServiceAddindex].TotalTaxAmount = $filter("sumByKey")($filter("filter")($scope.ServiceTaxList), "TaxAmount");
            var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
            var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
            var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount');
            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                //if ($scope.inventoryMaterialListPO[i].PODetailsID == data.Id) {
                //$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
                $scope.inventoryMaterialListPO[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2);
                $scope.inventoryMaterialListPO[i].ServiceTax = parseFloat((TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2);
                //}
                //else {
                //    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                //    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                //}
                if ($scope.productNew.IsNonCreditable == 1) {
                    //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
                    //  $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount + $scope.inventoryMaterialListPO[i].ServiceCharge + $scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2);
                    $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax))).toFixed(2);

                    $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

                }
                else {
                    //$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat(parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)).toFixed(2);
                    $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge))).toFixed(2);

                    $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
                }

            }
        }
        angular.element(document.querySelector('#ServiceTaxPopUp')).modal('hide');
    }

    $scope.calculateTaxAmountForService = function (data) {

        if ($scope.ActionForEdit === 'Update') {
            if (baseService.isUndefinedOrNull(data.Percentage)) {
                data.Percentage = 0;
            }
            data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
            for (var i = 0; i < $scope.POServiceTaxList.length; i++) {
                if ($scope.ServiceTaxList[i].Id === data.Id) {
                    $scope.ServiceTaxList[i].Percentage = data.Percentage;
                    $scope.ServiceTaxList[i].TaxAmount = data.TaxAmount;
                }
            }
        }
        else {
            if (baseService.isUndefinedOrNull(data.Percentage)) {
                data.Percentage = 0;
            }
            data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
            for (var i = 0; i < $scope.POServiceTaxList.length; i++) {
                if ($scope.POServiceTaxList[i].Id === data.Id) {
                    $scope.POServiceTaxList[i].Percentage = data.Percentage;
                    $scope.POServiceTaxList[i].TaxAmount = data.TaxAmount;
                }
            }
        }
    };
    $scope.checkRowValidationService = function (x) {
        if ($scope.ActionForEdit === 'Update') {
            for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.ServiceTaxList[i].TaxAmount) || $scope.ServiceTaxList[i].TaxAmount === 0) {
                    ShowResult("Taxable Amount can not null or zero", 'failure', 'ServiceTaxPopUp');
                }
                if ($scope.ServiceTaxList[i].Id === x.Id) {
                    $scope.ServiceTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100).toFixed(4);
                }

            }
            for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
                if ($scope.ServiceTaxList[i].Id === x.Id) {
                    $scope.ServiceTaxList[i].Percentage = x.Percentage;
                    $scope.ServiceTaxList[i].TaxAmount = x.TaxAmount;
                }
            }
        }
        else {
            for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.ServiceTaxList[i].TaxAmount) || $scope.ServiceTaxList[i].TaxAmount === 0) {
                    ShowResult("Taxable Amount can not null or zero", 'failure', 'ServiceTaxPopUp');
                }
                if ($scope.ServiceTaxList[i].Id === x.Id) {
                    $scope.ServiceTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100).toFixed(4);
                }

            }
            for (var i = 0; i < $scope.POServiceTaxList.length; i++) {
                if ($scope.POServiceTaxList[i].Id === x.Id) {
                    $scope.POServiceTaxList[i].Percentage = x.Percentage;
                    $scope.POServiceTaxList[i].TaxAmount = x.TaxAmount;
                }
            }
        }

    }

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
    $scope.checkRowValidationService1 = function (x) {
        if ($scope.Action === 'Update') {
            for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.taxCategoryList[i].TaxAmount) || $scope.taxCategoryList[i].TaxAmount === 0) {
                    ShowResult("Taxable Amount can not null or zero", 'failure', 'ServiceTaxPopUp');
                }
                if ($scope.taxCategoryList[i].Id === x.Id) {
                    $scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.serviceModel.TransactionAmount).toFixed(4) * 100).toFixed(4);
                }

            }
            for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                if ($scope.taxCategoryList[i].Id === x.Id) {
                    $scope.taxCategoryList[i].Percentage = x.Percentage;
                    $scope.taxCategoryList[i].TaxAmount = x.TaxAmount;
                }
            }
        }
    }
    $scope.copyDiscount = function () {
        try {
            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[0].DiscountAmount) || $scope.inventoryMaterialList[0].DiscountAmount == 0) {
                    throw "Enter discount value at row 1";
                }
                else if ($scope.inventoryMaterialList[i].DiscountAmount > $scope.inventoryMaterialList[i].TrnAmount) {
                    //$scope.inventoryMaterialList[i].DiscountAmount = 0;
                }
                else {
                    $scope.inventoryMaterialList[i].DiscountAmount = $scope.inventoryMaterialList[0].DiscountAmount;
                }
            }
        } catch (e) {
            ShowResult(e, 'info');
        }

    };

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.fileName = "Bin Wise GRN Report.xlsx";
    $scope.XlsDownloadBinWiseGRNReport = function (x) {

        //$http.get('Materials/DetentionLogout/XlsGetClosedDetentionReport?from=' + $scope.ModalNewClosedDetention.From + '&to=' + $scope.ModalNewClosedDetention.To + '&departmentId=' + $scope.ModalNewClosedDetention.DepartmentId + '&detentiontypeId=' + $scope.ModalNewClosedDetention.DetentionTypeId)
        $http({
            method: 'POST',
            url: 'Products/GoodsReceiveNote/XlsBinWiseGRNReport?grnId=' + x.data.Id,
            dataType: 'JSON',
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    //$rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };


    $scope.GRNAdditionalInfoDataList = [];
    $scope.GetGRNAdditionalInfoList = function () {
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/GetGRNAdditionalInfoData?grnId=' + $scope.productNew.Id
        }).then(function successCallback(response) {
            $scope.GRNAdditionalInfoDataList = response.data;
        });
    }

    $scope.SaveAddInfo = function () {
        $http({
            method: 'POST',
            url: 'Products/GoodsReceiveNote/CreateGRNAdditionalInfo',
            data: { 'data': $scope.GRNAdditionalInfoDataList, 'grnId': $scope.productNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetGRNAdditionalInfoList();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

}