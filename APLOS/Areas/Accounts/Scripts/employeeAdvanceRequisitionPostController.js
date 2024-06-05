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

    baseService.init($scope.listUrl, null, null, "DESC", "VoucherNo", "VoucherNo");
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
        $scope.loanRepaymentSchedulelist = [];
        $scope.getAdvanceReqScheduleList = [];
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
        if ($scope.form0.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidEntity && !$scope.invalidPostingDate) {
            $scope.data.UserName ='Advanec Of '+ $scope.advance.EmployeeName
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: {
                        "voucherVM": $scope.advance,
                        "data": $scope.data,
                        "advanceDetail": $scope.advanceDetailList
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
    //$scope.searchglemployeeReconGLByList = [
    //    {
    //        "name": "Account Group",
    //        "value": "AccountGroupName"
    //    },
    //    {
    //        "name": "GL Code",
    //        "value": "GLGeneralInfoCode"
    //    },
    //    {
    //        "name": "GL Name",
    //        "value": "GLGeneralInfoName"
    //    },
    //    {
    //        "name": "Budget",
    //        "value": "BudgetName"
    //    },
    //    {
    //        "name": "Activity",
    //        "value": "ActivityName"
    //    },
    //    {
    //        "name": "Ref No",
    //        "value": "RefNo"
    //    }
    //];

    //$scope.employeeReconglListParameters = {
    //    limit: 10,
    //    offset: 0,
    //    order: "asc",
    //    sort: "GLGeneralInfoCode",
    //    searchBy: "ActivityName",
    //    pageSize: 10,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};

    //$scope.GetemployeeReconGLList = function () {
    //    $scope.GLUrl2 = "Accounts/glitem/GetEmployeeReconAssetGLBudgetActivity";
    //    $scope.GetemployeeReconGLListData = function (pageno) {
    //        baseService.paginationBase($scope.GLUrl2, pageno, $scope.employeeReconglListParameters)
    //            .then(function (result) {
    //                $scope.employeeReconGLList = result.Rows;
    //                $scope.employeeReconglListParameters.total_count = result.Total;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, "failure");
    //            }).finally(function () {
    //            });
    //    };
    //    angular.element(document.querySelector("#employeeReconGLPopUp")).modal("show");
    //    $scope.modalShow = true;
    //    $scope.GetemployeeReconGLListData();
    //};

    //$scope.closeEmployeeReconGLListPopUp = function () {
    //    angular.element(document.querySelector("#employeeReconGLPopUp")).modal("hide");
    //};

    //$scope.closeEmployeeReconGLListPopUpSelected = function () {
    //    if ($scope.rowSelected !== null) {
    //        angular.element(document.querySelector("#employeeReconGLPopUp")).modal("hide");
    //    } else {
    //        angular.element(document.querySelector("#cancelPopUp")).modal("show");
    //    }
    //};

    //$scope.setemployeeReconGSelected = function (data) {
    //    $scope.advanceDetailList = [];
    //    $scope.advanceDetail.GLGeneralInfoId = data.GLGeneralInfoId;
    //    $scope.advanceDetail.GLGeneralInfoCode = data.GLGeneralInfoCode;
    //    $scope.advanceDetail.GLGeneralInfoName = data.GLGeneralInfoName;
    //    $scope.advanceDetail.BudgetMasterId = data.BudgetMasterId;
    //    $scope.advanceDetail.BudgetCode = data.BudgetCode;
    //    $scope.advanceDetail.BudgetName = data.BudgetName;
    //    $scope.advanceDetail.ActivityId = data.ActivityId;
    //    $scope.advanceDetail.ActivityCode = data.ActivityCode;
    //    $scope.advanceDetail.ActivityName = data.ActivityName;
    //    $scope.advance.ActivityName = data.ActivityName;

    //    $scope.advanceDetail.Narration = $scope.advance.Narration;
    //    $scope.advanceDetail.EmployeeId = $scope.advance.EmployeeId;
    //    $scope.advanceDetail.Amount = $scope.advance.Amount;
    //    $scope.advanceDetailList.push($scope.advanceDetail);
    //    $scope.advanceDetail = {};
    //    $scope.closeEmployeeReconGLListPopUp();
    //};


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