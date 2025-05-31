'use strict';
PurchaseOrderController.$inject = ['accountService', 'addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$location'];
function PurchaseOrderController(accountService, addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $location) {
    $rootScope.title = "Purchase Order";
    $scope.Action = 'Save';
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
    $scope.inventoryMaterialList = [];
    $scope.chargesList = [];
    $scope.ChargeTaxList = [];
    $scope.StateData = [];
    $scope.GetListForMasterOrderUpdate = [];
    //#region notification setting
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
    //#endregion

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

    //#endregion


    //#region PO Index Grid Data Display Load Function

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
            url: 'Products/PurchaseOrder/GetPOTypeList?POTypeStatus=' + $scope.POTypeStatus +'&poType='+'PO',
        }).then(function successCallback(response) {
          
            for (var i = 0; i < $scope.Griddata.length; i++) {
                response.data[i].PODate1 = new Date($scope.Griddata[i].PODate1);
            }
            $scope.Griddata = response.data;
        });
    };
    $scope.getalldata();



    $scope.GriddataPoApp = [];
    $scope.getalldataPoApp = function () {

        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseOrder/GetIndependentPOListByStatus?ApproveRejectHold=' + $scope.ApproveRejectHold,
        }).then(function successCallback(response) {
           
            for (var i = 0; i < response.data.length; i++) {
                response.data[i]["PODate"] = new Date(response.data[i]["PODate"]);
            }
            $scope.GriddataPoApp = response.data;
        });
    };



    //#region  PO  Details
    $scope.lst = [];
    $scope.POListDetails = function () {

        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/PurchaseOrder/GetInventoryMaterialListPoByReqDetail'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.lst = response.data;

        });
    }
    //$scope.POListDetails();
    $scope.PODocumentMapDataAll = function () {

        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/PurchaseOrder/PODocumentMapDataAll'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.Img = response.data;

        });
    }
    //$scope.PODocumentMapDataAll();
    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgrid = function detailGridData(e) {
        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("POmasterId", "equal", parseInt(filteredData), true).take(1000));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: ["MaterialGroupName", "MaterialName", "Article", "Sku1", "Sku2", "Sku3", "MaterialDetail", "TransactionQty", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalAmount"]
            //columns: ["MaterialGroupName", "MaterialName", "Article", "Sku1", "Sku2", "Sku3", "MaterialDetail", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalAmount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
        var dataImg = ej.DataManager(window.Img).executeLocal(ej.Query().where("POId", "equal", parseInt(filteredData), true).take(1000));
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
    //#endregion



    //#region Model

    $scope.product = {
        Id: null
        , GRNDate: null
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: $window.plantId
        , PartyId: null
        , EntityId: null
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
        , OrderSpecific: 'No'
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
    $scope.productDocMap = {
        Id: null
        , CompanyGroupId: null
        , FileName: null
        , UserFilename: null
        , SystemFileName: null
        , Description: null
        , Remarks: null
    };
    //#endregion

    //#region All Dropdownlist Load Function
    //#region Purchaser LC Intregrated to PurchaseOrder
    $scope.isProcurementOnBoM = false;
    $scope.getPlantConfigByPlant = function () {
        $scope.isProcurementOnBoM = false;
        $http({
            method: 'GET',
            url: 'Setups/plantconfig/GetPlantConfigDataByPlantId?plantid=' + $window.plantId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0)
                $scope.isProcurementOnBoM = response.data[0].ProcurementOnBoM;
        });
    };
    $scope.getPlantConfigByPlant();

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
    $scope.OrderSpecific = $scope.productNew.OrderSpecific;
    $scope.SelectedContract = function (obj) {

        //var data = obj.data.ContractId;
        $scope.productNew.ContractId = obj.data.ContractId;
        $scope.productNew.CustomerName = obj.data.CustomerName;
        $scope.productNew.ContractNo = obj.data.ContractNo;
        $scope.productNew.LCRef = obj.data.LCRef;
        //console.log($scope.productNew);
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }

    $scope.ClearFields = function () {
        //$scope.purchaseLC = {};
        $scope.productNew.ContractId = null;
        // $scope.productNew = { OrderSpecific: 'Yes', Id: null, Tenure: 0 };
        //$scope.purchaseLCChargesList = [];
        //$scope.Action = 'Save';
    }
    $scope.CloseContractPopUp = function () {
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }
    $scope.masterOrderCustomerList = [];
    $scope.GetMasterOrderByContractList = function () {
        $scope.masterOrderCustomerList = [];
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetMasterOrderListbyContract?contractId=" + $scope.productNew.ContractId
        }).then(function (response) {
            $scope.masterOrderCustomerList = response.data;
        });
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
    }

    $scope.CloseMasterOrder = function () {
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
    }





    $scope.GriddataPOWithLC = [];
    $scope.getalldataPOWithLC = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseOrder/GetalldataPOWithLCMap',
        }).then(function successCallback(response) {
            $scope.GriddataPOWithLC = response.data;
            //entrydata = copy(searchdata);
        });
    };
    $scope.getalldataPOWithLC();



    $scope.GriddataPOWithOutLC = [];
    $scope.getalldataPOWithOutLC = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseOrder/GetalldataPOWithoutLCMap',
        }).then(function successCallback(response) {
            $scope.GriddataPOWithOutLC = response.data;
        });
    };
    $scope.getalldataPOWithOutLC();




    $scope.POTypeStatus = '';
    $scope.tab1 = 1;
    $scope.setTabPOLCMapIndex = function (newTab) {

        //$scope.POTypeStatus = 'Pending';
        $scope.tab1 = newTab;
        $scope.getalldataPOWithLC();
    };
    $scope.isSetPOLCMapIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };
    $scope.setTabPOLCMap = function (newTab) {
        //alert('tabCHR');

        // $scope.POTypeStatus = 'CheckedHoldRej';
        $scope.tab1 = newTab;
        $scope.getalldataPOWithOutLC();
    };
    $scope.isSetPOLCMap = function (tabNum) {
        return $scope.tab1 === tabNum;
    };
    $scope.LcList = [];
    $scope.GetLCByContract = function () {

        $http({
            method: 'GET',//?id=' + id+' & name='+name
            url: "Products/PurchaseOrder/GetLCListByCotract?ContractId=" + $scope.data.ContractId + "&VendorId=" + $scope.data.PartyId
        }).then(function successCallback(response) {
            $scope.LcList = response.data;
            angular.element(document.querySelector('#ContractPopUp')).modal('show');

        });

    }

    // $scope.GetLCByContract();

    $scope.CurrencyId = null;
    $scope.a = function (args) {
        var gridObj = $("#Grid123").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.rowID = $scope.data.Id;
        $scope.CurrencyId = $scope.data.CurrencyId;
        $scope.GetLCByContract();
    };


    $scope.recorddoubleclickContract = function ($event) {

        var x = $event;
        var Id = x.data.Id;

        for (var i = 0; i < $scope.GriddataPOWithOutLC.length; i++) {
            if ($scope.GriddataPOWithOutLC[i].Id === $scope.rowID) {

                if ($scope.CurrencyId === x.data.CurrencyId) {
                    $scope.GriddataPOWithOutLC[i].PurchaseLCId = x.data.Value;
                    angular.element(document.querySelector('#ContractPopUp')).modal('hide');
                } else {
                    ShowResult("Purchase Order Currency and PurchaseLC Currency is not same!!!", 'failure', 'ContractPopUp');
                }
            }
        }

    };



    $scope.UpdatePOforLCdata = function () {

        if ($scope.data.PurchaseLCId === null || $scope.data.PurchaseLCId === '' || $scope.data.PurchaseLCId === undefined) {
            ShowResult('Please select Purchase LC');
            return false;
        }


        $http({
            method: 'POST',
            url: "Products/PurchaseOrder/UpdatePOforLC",
            data:
            {
                POId: $scope.rowID,
                PurchaseLCId: $scope.data.PurchaseLCId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.getDataList();
                $scope.getalldataPOWithOutLC();

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });

    }
    //#endregion
    $scope.entityList = [];
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });

    $scope.invoicingPartyPopUp = function () {
        // getPartyPlantEditList();
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closeInvoicingPartyPopUp = function () {
        if ($scope.inventoryMaterialList.length || $scope.chargesList.length) {
            if (!baseService.isUndefinedOrNull($scope.productNew.ChangeInvoicingStateId)) {
                if ($scope.productNew.PlantStateId === $scope.productNew.InvoicingStateId == $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.productNew.InvoicingStateId === $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.productNew.PlantStateId !== $scope.productNew.InvoicingStateId && $scope.productNew.PlantStateId != $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else
                    ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
            }
            else
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        }
        else
            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
    };
    //#endregion

    //#region Save Update Delete Function

    $scope.Save = function () {

        try {
            $scope.dbval = $scope.StateData;
            $scope.UIval = $scope.productNew.InvoicingState;
            if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
                ShowResult("Please select to be approved by", 'failure');
                return false;
            }
            else
                if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
                    ShowResult("Please select to be checked by", 'failure');
                    return false;
                }
                else
                    if ($scope.inventoryMaterialList.length === 0) {
                        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                    }
                    else if ($scope.dbval.length === 0) {
                        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                    }
                    else if ($scope.dbval === $scope.UIval) {
                        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                    }
                    else if ($scope.productNew.OrderSpecific === 'Yes' && baseService.isUndefinedOrNull($scope.productNew.ContractId)) {
                        ShowResult('Please Select Contract');
                        return false;
                    }
                    else if (baseService.isUndefinedOrNull($scope.TermsAndConditions.Id)) {
                        ShowResult('Please select Terms and Condition.');
                        return false;
                    }
                    else {
                        ShowResult('You can not change Invoicing party.Line is available', 'failure', 'invoicingPartyPopUp');
                    }
            if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
            if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
            $scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
            $scope.modelValidation('div_docDate', 'productNew', 'DocDate');
            //$scope.modelValidation('div_entryNo', 'productNew', 'GateEntryNo');
            $scope.modelValidation('div_PODate', 'productNew', 'PODate', 'PO Entry Date');
            $scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');
            if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId)
                $scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');
            else
                manualValidation('div_rate', false);

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {
                if ($scope.productNew.OrderSpecific == 'No') {
                    $scope.productNew.ContractId = null;
                }
                if (new Date($scope.productNew.PODate) < new Date($scope.productNew.DocDate))
                    return manualValidation('div_PODate', true, "PO date can't be less than Doc entry date");
                else
                    manualValidation('div_PODate', false);

                $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                $scope.product = Object.assign({}, $scope.productNew);
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'entity': $scope.product,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                        },
                        dataType: 'JSON'
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.productNew.Id = response.data.entity.Id;
                            $scope.productNew.PartyName = $scope.product.PartyName;
                            $scope.LoadTermsAndConditionGrid($scope.productNew.TermsAndConditionsId, $scope.productNew.Id);
                            $scope.LoadTermsAndConditionDetailGrid();
                            /*$scope.SaveTermsDetail($scope.productNew.TermsAndConditionChildId, $scope.productNew.Id);*/
                            $scope.Action = "Update";
                            //$scope.getDataList();
                            $scope.getalldata();
                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };

                }
                else if ($scope.Action === "Update") {

                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: {
                            'entity': $scope.product,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            //$scope.getDataList();
                            $scope.getalldata();
                            $scope.LoadTermsAndConditionGrid($scope.productNew.TermsAndConditionsId, $scope.productNew.Id);
                            $scope.LoadTermsAndConditionDetailGrid();

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


    $scope.Delete = function () {
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0 && baseService.arrayLength($scope.chargesList) === 0) {
            if (!baseService.isUndefinedOrNull($scope.productNew.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.deleteUrl + $scope.productNew.Id,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getDataList();
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
        $scope.TermsAndConditionGridList = [];
        $scope.POPupList = [];
        ClearFields();
        $scope.NotificationSettingStatus();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
        return true;

    };

    //#region Otheres Code 

    $scope.getDataList = function () {
        baseService.init($scope.getListUrl, null, null, "DESC", 'Id', 'PartyName');
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


    //$scope.getDataList();
    $scope.uom = function () {
        cboService.getUoMCbo(function (response) {
            $scope.uoMList = response;
            console.log('uoMList', $scope.uoMList);
        });
    }
    $scope.uom();
    $scope.storageList = [];
    $http({
        method: 'GET',
        url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });
    $scope.currencyList = [];


    //$scope.productNew.OrderSpecific = 'No';
    addressService.getCountryCbo(function (result) {
        $scope.countryList = result;
    });

    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
    }).then(function successCallback(response) {
        $scope.baseCurrencyId = response.data[0].Value;
        $scope.productNew.BaseCurrencyId = response.data[0].Value;
        //factoryService.getCurrencyPrecision($scope.baseCurrencyId);
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

    $scope.Get = function (index) {
        $scope.index = index;
        $scope.product = $scope.products[$scope.index];
        $scope.productNew = Object.assign({}, $scope.product);
        getPartyPlantList();
        getInventoryMaterialList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
        $scope.LoadTermsAndConditionGrid($scope.productNew.TermsAndConditionsId, $scope.productNew.Id);
        $scope.LoadTermsAndConditionDetailGrid();
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

    function GetMasterData() {
        var aa = $("#masterId").text();
        $http.get('Products/PurchaseOrder/GetPOMasterById?id=' + aa).then(function (response) {
            $scope.productNew = response.data;
        });

        getPartyPlantList();
        getInventoryMaterialList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
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


    // #region Extra Tax Add
    $scope.calculateTaxAmount = function (data) {
        //data.TotalAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
        data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
    };
    $scope.calculateTaxAmountForMat = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.detailModel.TransactionAmount * data.Percentage) / 100;
    };
    $scope.checkRowValidationMat = function (x) {
        ;
        for (var i = 0; i < $scope.taxCategoryList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.detailModel.TransactionAmount) || $scope.detailModel.TransactionAmount === 0) {
                ShowResult("Taxable Amount can not null or zero", 'failure', 'detailPopUp');
            }
            if ($scope.taxCategoryList[i].Id === x.Id) {
                $scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.detailModel.TransactionAmount).toFixed(4) * 100);
            }

        }
    }

    $scope.calculateTaxAmountForService = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.serviceModel.TransactionAmount * data.Percentage) / 100;
        $scope.sumSvcTaxAmount();
    };
    $scope.checkRowValidationService = function (x) {
        ;
        for (var i = 0; i < $scope.taxCategoryList.length; i++) {
            if ($scope.taxCategoryList[i].Id === x.Id) {
                $scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.serviceModel.TransactionAmount).toFixed(4) * 100);
            }
        }
    }

    $scope.calculateTaxAmountForServiceModify = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
    };
    $scope.checkRowValidationServiceModify = function (x) {
        ;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {

            if ($scope.receiveTaxList[i].Id === x.Id) {
                $scope.receiveTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100);
            }

        }
    }

    $scope.receiveTaxList = [];
   
    $scope.taxCategoryListcbo = [];
    $scope.LoadTaxButtonClick = function () {
        accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
            $scope.taxCategoryListcbo = result;
        });
    }
    accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
        $scope.taxCategoryListcbo = result;
    });
    $scope.addTax = function () {
        var data = {
            TotalAmount: 0,
            Id: null,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null
        };
        $scope.receiveTaxList.push(data);
    };

    // #endregion 


    $scope.Clearcontract = function () {
        $scope.productNew.CustomerName = "";
        $scope.productNew.ContractId = "";
        $scope.productNew.ContractNo = "";

    };

    function ClearFields() {


        $scope.Action = "Save";
        $scope.product = {};
        $scope.IsBaseOnDueDateEnable = false;
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
            , IsTradingPO: false
            , Tolerance:0
        };

        $scope.inventoryMaterialList = [];
        $scope.GetListForMasterOrder = [];
        $scope.GetListForMasterOrderUpdate = [];
        $scope.chargesList = [];
        $scope.grossTotal = 0;
        baseService.removeErrorClasses();
        //$scope.getToCurrencyRate();
        $scope.productNew.OrderSpecific = 'No';
        $scope.productNew.DiscountAmount = '0';
    }


    $scope.changeAllInvoice = function () {
        $scope.productNew.InvoiceNo = null;
        $scope.productNew.InvoiceDate = null;
    };
    $scope.showPartyPopUp = function () {
        baseService.setCurrentPage('partyList');
        $scope.getPartyList = function (pageno) {
            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList?partyType=' + $scope.partyType;
            }
            else if ($scope.partyType === 'Party') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList';
            }
            else if ($scope.partyType === 'Director') {
                $scope.partyUrl = 'Parties/party/GetCompanyDirectorDataList';
            }
            else if ($scope.partyType === 'Other') {
                $scope.partyUrl = 'Parties/party/GetCompanyOtherDataList';
            }
            baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
                .then(function (result) {
                    $scope.partyList = result.Rows;
                    $scope.partyParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#partyPopUp')).modal('show');
        $scope.getPartyList();
    };
    $scope.closePartyPopUp = function (x) {

        //if ($scope.partyIndex !== -1) {
        var party = x.data;
        // var party = $scope.partyList[$scope.partyIndex];
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
        $scope.PaymentModeByPaymentTerm();
        //}
    };
    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull(!baseService.isUndefinedOrNull($scope.productNew.CurrencyId))) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?currencyId=" + $scope.productNew.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.productNew.ToCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };
    $scope.getToCurrencyRate = function () {

        if (baseService.isUndefinedOrNull($scope.productNew.PODate)) {
            $scope.productNew.ToCurrencyRate = 1;
            return;
        }
        $http.get($scope.path + 'GetToCurrencyRate?currencyId=' + $scope.productNew.CurrencyId + '&baseCurrencyId=' + $scope.productNew.BaseCurrencyId + '&docDate=' + $filter('dateFiltering')($scope.productNew.PODate))
            .then(function (response) {
                if (parseFloat(response.data) === 0)
                    $scope.productNew.ToCurrencyRate = 1;
                else
                    $scope.productNew.ToCurrencyRate = response.data;
            });
    };



    $scope.billShippAddress = function (id, flag) {

        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateName;
            var stateId = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateId;// 30-5
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = state;
                $scope.productNew.ChangeInvoicingStateId = stateId;//30-5
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
    $scope.businessProcesses = '';//"BP.BusinessProcessName IN('MaintenanceSpare','BOM','WetProcess','Consumable')";
    $scope.detailPopUp = function () {
        $scope.productNew.TaxOptionMat = 'Yes';
        $scope.receiveTaxList = [];
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
            , TransactionRate: null
            , TransactionAmount: null
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
            , PartyCode: null
            , RefferenceNo: null
            , Tolerance: $scope.productNew.Tolerance
        };
        $scope.clearCharNames();
        angular.element(document.querySelector('#detailPopUp')).modal('show');
    };
    $scope.detailPopUpEdit = function () {
        for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
            for (var t = 0; t < $scope.inventoryMaterialList[i].TaxList.length; t++) {
                if ($scope.inventoryMaterialList[i].TransactionRate === 0 || $scope.inventoryMaterialList[i].TransactionRate === '0.0' || $scope.inventoryMaterialList[i].TransactionRate === '') {
                    ShowResult('Enter Rate', 'failure');
                    return false;
                }
                else if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].DeliveryDate)) {
                    ShowResult('Enter Delivery Date', 'failure');
                    return false;
                }
                $scope.receiveTaxList.push($scope.inventoryMaterialList[i].TaxList[t]);
            }
        }
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/UpdateMaterial',
            data: {
                entity: $scope.inventoryMaterialList,
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
    };
    $scope.MaterilaUpdate = function () {


        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.detailPopUpEditForm.$valid) {
                $http({
                    method: 'POST',
                    url: 'Products/PurchaseOrder/UpdateMaterial',
                    data: $scope.detailModel,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'detailPopUpEdit');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'detailPopUpEdit');
                    }
                }), function (response) {
                    ShowResult(response.data.Message, 'failure', 'detailPopUpEdit');
                };
            }

        } catch (e) {
            throw e;
        }
    };
    $scope.closeDetaiPopUp = function () {

        $scope.detailModel = {};
        $scope.taxCategoryList = [];
        removeValidationMsg();
        angular.element(document.querySelector('#detailPopUp')).modal('hide');
    };
    //test
    $scope.closeDetaiPopUpEdit = function () {
        $scope.detailModel = {};
        $scope.taxCategoryList = [];
        removeValidationMsg();
        angular.element(document.querySelector('#detailPopUpEdit')).modal('hide');
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
        $scope.productNew.TaxOptionMat = 'Yes';
        getTaxCategoryList(ob.HSNCodeId, ob.HSNCode);
        var mmId = []; mmId.push(ob.Id);

        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
            $scope.uoMList = result;
            console.log($scope.uoMList)
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
            getTaxCategoryList(ob.HSNCodeId, ob.HSNCode);
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
    $scope.materialValidation = function () {
        var getRow3 = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.detailModel.MaterialMasterId, "ArticleId": $scope.detailModel.ArticleId, "FirstCharacteristicsValueId": $scope.detailModel.FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.detailModel.SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.detailModel.ThirdCharacteristicsValueId });
        if (getRow3 == 0) {
            $scope.invalid = true;
        }
        else {
            ShowResult('Material Combination Already Exist', 'failure', 'detailPopUp');
            $scope.invalid = false;
        }
        $scope.MatDescriptionValidation();
    }
    $scope.MatDescriptionValidation = function () {

        var getRow31 = $filter("filter")($scope.inventoryMaterialList, { "Description": $scope.detailModel.Description, "MaterialMasterId": null });

        if (getRow31 == 0) {
            $scope.invalid1 = true;
        }

        else {
            ShowResult('Material Description Already Exist', 'failure', 'detailPopUp');
            $scope.invalid1 = false;
            return false;
        }
    }
    $scope.detailSave = function () {

        try {
            $scope.validation();
           
            $scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
            if ($scope.char1.CharacteristicsId === undefined) {
                $scope.char1.CharacteristicsId = null;

            }
            else if ($scope.char1.CharacteristicsValueId === undefined) {
                $scope.char1.CharacteristicsValueId = null;
            }
            else if ($scope.char2.CharacteristicsId === undefined) {
                $scope.char2.CharacteristicsId = null;
            }
            else if ($scope.char2.CharacteristicsValueId === undefined) {
                $scope.char2.CharacteristicsValueId = null;
            }
            else if ($scope.char3.CharacteristicsId === undefined) {
                $scope.char3.CharacteristicsId = null;
                $scope.char3.CharacteristicsValueId = null;
            }


            $scope.detailModel.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
            $scope.detailModel.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
            $scope.detailModel.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
            $scope.detailModel.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;
            $scope.detailModel.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
            $scope.detailModel.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;




            $scope.detailModel.CountryId = $scope.detailModel.CountryId;
            for (var i = 0; i < baseService.arrayLength($scope.inventoryMaterialList); i++) {
                if ($scope.detailModel.MaterialMasterId === $scope.inventoryMaterialList[i].InventoryMaterialId &&
                    $scope.detailModel.ArticleId === $scope.inventoryMaterialList[i].ArticleId &&
                    $scope.detailModel.FirstCharacteristicsId === $scope.inventoryMaterialList[i].FirstCharacteristicsId &&
                    $scope.detailModel.FirstCharacteristicsValueId === $scope.inventoryMaterialList[i].FirstCharacteristicsValueId &&
                    $scope.detailModel.SecondCharacteristicsId === $scope.inventoryMaterialList[i].SecondCharacteristicsId &&
                    $scope.detailModel.SecondCharacteristicsValueId === $scope.inventoryMaterialList[i].SecondCharacteristicsValueId &&
                    $scope.detailModel.ThirdCharacteristicsId === $scope.inventoryMaterialList[i].ThirdCharacteristicsId &&
                    $scope.detailModel.ThirdCharacteristicsValueId === $scope.inventoryMaterialList[i].ThirdCharacteristicsValueId &&
                    $scope.detailModel.CountryId === $scope.inventoryMaterialList[i].CountryId) {
                    return ShowResult('This material already received');
                }
            }
            $scope.materialValidation();
            // }

            if ($scope.invalid && $scope.invalid1) {
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
                            , TransactionAmount: null
                            , ToCurrencyRate: $scope.productNew.ToCurrencyRate
                            , IsNonCreditable: $scope.productNew.IsNonCreditable
                            , IsOriginApplicable: false

                        };
                        $scope.taxCategoryList = [];
                        getInventoryMaterialList($scope.productNew.Id);
                        $scope.getDataList();
                        $scope.getalldata();
                        $scope.clearCharNames();
                        $scope.uom();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'detailPopUp');
                };
            }
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };
    $scope.valuePassInDelModal = function (id) {
        $scope.id = id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
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
                    $scope.id = null;
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                    $scope.ActionPOBOQ = 'Save';
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
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
    $scope.GetSalesTaxData = function (salesId) {
        $scope.TaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + $scope.masterId
        }).then(function (response) {
            $scope.TaxList = response.data;

            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                var linepk = $scope.inventoryMaterialList[i].InventoryReceiveDetailId;
                var list = gettaxlist(linepk);
                $scope.inventoryMaterialList[i].TaxList = list;
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
    $scope.sumORnot = false;
    // Material Load
    function getInventoryMaterialList(inveReveiveId) {
        $scope.masterId = inveReveiveId;

        $scope.inventoryMaterialList = [];
        $http.get($scope.path + 'GetInventoryMaterialList?inveReveiveId=' + inveReveiveId)
            .then(function (response) {

                $scope.inventoryMaterialList = ej.DataManager(response.data.Rows).executeLocal(ej.Query().sortBy("UserName desc"));//response.data.Rows;
                //var dataManagerObj = ej.DataManager(response.data.Rows).executeLocal(ej.Query().sortBy("UserName ASC"));
                $scope.DetailId = $scope.inventoryMaterialList[0].InventoryReceiveDetailId;
                $scope.InvoicingPartyPlantId = $scope.inventoryMaterialList[0].InvoicingPartyPlantId;
                $scope.productNew.InvoicingPartyPlantId = $scope.inventoryMaterialList[0].InvoicingPartyPlantId;
                $scope.productNew.InvoicingStateId = $scope.inventoryMaterialList[0].InvoicingStateId;
                $scope.productNew.PlantStateId = $scope.inventoryMaterialList[0].PlantStateId;
                checkSameValueInColumnList($scope.inventoryMaterialList, 'TransactionUoM');
                getGrossAmount($scope.inventoryMaterialList, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
                $scope.GetSalesTaxData();
            });

    }
    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }
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
    $scope.calculateTaxCategory = function () {
        $scope.detailModel.TotalTaxAmount = 0;
        var tQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) ? 0 : parseFloat($scope.detailModel.TransactionQty);
        var trate = baseService.isUndefinedOrNull($scope.detailModel.TransactionRate) ? 0 : parseFloat($scope.detailModel.TransactionRate);
        if (tQty > 0 && trate > 0)
            $scope.detailModel.TransactionAmount = Math.round((tQty * trate) * 100 + Number.EPSILON) / 100;
        else
            $scope.detailModel.TransactionAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.detailModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.detailModel.TotalTaxAmount)) $scope.detailModel.TotalTaxAmount = 0;
    };
    $scope.calculateTaxCategoryRate = function () {

        $scope.detailModel.TotalTaxAmount = 0;
        var tQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) ? 0 : parseFloat($scope.detailModel.TransactionQty);
        var trate = baseService.isUndefinedOrNull($scope.detailModel.TransactionRate) ? 0 : parseFloat($scope.detailModel.TransactionRate);
        if (tQty > 0)
            $scope.detailModel.TransactionAmount = Math.round((tQty * trate) * 100 + Number.EPSILON) / 100;
        else
            $scope.detailModel.TransactionAmount = 0;
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
            //data: $scope.receiveTaxList,
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
                getInventoryMaterialList($scope.productNew.Id);
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
        };
    }

    $scope.closeServiceChargeTaxPopUp = function () { //hossain
        //
        $scope.detailModel = {};
        $scope.detailModel.InventoryReceiveDetailId = $scope.ServiceId;
        $scope.detailModel.InventoryReceiveDetailId = $scope.DetailId;
        $scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
            if (getRow.length == 2) {
                ShowResult("You can't add Same Tax two times", 'failure', 'ServiceChargeTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxCategoryId)) {
                ShowResult("Select Tax Category.", 'failure', 'ServiceChargeTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].Percentage)) {
                ShowResult("Input Percentage.", 'failure', 'ServiceChargeTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxAmount)) {
                ShowResult("Input Tax Amount.", 'failure', 'ServiceChargeTaxPopUp');
                return false;
            }
        }

        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/InsertserviceTax',
            data: {
                entity: $scope.detailModel
                , taxCategoryList: $scope.receiveTaxList
                , ServiceId: $scope.ServiceId
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
            }
            else {
                ShowResult(response.data.Message, 'success', 'ServiceChargeTaxPopUp');
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
        };
    }
    $scope.closeReceiveTaxPopUpwindow = function () {

        getInventoryMaterialList($scope.productNew.Id);
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }
    $scope.closeServiceChargeTaxPopUpwindow = function () {
        getServiceChargeList($scope.productNew.Id);
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
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
    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getvendorcbo'
    }).then(function successCallback(response) {

        $scope.paymentTermList = response.data;
    });

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

    $scope.serviceChargePopUp = function () {
        $scope.productNew.TaxOptionService = 'Yes';
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
    // #endregion Service

    $scope.inventoryReceiveReport = function (id, reportFormat) {
        if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/InventoryReceive/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + id + '&plantId=' + $scope.productNew.PlantId, '_blank');
    };
    
    $scope.GriddataVendor = [];
    $scope.getalldataVendor = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseOrder/GetListByParty',
        }).then(function successCallback(response) {
            $scope.GriddataVendor = response.data;
            //entrydata = copy(searchdata);
        });
    };
    function getPartyPlantList() {


        //var aa = $scope.Id;
        $scope.plantList = [];
        $http.get('Products/PurchaseOrder/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId + '&Id=' + $scope.Id).then(function (response) {
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
    $scope.getalldataVendor();
    $scope.getalldata();
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
        getServiceChargeList($scope.productNew.Id);

        if (!baseService.isUndefinedOrNull(x.data.ContractId)) {
            $scope.productNew.OrderSpecific = 'Yes';
        }
        else {
            $scope.productNew.OrderSpecific = 'No';
        }
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
        $scope.ImagedataLoad($scope.productNew.Id);
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
        if (!$rootScope.isCollapsed) $rootScope.toggle();


    };
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
    $scope.closeServiceChargePopUpEdit = function () {
        $scope.serviceModel = {};
        $scope.receiveTaxList = [];
        angular.element(document.querySelector('#serviceChargePopUpEdit')).modal('hide');
    };
    $scope.dindex = -1;
    $scope.DelCharge = function (Id, index) {
        $scope.dindex = index;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            if ($scope.receiveTaxList[i].Id === Id) {
                $scope.receiveTaxList.splice($scope.dindex, 1);
                return true;
                break;
            }
        }
        $scope.dindex = -1;
    };
    $scope.Del = function (Id, index) {
        $scope.dindex = index;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            if ($scope.receiveTaxList[i].Id === Id) {
                $scope.receiveTaxList.splice($scope.dindex, 1);
                return true;
                break;
            }
        }
        $scope.dindex = -1;
    };
    $scope.calculateAmount = function (data) {

        data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        if (data.TransactionRate === 'NaN')
            data.TransactionRate = 0;
        if (data.TrnAmount === 'NaN')
            data.TrnAmount = 0;
        data.TaxAmount = 0;
        data.BaseTaxAmount = 0;
        angular.forEach(data.TaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;

            data.BaseTaxAmount += item.TaxAmount;
        });
        if ($scope.productNew.IsNonCreditable == 1) {
            //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
            if (data.BaseTaxAmount === null) {
                data.BaseTaxAmount = '0.00';
            }
            data.BaseAmount = parseFloat(data.TrnAmount + data.BaseTaxAmount);
            $scope.detailPopUpEdit();
        }
        else {
            // data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
            data.BaseAmount = data.TrnAmount;
            $scope.detailPopUpEdit();
        }
    };
    $scope.calculateRate = function (data, event) {

        data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
        if (data.TransactionRate === 'NaN')
            data.TransactionRate = 0;
        data.BaseTaxAmount = 0;
        angular.forEach(data.TaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;

            data.BaseTaxAmount += item.TaxAmount;
        });
        // data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
        if ($scope.productNew.IsNonCreditable == 1) {
            //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
            data.BaseAmount = data.TrnAmount + data.BaseTaxAmount;
        }
        else {
            // data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
            data.BaseAmount = data.TrnAmount;
        }

    };
    $scope.calculateAmountForServiceCharge = function (data) {
        data.TotalTaxAmount = 0;
        for (var i = 0; i < $scope.ChargeTaxList.length; i++) {
            if ($scope.ChargeTaxList[i].InventoryServiceId === data.Id) {
                $scope.ChargeTaxList[i].TaxAmount = data.Amount * $scope.ChargeTaxList[i].Percentage / 100;
                data.TotalTaxAmount += $scope.ChargeTaxList[i].TaxAmount;
            }
        }
    };
    $scope.onchangeFunction = function (id) {
        $scope.TaxCategoryId = id;

        var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
        if (getRow.length === 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'ServiceChargeTaxPopUp');

        }

    }
    $scope.onchangeFunction1 = function (id) {
        $scope.TaxCategoryId = id;

        var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
        if (getRow.length === 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
        }
    };

    //#region Print for po Approval

    $scope.onClickpoApprovalprint = function (args) {

        var gridObj = $("#GridPO1").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/PurchaseOrder/GePurchaseOrderReport?purchaseOrderId=" + data.Id;

    };
    $scope.commandprint = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",
            click: $scope.onClickpoApprovalprint
        }
    }];
    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.productNew.DocDate) > new Date($scope.productNew.PODate)) {
            msg = "Doc date must be grater or equal to Vendor Doc. RefNo!";
            $scope.invalidDocDate = true;
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };
    //#region Shahazahan Code for PO Approval

    $scope.onClickPO = function (args) {

        var gridObj = $("#Grid").data("ejGrid");
        //getting corresponding record 
        $scope.data = gridObj.getSelectedRecords()[0];
        //alert('POClose' + data.Id);
        $scope.approveAlert();

    };
    $scope.Status = null;
    $scope.poAppUnApproved = function () {
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/PoUnApproved',
            data: {
                'PoId': $scope.podata1.Id,
                'PoValue': $scope.podata1.TotalQty
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ListForPOApproval1UnApproved();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    //#endregion
    //#region Towfik PO Closed
    $scope.GriddataPOClose = [];
    $scope.getalldataPOClose = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetListForPOClose',
        }).then(function successCallback(response) { //datagatefun
            $scope.GriddataPOClose = response.data;
        });
    };
    $scope.getalldataPOClose();
    $scope.onClickPOlock = function (args) {

        var gridObj = $("#Grid").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.approvalAlertlock();

    };
    $scope.approvalAlertlock = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#poapprovealertlock')).modal('show');
    };

    $scope.commandPoClose = [{
        type: "details", buttonOptions: {
            text: "Po Unlock",
            width: "120",
            height: "20",
            click: $scope.onClickPOlock
        }
    }];
    $scope.Poclosed = function () {
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/POClose',
            data: {
                'PoId': $scope.data.Id,
                'PoValue': $scope.data.TotalQty
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldataPOClose();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });

    }
    //#endRegion

    // # Taufik region setTab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion

    // #region Taufik Un Approval po data post start
    $scope.ListForPOApproval1UnApproved = [];
    $scope.GetListForPOApproval1UnApproved = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetListForPOApproval1UnApproved',
        }).then(function successCallback(response) {
            $scope.ListForPOApproval1UnApproved = response.data;
        });
    };
    $scope.GetListForPOApproval1UnApproved();
    $scope.onClickPOA1 = function (args) {
        var gridObj = $("#GridPO1").data("ejGrid");
        $scope.podata1 = gridObj.getSelectedRecords()[0];
        $scope.approveAlert1();
    };
    $scope.commandpo1 = [{
        type: "details", buttonOptions: {
            text: "Un Approve",
            width: "100",
            height: "30",
            click: $scope.onClickPOA1
        }
    }];
    $scope.approveAlert1 = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#poapprovalalert1')).modal('show');
    };

    $scope.poApp1 = function () {
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/PoApproved1',
            data: {
                'PoId': $scope.podata1.Id,
                'PoValue': $scope.podata1.TotalQty
            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Griddataapprovpo1();
                $scope.ClosedPOPUp();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
    $scope.ClosedPOPUp = function (args) {
        angular.element(document.querySelector('#poapprovalalert1')).modal('hide');
    };
    $scope.onClickPOA = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        $scope.approvalAlert();
    };
    $scope.commandPOUNChecked = [{
        type: "details", buttonOptions: {
            text: "Checked",
            width: "100",
            height: "30",
            click: $scope.onClickPOA
        }
    }];

    $scope.approvalAlert = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };
    //#endregion

    //#region Towfik PO Unlock
    $scope.GriddataPOlock = [];
    $scope.getalldataPOUnlock = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseOrder/GetListForPOUnClose',
        }).then(function successCallback(response) { //datagatefun
            $scope.GriddataPOlock = response.data;
            //entrydata = copy(searchdata);
        });
    };

    $scope.getalldataPOUnlock();

    $scope.onClickPOlock = function (args) {

        var gridObj = $("#GridUc").data("ejGrid");
        //getting corresponding record 
        $scope.data = gridObj.getSelectedRecords()[0];
        //alert('POClose' + data.Id);
        $scope.approvalAlertUnlock();

    };
    $scope.approvalAlertUnlock = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#POPUnlock')).modal('show');
    };
    $scope.PoUnlock = function () {
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/POUnClose',
            data: {
                'PoId': $scope.data.Id,
                'PoValue': $scope.data.TotalQty
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldataPOUnlock();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.commandPoUnlock = [{
        type: "details", buttonOptions: {
            text: "Po lock",
            width: "120",
            height: "20",
            click: $scope.onClickPOlock
        }
    }];

    //#endRegion

    // # Taufik region setTab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion

    $scope.GriddataPOListforPoclosedui = [];
    $scope.getalldataPOListforPoclosedui = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetListForAllPOList',
        }).then(function successCallback(response) { //datagatefun
            $scope.GriddataPOListforPoclosedui = response.data;
        });
    };

    $scope.getalldataPOListforPoclosedui();
    $scope.onClickPoList = function (args) {

        var gridObj = $("#GridPOListforPoclosedui").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.approvalAlertPoList();
    };
    $scope.approvalAlertPoList = function () {
        $scope.message = 'Are you sure want to Approve?';

        angular.element(document.querySelector('#AllPoListmi')).modal('show');
    };
    $scope.PoListinClose = function () {
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/POClose',
            data: {
                'PoId': $scope.data.Id,
                'PoValue': $scope.data.TotalQty
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldataPOListforPoclosedui();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.commandAllPoList = [{

        type: "details", buttonOptions: {
            text: "Po lock",
            width: "120",
            height: "20",
            click: $scope.onClickPoList
        }
    }];

    $scope.MasterOrderList = function () {
        $scope.getalldataListForMasterOrder();
        angular.element(document.querySelector('#ListOfMasterOrder')).modal('show');
    };

    $scope.MasterOrderListHide = function () {
        angular.element(document.querySelector('#ListOfMasterOrder')).modal('hide');
    };

    $scope.GetListForMasterOrder = [];
    $scope.getalldataListForMasterOrder = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetListForMasterOrder',
        }).then(function successCallback(response) { //datagatefun
            $scope.GetListForMasterOrder = response.data;
        });
    };

    $scope.Getrecorddoubleclick = function ($event, index) {
        var x = $event;
        var Id = x.data.Id;
        $scope.MONo = Id;
        getMasterItemList();
        angular.element(document.querySelector('#ListOfMasterOrder')).modal('hide');
    };
    function getMasterItemList() {
        $scope.inventoryMaterialList = [];
        $http.get($scope.path + 'GetMasterItemList?masterOrderId=' + $scope.MONo)
            .then(function (response) {
                $scope.inventoryMaterialList = response.data;
                $scope.GetSalesTaxData();
            });
    }
    $scope.calculateAmountByRateFG = function (data) {

        data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        if (data.TrnAmount === 'NaN')
            data.TrnAmount = 0;
        data.TaxAmount = 0;
        angular.forEach(data.TaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;
            data.BaseTaxAmount += item.TaxAmount;
        });
        data.BaseAmount = parseFloat($scope.productNew.ToCurrencyRate * data.TrnAmount).toFixed(2);
    };
    $scope.changeServiceForFG = function () {
        $scope.serviceModel.CurrencyName = "INR";
        $scope.serviceModel.ToCurrencyRate = 1;
        if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
        getTaxCategoryListForFGService(hsnCodeId);
    };
    function getTaxCategoryListForFGService(hsnCodeId) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: $scope.path + 'GetTaxCategoryListForFGService?partyPlantId=' + $scope.productNew.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
        });
    }
    $scope.ServiceListFGAdd = function () {
        var TempList = [];
        TempList.Id = $scope.serviceModel.ServiceMasterId;
        TempList.ServiceMasterName = angular.element("#ServiceMasterId :selected").text();
        TempList.Amount = $scope.serviceModel.TransactionAmount;
        TempList.TotalTaxAmount = 0;
        TempList.TotalTaxAmount = $filter('sumByKey')($filter('filter')($scope.taxCategoryList), 'TaxAmount');
        $scope.chargesList.push(TempList);
        for (var i = 0; i < $scope.taxCategoryList.length; i++) {
            $scope.taxCategoryList[i].ServiceMasterId = $scope.serviceModel.ServiceMasterId;
            $scope.ChargeTaxList.push($scope.taxCategoryList[i]);
        }
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    }

    $scope.getServiceTaxFGList = function (data, flag, ServiceId, index) {
        $scope.LoadTaxButtonClick();
        $scope.Currency = $("#currency option:selected").text();
        $scope.ServiceId = ServiceId;
        $scope.taxAbleAmnt = data.Amount;//+ data.TotalTaxAmount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.receiveTaxList = [];
        if ($scope.ChargeTaxList.length > 0) {
            $scope.HSNCode = $scope.ChargeTaxList[0].HSNCode;
            $scope.receiveTaxList = $filter('filter')($scope.ChargeTaxList, { 'ServiceMasterId': ServiceId });
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');
    }

    $scope.AddReceiveTaxPopUpFG = function (Id, index) { //hossain
        $scope.detailModel = {};
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.receiveTaxList), 'TaxAmount');
        for (var j = 0; j < $scope.inventoryMaterialList.length; j++) {
            if ($scope.inventoryMaterialList[j].Id === $scope.PODetailid) {
                $scope.inventoryMaterialList[j].BaseTaxAmount = TotalServiceTaxAmount;
            }
        }
        $scope.detailModel.InventoryReceiveDetailId = $scope.currentInventoryReceiveDetailIdRow;
        $scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
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
            $scope.TaxList.push($scope.receiveTaxList);
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }

    $scope.closeReceiveTaxPopUpFG = function () { //hossain        
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }

    $scope.getReceiveTaxListFG = function (data, flag, index, Id) {
        $scope.PODetailid = data.Id;
        $scope.LoadTaxButtonClick();
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        if (data.TaxList.length > 0) {
            $scope.HSNCode = data.TaxList[0].HSNCode;
            $scope.receiveTaxList = data.TaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.receiveTaxList[j].Id = $scope.PODetailid;
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    }
    $scope.addTaxFG = function () {
        var data = {
            TotalAmount: 0,
            Id: $scope.PODetailid,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null
        };
        $scope.receiveTaxList.push(data);
    };
    $scope.sumSvcTaxAmountFG = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
    };

    $scope.SaveFG = function () {
        //
        try {
            $scope.dbval = $scope.StateData;
            $scope.UIval = $scope.productNew.InvoicingState;

            if ($scope.inventoryMaterialList.length === 0) {
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            }
            else if ($scope.dbval.length === 0) {
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            }
            else if ($scope.dbval === $scope.UIval) {
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            }
            else {
                ShowResult('You can not change Invoicing party.Line is available', 'failure', 'invoicingPartyPopUp');
            }

            if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
            if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
            $scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
            $scope.modelValidation('div_docDate', 'productNew', 'DocDate');
            //$scope.modelValidation('div_entryNo', 'productNew', 'GateEntryNo');
            $scope.modelValidation('div_PODate', 'productNew', 'PODate', 'PO Entry Date');
            $scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');
            if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId)
                $scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');
            else
                manualValidation('div_rate', false);

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {
                if (new Date($scope.productNew.PODate) < new Date($scope.productNew.DocDate))
                    return manualValidation('div_PODate', true, "PO date can't be less than Doc entry date");
                else
                    manualValidation('div_PODate', false);
                $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                $scope.product = Object.assign({}, $scope.productNew);
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrlFg,
                        data: $scope.product,
                        dataType: 'JSON'
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.productNew.Id = response.data.entity.Id;
                            $scope.productNew.PartyName = $scope.product.PartyName;
                            $scope.Action = "Update";
                            //$scope.getDataList();
                            $scope.getalldata();
                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrlFG,
                        data: $scope.product,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            //$scope.getDataList();
                            $scope.getalldata();

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

    $scope.closeServiceChargeTaxPopUpwindowFG = function () {
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
    }
    //#endregion
    $scope.checkedByList = [];
    $scope.GetSupervisorCboList = function () {

        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/GetSupervisorCbo'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
    }
    $window.onresize = function (event) {
        $scope.actionCompleteSelected();
    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#Griddata1").ejGrid("instance");
                var scrollerwidth = $("#gridscroll").width();//Obtain the width of the container
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
        }
    };
    $window.onresize = function (event) {
        $scope.actionCompleteSelected();
    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GriddataAUth").ejGrid("instance");
                var scrollerwidth = $("#gridscroll1").width();//Obtain the width of the container
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
        }
    };
    $scope.onClickPOHR = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        $scope.approvalAlert();
    };

    //#endregion
    //#region----PO-without-requisition----
    //#endregion
    //#region----purchaseOrder-Authorized----
    $scope.POTypeApprovalStatus = '';
    $scope.setTabAHR = function (newTab) {
        $scope.tab = newTab;
        // $scope.POTypeApprovalStatus = '';
        $scope.POTypeApprovalStatus = 'ApprovedHoldRej';
        $scope.getApprovaldataAUth();
    };
    $scope.isSetAHR = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabpoa12 = function (newTab) {
        $scope.tab = newTab;
        $scope.getApprovaldataAUth1();
    };
    $scope.isSetpoa12 = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.onClickPOAUTH = function (args) {

        var gridObj = $("#GridPOAPp").data("ejGrid");
        //getting corresponding record 
        $scope.podata = gridObj.getSelectedRecords()[0];
        $scope.approvalAlert();
    };
    $scope.commandpoAuth = [{
        type: "details", buttonOptions: {
            text: "Approved",
            width: "100",
            height: "30",
            click: $scope.onClickPOAUTH
        }
    }];
    $scope.onClickPOAUTH1 = function (args) {
        var gridObj = $("#GridPOAHR").data("ejGrid");
        //getting corresponding record 
        $scope.podata = gridObj.getSelectedRecords()[0];
        $scope.approvalAlertHold();
    };
    $scope.commandpoAuth1 = [{
        type: "details", buttonOptions: {
            text: "Approved",
            width: "100",
            height: "30",
            click: $scope.onClickPOAUTH1
        }
    }];

    //#endregion

    $scope.approvalAlertHold = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#approvalAlertHold')).modal('show');
    };

    //#region ---All print option of PO-without-requisition
    $scope.onClick = function (args) {
        var gridObj = $("#Grid").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "Products/PurchaseOrder/GePurchaseOrderReport?purchaseOrderId=" + data.Id;
    };
    $scope.command = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.onClick
        }
    }];

    $scope.onClickCHR = function (args) {
        var gridObj = $("#GridCHR").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/PurchaseOrder/GePurchaseOrderReport?purchaseOrderId=" + data.Id;
    };
    $scope.commandCHR = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",
            click: $scope.onClickCHR
        }
    }];
    $scope.onClickChecked = function (args) {
        var gridObj = $("#GridChecked").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/PurchaseOrder/GePurchaseOrderReport?purchaseOrderId=" + data.Id;
    };
    $scope.commandChecked = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",
            click: $scope.onClickChecked
        }
    }];
    $scope.onClickApprovedHR = function (args) {
        var gridObj = $("#GridApprovedHR").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/PurchaseOrder/GePurchaseOrderReport?purchaseOrderId=" + data.Id;
    };
    $scope.commandAHR = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",
            click: $scope.onClickApprovedHR
        }
    }];
    $scope.onClickPO = function (args) {
        var gridObj = $("#GridApproved").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/PurchaseOrder/GePurchaseOrderReport?purchaseOrderId=" + data.Id;
    };
    $scope.commandPO = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",
            click: $scope.onClickPO
        }
    }];
    //#endregion
    //#region All Print option of purchaseOrder-Checked-By
    $scope.onClick11 = function (args) {
        var gridObj = $("#GridPO").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;
    };
    $scope.command = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "100",
            height: "30",
            click: $scope.onClick11
        }
    }];

    $scope.onClick111 = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;
    };
    $scope.commandNewPo = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "100",
            height: "30",
            click: $scope.onClick111
        }
    }];



    $scope.onClickPOReqHR = function (args) {
        var gridObj = $("#GridReqHR").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;
    };
    $scope.commandPOReqHRPrint = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "100",
            height: "30",
            click: $scope.onClickPOReqHR
        }
    }];

    $scope.onClickPOCheckRPrint = function (args) {

        var gridObj = $("#GridPO1").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;
    };
    $scope.commandPOReqHRP = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "100",
            height: "30",
            click: $scope.onClickPOCheckRPrint
        }
    }];
    //#endregion
    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalQty", dataMember: "TotalQty", format: "{0:N0}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amt", dataMember: "Amt", format: "{0:N0}" }],
        showCaptionSummary: true
    }];

    $scope.onClickPOA = function (z) {

        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        $scope.url = $location.absUrl().split('!/')[1]
        if ($scope.url === 'purchaseOrder-Authorized') {
            //$scope.tabType = 'UnApprovedList';
            if ($scope.podata.AuthorizedStatus === 'Approved') {
                $scope.POPUpStatus = 'Approve';
            }
            else {
                $scope.POPUpStatus = $scope.podata.AuthorizedStatus;
            }
        }
        else if ($scope.url === 'purchaseOrder-Checked-By') {
            // $scope.tabType = 'UnCheckedList';
            if ($scope.podata.CheckedStatus === 'Checked') {
                $scope.POPUpStatus = 'Check';
            }
            else {
                $scope.POPUpStatus = $scope.podata.CheckedStatus;
            }
        }
        $scope.message = 'Are you sure to ' + $scope.POPUpStatus + '?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };
    $scope.AllTabPrint = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/PurchaseOrder/GePurchaseOrderReport?purchaseOrderId=" + data.Id;
    };
    $scope.PaymentModeList = [];
    $scope.PaymentModeByPaymentTerm = function () {
         
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/PurchaseOrder/PaymentModeByPaymentTerm?Id=' + $scope.productNew.PaymentTermId
        }).then(function successCallback(response) {
            $scope.PaymentModeList = response.data;
            $scope.productNew.PaymentMode = response.data[0].PaymentMode;

        });
    }
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



    //#endregion


    //#region Order Specific PO Material

    //#region all Tab Function of POBOQItem Index

    $scope.IsOwnVendor = 'OwnVendor';
    $scope.tab1 = 1;
    $scope.setOwnVendorTabIndex = function (newTab) {

        $scope.IsOwnVendor = 'OwnVendor';
        $scope.GetBOQItemList();

        $scope.tab1 = newTab;

    };
    $scope.isSetOwnVendorIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTabOtherVendorIndex = function (newTab) {
        //alert('tabCHR');

        $scope.IsOwnVendor = 'OtherVendor';
        $scope.GetListForMasterOrderOtherVendor = [];
        $scope.GetListForMasterOrdernew = [];
        $scope.taxCategoryList = [];
        $scope.groupList = [];
        $scope.Action1 = 'Save';
        $scope.ActionPOBOQ = 'Save';

        $scope.getalldataListForOtherVendorBOQList();
        $scope.tab1 = newTab;

    };
    $scope.isSetOtherVendorIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };


    $scope.setTabParentIndex = function (newTab) {


        $scope.IsOwnVendor = 'Parent';
        $scope.getalldataListForParentBOQList();
        $scope.tab1 = newTab;

    };
    $scope.isSetParentIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.changeIsTradingPO = function () {
        $scope.productNew.ContractId = null;
        $scope.productNew.ContractNo = null;
        $scope.productNew.CustomerName = null;
    }
    //#endregion
    $scope.GetBOQItemList = function () {

        $scope.GetListForMasterOrder = [];
        $scope.GetListForMasterOrderOtherVendor = [];
        $scope.groupList = [];
        $scope.GetListForMasterOrdernew = [];
        $scope.taxCategoryList = [];
        $scope.groupList = [];
        $scope.Action1 = 'Save';


        $scope.getalldataListForBOQList();
        $scope.ActionPOBOQ = 'Save';
    };
    $scope.groupList = [];
    $scope.processgroupList111 = function () {

        if ($scope.inventoryMaterialList.length > 0) {
            $scope.newlistitems = [];
            $scope.newlistitems = $scope.GetListForMasterOrder;
            $scope.GetListForMasterOrder = [];
            for (var i = 0; i < $scope.newlistitems.length; i++) {
                var getRow = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.newlistitems[i].MaterialMasterId, "ArticleId": $scope.newlistitems[i].ArticleId, "FirstCharacteristicsValueId": $scope.newlistitems[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.newlistitems[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.newlistitems[i].ThirdCharacteristicsValueId });
                //var getRow = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.newlistitems[i].MaterialMasterId });
                if (getRow.length == 0) {
                    $scope.GetListForMasterOrder.push($scope.newlistitems[i]);
                }
            }
        }
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('show');

    }
    $scope.processgroupListOtherVendor = function () {
        if ($scope.inventoryMaterialList.length > 0) {
            $scope.newlistitems = [];
            $scope.newlistitems = $scope.GetListForMasterOrderOtherVendor;
            $scope.GetListForMasterOrderOtherVendor = [];
            for (var i = 0; i < $scope.newlistitems.length; i++) {
                var getRow = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.newlistitems[i].MaterialMasterId, "ArticleId": $scope.newlistitems[i].ArticleId, "FirstCharacteristicsValueId": $scope.newlistitems[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.newlistitems[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.newlistitems[i].ThirdCharacteristicsValueId });
                //var getRow = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.newlistitems[i].MaterialMasterId });
                if (getRow.length == 0) {
                    $scope.GetListForMasterOrderOtherVendor.push($scope.newlistitems[i]);
                }
            }
        }
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('show');

    }

    $scope.GetListForMasterOrder = [];
    $scope.getalldataListForBOQList = function () {

        var gridObj = $("#GridReq").data("ejGrid");
        gridObj.clearFiltering();

        $scope.GetListForMasterOrder = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetBOQItems?ContractId=' + $scope.productNew.ContractId + '&VendorId=' + $scope.productNew.PartyCode + '&IsOwnVendor=' + $scope.IsOwnVendor + '&inveReveiveMasterId=' + $scope.productNew.Id + '&istradingPO=' + $scope.productNew.IsTradingPO,
        }).then(function successCallback(response) { //datagatefun			
            $scope.GetListForMasterOrder = [];
            $scope.GetListForMasterOrder = response.data;
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $scope.processgroupList111();
        });
        $scope.Action1 = 'Save';
        $scope.processgroupList1();
    };

    $scope.GetListForMasterOrderOtherVendor = [];
    $scope.getalldataListForOtherVendorBOQList = function () {
        var gridObj = $("#GridReqeee").data("ejGrid");
        gridObj.clearFiltering();
        $scope.GetListForMasterOrder = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetBOQItems?ContractId=' + $scope.productNew.ContractId + '&VendorId=' + $scope.productNew.PartyCode + '&IsOwnVendor=' + $scope.IsOwnVendor + '&inveReveiveMasterId=' + $scope.productNew.Id + '&istradingPO=' + $scope.productNew.IsTradingPO,
        }).then(function successCallback(response) { //datagatefun			
            $scope.GetListForMasterOrderOtherVendor = [];
            $scope.GetListForMasterOrderOtherVendor = response.data;
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $scope.processgroupListOtherVendor();
        });
        $scope.Action1 = 'Save';
        $scope.processgroupListOV();
    };
    $scope.GetListForMasterOrderParent = [];
    $scope.getalldataListForParentBOQList = function () {
        var gridObj = $("#GridReq3").data("ejGrid");
        gridObj.clearFiltering();
        $scope.GetListForMasterOrderParent = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetBOQItems?ContractId=' + $scope.productNew.ContractId + '&VendorId=' + $scope.productNew.PartyCode + '&IsOwnVendor=' + $scope.IsOwnVendor + '&inveReveiveMasterId=' + $scope.productNew.Id + '&istradingPO=' + $scope.productNew.IsTradingPO,
        }).then(function successCallback(response) { //datagatefun			
            $scope.GetListForMasterOrderParent = [];
            $scope.GetListForMasterOrderParent = response.data;
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();

        });
        $scope.Action1 = 'Save';

    };
    $scope.groupList = [];
    $scope.processgroupList1 = function () {
        if ($scope.inventoryMaterialList.length > 0) {
            $scope.newlistitems = [];
            $scope.newlistitems = $scope.GetListForMasterOrder;
            $scope.GetListForMasterOrder = [];
            for (var i = 0; i < $scope.newlistitems.length; i++) {
                var getRow = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.newlistitems[i].MaterialMasterId, "ArticleId": $scope.newlistitems[i].ArticleId, "FirstCharacteristicsValueId": $scope.newlistitems[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.newlistitems[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": $scope.newlistitems[i].ThitrdCharacteristicsValueId });
                if (getRow.length == 0) {
                    $scope.GetListForMasterOrder.push($scope.newlistitems[i]);
                }
            }
        }
        $scope.Action1 = 'Save';
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('show');

    }
    $scope.processgroupListOV = function () {
        if ($scope.inventoryMaterialList.length > 0) {
            $scope.newlistitems = [];
            $scope.newlistitems = $scope.GetListForMasterOrderOtherVendor;
            $scope.GetListForMasterOrderOtherVendor = [];
            for (var i = 0; i < $scope.newlistitems.length; i++) {
                var getRow = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.newlistitems[i].MaterialMasterId, "ArticleId": $scope.newlistitems[i].ArticleId, "FirstCharacteristicsValueId": $scope.newlistitems[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.newlistitems[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": $scope.newlistitems[i].ThitrdCharacteristicsValueId });
                if (getRow.length == 0) {
                    $scope.GetListForMasterOrderOtherVendor.push($scope.newlistitems[i]);
                }
            }
        }
        $scope.Action1 = 'Save';
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('show');

    }
    $scope.RequisitionListHide = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('hide');
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
    $scope.rowUpdateDataBound = function rowUpdateDataBound(e) {

        try {


            if ($scope.RowColor != e.data.MaterialGroupMasterName + e.data.UserName + e.data.StandardName + e.data.FirstCharacteristicsValue + e.data.SecondCharacteristicsValue + e.data.ThirdCharacteristicsValue) {
                $scope.isAlternative = $scope.isAlternative * -1;
                $scope.RowColor = e.data.MaterialGroupMasterName + e.data.UserName + e.data.StandardName + e.data.FirstCharacteristicsValue + e.data.SecondCharacteristicsValue + e.data.ThirdCharacteristicsValue;
            }
            if ($scope.isAlternative > 0)
                e.row.css("background-color", '#D3D3D3');
            else
                e.row.css("background-color", '#ffffff');

        } catch (e) {

        }
    }
    $window.onresize = function (event) {

        $scope.actionCompleteSelected();

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridReq").ejGrid("instance");
                var scrollerwidth = $("#ReqaddId").width();//Obtain the width of the container

                //   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
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
    $scope.trnRateDiff = function () {
        var newRate = $scope.GetListForMasterOrdernew[0].TransactionRate;

        for (var i = 0; i < $scope.GetListForMasterOrdernew.length; i++) {
            if (newRate != $scope.GetListForMasterOrdernew[i].TransactionRate) {
                ShowResult('Transaction Rate msut be same !', 'failure', 'ListOfPOMaterial1');
                return true;
            }
        }
        
        return false;
    }
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


    $scope.materialValidationForBOQItemUOMCheck = function () {
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


    }
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
                if ($scope.ActionPOBOQ === 'Update') {

                    ShowResult('Have you selected Same UOM?', 'failure', 'ListOfPOMaterial1');
                    return true;
                }
                else {
                    ShowResult('Have you selected Same UOM?', 'failure', 'ListOfPOMaterial');
                    return true;
                }


            }

        }
        return false;
    }
    $scope.ActionPOBOQ = 'Save';

    $scope.check = function () {
        var aa = 0;
        for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
            if ($scope.GetListForMasterOrder[i].CheckedStatus === true) {
                aa++;

            }
        }
        if (aa === 0) {
            ShowResult('Your selected Material is not Approved.Please see Approved Coulmn!', 'failure', 'ListOfPOMaterial');
            return false;
        }

    }
    $scope.detailPOSaveForBOQ = function () {
        ;
        try {
            $scope.check();
            $scope.GetListForMasterOrdernew = [];
            if ($scope.ActionPOBOQ === 'Save') {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    //if ($scope.GetListForMasterOrder[i].CheckedStatus === false && $scope.GetListForMasterOrder[i].TransactionQty > 0) {//(!(baseService.isUndefinedOrNull($scope.GetListForMasterOrder[i].TransactionQty) ||

                    if ((baseService.isUndefinedOrNull($scope.GetListForMasterOrder[i].TransactionQty) || $scope.GetListForMasterOrder[i].TransactionQty === 0) && $scope.GetListForMasterOrder[i].CheckedStatus === true) {
                        ShowResult('Enter the Selected  Material Qty', 'failure', 'ListOfPOMaterial');
                        return false;
                    }

                    if ($scope.GetListForMasterOrder[i].CheckedStatus === true && $scope.GetListForMasterOrder[i].RequiredQtyApproved === 'Yes' && $scope.GetListForMasterOrder[i].IncompleteMaterial === 'No') {
                        if ($scope.ActionPOBOQ === 'Save') {
                            if ((parseFloat($scope.GetListForMasterOrder[i].TransactionQty) + parseFloat($scope.GetListForMasterOrder[i].OtherPOQty)) > parseFloat($scope.GetListForMasterOrder[i].RequiredQtyPO)) {
                                ShowResult('Trasaction qty can not grater than booking Qty', 'failure', 'ListOfPOMaterial');
                                $scope.GetListForMasterOrder[i].TransactionQty = '';
                                return false;
                            }
                            if (baseService.isUndefinedOrNull($scope.GetListForMasterOrder[i].TransactionQty)) {
                                ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial');
                                return false;
                            }
                            if ($scope.GetListForMasterOrder[i].TransactionQty < 0) {
                                ShowResult('Negative Qty  not allowed', 'failure', 'ListOfPOMaterial');
                                return false;
                            }
                            if ($scope.GetListForMasterOrder[i].TransactionQty === 0 || $scope.GetListForMasterOrder[i].TransactionQty === 0.00 || $scope.GetListForMasterOrder[i].TransactionQty === 0.0) {
                                ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial');
                                return false;
                            }
                            if (baseService.isUndefinedOrNull($scope.GetListForMasterOrder[i].TransactionRate)) {
                                ShowResult('Enter the current rate.Zero not allowed', 'failure', 'ListOfPOMaterial');
                                return false;
                            }
                            if ($scope.GetListForMasterOrder[i].TransactionRate === 0 || $scope.GetListForMasterOrder[i].TransactionRate === 0.0 || $scope.GetListForMasterOrder[i].TransactionRate === 0.00) {
                                ShowResult('Enter the current rate.Zero not allowed', 'failure', 'ListOfPOMaterial');
                                return false;
                            }
                            if ($scope.GetListForMasterOrder[i].RequiredQtyApproved === 'No') {
                                ShowResult('Required Qty not yet Approved.So you can not take this material', 'failure', 'ListOfPOMaterial');
                                return false;
                            }
                            if ($scope.GetListForMasterOrder[i].IncompleteMaterial === 'Yes') {
                                ShowResult('This is incomplete material.So you can not take this material', 'failure', 'ListOfPOMaterial');
                                return false;
                            }

                            else {
                                $scope.GetListForMasterOrdernew.push($scope.GetListForMasterOrder[i]);

                            }
                        }
                    }

                }
            }
            else if ($scope.ActionPOBOQ === 'Update') {
                for (var i = 0; i < $scope.GetListForMasterOrderUpdate.length; i++) {
                    if ((baseService.isUndefinedOrNull($scope.GetListForMasterOrderUpdate[i].TransactionQty) || $scope.GetListForMasterOrderUpdate[i].TransactionQty === 0) && $scope.GetListForMasterOrderUpdate[i].CheckedStatus === true) {
                        ShowResult('Enter the Selected  Material Qty', 'failure', 'ListOfPOMaterial1');
                        return false;
                    }

                    if ($scope.GetListForMasterOrderUpdate[i].CheckedStatus === true && $scope.GetListForMasterOrderUpdate[i].RequiredQtyApproved === 'Yes' && $scope.GetListForMasterOrderUpdate[i].IncompleteMaterial === 'No') {
                        if ((parseFloat($scope.GetListForMasterOrderUpdate[i].TransactionQty) + parseFloat($scope.GetListForMasterOrderUpdate[i].OtherPOQty)) > parseFloat($scope.GetListForMasterOrderUpdate[i].RequiredQtyPO)) {
                            ShowResult('Trasaction qty can not grater than required Qty', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        if (baseService.isUndefinedOrNull($scope.GetListForMasterOrderUpdate[i].TransactionQty)) {
                            ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        if ($scope.GetListForMasterOrderUpdate[i].TransactionQty < 0) {
                            ShowResult('Negative Qty  not allowed', 'failure', 'ListOfPOMaterial');
                            return false;
                        }
                        if ($scope.GetListForMasterOrderUpdate[i].TransactionQty === '0' || $scope.GetListForMasterOrderUpdate[i].TransactionQty === '0.00' || $scope.GetListForMasterOrderUpdate[i].TransactionQty === '0.0') {
                            ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        if (baseService.isUndefinedOrNull($scope.GetListForMasterOrderUpdate[i].TransactionRate)) {
                            ShowResult('Enter the current rate.Zero not allowed', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        if ($scope.GetListForMasterOrderUpdate[i].TransactionRate === 0 || $scope.GetListForMasterOrderUpdate[i].TransactionRate === 0.0 || $scope.GetListForMasterOrderUpdate[i].TransactionRate === 0.00) {
                            ShowResult('Enter the current rate.Zero not allowed', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }

                        if ($scope.GetListForMasterOrderUpdate[i].RequiredQtyApproved === 'No') {
                            ShowResult('Required Qty not yet Approved.So you can not take this material', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        if ($scope.GetListForMasterOrderUpdate[i].IncompleteMaterial === 'Yes') {
                            ShowResult('This is incomplete material.So you can not take this material', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        else {
                            $scope.GetListForMasterOrdernew.push($scope.GetListForMasterOrderUpdate[i]);
                        }

                    }
                }
            }


            for (var j = 0; j < $scope.GetListForMasterOrdernew.length; j++) {
                if ($scope.GetListForMasterOrdernew[j].CheckedStatus === true) {
                    $scope.tempList.push($scope.GetListForMasterOrdernew[j]);
                }
            }

            if ($scope.GetListForMasterOrdernew.length === 0) {
                if ($scope.ActionPOBOQ === 'Update') {

                    ShowResult('Please select at least one material', 'failure', 'ListOfPOMaterial1');
                    return false;
                }
                else {
                    ShowResult('Please select at least one material', 'failure', 'ListOfPOMaterial');
                    return false;
                }

            }

            $scope.UOMValidation();
            $scope.groupList = [];
            $scope.processgroupList($scope.GetListForMasterOrdernew, $scope.groupList);

            for (var i = 0; i < $scope.GetListForMasterOrdernew.length; i++) {
                $scope.GetListForMasterOrdernew[i].Tolerance = $scope.productNew.Tolerance;
            }
            for (var i = 0; i < $scope.groupList.length; i++) {
                $scope.groupList[i].Tolerance = $scope.productNew.Tolerance;
            }


            if ($scope.ActionPOBOQ === 'Save') {
                $scope.materialValidationForBOQItem();
                if (!$scope.UOMValidation()) {//$scope.invalid &&

                    $http({
                        method: 'POST',
                        url: 'Products/PurchaseOrder/detailPOSaveForBOQ',
                        data: {
                            entity: JSON.stringify($scope.GetListForMasterOrdernew)
                            , taxCategoryList: $scope.taxCategoryList//$scope.taxCategoryList
                            , PoId: $scope.productNew.Id
                            , groupList: JSON.stringify($scope.groupList)
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
                            entity: JSON.stringify($scope.GetListForMasterOrdernew)
                            , taxCategoryList: $scope.taxCategoryList
                            , PoId: $scope.productNew.Id
                            , groupList: JSON.stringify($scope.groupList)
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
    $scope.getTaxCategoryList1 = function ($event) {
        //
        if ($event.isInteraction == false)
            return;
        var gridObj = $("#GridReq").ejGrid("instance");
        var currRow = gridObj.model.currentViewData[this.element.closest("tr").index()];
        var x = $event;
        if (currRow.RequiredQtyApproved === 'No') {
            ShowResult('Required qty not yet approved.You can not take this material', 'failure', 'ListOfPOMaterial');
            return false;
        }
        else if (currRow.IncompleteMaterial === 'Yes') {
            ShowResult('This is incomplete material.You can not take this material', 'failure', 'ListOfPOMaterial');
            return false;
        }

        //var Id = x.data.Id;
        for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
            if ($scope.GetListForMasterOrder[i].CheckedStatus === true) {
                $scope.RequisitionDetailId = currRow.RequisitionDetailId;
                if ($scope.GetListForMasterOrder[i].RequisitionDetailId === currRow.RequisitionDetailId) {

                    if ((parseFloat($scope.GetListForMasterOrder[i].PORaisedQty) + parseFloat($scope.GetListForMasterOrder[i].TransactionQty)) > parseFloat($scope.GetListForMasterOrder[i].ReqQty)) {
                        $scope.GetListForMasterOrder[i].WantToClose = true;
                    }
                    else if ((parseFloat($scope.GetListForMasterOrder[i].PORaisedQty) + parseFloat($scope.GetListForMasterOrder[i].TransactionQty)) === parseFloat($scope.GetListForMasterOrder[i].ReqQty)) {
                        $scope.GetListForMasterOrder[i].WantToClose = true;
                    }
                    else {
                        $scope.message = 'Do you want to close this line item?';
                        angular.element(document.querySelector('#ConfirmationForReqClosePopUp')).modal('show');
                        // $scope.GetListForMasterOrder[i].WantToClose = false;
                    }
                }
            }
            else {
                $scope.GetListForMasterOrder[i].WantToClose = false;
            }




        }
        //var Id = x.data.Id;
        var hsnCodeId = currRow.hsnCodeId;
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
        });
    }

    //Update Process
    $scope.PODetailsUpdatePOPUp = function (x) {
        ;
        //$scope.uom();
        $scope.ActionPOBOQ = "Update";
        getInventoryMaterialListForUpdate(x.InventoryReceiveDetailId, x.InventoryMaterialId, x.ArticleId, x.FirstCharacteristicsValueId, x.SecondCharacteristicsValueId, x.ThirdCharacteristicsValueId);
    };
    function getInventoryMaterialListForUpdate(inveReveiveId, MaterialMasterId, ArticleId, FirstCharacteristicsValueId, SecondCharacteristicsValueId, ThirdCharacteristicsValueId) {
        $scope.masterId = inveReveiveId;

        $http.get($scope.path + 'GetBOQItemsListForUpdate?ContractId=' + $scope.productNew.ContractId + '&VendorId=' + $scope.productNew.PartyCode + '&inveReveiveId=' + inveReveiveId + '&inveReveiveMasterId=' + $scope.productNew.Id + '&MaterialMasterId=' + MaterialMasterId + '&ArticleId=' + ArticleId + '&FirstCharacteristicsValueId=' + FirstCharacteristicsValueId + '&SecondCharacteristicsValueId=' + SecondCharacteristicsValueId + '&ThirdCharacteristicsValueId=' + ThirdCharacteristicsValueId)
            .then(function (response) {
                $scope.GetListForMasterOrderUpdate = [];
                $scope.GetListForMasterOrderUpdate = response.data;
            });
        angular.element(document.querySelector('#ListOfPOMaterial1')).modal('show');

    }
    $scope.detailPOSaveForBOQHide = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListOfPOMaterial1')).modal('hide');
    };

    $scope.ClearList = function (data) {
        ;
        $scope.inventoryMaterialList = [];
        $scope.OrderSpecific = data;

    };
    $scope.ConvertedDataRowList = [];
    $scope.GetListForMasterOrderTemp = [];
    $scope.ConvertedDataRow = function (data) {
        var gridObj = $("#GridReq").data("ejGrid");
        var gridObjUpdate = $("#PODetailUpdate").data("ejGrid");
        //var x = $event;
        //var res = x.data;
        ;
        $http({
            method: 'POST',
            url: $scope.path + 'ConverttedBOQUOMData',
            data: {
                'data': data
            },
            dataType: 'JSON'
        }).then(function (response) {
            $scope.ConvertedDataRowList = response.data;
            for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                if ($scope.GetListForMasterOrder[i].BOQId === $scope.ConvertedDataRowList.data.BOQId) {
                    $scope.GetListForMasterOrder[i].RequiredQtyPO = $scope.ConvertedDataRowList.data.RequiredQtyPO;
                    $scope.GetListForMasterOrder[i].OtherPOQty = $scope.ConvertedDataRowList.data.OtherPOQty;
                    $scope.GetListForMasterOrder[i].TransactionQty = $scope.ConvertedDataRowList.data.TransactionQty;

                }
            }
            gridObj.refreshContent(true);
            gridObjUpdate.refreshContent(true);

            gridObj.refreshTemplate();
            gridObjUpdate.refreshTemplate();

        });

    };
    //#endregions


    $scope.checkRowValidation = function (x) {
        ;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            if ($scope.receiveTaxList[i].Id === x.Id) {
                $scope.receiveTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100);
            }
        }
    }



    $scope.TaxOption = function (data) {
        ;
        $scope.productNew.TaxOption = data;
    };
    $scope.TaxOptionMat = function (data) {
        ;
        $scope.productNew.TaxOptionMat = data;

    };
    $scope.TaxOptionService = function (data) {
        ;
        $scope.productNew.TaxOptionService = data;

    };
    $scope.TaxOptionServiceModify = function (data) {
        ;
        $scope.productNew.TaxOptionServiceModify = data;

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
        ;
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
                url: 'Products/PurchaseOrder/PODocCreate',
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
                    $scope.ImagedataLoad();
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
            url: 'Products/PurchaseOrder/PODocumentMapData?POID=' + $scope.productNew.Id,
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
                url: 'Products/PurchaseOrder/POImageDelete?Id=' + $scope.DocId,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ImagedataLoad();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }


    };



    //#endregion 


    $scope.detailPopUpEdit = function () {
        for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
            for (var t = 0; t < $scope.inventoryMaterialList[i].TaxList.length; t++) {
                if ($scope.inventoryMaterialList[i].TransactionRate === 0 || $scope.inventoryMaterialList[i].TransactionRate === '0.0' || $scope.inventoryMaterialList[i].TransactionRate === '') {
                    ShowResult('Enter Rate', 'failure');
                    return false;
                }
                else if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].DeliveryDate)) {
                    ShowResult('Enter Delivery Date', 'failure');
                    return false;
                }
                $scope.receiveTaxList.push($scope.inventoryMaterialList[i].TaxList[t]);
            }
            if ($scope.productNew.Tolerance > 0 && $scope.inventoryMaterialList[i].Tolerance == 0) {
                $scope.inventoryMaterialList[i].Tolerance = $scope.productNew.Tolerance;
            }
        }
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/UpdateMaterial',
            data: {
                entity: $scope.inventoryMaterialList,
                receiveTaxList: $scope.receiveTaxList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
               
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };

    };


    // #region checkbox all



    $scope.refreshTemplateemployee = function (args) {
        $("#headchk111").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };



    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridReq").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                $scope.GetListForMasterOrder[i].CheckedStatus = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridReq").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.refreshUpdateTemplate = function (args) {
        $("#headchkupdate").ejCheckBox({ "change": CheckBoxSelectAllUpdate });
    };
    $scope.refreshQtyTemplete = function (args) {
        var gridObj = $("#GridReq").data("ejGrid");
        /*		gridObj.refreshContent();*/
        gridObj.refreshTemplate(true);
    }


    function CheckBoxSelectAllUpdate(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridReq").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.GetListForMasterOrderUpdate.length; i++) {
                $scope.GetListForMasterOrderUpdate[i].CheckedStatus = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#PODetailUpdate").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.refreshQtyUpdateTemplete = function () {
        var gridObj = $("#PODetailUpdate").data("ejGrid");
        /*gridObj.refreshContent();*/
        gridObj.refreshTemplate(true);
    }

    // #endregion checkbox all


    $scope.BOQItemsDetailsDataList = [];
    $scope.GetBOQItemsDetailsData = function () {
        $scope.BOQItemsDetailsDataList = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetBOQItemsDetailsData'
        }).then(function successCallback(response) {
            $scope.BOQItemsDetailsDataList = response.data;
        });
        angular.element(document.querySelector('#ListMaterial')).modal('show');
    };
    $scope.BOQItemsDetailsDataListHide = function () {
        angular.element(document.querySelector('#ListMaterial')).modal('hide');
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

    $scope.detailTempTitle = "#tabGridContentsTitle";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgridTitle = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];

        var data = ej.DataManager($scope.TermsAndConditionDetailGridList).executeLocal(ej.Query().where("TermsAndConditionPOChildId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGridTitle").ejGrid({
            dataSource: data,
            columns: ["HeaderCaption", "Description"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }

    $scope.showTermsAndConditionDetailPopUp = function (args) {
        $scope.TitleId = args.TermsAndConditionPOChildId;
        $scope.POPupList = [];
        $scope.GetRemarksByMaster($scope.TitleId);
        angular.element(document.querySelector('#GridPopUp')).modal('show');
    }
    $scope.message_detailconfirmation = null;
    $scope.removeBoMDetail = function (obj) {
        $scope.TitleModel = obj.data;
        if (!baseService.isUndefinedOrNull($scope.TitleModel.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.TitleModel.Title + ' ]';
        angular.element(document.querySelector('#confirmBoMDetailPopUp')).modal('show');
    }

    $scope.DeleteBomDetail = function () {
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/DeleteTitle?id=' + $scope.TitleModel.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadTermsAndConditionGrid($scope.productNew.TermsAndConditionsId, $scope.productNew.Id);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.closeRemarksPopUp = function () {

        angular.element(document.querySelector('#GridPopUp')).modal('hide');
    }

    $scope.POPupList = [];

    $scope.GetRemarksByMaster = function (id) {
        $scope.POPupList = [];
        $http.get('Products/PurchaseOrder/GetPopUp?TermsAndConditionsPODetailId=' + id)
            .then(function successCallback(response) {
                $scope.POPupList = response.data;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            })
    }

    $scope.DeletePODetailPOPUp = function (model) {
        try {

            $http({
                method: 'POST',
                url: 'Products/PurchaseOrder/DeletePODetailPOPup',
                data: { id: model.data.Id }
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetRemarksByMaster($scope.TitleId);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.SaveGrid = function (model) {
        //$scope.TitleModel.TermsAndConditionsMasterId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.saveGridUrl,
            data: { 'GridData': model.data, 'TitleId': $scope.TitleId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetRemarksByMaster($scope.TitleId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };
    //$window.onresize = function (event) {
    //	$scope.actionComplete();
    //};
    $scope.TitleModel = {
        Id: null,
        TermsAndConditionsMasterId: $scope.TermsAndConditions.Id,
        Title: null,
        Sequence: 0
    }
    $scope.SaveTitle = function () {
        try {
            $scope.TermsAndConditionGridNewList = [];
            for (var i = 0; i < $scope.TermsAndConditionGridList.length; i++) {
                $scope.TermsAndConditionGridList[i].Id = null;

            }
            $scope.TermsAndConditionGridNewList = $scope.TermsAndConditionGridList;
            $scope.TitleModel.TermsAndConditionsMasterId = $scope.TermsAndConditions.Id;
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveTitleUrl,
                data: { 'TitleData': $scope.TitleModel, 'TitleId': $scope.TermsAndConditions.Id, 'TermsAndConditionGridList': $scope.TermsAndConditionGridNewList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.TitleModel = {
                        Id: null,
                        TermsAndConditionsMasterId: $scope.TermsAndConditions.Id,
                        Title: null
                    };
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure')
        }

    };
    $scope.SaveTermsDetail = function () {
        try {
            $scope.TitleModel.TermsAndConditionsMasterId = $scope.TermsAndConditions.Id;
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveTermsDetail,
                data: { 'TitleId': $scope.productNew.TermsAndConditionChildId, 'POId': $scope.productNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.TitleModel = {
                        Id: null,
                        TermsAndConditionsMasterId: $scope.TermsAndConditions.Id,
                        Title: null
                    };
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure')
        }

    };



    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TransactionQty", dataMember: "TransactionQty", format: "{0:N4}" }]
        /*,showCaptionSummary: true*/
    }];




}//End Of main

