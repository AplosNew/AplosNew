"use strict";
employeeSalaryPayableController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function employeeSalaryPayableController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Employee Salary Payable";
    $scope.Action = "Save";
    $scope.CAction = "Add";
    $scope.isPartyListing = false;
    $scope.index = -1;
    $scope.budgetTransactionMasters = [];
    $scope.currencyExchangeRate = [];
    $scope.voucherDetailList = [];
    $scope.transactionTypeList = [];
    $scope.employeePayables = [];
    $scope.path = "accounts/employeepayable/";
    $scope.getListUrl = $scope.path + "GetEmployeeSalaryPayableList";
    $scope.saveUrl = $scope.path + "InsertEmployeeSalaryPayable";
    $scope.updateUrl = $scope.path + "UpdateEmployeePayable";
    $scope.deleteUrl = $scope.path + "DeleteEmployeeSalaryPayable/";
    $scope.postUrl = $scope.path + "PostEmployeeSalaryPayable";
    $scope.hideSource = true;
    $scope.isWriteOff = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    baseService.init($scope.getListUrl, null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno, $scope.parameters)
            .then(function (result) {
                $scope.employeePayables = result.Rows;
                $scope.parameters.total_count = result.Total;
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
            "name": "Employee Code",
            "value": "EmployeeCode"
        },
        {
            "name": "Employee Name",
            "value": "EmployeeName"
        },
        {
            "name": "Invoice No",
            "value": "DocRefNo"
        },
        {
            "name": "Invoice Date",
            "value": "DocDate"
        },
        {
            "name": "Currency",
            "value": "Currency"
        }
    ];

    $scope.voucher = {
        Id: null,
        CompanyGroupId: null,
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
        Amount: null,
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
        CompanyCurrencyAmount: null
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

    cboService.GetCboExpensesBookingTransactionType(function (result) {
        $scope.employeeTransactionTypeList = result;
    });
    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
    });

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

    $scope.getDetail = function (voucherId) {
        $http({
            method: 'GET',
            url: 'Accounts/EmployeePayable/GetEmployeeSalaryPayableDetailList?voucherId=' + voucherId
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
        });
    };

    $scope.Get=function(data) {
        $scope.voucher = data;
        $filter("dateFiltering")
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.PostingDate);
        $scope.voucher.DocDate = $filter("dateFiltering")(data.DocDate);
        $scope.getDetail(data.VoucherId);
        $scope.Action = "Save";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.addRow = function (data) {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure", "GLPopUp");
            return true;
        }
        if ($scope.companyConfig.IsVoucherFromBudget)
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
            $scope.voucherDetail.DrAmount = null;
            $scope.voucherDetail.CrAmount = null;
            $scope.voucherDetail.TrnType = "Dr";
            $scope.voucherDetail.PartyType = "GL";
            $scope.voucherDetail.DrDisable = false;
            $scope.voucherDetail.CrDisable = false;
            $scope.voucherDetailList.push($scope.voucherDetail);
            $scope.voucherDetail = {};
            $scope.closeCOAICodeListPopUp();
        }
    };

    $scope.removeRow = function (index) {
        var row = $scope.voucherDetailList[index];
        $scope.voucherDetailList.splice(index, 1);
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
            baseService.paginationBase("Accounts/GLItem/GetGLBudgetActivityForEmployeeSalaryPayable", pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "GLPopUp");
                }).finally(function () {
                });
        };
        $scope.GetCOAICodeListData();
        angular.element(document.querySelector("#GLPopUp")).modal("show");
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

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.voucher.EmployeeName = employee.EmployeeName;
            $scope.voucher.EmployeeId = employee.SystemId;
            $scope.voucher.EntityId = employee.EntityId;
            $scope.GetEmployeeTransactionNo($scope.voucher.EmployeeId);
        }
        $scope.hideEmployeePopUp();
    };
    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
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

    $scope.expensesBookingTransaction = function (id) {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
        }
        var employeeTransactionTypeData = $filter("filter")($scope.employeeTransactionTypeList, { EmployeeTransactionTypeId: id });

        var getRow = $filter("filter")($scope.voucherDetailList, { "PartyType": "Employee"});

        if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 ) {
            ShowResult("Transaction Type already added!", "failure");
        }
        else {
            $scope.voucher.EmployeeTransactionTypeId = id;

            $scope.voucherDetail.EmployeeTransactionTypeId = id;
            $scope.voucherDetail.BudgetMasterId = employeeTransactionTypeData[0].PayableBudgetMasterId;
            $scope.voucherDetail.BudgetName = employeeTransactionTypeData[0].PayableBudgetName;
            $scope.voucherDetail.ActivityId = employeeTransactionTypeData[0].PayableActivityId;
            $scope.voucherDetail.ActivityName = employeeTransactionTypeData[0].PayableActivityName;

            $scope.voucherDetail.GLGeneralInfoId = employeeTransactionTypeData[0].PayableGLId;
            $scope.voucherDetail.GLGeneralInfoCode = employeeTransactionTypeData[0].PayableGLCode;
            $scope.voucherDetail.GLGeneralInfoName = employeeTransactionTypeData[0].PayableGLName;

            $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
            $scope.voucherDetail.CrAmount = null;
            $scope.voucherDetail.TrnType = "Cr";
            $scope.voucherDetail.PartyType = "Employee";
            $scope.voucherDetail.DrDisable = true;
            $scope.voucherDetail.CrDisable = false;
            $scope.voucherDetailList.push($scope.voucherDetail);
            $scope.voucherDetail = {};
        }
        
    };


   

    $scope.getCboVoucherTypeEmployeePayableList = function () {
        cboService.getCboVoucherTypeSalaryPayableList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
            }
        });
    }
    $scope.getCboVoucherTypeEmployeePayableList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
    };

    $scope.entityChange = function (id) {
        var entityrowdata = $filter("filter")($scope.entityList, { Value: id });
        $scope.voucher.PlantId = entityrowdata[0].PlantId;
    };

    $scope.checkDocDate = function () {
        if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            ShowResult("Doc date must be below or equal to Posting Date!", "failure");
            return true;
        }
        return false;
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        try {
            if ($scope.form0.$valid && !$scope.checkDocDate()) {
                if ($scope.Action === "Save") {
                    $http({
                        method: "POST",
                        url: $scope.saveUrl,
                        data: {
                            "voucherVM": $scope.voucher,
                            "voucherDetailList": $scope.voucherDetailList,
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
                            "voucherVM": $scope.voucher,
                            "voucherDetailList": $scope.voucherDetailList,
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
            }
        } catch (e) {
            throw ShowResult(e, "failure");
        }
        return true;
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

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.voucher = {};
        $scope.voucher.EmployeeId = null;
        $scope.getCboVoucherTypeEmployeePayableList();
        $scope.voucher.Active = true;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.DocRefNo = null;
        $scope.voucherDetailList = [];
        $scope.expensesBookingDetailList = [];
        $scope.advanceTaxesList = [];
    }

    $scope.getById = function (id) {
        $http({
            method: 'GET',
            url: 'Accounts/EmployeePayable/GetEmployeePayableById/' + id
        }).then(function successCallback(response) {
            $scope.voucher = response.data;
            $scope.getEmployeePayableDetailList($scope.voucher.VoucherId);
            $scope.voucher.DocDate = $filter('dateFiltering')($scope.voucher.DocDate);
            $scope.voucher.VoucherDate = $filter('dateFiltering')($scope.voucher.VoucherDate);
            $scope.voucher.PostingDate = $filter('dateFiltering')($scope.voucher.PostingDate);
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };

    $scope.getEmployeePayableDetailList = function (voucherId) {
        $http({
            method: 'GET',
            url: 'Accounts/EmployeePayable/GetEmployeePayableDetailList?voucherId=' + voucherId
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
        });
    };

    $scope.taxCodCboList = [];
    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYear = function (date) {
        $http({
            method: "get",
            url: "accounts/TaxCode/GetWithholdInputTaxCodeCbo?postingDate=" + $filter("dateFiltering")(date)
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

    $scope.advanceTaxesList = [];
    $scope.changeTaxCode = function (id) {
        var taxCodeData = $filter("filter")($scope.taxCodCboList, { Id: id });

        var getRowtax = $filter("filter")($scope.voucherDetailList, { "TrnType": "Cr", "BudgetMasterId": taxCodeData[0].BudgetMasterId, "ActivityId": taxCodeData[0].ActivityId, });

        if (!baseService.isUndefinedOrNull(getRowtax) && getRowtax.length > 0 && taxCodeData[0].BudgetMasterId === getRowtax[0].BudgetMasterId) {
            ShowResult("This Tax is already added!", "failure");
        }
        else {
            $scope.voucherDetail.TaxCodeId = id;
            $scope.voucherDetail.TaxCategoryId = taxCodeData[0].TaxCategoryId;

            $scope.voucherDetail.BudgetMasterId = taxCodeData[0].BudgetMasterId;
            $scope.voucherDetail.BudgetName = taxCodeData[0].BudgetName;
            $scope.voucherDetail.ActivityId = taxCodeData[0].ActivityId;
            $scope.voucherDetail.ActivityName = taxCodeData[0].ActivityName;

            $scope.voucherDetail.GLGeneralInfoId = taxCodeData[0].GLGeneralInfoId;
            $scope.voucherDetail.GLGeneralInfoCode = taxCodeData[0].GLGeneralInfoCode;
            $scope.voucherDetail.GLGeneralInfoName = taxCodeData[0].GLGeneralInfoName;

            $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
            $scope.voucherDetail.CrAmount = null;
            $scope.voucherDetail.DrAmount = null;
            $scope.voucherDetail.TrnType = "Cr";
            $scope.voucherDetail.PartyType = "Tax";
            $scope.voucherDetail.DrDisable = true;
            $scope.voucherDetail.CrDisable = false;
            $scope.voucherDetailList.push($scope.voucherDetail);
            $scope.voucherDetail = {};
        }
        
    };

    $scope.employeeAdvanceSearchList = [
        {
            "Text": "VoucherNo",
            "Value": "VoucherNo"
        },
        {
            "Text": "Employee Code",
            "Value": "EmployeeCode"
        },
        {
            "Text": "Employee Name",
            "Value": "EmployeeName"
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

    $scope.employeeAdvanceDataList = [];
    $scope.employeeAdvanceSearch = [];
    $scope.employeeAdvanceSelectedIndex = -1;
    $scope.employeeAdvanceParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'VoucherNo',
        searchBy: 'VoucherNo',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showEmployeeAdvancePopUpList = function (employeeId) {
        $scope.compareCurrencyId = $scope.voucher.CurrencyId;
        $scope.getEmployeeAdvanceData = function (pageno) {
            baseService.paginationBase('accounts/Advance/GetEmployeeAvilabeAdvanceByIdList?employeeId=' + $scope.voucher.EmployeeId, pageno, $scope.employeeAdvanceParameters)
                .then(function (response) {
                    $scope.employeeAdvanceDataList = response.Rows;
                    $scope.employeeAdvanceParameters.total_count = response.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeeAdvancePopUp')).modal('show');
        $scope.getEmployeeAdvanceData();
    };
    $scope.closeEmployeeAdvancePopUp = function (data) {

        var getRowAdvance = $filter("filter")($scope.voucherDetailList, { "PartyType": "Advance", "AdvanceId": data.AdvanceId});

        if (!baseService.isUndefinedOrNull(getRowAdvance) && getRowAdvance.length > 0) {
            ShowResult("This Advance is already added!", "failure", "employeeAdvancePopUp");
        }
        else {
            $scope.voucher.EmployeeId = data.EmployeeId;
            $scope.voucher.EmployeeName = data.EmployeeName;
            $scope.voucher.AdvanceAmount = data.Balance;
            $scope.voucher.VoucherNo = data.VoucherNo;
            $scope.voucher.CompanyId = data.CompanyId;
            $scope.voucher.PlantId = data.PlantId;
            $scope.voucher.CurrencyId = data.CurrencyId;

            $scope.voucher.PartyType = data.PartyType;
            $scope.advancePostingDate = data.PostingDate;
            $scope.advanceDocRefNo = data.DocRefNo;

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
            $scope.voucherDetail.DrAmount = null;
            $scope.voucherDetail.TrnType = "Cr";
            $scope.voucherDetail.PartyType = "Advance";
            $scope.voucherDetail.DrDisable = true;
            $scope.voucherDetail.CrDisable = false;
            $scope.voucherDetail.AdvanceId = data.AdvanceId;
            $scope.voucherDetail.AdvanceDetailId = data.AdvanceDetailId;
            $scope.voucherDetailList.push($scope.voucherDetail);
            $scope.voucherDetail = {};


            $scope.GetEmployeeTransactionNo($scope.voucher.EmployeeId);
            angular.element(document.querySelector("#employeeAdvancePopUp")).modal("hide");
        }
        
    };

    $scope.checkDrAmount = function (index) {
        if ($scope.voucherDetailList[index].DrAmount > 0) {
            $scope.voucherDetailList[index].CrAmount = null;
            $scope.voucherDetailList[index].TrnType = 'Dr';
        }
    };

    $scope.checkCrAmount = function (index) {
        if ($scope.voucherDetailList[index].CrAmount > 0) {
            $scope.voucherDetailList[index].DrAmount = null;
            $scope.voucherDetailList[index].TrnType = 'Cr';
        }
    };


    $scope.delete = function (payableId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "payableId": payableId, "voucherId": voucherId
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
                $scope.payableId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.payableId = null;
    $scope.confirmDelete = function (payableId, voucherId) {
        $scope.payableId = payableId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };
}