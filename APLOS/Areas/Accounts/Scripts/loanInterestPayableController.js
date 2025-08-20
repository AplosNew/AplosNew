"use strict";
loanInterestPayableController.$inject = ["accountService", "bankService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function loanInterestPayableController(accountService, bankService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Loan Interest Payable";
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
    $scope.deleteUrl = $scope.url + "/DeleteLoanInterestPayable";
    $scope.postUrl = $scope.url + "/PostLoanInterestPayable";

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
        DownPaymentAmount: "",
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
        SourceType: "LoanInterestPayable"
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
    ];

    baseService.init("accounts/Loan/GetLoanInterestPayableList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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
        accountService.getCboVoucherTypeLoanInterestPayableList(function (result) {
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
    $scope.taxCodCboList = [];
    $scope.getTaxCodeInvoiceTriggeringInstanceOthers = function () {
        $http({
            method: "GET",
            url: "accounts/TaxCode/GetTaxCodeForSubsequentLoan?postingDate=" + $filter("dateFiltering")(Date.now())
        }).then(function successCallback(response) {
            $scope.taxCodCboList = response.data;
        });
    };
    $scope.getTaxCodeInvoiceTriggeringInstanceOthers();
    $scope.taxCodDataList = [];
    $scope.addTaxCodeonList = function (item) {
        if (baseService.isUndefinedOrNull($scope.voucher.PostingDate)) {
            ShowResult("Please select Posting Date!", "failure");
            return;
        }
        else if (baseService.isUndefinedOrNull(item)) {
            ShowResult(" Please select Tax Code!", "failure");
            return;
        }
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

            var getRow = $filter("filter")($scope.taxCodDataList, { "TaxCodeId": ob.TaxCodeId });
            if (getRow.length === 0) {
                $scope.taxCodDataList.push(ob);
            }
            else {
                ShowResult("Tax code (<b>" + ob.UserName + "</b>) is already added !!!", "failure");
            }
            
        });
    };
    $scope.taxCodeDelModal = function (taxcodeid, username) {
        $scope.TaxCodeId = taxcodeid;
        if (baseService.isUndefinedOrNull($scope.TaxCodeId))
            $scope.Taxmessage_confirmation = "Are you sure want to delete [ " + username + " ] data....";
        else
            $scope.Taxmessage_confirmation = "Are you sure want to delete [ " + username + " ] ?";
        angular.element(document.querySelector("#confirmTaxCodeDelPopUp")).modal("show");
    };

    $scope.removeTaxCodeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.TaxCodeId)) {
            for (var i = 0; i < $scope.taxCodDataList.length; i++) {
                if ($scope.taxCodDataList[i].TaxCodeId === $scope.TaxCodeId) {
                    $scope.taxCodDataList.splice(i, 1);
                }
            }
            $scope.TaxCodeId = "";
        }
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

    bankService.getBankMasterHouseBankCboList(function (result) {
        $scope.bankMasterList = result;
    });

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
    $scope.passTaxAmount = function () {
        if ($scope.voucher.SourceType === "LoanTax") {
            $scope.voucher.Amount = Math.round($filter("sumByKey")($filter("filter")($scope.taxCodDataList), "TaxAmount") * 1000 + Number.EPSILON) / 1000;
        }
    };

    $scope.changeTransactionType = function (type) {
        $scope.Clear();
        $scope.voucher.TransactionType = type;
    };

    $scope.changeSourceType = function (type) {
        $scope.voucher.SourceType = type;
    };
    $scope.validation = function () {
        if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.LoanPostingDate)) {
            ShowResult("Posting date must be below or equal to Loan PostingDate!", "failure");;
            return true;
        }
        return false;
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        $scope.passBankCashAmount();
        $scope.passTaxAmount();
        if ($scope.form0.$valid && !$scope.invalidDocDate && !$scope.invalidPostingDate && !$scope.validation()) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "Accounts/Loan/InsertLoanInterestPayable",
                    data: {
                        "voucherVM": $scope.voucher,
                        "loanRepaymentSchedulelist": $scope.loanRepaymentSchedulelist,
                        "invoiceTaxVMList": $scope.taxCodDataList,
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
        $scope.voucher.SourceType = "LoanInterestPayable";
        $scope.currencyExchangeRate = [];
        $scope.taxCodDataList = [];
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


    $scope.voucherId = null;
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

   
   
   

   
    $scope.loanDataList = [];
    $scope.getPopUpData = function () {        $http({            method: 'GET',            url: 'Accounts/Loan/GetLoanPopUpList?transactionType=' + $scope.voucher.TransactionType        }).then(function successCallback(response) {            $scope.loanDataList = response.data;            for (var i = 0; i < $scope.loanDataList.length; i++) {
                response.data[i].PostingDateNew = new Date($scope.loanDataList[i].PostingDateNew);                response.data[i].DocDate = new Date($scope.loanDataList[i].DocDate);
            }        });    };
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
        $scope.voucher.InterestWriteOff = data.InterestWriteOff;
        $scope.voucher.InterestBalance = data.InterestBalance;
        $scope.voucher.InterestCashPayment = data.InterestCashPayment;
        $scope.voucher.Balance = data.Balance;
        $scope.voucher.LoanPostingDate = data.PostingDate;
        $scope.voucher.LoanDocDate = data.DocDateNew;
        $scope.voucher.LoanDocRefNo = data.DocRefNo;
        $scope.voucher.OtherBankMasterId = data.OtherBankMasterId;
        $scope.voucher.ToCurrencyRate = data.CompanyCurrencyRate;
        $scope.voucher.CompanyCurrencyRate = data.CompanyCurrencyRate;
        $scope.getPartyPlantList(data.PartyId);
        $scope.voucher.PartyPlantId = data.PartyPlantId;
        //$scope.GetCurrencyExchangeRateList();
        angular.element(document.querySelector("#loanPopUp")).modal("hide");
    };

    $scope.exchangeGainLossAmount = function (amount) {
        var balance = parseFloat($scope.voucher.LoanAmount), dramount = parseFloat(amount);
        if (dramount > balance) {
            amount = $scope.voucher.LoanAmount;
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
        if ($scope.voucher.TransactionType =='LoanTaken')
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
        $scope.voucher.GLName = data.GLGeneralInfoId + ' - ' + data.BudgetName+' - '+data.ActivityName;
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

    //loanIntPayableId, financingId
    $scope.delete = function (loanIntPayableId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "loanIntPayableId": loanIntPayableId, "voucherId": voucherId   //"financingId": financingId,
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
                //$scope.financingId = null;
                $scope.loanIntPayableId = null;

                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };



    // Loan interest Payable Delete option 
     $scope.loanIntPayableId = null;
     $scope.voucherId = null;
    //$scope.intPayableId = null;
    //$scope.vId = null;
    $scope.confirmDelete = function (loanIntPayableId, voucherId) {
        $scope.loanIntPayableId = loanIntPayableId;
        $scope.voucherId = voucherId;

        //$scope.intPayableId = loanIntPayableId;
        //$scope.vId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

}