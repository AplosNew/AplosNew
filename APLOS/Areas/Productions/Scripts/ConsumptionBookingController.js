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
        ToDate: null,
        MaterialStorageId: null,
        ToCurrencyRate: null,
        CurrencyId: null,
        SourceType: 'Packing',
        CompanyCurrencyId: null,
        Level: "QBOQ"
    }

    $scope.LevelList = [];
    $scope.LevelList = [
        {
            'Value': 'Costing',
            'Text': 'Costing'
        },
        {
            'Value': 'QBOQ',
            'Text': 'QBOQ'
        }
    ];


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

    $scope.storageList = [];
    $http({
        method: 'GET',
        url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = result;
    });

    $scope.companyCurrencyId = null;
    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CurrencyParallel'
    }).then(function successCallback(response) {
        angular.forEach(response.data, function (item, i) {
            if (item.ParallelCurrencyType === 'CompanyCurrency') {
                $scope.companyCurrencyId = item.CurrencyId;
                $scope.modelNew.CompanyCurrencyId = item.CurrencyId;
            }
        });
    });

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $filter("dateFiltering")(Date.now()) + "&currencyId=" + $scope.modelNew.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.modelNew.ToCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
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

    $scope.calculateAmount = function (data) {
        data.Amount = parseFloat(data.Qty * data.Rate).toFixed(2);
        var gridObj = $("#GridLineItems").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    }

   

    $scope.LineItemsList = [];
    $scope.WorkDayList = [];
    $scope.LoadData = function () {
        try {
            if (new Date($scope.modelNew.FromDate) > new Date($scope.modelNew.ToDate)) {
                throw "From date must be below or equal to To Date";
            }
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.modelForm.$valid) {
                $scope.LineItemsList = [];
                $scope.WorkDayList = [];
                var ob = {};
                $http.get("Productions/FinishGoodsBooking/GetItemScanChildData?entityId=" + $scope.modelNew.ProductionEntityId + '&fromDate=' + $scope.modelNew.FromDate + '&toDate=' + $scope.modelNew.ToDate)
                    .then(
                        function successCallback(response) {
                            if (baseService.arrayLength(response.data) > 0) {
                                $scope.LineItemsList = response.data;
                                //GetDateWiseData();
                            }
                        },
                        function errorCallback(response) {
                            ShowResult(response, 'failure');
                        });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.tempList = [];
    function GetIdsandCode() {
        for (var i = 0; i < $scope.LineItemsList.length; i++) {
            if ($scope.LineItemsList[i].Flag == true) {
                $scope.tempList.push($scope.LineItemsList[i]);
            }
        }
        var uniqueProductCode = removeDuplicates($scope.tempList, 'ProductCode');
        var uniqueProductionOrderId = removeDuplicates($scope.tempList, 'ProductionOrderId');
        var wcProductCode = "";
        if (uniqueProductCode.length > 0) {
            wcProductCode = "IN(";
            wcProductCode += Array.prototype.map.call(uniqueProductCode, function (item) { return "'" + item.ProductCode + "'"; }).join(",") + ")";
        }
        $scope.sqlInProductCode = wcProductCode;

        var wcProductionOrderId = "";
        if (uniqueProductionOrderId.length > 0) {
            wcProductionOrderId = "IN(";
            wcProductionOrderId += Array.prototype.map.call(uniqueProductionOrderId, function (item) { return "'" + item.ProductionOrderId + "'"; }).join(",") + ")";
        }
        $scope.sqlInProductionOrderId = wcProductionOrderId;
    }

    function GetDateWiseData() {
        GetIdsandCode();

        var ob = {};
        $http.get("Productions/FinishGoodsBooking/GetDateWiseDetailDataData?entityId=" + $scope.modelNew.ProductionEntityId + '&fromDate=' + $scope.modelNew.FromDate + '&toDate=' + $scope.modelNew.ToDate + '&POId=' + $scope.sqlInProductionOrderId + '&ProductCode=' + $scope.sqlInProductCode)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.DatewiseList = response.data;

                        for (var i = 0; i < $scope.DatewiseList.length; i++) {
                            ob.WorkDate = $scope.DatewiseList[i].WorkDate;
                            ob.ProductionOrderId = $scope.DatewiseList[i].ProductionOrderId;
                            if (checkExistList($scope.WorkDayList, ob.WorkDate) === false) {
                                $scope.WorkDayList.push(ob);
                                ob = {};
                            }
                        }
                        $scope.SetRate();
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    }

    function checkExistList(list, WorkDate) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].WorkDate === WorkDate) {
                return true;
            }
        }
        return false;
    }

    //$scope.SetRate = function () {
    //    for (var i = 0; i < $scope.tempList.length; i++) {
    //        for (var j = 0; j < $scope.DatewiseList.length; j++) {
    //            if ($scope.tempList[i].ProductionOrderId == $scope.DatewiseList[j].ProductionOrderId
    //                && $scope.tempList[i].ProductCode == $scope.DatewiseList[j].ProductCode
    //                && $scope.tempList[i].MaterialMasterId == $scope.DatewiseList[j].MaterialMasterId
    //                && $scope.tempList[i].ArticleId == $scope.DatewiseList[j].ArticleId
    //                && $scope.tempList[i].UOM == $scope.DatewiseList[j].UOM && $scope.tempList[i].Flag == true) {
    //                $scope.DatewiseList[j].Id = null;
    //                $scope.DatewiseList[j].Rate = $scope.tempList[i].Rate;
    //                $scope.DatewiseList[j].Amount = parseFloat($scope.DatewiseList[j].Rate) * $scope.DatewiseList[j].Qty;
    //            }
    //        }
    //    }
    //    SetToSave();
    //};

    function SetToSave() {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.modelForm.$valid) {
            if ($scope.Action === "Save" || $scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: "Productions/FinishGoodsBooking/Create",
                    data: {
                        "data": $scope.modelNew
                        , "WorkDayList": $scope.WorkDayList
                        , "FinishGoodsBookingDetailList": $scope.DatewiseList
                        , 'FGList': $scope.LineItemsList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getSavedData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }

    }

    
    //$scope.Save = function () {
    //    try {
    //        GetDateWiseData();
    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //};

    // $scope.SetRate = function () {
    //    for (var i = 0; i < $scope.LineItemsList.length; i++) {
    //        for (var j = 0; j < $scope.DatewiseList.length; j++) {
    //            if ($scope.LineItemsList[i].ProductionOrderId == $scope.DatewiseList[j].ProductionOrderId
    //                && $scope.LineItemsList[i].ProductCode == $scope.DatewiseList[j].ProductCode
    //                && $scope.LineItemsList[i].MaterialMasterId == $scope.DatewiseList[j].MaterialMasterId
    //                && $scope.LineItemsList[i].ArticleId == $scope.DatewiseList[j].ArticleId
    //                && $scope.LineItemsList[i].UOM == $scope.DatewiseList[j].UOM && $scope.LineItemsList[i].Flag==true) {
    //                $scope.DatewiseList[j].Rate = $scope.LineItemsList[i].Rate;
    //                $scope.DatewiseList[j].Amount = parseFloat($scope.DatewiseList[j].Rate) * $scope.DatewiseList[j].Qty;
    //            }
    //        }
    //    }
    //};

    $scope.selectedLineItems = [];
    $scope.Save = function () {
        try {
            $scope.selectedLineItems = [];
            //GetDateWiseData();
            for (var i = 0; i < $scope.LineItemsList.length; i++) {
                if ($scope.LineItemsList[i].Flag) {
                    $scope.selectedLineItems.push($scope.LineItemsList[i]);
                }
            }
            $scope.$broadcast("show-errors-check-validity");
            if ($scope.modelForm.$valid) {
                if ($scope.Action === "Save" || $scope.Action === "Update") {
                    $http({
                        method: "POST",
                        url: "Productions/FinishGoodsBooking/Create",
                        data: {
                            "data": $scope.modelNew
                            , 'FGList': $scope.selectedLineItems
                            , 'ToDate': $scope.modelNew.ToDate
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.getSavedData();
                            $scope.Clear();
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
            Id: null,
            ProductionEntityId: null,
            ProcessId: null,
            ProductionOrderId: null,
            FromDate: null,
            ToDate: null,
            MaterialStorageId: null,
            ToCurrencyRate: null,
            CurrencyId: null,
            SourceType: 'Packing',
            CompanyCurrencyId: $scope.companyCurrencyId,
            Level: "QBOQ"
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
        $http.get("Productions/FinishGoodsBooking/GetListByPacking")
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
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "GrossAmount", dataMember: "GrossAmount", format: "{0:0.0000}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Rate", dataMember: "Rate", format: "{0:0.0000}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "GrossConsumption", dataMember: "GrossConsumption", format: "{0:0.0000}" }
        ],
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
                $scope.LineItemsList[i].Flag = ChkOrUnchk;
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
        $http.get("Productions/FinishGoodsBooking/GetCostingItemDetailData?costingId=" + obj.data.OrderCostingMasterTemplateId)
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



    $scope.FGInventoryRegisterPoPUpList = [];
    $scope.GetRawMaterialDetail = function (obj) {

        $scope.getFGInventoryRegisterPopUpData(obj.data.Id)

        //$scope.FinishGoodsBookingId = obj.data.Id;
        //$scope.getFGInventoryRegisterPopUpData = function () {

        //    $http({
        //        method: "GET",
        //        url: "Productions/FinishGoodsBooking/GetFGInventoryRegisterPoPUpListData?finishGoodsBookingId=" + FinishGoodsBookingId
        //    }).then(function successCallback(response) {
        //        $scope.FGInventoryRegisterPoPUpList = response.data;
        //        //$scope.TotalDRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.BankLedgerDetailLevelPoPUpList), "CompanyCurrencyDrAmount") * 100 + Number.EPSILON) / 100;
        //        //$scope.TotalCRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.BankLedgerDetailLevelPoPUpList), "CompanyCurrencyCrAmount") * 100 + Number.EPSILON) / 100;
        //        //$scope.BankLedgerClosingBalance = Math.round(($scope.TotalDRAmount - $scope.TotalCRAmount) * 100 + Number.EPSILON) / 100;
        //        //$scope.DRBalanceType = 'DR'
        //    });
        //    // $rootScope.openPopupAngular('TrialBLBankMasterLedgerPopUp');
        //};

        angular.element(document.querySelector('#FGInventoryRegisterPopup')).modal('show');
    };

    $scope.getFGInventoryRegisterPopUpData = function (id) {

        $http({
            method: "GET",
            url: "Productions/FinishGoodsBooking/GetFGInventoryRegisterPoPUpListData?finishGoodsBookingId=" + id
        }).then(function successCallback(response) {
            $scope.FGInventoryRegisterPoPUpList = response.data;
            //$scope.TotalDRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.BankLedgerDetailLevelPoPUpList), "CompanyCurrencyDrAmount") * 100 + Number.EPSILON) / 100;
            //$scope.TotalCRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.BankLedgerDetailLevelPoPUpList), "CompanyCurrencyCrAmount") * 100 + Number.EPSILON) / 100;
            //$scope.BankLedgerClosingBalance = Math.round(($scope.TotalDRAmount - $scope.TotalCRAmount) * 100 + Number.EPSILON) / 100;
            //$scope.DRBalanceType = 'DR'
        });
        // $rootScope.openPopupAngular('TrialBLBankMasterLedgerPopUp');
    };

    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "GoodsReceiveNote/FGGRNReport?grnId=" + data.Id;
    };

    //$scope.CloseFGInventoryRegister = function () {
    //    angular.element(document.querySelector('#FGInventoryRegisterPopup')).modal('hide');
    //}

    $scope.ItemDetailList = [];
    $scope.GetItemDetailListData = function (obj) {
        $scope.ItemDetailList = [];
        $http.get("Productions/FinishGoodsBooking/GetItemDetailListData?productionOrderId=" + obj.ProductionOrderId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ItemDetailList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#ItemDetailPopup')).modal('show');
    };

    $scope.CurrentQtyBreakDownList  = [];
    $scope.GetCurrentQtyBreakDownData = function (obj) {
        $scope.CurrentQtyBreakDownList = [];
        $http.get("Productions/FinishGoodsBooking/GetCurrentQtyBreakDownData?productionOrderId=" + obj.ProductionOrderId + '&productCode=' + obj.ProductCode + '&entityId=' + $scope.modelNew.ProductionEntityId + '&fromDate=' + $scope.modelNew.FromDate + '&toDate=' + $scope.modelNew.ToDate)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.CurrentQtyBreakDownList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#CurrentQtyBreakDownPopup')).modal('show');
    };

    $scope.UnBookedQtyBreakDownList = [];
    $scope.GetUnBookedQtyBreakDownData = function (obj) {
        $scope.UnBookedQtyBreakDownList = [];
        $http.get("Productions/FinishGoodsBooking/GetUnBookedQtyBreakDownData?productionOrderId=" + obj.ProductionOrderId + '&productCode=' + obj.ProductCode + '&entityId=' + $scope.modelNew.ProductionEntityId + '&fromDate=' + $scope.modelNew.FromDate + '&toDate=' + $scope.modelNew.ToDate)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.UnBookedQtyBreakDownList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#UnBookedQtyBreakDownPopup')).modal('show');
    };

    $scope.BookedQtyBreakDownList = [];
    $scope.GetBookedQtyBreakDownData = function (obj) {
        $scope.BookedQtyBreakDownList = [];
        $http.get("Productions/FinishGoodsBooking/GetBookedQtyBreakDownData?productionOrderId=" + obj.ProductionOrderId + '&productCode=' + obj.ProductCode + '&entityId=' + $scope.modelNew.ProductionEntityId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.BookedQtyBreakDownList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#BookedQtyBreakDownPopup')).modal('show');
    };
}


