'use strict';
DailyPlanningAndProductionReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function DailyPlanningAndProductionReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Daily Planning And Production Report';
    $scope.Action = 'Save';
    $scope.path = 'Productions/DailyPlanningAndProductionReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    // Tab Change
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 ;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    $scope.tab3;
    $scope.setTab3 = function (newTab) {
        $scope.tab3 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab3 === tabNum;
    };

    //Tabe 1 start
    $scope.dailyProduction = {
        Id: null,
        PlantId: null,
        EntityId: null,
        ProcessId: null,
        ProductionOrderId: null,
        WorkCenterMasterId: null,
        ProductionBookingLevel: null,
        FromDate: null,
        ToDate: null
    };
    $scope.dailyProductionNew = Object.assign({}, $scope.dailyProduction);

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.dailyProductionNew.EntityId = $scope.entityList[0].Value;
                //default
                $scope.loadProcessList($scope.dailyProductionNew.EntityId);
            }
        });
    }
    $scope.getAllEntities();

    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
            if (baseService.arrayLength(result) === 1) {
                $scope.dailyProductionNew.ProcessId = $scope.processList[0].Value;
                $scope.getProdLevel();
                //default
                $scope.loadWC($scope.dailyProductionNew.ProcessId, $scope.dailyProductionNew.EntityId, $scope.dailyProductionNew.ProductionShiftId);
            }
        });
    };

    $scope.shiftList = [];
    $scope.GetShiftList = function () {
        $scope.shiftList = [];
        $http.get('Productions/Productionsummary/GetShiftList?processId=' + $scope.dailyProductionNew.ProcessId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.shiftList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.dailyProductionNew.ProductionShiftId = $scope.shiftList[0].Value;
                    }
                }
            });
    }


    $scope.wcList = [];
    $scope.loadWC = function () {
        cboService.GetWCProcessCbo($scope.dailyProductionNew.ProcessId, $scope.dailyProductionNew.EntityId, $scope.dailyProductionNew.ProductionShiftId, function (result) {
            $scope.wcList = result;
        });
    };

    $scope.ToCloseAllowed = false;
    $scope.ProductionOrderList = [];
    $scope.getProductionOrderPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.dailyProductionNew.WorkCenterMasterId)) {
            return ShowResult('Please Work Center.', 'failure');
        }
        $scope.ProductionOrderList = [];
        $http.get('Productions/DailyPlanningAndProductionReport/GetProductionOrderDataList?entityid=' + $scope.dailyProductionNew.EntityId + '&workCenterMasterId=' + $scope.dailyProductionNew.WorkCenterMasterId + '&productionLevel=' + $scope.dailyProductionNew.ProductionBookingLevel + '&processId=' + $scope.dailyProductionNew.ProcessId + '&ToCloseAllowed=' + $scope.ToCloseAllowed)
            .then(
                function successCallback(response) {
                    $scope.ProductionOrderList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

        angular.element(document.querySelector('#POItemPopup')).modal('show');

    };

    $scope.getSalesOrderByProdOrderList = function (prodOrdId) {
        $scope.openPopup('dialogSOItemsFromProductionOrder');
        $http({
            method: 'GET',
            url: 'Productions/DailyPlanningAndProductionReport/GetProductionRecipeMaterialList?productionOrderId=' + prodOrdId
        }).then(function successCallback(response) {
            $scope.SalesOrderListForProductionOrderId = response.data;

        });
    }

    $scope.SetPrOData = function ($event) {
        $scope.dailyProductionNew.ProductionOrderId = $event.data.POId;
        $scope.dailyProductionNew.BuyerItem = $event.data.BuyerItem;
        $scope.dailyProductionNew.OwnItem = $event.data.OwnItem;
        $scope.dailyProductionNew.BuyerOrder = $event.data.BuyerOrder;
        $scope.dailyProductionNew.OwnOrder = $event.data.OwnOrder;

        $scope.dailyProductionNew.ProductLibraryId = null;
        $scope.dailyProductionNew.ProductCode = null;
        $scope.dailyProductionNew.MasterOrderItemId = null;
        $scope.dailyProductionNew.SalesOrderId = null;
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
        //$scope.GetTotalProductionBookingQty();
        //$scope.getLotNumberCbo();
    }


    $scope.DailyPlanningProductionDataList = [];
    $scope.GetDailyPlanningProductionData = function () {
        $http.get('Productions/DailyPlanningAndProductionReport/GetDailyPlanningProductionData?fromdate=' + $scope.dailyProductionNew.FromDate + '&todate=' + $scope.dailyProductionNew.ToDate + '&entityId=' + $scope.dailyProductionNew.EntityId + '&processId=' + $scope.dailyProductionNew.ProcessId + '&shiftId=' + $scope.dailyProductionNew.ProductionShiftId + '&wcId=' + $scope.dailyProductionNew.WorkCenterMasterId + '&POId=' + $scope.dailyProductionNew.ProductionOrderId)
            .then(function (response) {
                $scope.DailyPlanningProductionDataList = [];
                $scope.DailyPlanningProductionDataList = response.data;
            });
    };

    //Tab 1 End

    //Route Emp Start

    $scope.ModelList = [];
    $scope.view = function () {
        $http({
            method: "Get",
            url: $scope.path + 'GetRouteEmployeesData',
            //data: { 'parameters': $scope.parameters },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        })
    }
    $scope.view();

    $scope.AssignReport = function () {
        $scope.fileName = 'Summary List';

        var dataList = [];
        var g = $("#GridRouteEmp").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ModelList;
        }

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.ModelAssignEmployeeList = [];
    $scope.AssignEmployeeView = function () {
        if (baseService.isUndefinedOrNull($scope.PlantId)) {
            $scope.PlantId = $window.plantId;
        }
        $http({
            method: "Get",
            url: $scope.path + 'getemployeeListRoute?PlantId=' + $scope.PlantId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelAssignEmployeeList = response.data;
        })
    }
    $scope.AssignEmployeeView();

    $scope.AssignEmployeeReport = function () {
        $scope.fileName = 'To Assign List';

        var dataList = [];
        var g = $("#GridEmp").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ModelAssignEmployeeList;
        }

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };


    $scope.UnassignReport = function () {
        $scope.fileName = 'To Unassign List';
        var dataList = [];
        var g = $("#GridEUnassign").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ModelUnassignList;
        }
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };


    $scope.ModelUnassignList = [];
    $scope.UnassignView = function () {
        if (baseService.isUndefinedOrNull($scope.PlantId)) {
            $scope.PlantId = $window.plantId;
        }
        $http({
            method: "Get",
            url: $scope.path + 'viewUnassign?PlantId=' + $scope.PlantId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelUnassignList = response.data;
        })
    }
    $scope.UnassignView();

    $scope.ModelTransportSummaryList = [];
    $scope.viewTransportSummary = function () {
        $http({
            method: "Get",
            url: $scope.path + 'GetTransportSummaryData',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelTransportSummaryList = response.data;
        })
    }
    $scope.viewTransportSummary();

    $scope.TransportSummaryReport = function () {
        $scope.fileName = 'Transport Status Detail';

        var dataList = [];
        var g = $("#GridTranSummary").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ModelTransportSummaryList;
        }

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

  
}