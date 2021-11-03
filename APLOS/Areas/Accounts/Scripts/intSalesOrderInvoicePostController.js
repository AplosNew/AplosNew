'use strict';
intSalesOrderInvoicePostController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function intSalesOrderInvoicePostController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    $rootScope.title = 'Sales Order Invoice Post';
    $scope.Action = 'Post';
    $scope.index = -1;

    $scope.CurrencyParallel = [];
    $scope.currencyexchangerate = [];
    $scope.voucherDetailCurrency = [];
    $scope.invoiceDetailCurrencyrow = [];
    $scope.voucherDetailCurrencyrow = [];
    $scope.voucherDetailrow = [];
    $scope.customerInvoiceDetailIdguid = null;
    $scope.path = 'accounts/voucher/';
    $scope.saveUrl = $scope.path + 'SalesOrderInvoicePost';
    $scope.parkUrl = $scope.path + 'customerinvoicepark';
    $scope.getListUrl = $scope.path + 'GetCustomerInvoiceSaleOrderInvoice';
    baseService.init($scope.getListUrl, null, null, 'desc', 'DocRefNo', 'DocRefNo');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                // $scope.saleOrderInvoiceEdit = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.voucher = {
        Id: null,
        CustomerInvoiceId: null,
        CurrencyId: null,
        CurrencyCode: null,
        VoucherTypeId: "1",
        PartyId: null,
        APLOS0RDId: null,
        Sequence: 0,
        Type: 'Receivable',
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: $filter("dateFiltering")(Date.now()),
        DocRefNo: null,
        DocDate: $filter("dateFiltering")(Date.now()),
        FiscalYearId: null,
        FiscalYearPeriodId: null,
        IsExcludingTax: false,
        VoucherDetailId: null,//Only for get voucherdetail data from VoucherDetail
        Amount: 0,//only for get VendorInvoice Amount
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
    $scope.materialHSNCodeTaxes = [{
        Id: null
        , CustomerInvoiceId: null
        , CustomerInvoiceDetailId: null
        , MaterialMasterId: null
        , SubMaterialId: null
        , UomId: null
        , Qty: 0
        , Amount: 0
        , Rate: 0
        , MaterialName: null
        , HSNCode: null
        , CountryId: null
        , CustomerId: null
        , HSNCodeTaxes: [
            {
                Id: null
                , MaterialMasterId: null
                , CustomerInvoiceMaterialId: null
                , TaxCategoryId: null
                , TaxAmount: 0.00
                , HSNCodeId: null
                , HSNCode: null
                , TaxCategoryName: null
            }
        ],
    }];
    // #endregion

    // #region ***********voucherDetail**********
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
        Narration: null,
        RandomNumber: 0,
        Active: true,
        AddedBy: null,
        AddedDate: $filter('date')(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        PostingWithoutTaxAllow: false,
        TaxCategory: null,
        taxCategoryStatus: false,
        CustomerInvoiceTax: [
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

    $scope.saleOrderInvoiceSales = [{
        Id: null,
        VoucherId: null,
        CustomerInvoiceDetailId: null,
        CompanyGroupId: null,
        CompanyId: null,
        BudgetMasterId: null,
        BudgetActivityId: null,
        CurrencyId: null,
        CurrencyCode: null,
        SaleOrderInvoiceMasterId: null,
        SaleTypeName: null,
        SalesTypeId: null,
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
        Amount: 0,
        CrAmount: 0,
        DrAmount: 0,
        TaxAmount: 0,
        NetAmount: 0,
        Narration: null,
        RandomNumber: 0,
        Active: true,
        AddedBy: null,
        AddedDate: $filter('date')(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        PostingWithoutTaxAllow: false,
        TaxCategory: null,
        taxCategoryStatus: false,
        CustomerInvoiceTax: [
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
    }];
    $scope.additionalexchangerate = {
        ToCurrencyRate: null,
        FromCurrencyUnit: 1,
        FromCurrencyCode: null,
        ToCurrency: null
    };

    $('.datepicker').datepicker({
        format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
    });
    cboService.getCboVoucherTypeAccountReceivableList(function (result) {
        $scope.voucherTypeList = result;
    });

    $scope.GetCurrencyParallel = function () {
        $http({
            method: 'GET',
            url: 'currencies/CompanyParallelCurrency/CurrencyParallel',
        }).then(function successCallback(response) {
            $scope.CurrencyParallel = response.data;
        });
        $scope.CheckParallelCurrencyValid();
    };

    $scope.CheckParallelCurrencyValid = function () {
        if ($scope.CurrencyParallel[0] == 0) {
            ShowResult('Company Parallel Currency did not set!', 'failure');
            return false;
        }
        else {
            return true;
        }
    };
    $scope.GetCurrencyParallel();
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
            $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);
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
                        $scope.voucher.PostingDate = '';
                        $scope.fiscalYearInfo = null;
                    }
                    else if (result.IsExchangeRateConfirmed === false) {
                        ShowResult(commonMessage.FiscalPeriodExchangeRateConfirmed, 'failure');
                        $scope.voucher.PostingDate = '';
                        $scope.fiscalYearInfo = null;
                    }
                    else {
                        $scope.fiscalYearInfo = result;
                        // $scope.GetCurrencyExchangeRateList();
                    }
                }
            },
            function errorCallback(response) {
            });
    };
    // For fist time calling of Posting date changes.
    $scope.getPostingFiscalYearPeriod($scope.voucher.PostingDate);
    $scope.taxcodelistMessage = '';
    $scope.getTaxCodeByTaxYear = function (date) {
        $http({
            method: 'get',
            url: 'accounts/TaxCode/GetCboOutput?postingDate=' + $filter("dateFiltering")(date),
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

    Get($routeParams.id);
    //GetSaleType($routeParams.id);
    function Get(id) {
        $http.get('accounts/voucher/GetSaleOrderInvoiceById?salesOrderInvoiceMasterId=' + id)
            .then(function (response) {
                $scope.saleOrderInvoiceEdit = response.data.Rows;
                $scope.voucher.CurrencyId = $scope.saleOrderInvoiceEdit[0].CurrencyId;
                $scope.voucher.CurrencyCode = $scope.saleOrderInvoiceEdit[0].CurrencyCode;
                $scope.CheckParallelCurrencyBySelecteCurr($scope.voucher.CurrencyId);
                $scope.salesguid();
                GetSaleType($routeParams.id);
                GetCustomerInvoiceDetail($scope.saleOrderInvoiceEdit[0].Id);
                $scope.partyName = $.grep($scope.saleOrderInvoiceEdit, function (item) {
                    return item.SaleOrderInvoiceMasterId === id;
                })[0].PartyName;

                var partyid = $.grep($scope.saleOrderInvoiceEdit, function (item) {
                    return item.SaleOrderInvoiceMasterId === id;
                })[0].PartyId;

                GetMaterialHSNCodeTaxAndGL(id);
                //GetTaxCategoryByParty(partyid);
            });
    }

    function GetTaxCategoryByParty(partyid) {
        $http.get('accounts/voucher/GetTaxCategoryByParty?partyId=' + partyid)
            .then(function (response) {
                $scope.taxcategories = response.data;
                $scope.partyTaxCategory = $.grep($scope.taxcategories, function (item) {
                    return item.PartyId === partyid;
                })[0].PartyTaxCategory;
            });
    };

    function GetMaterialHSNCodeTaxAndGL(id) {
        $http.get('accounts/voucher/GetMaterialHSNCodeTaxAndGL?saleOrderInvoiceMasterId=' + id)
            .then(function (response) {
                $scope.materialHSNCodeTaxes = response.data;
                //for (var i = 0; i < $scope.materialHSNCodeTaxes.length; i++) {
                //    GetHSNCodeTaxGL($scope.materialHSNCodeTaxes[i].CustomerId, $scope.materialHSNCodeTaxes[i].SalesOrganizationId
                //        , $scope.materialHSNCodeTaxes[i].HSNCodeId, $scope.materialHSNCodeTaxes[i].MaterialMasterId)
                //}
            });
    };

    function GetHSNCodeTaxGL(customerId, salesOrgId, hsnCodeId, materialMstId, countryId) {
        $http.get('accounts/voucher/GetHSNCodeTaxGL?customerId=' + customerId + '&salesOrganisationId=' + salesOrgId + '&hSNCodeId='
            + hsnCodeId + '&materialMasterId=' + materialMstId)
            .then(function (response) {
                $scope.HSNCodeTaxes = response.data;
                console.log('materialHSNCodeTaxes', $scope.materialHSNCodeTaxes);
            });
    };
    function GetCustomerInvoiceDetail(id) {
        $http.get('accounts/voucher/GetIntCustomerInvoiceDetail?customerInvoiceId=' + id)
            .then(function (response) {
                $scope.customerInvoiceDetails = response.data.Rows[0];
                console.log('customerInvoiceDetails', $scope.customerInvoiceDetails);
            });
    }

    $scope.getHSNTax = function (data) {
        GetHSNCodeTaxGL(data.CustomerId, data.SalesOrganizationId
            , data.HSNCodeId, data.MaterialMasterId);
    };
    function GetSaleType(id) {
        $http.get('accounts/voucher/GetSaleOrderInvoiceSaleTypeById?salesOrderInvoiceMasterId=' + id)
            .then(function (response) {
                $scope.saleOrderInvoice = response.data.Rows;
                $scope.saleOrderInvoiceSales[0].Id = $scope.customerInvoiceDetailIdguid;
                $scope.saleOrderInvoiceSales[0].COAICode = $scope.saleOrderInvoice[0].COAICode;
                $scope.saleOrderInvoiceSales[0].COAIText = $scope.saleOrderInvoice[0].COAIText;
                $scope.saleOrderInvoiceSales[0].CompanyGroupId = $scope.saleOrderInvoice[0].CompanyGroupId;
                $scope.saleOrderInvoiceSales[0].CompanyId = $scope.saleOrderInvoice[0].CompanyId;
                $scope.saleOrderInvoiceSales[0].Amount = $scope.saleOrderInvoice[0].Amount;
                $scope.saleOrderInvoiceSales[0].CrAmount = $scope.saleOrderInvoice[0].CrAmount;
                $scope.saleOrderInvoiceSales[0].DrAmount = 0.00;//$scope.saleOrderInvoice[0].DrAmount;
                $scope.saleOrderInvoiceSales[0].CurrencyId = $scope.saleOrderInvoice[0].CurrencyId;
                $scope.saleOrderInvoiceSales[0].DocDate = $scope.saleOrderInvoice[0].DocDate;
                $scope.voucher.DocDate = $scope.saleOrderInvoice[0].DocDate;
                $scope.saleOrderInvoiceSales[0].DocRefNo = $scope.saleOrderInvoice[0].DocRefNo;
                $scope.voucher.DocRefNo = $scope.saleOrderInvoice[0].DocRefNo;
                $scope.saleOrderInvoiceSales[0].GLGeneralInfoId = $scope.saleOrderInvoice[0].GLGeneralInfoId;
                $scope.saleOrderInvoiceSales[0].SaleOrderInvoiceMasterId = $scope.saleOrderInvoice[0].SaleOrderInvoiceMasterId;
                $scope.saleOrderInvoiceSales[0].SaleTypeName = $scope.saleOrderInvoice[0].SaleTypeName;
                $scope.saleOrderInvoiceSales[0].SalesTypeId = $scope.saleOrderInvoice[0].SalesTypeId;
                $scope.saleOrderInvoiceSales[0].CurrencyCode = $scope.saleOrderInvoice[0].CurrencyCode;
                //$scope.saleOrderInvoiceSales[0].CustomerInvoiceTax = [0];
                $scope.GetCurrencyExchangeRateList();
                console.log('saleOrderInvoiceSalestest', $scope.saleOrderInvoiceSales);
            });
    }

    $scope.salesguid = function () {
        $scope.customerInvoiceDetailIdguid = guid();
    };
    $scope.CheckParallelCurrencyBySelecteCurr = function (item) {
        $http({
            method: 'GET',
            url: 'currencies/CompanyParallelCurrency/CheckParallelCurrencyBySelecteCurrency?currencyid=' + item,
        }).then(function successCallback(response) {
            $scope.CheckParallelCurrencyBySelecteCurrency = response.data;
        });
    };

    $scope.GetCurrencyExchangeRateList = function () {
        $scope.additionalexchangerate = {};
        $scope.additionalexchangerate.FromCurrencyCode = $scope.voucher.CurrencyCode;
        $http({
            method: 'GET',
            url: 'currencies/CompanyParallelCurrency/ParallelExchangeRate?fromdate=' + $scope.voucher.PostingDate + '&currencyId=' + $scope.voucher.CurrencyId,
        }).then(function successCallback(response) {
            $scope.currencyexchangerate = response.data;
            if (response.data.Error == true) {
            }
            else {
                if ($scope.currencyexchangerate != null) {
                    $scope.additionalexchangerate.ToCurrency = $scope.currencyexchangerate[0].ToCurrency;
                }
                excurrencyRate($scope.currencyexchangerate);
            }
        });
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
        $scope.voucherDetailCurrency = [];
        $scope.voucherDetailCurrency.push(
            {
                ToCurrencyRate: ($scope.additionalexchangerate.ToCurrencyRate ? $scope.additionalexchangerate.ToCurrencyRate : 1),
                Id: null,
                FromCurrencyId: selectAndParallelCurrency(),
                ToCurrencyId: $scope.CurrencyParallel[0].CurrencyId,
                VoucherDetailId: null,
                ParallelCurrencyType: getct($scope.CheckParallelCurrencyBySelecteCurrency.ParallelCurrencyType),//Additional work March 20

                Index: 2
            })

        for (var i = 0; i < list.length; i++) {
            $scope.voucherDetailCurrency.push(
                {
                    ToCurrencyRate: list[i].ToCurrencyRate,
                    Id: null,
                    FromCurrencyId: list[i].FromCurrencyId,
                    ToCurrencyId: list[i].ToCurrencyId,
                    VoucherDetailId: null,
                    ParallelCurrencyType: list[i].ParallelCurrencyType,
                    Index: i
                }
            )
        }
        $scope.currencyParameter($scope.voucherDetailCurrency);
        tabwiseinvoicerow($scope.saleOrderInvoiceEdit[0]);
        tabwiseSalesinvoicerow($scope.saleOrderInvoiceSales[0]);
    };
    function selectAndParallelCurrency() {
        if ($scope.CurrencyParallel != null) {
            if ($scope.voucher.CurrencyId == $scope.CurrencyParallel[0].CurrencyId) {
                return $scope.CurrencyParallel[0].CurrencyId;
            }
            else if ($scope.voucher.CurrencyId == $scope.CurrencyParallel[1].CurrencyId) {
                return $scope.CurrencyParallel[0].CurrencyId;
            }
            else if ($scope.voucher.CurrencyId == $scope.CurrencyParallel[2].CurrencyId) {
                return $scope.CurrencyParallel[0].CurrencyId;
            }
            else {
                return $scope.voucher.CurrencyId;
            }
        }
    };
    $scope.currencyParameter = function (list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ParallelCurrencyType == 'CompanyCurrency') {
                $scope.baseCurrency = list[i].ToCurrencyRate;
                console.log('baseCurrency', $scope.baseCurrency);
            }
            else if (list[i].ParallelCurrencyType == 'CompanyGroupCurrency') {
                $scope.groupCurrency = list[i].ToCurrencyRate;
                console.log('groupCurrency', $scope.groupCurrency);
            }
            else {
                $scope.hardCurrency = list[i].ToCurrencyRate;
                console.log('hardCurrency', $scope.hardCurrency);
            }
        }
    };
    function guid() {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
                .toString(16)
                .substring(1);
        }
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
            s4() + '-' + s4() + s4() + s4();
    };

    function GetBaseRate(selectedcurrency, list) {//Selected to CompanyCurrency rate
        var rate = 0;
        for (var i = 0; i < list.length; i++) {
            if (list[i].FromCurrencyId == selectedcurrency) {
                rate = list[i].ToCurrencyRate;
                break;
            }
            //else
            //    rate = $scope.additionalexchangerate.ToCurrencyRate;//Additional work March 20
            //break;
        }
        return rate;
    };

    function GetPerallelRate(paracurrency, list, selectedcurrency) {//Selected to CompanyCurrency rate
        var rate = 0;
        var found = false;
        for (var i = 0; i < list.length; i++) {
            if (list[i].FromCurrencyId == paracurrency) {
                rate = list[i].ToCurrencyRate;
                found = true;
                break;
            }
        }
        if (found == false) {
            rate = 1;
        }//If
        return rate;
    };

    function GetRate(list, id, obj) { //
        obj.rate = 0; obj.fromcurrencyid = ''; obj.tocurrencyid = ''; obj.ParallelCurrencyType = ''
        for (var i = 0; i < list.length; i++) {
            if (list[i].FromCurrencyId == id) {
                obj.fromcurrencyid = list[i].FromCurrencyId;
                obj.tocurrencyid = list[i].ToCurrencyId;
                obj.ParallelCurrencyType = list[i].ParallelCurrencyType;
                break;
            }

            else if (list[i].ToCurrencyId == id) {
                obj.fromcurrencyid = list[i].FromCurrencyId;
                obj.tocurrencyid = list[i].ToCurrencyId;
                obj.ParallelCurrencyType = 'CompanyCurrency';
                break;
            }
        }
    };

    function SetBaseValue(objDetail, exchangeratelist, paracurrency, selectedCurrency, outobj) {
        var drcr = { amount: null, rate: null, fromcurrencyid: null, tocurrencyid: null, ParallelCurrencyType: null };
        var sel = GetBaseRate(selectedCurrency, exchangeratelist);
        var para = GetPerallelRate(paracurrency, exchangeratelist, selectedCurrency);
        var orate = sel / para;
        GetRate(exchangeratelist, paracurrency, drcr);
        outobj.dr = (orate * objDetail.DrAmount).toFixed(4);
        outobj.cr = (orate * objDetail.CrAmount).toFixed(4);
        // outobj.currencyrate = orate;
        if (drcr.ParallelCurrencyType == 'CompanyCurrency') {
            outobj.currencyrate = sel;
            outobj.fromcurrencyid = $scope.voucher.CurrencyId;
        }
        else if ($scope.voucher.CurrencyId == drcr.fromcurrencyid) {
            outobj.currencyrate = orate;
            outobj.fromcurrencyid = drcr.fromcurrencyid;
        }
        else {
            outobj.currencyrate = para;
            outobj.fromcurrencyid = drcr.fromcurrencyid;
        }
        outobj.tocurrencyid = drcr.tocurrencyid;
        outobj.parallelcurrencyid = paracurrency;
        outobj.ParallelCurrencyType = drcr.ParallelCurrencyType;
    };

    function tabwiseinvoicerow(obj) {
        var tabrow = {
            Id: null,
            dr: null,
            cr: null,
            currencyrate: null,
            tocurrencyid: null,
            fromcurrencyid: null,
            parallelcurrencyid: null,
            ParallelCurrencyType: null
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
            SetBaseValue(obj, $scope.voucherDetailCurrency, $scope.CurrencyParallel[i].CurrencyId, $scope.voucher.CurrencyId, tabrow);

            //CheckCurrencyExceptBase
            // #endregion
            // #region ************voucherDetailCurrencyrow push**************
            if (IsavailbleTab(obj.COAICode, tabrow.ParallelCurrencyType, $scope.invoiceDetailCurrencyrow) == false) {
                $scope.invoiceDetailCurrencyrow.push(
                    {
                        Id: guid(),
                        VoucherDetailId: $scope.customerInvoiceDetailIdguid,
                        TempId: $scope.customerInvoiceDetailIdguid,
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
                console.log('invoiceDetailCurrencyrow', $scope.invoiceDetailCurrencyrow);
            }
            else {
                var localupdate = {
                    Id: updatevoucercurrencyrow($scope.voucherDetailCurrencyrowupdate, obj.COAICode, obj.GLGeneralInfoId, tabrow.ParallelCurrencyType),
                    VoucherDetailId: $scope.customerInvoiceDetailIdguid,
                    TempId: $scope.customerInvoiceDetailIdguid,
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
                UpdateTab(localupdate, $scope.invoiceDetailCurrencyrow);
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

        //$scope.clearDetailRow();
        //$scope.total();

        //$scope.totalAmountCheck();
        //$scope.BaseCurrencytotal();
        if ($scope.indexdetails != -1 && $scope.CAction == 'Update') {
            //$scope.voucherDetailrow[$scope.indexdetails] = $scope.voucherDetail;
            $scope.indexdetails = -1;
            $scope.CAction = 'Add';
            //$scope.total();
            //$scope.BaseCurrencytotal();
            //$scope.totalAmountCheck();
            //$scope.clearDetailRow();
        }
    };

    function tabwiseSalesinvoicerow(obj) {
        var tabrow = {
            Id: null,
            dr: null,
            cr: null,
            currencyrate: null,
            tocurrencyid: null,
            fromcurrencyid: null,
            parallelcurrencyid: null,
            ParallelCurrencyType: null
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
            SetBaseValue(obj, $scope.voucherDetailCurrency, $scope.CurrencyParallel[i].CurrencyId, $scope.voucher.CurrencyId, tabrow);

            //CheckCurrencyExceptBase
            // #endregion
            // #region ************voucherDetailCurrencyrow push**************
            if (IsavailbleTab(obj.COAICode, tabrow.ParallelCurrencyType, $scope.voucherDetailCurrencyrow) == false) {
                $scope.voucherDetailCurrencyrow.push(
                    {
                        Id: guid(),
                        VoucherDetailId: $scope.customerInvoiceDetailIdguid,
                        TempId: $scope.customerInvoiceDetailIdguid,
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
                console.log('voucherDetailCurrencyrow', $scope.voucherDetailCurrencyrow);
            }
            else {
                var localupdate = {
                    Id: updatevoucercurrencyrow($scope.voucherDetailCurrencyrowupdate, obj.COAICode, obj.GLGeneralInfoId, tabrow.ParallelCurrencyType),
                    VoucherDetailId: $scope.customerInvoiceDetailIdguid,
                    TempId: $scope.customerInvoiceDetailIdguid,
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
                UpdateTab(localupdate, $scope.voucherDetailCurrencyrow);
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

        //$scope.clearDetailRow();
        //$scope.total();

        //$scope.totalAmountCheck();
        //$scope.BaseCurrencytotal();

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
            if (ob.TaxCodeId != null) {
                $scope.taxCodDataList.push(ob);
            }
            console.log('$scope.taxCodDataList', $scope.taxCodDataList);
            //}
            //}
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

        $scope.taxCodDataList = [];// $scope.saleOrderInvoiceSales[index]['CustomerInvoiceTax'];

        angular.element(document.querySelector('#texCodePopUp')).modal('show');
    }

    $scope.closeTaxCodePopUp = function () {
        angular.element(document.querySelector('#texCodePopUp')).modal('hide');
    }
    // #endregion
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

    // #endregion
    // #region OnVendor Select Enable Tax
    $scope.enabletax = function () {
        if (baseService.isUndefinedOrNull($scope.getCustomerDetails)) {
            return true
        }
        else
            return false
    }
    // #endregion

    // #region *******InvoiceTaxPush******
    $scope.vendorInvoiceTaxes = [];

    $scope.vendorInvoiceTaxPush = function () {
        var vendorInvoiceTaxesLength = $scope.vendorInvoiceTaxes.length;
        for (var i = 0; i < $scope.taxCodDataList.length; i++) {
            $scope.taxCodDataList[i].InvoiceDetailOppositEntryId = $scope.setIndex;
            if (vendorInvoiceTaxesLength > 0) {
                var aabc = $scope.saleOrderInvoiceSales.CustomerInvoiceTax;
                $scope.saleOrderInvoiceSales.CustomerInvoiceTax.push($scope.taxCodDataList[i]);
            }
            else {
                $scope.saleOrderInvoiceSales[0]['CustomerInvoiceTax'] = $scope.taxCodDataList;
            }
        }
        vendorInvoiceTaxesLength = 0;
        $scope.saleOrderInvoiceSales[$scope.setTaxVoucherDetailIndex].taxCategoryStatus = false;
        console.log('saleOrderInvoiceSalesTax', $scope.saleOrderInvoiceSales);
    };

    $scope.checkNarrationMsg = '';
    $scope.CheckNarration = function () {
        if ($scope.voucher.Narration != null) {
            $scope.checkNarrationMsg = '';
            return true;
        } else {
            $scope.pop('error', 'Narration do not Null allow');
            return false
        }
    }

    $scope.Post = function () {
        try {
            $scope.voucher.FiscalYearId = $scope.fiscalYearInfo.FiscalYearId;
            $scope.voucher.FiscalYearPeriodId = $scope.fiscalYearInfo.FiscalYearPeriodId;
            if ($scope.intSaleOrderInvoicePostForm.$valid && $scope.CheckNarration()) {
                if ($scope.Action == "Post") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'voucher': $scope.voucher, 'voucherDetails': $scope.saleOrderInvoiceSales, 'customerInvoice': $scope.saleOrderInvoiceEdit[0],
                            'customerInvoiceDetails': $scope.customerInvoiceDetails, 'customerDetailCurrencies': $scope.invoiceDetailCurrencyrow,
                            'voucherDetailCurrencies': $scope.voucherDetailCurrencyrow, 'baseCurrencyrate': $scope.baseCurrency,
                            'groupCurrencyrate': $scope.groupCurrency,
                            'hardCurrencyrate': $scope.hardCurrency
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            ClearFields();
                            //$scope.getData();
                        }
                    });
                    return true;
                }
            }
        } catch (e) {
            ShowResult(e, 'failure')
        }
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
        $scope.voucher.Active = true;
        $scope.voucher.VoucherDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucher.PostingDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucher.DocDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucher.VoucherTypeId = "1";
        $scope.fiscalYearInfo = null;
        $scope.currencyexchangerate = [];
        $scope.customerInvoice = {};
        $scope.saleOrderInvoiceSales = [];
        $scope.saleOrderInvoiceEdit = [];
        $scope.customerInvoiceDetails = {};
        $scope.saleOrderInvoice = [];
        $scope.voucherDetailCurrencyrow = [];
        $scope.invoiceDetailCurrencyrow = [];
        $scope.taxcodedata = [];
        $scope.taxCodDataList = [];
        $scope.customerInvoiceDetailIdguid = null;
        $scope.getPostingFiscalYearPeriod($scope.voucher.PostingDate);
        $('.datepicker').datepicker({
            format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
        });
        $location.path('UPanel/sales-order-edit-invoice');
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
        else if ($scope.invoiceCustomerForm4.$invalid) {
            $scope.setTab(4);
        }
    }
}