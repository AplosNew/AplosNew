'use strict';
employeeAdvanceRequisitionPostController.$inject = ["bankService", "cboService", "baseService", "commonMessage", "$scope", "$rootScope", "$http", "$filter", "$controller"];
function employeeAdvanceRequisitionPostController(bankService, cboService, baseService, commonMessage, $scope, $rootScope, $http, $filter, $controller) {
    $scope.Action = "Save";
    $rootScope.title = 'Employee Advance Requsition Posting';
    $scope.isBankAmount = false;
    $scope.hideSource = true;
    $scope.voucherList = [];
    $scope.index = -1;
    $scope.url = 'accounts/Advance';
    $scope.listUrl = $scope.url + '/GetEmployeeAdvanceRequisitionPostList';
   // $scope.saveUrl = $scope.url + '/ParkEmployeeAdvanceRequisitionPost';
    $scope.saveUrl = $scope.url + '/ParkEmployeeAdvanceRequisition';
    $scope.updateUrl = $scope.url + '/UpdateEmployeeAdvanceRequisitionPost';
    $scope.postUrl = $scope.url + '/PostEmployeeAdvanceHR';
    $scope.unPostUrl = $scope.url + '/UnPostEmployeeAdvanceRequisitionPost';
    $scope.deleteUrl = $scope.url + "/DeleteEmployeeAdvanceHR";

    $scope.partyType = "Employee";
    $scope.partyGLType = "DownPayment";
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
        CompanyCurrencyRate: 1,
        RequisitionId: null,
        CheckedBy: null,
        ApprovedBy: null,
        RequisitionRequiredDate: null,
        AdvanceType:null
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

    
    $scope.GetEmployeeTransactionType = function (advanceType) {
        cboService.getEmpTrnTypeByAdvanceType(advanceType ,function (result) {
            $scope.employeeTransactionTypeList = result;
            if ($scope.employeeTransactionTypeList.length === 1) {
                $scope.advance.EmployeeTransactionTypeId = $scope.employeeTransactionTypeList[0].EmployeeTransactionTypeId;
                $scope.getTransactionTypeGL($scope.advance.EmployeeTransactionTypeId)
            }
        });
    }
    

    $scope.transactionTypeGL = null;
    $scope.getTransactionTypeGL = function (id) {
        $scope.advanceDetailList = [];
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
                $scope.advanceDetail.Narration = $scope.advance.Narration;
                $scope.advanceDetail.EmployeeId = $scope.advance.EmployeeId;
                $scope.advanceDetail.Amount = $scope.advance.Amount;
                $scope.advanceDetailList.push($scope.advanceDetail);

                $scope.advance.GLGeneralInfoId = $scope.transactionTypeGL.AdvanceGLId;
                $scope.advance.BudgetMasterId = $scope.transactionTypeGL.AdvanceBudgetMasterId;
                $scope.advance.ActivityId = $scope.transactionTypeGL.AdvanceActivityId;
                $scope.advance.EmployeeTransactionTypeId = $scope.transactionTypeGL.EmployeeTransactionTypeId;

                $scope.advanceDetail = {};
            }
        }
        else {
            manualValidation('div_TransactionType', false, '');
            $scope.transactionTypeGL = null;
        }
    };

    baseService.init($scope.listUrl, null, null, "DESC", "PostingDate desc,VoucherNo", "VoucherNo");
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
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };

    $scope.advanceId = null;
    $scope.voucherId = null;
    $scope.requisitionId = null;
    $scope.confirmPost = function (voucherId, RequisitionId) {
        $scope.voucherId = voucherId;
        $scope.requisitionId = RequisitionId;
        $scope.message_confirmation = 'Are you sure to Post?';
        angular.element(document.querySelector('#confirmPostPopUp')).modal('show');
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

    $scope.advanceEMIScheduleList = [];
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
        if ($scope.loanRepaymentSchedulelist.length > 0) {
            $scope.advanceEMIScheduleList = [];
            for (var i = 0; i < $scope.loanRepaymentSchedulelist.length; i++) {
                $scope.advanceEMIScheduleList.push($scope.loanRepaymentSchedulelist[i])
            }
        }
        if ($scope.getAdvanceReqScheduleList.length > 0) {
            $scope.advanceEMIScheduleList = [];
            for (var i = 0; i < $scope.getAdvanceReqScheduleList.length; i++) {
                $scope.advanceEMIScheduleList.push($scope.getAdvanceReqScheduleList[i])
            }
        }
        if ($scope.advance.AdvanceType == 'Salary' && $scope.advanceEMIScheduleList.length == 0) {
            ShowResult("Please Input Installment !!", "failure");
            return true;
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
        $scope.advance = {};
        $scope.getCboVoucherTypeEmployeeAdvanceList();
        $scope.advance.Active = true;
        $scope.advance.Id = null;
        $scope.advance.DocRefNo = null;
        $scope.advance.CheckedBy = null;
        $scope.advance.AdvanceType = null;
        $scope.advance.ApprovedBy = null;
        $scope.advance.RequisitionRequiredDate = null;
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
        $scope.voucher.RepaymentStartDate = null;
        $scope.voucher.LifeOfYear = null;
        $scope.voucher.ProfitRate = null;
        $scope.voucher.NoOfInstallmentPerYear = null;
        $scope.loanDetails = [];
        $scope.loanRepaymentSchedulelist = [];
        $scope.getAdvanceReqScheduleList = []; 
        $scope.advanceEMIScheduleList = [];
        $scope.clearEmployeePopUp();
        $scope.clearCashPopUp();
        $scope.clearBankPopUp();
        $scope.clearResponsiblePersonPopUp();
    };

    $scope.data = {
        Id: null,
        YearNo: null,
        MonthNo: null,
        FromDate: null,
        ToDate: null,
        UserName: null,
        UserRef: null,
        
        PayDaysType: null,
        Percentage: null,
        Multiple: null,
        MinimumPresentDay: null,
        IsPayDay:null,
        IsStandardOT: null,
        IsAdditionalOT: null,
        IsAdditionalOT: null,
        PreparedById: null,
        ApprovedById: null,
        Remarks: null,
        

        ApprovedStatus: null,
        PaymentsStatus: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        EntityId: null,
        CurrencyId: null,
        ToCurrencyRate: null,
        EmployeeTransactionTypeId: null,
        SourceType: null,
        FiscalYearId: null,
        FiscalYearPeriodId: null,
        TaxYearId: null,
        TaxYearPeriodId: null,
        RequisitionId: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null,
        
    };
    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        if ($scope.form0.$valid && !$scope.invalidDocDate && !$scope.invalidEntity && !$scope.invalidPostingDate && !$scope.validation()) {
            $scope.data.UserName ='Advanec Of '+ $scope.advance.EmployeeName
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: {
                        "voucherVM": $scope.advance,
                        "data": $scope.data,
                        "advanceDetail": $scope.advanceDetailList,
                        "advanceSalarySchedulelist": $scope.advanceEMIScheduleList
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
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: {
                        "advanceVM": $scope.advance,
                        "advanceDetailVMList": $scope.advanceDetailList
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
            }
            return true;
        }
        return true;
    };

    $scope.post = function (requisitionId,voucherId ) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "voucherId": voucherId,
                "requisitionId": requisitionId
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

    $scope.getEmployeeAdvanceRequisitionList = [];
    $scope.getEmployeeAdvanceRequisitionApprovedList = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: "accounts/Advance/GetEmployeeAdvanceRequisitionApprovedList"
        }).then(function successCallback(response) {
            $scope.getEmployeeAdvanceRequisitionList = response.data;
        });
    };
    
    $scope.popUp = function () {
        $scope.getEmployeeAdvanceRequisitionApprovedList();
        angular.element(document.querySelector('#EmployeeAdvanceRequisitionPopUp')).modal('show');
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#EmployeeAdvanceRequisitionPopUp')).modal('hide');
    };
    $scope.selectDoubleClick = function (args) {

    }
    $scope.isVisibleInstallment = false;
    $scope.setVisible = function (AdvanceType) {
        if (AdvanceType === "Salary") {
            $scope.isVisibleInstallment = true;
        }
        else {
            $scope.isVisibleInstallment = false;
        }
    } 
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

        if ($scope.voucher.ProfitRate === '' || $scope.voucher.ProfitRate == 'undefined' || $scope.voucher.ProfitRate === null) {
            $scope.voucher.ProfitRate = 0;
        }
        //if ($scope.voucher.IsSchedule) {
        if ($scope.voucher.NoOfInstallmentPerYear < 12) {
            $scope.voucher.LifeOfYear = 1;
        }
        else {
            $scope.voucher.LifeOfYear = $scope.voucher.NoOfInstallmentPerYear / 12;
        }

        $scope.voucher.Amount = $scope.advance.Amount;
        $scope.loanRepaymentSchedulelist = [];
        $("#loanDetails").children().remove();
        //var numberOfInstallment = $scope.voucher.TotalNoOfInstallment;
        var numberOfInstallment = $scope.voucher.NoOfInstallmentPerYear;
        var actualAmount = parseFloat($scope.voucher.Amount);
        var actualAmountWithoutProfit = parseFloat($scope.voucher.Amount);
        var profitAmount = $scope.voucher.ProfitAmount;
        //var installmentPerYear = $scope.voucher.NoOfInstallmentPerYear;
        var installmentPerYear = 12;
        //if ($scope.voucher.NoOfInstallmentPerYear < 12) {           
        //    installmentPerYear = $scope.voucher.NoOfInstallmentPerYear;
        //}
        var rate = parseFloat((parseFloat($scope.voucher.ProfitRate) / 100) / installmentPerYear);
        //rate = parseFloat(rate.toFixed(2));
        //var rate = parseFloat((parseInt($scope.voucher.ProfitRate) / 100) / installmentPerYear);
        //console.log('rate', rate);
        //console.log('rated', $scope.voucher.ProfitRate);

        var disbursmentDate = $scope.voucher.DocDate;
        var repaymentStartDate = $scope.voucher.RepaymentStartDate;
        // var installmentDate = new Date(repaymentStartDate);
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
        //periodHtml += "<tr><td>" + FormatDate(disbursmentDate) + " (Disbursement date)" + "</td><td>" + " " + "</td><td style='text-align:right'>" + payment.toFixed(2) + "</td><td style='text-align:right'>" + profit.toFixed(2) + "</td><td style='text-align:right'>" + principal.toFixed(2) + "</td><td style='text-align:right'>" + actualAmount.toFixed(2) + "</td></tr>";
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
                InstallmentAmount: payment.toFixed(2),
                ProfitAmount: profit.toFixed(2),
                PrincipalAmount: principal.toFixed(2),
                Balance: actualAmount.toFixed(2),
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
        //periodHtml += "<tr><td></td><td></td><td style='text-align:right;font-weight: bold'>" + totalPayment.toFixed(2) + "</td><td style='text-align:right;font-weight: bold'>" + totalProfit.toFixed(2) + "</td><td style='text-align:right;font-weight: bold'>" + totalPrincipal.toFixed(2) + "</td><td></tr></table></div>";
        $("#loanDetails").append(periodHtml);
        $scope.voucher.ProfitAmount = totalProfit.toFixed(2);
        return false;
        //}
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

    
    $scope.selectDoubleClick = function (args) {
        var gridObj = $("#employeeAdvanceRequisitionId").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.advance.EmployeeName = data.EmployeeName;
        $scope.advance.EmployeeId = data.EmpSystemId;
        $scope.advance.Amount = data.Amount;
        $scope.advance.CurrencyId = data.CurrencyId;
        $scope.advance.RequisitionId = data.SystemId;
        $scope.advance.CheckedBy = data.CheckedBy;
        $scope.advance.AdvanceType = data.AdvanceType;
        $scope.advance.JournalType = data.AdvanceType;
        $scope.advance.ApprovedBy = data.ApprovedBy;
        $scope.advance.RequisitionRequiredDate = data.RequisitionRequiredDate;
        if ($scope.advance.AdvanceType === 'Salary') {
            $scope.getAdvanceReqScheduleListByRequisitionId();
            $scope.setVisible($scope.advance.AdvanceType);
        }
        $scope.GetEmployeeTransactionType(data.AdvanceType);
        $scope.GetEmployeeTransactionNo($scope.advance.EmployeeId);
        $scope.GetCurrencyExchangeRateList();

        angular.element(document.querySelector('#EmployeeAdvanceRequisitionPopUp')).modal('hide');

    };
    $scope.getAdvanceReqScheduleList = [];
    $scope.getAdvanceReqScheduleListByRequisitionId = function () {
        $http({
            method: 'GET',
            url: "accounts/Advance/GetAdvanceReqScheduleListByRequisitionId?requisitionId=" + $scope.advance.RequisitionId
        }).then(function successCallback(response) {
            $scope.getAdvanceReqScheduleList = response.data;
        });
    };
    
    $scope.delete = function (employeeAdvanceId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "employeeAdvanceId": employeeAdvanceId, "voucherId": voucherId
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
    $scope.confirmDelete = function (employeeAdvanceId, voucherId) {
        $scope.employeeAdvanceId = employeeAdvanceId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

}