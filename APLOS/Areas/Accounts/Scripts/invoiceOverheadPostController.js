"use strict";
invoiceOverheadPostController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "bankService", "$location", "$routeParams", '$window'];
function invoiceOverheadPostController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, bankService, $location, $routeParams, $window) {
    $rootScope.title = "Invoice Overhead";
    $scope.voucherDetailList = [];
    $scope.taxCodDataList = [];
    $scope.ServiceMasterChargesList = [];
    $scope.Action = "Save";
    $scope.url = "Accounts/Invoice";
    $scope.listUrl = $scope.url + "/GetInvoiceOvereheadPostingList";
    $scope.postedUrl = $scope.url + "/GetInvoiceOvereheadPostedList";
    $scope.saveUrl = $scope.url + "/PostInvoiceOverhead";
    $scope.updateUrl = $scope.url + "/UpdateVendorInvoice";
    $scope.reportUrl = $scope.url + "/ReportCustomerAdvance?voucherId=";
    $scope.jouranlUrl = $scope.url + "/GetAvailableJournalCustomerAdvance";
    $scope.postUrl = $scope.url + "/PostVendorInvoice";
    $scope.deleteUrl = $scope.url + "/DeleteVendorInvoice";
    $scope.searchBy = "UserName"; $scope.search = "";

    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.partyType = "Vendor";
    $scope.isAdvance = false;
    $controller("partyBaseController", { $scope: $scope, $http: $http });

    $scope.partyGLList = [];
    $scope.taxExemption = false;
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
        CompanyCurrencyRate:1
    };

  

   

    $scope.invoiceList = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.listUrl,
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.invoiceList = response.data;
            console.log('invoiceList',$scope.invoiceList);
        });
    }
    $scope.getData();

    $scope.invoicePostedList = [];
    $scope.getPostedData = function () {
        $http({
            method: 'GET',
            url: $scope.postedUrl,
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.invoicePostedList = response.data;
            console.log('invoicePostedList', $scope.invoicePostedList);
        });
    }
    $scope.getPostedData();


    $scope.GetInvoiceServiceMasterChargesDetail = function (id) {
        $http({
            method: "GET",
            url: $scope.url + "/GetInvoiceServiceMasterChargesDetailPosting?invServiceMasterChargesId=" + id,
        }).then(function successCallback(response) {
            $scope.chargesList = response.data;
        });
    };

    $scope.GetInvoiceServiceMasterChargesTax = function (id) {
        $http({
            method: "GET",
            url: $scope.url + "/GetInvoiceServiceMasterChargesTax?invServiceMasterChargesId=" + id,
        }).then(function successCallback(response) {
            $scope.taxCategoryList = response.data;
        });
    };


    $scope.Get = function (args) {
        $scope.voucher = Object.assign({}, args.data);
        $scope.GetInvoiceServiceMasterChargesDetail($scope.voucher.Id);
        $scope.GetInvoiceServiceMasterChargesTax($scope.voucher.Id);
        $scope.getCboVoucherTypeAccountPayableList();
        $scope.Action = 'Post';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

   
    $scope.searchInvoiceList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            "name": "Vendor Code",
            "value": "PartyCode"
        },
        {
            "name": "Vendor Name",
            "value": "PartyName"
        },
        {
            "name": "Ordering Vendor",
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
    ];

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, "", function (result) {
                $scope.entityList = result;
            });
    });

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

    bankService.getCashMasterCboList(function (result) {
        $scope.cashMasterList = result;
    });

    $scope.paymentTerm = function () {
        if ($scope.partyType === "Customer")
            $scope.paymenttermUrl = "accounts/PaymentTerm/getcustomercbo";
        else if ($scope.partyType === "Vendor")
            $scope.paymenttermUrl = "accounts/PaymentTerm/getvendorcbo";
        $http({
            method: "GET",
            url: $scope.paymenttermUrl
        }).then(function successCallback(response) {
            $scope.paymentTermList = response.data;
        });
    };
    $scope.paymentTerm();

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
    };

   
    //Gets data from the Database
   

    $scope.getCboVoucherTypeAccountPayableList = function () {
        cboService.getCboVoucherTypeAccountPayableList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.getTaxCodeByTaxYear($scope.voucher.DocDate);
            }
        });
    };
    

    $scope.Clear = function () {
        var voucherTypeId = $scope.voucher.VoucherId;
        $scope.Action = "Save";
        $scope.voucher = {};
        $scope.getCboVoucherTypeAccountPayableList();
        $scope.voucher.Active = true;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
        $scope.taxCodDataList = [];
        $scope.invoiceDetailChargesList = [];
        $scope.TaxCopyList = [];
        $scope.chargesList = [];
        $scope.checkedInvoiceList = [];
        $scope.checkedOutBoundInvoiceList = [];
        $scope.selectedInvoiceGLId = null;
        $scope.plantList = [];
        $scope.GLGeneralInfoName = null;
        $scope.BudgetItemList = [];
        $scope.ActivityList = [];
        $scope.voucher.PaymentSource = "GL";
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

    $scope.changePostingGetTaxCode = function () {
        $scope.taxCodCboList = [];
        $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);
    }
    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYear = function (date) {
        $http({
            method: "get",
            url: "accounts/TaxCode/GetCboInput?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message;
                }
                else {
                    var result = response.data;
                    $scope.taxCodCboList = result;
                    //console.log("taxCodCboList", $scope.taxCodCboList);
                    if ($scope.taxCodCboList.length === 0) {
                        //$scope.pop("error", "No TaxCode found in this Fiscal Year ");
                    }
                }
            },
            function errorCallback(response) {
            });
    };

    $scope.ontaxCodeChange = function (item) {
        $http({
            method: "get",
            url: "accounts/taxcode/GetTaxCodeById?id=" + item
        }).then(function successCallback(response) {
            $scope.taxcodedata = response.data;
        });
    };

    $scope.voucherDetailId = "";
    $scope.getTaxCode = function (index, voucherDetailId) {
        $scope.setTaxVoucherDetailIndex = index;
        $scope.voucherDetailId = voucherDetailId;
        angular.element(document.querySelector("#texCodePopUp")).modal("show");
    };

    $scope.closeTaxCodePopUp = function () {
        angular.element(document.querySelector("#texCodePopUp")).modal("hide");
    };



    $scope.taxCodeDelModal = function (taxcodeid, vdid, username) {
        $scope.TaxCodeId = taxcodeid;
        $scope.voucherDetailId = vdid;

        if (baseService.isUndefinedOrNull($scope.TaxCodeId))
            $scope.Taxmessage_confirmation = "Are you sure want to delete [ " + username + " ] data....";
        else
            $scope.Taxmessage_confirmation = "Are you sure want to delete [ " + username + " ] ?";
        angular.element(document.querySelector("#confirmTaxCodeDelPopUp")).modal("show");
    };

   
    $scope.vendorInvoiceTaxes = [];
    $scope.vendorInvoiceTaxPush = function () {
        $scope.closeTaxCodePopUp();
    };

    $scope.checkTaxAllow = function () {
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if ($scope.voucherDetailList[i].TrnType === "Dr" && $scope.voucherDetailList[i].PostingWithoutTaxAllow === false) {
                ShowResult("PositionWithout Tax is not Allow where Amount " + $scope.voucherDetailList[i].Amount, "failure");
                return false;
            }
        }
        return true;
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
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };



  
    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYear = function (date) {
        $http({
            method: "get",
            url: "accounts/TaxCode/GetAdditionalTaxCbo?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message;
                }
                else {
                    var result = response.data;
                    $scope.taxCodCboList = result;
                }
            },
            function errorCallback(response) {
            });
    };

    $scope.getTaxCodeByTaxYear($filter("dateFiltering")(Date.now()));
   
    $scope.removeChargesRow = function (index) {
        $scope.ChargesList.splice(index, 1);
    };

    $scope.InsertInvoiceOverhead = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
            if ($scope.Action === "Post") {
                $http({
                    method: "POST",
                    url: "accounts/Invoice/PostInvoiceOverhead",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.chargesList,
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.getPostedData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            return true;
    };


    //#region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };


    $scope.onClickReportDownloadWord = function (args) {        debugger;        var gridObj = $("#GridPrint").data("ejGrid");        //getting corresponding record         var data = gridObj.getSelectedRecords()[0];        var reportFormat = "Pdf";        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');        $window.open('Accounts/Invoice/ReportInvoiceOverheadVoucher?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');
    };    $scope.commandPDF = [{        type: "details", buttonOptions: {            text: "PDF",            width: "50",            height: "20",            click: $scope.onClickReportDownloadWord        }    }];

    $scope.onClickReportDownloadExcel = function (args) {        debugger;        var gridObj = $("#GridPrint").data("ejGrid");        var data = gridObj.getSelectedRecords()[0];        var reportFormat = "Excel";        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');        $window.open('Accounts/Invoice/ReportInvoiceOverheadVoucher?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');
    };
    $scope.commandExcel = [{        type: "details", buttonOptions: {            text: "Excel",            width: "50",            height: "20",            click: $scope.onClickReportDownloadExcel        }    }];


}