"use strict";
loanCloseController.$inject = ["accountService", "bankService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function loanCloseController(accountService, bankService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Loan Close";
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
    $scope.bankACType === "Loan"
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
        CompanyCurrencyRate:1
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

    baseService.init("accounts/Loan/GetLoanClosedList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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

   
    $scope.changeTransactionType = function (type) {
        $scope.Clear();
        $scope.voucher.TransactionType = type;
    };
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
       // if ($scope.form0.$valid && !$scope.invalidDocDate && !$scope.invalidPostingDate && !$scope.validation()) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "Accounts/Loan/InsertLoanClose",
                    data: {
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
                        $scope.isReadOnly = false;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            
        //    return true;
        //}
    };

   

   
    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.ExistingLoanList = [];
    };

    $scope.report = function (voucherId) {
        location.href = "accounts/Loan/LoanReport?voucherId=" + voucherId;
    };


   
   
    $scope.loanDataList = [];
    $scope.getPopUpData = function () {        $http({            method: 'GET',            url: 'Accounts/Loan/GetLoanZeroBalanceList?transactionType=' + $scope.voucher.TransactionType        }).then(function successCallback(response) {            $scope.loanDataList = response.data;            for (var i = 0; i < $scope.loanDataList.length; i++) {
                response.data[i].PostingDateNew = new Date($scope.loanDataList[i].PostingDateNew);                response.data[i].DocDate = new Date($scope.loanDataList[i].DocDate);
            }        });    };
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
            $scope.ExistingLoanList.push($scope.existingLoan);
            $scope.existingLoan = {};

            angular.element(document.querySelector("#loanPopUp")).modal("hide");
        }
        else {
            ShowResult(data.DocRefNo + " already  Exist", "failure", "loanPopUp");
        }
    };

    $scope.removeRow = function (index) {
        $scope.ExistingLoanList.splice(index, 1);
    };
}