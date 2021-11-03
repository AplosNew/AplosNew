'use strict';
baseInvoiceWriteOffController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function baseInvoiceWriteOffController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $scope.voucherDetailCurrencyList = [];
    $scope.partyGLList = [];

    $scope.voucher = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
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
        BankMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        SourceFrom: null,
        SourceTo: 'Bank',

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
        EmployeeTransactionTypeId: null
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
        ToCurrencyRate: null,
        DrAmount: 0,
        CrAmount: 0,
        TrnType: null
    };

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.voucherList = result.Rows;
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
            cboService.getCboEntityByCompanyWise(null, null, function (result) {
                $scope.entityList = result;
            });
    });

    $scope.getBudgetCboByGL = function (glgeneralInfoId) {
        cboService.getBudgetCboByGL(glgeneralInfoId, function (result) {
            $scope.BudgetItemList = result;
            if ($scope.BudgetItemList.length === 1) {
                $scope.voucherDetail.BudgetId = $scope.BudgetItemList[0].Value;
                $scope.voucherDetail.BudgetName = $scope.BudgetItemList[0].Text;
                $scope.getActivity(glgeneralInfoId);
            }
        });
    };

    $scope.ActivityList = [];
    $scope.getActivity = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/Budget/GetBudgetActivityCbo?budgetId=' + id
        }).then(function successCallback(response) {
            $scope.ActivityList = response.data;
        });
    };

    $scope.setDrExchangeRate = function (amount, glId, glName, budgetId, budgetName, activityId, activityName, isSplit) {
        if (!baseService.isUndefinedOrNull(glId)) {
            var companyCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
            var companyGroupCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
            var hardCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'HardCurrency' });
            var getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr' });
            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
                getRow[0].TrnType = 'Dr';
                getRow[0].GLGeneralInfoId = glId;
                getRow[0].GLGeneralInfoName = glName;
                getRow[0].BudgetId = budgetId;
                getRow[0].BudgetName = budgetName;
                getRow[0].ActivityId = activityId;
                getRow[0].ActivityName = activityName;
                // Base currency
                getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                getRow[0].CompanyCurrencyName = $scope.companyCurrencyName;
                getRow[0].CompanyFromCurrencyId = companyCurrencyExchangeRate[0].FromCurrencyId;
                getRow[0].ToCurrencyId = companyCurrencyExchangeRate[0].ToCurrencyId;
                getRow[0].CompanyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;
                getRow[0].CompanyCurrencyDr = (amount * companyCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                getRow[0].CompanyCurrencyCr = null;
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    getRow[0].CompanyGroupCurrencyDr = (amount / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
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
                    // Base currency
                    CompanyCurrencyId: $scope.companyCurrencyId,
                    CompanyCurrencyName: $scope.companyCurrencyName,
                    CompanyFromCurrencyId: companyCurrencyExchangeRate[0].FromCurrencyId,
                    ToCurrencyId: companyCurrencyExchangeRate[0].ToCurrencyId,
                    CompanyCurrencyRate: companyCurrencyExchangeRate[0].ToCurrencyRate,
                    CompanyCurrencyDr: (amount * companyCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4),
                    CompanyCurrencyCr: null
                };
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    data.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    data.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;
                    data.CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                    data.CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    data.CompanyGroupCurrencyDr = (amount / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
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

    $scope.setCrExchangeRate = function (data) {
        if (!baseService.isUndefinedOrNull(data.GLGeneralInfoId)) {
            var companyCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
            var companyGroupCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
            var hardCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'HardCurrency' });
            var getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Cr', 'InvoiceDetailId': data.InvoiceDetailId, 'GLGeneralInfoId': data.GLGeneralInfoId });

            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
                getRow[0].TrnType = 'Cr';
                getRow[0].InvoiceDetailId = data.InvoiceDetailId;
                getRow[0].GLGeneralInfoId = data.GLGeneralInfoId;
                getRow[0].GLGeneralInfoName = data.GLGeneralInfoName;
                getRow[0].BudgetId = data.BudgetId;
                getRow[0].BudgetName = data.BudgetName;
                getRow[0].ActivityId = data.ActivityId;
                getRow[0].ActivityName = data.ActivityName;
                // Base currency
                if (!baseService.isUndefinedOrNull($scope.companyCurrencyId)) {
                    getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                    getRow[0].CompanyCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                    getRow[0].ToCurrencyId = data.ToCurrencyId;
                    getRow[0].CompanyCurrencyRate = data.CompanyCurrencyRate;
                    getRow[0].CompanyCurrencyDr = null;
                    getRow[0].CompanyCurrencyCr = (data.Amount * data.CompanyCurrencyRate).toFixed(4);

                    if ($scope.companyCurrencyId === $scope.voucher.CurrencyId) {
                        data.ConvertedAmount = getRow[0].CompanyCurrencyCr;
                    }
                }
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                    getRow[0].CompanyGroupCurrencyRate = data.CompanyGroupCurrencyRate;
                    getRow[0].CompanyGroupCurrencyDr = null;
                    getRow[0].CompanyGroupCurrencyCr = (data.Amount / data.CompanyGroupCurrencyRate).toFixed(4);

                    if ($scope.companyGroupCurrencyId === $scope.voucher.CurrencyId) {
                        data.ConvertedAmount = getRow[0].CompanyGroupCurrencyCr;
                    }
                }
                // Hard currency
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    getRow[0].HardCurrencyId = $scope.hardCurrencyId;
                    getRow[0].HardCurrencyName = $scope.companyCurrencyName;
                    getRow[0].HardFromCurrencyId = data.HardFromCurrencyId;
                    getRow[0].HardCurrencyRate = data.HardCurrencyRate;
                    getRow[0].HardCurrencyDr = null;
                    getRow[0].HardCurrencyCr = (data.Amount * data.HardCurrencyRate).toFixed(4);

                    if ($scope.hardCurrencyId === $scope.voucher.CurrencyId) {
                        data.ConvertedAmount = getRow[0].HardCurrencyCr;
                    }
                }
            }
            else {
                var obj = {
                    TrnType: 'Cr',
                    InvoiceDetailId: data.InvoiceDetailId,
                    GLGeneralInfoId: data.GLGeneralInfoId,
                    GLGeneralInfoName: data.GLGeneralInfoName,
                    BudgetId: data.BudgetId,
                    BudgetName: data.BudgetName,
                    ActivityId: data.ActivityId,
                    ActivityName: data.ActivityName
                };
                // Base currency
                if (!baseService.isUndefinedOrNull($scope.companyCurrencyId)) {
                    obj.CompanyCurrencyId = $scope.companyCurrencyId;
                    obj.CompanyCurrencyName = $scope.companyCurrencyName;
                    obj.CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                    obj.ToCurrencyId = data.ToCurrencyId;
                    obj.CompanyCurrencyRate = data.CompanyCurrencyRate;
                    obj.CompanyCurrencyDr = null;
                    obj.CompanyCurrencyCr = (data.Amount * data.CompanyCurrencyRate).toFixed(4);

                    if ($scope.companyCurrencyId === $scope.voucher.CurrencyId) {
                        obj.ConvertedAmount = data.CompanyCurrencyCr;
                    }
                }
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    obj.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    obj.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;
                    obj.CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                    obj.CompanyGroupCurrencyRate = data.CompanyGroupCurrencyRate;
                    obj.CompanyGroupCurrencyDr = null;
                    obj.CompanyGroupCurrencyCr = (data.Amount / data.CompanyGroupCurrencyRate).toFixed(4);

                    if ($scope.companyGroupCurrencyId === $scope.voucher.CurrencyId) {
                        obj.ConvertedAmount = data.CompanyGroupCurrencyCr;
                    }
                }
                // Hard currency
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    obj.HardCurrencyId = $scope.hardCurrencyId;
                    obj.HardCurrencyName = $scope.hardCurrencyName;
                    obj.HardFromCurrencyId = data.HardFromCurrencyId;
                    obj.HardCurrencyRate = data.HardCurrencyRate;
                    obj.HardCurrencyDr = null;
                    obj.HardCurrencyCr = (data.Amount * data.HardCurrencyRate).toFixed(4);

                    if ($scope.hardCurrencyId === $scope.voucher.CurrencyId) {
                        obj.ConvertedAmount = data.HardCurrencyCr;
                    }
                }
                $scope.voucherDetailCurrencyList.push(data);
            }
        }
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

    $scope.changeCurrencyExchangeRate = function () {
        angular.forEach($scope.voucherDetailList, function (item, i) {
            if (item.TrnType === 'Dr') {
                $scope.setDrExchangeRate(item.Amount, item.GLGeneralInfoId, item.GLGeneralInfoName, item.BudgetId, item.BudgetName, item.ActivityId, item.ActivityName, $scope.voucher.isSplit);
            }
            else if (item.TrnType === 'Cr') {
                $scope.setCrExchangeRate(item);
            }
        });
    };

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
        return manualValidation('div_PostingDate', $scope.invalidPostingDate, msg);
    };

    $scope.invalidEntity = false;
    $scope.entityValidation = function () {
        $scope.invalidEntity = baseService.isUndefinedOrNull($scope.voucher.EntityId);
        return manualValidation('div_entity', $scope.invalidEntity, 'Entity is required.');
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

            $scope.setDrExchangeRate(row[0].Amount, row[0].GLGeneralInfoId, row[0].GLGeneralInfoName, row[0].BudgetId, row[0].BudgetName, row[0].ActivityId, row[0].ActivityName, false);
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
                drRow[0].ActivityId, drRow[0].ActivityName, false);
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
                $scope.voucherDetail.ActivityId, $scope.voucherDetail.ActivityName, true);
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

    $scope.removeRow = function (index) {
        var row = $scope.voucherDetailList[index];
        $scope.voucherDetailList.splice(index, 1);
        var drc = $scope.voucherDetailCurrencyList.length;
        while (drc--) {
            if ($scope.voucherDetailCurrencyList[drc]['GLGeneralInfoId'] === row.GLGeneralInfoId) {
                $scope.voucherDetailCurrencyList.splice(drc, 1);
            }
        }
    };

    //Gets data from the Database
    $scope.invoiceGLList = [];
    var glUrl = null;
    if ($scope.partyType === 'Customer') {
        glUrl = 'accounts/GLItem/GetCustomerInvoiceGLList2';
    }
    else if ($scope.partyType === 'Vendor') {
        glUrl = 'accounts/GLItem/GetVendorInvoiceGLList';
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
        $scope.getFiscalYearPeriod($scope.voucher.PostingDate);
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
    };
}