'use strict';
ServiceRequisitionController.$inject = ['accountService', 'addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function ServiceRequisitionController(accountService, addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    
    $rootScope.title = "Service Requisition";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/ServiceRequisition/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.updateByIdUrl = $scope.path + 'DetailEdit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete/';
    $scope.sreviceSaveUrl = $scope.path + 'createSreviceReqDetail';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
    $scope.partyType = 'Vendor';
    $scope.isAdvance = false;
    $scope.currentDate = new Date(Date.now());
    $scope.grossTotal = 0;
    $scope.PartyId = null;
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $scope.inventoryMaterialList = [];
    $scope.chargesList = [];
    $scope.ChargeTaxList = [];
    $scope.StateData = [];



    //#region notification setting for Service Requisition

    $scope.NotificationSettingStatus = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/ServiceRequisition/ServiceRequisitionCreationNotificationSetting',
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
                url: 'Products/ServiceRequisition/GetCheckedByAndApprovedBYServiceRequisitionCreation?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.checkedByList = response.data;
            });

        }
        else {

        }

    }

//#endregion


    ////#region  Req Detail
    $scope.lst = [];
    $scope.ReqListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/ServiceRequisition/GetAllReqdataDetails'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.lst = response.data;
        });
    }
    $scope.ReqListDetails();
    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("ServiceRequisitionMasterID", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            //columns: ["MaterialGroupName", "MaterialName", "ArticleName", "SKU1", "SKU2", "SKU3","MaterialDetail", "TransactionQty", "TransactionUoM", "EstimatedRate", "CurrencyName", "TotalAmount" ]
            //
            columns: [{ field: "ServiceMasterName", headerText: "ServiceMasterName", width: 150 },
                { field: "CurrencyName", headerText: "CurrencyName", width: 150 },
                { field: "Qty", headerText: "Qty", width: 100 },
                { field: "UoM", headerText: "UoM", width: 100 },
                { field: "TransactionRate", headerText: "Rate", width: 100 },              
                { field: "ToCurrencyRate", headerText: "ToCurrencyRate", width: 100 },
                { field: "CurrencyName", headerText: "Currency Name", width: 100 },
                { field: "TotalServiceTranAmount", headerText: "Amount(TRN)", width: 150 },
                { field: "TotalServiceBooksCurrencyAmount", headerText: "Amount(BC)", width: 150 },
            ]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    //#endregion
    
    $scope.currencyList = [];

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

    $scope.getToCurrencyRate = function () {
        //debugger;
        $http.get($scope.path + 'GetToCurrencyRate?currencyId=' + $scope.serviceReqDetailModel.CurrencyId)
            .then(function (response) {
                if (parseFloat(response.data) === 0) {

                    $scope.serviceReqDetailModel.Rate = 1;
                    $scope.serviceReqDetailModel.CurrencyName = angular.element("#currency :selected").text();
                }
                else {

                    $scope.serviceReqDetailModel.Rate = response.data;
                    $scope.serviceReqDetailModel.CurrencyName = angular.element("#currency :selected").text();
                }
            });
    };


    $scope.Get = function (index) {
        $scope.index = index;
        $scope.product = $scope.products[$scope.index];
        $scope.productNew = Object.assign({}, $scope.product);
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
        //$scope.GetReq();
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
    
    function getInventoryMaterialList(inveReveiveId) {
        $scope.masterId = inveReveiveId;
        //debugger;
        $scope.inventoryMaterialList = [];
        $http.get($scope.path + 'GetInventoryMaterialList?inveReveiveId=' + inveReveiveId)
            .then(function (response) {

                $scope.inventoryMaterialList = response.data.Rows;
                
            });
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
    
    $scope.closeServiceChargeTaxPopUp = function () { //hossain
        ////debugger;

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

        //if ($scope.TAction === "OK") {
        $http({
            method: 'POST',
            //url: $scope.saveUrl,
            url: 'Products/ServiceRequisition/InsertserviceTax',
            //data: $scope.receiveTaxList,
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
        // }

        //angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

    }

    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];

        $http({
            method: 'GET',
            url: 'Products/Requisition/GetFiscalYear?formattedDate=' + data.RequisitionDate,
        }).then(function successCallback(response) {
            $scope.startDate = response.data[0].StartDate;
            $scope.endDate = response.data[0].EndDate;
            //location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id + '&startDate=' + $scope.startDate + '&endDate=' + $scope.endDate;
            location.href = "Products/ServiceRequisition/ServiceRequisitionReportby?RequisitionId=" + data.Id + '&startDate=' + $scope.startDate + '&endDate=' + $scope.endDate;
        });

    };
    
    $scope.changeServiceForFG = function () {
        //debugger;

        $scope.serviceModel.CurrencyName = "INR";
        $scope.serviceModel.ToCurrencyRate = 1;
        if (baseService.isUndefinedOrNull($scope.serviceReqDetailModel.ServiceMasterId))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceReqDetailModel.ServiceMasterId; })[0].HSNCodeId;
        getTaxCategoryListForFGService(hsnCodeId);
    };

    $scope.getServiceTaxFGList = function (data, flag, ServiceId, index) {

        //debugger;
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
            //$scope.receiveTaxList = $scope.ChargeTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');
        
    }

    $scope.AddReceiveTaxPopUpFG = function (Id, index) { //hossain
        //debugger;
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

//#region Model Of ServiceRequisitionMaster
    $scope.product = {
        Id: null,
        CompanyGroupId: $window.companyGroupId,
        EntityId: null,
        RequisitionType: null,
        RequirmentType: null,
        QualityApprovalResponsiblePersonId: null,
        NeedSpecialAppId: null,
        ReasonWhyItIsNotPlanEarlier: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null,
        RequisitionDate: $filter("dateFiltering")(Date.now()),
        Remarks: null,
        CheckedBy: null,
        CheckedByStatus: null,
        AuthorizedBy: null,
        AuthorizedByStatus: null,
        IsApproved: null,
        RequisitionStatus: null,
        ResponsiblePersonName: null,
        EmployeeName: null,
        CheckedHoldRejectReason: null,
        ReasonWhyItIsNotPlanEarlier: null,
        labelCheckAndApproved: null
        

    };
    $scope.productNew = Object.assign({}, $scope.product);


    $scope.Clear = function () {
        ClearFields();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
        return true;
    };

    function ClearFields()

  

    {
        $scope.Action = "Save";
        $scope.chargesList = [];
        $scope.product = {};
        $scope.IsBaseOnDueDateEnable = false;
        $scope.productNew = {
            Id: null,
            CompanyGroupId: $window.companyGroupId,
            EntityId: null,
            RequisitionType: null,
            RequirmentType: null,
            QualityApprovalResponsiblePersonId: null,
            NeedSpecialAppId: null,
            ReasonWhyItIsNotPlanEarlier: null,
            AddedBy: null,
            AddedDate: new Date(),
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
            UpdatedFromIP: null,
            RequisitionDate: $filter("dateFiltering")(Date.now()),
            Remarks: null,
            CheckedBy: null,
            CheckedByStatus: null,
            AuthorizedBy: null,
            AuthorizedByStatus: null,
            IsApproved: null,
            RequisitionStatus: null,
            ResponsiblePersonName: null,
            EmployeeName: null,
            Rate: null,
            CurrencyId: null,
            CheckedHoldRejectReason: null,
            ReasonWhyItIsNotPlanEarlier: null,
            
        };
        $scope.detailModel.NeedSpecialAppId = $scope.productNew.NeedSpecialAppId;
        $scope.detailModel.QualityApprovalResponsiblePersonId = $scope.productNew.QualityApprovalResponsiblePersonId;
        $scope.inventoryMaterialList = [];
       
        $scope.grossTotal = 0;
        baseService.removeErrorClasses();
        $scope.detailModel.NeedSpecialAppId = $scope.productNew.NeedSpecialAppId;
        $scope.detailModel.QualityApprovalResponsiblePersonId = $scope.productNew.QualityApprovalResponsiblePersonId;
        //$scope.getToCurrencyRate();
    }

    //#endregion

 //#region Dropdown list

    $scope.EntityList = [];
    $scope.GetEntity = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/ServiceRequisition/GetEntity'
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        });
    }
    $scope.GetEntity();
    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
    });
    cboService.getCboEntityByPlant(null, null, '', function (result) {
        $scope.EntityList = result;
    });
    //#endregion

