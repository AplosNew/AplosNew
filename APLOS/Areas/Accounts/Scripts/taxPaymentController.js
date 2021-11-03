'use strict';
taxPaymentController.$inject = ['addressService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller','bankService'];
function taxPaymentController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller, bankService) {
    $rootScope.title = 'Payment';
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
    $scope.isAdvance = false;
    $scope.isBankAmount = false;
    $controller('cashBaseController', { $scope: $scope, $http: $http });
    $controller('bankBaseController', { $scope: $scope, $http: $http });
    baseService.init('accounts/Invoice/GetVendorPaymentList', null, null, 'DESC', 'DocDate', 'DocDate');
    $scope.changePartyType = function () {
        if ($scope.voucher.PartyType == 'Vendor')
            $scope.partyType = 'Vendor';
        else
            $scope.partyType = 'Customer';
    }
    $scope.showPartyPop = function () {
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
        $scope.showPartyPopUpNew();
    }
    $scope.voucher = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PartyId: null,
        PartyName: null,
        PartyType: 'Vendor',
        CurrencyId: null,
        CountryId: null,
        TaxCategoryId:null,
        PaymentTermId: null,
        SourceType: null,
        VoucherNo: null,
        VoucherDate: $filter('dateFiltering')(Date.now()),
        PostingDate: $filter('dateFiltering')(Date.now()),
        DocDate: $filter('dateFiltering')(Date.now()),
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
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
        GLGeneralInfoId: null,
        GLGeneralInfoName: null,
        BudgetId: null,
        BudgetName: null,
        ActivityId: null,
        ActivityName: null,

        InvoiceAmount: 0,
        ExGainLossAmount: 0,
        NetInvoiceAmount: 0,
        InvoiceGroupAmount: 0,
        ExGainLossGroupAmount: 0
    };
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.voucherDetail = {
        EntityId: null
    };

    $scope.tranCurrencyList = [];
    cboService.getCboParallelCurrency(function (result) {
        $scope.tranCurrencyList = result;
        $scope.voucher.CurrencyId = $scope.tranCurrencyList[0].Value;
    });

    $scope.searchByPostedTaxPayment = "VoucherNo"; $scope.searchTaxPayment = "";
    $scope.searchByPostedTaxPaymentList = [{ value: 'VoucherNo', name: "Voucher No" }, { value: 'PostingDate', name: "Posting Date" }, { value: 'DocDate', name: "Doc Date" }
       , { value: 'DocRefNo', name: "DocRef No" }];

    $scope.taxpayments = [];
    $scope.getDataList = function () {
        $http({
            method: 'POST',
            url: 'Accounts/InvoiceTax/GetTaxPaymentDataList',
            data: { column: $scope.searchByPostedTaxPayment, value: $scope.searchTaxPayment },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.taxpayments = response.data;
        });
    };
    $scope.getDataList();


    $scope.countryList = [];
    addressService.getCountryCbo(function (result) {
        $scope.countryList = result;
    });

    $scope.GetTaxCategory = function(id) {
        $scope.voucher.CountryId = id;
        $scope.TaxCategoryList = [];
        cboService.getTaxCategoryCboByCountry(id, function (result) {
            $scope.TaxCategoryList = result;
        });
    };

    bankService.getBankMasterHouseBankCboList(function (result) {
        $scope.bankMasterList = result;
    });
    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];

            $scope.voucher.AccountTitle = bank.AccountTitle;
            $scope.voucher.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
            $scope.voucher.BankMasterId = bank.BankMasterId;
        }
        $scope.hideBankPopUp();
    };
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
            
    });
    cboService.getCboEntityByCompanyWise(null, null, function (result) {
        $scope.entityList = result;
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

    cboService.getCboVoucherTypeTaxPaymentList(function (result) {
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

    $scope.getTaxPayable = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CountryId)) {
            ShowResult('Please select Country!', 'failure');
            return true;
        }

        if (baseService.isUndefinedOrNull($scope.voucher.TaxCategoryId)) {
            ShowResult('Please select Tax Cetagory!', 'failure');
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.voucher.PartyId)) {
            ShowResult('Please select ' + $scope.voucher.PartyType, 'failure');
            return true;
        }
        $http({
            method: 'GET',
            url: 'accounts/InvoiceTax/GetInvoiceTaxPayableList?fromDate=' + $filter('dateFiltering')($scope.voucher.FromDate) +
                '&toDate=' + $filter('dateFiltering')($scope.voucher.ToDate) + '&taxCategoryId=' + $scope.voucher.TaxCategoryId + '&partyType=' + $scope.voucher.PartyType
                + '&partyId=' + $scope.voucher.PartyId + '&partyPlantId=' + $scope.voucher.PartyPlantId
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
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
        $scope.getFiscalYearPeriod($scope.voucher.PostingDate);
        $scope.voucher.CurrencyId = $scope.tranCurrencyList[0].Value;
    };

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult('Please select Currency!', 'failure');
            return true;
        }

        //if (baseService.isUndefinedOrNull($scope.voucher.EntityId)) {
        //    ShowResult('Please select Entity!', 'failure');
        //    return true;
        //}
        //if ($scope.CurrencyParallel.length == 2) {
        //    if ($scope.voucher.BankAmount != $scope.voucher.InvoiceGroupAmount) {
        //        ShowResult('Bank Amount and Group Currency Amount are not equal!', 'failure');
        //        return true;
        //    }
        //}
        return false;
    };
    $scope.totalAmount = function() {
        $scope.voucher.Amount = Math.round($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount") * 100 + Number.EPSILON) / 100;
    }
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $scope.checkDocDate();
        $scope.checkPostingDate();
        $scope.totalAmount();
        if ($scope.form1.$valid  && !$scope.invalidDocDate  && !$scope.invalidPostingDate) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/InvoiceTax/InsertTaxPayment',
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

   
    $scope.closePartyPopUp = function (x) {
        var party = x.data;
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
            $scope.voucher.CurrencyId = party.CurrencyId;
            $scope.partyPlantList = [];
            $scope.getPartyPlantList(party.Id);
            $scope.voucherDetailList = [];
        }
        $scope.hidePartyPopUp();
    };

   
    $scope.vendorInvoicePaymentReport = function (voucherNo) {
        location.href = 'accounts/invoice/vendorinvoicepaymentreport?voucherNo=' + voucherNo;
    };

   
}