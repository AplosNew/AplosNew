'use strict';
SalesOrderUpdateController.$inject = ['accountService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function SalesOrderUpdateController(accountService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Sales Order Update";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.files = [];
    $scope.orderCategoryList = [];
    $scope.orderStatusList = [];
    $scope.searchMasterFilterList = [];
    $scope.itemList = [];
    $scope.personCboList = [];
    $scope.attributeList = [];
    $scope.personList = [];

    $scope.path = 'OrderManagements/SalesOrderUpdate/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListResponsible';
    $scope.partyType = 'Customer';

    $scope.file = {
        Id: null
        , CompanyId: null
        , PlantId: null
        , EntityId: null
        , CommitmentId: null
        , InquiryId: null
        , PartyId: null
        , BuyerId: null
        , BuyerBrandId: null
        , BuyerDivisionId: null
        , BuyerDepartmentId: null
        , TestingStandardId: null
        , MasterOrderNo: null
        , OrderStatusId: null
        , OrderCategoryId: null
        , SeasonId: null
        , OrderYear: null
        , CurrencyId: null
        , OrderType: 'ExternalOrder'
        , TotalQty: null
        , NoOfLineItem: null
        , ResponsiblePersonId: null
        , ResponsiblePersonName: null
        , InvoicingPartyPlantId: null
        , InvoicingByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryPartyPlantId: null
        , DeliveryByAddress: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , OrderWastagePercentage: null
        , ExtraOrderPercentage: null
        , IsExtraOrderPercentage: false
        , TotalQtyUOMId: null
        , IsReplacement: false
        , Type: null
        , SpecialTaxId: null
        , BuyerReferenceNo: null
        , OwnReferenceNo: null
        , PaymentTermId: null
        , OwnReferenceNo: null
        , IsPaymentTermChangeable: null
        , ExceptionalProcessId: null
        , ExceptionalSubProcessId: null
    };
    $scope.fileNew = Object.assign({}, $scope.file);
    $scope.isBuyerApplicable = false;

    $scope.soModel = {
        Id: null
        , MasterOrderItemId: $scope.masterItemId
        , DeliveryDate: null
        , CommitmentDate: null
        , DestinationId: null
        , ShipmentModeId: null
        , CustomerPOId: null
        , PONumber: null
        , UpCharge: null
        , OrderStatusId: $scope.fileNew.OrderStatusId
        , OrderCategoryId: $scope.fileNew.OrderCategoryId
        , SOType: null
        , ResponsiblePersonId: $scope.ResponsiblePersonId
        , ResponsiblePersonName: $scope.ResponsiblePersonName
        , Qty: null
        , Rate: null
        , HSNCodeId: $scope.HSNCodeId
        , TotalTaxAmount: 0
        , MainRawMaterialInhouseDate: null
        , OtherRawMaterialInhouseDate: null
        , CM: 0
        , SalesOrderYear: null
        , WeekNo: null
        , PlanExFactoryDate: null
        , ProductionBookedQty: null
        , ProductionBookingLevel: null
        , QtyChangedBy: null
        , QtyChangedDate: null
        , QtyChangedFromIP: null
        , DestinationDescription: null
        , SalesExpense: null
        , NetSalesRealization: null
    };

    function clearSO() {
        $scope.soModel = {
            Id: null
            , MasterOrderItemId: $scope.masterItemId
            , DeliveryDate: null
            , CommitmentDate: null
            , DestinationId: null
            , ShipmentModeId: null
            , CustomerPOId: null
            , PONumber: null
            , UpCharge: null
            , OrderStatusId: $scope.fileNew.OrderStatusId
            , OrderCategoryId: $scope.fileNew.OrderCategoryId
            , SOType: null
            , ResponsiblePersonId: $scope.ResponsiblePersonId
            , ResponsiblePersonName: $scope.ResponsiblePersonName
            , Qty: null
            , Rate: null
            , HSNCodeId: $scope.HSNCodeId
            , TotalTaxAmount: 0
            , MainRawMaterialInhouseDate: null
            , OtherRawMaterialInhouseDate: null
            , CM: 0
            , SalesOrderYear: null
            , WeekNo: null
            , PlanExFactoryDate: null
            , ProductionBookedQty: null
            , ProductionBookingLevel: null
            , QtyChangedBy: null
            , QtyChangedDate: null
            , QtyChangedFromIP: null
            , DestinationDescription: null
            , SalesExpense: null
            , NetSalesRealization: null
        };
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };
    $scope.tab3 = 1;
    $scope.setTab3 = function (newTab) {
        $scope.tab3 = newTab;
    };
    $scope.isSet3 = function (tabNum) {
        return $scope.tab3 === tabNum;
    };
    $scope.tab4 = 1;
    $scope.setTab4 = function (newTab) {
        $scope.tab4 = newTab;
    };
    $scope.isSet4 = function (tabNum) {
        return $scope.tab4 === tabNum;
    };

    $scope.Save = function () {

        if (baseService.isUndefinedOrNull($scope.fileNew.ResponsiblePersonId)) {
            return ShowResult('Responsible Person is required.', 'failure');
        }
        $scope.customerName = $scope.fileNew.CustomerName;
        $scope.modelNew.CustomerId = $scope.fileNew.PartyId;
        $scope.modelNew.CustomerName = $scope.fileNew.CustomerName;
        $scope.ResponsiblePersonName = $scope.fileNew.ResponsiblePersonName;
        $scope.ResponsiblePersonId = $scope.fileNew.ResponsiblePersonId;
        if ($scope.isBuyerApplicable) {
            if (baseService.isUndefinedOrNull($scope.fileNew.BuyerId)) {
                return ShowResult('Buyer is required.', 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.fileNew.BuyerDivisionId)) {
                return ShowResult('Division is required.', 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.fileNew.BuyerDepartmentId)) {
                return ShowResult('Department is required.', 'failure');
            }
        }

        if (parseFloat(baseService.isUndefinedOrNull($scope.fileNew.TotalQty) ? 0 : $scope.fileNew.TotalQty) === 0) return ShowResult('Please insert total qty.', 'failure');

        if (baseService.isUndefinedOrNull($scope.fileNew.TotalQtyUOMId)) {
            return ShowResult('Total Quantity UoM is required.', 'failure');
        }

        if ($scope.fileNew.IsExtraOrderPercentage && $scope.fileNew.ExtraOrderPercentage === 0)
            return ShowResult('Please insert Extra Order Percentage.', 'failure');
        if (!baseService.isUndefinedOrNull($scope.fileNew.OrderWastagePercentage)) {
            if ($scope.fileNew.OrderWastagePercentage > 99) {
                return ShowResult('Order Wastage Percentage should less than 99 Percent.', 'failure');
            }
        }
        angular.copy($scope.fileNew, $scope.file);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.fileNewForm.$valid) {

            if ($scope.ExchangeSaveExchangeRates($scope.fileNew.CurrencyId) == false) {
                return;
            }

            if ($scope.Action === "Save") {

                //if (baseService.arrayLength($scope.taskList) === 0) {
                //    return ShowResult('Select Task.', 'failure');
                //}

                for (var i = 0; i < $scope.taskList.length; i++) {

                    if ($scope.taskList[i].Active) {
                        $scope.taskList[i].IsRequired = $scope.taskList[i].Active;
                    }
                }

                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: {
                        'entity': $scope.file, 'taskList': $scope.taskList, 'CurrencyData': $scope.ExchangeDisplayCurrency
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.fileNew = response.data.MasterOrder;
                        $scope.ExchangeDisplayExchangeRates(response.data.MasterOrder.Id, response.data.MasterOrder.CurrencyId);//reloading currency exchange rates
                        $scope.getData();
                        $scope.setTab(2);
                        $scope.getMasterItemList();
                        //$scope.getAllEntities();
                        $scope.Action = 'Update';
                        $scope.fileNew.CustomerName = $scope.customerName;
                        $scope.fileNew.ResponsiblePersonName = $scope.ResponsiblePersonName;
                        $scope.fileNew.ResponsiblePersonId = $scope.ResponsiblePersonId;
                        //ClearFields();
                        cboService.getBuyerDivisionCboByBuyer($scope.fileNew.BuyerId, function (result) {
                            $scope.divisionList = result;
                        });
                        cboService.getBuyerDepartmentCboByBuyer($scope.fileNew.BuyerId, function (result) {
                            $scope.departmentList = result;
                        });
                        angular.element(document.querySelector('#TaskListPopUp')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                for (var i = 0; i < baseService.arrayLength($scope.itemList); i++) {
                    if (baseService.isUndefinedOrNull($scope.itemList[i].MaterialMasterId))
                        return ShowResult('Material master need in row number ' + (i + 1), 'failure');
                    if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
                        $scope.itemList[i].ContractId = $scope.modelNew.Id;
                    }
                }
                $http({
                    method: 'POST'
                    , url: $scope.updateUrl
                    , data: {
                        'entity': $scope.file
                        , 'masterId': $scope.fileNew.Id
                        , 'personList': $scope.personList
                        , 'itemList': $scope.itemList
                        , 'CurrencyData': $scope.ExchangeDisplayCurrency
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');


                        //$scope.GetResponsiblePersonList();
                        $scope.getMasterItemList();
                        $scope.getData();
                        //GetDepartmentPersonCbo();
                        //$scope.getAllEntities();
                        $scope.mmChangeFlag = false;
                        $scope.fileNew.ResponsiblePersonName = $scope.ResponsiblePersonName;
                        $scope.fileNew.ResponsiblePersonId = $scope.ResponsiblePersonId;
                        if (!baseService.isUndefinedOrNull($scope.fileNew.SpecialTaxId)) {
                            $scope.SpecialTax = true;
                        }
                        cboService.getBuyerDivisionCboByBuyer($scope.fileNew.BuyerId, function (result) {
                            $scope.divisionList = result;
                        });
                        cboService.getBuyerDepartmentCboByBuyer($scope.fileNew.BuyerId, function (result) {
                            $scope.departmentList = result;
                        });

                        $scope.ExchangeDisplayExchangeRates($scope.fileNew.Id, $scope.fileNew.CurrencyId);//reloading currency exchange rates

                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.removeMaster = function () {
        try {
            $scope.message_confirmation = "Are you sure want to permanent delete";
            angular.element(document.querySelector('#confirmMasterPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.fileNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.fileNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.files.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.deleteItem = function () {
        if (!baseService.isUndefinedOrNull($scope.id)) {
            $http({
                method: 'POST'
                , url: $scope.path + 'deleteItem?id=' + $scope.id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getMasterItemList();
                    $scope.id = null;
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        $scope.personList = [];
        $scope.itemList = [];
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.isBuyerApplicable = false;
        $scope.file = {};
        $scope.fileNew = {
            Id: null
            , EntityId: $scope.fileNew.EntityId
            , PlantId: null
            , OrderType: 'ExternalOrder'
            , PartyId: $scope.fileNew.PartyId
            , CompanyId: $scope.fileNew.CompanyId
        };
        $scope.getPlantConfigByPlant();
        $scope.SpecialTax = false;
        $scope.mmChangeFlag = false;
        $scope.customerName = null;
        $scope.ExchangeReset();
        $scope.enableJobOrOutSource = true;
        $scope.modelNew = Object.assign({}, $scope.model);
    }
    //#region MasterOrderPopUp
    $scope.MasterOrderList = [];

    $scope.MasterOrderPopUp = function () {
        $http.get('OrderManagements/SalesOrderUpdate/GetMasterOrderData')
            .then(function (response) {
                $scope.MasterOrderList = response.data;
            });
    
        angular.element(document.querySelector('#masterOrderPopUp')).modal('show');
    }

    $scope.SelectMO = function (args) {
        var gridObj = $("#Grid").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.MasterOrderNo = $scope.data.MasterOrderNo;
        $scope.MasterOrderItemId = $scope.data.MasterOrderItemId;
        angular.element(document.querySelector('#masterOrderPopUp')).modal('hide');
        $scope.getSOData();
    }

    $scope.CloseCommitment = function () {
        angular.element(document.querySelector('#masterOrderPopUp')).modal('hide');
    }
    $scope.SOList = [];
    $scope.getSOData = function () {
        $http.get('OrderManagements/SalesOrderUpdate/GetSOData?MasterOrderItemId=' + $scope.MasterOrderItemId)
            .then(function (response) {
                $scope.SOList = response.data;
            });
       
    }

    $scope.EditSOPopUp = function (obj) {
        $scope.soModel = Object.assign({}, obj.data);
        angular.element(document.querySelector('#salesOrderEditPopUp')).modal('show');
    }
    $scope.closeSOPopUp = function () {
        angular.element(document.querySelector('#salesOrderEditPopUp')).modal('hide');
    };

    $scope.saveSalesOrderDate = function () {

        $scope.$broadcast('show-errors-check-validity');

        if ($scope.soDateForm.$valid) {
            if (!baseService.isUndefinedOrNull($scope.soModel.Id)) {
                $http({
                    method: 'POST'
                    , url: $scope.path + 'UpdateSODate'
                    , data: {'salesOrderMaster': $scope.soModel}
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'salesOrderEditPopUp');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'salesOrderEditPopUp');
                        getSalesOrderList();
                        clearSO();
                        $scope.getMasterItemList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'salesOrderEditPopUp');
                };
            } 
        }
    };
    $scope.saveSalesOrderRate = function () {
   
        if ($scope.soModel.Rate < $scope.soModel.Discount) {
            ShowResult("Sales order discount can't greater than Rate", 'failure', 'salesOrderEditPopUp');
            return false;
        }
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.soForm.$valid) {
            if (baseService.isUndefinedOrNull($scope.soModel.Id)) {
                $http({
                    method: 'POST'
                    , url: $scope.path + 'UpdateSORate'
                    , data: { 'salesOrderMaster': $scope.soModel }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'salesOrderEditPopUp');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'salesOrderEditPopUp');
                        getSalesOrderList();
                        clearSO();
                        $scope.getMasterItemList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'salesOrderEditPopUp');
                };
            } else {
                getSalesOrderTaxCategoryUpdateList($scope.soModel.Id);
            }
        }
    };
    $scope.saveSalesOrderQTY = function () {
        if ($scope.soModel.Qty <= 0) {
            ShowResult("Sales order quantity can't be zero", 'failure', 'salesOrderEditPopUp');
            return false;
        }
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.soForm.$valid) {
            if (baseService.isUndefinedOrNull($scope.soModel.Id)) {
                $http({
                    method: 'POST'
                    , url: $scope.path + 'UpdateSOQTY'
                    , data: { 'salesOrderMaster': $scope.soModel }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'salesOrderEditPopUp');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'salesOrderEditPopUp');
                        getSalesOrderList();
                        clearSO();
                        $scope.getMasterItemList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'salesOrderEditPopUp');
                };
            } else {
                getSalesOrderTaxCategoryUpdateList($scope.soModel.Id);
            }
        }
    };
    $scope.saveSalesOrderStatus = function () {

        if ($scope.soModel.OrderStatusId !== 'Active') {
            if ($scope.soModel.ProductionBookedQty < 0) {
                ShowResult("Production Booked Qty can't less than 0.", 'failure', 'salesOrderEditPopUp');
                return false;
            }          
        }

        $scope.$broadcast('show-errors-check-validity');

        if ($scope.soForm.$valid) {
            if (baseService.isUndefinedOrNull($scope.soModel.Id)) {
                $http({
                    method: 'POST'
                    , url: $scope.path + 'UpdateSOStatus'
                    , data: { 'salesOrderMaster': $scope.soModel }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'salesOrderEditPopUp');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'salesOrderEditPopUp');
                        getSalesOrderList();
                        clearSO();
                        $scope.getMasterItemList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'salesOrderEditPopUp');
                };
            } else {
                getSalesOrderTaxCategoryUpdateList($scope.soModel.Id);
            }
        }
    };


}


