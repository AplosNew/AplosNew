"use strict";
multipleEmployeePaymentController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function multipleEmployeePaymentController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Multiple Employee Payment";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.currencyExchangeRate = [];
    $scope.isWriteOff = true;
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $scope.partyType = "Employee";
    $scope.isAdvance = false;
    $scope.isBankAmount = false;
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    baseService.init("Accounts/EmployeePayable/GetEmployeePaymentList", null, null, "DESC", "VoucherNo", "VoucherNo");
    $scope.path = "Accounts/EmployeePayable/";
    $scope.postUrl = $scope.path + "PostEmployeePayment";
    $scope.deleteUrl = $scope.path + "/DeleteEmployeePayment";

    $scope.voucher = {
        Id: null,
        CompanyId: null,
        PartyId: null,
        EntityId: null,
        PlantId: null,
        PartyName: null,
        PartyType: null,
        CurrencyId: null,
        PaymentTermId: null,
        SourceType: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: $filter("dateFiltering")(Date.now()),
        BankTransactionDate: $filter("dateFiltering")(Date.now()),
        BankReferenceNo: null,
        DocDate: $filter("dateFiltering")(Date.now()),
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        MatureDate: null,
        DocRefNo: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        IsExcludingTax: false,
        IsSplit: false,
        Amount: 0,
        Narration: null,
        Remarks: null,
        BankName: null,
        BankMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        SourceFrom: null,
        SourceTo: null,

        GLGeneralInfoId: null,
        GLGeneralInfoName: null,
        BudgetId: null,
        BudgetName: null,
        ActivityId: null,
        ActivityName: null,

        EmployeeId: null,
        EmployeeName: null,
        EmployeeGLGeneralInfoId: null,
        EmployeeGLGeneralInfoName: null,
        EmployeeTransactionTypeId: null,
        PaymentSource: "Bank",
        RoundingType: null,
        RoundingAmount: null
    };

    $scope.voucherDetail = {
        EntityId: null
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
        TrnType: null
    };

    $scope.GetEmployeeTransactionNo = function (employeeId) {
        $http({
            method: "GET",
            url: "accounts/expenseBooking/GetEmployeeTransactionNo?employeeId=" + employeeId
        }).then(function successCallback(response) {
            $scope.employeeTransactionNo = response.data;
            $scope.voucher.DocRefNo = "EP-" + $scope.employeeTransactionNo;
        });
    };

    $scope.GetCurrencyParallel = function () {
        $http({
            method: "GET",
            url: "currencies/CompanyParallelCurrency/CurrencyParallel",
        }).then(function successCallback(response) {
            $scope.CurrencyParallel = response.data;
            if ($scope.CurrencyParallel.length === 0) {
                $scope.pop("error", "Company Parallel Currency is not set! ");
                $scope.showform = false;
            }
            else {
                $scope.showform = true;
            }
            $scope.BaseCurrencyCode = $scope.CurrencyParallel[0].Code;
        });
    };
    $scope.GetCurrencyParallel();

    $scope.tranCurrencyList = [];
    cboService.getCboParallelCurrency(function (result) {
        $scope.tranCurrencyList = result;
        $scope.voucher.CurrencyId = $scope.tranCurrencyList[0].Value;
    });

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.paymentList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };

    $scope.getData();

    $scope.searchEmployeePaymentList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
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
            "name": "Currency",
            "value": "CurrencyCode"
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
            "name": "Status",
            "value": "RowState"
        }
    ];

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        cboService.getCboEntityByCompanyWise(null, null, function (result) {
            $scope.entityList = result;
        });
    });

    $scope.getBudgetCboByGL = function (glgeneralInfoId) {
        cboService.getBudgetCboByGL(glgeneralInfoId, function (result) {
            $scope.BudgetItemList = result;
            if ($scope.BudgetItemList.length === 1) {
                $scope.voucherDetail.BudgetId = $scope.BudgetItemList[0].Value;
                $scope.voucherDetail.BudgetName = $scope.BudgetItemList[0].Text;
                $scope.getActivity(glgeneralInfoId);
            }
        });
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
            $scope.currencyExchangeRate = null;
        }
    };

    cboService.getEnumCbo('Enum/GetCboRoundingType', function (result) {
        $scope.roundingTypeList = result;
        $scope.voucher.RoundingType = $scope.roundingTypeList[0].Value;
    });

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            msg = "Doc date must be below or equal to Posting Date!";
            $scope.invalidDocDate = true;
        }
        else $scope.invalidDocDate = false;
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
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if (new Date($scope.voucherDetailList[i].PostingDate) > new Date($scope.voucher.PostingDate)) {
                msg = "Posting date must be below or equal to payable of " + $scope.voucherDetailList[i].DocRefNo;
                $scope.invalidPostingDate = true;
                break;
            }
            else {
                $scope.invalidPostingDate = false;
            }
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.invalidEntity = false;
    $scope.entityValidation = function () {
        $scope.invalidEntity = baseService.isUndefinedOrNull($scope.voucher.EntityId);
        return manualValidation("div_entity", $scope.invalidEntity, "Entity is required.");
    };

    //$scope.employeeWiseOutstandingAdvanceList = [];
    //$scope.getEmployeeWiseOutstandingAdvance = function (id) {
    //    $scope.employeeWiseOutstandingAdvanceList = [];
    //    $http({
    //        method: "GET",
    //        url: "accounts/Advance/GetEmployeeTotalAdvanceAmountByEmployeeId?employeeId=" + id
    //    }).then(function successCallback(response) {
    //        $scope.employeeOutStandingAdvanceDataList = response.data.Rows;
    //        $scope.TotalAdvanceAmount = $filter("sumByKey")($filter("filter")($scope.employeeOutStandingAdvanceDataList), "Balance");
    //        if ($scope.employeeOutStandingAdvanceDataList.length > 0) {
    //            angular.element(document.querySelector("#employeeOutStandingAdvancePopUp")).modal("show");
    //        }
    //    });
    //};
    //$scope.showEmployeeListPopUp = function () {
    //    baseService.setCurrentPage('employeeList');
    //    $scope.getEmployeeData = function (pageno) {
    //        var url = null;
    //        if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
    //            url = 'accounts/EmployeePayable/GetEmployeeListByPlant';
    //        }
    //        else {
    //            url = $scope.employeeUrl;
    //        }
    //        baseService.paginationBase(url, pageno, $scope.employeeParameters)
    //            .then(function (result) {
    //                $scope.employeeList = result.Rows;
    //                $scope.employeeParameters.total_count = result.Total;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //    };
    //    angular.element(document.querySelector('#employeePopUp')).modal('show');
    //    $scope.getEmployeeData();
    //};

    //$scope.closeEmployeePopUp = function () {
    //    if ($scope.employeeIndex !== -1) {
    //        var employee = $scope.employeeList[$scope.employeeIndex];
    //        $scope.voucher.EmployeeName = employee.EmployeeName;
    //        $scope.voucher.EmployeeId = employee.SystemId;
    //        $scope.voucher.EntityId = employee.EntityId;
    //        $scope.GetEmployeeTransactionNo($scope.voucher.EmployeeId);
    //        $scope.getEmployeeWiseOutstandingAdvance($scope.voucher.EmployeeId);
    //    }
    //    $scope.hideEmployeePopUp();
    //};

    //$scope.hideEmployeePopUp = function () {
    //    angular.element(document.querySelector("#employeePopUp")).modal("hide");
    //};

    //$scope.updatePartyAmount = function () {
    //    var row = $filter("filter")($scope.voucherDetailList, { "TrnType": "Cr" });
    //    if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
    //        row[0].Amount = $scope.voucher.Amount;
    //    }
    //};

    $scope.selectedInvoiceGLId = null;
    $scope.selectedInvoiceGLName = null;
    $scope.selectedInvoiceGL = function (selected) {
        if (selected) {
            $scope.selectedInvoiceGLId = selected.originalObject.GLGeneralInfoId;
            $scope.selectedInvoiceGLName = selected.originalObject.GLGeneralInfoName;
        }
    };

    $scope.inputChanged = function (str) {
        $scope.voucherDetail.GLGeneralInfoId = str;
    };

    $scope.getCboVoucherTypeEmployeePaymentList = function () {
        cboService.getCboVoucherTypeEmployeePaymentList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.BankTransactionDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
            }
        });
    }
    $scope.getCboVoucherTypeEmployeePaymentList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
    };

    //$scope.getById = function (id) {
    //    $http({
    //        method: "GET",
    //        url: "accounts/Advance/GetAdvance/" + id
    //    }).then(function successCallback(response) {
    //        $scope.voucher = response.data;
    //        $scope.voucher.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
    //        $scope.voucher.VoucherDate = $filter("dateFiltering")($scope.voucher.VoucherDate);
    //        $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucher.PostingDate);
    //        $scope.Action = "Update";
    //        if (!$rootScope.isCollapsed) {
    //            $rootScope.toggle();
    //        }
    //    });
    //};


    //$scope.employeePayableSearchList = [
    //    {
    //        "Text": "VoucherNo",
    //        "Value": "VoucherNo"
    //    },
    //    {
    //        "Text": "Employee Code",
    //        "Value": "EmployeeCode"
    //    },
    //    {
    //        "Text": "Employee Name",
    //        "Value": "EmployeeName"
    //    },
    //    {
    //        "Text": "Currency",
    //        "Value": "CurrencyCode"
    //    },
    //    {
    //        "Text": "Doc Date",
    //        "Value": "DocDate"
    //    },
    //    {
    //        "Text": "Doc Ref",
    //        "Value": "DocRefNo"
    //    }
    //];
    //$scope.employeePayableParameters = {
    //    limit: 5,
    //    offset: 0,
    //    order: "ASC",
    //    sort: "VoucherNo",
    //    searchBy: "VoucherNo",
    //    pageSize: 5,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};

    //$scope.getPopupEmployeePayableList = function () {
    //    $scope.getEmployeePayableData = function (pageno) {
    //        $scope.customerReceivableGLUrl1 = "accounts/EmployeePayable/GetEmployeeAvailableInvoiceList?employeeId=" + $scope.voucher.EmployeeId;
    //        baseService.paginationBase($scope.customerReceivableGLUrl1, pageno, $scope.employeePayableParameters)
    //            .then(function (result) {
    //                try {
    //                    $scope.employeePayableDataList = result.Rows;
    //                    $scope.employeePayableParameters.total_count = result.Total;
    //                    if (baseService.arrayLength($scope.employeePayableSearchList) === 0) {
    //                        baseService.getDDLSearchColumn($scope.employeePayableDataList, $scope.employeePayableSearchList);
    //                    }
    //                } catch (e) {
    //                    ShowResult(e, "Error");
    //                }
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, "failure");
    //            }).finally(function () {
    //            });
    //    };
    //    angular.element(document.querySelector("#employeePayablePopUp")).modal("show");
    //    $scope.getEmployeePayableData();
    //};
    //$scope.closePopUp = function () {
    //    angular.element(document.querySelector("#employeePayablePopUp")).modal("hide");
    //};

    //$scope.selectEmployeePayablePopUp = function (data) {
    //    data.Amount = null;
    //    data.TrnType = "Dr";
    //    var getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "DocRefNo": data.DocRefNo });
    //    if (getRow.length === 0) {
    //        data.Amount = data.Balance;
    //        $scope.voucherDetailList.push(data);
    //        $scope.GetCurrencyExchangeRateList(data.CurrencyId, data);
    //        if ($scope.voucherDetailList.length > 0)
    //            $scope.isReadOnly = true;
    //        else
    //            $scope.isReadOnly = false;
    //        angular.element(document.querySelector("#employeePayablePopUp")).modal("hide");
    //    }
    //    else {
    //        ShowResult("Already Exist Payable", "failure", "employeePayablePopUp");
    //    }
    //};

    //$scope.showEmployeeOutStanding = function () {
    //    angular.element(document.querySelector("#employeeOutStandingAdvancePopUp")).modal("show");
    //}

    //$scope.closeEmployeeOutStandingAdvancePopUp = function () {
    //    angular.element(document.querySelector("#employeeOutStandingAdvancePopUp")).modal("hide");
    //}

    $scope.totalInvoiceAmount = function () {
        var invoiceData = null;
        $scope.voucher.InvoiceAmount = 0;
        invoiceData = $filter("filter")($scope.voucherDetailCurrencyList, { "TrnType": "Dr" });
        angular.forEach(invoiceData, function (item, i) {
            $scope.voucher.InvoiceAmount += parseFloat(item.CompanyCurrencyDr);
        });
        $scope.voucher.Amount = parseFloat($scope.voucher.InvoiceAmount);
    };

    //$scope.removeRow = function (index) {
    //    $scope.voucherDetailList.splice(index, 1);
    //    $scope.deletecurrency = null;
    //};

    $scope.clearBankCash = function (from) {
        $scope.voucher.GLGeneralInfoId = null;
        $scope.voucher.GLGeneralInfoName = null;
        $scope.voucher.BudgetMasterId = null;
        $scope.voucher.ActivityId = null;
        $scope.voucher.BankName = null;
        $scope.voucher.CashName = null;
        $scope.voucher.SourceFrom = from;
        $scope.voucher.BankMasterId = null;
        $scope.voucher.CashMasterId = null;
        $scope.isBankAmount = false;
        $scope.voucher.BankCurrencyId = null;
    };

    $scope.closeCashPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please Select Currency!", "failure", "cashPopUp");
            return;
        }
        if ($scope.cashIndex !== -1) {
            var cash = $scope.cashList[$scope.cashIndex];
            if (baseService.isUndefinedOrNull(cash.GLGeneralInfoId)) {
                ShowResult("Cash GL not found!", "failure", "bankPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(cash.BudgetMasterId)) {
                ShowResult("Cash Budget not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(cash.CurrencyId)) {
                ShowResult("Cash Transaction Currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.voucher.CashMasterId = cash.Id;
                $scope.voucher.CashCurrencyId = cash.CurrencyId;
                $scope.voucher.CashName = cash.CashName;
                $scope.voucher.GLGeneralInfoId = cash.GLGeneralInfoId;
                $scope.voucher.GLGeneralInfoName = cash.GLGeneralInfoCode + " - " + cash.GLGeneralInfoName;
                $scope.voucher.BudgetMasterId = cash.BudgetMasterId;
                $scope.voucher.BudgetName = cash.BudgetName;
                $scope.voucher.ActivityId = cash.ActivityId;
                $scope.voucher.ActivityName = cash.ActivityName;
                $scope.totalInvoiceAmount();
            }
        }
        $scope.hideCashPopUp();
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
                ShowResult("Bank GL not found!", "failure", "bankPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(bank.BudgetMasterId)) {
                ShowResult("Bank Budget not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(bank.CurrencyId)) {
                ShowResult("Bank Transaction Currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.voucher.AccountTitle = bank.AccountTitle;
                $scope.voucher.BankName = bank.GLGeneralInfoCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
                $scope.voucher.BankMasterId = bank.BankMasterId;
                $scope.voucher.BankCurrencyId = bank.CurrencyId;

                $scope.voucher.GLGeneralInfoId = bank.GLGeneralInfoId;
                $scope.voucher.GLGeneralInfoName = bank.GLGeneralInfoCode + " - " + bank.GLGeneralInfoName;
                $scope.voucher.BudgetMasterId = bank.BudgetMasterId;
                $scope.voucher.BudgetName = bank.BudgetName;
                $scope.voucher.ActivityId = bank.ActivityId;
                $scope.voucher.ActivityName = bank.ActivityName;
            }
        }
        $scope.hideBankPopUp();
    };

    $scope.checkBankAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.BankCurrencyId)) {
            if ($scope.voucher.BankCurrencyId !== $scope.companyCurrencyId) {
                $scope.isBankAmount = true;
                $scope.voucher.BankAmount = 0;
            }
            else {
                $scope.isBankAmount = false;
                $scope.voucher.BankAmount = 0;
            }
        }
    };

    $scope.checkCashAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.CashCurrencyId)) {
            if ($scope.voucher.CashCurrencyId !== $scope.companyCurrencyId) {
                $scope.isBankAmount = true;
                $scope.voucher.BankAmount = 0;
            }
            else {
                $scope.isBankAmount = false;
                $scope.voucher.BankAmount = 0;
            }
        }
    };

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.Active = true;
        $scope.voucher.Amount = null;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.CurrencyId = $scope.tranCurrencyList[0].Value;
        $scope.voucher.EmployeeId = null;
        $scope.voucher.EmployeeName = null;
        $scope.voucher.EmployeeName = null;
        $scope.voucher.BankName = null;
        $scope.voucher.CashName = null;
        $scope.voucher.Narration = null;
        $scope.voucher.PaymentSource = "Bank";
        $scope.voucher.RoundingType = $scope.roundingTypeList[0].Value;
        $scope.voucher.RoundingAmount = null;
        $scope.getCboVoucherTypeEmployeePaymentList();
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
        $scope.TotalAdvanceAmount = null;
        $scope.employeeOutStandingAdvanceDataList = [];
        $scope.getFiscalYearPeriod($scope.voucher.PostingDate);
    };

    $scope.passBankCashAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.CashCurrencyId)) {
            if ($scope.voucher.CashCurrencyId === $scope.companyCurrencyId) {
                $scope.voucher.BankAmount = $scope.voucher.Amount;
            }
        }
        if (!baseService.isUndefinedOrNull($scope.voucher.BankCurrencyId)) {
            if ($scope.voucher.BankCurrencyId === $scope.companyCurrencyId) {
                $scope.voucher.BankAmount = $scope.voucher.Amount;
            }
        }
    };

    $scope.confirmSave = function () {
        if ($scope.Action === "Save" && $scope.TotalAdvanceAmount > 0) {
            $scope.message_confirmation = "This Employee have advance. Are you sure to Save?";
            angular.element(document.querySelector("#confirmSavePopUp")).modal("show");
        } else {
            $scope.Save();
        }
    };

    $scope.Save = function () {
        if ($scope.voucher.CompanyCurrencyRate == 0)
            $scope.voucher.CompanyCurrencyRate = 1
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        $scope.passBankCashAmount();
        if ($scope.form1.$valid && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "accounts/EmployeePayable/InsertEmployeePayment",
                    data: {
                        "voucherVM": $scope.voucher,
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
                    url: "accounts/EmployeePayable/UpdateEmployeePayment",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.voucherDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.menuFrames[$scope.index] = $scope.menuFrame;
                        }
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            return true;
        }
    };

    $scope.Report = function (voucherId) {
        location.href = "accounts/EmployeePayable/EmployeePaymentReport?voucherId=" + voucherId;
    };

    $scope.payableId = null;
    $scope.confirmPost = function (payableId) {
        $scope.payableId = payableId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function (payableId) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "id": payableId
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

    $scope.delete = function (employeePayableWriteOffId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "employeePayableWriteOffId": employeePayableWriteOffId, "voucherId": voucherId
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
                $scope.employeePayableWriteOffId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.employeePayableWriteOffId = null;
    $scope.confirmDelete = function (employeePayableWriteOffId, voucherId) {
        $scope.employeePayableWriteOffId = employeePayableWriteOffId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

    $scope.searchByParty = "EmployeeName"; $scope.searchParty = "";
    $scope.searchByPartyList = [{ value: 'EmployeeCode', name: "Employee Code" }, { value: 'EmployeeName', name: "Employee Name" }, { value: 'LegalDesignation', name: "Designation" }, { value: 'Department', name: "Department" }, { value: 'Section', name: "Section" }, { value: 'SubSection', name: "SubSection" }];

    $scope.partyList = [];
    $scope.getPopupEmployeeList = function () {
        $scope.partyList = [];
        if ($scope.partyType === 'Employee') {
            $scope.partyUrl = 'accounts/EmployeePayable/GetMultipleEmployeeList';

        }
        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#partyPopUp')).modal('show');
    };

    $scope.closePartyPopUpNew = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
    };

    $scope.hidePartyPopUp = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };

    $scope.partyDataListNew = [];
    $scope.ViewParty = function () {
        try {
            for (var i = 0; i < $scope.partyList.length; i++) {
                if ($scope.partyList[i].CheckBoxSelect == true) {
                    if (checkDoublePartyInformation($scope.partyDataListNew, $scope.partyList[i].EmployeeId) === false) {
                        $scope.partyDataListNew.push($scope.partyList[i]);
                    }
                }
            }
            angular.element(document.querySelector('#partyPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkDoublePartyInformation(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.multiplePaymentDetail = [];

    $scope.tempList = [];
    $scope.paymentSelectedList = [];
    $scope.multipleEmployeeInvoiceSearchList = [
        {
            "name": "Voucher No",
            "value": "VoucherNo"
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
            "name": "Entity",
            "value": "EntityName"
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
        }
    ];
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'EmployeeName',
        searchBy: 'VoucherNo',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    function avoidCheckList(id) {
        for (var i = 0; i < $scope.paymentSelectedList.length; i++) {
            if ($scope.paymentSelectedList[i].EmployeePayableDetailId === id) {
                return true;
                break;
            }
        }
        return false;
    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.sqlInStatement = null;
    $scope.getPopupEmployeePayableList = function () {
        if ($scope.partyDataListNew.length > 0) {
            var uniqueEmployeeId = removeDuplicates($scope.partyDataListNew, 'EmployeeId');
            var wcEmpCode = "";
            if (uniqueEmployeeId.length > 0) {
                wcEmpCode = "IN(";
                wcEmpCode += Array.prototype.map.call(uniqueEmployeeId, function (item) { return "'" + item.EmployeeId + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcEmpCode;
        }

        $scope.tempList = [];
        $scope.customerreceivableGLData = function (pageno) {
            $scope.customerReceivableGLUrl1 = 'accounts/EmployeePayable/GetMultipleEmployeeAvailableInvoiceList?employeeId=' + $scope.sqlInStatement;
            baseService.paginationBase($scope.customerReceivableGLUrl1, pageno, $scope.popUpParameters)
                .then(function (result) {
                    try {
                        $scope.paymentList = [];
                        angular.forEach(result.Rows, function (item) {
                            if (avoidCheckList(item.EmployeePayableDetailId) === false) {
                                $scope.paymentList.push(item);
                            }
                        })
                        $scope.popUpParameters.total_count = result.Total;
                        for (var i = 0; i < $scope.paymentList.length; i++) {
                            $scope.paymentList[i].Active = getActive($scope.tempList, $scope.paymentList[i].EmployeePayableDetailId);
                        }
                    } catch (e) {
                        ShowResult(e, 'Error');
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#EmployeePayableListPopUP')).modal('show');
        $scope.customerreceivableGLData();
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#EmployeePayableListPopUP')).modal('hide');
    };
    
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeePayableDetailId === id) {
                return true;
            }
        }

        return false;
    }
    $scope.pushInTempList = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.EmployeePayableDetailId) === false) {
                    $scope.tempList.push(data);
                }
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                        if ($scope.tempList[i].EmployeePayableDetailId === data.EmployeePayableDetailId) {
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }

                    $scope.tempList.push(data);
                }
            }
            else {
                for (var t = 0; t < baseService.arrayLength($scope.tempList); t++) {
                    if ($scope.tempList[t].EmployeePayableDetailId === data.EmployeePayableDetailId) {
                        $scope.tempList.splice(t, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }

    function checkExistTempList(list, id) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].EmployeePayableDetailId === id) {
                return true;
            }
        }
        return false;
    }

    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });

    $scope.closePopUp = function () {
        if (baseService.arrayLength($scope.tempList) > 0) {
            angular.forEach($scope.tempList, function (item) {
                $scope.paymentSelectedList.push({
                    EmployeePayableDetailId: item.EmployeePayableDetailId
                    , EmployeePayableId: item.EmployeePayableId
                    , VoucherNo: item.VoucherNo
                    , PostingDate: item.PostingDate
                    , DocDate: item.DocDate
                    , DocRefNo: item.DocRefNo
                    , CurrencyCode: item.CurrencyCode
                    , Receivable: item.Receivable
                    , Received: item.Received
                    , Balance: item.Balance
                    , Amount: item.Amount
                    , EmployeeCode: item.EmployeeCode
                    , EmployeeId: item.EmployeeId
                    , EmployeeName: item.EmployeeName
                    
                });
            });
        }
        angular.element(document.querySelector('#EmployeePayableListPopUP')).modal('hide');
    };

    $scope.removeRow = function (index, data) {
        var row = $scope.paymentSelectedList[index];
        var drc = $scope.tempList.length;
        while (drc--) {
            if ($scope.tempList[drc]['EmployeePayableDetailId'] === row.EmployeePayableDetailId) {
                $scope.tempList.splice(drc, 1);
            }
        }
        $scope.paymentSelectedList.splice(index, 1);
    }

    $scope.copyPayableBalanceAmount = function () {
        for (var i = 0; i < $scope.paymentSelectedList.length; i++) {
            $scope.paymentSelectedList[i].Amount = $scope.paymentSelectedList[i].Balance;
        }
    }

    $scope.checkPaymentAmountByBalanceAmount = function (data) {
        if (data.Amount > data.Balance) {
            data.Amount = data.Balance;
            ShowResult("Payment Amount can't be greater than Balance Amount!!", 'failure');
        }
    }

}