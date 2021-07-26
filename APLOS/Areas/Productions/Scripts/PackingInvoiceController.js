'use strict';
PackingInvoiceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function PackingInvoiceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Packing Invoice';
    $scope.path = 'Productions/PackingInvoice/';
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.searchBy = "Customer"; $scope.search = "";
    $scope.searchByList = [{ value: 'PO', name: "PO" }, { value: 'Customer', name: "Customer" }, { value: 'Productcode', name: "Product Code" }];



    $scope.salesVM = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PartyId: null,
        PartyName: null,
        CurrencyId: null,
        PartyType: "Customer",
        InvoiceDate: $filter("dateFiltering")(Date.now()),
        VoucherDate: $filter("dateFiltering")(Date.now()),
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

    $scope.PackingList = [];
    $scope.GetPackingListPopUp = function () {
        $scope.PackingList = [];
        $http({
            method: 'GET',
            url: "Productions/PackingInvoice/GetPackingData"
        }).then(function (response) {
            $scope.PackingList = response.data;
        });
        angular.element(document.querySelector('#PackingListPopUp')).modal('show');
    }

    $scope.ClosePackingList = function () {
        MakeData();
        $scope.GetPackingSOData();
        angular.element(document.querySelector('#PackingListPopUp')).modal('hide');
    }

    $scope.selectedPackingList = [];
    function MakeData() {
        try {
            for (var i = 0; i < $scope.PackingList.length; i++) {
                var getRow = $filter("filter")($scope.selectedPackingList, { "selectedPackingList": $scope.PackingList[i].PackingId });
                if (getRow.length == 0) {
                    if ($scope.PackingList[i].Active == true) {
                        var ob = {};
                        ob.PackingId = $scope.PackingList[i].PackingId;
                        ob.PartyId = $scope.PackingList[i].CustomerId;

                        if (checkExistCustomer($scope.selectedPackingList, ob.PartyId)) {
                            if (checkExistList($scope.selectedPackingList, ob.PackingId) === false) {

                                ob.PackingId = $scope.PackingList[i].PackingId;
                                $scope.salesVM.PackingId = $scope.PackingList[i].PackingId;
                                ob.Entity = $scope.PackingList[i].Entity;
                                $scope.salesVM.EntityId = $scope.PackingList[i].EntityId;
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
                        else {
                            throw 'Select same Customer.';
                        }
                    }
                }
            }
            $scope.changePaymentTerm($scope.salesVM.PaymentTermId);
            $scope.GetCurrencyExchangeRateList();
        } catch (e) {
            ShowResult(e, 'failure', 'PackingListPopUp');
        }
    }

    function checkExistCustomer(list, customerId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PartyId !== customerId) {
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

    $scope.dateMessage = "";
    $scope.checkDocDate = function () {
        if (new Date($scope.salesVM.DocDate) > new Date()) {
            $scope.dateMessage = "Doc date must be below or equal to current Date!";
            return false;
        }
        else {
            $scope.dateMessage = "";
            return true;
        }
    };

    $scope.entityList = [];
    cboService.getCboEntityByPlant(null, null, " ", function (result) {
        $scope.entityList = result;
    });
    $scope.salesOrderList = [];
    $scope.GetPackingSOData = function () {
        $scope.salesOrderList = [];
        $http({
            method: 'GET',
            url: "Productions/PackingInvoice/GetPackingSOData?PackingId=" + $scope.salesVM.PackingId
        }).then(function (response) {
            $scope.salesOrderList = response.data;
        });
        // angular.element(document.querySelector('#SalesOrderPopUp')).modal('show');
    }
    //$scope.CloseSalesOrderPopUp = function () {
    //    angular.element(document.querySelector('#SalesOrderPopUp')).modal('hide');

    //}
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };

    $scope.closeInvoicingPartyPopUp = function () {
        if ($scope.salesMaterialList.length || $scope.chargesList.length) {
            if (!baseService.isUndefinedOrNull($scope.salesVM.ChangeInvoicingStateId)) {
                if ($scope.salesVM.PlantStateId == $scope.salesVM.InvoicingStateId == $scope.salesVM.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.salesVM.InvoicingStateId == $scope.salesVM.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.salesVM.PlantStateId != $scope.salesVM.InvoicingStateId && $scope.salesVM.PlantStateId != $scope.salesVM.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else
                    ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
            }
            else
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        }
        else
            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');

    };

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

    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
            var stateId = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateId;
            if (flag === 'billTo') {
                $scope.salesVM.InvoicingState = state;
                $scope.salesVM.ChangeInvoicingStateId = stateId;
                $scope.salesVM.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.salesVM.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.salesVM.DeliveryState = state;
                $scope.salesVM.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.salesVM.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.salesVM.InvoicingState = null;
                $scope.salesVM.InvoicingGSTIN = null;
                return $scope.productNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.salesVM.DeliveryState = null;
                $scope.salesVM.DeliveryGSTIN = null;
                return $scope.salesVM.DeliveryByAddress = null;
            }
        }
    };

    $scope.closeInvoicingPartyPopUp = function () {
        //if ($scope.salesMaterialList.length || $scope.chargesList.length) {
        //    if (!baseService.isUndefinedOrNull($scope.salesVM.ChangeInvoicingStateId)) {
        //        if ($scope.salesVM.PlantStateId == $scope.salesVM.InvoicingStateId == $scope.salesVM.ChangeInvoicingStateId)
        //            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        //        else if ($scope.salesVM.InvoicingStateId == $scope.salesVM.ChangeInvoicingStateId)
        //            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        //        else if ($scope.salesVM.PlantStateId != $scope.salesVM.InvoicingStateId && $scope.salesVM.PlantStateId != $scope.salesVM.ChangeInvoicingStateId)
        //            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        //        else
        //            ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
        //    }
        //    else
        //        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        //}
        //else
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');

    };

    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
    });

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

    $scope.refreshPackingTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllPackingWise });
    };

    function CheckBoxSelectAllPackingWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPacking").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PackingList.length; i++) {
                $scope.PackingList[i].Active = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Active = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPacking").data("ejGrid");
        gridObj.refreshContent();
    };

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
            VoucherDate: $filter("dateFiltering")(Date.now()),
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
        $scope.selectedMasterOrderList = [];
        $scope.selectedMasterOrderItemList = [];
    }
















}