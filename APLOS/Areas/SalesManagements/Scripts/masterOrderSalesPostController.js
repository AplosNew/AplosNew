"use strict";
masterOrderSalesPostController.$inject = ["cboService", "commonMessage", '$window', "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "accountService"];
function masterOrderSalesPostController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $http, $filter, $controller, accountService) {
    $rootScope.title = "Master Order Sales Posting";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherDetailList = [];
    $scope.voucherList = [];
    $scope.partyType = "Customer";
    $scope.isAdvance = false;
    $scope.salesMaterialList = [];
    $scope.masterOrderDetailList = [];
    $scope.masterOrderServiceDetailList = [];
    $scope.postUrl = 'SalesManagements/Sales/PostMasterOrderSales';
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("baseMaterialAndArticleController", { $scope: $scope, $http: $http });




    $scope.getMasterOrderSalesList = [];
    $scope.getMasterOrderSales = function () {
        $http({
            method: 'GET'
            , url: 'SalesManagements/Sales/GetMasterOrderSalesList'
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.getMasterOrderSalesList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.searchByPostedSales = "InvoiceNo"; $scope.searchSales = "";
    $scope.searchByPostedSalesList = [{ value: 'InvoiceNo', name: "Invoice No" }, { value: 'VoucherNo', name: "Voucher No" }, { value: 'PartyCode', name: "Party Code" }, { value: 'PartyName', name: "Party Name" }
        , { value: 'DocRefNo', name: "DocRef No" }
    ];

    $scope.getMasterOrderSalesPostedList = [];
    $scope.getMasterOrderSalesPosted = function () {
        $http({
            method: 'POST'
            , url: 'SalesManagements/Sales/GetMasterOrderSalesPostedList'
            , data: { column: $scope.searchByPostedSales, value: $scope.searchSales }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.getMasterOrderSalesPostedList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.getMasterOrderSalesPosted();

    //#region  GetSalesWordReport
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
        , AddtionalTax: null
        , IsInvoice: false
        , EntityId: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.paymentTerm = function () {

        $scope.paymenttermUrl = "accounts/PaymentTerm/getcustomercbo";
        $http({
            method: "GET",
            url: $scope.paymenttermUrl
        }).then(function successCallback(response) {
            $scope.paymentTermList = response.data;
        });
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

    $scope.SalesReport = function (data) {
        location.href = "Sales/SalesReportService?grnId=" + data.Id;
    };
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Salesdb = {};
    $scope.confirmPost = function (data) {
        $scope.salesdb = data;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };
    $scope.clear = function () {
        $scope.modelNew = {};
        $scope.newList = [];
        $scope.masterOrderDetailList = [];
        $scope.masterOrderServiceDetailList = [];
        $scope.getCboVoucherTypeAccountReceivableList();
    }
    $scope.Post = function () {
        if (baseService.isUndefinedOrNull($scope.modelNew.BaseOnDueDate)) {
            ShowResult('Please select Due Date BaseOn!', 'failure');
            return true;
        }
        if ($scope.modelNew.SourceType !== 'Packing') {
            $http({
                method: "POST",
                url: $scope.postUrl,
                data: {
                    "sales": $scope.modelNew,
                    "salesDetailVMList": $scope.newList,
                    "salesMaterialDetailGLList": $scope.masterOrderDetailList,
                    "salesServiceDetailGLList": $scope.masterOrderServiceDetailList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getMasterOrderSalesPosted();
                    $scope.clear();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            $http({
                method: "POST", 
                url: 'SalesManagements/Sales/PostSalesPacking',
                data: {
                    "sales": $scope.modelNew,
                    "salesDetailVMList": $scope.newList,
                    "salesMaterialDetailGLList": $scope.masterOrderDetailList,
                    "salesServiceDetailGLList": $scope.masterOrderServiceDetailList,
                     "packing": $scope.modelPacking,
                    "PackingDetailVMList": $scope.packingJournaldataList,
                    "packingVoucherTypeId": $scope.modelPacking.VoucherTypeId
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getMasterOrderSalesPosted();
                    $scope.clear();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
       
        return true;
    };

    $scope.onClickPost = function (args) {

        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.SalesId)) return ShowResult('No Id found', 'failure');
        $scope.confirmPost(data)
    };

    $scope.commandPost = [{
        type: "details", buttonOptions: {
            text: "Post",
            width: "50",
            height: "20",
            click: $scope.onClickPost
        }
    }];

    $scope.onClickReportDownloadWord = function (args) {

        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.SalesId)) return ShowResult('No Id found', 'failure');
        $window.open('SalesManagements/Sales/SalesReport?reportFormat=' + reportFormat + '&&salesId=' + data.SalesId, '_blank');
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

        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.SalesId)) return ShowResult('No Id found', 'failure');
        $window.open('SalesManagements/Sales/SalesReport?reportFormat=' + reportFormat + '&&salesId=' + data.SalesId, '_blank');
    };
    $scope.commandExcel = [{
        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            click: $scope.onClickReportDownloadExcel
        }
    }];


    $scope.onClickReportDownloadExcel = function (args) {

        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.SalesId)) return ShowResult('No Id found', 'failure');
        $window.open('SalesManagements/Sales/SalesReport?reportFormat=' + reportFormat + '&&salesId=' + data.SalesId, '_blank');
    };
    $scope.commandExcel = [{
        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            click: $scope.onClickReportDownloadExcel
        }
    }];


    $scope.onClickReportMOS = function (data) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.SalesId)) return ShowResult('No Id found', 'failure');
        $window.open('SalesManagements/Sales/SalesReport?reportFormat=' + reportFormat + '&&salesId=' + data.SalesId, '_blank');
    };


    $scope.onClickReportExcelMOS = function (data) {
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.SalesId)) return ShowResult('No Id found', 'failure');
        $window.open('SalesManagements/Sales/SalesReport?reportFormat=' + reportFormat + '&&salesId=' + data.SalesId, '_blank');
    };
   

    $scope.onClickReportExcelPosting = function (data) {
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.VoucherId)) return ShowResult('No Id found', 'failure');
        $window.open('SalesManagements/Sales/SalesReceivableReport?reportFormat=' + reportFormat + '&&voucherId=' + data.VoucherId, '_blank');
    };
    $scope.onClickReportPDFPosting = function (data) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.VoucherId)) return ShowResult('No Id found', 'failure');
        $window.open('SalesManagements/Sales/SalesReceivableReport?reportFormat=' + reportFormat + '&&voucherId=' + data.VoucherId, '_blank');
    };
   

    $scope.onClickReportPDFPackingPosting = function (data) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.VoucherId)) return ShowResult('No Id found', 'failure');
        $window.open('SalesManagements/Sales/SalesReceivableReport?reportFormat=' + reportFormat + '&&voucherId=' + data.SalesPackingVoucherId, '_blank');
    };
  
    //$scope.onClickGRNID = function (args) {

    $scope.popUp = function () {
        $scope.getMasterOrderSales();
        angular.element(document.querySelector('#masterOrderSalespopUp')).modal('show');
    };
    $scope.paymentTerm();
    $scope.selectDoubleClick = function (x) {
        $scope.modelNew = x.data;
        $scope.modelNew.PostingDate = $scope.modelNew.InvoiceDate;
        $scope.modelNew.IsPaymentTermChangeable = x.data.IsPaymentTermChangeable;

        getmasterOrderDetailData($scope.modelNew.PartyAccountGroupId);
        getmasterOrderServiceDetailData($scope.modelNew.PartyAccountGroupId);
        getmasterOrderSalesJournalList($scope.modelNew.Id, $scope.modelNew.TaxApplicable, $scope.modelNew.PartyAccountGroupId);
        //getInventoryTaxList(data.data.Id);

        //factoryService.getCurrencyPrecision(data.data.BaseCurrencyId);
        //GetCurrencyExchangeRateList();
        $scope.modelNew.SourceType = x.data.SourceType;
        if ($scope.modelNew.SourceType == 'Packing') {
            //$scope.modelPacking.Id = x.data.SalesPackingId
            $scope.GetPackingDetail();
            $scope.packingJournal();
            $scope.GetCboVoucherTypePackingJournalList();
        }
        $scope.modelNew.PaymentTermId = x.data.PaymentTermId;
        $scope.modelNew.BaseNoOfDays = x.data.BaseNoOfDays;
        $scope.modelNew.BaseOnDueDate = x.data.BaseOnDueDate;
        $scope.modelNew.MatureDate = x.data.MatureDate;
        if ($scope.modelNew.IsPaymentTermChangeable) {
            $scope.changePaymentTerm($scope.modelNew.PaymentTermId)

        }
        else {
            if ($scope.modelNew.PaymentTermId != null) {
                var paymentTerm = $.grep($scope.paymentTermList, function (item) {
                    return item.Value === $scope.modelNew.PaymentTermId;
                })[0];
                if (paymentTerm.BaseLineDate === "documentdate") {
                    $scope.IsBaseOnDueDateEnable = false;
                }
            }

        }
        $scope.getCboVoucherTypeAccountReceivableList();
        $scope.closeMOSlesPopUp();
    };

    $scope.changePaymentTerm = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) {
                return item.Value === id;
            })[0];
            $scope.modelNew.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.modelNew.BaseNoOfDays = paymentTerm.NoOfDay;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === "postingdate") {
                    $scope.modelNew.BaseOnDueDate = $scope.modelNew.PostingDate;
                    $scope.IsBaseOnDueDateEnable = true;
                }
            if (paymentTerm.BaseLineDate === "default") {
                $scope.modelNew.BaseOnDueDate = $scope.modelNew.PostingDate;
                $scope.IsBaseOnDueDateEnable = true;
            }
            else {
                $scope.IsBaseOnDueDateEnable = false;
                $scope.modelNew.BaseOnDueDate = $scope.modelNew.DocDate;
            }
            $scope.getMatureDate($scope.modelNew.BaseOnDueDate, $scope.modelNew.BaseNoOfDays);
        }
    };

    $scope.closeMOSlesPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#masterOrderSalespopUp')).modal('hide');
    };
    function getmasterOrderDetailData(partyAccountGroupId) {
        $http.get('SalesManagements/Sales/GetMasterOrderSalesDetailList?salesId=' + $scope.modelNew.Id + '&partyAccountGroup=' + partyAccountGroupId)
            .then(function (response) {
                $scope.masterOrderDetailList = response.data;
            });
    }
    function getmasterOrderServiceDetailData(partyAccountGroupId) {
        $http.get('SalesManagements/Sales/GetMasterOrderSalesServiceDetailList?salesId=' + $scope.modelNew.Id + '&partyAccountGroup=' + partyAccountGroupId)
            .then(function (response) {
                $scope.masterOrderServiceDetailList = response.data;
            });
    }
    function getmasterOrderSalesJournalList(salesId, taxApplicable, partyAccountGroup) {
        $http.get('SalesManagements/Sales/GetMasterOrderSalesReceivableList?salesId=' + salesId + '&taxApplicable=' + taxApplicable + '&partyAccountGroup=' + partyAccountGroup)
            .then(function (response) {
                $scope.inventoryPayableList = [];
                $scope.inventoryReceiveDetailList = [];
                $scope.inventoryMaterialList = [];
                $scope.newList = [];
                $scope.inventoryMaterialList = response.data;
                reArrangeCreditableList($scope.inventoryMaterialList, $scope.newList, $scope.inventoryReceiveDetailList);
                //else if ($scope.modelNew.IsNonCreditable)
                //    reArrangeNonCreditableList($scope.inventoryMaterialList, $scope.newList, $scope.inventoryReceiveDetailList);
                //if (!baseService.isUndefinedOrNull(employeeId))
                //    $scope.glPushInList();
                //if (baseService.isUndefinedOrNull(employeeId))
                //    getVendorPayableGLBudgetActivity(inveReveiveId);
            });
    }
    $scope.inventoryTaxList = [];
    //function getInventoryTaxList(inveReveiveId) {
    //    $scope.inventoryTaxList = [];
    //    $http.get('Products/InventoryReceive/GetInventoryTaxList?inveReveiveId=' + inveReveiveId)
    //        .then(function (response) {
    //            $scope.inventoryTaxList = response.data;
    //        });
    //}

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
            if (row.OtherName === 'TaxReceivable' && row.TrnType === 'Dr' && row.Dr > 0) {
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
            else if (row.OtherName === 'TaxPayable' && row.TrnType === 'Cr' && row.Cr > 0) {
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
            else if (row.OtherName === 'SVTaxPayable' && row.TrnType === 'Cr' && row.Cr > 0) {
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
            else if (row.OtherName === 'SVTaxReceivable' && row.TrnType === 'Dr' && row.Dr > 0) {
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
            else if (row.OtherName === 'TCSPayable' && row.TrnType === 'Cr' && row.Cr > 0) {
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
            else if (row.OtherName === 'TCSReceivable' && row.TrnType === 'Dr' && row.Dr > 0) {
                var has = false;
                for (var a = 0; a < baseService.arrayLength(newList); a++) {
                    if (row.OtherName === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId && row.BudgetMasterId === newList[a].BudgetMasterId
                        && row.ActivityId === newList[a].ActivityId) {
                        newList[a].Dr += row.Dr;
                        newList[a].Amount += row.Cr;
                        has = true;
                        break;
                    }
                }
                if (!has)
                    newList.push(list[i]);
            }
            else if (row.OtherName === 'Sales' && row.TrnType === 'Cr') {
                newInvRecDetailList.push(list[i]);
                var has = false;
                for (var a = 0; a < baseService.arrayLength(newList); a++) {
                    if (row.OtherName === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId
                        && row.BudgetMasterId === newList[a].BudgetMasterId && row.ActivityId === newList[a].ActivityId) {
                        newList[a].Cr = Math.round((newList[a].Cr + row.Cr) * 100 + Number.EPSILON) / 100;
                        newList[a].Amount = Math.round((newList[a].Amount + row.Cr) * 100 + Number.EPSILON) / 100;
                        has = true;
                        break;
                    }
                }
                if (!has) {
                    newList.push(list[i]);
                }
            }
            else if (row.OtherName === 'Service' && row.TrnType === 'Cr') {
                newInvRecDetailList.push(list[i]);
                var has = false;
                for (var a = 0; a < baseService.arrayLength(newList); a++) {
                    if (row.OtherName === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId
                        && row.BudgetMasterId === newList[a].BudgetMasterId && row.ActivityId === newList[a].ActivityId) {
                        newList[a].Cr += row.Cr;
                        newList[a].Amount += row.Cr;
                        has = true;
                        break;
                    }
                }
                if (!has)
                    newList.push(list[i]);
            }
            else if (row.OtherName === 'Customer' && row.TrnType === 'Dr') {
                newList.push(list[i]);
                $scope.TotalPayableAmount += list[i].Amount;
            }
        }
    }

    $scope.voucherTypeList = [];
    $scope.getCboVoucherTypeAccountReceivableList = function () {
        cboService.getCboVoucherTypeAccountReceivableList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;

            }
        });
    };

    $scope.onShowReportMOS = function () {
        if (baseService.isUndefinedOrNull($scope.modelNew.Id)) return ShowResult('No Id found', 'failure');
        $window.open('SalesManagements/Sales/SalesReport?reportFormat=' + 'Pdf' + '&&salesId=' + $scope.modelNew.Id, '_blank');
    };
    $scope.modelPacking = {};
    $scope.packingVoucherTypeList = [];
    $scope.GetCboVoucherTypePackingJournalList = function () {
        cboService.getCboVoucherTypePackingJournalList(function (result) {
            $scope.packingVoucherTypeList = result;
            if ($scope.packingVoucherTypeList.length === 1) {
                $scope.modelPacking.VoucherTypeId = $scope.packingVoucherTypeList[0].Value;

            }
        });
    };

    $scope.packingJournaldataList = [];
    $scope.packingJournal = function () {
        $scope.packingJournaldataList = [];
        $http.get('SalesManagements/Sales/GetPackingJournal?salesId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.packingJournaldataList = response.data;
            });
    }
    $scope.packingDetailList = [];
    $scope.GetPackingDetail = function () {
        $scope.packingDetailList = [];
        $http.get('SalesManagements/Sales/GetPackingDetail?salesId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.packingDetailList = response.data;
            });
    }


    $scope.onClickDeletePopUp = function (x) {
        var data = x;
        $scope.salesId = data.Id;
        $scope.voucherId = data.VoucherId;

        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };


    $scope.delete = function (salesId, voucherId) {
        $http({
            method: "POST",
            url: 'SalesManagements/Sales/DeleteMasterOrderSalePost',
            data: {
                "salesId": salesId, "voucherId": voucherId 
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getMasterOrderSalesPosted();
                $scope.Clear();
                $scope.salesId = null;
                $scope.voucherId = null;
               
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };






}