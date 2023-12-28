"use strict";
investmentController.$inject = ["accountService", "bankService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function investmentController(accountService, bankService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Investment";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.currencyExchangeRate = [];
    $scope.hideSource = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.isWriteOff = true;
    $scope.sourceType = "Investment";
    $scope.isAdvance = false;
    $scope.isReadOnly = false;
    $scope.partyType = "Party";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $scope.url = "accounts/Investment";
    $scope.postUrl = $scope.url + "/PostInvestment";
    $scope.deleteUrl = $scope.url + "/DeleteInvestment";

    $scope.voucher = {
        Id: null,
        PartyType: "Customer",
        EntityId: null,
        PartyId: null,
        PartyName: null,
        CurrencyId: null,
        PaymentTermId: null,
        SourceType: "Given",
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        MatureDate: null,
        DocRefNo: null,
        Amount: null,
        Narration: null,
        Remarks: null,
        BankName: null,
        BankMasterId: null,
        CashName: null,
        CashMasterId: null,
        BankCurrencyId: null,
        BankAmount: null,
        BankAccountNumber: null,
        PaymentSource: "Bank",
        CompanyCurrencyRate: null,
        TransactionType: "InvestmentGiven"
    };

    baseService.init("Accounts/Investment/GetInvestmentList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.investmentGivenList = result.Rows;
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
            "name": "Party Code",
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
        },
        {
            "name": "Status",
            "value": "Status"
        },
        {
            "name": "Particulars",
            "value": "Particulars"
        },
        {
            "name": "Investment Type",
            "value": "InvestmentType"
        },
        {
            "name": "Transaction Type",
            "value": "TransactionType"
        }
    ];

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, "", function (result) {
                $scope.entityList = result;
            });
    });

    $scope.getCboVoucherTypeInvestmentList = function () {
        accountService.getCboVoucherTypeInvestmentList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
            }
        });
    };
    $scope.getCboVoucherTypeInvestmentList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
    };

    cboService.getCboOtherFinancingType($scope.sourceType, function (result) {
        $scope.financingTypeList = result;
    });

    $scope.getdirectorList = function () {
        $scope.directorList = [];
        $http.get("Parties/party/GetCompanyDirectorDataList")
            .then(function (response) {
                $scope.directorList = response.data.Rows;
            });
    };
    $scope.getdirectorList();

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

    $scope.customerList = [];
    $scope.customerIndex = -1;
    $scope.selectedCustomer = null;
    $scope.searchCustomerByList = [
        {
            "name": "Party Code",
            "value": "Code"
        },
        {
            "name": "Party Name",
            "value": "UserName"
        },
        {
            "name": "Currency",
            "value": "CurrencyCode"
        },
        {
            "name": "VATResistrationNo",
            "value": "VATResistrationNo"
        },
        {
            "name": "TradeLicenseNo",
            "value": "TradeLicenseNo"
        },
        {
            "name": "Debit Limit",
            "value": "DebitLimit"
        },
        {
            "name": "Credit Limit",
            "value": "CreditLimit"
        }
    ];

    $scope.customerParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "UserName",
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    bankService.getCashMasterCboList(function (result) {
        $scope.cashMasterList = result;
    });
    $scope.bankMasterList = [];
    $scope.investmentbankMasterList = [];
    bankService.getInvestmentBankMasterCbo(function (result) {
        $scope.investmentbankMasterList = result;
    });
    bankService.getBankMasterHouseBankCboList(function (result) {
        $scope.bankMasterList = result;
    });
    $scope.getCustomerGL = function () {
        $scope.glUrl = "Parties/party/GetCompanyPartyDataList?partyType=" + $scope.voucher.PartyType;
        $scope.getCustomerData = function (pageno) {
            baseService.paginationBase($scope.glUrl, pageno, $scope.customerParameters)
                .then(function (result) {
                    $scope.customerList = result.Rows;
                    $scope.customerParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#customerListPopUp")).modal("show");
        $scope.getCustomerData();
    };

    $scope.selectCustomerPopUp = function (index, id) {
        $scope.customerIndex = index;
        $scope.selectedCustomer = id;
    };

    $scope.getPartyPlantList = function (partyId, isUpdateMode) {
        $scope.partyPlantList = [];
        $http.get("Parties/party/GetPartyPlantCbo?partyId=" + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault && !isUpdateMode) {
                        $scope.voucher.PartyPlantId = item.Value;
                    }
                });
            });
    };

    $scope.closeCustomerPopUp = function () {
        if ($scope.customerIndex !== -1) {
            var party = $scope.customerList[$scope.customerIndex];
            $scope.voucher.PartyName = party.Code + " - " + party.UserName;
            $scope.voucher.PartyId = party.Id;
            $scope.voucher.PartyType = party.PartyType;
            $scope.getPartyPlantList(party.Id);
        }
        angular.element(document.querySelector("#customerListPopUp")).modal("hide");
        $scope.customerIndex = -1;
        $scope.selectedCustomer = null;
    };

    $scope.partyTypeOther = function () {
        $scope.voucher.PartyType = $scope.partyType;
    };

    $scope.changeSourceTo = function (to) {
        $scope.voucher.DrBankMasterId = null;
        $scope.voucher.PartyName = null;
        $scope.voucher.DirectorName = null;
        $scope.partyType = to;
    };

    $scope.changeDirector = function () {
        $scope.voucher.PartyType = $scope.partyType;
    };

    $scope.changeTransactionType = function (type) {
        $scope.voucher.TransactionType = type;
    };

    $scope.changeSourceFrom = function (from) {
        $scope.voucher.CrGLId = null;
        $scope.voucher.CrGLName = null;
        $scope.voucher.CrBudgetId = null;
        $scope.voucher.CrActivityId = null;
        $scope.voucher.BankName = null;
        $scope.voucher.CashName = null;
        $scope.voucher.BankMasterId = null;
        $scope.voucher.CashMasterId = null;
        $scope.isBankAmount = false;
        $scope.voucher.BankCurrencyId = null;
    };

    $scope.transactionTypeGL = null;
    $scope.getTransactionTypeGL = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $scope.transactionTypeGL = $.grep($scope.financingTypeList, function (item) {
                return item.FinancingTypeId === id;
            })[0];
            if (manualValidation("div_TransactionType", baseService.isUndefinedOrNull($scope.transactionTypeGL.AssetGLId), "Transaction Type GL not found!")) {
                $scope.transactionTypeGL = null;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget
                && manualValidation("div_TransactionType", baseService.isUndefinedOrNull($scope.transactionTypeGL.AssetBudgetMasterId), "Transaction Type Budget not found!")) {
                $scope.transactionTypeGL = null;
            }
            $scope.voucher.DrGLId = $scope.transactionTypeGL.AssetGLId;
            $scope.voucher.DrBudgetId = $scope.transactionTypeGL.AssetBudgetMasterId;
            $scope.voucher.DrActivityId = $scope.transactionTypeGL.AssetActivityId;
        }
        else {
            manualValidation("div_TransactionType", true, "Transaction Type is required.");
            $scope.transactionTypeGL = null;
        }
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

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        $scope.passBankCashAmount();
        if ($scope.form0.$valid && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "accounts/Investment/InsertInvestment",
                    data: {
                        "voucherVM": $scope.voucher
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
                        $scope.isReadOnly = false;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: "accounts/Investment/UpdateInvestment",
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

    $scope.closePartyPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure", "partyPopUp");
            return true;
        }
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
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
            }
        }
        $scope.hidePartyPopUp();
    };

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher = {};
        $scope.voucher.Active = true;
        $scope.voucher.TransactionType = "InvestmentGiven";
        $scope.voucher.PaymentSource = "Bank";
        $scope.voucher.PartyType = "Customer";
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.getCboVoucherTypeInvestmentList();
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
        $scope.SelectedCurrency = null;
        $scope.voucher.CurrencyId = null;
        $scope.isReadOnly = false;
    };


    $scope.financingId = null;
    $scope.confirmPost = function (financingId) {
        $scope.financingId = financingId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function (financingId) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "financingId": financingId
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

    $scope.delete = function (financingId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "financingId": financingId, "voucherId": voucherId
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
                $scope.financingId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };
    $scope.financingId = null;
    $scope.confirmDelete = function (financingId, voucherId) {
        $scope.financingId = financingId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };
}