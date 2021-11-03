'use strict';
loanTakenController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function loanTakenController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Loan Taken';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.currencyExchangeRate = [];
    $scope.partyFromTo = 'From';
    $scope.bankFromTo = 'To';
    $scope.isWriteOff = true;
    $scope.partyType = 'Party';
    $scope.sourceType = 'Loan';
    $scope.isAdvance = false;
    $scope.isReadOnly = false;
    $scope.isWriteOff = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller('bankBaseController', { $scope: $scope, $http: $http });
    $controller('cashBaseController', { $scope: $scope, $http: $http });
    baseService.init('accounts/Loan/GetLoanTakenList', null, null, 'DESC', 'DocDate', 'DocDate');
    $scope.voucherDetailCurrencyList = [];
    $scope.partyGLList = [];

    $scope.voucher = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        DrEntityId: null,
        CrEntityId: null,
        PlantId: null,
        PartyId: null,
        PartyName: null,
        PartyType: null,
        CurrencyId: null,
        PaymentTermId: null,
        SourceType: null,
        VoucherNo: null,
        VoucherDate: $filter('dateFiltering')(Date.now()),
        PostingDate: $filter('dateFiltering')(Date.now()),
        DocDate: $filter('dateFiltering')(Date.now()),
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        MatureDate: null,
        DocRefNo: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        IsExcludingTax: false,
        IsSplit: false,
        Amount: 0,
        Narration: null,
        Remarks: null,
        BankName: null,
        CashName: null,
        BankMasterId: null,
        CashMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        SourceTo: 'Bank',
        SourceFrom: 'Party',
        DirectorName: null,
        LoanGiven: null,
        LoanTaken: null,
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
        BudgetId: null,
        BudgetName: null,
        ActivityId: null,
        ActivityName: null,

        EmployeeGLGeneralInfoId: null,
        EmployeeGLGeneralInfoName: null,
        EmployeeTransactionTypeId: null,
        InvoiceAmount: 0,
        ExGainLossAmount: 0,
        NetInvoiceAmount: 0,
        InvoiceGroupAmount: 0,
        ExGainLossGroupAmount: 0,
        FinancingTypeId: null,
        RepaymentStartDate: $filter('dateFiltering')(Date.now()),
        LifeOfYear: 0,
        NoOfInstallmentPerYear: 0,
        TotalNoOfInstallment: 0,
        ProfitRate: 0,
        ProfitAmount: 0
    };

    $scope.voucherDetail = {
        EntityId: null
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
        ToCurrencyRate: 0,
        DrAmount: 0,
        CrAmount: 0,
        TrnType: null
    };

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.investmentTakenList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchInvoiceList = [
        {
            'name': 'VoucherNo',
            'value': 'VoucherNo'
        },
        {
            'name': 'VoucherType',
            'value': 'VoucherType'
        },
        {
            'name': 'Doc Date',
            'value': 'DocDate'
        },
        {
            'name': 'Doc Ref No',
            'value': 'DocRefNo'
        },
        {
            'name': 'Posting Date',
            'value': 'PostingDate'
        },
        {
            'name': $scope.partyType,
            'value': 'Party'
        },
        {
            'name': 'Currency',
            'value': 'Currency'
        }
    ];

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, '', function (result) {
                $scope.entityList = result;
            });
    });

    cboService.getCboVoucherTypeLoanTakenList(function (result) {
        $scope.voucherTypeList = result;
        if ($scope.voucherTypeList.length === 1) {
            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
        }
    });

    cboService.getCboOtherFinancingType($scope.sourceType, function (result) {
        $scope.financingTypeList = result;
    });

    $scope.searchVendorInvoiceList = [
        {
            'name': 'Posting Date',
            'value': 'PostingDate'
        },
        {
            'name': 'VoucherType',
            'value': 'VoucherType'
        },
        {
            'name': 'Doc Date',
            'value': 'DocDate'
        },
        {
            'name': 'Doc Ref No',
            'value': 'DocRefNo'
        },
        {
            'name': 'Posting Date',
            'value': 'PostingDate'
        },
        {
            'name': 'Customer',
            'value': 'Party'
        },
        {
            'name': 'Currency',
            'value': 'Currency'
        }
    ];

    $scope.getById = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/Advance/GetAdvance/' + id
        }).then(function successCallback(response) {
            $scope.voucher = response.data;
            $scope.voucher.DocDate = $filter('dateFiltering')($scope.voucher.DocDate);
            $scope.voucher.VoucherDate = $filter('dateFiltering')($scope.voucher.VoucherDate);
            $scope.voucher.PostingDate = $filter('dateFiltering')($scope.voucher.PostingDate);
            // $scope.GetCurrencyExchangeRateList();
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };

    //**************************************** Customer List Start ***************************
    $scope.customerList = [];
    $scope.customerIndex = -1;
    $scope.selectedCustomer = null;
    $scope.searchCustomerByList = [
        {
            'name': 'Party Code',
            'value': 'Code'
        },
        {
            'name': 'Party Name',
            'value': 'UserName'
        },
        {
            'name': 'Currency',
            'value': 'CurrencyCode'
        },
        {
            'name': 'VATResistrationNo',
            'value': 'VATResistrationNo'
        },
        {
            'name': 'TradeLicenseNo',
            'value': 'TradeLicenseNo'
        },
        {
            'name': 'Debit Limit',
            'value': 'DebitLimit'
        },
        {
            'name': 'Credit Limit',
            'value': 'CreditLimit'
        }
    ];

    $scope.customerParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getCustomerGL = function () {
        $scope.customerUrl = 'Parties/party/GetCompanyPartyDataList';
        $scope.getCustomerData = function (pageno) {
            baseService.paginationBase($scope.customerUrl, pageno, $scope.customerParameters)
                .then(function (result) {
                    $scope.customerList = result.Rows;
                    $scope.customerParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#customerListPopUp')).modal('show');
        $scope.getCustomerData();
    };

    $scope.selectCustomerPopUp = function (index, id) {
        $scope.customerIndex = index;
        $scope.selectedCustomer = id;
    };

    $scope.closeCustomerPopUp = function () {
        if ($scope.customerIndex !== -1) {
            var party = $scope.customerList[$scope.customerIndex];
            $scope.voucher.PartyName = party.Code + " - " + party.UserName;
            $scope.voucher.PartyId = party.Id;
            $scope.voucher.PartyType = $scope.partyType;
            // $scope.GetCurrencyExchangeRateList();
        }
        angular.element(document.querySelector('#customerListPopUp')).modal('hide');
        $scope.customerIndex = -1;
        $scope.selectedCustomer = null;
    };
    //**************************************** Customer List End ***************************

    $scope.totalInvoiceAmount = function () {
        var invoiceData = null;
        var exLossData = null;
        var exGainData = null;
        $scope.voucher.InvoiceAmount = 0;
        $scope.voucher.ExGainLossAmount = 0;
        $scope.ExLossAmount = 0;
        $scope.ExGainAmount = 0;

        invoiceData = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Cr', 'ExchangeStatus': 'No' });
        exLossData = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'ExchangeStatus': 'ExchangeLoss' });
        exGainData = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Cr', 'ExchangeStatus': 'ExchangeGain' });
        angular.forEach(invoiceData, function (item, i) {
            $scope.voucher.InvoiceAmount += parseFloat(item.CompanyCurrencyCr);
        });

        angular.forEach(exGainData, function (item, i) {
            $scope.ExGainAmount += isNaN(item.CompanyCurrencyCr) ? 0 : parseFloat(item.CompanyCurrencyCr);
        });

        angular.forEach(exLossData, function (item, i) {
            $scope.ExLossAmount += isNaN(item.CompanyCurrencyDr) ? 0 : parseFloat(item.CompanyCurrencyDr);
        });

        $scope.voucher.ExGainLossAmount = Math.abs($scope.ExGainAmount - $scope.ExLossAmount).toFixed(4);
        // $scope.voucher.Amount = parseFloat($scope.voucher.InvoiceAmount) + parseFloat($scope.ExGainAmount) - parseFloat($scope.ExLossAmount);
        if ($scope.CurrencyParallel.length === 2) {
            var invoiceGroupData = null;
            var exLossGroupData = null;
            var exGainGroupData = null;
            $scope.voucher.InvoiceGroupAmount = 0;
            $scope.voucher.ExGainLossGroupAmount = 0;
            $scope.ExLossGroupAmount = 0;
            $scope.ExGainGroupAmount = 0;

            angular.forEach(invoiceData, function (item, i) {
                $scope.voucher.InvoiceGroupAmount += parseFloat(item.CompanyGroupCurrencyCr);
            });

            angular.forEach(exGainData, function (item, i) {
                $scope.ExGainGroupAmount += isNaN(item.CompanyGroupCurrencyCr) ? 0 : parseFloat(item.CompanyGroupCurrencyCr);
            });

            angular.forEach(exLossData, function (item, i) {
                $scope.ExLossGroupAmount += isNaN(item.CompanyGroupCurrencyDr) ? 0 : parseFloat(item.CompanyGroupCurrencyDr);
            });

            $scope.voucher.ExGainLossGroupAmount = Math.abs($scope.ExGainGroupAmount - $scope.ExLossGroupAmount).toFixed(4);
            $scope.voucher.InvoiceGroupAmount = parseFloat($scope.voucher.InvoiceGroupAmount) + parseFloat($scope.ExGainGroupAmount) - parseFloat($scope.ExLossGroupAmount);
        }
    };

    $scope.convertAmount = function (data) {
        var cramount = parseInt(data.Amount), balance = parseInt(data.Balance);
        if (cramount > balance) {
            data.Amount = data.Balance;
            ShowResult('DocRefNo ' + data.DocRefNo + ' Payment Amount should not exceed Balance Amount', 'failure');
        }
        else {
            CloseShowResult();
        }
        $scope.setCrExchangeRate(data);
        // $scope.totalInvoiceAmount();

        var getRowDrData = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'ExchangeStatus': 'No' });
        if (getRowDrData.length > 0) {
            $scope.setDrExchangeRate($scope.voucher.Amount,
                getRowDrData[0].GLGeneralInfoId, getRowDrData[0].GLGeneralInfoName,
                getRowDrData[0].BudgetId, getRowDrData[0].BudgetName,
                getRowDrData[0].ActivityId, getRowDrData[0].ActivityName, getRowDrData[0].DocRefNo, true, $scope.voucher.InvoiceGroupAmount);
        }
    };

    $scope.removeRow = function (index, data) {
        $scope.deletecurrency = data.CurrencyId;
        var row = $scope.voucherDetailList[index];
        $scope.voucherDetailList.splice(index, 1);
        var drc = $scope.voucherDetailCurrencyList.length;
        while (drc--) {
            if ($scope.voucherDetailCurrencyList[drc]['DocRefNo'] === row.DocRefNo) {
                $scope.voucherDetailCurrencyList.splice(drc, 1);
            }
        }

        var deletecurrencyData = $filter('filter')($scope.voucherDetailCurrencyList, { 'FromCurrencyId': $scope.deletecurrency });
        if (deletecurrencyData.length === 0) {
            for (var i = 0; i < $scope.currencyExchangeRate.length; i++) {
                if ($scope.currencyExchangeRate[i].FromCurrencyId === $scope.deletecurrency) {
                    if ($scope.CurrencyParallel.length === 2) {
                        if ($scope.currencyExchangeRate[i].FromCurrencyId === $scope.CurrencyParallel[1].CurrencyId) {
                        }
                        else {
                            $scope.currencyExchangeRate.splice(i, 1);
                        }
                    }
                    else
                        $scope.currencyExchangeRate.splice(i, 1);
                }
            }
        }
        $scope.deletecurrency = null;
        $scope.changeCurrencyExchangeRate();
    };
    $scope.changeSourceFrom = function (from) {
        $scope.voucher.PartyId = null;
        $scope.voucher.PartyName = null;
        $scope.partyType = from;
    };
    $scope.changeDirector = function () {
        $scope.voucher.PartyType = $scope.partyType;
    };
    $scope.changeSourceTo = function (to) {
        $scope.voucher.DrGLId = null;
        $scope.voucher.DrGLName = null;
        $scope.voucher.DrBudgetId = null;
        $scope.voucher.DrActivityId = null;
        $scope.voucher.BankName = null;
        $scope.voucher.CashName = null;
        $scope.voucher.BankMasterId = null;
        $scope.voucher.CashMasterId = null;
        $scope.isBankAmount = false;
        $scope.voucher.BankCurrencyId = null;
        for (var i = 0; i < $scope.voucherDetailCurrencyList.length; i++) {
            if ($scope.voucherDetailCurrencyList[i].TrnType === 'Dr') {
                $scope.voucherDetailCurrencyList.splice(i, 1);
            }
        }
    };

    $scope.transactionTypeGL = null;
    $scope.getTransactionTypeGL = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $scope.transactionTypeGL = $.grep($scope.financingTypeList, function (item) {
                return item.FinancingTypeId === id;
            })[0];
            if (manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.transactionTypeGL.AssetGLId), 'Transaction Type GL not found!')) {
                $scope.transactionTypeGL = null;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget
                && manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.transactionTypeGL.AssetBudgetMasterId), 'Transaction Type Budget not found!')) {
                $scope.transactionTypeGL = null;
            }
            for (var i = 0; i < $scope.voucherDetailCurrencyList.length; i++) {
                if ($scope.voucherDetailCurrencyList[i].TrnType === 'Cr') {
                    $scope.voucherDetailCurrencyList.splice(i, 1);
                }
            }
            $scope.voucher.CrGLId = $scope.transactionTypeGL.LiabilityGLId;
            $scope.voucher.CrBudgetId = $scope.transactionTypeGL.LiabilityBudgetId;
            $scope.voucher.CrActivityId = $scope.transactionTypeGL.LiabilityActivityId;
            $scope.setCrExchangeRate($scope.voucher.Amount,
                $scope.transactionTypeGL.LiabilityGLId, $scope.transactionTypeGL.LiabilityGLName,
                $scope.transactionTypeGL.LiabilityBudgetId, $scope.transactionTypeGL.LiabilityBudgetName,
                $scope.transactionTypeGL.LiabilityActivityId, $scope.transactionTypeGL.LiabilityActivityName, $scope.voucher.DocRefNo, false, $scope.voucher.InvoiceGroupAmount);
        }
        else {
            manualValidation('div_TransactionType', true, 'Transaction Type is required.');
            $scope.transactionTypeGL = null;
        }
    };

    $scope.closeCashPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult('Please Select Currency!', 'failure', 'cashPopUp');
            return;
        }
        if ($scope.cashIndex !== -1) {
            var cash = $scope.cashList[$scope.cashIndex];
            if (baseService.isUndefinedOrNull(cash.GLGeneralInfoId)) {
                ShowResult('Cash GL not found!', 'failure', 'bankPopUp');
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(cash.BudgetId)) {
                ShowResult('Cash Budget not found!', 'failure', 'bankPopUp');
                return;
            }
            else if (baseService.isUndefinedOrNull(cash.CurrencyId)) {
                ShowResult('Cash Transaction Currency not found!', 'failure', 'bankPopUp');
                return;
            }
            else {
                $scope.voucher.CashMasterId = cash.Id;
                $scope.voucher.CashCurrencyId = cash.CurrencyId;
                $scope.voucher.CashName = cash.CashName;
                $scope.voucher.DrGLId = cash.GLGeneralInfoId;
                $scope.voucher.GLGeneralInfoName = cash.GLItem;
                $scope.voucher.DrBudgetId = cash.BudgetId;
                $scope.voucher.BudgetName = cash.BudgetName;
                $scope.voucher.DrActivityId = cash.ActivityId;
                $scope.voucher.DrEntityId = cash.EntityId;
                // $scope.totalInvoiceAmount();
                $scope.setDrExchangeRate($scope.voucher.Amount,
                    $scope.voucher.DrGLId, $scope.voucher.GLGeneralInfoName,
                    $scope.voucher.DrBudgetId, $scope.voucher.BudgetName,
                    $scope.voucher.DrActivityId, $scope.voucher.ActivityName, $scope.voucher.DocRefNo, false, $scope.voucher.InvoiceGroupAmount);
                $scope.checkCashAmount();
            }
        }
        $scope.hideCashPopUp();
    };

    $scope.closeBankPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult('Please Select Currency!', 'failure', 'bankPopUp');
            return;
        }
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
                ShowResult('Bank GL not found!', 'failure', 'bankPopUp');
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(bank.BudgetId)) {
                ShowResult('Bank Budget not found!', 'failure', 'bankPopUp');
                return;
            }
            else if (baseService.isUndefinedOrNull(bank.CurrencyId)) {
                ShowResult('Bank Transaction Currency not found!', 'failure', 'bankPopUp');
                return;
            }
            else {
                $scope.voucher.AccountTitle = bank.AccountTitle;
                $scope.voucher.BankName = bank.COAItemCode + ' - ' + bank.Bank + ' - ' + bank.AccountTitle + ' - ' + bank.AccountNumber;
                $scope.voucher.BankMasterId = bank.BankMasterId;
                $scope.voucher.BankCurrencyId = bank.CurrencyId;

                $scope.voucher.DrGLId = bank.GLGeneralInfoId;
                $scope.voucher.GLGeneralInfoName = bank.GLItem;
                $scope.voucher.DrBudgetId = bank.BudgetId;
                $scope.voucher.BudgetName = bank.BudgetName;
                $scope.voucher.DrActivityId = bank.ActivityId;
                $scope.voucher.DrEntityId = bank.EntityId;
                // $scope.totalInvoiceAmount();
                $scope.setDrExchangeRate($scope.voucher.Amount,
                    $scope.voucher.DrGLId, $scope.voucher.GLGeneralInfoName,
                    $scope.voucher.DrBudgetId, $scope.voucher.BudgetName,
                    $scope.voucher.DrActivityId, $scope.voucher.ActivityName, $scope.voucher.DocRefNo, false, $scope.voucher.InvoiceGroupAmount);
                $scope.checkBankAmount();
            }
        }
        $scope.hideBankPopUp();
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
    $scope.exchangeAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.transactionTypeGL.LiabilityGLId)) {
            $scope.setCrExchangeRate($scope.voucher.Amount,
                $scope.transactionTypeGL.LiabilityGLId, $scope.transactionTypeGL.LiabilityGLName,
                $scope.transactionTypeGL.LiabilityBudgetId, $scope.transactionTypeGL.LiabilityBudgetName,
                $scope.transactionTypeGL.LiabilityActivityId, $scope.transactionTypeGL.LiabilityActivityName, $scope.voucher.DocRefNo, false, $scope.voucher.InvoiceGroupAmount);
        }
        if (!baseService.isUndefinedOrNull($scope.voucher.DrGLId)) {
            $scope.setDrExchangeRate($scope.voucher.Amount,
                $scope.voucher.DrGLId, $scope.voucher.GLGeneralInfoName,
                $scope.voucher.DrBudgetId, $scope.voucher.BudgetName,
                $scope.voucher.DrActivityId, $scope.voucher.ActivityName, $scope.voucher.DocRefNo, false, $scope.voucher.InvoiceGroupAmount);
        }
    };

    $scope.totalInstallment = function () {
        $scope.voucher.TotalNoOfInstallment = ($scope.voucher.LifeOfYear * $scope.voucher.NoOfInstallmentPerYear);
    };
    $scope.validation = function () {
        if ($scope.voucher.Amount === 0) {
            ShowResult('Amount Can not 0!', 'failure');
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult('Please select Currency!', 'failure');
            return true;
        }

        if (baseService.isUndefinedOrNull($scope.voucher.CrEntityId)) {
            ShowResult('Please select Entity!', 'failure');
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.voucher.DrEntityId)) {
            ShowResult('Please select Bank or Cash Entity!', 'failure');
            return true;
        }

        if ($scope.CurrencyParallel.length === 2) {
            if ($scope.voucher.BankCurrencyId === $scope.companyGroupCurrencyId) {
                if ($scope.voucher.BankAmount !== $scope.voucher.InvoiceGroupAmount) {
                    ShowResult('Bank Amount and Group Currency Amount are not equal!', 'failure');
                    return true;
                }
            }
        }
        return false;
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
        $scope.redirectTab();
        $scope.$broadcast('show-errors-check-validity');
        $scope.checkDocDate();
        $scope.checkPostingDate();
        $scope.passBankCashAmount();
        //if ($scope.companyConfig.IsProfitCenterApplicable) {
        //    $scope.entityValidation();
        //}
        //$scope.form1.$valid &&
        if (!$scope.validation() && !$scope.invalidDocDate && !$scope.invalidPostingDate && $scope.checkDrCrBalancing()) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/Loan/InsertLoanTaken',
                    data: {
                        'voucherVM': $scope.voucher,
                        'currencyList': $scope.voucherDetailCurrencyList,
                        'loanRepaymentSchedulelist': $scope.loanRepaymentSchedulelist
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();
                        $scope.isReadOnly = false;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'accounts/Invoice/InsertVendorPayment',
                    data: {
                        'voucherVM': $scope.voucher,
                        'voucherDetailVMList': $scope.voucherDetailList,
                        'voucherDetailCurrencyVMList': $scope.voucherDetailCurrencyList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.menuFrames[$scope.index] = $scope.menuFrame;
                        }
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    };

    $scope.setDrExchangeRate = function (amount, glId, glName, budgetId, budgetName, activityId, activityName, docRefNo, isSplit, groupamount) {
        if (!baseService.isUndefinedOrNull(glId)) {
            var companyCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
            var companyGroupCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
            var hardCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'HardCurrency' });
            var getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'GLGeneralInfoId': glId });
            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
                getRow[0].TrnType = 'Dr';
                getRow[0].GLGeneralInfoId = glId;
                getRow[0].GLGeneralInfoName = glName;
                getRow[0].BudgetId = budgetId;
                getRow[0].BudgetName = budgetName;
                getRow[0].ActivityId = activityId;
                getRow[0].ActivityName = activityName;
                getRow[0].DocRefNo = docRefNo;
                getRow[0].Amount = amount;
                getRow[0].ExchangeStatus = 'No';
                // Base currency
                getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                getRow[0].CompanyCurrencyName = $scope.companyCurrencyName;
                getRow[0].CompanyFromCurrencyId = $scope.companyCurrencyId;
                getRow[0].ToCurrencyId = $scope.companyCurrencyId;
                getRow[0].CompanyCurrencyRate = 1;
                getRow[0].CompanyCurrencyDr = amount;
                getRow[0].CompanyCurrencyCr = null;
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyGroupFromCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupCurrencyRate = 0;
                    getRow[0].CompanyGroupCurrencyDr = groupamount;
                    getRow[0].CompanyGroupCurrencyCr = null;
                }
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    getRow[0].HardCurrencyId = $scope.hardCurrencyId;
                    getRow[0].HardCurrencyName = $scope.companyCurrencyName;
                    getRow[0].HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                    getRow[0].HardCurrencyCr = null;
                    getRow[0].HardCurrencyDr = (amount * hardCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                }
            }
            else {
                var data = {
                    TrnType: 'Dr',
                    GLGeneralInfoId: glId,
                    GLGeneralInfoName: glName,
                    BudgetId: budgetId,
                    BudgetName: budgetName,
                    ActivityId: activityId,
                    ActivityName: activityName,
                    DocRefNo: docRefNo,
                    ExchangeStatus: 'No',
                    Amount: amount,
                    // Base currency
                    CompanyCurrencyId: $scope.companyCurrencyId,
                    CompanyCurrencyName: $scope.companyCurrencyName,
                    CompanyFromCurrencyId: $scope.companyCurrencyId,
                    ToCurrencyId: $scope.companyCurrencyId,
                    CompanyCurrencyRate: 1,
                    CompanyCurrencyDr: amount,
                    CompanyCurrencyCr: null
                };
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    data.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    data.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;
                    data.CompanyGroupFromCurrencyId = $scope.companyGroupCurrencyId;
                    data.CompanyGroupCurrencyRate = 1;
                    data.CompanyGroupCurrencyDr = groupamount;
                    data.CompanyGroupCurrencyCr = null;
                }
                // Hard currency
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    data.HardCurrencyId = $scope.hardCurrencyId;
                    data.HardCurrencyName = $scope.hardCurrencyName;
                    data.HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                    data.HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                    data.HardCurrencyDr = (amount * hardCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                    data.HardCurrencyCr = null;
                }
                $scope.voucherDetailCurrencyList.push(data);
            }
        }
    };
    $scope.setCrExchangeRate = function (amount, glId, glName, budgetId, budgetName, activityId, activityName, docRefNo, isSplit, groupamount) {
        if (!baseService.isUndefinedOrNull(glId)) {
            var companyCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
            var companyGroupCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
            var hardCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'HardCurrency' });
            var getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Cr', 'GLGeneralInfoId': glId });
            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
                getRow[0].TrnType = 'Cr';
                getRow[0].GLGeneralInfoId = glId;
                getRow[0].GLGeneralInfoName = glName;
                getRow[0].BudgetId = budgetId;
                getRow[0].BudgetName = budgetName;
                getRow[0].ActivityId = activityId;
                getRow[0].ActivityName = activityName;
                getRow[0].DocRefNo = docRefNo;
                getRow[0].Amount = amount;
                // Base currency
                getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                getRow[0].CompanyCurrencyName = $scope.companyCurrencyName;
                getRow[0].CompanyFromCurrencyId = $scope.companyCurrencyId;
                getRow[0].ToCurrencyId = $scope.companyCurrencyId;
                getRow[0].CompanyCurrencyRate = 1;
                getRow[0].CompanyCurrencyCr = amount;
                getRow[0].CompanyCurrencyDr = null;
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyGroupFromCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupCurrencyRate = 0;
                    getRow[0].CompanyGroupCurrencyCr = groupamount;
                    getRow[0].CompanyGroupCurrencyDr = null;
                }
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    getRow[0].HardCurrencyId = $scope.hardCurrencyId;
                    getRow[0].HardCurrencyName = $scope.companyCurrencyName;
                    getRow[0].HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                    getRow[0].HardCurrencyDr = null;
                    getRow[0].HardCurrencyCr = (amount * hardCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                }
            }
            else {
                var data = {
                    TrnType: 'Cr',
                    GLGeneralInfoId: glId,
                    GLGeneralInfoName: glName,
                    BudgetId: budgetId,
                    BudgetName: budgetName,
                    ActivityId: activityId,
                    ActivityName: activityName,
                    DocRefNo: docRefNo,
                    Amount: amount,
                    // Base currency
                    CompanyCurrencyId: $scope.companyCurrencyId,
                    CompanyCurrencyName: $scope.companyCurrencyName,
                    CompanyFromCurrencyId: $scope.companyCurrencyId,
                    ToCurrencyId: $scope.companyCurrencyId,
                    CompanyCurrencyRate: 1,
                    CompanyCurrencyCr: amount,
                    CompanyCurrencyDr: null
                };
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    data.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    data.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;
                    data.CompanyGroupFromCurrencyId = $scope.companyGroupCurrencyId;
                    data.CompanyGroupCurrencyRate = 1;
                    data.CompanyGroupCurrencyCr = groupamount;
                    data.CompanyGroupCurrencyDr = null;
                }
                // Hard currency
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    data.HardCurrencyId = $scope.hardCurrencyId;
                    data.HardCurrencyName = $scope.hardCurrencyName;
                    data.HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                    data.HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                    data.HardCurrencyCr = (amount * hardCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                    data.HardCurrencyDr = null;
                }
                $scope.voucherDetailCurrencyList.push(data);
            }
        }
    };

    $scope.changeCurrencyExchangeRate = function () {
        angular.forEach($scope.voucherDetailCurrencyList, function (item, i) {
            if (item.TrnType === 'Cr' && item.ExchangeStatus === 'No') {
                $scope.setCrExchangeRate(item);
                $scope.totalInvoiceAmount();
            }
        });
        angular.forEach($scope.voucherDetailCurrencyList, function (item, i) {
            if (item.TrnType === 'Dr' && item.ExchangeStatus === 'No') {
                if ($scope.CurrencyParallel.length === 1) {
                    $scope.setDrExchangeRate($scope.voucher.Amount, item.GLGeneralInfoId, item.GLGeneralInfoName, item.BudgetId, item.BudgetName, item.ActivityId, item.ActivityName, $scope.voucher.DocRefNo, $scope.voucher.isSplit, 0);
                }
                else {
                    $scope.setDrExchangeRate($scope.voucher.Amount, item.GLGeneralInfoId, item.GLGeneralInfoName, item.BudgetId, item.BudgetName, item.ActivityId, item.ActivityName, $scope.voucher.DocRefNo, $scope.voucher.isSplit, $scope.voucher.InvoiceGroupAmount);
                }
            }
        });
    };

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            $http({
                method: 'GET',
                url: 'currencies/ExchangeRate/ParallelExchangeRate?fromdate=' + $scope.voucher.PostingDate + '&currencyId=' + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.changeCurrencyExchangeRate();
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.getFiscalYearPeriod = function (date) {
        if (!baseService.isUndefinedOrNull(date) && !$scope.invalidPostingDate) {
            $http({
                method: 'get',
                url: 'accounts/CompanyFiscalYear/CheckingFiscalYearPeriod?postingDate=' + $filter('dateFiltering')(date)
            }).then(
                function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.currencyExchangeRate = [];
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        var result = response.data;
                        if (result.IsTransationLocked === true) {
                            ShowResult(commonMessage.FiscalPeriodTransactionLocked, 'failure');
                            $scope.voucher.PostingDate = '';
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else if (result.IsExchangeRateConfirmed === false) {
                            ShowResult(commonMessage.FiscalPeriodExchangeRateConfirmed, 'failure');
                            $scope.voucher.PostingDate = '';
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
                            $scope.GetCurrencyExchangeRateList();
                        }
                    }
                },
                function errorCallback(response) {
                });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.getFiscalYearPeriod($scope.voucher.PostingDate);

    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getcustomercbo'
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });

    $scope.changePaymentTerm = function (id) {
        if (id !== null) {
            var baseLineDate = $.grep($scope.paymentTermList, function (item) {
                return item.Value === id;
            })[0].BaseLineDate;

            var paymentTermCode = $.grep($scope.paymentTermList, function (item) {
                return item.Value === id;
            })[0].PaymentTermCode;
            var noOfDay = $.grep($scope.paymentTermList, function (item) {
                return item.Value === id;
            })[0].NoOfDay;
            $scope.voucher.PaymentTermCode = paymentTermCode;
            $scope.voucher.BaseNoOfDays = noOfDay;
            if (baseLineDate !== null)
                if (baseLineDate === 'documentdate') {
                    $scope.voucher.BaseOnDueDate = $scope.voucher.DocDate;
                } else if (baseLineDate === 'postingdate') {
                    $scope.paymentTerms.BaseOnDueDate = $scope.voucher.PostingDate;
                }
                else {
                    $scope.voucher.BaseOnDueDate = $filter('dateFiltering')(Date.now());
                }
            $scope.getMatureDate($scope.voucher.BaseOnDueDate, $scope.voucher.BaseNoOfDays);
        }
    };

    $scope.getMatureDate = function (date, days) {
        var declareDate = new Date(date);
        declareDate.setDate(declareDate.getDate() + days);
        var dateFormated = $filter('date')(declareDate, 'dd-MMM-yyyy');
        $scope.voucher.MatureDate = dateFormated;
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = '';
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = 'Doc date must be below or equal to current Date!';
        }
        else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            msg = 'Doc date must be below or equal to Posting Date!';
            $scope.invalidDocDate = true;
        }
        else $scope.invalidDocDate = false;
        return manualValidation('div_DocDate', $scope.invalidDocDate, msg);
    };
    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = '';
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            msg = 'Posting date must be below or equal to current Date!';
            $scope.voucher.FiscalYearId = null;
            $scope.voucher.FiscalYearName = null;
            $scope.voucher.FiscalYearPeriodId = null;
            $scope.voucher.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if (new Date($scope.voucherDetailList[i].PostingDate) > new Date($scope.voucher.PostingDate)) {
                msg = 'Posting date must be above or equal to receivable of ' + $scope.voucherDetailList[i].DocRefNo;
                $scope.invalidPostingDate = true;
                break;
            }
            else {
                $scope.invalidPostingDate = false;
            }
        }
        return manualValidation('div_PostingDate', $scope.invalidPostingDate, msg);
    };

    $scope.checkDrCrBalancing = function () {
        var companyCurrencyAmountDr = $filter('sumByKey')($filter('filter')($scope.voucherDetailCurrencyList, { TrnType: 'Dr' }), 'CompanyCurrencyDr');
        var companyCurrencyAmountCr = $filter('sumByKey')($filter('filter')($scope.voucherDetailCurrencyList, { TrnType: 'Cr' }), 'CompanyCurrencyCr');
        if (companyCurrencyAmountDr !== companyCurrencyAmountCr) {
            ShowResult($scope.companyCurrencyCode + ' Dr amount and Cr amount is not equal!', 'failure');
            $scope.setTab(2);
            return false;
        }
        if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
            var companyGroupCurrencyAmountDr = $filter('sumByKey')($filter('filter')($scope.voucherDetailCurrencyList, { TrnType: 'Dr' }), 'CompanyGroupCurrencyDr');
            var companyGroupCurrencyAmountCr = $filter('sumByKey')($filter('filter')($scope.voucherDetailCurrencyList, { TrnType: 'Cr' }), 'CompanyGroupCurrencyCr');
            if (companyGroupCurrencyAmountDr !== companyGroupCurrencyAmountCr) {
                ShowResult($scope.companyGroupCurrencyCode + ' Dr amount and Cr amount is not equal!', 'failure');
                $scope.setTab(2);
                return false;
            }
        }
        if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
            var hardCurrencyAmountDr = $filter('sumByKey')($filter('filter')($scope.voucherDetailCurrencyList, { TrnType: 'Dr' }), 'HardCurrencyDr');
            var hardCurrencyAmountCr = $filter('sumByKey')($filter('filter')($scope.voucherDetailCurrencyList, { TrnType: 'Cr' }), 'HardCurrencyCr');
            if (hardCurrencyAmountDr !== hardCurrencyAmountCr) {
                ShowResult($scope.hardCurrencyCode + ' Dr amount and Cr amount is not equal!', 'failure');
                $scope.setTab(2);
                return false;
            }
        }
        return true;
    };

    $scope.closePartyPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult('Please select Currency!', 'failure', 'partyPopUp');
            return true;
        }
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            if (baseService.isUndefinedOrNull(party.ReconciliationGLId)) {
                ShowResult($scope.partyType + ' GL not found!', 'failure', 'partyPopUp');
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.ReconciliationBudgetId)) {
                ShowResult($scope.partyType + ' Budget not found!', 'failure', 'partyPopUp');
                return;
            }
            else {
                $scope.voucher.PartyName = party.Code + ' - ' + party.UserName;
                $scope.voucher.PartyId = party.Id;
                $scope.voucher.PartyType = $scope.partyType;
                $scope.totalAdvanceAmount(party.Id, party.UserName);
            }
        }
        $scope.hidePartyPopUp();
    };
    $scope.updatePartyAmount = function () {
        var row = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Dr' });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            row[0].Amount = $scope.voucher.Amount;
        }
    };

    $scope.changePartyGL = function (glId) {
        var drGL = $.grep($scope.partyGLList, function (item) {
            return item.GLGeneralInfoId === glId;
        })[0];
        if (!$scope.voucher.IsSplit) {
            var row = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Dr' });
            $scope.voucher.GLGeneralInfoId = drGL.GLGeneralInfoId;
            $scope.voucher.GLGeneralInfoName = drGL.GLGeneralInfoName;
            $scope.voucher.BudgetId = drGL.BudgetId;
            $scope.voucher.BudgetName = drGL.BudgetId;
            $scope.voucher.ActivityId = drGL.ActivityId;
            $scope.voucher.ActivityName = drGL.ActivityId;

            row[0].Amount = $scope.voucher.Amount;
            row[0].GLGeneralInfoId = $scope.voucher.GLGeneralInfoId;
            row[0].GLGeneralInfoName = $scope.voucher.GLGeneralInfoName;
            row[0].BudgetId = $scope.voucher.BudgetId;
            row[0].BudgetName = $scope.voucher.BudgetName;
            row[0].ActivityId = $scope.voucher.ActivityId;
            row[0].ActivityName = $scope.voucher.ActivityName;

            $scope.setDrExchangeRate(row[0].Amount, row[0].GLGeneralInfoId, row[0].GLGeneralInfoName, row[0].BudgetId, row[0].BudgetName, row[0].ActivityId, row[0].ActivityName, row[0].DocRefNo, false, 0);
        }
    };

    $scope.changePartySplitGL = function (glId) {
        var drGL = $.grep($scope.partyGLList, function (item) {
            return item.GLGeneralInfoId === glId;
        })[0];

        if (!$scope.voucher.IsSplit) {
            var drRow = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Dr' });
            drRow[0].GLGeneralInfoId = drGL.GLGeneralInfoId;
            drRow[0].GLGeneralInfoName = drGL.GLGeneralInfoName;
            drRow[0].BudgetId = drGL.BudgetId;
            drRow[0].BudgetName = drGL.BudgetName;
            drRow[0].ActivityId = drGL.ActivityId;
            drRow[0].ActivityName = drGL.ActivityName;
            $scope.setDrExchangeRate(drRow[0].Amount,
                drRow[0].GLGeneralInfoId, drRow[0].GLGeneralInfoName,
                drRow[0].BudgetId, drRow[0].BudgetName,
                drRow[0].ActivityId, drRow[0].ActivityName, drRow[0].DocRefNo, false, 0);
        }
        else {
            $scope.voucherDetail.GLGeneralInfoId = drGL.GLGeneralInfoId;
            $scope.voucherDetail.GLGeneralInfoName = drGL.GLGeneralInfoName;
            $scope.voucherDetail.BudgetId = null;
            $scope.voucherDetail.BudgetName = null;
            $scope.voucherDetail.ActivityId = null;
            $scope.voucherDetail.ActivityName = null;
            $scope.voucherDetail.DocDate = $scope.voucher.DocDate;
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.DrAmount = 0;
            $scope.voucherDetail.TrnType = 'Dr';
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);

            $scope.setDrExchangeRate($scope.voucherDetail.DrAmount,
                $scope.voucherDetail.GLGeneralInfoId, $scope.voucherDetail.GLGeneralInfoName,
                $scope.voucherDetail.BudgetId, $scope.voucherDetail.BudgetName,
                $scope.voucherDetail.ActivityId, $scope.voucherDetail.ActivityName, $scope.voucherDetail.DocRefNo, true, 0);
        }
        $scope.getBudgetCboByGL($scope.voucherDetail.GLGeneralInfoId);
        clearVoucherDetail();
    };

    $scope.removeDrRow = function () {
        var dr = $scope.voucherDetailList.length;
        while (dr--) {
            if ($scope.voucherDetailList[dr]['TrnType'] === 'Dr') {
                $scope.voucherDetailList.splice(dr, 1);
            }
        }
        var drc = $scope.voucherDetailCurrencyList.length;
        while (drc--) {
            if ($scope.voucherDetailCurrencyList[drc]['TrnType'] === 'Dr') {
                $scope.voucherDetailCurrencyList.splice(drc, 1);
            }
        }
    };

    //Gets data from the Database
    $scope.invoiceGLList = [];
    var glUrl = null;
    if ($scope.partyType === 'Party') {
        glUrl = 'Parties/Party/GetCompanyPartyDataList';
    }
    else if ($scope.partyType === 'Director') {
        glUrl = 'accounts/GLItem/GetCustomerInvoiceGLList2';
    }

    $http.get(glUrl)
        .then(
        function successCallback(response) {
            $scope.invoiceGLList = response.data;
        },
        function errorCallback(response) {
            ShowResult(response, 'failure');
        });

    $scope.selectedInvoiceGLId = null;
    $scope.selectedInvoiceGLName = null;
    $scope.selectedInvoiceGL = function (selected) {
        if (selected) {
            $scope.selectedInvoiceGLId = selected.originalObject.GLGeneralInfoId;
            $scope.selectedInvoiceGLName = selected.originalObject.GLGeneralInfoName;
        }
    };

    $scope.inputChanged = function (str) {
        $scope.voucherDetail.GLGeneralInfoId = str;
    };

    function clearVoucherDetail() {
        $scope.voucherDetail = {};
    }

    $scope.pop = function (type, msg) {
        toaster.pop({
            type: type,
            body: msg,
            timeout: 5000
        });
    };

    $scope.Clear = function () {
        var voucherTypeId = $scope.voucher.VoucherId;
        $scope.Action = 'Save';
        $scope.voucher = {};
        if ($scope.voucherTypeList.length === 1) {
            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
        }
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.VoucherDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucher.PostingDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucher.DocDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
        $scope.voucherDetailCurrencyList = [];
        $scope.SelectedCurrency = null;
        $scope.voucher.CurrencyId = $scope.tranCurrencyList[0].Value;
        $scope.getFiscalYearPeriod($scope.voucher.PostingDate);
        $scope.loanRepaymentSchedulelist = [];
        $scope.isReadOnly = false;
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.redirectTab = function () {
        if ($scope.tabForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.tabForm2.$invalid) {
            $scope.setTab(2);
        }
        else if ($scope.tabForm3.$invalid) {
            $scope.setTab(3);
        }
    };

    $scope.customerInvoiceReceiptReport = function (voucherNo) {
        location.href = 'accounts/Loan/LoanTakenReport?voucherNo=' + voucherNo;
    };

    $scope.loanRepaymentSchedulelist = [];

    $scope.LoadRepamentDetail = function () {
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
        periodHtml += "<tr><td>" + FormatDate(disbursmentDate) + " (Disbursment date)" + "</td><td>" + " " + "</td><td style='text-align:right'>" + payment.toFixed(2) + "</td><td style='text-align:right'>" + profit.toFixed(2) + "</td><td style='text-align:right'>" + principal.toFixed(2) + "</td><td style='text-align:right'>" + actualAmount.toFixed(2) + "</td></tr>";
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
}