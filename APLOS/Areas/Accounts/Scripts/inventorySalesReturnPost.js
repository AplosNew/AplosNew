'use strict';
inventorySalesReturnPost.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'factoryService', '$window'];
function inventorySalesReturnPost(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, factoryService, $window) {
    $rootScope.title = "Inventory Sales Return Post";
    $scope.Action = 'Save';
    $scope.Journal = 'Journal';
    $scope.index = -1;
    $scope.products = [];
    $scope.partyType = 'Customer';
    $scope.path = 'Accounts/InventorySale/';
    $scope.getListUrl = 'Products/InventoryIssue/GetPostingInvReceivableList/';
    $scope.siglesaveUrl = $scope.path + 'InventorySalesSingleJournalPosting';
    $scope.multiplesaveUrl = $scope.path + 'InventorySalesReturnMultipleJournalPosting';
    $scope.postUrl = 'Accounts/InvoicePost/InventorySalesReturnMultipleJournalPosting';
    $scope.AcceptanceId = null;
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

    $scope.searchByPostedSales = "Id"; $scope.searchSales = "";
    $scope.searchByPostedSaleList = [{ value: 'Id', name: "Sales No" }, { value: 'SalesDate', name: "GRN Date" }, { value: 'PartyName', name: "PartyName" }, { value: 'VoucherNo', name: "VoucherNo" }
        , { value: 'PostingDate', name: "PostingDate" }, { value: 'DocRefNo', name: "DocRef No" }
        , { value: 'DocDate', name: "Doc Date" }];

    $scope.products = [];
    $scope.getDataList = function () {
        $http({
            method: 'POST',
            url: 'Accounts/InventorySale/GetPostingInvReceivableList',
            data: { column: $scope.searchByPostedSales, value: $scope.searchSales },
        }).then(function successCallback(response) {
            $scope.products = response.data;
        });
    };
    $scope.getDataList();

    $scope.searchByPostedSales2 = "Id"; $scope.searchSales2 = "";
    $scope.searchByPostedSaleList2 = [{ value: 'Id', name: "Sales No" }, { value: 'SalesDate', name: "GRN Date" }, { value: 'PartyName', name: "PartyName" }, { value: 'VoucherNo', name: "VoucherNo" }
        , { value: 'PostingDate', name: "PostingDate" }, { value: 'DocRefNo', name: "DocRef No" }
        , { value: 'DocDate', name: "Doc Date" }];
    $scope.inventorySales = [];
    $scope.getInventorySalesDataList = function () {
        $http({
            method: 'POST',
            url: 'Accounts/InventorySale/GetPostingInventorySalesList',
            data: { column: $scope.searchByPostedSales2, value: $scope.searchSales2 },
        }).then(function successCallback(response) {
            $scope.inventorySales = response.data;
        });
    };
    $scope.getInventorySalesDataList();


  

  

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
        ,NoteForAccounts : null
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
        $scope.paymenttermUrl = "accounts/PaymentTerm/getcustomercbo";
        $http({
            method: "GET",
            url: $scope.paymenttermUrl
        }).then(function successCallback(response) {
            $scope.paymentTermList = response.data;
        });
    };
    $scope.paymentTerm();
    
    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        console.log($scope.companyConfig);
    });

  

    cboService.getCboVoucherTypeAccountReceivableList(function (result) {
        $scope.voucherTypeList = result;
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
    });

    //cboService.GetCboExpensesBookingTransactionType(function (result) {
    //    $scope.employeeTransactionTypeList = result;
    //});

    $scope.searchByPostedGRN = "Id"; $scope.searchGRN = "";
    $scope.searchByPostedGRNList = [{ value: 'Id', name: "Sales No" }, { value: 'SalesDate', name: "Sales Date" }
        , { value: 'Tracenent', name: "Tracenent" }
        , { value: 'PartyName', name: "Party" }
        , { value: 'GateEntryNo', name: "Gate EntryNo" }, { value: 'DocRefNo', name: "DocRef No" }
        , { value: 'DocDate', name: "Doc Date" }];


    $scope.approvedSalesList = [];
    $scope.getPopUpData = function () {
        $http({
            method: 'POST',
            url: 'Accounts/InventorySale/GetInventorySalesReturnForPost',
            data: { column: $scope.searchByPostedGRN, value: $scope.searchGRN },
        }).then(function successCallback(response) {
            $scope.approvedSalesList = response.data;
            for (var i = 0; i < $scope.approvedSalesList.length; i++) {
                response.data[i].SalesDate = new Date($scope.approvedSalesList[i].SalesDate);
            }
        });
    };
    $scope.popUp = function () {
        $scope.getPopUpData();
        angular.element(document.querySelector('#GRNpopUp')).modal('show');
    };
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
                    $scope.IsBaseOnDueDateEnable = false;
                } else if (paymentTerm.BaseLineDate === "postingdate") {
                    $scope.modelNew.BaseOnDueDate = $scope.modelNew.PostingDate;
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else {
                    $scope.IsBaseOnDueDateEnable = true;
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

    $scope.selectDoubleClick = function (data) {
        var voucherTypeId = $scope.modelNew.VoucherTypeId;
        $scope.modelNew = data.data;
        $scope.modelNew.VoucherTypeId = voucherTypeId;
        $scope.modelNew.EmployeeTransactionTypeId = null;
        $scope.TempEmployeeId = data.data.EmployeeId;
        $scope.modelNew.PostingDate = data.data.SalesDateNew;
        $scope.modelNew.DocDate = data.data.SalesDateNew;
        $scope.modelNew.DocRefNo = data.data.Id;
        $scope.modelNew.TaxApplicable = data.data.TaxApplicable;
        $scope.modelNew.IsPaymentTermChangeable = data.data.IsPaymentTermChangeable;
        $scope.modelNew.PaymentTermId = data.data.PaymentTermId;
        $scope.modelNew.SalesDateNewGetBudgetActivityInSalesMaterial = data.data.SalesDateNew;
        if (!baseService.isUndefinedOrNull(data.data.EmployeeId) && $scope.employeeTransactionTypeList.length === 1) {
            $scope.modelNew.EmployeeTransactionTypeId = $scope.employeeTransactionTypeList[0].EmployeeTransactionTypeId;
        }

        if ($scope.modelNew.IsPaymentTermChangeable) {
            $scope.changePaymentTerm($scope.modelNew.PaymentTermId)

        }
        else {
            if (!baseService.isUndefinedOrNull($scope.modelNew.PaymentTermId)) {
                var paymentTerm = $.grep($scope.paymentTermList, function (item) {
                    return item.Value === $scope.modelNew.PaymentTermId;
                })[0];
                if (paymentTerm.BaseLineDate === "documentdate") {
                    $scope.IsBaseOnDueDateEnable = false;
                }
            }
           
        }
        getRecievedList();
        getInventoryMaterialList(data.data.Id, data.data.EmployeeId, data.data.PartyId,$scope.modelNew.TaxApplicable);
        //getInventoryTaxList(data.data.Id);
        factoryService.getCurrencyPrecision(data.data.BaseCurrencyId);
        GetCurrencyExchangeRateList();
        $scope.closeGRNPopUp();
    };

    $scope.closeGRNPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#GRNpopUp')).modal('hide');
    };
    function getRecievedList() {
        $http.get('Accounts/InventorySale/GetInventorySalesReturnMaterial?inveReveiveId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.inventoryReceivedList = response.data.Rows;
                checkSameValueInColumnList($scope.inventoryReceivedList, 'TransactionUoM');
            });
    }


    function getVendorPayableGLBudgetActivity(inventorysalesId, customerId) {
       
        $http.get('Accounts/InventorySale/GetInventorySaleDetailGLList?inventorySalesId=' + inventorysalesId + '&customerId=' + customerId)
                .then(function (response) {
                    $scope.inventoryPayableList = [];
                    $scope.inventoryPayableList = response.data;
                });
    }
    $scope.inventoryJV = [];
    function getInventortGLBudgetActivity(inventorysalesId, customerId) {
        $http.get('Accounts/InventorySale/GetBudgetActivityInSalesReturnMaterial?inventorysalesId=' + inventorysalesId + '&customerId=' + customerId)
            .then(function (response) {
                $scope.inventoryJV = [];
                $scope.inventoryJV = response.data;
            });
    }


    $scope.inventorySalesJV = [];
    $scope.newList = [];
    function getInventoryMaterialList(inveReveiveId, employeeId, customerId,taxapplicable) {
        $scope.inventorySalesJV = [];
        if ($scope.companyConfig.IsInventorySalesBook) {
            $scope.Journal = 'Sales Journal';
            $scope.jvurl1 = 'Accounts/InventorySale/GetInventorySalesReturnInventorySalesBook?inventorysalesId=' + inveReveiveId + '&customerId=' + customerId + '&taxapplicable=' + taxapplicable
            getInventortGLBudgetActivity(inveReveiveId, customerId);
        }
        else {
            $scope.Journal = 'Journal';
            $scope.jvurl1 = 'Accounts/InventorySale/GetInventorySalesReturnMaterialReceivable?inveReveiveId=' + inveReveiveId + '&partyId=' + customerId + '&taxapplicable=' + taxapplicable
        }
        $http.get($scope.jvurl1)
            .then(function (response) {
                $scope.inventoryPayableList = [];
                $scope.inventoryReceiveDetailList = [];
                $scope.inventoryMaterialList = [];
                
                $scope.newList = response.data;
                for (var i = 0; i < $scope.newList.length; i++) {
                    if (($scope.newList[i].Dr + $scope.newList[i].Cr) > 0) {
                        $scope.inventorySalesJV.push($scope.newList[i]);
                    }
                }
                //getVendorPayableGLBudgetActivity(inveReveiveId, customerId);
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
            if ($scope.inventoryTaxList.length > 0 && $scope.modelNew.IsNonCreditable==false) {
                for (var i = 0; i < $scope.inventoryTaxList.length; i++) {
                    if ($scope.inventoryTaxList[i].ActivityId == null)
                        ShowResult('In Tax Category Determinate,  Tax  GL,Budget and Activity are missing !!', 'failure');
                }
            }
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
                        && row.ActivityId === newList[t].ActivityId ) {
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
                        && row.ActivityId === newList[a].ActivityId ) {
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
            else if (row.OtherName !== 'Svc' && row.OtherName === 'Vendor' && $scope.AcceptanceId === null)
                newList.push(list[i]);
            else if (row.OtherName !== 'Svc' && row.OtherName === 'Acceptance' && $scope.AcceptanceId !== null)
                newList.push(list[i]);
            //else newList.push(list[i]);
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
           
            else if (row.OtherName === 'Vendor' && $scope.AcceptanceId === null)
                newList.push(list[i]);
            else if (row.OtherName === 'Acceptance' && $scope.AcceptanceId !== null)
                newList.push(list[i]);

        }
    }

    function assignSvcInTax(row, taxList, trnType) {
        for (var i = 0; i < baseService.arrayLength(taxList); i++) {
            var row2 = taxList[i];
            if (row2.OtherName === 'Tax' && row2.TrnType === trnType && row2.GLGeneralInfoId === row.GLGeneralInfoId
                && row2.BudgetMasterId === row.BudgetMasterId && row2.ActivityId === row.ActivityId && row2.TaxCategoryId === row.TaxCategoryId) {
                    row2[trnType] += row.Amount ;
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
                url: 'currencies/ExchangeRate/ParallelExchangeRate?fromdate=' + $scope.modelNew.SalesDateNew + '&currencyId=' + $scope.modelNew.CurrencyId
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
    $scope.Post = function () {
        if ($scope.companyConfig.IsInventorySalesBook) {
            $scope.MultipleJournalPost();
        } else {

        $scope.SingleJournalPost();
        }
    }
    $scope.SingleJournalPost = function () {
        $scope.modelNew.Narration = $scope.modelNew.NoteForAccounts;
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
            $scope.newList[i].Amount = parseFloat($scope.newList[i].Amount).toFixed(4);
        }
        $http({
            method: 'POST',
            //url: $scope.siglesaveUrl,
            url: $scope.postUrl,
            data: {
                receiveId: $scope.modelNew.Id
                , acceptanceId: $scope.AcceptanceId
                , voucherVM: $scope.modelNew
                , voucherDetailVMList: $scope.inventorySalesJV/*$scope.inventoryMaterialList*/
                , voucherDetailCurrencyVMList: $scope.currencyExchangeRate
                , inventoryPayableVMList: $scope.inventoryPayableList
                , inventoryReceiveDetailVMList: $scope.inventoryPayableList
                , inventoryJVList: $scope.inventoryJV
                , IsInventorySalesBook: $scope.companyConfig.IsInventorySalesBook
                , otherInvoiceVM: $scope.otherVoucher
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDataList();
                $scope.Clear();
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.MultipleJournalPost = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.EmployeeId)) {
            var data = $filter('filter')($scope.newList, { OtherName: 'Customer' }, true);
            if (baseService.isUndefinedOrNull(data[0].GLGeneralInfoId)) return ShowResult('Employee GL not found', 'failure');
            if (baseService.isUndefinedOrNull(data[0].BudgetMasterId)) return ShowResult('Employee budget not found', 'failure');
            if (baseService.isUndefinedOrNull(data[0].ActivityId)) return ShowResult('Employee activity not found', 'failure');
            for (var i = 0; i < baseService.arrayLength($scope.inventoryMaterialList); i++) {
                if ($scope.inventoryMaterialList[i].OtherName === 'Customer') {
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
            $scope.newList[i].Amount = parseFloat($scope.newList[i].Amount).toFixed(4);
        }
        $http({
            method: 'POST',
           // url: $scope.multiplesaveUrl,
            url: $scope.postUrl,
            data: {
                receiveId: $scope.modelNew.Id
                , acceptanceId: $scope.AcceptanceId
                , voucherVM: $scope.modelNew
                , voucherDetailVMList: $scope.inventorySalesJV/*$scope.inventoryMaterialList*/
                , voucherDetailCurrencyVMList: $scope.currencyExchangeRate
                , inventoryPayableVMList: $scope.inventoryPayableList
                , inventoryReceiveDetailVMList: $scope.inventoryPayableList
                , inventoryJVList: $scope.inventoryJV
                , IsInventorySalesBook: $scope.companyConfig.IsInventorySalesBook
                , otherInvoiceVM: $scope.otherVoucher
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDataList();
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
        $scope.otherVoucher = {};
        $scope.newList = [];
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
    };

   

    $scope.sumORnot = false;
    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }

    $scope.getPabyableJournal = function (data, reportFormat) {
        $window.open($scope.path + 'PabyableJournal?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&employeeId=' + data.EmployeeId + '&isReversCharge=' + data.IsTaxApplicable, '_blank');
    };



    $scope.onClickReportDownloadWord = function (data) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/InventoryIssue/ReportSalesPosting?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');

    };

    $scope.onClickReportDownloadExcel = function (data) {
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/InventoryIssue/ReportSalesPosting?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId , '_blank');

    };

    $scope.onClickGRNID = function (args) {
        debugger;

        var gridObj = $("#GridPrint").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "InventoryIssue/ReportInventorySalesPosting?reportFormat=" + 'Pdf' +'&voucherId='+data.VoucherId;

    };
    $scope.commandGRN = [{

        type: "details", buttonOptions: {
            text: "GRN",
            width: "50",
            height: "20",

            click: $scope.onClickGRNID
        }
    }];

// Sales Button
    $scope.downloadSalesReport = function (data) {
        location.href = "Products/InventoryIssue/inventorySalesReportPrint?grnId=" + data.Id;

    };

    $scope.downloadInventorySales = function (data) {
        location.href = "Products/InventoryIssue/inventorySalesReportPrint?grnId=" + data.Id;

    };


    $scope.downloadInventorySalePDF = function (data) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/InventoryIssue/ReportInventorySalesPosting?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');
    };


    $scope.downloadInventorySaleExcel = function (data) {
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/InventoryIssue/ReportInventorySalesPosting?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');

    };
   
    $scope.tab = 1;
    $scope.setTabSalesList = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSetSalesList = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 1;

    };

    $scope.otherVoucher = {
        Id: null,
        CompanyGroupId: null,
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
        IsGovtSubsidy: false
    };
    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
    $scope.partyList = [];
    $scope.otherPartyPlantList = [];
    $scope.getOtherPartyPlantList = function (partyId) {
        $scope.otherPartyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.otherPartyPlantList.push(item);
                    if (item.IsDefault) {
                        $scope.otherPartyPlantId = item.Value;
                        $scope.otherVoucher.PartyPlantId = item.Value;
                        $scope.otherVoucher.DeliveryPartyPlantId = item.Value;
                        $scope.billToAddress = item.Address1;
                        $scope.shipToAddress = item.Address1;
                    }
                });
            });
    };
    $scope.showPartyOtherPopUpNew = function () {
        if ($scope.OrderSpecific === 'Yes') {
            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {

            }
            $http({
                method: 'POST',
                url: 'Parties/party/GetCompanyPartyDataListByContract?ContractId=' + $scope.productNew.ContractId + '&partyType=' + $scope.partyType,
                data: { column: $scope.searchByParty, value: $scope.searchParty },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.partyList = response.data;
                if ($scope.partyList.length === 0) {
                    if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
                    }
                    else if ($scope.partyType === 'Party') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    else if ($scope.partyType === 'Director') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    else if ($scope.partyType === 'Other') {
                        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
                    }
                    $http({
                        method: 'POST',
                        url: $scope.partyUrl,
                        data: { column: $scope.searchByParty, value: $scope.searchParty },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        $scope.partyList = response.data;
                    });
                }
            });

        }
        else {

            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
            }
            else if ($scope.partyType === 'Party') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            else if ($scope.partyType === 'Director') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            else if ($scope.partyType === 'Other') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
            }
            $http({
                method: 'POST',
                url: $scope.partyUrl,
                data: { column: $scope.searchByParty, value: $scope.searchParty },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.partyList = response.data;
            });
        }
        angular.element(document.querySelector('#partyOtherPopUp')).modal('show');
    };

    $scope.closeOtherPartyPopUp = function (x) {
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
            $scope.otherVoucher.PartyId = party.Id;
            $scope.otherVoucher.PartyCode = party.Code;
            $scope.otherVoucher.PartyName = party.UserName;
            $scope.otherVoucher.PartyType = $scope.partyType;
            $scope.otherVoucher.GLGeneralInfoId = party.ReconciliationGLId;
            $scope.otherVoucher.GLGeneralInfoCode = party.ReconciliationGLCode;
            $scope.otherVoucher.GLGeneralInfoName = party.ReconciliationGLName;
            $scope.otherVoucher.CurrencyId = party.CurrencyId;
            $scope.otherVoucher.BudgetMasterId = party.ReconciliationBudgetId;
            $scope.otherVoucher.BudgetCode = party.ReconciliationBudgetCode;
            $scope.otherVoucher.BudgetName = party.ReconciliationBudgetName;
            $scope.otherVoucher.ActivityId = party.ReconciliationActivityId;
            $scope.otherVoucher.ActivityCode = party.ReconciliationActivityCode;
            $scope.otherVoucher.ActivityName = party.ReconciliationActivityName;
            $scope.getOtherPartyPlantList($scope.otherVoucher.PartyId);
        }
        $scope.hideOtherPartyPopUp();
    };
    $scope.hideOtherPartyPopUp = function () {
        angular.element(document.querySelector('#partyOtherPopUp')).modal('hide');

    }
    $scope.clearOtherPartyData = function () {
        $scope.otherVoucher.PartyId = null;
        $scope.otherVoucher.PartyName = null;
        $scope.otherVoucher.PartyPlantId = null;
        $scope.otherPartyPlantList = [];
    }

    
    $scope.closeJournalPopUp = function () {
        angular.element(document.querySelector('#JournalPopUp')).modal('hide');
    }

    $scope.otherInvoicepost = function (id, data, otherInvoiceJVlist) {
        $http({
            method: "POST",
            url: 'Accounts/Invoice/InsertOtherInvoiceJournal',
            data: {
                "otherInvoiceId": id,
                "voucherVM": data,
                "voucherDetailVMList": otherInvoiceJVlist
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                if (tdsId != null) {
                }
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.invoiceId = null;
    $scope.confirmotherInvoicePost = function (id, data, list) {
        $scope.otherinvoiceId = id;
        $scope.data = data;
        $scope.otherInvoiceJVlist = list;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmotherInvoicePostPopUp")).modal("show");
    };

    $scope.OtherInvoiceVouchereReport = function () {
        $window.open('Accounts/Invoice/CustomerInvoiceReceiptGovtSubsidyReport?reportFormat=' + 'Excel' + '&voucherId=' + $scope.otherVoucher.OtherInvoiceVoucherId, '_blank');
    }

    $scope.voucherTypeListnew = [];
    $scope.otherInvoiceVoucherTypeId = null;
    $scope.getReceivableFromOthersVoucherType = function () {
        cboService.getCboVoucherTypeReceivableFromOthersList(function (result) {
            $scope.voucherTypeListnew = result;
            if (baseService.arrayLength($scope.voucherTypeListnew) === 1)
                $scope.otherVoucher.VoucherTypeId = $scope.voucherTypeListnew[0].Value;
        });
    }

    $scope.additionalTaxPostUrl = 'Accounts/InvoicePost/InsertAdditionalTaxPayable';
    $scope.otherInvoiceDetailList = [];
    $scope.onClickadditionalTaxPop = function (x) {
        var data = x;
        data.VoucherTypeId = null;
        data.VoucherDate = new Date();
        $scope.otherVoucher = data;
        $scope.otherVoucher.DocRefNo = data.Id;
        $scope.OtherInvoiceJournalId = data.OtherInvoiceId;
        $http({
            method: 'GET',
            url: 'Accounts/Invoice/GetOtherInvoiceJournal?otherInvoieId=' + data.OtherInvoiceId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.otherInvoiceDetailList = response.data;
        });
        $scope.getReceivableFromOthersVoucherType();
        angular.element(document.querySelector('#JournalPopUp')).modal('show');
    };
   
    $scope.additionalTaxPop = [{
        type: "details", buttonOptions: {
            text: "TDS Post",
            width: "80",
            height: "20",
            click: $scope.onClickadditionalTaxPop
        }
    }];


    $scope.onClickDeletePopUp = function (x) {
        var data = x;
        $scope.SalesId = data.Id;
        $scope.VoucherId = data.VoucherId;
        $scope.InventoryVoucherId = data.InventoryVoucherId;

        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };



    $scope.delete = function (salesId, voucherId, inventoryVoucherId) {
        $http({
            method: "POST",
            url: 'Accounts/Invoice/DeleteInventorySales',
            data: {
                "salesId": salesId, "voucherId": voucherId, "InventoryVoucherId": inventoryVoucherId
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
                $scope.SalesId = null;
                $scope.VoucherId = null;
                $scope.InventoryVoucherId = null;
               
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };



}