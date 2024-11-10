"use strict";
expenseBookingApprovedController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$http", "$filter", "$controller", "$window"];
function expenseBookingApprovedController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $filter, $controller, $window) {
    $rootScope.title = "Expense Booking Approval Approved";
    $scope.Action = "Post";
    $scope.CAction = "Add";
    $scope.isPartyListing = false;
    $scope.index = -1;
    $scope.budgetTransactionMasters = [];
    $scope.currencyExchangeRate = [];
    $scope.CurrencyList = [];
    $scope.transactionTypeList = [];
    $scope.ispostDisable = false;
    $scope.path = "accounts/expenseBooking/";
    $scope.getListUrl = $scope.path + "getlist";
    $scope.saveUrl = $scope.path + "InsertExpenseBookingApproved";
    $scope.updateUrl = $scope.path + "edit";
    $scope.hideSource = true;
    $scope.isWriteOff = true;
    $scope.expensesBookingId = null;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });

    $scope.voucher = {
        Id: null,
        CompanyId: null,
        ExpenseBookingId: null,
        EmployeeId: null,
        EmployeeName: null,
        EmployeeCodeName: null,
        PartyGLGeneralInfoId: null,
        EmployeeTransactionTypeId: null,
        GLGeneralInfoId: null,
        CurrencyId: null,
        EntityId: null,
        PlantId: null,
        CurrencyCode: null,
        VoucherTypeId: null,
        PartyType: "Employee",
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: $filter("dateFiltering")(Date.now()),
        DocRefNo: null,
        DocDate: $filter("dateFiltering")(Date.now()),
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
        BeneficiaryType: null,
        PartyId: null,
        PartyPlantId: null,
        JournalType:null
    };

    $scope.advanceCA = null;
    $scope.getIsExpensesBookingGL = function () {
        $http.get("accounts/EmployeeTransaction/GetIsExpensesBookingGL/")
            .then(function (response) {
                $scope.advanceCA = response.data;
                if (manualValidation("div_TransactionType", baseService.isUndefinedOrNull($scope.advanceCA.PayableGLId), "Transaction Type GL not found!")) {
                    $scope.advanceCA = null;
                }
                else {
                    $scope.voucher.GLGeneralInfoId = $scope.advanceCA.PayableGLId;
                    $scope.voucher.GLGeneralInfoName = $scope.advanceCA.PayableGLCode + " - " + $scope.advanceCA.PayableGLName;
                    $scope.voucher.BudgetMasterId = $scope.advanceCA.PayableBudgetMasterId;
                    $scope.voucher.BudgetName = $scope.advanceCA.BudgetPayableName;
                    $scope.voucher.ActivityId = $scope.advanceCA.PayableActivityId;
                    $scope.voucher.ActivityName = $scope.advanceCA.PayableActivityName;
                }
            });
    };

    $scope.GetCboExpensesBookingTransactionType = function () {
        cboService.GetCboExpensesBookingTransactionType(function (result) {
            $scope.employeeTransactionTypeList = result;
            if ($scope.employeeTransactionTypeList.length === 1) {
                $scope.voucher.EmployeeTransactionTypeId = $scope.employeeTransactionTypeList[0].EmployeeTransactionTypeId;
                $scope.voucher.JournalType = $scope.employeeTransactionTypeList[0].AdvanceType;

                $scope.getIsExpensesBookingGL();
            }
        });
    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
    });

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

    $scope.getFiscalYearPeriod = function (date) {
        if (!baseService.isUndefinedOrNull(date)) {
            $http({
                method: "get",
                url: "accounts/CompanyFiscalYear/CheckingFiscalYearPeriod?postingDate=" + $filter("dateFiltering")(date)
            }).then(
                function successCallback(response) {
                    if (response.data.Error === true) {
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
                        }
                    }
                },
                function errorCallback(response) {
                });
        }
    };

    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });


    $scope.GetExpensesBookingById = function (id) {
        $http({
            method: "GET",
            url: "accounts/expenseBooking/GetExpensesBookingById?Id=" + id
        }).then(function successCallback(response) {
            $scope.expensesBookingDetailList = response.data.Rows;
            $scope.voucher.ExpenseBookingId = $scope.expensesBookingDetailList[0].ExpenseBookingId;
            $scope.voucher.EmployeeCodeName = $scope.expensesBookingDetailList[0].EmployeeCodeName;
            $scope.voucher.EmployeeId = $scope.expensesBookingDetailList[0].EmployeeId;
            $scope.voucher.CurrencyId = $scope.expensesBookingDetailList[0].CurrencyId;
            $scope.voucher.DocRefNo = $scope.expensesBookingDetailList[0].InvoiceNumber;
            $scope.voucher.DocDate = $filter("dateFiltering")($scope.expensesBookingDetailList[0].InvoiceDate);
            $scope.voucher.PostingDate = $filter("dateFiltering")($scope.expensesBookingDetailList[0].InvoiceDate);
            $scope.voucher.CurrencyCode = $scope.expensesBookingDetailList[0].CurrencyCode;
            $scope.voucher.EntityId = $scope.expensesBookingDetailList[0].EntityId;
            $scope.voucher.PlantId = $scope.expensesBookingDetailList[0].PlantId;
            $scope.voucher.PartyId = $scope.expensesBookingDetailList[0].PartyId;
            $scope.voucher.PartyPlantId = $scope.expensesBookingDetailList[0].PartyPlantId;
            $scope.voucher.PartyName = $scope.expensesBookingDetailList[0].PartyName;
            $scope.voucher.Narration = $scope.expensesBookingDetailList[0].Narration;
            $scope.voucher.BeneficiaryType = $scope.expensesBookingDetailList[0].BeneficiaryType;
            for (var i = 0; i < $scope.expensesBookingDetailList.length; i++) {
                $scope.expensesBookingDetailList[i].TrnType = "Dr";
                $scope.voucher.Amount += $scope.expensesBookingDetailList[i].Amount;
            }
            $scope.getFiscalYearPeriod($scope.voucher.PostingDate);
            $scope.GetCboExpensesBookingTransactionType();
        });
    };

    

    function Get(id) {
        $scope.expensesBookingId = id;
        $scope.voucher.Amount = 0;
        $scope.expensesBookingDetailList = [];
        $scope.GetExpensesBookingById(id);
        $scope.Action = "Post";
    }

    Get($routeParams.id);

    $scope.expensesBookingTransaction = function (id) {
        var employeeTransactionTypeData = $filter("filter")($scope.employeeTransactionTypeList, { EmployeeTransactionTypeId: id });
        $scope.voucher.GLGeneralInfoId = employeeTransactionTypeData[0].PayableGLId;
        $scope.voucher.GLGeneralInfoName = employeeTransactionTypeData[0].PayableGLCode + " - " + employeeTransactionTypeData[0].PayableGLName;
        $scope.voucher.BudgetMasterId = employeeTransactionTypeData[0].PayableBudgetMasterId;
        $scope.voucher.BudgetName = employeeTransactionTypeData[0].PayableBudgetName;
        $scope.voucher.ActivityId = employeeTransactionTypeData[0].PayableActivityId;
        $scope.voucher.ActivityName = employeeTransactionTypeData[0].PayableActivityName;
        $scope.voucher.JournalType = employeeTransactionTypeData[0].AdvanceType;
    };



    cboService.getCboVoucherTypeEmployeePayableList(function (result) {
        $scope.voucherTypeList = result;
        if ($scope.voucherTypeList.length === 1) {
            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
        }
    });


    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
    };

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
    $scope.budgetTransactionMaster = {
        Id: null,
        EmployeeId: null,
        InvoiceNumber: null,
        InvoiceDate: new Date(),
        Active: true
    };

    $scope.budgetTransactionDetail = {
        Id: null,
        BudgetTransactionMasterId: null,
        BudgetMasterId: null,
        PartyId: null,
        ActivityId: null,
        ActivityPhoneId: null,
        Amount: 0.00,
        ExpenseDate: new Date()
    };

    $scope.entityChange = function (id) {
        var entityrowdata = $filter("filter")($scope.entityList, { Value: id });
        $scope.voucher.PlantId = entityrowdata[0].PlantId;
    };

    $scope.removeRow = function () {
        $scope.budgetTransactionDetailList.splice($scope.dindex, 1);
    };


    $scope.checkDocDate = function () {
        if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            ShowResult("Doc date must be below or equal to Posting Date!", "failure");
            return true;
        }
        return false;
    };


    $scope.validation = function () {
        if ($scope.voucher.BeneficiaryType === "Self") {
            if (baseService.isUndefinedOrNull($scope.voucher.EmployeeTransactionTypeId)) {
                ShowResult("Please Select Transaction Type", "failure");
                return true;
            }
        }
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            ShowResult("Posting Date must be below or equal to current Date!", "failure");
            return true;
        }
        return false;
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        try {
            $scope.ispostDisable = true;
            if ($scope.form1.$valid && !$scope.validation() && !$scope.checkDocDate()) {
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
                            $scope.ispostDisable = false;
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            ClearFields(response.data.Sequence);
                            $scope.ispostDisable = true;
                            $window.history.back();
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
                            "budgetTransactionMaster": $scope.budgetTransactionMaster,
                            "expenseBookingDetailList": $scope.budgetTransactionDetailList
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            if ($scope.index > -1) {
                                $scope.budgetTransactionMasters[$scope.index] = $scope.budgetTransactionMaster;
                            }
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
            }
        } catch (e) {
            throw ShowResult(e, "failure");
        }
        return true;
    };

    $scope.Back = function () {
        $window.history.back();
    };

    $scope.Delete = function () {
            $http({
                method: "POST",
                url: $scope.path + "DeleteApprovedExpenseBooking?employeeBookingId=" + $scope.expensesBookingId,
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
                    $window.history.back();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        return true;
    };
    $scope.confirmDelete = function () {
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };
    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = "Post";
        $scope.voucher = {};
        $scope.expensesBookingId = null;
        $scope.expensesBookingDetailList = [];
    };
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
        sort: "GLGeneralInfoName",
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.indexGL = "";
    $scope.popUpGL = function (index) {
        $scope.indexGL = index;
        baseService.setCurrentPage("cOAICodeList");
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetExpenseGLBudgetActivity", pageno, $scope.glListParameters)
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
    $scope.setSelected = function (data, index) {
        $scope.expensesBookingDetailList[$scope.indexGL].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.expensesBookingDetailList[$scope.indexGL].GLGeneralInfoCode = data.GLGeneralInfoCode;
        $scope.expensesBookingDetailList[$scope.indexGL].GLGeneralInfoName = data.GLGeneralInfoName;
        $scope.expensesBookingDetailList[$scope.indexGL].BudgetMasterId = data.BudgetMasterId;
        $scope.expensesBookingDetailList[$scope.indexGL].BudgetName = data.BudgetName;
        $scope.expensesBookingDetailList[$scope.indexGL].ActivityId = data.ActivityId;
        $scope.expensesBookingDetailList[$scope.indexGL].ActivityName = data.ActivityName;
        $scope.expensesBookingDetailList[$scope.indexGL].ServiceMasterId = data.ServiceMasterId;
        $scope.closeCOAICodeListPopUp();
    };
}