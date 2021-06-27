"use strict";
entityExpenseBookingController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "bankService"];
function entityExpenseBookingController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, bankService) {
    $rootScope.title = "Entity Expense Booking";
    $scope.voucherDetailList = [];
    $scope.hideSource = true;
    $scope.Action = "Save";
    $scope.voucherDetailCurrencyList = [];
    $scope.url = "Banks/CashJournal";
    $scope.listUrl = $scope.url + "/GetExpenseBookingList";
    $scope.saveUrl = $scope.url + "/InsertEntityExpenseBooking";
    $scope.updateUrl = $scope.url + "/EntityExpensesBookingEdit";
    $scope.submitUrl = $scope.url + "/EntityExpensesBookingSubmit";
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });

    $scope.voucher = {
        Id: null,
        CurrencyId: null,
        DocDate: null,
        PostingDate: null,
        DocRefNo: null,
        Narration: null,
        BankName: null,
        BankMasterId: null,
        CashMasterId: null,
        BankCurrencyId: null,
        BankAmount: null,
        BankAccountNumber: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PaymentSource: "Cash",
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
        BankJournalType: "CashExpense",
        CompanyCurrencyRate: 1
    };

    $scope.voucherDetail = {
        Id: null,
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
        NetAmount: null,
        CostCenterId:null,
        CompanyCurrencyAmount: null
    };

    baseService.init($scope.listUrl, null, null, "DESC", "PostingDate DESC, DocRefNo", "DocRefNo");
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
            "name": "Doc Date",
            "value": "InvoiceDate"
        },
        {
            "name": "Doc Ref",
            "value": "InvoiceNumber"
        },
        {
            "name": "Currency",
            "value": "Currency"
        }
    ];

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getEntityByGeneralUser(function (result) {
                $scope.entityList = result;
                if ($scope.entityList.length == 1) {
                    $scope.voucher.EntityId = $scope.entityList[0].Value;
                    $scope.changeEntityForCash($scope.voucher.EntityId);
                }
            });
    });


    $scope.getCboVoucherTypeCashJournalList = function () {
        bankService.getCboVoucherTypeCashExpensesList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.GetCurrencyExchangeRateList();
            }
        });
    };
    $scope.getCboVoucherTypeCashJournalList();

    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
    });
    cboService.getCostCenterCbo(function (result) {
        $scope.costCenterList = result;
    });
    
    $scope.SelectedBudgetItem = function (id) {
        $scope.voucherDetail.BudgetName = $("#budgetid option:selected").text();
        $scope.voucherDetail.BudgetMasterId = id;
        $scope.getActivity(id);
    };

    $scope.SelectedActivityItem = function (id) {
        $scope.voucherDetail.ActivityName = $("#activityid option:selected").text();
        $scope.voucherDetail.ActivityId = id;
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
        }
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
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
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.InvoiceDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.InvoiceDate + "&currencyId=" + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.voucher.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.addRow = function (data) {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure", "GLPopUp");
            return true;
        }
        if ($scope.companyConfig.IsVoucherFromBudget)
            var getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "BudgetMasterId": data.BudgetMasterId, "ActivityId": data.ActivityId });

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

            $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.InvoiceDate);
            $scope.voucherDetail.DocRefNo = $scope.voucher.InvoiceNumber;
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
    };

    $scope.removeRow = function (index) {
        var row = $scope.voucherDetailList[index];
        $scope.voucherDetailList.splice(index, 1);
    };

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.VoucherId = null;
        $scope.voucher.Id = null;
        $scope.voucher.Active = true;
        $scope.voucher.Remarks = null;
        $scope.voucher.InvoiceNumber = null;
        $scope.voucher.CashMasterId = null;
        $scope.voucher.CurrencyId = null;
        $scope.voucherDetailList = [];
        $scope.getCboVoucherTypeCashJournalList();
    };

    $scope.save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        if ($scope.form0.$valid && !$scope.invalidDocDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: {
                        "expenseBooking": $scope.voucher,
                        "expenseBookingDetailList": $scope.voucherDetailList
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
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: {
                        "expenseBooking": $scope.voucher,
                        "expenseBookingDetailList": $scope.voucherDetailList
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
            }
            return true;
        }
    };

    $scope.submit = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        if ($scope.form0.$valid && !$scope.invalidDocDate) {
            $http({
                method: "POST",
                url: $scope.submitUrl,
                data: {
                    "expenseBooking": $scope.voucher,
                    "expenseBookingDetailList": $scope.voucherDetailList
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
        }
    };

    $scope.advanceId = null;
    $scope.confirmPost = function (advanceId) {
        $scope.advanceId = advanceId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function (id) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "id": id
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

    $scope.searchglByList = [
        {
            "name": "GL",
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

    $scope.GetBudgetTransactionDetail = function (id) {
        $http({
            method: "GET",
            url: "banks/cashjournal/GetExpensesBookingDetail?voucherId=" + id
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
        });
    };

    $scope.Get = function (data) {
        $scope.voucher = data;
        $scope.voucher.DocDate = $filter("dateFiltering")(data.DocDate);
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.PostingDate);
        $scope.changeEntityForCash(data.EntityId);
        $scope.voucher.CashMasterId = data.CashMasterId;
        $scope.voucherDetail = {};
        $scope.GetBudgetTransactionDetail(data.VoucherId);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    

    $scope.changeEntityForCash = function (entityId) {
        bankService.getCashMasterCboListByEntity(entityId, function (result) {
            $scope.cashMasterList = result;
        });
    };

    $scope.onCashChange = function (cashMasterId) {
        var cash = $.grep($scope.cashMasterList, function (item) {
            return item.Id === cashMasterId;
        })[0];
        $scope.voucher.CurrencyId = cash.CurrencyId;
    };
}