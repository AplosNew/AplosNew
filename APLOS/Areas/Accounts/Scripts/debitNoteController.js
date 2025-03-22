"use strict";
debitNoteController.$inject = ["accountService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", '$window'];
function debitNoteController(accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $window) {
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
        IsInvoiceSetOff: false,
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
            "name": "Entity",
            "value": "EntityName"
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
            $scope.voucherDetail.IsOrderSpecific = data.IsOrderSpecific;
            $scope.voucherDetail.ActivityOrderType = data.ActivityOrderType;
            $scope.voucherDetail.ValueOfDistribution = data.ValueOfDistribution;

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
        else if ($scope.voucher.IsInvoiceSetOff === true) {
            $scope.TotalDebitNoteAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.invoiceSalesAvailableList), "TotalAmount"));
            $scope.TotalInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.voucherInvoiceDetailList), "Amount"));
                if ($scope.TotalInvoiceAmount != $scope.TotalDebitNoteAmount){
                    ShowResult("Dr and Cr amount is not equal.!", "failure");
                    return true;
                }
            } 
        
        else {
            for (var j = 0; j < $scope.invoiceSalesAvailableList.length; j++) {
                if ($scope.invoiceSalesAvailableList[j].IsOrderSpecific === true && $scope.invoiceDetailChargesList.length === 0) {
                    ShowResult($scope.invoiceSalesAvailableList[j].GLGeneralInfoName + ", Please Distribute Expense!", "failure");
                    return true;
                }
            }
        }
        return false;
    };

    $scope.Save = function () {
        $scope.InvoiceDetailChargesList();
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
                        "tdsTaxList": $scope.TDSList,
                        "invoiceDetailChargesList": $scope.invoiceDetailChargesList,
                        "voucherDetailInvoiceList": $scope.voucherInvoiceDetailList
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
        $scope.voucher.IsInvoiceSetOff = false;
        $scope.voucher.Amount = null;
        $scope.voucher.CompanyCurrencyRate = null;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.NoteType = "CustomerDebitNote";
        $scope.voucher.SettlementType = "Others";
        $scope.TDSList = [];
        $scope.invoiceSalesAvailableList = [];
        $scope.voucherInvoiceDetailList = [];
        $scope.voucherDetail.InvoiceTaxViewModel = [];
        $scope.invoiceTaxDetailList = [];
        $scope.salesDetailList = [];
        $scope.SelectedCurrency = null;
        $scope.isReadOnly = false;
        $scope.invoiceDetailChargesList = [];
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
    $scope.voucher_Post = {};
    $scope.confirmPost = function (adjustmentNoteId, data) {
        $scope.adjustmentNoteId = adjustmentNoteId;
        $scope.voucher_Post = {};
        $scope.voucher_Post = data;
        $scope.voucher_Post.PostingDate = $filter("dateFiltering")(data.PostingDate);
        $scope.voucher_Post.DocDate = $filter("dateFiltering")(data.DocDate);
        angular.element(document.querySelector('#PostPopUp')).modal('show');
        //$scope.message_confirmation = "Are you sure to Post?";
        //angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.closePostPopUp = function () {
        angular.element(document.querySelector("#PostPopUp")).modal("hide");
    };
    function containsSpecialChars(str) {
        const specialChars = /[@!#$%^&*()_+\=\[\]{};':"|,.<>\?`~]/;
        return specialChars.test(str);
    }
    $scope.CheckSpecialCharecter_Edit = function () {
        try {
            if (containsSpecialChars($scope.voucher_Post.DocRefNo)) {
                $scope.voucher_Post.DocRefNo = $scope.voucher_Post.DocRefNo.substring(0, $scope.voucher_Post.DocRefNo.length - 1);
                throw "No special characters allowed for Doc Ref No.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.checkDocDate_Edit = function () {
        var msg = "";
        if (new Date($scope.voucher_Post.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (new Date($scope.voucher_Post.PostingDate) < new Date($scope.voucher_Post.DocDate)) {
            msg = "Doc date must be below or equal to Posting Date!";
            $scope.invalidDocDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.voucher_Post.DocDate)) {
            msg = "Doc Date is required.";
            $scope.invalidDocDate = true;
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate_Edit", $scope.invalidDocDate, msg);
    };

    $scope.checkPostingDate_Edit = function () {
        var msg = "";
        if (new Date($scope.voucher_Post.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.invalidPostingDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.voucher_Post.PostingDate)) {
            msg = "Posting Date is required.";
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate_Edit", $scope.invalidPostingDate, msg);
    };

    $scope.post = function () {
        if ($scope.voucher_Post.EntityId == null || $scope.voucher_Post.EntityId == "" || $scope.voucher_Post.EntityId == undefined) {
            ShowResult("Please select Entity First!!", "failure");
        }
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "adjustmentNoteId": $scope.adjustmentNoteId,
                "voucherVM": $scope.voucher_Post
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
    //ExpenseDistribute
    $scope.activityOrderType = "";
    $scope.GLGeneralInfoId = 0;
    $scope.BudgetMasterId = 0;
    $scope.ActivityId = 0;
    $scope.getExpenseDistribute = function (index, item) {
        $scope.activityOrderType = "";
        $scope.TotalChargesAmount = 0;
        $scope.GLGeneralInfoId = 0;
        $scope.BudgetMasterId = 0;
        $scope.ActivityId = 0;
        $scope.activityOrderType = item.ActivityOrderType;
        $scope.ValueOfDistribution = item.ValueOfDistribution;
        $scope.TotalChargesAmount = item.Amount;
        $scope.GLGeneralInfoId = item.GLGeneralInfoId;
        $scope.BudgetMasterId = item.BudgetMasterId;
        $scope.ActivityId = item.ActivityId;

        if ($scope.activityOrderType == "InboundInvoice") {
            $scope.isSet(1);
            if ($scope.ValueOfDistribution == 'Amount') {
                $scope.calDistributedAmount();
            }
            else {
                $scope.calDistributedQtyWise();
            }
        }
        else if ($scope.activityOrderType == "OutboundInvoice") {
            $scope.isSet(2);
            if ($scope.ValueOfDistribution == 'Amount') {
                $scope.calOutBoundDistributedAmount();
            }
            else {
                $scope.calOutBoundDistributedQtyWise();
            }
        }
        else if ($scope.activityOrderType == "BothInOutboundInvoice") {
            $scope.isSet(1);
            if ($scope.ValueOfDistribution == 'Amount') {
                $scope.calDistributedAmount();
                $scope.calOutBoundDistributedAmount();
            }
            else {
                $scope.calDistributedQtyWise();
                $scope.calOutBoundDistributedQtyWise();
            }
        }
        else if ($scope.activityOrderType == "Order") {
            $scope.isSet(3);
            $scope.calMasterOrderDistributedAmount();
        }
        else if ($scope.activityOrderType == "Contract") {
            $scope.isSet(4);
            $scope.calContractDistributedAmount();
        }

        angular.element(document.querySelector("#ExpenseDistributePopUp")).modal("show");
    };

    $scope.closeExpenseDistributePopUp = function () {
        $scope.TotalDistributedAmountInBound = 0;
        $scope.TotalDistributedAmountOutBound = 0;
        $scope.TotalDistributedAmountInBound = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
        $scope.TotalDistributedAmountOutBound = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount"));

        if ((parseFloat($scope.TotalDistributedAmountInBound) + parseFloat($scope.TotalDistributedAmountOutBound)) !== parseFloat($scope.TotalChargesAmount)) {
            ShowResult('Distributed Amount must be equal Taxable Amount.!', 'failure', 'ExpenseDistributePopUp');
        }
        else {
            angular.element(document.querySelector("#ExpenseDistributePopUp")).modal("hide");
        }
    };
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.checkedInvoiceList = [];
    $scope.VendorAvailableInvoiceList = [];
    $scope.searchByVendor = "UserName"; $scope.searchVendor = "";
    $scope.searchByVendorList = [{ value: 'VoucherNo', name: "VoucherNo" }, { value: 'EntityName', name: "Entity" }, { value: 'PartyPlantName', name: "Party" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'DocDate', name: "DocDate" }, { value: 'PostingDate', name: "Invoice Date" }, { value: 'DocRefNo', name: "Invoice No" }];
    $scope.showInvoicePopUp = function () {
        $http({
            method: 'POST',
            url: 'accounts/Invoice/GetVendorAllInvoiceList',
            data: { column: $scope.searchByVendor, value: $scope.searchVendor },
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

        angular.element(document.querySelector('#InboundInvoicePopUp')).modal('show');

    };
    function checkLCExist(list, InvoiceId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].InvoiceId === InvoiceId) {

                return true;
            }
        }
        return false;
    }
    $scope.hideInvoicePopUp = function () {
        angular.element(document.querySelector("#InboundInvoicePopUp")).modal("hide");
    };
    $scope.checkedOutBoundInvoiceList = [];
    $scope.CustomerAvailableInvoiceList = [];
    $scope.searchByCustomer = "UserName"; $scope.searchCustomer = "";
    $scope.searchByCustomerList = [{ value: 'VoucherNo', name: "VoucherNo" }, { value: 'EntityName', name: "Entity" }, { value: 'PartyPlantName', name: "Party" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'DocDate', name: "DocDate" }, { value: 'PostingDate', name: "Invoice Date" }, { value: 'DocRefNo', name: "Invoice No" }];

    $scope.showOutBoundInvoicePopUp = function () {
        try {
            $http({
                method: 'POST',
                url: 'accounts/CustomerInvoice/GetCustomerAllReceivableData',
                data: { column: $scope.searchByCustomer, value: $scope.searchCustomer },
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
    $scope.ShowResultMasterOrderPopUp = function () {
        $scope.GetMasterOrderList();
        angular.element(document.querySelector('#masterOrderPopUp')).modal('show');
    }
    $scope.masterOrderList = [];
    $scope.GetMasterOrderList = function () {
        $scope.masterOrderList = [];
        $http({
            method: 'GET',
            url: "accounts/CustomerInvoice/GetMasterOrderPopUp"
        }).then(function (response) {
            $scope.masterOrderList = response.data;
        });
    }
    $scope.CloseMasterOrder = function () {
        angular.element(document.querySelector('#masterOrderPopUp')).modal('hide');
    }
    $scope.ShowResultContractPopUp = function () {
        $scope.getcontractList();
        angular.element(document.querySelector('#contractPopUp')).modal('show');
    }
    $scope.contractList = [];
    $scope.getcontractList = function () {
        $scope.contractList = [];
        $http.get("Commercial/Contract/getlist")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.contractList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.CloseContract = function () {
        angular.element(document.querySelector('#contractPopUp')).modal('hide');
    }
    $scope.TotalInvoiceAmount = 0;
    $scope.getTotalInvoiceAmount = function () {
        $scope.TotalInvoiceAmount = 0;
        if ($scope.activityOrderType == "InboundInvoice") {
            if (baseService.arrayLength($scope.checkedInvoiceList) > 0)
                $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        }
        else if ($scope.activityOrderType == "OutboundInvoice") {
            if (baseService.arrayLength($scope.checkedOutBoundInvoiceList))
                $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        }
        else if ($scope.activityOrderType == "BothInOutboundInvoice") {
            if (baseService.arrayLength($scope.checkedInvoiceList) > 0)
                $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
            if (baseService.arrayLength($scope.checkedOutBoundInvoiceList))
                $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        }

    }

    $scope.TotalInvoiceQty = 0;
    $scope.getTotalInvoiceQty = function () {
        $scope.TotalInvoiceQty = 0;
        if ($scope.activityOrderType == "InboundInvoice") {
            if (baseService.arrayLength($scope.checkedInvoiceList) > 0)
                $scope.TotalInvoiceQty += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "Qty"));
        }
        else if ($scope.activityOrderType == "OutboundInvoice") {
            if (baseService.arrayLength($scope.checkedOutBoundInvoiceList))
                $scope.TotalInvoiceQty += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "Qty"));
        }
        else if ($scope.activityOrderType == "BothInOutboundInvoice") {
            if (baseService.arrayLength($scope.checkedInvoiceList) > 0)
                $scope.TotalInvoiceQty += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "Qty"));
            if (baseService.arrayLength($scope.checkedOutBoundInvoiceList))
                $scope.TotalInvoiceQty += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "Qty"));
        }

    }

    $scope.TotalChargesAmount = 0;
    $scope.calDistributedAmount = function myfunction() {
        $scope.getTotalInvoiceAmount();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedAmountInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        var totali = parseFloat(($scope.TotalDistributedAmountInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            $scope.checkedInvoiceList[i].DistributedAmount = 0;
        }

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            if ($scope.checkedInvoiceList.length == 1) {
                $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
            }
            else {
                if ($scope.checkedInvoiceList.length - 1 == i) {

                    $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                }
                else {
                    $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                }
            }
        }

    }
    $scope.calDistributedQtyWise = function myfunction() {
        $scope.getTotalInvoiceQty();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedQtyInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "Qty"));
        var totali = parseFloat(($scope.TotalDistributedQtyInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceQty);

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            $scope.checkedInvoiceList[i].DistributedAmount = 0;
        }

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            if ($scope.checkedInvoiceList.length == 1) {
                $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].Qty) * $scope.TotalChargesAmount / $scope.TotalInvoiceQty).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
            }
            else {
                if ($scope.checkedInvoiceList.length - 1 == i) {

                    $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                }
                else {
                    $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].Qty) * $scope.TotalChargesAmount / $scope.TotalInvoiceQty).toFixed(2);
                }
            }
        }

    }
    $scope.calOutBoundDistributedAmount = function myfunction() {
        $scope.getTotalInvoiceAmount();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedAmountout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        var tatalout = parseFloat(($scope.TotalDistributedAmountout * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
        }

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

            if ($scope.checkedOutBoundInvoiceList.length == 1) {
                $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
            }
            else {
                if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                    $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (tatalout - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                }
                else {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                }
            }
        }

    }
    $scope.calOutBoundDistributedQtyWise = function myfunction() {
        $scope.getTotalInvoiceQty();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedQtyout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "Qty"));
        var totaloutQty = parseFloat(($scope.TotalDistributedQtyout * $scope.TotalChargesAmount) / $scope.TotalInvoiceQty);

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
        }

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

            if ($scope.checkedOutBoundInvoiceList.length == 1) {
                $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].Qty) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceQty).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
            }
            else {
                if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                    $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (totaloutQty - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                }
                else {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].Qty) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceQty).toFixed(2);
                }
            }
        }

    }
    $scope.calMasterOrderDistributedAmount = function myfunction() {
        for (var i = 0; i < $scope.checkedMasterOrderList.length; i++) {
            $scope.checkedMasterOrderList[i].DistributedAmount = $scope.TotalChargesAmount;

        }

    }
    $scope.calContractDistributedAmount = function myfunction() {
        for (var i = 0; i < $scope.checkedContractList.length; i++) {
            $scope.checkedContractList[i].DistributedAmount = $scope.TotalChargesAmount;

        }

    }
    $scope.calReDistributedAmount = function myfunction(index, item) {
        $scope.TotalChargesAmount = parseFloat($scope.invoiceSalesAvailableList[index].Amount);
        $scope.activityOrderType = "";
        $scope.activityOrderType = item.ActivityOrderType;
        if ($scope.activityOrderType == "InboundInvoice") {
            if (item.ValueOfDistribution == "Amount") {
                $scope.getTotalInvoiceAmount();
                $scope.TotalDistributedInvoiceAmount = 0;
                $scope.TotalDistributedAmountInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
                var totali = parseFloat(($scope.TotalDistributedAmountInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    $scope.checkedInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    if ($scope.checkedInvoiceList.length == 1) {
                        $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    }
                    else {
                        if ($scope.checkedInvoiceList.length - 1 == i) {

                            $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                            $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                        }
                    }
                }
            }
            else {
                $scope.getTotalInvoiceQty();
                $scope.TotalDistributedInvoiceAmount = 0;

                $scope.TotalDistributedQtyInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "Qty"));
                var totali = parseFloat(($scope.TotalDistributedQtyInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceQty);

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    $scope.checkedInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    if ($scope.checkedInvoiceList.length == 1) {
                        $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].Qty) * $scope.TotalChargesAmount / $scope.TotalInvoiceQty).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    }
                    else {
                        if ($scope.checkedInvoiceList.length - 1 == i) {

                            $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                            $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].Qty) * $scope.TotalChargesAmount / $scope.TotalInvoiceQty).toFixed(2);
                        }
                    }
                }
            }
        }
        else if ($scope.activityOrderType == "OutboundInvoice") {
            if (item.ValueOfDistribution == "Amount") {
                $scope.getTotalInvoiceAmount();
                $scope.TotalDistributedInvoiceAmount = 0;

                $scope.TotalDistributedAmountout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
                var tatalout = parseFloat(($scope.TotalDistributedAmountout * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

                    if ($scope.checkedOutBoundInvoiceList.length == 1) {
                        $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
                    }
                    else {
                        if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                            $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (tatalout - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                        }
                    }
                }
            }
            else {
                $scope.getTotalInvoiceQty();
                $scope.TotalDistributedInvoiceAmount = 0;

                $scope.TotalDistributedQtyout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "Qty"));
                var totaloutQty = parseFloat(($scope.TotalDistributedQtyout * $scope.TotalChargesAmount) / $scope.TotalInvoiceQty);

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

                    if ($scope.checkedOutBoundInvoiceList.length == 1) {
                        $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].Qty) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceQty).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
                    }
                    else {
                        if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                            $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (totaloutQty - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].Qty) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceQty).toFixed(2);
                        }
                    }
                }
            }
        }
        else if ($scope.activityOrderType == "BothInOutboundInvoice") {
            if (item.ValueOfDistribution == "Amount") {
                $scope.getTotalInvoiceAmount();
                $scope.TotalDistributedInvoiceAmount = 0;

                $scope.TotalDistributedAmountInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
                var totali = parseFloat(($scope.TotalDistributedAmountInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    $scope.checkedInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    if ($scope.checkedInvoiceList.length == 1) {
                        $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    }
                    else {
                        if ($scope.checkedInvoiceList.length - 1 == i) {

                            $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                            $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                        }
                    }
                }

                $scope.TotalDistributedAmountout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
                var tatalout = parseFloat(($scope.TotalDistributedAmountout * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

                    if ($scope.checkedOutBoundInvoiceList.length == 1) {
                        $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
                    }
                    else {
                        if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                            $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (tatalout - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                        }
                    }
                }
            }
            else {
                $scope.getTotalInvoiceQty();
                $scope.TotalDistributedInvoiceAmount = 0;

                $scope.TotalDistributedQtyInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "Qty"));
                var totali = parseFloat(($scope.TotalDistributedQtyInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceQty);

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    $scope.checkedInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                    if ($scope.checkedInvoiceList.length == 1) {
                        $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].Qty) * $scope.TotalChargesAmount / $scope.TotalInvoiceQty).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    }
                    else {
                        if ($scope.checkedInvoiceList.length - 1 == i) {

                            $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                            $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].Qty) * $scope.TotalChargesAmount / $scope.TotalInvoiceQty).toFixed(2);
                        }
                    }
                }

                $scope.TotalDistributedQtyout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "Qty"));
                var totaloutQty = parseFloat(($scope.TotalDistributedQtyout * $scope.TotalChargesAmount) / $scope.TotalInvoiceQty);

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
                }

                for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

                    if ($scope.checkedOutBoundInvoiceList.length == 1) {
                        $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].Qty) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceQty).toFixed(2);
                        $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
                    }
                    else {
                        if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                            $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (totaloutQty - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                        }
                        else {
                            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].Qty) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceQty).toFixed(2);
                        }
                    }
                }
            }
        }
        else if ($scope.activityOrderType == "Order") {
            for (var i = 0; i < $scope.checkedMasterOrderList.length; i++) {
                $scope.checkedMasterOrderList[i].DistributedAmount = $scope.TotalChargesAmount;

            }
        }
        else if ($scope.activityOrderType == "Contract") {
            for (var i = 0; i < $scope.checkedContractList.length; i++) {
                $scope.checkedContractList[i].DistributedAmount = $scope.TotalChargesAmount;

            }
        }

    }

    $scope.totalBooksAmount = 0;
    $scope.totalDistributedAmount = 0;
    $scope.InBoundInvoiceAmount = 0; $scope.OutBoundInvoiceAmount = 0;
    $scope.InBoundDistributed = 0; $scope.OutBoundDistributed = 0;
    $scope.totalBooksAmountCal = function () {

        $scope.InBoundInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        $scope.OutBoundInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        $scope.totalBooksAmount = parseFloat($scope.InBoundInvoiceAmount + $scope.OutBoundInvoiceAmount)

        $scope.InBoundDistributed = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
        $scope.OutBoundDistributed = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount"));
        $scope.totalDistributedAmount = parseFloat($scope.InBoundDistributed + $scope.OutBoundDistributed)
    }
    $scope.AddInvoice = function () {

        if (baseService.arrayLength($scope.VendorAvailableInvoiceList) > 0) {
            $scope.checkedInvoiceList = [];
            angular.forEach($scope.VendorAvailableInvoiceList, function (a) {
                if (a.Active) {
                    $scope.checkedInvoiceList.push({
                        InvoiceId: a.InvoiceId
                        , InvoiceDetailId: a.InvoiceDetailId
                        , Amount: a.Receivable
                        , Qty: a.TrnQty
                        , BooksAmount: a.Receivable * a.CompanyCurrencyRate
                        , DistributedAmount: 0
                        , ChargesAmount: 0
                        , TaxAmount: 0
                        , Active: true
                        , PostingDate: a.PostingDate
                        , PartyPlantName: a.PartyPlantName
                        , CurrencyCode: a.CurrencyCode
                        , VoucherNo: a.VoucherNo
                        , InvoiceType: 'InboundInvoice'
                        , GLGeneralInfoId: $scope.GLGeneralInfoId
                        , BudgetMasterId: $scope.BudgetMasterId
                        , ActivityId: $scope.ActivityId
                        , DocRefNo: a.DocRefNo
                    });
                }
            });
        }

        $scope.hideInvoicePopUp();
        if ($scope.ValueOfDistribution == 'Amount') {
            $scope.calDistributedAmount();
            $scope.calOutBoundDistributedAmount();
        }
        else {
            $scope.calDistributedQtyWise();
            $scope.calOutBoundDistributedQtyWise();
        }
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
                            , Qty: a.TrnQty
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
        if ($scope.ValueOfDistribution == 'Amount') {
            $scope.calDistributedAmount();
            $scope.calOutBoundDistributedAmount();
        }
        else {
            $scope.calDistributedQtyWise();
            $scope.calOutBoundDistributedQtyWise();
        }

        $scope.totalBooksAmountCal();
    };
    $scope.checkedMasterOrderList = [];
    $scope.AddOrder = function (x) {
        var a = x.rowData;
        if (baseService.arrayLength($scope.masterOrderList) > 0) {
            $scope.checkedMasterOrderList = [];
            $scope.checkedMasterOrderList.push({
                InvoiceId: null
                , InvoiceDetailId: null
                , Amount: 0
                , BooksAmount: 0
                , DistributedAmount: 0
                , ChargesAmount: 0
                , TaxAmount: 0
                , Active: true
                , PostingDate: ""
                , PartyPlantName: a.InvoicingPartyPlant
                , CurrencyCode: ""
                , VoucherNo: ""
                , InvoiceType: 'Order'
                , GLGeneralInfoId: $scope.GLGeneralInfoId
                , BudgetMasterId: $scope.BudgetMasterId
                , ActivityId: $scope.ActivityId
                , MasterOrderId: a.MasterOrderId
                , ContractId: null
                , CustomerName: a.CustomerName
                , InvoicingPartyPlant: a.InvoicingPartyPlant
                , DeliveryPartyPlant: a.DeliveryPartyPlant
                , Type: a.Type
            });
        }



        $scope.CloseMasterOrder();
        $scope.calMasterOrderDistributedAmount();

    };
    $scope.checkedContractList = [];
    $scope.AddContract = function (x) {
        var a = x.rowData;
        if (baseService.arrayLength($scope.contractList) > 0) {
            $scope.checkedContractList = [];
            $scope.checkedContractList.push({
                InvoiceId: null
                , InvoiceDetailId: null
                , Amount: 0
                , BooksAmount: 0
                , DistributedAmount: 0
                , ChargesAmount: 0
                , TaxAmount: 0
                , Active: true
                , PostingDate: ""
                , PartyPlantName: ""
                , CurrencyCode: ""
                , VoucherNo: ""
                , InvoiceType: 'Contract'
                , GLGeneralInfoId: $scope.GLGeneralInfoId
                , BudgetMasterId: $scope.BudgetMasterId
                , ActivityId: $scope.ActivityId
                , MasterOrderId: null
                , ContractId: a.Id
                , ContractNo: a.ContractNo
                , UDNo: a.UDNo
                , CustomerName: a.CustomerName
                , Buyer: a.Buyer
                , Remarks: a.Remarks
            });
        }



        $scope.CloseContract();
        $scope.calContractDistributedAmount();

    };

    $scope.DeleteConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteConfirmationPopUp")).modal("show");
    };
    $scope.RemoveInvoice = function () {

        for (var i = 0; i < baseService.arrayLength($scope.checkedInvoiceList); i++) {
            if ($scope.checkedInvoiceList[i].InvoiceId == $scope.InvoiceId)
                $scope.checkedInvoiceList.splice(i, 1);
        }

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            $scope.checkedInvoiceList[i].DistributedAmount = 0;
        }
        if ($scope.ValueOfDistribution == 'Amount') {
            $scope.calDistributedAmount();
            $scope.calOutBoundDistributedAmount();
        }
        else {
            $scope.calDistributedQtyWise();
            $scope.calOutBoundDistributedQtyWise();
        }
        $scope.totalBooksAmountCal();

    }
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
        if ($scope.ValueOfDistribution == 'Amount') {
            $scope.calDistributedAmount();
            $scope.calOutBoundDistributedAmount();
        }
        else {
            $scope.calDistributedQtyWise();
            $scope.calOutBoundDistributedQtyWise();
        }
        $scope.totalBooksAmountCal();
    }
    $scope.DeleteMasterOrderConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteMasterOrderConfirmationPopUp")).modal("show");
    };
    $scope.RemoveMasterOrderInvoice = function () {
        for (var i = 0; i < baseService.arrayLength($scope.checkedMasterOrderList); i++) {
            if ($scope.checkedMasterOrderList[i].MasterOrderId == $scope.InvoiceId)
                $scope.checkedMasterOrderList.splice(i, 1);
        }

        $scope.calMasterOrderDistributedAmount();
    }
    $scope.DeleteContractConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteContractConfirmationPopUp")).modal("show");
    };
    $scope.RemoveContractInvoice = function () {
        for (var i = 0; i < baseService.arrayLength($scope.checkedContractList); i++) {
            if ($scope.checkedContractList[i].ContractId == $scope.InvoiceId)
                $scope.checkedContractList.splice(i, 1);
        }

        $scope.calContractDistributedAmount();
    }
    $scope.checkDistributedAmount = function myfunction(index, item) {
        if ($scope.activityOrderType == "InboundInvoice") {
            $scope.TotalDistributedAmounts = 0;
            $scope.TotalDistributedAmounts = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));

            if (parseFloat($scope.TotalDistributedAmounts) > parseFloat($scope.TotalChargesAmount)) {
                $scope.checkedInvoiceList[index].DistributedAmount = 0;
                ShowResult('Distributed Amount must be equal Taxable Amount.!', 'failure', 'ExpenseDistributePopUp');
            }
        }
        else if ($scope.activityOrderType == "OutboundInvoice") {
            $scope.TotalDistributedAmounts = 0;
            $scope.TotalDistributedAmounts = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount"));

            if (parseFloat($scope.TotalDistributedAmounts) > parseFloat($scope.TotalChargesAmount)) {
                $scope.checkedOutBoundInvoiceList[index].DistributedAmount = 0;
                ShowResult('Distributed Amount must be equal Taxable Amount.!', 'failure', 'ExpenseDistributePopUp');
            }
        }
        else if ($scope.activityOrderType == "BothInOutboundInvoice") {
            $scope.TotalDistributedAmountInBound = 0;
            $scope.TotalDistributedAmountOutBound = 0;
            $scope.TotalDistributedAmountInBound = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
            $scope.TotalDistributedAmountOutBound = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount"));

            if ((parseFloat($scope.TotalDistributedAmountInBound) + parseFloat($scope.TotalDistributedAmountOutBound)) > parseFloat($scope.TotalChargesAmount)) {
                ShowResult('Distributed Amount must be equal Taxable Amount.!', 'failure', 'ExpenseDistributePopUp');
            }
        }
    }
    $scope.invoiceDetailChargesList = [];
    $scope.InvoiceDetailChargesList = function myfunction() {
        $scope.invoiceDetailChargesList = $scope.checkedInvoiceList.concat($scope.checkedOutBoundInvoiceList).concat($scope.checkedMasterOrderList).concat($scope.checkedContractList);

    };
    $scope.ExpenseDistributionReport = function (reportFormat, voucherId) {
        $window.open('Accounts/Invoice/ReportVendorInvoiceExpenseDistribution?reportFormat=' + reportFormat + '&voucherId=' + voucherId, '_blank');
    }

    //*********************** Customer Invoice PopUp Start *************************************
    $scope.customerInvoiceSearchList = [
        {
            "Text": "VoucherNo",
            "Value": "VoucherNo"
        },
        {
            "Text": "RefNo",
            "Value": "DocRefNo"
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

    $scope.showVendorInvoicePopUp = function (partyId) {
        $scope.customerreceivableList = [];
        $scope.customerInvoiceSearch = [];
        if (baseService.isUndefinedOrNull(partyId)) {
            $scope.customerreceivableList = [];
            ShowResult("Please select Vendor.", "failure");
            return;
        }
        else {
            $scope.compareCurrencyId = $scope.voucher.CurrencyId;
            $scope.customerInvoiceParameters.partyId = partyId;
            $scope.customerreceivableGLData = function (pageno) {
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
            $scope.customerreceivableGLData();
        }

    };
    $scope.voucherInvoiceDetailList = [];
    $scope.closePopUpselected = function () {
        angular.forEach($scope.customerreceivableList, function (data, i) {
            if (data.Active === true) {
                data.TrnType = "Cr";
                data.PartyPlantName = data.PartyPlantName;
                var getRow = $filter("filter")($scope.voucherInvoiceDetailList, { "TrnType": "Cr", "DocRefNo": data.DocRefNo });
                if (getRow.length === 0) {
                    data.Receivable = data.Receivable;
                    data.WriteOff = data.Received;
                    data.Advilable = data.Balance;
                    data.Amount = data.Balance;
                    data.CompanyCurrencyRate = data.CompanyCurrencyRate;
                    $scope.voucherInvoiceDetailList.push(data);
                    $scope.exchangeGainLossAmountInvoice(data);
                    $scope.voucher.InvoiceVoucherNo = data.VoucherNo;
                    angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
                }
                else {
                    ShowResult(data.DocRefNo + " already  Exist", "failure", "customerInvoicePopUp");
                }
            }
        });
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
    };

    $scope.exchangeGainLossAmountInvoice = function (data) {
        var balance = parseFloat(data.Advilable), dramount = parseFloat(data.Amount);
        if (dramount > balance) {
            data.Amount = data.Balance;
            ShowResult("Payment Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        //$scope.TotalCreditNoteAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.invoiceSalesAvailableList), "TotalAmount"));
        //$scope.TotalInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.voucherInvoiceDetailList), "Amount"));
        //if ($scope.TotalInvoiceAmount > $scope.TotalCreditNoteAmount) {
        //    data.Amount = 0;
        //    ShowResult("Invoice Amount should not exceed Debit Note Amount.", "failure");
        //}
        //else {
        //    CloseShowResult();
        //}
        if (data.CompanyCurrencyRate < $scope.voucher.CompanyCurrencyRate) {
            data.ExchangeAmount = Math.abs(data.Amount * (data.CompanyCurrencyRate - $scope.voucher.CompanyCurrencyRate)).toFixed(2);
            data.ExchangeType = "ExchangeLoss";
        }
        else if (data.CompanyCurrencyRate > $scope.voucher.CompanyCurrencyRate) {
            data.ExchangeAmount = Math.abs(data.Amount * ($scope.voucher.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
            data.ExchangeType = "ExchangeGain";
        }
        else {
            data.ExchangeAmount = 0;
            data.ExchangeType = null;
        }
    };
    
    $scope.removeInvoiceRow = function (index, data) {
        $scope.voucherInvoiceDetailList.splice(index, 1);
    };
    //*********************** Customer Invoice PopUp End ***************************************
}