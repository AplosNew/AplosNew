
'use strict';
ProductionDashboardController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProductionDashboardController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Production Dashboard";
    $scope.path = "Productions/ProductionDashboard/";


    $scope.FromDateParameter = new Date();
    $scope.SelectedEntity = null;
    $scope.SelectedCompany = null;
    $scope.SelectedPlant = null;
    $scope.EntityList = [];
    $scope.ProcessList = [];
    $scope.ProcessWiseProduction = [];
    $scope.ProcessWiseProductionGrid = [];
    $scope.BaseProcessId = null;


    $scope.CompanyList = [];
    $scope.PlantListMain = [];
    $scope.EntityListMain = [];

    $scope.PlantList = [];
    $scope.EntityList = [];


    $http({
        method: 'GET',
        url: $scope.path + 'GetAllCompaniesAndPlants'
    }).then(function successCallback(response) {
        $scope.PlantListMain = response.data.Plant;
        $scope.SelectedPlant = response.data.PlantId;
        $scope.BaseProcessId = response.data.BaseProcessId;

        $scope.CompanyList = response.data.Company;
        $scope.SelectedCompany = response.data.CompanyId;
        $scope.getAllEntitiesAndProcess();
    });

    $scope.getAllEntitiesAndProcess = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/productionOrderSchedulingParametersType1/GetAllEntity'
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                $scope.EntityListMain = response.data;
            }
            $scope.ChangeCompany();
            $scope.ChangePlant();
            $scope.DrawWorkcenterWisePlan();
        });
    }
    // $scope.getAllEntitiesAndProcess();
    function EntityText() {
        $scope.SelectedEntityText = $('#ddlEntity option:selected').text();
    }

    $scope.ChangeCompany = function () {
        $scope.EntityList = [];
        $scope.PlantList = [];
        $scope.PlantList = ej.DataManager($scope.PlantListMain).executeLocal(ej.Query().where("CompanyId", "equal", $scope.SelectedCompany));
    }
    $scope.ChangePlant = function () {
        $scope.EntityList = [];
        $scope.EntityList = ej.DataManager($scope.EntityListMain).executeLocal(ej.Query().where("PlantId", "equal", $scope.SelectedPlant));
    }

    $scope.chartPreRender = function (args) {

        try {
            var points = args.model.series[0].points;
            var pointsTarget = args.model.series[1].points;
            for (var i = 0; i < points.length; i++) {
                if (points[i].y >= pointsTarget[i].y)
                    points[i].fill = "#06B200";
                else
                    points[i].fill = "#FF7769";

            }
        } catch (e) {

        }
    }
    $scope.chartWIPPreRender = function (args) {

        try {
            var points = args.model.series[0].points;//WIP
            var pointsTarget = args.model.series[1].points;//Capacity
            for (var i = 0; i < points.length; i++) {
                if (points[i].y >= pointsTarget[i].y)
                    points[i].fill = "#FF7769";
                else
                    points[i].fill = "#06B200";

            }
        } catch (e) {

        }
    }


    $scope.chartProfitabilityPreRender = function (args) {

        try {
            var points = args.model.series[0].points;
            var pointsTarget = args.model.series[1].points;
            for (var i = 0; i < points.length; i++) {
                if (points[i].y >= pointsTarget[i].y)
                    points[i].fill = "#06B200";
                else
                    points[i].fill = "#FF7769";

            }
        } catch (e) {

        }
    }

    $scope.graphmaxheight = function (list, column) {
        var _graphmaxheight = 10;
        _graphmaxheight = 10;
        for (var i = 0; i < list.length; i++) {
            if (list[i][column] > _graphmaxheight)
                _graphmaxheight = list[i][column];
        }

        return _graphmaxheight + (_graphmaxheight * .30);
    }

    $scope.graphmaxwidth = function (list, width) {
        if (baseService.isUndefinedOrNull(width))
            width = 100;

        return ((list.length * width) + 100) + 'px';
    }

    $scope.EntityIndex = 0;

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;

    };

    $scope.DrawWorkcenterWisePlan = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.SelectedPlant))
                throw 'Select plant';

            $http({
                method: 'GET',
                url: $scope.path + 'GetProcessWiseProduction?PlantId=' + $scope.SelectedPlant + '&EntityId=' + $scope.SelectedEntity + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
            }).then(function successCallback(response) {
                $scope.ProcessWiseProduction = response.data;
                $scope.ProcessWiseProductionGrid = response.data;

                if (response.data.length == 0)
                    $scope.ProcessWiseProduction = [];
                var chartObj = $("#chartProcessWiseProduction").data("ejChart");
                chartObj.redraw();

                $scope.GetDailyPlanVsProduction();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    $scope.DayStatistics = [];
    $scope.WorkcenterWiseWIPListGraph = [];
    $scope.DailyPlanVsProduction = [];
    $scope.DailyLast30DaysPlanVsProduction = [];
    $scope.DailyPrifitability = [];

    $scope.ProcessName = "";
    $scope.ChartCaptions = { PlanVsProductionWC: 'Workcenter Wise Production Vs Plan', PlanVsProduction30: 'Day Wise Production vs Plan', DailyPrifitability: 'Workcenter Wise Cost' };
    $scope.GetDailyPlanVsProduction = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetDailyPlanVsProduction?PlantId=' + $scope.SelectedPlant + '&EntityId=' + $scope.SelectedEntity + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        }).then(function successCallback(response) {
            $scope.DailyPlanVsProduction = response.data.PlanVsProductionWC;
            $scope.ProcessName = response.data.ProcessName;

            $scope.ChartCaptions.PlanVsProductionWC = 'Workcenter Wise Production Vs Plan (Process: ' + response.data.ProcessName + ')';
            $scope.ChartCaptions.PlanVsProduction30 = 'Day Wise Production vs Plan (Process: ' + response.data.ProcessName + ')';
            $scope.ChartCaptions.DailyPrifitability = 'CM Analysis (Process: ' + $scope.ProcessName + ')';

            if (response.data.PlanVsProductionWC.length == 0)
                $scope.DailyPlanVsProduction = [];


            var chartObj = $("#chartDailyPlanVsProduction").data("ejChart");
            chartObj.redraw();


        });

        $http({
            method: 'GET',
            url: $scope.path + 'GetDailyLast30DaysPlanVsProduction?PlantId=' + $scope.SelectedPlant + '&EntityId=' + $scope.SelectedEntity + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        }).then(function successCallback(response) {
            $scope.DailyLast30DaysPlanVsProduction = response.data.PlanVsProduction30;
            if (response.data.PlanVsProduction30.length == 0)
                $scope.DailyLast30DaysPlanVsProduction = [];

            var chartObj = $("#chartDailyLast30DaysPlanVsProduction").data("ejChart");
            chartObj.redraw();

        });

        $http({
            method: 'GET',
            url: $scope.path + 'GetLastDaysPlanVsProductionStatistics?PlantId=' + $scope.SelectedPlant + '&EntityId=' + $scope.SelectedEntity + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        }).then(function successCallback(response) {
            $scope.DayStatistics = response.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + 'GetProfitability?PlantId=' + $scope.SelectedPlant + '&EntityId=' + $scope.SelectedEntity + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        }).then(function successCallback(response) {
            $scope.DailyPrifitability = response.data;


            if (response.data.length == 0)
                $scope.DailyPrifitability = [];

            var chartObj = $("#chartDailyPrifitability").data("ejChart");
            chartObj.redraw();

        });

        $http({
            method: 'GET',
            url: $scope.path + 'GetWorkCenterWiseWIPForGraph?PlantId=' + $scope.SelectedPlant + '&EntityId=' + $scope.SelectedEntity + '&ProcessId=' + $scope.BaseProcessId + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        }).then(function successCallback(response) {
            $scope.WorkcenterWiseWIPListGraph = response.data;

            if (response.data.length == 0)
                $scope.WorkcenterWiseWIPListGraph = [];

            var chartObj = $("#chartWIP").data("ejChart");
            chartObj.redraw();
        });
    }

    $scope.trendlineRendering = function (args) {

        var a = 20;
    }

    $scope.summaryRowsProduction = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Quantity", dataMember: "Quantity", format: "{0:N0}" }],
        showCaptionSummary: true
    }];
    $scope.ProductionOrderWiseProductionList = [];
    $scope.getOrderWiseProduction = function (args) {
        $scope.ProductionOrderWiseProductionList = [];
        $scope.SelectedProcess = args;
        EntityText();
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionOrderWiseProduction?PlantId=' + $scope.SelectedPlant + '&EntityId=' + $scope.SelectedEntity + '&ProcessId=' + args.Id + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        }).then(function successCallback(response) {
            $scope.ProductionOrderWiseProductionList = response.data;
        });
        $rootScope.openPopup('dialogProductionOrderWiseProduction');
    }

    $scope.summaryRowsWIP = [{
        title: "Total Qty", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "InQuantityToday", dataMember: "InQuantityToday", format: "{0:N0}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "OutQuantityToday", dataMember: "OutQuantityToday", format: "{0:N0}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "KillQuantityToday", dataMember: "KillQuantityToday", format: "{0:N0}" },

            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "InQuantity", dataMember: "InQuantity", format: "{0:N0}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "OutQuantity", dataMember: "OutQuantity", format: "{0:N0}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "KillQuantity", dataMember: "KillQuantity", format: "{0:N0}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "WIP", dataMember: "WIP", format: "{0:N0}" }
        ],
        showCaptionSummary: true
    }];
    $scope.WorkcenterWiseWIPList = [];
    $scope.SelectedProcess = null;
    $scope.GetWorkCenterWiseWIP = function (args) {
        $scope.WorkcenterWiseWIPList = [];
        $scope.SelectedProcess = args;
        EntityText();
        $http({
            method: 'GET',
            url: $scope.path + 'GetWorkCenterWiseWIP?PlantId=' + $scope.SelectedPlant + '&EntityId=' + $scope.SelectedEntity + '&ProcessId=' + args.Id + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        }).then(function successCallback(response) {
            $scope.WorkcenterWiseWIPList = response.data;
        });
        $rootScope.openPopup('dialogWorkcenterWiseWIP');
    }

    
    //$scope.SelectedProcess = null;
    //$scope.GetProductionRelay = function (args) {
    //    $scope.WorkcenterWiseWIPList = [];
    //    $scope.SelectedProcess = args;
    //    EntityText();
    //    $http({
    //        method: 'GET',
    //        url: $scope.ProductionRelaypath + 'GetWorkCenterWiseWIP?PlantId=' + $scope.SelectedPlant + '&EntityId=' + $scope.SelectedEntity + '&ProcessId=' + args.Id + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
    //    }).then(function successCallback(response) {
    //        $scope.WorkcenterWiseWIPList = response.data;
    //    });
    //    $rootScope.openPopup('dialogWorkcenterWiseWIP');
    //}


    $scope.ProductionRelayList = [];

    $scope.getProductionRelay = function (args) {
        try {

            $scope.SelectedProcess = args;

            $http({
                method: 'GET',
                url: $scope.path + 'GetProductionRelay?PlantId=' + $scope.SelectedPlant + '&EntityId=' + $scope.SelectedEntity + '&ProcessId=' + args.Id,
            }).then(function successCallback(response) {

                for (var i = 0; i < response.data.length; i++) {
                    if (angular.isUndefinedOrNull(response.data[i].LSD) == false)
                        response.data[i].LSD = new Date(response.data[i].LSD);

                    if (angular.isUndefinedOrNull(response.data[i].StartDate) == false)
                        response.data[i].StartDate = new Date(response.data[i].StartDate);

                    if (angular.isUndefinedOrNull(response.data[i].PreviousProcessStartDate) == false)
                        response.data[i].PreviousProcessStartDate = new Date(response.data[i].PreviousProcessStartDate);

                    if (angular.isUndefinedOrNull(response.data[i].ClosedDate) == false)
                        response.data[i].ClosedDate = new Date(response.data[i].ClosedDate);
                }

                $scope.ProductionRelayList = response.data;
            })
            $rootScope.openPopup('dialogProductionRelay');            

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.rowDataBoundOrder = function rowDataBoundOrder(e) {
        try {
            if (angular.isUndefinedOrNull(e.data.PPRId) == true) return;

            if (angular.isUndefinedOrNull(e.data.ClosedDate) == false && angular.isUndefinedOrNull(e.data.StartDate) == false) {
                e.row.css("background-color", "#6FEAFF");
                return;
            }

            if (angular.isUndefinedOrNull(e.data.ClosedDate) == false && angular.isUndefinedOrNull(e.data.StartDate) == true) {
                e.row.css("background-color", "#FF502A");
                return;
            }

            if (angular.isUndefinedOrNull(e.data.PreviousProcessStartDate) == false && angular.isUndefinedOrNull(e.data.StartDate) == true) {
                e.row.css("background-color", "#FFB42A");
                return;
            }

            if (angular.isUndefinedOrNull(e.data.PreviousProcessStartDate) == false && angular.isUndefinedOrNull(e.data.StartDate) == false) {
                e.row.css("background-color", "#7EFF87");
                return;
            }

            //e.row.css("background-color", e.data.Color);
            //var inColor = invertColor(e.data.Color, true);
            //e.row.css("color", inColor);
        } catch (e) {

        }
    }
    $scope.rowDataBoundOrder = function rowDataBoundOrder(e) {
        try {
            if (angular.isUndefinedOrNull(e.data.PPRId) == true) return;

            if (angular.isUndefinedOrNull(e.data.ClosedDate) == false && angular.isUndefinedOrNull(e.data.StartDate) == false) {
                e.row.css("background-color", "#6FEAFF");
                return;
            }

            if (angular.isUndefinedOrNull(e.data.ClosedDate) == false && angular.isUndefinedOrNull(e.data.StartDate) == true) {
                e.row.css("background-color", "#FF502A");
                return;
            }

            if (angular.isUndefinedOrNull(e.data.PreviousProcessStartDate) == false && angular.isUndefinedOrNull(e.data.StartDate) == true) {
                e.row.css("background-color", "#FFB42A");
                return;
            }

            if (angular.isUndefinedOrNull(e.data.PreviousProcessStartDate) == false && angular.isUndefinedOrNull(e.data.StartDate) == false) {
                e.row.css("background-color", "#7EFF87");
                return;
            }

        } catch (e) {

        }
    }
    $scope.ProductionRelayAllCheck = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };
    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridProductionRelay").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ProductionRelayList.length; i++) {
                $scope.ProductionRelayList[i].Checked = ChkOrUnchk;
            }
        }
        else {
            for (var i = 0; i < filtered.length; i++) {
                filtered[i].Checked = ChkOrUnchk;
            }
        }


        var gridObj = $("#GridProductionRelay").data("ejGrid");
        gridObj.refreshContent();
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = "Save";
        $scope.ProductionRelay = {}
        $scope.ProductionRelayList = [];
    }
    $scope.ProductionRelayReport = function () {

        try {
          

            var file_src = $scope.path + 'GetProductionRelayReport?PlantId=' + $scope.SelectedPlant + '&EntityId=' + $scope.SelectedEntity + '&ProcessId=' + $scope.SelectedProcess.Id ;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.WCWIPRowDataBound = function (e) {

        try {
            if (e.data.AlignedWithPlan == false)
                e.row.css("color", '#a90000');
        } catch (e) {

        }
    }

    $scope.PRWIPRowDataBound = function (e) {

        try {
            if (e.data.AlignedWithPlanPR == false)
                e.row.css("color", '#a90000');
        } catch (e) {

        }
    }

    $scope.PRWiseWIPList = [];
    $scope.SelectedWorkcenter = null;
    $scope.WorkCenterPlanList = [];
    $scope.GetPRWiseWIP = function (args) {
        $scope.PRWiseWIPList = [];
        $scope.SelectedWorkcenter = args;
        $http({
            method: 'GET',
            url: $scope.path + 'GetPRWiseWIP?PlantId=' + $scope.SelectedPlant + '&EntityId=' + $scope.SelectedEntity + '&ProcessId=' + $scope.SelectedProcess.Id + '&WorkCenterMasterId=' + args.WorkCenterMasterId + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        }).then(function successCallback(response) {
            $scope.PRWiseWIPList = response.data;
        });
        $rootScope.openPopup('dialogPRWiseWIP');

        try {
            $http({
                method: 'POST',
                url: $scope.PlanPath + "GetSingleWorkcenterWiseTargetSummaryByDate?WorkCenterId=" + args.WorkCenterMasterId + "&Date=" + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
            }).then(function successCallback(response) {
                $scope.WorkCenterPlanList = response.data;
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {

            ShowResult(response.data.Message, 'failure');
        }

    }

    $scope.InOutKillWC = [];
    $scope.GetInWC = function (flag, type, data) {
        $scope.InOutKillWC = [];
        $scope.SelectedWorkcenter = data;

        var _url = $scope.path + 'GetInWC?FDUD=' + flag + '&PlantId=' + $scope.SelectedPlant + '&EntityId=' + data.EntityId + '&ProcessId=' + $scope.SelectedProcess.Id + '&WorkCenterMasterId=' + data.WorkCenterMasterId + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        if (type == "OUT")
            _url = $scope.path + 'GetOutWC?FDUD=' + flag + '&PlantId=' + $scope.SelectedPlant + '&EntityId=' + data.EntityId + '&ProcessId=' + $scope.SelectedProcess.Id + '&WorkCenterMasterId=' + data.WorkCenterMasterId + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        else if (type == "KILL")
            _url = $scope.path + 'GetKillWC?FDUD=' + flag + '&PlantId=' + $scope.SelectedPlant + '&EntityId=' + data.EntityId + '&ProcessId=' + $scope.SelectedProcess.Id + '&WorkCenterMasterId=' + data.WorkCenterMasterId + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))

        $http({
            method: 'GET',
            url: _url
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++)
                response.data[i].ProductionDate = new Date(response.data[i].ProductionDate);

            $scope.InOutKillWC = response.data;
        });
        $rootScope.openPopup('dialogInOutKill');

    }



    $scope.InOutKillPO = [];
    $scope.GetInPO = function (flag, type, data) {
        $scope.InOutKillPO = [];
        $scope.SelectedWorkcenter = data;

        var _url = $scope.path + 'GetInPO?FDUD=' + flag + '&ProductionOrderId=' + data.ProductionOrderId + '&ProcessId=' + $scope.SelectedProcess.Id + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        if (type == "OUT")
            _url = $scope.path + 'GetOutPO?FDUD=' + flag + '&ProductionOrderId=' + data.ProductionOrderId + '&ProcessId=' + $scope.SelectedProcess.Id + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        else if (type == "KILL")
            _url = $scope.path + 'GetKillPO?FDUD=' + flag + '&ProductionOrderId=' + data.ProductionOrderId + '&ProcessId=' + $scope.SelectedProcess.Id + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))

        $http({
            method: 'GET',
            url: _url
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++)
                response.data[i].ProductionDate = new Date(response.data[i].ProductionDate);
            $scope.InOutKillPO = response.data;
        });
        $rootScope.openPopup('dialogInOutKillPO');

    }

    $scope.GetInWCPO = function (flag, type, data) {
        $scope.InOutKillPO = [];
        $scope.SelectedWorkcenter = data;

        var _url = $scope.path + 'GetInWCPO?FDUD=' + flag + '&ProductionOrderId=' + data.ProductionOrderId + '&WorkCenterMasterId=' + $scope.SelectedWorkcenter.WorkCenterMasterId + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        if (type == "OUT")
            _url = $scope.path + 'GetOutWCPO?FDUD=' + flag + '&ProductionOrderId=' + data.ProductionOrderId + '&WorkCenterMasterId=' + $scope.SelectedWorkcenter.WorkCenterMasterId + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        else if (type == "KILL")
            _url = $scope.path + 'GetKillWCPO?FDUD=' + flag + '&ProductionOrderId=' + data.ProductionOrderId + '&WorkCenterMasterId=' + $scope.SelectedWorkcenter.WorkCenterMasterId + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))

        $http({
            method: 'GET',
            url: _url
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++)
                response.data[i].ProductionDate = new Date(response.data[i].ProductionDate);
            $scope.InOutKillPO = response.data;
        });
        $rootScope.openPopup('dialogInOutKillPO');

    }

    $scope.summaryRows = [{
        title: "Total Order Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N0}" }],
        showCaptionSummary: true

    }];
    $scope.summaryRowsForWorkCenter = [{
        title: "Total Planned Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "PlannedQuantity", dataMember: "PlannedQuantity", format: "{0:N0}" }],
        showCaptionSummary: true

    }];
    $scope.summaryRowsForProduction = [{
        title: "Total Production Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "ProductionQuantity", dataMember: "ProductionQuantity", format: "{0:N0}" }],
        showCaptionSummary: true

    }];
    $scope.PlanPath = 'OrderManagements/productionOrderSchedulingParametersType1/';
    $scope.VWPRODDATA = [];
    $scope.VWCDATA = [];
    $scope.VROWDATA = {};
    $scope.VPRDATA = {};
    $scope.VSTYLEDATA = [];
    $scope.SAMEDAYDATA = [];
    $scope.GetProductionPlanningData = function (id, PRID) {
        try {
            $scope.SalesOrderListForProductionOrderId = [];
            $http({
                method: 'POST',
                url: $scope.PlanPath + "GetProductionPlanningData?planrowid=" + id + "&ProductionOrderId=" + PRID + "&processid=" + $scope.BaseProcessId
            }).then(function successCallback(res) {

                $scope.VWCDATA = res.data.WCDATA;
                $scope.VWPRODDATA = res.data.WPRODDATA;
                $scope.VROWDATA = res.data.ROWDATA[0];
                $scope.VPRDATA = res.data.PRDATA[0];
                $scope.VSTYLEDATA = res.data.WSTYLEDATA;
                $scope.SAMEDAYDATA = res.data.SAMEDAYDATA;

                $scope.getSalesOrderOfProdOrderList(PRID);

                if (id) {
                    $("#dialogProductionPlanView").ejDialog("setTitle", "Plan Summary for Date: [" + $scope.VROWDATA.ProductionDate + "], Prod. Order [" + $scope.VPRDATA.ProductionOrderID + "]");
                }
                else {
                    $("#dialogProductionPlanView").ejDialog("setTitle", "Plan Summary for Prod. Order [" + PRID + "]");

                }
                var eDialog = $("#dialogProductionPlanView").data("ejDialog");
                eDialog.open();

            });
        } catch (e) {

        }
        //var schObj = $("#ResourceGroupSchedule").data("ejSchedule");
        //var appointments = schObj.getAppointments();


    }
    $scope.SalesOrderListForProductionOrderId = [];
    $scope.getSalesOrderOfProdOrderList = function (prodOrdId) {
        $scope.SalesOrderListForProductionOrderId = [];
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetProductionRecipeMaterialList?productionOrderId=' + prodOrdId
        }).then(function successCallback(response) {
            $scope.SalesOrderListForProductionOrderId = response.data;

        });
    }

    $scope.dataSourceLineGraph = [];
    $scope.showlinegraphPRWise = function () {

        try {
            $http({
                method: 'GET',
                url: $scope.PlanPath + "GetProductionPlanGraphPRWise?orderid=" + $scope.VPRDATA.ProductionOrderID
            }).then(function successCallback(res) {

                $scope.dataSourceLineGraph = res.data;

                $("#graph").ejDialog("setTitle", "Production Order#" + $scope.VPRDATA.ProductionOrderID);
                var eDialog = $("#graph").data("ejDialog");
                eDialog.open();
            });



        } catch (e) {

        }
    }

    $scope.dataSourceProductionLineGraph = [];
    $scope.showProductionlinegraphPRWise = function () {

        try {
            $http({
                method: 'GET',
                url: $scope.PlanPath + "GetProductionGraphPRWise?orderid=" + $scope.VPRDATA.ProductionOrderID
            }).then(function successCallback(res) {


                $scope.dataSourceProductionLineGraph = res.data;
                $("#graphProduction").ejDialog("setTitle", "Production Info Production Order#" + $scope.VPRDATA.ProductionOrderID);
                var eDialog = $("#graphProduction").data("ejDialog");
                eDialog.open();


                var chartObj = $("#chartProductionContainer").data("ejChart");
                chartObj.redraw();

            });


        } catch (e) {

        }
    }

    $scope.dataSourceLineGraph = [];
    $scope.showlinegraph = function (args) {

        try {

            $http({
                method: 'GET',
                url: $scope.PlanPath + "GetProductionPlanGraph?orderid=" + args.data.ProductionOrderID + "&workcentrid=" + args.data.WorkCenterMasterId
            }).then(function successCallback(res) {


                $scope.dataSourceLineGraph = res.data;

                $("#graph").ejDialog("setTitle", "Production Plan for Workcenter [" + args.data.WorkCenter + "], Production Order#" + args.data.ProductionOrderID);
                var eDialog = $("#graph").data("ejDialog");
                eDialog.open();

                var chartObj = $("#chartProductionContainer").data("ejChart");
                chartObj.redraw();
            });



        } catch (e) {

        }
    }

    $scope.workcenterclick = function (args) {
        $scope.GetWorkCenterParametersData(args.data.WorkCenterMasterId);
    }
    $scope.WORKCENTERPARAMS = [];
    $scope.WORKCENTERProductList = [];
    $scope.GetWorkCenterParametersData = function (Id) {
        try {
            $http({
                method: 'GET',
                url: $scope.PlanPath + "getWorkcenterParametersDisplay?WorkCenterMasterId=" + Id
            }).then(function successCallback(res) {

                $scope.WORKCENTERPARAMS = res.data.WORKCENTERPARAMS[0];
                $scope.WORKCENTERProductList = res.data.WORKCENTERProductList;

                $("#dialogWorkCenterParameters").ejDialog("setTitle", "Configurations for Work Center [" + $scope.WORKCENTERPARAMS.WorkCenter + "]");
                var eDialog = $("#dialogWorkCenterParameters").data("ejDialog");
                eDialog.open();
            });




        } catch (e) {

        }

    }

    $scope.dataSourceProductionLineGraph = [];
    $scope.showProductionlinegraph = function (args) {

        try {

            $http({
                method: 'GET',
                url: $scope.PlanPath + "GetProductionGraph?orderid=" + args.data.ProductionOrderID + "&workcentrid=" + args.data.WorkCenterMasterId
            }).then(function successCallback(res) {


                $scope.dataSourceProductionLineGraph = res.data;
                $("#graphProduction").ejDialog("setTitle", "Production Info for Workcenter [" + args.data.WorkCenter + "], Production Order#" + args.data.ProductionOrderID);
                var eDialog = $("#graphProduction").data("ejDialog");
                eDialog.open();


            });


        } catch (e) {

        }
    }

    $scope.getReport = function () {

        try {

            var file_src = $scope.path + 'GetProfitabilityReport?PlantId=' + $scope.SelectedPlant + '&EntityId=' + $scope.SelectedEntity + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'));

            $rootScope.report(file_src);

        } catch (e) {

        }

    }
    $scope.ProductionEfficiency = function () {

        try {

            var file_src = $scope.path + 'ProductionEfficiencyReport?PlantId=' + $scope.SelectedPlant + '&entityid=' + $scope.SelectedEntity + "&Date=" + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'));
            $rootScope.report(file_src);

        } catch (e) {

        }
    }



}