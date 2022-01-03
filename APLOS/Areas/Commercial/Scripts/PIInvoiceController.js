'use strict';
PIInvoiceController.$inject = ['accountService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService', 'bankService', '$controller'];
function PIInvoiceController(accountService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService, bankService, $controller) {
    $rootScope.title = "PI wise sales";
    $scope.Action = 'Save';
    $scope.path = 'Commercial/PIInvoice/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveChargesUrl = $scope.path + 'CreateCharge';
    $scope.deleteUrl = $scope.path + 'delete/';
    //$controller("partyBaseController", { $scope: $scope, $http: $http });

    //#region Tab
    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.TaxOption = function (data) {
        $scope.salesVM.TaxOption = data;
    };
    $scope.TaxOptionMat = function (data) {
        $scope.salesVM.TaxOptionMat = data;

    };
    $scope.TaxOptionService = function (data) {
        $scope.salesVM.TaxOptionService = data;

    };
    $scope.TaxOptionServiceModify = function (data) {
        $scope.salesVM.TaxOptionServiceModify = data;

    };
    //#endregion

    //#region Model

    $scope.salesVM = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PartyId: null,
        PartyName: null,
        CurrencyId: null,
        PartyType: "Customer",
        InvoiceDate: $filter("dateFiltering")(Date.now()),
        EntryDate: $filter("dateFiltering")(Date.now()),
        PostingDate: $filter("dateFiltering")(Date.now()),
        DocDate: $filter("dateFiltering")(Date.now()),
        DocRefNo: null,
        Amount: 0,
        BankAmount: 0,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        PaymentTermId: null,
        Narration: null,
        CompanyCurrencyRate: 1,
        InvoicingPartyPlantId: null,
        DeliveryPartyPlantId: null,
        InvoicingByAddress: null,
        DeliveryByAddress: null,
        InvoicingState: null,
        InvoicingGSTIN: null,
        DeliveryState: null,
        DeliveryGSTIN: null,
        BLNumber: null,
        LCNumber: null,
        ComercialInvoiceNo: null,
        EXPFromNo: null,
        SourceType: 'MasterOrderSales',
        ContractId: null
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionService: 'Yes'
        , TaxOptionServiceModify: 'Yes'
        , TaxOptionAddiTax: 'Yes',
        BooksCurrencyTransactionAmount: null,
        BooksCurrencyTaxAmount: null,
        BooksCurrencyBaseRate: null,
        IsPark: 1
    };

    //#endregion

    //#region PACKING Popup

    $scope.PackingList = [];
    $scope.GetPackingListPopUp = function () {
        $scope.PackingList = [];
        $http({
            method: 'GET',
            url: "Commercial/PIInvoice/GetPackingData"
        }).then(function (response) {
            $scope.PackingList = response.data;
            if ($scope.selectedPackingList.length != 0) {
                for (var j = 0; j < $scope.PackingList.length; j++) {
                    for (var i = 0; i < $scope.selectedPackingList.length; i++) {
                        if ($scope.selectedPackingList[i].PackingId == $scope.PackingList[j].PackingId) {
                            $scope.PackingList[j].Active = true;
                        }
                    }
                }
            }
        });
        angular.element(document.querySelector('#PackingListPopUp')).modal('show');
    }

    $scope.ClosePackingList = function () {
        try {
            MakeData();

            if ($scope.selectedPackingList.length > 0) {
                var uniquePackingId = removeDuplicates($scope.selectedPackingList, 'PackingId');
                var wcPackingId = "";
                if (uniquePackingId.length > 0) {
                    wcPackingId = "IN(";
                    wcPackingId += Array.prototype.map.call(uniquePackingId, function (item) { return "'" + item.PackingId + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcPackingId;
            }


            $scope.GetPackingSOData($scope.sqlInStatement);
            angular.element(document.querySelector('#PackingListPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'info');
        }
    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.selectedPackingList = [];
    function MakeData() {
        try {
            $scope.selectedPackingList = [];
            for (var i = 0; i < $scope.PackingList.length; i++) {
                var getRow = $filter("filter")($scope.selectedPackingList, { "selectedPackingList": $scope.PackingList[i].PackingId });
                if (getRow.length == 0) {
                    if ($scope.PackingList[i].Active == true) {
                        var ob = {};
                        ob.PackingId = $scope.PackingList[i].PackingId;
                        ob.PartyId = $scope.PackingList[i].CustomerId;
                        ob.Id = null;
                        ob.PackingId = $scope.PackingList[i].PackingId;
                        $scope.salesVM.PackingId = $scope.PackingList[i].PackingId;
                        $scope.salesVM.Id = $scope.PackingList[i].Id;
                        //ob.Entity = $scope.PackingList[i].Entity;
                        //$scope.salesVM.EntityId = $scope.PackingList[i].EntityId;
                        $scope.salesVM.CurrencyId = $scope.PackingList[i].CurrencyId;
                        ob.Customer = $scope.PackingList[i].Customer;
                        ob.CustomerId = $scope.PackingList[i].CustomerId;
                        $scope.salesVM.PartyName = $scope.PackingList[i].Customer;
                        $scope.salesVM.PartyId = $scope.PackingList[i].CustomerId;
                        ob.StorageLoc = $scope.PackingList[i].StorageLoc;
                        ob.ByWhom = $scope.PackingList[i].ByWhom;
                        ob.DRespPerson = $scope.PackingList[i].DRespPerson;
                        ob.AddedDate = $scope.PackingList[i].AddedDate;
                        ob.InActiveDate = $scope.PackingList[i].InActiveDate;

                        $scope.selectedPackingList.push(ob);
                        $scope.getPartyPlant();

                    }
                }
            }
            $scope.changePaymentTerm($scope.salesVM.PaymentTermId);
            $scope.GetCurrencyExchangeRateList();
        } catch (e) {
            ShowResult(e, 'info');
        }
    }

    function checkExistCustomer(list, customerId, EntityId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PartyId !== customerId || list[i].EntityId !== EntityId) {
                return false;
            }
        }
        return true;
    }

    function checkExistList(list, PackingId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PackingId === PackingId) {
                return true;
            }
        }
        return false;
    }

    $scope.getPartyPlant = function () {
        $scope.getCboPartyPlantList($scope.salesVM.PartyId, function (result) {
            $scope.partyPlantList = result;
            angular.forEach($scope.partyPlantList, function (item, i) {
                if (item.IsDefault) {
                    $scope.partyPlantId = item.Value;
                    $scope.salesVM.InvoicingPartyPlantId = item.Value;
                    $scope.salesVM.DeliveryPartyPlantId = item.Value;
                    $scope.salesVM.InvoicingByAddress = item.Address1;
                    $scope.salesVM.DeliveryByAddress = item.Address1;
                    $scope.salesVM.InvoicingState = item.StateName;
                    $scope.salesVM.InvoicingGSTIN = item.GSTIN;
                    $scope.salesVM.DeliveryState = item.StateName;
                    $scope.salesVM.DeliveryGSTIN = item.GSTIN;
                    $scope.salesVM.InvoicingStateId = item.StateId;
                }
            });
        });
    }
    $scope.getCboPartyPlantList = function (partyId, callback) {
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
            .then(function (response) {
                callback(response.data);
            });
    };
    $scope.salesOrderList = [];
    $scope.GetPackingSOData = function () {
        $scope.salesOrderList = [];
        $http({
            method: 'GET',
            url: "Commercial/PIInvoice/GetPackingSOData?PackingId=" + $scope.sqlInStatement
        }).then(function (response) {
            $scope.salesOrderList = response.data;
            for (var i = 0; i < $scope.salesOrderList.length; i++) {
                getTaxCategoryList($scope.salesOrderList[i].HSNCodeId, $scope.salesOrderList[i].SONo, $scope.salesOrderList[i].Amount);

            }
        });
    }

    function getTaxCategoryList(hsnCodeId, soId, transactionAmount) {
        $http({
            method: 'GET',
            url: 'SalesManagements/Sales/GetTaxCategoryList?receiveId=' + $scope.salesVM.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.salesVM.InvoiceDate
        }).then(function (response) {
            $scope.materialtaxCategoryList = response.data;

            for (var i = 0; i < $scope.salesOrderList.length; i++) {
                if ($scope.salesOrderList[i].HSNCodeId === hsnCodeId) {
                    $scope.salesOrderList[i].TaxList = $scope.materialtaxCategoryList;
                    for (var j = 0; j < $scope.salesOrderList[i].TaxList.length; j++) {
                        $scope.calculateHSNTaxAmount($scope.salesOrderList[i].TaxList[j], transactionAmount);
                    }
                }
                $scope.CalculateTransactionAmount($scope.salesOrderList[i]);
            }
        });
    }

    $scope.CalculateTransactionAmount = function (data) {

        data.TaxAmount = 0;
        if (!baseService.isUndefinedOrNull(data.PIMaterialId)) {

            data.TransactionAmount = parseFloat(data.Rate * data.Quantity).toFixed(2);
        } else {

            data.TransactionAmount = parseFloat(data.Rate * data.Quantity).toFixed(2);
        }

        if (baseService.arrayLength(data.TaxList) > 0) {
            angular.forEach(data.TaxList, function (item) {
                item.TotalAmount = parseFloat((data.TransactionAmount * item.Percentage / 100).toFixed(2));
                data.TaxAmount += item.TotalAmount;
            });
            data.NetAmount = parseFloat(data.TransactionAmount) + parseFloat(data.TaxAmount);
        } else {
            data.NetAmount = parseFloat(data.TransactionAmount).toFixed(2);
        }
    }
    $scope.calculateHSNTaxAmount = function (data, transactionAmount) {
        $scope.taxAbleAmnt = transactionAmount;
        data.TotalAmount = $scope.taxAbleAmnt * data.Percentage / 100;
    };

    $scope.changePaymentTerm = function () {
        if (!baseService.isUndefinedOrNull($scope.salesVM.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.salesVM.PaymentTermId; })[0];
            $scope.salesVM.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.salesVM.BaseNoOfDays = paymentTerm.NoOfDay;
            $scope.BaseLineDate = paymentTerm.BaseLineDate;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate') {
                    $scope.salesVM.BaseOnDueDate = $filter('dateFiltering')($scope.salesVM.InvoiceDate);
                    $scope.IsBaseOnDueDateEnable = false;
                }
                else if (paymentTerm.BaseLineDate === 'postingdate') {
                    $scope.salesVM.BaseOnDueDate = $filter('dateFiltering')($scope.salesVM.InvoiceDate);
                    $scope.salesVM.BaseOnDueDate = null;
                    $scope.salesVM.BaseNoOfDays = null;
                    $scope.salesVM.MatureDate = null;
                    $scope.IsBaseOnDueDateEnable = false;
                }

                else {
                    $scope.salesVM.BaseOnDueDate = null;
                    $scope.IsBaseOnDueDateEnable = true;
                }

            $scope.getMatureDate($scope.salesVM.BaseOnDueDate, $scope.salesVM.BaseNoOfDays);
        }
    };
    $scope.getMatureDate = function (date, days) {
        if (baseService.isUndefinedOrNull(date)) return $scope.salesVM.MatureDate = null;
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.salesVM.MatureDate = $filter('date')(date, 'dd-MMM-yyyy');
    };
    $scope.GetCurrencyExchangeRateList = function () {
        if ($scope.salesVM.CurrencyId !== null && undefined !== $scope.salesVM.CurrencyId) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.salesVM.PostingDate + "&currencyId=" + $scope.salesVM.CurrencyId
            }).then(function (response) {
                $scope.currencyExchangeRate = response.data;
                $scope.salesVM.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
    };

    $http({
        method: "GET",
        url: "accounts/PaymentTerm/getcustomercbo"
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });



    //#endregion

    //#region Page load Function
    $scope.invoiceList = [];
    $scope.GetMasterData = function () {
        $http({
            method: "GET",
            url: "Commercial/PIInvoice/GetMaster"
        }).then(function successCallback(response) {
            $scope.invoiceList = response.data;
        });
    };
    $scope.GetMasterData();
    //#endregion

    //#region Party PoPup
    $scope.partyList = [];
    $scope.showPartyPopUp = function (flg) {
        $scope.flag = flg;
        if ($scope.flag === 'Transport' || $scope.flag === 'CNF') {
            $scope.partyType = 'Vendor';
        }
        $scope.searchByParty = "UserName"; $scope.searchParty = "";
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;

        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#partyPopUpNew')).modal('show');
    };

    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };

    $scope.closeInvoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');

    };
    //#endregion

    //#region Clear

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.savebtndisable = false;
        $scope.salesVM.RowState = 'Parked';
        $scope.salesVM = {
            Id: null,
            CompanyGroupId: null,
            CompanyId: null,
            PartyId: null,
            PartyName: null,
            CurrencyId: null,
            PartyType: "Customer",
            InvoiceDate: $filter("dateFiltering")(Date.now()),
            EntryDate: $filter("dateFiltering")(Date.now()),
            PostingDate: $filter("dateFiltering")(Date.now()),
            DocDate: $filter("dateFiltering")(Date.now()),
            DocRefNo: null,
            Amount: 0,
            BankAmount: 0,
            BaseOnDueDate: null,
            BaseNoOfDays: null,
            PaymentTermId: null,
            Narration: null,
            CompanyCurrencyRate: 1,
            InvoicingPartyPlantId: null,
            DeliveryPartyPlantId: null,
            InvoicingByAddress: null,
            DeliveryByAddress: null,
            InvoicingState: null,
            InvoicingGSTIN: null,
            DeliveryState: null,
            DeliveryGSTIN: null,
            BLNumber: null,
            LCNumber: null,
            ComercialInvoiceNo: null,
            EXPFromNo: null,
            SourceType: 'MasterOrderSales',
            ContractId: null
            , TaxOption: 'Yes'
            , TaxOptionMat: 'Yes'
            , TaxOptionService: 'Yes'
            , TaxOptionServiceModify: 'Yes'
            , TaxOptionAddiTax: 'Yes',
            BooksCurrencyTransactionAmount: null,
            BooksCurrencyTaxAmount: null,
            BooksCurrencyBaseRate: null,
            IsPark: 1
        };

        $scope.materialMaster = {
            MaterialMasterId: null,
            MaterialMasterName: null,
            BaseUOMId: null,
            BaseUoM: null,
            OurStyleName: null,
            MaterialGroupMasterName: null,
            ProductMasterName: null,
            IsOurStyleRequired: null,
            IsProductMstRequired: null,
            TransactionUoMId: null,
            ArticleId: null,
            ArticleName: null,
            CountryId: null
        };
        $scope.currencyExchangeRate = [];
        $scope.salesMaterialList = [];
        $scope.chargesList = [];
        $scope.receiveTaxList = [];
        $scope.uoMList = [];
        $scope.selectedPackingList = [];
        $scope.salesOrderList = [];
    }

    //#endregion

    //#region Page Function

    //$scope.entityList = [];
    //cboService.getCboEntityByPlant(null, null, " ", function (result) {
    //    $scope.entityList = result;
    //});
    //$scope.salesOrderList = [];
    //$scope.GetPackingSOData = function () {
    //    $scope.salesOrderList = [];
    //    $http({
    //        method: 'GET',
    //        url: "Productions/PackingInvoice/GetPackingSOData?PackingId=" + $scope.sqlInStatement
    //    }).then(function (response) {
    //        $scope.salesOrderList = response.data;
    //        for (var i = 0; i < $scope.salesOrderList.length; i++) {
    //            getTaxCategoryList($scope.salesOrderList[i].HSNCodeId, $scope.salesOrderList[i].SONo, $scope.salesOrderList[i].TransactionAmount);

    //        }
    //    });
    //}

    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
    });

    $scope.advanceTax = { TotalSumAfterTCSVal: 0 };

    $scope.getMaterialTaxList = function (data, flag, index) {
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        var d = $scope.salesOrderList[$scope.currentMaterialRow];

        $scope.salesVM.TaxOption = 'Yes';
        $scope.LoadTaxButtonClick();
        $scope.Currency = $("#currency option:selected").text();
        $scope.taxAbleAmnt = data.TransactionAmount;

        $scope.receiveTaxList = [];


        if ($scope.salesOrderList[$scope.currentMaterialRow].TaxList.length > 0) {
            $scope.HSNCode = $scope.salesOrderList[$scope.currentMaterialRow].TaxList[0].HSNCode;
            if (baseService.isUndefinedOrNull($scope.salesOrderList[$scope.currentMaterialRow].TaxList[0].HSNCode)) {
                $scope.HSNCode = $scope.salesOrderList[$scope.currentMaterialRow].HSNCode;
            }

            angular.copy($scope.salesOrderList[$scope.currentMaterialRow].TaxList, $scope.receiveTaxList);


        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    };

    $scope.LoadTaxButtonClick = function () {
        accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
            $scope.taxCategoryList = result;
        });
    }
    $scope.closeReceiveTaxPopUp = function () {
        try {
            var materialData = $scope.salesOrderList[$scope.currentMaterialRow];
            $scope.salesOrderList[$scope.currentMaterialRow].TaxAmount = 0;
            for (var i = 0; i < $scope.receiveTaxList.length; i++) {
                var taxcat = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
                if (taxcat.length == 2) {
                    ShowResult('Same Tax Category already exsist', 'failure', 'receiveTaxPopUp');
                    angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
                }
                var TxA = parseFloat($scope.salesOrderList[$scope.currentMaterialRow].TaxAmount) + parseFloat($scope.receiveTaxList[i].TotalAmount);
                $scope.salesOrderList[$scope.currentMaterialRow].TaxAmount = parseFloat(TxA.toFixed(2));
            }
            $scope.salesOrderList[$scope.currentMaterialRow].TaxList = $scope.receiveTaxList;
            var NAmount = parseFloat($scope.salesOrderList[$scope.currentMaterialRow].TransactionAmount) + parseFloat($scope.salesOrderList[$scope.currentMaterialRow].TaxAmount);
            $scope.salesOrderList[$scope.currentMaterialRow].NetAmount = parseFloat(NAmount.toFixed(2));
            $scope.materialMaster = {};
            $scope.receiveTaxList = [];
            $scope.isService = false;
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure', 'receiveTaxPopUp');
        }
    };
    $scope.addTax = function () {
        var data = {
            TotalAmount: 0,
            Id: null,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null
        };
        $scope.receiveTaxList.push(data);
    };
    $scope.closeReceiveTaxPopUpwindow = function () {
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }
    //#endregion

    //#region
    $scope.serviceChargePopUp = function () {
        try {
            $scope.salesVM.TaxOptionService = 'Yes';
            $scope.serviceModel = {
                Id: null
                , ServiceMasterId: null
                , SalesId: $scope.salesVM.Id
                , CurrencyName: angular.element("#currency :selected").text()
                , ChargeName: null
                , CurrencyId: $scope.salesVM.CurrencyId
                , BaseCurrencyId: $scope.baseCurrencyId
                , DocDate: $scope.salesVM.DocDate
                , Amount: 0
                , TaxAmount: 0
                , NetAmount: 0
                , ServiceTaxList: null
            };
            angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
    $http.get("Setups/CompanyServiceMaster/GetCboList")
        .then(function (response) {
            $scope.serviceList = response.data;
        });
    $scope.changeService = function (id) {
        $scope.serviceModel.ServiceMasterId = id;
        if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
        var HSNCode = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCode;
        $scope.serviceModel.ChargeName = angular.element("#charge :selected").text();
        getServiceTaxCategoryList(hsnCodeId, HSNCode);
    };
    function getServiceTaxCategoryList(hsnCodeId, HSNCode) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: 'SalesManagements/Sales/GetTaxCategoryList?receiveId=' + $scope.salesVM.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.salesVM.InvoiceDate
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
            for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.taxCategoryList[i].hsnCodeId)) {
                    $scope.taxCategoryList[i].HSNCode = HSNCode;
                    $scope.taxCategoryList[i].HSNCodeId = hsnCodeId;
                }
            }
        });
    }
    $scope.closeServiceChargePopUp = function () {
        $scope.serviceModel = {};
        $scope.receiveTaxList = [];
        angular.element(document.querySelector("#serviceChargeTaxPopUp")).modal("hide");
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    };
    //#endregion

    //#region show Customer popuP

    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

    $scope.OrderSpecific = 'No';
    $scope.partyType = "Customer";
    $scope.showPartyPopUpNew = function () {
        if ($scope.OrderSpecific === 'Yes') {
            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {

            }
            $http({
                method: 'POST',
                url: 'Parties/party/GetCompanyPartyDataListByContract?ContractId=' + $scope.productNew.ContractId + '&partyType=' + $scope.partyType,
                data: { column: $scope.searchByParty, value: $scope.searchParty },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.partyList = response.data;
                if ($scope.partyList.length === 0) {
                    if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
                    }
                    else if ($scope.partyType === 'Party') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    else if ($scope.partyType === 'Director') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    else if ($scope.partyType === 'Other') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    $http({
                        method: 'POST',
                        url: $scope.partyUrl,
                        data: { column: $scope.searchByParty, value: $scope.searchParty },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        $scope.partyList = response.data;
                    });
                }
            });

        }
        else {

            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
            }
            else if ($scope.partyType === 'Party') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            else if ($scope.partyType === 'Director') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            else if ($scope.partyType === 'Other') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            $http({
                method: 'POST',
                url: $scope.partyUrl,
                data: { column: $scope.searchByParty, value: $scope.searchParty },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.partyList = response.data;
            });
        }
        angular.element(document.querySelector('#partyPopUp')).modal('show');
    };
    $scope.closePartyPopUp = function (x) {

        var party = x.data;
        $scope.salesVM.PartyName = party.UserName;
        $scope.salesVM.PartyId = party.Id;
        $scope.salesVM.PaymentTermId = party.PaymentTermId;
        $scope.salesVM.CurrencyId = party.CurrencyId;
        $scope.GetCurrencyExchangeRateList();
        $scope.changePaymentTerm($scope.salesVM.PaymentTermId);
        $scope.partyPlantList = [];
        $scope.getCboPartyPlantList(party.Id, function (result) {
            $scope.partyPlantList = result;
            angular.forEach($scope.partyPlantList, function (item, i) {
                if (item.IsDefault) {
                    $scope.partyPlantId = item.Value;
                    $scope.salesVM.InvoicingPartyPlantId = item.Value;
                    $scope.salesVM.DeliveryPartyPlantId = item.Value;
                    $scope.salesVM.InvoicingByAddress = item.Address1;
                    $scope.salesVM.DeliveryByAddress = item.Address1;
                    $scope.salesVM.InvoicingState = item.StateName;
                    $scope.salesVM.InvoicingGSTIN = item.GSTIN;
                    $scope.salesVM.DeliveryState = item.StateName;
                    $scope.salesVM.DeliveryGSTIN = item.GSTIN;
                    $scope.salesVM.InvoicingStateId = item.StateId;
                }
            });
        });
        $scope.partyType = "Customer";
        $scope.flag = null;
        $scope.hidePartyPopUp();
    };
    $scope.hidePartyPopUp = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };
    //#endregion

    //#region Double Click

    $scope.Get = function (obj) {
        $scope.salesVM = obj.data;

        $scope.getPartyPlant();
        //$scope.changePaymentTerm($scope.salesVM.PaymentTermId);
        $scope.GetCurrencyExchangeRateList();

        $scope.GetListData($scope.salesVM.Id);

        
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.salesVM.TaxOptionAddiTax = 'Yes';
    };

    $scope.GetListData = function (CommercialInvoiceMasterId) {
        $http({
            method: "GET",
            url: "Commercial/PIInvoice/GetSelectedList?CommercialInvoiceMasterId=" + CommercialInvoiceMasterId,
        }).then(function successCallback(response) {
            $scope.selectedPackingList = response.data;

            if ($scope.selectedPackingList.length > 0) {
                var uniquePackingId = removeDuplicates($scope.selectedPackingList, 'PackingId');
                var wcPackingId = "";
                if (uniquePackingId.length > 0) {
                    wcPackingId = "IN(";
                    wcPackingId += Array.prototype.map.call(uniquePackingId, function (item) { return "'" + item.PackingId + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcPackingId;
            }


            $scope.GetPackingSOData($scope.sqlInStatement);

        });
    };

    function getPartyPlantEditList(invoicingPartyPlantId, invoAddress, deliveryplant, deliAddress, deliState, deliGSTIN) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.salesVM.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (item.Value == invoicingPartyPlantId) {
                    $scope.partyPlantId = item.Value;
                    $scope.salesVM.InvoicingPartyPlantId = item.Value;
                    $scope.salesVM.DeliveryPartyPlantId = deliveryplant;
                    $scope.salesVM.InvoicingByAddress = invoAddress;
                    $scope.salesVM.DeliveryByAddress = deliAddress;
                    $scope.salesVM.InvoicingState = item.StateName;
                    $scope.salesVM.InvoicingGSTIN = item.GSTIN;
                    $scope.salesVM.DeliveryState = deliState;
                    $scope.salesVM.DeliveryGSTIN = deliGSTIN;
                    $scope.salesVM.InvoicingStateId = item.StateId;
                }
            });

        });
    }
    $scope.selectedMasterOrderItemTempList = [];
    $scope.GetSalesMaterialData = function (salesId) {
        $scope.salesOrderList = [];
        $scope.salesMaterialList = [];
        $scope.selectedMasterOrderItemTempList = [];
        $scope.uoMList = [];
        $http({
            method: "GET",
            url: "Productions/PackingInvoice/GetMasterOrderSalesMaterialData?salesId=" + salesId
        }).then(function (response) {
            $scope.salesMaterialList = response.data;

            $scope.salesOrderList = response.data;

            for (var i = 0; i < $scope.salesMaterialList.length; i++) {
                $scope.getAllTransactionUoM($scope.salesMaterialList[i].MaterialMasterId);
            }

            $scope.GetSalesTaxData(salesId);
            $scope.GetAdvanceTaxInfo($scope.salesVM.Id);
            //$scope.TotalSumAfterTCS();
        });
    };
    $scope.getAllTransactionUoM = function (materialMasterId) {
        var mmId = [];
        mmId.push(materialMasterId);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
            var getRow = $filter("filter")($scope.uoMList, { "MaterialMasterId": materialMasterId });
            if (getRow.length === 0) {
                angular.forEach(result, function (item, i) {
                    $scope.uoMList.push(item);
                });
            } else {
                $scope.uoMList = result;
            }
        });
    };
    $scope.GetSalesTaxData = function (salesId) {
        $scope.TaxList = [];
        $http({
            method: "GET",
            url: "SalesManagements/Sales/GetSalesTaxData?salesId=" + salesId
        }).then(function (response) {
            $scope.TaxList = response.data;
            for (var i = 0; i < $scope.salesOrderList.length; i++) {
                var linepk = $scope.salesOrderList[i].Id;
                var list = gettaxlist(linepk);
                $scope.salesOrderList[i].TaxList = list;

            }
            $scope.GetSalesServiceData($scope.salesVM.Id);
        });
    };
    $scope.GetAdvanceTaxInfo = function (Id) {

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'SalesManagements/Sales/GetAdvanceTaxInfo?SalesId=' + Id,
        }).then(function successCallback(response) {
            $scope.advanceTaxesList = response.data;

            $scope.advanceTax.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);

        });

    }
    $scope.GetSalesPackingData = function (salesId) {
        $scope.selectedPackingList = [];
        $http({
            method: 'GET',
            url: "Productions/PackingInvoice/GetSalesPackingData?salesId=" + salesId
        }).then(function (response) {
            $scope.selectedPackingList = response.data;
        });
    }
    $scope.getPostSalesData = function () {
        $http.get("Commercial/PostSalesInvoice/GetListBySalesId?SalesId=" + $scope.salesVM.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ModelNew = Object.assign({}, response.data[0]);
                    }
                    $scope.ModelNew.SalesId = $scope.salesVM.Id;
                    $scope.ModelNew.InvoiceDate = $scope.salesVM.InvoiceDate;
                    $scope.ModelNew.InvoiceNo = $scope.salesVM.InvoiceNo;
                    $scope.ModelNew.ContractNo = $scope.salesVM.ContractNo;
                    $scope.ModelNew.PartyName = $scope.salesVM.PartyName;
                    $scope.ModelNew.Amount = $scope.salesVM.Amount;

                    if (baseService.arrayLength($scope.bankMasterList) > 0 && !baseService.isUndefinedOrNull($scope.salesVM.BenificiaryBankId)) {
                        for (var i = 0; i < $scope.bankMasterList.length; i++) {
                            if ($scope.bankMasterList[i].Id === $scope.salesVM.BenificiaryBankId) {
                                $scope.ModelNew.BankMasterId = $scope.bankMasterList[i].Id;
                            }
                        }
                    }

                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }

    function gettaxlist(linepk) {
        var result = [];
        for (var i = 0; i < $scope.TaxList.length; i++) {
            if ($scope.TaxList[i].SalesMaterialId === linepk) {
                result.push($scope.TaxList[i]);
            }
        }
        return result;
    }

    $scope.taxCodCboListWithhold = [];
    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYearWithhold = function (date) {
        $scope.salesVM.TaxOptionAddiTax = 'Yes';
        $http({
            method: "Get",
            url: "accounts/TaxCode/GetAdditionalTaxOutputCbo?postingDate=" + $filter("dateFiltering")(date)
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

    //#endregion

    //#region S A V E

    $scope.Save = function () {
        try {
            var TaxLists = [];
            for (var i = 0; i < $scope.salesOrderList.length; i++) {
                for (var j = 0; j < $scope.salesOrderList[i].TaxList.length; j++) {
                    TaxLists.push($scope.salesOrderList[i].TaxList[j]);
                }
            }

            $http({
                method: 'POST',
                url: $scope.path + 'Create',
                data: { 'MasterData': $scope.salesVM, 'CommercialInvoicePackingList': $scope.selectedPackingList, 'CommercialInvoicePIMaterial': $scope.salesOrderList, 'taxList': TaxLists },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetMasterData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'info');
        }
    }
    //#endregion
}