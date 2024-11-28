'use strict';
servicePayableController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'factoryService', '$window'];
function servicePayableController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, factoryService, $window) {
    $rootScope.title = "Service Payable";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Accounts/InventoryPayable/';
    /*$scope.getListUrl = 'Accounts/InventoryPayable/GetServicePostingList/';*/
    $scope.saveUrl = 'Accounts/InvoicePost/ServicePost';
    $scope.AcceptanceId = null;

    $scope.products = [];
    $scope.searchByPostedService = "Id"; $scope.searchService = "";
    $scope.searchByPostedServiceList = [{ value: 'Id', name: "Acknowledge No" }, { value: 'GRNDate', name: "Acknowledge Date" }, { value: 'Particular', name: "Particular" }, { value: 'VoucherNo', name: "VoucherNo" }
        , { value: 'PostingDate', name: "PostingDate" }, { value: 'DocRefNo', name: "DocRef No" }, { value: 'TDSVoucherNo', name: "TDS VoucherNo" }
        , { value: 'DocDate', name: "Doc Date" }];

    $scope.getDataList = function () {
        $http({
            method: 'POST',
            url: 'Accounts/InventoryPayable/GetServicePostingList',
            data: { column: $scope.searchByPostedService, value: $scope.searchService },
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

        , PartyId: null
        , PartyPlantId: null
        , PartyName: null
        , PaymentTermId: null
        , PaymentTermName: null
        , PostingDate: new Date()
        , VoucherTypeId: null
        , ToCurrencyRate: null
        , Narration: null
        , PaymentTermCode: null
        , AcknolwdgementDate: null
        , EntityId: null
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

    cboService.getCboVoucherTypeAccountPayableList(function (result) {
        $scope.voucherTypeList = result;
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
    });

    cboService.GetCboExpensesBookingTransactionType(function (result) {
        $scope.employeeTransactionTypeList = result;
    });

    $scope.approvedGRNList = [];
    $scope.getPopUpData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/InventoryPayable/GetListForSvcPayable',
        }).then(function successCallback(response) {
            $scope.approvedGRNList = response.data;
            for (var i = 0; i < $scope.approvedGRNList.length; i++) {
                response.data[i].DocDate = new Date($scope.approvedGRNList[i].DocDate);
            }
        });
    };
    $scope.popUp = function () {
        $scope.getPopUpData();
        angular.element(document.querySelector('#GRNpopUp')).modal('show');
    };


    $scope.selectDoubleClick = function (data) {
        var voucherTypeId = $scope.modelNew.VoucherTypeId;
        $scope.modelNew = data.data;
        $scope.modelNew.VoucherTypeId = voucherTypeId;
        $scope.modelNew.EmployeeTransactionTypeId = null;
        $scope.modelNew.PostingDate = data.data.DocDate;
        $scope.modelNew.AcknolwdgementDate = data.data.AcknolwdgementDate;
        $scope.modelNew.IsInvoice = true;
        if (!baseService.isUndefinedOrNull(data.data.EmployeeId) && $scope.employeeTransactionTypeList.length === 1) {
            $scope.modelNew.EmployeeTransactionTypeId = $scope.employeeTransactionTypeList[0].EmployeeTransactionTypeId;
        }
        $scope.paymentTerm();
        getRecievedList();

        getInventoryMaterialList(data.data.Id, data.data.EmployeeId, data.data.IsTaxApplicable);
        factoryService.getCurrencyPrecision(data.data.BaseCurrencyId);
        GetCurrencyExchangeRateList();
        $scope.TDSList = [];
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
    function getInventoryMaterialList(inveReveiveId, employeeId, isReversCharge) {
        $http.get('Accounts/InventoryPayable/GetServicePayable?serviceAcknowledgementMasterId=' + inveReveiveId)
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
                //if (!baseService.isUndefinedOrNull(employeeId))
                //    $scope.glPushInList();
                //if (baseService.isUndefinedOrNull(employeeId))

                getServiceDetailGL(inveReveiveId);
            });
    }
    function getServiceDetailGL(inveReveiveId) {
        $http.get('Accounts/InventoryPayable/GetServiceDetailGL?serviceAcknowledgementMasterId=' + inveReveiveId)
            .then(function (response) {
                $scope.serviceDetailGLList = [];
                $scope.serviceDetailGLList = response.data;
            });
    }
    $scope.serviceTaxList = [];
    function getInventoryTaxList(inveReveiveId) {
        $scope.serviceTaxList = [];
        $http.get('Accounts/InventoryPayable/GetServiceAdditionalTax?serviceAcknowledgementMasterId=' + inveReveiveId)
            .then(function (response) {
                $scope.serviceTaxList = response.data.Rows;
                distributeTCSAmount();
            });

    }

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
        var svcList = ($filter('filter')(list, { OtherName: 'Svc' }, true));
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
            else if (row.OtherName === 'Tax' && row.TrnType === 'Cr' && row.Dr > 0) {
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
            else if (row.OtherName === 'Svc' && row.TrnType === 'Dr') {
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
            else if (row.OtherName === 'Charges' && row.TrnType === 'Dr') {
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
            else if (row.OtherName !== 'Svc' && row.OtherName === 'Vendor' && $scope.AcceptanceId === null)
                newList.push(list[i]);
            //else if (row.OtherName !== 'Svc' && row.OtherName === 'GIRI' && $scope.AcceptanceId !== null)
            //    newList.push(list[i]);
            //else newList.push(list[i]);
        }
        getInventoryTaxList($scope.modelNew.Id);
    }
    function distributeTCSAmount() {
        var vendorList = ($filter('filter')($scope.newList, { OtherName: 'Vendor' }, true));
        if (vendorList.length > 1) {
            for (var z = 0; z < vendorList.length; z++) {
                if (z > 0) {
                    var totaltcsAmount = Math.round($filter("sumByKey")($filter("filter")($scope.serviceTaxList), "TaxAmount") * 100 + Number.EPSILON) / 100;
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

    function distributeTCSAmountGIRI() {
        var vendorList = ($filter('filter')($scope.newList, { OtherName: 'GIRI' }, true));
        if (vendorList.length > 1) {
            for (var z = 0; z < vendorList.length; z++) {
                if (z > 0) {
                    var totaltcsAmount = Math.round($filter("sumByKey")($filter("filter")($scope.serviceTaxList), "TaxAmount") * 100 + Number.EPSILON) / 100;
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

    $scope.controlInvoice = function () {
        if ($scope.modelNew.IsInvoice == true) {
            $scope.IsInvoiceDisable = false;
            $scope.modelNew.AddtionalTax = true;
            $scope.applyGIRI();
            // 
        }
        else if ($scope.modelNew.IsInvoice == false) {
            $scope.IsInvoiceDisable = false;
            $scope.modelNew.AddtionalTax = false;
            $scope.applyGIRI();
            // distributeTCSAmount();
        }
    }


    $scope.applyGIRI = function () {
        if ($scope.modelNew.IsInvoice == false) {
            var j = $scope.newList.length;
            while (j--) {
                if ($scope.newList[j]["OtherName"] === 'Vendor') {
                    $scope.newList.splice(j, 1);
                }
            }
            var newRow = ($filter('filter')($scope.inventoryMaterialList, { OtherName: 'GIRI' }));
            for (var i = 0; i < newRow.length; i++) {
                $scope.newList.push(newRow[i]);
            }
            distributeTCSAmountGIRI();
        }
        if ($scope.modelNew.IsInvoice == true) {
            var k = $scope.newList.length;
            while (k--) {
                if ($scope.newList[k]["OtherName"] === 'GIRI') {
                    $scope.newList.splice(k, 1);
                }
            }
            var newRow1 = ($filter('filter')($scope.inventoryMaterialList, { OtherName: 'Vendor' }));
            for (var h = 0; h < newRow1.length; h++) {
                $scope.newList.push(newRow1[h]);

            }
            //distributeTCSAmount();
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
        for (var i = 0; i < baseService.arrayLength(taxList); i++) {
            var row2 = taxList[i];
            if (row2.OtherName === 'Tax' && row2.TrnType === trnType && row2.GLGeneralInfoId === row.GLGeneralInfoId
                && row2.BudgetMasterId === row.BudgetMasterId && row2.ActivityId === row.ActivityId && row2.TaxCategoryId === row.TaxCategoryId) {
                row2[trnType] += row.Amount;
                row2.Amount += row.Amount;
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
                url: 'currencies/ExchangeRate/ParallelExchangeRate?fromdate=' + $scope.modelNew.AcknolwdgementDate + '&currencyId=' + $scope.modelNew.CurrencyId
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

    $scope.NewgetDataList = function (grnId) {
        $scope.searchService = grnId;
        $http({
            method: 'POST',
            url: 'Accounts/InventoryPayable/GetServicePostingList',
            data: { column: $scope.searchByPostedService, value: $scope.searchService },
        }).then(function successCallback(response) {
            $scope.products = response.data;
            var rowdata = $filter("filter")($scope.products, { "Id": grnId });
            if (!baseService.isUndefinedOrNull(rowdata[0].AdditionalTaxId)) {
                $scope.onClickadditionalTaxPop(rowdata[0]);
            }
        });
    };

    
    $scope.Post = function () {
        if (baseService.isUndefinedOrNull($scope.modelNew.EntityId)) return ShowResult('Please Select Entity', 'failure');

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

        }
        for (var i = 0; i < $scope.newList.length; i++) {
            $scope.newList[i].Amount = parseFloat($scope.newList[i].Amount).toFixed(2);
        }
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                receiveId: $scope.modelNew.Id
                , acceptanceId: $scope.AcceptanceId
                , voucherVM: $scope.modelNew
                , voucherDetailVMList: $scope.newList/*$scope.inventoryMaterialList*/
                , voucherDetailCurrencyVMList: $scope.currencyExchangeRate
                , serviceDetailGLList: $scope.serviceDetailGLList
                , inventoryReceiveDetailVMList: $scope.inventoryReceiveDetailList
                , tdsTaxList: $scope.TDSList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.NewgetDataList($scope.modelNew.Id);
                $scope.Clear();
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
        $scope.newList = [];
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
    };

    function getRecievedList() {
        $http.get('Accounts/InventoryPayable/GetServiceData?serviceAcknowledgementMasterId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.inventoryReceivedList = response.data.Rows;
                checkSameValueInColumnList($scope.inventoryReceivedList, 'TransactionUoM');
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
        $window.open($scope.path + 'ServicePabyableJournal?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&voucherId=' + data.VoucherId + '&isReversCharge=' + data.IsTaxApplicable, '_blank');
    };



    $scope.onClickReportDownloadWord = function (args) {
        debugger;
        var gridObj = $("#GridPrint").data("ejGrid");
        //getting corresponding record 
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open($scope.path + 'ServicePabyableJournal?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&voucherId=' + data.VoucherId + '&isReversCharge=' + data.IsTaxApplicable, '_blank');

    };

    $scope.commandPDF = [{
        type: "details", buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",
            //contentType: "imageonly",
            //prefixIcon: "e-icon e-dataexport",

            //prefixIcon: "e-icon e-edit" ,
            //prefixIcon: "e-icon e-delete",
            //prefixIcon: " e-icon e-save",
            //prefixIcon: " e-icon e-cancel",

            click: $scope.onClickReportDownloadWord
        }
    }];

    $scope.onClickReportDownloadExcel = function (args) {
        debugger;
        var gridObj = $("#GridPrint").data("ejGrid");
        //getting corresponding record 
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open($scope.path + 'ServicePabyableJournal?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&voucherId=' + data.VoucherId + '&isReversCharge=' + data.IsTaxApplicable, '_blank');

    };
    $scope.commandExcel = [{
        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            //contentType: "imageonly",
            //prefixIcon: "e-icon e-dataexport",

            //prefixIcon: "e-icon e-edit" ,
            //prefixIcon: "e-icon e-delete",
            //prefixIcon: " e-icon e-save",
            //prefixIcon: " e-icon e-cancel",

            click: $scope.onClickReportDownloadExcel
        }
    }];


    $scope.onClickGRNID = function (args) {
        debugger;

        var gridObj = $("#GridPrint").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };
    $scope.commandGRN = [{

        type: "details", buttonOptions: {
            text: "GRN",
            width: "50",
            height: "20",

            click: $scope.onClickGRNID
        }
    }];


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
        Type: null,
        TaxCategoryId: null
    };
    $scope.selectTDS = function () {
        $scope.TDS.ValueOfFixed = $.grep($scope.TDSCboList, function (item) {
            return item.Id === $scope.TDS.TaxCodeId;
        })[0].ValueOfFixed;
        $scope.TDS.TaxCategoryId = $.grep($scope.TDSCboList, function (item) {
            return item.Id === $scope.TDS.TaxCodeId;
        })[0].TaxCategoryId;
        $scope.TDS.Type = $.grep($scope.TDSCboList, function (item) {
            return item.Id === $scope.TDS.TaxCodeId;
        })[0].Type;
        if ($scope.TDS.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.TDS.ValueOfFixed)) {
            $scope.TDS.TaxAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryReceivedList), "TrnAmount") * $scope.TDS.ValueOfFixed / 100).toFixed(4);
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

    $scope.voucherTypeListnew = [];
    $scope.additionalTaxVoucherTypeId = null;
    $scope.getPaymentVoucherType = function () {
        cboService.getCboVoucherTypePaymentList(function (result) {
            $scope.voucherTypeListnew = result;
            if (baseService.arrayLength($scope.voucherTypeListnew) === 1)
                $scope.additionalTaxVoucherTypeId = $scope.voucherTypeListnew[0].Value;
        });
    }
    $scope.getPaymentVoucherType();
    $scope.additionalTaxUrl = 'Accounts/InvoicePost/InsertAdditionalTaxPayable';
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

        angular.element(document.querySelector('#additionalTaxPopUp')).modal('show');
    };
    $scope.postAdditionalTax = function () {
        if ($scope.additionalTaxVoucherTypeId == null)
            ShowResult('Please select VoucherType', 'failure', 'additionalTaxPopUp');

        $scope.additionalTaxData.VoucherTypeId = $scope.additionalTaxVoucherTypeId;
        if ($scope.additionalTaxData != null && $scope.additionalTaxVoucherTypeId != null) {
            $http({
                method: 'POST',
                url: $scope.additionalTaxUrl,
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
    $scope.additionalTaxPop = [{
        type: "details", buttonOptions: {
            text: "TDS Post",
            width: "80",
            height: "20",
            click: $scope.onClickadditionalTaxPop
        }
    }];

    $scope.additionalTaxPrint = function () {
        try {
            var file_src = 'Accounts/invoice/VendorInvoicePaymentReport?reportFormat=' + 'Excel' + '&voucherId=' + $scope.additionalTaxData.TDSTaxVoucherId
            $rootScope.report(file_src);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.onClickDeletePopUp = function (x) {
        var data = x;
        $scope.ServiceAckId = data.Id;
        $scope.VoucherId = data.VoucherId;
        $scope.TDSTaxVoucherId = data.TDSTaxVoucherId;
        $scope.TDSVoucherNo = data.TDSVoucherNo;
        $scope.InvoiceId = data.InvoiceId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };

    $scope.delete = function (serviceAckId, voucherId, invoiceId, tDSTaxVoucherId, tDSVoucherNo) {
        $http({
            method: "POST",
            url: 'accounts/Invoice/DeleteServicePayable',
            data: {
                "serviceAckId": serviceAckId, "voucherId": voucherId, "invoiceId": invoiceId,  "tDSTaxVoucherId": tDSTaxVoucherId, "tDSVoucherNo": tDSVoucherNo
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
                $scope.ServiceAckId = null;
                $scope.VoucherId = null;
                $scope.TDSTaxVoucherId = null;
                $scope.TDSVoucherNo = null;
                $scope.InvoiceId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.onClickTDSDeletePopUp = function (x) {
        var data = x;
        $scope.AdditionalTaxId = data.AdditionalTaxId;
        $scope.TDSTaxVoucherId = data.TDSTaxVoucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector('#confirmTDSDeletePopUp')).modal('show');
    };
    $scope.deleteAdditionalTax = function (additionalTaxId, tDSTaxVoucherId) {
        $http({
            method: "POST",
            url: 'accounts/InvoicePost/DeleteTDSServicePayable',
            data: {
                "additionalTaxId": additionalTaxId, "voucherId": tDSTaxVoucherId
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
                $scope.AdditionalTaxId = null;
                $scope.VoucherId = null;
                $scope.TDSTaxVoucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    }

    $scope.onClickTDSPostDeletePopUp = function (x) {
        var data = x;
        $scope.ServiceAckId = data.Id;
        $scope.TDSTaxVoucherId = data.TDSTaxVoucherId;
        $scope.InvoiceWriteOffId = data.InvoiceWriteOffId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector('#confirmTDSPostDeletePopUp')).modal('show');
    };
    $scope.deletePostAdditionalTax = function (voucherId, serviceAckId, invoiceWriteOffId) {
        $http({
            method: "POST",
            url: 'accounts/InvoicePost/DeleteTDSPostServicePayable',
            data: {
                "voucherId": voucherId, "serviceAckId": serviceAckId, "invoiceWriteOffId": invoiceWriteOffId
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
                $scope.ServiceAckId = null;
                $scope.VoucherId = null;
                $scope.TDSTaxVoucherId = null;
                $scope.TDSVoucherNo = null;
                $scope.InvoiceId = null;
                $scope.InvoiceWriteOffId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
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

    $scope.searchByService = "Activity"; $scope.searchService = "";
    $scope.searchByServiceList = [{ value: 'ServiceName', name: "Service" }, { value: 'ServiceType', name: "Service Type" }, { value: 'ServiceGroup', name: "Service Group" }, { value: 'GLCode', name: "GL Code" }
        , { value: 'GL', name: "GL" }, { value: 'Budget', name: "Budget" }, { value: 'Activity', name: "Activity" }];

    $scope.serviceLists = [];

    $scope.indexGL = "";
    $scope.getServiceDataList = function (index, data) {
        $scope.indexGL = index;
        $scope.serviceLists = [];
        $http({
            method: 'POST',
            url: 'SetUps/ServiceMaster/GetServicePopUpListByServiceMasterId',
            data: { column: $scope.searchByService, value: $scope.searchService, serviceMasterId: data.ServiceMasterId },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.serviceLists = response.data;
        });
        angular.element(document.querySelector('#ServicePopUp')).modal('show');
    };
    $scope.closeServiceDataPopUp = function () {
        angular.element(document.querySelector("#ServicePopUp")).modal("hide");
    };

    $scope.ServiceGLSelect = function (obj) {
        $scope.newList[$scope.indexGL].GLGeneralInfoId = obj.data.GLGeneralInfoId;
        $scope.newList[$scope.indexGL].GLGeneralInfoCode = obj.data.GLGeneralInfoCode;
        $scope.newList[$scope.indexGL].GLGeneralInfoName = obj.data.GLGeneralInfoName;
        $scope.newList[$scope.indexGL].BudgetMasterId = obj.data.BudgetMasterId;
        $scope.newList[$scope.indexGL].BudgetName = obj.data.BudgetName;
        $scope.newList[$scope.indexGL].ActivityId = obj.data.ActivityId;
        $scope.newList[$scope.indexGL].ActivityName = obj.data.ActivityName;
        $scope.closeServiceDataPopUp();
    };

}