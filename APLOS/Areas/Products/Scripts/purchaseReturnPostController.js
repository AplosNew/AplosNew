"use strict";
purchaseReturnPostController.$inject = ["accountService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller",'factoryService'];
function purchaseReturnPostController(accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, factoryService) {
    $rootScope.title = "Purchase Return Post";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.salesDetailList = [];
    $scope.invoiceSalesAvailableList = [];
    $scope.currencyExchangeRate = [];
    $scope.partyType = "Vendor";
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
        NoteType: "VendorDebitNote",
        SettlementType: "Others",
        FinancingTypeId: null,
        CompanyCurrencyRate: 1
    };

    $scope.voucherDetail = {
        EntityId: null
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

    baseService.init("Products/InventoryPurchaseReturn/GetPurchaseReturnPostedData", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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
            "value": "Currency"
        }
    ];

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, "", function (result) {
                $scope.entityList = result;
            });
    });

    cboService.getCboVoucherTypeInventoryReturnPayableList(function (result) {
        $scope.voucherTypeList = result;
        if ($scope.voucherTypeList.length === 1) {
            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
            $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
            $scope.voucher.DocDate = $scope.voucher.PostingDate;
            $scope.GetCurrencyExchangeRateList();
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
        //if ($scope.invoiceSalesAvailableList.length === 0) {
        //    if ($scope.voucher.SettlementType === "Invoice") {
        //        ShowResult("Please select Invoice!", "failure");
        //        return true;
        //    }
        //    else {
        //        ShowResult("Please select GL!", "failure");
        //        return true;
        //    }
        //}
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
                    url: "products/InventoryPurchaseReturn/InsertPurchaseReturnPayable",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.newList,
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
        $scope.voucher.SettlementType = "Invoice";
        $scope.purchaseReturnList = [];
        $scope.newList = [];
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

    $scope.approvedPurchaseReturnList = [];
    $scope.getPopUpData = function () {
        $http({
            method: 'GET',
            url: 'Products/InventoryPurchaseReturn/GetPurchaseReturnPostableData',
        }).then(function successCallback(response) {
            $scope.approvedPurchaseReturnList = response.data;
            for (var i = 0; i < $scope.approvedPurchaseReturnList.length; i++) {
                response.data[i].GRNDate = new Date($scope.approvedPurchaseReturnList[i].GRNDate);
                response.data[i].DocDate = new Date($scope.approvedPurchaseReturnList[i].DocDate);
                response.data[i].PODate =  new Date($scope.approvedPurchaseReturnList[i].PODate);
            }
        });
    };
    $scope.popUp = function () {
        $scope.getPopUpData();
        angular.element(document.querySelector('#PurchaseReturnpopUp')).modal('show');
    };

    $scope.paymentTerm = function () {

        $scope.paymenttermUrl = "accounts/PaymentTerm/getvendorcbo";
        $http({
            method: "GET",
            url: $scope.paymenttermUrl
        }).then(function successCallback(response) {
            $scope.paymentTermList = response.data;
        });
    };

    $scope.selectDoubleClick = function (data) {
        var voucherTypeId = $scope.voucher.VoucherTypeId;
        $scope.voucher = data.data;
        $scope.voucher.VoucherTypeId = voucherTypeId;
        $scope.voucher.EmployeeTransactionTypeId = null;
        $scope.TempEmployeeId = data.data.EmployeeId;
        $scope.TotalPayableAmount = 0;

        $scope.voucher.PostingDate = data.data.GRNDateNew;
        $scope.voucher.GRNDateNew = data.data.GRNDateNew;
        $scope.voucher.VoucherDate = $filter("dateFiltering")(Date.now());
        $scope.voucher.CompanyCurrencyRate = data.data.ToCurrencyRate;
        $scope.voucher.PurchcaseReturnId = data.data.Id;
        $scope.voucher.Id = data.data.Id;
        $scope.paymentTerm();
        getPurchaseReturnList();
        getPurchaseReturnServiceList();
        //getServiceChargeList();
        getPurchaseReturnMaterialPayableList(data.data.Id, data.data.IsTaxApplicable);
        //getInventoryTaxList(data.data.Id);
        factoryService.getCurrencyPrecision(data.data.BaseCurrencyId);
        //GetCurrencyExchangeRateList();
        $scope.closePurchaseReturnPopUp();
    };

    $scope.closePurchaseReturnPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#PurchaseReturnpopUp')).modal('hide');
    };
    $scope.purchaseReturnList = [];
    function getPurchaseReturnList() {
        $http.get('Products/InventoryPurchaseReturn/GetGetPurchaseReturnMaterialList?purchaseReturnId=' + $scope.voucher.Id)
            .then(function (response) {
                $scope.purchaseReturnList = response.data.Rows;
                checkSameValueInColumnList($scope.purchaseReturnList, 'TransactionUoM');
            });
    }
    $scope.chargesList = [];
    function getPurchaseReturnServiceList() {
        $http.get('Products/InventoryPurchaseReturn/GetGetPurchaseReturnServiceList?purchaseReturnId=' + $scope.voucher.Id)
            .then(function (response) {
                $scope.chargesList = response.data.Rows;
            });
    }
    //function getVendorPayableGLBudgetActivity(inveReveiveId) {
    //    $http.get('Products/InventoryReceive/GetVendorPayableGLBudgetActivity?inveReveiveId=' + inveReveiveId)
    //        .then(function (response) {
    //            $scope.inventoryPayableList = [];
    //            $scope.inventoryPayableList = response.data;
    //        });
    //}
    function getPurchaseReturnMaterialPayableList(purchaseReturnId, isTaxApplicable) {
        $http.get('Products/InventoryPurchaseReturn/GetPurchaseReturnMaterialPayable?purchaseReturnId=' + purchaseReturnId + '&IsTaxApplicable=' + isTaxApplicable )
            .then(function (response) {
                $scope.inventoryPayableList = [];
                $scope.inventoryReceiveDetailList = [];
                $scope.inventoryMaterialList = [];
                $scope.newList = [];
                $scope.inventoryMaterialList = response.data;

                //if (!$scope.modelNew.IsNonCreditable)
                //    reArrangeCreditableList($scope.inventoryMaterialList, $scope.newList, $scope.inventoryReceiveDetailList);
                //else if ($scope.modelNew.IsNonCreditable)
                reArrangeCreditableList($scope.inventoryMaterialList, $scope.newList, $scope.inventoryReceiveDetailList);
                //if (!baseService.isUndefinedOrNull(employeeId))
                //    $scope.glPushInList();
                //if (baseService.isUndefinedOrNull(employeeId))
                //    getVendorPayableGLBudgetActivity(inveReveiveId);
            });
    }
    $scope.inventoryTaxList = [];
    function getInventoryTaxList(inveReveiveId) {
        $scope.inventoryTaxList = [];
        $http.get('Products/InventoryReceive/GetInventoryTaxList?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.inventoryTaxList = response.data;
            });
    }
    $scope.sumORnot = false;
    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }
    function reArrangeCreditableList(list, newList, newInvRecDetailList) {
        var svcList = ($filter('filter')(list, { OtherName: 'Svc' }, true));
        for (var t = 0; t < baseService.arrayLength(svcList); t++) {
            var row = svcList[t];
            if (row.OtherName === 'Svc' && row.TrnType === 'Dr') {
                var taxList = ($filter('filter')(list, { OtherName: 'Svc', TrnType: 'Dr', GLGeneralInfoId: row.GLGeneralInfoId, BudgetMasterId: row.BudgetMasterId, ActivityId: row.ActivityId }, true));
                row.Amount = parseFloat(row.Amount) / parseFloat(baseService.arrayLength(taxList));
                assignSvcInTax(row, list, 'Dr');
            }
            else if (row.OtherName === 'Svc' && row.TrnType === 'Cr') {
                var taxList = ($filter('filter')(list, { OtherName: 'Svc', TrnType: 'Cr', GLGeneralInfoId: row.GLGeneralInfoId, BudgetMasterId: row.BudgetMasterId, ActivityId: row.ActivityId }, true));
                row.Amount = parseFloat(row.Amount) / parseFloat(baseService.arrayLength(taxList));
                assignSvcInTax(row, list, 'Cr');
            }
        }
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            var row = list[i];
            if (row.OtherName === 'Tax' && row.TrnType === 'Dr' && row.Dr > 0) {
                var flag = false;
                for (var t = 0; t < baseService.arrayLength(newList); t++) {
                    if (row.OtherName === newList[t].OtherName && row.TrnType === newList[t].TrnType && row.GLGeneralInfoId === newList[t].GLGeneralInfoId && row.BudgetMasterId === newList[t].BudgetMasterId
                        && row.ActivityId === newList[t].ActivityId) {
                        newList[t].Dr += row.Dr;
                        newList[t].Amount += row.Dr;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    newList.push(list[i]);
            }
            else if (row.OtherName === 'Tax' && row.TrnType === 'Cr' && row.Cr > 0) {
                var has = false;
                for (var a = 0; a < baseService.arrayLength(newList); a++) {
                    if (row.OtherName === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId && row.BudgetMasterId === newList[a].BudgetMasterId
                        && row.ActivityId === newList[a].ActivityId) {
                        newList[a].Cr += row.Cr;
                        newList[a].Amount += row.Cr;
                        has = true;
                        break;
                    }
                }
                if (!has)
                    newList.push(list[i]);
            }
            else if (row.OtherName === 'TCS' && row.TrnType === 'Cr' && row.Cr > 0) {
                var has = false;
                for (var a = 0; a < baseService.arrayLength(newList); a++) {
                    if (row.OtherName === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId && row.BudgetMasterId === newList[a].BudgetMasterId
                        && row.ActivityId === newList[a].ActivityId) {
                        newList[a].Cr += row.Cr;
                        newList[a].Amount += row.Cr;
                        has = true;
                        break;
                    }
                }
                if (!has)
                    newList.push(list[i]);
            }

            else if (row.OtherName === 'Material' && row.TrnType === 'Cr') {
                newInvRecDetailList.push(list[i]);
                var has = false;
                for (var a = 0; a < baseService.arrayLength(newList); a++) {
                    if (row.OtherName === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId && row.BudgetMasterId === newList[a].BudgetMasterId && row.ActivityId === newList[a].ActivityId) {
                        var cr = parseFloat(newList[a].Cr.toFixed(4)) + parseFloat(row.Cr.toFixed(4));
                        newList[a].Cr = parseFloat(cr.toFixed(4));
                        newList[a].Amount = parseFloat(cr.toFixed(4));
                        has = true;
                        break;
                    }
                }
                if (!has)
                    newList.push(list[i]);
            }
            else if (row.OtherName === 'Return' && row.TrnType === 'Dr' && row.Dr > 0) {
                newInvRecDetailList.push(list[i]);
                var has = false;
                for (var a = 0; a < baseService.arrayLength(newList); a++) {
                    if (row.OtherName === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId && row.BudgetMasterId === newList[a].BudgetMasterId && row.ActivityId === newList[a].ActivityId) {
                        newList[a].Dr += row.Dr;
                        newList[a].Amount += row.Dr;
                        has = true;
                        break;
                    }
                }
                if (!has)
                    newList.push(list[i]);
            }
            else if (row.OtherName !== 'Svc' && row.OtherName === 'Vendor' && $scope.AcceptanceId === null && $scope.PurchaseLCId == null) {
                newList.push(list[i]);
                $scope.TotalPayableAmount += list[i].Amount;
            }
            else if (row.OtherName !== 'Svc' && row.OtherName === 'LCBase' && $scope.PurchaseLCId != null) {
                newList.push(list[i]);
                $scope.TotalPayableAmount += list[i].Amount;
            }
            //else if (row.OtherName !== 'Svc' && row.OtherName === 'Acceptance' && $scope.AcceptanceId !== null)
            //    newList.push(list[i]);
            //else newList.push(list[i]);
        }
    }
    function assignSvcInTax(row, taxList, trnType) {
        for (var i = 0; i < baseService.arrayLength(taxList); i++) {
            var row2 = taxList[i];
            if (row2.OtherName === 'Tax' && row2.TrnType === trnType && row2.GLGeneralInfoId === row.GLGeneralInfoId
                && row2.BudgetMasterId === row.BudgetMasterId && row2.ActivityId === row.ActivityId && row2.TaxCategoryId === row.TaxCategoryId) {
                row2[trnType] += row.Amount;
                row2.Amount += row.Amount;

            }
        }

    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.redirectTab = function () {
        if ($scope.tabForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.tabForm2.$invalid) {
            $scope.setTab(2);
        }
        else if ($scope.tabForm3.$invalid) {
            $scope.setTab(3);
        }
        else if ($scope.tabForm4.$invalid) {
            $scope.setTab(4);
        }
    };
}