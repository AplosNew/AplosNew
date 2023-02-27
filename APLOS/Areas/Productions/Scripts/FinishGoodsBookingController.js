'use strict';
FinishGoodsBookingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FinishGoodsBookingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Finish Goods Booking';
    $scope.Action = 'Save';
    $scope.LineItemsList = [];
    $scope.LineItemsList = [];

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
        SourceType: 'ProductionBooking',
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

    $scope.GetProductionBookFromToDate = function () {
        $http({
            method: 'Get',
            url: 'Productions/FinishGoodsBooking/GetProductionBookFromToDate'
        }).then(function (response) {
            $scope.modelNew.FromDate = response.data[0].FromDate;
            $scope.modelNew.ToDate = response.data[0].ToDate;
        });
    };
    $scope.GetProductionBookFromToDate();

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.modelNew.ProductionEntityId = $scope.entityList[0].Value;
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

    $scope.masterDataList = [];
    $scope.getSavedData = function () {
        $scope.masterDataList = [];
        $http.get("Productions/FinishGoodsBooking/GetListByProductionBooking")
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

    $scope.calculateAmount = function (data) {
        data.Amount = parseFloat(data.Qty * data.Rate).toFixed(2);
        var gridObj = $("#GridLineItems").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    }

    $scope.LineItemsList = [];
    $scope.WorkDayList = [];
    $scope.DatewiseList = [];
    $scope.LoadData = function () {
        try {
            if (new Date($scope.modelNew.FromDate) > new Date($scope.modelNew.ToDate)) {
                throw "From date must be below or equal to To Date";
            }
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.modelForm.$valid) {
                $scope.LineItemsList = [];
                $scope.WorkDayList = [];
                $scope.DatewiseList = [];
                
                $http.get("Productions/FinishGoodsBooking/GetNonPostedProductionSummeryData?entityId=" + $scope.modelNew.ProductionEntityId + '&processId=' + $scope.modelNew.ProcessId + '&fromDate=' + $scope.modelNew.FromDate + '&toDate=' + $scope.modelNew.ToDate)
                    .then(
                        function successCallback(response) {
                            if (baseService.arrayLength(response.data) > 0) {
                                $scope.LineItemsList = response.data;
                                GetDateWiseData();
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

    function GetDateWiseData() {
        var ob = {};
        $http.get("Productions/FinishGoodsBooking/GetDatewiseNonPostedProductionSummeryData?entityId=" + $scope.modelNew.ProductionEntityId + '&processId=' + $scope.modelNew.ProcessId + '&fromDate=' + $scope.modelNew.FromDate + '&toDate=' + $scope.modelNew.ToDate)
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

    $scope.SetRate = function () {
        for (var i = 0; i < $scope.LineItemsList.length; i++) {
            for (var j = 0; j < $scope.DatewiseList.length; j++) {
                if ($scope.LineItemsList[i].ProductionOrderId == $scope.DatewiseList[j].ProductionOrderId
                    && $scope.LineItemsList[i].MaterialMasterId == $scope.DatewiseList[j].MaterialMasterId
                    && $scope.LineItemsList[i].ArticleId == $scope.DatewiseList[j].ArticleId
                    && $scope.LineItemsList[i].UOM == $scope.DatewiseList[j].UOM) {
                    $scope.DatewiseList[j].Rate = $scope.LineItemsList[i].Rate;
                    $scope.DatewiseList[j].Amount = $scope.LineItemsList[i].Amount;
                }
            }
        }
    };

    $scope.Save = function () {
        try {
            $scope.SetRate();

            $scope.$broadcast("show-errors-check-validity");
            if ($scope.modelForm.$valid) {
                if ($scope.Action === "Save" || $scope.Action === "Update") {
                    $http({
                        method: "POST",
                        url: "Productions/FinishGoodsBooking/CreateConsumtionBook",
                        data: {
                            "data": $scope.modelNew
                            , "WorkDayList": $scope.WorkDayList
                            , "FinishGoodsBookingDetailList": $scope.DatewiseList
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
                            $scope.GetProductionBookFromToDate();
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
            SourceType: 'ProductionBooking',
            CompanyCurrencyId: $scope.companyCurrencyId,
            Level: "QBOQ"
        }
        $scope.ProductCodeList = [];
        $scope.SalesOrderLineItems = [];
        $scope.BookedAndBalancedDataList = [];
        $scope.LineItemsList = [];
        $scope.GetProductionBookFromToDate();
    }

    $scope.FGInventoryRegisterPoPUpList = [];
    $scope.GetRawMaterialDetail = function (obj) {
        $scope.getFGInventoryRegisterPopUpData(obj.data.Id)
        angular.element(document.querySelector('#FGInventoryRegisterPopup')).modal('show');
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

    $scope.SummaryRows = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "GrossAmount", dataMember: "GrossAmount", format: "{0:0.0000}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Rate", dataMember: "Rate", format: "{0:0.0000}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "GrossConsumption", dataMember: "GrossConsumption", format: "{0:0.0000}" }
        ],
        showCaptionSummary: true
    }];

    $scope.getFGInventoryRegisterPopUpData = function (id) {
        $http({
            method: "GET",
            url: "Productions/FinishGoodsBooking/GetFGInventoryRegisterPoPUpListData?finishGoodsBookingId=" + id
        }).then(function successCallback(response) {
            $scope.FGInventoryRegisterPoPUpList = response.data;
        });
    };

    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "GoodsReceiveNote/FGGRNReport?grnId=" + data.Id;
    };

}


