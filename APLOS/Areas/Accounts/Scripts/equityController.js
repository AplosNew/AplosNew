'use strict';
equityController.$inject = ['accountService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function equityController(accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Equity Taken';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.voucherDetailCurrency = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.isBankAmount = false;
    $scope.partyType = 'Party';
    $scope.partyFromTo = 'From';
    $scope.glFromTo = 'To';
    baseService.init('accounts/Advance/GetEquityList', null, null, 'DESC', 'PostingDate', 'PostingDate');
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

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.tranCurrencyList = result;
    });

    cboService.getCboVoucherType(function (result) {
        $scope.voucherTypeList = result;
    });

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByCompanyWise(null, null, function (result) {
                $scope.entityList = result;
            });
    });

    $scope.getCboBudgetByPartyGLId = function (glId) {
        accountService.getBudgetMasterCboList(glId, function (result) {
            $scope.partyBudgetList = result;
        });
    };

    $scope.getCboBudgetByBankGLId = function (glId) {
        accountService.getBudgetMasterCboList(glId, function (result) {
            $scope.bankBudgetList = result;
        });
    };

    $scope.voucher = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PartyId: null,
        PartyName: null,
        PartyGLGeneralInfoId: null,
        GLGeneralInfoId: null,
        CurrencyId: null,
        VoucherTypeId: null,
        PartyType: null,
        Type: 'SD',
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: $filter("dateFiltering")(Date.now()),
        DocDate: $filter("dateFiltering")(Date.now()),
        DocRefNo: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        IsExcludingTax: false,
        VoucherDetailId: null,
        Amount: 0,
        BankAmount: 0,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        PaymentTermId: null,
        Narration: null,
        Remarks: null,
        BankMasterId: null,
        BankName: null,
        BankAccountNumber: null,
        BankGL: null,
        BankGLGeneralInfoId: null,
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
        Type: null
    };

    // Creating parallel currency table heading.
    $scope.parallelCurrencyTableHead = '<tr><th style="width:250px; vertical-align: middle; text-align:center" rowspan="2">GL</th>';
    var debitCreditHead = '</tr><tr>';
    $scope.parallelCurrencyTypeList = [];
    $scope.companyCurrencyId = null;
    $scope.companyGroupCurrencyId = null;
    $scope.hardCurrencyId = null;
    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CurrencyParallel'
    }).then(function successCallback(response) {
        angular.forEach(response.data, function (item, i) {
            $scope.parallelCurrencyTableHead += '<th style="text-align:center" colspan="2">' + item.Code + '</th>';
            debitCreditHead += '<th>Dr</th><th>Cr</th>';
            if (item.ParallelCurrencyType === 'CompanyCurrency') {
                $scope.companyCurrencyId = item.CurrencyId;
                $scope.companyCurrencyName = item.Code;
                $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyCurrency', CurrencyType: 'CompanyCurrencyDr', CurrencyId: item.CurrencyId });
                $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyCurrency', CurrencyType: 'CompanyCurrencyCr', CurrencyId: item.CurrencyId });
            }
            else if (item.ParallelCurrencyType === 'CompanyGroupCurrency') {
                $scope.companyGroupCurrencyId = item.CurrencyId;
                $scope.companyGroupCurrencyName = item.Code;
                $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyGroupCurrency', CurrencyType: 'CompanyGroupCurrencyDr', CurrencyId: item.CurrencyId });
                $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyGroupCurrency', CurrencyType: 'CompanyGroupCurrencyCr', CurrencyId: item.CurrencyId });
            }
            else if (item.ParallelCurrencyType === 'HardCurrency') {
                $scope.hardCurrencyId = item.CurrencyId;
                $scope.hardCurrencyName = item.Code;
                $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'HardCurrency', CurrencyType: 'HardCurrencyDr', CurrencyId: item.CurrencyId });
                $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'HardCurrency', CurrencyType: 'HardCurrencyCr', CurrencyId: item.CurrencyId });
            }
        });
        $scope.parallelCurrencyTableHead += debitCreditHead + '</tr>';
    });

    $scope.getPostingFiscalYearPeriod = function (date) {
        if (date !== undefined) {
            $http({
                method: 'get',
                url: 'accounts/CompanyFiscalYear/CheckingFiscalYearPeriod?postingDate=' + $filter("dateFiltering")(date)
            }).then(
                function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.postingDateMessage = response.data.Message;
                    }
                    else {
                        $scope.postingDateMessage = null;
                        var result = response.data;
                        if (result.IsTransationLocked === true) {
                            ShowResult(commonMessage.FiscalPeriodTransactionLocked, 'failure');
                            $scope.voucher.PostingDate = '';
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                        }
                        else if (result.IsExchangeRateConfirmed === false) {
                            ShowResult(commonMessage.FiscalPeriodExchangeRateConfirmed, 'failure');
                            $scope.voucher.PostingDate = '';
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                        }
                        else {
                            $scope.voucher.FiscalYearId = result.FiscalYearId;
                            $scope.voucher.FiscalYearName = result.FiscalYearName;
                            $scope.voucher.FiscalYearPeriodId = result.FiscalYearPeriodId;
                            $scope.voucher.FiscalYearPeriodName = result.PeriodName;
                            $scope.voucher.VoucherDate = $filter("dateFiltering")(Date.now());
                            $scope.voucher.PostingDate = $filter("dateFiltering")(Date.now());
                            $scope.voucher.DocDate = $filter("dateFiltering")(Date.now());
                            $scope.GetCurrencyExchangeRateList();
                        }
                    }
                },
                function errorCallback(response) {
                });
        }
    };

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
            url: 'accounts/Advance/Get/' + id
        }).then(function successCallback(response) {
            $scope.voucher = response.data;
            $scope.voucher.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
            $scope.voucher.VoucherDate = $filter("dateFiltering")($scope.voucher.VoucherDate);
            $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucher.PostingDate);
            $scope.GetCurrencyExchangeRateList();
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };

    $scope.GetCurrencyExchangeRateList = function () {
        if ($scope.voucher.CurrencyId !== null && undefined !== $scope.voucher.CurrencyId) {
            $http({
                method: 'GET',
                url: 'currencies/ExchangeRate/ParallelExchangeRate?fromdate=' + $scope.voucher.PostingDate + '&currencyId=' + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.currencyRateConversion($scope.voucher.CurrencyId);
                $scope.setCrExchangeRate($scope.voucher.PartyGLGeneralInfoId);
            });
        }
    };

    $scope.setDrExchangeRate = function (glId) {
        if (undefined !== glId && null !== glId) {
            var companyCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
            var companyGroupCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
            var hardCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: 'HardCurrency' });

            var getRow = $filter("filter")($scope.voucherDetailCurrencyList, { 'Type': 'Dr' });
            if (undefined !== getRow && getRow.length > 0) {
                getRow[0].Type = 'Dr';
                getRow[0].GLGeneralInfoId = glId;
                getRow[0].GL = $scope.voucher.BankGL;
                getRow[0].CompanyCurrencyCr = null;
                getRow[0].CompanyCurrencyDr = ($scope.voucher.Amount * companyCurrencyRate).toFixed(4);
                getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                getRow[0].CompanyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;
                getRow[0].CompanyGroupCurrencyCr = null;
                getRow[0].CompanyGroupCurrencyDr = ($scope.voucher.Amount * groupCurrencyRate).toFixed(4);
                getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                if ($scope.voucher.CurrencyId === $scope.companyGroupCurrencyId) {
                    getRow[0].CompanyGroupCurrencyRate = 1;
                    getRow[0].CompanyCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                }
                else {
                    getRow[0].CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                }
                getRow[0].HardCurrencyCr = null;
                getRow[0].HardCurrencyDr = ($scope.voucher.Amount * hardCurrencyRate).toFixed(4);
                getRow[0].HardCurrencyId = $scope.hardCurrencyId;
                if ($scope.voucher.CurrencyId === $scope.hardCurrencyId) {
                    getRow[0].HardCurrencyRate = 1;
                    getRow[0].CompanyCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                }
                else {
                    getRow[0].HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                }
                getRow[0].HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                getRow[0].CompanyFromCurrencyId = companyCurrencyExchangeRate[0].FromCurrencyId;
                getRow[0].CompanyToCurrencyId = companyCurrencyExchangeRate[0].ToCurrencyId;
                getRow[0].CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                getRow[0].CompanyGroupToCurrencyId = companyGroupCurrencyExchangeRate[0].ToCurrencyId;
                getRow[0].HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                getRow[0].HardToCurrencyId = hardCurrencyExchangeRate[0].ToCurrencyId;
                getRow[0].CompanyCurrencyName = $scope.companyCurrencyName;
                getRow[0].CompanyGroupCurrencyName = $scope.companyCurrencyName;
                getRow[0].HardCurrencyName = $scope.companyCurrencyName;
            }
            else {
                $scope.voucherDetailCurrencyList.push({
                    Type: 'Dr',
                    GLGeneralInfoId: glId,
                    GL: $scope.voucher.BankGL,
                    CompanyCurrencyCr: null,
                    CompanyCurrencyDr: ($scope.voucher.Amount * companyCurrencyRate).toFixed(4),
                    CompanyCurrencyId: $scope.companyCurrencyId,
                    CompanyCurrencyRate: companyCurrencyExchangeRate[0].ToCurrencyRate,
                    CompanyGroupCurrencyCr: null,
                    CompanyGroupCurrencyDr: ($scope.voucher.Amount * groupCurrencyRate).toFixed(4),
                    CompanyGroupCurrencyId: $scope.companyGroupCurrencyId,
                    CompanyGroupCurrencyRate: companyGroupCurrencyExchangeRate[0].ToCurrencyRate,
                    HardCurrencyCr: null,
                    HardCurrencyDr: ($scope.voucher.Amount * hardCurrencyRate).toFixed(4),
                    HardCurrencyId: $scope.hardCurrencyId,
                    HardCurrencyRate: hardCurrencyExchangeRate[0].ToCurrencyRate,
                    CompanyFromCurrencyId: companyCurrencyExchangeRate[0].FromCurrencyId,
                    CompanyToCurrencyId: companyCurrencyExchangeRate[0].ToCurrencyId,
                    CompanyGroupFromCurrencyId: companyGroupCurrencyExchangeRate[0].FromCurrencyId,
                    CompanyGroupToCurrencyId: companyGroupCurrencyExchangeRate[0].ToCurrencyId,
                    HardFromCurrencyId: hardCurrencyExchangeRate[0].FromCurrencyId,
                    HardToCurrencyId: hardCurrencyExchangeRate[0].ToCurrencyId,
                    CompanyCurrencyName: $scope.companyCurrencyName,
                    CompanyGroupCurrencyName: $scope.companyGroupCurrencyName,
                    HardCurrencyName: $scope.hardCurrencyName
                });
            }
        }
    };

    $scope.setCrExchangeRate = function (glId) {
        if (undefined !== glId && null !== glId) {
            var companyCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
            var companyGroupCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
            var hardCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: 'HardCurrency' });

            var getRow = $filter("filter")($scope.voucherDetailCurrencyList, { 'Type': 'Cr' });
            if (undefined !== getRow && getRow.length > 0) {
                getRow[0].Type = 'Cr';
                getRow[0].GLGeneralInfoId = glId;
                getRow[0].GL = $scope.voucher.PartyGL;
                getRow[0].CompanyCurrencyCr = ($scope.voucher.Amount * companyCurrencyRate).toFixed(4);
                getRow[0].CompanyCurrencyDr = null;
                getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                getRow[0].CompanyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;
                getRow[0].CompanyGroupCurrencyCr = ($scope.voucher.Amount * groupCurrencyRate).toFixed(4);
                getRow[0].CompanyGroupCurrencyDr = null;
                getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                if ($scope.voucher.CurrencyId === $scope.companyGroupCurrencyId) {
                    getRow[0].CompanyGroupCurrencyRate = 1;
                    getRow[0].CompanyCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                }
                else {
                    getRow[0].CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                }
                getRow[0].HardCurrencyCr = ($scope.voucher.Amount * hardCurrencyRate).toFixed(4);
                getRow[0].HardCurrencyDr = null;
                getRow[0].HardCurrencyId = $scope.hardCurrencyId;
                if ($scope.voucher.CurrencyId === $scope.hardCurrencyId) {
                    getRow[0].HardCurrencyRate = 1;
                    getRow[0].CompanyCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                }
                else {
                    getRow[0].HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                }
                getRow[0].CompanyFromCurrencyId = companyCurrencyExchangeRate[0].FromCurrencyId;
                getRow[0].CompanyToCurrencyId = companyCurrencyExchangeRate[0].ToCurrencyId;
                getRow[0].CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                getRow[0].CompanyGroupToCurrencyId = companyGroupCurrencyExchangeRate[0].ToCurrencyId;
                getRow[0].HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                getRow[0].HardToCurrencyId = hardCurrencyExchangeRate[0].ToCurrencyId;
                getRow[0].CompanyCurrencyName = $scope.companyCurrencyName;
                getRow[0].CompanyGroupCurrencyName = $scope.companyCurrencyName;
                getRow[0].HardCurrencyName = $scope.companyCurrencyName;
            }
            else {
                $scope.voucherDetailCurrencyList.push({
                    Type: 'Cr',
                    GLGeneralInfoId: glId,
                    GL: $scope.voucher.PartyGL,
                    CompanyCurrencyCr: ($scope.voucher.Amount * companyCurrencyRate).toFixed(4),
                    CompanyCurrencyDr: null,
                    CompanyCurrencyId: $scope.companyCurrencyId,
                    CompanyCurrencyRate: companyCurrencyExchangeRate[0].ToCurrencyRate,
                    CompanyGroupCurrencyCr: ($scope.voucher.Amount * groupCurrencyRate).toFixed(4),
                    CompanyGroupCurrencyDr: null,
                    CompanyGroupCurrencyId: $scope.companyGroupCurrencyId,
                    CompanyGroupCurrencyRate: companyGroupCurrencyExchangeRate[0].ToCurrencyRate,
                    HardCurrencyCr: ($scope.voucher.Amount * hardCurrencyRate).toFixed(4),
                    HardCurrencyDr: null,
                    HardCurrencyId: $scope.hardCurrencyId,
                    HardCurrencyRate: hardCurrencyExchangeRate[0].ToCurrencyRate,
                    CompanyFromCurrencyId: companyCurrencyExchangeRate[0].FromCurrencyId,
                    CompanyToCurrencyId: companyCurrencyExchangeRate[0].ToCurrencyId,
                    CompanyGroupFromCurrencyId: companyGroupCurrencyExchangeRate[0].FromCurrencyId,
                    CompanyGroupToCurrencyId: companyGroupCurrencyExchangeRate[0].ToCurrencyId,
                    HardFromCurrencyId: hardCurrencyExchangeRate[0].FromCurrencyId,
                    HardToCurrencyId: hardCurrencyExchangeRate[0].ToCurrencyId,
                    CompanyCurrencyName: $scope.companyCurrencyName,
                    CompanyGroupCurrencyName: $scope.companyGroupCurrencyName,
                    HardCurrencyName: $scope.hardCurrencyName
                });
            }
        }
    };

    var companyCurrencyRate = 0;
    var groupCurrencyRate = 0;
    var hardCurrencyRate = 0;
    $scope.currencyRateConversion = function (transactionCurrencyId) {
        companyCurrencyRate = 0;
        groupCurrencyRate = 0;
        hardCurrencyRate = 0;
        var companyCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
        var companyGroupCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
        var hardCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: 'HardCurrency' });
        if (companyCurrencyExchangeRate[0].FromCurrencyId === transactionCurrencyId) {
            companyCurrencyRate = 1;
            groupCurrencyRate = companyCurrencyRate / companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
            hardCurrencyRate = companyCurrencyRate / hardCurrencyExchangeRate[0].ToCurrencyRate;
        }
        else if (companyGroupCurrencyExchangeRate[0].FromCurrencyId === transactionCurrencyId) {
            groupCurrencyRate = 1;
            companyCurrencyRate = groupCurrencyRate * companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
            hardCurrencyRate = companyCurrencyRate / hardCurrencyExchangeRate[0].ToCurrencyRate;
        }
        else if (hardCurrencyExchangeRate[0].FromCurrencyId === transactionCurrencyId) {
            hardCurrencyRate = 1;
            companyCurrencyRate = hardCurrencyRate * hardCurrencyExchangeRate[0].ToCurrencyRate;
            groupCurrencyRate = companyCurrencyRate / companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
        }
        else {
            companyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;
            groupCurrencyRate = companyCurrencyRate / companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
            hardCurrencyRate = companyCurrencyRate / hardCurrencyExchangeRate[0].ToCurrencyRate;
        }
        $scope.setCrExchangeRate($scope.voucher.PartyGLGeneralInfoId);
        $scope.setDrExchangeRate($scope.voucher.BankGLGeneralInfoId);
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
        sort: 'UserName, PartyType',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getPartyList = function () {
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
            addRow();
        }
        angular.element(document.querySelector('#customerListPopUp')).modal('hide');
        $scope.customerIndex = -1;
        $scope.selectedCustomer = null;
    };
    //**************************************** Customer List End ***************************

    function addRow() {
        var party = $scope.customerList[$scope.customerIndex];
        $scope.voucher.PartyName = party.Code + " - " + party.UserName;
        $scope.voucher.PartyId = party.Id;
        $scope.voucher.PartyType = party.PartyType;
        $scope.voucher.PartyGLGeneralInfoId = party.ReconciliationGLId;
        $scope.voucher.PartyGL = party.ReconciliationGLCode + " - " + party.ReconciliationGLName;
        $scope.voucher.CurrencyId = party.CurrencyId;
        $scope.getCboBudgetByPartyGLId(party.ReconciliationGLId);
        $scope.GetCurrencyExchangeRateList();
    }

    //**************************************** GL List Start ***************************
    $scope.rowSelected = null;
    $scope.cOAICodeList = [];
    $scope.glIndex = -1;
    $scope.searchglByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL',
            'value': 'GLItem'
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName',
        searchBy: "GLItem",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetCOAICodeList = function () {
        $scope.GLUrl1 = 'accounts/glitem/getgllist';
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#GLPopUp')).modal('show');
        $scope.GetCOAICodeListData();
    };

    $scope.setSelected = function (x, index) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.glIndex = index;
    };

    $scope.closeCOAICodeListPopUp = function () {
        updateRow();
        $scope.setDrExchangeRate($scope.voucher.BankGLGeneralInfoId, $scope.voucher.BankGL);
        angular.element(document.querySelector('#GLPopUp')).modal('hide');
        $scope.glIndex = -1;
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        updateRow();
        $scope.setDrExchangeRate($scope.voucher.BankGLGeneralInfoId, $scope.voucher.BankGL);
        angular.element(document.querySelector('#GLPopUp')).modal('hide');
        $scope.glIndex = -1;
    };

    function updateRow() {
        if ($scope.glIndex !== -1) {
            var coa = $scope.cOAICodeList[$scope.glIndex];
            $scope.voucher.BankGL = coa.GLGeneralInfoCode + ' - ' + coa.GLItem;
            $scope.voucher.BankGLGeneralInfoId = coa.GLGeneralInfoId;
        }
        $scope.glIndex = -1;
    }
    //**************************************** GL List End ***************************

    $scope.postingDateMessage = '';
    $scope.checkPostingDate = function () {
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            $scope.postingDateMessage = 'Posting date must be below or equal to current Date!';
            $scope.voucher.FiscalYearId = null;
            $scope.voucher.FiscalYearName = null;
            $scope.voucher.FiscalYearPeriodId = null;
            $scope.voucher.FiscalYearPeriodName = null;
            return false;
        }
        else if (new Date($scope.voucher.PostingDate) > $scope.voucher.DocDate) {
            $scope.postingDateMessage = 'Posting date must be below or equal to Doc Date!';
            $scope.voucher.FiscalYearId = null;
            $scope.voucher.FiscalYearName = null;
            $scope.voucher.FiscalYearPeriodId = null;
            $scope.voucher.FiscalYearPeriodName = null;
            return false;
        } else {
            $scope.getPostingFiscalYearPeriod($scope.voucher.PostingDate);
            $scope.postingDateMessage = '';
            return true;
        }
    };

    $scope.dateMessage = '';
    $scope.checkDocDate = function () {
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.dateMessage = 'Doc date must be below or equal to current Date!';
            return false;
        }
        else {
            $scope.dateMessage = '';
            return true;
        }
    };

    $scope.Save = function () {
        tabRedirect();
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form1.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/Advance/InsertEquity',
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
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'accounts/Advance/UpdateEquity',
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
                        $scope.getData();
                        ClearFields();
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
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.voucher = {};
        $scope.voucher.Active = true;
        $scope.voucher.VoucherTypeId = '4';
        $scope.voucher.Amount = 0;
        $scope.voucher.VoucherDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucher.PostingDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucher.DocDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailCurrencyList = [];
        $scope.getPostingFiscalYearPeriod($scope.voucher.PostingDate);
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    function tabRedirect() {
        if ($scope.tabForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.tabForm2.$invalid) {
            $scope.setTab(2);
        }
    }
}