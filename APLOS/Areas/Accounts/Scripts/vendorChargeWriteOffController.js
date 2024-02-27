"use strict";
vendorChargeWriteOffController.$inject = ["bankService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function vendorChargeWriteOffController(bankService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Vendor Charge Set-off";
    $scope.hideSource = true;
    $scope.url = "Accounts/Advance";
    $scope.listUrl = $scope.url + "/GetVendorInvoiceChargeWriteOffList"; 
    $scope.parkUrl = $scope.url + "/ParkVendorChargeWriteOff";
    $scope.updateUrl = $scope.url + "/UpdateInvoiceChargeWriteOff";
    $scope.postUrl = $scope.url + "/PostVenodrInvoiceCharge";
    $scope.reportUrl = $scope.url + "/GetInvoiceChargeWriteOffReport?voucherId=";
    $scope.partyType = "Vendor";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.isWriteOff = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });

    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });

    baseService.init($scope.listUrl, null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, "", function (result) {
                $scope.entityList = result;
            });
    });

    bankService.getBankMasterHouseBankCboList(function (result) {
        $scope.bankMasterList = result;
    });

    bankService.getCashMasterCboList(function (result) {
        $scope.cashMasterList = result;
    });

    $scope.advance = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PartyId: null,
        PartyName: null,
        PartyPlantName: null,
        PartyGLGeneralInfoId: null,
        GLGeneralInfoId: null,
        CurrencyId: null,
        CurrencyCode: null,
        CompanyCurrencyRate: null,
        OtherCompanyCurrencyRate: 1,
        VoucherTypeId: null,
        PartyType: "Customer",
        SettlementType: "Charge",
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        InvoicePostingDate: null,
        DocDate: null,
        DocRefNo: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        IsExcludingTax: false,
        VoucherDetailId: null,
        Amount: 0,
        BaseOnDueDate: $filter("dateFiltering")(Date.now()),
        BaseNoOfDays: null,
        PaymentTermId: null,
        Narration: null,
        Remarks: null,
        BankName: null,
        BankAccountNumber: null,
        BankGL: null,
        BankGLGeneralInfoId: null,
        PaymentPostingDate: null,
        PaymentNarration: null,
        PaymentSource: "Bank"
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
        DocDate: $filter("date")(Date.now(), "dd-MMM-yyyy"),
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

    $scope.searchByList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            "name": "Ordering Party",
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
            "value": "Currency"
        }
    ];

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.advance = $scope.voucherList[$scope.index];
        $scope.advance.DocDate = $filter("dateFiltering")($scope.advance.DocDate);
        $scope.advance.VoucherDate = $filter("dateFiltering")($scope.advance.VoucherDate);
        $scope.advance.PostingDate = $filter("dateFiltering")($scope.advance.PostingDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.exchangeGainLossList = [];
    $http.get("accounts/ExchangeGainLoss/GetExchangeGainLoss")
        .then(function successCallback(response) {
            $scope.exchangeGainLossList = response.data;
        },
        function errorCallback(response) {
            ShowResult(response, "failure");
        });

    $scope.checkDrAmount = function () {
        if ($scope.voucherDetail.CrAmount > 0) {
            $scope.voucherDetail.DrAmount = 0;
        }
    };

    function validationAddGL(obj) {
        try {
            obj.FiscalYearText = $("#FiscalYear option:selected").text();
            obj.FiscalYearPeriodText = $("#FiscalYearPeriod option:selected").text();

            if (baseService.isUndefinedOrNull(obj.COAICode)) {
                throw "Please Select GL!!";
            }
            if ($scope.advance.Narration === "" || $scope.advance.Narration === null) {
                throw "Please input narration!!";
            }
            if ($scope.advance.DocRefNo === "" || $scope.advance.DocRefNo === null) {
                throw "Please input DocRefNo!!";
            }
            if (obj.DrAmount === 0 && obj.CrAmount === 0) {
                throw "Please Input Devit Amount or Credit Amount!!";
            }
        } catch (e) {
            throw ShowResult(e, "failure");
        }
    }

    $scope.checkCrAndDrEquealMsg = "";
    $scope.checkCrAndDrEqueal = function () {
        if ($scope.Crtotal === $scope.customerInvoice.Amount) {
            $scope.checkCrAndDrEquealMsg = "";
            return true;
        } else {
            $scope.pop("error", "Debit and Credit is not equal");
            return false;
        }
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.advance.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.advance.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.advance.PostingDate) < new Date($scope.advance.DocDate)) {
            msg = "Posting date must be below or equal to Doc Date!";
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        } else {
            $scope.invalidPostingDate = false;
        }

        
        if (new Date($scope.advance.InvoicePostingDate) > new Date($scope.advance.PostingDate)) {
            msg = "Posting date must be below or equal to payable of " + $scope.advance.VoucherNo;
                $scope.invalidPostingDate = true;
            }
            else {
                $scope.invalidPostingDate = false;
            }

        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.invalidEntity = false;
    $scope.entityValidation = function () {
        $scope.invalidEntity = baseService.isUndefinedOrNull($scope.advance.EntityId);
        return manualValidation("div_entity", $scope.invalidEntity, "Entity is required.");
    };

    cboService.getCboInterCompanyFinancingType("InterTransaction", function (result) {
        $scope.financingTypeList = result;
    });

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return true;
        }
        if (new Date($scope.advance.PaymentPostingDate) > new Date($scope.advance.PostingDate)) {
            ShowResult("Posting is not possible before Advance!", "failure");
            return true;
        }

        if ($scope.advance.SettlementType === "SetOff") {
            for (var i = 0; i < $scope.voucherDetailList.length; i++) {
                if (new Date($scope.voucherDetailList[i].PostingDate) > new Date($scope.advance.PostingDate)) {
                    ShowResult("Posting is not possible before Invoice!", "failure");
                    return true;
                }
                if ($scope.voucherDetailList[i].DrAmount == 0 || $scope.voucherDetailList[i].DrAmount == null) {
                    ShowResult("Received Amount should more than 0!", "failure");
                    return true;
                }
            };
        }

        if ($scope.advance.SettlementType == "Charge" || $scope.advance.SettlementType == "Return") {
            if ($scope.advance.OtherCompanyCurrencyRate == null || $scope.advance.OtherCompanyCurrencyRate == 0) {
                ShowResult("Please Input Spot Rate!", "failure");
                return true;
            }
        }
        if ($scope.partyType === "Customer") {
            if ($scope.advance.PartyId === null) {
                ShowResult("Please select Customer!", "failure");
                return true;
            }
            if ($scope.voucherDetailList.length === 0 && $scope.advance.Amount === 0) {
                ShowResult("Please select Invoice Receivable!", "failure");
                return true;
            }
        }
        return false;
    };

    $scope.clear = function () {
        $scope.Action = "Save";
        $scope.advance = {};
        $scope.advance.Active = true;
        $scope.advance.SettlementType = "SetOff";
        $scope.advance.PaymentSource = "Bank";
        $scope.advance.OtherCompanyCurrencyRate = 1;
        $scope.advance.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.getCboVoucherTypeAdvanceTakenWriteOffList();
        $scope.voucherDetailList = [];
        $scope.partyPlantList = [];
    };

    $scope.getFiscalYearPeriod = function (date) {
        if (!baseService.isUndefinedOrNull(date) && !$scope.invalidPostingDate) {
            $http({
                method: "get",
                url: "accounts/CompanyFiscalYear/CheckingFiscalYearPeriod?postingDate=" + $filter("dateFiltering")(date)
            }).then(
                function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.currencyExchangeRate = [];
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        var result = response.data;
                        if (result.IsTransationLocked === true) {
                            ShowResult(commonMessage.FiscalPeriodTransactionLocked, "failure");
                            $scope.advance.PostingDate = "";
                            $scope.advance.FiscalYearId = null;
                            $scope.advance.FiscalYearName = null;
                            $scope.advance.FiscalYearPeriodId = null;
                            $scope.advance.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else if (result.IsExchangeRateConfirmed === false) {
                            ShowResult(commonMessage.FiscalPeriodExchangeRateConfirmed, "failure");
                            $scope.advance.PostingDate = "";
                            $scope.advance.FiscalYearId = null;
                            $scope.advance.FiscalYearName = null;
                            $scope.advance.FiscalYearPeriodId = null;
                            $scope.advance.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else {
                            $scope.advance.FiscalYearId = result.FiscalYearId;
                            $scope.advance.FiscalYearName = result.FiscalYearName;
                            $scope.advance.FiscalYearPeriodId = result.FiscalYearPeriodId;
                            $scope.advance.FiscalYearPeriodName = result.PeriodName;
                        }
                    }
                },
                function errorCallback() {
                });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.getFiscalYearPeriod($scope.advance.PostingDate);

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            setPartyData(party);
            $scope.advance.CurrencyId = $scope.selectBaseCurrency();
            // Party plant list calling.
            $scope.getPartyPlantList($scope.advance.PartyId, false);
        }
        $scope.hidePartyPopUp();
    };

    $scope.clearPartyPopUp = function () {
        $scope.advance.PartyId = null;
        $scope.advance.PartyCode = null;
        $scope.advance.PartyName = null;
        $scope.advance.PartyType = null;
        $scope.advance.CurrencyId = null;
        $scope.advance.TotalPartyPlant = null;
        $scope.voucherList = [];
        $scope.partyPlantList = [];
    };

    $scope.getPartyPlantList = function (partyId, isUpdateMode) {
        $scope.partyPlantList = [];
        $http.get("Parties/party/GetPartyPlantCbo?partyId=" + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault && !isUpdateMode) {
                        $scope.advance.PartyPlantId = item.Value;
                    }
                });
                $scope.advanceDetail = {};
            });
    };

    function setPartyData(party) {
        if (baseService.isUndefinedOrNull(party.DownPaymentGLId)) {
            ShowResult("Customer gl not found!", "failure", "partyPopUp");
            return;
        }
        else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.DownPaymentBudgetId)) {
            ShowResult("Customer budget not found!", "failure", "partyPopUp");
            return;
        }
        else {
            $scope.advance.PartyName = party.Code + " - " + party.UserName;
            $scope.advance.PartyId = party.Id;
            $scope.advance.PartyType = party.PartyType;
            // TODO: have to check.
            $scope.advance.PartyGLGeneralInfoId = party.DownPaymentGLId;
            $scope.advance.PartyGL = party.DownPaymentGLCode + " - " + party.DownPaymentGLName;
            $scope.advance.PaymentNarration = party.Narration;
        }
    }

    //*********************** Customer Invoice PopUp Start *************************************
    $scope.customerInvoiceSearchList = [
        {
            "Text": "VoucherNo",
            "Value": "VoucherNo"
        },
        {
            "Text": "Party Code",
            "Value": "PartyCode"
        },
        {
            "Text": "Party",
            "Value": "PartyName"
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

    $scope.showCustomerInvoicePopUp = function () {
        $scope.compareCurrencyId = $scope.advance.CurrencyId;
        $scope.getCustomerInvoiceData = function (pageno) {
            baseService.paginationBase("accounts/Invoice/GetVendorAvailableInvoiceNewList", pageno, $scope.customerInvoiceParameters)
                .then(function (response) {
                    $scope.customerreceivableList = response.Rows;
                    $scope.customerInvoiceParameters.total_count = response.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#customerInvoicePopUp")).modal("show");
        $scope.getCustomerInvoiceData();
    };

    $scope.closePopUpselected = function (data) {
        $scope.advance.PartyId = data.PartyId;
        $scope.advance.PartyPlantId = data.PartyPlantId;
        $scope.advance.PaymentAmount = data.Balance;
        $scope.advance.VoucherNo = data.VoucherNo;
        $scope.advance.PartyName = data.PartyCode + " - " + data.PartyName;
        $scope.advance.PartyType = data.PartyType;
        $scope.advance.PartyPlantName = data.PartyPlantName;
        $scope.advance.CurrencyId = data.CurrencyId;
        $scope.advance.CurrencyCode = data.CurrencyCode;
        $scope.advance.CompanyCurrencyRate = data.CompanyCurrencyRate;
        $scope.advance.EntityId = data.EntityId;
        $scope.advance.CompanyId = data.CompanyId;
        $scope.advance.InvoicePostingDate = data.PostingDate;
        $scope.advance.PlantId = data.PlantId;
        $scope.advance.InvoiceId = data.InvoiceId;
        $scope.advance.InvoiceDetailId = data.InvoiceDetailId;
        $scope.advance.PaymentNarration = data.Narration;
        $scope.closePopUp();
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector("#customerInvoicePopUp")).modal("hide");
    };

    $scope.exchangeGainLossAmount = function (data) {
        var balance = parseFloat(data.Advilable), dramount = parseFloat(data.DrAmount);
        if (dramount > balance) {
            data.DrAmount = data.Balance;
            ShowResult("Payment Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        $scope.DrAmountSubTotal = $filter("sumByKey")($filter("filter")($scope.voucherDetailList), "DrAmount");
        if (parseFloat($scope.advance.PaymentAmount) < $scope.DrAmountSubTotal) {
            data.DrAmount = 0;
            ShowResult("Total Received Amount should not exceed Payment Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        if (data.CompanyCurrencyRate > $scope.advance.CompanyCurrencyRate) {
            data.ExchangeAmount = Math.abs(data.DrAmount * ($scope.advance.CompanyCurrencyRate - data.CompanyCurrencyRate)).toFixed(2);
            data.ExchangeType = "ExchangeLoss";
        }
        else if (data.CompanyCurrencyRate < $scope.advance.CompanyCurrencyRate) {
            data.ExchangeAmount = Math.abs(data.DrAmount * (data.CompanyCurrencyRate - $scope.advance.CompanyCurrencyRate)).toFixed(2);
            data.ExchangeType = "ExchangeGain";
        }
        else {
            data.ExchangeAmount = 0;
            data.ExchangeType = null;
        }
    };
    //*********************** Customer Invoice PopUp End ***************************************

    //*********************** Customer Advance PopUp Start *************************************
    $scope.customerAdvanceSearchList = [
        {
            "Text": "VoucherNo",
            "Value": "VoucherNo"
        },
        {
            "Text": "Customer Code",
            "Value": "PartyCode"
        },
        {
            "Text": "Customer Name",
            "Value": "PartyName"
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
    $scope.customerAdvanceDataList = [];
    $scope.customerAdvanceSearch = [];
    $scope.customerAdvanceSelectedIndex = -1;
    $scope.customerAdvanceParameters = {
        limit: 10,
        offset: 0,
        order: "DESC",
        sort: "PostingDate",
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showCustomerAdvancePopUp = function (partyId, partyPlantId) {
        $scope.voucherDetailList = [];
        $scope.compareCurrencyId = $scope.advance.CurrencyId;
        $scope.getCustomerAdvanceData = function (pageno) {
            baseService.paginationBase("accounts/Advance/GetAvilabeCustomerAdvanceList", pageno, $scope.customerAdvanceParameters)
                .then(function (response) {
                    $scope.customerAdvanceDataList = response.Rows;
                    $scope.customerAdvanceParameters.total_count = response.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#customerAdvancePopUp")).modal("show");
        $scope.getCustomerAdvanceData();
    };

    $scope.closeCustomerAdvancePopUp = function () {
        angular.element(document.querySelector("#customerAdvancePopUp")).modal("hide");
    };

    $scope.closeCustomerAdvancePopUpSelected = function (data) {
        $scope.advance.AdvanceId = data.AdvanceId;
        $scope.advance.AdvanceDetailId = data.AdvanceDetailId;
        $scope.advance.VoucherNo = data.VoucherNo;
        $scope.advance.PartyName = data.PartyCode + " - " + data.PartyName;
        $scope.advance.PartyId = data.PartyId;
        $scope.advance.PartyType = data.PartyType;
        $scope.advance.PartyPlantId = data.PartyPlantId;
        $scope.advance.PartyPlantName = data.PartyPlantName;
        $scope.advance.CurrencyId = data.CurrencyId;
        $scope.advance.CurrencyCode = data.CurrencyCode;
        $scope.advance.CompanyCurrencyRate = data.CompanyCurrencyRate;
        $scope.advance.EntityId = data.EntityId;
        $scope.advance.CompanyId = data.CompanyId;
        $scope.advance.PlantId = data.PlantId;
        $scope.advance.PartyType = data.PartyType;
        $scope.advance.PaymentAmount = data.Balance;
        $scope.advance.PaymentNarration = data.Narration;
        $scope.advance.PaymentPostingDate = $filter("dateFiltering")(data.PostingDate);
        angular.element(document.querySelector("#customerAdvancePopUp")).modal("hide");
    };

    $scope.removeRow = function (index) {
        var voucherId = $scope.voucherDetailList[index].AdvanceId;
        $scope.voucherDetailList.splice(index, 1);
    };
    //*********************** Customer Advance PopUp End *************************************

    $scope.report = function (voucherId) {
        location.href = $scope.reportUrl + voucherId;
    };

    $scope.getCboVoucherTypeAdvanceTakenWriteOffList = function () {
        cboService.getCboVoucherTypeAdvanceTakenWriteOffList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.advance.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.advance.DocDate = $scope.advance.PostingDate;
            }
        });
    };
    $scope.getCboVoucherTypeAdvanceTakenWriteOffList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.advance.VoucherTypeId = data.Value;
        $scope.advance.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.advance.DocDate = $scope.advance.PostingDate;
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkPostingDate();
        if ($scope.form0.$valid && !$scope.validation()  && !$scope.invalidPostingDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.parkUrl,
                    data: {
                        "advanceVM": $scope.advance,
                        "voucherDetailVMList": $scope.voucherDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.clear();
                        $scope.getData();
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
                        "advanceDetailVMList": $scope.voucherDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.clear();
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            return true;
        }
        return true;
    };

    $scope.invoiceWriteOffId = null;
    $scope.confirmPost = function (id) {
        $scope.invoiceWriteOffId = id;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function (id) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "invoiceWriteOffId": id
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

    $scope.changeSettlementType = function () {
        $scope.voucherDetailList = [];
        $scope.clearBankPopUp();
        $scope.clearCashPopUp();
        if ($scope.advance.SettlementType == "Payment")
            $scope.advance.PaymentSource = "Bank";
        $scope.advance.Amount = null;
        $scope.GetCurrencyExchangeRateList();
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
                ShowResult("Please select currency!", "failure", "bankPopUp");
                return;
            }
            if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
                ShowResult("Bank GL not found!", "failure", "bankPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(bank.BudgetMasterId)) {
                ShowResult("Bank budget not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(bank.CurrencyId)) {
                ShowResult("Bank transaction currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.advance.AccountTitle = bank.AccountTitle;
                $scope.advance.BankName = bank.AccountTitle;
                $scope.advance.BankMasterId = bank.BankMasterId;
                setBankGL(bank);
            }
        }
        $scope.hideBankPopUp();
    };

    function setBankGL(bank) {
        $scope.advance.BankCurrencyId = bank.CurrencyId;
        $scope.advance.GLGeneralInfoId = bank.GLGeneralInfoId;
        $scope.advance.GLGeneralInfoCode = bank.GLGeneralInfoCode;
        $scope.advance.GLGeneralInfoName = bank.GLGeneralInfoName;
        $scope.advance.BudgetMasterId = bank.BudgetMasterId;
        $scope.advance.BudgetCode = bank.BudgetCode;
        $scope.advance.BudgetName = bank.BudgetName;
        $scope.advance.ActivityId = bank.ActivityId;
        $scope.advance.ActivityCode = bank.ActivityCode;
        $scope.advance.ActivityName = bank.ActivityName;
        $scope.advance.InvoiceDetailId = bank.BankMasterId;
        $scope.advance.TrnType = "Cr";
        $scope.advance.CompanyCurrencyRate = 1;
    }

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.advance.PostingDate) && !baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.advance.PostingDate + "&currencyId=" + $scope.advance.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.advance.OtherCompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };

    $scope.clearBankPopUp = function () {
        $scope.isBankAmount = false;
        $scope.advance.AccountTitle = null;
        $scope.advance.BankName = null;
        $scope.advance.BankMasterId = null;
        $scope.advance.BankCurrencyId = null;
        $scope.advance.CashMasterId = null;
        $scope.advance.CashName = null;
        $scope.advance.CashCurrencyId = null;
        $scope.advance.GLGeneralInfoId = null;
        $scope.advance.GLGeneralInfoCode = null;
        $scope.advance.GLGeneralInfoName = null;
        $scope.advance.BudgetMasterId = null;
        $scope.advance.BudgetCode = null;
        $scope.advance.BudgetName = null;
        $scope.advance.ActivityId = null;
        $scope.advance.ActivityCode = null;
        $scope.advance.ActivityName = null;
    };

    $scope.closeCashPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult("Please select currency!", "failure", "cashPopUp");
            return;
        }
        if ($scope.cashIndex !== -1) {
            var cash = $scope.cashList[$scope.cashIndex];
            if (baseService.isUndefinedOrNull(cash.GLGeneralInfoId)) {
                ShowResult("Cash GL not found!", "failure", "cashPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(cash.BudgetMasterId)) {
                ShowResult("Cash budget not found!", "failure", "cashPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(cash.CurrencyId)) {
                ShowResult("Cash transaction currency not found!", "failure", "cashPopUp");
                return;
            }
            else {
                $scope.advance.CashMasterId = cash.Id;
                $scope.advance.CashName = cash.CashName;
                setCashGL(cash);
            }
        }
        $scope.hideCashPopUp();
    };

    function setCashGL(cash) {
        $scope.advance.CashCurrencyId = cash.CurrencyId;
        $scope.advance.GLGeneralInfoId = cash.GLGeneralInfoId;
        $scope.advance.GLGeneralInfoCode = cash.GLGeneralInfoCode;
        $scope.advance.GLGeneralInfoName = cash.GLGeneralInfoName;
        $scope.advance.BudgetMasterId = cash.BudgetMasterId;
        $scope.advance.BudgetCode = cash.BudgetCode;
        $scope.advance.BudgetName = cash.BudgetName;
        $scope.advance.ActivityId = cash.ActivityId;
        $scope.advance.ActivityCode = cash.ActivityCode;
        $scope.advance.ActivityName = cash.ActivityName;
        $scope.advance.InvoiceDetailId = cash.Id;
        $scope.advance.TrnType = "Cr";
        $scope.advance.CompanyCurrencyRate = 1;
    }

    $scope.clearCashPopUp = function () {
        $scope.clearBankPopUp();
    };

    bankService.getCboBankChargeTypeList(function (result) {
        $scope.bankChargeTypeList = result;
    });

    cboService.getCboInterCompany(null, function (result) {
        $scope.companyList = result;
    });

    $scope.companyChange = function (companyId) {
        cboService.getCboInterPlant("", companyId, "", function (result) {
            $scope.interplantList = result;
        });
    };

        //Vendor Invoice Charge GL Multiple Select
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
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];

    $scope.glListParameters = {
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


   // $scope.cOAICodeList = [];
    $scope.GetCOAICodeList = function () {
        $scope.GLUrl1 = "Accounts/glitem/GetExpenseRevenueGLBudgetActivity";
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
    $scope.addRow = function (data) {
        //if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
        //    ShowResult("Please select Currency!", "failure", "GLPopUp");
        //    return true;
        //}
        if ($scope.companyConfig.IsVoucherFromBudget)
            var getRow = $filter("filter")($scope.voucherDetailList, { "BudgetMasterId": data.BudgetMasterId, "ActivityId": data.ActivityId });

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

            $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.advance.DocDate);
            $scope.voucherDetail.DocRefNo = $scope.advance.DocRefNo;
            $scope.voucherDetail.Narration = $scope.advance.Narration;
            $scope.voucherDetail.EntityId = $scope.advance.EntityId;
            $scope.voucherDetail.PlantId = $scope.advance.PlantId;
            $scope.voucherDetail.Amount = null;
            $scope.voucherDetail.Id = null;
            $scope.voucherDetail.PartyType = $scope.advance.PartyType;
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            $scope.voucherDetail = {};
            $scope.closeCOAICodeListPopUp();
        }
    };

    $scope.removeRow = function (index) {
        $scope.voucherDetailList.splice(index, 1);
    };
    $scope.deleteUrl = $scope.url + "/DeleteVenodrInvoiceCharge";
    $scope.delete = function (invoiceWriteOffId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "invoiceWriteOffId": invoiceWriteOffId, "voucherId": voucherId
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
                $scope.invoiceWriteOffId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.invoiceId = null;
    $scope.confirmDelete = function (invoiceWriteOffId, voucherId) {
        $scope.invoiceWriteOffId = invoiceWriteOffId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };
}