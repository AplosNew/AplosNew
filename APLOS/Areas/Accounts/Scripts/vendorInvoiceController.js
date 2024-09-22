"use strict";
vendorInvoiceController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "bankService", '$window'];
function vendorInvoiceController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, bankService, $window) {
    $rootScope.title = "Vendor Invoice";
    $scope.voucherDetailList = [];
    $scope.taxCodDataList = [];
    $scope.Action = "Save";
    $scope.url = "Accounts/Invoice";
    $scope.listUrl = $scope.url + "/GetVendorInvoiceList";
    $scope.saveUrl = $scope.url + "/InsertVendorInvoice";
    $scope.updateUrl = $scope.url + "/UpdateVendorInvoice";
    $scope.postUrl = $scope.url + "/PostVoucher";
    $scope.reportUrl = $scope.url + "/ReportCustomerAdvance?voucherId=";
    $scope.jouranlUrl = $scope.url + "/GetAvailableJournalCustomerAdvance";

    $scope.deleteUrl = $scope.url + "/DeleteVendorInvoice";

    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $scope.partyType = "Vendor";
    $scope.isAdvance = false;
    $controller("partyBaseController", { $scope: $scope, $http: $http });

    $scope.partyGLList = [];
    $scope.taxExemption = false;
    $scope.IsBaseOnDueDateEnable = true;

    $scope.voucher = {
        Id: null,
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
        PaymentSource: "GL",
        InvoiceType: "Vendor",
        LCRef: null,
        IsLoanSetOff: false,
        CashMasterId: null,
        CompanyCurrencyRate: 1,
        EmployeeName: null,
        EmployeeId: null,
        BeneficiaryType: null,
        EmployeeTransactionTypeId: null,
        AccountType: null
    };

    $scope.voucherDetail = {
        EntityId: null,
        InvoiceTaxViewModel: [
            {
                TaxAmount: 0,
                TaxAutoAmount: 0,
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

    $scope.voucherDetailCurrency = {
        Id: null,
        VoucherId: null,
        VoucherDetailId: null,
        ParallelCurrencyId: null,
        FromCurrencyId: null,
        ToCurrencyId: null,
        ToCurrencyRate: null,
        DrAmount: null,
        CrAmount: null,
        TrnType: null
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
            "name": "Vendor Code",
            "value": "PartyCode"
        },
        {
            "name": "Particulars",
            "value": "Particulars"
        },
        {
            "name": "Ordering Vendor",
            "value": "PartyPlantName"
        },
        {
            "name": "Posting Date",
            "value": "PostingDate"
        },
        {
            "name": "Doc Date",
            "value": "DocDate"
        },
        {
            "name": "Doc Ref",
            "value": "DocRefNo"
        }
        ,
        {
            "name": "Beneficiary",
            "value": "BeneficiaryType"
        }
        ,
        {
            "name": "Status",
            "value": "Status"
        }
    ];

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;

    });
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });

    $scope.costCenterCboList = [];
    $scope.GetCboCostCenterIdByEntity = function (entityId) {
        $http({
            method: "GET",
            url: "accounts/expenseBooking/GetCboCostCenterIdByEntity?entityId=" + entityId
        }).then(function successCallback(response) {
            $scope.costCenterCboList = response.data;

        });
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

    bankService.getCashMasterCboList(function (result) {
        $scope.cashMasterList = result;
    });

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
        if (!baseService.isUndefinedOrNull(date)) {
            date = new Date(date);
            date.setDate(date.getDate() + days);
            $scope.voucher.MatureDate = $filter("date")(date, "dd-MMM-yyyy");
        }
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
        $scope.getFiscalInvoiceTotalAmountByParty($scope.voucher.PartyId, $scope.voucher.PostingDate);
    };
    $scope.beneficiaryTypeList = [];

    $scope.getBeneficiaryType = function () {
        $http({
            method: "GET",
            url: "Enum/GetNewBeneficiaryTypeCbo/"
        }).then(function successCallback(response) {
            $scope.beneficiaryTypeList = response.data;
            for (var i = 0; i < $scope.beneficiaryTypeList.length; i++) {
                if ($scope.beneficiaryTypeList[i].Value == 'Vendor')
                    $scope.voucher.BeneficiaryType = $scope.beneficiaryTypeList[i].Value;
            }
        });
    };
    $scope.getBeneficiaryType();
    $scope.approvedByList = [];
    $scope.getCboApprovedByList = function () {
        cboService.getAuthorizationConfigCbo('JournalApproveBy', function (result) {
            $scope.approvedByList = result;
            if ($scope.approvedByList.length == 1) {
                $scope.voucher.ApprovedById = $scope.approvedByList[0].Id;
            }
        });
    };
    $scope.getCboApprovedByList();
    $scope.validation = function () {
        if ($scope.partyType === "Vendor") {
            if (baseService.isUndefinedOrNull($scope.voucher.PartyId)) {
                ShowResult("Please select vendor!", "failure");
                return true;
            }
            if (baseService.isUndefinedOrNull($scope.voucher.CompanyCurrencyRate)) {
                ShowResult("Rate can not Empty!", "failure");
                return true;
            }
            if ($scope.voucher.PartyId !== LCVendorId && $scope.voucher.InvoiceType === "LC") {
                ShowResult("LC Vendor and Invoice venodr are not same!,Please select Same Vendor!", "failure");
                return true;
            }
            if ($scope.approvedByList.length > 0 && $scope.voucher.ApprovedById==null) {
                ShowResult("Please select Approved By!", "failure");
                return true;
            }
            if ($scope.voucher.PaymentSource === "GL") {
                var vdetailDr = $filter("filter")($scope.voucherDetailList, { TrnType: "Dr" });
                if (vdetailDr.length === 0) {
                    ShowResult("Please select GL!", "failure");
                    return true;
                }
                else {
                    for (var j = 0; j < vdetailDr.length; j++) {
                        if (vdetailDr[j].Amount === 0 || vdetailDr[j].Amount === null) {
                            ShowResult(vdetailDr[j].GLGeneralInfoName + " Amount must greater than 0!", "failure");
                            return true;
                        }
                        else if (vdetailDr[j].IsOrderSpecific === true && $scope.invoiceDetailChargesList.length === 0) {
                            ShowResult(vdetailDr[j].GLGeneralInfoName + ", Please Distribute Expense!", "failure");
                            return true;
                        }
                    }
                }
            }
            else if ($scope.voucher.PaymentSource === "Loan") {
                if ($scope.ExistingLoanList.length === 0) {
                    ShowResult("Please select Loan!", "failure");
                    return true;
                }
                
            }
            else if ($scope.voucher.PaymentSource === "Cash") {
                if (baseService.isUndefinedOrNull($scope.voucher.CashMasterId)) {
                    ShowResult("Please select Cash!", "failure");
                    return true;
                }
                
            }
            else if ($scope.voucher.PaymentSource === "Bank") {
                if (baseService.isUndefinedOrNull($scope.voucher.BankMasterId)) {
                    ShowResult("Please select Bank!", "failure");
                    return true;
                }
            }
            else {
                ShowResult("Please select Source!", "failure");
                return true;
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
            $scope.voucher.CurrencyId = party.CurrencyId;
            $scope.voucher.PartyType = $scope.partyType;
            $scope.voucher.GLGeneralInfoId = party.ReconciliationGLId;
            $scope.voucher.GLGeneralInfoCode = party.ReconciliationGLCode;
            $scope.voucher.GLGeneralInfoName = party.ReconciliationGLName;
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
            $scope.getFiscalInvoiceTotalAmountByParty(party.Id, $scope.voucher.PostingDate);
            clearVoucherDetail();
        }
        $scope.hidePartyPopUp();
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
        if ($scope.partyType === "Vendor") {
            if ($scope.companyConfig.IsVoucherFromBudget) {
                getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "GLGeneralInfoId": $scope.selectedInvoiceGLId, "BudgetMasterId": $scope.voucherDetail.BudgetMasterId, "ActivityId": $scope.voucherDetail.ActivityId });
            }
            else
                getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "GLGeneralInfoId": $scope.selectedInvoiceGLId });
        }

        if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].GLGeneralInfoId === $scope.selectedInvoiceGLId) {
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
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
            //if ($scope.voucher.AccountType = 'Expense') {
            //    $scope.voucherDetail.IsCostCenter = false;
            //}
            //else {
            //    $scope.voucherDetail.IsCostCenter = true;
            //}
            $scope.voucherDetail.Amount = null;
            $scope.voucherDetail.TotalTax = null;
            $scope.voucherDetail.TotalAmount = null;

            $scope.voucherDetail.TrnType = $scope.partyType === "Customer" ? "Cr" : "Dr";
            $scope.voucherDetail.InvoiceTaxViewModel = [];
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            clearVoucherDetail();
            $scope.searchStr = null;
        }
    };

    $scope.changePartyGL = function (glId) {
        var drGL = $.grep($scope.partyGLList, function (item) {
            return item.GLGeneralInfoId === glId;
        })[0];

        if (!$scope.voucher.IsSplit) {
            var row = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr" });
            $scope.voucher.GLGeneralInfoId = drGL.GLGeneralInfoId;
            $scope.voucher.GLGeneralInfoName = drGL.GLGeneralInfoName;
            $scope.voucher.BudgetMasterId = drGL.BudgetMasterId;
            $scope.voucher.BudgetName = drGL.BudgetMasterId;
            $scope.voucher.ActivityId = drGL.ActivityId;
            $scope.voucher.ActivityName = drGL.ActivityId;

            row[0].Amount = $scope.voucher.Amount;
            row[0].GLGeneralInfoId = $scope.voucher.GLGeneralInfoId;
            row[0].GLGeneralInfoName = $scope.voucher.GLGeneralInfoName;
            row[0].BudgetMasterId = $scope.voucher.BudgetMasterId;
            row[0].BudgetName = $scope.voucher.BudgetName;
            row[0].ActivityId = $scope.voucher.ActivityId;
            row[0].ActivityName = $scope.voucher.ActivityName;
        }
    };

    $scope.changePartySplitGL = function (glId) {
        var drGL = $.grep($scope.partyGLList, function (item) {
            return item.GLGeneralInfoId === glId;
        })[0];

        if (!$scope.voucher.IsSplit) {
            var drRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr" });
            drRow[0].GLGeneralInfoId = drGL.GLGeneralInfoId;
            drRow[0].GLGeneralInfoName = drGL.GLGeneralInfoName;
            drRow[0].BudgetMasterId = drGL.BudgetMasterId;
            drRow[0].BudgetName = drGL.BudgetName;
            drRow[0].ActivityId = drGL.ActivityId;
            drRow[0].ActivityName = drGL.ActivityName;
        }
        else {
            $scope.voucherDetail.GLGeneralInfoId = drGL.GLGeneralInfoId;
            $scope.voucherDetail.GLGeneralInfoName = drGL.GLGeneralInfoName;
            $scope.voucherDetail.BudgetMasterId = null;
            $scope.voucherDetail.BudgetName = null;
            $scope.voucherDetail.ActivityId = null;
            $scope.voucherDetail.ActivityName = null;
            $scope.voucherDetail.DocDate = $scope.voucher.DocDate;
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.DrAmount = 0;
            $scope.voucherDetail.TrnType = "Dr";
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
        }
        $scope.getBudgetCboByGL($scope.voucherDetail.GLGeneralInfoId);
        clearVoucherDetail();
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
        var taxdr = $scope.taxCodDataList.length;
        while (taxdr--) {
            if ($scope.taxCodDataList[taxdr]["VoucherDetailId"] === row.VoucherDetailId) {
                $scope.taxCodDataList.splice(taxdr, 1);
            }
        }
    };

    $scope.splitInvoice = function () {
        $scope.removeDrRow();
        if (!$scope.voucher.IsSplit) {
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.GLGeneralInfoId = $scope.voucher.GLGeneralInfoId;
            $scope.voucherDetail.GLGeneralInfoName = $scope.voucher.GLGeneralInfoName;
            $scope.voucherDetail.BudgetMasterId = $scope.voucher.BudgetMasterId;
            $scope.voucherDetail.BudgetName = $scope.voucher.BudgetName;
            $scope.voucherDetail.ActivityId = $scope.voucher.ActivityId;
            $scope.voucherDetail.ActivityName = $scope.voucher.ActivityName;
            $scope.voucherDetail.DocDate = $scope.voucher.DocDate;
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.Amount = $scope.voucher.Amount;
            $scope.voucherDetail.TrnType = $scope.partyType === "Customer" ? "Dr" : "Cr";
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            clearVoucherDetail();
        }
    };

    //Gets data from the Database
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

    $scope.getCboVoucherTypeAccountPayableList = function () {
        cboService.getCboVoucherTypeAccountPayableList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);
            }
        });
    };
    $scope.getCboVoucherTypeAccountPayableList();

    $scope.Clear = function () {
        var voucherTypeId = $scope.voucher.VoucherId;
        $scope.Action = "Save";
        $scope.voucher = {};
        $scope.voucher.InvoiceType = "Vendor";
        $scope.voucher.IsLoanSetOff = false;
        $scope.getCboVoucherTypeAccountPayableList();
        $scope.voucher.Active = true;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
        $scope.taxCodDataList = [];
        $scope.invoiceDetailChargesList = [];
        $scope.selectedInvoiceGLId = null;
        $scope.plantList = [];
        $scope.GLGeneralInfoName = null;
        $scope.BudgetItemList = [];
        $scope.ActivityList = [];
        $scope.checkedInvoiceList = [];
        $scope.checkedOutBoundInvoiceList = [];
        $scope.voucher.PaymentSource = "GL";
        $scope.voucherDetail.InvoiceTaxViewModel = [];
        $scope.advanceTaxesList = [];
        $scope.TDSList = [];
        $scope.voucherDetailId = null;
        clearVoucherDetail();
        $scope.TotalInvoiceAmount = null;
        $scope.CustomerAvailableInvoiceList = [];
        $scope.checkedMasterOrderList = [];
        $scope.checkedContractList = [];
    };


    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.taxCodCboList = [];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
        $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);
    };

    $scope.changePostingGetTaxCode = function () {
        $scope.taxCodCboList = [];
        $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);
        $scope.getTaxCodeByTaxYearWithhold($scope.voucher.PostingDate);
    }
    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYear = function (date) {
        $http({
            method: "get",
            url: "accounts/TaxCode/GetTaxCodeInputVATGST?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message;
                }
                else {
                    var result = response.data;
                    $scope.taxCodCboList = result;
                    if ($scope.taxCodCboList.length === 0) {
                        //$scope.pop("error", "No TaxCode found in this Fiscal Year ");
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
    $scope.activityOrderType = "";
    $scope.GLGeneralInfoId = 0;
    $scope.BudgetMasterId = 0;
    $scope.ActivityId = 0;
    $scope.getExpenseDistribute = function (index, item) {
        $scope.activityOrderType = "";
        $scope.TotalChargesAmount = 0;
        $scope.GLGeneralInfoId = 0;
        $scope.BudgetMasterId = 0;
        $scope.ActivityId = 0;
        $scope.activityOrderType = item.ActivityOrderType;
        $scope.ValueOfDistribution = item.ValueOfDistribution;
        $scope.TotalChargesAmount = item.Amount;
        $scope.GLGeneralInfoId = item.GLGeneralInfoId;
        $scope.BudgetMasterId = item.BudgetMasterId;
        $scope.ActivityId = item.ActivityId;
        
        if ($scope.activityOrderType == "InboundInvoice") {
            $scope.isSet(1);
            if ($scope.ValueOfDistribution == 'Amount') {
                $scope.calDistributedAmount();
            }
            else {
                $scope.calDistributedQtyWise();
            }
        }
        else if ($scope.activityOrderType == "OutboundInvoice") {
            $scope.isSet(2);
            if ($scope.ValueOfDistribution == 'Amount') {
                $scope.calOutBoundDistributedAmount();
            }
            else {
                $scope.calOutBoundDistributedQtyWise();
            }
        }
        else if ($scope.activityOrderType == "BothInOutboundInvoice") {
            $scope.isSet(1);
            if ($scope.ValueOfDistribution == 'Amount') {
                $scope.calDistributedAmount();
                $scope.calOutBoundDistributedAmount();
            }
            else {
                $scope.calDistributedQtyWise();
                $scope.calOutBoundDistributedQtyWise();
            }
        }
        else if ($scope.activityOrderType == "Order") {
            $scope.isSet(3);
            $scope.calMasterOrderDistributedAmount();
        }
        else if ($scope.activityOrderType == "Contract") {
            $scope.isSet(4);
            $scope.calContractDistributedAmount();
        }

        angular.element(document.querySelector("#ExpenseDistributePopUp")).modal("show");
    };

    $scope.closeExpenseDistributePopUp = function () {
        $scope.TotalDistributedAmountInBound = 0;
        $scope.TotalDistributedAmountOutBound = 0;
        $scope.TotalDistributedAmountInBound = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
        $scope.TotalDistributedAmountOutBound = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount"));

        if ((parseFloat($scope.TotalDistributedAmountInBound) + parseFloat($scope.TotalDistributedAmountOutBound)) !== parseFloat($scope.TotalChargesAmount)) {
             ShowResult('Distributed Amount must be equal Taxable Amount.!', 'failure', 'ExpenseDistributePopUp');
        }
        else
        {
            angular.element(document.querySelector("#ExpenseDistributePopUp")).modal("hide");
        }
        
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
                TaxAmount: null,
                TaxAutoAmount: null,
                TaxCodeId: $scope.taxcodedata.TaxCodeId,
                TaxCategoryId: $scope.taxcodedata.TaxCategoryId,
                InvoiceDetailId: null,
                Id: null,
                WithholdCreditableGLId: $scope.taxcodedata.WithholdCreditableGLId,
                ExpensesGLId: $scope.taxcodedata.ExpensesGLId,
                CreditableGLId: $scope.taxcodedata.CreditableGLId,
                IsWithhold: $scope.taxcodedata.IsWithhold,
                IsCreditable: $scope.taxcodedata.IsCreditable,
                IsMerge: $scope.taxcodedata.IsMerge,
                IsRCM: $scope.taxcodedata.IsRCM,
                ManuallyEditable: $scope.taxcodedata.ManuallyEditable,
                TotalTax: null,
                TotalAmount: null,
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
            $scope.voucherDetailList[$scope.setTaxVoucherDetailIndex].TotalTax = $filter("sumByKey")($filter("filter")($scope.voucherDetailList[$scope.setTaxVoucherDetailIndex].InvoiceTaxViewModel), "TaxAmount");
        });
    };

    $scope.updateTaxAmount = function (data, amount) {
        if (data != null) {
            for (var i = 0; i < $scope.taxCodDataList.length; i++) {
                if ($scope.taxCodDataList[i].TaxCodeId === data.TaxCodeId && $scope.taxCodDataList[i].VoucherDetailId == data.VoucherDetailId) {
                    $scope.taxCodDataList[i].TaxAmount = amount;
                    return;
                }
            }
            for (var t = 0; t < baseService.arrayLength($scope.voucherDetailList); t++) {
                if ($scope.voucherDetailList[t].Id === data.VoucherDetailId)
                    for (var a = 0; a < baseService.arrayLength($scope.voucherDetailList[t].InvoiceTaxViewModel); a++) {
                        if ($scope.voucherDetailList[t].InvoiceTaxViewModel[a].TaxCodeId === data.TaxCodeId) {
                            $scope.voucherDetailList[t].InvoiceTaxViewModel[a].TaxAmount = amount;
                            $scope.$scope.voucherDetailList[t].TotalTax += amount;
                            return;
                        }
                    }
            }
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


    $scope.calculatebackTax = function (index) {
        if ($scope.voucherDetailList[index].InvoiceTaxViewModel.length > 0) {
            for (var i = 0; i < $scope.voucherDetailList[index].InvoiceTaxViewModel.length; i++) {
                $scope.voucherDetailList[index].InvoiceTaxViewModel[i].TaxAmount = Math.round((($scope.voucherDetailList[index].Amount * $scope.voucherDetailList[index].InvoiceTaxViewModel[i].ValueOfFixed) / 100) * 100 + Number.EPSILON) / 100
            }
        }
        $scope.TotalChargesAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount"));
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
            $scope.recalculationAdditionaltax();
        }
        else {
            $scope.voucherDetailList[index].TotalAmount = Math.round(($scope.voucherDetailList[index].Amount) * 10000 + Number.EPSILON) / 10000;
            $scope.voucherDetailList[index].TotalTax = 0;
            $scope.voucher.Amount = Math.round(($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount")) + ($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "TotalTax")) * 10000 + Number.EPSILON) / 10000;
        }
    };

    $scope.taxCodCboListWithhold = [];
    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYearWithhold = function (date) {
        $http({
            method: "get",
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
        if ($scope.TDSList.length > 0) {
            for (var i = 0; i < $scope.TDSList.length; i++) {
                $scope.TDSList[i].TaxAmount = Math.round((($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount") * $scope.TDSList[i].ValueOfFixed / 100)) * 100 + Number.EPSILON) / 100;
            }

        }
    }

    $scope.removeTaxesRow = function (index) {
        $scope.advanceTaxesList.splice(index, 1);
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
            $scope.calculateTax($scope.voucherDetailId, $scope.setTaxVoucherDetailIndex);
        }
    };

    $scope.vendorInvoiceTaxes = [];
    $scope.vendorInvoiceTaxPush = function () {
        $scope.calculateTax($scope.voucherDetailId, $scope.setTaxVoucherDetailIndex);
        $scope.closeTaxCodePopUp();
    };

    $scope.checkTaxAllow = function () {
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if ($scope.voucherDetailList[i].TrnType === "Dr" && $scope.voucherDetailList[i].PostingWithoutTaxAllow === false) {
                ShowResult("PositionWithout Tax is not Allow where Amount " + $scope.voucherDetailList[i].Amount, "failure");
                return false;
            }
        }
        return true;
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
            baseService.paginationBase("Accounts/GLItem/GetVendorInvoiceGLBudgetList", pageno, $scope.customerInvoiceGLParameters)
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
        $scope.selectedInvoiceGLName = data.GLGeneralInfoName;
        $scope.selectedInvoiceGLCode = data.GLGeneralInfoCode;
        $scope.voucherDetail.BudgetMasterId = data.BudgetMasterId;
        $scope.voucherDetail.BudgetName = data.BudgetName;
        $scope.voucherDetail.BudgetCode = data.BudgetCode;
        $scope.voucherDetail.ActivityId = data.ActivityId;
        $scope.voucherDetail.ActivityName = data.ActivityName;
        $scope.voucherDetail.ActivityCode = data.ActivityCode;
        $scope.voucherDetail.IsOrderSpecific = data.IsOrderSpecific;
        $scope.voucherDetail.ActivityOrderType = data.ActivityOrderType;
        $scope.voucherDetail.ValueOfDistribution = data.ValueOfDistribution;
        $scope.voucherDetail.AccountType = data.AccountType;
        $scope.addRow();
        $scope.closeGLPopUp();
    };
    $scope.closeGLPopUp = function () {
        angular.element(document.querySelector("#CustomerInvoiceGLPopUp")).modal("hide");
    };
    $scope.invoiceDetailChargesList = [];
    $scope.InvoiceDetailChargesList = function myfunction() {
        $scope.invoiceDetailChargesList = $scope.checkedInvoiceList.concat($scope.checkedOutBoundInvoiceList).concat($scope.checkedMasterOrderList).concat($scope.checkedContractList);
        
    };
    $scope.popUpTDSMessage = false;
    $scope.Save = function () {
        $scope.InvoiceDetailChargesList();
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        if ($scope.TDSList.length > 0) {
            $scope.popUpTDSMessage = true;
        } else $scope.popUpTDSMessage = false;
        if ($scope.form1.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "accounts/Invoice/InsertVendorInvoice",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.voucherDetailList,
                        "taxDetailVMList": $scope.advanceTaxesList,
                        "tdsVMList": $scope.TDSList,
                        "invoiceDetailChargesList": $scope.invoiceDetailChargesList,
                        "existingLoanList": $scope.ExistingLoanList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.popUpTDSMessage) {

                        }
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
                    url: "accounts/Invoice/UpdateVendorInvoice",
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
        location.href = "accounts/invoice/ReportVendorInvoice?voucherId=" + voucherId;
    };


    $scope.delete = function (invoiceId, voucherId, type, tDSVoucherId, tDSVoucherNo, deletedRemarks) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "invoiceId": invoiceId, "voucherId": voucherId, "type": type, "tDSVoucherId": tDSVoucherId, "tDSVoucherNo": tDSVoucherNo, "deletedRemarks": deletedRemarks
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
                $scope.type = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.deletedRemarks = "";
    $scope.invoiceId = null;
    $scope.confirmDelete = function (invoiceId, voucherId, type, tDSVoucherId, tDSVoucherNo) {
        $scope.invoiceId = invoiceId;
        $scope.voucherId = voucherId;
        $scope.type = type;
        $scope.tDSVoucherId = tDSVoucherId;
        $scope.tDSVoucherNo = tDSVoucherNo;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp_Remarks")).modal("show");
    };

    $scope.closeconfirmDeletePopUp_Remarks = function () {
        angular.element(document.querySelector("#confirmDeletePopUp_Remarks")).modal("hide");
    };

    $scope.clearPartyData = function () {
        $scope.voucher.PartyId = null;
        $scope.voucher.PartyName = null;
        $scope.voucher.PartyPlantId = null;
    }


    $scope.editTaxAssaignVDList = function () {

        for (var i = 0; i < baseService.arrayLength($scope.voucherDetailList); i++) {
            $scope.voucherDetailList[i].InvoiceTaxViewModel = [];
            $scope.taxindex = 0;
            for (var t = 0; t < $scope.taxCodDataList.length; t++) {
                if ($scope.taxCodDataList[t].VoucherDetailId == $scope.voucherDetailList[i].VoucherDetailId) {
                    //$scope.voucherDetailList[i].TotalAmount += $scope.voucherDetailList[i].Amount + $scope.taxCodDataList[t].TaxAmount;
                    //$scope.voucherDetailList[i].TotalTax += $scope.taxCodDataList[t].TaxAmount;
                    $scope.voucherDetailList[i].InvoiceTaxViewModel.push($scope.taxCodDataList[t]);
                    $scope.taxindex++;
                }
            }
        }
    };
    $scope.GetInvoiceGLBudgetActivityDetail = function (id, invoiceId) {
        $http({
            method: "get",
            url: "accounts/invoice/GetInvoiceGLBudgetActivityDetail?voucherId=" + id
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
            $scope.GetInvoiceTaxDetail(invoiceId);
        });
    };
    $scope.GetInvoiceTaxDetail = function (id) {
        $http({
            method: "get",
            url: "accounts/invoice/GetInvoiceTaxDetail?invoiceId=" + id
        }).then(function successCallback(response) {
            $scope.taxCodDataList = response.data;
            $scope.editTaxAssaignVDList();
        });
    };

    //InvoiceTaxViewModel

    $scope.Get = function (index) {
        $scope.index = index;
        $scope.voucher = $scope.invoiceList[$scope.index];
        $scope.voucher.PostingDate = $filter("dateFiltering")($scope.invoiceList[$scope.index].PostingDate);
        $scope.voucher.DocDate = $filter("dateFiltering")($scope.invoiceList[$scope.index].DocDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.getPartyPlantList($scope.voucher.PartyId);
        $scope.GetInvoiceGLBudgetActivityDetail($scope.voucher.VoucherId, $scope.voucher.Id);
        // $scope.GetInvoiceTaxDetail($scope.voucher.Id);
    }





    $scope.ChangecustomerInvoiceGLSearchList = [
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
    $scope.ChangecustomerInvoiceGLParameters = {
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
    $scope.ChangeGLpopUp = function (rowdata, index) {
        $scope.customerInvoiceGLList = [];
        baseService.setCurrentPage("ChangecustomerInvoiceGLList");
        $scope.ChangecustomerInvoiceGLGLData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetVendorInvoiceGLBudgetList", pageno, $scope.ChangecustomerInvoiceGLParameters)
                .then(function (result) {
                    $scope.ChangecustomerInvoiceGLList = result.Rows;
                    $scope.ChangecustomerInvoiceGLParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "ChangeInvoiceGLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#ChangeInvoiceGLPopUp")).modal("show");
        $scope.rowindex = index;
        $scope.ChangecustomerInvoiceGLGLData();
    };
    $scope.ChangeglSelect = function (data) {
        $scope.voucherDetailList[$scope.rowindex].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.voucherDetailList[$scope.rowindex].GLGeneralInfoName = data.GLGeneralInfoName;
        $scope.voucherDetailList[$scope.rowindex].GLGeneralInfoCode = data.GLGeneralInfoCode;
        $scope.voucherDetailList[$scope.rowindex].BudgetMasterId = data.BudgetMasterId;
        $scope.voucherDetailList[$scope.rowindex].BudgetName = data.BudgetName;
        $scope.voucherDetailList[$scope.rowindex].BudgetCode = data.BudgetCode;
        $scope.voucherDetailList[$scope.rowindex].ActivityId = data.ActivityId;
        $scope.voucherDetailList[$scope.rowindex].ActivityName = data.ActivityName;
        $scope.voucherDetailList[$scope.rowindex].ActivityCode = data.ActivityCode;
        $scope.voucherDetailList[$scope.rowindex].IsOrderSpecific = data.IsOrderSpecific;
        $scope.voucherDetailList[$scope.rowindex].ActivityOrderType = data.ActivityOrderType;
        $scope.ChangecloseGLPopUp();
    };
    $scope.ChangecloseGLPopUp = function () {
        angular.element(document.querySelector("#ChangeInvoiceGLPopUp")).modal("hide");
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
        $scope.TDS.TaxCategoryId = $.grep($scope.TDSCboList, function (item) {
            return item.Id === $scope.TDS.TaxCodeId;
        })[0].TaxCategoryId;

        if ($scope.TDS.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.TDS.ValueOfFixed)) {
            $scope.TDS.TaxAmount = Math.round((($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount") * $scope.TDS.ValueOfFixed / 100)) * 10000 + Number.EPSILON) / 10000;
        }
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

    $scope.VendorInvoiceReport = function (reportFormat, voucherId, beneficiaryType) {
        if (beneficiaryType == 'Vendor') {
            $window.open('Accounts/Invoice/ReportVendorInvoice?reportFormat=' + reportFormat + '&voucherId=' + voucherId, '_blank');
        }
        else
            $window.open('Employees/EmployeeReport/GetEmployeePayableExpenseReport?reportFormat=' + reportFormat + '&voucherId=' + voucherId, '_blank');

    }
    $scope.ExpenseDistributionReport = function (reportFormat, voucherId) {
        $window.open('Accounts/Invoice/ReportVendorInvoiceExpenseDistribution?reportFormat=' + reportFormat + '&voucherId=' + voucherId, '_blank');
    }
    $scope.voucherTypeListnew = [];
    $scope.additionalTaxVoucherTypeId = null;
    $scope.getPaymentVoucherType = function () {
        cboService.getCboVoucherTypePaymentList(function (result) {
            $scope.voucherTypeListnew = result;
            if (baseService.arrayLength($scope.voucherTypeListnew) === 1)
                $scope.additionalTaxVoucherTypeId = $scope.voucherTypeListnew[0].Value;
        });
    }
    $scope.getPaymentVoucherType();

    $scope.additionalTaxDetailList = [];
    $scope.additionalTaxData = {};
    $scope.onClickadditionalTaxPop = function (x) {
        $scope.additionalTaxData = {};
        var data = x;
        data.VoucherDate = new Date();
        $scope.additionalTaxData = data;
        $http({
            method: 'POST',
            url: 'Accounts/InventoryPayable/GetAdditionalTaxDetail?additionalTaxId=' + data.AdditionalTaxId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.additionalTaxDetailList = response.data;
        });

        angular.element(document.querySelector('#additionalTaxPopUp')).modal('show');
    };

    $scope.additionalTaxId = null;
    //$scope.confirmTDSPost = function (additionalTaxId, data) {
    //    $scope.additionalTaxId = additionalTaxId;
    //    $scope.additionalTaxData = data;
    //    $scope.message_confirmation = "Are you sure to Post?";
    //    angular.element(document.querySelector("#confirmTDSPostPopUp")).modal("show");
    //};

    //$scope.confirmAutoTDSPost = function (additionalTaxId, data) {
    //    $scope.additionalTaxId = additionalTaxId;
    //    $scope.additionalTaxData = data;
    //    $scope.message_TDSAuto_confirmation = "Please  Post TDS ?";
    //    angular.element(document.querySelector("#confirmAutoTDSPostPopUp")).modal("show");
    //};

    $scope.postAdditionalTax = function () {
        if (baseService.isUndefinedOrNull($scope.additionalTaxVoucherTypeId))
            ShowResult('Please select VoucherType', 'failure', 'additionalTaxPopUp');

        $scope.additionalTaxData.VoucherTypeId = $scope.additionalTaxVoucherTypeId;
        if ($scope.additionalTaxData != null && !baseService.isUndefinedOrNull($scope.additionalTaxVoucherTypeId)) {
            $http({
                method: 'POST',
                url: 'Accounts/Invoice/InsertAdditionalTaxPayable',
                data: {
                    "additionalTaxId": $scope.additionalTaxData.AdditionalTaxId
                    , "voucherVM": $scope.additionalTaxData
                },
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.Clear();
                    $scope.additionalTaxId = null;
                    $scope.additionalTaxData = null;
                    //$scope.tdsId = null;
                    //$scope.data = null;
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
            angular.element(document.querySelector('#additionalTaxPopUp')).modal('hide');
        }

    }

    $scope.TDSVouchereReport = function (reportFormat, voucherId) {
            $window.open('Accounts/Invoice/VendorInvoicePaymentReport?reportFormat=' + reportFormat + '&voucherId=' + voucherId, '_blank');
    }
    $scope.additionalTaxPrint = function () {
        try {
            var file_src = 'Accounts/invoice/VendorInvoicePaymentReport?reportFormat=' + 'Excel' + '&voucherId=' + $scope.additionalTaxData.TDSTaxVoucherId
            $rootScope.report(file_src);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.changeBefeficiary = function () {
        $scope.TDSList = [];
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

    $scope.getFiscalInvoiceTotalAmountByParty = function (partyId,postingDate) {
        $scope.fiscalinvoiceAmountByParty = [];
        $http({
            method: "GET",
            url: "accounts/invoice/GetFiscalInvoiceTotalAmountByParty?partyId=" + partyId + '&postingDate=' + postingDate
        }).then(function successCallback(response) {
            $scope.fiscalinvoiceAmountByParty = response.data;
            $scope.TotalAdvanceAmount = Math.round($filter("sumByKey")($filter("filter")($scope.fiscalinvoiceAmountByParty), "BooksInvoiceAmount") * 10000 + Number.EPSILON) / 10000;
            //if ($scope.fiscalinvoiceAmountByParty.length > 0) {
            //    angular.element(document.querySelector("#partyfiscalInvoiceAmountPopUp")).modal("show");
            //}
        });
    };
    $scope.showFiscalInvoiceAmountByParty = function () {
        if ($scope.fiscalinvoiceAmountByParty.length > 0) {
            angular.element(document.querySelector("#partyfiscalInvoiceAmountPopUp")).modal("show");
        }
    }
    $scope.closeFiscalInvoiceTotalAmountByParty = function () {
        angular.element(document.querySelector('#partyfiscalInvoiceAmountPopUp')).modal('hide');

    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.checkedInvoiceList = [];
    $scope.VendorAvailableInvoiceList = [];
    $scope.searchByVendor = "UserName"; $scope.searchVendor = "";
    $scope.searchByVendorList = [{ value: 'VoucherNo', name: "VoucherNo" }, { value: 'EntityName', name: "Entity" }, { value: 'PartyPlantName', name: "Party" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'DocDate', name: "DocDate" }, { value: 'PostingDate', name: "Invoice Date" }, { value: 'DocRefNo', name: "Invoice No" }];
    $scope.showInvoicePopUp = function () {
        $http({
            method: 'POST',
            url: 'accounts/Invoice/GetVendorAllInvoiceList',
            data: { column: $scope.searchByVendor, value: $scope.searchVendor },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VendorAvailableInvoiceList = response.data;

            if (baseService.arrayLength($scope.checkedInvoiceList) > 0) {
                for (var i = 0; i < baseService.arrayLength($scope.checkedInvoiceList); i++) {
                    for (var j = 0; j < baseService.arrayLength($scope.VendorAvailableInvoiceList); j++) {
                        if ($scope.checkedInvoiceList[i].InvoiceId == $scope.VendorAvailableInvoiceList[j].InvoiceId) {
                            $scope.VendorAvailableInvoiceList[j].Active = true;
                        }
                    }
                }
            }
        });

        angular.element(document.querySelector('#InboundInvoicePopUp')).modal('show');

    };
    function checkLCExist(list, InvoiceId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].InvoiceId === InvoiceId) {

                return true;
            }
        }
        return false;
    }
    $scope.hideInvoicePopUp = function () {
        angular.element(document.querySelector("#InboundInvoicePopUp")).modal("hide");
    };
    $scope.checkedOutBoundInvoiceList = [];
    $scope.CustomerAvailableInvoiceList = [];
    $scope.searchByCustomer = "VoucherNo"; $scope.searchCustomer = "";
    $scope.searchByCustomerList = [{ value: 'VoucherNo', name: "VoucherNo" }, { value: 'EntityName', name: "Entity" }, { value: 'PartyPlantName', name: "Party" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'DocDate', name: "DocDate" }, { value: 'PostingDate', name: "Invoice Date" }, { value: 'DocRefNo', name: "Invoice No" }, { value: 'SalesNo', name: "Sales No" }];

    $scope.showOutBoundInvoicePopUp = function () {
        try {
            $http({
                method: 'POST',
                url: 'accounts/CustomerInvoice/GetCustomerAllReceivableData',
                data: { column: $scope.searchByCustomer, value: $scope.searchCustomer },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.CustomerAvailableInvoiceList = response.data;
                if (baseService.arrayLength($scope.checkedOutBoundInvoiceList) > 0) {
                    for (var i = 0; i < baseService.arrayLength($scope.checkedOutBoundInvoiceList); i++) {
                        for (var j = 0; j < baseService.arrayLength($scope.CustomerAvailableInvoiceList); j++) {
                            if ($scope.checkedOutBoundInvoiceList[i].InvoiceId == $scope.CustomerAvailableInvoiceList[j].InvoiceId) {
                                $scope.CustomerAvailableInvoiceList[j].Active = true;
                            }
                        }
                    }
                }
            });
        } catch (e) {
            throw e;
        }
        angular.element(document.querySelector('#OutBoundInvoicePopUp')).modal('show');
    };
    $scope.hideOutBoundInvoicePopUp = function () {
        angular.element(document.querySelector("#OutBoundInvoicePopUp")).modal("hide");
    };
    $scope.ShowResultMasterOrderPopUp = function () {
        $scope.GetMasterOrderList();
        angular.element(document.querySelector('#masterOrderPopUp')).modal('show');
    }
    $scope.masterOrderList = [];
    $scope.GetMasterOrderList = function () {
        $scope.masterOrderList = [];
        $http({
            method: 'GET',
            url: "accounts/CustomerInvoice/GetMasterOrderPopUp"
        }).then(function (response) {
            $scope.masterOrderList = response.data;
        });
    }
    $scope.CloseMasterOrder = function () {
        angular.element(document.querySelector('#masterOrderPopUp')).modal('hide');      
    }
    $scope.ShowResultContractPopUp = function () {
        $scope.getcontractList();
        angular.element(document.querySelector('#contractPopUp')).modal('show');
    }
    $scope.contractList = [];
    $scope.getcontractList = function () {
        $scope.contractList = [];
        $http.get("Commercial/Contract/getlist")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.contractList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.CloseContract = function () {
        angular.element(document.querySelector('#contractPopUp')).modal('hide');
    }
    $scope.TotalInvoiceAmount = 0;
    $scope.getTotalInvoiceAmount = function () {
        $scope.TotalInvoiceAmount = 0;
        if ($scope.activityOrderType == "InboundInvoice") {
            if (baseService.arrayLength($scope.checkedInvoiceList) > 0)
               $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        }
        else if ($scope.activityOrderType == "OutboundInvoice") {
            if (baseService.arrayLength($scope.checkedOutBoundInvoiceList))
               $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        }
        else if ($scope.activityOrderType == "BothInOutboundInvoice") {
            if (baseService.arrayLength($scope.checkedInvoiceList) > 0)
                $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
            if (baseService.arrayLength($scope.checkedOutBoundInvoiceList))
                $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount")); 
        }
        
    }

    $scope.TotalInvoiceQty = 0;
    $scope.getTotalInvoiceQty = function () {
        $scope.TotalInvoiceQty = 0;
        if ($scope.activityOrderType == "InboundInvoice") {
            if (baseService.arrayLength($scope.checkedInvoiceList) > 0)
                $scope.TotalInvoiceQty += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "Qty"));
        }
        else if ($scope.activityOrderType == "OutboundInvoice") {
            if (baseService.arrayLength($scope.checkedOutBoundInvoiceList))
                $scope.TotalInvoiceQty += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "Qty"));
        }
        else if ($scope.activityOrderType == "BothInOutboundInvoice") {
            if (baseService.arrayLength($scope.checkedInvoiceList) > 0)
                $scope.TotalInvoiceQty += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "Qty"));
            if (baseService.arrayLength($scope.checkedOutBoundInvoiceList))
                $scope.TotalInvoiceQty += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "Qty"));
        }

    }

    $scope.TotalChargesAmount = 0;
    $scope.calDistributedAmount = function myfunction() {
        $scope.getTotalInvoiceAmount();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedAmountInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        var totali = parseFloat(($scope.TotalDistributedAmountInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            $scope.checkedInvoiceList[i].DistributedAmount = 0;
        }

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            if ($scope.checkedInvoiceList.length == 1) {
                $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
            }
            else {
                if ($scope.checkedInvoiceList.length - 1 == i) {

                    $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                }
                else {
                    $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                }
            }
        }
       
    }
    $scope.calDistributedQtyWise = function myfunction() {
        $scope.getTotalInvoiceQty();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedQtyInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "Qty"));
        var totali = parseFloat(($scope.TotalDistributedQtyInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceQty);

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            $scope.checkedInvoiceList[i].DistributedAmount = 0;
        }

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            if ($scope.checkedInvoiceList.length == 1) {
                $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].Qty) * $scope.TotalChargesAmount / $scope.TotalInvoiceQty).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
            }
            else {
                if ($scope.checkedInvoiceList.length - 1 == i) {

                    $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                }
                else {
                    $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].Qty) * $scope.TotalChargesAmount / $scope.TotalInvoiceQty).toFixed(2);
                }
            }
        }

    }
    $scope.calOutBoundDistributedAmount = function myfunction() {
        //$scope.TotalChargesAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount"));
        $scope.getTotalInvoiceAmount();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedAmountout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        var tatalout = parseFloat(($scope.TotalDistributedAmountout * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
        }

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

            if ($scope.checkedOutBoundInvoiceList.length == 1) {
                $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
            }
            else {
                if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                    $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (tatalout - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                }
                else {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                }
            }
        }
       
    }
    $scope.calOutBoundDistributedQtyWise = function myfunction() {
        $scope.getTotalInvoiceQty();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedQtyout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "Qty"));
        var totaloutQty = parseFloat(($scope.TotalDistributedQtyout * $scope.TotalChargesAmount) / $scope.TotalInvoiceQty);

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
        }

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

            if ($scope.checkedOutBoundInvoiceList.length == 1) {
                $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].Qty) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceQty).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
            }
            else {
                if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                    $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (totaloutQty - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                }
                else {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].Qty) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceQty).toFixed(2);
                }
            }
        }

    }
    $scope.calMasterOrderDistributedAmount = function myfunction() {
        //$scope.TotalChargesAmount = 0;
        for (var i = 0; i < $scope.checkedMasterOrderList.length; i++) {
            $scope.checkedMasterOrderList[i].DistributedAmount = $scope.TotalChargesAmount;
         
        }

    }
    $scope.calContractDistributedAmount = function myfunction() {
        //$scope.TotalChargesAmount = 0;
        for (var i = 0; i < $scope.checkedContractList.length; i++) {
            $scope.checkedContractList[i].DistributedAmount = $scope.TotalChargesAmount;

        }

    }
    $scope.calReDistributedAmount = function myfunction(index, item) {
        $scope.TotalChargesAmount = parseFloat($scope.voucherDetailList[index].Amount);
        $scope.activityOrderType = "";
        $scope.activityOrderType = item.ActivityOrderType;
        if ($scope.activityOrderType == "InboundInvoice") {
            if (item.ValueOfDistribution == "Amount") {
                $scope.getTotalInvoiceAmount();
                $scope.TotalDistributedInvoiceAmount = 0;
                $scope.TotalDistributedAmountInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
                var totali = parseFloat(($scope.TotalDistributedAmountInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    $scope.checkedInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    if ($scope.checkedInvoiceList.length == 1) {
                        $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    }
                    else {
                        if ($scope.checkedInvoiceList.length - 1 == i) {

                            $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                            $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                        }
                    }
                }
            }
            else {
                $scope.getTotalInvoiceQty();
                $scope.TotalDistributedInvoiceAmount = 0;

                $scope.TotalDistributedQtyInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "Qty"));
                var totali = parseFloat(($scope.TotalDistributedQtyInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceQty);

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    $scope.checkedInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    if ($scope.checkedInvoiceList.length == 1) {
                        $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].Qty) * $scope.TotalChargesAmount / $scope.TotalInvoiceQty).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    }
                    else {
                        if ($scope.checkedInvoiceList.length - 1 == i) {

                            $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                            $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].Qty) * $scope.TotalChargesAmount / $scope.TotalInvoiceQty).toFixed(2);
                        }
                    }
                }
            }
        }
        else if ($scope.activityOrderType == "OutboundInvoice") {
            if (item.ValueOfDistribution == "Amount") {
                $scope.getTotalInvoiceAmount();
                $scope.TotalDistributedInvoiceAmount = 0;

                $scope.TotalDistributedAmountout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
                var tatalout = parseFloat(($scope.TotalDistributedAmountout * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

                    if ($scope.checkedOutBoundInvoiceList.length == 1) {
                        $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
                    }
                    else {
                        if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                            $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (tatalout - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                        }
                    }
                }
            }
            else {
                $scope.getTotalInvoiceQty();
                $scope.TotalDistributedInvoiceAmount = 0;

                $scope.TotalDistributedQtyout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "Qty"));
                var totaloutQty = parseFloat(($scope.TotalDistributedQtyout * $scope.TotalChargesAmount) / $scope.TotalInvoiceQty);

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

                    if ($scope.checkedOutBoundInvoiceList.length == 1) {
                        $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].Qty) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceQty).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
                    }
                    else {
                        if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                            $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (totaloutQty - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].Qty) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceQty).toFixed(2);
                        }
                    }
                }
            }
        }
        else if ($scope.activityOrderType == "BothInOutboundInvoice") {
            if (item.ValueOfDistribution == "Amount") {
                $scope.getTotalInvoiceAmount();
                $scope.TotalDistributedInvoiceAmount = 0;

                $scope.TotalDistributedAmountInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
                var totali = parseFloat(($scope.TotalDistributedAmountInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    $scope.checkedInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    if ($scope.checkedInvoiceList.length == 1) {
                        $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    }
                    else {
                        if ($scope.checkedInvoiceList.length - 1 == i) {

                            $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                            $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                        }
                    }
                }

                $scope.TotalDistributedAmountout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
                var tatalout = parseFloat(($scope.TotalDistributedAmountout * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

                    if ($scope.checkedOutBoundInvoiceList.length == 1) {
                        $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
                    }
                    else {
                        if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                            $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (tatalout - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                        }
                    }
                }
            }
            else {
                $scope.getTotalInvoiceQty();
                $scope.TotalDistributedInvoiceAmount = 0;

                $scope.TotalDistributedQtyInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "Qty"));
                var totali = parseFloat(($scope.TotalDistributedQtyInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceQty);

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    $scope.checkedInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    if ($scope.checkedInvoiceList.length == 1) {
                        $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].Qty) * $scope.TotalChargesAmount / $scope.TotalInvoiceQty).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    }
                    else {
                        if ($scope.checkedInvoiceList.length - 1 == i) {

                            $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                            $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].Qty) * $scope.TotalChargesAmount / $scope.TotalInvoiceQty).toFixed(2);
                        }
                    }
                }

                $scope.TotalDistributedQtyout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "Qty"));
                var totaloutQty = parseFloat(($scope.TotalDistributedQtyout * $scope.TotalChargesAmount) / $scope.TotalInvoiceQty);

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

                    if ($scope.checkedOutBoundInvoiceList.length == 1) {
                        $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].Qty) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceQty).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
                    }
                    else {
                        if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                            $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (totaloutQty - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].Qty) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceQty).toFixed(2);
                        }
                    }
                }
            }
        }
        else if ($scope.activityOrderType == "Order") {
            for (var i = 0; i < $scope.checkedMasterOrderList.length; i++) {
                $scope.checkedMasterOrderList[i].DistributedAmount = $scope.TotalChargesAmount;

            }
        }
        else if ($scope.activityOrderType == "Contract") {
            for (var i = 0; i < $scope.checkedContractList.length; i++) {
                $scope.checkedContractList[i].DistributedAmount = $scope.TotalChargesAmount;

            } 
        }

    }

    $scope.totalBooksAmount = 0;
    $scope.totalDistributedAmount = 0;
    $scope.InBoundInvoiceAmount = 0; $scope.OutBoundInvoiceAmount = 0;
    $scope.InBoundDistributed = 0; $scope.OutBoundDistributed = 0;
    $scope.totalBooksAmountCal = function () {
       
        $scope.InBoundInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        $scope.OutBoundInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        $scope.totalBooksAmount = parseFloat($scope.InBoundInvoiceAmount + $scope.OutBoundInvoiceAmount)

        $scope.InBoundDistributed = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
        $scope.OutBoundDistributed = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount"));
        $scope.totalDistributedAmount = parseFloat($scope.InBoundDistributed + $scope.OutBoundDistributed)
    }
    $scope.AddInvoice = function () {

        if (baseService.arrayLength($scope.VendorAvailableInvoiceList) > 0) {
            $scope.checkedInvoiceList = [];
            angular.forEach($scope.VendorAvailableInvoiceList, function (a) {
                    if (a.Active) {
                        $scope.checkedInvoiceList.push({
                            InvoiceId: a.InvoiceId
                            , InvoiceDetailId: a.InvoiceDetailId
                            , Amount: a.Receivable
                            , Qty: a.TrnQty
                            , BooksAmount: a.Receivable * a.CompanyCurrencyRate
                            , DistributedAmount: 0
                            , ChargesAmount: 0
                            , TaxAmount: 0
                            , Active: true
                            , PostingDate: a.PostingDate
                            , PartyPlantName: a.PartyPlantName
                            , CurrencyCode: a.CurrencyCode
                            , VoucherNo: a.VoucherNo
                            , InvoiceType: 'InboundInvoice'
                            , GLGeneralInfoId: $scope.GLGeneralInfoId
                            , BudgetMasterId: $scope.BudgetMasterId
                            , ActivityId: $scope.ActivityId
                            , DocRefNo: a.DocRefNo
                        });
                    }
            });
        }

        $scope.hideInvoicePopUp();
        if ($scope.ValueOfDistribution == 'Amount') {
            $scope.calDistributedAmount();
            $scope.calOutBoundDistributedAmount();
        }
        else
        {
            $scope.calDistributedQtyWise();
            $scope.calOutBoundDistributedQtyWise();
        }
        $scope.totalBooksAmountCal();
    };
    $scope.checkedOutBoundInvoiceList = [];
    $scope.AddIOutBoundInvoice = function () {
        if (baseService.arrayLength($scope.CustomerAvailableInvoiceList) > 0) {
            angular.forEach($scope.CustomerAvailableInvoiceList, function (a) {
                if (checkLCExist($scope.checkedOutBoundInvoiceList, a.InvoiceId) === false) {
                    if (a.Active) {
                        $scope.checkedOutBoundInvoiceList.push({
                            InvoiceId: a.InvoiceId
                            , InvoiceDetailId: a.InvoiceDetailId
                            , Amount: a.Receivable
                            , Qty: a.TrnQty
                            , BooksAmount: a.Receivable * a.CompanyCurrencyRate
                            , DistributedAmount: 0
                            , ChargesAmount: 0
                            , TaxAmount: 0
                            , Active: true
                            , PostingDate: a.PostingDate
                            , PartyPlantName: a.PartyPlantName
                            , CurrencyCode: a.CurrencyCode
                            , VoucherNo: a.VoucherNo
                            , InvoiceType: 'OutboundInvoice'
                            , GLGeneralInfoId: $scope.GLGeneralInfoId
                            , BudgetMasterId: $scope.BudgetMasterId
                            , ActivityId: $scope.ActivityId
                            , DocRefNo: a.DocRefNo
                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.checkedOutBoundInvoiceList, function (a) {
                if (!baseService.valueCheckInList($scope.checkedOutBoundInvoiceList, 'Id', a.InvoiceId))
                    $scope.checkedOutBoundInvoiceList.splice(a, 1);
            });
        $scope.hideOutBoundInvoicePopUp();
        if ($scope.ValueOfDistribution == 'Amount') {
            $scope.calDistributedAmount();
            $scope.calOutBoundDistributedAmount();
        }
        else {
            $scope.calDistributedQtyWise();
            $scope.calOutBoundDistributedQtyWise();
        }
        
        $scope.totalBooksAmountCal();
    };
    $scope.checkedMasterOrderList = [];
    $scope.AddOrder = function (x) {
        var a = x.rowData;
        if (baseService.arrayLength($scope.masterOrderList) > 0) {
            $scope.checkedMasterOrderList = [];
                    $scope.checkedMasterOrderList.push({
                        InvoiceId: null
                        , InvoiceDetailId: null
                        , Amount: 0
                        , BooksAmount: 0
                        , DistributedAmount: 0
                        , ChargesAmount: 0
                        , TaxAmount: 0
                        , Active: true
                        , PostingDate: ""
                        , PartyPlantName: a.InvoicingPartyPlant
                        , CurrencyCode: ""
                        , VoucherNo: ""
                        , InvoiceType: 'Order'
                        , GLGeneralInfoId: $scope.GLGeneralInfoId
                        , BudgetMasterId: $scope.BudgetMasterId
                        , ActivityId: $scope.ActivityId
                        , MasterOrderId: a.MasterOrderId
                        , ContractId: null
                        , CustomerName: a.CustomerName
                        , InvoicingPartyPlant: a.InvoicingPartyPlant
                        , DeliveryPartyPlant: a.DeliveryPartyPlant
                        , Type: a.Type
                    });
                }
            
      

        $scope.CloseMasterOrder();
        $scope.calMasterOrderDistributedAmount();
       
    };
    $scope.checkedContractList = [];
    $scope.AddContract = function (x) {
        var a = x.rowData;
        if (baseService.arrayLength($scope.contractList) > 0) {
            $scope.checkedContractList = [];
            $scope.checkedContractList.push({
                InvoiceId: null
                , InvoiceDetailId: null
                , Amount: 0
                , BooksAmount: 0
                , DistributedAmount: 0
                , ChargesAmount: 0
                , TaxAmount: 0
                , Active: true
                , PostingDate: ""
                , PartyPlantName: ""
                , CurrencyCode: ""
                , VoucherNo: ""
                , InvoiceType: 'Contract'
                , GLGeneralInfoId: $scope.GLGeneralInfoId
                , BudgetMasterId: $scope.BudgetMasterId
                , ActivityId: $scope.ActivityId
                , MasterOrderId: null
                , ContractId: a.Id
                , ContractNo: a.ContractNo
                , UDNo: a.UDNo
                , CustomerName: a.CustomerName
                , Buyer: a.Buyer
                , Remarks: a.Remarks
            });
        }



        $scope.CloseContract();
        $scope.calContractDistributedAmount();

    };

    $scope.DeleteConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteConfirmationPopUp")).modal("show");
    };
    $scope.RemoveInvoice = function () {

        for (var i = 0; i < baseService.arrayLength($scope.checkedInvoiceList); i++) {
            if ($scope.checkedInvoiceList[i].InvoiceId == $scope.InvoiceId)
                $scope.checkedInvoiceList.splice(i, 1);
        }

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            $scope.checkedInvoiceList[i].DistributedAmount = 0;
        }
        if ($scope.ValueOfDistribution == 'Amount') {
            $scope.calDistributedAmount();
            $scope.calOutBoundDistributedAmount();
        }
        else {
            $scope.calDistributedQtyWise();
            $scope.calOutBoundDistributedQtyWise();
        }
        $scope.totalBooksAmountCal();

    }
    $scope.DeleteOutBoutConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteOutBoundConfirmationPopUp")).modal("show");
    };
    $scope.RemoveOutBoundInvoice = function () {

        for (var i = 0; i < baseService.arrayLength($scope.checkedOutBoundInvoiceList); i++) {
            if ($scope.checkedOutBoundInvoiceList[i].InvoiceId == $scope.InvoiceId)
                $scope.checkedOutBoundInvoiceList.splice(i, 1);
        }
        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
        }
        if ($scope.ValueOfDistribution == 'Amount') {
            $scope.calDistributedAmount();
            $scope.calOutBoundDistributedAmount();
        }
        else {
            $scope.calDistributedQtyWise();
            $scope.calOutBoundDistributedQtyWise();
        }
        $scope.totalBooksAmountCal();
    }
    $scope.DeleteMasterOrderConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteMasterOrderConfirmationPopUp")).modal("show");
    };
    $scope.RemoveMasterOrderInvoice = function () {
        for (var i = 0; i < baseService.arrayLength($scope.checkedMasterOrderList); i++) {
            if ($scope.checkedMasterOrderList[i].MasterOrderId == $scope.InvoiceId)
                $scope.checkedMasterOrderList.splice(i, 1);
        }
       
        $scope.calMasterOrderDistributedAmount();
    }
    $scope.DeleteContractConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteContractConfirmationPopUp")).modal("show");
    };
    $scope.RemoveContractInvoice = function () {
        for (var i = 0; i < baseService.arrayLength($scope.checkedContractList); i++) {
            if ($scope.checkedContractList[i].ContractId == $scope.InvoiceId)
                $scope.checkedContractList.splice(i, 1);
        }

        $scope.calContractDistributedAmount();
    }
    $scope.checkDistributedAmount = function myfunction(index, item) {
        if ($scope.activityOrderType == "InboundInvoice") {
            $scope.TotalDistributedAmounts = 0;
            $scope.TotalDistributedAmounts = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
        
            if (parseFloat($scope.TotalDistributedAmounts) > parseFloat($scope.TotalChargesAmount)) {
                $scope.checkedInvoiceList[index].DistributedAmount = 0;
                ShowResult('Distributed Amount must be equal Taxable Amount.!', 'failure', 'ExpenseDistributePopUp');
            }    
        }
        else if ($scope.activityOrderType == "OutboundInvoice") {
            $scope.TotalDistributedAmounts = 0;
            $scope.TotalDistributedAmounts = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount"));

            if (parseFloat($scope.TotalDistributedAmounts) > parseFloat($scope.TotalChargesAmount)) {
                $scope.checkedOutBoundInvoiceList[index].DistributedAmount = 0;
                ShowResult('Distributed Amount must be equal Taxable Amount.!', 'failure', 'ExpenseDistributePopUp');
            }
        }
        else if ($scope.activityOrderType == "BothInOutboundInvoice") {
            $scope.TotalDistributedAmountInBound = 0;
            $scope.TotalDistributedAmountOutBound = 0;
            $scope.TotalDistributedAmountInBound = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
            $scope.TotalDistributedAmountOutBound = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount"));

            if ((parseFloat($scope.TotalDistributedAmountInBound) + parseFloat($scope.TotalDistributedAmountOutBound)) > parseFloat($scope.TotalChargesAmount)) {
                ShowResult('Distributed Amount must be equal Taxable Amount.!', 'failure', 'ExpenseDistributePopUp');
            }
        }
    }
    $scope.purchaseLCList = [];
    $scope.getpurchaseLCListData = function () {
        $scope.purchaseLCList = [];
        $http.get("Commercial/InvoiceTaggedWithLC/getpurchaseLCList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {

                        $scope.purchaseLCList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getpurchaseLCListData();

    $scope.getPurchaseLCData = function () {
        angular.element(document.querySelector("#PurchaseLCPopUp")).modal("show");
    }
    var LCVendorId = "";
    $scope.SetPurchaseLCDetails = function (args) {
        $scope.voucher.LCRef = args.data.LCRef;
        $scope.voucher.PurchaseLCId = args.data.Id;
        LCVendorId = args.data.VendorId;
        angular.element(document.querySelector("#PurchaseLCPopUp")).modal("hide");
    }
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
        if (baseService.isUndefinedOrNull($scope.voucher.PurchaseLCId)) {
            ShowResult("Please Select LC !", "failure", "loanPopUp");
            return;
        }
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
            //$scope.getPartyPlantList(data.PartyId);
            $scope.existingLoan.PartyPlantId = data.PartyPlantId;

            $scope.ExistingLoanList.push($scope.existingLoan);
            $scope.existingLoan = {};

            $scope.voucher.IsLoanSetOff = true;
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
    $scope.exchangeGainLossAmountExistingLoan = function (data) {
        var balance = parseFloat(data.Balance), dramount = parseFloat(data.LoanSetOffAmount);
        if (dramount > balance) {
            data.LoanSetOffAmount = data.Balance;
            ShowResult("Loan SetOff Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        if (data.CurrencyId === $scope.voucher.CurrencyId)
        {
            if (data.ToCurrencyRate < $scope.voucher.CompanyCurrencyRate) {
                data.ConversionAmount = Math.abs(data.LoanSetOffAmount / data.ToCurrencyRate).toFixed(2);
                data.ExchangeAmount = Math.abs(data.ConversionAmount * ($scope.voucher.CompanyCurrencyRate - data.ToCurrencyRate)).toFixed(2);
                data.ExchangeType = "ExchangeLoss";
            }
            else if (data.ToCurrencyRate > $scope.voucher.CompanyCurrencyRate) {
                data.ConversionAmount = Math.abs(data.LoanSetOffAmount / data.ToCurrencyRate).toFixed(2);
                data.ExchangeAmount = Math.abs(data.ConversionAmount * (data.ToCurrencyRate - $scope.voucher.CompanyCurrencyRate)).toFixed(2);
                data.ExchangeType = "ExchangeGain";
            }
            else {
                data.ExchangeAmount = 0;
                data.ExchangeType = null;
                data.ConversionAmount = Math.abs(data.LoanSetOffAmount / data.ToCurrencyRate).toFixed(2);
            }
        }
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
        //$scope.calBaseAmount();
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

    $scope.clearSourceInfo = function () {
        $scope.voucher.BankMasterId = null;
        $scope.voucher.BankCurrencyId = null;
        $scope.voucher.BankAmount = 0;
        $scope.voucher.CashMasterId = null;
        $scope.voucher.PurchaseLCId = null;
        $scope.ExistingLoanList = [];
        $scope.existingLoan = {};
        $scope.voucherDetailList = [];
    }

    function containsSpecialChars(str) {
        const specialChars = /[@!#$%^&*()_+\=\[\]{};':"|,.<>\?`~]/;
        return specialChars.test(str);
    }

    $scope.CheckSpecialCharecter = function () {
        try {
            if (containsSpecialChars($scope.voucher.DocRefNo)) {
                $scope.voucher.DocRefNo = $scope.voucher.DocRefNo.substring(0, $scope.voucher.DocRefNo.length - 1);
                throw "No special characters allowed for Doc Ref No.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.postVoucher = function () {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "voucher": $scope.voucherdb,
                "invoiceId": $scope.voucherdb.Id,
                "voucherDetailList": $scope.newJVList,
                "type": $scope.Type
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                if ($scope.tdsId != null) {
                    //$scope.confirmAutoTDSPost(tdsId, data);
                    $scope.onClickadditionalTaxPop($scope.voucherdb);
                }
                $scope.getData();
                $scope.Clear();
                $scope.invoiceId = null;
                $scope.type = null;
                angular.element(document.querySelector('#JournalPopUp')).modal('hide');
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.invoiceId = null;
    //$scope.confirmPost = function (invoiceId, type, tdsId, data) {
       
    //    $scope.invoiceId = invoiceId;
    //    $scope.type = type;
    //    $scope.tdsId = tdsId;
    //    $scope.data = data;

    //    $scope.message_confirmation = "Are you sure to Post?";
    //    angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    //};

    $scope.ShowJournalPopUp = function (id, BeneficiaryType, AdditionalTaxId, data) {
        if (data.ApprovedByStatus == 'ToBeApproved' || data.ApprovedByStatus == 'Hold' || data.ApprovedByStatus == 'Reject') {
            ShowResult("Before Post, Please Approve First. Mr." + data.ApprovedBy + " is responsible for Approve", "failure");
        }
        else {
            $scope.voucherdb = {};
            $scope.voucherdb = data;
            $scope.tdsId = AdditionalTaxId;
            $scope.Type = BeneficiaryType;
            getJournalList(data.VoucherId);
            //if (data.EmployeeId != null) {
            //    $scope.Type = 'Employee';
            //}
            //else {
            //    $scope.Type = 'Vendor';
            //}
            angular.element(document.querySelector('#JournalPopUp')).modal('show');
        }
      
    }

    function getJournalList(voucherId) {
        $http.get('Accounts/Voucher/GetEditableJournalList?voucherId=' + voucherId)
            .then(function (response) {
                $scope.newJVList = [];
                $scope.newJVList = response.data;
            });
    }

    $scope.CloseJournalPopUp = function () {
        angular.element(document.querySelector('#JournalPopUp')).modal('hide');

    }

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
    $scope.indexGL = "";
    $scope.popUpGL = function (index) {
        $scope.indexGL = index;
        baseService.setCurrentPage("cOAICodeList");
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetVendorInvoiceGLBudgetList", pageno, $scope.glListParameters)
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
    $scope.setSelected = function (data, index) {
        $scope.newJVList[$scope.indexGL].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.newJVList[$scope.indexGL].GLGeneralInfoCode = data.GLGeneralInfoCode;
        $scope.newJVList[$scope.indexGL].GLGeneralInfoName = data.GLGeneralInfoName;
        $scope.newJVList[$scope.indexGL].BudgetMasterId = data.BudgetMasterId;
        $scope.newJVList[$scope.indexGL].BudgetName = data.BudgetName;
        $scope.newJVList[$scope.indexGL].ActivityId = data.ActivityId;
        $scope.newJVList[$scope.indexGL].ActivityName = data.ActivityName;
        $scope.closeCOAICodeListPopUp();
    };


    $scope.searchByService = "ServiceName"; $scope.searchService = "";
    $scope.searchByServiceList = [{ value: 'ServiceName', name: "Service" }, { value: 'ServiceType', name: "Service Type" }, { value: 'ServiceGroup', name: "Service Group" }, { value: 'GLCode', name: "GL Code" }
        , { value: 'GL', name: "GL" }, { value: 'Budget', name: "Budget" }, { value: 'Activity', name: "Activity" } ];

    $scope.serviceLists = [];
    $scope.getServiceDataList = function () {
        $http({
            method: 'POST',
            url: 'SetUps/ServiceMaster/GetServicePopUpList',
            data: { column: $scope.searchByService, value: $scope.searchService },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.serviceLists = response.data;
        });
        angular.element(document.querySelector('#ServicePopUp')).modal('show');
    };
    $scope.closeServiceDataPopUp = function () {
        angular.element(document.querySelector("#ServicePopUp")).modal("hide");
    };
}