"use strict";
salaryPayableDisbursementController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "$window"];
function salaryPayableDisbursementController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $window) {
    $rootScope.title = "Disbursement Posting";
    $scope.Action = "Park";
    $scope.CAction = "Add";
    $scope.isPartyListing = false;
    $scope.index = -1;
    $scope.currencyExchangeRate = [];
    $scope.voucherDetailList = [];
    $scope.transactionTypeList = [];
    $scope.salaryPayables = [];
    $scope.path = "accounts/salarydisbursement/";
    $scope.getListUrl = $scope.path + "GetSalaryPayableDisbursementVoucherList";
    $scope.saveUrl = $scope.path + "ParkSalaryPayableDisbursement";
    $scope.postUrl = $scope.path + "PostSalarydisbursement";
    $scope.hideSource = true;
    $scope.isWriteOff = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });
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

    // #region TAB CHANGE Main
    $scope.tabMain = 1;
    $scope.setTabMain = function (newTab) {
        $scope.tabMain = newTab;
    };
    $scope.isSetMain = function (tabNum) {
        return $scope.tabMain === tabNum;
    };
    // #endregion TAB CHANGE Main

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.PostingDate + "&currencyId=" + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.voucher.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
                $scope.voucherBonus.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
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
    $scope.EmployeeListNew = [];
    $scope.getSalaryLockPayableGL = function () {

        $scope.salaryLockPayableGLData = [];
        $http({
            method: "POST",
            url: "Accounts/SalaryDisbursement/GetDirectSalaryPayableDisbursementDataList",
            data: { 'yearNo': $scope.voucher.YearNo, 'monthNo': $scope.voucher.MonthNo, 'disbursementAdviceId': $scope.voucher.DisbursementAdviceId, 'employeeListNew': $scope.EmployeeListNew },
            dataType: 'JSON'
            , contentType: "application/json charset=utf-8"
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
            url: 'Accounts/SalaryDisbursement/GetEmployeeDisbursementDataList?yearNo=' + $scope.voucher.YearNo + '&monthNo=' + $scope.voucher.MonthNo + '&disbursementAdviceId=' + $scope.voucher.DisbursementAdviceId,
            
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.empGrid = true;
                $scope.employeeDisbursementDataList = response.data;
                $scope.EmployeeListNew = $scope.employeeDisbursementDataList;
            }
            else {
                ShowResult("No Data Found", 'failure');
                $scope.empGrid = false;
            }
        });
    };

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
    $scope.saveBtnDisable = false;
    $scope.Save = function () {
        if ($scope.EmployeeListNew.length === 0) {
            ShowResult("Please select Employee!", "failure");
            return true;
        }
        if ( baseService.isUndefinedOrNull($scope.voucher.PaymentMode)) {
            ShowResult("Please select Payment Mode!", "failure");
                return true;
        }
        if ($scope.voucher.PaymentMode === "Bank") {
            if ($scope.voucher.BankName === "" || baseService.isUndefinedOrNull($scope.voucher.BankMasterId)) {
                ShowResult("Please select Bank!", "failure");
                return true;
            }
        }
        if ($scope.voucher.PaymentMode === "Cash") {
            if ($scope.voucher.CashName === "" || baseService.isUndefinedOrNull($scope.voucher.CashMasterId)) {
                ShowResult("Please select Cash!", "failure");
                return true;
            }
        }
        
        $scope.$broadcast("show-errors-check-validity");
        $scope.saveBtnDisable = true;
        try {
            if ($scope.form0.$valid) {
                if ($scope.Action === "Park") {
                    $http({
                        method: "POST",
                        url: $scope.saveUrl,
                        data: {
                            "voucherVM": $scope.voucher,
                            "yearNo": $scope.voucher.YearNo,
                            "monthNo": $scope.voucher.MonthNo,
                            "monthName": $scope.voucher.MonthName,
                            "pMode": $scope.voucher.PaymentMode,
                            "directJVList": $scope.salaryLockPayableGLData,
                            "disbursementAdviceId": $scope.voucher.DisbursementAdviceId,
                            "employeeListNew": $scope.EmployeeListNew
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            $scope.saveBtnDisable = false;
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

    $scope.voucherId = null;
    $scope.confirmPost = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function (voucherId) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "voucherId": voucherId
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
        $scope.Action = "Park";
        $scope.voucher = {};
        $scope.voucher.PaymentMode ='';
        $scope.voucher.EmployeeId = null;
        $scope.getCboVoucherTypeSalaryDisbursementList();
        $scope.voucher.Active = true;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.DocRefNo = null;
        $scope.employeeDisbursementDataList = [];
        $scope.salaryLockPayableGLData = [];
        $scope.EmployeeListNew = [];
        $scope.saveBtnDisable = false;

    }

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
            $scope.voucher.CashMasterId = null;
            $scope.voucher.CashCurrencyId = null;
            $scope.voucher.CashName = null;
            $scope.getBank();
        }
        else {
            $scope.voucher.PaymentSource = $scope.voucher.PaymentMode;
            $scope.voucher.BankId = null;
            $scope.voucher.BankMasterId = null;
            $scope.voucher.BankCurrencyId = null;
            $scope.voucher.AccountTitle = null;
            $scope.voucher.BankName = null;
        }
    }
    $scope.changeBank = function () {
        if ($scope.voucher.PaymentMode == 'Bank') {
            $scope.voucher.PaymentSource = $scope.voucher.PaymentMode;
        }
    }
    $scope.bankSearchByList = [
        {
            "name": "Bank",
            "value": "BankName"
        },
        {
            "name": "Bank Branch",
            "value": "BankBranchName"
        },
        {
            "name": "Account Type",
            "value": "BankAccountTypeName"
        },
        {
            "name": "Account Number",
            "value": "AccountNumber"
        },
        {
            "name": "Currency",
            "value": "CurrencyCode"
        }
    ];
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

    $scope.masterList = [];
    $scope.getMasterData = function () {
        $scope.masterList = [];
        $http.get("Accounts/SalaryDisbursement/GetDisbursementAdviceData")
            .then(
                function successCallback(response) {
                    $scope.masterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#DisbursementAdvicepopUp')).modal('show');
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#DisbursementAdvicepopUp')).modal('hide');
    }

    $scope.SelectMaster = function (x) {
        var data = x.data;
        $scope.voucher.DisbursementAdviceId = data.Id;
        $scope.voucher.YearNo = data.YearNo;
        $scope.voucher.MonthNo = data.MonthNo;
        $scope.voucher.MonthName = data.MonthName;
        $scope.voucher.PaymentMode = data.PaymentMode;
        $scope.voucher.PaymentSource = data.PaymentMode;

        $scope.GetemployeeDisbursement();
        $scope.getSalaryLockPayableGL();
        
        angular.element(document.querySelector('#DisbursementAdvicepopUp')).modal('hide');
    };
    $scope.EmployeeListNew = [];
    $scope.pushInTempListforProcess = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempListforConfirm($scope.EmployeeListNew, data.EmpSystemId) === false) {
                    $scope.EmployeeListNew.push(data);
                }
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.EmployeeListNew); i++) {
                        if ($scope.EmployeeListNew[i].EmpSystemId === data.EmpSystemId) {
                            $scope.EmployeeListNew.splice(i, 1);
                            break;
                        }
                    }

                    $scope.EmployeeListNew.push(data);
                }
            }
            else {
                for (var t = 0; t < baseService.arrayLength($scope.EmployeeListNew); t++) {
                    if ($scope.EmployeeListNew[t].EmpSystemId === data.EmpSystemId) {
                        $scope.EmployeeListNew.splice(t, 1);
                        break;
                    }
                }
            }
            $scope.getSalaryLockPayableGL();
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }

    function checkExistTempListforConfirm(list, empSystemId) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].EmpSystemId === empSystemId) {
                return true;
            }
        }
        return false;
    }
    $scope.refreshTemplateEmployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmployee });
    };

    function CheckBoxSelectAllEmployee(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#empInfoGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.employeeDisbursementDataList.length; i++) {
                $scope.employeeDisbursementDataList[i].isSelected = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#empInfoGrid").data("ejGrid");
        gridObj.refreshContent();
        $scope.EmployeeListNew = [];
        for (var i = 0; i < $scope.employeeDisbursementDataList.length; i++) {
            if ($scope.employeeDisbursementDataList[i].isSelected) {
                $scope.EmployeeListNew.push($scope.employeeDisbursementDataList[i]);
            }
        }
        $scope.getSalaryLockPayableGL();
    };

    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.XlsSalaryDisbursement = function () {
        var dataList = [];
        var newDataList = [];
        var g = $("#empInfoGrid").data("ejGrid");
        dataList = g.getFilteredRecords();
        var obj = {};

        if (dataList.length == 0) {

            dataList = $scope.employeeDisbursementDataList;
        }

        for (let i = 0; i < dataList.length; i++) {
            obj.DisbursementAdviceId = dataList[i].DisbursementAdviceId;
            obj.AdviceDate = dataList[i].AdviceDate;
            obj.Remarks = dataList[i].Remarks;
            obj.AddedBy = dataList[i].AddedBy;
            obj.Year = dataList[i].YearNo;
            obj.Month = dataList[i].MonthName;
            obj.EmployeeCode = dataList[i].EmployeeCode;
            obj.EmployeeName = dataList[i].EmployeeName;
            obj.Designation = dataList[i].Designation;
            obj.Department = dataList[i].Department;
            obj.EmployeeCategory = dataList[i].EmployeeCategory;
            obj.Plant = dataList[i].Plant;
            obj.Section = dataList[i].Section;
            obj.SubSection = dataList[i].SubSection;
            obj.Unit = dataList[i].Unit;
            obj.DOJ = dataList[i].DOJ;
            obj.DOS = dataList[i].DOS;
            obj.CurrentMonthEmployeeStatus = dataList[i].CurrentMonthEmployeeStatus;
            obj.EmployeeStatus = dataList[i].EmployeeStatus;
            obj.PaymentMode = dataList[i].PaymentMode;
            obj.BankName = dataList[i].BankName;
            obj.BankAccNo = dataList[i].BankAccNo;
            obj.IFSCCode = dataList[i].IFSCCode;
            obj.VoucherNo = dataList[i].VoucherNo;
            obj.PayableVoucherNo = dataList[i].PayableVoucherNo;
            obj.NetPayment = dataList[i].Amount;
            newDataList.push(obj);
            obj = {};
        }
        $scope.fileName = 'SalaryDisbursement';
        $http({
            method: "POST",
            url: $scope.exportgriddataUrl,
            data: {
                'data': newDataList,
                'reportFileName': $scope.fileName,

            },

            dataType: 'JSON',

        })
            .then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };

    $scope.summaryfileName = "Salary Disbursement.xlsx"
    $scope.XlsSalaryDisbursementVoucherWiseReport = function (PayableVoucherId) {
        var parameters = {
            'voucherId': PayableVoucherId
        };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Accounts/SalaryDisbursement/GetEmployeeSalaryDisbursementVoucherWise',
            data: parameters
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.summaryfileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
    };

    //Start Bonus Disbursement Posting
    $scope.getBonusListUrl = $scope.path + "GetBonusDisbursementVoucherList";
    $scope.saveBonusUrl = $scope.path + "SaveBonusDisbursementPosting";
    $scope.postBonusUrl = $scope.path + "PostBonusDisbursement";
    $scope.deleteBonusUrl = "Accounts/SalaryDisbursement/DeleteBonusDisbursementVoucher";

    $scope.voucherBonus = {
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
        IsMaternity: false,
        BonusDisbursementAdviceId: null,
        FromDate: null,
        ToDate: null


    };

    $scope.bonusParameters = {
        limit: 10,
        offset: 0,
        order: 'DESC',
        sort: 'PostingDate DESC, VoucherNo',
        searchBy: 'VoucherNo',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getBonusData = function (pageno) {
        baseService.paginationBase($scope.getBonusListUrl, pageno, $scope.bonusParameters)
            .then(function (result) {
                $scope.bonusPayables = result.Rows;
                $scope.bonusParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getBonusData();
    $scope.getCboVoucherTypeBonusDisbursementList = function () {
        cboService.getCboVoucherTypeBonusDisbursementList(function (result) {
            $scope.voucherTypeListBonus = result;
            if ($scope.voucherTypeListBonus.length === 1) {
                $scope.voucherBonus.VoucherTypeId = $scope.voucherTypeListBonus[0].Value;
                $scope.voucherBonus.PostingDate = $filter("dateFiltering")($scope.voucherTypeListBonus[0].LastPostingDate);
            }
        });
    }
    $scope.getCboVoucherTypeBonusDisbursementList();

    $scope.bonusmasterList = [];
    $scope.getBonusMasterData = function () {
        $scope.bonusmasterList = [];
        $http.get("Accounts/SalaryDisbursement/GetBonusDisbursementAdviceData")
            .then(
                function successCallback(response) {
                    $scope.bonusmasterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#BonusDisbursementAdvicepopUp')).modal('show');
    };

    $scope.closeBonusPopUp = function () {
        angular.element(document.querySelector('#BonusDisbursementAdvicepopUp')).modal('hide');
    }

    $scope.SelectBonusMaster = function (x) {
        var data = x.data;
        $scope.voucherBonus.BonusDisbursementAdviceId = data.Id;
        $scope.voucherBonus.FromDate = data.FromDate;
        $scope.voucherBonus.ToDate = data.ToDate;
        $scope.voucherBonus.PaymentMode = data.PaymentMode;
        $scope.voucherBonus.PaymentSource = data.PaymentMode;

        $scope.GetemployeeBonusDisbursement();
        $scope.getBonusLockPayableGL();

        angular.element(document.querySelector('#BonusDisbursementAdvicepopUp')).modal('hide');
    };
    $scope.employeeBonusDisbursementDataList = [];
    $scope.EmployeeBonusListNew = [];
    $scope.GetemployeeBonusDisbursement = function () {
        $scope.employeeBonusDisbursementDataList = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Accounts/SalaryDisbursement/GetEmployeeBonusDisbursementDataList?disbursementAdviceId=' + $scope.voucherBonus.BonusDisbursementAdviceId,

        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.empBonusGrid = true;
                $scope.employeeBonusDisbursementDataList = response.data;
                $scope.EmployeeBonusListNew = response.data;
            }
            else {
                ShowResult("No Data Found", 'failure');
                $scope.empBonusGrid = false;
            }
        });
    };
    $scope.bonusLockPayableGLData = [];
    $scope.getBonusLockPayableGL = function () {

        $scope.bonusLockPayableGLData = [];
        $http({
            method: "POST",
            url: "Accounts/SalaryDisbursement/GetDirectBonusPayableDisbursementDataList",
            data: { 'disbursementAdviceId': $scope.voucherBonus.BonusDisbursementAdviceId, 'employeeListNew': $scope.EmployeeBonusListNew },
            dataType: 'JSON'
            , contentType: "application/json charset=utf-8"
        }).then(function successCallback(response) {
            $scope.bonusLockPayableGLData = response.data;
        });
    };
    
    $scope.pushInTempListforBonusProcess = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempListforBonusConfirm($scope.EmployeeBonusListNew, data.Id) === false) {
                    $scope.EmployeeBonusListNew.push(data);
                }
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.EmployeeBonusListNew); i++) {
                        if ($scope.EmployeeBonusListNew[i].Id === data.Id) {
                            $scope.EmployeeBonusListNew.splice(i, 1);
                            break;
                        }
                    }

                    $scope.EmployeeBonusListNew.push(data);
                }
            }
            else {
                for (var t = 0; t < baseService.arrayLength($scope.EmployeeBonusListNew); t++) {
                    if ($scope.EmployeeBonusListNew[t].Id === data.Id) {
                        $scope.EmployeeBonusListNew.splice(t, 1);
                        break;
                    }
                }
            }
            $scope.getBonusLockPayableGL();
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }

    function checkExistTempListforBonusConfirm(list, Id) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }
    $scope.refreshTemplateEmployeeBonus = function (args) {
        $("#headchkBonus").ejCheckBox({ "change": CheckBoxSelectAllEmployeeBonus });
    };

    function CheckBoxSelectAllEmployeeBonus(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#empInfoBonusGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.employeeBonusDisbursementDataList.length; i++) {
                $scope.employeeBonusDisbursementDataList[i].isSelected = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#empInfoBonusGrid").data("ejGrid");
        gridObj.refreshContent();
        $scope.EmployeeBonusListNew = [];
        for (var i = 0; i < $scope.employeeBonusDisbursementDataList.length; i++) {
            if ($scope.employeeBonusDisbursementDataList[i].isSelected) {
                $scope.EmployeeBonusListNew.push($scope.employeeBonusDisbursementDataList[i]);
            }
        }
        $scope.getBonusLockPayableGL();
    };

    $scope.XlsBonusDisbursement = function () {
        var dataList = [];
        var newDataList = [];
        var g = $("#empInfoBonusGrid").data("ejGrid");
        dataList = g.getFilteredRecords();
        var obj = {};

        if (dataList.length == 0) {

            dataList = $scope.employeeBonusDisbursementDataList;
        }

        for (let i = 0; i < dataList.length; i++) {
            obj.DisbursementAdviceId = dataList[i].DisbursementAdviceId;
            obj.AdviceDate = dataList[i].AdviceDate;
            obj.Remarks = dataList[i].Remarks;
            obj.AddedBy = dataList[i].AddedBy;
            obj.Year = dataList[i].YearNo;
            obj.Month = dataList[i].MonthName;
            obj.EmployeeCode = dataList[i].EmployeeCode;
            obj.EmployeeName = dataList[i].EmployeeName;
            obj.Designation = dataList[i].Designation;
            obj.Department = dataList[i].Department;
            obj.EmployeeCategory = dataList[i].EmployeeCategory;
            obj.Plant = dataList[i].Plant;
            obj.Section = dataList[i].Section;
            obj.SubSection = dataList[i].SubSection;
            obj.Unit = dataList[i].Unit;
            obj.DOJ = dataList[i].DOJ;
            obj.DOS = dataList[i].DOS;
            obj.CurrentMonthEmployeeStatus = dataList[i].CurrentMonthEmployeeStatus;
            obj.EmployeeStatus = dataList[i].EmployeeStatus;
            obj.PaymentMode = dataList[i].PaymentMode;
            obj.BankName = dataList[i].BankName;
            obj.BankAccNo = dataList[i].BankAccNo;
            obj.IFSCCode = dataList[i].IFSCCode;
            obj.VoucherNo = dataList[i].VoucherNo;
            obj.PayableVoucherNo = dataList[i].PayableVoucherNo;
            obj.BonusPayment = dataList[i].Amount;
            newDataList.push(obj);
            obj = {};
        }
        $scope.fileName = 'BonusDisbursement';
        $http({
            method: "POST",
            url: $scope.exportgriddataUrl,
            data: {
                'data': newDataList,
                'reportFileName': $scope.fileName,

            },

            dataType: 'JSON',

        })
            .then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };
    
    $scope.XlsBonusDisbursementVoucherWiseReport = function (PayableVoucherId) {
        $scope.summaryfileName = "Bonus Disbursement.xlsx"
        var parameters = {
            'voucherId': PayableVoucherId
        };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Accounts/SalaryDisbursement/GetEmployeeBonusDisbursementVoucherWise',
            data: parameters
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.summaryfileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
    };
    $scope.SaveBonus = function () {
        if ($scope.EmployeeBonusListNew.length === 0) {
            ShowResult("Please select Employee!", "failure");
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.voucherBonus.PaymentMode)) {
            ShowResult("Please select Payment Mode!", "failure");
            return true;
        }
        if ($scope.voucherBonus.PaymentMode === "Bank") {
            if ($scope.voucherBonus.BankName === "" || baseService.isUndefinedOrNull($scope.voucherBonus.BankMasterId)) {
                ShowResult("Please select Bank!", "failure");
                return true;
            }
        }
        if ($scope.voucherBonus.PaymentMode === "Cash") {
            if ($scope.voucherBonus.CashName === "" || baseService.isUndefinedOrNull($scope.voucherBonus.CashMasterId)) {
                ShowResult("Please select Cash!", "failure");
                return true;
            }
        }

        $scope.$broadcast("show-errors-check-validity");
        $scope.saveBtnDisable = true;
        try {
            if ($scope.form0.$valid) {
                if ($scope.Action === "Park") {
                    $http({
                        method: "POST",
                        url: $scope.saveBonusUrl,
                        data: {
                            "voucherVM": $scope.voucherBonus,
                            "fromDate": $scope.voucherBonus.FromDate,
                            "toDate": $scope.voucherBonus.ToDate,
                            "pMode": $scope.voucherBonus.PaymentMode,
                            "directJVList": $scope.bonusLockPayableGLData,
                            "disbursementAdviceId": $scope.voucherBonus.BonusDisbursementAdviceId,
                            "employeeListNew": $scope.EmployeeBonusListNew
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            $scope.saveBtnDisable = false;
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.getBonusData();
                            $scope.ClearBonus();
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

    $scope.confirmPostBonus = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostBonusPopUp")).modal("show");
    };

    $scope.postBonus = function (voucherId) {
        $http({
            method: "POST",
            url: $scope.postBonusUrl,
            data: {
                "voucherId": voucherId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getBonusData();
                $scope.ClearBonus();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.ClearBonus = function () {
        ClearBonusFields();
    };

    function ClearBonusFields() {
        $scope.Action = "Park";
        $scope.voucherBonus = {};
        $scope.voucherBonus.PaymentMode = '';
        $scope.getCboVoucherTypeBonusDisbursementList();
        $scope.voucherBonus.Active = true;
        $scope.voucherBonus.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucherBonus.DocRefNo = null;
        $scope.employeeBonusDisbursementDataList = [];
        $scope.bonusLockPayableGLData = [];
        $scope.EmployeeBonusListNew = [];
        $scope.saveBtnDisable = false;

    }
    $scope.deleteBonusDisbursement = function (voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteBonusUrl,
            data: {
                "voucherId": voucherId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getBonusData();
                $scope.ClearBonus();
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.confirmDeleteBonus = function (data) {
        $scope.voucherId = data.PayableVoucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeleteBonusPopUp")).modal("show");
    };
    $scope.showBankPopUpBonus = function () {
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
        angular.element(document.querySelector("#bankPopUpBonus")).modal("show");
    };

    $scope.showCashPopUpBonus = function (index, entityId) {
        $scope.getCashList = function (pageno) {
            baseService.paginationBase("banks/cashmaster/GetCashMasterVoucher?id=&entityId=" + entityId, pageno, $scope.cashParameters)
                .then(function (result) {
                    $scope.cashList = result.Rows;
                    $scope.cashParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getCashList();
        angular.element(document.querySelector("#cashPopUpBonus")).modal("show");
    };
    $scope.closeCashPopUpBonus = function () {
        if ($scope.cashIndex !== -1) {
            var cash = $scope.cashList[$scope.cashIndex];
            if (baseService.isUndefinedOrNull(cash.GLGeneralInfoId)) {
                ShowResult("Cash GL not found!", "failure", "cashPopUpBonus");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(cash.BudgetMasterId)) {
                ShowResult("Cash Budget not found!", "failure", "cashPopUpBonus");
                return;
            }
            else if (baseService.isUndefinedOrNull(cash.CurrencyId)) {
                ShowResult("Cash Transaction Currency not found!", "failure", "cashPopUpBonus");
                return;
            }
            else {
                $scope.voucherBonus.CashMasterId = cash.Id;
                $scope.voucherBonus.CashCurrencyId = cash.CurrencyId;
                $scope.voucherBonus.CashName = cash.CashName;
                $scope.voucherBonus.GLGeneralInfoId = cash.GLGeneralInfoId;
                $scope.voucherBonus.GLGeneralInfoName = cash.GLItem;
                $scope.voucherBonus.BudgetName = cash.BudgetName;
                $scope.voucherBonus.BudgetMasterId = cash.BudgetMasterId;
                $scope.voucherBonus.ActivityId = cash.ActivityId;
                $scope.voucherBonus.ActivityName = cash.ActivityName;
                $scope.checkCashAmountBonus();
            }
        }
        $scope.hideCashPopUpBonus();
    };
    $scope.hideCashPopUpBonus = function () {
        angular.element(document.querySelector("#cashPopUpBonus")).modal("hide");
        $scope.cashIndex = -1;
        $scope.cashSelected = null;
    };
    $scope.closeBankPopUpBonus = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankmasterList[$scope.bankIndex];

            $scope.voucherBonus.AccountTitle = bank.AccountTitle;
            $scope.voucherBonus.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
            $scope.voucherBonus.BankMasterId = bank.BankMasterId;
            $scope.voucherBonus.BankCurrencyId = bank.CurrencyId;

            $scope.voucherBonus.GLGeneralInfoId = bank.GLGeneralInfoId;
            $scope.voucherBonus.GLGeneralInfoName = bank.GLGeneralInfoName;
            $scope.voucherBonus.BudgetMasterId = bank.BudgetMasterId;
            $scope.voucherBonus.BudgetName = bank.BudgetName;
            $scope.voucherBonus.ActivityId = bank.ActivityId;
            $scope.voucherBonus.ActivityName = bank.ActivityName;
            $scope.checkBankAmount();
        }
        $scope.hideBankPopUpBonus();
    };
    $scope.selectBankPopUpBonus = function (index, id) {
        $scope.bankIndex = index;
    };

    $scope.hideBankPopUpBonus = function () {
        angular.element(document.querySelector("#bankPopUpBonus")).modal("hide");
        $scope.bankIndex = -1;
        $scope.bankSelected = null;
    };
    $scope.checkBankAmountBonus = function () {
        if (!baseService.isUndefinedOrNull($scope.voucherBonus.BankCurrencyId)) {
            if ($scope.voucherBonus.BankCurrencyId !== $scope.companyCurrencyId) {
                $scope.isBankAmount = true;
                $scope.voucherBonus.BankAmount = 0;
            }
            else {
                $scope.isBankAmount = false;
                $scope.voucherBonus.BankAmount = 0;
            }
        }
    };

    $scope.checkCashAmountBonus = function () {
        if (!baseService.isUndefinedOrNull($scope.voucherBonus.CashCurrencyId)) {
            if ($scope.voucherBonus.CashCurrencyId !== $scope.companyCurrencyId) {
                $scope.isBankAmount = true;
                $scope.voucherBonus.BankAmount = 0;
            }
            else {
                $scope.isBankAmount = false;
                $scope.voucherBonus.BankAmount = 0;
            }
        }
    };
    //End Bonus Disbursement Posting

}