'use strict';
ServiceAcknowledgementController.$inject = ['accountService', 'addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function ServiceAcknowledgementController(accountService, addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Service Acknowledgement";
    $scope.Action = 'Save';
    $scope.DetailAction = 'Save';
    $scope.ActionService = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/PurchaseOrder/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getListUrl1 = $scope.path + 'GetListForGRNSaveData';
    $scope.getListUrl2 = $scope.path + 'GetListForGrnByPoReq';

    $scope.saveUrl = $scope.path + 'CreateIndependentServiceAcknowledge';
    $scope.detailSaveUrlIndependent = $scope.path + 'CreateIndependentServiceAcknowledgeDetail';
    $scope.updateUrl1 = $scope.path + 'UpdareGRN';

    //$scope.saveUrl = $scope.path + 'InsertGRN';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete?receiveDetailId=';
    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.sreviceSaveUrl1 = $scope.path + 'ServiceChargesCreates';
    $scope.sreviceUpdateUrl = $scope.path + 'ServiceChargesUpdate';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';

    $scope.updateUrlForSerPOAckTaxValue = $scope.path + 'UpdateServicePOAckTax';

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


    $scope.NotificationSettingStatus = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/ServicePOAcknowledgementNotificationSetting',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NotificationSetting = response.data;
            $scope.CheckedByStatusForNoti = $scope.NotificationSetting[0].RequiredChecking;
            $scope.ApprovedByStatusForNoti = $scope.NotificationSetting[0].RequiredApproval;
            $scope.GetCheckedByAndApprovedBy1();
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
                url: 'Products/PurchaseOrder/GetCheckedByAndApprovedBYServicePOAcknowledgement?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.checkedByList = response.data;
            });

        }
        else {

        }
    }

    //#endregion


    $scope.getListPOByReqG = [];
    $scope.getListServiceApprovedPO = function () {
        //debugger;
        var PoType = 'POByReq';
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetServiceApprovedPO',
        }).then(function successCallback(response) {
            $scope.getListPOByReqG = response.data;
            $scope.productNew.AcknowledgementDate = $filter("dateFiltering")(Date.now());
        });
    };
    $scope.POPopUpGRNPOReqList = function () {
        $scope.productNew.PO = 'PO';
        $scope.getListServiceApprovedPO();
        angular.element(document.querySelector('#POPopUp')).modal('show');

    };


    $scope.ServiceMasterList = [];
    $scope.getServiceMasterPopUpData = function () {
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/GetServiceMasterServiceControlData',
        }).then(function successCallback(response) {
            $scope.ServiceMasterList = response.data;
        });
        angular.element(document.querySelector('#ServiceMasterpopUp')).modal('show');

    };
    $scope.closeServicePopUP = function () {
        angular.element(document.querySelector('#ServiceMasterpopUp')).modal('hide');

    }
    $scope.selectServiceMaster = function () {
        var gridObj = $("#ServiceMasterGrid").data("ejGrid");
        var $event = gridObj.getSelectedRecords()[0];
        var x = $event;
        $scope.serviceModel.ServiceName = x.ServiceMaster;
        $scope.serviceModel.ServiceMasterId = x.ServiceMasterId;
        $scope.serviceModel.TransactionUoMId = x.TransactionUoMId;
        getTaxCategoryList(x.HSNCodeId, x.HSNCode);
        $scope.closeServicePopUP();
    }

    $scope.taxCategoryList = [];
    function getTaxCategoryList(hsnCodeId, HSNCode) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: 'Products/PurchaseOrder/GetTaxCategoryListServiceAcknowledgement?serviceId=' + $scope.ServiceAckId + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
            for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.taxCategoryList[i].hsnCodeId)) {
                    $scope.taxCategoryList[i].HSNCode = HSNCode;
                    $scope.taxCategoryList[i].HSNCodeId = hsnCodeId;
                }
            }
        });
    }


    $scope.closePartyPopUp = function (x) {
        var party = x.data;
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
        $scope.hidePartyPopUp();
    };

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

    $scope.POPopUpClose = function () {
        angular.element(document.querySelector('#POPopUp')).modal('hide');
    };
    $scope.GriddataSelected = [];

    $scope.ServicePOAndAckTax = [];
    function GetServicePOAndAckTax(inveReveiveId) {
        $scope.masterId1 = inveReveiveId;
        $http.get('Products/PurchaseOrder/getServicePOTaxForAckSave?POID=' + inveReveiveId)
            .then(function (response) {
                $scope.ServicePOAndAckTax = [];
                $scope.ServicePOAndAckTax = response.data;

                //$scope.GetPOServiceTaxData();
            });
    }

    $scope.chargesListPO = [];
    function getServiceChargeListPO(inveReveiveId) {
        $scope.masterId1 = inveReveiveId;
        $http.get('Products/PurchaseOrder/GetServiceListByServicePO?servicepoid=' + inveReveiveId)
            .then(function (response) {
                $scope.chargesListPO = [];
                $scope.chargesListPO = response.data;
                //$scope.GetPOServiceTaxData();
            });
    }


    $scope.checkedByList = [];
    $scope.ServicePOAcknowledgementCheckedBy = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/ServicePOAcknowledgementCheckedBy'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
    }
    $scope.ServicePOAcknowledgementCheckedBy();
    $scope.ApprovedByList = [];
    $scope.ServicePOAcknowledgementApproveBy = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/ServicePOAcknowledgementApproveBy'
        }).then(function successCallback(response) {
            $scope.ApprovedByList = response.data;
        });
    }
    $scope.ServicePOAcknowledgementApproveBy();

    $scope.GriddataSelected = [];
    $scope.GetSavedPOListNew = [];
    $scope.GetSavedPOList1 = function (Id) {
        //debugger;
        var PoType = 'PO';
        $scope.GriddataSelected = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetSavedPOList1?AckId=' + Id,
        }).then(function successCallback(response) {
            //$scope.GetSavedPOListNew = [];
            $scope.GetSavedPOListNew = response.data;
            for (var i = 0; i < $scope.GetSavedPOListNew.length; i++) {

                $scope.GriddataSelected.push($scope.GetSavedPOListNew[i]);
            }

        });
    };

    $scope.product = {
        Id: null
        , AcknowledgementDate: $filter("dateFiltering")(Date.now())
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
        , labelCheckAndApproved: null
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionService: 'Yes'
        , TaxOptionServiceModify: 'Yes'
        , TaxOptionAddiTax: 'Yes'
        , GateEntryNo: null
        , GateEntryDate: null
    };
    $scope.productNew = Object.assign({}, $scope.product);
    $scope.productNew.TaxOptionService = 'Yes';
    $scope.advanceTax = {
        TaxCodeId: null,
        Text: null,
        TaxAmount: null,
        ValueOfFixed: null,
        CompanyCurrencyAmount: null,
        Type: null
        , TotalSumAfterTCSVal: null
    };
    //#region Index tab and dataloadfunction


    $scope.tabType = "ForChecking";
    $scope.GriddataMaster = [];
    $scope.getalldataMaster = function () {
        if ($scope.tabType === "ForChecking") {
            $scope.tabType = "ForChecking";
        }
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetListIndependentServiceAcknowledgementData?tabType=' + $scope.tabType,
        }).then(function successCallback(response) {
            $scope.GriddataMaster = response.data;
        });
    };
    $scope.getalldataMaster();



    $scope.GriddataMaster2 = [];
    $scope.getalldataMaster2 = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetListForGrnByPoReq?GRNWithReqPOApprovedStatus=' + $scope.GRNWithReqPOApprovedStatus,
            // url: $scope.getListUrl2,
        }).then(function successCallback(response) {
            $scope.GriddataMaster2 = response.data;

        });
    };



    $scope.GRN = "";
    //$scope.tab = 1;
    $scope.tabGL = 1;
    //debugger;
    $scope.tabType = "ForChecking";
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
        $scope.tabGL = newTab;
        $scope.tabType = "CheckedHoldReject";
        $scope.getalldataMaster();

    };
    $scope.isSetCheckedHoldReject = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 2;

    };
    $scope.setTabNotApprovedChecked = function (newTab) {
        $scope.tabGL = newTab;
        $scope.tabType = "Checked";
        $scope.getalldataMaster();

    };
    $scope.isSetNotApprovedChecked = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 3;

    };

    $scope.setTabApprovedHoldReject = function (newTab) {
        $scope.tabGL = newTab;
        $scope.tabType = "ApprovedHoldReject";
        $scope.getalldataMaster();

    };
    $scope.isSetApprovedHoldReject = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 4;

    };



    $scope.setTabApprovedNotPosted = function (newTab) {
        $scope.tabGL = newTab;
        $scope.tabType = "Approved";
        $scope.getalldataMaster();

    };
    $scope.isSetApprovedNotPosted = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 5;

    };


    $scope.setTabPosted = function (newTab) {
        $scope.tabGL = newTab;
        $scope.tabType = "Posted";
        $scope.getalldataMaster();

    };
    $scope.isSetPosted = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 6;

    };


    //#endregion


    $scope.Save = function () {
        try {
            if ($scope.productNew.NoteForAccounts === '' || $scope.productNew.NoteForAccounts === null || $scope.productNew.NoteForAccounts === undefined) {
                ShowResult("Enter Note for accounts", 'failure');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
            if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
            $scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
            $scope.modelValidation('div_docDate', 'productNew', 'DocDate');
            if ($scope.Action === 'Update')
                $scope.modelValidation('div_grnNo', 'productNew', 'Id');
            $scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');

            if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId)
                $scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');
            else
                manualValidation('div_rate', false);

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {
                $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                $scope.product = Object.assign({}, $scope.productNew);
                $scope.product.POId = $scope.POId;
                // $scope.product.Id = null;
                if ($scope.Action === "Save") {

                    $scope.chargesListPOnew = [];
                    for (var i = 0; i < $scope.chargesListPO.length; i++) {
                        if (isNaN($scope.chargesListPO[i].CurrentQty) || baseService.isUndefinedOrNull($scope.chargesListPO[i].CurrentQty))
                            $scope.chargesListPO[i].CurrentQty = 0;

                        if ($scope.chargesListPO[i].check == false && $scope.chargesListPO[i].CurrentQty > 0) {
                            ShowResult('Please check line item', 'failure');
                            return false;
                        }

                        if ($scope.chargesListPO[i].check == true && baseService.isUndefinedOrNull($scope.chargesListPO[i].CurrentQty)) {
                            ShowResult('Enter the qty for check line', 'failure');
                            return false;
                        }
                        if ($scope.chargesListPO[i].check == true && $scope.chargesListPO[i].CurrentQty === 0) {
                            ShowResult('Enter the qty for checked line', 'failure');
                            return false;
                        }
                        if ($scope.chargesListPO[i].check == true) {
                            if ($scope.Action === "Save") {
                                if ($scope.chargesListPO[i].Qty < Math.round(($scope.chargesListPO[i].CurrentQty + $scope.chargesListPO[i].OtherReceived) * 100 + Number.EPSILON) / 100) {
                                    ShowResult('Current Receive can not grater than balance', 'failure');
                                    $scope.chargesListPO[i].CurrentQty = '';
                                    return false;
                                }
                            }
                            if (baseService.isUndefinedOrNull($scope.chargesListPO[i].CurrentQty) || $scope.chargesListPO[i].CurrentQty === 0) {
                                ShowResult('Current Receive can not be Zero(0)', 'failure');
                                $scope.chargesListPO[i].CurrentQty = '';
                                return false;
                            }

                            $scope.chargesListPOnew.push($scope.chargesListPO[i]);
                            //$scope.chargesListPOnew.push($scope.chargesList[i]);
                        }
                        //else if ($scope.chargesList[i].check == true) {                           
                        //    $scope.chargesListPOnew.push($scope.chargesList[i]);
                        //}
                        else {

                        }
                    }
                    //debugger;
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data:
                        {
                            'entity': $scope.product,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti,
                            'Status': 'Save'

                        },
                        dataType: 'JSON'
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.productNew.Id = response.data.entity.Id;
                            $scope.productId = response.data.entity.Id;
                            $scope.productNew.PartyName = $scope.product.PartyName;
                            $scope.tabType = "ForChecking";
                            $scope.getalldataMaster();
                            $scope.Action = "Update";
                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }

            }
        } catch (e) {
            throw e;
        }
    };

    $scope.DeleteServiceAckRow = function (id) {
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/DeleteServiceAckRow?Id=' + id,
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult('Delete Error', 'failure');
            else {
                ShowResult('Deleted Service line Successfully', 'success');
                getServiceDetailList($scope.ServiceAckId);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
        //else
        //    ShowResult('First delete all line item.', 'failure');
    };

    $scope.Delete = function () {
        //debugger;
        if (baseService.arrayLength($scope.chargesListPO) === 0) {
            if (!baseService.isUndefinedOrNull($scope.productId)) {
                $http({
                    method: 'POST',
                    url: 'Products/PurchaseOrder/DeleteServiceAck?Id=' + $scope.productId,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult("Not Deleted", 'failure');
                    else {
                        ShowResult('Delete Successfully', 'success');
                        $scope.getalldataMaster();
                        $scope.tempServiceDetailId = null;
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

    $scope.removeDetailRow = function (id) {
        $scope.tempServiceDetailId = id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#serviceDetailDeletePopUp')).modal('show');
    };

    $scope.Clear = function () {
        //debugger;
        $scope.chargesListPO = [];
        $scope.productId = "";
        $scope.GriddataSelected = [];

        $scope.inventoryMaterialListPOnew = [];

        ClearFields();
        return true;
    };

    function ClearFields() {

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
            //, POId: $scope.product.POId            
            , AcknowledgementDate: $filter("dateFiltering")(Date.now())
            , TaxOption: 'Yes'
            , TaxOptionMat: 'Yes'
            , TaxOptionService: 'Yes'
            , TaxOptionServiceModify: 'Yes'
            , TaxOptionAddiTax: 'Yes'
        };
        $scope.NotificationSettingStatus();

        //$scope.productNew.TaxOptionService = 'Yes';

        baseService.removeErrorClasses();
        //$scope.getToCurrencyRate();
    }


    $scope.recorddoubleclickFromMasterGrid = function ($event) {
        //debugger;

        ClearFields();
        var x = $event;
        var Id = x.data.Id;
        $scope.POId1 = x.data.POID;
        $scope.POID = x.data.POID;
        $scope.productNew = x.data;
        $scope.productNew.CheckedBy = x.data.CheckedBy;
        getPartyPlantList();
        $scope.productNew.TaxOptionAddiTax = 'Yes';
        $scope.ServiceAckId = Id;
        getServiceDetailList($scope.ServiceAckId);
        if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {
            $scope.CheckedByStatusForNoti = false;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = x.data.ApprovedById;
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {
            $scope.CheckedByStatusForNoti = true;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = x.data.CheckedById;
        }

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
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    }
    $scope.chargesList = [];
    function getServiceDetailList(serviceAckId) {
        $http.get($scope.path + 'GetServiceLisrByAckid?Id=' + serviceAckId)
            .then(function (response) {
                $scope.chargesList = response.data;
                getACKTaxList(serviceAckId);
            });
    }

    $scope.ServiceTaxList = [];
    function getACKTaxList(Id) {
        $scope.ServiceTaxList = [];
        $http.get($scope.path + 'getServicePOAckTax?Id=' + Id)
            .then(function (response) {
                $scope.ServiceTaxList = response.data;
            });
    }

    function getPartyPlantList() {
        //debugger;
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

    $scope.ServiceTaxPopUpList = [];
    $scope.getServiceTaxPopUp = function (data, index) {
        $scope.taxAbleAmnt = data.Amount;
        $scope.HSNCode = data.HSNCode;
        $scope.ServiceTaxPopUpList = [];
        for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
            if ($scope.ServiceTaxList[i].ServiceAcknowledgementDetailId == data.ServicePODetailId) {
                $scope.ServiceTaxPopUpList.push($scope.ServiceTaxList[i]);
            }
        }
        angular.element(document.querySelector('#ServiceTaxPopUp')).modal('show');
    }
    $scope.CloseServiceTaxPopUp = function () {
        angular.element(document.querySelector('#ServiceTaxPopUp')).modal('hide');
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.modelValidation = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
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



    //#region Service Detail
    $scope.lst = [];
    $scope.ServiceListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/LoadAllAckServicesData'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            window.lst = response.data;

        });
    }
    $scope.ServiceListDetails();

    $scope.PODocumentMapDataAll = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/PurchaseOrder/ServicePOACKDocumentMapDataAll'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.Img = response.data;

        });
    }
    $scope.PODocumentMapDataAll();
    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("ServiceAcknowledgementMasterId", "equal", parseInt(filteredData), true).take(20000));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: ["Id", "ServiceName", "Qty", "UoM", "Rate", "Amount", "Code", "TotalTaxAmount", "TotalAmount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();

        var dataImg = ej.DataManager(window.Img).executeLocal(ej.Query().where("ServiceAcknowledgementMasterId", "equal", parseInt(filteredData), true).take(1000));
        e.detailsElement.find("#detailGrid1").ejGrid({
            dataSource: dataImg,
            columns: [{ field: "UserFilename", headerText: "UserFilename", width: 100 },
            { field: "Description", headerText: "Description", width: 100 },
            { field: "Remarks", headerText: "Remarks", width: 100 }

            ]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    //#endregion
    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = " PurchaseOrder/ServiceAcknowledgementReport?SurviceAckId=" + data.Id;
    };






    $scope.tab1 = 1;
    //$scope.GRNbyPOCheckStatus = "ForChecked";
    $scope.productNew.PO = "PO";
    $scope.status = "PO";
    $scope.setTabGRNPOList = function (newTab12) {
        $scope.GriddataSelected = [];

        $scope.AcceptanceId = '';
        $scope.Clear();
        $scope.productId = "";
        $scope.productNew.PO = "PO";
        $scope.status = "PO";
        $scope.tab1 = newTab12;
    };
    $scope.isSetGRNLPOist = function (tabNum12) {
        return $scope.tab1 === tabNum12;
        //$scope.GRN = 1;

    };


    //#region Document Upload
    $scope.DocDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.UserFilename;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ExpensesDocument + '/' + data.Id + extention;
    };

    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });
    document.getElementById("uploadBtn").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };

    $scope.DocumentSave = function () {
        debugger;
        //$scope.$broadcast("show-errors-check-validity");

        if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
            throw $scope.filedata.name + ' File size must be below 2 mb';
        var fileName = null;
        if (!baseService.isUndefinedOrNull($scope.filedata))
            fileName = $scope.filedata.name;
        $scope.productDocMap.UserFilename = fileName;
        $scope.productDocMap.POId = $scope.productNew.Id;
        if (baseService.isUndefinedOrNull($scope.productDocMap.UserFilename)) {
            ShowResult('Select Attachment file');
            return false;
        }
        if (!baseService.isUndefinedOrNull($scope.productDocMap.UserFilename)) {
            if ($scope.productDocMap.UserFilename.length > 50) {
                throw "File Name must be less than 50 character.";
            }
        }
        for (var i = 0; i < $scope.Imagedata.length; i++) {
            var getRow = $filter("filter")($scope.Imagedata, { "UserFilename": $scope.productDocMap.UserFilename });
            if (getRow.length === 1) {
                ShowResult('File Already added');
                return false;
            }
        }

        try {

            var formData = new FormData();

            $http({
                method: "POST",
                url: 'Products/PurchaseOrder/ServicePOACKDocCreate',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("PODocumentMap", angular.toJson($scope.productDocMap));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: {
                    "PODocumentMap": $scope.productDocMap,
                    "file": $scope.filedata,
                    "POId": $scope.productNew.Id,
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.ImagedataLoad($scope.productId);
                    $scope.productDocMap.UserFilename = "";
                    $scope.productDocMap.Description = "";
                    $scope.productDocMap.Remarks = "";
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            throw ShowResult(e, "failure");
        }

        return true;
    };
    $scope.Imagedata = [];
    $scope.ImagedataLoad = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseOrder/ServicePOACKDocumentMapData?POID=' + $scope.productId,
        }).then(function successCallback(response) { //datagatefun
            $scope.Imagedata = response.data;

        });
    };
    $scope.removePopUpForDoc = function (Id) {
        $scope.DocId = Id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removePopUpForDoc')).modal('show');
    };
    $scope.DeletePOIgame = function (Id) {

        if (!baseService.isUndefinedOrNull($scope.DocId)) {
            $http({
                method: 'POST',
                url: 'Products/PurchaseOrder/ServicePOACKImageDelete?Id=' + $scope.DocId,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ImagedataLoad($scope.productId);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }


    };

    //#endregion 



    $scope.POPopUpGateEntry = function () {
        $scope.getalldataGateEntry();
        angular.element(document.querySelector('#POPopUpGateEntry')).modal('show');
    };
    $scope.POPopUpCloseGateEntry = function () {
        angular.element(document.querySelector('#POPopUpGateEntry')).modal('hide');
    };
    $scope.GriddataGateEntry = [];
    $scope.getalldataGateEntry = function () {
        //debugger;
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
        $scope.productNew.GateEntryDate = x.data.EntryDate;

        $scope.POPopUpCloseGateEntry();
    }



    $scope.delModal = function (id) {
        //debugger;
        if (baseService.arrayLength($scope.chargesList) > 0) {
            if (!baseService.isUndefinedOrNull($scope.productId)) {
                $http({
                    method: 'POST',
                    url: 'Products/PurchaseOrder/DeleteServiceAckChargesRow?Id=' + id,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult('Delete Error', 'failure');
                    else {
                        ShowResult('Deleted Service line Successfully', 'success');
                        getServiceChargeList($scope.productId);
                        getServiceChargeListForCharge($scope.productId)
                        //ClearFields();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
            }
        }
    }

    // #region Service
    $scope.serviceChargePopUp = function () {
        $scope.productNew.TaxOptionService = 'Yes';
        $scope.serviceModel = {
            Id: null
            , ServiceMasterId: null
            , CurrencyName: angular.element("#currency :selected").text()
            , CurrencyId: $scope.productNew.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.productNew.DocDate
            , TransactionAmount: null
            , BaseAmount: 0
            , TotalTaxAmount: 0
            , ToCurrencyRate: $scope.productNew.ToCurrencyRate
            , IsNonCreditable: $scope.productNew.IsNonCreditable
            , ServicePOMasterId: null
            , ServiceMasterId: null
            , Amount: null
            , GRNServiceAmount: null
            , AmountStatus: null
            , Description: null
            , ServiceRequsitionDetailId: null
            , ServiceReqMasterId: null
            , Rate: 0
            , Qty: 0
            , TransactionUoMId: null
        };
        angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
    };

    $scope.closeServiceChargePopUp = function () {
        $scope.serviceModel = {};
        $scope.receiveTaxList = [];
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    };


    $scope.calculateRate = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        if (baseService.isUndefinedOrNull($scope.serviceModel.Qty)) {
            $scope.serviceModel.Qty = 0;
        }
        //item.TaxAmount = Math.round((data.TrnAmount * item.Percentage / 100) * 100 + Number.EPSILON) / 100;
        $scope.serviceModel.Rate = Math.round(($scope.serviceModel.TransactionAmount / $scope.serviceModel.Qty) * 100 + Number.EPSILON) / 100;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
    };
    //$scope.calculateTrnRate = function () {

    //    $scope.serviceModel.TotalTaxAmount = 0;
    //    if (baseService.isUndefinedOrNull($scope.serviceModel.Qty)) {
    //        $scope.serviceModel.Qty = 0;
    //    }
    //    $scope.serviceModel.TransactionAmount = Math.round(($scope.serviceModel.Qty * $scope.serviceModel.Rate) * 100 + Number.EPSILON) / 100;
    //    for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
    //        $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
    //        $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
    //    }
    //    if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
    //};
    //$scope.calculateSvcTaxCategory = function () {
    //    $scope.serviceModel.TotalTaxAmount = 0;
    //    for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
    //        $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
    //        $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
    //    }
    //    if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
    //};
    //$scope.sumSvcTaxAmount = function () {
    //    $scope.serviceModel.TotalTaxAmount = 0;
    //    for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
    //        $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
    //    }
    //};


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
        //$scope.taxAbleAmnt = data.TransactionAmount;
        //$scope.taxAmnt = data.TaxAmount;

        $scope.receiveTaxList = [];
        if (data.ChargeTaxList.length > 0) {
            $scope.HSNCode = data.ChargeTaxList[0].HSNCode;
            $scope.receiveTaxList = data.ChargeTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');

    }
    //Load2
    $scope.GetServiceTaxData = function (masterId) {
        //
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
        //for (var i = 0; i < $scope.TaxList.length; i++) {
        //    if ($scope.TaxList[i].PODetailId === linepk) {
        //        result.push($scope.TaxList[i]);
        //    }
        //}

        for (var i = 0; i < $scope.ChargeTaxList.length; i++) {
            if ($scope.ChargeTaxList[i].InventoryServiceId === linepk1) {
                result1.push($scope.ChargeTaxList[i]);
            }
        }
        return result1;
    }



    $scope.serviceChargePopUpEdit = function (Id, Amount, TotalTaxAmount) {
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0)
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

        //}
        //else {

        //}

    };
    // #endregion Service
    $scope.uom = function () {

        cboService.getUoMCbo(function (response) {
            $scope.uoMList = response;
        });
    }
    $scope.uom();

    $scope.Addclick = function () {


        if (baseService.isUndefinedOrNull($scope.serviceModel.Qty) || $scope.serviceModel.Qty === 0) {
            ShowResult('Enter the Qty');
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.serviceModel.TransactionUoMId)) {
            ShowResult('Select The UoM');
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.serviceModel.Rate) || $scope.serviceModel.Rate === 0) {
            ShowResult('Enter the Rate');
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.serviceModel.TransactionAmount) || $scope.serviceModel.TransactionAmount === 0) {
            ShowResult('Enter the Qty and Rate');
            return false;
        }
        $scope.chargesList.push($scope.serviceModel);
    }
    $scope.DetailSaveIndividualService = function () {
        //debugger;  
        try {

            if (baseService.isUndefinedOrNull($scope.serviceModel.Qty) || $scope.serviceModel.Qty === 0) {
                ShowResult('Enter the Qty');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.serviceModel.TransactionUoMId)) {
                ShowResult('Select The UoM');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.serviceModel.Rate) || $scope.serviceModel.Rate === 0) {
                ShowResult('Enter the Rate');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.serviceModel.TransactionAmount) || $scope.serviceModel.TransactionAmount === 0) {
                ShowResult('Enter the Qty and Rate');
                return false;
            }
            //$scope.materialValidation();
            $scope.serviceModel.Amount = $scope.serviceModel.TransactionAmount;
            $scope.serviceModel.Amount = $scope.serviceModel.TransactionAmount;
            $scope.Isvalid = true;
            if ($scope.Isvalid) {
                if ($scope.DetailAction === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.detailSaveUrlIndependent,
                        data: {
                            'ServiceAckId': $scope.productNew.Id,
                            'ackDetailModel': $scope.serviceModel,
                            'servicePOAckTax': $scope.taxCategoryList
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true)
                            ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                        else {
                            ShowResult(response.data.Message, 'success', 'serviceChargePopUp');
                            getServiceDetailList($scope.productNew.Id);
                            $scope.RequisitionListHide();
                            $scope.setTabIndex(1);
                            $scope.serviceModel = {};
                            $scope.taxCategoryList = [];
                            angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                    };

                }

            }
        } catch (e) {
            ShowResult(e, 'failure', 'ListOfRequisition');
        }
    };

}