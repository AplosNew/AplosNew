"use strict";
debitNoteSetOffController.$inject = ["bankService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function debitNoteSetOffController(bankService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Debit Note SetOff";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.voucherInvoiceDetailList = [];
    $scope.taxCodCboList = [];
    $scope.currencyExchangeRate = [];
    $scope.partyFromTo = "From";
    $scope.bankFromTo = "To";
    $scope.isWriteOff = false;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.partyType = "Customer";
    $scope.isAdvance = false;
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    baseService.init("Accounts/AdjustmentNote/GetDebitNoteSetOffList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
    $scope.url = "Accounts/AdjustmentNote";
    $scope.postUrl = $scope.url + "/PostDebitNoteSetOff";
    $scope.deleteUrl = $scope.url + "/DeleteDebitNoteSetOff";

    $scope.voucher = {
        Id: null,
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
        IsExcludingTax: false,
        IsSplit: false,
        Amount: 0,
        Narration: null,
        Remarks: null,
        BankName: null,
        BankMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        SourceFrom: null,
        SourceTo: null,
        PaymentSource: "Bank",

        GLGeneralInfoId: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetName: null,
        ActivityId: null,
        ActivityName: null,

        EmployeeGLGeneralInfoId: null,
        EmployeeGLGeneralInfoName: null,
        EmployeeTransactionTypeId: null,

        InvoiceAmount: 0,
        ExGainLossAmount: 0,
        NetInvoiceAmount: 0,
        InvoiceGroupAmount: 0,
        ExGainLossGroupAmount: 0,
        CompanyCurrencyRate: 1,
        RoundingType: null,
        NoteType: 'CustomerDebitNote'
    };

    $scope.voucherDetail = {
        EntityId: null
    };

    $scope.GetCurrencyParallel = function () {
        $http({
            method: "GET",
            url: "currencies/CompanyParallelCurrency/CurrencyParallel"
        }).then(function successCallback(response) {
            $scope.CurrencyParallel = response.data;
            if ($scope.CurrencyParallel.length === 0) {
                $scope.pop("error", "Company Parallel Currency is not set!");
                $scope.showform = false;
            }
            else {
                $scope.showform = true;
            }
            $scope.BaseCurrencyCode = $scope.CurrencyParallel[0].Code;
        });
    };
    $scope.GetCurrencyParallel();

    $scope.tranCurrencyList = [];
    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
    });

    cboService.getEnumCbo("Enum/GetCboRoundingType", function (result) {
        $scope.roundingTypeList = result;
        $scope.voucher.RoundingType = $scope.roundingTypeList[0].Value;
    });

    $scope.searchVendorInvoiceList = [
        {
            "Text": "Voucher No",
            "Value": "VoucherNo"
        },
        {
            "Text": "Vendor/Party",
            "Value": "PartyName"
        },
        {
            "Text": "Posting Date",
            "Value": "PostingDate"
        },
        {
            "Text": "Currency Code",
            "Value": "CurrencyCode"
        },
        {
            "Text": "Amount",
            "Value": "Amount"
        },
        {
            "Text": "Status",
            "Value": "Status"
        },
        {
            "Text": "Party Type",
            "Value": "PartyType"
        },
        {
            "Text": "Payment Source",
            "Value": "PaymentSource"
        }
    ];

    $scope.parameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "VoucherNo",
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getData = function (pageno) {
        try {
            if ($scope.parameters.searchBy == "Status" && baseService.isUndefinedOrNull($scope.parameters.search)) {
                throw "Value is required.";
            }
            baseService.pagination(pageno, $scope.parameters)
            .then(function (result) {
                $scope.paymentList = result.Rows;
                $scope.parameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getData();

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
                $scope.getActivity(glgeneralInfoId);
            }
        });
    };

    $scope.ActivityList = [];
    $scope.getActivity = function (id) {
        $http({
            method: "GET",
            url: "accounts/Budget/GetBudgetActivityCbo?BudgetMasterId=" + id
        }).then(function successCallback(response) {
            $scope.ActivityList = response.data;
        });
    };

    $scope.exchangeGainLossList = [];
    $http.get("accounts/ExchangeGainLoss/GetExchangeGainLoss")
        .then(function successCallback(response) {
            $scope.exchangeGainLossList = response.data;
        },
        function errorCallback(response) {
            ShowResult(response, "failure");
        });

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
            $scope.currencyExchangeRate = null;
        }
    };

    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYear = function (date) {
        $http({
            method: "get",
            url: "accounts/TaxCode/GetAdditionalTaxCbo?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message;
                }
                else {
                    var result = response.data;
                    $scope.taxCodCboList = result;
                    console.log($scope.taxCodCboList);
                    if ($scope.taxCodCboList.length === 0) {
                        $scope.pop("error", "No TaxCode found in this Fiscal Year ");
                    }
                }
            },
            function errorCallback(response) {
            });
    };

    $http({
        method: "GET",
        url: "accounts/PaymentTerm/getcustomercbo"
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });

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
        else if (baseService.isUndefinedOrNull($scope.voucher.DocDate)) {
            msg = "Doc Date is required.";
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
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.voucher.PostingDate)) {
            msg = "Posting Date is required.";
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.checkPostingDateWithInvoice = function () {
        var msg = "";
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if (new Date($scope.voucherDetailList[i].PostingDate) > new Date($scope.voucher.PostingDate)) {
                msg = "Posting date must be below or equal to payable of " + $scope.voucherDetailList[i].VoucherNo;
                $scope.invalidPostingDate = true;
                break;
            }
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.invalidEntity = false;
    $scope.entityValidation = function () {
        $scope.invalidEntity = baseService.isUndefinedOrNull($scope.voucher.EntityId);
        return manualValidation("div_entity", $scope.invalidEntity, "Entity is required.");
    };

    $scope.getPartyWiseOutstandingAdvance = function (id) {
        $scope.partyWiseOutstandingAdvanceList = [];
        $http({
            method: "GET",
            url: "accounts/Advance/GetPartyWiseOutstandingAdvance?partyId=" + id
        }).then(function successCallback(response) {
            $scope.partyWiseOutstandingAdvanceList = response.data;
            $scope.TotalAdvanceAmount = $filter("sumByKey")($filter("filter")($scope.partyWiseOutstandingAdvanceList), "Balance");
            if ($scope.partyWiseOutstandingAdvanceList.length > 0) {
                angular.element(document.querySelector("#partyAdvanceAmountPopUp")).modal("show");
            }
        });
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
                $scope.voucher.PartyName = party.Code + " - " + party.UserName;
                $scope.voucher.PartyId = party.Id;
                $scope.voucher.PartyType = $scope.partyType;
                $scope.voucher.CurrencyId = party.CurrencyId;
                $scope.partyPlantList = [];
                $scope.getPartyPlantList(party.Id);
                $scope.GetCurrencyExchangeRateList();
                //$scope.getPartyWiseOutstandingAdvance($scope.voucher.PartyId);
                $scope.voucherDetailList = [];
            }
        $scope.hidePartyPopUp();
    };

    $scope.updatePartyAmount = function () {
        var row = $filter("filter")($scope.voucherDetailList, { "TrnType": "Cr" });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            row[0].Amount = $scope.voucher.Amount;
        }
    };

    $scope.showPartyAdvanceAmount = function () {
        angular.element(document.querySelector("#partyAdvanceAmountPopUp")).modal("show");
    };

    $scope.closePartyAdvanceAmount = function () {
        angular.element(document.querySelector("#partyAdvanceAmountPopUp")).modal("hide");
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
    $scope.selectedInvoiceGL = function (selected) {
        if (selected) {
            $scope.selectedInvoiceGLId = selected.originalObject.GLGeneralInfoId;
            $scope.selectedInvoiceGLName = selected.originalObject.GLGeneralInfoName;
        }
    };

    $scope.inputChanged = function (str) {
        $scope.voucherDetail.GLGeneralInfoId = str;
    };

    $scope.getCboVoucherTypePaymentList = function () {
        cboService.getCboVoucherTypeReceiptList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);

            }
        });
    };
    $scope.getCboVoucherTypePaymentList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
        $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);
    };

    $scope.getById = function (id) {
        $http({
            method: "GET",
            url: "accounts/Advance/GetAdvance/" + id
        }).then(function successCallback(response) {
            $scope.voucher = response.data;
            $scope.voucher.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
            $scope.voucher.VoucherDate = $filter("dateFiltering")($scope.voucher.VoucherDate);
            $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucher.PostingDate);
            $scope.Action = "Update";
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };

    $scope.debitNoteSearchList = [
        {
            "Text": "VoucherNo",
            "Value": "VoucherNo"
        },
        {
            "Text": "Entity",
            "Value": "EntityName"
        },
        {
            "Text": "Party Name",
            "Value": "PartyPlantName"
        },
        {
            "Text": "Posting Date",
            "Value": "PostingDate"
        },
        {
            "Text": "Doc Date",
            "Value": "DocDate"
        },
        {
            "Text": "Doc RefNo",
            "Value": "DocRefNo"
        }
    ];

    $scope.debitNoteParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "VoucherNo",
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.debitNoteList = [];
    $scope.getPopupDebitNoteList = function () {
        $scope.debitNoteGLData = function (pageno) {
            $scope.debitNoteList = [];
            $scope.customerReceivableGLUrl1 = "accounts/AdjustmentNote/GetDebitNoteAvailableList?partyId=" + $scope.voucher.PartyId + '&partyType=' + $scope.partyType;
            baseService.paginationBase($scope.customerReceivableGLUrl1, pageno, $scope.debitNoteParameters)
                .then(function (result) {
                    try {
                        $scope.debitNoteList = result.Rows;
                        $scope.debitNoteParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.debitNoteSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.debitNoteList, $scope.debitNoteSearchList);
                        }
                    } catch (e) {
                        ShowResult(e, "Error");
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#debitCreditNotePopUp")).modal("show");
        $scope.debitNoteGLData();
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector("#debitCreditNotePopUp")).modal("hide");
    };

    $scope.closedebitCreditNotePopUpselected = function () {
        angular.forEach($scope.debitNoteList, function (data, i) {
            if (data.Active === true) {
                data.TrnType = "Dr";
                data.PartyPlantName = data.PartyPlantName;
                var getRow = null;
                getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "DocRefNo": data.DocRefNo, "InvoiceDetailId": data.InvoiceDetailId });
                if (getRow.length === 0) {
                    data.Amount = data.Balance;
                    $scope.voucherDetailList.push(data);
                    if ($scope.voucher.PaymentSource == 'SetOff')
                        $scope.voucher.CompanyCurrencyRate = $scope.voucherDetailList[0].CompanyCurrencyRate;
                    $scope.exchangeGainLossAmount(data);
                    if ($scope.voucherDetailList.length > 0)
                        $scope.isReadOnly = true;
                    else
                        $scope.isReadOnly = false;

                    angular.element(document.querySelector("#debitCreditNotePopUp")).modal("hide");
                }
                else {
                    ShowResult(data.DocRefNo + " already  Exist", "failure", "debitCreditNotePopUp");
                }
            }
        });
    };

    $scope.exchangeGainLossAmount = function (data) {
        var balance = parseFloat(data.Balance), dramount = parseFloat(data.Amount);
        if (dramount > balance) {
            data.Amount = data.Balance;
            ShowResult("Invoice Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        if (data.CompanyCurrencyRate < $scope.voucher.CompanyCurrencyRate) {
            data.ExchangeAmount = Math.abs(data.Amount * ($scope.voucher.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
            data.ExchangeType = "ExchangeGain";
        }
        else if (data.CompanyCurrencyRate > $scope.voucher.CompanyCurrencyRate) {
            data.ExchangeAmount = Math.abs(data.Amount * (data.CompanyCurrencyRate - $scope.voucher.CompanyCurrencyRate)).toFixed(2);
            data.ExchangeType = "ExchangeLoss";
        }
        else {
            data.ExchangeAmount = 0;
            data.ExchangeType = null;
        }
    };

    $scope.exchangeGainLossCal = function (rate) {
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if ($scope.voucherDetailList[i].CompanyCurrencyRate < rate) {
                $scope.voucherDetailList[i].ExchangeAmount = Math.abs($scope.voucherDetailList[i].Amount * (rate - $scope.voucherDetailList[i].CompanyCurrencyRate)).toFixed(2);
                $scope.voucherDetailList[i].ExchangeType = "ExchangeGain";
            }
            else if ($scope.voucherDetailList[i].CompanyCurrencyRate > rate) {
                $scope.voucherDetailList[i].ExchangeAmount = Math.abs($scope.voucherDetailList[i].Amount * ($scope.voucherDetailList[i].CompanyCurrencyRate - rate)).toFixed(2);
                $scope.voucherDetailList[i].ExchangeType = "ExchangeLoss";
            }
            else {
                $scope.voucherDetailList[i].ExchangeAmount = 0;
                $scope.voucherDetailList[i].ExchangeType = null;
            }
        }
    };

    $scope.removeRow = function (index, data) {
        $scope.deletecurrency = data.CurrencyId;
        $scope.voucherDetailList.splice(index, 1);
    };

    $scope.changeSourceFrom = function (from) {
        $scope.voucher.CrGLId = null;
        $scope.voucher.CrGLName = null;
        $scope.voucher.CrBudgetMasterId = null;
        $scope.voucher.CrActivityId = null;
        $scope.voucher.BankName = null;
        $scope.voucher.CashName = null;
        $scope.voucher.SourceFrom = from;
        $scope.voucher.BankMasterId = null;
        $scope.voucher.CashMasterId = null;
        $scope.isBankAmount = false;
        $scope.voucher.BankCurrencyId = null;
    };

    $scope.closeCashPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please Select Currency!", "failure", "cashPopUp");
            return;
        }
        if ($scope.cashIndex !== -1) {
            var cash = $scope.cashList[$scope.cashIndex];
            if (baseService.isUndefinedOrNull(cash.GLGeneralInfoId)) {
                ShowResult("Cash GL not found!", "failure", "bankPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(cash.BudgetMasterId)) {
                ShowResult("Cash Budget not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(cash.CurrencyId)) {
                ShowResult("Cash Transaction Currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.voucher.CashMasterId = cash.Id;
                $scope.voucher.CashCurrencyId = cash.CurrencyId;
                $scope.voucher.CashName = cash.CashName;
                $scope.voucher.GLGeneralInfoId = cash.GLGeneralInfoId;
                $scope.voucher.GLGeneralInfoName = cash.GLItem;
                $scope.voucher.BudgetName = cash.BudgetName;
                $scope.voucher.BudgetMasterId = cash.BudgetMasterId;
                $scope.voucher.ActivityId = cash.ActivityId;
                $scope.voucher.ActivityName = cash.ActivityName;
                $scope.checkCashAmount();
            }
        }
        $scope.hideCashPopUp();
    };

    $scope.closeBankPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please Select Currency !", "failure", "bankPopUp");
            return;
        }
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
                ShowResult("Bank GL not found!", "failure", "bankPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(bank.BudgetMasterId)) {
                ShowResult("Bank Budget not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(bank.CurrencyId)) {
                ShowResult("Bank Transaction Currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.voucher.AccountTitle = bank.AccountTitle;
                $scope.voucher.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
                $scope.voucher.BankMasterId = bank.BankMasterId;
                $scope.voucher.BankCurrencyId = bank.CurrencyId;

                $scope.voucher.GLGeneralInfoId = bank.GLGeneralInfoId;
                $scope.voucher.GLGeneralInfoName = bank.GLGeneralInfoName;
                $scope.voucher.BudgetMasterId = bank.BudgetMasterId;
                $scope.voucher.BudgetName = bank.BudgetName;
                $scope.voucher.ActivityId = bank.ActivityId;
                $scope.voucher.ActivityName = bank.ActivityName;
                $scope.checkBankAmount();
            }
        }
        $scope.hideBankPopUp();
    };

    $scope.checkBankAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.BankCurrencyId)) {
            if ($scope.voucher.BankCurrencyId !== $scope.companyCurrencyId) {
                $scope.isBankAmount = true;
                $scope.voucher.BankAmount = 0;
            }
            else {
                $scope.isBankAmount = false;
                $scope.voucher.BankAmount = 0;
            }
        }
    };

    $scope.checkCashAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.CashCurrencyId)) {
            if ($scope.voucher.CashCurrencyId !== $scope.companyCurrencyId) {
                $scope.isBankAmount = true;
                $scope.voucher.BankAmount = 0;
            }
            else {
                $scope.isBankAmount = false;
                $scope.voucher.BankAmount = 0;
            }
        }
    };

    $scope.customerAdvanceSearchList = [];
    $scope.customerAdvanceDataList = [];
    $scope.customerAdvanceSearch = [];
    $scope.customerAdvanceUrl = "accounts/Advance/GetVendorAvilabeAdvanceList";
    $scope.customerAdvanceSelectedIndex = -1;
    $scope.customerAdvanceParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "VoucherNo",
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.VendorAdvancePopUp = function (partyId) {
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.customerAdvanceDataList = [];
        }
        else {
            $scope.compareCurrencyId = $scope.voucher.CurrencyId;
            $scope.customerAdvanceParameters.partyId = partyId;
            $scope.getVendorAdvanceData = function (pageno) {
                baseService.paginationBase($scope.customerAdvanceUrl, pageno, $scope.customerAdvanceParameters)
                    .then(function (response) {
                        $scope.vendorAdvanceDataList = response.Rows;
                        $scope.customerAdvanceParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.customerAdvanceSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.customerAdvanceDataList, $scope.customerAdvanceSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector("#VendorAdvancePopUp")).modal("show");
            $scope.getVendorAdvanceData();
        }
    };

    $scope.closeVendorAdvancePopUpSelected = function (index, data) {
        angular.element(document.querySelector("#VendorAdvancePopUp")).modal("hide");
    };

    $scope.closeVendorAdvancePopUp = function () {
        angular.element(document.querySelector("#VendorAdvancePopUp")).modal("hide");
    };

    function clearVoucherDetail() {
        $scope.voucherDetail = {};
    }

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher = {};
        $scope.voucher.CurrencyId = null;
        $scope.voucher.VoucherTypeId = null;
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.RoundingType = $scope.roundingTypeList[0].Value;
        $scope.voucher.PaymentSource = "Bank";
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.getCboVoucherTypePaymentList();
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
        $scope.voucherInvoiceDetailList = [];
        $scope.bankChargesList = [];
        $scope.advanceTaxesList = [];
        $scope.advanceTax = {};
        $scope.bankCharge = {};
        $scope.TotalAdvanceAmount = 0;
        $scope.partyType = "Customer";
    };

    $scope.clearBankCashTaxPopUp = function () {
        $scope.isBankAmount = false;
        $scope.voucher.AccountTitle = null;
        $scope.voucher.BankName = null;
        $scope.voucher.BankMasterId = null;
        $scope.voucher.BankCurrencyId = null;
        $scope.voucher.CashMasterId = null;
        $scope.voucher.CashName = null;
        $scope.voucher.CashCurrencyId = null;
        $scope.voucher.GLGeneralInfoId = null;
        $scope.voucher.GLGeneralInfoCode = null;
        $scope.voucher.GLGeneralInfoName = null;
        $scope.voucher.BudgetMasterId = null;
        $scope.voucher.BudgetCode = null;
        $scope.voucher.BudgetName = null;
        $scope.voucher.ActivityId = null;
        $scope.voucher.ActivityCode = null;
        $scope.voucher.ActivityName = null;
    };

    $scope.clearCashPopUp = function () {
        $scope.clearBankCashTaxPopUp();
        $scope.advanceTaxesList = [];
        $scope.advanceTax = {};
    };

    $scope.clearBankPopUp = function () {
        $scope.clearBankCashTaxPopUp();
        $scope.advanceTaxesList = [];
        $scope.advanceTax = {};
    };

    $scope.clearTaxPopUp = function () {
        $scope.clearBankCashTaxPopUp();
        $scope.bankChargesList = [];
        $scope.bankCharge = {};
    };

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.voucher.CompanyCurrencyRate)) {
            ShowResult("Please input Rate!", "failure");
            return true;
        }
      
        if ($scope.partyType === "Customer") {
            if (baseService.isUndefinedOrNull($scope.voucher.PartyId)) {
                ShowResult("Please select Customer!", "failure");
                return true;
            }
            var vdetailCr = $filter("filter")($scope.voucherDetailList, { TrnType: "Dr" });
            if (vdetailCr.length === 0) {
                ShowResult("Please Select Customer Receivable !", "failure");
                return true;
            }
            //if (baseService.isUndefinedOrNull($scope.voucher.GLGeneralInfoId)) {
            //    ShowResult("Please select Cash or Bank!", "failure");
            //    return true;
            //}
        }
        else if ($scope.partyType === "Vendor") {
            if (baseService.isUndefinedOrNull($scope.voucher.PartyId)) {
                ShowResult("Please select Vendor!", "failure");
                return true;
            }
            var vdetailDr = $filter("filter")($scope.voucherDetailList, { TrnType: "Dr" });
            if (vdetailDr.length === 0) {
                ShowResult("Please select Payable!", "failure");
                return true;
            }
        }

        if ($scope.CurrencyParallel.length === 2) {
            if ($scope.voucher.BankAmount !== $scope.voucher.InvoiceGroupAmount) {
                ShowResult("Bank Amount and Group Currency Amount are not equal!", "failure");
                return true;
            }
        }
        return false;
    };

    $scope.passBankCashAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.CashCurrencyId)) {
            if ($scope.voucher.CashCurrencyId === $scope.companyCurrencyId) {
                $scope.voucher.BankAmount = $scope.voucher.Amount;
            }
        }
        if (!baseService.isUndefinedOrNull($scope.voucher.BankCurrencyId)) {
            if ($scope.voucher.BankCurrencyId === $scope.companyCurrencyId) {
                $scope.voucher.BankAmount = $scope.voucher.Amount;
            }
        }
    };

   
    $scope.confirmSave = function () {
        if ($scope.Action === "Save" && $scope.TotalAdvanceAmount > 0) {
            $scope.message_confirmation = "This Vendor have advance. Are you sure to Save?";
            angular.element(document.querySelector("#confirmSavePopUp")).modal("show");
        } else {
            $scope.Save();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        $scope.checkPostingDateWithInvoice();
        $scope.passBankCashAmount();
            $scope.entityValidation();
        if ($scope.form1.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            if ($scope.voucher.PaymentSource == "AdvanceToVendor" || $scope.voucher.PaymentSource == "AdvanceToCustomer") {
                $scope.saveUrl = "accounts/CommonAccounts/InsertDebitNoteAdvanceSetOff";
                $scope.voucher.SettlementType = $scope.voucher.PaymentSource;
            }
            else
                $scope.saveUrl = "accounts/AdjustmentNote/InsertDebitNoteSetOff";

            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.voucherDetailList,
                        "voucherDetailInvoiceList": $scope.voucherInvoiceDetailList,
                        "bankChargeDetailVMList": $scope.bankChargesList
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
                    url: "accounts/Invoice/UpdateCustomerAdvance",
                    data: {
                        "voucherVM": $scope.voucher,
                        "bankChargeDetailVMList": $scope.voucherDetailCurrencyList
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
        location.href = "accounts/invoice/VendorInvoicePaymentReport?voucherId=" + voucherId;
    };

    cboService.getEnumCbo("enum/GetCboPaymentType", function (result) {
        $scope.paymentTypeList = result;
    });

 
    $scope.invoiceWriteOffId = null;
    $scope.confirmPost = function (invoiceWriteOffId) {
        $scope.invoiceWriteOffId = invoiceWriteOffId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function (invoiceWriteOffId) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "invoiceWriteOffId": invoiceWriteOffId
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
    $scope.delete = function (invoiceWriteOffId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "invoiceWriteOffId": invoiceWriteOffId, "voucherId": voucherId
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
    $scope.invoiceWriteOffId = null;
    $scope.confirmDelete = function (invoiceWriteOffId, voucherId) {
        $scope.invoiceWriteOffId = invoiceWriteOffId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };
    $scope.getPartyType = function (party) {
        $scope.voucher.PartyId = null;
        $scope.voucher.PartyPlantId = null;
        $scope.voucher.PartyCode = null;
        $scope.voucher.PartyName = null;
        $scope.voucher.CurrencyId = null;
        if (party === "CustomerDebitNote")
            $scope.partyType = "Vendor";
        if (party === "VendorDebitNote")
            $scope.partyType = "Customer";
        $scope.changeSearchByParty();
    };
    $scope.changeSearchByParty = function () {
        $scope.searchByParty = 'UserName'; $scope.searchParty = "";
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

    }

    $scope.clearInvoicePopUp = function () {
        $scope.voucher.CompanyCurrencyRate = $scope.voucherDetailList[0].CompanyCurrencyRate;
        $scope.exchangeGainLossCal($scope.voucher.CompanyCurrencyRate);
    };

    //*********************** Customer Invoice PopUp Start *************************************
    $scope.customerInvoiceSearchList = [
        {
            "Text": "VoucherNo",
            "Value": "VoucherNo"
        },
        {
            "Text": "RefNo",
            "Value": "DocRefNo"
        },
        {
            "Text": "PINo",
            "Value": "SalesOrderNo"
        },
        {
            "Text": "Location",
            "Value": "PartyPlantName"
        },
        {
            "Text": "PostingDate",
            "Value": "PostingDate"
        },
        {
            "Text": "DocDate",
            "Value": "DocDate"
        },
        {
            "Text": "Currency",
            "Value": "CurrencyCode"
        }
    ];
    $scope.customerreceivableList = [];
    $scope.customerInvoiceSearch = [];
    $scope.customerInvoiceSelectedIndex = -1;
    $scope.customerInvoiceParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "VoucherNo",
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showVendorInvoicePopUp = function (partyId) {
        $scope.customerreceivableList = [];
        $scope.customerInvoiceSearch = [];
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.customerreceivableList = [];
            ShowResult("Please select Vendor.", "failure");
            return;
        }
        else {
            $scope.compareCurrencyId = $scope.voucher.CurrencyId;
            $scope.customerInvoiceParameters.partyId = partyId;
            $scope.customerreceivableGLData = function (pageno) {
                baseService.paginationBase("accounts/Invoice/GetVendorAvailableInvoiceList", pageno, $scope.customerInvoiceParameters)
                    .then(function (response) {
                        $scope.customerreceivableList = response.Rows;
                        $scope.customerInvoiceParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.customerInvoiceSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.customerreceivableList, $scope.customerInvoiceSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector("#customerInvoicePopUp")).modal("show");
            $scope.customerreceivableGLData();
        }

    };
   
    $scope.closePopUpselected = function () {
        angular.forEach($scope.customerreceivableList, function (data, i) {
            if (data.Active === true) {
                data.TrnType = "Cr";
                data.PartyPlantName = data.PartyPlantName;
                var getRow = $filter("filter")($scope.voucherInvoiceDetailList, { "TrnType": "Cr", "DocRefNo": data.DocRefNo });
                if (getRow.length === 0) {
                    data.Receivable = data.Receivable;
                    data.WriteOff = data.Received;
                    data.Advilable = data.Balance;
                    data.Amount = data.Balance;
                    data.CompanyCurrencyRate = data.CompanyCurrencyRate;
                    $scope.voucherInvoiceDetailList.push(data);
                    $scope.exchangeGainLossAmountInvoice(data);
                    $scope.voucher.InvoiceVoucherNo = data.VoucherNo;
                    angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
                }
                else {
                    ShowResult(data.DocRefNo + " already  Exist", "failure", "customerInvoicePopUp");
                }
            }
        });
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
    };

    $scope.exchangeGainLossAmountInvoice = function (data) {
        var balance = parseFloat(data.Advilable), dramount = parseFloat(data.Amount);
        if (dramount > balance) {
            data.Amount = data.Balance;
            ShowResult("Payment Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        $scope.TotalCreditNoteAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount"));
        $scope.TotalInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.voucherInvoiceDetailList), "Amount"));
        if ($scope.TotalInvoiceAmount > $scope.TotalCreditNoteAmount) {
            data.Amount = 0;
            ShowResult("Invoice Amount should not exceed Debit Note Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        if (data.CompanyCurrencyRate < $scope.voucher.CompanyCurrencyRate) {
            data.ExchangeAmount = Math.abs(data.Amount * (data.CompanyCurrencyRate - $scope.voucher.CompanyCurrencyRate)).toFixed(2);
            data.ExchangeType = "ExchangeLoss";
        }
        else if (data.CompanyCurrencyRate > $scope.voucher.CompanyCurrencyRate) {
            data.ExchangeAmount = Math.abs(data.Amount * ($scope.voucher.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
            data.ExchangeType = "ExchangeGain";
        }
        else {
            data.ExchangeAmount = 0;
            data.ExchangeType = null;
        }
    };
    //*********************** Customer Invoice PopUp End ***************************************
    $scope.removeInvoiceRow = function (index, data) {
        $scope.voucherInvoiceDetailList.splice(index, 1);
    };

    $scope.advancList = [];
    $scope.showAdvancePopUpNew = function (partyId, partyType) {
        $scope.advanceUrl = 'Accounts/Advance/GetAvailableAdvanceByVendor?vendorId=' + $scope.voucher.PartyId + '&partyType=' + $scope.voucher.NoteType;
        $http({
            method: 'POST',
            url: $scope.advanceUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.advancList = response.data;
        });
        //}
        angular.element(document.querySelector('#advancePopUp')).modal('show');
    };
    $scope.CloseAdvancePopUp = function () {
        angular.element(document.querySelector('#advancePopUp')).modal('hide');

    }
    $scope.selectAndCloseadvancePopUp = function (x) {
        $scope.voucherInvoiceDetailList = [];
        var advance = x.data;
        $scope.voucherInvoiceDetailList.push(advance);
        angular.element(document.querySelector("#advancePopUp")).modal("hide");
    }


    $scope.bankCharge = {
        FinancingTypeId: null,
        FinancingTypeName: null,
        Amount: null,
        CompanyCurrencyAmount: null
    };

    $scope.bankChargesList = [];
    $scope.addCharge = function () {
        if (manualValidation("td_FinancingType", baseService.isUndefinedOrNull($scope.bankCharge.FinancingTypeId), "Charges Type is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_FinancingTypeAmount", baseService.isUndefinedOrNull($scope.bankCharge.Amount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_FinancingTypeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.bankCharge.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.bankCharge.FinancingTypeName = $.grep($scope.bankChargeTypeList, function (item) {
                return item.FinancingTypeId === $scope.bankCharge.FinancingTypeId;
            })[0].ExpensesUserName;
            $scope.bankChargesList.push($scope.bankCharge);
            $scope.bankCharge = {};
            /*$scope.calBaseAmount();*/
        }
    };

    $scope.copyChargesAmount = function () {
        if ($scope.voucher.CurrencyId === $scope.companyCurrencyId) {
            $scope.bankCharge.CompanyCurrencyAmount = $scope.bankCharge.Amount;
        }
        else {
            $scope.bankCharge.CompanyCurrencyAmount = ($scope.bankCharge.Amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
        }
    };

    bankService.getCboBankChargeTypeList(function (result) {
        $scope.bankChargeTypeList = result;
    });

    $scope.removeChargesRow = function (index) {
        $scope.bankChargesList.splice(index, 1);
        $scope.calBaseAmount();
    };
}