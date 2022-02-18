'use strict';
purchaseOrderBOQController.$inject = ['accountService', 'addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$location'];
function purchaseOrderBOQController(accountService, addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $location) {
    $rootScope.title = "PO BOQ";
    $scope.ActionPOBOQ = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/PurchaseOrder/';
    $scope.saveGridUrl = $scope.path + 'SaveData';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlFG = $scope.path + 'CreateFGMasterOrder';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.updateUrlFG = $scope.path + 'FGMasterOrderedit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete?receiveDetailId=';
    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
    $scope.saveTitleUrl = $scope.path + 'SaveTitle';
    $scope.saveTermsDetail = $scope.path + 'SaveTermsDetail';
    $scope.PurchaseOrderFileLocation = virtualPath.PurchaseOrder;
    $scope.partyType = 'Vendor';
    $scope.isAdvance = false;
    $scope.currentDate = new Date(Date.now());
    $scope.grossTotal = 0;
    $scope.PartyId = null;
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.isSubmitted = 'No';
    $scope.SubmitContractId = null;
    $scope.SubmitContractNo = null;
    $scope.SubmitCustomerName = null;
    $scope.SubmitPartyCode = null;
    $scope.SubmitPartyName = null;
    $scope.SubmitPartyId = null;


    $scope.contractList = [];
    $scope.GetPopUpContract = function () {
        $scope.contractList = [];
        $http.get("Products/PurchaseOrder/GetLCContractList?isProcurementOnBom=" + $scope.productNew.IsTradingPO)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.contractList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#ContractPopUp')).modal('show');
    };
    $scope.Clearcontract = function () {
        $scope.SubmitContractId = null;
        $scope.SubmitContractNo = null;
        $scope.SubmitCustomerName = null;
    };

    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.SubmitPartyCode = party.Code;
        $scope.SubmitPartyName = party.UserName;
        $scope.SubmitPartyId = party.Id;
        $scope.SubmitPaymentTermId = party.PaymentTermId;
        $scope.SubmitCurrencyId = party.CurrencyId;
        getPartyPlantList();
        $scope.hidePartyPopUp();
    };
    function getPartyPlantList() {
        $scope.plantList = [];
        $http.get('Products/PurchaseOrder/GetPartyPlantCbo?partyId=' + $scope.SubmitPartyId + '&Id=' + $scope.Id).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.IsDefault) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = item.Value;
                    $scope.productNew.InvoicingByAddress = item.Address1;
                    $scope.productNew.DeliveryByAddress = item.Address2;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = item.StateName;
                    $scope.productNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });

    }

    $scope.SelectedContract = function (obj) {
        $scope.SubmitContractId = obj.data.ContractId;
        $scope.SubmitContractNo = obj.data.ContractNo;
        $scope.SubmitCustomerName = obj.data.CustomerName;
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }

    $scope.CloseContractPopUp = function () {
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }
    $scope.poBoqItemList = [];
    $scope.GetPOBoqItem = function () {
        $scope.poBoqItemList = [];
        $http.get("Products/PurchaseOrder/GetPOBOQItems?ContractId=" + $scope.SubmitContractId + '&VendorId=' + $scope.SubmitPartyId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.poBoqItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }


    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
    }).then(function successCallback(response) {
        $scope.baseCurrencyId = response.data[0].Value;
        $scope.productNew.BaseCurrencyId = response.data[0].Value;
    });
    $scope.currencyList = [];
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = result;
    });


    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
    }).then(function successCallback(response) {
        $scope.baseCurrencyId = response.data[0].Value;
        $scope.productNew.BaseCurrencyId = response.data[0].Value;
    });
    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getvendorcbo'
    }).then(function successCallback(response) {

        $scope.paymentTermList = response.data;
    });
    $scope.product = {
        Id: null
        , GRNDate: null
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
        , IsClosed: false
        , DeliveryInstruction: null
        , SpecialInstruction: null
        , CheckedBy: null
        , AuthorizedBy: null
        , CheckedByStatus: null
        , AuthorizedByStatus: null
        , ContractId: null
        , OrderSpecific: 'Yes'
        , PurchaseLCId: null
        , CustomerName: null
        , PaymentMode: null
        , ContractNo: null
        , LCRef: null
        , labelCheckAndApproved: null
        , CheckedByStatusForNoti: null
        , ApprovedByStatusForNoti: null
        , DiscountAmount: 0
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionService: 'Yes'
        , TaxOptionServiceModify: 'Yes'
        , FileName: null
        , UserFilename: null
        , SystemFileName: null
        , Description: null
        , Remarks: null
        , PODate: null
        , Tolerance: 0
        , TermsAndConditionsId: null
        , IsTradingPO: false
    };
    $scope.productNew = Object.assign({}, $scope.product);


    $scope.TermsAndConditions = {
        Id: null
        , Description: null
        , TermsAndConditions: null
    };
    $scope.TermsAndConditionsList = [];
    $scope.TermsAndConditions = function () {

        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/PurchaseOrder/TermsAndConditions'
        }).then(function successCallback(response) {
            $scope.TermsAndConditionsList = response.data;
            //$scope.TermsAndCondition.TermsAndConditions = response.data[0].TermsAndConditions;

        });
    }
    $scope.TermsAndConditions();
    $scope.changeTermsAndCondition = function () {

        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.TermsAndConditionsList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
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

    $scope.submit = function () {
        $scope.productNew.PartyCode = $scope.SubmitPartyCode;
        $scope.productNew.PartyName = $scope.SubmitPartyName;
        $scope.productNew.PartyId = $scope.SubmitPartyId;
        $scope.productNew.PaymentTermId = $scope.SubmitPaymentTermId;
        $scope.productNew.CurrencyId = $scope.SubmitCurrencyId;
        $scope.productNew.ContractId = $scope.SubmitContractId;
        $scope.productNew.ContractNo = $scope.SubmitContractNo;
        $scope.productNew.CustomerName = $scope.SubmitCustomerName;
        $scope.submitBOQItem();
        $scope.isSubmitted = 'Yes';
    }
    $scope.Back = function () {
        $scope.isSubmitted = 'No';

    };
    $scope.poBoqItemListNew = [];
    $scope.submitBOQItem = function () {
        try {
            $scope.poBoqItemListNew = [];
            for (var i = 0; i < $scope.poBoqItemList.length; i++) {
                if ((baseService.isUndefinedOrNull($scope.poBoqItemList[i].TransactionQty) || $scope.poBoqItemList[i].TransactionQty === 0) && $scope.poBoqItemList[i].CheckedStatus === true) {
                    ShowResult('Enter the Selected  Material Qty', 'failure');
                    return false;
                }
                if ($scope.poBoqItemList[i].CheckedStatus === true) {
                    if ((parseFloat($scope.poBoqItemList[i].TransactionQty) + parseFloat($scope.poBoqItemList[i].OtherPOQty)) > parseFloat($scope.poBoqItemList[i].RequiredQtyPO)) {
                        ShowResult('Trasaction qty can not grater than booking Qty', 'failure');
                        $scope.poBoqItemList[i].TransactionQty = '';
                        return false;
                    }
                    if (baseService.isUndefinedOrNull($scope.poBoqItemList[i].TransactionQty)) {
                        ShowResult('Enter the current Qty.Zero not allowed', 'failure');
                        return false;
                    }
                    if ($scope.poBoqItemList[i].TransactionQty < 0) {
                        ShowResult('Negative Qty  not allowed', 'failure');
                        return false;
                    }
                    if ($scope.poBoqItemList[i].TransactionQty === 0 || $scope.poBoqItemList[i].TransactionQty === 0.00 || $scope.poBoqItemList[i].TransactionQty === 0.0) {
                        ShowResult('Enter the current Qty.Zero not allowed', 'failure');
                        return false;
                    }
                    if (baseService.isUndefinedOrNull($scope.poBoqItemList[i].TransactionRate)) {
                        ShowResult('Enter the current rate.Zero not allowed', 'failure');
                        return false;
                    }
                    if ($scope.poBoqItemList[i].TransactionRate === 0 || $scope.poBoqItemList[i].TransactionRate === 0.0 || $scope.poBoqItemList[i].TransactionRate === 0.00) {
                        ShowResult('Enter the current rate.Zero not allowed', 'failure');
                        return false;
                    }
                    if ($scope.poBoqItemList[i].RequiredQtyApproved === 'No') {
                        ShowResult('Required Qty not yet Approved.So you can not take this material', 'failure');
                        return false;
                    }
                    if ($scope.poBoqItemList[i].IncompleteMaterial === 'Yes') {
                        ShowResult('This is incomplete material.So you can not take this material', 'failure');
                        return false;
                    }

                    else {
                        $scope.poBoqItemList[i].TrnAmount = Math.round(($scope.poBoqItemList[i].TransactionQty * $scope.poBoqItemList[i].TransactionRate) * 100 + Number.EPSILON) / 100
                        var getRow = $filter("filter")($scope.poBoqItemListNew, {
                            "MaterialMasterId": $scope.poBoqItemList[i].MaterialMasterId, "ArticleId": $scope.poBoqItemList[i].ArticleId
                            , "FirstCharacteristicsValueId": $scope.poBoqItemList[i].FirstCharacteristicsValueId
                            , "SecondCharacteristicsValueId": $scope.poBoqItemList[i].SecondCharacteristicsValueId
                            , "ThitrdCharacteristicsValueId": $scope.poBoqItemList[i].ThitrdCharacteristicsValueId
                            , "GroupId": $scope.poBoqItemList[i].GroupId
                        });

                        if (getRow.length == 0) {
                            $scope.poBoqItemListNew.push($scope.poBoqItemList[i]);
                        } else {
                            for (var j = 0; j < $scope.poBoqItemListNew.length; j++) {
                                var row = $scope.poBoqItemListNew[j];
                                if ( row.MaterialMasterId == $scope.poBoqItemList[i].MaterialMasterId
                                    && row.ArticleId == $scope.poBoqItemList[i].ArticleId
                                    && row.FirstCharacteristicsValueId == $scope.poBoqItemList[i].FirstCharacteristicsValueId
                                    && row.SecondCharacteristicsValueId == $scope.poBoqItemList[i].SecondCharacteristicsValueId
                                    && row.ThitrdCharacteristicsValueId == $scope.poBoqItemList[i].ThitrdCharacteristicsValueId
                                    && row.GroupId == $scope.poBoqItemList[i].GroupId
                                )
                                $scope.poBoqItemListNew[j].TransactionQty = Math.round($scope.poBoqItemListNew[j].TransactionQty * 100 + Number.EPSILON) / 100 + Math.round($scope.poBoqItemList[i].TransactionQty * 100 + Number.EPSILON) / 100;
                                $scope.poBoqItemListNew[j].TrnAmount = Math.round($scope.poBoqItemListNew[j].TrnAmount * 100 + Number.EPSILON) / 100 + Math.round($scope.poBoqItemList[i].TrnAmount * 100 + Number.EPSILON) / 100;
                            }
                        }
                        $scope.tempList.push($scope.poBoqItemList[i]);
                    }
                }
            }
            $scope.UOMValidation();
            $scope.groupList = [];
            $scope.processgroupList($scope.GetListForMasterOrdernew, $scope.groupList);
        } catch (e) {
        }
    };

    $scope.groupList = [];
    $scope.processgroupList = function (oldlist, newlist) {
        for (var i = 0; i < oldlist.length; i++) {
            var getRow = $filter("filter")(oldlist, { "MaterialMasterId": oldlist[i].MaterialMasterId, "ArticleId": oldlist[i].ArticleId, "FirstCharacteristicsValueId": oldlist[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": oldlist[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": oldlist[i].ThitrdCharacteristicsValueId });
            var ExistingRow = $filter("filter")(newlist, { "MaterialMasterId": oldlist[i].MaterialMasterId, "ArticleId": oldlist[i].ArticleId, "FirstCharacteristicsValueId": oldlist[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": oldlist[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": oldlist[i].ThitrdCharacteristicsValueId });
            // getRow.TransactionQty = $filter('sumByKey')($filter('filter')(oldlist), 'TaxAmount');
            if (ExistingRow.length === 0) {
                if (!baseService.isUndefinedOrNull(getRow[0].MaterialMasterId)) {
                    newlist.push(getRow[0]);
                }


            }
            var getRowWithoutMaterial = $filter("filter")(oldlist, { "MaterialDetail": oldlist[i].MaterialDetail, "RequisitionDetailId": oldlist[i].RequisitionDetailId });

            if (getRowWithoutMaterial.length === 1) {
                if (baseService.isUndefinedOrNull(getRowWithoutMaterial[0].MaterialMasterId)) {
                    newlist.push(getRowWithoutMaterial[0]);
                }
            }

        }
        return newlist;
    };
    $scope.tempList = [];
    $scope.UOMValidation = function () {
        var getRow3
        $scope.invalid = false;
        for (var i = 0; i < $scope.tempList.length; i++) {
            getRow3 = $filter("filter")($scope.tempList, { "MaterialMasterId": $scope.tempList[i].MaterialMasterId, "ArticleId": $scope.tempList[i].ArticleId, "FirstCharacteristicsValueId": $scope.tempList[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.tempList[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.tempList[i].ThirdCharacteristicsValueId });
        }
        $scope.TransactionUoMId = '';
        for (var k = 0; k < getRow3.length; k++) {
            $scope.TransactionUoMId = getRow3[0].TransactionUoMId;
            if (getRow3[k].TransactionUoMId != $scope.TransactionUoMId) {
                ShowResult('Have you selected Same UOM?', 'failure', 'ListOfPOMaterial');
                return true;
            }
        }
        return false;
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.GetCurrencyExchangeRateList = function () {

        //if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
        if (!baseService.isUndefinedOrNull(!baseService.isUndefinedOrNull($scope.productNew.CurrencyId))) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.productNew.DocDate + "&currencyId=" + $scope.productNew.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.productNew.ToCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };

    $scope.detailPOSaveForBOQ = function () {
        ;
        try {
            $scope.UOMValidation();

            if ($scope.ActionPOBOQ === 'Save') {
                if (!$scope.UOMValidation()) {//$scope.invalid &&

                    $http({
                        method: 'POST',
                        url: 'Products/PurchaseOrder/POBoqInsertUpdate',
                        data: {
                              entity: $scope.productNew
                            , groupList: JSON.stringify($scope.poBoqItemListNew)
                            , boqmapList: JSON.stringify($scope.tempList)
                            , taxCategoryList: $scope.taxCategoryList//$scope.taxCategoryList
                            , PoId: $scope.productNew.Id
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true)
                            ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial');
                        else {
                            ShowResult(response.data.Message, 'success', 'ListOfPOMaterial');
                            getInventoryMaterialList($scope.productNew.Id);
                            angular.element(document.querySelector('#ListOfPOMaterial')).modal('hide');

                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial');
                    };

                }
            }

            else if ($scope.ActionPOBOQ === "Update") {
                $scope.materialValidationForBOQItem();
                if (!$scope.UOMValidation() && !$scope.trnRateDiff()) {
                    $http({
                        method: 'POST',
                        url: 'Products/PurchaseOrder/detailPOUpdateForBOQ',
                        data: {
                            entity: $scope.productNew
                            , groupList: JSON.stringify($scope.poBoqItemListNew)
                            , boqmapList: JSON.stringify($scope.tempList)
                            , taxCategoryList: $scope.taxCategoryList//$scope.taxCategoryList
                            , PoId: $scope.productNew.Id
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true)
                            ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial1');
                        else {
                            ShowResult(response.data.Message, 'success', 'ListOfPOMaterial1');
                            getInventoryMaterialList($scope.productNew.Id);
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial1');
                    };

                }
            }

        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };
}//End Of main

