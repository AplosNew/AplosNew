"use strict";
bonusDisbursementPostController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function bonusDisbursementPostController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Bonus Disbursement";
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
    $scope.getListUrl = $scope.path + "GetBonusDisbursementVoucherList";
    $scope.saveUrl = $scope.path + "SaveBonusDisbursementPosting";
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
                if ($scope.Action === "Save") {
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
        $http.get("Accounts/SalaryDisbursement/GetBonusDisbursementAdviceData")
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

}