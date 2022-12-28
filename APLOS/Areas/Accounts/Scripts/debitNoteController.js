"use strict";
debitNoteController.$inject = ["accountService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function debitNoteController(accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Debit Note";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.salesDetailList = [];
    $scope.invoiceSalesAvailableList = [];
    $scope.currencyExchangeRate = [];
    $scope.partyType = "Customer";
    $scope.sourceType = "DebitNote";
    $scope.hideSource = true;
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $scope.url = "accounts/AdjustmentNote";
    $scope.postUrl = $scope.url + "/PostDebitNote";
    $scope.deleteUrl = $scope.url + "/DeleteDebitNote";

    $scope.voucher = {
        Id: null,
        CashName: null,
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
        Amount: 0,
        Narration: null,
        Remarks: null,
        BankName: null,
        BankMasterId: null,
        CashMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        SourceFrom: null,

        GLGeneralInfoId: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetName: null,
        ActivityId: null,
        ActivityName: null,

        EmployeeGLGeneralInfoId: null,
        EmployeeGLGeneralInfoName: null,
        EmployeeTransactionTypeId: null,
        InvoiceAmount: 0,
        ExGainLossAmount: 0,
        NetInvoiceAmount: 0,
        TakenAmount: 0,
        DeductionAmount: 0,
        DeductionGroupAmount: 0,
        InvoiceGroupAmount: 0,
        ExGainLossGroupAmount: 0,
        BankChargeAmount: 0,
        FinancingTypeBankChargeId: null,
        NoteType: "CustomerDebitNote",
       // NoteType: "VendorDebitNote",
        SettlementType: "Others",
        FinancingTypeId: null,
        CompanyCurrencyRate: 1
    };

   
    $scope.transactionTypeList = function () {
        $scope.financingTypeList = [];
        accountService.getCboDebitNoteTypeList($scope.partyType, function (result) {
            $scope.financingTypeList = result;
        });
    };
    $scope.transactionTypeList();

    $scope.advanceCA = null;
    $scope.getTransactionTypeGL = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $scope.advanceCA = $.grep($scope.financingTypeList, function (item) {
                return item.FinancingTypeId === id;
            })[0];
        }
        else {
            manualValidation("div_TransactionType", false, "");
            $scope.advanceCA = null;
        }
    };

    $scope.GetCurrencyParallel = function () {
        $http({
            method: "GET",
            url: "currencies/CompanyParallelCurrency/CurrencyParallel"
        }).then(function successCallback(response) {
            $scope.CurrencyParallel = response.data;
            if ($scope.CurrencyParallel.length === 0) {
                $scope.showform = false;
            }
            else {
                $scope.showform = true;
            }
            $scope.BaseCurrencyCode = $scope.CurrencyParallel[0].Code;
        });
    };
    $scope.GetCurrencyParallel();

    baseService.init("Accounts/AdjustmentNote/GetDebitNoteList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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
            "name": "Party Code",
            "value": "PartyCode"
        },
        {
            "name": "Customer",
            "value": "PartyName"
        },
        {
            "name": "Ordering Customer",
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
        },
        {
            "name": "Currency",
            "value": "CurrencyCode"
        }
    ];

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, "", function (result) {
                $scope.entityList = result;
            });
    });

    cboService.getCboVoucherTypeDebitNoteList(function (result) {
        $scope.voucherTypeList = result;
        if ($scope.voucherTypeList.length === 1) {
            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
            if ($scope.voucherTypeList[0].LastPostingDate != null) {
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
            }
            else {
                $scope.voucher.PostingDate = $filter("dateFiltering")($filter("dateFiltering")(Date.now()));

            }
            $scope.voucher.DocDate = $scope.voucher.PostingDate;
            $scope.GetCurrencyExchangeRateList();
            $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);
        }
    });

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
    };

    $scope.exchangeGainLossList = [];
    $http.get("accounts/ExchangeGainLoss/GetExchangeGainLoss")
        .then(function successCallback(response) {
            $scope.exchangeGainLossList = response.data;
            if ($scope.exchangeGainLossList.length === 0) {
                $scope.pop("error", " Exchange Gain and Loss GL is not determine");
            }
        },
        function errorCallback(response) {
            ShowResult(response, "failure");
        });

    $scope.getById = function (id) {
        $http({
            method: "GET",
            url: "accounts/Advance/GetAdvance/" + id
        }).then(function successCallback(response) {
            $scope.voucher = response.data;
            $scope.voucher.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
            $scope.voucher.VoucherDate = $filter("dateFiltering")($scope.voucher.VoucherDate);
            $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucher.PostingDate);
            $scope.Action = "Update";
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };

    $scope.partySearchByList = [
        {
            "name": "Account Group",
            "value": "PartyAccountGroupName"
        },
        {
            "name": "Currency",
            "value": "CurrencyCode"
        }
    ];

    $scope.getPartyType = function (party) {
        $scope.voucher.PartyId = null;
        $scope.voucher.PartyPlantId = null;
        $scope.voucher.PartyCode = party.Code;
        $scope.voucher.PartyName = null;
        $scope.voucher.CurrencyId = null;
        if (party === "CustomerDebitNote")
            $scope.partyType = "Vendor";

        if (party === "VendorDebitNote")
            $scope.partyType = "Customer"; 
        $scope.changeSearchByParty();
        $scope.transactionTypeList();

        $scope.getTaxCodeByTaxYear($filter("dateFiltering")($scope.voucher.PostingDate));
    };
    $scope.changeSearchByParty = function () {
        $scope.searchByParty = 'UserName'; $scope.searchParty = "";
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

    }
    $scope.customerInvoiceSearchList = [
        {
            "Text": "VoucherNo",
            "Value": "VoucherNo"
        },
        {
            "Text": "RefNo",
            "Value": "TransactionRefNo"
        },
        {
            "Text": "PINo",
            "Value": "SalesOrderNo"
        },
        {
            "Text": "Location",
            "Value": "PartyPlantName"
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

    $scope.customerreceivableList = [];
    $scope.customerInvoiceSearch = [];
    $scope.customerInvoiceSelectedIndex = -1;
    $scope.customerInvoiceParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "VoucherNo",
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showCustomerInvoicePopUp = function (partyId) {
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.customerreceivableList = [];
            ShowResult("Please select Customer.", "failure");
            return;
        }
        else {
            $scope.customerInvoiceParameters.partyId = partyId;
            $scope.getCustomerInvoiceData = function (pageno) {
                baseService.paginationBase("accounts/Invoice/GetCustomerAvailableInvoiceList", pageno, $scope.customerInvoiceParameters)
                    .then(function (response) {
                        $scope.customerreceivableList = response.Rows;
                        $scope.customerInvoiceParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.customerInvoiceSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.customerreceivableList, $scope.customerInvoiceSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector("#customerInvoicePopUp")).modal("show");
            $scope.getCustomerInvoiceData();
        }
    };

    $scope.showVenorInvoicePopUp = function (partyId) {
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.customerreceivableList = [];
            ShowResult("Please select Customer.", "failure");
            return;
        }
        else {
            $scope.customerInvoiceParameters.partyId = partyId;
            $scope.getCustomerInvoiceData = function (pageno) {
                baseService.paginationBase("accounts/Invoice/GetVendorAvailableInvoiceList", pageno, $scope.customerInvoiceParameters)
                    .then(function (response) {
                        $scope.customerreceivableList = response.Rows;
                        $scope.customerInvoiceParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.customerInvoiceSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.customerreceivableList, $scope.customerInvoiceSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector("#customerInvoicePopUp")).modal("show");
            $scope.getCustomerInvoiceData();
        }
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
    };

    $scope.getPartyPlantList = function (partyId, isUpdateMode) {
        $scope.partyPlantList = [];
        $http.get("Parties/party/GetPartyPlantCbo?partyId=" + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault && !isUpdateMode) {
                        $scope.voucher.PartyPlantId = item.Value;
                    }
                });
                $scope.advanceDetail = {};
            });
    };

    $scope.closePartyPopUp = function (x) {
            var party = x.data;
            if (baseService.isUndefinedOrNull(party.CurrencyId)) {
                ShowResult("Customer transaction currency not found!", "failure", "partyPopUp");
                return;
            }
            else {
                $scope.voucher.PartyId = party.Id;
                $scope.voucher.PartyCode = party.Code;
                $scope.voucher.PartyName = party.Code + " - " + party.UserName;
                $scope.voucher.PartyType = party.PartyType;
                $scope.voucher.CurrencyId = party.CurrencyId;
                $scope.getPartyPlantList(party.Id);
                $scope.GetCurrencyExchangeRateList();
            }
        $scope.hidePartyPopUp();
    };

    $scope.invoiceTaxDetail = function (id) {
        $http({
            method: "GET",
            url: "accounts/CustomerInvoice/GetInvoiceTaxAvailable?invoiceId=" + id
        }).then(function successCallback(response) {
            $scope.invoiceTaxDetailList = response.data;
        });
    };

    $scope.invoiceSalesAvailable = function (id) {
        $http({
            method: "GET",
            url: "accounts/CustomerInvoice/GetInvoiceSalesAvailable?VoucherId=" + id
        }).then(function successCallback(response) {
            $scope.invoiceSalesAvailableList = response.data;
        });
    };
    $scope.invoicePurchasesAvailable = function (id) {
        $http({
            method: "GET",
            url: "accounts/CustomerInvoice/GetInvoicePurchasesAvailable?VoucherId=" + id
        }).then(function successCallback(response) {
            $scope.invoiceSalesAvailableList = response.data;
        });
    };

    $scope.saleTypeGLBudget = function (id) {
        $http({
            method: "GET",
            url: "accounts/CustomerInvoice/GetSaleTypeGLBudget?saleTypeId=" + id
        }).then(function successCallback(response) {
            $scope.saleTypeGLBudgetList = response.data;
        });
    };

    $scope.voucherGLBudget = function (id) {
        $http({
            method: "GET",
            url: "accounts/CustomerInvoice/GetVoucherGLBudget?voucherId=" + id
        }).then(function successCallback(response) {
            $scope.saleTypeGLBudgetList = response.data;
            var list = $scope.saleTypeGLBudgetList[0];
        });
    };

    $scope.convertAmount = function (data) {
        var cramount = parseInt(data.Amount), balance = parseInt(data.Balance);
        if (cramount > balance) {
            data.Amount = data.Balance;
            ShowResult("DocRefNo " + data.DocRefNo + " Payment Amount should not exceed Balance Amount", "failure");
        }
        else {
            CloseShowResult();
        }
        $scope.voucher.Amount = parseInt(data.Amount);
    };

    $scope.removeTaxRow = function (index) {
        $scope.invoiceTaxDetailList.splice(index, 1);
    };

    $scope.removeSalaryRow = function (index) {
        $scope.invoiceSalesAvailableList.splice(index, 1);
    };

    $scope.changeSourceTo = function (to) {
        $scope.voucher.DrGLId = null;
        $scope.voucher.DrGLName = null;
        $scope.voucher.DrBudgetMasterId = null;
        $scope.voucher.DrActivityId = null;
        $scope.voucher.BankName = null;
        $scope.voucher.CashName = null;
        $scope.voucher.BankMasterId = null;
        $scope.voucher.CashMasterId = null;
        $scope.isBankAmount = false;
        $scope.voucher.BankCurrencyId = null;
    };

    $scope.addRow = function (data) {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure", "GLPopUp");
            return true;
        }
            var getRow = $filter("filter")($scope.invoiceSalesAvailableList, { "BudgetMasterId": data.BudgetMasterId, "ActivityId": data.ActivityId });

        if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].BudgetMasterId === data.BudgetMasterId) {
            ShowResult("This Activity is already added!", "failure", "GLPopUp");
        }
        else {
            $scope.voucherDetail.BudgetMasterId = data.BudgetMasterId;
            $scope.voucherDetail.BudgetCode = data.BudgetCode;
            $scope.voucherDetail.BudgetName = data.BudgetName;
            $scope.voucherDetail.ActivityId = data.ActivityId;
            $scope.voucherDetail.ActivityCode = data.ActivityCode;
            $scope.voucherDetail.ActivityName = data.ActivityName;

            $scope.voucherDetail.GLGeneralInfoId = data.GLGeneralInfoId;
            $scope.voucherDetail.GLGeneralInfoCode = data.GLGeneralInfoCode;
            $scope.voucherDetail.GLGeneralInfoName = data.GLGeneralInfoName;

            $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
            $scope.voucherDetail.CrAmount = null;
            $scope.voucherDetail.DrAmount = null;
            $scope.voucherDetail.InvoiceTaxViewModel = [];
            $scope.invoiceSalesAvailableList.splice(0, 0, $scope.voucherDetail);
            $scope.voucherDetail = {};
            $scope.closeCOAICodeListPopUp();
        }
    };

    $scope.removeRow = function (index) {
        $scope.invoiceSalesAvailableList.splice(index, 1);
    };

    $scope.searchglByList = [
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
            "name": "Activity",
            "value": "ActivityName"
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "GLGeneralInfoCode",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetCOAICodeList = function () {
        $scope.GLUrl1 = "Accounts/glitem/GetGLBudgetActivityList";
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    $scope.setSelected = function (data) {
        $scope.addRow(data);
    };

    $scope.validation = function () {
        if ($scope.invoiceSalesAvailableList.length === 0) {
            if ($scope.voucher.SettlementType === "Invoice") {
                ShowResult("Please select Invoice!", "failure");
                return true;
            }
            else {
                ShowResult("Please select GL!", "failure");
                return true;
            }
        }
        return false;
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        if ($scope.form0.$valid && !$scope.invalidDocDate && !$scope.invalidPostingDate && !$scope.validation()) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "accounts/AdjustmentNote/InsertDebitNote",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.invoiceSalesAvailableList,
                        "invoiceTaxVMList": $scope.invoiceTaxDetailList,
                         "tdsTaxList": $scope.TDSList
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
            else if ($scope.form0.$valid && $scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: "accounts/AdjustmentNote/UpdateDebitNote",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.invoiceSalesAvailableList,
                        "invoiceTaxVMList": $scope.invoiceTaxDetailList
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
            }
            return true;
        }
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
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

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
    $scope.selectedInvoiceGL = function (selected) {
        if (selected) {
            $scope.selectedInvoiceGLId = selected.originalObject.GLGeneralInfoId;
            $scope.selectedInvoiceGLName = selected.originalObject.GLGeneralInfoName;
        }
    };

    $scope.inputChanged = function (str) {
        $scope.voucherDetail.GLGeneralInfoId = str;
    };

    function clearVoucherDetail() {
        $scope.voucherDetail = {};
    }

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher = {};
        if ($scope.voucherTypeList.length === 1) {
            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
            $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
            $scope.voucher.DocDate = $scope.voucher.PostingDate;
        }
        $scope.voucher.Active = true;
        $scope.voucher.Amount = null;
        $scope.voucher.CompanyCurrencyRate = null;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.NoteType = "CustomerDebitNote";
        $scope.voucher.SettlementType = "Others";
        $scope.TDSList = [];
        $scope.invoiceSalesAvailableList = [];
        $scope.voucherDetail.InvoiceTaxViewModel = [];
        $scope.invoiceTaxDetailList = [];
        $scope.salesDetailList = [];
        $scope.SelectedCurrency = null;
        $scope.isReadOnly = false;
    };

    $scope.closePopUpselected = function (data) {
        $scope.invoiceSalesAvailableList = [];
        $scope.invoiceTaxDetailList = [];
        $scope.voucher.InvoiceId = data.InvoiceId;
        $scope.invoiceTaxDetail(data.InvoiceId);
        if ($scope.voucher.NoteType =='CustomerDebitNote')
            $scope.invoiceSalesAvailable(data.VoucherId);
        else
            $scope.invoicePurchasesAvailable(data.VoucherId);
        angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
    };

    $scope.clearVendor = function () {
        $scope.voucher.VoucherNo = "";
    };

    $scope.changeSettlementType = function () {
        $scope.invoiceSalesAvailableList = [];
        $scope.invoiceTaxDetailList = [];
    };

    $scope.adjustmentNoteId = null;
    $scope.confirmPost = function (adjustmentNoteId) {
        $scope.adjustmentNoteId = adjustmentNoteId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function (adjustmentNoteId) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "adjustmentNoteId": adjustmentNoteId
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

    $scope.validationInvoiceRow = function (data) {
        if (data.InvoiceAmount < data.Amount && voucher.SettlementType === "Invoice") {
            ShowResult("Credit amount can not greater than Invoice Amount.", "failure");
            data.Amount = data.InvoiceAmount;
        }
    };

    $scope.validationTaxRow = function (data) {
        if (data.Amount < data.TaxAmount) {
            ShowResult("Credit amount can not greater than Tax Amount.", "failure");
            data.TaxAmount = data.Amount;
        }
    };

    //Delete option
    $scope.delete = function (adjustmentNoteId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "adjustmentNoteId": adjustmentNoteId, "voucherId": voucherId
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
                $scope.adjustmentNoteId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.adjustmentNoteId = null;
    $scope.confirmDelete = function (adjustmentNoteId, voucherId) {
        $scope.adjustmentNoteId = adjustmentNoteId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };


    //----------------#regoin for Debit Note-----------------------
   // $scope.voucherDetailId = "";
    $scope.getTaxCode = function (index, voucherDetailId) {
        $scope.setTaxVoucherDetailIndex = index;
        $scope.voucherDetailId = voucherDetailId;
        angular.element(document.querySelector("#texCodePopUp")).modal("show");
    };

    $scope.closeTaxCodePopUp = function () {
        $scope.setTaxVoucherDetailIndex = null;
        angular.element(document.querySelector("#texCodePopUp")).modal("hide");
    };



    $scope.removeTaxCodeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.TaxCodeId)) {
            for (var t = 0; t < baseService.arrayLength($scope.invoiceSalesAvailableList); t++) {
                if ($scope.invoiceSalesAvailableList[t].ActivityId === $scope.voucherDetailId)
                    for (var a = 0; a < baseService.arrayLength($scope.invoiceSalesAvailableList[t].InvoiceTaxViewModel); a++) {
                        if ($scope.invoiceSalesAvailableList[t].InvoiceTaxViewModel[a].TaxCodeId === $scope.TaxCodeId) {
                            $scope.invoiceSalesAvailableList[t].InvoiceTaxViewModel.splice(a, 1);
                        }
                    }
            }
            for (var i = 0; i < $scope.taxCodDataList.length; i++) {
                if ($scope.taxCodDataList[i].TaxCodeId === $scope.TaxCodeId) {
                    $scope.taxCodDataList.splice(i, 1);
                }
            }
            $scope.TaxCodeId = "";
            $scope.calculatebackTax($scope.setTaxVoucherDetailIndex);
            $scope.calculateTax($scope.setTaxVoucherDetailIndex);
        }
    };
    $scope.vendorInvoiceTaxes = [];
    $scope.vendorInvoiceTaxPush = function () {
        $scope.calculateTax($scope.setTaxVoucherDetailIndex);
        $scope.closeTaxCodePopUp();
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
        $scope.getTaxCodeByTaxYearWithhold($scope.voucher.PostingDate);
    }

    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYear = function (date) {
        if ($scope.partyType == 'Vendor')
            $scope.taxCodeUrl = "accounts/TaxCode/GetTaxCodeInputVATGST?postingDate=" + $filter("dateFiltering")(date);
        else 
            $scope.taxCodeUrl = "accounts/TaxCode/GetTaxCodeOutputVATGST?postingDate=" + $filter("dateFiltering")(date);
        $http({
            method: "get",
            url: $scope.taxCodeUrl
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message;
                }
                else {
                    var result = response.data;
                    $scope.taxCodCboList = result;
                    if ($scope.taxCodCboList.length === 0) {
                        $scope.pop("error", "No TaxCode found in this Fiscal Year ");
                    }
                }
            },
            function errorCallback(response) {
            });
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

    //FOR POPUP ADD BUTTON
    $scope.taxCodDataList = [];
    $scope.addTaxCodeonList = function (item) {

        $http({
            method: "get",
            url: "accounts/taxcode/GetTaxCodewithPersentageById?id=" + item + '&postingDate=' + $scope.voucher.PostingDate
        }).then(function successCallback(response) {
            $scope.taxcodedata = response.data;
            var ob = {
                Code: $scope.taxcodedata.Code,
                Type: $scope.taxcodedata.Type,
                ValueOfFixed: $scope.taxcodedata.ValueOfFixed,
                Description: $scope.taxcodedata.Description,
                UserName: $scope.taxcodedata.UserName,
                VoucherDetailId: $scope.voucherDetailId,
                Sequence: 1,
                TaxAmount: null,
                TaxAutoAmount: null,
                TaxCodeId: $scope.taxcodedata.TaxCodeId,
                TaxCategoryId: $scope.taxcodedata.TaxCategoryId,
                InvoiceDetailId: null,
                Id: null,
                WithholdCreditableGLId: $scope.taxcodedata.WithholdCreditableGLId,
                ExpensesGLId: $scope.taxcodedata.ExpensesGLId,
                CreditableGLId: $scope.taxcodedata.CreditableGLId,
                IsWithhold: $scope.taxcodedata.IsWithhold,
                IsCreditable: $scope.taxcodedata.IsCreditable,
                IsMerge: $scope.taxcodedata.IsMerge,
                IsRCM: $scope.taxcodedata.IsRCM,
                ManuallyEditable: $scope.taxcodedata.ManuallyEditable,
                TotalTax: null,
                TotalAmount: null,
            };

            var getRow = $filter("filter")($scope.taxCodDataList, { "TaxCodeId": ob.TaxCodeId, "VoucherDetailId": $scope.voucherDetailId });
            if (getRow.length === 0) {
                $scope.invoiceSalesAvailableList[$scope.setTaxVoucherDetailIndex].PostingWithoutTaxAllow = true;
                var vdetailrow = $scope.invoiceSalesAvailableList[$scope.setTaxVoucherDetailIndex]
                if ($scope.taxcodedata.Type = 'FixedPercentage') {
                    ob.TaxAmount = parseFloat((vdetailrow.Amount * ob.ValueOfFixed) / 100).toFixed(2);
                }
                $scope.invoiceSalesAvailableList[$scope.setTaxVoucherDetailIndex].InvoiceTaxViewModel.push(ob);
                $scope.taxCodDataList.push(ob);

            }
            else {
                ShowResult("Tax code (<b>" + ob.UserName + "</b>) is already added !!!", "failure", "texCodePopUp");
            }
            $scope.invoiceSalesAvailableList[$scope.setTaxVoucherDetailIndex].TotalTax = $filter("sumByKey")($filter("filter")($scope.invoiceSalesAvailableList[$scope.setTaxVoucherDetailIndex].InvoiceTaxViewModel), "TaxAmount");
        });
    };
    $scope.calculatebackTax = function (index) {
        if ($scope.invoiceSalesAvailableList[index].InvoiceTaxViewModel.length > 0) {
            for (var i = 0; i < $scope.invoiceSalesAvailableList[index].InvoiceTaxViewModel.length; i++) {
                $scope.invoiceSalesAvailableList[index].InvoiceTaxViewModel[i].TaxAmount = Math.round((($scope.invoiceSalesAvailableList[index].Amount * $scope.invoiceSalesAvailableList[index].InvoiceTaxViewModel[i].ValueOfFixed) / 100) * 100 + Number.EPSILON) / 100
            }
        }
    };
    $scope.calculateTax = function (index) {
        if ($scope.invoiceSalesAvailableList[index].InvoiceTaxViewModel.length > 0) {
            if ($scope.voucher.IsExcludingTax) {
                $scope.invoiceSalesAvailableList[index].TotalTax = Math.round(($filter("sumByKey")($filter("filter")($scope.invoiceSalesAvailableList[index].InvoiceTaxViewModel), "TaxAmount")) * 100 + Number.EPSILON) / 100;
                $scope.invoiceSalesAvailableList[index].TotalAmount = Math.round(($scope.invoiceSalesAvailableList[index].Amount) * 10000 + Number.EPSILON) / 10000;
                $scope.voucher.Amount = Math.round(($filter("sumByKey")($filter("filter")($scope.invoiceSalesAvailableList), "Amount")) * 10000 + Number.EPSILON) / 10000;

            }
            else {
                $scope.invoiceSalesAvailableList[index].TotalTax = Math.round(($filter("sumByKey")($filter("filter")($scope.invoiceSalesAvailableList[index].InvoiceTaxViewModel), "TaxAmount")) * 10000 + Number.EPSILON) / 10000;
                $scope.invoiceSalesAvailableList[index].TotalAmount = Math.round(($scope.invoiceSalesAvailableList[index].Amount + $scope.invoiceSalesAvailableList[index].TotalTax) * 10000 + Number.EPSILON) / 10000;

                $scope.voucher.Amount = Math.round(($filter("sumByKey")($filter("filter")($scope.invoiceSalesAvailableList), "Amount") + $filter("sumByKey")($filter("filter")($scope.invoiceSalesAvailableList), "TotalTax")) * 10000 + Number.EPSILON) / 10000;
            }
        }
        else {
            $scope.invoiceSalesAvailableList[index].TotalAmount = Math.round(($scope.invoiceSalesAvailableList[index].Amount) * 10000 + Number.EPSILON) / 10000;
            $scope.invoiceSalesAvailableList[index].TotalTax = 0;
            $scope.voucher.Amount = Math.round(($filter("sumByKey")($filter("filter")($scope.invoiceSalesAvailableList), "Amount")) + ($filter("sumByKey")($filter("filter")($scope.invoiceSalesAvailableList), "TotalTax")) * 10000 + Number.EPSILON) / 10000;
        }
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

    $scope.TDSCboList = [];
    $scope.TDSlistMessage = "";
    $scope.getTDS = function (date) {
        $http({
            method: "get",
            url: "accounts/TaxCode/GetTDSCbo?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.TDSlistMessage = response.data.Message;
                }
                else {
                    $scope.TDSCboList = response.data;;
                }
            },
            function errorCallback(response) {
            });
    };

    $scope.getTDS($filter("dateFiltering")(Date.now()));

    $scope.TDS = {
        TaxCodeId: null,
        Text: null,
        TaxAmount: null,
        ValueOfFixed: null,
        CompanyCurrencyAmount: null,
        Type: null
    };
    $scope.selectTDS = function () {
        $scope.TDS.ValueOfFixed = $.grep($scope.TDSCboList, function (item) {
            return item.Id === $scope.TDS.TaxCodeId;
        })[0].ValueOfFixed;
        $scope.TDS.Type = $.grep($scope.TDSCboList, function (item) {
            return item.Id === $scope.TDS.TaxCodeId;
        })[0].Type;
        $scope.TDS.TaxCategoryId = $.grep($scope.TDSCboList, function (item) {
            return item.Id === $scope.TDS.TaxCodeId;
        })[0].TaxCategoryId;
        if ($scope.TDS.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.TDS.ValueOfFixed)) {
            $scope.TDS.TaxAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryReceivedList), "TaxableAmount") * $scope.TDS.ValueOfFixed / 100).toFixed(4);
        }
    }
    $scope.TDSList = [];
    $scope.addTDS = function () {
        if (manualValidation("td_TDS_TaxCode", baseService.isUndefinedOrNull($scope.TDS.TaxCodeId), "Tax Code is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TDS_TaxCodeAmount", baseService.isUndefinedOrNull($scope.TDS.TaxAmount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TDS_TaxCodeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.TDS.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.TDS.TaxName = $.grep($scope.TDSCboList, function (item) {
                return item.Id === $scope.TDS.TaxCodeId;
            })[0].UserName;

            $scope.TDSList.push($scope.TDS);
            $scope.TDS = {};
        }
        $scope.calBaseAmount();
    };
    $scope.removeTDSRow = function (index) {
        $scope.TDSList.splice(index, 1);
    };
}