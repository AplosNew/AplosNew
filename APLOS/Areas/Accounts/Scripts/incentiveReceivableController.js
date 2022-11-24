"use strict";
incentiveReceivableController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "bankService", '$window'];
function incentiveReceivableController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, bankService, $window) {
    $rootScope.title = "Incentive Receivable";
    $scope.Action = "Save";
    $scope.url = "Accounts/Incentive";
    $scope.listUrl = $scope.url + "/GetIncentiveReceivableList";
    $scope.postUrl =  "accounts/Invoice/PostIncentiveReceivableInvoice";
    $scope.deleteUrl = "accounts/Invoice/DeleteIncentiveReceivableInvoice";
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $scope.partyType = "Customer";
    $scope.isAdvance = false;
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.IsBaseOnDueDateEnable = true;

    $scope.voucher = {
        Id: null,
        CompanyId: null,
        EntityId: null,
        PlantId: null,
        PartyId: null,
        PartyName: null,
        PartyType: null,
        CurrencyId: null,
        PaymentTermId: null,
        SourceType: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        MatureDate: null,
        DocRefNo: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        TaxYearId: null,
        TaxYearName: null,
        TaxYearPeriodId: null,
        TaxYearPeriodName: null,
        IsExcludingTax: false,
        IsSplit: false,
        Amount: null,
        Narration: null,
        Remarks: null,
        BankName: null,
        BankMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        SourceFrom: null,
        SourceTo: null,
        DrGLId: null,
        DrGLName: null,
        DrBudgetId: null,
        DrBudgetName: null,
        DrActivityId: null,
        DrActivityName: null,
        CrGLId: null,
        CrGLName: null,
        CrBudgetName: null,
        CrBudgetId: null,
        CrActivityId: null,
        CrActivityName: null,
        GLGeneralInfoId: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetName: null,
        ActivityId: null,
        ActivityName: null,
        EmployeeGLGeneralInfoId: null,
        EmployeeGLGeneralInfoName: null,
        EmployeeTransactionTypeId: null,
        PartyPlantId: null,
        DeliveryPartyPlantId: null,
        PaymentSource: "GL",
        CashMasterId: null,
        CompanyCurrencyRate: 1,
        EmployeeName: null,
        EmployeeId: null,
        BeneficiaryType: null,
        EmployeeTransactionTypeId: null,
        IncentiveMasterId: null,
        AccountType: null
    };

    

    baseService.init($scope.listUrl, null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.invoiceList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchInvoiceList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            "name": "Customer Code",
            "value": "PartyCode"
        },
        {
            "name": "Customer",
            "value": "PartyName"
        },
        {
            "name": "Customer Plant",
            "value": "PartyPlantName"
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
        ,
        {
            "name": "Status",
            "value": "Status"
        }
    ];

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;

    });
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });

    $scope.costCenterCboList = [];
    $scope.GetCboCostCenterIdByEntity = function (entityId) {
        $http({
            method: "GET",
            url: "accounts/expenseBooking/GetCboCostCenterIdByEntity?entityId=" + entityId
        }).then(function successCallback(response) {
            $scope.costCenterCboList = response.data;

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
            $scope.currencyExchangeRate = [];
        }
    };

   
    $scope.getIncentivemaster = function () {
        $scope.incentivemasterUrl = "accounts/Incentive/GetCbo";
        $http({
            method: "GET",
            url: $scope.incentivemasterUrl
        }).then(function successCallback(response) {
            $scope.incentivemasterlist = response.data;
        });
    };
    $scope.getIncentivemaster();

    $scope.changePaymentTerm = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) {
                return item.Value === id;
            })[0];
            $scope.voucher.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.voucher.BaseNoOfDays = paymentTerm.NoOfDay;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === "documentdate") {
                    $scope.voucher.BaseOnDueDate = $scope.voucher.DocDate;
                    $scope.IsBaseOnDueDateEnable = true;
                } else if (paymentTerm.BaseLineDate === "postingdate") {
                    $scope.voucher.BaseOnDueDate = $scope.voucher.PostingDate;
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else if (paymentTerm.BaseLineDate === "voucherdate") {
                    $scope.voucher.BaseOnDueDate = $filter("dateFiltering")(Date.now());
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else {
                    $scope.IsBaseOnDueDateEnable = false;
                    $scope.voucher.BaseOnDueDate = $filter("dateFiltering")(Date.now());
                }
            $scope.getMatureDate($scope.voucher.BaseOnDueDate, $scope.voucher.BaseNoOfDays);
        }
    };

    $scope.getMatureDate = function (date, days) {
        if (!baseService.isUndefinedOrNull(date)) {
            date = new Date(date);
            date.setDate(date.getDate() + days);
            $scope.voucher.MatureDate = $filter("date")(date, "dd-MMM-yyyy");
        }
    };

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
        else if (baseService.isUndefinedOrNull($scope.voucher.DocDate)) {
            msg = "Doc Date is required.";
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
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.voucher.PostingDate)) {
            msg = "Posting Date is required.";
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
        $scope.getFiscalInvoiceTotalAmountByParty($scope.voucher.PartyId, $scope.voucher.PostingDate);
    };
    $scope.beneficiaryTypeList = [];

    $scope.getBeneficiaryType = function () {
        $http({
            method: "GET",
            url: "Enum/GetNewBeneficiaryTypeCbo/"
        }).then(function successCallback(response) {
            $scope.beneficiaryTypeList = response.data;
            for (var i = 0; i < $scope.beneficiaryTypeList.length; i++) {
                if ($scope.beneficiaryTypeList[i].Value == 'Customer')
                    $scope.voucher.BeneficiaryType = $scope.beneficiaryTypeList[i].Value;
            }
        });
    };
    $scope.getBeneficiaryType();
    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.IncentiveMasterId)) {
            ShowResult("Please select Incentive Master!", "failure");
                return true;
            }
            if (baseService.isUndefinedOrNull($scope.voucher.CompanyCurrencyRate)) {
                ShowResult("Rate can not Empty!", "failure");
                return true;
            }
        if ($scope.checkedOutBoundInvoiceList.length === 0) {
            ShowResult("Please Add Invoice!", "failure");
                return true;
            }
           
        return false;
    };

    $scope.getCboVoucherTypeReceivableFromOthersList = function () {
        cboService.getCboVoucherTypeReceivableFromOthersList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);
            }
        });
    };
    $scope.getCboVoucherTypeReceivableFromOthersList();

    $scope.Clear = function () {
        var voucherTypeId = $scope.voucher.VoucherId;
        $scope.Action = "Save";
        $scope.voucher = {};
        $scope.getCboVoucherTypeReceivableFromOthersList();
        $scope.voucher.Active = true;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.currencyExchangeRate = [];
        $scope.selectedInvoiceGLId = null;
        $scope.plantList = [];
        $scope.checkedOutBoundInvoiceList = [];
        $scope.voucher.PaymentSource = "GL";
        $scope.voucherDetailId = null;
        $scope.TotalInvoiceAmount = null;
        $scope.CustomerAvailableInvoiceList = [];
        
    };


    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.taxCodCboList = [];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
        $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);
    };

    
    $scope.popUpTDSMessage = false;
    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        if ($scope.form1.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "accounts/Invoice/InsertIncentiveReceivableInvoice",
                    data: {
                        "voucherVM": $scope.voucher,
                        "incentiveReceivableMapList": $scope.checkedOutBoundInvoiceList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.popUpTDSMessage) {

                        }
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            
            return true;
        }
    };

    $scope.report = function (voucherId) {
        location.href = "accounts/invoice/ReportVendorInvoice?voucherId=" + voucherId;
    };

    $scope.post = function (invoiceId) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "invoiceId": invoiceId
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
                $scope.invoiceId = null;
                $scope.type = null;
                
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.invoiceId = null;
    $scope.confirmPost = function (invoiceId, type,tdsId,data) {
        $scope.invoiceId = invoiceId;
        $scope.type = type;
        $scope.tdsId = tdsId;
        $scope.data = data;

        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.delete = function (invoiceId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "invoiceId": invoiceId, "voucherId": voucherId
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
                $scope.invoiceId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.invoiceId = null;
    $scope.confirmDelete = function (invoiceId, voucherId, type, tDSVoucherId, tDSVoucherNo) {
        $scope.invoiceId = invoiceId;
        $scope.voucherId = voucherId;
        $scope.type = type;
        $scope.tDSVoucherId = tDSVoucherId;
        $scope.tDSVoucherNo = tDSVoucherNo;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

    $scope.IncentiveReceivableInvoiceReport = function (reportFormat, voucherId) {
        $window.open('Accounts/Invoice/ReportIncentiveReceivableInvoice?reportFormat=' + reportFormat + '&voucherId=' + voucherId, '_blank');
    }
    $scope.ExpenseDistributionReport = function (reportFormat, voucherId) {
        $window.open('Accounts/Invoice/ReportVendorInvoiceExpenseDistribution?reportFormat=' + reportFormat + '&voucherId=' + voucherId, '_blank');
    }
    
    function checkLCExist(list, InvoiceId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].InvoiceId === InvoiceId) {

                return true;
            }
        }
        return false;
    }
    
    $scope.checkedOutBoundInvoiceList = [];
    $scope.CustomerAvailableInvoiceList = [];
    $scope.showOutBoundInvoicePopUp = function () {
        try {
            $http({
                method: 'GET',
                url: 'accounts/CustomerInvoice/GetCustomerAvailableReceivableData',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.CustomerAvailableInvoiceList = response.data;
                if (baseService.arrayLength($scope.checkedOutBoundInvoiceList) > 0) {
                    for (var i = 0; i < baseService.arrayLength($scope.checkedOutBoundInvoiceList); i++) {
                        for (var j = 0; j < baseService.arrayLength($scope.CustomerAvailableInvoiceList); j++) {
                            if ($scope.checkedOutBoundInvoiceList[i].InvoiceId == $scope.CustomerAvailableInvoiceList[j].InvoiceId) {
                                $scope.CustomerAvailableInvoiceList[j].Active = true;
                            }
                        }
                    }
                }
            });
        } catch (e) {
            throw e;
        }
        angular.element(document.querySelector('#OutBoundInvoicePopUp')).modal('show');
    };
    $scope.hideOutBoundInvoicePopUp = function () {
        angular.element(document.querySelector("#OutBoundInvoicePopUp")).modal("hide");
    };
    
    $scope.TotalInvoiceAmount = 0;
    $scope.getTotalInvoiceAmount = function () {
        $scope.TotalInvoiceAmount = 0;
        if ($scope.activityOrderType == "OutboundInvoice") {
            if (baseService.arrayLength($scope.checkedOutBoundInvoiceList))
               $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        }
    }
    $scope.TotalChargesAmount = 0;
    
    $scope.calOutBoundDistributedAmount = function myfunction() {
        $scope.getTotalInvoiceAmount();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedAmountout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        var tatalout = parseFloat(($scope.TotalDistributedAmountout * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
        }

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount)).toFixed(2);
            $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
            $scope.voucher.Amount= $scope.TotalDistributedInvoiceAmount;
        }
       
    }
   
    $scope.totalBooksAmountCal = function () {
        $scope.OutBoundInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        $scope.totalBooksAmount = parseFloat($scope.OutBoundInvoiceAmount);
        $scope.OutBoundDistributed = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount"));
        $scope.totalDistributedAmount = parseFloat($scope.OutBoundDistributed);
    }
   
    $scope.checkedOutBoundInvoiceList = [];
    $scope.AddIOutBoundInvoice = function () {
        if (baseService.arrayLength($scope.CustomerAvailableInvoiceList) > 0) {
            angular.forEach($scope.CustomerAvailableInvoiceList, function (a) {
                if (checkLCExist($scope.checkedOutBoundInvoiceList, a.InvoiceId) === false) {
                    if (a.Active) {
                        $scope.checkedOutBoundInvoiceList.push({
                            InvoiceId: a.InvoiceId
                            , InvoiceDetailId: a.InvoiceDetailId
                            , Amount: a.Receivable
                            , BooksAmount: a.Receivable * a.CompanyCurrencyRate
                            , DistributedAmount: 0
                            , ChargesAmount: 0
                            , TaxAmount: 0
                            , Active: true
                            , PostingDate: a.PostingDate
                            , PartyPlantName: a.PartyPlantName
                            , CurrencyCode: a.CurrencyCode
                            , VoucherNo: a.VoucherNo
                            , InvoiceType: 'OutboundInvoice'
                            , GLGeneralInfoId: $scope.GLGeneralInfoId
                            , BudgetMasterId: $scope.BudgetMasterId
                            , ActivityId: $scope.ActivityId
                            , DocRefNo: a.DocRefNo
                            , IncentiveMasterId: $scope.voucher.IncentiveMasterId
                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.checkedOutBoundInvoiceList, function (a) {
                if (!baseService.valueCheckInList($scope.checkedOutBoundInvoiceList, 'Id', a.InvoiceId))
                    $scope.checkedOutBoundInvoiceList.splice(a, 1);
            });
        $scope.hideOutBoundInvoicePopUp();
        $scope.calOutBoundDistributedAmount();
        $scope.totalBooksAmountCal();
    };
    
    $scope.DeleteOutBoutConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteOutBoundConfirmationPopUp")).modal("show");
    };
    $scope.RemoveOutBoundInvoice = function () {

        for (var i = 0; i < baseService.arrayLength($scope.checkedOutBoundInvoiceList); i++) {
            if ($scope.checkedOutBoundInvoiceList[i].InvoiceId == $scope.InvoiceId)
                $scope.checkedOutBoundInvoiceList.splice(i, 1);
        }
        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
        }
        $scope.calDistributedAmount();
        $scope.calOutBoundDistributedAmount();
        $scope.totalBooksAmountCal();
    }
    
    $scope.checkDistributedAmount = function myfunction(index, item) {
        $scope.TotalDistributedInvoiceAmount = 0;
        $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
        $scope.voucher.Amount = $scope.TotalDistributedInvoiceAmount;  
    }

}