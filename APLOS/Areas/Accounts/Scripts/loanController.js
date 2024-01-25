"use strict";
loanController.$inject = ["accountService", "bankService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function loanController(accountService, bankService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Loan";
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
    $scope.postUrl = $scope.url + "/PostLoan";
    $scope.deleteUrl = $scope.url + "/DeleteLoan";
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
        ExpectedCloseDate: null,
        DocRefNo: null,
        Amount: "",
        DownPaymentAmount:"",
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
        IsPayment: true,
        OrderSpecific: 'No',
        OrderSpecificPartyName: null,
        OrderSpecificPartyId: null
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
        },
        {
            "name": "Status",
            "value": "Status"
        }
    ];

    baseService.init("accounts/Loan/GetLoanList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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

    $scope.getFinancingType = function () {
        cboService.getCboOtherFinancingType($scope.sourceType, function (result) {
            $scope.financingTypeList = result;
        });
    }
    $scope.getFinancingType();

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

    $scope.closePartyPopUp_Loan = function (x) {
        var party = x.data;
        $scope.voucher.PartyName = party.Code + " - " + party.UserName;
        $scope.voucher.PartyId = party.Id;
        $scope.voucher.PartyType = $scope.partyType;
        $scope.getPartyPlantList(party.Id);
        $scope.closePartyPopUpNew_Loan();
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
    $scope.changeOrderSpecific = function (to) {
        $scope.partyType = to;
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
    //$scope.loanbankMasterList = [];
    //bankService.getBankMasterLoanBankCboList(function (result) {
    //    $scope.loanbankMasterList = result;
    //});

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


    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];

            $scope.voucher.SourceBankAccountTitle = bank.AccountTitle;
            $scope.voucher.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
            $scope.voucher.OtherBankMasterId = bank.BankMasterId;
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
    $scope.calTotalLoanAmount = function () {
        var voucherAmount = 0;
        var DownPaymentAmount = 0;
        if ($scope.voucher.Amount != '' && $scope.voucher.Amount != undefined) {
            voucherAmount = parseFloat($scope.voucher.Amount);
        }
        if ($scope.voucher.DownPaymentAmount != '' && $scope.voucher.DownPaymentAmount != undefined) {
            DownPaymentAmount = parseFloat($scope.voucher.DownPaymentAmount);
        }
        $scope.voucher.TotalLoanAmount = (voucherAmount + DownPaymentAmount);
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
        if ($scope.voucher.OrderSpecific === "Yes" && $scope.selectedMasterOrderList.length ===0) {
            ShowResult("Please select Master Order!", 'failure');
            return;
        }
        if ($scope.voucher.FinancingTypeId === null ) {
            ShowResult("Please select Loan Type!", 'failure');
            return;
        }
        if ($scope.form0.$valid && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "Accounts/Loan/InsertLoan",
                    data: {
                        "voucherVM": $scope.voucher,
                        "existingLoanList": $scope.ExistingLoanList,
                        "loanRepaymentSchedulelist": $scope.loanRepaymentSchedulelist,
                        "financingMasterOrderlist": $scope.selectedMasterOrderList,
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
                        "existingLoanList": $scope.ExistingLoanList,
                        "loanRepaymentSchedulelist": $scope.loanRepaymentSchedulelist,
                        "financingMasterOrderlist": $scope.selectedMasterOrderList,
                        "bankChargeDetailVMList": $scope.bankChargesList
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

    $scope.invalidExpectedCloseDate = false;
    $scope.checkExpectedCloseDate = function () {
        var msg = "";
        if (new Date($scope.voucher.ExpectedCloseDate) < new Date()) {
            $scope.invalidExpectedCloseDate = true;
            msg = "Expected Close Date must be above to current Date!";
        }
        else $scope.invalidExpectedCloseDate = false;
        return manualValidation("div_ExpectedCloseDate", $scope.invalidExpectedCloseDate, msg);
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
        $scope.voucher.FinancingTypeId = null;
        $scope.voucher.IsSchedule = false;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.PaymentSource = "Bank";
        $scope.voucher.PartyType = "Bank";
        $scope.voucher.TransactionType = "LoanTaken";
        $scope.voucher.OrderSpecific = "No";
        $scope.ExistingLoanList = [];
        $scope.currencyExchangeRate = [];
        $scope.getFinancingType();
        $scope.getCboVoucherTypeLoanList();
        $scope.loanRepaymentSchedulelist = [];
        $scope.selectedMasterOrderList = [];
        $("#loanDetails").children().remove();
        $scope.isReadOnly = false;
        $scope.bankChargesList = [];
        $scope.bankCharge = {};
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
        getRow = $filter("filter")($scope.ExistingLoanList, {"FinancingId": data.FinancingId });
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
            $scope.GetCurrencyExchangeRateList();
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
        var balance = parseFloat($scope.voucher.Balance), dramount = parseFloat(amount);
        if (dramount > balance) {
            amount = $scope.voucher.LoanAmount;
            $scope.voucher.Amount = $scope.voucher.LoanAmount;
            ShowResult("Payment Amount should not exceed Loan Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
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
    $scope.exchangeGainLossAmountExistingLoan = function (data) {
        var balance = parseFloat(data.Balance), dramount = parseFloat(data.LoanSetOffAmount);
        if (dramount > balance) {
            data.LoanSetOffAmount = data.Balance;
            ShowResult("Loan SetOff Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
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
        
    };
    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        if (baseService.isUndefinedOrNull(party.DownPaymentGLId)) {
            ShowResult("Customer DownPaymentGL not found!", "failure", "partyPopUp");
            return;
        }
        else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.DownPaymentBudgetId)) {
            ShowResult("Customer budget not found!", "failure", "partyPopUp");
            return;
        }
        else if (baseService.isUndefinedOrNull(party.CurrencyId)) {
            ShowResult('Customer transaction currency not found!', 'failure', 'partyPopUp');
            return;
        }
        
        $scope.voucher.OrderSpecificPartyId = party.Id;
        $scope.voucher.OrderSpecificPartyName = party.Code + " - " + party.UserName;
       
        $scope.hidePartyPopUp();
    };
    $scope.ShowResultMasterOrderPopUp = function () {
        $scope.GetMasterOrderList();
        angular.element(document.querySelector('#masterOrderPopUp')).modal('show');
    }
    $scope.masterOrderList = [];
    $scope.selectedMasterOrderList = [];
    $scope.GetMasterOrderList = function () {
        $scope.masterOrderList = [];
        $http({
            method: 'GET',
            url: "accounts/CustomerInvoice/GetMasterOrderListByPartyId?partyId=" + $scope.voucher.OrderSpecificPartyId
        }).then(function (response) {
            $scope.masterOrderList = response.data;
            if (baseService.arrayLength($scope.selectedMasterOrderList) > 0) {
                for (var i = 0; i < $scope.selectedMasterOrderList.length; i++) {
                    for (var j = 0; j < $scope.masterOrderList.length; j++) {
                        if ($scope.selectedMasterOrderList[i].MasterOrderId === $scope.masterOrderList[j].MasterOrderId) {
                            $scope.masterOrderList[j].Active = true;
                        }
                    }
                }
            }
        });
    }
   
    $scope.CloseMasterOrder = function () {
        MakeData();
        angular.element(document.querySelector('#masterOrderPopUp')).modal('hide');
    }
    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridOperation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.masterOrderList.length; i++) {
                $scope.masterOrderList[i].Active = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].Active = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridOperation").data("ejGrid");
        gridObj.refreshContent();
    };
    function MakeData() {
        $scope.selectedMasterOrderList = [];
        try {
            for (var i = 0; i < $scope.masterOrderList.length; i++) {
                var getRow = $filter("filter")($scope.selectedMasterOrderList, { "selectedMasterOrderList": $scope.masterOrderList[i].MasterOrderId });
              
                if (getRow.length == 0) {
                    if ($scope.masterOrderList[i].Active == true) {
                        var ob = {};
                        ob.MasterOrderId = $scope.masterOrderList[i].MasterOrderId;
                        ob.PartyId = $scope.masterOrderList[i].PartyId;
                            if (checkExistList($scope.selectedMasterOrderList, ob.MasterOrderId) === false) {
                                ob.Active = $scope.masterOrderList[i].Active;
                                ob.CustomerName = $scope.masterOrderList[i].CustomerName;
                                ob.InvoicingPartyPlant = $scope.masterOrderList[i].InvoicingPartyPlant;
                                ob.DeliveryPartyPlant = $scope.masterOrderList[i].DeliveryPartyPlant;
                                ob.Type = $scope.masterOrderList[i].Type;
                                ob.Currency = $scope.masterOrderList[i].Currency;
                                $scope.selectedMasterOrderList.push(ob);
                            }
                    }
                }

            }
            
        } catch (e) {
            ShowResult(e, 'failure', 'masterOrderPopUp');
        }
    }
    function checkExistList(list, MasterOrderId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MasterOrderId === MasterOrderId) {
                return true;
            }
        }
        return false;
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
    $scope.rateChangeBankCharge = function (rate) {
        $scope.bankCharge.CompanyCurrencyAmount = $scope.bankCharge.Amount * rate;
        if ($scope.bankChargesList.length !== null) {
            for (var i = 0; i < $scope.bankChargesList.length; i++) {
                $scope.bankChargesList[i].CompanyCurrencyAmount = $scope.bankChargesList[i].Amount * rate;
            }
        }
    };
}