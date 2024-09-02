'use strict';
inventoryPayableController.$inject = ['accountService','cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'factoryService', '$window'];
function inventoryPayableController(accountService,cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, factoryService, $window) {
    $rootScope.title = "Inventory Payable";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Accounts/InventoryPayable/';
    $scope.getListUrl = 'Accounts/InventoryPayable/GetPostingList/';
    $scope.saveUrl = 'Accounts/InvoicePost/GRNPost/';
    $scope.AcceptanceId = null;
    $scope.TotalPayableAmount = 0;
    $scope.ispostDisable = false;
    //$scope.getDataList = function () {
    //    baseService.init($scope.getListUrl, null, null, null, 'PartyName, PartyAccountGroupName, Id, GRNDate', 'PartyName');
    //    $scope.getData = function (pageno) {
    //        baseService.pagination(pageno)
    //            .then(function (result) {
    //                $scope.products = [];
    //                $scope.products = result.Rows;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //    };
    //    $scope.getData();
    //};


    $scope.searchByPostedGRN = "Id"; $scope.searchGRN = "";
    $scope.searchByPostedGRNList = [{ value: 'Id', name: "GRN No" }, { value: 'GRNDate', name: "GRN Date" }, { value: 'Particular', name: "Particular" }, { value: 'VoucherNo', name: "VoucherNo" }
        , { value: 'PostingDate', name: "PostingDate" }, { value: 'GateEntryNo', name: "Gate EntryNo" }, { value: 'DocRefNo', name: "DocRef No" }
        , { value: 'DocDate', name: "Doc Date" }];

    $scope.products = [];
    $scope.getDataList = function () {
        $http({
            method: 'POST',
            url: 'Accounts/InventoryPayable/GetPostingList',
            data: { column: $scope.searchByPostedGRN, value: $scope.searchGRN },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.products = response.data;
        });
    };
    $scope.getDataList();

    $scope.model = {
        AlongwithInvoice: null
        , BaseAmount: null
        , BaseCurrencyId: null
        , BaseNoOfDays: null
        , BaseOnDueDate: null
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: null
        , CurrencyCode: null
        , CurrencyId: null
        , DeliveryBy: null
        , DeliveryByAddress: null
        , DeliveryPartyPlantId: null
        , DeliveryState: null
        , DocDate: null
        , DocRefNo: null
        , EntryDate: null
        , FixedAssetOrInventory: null
        , GRNDate: null
        , GateEntryNo: null
        , Id: null
        , InvoiceDate: null
        , InvoiceNo: null
        , InvoicingBy: null
        , InvoicingByAddress: null
        , InvoicingPartyPlantId: null
        , InvoicingState: null
        , IsNonCreditable: null
        , MaterialStorageId: null
        , MatureDate: null
        , PODepended: null
        , PartyAccountGroupName: null
        , PartyCode: null
        , TransactionAmount: null
        , TransactionQty: null
        , TransactionUoM: null
        , TransactionUoMId: null
        , EmployeeTransactionTypeId: null
        , EmployeeId: null
        , EmployeeCode: null
        , EmployeeName: null
        , PaymentMode:null
        , PartyId: null
        , PartyPlantId: null
        , PartyName: null
        , PartyType: null
        , PaymentTermId: null
        , PaymentTermName: null
        , PostingDate: new Date()
        , VoucherTypeId: null
        , ToCurrencyRate: null
        , Narration: null
        , PaymentTermCode: null
        , AddtionalTax: null
        , IsInvoice: false
        , EntityId: null
        , GSTINNo: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    // #region Tab

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

    // #endregion Tab
    $scope.paymentTerm = function () {

        $scope.paymenttermUrl = "accounts/PaymentTerm/getvendorcbo";
        $http({
            method: "GET",
            url: $scope.paymenttermUrl
        }).then(function successCallback(response) {
            $scope.paymentTermList = response.data;
        });
    };
    $scope.paymentTerm();
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });

    $scope.changePaymentTerm = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) {
                return item.Value === id;
            })[0];
            $scope.modelNew.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.modelNew.BaseNoOfDays = paymentTerm.NoOfDay;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === "documentdate") {
                    $scope.modelNew.BaseOnDueDate = $scope.modelNew.DocDate;
                    $scope.IsBaseOnDueDateEnable = true;
                } else if (paymentTerm.BaseLineDate === "postingdate") {
                    $scope.modelNew.BaseOnDueDate = $scope.modelNew.PostingDate;
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else if (paymentTerm.BaseLineDate === "voucherdate") {
                    $scope.modelNew.BaseOnDueDate = $filter("dateFiltering")(Date.now());
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else {
                    $scope.IsBaseOnDueDateEnable = false;
                    $scope.modelNew.BaseOnDueDate = $filter("dateFiltering")(Date.now());
                }
            $scope.getMatureDate($scope.modelNew.BaseOnDueDate, $scope.modelNew.BaseNoOfDays);
        }
    };

    $scope.getMatureDate = function (date, days) {
        if (!baseService.isUndefinedOrNull(date)) {
            date = new Date(date);
            date.setDate(date.getDate() + days);
            $scope.modelNew.MatureDate = $filter("date")(date, "dd-MMM-yyyy");
        }
    };
    $scope.getMatureDateNew = function (date) {
        if (!baseService.isUndefinedOrNull(date)) {
            $scope.modelNew.MatureDate = $filter("date")(date, "dd-MMM-yyyy");
        }
    };

    $scope.getCboVoucherType = function () {
        cboService.getCboVoucherTypeAccountPayableList(function (result) {
            $scope.voucherTypeList = result;
            if (baseService.arrayLength($scope.voucherTypeList) === 1)
                $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
        });
    }

    $scope.GetCboExpensesBookingTranType = function () {
        cboService.GetCboExpensesBookingTransactionType(function (result) {
            $scope.employeeTransactionTypeList = result;
            //if ($scope.employeeTransactionTypeList.length === 1) {

            //$scope.modelNew.EmployeeTransactionTypeId = $scope.employeeTransactionTypeList[0].EmployeeTransactionTypeId;
            //}
        });
    }

    $scope.approvedGRNList = [];
    $scope.getPopUpData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/InventoryPayable/GetListForInvPayable',
        }).then(function successCallback(response) {
            $scope.approvedGRNList = response.data;
            for (var i = 0; i < $scope.approvedGRNList.length; i++) {
                response.data[i].GRNDate = new Date($scope.approvedGRNList[i].GRNDate);
                response.data[i].DocDate = new Date($scope.approvedGRNList[i].DocDate);
                response.data[i].PODate = new Date($scope.approvedGRNList[i].PODate);
            }
        });
    };
    $scope.popUp = function () {
        $scope.getPopUpData();
        angular.element(document.querySelector('#GRNpopUp')).modal('show');
    };
    $scope.PurchaseLCId = null;
    $scope.TempEmployeeId = null;
    $scope.IsInvoiceDisable = false;
    $scope.IsPaymentTermHide = true;

    $scope.controlInvoicePaymentTerm = function () {
        //if (!baseService.isUndefinedOrNull($scope.PurchaseLCId)) {
        //    $scope.IsInvoiceDisable = true;
        //    $scope.modelNew.IsInvoice = false;
        //    $scope.IsPaymentTermHide = true;
        //}
        //else
            if ($scope.modelNew.PaymentMode == 'LC') {
            $scope.IsInvoiceDisable = true;
            $scope.modelNew.IsInvoice = false;
            $scope.IsPaymentTermHide = true;
            
        }
        else if ($scope.TempEmployeeId != null) {
            $scope.IsInvoiceDisable = true;
            $scope.modelNew.IsInvoice = true;
            $scope.IsPaymentTermHide = true;

        }
        else if ($scope.modelNew.IsInvoice == true) {
            $scope.IsInvoiceDisable = false;
            $scope.IsPaymentTermHide = false;
            $scope.applyGIRI();
        }
        else if ($scope.modelNew.IsInvoice == false) {
            $scope.IsInvoiceDisable = false;
            $scope.IsPaymentTermHide = false;
            $scope.applyGIRI();
        }
    }
    $scope.applyGIRI = function () {
        if ($scope.modelNew.IsInvoice == false) {
            for (var i = 0; i < baseService.arrayLength($scope.newList); i++) {
                var row = $scope.newList[i];
                if (row.OtherName === 'Vendor') {
                    $scope.newList.splice(i, 1);
                    var newRow = ($filter('filter')($scope.inventoryMaterialList, { OtherName: 'LCBase' }));
                    $scope.newList.push(newRow[0]);
                }
            }
        }
        if ($scope.modelNew.IsInvoice == true) {
            for (var i = 0; i < baseService.arrayLength($scope.newList); i++) {
                var row = $scope.newList[i];
                if (row.OtherName === 'LCBase') {
                    $scope.newList.splice(i, 1);
                    var newRow = ($filter('filter')($scope.inventoryMaterialList, { OtherName: 'Vendor' }));
                    $scope.newList.push(newRow[0]);
                }
            }
        }
    }


    $scope.selectDoubleClick = function (data) {
        var voucherTypeId = $scope.modelNew.VoucherTypeId;
        $scope.modelNew = data.data;
        $scope.modelNew.VoucherTypeId = voucherTypeId;
        $scope.modelNew.EmployeeTransactionTypeId = null;
        $scope.TempEmployeeId = data.data.EmployeeId;
        $scope.AcceptanceId = data.data.AcceptanceId;
        $scope.AcceptanceDate = data.data.AcceptanceDate;
        $scope.PurchaseLCId = data.data.PurchaseLCId;
        $scope.IsAccepptanceFirst = data.data.IsAccepptanceFirst;
        $scope.LCNo = data.data.LCNo;
        $scope.modelNew.PaymentMode = data.data.PaymentMode;
        $scope.ContractId = data.data.ContractNo;
        $scope.modelNew.InvoiceNo = data.data.DocRefNo;
        $scope.modelNew.InvoiceDate = data.data.DocDate;
        $scope.modelNew.IsFOC = data.data.IsFOC;
        $scope.modelNew.ShortageQty = data.data.ShortageQty;
        $scope.modelNew.ShortageValue = data.data.ShortageValue;
        $scope.modelNew.OtherPartyDocRefNo = data.data.OtherPartyDocRefNo;
        $scope.modelNew.OtherPartyRCMApplicable = data.data.OtherPartyRCMApplicable;
        $scope.modelNew.IsInvoice = true;
        $scope.TDSList = [];
        $scope.controlInvoicePaymentTerm();
        //if (baseService.isUndefinedOrNull($scope.PurchaseLCId) && $scope.TempEmployeeId!=null) {
        //    $scope.modelNew.IsInvoice = true;
        //};
        $scope.TotalPayableAmount = 0;
        $scope.getCboVoucherType();
        
        $scope.modelNew.PostingDate = data.data.GRNDateNew;
        $scope.modelNew.GRNDateNew = data.data.GRNDateNew;
        if (!baseService.isUndefinedOrNull(data.data.EmployeeId)){
            $scope.GetCboExpensesBookingTranType();
            
        }
        //$scope.paymentTerm();
        getRecievedList();
        getServiceChargeList();
        getInventoryMaterialList(data.data.Id, data.data.EmployeeId, data.data.IsTaxApplicable, $scope.modelNew.IsFOC);
        getInventoryTaxList(data.data.Id);
        if (data.data.OtherPartyId) {
            getOtherVendorChargesList(data.data.Id, data.data.OtherPartyId, data.data.OtherPartyRCMApplicable);
        }
        if (data.data.GRNType == 'GRNBYPO') {

            $scope.GetPurchaseOrderDiscount(data.data.Id);
        }
        factoryService.getCurrencyPrecision(data.data.BaseCurrencyId);
        GetCurrencyExchangeRateList();
        $scope.getFiscalInvoiceTotalAmountByParty(data.data.PartyId, $scope.modelNew.PostingDate);
        $scope.ispostDisable = false;
        $scope.closeGRNPopUp();
    };

    $scope.closeGRNPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#GRNpopUp')).modal('hide');
    };
    function getVendorPayableGLBudgetActivity(inveReveiveId) {
        $http.get('Products/InventoryReceive/GetVendorPayableGLBudgetActivity?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.inventoryPayableList = [];
                $scope.inventoryPayableList = response.data;
            });
    }
    function getInventoryMaterialList(inveReveiveId, employeeId, isReversCharge, foc) {
        $http.get('Products/InventoryReceive/GetInventoryMaterialPayable?inveReveiveId=' + inveReveiveId + '&employeeId=' + employeeId + '&isReversCharge=' + isReversCharge + '&foc=' + foc)
            .then(function (response) {
                $scope.inventoryPayableList = [];
                $scope.inventoryReceiveDetailList = [];
                $scope.inventoryMaterialList = [];
                $scope.newList = [];
                $scope.inventoryMaterialList = response.data;

                if (!$scope.modelNew.IsNonCreditable)
                    reArrangeCreditableList($scope.inventoryMaterialList, $scope.newList, $scope.inventoryReceiveDetailList);
                else if ($scope.modelNew.IsNonCreditable)
                    reArrangeNonCreditableList($scope.inventoryMaterialList, $scope.newList, $scope.inventoryReceiveDetailList);
                if (!baseService.isUndefinedOrNull(employeeId))
                    $scope.glPushInList();
                if (baseService.isUndefinedOrNull(employeeId))
                    getVendorPayableGLBudgetActivity(inveReveiveId);
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

    $scope.OtherVendorChargesPayableList = [];
    function getOtherVendorChargesList(inveReveiveId, otherVendorId,rcmApplicable) {
        $http.get('Products/InventoryReceive/GetOtherVendorChargesPayable?inveReveiveId=' + inveReveiveId + '&otherPartyId=' + otherVendorId + '&rcmApplicable=' + rcmApplicable)
            .then(function (response) {
                $scope.OtherVendorChargesPayableList = [];
                $scope.OtherVendorChargesPayableList = response.data;
            });
    }

    $scope.PurchaseOrderDiscountList = [];
    $scope.GetPurchaseOrderDiscount = function (id) {
        $http({
            method: 'POST',
            url: 'Accounts/InventoryPayable/GetPurchaseOrderDiscount?grnId=' + id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PurchaseOrderDiscountList = response.data;
            $scope.PODiscountAmount = $scope.PurchaseOrderDiscountList[0].DiscountAmount;
        });
    };

    $scope.purchcaseDiscountList = [];


    $scope.GetPurchaseDiscountGL = function () {
        $http({
            method: 'POST',
            url: 'Accounts/InventoryPayable/GetPurchaseDiscountGL',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.purchcaseDiscountList = response.data;

        });
    };
    $scope.GetPurchaseDiscountGL();

    $scope.materialConfigMassege = function () {
        if (!baseService.isUndefinedOrNull($scope.TempEmployeeId) && baseService.isUndefinedOrNull($scope.modelNew.EmployeeTransactionTypeId))
            ShowResult('Please Select Transaction Type', 'failure');
        else {
            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                if ($scope.inventoryMaterialList[i].IsAsset && $scope.inventoryMaterialList[i].TrnType == 'Dr' && baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].BudgetMasterId)) {
                    var matreialRow = ($filter('filter')($scope.inventoryReceivedList, { "InventoryReceiveDetailId": $scope.inventoryMaterialList[i].InventoryReceiveDetailId }));
                    if (baseService.isUndefinedOrNull(matreialRow[0].BudgetMasterId)) {
                        ShowResult('In Material Master, ' + matreialRow[0].UserName + ' is Asset but Budget and Activity are missing !!', 'failure');
                    }
                    else if (baseService.isUndefinedOrNull(matreialRow[0].FixedAssetMasterId)) {
                        ShowResult(matreialRow[0].BudgetName + ' Budget,  Asset Master is missing !!', 'failure');
                    }
                    else {
                        ShowResult(matreialRow[0].FixedAssetMasterName + ' Fixed Asset Master, Asset Under Constraction (AUC) is not determinate !!', 'failure');
                    }
                }
                else if ($scope.inventoryMaterialList[i].IsAsset == 0 && $scope.inventoryMaterialList[i].TrnType == 'Dr' && baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].BudgetMasterId)) {
                    var matreialRow = ($filter('filter')($scope.inventoryReceivedList, { "InventoryReceiveDetailId": $scope.inventoryMaterialList[i].InventoryReceiveDetailId }));
                    if (baseService.isUndefinedOrNull(matreialRow[0].BudgetMasterId)) {
                        ShowResult('In Material Group Determinate, ' + matreialRow[0].MaterialGroupMasterName + ',  Inventory GL,Budget and Activity are missing !!', 'failure');
                    }
                }
                // NEED TO ADD in Query MaterialGroupMasterId 
                else if ($scope.inventoryMaterialList[i].IsAsset == 0 && $scope.inventoryMaterialList[i].TrnType == 'Cr' && baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].BudgetMasterId)) {
                    ShowResult('In Material Group Determinate,  Vendor  GL,Budget and Activity are missing !!', 'failure');
                }
            }
            if ($scope.inventoryTaxList.length > 0 && $scope.modelNew.IsNonCreditable == false) {
                for (var i = 0; i < $scope.inventoryTaxList.length; i++) {
                    if ($scope.inventoryTaxList[i].ActivityId == null)
                        ShowResult('In Tax Category Determinate,  Tax  GL,Budget and Activity are missing !!', 'failure');
                }
            }
        }
    }


    function reArrangeCreditableList(list, newList, newInvRecDetailList) {
        //var svcList = ($filter('filter')(list, { OtherName: 'Svc' }, true));
        //for (var t = 0; t < baseService.arrayLength(svcList); t++) {
        //    var row = svcList[t];
        //    if (row.OtherName === 'Svc' && row.TrnType === 'Dr') {
        //        var taxList = ($filter('filter')(list, { OtherName: 'Svc', TrnType: 'Dr', GLGeneralInfoId: row.GLGeneralInfoId, BudgetMasterId: row.BudgetMasterId, ActivityId: row.ActivityId }, true));
        //        row.Amount = parseFloat(row.Amount) / parseFloat(baseService.arrayLength(taxList));
        //        assignSvcInTax(row, list, 'Dr');
        //    }
        //    else if (row.OtherName === 'Svc' && row.TrnType === 'Cr') {
        //        var taxList = ($filter('filter')(list, { OtherName: 'Svc', TrnType: 'Cr', GLGeneralInfoId: row.GLGeneralInfoId, BudgetMasterId: row.BudgetMasterId, ActivityId: row.ActivityId }, true));
        //        row.Amount = parseFloat(row.Amount) / parseFloat(baseService.arrayLength(taxList));
        //        assignSvcInTax(row, list, 'Cr');
        //    }
        //}
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            var row = list[i];
            //if (row.OtherName === 'Svc' && row.TrnType === 'Dr' && row.Dr > 0) {
            //    var has = false;
            //    for (var t = 0; t < baseService.arrayLength(newList); t++) {
            //        if ('Tax' === newList[t].OtherName && row.TrnType === newList[t].TrnType && row.GLGeneralInfoId === newList[t].GLGeneralInfoId && row.BudgetMasterId === newList[t].BudgetMasterId
            //            && row.ActivityId === newList[t].ActivityId) {
            //            newList[t].Dr += row.Dr;
            //            newList[t].Amount += row.Dr;
            //            flag = true;
            //            break;
            //        }
            //    }
            //    if (!has) {
            //        list[i].OtherName = 'Tax';
            //        newList.push(list[i]);
            //    }
            //}
            //else if (row.OtherName === 'Svc' && row.TrnType === 'Cr' && row.Cr > 0) {
            //    var has = false;
            //    for (var a = 0; a < baseService.arrayLength(newList); a++) {
            //        if ('Tax' === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId && row.BudgetMasterId === newList[a].BudgetMasterId
            //            && row.ActivityId === newList[a].ActivityId) {
            //            newList[a].Cr += row.Cr;
            //            newList[a].Amount += row.Cr;
            //            has = true;
            //            break;
            //        }
            //    }
            //    if (!has) {
            //        list[i].OtherName = 'Tax';
            //        newList.push(list[i]);
            //    }
            //}
            if (row.OtherName === 'Tax' && row.TrnType === 'Dr' && row.Dr > 0) {
                var has = false;
                for (var a = 0; a < baseService.arrayLength(newList); a++) {
                    if (row.OtherName === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId && row.BudgetMasterId === newList[a].BudgetMasterId
                        && row.ActivityId === newList[a].ActivityId) {
                        newList[a].Dr += row.Dr;
                        newList[a].Amount += row.Dr;
                        has = true;
                        break;
                    }
                }
                if (!has)
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
            else if (row.OtherName === 'TCS' && row.TrnType === 'Dr' && row.Dr > 0) {
                var has = false;
                for (var a = 0; a < baseService.arrayLength(newList); a++) {
                    if (row.OtherName === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId && row.BudgetMasterId === newList[a].BudgetMasterId
                        && row.ActivityId === newList[a].ActivityId) {
                        newList[a].Dr += row.Dr;
                        newList[a].Amount += row.Dr;
                        has = true;
                        break;
                    }
                }
                if (!has)
                    newList.push(list[i]);
            }

            else if (row.OtherName === 'Material' && row.TrnType === 'Dr') {
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
            else if (row.OtherName === 'Shortage' && row.TrnType === 'Dr' && row.Dr > 0) {
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
            else if (row.OtherName !== 'Svc' && row.OtherName === 'Vendor' && $scope.AcceptanceId === null && $scope.PurchaseLCId == null && $scope.modelNew.PaymentMode !== 'LC') {
                newList.push(list[i]);
                $scope.TotalPayableAmount += list[i].Amount;
            }
            else if (row.OtherName !== 'Svc' && row.OtherName === 'LCBase' && $scope.PurchaseLCId != null && $scope.IsAccepptanceFirst == false) {
                newList.push(list[i]);
                $scope.TotalPayableAmount += list[i].Amount;
            }
            else if (row.OtherName !== 'Svc' && row.OtherName === 'LCBase' && $scope.modelNew.PaymentMode == 'LC') {
                newList.push(list[i]);
                $scope.TotalPayableAmount += list[i].Amount;
            }
            else if (row.OtherName === 'Acceptance' && $scope.AcceptanceId !== null && $scope.IsAccepptanceFirst == true) {
                newList.push(list[i]);
                $scope.TotalPayableAmount += list[i].Amount;
            }
                
            //else newList.push(list[i]);
        }
        distributeTCSAmount();
    }

    function distributeTCSAmount() {
        var vendorList = ($filter('filter')($scope.newList, { OtherName: 'Vendor' }, true));
        if (vendorList.length > 1) {
            for (var z = 0; z < vendorList.length; z++) {
                if (z > 0) {
                    var totaltcsAmount = Math.round($filter("sumByKey")($filter("filter")($scope.newList, { OtherName: 'TCS' }), "Amount") * 100 + Number.EPSILON) / 100;
                    for (var y = 0; y < $scope.newList.length; y++) {
                        if (vendorList[z].OtherName == $scope.newList[y].OtherName && vendorList[z].TrnType === $scope.newList[y].TrnType
                            && vendorList[z].GLGeneralInfoId === $scope.newList[y].GLGeneralInfoId && vendorList[z].BudgetMasterId === $scope.newList[y].BudgetMasterId
                            && vendorList[z].ActivityId === $scope.newList[y].ActivityId) {
                            $scope.newList[y].Cr -= totaltcsAmount;
                            $scope.newList[y].Amount -= totaltcsAmount;
                        }
                    }
                }
            }
        }
    }

    function distinct(taxList) {

        var lst = [];
        var newList = [];
        var newListRow = {};
        for (var i = 0; i < taxList.length; i++) {
            if (!lst.includes(taxList[i].TaxCategoryID)) {
                lst.push(taxList[i].TaxCategoryID);

                var svcList = ($filter('filter')(taxList, { TaxCategoryID: taxList[i].TaxCategoryID }, true));

                var sum = 0;
                for (var j = 0; j < svcList.length; j++) {
                    sum += svcList[j].Amount;
                }
                newListRow = taxList[i];
                newListRow.Amount = sum;
                newList.push(newListRow);
            }
        }

    }
    function assaignTax(taxList, newList) {

        var lst = [];//use only for check duplicate.
        // var newList = [];
        var newListRow = {};
        for (var i = 0; i < taxList.length; i++) {
            // var rowset = ($filter('filter')(taxList, { GLGeneralInfoId: taxList[i].GLGeneralInfoId, BudgetMasterId: taxList[i].BudgetMasterId, ActivityId: taxList[i].ActivityId }, true));
            if (!lst.includes(taxList[i].ActivityId)) {
                lst.push(taxList[i].ActivityId);
                var svcList = ($filter('filter')(taxList, { GLGeneralInfoId: taxList[i].GLGeneralInfoId, BudgetMasterId: taxList[i].BudgetMasterId, ActivityId: taxList[i].ActivityId }, true));

                var sum = 0;
                for (var j = 0; j < svcList.length; j++) {
                    sum += svcList[j].Amount;
                }
                newListRow = taxList[i];
                newListRow.Amount = sum;
                newListRow.Dr = sum;
                newList.push(newListRow);
            }
        }

    }


    function reArrangeNonCreditableList(list, newList, newInvRecDetailList) {
        var svcList = ($filter('filter')(list, { OtherName: 'Svc' }, true));
        var taxList0 = ($filter('filter')(list, { OtherName: 'Tax' }, true));
        var taxList = taxList0.concat(svcList);
        assaignTax(taxList, newList);
        //for (var t = 0; t < baseService.arrayLength(svcList); t++) {
        //    var row = svcList[t];
        //    if (row.OtherName === 'Svc' && row.TrnType === 'Dr') {
        //        var taxList = ($filter('filter')(list, { OtherName: 'Tax', TrnType: 'Dr', GLGeneralInfoId: row.GLGeneralInfoId, BudgetMasterId: row.BudgetMasterId, ActivityId: row.ActivityId, TaxCategoryId: row.TaxCategoryId }, true));
        //       // row.Amount = parseFloat(row.Amount) / parseFloat(baseService.arrayLength(taxList));
        //        assignSvcInTax(row, taxList, 'Dr');
        //    }
        //    else if (row.OtherName === 'Svc' && row.TrnType === 'Cr') {
        //        var taxList = ($filter('filter')(list, { OtherName: 'Tax', TrnType: 'Cr', GLGeneralInfoId: row.GLGeneralInfoId, BudgetMasterId: row.BudgetMasterId, ActivityId: row.ActivityId, TaxCategoryId: row.TaxCategoryId }, true));
        //        row.Amount = parseFloat(row.Amount) / parseFloat(baseService.arrayLength(taxList));
        //        assignSvcInTax(row, taxList, 'Cr');
        //    }
        //}

        for (var i = 0; i < baseService.arrayLength(list); i++) {
            var row = list[i];
            if (row.OtherName === 'Material' && row.TrnType === 'Dr') {
                newInvRecDetailList.push(list[i]);
                var flag = false;
                for (var t = 0; t < baseService.arrayLength(newList); t++) {
                    if (row.OtherName === newList[t].OtherName && row.TrnType === newList[t].TrnType && row.GLGeneralInfoId === newList[t].GLGeneralInfoId && row.BudgetMasterId === newList[t].BudgetMasterId && row.ActivityId === newList[t].ActivityId) {
                        newList[t].Dr += row.Dr;
                        newList[t].Amount += row.Dr;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    newList.push(list[i]);
            }
            //else if (row.OtherName == 'Charge' || row.OtherName == 'Vendor')
            //    newList.push(list[i]);

            else if (row.OtherName === 'Vendor' && $scope.AcceptanceId === null)
                newList.push(list[i]);
            else if (row.OtherName === 'Acceptance' && $scope.AcceptanceId !== null)
                newList.push(list[i]);

            //else if(row.OtherName !== 'Svc')
            //    if(row.OtherName !== 'Material')
            //    newList.push(list[i]);
            //else newList.push(list[i]);
        }
    }

    function assignSvcInTax(row, taxList, trnType) {
        // $scope.TotalTaxAmount = parseFloat($filter('sumByKey')($filter('filter')(taxList, { OtherName: 'Tax' }), 'Amount'));

        for (var i = 0; i < baseService.arrayLength(taxList); i++) {
            var row2 = taxList[i];
            if (row2.OtherName === 'Tax' && row2.TrnType === trnType && row2.GLGeneralInfoId === row.GLGeneralInfoId
                && row2.BudgetMasterId === row.BudgetMasterId && row2.ActivityId === row.ActivityId && row2.TaxCategoryId === row.TaxCategoryId) {
                row2[trnType] += row.Amount;
                row2.Amount += row.Amount;
                //}
                //else {
                //    row2[trnType] += (row.Amount * row2.Amount) / $scope.TotalTaxAmount;
                //    row2.Amount += (row.Amount * row2.Amount) / $scope.TotalTaxAmount;
                //}

            }

        }
    }

    $scope.glPushInList = function () {
        var data = $filter('filter')($scope.employeeTransactionTypeList, { EmployeeTransactionTypeId: $scope.modelNew.EmployeeTransactionTypeId }, true);
        for (var i = 0; i < baseService.arrayLength($scope.newList); i++) {
            if ($scope.newList[i].OtherName === 'Vendor') {
                if (baseService.arrayLength(data) > 0) {
                    $scope.newList[i].GLGeneralInfoId = data[0].PayableGLId;
                    $scope.newList[i].GLGeneralInfoCode = data[0].PayableGLCode;
                    $scope.newList[i].GLGeneralInfoName = data[0].PayableGLName;
                    $scope.newList[i].BudgetMasterId = data[0].PayableBudgetMasterId;
                    $scope.newList[i].BudgetCode = data[0].PayableBudgetCode;
                    $scope.newList[i].BudgetName = data[0].PayableBudgetName;
                    $scope.newList[i].ActivityId = data[0].PayableActivityId;
                    $scope.newList[i].ActivityCode = data[0].PayableActivityCode;
                    $scope.newList[i].ActivityName = data[0].PayableActivityName;
                    $scope.newList[i].BudgetActive = data[0].PayableBudgetActive;
                    $scope.newList[i].BudgetMasterActivityActive = data[0].PayableBudgetMasterActivityActive;
                }
                else {
                    if ($scope.modelNew.EmployeeTransactionTypeId != null) {
                        for (var k = 0; k < $scope.newList.length; k++) {
                            if ($scope.newList[i].BudgetMasterId == $scope.newList[k].BudgetMasterId
                                && $scope.newList[i].ActivityId == $scope.newList[k].ActivityId) {
                                $scope.newList[k].Cr += $scope.newList[i].Amount;
                                $scope.newList[k].Amount += $scope.newList[i].Amount;
                            }
                        }

                    }
                }
            }
        }
    };

    $scope.parallelCurrencyTypeList = [];
    $scope.companyCurrencyId = null;
    $scope.companyGroupCurrencyId = null;
    $scope.hardCurrencyId = null;
    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CurrencyParallel'
    }).then(function successCallback(response) {
        angular.forEach(response.data, function (item, i) {
            if (item.ParallelCurrencyType === 'CompanyCurrency') {
                $scope.companyCurrencyId = item.CurrencyId;
                $scope.companyCurrencyName = item.Code;
                $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyCurrency', CurrencyType: 'CompanyCurrencyDr', CurrencyId: item.CurrencyId });
                $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyCurrency', CurrencyType: 'CompanyCurrencyCr', CurrencyId: item.CurrencyId });
            }
        });
    });
    function GetCurrencyExchangeRateList() {
        //$scope.modelNew.GRNDate = $filter("dateFiltering")(Date.now($scope.modelNew.GRNDate));
        if ($scope.modelNew.CurrencyId !== null && undefined !== $scope.modelNew.CurrencyId) {
            $http({
                method: 'GET',
                url: 'currencies/ExchangeRate/ParallelExchangeRate?fromdate=' + $scope.modelNew.GRNDateNew + '&currencyId=' + $scope.modelNew.CurrencyId
            }).then(function (response) {
                $scope.currencyExchangeRate = [];
                for (var i = 0; i < baseService.arrayLength(response.data); i++) {
                    $scope.currencyExchangeRate.push({
                        CompanyCurrencyId: $scope.companyCurrencyId
                        , CompanyCurrencyName: $scope.companyCurrencyName
                        , CompanyFromCurrencyId: response.data[i].FromCurrencyId
                        , ToCurrencyId: response.data[i].ToCurrencyId
                        , CompanyCurrencyRate: response.data[i].ToCurrencyRate

                        , FromCurrencyUnit: response.data[i].FromCurrencyUnit
                        , FromCurrencyCode: response.data[i].FromCurrencyCode
                    });
                }
            });
        }
    }

    $scope.getNewDataList = function (grnId, docrefno) {
        $scope.products = [];
        $scope.searchGRN = grnId;
        $http({
            method: 'POST',
            url: 'Accounts/InventoryPayable/GetPostingList',
            data: { column: $scope.searchByPostedGRN, value: $scope.searchGRN },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.products = response.data;
            var rowdata = $filter("filter")($scope.products, { "Id": grnId, "DocRefNo": docrefno });
            if (!baseService.isUndefinedOrNull(rowdata[0].AdditionalTaxId)) {
                $scope.onClickadditionalTaxPop(rowdata[0]);
            }
            $scope.Clear();
        });
    };
    $scope.updatePayableGL = function () {
        for (var i = 0; i < $scope.newList.length; i++) {
            if ($scope.newList[i].OtherName == 'LCBase' && $scope.newList[i].TrnType == 'Cr') {
                for (var j = 0; j < $scope.inventoryPayableList.length; j++) {
                    if ($scope.inventoryPayableList[j].TrnType == 'Cr') {
                        $scope.inventoryPayableList[j].GLGeneralInfoId = $scope.newList[i].GLGeneralInfoId;
                        $scope.inventoryPayableList[j].BudgetMasterId = $scope.newList[i].BudgetMasterId;
                        $scope.inventoryPayableList[j].ActivityId = $scope.newList[i].ActivityId;
                    }
                }    
            }
        }
    }
    $scope.Post = function () {
        
        if (baseService.isUndefinedOrNull($scope.modelNew.EntityId)) return ShowResult('Please Select Entity', 'failure');
        if ($scope.modelNew.IsInvoice && $scope.modelNew.EmployeeId == null && $scope.modelNew.PaymentTermId == null)
            return ShowResult("Please select Payment Term");
        if (!baseService.isUndefinedOrNull($scope.modelNew.EmployeeId)) {
            var data = $filter('filter')($scope.newList, { OtherName: 'Vendor' }, true);
            if (baseService.isUndefinedOrNull(data[0].GLGeneralInfoId)) return ShowResult('Employee GL not found', 'failure');
            if (baseService.isUndefinedOrNull(data[0].BudgetMasterId)) return ShowResult('Employee budget not found', 'failure');
            if (baseService.isUndefinedOrNull(data[0].ActivityId)) return ShowResult('Employee activity not found', 'failure');
            for (var i = 0; i < baseService.arrayLength($scope.inventoryMaterialList); i++) {
                if ($scope.inventoryMaterialList[i].OtherName === 'Vendor') {
                    $scope.inventoryMaterialList[i].GLGeneralInfoId = data[0].GLGeneralInfoId;
                    $scope.inventoryMaterialList[i].GLGeneralInfoCode = data[0].GLGeneralInfoCode;
                    $scope.inventoryMaterialList[i].GLGeneralInfoName = data[0].GLGeneralInfoName;
                    $scope.inventoryMaterialList[i].BudgetMasterId = data[0].BudgetMasterId;
                    $scope.inventoryMaterialList[i].BudgetCode = data[0].BudgetCode;
                    $scope.inventoryMaterialList[i].BudgetName = data[0].BudgetName;
                    $scope.inventoryMaterialList[i].ActivityId = data[0].ActivityId;
                    $scope.inventoryMaterialList[i].ActivityCode = data[0].ActivityCode;
                    $scope.inventoryMaterialList[i].ActivityName = data[0].ActivityName;
                }
            }
          
            $scope.modelNew.MatureDate = $filter("date")($scope.modelNew.NewBaseOnDueDate, "dd-MMM-yyyy");
            $scope.modelNew.BaseOnDueDate = $filter("date")($scope.modelNew.NewBaseOnDueDate, "dd-MMM-yyyy");
        }
        $scope.updatePayableGL();
        for (var i = 0; i < $scope.newList.length; i++) {
            $scope.newList[i].Amount = parseFloat($scope.newList[i].Amount).toFixed(4);
        }
        $scope.ispostDisable = true;
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                receiveId: $scope.modelNew.Id
                , acceptanceId: $scope.AcceptanceId
                , voucherVM: $scope.modelNew
                , voucherDetailVMList: $scope.newList/*$scope.inventoryMaterialList*/
                , voucherDetailCurrencyVMList: $scope.currencyExchangeRate
                , inventoryPayableVMList: $scope.inventoryPayableList
                , inventoryReceiveDetailVMList: $scope.inventoryReceiveDetailList
                , tdsTaxList: $scope.TDSList
                , otherVendorChargesList: $scope.OtherVendorChargesPayableList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                $scope.ispostDisable = false;
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ispostDisable = true;
                $scope.getNewDataList($scope.modelNew.Id, $scope.modelNew.DocRefNo);
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.Clear = function () {
        $scope.model = {};
        $scope.modelNew = { PostingDate: new Date() };
        $scope.inventoryMaterialList = [];
        $scope.currencyExchangeRate = [];
        $scope.inventoryReceivedList = [];
        $scope.inventoryPayableList = [];
        $scope.inventoryReceiveDetailList = [];
        $scope.advanceTaxesList = [];
        $scope.newList = [];
        $scope.TDSList = [];
        $scope.DiscountAmount = 0;
        $scope.PODiscountAmount = 0;
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
        $scope.ispostDisable = false;
    };

    function getRecievedList() {
        $http.get('Products/GoodsReceiveNote/GetInventoryMaterialPayableList?inveReveiveId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.inventoryReceivedList = response.data.Rows;
                checkSameValueInColumnList($scope.inventoryReceivedList, 'TransactionUoM');
            });
    }

    function getServiceChargeList() {
        $http.get('Products/GoodsReceiveNote/GetServiceChargeList?receiveId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.chargesList = [];
                $scope.chargesList = response.data;
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

    $scope.getPabyableJournal = function (data, reportFormat) {
        $window.open($scope.path + 'PabyableJournal?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&employeeId=' + data.EmployeeId + '&isReversCharge=' + data.IsTaxApplicable + '&otherVendorId=' + data.OtherPartyId, '_blank');
    };



    $scope.onClickReportDownloadWord = function (data) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open($scope.path + 'PabyableJournal?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&employeeId=' + data.EmployeeId + '&isReversCharge=' + data.IsTaxApplicable + '&isFoc=' + data.IsFOC + '&otherVendorId=' + data.OtherPartyId, '_blank');

    };

    $scope.commandPDF = [{
        type: "details", buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",
            click: $scope.onClickReportDownloadWord
        }
    }];

    $scope.onClickReportDownloadExcel = function (data) {
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open($scope.path + 'PabyableJournal?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&employeeId=' + data.EmployeeId + '&isReversCharge=' + data.IsTaxApplicable + '&isFoc=' + data.IsFOC + '&otherVendorId=' + data.OtherPartyId, '_blank');
    };
   

    $scope.downloadGRN = function () {
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + $scope.modelNew.Id;
    };

    $scope.onClickGRNID = function (data) {
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id;
    };
  
    $scope.taxCodCboList = [];
    $scope.taxcodelistMessage = "";


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



    $scope.copyTaxesAmount = function () {
        if ($scope.advance.CurrencyId === $scope.companyCurrencyId) {
            $scope.advanceTax.CompanyCurrencyAmount = $scope.advanceTax.TaxAmount;
        }
        else {
            $scope.advanceTax.CompanyCurrencyAmount = ($scope.advanceTax.TaxAmount * $scope.advance.CompanyCurrencyRate).toFixed(2);
        }
    };

    $scope.removeTaxesRow = function (index) {
        $scope.advanceTaxesList.splice(index, 1);
    };
    $scope.voucherTypeListnew = [];
    $scope.additionalTaxVoucherTypeId = null;
    $scope.getPaymentVoucherType = function () {
        cboService.getCboVoucherTypePaymentList(function (result) {
            $scope.voucherTypeListnew = result;
            if (baseService.arrayLength($scope.voucherTypeListnew) === 1)
                $scope.additionalTaxVoucherTypeId = $scope.voucherTypeListnew[0].Value;
        });
    }
   
    $scope.additionalTaxPostUrl = 'Accounts/InvoicePost/InsertAdditionalTaxPayable';
    $scope.additionalTaxDetailList = [];
    $scope.onClickadditionalTaxPop = function (x) {
        $scope.additionalTaxData = {};
        var data = x;
        data.VoucherTypeId = null;
        data.VoucherTypeId = $scope.additionalTaxVoucherTypeId;
        data.VoucherDate = new Date();
        $scope.additionalTaxData = data;
        $http({
            method: 'POST',
            url: 'Accounts/InventoryPayable/GetAdditionalTaxDetail?additionalTaxId=' + data.AdditionalTaxId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.additionalTaxDetailList = response.data;
        });
        $scope.getPaymentVoucherType();
        angular.element(document.querySelector('#additionalTaxPopUp')).modal('show');
    };

    $scope.postAdditionalTax = function () {
        if ($scope.additionalTaxVoucherTypeId == null)
            ShowResult('Please select VoucherType', 'failure', 'additionalTaxPopUp');

        $scope.additionalTaxData.VoucherTypeId = $scope.additionalTaxVoucherTypeId;
        if ($scope.additionalTaxData != null && $scope.additionalTaxVoucherTypeId != null) {
            $http({
                method: 'POST',
                url: $scope.additionalTaxPostUrl,
                data: {
                    "additionalTaxId": $scope.additionalTaxData.AdditionalTaxId
                    , "voucherVM": $scope.additionalTaxData
                },
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getDataList();
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
            angular.element(document.querySelector('#additionalTaxPopUp')).modal('hide');
        }

    }
    $scope.closeAdditionalTax = function () {
        $scope.additionalTaxData = {};
        angular.element(document.querySelector('#additionalTaxPopUp')).modal('hide');

    }
    //$scope.additionalTaxPop = [{
    //    type: "details", buttonOptions: {
    //        text: "TDS Post",
    //        width: "80",
    //        height: "20",
    //        click: $scope.onClickadditionalTaxPop
    //    }
    //}];

    $scope.additionalTaxPrint = function () {
        try {
            var file_src = 'Accounts/invoice/VendorInvoicePaymentReport?reportFormat=' + 'Excel' + '&voucherId=' + $scope.additionalTaxData.TDSTaxVoucherId
            $rootScope.report(file_src);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.shortageQtyPostUrl = 'Accounts/InvoicePost/InsertShortageDebitNote';
    $scope.shortageQtyDetailList = [];
    $scope.onClickshortageQtyPop = function (x) {
        $scope.shortageQtyData = {};
        var data = x;
        data.VoucherTypeId = null;
        data.VoucherTypeId = $scope.additionalTaxVoucherTypeId;
        data.VoucherDate = new Date();
        $scope.shortageQtyData = data;
        $http({
            method: 'POST',
            url: 'Accounts/InventoryPayable/GetShortageQtyDetail?grnId=' + data.Id + '&adjustmentNoteTypeId=' + data.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.shortageValueJournalList = response.data;
        });
        $scope.getShortageVoucherType();
        $scope.transactionTypeList();
        angular.element(document.querySelector('#shortagePopUp')).modal('show');
    };
    $scope.closeShortageQtyPopUp = function () {
        $scope.shortageQtyData = {};
        angular.element(document.querySelector('#shortagePopUp')).modal('hide');

    }
    $scope.transactionTypeList = function () {
        $scope.financingTypeList = [];
        $scope.partyType = 'Vendor';
        accountService.getCboDebitNoteTypeList($scope.partyType, function (result) {
            $scope.financingTypeList = result;
        });
    };


    $scope.getShortageValueJournal = function (grnId,adjustmentNoteTypeId) {
        $scope.tempshortageValueJournalList = [];
        $scope.shortageValueJournalList = [];
        $http({
            method: 'POST',
            url: 'Accounts/InventoryPayable/GetShortageQtyDetail?grnId=' + grnId + '&adjustmentNoteTypeId=' + adjustmentNoteTypeId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.tempshortageValueJournalList = response.data;
            var tempshortageValue = Math.round($filter("sumByKey")($filter("filter")($scope.tempshortageValueJournalList, { TrnType: 'Cr' }), "CrAmount") * 100 + Number.EPSILON) / 100;
            for (var i = 0; i < $scope.tempshortageValueJournalList.length; i++) {
                if ($scope.tempshortageValueJournalList[i].AType == 'Dr') {
                    $scope.tempshortageValueJournalList[i].DrAmount = tempshortageValue
                    $scope.tempshortageValueJournalList[i].Amount = tempshortageValue
                }
                if ($scope.tempshortageValueJournalList[i].Amount > 0) {
                    $scope.shortageValueJournalList.push($scope.tempshortageValueJournalList[i])
                }
            }
        });
    }

    $scope.shortageVoucherTypeList = [];
    $scope.getShortageVoucherType = function () {
        cboService.getCboVoucherTypeDebitNoteList(function (result) {
            $scope.shortageVoucherTypeList = result;
            if ($scope.shortageVoucherTypeList.length === 1) {
                $scope.shortageQtyData.VoucherTypeId = $scope.shortageVoucherTypeList[0].Value;
            }
        });
    }
    $scope.postShortageDebitNote = function () {
        if ($scope.shortageQtyData.VoucherTypeId == null)
            ShowResult('Please select VoucherType', 'failure', 'shortagePopUp');

        if ($scope.shortageQtyData != null && $scope.shortageQtyData.VoucherTypeId  != null) {
            $http({
                method: 'POST',
                url: $scope.shortageQtyPostUrl,
                data: {
                     "voucherVM": $scope.shortageQtyData
                    , "grnId": $scope.shortageQtyData.Id
                    , "voucherDetailVMList": $scope.shortageValueJournalList
                },
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getDataList();
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
            angular.element(document.querySelector('#additionalTaxPopUp')).modal('hide');
        }
    }
    $scope.ShortageDebitNotePrint = function () {
        try {
            var file_src = 'Accounts/AdjustmentNote/GetDebitNoteReport?reportFormat=' + 'Excel' + '&voucherId=' + $scope.shortageQtyData.DebitNoteVoucherId
            $rootScope.report(file_src);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.podiscountAmountCal = function (amount) {
        for (var i = 0; i < $scope.newList.length; i++) {
            if ($scope.newList[i].OtherName == 'Vendor') {

                var payableamount = 0;
                payableamount = $scope.TotalPayableAmount;
                $scope.newList[i].Cr = payableamount - amount;
                $scope.newList[i].Amount = payableamount - amount;
                var discountRow = $filter("filter")($scope.newList, { OtherName: "PurchaseDiscount" });
                if (!baseService.isUndefinedOrNull(discountRow) && discountRow.length > 0) {
                    for (var j = 0; j < $scope.newList.length; j++) {
                        if ($scope.newList[j].OtherName == 'PurchaseDiscount') {
                            $scope.newList[j].Cr = amount;
                            $scope.newList[j].Amount = amount;
                        }
                    }
                }
                else {
                    $scope.purchcaseDiscountList[0].Cr = amount;
                    $scope.purchcaseDiscountList[0].Amount = amount;
                    $scope.purchcaseDiscountList[0].TrnType = "Cr";
                    $scope.newList.push($scope.purchcaseDiscountList[0]);

                }
            }
        }

    }

    $scope.delete = function (gRNId, voucherId, invoiceId, type, tDSTaxVoucherId, tDSVoucherNo, deletedRemarks) {
        $http({
            method: "POST",
            url: 'accounts/Invoice/DeleteInventoryPayable',
            data: {
                "grnId": gRNId, "voucherId": voucherId, "invoiceId": invoiceId, "type": type, "tDSTaxVoucherId": tDSTaxVoucherId, "tDSVoucherNo": tDSVoucherNo, "deletedRemarks": deletedRemarks
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                $scope.deletedRemarks = "";
                $scope.closeconfirmDeletePopUp_Remarks();
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.Clear();
                $scope.GRNId = null;
                $scope.VoucherId = null;
                $scope.Type = null;
                $scope.TDSTaxVoucherId = null;
                $scope.TDSVoucherNo = null;
                $scope.InvoiceId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.deletedRemarks = "";
    $scope.onClickDeletePopUp = function (x) {
        var data = x;
        $scope.GRNId = data.Id;
        $scope.VoucherId = data.VoucherId;
        $scope.TDSTaxVoucherId = data.TDSTaxVoucherId;
        $scope.TDSVoucherNo = data.TDSVoucherNo;
        $scope.InvoiceId = data.InvoiceId;
        $scope.Type = data.GRNType;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector('#confirmDeletePopUp_Remarks')).modal('show');
    };

    $scope.closeconfirmDeletePopUp_Remarks = function () {
        angular.element(document.querySelector("#confirmDeletePopUp_Remarks")).modal("hide");
    };

    $scope.fiscalinvoiceAmountByParty = [];
    $scope.getFiscalInvoiceTotalAmountByParty = function (partyId, postingDate) {
        $scope.fiscalinvoiceAmountByParty = [];
        $http({
            method: "GET",
            url: "accounts/invoice/GetFiscalInvoiceTotalAmountByParty?partyId=" + partyId + '&postingDate=' + postingDate
        }).then(function successCallback(response) {
            $scope.fiscalinvoiceAmountByParty = response.data;
            $scope.TotalfiscalinvoiceAmountByParty = Math.round($filter("sumByKey")($filter("filter")($scope.fiscalinvoiceAmountByParty), "BooksInvoiceAmount") * 10000 + Number.EPSILON) / 10000;
        });
    };
    $scope.showFiscalInvoiceAmountByParty = function () {
        if ($scope.fiscalinvoiceAmountByParty.length > 0) {
            angular.element(document.querySelector("#partyfiscalInvoiceAmountPopUp")).modal("show");
        }
    }
    $scope.closeFiscalInvoiceTotalAmountByParty = function () {
        angular.element(document.querySelector('#partyfiscalInvoiceAmountPopUp')).modal('hide');

    }

    $scope.invoiceSetOffDetailList = [];
    $scope.getInvoiceSetOffDetailByInvoice = function (data) {
        $scope.invoiceSetOffDetailList = [];
        $http({
            method: "get",
            url: "accounts/invoice/getInvoiceSetOffDetailByInvoice?invoiceId=" + data.InvoiceId
        }).then(function successCallback(response) {
            $scope.invoiceSetOffDetailList = response.data;

            angular.element(document.querySelector('#invoiceetOffByInvoicePopUp')).modal('show');

        });
    };
    $scope.closeInvoiceSetOffDetailByInvoice = function () {
        angular.element(document.querySelector('#invoiceetOffByInvoicePopUp')).modal('hide');
    }

    $scope.postUrl = "Accounts/Invoice/PostVendorInvoice";
    $scope.post = function (invoiceId, type, tdsId, data) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "invoiceId": invoiceId,
                "type": type
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
    $scope.confirmPost = function (invoiceId, type, tdsId, data) {
        $scope.invoiceId = invoiceId;
        $scope.type = 'Vendor';
        $scope.tdsId = tdsId;
        $scope.data = data;

        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };


    $scope.searchglByList = [
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
        sort: "GLGeneralInfoName",
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.indexGL = "";
    $scope.popUpGL = function (index) {
        $scope.indexGL = index;
        baseService.setCurrentPage("cOAICodeList");
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetVendorInvoiceGLBudgetList", pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "GLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };
    $scope.setSelected = function (data, index) {
        $scope.newList[$scope.indexGL].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.newList[$scope.indexGL].GLGeneralInfoCode = data.GLGeneralInfoCode;
        $scope.newList[$scope.indexGL].GLGeneralInfoName = data.GLGeneralInfoName;
        $scope.newList[$scope.indexGL].BudgetMasterId = data.BudgetMasterId;
        $scope.newList[$scope.indexGL].BudgetName = data.BudgetName;
        $scope.newList[$scope.indexGL].ActivityId = data.ActivityId;
        $scope.newList[$scope.indexGL].ActivityName = data.ActivityName;
        $scope.closeCOAICodeListPopUp();
    };
}