//#region Save Update Delete

    $scope.Save = function () {
        //debugger;
        if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
            ShowResult("Please select to be approved by", 'failure');
            return false;
        }
        else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
            ShowResult("Please select to be checked by", 'failure');
            return false;
        }
        $scope.productNew.CheckedByStatusForNoti = $scope.CheckedByStatusForNoti;
        $scope.productNew.ApprovedByStatusForNoti = $scope.ApprovedByStatusForNoti;
        try {
            $scope.$broadcast('show-errors-check-validity');
            $scope.product = Object.assign({}, $scope.productNew);
            // if ($scope.productNewForm.$valid) {
            if (baseService.isUndefinedOrNull($scope.productNew.RequirmentType)) {
                ShowResult("Please select Requirment Type", 'failure');
                return false;
            }
            else if (baseService.isUndefinedOrNull($scope.productNew.EntityId)) {
                ShowResult("Please select Entity", 'failure');
                return false;
             }
            else if (baseService.isUndefinedOrNull($scope.productNew.RequisitionType)) {
                 ShowResult("Please select Requisition Type", 'failure');
                return false;
            }
            //else if (baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
            //     ShowResult("Please select Checked By", 'failure');
            //     return false;
            // }
            
            else {
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data:
                        {
                            'entity': $scope.product,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                        },
                        //data: $scope.product,
                        dataType: 'JSON'
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.productNew.Id = response.data.entity.Id;
                            //$scope.productNew.PartyName = $scope.product.PartyName;

                            $scope.Action = "Update";
                            $scope.GetReq();
                           
                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {

                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data:
                        {
                            'entity': $scope.product,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                        },
                        //data: $scope.product,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            //$scope.getDataList();
                            $scope.GetReq();
                           // $scope.setTab(1);
                            //$scope.setTabReqList(1);
                            $scope.getalldata();
                           
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
           // }
            }
            
        } catch (e) {
            throw e;
        }
    };
    $scope.clearNeedSpecialApproval = function () {
        $scope.productNew.EmployeeName = null;
        $scope.NeedSpecialAppId = null;
    }
    $scope.clearQARespPerson = function () {
        $scope.productNew.ResponsiblePersonName = null;
        $scope.QualityApprovalResponsiblePersonId = null;

    }

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
                        //$scope.getDataList();
                       
                        $scope.RequisitionUnapproved();
                        $scope.Requisitionapproved();
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
    //#endregion

 //#region EmployeePOPUP
    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.productNew.EmployeeName = employee.EmployeeName;
            $scope.productNew.NeedSpecialAppId = employee.SystemId;

        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };
    $scope.closeResponsiblePersonPopUp = function () {
        if ($scope.responsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.responsiblePersonIndex];
            $scope.productNew.ResponsiblePersonName = employee.EmployeeName;
            $scope.productNew.QualityApprovalResponsiblePersonId = employee.SystemId;

        }
        $scope.hideResponsiblePersonPopUp();
    };
    $scope.hideResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };

    //#endregion

