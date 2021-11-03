"use strict";
entityExpenseBookingApprovalController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "bankService"];
function entityExpenseBookingApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, bankService) {
    $rootScope.title = "Expense Booking Approval";
    $scope.Action = "Post";
    $scope.CAction = "Add";
    $scope.isPartyListing = false;
    $scope.index = -1;
    $scope.budgetTransactionMasters = [];
    $scope.CurrencyList = [];
    $scope.path = "banks/cashjournal/";
    $scope.getListUrl = $scope.path + "GetEntityExpenseBookingSubmittedList";
    $scope.saveUrl = $scope.path + "PostEntityExpenseBooking";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    baseService.init($scope.getListUrl, null, null, "DESC", "InvoiceDate DESC, InvoiceNumber", "InvoiceNumber");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.budgetTransactionMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchByList =
        [{
            name: "Transaction Id#",
            value: "InvoiceNumber"
        },
        {
            "name": "Employee Code",
            "value": "EmployeeCode"
        },
        {
            "name": "Employee Name",
            "value": "EmployeeName"
        },
        {
            "name": "Approver Code",
            "value": "ApproverCode"
        }
            ,
        {
            "name": "Approver By",
            "value": "ApprovedBy"
        }];

    $scope.budgetList = [];
    cboService.getCboEmployeeBudgetList("", function (result) {
        $scope.budgetList = result;
    });

    $scope.activityList = [];
    $scope.getCboEmployeeBudgetActivityList = function (budgetId) {
        cboService.getCboEmployeeBudgetActivityList(" ", budgetId, function (result) {
            $scope.activityList = result;
        });
    };
    $scope.partyList = [];
    $scope.voucher = {
        Id: null,
        EmployeeId: null,
        InvoiceNumber: null,
        InvoiceDate: new Date(),
        Active: true
    };

    $scope.budgetTransactionDetail = {
        Id: null,
        BudgetTransactionMasterId: null,
        BudgetId: null,
        PartyId: null,
        ActivityId: null,
        ActivityPhoneId: null,
        Amount: 0.00,
        ExpenseDate: new Date()
    };

    $scope.GetVoucherDetailrow = function (data, index) {
        $scope.indexdetails = index;
        data.ExpenseDate = $filter("dateFiltering")(data.ExpenseDate);
        $scope.budgetTransactionDetail = data;
        $scope.getCboBudgetByEmployeeActivity($scope.budgetTransactionDetail.ActivityId);
        $scope.getCboActivityPhoneByEmployeeActivity($scope.budgetTransactionDetail.ActivityId);
        $scope.CAction = "Update";
    };

    $scope.valuePassInDelModal = function (x, index) {
        $scope.id = x.Id;
        $scope.dindex = index;
        $scope.message_confirmation = "Are you sure want to delete this data....";
        angular.element(document.querySelector("#confirmgenericPopUp")).modal("show");
    };

    $scope.removeRow = function () {
        $scope.expensesBookingDetailList.splice($scope.dindex, 1);
    };

    $scope.getCboVoucherTypeCashJournalList = function () {
        bankService.getCboVoucherTypeCashJournalList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
            }
        });
    };

    $scope.getCboVoucherTypeCashJournalList();

    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });

    $scope.getCash = function (entityId) {
        bankService.getCashMasterCboListByEntity(entityId, function (result) {
            $scope.cashMasterList = result;
        });
    };

    $scope.GetExpensesBookingById = function (id) {
        $http({
            method: "GET",
            url: "banks/cashjournal/GetEntityExpenseBookingSubmittedData?Id=" + id
        }).then(function successCallback(response) {
            $scope.expensesBookingDetailList = response.data.Rows;
            for (var i = 0; i < $scope.expensesBookingDetailList.length; i++) {
                $scope.expensesBookingDetailList[i].TrnType = "Dr";
            }
        });
    };

    $scope.Get = function (data) {
        $scope.voucher = data;
        $scope.voucher.ExpenseBookingId = data.Id;
        $scope.voucher.EmployeeId = data.EmployeeId;
        $scope.getCash(data.EntityId);
        $scope.getCboVoucherTypeCashJournalList();
        $scope.expensesBookingDetailList = [];
        $scope.GetExpensesBookingById($scope.voucher.Id);
        $scope.voucher.AddedDate = $filter("dateFiltering")($scope.voucher.AddedDate);
        $scope.voucher.UpdatedDate = $filter("dateFiltering")($scope.voucher.UpdatedDate);
        $scope.voucher.DocDate = $filter("dateFiltering")(data.InvoiceDate);
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.InvoiceDate);
        $scope.voucher.VoucherDate = $filter("dateFiltering")(new Date());
        $scope.voucher.CurrencyId = data.CurrencyId;
        $scope.voucher.DocRefNo = data.InvoiceNumber;
        $scope.voucher.PlantId = data.PlantId;
        $scope.voucher.BeneficiaryType = data.BeneficiaryType;

        $scope.Action = "Post";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
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
        }
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            msg = "Posting date must be below or equal to Doc Date!";
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

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        if ($scope.form1.$valid && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            try {
                if ($scope.expensesBookingDetailList.length < 1) {
                    throw "Please add at least one TransactionDetail. ";
                }
                if ($scope.Action === "Post") {
                    $http({
                        method: "POST",
                        url: $scope.saveUrl,
                        data: {
                            "voucherVM": $scope.voucher,
                            "voucherDetailList": $scope.expensesBookingDetailList
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.getData();
                            baseService.paginationAdd();
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
            } catch (e) {
                throw ShowResult(e, "failure");
            }
        }
        return true;
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.voucher.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.budgetTransactionMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = "Post";
        $scope.voucher = {};
        $scope.expensesBookingDetailList = [];
        $scope.voucher.Active = true;
    }
}