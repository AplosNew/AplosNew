'use strict';
FinishGoodsBookingPostController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window'];
function FinishGoodsBookingPostController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $rootScope.title = "FinishGoods Book Post ";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Productions/FinishGoodsBooking/';
    $scope.getListUrl = 'Productions/FinishGoodsBooking/GetPostingList/';
    $scope.saveUrl = 'Productions/FinishGoodsBooking/FinishGoodsBookingPost/';
    $scope.AcceptanceId = null;
    $scope.TotalPayableAmount = 0;
    $scope.IsPostButtonDisable = false;

    $scope.searchByPostedGRN = "Id"; $scope.searchGRN = "";
    $scope.searchByPostedGRNList = [{ value: 'InventoryReceiveId', name: "FG Inventory No" }, { value: 'Id', name: "FG Book No" }, { value: 'VoucherNo', name: "VoucherNo" }
        , { value: 'PostingDate', name: "PostingDate" }, { value: 'DocRefNo', name: "DocRef No" }
        , { value: 'DocDate', name: "Doc Date" }];

    $scope.products = [];
    $scope.getDataList = function () {
        $http({
            method: 'Get',
            url: 'Productions/FinishGoodsBooking/GetPostedFinishGoodsBookingData',
            data: { column: $scope.searchByPostedGRN, value: $scope.searchGRN },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.products = response.data;
            for (var i = 0; i < $scope.products.length; i++) {
                response.data[i].PostingDate = new Date($scope.products[i].PostingDate);
                response.data[i].DocDate = new Date($scope.products[i].DocDate);
            }
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
        , PostingDate: $filter("dateFiltering")(Date.now())
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
        , InvoiceReceiveId: null
        , ProcessName: null

        , PartyId: null
        , PartyPlantId: null
        , PartyName: null
        , PaymentTermId: null
        , PaymentTermName: null
        , VoucherTypeId: null
        , ToCurrencyRate: null
        , Narration: null
        , PaymentTermCode: null
        , AddtionalTax: null
        , IsInvoice: false
        , EntityId: null
        , ProcessId: null
        , Description: null
        , BookingDate:null
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

   
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });


    $scope.getCboVoucherType = function () {
        cboService.getCboVoucherTypeFGInventoryList(function (result) {
            $scope.voucherTypeList = result;
            if (baseService.arrayLength($scope.voucherTypeList) === 1)
                $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
        });
    }

    $scope.approvedGRNList = [];
    $scope.getPopUpData = function () {
        $http({
            method: 'GET',
            url: 'Productions/FinishGoodsBooking/GetListForFinishGoodsBookingPost',
        }).then(function successCallback(response) {
            $scope.approvedGRNList = response.data;
            for (var i = 0; i < $scope.approvedGRNList.length; i++) {
                response.data[i].PostingDate = new Date($scope.approvedGRNList[i].PostingDate);
                response.data[i].BookingDate = new Date($scope.approvedGRNList[i].BookingDate);
                response.data[i].FromDate = new Date($scope.approvedGRNList[i].FromDate);
                response.data[i].ToDate = new Date($scope.approvedGRNList[i].ToDate);
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
        $scope.modelNew.DocRefNo = data.data.Id;
        $scope.modelNew.InventoryReceiveId = data.data.InventoryReceiveId;
        $scope.modelNew.ProcessName = data.data.ProcessName;
        $scope.modelNew.CompanyCurrencyRate = data.data.CompanyCurrencyRate;
        $scope.modelNew.CurrencyId = data.data.CurrencyId;
        $scope.TotalPayableAmount = 0;
        $scope.getCboVoucherType();

        $scope.modelNew.PostingDate = $filter("dateFiltering")(data.data.PostingDate);
        $scope.modelNew.DocDate = $filter("dateFiltering")(data.data.PostingDate);
        getRecievedList();
        getInventoryMaterialList(data.data.InventoryReceiveId);
        getInventoryTaxList(data.data.Id);
       
        $scope.closeGRNPopUp();
    };

    $scope.closeGRNPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#GRNpopUp')).modal('hide');
    };
    $scope.fGInventoryGLBudgetActivityList = [];
    function getFGInventoryGLBudgetActivity(inveReveiveId) {
        $http.get('Productions/FinishGoodsBooking/GetFGInventoryGLBudgetActivity?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.fGInventoryGLBudgetActivityList = [];
                $scope.fGInventoryGLBudgetActivityList = response.data;
            });
    }
    function getInventoryMaterialList(inveReveiveId) {
        $http.get('Productions/FinishGoodsBooking/GetFGJournal?inventoryReceiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.fGInventoryGLBudgetActivityList = [];
                $scope.inventoryReceiveDetailList = [];
                $scope.inventoryMaterialList = [];
                $scope.newList = [];
                $scope.inventoryMaterialList = response.data;

                
                    reArrangeCreditableList($scope.inventoryMaterialList, $scope.newList, $scope.inventoryReceiveDetailList);
                getFGInventoryGLBudgetActivity(inveReveiveId);
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
      
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            var row = list[i];
           
            if (row.OtherName === 'FGInventory' && row.TrnType === 'Dr') {
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
           
             else if (row.OtherName === 'WIPSFG'  && row.TrnType === 'Cr') {
                newList.push(list[i]);
                $scope.TotalPayableAmount += list[i].Amount;
            }
            
        }
    }


    $scope.Post = function () {
        $scope.IsPostButtonDisable = true;
        if (baseService.isUndefinedOrNull($scope.modelNew.EntityId)) return ShowResult('Please Select Entity', 'failure');
       
        for (var i = 0; i < $scope.newList.length; i++) {
            $scope.newList[i].Amount = parseFloat($scope.newList[i].Amount).toFixed(4);
        }
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                 voucherVM: $scope.modelNew
                , voucherDetailVMList: $scope.newList
                , fGInventoryGLBudgetActivityVMList : $scope.fGInventoryGLBudgetActivityList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Clear();
                $scope.getDataList($scope.modelNew.Id);
                $scope.IsPostButtonDisable = false;
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.Clear = function () {
        $scope.model = {};
        $scope.modelNew = { PostingDate: new Date() };
        $scope.inventoryMaterialList = [];
        $scope.inventoryReceivedList = [];
        $scope.fGInventoryGLBudgetActivityList = [];
        $scope.inventoryReceiveDetailList = [];
        $scope.advanceTaxesList = [];
        $scope.IsPostButtonDisable = false;
        $scope.newList = [];
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
    };

    function getRecievedList() {
        $http.get('Productions/FinishGoodsBooking/GetFGMaterialDetail?inventoryReceiveId=' + $scope.modelNew.InventoryReceiveId)
            .then(function (response) {
                $scope.inventoryReceivedList = response.data.Rows;
            });
    }


   


    $scope.onClickReportDownloadWord = function (data) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open($scope.path + 'FinishGoodsBookingPostReport?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');

    };

  

    $scope.onClickReportDownloadExcel = function (data) {
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open($scope.path + 'FinishGoodsBookingPostReport?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');

    };
  

    $scope.onClickGRNID = function (data) {
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id;
    };
}