'use strict';
securityDepositWriteOffController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'toaster'];
function securityDepositWriteOffController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, toaster) {
    $rootScope.title = 'Security Deposit Write-off';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.voucherDetailCurrency = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.advanceList = [];
    $scope.voucherDetailList = [];
    baseService.init('accounts/SecurityDeposit/GetSecurityDepositWriteOffList', null, null, 'DESC', 'PostingDate', 'PostingDate');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.advanceList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.tranCurrencyList = result;
    });

    cboService.getCboVoucherType(function (result) {
        $scope.voucherTypeList = result;
    });

    $scope.advance = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PartyId: null,
        PartyName: null,
        PartyGLGeneralInfoId: null,
        GLGeneralInfoId: null,
        CurrencyId: null,
        VoucherTypeId: null,
        PartyType: 'Customer',
        Type: 'Receivable',
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
        Amount: 0,
        BaseOnDueDate: $filter("dateFiltering")(Date.now()),
        BaseNoOfDays: null,
        PaymentTermId: null,
        Narration: null,
        Remarks: null,
        BankName: null,
        BankAccountNumber: null,
        BankGL: null,
        BankGLGeneralInfoId: null
    };

    $scope.voucherDetail = {
        Id: null,
        VoucherId: null,
        CustomerInvoiceDetailId: null,
        BudgetMasterId: null,
        BudgetActivityId: null,
        CurrencyId: null,
        VoucherTypeId: null,
        GLGeneralInfoId: null,
        COAICode: null,
        COAIText: null,
        GLTextAndCode: null,
        OldCOAICode: null,
        DocRefNo: null,
        DocDate: $filter('date')(Date.now(), 'dd-MMM-yyyy'),
        FiscalYear: null,
        FiscalYearText: null,
        FiscalYearPeriod: null,
        FiscalYearPeriodText: null,
        DrAmount: 0,
        CrAmount: 0,
        Amount: 0,
        TaxAmount: 0,
        NetAmount: 0,
        Active: true
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
        Type: null
    };

    // Creating parallel currency table heading.
    $scope.parallelCurrencyTableHead = '<tr><th style="width:250px; vertical-align: middle; text-align:center" rowspan="2">GL</th>';
    var debitCreditHead = '</tr><tr>';
    $scope.parallelCurrencyTypeList = [];
    $scope.companyCurrencyId = null;

    $scope.GetCurrencyParallel = function () {
        $http({
            method: 'GET',
            url: 'currencies/CompanyParallelCurrency/CurrencyParallel',
        }).then(function successCallback(response) {
            angular.forEach(response.data, function (item, i) {
                $scope.parallelCurrencyTableHead += '<th style="text-align:center" colspan="2">' + item.Code + '</th>';
                debitCreditHead += '<th>Dr</th><th>Cr</th>';
                if (item.ParallelCurrencyType == 'CompanyCurrency') {
                    $scope.companyCurrencyId = item.CurrencyId;
                    $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyCurrency', CurrencyType: 'CompanyCurrencyDr', CurrencyId: item.CurrencyId });
                    $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyCurrency', CurrencyType: 'CompanyCurrencyCr', CurrencyId: item.CurrencyId });
                }
                else if (item.ParallelCurrencyType == 'CompanyGroupCurrency') {
                    $scope.companyGroupCurrencyId = item.CurrencyId;
                    $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyGroupCurrency', CurrencyType: 'CompanyGroupCurrencyDr', CurrencyId: item.CurrencyId });
                    $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyGroupCurrency', CurrencyType: 'CompanyGroupCurrencyCr', CurrencyId: item.CurrencyId });
                }
                else if (item.ParallelCurrencyType == 'HardCurrency') {
                    $scope.hardCurrencyId = item.CurrencyId;
                    $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'HardCurrency', CurrencyType: 'HardCurrencyDr', CurrencyId: item.CurrencyId });
                    $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'HardCurrency', CurrencyType: 'HardCurrencyCr', CurrencyId: item.CurrencyId });
                }
            })
            $scope.parallelCurrencyTableHead += debitCreditHead + '</tr>';
        });
    };
    $scope.GetCurrencyParallel();

    $('.datepicker').datepicker({
        format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
    });

    $scope.searchVendorInvoiceList = [
        {
            'name': 'PostingDate',
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
        },
    ];

    // #region ********Get CustomerInvoice************
    $scope.customerInvoice = [];
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.advance = $scope.customerInvoices[$scope.index];
        $scope.advance.DocDate = $filter("dateFiltering")($scope.advance.DocDate);
        $scope.advance.VoucherDate = $filter("dateFiltering")($scope.advance.VoucherDate);
        $scope.advance.PostingDate = $filter("dateFiltering")($scope.advance.PostingDate);
        $scope.GetAdditionalexchangerate($scope.advance.Id);
        $scope.onVoucherDetailCurrencyExchangeRateSelected($scope.advance.Id);
        $scope.crRowSelected = $scope.advance.PartyId;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.exchangeGainLossList = [];
    $http.get('accounts/ExchangeGainLoss/GetExchangeGainLoss')
        .then(function successCallback(response) {
            $scope.exchangeGainLossList = response.data;
        },
        function errorCallback(response) {
            ShowResult(response, 'failure');
        });

    $scope.pop = function (type, msg) {
        toaster.pop({
            type: type,
            body: msg,
            timeout: 3000
        });
    };

    $scope.popVoucherCode = function (type, msg) {
        toaster.pop({
            type: type,
            body: msg,
            timeout: 10000
        });
    };

    $scope.checkDrAmount = function () {
        if ($scope.voucherDetail.CrAmount > 0) {
            $scope.voucherDetail.DrAmount = 0;
        }
    }

    function validationAddGL(obj) {
        try {
            obj.FiscalYearText = $('#FiscalYear option:selected').text();
            obj.FiscalYearPeriodText = $('#FiscalYearPeriod option:selected').text();

            if (baseService.isUndefinedOrNull(obj.COAICode)) {
                throw 'Please Select GL!!';
            }
            if ($scope.advance.Narration == '' || $scope.advance.Narration == null) {
                throw 'Please input narration!!'
            }
            if ($scope.advance.DocRefNo == '' || $scope.advance.DocRefNo == null) {
                throw 'Please input DocRefNo!!'
            }
            if (obj.DrAmount == 0 && obj.CrAmount == 0) {
                throw 'Please Input Devit Amount or Credit Amount!!';
            }
        } catch (e) {
            throw ShowResult(e, 'failure');
        }
    };

    $scope.checkCrAndDrEquealMsg = '';
    $scope.checkCrAndDrEqueal = function () {
        if ($scope.Crtotal == $scope.customerInvoice.Amount) {
            $scope.checkCrAndDrEquealMsg = '';
            return true;
        } else {
            $scope.pop('error', 'Debit and Credit is not equeal');
            return false
        }
    }

    $scope.postingDateMessage = '';
    $('#postingDate').datepicker().on('changeDate', function (ev) {
        $scope.advance.PostingDate = ev.date;
        if (new Date($scope.advance.PostingDate) > new Date()) {
            $scope.postingDateMessage = 'Posting date must be below or equal to current Date!';
            $scope.advance.PostingDate = '';
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
        }
        else if ($scope.advance.PostingDate > $scope.advance.DocDate) {
            $scope.postingDateMessage = 'Posting date must be below or equal to Doc Date!';
            $scope.advance.PostingDate = '';
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
        } else {
            $scope.getPostingFiscalYearPeriod($scope.advance.PostingDate);
            $scope.postingDateMessage = '';
        }
    });

    $scope.getPostingFiscalYearPeriod = function (date) {
        $http({
            method: 'get',
            url: 'accounts/CompanyFiscalYear/CheckingFiscalYearPeriod?postingDate=' + $filter("dateFiltering")(date),
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.postingDateMessage = response.data.Message
                }
                else {
                    var result = response.data;
                    if (result.IsTransationLocked === true) {
                        ShowResult(commonMessage.FiscalPeriodTransactionLocked, 'failure');
                        $scope.advance.PostingDate = '';
                        $scope.advance.FiscalYearId = null;
                        $scope.advance.FiscalYearName = null;
                        $scope.advance.FiscalYearPeriodId = null;
                        $scope.advance.FiscalYearPeriodName = null;
                    }
                    else if (result.IsExchangeRateConfirmed === false) {
                        ShowResult(commonMessage.FiscalPeriodExchangeRateConfirmed, 'failure');
                        $scope.advance.PostingDate = '';
                        $scope.advance.FiscalYearId = null;
                        $scope.advance.FiscalYearName = null;
                        $scope.advance.FiscalYearPeriodId = null;
                        $scope.advance.FiscalYearPeriodName = null;
                    }
                    else {
                        $scope.advance.FiscalYearId = result.FiscalYearId;
                        $scope.advance.FiscalYearName = result.FiscalYearName;
                        $scope.advance.FiscalYearPeriodId = result.FiscalYearPeriodId;
                        $scope.advance.FiscalYearPeriodName = result.PeriodName;
                    }
                }
            },
            function errorCallback(response) {
            });
    };
    // For fist time calling of Posting date changes.
    $scope.getPostingFiscalYearPeriod($scope.advance.PostingDate);

    $scope.dateMessage = '';
    $scope.checkDate = function () {
        if (new Date($scope.advance.DocDate) > new Date()) {
            $scope.dateMessage = 'Doc date must be below or equal to current Date ';
            return false;
        }
        else if ($scope.advance.DocDate > $scope.advance.VoucherDate) {
            $scope.dateMessage = 'Doc date must be below or equal to Voucher Date ';
            return false;
        } else {
            $scope.dateMessage = '';
            return true;
        }
    }

    $scope.checkVDockDate = function () {
        if (new Date($scope.voucherDetail.DocDate) > new Date()) {
            $scope.pop('error', 'Doc date must be below or equal to current Date ');
            return false;
        }
        else if ($scope.advance.DocDate > $scope.advance.VoucherDate) {
            $scope.pop('error', 'Doc date must be below or equal to Voucher Date');
            return false;
        } else {
            return true;
        }
    }

    $scope.VoucherDateMessage = '';
    $scope.checkVoucherDate = function () {
        if (new Date($scope.advance.VoucherDate) > new Date()) {
            $scope.VoucherDateMessage = 'Voucher date must be below or equal to current Date ';
            return false
        }
        else if (new Date($scope.advance.VoucherDate) < new Date()) {
            $scope.VoucherDateMessage = '';
            return true
        }
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.customerAdvanceForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/SecurityDeposit/InsertCustomer',
                    data: {
                        'advanceVM': $scope.advance,
                        'voucherDetailList': $scope.voucherDetailList,
                        'currencyList': $scope.voucherDetailCurrencyList,
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: 'Menus/menuframe/edit',
                    data: $scope.menuFrame,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.menuFrames[$scope.index] = $scope.menuFrame;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.advance = {};
        $scope.advance.Active = true;
        $scope.advance.VoucherTypeId = '2';
        $scope.advance.PartyType = 'Customer';
        $scope.advance.Type = 'Receivable';
        $scope.advance.VoucherDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.advance.PostingDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.advance.DocDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucherDetailCurrencyrow = [];
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    function reDirectToRequiredTab() {
        if ($scope.invoiceCustomerForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.invoiceCustomerForm2.$invalid) {
            $scope.setTab(2);
        } else if ($scope.invoiceCustomerForm3.$invalid) {
            $scope.setTab(3);
        } else if ($scope.invoiceCustomerForm4.$invalid) {
            $scope.setTab(4);
        }
    }
    // #endregion

    function sumByKey(data, key) {
        if (typeof (data) === 'undefined' || typeof (key) === 'undefined') {
            return 0;
        }
        var sum = 0;
        for (var i = data.length - 1; i >= 0; i--) {
            if (data[i][key] != null) {
                sum += parseFloat(data[i][key]);
            }
        }
        return sum.toFixed(4);
    }

    $scope.setDrExchangeRate = function (glId, gl, amount) {
        if (undefined !== glId && null != glId) {
            var getRow = $filter("filter")($scope.voucherDetailCurrencyList, { 'Type': 'Dr' });
            var invoice = $filter("filter")($scope.voucherDetailList, { 'Type': 'Dr' });
            invoice = invoice[0];
            if (invoice.Advilable >= amount && amount > -1) {
                var companyCurrencyExchangeRate = $filter("filter")(invoice.CurrencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
                var companyGroupCurrencyExchangeRate = $filter("filter")(invoice.CurrencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
                var hardCurrencyExchangeRate = $filter("filter")(invoice.CurrencyExchangeRate, { ParallelCurrencyType: 'HardCurrency' });
                var companyCurrencyRate = 0;
                var groupCurrencyRate = 0;
                var hardCurrencyRate = 0;

                if (companyCurrencyExchangeRate[0].ParallelCurrencyId == $scope.advance.CurrencyId) {
                    companyCurrencyRate = 1;
                    groupCurrencyRate = companyCurrencyRate / companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    hardCurrencyRate = companyCurrencyRate / hardCurrencyExchangeRate[0].ToCurrencyRate;
                }
                else if (companyGroupCurrencyExchangeRate[0].ParallelCurrencyId == $scope.advance.CurrencyId) {
                    groupCurrencyRate = 1;
                    companyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;
                    hardCurrencyRate = companyCurrencyRate / hardCurrencyExchangeRate[0].ToCurrencyRate;
                }
                else if (hardCurrencyExchangeRate[0].ParallelCurrencyId == $scope.advance.CurrencyId) {
                    hardCurrencyRate = 1;
                    companyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;
                    groupCurrencyRate = companyCurrencyRate / companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                }
                else {
                    companyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;
                    groupCurrencyRate = companyCurrencyRate / companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    hardCurrencyRate = companyCurrencyRate / hardCurrencyExchangeRate[0].ToCurrencyRate;
                }

                if (undefined !== getRow && getRow.length > 0) {
                    getRow[0].Type = 'Dr';
                    getRow[0].Exchange = 'No';
                    getRow[0].GLGeneralInfoId = glId;
                    getRow[0].GL = gl;
                    getRow[0].CompanyCurrencyCr = null;
                    getRow[0].CompanyCurrencyDr = (amount * companyCurrencyRate).toFixed(4);
                    getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                    getRow[0].CompanyFromCurrencyId = companyCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].CompanyToCurrencyId = companyCurrencyExchangeRate[0].ToCurrencyId;
                    getRow[0].CompanyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;

                    getRow[0].CompanyGroupCurrencyCr = null;
                    getRow[0].CompanyGroupCurrencyDr = (amount * groupCurrencyRate).toFixed(4);
                    getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].CompanyGroupToCurrencyId = companyGroupCurrencyExchangeRate[0].ToCurrencyId;
                    getRow[0].CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;

                    getRow[0].HardCurrencyCr = null;
                    getRow[0].HardCurrencyDr = (amount * hardCurrencyRate).toFixed(4);
                    getRow[0].HardCurrencyId = $scope.hardCurrencyId;
                    getRow[0].HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].HardToCurrencyId = hardCurrencyExchangeRate[0].ToCurrencyId;
                    getRow[0].HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                }
                else {
                    $scope.voucherDetailCurrencyList.push({
                        Type: 'Dr',
                        Exchange: 'No',
                        GLGeneralInfoId: glId,
                        GL: gl,
                        CompanyCurrencyCr: null,
                        CompanyCurrencyDr: (amount * companyCurrencyRate).toFixed(4),
                        CompanyCurrencyId: $scope.companyCurrencyId,
                        CompanyFromCurrencyId: companyCurrencyExchangeRate[0].FromCurrencyId,
                        CompanyToCurrencyId: companyCurrencyExchangeRate[0].ToCurrencyId,
                        CompanyCurrencyRate: companyCurrencyExchangeRate[0].ToCurrencyRate,

                        CompanyGroupCurrencyCr: null,
                        CompanyGroupCurrencyDr: (amount * groupCurrencyRate).toFixed(4),
                        CompanyGroupCurrencyId: $scope.companyGroupCurrencyId,
                        CompanyGroupFromCurrencyId: companyGroupCurrencyExchangeRate[0].FromCurrencyId,
                        CompanyGroupToCurrencyId: companyGroupCurrencyExchangeRate[0].ToCurrencyId,
                        CompanyGroupCurrencyRate: companyGroupCurrencyExchangeRate[0].ToCurrencyRate,

                        HardCurrencyCr: null,
                        HardCurrencyDr: (amount * hardCurrencyRate).toFixed(4),
                        HardCurrencyId: $scope.hardCurrencyId,
                        HardFromCurrencyId: hardCurrencyExchangeRate[0].FromCurrencyId,
                        HardToCurrencyId: hardCurrencyExchangeRate[0].ToCurrencyId,
                        HardCurrencyRate: hardCurrencyExchangeRate[0].ToCurrencyRate
                    });
                }
            }
            else {
                ShowResult('Dr amount will be less than or equal Advilable balance.', 'failure');
                invoice.DrAmount = invoice.Advilable;
            }
        }
    }

    $scope.setCrExchangeRate = function (advanceId, amount) {
        if (undefined !== advanceId && null != advanceId) {
            var getRow = $filter("filter")($scope.voucherDetailCurrencyList, { 'AdvanceId': advanceId, 'Exchange': 'No' });
            var advance = $filter("filter")($scope.voucherDetailList, { 'AdvanceId': advanceId });
            if (advance[0].Advilable >= amount && amount > -1) {
                var companyCurrencyExchangeRate = $filter("filter")(advance[0].CurrencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
                var companyGroupCurrencyExchangeRate = $filter("filter")(advance[0].CurrencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
                var hardCurrencyExchangeRate = $filter("filter")(advance[0].CurrencyExchangeRate, { ParallelCurrencyType: 'HardCurrency' });
                var companyCurrencyRate = 0;
                var groupCurrencyRate = 0;
                var hardCurrencyRate = 0;

                if (companyCurrencyExchangeRate[0].ParallelCurrencyId == $scope.advance.CurrencyId) {
                    companyCurrencyRate = 1;
                    groupCurrencyRate = companyCurrencyRate / companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    hardCurrencyRate = companyCurrencyRate / hardCurrencyExchangeRate[0].ToCurrencyRate;
                }
                else if (companyGroupCurrencyExchangeRate[0].ParallelCurrencyId == $scope.advance.CurrencyId) {
                    groupCurrencyRate = 1;
                    companyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;
                    hardCurrencyRate = companyCurrencyRate / hardCurrencyExchangeRate[0].ToCurrencyRate;
                }
                else if (hardCurrencyExchangeRate[0].ParallelCurrencyId == $scope.advance.CurrencyId) {
                    hardCurrencyRate = 1;
                    companyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;
                    groupCurrencyRate = companyCurrencyRate / companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                }
                else {
                    companyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;
                    groupCurrencyRate = companyCurrencyRate / companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    hardCurrencyRate = companyCurrencyRate / hardCurrencyExchangeRate[0].ToCurrencyRate;
                }

                // Exchange Gain Loss Checking
                var invoice = $filter("filter")($scope.voucherDetailList, { 'Type': 'Dr' });
                invoice = invoice[0];
                var companyCurrencyDrExchangeRate = $filter("filter")(invoice.CurrencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
                var companyGroupCurrencyDrExchangeRate = $filter("filter")(invoice.CurrencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
                var hardCurrencyDrExchangeRate = $filter("filter")(invoice.CurrencyExchangeRate, { ParallelCurrencyType: 'HardCurrency' });

                if (undefined !== getRow && getRow.length > 0) {
                    getRow[0].AdvanceId = advanceId;
                    getRow[0].Type = 'Cr';
                    getRow[0].Exchange = 'No';
                    getRow[0].GLGeneralInfoId = advance[0].GLGeneralInfoId;
                    getRow[0].GL = advance[0].GL;

                    getRow[0].CompanyCurrencyCr = (amount * companyCurrencyRate).toFixed(4);
                    getRow[0].CompanyCurrencyDr = null;
                    getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                    getRow[0].CompanyFromCurrencyId = companyCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].CompanyToCurrencyId = companyCurrencyExchangeRate[0].ToCurrencyId;
                    getRow[0].CompanyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;

                    getRow[0].CompanyGroupCurrencyCr = (amount * groupCurrencyRate).toFixed(4);
                    getRow[0].CompanyGroupCurrencyDr = null;
                    getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].CompanyGroupToCurrencyId = companyGroupCurrencyExchangeRate[0].ToCurrencyId;
                    getRow[0].CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;

                    getRow[0].HardCurrencyCr = (amount * hardCurrencyRate).toFixed(4);
                    getRow[0].HardCurrencyDr = null;
                    getRow[0].HardCurrencyId = $scope.hardCurrencyId;
                    getRow[0].HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                    getRow[0].HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].HardToCurrencyId = hardCurrencyExchangeRate[0].ToCurrencyId;

                    // Group currecny gain/loss
                    if (companyGroupCurrencyDrExchangeRate[0].ToCurrencyRate > companyGroupCurrencyExchangeRate[0].ToCurrencyRate) {
                        var rowGroup = $filter("filter")($scope.voucherDetailCurrencyList, { 'AdvanceId': advanceId, 'Exchange': 'Group' });
                        var loss = $filter("filter")($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });

                        rowGroup[0].AdvanceId = advanceId;
                        rowGroup[0].Type = 'Dr';
                        rowGroup[0].Exchange = 'Group';
                        rowGroup[0].GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                        rowGroup[0].GL = loss[0].CompanyGroupCurrencyGL;
                        rowGroup[0].CompanyGroupCurrencyDr = (amount * (1 / companyCurrencyDrExchangeRate[0].ToCurrencyRate - 1 / companyGroupCurrencyExchangeRate[0].ToCurrencyRate)).toFixed(4);
                        rowGroup[0].CompanyGroupCurrencyCr = null
                        rowGroup[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                        rowGroup[0].CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                        rowGroup[0].CompanyGroupToCurrencyId = companyGroupCurrencyExchangeRate[0].ToCurrencyId;
                        rowGroup[0].CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    }
                    else if (companyGroupCurrencyDrExchangeRate[0].ToCurrencyRate < companyGroupCurrencyExchangeRate[0].ToCurrencyRate) {
                        var rowGroup = $filter("filter")($scope.voucherDetailCurrencyList, { 'AdvanceId': advanceId, 'Exchange': 'Group' });
                        var gain = $filter("filter")($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                        rowGroup[0].AdvanceId = advanceId;
                        rowGroup[0].Type = 'Cr';
                        rowGroup[0].Exchange = 'Group';
                        rowGroup[0].GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                        rowGroup[0].GL = gain[0].CompanyGroupCurrencyGL;
                        rowGroup[0].CompanyGroupCurrencyDr = null;
                        rowGroup[0].CompanyGroupCurrencyCr = Math.abs(amount * (1 / companyGroupCurrencyExchangeRate[0].ToCurrencyRate - 1 / companyGroupCurrencyDrExchangeRate[0].ToCurrencyRate)).toFixed(4);
                        rowGroup[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                        rowGroup[0].CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                        rowGroup[0].CompanyGroupToCurrencyId = companyGroupCurrencyExchangeRate[0].ToCurrencyId;
                        rowGroup[0].CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    }
                    // Hard Currency gain/loss
                    if (hardCurrencyDrExchangeRate[0].ToCurrencyRate > hardCurrencyExchangeRate[0].ToCurrencyRate) {
                        var rowHard = $filter("filter")($scope.voucherDetailCurrencyList, { 'AdvanceId': advanceId, 'Exchange': 'Hard' });
                        var loss = $filter("filter")($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });

                        rowHard[0].AdvanceId = advanceId;
                        rowHard[0].Type = 'Dr';
                        rowHard[0].Exchange = 'Hard';
                        rowHard[0].GLGeneralInfoId = loss[0].HardCurrencyGLId;
                        rowHard[0].GL = loss[0].HardCurrencyGL;
                        rowHard[0].HardCurrencyDr = Math.abs(amount * (1 / hardCurrencyDrExchangeRate[0].ToCurrencyRate - 1 / hardCurrencyExchangeRate[0].ToCurrencyRate)).toFixed(4);
                        rowHard[0].HardCurrencyCr = null;
                        rowHard[0].HardCurrencyId = $scope.hardCurrencyId;
                        rowHard[0].HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                        rowHard[0].HardToCurrencyId = hardCurrencyExchangeRate[0].ToCurrencyId;
                        rowHard[0].HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                    }
                    else if (hardCurrencyDrExchangeRate[0].ToCurrencyRate < hardCurrencyExchangeRate[0].ToCurrencyRate) {
                        var rowHard = $filter("filter")($scope.voucherDetailCurrencyList, { 'AdvanceId': advanceId, 'Exchange': 'Group' });
                        var gain = $filter("filter")($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                        rowHard[0].AdvanceId = advanceId;
                        rowHard[0].Type = 'Cr';
                        rowHard[0].Exchange = 'Hard';
                        rowHard[0].GLGeneralInfoId = loss[0].HardCurrencyGLId;
                        rowHard[0].GL = loss[0].HardCurrencyGL;
                        rowHard[0].HardCurrencyDr = null;
                        rowHard[0].HardCurrencyCr = Math.abs(amount * (1 / hardCurrencyExchangeRate[0].ToCurrencyRate - 1 / hardCurrencyDrExchangeRate[0].ToCurrencyRate)).toFixed(4);
                        rowHard[0].HardCurrencyId = $scope.hardCurrencyId;
                        rowHard[0].HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                        rowHard[0].HardToCurrencyId = hardCurrencyExchangeRate[0].ToCurrencyId;
                        rowHard[0].HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                    }
                }
                else {
                    $scope.voucherDetailCurrencyList.push({
                        AdvanceId: advanceId,
                        Type: 'Cr',
                        Exchange: 'No',
                        GLGeneralInfoId: advance[0].GLGeneralInfoId,
                        GL: advance[0].GL,

                        CompanyCurrencyCr: (amount * companyCurrencyRate).toFixed(4),
                        CompanyCurrencyDr: null,
                        CompanyCurrencyId: $scope.companyCurrencyId,
                        CompanyFromCurrencyId: companyCurrencyExchangeRate[0].FromCurrencyId,
                        CompanyToCurrencyId: companyCurrencyExchangeRate[0].ToCurrencyId,
                        CompanyCurrencyRate: companyCurrencyExchangeRate[0].ToCurrencyRate,

                        CompanyGroupCurrencyCr: (amount * groupCurrencyRate).toFixed(4),
                        CompanyGroupCurrencyDr: null,
                        CompanyGroupCurrencyId: $scope.companyGroupCurrencyId,
                        CompanyGroupFromCurrencyId: companyGroupCurrencyExchangeRate[0].FromCurrencyId,
                        CompanyGroupToCurrencyId: companyGroupCurrencyExchangeRate[0].ToCurrencyId,
                        CompanyGroupCurrencyRate: companyGroupCurrencyExchangeRate[0].ToCurrencyRate,

                        HardCurrencyCr: (amount * hardCurrencyRate).toFixed(4),
                        HardCurrencyDr: null,
                        HardCurrencyId: $scope.hardCurrencyId,
                        HardFromCurrencyId: hardCurrencyExchangeRate[0].FromCurrencyId,
                        HardToCurrencyId: hardCurrencyExchangeRate[0].ToCurrencyId,
                        HardCurrencyRate: hardCurrencyExchangeRate[0].ToCurrencyRate
                    });

                    // Group currecny gain/loss
                    if (companyGroupCurrencyDrExchangeRate[0].ToCurrencyRate > companyGroupCurrencyExchangeRate[0].ToCurrencyRate) {
                        var loss = $filter("filter")($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });
                        $scope.voucherDetailCurrencyList.push({
                            AdvanceId: advanceId,
                            Type: 'Dr',
                            Exchange: 'Group',
                            GLGeneralInfoId: loss[0].CompanyGroupCurrencyGLId,
                            GL: loss[0].CompanyGroupCurrencyGL,

                            CompanyGroupCurrencyDr: Math.abs(amount * (1 / companyCurrencyDrExchangeRate[0].ToCurrencyRate - 1 / companyGroupCurrencyExchangeRate[0].ToCurrencyRate)).toFixed(4),
                            CompanyGroupCurrencyCr: null,
                            CompanyGroupCurrencyId: $scope.companyGroupCurrencyId,
                            CompanyGroupFromCurrencyId: companyGroupCurrencyExchangeRate[0].FromCurrencyId,
                            CompanyGroupToCurrencyId: companyGroupCurrencyExchangeRate[0].ToCurrencyId,
                            CompanyGroupCurrencyRate: companyGroupCurrencyExchangeRate[0].ToCurrencyRate
                        });
                    }
                    else if (companyGroupCurrencyDrExchangeRate[0].ToCurrencyRate < companyGroupCurrencyExchangeRate[0].ToCurrencyRate) {
                        var gain = $filter("filter")($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                        $scope.voucherDetailCurrencyList.push({
                            AdvanceId: advanceId,
                            Type: 'Cr',
                            Exchange: 'Group',
                            GLGeneralInfoId: gain[0].CompanyGroupCurrencyGLId,
                            GL: gain[0].CompanyGroupCurrencyGL,

                            CompanyGroupCurrencyDr: null,
                            CompanyGroupCurrencyCr: Math.abs(amount * (1 / companyGroupCurrencyExchangeRate[0].ToCurrencyRate - 1 / companyGroupCurrencyDrExchangeRate[0].ToCurrencyRate)).toFixed(4),
                            CompanyGroupCurrencyId: $scope.companyGroupCurrencyId,
                            CompanyGroupFromCurrencyId: companyGroupCurrencyExchangeRate[0].FromCurrencyId,
                            CompanyGroupToCurrencyId: companyGroupCurrencyExchangeRate[0].ToCurrencyId,
                            CompanyGroupCurrencyRate: companyGroupCurrencyExchangeRate[0].ToCurrencyRate
                        });
                    }

                    // Hard Currency gain/loss
                    if (hardCurrencyDrExchangeRate[0].ToCurrencyRate > hardCurrencyExchangeRate[0].ToCurrencyRate) {
                        var loss = $filter("filter")($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });
                        $scope.voucherDetailCurrencyList.push({
                            AdvanceId: advanceId,
                            Type: 'Dr',
                            Exchange: 'Hard',
                            GLGeneralInfoId: loss[0].HardCurrencyGLId,
                            GL: loss[0].HardCurrencyGL,

                            HardCurrencyDr: Math.abs(amount * (1 / hardCurrencyDrExchangeRate[0].ToCurrencyRate - 1 / hardCurrencyExchangeRate[0].ToCurrencyRate)).toFixed(4),
                            HardCurrencyCr: null,
                            HardCurrencyId: $scope.hardCurrencyId,
                            HardFromCurrencyId: hardCurrencyExchangeRate[0].FromCurrencyId,
                            HardToCurrencyId: hardCurrencyExchangeRate[0].ToCurrencyId,
                            HardCurrencyRate: hardCurrencyExchangeRate[0].ToCurrencyRate
                        });
                    }
                    else if (hardCurrencyDrExchangeRate[0].ToCurrencyRate < hardCurrencyExchangeRate[0].ToCurrencyRate) {
                        var gain = $filter("filter")($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                        $scope.voucherDetailCurrencyList.push({
                            AdvanceId: advanceId,
                            Type: 'Cr',
                            Exchange: 'Hard',
                            GLGeneralInfoId: loss[0].HardCurrencyGLId,
                            GL: loss[0].HardCurrencyGL,

                            HardCurrencyDr: null,
                            HardCurrencyCr: Math.abs(amount * (1 / hardCurrencyExchangeRate[0].ToCurrencyRate - 1 / hardCurrencyDrExchangeRate[0].ToCurrencyRate)).toFixed(4),
                            HardCurrencyId: $scope.hardCurrencyId,
                            HardFromCurrencyId: hardCurrencyExchangeRate[0].FromCurrencyId,
                            HardToCurrencyId: hardCurrencyExchangeRate[0].ToCurrencyId,
                            HardCurrencyRate: hardCurrencyExchangeRate[0].ToCurrencyRate
                        });
                    }
                }
            }
            else {
                ShowResult('Cr amount will be less than or equal Advilable balance.', 'failure');
                advance.CrAmount = advance.Advilable;
            }
        }
    }

    //**************************************** Customer List Start ******************************************************
    $scope.customerList = [];
    $scope.customerIndex = -1;
    $scope.searchCustomerByList = [
        {
            'name': 'Customer Code',
            'value': 'Code'
        },
        {
            'name': 'Customer Name',
            'value': 'UserName'
        },
        {
            'name': 'GL Code',
            'value': 'DownPaymentGLCode'
        },
        {
            'name': 'GL Name',
            'value': 'DownPaymentGLName'
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
        $scope.customerGLUrl = 'Parties/party/GetCompanyPartyDataList?partyType=Customer';
        $scope.getCustomerData = function (pageno) {
            baseService.paginationBase($scope.customerGLUrl, pageno, $scope.customerParameters)
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
    }

    $scope.closeCustomerPopUp = function () {
        if ($scope.customerIndex !== -1) {
            var customer = $scope.customerList[$scope.customerIndex];
            $scope.partyName = customer.Code + " - " + customer.UserName;
            $scope.advance.PartyName = customer.Code + " - " + customer.UserName;
            $scope.advance.PartyId = customer.Id;
            $scope.advance.PartyGLGeneralInfoId = customer.DownPaymentGLId;
            $scope.advance.PartyGL = customer.DownPaymentGLCode + " - " + customer.DownPaymentGLName;
        }
        angular.element(document.querySelector('#customerListPopUp')).modal('hide');
        $scope.customerIndex = -1
    };
    //**************************************** Customer List End ***************************

    //*********************** Customer Invoice PopUp Start *************************************
    $scope.customerInvoiceSearchList = [];
    $scope.customerInvoiceDataList = [];
    $scope.customerInvoiceSearch = [];
    $scope.customerInvoiceUrl = 'accounts/voucher/getcustomerinvoiceparty';
    $scope.customerInvoiceSelectedIndex = -1;
    $scope.customerInvoiceParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'VoucherNo',
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.customerInvoicePopUp = function (partyId) {
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.customerInvoiceDataList = [];
            ShowResult('Please select Customer.', 'failure');
        }
        else {
            $scope.compareCurrencyId = $scope.advance.CurrencyId;
            $scope.customerInvoiceParameters.partyId = partyId;
            $scope.getCustomerInvoiceData = function (pageno) {
                baseService.paginationBase($scope.customerInvoiceUrl, pageno, $scope.customerInvoiceParameters)
                    .then(function (response) {
                        $scope.customerInvoiceDataList = response.Rows;
                        $scope.customerInvoiceParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.customerInvoiceSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.customerInvoiceDataList, $scope.customerInvoiceSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#customerInvoicePopUp')).modal('show');
            $scope.getCustomerInvoiceData();
        }
    };

    $scope.closeCustomerInvoicePopUp = function () {
        if ($scope.customerInvoiceSelectedIndex !== -1) {
            var customer = $scope.customerInvoiceDataList[$scope.customerInvoiceSelectedIndex];
            if (customer.CurrencyId === $scope.advance.CurrencyId) {
                var getRow = $filter("filter")($scope.voucherDetailList, { 'Type': 'Dr' });
                if (undefined !== getRow && getRow.length > 0) {
                    ShowResult('This row is already exist.', 'failure');
                }
                else {
                    $http({
                        method: 'GET',
                        url: 'accounts/Voucher/GetCustomerVoucherDetailIdByvoucherId?voucherId=' + customer.VoucherId + '&customerInvoiceDetailId=' + customer.CustomerInvoiceDetailId,
                    }).then(function successCallback(response) {
                        if (response.data.Total > 0) {
                            $scope.voucherDetailList.push({
                                Type: 'Dr',
                                CustomerInvoiceDetailId: customer.CustomerInvoiceDetailId,
                                GLGeneralInfoId: customer.GLGeneralInfoId,
                                VoucherNo: customer.VoucherNo,
                                GL: customer.COAIText,
                                DocDate: customer.DocDate,
                                DocRefNo: customer.DocRefNo,
                                Narration: customer.Narration,
                                DrAmount: customer.Balance,
                                CrAmount: 0,
                                Amount: customer.Receivable,
                                WriteOff: customer.Received,
                                Advilable: customer.Balance,
                                CurrencyExchangeRate: response.data.Rows
                            });
                            $scope.setDrExchangeRate(customer.GLGeneralInfoId, customer.COAIText, customer.Balance);
                        }
                        else {
                            ShowResult('This invoice currency rate not found!', 'failure');
                        }
                    });
                }
            }
            else {
                ShowResult('Invoice currency does not match with transaction currency.', 'failure');
            }
        }
        angular.element(document.querySelector('#customerInvoicePopUp')).modal('hide');
        $scope.customerInvoiceSelectedIndex = -1;
    };

    $scope.selectCustomerInvoicePopUp = function (index, id) {
        $scope.customerInvoiceSelectedIndex = index;
        $scope.selectedInvoiceId = id;
    };

    //*********************** Customer Invoice PopUp End ***************************************

    //*********************** Customer Advance PopUp Start *************************************
    $scope.customerAdvanceSearchList = [];
    $scope.customerAdvanceDataList = [];
    $scope.customerAdvanceSearch = [];
    $scope.customerAdvanceUrl = 'accounts/Advance/GetAvilabeAdvanceList';
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

    $scope.customerAdvancePopUp = function (partyId) {
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.customerAdvanceDataList = [];
            ShowResult('Please select Customer.', 'failure');
        }
        else {
            $scope.compareCurrencyId = $scope.advance.CurrencyId;
            $scope.customerAdvanceParameters.partyId = partyId;
            $scope.getCustomerAdvanceData = function (pageno) {
                baseService.paginationBase($scope.customerAdvanceUrl, pageno, $scope.customerAdvanceParameters)
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
        if ($scope.customerAdvanceSelectedIndex !== -1) {
            var advance = $scope.customerAdvanceDataList[$scope.customerAdvanceSelectedIndex];
            if (advance.CurrencyId === $scope.advance.CurrencyId) {
                var getRow = $filter("filter")($scope.voucherDetailList, { 'AdvanceId': advance.Id });
                if (undefined !== getRow && getRow.length > 0) {
                    ShowResult('This row is already exist.', 'failure');
                }
                else {
                    $http({
                        method: 'GET',
                        url: 'accounts/Advance/ParallelExchangeRate?voucherId=' + advance.VoucherId + '&voucherDetailId=' + advance.VoucherDetailId,
                    }).then(function successCallback(response) {
                        if (response.data != null) {
                            $scope.voucherDetailList.push({
                                AdvanceId: advance.Id,
                                VoucherNo: advance.VoucherNo,
                                Type: 'Cr',
                                GLGeneralInfoId: advance.GLGeneralInfoId,
                                GL: advance.GL,
                                DocDate: advance.DocDate,
                                DocRefNo: advance.DocRefNo,
                                Narration: advance.Narration,
                                DrAmount: 0,
                                CrAmount: advance.Balance,
                                Amount: advance.Advanced,
                                WriteOff: advance.WriteOff,
                                Advilable: advance.Balance,
                                CurrencyExchangeRate: response.data
                            });
                            $scope.setCrExchangeRate(advance.Id, advance.Balance);
                        }
                        else {
                            ShowResult('This advance currency rate not found!', 'failure');
                        }
                    });
                }
            }
            else {
                ShowResult('Advance currency does not match with transaction currency.', 'failure');
            }
        }
        angular.element(document.querySelector('#customerAdvancePopUp')).modal('hide');
    };

    $scope.selectCustomerAdvancePopUp = function (index, id) {
        $scope.customerAdvanceSelectedIndex = index;
        $scope.selectedAdvanceId = id;
    };

    $scope.removeRow = function (index) {
        var advanceId = $scope.voucherDetailList[index].AdvanceId;
        $scope.voucherDetailList.splice(index, 1);
        var i = $scope.voucherDetailCurrencyList.length;
        while (i--) {
            if ($scope.voucherDetailCurrencyList[i]['AdvanceId'] === advanceId) {
                $scope.voucherDetailCurrencyList.splice(i, 1);
            }
        }
    }

    function findWithAttr(array, attr, value) {
        for (var i = 0; i < array.length; i += 1) {
            if (array[i][attr] === value) {
                return i;
            }
        }
        return -1;
    }
    //*********************** Customer Advance PopUp End *************************************
}