'use strict';
employeeAdvanceController.$inject = ["bankService", "cboService", "baseService", "commonMessage", "$scope", "$rootScope", "$http", "$filter", "$controller"];
function employeeAdvanceController(bankService, cboService, baseService, commonMessage, $scope, $rootScope, $http, $filter, $controller) {
    $scope.Action = "Save";
    $rootScope.title = 'Employee Advance';
    $scope.isBankAmount = false;
    $scope.hideSource = true;
    $scope.voucherList = [];
    $scope.index = -1;
    $scope.url = 'accounts/Advance';
    $scope.listUrl = $scope.url + '/GetEmployeeAdvanceList';
    $scope.saveUrl = $scope.url + '/ParkEmployeeAdvance';
    $scope.updateUrl = $scope.url + '/UpdateEmployeeAdvance';
    $scope.postUrl = $scope.url + '/PostEmployeeAdvance';
    $scope.unPostUrl = $scope.url + '/UnPostEmployeeAdvance';
    $scope.deleteUrl = $scope.url + "/DeleteEmployeeAdvance";
    $scope.partyType = "Employee";
    $scope.partyGLType = "DownPayment";
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $controller('employeeBaseController', { $scope: $scope, $http: $http });

    $scope.advance = {
        Id: null,
        EntityId: null,
        PartyId: null,
        PartyCode: null,
        PartyName: null,
        PartyType: null,
        PartyPlantId: null,
        PartyPlantName: null,
        CurrencyId: null,
        PaymentTermId: null,
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        BankTransactionDate: null,
        BankReferenceNo: null,
        DocDate: null,
        DocRefNo: null,
        DocDate: null,
        DocRefNo: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        IsExcludingTax: false,
        Amount: '',
        Narration: null,
        BankName: null,
        CashName: null,
        BankMasterId: null,
        CashMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        PaymentSource: 'Bank',
        GLGeneralInfoId: null,
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetCode: null,
        BudgetName: null,
        ActivityId: null,
        ActivityCode: null,
        ActivityName: null,
        EmployeeId: null,
        EmployeeName: null,
        EmployeeGLGeneralInfoId: null,
        EmployeeGLGeneralInfoName: null,
        EmployeeTransactionTypeId: null,
        FinancingTypeId: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        IsInterTransaction: false,
        CompanyCurrencyRate: 1
    };

    $scope.advanceDetail = {
        Id: null,
        AdvanceId: null,
        PartyId: null,
        PartyCode: null,
        PartyName: null,
        PartyPlantId: null,
        PartyPlantName: null,
        PartyType: null,
        Narration: null,
        GLGeneralInfoId: null,
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetCode: null,
        BudgetName: null,
        ActivityId: null,
        ActivityCode: null,
        ActivityName: null,
        Amount: '',
        TaxAmount: '',
        NetAmount: '',
        EmployeeName: null
    };

    $scope.getCboVoucherTypeEmployeeAdvanceList = function () {
        cboService.getCboVoucherTypeEmployeeAdvanceList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.advance.PostingDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
                $scope.advance.BankTransactionDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
                $scope.advance.DocDate = $scope.advance.PostingDate;
            }
        });
    };
    $scope.getCboVoucherTypeEmployeeAdvanceList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.advance.VoucherTypeId = data.Value;
        $scope.advance.PostingDate = $filter('dateFiltering')(data.LastPostingDate);
        $scope.advance.DocDate = $scope.advance.PostingDate;
    };



    cboService.getCboEmployeeTransactionType(function (result) {
        $scope.employeeTransactionTypeList = result;
        if ($scope.employeeTransactionTypeList.length === 1) {
            $scope.advance.EmployeeTransactionTypeId = $scope.employeeTransactionTypeList[0].Value;
            $scope.advance.AdvanceType = $scope.employeeTransactionTypeList[0].AdvanceType;
            $scope.setVisible($scope.employeeTransactionTypeList[0].AdvanceType);
        }
    });

    $scope.transactionTypeGL = null;
    $scope.getTransactionTypeGL = function (id) {
        $scope.advanceDetailList = [];
        $scope.loanDetails = [];
        $("#loanDetails").children().remove();
        $scope.loanRepaymentSchedulelist = [];
        $scope.voucher.RepaymentStartDate = null;
        $scope.TotalPayments = null;
        $scope.voucher.NoOfInstallmentPerYear = null;
        if ($scope.advance.CurrencyId === null) {
            ShowResult('Please Select Currency!', 'failure');
            return;
        }
        if (!baseService.isUndefinedOrNull(id)) {
            $scope.transactionTypeGL = $.grep($scope.employeeTransactionTypeList, function (item) {
                return item.EmployeeTransactionTypeId === id;
            })[0];
            if (manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.transactionTypeGL.AdvanceGLId), 'Transaction Type GL not found!')) {
                $scope.transactionTypeGL = null;
                $scope.advanceDetailList = [];
            }
            else {
                $scope.advanceDetail.GLGeneralInfoId = $scope.transactionTypeGL.AdvanceGLId;
                $scope.advanceDetail.GLGeneralInfoCode = $scope.transactionTypeGL.AdvanceGLCode;
                $scope.advanceDetail.GLGeneralInfoName = $scope.transactionTypeGL.AdvanceGLName;
                $scope.advanceDetail.BudgetMasterId = $scope.transactionTypeGL.AdvanceBudgetMasterId;
                $scope.advanceDetail.BudgetCode = $scope.transactionTypeGL.AdvanceBudgetCode;
                $scope.advanceDetail.BudgetName = $scope.transactionTypeGL.AdvanceBudgetName;
                $scope.advanceDetail.ActivityId = $scope.transactionTypeGL.AdvanceActivityId;
                $scope.advanceDetail.ActivityCode = $scope.transactionTypeGL.AdvanceActivityCode;
                $scope.advanceDetail.ActivityName = $scope.transactionTypeGL.AdvanceActivityName;
                $scope.advanceDetail.EmployeeTransactionTypeId = $scope.transactionTypeGL.EmployeeTransactionTypeId;
                $scope.advance.JournalType = $scope.transactionTypeGL.AdvanceType;

                $scope.advanceDetail.Narration = $scope.advance.Narration;
                $scope.advanceDetail.EmployeeId = $scope.advance.EmployeeId;
                $scope.advanceDetail.Amount = $scope.advance.Amount;

                $scope.advanceDetailList.push($scope.advanceDetail);
                $scope.setVisible($scope.transactionTypeGL.AdvanceType);
                $scope.advanceDetail = {};
            }
        }
        else {
            manualValidation('div_TransactionType', false, '');
            $scope.transactionTypeGL = null;
        }
    };

    baseService.init($scope.listUrl, null, null, "DESC", "PostingDate", "VoucherNo");
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
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            "name": "Employee Code",
            "value": "EmployeeCode"
        },
        {
            "name": "Employee",
            "value": "EmployeeName"
        },
        {
            "name": "Posting Date",
            "value": "PostingDate"
        },
        {
            "name": "Entity",
            "value": "EntityName"
        },
        {
            "name": "Amount",
            "value": "Amount"
        },
        {
            "name": "Written Off Amount",
            "value": "WrittenOffAmount"
        },
        {
            "name": "Currency",
            "value": "CurrencyCode"
        },
        {
            "name": "Status",
            "value": "Status"
        }
    ];
    $scope.advanceDetailList = [];

    cboService.getCboEntityByPlant(null, null, '', function (result) {
        $scope.entityList = result;
    });
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.tranCurrencyList = result;
        $scope.advance.CurrencyId = $scope.selectBaseCurrency();
        $scope.companyCurrencyId = $scope.selectBaseCurrency();
    });
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
            $scope.advance.DocRefNo = "EA-" + $scope.employeeTransactionNo;
        });
    };

    $scope.getById = function (id, empcode, employeeName, rescode, resName) {
        $http({
            method: 'GET',
            url: 'accounts/Advance/GetAdvance/' + id
        }).then(function successCallback(response) {
            $scope.advance = response.data;
            $scope.advance.Id = id;
            $scope.advance.DocDate = $filter('dateFiltering')($scope.advance.DocDate);
            $scope.advance.VoucherDate = $filter('dateFiltering')($scope.advance.VoucherDate);
            $scope.advance.PostingDate = $filter('dateFiltering')($scope.advance.PostingDate);
            $scope.advance.EmployeeName = empcode + ' - ' + employeeName;
            if ($scope.advance.ResponsiblePersonId) {
                $scope.advance.ResponsiblePersonName = rescode + ' - ' + resName;
            }
            $scope.getTransactionTypeGL($scope.advance.EmployeeTransactionTypeId);
            $scope.GetAdvanceReqSchedule($scope.advance.Id);
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };
    $scope.DetailsList = [];
    $scope.GetAdvanceReqSchedule = function (Id) {
        $http({
            method: "GET",
            url: "accounts/Advance/GetAdvanceReqSchedule?Id=" + Id
        }).then(function successCallback(response) {
            $scope.DetailsList = response.data;
            $scope.voucher.ProfitRate = 0;
            $scope.TotalInterestPaid = 0;
            $scope.voucher.RepaymentStartDate = $scope.DetailsList[0].InstallmentDate;
            $scope.voucher.TotalNoOfInstallment = $scope.DetailsList.length;
            $scope.voucher.NoOfInstallmentPerYear = $scope.DetailsList.length;
            $scope.TotalPayments = $scope.advance.Amount;
            //for (var i = 0; i < $scope.DetailsList.length; i++) {
            //    $scope.TotalPayments += $scope.DetailsList[i].PrincipalAmount;
            //}

        });
    }

    $scope.InstallValidation = function () {

    }
    $scope.voucher_Post = {
        Id: null,
        EntityId: null,
        CurrencyId: null,
        CurrencyCode: null,
        VoucherNo: null,
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        Narration: null,
        Amount: null
    };
    $scope.advanceId = null;
    $scope.EntityId_Post = null;
    $scope.voucherId = null;
    $scope.confirmPost = function (advanceId, data) {
        $scope.advanceId = advanceId;
        $scope.EntityId_Post = data.EntityId;
        $scope.voucherId = data.VoucherId;
        $scope.voucher_Post = data;
        angular.element(document.querySelector('#PostPopUp')).modal('show');
        //$scope.message_confirmation = 'Are you sure to Post?';
        //angular.element(document.querySelector('#confirmPostPopUp')).modal('show');
    };
    $scope.closePostPopUp = function () {
        angular.element(document.querySelector("#PostPopUp")).modal("hide");
    };
    $scope.GetCurrencyExchangeRateList = function () {
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

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.advance.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (baseService.isUndefinedOrNull($scope.advance.DocDate)) {
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
        if (new Date($scope.advance.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.advance.PostingDate) < new Date($scope.advance.DocDate)) {
            msg = "Posting date must be below or equal to Doc Date!";
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.advance.PostingDate)) {
            msg = "Posting Date is required.";
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
            $scope.GetCurrencyExchangeRateList();
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.invalidEntity = false;
    $scope.entityValidation = function () {
        $scope.invalidEntity = baseService.isUndefinedOrNull($scope.advance.EntityId);
        return manualValidation("div_entity", $scope.invalidEntity, "Entity is required.");
    };

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return true;
        }
        if ($scope.partyType === "Employee") {
            if ($scope.advance.EmployeeId === null) {
                ShowResult("Please select Employee!", "failure");
                return true;
            }
        }
        return false;
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.advance.PartyType = $scope.partyType;
            $scope.advance.EmployeeId = employee.SystemId;
            $scope.advance.EmployeeName = employee.EmployeeName;
            $scope.advance.CurrencyId = $scope.selectBaseCurrency();
            $scope.GetEmployeeTransactionNo($scope.advance.EmployeeId)
        }
        $scope.hideEmployeePopUp();
    };

    $scope.clearEmployeePopUp = function () {
        $scope.advance.EmployeeId = null;
        $scope.advance.EmployeeName = null;
    };

    $scope.closeResponsiblePersonPopUp = function () {
        if ($scope.responsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.responsiblePersonIndex];
            $scope.advance.ResponsiblePersonId = employee.SystemId;
            $scope.advance.ResponsiblePersonName = employee.EmployeeName;
        }
        $scope.hideResponsiblePersonPopUp();
    };

    $scope.clearResponsiblePersonPopUp = function () {
        $scope.advance.ResponsiblePersonId = null;
        $scope.advance.ResponsiblePersonName = null;
    };

    $scope.clearBankPopUp = function () {
        $scope.advance.BankName = null;
        $scope.advance.BankMasterId = null;
        $scope.advance.BankCurrencyId = null;
        $scope.advance.CashMasterId = null;
        $scope.advance.CashName = null;
    };
    $scope.clearCashPopUp = function () {
        $scope.advance.BankName = null;
        $scope.advance.BankMasterId = null;
        $scope.advance.BankCurrencyId = null;
        $scope.advance.CashMasterId = null;
        $scope.advance.CashName = null;
    };

    $scope.clear = function () {
        $scope.Action = "Save";
        $scope.getCboVoucherTypeEmployeeAdvanceList();
        $scope.advance.Active = true;
        $scope.advance.Id = null;
        $scope.advance.DocRefNo = null;
        $scope.advance.PaymentSource = 'Bank';
        $scope.advance.DocRefNo = null;
        $scope.advance.ReviewDate = null;
        $scope.advance.Amount = null;
        $scope.advance.EmployeeTransactionTypeId = null;
        $scope.advance.Narration = null;
        $scope.advance.IsInterTransaction = false;
        $scope.advance.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.currencyExchangeRate = [];
        $scope.advanceDetailList = [];
        $scope.loanDetails = [];
        $("#loanDetails").children().remove();
        $scope.loanRepaymentSchedulelist = [];
        $scope.voucher.RepaymentStartDate = null;
        $scope.voucher.NoOfInstallmentPerYear = null;
        $scope.TotalPayments = null;
        $scope.TotalInterestPaid = null;
        $scope.clearEmployeePopUp();
        $scope.clearCashPopUp();
        $scope.clearBankPopUp();
        $scope.clearResponsiblePersonPopUp();
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        try {            
            if ($scope.form0.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidEntity && !$scope.invalidPostingDate) {
                if ($scope.Action === "Save") {
                    $http({
                        method: "POST",
                        url: $scope.saveUrl,
                        data: {
                            "voucherVM": $scope.advance,
                            "voucherDetailVMList": $scope.advanceDetailList,
                            "advanceSalarySchedulelist": $scope.loanRepaymentSchedulelist
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
                }
                else if ($scope.Action === "Update") {
                    //if ($scope.advance.IsPark == true) {
                        var Total = 0;

                        for (var i = 0; i < $scope.DetailsList.length; i++) {
                            Total += parseFloat($scope.DetailsList[i].InstallmentAmount);                            
                        }
                        if ($scope.TotalPayments != Total) {
                            throw "Installment amount cannot exceed or Less than [Total Payments]";
                        }


                        $http({
                            method: "POST",
                            url: $scope.updateUrl,
                            data: {
                                "advanceVM": $scope.advance,
                                "advanceDetailVMList": $scope.advanceDetailList,
                                'DetailsList': $scope.DetailsList
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

                    //}
                    //else {
                    //    throw "In Post Mood";
                    //}
                }
            }
        } catch (e) {
            ShowResult(e, 'info');
        }
    };

    $scope.post = function () {
        if ($scope.EntityId_Post == null || $scope.EntityId_Post == "" || $scope.EntityId_Post == undefined) {
            ShowResult("Please select Entity First!!", "failure");
        }
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "advanceId": $scope.advanceId,
                "entityId": $scope.EntityId_Post,
                "voucherId": $scope.voucherId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                $scope.closePostPopUp();
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.unPost = function (advanceId) {
        $http({
            method: "POST",
            url: $scope.unPostUrl,
            data: {
                "advanceId": advanceId
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


    $scope.closeCashPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult("Please Select Currency!", "failure", "cashPopUp");
            return;
        }
        if ($scope.cashIndex !== -1) {
            var cash = $scope.cashList[$scope.cashIndex];
            if (baseService.isUndefinedOrNull(cash.GLGeneralInfoId)) {
                ShowResult("Cash GL not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(cash.BudgetMasterId)) {
                ShowResult("Cash Budget not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(cash.CurrencyId)) {
                ShowResult("Cash Transaction Currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.advance.CashMasterId = cash.Id;
                $scope.advance.CashCurrencyId = cash.CurrencyId;
                $scope.advance.CashName = cash.CashName;
                $scope.advance.GLGeneralInfoId = cash.GLGeneralInfoId;
                $scope.advance.GLGeneralInfoName = cash.GLItem;
                $scope.advance.BudgetName = cash.BudgetName;
                $scope.advance.BudgetMasterId = cash.BudgetMasterId;
                $scope.advance.ActivityId = cash.ActivityId;
                $scope.advance.ActivityName = cash.ActivityName;
                $scope.checkCashAmount();
            }
        }
        $scope.hideCashPopUp();
        $scope.calBaseAmount();
    };

    $scope.closeBankPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult("Please Select Currency !", "failure", "bankPopUp");
            return;
        }
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
                ShowResult("Bank GL not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(bank.BudgetMasterId)) {
                ShowResult("Bank Budget not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(bank.CurrencyId)) {
                ShowResult("Bank Transaction Currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.advance.AccountTitle = bank.AccountTitle;
                $scope.advance.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
                $scope.advance.BankMasterId = bank.BankMasterId;
                $scope.advance.BankCurrencyId = bank.CurrencyId;
                $scope.advance.BankCurrencyCode = bank.CurrencyCode;
                $scope.advance.GLGeneralInfoId = bank.GLGeneralInfoId;
                $scope.advance.GLGeneralInfoName = bank.GLGeneralInfoName;
                $scope.advance.BudgetMasterId = bank.BudgetMasterId;
                $scope.advance.BudgetName = bank.BudgetName;
                $scope.advance.ActivityId = bank.ActivityId;
                $scope.advance.ActivityName = bank.ActivityName;
                //$scope.checkBankAmount();
            }
        }
        $scope.hideBankPopUp();
        $scope.calBaseAmount();
    };
    $scope.checkCashAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.advance.CashCurrencyId)) {
            if ($scope.advance.CashCurrencyId !== $scope.companyCurrencyId) {
                $scope.isBankAmount = true;
                $scope.advance.BankAmount = 0;
            }
            else {
                $scope.isBankAmount = false;
                $scope.advance.BankAmount = 0;
            }
        }
    };


    $scope.delete = function (advanceId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "advanceId": advanceId, "voucherId": voucherId
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
                $scope.advanceId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.advanceId = null;
    $scope.confirmDelete = function (advanceId, voucherId) {
        $scope.advanceId = advanceId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };


    $scope.totalInstallment = function () {
        //if ($scope.voucher.NoOfInstallmentPerYear < 12) {
        //    $scope.voucher.LifeOfYear = 1;
        //}
        //else {
        //    $scope.voucher.LifeOfYear = $scope.voucher.NoOfInstallmentPerYear / 12;
        //}

        //$scope.voucher.TotalNoOfInstallment = ($scope.voucher.LifeOfYear * $scope.voucher.NoOfInstallmentPerYear);
        $scope.voucher.TotalNoOfInstallment = $scope.voucher.NoOfInstallmentPerYear;
    };

    $scope.loanRepaymentSchedulelist = [];
    $scope.TotalPayments = 0;
    $scope.TotalInterestPaid = 0;
    $scope.LoadRepamentDetail = function () {
        if ($scope.Action == "Save") {
            if ($scope.voucher.ProfitRate === '' || $scope.voucher.ProfitRate == 'undefined' || $scope.voucher.ProfitRate === null) {
                $scope.voucher.ProfitRate = 0;
            }

            if ($scope.voucher.NoOfInstallmentPerYear < 12) {
                $scope.voucher.LifeOfYear = 1;
            }
            else {
                $scope.voucher.LifeOfYear = $scope.voucher.NoOfInstallmentPerYear / 12;
            }

            $scope.voucher.Amount = $scope.advance.Amount;
            $scope.loanRepaymentSchedulelist = [];
            $("#loanDetails").children().remove();
            var numberOfInstallment = $scope.voucher.NoOfInstallmentPerYear;
            var actualAmount = parseFloat($scope.voucher.Amount);
            var actualAmountWithoutProfit = parseFloat($scope.voucher.Amount);
            var profitAmount = $scope.voucher.ProfitAmount;

            var installmentPerYear = 12;
            var rate = parseFloat((parseFloat($scope.voucher.ProfitRate) / 100) / installmentPerYear);


            var disbursmentDate = $scope.voucher.DocDate;
            var repaymentStartDate = $scope.voucher.RepaymentStartDate;
            var installmentDate;
            var payment = 0.00;
            var profit = 0.00;
            var principal = 0.00;

            var totalPayment = 0.00;
            var totalProfit = 0.00;
            var totalPrincipal = 0.00;

            var i = 0;

            var idate;
            var periodHtml = "<div class='SearchResult'> <table><thead><tr><td style='width:220px;'>Installment date</td><td style='width:100px;'>Installment no.</td><td style='text-align:right; width:120px;'>Payment</td><td style='text-align:right; width:120px;'>Interest</td><td style='text-align:right; width:120px;'>Principal</td><td style='text-align:right; width:120px;'>Loan</td></tr></thead>";

            for (var i = 1; i <= numberOfInstallment; i++) {
                if (i === 1) {
                    installmentDate = new Date(repaymentStartDate);
                    idate = installmentDate;
                }
                if (i > 1) {
                    installmentDate = new Date((new Date(idate)).setMonth((new Date(idate)).getMonth() + (12 / installmentPerYear)));
                    idate = installmentDate;
                }
                if (rate === 0) {
                    payment = actualAmountWithoutProfit / numberOfInstallment;
                }
                else {
                    payment = PMT(rate, numberOfInstallment, installmentPerYear, parseFloat($scope.voucher.Amount));
                }
                var iRate = parseFloat($scope.voucher.ProfitRate) / 100;
                profit = (actualAmount * iRate) / installmentPerYear;

                principal = payment - profit;

                if (i === parseFloat(numberOfInstallment)) {
                    actualAmount = parseFloat("0.00");
                }
                else {
                    actualAmount = actualAmount - principal;
                }
                var schedule = new Object({
                    InstallmentNo: i,
                    InstallmentDate: new Date(idate),
                    InstallmentAmount: payment,
                    ProfitAmount: profit,
                    PrincipalAmount: principal,
                    Balance: actualAmount,
                    ScheduleNo: 1
                });
                $scope.loanRepaymentSchedulelist.push(schedule);

                totalPayment = totalPayment + payment;
                totalProfit = totalProfit + profit;
                totalPrincipal = totalPrincipal + principal;

                $scope.TotalPayments = totalPayment.toFixed(2);
                $scope.TotalInterestPaid = totalProfit.toFixed(2);

                periodHtml += "<tr><td style ='width:220px;'>" + FormatDate(idate) + "</td><td style ='width:100px;'>" + i + "</td><td style='text-align:right; width:120px;'>" + payment.toFixed(2) + "</td><td style='text-align:right; width:120px;'>" + profit.toFixed(2) + "</td><td style='text-align:right; width:120px;'>" + principal.toFixed(2) + "</td><td style='text-align:right; width:120px;'>" + actualAmount.toFixed(2) + "</td></tr>";
            }
            $("#loanDetails").append(periodHtml);
            $scope.voucher.ProfitAmount = totalProfit.toFixed(2);
            return false;
        }
        else {
            var numberOfInstallments = $scope.voucher.NoOfInstallmentPerYear;
            var numberOfexistingschedule = $scope.DetailsList.length;
            var installmentDates, idates = $scope.DetailsList[numberOfexistingschedule - 1].InstallmentDate;
            var installmentPerYears = 12;
            for (var i = 1; i <= numberOfInstallments - numberOfexistingschedule; i++) {
                installmentDates = new Date((new Date(idates)).setMonth((new Date(idates)).getMonth() + (12 / installmentPerYears)));
                idates = installmentDates;
                var schedule = new Object({
                    InstallmentNo: numberOfexistingschedule + i,
                    InstallmentDate: FormatDate(idates),
                    InstallmentAmount: 0,
                    ProfitAmount: 0,
                    PrincipalAmount: 0,
                    Balance: 0,
                    ScheduleNo: 1
                });
                $scope.DetailsList.push(schedule); 
            }
        }
        
    };

    function PMT(rate, numberOfInstallment, installmentPerYear, actualAmount) {
        var numberOfYear = numberOfInstallment / installmentPerYear;

        var a = 1 / rate;
        var b = 1 + rate;
        var c = Math.pow(b, numberOfInstallment);//
        var d = rate * c;
        var e = 1 / d;

        var pvFactor = a - e;
        var payment = actualAmount / pvFactor;
        return payment;
    }

    function FormatDate(input) {
        var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        var dt = new Date(input);
        return [dt.getDate(), months[dt.getMonth()], dt.getFullYear()].join('-');
    }

    $scope.isVisibleInstallment = false;
    $scope.setVisible = function (AdvanceType) {
        if (AdvanceType === null || AdvanceType === undefined || AdvanceType === '') {
            $scope.isVisibleInstallment = false;
            //return 
        }
        else {
            if (AdvanceType === 'Salary') {
                $scope.isVisibleInstallment = true;
            }
            else {
                $scope.isVisibleInstallment = false;
            }
        }
    }
    $scope.ValueChange = function (data, index) {
        for (var i = 0; i < $scope.DetailsList.length; i++) {
            if ($scope.DetailsList[i].InstallmentNo == 1) {
                $scope.DetailsList[i].PrincipalAmount = parseFloat($scope.DetailsList[i].InstallmentAmount);
                $scope.DetailsList[i].Balance = parseFloat($scope.TotalPayments - $scope.DetailsList[i].PrincipalAmount).toFixed(2);
            }
            else {
                $scope.DetailsList[i].PrincipalAmount = parseFloat($scope.DetailsList[i].InstallmentAmount);
                $scope.DetailsList[i].Balance = parseFloat(($scope.DetailsList[i - 1].Balance - $scope.DetailsList[i].PrincipalAmount)).toFixed(2);
            }
          };
    }
}