'use strict';
FinishGoodsBookingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FinishGoodsBookingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Finish Goods Booking';
    $scope.Action = 'Save';

    $scope.modelNew = {
        Id: null,
        ProductionEntityId: null,
        ProcessId: null,
        ProductionOrderId: null
    }

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.modelNew.ProductionEntityId = $scope.entityList[0].Value;
                //default
                $scope.loadProcessList();
            }
        });
    }
    $scope.getAllEntities();

    $scope.loadProcessList = function () {
        $http({
            method: 'GET',
            url: "Productions/FinishGoodsBooking/GetProcessCbo?entityId=" + $scope.modelNew.ProductionEntityId
        }).then(function successCallback(response) {
            $scope.processList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.modelNew.ProcessId = $scope.processList[0].Value;
            }
        });

    };

    $scope.ProductionOrderList = [];
    $scope.ProdOrderList = [];
    $scope.getProductionOrderPopUp = function () {
        $scope.ProductionOrderList = [];
        $http.get("Productions/FinishGoodsBooking/GetProductionOrderDataList?entityId=" + $scope.modelNew.ProductionEntityId + '&processId=' + $scope.modelNew.ProcessId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ProductionOrderList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');
    };

    $scope.SalesOrderListForProductionOrderId = [];
    $scope.getSalesOrderOfProdOrderList = function (prodOrdId) {
        $scope.openPopup('dialogSOItemsForProductionOrder');
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetProductionRecipeMaterialList?productionOrderId=' + prodOrdId
        }).then(function successCallback(response) {
            $scope.SalesOrderListForProductionOrderId = response.data;

        });
    }
    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N0}" }],
        showCaptionSummary: true

    }];
    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {

        }
    }
    $scope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }

    $scope.SelectPOItem = function ($event) {
        $scope.modelNew.ProductionOrderId = $event.data.POId;
        $scope.GetLineItemData();
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
    }

    $scope.SalesOrderLineItems = [];
    $scope.GetLineItemData = function () {
        $http({
            method: 'GET',
            url: 'Productions/FinishGoodsBooking/GetLineItemData?entityId=' + $scope.modelNew.ProductionEntityId + '&processId=' + $scope.modelNew.ProcessId + '&productionOrderId=' + $scope.modelNew.ProductionOrderId + '&masterId=' + $scope.modelNew.Id
        }).then(function successCallback(response) {
            $scope.SalesOrderLineItems = response.data;
            $scope.GetBookedAndBalancedData();
            //$scope.GetSavedBookedAndBalancedData();
        });
    }

    $scope.ProductCodeList = [];
    $scope.BookedAndBalancedDataList = [];
    $scope.GetBookedAndBalancedData = function () {
        var obj = {};
        $http({
            method: 'GET',
            url: 'Productions/FinishGoodsBooking/GetBookedAndBalancedData?productionOrderId=' + $scope.modelNew.ProductionOrderId
        }).then(function successCallback(response) {
            $scope.BookedAndBalancedDataList = response.data;
            if (baseService.arrayLength($scope.BookedAndBalancedDataList) > 0) {
                for (var i = 0; i < $scope.SalesOrderLineItems.length; i++) {
                    for (var j = 0; j < $scope.BookedAndBalancedDataList.length; j++) {

                        obj.ProductLibraryId = $scope.BookedAndBalancedDataList[j].ProductLibraryId;
                        obj.ProductCode = $scope.BookedAndBalancedDataList[j].ProductCode;

                        if (checkExistList($scope.ProductCodeList, obj.ProductLibraryId) === false) {
                            $scope.ProductCodeList.push(obj);
                            obj = {};
                        }
                       
                        //if ($scope.SalesOrderLineItems[i].ProductLibraryId == $scope.BookedAndBalancedDataList[j].ProductLibraryId) {

                        //    $scope.SalesOrderLineItems[i].Qty = $scope.BookedAndBalancedDataList[j].FGQty;
                        //}
                    }
                }
                var gridObj = $("#GridLineItems").data("ejGrid");
                gridObj.refreshContent();
                gridObj.refreshTemplate();
            }
        });
    };

    $scope.SetProductCodeQty = function () {

        var gridObj = $("#GridLineItems").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];

        for (var j = 0; j < $scope.BookedAndBalancedDataList.length; j++) {

            if (data.ProductLibraryId == $scope.BookedAndBalancedDataList[j].ProductLibraryId) {
                data.Qty = $scope.BookedAndBalancedDataList[j].FGQty;
            }
            else {
                data.Qty = "";
            }
        }
    }

    function checkExistList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProductLibraryId == Id) {
                return true;
            }
        }
        return false;
    }

    $scope.Action = "Save";
    $scope.Save = function () {
        try {
            $scope.$broadcast("show-errors-check-validity");
            if ($scope.modelForm.$valid) {
                if ($scope.Action === "Save" || $scope.Action === "Update") {
                    $http({
                        method: "POST",
                        url: "Productions/FinishGoodsBooking/Insert",
                        data: {
                            "data": $scope.modelNew
                            , "FinishGoodsBookingDetailList": $scope.SalesOrderLineItems
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.modelNew.Id = response.data.Id;
                            $scope.getSavedData();
                            $scope.getSavedDetailData();
                            $scope.Action = "Update";
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
            }
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        $scope.modelNew = {
            Id: null,ProductionEntityId: null,ProcessId: null,ProductionOrderId: null
        }
        $scope.ProductCodeList = [];
        $scope.SalesOrderLineItems = [];
        $scope.BookedAndBalancedDataList = [];
    }


    $scope.masterDataList = [];
    $scope.getSavedData = function () {
        $scope.masterDataList = [];
        $http.get("Productions/FinishGoodsBooking/GetList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.masterDataList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getSavedData();

    $scope.getSavedDetailData = function () {
        $http.get("Productions/FinishGoodsBooking/GetDetailList?masterId=" + $scope.modelNew.Id + '&entityId=' + $scope.modelNew.ProductionEntityId + '&processId=' + $scope.modelNew.ProcessId + '&productionOrderId=' + $scope.modelNew.ProductionOrderId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SalesOrderLineItems = response.data;
                        $scope.GetBookedAndBalancedData();
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.GetSavedBookedAndBalancedData = function () {
        var obj = {};
        $http({
            method: 'GET',
            url: 'Productions/FinishGoodsBooking/GetSavedBookedAndBalancedData?productionOrderId=' + $scope.modelNew.ProductionOrderId
        }).then(function successCallback(response) {
            $scope.BookedAndBalancedDataList = response.data;
            if (baseService.arrayLength($scope.BookedAndBalancedDataList) > 0) {
                for (var i = 0; i < $scope.SalesOrderLineItems.length; i++) {
                    for (var j = 0; j < $scope.BookedAndBalancedDataList.length; j++) {

                        obj.ProductLibraryId = $scope.BookedAndBalancedDataList[j].ProductLibraryId;
                        obj.ProductCode = $scope.BookedAndBalancedDataList[j].ProductCode;

                        if (checkExistList($scope.ProductCodeList, obj.ProductLibraryId) === false) {
                            $scope.ProductCodeList.push(obj);
                            obj = {};
                        }
                    }
                }
                var gridObj = $("#GridLineItems").data("ejGrid");
                gridObj.refreshContent();
                gridObj.refreshTemplate();
            }
        });
    };

    $scope.Get = function (obj) {
        $scope.modelNew = Object.assign({}, obj.data);
        $scope.loadProcessList();
        $scope.getSavedDetailData();
        
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };




}


