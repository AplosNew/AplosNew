"use strict";
vendorPaymentController.$inject = ["bankService", "accountService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller","$window"];
function vendorPaymentController(bankService, accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $window) {
    $rootScope.title = "Payment";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.taxCodCboList = [];
    $scope.currencyExchangeRate = [];
    $scope.partyFromTo = "From";
    $scope.bankFromTo = "To";
    $scope.isWriteOff = false;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.partyType = "Vendor";
    $scope.isAdvance = false;
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    baseService.init("Accounts/Invoice/GetVendorPaymentList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
    $scope.url = "Accounts/Invoice";
    $scope.postUrl = $scope.url + "/PostVendorPayment";
    $scope.deleteUrl = $scope.url + "/DeleteWriteOff";
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
        BankTransactionDate: null,
        BankReferenceNo: null,
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
        BankAmount: null,
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
        ExchangeAmount: null,
        ExchangeType: null,
        DiscountAmount: null,
        BaseCurrencyId: null
    };
    $scope.advance = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        EmployeeId: null,
        EmployeeName: null,
        PartyGLGeneralInfoId: null,
        GLGeneralInfoId: null,
        CurrencyId: null,
        VoucherTypeId: null,
        PartyType: 'Employee',
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        IsExcludingTax: false,
        VoucherDetailId: null,
        Amount: null,
        BaseOnDueDate: $filter("dateFiltering")(Date.now()),
        BaseNoOfDays: null,
        PaymentTermId: null,
        Narration: null,
        Remarks: null,
        BankName: null,
        BankAccountNumber: null,
        BankGL: null,
        BankGLGeneralInfoId: null,
        AdvanceAmount: null,
        SettlementType: 'SetOff',
        PaymentSource: 'Bank',
        CashMasterId: null,
        BankMasterId: null,
        JournalType: null
    };

    $scope.voucherDetail = {
        EntityId: null
    };
    $scope.changeExhangeType = function (type) {
        if (type === 'ExchangeGain') {
            $scope.voucher.ExchangeType = 'ExchangeGain';
        }
        if (type === 'ExchangeLoss') {
            $scope.voucher.ExchangeType = 'ExchangeLoss';
        }
        $scope.calBaseAmount();
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
            $scope.voucher.BaseCurrencyId = $scope.CurrencyParallel[0].CurrencyId;
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
            "Text": "Multiple Payment No",
            "Value": "MultiplePaymentNo"
        }
        ,
        {
            "Text": "Currency Code",
            "Value": "CurrencyCode"
        },
        {
            "Text": "Status",
            "Value": "Status"
        },
        {
            "Text": "Doc. RefNo",
            "Value": "DocRefNo"
        },
        {
            "Text": "Amount",
            "Value": "Amount"
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

    });
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
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
        else if ($scope.voucherDetailList.length > 0) {
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
        }
        else {
            $scope.invalidPostingDate = false;
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
            $scope.TotalAdvanceAmount = Math.round($filter("sumByKey")($filter("filter")($scope.partyWiseOutstandingAdvanceList), "Balance") * 1000 + Number.EPSILON) / 1000;
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
            $scope.getPartyWiseOutstandingAdvance($scope.voucher.PartyId);
            $scope.getPartyWiseOutstandingDebitNote($scope.voucher.PartyId);
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

    $scope.getPartyWiseOutstandingDebitNote = function (id) {
        $scope.partyWiseOutstandingDebitNoteList = [];
        $http({
            method: "GET",
            url: "accounts/Advance/GetPartyWiseOutstandingDebitNote?partyId=" + id
        }).then(function successCallback(response) {
            $scope.partyWiseOutstandingDebitNoteList = response.data;
            $scope.TotalDebitNoteAmount = Math.round($filter("sumByKey")($filter("filter")($scope.partyWiseOutstandingDebitNoteList), "Balance") * 1000 + Number.EPSILON) / 1000;

            if ($scope.partyWiseOutstandingDebitNoteList.length > 0) {
                angular.element(document.querySelector("#partyDebitNotePopUp")).modal("show");
            }

        });
    };

    $scope.showPartyDebitNoteAmount = function () {
        angular.element(document.querySelector("#partyDebitNotePopUp")).modal("show");
    };

    $scope.closePartyDebitNoteAmount = function () {
        angular.element(document.querySelector("#partyDebitNotePopUp")).modal("hide");
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
        cboService.getCboVoucherTypePaymentList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.BankTransactionDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                //$scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);

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
        //$scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);
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

    $scope.invoiceSearchList = [
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
        },
        {
            "Text": "LC No",
            "Value": "LCRef"
        },
        {
            "Text": "Contract No",
            "Value": "ContractNo"
        }
    ];

    $scope.invoiceParameters = {
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
            $scope.customerReceivableGLUrl1 = "accounts/Invoice/GetVendorAvailableInvoiceList?partyId=" + $scope.voucher.PartyId;
            baseService.paginationBase($scope.customerReceivableGLUrl1, pageno, $scope.invoiceParameters)
                .then(function (result) {
                    try {
                        $scope.invoiceList = result.Rows;
                        $scope.invoiceParameters.total_count = result.Total;
                    } catch (e) {
                        ShowResult(e, "Error");
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#vendorInvoicePopUp")).modal("show");
        $scope.getInvoiceData();
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector("#vendorInvoicePopUp")).modal("hide");
    };

    $scope.closeInvoicePopUpselected = function () {
        angular.forEach($scope.invoiceList, function (data, i) {
            if (data.Active === true) {
                data.TrnType = "Dr";
                data.PartyPlantName = data.PartyPlantName;
                var getRow = null;
                getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "DocRefNo": data.DocRefNo, "InvoiceDetailId": data.InvoiceDetailId });
                if (getRow.length === 0) {
                    data.Amount = data.Balance;
                    $scope.voucher.EntityId = data.EntityId;
                    if (data.CompanyCurrencyRate < $scope.voucher.CompanyCurrencyRate) {
                        data.ExchangeAmount = Math.round((data.Amount * ($scope.voucher.CompanyCurrencyRate - data.CompanyCurrencyRate)) * 1000 + Number.EPSILON) / 1000;
                        data.ExchangeType = "ExchangeLoss";
                        data.BaseDrAmount = Math.round((data.Amount * data.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;

                    }
                    else if (data.CompanyCurrencyRate > $scope.voucher.CompanyCurrencyRate) {
                        data.ExchangeAmount = Math.round((data.Amount * (data.CompanyCurrencyRate - $scope.voucher.CompanyCurrencyRate)) * 1000 + Number.EPSILON) / 1000;
                        data.ExchangeType = "ExchangeGain";
                        data.BaseDrAmount = Math.round((data.Amount * data.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;

                    }
                    else {
                        data.ExchangeAmount = 0;
                        data.BaseDrAmount = Math.round((data.Amount * data.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;
                        data.ExchangeType = null;
                    }
                    $scope.voucherDetailList.push(data);

                    if ($scope.voucherDetailList.length > 0)
                        $scope.isReadOnly = true;
                    else
                        $scope.isReadOnly = false;

                    angular.element(document.querySelector("#vendorInvoicePopUp")).modal("hide");
                }
                else {
                    ShowResult(data.DocRefNo + " already  Exist", "failure", "vendorInvoicePopUp");
                }
            }
        });
        $scope.calBaseAmount();
    };

    //#region File Upload
    $scope.ItemId = null;
    $scope.onBeginUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull(args.model.Data))
                throw 'Please select/save the order first'
            $scope.ItemId = args.model.Data;
            args.data = args.model.Data;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = "Accounts/Invoice/SaveDefault";
    $scope.fileselect = function (e) {

    }
    $scope.errorPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.ItemId))
            ShowResult('Please select/save the order first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.InvoiceDocument + '/' + data.InvoiceId + extention;
      //  $window.open($scope.dwonloadUrl, '_blank');
    };

    //#endregion


    $scope.BaseAmountList = [];
    $scope.BaseAmountObj = {
        Type: null,
        BaseDrAmount: null,
        BaseCrAmount: null,
    };
    $scope.calBaseAmount = function () {
        $scope.BaseAmountList = [];
        $scope.calBankChargesBaseAmount();
        $scope.calOtherChargesBaseAmount();
        $scope.caltaxBaseAmount();
        $scope.calExchangeLossBaseAmount();
        $scope.calPayableBaseAmount();
        $scope.calExchangeGainBaseAmount();
        $scope.calExchangeTypeGainLossBaseAmount();
        $scope.calBankAmount();
        $scope.calBankBaseAmount();
        $scope.calCashBaseAmount();
        $scope.calDiscountAmount();
        $scope.calCreditNoteVendorAmount();
        $scope.calGLBaseAmount();
        $scope.calAdvanceBaseAmount();
        
    }
    $scope.calPayableBaseAmount = function () {
        if ($scope.voucherDetailList.length) {
            $scope.BaseAmountObj.Type = 'A/P';
            $scope.BaseAmountObj.BaseDrAmount = Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "BaseDrAmount") * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseCrAmount = null;
            if ($scope.BaseAmountObj.BaseDrAmount > 0)
                $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
        //if ($scope.voucher.BankMasterId != null) {
        //    $scope.BaseAmountObj.Type = 'Bank';
        //    $scope.BaseAmountObj.BaseCrAmount = $filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount");
        //    $scope.BaseAmountObj.BaseDrAmount = null;
        //    $scope.BaseAmountList.push($scope.BaseAmountObj);
        //    $scope.BaseAmountObj = {};
        //}

    }
    $scope.calDiscountAmount = function () {
        if ($scope.voucher.PaymentSource == 'Discount') {
            $scope.BaseAmountObj.Type = 'Discount';
            $scope.BaseAmountObj.BaseCrAmount = Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "BaseDrAmount") * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseCrAmount += Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList, { ExchangeType: "ExchangeLoss" }), "ExchangeAmount") * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseCrAmount -= Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList, { ExchangeType: "ExchangeGain" }), "ExchangeAmount") * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseDrAmount = null;
            $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
    }
    $scope.calCreditNoteVendorAmount = function () {
        if ($scope.voucher.PaymentSource == 'Vendor') {
            $scope.BaseAmountObj.Type = 'Vendor';
            $scope.BaseAmountObj.BaseCrAmount = Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "BaseDrAmount") * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseCrAmount += Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList, { ExchangeType: "ExchangeLoss" }), "ExchangeAmount") * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseCrAmount -= Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList, { ExchangeType: "ExchangeGain" }), "ExchangeAmount") * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseDrAmount = null;
            $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
    }
    $scope.calExchangeGainBaseAmount = function () {
        if ($scope.voucherDetailList.length) {
            $scope.BaseAmountObj.Type = 'Exchange Gain';
            $scope.BaseAmountObj.BaseCrAmount = Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList, { ExchangeType: "ExchangeGain" }), "ExchangeAmount") * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseDrAmount = null;
            if ($scope.BaseAmountObj.BaseCrAmount > 0)
                $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }

    }
    $scope.calExchangeLossBaseAmount = function () {
        if ($scope.voucherDetailList.length) {
            $scope.BaseAmountObj.Type = 'Exchange Loss';
            $scope.BaseAmountObj.BaseDrAmount = Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList, { ExchangeType: "ExchangeLoss" }), "ExchangeAmount") * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseCrAmount = null;
            if ($scope.BaseAmountObj.BaseDrAmount > 0)
                $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
    }
    $scope.calExchangeTypeGainLossBaseAmount = function () {
        $scope.BaseAmountObj.Type = $scope.voucher.ExchangeType;
        if ($scope.BaseAmountObj.Type == 'ExchangeLoss') {
            $scope.BaseAmountObj.BaseDrAmount = $scope.voucher.ExchangeAmount;
            $scope.BaseAmountObj.BaseCrAmount = 0;
            if ($scope.BaseAmountObj.BaseDrAmount > 0)
                $scope.BaseAmountList.push($scope.BaseAmountObj);
        }
        if ($scope.BaseAmountObj.Type == 'ExchangeGain') {
            $scope.BaseAmountObj.BaseCrAmount = $scope.voucher.ExchangeAmount;
            $scope.BaseAmountObj.BaseDrAmount = 0;
            if ($scope.BaseAmountObj.BaseCrAmount > 0)
                $scope.BaseAmountList.push($scope.BaseAmountObj);
        }

        $scope.BaseAmountObj = {};

    }

    $scope.calBankAmount = function () {
        if ($scope.voucher.BankMasterId != null) {
            $scope.voucher.Amount = Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount") * 1000 + Number.EPSILON) / 1000 + Math.round($filter("sumByKey")($filter("filter")($scope.bankChargesList), "CompanyCurrencyAmount") * 1000 + Number.EPSILON) / 1000 + Math.round($filter("sumByKey")($filter("filter")($scope.purchaseLCChargesList), "ChargesValue") * 1000 + Number.EPSILON) / 1000;
            if ($scope.voucher.CurrencyId == $scope.voucher.BankCurrencyId) {
                //if ($scope.voucher.ExchangeType == 'ExchangeLoss')
                //    $scope.voucher.BankBookAmount = Math.round(($scope.voucher.Amount + $scope.voucher.ExchangeAmount) * 1000 + Number.EPSILON) / 1000;
                //else if ($scope.voucher.ExchangeType == 'ExchangeGain')
                //    $scope.voucher.BankBookAmount = Math.round(($scope.voucher.Amount - $scope.voucher.ExchangeAmount) * 1000 + Number.EPSILON) / 1000;
                //else
                //    $scope.voucher.BankBookAmount = $scope.voucher.Amount
                //    $scope.voucher.BankAmount = $scope.voucher.BankBookAmount;
                if ($scope.voucher.ExchangeType == 'ExchangeLoss')
                    $scope.voucher.BankBookAmount = Math.round((($scope.voucher.Amount * $scope.voucher.CompanyCurrencyRate) + $scope.voucher.ExchangeAmount) * 1000 + Number.EPSILON) / 1000;
                else if ($scope.voucher.ExchangeType == 'ExchangeGain')
                    $scope.voucher.BankBookAmount = Math.round((($scope.voucher.Amount * $scope.voucher.CompanyCurrencyRate) - $scope.voucher.ExchangeAmount) * 1000 + Number.EPSILON) / 1000;
                else
                    $scope.voucher.BankBookAmount = Math.round(($scope.voucher.Amount * $scope.voucher.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;
                $scope.voucher.BankAmount = Math.round($scope.voucher.Amount * 1000 + Number.EPSILON) / 1000;
            }
            if ($scope.voucher.CurrencyId != $scope.voucher.BankCurrencyId) {
                $scope.voucher.Amount = Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount") * 1000 + Number.EPSILON) / 1000 + Math.round($filter("sumByKey")($filter("filter")($scope.bankChargesList), "Amount") * 1000 + Number.EPSILON) / 1000 + Math.round($filter("sumByKey")($filter("filter")($scope.purchaseLCChargesList), "ChargesValue") * 1000 + Number.EPSILON) / 1000;
                if ($scope.voucher.ExchangeType == 'ExchangeLoss')
                    $scope.voucher.BankBookAmount = Math.round((($scope.voucher.Amount * $scope.voucher.CompanyCurrencyRate) + $scope.voucher.ExchangeAmount) * 1000 + Number.EPSILON) / 1000;
                else if ($scope.voucher.ExchangeType == 'ExchangeGain')
                    $scope.voucher.BankBookAmount = Math.round((($scope.voucher.Amount * $scope.voucher.CompanyCurrencyRate) - $scope.voucher.ExchangeAmount) * 1000 + Number.EPSILON) / 1000;
                else
                    //$scope.voucher.BankBookAmount = Math.round(($scope.voucher.Amount * $scope.voucher.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;
                    $scope.voucher.BankBookAmount = Math.round(((Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount") * 1000 + Number.EPSILON) / 1000) * $scope.voucher.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000 + Math.round($filter("sumByKey")($filter("filter")($scope.bankChargesList), "CompanyCurrencyAmount") * 1000 + Number.EPSILON) / 1000 + Math.round($filter("sumByKey")($filter("filter")($scope.purchaseLCChargesList), "BankAmount") * 1000 + Number.EPSILON) / 1000;
                    $scope.voucher.BankAmount = $scope.voucher.BankBookAmount;
            }
        }
    }
    $scope.calBankBaseAmount = function () {
        if ($scope.voucher.BankMasterId != null) {
            $scope.BaseAmountObj.Type = 'Bank';
            if ($scope.voucher.CurrencyId == $scope.voucher.BankCurrencyId) {
                //$scope.BaseAmountObj.BaseCrAmount = Math.round(($scope.voucher.BankBookAmount * $scope.voucher.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;
                $scope.BaseAmountObj.BaseCrAmount = $scope.voucher.BankBookAmount;
            }
            else {

                $scope.BaseAmountObj.BaseCrAmount = $scope.voucher.BankBookAmount;
            }
            $scope.BaseAmountObj.BaseDrAmount = null;
            if ($scope.BaseAmountObj.BaseCrAmount > 0)
                $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
    }

    $scope.calCashBaseAmount = function () {
        if ($scope.voucher.CashMasterId != null) {
            $scope.BaseAmountObj.Type = 'Cash';
            $scope.BaseAmountObj.BaseCrAmount = Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "BaseDrAmount") * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseDrAmount = null;
            if ($scope.BaseAmountObj.BaseCrAmount > 0)
                $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
    }
    $scope.calBankChargesBaseAmount = function () {
        if ($scope.bankChargesList.length >0) {
            $scope.BaseAmountObj.Type = 'BankCharges';
            $scope.BaseAmountObj.BaseDrAmount = Math.round($filter("sumByKey")($filter("filter")($scope.bankChargesList), "CompanyCurrencyAmount") * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseCrAmount = null;
            if ($scope.BaseAmountObj.BaseDrAmount > 0)
                $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
        //else if ($scope.bankChargesList.length > 1) {
        //    $scope.BaseAmountObj.Type = 'BankCharges';
        //    $scope.BaseAmountObj.BaseDrAmount = Math.round($filter("sumByKey")($filter("filter")($scope.bankChargesList), "CompanyCurrencyAmount") * 1000 + Number.EPSILON) / 1000;
        //    $scope.BaseAmountObj.BaseCrAmount = null;
        //    for (var i = 0; i < $scope.BaseAmountList.length; i++) {
        //        if ($scope.BaseAmountList[i].Type == 'BankCharges') {
        //            $scope.BaseAmountList[i].BaseDrAmount = $scope.BaseAmountObj.BaseDrAmount;
        //        }
        //    }
        //    $scope.BaseAmountObj = {};
        //}

    }
    $scope.calOtherChargesBaseAmount = function () {
        if ($scope.purchaseLCChargesList.length > 0) {
            $scope.BaseAmountObj.Type = 'OtherCharges';
            $scope.BaseAmountObj.BaseDrAmount = Math.round($filter("sumByKey")($filter("filter")($scope.purchaseLCChargesList), "BankAmount") * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseCrAmount = null;
            if ($scope.BaseAmountList.length > 0) {
                for (var i = 0; i < $scope.BaseAmountList.length; i++) {
                    if ($scope.BaseAmountList[i].Type == 'OtherCharges') {
                        $scope.BaseAmountList[i].BaseDrAmount = $scope.BaseAmountObj.BaseDrAmount;
                    }
                }
            }

            else {
                if ($scope.BaseAmountObj.BaseDrAmount > 0)
                    $scope.BaseAmountList.push($scope.BaseAmountObj);
            }
            
            $scope.BaseAmountObj = {};
        }

    }

    $scope.caltaxBaseAmount = function () {
        if ($scope.TDSList.length > 0) {
            $scope.BaseAmountObj.Type = 'Tax';
            $scope.BaseAmountObj.BaseCrAmount = Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount") * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseDrAmount = null;
            $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
    }

    $scope.calGLBaseAmount = function () {
        if ($scope.glList.length) {
            $scope.BaseAmountObj.Type = 'GL';
            $scope.BaseAmountObj.BaseCrAmount = Math.round(((Math.round($filter("sumByKey")($filter("filter")($scope.glList), "Amount") * 1000 + Number.EPSILON) / 1000) * $scope.voucher.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseDrAmount = null;
            if ($scope.BaseAmountObj.BaseCrAmount > 0)
                $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
    }
    $scope.calAdvanceBaseAmount = function () {
        if ($scope.advanceList.length) {
            $scope.BaseAmountObj.Type = 'Advance';
            $scope.BaseAmountObj.BaseCrAmount = Math.round(((Math.round($filter("sumByKey")($filter("filter")($scope.advanceList), "Amount") * 1000 + Number.EPSILON) / 1000) * $scope.voucher.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;
            $scope.BaseAmountObj.BaseDrAmount = null;
            if ($scope.BaseAmountObj.BaseCrAmount > 0)
                $scope.BaseAmountList.push($scope.BaseAmountObj);
            $scope.BaseAmountObj = {};
        }
    }
    $scope.calAdvanceBaseAmountValidation = function (data) {
        var crbalance = parseFloat(data.AdvanceAmount), cramount = parseFloat(data.Amount);
        if (cramount > crbalance) {
            data.Amount = data.AdvanceAmount;
            ShowResult("Advance Amount should not exceed Advance Balance Amount.", "failure");
        }
        
    }
    var CurrencyDifferentRate = 0.0000;
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
            CurrencyDifferentRate = ($scope.voucher.CompanyCurrencyRate - data.CompanyCurrencyRate).toFixed(2);
            //data.ExchangeAmount = Math.round((data.Amount * ($scope.voucher.CompanyCurrencyRate - data.CompanyCurrencyRate)) * 1000 + Number.EPSILON) / 1000;
            data.ExchangeAmount = Math.round((data.Amount * CurrencyDifferentRate) * 1000 + Number.EPSILON) / 1000;
            data.ExchangeType = "ExchangeLoss";
            data.BaseDrAmount = Math.round((data.Amount * data.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;

        }
        else if (data.CompanyCurrencyRate > $scope.voucher.CompanyCurrencyRate) {
            CurrencyDifferentRate = (data.CompanyCurrencyRate - $scope.voucher.CompanyCurrencyRate).toFixed(2);
            //data.ExchangeAmount = Math.round((data.Amount * (data.CompanyCurrencyRate - $scope.voucher.CompanyCurrencyRate)) * 1000 + Number.EPSILON) / 1000;
            data.ExchangeAmount = Math.round((data.Amount * CurrencyDifferentRate) * 1000 + Number.EPSILON) / 1000;
            data.ExchangeType = "ExchangeGain";
            data.BaseDrAmount = Math.round((data.Amount * data.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;
        }
        else {
            data.ExchangeAmount = 0;
            data.BaseDrAmount = Math.round((data.Amount * data.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;
            data.ExchangeType = null;
        }
        $scope.calBaseAmount();
    };

    $scope.exchangeGainLossCal = function (rate) {
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if ($scope.voucherDetailList[i].CompanyCurrencyRate < rate) {
                CurrencyDifferentRate = (rate - $scope.voucherDetailList[i].CompanyCurrencyRate).toFixed(2);
                //$scope.voucherDetailList[i].ExchangeAmount = Math.round($scope.voucherDetailList[i].Amount * (rate - $scope.voucherDetailList[i].CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;
                $scope.voucherDetailList[i].ExchangeAmount = Math.round(($scope.voucherDetailList[i].Amount * CurrencyDifferentRate) * 1000 + Number.EPSILON) / 1000;
                $scope.voucherDetailList[i].ExchangeType = "ExchangeLoss";
            }
            else if ($scope.voucherDetailList[i].CompanyCurrencyRate > rate) {
                CurrencyDifferentRate = ($scope.voucherDetailList[i].CompanyCurrencyRate - rate).toFixed(2);
                //$scope.voucherDetailList[i].ExchangeAmount = Math.round($scope.voucherDetailList[i].Amount * ($scope.voucherDetailList[i].CompanyCurrencyRate - rate) * 1000 + Number.EPSILON) / 1000;
                $scope.voucherDetailList[i].ExchangeAmount = Math.round(($scope.voucherDetailList[i].Amount * CurrencyDifferentRate) * 1000 + Number.EPSILON) / 1000;
                $scope.voucherDetailList[i].ExchangeType = "ExchangeGain";
            }
            else {
                $scope.voucherDetailList[i].ExchangeAmount = 0;
                $scope.voucherDetailList[i].ExchangeType = null;
            }
        }
        $scope.calBaseAmount();
    };
    $scope.reCalBankBaseAmount = function () {
        if ($scope.voucher.BankMasterId != null && $scope.BaseAmountList.length > 0) {
            for (var i = 0; i < $scope.BaseAmountList.length; i++) {
                if ($scope.BaseAmountList[i].Type == 'Bank') {
                    $scope.BaseAmountList[i].BaseCrAmount = $scope.voucher.BankBookAmount;
                }
            }
        }
    }
    $scope.recalBankAmount = function () {
        if ($scope.voucher.BankMasterId != null) {
            if ($scope.voucher.CurrencyId == $scope.voucher.BankCurrencyId) {

                $scope.voucher.BankBookAmount = Math.round(($scope.voucher.BankAmount * $scope.voucher.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;
            }
            if ($scope.voucher.CurrencyId != $scope.voucher.BankCurrencyId) {
                $scope.voucher.BankBookAmount = $scope.voucher.BankAmount;
                $scope.reCalBankBaseAmount();

            }
        }
        $scope.calBaseAmount();
    }

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
        $scope.calBaseAmount();
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
                $scope.voucher.BankCurrencyCode = bank.CurrencyCode;

                $scope.voucher.GLGeneralInfoId = bank.GLGeneralInfoId;
                $scope.voucher.GLGeneralInfoName = bank.GLGeneralInfoName;
                $scope.voucher.BudgetMasterId = bank.BudgetMasterId;
                $scope.voucher.BudgetName = bank.BudgetName;
                $scope.voucher.ActivityId = bank.ActivityId;
                $scope.voucher.ActivityName = bank.ActivityName;
                //$scope.checkBankAmount();
            }
        }
        $scope.hideBankPopUp();
        $scope.calBaseAmount();
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
        $scope.voucher.BaseCurrencyId = null;
        $scope.voucher.VoucherTypeId = null;
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.PaymentSource = "Bank";
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.getCboVoucherTypePaymentList();
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
        $scope.bankChargesList = [];
        $scope.purchaseLCChargesList = [];
        $scope.advanceTaxesList = [];
        $scope.glList = [];
        $scope.advanceList = [];
        $scope.ExistingLoanList = [];
        $scope.advanceTax = {};
        $scope.bankCharge = {};
        $scope.TotalAdvanceAmount = 0;
        $scope.TotalDebitNoteAmount = 0;
        $scope.employeeadvance.EmployeeAdvanceDetailId = null;
        $scope.actionIsDisable = false;
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
        if (!baseService.isUndefinedOrNull($scope.TDS.TaxCodeId)) {
            ShowResult("Please add Taxes!", "failure");
            return true;
        }
        if ($scope.partyType === "Customer") {
            if (baseService.isUndefinedOrNull($scope.voucher.PartyId)) {
                ShowResult("Please select Customer!", "failure");
                return true;
            }
            if (parseFloat($scope.voucher.Amount) === 0) {
                ShowResult(" Amount must greater than 0!", "failure");
                return true;
            }
            var vdetailCr = $filter("filter")($scope.voucherDetailList, { TrnType: "Cr" });
            if (vdetailCr.length === 0) {
                ShowResult("Please Select Customer Receivable !", "failure");
                return true;
            }
            if (baseService.isUndefinedOrNull($scope.voucher.GLGeneralInfoId)) {
                ShowResult("Please select Cash or Bank!", "failure");
                return true;
            }
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
            $scope.BaseDrAmount = $filter("sumByKey")($filter("filter")($scope.BaseAmountList), "BaseDrAmount");
            $scope.BaseCrAmount = $filter("sumByKey")($filter("filter")($scope.BaseAmountList), "BaseCrAmount");
            if ($scope.BaseDrAmount != $scope.BaseCrAmount) {
                ShowResult("JV Books Dr Cr are not equal!", "failure");
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
        //if (!baseService.isUndefinedOrNull($scope.voucher.BankCurrencyId)) {
        //    if ($scope.voucher.BankCurrencyId === $scope.companyCurrencyId) {
        //        $scope.voucher.BankAmount = $scope.voucher.Amount;
        //    }
        //}
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


    $scope.actionIsDisable = false;
    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        $scope.passBankCashAmount();
        $scope.entityValidation();
        if ($scope.form1.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            $scope.actionIsDisable = true;
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "accounts/Invoice/InsertVendorPayment",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.voucherDetailList,
                        "bankChargeDetailVMList": $scope.bankChargesList,
                        "purchaseLCChargesVMList": $scope.purchaseLCChargesList,
                        "taxDetailVMList": $scope.TDSList,
                        "glVMList": $scope.glList,
                        "advanceVMList": $scope.advanceList,
                        "VoucherDetailVM": $scope.employeeadvance,
                        "existingLoanList": $scope.ExistingLoanList
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
                        $scope.actionIsDisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.actionIsDisable = false;
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
            $scope.bankCharge.CompanyCurrencyAmount = Math.round(($scope.bankCharge.Amount * $scope.voucher.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;
        }
    };

    bankService.getCboBankChargeTypeList(function (result) {
        $scope.bankChargeTypeList = result;
    });

    $scope.removeChargesRow = function (index) {
        $scope.bankChargesList.splice(index, 1);
    };


    $scope.copyTaxesAmount = function () {
        if ($scope.advance.CurrencyId === $scope.companyCurrencyId) {
            $scope.advanceTax.CompanyCurrencyAmount = $scope.advanceTax.TaxAmount;
        }
        else {
            $scope.advanceTax.CompanyCurrencyAmount = Math.round(($scope.advanceTax.TaxAmount * $scope.advance.CompanyCurrencyRate) * 100 + Number.EPSILON) / 100;
        }
    };



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
    $scope.delete = function (invoiceWriteOffId, voucherId, deletedRemarks) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "invoiceWriteOffId": invoiceWriteOffId, "voucherId": voucherId, "deletedRemarks": deletedRemarks
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
                $scope.invoiceWriteOffId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.deletedRemarks = "";
    $scope.invoiceWriteOffId = null;
    $scope.confirmDelete = function (invoiceWriteOffId, voucherId) {
        $scope.invoiceWriteOffId = invoiceWriteOffId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp_Remarks")).modal("show");
    };

    $scope.closeconfirmDeletePopUp_Remarks = function () {
        angular.element(document.querySelector("#confirmDeletePopUp_Remarks")).modal("hide");
    };

    $scope.closeNotePartyPopUpNew = function (x) {
        var party = x.data;
        $scope.voucher.OtherPartyName = party.Code + " - " + party.UserName;
        $scope.voucher.OtherPartyId = party.Id;
        $scope.notepartyPlantList = [];
        $scope.getNotePartyPlantList(party.Id);
        $scope.voucherDetailList = [];
        $scope.hideNotePartyPopUp();
    };

    $scope.transactionTypeList = function () {
        $scope.financingTypeList = [];
        accountService.getCboCreditNoteTypeList($scope.partyType, function (result) {
            $scope.financingTypeList = result;
        });
    };

    $scope.transactionTypeList();

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.voucher.EmployeeName = employee.EmployeeName;
            $scope.voucher.EmployeeId = employee.SystemId;
            $scope.GetCboExpensesBookingTransactionType();
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };
    $scope.clearEmployee = function () {
        $scope.voucher.EmployeeName = null;
        $scope.voucher.EmployeeId = null;
    };
    $scope.selectTransactionTye = function (id) {
        var employeeTransactionTypeData = $filter("filter")($scope.employeeTransactionTypeList, { EmployeeTransactionTypeId: id });
        $scope.voucher.GLGeneralInfoId = employeeTransactionTypeData[0].PayableGLId;
        $scope.voucher.GLGeneralInfoName = employeeTransactionTypeData[0].PayableGLCode + " - " + employeeTransactionTypeData[0].PayableGLName;
        $scope.voucher.BudgetMasterId = employeeTransactionTypeData[0].PayableBudgetMasterId;
        $scope.voucher.BudgetName = employeeTransactionTypeData[0].PayableBudgetName;
        $scope.voucher.ActivityId = employeeTransactionTypeData[0].PayableActivityId;
        $scope.voucher.ActivityName = employeeTransactionTypeData[0].PayableActivityName;
    };
    $scope.employeeTransactionTypeList = [];
    $scope.GetCboExpensesBookingTransactionType = function () {
        cboService.GetCboExpensesBookingTransactionType(function (result) {
            $scope.employeeTransactionTypeList = result;
            if ($scope.employeeTransactionTypeList.length === 1) {
                $scope.voucher.EmployeeTransactionTypeId = $scope.employeeTransactionTypeList[0].EmployeeTransactionTypeId;
                $scope.selectTransactionTye($scope.voucher.EmployeeTransactionTypeId);
            }
        });
    };

    $scope.TDSCboList = [];
    $scope.TDSlistMessage = "";
    $scope.getTDS = function (date) {
        $http({
            method: "get",
            url: "accounts/TaxCode/GetTDSCbo?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.TDSlistMessage = response.data.Message;
                }
                else {
                    $scope.TDSCboList = response.data;;
                }
            },
            function errorCallback(response) {
            });
    };

    $scope.getTDS($filter("dateFiltering")(Date.now()));

    $scope.TDS = {
        TaxCodeId: null,
        Text: null,
        TaxAmount: null,
        ValueOfFixed: null,
        CompanyCurrencyAmount: null,
        Type: null
    };
    $scope.selectTDS = function () {
        $scope.TDS.ValueOfFixed = $.grep($scope.TDSCboList, function (item) {
            return item.Id === $scope.TDS.TaxCodeId;
        })[0].ValueOfFixed;
        $scope.TDS.Type = $.grep($scope.TDSCboList, function (item) {
            return item.Id === $scope.TDS.TaxCodeId;
        })[0].Type;
        //if ($scope.TDS.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.TDS.ValueOfFixed)) {
        //    $scope.TDS.TaxAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount") * $scope.TDS.ValueOfFixed / 100).toFixed(4);
        //}
    }
    $scope.TDSList = [];
    $scope.addTDS = function () {
        if (manualValidation("td_TDS_TaxCode", baseService.isUndefinedOrNull($scope.TDS.TaxCodeId), "Tax Code is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TDS_TaxCodeAmount", baseService.isUndefinedOrNull($scope.TDS.TaxAmount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TDS_TaxCodeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.TDS.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.TDS.TaxName = $.grep($scope.TDSCboList, function (item) {
                return item.Id === $scope.TDS.TaxCodeId;
            })[0].UserName;

            $scope.TDSList.push($scope.TDS);
            $scope.TDS = {};
        }
        $scope.calBaseAmount();
    };
    $scope.removeTDSRow = function (index) {
        $scope.TDSList.splice(index, 1);
    };
    //$scope.advanceCA = null;
    //$scope.getTransactionTypeGL = function (id) {
    //    if (!baseService.isUndefinedOrNull(id)) {
    //        $scope.advanceCA = $.grep($scope.financingTypeList, function (item) {
    //            return item.FinancingTypeId === id;
    //        })[0];
    //    }
    //    else {
    //        manualValidation("div_TransactionType", false, "");
    //        $scope.advanceCA = null;
    //    }
    //};

    $scope.removeglRow = function (index, data) {
        $scope.glList.splice(index, 1);
    };
    $scope.removeAdvanceRow = function (index, data) {
        $scope.advanceList.splice(index, 1);
    };
    $scope.searchglByList = [
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
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

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoName",
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.popUp = function () {
        $scope.customerInvoiceGLList = [];
        baseService.setCurrentPage("cOAICodeList");
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetAllGLBudgetActivityPostingAutomaticOnly", pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "GLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function (x) {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };
    $scope.glList = [];
    $scope.setSelected = function (data) {
        $scope.addRow(data);
    };

    $scope.addRow = function (data) {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure", "GLPopUp");
            return true;
        }
        var getRow = $filter("filter")($scope.glList, { "TrnType": "Dr", "BudgetMasterId": data.BudgetMasterId, "ActivityId": data.ActivityId, });
        if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].BudgetMasterId === data.BudgetMasterId) {
            ShowResult("This Activity is already added!", "failure", "GLPopUp");
        }
        else {
            $scope.voucherDetail.BudgetMasterId = data.BudgetMasterId;
            $scope.voucherDetail.BudgetCode = data.BudgetCode;
            $scope.voucherDetail.BudgetName = data.BudgetName;
            $scope.voucherDetail.ActivityId = data.ActivityId;
            $scope.voucherDetail.ActivityCode = data.ActivityCode;
            $scope.voucherDetail.ActivityName = data.ActivityName;

            $scope.voucherDetail.GLGeneralInfoId = data.GLGeneralInfoId;
            $scope.voucherDetail.GLGeneralInfoCode = data.GLGeneralInfoCode;
            $scope.voucherDetail.GLGeneralInfoName = data.GLGeneralInfoName;

            $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
            $scope.voucherDetail.CrAmount = 0;
            $scope.voucherDetail.DrAmount = 0;
            $scope.voucherDetail.TrnType = "Dr";
            $scope.glList.push($scope.voucherDetail);
            $scope.voucherDetail = {};
            $scope.closeCOAICodeListPopUp();
        }
    };
    //*********************** Employee Advance PopUp Start *************************************
    $scope.employeeAdvanceSearchList = [
        {
            "Text": "VoucherNo",
            "Value": "VoucherNo"
        },
        {
            "Text": "Employee Code",
            "Value": "EmployeeCode"
        },
        {
            "Text": "Employee Name",
            "Value": "EmployeeName"
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

    $scope.employeeAdvanceDataList = [];
    $scope.employeeAdvanceSearch = [];
    $scope.employeeAdvanceUrl = 'accounts/Advance/GetEmployeeAvilabeAdvanceList';
    $scope.employeeAdvanceSelectedIndex = -1;
    //$scope.employeeAdvanceParameters = {
    //    limit: 10,
    //    offset: 0,
    //    order: 'ASC',
    //    sort: 'VoucherNo',
    //    searchBy: 'VoucherNo',
    //    pageSize: 10,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};

    //$scope.showEmployeeAdvancePopUpList = function (employeeId) {
    //    $scope.compareCurrencyId = $scope.advance.CurrencyId;
    //    $scope.getEmployeeAdvanceData = function (pageno) {
    //        baseService.paginationBase($scope.employeeAdvanceUrl, pageno, $scope.employeeAdvanceParameters)
    //            .then(function (response) {
    //                $scope.employeeAdvanceDataList = response.Rows;
    //                $scope.employeeAdvanceParameters.total_count = response.Total;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //    };
    //    angular.element(document.querySelector('#employeeAdvancePopUp')).modal('show');
    //    $scope.getEmployeeAdvanceData();
    //};

    $scope.employeeAdvanceDataList = [];
    $scope.employeeAdvanceSearch = [];
    $scope.searchByEmployeeAdvance = "EmployeeName"; $scope.searchAdvance = "";
    $scope.showEmployeeAdvancePopUpList = function () {
        $scope.compareCurrencyId = $scope.advance.CurrencyId;
        $http({
            method: 'POST',
            url: 'accounts/Advance/GetEmployeeAvilabeAllAdvanceList',
            data: { column: $scope.searchByEmployeeAdvance, value: $scope.searchAdvance },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.employeeAdvanceDataList = response.data;
        });
        angular.element(document.querySelector('#employeeAdvancePopUp')).modal('show');
    };

    $scope.employeeadvance = {
        EmployeeAdvanceDetailId: null
    };

    $scope.advanceList = [];
    $scope.closeEmployeeAdvancePopUp = function (obj) {
        var data = obj.data;
        $scope.advanceList = [];
        $scope.advance.EmployeeId = data.EmployeeId;
        $scope.advance.EmployeeName = data.EmployeeName;
        $scope.advance.AdvanceAmount = data.Balance;
        $scope.advance.VoucherNo = data.VoucherNo;
        $scope.advance.CompanyId = data.CompanyId;
        $scope.advance.PlantId = data.PlantId;
        $scope.advance.CurrencyId = data.CurrencyId;
        $scope.voucher.CurrencyId = data.CurrencyId;
        $scope.voucher.CompanyCurrencyRate = data.CompanyCurrencyRate;
        $scope.voucher.AdvanceId = data.AdvanceId;
        $scope.voucher.AdvanceDetailId = data.AdvanceDetailId;
        $scope.advance.AdvanceId = data.AdvanceId;
        $scope.advance.AdvanceDetailId = data.AdvanceDetailId;
        $scope.advance.PartyType = data.PartyType;
        $scope.advance.advancePostingDate = data.PostingDate;
        $scope.advance.advanceDocRefNo = data.DocRefNo;
        $scope.advance.CrAmount = null;
        $scope.advance.JournalType = data.JournalType;
        $scope.advance.BudgetMasterId = data.BudgetMasterId;
        $scope.advance.BudgetCode = data.BudgetCode;
        $scope.advance.BudgetName = data.BudgetName;
        $scope.advance.ActivityId = data.ActivityId;
        $scope.advance.ActivityCode = data.ActivityCode;
        $scope.advance.ActivityName = data.ActivityName;
        $scope.advance.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.advance.GLGeneralInfoCode = data.GLGeneralInfoCode;
        $scope.advance.GLGeneralInfoName = data.GLGeneralInfoName;
        $scope.employeeadvance.EmployeeAdvanceDetailId = data.EmployeeAdvanceDetailId;
        $scope.advanceList.push($scope.advance);
        //$scope.GetEmployeeTransactionNo($scope.advance.EmployeeId);
        angular.element(document.querySelector("#employeeAdvancePopUp")).modal("hide");
    };
    $scope.loanDataList = [];
    $scope.getloanPopUpData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/Loan/GetLoanPopUpList?transactionType=' + "LoanTaken"
        }).then(function successCallback(response) {
            $scope.loanDataList = response.data;
        });
    };
    $scope.showloanPopUp = function () {
        $scope.getloanPopUpData();
        angular.element(document.querySelector('#loanPopUp')).modal('show');
    };
    $scope.closeloanPopUp = function () {
        angular.element(document.querySelector("#loanPopUp")).modal("hide");
    };
    $scope.ExistingLoanList = [];
    $scope.existingLoan = {};
    $scope.closeloanPopUpSelected = function (x) {
        var data = x.data;
        //if (baseService.isUndefinedOrNull($scope.voucher.PurchaseLCId)) {
        //    ShowResult("Please Select LC !", "failure", "loanPopUp");
        //    return;
        //}
        var getRow = null;
        getRow = $filter("filter")($scope.ExistingLoanList, { "FinancingId": data.FinancingId });
        if (getRow.length === 0) {
            $scope.existingLoan.FinancingId = data.FinancingId;
            $scope.existingLoan.FinancingDetailId = data.FinancingDetailId;
            $scope.existingLoan.FinancingTypeId = data.FinancingTypeId;
            $scope.existingLoan.VoucherNo = data.VoucherNo;
            $scope.existingLoan.PartyName = data.Particulars;
            $scope.existingLoan.PartyId = data.PartyId;
            $scope.existingLoan.PartyType = data.PartyType;
            $scope.existingLoan.PartyPlantName = data.PartyPlantName;
            $scope.existingLoan.CurrencyId = data.CurrencyId;
            $scope.existingLoan.CurrencyCode = data.CurrencyCode;
            $scope.existingLoan.EntityId = data.EntityId;
            $scope.existingLoan.FinancingTypeId = data.FinancingTypeId;
            $scope.existingLoan.CompanyId = data.CompanyId;
            $scope.existingLoan.PlantId = data.PlantId;
            $scope.existingLoan.LoanAmount = data.LoanAmount - data.AdditionalLoanAmount;
            $scope.existingLoan.LoanSetOff = data.LoanPayment;
            $scope.existingLoan.Balance = data.Balance;
            $scope.existingLoan.LoanDocRefNo = data.DocRefNo;
            $scope.existingLoan.InitialSactionAmount = data.InitialSactionAmount;
            $scope.existingLoan.AdditionalLoanAmount = data.AdditionalLoanAmount;
            $scope.existingLoan.LoanPostingDate = data.PostingDate;
            $scope.existingLoan.LoanDocDate = data.DocDateNew;
            $scope.existingLoan.InterestWriteOff = data.InterestWriteOff;
            $scope.existingLoan.InterestBalance = data.InterestBalance;
            $scope.existingLoan.InterestCashPayment = data.InterestCashPayment;
            $scope.existingLoan.InterestAmount = data.InterestAmount - data.OtherExpensesPayable;
            $scope.existingLoan.OtherExpensesPayable = data.OtherExpensesPayable;
            $scope.existingLoan.TotalLoanLiability = data.LoanAmount + $scope.existingLoan.InterestAmount + $scope.existingLoan.OtherExpensesPayable
            $scope.existingLoan.TotalInterestPayableAmount = data.InterestAmount;
            $scope.existingLoan.ToCurrencyRate = data.CompanyCurrencyRate;
            $scope.existingLoan.PartyPlantId = data.PartyPlantId;

            $scope.ExistingLoanList.push($scope.existingLoan);
            $scope.existingLoan = {};
           
            $scope.voucher.FinancingId = data.FinancingId;
            $scope.voucher.FinancingDetailId = data.FinancingDetailId;
            $scope.voucher.FinancingTypeId = data.FinancingTypeId;
            angular.element(document.querySelector("#loanPopUp")).modal("hide");
        }
        else {
            ShowResult(data.DocRefNo + " already  Exist", "failure", "loanPopUp");
        }
    };

    $scope.removeRowLoan = function (index) {
        $scope.ExistingLoanList.splice(index, 1);
    };

    $scope.LCChargesList = [];
    $scope.GetPurchaseLCCharges = function () {
        try {
            $scope.LCChargesList = [];
            $http.get("Commercial/PurchaseLC/GetOpenLCChargesGLData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.LCChargesList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#LCChargesPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseLCPopUp = function () {
        angular.element(document.querySelector('#LCChargesPopUp')).modal('hide');
    }

    $scope.purchaseLCChargesList = [];
    $scope.Rate = null;
    $scope.SelectedLC = function () {
        if (baseService.arrayLength($scope.LCChargesList) > 0) {
            angular.forEach($scope.LCChargesList, function (a) {
                if (a.Active) {
                    if (checkLCExist($scope.purchaseLCChargesList, a.Id) === false) {
                        $scope.purchaseLCChargesList.push({
                            Id: null
                            , OverHeadTypeGLId: a.Id
                            , OverHeadType: a.OverHeadType
                            , GL: a.GL
                            , Budget: a.Budget
                            , Activity: a.Activity
                            , ExpensesGLId: a.ExpensesGLId
                            , ExpensesBudgetMasterId: a.ExpensesBudgetMasterId
                            , ExpensesActivityId: a.ExpensesActivityId
                            , ChargesValue: null
                            , BankAmount: null
                        });
                    }
                }

            });
        }
        else
            angular.forEach($scope.purchaseLCChargesList, function (a) {
                if (!baseService.valueCheckInList($scope.purchaseLCChargesList, 'Id', a.OverHeadTypeGLId))
                    $scope.purchaseLCChargesList.splice(a, 1);
            });
        $scope.calBaseAmount();
        $scope.CloseLCPopUp();
    };

    function checkLCExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].OverHeadTypeGLId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.removeRowPurchaseLCCharges = function (index, data) {
        $scope.purchaseLCChargesList.splice(index, 1);
        $scope.calBaseAmount();
    };
    $scope.copyOtherChargesAmount = function (index) {
        if ($scope.voucher.CurrencyId === $scope.companyCurrencyId) {
            $scope.purchaseLCChargesList[index].BankAmount = $scope.purchaseLCChargesList[index].ChargesValue;
        }
        else {
            $scope.purchaseLCChargesList[index].BankAmount = Math.round(($scope.purchaseLCChargesList[index].ChargesValue * $scope.voucher.CompanyCurrencyRate) * 1000 + Number.EPSILON) / 1000;
        }
        $scope.calBaseAmount();
    };
}