'use strict';
PurchaseReturnController.$inject = ['addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function PurchaseReturnController(addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Purchase Return"; //Inventory Receive
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/GoodsReceiveNote/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getListUrl1 = $scope.path + 'GetListForGRNSaveData';
    $scope.getListUrl2 = $scope.path + 'GetListForGrnByPoReq';

    $scope.saveUrl = $scope.path + 'CreatePurchaseReturn';
    $scope.updateUrl1 = $scope.path + 'UpdareGRN';

    //$scope.saveUrl = $scope.path + 'InsertGRN';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'DeletePurchaseReturnfinal/';
    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete?receiveDetailId=';
    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
    $scope.partyType = 'Vendor';
    $scope.isAdvance = false;
    $scope.currentDate = new Date(Date.now());
    $scope.grossTotal = 0;
    $scope.chargesList = [];
    $scope.chargesListPO = [];
    $scope.storageList = [];
    $scope.currencyList = [];
    $scope.detailModelSave = [];
    $scope.inventoryMaterialListPOnew = [];
    $scope.chargesListPOnew = [];

    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.POReturnNo = null;
    $scope.NotificationSettingStatus = function () {
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/NotificationSettingForPurchaserReturn',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NotificationSetting = response.data;
            $scope.CheckedByStatusForNoti = $scope.NotificationSetting[0].RequiredChecking;
            $scope.ApprovedByStatusForNoti = $scope.NotificationSetting[0].RequiredApproval;
            //$scope.GetCheckedByAndApprovedBy1();
            if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === false) {
                $scope.productNew.labelCheckAndApproved = 'To be checked by';
            }
            else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be approved by';
            }
            else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be checked by';
            }

        });
    }
    $scope.NotificationSettingStatus();
    $scope.GetCheckedByAndApprovedBy1 = function () {
        //debugger;

        if (!baseService.isUndefinedOrNull($scope.CheckedByStatusForNoti) && !baseService.isUndefinedOrNull($scope.ApprovedByStatusForNoti)) {
            $http({
                method: 'GET',
                url: 'Products/GoodsReceiveNote/GetCheckedByAndApprovedBYForPurchaserReturn?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.checkedByList = response.data;
            });

        }
        else {

        }

    }

    $scope.searchByPostedGRN = "Id"; $scope.searchGRN = "";
    $scope.searchByPostedGRNList = [{ value: 'Id', name: "GRN No" }, { value: 'GRNDate', name: "GRN Date" }, { value: 'PartyName', name: "Particular" }, { value: 'VoucherNo', name: "VoucherNo" }
        , { value: 'PostingDate', name: "PostingDate" }, { value: 'GateEntryNo', name: "Gate EntryNo" }, { value: 'DocRefNo', name: "DocRef No" }
        , { value: 'DocDate', name: "Doc Date" }];

    $scope.getListPOByReqG = [];
    $scope.ApprovedGRNList = [];
    $scope.ApprovedGRNListFordisplay = function () {
        //debugger;
        var PoType = 'POByReq';
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/PostedGRNListForPurchaseReturn',
            data: { column: $scope.searchByPostedGRN, value: $scope.searchGRN },
        }).then(function successCallback(response) {
            $scope.ApprovedGRNList = response.data;
            $scope.productNew.GRNDate = $filter("dateFiltering")(Date.now());
        });
    };

    $scope.POPopUpGRNPOReqList = function () {
        $scope.ApprovedGRNListFordisplay();

        angular.element(document.querySelector('#POPopUp')).modal('show');

    };
    $scope.recorddoubleclickFromMasterGrid = function ($event) {

        ClearFields();
        $scope.NotificationSettingStatus();
        var x = $event;
        var Id = x.data.Id;
        if (x.data.IsOpeningBalance === 'Yes') {
            ShowResult('You can not return opening balance data', 'failure', 'POPopUp');
            return false;
        }
        else {
            $scope.POId1 = x.data.Id;
            $scope.POID = x.data.POID;
            $scope.product = $scope.products[$scope.index];
            $scope.productNew = Object.assign({}, $scope.product);
            $scope.productNew = x.data;
            $scope.productNew.GRNDate = x.data.GRNDate1;
            $scope.productNew.CheckedBy = "";

            getPartyPlantList();
            getInventoryMaterialList(Id);
            getServiceChargeList(Id);
            $scope.productNew.TaxOptionAddiTax = 'Yes';
            getTCSData(Id)
            $scope.productId = Id;

            if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
                var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
                if (paymentTerm.BaseLineDate !== null)
                    if (paymentTerm.BaseLineDate === 'documentdate')
                        $scope.IsBaseOnDueDateEnable = true;
                    else
                        $scope.IsBaseOnDueDateEnable = false;
            }

            angular.element(document.querySelector('#POPopUp')).modal('hide');
        }

    }
    $scope.inventoryMaterialListPO = [];
    $scope.sumORnot = false;
    function getInventoryMaterialList(inveReveiveId) {
        $scope.masterId5 = inveReveiveId;
        $http.get('Products/GoodsReceiveNote/GetInventoryMaterialListForPurchaseReturn?inveReveiveId=' + inveReveiveId + '&POID=' + $scope.POID)
            .then(function (response) {
                $scope.inventoryMaterialList = [];
                $scope.inventoryMaterialList = response.data;
                $scope.inventoryMaterialListPO = response.data;
                $scope.POIDs = $scope.inventoryMaterialList.POId;
                $scope.productNew.PODate = $scope.inventoryMaterialList[0].AddedDate;
                checkSameValueInColumnList($scope.inventoryMaterialList, 'TransactionUoM');
                getGrossAmount($scope.inventoryMaterialList, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
                $scope.GetPOMaterialTaxData();
            });
    }

    function getPartyPlantList() {
        $scope.plantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.IsDefault) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = item.Value;
                    $scope.productNew.InvoicingByAddress = item.Address1;
                    $scope.productNew.DeliveryByAddress = item.Address1;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = item.StateName;
                    $scope.productNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }

    $scope.calculateAmount1 = function (data, index) {
        if ($scope.Action === 'Save') {
            data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);//BooksCurrencyBaseRate
            if (data.TrnAmount == 'NaN')
                data.TrnAmount = 0;
            data.TaxAmount = 0;
            data.BaseTaxAmount = 0;
            angular.forEach(data.POMaterialTaxList, function (item) {
                if (data.POMaterialTaxList.InventoryReceiveDetailId = data.InventoryReceiveDetailId) {
                    item.TaxAmount = Math.round((data.TrnAmount * item.Percentage / 100) * 100 + Number.EPSILON) / 100;
                    data.BaseTaxAmount += item.TaxAmount;
                }
            });
            data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
            var TotalServiceAmount = data.ServiceChargeGRN * data.TransactionQty;
            var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'ServiceTaxGRN');
            var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TrnAmount');
            var ServiceTax = 0;
            angular.forEach($scope.ServiceTaxList, function (item1) {
                item1.ServiceChargeTaxAmount = TotalServiceAmount * item1.Percentage / 100;
                ServiceTax += item1.ServiceChargeTaxAmount;
            });

            var TotalServiceCharge = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'ServiceCharge');
            var TotalServiceTax = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'ServiceTax');
            var TotalMatSum = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TotalMatSum');
            var TotalChargesSum = $filter('sumByKey')($filter('filter')($scope.chargesList), 'GRNServiceAmount');
            for (var i5 = 0; i5 < $scope.chargesList.length; i5++) {
                $scope.chargesList[i5].Amount = 0;
            }
            for (var i6 = 0; i6 < $scope.inventoryMaterialList.length; i6++) {
                $scope.inventoryMaterialList[i6].ServiceTax = 0;
            }
            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId == data.InventoryReceiveDetailId) {//+ $scope.inventoryMaterialList[i].OtherReturned
                    if (data.TransactionQty <= ($scope.inventoryMaterialList[i].GRNReceived - ($scope.inventoryMaterialList[i].BaseIssueQty + $scope.inventoryMaterialList[i].ReductionByAdjustmentQty + $scope.inventoryMaterialList[i].InventorySalesQty + $scope.inventoryMaterialList[i].InventoryScrapQty + $scope.inventoryMaterialList[i].PurchaseReturnQty + $scope.inventoryMaterialList[i].InventoryTransferQty) + $scope.inventoryMaterialList[i].IssueReturnQty)) {

                    }
                    else {
                        ShowResult('Return qty can not grater than balance qty', 'failure');
                        $scope.inventoryMaterialList[i].TransactionQty = "";
                    }
                    $scope.inventoryMaterialList[i].TrnAmount = Math.round(data.TrnAmount * 100 + Number.EPSILON) / 100;
                    $scope.inventoryMaterialList[i].ServiceCharge = Math.round((data.ServiceChargeGRN * data.TransactionQty) * 100 + Number.EPSILON) / 100; //* $scope.inventoryMaterialList[i].TrnAmount;
                    $scope.inventoryMaterialList[i].BaseTaxAmount = Math.round(data.BaseTaxAmount * 100 + Number.EPSILON) / 100;//+ $scope.inventoryMaterialList[i].OtherReturned
                    $scope.inventoryMaterialList[i].Balance = (($scope.inventoryMaterialList[i].GRNReceived - ($scope.inventoryMaterialList[i].BaseIssueQty + $scope.inventoryMaterialList[i].ReductionByAdjustmentQty + $scope.inventoryMaterialList[i].InventorySalesQty + $scope.inventoryMaterialList[i].InventoryScrapQty + $scope.inventoryMaterialList[i].PurchaseReturnQty) + $scope.inventoryMaterialList[i].IssueReturnQty) - $scope.inventoryMaterialList[i].TransactionQty);
                    var totalTaxAmountTemp = 0;
                    var ServiceMasterId = '';
                }
                else {
                    if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId == data.InventoryReceiveDetailId) {
                        $scope.inventoryMaterialList[i].ServiceCharge = Math.round((TotalServiceAmount * data.TransactionQty) * 100 + Number.EPSILON) / 100; //* $scope.inventoryMaterialList[i].TrnAmount;
                        $scope.inventoryMaterialList[i].BaseTaxAmount = Math.round(data.BaseTaxAmount * 100 + Number.EPSILON) / 100;//+ $scope.inventoryMaterialList[i].OtherReturned 
                        $scope.inventoryMaterialList[i].Balance = (($scope.inventoryMaterialList[i].GRNReceived - ($scope.inventoryMaterialList[i].BaseIssueQty + $scope.inventoryMaterialList[i].ReductionByAdjustmentQty + $scope.inventoryMaterialList[i].InventorySalesQty + $scope.inventoryMaterialList[i].InventoryScrapQty + $scope.inventoryMaterialList[i].PurchaseReturnQty) + $scope.inventoryMaterialList[i].IssueReturnQty) - $scope.inventoryMaterialList[i].TransactionQty);
                        var totalTaxAmountTemp = 0;
                        var ServiceMasterId = '';
                    }
                }
            }
            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                for (var i4 = 0; i4 < $scope.chargesList.length; i4++) {
                    $scope.chargesList[i4].Amount += Math.round((($scope.chargesList[i4].GRNServiceAmount / TotalMatSum) * $scope.inventoryMaterialList[i].TransactionRate * $scope.inventoryMaterialList[i].TransactionQty) * 100 + Number.EPSILON) / 100;
                    for (var i2 = 0; i2 < $scope.ServiceTaxList.length; i2++) {
                        if ($scope.chargesList[i4].ServiceMasterId === $scope.ServiceTaxList[i2].ServiceMasterId) {
                            $scope.ServiceTaxList[i2].TaxAmount = Math.round((($scope.chargesList[i4].Amount * $scope.ServiceTaxList[i2].Percentage).toFixed(2) / 100) * 100 + Number.EPSILON) / 100;
                        }
                    }
                    var TotalChargesTaxSum = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList, { "ServiceMasterId": $scope.chargesList[i4].ServiceMasterId }), 'TaxAmount');
                    $scope.chargesList[i4].TotalTaxAmount = Math.round(TotalChargesTaxSum * 100 + Number.EPSILON) / 100;
                }

            }


            for (var i7 = 0; i7 < $scope.inventoryMaterialList.length; i7++) {
                var TotalServiceNewTax = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');

                var Totalsumtrnamt = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TrnAmount');
                $scope.inventoryMaterialList[i7].ServiceTax = Math.round(((TotalServiceNewTax / Totalsumtrnamt) * $scope.inventoryMaterialList[i7].TrnAmount) * 100 + Number.EPSILON) / 100;
            }

            $scope.advanceTax.TaxAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseAmount") * $scope.advanceTax.ValueOfFixed / 100).toFixed(2);
            $scope.additionalTax();
            for (var i9 = 0; i9 < $scope.inventoryMaterialList.length; i9++) {
                if ($scope.productNew.IsNonCreditable == 1) {
                    if ($scope.inventoryMaterialList[i9].InventoryReceiveDetailId == data.InventoryReceiveDetailId) {
                        $scope.inventoryMaterialList[i9].TotalMaterialTranAmount = Math.round(parseFloat($scope.inventoryMaterialList[i9].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i9].ServiceCharge) + parseFloat(data.ServiceTax) * 100 + Number.EPSILON) / 100;//data.TrnAmount+;//
                        $scope.inventoryMaterialList[i9].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialList[i9].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i9].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;//data.TrnAmount;
                    }
                }
                else {
                    if ($scope.inventoryMaterialList[i9].InventoryReceiveDetailId == data.InventoryReceiveDetailId) {
                        $scope.inventoryMaterialList[i9].TotalMaterialTranAmount = Math.round(($scope.inventoryMaterialList[i9].TrnAmount + $scope.inventoryMaterialList[i9].ServiceCharge) * 100 + Number.EPSILON) / 100;
                        $scope.inventoryMaterialList[i9].TotalMaterialBaseAmount = Math.round((($scope.inventoryMaterialList[i9].TrnAmount + $scope.inventoryMaterialList[i9].ServiceCharge) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
                    }
                }
            }

        }
        else {

            data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);//BooksCurrencyBaseRate
            if (data.TrnAmount == 'NaN')
                data.TrnAmount = 0;
            data.TaxAmount = 0;
            data.BaseTaxAmount = 0;
            angular.forEach(data.POMaterialTaxList, function (item) {
                if (data.POMaterialTaxList.InventoryReceiveDetailId = data.InventoryReceiveDetailId) {
                    item.TaxAmount = data.TrnAmount * item.Percentage / 100;
                    data.BaseTaxAmount += item.TaxAmount;
                }
            });
            data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
            var TotalServiceAmount = data.ServiceChargeGRN * data.TransactionQty;
            var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'ServiceTaxGRN');
            var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TrnAmount');
            var ServiceTax = 0;
            angular.forEach($scope.ServiceTaxList, function (item1) {
                item1.ServiceChargeTaxAmount = TotalServiceAmount * item1.Percentage / 100;
                ServiceTax += item1.ServiceChargeTaxAmount;
            });

            var TotalServiceCharge = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'ServiceCharge');
            var TotalServiceTax = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'ServiceTax');
            var TotalMatSum = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TotalMatSum');
            var TotalChargesSum = $filter('sumByKey')($filter('filter')($scope.chargesList), 'GRNServiceAmount');
            for (var i5 = 0; i5 < $scope.chargesList.length; i5++) {
                $scope.chargesList[i5].Amount = 0;
            }
            for (var i6 = 0; i6 < $scope.inventoryMaterialList.length; i6++) {
                $scope.inventoryMaterialList[i6].ServiceTax = 0;
            }
            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId == data.InventoryReceiveDetailId) {//+ $scope.inventoryMaterialList[i].OtherReturned
                    if (data.TransactionQty <= ($scope.inventoryMaterialList[i].GRNReceived - ($scope.inventoryMaterialList[i].BaseIssueQty + $scope.inventoryMaterialList[i].ReductionByAdjustmentQty + $scope.inventoryMaterialList[i].InventorySalesQty + $scope.inventoryMaterialList[i].InventoryScrapQty + $scope.inventoryMaterialList[i].PurchaseReturnQty + $scope.inventoryMaterialList[i].InventoryTransferQty) + $scope.inventoryMaterialList[i].IssueReturnQty)) {

                    }
                    else {
                        ShowResult('Return qty can not grater than balance qty', 'failure');
                        $scope.inventoryMaterialList[i].TransactionQty = "";
                    }
                    $scope.inventoryMaterialList[i].TrnAmount = Math.round(data.TrnAmount * 100 + Number.EPSILON) / 100;
                    $scope.inventoryMaterialList[i].ServiceCharge = Math.round((data.ServiceChargeGRN * data.TransactionQty) * 100 + Number.EPSILON) / 100; //* $scope.inventoryMaterialList[i].TrnAmount;
                    $scope.inventoryMaterialList[i].BaseTaxAmount = Math.round(data.BaseTaxAmount * 100 + Number.EPSILON) / 100;//+ $scope.inventoryMaterialList[i].OtherReturned
                    $scope.inventoryMaterialList[i].Balance = (($scope.inventoryMaterialList[i].GRNReceived - ($scope.inventoryMaterialList[i].BaseIssueQty + $scope.inventoryMaterialList[i].ReductionByAdjustmentQty + $scope.inventoryMaterialList[i].InventorySalesQty + $scope.inventoryMaterialList[i].InventoryScrapQty + $scope.inventoryMaterialList[i].PurchaseReturnQty) + $scope.inventoryMaterialList[i].IssueReturnQty) - $scope.inventoryMaterialList[i].TransactionQty);
                    var totalTaxAmountTemp = 0;
                    var ServiceMasterId = '';
                }
                else {
                    if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId == data.InventoryReceiveDetailId) {
                        $scope.inventoryMaterialList[i].ServiceCharge = Math.round((TotalServiceAmount * data.TransactionQty) * 100 + Number.EPSILON) / 100; //* $scope.inventoryMaterialList[i].TrnAmount;
                        $scope.inventoryMaterialList[i].BaseTaxAmount = Math.round(data.BaseTaxAmount * 100 + Number.EPSILON) / 100;//+ $scope.inventoryMaterialList[i].OtherReturned 
                        $scope.inventoryMaterialList[i].Balance = (($scope.inventoryMaterialList[i].GRNReceived - ($scope.inventoryMaterialList[i].BaseIssueQty + $scope.inventoryMaterialList[i].ReductionByAdjustmentQty + $scope.inventoryMaterialList[i].InventorySalesQty + $scope.inventoryMaterialList[i].InventoryScrapQty + $scope.inventoryMaterialList[i].PurchaseReturnQty) + $scope.inventoryMaterialList[i].IssueReturnQty) - $scope.inventoryMaterialList[i].TransactionQty);
                        var totalTaxAmountTemp = 0;
                        var ServiceMasterId = '';

                    }

                }
                if ($scope.productNew.IsNonCreditable == 1) {
                    if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId == data.InventoryReceiveDetailId) {
                        $scope.inventoryMaterialList[i].TotalMaterialTranAmount = Math.round(parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax) * 100 + Number.EPSILON) / 100;//data.TrnAmount+;//
                        $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;//data.TrnAmount;
                    }
                }
                else {
                    if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId == data.InventoryReceiveDetailId) {
                        $scope.inventoryMaterialList[i].TotalMaterialTranAmount = Math.round(($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) * 100 + Number.EPSILON) / 100;
                        $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = Math.round(($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) * $scope.productNew.ToCurrencyRate * 100 + Number.EPSILON) / 100;
                    }
                }



            }
            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                for (var i4 = 0; i4 < $scope.chargesList.length; i4++) {
                    $scope.chargesList[i4].Amount += Math.round((($scope.chargesList[i4].GRNServiceAmount / TotalMatSum) * $scope.inventoryMaterialList[i].TransactionRate * $scope.inventoryMaterialList[i].TransactionQty) * 100 + Number.EPSILON) / 100;
                    for (var i2 = 0; i2 < $scope.ServiceTaxList.length; i2++) {
                        if ($scope.chargesList[i4].ServiceMasterId === $scope.ServiceTaxList[i2].ServiceMasterId) {
                            $scope.ServiceTaxList[i2].TaxAmount = Math.round((($scope.chargesList[i4].Amount * $scope.ServiceTaxList[i2].Percentage).toFixed(2) / 100) * 100 + Number.EPSILON) / 100;

                        }


                    }
                    var TotalChargesTaxSum = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList, { "ServiceMasterId": $scope.chargesList[i4].ServiceMasterId }), 'TaxAmount');
                    $scope.chargesList[i4].TotalTaxAmount = parseFloat(TotalChargesTaxSum).toFixed(2);
                }

            }


            for (var i7 = 0; i7 < $scope.inventoryMaterialList.length; i7++) {
                var TotalServiceNewTax = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');
                var Totalsumtrnamt = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TrnAmount');
                $scope.inventoryMaterialList[i7].ServiceTax = Math.round(((TotalServiceNewTax / Totalsumtrnamt) * $scope.inventoryMaterialList[i7].TrnAmount) * 100 + Number.EPSILON) / 100;
            }


        }


    };

    $scope.product = {
        Id: null
        , POReturnDate: $filter("dateFiltering")(Date.now())
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: $window.plantId
        , PartyId: null
        , InvoicingPartyPlantId: null
        , InvoicingByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryPartyPlantId: null
        , DeliveryByAddress: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , CutOffDate: null
        , MaterialStorageId: null
        , CurrencyId: null
        , BaseCurrencyId: $scope.baseCurrencyId
        , ToCurrencyRate: 0
        , PaymentTermId: null
        , BaseOnDueDate: null
        , BaseNoOfDays: null
        , MatureDate: null
        , DocRefNo: null
        , DocDate: null
        , GateEntryNo: null
        , EntryDate: null
        , FixedAssetOrInventory: 'Inventory'
        , PODepended: false
        , AlongwithInvoice: true
        , InvoiceNo: null
        , InvoiceDate: null
        , IsNonCreditable: false
        , TaxApplicable: null
        , IsTaxApplicable: false
        , IsTaxApplicableChangeable: false
        , PartyType: $scope.partyType
        , POId: null
        , IsApproved: 0
        , CheckedBy: null
        , CheckedByStatus: null
        , AuthorizedBy: null
        , AuthorizedByStatus: null
        , NoteForAccounts: null
        , ApprovedByName: null
        , CheckedByName: null
        , labelCheckAndApproved: null
        , TaxOptionAddiTax: 'Yes'
    };

    $scope.GetMaterialTaxData = function () {
        //debugger;
        $scope.MaterialTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + $scope.masterId5
        }).then(function (response) {
            $scope.MaterialTaxList = response.data;

            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                var linepk = $scope.inventoryMaterialList[i].InventoryReceiveDetailId;
                var list = getMaterialtaxlist(linepk);
                $scope.inventoryMaterialList[i].MaterialTaxList = list;
            }
        });
    };
    function getMaterialtaxlist(linepk) {
        //debugger;
        var result4 = [];
        for (var i = 0; i < $scope.MaterialTaxList.length; i++) {
            if ($scope.MaterialTaxList[i].PODetailId === linepk) {
                result4.push($scope.MaterialTaxList[i]);
            }
        }
        return result4;
    }



    $scope.GetPOMaterialTaxData = function () {
        //debugger;
        $scope.POMaterialTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetReceiveTaxListGRN?receiveDetailId=' + $scope.POId1
        }).then(function (response) {
            $scope.POMaterialTaxList = response.data;

            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                var linepk = $scope.inventoryMaterialList[i].InventoryReceiveDetailId;
                var list = getPOMaterialtaxlist(linepk);
                $scope.inventoryMaterialList[i].POMaterialTaxList = list;
            }
        });
    };
    function getPOMaterialtaxlist(linepk) {
        //debugger;
        var result = [];
        for (var i = 0; i < $scope.POMaterialTaxList.length; i++) {
            if ($scope.POMaterialTaxList[i].PODetailId === linepk) {
                result.push($scope.POMaterialTaxList[i]);
            }
        }
        return result;
    }

    $scope.GetPOMaterialTaxDataModify = function () {
        //debugger;
        $scope.POMaterialTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetReceiveTaxListGRNPurchaseReturnModify?receiveDetailId=' + $scope.POReturnNo
        }).then(function (response) {
            $scope.POMaterialTaxList = response.data;

            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                var linepk = $scope.inventoryMaterialList[i].InventoryReceiveDetailId;
                var list = getPOMaterialtaxlistModify(linepk);
                $scope.inventoryMaterialList[i].POMaterialTaxList = list;
            }
        });
    };
    function getPOMaterialtaxlistModify(linepk) {
        //debugger;
        var result = [];
        for (var i = 0; i < $scope.POMaterialTaxList.length; i++) {
            if ($scope.POMaterialTaxList[i].PODetailId === linepk) {
                result.push($scope.POMaterialTaxList[i]);
            }
        }
        return result;
    }

    $scope.checkedByList = [];
    $scope.GetPurchaseReturnCheckedBy = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/GetPurchaseReturnCheckedBy'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
    }
    $scope.GetPurchaseReturnCheckedBy();
    $scope.invalidPostingDate = false;

    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.GRNDate) > new Date()) {
            msg = "Return date must be below or equal to current Date!";
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.productNew.GRNDate) > new Date($scope.GRNDate)) {
            msg = "Return date must be below or equal to GRN Date!";
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.GRNDate) < new Date($scope.productNew.DocDate)) {
            msg = "Doc date must be below or equal to Return Date!";
            $scope.invalidPostingDate = true;
        }

        else if (baseService.isUndefinedOrNull($scope.GRNDate)) {
            msg = "Return Date is required.";
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_ReturnDate", $scope.invalidPostingDate, msg);
    };

    $scope.inventoryMaterialListPOnew = [];
    $scope.Save = function () {
        //debugger;
        try {
            if ($scope.productNew.NoteForAccounts === '' || $scope.productNew.NoteForAccounts === null || $scope.productNew.NoteForAccounts === undefined) {
                ShowResult("Enter Note for accounts", 'failure');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
            if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
            $scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');

            if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId)
                $scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');
            else
                manualValidation('div_rate', false);

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid && !$scope.invalidPostingDate) {

                manualValidation('div_grnDate', false);
                $scope.productNew.POReturnDate = $scope.GRNDate;

                $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                $scope.product = Object.assign({}, $scope.productNew);
                $scope.product.POId = $scope.POId;
                $scope.product.InventoryReceiveId = $scope.POId1;

                if ($scope.Action === "Save") {
                    $scope.inventoryMaterialListPOnew = [];
                    for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                        if ($scope.inventoryMaterialList[i].check == true) {
                            $scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialList[i]);

                        }

                    }
                    //debugger;
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data:
                        {
                            'entity': $scope.product,
                            'entityMatAndImat': $scope.inventoryMaterialListPOnew,
                            'receiveTaxList': $scope.POMaterialTaxList,
                            'grnBoqList': $scope.SelectedGRNBoqList,
                            'GRNType': 'Save',
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti,
                            'chargesList': $scope.chargesList,
                            'ServicetaxCategoryList': $scope.ServiceTaxList
                        },
                        dataType: 'JSON'
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {

                            ShowResult(response.data.Message, 'success');
                            $scope.Action = "Update";
                            $scope.inventoryMaterialList = [];
                            $scope.inventoryMaterialListPOnew = [];
                            $scope.POMaterialTaxList = [];
                            $scope.SelectedGRNBoqList = [];
                            $scope.chargesList = [];
                            $scope.ServiceTaxList=[];
                            $scope.setTabGRNList(1);
                            $scope.getalldataMaster();
                            $scope.PurchaserReturnListDetails();

                            $scope.POReturnNo = response.data.entity.Id;
                            $scope.productId = response.data.entity.Id;
                            $scope.productNew.PartyName = $scope.product.PartyName;
                            $scope.SaveAdditinalTaxInGRNList($scope.POReturnNo);

                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };

                }
                else if ($scope.Action === "Update") {

                }
            }
        } catch (e) {
            throw e;
        }
    };


    $scope.Delete = function () {
        //debugger;
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0) {
            if (!baseService.isUndefinedOrNull($scope.POReturnNo)) {
                $http({
                    method: 'POST',
                    url: $scope.deleteUrl + $scope.POReturnNo,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getalldataMaster();
                        ClearFields();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
            }
        }
        else
            ShowResult('First delete all line item.', 'failure');
    };

    $scope.Clear = function () {
        //debugger;
        $scope.POReturnNo = "";
        $scope.POId1 = "";
        $scope.inventoryMaterialListPO = [];
        $scope.inventoryMaterialListPOnew = [];
        $scope.GriddataSelected = [];
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.NotificationSettingStatus();
        $scope.Action = "Save";
        // $scope.product = { POId: $scope.product.POId };
        $scope.IsBaseOnDueDateEnable = false;
        $scope.inventoryMaterialListPO = [];
        $scope.chargesListPO = [];
        $scope.inventoryMaterialList = [];
        $scope.chargesList = [];

        $scope.grossTotal = 0;
        $scope.productNew = {
            FixedAssetOrInventory: 'Inventory'
            , PODepended: false
            , AlongwithInvoice: true
            , IsNonCreditable: false
            , BaseCurrencyId: $scope.baseCurrencyId
            , ToCurrencyRate: 1
            , TaxApplicable: null
            , IsTaxApplicable: false
            , IsTaxApplicableChangeable: false
            , PartyType: $scope.partyType
            , PlantId: $window.plantId
            , labelCheckAndApproved: null
            //, POId: $scope.product.POId            
            , GRNDate: $filter("dateFiltering")(Date.now())

        };
        // $scope.POId1 = '';

        baseService.removeErrorClasses();
        //$scope.getToCurrencyRate();
    }

    $scope.GRNBoqList = [];
    $scope.GetGRNBoqList = function (grnRowId) {
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetGRNBOQListForPurchaseReturn?InventoryreceiveDetailId=' + grnRowId,
        }).then(function successCallback(response) {
            $scope.GRNBoqList = response.data;
        });
    };

    $scope.GetGRNBoqPopUp = function (grnRowId, index) {
        $scope.TempIndex = index;
        $scope.TempGrnRowId = grnRowId;
        $scope.GetGRNBoqList(grnRowId);
        angular.element(document.querySelector('#GRNBoqPopUp')).modal('show');

    };
    $scope.GRNBoqPOPopUpClose = function () {
        angular.element(document.querySelector('#GRNBoqPopUp')).modal('hide');
    };
    $scope.SelectedGRNBoqList = [];
    $scope.addToBOQList = function () {
        for (var i = 0; i < $scope.GRNBoqList.length; i++) {
            if ($scope.GRNBoqList[i].ReturnQty > 0) {
                var getRow = $filter("filter")($scope.SelectedGRNBoqList, { "InventoryReceiveDetailId": $scope.GRNBoqList[i].InventoryReceiveDetailId, "BOQDetailId": $scope.GRNBoqList[i].BOQDetailId });
                if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].InventoryReceiveDetailId === $scope.GRNBoqList[i].InventoryReceiveDetailId && getRow[0].BOQDetailId === $scope.GRNBoqList[i].BOQDetailId) {
                    ShowResult("This BOQ Item have already added!", "failure", "GRNBoqPopUp");
                }
                else {
                    $scope.SelectedGRNBoqList.splice(0, 0, $scope.GRNBoqList[i]);
                }
            }
        }
        var tempReturnQty = parseFloat($filter("sumByKey")($filter("filter")($scope.SelectedGRNBoqList, { InventoryReceiveDetailId: $scope.TempGrnRowId }), "ReturnQty")).toFixed(2);
        for (var j = 0; j < $scope.inventoryMaterialList.length; j++) {
            if ($scope.inventoryMaterialList[j].InventoryReceiveDetailId === $scope.TempGrnRowId) {
                $scope.inventoryMaterialList[j].TransactionQty = parseFloat(tempReturnQty).toFixed(2);
            }
        }
        //TODO:taxamount calculation
        angular.element(document.querySelector('#GRNBoqPopUp')).modal('hide');
        $scope.TempIndex = null;
        $scope.TempGrnRowId = null;
        tempReturnQty = 0;
    }
    $scope.tabType = "ForChecking";
    $scope.GriddataMaster = [];
    $scope.getalldataMaster = function () {
        if ($scope.tabType === "ForChecking") {
            $scope.tabType = "ForChecking";
        }
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetListPurchaseReturnData?tabType=' + $scope.tabType,
        }).then(function successCallback(response) {
            // url: $scope.getListUrl1,
            $scope.GriddataMaster = response.data;
            //entrydata = copy(searchdata);
        });
    };
    $scope.getalldataMaster();



    $scope.recorddoubleclickformodify = function ($event) {
        ClearFields();
        var x = $event;
        var Id = x.data.Id;
        //debugger;
        $scope.POId1 = x.data.InventoryReceiveId;
        $scope.POID = x.data.POID;
        $scope.POReturnNo = x.data.Id;
        $scope.product = $scope.products[$scope.index];
        $scope.productNew = Object.assign({}, $scope.product);
        $scope.productNew = x.data;
        $scope.GRNDate = x.data.GRNDate1;
        $scope.productNew.CheckedBy = x.data.CheckedBy;
        getPartyPlantList();
        getInventoryMaterialListModify(Id);
        getPurchaseServiceChargeList(Id);
        $scope.getTaxCodeByTaxYearWithhold($scope.GRNDate);
        $scope.productId = Id;
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate')
                    $scope.IsBaseOnDueDateEnable = true;
                else
                    $scope.IsBaseOnDueDateEnable = false;
        }

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();

        //angular.element(document.querySelector('#POPopUp')).modal('hide');
    }
    function getInventoryMaterialListModify(inveReveiveId) {
        $scope.masterId5 = inveReveiveId;
        $http.get('Products/GoodsReceiveNote/GetInventoryMaterialListForPurchaseReturnModify?inveReveiveId=' + inveReveiveId + '&POID=' + $scope.POID)
            .then(function (response) {
                $scope.inventoryMaterialList = [];
                $scope.inventoryMaterialList = response.data;
                $scope.inventoryMaterialListPO = response.data;
                $scope.POIDs = $scope.inventoryMaterialList.POId;
                //$scope.productNew.CheckedBy = $scope.inventoryMaterialList[0].CheckedBy;
                $scope.productNew.PODate = $scope.inventoryMaterialList[0].AddedDate;
                checkSameValueInColumnList($scope.inventoryMaterialList, 'TransactionUoM');
                getGrossAmount($scope.inventoryMaterialList, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
                //$scope.GetMaterialTaxData();
                $scope.GetPOMaterialTaxDataModify();
                $scope.GetAdvanceTaxInfo($scope.POReturnNo);
                $scope.TotalSumAfterTCS();
            });
    }



    $scope.GRN = "";
    //$scope.tab = 1;
    $scope.tabGL = 1;
    //debugger;
    $scope.tabType = "ForChecked";
    $scope.setTabGRNList = function (newTab) {

        $scope.tabType = "ForChecking";
        $scope.getalldataMaster();
        $scope.tabGL = newTab;
    };
    $scope.isSetGRNList = function (tabNum) {
        return $scope.tabGL === tabNum;
        //$scope.GRN = 1;

    };
    $scope.setTabCheckedHoldReject = function (newTab) {
        $scope.tabType = "CheckedHoldReject";
        $scope.getalldataMaster();
        $scope.PurchaserReturnListDetails();
        $scope.tabGL = newTab;

    };
    $scope.isSetCheckedHoldReject = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 2;

    };
    $scope.setTabNotApprovedChecked = function (newTab) {
        $scope.tabType = "Checked";
        $scope.getalldataMaster();
        $scope.PurchaserReturnListDetails();
        $scope.tabGL = newTab;

    };
    $scope.isSetNotApprovedChecked = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 3;

    };
    $scope.setTabApprovedHoldReject = function (newTab) {
        $scope.tabType = "ApprovedHoldReject";
        $scope.getalldataMaster();
        $scope.PurchaserReturnListDetails();
        $scope.tabGL = newTab;

    };
    $scope.isSetApprovedHoldReject = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 4;

    };



    $scope.setTabApprovedNotPosted = function (newTab) {
        $scope.tabType = "Approved";
        $scope.getalldataMaster();
        $scope.PurchaserReturnListDetails();
        $scope.tabGL = newTab;

    };
    $scope.isSetApprovedNotPosted = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 5;

    };


    $scope.setTabPosted = function (newTab) {
        $scope.tabGL = newTab;
        $scope.tabType = "Posted";
        $scope.PurchaserReturnListDetails();
        $scope.getalldataMaster();

    };
    $scope.isSetPosted = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 6;

    };

    $scope.getReceiveTaxList = function (data, flag, index, Id) {
        //debugger;
        $scope.receiveTaxList = [];
        if ($scope.Action === 'Update') {
            $scope.taxAbleAmnt = data.TrnAmount;
            $scope.percentageColumn = flag;

            for (var i = 0; i < $scope.POMaterialTaxList.length; i++) {
                if ($scope.POMaterialTaxList[i].PurchaseReturnDetailId === Id) {
                    $scope.receiveTaxList.push($scope.POMaterialTaxList[i]);

                }
            }
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
        }
        else {
            $scope.taxAbleAmnt = data.TrnAmount;
            $scope.percentageColumn = flag;


            for (var i = 0; i < $scope.POMaterialTaxList.length; i++) {
                if ($scope.POMaterialTaxList[i].InventoryReceiveDetailId === Id) {
                    $scope.receiveTaxList.push($scope.POMaterialTaxList[i]);

                }
            }
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');

        }


    };


    $scope.index = -1;
    $scope.staus = true;
    $scope.enableid = true;
    $scope.Change = function (event, index, x) {
        //debugger;
        if (baseService.isUndefinedOrNull(x.TransactionQty)) {
            ShowResult('Enter the current qty', 'failure');
        }
        else {
            if (event.currentTarget.checked) {

            }
            else {

                x.enableid = true;
                x.POClosStatus = false;
                x.TransactionQty = "";
                x.Balance = (x.GRNReceived - (x.OtherReturned + x.BaseIssueQty));//parseFloat(x.POQty - x.GRNRcvQty).toFixed(2);
            }
        }

    }
    $scope.valuePassInDelModal = function (x) {
        //debugger;
        $scope.InventoryReceiveDetailId = x.InventoryReceiveDetailId;
        $scope.InventoryServiceId = x.InventoryServiceId;
        $scope.InventoryMaterial = x.InventoryMaterialId;
        $scope.TransactionQty = x.TransactionQty;

        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
    };

    $scope.detailDelete = function () {

        try {
            $http({
                method: 'POST',
                url: $scope.path + 'DeletePurchaseReturnRow1',
                data:
                {
                    'PurchaseReturnDetailId': $scope.InventoryReceiveDetailId,
                    'inventoryReceiveDetailId': $scope.InventoryServiceId,
                    'InventoryMaterial': $scope.InventoryMaterial,
                    'Trasantionqty': $scope.TransactionQty
                }
                // dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.id = null;
                    getInventoryMaterialListModify($scope.POReturnNo);
                    // $scope.getDataList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };



    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = " GoodsReceiveNote/PurchaseReturnReport?grnId=" + data.Id;
    };


    //#region GRN Detail
    $scope.lst = [];
    $scope.GRNListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/GoodsReceiveNote/GRNDetailsData'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.lst = response.data;

        });
    }
    $scope.GRNListDetails();


    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("InventoryReceiveId", "equal", parseInt(filteredData), true).take(105));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: ["MaterialGroupName", "MaterialName", "Article", "SKU1", "SKU2", "SKU3", "MaterialDetail", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalMaterialTranAmount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }


    $scope.PurchaserReturn1st = [];
    $scope.PurchaserReturnListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/GoodsReceiveNote/PurchaseReturnDetailsData'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.lst1 = response.data;

        });
    }
    $scope.PurchaserReturnListDetails();


    $scope.data1 = $scope.PurchaserReturn1st;
    $scope.detailTemppurchaseReturn = "#tabGridpurchaseContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgridpurchaseReturn = function detailGridData(e) {
        //debugger;
        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst1).executeLocal(ej.Query().where("PurchaseReturnId", "equal", parseInt(filteredData), true).take(500));
        e.detailsElement.find("#detailGridPR").ejGrid({
            dataSource: data,
            columns: ["MaterialName", "Article", "SKU1", "SKU2", "SKU3", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "TotalMaterialTranAmount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    //#endregion





    $scope.getDataList = function () {
        baseService.init($scope.getListUrl, null, null, "DESC", "GRNDate", "PartyName");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.products = [];
                    $scope.products = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };
    $scope.getDataList();
    $http({
        method: 'GET',
        url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });

    //InventoryReceive Model


    $scope.searchByList = [
        {
            value: 'PartyCode'
            , name: 'Vendor Code'
        },
        {
            value: 'PartyName'
            , name: 'Vendor Name'
        },
        {
            value: 'PartyAccountGroupName'
            , name: 'Account Group'
        },
        {
            value: 'Id'
            , name: 'GRN No'
        },
        {
            value: 'GRNDate'
            , name: 'GRN Date'
        },
        {
            value: 'DocRefNo'
            , name: 'Vendor DocRefNo'
        },
        {
            value: 'InvoiceNo'
            , name: 'Invoice No'
        },
        {
            value: 'InvoiceDate'
            , name: 'Invoice Date'
        }
    ];

    $scope.partySearchByList = [
        {
            'name': $scope.partyType + ' Code',
            'value': 'Code'
        },
        {
            'name': $scope.partyType + ' Name',
            'value': 'UserName'
        },
        {
            'name': 'Account Group',
            'value': 'PartyAccountGroupName'
        },
        {
            'name': 'Country',
            'value': 'CountryName'
        },
        {
            'name': 'State',
            'value': 'StateName'
        },
        {
            'name': 'Currency',
            'value': 'CurrencyCode'
        }
    ];




    $scope.productNew = Object.assign({}, $scope.product);

    addressService.getCountryCbo(function (result) {
        $scope.countryList = result;
    });

    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
    }).then(function successCallback(response) {
        $scope.baseCurrencyId = response.data[0].Value;
        $scope.productNew.BaseCurrencyId = response.data[0].Value;
        factoryService.getCurrencyPrecision($scope.baseCurrencyId);
    });

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = result;
    });

    $http.get('accounts/OpeningBalance/GetACCCutOffDate')
        .then(function (response) {
            if (response.data !== null && !baseService.isUndefinedOrNull(response.data.CutOffDate)) {
                $scope.productNew.CutOffDate = response.data.CutOffDate;
                $('#cutOffDate').datepicker('setStartDate', new Date($scope.productNew.CutOffDate));
            }
            else
                ShowResult('Cut Off date not found!', 'failure');
        });



    $scope.Get = function (index) {

        $scope.index = index;
        $scope.product = $scope.products[$scope.index];
        $scope.productNew = Object.assign({}, $scope.product);
        // $scope.productNew.GRNDate = data.GRNDate;
        getPartyPlantList();
        getInventoryMaterialList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
        $scope.productId = $scope.productNew.Id;
        //$scope.getToCurrencyRate();
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate')
                    $scope.IsBaseOnDueDateEnable = true;
                else
                    $scope.IsBaseOnDueDateEnable = false;
        }
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };


    $scope.changeAllInvoice = function () {
        $scope.productNew.InvoiceNo = null;
        $scope.productNew.InvoiceDate = null;
    };

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.productNew.PartyCode = party.Code;
            $scope.productNew.PartyName = party.UserName;
            $scope.productNew.PartyId = party.Id;
            $scope.productNew.PaymentTermId = party.PaymentTermId;
            $scope.productNew.CurrencyId = party.CurrencyId;
            $scope.IsBaseOnDueDateEnable = false;
            $scope.productNew.BaseOnDueDate = null;
            $scope.productNew.BaseNoOfDays = null;
            $scope.productNew.MatureDate = null;

            $scope.productNew.TaxApplicable = party.TaxApplicable;
            $scope.productNew.IsTaxApplicableChangeable = party.IsTaxApplicableChangeable;
            if (party.TaxApplicable === 'Mandatory')
                $scope.productNew.IsTaxApplicable = true;
            else
                $scope.productNew.IsTaxApplicable = false;

            if (!baseService.isUndefinedOrNull($scope.productNew.DocDate))
                $scope.changePaymentTerm();
            getPartyPlantList();
        }
        $scope.hidePartyPopUp();
    };


    function getPartyPlantListPO() {
        //debugger;
        $scope.plantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.partyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.IsDefault) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = item.Value;
                    $scope.productNew.InvoicingByAddress = item.Address1;
                    $scope.productNew.DeliveryByAddress = item.Address1;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = item.StateName;
                    $scope.productNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }
    $scope.getToCurrencyRate = function () {
        if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
            $scope.productNew.ToCurrencyRate = 1;
            return;
        }
        $http.get($scope.path + 'GetToCurrencyRate?currencyId=' + $scope.productNew.CurrencyId + '&baseCurrencyId=' + $scope.productNew.BaseCurrencyId + '&docDate=' + $filter('dateFiltering')($scope.productNew.DocDate))
            .then(function (response) {
                if (parseFloat(response.data) === 0)
                    $scope.productNew.ToCurrencyRate = 1;
                else
                    $scope.productNew.ToCurrencyRate = response.data;
            });
    };
    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closeInvoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
    };
    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateName;
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = state;
                $scope.productNew.InvoicingGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.productNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.productNew.DeliveryState = state;
                $scope.productNew.DeliveryGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.productNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = null;
                $scope.productNew.InvoicingGSTIN = null;
                return $scope.productNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.productNew.DeliveryState = null;
                $scope.productNew.DeliveryGSTIN = null;
                return $scope.productNew.DeliveryByAddress = null;
            }
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #region Details
    $scope.detailModelSave = {
        Id: null
        , CountryId: null
        , InventoryReceiveId: $scope.productNew.Id
        , MaterialStorageId: $scope.productNew.MaterialStorageId
        , CurrencyName: angular.element("#currency :selected").text()
        , CurrencyId: $scope.productNew.CurrencyId
        , BaseCurrencyId: $scope.baseCurrencyId
        , DocDate: $scope.productNew.DocDate
        , InventoryMaterialId: null
        , MaterialMasterId: null
        , MaterialMasterName: null
        , ArticleId: null
        , ArticleName: null
        , MaterialType: null
        , OurStyleName: null
        , Description: null
        , MaterialGroupMasterName: null
        , ProductMasterName: null
        , IsOurStyleRequired: false
        , IsProductMstRequired: false

        , FirstCharacteristicsId: null
        , FirstCharacteristicsValueId: null

        , SecondCharacteristicsId: null
        , SecondCharacteristicsValueId: null

        , ThirdCharacteristicsId: null
        , ThirdCharacteristicsValueId: null

        , TransactionQty: null
        , TransactionUoMId: null
        , TransactionRate: 0
        , TransactionAmount: 0
        , BaseQty: null
        , BaseUOMId: null
        , BaseUoM: null
        , BaseUoMFactor: null

        , TotalQty: null
        , TotalAmount: 0
        , TotalTaxAmount: 0
        , AvgRate: null
        , ToCurrencyRate: $scope.productNew.ToCurrencyRate
        , IsNonCreditable: $scope.productNew.IsNonCreditable
        , IsOriginApplicable: false
    };
    $scope.businessProcesses = '';//"BP.BusinessProcessName IN('MaintenanceSpare','BOM','WetProcess','Consumable')";
    $scope.detailPopUp = function () {
        $scope.detailModel = {
            Id: null
            , CountryId: null
            , InventoryReceiveId: $scope.productNew.Id
            , MaterialStorageId: $scope.productNew.MaterialStorageId
            , CurrencyName: angular.element("#currency :selected").text()
            , CurrencyId: $scope.productNew.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.productNew.DocDate
            , InventoryMaterialId: null
            , MaterialMasterId: null
            , MaterialMasterName: null
            , ArticleId: null
            , ArticleName: null
            , MaterialType: null
            , OurStyleName: null
            , Description: null
            , MaterialGroupMasterName: null
            , ProductMasterName: null
            , IsOurStyleRequired: false
            , IsProductMstRequired: false

            , FirstCharacteristicsId: null
            , FirstCharacteristicsValueId: null

            , SecondCharacteristicsId: null
            , SecondCharacteristicsValueId: null

            , ThirdCharacteristicsId: null
            , ThirdCharacteristicsValueId: null

            , TransactionQty: null
            , TransactionUoMId: null
            , TransactionRate: 0
            , TransactionAmount: 0
            , BaseQty: null
            , BaseUOMId: null
            , BaseUoM: null
            , BaseUoMFactor: null

            , TotalQty: null
            , TotalAmount: 0
            , TotalTaxAmount: 0
            , AvgRate: null
            , ToCurrencyRate: $scope.productNew.ToCurrencyRate
            , IsNonCreditable: $scope.productNew.IsNonCreditable
            , IsOriginApplicable: false
        };
        $scope.clearCharNames();
        angular.element(document.querySelector('#detailPopUp')).modal('show');
    };
    $scope.closeDetaiPopUp = function () {
        $scope.detailModel = {};
        $scope.taxCategoryList = [];
        removeValidationMsg();
        angular.element(document.querySelector('#detailPopUp')).modal('hide');
    };

    $scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];
    //$scope.setMaterialMasterData
    $scope.selectMaterialByType = function (ob) {
        $scope.detailModel.MaterialMasterId = ob.Id;
        $scope.detailModel.MaterialMasterName = ob.UserName;
        $scope.detailModel.BaseUOMId = ob.BaseUOMId;
        $scope.detailModel.BaseUoM = ob.BaseUoM;
        $scope.detailModel.OurStyleName = ob.OurStyleName;
        $scope.detailModel.MaterialGroupMasterName = ob.MaterialGroupMasterName;
        $scope.detailModel.ProductMasterName = ob.ProductMasterName;
        $scope.detailModel.IsOurStyleRequired = ob.IsOurStyleRequired;
        $scope.detailModel.IsProductMstRequired = ob.IsProductMstRequired;
        $scope.detailModel.TransactionUoMId = ob.BaseUOMId;
        $scope.detailModel.ArticleId = null;
        $scope.detailModel.ArticleName = null;
        $scope.detailModel.FirstCharacteristicsValueId = null;
        $scope.detailModel.SecondCharacteristicsValueId = null;
        $scope.detailModel.ThirdCharacteristicsValueId = null;
        $scope.detailModel.IsOriginApplicable = ob.IsOriginApplicable;
        $scope.detailModel.CountryId = null;

        $scope.hasArticle = ob.HasAttribute;
        $scope.hasSku = ob.WithSKU;
        $scope.clearCharNames();
        if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
        if (ob.WithSKU) $scope.getCharacteristicsList(ob.Id);

        getTaxCategoryList(ob.HSNCodeId);
        var mmId = []; mmId.push(ob.Id);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
            $scope.uoMList = result;
            //$scope.detailModel.BaseUOMId = $filter("filter")($scope.uoMList, { IsBaseUom: 1 })[0].Value;
        });
        manualValidation('div_mm', false);
        manualValidation('div_country', false);
        $scope.closeMaterialMasterbyTypePopUp();
    };

    $scope.selectarticle = function (ob) {
        try {
            $scope.detailModel.ArticleId = ob.Id;
            $scope.detailModel.ArticleName = ob.StandardName;
            manualValidation('div_ar', false);
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.setCharData = function (data) {
        $scope[$scope.charValueSearchFor].CharacteristicsValueId = data.CharacteristicsValueId;
        $scope[$scope.charValueSearchFor].FreeText = data.UserName;
        $scope[$scope.charValueSearchFor].FlagDisable = $scope.isSearch;
        angular.element(document.querySelector('#searchcharactervaluepopup')).modal('hide');
    };

    $scope.detailSave = function () {
        try {
            $scope.validation();
            $scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
            $scope.detailModel.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
            $scope.detailModel.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
            $scope.detailModel.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
            $scope.detailModel.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;
            $scope.detailModel.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
            $scope.detailModel.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;

            for (var i = 0; i < baseService.arrayLength($scope.inventoryMaterialList); i++) {
                if ($scope.detailModel.MaterialMasterId === $scope.inventoryMaterialList[i].MaterialMasterId &&
                    $scope.detailModel.ArticleId === $scope.inventoryMaterialList[i].ArticleId &&
                    $scope.detailModel.FirstCharacteristicsId === $scope.inventoryMaterialList[i].FirstCharacteristicsId &&
                    $scope.detailModel.FirstCharacteristicsValueId === $scope.inventoryMaterialList[i].FirstCharacteristicsValueId &&
                    $scope.detailModel.SecondCharacteristicsId === $scope.inventoryMaterialList[i].SecondCharacteristicsId &&
                    $scope.detailModel.SecondCharacteristicsValueId === $scope.inventoryMaterialList[i].SecondCharacteristicsValueId &&
                    $scope.detailModel.ThirdCharacteristicsId === $scope.inventoryMaterialList[i].ThirdCharacteristicsId &&
                    $scope.detailModel.ThirdCharacteristicsValueId === $scope.inventoryMaterialList[i].ThirdCharacteristicsValueId) {
                    return ShowResult('This material already received');
                }
            }

            $http({
                method: 'POST',
                url: $scope.detailSaveUrl,
                data: {
                    entity: $scope.detailModel
                    , taxCategoryList: $scope.taxCategoryList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'detailPopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'detailPopUp');
                    $scope.detailModel.Id = null;
                    $scope.detailModel = {
                        InventoryReceiveId: $scope.productNew.Id
                        , MaterialStorageId: $scope.productNew.MaterialStorageId
                        , CurrencyName: angular.element("#currency :selected").text()
                        , CurrencyId: $scope.productNew.CurrencyId
                        , BaseCurrencyId: $scope.baseCurrencyId
                        , DocDate: $scope.productNew.DocDate
                        , TotalAmount: 0
                        , TransactionAmount: 0
                        , ToCurrencyRate: $scope.productNew.ToCurrencyRate
                        , IsNonCreditable: $scope.productNew.IsNonCreditable
                        , IsOriginApplicable: false
                    };
                    $scope.taxCategoryList = [];
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                    $scope.clearCharNames();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'detailPopUp');
            };
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };
    $scope.GrnRequisitionAllocationSave = function () {

        //debugger;

        try {

            $scope.GetListForMasterOrdernew = [];
            for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {

                if ($scope.GetListForMasterOrder[i].CheckedStatus === true) {
                    $scope.GetListForMasterOrdernew.push($scope.GetListForMasterOrder[i]);
                }

            }
            // if ($scope.invalid) {
            if ($scope.Action1 === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Products/GoodsReceiveNote/GrnRequisitionAllocationSave',
                    data: {
                        entity: $scope.GetListForMasterOrdernew
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
                    else {
                        ShowResult(response.data.Message, 'success', 'ListOfRequisition');
                        $scope.Action1 = "Update";
                        $scope.GetListForMasterOrder = [];
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
                };

            }
            else if ($scope.Action1 === "Update") {
                $http({
                    method: 'POST',
                    url: 'Products/GoodsReceiveNote/GrnRequisitionAllocationSave',
                    data: {
                        entity: $scope.GetListForMasterOrdernew
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
                    else {
                        ShowResult(response.data.Message, 'success', 'ListOfRequisition');

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
                };

            }
            //}
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };



    $scope.validation = function () {
        $scope.modelValidation('div_mm', 'detailModel', 'MaterialMasterName', 'Material Master');
        if ($scope.hasArticle) $scope.modelValidation('div_ar', 'detailModel', 'ArticleName');
        $scope.manualValidationAddRemove('div_qty', 'detailModel', 'TransactionQty');
        $scope.modelValidation('div_qty', 'detailModel', 'TransactionUoMId', 'UoM is required');
        if ($scope.detailModel.TransactionAmount === 0)
            throw manualValidation('div_tamnt', true, 'Total amount is required.');
        $scope.manualValidationAddRemove('div_tamnt', 'detailModel', 'TransactionAmount');
        if ($scope.detailModel.IsOriginApplicable)
            $scope.manualValidationAddRemove('div_country', 'detailModel', 'CountryId');

        var isSku = false;
        if ($scope.hasSku) {
            if (!baseService.isUndefinedOrNull($scope.char1.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char1.IsMandatory, $scope.char1.FreeText);
            }
            else if (!baseService.isUndefinedOrNull($scope.char2.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char2.IsMandatory, $scope.char2.FreeText);
            }
            else if (!baseService.isUndefinedOrNull($scope.char3.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char3.IsMandatory, $scope.char3.FreeText);
            }
            if (isSku) throw ShowResult('Please insert SKU.', 'failure', 'detailPopUp');
        }
    };
    $scope.manualValidationAddRemove = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else if (isNaN($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };
    //manualDateValidation
    $scope.modelValidation = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };




    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }
    function getTaxCategoryList(hsnCodeId) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
        });
    }

    $scope.calculateTaxCategory = function () {
        $scope.detailModel.TotalTaxAmount = 0;
        var tQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) ? 0 : parseFloat($scope.detailModel.TransactionQty);
        var tAmount = baseService.isUndefinedOrNull($scope.detailModel.TransactionAmount) ? 0 : parseFloat($scope.detailModel.TransactionAmount);
        if (tQty > 0 && tAmount > 0)
            $scope.detailModel.TransactionRate = tAmount / tQty;
        else
            $scope.detailModel.TransactionRate = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.detailModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.detailModel.TotalTaxAmount)) $scope.detailModel.TotalTaxAmount = 0;
    };
    $scope.sumTaxAmount = function () {
        $scope.detailModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
    };




    $scope.getTotalReceiveTaxList = function (amount, flag) {
        $scope.taxAbleAmnt = amount;
        $scope.percentageColumn = flag;
        $http({
            method: 'GET',
            url: $scope.path + 'GetTotalReceiveTaxList?receiveId=' + $scope.productNew.Id
        }).then(function (response) {
            $scope.receiveTaxList = response.data;
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
        });
    };
    $scope.closeReceiveTaxPopUp = function () {

        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }


    $scope.closeReceiveTaxPopUpValue = function (x) {
        //debugger;
        if ($scope.Action === 'Save') {
            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                var row = $filter('filter')($scope.new, { 'PODetailsID': $scope.inventoryMaterialListPO[i].PODetailsID });
                if (row.length != 0) {
                    if ($scope.inventoryMaterialListPO[i].PODetailsID === row[0].PODetailsID) {
                        $scope.inventoryMaterialListPO[i].ShortageRate = row[0].ShortageRate;
                        $scope.inventoryMaterialListPO[i].ShortageValue = row[0].ShortageValue;
                        $scope.inventoryMaterialListPO[i].RejectionRate = row[0].RejectionRate;
                        $scope.inventoryMaterialListPO[i].RejectionValue = row[0].RejectionValue;
                        $scope.inventoryMaterialListPO[i].RejectionClamRate = row[0].RejectionClamRate;
                    }
                    angular.element(document.querySelector('#ValueSet')).modal('hide');
                }
                else {
                    angular.element(document.querySelector('#ValueSet')).modal('hide');
                }

            }
            angular.element(document.querySelector('#ValueSet')).modal('hide');
        }
        else {

            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                var row = $filter('filter')($scope.new1, { 'PODetailsID': $scope.inventoryMaterialList[i].PODetailsID });
                if (row.length != 0) {
                    if ($scope.inventoryMaterialList[i].PODetailsID === row[0].PODetailsID) {
                        $scope.inventoryMaterialList[i].ShortageRate = row[0].ShortageRate;
                        $scope.inventoryMaterialList[i].ShortageValue = row[0].ShortageValue;
                        $scope.inventoryMaterialList[i].RejectionRate = row[0].RejectionRate;
                        $scope.inventoryMaterialList[i].RejectionValue = row[0].RejectionValue;
                        $scope.inventoryMaterialList[i].RejectionClamRate = row[0].RejectionClamRate;
                    }
                    angular.element(document.querySelector('#ValueSet')).modal('hide');
                }
                else {
                    angular.element(document.querySelector('#ValueSet')).modal('hide');
                }
                angular.element(document.querySelector('#ValueSet')).modal('hide');
            }
            angular.element(document.querySelector('#ValueSet')).modal('hide');
        }

    }


    function removeValidationMsg() {
        CloseModalShowResult();
        $scope.clearCharNames();
        manualValidation('div_mm', false);
        manualValidation('div_ar', false);
        manualValidation('div_qty', false);
        manualValidation('div_qty', false);
        manualValidation('div_rate', false);
    }
    function getGrossAmount(list, key1, key2, key3, fieldName) {
        $scope[fieldName] = 0;
        for (var t = 0; t < baseService.arrayLength(list); t++) {
            $scope[fieldName] += parseFloat(list[t][key1]);// + parseFloat(list[t][key2]) + parseFloat(list[t][key3]);
        }
    }

    // #endregion Details

    // #region Payment Term
    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getvendorcbo'
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });

    $scope.changePaymentTerm = function () {
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            $scope.productNew.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.productNew.BaseNoOfDays = paymentTerm.NoOfDay;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate') {
                    $scope.productNew.BaseOnDueDate = $filter('dateFiltering')($scope.productNew.DocDate);
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else {
                    $scope.productNew.BaseOnDueDate = null;
                    $scope.IsBaseOnDueDateEnable = false;
                }
            $scope.getMatureDate($scope.productNew.BaseOnDueDate, $scope.productNew.BaseNoOfDays);
        }
    };
    $scope.getMatureDate = function (date, days) {
        if (baseService.isUndefinedOrNull(date)) return $scope.productNew.MatureDate = null;
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.productNew.MatureDate = $filter('date')(date, 'dd-MMM-yyyy');
    };
    // #endregion Payment Term

    // #region Service
    $scope.serviceChargePopUp = function () {
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0)
            return ShowResult('Without material charges not aplicable.');
        $scope.serviceModel = {
            Id: null
            , ServiceMasterId: null
            , InventoryReceiveId: $scope.productNew.Id
            , CurrencyName: angular.element("#currency :selected").text()
            , CurrencyId: $scope.productNew.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.productNew.DocDate
            , TransactionAmount: 0
            , BaseAmount: 0
            , TotalTaxAmount: 0
            , ToCurrencyRate: $scope.productNew.ToCurrencyRate
            , IsNonCreditable: $scope.productNew.IsNonCreditable
        };
        angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
    };
    $http.get('Setups/CompanyServiceMaster/GetCboList')
        .then(function (response) {
            $scope.serviceList = response.data;
        });
    $scope.closeServiceChargePopUp = function () {
        $scope.serviceModel = {};
        $scope.receiveTaxList = [];
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    };
    $scope.changeService = function () {
        if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
        getTaxCategoryList(hsnCodeId);
    };

    $scope.calculateSvcTaxCategory = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
    };
    $scope.sumSvcTaxAmount = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
    };

    $scope.serviceSave = function () {
        try {
            $scope.manualValidationAddRemove('div_svc', 'serviceModel', 'ServiceMasterId');
            $scope.manualValidationAddRemove('div_svcRate', 'serviceModel', 'TransactionAmount', 'Amount');

            $http({
                method: 'POST',
                url: $scope.sreviceSaveUrl,
                data: {
                    entity: $scope.serviceModel
                    , taxCategoryList: $scope.taxCategoryList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'serviceChargePopUp');
                    $scope.serviceModel = {
                        Id: null
                        , ServiceMasterId: null
                        , InventoryReceiveId: $scope.productNew.Id
                        , CurrencyName: angular.element("#currency :selected").text()
                        , CurrencyId: $scope.productNew.CurrencyId
                        , BaseCurrencyId: $scope.baseCurrencyId
                        , DocDate: $scope.productNew.DocDate
                        , TransactionAmount: 0
                        , BaseAmount: 0
                        , TotalTaxAmount: 0
                        , ToCurrencyRate: $scope.productNew.ToCurrencyRate
                        , IsNonCreditable: $scope.productNew.IsNonCreditable
                    };
                    $scope.taxCategoryList = [];
                    getServiceChargeList($scope.productNew.Id);
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
            };
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };

    $scope.delModal = function (id) {
        $scope.id = id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removePopUp')).modal('show');
    };
    $scope.serviceDelete = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.sreviceDeleteUrl + $scope.id
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.id = null;
                    getServiceChargeList($scope.productNew.Id);
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };



    function getServiceChargeList(inveReveiveId) {
        $scope.masterId12 = inveReveiveId;
        //debugger;
        $http.get($scope.path + 'GetServiceChargeList?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.chargesList = [];
                $scope.chargesList = response.data;
                $scope.getServiceTaxList();
                $scope.Change2();

            });

    }


    function getTCSData(inveReveiveId) {
        $scope.masterId12 = inveReveiveId;
        //debugger;
        $http.get($scope.path + 'getTCSData?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.advanceTax.TaxCodeId = response.data[0].TaxCodeId;
                $scope.advanceTax.TaxCategoryId = response.data[0].TaxCategoryId;
                $scope.advanceTax.ValueOfFixed = response.data[0].Percentage;
            });

    }
    function getPurchaseServiceChargeList(inveReveiveId) {
        $scope.masterId12 = inveReveiveId;
        //debugger;
        $http.get($scope.path + 'GetPurchaseReturnServiceChargeList?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.chargesList = [];
                $scope.chargesList = response.data;
                $scope.getServiceTaxList();

            });
    }
    // #endregion Service

    $scope.inventoryReceiveReport = function (id, reportFormat) {
        if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + id + '&plantId=' + $scope.productNew.PlantId);
    };
    $scope.Griddata = [];
    $scope.getalldata = function () {
        //debugger;
        var PoType = 'POByReq';
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/GetListOfPO?PoType=' + PoType,
        }).then(function successCallback(response) {
            $scope.Griddata = response.data;
            $scope.productNew.GRNDate = $filter("dateFiltering")(Date.now());
            //entrydata = copy(searchdata);
        });
    };








    // #region shakawat
    $scope.POPopUp = function () {
        $scope.getalldata();

        angular.element(document.querySelector('#POPopUp')).modal('show');

    };


    $scope.POPopUpClose = function () {
        angular.element(document.querySelector('#POPopUp')).modal('hide');
    };
    $scope.GriddataSelected = [];
    $scope.recorddoubleclick = function ($event) {

        $scope.Griddatatemp = [];
        $scope.Griddatatemp1 = [];
        var partyId = null;
        $scope.tempList = [];
        for (var j = 0; j < $scope.getListPOByReqG.length; j++) {
            if ($scope.getListPOByReqG[j].Active === true) {
                $scope.tempList.push($scope.getListPOByReqG[j]);
            }
        }
        var flagTemp = false;
        if ($scope.tempList.length > 0) {
            for (var k = 0; k < $scope.tempList.length; k++) {
                if ($scope.tempList[k].PartyId != $scope.tempList[0].PartyId) {// && $scope.tempList[k].CurrencyId != $scope.tempList[0].CurrencyId
                    flagTemp = true;
                    // angular.element(document.querySelector('#POPopUp')).modal('hide');
                    ShowResult('Please select Same vendor', 'POPopUp');
                    return;

                }

            }
        }


        if (flagTemp == false) {

            var gridObj = $("#Grid").data("ejGrid");
            var $event = gridObj.getSelectedRecords()[0];
            var x = $event;
            var Id = x.Id;
            $scope.productNew = x;
            $scope.productId = "";
            $scope.POId = x.Id;
            $scope.productNew.GRNDate = $filter("dateFiltering")(Date.now());
            //$scope.product.POId = $scope.POId;
            var id1 = "''";
            for (var i = 0; i < $scope.getListPOByReqG.length; i++) {
                if ($scope.getListPOByReqG[i].Active === true) {
                    id1 += ",'" + $scope.getListPOByReqG[i].Id + "'";
                }
            }

            getPartyPlantList();
            //getPartyPlantEditList();
            GetInventoryMaterialListByPO(id1);
            getServiceChargeListPO(id1);
            $scope.GriddataSelected = [];
            for (var x = 0; x < $scope.getListPOByReqG.length; x++) {

                if ($scope.getListPOByReqG[x].Active === true) {
                    $scope.GriddataSelected.push($scope.getListPOByReqG[x]);
                }
            }

            $scope.POPopUpClose();
            if (!$rootScope.isCollapsed) $rootScope.toggle();


        }


    }

    $scope.GetSavedPOListNew = [];
    $scope.GetSavedPOList1 = function (Id) {
        //debugger;
        var PoType = 'PO';
        $scope.GriddataSelected = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetSavedPOList1?GRNId=' + Id,
        }).then(function successCallback(response) {
            //$scope.GetSavedPOListNew = [];
            $scope.GetSavedPOListNew = response.data;
            for (var i = 0; i < $scope.GetSavedPOListNew.length; i++) {

                $scope.GriddataSelected.push($scope.GetSavedPOListNew[i]);
            }

        });
    };

    $scope.POPopUpGateEntry = function () {
        $scope.getalldataGateEntry();
        angular.element(document.querySelector('#POPopUpGateEntry')).modal('show');
    };
    $scope.POPopUpCloseGateEntry = function () {
        angular.element(document.querySelector('#POPopUpGateEntry')).modal('hide');
    };

    $scope.GriddataGateEntry = [];
    $scope.getalldataGateEntry = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/GetListOfPOGateEntry?partyCode=' + $scope.productNew.PartyId,
        }).then(function successCallback(response) {
            $scope.GriddataGateEntry = response.data;
            //entrydata = copy(searchdata);
        });
    };
    $scope.recorddoubleclickGateEntry = function ($event) {
        var x = $event;
        var Id = x.data.Id;
        $scope.productNew.GateEntryNo = x.data.Id;
        $scope.productNew.EntryDate = x.data.EntryDate;

        $scope.POPopUpCloseGateEntry();
    }



    // Load tax with Material Data
    $scope.getReceiveTaxListPO = function (data, flag, index, Id) {
        //debugger;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.receiveTaxList = [];
        if (data.POMaterialTaxList.length > 0) {
            $scope.HSNCode = data.POMaterialTaxList[0].HSNCode;
            $scope.receiveTaxList = data.POMaterialTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');

    };

    $scope.getReceiveTaxListPOValueSet = function (data, flag, index, Id) {

        if ($scope.Action === 'Save') {
            $scope.ShortageRate = '';
            $scope.ShortageValue = '';
            $scope.RejectionRate = '';
            $scope.RejectionValue = '';
            $scope.RejectionClamRate = '';
            $scope.MaterialGroupMasterName = data.MaterialGroupMasterName;
            $scope.UserName = data.UserName;
            $scope.StandardName = data.StandardName;
            $scope.FirstCharacteristicsValue = data.FirstCharacteristicsValue;
            $scope.SecondCharacteristicsValue = data.SecondCharacteristicsValue;
            $scope.ThirdCharacteristicsValue = data.ThirdCharacteristicsValue;

            $scope.TransactionRate = data.TransactionRate;
            $scope.ShortageQty = data.ShortageQty;
            $scope.RejectionQty = data.RejectionQty;

            $scope.PODetailsID = data.PODetailsID;
            $scope.ShortageRate = data.ShortageRate;
            $scope.ShortageValue = data.ShortageValue;
            $scope.RejectionRate = data.RejectionRate;
            $scope.RejectionValue = data.RejectionValue;
            $scope.RejectionClamRate = data.RejectionClamRate;

            angular.element(document.querySelector('#ValueSet')).modal('show');
        }
        else {
            $scope.ShortageRate = '';
            $scope.ShortageValue = '';
            $scope.RejectionRate = '';
            $scope.RejectionValue = '';
            $scope.RejectionClamRate = '';
            $scope.MaterialGroupMasterName = data.MaterialGroupMasterName;
            $scope.UserName = data.UserName;
            $scope.StandardName = data.StandardName;
            $scope.FirstCharacteristicsValue = data.FirstCharacteristicsValue;
            $scope.SecondCharacteristicsValue = data.SecondCharacteristicsValue;
            $scope.ThirdCharacteristicsValue = data.ThirdCharacteristicsValue;

            $scope.TransactionRate = data.TransactionRate;
            $scope.ShortageQty = data.ShortageQty;
            $scope.RejectionQty = data.RejectionQty;

            $scope.PODetailsID = data.InventoryReceiveDetailId;
            $scope.ShortageRate = data.ShortageRate;
            $scope.ShortageValue = data.ShortageValue;
            $scope.RejectionRate = data.RejectionRate;
            $scope.RejectionValue = data.RejectionValue;
            $scope.RejectionClamRate = data.RejectionClamRate;

            angular.element(document.querySelector('#ValueSet')).modal('show');
        }


    };
    $scope.getReceiveTaxListPOValueSet1 = function (data, flag, index, Id) {
        //debugger;
        //angular.element(document.querySelector('#ValueSet')).modal('show');
        if ($scope.Action === 'Save') {

            $scope.new = [];
            //$scope.new = $scope.inventoryMaterialListPO;
            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                if ($scope.inventoryMaterialListPO[i].check === true) {
                    if ($scope.inventoryMaterialListPO[i].ShortageQty > 0 || $scope.inventoryMaterialListPO[i].RejectionQty > 0) {
                        $scope.new.push($scope.inventoryMaterialListPO[i]);
                    }
                }
            }

            //$scope.inventoryMaterialListPO = [];
            for (var i = 0; i < $scope.new.length; i++) {
                if ($scope.new[i].check == true) {
                    if ($scope.new[i].ShortageQty > 0 || $scope.new[i].RejectionQty > 0) {
                        $scope.new[i].ShortageRate = 110;
                        $scope.new[i].ShortageValue = (($scope.new[i].ShortageQty * $scope.new[i].ShortageRate) / 100) * $scope.new[i].TransactionRate;
                        $scope.new[i].RejectionRate = 50;
                        $scope.new[i].RejectionValue = (($scope.new[i].RejectionQty * $scope.new[i].RejectionRate) / 100) * $scope.new[i].TransactionRate;
                        $scope.new[i].RejectionClamRate = (100 - $scope.new[i].RejectionRate);

                    }
                }
            }

            angular.element(document.querySelector('#ValueSet')).modal('show');
        }
        else {

            $scope.new1 = [];

            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                if ($scope.inventoryMaterialList[i].check === true) {
                    if ($scope.inventoryMaterialList[i].ShortageQty > 0 || $scope.inventoryMaterialList[i].RejectionQty > 0) {
                        $scope.new1.push($scope.inventoryMaterialList[i]);
                    }
                }
            }

            for (var i = 0; i < $scope.new1.length; i++) {
                if ($scope.new1[i].check == true) {
                    if ($scope.new1[i].ShortageQty > 0 || $scope.new1[i].RejectionQty > 0) {
                        $scope.new1[i].ShortageRate = 110;
                        $scope.new1[i].ShortageValue = (($scope.new1[i].ShortageQty * $scope.new1[i].ShortageRate) / 100) * $scope.new1[i].TransactionRate;
                        $scope.new1[i].RejectionRate = 50;
                        $scope.new1[i].RejectionValue = (($scope.new1[i].RejectionQty * $scope.new1[i].RejectionRate) / 100) * $scope.new1[i].TransactionRate;
                        $scope.new1[i].RejectionClamRate = (100 - $scope.new1[i].RejectionRate);
                    }
                }
            }

            angular.element(document.querySelector('#ValueSet')).modal('show');
        }


    };
    $scope.CalculateShortageVal = function (x) {
        //debugger;
        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            $scope.inventoryMaterialListPO[i].ShortageValue = (($scope.inventoryMaterialListPO[i].ShortageQty * $scope.inventoryMaterialListPO[i].ShortageRate) / 100) * $scope.inventoryMaterialListPO[i].TransactionRate;
        }


    }
    $scope.CalculateRejectionVal = function () {
        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            $scope.inventoryMaterialListPO[i].RejectionValue = (($scope.inventoryMaterialListPO[i].RejectionQty * $scope.inventoryMaterialListPO[i].RejectionRate) / 100) * $scope.inventoryMaterialListPO[i].TransactionRate;
            $scope.inventoryMaterialListPO[i].RejectionClamRate = (100 - $scope.inventoryMaterialListPO[i].RejectionRate);


        }
    }
    function GetInventoryMaterialListByPO(inveReveiveId) {
        //debugger;
        $scope.masterId = inveReveiveId;
        $http.get($scope.path + 'GetInventoryMaterialListByPO?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.inventoryMaterialListPO = [];
                $scope.inventoryMaterialListPO = response.data.Rows;
                $scope.POID = $scope.inventoryMaterialListPO.POID;
                $scope.PreBal = $scope.inventoryMaterialListPO.Balance;
                $scope.PODetailsID = $scope.inventoryMaterialListPO.InventoryReceiveDetailId;
                $scope.productNew.InvoicingByAddress = $scope.inventoryMaterialListPO[0].InvoicingByAddress;
                $scope.productNew.DeliveryByAddress = $scope.inventoryMaterialListPO[0].DeliveryByAddress;
                $scope.inventoryMaterialListPO.BaseAmount = '0';
                //$scope.POId1 = '';
                checkSameValueInColumnList($scope.inventoryMaterialListPO, 'TransactionUoM');
                getGrossAmount($scope.inventoryMaterialListPO, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
                $scope.GetPOMaterialTaxData();
                $scope.POPopUpClose();
            });
    }



    //Load Service Tax with service charge
    function getServiceChargeListPO(inveReveiveId) {
        $scope.masterId1 = inveReveiveId;
        $http.get($scope.path + 'GetServiceChargeListPO?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.chargesListPO = [];
                $scope.chargesListPO = response.data;
                $scope.GetPOServiceTaxData();
            });
    }
    $scope.GetPOServiceTaxData = function () {
        //debugger;
        $scope.POServiceTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetServiceTaxListPO?serviceId=' + $scope.masterId1
        }).then(function (response) {
            $scope.POServiceTaxList = response.data;

            for (var i = 0; i < $scope.chargesListPO.length; i++) {
                var linepk = $scope.chargesListPO[i].Id;
                var list1 = getPOServicetaxlist(linepk);
                $scope.chargesListPO[i].POServiceTaxList = list1;
            }
        });
    };
    function getPOServicetaxlist(linepk1) {
        //debugger;
        var result1 = [];
        for (var i = 0; i < $scope.POServiceTaxList.length; i++) {
            if ($scope.POServiceTaxList[i].InventoryServiceId === linepk1) {
                result1.push($scope.POServiceTaxList[i]);
            }
        }
        return result1;
    }
    function getServicetaxlist1(linepk111) {
        //debugger;
        var result11 = [];
        for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
            if ($scope.ServiceTaxList[i].InventoryServiceId === linepk111) {
                result11.push($scope.ServiceTaxList[i]);
            }
        }
        return result11;
    }
    $scope.getServiceTaxList = function () { //,data, flag)
        //$scope.taxAbleAmnt = data.Amount + data.TotalTaxAmount;
        //$scope.percentageColumn = flag;

        $http({
            method: 'GET',
            url: $scope.path + 'GetServiceTaxListPR?serviceId=' + $scope.masterId12//data.Id
        }).then(function (response) {
            $scope.ServiceTaxList = response.data;
            for (var i = 0; i < $scope.chargesList.length; i++) {

                var linepk1 = $scope.chargesList[i].Id;
                var list11 = getServicetaxlist1(linepk1);
                $scope.chargesList[i].ServiceTaxList = list11;
                //$scope.inventoryMaterialList[i].ServiceTaxList = list11;
            }
            // angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
        });
    }



    $scope.getServiceTaxListPOPOP = function (data, flag, index, Id) {
        //debugger;
        $scope.taxAbleAmnt = data.Amount;
        $scope.percentageColumn = flag;
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.Amount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.ServiceTaxList = [];
        if (data.POServiceTaxList.length > 0) {
            $scope.HSNCode = data.POServiceTaxList[0].HSNCode;
            $scope.ServiceTaxList = data.POServiceTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.ServiceTaxList.length; j++) {
            $scope.total = $scope.total + $scope.ServiceTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#ServiceTaxPopUp')).modal('show');

    }


    $scope.getServiceTaxListPOPOP1 = function (data, flag, index, Id) {
        //debugger;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.ServiceTaxList = [];
        if (data.ServiceTaxList.length > 0) {
            $scope.HSNCode = data.ServiceTaxList[0].HSNCode;
            $scope.ServiceTaxList = data.ServiceTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.ServiceTaxList.length; j++) {
            $scope.total = $scope.total + $scope.ServiceTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#ServiceTaxPopUp')).modal('show');

    }

    $scope.closeReceiveTaxPopUp1 = function () {

        angular.element(document.querySelector('#ServiceTaxPopUp')).modal('hide');
    }

    $scope.YesMessageForClosed = function ($event) {
        //debugger

        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            if ($scope.inventoryMaterialListPO[i].check === true) {
                if ($scope.inventoryMaterialListPO[i].PODetailId === $scope.PODetailId) {
                    $scope.inventoryMaterialListPO[i].POClosStatus = true;
                }
            }
            else {
                $scope.inventoryMaterialListPO[i].POClosStatus = false;
            }
        }
    }
    $scope.NoMessageForClosed = function ($event) {
        //debugger

        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            if ($scope.GetListForMasterOrder[i].check === true) {
                if ($scope.GetListForMasterOrder[i].PODetailId === $scope.PODetailId) {
                    $scope.inventoryMaterialListPO[i].POClosStatus = false;
                }
            }
            else {
                $scope.GetListForMasterOrder[i].WantToClose = false;
            }
        }
    }






    $scope.calculateRate = function (data, event) {
        //debugger;
        data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
        if (data.TransactionRate === 'NaN')
            data.TransactionRate = 0;
        data.BaseTaxAmount = 0;
        angular.forEach(data.POMaterialTaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;

            data.BaseTaxAmount += item.TaxAmount;
        });
        data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;


    };
    $scope.calculateAmount = function (data, index) {
        //debugger;
        data.check = false;
        data.POClosStatus = false;
        $scope.PreBal = data.Balance;

        // data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
        data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        if (data.TrnAmount == 'NaN')
            data.TrnAmount = 0;
        data.TaxAmount = 0;
        data.BaseTaxAmount = 0;
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount');

        angular.forEach(data.POMaterialTaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;
            data.BaseTaxAmount += item.TaxAmount;

        });

        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            $scope.inventoryMaterialListPO[i].Balance = '';
            if ($scope.inventoryMaterialListPO[i].POQty < ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty)) {
                $scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
                ShowResult('Current quantity can not grater than balance qty!', 'failure');
                $scope.inventoryMaterialListPO[i].TransactionQty = '';
            }
            else if ($scope.inventoryMaterialListPO[i].ShortageQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
                //$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
                ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');

            }
            else if ($scope.inventoryMaterialListPO[i].RejectionQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
                //$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
                ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
            }
            else {
                if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
                    $scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
                    $scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                    $scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                    $scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);

                }
                else {
                    $scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                    $scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                    $scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);
                }
                if ($scope.productNew.IsNonCreditable == 1) {
                    if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
                        $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)).toFixed(2);
                        $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);
                    }

                }

                else {
                    if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {

                        $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge)).toFixed(2);
                        $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
                    }
                }
            }
        }

    };


    // #endregion


    $scope.enableid1 = true;
    $scope.enableid3 = true;
    $scope.Change1 = function (event, index, x) {

        if (event.currentTarget.checked) {
            $scope.index = index;
            //$scope.staus = false;
            x.enableid1 = false;
            x.check == true;
        }


        else {
            x.enableid1 = true;
            x.check == false;
            //$scope.index = index;
        }
    }
    $scope.enableid2 = true;

    $scope.Change2 = function (event, index, x) {
        for (var i = 0; i < $scope.chargesList.length; i++) {
            $scope.chargesList[i].check = true;
        }


    }


    $scope.calculateAmountForServiceCharge = function (data) {

        data.TotalTaxAmount = 0;
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');

        for (var i = 0; i < $scope.POServiceTaxList.length; i++) {
            if ($scope.POServiceTaxList[i].InventoryServiceId == data.Id) {
                $scope.POServiceTaxList[i].TaxAmount = data.Amount * $scope.POServiceTaxList[i].Percentage / 100;
                data.TotalTaxAmount += $scope.POServiceTaxList[i].TaxAmount;
            }
        }
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount');


        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            $scope.inventoryMaterialListPO[i].ServiceCharge = (parseFloat(TotalServiceAmount).toFixed(2) / parseFloat(TotalTrnAmount).toFixed(2)) * parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2);
            $scope.inventoryMaterialListPO[i].ServiceTax = (parseFloat(TotalServiceTaxAmount).toFixed(2) / parseFloat(TotalTrnAmount).toFixed(2)) * parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2);

            if ($scope.productNew.IsNonCreditable == 1) {
                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)).toFixed(2);
                $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);


            }
            else {

                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)).toFixed(2);
                $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
            }

        }

    };


    $scope.calculateAmountForServiceCharge1 = function (data) {


        data.TotalTaxAmount = 0;
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesList), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TrnAmount');

        for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
            if ($scope.ServiceTaxList[i].InventoryServiceId == data.Id) {
                $scope.ServiceTaxList[i].TaxAmount = data.Amount * $scope.ServiceTaxList[i].Percentage / 100;
                data.TotalTaxAmount += $scope.ServiceTaxList[i].TaxAmount;
            }
        }
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');






        for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {

            $scope.inventoryMaterialList[i].ServiceCharge = (parseFloat(TotalServiceAmount).toFixed(2) / parseFloat(TotalTrnAmount).toFixed(2)) * parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2);
            $scope.inventoryMaterialList[i].ServiceTax = (parseFloat(TotalServiceTaxAmount).toFixed(2) / parseFloat(TotalTrnAmount).toFixed(2)) * parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2);

            if ($scope.productNew.IsNonCreditable == 1) {


                $scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)).toFixed(2);
                $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);


            }
            else {
                data.TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge).toFixed(2);
                data.TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge).toFixed(2)) * $scope.productNew.ToCurrencyRate);
            }

        }
    };


    //#region  GRNReport

    $scope.GRNReport = function (data) {

        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;
    };

    //#endregion

    $scope.calculateMaterialTax = function (data, index) {

        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
        var TotalMaterialTaxAmount = $filter('sumByKey')($filter('filter')($scope.receiveTaxList), 'TaxAmount');


        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailId) {
                $scope.inventoryMaterialListPO[i].BaseTaxAmount = TotalMaterialTaxAmount;
                $scope.inventoryMaterialListPO[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
                $scope.inventoryMaterialListPO[i].ServiceTax = parseFloat((TotalMaterialTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
            }
            else {
                $scope.inventoryMaterialListPO[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
                $scope.inventoryMaterialListPO[i].ServiceTax = parseFloat((TotalMaterialTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
            }
            if ($scope.productNew.IsNonCreditable == 1) {
                //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
                $scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount + $scope.inventoryMaterialListPO[i].ServiceCharge + $scope.inventoryMaterialListPO[i].ServiceTax).toFixed(4);

            }
            else {
                data.BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(4);
            }

        }

    };

    $scope.calculateSerciceTax = function (data) {
        //debugger;
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
        var ServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'TotalTaxAmount');

        for (var i = 0; i < $scope.chargesListPO.length; i++) {
            if ($scope.chargesListPO[i].Id == data.InventoryServiceId) {
                $scope.chargesListPO[i].TotalTaxAmount = ServiceTaxAmount;
            }
        }

        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {

            $scope.inventoryMaterialListPO[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
            $scope.inventoryMaterialListPO[i].ServiceTax = parseFloat((TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);

            if ($scope.productNew.IsNonCreditable == 1) {
                //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
                $scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount + $scope.inventoryMaterialListPO[i].ServiceCharge + $scope.inventoryMaterialListPO[i].ServiceTax).toFixed(4);

            }
            else {
                data.BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(4);
            }

        }

    };
    $scope.onClickReportDownloadExcel = function (args) {
        //debugger;
        var gridObj = $("#GriddataMaster1").data("ejGrid");
        //getting corresponding record 
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);

    };

    $scope.commandExcel = [{
        type: "details", buttonOptions: {
            text: "Excel",
            BackgroundColor: "Black",
            Color: "White",
            width: "50",
            height: "20",
            contentType: "imageonly",
            prefixIcon: "e-icon e-dataexport",

            click: $scope.onClickReportDownloadExcel
        }
    }];
    $scope.onClickReportDownloadPdf = function (args) {
        //debugger;
        var gridObj = $("#GriddataMaster1").data("ejGrid");
        //getting corresponding record 
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);

    };
    $scope.commandPdf = [{
        type: "details", buttonOptions: {
            text: "Pdf",
            width: "50",
            height: "20",
            contentType: "imageonly",
            prefixIcon: "e-icon e-dataexport",
            click: $scope.onClickReportDownloadPdf
        }
    }];






    $scope.Get = function (index) {
        //debugger;
        $scope.index = index;
        $scope.product = $scope.products[$scope.index];
        $scope.productNew = Object.assign({}, $scope.product);
        // $scope.productNew.GRNDate = data.GRNDate;
        getPartyPlantList();
        getInventoryMaterialList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
        $scope.productId = $scope.productNew.Id;
        //$scope.getToCurrencyRate();
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate')
                    $scope.IsBaseOnDueDateEnable = true;
                else
                    $scope.IsBaseOnDueDateEnable = false;
        }
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };


    $scope.MasterOrderListHide = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListOfRequisition')).modal('hide');
    };
    $scope.rowDataBound = function rowDataBound(e) {

        if ($scope.RowColor != e.data.MaterialGroupMasterName + e.data.UserName + e.data.StandardName + e.data.FirstCharacteristicsValue + e.data.SecondCharacteristicsValue + e.data.ThirdCharacteristicsValue) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.RowColor = e.data.MaterialGroupMasterName + e.data.UserName + e.data.StandardName + e.data.FirstCharacteristicsValue + e.data.SecondCharacteristicsValue + e.data.ThirdCharacteristicsValue;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", '#D3D3D3');
        else
            e.row.css("background-color", '#ffffff');


    }
    $scope.PODetailsUpdatePOPUp = function (x, MaterialMasterId, InventoryReceiveDetailId) {
        //debugger;
        $scope.Action1 = 'Update'
        // $scope.GetListForMasterOrder = [];
        getInventoryMaterialListForUpdate(x, MaterialMasterId, InventoryReceiveDetailId);
        // $scope.GerRequisition();
        angular.element(document.querySelector('#ListOfRequisition')).modal('show');
    };
    $scope.GetListForMasterOrder = [];
    function getInventoryMaterialListForUpdate(inveReveiveId, MaterialMasterId, InventoryReceiveDetailId) {
        $scope.Action1 = 'Save';
        $scope.masterId = inveReveiveId;
        //debugger;
        //$scope.inventoryMaterialList = [];
        $http.get($scope.path + 'GetInventoryMaterialListForPOUpdate?inveReveiveId=' + inveReveiveId + '&InventoryReceiveId=' + $scope.productNew.Id + '&MaterialMasterId=' + MaterialMasterId + '&InventoryReceiveDetailId=' + InventoryReceiveDetailId)
            .then(function (response) {
                $scope.GetListForMasterOrder = response.data;
                $scope.totalGRNVal = $scope.GetListForMasterOrder[0].GRNQty;
                $scope.RejectionQty = $scope.GetListForMasterOrder[0].RejectionQty;
            });


    }


    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    function getTaxList(inveReveiveId) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetTaxCategoryListPO?receiveDetailId=' + inveReveiveId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
        });
    }
    function checkChangeemployee(e) {
        var val = e.model.value;
        var hsnCodeId = $scope.GetListForMasterOrder[0].HSNCodeId;
        var row = $filter('filter')($scope.GetListForMasterOrder, { 'RequisitionDetailId': e.model.value });

        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check") {
                row[0].CheckedStatus = true;

            }
            else
                row[0].CheckedStatus = false;
        }

    }
    function headCheckChangeemployee(e) {
        var val = e.model.value;
        var hsnCodeId = $scope.GetListForMasterOrder[0].HSNCodeId;
        var row = $filter('filter')($scope.GetListForMasterOrder, { 'RequisitionDetailId': e.model.value });

        if (e.model.checkState == "check") {
            // alert('2');

            var filtered = $("#GridReq").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    $scope.GetListForMasterOrder[i].CheckedStatus = true;
                }
            }
            else {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.GetListForMasterOrder[i].RequisitionDetailId == filtered[j].RequisitionDetailId)
                            $scope.GetListForMasterOrder[i].CheckedStatus = true;
                    }

                }
            }

            var checkbox = $("#GridReq .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#GridReq").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    $scope.GetListForMasterOrder[i].CheckedStatus = false;
                }
            }
            else {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.GetListForMasterOrder[i].RequisitionDetailId == filtered[j].RequisitionDetailId)
                            $scope.GetListForMasterOrder[i].CheckedStatus = false;
                    }

                }
            }
            var checkbox = $("#GridReq .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
    }
    $scope.dataBoundemployee = function (args) {
        $("#GridReq .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    }
    $scope.refreshTemplateemployee = function (args) {
        //alert('fff');
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.GetListForMasterOrder, { 'RequisitionDetailId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].CheckedStatus == true)
                $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    }



    $scope.tab1 = 1;
    $scope.setTabIndex = function (newTab) {
        $scope.tab1 = newTab;
        $scope.getalldata();
    };
    $scope.isSetIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTabIndex1 = function (newTab) {
        $scope.tab1 = newTab;
        $scope.getalldataIndexApp();
    };
    $scope.isSetIndex1 = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.onClickReportAHRDownloadWord = function (args) {
        var gridObj = $("#GriddataMasterAHR1").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandAHRWord = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportAHRDownloadWord
        }
    }];


    $scope.onClickReportPostedDownloadWord3 = function (args) {
        var gridObj = $("#GriddataMasterAHR3").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandWordPosted = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportPostedDownloadWord3
        }
    }];

    $scope.GriddataMaster2 = [];
    $scope.getalldataMaster2 = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetListForGrnByPoReq?GRNWithReqPOApprovedStatus=' + $scope.GRNWithReqPOApprovedStatus,
        }).then(function successCallback(response) {
            $scope.GriddataMaster2 = response.data;

        });
    };

    $scope.onClickReportANPDownloadWord1 = function (args) {
        var gridObj = $("#GriddataMaster1").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };
    $scope.commandANPWord1 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportANPDownloadWord1
        }
    }];


    $scope.onClickReportANPDownloadWord2 = function (args) {
        var gridObj = $("#GriddataMasterHR").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandANPWord2 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportANPDownloadWord2
        }
    }];



    $scope.onClickReportANPDownloadWord3 = function (args) {
        var gridObj = $("#GriddataMasterAC").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandANPWord3 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportANPDownloadWord3
        }
    }];

    $scope.onClickReportANPDownloadWord4 = function (args) {
        var gridObj = $("#GriddataMasterAHR4").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };
    $scope.commandANPWord4 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportANPDownloadWord4
        }
    }];


    $scope.onClickReportANPDownloadWord5 = function (args) {
        var gridObj = $("#GriddataMasterANP5").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandANPWord5 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportANPDownloadWord5
        }
    }];



    $scope.onClickReportANPDownloadWord6 = function (args) {
        var gridObj = $("#GriddataMasterANP6").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandANPWord6 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportANPDownloadWord6
        }
    }];


    //#endregion ---Inventory Receive GRN Print Option---

    //#region Additional Code
    $scope.advanceTax = {
        TaxCodeId: null,
        Text: null,
        TaxAmount: null,
        ValueOfFixed: null,
        CompanyCurrencyAmount: null,
        Type: null,
        TaxCategoryId: null,
        TotalSumAfterTCSVal: null,
    };
    $scope.advanceTaxesList = [];
    $scope.additionalTax = function () {
        $scope.advanceTaxesList = [];
        for (var i = 0; i < $scope.advanceTaxesList.length; i++) {
            if ($scope.advanceTaxesList[i].TaxCodeId === $scope.advanceTax.TaxCodeId) {
                ShowResult("Tax Already Added");
                return false;
            }

        }

        if (manualValidation("td_TaxCode", baseService.isUndefinedOrNull($scope.advanceTax.TaxCodeId), "Tax Code is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeAmount", baseService.isUndefinedOrNull($scope.advanceTax.TaxAmount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.advanceTax.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.advanceTax.TaxName = $.grep($scope.taxCodCboListWithhold, function (item) {
                return item.Id === $scope.advanceTax.TaxCodeId;
            })[0].UserName;

            $scope.advanceTaxesList.push($scope.advanceTax);
            //$scope.advanceTax = {};
        }
        $scope.TotalSumAfterTCS();
    };

    $scope.taxCodCboListWithhold = [];
    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYearWithhold = function (date) {
        $scope.productNew.TaxOptionAddiTax = 'Yes';
        $http({
            method: "Get",
            url: "accounts/TaxCode/GetAdditionalTaxCbo?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message;
                }
                else {
                    $scope.taxCodCboListWithhold = response.data;;
                }
            },
            function errorCallback(response) {
            });
    };
    $scope.GRNDate = $filter("dateFiltering")(Date.now());
    $scope.getTaxCodeByTaxYearWithhold($scope.GRNDate);
    $scope.selectadditionalTax = function () {
        $scope.advanceTax.ValueOfFixed = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].ValueOfFixed;
        $scope.advanceTax.TaxCategoryId = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].TaxCategoryId;
        $scope.advanceTax.Type = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].Type;
        if ($scope.advanceTax.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.advanceTax.ValueOfFixed)) {//* $scope.advanceTax.ValueOfFixed / 100
            //$scope.advanceTax.TaxAmount = (parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax")) * $scope.advanceTax.ValueOfFixed / 100);
            if ($scope.Action === 'Save') {
                $scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceTax"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);

            }
            else {
                $scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);

            }
        }
        $scope.TotalSumAfterTCS();
    }

    $scope.SaveAdditinalTaxInGRNList = function (POReturnNo) {
        $http({
            method: 'POST',
            url: 'Products/InventoryReceive/SaveAdditinalTaxInPurchaseReturn',
            data:
            {
                'InventoryReceiveId': POReturnNo,
                'UserSendData': $scope.advanceTaxesList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //ShowResult(response.data.Message, 'success');
                $scope.TotalSumAfterTCS();

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }



    $scope.GetAdvanceTaxInfo = function (Id) {

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/InventoryReceive/GetAdvanceTaxInfoPurchaseReturn?PurchaseReturnId=' + Id,
        }).then(function successCallback(response) {
            $scope.advanceTaxesList = response.data;

        });
    }
    $scope.removeTaxesRow = function (Id, index) {
        if (baseService.isUndefinedOrNull(Id)) {
            $scope.advanceTaxesList.splice(index, 1);

        }
        else {
            $scope.DeleteAdditinalTax(Id);
            $scope.GetAdvanceTaxInfo($scope.productNew.Id);
        }
    };
    $scope.DeleteAdditinalTax = function (Id) {
        $http({
            method: 'POST',
            url: 'Products/InventoryReceive/AdditionalTaxDeletePurchaseReturn?Id=' + Id,
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.TaxOptionAdditax = function (data) {
        debugger;
        $scope.productNew.TaxOptionAddiTax = data;
    };

    $scope.calculateTaxAmountForAdditionalTax = function (data) {
        $scope.advanceTax.TaxAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseAmount") * data / 100).toFixed(2);

    };
    $scope.checkRowValidationSdditionalTax = function (data) {
        debugger;

        if ($scope.Action === 'Save') {
            var netAmount = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceTax"))).toFixed(2);

            $scope.advanceTax.ValueOfFixed = ((data / netAmount).toFixed(4) * 100);
        }
        else {
            var netAmount1 = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax"))).toFixed(2);

            $scope.advanceTax.ValueOfFixed = ((data / netAmount1).toFixed(4) * 100);
        }
    }

    $scope.TotalSumAfterTCS = function () {

        if ($scope.Action === 'Save') {
            $scope.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);

        }
        else {
            $scope.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);

        }
    }
}