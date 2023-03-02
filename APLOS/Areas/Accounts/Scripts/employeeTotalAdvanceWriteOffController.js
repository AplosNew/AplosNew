'use strict';
employeeTotalAdvanceWriteOffController.$inject = ['bankService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function employeeTotalAdvanceWriteOffController(bankService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = 'Employee Advanced Set-off';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.voucherDetailCurrency = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.advanceList = [];
    $scope.isWriteOff = true;
    $scope.isBankAmount = false;
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $scope.url = 'accounts/Advance';
    $scope.deleteUrl = $scope.url + "/DeleteEmployeeTotalAdvanceWriteOff";
    $scope.postUrl = $scope.url + '/PostEmployeeAdvanceWriteOff';
    $scope.voucherDetailList = [];
    baseService.init('accounts/Advance/GetEmployeeAdvanceWriteOffList', null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.advanceList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    cboService.getCboEmployeeTransactionType(function (result) {
        $scope.employeeTransactionTypeList = result;
        if ($scope.employeeTransactionTypeList.length === 1) {
            $scope.advance.EmployeeTransactionTypeId = $scope.employeeTransactionTypeList[0].Value;
            $scope.advance.AdvanceType = $scope.employeeTransactionTypeList[0].AdvanceType;
            $scope.setVisible($scope.employeeTransactionTypeList[0].AdvanceType);
        }
    });
    if ($scope.isWriteOff) {
        cboService.getCboParallelCurrency(function (result) {
            $scope.tranCurrencyList = result;
        });
    }
    else {
        cboService.getCboTransactionCurrencyByCompany('', function (result) {
            $scope.tranCurrencyList = result;
        });
    }

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, '', function (result) {
                $scope.entityList = result;
            });
    });

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
        },
        {
            "name": "Currency",
            "value": "Currency"
        }
    ];

    $scope.advance = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        EmployeeId: null,
        EmployeeName: null,
        PartyGLGeneralInfoId: null,
        GLGeneralInfoId: null,
        CurrencyId: null,
        CompanyCurrencyRate: 1,
        VoucherTypeId: null,
        PartyType: 'Employee',
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
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
        AdvanceAmount: null,
        SettlementType: 'SetOff',
        PaymentSource: 'Bank',
        CashMasterId: null,
        BankMasterId: null,
        JournalType: null
    };

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
        DocDate: $filter('date')(Date.now(), 'dd-MMM-yyyy'),
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

    $scope.getCboVoucherTypeEmployeeAdvanceWriteOffList = function () {
        cboService.getCboVoucherTypeEmployeeAdvanceWriteOffList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.advance.PostingDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
                $scope.advance.DocDate = $scope.advance.PostingDate;
            }
        });
    }
    $scope.getCboVoucherTypeEmployeeAdvanceWriteOffList();

    bankService.getBankMasterHouseBankCboList(function (result) {
        $scope.bankMasterList = result;
    });

    bankService.getCashMasterCboList(function (result) {
        $scope.cashMasterList = result;
    });

    $scope.GetEmployeeTransactionNo = function (employeeId) {
        $http({
            method: "GET",
            url: "accounts/expenseBooking/GetEmployeeTransactionNo?employeeId=" + employeeId
        }).then(function successCallback(response) {
            $scope.employeeTransactionNo = response.data;
            $scope.advance.DocRefNo = "EAW-" + $scope.employeeTransactionNo;
        });
    };

    $scope.GetCurrencyExchangeRateList = function () {
        $scope.currencyExchangeRate = [];
        if (!baseService.isUndefinedOrNull($scope.advance.PostingDate) && !baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.advance.PostingDate + "&currencyId=" + $scope.advance.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.advance.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };


    $('.datepicker').datepicker({
        format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
    });

    $scope.searchVendorInvoiceList = [
        {
            'name': 'PostingDate',
            'value': 'PostingDate'
        },
        {
            'name': 'VoucherType',
            'value': 'VoucherType'
        },
        {
            'name': 'Doc Date',
            'value': 'DocDate'
        },
        {
            'name': 'Doc Ref No',
            'value': 'DocRefNo'
        },
        {
            'name': 'Customer',
            'value': 'Party'
        },
        {
            'name': 'Currency',
            'value': 'Currency'
        }
    ];

    // #region ********Get CustomerInvoice************
    $scope.customerInvoice = [];
    $scope.Get = function (data) {
        $scope.advance = data;
        $scope.advance.DocDate = $filter('dateFiltering')($scope.advance.DocDate);
        $scope.advance.VoucherDate = $filter('dateFiltering')($scope.advance.VoucherDate);
        $scope.advance.PostingDate = $filter('dateFiltering')($scope.advance.PostingDate);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.Action = 'Update';

        $http({
            method: 'GET',
            url: 'accounts/Advance/GetEmployeeAdvanceWriteOffDetailList?voucherId=' + $scope.advance.VoucherId
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
        });
    };

    $scope.exchangeGainLossList = [];
    $http.get('accounts/ExchangeGainLoss/GetExchangeGainLoss')
        .then(function successCallback(response) {
            $scope.exchangeGainLossList = response.data;
        },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });

    $scope.checkAmount = function () {
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if ($scope.advance.AdvanceAmount < $scope.voucherDetailList[i].Amount) {
                $scope.voucherDetailList[i].Amount = $scope.advance.AdvanceAmount;
                ShowResult('Payable amount can not exceed Advance amount!', 'failure');
                return true;
            }
            if ($scope.voucherDetailList[i].Amount > $scope.voucherDetailList[i].Balance) {
                $scope.voucherDetailList[i].Amount = $scope.voucherDetailList[i].Balance;
                ShowResult(' Amount can not more than Balance!', 'failure');
                return true;
            }
        }
        return false;
    };

    $scope.postingDateMessage = '';
    $('#postingDate').datepicker().on('changeDate', function (ev) {
        $scope.advance.PostingDate = ev.date;
        if (new Date($scope.advance.PostingDate) > new Date()) {
            $scope.postingDateMessage = 'Posting date must be below or equal to current Date!';
            $scope.advance.PostingDate = '';
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
        }
        else if ($scope.advance.PostingDate > $scope.advance.DocDate) {
            $scope.postingDateMessage = 'Posting date must be below or equal to Doc Date!';
            $scope.advance.PostingDate = '';
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
        } else {
            $scope.postingDateMessage = '';
        }
    });

    $scope.dateMessage = '';
    $scope.checkDate = function () {
        if (new Date($scope.advance.DocDate) > new Date()) {
            $scope.dateMessage = 'Doc date must be below or equal to current Date ';
            return false;
        }
        else if ($scope.advance.DocDate > $scope.advance.VoucherDate) {
            $scope.dateMessage = 'Doc date must be below or equal to Voucher Date ';
            return false;
        } else {
            $scope.dateMessage = '';
            return true;
        }
    };

    $scope.VoucherDateMessage = '';
    $scope.checkVoucherDate = function () {
        if (new Date($scope.advance.VoucherDate) > new Date()) {
            $scope.VoucherDateMessage = 'Voucher date must be below or equal to current Date ';
            return false;
        }
        else if (new Date($scope.advance.VoucherDate) < new Date()) {
            $scope.VoucherDateMessage = '';
            return true;
        }
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = '';
        if (new Date($scope.advance.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = 'Doc date must be below or equal to current Date!';
        }
        else $scope.invalidDocDate = false;
        return manualValidation('div_DocDate', $scope.invalidDocDate, msg);
    };

    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = '';
        if (new Date($scope.advance.PostingDate) > new Date()) {
            msg = 'Posting date must be below or equal to current Date!';
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.advance.PostingDate) < new Date($scope.advance.DocDate)) {
            msg = 'Posting date must be below or equal to Doc Date!';
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.advancePostingDate) > new Date($scope.advance.PostingDate)) {
            msg = 'Posting date must be below or equal to Advance of ' + $scope.advanceDocRefNo;
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation('div_PostingDate', $scope.invalidPostingDate, msg);
    };

    $scope.invalidEntity = false;
    $scope.entityValidation = function () {
        $scope.invalidEntity = baseService.isUndefinedOrNull($scope.advance.EntityId);
        return manualValidation('div_entity', $scope.invalidEntity, 'Entity is required.');
    };

    $scope.removeRow = function (index) {
        $scope.voucherDetailList.splice(index, 1);
    };

    $scope.validation = function () {
        if ($scope.advance.SettlementType == 'SetOff' && $scope.voucherDetailList.length == 0) {
            ShowResult("Please select Payable!", "failure");
            return true;
        }
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if ($scope.advance.AdvanceAmount < $scope.voucherDetailList[i].Amount) {
                ShowResult('Payable amount can not exceed Advance amount!', 'failure');
                return true;
            }
        }
        return false;
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $scope.checkPostingDate();
        if ($scope.form0.$valid && !$scope.invalidDocDate && !$scope.invalidEntity && !$scope.invalidPostingDate && !$scope.validation()) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/Advance/InsertEmployeeTotalAdvanceWriteOff',
                    data: {
                        'voucherVM': $scope.advance,
                        'voucherDetailList': $scope.voucherDetailList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Clear();
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            //else if ($scope.Action === 'Update') {
            //    $http({
            //        method: 'POST',
            //        url: 'accounts/Advance/UpdateEmployeeAdvanceWriteOff',
            //        data: {
            //            'voucherVM': $scope.advance,
            //            'voucherDetailList': $scope.voucherDetailList
            //        },
            //        dataType: 'JSON'
            //    }).then(function successCallback(response) {
            //        if (response.data.Error === true) {
            //            ShowResult(response.data.Message, 'failure');
            //        }
            //        else {
            //            ShowResult(response.data.Message, 'success');
            //            $scope.Clear();
            //            $scope.getData();
            //        }
            //    }, function errorCallback(response) {
            //        ShowResult(response.status.Message, 'failure');
            //    });
            //}
            return true;
        }
    };

    $scope.advanceWriteOffId = null;
    $scope.confirmPost = function (advanceWriteOffId) {
        $scope.advanceWriteOffId = advanceWriteOffId;
        $scope.message_confirmation = 'Are you sure to Post?';
        angular.element(document.querySelector('#confirmPostPopUp')).modal('show');
    };

    $scope.post = function (advanceWriteOffId) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "advanceWriteOffId": advanceWriteOffId
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
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.advance = {};
        $scope.advance.Active = true;
        $scope.advance.CurrencyId = null;
        $scope.advance.PartyType = 'Customer';
        $scope.advance.Type = 'Receivable';
        $scope.advance.VoucherDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.voucherDetailList = [];
        $scope.getCboVoucherTypeEmployeeAdvanceWriteOffList();
        $scope.advance.DocRefNo = null;
        $scope.advance.SettlementType = "SetOff";
        $scope.advance.PaymentSource = 'Bank';
        $scope.advancePostingDate = null;
        $scope.advanceDocRefNo = null;
    }

    $scope.employeePayableSearchList = [
        {
            "Text": "VoucherNo",
            "Value": "VoucherNo"
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

    $scope.employeePayableDataList = [];
    $scope.employeePayableSearch = [];
    $scope.vendorInvoiceUrl = 'accounts/advance/GetEmployeeAvilabePayableList';
    $scope.customerInvoiceSelectedIndex = -1;
    $scope.employeePayableParameters = {
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

    $scope.showEmployeePayablePopUp = function (employeeId) {
        $scope.compareCurrencyId = $scope.advance.CurrencyId;
        $scope.employeePayableParameters.employeeId = employeeId;
        $scope.getEmployeePayableData = function (pageno) {
            baseService.paginationBase($scope.vendorInvoiceUrl, pageno, $scope.employeePayableParameters)
                .then(function (response) {
                    $scope.employeePayableDataList = response.Rows;
                    $scope.employeePayableParameters.total_count = response.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePayablePopUp')).modal('show');
        $scope.getEmployeePayableData();
    };

    $scope.selectEmployeePayablePopUp = function (data) {
        var getRow = $filter("filter")($scope.voucherDetailList, { "VoucherNo": data.VoucherNo });
        if (getRow.length === 0) {
            data.CompanyId = data.CompanyId;
            data.PlantId = data.PlantId;
            data.PartyType = data.PartyType;
            data.Amount = data.Balance;
            data.DocDate = $filter("dateFiltering")(data.DocDate);
            $scope.voucherDetailList.push(data);
            angular.element(document.querySelector("#employeePayablePopUp")).modal("hide");
        }
        else {
            ShowResult(data.VoucherNo + " already  Exist", "failure", "employeePayablePopUp");
        }
    };

    $scope.closeEmployeePayablePopUp = function () {
        angular.element(document.querySelector('#employeePayablePopUp')).modal('hide');
    };

    //*********************** Employee Advance PopUp Start *************************************
    $scope.employeeAdvanceSearchList = [
        //{
        //    "Text": "VoucherNo",
        //    "Value": "VoucherNo"
        //},
        {
            "Text": "Employee Code",
            "Value": "EmployeeCode"
        },
        {
            "Text": "Employee Name",
            "Value": "EmployeeName"
        },
        //{
        //    "Text": "PostingDate",
        //    "Value": "PostingDate"
        //},
        //{
        //    "Text": "DocDate",
        //    "Value": "DocDate"
        //},
        {
            "Text": "Currency",
            "Value": "CurrencyCode"
        }
    ];

    $scope.employeeAdvanceDataList = [];
    $scope.employeeAdvanceSearch = [];
    $scope.employeeAdvanceUrl = 'accounts/Advance/GetEmployeeAvilabeTotalAdvanceList';
    $scope.employeeAdvanceSalaryUrl = 'accounts/Advance/GetEmployeeAvilabeAdvanceSalaryList';
    $scope.employeeAdvanceSelectedIndex = -1;
    $scope.employeeAdvanceParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'EmployeeName',
        searchBy: 'EmployeeName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showEmployeeAdvancePopUpList = function (employeeId) {
        $scope.compareCurrencyId = $scope.advance.CurrencyId;
        $scope.getEmployeeTotalAdvanceData = function (pageno) {
            baseService.paginationBase($scope.employeeAdvanceUrl, pageno, $scope.employeeAdvanceParameters)
                .then(function (response) {
                    $scope.employeeAdvanceDataList = response.Rows;
                    $scope.employeeAdvanceParameters.total_count = response.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeeTotalAdvancePopUp')).modal('show');
        $scope.getEmployeeTotalAdvanceData();
    };
    $scope.showEmployeeAdvanceSalaryPopUpList = function (employeeId) {
        $scope.compareCurrencyId = $scope.advance.CurrencyId;
        $scope.getEmployeeAdvanceData = function (pageno) {
            baseService.paginationBase($scope.employeeAdvanceSalaryUrl, pageno, $scope.employeeAdvanceParameters)
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

    $scope.clearBankPopUp = function () {
        $scope.advance.BankMasterId = null;
        $scope.advance.CashMasterId = null;
    }

    $scope.clearCashPopUp = function () {
        $scope.clearBankPopUp();
    }
    $scope.closeEmployeeAdvancePopUp = function (data) {
        $scope.advance.EmployeeId = data.EmployeeId;
        $scope.advance.EmployeeName = data.EmployeeName;
        $scope.advance.AdvanceAmount = data.Balance;
        $scope.advance.VoucherNo = data.VoucherNo;
        $scope.advance.CompanyId = data.CompanyId;
        $scope.advance.PlantId = data.PlantId;
        $scope.advance.CurrencyId = data.CurrencyId;
        $scope.advance.AdvanceId = data.AdvanceId;
        $scope.advance.AdvanceDetailId = data.AdvanceDetailId;
        $scope.advance.PartyType = data.PartyType;
        $scope.advancePostingDate = data.PostingDate;
        $scope.advanceDocRefNo = data.DocRefNo;
        $scope.advance.CrAmount = null;
        $scope.advance.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.advance.BudgetMasterId = data.BudgetMasterId;
        $scope.advance.ActivityId = data.ActivityId;
        $scope.advance.JournalType = data.JournalType;
        $scope.GetEmployeeTransactionNo($scope.advance.EmployeeId);
        angular.element(document.querySelector("#employeeTotalAdvancePopUp")).modal("hide");
        angular.element(document.querySelector("#employeeAdvancePopUp")).modal("hide");
    };

    $scope.delete = function (advanceWriteOffId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "advanceWriteOffId": advanceWriteOffId, "voucherId": voucherId
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
                $scope.advanceWriteOffId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.advanceId = null;
    $scope.confirmDelete = function (advanceWriteOffId, voucherId) {
        $scope.advanceWriteOffId = advanceWriteOffId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

    //TODO:Report

    $scope.EmployeeAdvanceDueList = function () { 

        //var MasterOrderId = "1935";
        try {
            var file_src = $scope.url + "/EmployeeAdvanceDueList"; 
            $rootScope.report(file_src);


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    
       


}