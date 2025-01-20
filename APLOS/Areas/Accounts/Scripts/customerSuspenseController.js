"use strict";
customerSuspenseController.$inject = ["cboService", "baseService", "factoryService", "commonMessage", "$scope", "$rootScope", "$http", "$filter", "$controller", "$routeParams"];
function customerSuspenseController(cboService, baseService, factoryService, commonMessage, $scope, $rootScope, $http, $filter, $controller, $routeParams) {
    $rootScope.title = "Customer Suspense";
    $scope.Action = "Save";
    $scope.isBankAmount = false;
    $scope.hideSource = true;
    $scope.voucherDetailCurrency = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.index = -1;
    $scope.url = 'accounts/Advance';
    $scope.listUrl = $scope.url + '/GetCustomerSuspenseList';
    $scope.parkUrl = $scope.url + '/ParkCustomerSuspense';
    $scope.updateUrl = $scope.url + '/UpdateCustomerSuspense';
    $scope.postUrl = $scope.url + '/PostCustomerSuspense';
    $scope.reportUrl = $scope.url + '/ReportCustomerSuspense?voucherId=';
    $scope.jouranlUrl = $scope.url + '/GetAvailableJournalCustomerSuspense';
    $scope.unPostUrl = $scope.url + '/UnPostCustomerAdvance';

    $scope.partyType = "Customer";
    $scope.partyGLType = "Suspense";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    baseService.init($scope.listUrl, null, null, "DESC", "PostingDate DESC, AdvanceNo", "AdvanceNo");
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
            "name": "#No",
            "value": "AdvanceNo"
        },
        {
            "name": "Customer Code",
            "value": "PartyCode"
        },
        {
            "name": "Customer Name",
            "value": "PartyName"
        },
        {
            "name": "Ordering Customer",
            "value": "PartyPlantName"
        },
        {
            "name": "Posting Date",
            "value": "PostingDate"
        },
        {
            "name": "Entity",
            "value": "EntityName"
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

    $scope.advance = {
        Id: null,
        AdvanceId: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        EntityId: null,
        PartyId: null,
        PartyCode: null,
        PartyName: null,
        PartyType: null,
        PartyPlantId: null,
        PartyPlantName: null,
        CurrencyId: null,
        PaymentTermId: null,
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
        Amount: 0,
        Narration: null,
        BankName: null,
        CashName: null,
        BankMasterId: null,
        CashMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        PaymentSource: 'Bank',
        GLGeneralInfoId: null,
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetCode: null,
        BudgetName: null,
        ActivityId: null,
        ActivityCode: null,
        ActivityName: null,
        EmployeeGLGeneralInfoId: null,
        EmployeeGLGeneralInfoName: null,
        EmployeeTransactionTypeId: null,
        FinancingTypeId: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        IsInterTransaction: false
    };

    $scope.advanceDetail = {
        Id: null,
        AdvanceId: null,
        PartyId: null,
        PartyCode: null,
        PartyName: null,
        PartyPlantId: null,
        PartyPlantName: null,
        PartyType: null,
        Narration: null,
        GLGeneralInfoId: null,
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetCode: null,
        BudgetName: null,
        ActivityId: null,
        ActivityCode: null,
        ActivityName: null,
        Amount: 0,
        TaxAmount: 0,
        NetAmount: 0
    };

    $scope.advanceDetailList = [];

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
        $http.get('Accounts/Advance/GetAdvanceForJournal?advanceId=' + advanceId)
            .then(function (response) {
                var advance = response.data;
                $scope.advance.Id = null;
                $scope.advance.PartyType = $scope.partyType;
                $scope.advance.PartyId = advance.PartyId;
                $scope.advance.PartyCode = advance.PartyCode;
                $scope.advance.PartyName = advance.PartyCode + " - " + advance.PartyName;
                $scope.advance.PartyPlantId = advance.PartyPlantId;
                //$scope.advance.IsInterTransaction = true;
                $scope.advance.CompanyId = advance.CompanyId;
                $scope.advance.PlantId = advance.CompanyId;
                $scope.advance.JournalId = advance.JournalId;

                $scope.advance.DocRefNo = advance.DocRefNo;
                $scope.advance.Narration = advance.Narration;
                $scope.advance.FinancingTypeId = advance.FinancingTypeId;
                $scope.advance.Amount = advance.Amount;
                $scope.advance.CurrencyId = advance.CurrencyId;
                $scope.advance.AdvanceNo = advance.AdvanceNo;
                $scope.advance.PaymentSource = 'Journal';
                $scope.GetCurrencyExchangeRateList();

                $http.get('Parties/Party/GetCompanyPartyDownPaymentGL?partyId=' + $scope.advance.PartyId + "&partyType=" + $scope.advance.PartyType)
                    .then(function (response) {
                        var data = response.data;

                        $scope.advanceDetail.GLGeneralInfoId = data.GLGeneralInfoId;
                        $scope.advanceDetail.GLGeneralInfoCode = data.GLGeneralInfoCode;
                        $scope.advanceDetail.GLGeneralInfoName = data.GLGeneralInfoName;
                        $scope.advanceDetail.BudgetMasterId = data.BudgetMasterId;
                        $scope.advanceDetail.BudgetCode = data.BudgetCode;
                        $scope.advanceDetail.BudgetName = data.BudgetName;
                        $scope.advanceDetail.ActivityId = data.ActivityId;
                        $scope.advanceDetail.ActivityCode = data.ActivityCode;
                        $scope.advanceDetail.ActivityName = data.ActivityName;

                        // Set to AdvanceDetail
                        $scope.advanceDetail.PartyType = $scope.advance.PartyType;
                        $scope.advanceDetail.PartyId = $scope.advance.PartyId;
                        $scope.advanceDetail.PartyCode = $scope.advance.PartyCode;
                        $scope.advanceDetail.PartyName = $scope.advance.PartyName;
                        $scope.advanceDetail.Narration = $scope.advance.Narration;
                        $scope.advanceDetail.Amount = $scope.advance.Amount;

                        $scope.getPartyPlantList($scope.advance.PartyId);
                        $scope.closeAdvanceInterTransactionPopUp(advance);
                    });
            });
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.getById = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/Advance/GetAdvance/' + id
        }).then(function successCallback(response) {
            $scope.advance = response.data;
            $scope.advance.DocDate = $filter('dateFiltering')($scope.advance.DocDate);
            $scope.advance.VoucherDate = $filter('dateFiltering')($scope.advance.VoucherDate);
            $scope.advance.PostingDate = $filter('dateFiltering')($scope.advance.PostingDate);
            $scope.advance.ReviewDate = $filter('dateFiltering')($scope.advance.ReviewDate);
            $scope.getPartyPlantList($scope.advance.PartyId, true);

            $http({
                method: 'GET',
                url: 'accounts/Advance/GetAdvanceDetail?advanceId=' + id
            }).then(function successCallback(response) {
                $scope.advanceDetailList = response.data;
                $scope.GetCurrencyExchangeRateList();

                if (!baseService.isUndefinedOrNull($scope.advance.BankMasterId)) {
                    factoryService.getBankMasterGL($scope.advance.BankMasterId, function (result) {
                        setBankGL(result);
                    });
                }

                if (!baseService.isUndefinedOrNull($scope.advance.CashMasterId)) {
                    factoryService.getCashMasterGL($scope.advance.CashMasterId, function (result) {
                        setCashGL(result);
                    });
                }
            });

            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };

    $scope.setDrExchangeRate = function (glId) {
        if (!baseService.isUndefinedOrNull(glId)) {
            var companyCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: "CompanyCurrency" });
            var companyGroupCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: "CompanyGroupCurrency" });
            var hardCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: "HardCurrency" });
            var getRow = $filter("filter")($scope.voucherDetailCurrencyList, { "TrnType": "Dr" });
            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
                getRow[0].TrnType = "Dr";
                getRow[0].GLGeneralInfoId = glId;
                getRow[0].GLGeneralInfoCode = $scope.advance.GLGeneralInfoCode;
                getRow[0].GLGeneralInfoName = $scope.advance.GLGeneralInfoName;
                getRow[0].BudgetMasterId = $scope.advance.BudgetMasterId;
                getRow[0].BudgetCode = $scope.advance.BudgetCode;
                getRow[0].BudgetName = $scope.advance.BudgetName;
                getRow[0].ActivityId = $scope.advance.ActivityId;
                getRow[0].ActivityCode = $scope.advance.ActivityCode;
                getRow[0].ActivityName = $scope.advance.ActivityName;
                // Base currency
                getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                getRow[0].CompanyCurrencyName = $scope.companyCurrencyName;
                getRow[0].CompanyFromCurrencyId = companyCurrencyExchangeRate[0].FromCurrencyId;
                getRow[0].ToCurrencyId = companyCurrencyExchangeRate[0].ToCurrencyId;
                getRow[0].CompanyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;
                getRow[0].CompanyCurrencyDr = ($scope.advance.Amount * companyCurrencyExchangeRate[0].ToCurrencyRate).toFixed(2);
                getRow[0].CompanyCurrencyCr = null;
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    getRow[0].CompanyGroupCurrencyDr = ($scope.advance.Amount / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(2);
                    getRow[0].CompanyGroupCurrencyCr = null;
                }
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    getRow[0].HardCurrencyId = $scope.hardCurrencyId;
                    getRow[0].HardCurrencyName = $scope.companyCurrencyName;
                    getRow[0].HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                    getRow[0].HardCurrencyCr = null;
                    getRow[0].HardCurrencyDr = ($scope.advance.Amount * hardCurrencyExchangeRate[0].ToCurrencyRate).toFixed(2);
                }
            }
            else {
                var data = {
                    TrnType: "Dr",
                    GLGeneralInfoId: glId,
                    GLGeneralInfoCode: $scope.advance.GLGeneralInfoCode,
                    GLGeneralInfoName: $scope.advance.GLGeneralInfoName,
                    BudgetMasterId: $scope.advance.BudgetMasterId,
                    BudgetCode: $scope.advance.BudgetCode,
                    BudgetName: $scope.advance.BudgetName,
                    ActivityId: $scope.advance.ActivityId,
                    ActivityCode: $scope.advance.ActivityCode,
                    ActivityName: $scope.advance.ActivityName,
                    // Base currency
                    CompanyCurrencyId: $scope.companyCurrencyId,
                    CompanyCurrencyName: $scope.companyCurrencyName,
                    CompanyFromCurrencyId: companyCurrencyExchangeRate[0].FromCurrencyId,
                    ToCurrencyId: companyCurrencyExchangeRate[0].ToCurrencyId,
                    CompanyCurrencyRate: companyCurrencyExchangeRate[0].ToCurrencyRate,
                    CompanyCurrencyDr: ($scope.advance.Amount * companyCurrencyExchangeRate[0].ToCurrencyRate).toFixed(2),
                    CompanyCurrencyCr: null
                };
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    data.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    data.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;
                    data.CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                    data.CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    data.CompanyGroupCurrencyDr = ($scope.advance.Amount / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(2);
                    data.CompanyGroupCurrencyCr = null;
                }
                // Hard currency
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    data.HardCurrencyId = $scope.hardCurrencyId;
                    data.HardCurrencyName = $scope.hardCurrencyName;
                    data.HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                    data.HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                    data.HardCurrencyDr = ($scope.advance.Amount * hardCurrencyExchangeRate[0].ToCurrencyRate).toFixed(2);
                    data.HardCurrencyCr = null;
                }
                $scope.voucherDetailCurrencyList.push(data);
            }
        }
    };

    $scope.setCrExchangeRate = function (data) {
        if (!baseService.isUndefinedOrNull(data.GLGeneralInfoId)) {
            var companyCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: "CompanyCurrency" });
            var companyGroupCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: "CompanyGroupCurrency" });
            var hardCurrencyExchangeRate = $filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: "HardCurrency" });

            var getRow = $filter("filter")($scope.voucherDetailCurrencyList, { "TrnType": "Cr", "GLGeneralInfoId": data.GLGeneralInfoId });
            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
                getRow[0].TrnType = "Cr";
                getRow[0].GLGeneralInfoId = data.GLGeneralInfoId;
                getRow[0].GLGeneralInfoCode = data.GLGeneralInfoCode;
                getRow[0].GLGeneralInfoName = data.GLGeneralInfoName;
                getRow[0].BudgetMasterId = data.BudgetMasterId;
                getRow[0].BudgetCode = data.BudgetCode;
                getRow[0].BudgetName = data.BudgetName;
                getRow[0].ActivityId = data.ActivityId;
                getRow[0].ActivityCode = data.ActivityCode;
                getRow[0].ActivityName = data.ActivityName;

                // Base currency
                getRow[0].CompanyCurrencyId = $scope.companyCurrencyId;
                getRow[0].CompanyCurrencyName = $scope.companyCurrencyName;
                getRow[0].CompanyFromCurrencyId = companyCurrencyExchangeRate[0].FromCurrencyId;
                getRow[0].ToCurrencyId = companyCurrencyExchangeRate[0].ToCurrencyId;
                getRow[0].CompanyCurrencyRate = companyCurrencyExchangeRate[0].ToCurrencyRate;
                getRow[0].CompanyCurrencyDr = null;
                getRow[0].CompanyCurrencyCr = (data.Amount * companyCurrencyExchangeRate[0].ToCurrencyRate).toFixed(2);
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    getRow[0].CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    getRow[0].CompanyGroupCurrencyName = $scope.companyCurrencyName;
                    getRow[0].CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    getRow[0].CompanyGroupCurrencyDr = null;
                    getRow[0].CompanyGroupCurrencyCr = (data.Amount / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(2);
                }
                // Hard currency
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    getRow[0].HardCurrencyId = $scope.hardCurrencyId;
                    getRow[0].HardCurrencyName = $scope.companyCurrencyName;
                    getRow[0].HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                    getRow[0].HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                    getRow[0].HardCurrencyDr = null;
                    getRow[0].HardCurrencyCr = (data.Amount * hardCurrencyExchangeRate[0].ToCurrencyRate).toFixed(2);
                }
            }
            else {
                var newData = {
                    TrnType: "Cr",
                    GLGeneralInfoId: data.GLGeneralInfoId,
                    GLGeneralInfoCode: data.GLGeneralInfoCode,
                    GLGeneralInfoName: data.GLGeneralInfoName,
                    BudgetMasterId: data.BudgetMasterId,
                    BudgetCode: data.BudgetCode,
                    BudgetName: data.BudgetName,
                    ActivityId: data.ActivityId,
                    ActivityCode: data.ActivityCode,
                    ActivityName: data.ActivityName,

                    // Base currency
                    CompanyCurrencyId: $scope.companyCurrencyId,
                    CompanyCurrencyName: $scope.companyCurrencyName,
                    CompanyFromCurrencyId: companyCurrencyExchangeRate[0].FromCurrencyId,
                    ToCurrencyId: companyCurrencyExchangeRate[0].ToCurrencyId,
                    CompanyCurrencyRate: companyCurrencyExchangeRate[0].ToCurrencyRate,
                    CompanyCurrencyDr: null,
                    CompanyCurrencyCr: (data.Amount * companyCurrencyExchangeRate[0].ToCurrencyRate).toFixed(2)
                };
                // Consolidated currency
                if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
                    newData.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    newData.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;
                    newData.CompanyGroupFromCurrencyId = companyGroupCurrencyExchangeRate[0].FromCurrencyId;
                    newData.CompanyGroupCurrencyRate = companyGroupCurrencyExchangeRate[0].ToCurrencyRate;
                    newData.CompanyGroupCurrencyDr = null;
                    newData.CompanyGroupCurrencyCr = (data.Amount / companyGroupCurrencyExchangeRate[0].ToCurrencyRate).toFixed(2);
                }
                // Hard currency
                if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
                    newData.HardCurrencyId = $scope.hardCurrencyId;
                    newData.HardCurrencyName = $scope.hardCurrencyName;
                    newData.HardFromCurrencyId = hardCurrencyExchangeRate[0].FromCurrencyId;
                    newData.HardCurrencyRate = hardCurrencyExchangeRate[0].ToCurrencyRate;
                    newData.HardCurrencyDr = null;
                    newData.HardCurrencyCr = (data.Amount * hardCurrencyExchangeRate[0].ToCurrencyRate).toFixed(2);
                }
                $scope.voucherDetailCurrencyList.push(newData);
            }
        }
    };

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.advance.PostingDate) && !baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/ParallelExchangeRate?fromdate=" + $scope.advance.PostingDate + "&currencyId=" + $scope.advance.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.setDrExchangeRate($scope.advance.GLGeneralInfoId);
                $scope.updateCrAmount();
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.copyAmount = function () {
        var getRow = $filter("filter")($scope.voucherDetailCurrencyList, { "TrnType": "Dr" });
        if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
            if ($scope.advance.BankCurrencyId === $scope.companyCurrencyId) {
                $scope.advance.BankAmount = getRow[0].CompanyCurrencyDr;
            }
            if ($scope.advance.BankCurrencyId === $scope.companyGroupCurrencyId) {
                $scope.advance.BankAmount = getRow[0].CompanyGroupCurrencyDr;
            }
            if ($scope.advance.BankCurrencyId === $scope.hardCurrencyId) {
                $scope.advance.BankAmount = getRow[0].HardCurrencyDr;
            }
        }
    };

    $scope.checkBankAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.advance.BankCurrencyId)) {
            if ($scope.advance.BankCurrencyId !== $scope.advance.CurrencyId) {
                if ($scope.advance.BankCurrencyId !== $scope.companyCurrencyId) {
                    if ($scope.advance.BankCurrencyId !== $scope.companyGroupCurrencyId) {
                        if ($scope.advance.BankCurrencyId !== $scope.hardCurrencyId) {
                            $scope.isBankAmount = true;
                            $scope.advance.BankAmount = 0;
                        }
                    }
                    else {
                        $scope.isBankAmount = false;
                        $scope.advance.BankAmount = 0;
                    }
                }
                else {
                    $scope.isBankAmount = false;
                    $scope.advance.BankAmount = 0;
                }
            }
            else {
                $scope.isBankAmount = false;
                $scope.advance.BankAmount = 0;
            }
        }
        else {
            $scope.isBankAmount = false;
            $scope.advance.BankAmount = 0;
        }
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.advance.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else {
            $scope.invalidDocDate = false;
            $scope.GetCurrencyExchangeRateList();
        }
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
            $scope.GetCurrencyExchangeRateList();
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
            if (parseFloat($scope.advance.Amount) === 0) {
                ShowResult("Advance Amount must greater than 0!", "failure");
                return true;
            }
            if (baseService.isUndefinedOrNull($scope.advance.GLGeneralInfoId)) {
                ShowResult("Please select Cash or Bank!", "failure");
                return true;
            }
        }
        else if ($scope.partyType === "Vendor") {
            if ($scope.advance.PartyId === null) {
                ShowResult("Please select Vendor!", "failure");
                return true;
            }
            if ($scope.advance.GLGeneralInfoId === null) {
                ShowResult("Please select Cash or Bank!", "failure");
                return true;
            }
        }
        else if ($scope.partyType === "Employee") {
            if ($scope.advance.EmployeeId === null) {
                ShowResult("Please select Employee!", "failure");
                return true;
            }
            if ($scope.advance.GLGeneralInfoId === null) {
                ShowResult("Please select Cash or Bank!", "failure");
                return true;
            }
        }
        return false;
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

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.advance.ResponsiblePersonId = employee.SystemId;
            $scope.advance.ResponsiblePerson = employee.EmployeeName;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.clearEmployeePopUp = function () {
        $scope.advance.ResponsiblePersonId = null;
        $scope.advance.ResponsiblePerson = null;
    };

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.clearDrData();
            if (baseService.isUndefinedOrNull(party.SuspenseGLId)) {
                ShowResult("Customer SuspenseGL not found!", "failure", "partyPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.SuspenseBudgetId)) {
                ShowResult("Customer budget not found!", "failure", "partyPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(party.CurrencyId)) {
                ShowResult('Customer transaction currency not found!', 'failure', 'partyPopUp');
                return;
            }
            else {
                $scope.advanceDetail.GLGeneralInfoId = party.SuspenseGLId;
                $scope.advanceDetail.GLGeneralInfoCode = party.SuspenseGLCode;
                $scope.advanceDetail.GLGeneralInfoName = party.SuspenseGLName;
                $scope.advanceDetail.BudgetMasterId = party.SuspenseBudgetId;
                $scope.advanceDetail.BudgetCode = party.SuspenseBudgetCode;
                $scope.advanceDetail.BudgetName = party.SuspenseBudgetName;
                $scope.advanceDetail.ActivityId = party.SuspenseActivityId;
                $scope.advanceDetail.ActivityCode = party.SuspenseActivityCode;
                $scope.advanceDetail.ActivityName = party.SuspenseActivityName;
            }

            // Set to Advance
            $scope.advance.PartyId = party.Id;
            $scope.advance.PartyCode = party.Code;
            $scope.advance.PartyName = party.Code + " - " + party.UserName;
            $scope.advance.PartyType = party.PartyType;
            $scope.advance.CurrencyId = party.CurrencyId;
            $scope.advance.TotalPartyPlant = party.TotalPartyPlant;

            // Set to AdvanceDetail
            $scope.advanceDetail.PartyId = party.Id;
            $scope.advanceDetail.PartyCode = party.Code;
            $scope.advanceDetail.PartyName = party.Code + " - " + party.UserName;
            $scope.advanceDetail.PartyType = party.PartyType;

            $scope.GetCurrencyExchangeRateList();
            $scope.checkBankAmount();
            $scope.getPartyPlantList(party.Id);
        }
        $scope.hidePartyPopUp();
    };

    // Clear Dr. data if pary selectionn change
    $scope.clearDrData = function () {
        $scope.advanceDetailList = [];
        for (var i = 0; i < $scope.voucherDetailCurrencyList.length; i++) {
            if ($scope.voucherDetailCurrencyList[i].TrnType === "Cr") {
                $scope.voucherDetailCurrencyList.splice(i, 1);
            }
        }
    };

    $scope.updateCrAmount = function () {
        angular.forEach($scope.advanceDetailList, function (item, i) {
            if (!$scope.advance.IsInterTransaction) {
                item.Narration = $scope.advance.Narration;
                item.Amount = $scope.advance.Amount;
            }
            if (!$scope.advance.IsInterTransaction) {
                item.Amount = $scope.advance.Amount;
            }
            $scope.setCrExchangeRate(item);
        });
    };

    $scope.removeRow = function (index) {
        $scope.advanceDetailList.splice(index, 1);
        $scope.updateCrAmount();
    };

    $scope.clearPartyPopUp = function () {
        $scope.advance.PartyId = null;
        $scope.advance.PartyCode = null;
        $scope.advance.PartyName = null;
        $scope.advance.PartyType = null;
        $scope.advance.CurrencyId = null;
        $scope.advance.TotalPartyPlant = null;
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
                        $scope.advanceDetail.PartyPlantId = item.Value;
                        $scope.advanceDetail.PartyPlantName = item.Text;
                        $scope.advanceDetailList.push($scope.advanceDetail);
                        $scope.setCrExchangeRate($scope.advanceDetail);
                    }
                });
                $scope.advanceDetail = {};
            });
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
        $scope.setDrExchangeRate($scope.advance.GLGeneralInfoId);
        $scope.checkBankAmount();
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
        for (var i = 0; i < $scope.voucherDetailCurrencyList.length; i++) {
            if ($scope.voucherDetailCurrencyList[i].TrnType === "Dr") {
                $scope.voucherDetailCurrencyList.splice(i, 1);
            }
        }
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
        $scope.setDrExchangeRate($scope.advance.GLGeneralInfoId);
        $scope.checkBankAmount();
    }

    $scope.clearCashPopUp = function () {
        $scope.clearBankPopUp();
    };

    $scope.clear = function () {
        $scope.Action = "Save";
        if ($scope.voucherTypeList.length === 1) {
            $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
        }
        $scope.advance.Active = true;
        $scope.advance.DocRefNo = null;
        $scope.advance.PaymentSource = 'Bank';
        $scope.advance.ReviewDate = null;
        $scope.advance.Amount = 0;
        $scope.advance.Narration = null;
        $scope.advance.IsInterTransaction = false;
        $scope.advance.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.currencyExchangeRate = [];
        $scope.advanceDetailList = [];
        $scope.voucherDetailCurrencyList = [];
        $scope.clearPartyPopUp();
        $scope.clearBankPopUp();
        $scope.clearCashPopUp();
        $scope.clearEmployeePopUp();
        $scope.setTab(1);
    };

    $scope.report = function (voucherId) {
        location.href = $scope.reportUrl + voucherId;
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

    cboService.getCboVoucherTypeCustomerSuspense(function (result) {
        $scope.voucherTypeList = result;
        if ($scope.voucherTypeList.length === 1) {
            $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
            $scope.advance.PostingDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
            $scope.advance.DocDate = $scope.advance.PostingDate;
        }
    });

    cboService.getCboInterCompanyFinancingType("InterTransaction", function (result) {
        $scope.financingTypeList = result;
    });

    $scope.transactionTypeGL = null;
    $scope.getTransactionTypeGL = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $scope.transactionTypeGL = $.grep($scope.financingTypeList, function (item) {
                return item.FinancingTypeId === id;
            })[0];
            if (manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.transactionTypeGL.LiabilityGLId), 'Transaction Type GL not found!')) {
                $scope.transactionTypeGL = null;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget
                && manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.transactionTypeGL.LiabilityBudgetMasterId), 'Transaction Type Budget not found!')) {
                $scope.transactionTypeGL = null;
            }
        }
        else {
            manualValidation('div_TransactionType', false, '');
            $scope.transactionTypeGL = null;
        }
    };

    cboService.getCboInterCompany(null, function (result) {
        $scope.companyList = result;
    });

    $scope.companyChange = function (companyId) {
        cboService.getCboInterPlant('', companyId, '', function (result) {
            $scope.interplantList = result;
        });
    };

    $scope.save = function () {
        $scope.redirectTab();
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
            $scope.entityValidation();
        if ($scope.form0.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidEntity && !$scope.invalidPostingDate && $scope.checkDrCrBalancing()) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.parkUrl,
                    data: {
                        "advanceVM": $scope.advance,
                        "advanceDetailVMList": $scope.advanceDetailList,
                        "currencyList": $scope.voucherDetailCurrencyList
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
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: {
                        "advanceVM": $scope.advance,
                        "advanceDetailVMList": $scope.advanceDetailList,
                        "currencyList": $scope.voucherDetailCurrencyList
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
            }
            return true;
        }
        return true;
    };
    $scope.voucher_Post = {
        Id: null,
        EntityId: null,
        CurrencyId: null,
        CurrencyCode: null,
        VoucherNo: null,
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        Narration: null,
        Amount: null
    };
    $scope.advanceId = null;
    $scope.EntityId_Post = null;
    $scope.voucherId = null;
    $scope.confirmPost = function (advanceId, data) {
        $scope.advanceId = advanceId;
        $scope.EntityId_Post = data.EntityId;
        $scope.voucherId = data.VoucherId;
        $scope.voucher_Post = data;
        angular.element(document.querySelector('#PostPopUp')).modal('show');
        //$scope.message_confirmation = 'Are you sure to Post?';
        //angular.element(document.querySelector('#confirmPostPopUp')).modal('show');
    };
    $scope.closePostPopUp = function () {
        angular.element(document.querySelector("#PostPopUp")).modal("hide");
    };
    $scope.post = function () {
        if ($scope.EntityId_Post == null || $scope.EntityId_Post == "" || $scope.EntityId_Post == undefined) {
            ShowResult("Please select Entity First!!", "failure");
        }
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "advanceId": $scope.advanceId,
                "entityId": $scope.EntityId_Post,
                "voucherId": $scope.voucherId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                $scope.closePostPopUp();
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.confirmUnPost = function (advanceId) {
        $scope.advanceId = advanceId;
        $scope.message_confirmation = 'Are you sure to UnPost?';
        angular.element(document.querySelector('#confirmUnPostPopUp')).modal('show');
    };

    $scope.unPost = function (advanceId) {
        $http({
            method: "POST",
            url: $scope.unPostUrl,
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
                $scope.clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.company = null;
    $scope.getCompanyInfo = function (companyId) {
        if (!baseService.isUndefinedOrNull(companyId)) {
            $scope.company = $.grep($scope.companyList, function (item) {
                return item.CompanyId === companyId;
            })[0];
            if (manualValidation('div_Company', baseService.isUndefinedOrNull($scope.company.PartyId), 'This Company is not created as InterCompany Party.')) {
                $scope.company = null;
            }
        }
        else {
            manualValidation('div_Company', false, '');
            $scope.company = null;
        }
    };

    $scope.plant = null;
    $scope.getPlantInfo = function (plantId) {
        if (!baseService.isUndefinedOrNull(companyId)) {
            $scope.plant = $.grep($scope.interplantList, function (item) {
                return item.PlantId === plantId;
            })[0];
            if (manualValidation('div_Plant', baseService.isUndefinedOrNull($scope.plant.PartyPlantId), 'This Company is not created as InterCompany Party Plant.')) {
                $scope.plant = null;
            }
        }
        else {
            manualValidation('div_Plant', false, '');
            $scope.plant = null;
        }
    };

    $scope.addRow = function () {
        $scope.advanceDetail.GLGeneralInfoId = $scope.transactionTypeGL.LiabilityGLId;
        $scope.advanceDetail.GLGeneralInfoCode = $scope.transactionTypeGL.LiabilityGLCode;
        $scope.advanceDetail.GLGeneralInfoName = $scope.transactionTypeGL.LiabilityGLName;
        $scope.advanceDetail.BudgetMasterId = $scope.transactionTypeGL.LiabilityBudgetMasterId;
        $scope.advanceDetail.BudgetCode = $scope.transactionTypeGL.LiabilityBudgetCode;
        $scope.advanceDetail.BudgetName = $scope.transactionTypeGL.LiabilityBudgetName;
        $scope.advanceDetail.ActivityId = $scope.transactionTypeGL.LiabilityActivityId;
        $scope.advanceDetail.ActivityCode = $scope.transactionTypeGL.LiabilityActivityCode;
        $scope.advanceDetail.ActivityName = $scope.transactionTypeGL.LiabilityActivityName;

        $scope.advanceDetail.PartyType = $scope.company.PartyType;
        $scope.advanceDetail.CompanyId = $scope.company.CompanyId;
        $scope.advanceDetail.PartyId = $scope.company.PartyId;
        $scope.advanceDetail.PartyCode = $scope.company.PartyCode;
        $scope.advanceDetail.PartyName = $scope.company.PartyCode + " - " + $scope.company.PartyName;
        $scope.advanceDetail.PlantId = $scope.plant.PlantId;
        $scope.advanceDetail.PartyPlantId = $scope.plant.PartyPlantId;
        $scope.advanceDetail.PartyPlantName = $scope.plant.PartyPlantName;

        $scope.advanceDetailList.push($scope.advanceDetail);
        $scope.advanceDetail = {};
    };

    $scope.advanceInterTransactionSearchByList = [
        {
            'name': '#No',
            'value': 'AdvanceNo'
        },
        {
            'name': 'Company',
            'value': 'CompanyName'
        },
        {
            'name': 'Plant',
            'value': 'PlantName'
        },
        {
            'name': 'Party Code',
            'value': 'PartyCode'
        },
        {
            'name': 'Party Name',
            'value': 'PartyName'
        },
        {
            'name': 'Party Plant',
            'value': 'PartyPlantName'
        }
    ];

    $scope.advanceInterTransactionParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'CompanyName, PlantName',
        searchBy: 'AdvanceNo',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showAdvanceInterTransactionPopUp = function () {
        $scope.advanceInterTransactionParameters.partyId = $scope.advance.PartyId;
        baseService.setCurrentPage('advanceInterTransactionList');
        $scope.getAdvanceInterTransactionList = function (pageno) {
            baseService.paginationBase($scope.jouranlUrl, pageno, $scope.advanceInterTransactionParameters)
                .then(function (result) {
                    $scope.advanceInterTransactionList = result.Rows;
                    $scope.advanceInterTransactionParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#advanceJournalPopUp')).modal('show');
        $scope.getAdvanceInterTransactionList();
    };

    $scope.closeAdvanceInterTransactionPopUp = function (data) {
        $scope.financingTypeGL = $.grep($scope.financingTypeList, function (item) {
            return item.FinancingTypeId === data.FinancingTypeId;
        })[0];
        if (baseService.isUndefinedOrNull($scope.financingTypeGL.AssetGLId)) {
            ShowResult('Transaction Type GL not found!', 'failure', 'advanceJournalPopUp');
        }
        else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull($scope.financingTypeGL.AssetBudgetMasterId)) {
            ShowResult('Transaction Type Budget not found!', 'failure', 'advanceJournalPopUp');
        }

        $scope.advance.AdvanceId = data.AdvanceId;
        $scope.advance.AdvanceNo = data.AdvanceNo;
        $scope.advance.Amount = data.NetAmount;
        $scope.advance.Narration = data.Narration;
        $scope.advance.JournalId = data.AdvanceId;
        $scope.advance.AdvanceDetailId = data.AdvanceDetailId;

        $scope.advance.GLGeneralInfoId = $scope.financingTypeGL.AssetGLId;
        $scope.advance.GLGeneralInfoCode = $scope.financingTypeGL.AssetGLCode;
        $scope.advance.GLGeneralInfoName = $scope.financingTypeGL.AssetGLName;
        $scope.advance.BudgetMasterId = $scope.financingTypeGL.AssetBudgetMasterId;
        $scope.advance.BudgetCode = $scope.financingTypeGL.AssetBudgetCode;
        $scope.advance.BudgetName = $scope.financingTypeGL.AssetBudgetName;
        $scope.advance.ActivityId = $scope.financingTypeGL.AssetActivityId;
        $scope.advance.ActivityCode = $scope.financingTypeGL.AssetActivityCode;
        $scope.advance.ActivityName = $scope.financingTypeGL.AssetActivityName;

        $scope.setDrExchangeRate($scope.advance.GLGeneralInfoId);
        $scope.updateCrAmount();
        $scope.checkBankAmount();
        $scope.hideAdvanceInterTransactionPopUp();
    };

    $scope.hideAdvanceInterTransactionPopUp = function () {
        angular.element(document.querySelector('#advanceJournalPopUp')).modal('hide');
    };

    $scope.invalidRow = false;
    $scope.checkRowValidation = function (data, index) {
        if (manualValidation('td_Narration_' + index, baseService.isUndefinedOrNull(data.Narration), 'Narration is required.')) {
            $scope.invalidRow = true;
        }
        else if (manualValidation('td_Amount_' + index, baseService.isUndefinedOrNaNOrZero(data.Amount), 'Amount is required and must greater than 0.')) {
            $scope.invalidRow = true;
        }
        else
            $scope.invalidRow = false;
    };
}