//#region All CBO function

    $scope.checkedByList = [];
    $scope.GetSupervisorCboList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/ServiceRequisition/GetSupervisorCbo'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
        console.log('checkedByList', $scope.checkedByList);
    }
    $scope.GetSupervisorCboList();


//#endregion

//#region  ServiceRequisition Creation All Tab 

    $scope.ReqStatus = 'ForChecked';

    $scope.tab = 1;
    $scope.setTabReqList = function (newTab) {
       
        $scope.ReqStatus = 'ForChecked';
        $scope.GetReq();
        $scope.ReqListDetails();
        $scope.tab = newTab;

    };
    $scope.isSetReqList = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GetReq();
    };
    $scope.setTabReqList1 = function (newTab) {
       
        $scope.ReqStatus = 'HoldReject';
        $scope.GetReq();
        $scope.ReqListDetails();
        $scope.tab = newTab;

    };
    $scope.isSetReqList1 = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabReqList2 = function (newTab) {
        $scope.tab = newTab;
        $scope.ReqStatus = 'Checked';
        $scope.GetReq();
        $scope.ReqListDetails();
        $scope.tab = newTab;

    };
    $scope.isSetReqList2 = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabReqApproved3 = function (newTab) {
        //debugger;
        $scope.tab = newTab;
        $scope.ReqStatusApproval = 'HoldReject';
        $scope.GetReq1();

    };
    $scope.isSetReqApproved3 = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.setTabReqApproved4 = function (newTab) {
        $scope.tab = newTab;
        $scope.ReqStatusApproval = 'Approval';
        $scope.GetReq1();

    };
    $scope.isSetReqApproved4 = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.setTabReqApproved5 = function (newTab) {
        $scope.tab = newTab;
        //$scope.ReqStatusApproval = 'Approval';
        $scope.ReqListDetails1();
    };
    $scope.isSetReqApproved5 = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.setTabReqApproved6 = function (newTab) {
        $scope.tab = newTab;
        //$scope.ReqStatusApproval = 'Approval';
        $scope.getRequisitionByEmpInMonth();
    };
    $scope.isSetReqApproved6 = function (tabNum) {
        return $scope.tab === tabNum;
    };
    //#endregion Requisition Tab


