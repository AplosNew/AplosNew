"use strict";
customerInvoiceController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller","$window"];
function customerInvoiceController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $window) {
    $scope.voucherDetailList = [];
    $scope.taxCodDataList = [];
    $scope.Action = "Save";
    $scope.partyGLList = [];
    $scope.taxExemption = false;
    $scope.IsBaseOnDueDateEnable = true;
    $scope.url = "accounts/Invoice";
    $scope.listUrl = "accounts/CustomerInvoice/GetCustomerInvoiceList";
    $scope.saveUrl = $scope.url + "/InsertCustomerInvoice";
    $scope.updateUrl = $scope.url + "/UpdateCustomerAdvance";
    $scope.reportUrl = $scope.url + "/ReportCustomerAdvance?voucherId=";
    $scope.jouranlUrl = $scope.url + "/GetAvailableJournalCustomerAdvance";
    $scope.postUrl = $scope.url + "/PostCustomerInvoice";

    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.partyType = "Customer";
    $scope.partyGLType = "Reconciliation";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.deleteUrl = $scope.url + "/DeleteCustomerInvoice";

    $scope.voucher = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        EntityId: null,
        PlantId: null,
        PartyId: null,
        PartyName: null,
        PartyType: null,
        CurrencyId: null,
        PaymentTermId: null,
        SourceType: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        MatureDate: null,
        DocRefNo: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        TaxYearId: null,
        TaxYearName: null,
        TaxYearPeriodId: null,
        TaxYearPeriodName: null,
        IsExcludingTax: false,
        IsSplit: false,
        Amount: null,
        Narration: null,
        Remarks: null,
        BankName: null,
        BankMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        SourceFrom: null,
        SourceTo: null,

        DrGLId: null,
        DrGLName: null,
        DrBudgetId: null,
        DrBudgetName: null,
        DrActivityId: null,
        DrActivityName: null,

        CrGLId: null,
        CrGLName: null,
        CrBudgetName: null,
        CrBudgetId: null,
        CrActivityId: null,
        CrActivityName: null,

        GLGeneralInfoId: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetName: null,
        ActivityId: null,
        ActivityName: null,

        EmployeeGLGeneralInfoId: null,
        EmployeeGLGeneralInfoName: null,
        EmployeeTransactionTypeId: null,
        PartyPlantId: null,
        DeliveryPartyPlantId: null,
        IsGovtSubsidy: false
    };

    $scope.voucherDetail = {
        EntityId: null,
        InvoiceTaxViewModel: [
            {
                TaxAmount: null,
                TaxAutoAmount: null,
                TaxCodeId: null,
                InvoiceDetailId: null,
                InvoiceDetailOppositEntryId: null,
                Id: null,
                WithholdCreditableGL: null,
                ExpensesGL: null,
                CreditableGL: null,
                IsWithhold: null,
                IsCreditable: null,
                IsMerge: null
            }
        ]
    };

    $scope.invoiceTax = {
    };

    baseService.init($scope.listUrl, null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.invoiceList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };

    $scope.getData();

    $scope.searchInvoiceList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            "name": "Customer Code",
            "value": "PartyCode"
        },
        {
            "name": "Customer",
            "value": "PartyName"
        },
        {
            "name": "Ordering Customer",
            "value": "PartyPlantName"
        },
        {
            "name": "Posting Date",
            "value": "PostingDate"
        },
        {
            "name": "Entity",
            "value": "EntityName"
        },
        {
            "name": "Doc Date",
            "value": "DocDate"
        },
        {
            "name": "Doc Ref",
            "value": "DocRefNo"
        },
        {
            "name": "Currency",
            "value": "CurrencyCode"
        }
        ,
        {
            "name": "Status",
            "value": "Status"
        }
    ];

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, "", function (result) {
                $scope.entityList = result;
            });
    });

    $scope.getBudgetCboByGL = function (glgeneralInfoId) {
        cboService.getBudgetCboByGL(glgeneralInfoId, function (result) {
            $scope.BudgetItemList = result;
            if ($scope.BudgetItemList.length === 1) {
                $scope.voucherDetail.BudgetMasterId = $scope.BudgetItemList[0].Value;
                $scope.voucherDetail.BudgetName = $scope.BudgetItemList[0].Text;
                $scope.getActivity($scope.voucherDetail.BudgetMasterId);
            }
        });
    };

    $scope.SelectedBudgetItem = function (id) {
        $scope.voucherDetail.BudgetName = $("#budgetid option:selected").text();
        $scope.voucherDetail.BudgetMasterId = id;
        $scope.getActivity(id);
    };

    $scope.ActivityList = [];
    $scope.getActivity = function (id) {
        $http({
            method: "GET",
            url: "accounts/Budget/GetBudgetActivityCbo?budgetId=" + id
        }).then(function successCallback(response) {
            $scope.ActivityList = response.data;
            if ($scope.ActivityList.length === 1) {
                $scope.voucherDetail.ActivityName = $scope.ActivityList[0].Text;
                $scope.voucherDetail.ActivityId = $scope.ActivityList[0].Value;
            }
        });
    };

    $scope.SelectedActivityItem = function (id) {
        $scope.voucherDetail.ActivityName = $("#activityid option:selected").text();
        $scope.voucherDetail.ActivityId = id;
    };

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.PostingDate + "&currencyId=" + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.voucher.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };


    $scope.getFiscalYearPeriod = function (date) {
        if (!baseService.isUndefinedOrNull(date) && !$scope.invalidPostingDate) {
            $http({
                method: "get",
                url: "accounts/CompanyFiscalYear/CheckingFiscalYearPeriod?postingDate=" + $filter("dateFiltering")(date)
            }).then(
                function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.currencyExchangeRate = [];
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        var result = response.data;
                        if (result.IsTransationLocked === true) {
                            ShowResult(commonMessage.FiscalPeriodTransactionLocked, "failure");
                            $scope.voucher.PostingDate = "";
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else if (result.IsExchangeRateConfirmed === false) {
                            ShowResult(commonMessage.FiscalPeriodExchangeRateConfirmed, "failure");
                            $scope.voucher.PostingDate = "";
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else {
                            $scope.voucher.FiscalYearId = result.FiscalYearId;
                            $scope.voucher.FiscalYearName = result.FiscalYearName;
                            $scope.voucher.FiscalYearPeriodId = result.FiscalYearPeriodId;
                            $scope.voucher.FiscalYearPeriodName = result.PeriodName;
                            $scope.GetCurrencyExchangeRateList();
                            $scope.changePaymentTerm($scope.voucher.PaymentTermId);
                        }
                    }
                },
                function errorCallback(response) {
                });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.paymentTerm = function () {
        if ($scope.partyType === "Customer")
            $scope.paymenttermUrl = "accounts/PaymentTerm/getcustomercbo";
        else if ($scope.partyType === "Vendor")
            $scope.paymenttermUrl = "accounts/PaymentTerm/getvendorcbo";
        $http({
            method: "GET",
            url: $scope.paymenttermUrl
        }).then(function successCallback(response) {
            $scope.paymentTermList = response.data;
        });
    };
    $scope.paymentTerm();

    $scope.changeCurrencyExchangeRate = function () {
        angular.forEach($scope.voucherDetailList, function (item, i) {

        });
    };

    $scope.changePaymentTerm = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) {
                return item.Value === id;
            })[0];
            $scope.voucher.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.voucher.BaseNoOfDays = paymentTerm.NoOfDay;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === "documentdate") {
                    $scope.voucher.BaseOnDueDate = $scope.voucher.DocDate;
                    $scope.IsBaseOnDueDateEnable = true;
                } else if (paymentTerm.BaseLineDate === "postingdate") {
                    $scope.voucher.BaseOnDueDate = $scope.voucher.PostingDate;
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else if (paymentTerm.BaseLineDate === "voucherdate") {
                    $scope.voucher.BaseOnDueDate = $filter("dateFiltering")(Date.now());
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else {
                    $scope.IsBaseOnDueDateEnable = false;
                    $scope.voucher.BaseOnDueDate = $filter("dateFiltering")(Date.now());
                }
            $scope.getMatureDate($scope.voucher.BaseOnDueDate, $scope.voucher.BaseNoOfDays);
        }
    };

    $scope.getMatureDate = function (date, days) {
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.voucher.MatureDate = $filter("date")(date, "dd-MMM-yyyy");
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            msg = "Doc date must be below or equal to Posting Date!";
            $scope.invalidDocDate = true;
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.voucher.FiscalYearId = null;
            $scope.voucher.FiscalYearName = null;
            $scope.voucher.FiscalYearPeriodId = null;
            $scope.voucher.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.invalidEntity = false;
    $scope.entityValidation = function () {
        $scope.invalidEntity = baseService.isUndefinedOrNull($scope.voucher.EntityId);
        if (baseService.isUndefinedOrNull($scope.voucher.EntityId)) {
            return manualValidation("div_entity", $scope.invalidEntity, "Entity is required.");
        }
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.voucherDetailList[i].EntityId)) {
                $scope.invalidEntity = baseService.isUndefinedOrNull($scope.voucherDetailList[i].EntityId);
                return ShowResult("Entity is required where GL is " + $scope.voucherDetailList[i].GLGeneralInfoName, "failure");
            }
        }
    };

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return true;
        }
        if ($scope.partyType === "Customer") {
            if (baseService.isUndefinedOrNull($scope.voucher.PartyId)) {
                ShowResult("Please select Customer!", "failure");
                return true;
            }
            if (parseFloat($scope.voucher.Amount) === 0) {
                ShowResult("Invoice Amount must greater than 0!", "failure");
                return true;
            }
            if ($scope.voucher.IsSplit) {
                var vdetailDrAmount = $filter("sumByKey")($filter("filter")($scope.voucherDetailList, { TrnType: "Dr" }), "DrAmount");
                if (parseFloat($scope.voucher.Amount) !== vdetailDrAmount) {
                    ShowResult("Splitted Amount is not equal  Invoice Amount!", "failure");
                    return true;
                }
            }
            var vdetailCrAmount = $filter("filter")($scope.voucherDetailList, { TrnType: "Cr" });
            if (vdetailCrAmount.length === 0) {
                ShowResult("There is no Sales Entry!", "failure");
                return true;
            }
            else {
                for (var i = 0; i < vdetailCrAmount.length; i++) {
                    if (vdetailCrAmount[i].Amount == 0) {
                        ShowResult(vdetailCrAmount[i].GLGeneralInfoName + " Amount must greater than 0!", "failure");
                        return true;
                    }
                }
            }
        }
        
        if ($scope.voucher.IsExcludingTax) {
            for (var j = 0; j < $scope.voucherDetailList.length; j++) {
                if ($scope.voucherDetailList[j].InvoiceTaxViewModel.length > 0) {
                    for (var i = 0; i < $scope.voucherDetailList[j].InvoiceTaxViewModel.length; i++) {

                        if ($scope.voucherDetailList[j].InvoiceTaxViewModel[i].IsRCM == 0) {
                            ShowResult("Please select RCM base Tax !", "failure");
                            return true;
                        }
                    }
                }
            }
        }
        if ($scope.voucher.IsExcludingTax == false) {
            for (var j = 0; j < $scope.voucherDetailList.length; j++) {
                if ($scope.voucherDetailList[j].InvoiceTaxViewModel.length > 0) {
                    for (var i = 0; i < $scope.voucherDetailList[j].InvoiceTaxViewModel.length; i++) {
                        if ($scope.voucherDetailList[j].InvoiceTaxViewModel[i].IsRCM == 1) {
                            ShowResult("Including tax is not allow RCM base Tax !", "failure");
                            return true;
                        }

                    }
                }
            }

        }
        return false;
    };

    $scope.closePartyPopUp = function (x) {
            var party = x.data;
            if (baseService.isUndefinedOrNull(party.ReconciliationGLId)) {
                ShowResult($scope.partyType + " GL not found!", "failure", "partyPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.ReconciliationBudgetId)) {
                ShowResult($scope.partyType + " Budget not found!", "failure", "partyPopUp");
                return;
            }
            else {
                $scope.removeDrRow();
                $scope.voucher.PartyId = party.Id;
                $scope.voucher.PartyCode = party.Code;
                $scope.voucher.PartyName = party.UserName;
                $scope.voucher.PartyType = $scope.partyType;
                $scope.voucher.GLGeneralInfoId = party.ReconciliationGLId;
                $scope.voucher.GLGeneralInfoCode = party.ReconciliationGLCode;
                $scope.voucher.GLGeneralInfoName = party.ReconciliationGLName;
                $scope.voucher.CurrencyId = party.CurrencyId;
                $scope.voucher.BudgetMasterId = party.ReconciliationBudgetId;
                $scope.voucher.BudgetCode = party.ReconciliationBudgetCode;
                $scope.voucher.BudgetName = party.ReconciliationBudgetName;
                $scope.voucher.ActivityId = party.ReconciliationActivityId;
                $scope.voucher.ActivityCode = party.ReconciliationActivityCode;
                $scope.voucher.ActivityName = party.ReconciliationActivityName;
                $scope.voucher.PaymentTermId = party.PaymentTermId;
                if ($scope.voucher.PaymentTermId !== null) {
                    $scope.changePaymentTerm($scope.voucher.PaymentTermId);
                }
                $scope.taxCodDataList = [];
                $scope.getPartyPlantList(party.Id);
                $scope.GetCurrencyExchangeRateList();
                clearVoucherDetail();
            }
        $scope.hidePartyPopUp();
    };

    $scope.updatePartyAmount = function () {
        var row = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr" });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            row[0].Amount = $scope.voucher.Amount;
        }
    };

    $scope.addRow = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.selectedInvoiceGLId)) {
            ShowResult("Please select GL.", "failure");
            return;
        }
        var getRow = null;
        if ($scope.companyConfig.IsVoucherFromBudget) {
            if (baseService.isUndefinedOrNull($scope.voucherDetail.BudgetMasterId)) {
                ShowResult("Please select Budget.", "failure");
                return;
            }
            else
                getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Cr", "GLGeneralInfoId": $scope.selectedInvoiceGLId, "BudgetMasterId": $scope.voucherDetail.BudgetMasterId, "ActivityId": $scope.voucherDetail.ActivityId });
        }
        else
            getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Cr", "GLGeneralInfoId": $scope.selectedInvoiceGLId });
        if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].GLGeneralInfoId == $scope.selectedInvoiceGLId) {
            ShowResult("This GL Budget and Activity is already added!", "failure");
        }
        else {
            $scope.voucherDetail.Id = baseService.pk();
            $scope.voucherDetail.GLGeneralInfoId = $scope.selectedInvoiceGLId;
            $scope.voucherDetail.GLGeneralInfoCode = $scope.selectedInvoiceGLCode;
            $scope.voucherDetail.GLGeneralInfoName = $scope.selectedInvoiceGLName;
            $scope.voucherDetail.PostingWithoutTaxAllow = $scope.selectedInvoiceGLPostingWithoutTaxAllow;

            $scope.voucherDetail.DocDate = $scope.voucher.DocDate;
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
            $scope.voucherDetail.Amount = null;

            $scope.voucherDetail.TrnType = $scope.partyType === "Customer" ? "Cr" : "Dr";
            $scope.voucherDetail.InvoiceTaxViewModel = [];
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            clearVoucherDetail();
            $scope.searchStr = null;
        }
    };


    $scope.removeDrRow = function () {
        var dr = $scope.voucherDetailList.length;
        while (dr--) {
            if ($scope.voucherDetailList[dr]["TrnType"] === "Dr") {
                $scope.voucherDetailList.splice(dr, 1);
            }
        }
    };

    $scope.removeRow = function (index) {
        var row = $scope.voucherDetailList[index];
        $scope.voucherDetailList.splice(index, 1);
    };


    $scope.invoiceGLList = [];
    var glUrl = null;
    if ($scope.partyType === "Customer") {
        glUrl = "accounts/GLItem/GetCustomerInvoiceGLList2";
    }
    else if ($scope.partyType === "Vendor") {
        glUrl = "accounts/GLItem/GetVendorInvoiceGLList";
    }

    $http.get(glUrl)
        .then(
        function successCallback(response) {
            $scope.invoiceGLList = response.data;
        },
        function errorCallback(response) {
            ShowResult(response, "failure");
        });

    $scope.selectedInvoiceGLId = null;
    $scope.selectedInvoiceGLName = null;
    $scope.selectedInvoiceGLPostingWithoutTaxAllow = null;
    $scope.selectedInvoiceGL = function (selected) {
        if (selected) {
            $scope.selectedInvoiceGLId = selected.originalObject.GLGeneralInfoId;
            $scope.selectedInvoiceGLName = selected.originalObject.GLGeneralInfoName;
            $scope.selectedInvoiceGLPostingWithoutTaxAllow = selected.originalObject.PostingWithoutTaxAllow;
            if ($scope.companyConfig.IsVoucherFromBudget) {
                $scope.getBudgetCboByGL(selected.originalObject.GLGeneralInfoId);
            }
        }
    };

    $scope.inputChanged = function (str) {
        $scope.voucherDetail.GLGeneralInfoId = str;
    };

    function clearVoucherDetail() {
        $scope.voucherDetail = {};
    }

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher = {};
        $scope.voucher.Active = true;
        $scope.voucher.Amount = null;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.getCboVoucherTypeAccountReceivableList();
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
        $scope.taxCodDataList = [];
        $scope.voucherDetailId = {};
        $scope.voucherDetail.InvoiceTaxViewModel = [];
        $scope.setTaxVoucherDetailIndex = {};
        $scope.selectedInvoiceGLId = null;
        $scope.plantList = [];
        $scope.GLGeneralInfoName = null;
        $scope.BudgetItemList = [];
        $scope.ActivityList = [];
        $scope.billToAddress = null;
        $scope.shipToAddress = null;
        clearVoucherDetail();
    };


    $scope.getCboVoucherTypeAccountReceivableList = function () {
        cboService.getCboVoucherTypeAccountReceivableList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                if ($scope.voucherTypeList[0].LastPostingDate != null) {
                    $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                }
                else {
                    $scope.voucher.PostingDate = $filter("dateFiltering")($filter("dateFiltering")(Date.now()));

                }
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);
                $scope.GetCurrencyExchangeRateList();
            }
        });
    };
    $scope.getCboVoucherTypeAccountReceivableList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.advance.VoucherTypeId = data.Value;
        $scope.advance.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.advance.DocDate = $scope.advance.PostingDate;
    };

    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYear = function (date) {
        $http({
            method: "get",
            url: "accounts/TaxCode/GetTaxCodeOutputVATGST?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message;
                }
                else {
                    var result = response.data;
                    $scope.taxCodCboList = result;
                    if ($scope.taxCodCboList.length === 0) {
                        return ShowResult("No TaxCode found in this Fiscal Year", "failure");
                    }
                }
            },
            function errorCallback(response) {
            });
    };


    $scope.ontaxCodeChange = function (item) {
        $http({
            method: "get",
            url: "accounts/taxcode/GetTaxCodeById?id=" + item
        }).then(function successCallback(response) {
            $scope.taxcodedata = response.data;
        });
    };

    $scope.voucherDetailId = "";
    $scope.getTaxCode = function (index, voucherDetailId) {
        $scope.setTaxVoucherDetailIndex = index;
        $scope.voucherDetailId = voucherDetailId;
        angular.element(document.querySelector("#texCodePopUp")).modal("show");
    };

    $scope.closeTaxCodePopUp = function () {
        $scope.setTaxVoucherDetailIndex = null;
        angular.element(document.querySelector("#texCodePopUp")).modal("hide");
    };

    $scope.addTaxCodeonList = function (item) {
        $http({
            method: "get",
            url: "accounts/taxcode/GetTaxCodewithPersentageById?id=" + item + '&postingDate=' + $scope.voucher.PostingDate
        }).then(function successCallback(response) {
            $scope.taxcodedata = response.data;
            var ob = {
                Code: $scope.taxcodedata.Code,
                Type: $scope.taxcodedata.Type,
                ValueOfFixed: $scope.taxcodedata.ValueOfFixed,
                Description: $scope.taxcodedata.Description,
                UserName: $scope.taxcodedata.UserName,
                VoucherDetailId: $scope.voucherDetailId,
                Sequence: 1,
                TaxAmount: '',
                TaxAutoAmount: 0,
                TaxCodeId: $scope.taxcodedata.TaxCodeId,
                TaxCategoryId: $scope.taxcodedata.TaxCategoryId,
                InvoiceDetailId: null,
                Id: null,
                WithholdCreditableGLId: $scope.taxcodedata.WithholdCreditableGLId,
                ExpensesGLId: $scope.taxcodedata.ExpensesGLId,
                CreditableGLId: $scope.taxcodedata.CreditableGLId,
                IsWithhold: $scope.taxcodedata.IsWithhold,
                IsCreditable: $scope.taxcodedata.IsCreditable,
                IsRCM: $scope.taxcodedata.IsRCM,
                IsMerge: $scope.taxcodedata.IsMerge,
                ManuallyEditable: $scope.taxcodedata.ManuallyEditable
            };

            var getRow = $filter("filter")($scope.taxCodDataList, { "TaxCodeId": ob.TaxCodeId, "VoucherDetailId": $scope.voucherDetailId });
            if (getRow.length === 0) {
                $scope.voucherDetailList[$scope.setTaxVoucherDetailIndex].PostingWithoutTaxAllow = true;
                var vdetailrow = $scope.voucherDetailList[$scope.setTaxVoucherDetailIndex]
                if ($scope.taxcodedata.Type = 'FixedPercentage') {
                    ob.TaxAmount = parseFloat((vdetailrow.Amount * ob.ValueOfFixed) / 100).toFixed(2);
                }
                $scope.voucherDetailList[$scope.setTaxVoucherDetailIndex].InvoiceTaxViewModel.push(ob);
                $scope.taxCodDataList.push(ob);

            }
            else {
                ShowResult("Tax code (<b>" + ob.UserName + "</b>) is already added !!!", "failure", "texCodePopUp");
            }
        });
    };

    $scope.calculatebackTax = function (index) {
        if ($scope.voucherDetailList[index].InvoiceTaxViewModel.length > 0) {
            for (var i = 0; i < $scope.voucherDetailList[index].InvoiceTaxViewModel.length; i++) {
                $scope.voucherDetailList[index].InvoiceTaxViewModel[i].TaxAmount = Math.round((($scope.voucherDetailList[index].Amount * $scope.voucherDetailList[index].InvoiceTaxViewModel[i].ValueOfFixed) / 100) * 100 + Number.EPSILON) / 100
            }
        }
    };
    $scope.calculateTax = function (voucherDetailId, index) {
        if ($scope.voucherDetailList[index].InvoiceTaxViewModel.length > 0) {
            if ($scope.voucher.IsExcludingTax) {
                $scope.voucherDetailList[index].TotalTax = Math.round(($filter("sumByKey")($filter("filter")($scope.voucherDetailList[index].InvoiceTaxViewModel), "TaxAmount")) * 100 + Number.EPSILON) / 100;
                $scope.voucherDetailList[index].TotalAmount = Math.round(($scope.voucherDetailList[index].Amount) * 10000 + Number.EPSILON) / 10000;
                $scope.voucher.Amount = Math.round(($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount")) * 10000 + Number.EPSILON) / 10000;

            }
            else {
                $scope.voucherDetailList[index].TotalTax = Math.round(($filter("sumByKey")($filter("filter")($scope.voucherDetailList[index].InvoiceTaxViewModel), "TaxAmount")) * 10000 + Number.EPSILON) / 10000;
                $scope.voucherDetailList[index].TotalAmount = Math.round(($scope.voucherDetailList[index].Amount + $scope.voucherDetailList[index].TotalTax) * 10000 + Number.EPSILON) / 10000;

                $scope.voucher.Amount = Math.round(($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount") + $filter("sumByKey")($filter("filter")($scope.voucherDetailList), "TotalTax")) * 10000 + Number.EPSILON) / 10000;
            }
        }
        else {
            $scope.voucherDetailList[index].TotalAmount = Math.round(($scope.voucherDetailList[index].Amount) * 10000 + Number.EPSILON) / 10000;
            $scope.voucherDetailList[index].TotalTax = 0;
            $scope.voucher.Amount = Math.round(($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount")) + ($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "TotalTax")) * 10000 + Number.EPSILON) / 10000;
        }
    };

    $scope.excludeTaxCalculate = function () {
        if ($scope.voucherDetailList.length > 0) {
            for (var i = 0; i < $scope.voucherDetailList.length; i++) {
                if ($scope.voucher.IsExcludingTax)
                    $scope.voucherDetailList[i].TotalAmount = Math.round(($scope.voucherDetailList[i].Amount) * 10000 + Number.EPSILON) / 10000
                else
                    $scope.voucherDetailList[i].TotalAmount = Math.round(($scope.voucherDetailList[i].Amount + $scope.voucherDetailList[i].TotalTax) * 10000 + Number.EPSILON) / 10000
            }
            $scope.recalculationAdditionaltax();
        }
    };

    //$scope.calculateTax = function (voucherDetailId) {
    //    for (var i = 0; i < $scope.voucherDetailList.length; i++) {
    //        if ($scope.voucherDetailList[i].Id === voucherDetailId) {
    //            $scope.voucherDetailList[i].TotalTax = null;
    //        }
    //    }
    //    for (var t = 0; t < baseService.arrayLength($scope.voucherDetailList); t++) {
    //        for (var a = 0; a < baseService.arrayLength($scope.voucherDetailList[t].InvoiceTaxViewModel); a++) {
    //            if ($scope.voucherDetailList[t].Id == voucherDetailId) {
    //                $scope.voucherDetailList[t].TotalTax += $scope.voucherDetailList[t].InvoiceTaxViewModel[a].TaxAmount;
    //            }
    //        }
    //        $scope.voucherDetailList[t].TotalAmount = $scope.voucherDetailList[t].Amount + $scope.voucherDetailList[t].TotalTax;
    //    }
    //};

    $scope.updateTaxAmount = function (data, amount) {
        if (!baseService.isUndefinedOrNull(data)) {
            for (var i = 0; i < $scope.taxCodDataList.length; i++) {
                if ($scope.taxCodDataList[i].TaxCodeId === data.TaxCodeId && $scope.taxCodDataList[i].VoucherDetailId == data.VoucherDetailId) {
                    $scope.taxCodDataList[i].TaxAmount = amount;
                }
            }
            for (var t = 0; t < baseService.arrayLength($scope.voucherDetailList); t++) {
                if ($scope.voucherDetailList[t].Id === data.VoucherDetailId)
                    for (var a = 0; a < baseService.arrayLength($scope.voucherDetailList[t].InvoiceTaxViewModel); a++) {
                        if ($scope.voucherDetailList[t].InvoiceTaxViewModel[a].TaxCodeId === data.TaxCodeId) {
                            $scope.voucherDetailList[t].InvoiceTaxViewModel[a].TaxAmount = amount;
                        }
                    }
            }
        }
    };

    $scope.taxCodeDelModal = function (taxcodeid, vdid, username) {
        $scope.TaxCodeId = taxcodeid;
        $scope.voucherDetailId = vdid;

        if (baseService.isUndefinedOrNull($scope.TaxCodeId))
            $scope.Taxmessage_confirmation = "Are you sure want to delete [ " + username + " ] data....";
        else
            $scope.Taxmessage_confirmation = "Are you sure want to delete [ " + username + " ] ?";
        angular.element(document.querySelector("#confirmTaxCodeDelPopUp")).modal("show");
    };

    $scope.removeTaxCodeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.TaxCodeId)) {
            for (var t = 0; t < baseService.arrayLength($scope.voucherDetailList); t++) {
                if ($scope.voucherDetailList[t].Id === $scope.voucherDetailId)
                    for (var a = 0; a < baseService.arrayLength($scope.voucherDetailList[t].InvoiceTaxViewModel); a++) {
                        if ($scope.voucherDetailList[t].InvoiceTaxViewModel[a].TaxCodeId === $scope.TaxCodeId) {
                            $scope.voucherDetailList[t].InvoiceTaxViewModel.splice(a, 1);
                        }
                    }
            }
            for (var i = 0; i < $scope.taxCodDataList.length; i++) {
                if ($scope.taxCodDataList[i].TaxCodeId === $scope.TaxCodeId) {
                    $scope.taxCodDataList.splice(i, 1);
                }
            }
            $scope.TaxCodeId = "";
        }
    };

    $scope.vendorInvoiceTaxes = [];

    $scope.vendorInvoiceTaxPush = function () {
        $scope.calculateTax($scope.voucherDetailId, $scope.setTaxVoucherDetailIndex);
        $scope.closeTaxCodePopUp();
    };

    $scope.checkTaxAllow = function () {
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if ($scope.voucherDetailList[i].TrnType === "Cr" && $scope.voucherDetailList[i].PostingWithoutTaxAllow === false) {
                ShowResult("PositionWithout Tax is not Allow where Amount " + $scope.voucherDetailList[i].Amount, "failure");
                return false;
            }
        }
        return true;
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        if ($scope.form1.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.voucherDetailList,
                        "taxDetailVMList": $scope.advanceTaxesList,
                        "otherInvoiceVM": $scope.otherVoucher
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: "accounts/Advance/UpdateCustomerAdvance",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.voucherDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.menuFrames[$scope.index] = $scope.menuFrame;
                        }
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            return true;
        }
    };

    $scope.report = function (voucherId) {
        location.href = "accounts/invoice/customerinvoicevoucherreport?voucherId=" + voucherId;
    };

    $scope.customerInvoiceGLSearchList = [
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "COA",
            "value": "COA"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];

    $scope.customerInvoiceGLParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        $scope.customerInvoiceGLList = [];
        baseService.setCurrentPage("customerInvoiceGLList");
        $scope.customerInvoiceGLGLData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetCustomerInvoiceGLBudgetList", pageno, $scope.customerInvoiceGLParameters)
                .then(function (result) {
                    $scope.customerInvoiceGLList = result.Rows;
                    $scope.customerInvoiceGLParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "CustomerInvoiceGLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#CustomerInvoiceGLPopUp")).modal("show");
        $scope.customerInvoiceGLGLData();
    };

    $scope.glSelect = function (data) {
        $scope.selectedInvoiceGLId = data.GLGeneralInfoId;
        $scope.selectedInvoiceGLCode = data.GLGeneralInfoCode;
        $scope.selectedInvoiceGLName = data.GLGeneralInfoName;
        $scope.voucherDetail.BudgetMasterId = data.BudgetMasterId;
        $scope.voucherDetail.BudgetCode = data.BudgetCode;
        $scope.voucherDetail.BudgetName = data.BudgetName;
        $scope.voucherDetail.ActivityId = data.ActivityId;
        $scope.voucherDetail.ActivityCode = data.ActivityCode;
        $scope.voucherDetail.ActivityName = data.ActivityName;
        $scope.addRow();
        $scope.closeGLPopUp();
    };

    $scope.closeGLPopUp = function () {
        angular.element(document.querySelector("#CustomerInvoiceGLPopUp")).modal("hide");
    };

    $scope.post = function (invoiceId) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "invoiceId": invoiceId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.Clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.invoiceId = null;
    $scope.confirmPost = function (invoiceId) {
        $scope.invoiceId = invoiceId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.clearPartyData = function () {
        $scope.voucher.PartyId = null;
        $scope.voucher.PartyName = null;
        $scope.voucher.PartyPlantId = null;
        $scope.partyPlantList = [];
    }

    //Delete Option

    $scope.delete = function (invoiceId, voucherId, deletedRemarks) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "invoiceId": invoiceId, "voucherId": voucherId, "deletedRemarks": deletedRemarks
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                $scope.deletedRemarks = "";
                $scope.closeconfirmDeletePopUp_Remarks();
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.Clear();
                $scope.invoiceId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.deletedRemarks = "";
    $scope.invoiceId = null;
    $scope.confirmDelete = function (invoiceId, voucherId) {
        $scope.invoiceId = invoiceId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp_Remarks")).modal("show");
    };

    $scope.closeconfirmDeletePopUp_Remarks = function () {
        angular.element(document.querySelector("#confirmDeletePopUp_Remarks")).modal("hide");
    };

    $scope.taxCodCboListWithhold = [];
    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYearWithhold = function (date) {
        $http({
            method: "get",
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
    $scope.getTaxCodeByTaxYearWithhold($filter("dateFiltering")(Date.now()));
    $scope.advanceTax = {
        TaxCodeId: null,
        Text: null,
        TaxAmount: null,
        ValueOfFixed: null,
        CompanyCurrencyAmount: null,
        Type: null
    };
    $scope.selectadditionalTax = function () {
        $scope.advanceTax.ValueOfFixed = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].ValueOfFixed;
        $scope.advanceTax.Type = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].Type;
        if ($scope.advanceTax.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.advanceTax.ValueOfFixed)) {
            $scope.advanceTax.TaxAmount = Math.round((($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "TotalAmount") * $scope.advanceTax.ValueOfFixed / 100)) * 10000 + Number.EPSILON) / 10000;
        }
    }

    $scope.advanceTaxesList = [];
    $scope.addTax = function () {
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
    };
    $scope.recalculationAdditionaltax = function () {
        if ($scope.advanceTaxesList.length > 0) {
            for (var i = 0; i < $scope.advanceTaxesList.length; i++) {
                $scope.advanceTaxesList[i].TaxAmount = Math.round(($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "TotalAmount") * $scope.advanceTaxesList[i].ValueOfFixed / 100) * 10000 + Number.EPSILON) / 10000;
            }
            $scope.TotalInvoiceAmount = Math.round(($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "TotalAmount")) + ($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount")) * 10000 + Number.EPSILON) / 10000;
        }
        else {
            $scope.TotalInvoiceAmount = Math.round(($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "TotalAmount")) * 10000 + Number.EPSILON) / 10000
        }
        //if ($scope.TDSList.length > 0) {
        //    for (var i = 0; i < $scope.TDSList.length; i++) {
        //        $scope.TDSList[i].TaxAmount = Math.round((($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount") * $scope.TDSList[i].ValueOfFixed / 100)) * 100 + Number.EPSILON) / 100;
        //    }

        //}
    }

    $scope.removeTaxesRow = function (index) {
        $scope.advanceTaxesList.splice(index, 1);
    };


    $scope.otherVoucher = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        EntityId: null,
        PlantId: null,
        PartyId: null,
        PartyName: null,
        PartyType: null,
        CurrencyId: null,
        PaymentTermId: null,
        SourceType: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        MatureDate: null,
        DocRefNo: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        TaxYearId: null,
        TaxYearName: null,
        TaxYearPeriodId: null,
        TaxYearPeriodName: null,
        IsExcludingTax: false,
        IsSplit: false,
        Amount: null,
        Narration: null,
        Remarks: null,
        BankName: null,
        BankMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        SourceFrom: null,
        SourceTo: null,

        DrGLId: null,
        DrGLName: null,
        DrBudgetId: null,
        DrBudgetName: null,
        DrActivityId: null,
        DrActivityName: null,

        CrGLId: null,
        CrGLName: null,
        CrBudgetName: null,
        CrBudgetId: null,
        CrActivityId: null,
        CrActivityName: null,

        GLGeneralInfoId: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetName: null,
        ActivityId: null,
        ActivityName: null,

        EmployeeGLGeneralInfoId: null,
        EmployeeGLGeneralInfoName: null,
        EmployeeTransactionTypeId: null,
        PartyPlantId: null,
        DeliveryPartyPlantId: null,
        IsGovtSubsidy: false
    };

    $scope.otherPartyPlantList = [];
    $scope.getOtherPartyPlantList = function (partyId) {
        $scope.otherPartyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.otherPartyPlantList.push(item);
                    if (item.IsDefault) {
                        $scope.otherPartyPlantId = item.Value;
                        $scope.otherVoucher.PartyPlantId = item.Value;
                        $scope.otherVoucher.DeliveryPartyPlantId = item.Value;
                        $scope.billToAddress = item.Address1;
                        $scope.shipToAddress = item.Address1;
                    }
                });
            });
    };
    $scope.showPartyOtherPopUpNew = function () {
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
        angular.element(document.querySelector('#partyOtherPopUp')).modal('show');
    };

    $scope.closeOtherPartyPopUp = function (x) {
        var party = x.data;
        if (baseService.isUndefinedOrNull(party.ReconciliationGLId)) {
            ShowResult($scope.partyType + " GL not found!", "failure", "partyPopUp");
            return;
        }
        else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.ReconciliationBudgetId)) {
            ShowResult($scope.partyType + " Budget not found!", "failure", "partyPopUp");
            return;
        }
        else {
            $scope.removeDrRow();
            $scope.otherVoucher.PartyId = party.Id;
            $scope.otherVoucher.PartyCode = party.Code;
            $scope.otherVoucher.PartyName = party.UserName;
            $scope.otherVoucher.PartyType = $scope.partyType;
            $scope.otherVoucher.GLGeneralInfoId = party.ReconciliationGLId;
            $scope.otherVoucher.GLGeneralInfoCode = party.ReconciliationGLCode;
            $scope.otherVoucher.GLGeneralInfoName = party.ReconciliationGLName;
            $scope.otherVoucher.CurrencyId = party.CurrencyId;
            $scope.otherVoucher.BudgetMasterId = party.ReconciliationBudgetId;
            $scope.otherVoucher.BudgetCode = party.ReconciliationBudgetCode;
            $scope.otherVoucher.BudgetName = party.ReconciliationBudgetName;
            $scope.otherVoucher.ActivityId = party.ReconciliationActivityId;
            $scope.otherVoucher.ActivityCode = party.ReconciliationActivityCode;
            $scope.otherVoucher.ActivityName = party.ReconciliationActivityName;
            $scope.getOtherPartyPlantList($scope.otherVoucher.PartyId);
        }
        $scope.hideOtherPartyPopUp();
    };
    $scope.hideOtherPartyPopUp = function () {
        angular.element(document.querySelector('#partyOtherPopUp')).modal('hide');

    }
    $scope.clearOtherPartyData = function () {
        $scope.otherVoucher.PartyId = null;
        $scope.otherVoucher.PartyName = null;
        $scope.otherVoucher.PartyPlantId = null;
        $scope.otherPartyPlantList = [];
    }

    $scope.ShowJournalPopUp = function (data) {
        $scope.OtherInvoiceJournal = data;
        $scope.OtherInvoiceJournalId = data.OtherInvoiceId;
        getOtherInvoiceJournal(data.Id);

        angular.element(document.querySelector('#JournalPopUp')).modal('show');
    }

    function getOtherInvoiceJournal(otherInvoieId) {
        $http.get('Accounts/Invoice/GetOtherInvoiceJournal?otherInvoieId=' + otherInvoieId)
            .then(function (response) {
                $scope.newList = [];
                $scope.newList = response.data;
            });
    }
    $scope.closeJournalPopUp = function () {
        angular.element(document.querySelector('#JournalPopUp')).modal('hide');
    }

    $scope.otherInvoicepost = function (id, data, otherInvoiceJVlist) {
        $http({
            method: "POST",
            url: 'Accounts/Invoice/InsertOtherInvoiceJournal',
            data: {
                "otherInvoiceId": id,
                "voucherVM": data,
                "voucherDetailVMList": otherInvoiceJVlist
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                if (tdsId != null) {
                }
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.invoiceId = null;
    $scope.confirmotherInvoicePost = function (id,data,list) {
        $scope.otherinvoiceId = id;
        $scope.data = data;
        $scope.otherInvoiceJVlist = list;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmotherInvoicePostPopUp")).modal("show");
    };

    $scope.OtherInvoiceVouchereReport = function (reportFormat, voucherId) {
        $window.open('Accounts/Invoice/CustomerInvoiceReceiptReport?reportFormat=' + reportFormat + '&voucherId=' + voucherId, '_blank');
    }

    $scope.invoiceSetOffDetailList = [];
    $scope.getInvoiceSetOffDetailByInvoice = function (id) {
        $scope.invoiceSetOffDetailList = [];
        $http({
            method: "get",
            url: "accounts/invoice/getInvoiceSetOffDetailByInvoice?invoiceId=" + id
        }).then(function successCallback(response) {
            $scope.invoiceSetOffDetailList = response.data;

            angular.element(document.querySelector('#invoiceetOffByInvoicePopUp')).modal('show');

        });
    };
    $scope.closeInvoiceSetOffDetailByInvoice = function () {
        angular.element(document.querySelector('#invoiceetOffByInvoicePopUp')).modal('hide');

    }
}