'use strict';
fullandfinalSettlementPaymentController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function fullandfinalSettlementPaymentController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Final Settlement Payments';
    $scope.path = 'Payrolls/FinalSettlement/';

    $scope.tabVoucher = 1;
    $scope.setTabVoucher = function (newTabVoucher) {
        $scope.tabVoucher = newTabVoucher;
    };

    $scope.isSetVoucher = function (tabNumVoucher) {
        return $scope.tabVoucher === tabNumVoucher;
    };

    $scope.voucher = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        ExpenseBookingId: null,
        EmployeeId: null,
        EmployeeName: null,
        EmployeeCodeName: null,
        PartyGLGeneralInfoId: null,
        EmployeeTransactionTypeId: null,
        GLGeneralInfoId: null,
        CurrencyId: null,
        EntityId: null,
        PlantId: null,
        CurrencyCode: null,
        VoucherTypeId: null,
        PartyType: "Employee",
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: $filter("dateFiltering")(Date.now()),
        DocRefNo: null,
        DocDate: $filter("dateFiltering")(Date.now()),
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
        PaymentMode: '',
        IsActive: true,
        IsSeperated: false,
        IsMaternity: false
    };

    $scope.FinalSettlementList = [];
    $scope.getMasterData = function () {
        try {
            $http.get('Payrolls/FinalSettlement/GetFNFApprovedMasterData')
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.FinalSettlementList = response.data;
                        angular.element(document.querySelector('#DisbursementAdvicepopUp')).modal('show');
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.masterId = null;
    $scope.SelectMaster = function (obj) {
        $scope.masterId = obj.data.Id;
        $scope.voucher.DisbursementAdviceId = obj.data.Id;
        $scope.GetemployeeDisbursement();
        angular.element(document.querySelector('#DisbursementAdvicepopUp')).modal('hide');
    };

    $scope.SelectedEmployeeList = [];
    $scope.GetemployeeDisbursement = function () {
        $scope.SelectedEmployeeList = [];
        try {
            $http.get('Payrolls/FinalSettlement/GetEmployeeFNFMasterData?masterId=' + $scope.masterId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.SelectedEmployeeList = response.data;
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.FormulaList = [];
    $scope.FinalSettlementUndisbursedEarningList = [];
    $scope.GetEmployeeItems = function (obj) {
        $scope.FormulaList = [];
        $scope.EmpSystemId = obj.data.EmpSystemId;
        $http({
            method: 'GET',
            url: 'Payrolls/FinalSettlement/GetEmployeeSeperationItemFormulaData?EmpSystemId=' + $scope.EmpSystemId
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                $scope.FormulaList = response.data.SeperationItem;
                $scope.FinalSettlementUndisbursedEarningList = response.data.FinalSettlementUndisbursedEarning;
                angular.element(document.querySelector('#FormulaInfo')).modal('show');
            }
        });
    }


    $scope.paymentModeList = [];
    $http({
        method: 'GET',
        url: 'Enum/GetEMPPaymentModeEnumCbo/'
    }).then(function successCallback(response) {
        $scope.paymentModeList = response.data;
    });


    $scope.changePaymentMode = function () {
        if ($scope.voucher.PaymentMode == 'Bank') {
            $scope.voucher.PaymentSource = $scope.voucher.PaymentMode;
            $scope.voucher.CashMasterId = null;
            $scope.voucher.CashCurrencyId = null;
            $scope.voucher.CashName = null;
            //$scope.getBank();
        }
        else {
            $scope.voucher.PaymentSource = $scope.voucher.PaymentMode;
            $scope.voucher.BankId = null;
            $scope.voucher.BankMasterId = null;
            $scope.voucher.BankCurrencyId = null;
            $scope.voucher.AccountTitle = null;
            $scope.voucher.BankName = null;
        }
    }
    $scope.changeBank = function () {
        if ($scope.voucher.PaymentMode == 'Bank') {
            $scope.voucher.PaymentSource = $scope.voucher.PaymentMode;
        }
    }
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
            "name": "Account Type",
            "value": "BankAccountTypeName"
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
    $scope.bankmasterList = [];
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

    $scope.showBankPopUp = function () {
        $scope.getBankList = function (pageno) {

            $scope.url = "Accounts/SalaryDisbursement/GetBankMasterList?bankACType=HouseBank&&bankId=" + $scope.voucher.BankId;
            baseService.paginationBase($scope.url, pageno, $scope.bankParameters)
                .then(function (result) {
                    $scope.bankmasterList = result.Rows;
                    $scope.bankParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getBankList();
        angular.element(document.querySelector("#bankPopUp")).modal("show");
    };

    $scope.closeCashPopUp = function () {
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
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankmasterList[$scope.bankIndex];

            $scope.voucher.AccountTitle = bank.AccountTitle;
            $scope.voucher.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
            $scope.voucher.BankMasterId = bank.BankMasterId;
            $scope.voucher.BankCurrencyId = bank.CurrencyId;

            $scope.voucher.GLGeneralInfoId = bank.GLGeneralInfoId;
            $scope.voucher.GLGeneralInfoName = bank.GLGeneralInfoName;
            $scope.voucher.BudgetMasterId = bank.BudgetMasterId;
            $scope.voucher.BudgetName = bank.BudgetName;
            $scope.voucher.ActivityId = bank.ActivityId;
            $scope.voucher.ActivityName = bank.ActivityName;
            $scope.checkBankAmount();
        }
        $scope.hideBankPopUp();
    };
    $scope.selectBankPopUp = function (index, id) {
        $scope.bankIndex = index;
    };

    $scope.hideBankPopUp = function () {
        angular.element(document.querySelector("#bankPopUp")).modal("hide");
        $scope.bankIndex = -1;
        $scope.bankSelected = null;
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

    $scope.deleteGoodWorkPaymentAdviseDisbursementUrl = "Attendances/GoodWork/DeleteGoodWorkPaymentAdviseDisbursement";

    $scope.deleteSalaryDisbursement = function (voucherId, monthNo, yearNo) {
        $http({
            method: "POST",
            url: $scope.deleteGoodWorkPaymentAdviseDisbursementUrl,
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
                $scope.getVoucherData();
                $scope.ClearGoodWorkPaymentAdviseDisbursement();
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.confirmDelete = function (data) {
        $scope.voucherId = data.PayableVoucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

    $scope.getFiscalYearPeriod = function (date) {
        if (!baseService.isUndefinedOrNull(date)) {
            $http({
                method: "get",
                url: "accounts/CompanyFiscalYear/CheckingFiscalYearPeriod?postingDate=" + $filter("dateFiltering")(date)
            }).then(
                function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        var result = response.data;
                        if (result.IsTransationLocked === true) {
                            ShowResult(commonMessage.FiscalPeriodTransactionLocked, "failure");
                            $scope.voucher.PostingDate = "";
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else if (result.IsExchangeRateConfirmed === false) {
                            ShowResult(commonMessage.FiscalPeriodExchangeRateConfirmed, "failure");
                            $scope.voucher.PostingDate = "";
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else {
                            $scope.voucher.FiscalYearId = result.FiscalYearId;
                            $scope.voucher.FiscalYearName = result.FiscalYearName;
                            $scope.voucher.FiscalYearPeriodId = result.FiscalYearPeriodId;
                            $scope.voucher.FiscalYearPeriodName = result.PeriodName;
                        }
                    }
                },
                function errorCallback(response) {
                });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.FinalSettlementModel = {};
        $scope.SelectedEmployeeList = [];
        $scope.FormulaList = [];
    }
};