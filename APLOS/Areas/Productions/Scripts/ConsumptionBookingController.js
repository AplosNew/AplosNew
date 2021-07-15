'use strict';
ConsumptionBookingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ConsumptionBookingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Consumption Booking';
    $scope.Action = 'Save';

    $scope.modelNew = {
        Id: null,
        ProductionEntityId: null,
        ProcessId: null,
        ProductionOrderId: null,
        FromDate: null,
        ToDate: null
    }

    $scope.GetFromDate = function () {
        $http({
            method: 'Get',
            url: 'Productions/FinishGoodsBooking/GetFromDate'
        }).then(function (response) {
            $scope.modelNew.FromDate = response.data[0].FromDate;
            $scope.modelNew.ToDate = response.data[0].ToDate;
        });
    };
    $scope.GetFromDate();

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

    $scope.Action = "Save";
    $scope.TotalQty = 0;
    $scope.TotalRcvQty = 0;

    $scope.SetReceiveQty = function (obj) {
        for (var i = 0; i < $scope.LineItemsList.length; i++) {
            if ($scope.LineItemsList[i].Active) {
                if ($scope.LineItemsList[i].ProductionOrderId == obj.data.ProductionOrderId && $scope.LineItemsList[i].ProductCode == obj.data.ProductCode) {
                    $scope.TotalRcvQty = $filter("sumByKey")($filter("filter")($scope.LineItemsList, { "ProductCode": obj.data.ProductCode, "ProductionOrderId": obj.data.ProductionOrderId }), "Qty");
                    $scope.TotalQty = $scope.LineItemsList[i].FGQty;

                    if ($scope.TotalQty < parseFloat($scope.TotalRcvQty)/* + parseFloat(obj.data.Qty)*/) {
                        ShowResult("Total Receive Qty cann't greater than Total Qty.", "failure");
                    }
                }
            }
        }
    }

    $scope.selectedLineItems = [];
    $scope.Save = function () {
        try {
            $scope.selectedLineItems = [];
            for (var i = 0; i < $scope.LineItemsList.length; i++) {
                if ($scope.LineItemsList[i].Active) {
                    if (baseService.isUndefinedOrNull($scope.LineItemsList[i].Id)) {
                        $scope.LineItemsList[i].Id = null;
                        $scope.selectedLineItems.push($scope.LineItemsList[i]);
                    }
                    else {
                        $scope.selectedLineItems.push($scope.LineItemsList[i]);
                    }
                }
                
            }

            $scope.$broadcast("show-errors-check-validity");
            if ($scope.modelForm.$valid) {
                if ($scope.Action === "Save" || $scope.Action === "Update") {
                    $http({
                        method: "POST",
                        url: "Productions/FinishGoodsBooking/Insert",
                        data: {
                            "data": $scope.modelNew
                            , "FinishGoodsBookingDetailList": $scope.selectedLineItems
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.modelNew = response.data.Data;
                            $scope.GetItemDetailData();
                            $scope.getSavedData();
                            $scope.LoadData();
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
            Id: null, ProductionEntityId: null, ProcessId: null, ProductionOrderId: null, FromDate:null, ToDate:null
        }
        $scope.ProductCodeList = [];
        $scope.SalesOrderLineItems = [];
        $scope.BookedAndBalancedDataList = [];
        $scope.LineItemsList = [];
        $scope.GetFromDate();
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
    $scope.summaryOfRows = [{
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
        $scope.LoadData();
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
    }


    $scope.LineItemsList = [];
    $scope.LoadData = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.modelForm.$valid) {
            $http.get("Productions/FinishGoodsBooking/GetItemScanChildData?fromDate=" + $scope.modelNew.FromDate + '&toDate=' + $scope.modelNew.ToDate)
            //$http.get("Productions/FinishGoodsBooking/GetItemScanChildData?productionOrderId=" + $scope.modelNew.ProductionOrderId)
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.LineItemsList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        }
    };

    $scope.PCode = "";
    $scope.isAlternative = -1;
    $scope.rowDataBound = function rowDataBound(e) {

        if ($scope.PCode != e.data.ProductLibraryId + e.data.ProductCode) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.PCode = e.data.ProductLibraryId + e.data.ProductCode;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", '#fff6b7');
        else
            e.row.css("background-color", '#d1e5ff');
    }

    $scope.costingItemDataList = [];
    $scope.GetCostingItemData = function (obj) {
        $http.get("Productions/FinishGoodsBooking/GetCostingItemData?productionOrderId=" + obj.data.ProductionOrderId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.costingItemDataList = response.data;
                        angular.element(document.querySelector('#CostingItemPoUp')).modal('show');
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.CloseCostingItemDetailDataPoUp = function () {
        angular.element(document.querySelector('#CostingItemDetailDataPoUp')).modal('hide');
    }

    $scope.SummaryRows = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Rate", dataMember: "Rate", format: "{0:0.0000}" }],
        showCaptionSummary: true

    }];

    // #region checkbox all

    $scope.refreshPackTemplate = function (args) {
        $("#Pkheadchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridLineItems").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.LineItemsList.length; i++) {
                $scope.LineItemsList[i].Active = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridLineItems").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    $scope.Get = function (obj) {
        $scope.modelNew = Object.assign({}, obj.data);
        $scope.modelNew.FromDate = $scope.modelNew.FDate;
        $scope.modelNew.ToDate = $scope.modelNew.TDate;
        $scope.loadProcessList();
        $scope.GetItemDetailData();

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.ItemDetailDataList = [];
    $scope.GetItemDetailData = function () {
        $http.get("Productions/FinishGoodsBooking/GetItemDetailData?masterId=" + $scope.modelNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.LineItemsList = response.data;

                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.costingItemDetailDataList = [];
    $scope.GetCostingItemDetailData = function (obj) {
        $scope.costingItemDetailDataList = [];
        $http.get("Productions/FinishGoodsBooking/GetCostingItemDetailData?costingId=" + obj.data.CostingMasterTemplateId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.costingItemDetailDataList = response.data;

                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#CostingItemDetailDataPoUp')).modal('show');
    };
}


