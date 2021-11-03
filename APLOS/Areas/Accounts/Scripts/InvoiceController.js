'use strict';
InvoiceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'toaster'];
function InvoiceController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, toaster) {
    $rootScope.title = 'Inbound Invoice';
    $scope.Action = 'Park';
    $scope.SAction = 'Post';
    $scope.CAction = 'Add';
    $scope.CAIction = 'Add';
    $scope.index = -1;
    $scope.vouchers = [];
    $scope.vendorInvoiceDatas = [];
    $scope.voucherDetails = [];
    $scope.voucherDetailrow = [];
    $scope.voucherDetailCurrency = [];
    $scope.CurrencyParallel = [];
    $scope.voucherDetailCurrencyrowratechange = [];
    $scope.VendorInvoiceDetail = [];
    $scope.invoiceDetailCurrencyrow = [];
    $scope.voucherDetailCurrencyrow = [];
    $scope.vendorSplitGLList = [];
    $scope.voucherDetailInvoiceSplitRow = [];
    $scope.CurrencyCheckParallel = [];
    $scope.CheckParallelCurrencyBySelecteCurrency = [];
    $scope.currencyList = [];
    $scope.InvoiceSplit = [];
    $scope.currencyTableshowhide = true;
    $scope.vendorInvoiceDetailIdguid = null;
    $scope.clickCount = 0;/*TODO*/
    $scope.disableGL = false;
    $scope.updateDiable = true;/*When click row edit icon then this flag will true and diable
        posting date,currency,edit row icon,GL.After update successfully it will false*/
    $scope.RateAmountDisable = true;/**/
    $scope.IsAssetBudgetYear = false;
    $scope.IsExpenseBudgetYear = false;
    $scope.IscurrencyHideShow = false;
    $scope.fiscalYearList = [];
    $scope.fiscalYearPeriodByIdList = [];
    $scope.IsExpenses = false;
    $scope.path = 'accounts/voucher/';
    $scope.vendorInvoiceVoucherXLUrl = 'accounts/voucher/vendorinvoicevoucherreport';
    $scope.saveUrl = $scope.path + 'insertcustomerinvoice';
    $scope.parkUrl = $scope.path + 'customerinvoicepark';
    $scope.postUrl = $scope.path + 'vendorinvoicepost';
    $scope.updateUrl = $scope.path + 'invoiceupdate';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'GetVendorInvoiceVoucherData';
    baseService.init($scope.getListUrl, null, null, 'desc', 'VoucherNo', 'VoucherNo');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.vendorInvoiceDatas = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.getData();
    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        cboService.getCboEntityByCompanyWise(null, null, function (result) {
            $scope.entityList = result;
            $scope.detailentityList = result;
            if ($scope.entityList.length == 1) {
                $scope.vendorInvoice.EntityId = $scope.entityList[0].Value;
            }
        });
    });

    $scope.voucher = {
        Id: null,
        CustomerInvoiceId: null,
        CurrencyId: null,
        VoucherTypeId: null,
        PartyId: null,
        APLOS0RDId: null,
        Sequence: 0,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: $filter("dateFiltering")(Date.now()),
        DocRefNo: null,
        DocDate: $filter("dateFiltering")(Date.now()),
        FiscalYearId: null,
        FiscalYearPeriodId: null,
        IsExcludingTax: false,
        VoucherDetailId: null,
        Amount: 0,
        BaseOnDueDate: $filter("dateFiltering")(Date.now()),
        BaseNoOfDays: null,
        PaymentTermId: null,
        Narration: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter("dateFiltering")(Date.now()),
        AddedFromIP: null
    };
    $scope.voucherDetail = {
        Id: null,
        VoucherId: null,
        CustomerInvoiceDetailId: null,
        BudgetMasterId: null,
        BudgetId: null,
        BudgetName: null,
        ActivityId: null,
        EntityId: null,
        EntityName: null,
        CurrencyId: null,
        VoucherTypeId: null,
        GLGeneralInfoId: null,
        COAICode: null,
        COAIText: null,
        GLTextAndCode: null,
        OldCOAICode: null,
        DocRefNo: null,
        DocDate: $filter('date')(Date.now(), 'dd-MMM-yyyy'),
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        PeriodName: null,
        DrAmount: 0,
        CrAmount: 0,
        Amount: 0,
        TaxAmount: 0,
        NetAmount: 0,
        Active: true,
        AddedBy: null,
        AddedDate: $filter('date')(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        PostingWithoutTaxAllow: false,
        TaxCategory: null,
        taxCategoryStatus: false,
        VendorInvoiceTax: [
            {
                TaxAmount: 0,
                TaxAutoAmount: 0,
                TaxCodeId: null,
                VendorInvoiceDetailId: null,
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

    $scope.additionalexchangerate = {
        ToCurrencyRate: null,
        FromCurrencyUnit: 1,
        FromCurrencyCode: null,
        ToCurrency: null
    };
    $scope.vendorInvoice = {
        Amount: 0,
        PartyId: null,
        PostingDate: $filter("dateFiltering")(Date.now()),
        GLGeneralInfoId: null,
        BudgetId: null,
        ActivityId: null,
        CurrencyId: null,
        DocRefNo: null,
        DocDate: $filter("dateFiltering")(Date.now()),
        Id: null,
        IsExcludingTax: false,
        PaymentTermId: null,
        BaseOnDueDate: $filter("dateFiltering")(Date.now()),
        BaseNoOfDays: 0,
        TempId: $scope.vendorInvoiceDetailIdguid,
        FiscalYearId: null,
        FiscalYearPeriodId: null
    };

    $scope.voucherInvoiceSplit = {
        Id: null,
        Amount: 0,
        CrAmount: 0,
        COAICode: null,
        COAIText: null,
        GLTextAndCode: null,
        GLGeneralInfoId: null,
        BudgetId: null,
        BudgetName: null,
        ActivityId: null,
        CurrencyId: null,
        TempId: null,
        DocRefNo: null,
        DocDate: $filter("dateFiltering")(Date.now())
    };
    // #endregion

    $('.datepicker').datepicker({
        format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
    });

    $scope.GetCurrencyParallel = function () {
        $http({
            method: 'GET',
            url: 'currencies/CompanyParallelCurrency/CurrencyParallel',
        }).then(function successCallback(response) {
            $scope.CurrencyParallel = response.data;
            if ($scope.CurrencyParallel.length == 0) {
                ShowResult('Company Parallel Currency did not set!', 'failure');
                $scope.showform = false;
            }
            else {
                $scope.showform = true;
            }
            $scope.BaseCurrencyCode = $scope.CurrencyParallel[0].Code;
            if ($scope.CurrencyParallel.length > 0 && $scope.CurrencyParallel.length < 2) {
                $scope.ParagroupCurrencyTab = false;
                $scope.ParahardCurrencyTab = false;
            }
            else if ($scope.CurrencyParallel.length > 1 && $scope.CurrencyParallel.length < 3) {
                $scope.ParagroupCurrencyTab = true;
                $scope.ParahardCurrencyTab = false;
            }
            else {
                $scope.ParagroupCurrencyTab = true;
                $scope.ParahardCurrencyTab = true;
            }
        });
    };
    $scope.GetCurrencyParallel();
    $scope.CheckParallelCurrencyBySelecteCurr = function (item) {
        $http({
            method: 'GET',
            url: 'currencies/CompanyParallelCurrency/CheckParallelCurrencyBySelecteCurrency?currencyid=' + item
        }).then(function successCallback(response) {
            $scope.CheckParallelCurrencyBySelecteCurrency = response.data;
        });
    };

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.tranCurrencyList = result;
    });

    $scope.searchVendorInvoiceList = [
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
            'name': 'Customer',
            'value': 'Party'
        },
        {
            'name': 'Currency',
            'value': 'Currency'
        },
    ];

    // #region ********Get CustomerInvoice************
    $scope.VendorInvs = [];
    $scope.voucherDtails = [];
    $scope.vendorInvoice = [];
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.voucher = $scope.vendorInvoiceDatas[$scope.index];
        $scope.voucher.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        $scope.voucher.VoucherDate = $filter("dateFiltering")($scope.voucher.VoucherDate);
        $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucher.PostingDate);
        $scope.GetAdditionalexchangerate($scope.voucher.Id);
        $scope.onVoucherDetailCurrencyExchangeRateSelected($scope.voucher.Id);
        $scope.GetCurrencyExchangeRateList();//Modified
        $scope.crRowSelected = $scope.voucher.PartyId;
        $scope.Action = 'Park';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.customerInvoiceVoucherDetailDR = [];

    $scope.GetAdditionalexchangerate = function (item) {
        $http({
            method: 'get',
            url: 'accounts/voucher/GetAdditionalexchangerateById?voucherId=' + item,
        }).then(function successCallback(response) {
            $scope.additionalexchangerate = response.data.Rows[0];
        });
    };

    function VendorInvoiceDetailData(customerInvoiceId) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetCustomerInvoiceDetailData?customerInvoiceId=' + customerInvoiceId
        }).then(function successCallback(response) {
            $scope.customerInvoiceDetails = response.data;
            for (var i = 0; i < $scope.customerInvoiceDetails.length; i++) {
                $scope.customerInvoiceDetails[i].TempId = $scope.customerInvoiceDetails[i].Id;
            }
            if ($scope.customerInvoiceDetails.length > 1) {
                for (var i = 0; i < $scope.customerInvoiceDetails.length; i++) {
                    $scope.voucherDetailInvoiceSplitRow[i] = $scope.customerInvoiceDetails[i];
                }
            }
            console.log('customerInvoiceAlternative1', $scope.customerInvoiceDetails);
        });
    };

    $scope.onVoucherDetailCurrencyExchangeRateSelected = function (item) {
        $http({
            method: 'get',
            url: 'accounts/voucher/GetGeneralVoucherDetailCurrencyExchangeRate?voucherId=' + item
        }).then(function successCallback(response) {
            $scope.voucherDetailCurrency = response.data.Rows;
            if ($scope.voucherDetailCurrency[1].ParallelCurrencyType == 'CompanyGroupCurrency'
                && $scope.voucherDetailCurrency[1].ParallelCurrencyId == $scope.voucher.CurrencyId) {
                $scope.voucherDetailCurrency[1].ToCurrencyRate = $scope.voucherDetailCurrency[0].ToCurrencyRate;
                $scope.voucherDetailCurrency[0].ToCurrencyRate = 1;
            }
            if ($scope.voucherDetailCurrency[2].ParallelCurrencyType == 'HardCurrency'
                && $scope.voucherDetailCurrency[2].ParallelCurrencyId == $scope.voucher.CurrencyId) {
                $scope.voucherDetailCurrency[2].ToCurrencyRate = $scope.voucherDetailCurrency[0].ToCurrencyRate;
                $scope.voucherDetailCurrency[0].ToCurrencyRate = 1;
            }
            excurrencyRate($scope.voucherDetailCurrency, false);
        });
    };
    $scope.voucherTypeList = [];

    $scope.tranCurrencyList = [];

    $scope.partyList = [];
    $http({
        method: 'GET',
        url: 'Parties/party/getpartycbo',
    }).then(function successCallback(response) {
        $scope.partyList = response.data;
    });

    $http({
        method: 'GET',
        url: 'accounts/fiscalyearperiod/getcbo',
    }).then(function (response) {
        $scope.fiscalYearPeriodList = response.data;
    });

    cboService.getCboVoucherTypeAccountPayableList(function (result) {
        $scope.voucherTypeList = result;
        if ($scope.voucherTypeList.length === 1) {
            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
        }
    });

    $scope.getBudgetCboByGL = function (glgeneralInfoId) {
        cboService.getBudgetCboByGL(glgeneralInfoId, function (result) {
            $scope.BudgetItemList = result;
            if ($scope.BudgetItemList.length == 1) {
                $scope.voucherDetail.BudgetId = $scope.BudgetItemList[0].Value;
                $scope.voucherDetail.BudgetName = $scope.BudgetItemList[0].Text;
                $scope.getActivity(glgeneralInfoId);
            }
        });
    };

    $scope.getSplitBudgetCboByGL = function (glgeneralInfoId) {
        cboService.getBudgetCboByGL(glgeneralInfoId, function (result) {
            $scope.SplitBudgetItemList = result;
            if ($scope.SplitBudgetItemList.length == 1) {
                $scope.voucherInvoiceSplit.BudgetId = $scope.SplitBudgetItemList[0].Value;
                $scope.voucherInvoiceSplit.BudgetName = $scope.SplitBudgetItemList[0].Text;
                $scope.getSplitActivity(glgeneralInfoId);
            }
        });
    };

    $scope.SelectedBudgetItem = function (id) {
        $scope.voucherDetail.BudgetName = $('#budgetid option:selected').text();
        $scope.voucherDetail.BudgetId = id;
        $scope.getActivity(id);
    };

    $scope.SelectedSplitBudgetItem = function (id) {
        $scope.voucherInvoiceSplit.BudgetName = $('#splitbudgetid option:selected').text();
        $scope.voucherInvoiceSplit.BudgetId = id;
        $scope.getActivity(id);
    };

    $scope.ActivityList = [];
    $scope.getActivity = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/Budget/GetBudgetActivityCbo?budgetId=' + id,
        }).then(function successCallback(response) {
            $scope.ActivityList = response.data;
        })
    }
    $scope.SplitActivityList = [];
    $scope.getSplitActivity = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/Budget/GetBudgetActivityCbo?budgetId=' + id,
        }).then(function successCallback(response) {
            $scope.SplitActivityList = response.data;
        })
    }
    $scope.SelectedActivityItem = function (id) {
        $scope.voucherDetail.ActivityName = $('#activityid option:selected').text();
        $scope.voucherDetail.ActivityId = id;
    }
    $scope.SelectedSplitActivityItem = function (id) {
        $scope.voucherInvoiceSplit.ActivityName = $('#splitactivityid option:selected').text();
        $scope.voucherInvoiceSplit.ActivityId = id;
    }

    $scope.SelectedEntityItem = function (id) {
        $scope.voucherDetail.EntityName = $('#entityId option:selected').text();
        $scope.voucherDetail.EntityId = id;
    };
    $scope.SelectedSplitEntityItem = function (id) {
        $scope.voucherInvoiceSplit.EntityName = $('#splitentityId option:selected').text();
        $scope.voucherInvoiceSplit.EntityId = id;
    }

    cboService.getCboEntityCostCenter(function (result) {
        $scope.costCenterList = result;
    });

    $scope.getEntityCboByCostCenter = function (costCenterId) {
        $scope.voucherDetail.CostCenterName = $('#costCenterId option:selected').text();
        $scope.voucherDetail.CostCenterId = costCenterId;

        cboService.getCboEntityByCostCenter(costCenterId, function (result) {
            $scope.costCenterEntityList = result;
        });
    }
    $scope.SelectedCostCenterEntityItem = function (id) {
        $scope.voucherDetail.EntityName = $('#costcenterentityId option:selected').text();
        $scope.voucherDetail.EntityId = id;
    };

    $scope.GetAdditionalexchangerate = function (item) {
        $http({
            method: 'get',
            url: 'accounts/voucher/GetAdditionalexchangerateById?voucherId=' + item,
        }).then(function successCallback(response) {
            $scope.additionalexchangerate = response.data.Rows[0];
        });
    };

    $scope.GetCurrencyExchangeRateList = function () {
        if ($scope.voucher.CurrencyId != null) {
            if ($scope.voucher.PostingDate != '') {
                $http({
                    method: 'GET',
                    url: 'currencies/ExchangeRate/ParallelExchangeRate?fromdate=' + $scope.voucher.PostingDate + '&&currencyId=' + $scope.voucher.CurrencyId,
                }).then(function successCallback(response) {
                    $scope.voucherDetailCurrency = response.data;
                    excurrencyRate($scope.voucherDetailCurrency);
                });
            }
            else
                $scope.pop('error', 'PostingDate is Null !! Please select PostigDate !');
        }
    };
    $scope.GetExceptCurrencyExchangeRateList = function () {
        if ($scope.voucher.CurrencyId != "") {
            $http({
                method: 'GET',
                url: 'currencies/CompanyParallelCurrency/ExceptParallelExchangeRate?fromdate=' + $scope.voucher.PostingDate + '&&currencyId=' + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.addiexchangerate = response.data;
                if ($scope.addiexchangerate.length > 0) {
                    $scope.additionalexchangerate.ToCurrencyRate = $scope.addiexchangerate[0].ToCurrencyRate;
                    excurrencyRate($scope.voucherDetailCurrency);
                    $scope.IscurrencyheaderHideShow = true;
                    $scope.IscurrencyHideShow = true;
                }
                else
                    $scope.IscurrencyheaderHideShow = true;
                $scope.IscurrencyHideShow = true;
            });
        }
    };
    $scope.glList = [];
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
        searchBy: 'GLItem',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetGLList = function (index) {
        $scope.AlternativeCoaList = [];
        $scope.rowSelectedIndex = index;
        $scope.GLUrl = 'accounts/glitem/getvendorinvoicegllist';
        $scope.GetGLListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.glList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#GLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetGLListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector('#GLListPopUp')).modal('hide');
    };

    $scope.closeVendorListPopUP = function () {
        angular.element(document.querySelector('#vendorListPopUp')).modal('hide');
    };

    $scope.closeGlCodeByPartyListPopUp = function () {
        angular.element(document.querySelector('#alternativeGLByPartyPopUp')).modal('hide');
    };

    $scope.closeGlCodeByPartySplitListPopUp = function () {
        angular.element(document.querySelector('#GlCodeByPartyListForSplitPopUp')).modal('hide');
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#GLListPopUp')).modal('hide');
        } else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };

    $scope.closeCrCodeByPartyListPopUpSelected = function () {
        if ($scope.crRowSelected != null) {
            angular.element(document.querySelector('#vendorListPopUp')).modal('hide');
        } else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };

    $scope.closeGlCodeByPartyListPopUpSelected = function () {
        if ($scope.crRowSelected != null) {
            angular.element(document.querySelector('#alternativeGLByPartyPopUp')).modal('hide');
        } else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };

    $scope.closeGlCodeByPartyListForSplitPopUpSelected = function () {
        if ($scope.crRowSelected != null) {
            angular.element(document.querySelector('#GlCodeByPartyListForSplitPopUp')).modal('hide');
        } else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };
    // #endregion

    $scope.removeRow = function () {
        angular.element(document.querySelector('#GLListPopUp')).modal('hide');
    };

    $scope.set = function () {
        if ($scope.selectedCode != null) {
            $scope.selectedCode = null;
        }
        console.log($scope.selectedCode)
    };

    $scope.rowSelected = null;
    $scope.setSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.voucherDetail.COAICode = x.GLGeneralInfoCode;
        //$scope.set();
        $scope.selectedCode = x.GLGeneralInfoCode;
        $scope.voucherDetail.COAIText = x.GLGeneralInfoCode;
        $scope.voucherDetail.COAIText = x.GLItem;
        $scope.voucherDetail.GLTextAndCode = x.GLGeneralInfoCode + "-" + x.GLItem;
        $scope.voucherDetail.GLGeneralInfoId = x.GLGeneralInfoId;
        /* To check GL account type Expenses or not. If selected gl are Expenses type
       and amount will DR then budget year will optional otherwise compolsary*/
        if ($scope.companyConfig.IsCostCenterApplicable == true && x.AccountTypeId == 'Expense') {
            $scope.IsExpenses = true;
            $scope.IsProfitCostCenterdisable = true;
            $scope.IsProfitCenterdisable = false;
        }
        else {
            $scope.IsExpenses = false;
            if ($scope.detailentityList.length == 1) {
                $scope.voucherDetail.EntityId = $scope.detailentityList[0].Value;
                $scope.voucherDetail.EntityName = $scope.detailentityList[0].Text;
            }
            $scope.IsProfitCostCenterdisable = false;
            $scope.IsProfitCenterdisable = true;
        };
        /* To check GL account type Asset or not. If selected gl are asset type and
        amount will DR then budget year will optional otherwise compolsary*/
        if (x.AccountTypeId == 'Asset') {
            $scope.IsAssetBudgetYear = true;
        }
        else {
            $scope.IsAssetBudgetYear = false;
        }
        $scope.getBudgetCboByGL(x.GLGeneralInfoId);
        $scope.voucherDetail.PostingWithoutTaxAllow = x.PostingWithoutTaxAllow;
        $scope.voucherDetail.TaxCategory = x.TaxCategory;
        $scope.voucherDetail.VendorInvoiceTax = [];
        $scope.voucherDetail.taxCategoryStatus = $scope.CheckTaxcagoryAllow(x.TaxCategory, x.PostingWithoutTaxAllow);
    };

    $scope.CheckTaxcagoryAllow = function (taxcat, postingWTA) {
        if (taxcat == null || postingWTA == true)
            return true;
        else
            return false;
    };
    $scope.setGlCodeForSplitSelected = function (x) {
        $scope.voucherInvoiceSplit = x;
        $scope.glRowForSplitSelected = x.COAICode;
        $scope.voucherInvoiceSplit.COAIText = x.GLItem;
        $scope.voucherInvoiceSplit.COAICode = x.COAICode;
        $scope.voucherInvoiceSplit.GLTextAndCode = x.COAICode + ' - ' + x.GLItem;
        $scope.voucherInvoiceSplit.GLGeneralInfoId = x.GLGeneralInfoId;
        $scope.getSplitBudgetCboByGL(x.GLGeneralInfoId);
        $scope.voucherInvoiceSplit.DocRefNo = $scope.voucher.DocRefNo;
        $scope.voucherInvoiceSplit.Narration = $scope.voucher.Narration;
        $scope.voucherInvoiceSplit.DocDate = $filter("dateFiltering")(Date.now());
    };

    $scope.getVoucherTypeValueText = function () {
        $scope.VoucherTypeValueText = $('#vouchertypeId option:selected').text();
    };

    $scope.getCurrencyText = function () {
        $scope.CurrencyText = $('#currencyTransactionId option:selected').text();
    };

    $scope.CurrencyHideShow = function () {
        if ($scope.CurrencyParallel != null && $scope.CurrencyParallel.length == 1) {
            if ($scope.voucher.CurrencyId == $scope.CurrencyParallel[0].CurrencyId) {
                /*when one parallel currency and match with selected Base /company Currency
                .No need to  IscurrencyHideShow And IscurrencyheaderHideShow falg true.In this case Rate always 1*/
                $scope.IscurrencyHideShow = false;
                $scope.IscurrencyheaderHideShow = false;
            }
        }
        else {
            $scope.IscurrencyheaderHideShow = true;
        }
    };

    function selectAndParallelCurrency() {
        if ($scope.CurrencyParallel.length > 1) {
            if ($scope.voucher.CurrencyId == $scope.CurrencyParallel[0].CurrencyId) {
                return $scope.CurrencyParallel[0].CurrencyId;
            }
            else if ($scope.voucher.CurrencyId == $scope.CurrencyParallel[1].CurrencyId) {
                return $scope.CurrencyParallel[0].CurrencyId;
            }
            else {
                return $scope.voucher.CurrencyId;
            }
        }
        else if ($scope.CurrencyParallel.length > 2) {
            if ($scope.voucher.CurrencyId == $scope.CurrencyParallel[2].CurrencyId) {
                return $scope.CurrencyParallel[0].CurrencyId;
            }
            else {
                return $scope.voucher.CurrencyId;
            }
        }
        else {
            return $scope.voucher.CurrencyId;
        }
    };

    function getct(v) {
        if (baseService.isUndefinedOrNull(v)) {
            return 'CompanyCurrency';
        }
        else {
            //return v;
            return 'CompanyCurrency';
        }
    };
    $scope.baseCurrency = null;
    $scope.groupCurrency = null;
    $scope.hardCurrency = null;

    $scope.currencyrowSelected = null;
    function excurrencyRate(list) {
        $scope.currencyParameter($scope.voucherDetailCurrency);
        //for
        for (var i = 0; i < $scope.voucherDetailrow.length; i++) {
            addsinglerow($scope.voucherDetailrow[i]);
        };
        $scope.CalvendorInvoice();

        if ($scope.voucherDetailInvoiceSplitRow == null || $scope.voucherDetailInvoiceSplitRow.length < 1) {
            tabwiseinvoicerow($scope.VendorInvoiceDetail);
        }

        for (var i = 0; i < $scope.voucherDetailInvoiceSplitRow.length; i++) {
            $scope.voucherDetailInvoiceSplitRow[i].DrAmount = $scope.voucherDetailInvoiceSplitRow[i].Amount;
            $scope.voucherDetailInvoiceSplitRow[i].VoucherDetailId = $scope.vendorInvoiceDetailIdguid;
            tabwiseinvoiceSplitrow($scope.voucherDetailInvoiceSplitRow[i]);
        }
    };

    $scope.currencyParameter = function (list) {
        for (var i = 0; i < list.length; i++) {
            if (list.length == 1) {
                $scope.baseCurrency = list[0].ToCurrencyRate;
                $scope.groupCurrency = 0;
                $scope.hardCurrency = 0;
            }
            else if (list.length == 2) {
                $scope.baseCurrency = list[0].ToCurrencyRate;
                $scope.groupCurrency = list[1].ToCurrencyRate;
                $scope.hardCurrency = 0;
            }
            else {
                $scope.baseCurrency = list[0].ToCurrencyRate;
                $scope.groupCurrency = list[1].ToCurrencyRate;
                $scope.hardCurrency = list[2].ToCurrencyRate;
            }
        }
    };

    $scope.setCurrencySelected = function () {
        excurrencyRate($scope.voucherDetailCurrency);
        angular.element(document.querySelector('#currencyexchangeListPopUp')).modal('hide');
    };
    $scope.excurrencyRateMethod = function (exrateobj) {
        // $scope.voucherDetailCurrency = [];
        excurrencyRate(exrateobj);
    };

    $scope.ChartOfAccountRelationshipData = [];

    $scope.SelectedManagementGroup = null;
    $scope.SelectedManagementGroup = function (selected) {
        if (selected) {
            $scope.voucherDetail.GLGeneralInfoCode = selected.originalObject.Value;
            $scope.voucherDetail.COAICode = selected.originalObject.Value;
            $scope.voucherDetail.COAIText = selected.originalObject.Text;
        }
    };

    $scope.inputChanged = function (str) {
        $scope.voucherDetail.GLGeneralInfoCode = str;
    };

    //Gets data from the Database
    $scope.cOAICodeListt = null;
    $scope.getAccountCodeby = function (keyEvent, accountcode) {
        if (keyEvent.which === 13)
            $http({
                method: 'GET',
                url: 'accounts/glitem/getglbyaccountcode?accountcode=' + accountcode
            }).then(function (result) {
                $scope.cOAICodeListt = result.data;
                $scope.voucherDetail.COAICode = $scope.cOAICodeListt[0].Value;
                $scope.voucherDetail.COAIText = $scope.cOAICodeListt[0].Text;
            }, function () {
            })
    };
    $scope.onLevelChange = function () {
        $scope.coaValue = $('#coc option:selected').text();

        $scope.voucherDetail.COAIText = $scope.coaValue;
    };
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

    $scope.checkCrAmount = function () {
        if ($scope.voucherDetail.DrAmount > 0) {
            $scope.voucherDetail.CrAmount = 0;
        }
    };

    // #region ********GUID**********
    function guid() {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
                .toString(16)
                .substring(1);
        }
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
            s4() + '-' + s4() + s4() + s4();
    };
    // #endregion
    $scope.customerguid = function () {
        $scope.vendorInvoiceDetailIdguid = guid();
    };
    $scope.vendorList = [];
    $scope.searchvendorGLByList = [
        {
            'name': 'Vendor Code',
            'value': 'Code'
        },
        {
            'name': 'Vendor',
            'value': 'Party'
        },
        {
            'name': 'GL Code',
            'value': 'COAICode'
        },
        {
            'name': 'GL',
            'value': 'GLItem'
        },
        {
            'name': 'Currency',
            'value': 'CurrencyCode'
        }
    ];
    $scope.vendorGLParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: 'Party',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getVendorGL = function (index) {
        $scope.vendorIndex = index;
        $scope.vendorGLUrl = 'Parties/party/getinvoicevendordata';
        $scope.getvendorGLData = function (pageno) {
            baseService.paginationBase($scope.vendorGLUrl, pageno, $scope.vendorGLParameters)
                .then(function (result) {
                    $scope.vendorList = result.Rows;
                    $scope.vendorGLParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };

        angular.element(document.querySelector('#vendorListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getvendorGLData();
    };
    $scope.setvendorSelected = function (x) {
        $scope.crRowSelected = x.PartyId;
        $scope.getVendorDetails = x;
        $scope.customerNameCode = $scope.getVendorDetails.Code + " - " + $scope.getVendorDetails.Party;
        $scope.GLNameCode = $scope.getVendorDetails.COAICode + " - " + $scope.getVendorDetails.GLItem;
        $scope.voucher.CurrencyId = $scope.getVendorDetails.CurrencyId;
        $scope.showCrlOnCurrency(voucher.CurrencyId);
        $scope.updateDiable = false;
        $scope.paymentTerms.PaymentTermId = x.PaymentTermId;
        $scope.getBudgetId = x.BudgetId;
        $scope.getActivityId = x.ActivityId;
        $scope.getBudgetNamecode = x.BudgetCode + " - " + x.BudgetName;
        $scope.getActivityNamecode = x.ActivityCode + " - " + x.ActivityName;
        $scope.onPaymnetChange(x.PaymentTermId);
        $scope.currencyTableshowhide = false;
        $scope.invoiceDetailCurrencyrow = [];
        $scope.customerguid();
    }

    $scope.vendorSplitGLByList = [
        {
            'name': 'AccountType',
            'value': 'AccountType'
        },
        {
            'name': 'GL Code',
            'value': 'COAICode'
        },
        {
            'name': 'GL',
            'value': 'GLItem'
        },
        {
            'name': 'Party',
            'value': 'Party'
        },
    ];
    $scope.customerSplitGLParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: 'GLItem',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getGlByPartyForSplit = function () {
        $scope.alternativeSplitGLData = function (pageno) {
            $rootScope.parameters.partyid = $scope.getVendorDetails.PartyId;
            $scope.alternativeSplitGLGLUrl = 'Parties/party/GetCompanyPartyReconAdditionalGLList?partyid=' + $scope.getVendorDetails.PartyId + '&partyType=Vendor';
            baseService.paginationBase($scope.alternativeSplitGLGLUrl, pageno, $scope.customerSplitGLParameters)
                .then(function (result) {
                    $scope.vendorSplitGLList = result.Rows;
                    $scope.customerSplitGLParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#GlCodeByPartyListForSplitPopUp')).modal('show');
        $scope.alternativeSplitGLData();
    }

    // #endregion
    // #region *************Alternative GL by party**********
    $scope.vendoralternativeGLList = [];
    $scope.VendoralternativeGLByParty = [
        {
            'name': 'Vendor',
            'value': 'Party'
        },
        {
            'name': 'Vendor Code',
            'value': 'Code'
        },
        {
            'name': 'GL Code',
            'value': 'COAICode'
        },
        {
            'name': 'GL',
            'value': 'GLItem'
        }
    ];
    $scope.vendorAlternativeGLParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: 'Party',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getVendorGLByParty = function () {
        $scope.vendoralternativeGLData = function (pageno) {
            $rootScope.parameters.partyid = $scope.getVendorDetails.PartyId;
            $scope.VendorAlternativeGLUrl = 'Parties/party/GetCompanyPartyReconAdditionalGLList?partyid=' + $scope.getVendorDetails.PartyId + '&partyType=Vendor';
            baseService.paginationBase($scope.VendorAlternativeGLUrl, pageno, $scope.vendorAlternativeGLParameters)
                .then(function (result) {
                    $scope.vendoralternativeGLList = result.Rows;
                    $scope.vendorAlternativeGLParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#alternativeGLByPartyPopUp')).modal('show');
        $scope.vendoralternativeGLData();
    }
    $scope.setGlCodeSelected = function (x) {
        $scope.glRowSelected = x.COAICode;
        $scope.getVendorDetails = x;
        $scope.getVendorDetails.PartyId = x.Id;
        $scope.customerNameCode = $scope.getVendorDetails.Code + " - " + $scope.getVendorDetails.PartyName;
        $scope.GLNameCode = $scope.getVendorDetails.COAICode + " - " + $scope.getVendorDetails.GLItem;
        $scope.invoiceDetailCurrencyrow = [];
    };
    // #endregion

    function addUpdateDetailRow(obj, rate, list) {//obj==$scope.voucherDetail
        try {
            if (Isavailble(obj, list) == false) {
                obj.Id = guid();
                // $scope.additionalexchangerate.ToCurrencyRate;
                obj.Rate = rate;
                obj.Active = true;
                var local = {};
                for (var i in obj) {
                    local[i] = obj[i];
                }
                list.push(
                    local
                );
            }
            else {
                Update(obj, list);
            }
        } catch (e) {
            throw e;
        }
    };

    function Update(obj, list) {
        try {
            for (var i = 0; i < list.length; i++) {
                if (list[i].Id == obj.Id) {
                    list[i] = obj;
                    break;
                }
            }
        } catch (e) {
            throw e;
        }
    };
    function Isavailble(obj, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id == obj.Id) {
                return true;
                //list[i] = obj;
            }
        }
        return false;
    };

    function SetBaseValue(objDetail, exchangeratelist, paracurrency, selectedCurrency, outobj) {
        var drcr = { amount: null, rate: null, fromcurrencyid: null, tocurrencyid: null, ParallelCurrencyType: null, orate: null, comAmount: null };
        for (var i = 0; i < exchangeratelist.length; i++) {
            if (exchangeratelist[i].ParallelCurrencyType == 'CompanyCurrency' && paracurrency.CurrencyId == exchangeratelist[i].ParallelCurrencyId) {
                drcr.rate = exchangeratelist[i].ToCurrencyRate;
                drcr.fromcurrencyid = exchangeratelist[i].FromCurrencyId;
                drcr.tocurrencyid = exchangeratelist[i].ToCurrencyId;
                drcr.ParallelCurrencyType = exchangeratelist[i].ParallelCurrencyType;
                if (exchangeratelist[i].FromCurrencyId == selectedCurrency) {
                    drcr.orate = drcr.rate;
                    outobj.ComCurRate = drcr.orate;
                }
                else {
                    drcr.orate = 1 / drcr.rate;
                    outobj.ComCurRate = drcr.orate;
                }
            }
            else if (exchangeratelist[i].ParallelCurrencyType == 'CompanyGroupCurrency' && paracurrency.CurrencyId == exchangeratelist[i].ParallelCurrencyId) {
                drcr.rate = exchangeratelist[i].ToCurrencyRate;
                drcr.fromcurrencyid = exchangeratelist[i].FromCurrencyId;
                drcr.tocurrencyid = exchangeratelist[i].ToCurrencyId;
                drcr.ParallelCurrencyType = exchangeratelist[i].ParallelCurrencyType;
                if (exchangeratelist[i].ParallelCurrencyId == selectedCurrency) {
                    drcr.orate = 1 / drcr.rate;
                }
                else {
                    drcr.orate = outobj.ComCurRate / drcr.rate;
                }
            }
            else if (exchangeratelist[i].ParallelCurrencyType == 'HardCurrency' && paracurrency.CurrencyId == exchangeratelist[i].ParallelCurrencyId) {
                drcr.rate = exchangeratelist[i].ToCurrencyRate;
                drcr.fromcurrencyid = exchangeratelist[i].FromCurrencyId;
                drcr.tocurrencyid = exchangeratelist[i].ToCurrencyId;
                drcr.ParallelCurrencyType = exchangeratelist[i].ParallelCurrencyType;
                if (exchangeratelist[i].ParallelCurrencyId == selectedCurrency) {
                    drcr.orate = 1 / drcr.rate;
                }
                else {
                    drcr.orate = outobj.ComCurRate / drcr.rate;
                }
            }
        }
        //outobj.dr = isNaN((drcr.orate * objDetail.DrAmount).toFixed(4)) ? 0 : (orate * objDetail.DrAmount).toFixed(4);
        //outobj.cr = isNaN((drcr.orate * objDetail.CrAmount).toFixed(4)) ? 0 : (orate * objDetail.CrAmount).toFixed(4);
        outobj.dr = parseFloat((drcr.orate * objDetail.DrAmount).toFixed(4));
        outobj.cr = parseFloat((drcr.orate * objDetail.CrAmount).toFixed(4));
        if (outobj.dr > 0) {
            outobj.crAmountDisable = true;
        }
        else
            outobj.drAmountDisable = true;
        outobj.tocurrencyid = drcr.tocurrencyid;
        outobj.parallelcurrencyid = paracurrency.CurrencyId;
        outobj.ParallelCurrencyType = drcr.ParallelCurrencyType;
        outobj.fromcurrencyid = drcr.fromcurrencyid;
        outobj.currencyrate = drcr.rate;
    };

    function ClearObj(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
    };

    $scope.addRow = function () {
        try {
            addsinglerow($scope.voucherDetail);
        } catch (e) {
        }
    };

    // obj==$scope.voucherDetail
    function addsinglerow(obj) {
        try {
            uniqueGLCheck(obj, $scope.voucherDetailrow);/*check GL unique base on budget,activity,costcenter , entity , budget year and period.*/
            validationCurrencyRate();
            validationAddGL(obj);//obj==$scope.voucherDetail
            $scope.disableGL = false;
            $scope.updateDiable = false;
            addUpdateDetailRow(obj, $scope.additionalexchangerate.ToCurrencyRate, $scope.voucherDetailrow);//obj==$scope.voucherDetail
            tabwiserow(obj);
        } catch (e) {
            throw e;
        }
        //ClearObj(obj);
    }

    function validationAddGL(obj) {
        try {
            obj.FiscalYearText = $('#FiscalYear option:selected').text();
            obj.FiscalYearPeriodText = $('#FiscalYearPeriod option:selected').text();
            if ($scope.voucher.PostingDate == '') {
                throw "PostingDate is Null !! Please select PostigDate !";
            }
            if (baseService.isUndefinedOrNull(obj.COAICode)) {
                throw 'Please Select GL!!';
            }
            if ($scope.voucher.Narration == '' || $scope.voucher.Narration == null) {
                throw 'Please input Narration!!'
            }
            if (obj.Narration == '' || obj.Narration == null) {
                throw 'Please input Detail Narration!!'
            }
            if ($scope.voucher.DocRefNo == '' || $scope.voucher.DocRefNo == null) {
                throw 'Please input DocRefNo!!'
            }
            if (obj.DrAmount == 0 || obj.DrAmount == null) {
                throw 'Please Input  Amount !!';
            }
            if ($scope.companyConfig.IsVoucherFromBudget) {
                if (obj.BudgetId == "" || obj.BudgetId == null) {
                    throw "Please Select Budget !!";
                }
            }
            //if ($scope.IsExpenses == true) {
            //    if ($scope.voucherDetail.CostCenterId == "" || $scope.voucherDetail.CostCenterId == null) {
            //        throw "Please Select Cost Center !!";
            //    }
            //}
            if ($scope.companyConfig.IsCostCenterApplicable) {/*Cost Center is applicable when in company IsCostCenterApplicable == true*/
                if ($scope.IsExpenses == true) {/*Applicable for Only Expenses GL DR amount and CR amount*/
                    if (obj.CostCenterId == "" || obj.CostCenterId == null) {
                        throw "Please Select Cost Center !!";
                    }
                }
            }
            if ($scope.companyConfig.IsProfitCenterApplicable) {/*Entity is applicable when in company IsProfitCenterApplicable == true*/
                if ($scope.vendorInvoice.EntityId == "" || $scope.vendorInvoice.EntityId == null) {
                    throw "Please  Select Vendor Entity !!";
                }
                if (obj.EntityId == "" || obj.EntityId == null) {
                    throw "Please Select Entity !!";
                }
            }
            if ($scope.companyConfig.IsBudgetPeriod) {/*Budget is applicable when in company IsBudgetPeriod == true*/
                if (obj.FiscalYearId == "" || obj.FiscalYearId == null && checkbudgetrequired()) {//&& obj.DrAmount != 0
                    throw "Please Select Budget Year !!";
                }
                if (obj.FiscalYearPeriodId == "" || obj.FiscalYearPeriodId == null && checkbudgetrequired()) {//&& obj.DrAmount != 0
                    throw "Please Select Budget Year Period !!";
                }
                if (obj.PeriodName == "" || obj.PeriodName == null && checkbudgetrequired()) {//&& obj.DrAmount != 0
                    throw "This  Budget Year Period is Locked!!";
                }
            }
        } catch (e) {
            throw ShowResult(e, 'failure');
        }
    };

    function validationAddSplitGL(obj) {
        try {
            if ($scope.vendorInvoice.Amount == 0 || $scope.vendorInvoice.Amount == undefined || $scope.vendorInvoice.Amount == "") {
                throw 'Vendor Amount 0 And null is not allow!!';
            }
            if (baseService.isUndefinedOrNull(obj.GLTextAndCode)) {
                throw 'Please Select Split GL!!';
            }
            if (obj.Narration == '' || obj.Narration == null) {
                throw 'Please input Split Narration!!'
            }
            if (obj.DocRefNo == '' || obj.DocRefNo == null) {
                throw 'Please input Split DocRefNo!!'
            }
            if (obj.Amount == 0 || obj.Amount == null) {
                throw 'Please Input Split Amount !!';
            }
            if ((parseFloat(obj.Amount) + parseFloat($scope.SplitAmount)) > $scope.vendorInvoice.Amount) {
                throw 'Split Amount can not more than vendor Amount !!';
            }
            //if ($scope.companyConfig.IsVoucherFromBudget) {
            //    if (obj.BudgetId == "" || obj.BudgetId == null) {
            //        throw "Please Select Budget !!";
            //    }
            //}
        } catch (e) {
            throw ShowResult(e, 'failure');
        }
    };

    function validationCurrencyRate() {
        try {
            if ($scope.IscurrencyHideShow) {
                if ($scope.additionalexchangerate.ToCurrencyRate == 0 || $scope.additionalexchangerate.ToCurrencyRate == ""
                    || $scope.additionalexchangerate.ToCurrencyRate == undefined) {
                    $scope.RateAmountDisable = true;
                    $scope.vendorInvoice.Amount = 0;
                    throw "Please input " + $scope.additionalexchangerate.FromCurrencyCode + " Currency rate!!";
                    //manualValidation('div_currency', $scope.RateAmountDisable, 'Please input ' + $scope.additionalexchangerate.FromCurrencyCode + ' Currency rate!!')
                }
                else {
                    $scope.RateAmountDisable = false;
                }
                if ($scope.voucherDetailCurrency.length > 0) {
                    for (var i = 0; i < $scope.voucherDetailCurrency.length; i++) {
                        if ($scope.voucherDetailCurrency[i].ToCurrencyRate == 0 || $scope.voucherDetailCurrency[i].ToCurrencyRate == null) {
                            $scope.RateAmountDisable = true;
                            $scope.vendorInvoice.Amount = 0;
                            throw "Please input " + $scope.voucherDetailCurrency[i].FromCurrencyCode + " Currency rate !!";
                        }
                        else {
                            $scope.RateAmountDisable = false;
                        }
                    }
                }
            }
        } catch (e) {
            throw ShowResult(e, 'failure');
        }
    }
    function checkbudgetrequired() {
        if ($scope.IsAssetBudgetYear == true) {
            return true
        }
        else if ($scope.IsExpenseBudgetYear == true) {
            return true
        }
        else {
            return false;
        }
    };
    function uniqueGLCheck(obj, rowlist) {/*Same GL combination check from GLGeneralInfoId to fiscalyearperiod*/
        try {
            for (var i = 0; i < rowlist.length; i++) {
                if (obj.$$hashKey == undefined) {//when add new row
                    if (rowlist[i].GLGeneralInfoId == obj.GLGeneralInfoId && rowlist[i].BudgetId == obj.BudgetId
                        && rowlist[i].ActivityId == obj.ActivityId && rowlist[i].CostCenterId == obj.CostCenterId
                        && rowlist[i].FiscalYearId == obj.FiscalYearId && rowlist[i].FiscalYearPeriodId == obj.FiscalYearPeriodId
                        && obj.$$hashKey == undefined) {
                        throw "Same GL combination already selected !!";
                    }
                }
                else
                    if (obj.Id != rowlist[i].Id) {/*Same GL combination check all row from array except selected row*/
                        if (rowlist[i].GLGeneralInfoId == obj.GLGeneralInfoId && rowlist[i].BudgetId == obj.BudgetId
                            && rowlist[i].ActivityId == obj.ActivityId && rowlist[i].CostCenterId == obj.CostCenterId
                            && rowlist[i].FiscalYearId == obj.FiscalYearId && rowlist[i].FiscalYearPeriodId == obj.FiscalYearPeriodId
                        ) {
                            throw "Same GL combination already selected !!";
                        }
                    }
                    else if (obj.Id == rowlist[i].Id) {/*Same GL combination check only selected row*/
                        if (rowlist[i].GLGeneralInfoId == obj.GLGeneralInfoId && rowlist[i].BudgetId == obj.BudgetId
                            && rowlist[i].ActivityId == obj.ActivityId && rowlist[i].CostCenterId == obj.CostCenterId
                            && rowlist[i].FiscalYearId == obj.FiscalYearId && rowlist[i].FiscalYearPeriodId == obj.FiscalYearPeriodId
                        ) {
                        }
                    }
            }
        }
        catch (e) {
            throw ShowResult(e, 'failure');
        }
    };
    function uniqueSplitGLCheck(obj, rowlist) {/*Same GL combination check from GLGeneralInfoId to Activity*/
        try {
            for (var i = 0; i < rowlist.length; i++) {
                if (obj.$$hashKey == undefined) {//when add new row
                    if (rowlist[i].GLGeneralInfoId == obj.GLGeneralInfoId && rowlist[i].BudgetId == obj.BudgetId
                        && rowlist[i].ActivityId == obj.ActivityId
                        && obj.$$hashKey == undefined) {
                        throw "Same GL combination already selected !!";
                    }
                }
                else
                    if (obj.Id != rowlist[i].Id) {/*Same GL combination check all row from array except selected row*/
                        if (rowlist[i].GLGeneralInfoId == obj.GLGeneralInfoId && rowlist[i].BudgetId == obj.BudgetId
                            && rowlist[i].ActivityId == obj.ActivityId

                        ) {
                            throw "Same GL combination already selected !!";
                        }
                    }
                    else if (obj.Id == rowlist[i].Id) {/*Same GL combination check only selected row*/
                        if (rowlist[i].GLGeneralInfoId == obj.GLGeneralInfoId && rowlist[i].BudgetId == obj.BudgetId
                            && rowlist[i].ActivityId == obj.ActivityId
                        ) {
                        }
                    }
            }
        }
        catch (e) {
            throw ShowResult(e, 'failure');
        }
    };
    function tabwiserow(obj) {
        var tabrow = {
            Id: null,
            dr: null,
            cr: null,
            currencyrate: null,
            tocurrencyid: null,
            fromcurrencyid: null,
            parallelcurrencyid: null,
            ParallelCurrencyType: null, ComCurRate: null
        };
        var baseDrTotalAmount = 0;
        var baseCrTotalAmount = 0;
        var currencyrate = 0;
        var fromcurrencyid = null;
        var tocurrencyid = null;
        var parallelcurrencyid = null;
        var vdindex = 0;
        var DrCr = { dr: null, cr: null };

        for (var i = 0; i < $scope.CurrencyParallel.length; i++) {
            // #region CheckCurrencyExceptBase False
            //PC==FC || PC==TC ==amount,rate
            SetBaseValue(obj, $scope.voucherDetailCurrency, $scope.CurrencyParallel[i], $scope.voucher.CurrencyId, tabrow);
            // #region ************voucherDetailCurrencyrow push**************
            if (IsavailbleTab(obj.COAICode, tabrow.ParallelCurrencyType, $scope.voucherDetailCurrencyrow) == false) {
                $scope.voucherDetailCurrencyrow.push(
                    {
                        Id: guid(),
                        VoucherDetailId: obj.Id,
                        COAICode: obj.COAICode,
                        COAIText: obj.COAIText,
                        GLGeneralInfoId: obj.GLGeneralInfoId,
                        DocRefNo: obj.DocRefNo,
                        DocDate: obj.DocDate,
                        FiscalYear: obj.FiscalYear,
                        RefCode: obj.RefCode,
                        ToCurrencyRate: tabrow.currencyrate,
                        FromCurrencyRate: 1,//
                        Narration: obj.Narration,
                        FromCurrencyId: tabrow.fromcurrencyid,
                        ToCurrencyId: tabrow.tocurrencyid,
                        VoucherTypeId: obj.VoucherTypeId,
                        ParallelCurrencyType: tabrow.ParallelCurrencyType,
                        DrAmount: tabrow.dr,
                        CrAmount: 0,
                        Active: true,
                        ParallelCurrencyId: tabrow.parallelcurrencyid
                    })//push
            }
            else {
                var localupdate = {
                    Id: updatevoucercurrencyrow($scope.voucherDetailCurrencyrowupdate, obj.COAICode, obj.GLGeneralInfoId, tabrow.ParallelCurrencyType),
                    VoucherDetailId: obj.Id,
                    COAICode: obj.COAICode,
                    COAIText: obj.COAIText,
                    GLGeneralInfoId: obj.GLGeneralInfoId,
                    DocRefNo: obj.DocRefNo,
                    DocDate: obj.DocDate,
                    FiscalYear: obj.FiscalYear,
                    RefCode: obj.RefCode,
                    ToCurrencyRate: tabrow.currencyrate,
                    FromCurrencyRate: 1,//
                    Narration: obj.Narration,
                    FromCurrencyId: tabrow.fromcurrencyid,
                    ToCurrencyId: tabrow.tocurrencyid,
                    VoucherTypeId: obj.VoucherTypeId,
                    ParallelCurrencyType: tabrow.ParallelCurrencyType,
                    DrAmount: tabrow.dr,
                    CrAmount: 0,
                    ParallelCurrencyId: tabrow.parallelcurrencyid,
                    Active: true
                }
                UpdateTab(localupdate, $scope.voucherDetailCurrencyrow);
            }
        }//for
        function UpdateTab(obj, list) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].COAICode == obj.COAICode && list[i].ParallelCurrencyType == obj.ParallelCurrencyType && obj.VoucherDetailId == list[i].VoucherDetailId) {
                    list[i] = obj;
                    break;
                }
            }
        };

        function IsavailbleTab(COAICode, ParallelCurrencyType, list) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].COAICode == COAICode && list[i].ParallelCurrencyType == ParallelCurrencyType && obj.Id == list[i].VoucherDetailId) {
                    return true;
                }
            }
            return false;
        };
        function updatevoucercurrencyrow(list, coacode, gl, currencyType) {
            var id = null;
            if (!baseService.isUndefinedOrNull(list)) {
                for (var i = 0; i < list.length; i++) {
                    if (list[i].COAICode == coacode && list[i].ParallelCurrencyType == currencyType) {//&& list[i].GLGeneralInfoId == gl
                        if (list[i].VoucherDetailId.length == 36) {
                            id = guid();
                            return id;
                        }
                        else {
                            id = list[i].Id;
                            return id;
                        }
                    }
                }
            }
            else {
                id = guid();
                return id;
            }
        };
        $scope.clearDetailRow();
        $scope.total();
        $scope.totalAmountCheck();
        $scope.BaseCurrencytotal();

        if ($scope.indexdetails != -1 && $scope.CAction == 'Update') {
            //$scope.voucherDetailrow[$scope.indexdetails] = $scope.voucherDetail;
            $scope.indexdetails = -1;
            $scope.CAction = 'Add';
            $scope.total();
            $scope.totalAmountCheck();
            $scope.clearDetailRow();
        }
    }
    $scope.CalvendorInvoice = function () {
        validationCurrencyRate();
        $scope.VendorInvoiceDetail.CrAmount = $scope.vendorInvoice.Amount;
        $scope.VendorInvoiceDetail.DocRefNo = $scope.voucher.DocRefNo;
        $scope.VendorInvoiceDetail.DrAmount = 0;
        $scope.VendorInvoiceDetail.Narration = $scope.voucher.Narration;
        $scope.VendorInvoiceDetail.GLGeneralInfoId = $scope.getVendorDetails.GLGeneralInfoId;
        $scope.VendorInvoiceDetail.COAIText = $scope.getVendorDetails.GLItem;
        $scope.VendorInvoiceDetail.COAICode = $scope.getVendorDetails.COAICode;
    };

    $scope.addInvoiceRow = function () {
        try {
            if ($scope.clickCount === 0) {
                tabwiseinvoicerow($scope.VendorInvoiceDetail);
            }
        } catch (e) {
        }
    };

    function tabwiseinvoicerow(obj) {
        var tabrow = {
            Id: null, dr: null, cr: null, currencyrate: null, tocurrencyid: null, fromcurrencyid: null, parallelcurrencyid: null,
            ParallelCurrencyType: null, ComCurRate: null
        };
        var baseDrTotalAmount = 0, baseCrTotalAmount = 0, currencyrate = 0, fromcurrencyid = null, tocurrencyid = null, parallelcurrencyid = null;
        var vdindex = 0;
        var DrCr = { dr: null, cr: null };
        for (var i = 0; i < $scope.CurrencyParallel.length; i++) {
            SetBaseValue(obj, $scope.voucherDetailCurrency, $scope.CurrencyParallel[i], $scope.voucher.CurrencyId, tabrow);
            if ((parseFloat(tabrow.dr) + parseFloat(tabrow.cr)) > 0) {
                // #region ************voucherDetailCurrencyrow push**************
                if (IsavailbleTab(obj.COAICode, tabrow.ParallelCurrencyType, $scope.invoiceDetailCurrencyrow) == false) {
                    $scope.invoiceDetailCurrencyrow.push(
                        {
                            Id: guid(),
                            VoucherDetailId: $scope.vendorInvoiceDetailIdguid,
                            TempId: $scope.vendorInvoiceDetailIdguid,
                            COAICode: obj.COAICode,
                            COAIText: obj.COAIText,
                            GLGeneralInfoId: obj.GLGeneralInfoId,
                            DocRefNo: obj.DocRefNo,
                            DocDate: obj.DocDate,
                            FiscalYear: obj.FiscalYear,
                            RefCode: obj.RefCode,
                            ToCurrencyRate: tabrow.currencyrate,
                            FromCurrencyRate: 1,//
                            Narration: obj.Narration,
                            FromCurrencyId: tabrow.fromcurrencyid,
                            ToCurrencyId: tabrow.tocurrencyid,
                            VoucherTypeId: obj.VoucherTypeId,
                            ParallelCurrencyType: tabrow.ParallelCurrencyType,
                            DrAmount: 0,
                            CrAmount: tabrow.cr,
                            Active: true,
                            ParallelCurrencyId: tabrow.parallelcurrencyid
                        })//push
                    console.log('invoiceDetailCurrencyrow', $scope.invoiceDetailCurrencyrow);
                }
                else {
                    var localupdate = {
                        Id: updatevoucercurrencyrow($scope.voucherDetailCurrencyrowupdate, obj.COAICode, obj.GLGeneralInfoId, tabrow.ParallelCurrencyType),
                        VoucherDetailId: $scope.vendorInvoiceDetailIdguid,
                        TempId: $scope.vendorInvoiceDetailIdguid,
                        COAICode: obj.COAICode,
                        COAIText: obj.COAIText,
                        GLGeneralInfoId: obj.GLGeneralInfoId,
                        DocRefNo: obj.DocRefNo,
                        DocDate: obj.DocDate,
                        FiscalYear: obj.FiscalYear,
                        RefCode: obj.RefCode,
                        ToCurrencyRate: tabrow.currencyrate,
                        FromCurrencyRate: 1,//
                        Narration: obj.Narration,
                        FromCurrencyId: tabrow.fromcurrencyid,
                        ToCurrencyId: tabrow.tocurrencyid,
                        VoucherTypeId: obj.VoucherTypeId,
                        ParallelCurrencyType: tabrow.ParallelCurrencyType,
                        DrAmount: 0,
                        CrAmount: tabrow.cr,
                        ParallelCurrencyId: tabrow.parallelcurrencyid,
                        Active: true
                    }
                    UpdateTab(localupdate, $scope.invoiceDetailCurrencyrow);
                }
            }
            // #endregion
        }//for
        function UpdateTab(obj, list) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].COAICode == obj.COAICode && list[i].ParallelCurrencyType == obj.ParallelCurrencyType) {
                    list[i] = obj;
                    break;
                }
            }
        };
        function IsavailbleTab(COAICode, ParallelCurrencyType, list) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].COAICode == COAICode && list[i].ParallelCurrencyType == ParallelCurrencyType) {
                    return true;
                }
            }
            return false;
        };
        function updatevoucercurrencyrow(list, coacode, gl, currencyType) {
            var id = null;
            if (!baseService.isUndefinedOrNull(list)) {
                for (var i = 0; i < list.length; i++) {
                    if (list[i].COAICode == coacode && list[i].ParallelCurrencyType == currencyType) {//&& list[i].GLGeneralInfoId == gl
                        if (list[i].VoucherDetailId.length == 36) {
                            id = guid();
                            return id;
                        }
                        else {
                            id = list[i].Id;
                            return id;
                        }
                    }
                }
            }
            else {
                id = guid();
                return id;
            }
        };
        $scope.clearDetailRow();
        $scope.total();
        $scope.totalAmountCheck();
        $scope.BaseCurrencytotal();
        if ($scope.indexdetails != -1 && $scope.CAction == 'Update') {
            //$scope.voucherDetailrow[$scope.indexdetails] = $scope.voucherDetail;
            $scope.indexdetails = -1;
            $scope.CAction = 'Add';
            $scope.total();
            $scope.BaseCurrencytotal();
            $scope.totalAmountCheck();
            $scope.clearDetailRow();
        }
    };
    // #region ****BaseCurrencytotal******
    $scope.BaseCurrencytotal = function () {
        $scope.Drtbase = 0;
        $scope.Crtbase = 0;
        $scope.Drtgroup = 0;
        $scope.Crtgroup = 0;
        $scope.Drthard = 0;
        $scope.Crthard = 0;
        angular.forEach($scope.voucherDetailCurrencyrow, function (item) {
            if (item.ParallelCurrencyType == 'CompanyCurrency') {
                $scope.Drtbase = parseFloat($scope.Drtbase) + parseFloat(item.DrAmount);
                $scope.Crtbase = parseFloat($scope.Crtbase) + parseFloat(item.CrAmount);
            }
            else if (item.ParallelCurrencyType == 'CompanyGroupCurrency') {
                $scope.Drtgroup = parseFloat($scope.Drtgroup) + parseFloat(item.DrAmount);
                $scope.Crtgroup = parseFloat($scope.Crtgroup) + parseFloat(item.CrAmount);
            }
            else if (item.ParallelCurrencyType == 'HardCurrency') {
                $scope.Drthard = parseFloat($scope.Drthard) + parseFloat(item.DrAmount);
                $scope.Crthard = parseFloat($scope.Crthard) + parseFloat(item.CrAmount);
            }
        });
        angular.forEach($scope.invoiceDetailCurrencyrow, function (item) {
            if (item.ParallelCurrencyType == 'CompanyCurrency') {
                $scope.Drtbase = parseFloat($scope.Drtbase) + parseFloat(item.DrAmount);
                $scope.Crtbase = parseFloat($scope.Crtbase) + parseFloat(item.CrAmount);
            }
            else if (item.ParallelCurrencyType == 'CompanyGroupCurrency') {
                $scope.Drtgroup = parseFloat($scope.Drtgroup) + parseFloat(item.DrAmount);
                $scope.Crtgroup = parseFloat($scope.Crtgroup) + parseFloat(item.CrAmount);
            }
            else if (item.ParallelCurrencyType == 'HardCurrency') {
                $scope.Drthard = parseFloat($scope.Drthard) + parseFloat(item.DrAmount);
                $scope.Crthard = parseFloat($scope.Crthard) + parseFloat(item.CrAmount);
            }
        });
    };

    $scope.removeVoucherRow = function () {
        if ($rootScope.VoucherId != null)
            $scope.dDetailId.push($rootScope.VoucherId)
        $scope.voucherDetailrow.splice($rootScope.VoucherIndex, 1);
        $rootScope.VoucherId = null;
        $scope.total();
        $scope.totalAmountCheck();
    };
    $scope.GetVoucherDetailrow = function (x, id, index) {
        $scope.indexdetails = index;
        validationCurrencyRate();
        $scope.onFiscalYearChange(x.FiscalYearId);
        if ($scope.companyConfig.IsCostCenterApplicable == true && x.AccountTypeId == 'Expense') {
            $scope.IsExpenses = true;
            /*if ($scope.companyConfig.IsProfitCenterApplicable) {if Profit Center Applicable and Expenses then Cost Center Entity will enable*/
            $scope.IsProfitCostCenterdisable = true;
            $scope.IsProfitCenterdisable = false;
            //}
        }
        else {
            $scope.IsExpenses = false;
        }
        //if ($scope.companyConfig.IsProfitCenterApplicable) {
        if ($scope.IsExpenses == true) {
            $scope.IsProfitCostCenterdisable = true;
            $scope.IsProfitCenterdisable = false;
        }
        else {
            $scope.IsProfitCostCenterdisable = false;
            $scope.IsProfitCenterdisable = true;
        }
        //}
        $scope.disableGL = true;
        $scope.updateDiable = true;
        $scope.voucherDetail = $scope.voucherDetailrow[$scope.indexdetails];
        $scope.CAction = 'Update';
    };

    $scope.clearDetailRow = function () {
        $scope.voucherDetail = { DrAmount: 0, CrAmount: 0, DocRefNo: $scope.voucherDetail.DocRefNo, DocDate: $scope.voucherDetail.DocDate };
        $scope.voucherDetail.GLTextAndCode = '';
        $scope.voucherDetail.COAIText = '';
    }
    // #region ***VendorInvoice Split Add Row***
    //$scope.splitrow = function () {
    //    $scope.InvoiceSplit.COAICode = $scope.$scope.voucherInvoiceSplit.COAICode;
    //    $scope.InvoiceSplit.COAIText = $scope.$scope.voucherInvoiceSplit.GLItem;
    //    $scope.InvoiceSplit.GLGeneralInfoId = $scope.$scope.voucherInvoiceSplit.GLGeneralInfoId;
    //    $scope.InvoiceSplit.DocRefNo = $scope.voucherInvoiceSplit.DocRefNo;
    //    $scope.InvoiceSplit.DocDate = $scope.$scope.voucherInvoiceSplit.DocDate;
    //};

    $scope.GetVendorSplitrow = function (x, id, index) {
        $scope.indexSplitdetails = index;
        $scope.voucherInvoiceSplit = $scope.voucherDetailInvoiceSplitRow[$scope.indexSplitdetails];
        $scope.updateDiable = true;
        $scope.CAIction = 'Update';
    };

    $scope.addCrRow = function () {
        if ($scope.CAIction == 'Add') {
            uniqueSplitGLCheck($scope.voucherInvoiceSplit, $scope.voucherDetailInvoiceSplitRow);/*check GL unique base on budget,activity,costcenter , entity , budget year and period.*/
            validationAddSplitGL($scope.voucherInvoiceSplit);
            $scope.customerguid();
            $scope.voucherInvoiceSplit.CurrencyId = $scope.voucher.CurrencyId;
            $scope.voucherInvoiceSplit.Id = $scope.vendorInvoiceDetailIdguid;
            $scope.voucherInvoiceSplit.TempId = $scope.vendorInvoiceDetailIdguid;
            $scope.voucherDetailInvoiceSplitRow.push(
                $scope.voucherInvoiceSplit
            );
            if ($scope.clickCount < 1) {
                $scope.clickCount++;
                $scope.invoiceDetailCurrencyrow = [];
            }
            $scope.voucherInvoiceSplit.CrAmount = $scope.voucherInvoiceSplit.Amount;
            //$scope.splitrow();
            $scope.addInvoiceSplitRow($scope.voucherInvoiceSplit);
            $scope.clearCrDetailRow();
            $scope.total();
            $scope.totalAmountCheck();
            console.log('$scope.voucherDetailInvoiceSplitRow', $scope.voucherDetailInvoiceSplitRow);
        }
        else if ($scope.indexSplitdetails != -1 && $scope.CAIction == 'Update') {
            //$scope.splitrow();
            $scope.voucherInvoiceSplit.CrAmount = $scope.voucherInvoiceSplit.Amount;
            $scope.addInvoiceSplitRow($scope.voucherInvoiceSplit);
            console.log('$scope.voucherInvoiceSplit', $scope.voucherInvoiceSplit);
            $scope.indexSplitdetails = -1;
            $scope.CAIction = 'Add';
            $scope.total();
            $scope.totalAmountCheck();
            $scope.clearInvoiceSplit();
            $scope.updateDiable = false;
            // $scope.InvoiceSplit = {};
        }
    };

    $scope.clearInvoiceSplit = function () {
        $scope.voucherInvoiceSplit = {};
    };
    $scope.addInvoiceSplitRow = function (obj) {
        try {
            tabwiseinvoiceSplitrow(obj);
        } catch (e) {
        }
    };
    function tabwiseinvoiceSplitrow(obj) {
        var tabrow = {
            Id: null,
            dr: null,
            cr: null,
            currencyrate: null,
            tocurrencyid: null,
            fromcurrencyid: null,
            parallelcurrencyid: null,
            ParallelCurrencyType: null, ComCurRate: null
        };
        var baseDrTotalAmount = 0;
        var baseCrTotalAmount = 0;
        var currencyrate = 0;
        var fromcurrencyid = null;
        var tocurrencyid = null;
        var parallelcurrencyid = null;
        var vdindex = 0;
        var DrCr = { dr: null, cr: null };

        for (var i = 0; i < $scope.CurrencyParallel.length; i++) {
            SetBaseValue(obj, $scope.voucherDetailCurrency, $scope.CurrencyParallel[i], $scope.voucher.CurrencyId, tabrow);
            if (IsavailblesplitTab(obj.COAICode, tabrow.ParallelCurrencyType, $scope.invoiceDetailCurrencyrow) == false) {
                $scope.invoiceDetailCurrencyrow.push(
                    {
                        Id: guid(),
                        VoucherDetailId: $scope.vendorInvoiceDetailIdguid,
                        TempId: $scope.vendorInvoiceDetailIdguid,
                        COAICode: obj.COAICode,
                        COAIText: obj.COAIText,
                        GLGeneralInfoId: obj.GLGeneralInfoId,
                        DocRefNo: obj.DocRefNo,
                        DocDate: obj.DocDate,
                        FiscalYear: obj.FiscalYear,
                        RefCode: obj.RefCode,
                        ToCurrencyRate: tabrow.currencyrate,
                        FromCurrencyRate: 1,//
                        Narration: obj.Narration,
                        FromCurrencyId: tabrow.fromcurrencyid,
                        ToCurrencyId: tabrow.tocurrencyid,
                        VoucherTypeId: obj.VoucherTypeId,
                        ParallelCurrencyType: tabrow.ParallelCurrencyType,
                        DrAmount: 0,
                        CrAmount: tabrow.cr,
                        Active: true,
                        ParallelCurrencyId: tabrow.parallelcurrencyid
                    })//push
            }
            else {
                var localupdate = {
                    Id: updatevoucercurrencysplitrow($scope.invoiceDetailCurrencyrow, obj.COAICode, obj.GLGeneralInfoId, tabrow.ParallelCurrencyType),
                    VoucherDetailId: obj.Id,
                    TempId: obj.Id,
                    COAICode: obj.COAICode,
                    COAIText: obj.COAIText,
                    GLGeneralInfoId: obj.GLGeneralInfoId,
                    DocRefNo: obj.DocRefNo,
                    DocDate: obj.DocDate,
                    FiscalYear: obj.FiscalYear,
                    RefCode: obj.RefCode,
                    ToCurrencyRate: tabrow.currencyrate,
                    FromCurrencyRate: 1,//
                    Narration: obj.Narration,
                    FromCurrencyId: tabrow.fromcurrencyid,
                    ToCurrencyId: tabrow.tocurrencyid,
                    VoucherTypeId: obj.VoucherTypeId,
                    ParallelCurrencyType: tabrow.ParallelCurrencyType,
                    DrAmount: 0,
                    CrAmount: tabrow.cr,
                    ParallelCurrencyId: tabrow.parallelcurrencyid,
                    Active: true
                }
                UpdatesplitTab(localupdate, $scope.invoiceDetailCurrencyrow);
            }

            // #endregion
        }//for

        function UpdatesplitTab(obj, list) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].COAICode == obj.COAICode && list[i].ParallelCurrencyType == obj.ParallelCurrencyType) {
                    list[i] = obj;
                    break;
                }
            }
        };
        function IsavailblesplitTab(COAICode, ParallelCurrencyType, list) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].COAICode == COAICode && list[i].ParallelCurrencyType == ParallelCurrencyType) {
                    return true;
                }
            }
            return false;
        };
        function updatevoucercurrencysplitrow(list, coacode, gl, currencyType) {
            var id = null;
            if (!baseService.isUndefinedOrNull(list)) {
                for (var i = 0; i < list.length; i++) {
                    if (list[i].COAICode == coacode && list[i].ParallelCurrencyType == currencyType) {//&& list[i].GLGeneralInfoId == gl
                        if (list[i].Id.length == 36) {
                            id = list[i].Id;
                            return id;
                        }
                        else {
                            id = list[i].Id;
                            return id;
                        }
                    }
                }
            }
            else {
                id = guid();
                return id;
            }
        };
        console.log('invoiceDetailCurrencyrow', $scope.invoiceDetailCurrencyrow);
        $scope.clearDetailRow();
        $scope.BaseCurrencytotal();
        if ($scope.indexdetails != -1 && $scope.CAction == 'Update') {
            //$scope.voucherDetailrow[$scope.indexdetails] = $scope.voucherDetail;
            $scope.indexdetails = -1;
            $scope.CAction = 'Add';
            $scope.clearDetailRow();
        }
    };
    $scope.dDetailsId = [];
    $scope.removeRow = function () {
        if ($rootScope.id != null)
            $scope.dDetailsId.push($rootScope.id)
        $scope.voucherDetailInvoiceSplitRow.splice($rootScope.index, 1);
        $rootScope.id = null;
        $scope.total();
        $scope.totalAmountCheck();
    };

    $scope.clearCrDetailRow = function () {
        $scope.voucherInvoiceSplit = { DrAmount: 0, CrAmount: 0, DocRefNo: $scope.voucherDetail.DocRefNo, DocDate: $scope.voucherDetail.DocDate };
        $scope.voucherInvoiceSplit.COAIText = '';
    }

    $scope.total = function () {
        $scope.Drtotal = 0;
        $scope.SplitAmount = 0;
        angular.forEach($scope.voucherDetailrow, function (item) {
            $scope.Drtotal += parseFloat(item.DrAmount);
        });
        angular.forEach($scope.voucherDetailInvoiceSplitRow, function (item) {
            $scope.SplitAmount += parseFloat(item.Amount);
        });
    };
    // #endregion

    $scope.totalAmountCheck = function () {
        $scope.balanceAmount = 0;
        if ($scope.vendorInvoice.Amount == "") {
            $scope.Crtotal = 0;
        }
        else {
            $scope.Crtotal = parseFloat($scope.vendorInvoice.Amount);
        }
        $scope.amount = (parseFloat($scope.Drtotal).toFixed(4) - parseFloat($scope.Crtotal).toFixed(4));
        if ($scope.amount > 0) {
            $scope.balanceText = +parseFloat($scope.amount).toFixed(4);
        } else if ($scope.amount < 0) {
            $scope.balanceText = +(parseFloat($scope.amount * (-2)) + parseFloat($scope.amount));
        } else if (parseFloat($scope.Drtotal).toFixed(4) == parseFloat($scope.Crtotal).toFixed(4)) {
            $scope.balanceText = 'Equal';
        }
    }

    $scope.checkCrAndDrEquealMsg = '';
    $scope.checkCrAndDrEqueal = function (vdcurrencyrowlist) {
        if ($scope.voucher.CurrencyId == null || $scope.voucher.CurrencyId == "") {
            $scope.pop('error', 'Please select Currency !');
            return false;
        }
        if ($scope.IscurrencyHideShow) {
            if ($scope.additionalexchangerate.ToCurrencyRate == 0 || $scope.additionalexchangerate.ToCurrencyRate == ""
                || $scope.additionalexchangerate.ToCurrencyRate == undefined) {
                $scope.pop('error', 'Please input ' + $scope.additionalexchangerate.FromCurrencyCode + 'Currency rate !!');
                return false;
            }
        }
        if ($scope.voucherDetailCurrency.length > 0) {
            for (var i = 0; i < $scope.voucherDetailCurrency.length; i++) {
                if ($scope.voucherDetailCurrency[i].ToCurrencyRate == 0 || $scope.voucherDetailCurrency[i].ToCurrencyRate == "") {
                    $scope.pop('error', 'Please input ' + $scope.voucherDetailCurrency[i].FromCurrencyCode + ' Currency rate !!');
                    return false;
                }
            }
        }
        if ($scope.vendorInvoice.Amount == 0 || $scope.vendorInvoice.Amount == null) {
            $scope.pop('error', 'Please Inpute Amount!. Can not null or 0 Amount Value');
            return false;
        }
        if (parseFloat($scope.Drtotal).toFixed(4) != parseFloat($scope.vendorInvoice.Amount).toFixed(4)) {
            $scope.pop('error', 'DR and CR is not equal');
            return false;
        }
        if (parseFloat($scope.Drtbase).toFixed(4) != parseFloat($scope.Crtbase).toFixed(4)) {
            $scope.pop('error', 'Base Currency DR and CR amount  is not equal');
            return false;
        }
        if (parseFloat($scope.Drtgroup).toFixed(4) != parseFloat($scope.Crtgroup).toFixed(4)) {
            $scope.pop('error', 'Group Currency DR and CR amount  is not equal');
            return false;
        }
        if (parseFloat($scope.Drthard).toFixed(4) != parseFloat($scope.Crthard).toFixed(4)) {
            $scope.pop('error', 'Hard Currency DR and CR amount  is not equal');
            return false;
        }
        if (vdcurrencyrowlist.length > 0) {
            $scope.isAmountNull = true;
            for (var i = 0; i < vdcurrencyrowlist.length; i++) {
                if (vdcurrencyrowlist[i].DrAmount == 0 && vdcurrencyrowlist[i].CrAmount == 0) {
                    $scope.pop('error', ' ' + vdcurrencyrowlist[i].COAIText + ' can not null or 0');
                    $scope.isAmountNull = false;
                    break;
                }
                else if (vdcurrencyrowlist[i].DrAmount == "" && vdcurrencyrowlist[i].CrAmount == 0) {
                    $scope.pop('error', ' ' + vdcurrencyrowlist[i].COAIText + ' can not null or 0');
                    $scope.isAmountNull = false;
                    break;
                }
                else if (vdcurrencyrowlist[i].DrAmount == 0 && vdcurrencyrowlist[i].CrAmount == "") {
                    $scope.pop('error', ' ' + vdcurrencyrowlist[i].COAIText + ' can not null or 0');
                    $scope.isAmountNull = false;
                    break;
                }
            }
            if ($scope.isAmountNull == false) {
                return false;
            }
            else {
                return true;
            }
        }
        else {
            return true;
        }
    };
    $scope.getRefCode = function (refCode) {
        if ($scope.voucherDetail.DocRefNo == null) {
            $scope.voucherDetail.DocRefNo = refCode;
        }
        if ($scope.voucherInvoiceSplit.DocRefNo == null) {
            $scope.voucherInvoiceSplit.DocRefNo = refCode;
        }
    }
    $scope.getDocDate = function (docDate) {
        if (docDate != null) {
            $scope.voucherDetail.DocDate = docDate;
            $scope.voucherInvoiceSplit.DocDate = docDate;
        }
    }
    //Voucher Narration pass to voucher Detail Narration.
    $scope.passNarration = function (narration) {
        if ($scope.voucherDetail.Narration == null) {
            $scope.voucherDetail.Narration = narration;
        }
        if ($scope.voucherInvoiceSplit.Narration == null) {
            $scope.voucherInvoiceSplit.Narration = narration;
        }
    }
    //**********************Search GL List******************
    $scope.searchByGLList = [

        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL',
            'value': 'GLItem'
        }
    ];
    $scope.postingDateMessage = '';
    $('#postingDate').datepicker().on('changeDate', function (ev) {
        $scope.voucher.PostingDate = ev.date;
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            $scope.postingDateMessage = 'Posting date must be below or equal to current Date!';
            $scope.voucher.PostingDate = '';
            $scope.fiscalYearInfo = null;
        }
        else if ($scope.voucher.PostingDate < $scope.voucher.DocDate) {
            $scope.postingDateMessage = 'Doc  date must be below or equal to Posting Date!';
            $scope.voucher.PostingDate = '';
            $scope.fiscalYearInfo = null;
        } else {
            $scope.getPostingFiscalYearPeriod($scope.voucher.PostingDate);
            $scope.getBudgetFiscalYear($scope.voucher.PostingDate);
            $scope.getBudgetFiscalYearPeriod($scope.voucher.PostingDate);
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
                    manualValidation('div_postingDate', response.data.Error, 'Fiscal Year Not Found !')
                }
                else {
                    var result = response.data;
                    if (result.IsTransationLocked === true) {
                        manualValidation('div_postingDate', result.IsTransationLocked, commonMessage.FiscalPeriodTransactionLocked)
                        $scope.voucher.PostingDate = '';
                        $scope.fiscalYearInfo = null;
                    }
                    else if (result.IsExchangeRateConfirmed === false) {
                        manualValidation('div_postingDate', !result.IsExchangeRateConfirmed, commonMessage.FiscalPeriodExchangeRateConfirmed)
                        $scope.voucher.PostingDate = '';
                        $scope.fiscalYearInfo = null;
                    }
                    else {
                        $scope.fiscalYearInfo = result;
                        $scope.checkDate();
                        $scope.GetCurrencyExchangeRateList();
                    }
                }
            },
            function errorCallback(response) {
            });
    };
    // For fist time calling of Posting date changes.
    $scope.getPostingFiscalYearPeriod($scope.voucher.PostingDate);

    $scope.getBudgetFiscalYear = function (date) {
        $http({
            method: 'GET',
            url: 'accounts/CompanyFiscalYear/CheckingBudgetFiscalYear?postingDate=' + $filter("dateFiltering")(date),
        }).then(function (response) {
            $scope.fiscalYearList = response.data;
        });
    }
    $scope.getBudgetFiscalYear($scope.voucher.PostingDate);

    $scope.getBudgetFiscalYearPeriod = function (date) {
        $http({
            method: 'GET',
            url: 'accounts/CompanyFiscalYear/BudgetFiscalYearPeriod?postingDate=' + $filter("dateFiltering")(date),
        }).then(function (response) {
            $scope.budgetFiscalYearPeriodIdList = response.data;
        });
    }
    $scope.getBudgetFiscalYearPeriod($scope.voucher.PostingDate);

    $scope.onFiscalYearChange = function (fiscalYearId) {
        if ($scope.companyConfig.IsBudgetPeriod) {
            $scope.voucherDetail.FiscalYearName = $("#FiscalYear option:selected").text();
            $scope.voucherDetail.FiscalYearId = fiscalYearId;
            if ($scope.voucher.PostingDate != '') {
                $http({
                    method: 'get',
                    url: 'accounts/CompanyFiscalYear/CheckingBudgetFiscalYearPeriod?fiscalYearId=' + fiscalYearId + '&postingDate=' + $scope.voucher.PostingDate,
                }).then(function successCallback(response) {
                    $scope.fiscalyearperiodbyidlist = response.data;
                });
            }
            else
                $scope.pop('error', 'PostingDate is Null !! Please select PostigDate !');
        }
    };
    $scope.onFiscalYearPeriodChange = function (fiscalYearPeriodId) {
        $scope.voucherDetail.PeriodName = null;
        for (var i = 0; i < $scope.fiscalyearperiodbyidlist.length; i++) {
            if ($scope.fiscalyearperiodbyidlist[i].FiscalYearPeriodId == fiscalYearPeriodId) {
                if ($scope.fiscalyearperiodbyidlist[i].IsBudgetLocked) {
                    ShowResult(commonMessage.FiscalPeriodBudgetLocked, 'failure');
                }
                else {
                    $scope.voucherDetail.FiscalYearPeriodId = fiscalYearPeriodId;
                    var FiscalYearPeriodName = $.grep($scope.fiscalyearperiodbyidlist, function (item) {
                        return item.FiscalYearPeriodId === fiscalYearPeriodId;
                    })[0].PeriodName;
                    $scope.voucherDetail.PeriodName = FiscalYearPeriodName;
                }
            }
        }
    };

    $scope.dateMessage = '';
    $scope.checkDate = function () {
        var invalidDocDate = false;
        if (new Date($scope.voucher.DocDate) > new Date()) {
            manualValidation('div_docDate', !invalidDocDate, 'Doc date must be below or equal to current Date ');
            return invalidDocDate = false;
        }
        else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            manualValidation('div_docDate', !invalidDocDate, 'Doc date must be below or equal to Posting Date ');
            return invalidDocDate = false;
        }
        else if (new Date($scope.voucher.VoucherDate) < new Date($scope.voucher.PostingDate)) {
            manualValidation('div_docDate', !invalidDocDate, 'Posting Date must be below or equal to Voucher Date ');
            return invalidDocDate = false;
        }
        else {
            $scope.dateMessage = '';
            $scope.voucherInvoiceSplit.DocDate = new Date($scope.voucher.DocDate);
            return invalidDocDate = true;
        }
    }
    $scope.checkAllDate = function () {
        if ($scope.voucher.VoucherDate >= $scope.voucher.PostingDate && $scope.voucher.PostingDate >= $scope.voucher.DocDate) {
            return true;
        }
        else {
            $scope.pop('error', 'Doc date must be below or equal to Posting Date ');
            return false;
        }
    }
    $scope.checkVoucherDetailDockDate = function () {
        if (new Date($scope.voucherDetail.DocDate) > new Date($scope.voucher.PostingDate)) {
            $scope.pop('error', 'VoucherDetail Doc date must be below or equal to Posting Date ');
            return false;
        }
        if ($scope.voucherDetailInvoiceSplitRow.length > 0) {
            for (var i = 0; i < $scope.voucherDetailInvoiceSplitRow.length; i++) {
                if (new Date($scope.voucherDetailInvoiceSplitRow[i].DocDate) > new Date($scope.voucher.PostingDate)) {
                    $scope.pop('error', ' InvoiceSplit Doc date must be below or equal to Posting Date ');
                    return false;
                }
            }
        }
        else {
            return true;
        }
    };

    $scope.VoucherDateMessage = '';
    $scope.checkVoucherDate = function () {
        if (new Date($scope.voucher.VoucherDate) > new Date()) {
            $scope.VoucherDateMessage = 'Voucher date must be below or equal to current Date ';
            return false
        }
        else
            $scope.VoucherDateMessage = '';
        return true
    };

    // #region ***********Tax Code Operation **********
    $scope.taxcodedata = [];

    $scope.taxPayable = {
        Sequence: 0,
        TaxAuto: 0,
        Tax: 0,
        TaxCodeId: null
    }
    $scope.taxCode = {
        Id: null
    }
    $scope.taxcodelistMessage = '';
    $scope.getTaxCodeByTaxYear = function (date) {
        $http({
            method: 'get',
            url: 'accounts/TaxCode/GetCboInput?postingDate=' + $filter("dateFiltering")(date),
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message
                }
                else {
                    var result = response.data;
                    $scope.taxCodCboList = result;
                    if ($scope.taxCodCboList.length == 0) {
                        $scope.pop('error', 'No TaxCode found in this Fiscal Year ');
                    }
                }
            },
            function errorCallback(response) {
            });
    };
    $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);

    $http({
        method: 'get',
        url: 'accounts/taxcode/TaxCodeListIncludeExpensesGl',
    }).then(function successCallback(response) {
        $scope.taxCodExpensesGlList = response.data.Rows;
    });

    $http({
        method: 'get',
        url: 'accounts/taxcode/TaxCodeListIncludeWithholdGl',
    }).then(function successCallback(response) {
        $scope.taxCodWithholdGlList = response.data.Rows;
    });

    $scope.ontaxCodeChange = function (item) {
        $http({
            method: 'get',
            url: 'accounts/taxcode/GetTaxCodeById?id=' + item,
        }).then(function successCallback(response) {
            $scope.taxcodedata = response.data;
        });
    }

    $scope.taxCodDataList = [];
    $scope.addTaxCodeonList = function () {
        try {
            var ob = angular.copy($scope.taxcodedata);
            ob.Sequence = $scope.taxCodDataList.length + 1;
            var has = false;
            var checkValue = false;//if not expse gl
            var exclud = false;//if expse gl
            for (var i = 0; i < $scope.taxCodDataList.length; i++) {
                if ($scope.taxCodDataList[i].TaxCodeId == ob.TaxCodeId) {//expnse gl
                    throw ('Tax code (<b>' + ob.UserName + '</b>) is already added !!!');
                }
            }
            $scope.taxCodDataList.push(ob);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    function isAvilable(id, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].TaxCodeId == id) {
                return true;
            }
        }
        return false;
    }
    $scope.getTaxCode = function (index) {
        $scope.setTaxVoucherDetailIndex = index;
        $scope.taxCodDataList = $scope.voucherDetailrow[index]['VendorInvoiceTax'];;
        angular.element(document.querySelector('#texCodePopUp')).modal('show');
    }

    $scope.closeTaxCodePopUp = function () {
        angular.element(document.querySelector('#texCodePopUp')).modal('hide');
    }

    $scope.checkTaxAllow = function () {
        for (var i = 0; i < $scope.voucherDetailrow.length; i++) {
            if ($scope.IsTaxExemptionCheck == false) {
                if ($scope.voucherDetailrow[i].taxCategoryStatus == true) {
                    $scope.pop('error', 'There is no Tax Exemption and PositionWithout Tax is not Allow where Amount ' + $scope.voucherDetailrow[i].DrAmount);
                    return false;
                }
            }
        }
        return true;
    };
    // #region TaxCode Row Delete
    $scope.taxCodeDelModal = function (id, username) {
        $scope.TaxCodeId = id;

        if (baseService.isUndefinedOrNull($scope.TaxCodeId))
            $scope.Taxmessage_confirmation = 'Are you sure want to delete [ ' + username + ' ] data....';
        else
            $scope.Taxmessage_confirmation = 'Are you sure want to delete [ ' + username + ' ] ?';
        angular.element(document.querySelector('#confirmTaxCodeDelPopUp')).modal('show');
    };

    $scope.removeTaxCodeRow = function () {
        if ($scope.TaxCodeId != null)
            for (var i = 0; i < $scope.taxCodDataList.length; i++) {
                if ($scope.taxCodDataList[i].TaxCodeId == $scope.TaxCodeId) {
                    $scope.taxCodDataList.splice(i, 1);
                }
            }
        $scope.TaxCodedId = null;
    };

    $scope.enabletax = function () {
        if (baseService.isUndefinedOrNull($scope.getVendorDetails)) {
            return true
        }
        else
            return false
    }
    $scope.checkVendorMsg = '';
    $scope.invoiceCheck = function () {
        if ($scope.getVendorDetails != undefined) {
            $scope.checkVendorMsg = '';
            return true;
        } else {
            $scope.pop('error', 'Customer is not selected');
            return false
        }
    }
    // #region *******InvoiceTaxPush******
    $scope.vendorInvoiceTaxes = [];

    $scope.vendorInvoiceTaxPush = function () {
        var vendorInvoiceTaxesLength = $scope.vendorInvoiceTaxes.length;
        for (var i = 0; i < vendorInvoiceTaxesLength; i++) {
            if ($scope.voucherDetail.VendorInvoiceTax[i].InvoiceDetailOppositEntryId == $scope.setIndex) {
                $scope.voucherDetail.VendorInvoiceTax.splice(i, 1);
            }
        }
        for (var i = 0; i < $scope.taxCodDataList.length; i++) {
            $scope.taxCodDataList[i].InvoiceDetailOppositEntryId = $scope.setIndex;
            if (vendorInvoiceTaxesLength > 0) {
                var aabc = $scope.voucherDetail.VendorInvoiceTax;
                $scope.voucherDetail.VendorInvoiceTax.push($scope.taxCodDataList[i]);
            }
            else {
                console.log($scope.voucherDetail);
                var aabcd = $scope.voucherDetail.VendorInvoiceTax;
                $scope.voucherDetail['VendorInvoiceTax'] = $scope.taxCodDataList;
            }
        }
        vendorInvoiceTaxesLength = 0;
        $scope.voucherDetailrow[$scope.setTaxVoucherDetailIndex].taxCategoryStatus = false;
    };
    //VoucherDetailRowdelete
    $scope.VoucherDetailDelete = function (data, index) {
        $scope.index1 = index;
        $scope.tempId = data.Id;
        $scope.message_confirmation = 'Are you sure to delete []';
        angular.element(document.querySelector('#confirmVoucherDetaildelete')).modal('show');
    }
    $scope.removeVoucherDetailRow = function () {
        for (var i = $scope.voucherDetailCurrencyrow.length - 1; i >= 0; i--) {// forr loop
            if ($scope.tempId == $scope.voucherDetailCurrencyrow[i].VoucherDetailId) {
                $scope.voucherDetailCurrencyrow.splice(i, 1);
            }
        }
        $scope.voucherDetailrow.splice($scope.index1, 1);
        $scope.total();
        $scope.totalAmountCheck();
        $scope.BaseCurrencytotal();
    };
    //VoucherDetailRowdelete
    $scope.vendorInvoiceSplitDelete = function (data, index) {
        $scope.index1 = index;
        //$scope.tempdocRef = data.DocRefNo;
        $scope.tempSplitId = data.Id;
        $scope.message_confirmation = 'Are you sure to delete []';
        angular.element(document.querySelector('#confirmvendorInvoiceSplitdelete')).modal('show');
    }

    $scope.removevendorInvoiceSplitRow = function () {
        for (var i = $scope.invoiceDetailCurrencyrow.length - 1; i >= 0; i--) {// forr loop
            if ($scope.tempSplitId == $scope.invoiceDetailCurrencyrow[i].VoucherDetailId) {
                $scope.invoiceDetailCurrencyrow.splice(i, 1);
            }
        }
        $scope.voucherDetailInvoiceSplitRow.splice($scope.index1, 1);
        $scope.BaseCurrencytotal();
        $scope.total();
        $scope.totalAmountCheck();
        if ($scope.voucherDetailInvoiceSplitRow.length == 0) {
            $scope.CalvendorInvoice();
            tabwiseinvoicerow($scope.VendorInvoiceDetail);
            $scope.clickCount = 0;
        }
    };

    $scope.CurrencyList = function myfunction() {
        merge($scope.invoiceDetailCurrencyrow, $scope.voucherDetailCurrencyrow);
    };
    // link http://stackoverflow.com/questions/32579066/merge-arrays-combining-matching-objects-in-angular-javascript
    function merge(array1, array2) {
        var ids = [];
        var merge_obj = [];

        array1.map(function (ele) {
            if (!(ids.indexOf(ele.Id) > -1)) {
                ids.push(ele.Id);
                merge_obj.push(ele);
            }
        });
        array2.map(function (ele) {
            var index = ids.indexOf(ele.Id);
            if (!(index > -1)) {
                ids.push(ele.Id);
                merge_obj.push(ele);
            } else {
                merge_obj[index] = ele;
            }
        });
        $scope.currencyList = merge_obj;
    }
    $scope.Park = function () {
        $scope.currencyTransactionId = $('#currencyTransactionId option:selected').text();
        $scope.vendorInvoiceAlternative = [];
        $scope.currencyList = [];
        $scope.CurrencyList();
        reDirectToRequiredTab();
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.invoiceCustomerForm.$valid && $scope.checkDate()) {//&& $scope.checkAllDate() && $scope.checkVoucherDate() && $scope.checkCrAndDrEqueal($scope.currencyList)
            if ($scope.vendorInvoice.Id == null) {
                $scope.vendorInvoice = {
                    PartyId: $scope.crRowSelected,
                    PostingDate: $scope.voucher.PostingDate,
                    GLGeneralInfoId: $scope.getVendorDetails.GLGeneralInfoId,
                    BudgetId: $scope.getVendorDetails.BudgetId,
                    ActivityId: $scope.getVendorDetails.ActivityId,
                    CurrencyId: $scope.voucher.CurrencyId,
                    DocRefNo: $scope.voucher.DocRefNo,
                    DocDate: $scope.voucher.DocDate,
                    Amount: $scope.vendorInvoice.Amount,
                    Id: $scope.vendorInvoiceDetailIdguid,
                    IsExcludingTax: $scope.voucher.IsExcludingTax,
                    PaymentTermId: $scope.paymentTerms.PaymentTermId,
                    BaseOnDueDate: $scope.paymentTerms.BaseOnDueDate,
                    BaseNoOfDays: $scope.paymentTerms.BaseNoOfDays,
                    TempId: $scope.vendorInvoiceDetailIdguid
                };
            }
            else {
                $scope.vendorInvoice.CurrencyId = $scope.voucher.CurrencyId;
                $scope.vendorInvoice.GLGeneralInfoId = $scope.getVendorDetails.GLGeneralInfoId;
                $scope.vendorInvoice.BudgetId = $scope.getVendorDetails.BudgetId;
                $scope.vendorInvoice.ActivityId = $scope.getVendorDetails.ActivityId;
                $scope.vendorInvoice.Amount = $scope.vendorInvoice.Amount;
                $scope.vendorInvoice.IsExcludingTax = $scope.voucher.IsExcludingTax;
                $scope.vendorInvoice.DocRefNo = $scope.voucher.DocRefNo;
                $scope.vendorInvoice.DocDate = $scope.voucher.DocDate;
            }
            if ($scope.customerInvoiceDetails == null) {
                $scope.vendorInvoiceAlternative.push($scope.vendorInvoice);
            }
            else if ($scope.customerInvoiceDetails != null && $scope.voucherDetailInvoiceSplitRow.length == 0) {
                angular.forEach($scope.customerInvoiceDetails, function (item) {
                    $scope.vendorInvoiceAlternative.push(item)
                })
            }
            else if ($scope.voucherDetailInvoiceSplitRow.length > 1) {
                angular.forEach($scope.voucherDetailInvoiceSplitRow, function (item) {
                    if (item.Id == 36) {
                        $scope.vendorInvoice.IsSplit = true;
                        $scope.vendorInvoiceAlternative.push(item)
                    }
                    else if (item.Id < 36) {
                        $scope.vendorInvoice.IsSplit = false;
                        $scope.vendorInvoiceAlternative.push(item)
                    }
                })
            }
            $scope.voucher.FiscalYearId = $scope.fiscalYearInfo.FiscalYearId;
            $scope.voucher.FiscalYearPeriodId = $scope.fiscalYearInfo.FiscalYearPeriodId;
            if ($scope.Action == 'Park') {
                $http({
                    method: 'POST',
                    url: $scope.parkUrl,//$scope.parkUrl
                    data: {
                        'voucher': $scope.voucher, 'voucherDetails': $scope.voucherDetailrow,
                        'vendorInvoice': $scope.vendorInvoice,
                        'customerInvoiceDetails': $scope.vendorInvoiceAlternative,
                        'voucherDetailCurrencies': $scope.currencyList,
                        'baseCurrencyrate': $scope.baseCurrency,
                        'groupCurrencyrate': $scope.groupCurrency,
                        'hardCurrencyrate': $scope.hardCurrency,
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.voucher = response.data.Voucher;
                        ClearFields();
                        $scope.voucher.VoucherNo = response.data.Voucher.VoucherNo;
                        $scope.popVoucherCode('error', $scope.voucher.VoucherNo);
                    }
                });
                return true;
            }
        }
    }

    $scope.Post = function () {
        $scope.currencyTransactionId = $('#currencyTransactionId option:selected').text();
        $scope.vendorInvoiceAlternative = [];
        $scope.currencyList = [];
        $scope.CurrencyList();
        reDirectToRequiredTab();
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.invoiceCustomerForm.$valid && $scope.checkCrAndDrEqueal($scope.currencyList) && $scope.checkTaxAllow()
            && $scope.checkDate() && $scope.checkVoucherDetailDockDate() && $scope.checkVoucherDate() //&& $scope.checkAllDate()
        ) {
            if ($scope.invoiceCheck()) {
                if ($scope.vendorInvoice.Id == null) {
                    $scope.vendorInvoice = {
                        PartyId: $scope.crRowSelected,
                        PostingDate: $scope.voucher.PostingDate,
                        GLGeneralInfoId: $scope.getVendorDetails.GLGeneralInfoId,
                        ActivityId: $scope.getVendorDetails.ActivityId,
                        BudgetId: $scope.getVendorDetails.BudgetId,
                        CurrencyId: $scope.voucher.CurrencyId,
                        DocRefNo: $scope.voucher.DocRefNo,
                        DocDate: $scope.voucher.DocDate,
                        Amount: $scope.vendorInvoice.Amount,
                        Id: $scope.vendorInvoiceDetailIdguid,
                        IsExcludingTax: $scope.voucher.IsExcludingTax,
                        PaymentTermId: $scope.paymentTerms.PaymentTermId,
                        BaseOnDueDate: $scope.paymentTerms.BaseOnDueDate,
                        BaseNoOfDays: $scope.paymentTerms.BaseNoOfDays,
                        TempId: $scope.vendorInvoiceDetailIdguid,
                        FiscalYearPeriodId: $scope.fiscalYearInfo.FiscalYearPeriodId,
                        FiscalYearId: $scope.fiscalYearInfo.FiscalYearId,
                        EntityId: $scope.vendorInvoice.EntityId
                    };
                }
                else {
                    $scope.vendorInvoice.CurrencyId = $scope.voucher.CurrencyId;
                    $scope.vendorInvoice.GLGeneralInfoId = $scope.getVendorDetails.GLGeneralInfoId;
                    $scope.vendorInvoice.ActivityId = $scope.getVendorDetails.ActivityId;
                    $scope.vendorInvoice.BudgetId = $scope.getVendorDetails.BudgetId;
                    $scope.vendorInvoice.Amount = $scope.vendorInvoice.Amount;
                    $scope.vendorInvoice.IsExcludingTax = $scope.voucher.IsExcludingTax;
                    $scope.vendorInvoice.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.vendorInvoice.DocDate = $scope.voucher.DocDate;
                }
                if ($scope.customerInvoiceDetails == null && $scope.voucherDetailInvoiceSplitRow.length == 0) {
                    $scope.vendorInvoiceAlternative.push($scope.vendorInvoice);
                }
                else if ($scope.customerInvoiceDetails != null && $scope.voucherDetailInvoiceSplitRow.length == 0) {
                    angular.forEach($scope.customerInvoiceDetails, function (item) {
                        $scope.vendorInvoiceAlternative.push(item)
                    })
                }
                else if ($scope.voucherDetailInvoiceSplitRow.length > 1) {
                    for (var i = 0; i < $scope.voucherDetailInvoiceSplitRow.length; i++) {
                        if ($scope.voucherDetailInvoiceSplitRow[i].Id.length == 36) {
                            $scope.vendorInvoiceAlternative[i] = $scope.voucherDetailInvoiceSplitRow[i];
                            $scope.vendorInvoice.IsSplit = true;
                        }
                        else if ($scope.voucherDetailInvoiceSplitRow[i].Id.length < 36) {
                            $scope.vendorInvoiceAlternative[i] = $scope.voucherDetailInvoiceSplitRow[i];
                            $scope.vendorInvoice.IsSplit = false;
                        }
                    }
                }
                $scope.voucher.FiscalYearId = $scope.fiscalYearInfo.FiscalYearId;
                $scope.voucher.FiscalYearPeriodId = $scope.fiscalYearInfo.FiscalYearPeriodId;
                $scope.voucher.EntityId = $scope.vendorInvoice.EntityId
                console.log('voucher', $scope.voucher);
                console.log('voucherDetails', $scope.voucherDetailrow);
                console.log('vendorInvoice', $scope.vendorInvoice);
                console.log('customerInvoiceDetails', $scope.vendorInvoiceAlternative);
                console.log('voucherDetailCurrencies', $scope.currencyList);
                console.log('baseCurrencyrate', $scope.baseCurrency);
                console.log('groupCurrencyrate', $scope.groupCurrency);
                console.log('hardCurrencyrate', $scope.hardCurrency);
                if ($scope.SAction == 'Post') {
                    $http({
                        method: 'POST',
                        url: $scope.postUrl,
                        data: {
                            'voucher': $scope.voucher, 'voucherDetails': $scope.voucherDetailrow,
                            'vendorInvoice': $scope.vendorInvoice,
                            'vendorInvoiceDetails': $scope.vendorInvoiceAlternative,
                            'voucherDetailCurrencies': $scope.currencyList,
                            'baseCurrencyrate': $scope.baseCurrency,
                            'groupCurrencyrate': $scope.groupCurrency,
                            'hardCurrencyrate': $scope.hardCurrency,
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.voucher = response.data.Voucher;
                            ClearFields();
                            $scope.getData();
                            // Show last voucher no.
                            $scope.voucher.VoucherNo = response.data.Voucher.VoucherNo;
                            $scope.popVoucherCode('error', $scope.voucher.VoucherNo);
                        }
                    });
                    return true;
                }
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.voucher.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.vouchers.splice($scope.index, 1);
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    }
    $scope.paymentTermList = [];
    $scope.paymentTerms = {
        BaseLineDate: null,
        BaseNoOfDays: 0,
        BaseOnDueDate: $filter('date')(Date.now(), 'yyyy-MM-dd'),
        PaymentTermCode: null,
        PaymentTermId: null
    }
    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getvendorcbo',
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
        console.log('paymentTermList', $scope.paymentTermList);
    });

    $scope.onPaymnetChange = function (id) {
        if (id != null) {
            var baseLineDate = $.grep($scope.paymentTermList, function (item) {
                return item.Value === id;
            })[0].BaseLineDate;

            var paymentTermCode = $.grep($scope.paymentTermList, function (item) {
                return item.Value === id;
            })[0].PaymentTermCode;
            var noOfDay = $.grep($scope.paymentTermList, function (item) {
                return item.Value === id;
            })[0].NoOfDay;
            $scope.paymentTerms.PaymentTermCode = paymentTermCode;
            $scope.paymentTerms.BaseNoOfDays = noOfDay;
            if (baseLineDate != null)
                if (baseLineDate == 'documentdate') {
                    $scope.paymentTerms.BaseOnDueDate = $scope.voucher.DocDate
                } else if (baseLineDate == 'postingdate') {
                    $scope.paymentTerms.BaseOnDueDate = $scope.voucher.PostingDate
                }
                else {
                    $scope.paymentTerms.BaseOnDueDate = $filter('date')(Date.now(), 'yyyy-MM-dd');
                }
            $scope.getMatureDate($scope.paymentTerms.BaseOnDueDate, $scope.paymentTerms.BaseNoOfDays);
        }
    };
    $scope.getMatureDate = function (date, days) {
        var declareDate = new Date(date);
        declareDate.setDate(declareDate.getDate() + days);
        var dateFormated = $filter('date')(declareDate, 'dd-MMM-yyyy');
        $scope.getM = dateFormated;
    }
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = 'Park';
        $scope.voucher = {};
        $scope.voucherDetailrow = [];
        $scope.voucherDetail = [];
        $scope.Drtotal = 0;
        $scope.Crtotal = 0;
        $scope.voucher.Active = true;
        $scope.vendorInvoice = {};
        $scope.vendorInvoice.Amount = 0;
        $scope.GLNameCode = null;
        $scope.updateDiable = true;
        $scope.RateAmountDisable = true;
        $scope.customerNameCode = null;
        $scope.paymentTerms = {};
        $scope.voucherDetailCurrency = [];
        $scope.voucherDetailInvoiceSplitRow = [];
        $scope.vendorInvoiceAlternative = [];
        $scope.showCtrl = true;
        $scope.voucher.IsExcludingTax = false;
        $scope.voucher.VoucherDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucher.PostingDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucher.DocDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucherDetail.DocDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucherInvoiceSplit.DocRefNo = null;
        $scope.voucherInvoiceSplit.Narration = null;
        $scope.voucher.VoucherTypeId = null;
        $('.datepicker').datepicker({
            format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
        });
        $scope.invoiceDetailCurrencyrow = [];
        $scope.voucherDetailCurrencyrow = [];
        $scope.getVendorDetails.Party = null;
        $scope.getVendorDetails.Code = null;
        $scope.getVendorDetails.GLItem = null;
        $scope.getVendorDetails.COAICode = null;
        $scope.Drtbase = 0;
        $scope.Crtbase = 0;
        $scope.Drtgroup = 0;
        $scope.Crtgroup = 0;
        $scope.Drthard = 0;
        $scope.Crthard = 0;
        $scope.SplitAmount = 0;
        $scope.getBudgetId = null;
        $scope.getActivityId = null;
        $scope.getBudgetNamecode = null;
        $scope.getActivityNamecode = null;
        $scope.IsAssetBudgetYear = false;
        $scope.IsExpenseBudgetYear = false;
        $scope.BudgetItemList = [];
        $scope.ActivityList = [];
        $scope.IsExpenses = false;
        $scope.IsTaxExemptionCheck = false;
        if ($scope.voucherTypeList.length === 1) {
            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
        }
    };
    $scope.showCtrl = true;
    $scope.showCrlOnCurrency = function (id) {
        if (id != null || id != '') {
            $scope.showCtrl = false;
        }
    };
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.tabdetail = 5;
    $scope.setTabdetail = function (newTab) {
        $scope.tabdetail = newTab;
    };
    $scope.isSetdetail = function (tabNum) {
        return $scope.tabdetail === tabNum;
    }
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

    $scope.vendorInvoiceVoucherReport = function (voucherNo) {
        location.href = 'accounts/voucher/vendorinvoicevoucherreport?voucherNo=' + voucherNo;
    };
}