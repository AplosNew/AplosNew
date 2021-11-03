'use strict';
interLoanPendingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function interLoanPendingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Inter Investment';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.currencyExchangeRate = [];
    $scope.partyFromTo = 'From';
    $scope.bankFromTo = 'To';
    $scope.isWriteOff = true;
    $scope.partyType = 'Customer';
    $scope.sourceType = 'Investment';
    $scope.isAdvance = false;
    $scope.isReadOnly = false;
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller('bankBaseController', { $scope: $scope, $http: $http });
    $controller('cashBaseController', { $scope: $scope, $http: $http });
    baseService.init('accounts/loan/GetInterLoanPendingList', null, null, 'DESC', 'DocDate', 'DocDate');
    $scope.voucherDetailCurrencyList = [];
    $scope.partyGLList = [];

    $scope.voucher = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        DrEntityId: null,
        CrEntityId: null,
        CashName: null,
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
        BankMasterId: null,
        CashMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        SourceFrom: 'Cash',
        SourceTo: 'InterCompany',

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
        InvestmentTypeGivenId: null,
        InterCompanyId: null,
        InterPlantId: null
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
                $scope.investmentGivenList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.customerInvoiceReceiptReport = function (voucherNo) {
        location.href = 'accounts/Investment/InsertInvestmentTakenReport?voucherNo=' + voucherNo;
    };
}