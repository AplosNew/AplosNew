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
        , MOIQty: 0
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
        , LSD: null
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
        , Currency: null      

    };
    $http.get("OrderManagements/ordercategory/getcbo/")
        .then(function (response) {
            $scope.orderCategoryList = response.data;
        });

    $http.get("OrderManagements/orderstatus/getcbo/")
        .then(function (response) {
            $scope.orderStatusList = response.data;
        });
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
        $scope.soModel.Currency=  $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
    });
    //function clearSO() {
    //    $scope.soModel();
    //    };
    //}

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

    $scope.removeMaster = function () {
        try {
            $scope.message_confirmation = "Are you sure want to permanent delete";
            angular.element(document.querySelector('#confirmMasterPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
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
        $scope.MasterOrderId = $scope.data.Id;
        angular.element(document.querySelector('#masterOrderPopUp')).modal('hide');
        $scope.getSOData();
    }

    $scope.CloseCommitment = function () {
        angular.element(document.querySelector('#masterOrderPopUp')).modal('hide');
    }
    $scope.SOList = [];
    $scope.getSOData = function () {
        $http.get('OrderManagements/SalesOrderUpdate/GetSOData?MasterOrderId=' + $scope.MasterOrderId)
            .then(function (response) {
                $scope.SOList = response.data;
            });

    }
    function ValidateSOQtyWithItem() {
        try {

            var AllSOList = ej.DataManager($scope.SOList).executeLocal(ej.Query().where("MasterOrderItemId", "equal", $scope.soModel.MasterOrderItemId));
            var SOQty = 0;
            for (var i = 0; i < AllSOList.length; i++) {
                if (AllSOList[i]["Id"] == $scope.soModel.Id) {
                    SOQty += $scope.soModel.Qty;
                }
                else {
                    SOQty += AllSOList[i].Qty;
                }
            }

            if (SOQty > $scope.soModel.MOIQty)
                throw 'Total SO Qty cannot be greater than master order item qty'

        } catch (e) {
            throw e;
        }
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
        try {
            if ($scope.soDateForm.$valid) {
                if (!baseService.isUndefinedOrNull($scope.soModel.Id)) {
                    if (new Date($scope.soModel.LSD) < new Date($scope.soModel.MainRawMaterialInhouseDate))
                        throw " Main raw material in house date can not be greater than LSD date.";
                    if (new Date($scope.soModel.LSD) < new Date($scope.soModel.OtherRawMaterialInhouseDate))
                        throw " Other raw material in house date can not be greater than LSD date.";
                    if (new Date($scope.soModel.PlanExFactoryDate) < new Date($scope.soModel.LSD))
                        throw " LSD date can not be greater than plan ex factory date.";
                    if (new Date($scope.soModel.DeliveryDate) < new Date($scope.soModel.PlanExFactoryDate))
                        throw "Plan ex factory date can not be greater than delivery date.";
                    if (new Date($scope.soModel.DeliveryDate) < new Date($scope.soModel.CommitmentDate))
                        throw "Commitment date can not be greater than delivery date.";
                    $http({
                        method: 'POST'
                        , url: $scope.path + 'UpdateSODate'
                        , data: { 'salesOrderMaster': $scope.soModel }
                        , dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'salesOrderEditPopUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'salesOrderEditPopUp');
                            //getSalesOrderList();
                            //clearSO();
                            $scope.getSOData();
                            //$scope.getMasterItemList();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'salesOrderEditPopUp');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.saveSalesOrderRate = function () {

        if ($scope.soModel.Rate < $scope.soModel.Discount) {
            ShowResult("Sales order discount can't greater than Rate", 'failure', 'salesOrderEditPopUp');
            return false;
        }
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.soRateForm.$valid) {
            if (!baseService.isUndefinedOrNull($scope.soModel.Id)) {
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
                        //getSalesOrderList();
                        //clearSO();
                        $scope.getSOData();
                        //$scope.getMasterItemList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'salesOrderEditPopUp');
                };
            }
        }
    };
    $scope.saveSalesOrderQTY = function () {
        if ($scope.soModel.Qty <= 0) {
            ShowResult("Sales order quantity can't be zero", 'failure', 'salesOrderEditPopUp');
            return false;
        }
        $scope.$broadcast('show-errors-check-validity');
        try {
            ValidateSOQtyWithItem();
            if ($scope.soQTYForm.$valid) {
                if (!baseService.isUndefinedOrNull($scope.soModel.Id)) {
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
                            // getSalesOrderList();
                            //clearSO();
                            $scope.getSOData();
                            //$scope.getMasterItemList();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'salesOrderEditPopUp');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.SetTotalProdQty = function () {
        $scope.TotalProducedQty = $scope.soModel.ProductionBookedQty + $scope.ProdBookedQty;
    }
    $scope.saveSalesOrderStatus = function () {

        if ($scope.soModel.OrderStatusId !== 'Active') {
            if ($scope.soModel.ProductionBookedQty < 0) {
                ShowResult("Production Booked Qty can't less than 0.", 'failure', 'salesOrderEditPopUp');
                return false;
            }
        }

        $scope.$broadcast('show-errors-check-validity');

        if ($scope.soStatusForm.$valid) {
            if (!baseService.isUndefinedOrNull($scope.soModel.Id)) {
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
                        //getSalesOrderList();
                        // clearSO();
                        $scope.getSOData();
                        //$scope.getMasterItemList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'salesOrderEditPopUp');
                };
            }
        }
    };
    $scope.SetNetSalesRealization = function () {
        $scope.soModel.NetSalesRealization = $scope.soModel.SalesExpense - $scope.soModel.Discount;
    }

    $scope.ProdBookedQty = 0;
    $scope.TotalProducedQty = 0;
    $scope.GetSOBookedQtyAndLevel = function (salesOrderId) {
        //$scope.TotalProducedQty = 0;
        //$scope.ProdBookedQty = 0;
        if (!baseService.isUndefinedOrNull($scope.soModel.Id) && $scope.soModel.OrderStatusId !== 'Active') {
            $http({
                method: 'GET',
                url: 'OrderManagements/MasterOrder/GetSOBookedQtyAndLevel?salesOrderId=' + salesOrderId
            }).then(function successCallback(response) {
                if (baseService.arrayLength(response.data) > 0) {
                    if ($scope.soModel.ProductionBookedQty === 0) {
                        $scope.soModel.ProductionBookedQty = response.data[0].Quantity;
                        $scope.soModel.ProductionBookingLevel = response.data[0].BookingLevel;
                        $scope.ProdBookedQty = response.data[0].Quantity;
                        $scope.TotalProducedQty = $scope.ProdBookedQty;
                    }
                }
                if ($scope.soModel.ProductionBookedQty == 0.00) {
                    $http({
                        method: 'GET',
                        url: 'OrderManagements/MasterOrder/GetPOBookedQtyAndLevel?salesOrderId=' + salesOrderId
                    }).then(function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            if ($scope.soModel.ProductionBookedQty === 0) {
                                $scope.soModel.ProductionBookedQty = 0;
                                $scope.soModel.ProductionBookingLevel = response.data[0].BookingLevel;
                                $scope.ProdBookedQty = response.data[0].Quantity;
                                $scope.TotalProducedQty = $scope.ProdBookedQty;
                            }
                        }
                    });
                }
            });
        } else {
            $scope.soModel.ProductionBookedQty = 0;
        }
    };
}