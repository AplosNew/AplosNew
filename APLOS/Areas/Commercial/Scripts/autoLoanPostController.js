'use strict';
autoLoanPostController.$inject = ["accountService", "bankService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function autoLoanPostController(accountService, bankService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Auto Loan Post";
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
    //$controller("bankBaseController", { $scope: $scope, $http: $http });
    //$controller("cashBaseController", { $scope: $scope, $http: $http });
    $scope.url = "Commercial/AutoLoan";
    $scope.postUrl = "Accounts/Loan/PostLoan";
    $scope.deleteUrl = "Accounts/Loan/DeleteAutoloanPost";
    $(".searchableDDL1").select2();

    $scope.voucher = {
        Id: null,
        EntityId: null,
        PartyId: null,
        PartyName: null,
        PartyType: "Bank",
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
        IsLoanSetOff: false,
        CompanyCurrencyRate:null
    };

    $scope.AutoLoanPostableDataList = [];
    $scope.getAutoLoanPostableList = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Commercial/AutoLoan/GetAutoLoanPostableList',
        }).then(function successCallback(response) {
            $scope.AutoLoanPostableDataList = response.data;
        });
        angular.element(document.querySelector("#autoLoanPopUp")).modal("show");

    };
    $scope.AutoLoanPostableDetailDataList = [];
    $scope.getAutoLoanPostableDetailList = function (LoanAgainstAcceptanceMasterId, SourceType) {
        $scope.AutoLoanPostableDetailDataList = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: "Commercial/AutoLoan/GetAutoLoanPostableDetailList?LoanAgainstAcceptanceMasterId=" + LoanAgainstAcceptanceMasterId + '&SourceType=' + SourceType,
        }).then(function successCallback(response) {
            $scope.AutoLoanPostableDetailDataList = response.data;
        });
    };
    var invoiceAmount = 0;
    $scope.selectAutoLoan = function (x) {
        var autoLoandata = x.data;
        //$scope.voucher.BankMasterId = autoLoandata.BankMasterId;
        //$scope.voucher.AccountTitle = autoLoandata.AccountTitle;
        $scope.voucher.Amount = autoLoandata.Amount;
        $scope.voucher.CurrencyId = autoLoandata.CurrencyId;
        $scope.voucher.BankCurrencyId = autoLoandata.CurrencyId;
        $scope.voucher.DocRefNo = autoLoandata.LoanNo;
        $scope.voucher.PostingDate = $filter("dateFiltering")(autoLoandata.LoanDate);
        $scope.voucher.DocDate = $filter("dateFiltering")(autoLoandata.LoanDate);
        $scope.voucher.TransactionType = autoLoandata.TransactionType;
        $scope.voucher.PaymentSource = autoLoandata.PaymentSource;
        $scope.voucher.PartyType = autoLoandata.PartyType;
        $scope.voucher.PartyName = autoLoandata.PartyName;
        $scope.voucher.PartyId = autoLoandata.PartyId;
        $scope.voucher.PartyPlantId = autoLoandata.PartyPlantId;
        $scope.voucher.GLGeneralInfoId = autoLoandata.GLGeneralInfoId;
        $scope.voucher.BudgetMasterId = autoLoandata.BudgetMasterId;
        $scope.voucher.ActivityId = autoLoandata.ActivityId;
        $scope.voucher.CompanyCurrencyRate = autoLoandata.CompanyCurrencyRate;
        //$scope.voucher.BankBookAmount = Math.round((autoLoandata.Amount * autoLoandata.CompanyCurrencyRate) * 100 + Number.EPSILON) / 100;
        $scope.voucher.BankBookAmount = autoLoandata.BankBookAmount;
        invoiceAmount = autoLoandata.BankBookAmount;
        $scope.voucher.InvoiceId = autoLoandata.InvoiceId;
        $scope.voucher.InvoiceDetailId = autoLoandata.InvoiceDetailId;
        $scope.voucher.AdjustmentNoteId = autoLoandata.AdjustmentNoteId;
        $scope.voucher.AdjustmentNoteDetailId = autoLoandata.AdjustmentNoteDetailId;
        $scope.voucher.LoanAgainstAcceptanceId = autoLoandata.LoanAgainstAcceptanceId;
        $scope.voucher.PurchaseDocAcceptanceId = autoLoandata.PurchaseDocAcceptanceId;
        $scope.voucher.PurchaseLCNo = autoLoandata.PurchaseLCNo;
        $scope.voucher.AcceptanceNo = autoLoandata.AcceptanceNo;
        $scope.voucher.PINo = autoLoandata.PINo;
        $scope.voucher.IsPayment = true;
        $scope.voucher.SourceType = autoLoandata.SourceType;
        $scope.voucher.SettlementType = autoLoandata.SourceType;
        $scope.getPartyPlantList(autoLoandata.PartyId);
        $scope.getAutoLoanPostableDetailList(autoLoandata.LoanAgainstAcceptanceId, autoLoandata.SourceType);
        $scope.closeAutoLoanPopUp();
    }
    $scope.closeAutoLoanPopUp = function () {
        angular.element(document.querySelector("#autoLoanPopUp")).modal("hide");
    }
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
    ];
    baseService.init("Commercial/AutoLoan/GetAutoLoanList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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
       /* cboService.getCboVoucherTypeAutoLoanList(function (result) {*/
            accountService.getCboVoucherTypeLoanList(function (result) {
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
    $scope.bankMasterList = [];
    bankService.getBankMasterHouseBankCboList(function (result) {
        $scope.bankMasterList = result;
    });
   

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
        $scope.voucher.TransactionType = type;
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        $scope.passBankCashAmount();
        $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucher.PostingDate);
        $scope.voucher.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        if ($scope.form0.$valid && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "Commercial/AutoLoan/PostAutoLoan",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.AutoLoanPostableDetailDataList,
                        "existingLoanList": $scope.ExistingLoanList,
                        "loanRepaymentSchedulelist": $scope.loanRepaymentSchedulelist
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
        $scope.voucher.IsPayment = true;
        $scope.voucher.Amount = "";
        $scope.voucher.CurrencyId = null;
        $scope.voucher.IsSchedule = false;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.PaymentSource = "Bank";
        $scope.voucher.PartyType = "Bank";
        $scope.voucher.TransactionType = "LoanTaken";
        $scope.ExistingLoanList = [];
        $scope.currencyExchangeRate = [];
        $scope.getCboVoucherTypeLoanList();
        $scope.loanRepaymentSchedulelist = [];
        $scope.AutoLoanPostableDetailDataList = [];
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
    $scope.bankSearchByList = [
        {
            "name": "Bank",
            "value": "BankName"
        },
        {
            "name": "Bank Branch",
            "value": "BankBranchName"
        },
        {
            "name": "Account Title",
            "value": "AccountTitle"
        },
        {
            "name": "Account Number",
            "value": "AccountNumber"
        },
        {
            "name": "Currency",
            "value": "CurrencyCode"
        }
    ];
    $scope.bankParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "BankName, BankBranchName, AccountTitle",
        searchBy: "AccountNumber",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.showBankPopUp = function (entityId) {
        if (entityId === undefined || entityId === "undefined") {
            entityId = null;
        }
        $scope.getBankList = function (pageno) {
            $scope.url = "Banks/BankMaster/GetBankMasterList?bankACType=Loan&&entityId=" + entityId;
            baseService.paginationBase($scope.url, pageno, $scope.bankParameters)
                .then(function (result) {
                    $scope.bankList = result.Rows;
                    $scope.bankParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getBankList();
        angular.element(document.querySelector("#bankPopUp")).modal("show");
    };
    $scope.selectBankPopUp = function (index, id) {
        $scope.bankIndex = index;
    };
    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];

            $scope.voucher.AccountTitle = bank.AccountTitle;
            $scope.voucher.BankMasterId = bank.BankMasterId;
        }
        $scope.hideBankPopUp();
    };
    $scope.hideBankPopUp = function () {
        angular.element(document.querySelector("#bankPopUp")).modal("hide");
        $scope.bankIndex = -1;
        $scope.bankSelected = null;
    };
    $scope.loanRepaymentSchedulelist = [];

    $scope.LoadRepamentDetail = function () {
        if ($scope.voucher.IsSchedule) {
            $scope.loanRepaymentSchedulelist = [];
            $("#loanDetails").children().remove();
            var numberOfInstallment = $scope.voucher.TotalNoOfInstallment;
            var actualAmount = parseFloat($scope.voucher.Amount);
            var actualAmountWithoutProfit = parseFloat($scope.voucher.Amount);
            var profitAmount = $scope.voucher.ProfitAmount;
            var installmentPerYear = $scope.voucher.NoOfInstallmentPerYear;
            var rate = parseFloat((parseInt($scope.voucher.ProfitRate) / 100) / installmentPerYear);
            var disbursmentDate = $scope.voucher.DocDate;
            var repaymentStartDate = $scope.voucher.RepaymentStartDate;
            // var installmentDate = new Date(repaymentStartDate);
            var installmentDate;
            var payment = 0.00;
            var profit = 0.00;
            var principal = 0.00;

            var totalPayment = 0.00;
            var totalProfit = 0.00;
            var totalPrincipal = 0.00;

            var i = 0;

            var idate;
            var periodHtml = "<div class='SearchResult'> <table><thead><tr><td style='width:220px;'>Installment date</td><td style='width:100px;'>Installment no.</td><td style='text-align:right; width:120px;'>Payment</td><td style='text-align:right; width:120px;'>Profit</td><td style='text-align:right; width:120px;'>Principal</td><td style='text-align:right; width:120px;'>Loan</td></tr></thead>";
            periodHtml += "<tr><td>" + FormatDate(disbursmentDate) + " (Disbursement date)" + "</td><td>" + " " + "</td><td style='text-align:right'>" + payment.toFixed(2) + "</td><td style='text-align:right'>" + profit.toFixed(2) + "</td><td style='text-align:right'>" + principal.toFixed(2) + "</td><td style='text-align:right'>" + actualAmount.toFixed(2) + "</td></tr>";
            for (var i = 1; i <= numberOfInstallment; i++) {
                if (i === 1) {
                    installmentDate = new Date(repaymentStartDate);
                    idate = installmentDate;
                }
                if (i > 1) {
                    installmentDate = new Date((new Date(idate)).setMonth((new Date(idate)).getMonth() + (12 / installmentPerYear)));
                    idate = installmentDate;
                }
                if (rate === "0") {
                    payment = actualAmountWithoutProfit / numberOfInstallment;
                }
                else {
                    payment = PMT(rate, numberOfInstallment, installmentPerYear, parseFloat($scope.voucher.Amount));
                }
                var iRate = parseInt($scope.voucher.ProfitRate) / 100;
                profit = (actualAmount * iRate) / installmentPerYear;

                principal = payment - profit;

                if (i === parseInt(numberOfInstallment)) {
                    actualAmount = parseFloat("0.00");
                }
                else {
                    actualAmount = actualAmount - principal;
                }
                var schedule = new Object({
                    InstallmentNo: i,
                    InstallmentDate: new Date(idate),
                    InstallmentAmount: payment,
                    ProfitAmount: profit,
                    PrincipalAmount: principal,
                    Balance: actualAmount,
                    ScheduleNo: 1
                });
                $scope.loanRepaymentSchedulelist.push(schedule);

                totalPayment = totalPayment + payment;
                totalProfit = totalProfit + profit;
                totalPrincipal = totalPrincipal + principal;

                periodHtml += "<tr><td style ='width:220px;'>" + FormatDate(idate) + "</td><td style ='width:100px;'>" + i + "</td><td style='text-align:right; width:120px;'>" + payment.toFixed(2) + "</td><td style='text-align:right; width:120px;'>" + profit.toFixed(2) + "</td><td style='text-align:right; width:120px;'>" + principal.toFixed(2) + "</td><td style='text-align:right; width:120px;'>" + actualAmount.toFixed(2) + "</td></tr>";
            }
            periodHtml += "<tr><td></td><td></td><td style='text-align:right;font-weight: bold'>" + totalPayment.toFixed(2) + "</td><td style='text-align:right;font-weight: bold'>" + totalProfit.toFixed(2) + "</td><td style='text-align:right;font-weight: bold'>" + totalPrincipal.toFixed(2) + "</td><td></tr></table></div>";
            $("#loanDetails").append(periodHtml);
            $scope.voucher.ProfitAmount = totalProfit.toFixed(2);
            return false;
        }
    };

    function PMT(rate, numberOfInstallment, installmentPerYear, actualAmount) {
        var numberOfYear = numberOfInstallment / installmentPerYear;

        var a = 1 / rate;
        var b = 1 + rate;
        var c = Math.pow(b, numberOfInstallment);
        var d = rate * c;
        var e = 1 / d;

        var pvFactor = a - e;
        var payment = actualAmount / pvFactor;
        return payment;
    }

    function FormatDate(input) {
        var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        var dt = new Date(input);
        return [dt.getDate(), months[dt.getMonth()], dt.getFullYear()].join('-');
    }

    $scope.financingId = null;
    $scope.confirmPost = function (financingId) {
        $scope.financingId = financingId;
        $scope.message_confirmation = 'Are you sure to Post?';
        angular.element(document.querySelector('#confirmPostPopUp')).modal('show');
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

    $scope.advanceId = null;
    $scope.confirmDelete = function (financingId, voucherId) {
        $scope.financingId = financingId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };


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
    $scope.ExistingLoanList = [];
    $scope.existingLoan = {};
    $scope.closeloanPopUpSelected = function (x) {
        var data = x.data;

        var getRow = null;
        getRow = $filter("filter")($scope.ExistingLoanList, { "FinancingId": data.FinancingId });
        if (getRow.length === 0) {
            $scope.existingLoan.FinancingId = data.FinancingId;
            $scope.existingLoan.FinancingDetailId = data.FinancingDetailId;
            //$scopexistingLoaner.FinancingTypeId = data.FinancingTypeId;
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
            $scope.getPartyPlantList(data.PartyId);
            $scope.existingLoan.PartyPlantId = data.PartyPlantId;

            $scope.ExistingLoanList.push($scope.existingLoan);
            $scope.existingLoan = {};

            $scope.voucher.TotalAmount = '';
            $scope.voucher.InterestPaymentAmount = '';
            $scope.voucher.InterestCashAmount = '';
            angular.element(document.querySelector("#loanPopUp")).modal("hide");
        }
        else {
            ShowResult(data.DocRefNo + " already  Exist", "failure", "loanPopUp");
        }
    };

    $scope.removeRow = function (index) {
        $scope.ExistingLoanList.splice(index, 1);
    };

    $scope.exchangeGainLossAmount = function (amount) {
        var  loanAmount = parseFloat(amount);
       
        if ($scope.voucher.TransactionType == 'LoanTaken') {
            if (loanAmount < invoiceAmount) {
                $scope.voucher.ExchangeAmount = Math.abs(invoiceAmount - loanAmount).toFixed(2);
                $scope.voucher.ExchangeType = "ExchangeGain";
            }
            else if (loanAmount > invoiceAmount) {
                $scope.voucher.ExchangeAmount = Math.abs(loanAmount - invoiceAmount).toFixed(2);
                $scope.voucher.ExchangeType = "ExchangeLoss";
            }
            else {
                $scope.voucher.ExchangeAmount = 0;
                $scope.voucher.ExchangeType = null;
            }
        }
        else {
            $scope.voucher.ExchangeAmount = 0;
            $scope.voucher.ExchangeType = null;
        }
    };

    $scope.exchangeGainLossCal = function (rate) {
        if ($scope.voucher.TransactionType == 'LoanGiven') {
            if ($scope.voucher.ToCurrencyRate > rate) {
                $scope.voucher.ExchangeAmount = $scope.voucher.Amount * (rate - $scope.voucher.ToCurrencyRate);
                $scope.voucher.ExchangeType = "ExchangeLoss";
            }
            else if ($scope.voucher.CompanyCurrencyRate < rate) {
                $scope.voucher.ExchangeAmount = $scope.voucher.Amount * ($scope.voucher.ToCurrencyRate - rate);
                $scope.voucher.ExchangeType = "ExchangeGain";
            }
            else {
                $scope.voucher.ExchangeAmount = 0;
                $scope.voucher.ExchangeType = null;
            }
        }
        if ($scope.voucher.TransactionType == 'LoanTaken') {
            if ($scope.voucher.ToCurrencyRate < rate) {
                $scope.voucher.ExchangeAmount = $scope.voucher.Amount * (rate - $scope.voucher.ToCurrencyRate);
                $scope.voucher.ExchangeType = "ExchangeLoss";
            }
            else if ($scope.voucher.ToCurrencyRate > rate) {
                $scope.voucher.ExchangeAmount = $scope.voucher.Amount * ($scope.voucher.ToCurrencyRate - rate);
                $scope.voucher.ExchangeType = "ExchangeGain";
            }
            else {
                $scope.voucher.ExchangeAmount = 0;
                $scope.voucher.ExchangeType = null;
            }
        }
    };

}