//#region All Grid function

    $scope.ReqList = [];
    $scope.ReqStatus = 'ForChecked';
    $scope.GetReq = function () {
        if ($scope.ReqStatus === 'ForChecked') {
            $scope.ReqStatus = 'ForChecked';
        }
        else {

        }
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/ServiceRequisition/GetAllReqdata?ReqStatus=' + $scope.ReqStatus
        }).then(function successCallback(response) {
            $scope.ReqList = response.data;
        });
    }
    $scope.GetReq();

    $scope.ReqList1 = [];
    $scope.ReqStatusApproval = 'HoldReject';
    $scope.GetReq1 = function () {

        //debugger;
        if ($scope.ReqStatusApproval === 'HoldReject') {
            $scope.ReqStatusApproval = 'HoldReject';
        }
        else {

        }
        $http({
            method: 'GET',
            url: 'Products/ServiceRequisition/GetAllReqdata1?ReqStatusApproval=' + $scope.ReqStatusApproval
        }).then(function successCallback(response) {
            $scope.ReqList1 = response.data;
        });
    }
     //$scope.GetReq1();


//#endregion

    RequisitionDate: $filter("dateFiltering")(Date.now()),
        $scope.recorddoubleclick = function ($event) {
            //debugger;
            var x = $event;
            var Id = x.data.Id;
            $scope.Currency = $("#currency option:selected").text();
            $scope.productNew = x.data;
        $scope.Id = $scope.productNew.Id;
        getServiceChargeList(Id);
        
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) $rootScope.toggle();
        };

    $scope.Getrecorddoubleclick = function ($event, index) {
        //debugger;
        // alert('Do you want to see Material Details');
        var x = $event;
        var Id = x.data.Id;
        $scope.MONo = Id;
        getMasterItemList();
        angular.element(document.querySelector('#ListOfMasterOrder')).modal('hide');
    };


    function getInventoryMaterialList(inveReveiveId) {
        $scope.masterId = inveReveiveId;
        //debugger;
        $scope.inventoryMaterialList = [];
        $http.get($scope.path + 'GetInventoryMaterialList?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.inventoryMaterialList = response.data.Rows;
                
            });

    }


    $scope.Griddata = [];
    $scope.getalldata = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/ServiceRequisition/GetListForHold',
        }).then(function successCallback(response) {
            $scope.Griddata = response.data;
            //entrydata = copy(searchdata);
        });
    };

    //#endregion
    // #region Service
    $scope.serviceChargePopUp = function () {
        $scope.serviceReqDetailModel.Rate = 0;
        $scope.serviceReqDetailModel.Qty = 0;
        $scope.serviceReqDetailModel.TransactionRate=0;
        $scope.uom();
        angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
    };
    $http.get('Setups/CompanyServiceMaster/GetCboList')

        .then(function (response) {
            $scope.serviceList = response.data;
        });

    $scope.closeServiceChargePopUp = function () {
        $scope.serviceReqDetailModel = {};
        $scope.receiveTaxList = [];
        $scope.serviceReqDetailModel.Rate = 0;
        $scope.serviceReqDetailModel.Qty = 0;
        $scope.serviceReqDetailModel.TransactionRate = 0;
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    };
    $scope.serviceSelectionList = [];
    $scope.showServiceSelectionPopUp = function () {
        $http.get('Setups/CompanyServiceMaster/GetServicePopUpByCompany')
            .then(function (response) {
                $scope.serviceSelectionList = response.data;
            });
        angular.element(document.querySelector('#serviceSelectionPopUp')).modal('show');
    };

    $scope.closeServiceSelectionPopUp = function () {
        angular.element(document.querySelector('#serviceSelectionPopUp')).modal('hide');
    };
    $scope.SelectPopUpService = function (args) {
        $scope.serviceReqDetailModel.ServiceMasterId = null;
        $scope.serviceReqDetailModel.ServiceMasterId = args.data.Value;
        angular.element(document.querySelector('#serviceSelectionPopUp')).modal('hide');
    }

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


    $scope.serviceReqDetailModel = {
         Id: null
        , ServiceRequisitionMasterID: $scope.productNew.Id
        , CurrencyId: null
        , Rate: 0
        , ServiceMasterId: null
        , TotalServiceTranAmount: 0
        , TotalServiceBooksCurrencyAmount: 0
        , CurrencyName: angular.element("#currency :selected").text()
        , AddedBy:null
        , AddedDate: null
        , AddedFromIP: null
        , UpdatedBy: null
        , UpdatedDate: null
        , UpdatedFromIP: null
        , Remarks: null
        , Description: null
        , RefferenceNo: null
        , TransactionRate: 0
        , Qty: 0
        , TransactionUoMId: null
        
       
    };

    $scope.serviceSave = function () {
        debugger;
        try {
            $scope.manualValidationAddRemove('div_svc', 'serviceReqDetailModel', 'ServiceMasterId');
            $scope.manualValidationAddRemove('div_svcRate', 'serviceReqDetailModel', 'TotalServiceTranAmount');
            $scope.serviceReqDetailModel.ServiceRequisitionMasterID = $scope.productNew.Id;
            if (baseService.isUndefinedOrNull($scope.serviceReqDetailModel.ServiceMasterId)) {
                ShowResult("Please select Service", 'failure','serviceChargePopUp');
                return false;
            }
            else if (baseService.isUndefinedOrNull($scope.serviceReqDetailModel.Qty) || $scope.serviceReqDetailModel.Qty === 0) {
                ShowResult('Enter the Qty', 'failure', 'serviceChargePopUp');
                return false;
            }
            else if (baseService.isUndefinedOrNull($scope.serviceReqDetailModel.TransactionUoMId)) {
                ShowResult('Select The UoM', 'failure', 'serviceChargePopUp');
                return false;
            }
            else if (baseService.isUndefinedOrNull($scope.serviceReqDetailModel.Rate) || $scope.serviceReqDetailModel.Rate === 0) {
                ShowResult('select Currency', 'failure', 'serviceChargePopUp');
                return false;
            }
            else if (baseService.isUndefinedOrNull($scope.serviceReqDetailModel.TotalServiceTranAmount) || $scope.serviceReqDetailModel.TotalServiceTranAmount === 0) {
                ShowResult('Enter the Qty and Rate', 'failure', 'serviceChargePopUp');
                return false;
            }
            else if (baseService.isUndefinedOrNull($scope.serviceReqDetailModel.CurrencyId)) {
                ShowResult("Please select currency", 'failure', 'serviceChargePopUp');
                return false;
            }

            else if (baseService.isUndefinedOrNull($scope.serviceReqDetailModel.TotalServiceTranAmount)) {
                ShowResult("Please enter the amount", 'failure', 'serviceChargePopUp');
                return false;
            }

            else if ($scope.serviceReqDetailModel.TotalServiceTranAmount === 0) {
                ShowResult("Please enter the amount", 'failure', 'serviceChargePopUp');
                return false;
            }
            
            else {
                $http({
                    method: 'POST',
                    url: $scope.sreviceSaveUrl,
                    data: {
                        entity: $scope.serviceReqDetailModel
                        //, taxCategoryList: $scope.taxCategoryList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                    else {
                        ShowResult(response.data.Message, 'success', 'serviceChargePopUp');
                        $scope.setTab(1);
                        $scope.setTabReqList(1);
                        $scope.serviceReqDetailModel = {
                            Id: null
                        };

                        getServiceChargeList($scope.productNew.Id);

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                };
            }
            
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };


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
    $scope.delModal = function (id) {
        $scope.id = id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removePopUp')).modal('show');
    };

    $scope.serviceDelete = function () {
        //debugger;
        try {
            $http({
                method: 'POST',
                url: 'Products/ServiceRequisition/ServiceChargesDelete?Id=' + $scope.id
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.id = null;
                    getServiceChargeList($scope.productNew.Id);
                    //getInventoryMaterialList($scope.productNew.Id);
                    //$scope.getDataList();
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
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');
    }

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
            url: 'Products/ServiceRequsitionMaster/UpdateServiceAndTax',
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
        
    };
    // #endregion Service

    $scope.tab1 = 1;
    $scope.setTab = function (newTab1) {
        $scope.tab1 = newTab1;
    };
    $scope.isSet = function (tabNum1) {
        return $scope.tab1 === tabNum1;
    };

    $scope.uom = function () {

        cboService.getUoMCbo(function (response) {
            $scope.uoMList = response;
        });
    }
    $scope.calculateQty = function () {
       // $scope.serviceModel.TotalTaxAmount = 0;
        if (baseService.isUndefinedOrNull($scope.serviceReqDetailModel.Rate)) {
            $scope.serviceReqDetailModel.Rate = 0;
        }
        //item.TaxAmount = Math.round((data.TrnAmount * item.Percentage / 100) * 100 + Number.EPSILON) / 100;
        $scope.serviceReqDetailModel.TotalServiceTranAmount = Math.round(($scope.serviceReqDetailModel.Qty * $scope.serviceReqDetailModel.TransactionRate) * 100 + Number.EPSILON) / 100;
        //for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
        //    $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
        //    $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        //}
        //if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
    };
    $scope.calculateTrnRate = function () {

       // $scope.serviceModel.TotalTaxAmount = 0;
        if (baseService.isUndefinedOrNull($scope.serviceReqDetailModel.Qty)) {
            $scope.serviceReqDetailModel.Qty = 0;
        }
        $scope.serviceReqDetailModel.TotalServiceTranAmount = Math.round(($scope.serviceReqDetailModel.Qty * $scope.serviceReqDetailModel.TransactionRate) * 100 + Number.EPSILON) / 100;
        //for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
        //    $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
        //    $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        //}
        //if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
    };


    $scope.startDate1 = '';
    $scope.endDate1 = '';
    $scope.GetFiscalYear1 = function () {
        $http({
            method: 'GET',
            url: 'Products/ServiceRequisition/GetFiscalYear?formattedDate=' + $filter("dateFiltering")(Date.now()),
        }).then(function successCallback(response) {
            $scope.startDate1 = response.data[0].StartDate;
            $scope.endDate1 = response.data[0].EndDate;
            $scope.ReqListDetails1();
        });

    }
    $scope.GetFiscalYear1();
    $scope.empwiseDataList = [];
    $scope.ReqListDetails1 = function () {
        //debugger
        $http({
            method: 'GET',
            url: "Products/ServiceRequisition/LoadServiceRequisitionMasterTotalEmpWise1?RequisitionId=" + 1 + '&startDate=' + $scope.startDate1 + '&endDate=' + $scope.endDate1,
        }).then(function successCallback(response) {
            //windowemp.lst = response.data;
            $scope.empwiseDataList = response.data;
            $scope.RequisitionId = response.data[0].RequisitionId;
            $scope.EmployeeName = response.data[0].EmployeeName;
            $scope.ReqTotalAmount = response.data[0].ReqTotalAmount;
            $scope.POTotalAmount = response.data[0].POTotalAmount;

        });
    }

	//$scope.detailgrid1 = function detailGridData1(e) {
	//	//debugger

	//	var filteredData = e.data["Id"];
	//	var data = windowemp.lst;//ej.DataManager(window.lst).executeLocal(ej.Query().where("MaterialReqqusitionMasterId", "equal", parseInt(filteredData), true).take(1000));
	//	e.detailsElement.find("#detailGridemp").ejGrid({
	//		dataSource: data,
	//		columns: [{ field: "EmployeeName", headerText: "EmployeeName", width: 50 },
	//		{ field: "ReqTotalAmount", headerText: "ReqTotalAmount", width: 150 },
	//		{ field: "POTotalAmount", headerText: "POTotalAmount", width: 100 }]
	//	});
	//	e.detailsElement.find(".tabcontrol").ejTab();
	//}

    $scope.empwisemonthDataList = [];
    $scope.getRequisitionByEmpInMonth = function () {
        //debugger
        $http({
            method: 'GET',
            url: "Products/ServiceRequisition/ServiceRequisitionByEmpInMonth?RequisitionId=" + 1 + '&startDate=' + $filter("dateFiltering")(Date.now()) + '&endDate=' + $scope.endDate1,
        }).then(function successCallback(response) {
            //windowemp.lst = response.data;
            $scope.empwisemonthDataList = response.data;
            $scope.RequisitionId = response.data[0].RequisitionId;
            $scope.EmployeeName = response.data[0].EmployeeName;
            $scope.ReqTotalAmount = response.data[0].ReqTotalAmount;
            $scope.POTotalAmount = response.data[0].POTotalAmount;

        });
    }
}