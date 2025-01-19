vendorAdvanceController.$inject = ["bankService", "cboService", "baseService", "commonMessage", "$scope", "$rootScope", "$http", "$filter", "$controller"];
function vendorAdvanceController(bankService, cboService, baseService, commonMessage, $scope, $rootScope, $http, $filter, $controller) {
    $rootScope.title = "Vendor Advance";
    $scope.Action = "Save";
    $scope.isBankAmount = false;
    $scope.hideSource = true;
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.taxCodCboList = [];
    $scope.index = -1;
    $scope.url = "accounts/Advance";
    $scope.listUrl = $scope.url + "/GetVendorAdvanceList";
    $scope.parkUrl = $scope.url + "/ParkVendorAdvance";
    $scope.updateUrl = $scope.url + "/UpdateVendorAdvance";
    $scope.postUrl = $scope.url + "/PostVendorAdvance";
    $scope.unPostUrl = $scope.url + "/UnPostVendorAdvance";
    $scope.jouranlUrl = $scope.url + "/GetAvailableJournalCustomerAdvance";
    $scope.deleteUrl = $scope.url + "/DeleteVendorAdvance";

    $scope.partyType = "Vendor";
    $scope.partyGLType = "DownPayment";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    baseService.init($scope.listUrl, null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.voucherList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchByList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            "name": "Vendor Code",
            "value": "PartyCode"
        },
        {
            "name": "Vendor Name",
            "value": "PartyName"
        },
        {
            "name": "Invoicing Vendor",
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
        },
        {
            "name": "Entity",
            "value": "EntityName"
        },
        {
            "name": "Currency",
            "value": "Currency"
        },
        {
            "name": "Status",
            "value": "Status"
        }
    ];

    $scope.advance = {
        Id: null,
        CompanyId: null,
        PlantId: null,
        EntityId: null,
        PartyId: null,
        PartyCode: null,
        PartyName: null,
        PartyType: null,
        PartyPlantId: null,
        PartyPlantName: null,
        CurrencyId: null,
        PaymentTermId: null,
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        BankTransactionDate: null,
        BankReferenceNo: null,
        DocDate: null,
        DocRefNo: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        IsExcludingTax: false,
        Amount: null,
        Narration: null,
        BankName: null,
        CashName: null,
        BankMasterId: null,
        CashMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        PaymentSource: "Bank",
        GLGeneralInfoId: null,
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetCode: null,
        BudgetName: null,
        ActivityId: null,
        ActivityCode: null,
        ActivityName: null,
        EmployeeGLGeneralInfoId: null,
        EmployeeGLGeneralInfoName: null,
        EmployeeTransactionTypeId: null,
        FinancingTypeId: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        IsInterTransaction: false,
        CompanyCurrencyRate: 1,
        VoucherId: null,
        BankChargeId: null
    };

    $scope.advanceDetail = {
        Id: null,
        AdvanceId: null,
        PartyId: null,
        PartyCode: null,
        PartyName: null,
        PartyPlantId: null,
        PartyPlantName: null,
        PartyType: null,
        Narration: null,
        GLGeneralInfoId: null,
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetCode: null,
        BudgetName: null,
        ActivityId: null,
        ActivityCode: null,
        ActivityName: null,
        Amount: 0,
        TaxAmount: 0,
        NetAmount: 0
    };

    $scope.advanceDetailList = [];
    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;

    });
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });
    $scope.getBankCharge = function () {
        $scope.advanceChargesList = [];
        $http({
            method: "GET",
            url: "Banks/BankJournal/GetAdvanceBankChargeList?bankChargeId=" + $scope.advance.BankChargeId
        }).then(function successCallback(response) {
            $scope.advanceChargesList = response.data;
        });
    };

    $scope.getTDSPayable = function () {
        $scope.advanceTaxesList = [];
        $http({
            method: "GET",
            url: "Accounts/InvoiceTax/GetTDSPayableList?advanceId=" + $scope.advance.Id
        }).then(function successCallback(response) {
            $scope.advanceTaxesList = response.data;
        });
    };

    $scope.getCompanyCurrencyRate = function (voucherId) {
        $http({
            method: "GET",
            url: "Accounts/Voucher/GetCompanyCurrencyRate?voucherId=" + voucherId
        }).then(function successCallback(response) {
            $scope.advance.CompanyCurrencyRate = response.data;
        });
    };

    $scope.getById = function (id) {
        $scope.advanceDetailList = [];
        $http({
            method: "GET",
            url: "accounts/Advance/GetAdvance/" + id
        }).then(function successCallback(response) {
            $scope.advance = response.data;
            $scope.advance.DocDate = $filter("dateFiltering")($scope.advance.DocDate);
            $scope.advance.VoucherDate = $filter("dateFiltering")($scope.advance.VoucherDate);
            $scope.advance.PostingDate = $filter("dateFiltering")($scope.advance.PostingDate);
            $scope.getPartyPlantList($scope.advance.PartyId);
            $scope.advance.PartyPlantId = $scope.advance.PartyPlantId;
            $scope.getCompanyCurrencyRate($scope.advance.VoucherId);
            $scope.getBankCharge();
            $scope.getTaxCodeByTaxYear($scope.advance.PostingDate);
            $scope.getTDSPayable();
            $scope.Action = "Update";
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };

    $scope.voucher_Post = {
        Id: null,
        EntityId: null,
        CurrencyId: null,
        CurrencyCode: null,
        VoucherNo: null,
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        Narration: null,
        Amount: null
    };

    $scope.advanceId = null;
    $scope.EntityId_Post = null;
    $scope.voucherId = null;
    $scope.confirmPost = function (advanceId, data) {
        $scope.advanceId = advanceId;
        $scope.EntityId_Post = data.EntityId;
        $scope.voucherId = data.VoucherId;
        $scope.voucher_Post = data;
        angular.element(document.querySelector('#PostPopUp')).modal('show');
        //$scope.message_confirmation = "Are you sure to Post?";
        //angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };
    $scope.closePostPopUp = function () {
        angular.element(document.querySelector("#PostPopUp")).modal("hide");
    };
    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.advance.PostingDate) && !baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.advance.PostingDate + "&currencyId=" + $scope.advance.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.advance.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
                $scope.rateChangeBankCharge($scope.advance.CompanyCurrencyRate);
                $scope.updatePartyAmount();
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYear = function (date) {
        $http({
            method: "get",
            url: "accounts/TaxCode/GetTDSCbo?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message;
                }
                else {
                    var result = response.data;
                    $scope.taxCodCboList = result;
                    if ($scope.taxCodCboList.length === 0) {
                        $scope.pop("error", "No TaxCode found in this Fiscal Year ");
                    }
                }
            },
            function errorCallback(response) {
            });
    };


    $scope.getCboVoucherTypeAdvanceGivenList = function () {
        cboService.getCboVoucherTypeAdvanceGivenList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.advance.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.advance.BankTransactionDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.advance.DocDate = $scope.advance.PostingDate;
                $scope.getTaxCodeByTaxYear($scope.advance.PostingDate);
            }
        });
    };
    $scope.getCboVoucherTypeAdvanceGivenList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.advance.VoucherTypeId = data.Value;
        $scope.advance.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.advance.DocDate = $scope.advance.PostingDate;
        $scope.getTaxCodeByTaxYear($scope.advance.PostingDate);
    };
    $scope.changePostingGetTaxCode = function () {
        $scope.advanceTaxesList = [];
        $scope.getTaxCodeByTaxYear($scope.advance.PostingDate);
    }

    $scope.copyAmount = function () {
        var getRow = $filter("filter")($scope.voucherDetailCurrencyList, { "TrnType": "Cr" });
        if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
            if ($scope.advance.BankCurrencyId === $scope.companyCurrencyId) {
                $scope.advance.BankAmount = getRow[0].CompanyCurrencyDr;
            }
            if ($scope.advance.BankCurrencyId === $scope.companyGroupCurrencyId) {
                $scope.advance.BankAmount = getRow[0].CompanyGroupCurrencyDr;
            }
            if ($scope.advance.BankCurrencyId === $scope.hardCurrencyId) {
                $scope.advance.BankAmount = getRow[0].HardCurrencyDr;
            }
        }
    };

    $scope.checkBankAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.advance.BankCurrencyId)) {
            if ($scope.advance.BankCurrencyId !== $scope.advance.CurrencyId) {
                if ($scope.advance.BankCurrencyId !== $scope.companyCurrencyId) {
                    if ($scope.advance.BankCurrencyId !== $scope.companyGroupCurrencyId) {
                        if ($scope.advance.BankCurrencyId !== $scope.hardCurrencyId) {
                            $scope.isBankAmount = true;
                            $scope.advance.BankAmount = 0;
                        }
                    }
                    else {
                        $scope.isBankAmount = false;
                        $scope.advance.BankAmount = 0;
                    }
                }
                else {
                    $scope.isBankAmount = false;
                    $scope.advance.BankAmount = 0;
                }
            }
            else {
                $scope.isBankAmount = false;
                $scope.advance.BankAmount = 0;
            }
        }
        else {
            $scope.isBankAmount = false;
            $scope.advance.BankAmount = 0;
        }
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.advance.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (baseService.isUndefinedOrNull($scope.advance.DocDate)) {
            msg = "Doc Date is required.";
            $scope.invalidDocDate = true;
        }
        else {
            $scope.invalidDocDate = false;
        }
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.advance.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.advance.PostingDate) < new Date($scope.advance.DocDate)) {
            msg = "Posting date must be below or equal to Doc Date!";
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.advance.PostingDate)) {
            msg = "Posting Date is required.";
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return false;
        }

        if ($scope.partyType === "Customer") {
            if ($scope.advance.PartyId === null) {
                ShowResult("Please select Customer!", "failure");
                return true;
            }
            if (parseFloat($scope.advance.Amount) === 0) {
                ShowResult("Advance Amount must greater than 0!", "failure");
                return true;
            }
            if (baseService.isUndefinedOrNull($scope.advance.GLGeneralInfoId)) {
                ShowResult("Please select Cash or Bank!", "failure");
                return true;
            }
        }
        else if ($scope.partyType === "Vendor") {
            if ($scope.advance.PartyId === null) {
                ShowResult("Please select Vendor!", "failure");
                return false;
            }
            if ($scope.advance.GLGeneralInfoId === null) {
                ShowResult("Please select Cash or Bank!", "failure");
                return false;
            }
            if ($scope.advance.ResponsiblePerson === null || $scope.advance.ResponsiblePerson === "") {
                ShowResult("Please select Responsible Person!", "failure");
                return false;
            }
        }
        else if ($scope.partyType === "Employee") {
            if ($scope.advance.EmployeeId === null) {
                ShowResult("Please select Employee!", "failure");
                return false;
            }
            if ($scope.advance.GLGeneralInfoId === null) {
                ShowResult("Please select Cash or Bank!", "failure");
                return false;
            }
        }
        return true;
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.advance.ResponsiblePersonId = employee.SystemId;
            $scope.advance.ResponsiblePerson = employee.EmployeeName;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.clearEmployeePopUp = function () {
        $scope.advance.ResponsiblePersonId = null;
        $scope.advance.ResponsiblePerson = null;
    };

    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.clearDrData();
        if (baseService.isUndefinedOrNull(party.DownPaymentGLId)) {
            ShowResult("Customer DownPaymentGL not found!", "failure", "partyPopUp");
            return;
        }
        else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.DownPaymentBudgetId)) {
            ShowResult("Customer budget not found!", "failure", "partyPopUp");
            return;
        }
        else if (baseService.isUndefinedOrNull(party.CurrencyId)) {
            ShowResult("Customer transaction currency not found!", "failure", "partyPopUp");
            return;
        }
        else {
            $scope.advanceDetail.GLGeneralInfoId = party.DownPaymentGLId;
            $scope.advanceDetail.GLGeneralInfoCode = party.DownPaymentGLCode;
            $scope.advanceDetail.GLGeneralInfoName = party.DownPaymentGLName;
            $scope.advanceDetail.BudgetMasterId = party.DownPaymentBudgetId;
            $scope.advanceDetail.BudgetCode = party.DownPaymentBudgetCode;
            $scope.advanceDetail.BudgetName = party.DownPaymentBudgetName;
            $scope.advanceDetail.ActivityId = party.DownPaymentActivityId;
            $scope.advanceDetail.ActivityCode = party.DownPaymentActivityCode;
            $scope.advanceDetail.ActivityName = party.DownPaymentActivityName;
        }

        // Set to Advance
        $scope.advance.PartyId = party.Id;
        $scope.advance.PartyCode = party.Code;
        $scope.advance.PartyName = party.Code + " - " + party.UserName;
        $scope.advance.PartyType = party.PartyType;
        $scope.advance.CurrencyId = party.CurrencyId;
        $scope.advance.TotalPartyPlant = party.TotalPartyPlant;

        // Set to AdvanceDetail
        $scope.advanceDetail.PartyId = party.Id;
        $scope.advanceDetail.PartyCode = party.Code;
        $scope.advanceDetail.PartyName = party.Code + " - " + party.UserName;
        $scope.advanceDetail.PartyType = party.PartyType;

        $scope.GetCurrencyExchangeRateList();
        $scope.checkBankAmount();
        $scope.getPartyPlantList(party.Id);
        $scope.advance.POId = null;
        $scope.hidePartyPopUp();
    };

    // Clear Dr. data if party selection change
    $scope.clearDrData = function () {
        $scope.advanceDetailList = [];
    };

    $scope.updatePartyAmount = function () {
        angular.forEach($scope.advanceDetailList, function (item, i) {
            item.Narration = $scope.advance.Narration;
            item.Amount = $scope.advance.Amount;
        });
    };

    $scope.removeRow = function (index) {
        $scope.advanceDetailList.splice(index, 1);
        $scope.updatePartyAmount();
    };

    $scope.clearPartyPopUp = function () {
        $scope.advance.PartyId = null;
        $scope.advance.PartyCode = null;
        $scope.advance.PartyName = null;
        $scope.advance.PartyType = null;
        $scope.advance.CurrencyId = null;
        $scope.advance.TotalPartyPlant = null;
        $scope.partyPlantList = [];
    };

    $scope.getPartyPlantList = function (partyId) {
        $scope.partyPlantList = [];
        $http.get("Parties/party/GetPartyPlantCbo?partyId=" + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault) {
                        $scope.advance.PartyPlantId = item.Value;
                        $scope.advanceDetail.PartyPlantId = item.Value;
                        $scope.advanceDetail.PartyPlantName = item.Text;
                        $scope.advanceDetailList.push($scope.advanceDetail);
                    }
                    $scope.updatePartyAmount();
                });
            });
    };

    $scope.changePartyPlantList = function (id) {
        for (var i = 0; i < $scope.advanceDetailList.length; i++) {
            $scope.advanceDetailList[i].PartyPlantId = id;
        }
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
                ShowResult("Please select currency!", "failure", "bankPopUp");
                return;
            }
            if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
                ShowResult("Bank GL not found!", "failure", "bankPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(bank.BudgetMasterId)) {
                ShowResult("Bank budget not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(bank.CurrencyId)) {
                ShowResult("Bank transaction currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.advance.AccountTitle = bank.AccountTitle;
                $scope.advance.BankName = bank.AccountTitle;
                $scope.advance.BankMasterId = bank.BankMasterId;
                $scope.advance.BankCurrencyId = bank.CurrencyId;

                $scope.advance.GLGeneralInfoId = bank.GLGeneralInfoId;
                $scope.advance.GLGeneralInfoCode = bank.GLGeneralInfoCode;
                $scope.advance.GLGeneralInfoName = bank.GLGeneralInfoName;
                $scope.advance.BudgetMasterId = bank.BudgetMasterId;
                $scope.advance.BudgetCode = bank.BudgetCode;
                $scope.advance.BudgetName = bank.BudgetName;
                $scope.advance.ActivityId = bank.ActivityId;
                $scope.advance.ActivityCode = bank.ActivityCode;
                $scope.advance.ActivityName = bank.ActivityName;
                $scope.checkBankAmount();
            }
        }
        $scope.hideBankPopUp();
    };

    $scope.clearBankPopUp = function () {
        $scope.isBankAmount = false;
        $scope.advance.AccountTitle = null;
        $scope.advance.BankName = null;
        $scope.advance.BankMasterId = null;
        $scope.advance.BankCurrencyId = null;
        $scope.advance.CashMasterId = null;
        $scope.advance.CashName = null;
        $scope.advance.CashCurrencyId = null;
        $scope.advance.GLGeneralInfoId = null;
        $scope.advance.GLGeneralInfoCode = null;
        $scope.advance.GLGeneralInfoName = null;
        $scope.advance.BudgetMasterId = null;
        $scope.advance.BudgetCode = null;
        $scope.advance.BudgetName = null;
        $scope.advance.ActivityId = null;
        $scope.advance.ActivityCode = null;
        $scope.advance.ActivityName = null;
    };

    $scope.closeCashPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult("Please select currency!", "failure", "cashPopUp");
            return;
        }
        if ($scope.cashIndex !== -1) {
            var cash = $scope.cashList[$scope.cashIndex];
            if (baseService.isUndefinedOrNull(cash.GLGeneralInfoId)) {
                ShowResult("Cash GL not found!", "failure", "cashPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(cash.BudgetMasterId)) {
                ShowResult("Cash budget not found!", "failure", "cashPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(cash.CurrencyId)) {
                ShowResult("Cash transaction currency not found!", "failure", "cashPopUp");
                return;
            }
            else {
                $scope.advance.CashMasterId = cash.Id;
                $scope.advance.CashName = cash.CashName;
                $scope.advance.CashCurrencyId = cash.CurrencyId;
                $scope.advance.GLGeneralInfoId = cash.GLGeneralInfoId;
                $scope.advance.GLGeneralInfoCode = cash.GLGeneralInfoCode;
                $scope.advance.GLGeneralInfoName = cash.GLGeneralInfoName;
                $scope.advance.BudgetMasterId = cash.BudgetMasterId;
                $scope.advance.BudgetCode = cash.BudgetCode;
                $scope.advance.BudgetName = cash.BudgetName;
                $scope.advance.ActivityId = cash.ActivityId;
                $scope.advance.ActivityCode = cash.ActivityCode;
                $scope.advance.ActivityName = cash.ActivityName;
                $scope.checkBankAmount();
            }
        }
        $scope.hideCashPopUp();
    };

    $scope.clearCashPopUp = function () {
        $scope.clearBankPopUp();
    };

    $scope.clear = function () {
        $scope.Action = "Save";
        $scope.getCboVoucherTypeAdvanceGivenList();
        $scope.advance.Active = true;
        $scope.advance.DocRefNo = null;
        $scope.advance.PaymentSource = "Bank";
        $scope.advance.ReviewDate = null;
        $scope.advance.Amount = null;
        $scope.advance.Narration = null;
        $scope.advance.IsInterTransaction = false;
        $scope.advance.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.currencyExchangeRate = [];
        $scope.advanceCharge = {};
        $scope.advanceTax = {};
        $scope.advanceDetailList = [];
        $scope.voucherDetailCurrencyList = [];
        $scope.advanceChargesList = [];
        $scope.clearPartyPopUp();
        $scope.clearBankPopUp();
        $scope.clearCashPopUp();
        $scope.clearEmployeePopUp();
        $scope.advance.CompanyCurrencyRate = null;
        $scope.advanceTaxesList = [];
        $scope.advance.POId = null;
    };

    $scope.rateChangeBankCharge = function (rate) {
        $scope.advanceCharge.CompanyCurrencyAmount = $scope.advanceCharge.Amount * rate;
        if ($scope.advanceChargesList.length !== null) {
            for (var i = 0; i < $scope.advanceChargesList.length; i++) {
                $scope.advanceChargesList[i].CompanyCurrencyAmount = $scope.advanceChargesList[i].Amount * rate;
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

    $scope.save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.validation();
        if (!baseService.isUndefinedOrNull($scope.advanceCharge.FinancingTypeId)) {
            ShowResult("Please add Charges!", "failure");
            return false;
        }
        if (!baseService.isUndefinedOrNull($scope.advanceTax.TaxCodeId)) {
            ShowResult("Please add Taxes!", "failure");
            return false;
        }
        if ($scope.form0.$valid && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.parkUrl,
                    data: {
                        "advanceVM": $scope.advance,
                        "advanceDetailVMList": $scope.advanceDetailList,
                        "bankChargeDetailVMList": $scope.advanceChargesList,
                        "taxDetailVMList": $scope.advanceTaxesList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: {
                        "advanceVM": $scope.advance,
                        "advanceDetailVMList": $scope.advanceDetailList,
                        "bankChargeDetailVMList": $scope.advanceChargesList,
                        "taxDetailVMList": $scope.advanceTaxesList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            return true;
        }
        return true;
    };

    //$scope.advanceId = null;
    //$scope.confirmPost = function (advanceId) {
    //    $scope.advanceId = advanceId;
    //    $scope.message_confirmation = "Are you sure to Post?";
    //    angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    //};

    $scope.post = function () {
        if ($scope.EntityId_Post == null || $scope.EntityId_Post == "" || $scope.EntityId_Post == undefined) {
            ShowResult("Please select Entity First!!", "failure");
        }
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "advanceId": $scope.advanceId,
                "entityId": $scope.EntityId_Post,
                "voucherId": $scope.voucherId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                $scope.closePostPopUp();
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.unPost = function (advanceId) {
        $http({
            method: "POST",
            url: $scope.unPostUrl,
            data: {
                "advanceId": advanceId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    cboService.getEnumCbo("enum/GetCboPaymentType", function (result) {
        $scope.paymentTypeList = result;
    });


    $scope.advanceCharge = {
        FinancingTypeId: null,
        FinancingTypeName: null,
        Amount: null,
        CompanyCurrencyAmount: null
    };

    $scope.advanceChargesList = [];
    $scope.addCharge = function () {
        if (manualValidation("td_FinancingType", baseService.isUndefinedOrNull($scope.advanceCharge.FinancingTypeId), "Charges Type is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_FinancingTypeAmount", baseService.isUndefinedOrNull($scope.advanceCharge.Amount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_FinancingTypeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.advanceCharge.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.advanceCharge.FinancingTypeName = $.grep($scope.bankChargeTypeList, function (item) {
                return item.FinancingTypeId === $scope.advanceCharge.FinancingTypeId;
            })[0].ExpensesUserName;
            $scope.advanceChargesList.push($scope.advanceCharge);
            $scope.advanceCharge = {};
            $scope.copyAmount();
        }
    };

    $scope.copyChargesAmount = function () {
        if ($scope.advance.CurrencyId === $scope.companyCurrencyId) {
            $scope.advanceCharge.CompanyCurrencyAmount = $scope.advanceCharge.Amount;
        }
        else {
            $scope.advanceCharge.CompanyCurrencyAmount = ($scope.advanceCharge.Amount * $scope.advance.CompanyCurrencyRate).toFixed(2);
        }
    };

    bankService.getCboBankChargeTypeList(function (result) {
        $scope.bankChargeTypeList = result;
    });


    $scope.removeChargesRow = function (index) {
        $scope.advanceChargesList.splice(index, 1);
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
                return item.Id === $scope.advanceTax.TaxCodeId;
            })[0].UserName;
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

    $scope.delete = function (advanceId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "advanceId": advanceId, "voucherId": voucherId
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
                $scope.advanceId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.advanceId = null;
    $scope.confirmDelete = function (advanceId, voucherId) {
        $scope.advanceId = advanceId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

    $scope.POList = [];
    $scope.getPOList = function () {
        //debugger;
        var PoType = 'PO';
        $scope.status = 'PO';
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetPOListForAdvance?PoType=' + PoType + '&Status=' + $scope.status + '&vendorId=' + $scope.advance.PartyId,
        }).then(function successCallback(response) {
            $scope.POList = response.data;
        });
        angular.element(document.querySelector("#POPopUp")).modal("show");

    };

    $scope.SelectPO = function (data) {
        $scope.advance.POId = data.data.Id;
        angular.element(document.querySelector("#POPopUp")).modal("hide");
    }

    $scope.ClsoePOPopUp = function () {
        angular.element(document.querySelector("#POPopUp")).modal("hide");

    }
}