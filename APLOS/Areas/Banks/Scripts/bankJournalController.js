"use strict";
bankJournalController.$inject = ["bankService", "accountService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function bankJournalController(bankService, accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Bank Journal";
    $scope.Action = "Save";
    $scope.voucherList = [];
    $scope.index = -1;
    $scope.url = "Banks/BankJournal";
    $scope.listUrl = $scope.url + "/GetBankJournalList";
    $scope.parkUrl = $scope.url + "/InsertBankJournal";
    $scope.updateUrl = $scope.url + "/UpdateBankJournal";
    $scope.postUrl = $scope.url + "/PostBankJournal";
    $scope.deleteUrl = $scope.url + "/DeleteBankJournal";
    $scope.voucherDetailList = [];

    $scope.hideSource = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller("bankBaseController", { $scope: $scope, $http: $http });

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
        },
        {
            "name": "Status",
            "value": "Status"
        }
    ];

    $scope.voucher = {
        Id: null,
        CurrencyId: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        Amount: null,
        Narration: null,
        BankName: null,
        BankMasterId: null,
        BankCurrencyId: null,
        BankAmount: null,
        BankAccountNumber: null,
        PaymentSource: "Bank",
        GLGeneralInfoId: null,
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetCode: null,
        BudgetName: null,
        ActivityId: null,
        ActivityCode: null,
        ActivityName: null,
        FinancingTypeId: null,
        BankJournalType: "GL",
        BankJournalId: null,
        IsReverse: false,
        CompanyCurrencyRate: 1
    };

    $scope.advanceDetailList = [];
    $scope.voucherDetail = {
        Id: null,
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
        Amount: null,
        NetAmount: null
    };

    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });

    $scope.approvedByList = [];
    $scope.getCboApprovedByList = function () {
        cboService.getAuthorizationConfigCbo('JournalApproveBy', function (result) {
            $scope.approvedByList = result;
            if ($scope.approvedByList.length == 1) {
                $scope.voucher.ApprovedById = $scope.approvedByList[0].Id;
            }
        });
    };
    $scope.getCboApprovedByList();

    $scope.changeEntityForBankCash = function (entityId) {
        bankService.getBankMasterHouseBankCboListByEntity(entityId, function (result) {
            $scope.bankMasterList = result;
        });

        bankService.getCashMasterCboListByEntity(entityId, function (result) {
            $scope.cashMasterList = result;
        });
    };

    $scope.getBankChargesList = function (id) {
        $http({
            method: "GET",
            url: "Banks/BankJournal/GetBankJournalDetailList/" + id
        }).then(function successCallback(response) {
            $scope.advanceChargesList = response.data;
        });
    };

    $scope.getCashExpenses = function (id) {
        $http({
            method: "GET",
            url: "Banks/CashJournal/GetBankJournalDetailList/" + id
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
        });
    };

    $scope.Get = function (data) {
        $scope.changeEntityForBankCash(data.EntityId);
        $scope.voucher = data;
        $scope.voucher.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        $scope.voucher.VoucherDate = $filter("dateFiltering")($scope.voucher.VoucherDate);
        $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucher.PostingDate);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.Action = "Update";

        $scope.getBankChargesList($scope.voucher.Id);
        $scope.getCashExpenses($scope.voucher.Id);
    };

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.PostingDate + "&currencyId=" + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.voucher.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
                $scope.rateChangeBankCharge($scope.voucher.CompanyCurrencyRate);
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };

    $scope.copyAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.BankCurrencyId)) {
            if ($scope.voucher.BankCurrencyId !== $scope.voucher.CurrencyId) {
                $scope.voucher.BankAmount = $scope.voucher.Amount * $scope.voucher.CompanyCurrencyRate;
            }
            else {
                $scope.voucher.BankAmount = $scope.voucher.Amount;
            }
        }
    };

    $scope.checkBankAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.BankCurrencyId)) {
            if ($scope.voucher.BankCurrencyId !== $scope.voucher.CurrencyId) {
                $scope.isBankAmount = true;
                $scope.voucher.BankAmount = null;
            }
            else {
                $scope.isBankAmount = false;
                $scope.voucher.BankAmount = null;
            }
        }
        else {
            $scope.isBankAmount = false;
            $scope.voucher.BankAmount = null;
        }
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (baseService.isUndefinedOrNull($scope.voucher.DocDate)) {
            msg = "Doc Date is required.";
            $scope.invalidDocDate = true;
        }
        else {
            $scope.invalidDocDate = false;
            $scope.GetCurrencyExchangeRateList();
        }
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.checkDocDateValidation = function () {
        var msg = "";
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (baseService.isUndefinedOrNull($scope.voucher.DocDate)) {
            msg = "Doc Date is required.";
            $scope.invalidDocDate = true;
        }
        else {
            $scope.invalidDocDate = false;
        }
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.currencyExchangeRate = null;
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            msg = "Posting date must be below or equal to Doc Date!";
            $scope.currencyExchangeRate = null;
            $scope.invalidPostingDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.voucher.PostingDate)) {
            msg = "Posting Date is required.";
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
            $scope.GetCurrencyExchangeRateList();
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.checkPostingDateValidation = function () {
        var msg = "";
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.currencyExchangeRate = null;
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            msg = "Posting date must be below or equal to Doc Date!";
            $scope.currencyExchangeRate = null;
            $scope.invalidPostingDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.voucher.PostingDate)) {
            msg = "Posting Date is required.";
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.invalidEntity = false;
    $scope.entityValidation = function () {
        $scope.invalidEntity = baseService.isUndefinedOrNull($scope.voucher.EntityId);
        return manualValidation("div_entity", $scope.invalidEntity, "Entity is required.");
    };

    //$scope.removeRow = function (index) {
    //    $scope.advanceDetailList.splice(index, 1);
    //};

    $scope.removeDetaillRow = function (Id, voucherId, voucherDetailId, index) {
        if (Id === null) {
            //$(this).remove();
            $scope.voucherDetailList.splice(index, 1);
            return false;
        }
        else {
            $scope.message = 'Are you sure want to permanently delete this?';
            angular.element(document.querySelector('#removePopUp')).modal('show');
            $scope.vdId = voucherDetailId;
            $scope.voucherId = voucherId;
            $scope.cashJournalDetailId = Id;
            $scope.mateIndex = index;
        }
    };

    $scope.detailDelete = function () {
        try {
            $http({
                method: 'POST',
                url: 'Banks/CashJournal/DeleteCashJournalDetail?Id=' + $scope.vdId + '&voucherId=' + $scope.voucherId + '&cashJournalDetailId=' + $scope.cashJournalDetailId,
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.vdId = null;
                    $scope.voucherId = null;
                    $scope.cashJournalDetailId = null;
                    $scope.voucherDetailList.splice($scope.mateIndex, 1);
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };


    $scope.setBankOb = function (id) {
        var bank;
        bank = $filter("filter")($scope.bankMasterList, { Id: id })[0];
        $scope.voucher.CurrencyId = bank.CurrencyId;
        $scope.voucher.EntityId = bank.EntityId;
        $scope.voucher.BankCurrencyId = bank.CurrencyId;

        $scope.checkBankAmount();
        $scope.copyAmount();
        $scope.GetCurrencyExchangeRateList();
    };

    $scope.clear = function () {
        $scope.Action = "Save";
        $scope.voucher.Active = true;
        $scope.actionIsDisable = false;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.PaymentSource = "Bank";
        $scope.voucher.BankJournalType = "GL";
        $scope.voucher.IsReverse = false;
        $scope.voucher.Amount = null;
        $scope.voucher.VoucherId = null;
        $scope.voucher.Narration = null;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.CurrencyId = null;
        $scope.voucher.OtherCashMasterId = null;
        $scope.voucher.CompanyCurrencyRate = 1;
        $scope.getCboVoucherTypeBankJournalList();
        $scope.advanceDetailList = [];
        $scope.advanceChargesList = [];
        $scope.voucherDetailList = [];
    };

    $scope.getCboVoucherTypeBankJournalList = function () {
        accountService.getCboVoucherTypeBankJournalList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.GetCurrencyExchangeRateList();
            }
        });
    };
    $scope.getCboVoucherTypeBankJournalList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
    };
    $scope.validation = function () {
        if ($scope.approvedByList.length > 0 && $scope.voucher.ApprovedById == null) {
            ShowResult("Please select Approved By!", "failure");
            return true;
        }
        return false;
    }

    $scope.actionIsDisable = false;
    $scope.save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDateValidation();
        $scope.checkPostingDateValidation();
        if (!baseService.isUndefinedOrNull($scope.advanceCharge.FinancingTypeId)) {
            ShowResult("Please add Charges!", "failure");
            return false;
        }
        if ($scope.voucher.BankJournalType == 'BankReverse') {
            $scope.voucher.IsReverse = true;
        }
        if ($scope.voucherDetailList.length > 0) {
            for (var i = 0; i < $scope.voucherDetailList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.voucherDetailList[i].CompanyCurrencyAmount)) {
                    ShowResult($scope.companyCurrencyCode + " is required.", "failure");
                    return false;
                }
                else if (parseFloat($scope.voucherDetailList[i].CompanyCurrencyAmount) === 0) {
                    ShowResult($scope.companyCurrencyCode + " is required.", "failure");
                    return false;
                }
            }
        }

        if ($scope.form0.$valid && !$scope.invalidDocDate && !$scope.invalidPostingDate && !$scope.invalidRow && !$scope.validation()) {
            $scope.actionIsDisable = true;
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.parkUrl,
                    data: {
                        "voucherVM": $scope.voucher,
                        "bankChargeDetailVMList": $scope.advanceChargesList,
                        "voucherDetailVMList": $scope.voucherDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                        $scope.actionIsDisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.actionIsDisable = false;
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
                        "voucherVM": $scope.voucher,
                        "bankChargeDetailVMList": $scope.advanceChargesList,
                        "voucherDetailVMList": $scope.voucherDetailList
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

    $scope.advanceId = null;
    $scope.voucher_Post = {};
    $scope.confirmPost = function (advanceId, data) {
        if (data.ApprovedByStatus == 'ToBeApproved' || data.ApprovedByStatus == 'Hold' || data.ApprovedByStatus == 'Reject') {
            ShowResult("Before Post, Please Approve First. Mr." + data.ApprovedBy + " is responsible for Approve", "failure");
        }
        $scope.advanceId = advanceId;
        $scope.ApprovedByStatus = data.ApprovedByStatus;
        $scope.voucher_Post = {};
        $scope.voucher_Post = data;
        $scope.voucher_Post.PostingDate = $filter("dateFiltering")(data.PostingDate);
        $scope.voucher_Post.DocDate = $filter("dateFiltering")(data.DocDate);
        angular.element(document.querySelector('#PostPopUp')).modal('show');
        //$scope.message_confirmation = "Are you sure to Post?";
        //angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.closePostPopUp = function () {
        angular.element(document.querySelector("#PostPopUp")).modal("hide");
    };
    function containsSpecialChars(str) {
        const specialChars = /[@!#$%^&*()_+\=\[\]{};':"|,.<>\?`~]/;
        return specialChars.test(str);
    }
    $scope.CheckSpecialCharecter_Edit = function () {
        try {
            if (containsSpecialChars($scope.voucher_Post.DocRefNo)) {
                $scope.voucher_Post.DocRefNo = $scope.voucher_Post.DocRefNo.substring(0, $scope.voucher_Post.DocRefNo.length - 1);
                throw "No special characters allowed for Doc Ref No.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.checkDocDate_Edit = function () {
        var msg = "";
        if (new Date($scope.voucher_Post.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (new Date($scope.voucher_Post.PostingDate) < new Date($scope.voucher_Post.DocDate)) {
            msg = "Doc date must be below or equal to Posting Date!";
            $scope.invalidDocDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.voucher_Post.DocDate)) {
            msg = "Doc Date is required.";
            $scope.invalidDocDate = true;
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate_Edit", $scope.invalidDocDate, msg);
    };

    $scope.checkPostingDate_Edit = function () {
        var msg = "";
        if (new Date($scope.voucher_Post.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.invalidPostingDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.voucher_Post.PostingDate)) {
            msg = "Posting Date is required.";
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate_Edit", $scope.invalidPostingDate, msg);
    };

    $scope.post = function () {
        if ($scope.ApprovedByStatus == 'ToBeApproved' || $scope.ApprovedByStatus == 'Hold' || $scope.ApprovedByStatus == 'Reject') {
            ShowResult("Before Post, Please Approve First!!", "failure");
        } else if ($scope.voucher_Post.EntityId == null || $scope.voucher_Post.EntityId == "" || $scope.voucher_Post.EntityId == undefined) {
            ShowResult("Please select Entity First!!", "failure");
        }
        else {
            $http({
                method: "POST",
                url: $scope.postUrl,
                data: {
                    "id": $scope.advanceId,
                    "voucherVM": $scope.voucher_Post
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
                    $scope.closePostPopUp();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }

        return true;
    };

    $scope.invalidRow = false;
    $scope.checkRowValidation = function (data, index) {
        if (manualValidation("td_Narration_" + index, baseService.isUndefinedOrNull(data.Narration), "Narration is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_Amount_" + index, baseService.isUndefinedOrNaNOrZero(data.Amount), "Amount is required and must greater than 0.")) {
            $scope.invalidRow = true;
        }
        else
            $scope.invalidRow = false;
    };

    cboService.getEnumCbo("enum/GetCboPaymentType", function (result) {
        $scope.paymentTypeList = result;
    });

    $scope.advanceCharge = {
        FinancingTypeId: null,
        FinancingTypeName: null,
        Amount: null,
        CompanyCurrencyAmount: null
    };

    $scope.advanceChargesList = [];
    $scope.addCharge = function () {
        if (manualValidation("td_FinancingType", baseService.isUndefinedOrNull($scope.advanceCharge.FinancingTypeId), "Charges Type is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_FinancingTypeAmount", baseService.isUndefinedOrNull($scope.advanceCharge.Amount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_FinancingTypeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.advanceCharge.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.advanceCharge.FinancingTypeName = $.grep($scope.bankChargeTypeList, function (item) {
                return item.FinancingTypeId === $scope.advanceCharge.FinancingTypeId;
            })[0].ExpensesUserName;
            $scope.advanceChargesList.push($scope.advanceCharge);
            $scope.advanceCharge = {};
            $scope.copyAmount();
        }
    };

    $scope.copyChargesAmount = function () {
        if ($scope.voucher.CurrencyId === $scope.companyCurrencyId) {
            $scope.advanceCharge.CompanyCurrencyAmount = $scope.advanceCharge.Amount;
        }
        else {
            $scope.advanceCharge.CompanyCurrencyAmount = ($scope.advanceCharge.Amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
        }
    };

    $scope.calvdCompanyCurrencyAmount = function (data) {
        if ($scope.voucher.CurrencyId === $scope.companyCurrencyId) {
            data.CompanyCurrencyAmount = data.Amount;
        }
        else {
            data.CompanyCurrencyAmount = (data.Amount * $scope.voucher.CompanyCurrencyRate).toFixed(2);
        }
    };

    bankService.getCboBankChargeTypeList(function (result) {
        $scope.bankChargeTypeList = result;
    });

    $scope.removeChargesRow = function (index) {
        $scope.advanceChargesList.splice(index, 1);
    };

    $scope.rateChangeBankCharge = function (rate) {
        $scope.advanceCharge.CompanyCurrencyAmount = ($scope.advanceCharge.Amount * rate).toFixed(2);
        if ($scope.advanceChargesList.length !== null) {
            for (var i = 0; i < $scope.advanceChargesList.length; i++) {
                $scope.advanceChargesList[i].CompanyCurrencyAmount = ($scope.advanceChargesList[i].Amount * rate).toFixed(2);
            }
        }
    };

    $scope.financingTypeList = [];
    $scope.sourceType = "Investment";
    cboService.getCboOtherFinancingType($scope.sourceType, function (result) {
        $scope.financingTypeList = result;
    });

    $scope.searchglByList = [
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
        sort: "ActivityName",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.popUp = function () {
        $scope.customerInvoiceGLList = [];
        baseService.setCurrentPage("cOAICodeList");
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetAllGLBudgetActivityPostingAutomaticOnly", pageno, $scope.glListParameters)
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

    $scope.addRow = function (data) {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure", "GLPopUp");
            return true;
        }
        var getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "BudgetMasterId": data.BudgetMasterId, "ActivityId": data.ActivityId, });
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
            $scope.voucherDetail.CrAmount = 0;
            $scope.voucherDetail.DrAmount = 0;
            $scope.voucherDetail.TrnType = "Dr";
            $scope.voucherDetailList.push($scope.voucherDetail);
            $scope.voucherDetail = {};
            $scope.closeCOAICodeListPopUp();
        }
    };

    $scope.removeDrRow = function () {
        var dr = $scope.voucherDetailList.length;
        while (dr--) {
            if ($scope.voucherDetailList[dr]["TrnType"] === "Dr") {
                $scope.voucherDetailList.splice(dr, 1);
            }
        }
        var drc = $scope.voucherDetailCurrencyList.length;
        while (drc--) {
            if ($scope.voucherDetailCurrencyList[drc]["TrnType"] === "Dr") {
                $scope.voucherDetailCurrencyList.splice(drc, 1);
            }
        }
    };

    $scope.removeRow = function (index) {
        var row = $scope.voucherDetailList[index];
        $scope.voucherDetailList.splice(index, 1);
    };

    $scope.onBankChange = function (bankMasterId) {
        var bank = $.grep($scope.bankMasterList, function (item) {
            return item.Id === bankMasterId;
        })[0];
        $scope.voucher.CurrencyId = bank.CurrencyId;
        $scope.GetCurrencyExchangeRateList();
    };

    $scope.clearBankPopUp = function () {
        $scope.voucher.OtherBankMasterId = null;
        $scope.voucher.OtherCashMasterId = null;
        $scope.voucher.FinancingTypeId = null;
        $scope.voucherDetailList = [];
    }

    $scope.delete = function (bankJournalId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "bankJournalId": bankJournalId, "voucherId": voucherId
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
                $scope.bankJournalId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.invoiceId = null;
    $scope.confirmDelete = function (bankJournalId, voucherId) {
        $scope.bankJournalId = bankJournalId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

}