"use strict";
salaryPayableDisbursementController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function salaryPayableDisbursementController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Salary Disbursement";
    $scope.Action = "Save";
    $scope.CAction = "Add";
    $scope.isPartyListing = false;
    $scope.index = -1;
    $scope.budgetTransactionMasters = [];
    $scope.currencyExchangeRate = [];
    $scope.voucherDetailList = [];
    $scope.transactionTypeList = [];
    $scope.salaryPayables = [];
    $scope.path = "accounts/salarydisbursement/";
    $scope.getListUrl = $scope.path + "GetSalaryPayableDisbursementVoucherList";
    $scope.saveUrl = $scope.path + "ParkSalaryPayableDisbursement";
    $scope.updateUrl = $scope.path + "UpdateEmployeePayable";
    $scope.deleteUrl = $scope.path + "delete/";
    $scope.postUrl = $scope.path + "PostEmployeePayable";
    $scope.hideSource = true;
    $scope.isWriteOff = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });
   // $controller("bankBaseController", { $scope: $scope, $http: $http });
    baseService.init($scope.getListUrl, null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");


    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.getData = function (pageno) {
        baseService.pagination(pageno, $scope.parameters)
            .then(function (result) {
                $scope.salaryPayables = result.Rows;
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
            "name": "Doc Ref No",
            "value": "DocRefNo"
        },
        {
            "name": "MonthNo",
            "value": "MonthNo"
        },
        {
            "name": "Year No",
            "value": "Year No"
        },
        {
            "name": "Employee Bank",
            "value": "EmployeeBank"
        },
        {
            "name": "Payment Bank",
            "value": "PaymentBank"
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
        BankGLGeneralInfoId: null,
        PaymentMode: '',
        IsActive: true,
        IsSeperated: false,
        IsMaternity: false


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
    
    $scope.getCboVoucherTypeSalaryDisbursementList = function () {
        cboService.getCboVoucherTypeSalaryDisbursementList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
            }
        });
    }
    $scope.getCboVoucherTypeSalaryDisbursementList();
    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
    };
    cboService.GetCboExpensesBookingTransactionType(function (result) {
        $scope.employeeTransactionTypeList = result;
    });
    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
    });
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });
    $scope.monthList = [
        {
            Value: 1,
            Text: 'January'
        },
        {
            Value: 2,
            Text: 'February'
        },
        {
            Value: 3,
            Text: 'March'
        },
        {
            Value: 4,
            Text: 'April'
        },
        {
            Value: 5,
            Text: 'May'
        },
        {
            Value: 6,
            Text: 'June'
        },
        {
            Value: 7,
            Text: 'July'
        },
        {
            Value: 8,
            Text: 'August'
        },
        {
            Value: 9,
            Text: 'September'
        },
        {
            Value: 10,
            Text: 'October'
        },
        {
            Value: 11,
            Text: 'November'
        },
        {
            Value: 12,
            Text: 'December'
        }
    ];
    $scope.changeMonth = function () {
        $scope.monthName = $.grep($scope.monthList, function (item) {
            return item.Value == $scope.voucher.MonthNo;
        })[0].Text;
    }
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

    $scope.takeAwaylist = [];
    $scope.cTClist = [];
    $scope.take = {};
    $scope.take.TakeAway = 'TakeAway';
    $scope.CT = {};
    $scope.CT.CTC = 'CTC';
    $scope.takeAwaylist.push($scope.take);
    $scope.cTClist.push($scope.CT);

    $scope.salaryLockPayableGLData = [];
    $scope.getSalaryLockPayableGL = function () {

        $scope.salaryLockPayableGLData = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Accounts/SalaryDisbursement/GetDirectSalaryPayableDisbursementDataList?yearNo=' + $scope.voucher.YearNo + '&monthNo=' + $scope.voucher.MonthNo + '&pMode=' + $scope.voucher.PaymentMode
                + '&isActive=' + $scope.voucher.IsActive + '&isSeperated=' + $scope.voucher.IsSeperated + '&isMaternity=' + $scope.voucher.IsMaternity + '&bankId=' + $scope.voucher.BankId,

        }).then(function successCallback(response) {
            $scope.salaryLockPayableGLData = response.data;
        });
    };

    $scope.employeeDisbursementDataList = [];
    $scope.GetemployeeDisbursement = function () {
        $scope.employeeDisbursementDataList = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Accounts/SalaryDisbursement/GetEmployeeDisbursementDataList?yearNo=' + $scope.voucher.YearNo + '&monthNo=' + $scope.voucher.MonthNo + '&pMode=' + $scope.voucher.PaymentMode
                + '&isActive=' + $scope.voucher.IsActive + '&isSeperated=' + $scope.voucher.IsSeperated + '&isMaternity=' + $scope.voucher.IsMaternity,
            
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.empGrid = true;
                $scope.employeeDisbursementDataList = response.data;
            }
            else {
                ShowResult("No Data Found", 'failure');
                $scope.empGrid = false;
            }
        });
    };

    $scope.GetBankemployeeDisbursement = function () {
        $scope.employeeDisbursementDataList = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Accounts/SalaryDisbursement/GetEmployeeDisbursementDataList?yearNo=' + $scope.voucher.YearNo + '&monthNo=' + $scope.voucher.MonthNo + '&pMode=' + $scope.voucher.PaymentMode + '&bankId=' + $scope.voucher.BankId,
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.empGrid = true;
                $scope.employeeDisbursementDataList = response.data;
            }
            else {
                ShowResult("No Data Found", 'failure');
                $scope.empGrid = false;
            }
        });
    };

    function Get(id) {
        $scope.voucher.Amount = 0;
        $scope.expensesBookingDetailList = [];
        $scope.voucherDetailCurrencyList = [];
        $scope.GetExpensesBookingById(id);
        $scope.Action = "Save";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }



    $scope.removeRow = function (index) {
        var row = $scope.voucherDetailList[index];
        $scope.voucherDetailList.splice(index, 1);
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



  
    

    $scope.entityChange = function (id) {
        var entityrowdata = $filter("filter")($scope.entityList, { Value: id });
        $scope.voucher.PlantId = entityrowdata[0].PlantId;
    };

   

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        try {
            if ($scope.form0.$valid ) {
                if ($scope.Action === "Save") {
                    $http({
                        method: "POST",
                        url: $scope.saveUrl,
                        data: {
                            "voucherVM": $scope.voucher,
                            "yearNo": $scope.voucher.YearNo,
                            "monthNo": $scope.voucher.MonthNo,
                            "monthName": $scope.monthName,
                            "pMode": $scope.voucher.PaymentMode,
                            "directJVList": $scope.salaryLockPayableGLData
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

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.budgetTransactionMaster.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.budgetTransactionMaster.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.budgetTransactionMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.Clear();
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
        $scope.Action = "Save";
        $scope.voucher = {};
        $scope.voucher.PaymentMode ='';
        $scope.voucher.EmployeeId = null;
        $scope.getCboVoucherTypeSalaryDisbursementList();
        $scope.voucher.Active = true;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.DocRefNo = null;
        $scope.salaryLockPayableData = [];
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
    $scope.paymentModeList = [];
    $http({
        method: 'GET',
        url: 'Enum/GetEMPPaymentModeEnumCbo/'
    }).then(function successCallback(response) {
        $scope.paymentModeList = response.data;
        });

    $scope.bankList = [];
    $scope.getBank = function () {
        $http({
            method: 'GET',
            url: 'Accounts/salarydisbursement/GetBankList?yearNo=' + $scope.voucher.YearNo + '&monthNo=' + $scope.voucher.MonthNo
        }).then(function successCallback(response) {
            $scope.bankList = response.data;
        });
    }
    $scope.changePaymentMode = function () {
        if ($scope.voucher.PaymentMode == 'Bank') {
            $scope.voucher.PaymentSource = $scope.voucher.PaymentMode;
            $scope.getBank();
        }
        else {
            $scope.voucher.PaymentSource = $scope.voucher.PaymentMode;
            $scope.GetemployeeDisbursement();
            $scope.getSalaryLockPayableGL();
            $scope.voucher.BankId = null;
        }
    }
    $scope.changeBank = function () {
        if ($scope.voucher.PaymentMode == 'Bank') {
            $scope.voucher.PaymentSource = $scope.voucher.PaymentMode;
            $scope.GetBankemployeeDisbursement();
            $scope.getSalaryLockPayableGL();
        }
    }
    $scope.bankmasterList = [];
    $scope.bankParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "BankName, BankBranchName, AccountTitle",
        searchBy: "AccountNumber",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showBankPopUp = function () {
        $scope.getBankList = function (pageno) {
         
            $scope.url = "Accounts/SalaryDisbursement/GetBankMasterList?bankACType=HouseBank&&bankId=" + $scope.voucher.BankId;
            baseService.paginationBase($scope.url, pageno, $scope.bankParameters)
                .then(function (result) {
                    $scope.bankmasterList = result.Rows;
                    $scope.bankParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getBankList();
        angular.element(document.querySelector("#bankPopUp")).modal("show");
    };

    $scope.closeCashPopUp = function () {
        //if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
        //    ShowResult("Please Select Currency!", "failure", "cashPopUp");
        //    return;
        //}
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
                $scope.voucher.GLGeneralInfoName = cash.GLItem;
                $scope.voucher.BudgetName = cash.BudgetName;
                $scope.voucher.BudgetMasterId = cash.BudgetMasterId;
                $scope.voucher.ActivityId = cash.ActivityId;
                $scope.voucher.ActivityName = cash.ActivityName;
                $scope.checkCashAmount();
            }
        }
        $scope.hideCashPopUp();
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankmasterList[$scope.bankIndex];
           
                $scope.voucher.AccountTitle = bank.AccountTitle;
                $scope.voucher.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
                $scope.voucher.BankMasterId = bank.BankMasterId;
                $scope.voucher.BankCurrencyId = bank.CurrencyId;

                $scope.voucher.GLGeneralInfoId = bank.GLGeneralInfoId;
                $scope.voucher.GLGeneralInfoName = bank.GLGeneralInfoName;
                $scope.voucher.BudgetMasterId = bank.BudgetMasterId;
                $scope.voucher.BudgetName = bank.BudgetName;
                $scope.voucher.ActivityId = bank.ActivityId;
                $scope.voucher.ActivityName = bank.ActivityName;
                $scope.checkBankAmount();
        }
        $scope.hideBankPopUp();
    };
    $scope.selectBankPopUp = function (index, id) {
        $scope.bankIndex = index;
    };

    $scope.hideBankPopUp = function () {
        angular.element(document.querySelector("#bankPopUp")).modal("hide");
        $scope.bankIndex = -1;
        $scope.bankSelected = null;
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

    $scope.GetEmployeeSalaryProcessedReportSalaryLogWiseInVoucher = function () {
        try {

            //$scope.month = p.MonthNo;
            //$scope.year = p.YearNo;
            //$scope.payableVoucherId = p.PayableVoucherId;
            var EmpBank = $("#PaymentModeIds option:selected").text();
            if (EmpBank == '--Select--') {
                EmpBank = '';
            }
            $http({
                method: 'POST',
                url: 'Accounts/SalaryDisbursement/GetEmployeeSalaryProcessedReportSalaryLogWiseSalaryPayableInVoucher',
                data: {
                    'month': $scope.voucher.MonthNo,
                    'year': $scope.voucher.YearNo, 
                    'salaryProcessId': $scope.voucher.salaryProcessId,
                    'payRollGroup': $scope.voucher.payGroupListSelected,
                    'parameters': $scope.voucher.parameters,
                    'isActive': $scope.voucher.IsActive,
                    'isSeperated': $scope.voucher.IsSeperated,
                    'isMaternity': $scope.voucher.IsMaternity,
                    'voucherId': null,
                    'Mode': $scope.voucher.PaymentMode,
                    'EmpBank': EmpBank
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.deleteUrl = "Accounts/SalaryDisbursement/DeleteSalaryDisbursementVoucher";

    $scope.deleteSalaryDisbursement = function (voucherId, monthNo, yearNo) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "voucherId": voucherId,
                "monthNo": monthNo,
                "yearNo": yearNo,
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
                $scope.voucherId = null;
                $scope.DelMonthNo = null;
                $scope.DelYearNo = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.confirmDelete = function (data) {
        $scope.voucherId = data.PayableVoucherId;
        $scope.DelMonthNo = data.MonthNo;
        $scope.DelYearNo = data.YearNo;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

}