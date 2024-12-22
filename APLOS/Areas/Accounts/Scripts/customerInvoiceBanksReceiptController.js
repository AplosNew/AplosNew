"use strict";
customerInvoiceBanksReceiptController.$inject = ["bankService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function customerInvoiceBanksReceiptController(bankService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Banks Receipt";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.bankDetailList = [];
    $scope.voucherDetailList = [];
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
    baseService.init("Accounts/Invoice/GetCustomerInvoiceBanksQueryList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
    $scope.url = "Accounts/Invoice";
    $scope.postUrl = $scope.url + "/PostCustomerBanksReceipt";
    $scope.deleteUrl = $scope.url + "/DeleteCustomerBanksReceipt";

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
        DiscountAmount:null
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
            console.log($scope.CurrencyParallel);
            if ($scope.CurrencyParallel.length === 0) {
                $scope.pop("error", "Company Parallel Currency is not set!");
                $scope.showform = false;
            }
            else {
                $scope.showform = true;
            }
            $scope.BaseCurrencyCode = $scope.CurrencyParallel[0].Code;
            $scope.BaseCurrencyId = $scope.CurrencyParallel[0].CurrencyId;
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
            "Text": "Doc Ref No",
            "Value": "DocRefNo"
        },
        {
            "Text": "Currency Code",
            "Value": "CurrencyCode"
        },
        {
            "Text": "Amount",
            "Value": "Amount"
        }
        ,
        {
            "Text": "Status",
            "Value": "IsPark"
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
        baseService.pagination(pageno, $scope.parameters)
            .then(function (result) {
                $scope.paymentList = result.Rows;
                $scope.parameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
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
                $scope.exchangeGainLossCal($scope.voucher.CompanyCurrencyRate);
                $scope.rateChangeBankCharge($scope.voucher.CompanyCurrencyRate);
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
                    //console.log($scope.taxCodCboList);
                    //if ($scope.taxCodCboList.length === 0) {
                    //    $scope.pop("error", "No TaxCode found in this Fiscal Year ");
                    //}
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
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if (new Date($scope.voucherDetailList[i].PostingDate) > new Date($scope.voucher.PostingDate)) {
                msg = "Posting date must be below or equal to payable of " + $scope.voucherDetailList[i].VoucherNo;
                $scope.invalidPostingDate = true;
                break;
            }
            else {
                $scope.invalidPostingDate = false;
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

    $scope.getCboVoucherTypeBanksPaymentList = function () {
        cboService.getCboVoucherTypeBanksReceiptList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);

            }
        });
    };
    $scope.getCboVoucherTypeBanksPaymentList();

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

    $scope.customerInvoiceSearchList = [
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

    $scope.getPopupCustomerReceivableList = function () {
        $scope.getInvoiceData = function (pageno) {
            $scope.customerReceivableGLUrl1 = "accounts/CustomerInvoice/GetCustomerAvailableInvoiceList?partyId=" + $scope.voucher.PartyId;
            baseService.paginationBase($scope.customerReceivableGLUrl1, pageno, $scope.customerInvoiceParameters)
                .then(function (result) {
                    try {
                        $scope.customerreceivableList = result.Rows;
                        $scope.customerInvoiceParameters.total_count = result.Total;
                    } catch (e) {
                        ShowResult(e, "Error");
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#customerInvoicePopUp")).modal("show");
        $scope.getInvoiceData();
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
    };

    //$scope.closePopUpselected = function (data) {
    //    $scope.voucherDetailList = [];
    //            data.TrnType = "Dr";
    //    data.PartyPlantName = data.PartyPlantName;
    //    $scope.voucherDetailList.push(data);
    //    angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");

    //};

    $scope.closePopUpselected = function () {
        angular.forEach($scope.customerreceivableList, function (data, i) {
            if (data.Active === true) {
                data.TrnType = "Dr";
                data.PartyPlantName = data.PartyPlantName;
                var getRow = null;
                getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "DocRefNo": data.DocRefNo, "InvoiceDetailId": data.InvoiceDetailId });
                if (getRow.length === 0) {
                    data.Amount = data.Balance;
                    $scope.voucherDetailList.push(data);

                    if ($scope.voucherDetailList.length > 0)
                        $scope.isReadOnly = true;
                    else
                        $scope.isReadOnly = false;

                    angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
                }
                else {
                    ShowResult(data.DocRefNo + " already  Exist", "failure", "customerInvoicePopUp");
                }
            }
        });
        $scope.exchangeGainLossCal($scope.voucher.CompanyCurrencyRate);
        $scope.calBaseAmount();
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
            data.BaseCrAmount = Math.abs(data.Amount * data.CompanyCurrencyRate).toFixed(2);
        }
        else if (data.CompanyCurrencyRate > $scope.voucher.CompanyCurrencyRate) {
            data.ExchangeAmount = Math.abs(data.Amount * (data.CompanyCurrencyRate - $scope.voucher.CompanyCurrencyRate)).toFixed(2);
            data.ExchangeType = "ExchangeLoss";
            data.BaseCrAmount = Math.abs(data.Amount * data.CompanyCurrencyRate).toFixed(2);

        }
        else {
            data.ExchangeAmount = 0;
            data.ExchangeType = null;
            data.BaseCrAmount = Math.abs(data.Amount * data.CompanyCurrencyRate).toFixed(2);
        }
        $scope.calBaseAmount();
    };

    $scope.exchangeGainLossCal = function (rate) {
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if ($scope.voucherDetailList[i].CompanyCurrencyRate < rate) {
                $scope.voucherDetailList[i].ExchangeAmount = Math.abs($scope.voucherDetailList[i].Amount * (rate - $scope.voucherDetailList[i].CompanyCurrencyRate)).toFixed(2);
                $scope.voucherDetailList[i].ExchangeType = "ExchangeGain";
                $scope.voucherDetailList[i].BaseCrAmount = Math.abs($scope.voucherDetailList[i].Amount * $scope.voucherDetailList[i].CompanyCurrencyRate).toFixed(2);

            }
            else if ($scope.voucherDetailList[i].CompanyCurrencyRate > rate) {
                $scope.voucherDetailList[i].ExchangeAmount = Math.abs($scope.voucherDetailList[i].Amount * ($scope.voucherDetailList[i].CompanyCurrencyRate - rate)).toFixed(2);
                $scope.voucherDetailList[i].ExchangeType = "ExchangeLoss";
                $scope.voucherDetailList[i].BaseCrAmount = Math.abs($scope.voucherDetailList[i].Amount * $scope.voucherDetailList[i].CompanyCurrencyRate).toFixed(2);
            }
            else {
                $scope.voucherDetailList[i].ExchangeAmount = 0;
                $scope.voucherDetailList[i].ExchangeType = null;
            }
        }
    };

    $scope.removeRowInvoice = function (index, data) {
        //$scope.deletecurrency = data.CurrencyId;
        $scope.voucherDetailList.splice(index, 1);
    };
    $scope.removeBankRow = function (index, data) {
        $scope.bankDetailList.splice(index, 1);
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

    $scope.calbankAmount = function (data) {
        if ($scope.voucher.CurrencyId == data.BankCurrencyId == $scope.BaseCurrencyId) {
            data.BankAmount = data.Amount;
            data.BaseDrAmount = data.Amount;
        }
        else if ($scope.voucher.CurrencyId == data.BankCurrencyId) {
            data.BankAmount = data.Amount;
            data.BaseDrAmount = Math.abs(data.Amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
        }
        else if ($scope.voucher.CurrencyId != data.BankCurrencyId && data.BankCurrencyId == $scope.BaseCurrencyId) {
            data.BankAmount = Math.abs(data.Amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
            data.BaseDrAmount = Math.abs(data.Amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
        }
        else
            data.BankAmount = '';
        $scope.calBaseAmount();
    }
    $scope.calbankCurrecyAmount = function (data) {
        if ($scope.voucher.CurrencyId == data.BankCurrencyId == $scope.BaseCurrencyId) {
            data.BaseDrAmount = data.BankAmount;
        }
        else if ($scope.voucher.CurrencyId == data.BankCurrencyId) {
            data.BankAmount = data.Amount;
            data.BaseDrAmount = Math.abs(data.BankAmount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
        }
        else if ($scope.voucher.CurrencyId != data.BankCurrencyId && data.BankCurrencyId == $scope.BaseCurrencyId) {
            data.BaseDrAmount = data.BankAmount;
        }
        else
            data.BankAmount = '';
        $scope.calBaseAmount();
    }
    $scope.changecalbankAmount = function () {
        for (var i = 0; i < $scope.bankDetailList.length; i++) {
            if ($scope.voucher.CurrencyId == $scope.bankDetailList[i].BankCurrencyId == $scope.BaseCurrencyId) {
                $scope.bankDetailList[i].BankAmount = $scope.bankDetailList[i].Amount;
                $scope.bankDetailList[i].BaseDrAmount = Math.abs($scope.bankDetailList[i].Amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
            }
            else if ($scope.voucher.CurrencyId == $scope.bankDetailList[i].BankCurrencyId) {
                $scope.bankDetailList[i].BankAmount = $scope.bankDetailList[i].Amount;
                $scope.bankDetailList[i].BaseDrAmount = Math.abs($scope.bankDetailList[i].Amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);

            }
            else if ($scope.voucher.CurrencyId != $scope.bankDetailList[i].BankCurrencyId && $scope.bankDetailList[i].BankCurrencyId == $scope.BaseCurrencyId) {
                $scope.bankDetailList[i].BankAmount = Math.abs($scope.bankDetailList[i].Amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
                $scope.bankDetailList[i].BaseDrAmount = Math.abs($scope.bankDetailList[i].Amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
            }
            else
                $scope.bankDetailList[i].BankAmount = '';
        }
    }
    $scope.changecalbankChargeAmount = function () {
        for (var i = 0; i < $scope.bankChargesList.length; i++) {
            if ($scope.voucher.CurrencyId == $scope.BaseCurrencyId) {
                $scope.bankChargesList[i].CompanyCurrencyAmount = $scope.bankChargesList[i].Amount;
            }
            else
                $scope.bankChargesList[i].CompanyCurrencyAmount = Math.abs($scope.bankChargesList[i].Amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
        }
    }
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
                var getRow = null;
                getRow = $filter("filter")($scope.bankDetailList, { "BankMasterId": bank.BankMasterId });
                if (getRow.length === 0) {
                    $scope.bankDetail = {};
                    $scope.bankDetail.SourceType = "Bank";
                    $scope.bankDetail.AccountTitle = bank.AccountTitle;
                    $scope.bankDetail.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
                    $scope.bankDetail.BankMasterId = bank.BankMasterId;
                    $scope.bankDetail.BankCurrencyId = bank.CurrencyId;

                    $scope.bankDetail.GLGeneralInfoId = bank.GLGeneralInfoId;
                    $scope.bankDetail.GLGeneralInfoName = bank.GLGeneralInfoName;
                    $scope.bankDetail.BudgetMasterId = bank.BudgetMasterId;
                    $scope.bankDetail.BudgetName = bank.BudgetName;
                    $scope.bankDetail.ActivityId = bank.ActivityId;
                    $scope.bankDetail.ActivityName = bank.ActivityName;
                    $scope.bankDetail.BankCurrencyId = bank.CurrencyId;
                    $scope.bankDetail.CurrencyCode=$scope.voucher.CurrencyCode;
                    $scope.bankDetail.BankCurrencyCode = bank.CurrencyCode;
                    $scope.bankDetail.FinancingId = "";
                    $scope.bankDetail.FinancingDetailId = "";
                    $scope.bankDetail.FinancingTypeId = "";
                    $scope.bankDetail.Balance = 0;
                    $scope.bankDetail.Amount = null;
                    $scope.bankDetail.BaseDrAmount = null;
                    $scope.bankDetailList.push($scope.bankDetail);
                    $scope.checkBankAmount();
                    $scope.hideBankPopUp();
                }
                    else {
                    ShowResult(bank.AccountTitle + " already  Exist", "failure", "bankPopUp");
                    }
            }
        }
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
        $scope.voucher.CompanyCurrencyRate = 1;
        $scope.voucher.VoucherTypeId = null;
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.RoundingType = $scope.roundingTypeList[0].Value;
        $scope.voucher.PaymentSource = "Bank";
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.getCboVoucherTypeBanksPaymentList();
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
        $scope.bankDetailList = [];
        $scope.bankChargesList = [];
        $scope.BaseAmountList = [];
        $scope.advanceTaxesList = [];
        $scope.advanceTax = {};
        $scope.bankCharge = {};
        $scope.TotalAdvanceAmount = 0;
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
        if (!baseService.isUndefinedOrNull($scope.bankCharge.FinancingTypeId)) {
            ShowResult("Please add Charges!", "failure");
            return true;
        }
        if (!baseService.isUndefinedOrNull($scope.advanceTax.TaxCodeId)) {
            ShowResult("Please add Taxes!", "failure");
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
        }
        var baseDr = $filter("sumByKey")($filter("filter")($scope.BaseAmountList), "BaseDrAmount");
        var baseCr = $filter("sumByKey")($filter("filter")($scope.BaseAmountList), "BaseCrAmount");
        if (baseDr != baseCr) {
            ShowResult("JV Dr Cr is not equal!", "failure");
            return true;
        }
        for (var i = 0; i < $scope.bankDetailList.length; i++) {
            if ($scope.bankDetailList[i].SourceType === "Loan") {
                if ($scope.bankDetailList[i].Balance < $scope.bankDetailList[i].Amount && $scope.bankDetailList[i].BankCurrencyId != $scope.companyCurrencyId) {
                    ShowResult("Payment Amount can't more than Loan Balance Amount", "failure");;
                    return true;
                }
                if ($scope.bankDetailList[i].Balance < $scope.bankDetailList[i].BaseDrAmount && $scope.bankDetailList[i].BankCurrencyId === $scope.companyCurrencyId) {
                    ShowResult("Payment Amount can't more than Loan Balance Amount", "failure");;
                    return true;
                }
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

    $scope.rateChangeBankCharge = function (rate) {
        $scope.bankCharge.CompanyCurrencyAmount = $scope.bankCharge.Amount * rate;
        if ($scope.bankChargesList.length !== null) {
            for (var i = 0; i < $scope.bankChargesList.length; i++) {
                $scope.bankChargesList[i].CompanyCurrencyAmount = $scope.bankChargesList[i].Amount * rate;
            }
        }
    };

    $scope.rateChangeTax = function (rate) {
        $scope.advanceTax.CompanyCurrencyAmount = $scope.advanceTax.Amount * rate;
        if ($scope.advanceChargesList.length !== null) {
            for (var i = 0; i < $scope.advanceChargesList.length; i++) {
                $scope.advanceChargesList[i].CompanyCurrencyAmount = $scope.advanceChargesList[i].Amount * rate;
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
        $scope.passBankCashAmount();
            $scope.entityValidation();
        if ($scope.form1.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "accounts/Invoice/InsertCustomerInvoiceBanksReceipt",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.voucherDetailList,
                        "banksDetailVMList": $scope.bankDetailList,
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
            $scope.calBaseAmount();
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

    $scope.advanceTax = {
        TaxCodeId: null,
        Text: null,
        TaxAmount: null,
        CompanyCurrencyAmount: null
    };

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
            $scope.advanceTax.TaxName = $.grep($scope.taxCodCboList, function (item) {
                return item.Value === $scope.advanceTax.TaxCodeId;
            })[0].Text;
            $scope.advanceTaxesList.push($scope.advanceTax);
            $scope.advanceTax = {};
        }
    };

    $scope.copyTaxesAmount = function () {
        if ($scope.advance.CurrencyId === $scope.companyCurrencyId) {
            $scope.advanceTax.CompanyCurrencyAmount = $scope.advanceTax.TaxAmount;
        }
        else {
            $scope.advanceTax.CompanyCurrencyAmount = ($scope.advanceTax.TaxAmount * $scope.advance.CompanyCurrencyRate).toFixed(2);
        }
    };

    $scope.removeTaxesRow = function (index) {
        $scope.advanceTaxesList.splice(index, 1);
    };

    $scope.InvoiceWriteOffGroupNo = null;
    $scope.confirmPost = function (InvoiceWriteOffGroupNo) {
        $scope.InvoiceWriteOffGroupNo = InvoiceWriteOffGroupNo;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function (InvoiceWriteOffGroupNo) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "invoiceWriteOffNo": InvoiceWriteOffGroupNo
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

    $scope.BaseAmountList = [];
    $scope.BaseAmountObj = {
        Type: null,
        BaseDrAmount: null,
        BaseCrAmount: null,
    };

    $scope.calBaseAmount = function () {
        $scope.BaseAmountList = [];
        $scope.calBankBaseAmount();
        $scope.calBankChargesBaseAmount();
        $scope.calReceivableBaseAmount();
        $scope.calExchangeGainBaseAmount();
        $scope.calExchangeLossBaseAmount();
    }
    $scope.calReceivableBaseAmount = function () {
        if ($scope.voucherDetailList.length) {
            $scope.BaseAmountObj.Type = 'A/R';
            $scope.BaseAmountObj.BaseCrAmount = $filter("sumByKey")($filter("filter")($scope.voucherDetailList), "BaseCrAmount");
            $scope.BaseAmountObj.BaseDrAmount = null;
            $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
        
    }
    $scope.calExchangeGainBaseAmount = function () {
        if ($scope.voucherDetailList.length) {
            $scope.BaseAmountObj.Type = 'Exchange Gain';
            $scope.BaseAmountObj.BaseCrAmount = $filter("sumByKey")($filter("filter")($scope.voucherDetailList, { ExchangeType: "ExchangeGain" }), "ExchangeAmount");
            $scope.BaseAmountObj.BaseDrAmount = null;
            $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
       
    }
    $scope.calExchangeLossBaseAmount = function () {
        if ($scope.voucherDetailList.length) {
            $scope.BaseAmountObj.Type = 'Exchange Loss';
            $scope.BaseAmountObj.BaseDrAmount = $filter("sumByKey")($filter("filter")($scope.voucherDetailList, { ExchangeType: "ExchangeLoss" }), "ExchangeAmount");
            $scope.BaseAmountObj.BaseCrAmount = null;
            $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
    }
    $scope.calBankBaseAmount = function () {
        if ($scope.bankDetailList.length) {
            $scope.BaseAmountObj.Type = 'Bank';
            $scope.BaseAmountObj.BaseDrAmount = $filter("sumByKey")($filter("filter")($scope.bankDetailList), "BaseDrAmount");
            $scope.BaseAmountObj.BaseCrAmount = null;
            $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
       
    }
    $scope.calBankChargesBaseAmount = function () {
        if ($scope.bankChargesList.length) {
            $scope.BaseAmountObj.Type = 'Bank Charges';
            $scope.BaseAmountObj.BaseDrAmount = $filter("sumByKey")($filter("filter")($scope.bankChargesList), "CompanyCurrencyAmount");
            $scope.BaseAmountObj.BaseCrAmount = null;
            $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
        
    }


   // $scope.deleteUrl = $scope.url + "/DeleteVendorAdvance";
    $scope.delete = function (invoiceWriteOffGroupNo) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "invoiceWriteOffGroupNo": invoiceWriteOffGroupNo  /*, "voucherId": voucherId*/
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
                $scope.invoiceWriteOffGroupNo = null;
               // $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };
   // $scope.customerReceivedId = null;
    $scope.confirmDelete = function (A) {
       // $scope.customerReceivedId = customerReceivedId;
        $scope.invoiceWriteOffGroupNo = A;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

    $scope.loanDataList = [];
    $scope.getPopUpData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/Loan/GetLoanPopUpListForSalesRealization?transactionType=' + "LoanTaken"
        }).then(function successCallback(response) {
            $scope.loanDataList = response.data;
            for (var i = 0; i < $scope.loanDataList.length; i++) {
                response.data[i].PostingDateNew = new Date($scope.loanDataList[i].PostingDateNew);
                response.data[i].DocDate = new Date($scope.loanDataList[i].DocDate);
            }
        });
    };
    $scope.showloanPopUp = function () {
        $scope.getPopUpData();
        angular.element(document.querySelector('#loanPopUp')).modal('show');
    };
    $scope.closeloanPopUp = function () {
        angular.element(document.querySelector("#loanPopUp")).modal("hide");
    };
    $scope.closeloanPopUpSelected = function (x) {
        var bank = x.data;
            if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
                ShowResult("Please Select Currency !", "failure", "loanPopUp");
                return;
            }
            if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
                ShowResult("Bank GL not found!", "failure", "loanPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(bank.BudgetMasterId)) {
                ShowResult("Bank Budget not found!", "failure", "loanPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(bank.CurrencyId)) {
                ShowResult("Bank Transaction Currency not found!", "failure", "loanPopUp");
                return;
            }
            
            else {
                var getRow = null;
                getRow = $filter("filter")($scope.bankDetailList, { "BankMasterId": bank.BankMasterId });
                if (getRow.length === 0) {
                    $scope.bankDetail = {};
                    $scope.bankDetail.SourceType = "Loan";
                    $scope.bankDetail.AccountTitle = bank.AccountTitle;
                    $scope.bankDetail.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
                    $scope.bankDetail.BankMasterId = bank.BankMasterId;
                    $scope.bankDetail.BankCurrencyId = bank.CurrencyId;

                    $scope.bankDetail.GLGeneralInfoId = bank.GLGeneralInfoId;
                    $scope.bankDetail.GLGeneralInfoName = "";
                    $scope.bankDetail.BudgetMasterId = bank.BudgetMasterId;
                    $scope.bankDetail.BudgetName = "";
                    $scope.bankDetail.ActivityId = bank.ActivityId;
                    $scope.bankDetail.ActivityName = "";
                    $scope.bankDetail.BankCurrencyId = bank.CurrencyId;
                    $scope.bankDetail.CurrencyCode = $scope.voucher.CurrencyCode;
                    $scope.bankDetail.BankCurrencyCode = bank.CurrencyCode;
                    $scope.bankDetail.FinancingId = bank.FinancingId;
                    $scope.bankDetail.FinancingDetailId = bank.FinancingDetailId;
                    $scope.bankDetail.FinancingTypeId = bank.FinancingTypeId;
                    $scope.bankDetail.DocRefNo = bank.DocRefNo;
                    $scope.bankDetail.CompanyCurrencyRate = bank.CompanyCurrencyRate;
                    $scope.bankDetail.Balance = bank.Balance;
                    $scope.bankDetail.Amount = null;
                    $scope.bankDetail.BaseDrAmount = null;
                    $scope.bankDetailList.push($scope.bankDetail);
                    $scope.checkBankAmount();
                   
                }
                else {
                    ShowResult(bank.AccountTitle + " already  Exist", "failure", "loanPopUp");
                }
            }
        
        //$scope.voucher.FinancingId = data.FinancingId;
        //$scope.voucher.FinancingDetailId = data.FinancingDetailId;
        //$scope.voucher.FinancingTypeId = data.FinancingTypeId;
        //$scope.voucher.VoucherNo = data.VoucherNo;
        //$scope.voucher.PartyName = data.Particulars;
        //$scope.voucher.PartyId = data.PartyId;
        //$scope.voucher.PartyType = data.PartyType;
        //$scope.voucher.PartyPlantName = data.PartyPlantName;
        //$scope.voucher.CurrencyId = data.CurrencyId;
        //$scope.voucher.CurrencyCode = data.CurrencyCode;
        //$scope.voucher.EntityId = data.EntityId;
        //$scope.voucher.CompanyId = data.CompanyId;
        //$scope.voucher.PlantId = data.PlantId;
        //$scope.voucher.LoanAmount = data.LoanAmount;
        //$scope.voucher.LoanSetOff = data.LoanPayment;
        //$scope.voucher.InitialSactionAmount = data.InitialSactionAmount;
        //$scope.voucher.AdditionalLoanAmount = data.AdditionalLoanAmount;
        //$scope.voucher.TotalInterestPayableAmount = data.InterestAmount;
        //$scope.voucher.InterestAmount = data.InterestAmount - data.OtherExpensesPayable;
        //$scope.voucher.OtherExpensesPayable = data.OtherExpensesPayable;
        //$scope.voucher.Balance = data.Balance;
        //$scope.voucher.LoanDocRefNo = data.DocRefNo;
        //$scope.voucher.LoanPostingDate = data.PostingDate;
        //$scope.voucher.LoanDocDate = data.DocDateNew;
        //$scope.voucher.InterestWriteOff = data.InterestWriteOff;
        //$scope.voucher.InterestBalance = data.InterestBalance;
        //$scope.voucher.InterestCashPayment = data.InterestCashPayment;

        //$scope.voucher.OtherBankMasterId = data.OtherBankMasterId;
        //$scope.voucher.ToCurrencyRate = data.CompanyCurrencyRate;
        //$scope.getPartyPlantList(data.PartyId);
        //$scope.voucher.PartyPlantId = data.PartyPlantId;
        //$scope.voucher.TotalAmount = '';
        //$scope.voucher.InterestPaymentAmount = '';
        //$scope.voucher.InterestCashAmount = '';
        //$scope.GetCurrencyExchangeRateList();
        angular.element(document.querySelector("#loanPopUp")).modal("hide");
    };

    //$scope.getvouchardetailjs = function (obj) {
    //    var reportformat = "pdf";
    //    var file_src = "";
    //    //if (baseservice.isundefinedornull(data.data.voucherid))
    //    //    return showresult('no id found', 'failure');
    //    //else {

    //    // file_src = 'accounts/voucherReport/GetCommonVoucherReport?reportformat=' + 'pdf' + '&compnaygroupid=' + obj.data.CompanyGroupId + '&companyid=' + obj.data.CompanyId + '&plantid=' + obj.data.PlantId + '&sourcetype=' + obj.data.SourceType + '&voucherid=' + obj.data.VoucherId;
    //    file_src = 'Accounts/VoucherReport/GetCommonVoucherReport?reportFormat=' + 'Pdf' + '&compnayGroupId=' + obj.data.CompanyGroupId + '&companyId=' + obj.data.CompanyId + '&plantId=' + obj.data.PlantId + '&sourceType=' + obj.data.SourceType + '&voucherId=' + obj.data.VoucherId + '&inventoryIssueId=' + obj.data.InventoryIssueId + '&inventoryReceiveId=' + obj.data.InventoryReceiveId + '&salesSourceType=' + obj.data.SalesSourceType + '&invoiceWriteOffGroupNo=' + obj.data.InvoiceWriteOffGroupNo;
    //    $window.open(file_src, '_blank');
    //    //}
    //};

    $scope.PrintData = function (data) {
        try {
           
            $scope.ReportFormat = 'Pdf';
            var url = 'Accounts/invoice/GetCustomerInvoiceReceiptBanksReportPdf?reportFormat=' + $scope.ReportFormat + '&invoiceWriteOffGroupNo=' + data.InvoiceWriteOffGroupNo;
            $rootScope.report(url);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


}