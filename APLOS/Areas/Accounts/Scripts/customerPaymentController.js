"use strict";
customerPaymentController.$inject = ["bankService", "cboService", "baseService", "factoryService", "commonMessage", "$scope", "$rootScope", "$http", "$filter", "$controller"];
function customerPaymentController(bankService, cboService, baseService, factoryService, commonMessage, $scope, $rootScope, $http, $filter, $controller) {
    $rootScope.title = "Customer Payment";
    $scope.Action = "Save";
    $scope.voucherDetailCurrency = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.index = -1;
    $scope.url = "accounts/Advance";
    $scope.listUrl = $scope.url + "/GetCustomerPaymentList";
    $scope.parkUrl = $scope.url + "/SaveCustomerPayment";
    $scope.updateUrl = $scope.url + "/UpdateCustomerPayment";
    $scope.postUrl = $scope.url + "/PostCustomerAdvance";
    $scope.unPostUrl = $scope.url + "/UnPostCustomerAdvance";
    $scope.reportUrl = $scope.url + "/ReportCustomerPayment?voucherId=";
    $scope.jouranlUrl = $scope.url + "/GetAvailableJournalCustomerAdvance";

    $scope.partyType = "Customer";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.hideSource = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });
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
            "name": "Doc Date",
            "value": "DocDate"
        },
        {
            "name": "Doc Ref",
            "value": "DocRefNo"
        },
        {
            "name": "Currency",
            "value": "Currency"
        }
    ];

    $scope.advance = {
        Id: null,
        AdvanceId: null,
        CompanyGroupId: null,
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
        IsInterTransaction: false
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
        Amount: null,
        TaxAmount: 0,
        NetAmount: 0
    };

    $scope.advanceDetailList = [];

    $scope.voucherDetailCurrency = {
        Id: null,
        VoucherId: null,
        VoucherDetailId: null,
        ParallelCurrencyId: null,
        FromCurrencyId: null,
        ToCurrencyId: null,
        ToCurrencyRate: null,
        DrAmount: 0,
        CrAmount: 0,
        TrnType: null
    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, "", function (result) {
                $scope.entityList = result;
            });
    });

    $scope.getById = function (id) {
        $http({
            method: "GET",
            url: "accounts/Advance/GetAdvance/" + id
        }).then(function successCallback(response) {
            $scope.advance = response.data;
            $scope.advance.DocDate = $filter("dateFiltering")($scope.advance.DocDate);
            $scope.advance.VoucherDate = $filter("dateFiltering")($scope.advance.VoucherDate);
            $scope.advance.PostingDate = $filter("dateFiltering")($scope.advance.PostingDate);
            $scope.advance.ReviewDate = $filter("dateFiltering")($scope.advance.ReviewDate);
            $scope.getPartyPlantList($scope.advance.PartyId, true);

            $http({
                method: "GET",
                url: "accounts/Advance/GetAdvanceDetail?advanceId=" + id
            }).then(function successCallback(response) {
                $scope.advanceDetailList = response.data;
                if (!baseService.isUndefinedOrNull($scope.advance.BankMasterId)) {
                    factoryService.getBankMasterGL($scope.advance.BankMasterId, function (result) {
                        setBankGL(result);
                    });
                }
                if (!baseService.isUndefinedOrNull($scope.advance.CashMasterId)) {
                    factoryService.getCashMasterGL($scope.advance.CashMasterId, function (result) {
                        setCashGL(result);
                    });
                }
            });
            $http({
                method: "GET",
                url: "accounts/Advance/GetBankChargeListByAdvance?advanceId=" + id
            }).then(function successCallback(response) {
                $scope.advanceChargesList = response.data;
            });
            $scope.Action = "Update";
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.advance.PostingDate) && !baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.advance.PostingDate + "&currencyId=" + $scope.advance.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.advance.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
                $scope.rateChange($scope.advance.CompanyCurrencyRate);
                $scope.rateChangeBankCharge($scope.advance.CompanyCurrencyRate);
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };

    $scope.GetChangePostingDateCurrencyExchangeRateList = function (postingDate) {
        if (!baseService.isUndefinedOrNull(postingDate) && !baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + postingDate + "&currencyId=" + $scope.advance.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.advance.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
                $scope.rateChange($scope.advance.CompanyCurrencyRate);
                $scope.rateChangeBankCharge($scope.advance.CompanyCurrencyRate);
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };

    $scope.copyAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.advance.BankCurrencyId)) {
            if ($scope.advance.BankCurrencyId !== $scope.advance.CurrencyId) {
                $scope.advance.BankAmount = $scope.advance.Amount * $scope.advance.CompanyCurrencyRate;
            }
            else {
                $scope.advance.BankAmount = $scope.advance.Amount;
            }
        }
    };

    $scope.checkBankAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.advance.BankCurrencyId)) {
            if ($scope.advance.BankCurrencyId !== $scope.advance.CurrencyId) {
                $scope.isBankAmount = true;
                $scope.advance.BankAmount = 0;
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
            $scope.currencyExchangeRate = null;
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.advance.PostingDate) < new Date($scope.advance.DocDate)) {
            msg = "Posting date must be below or equal to Doc Date!";
            $scope.currencyExchangeRate = null;
            $scope.invalidPostingDate = true;
        } else {
            $scope.invalidPostingDate = false;
            $scope.$scope.GetChangePostingDateCurrencyExchangeRateList(new Date($scope.advance.PostingDate));
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
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

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
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
            $scope.copyAmount();
        }
        $scope.hidePartyPopUp();
    };

    // Clear Dr. data if party selection change
    $scope.clearDrData = function () {
        $scope.advanceDetailList = [];
        for (var i = 0; i < $scope.voucherDetailCurrencyList.length; i++) {
            if ($scope.voucherDetailCurrencyList[i].TrnType === "Cr") {
                $scope.voucherDetailCurrencyList.splice(i, 1);
            }
        }
    };

    $scope.updateCrAmount = function (data) {
        angular.forEach($scope.advanceDetailList, function (item, i) {
            if (item.PartyType === $scope.partyType) {
                item.Narration = $scope.advance.Narration;
                item.PartyPlantId = $scope.advance.PartyPlantId;
                item.PartyPlantName = item.PartyPlantName === null ? $scope.PartyPlantName : item.PartyPlantName;
            }
            if (!$scope.advance.IsInterTransaction) {
                item.Amount = $scope.advance.Amount;
            }
            if (data !== undefined && data !== null && item.PartyType === $scope.partyType) {
                item.Amount = $scope.advance.Amount - data.Amount;
            }
        });
    };

    $scope.removeRow = function (index) {
        $scope.advanceDetailList.splice(index, 1);
        $scope.updateCrAmount(null);
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

    $scope.getPartyPlantList = function (partyId, isUpdateMode) {
        $scope.partyPlantList = [];
        $http.get("Parties/party/GetPartyPlantCbo?partyId=" + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault && !isUpdateMode) {
                        $scope.advance.PartyPlantId = item.Value;
                        $scope.advanceDetail.PartyPlantId = item.Value;
                        $scope.advanceDetail.PartyPlantName = item.Text;
                    }
                });
                $scope.advanceDetail = {};
            });
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
                setBankGL(bank);
            }
        }
        $scope.hideBankPopUp();
    };

    function setBankGL(bank) {
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
        $scope.copyAmount();
    }

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
                setCashGL(cash);
            }
        }
        $scope.hideCashPopUp();
    };

    function setCashGL(cash) {
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

    $scope.clearCashPopUp = function () {
        $scope.clearBankPopUp();
    };

    $scope.clear = function () {
        $scope.Action = "Save";
        $scope.advance.Active = true;
        $scope.advance.DocRefNo = null;
        $scope.advance.PaymentSource = "Bank";
        $scope.advance.ReviewDate = null;
        $scope.advance.Amount = null;
        $scope.advance.Narration = null;
        $scope.advance.IsInterTransaction = false;
        $scope.advance.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.getCboVoucherTypeReceiptList();
        $scope.currencyExchangeRate = null;
        $scope.advanceDetailList = [];
        $scope.advanceCharge = {};
        $scope.advanceDetail = {};
        $scope.advanceChargesList = [];
        $scope.clearPartyPopUp();
        $scope.clearBankPopUp();
        $scope.clearCashPopUp();
        $scope.clearEmployeePopUp();
    };

    $scope.report = function (voucherId) {
        location.href = $scope.reportUrl + voucherId;
    };

    $scope.getCboVoucherTypeReceiptList = function () {
        cboService.getCboVoucherTypeReceiptList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.advance.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.advance.DocDate = $scope.advance.PostingDate;
            }
        });
    };
    $scope.getCboVoucherTypeReceiptList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.advance.VoucherTypeId = data.Value;
        $scope.advance.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.advance.DocDate = $scope.advance.PostingDate;
    };

    cboService.getCboInterCompany(null, function (result) {
        $scope.companyList = result;
    });

    $scope.companyChange = function (companyId) {
        cboService.getCboInterPlant("", companyId, "", function (result) {
            $scope.interplantList = result;
        });
    };

    $scope.invalidadvanceDetailList = false;
    $scope.validation = function () {
        if ($scope.advanceDetailList.length === 0) {
            ShowResult("Please select one payment type!", "failure");
            $scope.invalidadvanceDetailList = true;
        }
        else {
            $scope.invalidadvanceDetailList = false;
        }
    };

    $scope.save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.validation();
        if ($scope.form0.$valid && !$scope.invalidDocDate && !$scope.invalidadvanceDetailList && !$scope.invalidPostingDate && !$scope.invalidRow) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.parkUrl,
                    data: {
                        "advanceVM": $scope.advance,
                        "advanceDetailVMList": $scope.advanceDetailList,
                        "bankChargeDetailVMList": $scope.advanceChargesList
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
                        "bankChargeDetailVMList": $scope.advanceChargesList
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

    $scope.advanceId = null;
    $scope.confirmPost = function (advanceId) {
        $scope.advanceId = advanceId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function (advanceId) {
        $http({
            method: "POST",
            url: $scope.postUrl,
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

    $scope.confirmUnPost = function (advanceId) {
        $scope.advanceId = advanceId;
        $scope.message_confirmation = "Are you sure to UnPost?";
        angular.element(document.querySelector("#confirmUnPostPopUp")).modal("show");
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

    $scope.company = null;
    $scope.getCompanyInfo = function (companyId) {
        if (!baseService.isUndefinedOrNull(companyId)) {
            $scope.company = $.grep($scope.companyList, function (item) {
                return item.CompanyId === companyId;
            })[0];
            if (manualValidation("div_Company", baseService.isUndefinedOrNull($scope.company.PartyId), "This Company is not created as InterCompany Party.")) {
                $scope.company = null;
            }
        }
        else {
            manualValidation("div_Company", true, "Company is required.");
            $scope.company = null;
        }
    };

    $scope.plant = null;
    $scope.getPlantInfo = function (plantId) {
        if (!baseService.isUndefinedOrNull(plantId)) {
            $scope.plant = $.grep($scope.interplantList, function (item) {
                return item.PlantId === plantId;
            })[0];
            if (manualValidation("div_Plant", baseService.isUndefinedOrNull($scope.plant.PartyPlantId), "This Company is not created as InterCompany Party Plant.")) {
                $scope.plant = null;
            }
        }
        else {
            manualValidation("div_Plant", true, "Plant is required.");
            $scope.plant = null;
        }
    };

    $scope.addRow = function () {
        $scope.advanceDetail.GLGeneralInfoId = $scope.transactionTypeGL.LiabilityGLId;
        $scope.advanceDetail.GLGeneralInfoCode = $scope.transactionTypeGL.LiabilityGLCode;
        $scope.advanceDetail.GLGeneralInfoName = $scope.transactionTypeGL.LiabilityGLName;
        $scope.advanceDetail.BudgetMasterId = $scope.transactionTypeGL.LiabilityBudgetMasterId;
        $scope.advanceDetail.BudgetCode = $scope.transactionTypeGL.LiabilityBudgetCode;
        $scope.advanceDetail.BudgetName = $scope.transactionTypeGL.LiabilityBudgetName;
        $scope.advanceDetail.ActivityId = $scope.transactionTypeGL.LiabilityActivityId;
        $scope.advanceDetail.ActivityCode = $scope.transactionTypeGL.LiabilityActivityCode;
        $scope.advanceDetail.ActivityName = $scope.transactionTypeGL.LiabilityActivityName;

        $scope.advanceDetail.PartyType = $scope.company.PartyType;
        $scope.advanceDetail.CompanyId = $scope.company.CompanyId;
        $scope.advanceDetail.PartyId = $scope.company.PartyId;
        $scope.advanceDetail.PartyCode = $scope.company.PartyCode;
        $scope.advanceDetail.PartyName = $scope.company.PartyCode + " - " + $scope.company.PartyName;
        $scope.advanceDetail.PlantId = $scope.plant.PlantId;
        $scope.advanceDetail.PartyPlantId = $scope.plant.PartyPlantId;
        $scope.advanceDetail.PartyPlantName = $scope.plant.PartyPlantName;
        $scope.advanceDetail.Amount = null;

        $scope.advanceDetailList.push($scope.advanceDetail);
        $scope.advanceDetail = {};
    };

    $scope.advanceInterTransactionSearchByList = [
        {
            "name": "#No",
            "value": "AdvanceNo"
        },
        {
            "name": "Company",
            "value": "CompanyName"
        },
        {
            "name": "Plant",
            "value": "PlantName"
        },
        {
            "name": "Party Code",
            "value": "PartyCode"
        },
        {
            "name": "Party Name",
            "value": "PartyName"
        },
        {
            "name": "Party Plant",
            "value": "PartyPlantName"
        }
    ];

    $scope.advanceInterTransactionParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "CompanyName, PlantName",
        searchBy: "AdvanceNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showAdvanceInterTransactionPopUp = function () {
        $scope.advanceInterTransactionParameters.partyId = $scope.advance.PartyId;
        baseService.setCurrentPage("advanceInterTransactionList");
        $scope.getAdvanceInterTransactionList = function (pageno) {
            baseService.paginationBase($scope.jouranlUrl, pageno, $scope.advanceInterTransactionParameters)
                .then(function (result) {
                    $scope.advanceInterTransactionList = result.Rows;
                    $scope.advanceInterTransactionParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#advanceJournalPopUp")).modal("show");
        $scope.getAdvanceInterTransactionList();
    };

    $scope.closeAdvanceInterTransactionPopUp = function (data) {
        $scope.financingTypeGL = $.grep($scope.financingTypeList, function (item) {
            return item.FinancingTypeId === data.FinancingTypeId;
        })[0];
        if (baseService.isUndefinedOrNull($scope.financingTypeGL.AssetGLId)) {
            ShowResult("Transaction Type GL not found!", "failure", "advanceJournalPopUp");
        }
        else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull($scope.financingTypeGL.AssetBudgetMasterId)) {
            ShowResult("Transaction Type Budget not found!", "failure", "advanceJournalPopUp");
        }

        $scope.advance.AdvanceId = data.AdvanceId;
        $scope.advance.AdvanceNo = data.AdvanceNo;
        $scope.advance.Amount = data.NetAmount;
        $scope.advance.Narration = data.Narration;
        $scope.advance.JournalId = data.AdvanceId;
        $scope.advance.AdvanceDetailId = data.AdvanceDetailId;

        $scope.advance.GLGeneralInfoId = $scope.financingTypeGL.AssetGLId;
        $scope.advance.GLGeneralInfoCode = $scope.financingTypeGL.AssetGLCode;
        $scope.advance.GLGeneralInfoName = $scope.financingTypeGL.AssetGLName;
        $scope.advance.BudgetMasterId = $scope.financingTypeGL.AssetBudgetMasterId;
        $scope.advance.BudgetCode = $scope.financingTypeGL.AssetBudgetCode;
        $scope.advance.BudgetName = $scope.financingTypeGL.AssetBudgetName;
        $scope.advance.ActivityId = $scope.financingTypeGL.AssetActivityId;
        $scope.advance.ActivityCode = $scope.financingTypeGL.AssetActivityCode;
        $scope.advance.ActivityName = $scope.financingTypeGL.AssetActivityName;
        $scope.updateCrAmount(null);
        $scope.checkBankAmount();
        $scope.hideAdvanceInterTransactionPopUp();
    };

    $scope.hideAdvanceInterTransactionPopUp = function () {
        angular.element(document.querySelector("#advanceJournalPopUp")).modal("hide");
    };

    $scope.invalidRow = false;
    $scope.addPayment = function () {
        if ($scope.advanceDetailList === null || $scope.advanceDetailList.length === 0) {
            if (manualValidation("td_PaymentType", baseService.isUndefinedOrNull($scope.advanceDetail.PaymentType), "Payment Type is required.")) {
                $scope.invalidRow = true;
            }
            else if (manualValidation("td_PaymentAmount", baseService.isUndefinedOrNull($scope.advanceDetail.Amount), "Amount is required.")) {
                $scope.invalidRow = true;
            }
            else if (manualValidation("td_PaymentCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.advanceDetail.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
                $scope.invalidRow = true;
            }
            else {
                $scope.advanceDetailList.push($scope.advanceDetail);
                $scope.advanceDetail = {};
            }
        }
        else
            ShowResult("Can not add more than one item!", "failure");
    };

    $scope.copyDetailAmount = function () {
        if ($scope.advance.CurrencyId === $scope.companyCurrencyId) {
            $scope.advanceDetail.CompanyCurrencyAmount = $scope.advanceDetail.Amount;
        }
        else {
            $scope.advanceDetail.CompanyCurrencyAmount = ($scope.advanceDetail.Amount * $scope.advance.CompanyCurrencyRate).toFixed(2);
        }
    };

    $scope.rateChange = function (rate) {
        $scope.advanceDetail.CompanyCurrencyAmount = $scope.advanceDetail.Amount * rate;
        if ($scope.advanceDetailList.length !== null) {
            for (var i = 0; i < $scope.advanceDetailList.length; i++) {
                $scope.advanceDetailList[i].CompanyCurrencyAmount = $scope.advanceDetailList[i].Amount * rate;
            }
        }
    };

    $scope.rateChangeBankCharge = function (rate) {
        $scope.advanceCharge.CompanyCurrencyAmount = $scope.advanceCharge.Amount * rate;
        if ($scope.advanceChargesList.length !== null) {
            for (var i = 0; i < $scope.advanceChargesList.length; i++) {
                $scope.advanceChargesList[i].CompanyCurrencyAmount = $scope.advanceChargesList[i].Amount * rate;
            }
        }
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

    $scope.removeChargesRow = function (index) {
        $scope.advanceChargesList.splice(index, 1);
    };
}