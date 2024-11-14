"use strict";
salaryPayableController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function salaryPayableController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Salary Payable";
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
    $scope.getListUrl = $scope.path + "GetSalaryPayableVoucherList";
    $scope.saveUrl = $scope.path + "ParkSalaryPayable";
    $scope.updateUrl = $scope.path + "UpdateEmployeePayable";
    $scope.deleteUrl = $scope.path + "delete/";
    $scope.postUrl = $scope.path + "PostSalaryPayable";

    $scope.downloadgriddataUrl = 'GridReports/Download';


    $scope.hideSource = true;
    $scope.isWriteOff = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    baseService.init($scope.getListUrl, null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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
            "name": "Doc No",
            "value": "DocRefNo"
        },
        {
            "name": "Month",
            "value": "Month"
        },
        {
            "name": "Year",
            "value": "YearNo"
        },
        {
            "name": "Entity",
            "value": "Entity"
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
        BankGLGeneralInfoId: null,
        IsActive: true,
        IsSeperated: false,
        IsMaternity: false,
        salaryProcessId: null,
        payGroupListSelected: null,
        parameters: null
       // IsDownloard: false

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
    $scope.salaryLockPayableData = [];
    $scope.salaryLockPayableList = [];
    $scope.salaryLockDirectTakeAwayNetPayAmount = null;
    $scope.getSalaryLockPayable = function () {
        if (angular.isUndefinedOrNull($scope.voucher.YearNo)) {
            ShowResult("Select Year", 'failure');
            return;
        }
        if (angular.isUndefinedOrNull($scope.voucher.MonthNo)) {
            ShowResult("Select Month", 'failure');
            return;
        }
        if (angular.isUndefinedOrNull($scope.voucher.EntityId)) {
            ShowResult("Select Entity", 'failure');
            return;
        }
        $scope.salaryLockPayableData = [];
        //$scope.IsDownloard = true;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Accounts/SalaryDisbursement/GetSalaryLockDataList?yearNo=' + $scope.voucher.YearNo + '&monthNo=' + $scope.voucher.MonthNo +
                '&employeeId=' + $scope.voucher.EmployeeId + '&isActive=' + $scope.voucher.IsActive + '&isSeperated=' + $scope.voucher.IsSeperated + '&isMaternity=' + $scope.voucher.IsMaternity + '&entityId=' + $scope.voucher.EntityId,
        }).then(function successCallback(response) {
            $scope.salaryLockPayableList = response.data;
           //$scope.IsDownloard = true;

            for (var i = 0; i < $scope.salaryLockPayableList.length; i++) {
                if ($scope.salaryLockPayableList[i].HeadHeadFilter == 'NetPay') {
                    $scope.salaryLockDirectTakeAwayNetPayAmount = $scope.salaryLockPayableList[i].DisbusmentAmount;
                }
                else {
                    $scope.salaryLockPayableData.push($scope.salaryLockPayableList[i]);
                }
            }
            
            $scope.getSalaryLockPayableGL();
            $scope.getSalaryLockDirectCTCPayable();
           
        });
    };

    $scope.salaryLockDirectCTCPayableData = [];
    $scope.getSalaryLockDirectCTCPayable = function () {
        $scope.salaryLockDirectCTCPayableData = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Accounts/SalaryDisbursement/GetSalaryLockCTCDataList?yearNo=' + $scope.voucher.YearNo + '&monthNo=' + $scope.voucher.MonthNo + '&employeeId='
                + $scope.voucher.EmployeeId + '&isActive=' + $scope.voucher.IsActive + '&isSeperated=' + $scope.voucher.IsSeperated + '&isMaternity=' + $scope.voucher.IsMaternity + '&entityId=' + $scope.voucher.EntityId,
        }).then(function successCallback(response) {
            $scope.salaryLockDirectCTCPayableData = response.data;
            $scope.getSalaryLockInDirectTakeAwayPayable();
        });
    };

    $scope.salaryLockInDirectTakeAwayPayableData = [];
    $scope.salaryLockInDirectTakeAwayPayableList = [];
    $scope.salaryLockInDirectTakeAwayNetPayAmount = null;
    $scope.getSalaryLockInDirectTakeAwayPayable = function () {
        $scope.salaryLockInDirectTakeAwayPayableData = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Accounts/SalaryDisbursement/GetSalaryLockInDirectTakeAwayDataList?yearNo=' + $scope.voucher.YearNo + '&monthNo=' + $scope.voucher.MonthNo + '&employeeId='
                + $scope.voucher.EmployeeId + '&isActive=' + $scope.voucher.IsActive + '&isSeperated=' + $scope.voucher.IsSeperated + '&isMaternity=' + $scope.voucher.IsMaternity + '&entityId=' + $scope.voucher.EntityId,
        }).then(function successCallback(response) {
            $scope.salaryLockInDirectTakeAwayPayableList = response.data;

            for (var i = 0; i < $scope.salaryLockInDirectTakeAwayPayableList.length; i++) {
                if ($scope.salaryLockInDirectTakeAwayPayableList[i].HeadHeadFilter == 'NetPay') {
                    $scope.salaryLockInDirectTakeAwayNetPayAmount = $scope.salaryLockInDirectTakeAwayPayableList[i].DisbusmentAmount;
                }
                else {
                    $scope.salaryLockInDirectTakeAwayPayableData.push($scope.salaryLockInDirectTakeAwayPayableList[i]);
                }
            }

            $scope.getSalaryLockInDirectCTCPayable();
        });
    };

    $scope.salaryLockInDirectCTCPayableData = [];
    $scope.getSalaryLockInDirectCTCPayable = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Accounts/SalaryDisbursement/GetSalaryLockInDirectCTCDataList?yearNo=' + $scope.voucher.YearNo + '&monthNo=' + $scope.voucher.MonthNo + '&employeeId='
                + $scope.voucher.EmployeeId + '&isActive=' + $scope.voucher.IsActive + '&isSeperated=' + $scope.voucher.IsSeperated + '&isMaternity=' + $scope.voucher.IsMaternity + '&entityId=' + $scope.voucher.EntityId,
        }).then(function successCallback(response) {
            $scope.salaryLockInDirectCTCPayableData = response.data;
            $scope.getsalaryHeadGLWithCoa();
        });
    };


    $scope.salaryLockPayableGLData = [];
    $scope.DirectDifferenceDrAmount = 0;
    $scope.DirectDifferenceCrAmount = 0;
    $scope.getSalaryLockPayableGL = function () {
        $scope.DirectDifferenceDrAmount = 0;
        $scope.DirectDifferenceCrAmount = 0;
        $scope.salaryLockPayableGLData = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Accounts/SalaryDisbursement/GetSalaryLockDataGLList?yearNo=' + $scope.voucher.YearNo + '&monthNo=' + $scope.voucher.MonthNo + '&employeeId='
                + $scope.voucher.EmployeeId + '&isActive=' + $scope.voucher.IsActive + '&isSeperated=' + $scope.voucher.IsSeperated + '&isMaternity=' + $scope.voucher.IsMaternity + '&entityId=' + $scope.voucher.EntityId,
        }).then(function successCallback(response) {
            $scope.salaryLockPayableGLData = response.data;

            $scope.DirectDifferenceAmount = (Math.round($filter("sumByKey")($filter("filter")($scope.salaryLockPayableGLData), "DrAmount") * 100 + Number.EPSILON) / 100) -
                (Math.round($filter("sumByKey")($filter("filter")($scope.salaryLockPayableGLData), "CrAmount") * 100 + Number.EPSILON) / 100);
            if ($scope.DirectDifferenceAmount > 0) {
                $scope.DirectDifferenceCrAmount = $scope.DirectDifferenceAmount;
            } else {
                $scope.DirectDifferenceDrAmount = ($scope.DirectDifferenceAmount*-1);
            }
            $scope.getSalaryLockInDirectPayableGL();
        });
    };

    $scope.salaryLockInDirectPayableGLData = [];
    $scope.InDirectDifferenceDrAmount = 0;
    $scope.InDirectDifferenceCrAmount = 0;
    $scope.getSalaryLockInDirectPayableGL = function () {
        $scope.InDirectDifferenceDrAmount = 0;
        $scope.InDirectDifferenceCrAmount = 0;
        $scope.salaryLockInDirectPayableGLData = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Accounts/SalaryDisbursement/GetSalaryLockInDirectDataGLList?yearNo=' + $scope.voucher.YearNo + '&monthNo=' + $scope.voucher.MonthNo + '&employeeId='
                + $scope.voucher.EmployeeId + '&isActive=' + $scope.voucher.IsActive + '&isSeperated=' + $scope.voucher.IsSeperated + '&isMaternity=' + $scope.voucher.IsMaternity + '&entityId=' + $scope.voucher.EntityId,
        }).then(function successCallback(response) {
            $scope.salaryLockInDirectPayableGLData = response.data;
            $scope.InDirectDifferenceAmount = (Math.round($filter("sumByKey")($filter("filter")($scope.salaryLockInDirectPayableGLData), "DrAmount") * 100 + Number.EPSILON) / 100) -
                (Math.round($filter("sumByKey")($filter("filter")($scope.salaryLockInDirectPayableGLData), "CrAmount") * 100 + Number.EPSILON) / 100);

            if ($scope.InDirectDifferenceAmount > 0) {
                $scope.InDirectDifferenceCrAmount = $scope.InDirectDifferenceAmount;
            } else {
                $scope.InDirectDifferenceDrAmount = ($scope.InDirectDifferenceAmount * -1);
            }
            $scope.GetDirectSalaryLockSalarySheetData();
            $scope.GetInDirectSalaryLockSalarySheetData();
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



    $scope.getCboVoucherTypeSalaryPayableList = function () {
        cboService.getCboVoucherTypeSalaryPayableList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
            }
        });
    }
    $scope.getCboVoucherTypeSalaryPayableList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
    };
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });
    $scope.entityChange = function (id) {
        var entityrowdata = $filter("filter")($scope.entityList, { Value: id });
        $scope.voucher.PlantId = entityrowdata[0].PlantId;
    };

    $scope.validation = function () {
        if ($scope.salaryLockPayableGLDataList.length == 0 && $scope.salaryLockInDirectPayableGLDataList.length == 0) {
                ShowResult("Direct and Indirect JV is missing!.", "failure");
                return true;
            }
        return false;
    };

    $scope.Save = function () {
        $scope.DiffrenceAmountProcess();
        $scope.$broadcast("show-errors-check-validity");
        try {
            if ($scope.form0.$valid && !$scope.validation()) {
                if ($scope.Action === "Save") {
                    $http({
                        method: "POST",
                        url: $scope.saveUrl,
                        data: {
                            "voucherVM": $scope.voucher,
                            "yearNo": $scope.voucher.YearNo,
                            "monthNo": $scope.voucher.MonthNo,
                            "monthName": $scope.monthName,
                            "directJVList": $scope.salaryLockPayableGLDataList,
                            "inDirectJVList": $scope.salaryLockInDirectPayableGLDataList,
                            "directSalaryLockList": $scope.directSalaryLockSalarySheetDataList,
                            "indirectSalaryLockList": $scope.inDirectSalaryLockSalarySheetDataList
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
                "voucherId": payableId
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
        //$scope.voucher = {};
        $scope.voucher.EmployeeId = null;
        $scope.getCboVoucherTypeSalaryPayableList();
        $scope.voucher.Active = true;
        //$scope.voucher.IsActive = false;
        //$scope.voucher.IsMaternity = false;
        //$scope.voucher.IsSeperated = false;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.DocRefNo = null;
        $scope.voucher.EntityId = null;
        $scope.takeAwaylist = [];
        $scope.salaryLockPayableData = [];
        $scope.cTClist = [];
        $scope.salaryLockDirectCTCPayableData = [];
        $scope.DirectSalaryHeadGLCombineList = [];
        $scope.salaryLockPayableGLData = [];
        $scope.salaryLockInDirectTakeAwayPayableData = [];
        $scope.salaryLockInDirectCTCPayableData = [];
        $scope.inDirectSalaryHeadGLCombineList = [];
        $scope.salaryLockPayableData = [];
        $scope.expensesBookingDetailList = [];
        $scope.advanceTaxesList = [];
        $scope.salaryLockInDirectPayableGLData = [];
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

    $scope.selectsalaryHeadGLWithCombineList = [];
    $scope.inDirectSalaryHeadGLCombineList = [];
    $scope.DirectSalaryHeadGLCombineList = [];
    $scope.getsalaryHeadGLWithCoa = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Accounts/SalaryDisbursement/getlistwithcombine'
        }).then(function successCallback(response) {
            $scope.selectsalaryHeadGLWithCombineList = response.data;
            $scope.inDirectSalaryHeadGLCombineList = [];
            $scope.DirectSalaryHeadGLCombineList = [];
            $scope.DirectSalaryHeadGLCombineTakeAway();
            $scope.DirectSalaryHeadGLCombineCTC();
            $scope.InDirectSalaryHeadGLCombineTakeAway();
            $scope.InDirectSalaryHeadGLCombineCTC();
        });
    };

    $scope.DirectSalaryHeadGLCombineTakeAway = function () {
        for (var i = 0; i < $scope.selectsalaryHeadGLWithCombineList.length; i++) {
            var getRow = null;
            getRow = $filter("filter")($scope.salaryLockPayableData, { "SalaryHead": $scope.selectsalaryHeadGLWithCombineList[i].SalaryHead });
            if (getRow.length > 0) {
                $scope.selectsalaryHeadGLWithCombineList[i].HType = 'TakeAway';
                $scope.DirectSalaryHeadGLCombineList.push($scope.selectsalaryHeadGLWithCombineList[i])
            }

        }
    }
    $scope.DirectSalaryHeadGLCombineCTC = function () {
        for (var i = 0; i < $scope.selectsalaryHeadGLWithCombineList.length; i++) {
            var getRow = null;
            getRow = $filter("filter")($scope.salaryLockDirectCTCPayableData, { "SalaryHead": $scope.selectsalaryHeadGLWithCombineList[i].SalaryHead });
            if (getRow.length > 0) {
                $scope.selectsalaryHeadGLWithCombineList[i].HType = 'CTC';
                $scope.DirectSalaryHeadGLCombineList.push($scope.selectsalaryHeadGLWithCombineList[i])
            }

        }
    }



    $scope.InDirectSalaryHeadGLCombineTakeAway = function () {
        for (var i = 0; i < $scope.selectsalaryHeadGLWithCombineList.length; i++) {
            var getRow = null;
            getRow = $filter("filter")($scope.salaryLockInDirectTakeAwayPayableData, { "SalaryHead": $scope.selectsalaryHeadGLWithCombineList[i].SalaryHead });
            if (getRow.length > 0) {
                $scope.selectsalaryHeadGLWithCombineList[i].HType = 'TakeAway';
                $scope.inDirectSalaryHeadGLCombineList.push($scope.selectsalaryHeadGLWithCombineList[i])
            }

        }
    }
    $scope.InDirectSalaryHeadGLCombineCTC = function () {
        for (var i = 0; i < $scope.selectsalaryHeadGLWithCombineList.length; i++) {
            var getRow = null;
            getRow = $filter("filter")($scope.salaryLockInDirectCTCPayableData, { "SalaryHead": $scope.selectsalaryHeadGLWithCombineList[i].SalaryHead });
            if (getRow.length > 0) {
                $scope.selectsalaryHeadGLWithCombineList[i].HType = 'CTC';
                $scope.inDirectSalaryHeadGLCombineList.push($scope.selectsalaryHeadGLWithCombineList[i])
            }

        }
    }

    $scope.searchissueAUCglByList = [
        
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
            "name": "RefNo",
            "value": "RefNo"
        }
    ];

    $scope.issueAUCglListParameters = {
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
    $scope.issueAUCglList = [];
    $scope.GetRevenueExpensGLbudgetPopUp = function () {
       
        $scope.IssueAUCGLUrl = "Accounts/glitem/GetCurrentAssetRevenueExpenseGLBudget";
        baseService.setCurrentPage('issueAUCglList');
        $scope.GetIssueAUCGLData = function (pageno) {

            baseService.paginationBase($scope.IssueAUCGLUrl, pageno, $scope.issueAUCglListParameters)
                .then(function (result) {
                    $scope.issueAUCglList = result.Rows;
                    $scope.issueAUCglListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#RevenueExpensGLbudgetPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetIssueAUCGLData();
    };

    $scope.closeIssueAUCglListPopUp = function () {
        angular.element(document.querySelector("#RevenueExpensGLbudgetPopUp")).modal("hide");
    };

    $scope.setissueAUCglSelected = function (data) {
        $scope.DirectDifferenceBudgetName = data.BudgetName;
        $scope.DirectDifferenceBudgetMasterId = data.BudgetMasterId;
        $scope.DirectDifferenceGLGeneralInfoId = data.GLGeneralInfoId;
        $scope.DirectDifferenceGLGeneralInfoName = data.GLGeneralInfoName;
        $scope.getActivity(data.BudgetMasterId);

        //$scope.getActivityListWithCallBack(data.BudgetMasterId, function (result) { });
        $scope.closeIssueAUCglListPopUp();
        //$scope.issueJournalNewBudjetAdd($scope.inventoryIssueList[$scope.indexMB]);
    };
    $scope.activityList = [];
    $scope.getActivity = function (budgetMasterId) {
        cboService.getBudgetMasterActivityCbo(budgetMasterId, function (result) {
            //$scope.activityList = result;
            $scope.DirectDifferenceActivityId = result[0].ActivityId;
            $scope.DirectDifferenceActivityName = result[0].ActivityName;

            angular.forEach(result, function (item) {
                var ob = {
                    ActivityId: item.ActivityId,
                    ActivityName: item.ActivityName,
                    BudgetMasterId: item.BudgetMasterId
                }
                $scope.activityList.push(ob);
            })
        });

    };
    $scope.salaryLockPayableGLDataList = [];
    $scope.salaryLockInDirectPayableGLDataList = [];
    $scope.DiffrenceAmountProcess = function () {
        $scope.salaryLockPayableGLDataList = [];
        $scope.salaryLockInDirectPayableGLDataList = [];
        angular.forEach($scope.salaryLockPayableGLData, function (item) {
            $scope.salaryLockPayableGLDataList.push(item);
        })

        angular.forEach($scope.salaryLockInDirectPayableGLData, function (item) {
            $scope.salaryLockInDirectPayableGLDataList.push(item);
        })

        if (($scope.DirectDifferenceDrAmount + $scope.DirectDifferenceCrAmount) > 0) {

            var dirDiffDrAmountOBject = {
                GLGeneralInfoId: $scope.DirectDifferenceGLGeneralInfoId,
                BudgetMasterId: $scope.DirectDifferenceBudgetMasterId,
                ActivityId: $scope.DirectDifferenceActivityId,
                DrAmount: $scope.DirectDifferenceDrAmount,
                CrAmount: $scope.DirectDifferenceCrAmount,
            };
            $scope.salaryLockPayableGLDataList.push(dirDiffDrAmountOBject);
        }
        if (($scope.InDirectDifferenceDrAmount + $scope.InDirectDifferenceCrAmount) > 0) {

            var inDirDiffDrAmountOBject = {
                //GLGeneralInfoId: $scope.InDirectDifferenceGLGeneralInfoId,
                //BudgetMasterId: $scope.InDirectDifferenceBudgetMasterId,
                //ActivityId: $scope.InDirectDifferenceActivityId,

                GLGeneralInfoId: $scope.DirectDifferenceGLGeneralInfoId,
                BudgetMasterId: $scope.DirectDifferenceBudgetMasterId,
                ActivityId: $scope.DirectDifferenceActivityId,
                DrAmount: $scope.InDirectDifferenceDrAmount,
                CrAmount: $scope.InDirectDifferenceCrAmount,
            };
            $scope.salaryLockInDirectPayableGLDataList.push(inDirDiffDrAmountOBject);
        }
    }
        $scope.salaryLockSalarySheetDataList = [];
    $scope.inDirectSalaryLockSalarySheetDataList = [];

    $scope.GetDirectSalaryLockSalarySheetData = function () {
        $scope.directSalaryLockSalarySheetDataList = [];
        try {
           
            if (angular.isUndefinedOrNull($scope.voucher.MonthNo)) {
                ShowResult("Select Month", 'failure');
            }
            if (angular.isUndefinedOrNull($scope.voucher.YearNo)) {
                ShowResult("Select Year", 'failure');
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Accounts/SalaryDisbursement/GetDirectSalaryLockSalarySheetData',
                    data: {
                        'yearNo': $scope.voucher.YearNo,
                        'monthNo': $scope.voucher.MonthNo,
                        'isActive': $scope.voucher.IsActive,
                        'isSeperated': $scope.voucher.IsSeperated,
                        'isMaternity': $scope.voucher.IsMaternity,
                        'entityId': $scope.voucher.EntityId
                    }
                }).then(function successCallback(response) {
                    $scope.directSalaryLockSalarySheetDataList = response.data;
                    
                });
            }
            
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetInDirectSalaryLockSalarySheetData = function () {
        $scope.inDirectSalaryLockSalarySheetDataList = [];
        try {

            if (angular.isUndefinedOrNull($scope.voucher.MonthNo)) {
                ShowResult("Select Month", 'failure');
            }
            if (angular.isUndefinedOrNull($scope.voucher.YearNo)) {
                ShowResult("Select Year", 'failure');
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Accounts/SalaryDisbursement/GetInDirectSalaryLockSalarySheetData',
                    data: {
                        'yearNo': $scope.voucher.YearNo,
                        'monthNo': $scope.voucher.MonthNo,
                        'isActive': $scope.voucher.IsActive,
                        'isSeperated': $scope.voucher.IsSeperated,
                        'isMaternity': $scope.voucher.IsMaternity,
                        'entityId': $scope.voucher.EntityId
                    }
                }).then(function successCallback(response) {
                    $scope.inDirectSalaryLockSalarySheetDataList = response.data;

                });
            }

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



    //............salary payble report.............

    $scope.GetEmployeeSalaryProcessedReportSalaryLogWiseDirectSPayable = function () {
        try {
            //var parameters = [];
            //var gridObj = $("#empInfoGrid").ejGrid("instance");
            //var filteredRecords = gridObj.getFilteredRecords();
            //if ($scope.isManualFilter == true) {
            //    if (filteredRecords.length == 0) {
            //        filteredRecords = $scope.EmployeeListTemp;

            //    }
            //}
            //if (angular.isUndefinedOrNull(filteredRecords) === false) {
            //    if (filteredRecords.length > 0) {
            //        parameters = [];
            //        parameters.push({ "Key": "EmpSystemId", "Value": getString(filteredRecords, "EmpSystemId") });
            //    }
            //}
            //if (parameters.length === 0) {
            //    parameters.push({ "Key": "", "Value": "" });

            //}

            $http({
                method: 'POST',
                url: 'humanresource/PayrollReports/GetEmployeeSalaryProcessedReportSalLogWiseDirectInDirectSalaryPayable',
                data: {
                    'month': $scope.voucher.MonthNo,
                    'year': $scope.voucher.YearNo,
                    'salaryProcessId': $scope.voucher.salaryProcessId,
                    'payRollGroup': $scope.voucher.payGroupListSelected,
                    'parameters': $scope.voucher.parameters,
                    'isActive': $scope.voucher.IsActive,
                    'isSeperated': $scope.voucher.IsSeperated,
                    'isMaternity': $scope.voucher.IsMaternity,
                    'IsDirectInDirect': true
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

    $scope.GetEmployeeSalaryProcessedReportSalaryLogWiseInDirectSPayable = function () {
        try {
           
            $http({
                method: 'POST',
                url: 'humanresource/PayrollReports/GetEmployeeSalaryProcessedReportSalLogWiseDirectInDirectSalaryPayable',
                data: {
                    'month': $scope.voucher.MonthNo,
                    'year': $scope.voucher.YearNo,
                    'salaryProcessId': $scope.voucher.salaryProcessId,
                    'payRollGroup': $scope.voucher.payGroupListSelected,
                    'parameters': $scope.voucher.parameters,
                    'isActive': $scope.voucher.IsActive,
                    'isSeperated': $scope.voucher.IsSeperated,
                    'isMaternity': $scope.voucher.IsMaternity,
                    'IsDirectInDirect': false
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




    $scope.GetEmployeeSalaryProcessedReportSalaryLogWiseInVoucher = function (p) {
        try {
            $scope.month = p.MonthNo;
            $scope.year = p.YearNo;
            $scope.payableVoucherId = p.PayableVoucherId;

            $http({
                method: 'POST',
                url: 'Accounts/SalaryDisbursement/GetEmployeeSalaryProcessedReportSalaryLogWiseInVoucher',
                data: {
                    'month': $scope.month,
                    'year': $scope.year,
                    'salaryProcessId': $scope.voucher.salaryProcessId,
                    'payRollGroup': $scope.voucher.payGroupListSelected,
                    'parameters': $scope.voucher.parameters,
                    'isActive': $scope.voucher.IsActive,
                    'isSeperated': $scope.voucher.IsSeperated,
                    'isMaternity': $scope.voucher.IsMaternity,
                    'voucherId': $scope.payableVoucherId
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
    $scope.deleteUrl = "Accounts/SalaryDisbursement/DeleteSalaryPayable";

    $scope.deleteSalaryPayable = function (voucherId, monthNo, yearNo) {
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