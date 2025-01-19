"use strict";
interTransactionController.$inject = ["cboService", "baseService", "factoryService", "commonMessage", "$scope", "$rootScope", "$http", "$filter", "$controller", "$routeParams", "bankService"];
function interTransactionController(cboService, baseService, factoryService, commonMessage, $scope, $rootScope, $http, $filter, $controller, $routeParams, bankService) {
    $rootScope.title = "Inter Transaction";
    $scope.Action = "Save";
    $scope.voucherDetailCurrency = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.employeePayableDataList = [];
    $scope.voucherDetailList = [];
    $scope.voucherList = [];
    $scope.index = -1;
    $scope.url = 'accounts/Advance';
    $scope.listUrl = $scope.url + '/GetInterTransactionList';
    $scope.parkUrl = $scope.url + '/ParkInterTransaction';
    $scope.updateUrl = $scope.url + '/UpdateInterTransaction';
    $scope.postUrl = $scope.url + '/PostInterTransaction';
    $scope.unPostUrl = $scope.url + '/UnPostCustomerAdvance';
    $scope.reportUrl = $scope.url + '/ReportInterTransaction?voucherId=';
    $scope.jouranlUrl = $scope.url + '/GetInterTransactionList';
    $scope.deleteUrl = $scope.url + '/DeleteInterTransaction';

    $scope.partyType = "Customer";
    $scope.partyGLType = "DownPayment";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    $scope.hideSource = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    baseService.init($scope.listUrl, null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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
            "name": "Inter Plant",
            "value": "PlantName"
        },
        {
            "name": "Party Code",
            "value": "PartyCode"
        },
        {
            "name": "Customer",
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
        JournalType: 'Receivable',
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
        IsInterTransaction: false,
        SettlementType: 'SetOff',
        PaymentSource: 'Bank',
        ExchangeType: null
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

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, '', function (result) {
                $scope.entityList = result;
            });
        if (!baseService.isUndefinedOrNull($routeParams.advanceId)) {
            getByParams($routeParams.advanceId);
        }
    });

    bankService.getBankMasterHouseBankCboList(function (result) {
        $scope.bankMasterList = result;
    });

    bankService.getCashMasterCboList(function (result) {
        $scope.cashMasterList = result;
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

    $scope.getInterTransactionDetail = function (id) {
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
    }

    $scope.getById = function (index, data) {

        $scope.advance = data;
        $scope.advance.DocDate = $filter('dateFiltering')($scope.advance.DocDate);
        $scope.advance.VoucherDate = $filter('dateFiltering')($scope.advance.VoucherDate);
        $scope.advance.PostingDate = $filter('dateFiltering')($scope.advance.PostingDate);
        $scope.advance.ReviewDate = $filter('dateFiltering')($scope.advance.ReviewDate);
        $scope.companyChange(data.CompanyId);
        $scope.advance.PlantId = data.PlantId;
        $scope.getPartyPlantList($scope.advance.PartyId, true);
        $scope.advance.PartyPlantId = data.PartyPlantId;
        $scope.getInterTransactionDetail(data.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.advance.PostingDate) && !baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.advance.PostingDate + "&currencyId=" + $scope.advance.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.advance.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = null;
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
            // $scope.GetCurrencyExchangeRateList();
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

        if ($scope.advance.PartyId === null && $scope.advance.SettlementType == "SetOff") {
            ShowResult("Please select Customer!", "failure");
            return true;
        }
        if (parseFloat($scope.advance.Amount) === 0) {
            ShowResult("Advance Amount must greater than 0!", "failure");
            return true;
        }
        if ($scope.advance.SettlementType == "Payment") {
            if (baseService.isUndefinedOrNull($scope.advance.BankMasterId) && $scope.advancePaymentSource == "Bank") {
                ShowResult("Please select  Bank!", "failure");
                return true;
            }
            if (baseService.isUndefinedOrNull($scope.advance.CashMasterId) && $scope.advancePaymentSource == "Cash") {
                ShowResult("Please select  Bank!", "failure");
                return true;
            }
        }
        return false;
    };

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            if (baseService.isUndefinedOrNull(party.DownPaymentGLId)) {
                ShowResult("Customer DownPaymentGL not found!", "failure", "partyPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.DownPaymentBudgetId)) {
                ShowResult("Customer budget not found!", "failure", "partyPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(party.CurrencyId)) {
                ShowResult('Customer transaction currency not found!', 'failure', 'partyPopUp');
                return;
            }
            else {
                $scope.advance.PartyId = party.Id;
                $scope.advance.PartyCode = party.Code;
                $scope.advance.PartyName = party.Code + " - " + party.UserName;
                $scope.advance.PartyType = party.PartyType;
                $scope.advance.CurrencyId = party.CurrencyId;
                $scope.advance.TotalPartyPlant = party.TotalPartyPlant;
                $scope.GetCurrencyExchangeRateList();
                $scope.getPartyPlantList(party.Id);
            }
        }
        $scope.hidePartyPopUp();
    };

    //$scope.updateCrAmount = function (data) {
    //    angular.forEach($scope.advanceDetailList, function (item, i) {
    //        if (item.PartyType === $scope.partyType) {
    //            item.Narration = $scope.advance.Narration;
    //            item.PartyPlantId = $scope.advance.PartyPlantId;
    //            item.PartyPlantName = item.PartyPlantName === null ? $scope.PartyPlantName : item.PartyPlantName;
    //        }
    //        if (!$scope.advance.IsInterTransaction) {
    //            item.Amount = $scope.advance.Amount;
    //        }
    //        if (data !== undefined && data !== null && item.PartyType === $scope.partyType) {
    //            item.Amount = $scope.advance.Amount - data.Amount;
    //        }
    //    });
    //};

    $scope.removeRow = function (index) {
        $scope.advanceDetailList.splice(index, 1);
        $scope.updateCrAmount(null);
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
                    }
                });
                $scope.advanceDetail = {};
            });
    };

    $scope.clear = function () {
        $scope.Action = "Save";
        $scope.getCboVoucherTypeInterTransactionList();
        $scope.advance.Active = true;
        $scope.advance.DocRefNo = null;
        $scope.advance.PaymentSource = 'Bank';
        $scope.advance.JournalType = 'Receivable';
        $scope.advance.SettlementType = 'SetOff';
        $scope.advance.ReviewDate = null;
        $scope.advance.Amount = 0;
        $scope.advance.Narration = null;
        $scope.advance.EmployeeName = null;
        $scope.advance.EmployeeId = null;
        $scope.advance.IsInterTransaction = false;
        $scope.advance.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.currencyExchangeRate = [];
        $scope.advanceDetailList = [];
        $scope.bankChargesList = [];
        $scope.debitCreditNoteList = [];
        $scope.voucherDetailList = [];
        $scope.advanceTaxesList = [];
        $scope.clearPartyPopUp();
        $scope.advance.Exchange = false;
        $scope.advance.ExchangeAmount = 0;
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

    $scope.getCboVoucherTypeInterTransactionList = function () {
        cboService.getCboVoucherTypeInterTransactionList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.advance.PostingDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
                $scope.advance.DocDate = $scope.advance.PostingDate;
            }
        });
    }
    $scope.getCboVoucherTypeInterTransactionList();

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
            manualValidation('div_TransactionType', true, 'Transaction Type is required.');
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
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
       
            $scope.entityValidation();
        angular.forEach($scope.advanceDetailList, function (item, i) {
            if ($scope.invalidRow) {
                return;
            }
            $scope.checkRowValidation(item, i);
        });
        if ($scope.form0.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidEntity && !$scope.invalidPostingDate && !$scope.invalidRow) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.parkUrl,
                    data: {
                        "advanceVM": $scope.advance,
                        "advanceDetailVMList": $scope.advanceDetailList,
                        "bankChargeDetailVMList": $scope.bankChargesList,
                        "NoteSetOffList": $scope.debitCreditNoteList,
                        "voucherDetailVMList": $scope.voucherDetailList,
                        "taxDetailVMList": $scope.advanceTaxesList
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
                        "bankChargeDetailVMList": $scope.bankChargesList
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
    $scope.post = function (advanceId) {
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
            manualValidation('div_Company', true, 'Company is required.');
            $scope.company = null;
        }
    };

    $scope.plant = null;
    $scope.getPlantInfo = function (plantId) {
        if (!baseService.isUndefinedOrNull(plantId)) {
            $scope.plant = $.grep($scope.interplantList, function (item) {
                return item.PlantId === plantId;
            })[0];
            if (manualValidation('div_Plant', baseService.isUndefinedOrNull($scope.plant.PartyPlantId), 'This Company is not created as InterCompany Party Plant.')) {
                $scope.plant = null;
            }
        }
        else {
            manualValidation('div_Plant', true, 'Plant is required.');
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
        $scope.advanceDetail.Amount = 0;

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

        $scope.updateCrAmount(null);
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

    $scope.searchglByList = [
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoName",
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.popUp = function () {
        $scope.customerInvoiceGLList = [];
        baseService.setCurrentPage("cOAICodeList");
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetInterTransactionGLBudgetActivity", pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "GLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };
    $scope.closeCOAICodeListPopUpSelected = function (x) {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    $scope.setSelected = function (data) {
        $scope.addRow(data);
    };
    $scope.voucherDetail = {};
    $scope.addRow = function (data) {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult("Please select Currency!", "failure", "GLPopUp");
            return true;
        }
        if ($scope.companyConfig.IsVoucherFromBudget)
            var getRow = $filter("filter")($scope.advanceDetailList, { "TrnType": "Dr", "BudgetMasterId": data.BudgetMasterId, "ActivityId": data.ActivityId, });

        if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].BudgetMasterId === data.BudgetMasterId) {
            ShowResult("This Activity is already added!", "failure", "GLPopUp");
        }
        else {
            $scope.voucherDetail.BudgetMasterId = data.BudgetMasterId;
            $scope.voucherDetail.BudgetCode = data.BudgetCode;
            $scope.voucherDetail.BudgetName = data.BudgetName;
            $scope.voucherDetail.ActivityId = data.ActivityId;
            $scope.voucherDetail.ActivityCode = data.ActivityCode;
            $scope.voucherDetail.ActivityName = data.ActivityName;

            //$scope.voucherDetail.Id = baseService.pk();
            $scope.voucherDetail.GLGeneralInfoId = data.GLGeneralInfoId;
            $scope.voucherDetail.GLGeneralInfoCode = data.GLGeneralInfoCode;
            $scope.voucherDetail.GLGeneralInfoName = data.GLGeneralInfoName;

            $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.advance.DocDate);
            $scope.voucherDetail.DocRefNo = $scope.advance.DocRefNo;
            $scope.voucherDetail.Narration = $scope.advance.Narration;
            $scope.voucherDetail.EntityId = $scope.advance.EntityId;
            $scope.voucherDetail.PlantId = $scope.advance.PlantId;
            $scope.voucherDetail.CrAmount = 0;
            $scope.voucherDetail.DrAmount = 0;
            $scope.voucherDetail.TrnType = "Dr";
            $scope.advanceDetailList.push($scope.voucherDetail);
            $scope.voucherDetail = {};
            $scope.closeCOAICodeListPopUp();
        }
    };

    $scope.showPartyPopUp = function () {
        baseService.setCurrentPage('partyList');
        $scope.getPartyList = function (pageno) {
            if ($scope.advance.SettlementType == 'DebitNoteSetOff' || $scope.advance.SettlementType == 'CreditNoteSetOff') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList';
            }
            else if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList?partyType=' + $scope.partyType;
            }

            baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
                .then(function (result) {
                    $scope.partyList = result.Rows;
                    $scope.partyParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#partyPopUp')).modal('show');
        $scope.getPartyList();
    };

    $scope.changeSettlementType = function () {
        $scope.advance.PartyId = null;
        $scope.advance.PartyPlantId = null;
        $scope.advance.PartyName = null;
        $scope.advanceDetailList = [];
    }
    $scope.customerInvoiceSearchList = [
        {
            "Text": "VoucherNo",
            "Value": "VoucherNo"
        },
        {
            "Text": "RefNo",
            "Value": "TransactionRefNo"
        },
        {
            "Text": "PINo",
            "Value": "SalesOrderNo"
        },
        {
            "Text": "Location",
            "Value": "PartyPlantName"
        },
        {
            "Text": "PostingDate",
            "Value": "PostingDate"
        },
        {
            "Text": "DocDate",
            "Value": "DocDate"
        },
        {
            "Text": "Currency",
            "Value": "CurrencyCode"
        }
    ];
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
                baseService.paginationBase("accounts/Invoice/GetCustomerAvailableInvoiceList", pageno, $scope.customerInvoiceParameters)
                    .then(function (response) {
                        $scope.customerreceivableList = response.Rows;
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
                var getRow = $filter("filter")($scope.advanceDetailList, { "TrnType": "Cr", "DocRefNo": data.DocRefNo });
                if (getRow.length === 0) {
                    data.Amount = data.Receivable;
                    data.WriteOff = data.Received;
                    data.Advilable = data.Balance;
                    data.CrAmount = data.Balance;
                    data.CompanyCurrencyRate = data.CompanyCurrencyRate;
                    $scope.advanceDetailList.push(data);
                    angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
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

    $scope.invoiceSearchList = [
        {
            "Text": "VoucherNo",
            "Value": "VoucherNo"
        },
        {
            "Text": "Entity",
            "Value": "EntityName"
        },
        {
            "Text": "Party Name",
            "Value": "PartyPlantName"
        },
        {
            "Text": "Posting Date",
            "Value": "PostingDate"
        },
        {
            "Text": "Doc Date",
            "Value": "DocDate"
        },
        {
            "Text": "Doc RefNo",
            "Value": "DocRefNo"
        }
    ];

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

    $scope.showVendorInvoicePopUp = function (partyId) {
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.customerreceivableList = [];
            ShowResult("Please select Vendor.", "failure");
            return;
        }
        else {
            $scope.getInvoiceData = function (pageno) {
                $scope.customerReceivableGLUrl1 = "accounts/Invoice/GetVendorAvailableInvoiceList?partyid=" + partyId;
                baseService.paginationBase($scope.customerReceivableGLUrl1, pageno, $scope.invoiceParameters)
                    .then(function (result) {
                        try {
                            $scope.invoiceList = result.Rows;
                            $scope.invoiceParameters.total_count = result.Total;
                        } catch (e) {
                            ShowResult(e, "Error");
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
            };
        }
        angular.element(document.querySelector("#vendorInvoicePopUp")).modal("show");
        $scope.getInvoiceData();
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector("#vendorInvoicePopUp")).modal("hide");
    };

    $scope.closeInvoicePopUpselected = function () {
        angular.forEach($scope.invoiceList, function (data, i) {
            if (data.Active === true) {
                data.TrnType = "Dr";
                data.PartyPlantName = data.PartyPlantName;
                var getRow = null;
                getRow = $filter("filter")($scope.advanceDetailList, { "TrnType": "Dr", "DocRefNo": data.DocRefNo });
                if (getRow.length === 0) {
                    data.Amount = data.Balance;
                    data.WriteOff = data.Received;
                    data.Advilable = data.Balance;
                    data.DrAmount = data.Balance;
                    data.CompanyCurrencyRate = data.CompanyCurrencyRate;
                    $scope.advanceDetailList.push(data);
                    angular.element(document.querySelector("#vendorInvoicePopUp")).modal("hide");
                }
                else {
                    ShowResult(data.DocRefNo + " already  Exist", "failure", "vendorInvoicePopUp");
                }
            }
        });
    };

    $scope.bankCharge = {
        FinancingTypeId: null,
        FinancingTypeName: null,
        Amount: null,
        CompanyCurrencyAmount: null
    };

    $scope.bankChargesList = [];
    $scope.addCharge = function () {
        if (manualValidation("td_FinancingType", baseService.isUndefinedOrNull($scope.bankCharge.FinancingTypeId), "Charges Type is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_FinancingTypeAmount", baseService.isUndefinedOrNull($scope.bankCharge.Amount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_FinancingTypeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.bankCharge.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.bankCharge.FinancingTypeName = $.grep($scope.bankChargeTypeList, function (item) {
                return item.FinancingTypeId === $scope.bankCharge.FinancingTypeId;
            })[0].ExpensesUserName;
            $scope.bankChargesList.push($scope.bankCharge);
            $scope.bankCharge = {};
        }
    };

    $scope.copyChargesAmount = function () {
        if ($scope.advance.CurrencyId === $scope.companyCurrencyId) {
            $scope.bankCharge.CompanyCurrencyAmount = $scope.bankCharge.Amount;
        }
        else {
            $scope.bankCharge.CompanyCurrencyAmount = ($scope.bankCharge.Amount * $scope.advance.CompanyCurrencyRate).toFixed(2);
        }
    };

    bankService.getCboBankChargeTypeList(function (result) {
        $scope.bankChargeTypeList = result;
    });

    $scope.removeChargesRow = function (index) {
        $scope.bankChargesList.splice(index, 1);
    };

    $scope.clearBankPopUp = function () {
        $scope.advance.CashMasterId = null;
        $scope.advance.BankMasterId = null;
    }
    $scope.clearCashPopUp = function () {
        $scope.advance.CashMasterId = null;
        $scope.advance.BankMasterId = null;
    }
    $scope.SetOffName = 'Customer Payment Receipt By Inter Company';
    $scope.PaymentName = 'Payment Receipt By Inter Company';
    $scope.TransferName = 'Lended To Inter Company';
    $scope.changeJournalType = function () {
        if ($scope.advance.JournalType == 'Receivable') {
            $scope.partyType = "Customer";
            $scope.SetOffName = 'Customer Payment Receipt By Inter Company';
            $scope.PaymentName = 'Payment Receipt By Inter Company';
            $scope.TransferName = 'Lended To Inter Company';

        }
        else {
            $scope.partyType = "Vendor";
            $scope.SetOffName = 'Vendor Payment By Inter Company';
            $scope.PaymentName = 'Payment To Inter Company';
            $scope.TransferName = 'Borrowed From Inter Company';

        }
    }
    $scope.delete = function (advanceId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "advanceId": advanceId, "voucherId": voucherId
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
                $scope.advanceId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.invoiceId = null;
    $scope.confirmDelete = function (advanceId, voucherId) {
        $scope.advanceId = advanceId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

    $scope.invoiceSearchList = [
        {
            "Text": "VoucherNo",
            "Value": "VoucherNo"
        },
        {
            "Text": "Entity",
            "Value": "EntityName"
        },
        {
            "Text": "Party Name",
            "Value": "PartyPlantName"
        },
        {
            "Text": "Posting Date",
            "Value": "PostingDate"
        },
        {
            "Text": "Doc Date",
            "Value": "DocDate"
        },
        {
            "Text": "Doc RefNo",
            "Value": "DocRefNo"
        }
    ];

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

    $scope.getPopupCustomerReceivableList = function () {
        $scope.getInvoiceData = function (pageno) {

            if ($scope.advance.SettlementType == 'CreditNoteSetOff') {
                $scope.customerReceivableGLUrl1 = "accounts/AdjustmentNote/GetCreditNoteAvailableList?partyId=" + $scope.advance.PartyId;
            } else {

                $scope.customerReceivableGLUrl1 = "accounts/AdjustmentNote/GetDebitNoteAvailableList?partyId=" + $scope.advance.PartyId;
            }

            baseService.paginationBase($scope.customerReceivableGLUrl1, pageno, $scope.invoiceParameters)
                .then(function (result) {
                    try {
                        $scope.customerreceivableList = result.Rows;
                        $scope.invoiceParameters.total_count = result.Total;
                    } catch (e) {
                        ShowResult(e, "Error");
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#debitCreditNotePopUp")).modal("show");
        $scope.getInvoiceData();
    };

    //$scope.closedebitCreditNotePopUpselected = function () {
    //    angular.element(document.querySelector("#debitCreditNotePopUp")).modal("hide");
    //};
    $scope.debitCreditNoteList = [];
    $scope.closedebitCreditNotePopUpselected = function () {

        if ($scope.advance.SettlementType == 'CreditNoteSetOff') {
            angular.forEach($scope.customerreceivableList, function (data, i) {
                if (data.Active === true) {
                    data.TrnType = "Cr";
                    data.PartyPlantName = data.PartyPlantName;
                    var getRow = null;
                    getRow = $filter("filter")($scope.debitCreditNoteList, { "TrnType": "Cr", "DocRefNo": data.DocRefNo, "InvoiceDetailId": data.InvoiceDetailId });
                    if (getRow.length === 0) {
                        data.Amount = data.Balance;
                        $scope.debitCreditNoteList.push(data);
                        angular.element(document.querySelector("#debitCreditNotePopUp")).modal("hide");
                    }
                    else {
                        ShowResult(data.DocRefNo + " already  Exist", "failure", "debitCreditNotePopUp");
                    }
                }
            });
        }
        else {
            angular.forEach($scope.customerreceivableList, function (data, i) {
                if (data.Active === true) {
                    data.TrnType = "Dr";
                    data.PartyPlantName = data.PartyPlantName;
                    var getRow = null;
                    getRow = $filter("filter")($scope.debitCreditNoteList, { "TrnType": "Dr", "DocRefNo": data.DocRefNo, "InvoiceDetailId": data.InvoiceDetailId });
                    if (getRow.length === 0) {
                        data.Amount = data.Balance;
                        $scope.debitCreditNoteList.push(data);
                        angular.element(document.querySelector("#debitCreditNotePopUp")).modal("hide");
                    }
                    else {
                        ShowResult(data.DocRefNo + " already  Exist", "failure", "debitCreditNotePopUp");
                    }
                }
            });
        }
    };

    $scope.CheckExchange = function () {
        if ($scope.advance.Exchange) {
            $scope.advance.ExchangeType = 'ExchangeLoss';
        } else {
            $scope.advance.ExchangeType = null;
            $scope.advance.ExchangeAmount = 0;
        }
    }

    $scope.changeExhangeType = function (type) {
        if (type === 'Loss') {
            $scope.advance.ExchangeType = 'ExchangeLoss';
        }
        if (type === 'Gain') {
            $scope.advance.ExchangeType = 'ExchangeGain';
        }
    };


    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.advance.EmployeeName = employee.EmployeeName;
            $scope.advance.EmployeeId = employee.SystemId;
            $scope.advance.EntityId = employee.EntityId;
            $scope.advance.PartyType = "Employee";
            $scope.partyType = "Employee";
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };

    $scope.employeePayableSearchList = [];
    $scope.employeePayableParameters = {
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

    $scope.getPopupEmployeePayableList = function () {
        $scope.customerreceivableGLData = function (pageno) {
            $scope.customerReceivableGLUrl1 = "accounts/EmployeePayable/GetEmployeeAvailableInvoiceList?employeeId=" + $scope.advance.EmployeeId;
            baseService.paginationBase($scope.customerReceivableGLUrl1, pageno, $scope.employeePayableParameters)
                .then(function (result) {
                    try {
                        $scope.employeePayableDataList = result.Rows;
                        $scope.employeePayableParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.employeePayableSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.employeePayableDataList, $scope.employeePayableSearchList);
                        }
                    } catch (e) {
                        ShowResult(e, "Error");
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#employeePayablePopUp")).modal("show");
        $scope.customerreceivableGLData();
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector("#employeePayablePopUp")).modal("hide");
    };

    $scope.selectEmployeePayablePopUp = function (data) {
        data.Amount = null;
        data.TrnType = "Dr";
        var getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "DocRefNo": data.DocRefNo });
        if (getRow.length === 0) {
            data.Amount = data.Balance;
            $scope.voucherDetailList.push(data);
            $scope.GetCurrencyExchangeRateList(data.CurrencyId, data);
            if ($scope.voucherDetailList.length > 0)
                $scope.isReadOnly = true;
            else
                $scope.isReadOnly = false;
            angular.element(document.querySelector("#employeePayablePopUp")).modal("hide");
        }
        else {
            ShowResult("Already Exist Payable", "failure", "employeePayablePopUp");
        }
    };

    $scope.advanceTaxesList = [];
    $scope.changePostingGetTaxCode = function () {
        $scope.advanceTaxesList = [];
        $scope.getTaxCodeByTaxYear($scope.advance.PostingDate);
    }
   
    $scope.taxCodCboList = [];
    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYear = function (date) {
        $http({
            method: "get",
            url: "accounts/TaxCode/GetAdditionalTaxCbo?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message;
                }
                else {
                    var result = response.data;
                    $scope.taxCodCboList = result;
                    if ($scope.taxCodCboList.length === 0) {
                        $scope.pop("error", "No TaxCode found in this Fiscal Year ");
                    }
                }
            },
            function errorCallback(response) {
            });
    };

    $scope.getTaxCodeByTaxYear($filter("dateFiltering")(Date.now()));
    $scope.advanceTax = {
        TaxCodeId: null,
        Text: null,
        TaxAmount: null,
        CompanyCurrencyAmount: null
    };

    $scope.advanceTaxesList = [];
    $scope.addTax = function () {
        if (manualValidation("td_TaxCode", baseService.isUndefinedOrNull($scope.advanceTax.TaxCodeId), "Tax Code is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeAmount", baseService.isUndefinedOrNull($scope.advanceTax.TaxAmount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.advanceTax.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.advanceTax.TaxName = $.grep($scope.taxCodCboList, function (item) {
                return item.Value === $scope.advanceTax.TaxCodeId;
            })[0].Text;
            $scope.advanceTaxesList.push($scope.advanceTax);
            $scope.advanceTax = {};
        }
    };

    $scope.copyTaxesAmount = function () {
        if ($scope.advance.CurrencyId === $scope.companyCurrencyId) {
            $scope.advanceTax.CompanyCurrencyAmount = $scope.advanceTax.TaxAmount;
        }
        else {
            $scope.advanceTax.CompanyCurrencyAmount = ($scope.advanceTax.TaxAmount * $scope.advance.CompanyCurrencyRate).toFixed(2);
        }
    };

    $scope.removeTaxesRow = function (index) {
        $scope.advanceTaxesList.splice(index, 1);
    };

    cboService.getCboEmployeeTransactionType(function (result) {
        $scope.employeeTransactionTypeList = result;
        if ($scope.employeeTransactionTypeList.length === 1) {
            $scope.advance.EmployeeTransactionTypeId = $scope.employeeTransactionTypeList[0].Value;
        }
    });
    $scope.transactionTypeGL = null;
    $scope.getTransactionTypeGL = function (id) {
        $scope.advanceDetailList = [];
        if ($scope.advance.CurrencyId === null) {
            ShowResult('Please Select Currency!', 'failure');
            return;
        }
        if (!baseService.isUndefinedOrNull(id)) {
            $scope.transactionTypeGL = $.grep($scope.employeeTransactionTypeList, function (item) {
                return item.EmployeeTransactionTypeId === id;
            })[0];
            if (manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.transactionTypeGL.AdvanceGLId), 'Transaction Type GL not found!')) {
                $scope.transactionTypeGL = null;
                $scope.advanceDetailList = [];
            }
            else {
                $scope.advanceDetail.GLGeneralInfoId = $scope.transactionTypeGL.AdvanceGLId;
                $scope.advanceDetail.GLGeneralInfoCode = $scope.transactionTypeGL.AdvanceGLCode;
                $scope.advanceDetail.GLGeneralInfoName = $scope.transactionTypeGL.AdvanceGLName;
                $scope.advanceDetail.BudgetMasterId = $scope.transactionTypeGL.AdvanceBudgetMasterId;
                $scope.advanceDetail.BudgetCode = $scope.transactionTypeGL.AdvanceBudgetCode;
                $scope.advanceDetail.BudgetName = $scope.transactionTypeGL.AdvanceBudgetName;
                $scope.advanceDetail.ActivityId = $scope.transactionTypeGL.AdvanceActivityId;
                $scope.advanceDetail.ActivityCode = $scope.transactionTypeGL.AdvanceActivityCode;
                $scope.advanceDetail.ActivityName = $scope.transactionTypeGL.AdvanceActivityName;
                $scope.advanceDetail.EmployeeTransactionTypeId = $scope.transactionTypeGL.EmployeeTransactionTypeId;

                $scope.advanceDetail.Narration = $scope.advance.Narration;
                $scope.advanceDetail.EmployeeId = $scope.advance.EmployeeId;
                $scope.advanceDetail.Amount = $scope.advance.Amount;

                $scope.advanceDetailList.push($scope.advanceDetail);
                $scope.advanceDetail = {};
            }
        }
        else {
            manualValidation('div_TransactionType', false, '');
            $scope.transactionTypeGL = null;
        }
    };

}