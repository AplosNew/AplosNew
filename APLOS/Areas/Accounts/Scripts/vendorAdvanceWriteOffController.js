'use strict';
vendorAdvanceWriteOffController.$inject = ['cboService', 'commonMessage', '$window','$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function vendorAdvanceWriteOffController(cboService, commonMessage, $window,$scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Vendor Advanced Set-off';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.voucherDetailCurrency = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.isWriteOff = true;
    $scope.hideSource = true;
    $scope.url = 'accounts/Advance';
    $scope.listUrl = $scope.url + '/GetVendorAdvanceWriteOffList';
    $scope.postUrl = $scope.url + '/PostVendorAdvanceWriteOff';

    $scope.deleteUrl = $scope.url + "/DeleteCustomerAdvanceWriteOff";

    $scope.exportgriddataUrl = 'GridReports/ExcelExportJson';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $scope.partyType = 'Vendor';
    $scope.isAdvance = true;
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });

    baseService.init($scope.listUrl, null, null, "DESC", "PostingDate", "VoucherNo");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.voucherList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchByList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            'name': 'Vendor Code',
            'value': 'PartyCode'
        },
        {
            'name': 'Vendor Name',
            'value': 'PartyName'
        },
        {
            "name": "Ordering Vendor",
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
        },
        {
            "name": "Status",
            "value": "Status"
        }
    ];
    $scope.getCboVoucherTypeAdvanceGivenWriteOffList = function () {
        cboService.getCboVoucherTypeAdvanceGivenWriteOffList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.advance.PostingDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
                $scope.advance.DocDate = $scope.advance.PostingDate;
            }
        });
    }
    $scope.getCboVoucherTypeAdvanceGivenWriteOffList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.advance.VoucherTypeId = data.Value;
        $scope.advance.PostingDate = $filter('dateFiltering')(data.LastPostingDate);
        $scope.advance.DocDate = $scope.advance.PostingDate;
    };


    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
       
        if (!baseService.isUndefinedOrNull($routeParams.advanceId)) {
            getByParams($routeParams.advanceId);
        }
    });
    cboService.getCboEntityByPlant(null, null, '', function (result) {
        $scope.entityList = result;
    });
    function getByParams(advanceId) {
        $http.get('Accounts/Advance/GetAdvanceForWriteOff?advanceId=' + advanceId)
            .then(function (response) {
                var party = response.data;
                $scope.advance.DocRefNo = party.DocRefNo;
                $scope.advance.EntityId = party.EntityId;
                $scope.advance.PartyType = party.PartyType;
                $scope.advance.PartyId = party.PartyId;
                $scope.advance.PartyName = party.PartyCode + " - " + party.PartyName;
                $scope.advance.PartyPlantId = party.PartyPlantId;
                $scope.advance.GLGeneralInfoId = party.DownPaymentGLId;
                $scope.advance.GLGeneralInfoCode = party.DownPaymentGLCode;
                $scope.advance.GLGeneralInfoName = party.DownPaymentGLName;
                $scope.advance.BudgetMasterId = party.DownPaymentBudgetId;
                $scope.advance.BudgetCode = party.DownPaymentBudgetCode;
                $scope.advance.BudgetName = party.DownPaymentBudgetName;
                $scope.advance.ActivityId = party.DownPaymentActivityId;
                $scope.advance.ActivityCode = party.DownPaymentActivityCode;
                $scope.advance.ActivityName = party.DownPaymentActivityName;

                // Party plant list calling.
                $scope.getPartyPlantList($scope.advance.PartyId, true);

                $http.get('Accounts/Advance/GetAvilabeVendorAdvance?partyId=' + $scope.advance.PartyId + '&advanceId=' + advanceId)
                    .then(function (response) {
                        var data = response.data;
                        data.TrnType = "Cr";
                        var getRow = null;
                        getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Cr", "DocRefNo": data.DocRefNo });
                        if (getRow.length === 0) {
                            data.Amount = data.Receivable;
                            data.WriteOff = data.Received;
                            data.Advilable = data.Balance;
                            $scope.voucherDetailList.push(data);
                            if ($scope.voucherDetailList.length > 0)
                                $scope.isReadOnly = true;
                            else
                                $scope.isReadOnly = false;
                        }
                        else {
                            ShowResult(data.DocRefNo + " already  Exist", "failure", "customerAdvancePopUp");
                        }
                        $scope.advance.CurrencyId = $scope.selectBaseCurrency();
                    });
            });
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    

    $scope.advance = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PartyId: null,
        AdvanceId: null,
        AdvanceDetailId: null,
        PartyName: null,
        PartyPlantName: null,
        PartyGLGeneralInfoId: null,
        GLGeneralInfoId: null,
        CurrencyId: null,
        CurrencyCode: null,
        VoucherTypeId: null,
        PartyType: 'Vendor',
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter('dateFiltering')(Date.now()),
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        IsExcludingTax: false,
        VoucherDetailId: null,
        Amount: 0,
        BaseOnDueDate: $filter('dateFiltering')(Date.now()),
        BaseNoOfDays: null,
        PaymentTermId: null,
        Narration: null,
        Remarks: null,
        BankName: null,
        BankAccountNumber: null,
        BankGL: null,
        BankGLGeneralInfoId: null,
        AdvancePostingDate: null,
        SettlementType: "SetOff"
        //PaymentSource: 'Bank'
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
    cboService.getEnumCbo("Enum/GetCboRoundingType", function (result) {
        $scope.roundingTypeList = result;
        $scope.advance.RoundingType = $scope.roundingTypeList[0].Value;
    });
    $scope.searchVendorInvoiceList = [
        {
            'name': 'Posting Date',
            'value': 'PostingDate'
        },
        {
            'name': 'Doc Date',
            'value': 'DocDate'
        },
        {
            'name': 'Vendor Code',
            'value': 'PartyCode'
        },
        {
            'name': 'Vendor Name',
            'value': 'PartyName'
        },
        {
            'name': 'Currency',
            'value': 'Currency'
        }
    ];

    $scope.customerInvoice = [];
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.advance = $scope.customerInvoices[$scope.index];
        $scope.advance.DocDate = $filter('dateFiltering')($scope.advance.DocDate);
        $scope.advance.VoucherDate = $filter('dateFiltering')($scope.advance.VoucherDate);
        $scope.advance.PostingDate = $filter('dateFiltering')($scope.advance.PostingDate);
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
    };

    function validationAddGL(obj) {
        try {
            obj.FiscalYearText = $('#FiscalYear option:selected').text();
            obj.FiscalYearPeriodText = $('#FiscalYearPeriod option:selected').text();

            if (baseService.isUndefinedOrNull(obj.COAICode)) {
                throw 'Please Select GL!!';
            }
            if ($scope.advance.Narration === '' || $scope.advance.Narration === null) {
                throw 'Please input narration!!';
            }
            if ($scope.advance.DocRefNo === '' || $scope.advance.DocRefNo === null) {
                throw 'Please input DocRefNo!!';
            }
            if (obj.DrAmount === 0 && obj.CrAmount === 0) {
                throw 'Please Input Devit Amount or Credit Amount!!';
            }
        } catch (e) {
            throw ShowResult(e, 'failure');
        }
    }

    $scope.checkCrAndDrEquealMsg = '';
    $scope.checkCrAndDrEqueal = function () {
        if ($scope.Crtotal === $scope.customerInvoice.Amount) {
            $scope.checkCrAndDrEquealMsg = '';
            return true;
        } else {
            $scope.pop('error', 'Debit and Credit is not equeal');
            return false;
        }
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = '';
        if (new Date($scope.advance.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = 'Doc date must be below or equal to current Date!';
        }
        else $scope.invalidDocDate = false;
        return manualValidation('div_DocDate', $scope.invalidDocDate, msg);
    };

    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = '';
        if (new Date($scope.advance.PostingDate) > new Date()) {
            msg = 'Posting date must be below or equal to current Date!';
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.advance.PostingDate) < new Date($scope.advance.DocDate)) {
            msg = 'Posting date must be below or equal to Doc Date!';
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        } else {
            $scope.invalidPostingDate = false;
        }
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if (new Date($scope.voucherDetailList[i].PostingDate) > new Date($scope.advance.PostingDate)) {
                msg = 'Posting date must be below or equal to payable of ' + $scope.voucherDetailList[i].DocRefNo;
                $scope.invalidPostingDate = true;
                break;
            }
            else {
                $scope.invalidPostingDate = false;
            }
        }
        return manualValidation('div_PostingDate', $scope.invalidPostingDate, msg);
    };

    $scope.invoice1stCurrencyId = "";
   
    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult('Please select Currency!', 'failure');
            return true;
        }
        var vdetailDr = $filter('filter')($scope.voucherDetailList, { TrnType: 'Dr' });
        if (vdetailDr.length === 0 && $scope.advance.SettlementType === 'SetOff') {
            ShowResult('Please add Invoice!', 'failure');
            return true;
        }
        if (new Date($scope.advance.AdvancePostingDate) > new Date($scope.advance.PostingDate)) {
            ShowResult('Posting is not possible before Advance!', 'failure');
            return true;
        }
        if ($scope.advance.PaymentSource === 'Bank' && $scope.advance.SettlementType === 'Return' && $scope.advance.BankMasterId == null) {
            ShowResult("Please select Bank!", "failure");
            return true;
        }
        if ($scope.advance.PaymentSource === 'Cash' && $scope.advance.SettlementType === 'Return' && $scope.advance.CashMasterId == null) {
            ShowResult("Please select Bank!", "failure");
            return true;
        }
        $scope.invoice1stCurrencyId = "";
        if ($scope.voucherDetailList.length > 0) {
            $scope.invoice1stCurrencyId = $scope.voucherDetailList[0].CurrencyId;
            for (var i = 0; i < $scope.voucherDetailList.length; i++) {
                if (new Date($scope.voucherDetailList[i].PostingDate) > new Date($scope.advance.PostingDate)) {
                    ShowResult('Posting is not possible before Invoice!', 'failure');
                    return true;
                }
                if ($scope.invoice1stCurrencyId != $scope.voucherDetailList[i].CurrencyId) {
                    ShowResult('Please add Same Currency Invoice!', 'failure');
                    return true;
                }
            };
        }
        
        $scope.DrAmountSubTotal = $filter('sumByKey')($filter('filter')($scope.voucherDetailList), 'DrAmount');
        if (parseFloat($scope.advance.AdvanceAmount) * parseFloat($scope.advance.CompanyCurrencyRate) < parseFloat($scope.DrAmountSubTotal)) {
            ShowResult("Invoice  Amount should not exceed Advance Amount.", "failure");
            return true;
        }
        
        if ($scope.partyType === 'Customer') {
            if ($scope.advance.PartyId === null) {
                ShowResult('Please select vendor!', 'failure');
                return true;
            }
            if (parseFloat($scope.advance.Amount) === 0) {
                ShowResult('Advance Amount must greater than 0!', 'failure');
                return true;
            }
        }

        return false;
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $scope.checkPostingDate();
        if ($scope.form0.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/Advance/InsertVendorAdvanceWriteOff',
                    data: {
                        'advanceVM': $scope.advance,
                        'advanceDetailVMList': $scope.voucherDetailList,
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        Clear();
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'accounts/Advance/UpdateAdvanceWriteOff',
                    data: {
                        'advanceVM': $scope.advance,
                        'advanceDetailVMList': $scope.voucherDetailList,
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
                        Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    };
    $scope.clear = function () {
        Clear();
    }
    function Clear() {
        $scope.Action = 'Save';
        $scope.advance = {};
        $scope.advance.Active = true;
        $scope.getCboVoucherTypeAdvanceGivenWriteOffList();
        $scope.advance.VoucherDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucherDetailCurrencyList = [];
        $scope.voucherDetailList = [];
        $scope.advance.SettlementType = "SetOff";
        //$scope.advance.PaymentSource = "Bank";
    }

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.advance.PostingDate) && !baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            $http({
                method: 'GET',
                url: 'currencies/ExchangeRate/ParallelExchangeRate?fromdate=' + $scope.advance.PostingDate + '&currencyId=' + $scope.advance.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    //*********************** Invoice PopUp Start *************************************
    $scope.invoiceSearchList = [];
    $scope.invoiceList = [];
    $scope.invoiceSearch = [];
    $scope.invoiceSelectedIndex = -1;
    $scope.invoiceParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "VoucherNo",
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showInvoicePopUp = function (partyId) {
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.invoiceList = [];
            ShowResult("Please select Vendor.", "failure");
            return;
        }
        else {
            $scope.compareCurrencyId = $scope.advance.CurrencyId;
            $scope.invoiceParameters.partyId = partyId;
            $scope.getInvoiceData = function (pageno) {
                baseService.paginationBase("accounts/Invoice/GetVendorAvailableInvoiceList", pageno, $scope.invoiceParameters)
                    .then(function (response) {
                        $scope.invoiceList = response.Rows;
                        $scope.invoiceParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.invoiceSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.invoiceList, $scope.invoiceSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector("#vendorInvoicePopUp")).modal("show");
            $scope.getInvoiceData();
        }
    };

    $scope.closeInvoicePopUpselected = function () {
        angular.forEach($scope.invoiceList, function (data, i) {
            if (data.Active === true) {
                data.TrnType = "Dr";
                data.PartyPlantName = data.PartyPlantName;
                var getRow = null;
                getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "DocRefNo": data.DocRefNo, "InvoiceDetailId": data.InvoiceDetailId });
                if (getRow.length === 0) {
                    data.Amount = data.Receivable;
                    data.WriteOff = data.Received;
                    data.Advilable = data.Balance;
                    data.DrAmount = data.Balance;

                    if (data.CurrencyCode === $scope.advance.CurrencyCode) {
                        if (data.CompanyCurrencyRate < $scope.advance.CompanyCurrencyRate) {
                            data.ExchangeAmount = Math.abs(data.DrAmount * ($scope.advance.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
                            data.ExchangeType = "ExchangeLoss";
                        }
                        else if (data.CompanyCurrencyRate > $scope.advance.CompanyCurrencyRate) {
                            data.ExchangeAmount = Math.abs(data.DrAmount * (data.CompanyCurrencyRate - $scope.advance.CompanyCurrencyRate)).toFixed(2);
                            data.ExchangeType = "ExchangeGain";
                        }
                        else {
                            data.ExchangeAmount = 0;
                            data.ExchangeType = null;
                        }
                    }

                    else {
                        data.ExchangeAmount = 0;
                        data.ExchangeType = null;
                    }

                    $scope.voucherDetailList.push(data);
                    if ($scope.voucherDetailList.length > 0)
                        $scope.isReadOnly = true;
                    else
                        $scope.isReadOnly = false;
                    angular.element(document.querySelector("#vendorInvoicePopUp")).modal("hide");
                }
                else {
                    ShowResult(data.DocRefNo + " already  Exist", "failure", "vendorInvoicePopUp");
                }
            }
        });
    };

    $scope.closeInvoicePopUp = function () {
        angular.element(document.querySelector("#vendorInvoicePopUp")).modal("hide");
    };

    //*********************** Invoice PopUp End ***************************************

    //*********************** Advance PopUp Start *************************************
    $scope.advanceSearchList = [];
    $scope.advanceDataList = [];
    $scope.advanceSearch = [];
    $scope.advanceSelectedIndex = -1;
    $scope.advanceParameters = {
        limit: 10,
        offset: 0,
        order: "DESC",
        sort: "VoucherNo",
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.advanceSearchList = [
        {
            'Text': 'Voucher No',
            'Value': 'VoucherNo'
        },
        {
            'Text': 'Vendor Code',
            'Value': 'PartyCode'
        },
        {
            'Text': 'Vendor Name',
            'Value': 'PartyName'
        },
        {
            'Text': 'Location',
            'Value': 'PartyPlantName'
        },
        {
            'Text': 'Posting Date',
            'Value': 'PostingDate'
        }
    ];

    $scope.showAdvancePopUp = function (partyId, partyPlantId) {
        $scope.compareCurrencyId = $scope.advance.CurrencyId;
        $scope.getAdvanceData = function (pageno) {
            baseService.paginationBase("accounts/Advance/GetVendorAvilabeAdvanceList", pageno, $scope.advanceParameters)
                .then(function (response) {
                    $scope.advanceDataList = response.Rows;
                    $scope.advanceParameters.total_count = response.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#vendorAdvancePopUp")).modal("show");
        $scope.getAdvanceData();
        //}
    };
    $scope.totalAdvanceVendorWise = function (partyId) {
        $scope.advance.TotalAdvanceAmount = Math.round(($filter('sumByKey')($filter('filter')($scope.advanceDataList, { 'PartyId': partyId }), 'Balance')) * 100 + Number.EPSILON) / 100;
    }
    $scope.closeAdvancePopUpSelected = function (data) {
        data.TrnType = "Cr";
        $scope.advance.AdvanceId = data.AdvanceId;
        $scope.advance.AdvanceDetailId = data.AdvanceDetailId;
        $scope.advance.CompanyId = data.CompanyId;
        $scope.advance.PlantId = data.PlantId;
        $scope.advance.EntityId = data.EntityId;
        $scope.advance.PartyType = data.PartyType;
        $scope.advance.WriteOff = data.Received;
        $scope.advance.VoucherNo = data.VoucherNo;
        $scope.advance.AdvanceAmount = data.Balance;
        $scope.advance.PartyId = data.PartyId;
        $scope.advance.PartyPlantId = data.PartyPlantId;
        $scope.advance.PartyName = data.PartyName;
        $scope.advance.PartyPlantName = data.PartyPlantName;
        $scope.advance.CurrencyId = data.CurrencyId;
        $scope.advance.CurrencyCode = data.CurrencyCode;
        $scope.advance.CompanyCurrencyRate = data.CompanyCurrencyRate;
        $scope.advance.AdvancePostingDate = $filter('dateFiltering')(data.PostingDate);
        $scope.totalAdvanceVendorWise($scope.advance.PartyId);
        $scope.GetCurrencyExchangeRateList();
        $scope.voucherDetailList = [];
        angular.element(document.querySelector("#vendorAdvancePopUp")).modal("hide");
    };

    $scope.closeAdvancePopUp = function () {
        angular.element(document.querySelector("#vendorAdvancePopUp")).modal("hide");
    };

    $scope.removeRow = function (index) {
        var voucherId = $scope.voucherDetailList[index].AdvanceId;
        $scope.voucherDetailList.splice(index, 1);
        var i = $scope.voucherDetailCurrencyList.length;
        while (i--) {
            if ($scope.voucherDetailCurrencyList[i]["AdvanceId"] === voucherId) {
                $scope.voucherDetailCurrencyList.splice(i, 1);
            }
        }
    };
    //*********************** Advance PopUp End *************************************

    $scope.report = function (voucherId) {
        location.href = 'accounts/advance/ReportVendorAdvanceWriteOff?voucherId=' + voucherId;
    };


    $scope.exchangeGainLossAmount = function (data) {
        $scope.DrAmountSubTotal = $filter('sumByKey')($filter('filter')($scope.voucherDetailList), 'DrAmount');
        if (parseFloat($scope.advance.AdvanceAmount) * parseFloat($scope.advance.CompanyCurrencyRate) < parseFloat($scope.DrAmountSubTotal)) {
            data.DrAmount = parseFloat($scope.advance.AdvanceAmount) * parseFloat($scope.advance.CompanyCurrencyRate);
            ShowResult("Invoice  Amount should not exceed Advance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }

        var balance = parseFloat(data.Advilable), dramount = parseFloat(data.DrAmount);
        if (dramount > balance) {
            data.DrAmount = data.Balance;
            ShowResult("Invoice Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        if (data.CurrencyCode === $scope.advance.CurrencyCode) {
            if (data.CompanyCurrencyRate < $scope.advance.CompanyCurrencyRate) {
                data.ExchangeAmount = Math.abs(data.DrAmount * ($scope.advance.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
                data.ExchangeType = "ExchangeLoss";
            }
            else if (data.CompanyCurrencyRate > $scope.advance.CompanyCurrencyRate) {
                data.ExchangeAmount = Math.abs(data.DrAmount * (data.CompanyCurrencyRate - $scope.advance.CompanyCurrencyRate)).toFixed(2);
                data.ExchangeType = "ExchangeGain";
            }  
            else {
                data.ExchangeAmount = 0;
                data.ExchangeType = null;
            }
        }
       
        else {
            data.ExchangeAmount = 0;
            data.ExchangeType = null;
        }

    };


    $scope.advanceId = null;
    $scope.confirmPost = function (advanceId) {
        $scope.advanceId = advanceId;
        $scope.message_confirmation = 'Are you sure to Post?';
        angular.element(document.querySelector('#confirmPostPopUp')).modal('show');
    };

    $scope.post = function (advanceId) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "advanceId": advanceId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.Clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };


    $scope.delete = function (voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "voucherId": voucherId  /*, "voucherId": voucherId*/
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.Clear();
                $scope.voucherId = null;
                // $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.confirmDelete = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

    $scope.changeSettlementType = function () {
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
                ShowResult("Please select currency!", "failure", "bankPopUp");
                return;
            }
            if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
                ShowResult("Bank GL not found!", "failure", "bankPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(bank.BudgetMasterId)) {
                ShowResult("Bank budget not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(bank.CurrencyId)) {
                ShowResult("Bank transaction currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.advance.AccountTitle = bank.AccountTitle;
                $scope.advance.BankName = bank.AccountTitle;
                $scope.advance.BankMasterId = bank.BankMasterId;
                setBankGL(bank);
            }
        }
        $scope.hideBankPopUp();
    };

    function setBankGL(bank) {
        $scope.advance.BankCurrencyId = bank.CurrencyId;
        $scope.advance.GLGeneralInfoId = bank.GLGeneralInfoId;
        $scope.advance.GLGeneralInfoCode = bank.GLGeneralInfoCode;
        $scope.advance.GLGeneralInfoName = bank.GLGeneralInfoName;
        $scope.advance.BudgetMasterId = bank.BudgetMasterId;
        $scope.advance.BudgetCode = bank.BudgetCode;
        $scope.advance.BudgetName = bank.BudgetName;
        $scope.advance.ActivityId = bank.ActivityId;
        $scope.advance.ActivityCode = bank.ActivityCode;
        $scope.advance.ActivityName = bank.ActivityName;
        $scope.advance.InvoiceDetailId = bank.BankMasterId;
        $scope.advance.TrnType = "Cr";
        //$scope.advance.CompanyCurrencyRate = 1;
    }

    $scope.clearBankPopUp = function () {
        $scope.isBankAmount = false;
        $scope.advance.AccountTitle = null;
        $scope.advance.BankName = null;
        $scope.advance.BankMasterId = null;
        $scope.advance.BankCurrencyId = null;
        $scope.advance.CashMasterId = null;
        $scope.advance.CashName = null;
        $scope.advance.CashCurrencyId = null;
        $scope.advance.GLGeneralInfoId = null;
        $scope.advance.GLGeneralInfoCode = null;
        $scope.advance.GLGeneralInfoName = null;
        $scope.advance.BudgetMasterId = null;
        $scope.advance.BudgetCode = null;
        $scope.advance.BudgetName = null;
        $scope.advance.ActivityId = null;
        $scope.advance.ActivityCode = null;
        $scope.advance.ActivityName = null;

    };

    $scope.closeCashPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult('Please select currency!', 'failure', 'cashPopUp');
            return;
        }
        if ($scope.cashIndex !== -1) {
            var cash = $scope.cashList[$scope.cashIndex];
            if (baseService.isUndefinedOrNull(cash.GLGeneralInfoId)) {
                ShowResult('Cash GL not found!', 'failure', 'cashPopUp');
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(cash.BudgetMasterId)) {
                ShowResult('Cash budget not found!', 'failure', 'cashPopUp');
                return;
            }
            else if (baseService.isUndefinedOrNull(cash.CurrencyId)) {
                ShowResult('Cash transaction currency not found!', 'failure', 'cashPopUp');
                return;
            }
            else {
                $scope.advance.CashMasterId = cash.Id;
                $scope.advance.CashName = cash.CashName;
                setCashGL(cash);
            }
        }
        $scope.hideCashPopUp();
    };

    function setCashGL(cash) {
        $scope.advance.CashCurrencyId = cash.CurrencyId;
        $scope.advance.GLGeneralInfoId = cash.GLGeneralInfoId;
        $scope.advance.GLGeneralInfoCode = cash.GLGeneralInfoCode;
        $scope.advance.GLGeneralInfoName = cash.GLGeneralInfoName;
        $scope.advance.BudgetMasterId = cash.BudgetMasterId;
        $scope.advance.BudgetCode = cash.BudgetCode;
        $scope.advance.BudgetName = cash.BudgetName;
        $scope.advance.ActivityId = cash.ActivityId;
        $scope.advance.ActivityCode = cash.ActivityCode;
        $scope.advance.ActivityName = cash.ActivityName;
        $scope.advance.InvoiceDetailId = cash.Id;
        $scope.advance.TrnType = "Cr";
        //$scope.advance.CompanyCurrencyRate = 1;
    }

    $scope.clearCashPopUp = function () {
        $scope.clearBankPopUp();
    };

    //$scope.VendorAdvanceReportExcel = function () {

    //    var gridObj1 = $("#vendorAdvancePopUp").data("ejGrid");
    //    var data1 = gridObj1.model.dataSource();
    //    $http({
    //        method: 'POST',
    //        url: $scope.exportgriddataUrl,
    //        //data: { 'data': data1 }
    //        data: JSON.stringify(data1)
    //    }).then(function successCallback(response) {
    //        if (response.data.Error == true) {
    //            // ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');
    //        }
    //        else {

    //            location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
    //        }
    //    });
    //}

    $scope.VendorAdvanceReportExcel = function () {
        try {
            $scope.fileName = "VendorAdvanceReport.xlsx";
            $http({
                method: 'POST',
                url: $scope.url + "/GetVendorAdvanceReport",
                data: { 'plantId': $window.plantId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    //$window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

}