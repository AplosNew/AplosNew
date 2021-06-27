"use strict";
securityDepositController.$inject = ["bankService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function securityDepositController(bankService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Security Deposit";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.hideSource = true;
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrency = [];
    $scope.voucherList = [];
    $scope.isBankAmount = false;
    $scope.sourceType = "Security";
    $scope.partyType = "Party";
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });

    $scope.voucher = {
        Id: null,
        EntityId: null,
        FinancingTypeId: null,
        VoucherTypeId: null,
        CurrencyId: null,
        PartyId: null,
        PartyName: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: $filter("dateFiltering")(Date.now()),
        DocDate: $filter("dateFiltering")(Date.now()),
        DocRefNo: null,
        BankMasterId: null,
        CashMasterId: null,
        BankName: null,
        Amount: "",
        BankAmount: "",
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        PaymentTermId: null,
        Narration: null,
        Remarks: null,
        PaymentSource: "Bank",
        SourceType: "Given",
        PartyType: "Customer",
        TransactionType: "SecurityGiven",
        CompanyCurrencyRate: 1
    };

    $scope.voucherDetail = {};
    $scope.voucherDetailList = [];

    bankService.getBankMasterHouseBankCboList(function (result) {
        $scope.bankMasterList = result;
    });

    bankService.getCashMasterCboList(function (result) {
        $scope.cashMasterList = result;
    });

    $scope.changeSourceFrom = function (to) {
        $scope.voucher.CrGLId = null;
        $scope.voucher.CrGLName = null;
        $scope.voucher.CrBudgetId = null;
        $scope.voucher.CrActivityId = null;
        $scope.voucher.BankName = null;
        $scope.voucher.BankMasterId = null;
        $scope.voucher.CashMasterId = null;
        $scope.voucher.CashName = null;
    };

    baseService.init("accounts/SecurityDeposit/GetSecurityDepositList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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

    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
    });

    cboService.getCboOtherFinancingType($scope.sourceType, function (result) {
        $scope.financingTypeList = result;
    });

    $scope.getCboVoucherTypeSecurityTakenList = function () {
        cboService.getCboVoucherTypeSecurityTakenList(function (result) {
            $scope.voucherTypeList = result;
            if (baseService.arrayLength($scope.voucherTypeList) === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
            }
        });
    };
    $scope.getCboVoucherTypeSecurityTakenList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
    });
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });
    $scope.advanceCA = null;
    $scope.getTransactionTypeGL = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $scope.advanceCA = $.grep($scope.financingTypeList, function (item) {
                return item.FinancingTypeId === id;
            })[0];
        }
        else {
            manualValidation("div_TransactionType", false, "");
            $scope.advanceCA = null;
        }
    };

    $scope.changeSourceTo = function (to) {
        $scope.voucher.DrBankMasterId = null;
        $scope.voucher.PartyName = null;
        $scope.voucher.DirectorName = null;
        $scope.voucher.PartyType = to;
    };

    $scope.customerParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "UserName",
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.customerList = [];
    $scope.getCustomerGL = function () {
        $scope.glUrl = "Parties/party/GetCompanyPartyDataList?partyType=" + $scope.voucher.PartyType;
        $scope.getCustomerData = function (pageno) {
            baseService.paginationBase($scope.glUrl, pageno, $scope.customerParameters)
                .then(function (result) {
                    $scope.customerList = result.Rows;
                    $scope.customerParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#customerListPopUp")).modal("show");
        $scope.getCustomerData();
    };

    $scope.selectCustomerPopUp = function (index, id) {
        $scope.customerIndex = index;
        $scope.selectedCustomer = id;
    };

    $scope.getPartyPlantList = function (partyId, isUpdateMode) {
        $scope.partyPlantList = [];
        $http.get("Parties/party/GetPartyPlantCbo?partyId=" + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault && !isUpdateMode) {
                        $scope.voucher.PartyPlantId = item.Value;
                    }
                });
            });
    };

    $scope.customerIndex = -1;
    $scope.closeCustomerPopUp = function () {
        if ($scope.customerIndex !== -1) {
            var party = $scope.customerList[$scope.customerIndex];
            $scope.voucher.PartyName = party.Code + " - " + party.UserName;
            $scope.voucher.PartyId = party.Id;
            $scope.getPartyPlantList(party.Id);
            $scope.voucher.CurrencyId = party.CurrencyId;
            $scope.GetCurrencyExchangeRateList();
        }
        angular.element(document.querySelector("#customerListPopUp")).modal("hide");
        $scope.customerIndex = -1;
        $scope.selectedCustomer = null;
    };

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.PostingDate + "&currencyId=" + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.voucher.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.updateCrAmount = function () {
        angular.forEach($scope.voucherDetailList, function (item, i) {
            if (item.PartyType === $scope.partyType) {
                item.Narration = $scope.voucher.Narration;
            }
            if (!$scope.advance.IsInterTransaction) {
                item.Amount = $scope.voucher.Amount;
            }
        });
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
                            $scope.voucher.PostingDate = "";
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else if (result.IsExchangeRateConfirmed === false) {
                            ShowResult(commonMessage.FiscalPeriodExchangeRateConfirmed, "failure");
                            $scope.voucher.PostingDate = "";
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

    $scope.getFiscalYearPeriod($scope.voucher.PostingDate);
    $scope.checkBankAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.BankCurrencyId)) {
            if ($scope.voucher.BankCurrencyId !== $scope.voucher.CurrencyId) {
                if ($scope.voucher.BankCurrencyId !== $scope.companyCurrencyId) {
                    if ($scope.voucher.BankCurrencyId !== $scope.companyGroupCurrencyId) {
                        if ($scope.voucher.BankCurrencyId !== $scope.hardCurrencyId) {
                            $scope.isBankAmount = true;
                            $scope.voucher.BankAmount = 0;
                        }
                    }
                    else {
                        $scope.isBankAmount = false;
                        $scope.voucher.BankAmount = 0;
                    }
                }
                else {
                    $scope.isBankAmount = false;
                    $scope.voucher.BankAmount = 0;
                }
            }
            else {
                $scope.isBankAmount = false;
                $scope.voucher.BankAmount = 0;
            }
        }
        else {
            $scope.isBankAmount = false;
            $scope.voucher.BankAmount = 0;
        }
    };

    $scope.searchByList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
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

    $scope.getById = function (index) {
        $scope.index = index;
        $scope.voucher = Object.assign({}, $scope.voucherList[$scope.index]);
        $scope.voucher.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        $scope.voucher.VoucherDate = $filter("dateFiltering")($scope.voucher.VoucherDate);
        $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucher.PostingDate);
        $scope.GetCurrencyExchangeRateList();
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.voucher.FiscalYearId = null;
            $scope.voucher.FiscalYearName = null;
            $scope.voucher.FiscalYearPeriodId = null;
            $scope.voucher.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            msg = "Posting date must be below or equal to Doc Date!";
            $scope.voucher.FiscalYearId = null;
            $scope.voucher.FiscalYearName = null;
            $scope.voucher.FiscalYearPeriodId = null;
            $scope.voucher.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        } else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    var invalidEntity = false;
    $scope.entityValidation = function () {
        invalidEntity = baseService.isUndefinedOrNull($scope.voucher.EntityId);
        return manualValidation("div_entity", invalidEntity, "Entity is required.");
    };

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return true;
        }
        if ($scope.voucher.PartyType === "Customer") {
            if ($scope.voucher.PartyId === null) {
                ShowResult("Please select Customer!", "failure");
                return true;
            }
        }
        if ($scope.voucher.PartyType === "Vendor") {
            if ($scope.voucher.PartyId === null) {
                ShowResult("Please select Vendor!", "failure");
                return true;
            }
        }
        if (parseFloat($scope.voucher.Amount) === 0) {
            ShowResult("Advance Amount must greater than 0!", "failure");
            return true;
        }
        if ($scope.voucher.PaymentSource!='GL' && baseService.isUndefinedOrNull($scope.voucher.BankMasterId || $scope.voucher.CashMasterId)) {
            ShowResult("Please select Cash or Bank!", "failure");
            return true;
        }
        return false;
    };

    $scope.changeTransactionType = function (type) {
        $scope.voucher.TransactionType = type;
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
            $scope.entityValidation();
        if ($scope.form0.$valid && !$scope.validation() && !$scope.invalidDocDate && !invalidEntity && !$scope.invalidPostingDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "accounts/SecurityDeposit/InsertSecurityDeposit",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.voucherDetailList,
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: "accounts/SecurityDeposit/UpdateSecurityDeposit",
                    data: {
                        "voucherVM": $scope.voucher,
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
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            return true;
        }
    };

    $scope.securityDepositId = null;
    $scope.confirmPost = function (securityDepositId) {
        $scope.securityDepositId = securityDepositId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function (securityDepositId) {
        $http({
            method: "POST",
            url: "accounts/SecurityDeposit/PostSecurityDeposit",
            data: {
                "securityDepositId": securityDepositId
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

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.voucher = {};
        $scope.voucher.Active = true;
        $scope.voucher.Amount = "";
        $scope.voucher.PaymentSource = "Bank";
        $scope.voucher.SourceType = "Given";
        $scope.voucher.PartyType = "Customer";
        $scope.voucher.TransactionType = "SecurityGiven";
        $scope.voucher.CompanyCurrencyRate= 1;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.getCboVoucherTypeSecurityTakenList();
        $scope.currencyExchangeRate = [];
        $scope.partyPlantList = [];
        $scope.getPostingFiscalYearPeriod($scope.voucher.PostingDate);
    }

    $scope.searchglByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
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
        sort: "GLGeneralInfoCode",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };


    // $scope.cOAICodeList = [];
    $scope.GetCOAICodeList = function () {
        $scope.GLUrl1 = "Accounts/glitem/GetNonReconAssetLiabilityGLBudgetActivityList";
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetCOAICodeListData();
    };
    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    $scope.setSelected = function (data) {
        $scope.addRow(data);
    };
    $scope.addRow = function (data) {
        //if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
        //    ShowResult("Please select Currency!", "failure", "GLPopUp");
        //    return true;
        //}
        if ($scope.companyConfig.IsVoucherFromBudget)
            var getRow = $filter("filter")($scope.voucherDetailList, { "BudgetMasterId": data.BudgetMasterId, "ActivityId": data.ActivityId });

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

            $scope.voucherDetail.GLGeneralInfoId = data.GLGeneralInfoId;
            $scope.voucherDetail.GLGeneralInfoCode = data.GLGeneralInfoCode;
            $scope.voucherDetail.GLGeneralInfoName = data.GLGeneralInfoName;

            $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
            $scope.voucherDetail.Amount = null;
            $scope.voucherDetail.Id = null;
            $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            $scope.voucherDetail = {};
            $scope.closeCOAICodeListPopUp();
        }
    };

    $scope.removeRow = function (index) {
        $scope.voucherDetailList.splice(index, 1);
    };
}
