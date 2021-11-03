'use strict';
CustomerInterPlantCompanyReceiptController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function CustomerInterPlantCompanyReceiptController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Received';
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
    $scope.sourceType = 'ReceiveDeduction';
    $scope.interSourceType = 'InterTransaction';
    $scope.isAdvance = false;
    $scope.isReadOnly = false;
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller('bankBaseController', { $scope: $scope, $http: $http });
    $controller('cashBaseController', { $scope: $scope, $http: $http });
    baseService.init('accounts/Invoice/GetCustomerReceiptList', null, null, 'DESC', 'DocDate', 'DocDate');
    $scope.voucherDetailCurrencyList = [];
    $scope.partyGLList = [];

    $scope.voucher = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        EntityId: null,
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
        CashName: null,
        BankMasterId: null,
        CashMasterId: null,
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
        EmployeeTransactionTypeId: null,
        InvoiceAmount: 0,
        ExGainLossAmount: 0,
        NetInvoiceAmount: 0,
        InvoiceGroupAmount: 0,
        ExGainLossGroupAmount: 0,
        BankChargeAmount: 0,
        FinancingTypeBankChargeId: null,
        PartyType: 'Plant'
    };

    $scope.voucherDetail = {
        EntityId: null
    };

    $scope.invoiceTax = {
    };
    //$scope.voucherDetailList = [
    //    {
    //        InvoiceDeduction: [
    //            {
    //                //Id: null,
    //                //FinancingTypeId: null,
    //                //DeductAmount: 0
    //            }
    //        ]
    //    }
    //];

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

    cboService.getCboInterCompanyFinancingType($scope.sourceType, function (result) {
        $scope.financingTypeList = result;
    });

    cboService.getCboInterCompanyFinancingType($scope.sourceType, function (result) {
        $scope.financingTypeBankChargeList = result;
    });
    cboService.getCboInterPlant(null, null, null, function (result) {
        $scope.interplantList = result;
        //$scope.openingBalanceDetailList[0].PlantId = null;
    });

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
        for (var i = 0; i < baseService.arrayLength($scope.companyList); i++) {
            if ($scope.companyList[i].Value === $window.companyId)
                return $scope.companyList.splice(i, 1);
        }
    });

    $scope.changeSourceTo = function (to) {
        $scope.voucher.PartyType = to;
        //$scope.voucher.CompanyId = null;
        //$scope.voucher.EntityId = null;
        if (to === 'Company') {
            cboService.getCboInterCompanyFinancingType($scope.interSourceType, function (result) {
                $scope.financingTypeList = result;
            });
        }
        else if (to === 'Plant') {
            cboService.getCboInterPlantFinancingType($scope.interSourceType, function (result) {
                $scope.financingTypeList = result;
            });
        }
    };

    $scope.advanceCA = null;
    $scope.getTransactionTypeGL = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $http.get('accounts/Investment/GetFinancingTypeGL?id=' + id)
                .then(function (response) {
                    $scope.advanceCA = response.data;
                    if (manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.advanceCA.AssetGLId), 'Transaction Type GL not found!')) {
                        $scope.advanceCA = null;
                    }
                    else if ($scope.companyConfig.IsVoucherFromBudget
                        && manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.advanceCA.AssetBudgetId), 'Transaction Type Budget not found!')) {
                        $scope.advanceCA = null;
                    }
                });
        }
        else {
            manualValidation('div_TransactionType', false, '');
            $scope.advanceCA = null;
        }
    };

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
            cboService.getCboEntityByPlant(null, null, null, function (result) {
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

    cboService.getCboVoucherTypeAdvanceTakenList(function (result) {
        $scope.voucherTypeList = result;
        if ($scope.voucherTypeList.length === 1) {
            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
        }
    });

    $scope.exchangeGainLossList = [];
    $http.get('accounts/ExchangeGainLoss/GetExchangeGainLoss')
        .then(function successCallback(response) {
            $scope.exchangeGainLossList = response.data;
            if ($scope.exchangeGainLossList.length == 0) {
                $scope.pop('error', ' Exchange Gain and Loss GL is not determine');
            }
        },
        function errorCallback(response) {
            ShowResult(response, 'failure');
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
            $scope.customerReceivableGLUrl1 = 'accounts/Invoice/GetCustomerAvailableInvoiceList?partyid=' + $scope.voucher.PartyId;
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

    $scope.closePopUpselected = function (data, index) {
        data.Amount = 0;
        data.TrnType = 'Cr';
        var getRow = null;
        getRow = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Cr', 'DocRefNo': data.DocRefNo });
        if (getRow.length == 0) {
            $scope.voucherDetailList.push(data);
            $scope.GetCurrencyExchangeRateList(data.CurrencyId, data)

            if ($scope.voucherDetailList.length > 0)
                $scope.isReadOnly = true;
            else
                $scope.isReadOnly = false;

            angular.element(document.querySelector('#CustomerReceivableListPopUP')).modal('hide');
        }
        else {
            ShowResult(data.DocRefNo + ' already  Exist', 'failure', 'CustomerReceivableListPopUP');
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
        $scope.voucher.Amount = parseFloat($scope.voucher.InvoiceAmount) + parseFloat($scope.ExGainAmount) - parseFloat($scope.ExLossAmount);
        if ($scope.CurrencyParallel.length == 2) {
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
        $scope.totalInvoiceAmount();

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
            if ($scope.voucherDetailCurrencyList[i].TrnType === 'Dr' && $scope.voucherDetailCurrencyList[i].ExchangeStatus === 'No') {
                $scope.voucherDetailCurrencyList.splice(i, 1);
            }
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
                $scope.voucher.GLGeneralInfoId = cash.GLGeneralInfoId;
                $scope.voucher.GLGeneralInfoName = cash.GLItem;
                $scope.voucher.BudgetId = cash.BudgetId;
                $scope.voucher.ActivityId = cash.ActivityId;
                $scope.voucher.EntityId = cash.EntityId;
                $scope.totalInvoiceAmount();
                $scope.setDrExchangeRate($scope.voucher.Amount,
                    $scope.voucher.GLGeneralInfoId, $scope.voucher.GLGeneralInfoName,
                    $scope.voucher.BudgetId, $scope.voucher.BudgetName,
                    $scope.voucher.ActivityId, $scope.voucher.ActivityName, $scope.voucher.DocRefNo, false, $scope.voucher.InvoiceGroupAmount);
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
                $scope.voucher.BankMasterId = bank.Id;
                $scope.voucher.BankCurrencyId = bank.CurrencyId;

                $scope.voucher.GLGeneralInfoId = bank.GLGeneralInfoId;
                $scope.voucher.GLGeneralInfoName = bank.GLItem;
                $scope.voucher.BudgetId = bank.BudgetId;
                $scope.voucher.ActivityId = bank.ActivityId;
                $scope.voucher.EntityId = bank.EntityId;
                $scope.totalInvoiceAmount();
                $scope.setDrExchangeRate($scope.voucher.Amount,
                    $scope.voucher.GLGeneralInfoId, $scope.voucher.GLGeneralInfoName,
                    $scope.voucher.BudgetId, $scope.voucher.BudgetName,
                    $scope.voucher.ActivityId, $scope.voucher.ActivityName, $scope.voucher.DocRefNo, false, $scope.voucher.InvoiceGroupAmount);
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
    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult('Please select Currency!', 'failure');
            return true;
        }

        if ($scope.partyType == 'Customer') {
            if (baseService.isUndefinedOrNull($scope.voucher.PartyId)) {
                ShowResult('Please select Customer!', 'failure');
                return true;
            }
            if (parseFloat($scope.voucher.Amount) == 0) {
                ShowResult(' Amount must greater than 0!', 'failure');
                return true;
            }
            var vdetailCr = $filter('filter')($scope.voucherDetailList, { TrnType: 'Cr' });
            if (vdetailCr.length == 0) {
                ShowResult('Please Select Customer Receivable !', 'failure');
                return true;
            }
            if (baseService.isUndefinedOrNull($scope.voucher.GLGeneralInfoId)) {
                ShowResult('Please select Cash or Bank!', 'failure');
                return true;
            }
        }
        else if ($scope.partyType == 'Vendor') {
            if (baseService.isUndefinedOrNull($scope.voucher.PartyId)) {
                ShowResult('Please select Vendor!', 'failure');
                return true;
            }
            var vdetailDr = $filter('filter')($scope.voucherDetailList, { TrnType: 'Dr' })
            if (vdetailDr.length == 0) {
                ShowResult('There is no Purchase Entry!', 'failure');
                return true;
            }
        }
        if (baseService.isUndefinedOrNull($scope.voucher.EntityId)) {
            ShowResult('Please select Entity!', 'failure');
            return true;
        }
        if ($scope.CurrencyParallel.length == 2) {
            if ($scope.voucher.BankCurrencyId == $scope.companyGroupCurrencyId) {
                if ($scope.voucher.BankAmount != $scope.voucher.InvoiceGroupAmount) {
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
                    url: 'accounts/Invoice/InsertCustomerReceipt',
                    data: {
                        'voucherVM': $scope.voucher,
                        'voucherDetailVMList': $scope.voucherDetailList,
                        'voucherDetailCurrencyVMList': $scope.voucherDetailCurrencyList,
                        'deductionList': $scope.deductionList
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
    $scope.setCrExchangeRate = function (data) {
        if (!baseService.isUndefinedOrNull(data.GLGeneralInfoId)) {
            var companyCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
            var companyGroupCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
            var getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Cr', 'InvoiceDetailId': data.InvoiceDetailId, 'GLGeneralInfoId': data.GLGeneralInfoId });
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
                getRow[0].TrnType = 'Cr';
                getRow[0].InvoiceDetailId = data.InvoiceDetailId;
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
                        getRow[0].CompanyCurrencyCr = (data.ConvertedAmount / data.CompanyCurrencyRate).toFixed(4);
                    }
                    else if (baseService.isUndefinedOrNull($scope.companyGroupCurrencyId) && data.CurrencyId != $scope.companyCurrencyId) {
                        getRow[0].CompanyCurrencyCr = (data.Amount * data.CompanyCurrencyRate).toFixed(4);
                    }
                    else if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId) && data.CurrencyId == $scope.companyGroupCurrencyId) {
                        getRow[0].CompanyCurrencyCr = (data.ConvertedAmount) /*(data.ConvertedAmount * data.CompanyCurrencyRate).toFixed(4)*/;
                    }
                    else {
                        getRow[0].CompanyCurrencyCr = data.ConvertedAmount;
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
                        if (data.CompanyCurrencyRate > excRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Base' });
                            if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                var loss = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });
                                rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                rowGroup[0].TrnType = 'Dr';
                                rowGroup[0].Exchange = 'Base';
                                rowGroup[0].ExchangeStatus = 'ExchangeLoss';
                                rowGroup[0].GLGeneralInfoId = loss[0].CompanyCurrencyGLId;
                                rowGroup[0].GLGeneralInfoName = loss[0].CompanyCurrencyGL;
                                rowGroup[0].DocRefNo = data.DocRefNo;
                                if ($scope.companyCurrencyId != data.CurrencyId) {
                                    rowGroup[0].CompanyCurrencyDr = (data.Amount * (data.CompanyCurrencyRate - excRate)).toFixed(4);
                                }
                                else
                                    rowGroup[0].CompanyCurrencyDr = (data.ConvertedAmount * (data.CompanyCurrencyRate - excRate)).toFixed(4);

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
                                    exlosslist.CompanyCurrencyDr = (data.Amount * (data.CompanyCurrencyRate - excRate)).toFixed(4);
                                }
                                else /*if ($scope.companyGroupCurrencyId === data.CurrencyId) {*/
                                    exlosslist.CompanyCurrencyDr = (data.ConvertedAmount * (data.CompanyCurrencyRate - excRate)).toFixed(4);
                                //}
                                exlosslist.CompanyCurrencyCr = null;
                                exlosslist.CompanyCurrencyId = $scope.companyCurrencyId;
                                exlosslist.CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                                exlosslist.CompanyCurrencyRate = excRate;
                                $scope.voucherDetailCurrencyList.push(exlosslist);
                            }
                        }
                        else if (data.CompanyCurrencyRate < excRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Base' });
                            if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                var gain = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                                rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                rowGroup[0].TrnType = 'Cr';
                                rowGroup[0].Exchange = 'Base';
                                rowGroup[0].ExchangeStatus = 'ExchangeGain';
                                rowGroup[0].GLGeneralInfoId = gain[0].CompanyCurrencyGLId;
                                rowGroup[0].GLGeneralInfoName = gain[0].CompanyCurrencyGL;
                                rowGroup[0].DocRefNo = data.DocRefNo;
                                rowGroup[0].CompanyCurrencyDr = null;

                                if ($scope.companyCurrencyId != data.CurrencyId) {
                                    rowGroup[0].CompanyCurrencyCr = Math.abs(data.Amount * (excRate - data.CompanyCurrencyRate)).toFixed(4);
                                }
                                else //if ($scope.companyGroupCurrencyId === data.CurrencyId) {
                                    rowGroup[0].CompanyCurrencyCr = Math.abs(data.ConvertedAmount * (excRate - data.CompanyCurrencyRate)).toFixed(4);
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
                                    exgainlist.CompanyCurrencyCr = Math.abs(data.Amount * (excRate - data.CompanyCurrencyRate)).toFixed(4);
                                }
                                else //if ($scope.companyGroupCurrencyId === data.CurrencyId) {
                                    exgainlist.CompanyCurrencyCr = Math.abs(data.ConvertedAmount * (excRate - data.CompanyCurrencyRate)).toFixed(4);
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
                    getRow[0].CompanyGroupCurrencyDr = null;
                    if (data.CurrencyId == $scope.companyCurrencyId) {
                        getRow[0].CompanyGroupCurrencyCr = (data.ConvertedAmount / data.CompanyGroupCurrencyRate).toFixed(4);
                    }
                    else if (data.CurrencyId == $scope.companyGroupCurrencyId) {
                        getRow[0].CompanyGroupCurrencyCr = data.Amount;
                    }
                    else {
                        getRow[0].CompanyGroupCurrencyCr = (data.ConvertedAmount / data.CompanyGroupCurrencyRate).toFixed(4);
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
                                rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                rowGroup[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                rowGroup[0].CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                rowGroup[0].CompanyGroupCurrencyRate = excRate;
                                rowGroup[0].DocRefNo = data.DocRefNo;
                                if (convertDataAmount < convertExcAmount) {
                                    rowGroup[0].TrnType = 'Cr';
                                    rowGroup[0].Exchange = 'Group';
                                    rowGroup[0].ExchangeStatus = 'ExchangeGain';
                                    rowGroup[0].GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                    rowGroup[0].GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                    rowGroup[0].CompanyGroupCurrencyCr = Math.abs(convertDataAmount - convertExcAmount).toFixed(4);
                                    rowGroup[0].CompanyGroupCurrencyDr = null;
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
                                var exlosslist = {};
                                exlosslist.InvoiceDetailId = data.InvoiceDetailId;
                                exlosslist.DocRefNo = data.DocRefNo;
                                exlosslist.CompanyCurrencyId = $scope.companyCurrencyId;
                                exlosslist.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                exlosslist.CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                exlosslist.CompanyGroupCurrencyRate = excRate;
                                if (convertDataAmount < convertExcAmount) {
                                    exlosslist.TrnType = 'Cr';
                                    exlosslist.Exchange = 'Group';
                                    exlosslist.ExchangeStatus = 'ExchangeGain';
                                    exlosslist.GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                    exlosslist.GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                    exlosslist.CompanyGroupCurrencyCr = Math.abs(convertDataAmount - convertExcAmount).toFixed(4);
                                    exlosslist.CompanyGroupCurrencyDr = null;
                                }
                                else {
                                    exlosslist.TrnType = 'Dr';
                                    exlosslist.Exchange = 'Group';
                                    exlosslist.ExchangeStatus = 'ExchangeLoss';
                                    exlosslist.GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                                    exlosslist.GLGeneralInfoName = loss[0].CompanyGroupCurrencyGL;
                                    exlosslist.CompanyGroupCurrencyDr = Math.abs(convertDataAmount - convertExcAmount).toFixed(4);
                                    exlosslist.CompanyGroupCurrencyCr = null;
                                }
                                $scope.voucherDetailCurrencyList.push(exlosslist);
                            }
                        }
                        else if (dataRate < excRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Group' });
                            if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                rowGroup[0].DocRefNo = data.DocRefNo;
                                rowGroup[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                rowGroup[0].CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                rowGroup[0].CompanyGroupCurrencyRate = excRate;
                                /*when group rate change large then gain may change loss*/
                                if (convertExcAmount < convertDataAmount) {
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
                                    rowGroup[0].CompanyGroupCurrencyDr = null;
                                    rowGroup[0].CompanyGroupCurrencyCr = Math.abs(convertDataAmount - convertExcAmount).toFixed(4);
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
                                    exgainlist.TrnType = 'Dr';
                                    exgainlist.Exchange = 'Group';
                                    exgainlist.ExchangeStatus = 'ExchangeLoss';
                                    exgainlist.GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                                    exgainlist.GLGeneralInfoName = loss[0].CompanyGroupCurrencyGL;
                                    exgainlist.CompanyGroupCurrencyCr = null;
                                    exgainlist.CompanyGroupCurrencyDr = Math.abs(convertExcAmount - convertDataAmount).toFixed(4);
                                }
                                else {
                                    exgainlist.TrnType = 'Cr';
                                    exgainlist.Exchange = 'Group';
                                    exgainlist.ExchangeStatus = 'ExchangeGain';
                                    exgainlist.GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                    exgainlist.GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                    exgainlist.CompanyGroupCurrencyDr = null;
                                    exgainlist.CompanyGroupCurrencyCr = Math.abs(convertDataAmount - convertExcAmount).toFixed(4);
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
                                        rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                        rowGroup[0].TrnType = 'Dr';
                                        rowGroup[0].Exchange = 'Group';
                                        rowGroup[0].ExchangeStatus = 'ExchangeLoss';
                                        rowGroup[0].GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                                        rowGroup[0].GLGeneralInfoName = loss[0].CompanyGroupCurrencyGL;
                                        rowGroup[0].DocRefNo = data.DocRefNo;
                                        rowGroup[0].CompanyGroupCurrencyDr = (((data.Amount * dataRate) / data.CompanyGroupCurrencyRate) - ((data.Amount * dataRate) / grouprate)).toFixed(4);
                                        rowGroup[0].CompanyGroupCurrencyCr = null
                                        rowGroup[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                        rowGroup[0].CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                        rowGroup[0].CompanyGroupCurrencyRate = excRate;
                                    }
                                    else {
                                        var exlosslist = {};
                                        exlosslist.TrnType = 'Dr';
                                        exlosslist.Exchange = 'Group';
                                        exlosslist.ExchangeStatus = 'ExchangeLoss';
                                        exlosslist.InvoiceDetailId = data.InvoiceDetailId;
                                        exlosslist.GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                                        exlosslist.GLGeneralInfoName = loss[0].CompanyGroupCurrencyGL;
                                        exlosslist.DocRefNo = data.DocRefNo;
                                        exlosslist.CompanyGroupCurrencyDr = (((data.Amount * dataRate) / data.CompanyGroupCurrencyRate) - ((data.Amount * dataRate) / grouprate)).toFixed(4);
                                        exlosslist.CompanyGroupCurrencyCr = null;
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
                                        rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                        rowGroup[0].TrnType = 'Cr';
                                        rowGroup[0].Exchange = 'Group';
                                        rowGroup[0].ExchangeStatus = 'ExchangeGain';
                                        rowGroup[0].GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                        rowGroup[0].GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                        rowGroup[0].DocRefNo = data.DocRefNo;
                                        rowGroup[0].CompanyGroupCurrencyDr = null;
                                        rowGroup[0].CompanyGroupCurrencyCr = (((data.Amount * dataRate) / grouprate) - ((data.Amount * dataRate) / data.CompanyGroupCurrencyRate)).toFixed(4);
                                        rowGroup[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                                        rowGroup[0].CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                                        rowGroup[0].CompanyGroupCurrencyRate = excRate;
                                    }
                                    else {
                                        var exgainlist = {};
                                        exgainlist.TrnType = 'Cr';
                                        exgainlist.Exchange = 'Group';
                                        exgainlist.ExchangeStatus = 'ExchangeGain';
                                        exgainlist.InvoiceDetailId = data.InvoiceDetailId;
                                        exgainlist.GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                                        exgainlist.GLGeneralInfoName = gain[0].CompanyGroupCurrencyGL;
                                        exgainlist.DocRefNo = data.DocRefNo;
                                        exgainlist.CompanyGroupCurrencyCr = (((data.Amount * dataRate) / grouprate) - ((data.Amount * dataRate) / data.CompanyGroupCurrencyRate)).toFixed(4);
                                        exgainlist.CompanyGroupCurrencyDr = null;
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
                    getRow[0].HardCurrencyDr = null;
                    getRow[0].HardCurrencyCr = data.ConvertedAmount * data.HardCurrencyConversion;
                }
            }
            else {
                var obj = {
                    TrnType: 'Cr',
                    Exchange: 'No',
                    ExchangeStatus: 'No',
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
                    obj.CompanyCurrencyDr = null;
                    obj.CompanyCurrencyCr = (data.ConvertedAmount * data.CompanyCurrencyRate).toFixed(4);

                    if ($scope.companyCurrencyId != data.CurrencyId) {
                        var CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': data.CurrencyId });
                        excRate = CurrencyRate[0].ToCurrencyRate;
                        if (data.CompanyCurrencyRate > excRate) {
                            var loss = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });
                            $scope.voucherDetailCurrencyList.push({
                                TrnType: 'Dr',
                                Exchange: 'Base',
                                ExchangeStatus: 'ExchangeLoss',
                                InvoiceDetailId: data.InvoiceDetailId,
                                GLGeneralInfoId: loss[0].CompanyCurrencyGLId,
                                GLGeneralInfoName: loss[0].CompanyCurrencyGL,
                                DocRefNo: data.DocRefNo,
                                //BudgetId: voucher.CrBudgetId,
                                //BudgetName: voucher.CrBudgetName,
                                //ActivityId: voucher.CrActivityId,
                                //ActivityName: voucher.CrActivityName,

                                CompanyCurrencyDr: Math.abs(data.Amount * (1 / excRate - 1 / data.CompanyGroupCurrencyRate)).toFixed(4),
                                CompanyCurrencyCr: null,
                                CompanyCurrencyId: $scope.companyCurrencyId,
                                CompanyFromCurrencyId: data.CompanyFromCurrencyId,
                                CompanyCurrencyRate: excRate
                            });
                        }
                        else if (data.CompanyCurrencyRate < excRate) {
                            var gain = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                            $scope.voucherDetailCurrencyList.push({
                                TrnType: 'Cr',
                                Exchange: 'Base',
                                ExchangeStatus: 'ExchangeGain',
                                InvoiceDetailId: data.InvoiceDetailId,
                                GLGeneralInfoId: gain[0].CompanyCurrencyGLId,
                                GLGeneralInfoName: gain[0].CompanyCurrencyGL,
                                DocRefNo: data.DocRefNo,
                                //BudgetId: voucher.CrBudgetId,
                                //BudgetName: voucher.CrBudgetName,
                                //ActivityId: voucher.CrActivityId,
                                //ActivityName: voucher.CrActivityName,

                                CompanyCurrencyDr: null,
                                CompanyCurrencyCr: Math.abs(data.Amount * (1 / data.CompanyCurrencyRate - 1 / excRate)).toFixed(4),
                                CompanyCurrencyId: $scope.companyCurrencyId,
                                CompanyFromCurrencyId: data.CompanyFromCurrencyId,
                                CompanyCurrencyRate: excRate
                            });
                        }
                    }
                }

                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    obj.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    obj.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;
                    obj.CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                    obj.CompanyGroupCurrencyRate = data.CompanyGroupCurrencyRate;
                    obj.CompanyGroupCurrencyDr = null;
                    obj.CompanyGroupCurrencyCr = data.ConvertedAmount / data.CompanyGroupCurrencyRate;

                    // Group currecny gain/loss
                    if (data.CurrencyId != $scope.companyGroupCurrencyId) {
                        var CurrencyRate;
                        if (CheckParaCurrency.length > 0)
                            CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'ParallelCurrencyType': 'CompanyCurrency' });
                        else
                            CurrencyRate = $filter('filter')($scope.currencyExchangeRate, { 'FromCurrencyId': data.CurrencyId });

                        excRate = CurrencyRate[0].ToCurrencyRate;
                        if (data.CompanyCurrencyRate < excRate) {
                            var loss = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });
                            $scope.voucherDetailCurrencyList.push({
                                TrnType: 'Dr',
                                Exchange: 'Group',
                                ExchangeStatus: 'ExchangeLoss',
                                InvoiceDetailId: data.InvoiceDetailId,
                                GLGeneralInfoId: loss[0].CompanyGroupCurrencyGLId,
                                GLGeneralInfoName: loss[0].CompanyGroupCurrencyGL,
                                DocRefNo: data.DocRefNo,
                                //BudgetId: voucher.CrBudgetId,
                                //BudgetName: voucher.CrBudgetName,
                                //ActivityId: voucher.CrActivityId,
                                //ActivityName: voucher.CrActivityName,

                                CompanyGroupCurrencyDr: Math.abs(data.ConvertedAmount * (1 / dataRate - 1 / excRate)).toFixed(4),
                                CompanyGroupCurrencyCr: null,
                                CompanyGroupCurrencyId: $scope.companyGroupCurrencyId,
                                CompanyGroupFromCurrencyId: data.CompanyGroupFromCurrencyId,
                                CompanyGroupCurrencyRate: excRate
                            });
                        }
                        else if (data.CompanyCurrencyRate > excRate) {
                            var gain = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                            $scope.voucherDetailCurrencyList.push({
                                TrnType: 'Cr',
                                Exchange: 'Group',
                                ExchangeStatus: 'ExchangeGain',
                                InvoiceDetailId: data.InvoiceDetailId,
                                GLGeneralInfoId: gain[0].CompanyGroupCurrencyGLId,
                                GLGeneralInfoName: gain[0].CompanyGroupCurrencyGL,
                                DocRefNo: data.DocRefNo,
                                //BudgetId: voucher.CrBudgetId,
                                //BudgetName: voucher.CrBudgetName,
                                //ActivityId: voucher.CrActivityId,
                                //ActivityName: voucher.CrActivityName,
                                CompanyGroupCurrencyDr: null,
                                CompanyGroupCurrencyCr: Math.abs(data.ConvertedAmount * (1 / excRate - 1 / dataRate)).toFixed(4),
                                CompanyGroupCurrencyId: $scope.companyGroupCurrencyId,
                                CompanyGroupFromCurrencyId: data.CompanyGroupFromCurrencyId,
                                CompanyGroupCurrencyRate: excRate
                            });
                        }
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
                }
                $scope.voucherDetailCurrencyList.push(obj);
                console.log('voucherDetailCurrencyList', $scope.voucherDetailCurrencyList);
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
                if ($scope.CurrencyParallel.length == 1) {
                    $scope.setDrExchangeRate($scope.voucher.Amount, item.GLGeneralInfoId, item.GLGeneralInfoName, item.BudgetId, item.BudgetName, item.ActivityId, item.ActivityName, $scope.voucher.DocRefNo, $scope.voucher.isSplit, 0);
                }
                else {
                    $scope.setDrExchangeRate($scope.voucher.Amount, item.GLGeneralInfoId, item.GLGeneralInfoName, item.BudgetId, item.BudgetName, item.ActivityId, item.ActivityName, $scope.voucher.DocRefNo, $scope.voucher.isSplit, $scope.voucher.InvoiceGroupAmount);
                }
            }
        });
    };

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

                    $scope.setCrExchangeRate(data);
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
                    $scope.setCrExchangeRate(data);
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
                            // $scope.GetCurrencyExchangeRateList();
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

    $scope.customerAdvanceSearchList = [];
    $scope.customerAdvanceDataList = [];
    $scope.customerAdvanceSearch = [];
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

    $scope.showCustomerAdvancePopUp = function (partyId) {
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.customerAdvanceDataList = [];
            ShowResult('Please select Customer.', 'failure');
        }
        else {
            $scope.compareCurrencyId = $scope.voucher.CurrencyId;
            $scope.customerAdvanceParameters.partyId = partyId;
            $scope.getCustomerAdvanceData = function (pageno) {
                baseService.paginationBase('accounts/Advance/GetAvilabeAdvanceList', pageno, $scope.customerAdvanceParameters)
                    .then(function (response) {
                        $scope.customerAdvanceDataList = response.Rows;
                        $scope.customerAdvanceParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.customerAdvanceSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.customerAdvanceDataList, $scope.customerAdvanceSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#customerAdvancePopUp')).modal('show');
            $scope.getCustomerAdvanceData();
        }
    };

    $scope.closeCustomerAdvancePopUp = function () {
        angular.element(document.querySelector('#customerAdvancePopUp')).modal('hide');
    };

    $scope.closeCustomerAdvancePopUpSelected = function (index, data) {
        angular.element(document.querySelector('#customerAdvancePopUp')).modal('hide');
    };
    $scope.totalAdvanceAmount = function (partyid, partyName) {
        $scope.TotalAdvance = 0;
        $http.get('accounts/Advance/GetCustomerTotalAdvanceAmount?partyId=' + partyid)
            .then(function successCallback(response) {
                $scope.TotalAdvance = response.data;
                //console.log('TotalAdvance', $scope.TotalAdvance);
                //$scope.pop('success', partyName + ' Total Advance Amount is BDT ' + $scope.TotalAdvance);
                if ($scope.TotalAdvance > 0) {
                    angular.element(document.querySelector('#TotaladvancePopUp')).modal('show');
                }
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    $scope.closeTotaladvancePopUp = function () {
        angular.element(document.querySelector('#TotaladvancePopUp')).modal('hide');
    };
    $scope.deductionOb = {
        FinancingTypeId: null,
        InvoiceDetailId: null,
        Deduction: null,
        Amount: null
    }
    $scope.getDeductionPopUp = function (index, data) {
        $scope.TempinvoiceData = {};
        $scope.setIndex = index;
        $scope.TempinvoiceData = data;
        angular.element(document.querySelector('#deductionPopUp')).modal('show');
    }
    $scope.deductionList = [];
    $scope.addDeduction = function () {
        $scope.deductionOb.Deduction = document.getElementById("deductioncbo").options[document.getElementById('deductioncbo').selectedIndex].text;
        $scope.deductionOb.InvoiceDetailId = $scope.TempinvoiceData.InvoiceDetailId;
        if ($scope.deductionList.length > 0) {
            angular.forEach($scope.deductionList, function (item) {
                if (item.FinancingTypeId != $scope.deductionOb.FinancingTypeId) {
                    $scope.deductionList.push($scope.deductionOb);
                } else {
                    return ShowResult('Same deduction already exist.', 'failure', 'deductionPopUp');
                }
            });
        } else {
            $scope.deductionList.push($scope.deductionOb);
        }
        $scope.deductionOb = {
            FinancingTypeId: null,
            InvoiceDetailId: null,
            Deduction: null,
            Amount: null
        }
    }
    $scope.removeRowModal = function (index) {
        $scope.deductionList.splice(index, 1);
    }
    $scope.closeDeducitonPopUp = function () {
        var cam = 0;
        angular.forEach($scope.deductionList, function (item) {
            cam = cam + item.Amount;
        });
        if (cam > $scope.TempinvoiceData.Amount) {
            return ShowResult('Deduction amount can not be greater than voucher detail', 'failure', 'deductionPopUp');
        }
        $scope.setIndex = null;
        $scope.TempinvoiceData = {};
        angular.element(document.querySelector('#deductionPopUp')).modal('hide');
    }
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

    $scope.customerInvoiceReceiptReport = function (voucherNo) {
        location.href = 'accounts/invoice/CustomerInvoiceReceiveReport?voucherNo=' + voucherNo;
    };
}