"use strict";
customerSuspenseWriteOffController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "toaster", "$controller"];
function customerSuspenseWriteOffController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = "Customer Suspense Set-off";
    $scope.hideSource = true;
    $scope.url = 'accounts/Advance';
    $scope.listUrl = $scope.url + '/GetCustomerSuspenseWriteOffList';
    $scope.parkUrl = $scope.url + '/ParkCustomerSuspenseWriteOff';
    $scope.updateUrl = $scope.url + '/UpdateCustomerSuspenseWriteOff';
    $scope.postUrl = $scope.url + '/PostCustomerSuspenseWriteOff';
    $scope.reportUrl = $scope.url + '/ReportCustomerSuspenseWriteOff?voucherId=';

    $scope.partyType = "Customer";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherDetailCurrency = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.isWriteOff = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.partyType = "Customer";
    $controller("partyBaseController", { $scope: $scope, $http: $http });

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

                $http.get('Accounts/Advance/GetAvilabeCustomerSuspense?partyId=' + $scope.advance.PartyId + '&advanceId=' + advanceId)
                    .then(function (response) {
                        var data = response.data;
                        data.TrnType = "Dr";
                        var getRow = null;
                        getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "DocRefNo": data.DocRefNo });
                        if (getRow.length === 0) {
                            data.Amount = data.Receivable;
                            data.WriteOff = data.Received;
                            data.Advilable = data.Balance;
                            data.DrAmount = data.Balance;
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
        PartyType: $scope.partyType,
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
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

    $scope.searchByList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            "name": "Ordering Party",
            "value": "InvoicingPartyPlantName"
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
        } else {
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
            if ($scope.voucherDetailList.length === 0) {
                ShowResult("Please select Invoice Receivable!", "failure");
                return true;
            }
            var customeradvanceList = $filter("filter")($scope.voucherDetailList, { TrnType: "Dr" });
            if (customeradvanceList.length === 0) {
                ShowResult("Please select Customer Advance!", "failure");
                return true;
            }
        }
        return false;
    };

    $scope.clear = function () {
        $scope.Action = "Save";
        $scope.advance = {};
        $scope.advance.Active = true;
        if ($scope.voucherTypeList.length === 1) {
            $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
        }
        $scope.advance.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.advance.PostingDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.advance.DocDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucherDetailCurrencyList = [];
        $scope.voucherDetailList = [];
    };

    $scope.setDrExchangeRate = function (data) {
        if (!baseService.isUndefinedOrNull(data.GLGeneralInfoId)) {
            var getRow = $filter("filter")($scope.voucherDetailCurrencyList, { "TrnType": "Dr", "AdvanceDetailId": data.AdvanceDetailId, "GLGeneralInfoId": data.GLGeneralInfoId });
            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
                getRow[0].TrnType = "Dr";
                getRow[0].AdvanceDetailId = data.AdvanceDetailId;
                getRow[0].GLGeneralInfoId = data.GLGeneralInfoId;
                getRow[0].GLGeneralInfoCode = data.GLGeneralInfoCode;
                getRow[0].GLGeneralInfoName = data.GLGeneralInfoName;
                getRow[0].BudgetMasterId = data.BudgetMasterId;
                getRow[0].BudgetCode = data.BudgetCode;
                getRow[0].BudgetName = data.BudgetName;
                getRow[0].ActivityId = data.ActivityId;
                getRow[0].ActivityCode = data.ActivityCode;
                getRow[0].ActivityName = data.ActivityName;
                getRow[0].DocRefNo = data.DocRefNo;
                // Base currency
                if (!baseService.isUndefinedOrNull($scope.companyCurrencyId)) {
                    getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                    getRow[0].CompanyCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                    getRow[0].ToCurrencyId = data.ToCurrencyId;
                    getRow[0].CompanyCurrencyRate = data.CompanyCurrencyRate;
                    getRow[0].CompanyCurrencyCr = null;
                    getRow[0].CompanyCurrencyDr = (data.DrAmount * data.CompanyCurrencyRate).toFixed(2);
                    if ($scope.companyCurrencyId === $scope.advance.CurrencyId) {
                        data.ConvertedAmount = getRow[0].CompanyCurrencyDr;
                    }
                }
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                    getRow[0].CompanyGroupCurrencyRate = data.CompanyGroupCurrencyRate;
                    getRow[0].CompanyGroupCurrencyCr = null;
                    getRow[0].CompanyGroupCurrencyDr = (data.DrAmount / data.CompanyGroupCurrencyRate).toFixed(2);
                    if ($scope.companyGroupCurrencyId === $scope.advance.CurrencyId) {
                        data.ConvertedAmount = getRow[0].CompanyGroupCurrencyDr;
                    }
                }
                // Hard currency
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    getRow[0].HardCurrencyId = $scope.hardCurrencyId;
                    getRow[0].HardCurrencyName = $scope.companyCurrencyName;
                    getRow[0].HardFromCurrencyId = data.HardFromCurrencyId;
                    getRow[0].HardCurrencyRate = data.HardCurrencyRate;
                    getRow[0].HardCurrencyCr = null;
                    getRow[0].HardCurrencyDr = (data.DrAmount * data.HardCurrencyRate).toFixed(2);
                    if ($scope.hardCurrencyId === $scope.advance.CurrencyId) {
                        data.ConvertedAmount = getRow[0].HardCurrencyDr;
                    }
                }
            }
            else {
                var obj = {
                    TrnType: "Dr",
                    DocRefNo: data.DocRefNo,
                    AdvanceDetailId: data.AdvanceDetailId,
                    GLGeneralInfoId: data.GLGeneralInfoId,
                    GLGeneralInfoCode: data.GLGeneralInfoCode,
                    GLGeneralInfoName: data.GLGeneralInfoName,
                    BudgetMasterId: data.BudgetMasterId,
                    BudgetCode: data.BudgetCode,
                    BudgetName: data.BudgetName,
                    ActivityId: data.ActivityId,
                    ActivityCode: data.ActivityCode,
                    ActivityName: data.ActivityName
                };
                // Base currency
                if (!baseService.isUndefinedOrNull($scope.companyCurrencyId)) {
                    obj.CompanyCurrencyId = $scope.companyCurrencyId;
                    obj.CompanyCurrencyName = $scope.companyCurrencyName;
                    obj.CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                    obj.ToCurrencyId = data.ToCurrencyId;
                    obj.CompanyCurrencyRate = data.CompanyCurrencyRate;
                    obj.CompanyCurrencyCr = null;
                    obj.CompanyCurrencyDr = (data.DrAmount * data.CompanyCurrencyRate).toFixed(2);
                    if ($scope.companyCurrencyId === $scope.advance.CurrencyId) {
                        obj.ConvertedAmount = data.CompanyCurrencyDr;
                    }
                }
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    obj.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    obj.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;
                    obj.CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                    obj.CompanyGroupCurrencyRate = data.CompanyGroupCurrencyRate;
                    obj.CompanyGroupCurrencyCr = null;
                    obj.CompanyGroupCurrencyDr = (data.DrAmount / data.CompanyGroupCurrencyRate).toFixed(2);
                    if ($scope.companyGroupCurrencyId === $scope.advance.CurrencyId) {
                        obj.ConvertedAmount = data.CompanyGroupCurrencyDr;
                    }
                }
                // Hard currency
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    obj.HardCurrencyId = $scope.hardCurrencyId;
                    obj.HardCurrencyName = $scope.hardCurrencyName;
                    obj.HardFromCurrencyId = data.HardFromCurrencyId;
                    obj.HardCurrencyRate = data.HardCurrencyRate;
                    obj.HardCurrencyCr = null;
                    obj.HardCurrencyDr = (data.DrAmount * data.HardCurrencyRate).toFixed(2);
                    if ($scope.hardCurrencyId === $scope.advance.CurrencyId) {
                        obj.ConvertedAmount = data.HardCurrencyDr;
                    }
                }
                $scope.voucherDetailCurrencyList.push(data);
            }
        }
    };

    $scope.setCrExchangeRate = function (data) {
        if (!baseService.isUndefinedOrNull(data.GLGeneralInfoId)) {
            var companyCurrencyExchangeRate = data.CompanyCurrencyRate;
            var companyGroupCurrencyExchangeRate = data.CompanyGroupCurrencyRate;
            var getRow = $filter("filter")($scope.voucherDetailCurrencyList, { "TrnType": "Cr", "InvoiceDetailId": data.InvoiceDetailId, "GLGeneralInfoId": data.GLGeneralInfoId });
            // Exchange Gain Loss Checking
            var advance = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr" });
            advance = advance[0];
            var companyCurrencyDrExchangeRate = advance.CompanyCurrencyRate;
            var companyGroupCurrencyDrExchangeRate = advance.CompanyGroupCurrencyRate;

            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
                getRow[0].TrnType = "Cr";
                getRow[0].InvoiceDetailId = data.InvoiceDetailId;
                getRow[0].GLGeneralInfoId = data.GLGeneralInfoId;
                getRow[0].GLGeneralInfoCode = data.GLGeneralInfoCode;
                getRow[0].GLGeneralInfoName = data.GLGeneralInfoName;
                getRow[0].DocRefNo = data.DocRefNo;
                getRow[0].BudgetMasterId = data.BudgetMasterId;
                getRow[0].BudgetCode = data.BudgetCode;
                getRow[0].BudgetName = data.BudgetName;
                getRow[0].ActivityId = data.ActivityId;
                getRow[0].ActivityCode = data.ActivityCode;
                getRow[0].ActivityName = data.ActivityName;
                // Base currency
                if (!baseService.isUndefinedOrNull($scope.companyCurrencyId)) {
                    getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                    getRow[0].CompanyCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                    getRow[0].ToCurrencyId = data.ToCurrencyId;
                    getRow[0].CompanyCurrencyRate = data.CompanyCurrencyRate;
                    getRow[0].CompanyCurrencyDr = null;
                    getRow[0].CompanyCurrencyCr = (data.CrAmount * data.CompanyCurrencyRate).toFixed(2);

                    if ($scope.companyCurrencyId === $scope.advance.CurrencyId) {
                        data.ConvertedAmount = getRow[0].CompanyCurrencyCr;
                    }
                    if ($scope.companyCurrencyId !== data.CurrencyId) {
                        if (data.CompanyCurrencyRate > advance.CompanyCurrencyRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Base' });
                            if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                var loss = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });
                                rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                rowGroup[0].TrnType = 'Dr';
                                rowGroup[0].Exchange = 'Base';
                                rowGroup[0].ExchangeStatus = 'ExchangeLoss';
                                rowGroup[0].GLGeneralInfoId = loss[0].CompanyCurrencyGLId;
                                rowGroup[0].GLGeneralInfoCode = loss[0].CompanyCurrencyGLCode;
                                rowGroup[0].GLGeneralInfoName = loss[0].CompanyCurrencyGLName;
                                rowGroup[0].BudgetMasterId = loss[0].CompanyCurrencyBudgetMasterId;
                                rowGroup[0].BudgetName = loss[0].CompanyCurrencyBudgetMasterName;
                                rowGroup[0].ActivityId = loss[0].CompanyCurrencyActivityId;
                                rowGroup[0].ActivityName = loss[0].CompanyCurrencyActivityName;
                                if ($scope.companyCurrencyId !== data.CurrencyId) {
                                    rowGroup[0].CompanyCurrencyDr = (data.CrAmount * (data.CompanyCurrencyRate - advance.CompanyCurrencyRate)).toFixed(2);
                                }
                                else
                                    rowGroup[0].CompanyCurrencyDr = (data.ConvertedAmount * (data.CompanyCurrencyRate - advance.CompanyCurrencyRate)).toFixed(2);

                                rowGroup[0].CompanyCurrencyCr = null;
                                rowGroup[0].CompanyCurrencyId = $scope.companyCurrencyId;
                                rowGroup[0].CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                                rowGroup[0].CompanyCurrencyRate = advance.CompanyCurrencyRate;
                            }
                            else {
                                var loss = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });
                                var exlosslist = {};
                                exlosslist.TrnType = 'Dr';
                                exlosslist.Exchange = 'Base';
                                exlosslist.ExchangeStatus = 'ExchangeLoss';
                                exlosslist.InvoiceDetailId = data.InvoiceDetailId;
                                exlosslist.GLGeneralInfoId = loss[0].CompanyCurrencyGLId;
                                exlosslist.GLGeneralInfoCode = loss[0].CompanyCurrencyGLCode;
                                exlosslist.GLGeneralInfoName = loss[0].CompanyCurrencyGLName;
                                exlosslist.BudgetMasterId = loss[0].CompanyCurrencyBudgetMasterId;
                                exlosslist.BudgetName = loss[0].CompanyCurrencyBudgetMasterName;
                                exlosslist.ActivityId = loss[0].CompanyCurrencyActivityId;
                                exlosslist.ActivityName = loss[0].CompanyCurrencyActivityName;
                                if ($scope.companyCurrencyId !== data.CurrencyId) {
                                    exlosslist.CompanyCurrencyDr = (data.CrAmount * (data.CompanyCurrencyRate - advance.CompanyCurrencyRate)).toFixed(2);
                                }
                                else
                                    exlosslist.CompanyCurrencyDr = (data.ConvertedAmount * (data.CompanyCurrencyRate - advance.CompanyCurrencyRate)).toFixed(2);
                                exlosslist.CompanyCurrencyCr = null;
                                exlosslist.CompanyCurrencyId = $scope.companyCurrencyId;
                                exlosslist.CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                                exlosslist.CompanyCurrencyRate = advance.CompanyCurrencyRate;
                                $scope.voucherDetailCurrencyList.push(exlosslist);
                            }
                        }
                        else if (data.CompanyCurrencyRate < advance.CompanyCurrencyRate) {
                            var rowGroup = $filter('filter')($scope.voucherDetailCurrencyList, { 'InvoiceDetailId': data.InvoiceDetailId, 'Exchange': 'Base' });
                            if (!baseService.isUndefinedOrNull(rowGroup) && rowGroup.length > 0) {
                                var gain = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                                rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                                rowGroup[0].TrnType = 'Cr';
                                rowGroup[0].Exchange = 'Base';
                                rowGroup[0].ExchangeStatus = 'ExchangeGain';
                                rowGroup[0].GLGeneralInfoId = gain[0].CompanyCurrencyGLId;
                                rowGroup[0].GLGeneralInfoCode = gain[0].CompanyCurrencyGLCode;
                                rowGroup[0].GLGeneralInfoName = gain[0].CompanyCurrencyGLName;
                                rowGroup[0].BudgetMasterId = gain[0].CompanyCurrencyBudgetMasterId;
                                rowGroup[0].BudgetName = gain[0].CompanyCurrencyBudgetMasterName;
                                rowGroup[0].ActivityId = gain[0].CompanyCurrencyActivityId;
                                rowGroup[0].ActivityName = gain[0].CompanyCurrencyActivityName;
                                rowGroup[0].CompanyCurrencyDr = null;

                                if ($scope.companyCurrencyId !== data.CurrencyId) {
                                    rowGroup[0].CompanyCurrencyCr = Math.abs(data.CrAmount * (advance.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
                                }
                                else
                                    rowGroup[0].CompanyCurrencyCr = Math.abs(data.ConvertedAmount * (advance.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
                                rowGroup[0].CompanyCurrencyId = $scope.companyCurrencyId;
                                rowGroup[0].CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                                rowGroup[0].CompanyCurrencyRate = advance.CompanyCurrencyRate;
                            }
                            else {
                                var gain = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                                var exgainlist = {};
                                exgainlist.TrnType = 'Cr';
                                exgainlist.Exchange = 'Base';
                                exgainlist.ExchangeStatus = 'ExchangeGain';
                                exgainlist.InvoiceDetailId = data.InvoiceDetailId;
                                exgainlist.GLGeneralInfoId = gain[0].CompanyCurrencyGLId;
                                exgainlist.GLGeneralInfoCode = gain[0].CompanyCurrencyGLCode;
                                exgainlist.GLGeneralInfoName = gain[0].CompanyCurrencyGLName;
                                exgainlist.DocRefNo = data.DocRefNo;
                                if ($scope.companyCurrencyId !== data.CurrencyId) {
                                    exgainlist.CompanyCurrencyCr = Math.abs(data.CrAmount * (advance.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
                                }
                                else
                                    exgainlist.CompanyCurrencyCr = Math.abs(data.ConvertedAmount * (advance.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
                                exgainlist.CompanyCurrencyDr = null;
                                exgainlist.CompanyCurrencyId = $scope.companyCurrencyId;
                                exgainlist.CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                                exgainlist.CompanyCurrencyRate = advance.CompanyCurrencyRate;
                                $scope.voucherDetailCurrencyList.push(exgainlist);
                            }
                        }
                        else if (data.CompanyCurrencyRate === advance.CompanyCurrencyRate) {
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
                    getRow[0].CompanyGroupCurrencyCr = (data.CrAmount / data.CompanyGroupCurrencyRate).toFixed(2);

                    if ($scope.companyGroupCurrencyId === $scope.advance.CurrencyId) {
                        data.ConvertedAmount = getRow[0].CompanyGroupCurrencyCr;
                    }
                    // Group currecny gain/loss
                    if ($scope.advance.CurrencyId !== $scope.companyGroupCurrencyId) {
                        if (companyGroupCurrencyDrExchangeRate > companyGroupCurrencyExchangeRate) {
                            var rowGroup = $filter("filter")($scope.voucherDetailCurrencyList, { "AdvanceDetailId": advance.AdvanceDetailId, "Exchange": "Group" });
                            var loss = $filter("filter")($scope.exchangeGainLossList, { ExchangeStatus: "ExchangeLoss" });
                            rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                            rowGroup[0].AdvanceDetailId = advance.AdvanceDetailId;
                            rowGroup[0].TrnType = "Dr";
                            rowGroup[0].Exchange = "Group";
                            rowGroup[0].ExchangeStatus = "ExchangeLoss";
                            rowGroup[0].GLGeneralInfoId = loss[0].CompanyGroupCurrencyGLId;
                            rowGroup[0].GLGeneralInfoCode = loss[0].CompanyGroupCurrencyGLCode;
                            rowGroup[0].GLGeneralInfoName = loss[0].CompanyGroupCurrencyGLName;
                            rowGroup[0].CompanyGroupCurrencyDr = (data.CrAmount * (1 / companyGroupCurrencyExchangeRate - 1 / companyGroupCurrencyDrExchangeRate)).toFixed(2);
                            rowGroup[0].CompanyGroupCurrencyCr = null;
                            rowGroup[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                            rowGroup[0].CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                            rowGroup[0].CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate;
                        }
                        else if (companyGroupCurrencyDrExchangeRate < companyGroupCurrencyExchangeRate) {
                            var rowGroup = $filter("filter")($scope.voucherDetailCurrencyList, { "AdvanceDetailId": advance.AdvanceDetailId, "Exchange": "Group" });
                            var gain = $filter("filter")($scope.exchangeGainLossList, { ExchangeStatus: "ExchangeGain" });
                            rowGroup[0].AdvanceDetailId = advance.AdvanceDetailId;
                            rowGroup[0].InvoiceDetailId = data.InvoiceDetailId;
                            rowGroup[0].TrnType = "Cr";
                            rowGroup[0].Exchange = "Group";
                            rowGroup[0].ExchangeStatus = "ExchangeGain";
                            rowGroup[0].GLGeneralInfoId = gain[0].CompanyGroupCurrencyGLId;
                            rowGroup[0].GLGeneralInfoCode = gain[0].CompanyGroupCurrencyGLCode;
                            rowGroup[0].GLGeneralInfoName = gain[0].CompanyGroupCurrencyGLName;
                            rowGroup[0].CompanyGroupCurrencyDr = null;
                            rowGroup[0].CompanyGroupCurrencyCr = Math.abs(data.CrAmount * (1 / companyGroupCurrencyDrExchangeRate - 1 / companyGroupCurrencyExchangeRate)).toFixed(2);
                            rowGroup[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                            rowGroup[0].CompanyGroupFromCurrencyId = data.CompanyGroupFromCurrencyId;
                            rowGroup[0].CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate;
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
                    getRow[0].HardCurrencyCr = (data.CrAmount * data.HardCurrencyRate).toFixed(2);

                    if ($scope.hardCurrencyId === $scope.advance.CurrencyId) {
                        data.ConvertedAmount = getRow[0].HardCurrencyCr;
                    }
                }
            }
            // for first time.
            else {
                var obj = {
                    TrnType: "Cr",
                    Exchange: "No",
                    ExchangeStatus: "No",
                    InvoiceDetailId: data.InvoiceDetailId,
                    AdvanceDetailId: advance.AdvanceDetailId,
                    GLGeneralInfoId: data.GLGeneralInfoId,
                    GLGeneralInfoName: data.GLGeneralInfoName,
                    DocRefNo: data.DocRefNo,
                    BudgetMasterId: data.BudgetMasterId,
                    BudgetName: data.BudgetName,
                    ActivityId: data.ActivityId,
                    ActivityName: data.ActivityName
                };
                // Base currency
                if (!baseService.isUndefinedOrNull($scope.companyCurrencyId)) {
                    obj.CompanyCurrencyId = $scope.companyCurrencyId;
                    obj.CompanyCurrencyName = $scope.companyCurrencyName;
                    obj.CompanyFromCurrencyId = data.CompanyFromCurrencyId;
                    obj.ToCurrencyId = data.ToCurrencyId;
                    obj.CompanyCurrencyRate = data.CompanyCurrencyRate;
                    obj.CompanyCurrencyDr = null;
                    obj.CompanyCurrencyCr = (data.CrAmount * data.CompanyCurrencyRate).toFixed(2);
                    if ($scope.companyCurrencyId === $scope.advance.CurrencyId) {
                        obj.ConvertedAmount = data.CompanyCurrencyCr;
                    }

                    //***************Company Currency Exchange Gain Loss*************
                    if ($scope.companyCurrencyId !== data.CurrencyId) {
                        if (data.CompanyCurrencyRate > companyCurrencyDrExchangeRate) {
                            var loss = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeLoss' });
                            $scope.voucherDetailCurrencyList.push({
                                TrnType: 'Dr',
                                Exchange: 'Base',
                                ExchangeStatus: 'ExchangeLoss',
                                InvoiceDetailId: data.InvoiceDetailId,
                                GLGeneralInfoId: loss[0].CompanyCurrencyGLId,
                                GLGeneralInfoCode: loss[0].CompanyCurrencyGLCode,
                                GLGeneralInfoName: loss[0].CompanyCurrencyGLName,
                                BudgetMasterId: loss[0].CompanyCurrencyBudgetMasterId,
                                BudgetName: loss[0].CompanyCurrencyBudgetName,
                                ActivityId: loss[0].CompanyCurrencyActivityId,
                                ActivityName: loss[0].CompanyCurrencyActivityName,
                                CompanyCurrencyDr: Math.abs(data.Amount * (1 / companyCurrencyDrExchangeRate - 1 / data.CompanyCurrencyRate)).toFixed(2),
                                CompanyCurrencyCr: null,
                                CompanyCurrencyId: $scope.companyCurrencyId,
                                CompanyFromCurrencyId: data.CompanyFromCurrencyId,
                                CompanyCurrencyRate: companyCurrencyDrExchangeRate
                            });
                        }
                        else if (data.CompanyCurrencyRate < companyCurrencyDrExchangeRate) {
                            var gain = $filter('filter')($scope.exchangeGainLossList, { ExchangeStatus: 'ExchangeGain' });
                            $scope.voucherDetailCurrencyList.push({
                                TrnType: 'Cr',
                                Exchange: 'Base',
                                ExchangeStatus: 'ExchangeGain',
                                InvoiceDetailId: data.InvoiceDetailId,
                                GLGeneralInfoId: gain[0].CompanyCurrencyGLId,
                                GLGeneralInfoCode: gain[0].CompanyCurrencyGLCode,
                                GLGeneralInfoName: gain[0].CompanyCurrencyGLName,
                                BudgetMasterId: gain[0].CompanyCurrencyBudgetMasterId,
                                BudgetName: gain[0].CompanyCurrencyBudgetName,
                                ActivityId: gain[0].CompanyCurrencyActivityId,
                                ActivityName: gain[0].CompanyCurrencyActivityName,
                                CompanyCurrencyDr: null,
                                CompanyCurrencyCr: Math.abs(data.Amount * (1 / data.CompanyCurrencyRate) - 1 / companyCurrencyDrExchangeRate).toFixed(2),
                                CompanyCurrencyId: $scope.companyCurrencyId,
                                CompanyFromCurrencyId: data.CompanyFromCurrencyId,
                                CompanyCurrencyRate: companyCurrencyDrExchangeRate
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
                    obj.CompanyGroupCurrencyCr = (data.CrAmount / data.CompanyGroupCurrencyRate).toFixed(2);
                    if ($scope.companyGroupCurrencyId === $scope.advance.CurrencyId) {
                        obj.ConvertedAmount = data.CompanyGroupCurrencyCr;
                    }
                    // Group currecny gain/loss
                    if ($scope.advance.CurrencyId !== $scope.companyGroupCurrencyId) {
                        if (companyGroupCurrencyDrExchangeRate > companyGroupCurrencyExchangeRate) {
                            var loss = $filter("filter")($scope.exchangeGainLossList, { ExchangeStatus: "ExchangeLoss" });
                            $scope.voucherDetailCurrencyList.push({
                                TrnType: "Dr",
                                Exchange: "Group",
                                ExchangeStatus: "ExchangeLoss",
                                InvoiceDetailId: data.InvoiceDetailId,
                                AdvanceDetailId: advance.AdvanceDetailId,
                                GLGeneralInfoId: loss[0].CompanyGroupCurrencyGLId,
                                GLGeneralInfoCode: loss[0].CompanyGroupCurrencyGLCode,
                                GLGeneralInfoName: loss[0].CompanyGroupCurrencyGLName,
                                BudgetMasterId: loss[0].CompanyGroupCurrencyBudgetMasterId,
                                BudgetName: loss[0].CompanyGroupCurrencyBudgetName,
                                ActivityId: loss[0].CompanyGroupCurrencyActivityId,
                                ActivityName: loss[0].CompanyGroupCurrencyActivityName,
                                CompanyGroupCurrencyDr: Math.abs(data.CrAmount * (1 / companyCurrencyExchangeRate - 1 / companyCurrencyDrExchangeRate)).toFixed(2),
                                CompanyGroupCurrencyCr: null,
                                CompanyGroupCurrencyId: $scope.companyGroupCurrencyId,
                                CompanyGroupFromCurrencyId: data.CompanyGroupFromCurrencyId,
                                CompanyGroupCurrencyRate: companyCurrencyExchangeRate
                            });
                        }
                        else if (companyGroupCurrencyDrExchangeRate < companyGroupCurrencyExchangeRate) {
                            var gain = $filter("filter")($scope.exchangeGainLossList, { ExchangeStatus: "ExchangeGain" });
                            $scope.voucherDetailCurrencyList.push({
                                TrnType: "Cr",
                                Exchange: "Group",
                                ExchangeStatus: "ExchangeGain",
                                InvoiceDetailId: data.InvoiceDetailId,
                                AdvanceDetailId: advance.AdvanceDetailId,
                                GLGeneralInfoId: gain[0].CompanyGroupCurrencyGLId,
                                GLGeneralInfoCode: gain[0].CompanyGroupCurrencyGLCode,
                                GLGeneralInfoName: gain[0].CompanyGroupCurrencyGLName,
                                BudgetMasterId: gain[0].CompanyGroupCurrencyBudgetMasterId,
                                BudgetName: gain[0].CompanyGroupCurrencyBudgetName,
                                ActivityId: gain[0].CompanyGroupCurrencyActivityId,
                                ActivityName: gain[0].CompanyGroupCurrencyActivityName,
                                CompanyGroupCurrencyDr: null,
                                CompanyGroupCurrencyCr: Math.abs(data.CrAmount * (1 / companyCurrencyDrExchangeRate - 1 / companyCurrencyExchangeRate)).toFixed(2),
                                CompanyGroupCurrencyId: $scope.companyGroupCurrencyId,
                                CompanyGroupFromCurrencyId: data.CompanyGroupFromCurrencyId,
                                CompanyGroupCurrencyRate: companyCurrencyExchangeRate
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
                    obj.HardCurrencyCr = (data.CrAmount * data.HardCurrencyRate).toFixed(2);

                    if ($scope.hardCurrencyId === $scope.advance.CurrencyId) {
                        obj.ConvertedAmount = data.HardCurrencyCr;
                    }
                }
                $scope.voucherDetailCurrencyList.push(data);
            }
        }
    };

    $scope.updateJournalAmount = function () {
        angular.forEach($scope.voucherDetailList, function (item, i) {
            if (item.TrnType === "Dr") {
                $scope.setDrExchangeRate(item);
            }
            else if (item.TrnType === "Cr") {
                $scope.setCrExchangeRate(item);
            }
        });
    };

    $scope.convertAmountDr = function (data) {
        var dramount = parseInt(data.DrAmount), balance = parseInt(data.Balance);
        if (dramount > balance) {
            data.DrAmount = data.Balance;
            ShowResult("Payment Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        $scope.updateJournalAmount();
    };

    $scope.convertAmountCr = function (data) {
        var cramount = parseInt(data.CrAmount), balance = parseInt(data.Balance);
        if (cramount > balance) {
            data.CrAmount = data.Balance;
            ShowResult("Payment Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        $scope.updateJournalAmount();
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
            $scope.getData();
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
            $scope.getCustomerInvoiceData = function (pageno) {
                baseService.paginationBase("accounts/Invoice/GetCustomerAvailableInvoiceList", pageno, $scope.customerInvoiceParameters)
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
            $scope.getCustomerInvoiceData();
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
                    data.CrAmount = data.Balance;
                    $scope.voucherDetailList.push(data);
                    $scope.setCrExchangeRate(data);
                    if ($scope.voucherDetailList.length > 0)
                        $scope.isReadOnly = true;
                    else
                        $scope.isReadOnly = false;
                    angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
                    $scope.convertAmountCr(data);
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
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.customerAdvanceDataList = [];
            ShowResult("Please select Customer.", "failure");
        }
        else {
            $scope.compareCurrencyId = $scope.advance.CurrencyId;
            $scope.customerAdvanceParameters.partyId = partyId;
            $scope.customerAdvanceParameters.partyPlantId = partyPlantId;
            $scope.getCustomerAdvanceData = function (pageno) {
                baseService.paginationBase("accounts/Advance/GetAvilabeCustomerSuspenseList", pageno, $scope.customerAdvanceParameters)
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
        }
    };

    $scope.closeCustomerAdvancePopUp = function () {
        angular.element(document.querySelector("#customerAdvancePopUp")).modal("hide");
    };

    $scope.closeCustomerAdvancePopUpSelected = function (data) {
        data.TrnType = "Dr";
        var getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "DocRefNo": data.DocRefNo });
        if (getRow.length === 0) {
            data.CompanyId = data.CompanyId;
            data.PlantId = data.PlantId;
            data.PartyType = data.PartyType;
            data.CrAmount = 0;
            data.Amount = data.Receivable;
            data.WriteOff = data.Received;
            data.Advilable = data.Balance;
            data.DrAmount = data.Balance;
            $scope.voucherDetailList.push(data);
            $scope.setDrExchangeRate(data);
            if ($scope.voucherDetailList.length > 0)
                $scope.isReadOnly = true;
            else
                $scope.isReadOnly = false;
            angular.element(document.querySelector("#customerAdvancePopUp")).modal("hide");
            $scope.convertAmountDr(data);
        }
        else {
            ShowResult(data.AdvanceNo + " already  Exist", "failure", "customerAdvancePopUp");
        }
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

    //*********************** Customer Advance PopUp End *************************************

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

    $scope.report = function (voucherId) {
        location.href = $scope.reportUrl + voucherId;
    };

    cboService.getCboVoucherTypeAdvanceTakenWriteOffList(function (result) {
        $scope.voucherTypeList = result;
        if ($scope.voucherTypeList.length === 1) {
            $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
            $scope.advance.PostingDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
            $scope.advance.DocDate = $scope.advance.PostingDate;
        }
    });

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkPostingDate();
        $scope.redirectTab();
        if ($scope.form0.$valid && !$scope.validation() && $scope.checkDrCrBalancing()) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.parkUrl,
                    data: {
                        "advanceVM": $scope.advance,
                        "advanceDetailVMList": $scope.voucherDetailList,
                        "currencyList": $scope.voucherDetailCurrencyList
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
                        "voucherVM": $scope.advance,
                        "voucherDetailList": $scope.voucherDetailList,
                        "currencyList": $scope.voucherDetailCurrencyList
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

    $scope.checkDrCrBalancing = function () {
        var companyCurrencyAmountDr = $filter("sumByKey")($filter("filter")($scope.voucherDetailCurrencyList, { TrnType: "Dr" }), "CompanyCurrencyDr");
        var companyCurrencyAmountCr = $filter("sumByKey")($filter("filter")($scope.voucherDetailCurrencyList, { TrnType: "Cr" }), "CompanyCurrencyCr");
        if (companyCurrencyAmountDr === 0) {
            ShowResult($scope.companyCurrencyCode + " Dr amount can not zero!", "failure");
            $scope.setTab(2);
            return false;
        }
        if (companyCurrencyAmountCr === 0) {
            ShowResult($scope.companyCurrencyCode + " Cr amount can not zero!", "failure");
            $scope.setTab(2);
            return false;
        }
        if (companyCurrencyAmountDr !== companyCurrencyAmountCr) {
            ShowResult($scope.companyCurrencyCode + " Dr amount and Cr amount is not equal!", "failure");
            $scope.setTab(2);
            return false;
        }
        if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
            var companyGroupCurrencyAmountDr = $filter("sumByKey")($filter("filter")($scope.voucherDetailCurrencyList, { TrnType: "Dr" }), "CompanyGroupCurrencyDr");
            var companyGroupCurrencyAmountCr = $filter("sumByKey")($filter("filter")($scope.voucherDetailCurrencyList, { TrnType: "Cr" }), "CompanyGroupCurrencyCr");
            if (companyGroupCurrencyAmountDr === 0) {
                ShowResult($scope.companyGroupCurrencyCode + " Dr amount can not zero!", "failure");
                $scope.setTab(2);
                return false;
            }
            if (companyGroupCurrencyAmountCr === 0) {
                ShowResult($scope.companyGroupCurrencyCode + " Cr amount can not zero!", "failure");
                $scope.setTab(2);
                return false;
            }
            if (companyGroupCurrencyAmountDr !== companyGroupCurrencyAmountCr) {
                ShowResult($scope.companyGroupCurrencyCode + " Dr amount and Cr amount is not equal!", "failure");
                $scope.setTab(2);
                return false;
            }
        }
        if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
            var hardCurrencyAmountDr = $filter("sumByKey")($filter("filter")($scope.voucherDetailCurrencyList, { TrnType: "Dr" }), "HardCurrencyDr");
            var hardCurrencyAmountCr = $filter("sumByKey")($filter("filter")($scope.voucherDetailCurrencyList, { TrnType: "Cr" }), "HardCurrencyCr");
            if (hardCurrencyAmountDr === 0) {
                ShowResult($scope.hardCurrencyCode + " Dr amount can not zero!", "failure");
                $scope.setTab(2);
                return false;
            }
            if (hardCurrencyAmountCr === 0) {
                ShowResult($scope.hardCurrencyCode + " Cr amount can not zero!", "failure");
                $scope.setTab(2);
                return false;
            }
            if (hardCurrencyAmountDr !== hardCurrencyAmountCr) {
                ShowResult($scope.hardCurrencyCode + " Dr amount and Cr amount is not equal!", "failure");
                $scope.setTab(2);
                return false;
            }
        }
        return true;
    };
}