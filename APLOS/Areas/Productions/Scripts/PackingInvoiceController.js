'use strict';
PackingInvoiceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller','accountService'];
function PackingInvoiceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, accountService) {
    $rootScope.title = 'Packing Invoice';
    $scope.path = 'Productions/PackingInvoice/';
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.searchBy = "Customer"; $scope.search = "";
    $scope.searchByList = [{ value: 'PO', name: "PO" }, { value: 'Customer', name: "Customer" }, { value: 'Productcode', name: "Product Code" }];
    $scope.Action = 'Save';


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

    $http({
        method: "GET",
        url: "accounts/PaymentTerm/getcustomercbo"
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });

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
            //angular.element(document.querySelector('#PackingListPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    function getTaxCategoryList(hsnCodeId, soId, transactionAmount) {
        $http({
            method: 'GET',
            //url: 'OrderManagements/masterorder/GetTaxCategoryList?masterOrderId=' + $scope.salesVM.MasterOrderId + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.salesVM.InvoiceDate
            url: 'SalesManagements/Sales/GetTaxCategoryList?receiveId=' + $scope.salesVM.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.salesVM.InvoiceDate
        }).then(function (response) {
            $scope.materialtaxCategoryList = response.data;

            for (var i = 0; i < $scope.salesOrderList.length; i++) {
                if ($scope.salesOrderList[i].SONo === soId) {
                    $scope.salesOrderList[i].TaxList = $scope.materialtaxCategoryList;

                    for (var j = 0; j < $scope.salesOrderList[i].TaxList.length; j++) {
                        $scope.calculateHSNTaxAmount($scope.salesOrderList[i].TaxList[j], transactionAmount);
                    }

                }
            }
        });
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
                        ob.EntityId = $scope.PackingList[i].EntityId;
                        if (checkExistCustomer($scope.selectedPackingList, ob.PartyId, ob.EntityId)) {
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
                            throw 'Select same Entity and Customer.';
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

    $scope.entityList = [];
    cboService.getCboEntityByPlant(null, null, " ", function (result) {
        $scope.entityList = result;
    });
    $scope.salesOrderList = [];
    $scope.GetPackingSOData = function () {
        $scope.salesOrderList = [];
        $http({
            method: 'GET',
            url: "Productions/PackingInvoice/GetPackingSOData?PackingId=" + $scope.sqlInStatement
        }).then(function (response) {
            $scope.salesOrderList = response.data;
            for (var i = 0; i < $scope.salesOrderList.length; i++) {
                getTaxCategoryList($scope.salesOrderList[i].HSNCodeId, $scope.salesOrderList[i].HSNCodeId, null);
            }
        });
        // angular.element(document.querySelector('#SalesOrderPopUp')).modal('show');
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
        $scope.selectedPackingList = [];
        $scope.salesOrderList = [];
    }

    $scope.serviceChargeTaxPopUp = function () {
        angular.element(document.querySelector("#serviceChargeTaxPopUp")).modal("show");
    };

    $http.get("Setups/CompanyServiceMaster/GetCboList")
        .then(function (response) {
            $scope.serviceList = response.data;
        });

    $scope.closeServiceChargePopUp = function () {
        $scope.serviceModel = {};
        $scope.receiveTaxList = [];
        angular.element(document.querySelector("#serviceChargeTaxPopUp")).modal("hide");
    };

    $scope.onchangeFunction1 = function (id) {
        $scope.TaxCategoryId = id;

        var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
        if (getRow.length === 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
        }
    };

    $scope.calculateAmount = function (data) {
        data.TransactionAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        if (data.TransactionAmount == 'NaN')
            data.TransactionAmount = 0;
        data.TaxAmount = 0;
        angular.forEach(data.TaxList, function (item) {
            item.TotalAmount = data.TransactionAmount * item.Percentage / 100;
            data.TaxAmount += item.TotalAmount;
        });
        data.NetAmount = parseFloat(data.TransactionAmount) + parseFloat(data.TaxAmount);
    };

    $scope.calculateRate = function (data) {
        data.TransactionRate = (data.TransactionAmount / data.TransactionQty).toFixed(2);
        if (data.TransactionRate == 'NaN')
            data.TransactionRate = 0;
        data.TaxAmount = 0;
        angular.forEach(data.TaxList, function (item) {
            item.TotalAmount = data.TransactionAmount * item.Percentage / 100;
            data.TaxAmount += item.TotalAmount;
        });
        data.NetAmount = parseFloat(data.TransactionAmount) + parseFloat(data.TaxAmount);
    };

    $scope.calculateServiceAmount = function (data) {
        if (data.Amount == 'NaN')
            data.Amount = 0;
        data.TaxAmount = 0;
        angular.forEach(data.ServiceTaxList, function (item) {
            item.TotalAmount = data.Amount * item.Percentage / 100;
            data.TaxAmount += item.TotalAmount;
        });
        data.NetAmount = parseFloat(data.Amount) + parseFloat(data.TaxAmount);
    };

    $scope.updateMaterialTax = function () {
        var data = $scope.salesMaterialList[$scope.currentMaterialRow];
    };

    $scope.chargesList = [];
    $scope.addCharge = function () {
        var data = {
            Amount: 0
        };
        $scope.chargesList.push(data);
    };

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
        accountService.getTaxCategoryCbo(" ", function (result) {
            $scope.taxCategoryList = result;
        });
    }

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

    $scope.addServiceTax = function () {
        var data = {
            TotalAmount: 0,
            Id: null,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null
        };
        $scope.ServicetaxPopList.push(data);
    };

    $scope.calculateTaxAmount = function (data) {

        data.TotalAmount = parseFloat($scope.taxAbleAmnt * data.Percentage / 100).toFixed(2);
    };
    $scope.checkRowValidation = function (x) {
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {

            if ($scope.receiveTaxList[i].Id === x.Id) {
                $scope.receiveTaxList[i].Percentage = (parseFloat(x.TotalAmount / $scope.taxAbleAmnt).toFixed(2) * 100);
            }

        }
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

    $scope.closeReceiveTaxPopUpwindow = function () {
        // getInventoryMaterialList($scope.productNew.Id);
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }

    $scope.closeServiceTaxPopUp = function () {
        var salesData = $scope.chargesList[$scope.currentServiceRow];
        //$scope.chargesList[$scope.currentServiceRow].TaxAmount = 0;
        $scope.chargesList[$scope.currentServiceRow].Amount = 0;
        angular.forEach($scope.receiveTaxList, function (item) {
            $scope.chargesList[$scope.currentServiceRow].TaxAmount += item.Amount;
        });
        //$scope.chargesList[$scope.currentServiceRow].NetAmount = $scope.chargesList[$scope.currentServiceRow].Amount + $scope.chargesList[$scope.currentServiceRow].TaxAmount;
        $scope.chargesList[$scope.currentServiceRow].NetAmount = $scope.chargesList[$scope.currentServiceRow].Amount;

        //  $scope.materialMaster = {};
        //  $scope.ServicetaxPopList = [];
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
    };

    $scope.getServiceTax = function (index) {
        $scope.currentServiceIndex = index;
        var data = $scope.chargesList[$scope.currentServiceIndex];
        var TaxList = [];
        var hsnCodeId = $filter("filter")($scope.serviceList, { HSNCodeId: data.ServiceMasterId })[0].HSNCodeId;
        $http({
            method: 'GET',
            url: 'Accounts/TaxCategory/GetTaxCategoryList?partyPlantId=' + $scope.salesVM.PartyPlantId + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            TaxList = response.data;
            $scope.chargesList[$scope.currentServiceIndex].ServiceTaxList = TaxList;
        });
    };

    $scope.getServiceTaxList = function (data, flag, ServiceId, index) {
        $scope.isService = true;

        $scope.LoadTaxButtonClick();
        $scope.Currency = $("#currency option:selected").text();
        $scope.ServiceId = ServiceId;
        if (!$scope.isService) {
            $scope.taxAbleAmnt = data.TransactionAmount;
        }
        else {
            $scope.taxAbleAmnt = data.Amount;
        }
        $scope.percentageColumn = flag;
        $scope.currentServiceRow = index;
        $scope.receiveTaxList = [];
        if (data.ServiceTaxList.length > 0) {
            $scope.HSNCode = data.ServiceTaxList[0].HSNCode;
            $scope.receiveTaxList = data.ServiceTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        $scope.salesVM.TaxOptionServiceModify = 'Yes';
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');
    }

    $scope.closeServiceChargeTaxPopUpwindowFG = function () {
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
    }

    $scope.calculateTaxAmountForService = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        // data.Amount = Math.round($scope.serviceModel.Amount * data.Percentage) / 100;
        data.Amount = parseFloat(($scope.serviceModel.Amount * data.Percentage) / 100).toFixed(2);
        $scope.calculateSvcTaxCategory();
    };

    $scope.checkRowValidationService = function (x) {
        for (var i = 0; i < $scope.taxCategoryList.length; i++) {

            if ($scope.taxCategoryList[i].Id === x.Id) {
                $scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.serviceModel.Amount).toFixed(2) * 100);
            }
        }
    }

    $scope.calculateTaxAmountForServiceModify = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.Amount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
    };
    $scope.checkRowValidationServiceModify = function (x) {
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {

            if ($scope.receiveTaxList[i].Id === x.Id) {
                $scope.receiveTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(2) * 100);
            }
        }
    }

    $scope.closeServiceChargeTaxPopUpwindow = function () {
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
    }

    $scope.closeServiceChargeTaxPopUpwindowFG = function () {
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
    }

    $scope.calculateSvcTaxCategory = function () {
        $scope.serviceModel.TaxAmount = 0;
        $scope.serviceModel.NetAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].Amount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.Amount) / 100).toFixed(2);
            $scope.serviceModel.TaxAmount = (parseFloat($scope.serviceModel.TaxAmount) + parseFloat($scope.taxCategoryList[i].Amount)).toFixed(2);
        }
        if (isNaN($scope.serviceModel.TaxAmount)) $scope.serviceModel.TaxAmount = 0;
        //$scope.serviceModel.NetAmount = parseFloat($scope.serviceModel.TaxAmount) + $scope.serviceModel.Amount;
        $scope.serviceModel.NetAmount = $scope.serviceModel.Amount;
    };
    $scope.sumSvcTaxAmount = function () {
        $scope.serviceModel.TaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.serviceModel.TaxAmount = (parseFloat($scope.serviceModel.TaxAmount) + parseFloat($scope.taxCategoryList[i].Amount)).toFixed(2);
        }
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

    $scope.changeService = function (id) {
        $scope.serviceModel.ServiceMasterId = id;
        if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
        var HSNCode = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCode;
        $scope.serviceModel.ChargeName = angular.element("#charge :selected").text();
        getServiceTaxCategoryList(hsnCodeId, HSNCode);
    };

    $scope.serviceChargePopUp = function () {
        //if (baseService.arrayLength($scope.salesMaterialList) === 0)
        //    return ShowResult('Without material charges not aplicable.');

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
    };
    $scope.chargeValidation = function () {
        var getRowCharge = $filter("filter")($scope.chargesList, { "ServiceMasterId": $scope.serviceModel.ServiceMasterId });
        if (getRowCharge == 0) {
            $scope.invalidcharges = false;
        }
        else {
            ShowResult('This Charge  already exsist', 'failure', 'serviceChargePopUp');
            $scope.invalidcharges = true;
        }
    }

    $scope.closeServiceChargeAddPopUp = function () {
        //$scope.serviceModel.TaxAmount = $filter("sumByKey")($filter("filter")($scope.taxCategoryList), "TotalAmount");
        $scope.serviceModel.TaxAmount = $filter("sumByKey")($filter("filter")($scope.taxCategoryList), "Amount");
        $scope.serviceModel.ServiceTaxList = $scope.taxCategoryList;
        $scope.chargeValidation();
        if (!$scope.invalidcharges) {
            $scope.chargesList.push($scope.serviceModel);
            angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
        }
    }

    $scope.closeServiceChargePopUp = function () {
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');

    }

    $scope.calculateServicePopupAmount = function (data) {
        if (data.Amount == 'NaN')
            data.Amount = 0;
        data.TaxAmount = 0;
        angular.forEach($scope.taxCategoryList, function (item) {
            item.TotalAmount = data.Amount * item.Percentage / 100;
            data.TaxAmount += item.TotalAmount;
        });
        data.NetAmount = data.TaxAmount + data.Amount;
    };


    //#region Additional TAX Code
    $scope.advanceTax = { TotalSumAfterTCSVal: 0 };
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
            $scope.TotalSumAfterTCS();
        }

    };

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
    //$scope.getTaxCodeByTaxYearWithhold($filter("dateFiltering")(Date.now()));
    $scope.selectadditionalTax = function () {
        $scope.advanceTax.ValueOfFixed = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].ValueOfFixed;
        $scope.advanceTax.Type = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].Type;
        $scope.advanceTax.TaxCategoryId = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].TaxCategoryId;

        if ($scope.advanceTax.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.advanceTax.ValueOfFixed)) {//* $scope.advanceTax.ValueOfFixed / 100

            $scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceTax"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
            //$scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
        }
        else {
            $scope.advanceTax.TaxAmount = $scope.advanceTax.ValueOfFixed;
        }
        $scope.TotalSumAfterTCS();
    }

    $scope.SaveAdditinalTax = function () {
        try {
            if ($scope.salesVM.IsPark == 0) {
                throw "Posted data cann't save";
            }
            if (baseService.arrayLength($scope.advanceTaxesList) == 0) {
                throw "Add row for Additional Tax.";
            }
            $http({
                method: 'POST',
                url: 'SalesManagements/Sales/SaveAdditinalTax',
                data:
                {
                    'salesId': $scope.salesVM.Id,
                    'BooksCurrencyBaseRate': $scope.salesVM.CompanyCurrencyRate,
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
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

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
    $scope.removeTaxesRow = function (Id, index) {
        if (baseService.isUndefinedOrNull(Id)) {
            $scope.advanceTaxesList.splice(index, 1);

        }
        else {
            $scope.DeleteAdditinalTax(Id);
            $scope.GetAdvanceTaxInfo($scope.salesVM.Id);
        }
    };
    $scope.DeleteAdditinalTax = function (Id) {
        $http({
            method: 'POST',
            url: 'SalesManagements/Sales/AdditionalTaxDelete?Id=' + Id,
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
        $scope.salesVM.TaxOptionAddiTax = data;
    };

    $scope.calculateTaxAmountForAdditionalTax = function (data) {
        $scope.TaxAmountVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceTax"))).toFixed(2);
        //$scope.TaxAmountVal = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
        $scope.advanceTax.TaxAmount = (($scope.TaxAmountVal * data) / 100).toFixed(2);

    };
    $scope.checkRowValidationSdditionalTax = function (data) {

        $scope.TaxAmountVal1 = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceTax"))).toFixed(2);
        $scope.advanceTax.ValueOfFixed = ((data / $scope.TaxAmountVal1) * 100).toFixed(4);
    }
    //$scope.TotalSumAfterTCSVal = "";
    $scope.TotalSumAfterTCS = function () {

        $scope.advanceTax.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);
    }

    //#endregion









}