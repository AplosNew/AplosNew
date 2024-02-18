'use strict';
ServicePoAcknowledgementController.$inject = ['accountService', 'addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function ServicePoAcknowledgementController(accountService, addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Service Acknowledgement";
    $scope.Action = 'Save';
    $scope.ActionService = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/PurchaseOrder/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getListUrl1 = $scope.path + 'GetListForGRNSaveData';
    $scope.getListUrl2 = $scope.path + 'GetListForGrnByPoReq';

    $scope.saveUrl = $scope.path + 'CreateServiceAcknowledge';
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




    //#region notification setting for Service Requisition

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
            //else {
            //    $scope.productNew.labelCheckAndApproved = 'To be checked/approved by';
            //}

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
            $scope.productNew.AcknowledgementDate = $filter("dateFiltering")(Date.now());
            //$scope.product.POId = $scope.POId;
            var id1 = "''";
            for (var i = 0; i < $scope.getListPOByReqG.length; i++) {
                if ($scope.getListPOByReqG[i].Active === true) {
                    id1 += ",'" + $scope.getListPOByReqG[i].id + "'";
                }
            }

            getPartyPlantList();
            getServiceChargeListPO(id1);
            GetServicePOAndAckTax(id1);//x.id
            //getPartyPlantEditList();
            // GetInventoryMaterialListByPO(id1);

            $scope.GriddataSelected = [];
            for (var x = 0; x < $scope.getListPOByReqG.length; x++) {

                if ($scope.getListPOByReqG[x].Active === true) {
                    $scope.GriddataSelected.push($scope.getListPOByReqG[x]);
                }
            }
            $scope.NotificationSettingStatus();
            $scope.POPopUpClose();
            if (!$rootScope.isCollapsed) $rootScope.toggle();
        }


    }
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
            url: 'Products/PurchaseOrder/GetListServiceAcknowledgementData?tabType=' + $scope.tabType,
        }).then(function successCallback(response) {
            // url: $scope.getListUrl1,
            $scope.GriddataMaster = response.data;
            //entrydata = copy(searchdata);
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
        //debugger;
        try {
            if ($scope.productNew.NoteForAccounts === '' || $scope.productNew.NoteForAccounts === null || $scope.productNew.NoteForAccounts === undefined) {
                ShowResult("Enter Note for accounts", 'failure');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
            if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
            $scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
            $scope.modelValidation('div_docDate', 'productNew', 'DocDate');
            //$scope.modelValidation('div_entryNo', 'productNew', 'GateEntryNo');
            //$scope.modelValidation('div_entryDate', 'productNew', 'EntryDate', 'Gate Entry Date');
            if ($scope.Action === 'Update')
                $scope.modelValidation('div_grnNo', 'productNew', 'Id');
            //$scope.modelValidation('div_grnDate', 'productNew', 'AcknowledgementDate');

            $scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');

            if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId)
                $scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');
            else
                manualValidation('div_rate', false);

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {
                //if (new Date($scope.productNew.EntryDate) < new Date($scope.productNew.DocDate))
                //    return manualValidation('div_entryDate', true, "Gate entry date can't be less than Doc Date");
                //else
                //    manualValidation('div_entryDate', false);
                //if (new Date($scope.productNew.AcknowledgementDate) < new Date($scope.productNew.EntryDate))
                //    return manualValidation('div_grnDate', true, "GRN date can't be less than gate entry date");
                //else
                //    manualValidation('div_grnDate', false);

                $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                $scope.product = Object.assign({}, $scope.productNew);
                $scope.product.POId = $scope.POId;
                // $scope.product.Id = null;
                if ($scope.Action === "Save") {
                    //for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                    //    if ($scope.inventoryMaterialListPO[i].check == true) {
                    //        $scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialListPO[i]);

                    //    }
                    //    //else if ($scope.inventoryMaterialList[i].check == true) {                           
                    //    //    $scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialList[i]);
                    //    //}
                    //    //else {
                    //    //    ShowResult('Please select Material', 'failure');
                    //    //    break;
                    //    //}
                    //}
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
                            'DetailList': $scope.chargesListPOnew,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti,
                            'Status': 'Save',
                            'ServicePOAndAckTax': $scope.ServicePOAndAckTax

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
                            $scope.ServiceListDetails();
                            getServiceChargeList($scope.productId);
                            getACKTaxList($scope.productId);
                            $scope.Action = "Update";
                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {
                    $scope.chargesListPOnew = [];
                    for (var i = 0; i < $scope.chargesListPO.length; i++) {
                        if ($scope.chargesListPO[i].check == true) {
                            $scope.chargesListPOnew.push($scope.chargesListPO[i]);
                            $scope.tabType = "ForChecking";
                            $scope.getalldataMaster();
                            //$scope.chargesListPOnew.push($scope.chargesList[i]);
                        }
                        //else if ($scope.chargesList[i].check == true) {                           
                        //    $scope.chargesListPOnew.push($scope.chargesList[i]);
                        //}
                        else {

                        }
                    }
                    $scope.productNew.Id = $scope.productId;

                    $http({
                        method: 'POST',
                        //url: $scope.updateUrl,
                        url: $scope.saveUrl,
                        //data: $scope.product,
                        data:
                        {
                            'entity': $scope.product,
                            'DetailList': $scope.chargesListPOnew,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti,
                            'Status': 'Update',
                            'ServicePOAndAckTax': $scope.ServicePOAndAckTax
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getalldataMaster();
                            $scope.ServiceListDetails();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            throw e;
        }
    };
    $scope.DeleteServiceAckRow = function (x) {
        //debugger;
        if (baseService.arrayLength($scope.chargesListPO) > 0) {
            if (!baseService.isUndefinedOrNull($scope.productId)) {
                $http({
                    method: 'POST',
                    url: 'Products/PurchaseOrder/DeleteServiceAckRow?Id=' + x.ServicePODetailId,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult('Delete Error', 'failure');
                    else {
                        ShowResult('Deleted Service line Successfully', 'success');
                        getServiceChargeList($scope.productId);
                        //ClearFields();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
            }
        }
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

    $scope.Clear = function () {
        //debugger;
        $scope.chargesListPO = [];
        $scope.productId = "";
        $scope.GriddataSelected = [];
        $scope.productDocMap = {
            UserFilename: null
            , Description: null
            , Remarks: null
        };

        $scope.Imagedata = [];
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

        //debugger;
        //$scope.POId = x.data.POID;
        $scope.POId1 = x.data.POID;
        //$scope.index = index;
        $scope.POID = x.data.POID;
        // $scope.product = $scope.products[$scope.index];
        //$scope.productNew = Object.assign({}, $scope.product);
        $scope.productNew = x.data;
        //$scope.productNew.AcknowledgementDate = x.data.GRNDate1;
        $scope.productNew.CheckedBy = x.data.CheckedBy;
        getPartyPlantList();
        //getInventoryMaterialList(Id);
        $scope.productNew.TaxOptionAddiTax = 'Yes';
        getServiceChargeList(Id);
        getServiceChargeListForCharge(Id);
        getACKTaxList(Id);
        $scope.productId = Id;
        $scope.GetSavedPOList1(Id);
        $scope.ImagedataLoad(Id);
        $scope.TotalSumAfterTCS();
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

        //getServiceChargeListForCharge($scope.productNew.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();

    }


    function getServiceChargeList(inveReveiveId) {
        $scope.masterId12 = inveReveiveId;
        //debugger;
        $http.get($scope.path + 'GetServiceLisrByAckid?Id=' + inveReveiveId)
            .then(function (response) {
                //$scope.chargesList = [];
                $scope.chargesListPO = response.data;
                // $scope.getServiceTaxList();
                $scope.GetAdvanceTaxInfo($scope.productId);
                $scope.TotalSumAfterTCS();

            });
    }

    function getACKTaxList(Id) {

        //debugger;
        $http.get($scope.path + 'getServicePOAckTax?Id=' + Id)
            .then(function (response) {
                $scope.receiveTaxList1 = [];
                $scope.ServicePOAndAckTax = [];
                $scope.receiveTaxList1 = response.data;
                // $scope.getServiceTaxList();
                for (var i = 0; i < $scope.receiveTaxList1.length; i++) {
                    $scope.ServicePOAndAckTax.push($scope.receiveTaxList1[i]);

                }


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
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
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



    $scope.setTabGRNAcceptance = function (newTab12) {

        $scope.Clear();
        $scope.productId = '';
        $scope.productNew.PO = "Acceptance";
        $scope.status = "Acceptance";
        $scope.GriddataSelected = [];
        $scope.tab1 = newTab12;
    };
    $scope.isSetGRNAcceptance = function (tabNum12) {
        return $scope.tab1 === tabNum12;
        //$scope.GRN = 2;
    };








    $scope.Griddata = [];
    $scope.getalldata = function () {

        //debugger;
        var PoType = 'PO';

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetListOfPO?PoType=' + PoType + '&Status=' + $scope.status,
        }).then(function successCallback(response) {
            $scope.Griddata = response.data;
            $scope.productNew.GRNDate = $filter("dateFiltering")(Date.now());
        });
    };

    $scope.POPopUpNew = function () {
        $scope.getalldata();
        //debugger
        $scope.status === 'PO';
        if ($scope.status === 'PO') {
            $scope.status === 'PO';
            //alert('1');
            $scope.getalldata();
        }
        else if ($scope.status === 'Acceptance') {
            $scope.status === 'Acceptance';
            $scope.getalldata();
        }
        angular.element(document.querySelector('#POPopUp1')).modal('show');

    };



    $scope.POPopUpCloseNew = function () {
        //debugger;
        angular.element(document.querySelector('#POPopUp1')).modal('hide');

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
        if (angular.isUndefinedOrNull($scope.productNew.Id))
            ShowResult('Please select/save PO first', 'Error');
        else {
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
    $scope.LoadTaxButtonClick = function () {
        accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
            $scope.taxCategoryList = result;
        });
    }
    $scope.getServicePOTaxList = function (data, flag, index, Id) {
        debugger;
        if ($scope.Action === "Save") {
            $scope.productNew.TaxOptionServiceModify = 'Yes';
            $scope.LoadTaxButtonClick();
            $scope.Currency = $("#currency option:selected").text();
            $scope.currentMaterialRow = index;
            $scope.currentInventoryReceiveDetailIdRow = Id;
            $scope.taxAbleAmnt = data.TotalAmount;
            $scope.percentageColumn = flag;
            $scope.currentMaterialRow = index;
            $scope.ServiceMasterName = data.ServiceMasterName;
            $scope.receiveTaxList = [];
            if ($scope.ServicePOAndAckTax.length > 0) {
                for (var i = 0; i < $scope.ServicePOAndAckTax.length; i++) {
                    if ($scope.ServicePOAndAckTax[i].ServicePoDetailId === data.ServicePODetailId) {
                        $scope.HSNCode = $scope.ServicePOAndAckTax[0].HSNCode;
                        $scope.receiveTaxList.push($scope.ServicePOAndAckTax[i]);

                    }
                }

            }
        }
        else {
            $scope.productNew.TaxOptionServiceModify = 'Yes';
            $scope.LoadTaxButtonClick();
            $scope.Currency = $("#currency option:selected").text();
            $scope.currentMaterialRow = index;
            $scope.currentInventoryReceiveDetailIdRow = Id;
            $scope.taxAbleAmnt = data.TotalAmount;
            $scope.percentageColumn = flag;
            $scope.currentMaterialRow = index;
            $scope.ServiceMasterName = data.ServiceMasterName;
            if ($scope.receiveTaxList1.length > 0) {
                $scope.receiveTaxList = [];
                for (var i1 = 0; i1 < $scope.receiveTaxList1.length; i1++) {
                    if ($scope.receiveTaxList1[i1].ServiceAcknowledgementDetailId === data.ServicePODetailId) {
                        $scope.HSNCode = $scope.receiveTaxList1[0].HSNCodeId;

                        $scope.receiveTaxList.push($scope.receiveTaxList1[i1]);

                    }
                }

            }
        }

        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    };
    $scope.closegetServicePOTaxList = function () {
        var TotalTax = null;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            TotalTax += parseFloat($scope.receiveTaxList[i].TaxAmount);
        }
        $scope.chargesListPO[$scope.currentMaterialRow].TotalTaxAmount = TotalTax;
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }
    $scope.UpdateServicePOAckTax = function () {
        $http({
            method: 'POST',
            url: $scope.updateUrlForSerPOAckTaxValue,
            data:
            {
                'ServiceAcknowledgementMasterId': $scope.productId,
                'UserSendData': $scope.receiveTaxList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                getServiceChargeList($scope.productId);
                getServiceChargeListForCharge($scope.productId);
                angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.calculateTaxAmountForService = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.serviceModel.TransactionAmount * data.Percentage) / 100;
    };
    $scope.checkRowValidationService = function (x) {
        debugger;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            if ($scope.receiveTaxList[i].Id === x.Id) {
                $scope.receiveTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100);
            }

        }
    }

    //#region Additional Code
    $scope.advanceTaxesList = [];
    $scope.additionalTax = function () {
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
            $scope.advanceTax = {};
            $scope.TotalSumAfterTCS();
        }

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
                    $scope.taxCodCboListWithhold = response.data;
                }
            },
            function errorCallback(response) {
            });
    };
    $scope.getTaxCodeByTaxYearWithhold($filter("dateFiltering")(Date.now()));
    $scope.selectadditionalTax = function () {
        $scope.advanceTax.ValueOfFixed = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].ValueOfFixed;
        $scope.advanceTax.Type = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].Type;
        if ($scope.advanceTax.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.advanceTax.ValueOfFixed)) {//* $scope.advanceTax.ValueOfFixed / 100
            //$scope.advanceTax.TaxAmount = (parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax")) * $scope.advanceTax.ValueOfFixed / 100);

            $scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.chargesListPO), "Amount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TotalTaxAmount"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
        }
        $scope.TotalSumAfterTCS();
    }

    $scope.SaveAdditinalTaxInGRNList = function () {
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/SaveServiceAcknowledgementAdditionalTax',
            data:
            {
                'InventoryReceiveId': $scope.productNew.Id,
                'UserSendData': $scope.advanceTaxesList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
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
            url: 'Products/PurchaseOrder/GetServiceAcknowledgementAdditionalTaxInfo?ServicePOAckMasterId=' + Id,
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
            url: 'Products/PurchaseOrder/ServiceAcknowledgementAdditionalTaxInfoDelete?Id=' + Id,
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
        $scope.TaxAmountVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax"))).toFixed(2);
        $scope.advanceTax.TaxAmount = (($scope.TaxAmountVal * data) / 100).toFixed(2);

    };
    $scope.checkRowValidationSdditionalTax = function (data) {
        debugger;
        $scope.TaxAmountVal1 = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax"))).toFixed(2);
        $scope.advanceTax.ValueOfFixed = ((data / $scope.TaxAmountVal1) * 100).toFixed(4);
    }
    //$scope.TotalSumAfterTCSVal = "";
    $scope.TotalSumAfterTCS = function () {
        //$scope.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);
        $scope.advanceTax.TotalSumAfterTCSVal = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.chargesListPO), "Amount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesListPO), "TotalTaxAmount"))) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount")))).toFixed(2);
    }

    //#endregion



    ////#region ServiceAcknowledgement Register Report


    //$scope.PurchaseRegisterLst = [];
    //$scope.pivotTableFieldListID = [];
    //$scope.GetPurchaseRegister = function () {
    //    debugger;

    //    if ($scope.report.FromDate === null || $scope.report.FromDate === "") {
    //        ShowResult('Select From Date', 'failure');
    //        return false;
    //    }
    //    else if ($scope.report.ToDate === null || $scope.report.ToDate === "") {
    //        ShowResult('Select To Date', 'failure');
    //        return false;
    //    }
    //    $http({
    //        method: 'POST',
    //        //url: $scope.getSearchListUrl,
    //        url: 'Materials/MaterialLedger/GetServiceAcknowledgementRegister',
    //        data: {
    //            fromDate: $scope.report.FromDate,
    //            toDate: $scope.report.ToDate,
    //            Type: $scope.productNew.Type
    //        },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.PurchaseRegisterLst = response.data;

    //        for (var i = 0; i < $scope.PurchaseRegisterLst.length; i++) {
    //            response.data[i].GRNEntryDate = new Date($scope.PurchaseRegisterLst[i].GRNEntryDate);
    //        }

    //        $scope.load();
    //    });

    //};

    //$scope.getPurchaseRegisterReport = function () {
    //    $scope.GetPurchaseRegister();
    //}


    //$scope.ServiceAcknowledgementRegisterReportPdf = function (id, reportFormat) {

    //    if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
    //        ShowResult('Select From Date', 'failure');
    //        return false;
    //    }
    //    if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
    //        ShowResult('Select To Date', 'failure');
    //        return false;
    //    }
    //    var reportFormat = "Pdf";
    //    //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
    //    $window.open('Materials/MaterialLedger/ServiceAcknowledgementRegisterReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type, '_blank');
    //};

    //$scope.ServiceAcknowledgementRegisterReportExcel = function (reportFormat) {
    //    if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
    //        ShowResult('Select From Date', 'failure');
    //        return false;
    //    }
    //    if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
    //        ShowResult('Select To Date', 'failure');
    //        return false;
    //    }
    //    try {
    //        var Excel;
    //        var file_src = 'Materials/MaterialLedger/ServiceAcknowledgementRegisterReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Asset=' + $scope.productNew.WithStock + '&Inventory=' + $scope.productNew.WithoutStock;
    //        $rootScope.report(file_src);

    //    } catch (e) {

    //    }
    //}calculateAckRcvAmount


    ////#endregion ServiceAcknowledgement Register Report

    $scope.calculateAckRcvAmount = function (data) {
        if ($scope.Action === 'Save') {

            for (var i = 0; i < $scope.chargesListPO.length; i++) {
                if ($scope.chargesListPO[i].Qty < Math.round(($scope.chargesListPO[i].CurrentQty + $scope.chargesListPO[i].OtherReceived) * 100 + Number.EPSILON) / 100) {
                    ShowResult('Current Receive can not grater than balance', 'failure');
                    $scope.chargesListPO[i].CurrentQty = '';
                    return false;
                }

                if ($scope.chargesListPO[i].ServiceMasterId === data.ServiceMasterId && $scope.chargesListPO[i].ServicePODetailId === data.ServicePODetailId) {
                    $scope.chargesListPO[i].Amount = Math.round(($scope.chargesListPO[i].CurrentQty * $scope.chargesListPO[i].Rate) * 100 + Number.EPSILON) / 100;
                    if ($scope.ServicePOAndAckTax.length > 0) {
                        for (var i1 = 0; i1 < $scope.ServicePOAndAckTax.length; i1++) {
                            if ($scope.ServicePOAndAckTax[i1].ServicePoDetailId === data.ServicePODetailId) {
                                //$scope.HSNCode = $scope.ServicePOAndAckTax[0].HSNCode;
                                $scope.ServicePOAndAckTax[i1].TaxAmount = Math.round(($scope.chargesListPO[i].Amount * ($scope.ServicePOAndAckTax[i1].Percentage / 100)) * 100 + Number.EPSILON) / 100;
                            }
                        }

                    }
                }

                if ($scope.chargesListPO[i].ServicePODetailId === data.ServicePODetailId) {
                    $scope.chargesListPO[i].TotalTaxAmount = $filter("sumByKey")($filter("filter")($scope.ServicePOAndAckTax, { "ServicePoDetailId": data.ServicePODetailId }), "TaxAmount");

                    if (isNaN(data.CurrentQty)) data.CurrentQty = 0;
                    if (isNaN(data.OtherReceived)) data.OtherReceived = 0;
                    $scope.chargesListPO[i].Balance = (data.Qty - (data.OtherReceived + data.CurrentQty));
                    if (isNaN($scope.chargesListPO[i].Balance))
                        $scope.chargesListPO[i].Balance = 0;

                }
                if ($scope.productNew.IsNonCreditable == 1) {
                    if ($scope.chargesListPO[i].ServicePODetailId === data.ServicePODetailId) {
                        $scope.chargesListPO[i].TotalAmount = Math.round($scope.chargesListPO[i].Amount + $scope.chargesListPO[i].TotalTaxAmount * 100 + Number.EPSILON) / 100;
                    }

                }
                else {
                    if ($scope.chargesListPO[i].ServicePODetailId === data.ServicePODetailId) {
                        $scope.chargesListPO[i].TotalAmount = $scope.chargesListPO[i].Amount;
                    }

                }
            }
        }
        else {
            for (var i = 0; i < $scope.chargesListPO.length; i++) {
                if ($scope.chargesListPO[i].Qty < Math.round(($scope.chargesListPO[i].CurrentQty + $scope.chargesListPO[i].OtherReceived) * 100 + Number.EPSILON) / 100) {
                    ShowResult('Current Receive can not grater than balance', 'failure');
                    $scope.chargesListPO[i].CurrentQty = '';
                    return false;
                }

                if ($scope.chargesListPO[i].ServiceMasterId === data.ServiceMasterId) {
                    $scope.chargesListPO[i].Amount = Math.round(($scope.chargesListPO[i].CurrentQty * $scope.chargesListPO[i].Rate) * 100 + Number.EPSILON) / 100;
                    if ($scope.ServicePOAndAckTax.length > 0) {
                        for (var i1 = 0; i1 < $scope.ServicePOAndAckTax.length; i1++) {
                            if ($scope.ServicePOAndAckTax[i1].ServiceAcknowledgementDetailId === data.ServicePODetailId) {
                                //$scope.HSNCode = $scope.ServicePOAndAckTax[0].HSNCode;
                                $scope.ServicePOAndAckTax[i1].TaxAmount = Math.round(($scope.chargesListPO[i].Amount * ($scope.ServicePOAndAckTax[i1].Percentage / 100)) * 100 + Number.EPSILON) / 100;
                            }
                        }

                    }
                }

                if ($scope.chargesListPO[i].ServicePODetailId === data.ServicePODetailId) {
                    $scope.chargesListPO[i].TotalTaxAmount = $filter("sumByKey")($filter("filter")($scope.ServicePOAndAckTax, { "ServiceAcknowledgementDetailId": data.ServicePODetailId }), "TaxAmount");
                    $scope.chargesListPO[i].Balance = (data.Qty - (data.OtherReceived + data.CurrentQty));
                }
                if ($scope.productNew.IsNonCreditable == 1) {
                    if ($scope.chargesListPO[i].ServicePODetailId === data.ServicePODetailId) {
                        $scope.chargesListPO[i].TotalAmount = Math.round(($scope.chargesListPO[i].Amount + $scope.chargesListPO[i].TotalTaxAmount) * 100 + Number.EPSILON) / 100;
                    }

                }
                else {
                    if ($scope.chargesListPO[i].ServicePODetailId === data.ServicePODetailId) {
                        $scope.chargesListPO[i].TotalAmount = $scope.chargesListPO[i].Amount;
                    }
                }
            }
        }
    }
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
    $scope.serviceChargePopUp = function () {
        $scope.productNew.TaxOptionService = 'Yes';
        $scope.taxCategoryList = null;
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
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    };

    $scope.taxCategoryList = [];
    function getTaxCategoryList(hsnCodeId, HSNCode) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: 'Products/PurchaseOrder/getserviceTaxByTaxCategoryList?receiveId=' + $scope.GriddataSelected[0].id + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.productNew.AcknowledgementDate
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
    $scope.changeService = function () {
        if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
            return getTaxCategoryList(hsnCodeId);//$scope.taxCategoryList = [];
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

    $scope.AddCharges = function () {
        $scope.chargesListPO.push($scope.serviceModel)
        for (var i = 0; i < $scope.taxCategoryList.length; i++) {
            $scope.receiveTaxList.push($scope.taxCategoryList[i]);
        }
        $scope.serviceModel = {};
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    };
    $scope.serviceSave = function () {
        try {
            if ($scope.ActionService == 'Save') {

                $scope.manualValidationAddRemove('div_svc', 'serviceModel', 'ServiceMasterId');
                $scope.manualValidationAddRemove('div_svcRate', 'serviceModel', 'TransactionAmount', 'Amount');
                if (!baseService.isUndefinedOrNull($scope.serviceModel.InventoryReceiveId)) {
                    $scope.serviceModel.ServiceAcknowledgementMasterId = $scope.serviceModel.InventoryReceiveId;
                }
                $http({
                    method: 'POST',
                    url: $scope.sreviceSaveUrl1,
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
                        //getInventoryMaterialList($scope.productNew.Id);
                        getServiceChargeListForCharge($scope.productNew.Id);
                        //$scope.getDataList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                };
            }
            else {
                $http({
                    method: 'POST',
                    url: $scope.sreviceUpdateUrl,
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
                        //getInventoryMaterialList($scope.productNew.Id);
                        getServiceChargeListForCharge($scope.productNew.Id);
                        //$scope.getDataList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                };
            }
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
    $scope.chargesList = [];
    function getServiceChargeListForCharge(MasterId) {
        $http.get($scope.path + 'GetServiceChargeListForCharge?MasterId=' + MasterId)
            .then(function (response) {
                $scope.chargesList = response.data;
            });
    }
    $scope.getServiceTaxList1 = function (data, flag) {
        //debugger;
        $scope.ActionService = 'Update';
        $scope.productNew.TaxOptionService = 'Yes';
        $scope.taxAbleAmnt = data.Amount;// + data.TotalTaxAmount;
        $scope.percentageColumn = flag;
        $scope.LoadTaxButtonClick();
        $scope.serviceModel = data;
        $scope.serviceModel.TransactionAmount = data.Amount;
        $scope.serviceModel.ServiceAcknowledgementMasterId = data.ServiceAcknowledgementMasterId;
        $http({
            method: 'GET',
            url: $scope.path + 'GetServiceTaxListForTaxDetail?serviceId=' + data.Id
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
            angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
            //$scope.HSNCode = $scope.receiveTaxList[0].HSNCode;
        });

    }
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
        //debugger;
        var x = $event;
        var Id = x.data.Id;
        //alert('Id'+Id);
        // $scope.productNew = x.data;
        //  $scope.productId = "";
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

    function containsSpecialChars(str) {
        const specialChars = /[@!#$%^&*()_+\=\[\]{};':"|,.<>\?`~]/;
        return specialChars.test(str);
    }

    $scope.CheckSpecialCharecter = function () {
        try {
            if (containsSpecialChars($scope.productNew.DocRefNo)) {
                $scope.productNew.DocRefNo = $scope.productNew.DocRefNo.substring(0, $scope.productNew.DocRefNo.length - 1);
                throw "No special characters allowed for Doc Ref No.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

}