"use strict";
invoiceOverheadController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "bankService", "$location", "$routeParams", "accountService"];
function invoiceOverheadController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, bankService, $location, $routeParams, accountService) {
    $rootScope.title = "Invoice Overhead";
    $scope.voucherDetailList = [];
    $scope.taxCodDataList = [];
    $scope.ServiceMasterChargesList = [];
    $scope.TaxCopyLisForDisplay = [];
    $scope.Action = "Save";
    $scope.url = "Accounts/Invoice";
    $scope.listUrl = $scope.url + "/GetInvoiceOvereheadList";
    $scope.saveUrl = $scope.url + "/InsertInvoiceOverhead";
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
        CompanyCurrencyRate: 1
    };

    $scope.voucherDetail = {
        EntityId: null,
        InvoiceTaxViewModel: [
            {
                TaxAmount: 0,
                TaxAutoAmount: 0,
                TaxCodeId: null,
                InvoiceDetailId: null,
                InvoiceDetailOppositEntryId: null,
                Id: null,
                WithholdCreditableGL: null,
                ExpensesGL: null,
                CreditableGL: null,
                IsWithhold: null,
                IsCreditable: null,
                IsMerge: null
            }
        ]
    };

    $scope.invoiceTax = {
    };

    $scope.voucherDetailCurrency = {
        Id: null,
        VoucherId: null,
        VoucherDetailId: null,
        ParallelCurrencyId: null,
        FromCurrencyId: null,
        ToCurrencyId: null,
        ToCurrencyRate: null,
        DrAmount: null,
        CrAmount: null,
        TrnType: null
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
            for (var i = 0; i < $scope.invoiceList.length; i++) {
                response.data[i].DocDate = new Date($scope.invoiceList[i].DocDate);
            }
        });
    }
    $scope.getData();


    $scope.GetInvoiceServiceMasterChargesDetail = function (id) {
        $http({
            method: "GET",
            url: $scope.url + "/GetInvoiceServiceMasterChargesDetail?invServiceMasterChargesId=" + id,
        }).then(function successCallback(response) {
            $scope.chargesList = response.data;
        });
    };

    $scope.GetInvoiceServiceMasterChargesTax = function (id) {
        $http({
            method: "GET",
            url: $scope.url + "/GetInvoiceServiceMasterChargesTax?invServiceMasterChargesId=" + id,
        }).then(function successCallback(response) {
           // $scope.TaxCopyLisForDisplay = response.data;
            $scope.TaxCopyList = response.data;
        });
    };

    $scope.getInboundInvoiceDetailCharges = function (id) {
        $http({
            method: "GET",
            url: $scope.url + "/GetInboundInvoiceDetailCharges?invServiceMasterChargesId=" + id,
        }).then(function successCallback(response) {
            $scope.checkedInvoiceList = response.data;
        });
    };

    $scope.getOutboundInvoiceDetailCharges = function (id) {
        $http({
            method: "GET",
            url: $scope.url + "/GetOutBoundInvoiceDetailCharges?invServiceMasterChargesId=" + id,
        }).then(function successCallback(response) {
            $scope.checkedOutBoundInvoiceList = response.data;
        });
    };

    $scope.Get = function (args) {
        $scope.voucher = Object.assign({}, args.data);
        $scope.GetInvoiceServiceMasterChargesDetail($scope.voucher.Id);
        $scope.GetInvoiceServiceMasterChargesTax($scope.voucher.Id);
        $scope.getInboundInvoiceDetailCharges($scope.voucher.Id);
        $scope.getOutboundInvoiceDetailCharges($scope.voucher.Id);
        $scope.getPartyPlantList($scope.voucher.PartyId);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    //baseService.init($scope.listUrl, null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
    //$scope.getData = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.invoiceList = result.Rows;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, "failure");
    //        }).finally(function () {
    //        });
    //};
    //$scope.getData();

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


    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            if (baseService.isUndefinedOrNull(party.ReconciliationGLId)) {
                ShowResult($scope.partyType + " GL not found!", "failure", "partyPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.ReconciliationBudgetId)) {
                ShowResult($scope.partyType + " Budget not found!", "failure", "partyPopUp");
                return;
            }
            else {

                $scope.voucher.PartyId = party.Id;
                $scope.voucher.PartyCode = party.Code;
                $scope.voucher.PartyName = party.UserName;
                $scope.voucher.CurrencyId = party.CurrencyId;
                $scope.voucher.PartyType = $scope.partyType;
                $scope.voucher.GLGeneralInfoId = party.ReconciliationGLId;
                $scope.voucher.GLGeneralInfoCode = party.ReconciliationGLCode;
                $scope.voucher.GLGeneralInfoName = party.ReconciliationGLName;
                $scope.voucher.BudgetMasterId = party.ReconciliationBudgetId;
                $scope.voucher.BudgetCode = party.ReconciliationBudgetCode;
                $scope.voucher.BudgetName = party.ReconciliationBudgetName;
                $scope.voucher.ActivityId = party.ReconciliationActivityId;
                $scope.voucher.ActivityCode = party.ReconciliationActivityCode;
                $scope.voucher.ActivityName = party.ReconciliationActivityName;
                $scope.voucher.PaymentTermId = party.PaymentTermId;
                if ($scope.voucher.PaymentTermId !== null) {
                    $scope.changePaymentTerm($scope.voucher.PaymentTermId);
                }
                $scope.taxCodDataList = [];
                $scope.getPartyPlantList(party.Id);
                $scope.GetCurrencyExchangeRateList();
                clearVoucherDetail();
            }
        }
        $scope.hidePartyPopUp();
    };


    $scope.removeRow = function (index) {
        var row = $scope.voucherDetailList[index];
        $scope.voucherDetailList.splice(index, 1);
        var taxdr = $scope.taxCodDataList.length;
        while (taxdr--) {
            if ($scope.taxCodDataList[taxdr]["VoucherDetailId"] === row.VoucherDetailId) {
                $scope.taxCodDataList.splice(taxdr, 1);
            }
        }
    };



    //Gets data from the Database
    $scope.invoiceGLList = [];
    var glUrl = null;
    if ($scope.partyType === "Customer") {
        glUrl = "accounts/GLItem/GetCustomerInvoiceGLList2";
    }
    else if ($scope.partyType === "Vendor") {
        glUrl = "accounts/GLItem/GetVendorInvoiceGLList";
    }

    $http.get(glUrl)
        .then(
            function successCallback(response) {
                $scope.invoiceGLList = response.data;
            },
            function errorCallback(response) {
                ShowResult(response, "failure");
            });


    $scope.inputChanged = function (str) {
        $scope.voucherDetail.GLGeneralInfoId = str;
    };

    function clearVoucherDetail() {
        $scope.voucherDetail = {};
    }

    $scope.getCboVoucherTypeAccountPayableList = function () {
        cboService.getCboVoucherTypeAccountPayableList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);
            }
        });
    };
    $scope.getCboVoucherTypeAccountPayableList();

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
        $scope.voucherDetail.InvoiceTaxViewModel = [];
        clearVoucherDetail();
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

    $scope.invoiceId = null;
    $scope.confirmPost = function (invoiceId) {
        $scope.invoiceId = invoiceId;
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
    $scope.confirmDelete = function (invoiceId, voucherId) {
        $scope.invoiceId = invoiceId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };



    $scope.clearPartyData = function () {
        $scope.voucher.PartyId = null;
        $scope.voucher.PartyName = null;
        $scope.voucher.PartyPlantId = null;
    }


    $scope.ChangecustomerInvoiceGLSearchList = [
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "COA",
            "value": "COA"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];
    $scope.ChangecustomerInvoiceGLParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ChangeGLpopUp = function (rowdata, index) {
        $scope.customerInvoiceGLList = [];
        baseService.setCurrentPage("ChangecustomerInvoiceGLList");
        $scope.ChangecustomerInvoiceGLGLData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetVendorInvoiceGLBudgetList", pageno, $scope.ChangecustomerInvoiceGLParameters)
                .then(function (result) {
                    $scope.ChangecustomerInvoiceGLList = result.Rows;
                    $scope.ChangecustomerInvoiceGLParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "ChangeInvoiceGLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#ChangeInvoiceGLPopUp")).modal("show");
        $scope.rowindex = index;
        $scope.ChangecustomerInvoiceGLGLData();
    };
    $scope.ChangeglSelect = function (data) {
        $scope.voucherDetailList[$scope.rowindex].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.voucherDetailList[$scope.rowindex].GLGeneralInfoName = data.GLGeneralInfoName;
        $scope.voucherDetailList[$scope.rowindex].GLGeneralInfoCode = data.GLGeneralInfoCode;
        $scope.voucherDetailList[$scope.rowindex].BudgetMasterId = data.BudgetMasterId;
        $scope.voucherDetailList[$scope.rowindex].BudgetName = data.BudgetName;
        $scope.voucherDetailList[$scope.rowindex].BudgetCode = data.BudgetCode;
        $scope.voucherDetailList[$scope.rowindex].ActivityId = data.ActivityId;
        $scope.voucherDetailList[$scope.rowindex].ActivityName = data.ActivityName;
        $scope.voucherDetailList[$scope.rowindex].ActivityCode = data.ActivityCode;
        $scope.ChangecloseGLPopUp();
    };
    $scope.ChangecloseGLPopUp = function () {
        angular.element(document.querySelector("#ChangeInvoiceGLPopUp")).modal("hide");
    };

    $scope.checkedInvoiceList = [];
    $scope.showInvoicePopUp = function () {
        $http({
            method: 'GET',
            url: 'accounts/Invoice/GetVendorAvailableInvoiceList1',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VendorAvailableInvoiceList = response.data;

            if (baseService.arrayLength($scope.checkedInvoiceList) > 0) {
                for (var i = 0; i < baseService.arrayLength($scope.checkedInvoiceList); i++) {
                    for (var j = 0; j < baseService.arrayLength($scope.VendorAvailableInvoiceList); j++) {
                        if ($scope.checkedInvoiceList[i].InvoiceId == $scope.VendorAvailableInvoiceList[j].InvoiceId) {
                            $scope.VendorAvailableInvoiceList[j].Active = true;
                        }
                    }
                }
            }
        });

        angular.element(document.querySelector('#InvoicePopUp')).modal('show');

    };
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



    $scope.hideInvoicePopUp = function () {
        angular.element(document.querySelector("#InvoicePopUp")).modal("hide");
    };
    $scope.hideOutBoundInvoicePopUp = function () {
        angular.element(document.querySelector("#OutBoundInvoicePopUp")).modal("hide");
    };

    $scope.InvoiceList = [];
    $scope.InvoiceModel = {
        InvoiceId: null,
        Amount: 0,
        DistributedAmount: 0
    };

    $scope.SaveInvoiceDetail = function () {
        $http({
            method: 'POST',
            url: 'Commercial/ServiceMasterCharges/SaveInvoiceDetail',
            data: { 'data': $scope.checkedInvoiceList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }

    $scope.VendorAvailableInvoiceList = [];

    //endregion invoiceDetailsCharges

    //#region vendorpayment
    $scope.advanceTaxesList = [];
    $scope.addTax = function () {
        if (manualValidation("td_TaxCode", baseService.isUndefinedOrNull($scope.advanceTax.TaxCodeId), "Tax Code is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeAmount", baseService.isUndefinedOrNull($scope.advanceTax.TaxAmount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.advanceTax.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.advanceTax.TaxName = $.grep($scope.taxCodCboList, function (item) {
                return item.Value === $scope.advanceTax.TaxCodeId;
            })[0].Text;
            $scope.advanceTaxesList.push($scope.advanceTax);
            $scope.advanceTax = {};
        }
    };

    $scope.removeTaxesRow = function (index) {
        $scope.advanceTaxesList.splice(index, 1);
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
    $scope.ChargesList = [];
    $scope.selectServiceMasterCharges = function (args) {
        var temobj = {};
        $scope.ModelCharges = args.data;
        temobj = $scope.ModelCharges;

        temobj = $scope.ModelCharges;
        var getRowDr = $filter("filter")($scope.ChargesList, { "Id": temobj.Id });
        if (getRowDr.length == 0 && temobj.Id != null) {
            temobj.Amount = 0;
            $scope.ChargesList.push(temobj);
        }
        else {
            ShowResult("Data already exist", 'failure');
        }
        $scope.hideServiceMasterChargesPopUp();
    };
    $scope.hideServiceMasterChargesPopUp = function () {
        angular.element(document.querySelector("#ServiceMasterPopUp")).modal("hide");
    };

    $scope.ModelCharges = { OverHeadTypeId: null, Charges: null, Amount: 0 };

    //#endregion
    $scope.invoiceDetailChargesList = [];
    $scope.InvoiceDetailChargesList = function myfunction() {
        $scope.invoiceDetailChargesList = $scope.checkedInvoiceList.concat($scope.checkedOutBoundInvoiceList);
    };


    $scope.InsertInvoiceOverhead = function () {
        $scope.InvoiceDetailChargesList();
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        //if ($scope.form1.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
        if ($scope.Action === "Save") {
            $http({
                method: "POST",
                url: "accounts/Invoice/InsertInvoiceOverhead",
                data: {
                    "voucherVM": $scope.voucher,
                    "voucherDetailVMList": $scope.chargesList,
                    "taxDetailVMList": $scope.TaxCopyList,
                    "invoiceDetailChargesList": $scope.invoiceDetailChargesList
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
                url: "accounts/Invoice/InsertInvoiceOverhead",
                data: {
                    "voucherVM": $scope.voucher,
                    "voucherDetailVMList": $scope.chargesList,
                    "taxDetailVMList": $scope.TaxCopyList,
                    "invoiceDetailChargesList": $scope.invoiceDetailChargesList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.Clear();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        return true;
        //}
    };


    //#region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
        $scope.CheckIsNonCreditable();
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.voucher.IsNonCreditable = false;
    $scope.CheckIsNonCreditable = function () {
        if ($scope.voucher.IsNonCreditable == true) {
            if ($scope.tab == 2) {

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    $scope.checkedInvoiceList[i].DistributedAmount = 0;
                }
                $scope.calDistributedAmountNonCreditable();
            }
            else if ($scope.tab == 3) {
                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
                }
                $scope.calOutBoundDistributedAmountNonCreditable();
            }
        }
        else {

            if ($scope.tab == 2) {
                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    $scope.checkedInvoiceList[i].DistributedAmount = 0;
                }
                $scope.calDistributedAmount();
            }
            else if ($scope.tab == 3) {
                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
                }
                $scope.calOutBoundDistributedAmount();
            }
        }
        $scope.totalBooksAmountCal();
    }

    $scope.getTotalInvoiceAmount = function () {
        $scope.TotalInvoiceAmount = 0;
        if (baseService.arrayLength($scope.checkedInvoiceList) > 0)
            $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        if (baseService.arrayLength($scope.checkedOutBoundInvoiceList))
            $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
    }

    function controllDistribution(funName) {
        if ($scope.tab == 2 && funName == "calDistributedAmount" && $scope.voucher.IsNonCreditable == false) {
            $scope.calOutBoundDistributedAmount();
        }
        else if ($scope.tab == 2 && funName == "calDistributedAmount" && $scope.voucher.IsNonCreditable == true) {
            $scope.calOutBoundDistributedAmountNonCreditable();
        }
        else if ($scope.tab == 3 && funName == "calOutBoundDistributedAmount" && $scope.voucher.IsNonCreditable == false) {
            $scope.calDistributedAmount();
        }
        else if ($scope.tab == 3 && funName == "calOutBoundDistributedAmount" && $scope.voucher.IsNonCreditable == true) {
            $scope.calDistributedAmountNonCreditable();

        }
        else if ($scope.tab == 2 && funName == "calDistributedAmountNonCreditable" && $scope.voucher.IsNonCreditable == true) {
            $scope.calOutBoundDistributedAmountNonCreditable();
        }
        else if ($scope.tab == 3 && funName == "calOutBoundDistributedAmountNonCreditable" && $scope.voucher.IsNonCreditable == true) {
            $scope.calDistributedAmountNonCreditable();
        }
    }

    $scope.calDistributedAmountNonCreditable = function myfunction() {
        $scope.TotalChargesAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TransactionAmount"));
        $scope.TotalTaxAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TotalTaxAmount"));

        //$scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        $scope.getTotalInvoiceAmount();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedAmountInvoiceNonCreditable = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        var totalNonCreditable = parseFloat(($scope.TotalDistributedAmountInvoiceNonCreditable * ($scope.TotalChargesAmount + $scope.TotalTaxAmount)) / $scope.TotalInvoiceAmount);

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            if ($scope.checkedInvoiceList.length == 1) {
                $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(($scope.checkedInvoiceList[i].BooksAmount * (parseFloat($scope.TotalChargesAmount) + parseFloat($scope.TotalTaxAmount))) / $scope.TotalInvoiceAmount).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount");
            }
            else {
                if ($scope.checkedInvoiceList.length - 1 == i) {

                    $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount");

                    $scope.checkedInvoiceList[i].DistributedAmount = totalNonCreditable - $scope.TotalDistributedInvoiceAmount;
                }
                else {
                    $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(($scope.checkedInvoiceList[i].BooksAmount * (parseFloat($scope.TotalChargesAmount) + parseFloat($scope.TotalTaxAmount))) / $scope.TotalInvoiceAmount).toFixed(2);
                }
            }
        }
        controllDistribution("calDistributedAmountNonCreditable");
    }

    $scope.calDistributedAmount = function myfunction() {
        $scope.TotalChargesAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TransactionAmount"));

        $scope.TotalTaxAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TotalTaxAmount"));
        $scope.getTotalInvoiceAmount();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedAmountInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        var totali = parseFloat(($scope.TotalDistributedAmountInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            if ($scope.checkedInvoiceList.length == 1) {
                $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
            }
            else {
                if ($scope.checkedInvoiceList.length - 1 == i) {

                    $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    $scope.checkedInvoiceList[i].DistributedAmount = totali - $scope.TotalDistributedInvoiceAmount;
                }
                else {
                    $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                }
            }
        }
        controllDistribution("calDistributedAmount");
    }

    $scope.calOutBoundDistributedAmountNonCreditable = function myfunction() {
        $scope.TotalChargesAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TransactionAmount"));

        $scope.TotalTaxAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TotalTaxAmount"));


        $scope.getTotalInvoiceAmount();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributeAmountInvoiceNonCreditable = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        var tatalout = parseFloat(($scope.TotalDistributeAmountInvoiceNonCreditable * ($scope.TotalChargesAmount + $scope.TotalTaxAmount)) / $scope.TotalInvoiceAmount);

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

            if ($scope.checkedOutBoundInvoiceList.length == 1) {
                $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(($scope.checkedOutBoundInvoiceList[i].BooksAmount * (parseFloat($scope.TotalChargesAmount) + parseFloat($scope.TotalTaxAmount))) / $scope.TotalInvoiceAmount).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
            }
            else {
                if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                    $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = tatalout - $scope.TotalDistributedInvoiceAmount;
                }
                else {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(($scope.checkedOutBoundInvoiceList[i].BooksAmount * (parseFloat($scope.TotalChargesAmount) + parseFloat($scope.TotalTaxAmount))) / $scope.TotalInvoiceAmount).toFixed(2);
                }
            }
        }
        controllDistribution("calOutBoundDistributedAmountNonCreditable");
    }

    $scope.calOutBoundDistributedAmount = function myfunction() {
        $scope.TotalChargesAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TransactionAmount"));

        $scope.TotalTaxAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TotalTaxAmount"));
        //$scope.TotalInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "Amount");
        $scope.getTotalInvoiceAmount();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedAmountout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        var tatalout = parseFloat(($scope.TotalDistributedAmountout * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

            if ($scope.checkedOutBoundInvoiceList.length == 1) {
                $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
            }
            else {
                if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                    $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = tatalout - $scope.TotalDistributedInvoiceAmount;
                }
                else {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                }
            }
        }
        controllDistribution("calOutBoundDistributedAmount");
    }

    function checkLCExist(list, InvoiceId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].InvoiceId === InvoiceId) {

                return true;
            }
        }
        return false;
    }
    $scope.AddInvoice = function () {

        if (baseService.arrayLength($scope.VendorAvailableInvoiceList) > 0) {
            angular.forEach($scope.VendorAvailableInvoiceList, function (a) {
                if (checkLCExist($scope.checkedInvoiceList, a.InvoiceId) == false) {
                    if (a.Active) {
                        $scope.checkedInvoiceList.push({
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
                            , InvoiceType: 'InBound'
                        });
                    }
                }
            });
        }

        $scope.hideInvoicePopUp();
        $scope.calDistributedAmount();
        $scope.totalBooksAmountCal();
    };

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
                            , InvoiceType: 'OutBound'
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

    $scope.RemoveInvoice = function () {

        for (var i = 0; i < baseService.arrayLength($scope.checkedInvoiceList); i++) {
            if ($scope.checkedInvoiceList[i].InvoiceId == $scope.InvoiceId)
                $scope.checkedInvoiceList.splice(i, 1);
        }

        $scope.CheckIsNonCreditable();

    }
    $scope.RemoveOutBoundInvoice = function () {

        for (var i = 0; i < baseService.arrayLength($scope.checkedOutBoundInvoiceList); i++) {
            if ($scope.checkedOutBoundInvoiceList[i].InvoiceId == $scope.InvoiceId)
                $scope.checkedOutBoundInvoiceList.splice(i, 1);
        }

        $scope.CheckIsNonCreditable();
        //$scope.calOutBoundDistributedAmount();
    }
    $scope.serviceChargePopUp = function () {

        $scope.serviceModel = {
            Id: null
            , ServiceMasterId: null
            , InventoryReceiveId: $scope.voucher.Id
            , CurrencyName: angular.element("#currency :selected").text()
            , CurrencyId: $scope.voucher.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.voucher.DocDate
            , TransactionAmount: 0
            , BaseAmount: 0
            , TotalTaxAmount: 0
            , ToCurrencyRate: $scope.voucher.ToCurrencyRate
            , IsNonCreditable: $scope.voucher.IsNonCreditable
        };
        angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
    };

    //$scope.getServiceMasterCharges = function () {
    //    $scope.ServiceMasterChargesList = [];
    //    $http({
    //        method: 'POST',
    //        url: 'Commercial/ServiceMasterCharges/GetServiceMasterCharges',
    //        data: { column: $scope.searchBy, value: $scope.search },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.ServiceMasterChargesList = response.data;
    //    });
    //}
   // $scope.getServiceMasterCharges();

    $scope.ServiceMasterChargesList = [];
    $http.get('Commercial/OverHeadType/GetServiceCharges')
        .then(function (response) {
            $scope.ServiceMasterChargesList = response.data;
        });


    $scope.closeServiceChargePopUp = function () {

        $scope.serviceModel = {};
        $scope.receiveTaxList = [];
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    };

    $scope.changeService = function () {
        if (baseService.isUndefinedOrNull($scope.serviceModel.OverHeadTypeId))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.ServiceMasterChargesList, function (item) { return item.Id === $scope.serviceModel.OverHeadTypeId; })[0].HSNCodeId;
        $scope.ServiceMasterCharges = $.grep($scope.ServiceMasterChargesList, function (item) { return item.Id === $scope.serviceModel.OverHeadTypeId; })[0].UserName;
        $scope.ServiceMasterChargesGL = $.grep($scope.ServiceMasterChargesList, function (item) { return item.Id === $scope.serviceModel.OverHeadTypeId; })[0].PurchaseGLGeneralInfoId;
        $scope.ServiceMasterChargesBudget = $.grep($scope.ServiceMasterChargesList, function (item) { return item.Id === $scope.serviceModel.OverHeadTypeId; })[0].PurchaseBudgetMasterId;
        $scope.ServiceMasterChargesActivity = $.grep($scope.ServiceMasterChargesList, function (item) { return item.Id === $scope.serviceModel.OverHeadTypeId; })[0].PurchaseActivityId;
        getTaxCategoryList(hsnCodeId);
    };

    function getTaxCategoryList(hsnCodeId) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: 'Products/GoodsReceiveNote/GetTaxCategoryListByPartyPlant?partyPlantId=' + $scope.voucher.PartyPlantId + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
        });
    }

    $scope.calculateSvcTaxCategory = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
    };
    $scope.sumSvcTaxAmount = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
    };
    $scope.chargesList = [];
    $scope.obj = {
        OverHeadTypeId: null
        , ServiceMasterCharges: null
        , TransactionAmount: null
        , TotalTaxAmount: null
        , PurchaseGLGeneralInfoId: null
        , PurchaseBudgetMasterId: null
        , PurchaseActivityId: null
    };
    $scope.TaxCopyList = [];
    $scope.AddCharges = function () {
        $scope.obj = {
            OverHeadTypeId: $scope.serviceModel.OverHeadTypeId
            , ServiceMasterCharges: $scope.ServiceMasterCharges
            , PurchaseGLGeneralInfoId: $scope.ServiceMasterChargesGL
            , PurchaseBudgetMasterId: $scope.ServiceMasterChargesBudget
            , PurchaseActivityId: $scope.ServiceMasterChargesActivity
            , TransactionAmount: $scope.serviceModel.TransactionAmount
            , TotalTaxAmount: $scope.serviceModel.TotalTaxAmount
        };

        var temobj = $scope.obj;
        var getRowDr = $filter("filter")($scope.chargesList, { "OverHeadTypeId": temobj.OverHeadTypeId });
        if (getRowDr.length == 0 && temobj.OverHeadTypeId != null) {

            $scope.chargesList.push($scope.obj);
            $scope.ServiceMasterChargesGL = null;
            $scope.ServiceMasterChargesBudget = null;
            $scope.ServiceMasterChargesActivity = null;
            for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {

                var taxObj = Object.assign({}, $scope.taxCategoryList[i])
                taxObj.OverHeadTypeId = $scope.serviceModel.OverHeadTypeId;
                $scope.TaxCopyList.push(taxObj);
            }

            $scope.closeServiceChargePopUp();
            $scope.taxCategoryList = [];

        }
        else {
            ShowResult("Data already exist", 'failure');
        }

        $scope.calDistributedAmount();
        $scope.calOutBoundDistributedAmount();
    }

    $scope.RemoveCharges = function () {
        for (var i = 0; i < baseService.arrayLength($scope.chargesList); i++) {
            if ($scope.chargesList[i].OverHeadTypeId == $scope.OverHeadTypeId)
                $scope.chargesList.splice(i, 1);
        }

        for (var i = 0; i < baseService.arrayLength($scope.TaxCopyList); i++) {
            if ($scope.TaxCopyList[i].OverHeadTypeId == $scope.OverHeadTypeId) {
                $scope.TaxCopyList.splice(i, 1);
            }
        }

        $scope.calDistributedAmount();
    }
    //#endregion
    $scope.updateTaxCategoryList = [];
    accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
        $scope.updateTaxCategoryList = result;
    });

    $scope.showServiceChargesInfoPopUp = function (index) {
        var rowdata = $scope.chargesList[index];
        $scope.OverHeadTypeId = rowdata.OverHeadTypeId;
        $scope.TaxCopyLisForDisplay = [];
        for (var i = 0; i < baseService.arrayLength($scope.TaxCopyList); i++) {
            if ($scope.TaxCopyList[i].OverHeadTypeId == $scope.OverHeadTypeId) {
                $scope.TaxCopyLisForDisplay.push($scope.TaxCopyList[i]);
            }
        }
        angular.element(document.querySelector("#serviceChargesPopUp")).modal("show");
    }
    $scope.AddTaxRow = function () {
        var tax = {
            Id: null,
            TaxCategoryId: null,
            HSNCodeId: null,
            UserName: null,
            Percentage: null,
            TotalAmount: 0,
            TaxAmount: 0,
            OverHeadTypeId: $scope.OverHeadTypeId
        };

        $scope.TaxCopyLisForDisplay.push(tax);
    }
    $scope.onchangeTaxCategory = function (index) {
        var rowdata = $scope.TaxCopyLisForDisplay[index];
        var getRow = $filter("filter")($scope.TaxCopyList, { "TaxCategoryId": rowdata.TaxCategoryId });
        if (getRow.length == 0) {
            $scope.TaxCopyList.push($scope.TaxCopyLisForDisplay[index]);
        }
        else {
            $scope.TaxCopyLisForDisplay.splice(index, 1);
            ShowResult("TaxCatagory  already exist", 'failure','serviceChargesPopUp');
        }

    }
    $scope.RemoveTaxRow = function (index) {
        $scope.TaxCopyLisForDisplay.splice(index, 1);
    }

    $scope.closeServiceChargesInfoPopUp = function () {

        var TotalTaxAmount = $scope.TotalTaxAmount = $filter("sumByKey")($filter("filter")($scope.TaxCopyLisForDisplay), "TaxAmount");

        for (var i = 0; i < baseService.arrayLength($scope.chargesList); i++) {
            if ($scope.chargesList[i].OverHeadTypeId == $scope.OverHeadTypeId) {
                $scope.chargesList[i].TotalTaxAmount = TotalTaxAmount;
            }
        }

        angular.element(document.querySelector("#serviceChargesPopUp")).modal("hide");
    }

    $scope.DeleteConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteConfirmationPopUp")).modal("show");
    };

    $scope.DeleteOutBoutConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteOutBoundConfirmationPopUp")).modal("show");
    };

    $scope.OverHeadTypeId = "";
    $scope.DeleteChargesonfirmation = function (x) {
        $scope.OverHeadTypeId = x.OverHeadTypeId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteChargesConfirmationPopUp")).modal("show");
    };

    $scope.invoicingPartyPopUp = function () {
        getPartyPlantList();

        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closeInvoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
    };
    function getPartyPlantList() {
        $scope.plantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.voucher.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.IsDefault) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = item.Value;
                    $scope.productNew.InvoicingByAddress = item.Address1;
                    $scope.productNew.DeliveryByAddress = item.Address1;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = item.StateName;
                    $scope.productNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }

    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateName;
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = state;
                $scope.productNew.InvoicingGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.productNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.productNew.DeliveryState = state;
                $scope.productNew.DeliveryGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.productNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = null;
                $scope.productNew.InvoicingGSTIN = null;
                return $scope.productNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.productNew.DeliveryState = null;
                $scope.productNew.DeliveryGSTIN = null;
                return $scope.productNew.DeliveryByAddress = null;
            }
        }
    };

    $scope.totalBooksAmount = 0;
    $scope.totalDistributedAmount = 0;
    $scope.totalBooksAmountCal = function () {
        $scope.InBoundInvoiceAmount = 0; $scope.OutBoundInvoiceAmount = 0;
        $scope.InBoundDistributed = 0; $scope.OutBoundDistributed = 0;
        $scope.InBoundInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        $scope.OutBoundInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        $scope.totalBooksAmount = parseFloat($scope.InBoundInvoiceAmount + $scope.OutBoundInvoiceAmount)

        $scope.InBoundDistributed = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
        $scope.OutBoundDistributed = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount"));
        $scope.totalDistributedAmount = parseFloat($scope.InBoundDistributed + $scope.OutBoundDistributed)
    }
}