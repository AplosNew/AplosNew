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
    $scope.deleteUrl = $scope.path + 'DeletePOMaster/';
    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete?receiveDetailId=';
    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.POBOQ1SaveUrl = $scope.path + 'POBOQSave';
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

    $scope.GridApproved = [];
    $scope.GridApprovedHR = [];
    $scope.Griddata = [];
    $scope.POTypeStatus = 'Pending';
    $scope.getalldata = function () {
        $scope.Griddata = [];
        if ($scope.POTypeStatus === 'Pending') {
            $scope.POTypeStatus = 'Pending'
        }

        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseOrder/GetPOTypeList?POTypeStatus=' + $scope.POTypeStatus + '&poType=' + 'POBOQ',
        }).then(function successCallback(response) {

            for (var i = 0; i < $scope.Griddata.length; i++) {
                response.data[i].PODate1 = new Date($scope.Griddata[i].PODate1);
            }
            $scope.Griddata = response.data;
        });
    };
    $scope.getalldata();
    //#region Tab

    //#region all Tab Function of PO Index

    $scope.POTypeStatus = '';
    $scope.tab1 = 1;
    $scope.setTabIndex = function (newTab) {

        $scope.POTypeStatus = 'Pending';
        $scope.getalldata();
        $scope.tab1 = newTab;

    };
    $scope.isSetIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTabCHRIndex = function (newTab) {
        //alert('tabCHR');

        $scope.POTypeStatus = 'CheckedHoldRej';
        $scope.getalldata();
        $scope.tab1 = newTab;

    };
    $scope.isSetCHRIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTabCheckedIndex = function (newTab) {

        $scope.POTypeStatus = 'Checked';
        $scope.getalldata();
        $scope.tab1 = newTab;


    };
    $scope.isSetCheckedIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };
    $scope.setTabAHRIndex = function (newTab) {
        $scope.ApproveRejectHold = 'HoldReject';
        $scope.getalldataPoApp();
        $scope.tab1 = newTab;
    };
    $scope.isSetAHRIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };



    $scope.setTabIndex1 = function (newTab) {
        $scope.ApproveRejectHold = 'Approved';
        $scope.getalldataPoApp();
        $scope.tab1 = newTab;
    };
    $scope.isSetIndex1 = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.GriddataPoApp = [];
    $scope.getalldataPoApp = function () {

        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseOrder/GetListForHold11BOQ?ApproveRejectHold=' + $scope.ApproveRejectHold + '&poType=' + 'POBOQ',
        }).then(function successCallback(response) {
            $scope.GriddataPoApp = response.data;
            //entrydata = copy(searchdata);
        });
    };

    //#endregion
    //#endregion

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

    $scope.searchBOQByParty = "UserName"; $scope.searchBOQParty = "";
    $scope.searchBOQByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];


    $scope.newpartyList = [];
    $scope.showBOQPartyPopUpNew = function () {

        if ($scope.partyType === 'Vendor') {
            $scope.partyUrl = 'Products/PurchaseOrder/GetCompanyBOQPartyDataListNew?partyType=' + $scope.partyType;
        }

        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchBOQByParty, value: $scope.searchBOQParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.newpartyList = response.data;
        });
        //}
        angular.element(document.querySelector('#boqpartyPopUp')).modal('show');
    };

    $scope.closeBOQPartyPopUp = function (x) {
        var party = x.data;
        $scope.SubmitPartyCode = party.Code;
        $scope.SubmitPartyName = party.UserName;
        $scope.SubmitPartyId = party.Id;
        $scope.SubmitPaymentTermId = party.PaymentTermId;
        $scope.SubmitCurrencyId = party.CurrencyId;
        getPartyPlantList();
        $scope.hideBOQPartyPopUp();
    };
    $scope.hideBOQPartyPopUp = function () {
        angular.element(document.querySelector('#boqpartyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };
    $scope.closeBOQPartyPopUpNew = function () {
        angular.element(document.querySelector('#boqpartyPopUp')).modal('hide');
    }
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
        if (baseService.isUndefinedOrNull($scope.SubmitContractId))
            $scope.SubmitContractId = '';
        $scope.SubmitContractId
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

    $scope.ConvertedDataRow = function (data,list, trnuomId) {
        var BaseUOMFactortemp = $.grep(list, function (item) {
            return item.Value === trnuomId;
        })[0].BaseUOMFactor;
        data.TransactionRate = data.TransactionRate * BaseUOMFactortemp;
        //data.TransactionQty = data.TransactionQty / BaseUOMFactortemp;
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
        , ToCurrencyRate: 1
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

    $scope.Clear = function () {
        $scope.product = {};
        $scope.productNew = { ToCurrencyRate: 1, BaseCurrencyId: $scope.baseCurrencyId, OrderSpecific: 'Yes', PartyType: $scope.partyType, FixedAssetOrInventory: 'Inventory', PlantId: $window.plantId, IsTradingPO: false };
        $scope.poBoqItemListNew = [];
        $scope.tempList = [];
        $scope.taxCategoryList = [];
        $scope.ActionPOBOQ = 'Save';
    };



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
    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.productNew.PartyCode = party.Code;
        $scope.productNew.PartyName = party.UserName;
        $scope.productNew.PartyId = party.Id;
        $scope.productNew.PaymentTermId = party.PaymentTermId;
        $scope.productNew.CurrencyId = party.CurrencyId;
        getPartyPlantList();
        $scope.GetCurrencyExchangeRateList();
        $scope.hidePartyPopUp();
    };

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


    $scope.TermsAndConditionGridList = [];
    $scope.LoadTermsAndConditionGrid = function (TermsAndConditionId, POId) {
        $scope.TermsAndConditionGridList = [];

        $scope.termandconditionURL = $scope.path + "GetTermsAndConditionsPOList";

        try {
            $http({
                method: 'POST',
                url: $scope.termandconditionURL,
                data: { 'TermsAndConditionMasterId': TermsAndConditionId, 'POId': POId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.TermsAndConditionGridList = [];
                $scope.TermsAndConditionGridList = response.data;
            });
        }
        catch (e) {
            ShowResult(e, 'failure');


        }
    }
    $scope.TermsAndConditionDetailGridList = [];
    $scope.LoadTermsAndConditionDetailGrid = function () {
        $scope.TermsAndConditionDetailGridList = [];

        $scope.termandconditiondetailURL = $scope.path + "GetTermsAndConditionsPODetailList";

        try {
            $http({
                method: 'POST',
                url: $scope.termandconditiondetailURL,
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.TermsAndConditionDetailGridList = [];
                $scope.TermsAndConditionDetailGridList = response.data;
            });
        }
        catch (e) {
            ShowResult(e, 'failure');


        }
    }
    $scope.LoadTermsAndConditionDetailGrid();

    $scope.getMatureDate = function (date, days) {
        if (baseService.isUndefinedOrNull(date)) return $scope.productNew.MatureDate = null;
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.productNew.MatureDate = $filter('date')(date, 'dd-MMM-yyyy');
    };

    $scope.NotificationSettingStatus = function () {

        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/NotificationSetting',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NotificationSetting = response.data;
            $scope.CheckedByStatusForNoti = $scope.NotificationSetting[0].RequiredChecking;
            $scope.ApprovedByStatusForNoti = $scope.NotificationSetting[0].RequiredApproval;
            if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === false) {
                $scope.productNew.labelCheckAndApproved = 'To be checked by';
                $scope.GetCheckedByAndApprovedBy1();
            }
            else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be approved by';
                $scope.GetCheckedByAndApprovedBy1();
            }
            else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be checked by';
                $scope.GetCheckedByAndApprovedBy1();
            }
            //else {
            //    $scope.productNew.labelCheckAndApproved = 'To be checked/approved by';
            //}

        });
    };
    $scope.NotificationSettingStatus();
    $scope.GetCheckedByAndApprovedBy1 = function () {
        if (!baseService.isUndefinedOrNull($scope.CheckedByStatusForNoti) && !baseService.isUndefinedOrNull($scope.ApprovedByStatusForNoti)) {
            $http({
                method: 'GET',
                url: 'Products/PurchaseOrder/GetCheckedByAndApprovedBY?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.checkedByList = response.data;
            });

        }
        else {

        }

    }

    $scope.submit = function () {
        $scope.productNew.PartyCode = $scope.SubmitPartyCode;
        $scope.productNew.PartyName = $scope.SubmitPartyName;
        $scope.productNew.PartyId = $scope.SubmitPartyId;
        $scope.productNew.PaymentTermId = $scope.SubmitPaymentTermId;
        $scope.productNew.CurrencyId = $scope.SubmitCurrencyId;
        $scope.productNew.ContractId = $scope.SubmitContractId;
        $scope.productNew.ContractNo = $scope.SubmitContractNo;
        $scope.productNew.CustomerName = $scope.SubmitCustomerName;
        $scope.isSubmitted = 'Yes';
        $scope.submitBOQItem();

    }
    $scope.Back = function () {
        $scope.isSubmitted = 'No';

    };
    $scope.poBoqItemListNew = [];
    $scope.tempList = [];
    $scope.submitBOQItem = function () {
        var poboqlist = [];
        for (var i = 0; i < $scope.poBoqItemList.length; i++) {
            poboqlist.push(Object.assign({}, $scope.poBoqItemList[i]));
        }

        try {
            $scope.poBoqItemListNew = [];
            $scope.tempList = [];

            for (var i = 0; i < poboqlist.length; i++) {
                if ((baseService.isUndefinedOrNull(poboqlist[i].TransactionQty) || poboqlist[i].TransactionQty === 0) && poboqlist[i].CheckedStatus === true) {
                    ShowResult('Enter the Selected  Material Qty', 'failure');
                    return false;
                }
                if (poboqlist[i].CheckedStatus === true) {
                    if ((parseFloat(poboqlist[i].TransactionQty) + parseFloat(poboqlist[i].OtherPOQty)) > parseFloat(poboqlist[i].RequiredQtyPO)) {
                        ShowResult('Trasaction qty can not grater than booking Qty', 'failure');
                        poboqlist[i].TransactionQty = '';
                        return false;
                    }
                    if (baseService.isUndefinedOrNull(poboqlist[i].TransactionQty)) {
                        ShowResult('Enter the current Qty.Zero not allowed', 'failure');
                        $scope.isSubmitted = 'No';
                        return false;
                    }
                    if (poboqlist[i].TransactionQty < 0) {
                        ShowResult('Negative Qty  not allowed', 'failure');
                        $scope.isSubmitted = 'No';
                        return false;
                    }
                    if (poboqlist[i].BalanceQty < poboqlist[i].TransactionQty) {
                        ShowResult('Transaction QTY can not grater than Balance BOQ Qty', 'failure');
                        $scope.isSubmitted = 'No';
                        return false;
                    }

                    if (poboqlist[i].TransactionQty === 0 || poboqlist[i].TransactionQty === 0.00 || poboqlist[i].TransactionQty === 0.0) {
                        ShowResult('Enter the current Qty.Zero not allowed', 'failure');
                        return false;
                        $scope.isSubmitted = 'No';
                    }
                    if (baseService.isUndefinedOrNull(poboqlist[i].TransactionRate)) {
                        ShowResult('Enter the current rate.Zero not allowed', 'failure');
                        $scope.isSubmitted = 'No';
                        return false;
                    }
                    if (poboqlist[i].TransactionRate === 0 || poboqlist[i].TransactionRate === 0.0 || poboqlist[i].TransactionRate === 0.00) {
                        ShowResult('Enter the current rate.Zero not allowed', 'failure');
                        $scope.isSubmitted = 'No';
                        return false;
                    }
                    if (poboqlist[i].RequiredQtyApproved === 'No') {
                        ShowResult('Required Qty not yet Approved.So you can not take this material', 'failure');
                        $scope.isSubmitted = 'No';
                        return false;
                    }
                    if (poboqlist[i].IncompleteMaterial === 'Yes') {
                        ShowResult('This is incomplete material.So you can not take this material', 'failure');
                        $scope.isSubmitted = 'No';
                        return false;
                    }

                    else {
                        if ($scope.isSubmitted == 'Yes') {
                            var getRow = $filter("filter")($scope.poBoqItemListNew, {
                                "MaterialMasterId": poboqlist[i].MaterialMasterId, "ArticleId": poboqlist[i].ArticleId
                                , "FirstCharacteristicsValueId": poboqlist[i].FirstCharacteristicsValueId
                                , "FirstCharacteristicsValue": poboqlist[i].FirstCharacteristicsValue
                                , "SecondCharacteristicsValueId": poboqlist[i].SecondCharacteristicsValueId
                                , "SecondCharacteristicsValue": poboqlist[i].SecondCharacteristicsValue
                                , "ThitrdCharacteristicsValueId": poboqlist[i].ThitrdCharacteristicsValueId
                                , "GroupId": poboqlist[i].GroupId
                            });

                            if (getRow.length == 0) {
                                poboqlist[i].TrnAmount = Math.round((poboqlist[i].TransactionQty * poboqlist[i].TransactionRate) * 100 + Number.EPSILON) / 100
                                poboqlist[i].TransactionQty = Math.round((poboqlist[i].TransactionQty) * 100 + Number.EPSILON) / 100

                                if (baseService.isUndefinedOrNull(poboqlist[i].BaseTaxAmount)) {
                                    poboqlist[i].BaseAmount = poboqlist[i].TrnAmount + 0;
                                }
                                else {
                                    poboqlist[i].BaseAmount = poboqlist[i].TrnAmount + poboqlist[i].BaseTaxAmount;
                                }

                                $scope.poBoqItemListNew.push(poboqlist[i]);
                            }
                            else {
                                for (var j = 0; j < $scope.poBoqItemListNew.length; j++) {
                                    var row = $scope.poBoqItemListNew[j];
                                    if (row.MaterialMasterId == getRow[0].MaterialMasterId
                                        && row.ArticleId == getRow[0].ArticleId
                                        && row.FirstCharacteristicsValueId == getRow[0].FirstCharacteristicsValueId
                                        && row.FirstCharacteristicsValue == getRow[0].FirstCharacteristicsValue
                                        && row.SecondCharacteristicsValueId == getRow[0].SecondCharacteristicsValueId
                                        && row.SecondCharacteristicsValue == getRow[0].SecondCharacteristicsValue
                                        && row.ThitrdCharacteristicsValueId == getRow[0].ThitrdCharacteristicsValueId
                                        && row.GroupId == getRow[0].GroupId
                                    ) {
                                        var currentqty = Math.round(poboqlist[i].TransactionQty * 100 + Number.EPSILON) / 100;
                                        var currentamt = Math.round((poboqlist[i].TransactionQty * poboqlist[i].TransactionRate) * 100 + Number.EPSILON) / 100;
                                        $scope.poBoqItemListNew[j].TransactionQty = Math.round($scope.poBoqItemListNew[j].TransactionQty * 100 + Number.EPSILON) / 100 + currentqty;
                                        $scope.poBoqItemListNew[j].TrnAmount = Math.round($scope.poBoqItemListNew[j].TrnAmount * 100 + Number.EPSILON) / 100 + currentamt;
                                        currentqty = 0;
                                        currentamt = 0;
                                    }

                                }
                            }

                            for (var a = 0; a < $scope.poBoqItemList.length; a++) {
                                if ($scope.poBoqItemList[a].MaterialMasterId == poboqlist[i].MaterialMasterId
                                    && $scope.poBoqItemList[a].ArticleId == poboqlist[i].ArticleId
                                    && $scope.poBoqItemList[a].FirstCharacteristicsValueId == poboqlist[i].FirstCharacteristicsValueId
                                    && $scope.poBoqItemList[a].FirstCharacteristicsValue == poboqlist[i].FirstCharacteristicsValue
                                    && $scope.poBoqItemList[a].SecondCharacteristicsValueId == poboqlist[i].SecondCharacteristicsValueId
                                    && $scope.poBoqItemList[a].ThitrdCharacteristicsValueId == poboqlist[i].ThitrdCharacteristicsValueId
                                    && $scope.poBoqItemList[a].GroupId == poboqlist[i].GroupId
                                    && $scope.poBoqItemList[a].BOQId == poboqlist[i].BOQId
                                ) {
                                    $scope.tempList.push($scope.poBoqItemList[a]);
                                }
                            }
                        }

                    }
                }
            }
            $scope.UOMValidation();
            $scope.groupList = [];
            //$scope.processgroupList($scope.GetListForMasterOrdernew, $scope.groupList);
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

    $scope.Validation = function () {
        if (baseService.isUndefinedOrNull($scope.productNew.PartyId)) {
            ShowResult('Please select Vendor', 'failure');
            return true;
        }
        if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === false && $scope.productNew.CheckedBy == null) {
            ShowResult('Please select Checked By', 'failure');
            return true;
        }
        else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && $scope.productNew.CheckedBy == null) {
            ShowResult('Please select Approved By', 'failure');
            return true;
        }
        else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && $scope.productNew.CheckedBy == null) {
            ShowResult('Please select Checked By', 'failure');
            return true;
        }

        //if (baseService.isUndefinedOrNull($scope.productNew.DeliveryDate)) {
        //    ShowResult('Please Input DeliveryDate', 'failure');
        //    return true;
        //}

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
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $filter('dateFiltering')($scope.productNew.DocDate) + "&currencyId=" + $scope.productNew.CurrencyId
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
        try {
            $scope.UOMValidation();
            for (var i = 0; i < $scope.poBoqItemListNew.length; i++) {
                if ($scope.poBoqItemListNew[i].DeliveryDate == null || $scope.poBoqItemListNew[i].DeliveryDate == "undefined") {
                    ShowResult("Delivery Date is required", 'failure');
                    throw "";
                }
            }
            if ($scope.ActionPOBOQ === 'Save') {
                if (!$scope.UOMValidation() && !$scope.Validation()) {//$scope.invalid &&

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
                            ShowResult(response.data.Message, 'failure');
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.productNew.Id = response.data.entity.Id;
                            $scope.productNew.PartyName = $scope.product.PartyName;
                            $scope.LoadTermsAndConditionGrid($scope.productNew.TermsAndConditionsId, $scope.productNew.Id);
                            $scope.LoadTermsAndConditionDetailGrid();
                            $scope.Action = "Update";
                            $scope.ActionPOBOQ = "Update";
                            $scope.getalldata();
                            $scope.poBoqItemList = [];
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial');
                    };

                }
            }

            else if ($scope.ActionPOBOQ === "Update") {
                //$scope.materialValidationForBOQItem();
                if (!$scope.UOMValidation() /*&& !$scope.trnRateDiff()*/) {
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
                            ShowResult(response.data.Message, 'failure');
                        else {
                            ShowResult(response.data.Message, 'success');
                            getInventoryMaterialList($scope.productNew.Id);
                            $scope.poBoqItemList = [];
                            $scope.getalldata();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };

                }
            }

        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };
    $scope.materialValidationForBOQItem = function () {
        for (var i = 0; i < $scope.GetListForMasterOrdernew.length; i++) {
            var getRow3 = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.GetListForMasterOrdernew[i].MaterialMasterId, "ArticleId": $scope.GetListForMasterOrdernew[i].ArticleId, "FirstCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].ThirdCharacteristicsValueId });

            if (getRow3 == 0) {
                $scope.invalid = true;
            }
            else {
                ShowResult('Material Combination Already Exist', 'failure', 'ListOfPOMaterial');
                $scope.invalid = false;
            }
        }


    };
    $scope.serviceChargePopUp = function () {
        $scope.productNew.TaxOptionService = 'Yes';
        if (baseService.arrayLength($scope.poBoqItemListNew) === 0)
            return ShowResult('Without material charges not aplicable.');
        $scope.serviceModel = {
            Id: null
            , ServiceMasterId: null
            , InventoryReceiveId: $scope.productNew.Id
            , CurrencyName: angular.element("#currency :selected").text()
            , CurrencyId: $scope.productNew.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.productNew.DocDate
            , TransactionAmount: null
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
        var HSNCode = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCode;
        getTaxCategoryList(hsnCodeId, HSNCode);
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
    function getTaxCategoryList(hsnCodeId, HSNCode) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.productNew.PODate
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
            for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.taxCategoryList[i].hsnCodeId)) {
                    $scope.taxCategoryList[i].HSNCode = HSNCode;
                    $scope.taxCategoryList[i].HSNCodeId = hsnCodeId;
                    //$scope.HSNCode = HSNCode;
                }
            }
        });
    }
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
                        , TransactionAmount: null
                        , BaseAmount: 0
                        , TotalTaxAmount: 0
                        , ToCurrencyRate: $scope.productNew.ToCurrencyRate
                        , IsNonCreditable: $scope.productNew.IsNonCreditable
                    };
                    $scope.taxCategoryList = [];
                    getServiceChargeList($scope.productNew.Id);
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                    $scope.getalldata();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
            };
        } catch (e) {
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

    $scope.getServiceTaxList = function (data, flag, ServiceId, index) {


        $scope.LoadTaxButtonClick();
        $scope.Currency = $("#currency option:selected").text();
        $scope.ServiceId = ServiceId;
        $scope.taxAbleAmnt = data.Amount;//+ data.TotalTaxAmount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.receiveTaxList = [];
        if (data.ChargeTaxList.length > 0) {
            $scope.HSNCode = data.ChargeTaxList[0].HSNCode;
            $scope.receiveTaxList = data.ChargeTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        $scope.productNew.TaxOptionServiceModify = 'Yes';
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');
    }
    $scope.GetServiceTaxData = function (masterId) {
        $scope.ChargeTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetServiceTaxList?serviceId=' + $scope.productNew.Id
        }).then(function (response) {
            $scope.ChargeTaxList = response.data;
            for (var i = 0; i < $scope.chargesList.length; i++) {
                var linepk1 = $scope.chargesList[i].Id;
                var list1 = gettaxlist1(linepk1);
                $scope.chargesList[i].ChargeTaxList = list1;
            }
        });
    };
    function gettaxlist1(linepk1) {
        var result1 = [];
        for (var i = 0; i < $scope.ChargeTaxList.length; i++) {
            if ($scope.ChargeTaxList[i].InventoryServiceId === linepk1) {
                result1.push($scope.ChargeTaxList[i]);
            }
        }
        return result1;
    }
    function getServiceChargeList(inveReveiveId) {
        $scope.chargesList = [];
        $http.get($scope.path + 'GetServiceChargeList?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.chargesList = response.data;
                //$scope.ServiceId = $scope.chargesList[0].Id;
                $scope.GetServiceTaxData();
            });
    }

    $scope.serviceChargePopUpEdit = function (Id, Amount, TotalTaxAmount) {
        if (baseService.arrayLength($scope.poBoqItemListNew) === 0)
            return ShowResult('Without material charges not aplicable.');

        for (var i = 0; i < $scope.chargesList.length; i++) {
            for (var t = 0; t < $scope.chargesList[i].ChargeTaxList.length; t++) {
                $scope.receiveTaxList.push($scope.chargesList[i].ChargeTaxList[t]);
            }
        }
        $scope.productNew.Id
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/UpdateServiceAndTax',
            data: {
                entity: $scope.chargesList,
                receiveTaxList: $scope.receiveTaxList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
        $scope.enable = true;
        $scope.MSAction = "Edit";
        $scope.serviceModel = {
            Id: null
            , ServiceMasterId: null
            , InventoryReceiveId: $scope.productNew.Id
            , CurrencyName: angular.element("#currency :selected").text()
            , CurrencyId: $scope.productNew.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.productNew.DocDate
            , TransactionAmount: null
            , BaseAmount: 0
            , TotalTaxAmount: 0
            , ToCurrencyRate: $scope.productNew.ToCurrencyRate
            , IsNonCreditable: $scope.productNew.IsNonCreditable
        };

    };
    $scope.GetTerms = function (id) {
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/GetTerms?id=' + id
        }).then(function successCallback(response) {
            $scope.paymentTermList1 = response.data;
            $scope.productNew.DeliveryInstruction = $scope.paymentTermList1[0].DeliveryInstruction;
            $scope.productNew.SpecialInstruction = $scope.paymentTermList1[0].SpecialInstruction;
            //$scope.productNew.CheckedBy = $scope.paymentTermList1[0].CheckedBy;
        });
    }
    function getPartyPlantEditList(invoicingPartyPlantId, invoAddress, deliveryplant, deliAddress, deliState, deliGSTIN) {
        $scope.plantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.Value == invoicingPartyPlantId) {
                    //$scope.partyPlantId = item.Value;
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = deliveryplant;
                    $scope.productNew.InvoicingByAddress = invoAddress;
                    $scope.productNew.DeliveryByAddress = deliAddress;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = deliState;
                    $scope.productNew.DeliveryGSTIN = deliGSTIN;
                }
            });
        });
    }

    function getInventoryMaterialList(inveReveiveId) {
        $scope.masterId = inveReveiveId;

        $scope.poBoqItemListNew = [];
        $http.get($scope.path + 'GetInventoryMaterialList?inveReveiveId=' + inveReveiveId)
            .then(function (response) {

                $scope.poBoqItemListNew = ej.DataManager(response.data.Rows).executeLocal(ej.Query().sortBy("UserName desc"));//response.data.Rows;
                //var dataManagerObj = ej.DataManager(response.data.Rows).executeLocal(ej.Query().sortBy("UserName ASC"));
                $scope.DetailId = $scope.poBoqItemListNew[0].InventoryReceiveDetailId;
                $scope.InvoicingPartyPlantId = $scope.poBoqItemListNew[0].InvoicingPartyPlantId;
                $scope.productNew.InvoicingPartyPlantId = $scope.poBoqItemListNew[0].InvoicingPartyPlantId;
                $scope.productNew.InvoicingStateId = $scope.poBoqItemListNew[0].InvoicingStateId;
                $scope.productNew.PlantStateId = $scope.poBoqItemListNew[0].PlantStateId;
                checkSameValueInColumnList($scope.poBoqItemListNew, 'TransactionUoM');
                getGrossAmount($scope.poBoqItemListNew, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
                $scope.GetSalesTaxData();
            });

    }
    function getPOBOQMAPList(inveReveiveId) {
        $http.get($scope.path + 'GetPOBOQMAPList?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.tempList = response.data.Rows;
            });

    }

    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }
    function getGrossAmount(list, key1, key2, key3, fieldName) {
        $scope[fieldName] = 0;
        for (var t = 0; t < baseService.arrayLength(list); t++) {
            $scope[fieldName] += parseFloat(list[t][key1]);// + parseFloat(list[t][key2]) + parseFloat(list[t][key3]);
        }
    }
    $scope.GetSalesTaxData = function (salesId) {
        $scope.TaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + $scope.masterId
        }).then(function (response) {
            $scope.TaxList = response.data;

            for (var i = 0; i < $scope.poBoqItemListNew.length; i++) {
                var linepk = $scope.poBoqItemListNew[i].InventoryReceiveDetailId;
                var list = gettaxlist(linepk);
                $scope.poBoqItemListNew[i].TaxList = list;
            }
        });
    };
    function gettaxlist(linepk) {
        var result = [];
        for (var i = 0; i < $scope.TaxList.length; i++) {
            if ($scope.TaxList[i].PODetailId === linepk) {
                result.push($scope.TaxList[i]);
            }
        }
        return result;
    }

    $scope.ContractWiseData = function (Id) {

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/ContractWiseData?ContractId=' + Id
        }).then(function successCallback(response) { //datagatefun
            $scope.productNew.ContractNo = response.data[0].ContractNo;
            $scope.productNew.LCRef = response.data[0].LCRef;
        });
    };
    $scope.GetCheckedByAndApprovedBy1 = function () {
        if (!baseService.isUndefinedOrNull($scope.CheckedByStatusForNoti) && !baseService.isUndefinedOrNull($scope.ApprovedByStatusForNoti)) {
            $http({
                method: 'GET',
                url: 'Products/PurchaseOrder/GetCheckedByAndApprovedBY?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.checkedByList = response.data;
            });

        }
        else {

        }

    }
    // #endregion Service
    $scope.recorddoubleclick = function ($event) {

        var x = $event;
        var Id = x.data.Id;
        $scope.Currency = $("#currency option:selected").text();
        $scope.productNew = x.data;
        $scope.Id = $scope.productNew.Id;
        $scope.productNew.PODate = x.data.PODate1;
        $scope.GetTerms($scope.productNew.Id);
        getPartyPlantEditList($scope.productNew.InvoicingPartyPlantId, $scope.productNew.InvoicingByAddress, $scope.productNew.DeliveryPartyPlantId, $scope.productNew.DeliveryByAddress, $scope.productNew.DeliveryState, $scope.productNew.DeliveryGSTIN);
        getInventoryMaterialList($scope.productNew.Id);
        getPOBOQMAPList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
        $scope.productNew.OrderSpecific = 'Yes';
        $scope.isSubmitted = 'Yes'
        $scope.BOQItemDisabled = 'GridClick';
        if (baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = false;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = x.data.ApprovedById;
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = true;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = x.data.CheckedById;
        }
        $scope.ContractWiseData(x.data.ContractId);
        // $scope.ImagedataLoad($scope.productNew.Id);
        $scope.GetCheckedByAndApprovedBy1();
        if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {
            $scope.GetCheckedByAndApprovedBy1();
            $scope.productNew.CheckedBy = x.data.ApprovedById;
            $scope.productNew.labelCheckAndApproved = 'To be approved by';
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && baseService.isUndefinedOrNull(x.data.ApprovedById)) {
            $scope.GetCheckedByAndApprovedBy1();
            $scope.productNew.CheckedBy = x.data.CheckedById;
            $scope.productNew.labelCheckAndApproved = 'To be checked by';
        }
        $scope.LoadTermsAndConditionGrid($scope.productNew.TermsAndConditionsId, $scope.productNew.Id)
        $scope.Action = 'Update';
        $scope.ActionPOBOQ = 'Update';
        $scope.getPOBOQItemListS();
        if (!$rootScope.isCollapsed) $rootScope.toggle();


    };
    $scope.POWithTax = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/PurchaseOrder/GePurchaseOrderBOQReportWithTax?purchaseOrderBOQId=" + data.Id;
    };

    $scope.POWithoutTax = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/PurchaseOrder/GePurchaseOrderBOQReportWithoutTax?purchaseOrderBOQId=" + data.Id;
    };

    $scope.POBOQReportXl = function (data) {

        try {

            var file_src = 'Products/PurchaseOrder/POBOQReport?POID=' + data.Id;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }
    $scope.updatePOBOQListS = [];
    $scope.getPOBOQItemListS = function () {
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/GetPOBOQMapListForUpdateS?poId=' + $scope.productNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.updatePOBOQListS = response.data;
        });
        //angular.element(document.querySelector('#updatePOBOQPopUp')).modal('show');

    }
    $scope.MaterialModels = {};
    $scope.updatePOBOQList = [];
    $scope.getPOBOQItemList = function (data) {
        $scope.MaterialModels = data;
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/GetPOBOQMapListForUpdate?poId=' + $scope.productNew.Id + '&poDatailId=' + data.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.updatePOBOQList = response.data;
        });
        angular.element(document.querySelector('#updatePOBOQPopUp')).modal('show');

    }
    $scope.CloseupdatePOBOQPopUp = function () {
        angular.element(document.querySelector('#updatePOBOQPopUp')).modal('hide');

    }
    $scope.SaveOBOQPopUp = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.POBOQ1SaveUrl,
                data: {
                    updatePOBOQList: $scope.updatePOBOQList
                    , poBoqItemListNew: $scope.MaterialModels
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'updatePOBOQPopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'updatePOBOQPopUp');
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.CloseupdatePOBOQPopUp();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'updatePOBOQPopUp');
            };
        } catch (e) {
            ShowResult(e, 'info')
        }
    };
    $scope.UpdatepoBoqItemList = [];
    $scope.detailPopUp = function () {
        try {
            $http.get("Products/PurchaseOrder/GetPOBOQItems?ContractId=" + $scope.productNew.ContractId + '&VendorId=' + $scope.productNew.PartyId)
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.UpdatepoBoqItemList = response.data;
                            angular.element(document.querySelector('#AddMaterialPopUp')).modal('show');
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
    $scope.CloseMaterialPopUp = function () {
        angular.element(document.querySelector('#AddMaterialPopUp')).modal('hide');
    }
    $scope.UpdatesubmitBOQItem = function () {
        try {
            $scope.poBoqItemListNew;
            for (var i = 0; i < $scope.UpdatepoBoqItemList.length; i++) {
                if ($scope.UpdatepoBoqItemList[i].CheckedStatus) {
                    if ((parseFloat($scope.UpdatepoBoqItemList[i].TransactionQty) + parseFloat($scope.UpdatepoBoqItemList[i].OtherPOQty)) > parseFloat($scope.UpdatepoBoqItemList[i].RequiredQtyPO)) {
                        $scope.UpdatepoBoqItemList[i].TransactionQty = '';
                        throw "Trasaction qty can not grater than booking Qty";
                    }
                    if (baseService.isUndefinedOrNull($scope.UpdatepoBoqItemList[i].TransactionQty)) {
                        throw "Enter the current Qty.Zero not allowed";
                    }
                    if ($scope.UpdatepoBoqItemList[i].TransactionQty < 0) {
                        throw ('Negative Qty  not allowed');
                    }
                    if ($scope.UpdatepoBoqItemList[i].BalanceQty < $scope.UpdatepoBoqItemList[i].TransactionQty) {
                        throw ('Transaction QTY can not grater than Balance BOQ Qty');
                    }

                    if ($scope.UpdatepoBoqItemList[i].TransactionQty === 0 || $scope.UpdatepoBoqItemList[i].TransactionQty === 0.00 || $scope.UpdatepoBoqItemList[i].TransactionQty === 0.0) {
                        throw ('Enter the current Qty.Zero not allowed');
                    }
                    if (baseService.isUndefinedOrNull($scope.UpdatepoBoqItemList[i].TransactionRate)) {
                        throw ('Enter the current rate.Zero not allowed');
                    }
                    if ($scope.UpdatepoBoqItemList[i].TransactionRate === 0 || $scope.UpdatepoBoqItemList[i].TransactionRate === 0.0 || $scope.UpdatepoBoqItemList[i].TransactionRate === 0.00) {
                        throw ('Enter the current rate.Zero not allowed');
                    }
                    if ($scope.UpdatepoBoqItemList[i].RequiredQtyApproved === 'No') {
                        throw ('Required Qty not yet Approved.So you can not take this material');
                    }
                    if ($scope.UpdatepoBoqItemList[i].IncompleteMaterial === 'Yes') {
                        throw ('This is incomplete material.So you can not take this material');
                    }
                    else {
                        var Done = 0;
                        var getRow3 = $filter("filter")($scope.updatePOBOQListS, {
                            "BOQDetailId": $scope.UpdatepoBoqItemList[i].BOQId, "MaterialMasterId": $scope.UpdatepoBoqItemList[i].MaterialMasterId
                            , "ArticleId": $scope.UpdatepoBoqItemList[i].ArticleId, "FirstCharacteristicsValueId": $scope.UpdatepoBoqItemList[i].FirstCharacteristicsValueId
                                
                        });
                        if (getRow3.length > 0) {
                            throw "Already taken";
                        }
                        else {
                            $scope.UpdatepoBoqItemList[i].Id = null;
                            $scope.UpdatepoBoqItemList[i].TrnAmount = Math.round(($scope.UpdatepoBoqItemList[i].TransactionQty * $scope.UpdatepoBoqItemList[i].TransactionRate) * 100 + Number.EPSILON) / 100
                            $scope.poBoqItemListNew.push($scope.UpdatepoBoqItemList[i]);
                            Done = 1;
                            var getRow = $filter("filter")($scope.tempList, {
                                "MaterialMasterId": $scope.UpdatepoBoqItemList[i].MaterialMasterId, "ArticleId": $scope.UpdatepoBoqItemList[i].ArticleId
                                , "FirstCharacteristicsValueId": $scope.UpdatepoBoqItemList[i].FirstCharacteristicsValueId
                                , "SecondCharacteristicsValueId": $scope.UpdatepoBoqItemList[i].SecondCharacteristicsValueId
                                , "ThitrdCharacteristicsValueId": $scope.UpdatepoBoqItemList[i].ThitrdCharacteristicsValueId
                                , "GroupId": $scope.UpdatepoBoqItemList[i].GroupId
                            });
                            if (getRow.length == 0) {
                                $scope.tempList.push($scope.UpdatepoBoqItemList[i]);
                            }

                        }
                        if (Done == 1) {
                            angular.element(document.querySelector('#AddMaterialPopUp')).modal('hide');
                        }
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'info', 'AddMaterialPopUp');
        }
    };

    $scope.taxCategoryListcbo = [];
    $scope.LoadTaxButtonClick = function () {
        accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
            $scope.taxCategoryListcbo = result;
        });
    }

    $scope.closeReceiveTaxPopUp = function () { //hossain
        $scope.detailModel = {};
        $scope.receiveTaxList = [];
        $scope.detailModel.InventoryReceiveDetailId = $scope.currentInventoryReceiveDetailIdRow;
        $scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
        if ($scope.taxCategoryList.length > 0) {
            for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                $scope.receiveTaxList.push($scope.taxCategoryList[i]);
            }
        }

        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
            if (getRow.length == 2) {
                ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
                return false;
            }

            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxCategoryId)) {
                ShowResult("Select Tax Category.", 'failure', 'receiveTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].Percentage)) {
                ShowResult("Input Percentage.", 'failure', 'receiveTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxAmount)) {
                ShowResult("Input Tax Amount.", 'failure', 'receiveTaxPopUp');
                return false;
            }
        }
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/InsertExtraTax',
            data: {
                entity: $scope.detailModel
                , taxCategoryList: $scope.receiveTaxList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
            }
            else {
                ShowResult(response.data.Message, 'success', 'receiveTaxPopUp');
                angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
                //getInventoryMaterialList($scope.productNew.Id);
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
        };
    }

    $scope.getReceiveTaxList = function (data, flag, index, Id) {
        ;
        $scope.productNew.TaxOption = 'Yes';
        $scope.LoadTaxButtonClick();
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.receiveTaxList = [];
        if (data.TaxList.length > 0) {
            $scope.HSNCode = data.TaxList[0].HSNCode;
            if (baseService.isUndefinedOrNull(data.TaxList[0].HSNCode)) {
                $scope.HSNCode = data.HSNCode;
            }
            $scope.receiveTaxList = data.TaxList;
        }
        $scope.total = 0;
        $scope.taxCategoryList = [];
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.taxCategoryList.push($scope.receiveTaxList[j]);
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    };
    $scope.calculateTaxAmount = function (data) {
        data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
    };
    $scope.getTotalReceiveTaxList = function (amount, flag) {
        $scope.taxAbleAmnt = amount;
        $scope.percentageColumn = flag;
        $http({
            method: 'GET',
            url: $scope.path + 'GetTotalReceiveTaxList?receiveId=' + $scope.productNew.Id
        }).then(function (response) {
            $scope.receiveTaxList = response.data;

        });
        //angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    };
    $scope.closeReceiveTaxPopUpwindow = function () {
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }

    $scope.valuePassInDelModal = function (id, index) {
        $scope.id = id;
        $scope.deleteindexId = index;
        if (baseService.isUndefinedOrNull(id)) {
            var indexData = $scope.poBoqItemListNew[index];
            $scope.poBoqItemListNew.splice(index, 1);
            var i = $scope.updatePOBOQListS.length;
            while (i--) {
                if ($scope.updatePOBOQListS[i]["MaterialMasterId"] === indexData.MaterialMasterId
                    && $scope.updatePOBOQListS[i]["ArticleId"] === indexData.ArticleId
                    && $scope.updatePOBOQListS[i]["FirstCharacteristicsValueId"] === indexData.FirstCharacteristicsValueId
                    && $scope.updatePOBOQListS[i]["SecondCharacteristicsValueId"] === indexData.SecondCharacteristicsValueId
                    && $scope.updatePOBOQListS[i]["ThitrdCharacteristicsValueId"] === indexData.ThitrdCharacteristicsValueId
                    && $scope.updatePOBOQListS[i]["GroupId"] === indexData.GroupId) {
                    $scope.updatePOBOQListS.splice(i, 1);
                }
            }
        }
        else {
            $scope.message = 'Are you sure want to permanently delete this?';
            angular.element(document.querySelector('#rowDeletePopUp')).modal('show');
        }

    };
    $scope.detailDelete = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.detailDeleteUrl + $scope.id + '&OrderSpecific=' + $scope.productNew.OrderSpecific
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    {
                        var indexData = $scope.poBoqItemListNew[$scope.deleteindexId];
                        $scope.poBoqItemListNew.splice($scope.deleteindexId, 1);
                        var i = $scope.updatePOBOQListS.length;
                        while (i--) {
                            if ($scope.updatePOBOQListS[i]["MaterialMasterId"] === indexData.MaterialMasterId
                                && $scope.updatePOBOQListS[i]["ArticleId"] === indexData.ArticleId
                                && $scope.updatePOBOQListS[i]["FirstCharacteristicsValueId"] === indexData.FirstCharacteristicsValueId
                                && $scope.updatePOBOQListS[i]["SecondCharacteristicsValueId"] === indexData.SecondCharacteristicsValueId
                                && $scope.updatePOBOQListS[i]["ThitrdCharacteristicsValueId"] === indexData.ThitrdCharacteristicsValueId
                                && $scope.updatePOBOQListS[i]["GroupId"] === indexData.GroupId) {
                                $scope.updatePOBOQListS.splice(i, 1);
                            }
                        }
                        $scope.deleteindexId = null;

                    }
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };

    $scope.toleranceCalculate = function () {
        for (var t = 0; t < $scope.poBoqItemListNew.length; t++) {
            $scope.poBoqItemListNew[t].Tolerance = $scope.productNew.Tolerance;
        }
    }

    $scope.confirmDelete = function () {
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

    $scope.PoDelete = function () {
        if (baseService.arrayLength($scope.poBoqItemListNew) === 0) {
            if (!baseService.isUndefinedOrNull($scope.productNew.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.deleteUrl + $scope.productNew.Id,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult('Data Deleted Successfully', 'success');
                        $scope.getalldata();
                        $scope.Clear();
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

    $scope.refreshTemplatePOBOQ = function (args) {
        $("#headchk111").ejCheckBox({ "change": CheckBoxSelectAllPOBOQ });
    };

    function CheckBoxSelectAllPOBOQ(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPrint").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.poBoqItemList.length; i++) {
                $scope.poBoqItemList[i].CheckedStatus = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckedStatus = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPrint").data("ejGrid");
        gridObj.refreshContent();
    };
 
}//End Of main

