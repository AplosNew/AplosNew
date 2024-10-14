"use strict";
customerAdvanceWriteOffController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "toaster", "$controller"];
function customerAdvanceWriteOffController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = "Customer Advance Set-off";
    $scope.hideSource = true;
    $scope.url = 'accounts/Advance';
    $scope.listUrl = $scope.url + '/GetCustomerAdvanceWriteOffList';
    $scope.parkUrl = $scope.url + '/ParkCustomerAdvanceWriteOff';
    $scope.parkUrlMultiAdvance = $scope.url + '/ParkMultiCustomerAdvanceWriteOff';
    $scope.updateUrl = $scope.url + '/UpdateCustomerAdvanceWriteOff';
    $scope.postUrl = $scope.url + '/PostCustomerAdvanceWriteOff';
    $scope.reportUrl = $scope.url + '/ReportCustomerAdvanceWriteOff?voucherId=';
    $scope.deleteUrl = $scope.url + "/DeleteCustomerAdvanceWriteOff";


    $scope.partyType = "Customer";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherDetailCurrency = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.voucherDetailListNew = [];
    $scope.isWriteOff = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.partyType = "Customer";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
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

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, '', function (result) {
                $scope.entityList = result;
            });
        if (!baseService.isUndefinedOrNull($routeParams.advanceId)) {
            getByParams($routeParams.advanceId);
        }
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

                $http.get('Accounts/Advance/GetAvilabeCustomerAdvance?partyId=' + $scope.advance.PartyId + '&advanceId=' + advanceId)
                    .then(function (response) {
                        var data = response.data;
                        data.TrnType = "Dr";
                        var getRow = null;
                        getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "DocRefNo": data.DocRefNo });
                        if (getRow.length === 0) {
                            data.Amount = data.Receivable;
                            data.WriteOff = data.Received;
                            data.Advilable = data.Balance;
                            $scope.voucherDetailList.push(data);
                            $scope.setDrExchangeRate(data);
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
        PartyName: null,
        PartyGLGeneralInfoId: null,
        GLGeneralInfoId: null,
        CurrencyId: null,
        VoucherTypeId: null,
        PartyType: 'Customer',
        SettlementType: 'SetOff',
        PaymentSource: 'Invoice',
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        BankTransactionDate: null,
        BankReferenceNo: null,
        DocDate: null,
        DocRefNo: null,
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
        DocDate: $filter("date")(Date.now(), "dd-MMM-yyyy"),
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
    $scope.advanceNew = {
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
        SettlementType: 'SetOff',
        PaymentSource: 'Bank',
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        BankTransactionDate: null,
        BankReferenceNo: null,
        DocDate: null,
        DocRefNo: null,
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
        BankGLGeneralInfoId: null,
        DiscountAmount: 0,
        ExchangeAmount: 0,
        ExchangeType:null
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
    $scope.changeExhangeType = function (type) {
        if (type === 'ExchangeGain') {
            $scope.advanceNew.ExchangeType = 'ExchangeGain';
        }
        if (type === 'ExchangeLoss') {
            $scope.advanceNew.ExchangeType = 'ExchangeLoss';
        }
        $scope.calBaseAmount();
    };
    $scope.searchByList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
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
        },
        {
            "name": "Status",
            "value": "Status"
        }
    ];

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.advance = $scope.voucherList[$scope.index];
        $scope.advance.DocDate = $filter("dateFiltering")($scope.advance.DocDate);
        $scope.advance.VoucherDate = $filter("dateFiltering")($scope.advance.VoucherDate);
        $scope.advance.PostingDate = $filter("dateFiltering")($scope.advance.PostingDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.exchangeGainLossList = [];
    $http.get("accounts/ExchangeGainLoss/GetExchangeGainLoss")
        .then(function successCallback(response) {
            $scope.exchangeGainLossList = response.data;
        },
            function errorCallback(response) {
                ShowResult(response, "failure");
            });

    $scope.checkDrAmount = function () {
        if ($scope.voucherDetail.CrAmount > 0) {
            $scope.voucherDetail.DrAmount = 0;
        }
    };

    function validationAddGL(obj) {
        try {
            obj.FiscalYearText = $("#FiscalYear option:selected").text();
            obj.FiscalYearPeriodText = $("#FiscalYearPeriod option:selected").text();

            if (baseService.isUndefinedOrNull(obj.COAICode)) {
                throw "Please Select GL!!";
            }
            if ($scope.advance.Narration === "" || $scope.advance.Narration === null) {
                throw "Please input narration!!";
            }
            if ($scope.advance.DocRefNo === "" || $scope.advance.DocRefNo === null) {
                throw "Please input DocRefNo!!";
            }
            if (obj.DrAmount === 0 && obj.CrAmount === 0) {
                throw "Please Input Devit Amount or Credit Amount!!";
            }
        } catch (e) {
            throw ShowResult(e, "failure");
        }
    }

    $scope.checkCrAndDrEquealMsg = "";
    $scope.checkCrAndDrEqueal = function () {
        if ($scope.Crtotal === $scope.customerInvoice.Amount) {
            $scope.checkCrAndDrEquealMsg = "";
            return true;
        } else {
            $scope.pop("error", "Debit and Credit is not equeal");
            return false;
        }
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.advance.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.advance.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.advance.PostingDate) < new Date($scope.advance.DocDate)) {
            msg = "Posting date must be below or equal to Doc Date!";
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        } else if ($scope.voucherDetailList.length > 0) {
            for (var i = 0; i < $scope.voucherDetailList.length; i++) {
                if (new Date($scope.voucherDetailList[i].PostingDate) > new Date($scope.advance.PostingDate)) {
                    msg = "Posting date must be below or equal to payable of " + $scope.voucherDetailList[i].VoucherNo;
                    $scope.invalidPostingDate = true;
                    break;
                }
                else {
                    $scope.invalidPostingDate = false;
                }
            }
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.invalidEntity = false;
    $scope.entityValidation = function () {
        $scope.invalidEntity = baseService.isUndefinedOrNull($scope.advance.EntityId);
        return manualValidation("div_entity", $scope.invalidEntity, "Entity is required.");
    };

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return true;
        }
        if ($scope.partyType === "Customer") {
            if ($scope.advance.PartyId === null) {
                ShowResult("Please select Customer!", "failure");
                return true;
            }
            if ($scope.voucherDetailList.length === 0 && $scope.advance.SettlementType === 'SetOff' && $scope.advance.PaymentSource === 'Invoice') {
                ShowResult("Please select Invoice Receivable!", "failure");
                return true;
            }
            if ($scope.voucherDetailListNew.length === 0 && $scope.advanceNew.SettlementType === 'SetOff' && $scope.advanceNew.PaymentSource === 'Invoice') {
                ShowResult("Please select Invoice Receivable!", "failure");
                return true;
            }
            if ($scope.advance.PaymentSource === 'Bank' && $scope.advance.SettlementType === 'Return' && $scope.advance.BankMasterId==null) {
                ShowResult("Please select Bank!", "failure");
                return true;
            }
            if ($scope.advance.PaymentSource === 'Bank' && $scope.advance.SettlementType === 'SetOff' && $scope.advance.BankMasterId == null) {
                ShowResult("Please select Bank!", "failure");
                return true;
            }
            if ($scope.advance.PaymentSource === 'Cash' && $scope.advance.SettlementType === 'Return' && $scope.advance.CashMasterId == null) {
                ShowResult("Please select Bank!", "failure");
                return true;
            }
        }
        return false;
    };

    $scope.clear = function () {
        $scope.Action = "Save";
        $scope.advance = {};
        $scope.advanceNew = {};
        $scope.advance.Active = true;
        if ($scope.voucherTypeList.length === 1) {
            $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
            $scope.advanceNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
        }
        $scope.advance.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.advance.PostingDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.advance.DocDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.advanceNew.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.advanceNew.PostingDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.advanceNew.DocDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucherDetailCurrencyList = [];
        $scope.voucherDetailList = [];
        $scope.voucherDetailListNew = [];
        $scope.advanceDetailList = [];
        $scope.partyPlantList = [];
        $scope.advance.PaymentSource = "Invoice";
        $scope.advanceNew.PaymentSource = "Bank";
        $scope.advance.SettlementType = "SetOff";
        $scope.advanceNew.SettlementType = "SetOff";
        $scope.advance.PartyType= "Customer";
    };


    $scope.getFiscalYearPeriod = function (date) {
        if (!baseService.isUndefinedOrNull(date) && !$scope.invalidPostingDate) {
            $http({
                method: "get",
                url: "accounts/CompanyFiscalYear/CheckingFiscalYearPeriod?postingDate=" + $filter("dateFiltering")(date)
            }).then(
                function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.currencyExchangeRate = [];
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        var result = response.data;
                        if (result.IsTransationLocked === true) {
                            ShowResult(commonMessage.FiscalPeriodTransactionLocked, "failure");
                            $scope.advance.PostingDate = "";
                            $scope.advance.FiscalYearId = null;
                            $scope.advance.FiscalYearName = null;
                            $scope.advance.FiscalYearPeriodId = null;
                            $scope.advance.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else if (result.IsExchangeRateConfirmed === false) {
                            ShowResult(commonMessage.FiscalPeriodExchangeRateConfirmed, "failure");
                            $scope.advance.PostingDate = "";
                            $scope.advance.FiscalYearId = null;
                            $scope.advance.FiscalYearName = null;
                            $scope.advance.FiscalYearPeriodId = null;
                            $scope.advance.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else {
                            $scope.advance.FiscalYearId = result.FiscalYearId;
                            $scope.advance.FiscalYearName = result.FiscalYearName;
                            $scope.advance.FiscalYearPeriodId = result.FiscalYearPeriodId;
                            $scope.advance.FiscalYearPeriodName = result.PeriodName;
                        }
                    }
                },
                function errorCallback() {
                });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.getFiscalYearPeriod($scope.advance.PostingDate);

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            setPartyData(party);
            $scope.advance.CurrencyId = $scope.selectBaseCurrency();
            // Party plant list calling.
            $scope.getPartyPlantList($scope.advance.PartyId, false);
        }
        $scope.hidePartyPopUp();
    };

    $scope.clearPartyPopUp = function () {
        $scope.advance.PartyId = null;
        $scope.advance.PartyCode = null;
        $scope.advance.PartyName = null;
        $scope.advance.PartyType = null;
        $scope.advance.CurrencyId = null;
        $scope.advance.TotalPartyPlant = null;
        $scope.voucherList = [];
        $scope.partyPlantList = [];
    };

    $scope.getPartyPlantList = function (partyId, isUpdateMode) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault && !isUpdateMode) {
                        $scope.advance.PartyPlantId = item.Value;
                    }
                });
                $scope.advanceDetail = {};
            });
    };

    function setPartyData(party) {
        if (baseService.isUndefinedOrNull(party.DownPaymentGLId)) {
            ShowResult("Customer gl not found!", "failure", "partyPopUp");
            return;
        }
        else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.DownPaymentBudgetId)) {
            ShowResult("Customer budget not found!", "failure", "partyPopUp");
            return;
        }
        //else if (baseService.isUndefinedOrNull(party.CurrencyId)) {
        //    ShowResult("Customer transaction currency not found!", "failure", "partyPopUp");
        //    return;
        //}
        else {
            $scope.advance.PartyName = party.Code + " - " + party.UserName;
            $scope.advance.PartyId = party.Id;
            $scope.advance.PartyType = party.PartyType;
            // TODO: have to check.
            $scope.advance.PartyGLGeneralInfoId = party.DownPaymentGLId;
            $scope.advance.PartyGL = party.DownPaymentGLCode + " - " + party.DownPaymentGLName;
        }
    }

    //*********************** Customer Invoice PopUp Start *************************************
    $scope.customerInvoiceSearchList = [];
    $scope.customerreceivableList = [];
    $scope.customerInvoiceSearch = [];
    $scope.customerInvoiceSelectedIndex = -1;
    $scope.customerInvoiceParameters = {
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

    $scope.showCustomerInvoicePopUp = function (partyId) {
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.customerreceivableList = [];
            ShowResult("Please select Customer.", "failure");
            return;
        }
        else {
            $scope.compareCurrencyId = $scope.advance.CurrencyId;
            $scope.customerInvoiceParameters.partyId = partyId;
            $scope.customerreceivableGLData = function (pageno) {
                baseService.paginationBase("accounts/CustomerInvoice/GetCustomerAvailableInvoiceList", pageno, $scope.customerInvoiceParameters)
                    .then(function (response) {
                        $scope.customerreceivableList = response.Rows;
                        console.log($scope.customerreceivableList);
                        $scope.customerInvoiceParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.customerInvoiceSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.customerreceivableList, $scope.customerInvoiceSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector("#customerInvoicePopUp")).modal("show");
            $scope.customerreceivableGLData();
        }
    };

    $scope.closePopUpselected = function () {
        angular.forEach($scope.customerreceivableList, function (data, i) {
            if (data.Active === true) {
                data.TrnType = "Cr";
                data.PartyPlantName = data.PartyPlantName;
                var getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Cr", "DocRefNo": data.DocRefNo });
                if (getRow.length === 0) {
                    data.Amount = data.Receivable;
                    data.WriteOff = data.Received;
                    data.Advilable = data.Balance;
                    data.DrAmount = '';
                    data.CrAmount = data.Balance;
                    if ($scope.advanceNew.PartyId == null) {
                        $scope.voucherDetailList.push(data);
                        if ($scope.voucherDetailList.length > 0)
                            $scope.isReadOnly = true;
                        else
                            $scope.isReadOnly = false;
                    }
                    else {
                        $scope.voucherDetailListNew = [];
                        $scope.advanceNew.CurrencyId = data.CurrencyId
                        $scope.advanceNew.CompanyCurrencyRate = data.CompanyCurrencyRate;
                        $scope.voucherDetailListNew.push(data);
                        if ($scope.voucherDetailListNew.length > 0)
                            $scope.isReadOnly = true;
                        else
                            $scope.isReadOnly = false;
                    }
                   
                    angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
                    //$scope.convertAmountCr(data);
                }
                else {
                    ShowResult(data.DocRefNo + " already  Exist", "failure", "customerInvoicePopUp");
                }
            }
        });
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
    };

    //*********************** Customer Invoice PopUp End ***************************************

    //*********************** Customer Advance PopUp Start *************************************
    $scope.customerAdvanceSearchList = [];
    $scope.customerAdvanceDataList = [];
    $scope.customerAdvanceSearch = [];
    $scope.customerAdvanceSelectedIndex = -1;
    $scope.customerAdvanceParameters = {
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

    $scope.showCustomerAdvancePopUp = function (partyId, partyPlantId) {
        $scope.compareCurrencyId = $scope.advance.CurrencyId;
        $scope.getCustomerAdvanceData = function (pageno) {
            baseService.paginationBase("accounts/Advance/GetAvilabeCustomerAdvanceList", pageno, $scope.customerAdvanceParameters)
                .then(function (response) {
                    $scope.customerAdvanceDataList = response.Rows;
                    $scope.customerAdvanceParameters.total_count = response.Total;
                    if (baseService.arrayLength($scope.customerAdvanceSearchList) === 0) {
                        baseService.getDDLSearchColumn($scope.customerAdvanceDataList, $scope.customerAdvanceSearchList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#customerAdvancePopUp")).modal("show");
        $scope.getCustomerAdvanceData();
    };

    $scope.closeCustomerAdvancePopUp = function () {
        angular.element(document.querySelector("#customerAdvancePopUp")).modal("hide");
    };
    $scope.totalAdvanceVendorWise = function (partyId) {
        $scope.advance.TotalAdvanceAmount = Math.round(($filter('sumByKey')($filter('filter')($scope.customerAdvanceDataList, { 'PartyId': partyId }), 'Balance')) * 100 + Number.EPSILON) / 100;
    }
    $scope.closeCustomerAdvancePopUpSelected = function (data) {
        data.TrnType = "Dr";
        $scope.advance.VoucherNo = data.VoucherNo;
        $scope.advance.PartyName = data.PartyCode + " - " + data.PartyName;
        $scope.advance.PartyId = data.PartyId;
        $scope.advance.PartyType = data.PartyType;
        $scope.advance.PartyPlantId = data.PartyPlantId;
        $scope.advance.PartyPlantName = data.PartyPlantName;
        $scope.advance.CurrencyId = data.CurrencyId;
        $scope.advance.CurrencyCode = data.CurrencyCode;
        $scope.advance.EntityId = data.EntityId;
        $scope.advance.AdvanceId = data.AdvanceId;
        $scope.advance.AdvanceDetailId = data.AdvanceDetailId;

        $scope.advance.CompanyId = data.CompanyId;
        $scope.advance.PlantId = data.PlantId;
        $scope.advance.PartyType = data.PartyType;
        $scope.advance.PaymentAmount = data.Balance;
        $scope.advance.CompanyCurrencyRate = data.CompanyCurrencyRate;
        $scope.advance.NewCompanyCurrencyRate = data.CompanyCurrencyRate;
        $scope.advance.PaymentPostingDate = data.PostingDate;
        $scope.totalAdvanceVendorWise($scope.advance.PartyId);

        angular.element(document.querySelector("#customerAdvancePopUp")).modal("hide");
    };

    //*********************** Customer Advance PopUp End *************************************

    $scope.report = function (voucherId) {
        location.href = $scope.reportUrl + voucherId;
    };

    cboService.getCboVoucherTypeAdvanceTakenWriteOffList(function (result) {
        $scope.voucherTypeList = result;
        if ($scope.voucherTypeList.length === 1) {
            $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
            $scope.advance.PostingDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
            $scope.advance.BankTransactionDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
            $scope.advance.DocDate = $scope.advance.PostingDate;
            $scope.advanceNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
            $scope.advanceNew.PostingDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
            $scope.advanceNew.BankTransactionDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
            $scope.advanceNew.DocDate = $scope.advanceNew.PostingDate;
        }
    });

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.advance.VoucherTypeId = data.Value;
        $scope.advance.PostingDate = $filter('dateFiltering')(data.LastPostingDate);
        $scope.advance.DocDate = $scope.advance.PostingDate;
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkPostingDate();
        if ($scope.form0.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.parkUrl,
                    data: {
                        "advanceVM": $scope.advance,
                        "advanceDetailVMList": $scope.voucherDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.clear();
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: {
                        "advanceVM": $scope.advance,
                        "advanceDetailVMList": $scope.voucherDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.clear();
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            return true;
        }
        return true;
    };

    $scope.advanceWriteOffId = null;
    $scope.confirmPost = function (id) {
        $scope.advanceWriteOffId = id;
        $scope.message_confirmation = 'Are you sure to Post?';
        angular.element(document.querySelector('#confirmPostPopUp')).modal('show');
    };

    $scope.post = function (id) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "advanceWriteOffId": id
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.changeSettlementType = function () {

        if ($scope.advance.SettlementType == 'SetOff') {
            $scope.advance.PaymentSource = 'Bank'
        }
        else {
            $scope.advance.PaymentSource = 'Invoice'
        }
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            //if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            //    ShowResult("Please select currency!", "failure", "bankPopUp");
            //    return;
            //}
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
                $scope.advanceNew.AccountTitle = bank.AccountTitle;
                $scope.advanceNew.BankName = bank.AccountTitle;
                $scope.advanceNew.BankMasterId = bank.BankMasterId;
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
        $scope.advanceNew.BankCurrencyId = bank.CurrencyId;
        $scope.advance.TrnType = "Cr";
        //if ($scope.advance == $scope.advanceNew.BankCurrencyId)
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
        $scope.advance.CompanyCurrencyRate = 1;
    }

    $scope.clearCashPopUp = function () {
        $scope.clearBankPopUp();
    };
    $scope.removeRow = function (index) {
        $scope.voucherDetailList.splice(index, 1);
    };
    $scope.exchangeGainLossAmount = function (data) {
        var balance = parseFloat(data.Advilable), dramount = parseFloat(data.DrAmount);
        if (dramount > balance) {
            data.DrAmount = data.Balance;
            ShowResult("Payment Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        $scope.DrAmountSubTotal = $filter("sumByKey")($filter("filter")($scope.voucherDetailList), "DrAmount");
        if (parseFloat($scope.advance.PaymentAmount) < $scope.DrAmountSubTotal) {
            data.DrAmount = 0;
            ShowResult("Total Received Amount should not exceed Payment Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        if (data.CompanyCurrencyRate > $scope.advance.CompanyCurrencyRate) {
            if ($scope.voucherDetailList.length === 1) {
                data.ExchangeAmount = Math.abs(($scope.advance.PaymentAmount * $scope.advance.CompanyCurrencyRate).toFixed(2) - (data.DrAmount * data.CompanyCurrencyRate).toFixed(2)).toFixed(2);
            }
            else {
                data.ExchangeAmount = Math.abs(data.DrAmount * ($scope.advance.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
            }

            data.ExchangeType = "ExchangeLoss";
        }
        else if (data.CompanyCurrencyRate < $scope.advance.CompanyCurrencyRate) {
            if ($scope.voucherDetailList.length === 1) {
                data.ExchangeAmount = Math.abs(($scope.advance.PaymentAmount * $scope.advance.CompanyCurrencyRate).toFixed(2) - (data.DrAmount * data.CompanyCurrencyRate).toFixed(2)).toFixed(2);
            }
            else {
                data.ExchangeAmount = Math.abs(data.DrAmount * (data.CompanyCurrencyRate - $scope.advance.CompanyCurrencyRate)).toFixed(2);
            }
            
            data.ExchangeType = "ExchangeGain";
        }
        else {
            data.ExchangeAmount = 0;
            data.ExchangeType = null;
        }
    };

    $scope.exchangeGainLossAmountNew = function (data) {
        var balance = parseFloat(data.Advilable), dramount = parseFloat(data.DrAmount);
        if (dramount > balance) {
            data.DrAmount = data.Balance;
            ShowResult("Payment Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        //$scope.DrAmountSubTotal = $filter("sumByKey")($filter("filter")($scope.voucherDetailListNew), "DrAmount");
        //if (parseFloat($scope.advance.PaymentAmount) < $scope.DrAmountSubTotal) {
        //    data.DrAmount = 0;
        //    ShowResult("Total Received Amount should not exceed Payment Amount.", "failure");
        //}
        //else {
        //    CloseShowResult();
        //}
        if (data.CompanyCurrencyRate > $scope.advanceNew.CompanyCurrencyRate) {
            data.ExchangeAmount = Math.abs(data.DrAmount * ($scope.advanceNew.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
            data.ExchangeType = "ExchangeLoss";
        }
        else if (data.CompanyCurrencyRate < $scope.advanceNew.CompanyCurrencyRate) {
            data.ExchangeAmount = Math.abs(data.DrAmount * (data.CompanyCurrencyRate - $scope.advanceNew.CompanyCurrencyRate)).toFixed(2);
            data.ExchangeType = "ExchangeGain";
        }
        else {
            data.ExchangeAmount = 0;
            data.ExchangeType = null;
        }
    };
    //Delete option



    $scope.confirmDelete = function (A) {
        //$scope.invoiceWriteOffGroupNo = A;
        $scope.voucherId = A;
        
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
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
    // #region TAB CHANGE Main
    $scope.tab = 1;
    
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.setTabSingle = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.setTabMultiple = function (newTab) {
        $scope.tab = newTab;
        $scope.getFiscalYearPeriodNew($scope.advanceNew.PostingDate);
        $scope.TrnCurrency();
    };
    $scope.clearSourceInfo = function () {
        $scope.advance.Amount = 0;
        $scope.advance.BankAmount = 0;
        $scope.advance.CashMasterId = null;
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
        $scope.voucherDetailList = [];
        $scope.voucherDetailListNew = [];
    }

    

    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        if (baseService.isUndefinedOrNull(party.DownPaymentGLId)) {
            ShowResult("Customer DownPaymentGL not found!", "failure", "partyPopUp");
            return;
        }
        else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.ReconciliationBudgetId)) {
            ShowResult($scope.partyType + " Budget not found!", "failure", "partyPopUp");
            return;
        }
        else {
            $scope.advanceNew.PartyName = party.Code + " - " + party.UserName;
            $scope.advanceNew.PartyId = party.Id;
            $scope.advanceNew.PartyType = $scope.partyType;
            $scope.advanceNew.CurrencyId = party.CurrencyId;
            $scope.partyPlantList = [];
            $scope.getPartyPlantListNew(party.Id);
            $scope.GetCurrencyExchangeRateList();
            $scope.voucherDetailList = [];
            $scope.voucherDetailListNew = [];
        }
        $scope.hidePartyPopUp();
    };

    $scope.getPartyPlantListNew = function (partyId) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault) {
                        $scope.partyPlantId = item.Value;
                        $scope.advanceNew.PartyPlantId = item.Value;
                        $scope.advanceNew.DeliveryPartyPlantId = item.Value;
                        $scope.billToAddress = item.Address1;
                        $scope.shipToAddress = item.Address1;
                    }
                });
            });
    };
    $scope.invalidPostingDateNew = false;
    $scope.checkPostingDateNew = function () {
        var msg = "";
        if (new Date($scope.advance.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.advanceNew.FiscalYearId = null;
            $scope.advanceNew.FiscalYearName = null;
            $scope.advanceNew.FiscalYearPeriodId = null;
            $scope.advanceNew.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDateNew = true;
        }
        else if (new Date($scope.advance.PostingDate) < new Date($scope.advance.DocDate)) {
            msg = "Posting date must be below or equal to Doc Date!";
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        } else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.getFiscalYearPeriodNew = function (date) {
        if (!baseService.isUndefinedOrNull(date) && !$scope.invalidPostingDateNew) {
            $http({
                method: "get",
                url: "accounts/CompanyFiscalYear/CheckingFiscalYearPeriod?postingDate=" + $filter("dateFiltering")(date)
            }).then(
                function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.currencyExchangeRate = [];
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        var result = response.data;
                        if (result.IsTransationLocked === true) {
                            ShowResult(commonMessage.FiscalPeriodTransactionLocked, "failure");
                            $scope.advanceNew.PostingDate = "";
                            $scope.advanceNew.FiscalYearId = null;
                            $scope.advanceNew.FiscalYearName = null;
                            $scope.advanceNew.FiscalYearPeriodId = null;
                            $scope.advanceNew.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else if (result.IsExchangeRateConfirmed === false) {
                            ShowResult(commonMessage.FiscalPeriodExchangeRateConfirmed, "failure");
                            $scope.advanceNew.PostingDate = "";
                            $scope.advanceNew.FiscalYearId = null;
                            $scope.advanceNew.FiscalYearName = null;
                            $scope.advanceNew.FiscalYearPeriodId = null;
                            $scope.advanceNew.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else {
                            $scope.advanceNew.FiscalYearId = result.FiscalYearId;
                            $scope.advanceNew.FiscalYearName = result.FiscalYearName;
                            $scope.advanceNew.FiscalYearPeriodId = result.FiscalYearPeriodId;
                            $scope.advanceNew.FiscalYearPeriodName = result.PeriodName;
                        }
                    }
                },
                function errorCallback() {
                });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    

    $scope.tranCurrencyListNew = [];
    $scope.TrnCurrency = function () {
        cboService.getCboTransactionCurrencyByCompany('', function (result) {
            $scope.tranCurrencyListNew = result;
        });
    }
    
    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.advanceNew.PostingDate) && !baseService.isUndefinedOrNull($scope.advanceNew.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.advanceNew.PostingDate + "&currencyId=" + $scope.advanceNew.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.advanceNew.CurrencyCode = $scope.currencyExchangeRate.ToCurrencyCode;
                $scope.advanceNew.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
            //$scope.AdvanceExchangeGainLossAmount();
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    //*********************** Customer Advance PopUp Start *************************************
    $scope.customerAdvanceSearchList = [];
    $scope.customerAdvanceNewList = [];
    $scope.customerAdvanceNewSearch = [];
    $scope.customerAdvanceNewSelectedIndex = -1;
    $scope.customerAdvanceParametersNew = {
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

    $scope.showCustomerAdvanceNewPopUpNew = function (partyId) {
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.customerAdvanceNewList = [];
            ShowResult("Please select Customer.", "failure");
            return;
        }
        else {
            $scope.compareCurrencyId = $scope.advanceNew.CurrencyId;
            $scope.customerAdvanceParameters.partyId = partyId;
            $scope.customerAdvanceGLDataNew = function (pageno) {
                baseService.paginationBase("accounts/Advance/GetAvilabeCustomerAdvanceByCustomerList?CustomerId=" + partyId, pageno, $scope.customerAdvanceParameters)
                    .then(function (response) {
                        $scope.customerAdvanceNewList = response.Rows;
                        $scope.customerAdvanceParametersNew.total_count = response.Total;
                        if (baseService.arrayLength($scope.customerAdvanceSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.customerAdvanceNewList, $scope.customerAdvanceNewSearch);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector("#customerAdvancePopUpNew")).modal("show");
            $scope.customerAdvanceGLDataNew();
        }
    };
    $scope.advanceDetailList = [];
    $scope.closeAdvancePopUpNewselected = function (x) {
        x.TrnType = "Cr";
        x.PartyPlantName = x.PartyPlantName;
        var getRow = $filter("filter")($scope.advanceDetailList, { "TrnType": "Dr", "DocRefNo": x.DocRefNo });
        if (getRow.length === 0) {
            x.Amount = x.Receivable;
            x.WriteOff = x.Received;
            x.Advilable = x.Balance;
            x.CrAmount = '';
            x.DrAmount = x.Balance;
           
            $scope.advanceDetailList.push(x);
            if ($scope.advanceDetailList.length > 0)
                $scope.isReadOnly = true;
            else
                $scope.isReadOnly = false;
            angular.element(document.querySelector("#customerAdvancePopUpNew")).modal("hide");
            //$scope.convertAmountCr(x);
        }
        else {
            ShowResult(data.DocRefNo + " already  Exist", "failure", "customerAdvancePopUpNew");
        }
    };

    $scope.closeAdvanceNewPopUp = function () {
        angular.element(document.querySelector("#customerAdvancePopUpNew")).modal("hide");
    };

    //*********************** Customer Advance PopUp End ***************************************
    $scope.removeAdvanceRow = function (index) {
        $scope.advanceDetailList.splice(index, 1);
    };
    $scope.AdvanceExchangeGainLossAmount = function (data) {
        var balance = parseFloat(data.Advilable), dramount = parseFloat(data.DrAmount);
        if (dramount > balance) {
            data.DrAmount = data.Balance;
            ShowResult("Payment Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        $scope.DrAmountSubTotal = $filter("sumByKey")($filter("filter")($scope.advanceDetailList), "DrAmount");
        if (parseFloat($scope.advanceNew.PaymentAmount) < $scope.DrAmountSubTotal) {
            data.DrAmount = 0;
            ShowResult("Total Received Amount should not exceed Payment Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        if ($scope.advanceNew.PaymentSource == 'Bank') {
            if (data.CompanyCurrencyRate > $scope.advanceNew.CompanyCurrencyRate) {
                data.ExchangeAmount = Math.abs(data.DrAmount * ($scope.advanceNew.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
                data.ExchangeType = "ExchangeLoss";
            }
            else if (data.CompanyCurrencyRate < $scope.advanceNew.CompanyCurrencyRate) {
                data.ExchangeAmount = Math.abs(data.DrAmount * (data.CompanyCurrencyRate - $scope.advanceNew.CompanyCurrencyRate)).toFixed(2);
                data.ExchangeType = "ExchangeGain";
            }
            else {
                data.ExchangeAmount = 0;
                data.ExchangeType = null;
            }
        }
        else {
            if (data.CompanyCurrencyRate < $scope.advanceNew.CompanyCurrencyRate) {
                data.ExchangeAmount = Math.abs(data.DrAmount * ($scope.advanceNew.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
                data.ExchangeType = "ExchangeLoss";
            }
            else if (data.CompanyCurrencyRate > $scope.advanceNew.CompanyCurrencyRate) {
                data.ExchangeAmount = Math.abs(data.DrAmount * (data.CompanyCurrencyRate - $scope.advanceNew.CompanyCurrencyRate)).toFixed(2);
                data.ExchangeType = "ExchangeGain";
            }
            else {
                data.ExchangeAmount = 0;
                data.ExchangeType = null;
            }
        }
        
    };

    //$scope.AdvanceExchangeGainLossAmount = function () {
    //    for (var i = 0; i < $scope.advanceDetailList.length; i++) {
    //        if ($scope.advanceDetailList[i].CompanyCurrencyRate > $scope.advanceNew.CompanyCurrencyRate) {
    //            $scope.advanceDetailList[i].ExchangeAmount = Math.abs($scope.advanceDetailList[i].DrAmount * ($scope.advanceNew.CompanyCurrencyRate - $scope.advanceDetailList[i].CompanyCurrencyRate)).toFixed(2);
    //            $scope.advanceDetailList[i].ExchangeType = "ExchangeLoss";
    //        }
    //        else if ($scope.advanceDetailList[i].CompanyCurrencyRate < $scope.advanceNew.CompanyCurrencyRate) {
    //            $scope.advanceDetailList[i].ExchangeAmount = Math.abs($scope.advanceDetailList[i].DrAmount * ($scope.advanceDetailList[i].CompanyCurrencyRate - $scope.advanceNew.CompanyCurrencyRate)).toFixed(2);
    //            $scope.advanceDetailList[i].ExchangeType = "ExchangeGain";
    //        }
    //        else {
    //            $scope.advanceDetailList[i].ExchangeAmount = 0;
    //            $scope.advanceDetailList[i].ExchangeType = null;
    //        }
    //    }
       
    //};

    $scope.MultiAdvanceValidation = function () {
        if (baseService.isUndefinedOrNull($scope.advanceNew.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return true;
        }
        if ($scope.partyType === "Customer") {
            if ($scope.advanceNew.PartyId === null) {
                ShowResult("Please select Customer!", "failure");
                return true;
            }
            if ($scope.advanceDetailList.length === 0 && $scope.advanceNew.SettlementType === 'SetOff' && $scope.advanceNew.PaymentSource === 'Bank') {
                ShowResult("Please select Advance !", "failure");
                return true;
            }
            if ($scope.advanceNew.PaymentSource === 'Bank' && $scope.advanceNew.SettlementType === 'Return' && $scope.advanceNew.BankMasterId == null) {
                ShowResult("Please select Bank!", "failure");
                return true;
            }
            if ($scope.advanceNew.PaymentSource === 'Bank' && $scope.advanceNew.SettlementType === 'SetOff' && $scope.advanceNew.BankMasterId == null) {
                ShowResult("Please select Bank!", "failure");
                return true;
            }
            if ($scope.advanceNew.PaymentSource === 'Cash' && $scope.advanceNew.SettlementType === 'Return' && $scope.advanceNew.CashMasterId == null) {
                ShowResult("Please select Cash!", "failure");
                return true;
            }
            if ($scope.voucherDetailListNew.length > 0) {
                for (var i = 0; i < $scope.voucherDetailListNew.length; i++) {
                    if (new Date($scope.voucherDetailListNew[i].PostingDate) > new Date($scope.advanceNew.PostingDate)) {
                        ShowResult("Posting date must be below or equal to payable of " + $scope.voucherDetailListNew[i].VoucherNo , "failure");
                        return true;
                    }
                }
            }
        }
        return false;
    };

    $scope.MultiAdvanceSetOffSave = function () {
        $scope.$broadcast("show-errors-check-validity");
       /* $scope.checkPostingDate();*/
        if ($scope.form2.$valid && !$scope.MultiAdvanceValidation()) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.parkUrlMultiAdvance,
                    data: {
                        "advanceVM": $scope.advanceNew,
                        "advanceDetailVMList": $scope.advanceDetailList,
                        "voucherDetailListNew": $scope.voucherDetailListNew
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.clear();
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            return true;
        }
        return true;
    };

}