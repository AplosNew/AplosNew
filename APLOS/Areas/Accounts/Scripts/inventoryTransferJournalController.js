'use strict';
inventoryTransferJournalController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'factoryService', '$window'];
function inventoryTransferJournalController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, factoryService, $window) {
    $rootScope.title = "Inventory Transfer";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Accounts/InventoryPayable/';
    $scope.getListUrl = 'Products/InventoryReceive/GetPostingList/';
    $scope.saveUrl = 'Accounts/InvoicePost/PostInventoryTransfer';
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


    $scope.searchByPostedGRN = "Id"; $scope.searchGRN = "";
    $scope.searchByPostedGRNList = [{ value: 'Id', name: "GRN No" }, { value: 'GRNDate', name: "GRN Date" }, { value: 'Particular', name: "Particular" }, { value: 'VoucherNo', name: "VoucherNo" }
        , { value: 'PostingDate', name: "PostingDate" }, { value: 'GateEntryNo', name: "Gate EntryNo" }, { value: 'DocRefNo', name: "DocRef No" }
        , { value: 'DocDate', name: "Doc Date" }];

    $scope.products = [];    $scope.getDataList = function () {        $http({            method: 'POST',            url: 'Accounts/InventoryTransferPost/GetPostedInventoryTransferList',            data: { column: $scope.searchByPostedGRN, value: $scope.searchGRN },
            dataType: 'JSON',        }).then(function successCallback(response) {            $scope.products = response.data;        });    };
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
        , AddtionalTax: null
        , IsInvoice: true
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

    $scope.approvedGRNList = [];    $scope.getPopUpData = function () {        $http({            method: 'GET',            url: 'Accounts/InventoryTransferPost/GetListForTransferJournal',        }).then(function successCallback(response) {            $scope.approvedGRNList = response.data;            for (var i = 0; i < $scope.approvedGRNList.length; i++) {
                response.data[i].GRNDate = new Date($scope.approvedGRNList[i].GRNDate);                response.data[i].DocDate = new Date($scope.approvedGRNList[i].DocDate);
            }        });    };
    $scope.popUp = function () {
        $scope.getPopUpData();
        angular.element(document.querySelector('#GRNpopUp')).modal('show');
    };


    $scope.selectDoubleClick = function (data) {
        var voucherTypeId = $scope.modelNew.VoucherTypeId;
        $scope.modelNew = data.data;
        $scope.modelNew.VoucherTypeId = voucherTypeId;
        $scope.modelNew.EmployeeTransactionTypeId = null;
        $scope.TempEmployeeId = data.data.EmployeeId;
        $scope.AcceptanceId = data.data.AcceptanceId;
        $scope.AcceptanceDate = data.data.AcceptanceDate;
        $scope.PurchaseLCId = data.data.PurchaseLCId;
        $scope.ContractId = data.data.ContractId;
        $scope.PartyId = data.data.PartyId;
        $scope.modelNew.ToPlantId = data.data.ToPlantId;
        $scope.modelNew.IsFOC = data.data.IsFOC;
        if (data.data.AcceptanceId) {
            $scope.modelNew.IsInvoice = false;
        } else
        $scope.modelNew.IsInvoice = true;
            $scope.modelNew.PostingDate = data.data.GRNDateNew;
        $scope.modelNew.GRNDateNew = data.data.GRNDateNew;
        if (!baseService.isUndefinedOrNull(data.data.EmployeeId) && $scope.employeeTransactionTypeList.length === 1) {
            $scope.modelNew.EmployeeTransactionTypeId = $scope.employeeTransactionTypeList[0].EmployeeTransactionTypeId;
        }
        $scope.paymentTerm();
        getRecievedList();
        getFromPlantInventoryTransferPayable(data.data.Id);
        factoryService.getCurrencyPrecision(data.data.BaseCurrencyId);
        $scope.closeGRNPopUp();
    };

    $scope.closeGRNPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#GRNpopUp')).modal('hide');
    };
    function getVendorPayableGLBudgetActivity(inveReveiveId,partyId) {
        $http.get('Accounts/InventoryTransferPost/GetTransferVendorPayableGLBudgetActivity?inveReveiveId=' + inveReveiveId + '&partyId=' + partyId)
            .then(function (response) {
                $scope.inventoryPayableList = [];
                $scope.inventoryPayableList = response.data;
            });
    }
    $scope.FromPlantInventoryTransferPayableList = [];
    $scope.ToPlantInventoryTransferPayableList = [];
    function getFromPlantInventoryTransferPayable(inveReveiveId) {
        $http.get('Accounts/InventoryTransferPost/GetFromPlantInventoryTransferPayable?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.FromPlantInventoryTransferPayableList = [];
                $scope.FromPlantInventoryTransferPayableList = response.data;
                getToPlantInventoryTransferPayable(inveReveiveId);
            });
    }

    function getToPlantInventoryTransferPayable(inveReveiveId) {
        $http.get('Accounts/InventoryTransferPost/GetToPlantInventoryTransferPayable?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.ToPlantInventoryTransferPayableList = [];
                $scope.ToPlantInventoryTransferPayableList = response.data;
                getVendorPayableGLBudgetActivity(inveReveiveId, $scope.PartyId);
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


    $scope.Post = function () {
 
        for (var i = 0; i < $scope.FromPlantInventoryTransferPayableList.length; i++) {
            $scope.FromPlantInventoryTransferPayableList[i].Amount = parseFloat($scope.FromPlantInventoryTransferPayableList[i].Amount).toFixed(4);
        }
        for (var i = 0; i < $scope.ToPlantInventoryTransferPayableList.length; i++) {
            $scope.ToPlantInventoryTransferPayableList[i].Amount = parseFloat($scope.ToPlantInventoryTransferPayableList[i].Amount).toFixed(4);
        }
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                receiveId: $scope.modelNew.Id
                , voucherVM: $scope.modelNew
                , fromPlantInventoryTransferJVList: $scope.FromPlantInventoryTransferPayableList
                , toPlantInventoryTransferJVList: $scope.ToPlantInventoryTransferPayableList
                , inventoryPayableVMList: $scope.inventoryPayableList
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
        $scope.FromPlantInventoryTransferPayableList = [];
        $scope.ToPlantInventoryTransferPayableList = [];
        $scope.inventoryReceivedList = [];
        $scope.inventoryPayableList = [];
        $scope.inventoryReceiveDetailList = [];
        $scope.advanceTaxesList = [];
        $scope.newList = [];
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
    };

    function getRecievedList() {
        $http.get('Accounts/InventoryTransferPost/GetInventoryMaterialPayableList?inveReveiveId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.inventoryReceivedList = response.data.Rows;
                checkSameValueInColumnList($scope.inventoryReceivedList, 'TransactionUoM');
            });
    }

    //function getServiceChargeList() {
    //    $http.get('Products/GoodsReceiveNote/GetServiceChargeList?receiveId=' + $scope.modelNew.Id)
    //        .then(function (response) {
    //            $scope.chargesList = [];
    //            $scope.chargesList = response.data;
    //            console.log($scope.chargesList);
    //        });
    //}

    $scope.sumORnot = false;
    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }

    //$scope.getPabyableJournal = function (data, reportFormat) {
    //    $window.open($scope.path + 'GetInvetoryTransferVoucher?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&employeeId=' + data.EmployeeId + '&isReversCharge=' + data.IsTaxApplicable, '_blank');
    //};



    $scope.onClickReportDownloadWord = function (args) {        debugger;        var gridObj = $("#GridPrint").data("ejGrid");        var data = gridObj.getSelectedRecords()[0];        var reportFormat = "Pdf";        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');        $window.open('Accounts/InventoryTransferPost/GetInvetoryTransferVoucher?reportFormat=' + reportFormat + '&plantId=' + data.FromPlantId + '&plantName=' + data.FromPlantName + '&voucherId=' + data.FromVoucherId , '_blank');
    };    $scope.commandPDF = [{        type: "details", buttonOptions: {            text: "PDF",            width: "50",            height: "20",            click: $scope.onClickReportDownloadWord        }    }];

    $scope.onClickReportDownloadExcel = function (args) {        debugger;        var gridObj = $("#GridPrint").data("ejGrid");        //getting corresponding record         var data = gridObj.getSelectedRecords()[0];        var reportFormat = "Excel";        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');        $window.open('Accounts/InventoryTransferPost/GetInvetoryTransferVoucher?reportFormat=' + reportFormat + '&plantId=' + data.FromPlantId + '&plantName=' + data.FromPlantName + '&voucherId=' + data.FromVoucherId, '_blank');
    };
    $scope.commandExcel = [{        type: "details", buttonOptions: {            text: "Excel",            width: "50",            height: "20",            click: $scope.onClickReportDownloadExcel        }    }];



    $scope.onClickToPlantReportDownloadWord = function (args) {        debugger;        var gridObj = $("#GridPrint").data("ejGrid");        var data = gridObj.getSelectedRecords()[0];        var reportFormat = "Pdf";        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');        $window.open('Accounts/InventoryTransferPost/GetInvetoryTransferVoucher?reportFormat=' + reportFormat + '&plantId=' + data.ToPlantId + '&plantName=' + data.ToPlantName + '&voucherId=' + data.ToVoucherId, '_blank');
    };    $scope.commandToPlantPDF = [{        type: "details", buttonOptions: {            text: "PDF",            width: "50",            height: "20",            click: $scope.onClickToPlantReportDownloadWord        }    }];

    $scope.onClickToPlantReportDownloadExcel = function (args) {        debugger;        var gridObj = $("#GridPrint").data("ejGrid");        //getting corresponding record         var data = gridObj.getSelectedRecords()[0];        var reportFormat = "Excel";        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');        $window.open('Accounts/InventoryTransferPost/GetInvetoryTransferVoucher?reportFormat=' + reportFormat + '&plantId=' + data.ToPlantId + '&plantName=' + data.ToPlantName + '&voucherId=' + data.ToVoucherId, '_blank');
    };
    $scope.commandToPlantExcel = [{        type: "details", buttonOptions: {            text: "Excel",            width: "50",            height: "20",            click: $scope.onClickToPlantReportDownloadExcel        }    }];

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
   

}