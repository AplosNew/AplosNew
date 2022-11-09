"use strict";
expensesCapitalizedController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "bankService", "$window"];
function expensesCapitalizedController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, bankService,$window) {
    $rootScope.title = "Expenses Capitalize";
    $scope.voucherDetailList = [];
    $scope.taxCodDataList = [];
    $scope.Action = "Save";
    $scope.url = "FixedAssets/FixedAssetRegister";
    $scope.listUrl = $scope.url + "/GetVendorInvoiceList";
    $scope.saveUrl = $scope.url + "/InsertExpensesCapitalizeJournal";
    $scope.updateUrl = $scope.url + "/UpdateVendorInvoice";
    $scope.reportUrl = $scope.url + "/ReportCustomerAdvance?voucherId=";
    $scope.jouranlUrl = $scope.url + "/GetAvailableJournalCustomerAdvance";
    $scope.postUrl = $scope.url + "/PostVendorInvoice";
    $scope.deleteUrl = $scope.url + "/DeleteVendorInvoice";

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
        CompanyCurrencyRate: 1,
        FixedAssetName:null
    };

    $scope.voucherDetail = {
        EntityId: null,
        FixedAssetName: null,
        FixedAssetasterId: null,
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
                IsMerge: null,
                
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

    $scope.searchByPostedGRN = "Id"; $scope.searchGRN = "";
    $scope.searchByPostedGRNList = [ { value: 'VoucherNo', name: "VoucherNo" }
        , { value: 'PostingDate', name: "PostingDate" }, { value: 'DocRefNo', name: "DocRef No" }
        , { value: 'DocDate', name: "Doc Date" }];

    $scope.products = [];
    $scope.getDataList = function () {
        $http({
            method: 'POST',
            url: 'FixedAssets/FixedAssetRegister/GetExpensesCapitalizedList',
            data: { column: $scope.searchByPostedGRN, value: $scope.searchGRN },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.products = response.data;
        });
    };
    $scope.getDataList();

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
           
    });
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
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

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
                ShowResult("Please select Currency!", "failure");
                return true;
            }
            if (baseService.isUndefinedOrNull($scope.voucher.CompanyCurrencyRate)) {
                ShowResult("Rate can not Empty!", "failure");
                return true;
            }

            if ($scope.voucher.PaymentSource === "GL") {
                var vdetailCr = $filter("filter")($scope.voucherDetailList, { TrnType: "Cr" });
                if (vdetailCr.length === 0) {
                    ShowResult("Please add Expenses!", "failure");
                    return true;
                }
                else {
                    for (var j = 0; j < vdetailCr.length; j++) {
                        if (vdetailCr[j].CrAmount === 0 || vdetailCr[j].CrAmount === null) {
                            ShowResult(vdetailCr[j].GLGeneralInfoName + " Amount must greater than 0!", "failure");
                            return true;
                        }
                    }
                }
            }
            else {
                ShowResult("Please select Source!", "failure");
                return true;
            }
        return false;
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
                $scope.removeDrRow();
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


    $scope.addRow = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return true;
        }
        //if (baseService.isUndefinedOrNull($scope.selectedInvoiceGLId)) {
        //    ShowResult("Please select GL.", "failure");
        //    return;
        //}
        var getRow = null;
        //if ($scope.partyType === "Vendor") {
        //    if ($scope.companyConfig.IsVoucherFromBudget) {
        //        getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "GLGeneralInfoId": $scope.selectedInvoiceGLId, "BudgetMasterId": $scope.voucherDetail.BudgetMasterId, "ActivityId": $scope.voucherDetail.ActivityId });
        //    }
        //    else
        //        getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "GLGeneralInfoId": $scope.selectedInvoiceGLId });
        //}

        if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].GLGeneralInfoId === $scope.selectedInvoiceGLId) {
            ShowResult("This GL Budget and Activity is already added!", "failure");
        }
        else {
            $scope.voucherDetail.Id = baseService.pk();
            $scope.voucherDetail.GLGeneralInfoId = $scope.selectedInvoiceGLId;
            $scope.voucherDetail.GLGeneralInfoCode = $scope.selectedInvoiceGLCode;
            $scope.voucherDetail.GLGeneralInfoName = $scope.selectedInvoiceGLName;
            $scope.voucherDetail.PostingWithoutTaxAllow = $scope.selectedInvoiceGLPostingWithoutTaxAllow;
            $scope.voucherDetail.FixedAssetName = null;
            $scope.voucherDetail.FixedAssetMasterId = null;
            $scope.voucherDetail.DocDate = $scope.voucher.DocDate;
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
            $scope.voucherDetail.CrAmount = null;
            $scope.voucherDetail.DrAmount = null;
            $scope.voucherDetail.TotalTax = null;
            $scope.voucherDetail.TotalAmount = null;

            $scope.voucherDetail.TrnType = "Cr";
            $scope.voucherDetail.InvoiceTaxViewModel = [];
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            clearVoucherDetail();
            $scope.searchStr = null;
        }
    };
   
  
    $scope.removeDrRow = function () {
        var dr = $scope.voucherDetailList.length;
        while (dr--) {
            if ($scope.voucherDetailList[dr]["TrnType"] === "Dr") {
                $scope.voucherDetailList.splice(dr, 1);
            }
        }
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

    $scope.selectedInvoiceGLId = null;
    $scope.selectedInvoiceGLName = null;
    $scope.selectedInvoiceGLPostingWithoutTaxAllow = null;
    $scope.selectedInvoiceGL = function (selected) {
        if (selected) {
            $scope.selectedInvoiceGLId = selected.originalObject.GLGeneralInfoId;
            $scope.selectedInvoiceGLName = selected.originalObject.GLGeneralInfoName;
            $scope.selectedInvoiceGLPostingWithoutTaxAllow = selected.originalObject.PostingWithoutTaxAllow;
            if ($scope.companyConfig.IsVoucherFromBudget) {
                $scope.getBudgetCboByGL(selected.originalObject.GLGeneralInfoId);
            }
        }
    };


    function clearVoucherDetail() {
        $scope.voucherDetail = {};
    }
    $scope.voucherTypeList = [];
    cboService.getCboVoucherTypeFixedAssetCapitalizeJournalList(function (result) {
        $scope.voucherTypeList = result;
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
    });

    $scope.Clear = function () {
        var voucherTypeId = $scope.voucher.VoucherId;
        $scope.Action = "Save";
        $scope.voucher = {};
        $scope.voucher.Active = true;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
        $scope.voucher.PaymentSource = "GL";
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


    $scope.voucherDetailId = "";
  
    $scope.customerInvoiceGLSearchList = [
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
    $scope.customerInvoiceGLParameters = {
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
    $scope.popUp = function () {
        $scope.customerInvoiceGLList = [];
        baseService.setCurrentPage("customerInvoiceGLList");
        $scope.customerInvoiceGLGLData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetExpenseTypeGLBudgetActivityList", pageno, $scope.customerInvoiceGLParameters)
                .then(function (result) {
                    $scope.customerInvoiceGLList = result.Rows;
                    $scope.customerInvoiceGLParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "CustomerInvoiceGLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#CustomerInvoiceGLPopUp")).modal("show");
        $scope.customerInvoiceGLGLData();
    };
    $scope.glSelect = function (data) {
        $scope.selectedInvoiceGLId = data.GLGeneralInfoId;
        $scope.selectedInvoiceGLName = data.GLGeneralInfoName;
        $scope.selectedInvoiceGLCode = data.GLGeneralInfoCode;
        $scope.voucherDetail.BudgetMasterId = data.BudgetMasterId;
        $scope.voucherDetail.BudgetName = data.BudgetName;
        $scope.voucherDetail.BudgetCode = data.BudgetCode;
        $scope.voucherDetail.ActivityId = data.ActivityId;
        $scope.voucherDetail.ActivityName = data.ActivityName;
        $scope.voucherDetail.ActivityCode = data.ActivityCode;
        $scope.addRow();
        $scope.closeGLPopUp();
    };
    $scope.closeGLPopUp = function () {
        angular.element(document.querySelector("#CustomerInvoiceGLPopUp")).modal("hide");
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.addAssetRow();
        $scope.checkDocDate();
        $scope.checkPostingDate();
        if ($scope.form1.$valid && !$scope.invalidDocDate && !$scope.invalidPostingDate && !$scope.validation() ) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.voucherDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        //$scope.getData();
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
                    url: "accounts/Invoice/UpdateVendorInvoice",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.voucherDetailList
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

    $scope.delete = function (invoiceId,voucherId) {
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
    $scope.confirmDelete = function (invoiceId,voucherId) {
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


    $scope.editTaxAssaignVDList = function () {
        
        for (var i = 0; i < baseService.arrayLength($scope.voucherDetailList); i++) {
            $scope.voucherDetailList[i].InvoiceTaxViewModel = [];
            $scope.taxindex = 0;
            for (var t = 0; t < $scope.taxCodDataList.length; t++) {
                if ($scope.taxCodDataList[t].VoucherDetailId == $scope.voucherDetailList[i].VoucherDetailId) {
                    //$scope.voucherDetailList[i].TotalAmount += $scope.voucherDetailList[i].Amount + $scope.taxCodDataList[t].TaxAmount;
                    //$scope.voucherDetailList[i].TotalTax += $scope.taxCodDataList[t].TaxAmount;
                    $scope.voucherDetailList[i].InvoiceTaxViewModel.push($scope.taxCodDataList[t]);
                    $scope.taxindex++;
                }
            }
        }
    };
    $scope.GetInvoiceGLBudgetActivityDetail = function (id,invoiceId) {
        $http({
            method: "get",
            url: "accounts/invoice/GetInvoiceGLBudgetActivityDetail?voucherId=" + id
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
            $scope.GetInvoiceTaxDetail(invoiceId);
        });
    };
    $scope.GetInvoiceTaxDetail = function (id) {
        $http({
            method: "get",
            url: "accounts/invoice/GetInvoiceTaxDetail?invoiceId=" + id
        }).then(function successCallback(response) {
            $scope.taxCodDataList = response.data;
            $scope.editTaxAssaignVDList();
        });
    };

    //InvoiceTaxViewModel

    $scope.Get = function (index) {
        $scope.index = index;
        $scope.voucher = $scope.invoiceList[$scope.index];
        $scope.voucher.PostingDate = $filter("dateFiltering")($scope.invoiceList[$scope.index].PostingDate);
        $scope.voucher.DocDate = $filter("dateFiltering")($scope.invoiceList[$scope.index].DocDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.getPartyPlantList($scope.voucher.PartyId);
        $scope.GetInvoiceGLBudgetActivityDetail($scope.voucher.VoucherId, $scope.voucher.Id);
       // $scope.GetInvoiceTaxDetail($scope.voucher.Id);
    }
    $scope.searchissueAUCglByList = [
        {
            "name": "Fixed Asset",
            "value": "FixedAssetName"
        },
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
        sort: "FixedAssetName",
        searchBy: "FixedAssetName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.issueAUCglList = [];
    $scope.GetIssueAUC = function () {
        $scope.IssueAUCGLUrl = "Accounts/glitem/GetAssetMasterGLBudgetActivity";
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
        angular.element(document.querySelector("#assetGLBudgetActivityPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetIssueAUCGLData();
    };

    $scope.closeIssueAUCglListPopUp = function () {
        angular.element(document.querySelector("#assetGLBudgetActivityPopUp")).modal("hide");
    };
    $scope.removeAssetRow = function () {
        var i = $scope.voucherDetailList.length;
        while (i--) {
            if ($scope.voucherDetailList[i]["TrnType"] === 'Dr') {
                $scope.voucherDetailList.splice(i, 1);
            }
        }
    };
    $scope.addAssetRow = function () {
        $scope.removeAssetRow();
        $scope.voucherDetail.GLGeneralInfoId = $scope.voucher.GLGeneralInfoId;
        $scope.voucherDetail.GLGeneralInfoCode = $scope.voucher.GLGeneralInfoCode;
        $scope.voucherDetail.GLGeneralInfoName = $scope.voucher.GLGeneralInfoName;
        $scope.voucherDetail.FixedAssetName = $scope.voucher.FixedAssetName;
        $scope.voucherDetail.FixedAssetMasterId = $scope.voucher.FixedAssetMasterId;
        $scope.voucherDetail.BudgetMasterId = $scope.voucher.BudgetMasterId;
        $scope.voucherDetail.BudgetName = $scope.voucher.BudgetName;
        $scope.voucherDetail.ActivityName = $scope.voucher.ActivityName;
        $scope.voucherDetail.ActivityId = $scope.voucher.ActivityId;
            $scope.voucherDetail.DocDate = $scope.voucher.DocDate;
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
            $scope.voucherDetail.CrAmount = null;
            $scope.voucherDetail.DrAmount = null;
            $scope.voucherDetail.TotalTax = null;
            $scope.voucherDetail.TotalAmount = null;

            $scope.voucherDetail.TrnType = "Dr";
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            clearVoucherDetail();
    };

    $scope.closeIssueAUCglListPopUp = function () {
        angular.element(document.querySelector("#assetGLBudgetActivityPopUp")).modal("hide");
    };
    $scope.activityList = [];
    $scope.getActivityListWithCallBack = function (budgetMasterId, callback) {
        if (!baseService.isUndefinedOrNull($scope.activityList) && $scope.activityList.length > 0) {
            // $scope.PostDrBudgetMasterId = budgetMasterId
           // $scope.PostDrBudgetMasterId = budgetMasterId + '01';
            callback($scope.activityList);
        }
        else {
            $http.get('accounts/BudgetMaster/GetBudgetMasterActivityCbo?budgetMasterId=' + budgetMasterId)
                .then(function (response) {
                    callback(response.data);
                    angular.forEach(response.data, function (item, i) {
                        $scope.activityList.push(item);
                        $scope.ActivityId = item.Value;
                    });
                });
        }
    };


    $scope.setissueAUCglSelected = function (data) {
        $scope.voucher.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.voucher.GLGeneralInfoCode = data.GLGeneralInfoCode;
        $scope.voucher.GLGeneralInfoName = data.GLGeneralInfoName;
        $scope.voucher.FixedAssetName = data.FixedAssetName;
        $scope.voucher.FixedAssetMasterId = data.FixedAssetMasterId;
        $scope.voucher.BudgetMasterId = data.BudgetMasterId;
        $scope.voucher.BudgetName = data.BudgetName;
        $scope.voucher.ActivityName = data.ActivityName;
        $scope.voucher.ActivityId = data.ActivityId;
        $scope.closeIssueAUCglListPopUp();
    };

    $scope.refreshAssetMaster = function () {
        $scope.voucher.GLGeneralInfoId = null;
        $scope.voucher.GLGeneralInfoCode = null;
        $scope.voucher.GLGeneralInfoName = null;
        $scope.voucher.FixedAssetName = null;
        $scope.voucher.FixedAssetMasterId = null;
        $scope.voucher.BudgetMasterId = null;
        $scope.voucher.BudgetName = null;
        $scope.voucher.ActivityName = null;
        $scope.voucher.ActivityId = null;
    };

    $scope.onClickReportDownloadWord = function (args) {
        debugger;
        var gridObj = $("#GridPrint").data("ejGrid");
        //getting corresponding record 
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('FixedAssets/FixedAssetRegister/GetIssueFixedAssetCapitalizeJournalReport?reportFormat=' + reportFormat + '&voucherId=' + data.Id + '&sourceType=' + data.SourceType, '_blank');
    };

    $scope.commandPDF = [{
        type: "details", buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",
            click: $scope.onClickReportDownloadWord
        }
    }];
    $scope.onClickReportDownloadExcel = function (args) {
        debugger;
        var gridObj = $("#GridPrint").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('FixedAssets/FixedAssetRegister/GetIssueFixedAssetCapitalizeJournalReport?reportFormat=' + reportFormat + '&voucherId=' + data.Id + '&sourceType=' + data.SourceType , '_blank');

    };
    $scope.commandExcel = [{
        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            click: $scope.onClickReportDownloadExcel
        }
    }];

}