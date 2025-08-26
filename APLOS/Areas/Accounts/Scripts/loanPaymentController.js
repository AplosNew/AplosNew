"use strict";
loanPaymentController.$inject = ["accountService", "bankService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function loanPaymentController(accountService, bankService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Loan Payment";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.currencyExchangeRate = [];
    $scope.partyType = 'Customer';
    $scope.sourceType = 'Loan';
    $scope.isAdvance = false;
    $scope.isReadOnly = false;
    $scope.isWriteOff = false;

    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $scope.url = "accounts/Loan";
    $scope.deleteUrl = $scope.url + "/DeleteLoanPayment";
    $scope.postUrl = $scope.url + "/PostLoanPayment";
    $scope.voucher = {
        Id: null,
        EntityId: null,
        PartyId: null,
        PartyName: null,
        PartyType: "Customer",
        CurrencyId: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        Amount: "",
        Narration: null,
        BankName: null,
        BankMasterId: null,
        OtherBankMasterId: null,
        CashName: null,
        CashMasterId: null,
        BankCurrencyId: null,
        BankAmount: null,
        BankAccountNumber: null,
        PaymentSource: "Bank",
        FinancingTypeId: null,
        RepaymentStartDate: null,
        LifeOfYear: "",
        NoOfInstallmentPerYear: "",
        TotalNoOfInstallment: "",
        ProfitRate: "",
        ProfitAmount: "",
        TransactionType: "LoanTaken",
        IsSchedule: false,
        IsSplit: false,
        CompanyCurrencyRate: 1
    };
    $scope.loanAddition = {
        LoanDocRefNo: null,
        FinancingId: null
    };


    $scope.LoanSetOffType = 'Single';
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
        if ($scope.tab == 1) {
            $scope.LoanSetOffType = 'Single';
        } else {
            $scope.LoanSetOffType = 'Multi';

        }
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

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
        }
        ,
        {
            "name": "LoanSetOffGroupNo",
            "value": "LoanSetOffGroupNo"
        }
        ,
        {
            "name": "Loan No",
            "value": "LoanNo"
        }
        ,
        {
            "name": "Status",
            "value": "Status"
        }
    ];

    baseService.init("accounts/Loan/GetLoanPaymentList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        cboService.getCboEntityByPlant(null, null, "", function (result) {
            $scope.entityList = result;
        });
    });

    $scope.getCboVoucherTypeLoanList = function () {
        accountService.getCboVoucherTypeLoanPaymentList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
            }
        });
    }
    $scope.getCboVoucherTypeLoanList();

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

    $scope.changeDirector = function () {
        $scope.voucher.PartyType = $scope.partyType;
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
            $scope.voucher.PartyType = $scope.partyType;
            $scope.getPartyPlantList(party.Id);
        }
        angular.element(document.querySelector("#customerListPopUp")).modal("hide");
        $scope.customerIndex = -1;
        $scope.selectedCustomer = null;
    };
    //**************************************** Customer List End ***************************

    $scope.changeDirector = function () {
        $scope.voucher.PartyType = $scope.partyType;
    };

    $scope.changeSourceTo = function (to) {
        $scope.voucher.DrBankMasterId = null;
        $scope.voucher.PartyName = null;
        $scope.voucher.DirectorName = null;
        $scope.partyType = to;
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
        }
        else {
            manualValidation("div_TransactionType", true, "Transaction Type is required.");
            $scope.transactionTypeGL = null;
        }
    };

    //bankService.getBankMasterHouseBankCboList(function (result) {
    //    $scope.bankMasterList = result;
    //});

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if ($scope.LoanSetOffType == 'Single') {
                $scope.voucher.SourceBankAccountTitle = bank.AccountTitle;
                $scope.voucher.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
                $scope.voucher.BankMasterId = bank.BankMasterId;
            }
            else if ($scope.LoanSetOffType == 'Multi') {
                $scope.voucherML.SourceBankAccountTitle = bank.AccountTitle;
                $scope.voucherML.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
                $scope.voucherML.BankMasterId = bank.BankMasterId;
            }

        }
        $scope.hideBankPopUp();
    };

    bankService.getCashMasterCboList(function (result) {
        $scope.cashMasterList = result;
    });

    $scope.getdirectorList = function () {
        $scope.directorList = [];
        $http.get("Parties/party/GetCompanyDirectorDataList")
            .then(function (response) {
                $scope.directorList = response.data.Rows;
            });
    };
    $scope.getdirectorList();

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

    $scope.totalInstallment = function () {
        $scope.voucher.TotalNoOfInstallment = ($scope.voucher.LifeOfYear * $scope.voucher.NoOfInstallmentPerYear);
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

    $scope.changeTransactionType = function (type) {
        $scope.Clear();
        $scope.voucher.TransactionType = type;
    };
    $scope.changePartyTypeFrom = function (type) {
        $scope.voucherML.PartyType = type;
    };
    $scope.loanBankList = [];
    $http({
        method: "GET",
        url: "accounts/Loan/getloanBankListcbo"
    }).then(function successCallback(response) {
        $scope.loanBankList = response.data;
    });
    $scope.validation = function () {
        if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.LoanPostingDate)) {
            ShowResult("Posting date must be below or equal to Loan PostingDate!", "failure");;
            return true;
        }
        if ($scope.voucher.Balance < $scope.voucher.Amount) {
            ShowResult("Payment Amount can't more than Loan Balance Amount", "failure");;
            return true;
        }
        return false;

    };
    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        $scope.passBankCashAmount();
        if ($scope.form0.$valid && !$scope.invalidDocDate && !$scope.invalidPostingDate && !$scope.validation()) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "Accounts/Loan/InsertLoanPayment",
                    data: {
                        "voucherVM": $scope.voucher,
                        "loanAdditionVM": $scope.loanAddition,
                        "loanRepaymentSchedulelist": $scope.loanRepaymentSchedulelist,
                        "taxDetailVMList": $scope.TDSList,
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
                    url: "Accounts/Loan/UpdateLoan",
                    data: {
                        "voucherVM": $scope.voucher,
                        "loanRepaymentSchedulelist": $scope.loanRepaymentSchedulelist
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
                msg = "Posting date must be above or equal to receivable of " + $scope.voucherDetailList[i].DocRefNo;
                $scope.invalidPostingDate = true;
                break;
            }
            else {
                $scope.invalidPostingDate = false;
            }
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
                $scope.totalAdvanceAmount(party.Id, party.UserName);
            }
        }
        $scope.hidePartyPopUp();
    };

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher = {};
        $scope.voucher.Active = true;
        $scope.voucher.Amount = "";
        $scope.voucher.CurrencyId = null;
        $scope.voucher.IsSchedule = false;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.PaymentSource = "Bank";
        $scope.voucher.PartyType = "Customer";
        $scope.voucher.TransactionType = "LoanTaken";
        $scope.currencyExchangeRate = [];
        $scope.loanAddition = {};
        $scope.getCboVoucherTypeLoanList();
        $scope.loanRepaymentSchedulelist = [];
        $("#loanDetails").children().remove();
        $scope.isReadOnly = false;
    };

    $scope.clearSchedule = function () {
        $("#loanDetails").children().remove();
        $scope.voucher.RepaymentStartDate = null;
        $scope.voucher.LifeOfYear = null;
        $scope.voucher.ProfitRate = null;
        $scope.voucher.NoOfInstallmentPerYear = null;
        $scope.voucher.TotalNoOfInstallment = null;
    }
    $scope.report = function (voucherId) {
        location.href = "accounts/Loan/LoanReport?voucherId=" + voucherId;
    };


    $scope.financingId = null;
    $scope.confirmPost = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_confirmation = 'Are you sure to Post?';
        angular.element(document.querySelector('#confirmPostPopUp')).modal('show');
    };

    $scope.post = function (voucherId) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "voucherId": voucherId
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
    $scope.distributedTotalAmount = function (amount) {
        if ($scope.voucher.InterestBalance == 0) {
            $scope.voucher.Amount = amount;
        }
        else if ($scope.voucher.InterestBalance > amount || $scope.voucher.InterestBalance == amount) {
            $scope.voucher.InterestPaymentAmount = amount;
            $scope.voucher.Amount = 0;
        }
        else if ($scope.voucher.InterestBalance < amount) {
            $scope.voucher.InterestPaymentAmount = $scope.voucher.InterestBalance;
            if ($scope.voucher.Balance > (amount - $scope.voucher.InterestBalance))
                $scope.voucher.Amount = amount - $scope.voucher.InterestBalance;
            else
                $scope.voucher.Amount = $scope.voucher.Balance;
        }
    }
    $scope.InterestPaymentAmountValidation = function (amount) {
        if ($scope.voucher.InterestBalance < amount) {
            $scope.voucher.InterestPaymentAmount = $scope.voucher.InterestBalance;
            ShowResult("Interest Payment Amount should not exceed Interest Balance Amount.", "failure");
        }
    }
    $scope.loanDataList = [];
    $scope.getPopUpData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/Loan/GetLoanPopUpList?transactionType=' + $scope.voucher.TransactionType
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
        var data = x.data;
        $scope.voucher.FinancingId = data.FinancingId;
        $scope.voucher.FinancingDetailId = data.FinancingDetailId;
        $scope.voucher.FinancingTypeId = data.FinancingTypeId;
        $scope.voucher.VoucherNo = data.VoucherNo;
        $scope.voucher.PartyName = data.Particulars;
        $scope.voucher.PartyId = data.PartyId;
        $scope.voucher.PartyType = data.PartyType;
        $scope.voucher.PartyPlantName = data.PartyPlantName;
        $scope.voucher.CurrencyId = data.CurrencyId;
        $scope.voucher.CurrencyCode = data.CurrencyCode;
        $scope.voucher.EntityId = data.EntityId;
        $scope.voucher.CompanyId = data.CompanyId;
        $scope.voucher.PlantId = data.PlantId;
        $scope.voucher.LoanAmount = data.LoanAmount;
        $scope.voucher.LoanSetOff = data.LoanPayment;
        $scope.voucher.InitialSactionAmount = data.InitialSactionAmount;
        $scope.voucher.AdditionalLoanAmount = data.AdditionalLoanAmount;
        $scope.voucher.TotalInterestPayableAmount = data.InterestAmount;
        $scope.voucher.InterestAmount = data.InterestAmount - data.OtherExpensesPayable;
        $scope.voucher.OtherExpensesPayable = data.OtherExpensesPayable;
        $scope.voucher.Balance = data.Balance;
        $scope.voucher.LoanDocRefNo = data.Particulars + "-" + data.DocRefNo;
        $scope.voucher.LoanPostingDate = data.PostingDate;
        $scope.voucher.LoanDocDate = data.DocDateNew;
        $scope.voucher.InterestWriteOff = data.InterestWriteOff;
        $scope.voucher.InterestBalance = data.InterestBalance;
        $scope.voucher.InterestCashPayment = data.InterestCashPayment;

        $scope.voucher.OtherBankMasterId = data.OtherBankMasterId;
        $scope.voucher.ToCurrencyRate = data.CompanyCurrencyRate;
        $scope.getPartyPlantList(data.PartyId);
        $scope.voucher.PartyPlantId = data.PartyPlantId;
        $scope.voucher.TotalAmount = '';
        $scope.voucher.InterestPaymentAmount = '';
        $scope.voucher.InterestCashAmount = '';
        $scope.GetCurrencyExchangeRateList();
        angular.element(document.querySelector("#loanPopUp")).modal("hide");
    };
    $scope.showAdditionloanPopUp = function () {
        $scope.getPopUpData();
        angular.element(document.querySelector('#additionLoanPopUp')).modal('show');
    };
    $scope.closeAdditionloanPopUp = function () {
        angular.element(document.querySelector("#additionLoanPopUp")).modal("hide");
    };
    $scope.closeAdditionloanPopUpSelected = function (x) {
        var data = x.data;
        $scope.loanAddition.FinancingId = data.FinancingId;
        $scope.loanAddition.FinancingDetailId = data.FinancingDetailId;
        $scope.loanAddition.FinancingTypeId = data.FinancingTypeId;
        $scope.loanAddition.VoucherNo = data.VoucherNo;
        $scope.loanAddition.PartyName = data.Particulars;
        $scope.loanAddition.PartyId = data.PartyId;
        $scope.loanAddition.PartyType = data.PartyType;
        $scope.loanAddition.PartyPlantName = data.PartyPlantName;
        $scope.loanAddition.CurrencyId = data.CurrencyId;
        $scope.loanAddition.CurrencyCode = data.CurrencyCode;
        $scope.loanAddition.EntityId = data.EntityId;
        $scope.loanAddition.CompanyId = data.CompanyId;
        $scope.loanAddition.PlantId = data.PlantId;
        $scope.loanAddition.LoanAmount = data.LoanAmount;
        $scope.loanAddition.LoanSetOff = data.LoanPayment;
        $scope.loanAddition.InitialSactionAmount = data.InitialSactionAmount;
        $scope.loanAddition.AdditionalLoanAmount = data.AdditionalLoanAmount;
        $scope.loanAddition.TotalInterestPayableAmount = data.InterestAmount;
        $scope.loanAddition.InterestAmount = data.InterestAmount - data.OtherExpensesPayable;
        $scope.loanAddition.OtherExpensesPayable = data.OtherExpensesPayable;
        $scope.loanAddition.Balance = data.Balance;
        $scope.loanAddition.LoanDocRefNo = data.Particulars + "-" + data.DocRefNo;
        $scope.loanAddition.LoanPostingDate = data.PostingDate;
        $scope.loanAddition.LoanDocDate = data.DocDateNew;
        $scope.loanAddition.InterestWriteOff = data.InterestWriteOff;
        $scope.loanAddition.InterestBalance = data.InterestBalance;
        $scope.loanAddition.InterestCashPayment = data.InterestCashPayment;
        $scope.loanAddition.OtherBankMasterId = data.OtherBankMasterId;
        $scope.loanAddition.ToCurrencyRate = data.CompanyCurrencyRate;
        $scope.loanAddition.PartyPlantId = data.PartyPlantId;
        $scope.loanAddition.TotalAmount = '';
        $scope.loanAddition.InterestPaymentAmount = '';
        $scope.loanAddition.InterestCashAmount = '';
        $scope.loanAddition.Amount = 0;
        $scope.loanAddition.LoanSetOffAmount = 0;
        $scope.voucher.CompanyCurrencyRate = data.CompanyCurrencyRate;
        angular.element(document.querySelector("#additionLoanPopUp")).modal("hide");
    };

    $scope.exchangeGainLossAmount = function (amount) {
        $scope.voucher.LoanSetOffAmount = Math.abs($scope.voucher.Amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
        var balance = parseFloat($scope.voucher.Balance), dramount = parseFloat(amount);
        if (dramount > balance) {
            amount = $scope.voucher.Balance;
            $scope.voucher.Amount = $scope.voucher.Balance;
            ShowResult("Payment Amount should not exceed Loan Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        if ($scope.voucher.PaymentSource == "Loan") {
            $scope.loanAddition.Amount = Math.abs(amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
            if ($scope.loanAddition.Amount < $scope.loanAddition.LoanSetOffAmount) {
                $scope.voucher.ExchangeAmount = Math.abs($scope.loanAddition.LoanSetOffAmount - $scope.loanAddition.Amount).toFixed(2);
                $scope.voucher.ExchangeType = "ExchangeGain";
            }
            else if ($scope.loanAddition.Amount > $scope.loanAddition.LoanSetOffAmount) {
                $scope.voucher.ExchangeAmount = Math.abs($scope.loanAddition.Amount - $scope.loanAddition.LoanSetOffAmount).toFixed(2);
                $scope.voucher.ExchangeType = "ExchangeLoss";
            }
        }
        else {
            if ($scope.voucher.TransactionType == 'LoanTaken') {
                if ($scope.voucher.ToCurrencyRate < $scope.voucher.CompanyCurrencyRate) {
                    $scope.voucher.ExchangeAmount = Math.abs(amount * ($scope.voucher.CompanyCurrencyRate - $scope.voucher.ToCurrencyRate)).toFixed(2);
                    $scope.voucher.ExchangeType = "ExchangeLoss";
                }
                else if ($scope.voucher.ToCurrencyRate > $scope.voucher.CompanyCurrencyRate) {
                    $scope.voucher.ExchangeAmount = Math.abs(amount * ($scope.voucher.ToCurrencyRate - $scope.voucher.CompanyCurrencyRate)).toFixed(2);
                    $scope.voucher.ExchangeType = "ExchangeGain";
                }
            }
            else if ($scope.voucher.TransactionType == 'LoanGiven') {
                if ($scope.voucher.ToCurrencyRate > $scope.voucher.CompanyCurrencyRate) {
                    $scope.voucher.ExchangeAmount = Math.abs(amount * ($scope.voucher.CompanyCurrencyRate - $scope.voucher.ToCurrencyRate)).toFixed(2);
                    $scope.voucher.ExchangeType = "ExchangeLoss";
                }
                else if ($scope.voucher.ToCurrencyRate < $scope.voucher.CompanyCurrencyRate) {
                    $scope.voucher.ExchangeAmount = Math.abs(amount * ($scope.voucher.ToCurrencyRate - $scope.voucher.CompanyCurrencyRate)).toFixed(2);
                    $scope.voucher.ExchangeType = "ExchangeGain";
                }
            }
            else {
                $scope.voucher.ExchangeAmount = 0;
                $scope.voucher.ExchangeType = null;
            }
        }

    };

    $scope.exchangeGainLossCal = function (rate) {
        $scope.voucher.LoanSetOffAmount = Math.abs($scope.voucher.Amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
        if ($scope.voucher.PaymentSource == "Loan") {
            $scope.loanAddition.Amount = Math.abs(amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
            if ($scope.loanAddition.Amount < $scope.loanAddition.LoanSetOffAmount) {
                $scope.voucher.ExchangeAmount = Math.abs($scope.loanAddition.LoanSetOffAmount - $scope.loanAddition.Amount).toFixed(2);
                $scope.voucher.ExchangeType = "ExchangeGain";
            }
            else if ($scope.loanAddition.Amount > $scope.loanAddition.LoanSetOffAmount) {
                $scope.voucher.ExchangeAmount = Math.abs($scope.loanAddition.Amount - $scope.loanAddition.LoanSetOffAmount).toFixed(2);
                $scope.voucher.ExchangeType = "ExchangeLoss";
            }
        }
        else {
            if ($scope.voucher.TransactionType == 'LoanGiven') {
                if ($scope.voucher.ToCurrencyRate > rate) {
                    $scope.voucher.ExchangeAmount = Math.abs($scope.voucher.Amount * (rate - $scope.voucher.ToCurrencyRate)).toFixed(2);
                    $scope.voucher.ExchangeType = "ExchangeLoss";
                }
                else if ($scope.voucher.CompanyCurrencyRate < rate) {
                    $scope.voucher.ExchangeAmount = Math.abs($scope.voucher.Amount * ($scope.voucher.ToCurrencyRate - rate)).toFixed(2);
                    $scope.voucher.ExchangeType = "ExchangeGain";
                }
                else {
                    $scope.voucher.ExchangeAmount = 0;
                    $scope.voucher.ExchangeType = null;
                }
            }
            if ($scope.voucher.TransactionType == 'LoanTaken') {
                if ($scope.voucher.ToCurrencyRate < rate) {
                    $scope.voucher.ExchangeAmount = Math.abs($scope.voucher.Amount * (rate - $scope.voucher.ToCurrencyRate)).toFixed(2);
                    $scope.voucher.ExchangeType = "ExchangeLoss";
                }
                else if ($scope.voucher.ToCurrencyRate > rate) {
                    $scope.voucher.ExchangeAmount = Math.abs($scope.voucher.Amount * ($scope.voucher.ToCurrencyRate - rate)).toFixed(2);
                    $scope.voucher.ExchangeType = "ExchangeGain";
                }
                else {
                    $scope.voucher.ExchangeAmount = 0;
                    $scope.voucher.ExchangeType = null;
                }
            }
        }
    };
    $scope.exchangeGainLossAmountloanAddition = function () {
        if ($scope.voucher.PaymentSource == "Loan") {
            var balance = parseFloat($scope.voucher.Balance), dramount = parseFloat($scope.loanAddition.LoanSetOffAmount);
            if (dramount > balance) {
                $scope.loanAddition.LoanSetOffAmount = $scope.voucher.Balance;
                ShowResult("Payment Amount should not exceed Loan Amount.", "failure");
            }
            else {
                CloseShowResult();
            }
            if ($scope.voucher.PaymentSource == "Loan") {
                if ($scope.loanAddition.Amount < $scope.loanAddition.LoanSetOffAmount) {
                    $scope.voucher.ExchangeAmount = Math.abs($scope.loanAddition.LoanSetOffAmount - $scope.loanAddition.Amount).toFixed(2);
                    $scope.voucher.ExchangeType = "ExchangeGain";
                }
                else if ($scope.loanAddition.Amount > $scope.loanAddition.LoanSetOffAmount) {
                    $scope.voucher.ExchangeAmount = Math.abs($scope.loanAddition.Amount - $scope.loanAddition.LoanSetOffAmount).toFixed(2);
                    $scope.voucher.ExchangeType = "ExchangeLoss";
                }
                else {
                    $scope.voucher.ExchangeAmount = 0;
                    $scope.voucher.ExchangeType = null;
                }
            }
        }
    };
    $scope.exchangeGainLossAmountChangeBooksAmount = function () {
        if ($scope.voucher.IsSplit == true) {
            var balance = parseFloat(($scope.voucher.Balance * $scope.voucher.CompanyCurrencyRate).toFixed(2)), dramount = parseFloat($scope.voucher.LoanSetOffAmount), booksLoanAmount = parseFloat(($scope.voucher.Amount * $scope.voucher.ToCurrencyRate).toFixed(2));
            if (dramount > balance) {
                $scope.voucher.LoanSetOffAmount = balance;
                ShowResult("Payment Amount should not exceed Loan Amount.", "failure");
            }
            else {
                CloseShowResult();
            }

            if (booksLoanAmount > $scope.voucher.LoanSetOffAmount) {
                $scope.voucher.ExchangeAmount = Math.abs(booksLoanAmount - $scope.voucher.LoanSetOffAmount).toFixed(2);
                $scope.voucher.ExchangeType = "ExchangeGain";
            }
            else if (booksLoanAmount < $scope.voucher.LoanSetOffAmount) {
                $scope.voucher.ExchangeAmount = Math.abs($scope.voucher.LoanSetOffAmount - booksLoanAmount).toFixed(2);
                $scope.voucher.ExchangeType = "ExchangeLoss";
            }
            else {
                $scope.voucher.ExchangeAmount = 0;
                $scope.voucher.ExchangeType = null;
            }

        }
    };

    $scope.searchglByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
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
        }
    ];

    $scope.glListParameters = {
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

    $scope.GetCOAICodeList = function () {
        if ($scope.voucher.TransactionType == 'LoanTaken')
            $scope.GLUrl1 = "Accounts/glitem/GetExpenseGLBudgetActivity";
        else
            $scope.GLUrl1 = "Accounts/glitem/GetRevenueGLBudgetActivity";
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };
    $scope.setSelected = function (data) {
        $scope.voucher.GLName = data.GLGeneralInfoId + ' - ' + data.BudgetName + ' - ' + data.ActivityName;
        $scope.voucher.ActivityId = data.ActivityId;
        $scope.voucher.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.voucher.BudgetMasterId = data.BudgetMasterId;
        $scope.closeCOAICodeListPopUp();
    };

    $scope.clearGLData = function () {
        $scope.voucher.GLName = null;
        $scope.voucher.ActivityId = null;
        $scope.voucher.GLGeneralInfoId = null;
        $scope.voucher.BudgetMasterId = null;
        $scope.voucher.ExpenseAmount = '';
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

    $scope.advanceId = null;
    $scope.confirmDelete = function (financingId, voucherId) {
        $scope.financingId = financingId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };



    //******Multiple Loan SetOff*******
    $scope.voucherML = {
        Id: null,
        EntityId: null,
        PartyId: null,
        PartyName: null,
        PartyType: "Customer",
        CurrencyId: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        Amount: "",
        Narration: null,
        BankName: null,
        BankMasterId: null,
        OtherBankMasterId: null,
        CashName: null,
        CashMasterId: null,
        BankCurrencyId: null,
        BankAmount: null,
        BankAccountNumber: null,
        PaymentSource: "Bank",
        FinancingTypeId: null,
        RepaymentStartDate: null,
        LifeOfYear: "",
        NoOfInstallmentPerYear: "",
        TotalNoOfInstallment: "",
        ProfitRate: "",
        ProfitAmount: "",
        TransactionType: "LoanTaken",
        IsSchedule: false,
        CompanyCurrencyRate: 1,
        PartyType: "Bank",
        BankId: null
    };

    $scope.getCboVoucherTypeLoanListML = function () {
        accountService.getCboVoucherTypeLoanPaymentList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucherML.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucherML.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucherML.DocDate = $scope.voucherML.PostingDate;
            }
        });
    }
    $scope.getPartyPlantListML = function (partyId, isUpdateMode) {
        $scope.partyPlantList = [];
        $http.get("Parties/party/GetPartyPlantCbo?partyId=" + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault && !isUpdateMode) {
                        $scope.voucherML.PartyPlantId = item.Value;
                    }
                });
            });
    };

    $scope.closeCustomerPopUpML = function () {
        if ($scope.customerIndex !== -1) {
            var party = $scope.customerList[$scope.customerIndex];
            $scope.voucherML.PartyName = party.Code + " - " + party.UserName;
            $scope.voucherML.PartyId = party.Id;
            $scope.voucherML.PartyType = $scope.partyType;
            $scope.getPartyPlantListML(party.Id);
        }
        angular.element(document.querySelector("#customerListPopUp")).modal("hide");
        $scope.customerIndex = -1;
        $scope.selectedCustomer = null;
    };
    $scope.changeSourceToML = function (to) {
        $scope.voucherML.DrBankMasterId = null;
        $scope.voucherML.PartyName = null;
        $scope.voucherML.DirectorName = null;
        $scope.partyType = to;
    };
    $scope.changeSourceFromML = function (from) {
        $scope.voucherML.CrGLId = null;
        $scope.voucherML.CrGLName = null;
        $scope.voucherML.CrBudgetId = null;
        $scope.voucherML.CrActivityId = null;
        $scope.voucherML.BankName = null;
        $scope.voucherML.CashName = null;
        $scope.voucherML.BankMasterId = null;
        $scope.voucherML.CashMasterId = null;
        $scope.isBankAmount = false;
        $scope.voucherML.BankCurrencyId = null;
    };
    $scope.validation = function () {
        for (var i = 0; i < $scope.ExistingLoanList.length; i++) {
            if (new Date($scope.voucher.PostingDate) < new Date($scope.ExistingLoanList[i].LoanPostingDate)) {
                ShowResult("Posting date must be below or equal to Loan PostingDate!", "failure");;
                return true;
                break;
            }
            if ($scope.ExistingLoanList[i].Balance < $scope.ExistingLoanList[i].Amount) {
                ShowResult("Payment Amount can't more than Loan Balance Amount", "failure");;
                return true;
                break;
            }
            return false;
        }
    };

    $scope.SaveML = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDateML();
        $scope.checkPostingDateML();
        //$scope.passBankCashAmount();
        if ($scope.form1.$valid && !$scope.invalidDocDateML && !$scope.invalidPostingDateML && !$scope.validation()) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "Accounts/Loan/InsertMultiLoanPayment",
                    data: {
                        "voucherVM": $scope.voucherML,
                        "loanRepaymentlist": $scope.ExistingLoanList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.ClearML();
                        $scope.isReadOnly = false;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            return true;
        }
    };

    $scope.invalidDocDateML = false;
    $scope.checkDocDateML = function () {
        var msg = "";
        if (new Date($scope.voucherML.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (new Date($scope.voucherML.PostingDate) < new Date($scope.voucherML.DocDate)) {
            msg = "Doc date must be below or equal to Posting Date!";
            $scope.invalidDocDateML = true;
        }
        else if (baseService.isUndefinedOrNull($scope.voucherML.DocDate)) {
            msg = "Doc Date is required.";
            $scope.invalidDocDateML = true;
        }
        else $scope.invalidDocDateML = false;
        return manualValidation("div_DocDate", $scope.invalidDocDateML, msg);
    };

    $scope.invalidPostingDateML = false;
    $scope.checkPostingDateML = function () {
        var msg = "";
        if (new Date($scope.voucherML.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.voucherML.PostingDate)) {
            msg = "Posting Date is required.";
            $scope.invalidPostingDateML = true;
        }
        else {
            $scope.invalidPostingDateML = false;
        }
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if (new Date($scope.voucherDetailList[i].PostingDate) > new Date($scope.voucherML.PostingDate)) {
                msg = "Posting date must be above or equal to receivable of " + $scope.voucherDetailList[i].DocRefNo;
                $scope.invalidPostingDateML = true;
                break;
            }
            else {
                $scope.invalidPostingDateML = false;
            }
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.closePartyPopUpML = function () {
        if (baseService.isUndefinedOrNull($scope.voucherML.CurrencyId)) {
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
                $scope.voucherML.PartyName = party.Code + " - " + party.UserName;
                $scope.voucherML.PartyId = party.Id;
                $scope.voucherML.PartyType = $scope.partyType;
                $scope.totalAdvanceAmount(party.Id, party.UserName);
            }
        }
        $scope.hidePartyPopUp();
    };


    $scope.ClearML = function () {
        $scope.Action = "Save";
        $scope.voucherML = {};
        $scope.voucherML.Active = true;
        $scope.voucherML.Amount = "";
        $scope.voucherML.CurrencyId = null;
        $scope.voucherML.IsSchedule = false;
        $scope.voucherML.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucherML.PaymentSource = "Bank";
        $scope.voucherML.PartyType = "Customer";
        $scope.voucherML.TransactionType = "LoanTaken";
        $scope.voucherML.PartyType = "Bank";
        $scope.ExistingLoanList = [];
        $scope.currencyExchangeRate = [];
        $scope.getCboVoucherTypeLoanList();
        $scope.getCboVoucherTypeLoanListML();
        $scope.loanRepaymentSchedulelist = [];
        $("#loanDetails").children().remove();
        $scope.isReadOnly = false;
    };

    $scope.clearSchedule = function () {
        $("#loanDetails").children().remove();
        $scope.voucherML.RepaymentStartDate = null;
        $scope.voucherML.LifeOfYear = null;
        $scope.voucherML.ProfitRate = null;
        $scope.voucherML.NoOfInstallmentPerYear = null;
        $scope.voucherML.TotalNoOfInstallment = null;
    }
    $scope.report = function (voucherId) {
        location.href = "accounts/Loan/LoanReport?voucherId=" + voucherId;
    };


    $scope.financingId = null;
    $scope.confirmPost = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_confirmation = 'Are you sure to Post?';
        angular.element(document.querySelector('#confirmPostPopUp')).modal('show');
    };

    $scope.post = function (voucherId) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "voucherId": voucherId
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
            $scope.voucherML.BaseCurrencyId = $scope.CurrencyParallel[0].CurrencyId;
        });
    };



    $scope.currencyExchangeRateML = [];
    $scope.GetCurrencyExchangeRateListML = function () {
        if (!baseService.isUndefinedOrNull($scope.voucherML.PostingDate) && !baseService.isUndefinedOrNull($scope.voucherML.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucherML.PostingDate + "&currencyId=" + $scope.voucherML.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRateML = response.data;
                $scope.voucherML.CompanyCurrencyRate = $scope.currencyExchangeRateML.ToCurrencyRate;
                $scope.voucherML.ToCurrencyRate = $scope.currencyExchangeRateML.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRateML = null;
        }
    };
    $scope.distributedTotalAmount = function (amount) {
        if ($scope.voucherML.InterestBalance == 0) {
            $scope.voucherML.Amount = amount;
        }
        else if ($scope.voucherML.InterestBalance > amount || $scope.voucherML.InterestBalance == amount) {
            $scope.voucherML.InterestPaymentAmount = amount;
            $scope.voucherML.Amount = 0;
        }
        else if ($scope.voucherML.InterestBalance < amount) {
            $scope.voucherML.InterestPaymentAmount = $scope.voucherML.InterestBalance;
            if ($scope.voucherML.Balance > (amount - $scope.voucherML.InterestBalance))
                $scope.voucherML.Amount = amount - $scope.voucherML.InterestBalance;
            else
                $scope.voucherML.Amount = $scope.voucherML.Balance;
        }
    }
    $scope.InterestPaymentAmountValidation = function (amount) {
        if ($scope.voucherML.InterestBalance < amount) {
            $scope.voucherML.InterestPaymentAmount = $scope.voucherML.InterestBalance;
            ShowResult("Interest Payment Amount should not exceed Interest Balance Amount.", "failure");
        }
    }
    $scope.loanDataListML = [];
    $scope.getPopUpDataML = function () {
        //if ($scope.voucherML.PartyType =="Bank") {
        //    if (baseService.isUndefinedOrNull($scope.voucherML.BankId)) {
        //        ShowResult("Please select Party Type Bank!", "failure");;
        //        return true;
        //    }
        //}
        $http({
            method: 'GET',
            url: 'Accounts/Loan/GetLoanPopUpListML?transactionType=' + $scope.voucherML.TransactionType + "&partyType=" + $scope.voucherML.PartyType + "&bankId=" + $scope.voucherML.BankId
        }).then(function successCallback(response) {
            $scope.loanDataListML = response.data;
            for (var i = 0; i < $scope.loanDataListML.length; i++) {
                response.data[i].PostingDateNew = new Date($scope.loanDataListML[i].PostingDateNew);
                response.data[i].DocDate = new Date($scope.loanDataListML[i].DocDate);
            }
        });
    };
    $scope.showmultiloanPopUp = function () {
        $scope.getPopUpDataML();
        angular.element(document.querySelector('#multiloanPopUp')).modal('show');
    };
    $scope.closemultiloanPopUp = function () {
        angular.element(document.querySelector("#multiloanPopUp")).modal("hide");
    };

    $scope.ExistingLoanList = [];
    $scope.closemultiloanPopUpSelected = function () {
        $scope.ExistingLoanList = [];
        $scope.existingLoan = {};
        for (var i = 0; i < $scope.loanDataListML.length; i++) {
            if ($scope.loanDataListML[i].isSelected == true) {
                if ($scope.ExistingLoanList, $scope.loanDataListML[i].FinancingId) {
                    $scope.existingLoan.FinancingId = $scope.loanDataListML[i].FinancingId;
                    $scope.existingLoan.FinancingDetailId = $scope.loanDataListML[i].FinancingDetailId;
                    $scope.existingLoan.VoucherNo = $scope.loanDataListML[i].VoucherNo;
                    $scope.existingLoan.PartyName = $scope.loanDataListML[i].Particulars;
                    $scope.existingLoan.PartyId = $scope.loanDataListML[i].PartyId;
                    $scope.existingLoan.OtherBankMasterId = $scope.loanDataListML[i].OtherBankMasterId;
                    $scope.existingLoan.PartyType = $scope.loanDataListML[i].PartyType;
                    $scope.existingLoan.PartyPlantName = $scope.loanDataListML[i].PartyPlantName;
                    $scope.existingLoan.CurrencyId = $scope.loanDataListML[i].CurrencyId;
                    $scope.existingLoan.CurrencyCode = $scope.loanDataListML[i].CurrencyCode;
                    $scope.existingLoan.EntityId = $scope.loanDataListML[i].EntityId;
                    $scope.existingLoan.FinancingTypeId = $scope.loanDataListML[i].FinancingTypeId;
                    $scope.existingLoan.CompanyId = $scope.loanDataListML[i].CompanyId;
                    $scope.existingLoan.PlantId = $scope.loanDataListML[i].PlantId;
                    $scope.existingLoan.LoanAmount = $scope.loanDataListML[i].LoanAmount - $scope.loanDataListML[i].AdditionalLoanAmount;
                    $scope.existingLoan.LoanSetOff = $scope.loanDataListML[i].LoanPayment;
                    $scope.existingLoan.Balance = $scope.loanDataListML[i].Balance;
                    $scope.existingLoan.LoanDocRefNo = $scope.loanDataListML[i].DocRefNo;
                    $scope.existingLoan.InitialSactionAmount = $scope.loanDataListML[i].InitialSactionAmount;
                    $scope.existingLoan.AdditionalLoanAmount = $scope.loanDataListML[i].AdditionalLoanAmount;
                    $scope.existingLoan.LoanPostingDate = $scope.loanDataListML[i].PostingDate;
                    $scope.existingLoan.LoanDocDate = $scope.loanDataListML[i].DocDateNew;
                    $scope.existingLoan.InterestWriteOff = $scope.loanDataListML[i].InterestWriteOff;
                    $scope.existingLoan.InterestBalance = $scope.loanDataListML[i].InterestBalance;
                    $scope.existingLoan.InterestCashPayment = $scope.loanDataListML[i].InterestCashPayment;
                    $scope.existingLoan.InterestAmount = $scope.loanDataListML[i].InterestAmount - $scope.loanDataListML[i].OtherExpensesPayable;
                    $scope.existingLoan.OtherExpensesPayable = $scope.loanDataListML[i].OtherExpensesPayable;
                    $scope.existingLoan.TotalLoanLiability = $scope.loanDataListML[i].LoanAmount + $scope.existingLoan.InterestAmount + $scope.existingLoan.OtherExpensesPayable
                    $scope.existingLoan.TotalInterestPayableAmount = $scope.loanDataListML[i].InterestAmount;
                    $scope.existingLoan.ToCurrencyRate = $scope.loanDataListML[i].CompanyCurrencyRate;
                    $scope.getPartyPlantList($scope.loanDataListML[i].PartyId);
                    $scope.existingLoan.PartyPlantId = $scope.loanDataListML[i].PartyPlantId;
                    $scope.ExistingLoanList.push($scope.existingLoan);
                    $scope.existingLoan = {};
                }
            }
        }
        angular.element(document.querySelector("#multiloanPopUp")).modal("hide");
    };

    $scope.exchangeGainLossAmountML = function (data) {
        if ($scope.voucherML.TransactionType == 'LoanTaken') {
            var balance = parseFloat(data.Balance), dramount = parseFloat(data.Amount);
            if (dramount > balance) {
                data.LoanSetOffAmount = data.Balance;
                ShowResult("Invoice Amount should not exceed Balance Amount.", "failure");
            }
            else {
                CloseShowResult();
            }
            if (data.ToCurrencyRate < $scope.voucherML.CompanyCurrencyRate) {
                data.ExchangeAmount = Math.round((data.Amount * ($scope.voucherML.CompanyCurrencyRate - data.ToCurrencyRate)) * 10000 + Number.EPSILON) / 10000;
                data.ExchangeType = "ExchangeLoss";
                data.BaseDrAmount = Math.round((data.Amount * data.ToCurrencyRate) * 10000 + Number.EPSILON) / 10000;

            }
            else if (data.ToCurrencyRate > $scope.voucherML.CompanyCurrencyRate) {
                data.ExchangeAmount = Math.round((data.Amount * (data.ToCurrencyRate - $scope.voucherML.ToCurrencyRate)) * 10000 + Number.EPSILON) / 10000;
                data.ExchangeType = "ExchangeGain";
                data.BaseDrAmount = Math.round((data.Amount * data.ToCurrencyRate) * 10000 + Number.EPSILON) / 10000;
            }
            else {
                data.ExchangeAmount = 0;
                data.BaseDrAmount = Math.round((data.Amount * data.ToCurrencyRate) * 10000 + Number.EPSILON) / 10000;
                data.ExchangeType = null;
            }
        }
    };
    $scope.removeRowML = function (index) {
        $scope.ExistingLoanList.splice(index, 1);
    }

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
}