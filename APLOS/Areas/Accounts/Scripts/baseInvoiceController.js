'use strict';
baseInvoiceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function baseInvoiceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $scope.voucherDetailCurrencyList = [];
    $scope.partyGLList = [];
    $scope.taxExemption = false;
    $scope.IsBaseOnDueDateEnable = true;
    $scope.voucher = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        EntityId: null,
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
        TaxYearId: null,
        TaxYearName: null,
        TaxYearPeriodId: null,
        TaxYearPeriodName: null,
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
        BudgetMasterId: null,
        BudgetName: null,
        ActivityId: null,
        ActivityName: null,

        EmployeeGLGeneralInfoId: null,
        EmployeeGLGeneralInfoName: null,
        EmployeeTransactionTypeId: null,
        PartyPlantId: null,
        DeliveryPartyPlantId: null
    };

    $scope.voucherDetail = {
        EntityId: null,
        InvoiceTaxViewModel: [
            {
                TaxAmount: 0,
                TaxAutoAmount: 0,
                TaxCodeId: null,
                InvoiceDetailId: null,
                InvoiceDetailOppositEntryId: null,
                Id: null,
                WithholdCreditableGL: null,
                ExpensesGL: null,
                CreditableGL: null,
                IsWithhold: null,
                IsCreditable: null,
                IsMerge: null
            }
        ]
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

    baseService.init($scope.listUrl, null, null, "DESC", "PostingDate DESC, AdvanceNo", "AdvanceNo");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.invoiceList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.getData();

    $scope.searchInvoiceList = [
        {
            "name": "#No",
            "value": "AdvanceNo"
        },
        {
            "name": "Party Code",
            "value": "PartyCode"
        },
        {
            "name": "Party Name",
            "value": "PartyName"
        },
        {
            "name": "Ordering Party",
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

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, '', function (result) {
                $scope.entityList = result;
            });
    });

    $scope.getBudgetCboByGL = function (glgeneralInfoId) {
        cboService.getBudgetCboByGL(glgeneralInfoId, function (result) {
            $scope.BudgetItemList = result;
            if ($scope.BudgetItemList.length === 1) {
                $scope.voucherDetail.BudgetMasterId = $scope.BudgetItemList[0].Value;
                $scope.voucherDetail.BudgetName = $scope.BudgetItemList[0].Text;
                $scope.getActivity($scope.voucherDetail.BudgetMasterId);
            }
        });
    };
    $scope.SelectedBudgetItem = function (id) {
        $scope.voucherDetail.BudgetName = $('#budgetid option:selected').text();
        $scope.voucherDetail.BudgetMasterId = id;
        $scope.getActivity(id);
    };
    $scope.ActivityList = [];
    $scope.getActivity = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/Budget/GetBudgetActivityCbo?budgetId=' + id
        }).then(function successCallback(response) {
            $scope.ActivityList = response.data;
            if ($scope.ActivityList.length === 1) {
                $scope.voucherDetail.ActivityName = $scope.ActivityList[0].Text;
                $scope.voucherDetail.ActivityId = $scope.ActivityList[0].Value;
            }
        });
    };
    $scope.SelectedActivityItem = function (id) {
        $scope.voucherDetail.ActivityName = $('#activityid option:selected').text();
        $scope.voucherDetail.ActivityId = id;
    };

    $scope.setDrExchangeRate = function (amount, glId, glCode, glName, budgetMasterId, budgetCode, budgetName, activityId, activityCode, activityName, docRefNo, isSplit) {
        if (!baseService.isUndefinedOrNull(glId)) {
            var companyCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
            var companyGroupCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
            var hardCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'HardCurrency' });
            var getRow = null;
            if (isSplit) {
                //if ($scope.companyConfig.IsProfitCenterApplicable) {
                //    if ($scope.companyConfig.IsVoucherFromBudget) {
                //        getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'GLGeneralInfoId': glId, 'BudgetMasterId': budgetMasterId, 'EntityId': entityId });
                //    }
                //    else
                //        getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'GLGeneralInfoId': glId });
                //}
                //else
                //{
                if ($scope.companyConfig.IsVoucherFromBudget) {
                    getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'GLGeneralInfoId': glId, 'BudgetMasterId': budgetMasterId });
                }
                else
                    getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'GLGeneralInfoId': glId });
                //}
            }
            else {
                //if ($scope.companyConfig.IsProfitCenterApplicable) {
                //    if ($scope.companyConfig.IsVoucherFromBudget) {
                //        getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'GLGeneralInfoId': glId, 'BudgetMasterId': budgetMasterId, 'EntityId': entityId });
                //    }
                //    else
                //        getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'GLGeneralInfoId': glId });
                //}
                //else {
                if ($scope.companyConfig.IsVoucherFromBudget) {
                    getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'GLGeneralInfoId': glId, 'BudgetMasterId': budgetMasterId });
                }
                else
                    getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Dr', 'GLGeneralInfoId': glId });
                // }
            }
            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
                getRow[0].TrnType = 'Dr';
                getRow[0].GLGeneralInfoId = glId;
                getRow[0].GLGeneralInfoCode = glCode;
                getRow[0].GLGeneralInfoName = glName;
                getRow[0].BudgetMasterId = budgetMasterId;
                getRow[0].BudgetCode = budgetCode;
                getRow[0].BudgetName = budgetName;
                getRow[0].ActivityId = activityId;
                getRow[0].ActivityCode = activityCode;
                getRow[0].ActivityName = activityName;
                getRow[0].DocRefNo = docRefNo;
                // getRow[0].EntityId = entityId;
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
                    if ($scope.voucher.CurrencyId != $scope.companyCurrencyId && $scope.voucher.CurrencyId != $scope.companyGroupCurrencyId) {
                        getRow[0].CompanyGroupCurrencyDr = ((amount * companyCurrencyExchangeRate[0].ToCurrencyRate) / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                    }
                    else if ($scope.voucher.CurrencyId == $scope.companyGroupCurrencyId) {
                        getRow[0].CompanyGroupCurrencyDr = (amount / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                    }
                    else {
                        getRow[0].CompanyGroupCurrencyDr = (getRow[0].CompanyCurrencyDr / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                    }
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
                    GLGeneralInfoCode: glCode,
                    GLGeneralInfoName: glName,
                    BudgetMasterId: budgetMasterId,
                    BudgetCode: budgetCode,
                    BudgetName: budgetName,
                    ActivityId: activityId,
                    ActivityCode: activityCode,
                    ActivityName: activityName,
                    DocRefNo: docRefNo,
                    // EntityId: entityId,
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
                    if ($scope.voucher.CurrencyId != $scope.companyCurrencyId && $scope.voucher.CurrencyId != $scope.companyGroupCurrencyId) {
                        data.CompanyGroupCurrencyDr = ((amount * companyCurrencyExchangeRate[0].ToCurrencyRate) / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                    }
                    else {
                        data.CompanyGroupCurrencyDr = (amount / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                    }
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

    $scope.setCrExchangeRate = function (amount, glId, glCode, glName, budgetMasterId, budgetCode, budgetName, activityId, activityCode, activityName, docRefNo, isSplit) {
        if (!baseService.isUndefinedOrNull(glId)) {
            var companyCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyCurrency' });
            var companyGroupCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'CompanyGroupCurrency' });
            var hardCurrencyExchangeRate = $filter('filter')($scope.currencyExchangeRate, { ParallelCurrencyType: 'HardCurrency' });
            var getRow = null;
            if (isSplit) {
                if ($scope.companyConfig.IsVoucherFromBudget) {
                    getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Cr', 'GLGeneralInfoId': glId, 'BudgetMasterId': budgetMasterId });
                }
                else
                    getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Cr', 'GLGeneralInfoId': glId });
            }
            else {
                if ($scope.companyConfig.IsVoucherFromBudget) {
                    getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Cr', 'GLGeneralInfoId': glId, 'BudgetMasterId': budgetMasterId });
                }
                else
                    getRow = $filter('filter')($scope.voucherDetailCurrencyList, { 'TrnType': 'Cr', 'GLGeneralInfoId': glId });
            }
            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
                getRow[0].TrnType = 'Cr';
                getRow[0].GLGeneralInfoId = glId;
                getRow[0].GLGeneralInfoCode = glCode;
                getRow[0].GLGeneralInfoName = glName;
                getRow[0].BudgetMasterId = budgetMasterId;
                getRow[0].BudgetCode = budgetCode;
                getRow[0].BudgetName = budgetName;
                getRow[0].ActivityId = activityId;
                getRow[0].ActivityCode = activityCode;
                getRow[0].ActivityName = activityName;
                getRow[0].DocRefNo = docRefNo;
                // Base currency
                getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                getRow[0].CompanyCurrencyName = $scope.companyCurrencyName;
                getRow[0].CompanyFromCurrencyId = companyCurrencyExchangeRate[0].FromCurrencyId;
                getRow[0].ToCurrencyId = companyCurrencyExchangeRate[0].ToCurrencyId;
                getRow[0].CompanyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;
                getRow[0].CompanyCurrencyDr = null;
                getRow[0].CompanyCurrencyCr = (amount * companyCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    getRow[0].CompanyGroupCurrencyDr = null;
                    if ($scope.voucher.CurrencyId != $scope.companyCurrencyId && $scope.voucher.CurrencyId != $scope.companyGroupCurrencyId) {
                        getRow[0].CompanyGroupCurrencyCr = ((amount * companyCurrencyExchangeRate[0].ToCurrencyRate) / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                    }
                    else if ($scope.voucher.CurrencyId == $scope.companyGroupCurrencyId) {
                        getRow[0].CompanyGroupCurrencyCr = ((amount / companyGroupCurrencyExchangeRate[0].ToCurrencyRate)).toFixed(4);
                    }
                    else {
                        getRow[0].CompanyGroupCurrencyCr = (getRow[0].CompanyCurrencyCr / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                    }
                }
                // Hard currency
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
                    GLGeneralInfoCode: glCode,
                    GLGeneralInfoName: glName,
                    BudgetMasterId: budgetMasterId,
                    BudgetCode: budgetCode,
                    BudgetName: budgetName,
                    ActivityId: activityId,
                    ActivityCode: activityCode,
                    ActivityName: activityName,
                    DocRefNo: docRefNo,
                    // Base currency
                    CompanyCurrencyId: $scope.companyCurrencyId,
                    CompanyCurrencyName: $scope.companyCurrencyName,
                    CompanyFromCurrencyId: companyCurrencyExchangeRate[0].FromCurrencyId,
                    ToCurrencyId: companyCurrencyExchangeRate[0].ToCurrencyId,
                    CompanyCurrencyRate: companyCurrencyExchangeRate[0].ToCurrencyRate,
                    CompanyCurrencyDr: null,
                    CompanyCurrencyCr: (amount * companyCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4)
                };
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    data.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    data.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;
                    data.CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                    data.CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    data.CompanyGroupCurrencyDr = null;
                    data.CompanyGroupCurrencyCr = (amount / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
                }
                // Hard currency
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    data.HardCurrencyId = $scope.hardCurrencyId;
                    data.HardCurrencyName = $scope.hardCurrencyName;
                    data.HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                    data.HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                    data.HardCurrencyDr = null;
                    data.HardCurrencyCr = (amount * hardCurrencyExchangeRate[0].ToCurrencyRate).toFixed(4);
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
                            $scope.changePaymentTerm($scope.voucher.PaymentTermId)
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

    $scope.paymentTerm = function () {
        if ($scope.partyType === 'Customer')
            $scope.paymenttermUrl = 'accounts/PaymentTerm/getcustomercbo';
        else if ($scope.partyType === 'Vendor')
            $scope.paymenttermUrl = 'accounts/PaymentTerm/getvendorcbo';
        $http({
            method: 'GET',
            url: $scope.paymenttermUrl
        }).then(function successCallback(response) {
            $scope.paymentTermList = response.data;
            console.log($scope.paymentTermList);
        });
    }
    $scope.paymentTerm();

    $scope.changeCurrencyExchangeRate = function () {
        angular.forEach($scope.voucherDetailList, function (item, i) {
            if (item.TrnType == 'Dr') {
                $scope.setDrExchangeRate(item.Amount, item.GLGeneralInfoId, item.GLGeneralInfoCode, item.GLGeneralInfoName,
                    item.BudgetMasterId, item.BudgetCode, item.BudgetName,
                    item.ActivityId, item.ActivityCode, item.ActivityName, item.DocRefNo, $scope.voucher.IsSplit);
            }
            else if (item.TrnType == 'Cr') {
                $scope.setCrExchangeRate(item.Amount, item.GLGeneralInfoId, item.GLGeneralInfoCode, item.GLGeneralInfoName, item.BudgetMasterId
                    , item.BudgetCode, item.BudgetName,
                    item.ActivityId, item.ActivityCode, item.ActivityName, item.DocRefNo, $scope.voucher.IsSplit);
            }
        });
    };

    $scope.changePaymentTerm = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) {
                return item.Value === id;
            })[0];
            $scope.voucher.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.voucher.BaseNoOfDays = paymentTerm.NoOfDay;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate') {
                    $scope.voucher.BaseOnDueDate = $scope.voucher.DocDate;
                    $scope.IsBaseOnDueDateEnable = true;
                } else if (paymentTerm.BaseLineDate === 'postingdate') {
                    $scope.voucher.BaseOnDueDate = $scope.voucher.PostingDate;
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else if (paymentTerm.BaseLineDate === 'voucherdate') {
                    $scope.voucher.BaseOnDueDate = $filter('dateFiltering')(Date.now());
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else {
                    $scope.IsBaseOnDueDateEnable = false;
                    $scope.voucher.BaseOnDueDate = $filter('dateFiltering')(Date.now());
                }
            $scope.getMatureDate($scope.voucher.BaseOnDueDate, $scope.voucher.BaseNoOfDays);
        }
    };

    $scope.getMatureDate = function (date, days) {
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.voucher.MatureDate = $filter('date')(date, 'dd-MMM-yyyy');
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
        if (baseService.isUndefinedOrNull($scope.voucher.EntityId)) {
            return manualValidation('div_entity', $scope.invalidEntity, 'Entity is required.');
        }
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.voucherDetailList[i].EntityId)) {
                $scope.invalidEntity = baseService.isUndefinedOrNull($scope.voucherDetailList[i].EntityId);
                return ShowResult('Entity is required where GL is ' + $scope.voucherDetailList[i].GLGeneralInfoName, 'failure');
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
                ShowResult('Invoice Amount must greater than 0!', 'failure');
                return true;
            }
            if ($scope.voucher.IsSplit) {
                var vdetailDrAmount = $filter('sumByKey')($filter('filter')($scope.voucherDetailList, { TrnType: 'Dr' }), 'DrAmount');
                if (parseFloat($scope.voucher.Amount) != vdetailDrAmount) {
                    ShowResult('Splited Amount is not equal  Invoice Amount!', 'failure');
                    return true;
                }
            }
            var vdetailCrAmount = $filter('filter')($scope.voucherDetailList, { TrnType: 'Cr' })
            if (vdetailCrAmount.length == 0) {
                ShowResult('There is no Sales Entry!', 'failure');
                return true;
            }
            else {
                for (var i = 0; i < vdetailCrAmount.length; i++) {
                    if (vdetailCrAmount[i].Amount == 0) {
                        ShowResult(vdetailCrAmount[i].GLGeneralInfoName + ' Amount must greater than 0!', 'failure');
                        return true;
                    }
                }
            }
        }
        else if ($scope.partyType == 'Vendor') {
            if (baseService.isUndefinedOrNull($scope.voucher.PartyId)) {
                ShowResult('Please select Vendor!', 'failure');
                return true;
            }
            if ($scope.voucher.Amount == 0) {
                ShowResult('Invoice Amount must greater than 0!', 'failure');
                return true;
            }
            var vdetailDr = $filter('filter')($scope.voucherDetailList, { TrnType: 'Dr' })
            if (vdetailDr.length == 0) {
                ShowResult('There is no Purchase Entry!', 'failure');
                return true;
            }
            else {
                for (var i = 0; i < vdetailDr.length; i++) {
                    if (vdetailDr[i].Amount == 0) {
                        ShowResult(vdetailDr[i].GLGeneralInfoName + ' Amount must greater than 0!', 'failure');
                        return true;
                    }
                }
            }
        }
        return false;
    };
    $scope.checkDrCrBalancing = function () {
        var companyCurrencyAmountDr = $filter('sumByKey')($filter('filter')($scope.voucherDetailCurrencyList, { TrnType: 'Dr' }), 'CompanyCurrencyDr');
        var companyCurrencyAmountCr = $filter('sumByKey')($filter('filter')($scope.voucherDetailCurrencyList, { TrnType: 'Cr' }), 'CompanyCurrencyCr');
        if (companyCurrencyAmountDr == 0) {
            ShowResult($scope.companyCurrencyCode + ' Dr amount can not be zero!', 'failure');
            $scope.setTab(3);
            return false;
        }
        if (companyCurrencyAmountCr == 0) {
            ShowResult($scope.companyCurrencyCode + ' Cr amount can not be zero!', 'failure');
            $scope.setTab(3);
            return false;
        }
        if (companyCurrencyAmountDr !== companyCurrencyAmountCr) {
            ShowResult($scope.companyCurrencyCode + ' Dr amount and Cr amount is not equal!', 'failure');
            $scope.setTab(3);
            return false;
        }
        if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
            var companyGroupCurrencyAmountDr = $filter('sumByKey')($filter('filter')($scope.voucherDetailCurrencyList, { TrnType: 'Dr' }), 'CompanyGroupCurrencyDr');
            var companyGroupCurrencyAmountCr = $filter('sumByKey')($filter('filter')($scope.voucherDetailCurrencyList, { TrnType: 'Cr' }), 'CompanyGroupCurrencyCr');
            if (companyGroupCurrencyAmountDr == 0) {
                ShowResult($scope.companyGroupCurrencyCode + ' Dr amount can not be zero!', 'failure');
                $scope.setTab(3);
                return false;
            }
            if (companyGroupCurrencyAmountCr == 0) {
                ShowResult($scope.companyGroupCurrencyCode + ' Cr amount can not be zero!', 'failure');
                $scope.setTab(3);
                return false;
            }
            if (companyGroupCurrencyAmountDr !== companyGroupCurrencyAmountCr) {
                ShowResult($scope.companyGroupCurrencyCode + ' Dr amount and Cr amount is not equal!', 'failure');
                $scope.setTab(3);
                return false;
            }
        }
        if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
            var hardCurrencyAmountDr = $filter('sumByKey')($filter('filter')($scope.voucherDetailCurrencyList, { TrnType: 'Dr' }), 'HardCurrencyDr');
            var hardCurrencyAmountCr = $filter('sumByKey')($filter('filter')($scope.voucherDetailCurrencyList, { TrnType: 'Cr' }), 'HardCurrencyCr');
            if (hardCurrencyAmountDr == 0) {
                ShowResult($scope.hardCurrencyCode + ' Dr amount can not be zero!', 'failure');
                $scope.setTab(3);
                return false;
            }
            if (hardCurrencyAmountCr == 0) {
                ShowResult($scope.hardCurrencyCode + ' Cr amount can not be zero!', 'failure');
                $scope.setTab(3);
                return false;
            }
            if (hardCurrencyAmountDr !== hardCurrencyAmountCr) {
                ShowResult($scope.hardCurrencyCode + ' Dr amount and Cr amount is not equal!', 'failure');
                $scope.setTab(3);
                return false;
            }
        }
        return true;
    };

    $scope.closePartyPopUp = function () {
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
                $scope.removeDrRow();
                $scope.voucher.PartyId = party.Id;
                $scope.voucher.PartyCode = party.Code;
                $scope.voucher.PartyName = party.UserName;
                $scope.voucher.PartyType = $scope.partyType;
                $scope.voucher.GLGeneralInfoId = party.ReconciliationGLId;
                $scope.voucher.GLGeneralInfoCode = party.ReconciliationGLCode;
                $scope.voucher.GLGeneralInfoName = party.ReconciliationGLName;
                $scope.voucher.BudgetMasterId = party.ReconciliationBudgetId;
                $scope.voucher.BudgetCode = party.ReconciliationBudgetCode;
                $scope.voucher.BudgetName = party.ReconciliationBudgetName;
                $scope.voucher.ActivityId = party.ReconciliationActivityId;
                $scope.voucher.ActivityCode = party.ReconciliationActivityCode;
                $scope.voucher.ActivityName = party.ReconciliationActivityName;
                $scope.voucher.PaymentTermId = party.PaymentTermId;
                if ($scope.voucher.PaymentTermId != null) {
                    $scope.changePaymentTerm($scope.voucher.PaymentTermId)
                }
                $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                $scope.voucherDetail.GLGeneralInfoId = $scope.voucher.GLGeneralInfoId;
                $scope.voucherDetail.GLGeneralInfoName = $scope.voucher.GLGeneralInfoName;
                $scope.voucherDetail.GLGeneralInfoCode = $scope.voucher.GLGeneralInfoCode;
                $scope.voucherDetail.BudgetMasterId = $scope.voucher.BudgetMasterId;
                $scope.voucherDetail.BudgetName = $scope.voucher.BudgetName;
                $scope.voucherDetail.ActivityId = $scope.voucher.ActivityId;
                $scope.voucherDetail.ActivityName = $scope.voucher.ActivityName;
                $scope.voucherDetail.DocDate = $scope.voucher.DocDate;
                $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                $scope.voucherDetail.Narration = $scope.voucher.Narration;
                $scope.voucherDetail.Amount = $scope.voucher.Amount;
                $scope.voucherDetail.TrnType = $scope.partyType == 'Customer' ? 'Dr' : 'Cr';

                if ($scope.partyType === 'Customer') {
                    cboService.getCboCompanyPartyReconAdditionalGLList($scope.voucher.PartyId, $scope.partyType, function (result) {
                        $scope.partyGLList = result;

                        $scope.setDrExchangeRate($scope.voucher.Amount,
                            $scope.voucher.GLGeneralInfoId, $scope.voucher.GLGeneralInfoCode, $scope.voucher.GLGeneralInfoName,
                            $scope.voucher.BudgetMasterId, $scope.voucher.BudgetCode, $scope.voucher.BudgetName,
                            $scope.voucher.ActivityId, $scope.voucher.ActivityCode, $scope.voucher.ActivityName, $scope.voucher.DocRefNo, $scope.voucher.IsSplit);
                    });
                }
                else if ($scope.partyType === 'Vendor') {
                    cboService.getCboCompanyPartyReconAdditionalGLList($scope.voucher.PartyId, $scope.partyType, function (result) {
                        $scope.partyGLList = result;
                        $scope.setCrExchangeRate($scope.voucher.Amount,
                            $scope.voucher.GLGeneralInfoId, $scope.voucher.GLGeneralInfoCode, $scope.voucher.GLGeneralInfoName,
                            $scope.voucher.BudgetMasterId, $scope.voucher.BudgetCode, $scope.voucher.BudgetName,
                            $scope.voucher.ActivityId, $scope.voucher.ActivityCode, $scope.voucher.ActivityName, $scope.voucher.DocRefNo, false);
                    });
                }
                $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                $scope.taxCodDataList = [];
                $scope.getPartyPlantList(party.Id);

                clearVoucherDetail();
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

    $scope.addRow = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult('Please select Currency!', 'failure');
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.selectedInvoiceGLId)) {
            ShowResult('Please select GL.', 'failure');
            return;
        }
        var getRow = null;
        if ($scope.partyType === 'Customer') {
            //if ($scope.companyConfig.IsProfitCenterApplicable) {
            //    if ($scope.companyConfig.IsVoucherFromBudget) {
            //        getRow = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Cr', 'GLGeneralInfoId': $scope.selectedInvoiceGLId, 'BudgetMasterId': $scope.voucherDetail.BudgetMasterId, 'EntityId': $scope.voucher.EntityId });
            //    }
            //    else
            //        getRow = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Cr', 'GLGeneralInfoId': $scope.selectedInvoiceGLId, 'EntityId': $scope.voucher.EntityId });
            //}
            //else {
            if ($scope.companyConfig.IsVoucherFromBudget) {
                if (baseService.isUndefinedOrNull($scope.voucherDetail.BudgetMasterId)) {
                    ShowResult('Please select Budget.', 'failure');
                    return;
                }
                else
                    getRow = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Cr', 'GLGeneralInfoId': $scope.selectedInvoiceGLId, 'BudgetMasterId': $scope.voucherDetail.BudgetMasterId });
            }
            else
                getRow = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Cr', 'GLGeneralInfoId': $scope.selectedInvoiceGLId });
            //}
        }
        if ($scope.partyType === 'Vendor') {
            //if ($scope.companyConfig.IsProfitCenterApplicable) {
            //    if ($scope.companyConfig.IsVoucherFromBudget) {
            //        getRow = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Dr', 'GLGeneralInfoId': $scope.selectedInvoiceGLId, 'BudgetMasterId': $scope.voucherDetail.BudgetMasterId, 'EntityId': $scope.voucher.EntityId });
            //    }
            //    else
            //        getRow = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Dr', 'GLGeneralInfoId': $scope.selectedInvoiceGLId, 'EntityId': $scope.voucher.EntityId });
            //}
            //else {
            if ($scope.companyConfig.IsVoucherFromBudget) {
                if (baseService.isUndefinedOrNull($scope.voucherDetail.BudgetMasterId)) {
                    ShowResult('Please select Budget.', 'failure');
                    return;
                }
                else
                    getRow = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Dr', 'GLGeneralInfoId': $scope.selectedInvoiceGLId, 'BudgetMasterId': $scope.voucherDetail.BudgetMasterId });
            }
            else
                getRow = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Dr', 'GLGeneralInfoId': $scope.selectedInvoiceGLId });
            //}
        }

        if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].GLGeneralInfoId == $scope.selectedInvoiceGLId) {
            ShowResult('This GL is already added!', 'failure');
        }
        else {
            $scope.voucherDetail.Id = baseService.pk();
            $scope.voucherDetail.GLGeneralInfoId = $scope.selectedInvoiceGLId;
            $scope.voucherDetail.GLGeneralInfoCode = $scope.selectedInvoiceGLCode;
            $scope.voucherDetail.GLGeneralInfoName = $scope.selectedInvoiceGLName;
            $scope.voucherDetail.PostingWithoutTaxAllow = $scope.selectedInvoiceGLPostingWithoutTaxAllow;

            $scope.voucherDetail.DocDate = $scope.voucher.DocDate;
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
            $scope.voucherDetail.Amount = 0;

            $scope.voucherDetail.TrnType = $scope.partyType == 'Customer' ? 'Cr' : 'Dr';

            if ($scope.partyType === 'Customer') {
                $scope.setCrExchangeRate($scope.voucherDetail.Amount,
                    $scope.voucherDetail.GLGeneralInfoId, $scope.voucherDetail.GLGeneralInfoCode, $scope.voucherDetail.GLGeneralInfoName,
                    $scope.voucherDetail.BudgetMasterId, $scope.voucherDetail.BudgetCode, $scope.voucherDetail.BudgetName,
                    $scope.voucherDetail.ActivityId, $scope.voucherDetail.ActivityCode, $scope.voucherDetail.ActivityName, $scope.voucherDetail.DocRefNo, true);
            }
            else if ($scope.partyType === 'Vendor') {
                $scope.setDrExchangeRate($scope.voucherDetail.Amount,
                    $scope.voucherDetail.GLGeneralInfoId, $scope.voucherDetail.GLGeneralInfoCode, $scope.voucherDetail.GLGeneralInfoName,
                    $scope.voucherDetail.BudgetMasterId, $scope.voucherDetail.BudgetCode, $scope.voucherDetail.BudgetName,
                    $scope.voucherDetail.ActivityId, $scope.voucherDetail.ActivityCode, $scope.voucherDetail.ActivityName, $scope.voucher.DocRefNo, true);
            }
            $scope.voucherDetail.InvoiceTaxViewModel = [];
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            clearVoucherDetail();
            $scope.searchStr = null;
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
            $scope.voucher.BudgetMasterId = drGL.BudgetMasterId;
            $scope.voucher.BudgetName = drGL.BudgetMasterId;
            $scope.voucher.ActivityId = drGL.ActivityId;
            $scope.voucher.ActivityName = drGL.ActivityId;

            row[0].Amount = $scope.voucher.Amount;
            row[0].GLGeneralInfoId = $scope.voucher.GLGeneralInfoId;
            row[0].GLGeneralInfoName = $scope.voucher.GLGeneralInfoName;
            row[0].BudgetMasterId = $scope.voucher.BudgetMasterId;
            row[0].BudgetName = $scope.voucher.BudgetName;
            row[0].ActivityId = $scope.voucher.ActivityId;
            row[0].ActivityName = $scope.voucher.ActivityName;

            $scope.setDrExchangeRate(row[0].Amount, row[0].GLGeneralInfoId, row[0].GLGeneralInfoCode, row[0].GLGeneralInfoName, row[0].BudgetMasterId, row[0].BudgetCode, row[0].BudgetName, row[0].ActivityId, row[0].ActivityCode, row[0].ActivityName, $scope.voucher.DocRefNo, false);
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
            drRow[0].BudgetMasterId = drGL.BudgetMasterId;
            drRow[0].BudgetName = drGL.BudgetName;
            drRow[0].ActivityId = drGL.ActivityId;
            drRow[0].ActivityName = drGL.ActivityName;

            $scope.setDrExchangeRate(drRow[0].Amount,
                drRow[0].GLGeneralInfoId, drRow[0].GLGeneralInfoCode, drRow[0].GLGeneralInfoName,
                drRow[0].BudgetMasterId, drRow[0].BudgetCode, drRow[0].BudgetName,
                drRow[0].ActivityId, drRow[0].ActivityCode, drRow[0].ActivityName, $scope.voucher.DocRefNo, false);
        }
        else {
            $scope.voucherDetail.GLGeneralInfoId = drGL.GLGeneralInfoId;
            $scope.voucherDetail.GLGeneralInfoName = drGL.GLGeneralInfoName;
            $scope.voucherDetail.BudgetMasterId = null;
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
                $scope.voucherDetail.GLGeneralInfoId, $scope.voucherDetail.GLGeneralInfoCode, $scope.voucherDetail.GLGeneralInfoName,
                $scope.voucherDetail.BudgetMasterId, $scope.voucherDetail.BudgetCode, $scope.voucherDetail.BudgetName,
                $scope.voucherDetail.ActivityId, $scope.voucherDetail.ActivityCode, $scope.voucherDetail.ActivityName, $scope.voucher.DocRefNo, true);
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

    $scope.splitInvoice = function () {
        $scope.removeDrRow();
        if (!$scope.voucher.IsSplit) {
            if ($scope.partyType === 'Customer') {
                $scope.setDrExchangeRate($scope.voucher.Amount,
                    $scope.voucher.GLGeneralInfoId, $scope.voucher.GLGeneralInfoCode, $scope.voucher.GLGeneralInfoName,
                    $scope.voucher.BudgetMasterId, $scope.voucher.BudgetCode, $scope.voucher.BudgetName,
                    $scope.voucher.ActivityId, $scope.voucher.ActivityCode, $scope.voucher.ActivityName, $scope.voucher.DocRefNo, false);
            }
            else if ($scope.partyType === 'Vendor') {
                $scope.setCrExchangeRate($scope.voucher.Amount,
                    $scope.voucher.GLGeneralInfoId, $scope.voucher.GLGeneralInfoCode, $scope.voucher.GLGeneralInfoName,
                    $scope.voucher.BudgetMasterId, $scope.voucher.BudgetCode, $scope.voucher.BudgetName,
                    $scope.voucher.ActivityId, $scope.voucher.ActivityCode, $scope.voucher.ActivityName, $scope.voucher.DocRefNo, false);
            }
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.GLGeneralInfoId = $scope.voucher.GLGeneralInfoId;
            $scope.voucherDetail.GLGeneralInfoName = $scope.voucher.GLGeneralInfoName;
            $scope.voucherDetail.BudgetMasterId = $scope.voucher.BudgetMasterId;
            $scope.voucherDetail.BudgetName = $scope.voucher.BudgetName;
            $scope.voucherDetail.ActivityId = $scope.voucher.ActivityId;
            $scope.voucherDetail.ActivityName = $scope.voucher.ActivityName;
            $scope.voucherDetail.DocDate = $scope.voucher.DocDate;
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.Amount = $scope.voucher.Amount;
            $scope.voucherDetail.TrnType = $scope.partyType == 'Customer' ? 'Dr' : 'Cr';
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            clearVoucherDetail();
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
            console.log('invoiceGLList', $scope.invoiceGLList);
        },
        function errorCallback(response) {
            ShowResult(response, 'failure');
        });

    $scope.selectedInvoiceGLId = null;
    $scope.selectedInvoiceGLName = null;
    $scope.selectedInvoiceGLPostingWithoutTaxAllow = null;
    $scope.selectedInvoiceGL = function (selected) {
        if (selected) {
            $scope.selectedInvoiceGLId = selected.originalObject.GLGeneralInfoId;
            $scope.selectedInvoiceGLName = selected.originalObject.GLGeneralInfoName;
            $scope.selectedInvoiceGLPostingWithoutTaxAllow = selected.originalObject.PostingWithoutTaxAllow;
            if ($scope.companyConfig.IsVoucherFromBudget) {
                $scope.getBudgetCboByGL(selected.originalObject.GLGeneralInfoId);
            }
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
        $scope.taxCodDataList = [];
        $scope.voucherDetailCurrencyList = [];
        $scope.getFiscalYearPeriod($scope.voucher.PostingDate);
        $scope.selectedInvoiceGLId = null;
        $scope.plantList = [];
        $scope.GLGeneralInfoName = null;
        $scope.voucherDetail
        $scope.BudgetItemList = [];
        $scope.ActivityList = [];
        clearVoucherDetail();
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