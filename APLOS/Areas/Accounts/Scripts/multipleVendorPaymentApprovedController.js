'use strict';
multipleVendorPaymentApprovedController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function multipleVendorPaymentApprovedController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Multiple Vendor Payment Approved';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.currencyExchangeRate = [];
    $scope.partyFromTo = 'From';
    $scope.bankFromTo = 'To';
    $scope.isWriteOff = true;
    $scope.partyType = 'Vendor';
    $scope.isBankAmount = false;
    $controller('bankBaseController', { $scope: $scope, $http: $http });
    baseService.init('accounts/Invoice/GetMultipleVendorAvailableApprovalList', null, null, 'DESC', 'TentativeDate', 'TentativeDate');

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
        SourceTo: null,

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
        ExGainLossGroupAmount: 0
    };

    $scope.voucherDetail = {
        EntityId: null
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

    $scope.GetCurrencyParallel = function () {
        $http({
            method: 'GET',
            url: 'currencies/CompanyParallelCurrency/CurrencyParallel',
        }).then(function successCallback(response) {
            $scope.CurrencyParallel = response.data;
            if ($scope.CurrencyParallel.length == 0) {
                $scope.pop('error', 'Company Parallel Currency is not set! ');
                $scope.showform = false;
            }
            else {
                $scope.showform = true;
            }
            $scope.BaseCurrencyCode = $scope.CurrencyParallel[0].Code;
        });
    };
    $scope.GetCurrencyParallel();

    $scope.tranCurrencyList = [];
    cboService.getCboParallelCurrency(function (result) {
        $scope.tranCurrencyList = result;
        $scope.voucher.CurrencyId = $scope.tranCurrencyList[0].Value;
    });

    // Creating parallel currency table heading.
    $scope.parallelCurrencyTableHead += '<tr>' + $scope.source + '<th style="width:250px; vertical-align: middle; text-align:center" rowspan="2">GL</th>' +
        '<th style="width:100px; vertical-align: middle; text-align:center" rowspan="2">DocRefNo</th>' +
        '<th ng-show="companyConfig.IsVoucherFromBudget" style="width:250px; vertical-align: middle; text-align:center" rowspan="2">Budget</th>' +
        '<th ng-show="companyConfig.IsVoucherFromBudget" style="width:250px; vertical-align: middle; text-align:center" rowspan="2">Activity</th>';
    var debitCreditHead = '</tr><tr>';
    $scope.parallelCurrencyTypeList = [];
    $scope.companyCurrencyId = null;
    $scope.companyGroupCurrencyId = null;
    $scope.hardCurrencyId = null;
    $http.get('currencies/CompanyParallelCurrency/CurrencyParallel')
        .then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.showform = true;
                angular.forEach(response.data, function (item, i) {
                    $scope.parallelCurrencyTableHead += '<th style="text-align:center" colspan="2">' + item.Code + '</th>';
                    debitCreditHead += '<th>Dr</th><th>Cr</th>';
                    if (item.ParallelCurrencyType === 'CompanyCurrency') {
                        $scope.companyCurrencyId = item.CurrencyId;
                        $scope.companyCurrencyCode = item.Code;
                        $scope.companyCurrencyName = item.Code;
                        $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyCurrency', CurrencyType: 'CompanyCurrencyDr', CurrencyId: item.CurrencyId });
                        $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyCurrency', CurrencyType: 'CompanyCurrencyCr', CurrencyId: item.CurrencyId });
                    }
                    else if (item.ParallelCurrencyType === 'CompanyGroupCurrency') {
                        $scope.companyGroupCurrencyId = item.CurrencyId;
                        $scope.companyGroupCurrencyCode = item.Code;
                        $scope.companyGroupCurrencyName = item.Code;
                        $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyGroupCurrency', CurrencyType: 'CompanyGroupCurrencyDr', CurrencyId: item.CurrencyId });
                        $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyGroupCurrency', CurrencyType: 'CompanyGroupCurrencyCr', CurrencyId: item.CurrencyId });
                    }
                    else if (item.ParallelCurrencyType === 'HardCurrency') {
                        $scope.hardCurrencyId = item.CurrencyId;
                        $scope.hardCurrencyCode = item.Code;
                        $scope.hardCurrencyName = item.Code;
                        $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'HardCurrency', CurrencyType: 'HardCurrencyDr', CurrencyId: item.CurrencyId });
                        $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'HardCurrency', CurrencyType: 'HardCurrencyCr', CurrencyId: item.CurrencyId });
                    }
                });
            }
            else {
                ShowResult('Company Parallel Currency is not set!', 'failure');
                $scope.showform = false;
            }
            $scope.parallelCurrencyTableHead += debitCreditHead + '</tr>';
        });

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.paymentList = result.Rows;
                console.log('paymentList', $scope.paymentList);
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.getData();

    $http({
        method: 'GET',
        url: 'accounts/Invoice/GetMultiplePaymentPendingList'
    }).then(function successCallback(result) {
        $scope.multiplepaymentDetailList = result.data.Rows;
        console.log('multiplepaymentDetailList', $scope.multiplepaymentDetailList);
    });

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

    $scope.exchangeGainLossList = [];
    $http.get('accounts/ExchangeGainLoss/GetExchangeGainLoss')
        .then(function successCallback(response) {
            $scope.exchangeGainLossList = response.data;
        },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });

    $scope.GetCurrencyExchangeRateList = function (currencyId, data) {
        if ($scope.CurrencyParallel.length == 2) {
            var ParaCurrency = $filter('filter')($scope.CurrencyParallel, { 'CurrencyId': currencyId });
            if (ParaCurrency.length > 0) {
                $scope.exchangerateUrl = 'currencies/ExchangeRate/ReceiveParallelExchangeRate?fromdate=' + $scope.voucher.PostingDate + '&currencyId=' + currencyId
            }
            else {
                $scope.exchangerateUrl = 'currencies/ExchangeRate/ParallelExchangeRate?fromdate=' + $scope.voucher.PostingDate + '&currencyId=' + currencyId
            }
            if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull(currencyId)) {
                $http({
                    method: 'GET',
                    url: $scope.exchangerateUrl
                }).then(function successCallback(response) {
                    if ($scope.currencyExchangeRate.length == 0) {
                        $scope.currencyExchangeRate = response.data;
                        $scope.SelectedCurrency = currencyId;
                    }
                    else {
                        var excurrdata = response.data;
                        var ParaCurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': excurrdata[0].FromCurrencyId });
                        if (ParaCurrencyRate.length == 0) {
                            var AdditionalCurrency = {
                                ParallelCurrencyId: excurrdata[0].ToCurrencyId,
                                FromCurrencyUnit: 1,
                                FromCurrencyId: excurrdata[0].FromCurrencyId,
                                FromCurrencyCode: excurrdata[0].FromCurrencyCode,
                                ToCurrencyId: excurrdata[0].ToCurrencyId,
                                ToCurrencyRate: excurrdata[0].ToCurrencyRate,
                                ToCurrencyCode: $scope.currencyExchangeRate[0].ToCurrencyCode
                            }
                            $scope.currencyExchangeRate.push(AdditionalCurrency);
                        }
                    }

                    $scope.setDrExchangeRatePayment(data);
                    if (!baseService.isUndefinedOrNull($scope.voucher.GLGeneralInfoId)) {
                        $scope.totalInvoiceAmount();
                        $scope.setCrExchangeRatePayment($scope.voucher.GLGeneralInfoId, $scope.voucher.Amount,
                            $scope.voucher.GLGeneralInfoName,
                            $scope.voucher.BudgetId, $scope.voucher.BudgetName,
                            $scope.voucher.ActivityId, $scope.voucher.ActivityName, $scope.voucher.DocRefNo, $scope.voucher.InvoiceGroupAmount);
                    }
                });
            }
            else {
                $scope.currencyExchangeRate = [];
            }
        }
        else {
            if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull(currencyId)) {
                $http({
                    method: 'GET',
                    url: 'currencies/ExchangeRate/ParallelExchangeRate?fromdate=' + $scope.voucher.PostingDate + '&currencyId=' + currencyId
                }).then(function successCallback(response) {
                    if ($scope.currencyExchangeRate.length == 0) {
                        $scope.currencyExchangeRate = response.data;
                        console.log('currencyExchangeRate', $scope.currencyExchangeRate);
                    }
                    else if ($scope.CurrencyParallel[0].CurrencyId != currencyId) {
                        //TODO:find id exsist or not then push or response.data
                        var CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': currencyId });
                        if (CurrencyRate.length == 0) {
                            var excurrdata = response.data;
                            var AdditionalCurrency = {
                                ParallelCurrencyId: excurrdata[0].ToCurrencyId,
                                FromCurrencyUnit: 1,
                                FromCurrencyId: excurrdata[0].FromCurrencyId,
                                FromCurrencyCode: excurrdata[0].FromCurrencyCode,
                                ToCurrencyId: excurrdata[0].ToCurrencyId,
                                ToCurrencyRate: excurrdata[0].ToCurrencyRate,
                                ToCurrencyCode: $scope.currencyExchangeRate[0].ToCurrencyCode
                            }
                            $scope.currencyExchangeRate.push(AdditionalCurrency);
                        }
                    }
                    $scope.SelectedCurrency = currencyId;
                    $scope.setDrExchangeRatePayment(data);
                });
            }
            else {
                $scope.currencyExchangeRate = [];
            }
        }
        console.log('currencyExchangeRate', $scope.currencyExchangeRate);
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
                msg = 'Posting date must be below or equal to receivable of ' + $scope.voucherDetailList[i].DocRefNo;
                $scope.invalidPostingDate = true;
                break;
            }
            else {
                $scope.invalidPostingDate = false;
            }
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

    $scope.updatePartyAmount = function () {
        var row = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Cr' });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            row[0].Amount = $scope.voucher.Amount;
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

    cboService.getCboVoucherTypeAdvanceTakenList(function (result) {
        $scope.voucherTypeList = result;
        if ($scope.voucherTypeList.length === 1) {
            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
        }
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
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };

    $scope.customerInvoiceSearchList = [];
    $scope.customerInvoiceParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'VoucherNo',
        searchBy: 'VoucherNo',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getPopupCustomerReceivableList = function () {
        $scope.customerreceivableGLData = function (pageno) {
            $scope.customerReceivableGLUrl1 = 'accounts/Invoice/GetVendorAvailableInvoiceList?partyid=' + $scope.voucher.PartyId;
            baseService.paginationBase($scope.customerReceivableGLUrl1, pageno, $scope.customerInvoiceParameters)
                .then(function (result) {
                    try {
                        $scope.customerreceivableList = result.Rows;
                        $scope.customerInvoiceParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.customerInvoiceSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.customerreceivableList, $scope.customerInvoiceSearchList);
                        }
                    } catch (e) {
                        ShowResult(e, 'Error');
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#CustomerReceivableListPopUP')).modal('show');
        $scope.customerreceivableGLData();
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#CustomerReceivableListPopUP')).modal('hide');
    };

    $scope.checkApproved = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                var getRow = null;
                getRow = $filter('filter')($scope.multiplepaymentDetailList, { MultiplePaymentId: data.Id });
                if (getRow.length > 0) {
                    for (var i = 0; i < getRow.length; i++) {
                        getRow[i].TrnType = 'Dr';
                        getRow[i].MultiplePaymentId = data.Id;
                        $scope.GetCurrencyExchangeRateList(getRow[i].CurrencyId, getRow[i]);
                    }
                    //$scope.voucherDetailList.push(data);
                }
            }
            else {
                var drc = $scope.voucherDetailCurrencyList.length;
                while (drc--) {
                    if ($scope.voucherDetailCurrencyList[drc]['MultiplePaymentId'] === data.Id) {
                        $scope.voucherDetailCurrencyList.splice(drc, 1);
                    }
                }

                //for (var i = 0; i < $scope.voucherDetailList.length; i++) {
                //    if ($scope.voucherDetailList[i].DocRefNo === data.DocRefNo) {
                //        $scope.voucherDetailList.splice(i, 1);
                //        break;
                //    }
                //}
                if (!baseService.isUndefinedOrNull($scope.voucher.GLGeneralInfoId)) {
                    $scope.totalInvoiceAmount();
                    $scope.setCrExchangeRatePayment($scope.voucher.GLGeneralInfoId, $scope.voucher.Amount,
                        $scope.voucher.GLGeneralInfoName,
                        $scope.voucher.BudgetId, $scope.voucher.BudgetName,
                        $scope.voucher.ActivityId, $scope.voucher.ActivityName, $scope.voucher.DocRefNo, $scope.voucher.InvoiceGroupAmount);
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }

    $scope.VendorBankAmount = function () {
        $scope.VendorBankAmountList = [];
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            var list = $filter('filter')($scope.VendorBankAmountList, { 'PartyId': $scope.voucherDetailList[i].PartyId });
            if (list.length === 0) {
                $scope.VendorBankAmountList.push($scope.voucherDetailList[i]);
            }
        }
    };

    $scope.totalInvoiceAmount = function () {
        var invoiceData = null;
        var exLossData = null;
        var exGainData = null;
        $scope.voucher.InvoiceAmount = 0;
        $scope.voucher.ExGainLossAmount = 0;
        $scope.ExLossAmount = 0;
        $scope.ExGainAmount = 0;

        invoiceData = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'ExchangeStatus': 'No' });
        exLossData = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'ExchangeStatus': 'ExchangeLoss' });
        exGainData = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Cr', 'ExchangeStatus': 'ExchangeGain' });
        angular.forEach(invoiceData, function (item, i) {
            $scope.voucher.InvoiceAmount += parseFloat(item.CompanyCurrencyDr);
        });

        angular.forEach(exGainData, function (item, i) {
            $scope.ExGainAmount += isNaN(item.CompanyCurrencyCr) ? 0 : parseFloat(item.CompanyCurrencyCr);
        });

        angular.forEach(exLossData, function (item, i) {
            $scope.ExLossAmount += isNaN(item.CompanyCurrencyDr) ? 0 : parseFloat(item.CompanyCurrencyDr);
        });

        $scope.voucher.ExGainLossAmount = Math.abs($scope.ExGainAmount - $scope.ExLossAmount).toFixed(4);
        $scope.voucher.Amount = parseFloat($scope.voucher.InvoiceAmount) - parseFloat($scope.ExGainAmount) + parseFloat($scope.ExLossAmount);
        if ($scope.CurrencyParallel.length == 2) {
            var invoiceGroupData = null;
            var exLossGroupData = null;
            var exGainGroupData = null;
            $scope.voucher.InvoiceGroupAmount = 0;
            $scope.voucher.ExGainLossGroupAmount = 0;
            $scope.ExLossGroupAmount = 0;
            $scope.ExGainGroupAmount = 0;

            angular.forEach(invoiceData, function (item, i) {
                $scope.voucher.InvoiceGroupAmount += parseFloat(item.CompanyGroupCurrencyDr);
            });

            angular.forEach(exGainData, function (item, i) {
                $scope.ExGainGroupAmount += isNaN(item.CompanyGroupCurrencyCr) ? 0 : parseFloat(item.CompanyGroupCurrencyCr);
            });

            angular.forEach(exLossData, function (item, i) {
                $scope.ExLossGroupAmount += isNaN(item.CompanyGroupCurrencyDr) ? 0 : parseFloat(item.CompanyGroupCurrencyDr);
            });

            $scope.voucher.ExGainLossGroupAmount = Math.abs($scope.ExGainGroupAmount - $scope.ExLossGroupAmount).toFixed(4);
            $scope.voucher.InvoiceGroupAmount = parseFloat($scope.voucher.InvoiceGroupAmount) - parseFloat($scope.ExGainGroupAmount) + parseFloat($scope.ExLossGroupAmount);
        }
    };

    $scope.convertAmount = function (data) {
        var dramount = parseInt(data.Amount), balance = parseInt(data.Balance);
        if (dramount > balance) {
            data.Amount = data.Balance;
            ShowResult('DocRefNo ' + data.DocRefNo + ' Payment Amount should not exceed Balance Amount', 'failure');
        }
        else {
            CloseShowResult();
        }
        $scope.setDrExchangeRatePayment(data);
        $scope.totalInvoiceAmount();

        var getRowDrData = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Cr', 'ExchangeStatus': 'No' });
        if (getRowDrData.length > 0) {
            $scope.setCrExchangeRatePayment($scope.voucher.Amount,
                getRowDrData[0].GLGeneralInfoId, getRowDrData[0].GLGeneralInfoName,
                getRowDrData[0].BudgetId, getRowDrData[0].BudgetName,
                getRowDrData[0].ActivityId, getRowDrData[0].ActivityName, true, $scope.voucher.InvoiceGroupAmount);
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
        if (deletecurrencyData.length == 0) {
            for (var i = 0; i < $scope.currencyExchangeRate.length; i++) {
                if ($scope.currencyExchangeRate[i].FromCurrencyId === $scope.deletecurrency) {
                    if ($scope.CurrencyParallel.length == 2) {
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

    $scope.setCrExchangeRatePayment = function (glId, amount, glName, budgetId, budgetName, activityId, activityName, docRefNo, groupamount) {
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
                getRow[0].ExchangeStatus = 'No';
                // Base currency
                getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                getRow[0].CompanyCurrencyName = $scope.companyCurrencyName;
                getRow[0].CompanyFromCurrencyId = $scope.companyCurrencyId;
                getRow[0].ToCurrencyId = $scope.companyCurrencyId;
                getRow[0].CompanyCurrencyRate = 1;
                getRow[0].CompanyCurrencyDr = null;
                getRow[0].CompanyCurrencyCr = amount;

                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyGroupFromCurrencyId = $scope.companyCurrencyId;
                    getRow[0].CompanyGroupCurrencyRate = 1;
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
                    ExchangeStatus: 'No',
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
                    data.CompanyGroupFromCurrencyId = $scope.companyCurrencyId;
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

    $scope.setDrExchangeRatePayment = function (data) {
        if (!baseService.isUndefinedOrNull(data.GLGeneralInfoId)) {
            var companyCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
            var companyGroupCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
            var getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'InvoiceDetailId': data.InvoiceDetailId, 'GLGeneralInfoId': data.GLGeneralInfoId });
            var CheckParaCurrency = $filter('filter')($scope.CurrencyParallel, { 'CurrencyId': data.CurrencyId });
            var exchangeRate = $filter('filter')($scope.currencyExchangeRate, { 'ParallelCurrencyId': data.CurrencyId });

            if ($scope.companyCurrencyId === data.CurrencyId) {
                data.ConvertedAmount = (data.Amount * data.CompanyCurrencyRate).toFixed(4);
            }
            if ($scope.companyGroupCurrencyId === data.CurrencyId) {
                data.ConvertedAmount = (data.Amount / data.CompanyGroupCurrencyRate).toFixed(4);
            }
            if ($scope.hardCurrencyId === data.CurrencyId) {
                data.ConvertedAmount = (data.Amount * data.HardCurrencyRate).toFixed(4);
            }
            else
                data.ConvertedAmount = (data.Amount * data.CompanyCurrencyRate).toFixed(4);

            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
                getRow[0].TrnType = 'Dr';
                getRow[0].InvoiceDetailId = data.InvoiceDetailId;
                getRow[0].MultiplePaymentId = data.MultiplePaymentId;
                getRow[0].GLGeneralInfoId = data.GLGeneralInfoId;
                getRow[0].GLGeneralInfoName = data.GLGeneralInfoName;
                getRow[0].DocRefNo = data.DocRefNo;
                getRow[0].BudgetId = data.BudgetId;
                getRow[0].BudgetName = data.BudgetName;
                getRow[0].ActivityId = data.ActivityId;
                getRow[0].ActivityName = data.ActivityName;
                getRow[0].ExchangeStatus = 'No';
                getRow[0].Amount = data.Amount;
                // Base currency
                if (!baseService.isUndefinedOrNull($scope.companyCurrencyId)) {
                    getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                    getRow[0].CompanyCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                    getRow[0].ToCurrencyId = data.ToCurrencyId;
                    getRow[0].CurrencyId = data.CurrencyId;
                    getRow[0].CompanyCurrencyRate = data.CompanyCurrencyRate;
                    getRow[0].CompanyCurrencyDr = null;
                    if (data.CurrencyId == $scope.companyCurrencyId) {/*base==base*/
                        getRow[0].CompanyCurrencyDr = (data.ConvertedAmount / data.CompanyCurrencyRate).toFixed(4);
                    }
                    else if (baseService.isUndefinedOrNull($scope.companyGroupCurrencyId) && data.CurrencyId != $scope.companyCurrencyId) {
                        getRow[0].CompanyCurrencyDr = (data.Amount * data.CompanyCurrencyRate).toFixed(4);
                    }
                    else if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId) && data.CurrencyId == $scope.companyGroupCurrencyId) {
                        getRow[0].CompanyCurrencyDr = (data.ConvertedAmount) /*(data.ConvertedAmount * data.CompanyCurrencyRate).toFixed(4)*/;
                    }
                    else {
                        getRow[0].CompanyCurrencyDr = data.ConvertedAmount;
                    }
                    // Group currecny gain/loss
                    if ($scope.companyCurrencyId != data.CurrencyId) {
                        var dataRate = 0;
                        var excRate = 0;
                        if ($scope.CurrencyParallel.length == 1) {/*For One Parallel Currency*/
                            var CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': data.CurrencyId });
                            excRate = CurrencyRate[0].ToCurrencyRate;
                        }
                        else {
                            var CurrencyRate;
                            if (data.CurrencyId != $scope.companyCurrencyId && data.CurrencyId != $scope.companyGroupCurrencyId)
                                CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': data.CurrencyId });
                            else if ($scope.SelectedCurrency === $scope.companyCurrencyId || $scope.SelectedCurrency === $scope.companyGroupCurrencyId)
                                CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'ParallelCurrencyType': 'CompanyCurrency' });
                            else if (data.CurrencyId === $scope.companyCurrencyId || data.CurrencyId === $scope.companyGroupCurrencyId)
                                CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'ParallelCurrencyType': 'CompanyGroupCurrency' });
                            else
                                CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': data.CurrencyId });
                            excRate = CurrencyRate[0].ToCurrencyRate;
                        }
                        if (data.CompanyCurrencyRate < excRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Base' });
                            if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                var loss = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });
                                rowGroup[0].MultiplePaymentId = data.MultiplePaymentId;
                                rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                rowGroup[0].TrnType = 'Dr';
                                rowGroup[0].Exchange = 'Base';
                                rowGroup[0].ExchangeStatus = 'ExchangeLoss';
                                rowGroup[0].GLGeneralInfoId = loss[0].CompanyCurrencyGLId;
                                rowGroup[0].GLGeneralInfoName = loss[0].CompanyCurrencyGL;
                                rowGroup[0].DocRefNo = data.DocRefNo;
                                if ($scope.companyCurrencyId != data.CurrencyId) {
                                    rowGroup[0].CompanyCurrencyDr = (data.Amount * (excRate - data.CompanyCurrencyRate)).toFixed(4);
                                }
                                else
                                    rowGroup[0].CompanyCurrencyDr = (data.ConvertedAmount * (excRate - data.CompanyCurrencyRate)).toFixed(4);

                                rowGroup[0].CompanyCurrencyCr = null
                                rowGroup[0].CompanyCurrencyId = $scope.companyCurrencyId;
                                rowGroup[0].CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                                rowGroup[0].CompanyCurrencyRate = excRate;
                            }
                            else {
                                var loss = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });
                                var exlosslist = {};
                                exlosslist.TrnType = 'Dr';
                                exlosslist.Exchange = 'Base';
                                exlosslist.ExchangeStatus = 'ExchangeLoss';
                                exlosslist.InvoiceDetailId = data.InvoiceDetailId;
                                exlosslist.GLGeneralInfoId = loss[0].CompanyCurrencyGLId;
                                exlosslist.GLGeneralInfoName = loss[0].CompanyCurrencyGL;
                                exlosslist.DocRefNo = data.DocRefNo;
                                if ($scope.companyCurrencyId != data.CurrencyId) {
                                    exlosslist.CompanyCurrencyDr = (data.Amount * (excRate - data.CompanyCurrencyRate)).toFixed(4);
                                }
                                else /*if ($scope.companyGroupCurrencyId === data.CurrencyId) {*/
                                    exlosslist.CompanyCurrencyDr = (data.ConvertedAmount * (excRate - data.CompanyCurrencyRate)).toFixed(4);
                                //}
                                exlosslist.CompanyCurrencyCr = null;
                                exlosslist.CompanyCurrencyId = $scope.companyCurrencyId;
                                exlosslist.CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                                exlosslist.CompanyCurrencyRate = excRate;
                                $scope.voucherDetailCurrencyList.push(exlosslist);
                            }
                        }
                        else if (data.CompanyCurrencyRate > excRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Base' });
                            if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                var gain = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                                rowGroup[0].MultiplePaymentId = data.MultiplePaymentId;
                                rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                rowGroup[0].TrnType = 'Cr';
                                rowGroup[0].Exchange = 'Base';
                                rowGroup[0].ExchangeStatus = 'ExchangeGain';
                                rowGroup[0].GLGeneralInfoId = gain[0].CompanyCurrencyGLId;
                                rowGroup[0].GLGeneralInfoName = gain[0].CompanyCurrencyGL;
                                rowGroup[0].DocRefNo = data.DocRefNo;
                                rowGroup[0].CompanyCurrencyDr = null;

                                if ($scope.companyCurrencyId != data.CurrencyId) {
                                    rowGroup[0].CompanyCurrencyCr = Math.abs(data.Amount * (data.CompanyCurrencyRate - excRate)).toFixed(4);
                                }
                                else //if ($scope.companyGroupCurrencyId === data.CurrencyId) {
                                    rowGroup[0].CompanyCurrencyCr = Math.abs(data.ConvertedAmount * (data.CompanyCurrencyRate - excRate)).toFixed(4);
                                //}
                                rowGroup[0].CompanyCurrencyId = $scope.companyCurrencyId;
                                rowGroup[0].CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                                rowGroup[0].CompanyCurrencyRate = excRate;
                            }
                            else {
                                var gain = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                                var exgainlist = {};
                                exgainlist.TrnType = 'Cr';
                                exgainlist.Exchange = 'Base';
                                exgainlist.ExchangeStatus = 'ExchangeGain';
                                exgainlist.InvoiceDetailId = data.InvoiceDetailId;
                                exgainlist.GLGeneralInfoId = gain[0].CompanyCurrencyGLId;
                                exgainlist.GLGeneralInfoName = gain[0].CompanyCurrencyGL;
                                exgainlist.DocRefNo = data.DocRefNo;
                                if ($scope.companyCurrencyId != data.CurrencyId) {
                                    exgainlist.CompanyCurrencyCr = Math.abs(data.Amount * (data.CompanyCurrencyRate - excRate)).toFixed(4);
                                }
                                else //if ($scope.companyGroupCurrencyId === data.CurrencyId) {
                                    exgainlist.CompanyCurrencyCr = Math.abs(data.ConvertedAmount * (data.CompanyCurrencyRate - excRate)).toFixed(4);
                                //}
                                exgainlist.CompanyCurrencyDr = null;
                                exgainlist.CompanyCurrencyId = $scope.companyCurrencyId;
                                exgainlist.CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                                exgainlist.CompanyCurrencyRate = excRate;
                                $scope.voucherDetailCurrencyList.push(exgainlist);
                            }
                        }
                        else if (data.CompanyCurrencyRate == excRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Base' });
                            if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                var i = $scope.voucherDetailCurrencyList.length;
                                while (i--) {
                                    if ($scope.voucherDetailCurrencyList[i]['GLGeneralInfoId'] === rowGroup[0].GLGeneralInfoId && $scope.voucherDetailCurrencyList[i]['InvoiceDetailId'] === rowGroup[0].InvoiceDetailId) {
                                        $scope.voucherDetailCurrencyList.splice(i, 1);
                                    }
                                }
                            }
                        }
                    }
                }

                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                    getRow[0].CompanyGroupCurrencyRate = data.CompanyGroupCurrencyRate;
                    getRow[0].CompanyGroupCurrencyCr = null;
                    if (data.CurrencyId == $scope.companyCurrencyId) {
                        getRow[0].CompanyGroupCurrencyDr = (data.ConvertedAmount / data.CompanyGroupCurrencyRate).toFixed(4);
                    }
                    else if (data.CurrencyId == $scope.companyGroupCurrencyId) {
                        getRow[0].CompanyGroupCurrencyDr = data.Amount;
                    }
                    else {
                        getRow[0].CompanyGroupCurrencyDr = (data.ConvertedAmount / data.CompanyGroupCurrencyRate).toFixed(4);
                    }
                    // Group currecny gain/loss
                    if ($scope.companyGroupCurrencyId != data.CurrencyId) {
                        var dataRate = 0;
                        var excRate = 0;
                        var grouprate = 0;
                        var CurrencyRate;
                        if (data.CurrencyId != $scope.companyCurrencyId && data.CurrencyId != $scope.companyGroupCurrencyId)
                            CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': data.CurrencyId });
                        else if ($scope.SelectedCurrency === $scope.companyCurrencyId || $scope.SelectedCurrency === $scope.companyGroupCurrencyId)
                            CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'ParallelCurrencyType': 'CompanyCurrency' });
                        else if (data.CurrencyId === $scope.companyCurrencyId || data.CurrencyId === $scope.companyGroupCurrencyId)
                            CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'ParallelCurrencyType': 'CompanyGroupCurrency' });
                        else
                            CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': data.CurrencyId });

                        excRate = CurrencyRate[0].ToCurrencyRate;

                        if (data.CurrencyId != $scope.companyCurrencyId) {
                            dataRate = data.CompanyCurrencyRate;
                        }
                        else {
                            dataRate = data.CompanyGroupCurrencyRate;
                        }
                        /*Get Group currency Rate*/
                        var GroupCurrencyData = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': $scope.companyGroupCurrencyId });
                        grouprate = GroupCurrencyData[0].ToCurrencyRate;
                        /***********************************************/
                        var convertExcAmount = 0;
                        var convertDataAmount = 0;
                        if ($scope.companyCurrencyId === data.CurrencyId) {
                            convertExcAmount = parseFloat(data.ConvertedAmount / excRate).toFixed(4);
                            convertDataAmount = parseFloat(data.ConvertedAmount / data.CompanyGroupCurrencyRate).toFixed(4);
                        }
                        else if ($scope.companyGroupCurrencyId === data.CurrencyId) {
                            convertExcAmount = parseFloat(data.ConvertedAmount * excRate).toFixed(4);
                            convertDataAmount = parseFloat(data.ConvertedAmount * data.CompanyGroupCurrencyRate).toFixed(4);
                        }
                        else {
                            convertExcAmount = parseFloat((data.Amount * excRate) / grouprate).toFixed(4);
                            convertDataAmount = parseFloat(data.ConvertedAmount / data.CompanyGroupCurrencyRate).toFixed(4);
                        }
                        var loss = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });
                        var gain = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                        if (dataRate > excRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Group' });
                            if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                rowGroup[0].MultiplePaymentId = data.MultiplePaymentId;
                                rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                rowGroup[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                rowGroup[0].CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                rowGroup[0].CompanyGroupCurrencyRate = excRate;
                                rowGroup[0].DocRefNo = data.DocRefNo;
                                if (convertDataAmount < convertExcAmount) {
                                    rowGroup[0].TrnType = 'Dr';
                                    rowGroup[0].Exchange = 'Group';
                                    rowGroup[0].ExchangeStatus = 'ExchangeLoss';
                                    rowGroup[0].GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                                    rowGroup[0].GLGeneralInfoName = loss[0].CompanyGroupCurrencyGL;
                                    rowGroup[0].CompanyGroupCurrencyCr = null;
                                    rowGroup[0].CompanyGroupCurrencyDr = Math.abs(convertExcAmount - convertDataAmount).toFixed(4);
                                }
                                else {
                                    rowGroup[0].TrnType = 'Cr';
                                    rowGroup[0].Exchange = 'Group';
                                    rowGroup[0].ExchangeStatus = 'ExchangeGain';
                                    rowGroup[0].GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                    rowGroup[0].GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                    rowGroup[0].CompanyGroupCurrencyCr = Math.abs(convertDataAmount - convertExcAmount).toFixed(4);
                                    rowGroup[0].CompanyGroupCurrencyDr = null;
                                }
                            }
                            else {
                                var exlosslist = {};
                                exlosslist.InvoiceDetailId = data.InvoiceDetailId;
                                exlosslist.DocRefNo = data.DocRefNo;
                                exlosslist.CompanyCurrencyId = $scope.companyCurrencyId;
                                exlosslist.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                exlosslist.CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                exlosslist.CompanyGroupCurrencyRate = excRate;
                                if (convertDataAmount < convertExcAmount) {
                                    exlosslist.TrnType = 'Dr';
                                    exlosslist.Exchange = 'Group';
                                    exlosslist.ExchangeStatus = 'ExchangeLoss';
                                    exlosslist.GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                                    exlosslist.GLGeneralInfoName = loss[0].CompanyGroupCurrencyGL;
                                    exlosslist.CompanyGroupCurrencyDr = Math.abs(convertDataAmount - convertExcAmount).toFixed(4);
                                    exlosslist.CompanyGroupCurrencyCr = null;
                                }
                                else {
                                    exlosslist.TrnType = 'Cr';
                                    exlosslist.Exchange = 'Group';
                                    exlosslist.ExchangeStatus = 'ExchangeGain';
                                    exlosslist.GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                    exlosslist.GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                    exlosslist.CompanyGroupCurrencyCr = Math.abs(convertDataAmount - convertExcAmount).toFixed(4);
                                    exlosslist.CompanyGroupCurrencyDr = null;
                                }
                                $scope.voucherDetailCurrencyList.push(exlosslist);
                            }
                        }
                        else if (dataRate < excRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Group' });
                            if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                rowGroup[0].MultiplePaymentId = data.MultiplePaymentId;
                                rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                rowGroup[0].DocRefNo = data.DocRefNo;
                                rowGroup[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                rowGroup[0].CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                rowGroup[0].CompanyGroupCurrencyRate = excRate;
                                /*when group rate change large then gain may change loss*/
                                if (convertExcAmount < convertDataAmount) {
                                    rowGroup[0].TrnType = 'Cr';
                                    rowGroup[0].Exchange = 'Group';
                                    rowGroup[0].ExchangeStatus = 'ExchangeGain';
                                    rowGroup[0].GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                    rowGroup[0].GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                    rowGroup[0].CompanyGroupCurrencyDr = null;
                                    rowGroup[0].CompanyGroupCurrencyCr = Math.abs(convertDataAmount - convertExcAmount).toFixed(4);
                                }
                                else {
                                    rowGroup[0].TrnType = 'Dr';
                                    rowGroup[0].Exchange = 'Group';
                                    rowGroup[0].ExchangeStatus = 'ExchangeLoss';
                                    rowGroup[0].GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                                    rowGroup[0].GLGeneralInfoName = loss[0].CompanyGroupCurrencyGL;
                                    rowGroup[0].CompanyGroupCurrencyCr = null;
                                    rowGroup[0].CompanyGroupCurrencyDr = Math.abs(convertExcAmount - convertDataAmount).toFixed(4);
                                }
                            }
                            else {
                                var exgainlist = {};
                                exgainlist.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                exgainlist.CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                exgainlist.CompanyGroupCurrencyRate = excRate;
                                exgainlist.InvoiceDetailId = data.InvoiceDetailId;
                                exgainlist.DocRefNo = data.DocRefNo;
                                if (convertExcAmount < convertDataAmount) {
                                    exgainlist.TrnType = 'Cr';
                                    exgainlist.Exchange = 'Group';
                                    exgainlist.ExchangeStatus = 'ExchangeGain';
                                    exgainlist.GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                    exgainlist.GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                    exgainlist.CompanyGroupCurrencyDr = null;
                                    exgainlist.CompanyGroupCurrencyCr = Math.abs(convertDataAmount - convertExcAmount).toFixed(4);
                                }
                                else {
                                    exgainlist.TrnType = 'Dr';
                                    exgainlist.Exchange = 'Group';
                                    exgainlist.ExchangeStatus = 'ExchangeLoss';
                                    exgainlist.GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                                    exgainlist.GLGeneralInfoName = loss[0].CompanyGroupCurrencyGL;
                                    exgainlist.CompanyGroupCurrencyCr = null;
                                    exgainlist.CompanyGroupCurrencyDr = Math.abs(convertExcAmount - convertDataAmount).toFixed(4);
                                }
                                $scope.voucherDetailCurrencyList.push(exgainlist);
                            }
                        }
                        /*Except company and group currency transaction may possible exchange gain and loss while group exchange diffrence*/
                        else if (data.CurrencyId != $scope.companyCurrencyId && data.CurrencyId != $scope.companyGroupCurrencyId) {
                            if (dataRate == excRate && data.CompanyGroupCurrencyRate == grouprate) {
                                var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Group' });
                                if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                    var i = $scope.voucherDetailCurrencyList.length;
                                    while (i--) {
                                        if ($scope.voucherDetailCurrencyList[i]['GLGeneralInfoId'] === rowGroup[0].GLGeneralInfoId && $scope.voucherDetailCurrencyList[i]['InvoiceDetailId'] === rowGroup[0].InvoiceDetailId) {
                                            $scope.voucherDetailCurrencyList.splice(i, 1);
                                        }
                                    }
                                }
                            }
                            else {
                                if (data.CompanyGroupCurrencyRate < grouprate) {
                                    var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Group' });
                                    if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                        rowGroup[0].MultiplePaymentId = data.MultiplePaymentId;
                                        rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                        rowGroup[0].TrnType = 'Cr';
                                        rowGroup[0].Exchange = 'Group';
                                        rowGroup[0].ExchangeStatus = 'ExchangeGain';
                                        rowGroup[0].GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                        rowGroup[0].GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                        rowGroup[0].DocRefNo = data.DocRefNo;
                                        rowGroup[0].CompanyGroupCurrencyCr = (((data.Amount * dataRate) / data.CompanyGroupCurrencyRate) - ((data.Amount * dataRate) / grouprate)).toFixed(4);
                                        rowGroup[0].CompanyGroupCurrencyDr = null
                                        rowGroup[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                        rowGroup[0].CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                        rowGroup[0].CompanyGroupCurrencyRate = excRate;
                                    }
                                    else {
                                        var exlosslist = {};
                                        exlosslist.TrnType = 'Cr';
                                        exlosslist.Exchange = 'Group';
                                        exlosslist.ExchangeStatus = 'ExchangeGain';
                                        exlosslist.InvoiceDetailId = data.InvoiceDetailId;
                                        exlosslist.GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                        exlosslist.GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                        exlosslist.DocRefNo = data.DocRefNo;
                                        exlosslist.CompanyGroupCurrencyCr = (((data.Amount * dataRate) / data.CompanyGroupCurrencyRate) - ((data.Amount * dataRate) / grouprate)).toFixed(4);
                                        exlosslist.CompanyGroupCurrencyDr = null;
                                        exlosslist.CompanyCurrencyId = $scope.companyCurrencyId;
                                        exlosslist.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                        exlosslist.CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                        exlosslist.CompanyGroupCurrencyRate = grouprate;
                                        $scope.voucherDetailCurrencyList.push(exlosslist);
                                    }
                                }
                                else if (data.CompanyGroupCurrencyRate > grouprate) {
                                    var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Group' });
                                    if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                        rowGroup[0].MultiplePaymentId = data.MultiplePaymentId;
                                        rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                        rowGroup[0].TrnType = 'Dr';
                                        rowGroup[0].Exchange = 'Group';
                                        rowGroup[0].ExchangeStatus = 'ExchangeLoss';
                                        rowGroup[0].GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                                        rowGroup[0].GLGeneralInfoName = loss[0].CompanyGroupCurrencyGL;
                                        rowGroup[0].DocRefNo = data.DocRefNo;
                                        rowGroup[0].CompanyGroupCurrencyCr = null;
                                        rowGroup[0].CompanyGroupCurrencyDr = (((data.Amount * dataRate) / grouprate) - ((data.Amount * dataRate) / data.CompanyGroupCurrencyRate)).toFixed(4);
                                        rowGroup[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                        rowGroup[0].CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                        rowGroup[0].CompanyGroupCurrencyRate = excRate;
                                    }
                                    else {
                                        var exgainlist = {};
                                        exgainlist.TrnType = 'Dr';
                                        exgainlist.Exchange = 'Group';
                                        exgainlist.ExchangeStatus = 'ExchangeLoss';
                                        exgainlist.InvoiceDetailId = data.InvoiceDetailId;
                                        exgainlist.GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                                        exgainlist.GLGeneralInfoName = loss[0].CompanyGroupCurrencyGL;
                                        exgainlist.DocRefNo = data.DocRefNo;
                                        exgainlist.CompanyGroupCurrencyDr = (((data.Amount * dataRate) / grouprate) - ((data.Amount * dataRate) / data.CompanyGroupCurrencyRate)).toFixed(4);
                                        exgainlist.CompanyGroupCurrencyCr = null;
                                        exgainlist.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                        exgainlist.CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                        exgainlist.CompanyGroupCurrencyRate = grouprate;
                                        $scope.voucherDetailCurrencyList.push(exgainlist);
                                    }
                                }
                            }
                        }
                        else if (dataRate == excRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Group' });
                            if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                var i = $scope.voucherDetailCurrencyList.length;
                                while (i--) {
                                    if ($scope.voucherDetailCurrencyList[i]['GLGeneralInfoId'] === rowGroup[0].GLGeneralInfoId && $scope.voucherDetailCurrencyList[i]['InvoiceDetailId'] === rowGroup[0].InvoiceDetailId) {
                                        $scope.voucherDetailCurrencyList.splice(i, 1);
                                    }
                                }
                            }
                        }
                    }
                }
                // Hard currency
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    getRow[0].HardCurrencyId = $scope.hardCurrencyId;
                    getRow[0].HardCurrencyName = $scope.companyCurrencyName;
                    getRow[0].HardFromCurrencyId = data.HardFromCurrencyId;
                    getRow[0].HardCurrencyRate = data.HardCurrencyRate;
                    getRow[0].HardCurrencyCr = null;
                    getRow[0].HardCurrencyDr = data.ConvertedAmount * data.HardCurrencyConversion;
                }
            }
            else {
                var obj = {
                    TrnType: 'Dr',
                    Exchange: 'No',
                    ExchangeStatus: 'No',
                    MultiplePaymentId: data.MultiplePaymentId,
                    InvoiceDetailId: data.InvoiceDetailId,
                    GLGeneralInfoId: data.GLGeneralInfoId,
                    GLGeneralInfoName: data.GLGeneralInfoName,
                    DocRefNo: data.DocRefNo,
                    BudgetId: data.BudgetId,
                    BudgetName: data.BudgetName,
                    ActivityId: data.ActivityId,
                    ActivityName: data.ActivityName,
                    Amount: data.Amount
                };

                // Base currency
                if (!baseService.isUndefinedOrNull($scope.companyCurrencyId)) {
                    obj.CompanyCurrencyId = $scope.companyCurrencyId;
                    obj.CompanyCurrencyName = $scope.companyCurrencyName;
                    obj.CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                    obj.ToCurrencyId = data.ToCurrencyId;
                    obj.CompanyCurrencyRate = data.CompanyCurrencyRate;
                    obj.CurrencyId = data.CurrencyId;
                    obj.CompanyCurrencyCr = null;
                    if (data.CurrencyId == $scope.companyCurrencyId) {/*base==base*/
                        obj.CompanyCurrencyDr = (data.ConvertedAmount / data.CompanyCurrencyRate).toFixed(4);
                    }
                    else if (baseService.isUndefinedOrNull($scope.companyGroupCurrencyId) && data.CurrencyId != $scope.companyCurrencyId) {
                        obj.CompanyCurrencyDr = (data.Amount * data.CompanyCurrencyRate).toFixed(4);
                    }
                    else if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId) && data.CurrencyId == $scope.companyGroupCurrencyId) {
                        obj.CompanyCurrencyDr = (data.ConvertedAmount) /*(data.ConvertedAmount * data.CompanyCurrencyRate).toFixed(4)*/;
                    }
                    else {
                        obj.CompanyCurrencyDr = data.ConvertedAmount;
                    }

                    if ($scope.companyCurrencyId != data.CurrencyId) {
                        var dataRate = 0;
                        var excRate = 0;
                        if ($scope.CurrencyParallel.length == 1) {/*For One Parallel Currency*/
                            var CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': data.CurrencyId });
                            excRate = CurrencyRate[0].ToCurrencyRate;
                        }
                        else {
                            var CurrencyRate;
                            if (data.CurrencyId != $scope.companyCurrencyId && data.CurrencyId != $scope.companyGroupCurrencyId)
                                CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': data.CurrencyId });
                            else if ($scope.SelectedCurrency === $scope.companyCurrencyId || $scope.SelectedCurrency === $scope.companyGroupCurrencyId)
                                CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'ParallelCurrencyType': 'CompanyCurrency' });
                            else if (data.CurrencyId === $scope.companyCurrencyId || data.CurrencyId === $scope.companyGroupCurrencyId)
                                CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'ParallelCurrencyType': 'CompanyGroupCurrency' });
                            else
                                CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': data.CurrencyId });
                            excRate = CurrencyRate[0].ToCurrencyRate;
                        }
                        if (data.CompanyCurrencyRate < excRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Base' });
                            var loss = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });
                            var exlosslist = {};
                            exlosslist.TrnType = 'Dr';
                            exlosslist.Exchange = 'Base';
                            exlosslist.ExchangeStatus = 'ExchangeLoss';
                            exlosslist.InvoiceDetailId = data.InvoiceDetailId;
                            exlosslist.MultiplePaymentId = data.MultiplePaymentId;
                            exlosslist.GLGeneralInfoId = loss[0].CompanyCurrencyGLId;
                            exlosslist.GLGeneralInfoName = loss[0].CompanyCurrencyGL;
                            exlosslist.DocRefNo = data.DocRefNo;
                            if ($scope.companyCurrencyId != data.CurrencyId) {
                                exlosslist.CompanyCurrencyDr = (data.Amount * (excRate - data.CompanyCurrencyRate)).toFixed(4);
                            }
                            else /*if ($scope.companyGroupCurrencyId === data.CurrencyId) {*/
                                exlosslist.CompanyCurrencyDr = (data.ConvertedAmount * (excRate - data.CompanyCurrencyRate)).toFixed(4);
                            //}
                            exlosslist.CompanyCurrencyCr = null;
                            exlosslist.CompanyCurrencyId = $scope.companyCurrencyId;
                            exlosslist.CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                            exlosslist.CompanyCurrencyRate = excRate;
                            $scope.voucherDetailCurrencyList.push(exlosslist);
                        }
                        else if (data.CompanyCurrencyRate > excRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Base' });
                            var gain = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                            var exgainlist = {};
                            exgainlist.TrnType = 'Cr';
                            exgainlist.Exchange = 'Base';
                            exgainlist.ExchangeStatus = 'ExchangeGain';
                            exgainlist.MultiplePaymentId = data.MultiplePaymentId;
                            exgainlist.InvoiceDetailId = data.InvoiceDetailId;
                            exgainlist.GLGeneralInfoId = gain[0].CompanyCurrencyGLId;
                            exgainlist.GLGeneralInfoName = gain[0].CompanyCurrencyGL;
                            exgainlist.DocRefNo = data.DocRefNo;
                            if ($scope.companyCurrencyId != data.CurrencyId) {
                                exgainlist.CompanyCurrencyCr = Math.abs(data.Amount * (data.CompanyCurrencyRate - excRate)).toFixed(4);
                            }
                            else //if ($scope.companyGroupCurrencyId === data.CurrencyId) {
                                exgainlist.CompanyCurrencyCr = Math.abs(data.ConvertedAmount * (data.CompanyCurrencyRate - excRate)).toFixed(4);
                            //}
                            exgainlist.CompanyCurrencyDr = null;
                            exgainlist.CompanyCurrencyId = $scope.companyCurrencyId;
                            exgainlist.CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                            exgainlist.CompanyCurrencyRate = excRate;
                            $scope.voucherDetailCurrencyList.push(exgainlist);
                        }
                    }
                }

                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    obj.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    obj.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;
                    obj.CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                    obj.CompanyGroupCurrencyRate = data.CompanyGroupCurrencyRate;
                    obj.CompanyGroupCurrencyCr = null;

                    // Group currecny gain/loss
                    if (data.CurrencyId == $scope.companyCurrencyId) {
                        obj.CompanyGroupCurrencyDr = (data.ConvertedAmount / data.CompanyGroupCurrencyRate).toFixed(4);
                    }
                    else if (data.CurrencyId == $scope.companyGroupCurrencyId) {
                        obj.CompanyGroupCurrencyDr = data.Amount;
                    }
                    else {
                        obj.CompanyGroupCurrencyDr = (data.ConvertedAmount / data.CompanyGroupCurrencyRate).toFixed(4);
                    }
                    // Group currecny gain/loss
                    if ($scope.companyGroupCurrencyId != data.CurrencyId) {
                        var dataRate = 0;
                        var excRate = 0;
                        var grouprate = 0;
                        var CurrencyRate;
                        if (data.CurrencyId != $scope.companyCurrencyId && data.CurrencyId != $scope.companyGroupCurrencyId)
                            CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': data.CurrencyId });
                        else if ($scope.SelectedCurrency === $scope.companyCurrencyId || $scope.SelectedCurrency === $scope.companyGroupCurrencyId)
                            CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'ParallelCurrencyType': 'CompanyCurrency' });
                        else if (data.CurrencyId === $scope.companyCurrencyId || data.CurrencyId === $scope.companyGroupCurrencyId)
                            CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'ParallelCurrencyType': 'CompanyGroupCurrency' });
                        else
                            CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': data.CurrencyId });

                        excRate = CurrencyRate[0].ToCurrencyRate;

                        if (data.CurrencyId != $scope.companyCurrencyId) {
                            dataRate = data.CompanyCurrencyRate;
                        }
                        else {
                            dataRate = data.CompanyGroupCurrencyRate;
                        }
                        /*Get Group currency Rate*/
                        var GroupCurrencyData = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': $scope.companyGroupCurrencyId });
                        grouprate = GroupCurrencyData[0].ToCurrencyRate;
                        /***********************************************/
                        var convertExcAmount = 0;
                        var convertDataAmount = 0;
                        if ($scope.companyCurrencyId === data.CurrencyId) {
                            convertExcAmount = parseFloat(data.ConvertedAmount / excRate).toFixed(4);
                            convertDataAmount = parseFloat(data.ConvertedAmount / data.CompanyGroupCurrencyRate).toFixed(4);
                        }
                        else if ($scope.companyGroupCurrencyId === data.CurrencyId) {
                            convertExcAmount = parseFloat(data.ConvertedAmount * excRate).toFixed(4);
                            convertDataAmount = parseFloat(data.ConvertedAmount * data.CompanyGroupCurrencyRate).toFixed(4);
                        }
                        else {
                            convertExcAmount = parseFloat((data.Amount * excRate) / grouprate).toFixed(4);
                            convertDataAmount = parseFloat(data.ConvertedAmount / data.CompanyGroupCurrencyRate).toFixed(4);
                        }
                        var loss = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });
                        var gain = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                        if (dataRate > excRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Group' });
                            var exlosslist = {};
                            exlosslist.InvoiceDetailId = data.InvoiceDetailId;
                            exlosslist.MultiplePaymentId = data.MultiplePaymentId;
                            exlosslist.DocRefNo = data.DocRefNo;
                            exlosslist.CompanyCurrencyId = $scope.companyCurrencyId;
                            exlosslist.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                            exlosslist.CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                            exlosslist.CompanyGroupCurrencyRate = excRate;
                            if (convertDataAmount < convertExcAmount) {
                                exlosslist.TrnType = 'Dr';
                                exlosslist.Exchange = 'Group';
                                exlosslist.ExchangeStatus = 'ExchangeLoss';
                                exlosslist.GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                                exlosslist.GLGeneralInfoName = loss[0].CompanyGroupCurrencyGL;
                                exlosslist.CompanyGroupCurrencyDr = Math.abs(convertDataAmount - convertExcAmount).toFixed(4);
                                exlosslist.CompanyGroupCurrencyCr = null;
                            }
                            else {
                                exlosslist.TrnType = 'Cr';
                                exlosslist.Exchange = 'Group';
                                exlosslist.ExchangeStatus = 'ExchangeGain';
                                exlosslist.GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                exlosslist.GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                exlosslist.CompanyGroupCurrencyCr = Math.abs(convertDataAmount - convertExcAmount).toFixed(4);
                                exlosslist.CompanyGroupCurrencyDr = null;
                            }
                            $scope.voucherDetailCurrencyList.push(exlosslist);
                        }
                        else if (dataRate < excRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Group' });
                            var exgainlist = {};
                            exgainlist.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                            exgainlist.CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                            exgainlist.CompanyGroupCurrencyRate = excRate;
                            exgainlist.InvoiceDetailId = data.InvoiceDetailId;
                            exgainlist.MultiplePaymentId = data.MultiplePaymentId;
                            exgainlist.DocRefNo = data.DocRefNo;
                            if (convertExcAmount < convertDataAmount) {
                                exgainlist.TrnType = 'Cr';
                                exgainlist.Exchange = 'Group';
                                exgainlist.ExchangeStatus = 'ExchangeGain';
                                exgainlist.GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                exgainlist.GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                exgainlist.CompanyGroupCurrencyDr = null;
                                exgainlist.CompanyGroupCurrencyCr = Math.abs(convertDataAmount - convertExcAmount).toFixed(4);
                            }
                            else {
                                exgainlist.TrnType = 'Dr';
                                exgainlist.Exchange = 'Group';
                                exgainlist.ExchangeStatus = 'ExchangeLoss';
                                exgainlist.GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                                exgainlist.GLGeneralInfoName = loss[0].CompanyGroupCurrencyGL;
                                exgainlist.CompanyGroupCurrencyCr = null;
                                exgainlist.CompanyGroupCurrencyDr = Math.abs(convertExcAmount - convertDataAmount).toFixed(4);
                            }
                            $scope.voucherDetailCurrencyList.push(exgainlist);
                        }
                        /*Except company and group currency transaction may possible exchange gain and loss while group exchange diffrence*/
                        else if (data.CurrencyId != $scope.companyCurrencyId && data.CurrencyId != $scope.companyGroupCurrencyId) {
                            if (data.CompanyGroupCurrencyRate < grouprate) {
                                var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Group' });
                                var exlosslist = {};
                                exlosslist.TrnType = 'Cr';
                                exlosslist.Exchange = 'Group';
                                exlosslist.ExchangeStatus = 'ExchangeGain';
                                exlosslist.InvoiceDetailId = data.InvoiceDetailId;
                                exlosslist.MultiplePaymentId = data.MultiplePaymentId;
                                exlosslist.GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                exlosslist.GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                exlosslist.DocRefNo = data.DocRefNo;
                                exlosslist.CompanyGroupCurrencyCr = (((data.Amount * dataRate) / data.CompanyGroupCurrencyRate) - ((data.Amount * dataRate) / grouprate)).toFixed(4);
                                exlosslist.CompanyGroupCurrencyDr = null;
                                exlosslist.CompanyCurrencyId = $scope.companyCurrencyId;
                                exlosslist.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                exlosslist.CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                exlosslist.CompanyGroupCurrencyRate = grouprate;
                                $scope.voucherDetailCurrencyList.push(exlosslist);
                            }
                            else if (data.CompanyGroupCurrencyRate > grouprate) {
                                var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Group' });
                                var exgainlist = {};
                                exgainlist.TrnType = 'Dr';
                                exgainlist.Exchange = 'Group';
                                exgainlist.ExchangeStatus = 'ExchangeLoss';
                                exgainlist.InvoiceDetailId = data.InvoiceDetailId;
                                exgainlist.MultiplePaymentId = data.MultiplePaymentId;
                                exgainlist.GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                                exgainlist.GLGeneralInfoName = loss[0].CompanyGroupCurrencyGL;
                                exgainlist.DocRefNo = data.DocRefNo;
                                exgainlist.CompanyGroupCurrencyDr = (((data.Amount * dataRate) / grouprate) - ((data.Amount * dataRate) / data.CompanyGroupCurrencyRate)).toFixed(4);
                                exgainlist.CompanyGroupCurrencyCr = null;
                                exgainlist.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                exgainlist.CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                exgainlist.CompanyGroupCurrencyRate = grouprate;
                                $scope.voucherDetailCurrencyList.push(exgainlist);
                            }
                        }
                    }
                }
                // Hard currency
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    obj.HardCurrencyId = $scope.hardCurrencyId;
                    obj.HardCurrencyName = $scope.hardCurrencyName;
                    obj.HardFromCurrencyId = data.HardFromCurrencyId;
                    obj.HardCurrencyRate = data.HardCurrencyRate;
                    obj.HardCurrencyCr = null;
                    obj.HardCurrencyDr = (data.Amount * data.HardCurrencyRate).toFixed(4);
                }
                $scope.voucherDetailCurrencyList.push(obj);
                console.log('voucherDetailCurrencyList', $scope.voucherDetailCurrencyList);
            }
        }
    };

    $scope.changeCurrencyExchangeRate = function () {
        angular.forEach($scope.voucherDetailCurrencyList, function (item, i) {
            if (item.TrnType === 'Dr' && item.ExchangeStatus === 'No') {
                $scope.setDrExchangeRatePayment(item);
                $scope.totalInvoiceAmount();
            }
        });
        angular.forEach($scope.voucherDetailCurrencyList, function (item, i) {
            if (item.TrnType === 'Cr' && item.ExchangeStatus === 'No') {
                if ($scope.CurrencyParallel.length == 1) {
                    $scope.setCrExchangeRatePayment(item.GLGeneralInfoId, $scope.voucher.Amount, item.GLGeneralInfoName, item.BudgetId, item.BudgetName, item.ActivityId, item.ActivityName, $scope.voucher.isSplit, 0);
                }
                else {
                    $scope.setCrExchangeRatePayment(item.GLGeneralInfoId, $scope.voucher.Amount, item.GLGeneralInfoName, item.BudgetId, item.BudgetName, item.ActivityId, item.ActivityName, $scope.voucher.isSplit, $scope.voucher.InvoiceGroupAmount);
                }
            }
        });
    };

    $scope.changeSourceFrom = function (from) {
        $scope.voucher.CrGLId = null;
        $scope.voucher.CrGLName = null;
        $scope.voucher.CrBudgetId = null;
        $scope.voucher.CrActivityId = null;
        $scope.voucher.BankName = null;
        $scope.voucher.SourceFrom = from;
        $scope.voucher.BankMasterId = null;
        $scope.isBankAmount = false;
        $scope.voucher.BankCurrencyId = null;
        for (var i = 0; i < $scope.voucherDetailCurrencyList.length; i++) {
            if ($scope.voucherDetailCurrencyList[i].TrnType === 'Cr' && $scope.voucherDetailCurrencyList[i].ExchangeStatus === 'No') {
                $scope.voucherDetailCurrencyList.splice(i, 1);
            }
        }
    };

    $scope.getCashGL = function (id) {
        var cashGl = $.grep($scope.cashGLList, function (item) {
            return item.GLGeneralInfoId === id;
        })[0];
        $scope.voucher.GLGeneralInfoId = cashGl.GLGeneralInfoId;
        $scope.voucher.GLGeneralInfoName = cashGl.GLCode + ' - ' + cashGl.GLGeneralInfoName;
        $scope.voucher.BudgetId = cashGl.BudgetId;
        $scope.voucher.ActivityId = cashGl.ActivityId;
        $scope.setCrExchangeRatePayment($scope.voucher.GLGeneralInfoId,
            $scope.voucher.Amount, $scope.voucher.GLGeneralInfoName,
            $scope.voucher.BudgetId, $scope.voucher.BudgetName,
            $scope.voucher.ActivityId, $scope.voucher.ActivityName, $scope.voucher.DocRefNo, $scope.voucher.InvoiceGroupAmount);
    };

    $scope.closeBankPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult('Please Select Currency !', 'failure', 'bankPopUp');
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
                $scope.voucher.BankMasterId = bank.Id;
                $scope.voucher.BankCurrencyId = bank.CurrencyId;

                $scope.voucher.GLGeneralInfoId = bank.GLGeneralInfoId;
                $scope.voucher.GLGeneralInfoName = bank.GLItem;
                $scope.voucher.BudgetId = bank.BudgetId;
                $scope.voucher.BudgetName = bank.BudgetName;
                $scope.voucher.ActivityId = bank.ActivityId;
                $scope.totalInvoiceAmount();
                $scope.setCrExchangeRatePayment($scope.voucher.GLGeneralInfoId, $scope.voucher.Amount,
                    $scope.voucher.GLGeneralInfoName,
                    $scope.voucher.BudgetId, $scope.voucher.BudgetName,
                    $scope.voucher.ActivityId, $scope.voucher.ActivityName, $scope.voucher.DocRefNo, $scope.voucher.InvoiceGroupAmount);
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

    $scope.customerAdvanceSearchList = [];
    $scope.customerAdvanceDataList = [];
    $scope.customerAdvanceSearch = [];
    $scope.customerAdvanceUrl = 'accounts/Advance/GetVendorAvilabeAdvanceList';
    $scope.customerAdvanceSelectedIndex = -1;
    $scope.customerAdvanceParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'VoucherNo',
        searchBy: 'VoucherNo',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.VendorAdvancePopUp = function (partyId) {
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.customerAdvanceDataList = [];
        }
        else {
            $scope.compareCurrencyId = $scope.voucher.CurrencyId;
            $scope.customerAdvanceParameters.partyId = partyId;
            $scope.getVendorAdvanceData = function (pageno) {
                baseService.paginationBase($scope.customerAdvanceUrl, pageno, $scope.customerAdvanceParameters)
                    .then(function (response) {
                        $scope.vendorAdvanceDataList = response.Rows;
                        $scope.customerAdvanceParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.customerAdvanceSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.customerAdvanceDataList, $scope.customerAdvanceSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#VendorAdvancePopUp')).modal('show');
            $scope.getVendorAdvanceData();
        }
    };
    $scope.closeVendorAdvancePopUpSelected = function (index, data) {
        angular.element(document.querySelector('#VendorAdvancePopUp')).modal('hide');
    };

    $scope.closeVendorAdvancePopUp = function () {
        angular.element(document.querySelector('#VendorAdvancePopUp')).modal('hide');
    }
    $scope.totalAdvanceAmount = function (partyid) {
        $scope.TotalAdvance = 0;
        $http.get('accounts/Advance/GetVendorTotalAdvanceAmount?partyId=' + partyid)
            .then(function successCallback(response) {
                $scope.TotalAdvance = response.data;
                console.log('TotalAdvance', $scope.TotalAdvance);
            },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
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
        $scope.voucher.CurrencyId = $scope.tranCurrencyList[0].Value;
    };

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult('Please select Currency!', 'failure');
            return true;
        }
        //if ($scope.CurrencyParallel.length == 2) {
        //    if ($scope.voucher.BankAmount != $scope.voucher.InvoiceGroupAmount) {
        //        ShowResult('Bank Amount and Group Currency Amount are not equal!', 'failure');
        //        return true;
        //    }
        //}
        return false;
    };

    $scope.Save = function () {
        $scope.VendorBankAmount();
        $scope.redirectTab();
        $scope.$broadcast('show-errors-check-validity');
        $scope.checkDocDate();
        $scope.checkPostingDate();

        $scope.entityValidation();
        if ($scope.form1.$valid && !$scope.validation() && !$scope.invalidPostingDate && $scope.checkDrCrBalancing()) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/Invoice/InsertMultipleVendorPaymentApproved',
                    data: {
                        'partyIdList': $scope.VendorBankAmountList,
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
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'accounts/Invoice/UpdateCustomerAdvance',
                    data: {
                        'voucherVM': $scope.voucher,
                        'currencyList': $scope.voucherDetailCurrencyList
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

    $scope.getDetailData = function (data) {
        $http({
            method: 'GET',
            url: 'accounts/Invoice/GetMultipleVendorAvailableDetailApprovalList?multiplePaymentId=' + data.MultiplePaymentId + '&partyId=' + data.PartyId,
        }).then(function successCallback(result) {
            $scope.paymentDetailList = result.data.Rows;
            console.log(result.Rows);
        });
    };
    // $scope.getDetailData();

    $scope.planPopUP = function (data) {
        $http({
            method: 'GET',
            url: 'accounts/Invoice/GetMultipleVendorApprovalList?multiplePaymentId=' + data.Id,
        }).then(function successCallback(result) {
            $scope.vendorPaymentDetailList = result.data.Rows;
            console.log(result.Rows);
        });
        angular.element(document.querySelector('#VendorPaymentPlanListPopUP')).modal('show');
        $scope.customerreceivableGLData();
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#VendorPaymentPlanListPopUP')).modal('hide');
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
    $scope.vendorInvoicePaymentReport = function (voucherNo) {
        location.href = 'accounts/invoice/vendorinvoicepaymentreport?voucherNo=' + voucherNo;
    };

    $scope.expandAll = function (expanded) {
        // $scope is required here, hence the injection above, even though we're using "controller as" syntax
        $scope.$broadcast('onExpandAll', {
            expanded: expanded
        });
    };
}