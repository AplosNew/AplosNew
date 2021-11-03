partyReconciliationController.$inject = ["bankService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "$window"];
function partyReconciliationController(bankService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $window) {
    $rootScope.title = "Party Reconciliation";
    $scope.hideSource = true;
    $scope.url = "Accounts/PartyReconciliation";
    $scope.listUrl = $scope.url + "/GetPartyReconciliationList";
    $scope.parkUrl = $scope.url + "/InsertPartyReconciliation";
    $scope.updateUrl = $scope.url + "/UpdatePartyReconciliation";
    $scope.partyType = "Party";
    $scope.Action = "Save";
    $scope.FirstDivShow = true;
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.isWriteOff = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.partyType = "Party";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });

    

    $scope.searchByPosted = "VoucherNo"; $scope.search = "";
    $scope.searchByPostedList = [{ value: 'VoucherNo', name: "VoucherNo" }
        , { value: 'PostingDate', name: "PostingDate" }, { value: 'DocRefNo', name: "DocRef No" }
        , { value: 'DocDate', name: "Doc Date" }];

    $scope.voucherList = [];
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: 'Accounts/PartyReconciliation/GetPartyReconciliationList',
            data: { column: $scope.searchByPostedGRN, value: $scope.searchGRN },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.voucherList = response.data;
        });
    };
    $scope.getData();



    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
    });
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });
    bankService.getBankMasterHouseBankCboList(function (result) {
        $scope.bankMasterList = result;
    });

    bankService.getCashMasterCboList(function (result) {
        $scope.cashMasterList = result;
    });

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
    };

    $scope.getVoucherTypePartyReconcilliationList = function () {
        cboService.getCboVoucherTypePartyReconcilliationList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                //$scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);

            }
        });
    };
    $scope.getVoucherTypePartyReconcilliationList();

    $scope.voucher = {
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
        CompanyCurrencyRate: 1,
        OtherCompanyCurrencyRate: 1,
        VoucherTypeId: null,
        PartyType: "Customer",
        SettlementType: "SetOff",
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
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
        PaymentSource: null
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


    $scope.exchangeGainLossList = [];
    $http.get("accounts/ExchangeGainLoss/GetExchangeGainLoss")
        .then(function successCallback(response) {
            $scope.exchangeGainLossList = response.data;
        },
            function errorCallback(response) {
                ShowResult(response, "failure");
            });


    $scope.invalidEntity = false;
    $scope.entityValidation = function () {
        $scope.invalidEntity = baseService.isUndefinedOrNull($scope.voucher.EntityId);
        return manualValidation("div_entity", $scope.invalidEntity, "Entity is required.");
    };


    

    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        if (baseService.isUndefinedOrNull(party.ReconciliationGLId)) {
            ShowResult($scope.partyType + " GL not found!", "failure", "partyPopUp");
            return;
        }
        else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.ReconciliationBudgetId)) {
            ShowResult($scope.partyType + " Budget not found!", "failure", "partyPopUp");
            return;
        }
        else {
            //$scope.removeDrRow();
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
            //if ($scope.voucher.PaymentTermId !== null) {
            //    $scope.changePaymentTerm($scope.voucher.PaymentTermId);
            //}
            $scope.taxCodDataList = [];
            $scope.getPartyPlantList(party.Id);

            //TODO:
            $scope.getpartyWiseLiabilityList($scope.voucher.PartyId);
            $scope.getpartyWiseAssetList($scope.voucher.PartyId);


            //$scope.GetCurrencyExchangeRateList();
            //clearVoucherDetail();
        }
        $scope.hidePartyPopUp();
    };

    $scope.partyWiseAssetList = [];
    $scope.getpartyWiseAssetList = function (partyId) {
        $http({
            method: "GET",
            url: "Accounts/PartyReconciliation/GetPartyDrList?partyId=" + partyId
        }).then(function successCallback(response) {
            $scope.partyWiseAssetList = response.data;
        });
    };

    $scope.partyWiseLiabilityList = [];
    // $scope.PartyId = null;
    $scope.getpartyWiseLiabilityList = function (partyId) {
        // $scope.partyWiseLiabilityList = [];
        $http({
            method: "GET",
            url: "Accounts/PartyReconciliation/GetPartyCrList?partyId=" + partyId
        }).then(function successCallback(response) {
            $scope.partyWiseLiabilityList = response.data;
        });
    };

    //$scope.Get = function (args) {

    //    $scope.ModelNew = Object.assign({}, args.data);
    //    $scope.Action = 'Update';
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};

    $scope.clearPartyPopUp = function () {
        $scope.voucher.PartyId = null;
        $scope.voucher.PartyCode = null;
        $scope.voucher.PartyName = null;
        $scope.voucher.PartyType = null;
        $scope.voucher.CurrencyId = null;
        $scope.voucher.TotalPartyPlant = null;
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
                        $scope.voucher.PartyPlantId = item.Value;
                    }
                });
                $scope.advanceDetail = {};
            });
    };
    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.voucher.CompanyCurrencyRate)) {
            ShowResult("Please input Rate!", "failure");
            return true;
        }
        if (!baseService.isUndefinedOrNull($scope.bankCharge.FinancingTypeId)) {
            ShowResult("Please add Charges!", "failure");
            return true;
        }
        if (!baseService.isUndefinedOrNull($scope.TDS.TaxCodeId)) {
            ShowResult("Please add Taxes!", "failure");
            return true;
        }
        if ($scope.partyType === "Customer") {
            if (baseService.isUndefinedOrNull($scope.voucher.PartyId)) {
                ShowResult("Please select Customer!", "failure");
                return true;
            }
            if (parseFloat($scope.voucher.Amount) === 0) {
                ShowResult(" Amount must greater than 0!", "failure");
                return true;
            }
            var vdetailCr = $filter("filter")($scope.voucherDetailList, { TrnType: "Cr" });
            if (vdetailCr.length === 0) {
                ShowResult("Please Select Customer Receivable !", "failure");
                return true;
            }
            if (baseService.isUndefinedOrNull($scope.voucher.GLGeneralInfoId)) {
                ShowResult("Please select Cash or Bank!", "failure");
                return true;
            }
        }
      
        return false;
    };
    $scope.processSetOffLiabilityValidation = function () {
        if ($scope.SetOffType == 'Liability') {
            $scope.DrRowAmount = Math.round($filter("sumByKey")($filter("filter")($scope.selectedDrRow), "Amount") * 100 + Number.EPSILON) / 100;
            if ($scope.DrRowAmount > $scope.selectedCrRow[0].Balance) {
                ShowResult("Receivable amount should not greater than Payable Balance Amount!", "failure");
                return true;
            }
            for (var i = 0; i < $scope.selectedDrRow.length; i++) {
                if ($scope.selectedDrRow[i].CurrencyId != $scope.voucher.CurrencyId) {
                    ShowResult("Currency is not match!", "failure");
                    return true;
                }
            }

        }
       
        return false;
    }

    $scope.processSetOffAssetValidation = function () {
        if ($scope.SetOffType == 'Asset') {
            $scope.CrRowAmount = Math.round($filter("sumByKey")($filter("filter")($scope.selectedThirdCrRow), "Amount") * 100 + Number.EPSILON) / 100;
            if ($scope.CrRowAmount > $scope.selectedThirdDrRow[0].Balance) {
                ShowResult("Receivable amount should not greater than Payable Balance Amount!", "failure");
                return true;
            }
            for (var i = 0; i < $scope.selectedThirdCrRow.length; i++) {
                if ($scope.selectedThirdCrRow[i].CurrencyId != $scope.voucher.CurrencyId) {
                    ShowResult("Currency is not match!", "failure");
                    return true;
                }
            }
        }
        return false;
    }
    $scope.processSetOffLiabilityVoucher = function () {

        $scope.voucher.GLGeneralInfoId = $scope.selectedCrRow[0].GLGeneralInfoId;
        $scope.voucher.BudgetMasterId = $scope.selectedCrRow[0].BudgetMasterId;
        $scope.voucher.ActivityId = $scope.selectedCrRow[0].ActivityId;
        $scope.voucher.CurrencyId = $scope.selectedCrRow[0].CurrencyId;
        $scope.voucher.InvoiceId = $scope.selectedCrRow[0].InvoiceId;
        $scope.voucher.PartyType = $scope.selectedCrRow[0].PartyType;
        $scope.voucher.CompanyCurrencyRate = $scope.selectedCrRow[0].CompanyCurrencyRate;
        $scope.voucher.InvoiceDetailId = $scope.selectedCrRow[0].InvoiceDetailId;
        $scope.voucher.VoucherDate = $filter("dateFiltering")(Date.now());
        $scope.voucher.SettlementType = 'SetOff';
        $scope.voucher.Narration = 'SetOff';
    }

    $scope.processSetOffAssetVoucher = function () {

        $scope.voucher.GLGeneralInfoId = $scope.selectedThirdDrRow[0].GLGeneralInfoId;
        $scope.voucher.BudgetMasterId = $scope.selectedThirdDrRow[0].BudgetMasterId;
        $scope.voucher.ActivityId = $scope.selectedThirdDrRow[0].ActivityId;
        $scope.voucher.CurrencyId = $scope.selectedThirdDrRow[0].CurrencyId;
        $scope.voucher.InvoiceId = $scope.selectedThirdDrRow[0].InvoiceId;
        $scope.voucher.PartyType = $scope.selectedThirdDrRow[0].PartyType;
        $scope.voucher.CompanyCurrencyRate = $scope.selectedThirdDrRow[0].CompanyCurrencyRate;
        $scope.voucher.InvoiceDetailId = $scope.selectedThirdDrRow[0].InvoiceDetailId;
        $scope.voucher.VoucherDate = $filter("dateFiltering")(Date.now());
        $scope.voucher.SettlementType = 'SetOff';
        $scope.voucher.Narration = 'SetOff';

    }
    $scope.clear = function () {
        $scope.Action = "Save";
        $scope.voucher = {};
        $scope.voucher.Active = true;
        $scope.voucher.SettlementType = "SetOff";
        $scope.voucher.OtherCompanyCurrencyRate = 1;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.selectedDrRow = [];
        $scope.selectedThirdCrRow = [];
        $scope.partyDrDataList = [];
        $scope.partyPlantList = [];
        $scope.selectedThirdDrRow = [];
        $scope.selectedCrRow = [];
        $scope.OtherName = null;
        $scope.getVoucherTypePartyReconcilliationList();
    };
    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.SetOffType == 'Liability' && $scope.OtherName=='Invoice') {
            $scope.processSetOffLiabilityVoucher();
            if ($scope.form0.$valid && !$scope.processSetOffLiabilityValidation()) {
                if ($scope.Action === "Save") {
                    $http({
                        method: "POST",
                        url: 'Accounts/Advance/InsertPartyLiabilityReconciliation',
                        data: {
                            "voucherVM": $scope.voucher,
                            "voucherDetailVMList": $scope.selectedDrRow
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

            }
            return true;
        }
        else if ($scope.SetOffType == 'Liability' && $scope.OtherName == 'Invoice') {
            $scope.processSetOffLiabilityVoucher();
            if ($scope.form0.$valid && !$scope.processSetOffLiabilityValidation()) {
                if ($scope.Action === "Save") {
                    $http({
                        method: "POST",
                        url: 'Accounts/Advance/InsertPartyAdvanceLiabilityReconciliation',
                        data: {
                            "voucherVM": $scope.voucher,
                            "voucherDetailVMList": $scope.selectedDrRow
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

            }
            return true;
        }
        else if ($scope.SetOffType == 'Asset' && $scope.OtherName == 'Invoice') {
            $scope.processSetOffAssetVoucher();
            if ($scope.form0.$valid && !$scope.processSetOffAssetValidation()) {
                if ($scope.Action === "Save") {
                    $http({
                        method: "POST",
                        url: 'Accounts/Advance/InsertPartyAssetReconciliation',
                        data: {
                            "voucherVM": $scope.voucher,
                            "voucherDetailVMList": $scope.selectedThirdCrRow
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

            }
            return true;
            }
        else if ($scope.SetOffType == 'Asset' && $scope.OtherName == 'Advance') {
            $scope.processSetOffAssetVoucher();
            if ($scope.form0.$valid && !$scope.processSetOffAssetValidation()) {
                if ($scope.Action === "Save") {
                    $http({
                        method: "POST",
                        url: 'Accounts/Advance/InsertPartyAdvanceAssetReconciliation',
                        data: {
                            "voucherVM": $scope.voucher,
                            "voucherDetailVMList": $scope.selectedThirdCrRow
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

            }
            return true;
        }
    };
    $scope.GridBack = function () {
        $scope.FirstDivShow = true;
        $scope.SecondDivShow = false;
        $scope.ThirdDivShow = false;
        $scope.selectedCrRow = [];
        $scope.selectedDrRow = [];
        $scope.selectedThirdDrRow = [];
        $scope.selectedThirdCrRow = [];
        $scope.SetOffType = null;
    }
    $scope.selectedCrRow = [];
    $scope.ShowDiv = function (obj) {
        $scope.selectedCrRow.push(obj.data);
        $scope.OtherName = obj.data.OtherName;
        $scope.FirstDivShow = false;
        $scope.SecondDivShow = true;
        $scope.ThirdDivShow = false;
        $scope.SetOffType = 'Liability';

    };

    $scope.selectedDrRow = [];
    $scope.AddAssetRow = function (obj) {

       var getRow = $filter("filter")($scope.selectedDrRow, { "DocRefNo": obj.data.DocRefNo, "Id": obj.data.Id });
        if (getRow.length === 0) {
            $scope.selectedDrRow.push(obj.data);
        }
        else {
            ShowResult(obj.data.DocRefNo + " already  Exist", "failure");
        }

    };
    $scope.removeDrRow = function (index) {
        $scope.selectedDrRow.splice(index, 1);
    };

    $scope.selectedThirdDrRow = [];
    $scope.ShowThirdDiv = function (obj) {
        $scope.selectedThirdDrRow.push(obj.data);
        $scope.OtherName = obj.data.OtherName;
        $scope.FirstDivShow = false;
        $scope.SecondDivShow = false;
        $scope.ThirdDivShow = true;
        $scope.SetOffType = 'Asset';

    };

    $scope.selectedThirdCrRow = [];
    $scope.AddLiabilityRow = function (obj) {
      var  getRow = $filter("filter")($scope.selectedThirdCrRow, { "DocRefNo": obj.data.DocRefNo, "Id": obj.data.Id });
        if (getRow.length === 0) {
            $scope.selectedThirdCrRow.push(obj.data);
        }
        else {
            ShowResult(obj.data.DocRefNo + " already  Exist", "failure");
        }

    };
    $scope.removeThirdCrRow = function (index) {
        $scope.selectedThirdCrRow.splice(index, 1);
    };

    $scope.partyAssetDetailList = [];
    $scope.showAssetDetail = function (obj) {

        $scope.partyAssetDetailList.push(obj.data);
        angular.element(document.querySelector("#_partyDrDetailPopUP")).modal("show");

    }

    $scope.closeAssetDetailPopUp = function () {
        $scope.partyAssetDetailList = [];
        angular.element(document.querySelector("#_partyDrDetailPopUP")).modal("hide");

    }

    $scope.partyLiabilityDetailList = [];
    $scope.showLiabilityDetail = function (obj) {

        $scope.partyLiabilityDetailList.push(obj.data);
        angular.element(document.querySelector("#_partyCrDetailPopUP")).modal("show");

    }

    $scope.closeLiabilityDetailPopUp = function () {
        $scope.partyLiabilityDetailList = [];
        angular.element(document.querySelector("#_partyCrDetailPopUP")).modal("hide");

    }

    $scope.exchangeGainLossDrAmount = function (data) {

        var balance = parseFloat(data.Balance), dramount = parseFloat(data.Amount);
        if (dramount > balance) {
            data.Amount = data.Balance;
            ShowResult("Invoice Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        if (data.CompanyCurrencyRate > $scope.selectedCrRow[0].CompanyCurrencyRate) {
            data.ExchangeAmount = Math.round((data.Amount * (data.CompanyCurrencyRate - $scope.selectedCrRow[0].CompanyCurrencyRate)) * 10000 + Number.EPSILON) / 10000;
            data.ExchangeType = "ExchangeLoss";
            data.BaseDrAmount = Math.round((data.Amount * data.CompanyCurrencyRate) * 10000 + Number.EPSILON) / 10000;

        }
        else if (data.CompanyCurrencyRate < $scope.selectedCrRow[0].CompanyCurrencyRate) {
            data.ExchangeAmount = Math.round((data.Amount * ($scope.selectedCrRow[0].CompanyCurrencyRate - data.CompanyCurrencyRate)) * 10000 + Number.EPSILON) / 10000;
            data.ExchangeType = "ExchangeGain";
            data.BaseDrAmount = Math.round((data.Amount * data.CompanyCurrencyRate) * 10000 + Number.EPSILON) / 10000;
        }
        else {
            data.ExchangeAmount = 0;
            data.BaseDrAmount = Math.round((data.Amount * data.CompanyCurrencyRate) * 10000 + Number.EPSILON) / 10000;
            data.ExchangeType = null;
        }
        //$scope.calBaseAmount();
    };

    $scope.exchangeGainLossCrAmount = function (data) {

        var balance = parseFloat(data.Balance), dramount = parseFloat(data.Amount);
        if (dramount > balance) {
            data.Amount = data.Balance;
            ShowResult("Invoice Amount should not exceed Balance Amount.", "failure");
        }
        else {
            CloseShowResult();
        }
        if (data.CompanyCurrencyRate < $scope.selectedThirdDrRow[0].CompanyCurrencyRate) {
            data.ExchangeAmount = Math.round((data.Amount * ($scope.selectedThirdDrRow[0].CompanyCurrencyRate - data.CompanyCurrencyRate)) * 10000 + Number.EPSILON) / 10000;
            data.ExchangeType = "ExchangeLoss";
            data.BaseDrAmount = Math.round((data.Amount * data.CompanyCurrencyRate) * 10000 + Number.EPSILON) / 10000;

        }
        else if (data.CompanyCurrencyRate > $scope.selectedThirdDrRow[0].CompanyCurrencyRate) {
            data.ExchangeAmount = Math.round((data.Amount * (data.CompanyCurrencyRate - $scope.selectedThirdDrRow[0].CompanyCurrencyRate)) * 10000 + Number.EPSILON) / 10000;
            data.ExchangeType = "ExchangeGain";
            data.BaseDrAmount = Math.round((data.Amount * data.CompanyCurrencyRate) * 10000 + Number.EPSILON) / 10000;
        }
        else {
            data.ExchangeAmount = 0;
            data.BaseDrAmount = Math.round((data.Amount * data.CompanyCurrencyRate) * 10000 + Number.EPSILON) / 10000;
            data.ExchangeType = null;
        }
        //$scope.calBaseAmount();
    };

    $scope.excelReport = function (obj) {
        try {
            var file_src = 'Accounts/PartyReconciliation/ReportPartyReconciliation?reportFormat=' + 'Excel' + '&voucherId=' + obj.data.VoucherId
            $rootScope.report(file_src);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.pdfReport = function (obj) {
        try {
                $window.open('Accounts/PartyReconciliation/ReportPartyReconciliation?reportFormat=' + 'Pdf' + '&voucherId=' + obj.data.VoucherId, '_blank');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.confirmPost = function (data) {
        $scope.VoucherId = data.VoucherId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function (VoucherId) {
            $http({
                method: "POST",
                url: 'Accounts/Advance/PostPartyReconciliation',
                data: {
                    "voucherId": VoucherId,
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure", 'JournalPopUp');
            });

        return true;
    };

}