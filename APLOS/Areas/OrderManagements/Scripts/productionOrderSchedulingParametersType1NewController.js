'use strict';
ProductionOrderSchedulingParametersType1NewController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http"];
function ProductionOrderSchedulingParametersType1NewController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http) {
    $rootScope.title = "Product Planning";
    $scope.Action = 'Save';
    $scope.index = -1;

    $scope.EntityId = null;
    $scope.baseProcess = { Id: null, UserName: null };
    $scope.modelList = [];
    $scope.productionMaterialList = [];
    $scope.prdProcessSetList = [];
    $scope.productionEntityList = [];
    $scope.productionWorkCenterList = [];
    $scope.runningWorkCenterList = [];
    $scope.productWorkCenterList = [];
    $scope.productionStatusList = [];
    $scope.MaterialImagePath = virtualPath.ProductsImage;
    $scope.sortSettings = { sortedColumns: [{ field: "ProductionStatus", direction: "descending" }, { field: "LSD", direction: "ascending" }] };
    $scope.path = 'OrderManagements/productionOrderSchedulingParametersType1/';
    $scope.getListUrl = $scope.path + 'GetListNew';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.incrementType = [
        { text: "FIXED", contentType: "textonly" },
        { text: "PERCENTAGE", contentType: "textonly", selected: "selected" }];
    cboService.getProductionStatusCboByGroup(function (result) {
        $scope.productionStatusList = result;
    });

    $scope.planningTypeProcessList = [];
    $scope.GetPlanningTypeProcess = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetPlanningTypeProcessCbo'
        }).then(function successCallback(response) {
            $scope.planningTypeProcessList = response.data;
        });
    }
    $scope.GetPlanningTypeProcess();

    $scope.modelFilterByList = [
        { 'name': 'Prod. Order#', 'value': 'Id' },
        { 'name': 'Prod. Status', 'value': 'ProductionStatus' },
        { 'name': 'Material', 'value': 'Material' },
        { 'name': 'Product', 'value': 'Product' },
        { 'name': 'Product Category', 'value': 'ProductCategory' },
        { 'name': 'Master Order No', 'value': 'MasterOrderId' },
        { 'name': 'Buyer Order#', 'value': 'BuyerRefNo' },
        { 'name': 'Own Order#', 'value': 'OwnRefNo' },
        { 'name': 'Buyer Item#', 'value': 'StyleNo' },
        { 'name': 'Own Item#', 'value': 'OwnStyleNo' },
        { 'name': 'SO No', 'value': 'SONo' },
        { 'name': 'SO Desc', 'value': 'SODesc' },
        { 'name': 'Buyer', 'value': 'buyer' },
        { 'name': 'Customer', 'value': 'Customer' },
    ];
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
    $scope.openPopupAngular = function (popupName) {
        try {
            angular.element(document.querySelector("#" + popupName + "")).modal("show");
        } catch (e) {

        }

    }

    $scope.modelPriorityList = [];
    $scope.loadDataForPriority = function () {
        $scope.modelPriorityList = [];
        var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
        $scope.EntityId = DropDownEntityListObj.getSelectedValue();

        if (angular.isUndefinedOrNull($scope.EntityId)) {
            for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                if (angular.isUndefinedOrNull($scope.EntityId)) {
                    $scope.EntityId = + DropDownEntityListObj.popupListItems[i].Id;
                } else {
                    $scope.EntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                }
            }
        }
        try {
            $http({
                method: 'POST',
                data: {
                    'baseprocessid': $scope.PlanningTypeProcessId, 'entityid': $scope.EntityId, 'column': '', 'value': ''
                },
                url: $scope.getListUrl
            }).then(function successCallback(response) {
                for (var i = 0; i < response.data.length; i++) {
                    response.data[i].LSD = new Date(response.data[i].LSD);
                    response.data[i].FirstShipmentDate = new Date(response.data[i].FirstShipmentDate);
                    response.data[i].LastShipmentDate = new Date(response.data[i].LastShipmentDate);
                    response.data[i].LSD = new Date(response.data[i].LSD);
                }

                $scope.modelPriorityList = response.data;
            });
        } catch (e) {

        }
    }

    //$scope.modelFilterByList = [
    //    { value: 'Id', name: 'Order ID ' }, { value: 'EntityName', name: 'Entity ' }, { value: 'ProductionStatusName', name: 'Production Status ' }
    //    , { value: 'Product', name: 'Product ' }, { value: 'ProductCategory', name: 'Product Category ' }, { value: 'Material', name: 'Material ' }
    //    , { value: 'buyer', name: 'Buyer ' }
    //];
    $scope.linedaystooltip = "Order Quantity/Taret Quantity Per Day"
    $scope.requiredNoOfLines = "Required Line Days/Min. Line Days";
    $scope.targetperhourtooltip = "Workstations x 60 / SPT"
    $scope.targetperdaytooltip = "Target Per Hour x Plan Working Hours"
    $scope.targetPerDayOnEficiency = "Target Per day x Efficiency%"
   // baseService.init($scope.getListUrl, null, null, null, 'EntityName', 'EntityName');
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
    $scope.openPopupAngular = function (popupName) {
        try {
            angular.element(document.querySelector("#" + popupName + "")).modal("show");
        } catch (e) {

        }

    }

    $scope.getPriorityData = function (args) {
        var gridObj = $("#GridChangePriority").data("ejGrid");
        var kk = gridObj.model.query.queries;//"onSortBy"

        var dataManagerObj = ej.DataManager($scope.modelList);
        var query = ej.Query();

        for (var i = 0; i < kk.length; i++) {
            if (kk[i].fn == "onSortBy") {
                query.queries.push(Object.assign({}, kk[i]));
            }
        }

        dataManagerObj = dataManagerObj.executeLocal(query);

        var index = 1;
        for (var i = 0; i < dataManagerObj.length; i++) {
            dataManagerObj[i].ProductionPriority = index;
            index++;
        }
        //$scope.modelList = dataManagerObj;
        //gridObj.refreshContent(true);
        var sorteddata = ej.DataManager(dataManagerObj).executeLocal(ej.Query().select(["Id", "ProductionPriority"]));
        $http({
            method: 'POST',
            url: $scope.path + "UpdatePriority",
            data: { data: sorteddata }
        }).then(function successCallback(response) {
            $scope.loadDataForPriority();
        });


    }
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
    //search
    $scope.closeSearch = function () {
        angular.element(document.querySelector('#searchNewPopup')).modal('hide');
    }

    $scope.printChart = function (chartname) {
        var chartObj = $('#' + chartname).ejChart("instance");
        chartObj.print(chartname);
    }
    $scope.printChart__ = function (chartname) {         // to download chart in client side

        var chartObj = $('#' + chartname).data("ejChart");
        chartObj.export('image');
        //var type = "png";

        //var chart = $("#" + chartname).ejChart("instance"),
        //    exporting = chart.model.exportSettings, data, type;
        //exporting.fileName = "chart";
        //exporting.angle = 0;
        //exporting.type = type;
        //exporting.mode = "client";
        //exporting.orientation = "landscape";
        //data = chart.export();
        //if ($window.navigator.msSaveOrOpenBlob) {     // for IE
        //    var blob;
        //    if (type == "png")
        //        blob = data.msToBlob();
        //    else if (type == "jpg")
        //        blob = data.msToBlob(null, "image/jpeg");
        //    else if (type == "svg") {
        //        data = decodeURIComponent(data);
        //        blob = new Blob([data], { type: "image/svg-xml" });
        //    }
        //    $window.navigator.msSaveOrOpenBlob(blob, exporting.fileName + "." + type);
        //}
        //else {
        //    this.download = exporting.fileName + "." + type;
        //    if (type == "png")
        //        this.href = data.toDataURL();
        //    else if (type == "jpg")
        //        this.href = data.toDataURL("image/jpeg");
        //    else
        //        this.href = "data:text/plain;charset=utf-8," + data;
        //}

    }

    $scope.ModelFilter = null;
    $scope.filtergridonload = function () {
        try {
            $("#GridPlanFilter").children('.e-pager.e-js.e-pager').hide();
            $("#GridPlanFilter").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#GridPlanFilter").children('.e-gridcontent').hide();
        } catch (e) {

        }

    }

    $scope.getModelFilter = function () {
        $http({
            method: 'POST',
            url: $scope.path + "LoadFilterSQL"
        }).then(function successCallback(response) {

            if (response.data.length > 0) {
                try {
                    $scope.ModelFilter = response.data;

                } catch (e) {

                }

            }
            $scope.filtergridonload();
        });

    };
    $scope.filterComplete = function (args) {
        if (args.requestType == "filtering") {
            var gridObj = $("#GridPlanFilter").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (angular.isUndefinedOrNull(filteredRecords) == false) {
                if (filteredRecords.length > 0) {
                    var parameters = [];
                    parameters.push({ "Key": "ProductOrderId", "Value": getString(filteredRecords, "ProductOrderId") });
                    parameters.push({ "Key": "WorkCenterId", "Value": getString(filteredRecords, "WorkCenterId") });
                    //parameters.push({ "Key": "EntityId", "Value": getString(filteredRecords, "EntityId") });
                    parameters.push({ "Key": "ProductMasterId", "Value": getString(filteredRecords, "ProductMasterId") });
                    parameters.push({ "Key": "ProductCategoryId", "Value": getString(filteredRecords, "ProductCategoryId") });
                    parameters.push({ "Key": "MaterialMasterId", "Value": getString(filteredRecords, "MaterialMasterId") });
                    parameters.push({ "Key": "ArticleId", "Value": getString(filteredRecords, "ArticleId") });
                    parameters.push({ "Key": "BuyerId", "Value": getString(filteredRecords, "BuyerId") });
                    parameters.push({ "Key": "CustomerId", "Value": getString(filteredRecords, "CustomerId") });
                    parameters.push({ "Key": "AccountInchargeId", "Value": getString(filteredRecords, "AccountInchargeId") });
                    parameters.push({ "Key": "AccountHolderId", "Value": getString(filteredRecords, "AccountHolderId") });
                    parameters.push({ "Key": "ProductionStatusId", "Value": getString(filteredRecords, "ProductionStatusId") });

                    parameters.push({ "Key": "MasterOrderNo", "Value": getString(filteredRecords, "MasterOrderNo") });
                    parameters.push({ "Key": "BuyerOrderNo", "Value": getString(filteredRecords, "BuyerOrderNo") });
                    parameters.push({ "Key": "BuyerItemNo", "Value": getString(filteredRecords, "BuyerItemNo") });


                    $scope.SimulateVisual(parameters);
                }
                else {
                    $scope.SimulateVisual(null);
                }
            }
        }
    }

    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }

        return string;
    }


    $scope.getProcessData = function () {


        $http({
            method: 'GET',
            url: $scope.path + "GetProcessForPlanning"
        }).then(function successCallback(response) {

            if (response.data.length > 0) {
                try {
                    $scope.baseProcess = response.data[0];
                    $scope.getData();
                } catch (e) {

                }

            }

        });

    };
   // $scope.getProcessData();

    $scope.PRSearchColumn = 'Id';
    $scope.PRSearchValue = null;
    $scope.isLoadedPlanningBoardForTheFirstTime = false;
    $scope.getData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.PlanningTypeProcessId)) {
                throw "Select Planning Type Process.";
            }
            var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
            $scope.EntityId = DropDownEntityListObj.getSelectedValue();
            
            if (angular.isUndefinedOrNull($scope.EntityId)) {
                for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                    if (angular.isUndefinedOrNull($scope.EntityId)) {
                        $scope.EntityId = + DropDownEntityListObj.popupListItems[i].Id;
                    } else {
                        $scope.EntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                    }
                }
            }
            $http({
                method: 'POST',
                data: {
                    'baseprocessid': $scope.PlanningTypeProcessId, 'entityid': $scope.EntityId, 'column': $scope.PRSearchColumn, 'value': $scope.PRSearchValue
                },
                url: $scope.getListUrl
            }).then(function successCallback(response) {
                for (var i = 0; i < response.data.length; i++) {
                    response.data[i].LSD = new Date(response.data[i].LSD);
                    response.data[i].FirstShipmentDate = new Date(response.data[i].FirstShipmentDate);
                    response.data[i].LastShipmentDate = new Date(response.data[i].LastShipmentDate);
                    response.data[i].LSD = new Date(response.data[i].LSD);
                }

                $scope.modelList = response.data;
                $scope.GetAllWorkcenterWisePlanningSummary();

                if ($scope.modelList.length > 0)
                    $scope.isLoadedPlanningBoardForTheFirstTime = true;

                if ($scope.TabActiveIndex == 1) {
                    $scope.OpenSimulatedData();
                }
            });


            $http({
                method: 'POST',
                data: {
                    'EntityId': $scope.EntityId
                },
                url: $scope.path + 'GetSPTEfficiencySlab'
            }).then(function successCallback(response) {

                $scope.SPTEfficiencySlab = response.data;
                if ($scope.SPTEfficiencySlab.length > 0)
                    $scope.model["IncrementType"] = 'PERCENTAGE';
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



    $scope.modelListNew = [];
    $scope.NewSearchParameters = {
        searchBy: 'Id'
        , search: null
    };


    $scope.getNewData = function () {
        var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
        $scope.EntityId = DropDownEntityListObj.getSelectedValue();

        if (angular.isUndefinedOrNull($scope.EntityId)) {
            for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                if (angular.isUndefinedOrNull($scope.EntityId)) {
                    $scope.EntityId = + DropDownEntityListObj.popupListItems[i].Id;
                } else {
                    $scope.EntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                }
            }
        }
        $http({
            method: 'POST',
            data: { 'column': $scope.NewSearchParameters.searchBy, 'value': $scope.NewSearchParameters.search, 'baseprocessid': $scope.PlanningTypeProcessId, 'entityid': $scope.EntityId },
            url: $scope.path + "GetPONewList"
        }).then(function successCallback(response) {
            $scope.modelListNew = response.data;
        });

    };


    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N0}" }],
        showCaptionSummary: true

    }];
    $scope.recipeMaterialListSelected = [];
    function getProductionRecipeMaterialList() {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetProductionRecipeMaterialList?productionOrderId=' + + $scope.productionOrderModel.Id
        }).then(function successCallback(response) {
            $scope.recipeMaterialListSelected = response.data;
        });
    }

    $scope.AddNewOrder = function () {
        $scope.getNewData();
        angular.element(document.querySelector('#searchNewPopup')).modal('show');
    }

    $scope.getProductionOrder = function () {


        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.getListUrl
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                response.data[i].LSD = new Date(response.data[i].LSD);
                response.data[i].FirstShipmentDate = new Date(response.data[i].FirstShipmentDate);
                response.data[i].LastShipmentDate = new Date(response.data[i].LastShipmentDate);
                response.data[i].LSD = new Date(response.data[i].LSD);
            }
            $scope.modelList = response.data;
        });

    };
    $scope.getProductionOrderParameters = function () {


        $http({
            method: 'GET',
            //data: { 'productionOrderID': $scope.productionOrderModel.Id },
            url: $scope.path + "getProductionOrderParameters?productionOrderID=" + $scope.productionOrderModel.Id
        }).then(function successCallback(response) {
            try {
                if (!$scope.model.ID) {
                    $scope.model = response.data[0];
                    if (baseService.isUndefinedOrNull($scope.model.Qty) || $scope.model.Qty=='NaN') {
                        $scope.model.Qty = $scope.PlannedQty;
                    }
                    $scope.model.LSD = $filter('dateFiltering')($scope.model.LSD, 'dd-MM-yyyy');
                    $scope.model.CommitmentDate = $filter('dateFiltering')($scope.model.CommitmentDate, 'dd-MM-yyyy');

                    $scope.model.MainRawMaterialInhouseDate = $filter('dateFiltering')($scope.model.MainRawMaterialInhouseDate, 'dd-MM-yyyy');
                    $scope.model.OtherRawMaterialInhouseDate = $filter('dateFiltering')($scope.model.OtherRawMaterialInhouseDate, 'dd-MM-yyyy');

                    if (!$scope.model["IncrementType"])
                        $scope.model["IncrementType"] = 'FIXED';

                    // $scope.calculations();
                }


            } catch (e) {

            }

            getProductionOrderWorkCenterList();
            getRunningOrderWorkCenterList();
            getProductMasterParameters();
        });

    };
    function getProductMasterParameters() {

        $http({
            method: 'GET',
            url: $scope.path + "getProductMasterParametersNew?productionOrderID=" + $scope.productionOrderModel.Id + "&entityid=" + $scope.productionOrderModel.EntityId + "&baseprocessid=" + $scope.PlanningTypeProcessId
        }).then(function successCallback(response) {
            $scope.displayModel = response.data.MainData[0];
            if (!$scope.model.ID) {
                //add new
                //$scope.model = Object.assign({}, $scope.displayModel);

                $scope.model["NoOfWorkStation"] = $scope.displayModel.NoOfWorkStation;
                $scope.model["Efficiency"] = $scope.displayModel.Efficiency;
                $scope.model["SPT"] = $scope.displayModel.SPT;
                $scope.model["PlanWorkingHoursPerDay"] = $scope.displayModel.PlanWorkingHoursPerDay;
                $scope.model["FirstDayOutPut"] = $scope.displayModel.FirstDayOutPut;
                $scope.model["IncrementValue"] = $scope.displayModel.IncrementValue;
                $scope.model["IncrementType"] = $scope.displayModel.IncrementType;


                $scope.model["LSD"] = $filter('dateFiltering')($scope.displayModel.LSD, 'dd-MM-yyyy');
                $scope.model["CommitmentDate"] = $filter('dateFiltering')($scope.displayModel.CommitmentDate, 'dd-MM-yyyy');

                $scope.model["MainRawMaterialInhouseDate"] = $filter('dateFiltering')($scope.displayModel.MainRawMaterialInhouseDate, 'dd-MM-yyyy');
                $scope.model["OtherRawMaterialInhouseDate"] = $filter('dateFiltering')($scope.displayModel.OtherRawMaterialInhouseDate, 'dd-MM-yyyy');

                if (!$scope.model["IncrementType"])
                    $scope.model["IncrementType"] = 'FIXED';

                if (!$scope.model["WCPreferenceType"])
                    $scope.model["WCPreferenceType"] = 'INCLUDE';

                if (response.data.BulletinData) {
                    if (response.data.BulletinData.length > 0) {
                        $scope.model.SPT = response.data.BulletinData[0].SPT;
                        $scope.model.NoOfWorkStation = response.data.BulletinData[0].MaxNoOfWS;
                        $scope.model.PlanWorkingHoursPerDay = response.data.BulletinData[0].PlannedHoursPerDay;
                    }
                }
                $scope.AutoEfficiencyPercentageOnSPT();
            }
            getProductWorkCenterList();
        });
    }
    function getProductWorkCenterList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductWorkCenterNewList?processid=' + $scope.PlanningTypeProcessId + '&productId=' + $scope.displayModel.Id + "&entityId=" + $scope.productionOrderModel.EntityId
        }).then(function successCallback(response) {
            $scope.productWorkCenterList = response.data;
        });
    }
    $scope.entityList = null;
    //cboService.getCboProductionEntityByPlant(null, null, $window.plantId, function (result)
    //{
    //    $scope.entityList = result;
    //});
    //$scope.getAllEntities = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + "GetAllEntityForPlanningType1"
    //    }).then(function successCallback(response) {
    //        $scope.entityList = response.data;
    //    });
    //}
    //$scope.getAllEntities();

    $scope.GetPlanningTypeEntiy = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetPlanningTypeEntityCbo?processId=' + $scope.PlanningTypeProcessId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }

    cboService.getProductionStatusCboByGroup(function (result) {
        $scope.productionStatusList = result;
    });
    //end search

    $scope.EfficiencyPercentage = 0;
    $scope.TargetQtyAtFullEfficiency = 0;
    $scope.SPTEfficiencySlab = [];
    $scope.AutoEfficiencyPercentageOnSPT = function () {
        if ($scope.SPTEfficiencySlab.length > 0) {
            try {
                var percentage = null;
                for (var i = 0; i < $scope.SPTEfficiencySlab.length; i++) {
                    if (i == 0) {
                        if ($scope.model.SPT >= $scope.SPTEfficiencySlab[i].Minimum && $scope.model.SPT <= $scope.SPTEfficiencySlab[i].Maximum) {
                            $scope.model.Efficiency = $scope.SPTEfficiencySlab[i].LastDayEfficiency;
                            $scope.model.IncrementValue = $scope.SPTEfficiencySlab[i].Increment;
                            $scope.model.IncrementType = 'PERCENTAGE';
                            percentage = $scope.SPTEfficiencySlab[i];
                            break;
                        }
                    }
                    else {
                        if ($scope.model.SPT > $scope.SPTEfficiencySlab[i].Minimum && $scope.model.SPT <= $scope.SPTEfficiencySlab[i].Maximum) {
                            $scope.model.Efficiency = $scope.SPTEfficiencySlab[i].LastDayEfficiency;
                            $scope.model.IncrementValue = $scope.SPTEfficiencySlab[i].Increment;
                            $scope.model.IncrementType = 'PERCENTAGE';
                            percentage = $scope.SPTEfficiencySlab[i];
                            break;
                        }
                    }
                }

                if (percentage == null) {
                    $scope.model.Efficiency = 0;
                    $scope.model.IncrementValue = 0;
                    $scope.model.IncrementType = 'PERCENTAGE';
                }

            } catch (e) {

            }



            try {
                $scope.calculations();//two calculations are required, first on calculates target per/day and then we can apply firstdayeff% and then second round calculations
                //i am a genius man!!!
                if (percentage != null) {
                    $scope.model.FirstDayOutPut = parseInt($scope.TargetQtyAtFullEfficiency * percentage.FirstDayEfficiency / 100);
                }

                $scope.calculations();
            } catch (e) {

            }
        }
        else {
            $scope.calculations();
        }


    }
    $scope.calculations = function () {

        $scope.model.TargetPerHour = 0;
        $scope.model.TargetPerDay = 0;
        $scope.EfficiencyPercentage = 0;
        $scope.TargetQtyAtFullEfficiency = 0;
        $scope.model.RequiredLineDays = 0;
        $scope.model.RequiredNoOfLines = 0;
        $scope.model.DayToReachTheTarget = 0;

        //$scope.AutoEfficiencyPercentageOnSPT();

        if ($scope.model.NoOfWorkStation > 0 || $scope.model.Efficiency > 0 || $scope.model.SPT > 0) {

            $scope.model.TargetPerHour = ($scope.model.NoOfWorkStation * 60 / $scope.model.SPT);
            $scope.TargetQtyAtFullEfficiency = $scope.model.TargetPerHour;
            if ($scope.model.TargetPerHour > 0) {

                $scope.model.TargetPerDay = ($scope.model.PlanWorkingHoursPerDay * $scope.model.TargetPerHour);
                $scope.EfficiencyPercentage = ($scope.model.TargetPerDay);// * $scope.model.Efficiency / 100;


                //at efficiency level
                $scope.model.TargetPerHour = $scope.model.TargetPerHour * $scope.model.Efficiency / 100;
                $scope.model.TargetPerDay = $scope.model.TargetPerDay * $scope.model.Efficiency / 100;



                $scope.model.RequiredLineDays = ($scope.productionOrderModel.SOQuantity / $scope.model.TargetPerDay).toFixed(2);
            }

            if ($scope.model.MinimumLineDays > 0) {

                $scope.model.RequiredNoOfLines = $scope.model.RequiredLineDays / $scope.model.MinimumLineDays;

                if ($scope.model.RequiredNoOfLines > 0 && $scope.model.RequiredNoOfLines <= 1)
                    $scope.model.AllocatedLines = 1;

                if ($scope.model.RequiredNoOfLines > 1)
                    $scope.model.AllocatedLines = Math.floor($scope.model.RequiredNoOfLines);
            }

            try {
                $scope.model.RequiredNoOfLines = $scope.model.RequiredNoOfLines.toFixed(4)
                $scope.model.RequiredLineDays = $scope.model.RequiredLineDays.toFixed(4)
            } catch (e) {

            }
        }
        if ($scope.model.FirstDayOutPut > 0 && $scope.model.IncrementValue > 0) {

            if ($scope.model.IncrementType == "FIXED" || $scope.model.IncrementType == "PERCENTAGE") {
                var daysrequired = 1;
                if ($scope.model.FirstDayOutPut < $scope.model.TargetPerHour) {
                    daysrequired = 1;
                    var firstdaysoutput = $scope.model.FirstDayOutPut;
                    while (firstdaysoutput * $scope.model.PlanWorkingHoursPerDay < $scope.model.TargetPerDay) {
                        daysrequired++;
                        if ($scope.model.IncrementType == "FIXED")
                            firstdaysoutput += $scope.model.IncrementValue;

                        //compounding method
                        if ($scope.model.IncrementType == "PERCENTAGE")
                            firstdaysoutput = firstdaysoutput + (firstdaysoutput * $scope.model.IncrementValue / 100);



                    }

                }
                $scope.model.DayToReachTheTarget = daysrequired.toFixed(2);
            }
            //if ($scope.model.IncrementType == "PERCENTAGE") {
            //    var daysrequired = 0;
            //    var firstdaysoutput = $scope.model.FirstDayOutPut;
            //    while (firstdaysoutput < $scope.model.TargetPerHour) {
            //        daysrequired++;
            //        //compounding method
            //        firstdaysoutput = firstdaysoutput * (1 + ($scope.model.IncrementValue / 100));
            //    }
            //    $scope.model.DayToReachTheTarget = daysrequired;
            //}
        }
    }

    //endcalculations


    // #endregion
    //change list: data,Id,Active,GridName
    $scope.MaterialID = "";
    $scope.isAlternative = -1;
    $scope.rowDataBound = function rowDataBound(e) {

        if ($scope.MaterialID != e.data.ArticleId) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.MaterialID = e.data.ArticleId;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", '#fff6b7');
        else
            e.row.css("background-color", '#d1e5ff');


    }
    $scope.rowDataBoundOrder = function rowDataBoundOrder(e) {
        try {
            e.row.css("background-color", e.data.Color);
            var inColor = invertColor(e.data.Color, true);
            e.row.css("color", inColor);
        } catch (e) {

        }

    }
    function invertColor(hex, bw) {
        if (hex.indexOf('#') === 0) {
            hex = hex.slice(1);
        }
        // convert 3-digit hex to 6-digits.
        if (hex.length === 3) {
            hex = hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
        }
        if (hex.length !== 6) {
            throw new Error('Invalid HEX color.');
        }
        var r = parseInt(hex.slice(0, 2), 16),
            g = parseInt(hex.slice(2, 4), 16),
            b = parseInt(hex.slice(4, 6), 16);
        if (bw) {
            return (r * 0.299 + g * 0.587 + b * 0.114) > 186
                ? '#000000'
                : '#FFFFFF';
        }
        // invert color components
        r = (255 - r).toString(16);
        g = (255 - g).toString(16);
        b = (255 - b).toString(16);
        // pad each with zeros and return
        return "#" + padZero(r) + padZero(g) + padZero(b);
    }
    function padZero(str, len) {
        len = len || 2;
        var zeros = new Array(len).join('0');
        return (zeros + str).slice(-len);
    }
    $scope.rowDataBoundWorkCenter = function rowDataBoundWorkCenter(e) {

        //if (angular.isUndefinedOrNull($scope.productWorkCenterList) == false) {
        //    if (angular.isUndefinedOrNull($scope.productionWorkCenterList) == false) {

        //        for (var i = 0; i < $scope.productWorkCenterList.length; i++) {
        //            for (var j = 0; j < $scope.productionWorkCenterList.length; j++) {
        //                if (scope.productWorkCenterList[i].Code == scope.productionWorkCenterList[j].Code)
        //                    e.row.css("background-color", "#00ff00");
        //            }
        //        }
        //    }
        //}
        if (angular.isUndefinedOrNull($scope.productWorkCenterList) == false) {
            for (var i = 0; i < $scope.productWorkCenterList.length; i++) {
                if ($scope.productWorkCenterList[i].Code == e.data.Code)
                    e.row.css("background-color", "#00ff00");

            }
        }

    }
    $scope.changeRunningOrderResidual = function (args) {
        if (args.isInteraction == false)
            return;
        var gridObjRunning = $("#GridWorkCenterForRunningHeaven").ejGrid("instance");
        var currRow = gridObjRunning.model.currentViewData[this.element.closest("tr").index()];
        for (var i = 0; i < $scope.runningWorkCenterList.length; i++) {
            $scope.runningWorkCenterList[i].isResidualApplicable = false;
        }
        if (args.isChecked)
            currRow.isResidualApplicable = true;

        var tempo = [];
        for (var i = 0; i < $scope.runningWorkCenterList.length; i++) {
            tempo.push(Object.assign({}, $scope.runningWorkCenterList[i]))
        }
        $scope.runningWorkCenterList = [];


        gridObjRunning.refreshContent(true);
        gridObjRunning.refreshTemplate();

        $scope.runningWorkCenterList = tempo;

        gridObjRunning.refreshContent(true);
        gridObjRunning.refreshTemplate();

    }
    function checkChangeSOItem(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.recipeMaterialList, { 'SalesOrderId': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Checked = true;
            else
                row[0].Checked = false;
        }
        //$rootScope.genericPushInTempList(data, event, $scope.productionMaterialList, 'SalesOrderId', 'SalesOrderId');
    }

    $scope.AddNewWorkCenter = function () {
        if ($scope.workcenterfor == 'RUNNING') {
            for (var i = 0; i < $scope.workCenterList.length; i++) {
                var exists = ej.DataManager($scope.runningWorkCenterList).executeLocal(ej.Query().where("Code", "equal", $scope.workCenterList[i].Code));
                if ($scope.workCenterList[i].Flag == true) {
                    if (exists.length == 0) {
                        $scope.runningWorkCenterList.push({
                            Id: null
                            , isResidualApplicable: false
                            , Plant: $scope.workCenterList[i].Plant
                            , Entity: $scope.workCenterList[i].Entity
                            , WorkCenterMasterId: $scope.workCenterList[i].WorkCenterMasterId
                            , ProductionOrderId: $scope.productionOrderModel.Id
                            , Code: $scope.workCenterList[i].Code
                            , UserName: $scope.workCenterList[i].UserName
                        });
                    }
                }
                else {
                    if (exists.length > 0) {
                        exists.pop();
                    }
                }
            }

        }
        else {

            for (var i = 0; i < $scope.workCenterList.length; i++) {
                var exists = ej.DataManager($scope.productionWorkCenterList).executeLocal(ej.Query().where("Code", "equal", $scope.workCenterList[i].Code));
                if ($scope.workCenterList[i].Flag == true) {
                    if (exists.length == 0) {
                        $scope.productionWorkCenterList.push({
                            Id: null
                            , Plant: $scope.workCenterList[i].Plant
                            , Entity: $scope.workCenterList[i].Entity
                            , WorkCenterMasterId: $scope.workCenterList[i].WorkCenterMasterId
                            , ProductionOrderId: $scope.productionOrderModel.Id
                            , Code: $scope.workCenterList[i].Code
                            , UserName: $scope.workCenterList[i].UserName
                        });
                    }
                }
                else {
                    if (exists.length > 0) {
                        exists.pop();
                    }
                }
            }
        }

        $scope.CloseWorkCenterPopUp();
    }
    function headCheckChangeSOItem(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#GridSOItem").data("ejGrid");
            var filtered = $("#GridSOItem").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                    $scope.recipeMaterialList[i].Checked = true;
                }
            }
            else {
                for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.recipeMaterialList[i].SalesOrderId == filtered[j].SalesOrderId)
                            $scope.recipeMaterialList[i].Checked = true;
                    }

                }
            }

            var checkbox = $("#GridSOItem .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridSOItem .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridSOItem .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridSOItem .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeSOItem });
            }
        }
        else {
            var filtered = $("#GridSOItem").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                    $scope.recipeMaterialList[i].Checked = false;
                }
            }
            else {
                for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.recipeMaterialList[i].SalesOrderId == filtered[j].SalesOrderId)
                            $scope.recipeMaterialList[i].Checked = false;
                    }

                }
            }
            var checkbox = $("#GridSOItem .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridSOItem .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridSOItem .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridSOItem .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeSOItem });
            }
        }
        //header level check
    }
    $scope.dataBoundSOItem = function (args) {
        $("#GridSOItem .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeSOItem });

    }

    ////if the grid has scrollbars, you have to use the two functions (window.onresize,actioncomplete)
    //$window.onresize = function (event) {
    //    //$scope.actionCompleteSearch();
    //    $scope.actionCompleteSelected();
    //    $scope.actionCompleteSearch();
    //}
    //$scope.actionCompleteSearch = function (args) {
    //    try {
    //        var gridObj = $("#GridSOItem").ejGrid("instance");
    //        var scrollerwidth = $("#orderModal").width();//Obtain the width of the container
    //        if (scrollerwidth < 600)
    //            scrollerwidth = 600;
    //        //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
    //        gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20 } });//pass the obtainer width and height to gridmodel options
    //        gridObj.windowonresize();
    //    } catch (e) {

    //    }
    //}

    //$scope.actionCompleteSearch = function (args) {
    //    try {
    //        var gridObj = $("#Gridunassigned").ejGrid("instance");
    //        var scrollerwidth = $("#Tab").width();//Obtain the width of the container
    //        //if (scrollerwidth < 600)
    //        //    scrollerwidth = 600;
    //        //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
    //        gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 40, height: 300 } });//pass the obtainer width and height to gridmodel options
    //        gridObj.windowonresize();
    //    } catch (e) {

    //    }
    //}



    $scope.searchByList = [
        //{
        //    'name': 'Production Status',
        //    'value': 'ProductionStatusName'
        //},
        {
            'name': 'Recipe',
            'value': 'RecipeName'
        },
        {
            'name': 'Remarks',
            'value': 'Remarks'
        }
    ];
    $scope.productionOrderModel = {};
    $scope.model = { WCPreferenceType: 'INCLUDE' };
    $scope.displayModel = {};

    $scope.PlannedQty = 0;
    $scope.Get = function (Row) {
        $scope.Clear();
        $scope.productionOrderModel = Object.assign({}, Row.data);
        $scope.PlannedQty = Row.data.PlannedQty;
        $scope.getProductionOrderParameters();
        getProductionRecipeMaterialList();

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

        angular.element(document.querySelector('#searchNewPopup')).modal('hide');
    };
    $scope.Get1 = function (Id) {
        try {


            $http({
                method: 'GET',
                url: $scope.path + 'GetProductionReference?productionOrderId=' + Id
            }).then(function successCallback(response) {
                $scope.Clear();
                $scope.productionOrderModel = Object.assign({}, response.data[0]);
                $scope.getProductionOrderParameters();
                getProductionRecipeMaterialList();

                $scope.Action = 'Update';
            });
        } catch (e) {

        }
    };
    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    function numnericValidation(value, message, mandatory) {
        try {
            mandatory = true;
            message = "[" + message + "]";
            if (angular.isUndefinedOrNull(value) == true)
                throw "Please provide " + message;

            if (mandatory)
                if (value <= 0)
                    throw "Please provide " + message;

            if (value < 0)
                throw "Negative values are not allowed for field " + message;

        } catch (e) {
            throw e;
        }
    }

    $scope.validations = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.productionOrderModel.Id))
                throw "Plase select production order first";

            var RunningOrderQty = 0;
            var vacantLine = false;
            for (var i = 0; i < $scope.runningWorkCenterList.length; i++) {
                RunningOrderQty += $scope.runningWorkCenterList[i].Qty;
                if (baseService.isUndefinedOrNull($scope.runningWorkCenterList[i].Qty) || $scope.runningWorkCenterList[i].Qty == 0)
                    vacantLine = true;
            }

            if ($scope.runningWorkCenterList.length > 0)
                if (vacantLine == false)
                    throw "You cannot fix quantity for all running lines";

            if ($scope.model.Qty > 0) {
                if (RunningOrderQty > $scope.model.Qty)
                    throw "Total running order work center wise qunatity is greater than production order actual qty";
            }
            else {
                if (RunningOrderQty > $scope.productionOrderModel.SOQuantity)
                    throw "Total running order work center wise qunatity is greater than production order plan qty";
            }

            numnericValidation($scope.model.NoOfWorkStation, "no. of workstations");
            numnericValidation($scope.model.Efficiency, "Efficiency %");
            numnericValidation($scope.model.SPT, "SPT");
            numnericValidation($scope.model.PlanWorkingHoursPerDay, "Plan Working Hours Per Day");

            //numnericValidation($scope.mode.DayToReachTheTarget, "Day To Reach The Target");
            if (baseService.isUndefinedOrNull($scope.model.LSD))
                throw "Please provide Late Start Date [LSD]";
            if (baseService.isUndefinedOrNull($scope.model.CommitmentDate))
                throw "Please provide [Commitment Date]";

            numnericValidation($scope.model.ProductionPriority, "Production Priority");
            numnericValidation($scope.model.MinimumLineDays, "Minimum Line Days");
            numnericValidation($scope.model.AllocatedLines, "Allocated Lines");

            if ($scope.model.RequiredNoOfLines > 0 && $scope.model.RequiredNoOfLines <= 1)
                if ($scope.model.AllocatedLines != 1)
                    throw "Allocated line should be 1";

            if ($scope.model.RequiredNoOfLines > 1)
                if ($scope.model.AllocatedLines > Math.floor($scope.model.RequiredNoOfLines))
                    throw "Allocated line cannot be greater than " + Math.floor($scope.model.RequiredNoOfLines);

            //if (baseService.isUndefinedOrNull($scope.productionWorkCenterList) == false)
            //    if (($scope.productionWorkCenterList.length) > $scope.model.AllocatedLines)
            //        throw "No. of selected work centers cannot be greater than allocated lines";




            var daystoreachthetarget = 0;
            try {
                daystoreachthetarget = $scope.model.DayToReachTheTarget;
            } catch (e) {

            }
            if ($scope.model.MinimumLineDays < daystoreachthetarget)
                throw "Minimum line days cannot less than build days";


            if ($scope.model.FirstDayOutPut < $scope.model.TargetPerHour)
                if (daystoreachthetarget <= 0)
                    throw "First day output is less than plan taget per hour and there is no increment policy defined.";

        } catch (e) {
            try {
                if (e.hasOwnProperty("message")) {
                    if (e.message.match("Cannot read property")) {
                        e.message = e.message.replace("Cannot read property '", "Please provide [");
                        e.message = e.message.replace("' of undefined", "]");
                        throw e.message;
                    }
                }
            } catch (ex) {
                throw ex;
            }
            throw e;
        }
    }

    function getProductionOrderWorkCenterList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionOrderWorkCenterList?productionOrderId=' + $scope.productionOrderModel.Id
        }).then(function successCallback(response) {
            $scope.productionWorkCenterList = response.data;
        });
    }
    function getRunningOrderWorkCenterList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetRunningOrderWorkCenterList?productionOrderId=' + $scope.productionOrderModel.Id
        }).then(function successCallback(response) {
            $scope.runningWorkCenterList = response.data;
        });
    }

    $scope.Save = function () {
        try {

            $scope.validations();



            $scope.model.ProductionOrderID = $scope.productionOrderModel.Id;
            $http({
                method: 'POST'
                , url: $scope.saveUrl
                , data: {
                    'parameter': $scope.model,
                    'ProductionStatusId': $scope.model.ProductionStatusId,
                    'workcenterlist': $scope.productionWorkCenterList,
                    'runningworkcenterlist': $scope.runningWorkCenterList
                }
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();

                    ClearFields();
                }
                $scope.closeEntryDialog();
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
            //ShowResult(e, 'failure', 'parametersEntry');

        }

    }

    $scope.confirmdelete = false;
    $scope.Confirm = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.open();
        $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    };
    $scope.ConfirmClose = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.close();
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.model.Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl
                , data: { 'masterid': $scope.model.Id }
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };

    $scope.Clear = function () {
        ClearFields();


        return true;
    };
    $scope.closeEntryDialog = function () {
        try {
            $("#dialogProductionOrderParameters").data("ejDialog").close();
        } catch (e) {

        }
        try {
            $("#dialogProductionOrderParameters1").data("ejDialog").close();
        } catch (e) {

        }
    }

    function ClearFields() {
        $scope.tab = 1;
        $scope.Action = "Save";
        $scope.EfficiencyPercentage = 0;
        $scope.TargetQtyAtFullEfficiency = 0;
        $scope.recipeMaterialListSelected = [];
        $scope.model = {};
        $scope.model = { PlantId: $window.plantid };
        $scope.productionOrderModel = {};
        $scope.productionWorkCenterList = [];
        $scope.runningWorkCenterList = [];
        $scope.productWorkCenterList = [];
        $scope.model = {};
        $scope.model = {
            WCPreferenceType: 'INCLUDE', RunningOrderBlockSize: 1
        }
        $scope.displayModel = {};
        $scope.modelListNew = [];
        try {
            var gridObj = $("#GridSOItem").ejGrid("instance");
            gridObj.refreshContent(true);

        } catch (e) {

        }


        $scope.model.color = "#ffffff";
    }
    $scope.Clear();

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;

    };


    $scope.workCenterFilterList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Work Center',
            'value': 'UserName'
        }
    ];

    $scope.workCenterParameters = {
        limit: 500
        , offset: 0
        , order: 'asc'
        , sort: 'UserName'
        , searchBy: 'UserName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };

    $scope.pageno = 0;
    $scope.workCenterPopUp_back = function () {
        $rootScope.tempList = [];
        $rootScope.workCenterList = [];
        angular.forEach($scope.productionWorkCenterList, function (a) {
            $rootScope.tempList.push({
                Id: a.Id
                , Plant: a.Plant
                , Entity: a.Entity
                , WorkCenterMasterId: a.WorkCenterMasterId
                , ProductionOrderId: a.ProductionOrderId
                , Code: a.Code
                , UserName: a.UserName
                , Flag: true
            });
        });
        baseService.setCurrentPage('workCenterList');
        $scope.workCenterParameters.offset = 0;
        $scope.workCenterParameters.entityIds = $scope.productionOrderModel.EntityId; //baseService.getColumnValueList($scope.productionEntityList, 'EntityId');
        $scope.getWorkCenterData = function (pageno) {

            //$http({
            //    method: 'GET',
            //    url: $scope.path + 'GetWorkCenterList?entityIds=' + $scope.productionOrderModel.EntityId
            //}).then(function successCallback(res)
            //{
            //    $scope.workCenterList = res.data;
            //});

            baseService.paginationBase($scope.path + 'GetWorkCenterList', pageno, $scope.workCenterParameters)
                .then(function (result) {
                    $scope.workCenterList = result.Rows;
                    $scope.workCenterParameters.total_count = result.Rows.length;
                    for (var t = 0; t < baseService.arrayLength($scope.workCenterList); t++) {
                        $scope.workCenterList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'WorkCenterMasterId', $scope.workCenterList[t].WorkCenterMasterId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#workCenterPopUp')).modal('show');
        $scope.getWorkCenterData();
    };
    $scope.workCenterList = [];
    $scope.workcenterfor = '';
    //$scope.workcenterDialog = $("#workCenterPopUp").ejDialog({ target: "#entrycontainer" });
    $scope.workCenterPopUp = function (wcfor) {
        $scope.workcenterfor = wcfor;
        $rootScope.tempList = [];
        $scope.workCenterList = [];

        if (wcfor == 'RUNNING') {
            angular.forEach($scope.runningWorkCenterList, function (a) {
                $rootScope.tempList.push({
                    Id: a.Id
                    , Plant: a.Plant
                    , Entity: a.Entity
                    , WorkCenterMasterId: a.WorkCenterMasterId
                    , ProductionOrderId: a.ProductionOrderId
                    , Code: a.Code
                    , UserName: a.UserName
                    , Flag: true
                });
            });
        }
        else {
            angular.forEach($scope.productionWorkCenterList, function (a) {
                $rootScope.tempList.push({
                    Id: a.Id
                    , Plant: a.Plant
                    , Entity: a.Entity
                    , WorkCenterMasterId: a.WorkCenterMasterId
                    , ProductionOrderId: a.ProductionOrderId
                    , Code: a.Code
                    , UserName: a.UserName
                    , Flag: true
                });
            });
        }

        $http({
            method: 'GET',
            url: $scope.path + 'GetWorkCenterNewList?entityIds=' + $scope.productionOrderModel.EntityId + "&processid=" + $scope.PlanningTypeProcessId
        }).then(function successCallback(res) {
            $scope.workCenterList = res.data;

            if (baseService.arrayLength($scope.workCenterList) > 0) {
                for (var i = 0; i < $scope.workCenterList.length; i++) {
                    if (wcfor == 'RUNNING') {
                        for (var j = 0; j < $scope.runningWorkCenterList.length; j++) {
                            if ($scope.runningWorkCenterList[j].WorkCenterMasterId === $scope.workCenterList[i].WorkCenterMasterId) {
                                $scope.workCenterList[i].Flag = true;
                            }
                        }
                    }
                    else {
                        for (var j = 0; j < $scope.productionWorkCenterList.length; j++) {
                            if ($scope.productionWorkCenterList[j].WorkCenterMasterId === $scope.workCenterList[i].WorkCenterMasterId) {
                                $scope.workCenterList[i].Flag = true;
                            }
                        }
                    }

                }
            }
        });


        var eDialog = $("#workCenterPopUp").data("ejDialog");
        eDialog.open();
    }
    $scope.TabActiveIndex = -1;
    $scope.onactivetab = function (args) {
        try {
            $scope.TabActiveIndex = args.activeIndex;
            if (args.activeIndex == 1) {
                $scope.OpenSimulatedData();
                $scope.OpenSimulatedData();
            }
            else if (args.activeIndex == 2) {

                $scope.GetAllWorkcenterWisePlanningSummary();
            }
        } catch (e) {

        }

    }
    $scope.addWorkCenter = function () {
        if ($scope.workcenterfor == 'RUNNING') {
            if (baseService.arrayLength($rootScope.tempList) > 0) {
                angular.forEach($rootScope.tempList, function (a) {
                    if (!baseService.valueCheckInList($scope.runningWorkCenterList, 'WorkCenterMasterId', a.WorkCenterMasterId)) {
                        $scope.runningWorkCenterList.push({
                            Id: null
                            , isResidualApplicable: false
                            , Entity: a.Entity
                            , Plant: a.Plant
                            , WorkCenterMasterId: a.WorkCenterMasterId
                            , ProductionOrderId: $scope.productionOrderModel.Id
                            , Code: a.Code
                            , UserName: a.UserName
                        });
                    }
                });
            }
            else
                $scope.runningWorkCenterList = [];
            angular.forEach($scope.runningWorkCenterList, function (a) {
                if (!baseService.valueCheckInList($rootScope.tempList, 'WorkCenterMasterId', a.WorkCenterMasterId))
                    $scope.runningWorkCenterList.splice(a, 1);
            });
        }
        else {
            if (baseService.arrayLength($rootScope.tempList) > 0) {
                angular.forEach($rootScope.tempList, function (a) {
                    if (!baseService.valueCheckInList($scope.productionWorkCenterList, 'WorkCenterMasterId', a.WorkCenterMasterId)) {
                        $scope.productionWorkCenterList.push({
                            Id: null
                            , Entity: a.Entity
                            , Plant: a.Plant
                            , WorkCenterMasterId: a.WorkCenterMasterId
                            , ProductionOrderId: $scope.productionOrderModel.Id
                            , Code: a.Code
                            , UserName: a.UserName
                        });
                    }
                });
            }
            else
                $scope.productionWorkCenterList = [];
            angular.forEach($scope.productionWorkCenterList, function (a) {
                if (!baseService.valueCheckInList($rootScope.tempList, 'WorkCenterMasterId', a.WorkCenterMasterId))
                    $scope.productionWorkCenterList.splice(a, 1);
            });

        }


        $scope.CloseWorkCenterPopUp();
    };

    $scope.CloseWorkCenterPopUp = function () {

        var eDialog = $("#workCenterPopUp").data("ejDialog");
        eDialog.close();
    };

    // #endregion  Work Center

    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to delete [" + name + "] ";
            angular.element(document.querySelector('#confirmRecipeMaterialPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeRow = function () {
        for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
            if ($rootScope.tempList[t][$scope.tempId] === $scope[$scope.listName][$scope.popUpIndex][$scope.listId])
                $rootScope.tempList.splice(t, 1);
        }
        $scope[$scope.listName].splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmRecipeMaterialPopUp')).modal('hide');
    };




    ///////////////////////////////SCHEDULE////////////////////////////////
    $scope.data = window.gridData;
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
    $scope.appointments = [];
    $scope.setDate = new Date();
    $scope.group = {
        resources: ["WorkCenters"]
    };
    $scope.groupdata = [];
    $scope.resourcedata2 = {
        //dataSource: $scope.groupdata,
        dataSource: [
            { text: "Workcenter", id: 3, groupId: 1, color: "#ffaa00" }
        ],
        text: "text", id: "id", groupId: "groupId", color: "color"
    };

    $scope.workweek = ["Saturday", "Friday", "Monday", "Tuesday", "Wednesday", "Thursday"];
    $scope.FreezeDate = null;
    $scope.tempo = "";
    $scope.plancolorchange = function (args) {



        if ($scope.tempo != args.requestType) {
            $scope.tempo = args.requestType;
        }
        try {
            if (args.requestType == "resourcegroupheader") {
                args.element[0].innerText = "Work Centers";
                args.element.css("vertical-align", "middle");
                args.element.css("text-align", "center");
            }

            if (args.requestType == "headercells") {
                try {
                    try {
                        if (args.element[0].innerText.length > 4) {
                            args.element[0].innerText = "0" + args.element[0].innerText.substring(4);
                            args.element.css("color", "#0000ff");
                            args.element.css("vertical-align", "middle");
                            args.element.css("text-align", "center");
                        }

                    } catch (e) { }

                    var Ayear = args.model.currentDate().getFullYear();
                    var AMonth = args.model.currentDate().getMonth() + 1;
                    var ADay = parseInt(args.element[0].innerText);

                    var FDate = new Date($scope.FreezeDate);
                    var Fyear = FDate.getFullYear();
                    var FMonth = FDate.getMonth();
                    var FDay = FDate.getDate();

                    if (Ayear == Fyear & AMonth == FMonth & ADay == FDay) {
                        args.element.css("background", "#FF5733");
                    }
                } catch (e) {

                }

            }

            if (args.requestType == "appointment") {

                args.element.css("background", args.appointment.Color);
                args.element.css("border-color", args.appointment.Color);
                args.element.css("color", args.appointment.Color);
                args.element.css("font-size", "1px");
                args.element.css("height", "19px");

                try {
                    for (var i = 0; i < args.element.length; i++) {
                        args.element[i].innerText = "";
                    }
                } catch (e) {

                }

                if (args.appointment.isBuildUp == true) {
                    args.element.css("border-radius", "100%");
                }

                if (args.appointment.FilterData == 0) {
                    args.element.css("opacity", "0.1");
                }

                if (args.appointment.isStyleChange == true) {
                    args.element.css("border-color", "yellow");
                    args.element.css("border-style", "groove");
                    args.element.css("border-width", "4px");
                }

                if (args.appointment.planningStatus == "FREEZE") {
                    args.element.css("border-bottom", "4px  groove blue");
                }
                else if (args.appointment.planningStatus == "RUNNING") {
                    args.element.css("border-bottom", "4px  groove green");
                }
                if (args.appointment.FailedToCommitmentDate == true) {
                    args.element.css("border-top", "4px  groove red");
                }
            }
        } catch (e) {

        }

    }
    $scope.Simulate = function () {
        try {
            var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
            $scope.EntityId = DropDownEntityListObj.getSelectedValue();

            if (angular.isUndefinedOrNull($scope.EntityId)) {
                for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                    if (angular.isUndefinedOrNull($scope.EntityId)) {
                        EntityId = + DropDownEntityListObj.popupListItems[i].Id;
                    } else {
                        EntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                    }
                }
            }
            $http({
                method: 'GET',
                url: $scope.path + "ProductionPlanSimulationNew?entityid=" + $scope.EntityId + "&processid=" + $scope.PlanningTypeProcessId
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult("Simulated successfully", 'success');


                    var args = { "requestType": "filtering" };
                    $scope.filterComplete(args);

                }
            });
        } catch (e) {

        }
        //$scope.SimulateVisual();
    }
    $scope.OpenSimulatedData = function () {
        try {


            var args = { "requestType": "filtering" };
            $scope.filterComplete(args);

        } catch (e) {

        }
    }
    $scope.clickonschedule = function (args) {
        args.cancel = true;


    }

    $scope.OpenSchedule = function (args) {

        $scope.GetProductionPlanningData(args.appointment.Id, '');
        //args.cancel = true;

    }
    //$scope.viewtype = ["Month", "Agenda"];
    $scope.renderDates = {
        start: new Date(),
        end: new Date().setDate(new Date().getDate() + 30)
    }
    $scope.viewtype = ["CustomView"];
    $scope.currentDate = { day: new Date().getDate(), month: new Date().getMonth(), year: new Date().getFullYear() };


    $scope.SimulateVisual = function (ExtraParams) {
        var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
        $scope.EntityId = DropDownEntityListObj.getSelectedValue();

        if (angular.isUndefinedOrNull($scope.EntityId)) {
            for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                if (angular.isUndefinedOrNull($scope.EntityId)) {
                    EntityId = + DropDownEntityListObj.popupListItems[i].Id;
                } else {
                    EntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                }
            }
        }

        var _data = {};
        var _path = $scope.path + "GetNewScheduleData?entityid=" + $scope.EntityId + "&processid=" + $scope.PlanningTypeProcessId + "&year=" + $scope.currentDate.year + "&month=" + $scope.currentDate.month + "&day=" + $scope.currentDate.day;

        if (angular.isUndefinedOrNull(ExtraParams) == false) {
            _path = $scope.path + "GetNewScheduleDataFiltered?entityid=" + $scope.EntityId + "&processid=" + $scope.PlanningTypeProcessId + "&year=" + $scope.currentDate.year + "&month=" + $scope.currentDate.month + "&day=" + $scope.currentDate.day;

            var _data = {
                "parameters": ExtraParams
            }
        }
        try {
            $http({
                method: 'POST',
                url: _path,
                data: _data
            }).then(function successCallback(res) {

                if (res.data.DATA.length > 0) {
                    $scope.resourcedata2 = {
                        dataSource: res.data.GROUPDATA,
                        text: "text", id: "id", groupId: "groupId", color: "color"
                    };
                    //for (var i = 0; i < res.data.DATA.length; i++) {
                    //    res.data.DATA[i].AllDay = true;
                    //    res.data.DATA[i].Recurrence = false;
                    //}
                    $scope.workweek = res.data.WORKDAYDATA;
                    $scope.appointments = angular.copy(res.data.DATA);

                    try {
                        var gridObj = $("#GridPlanFilter").data("ejGrid");
                        //gridObj.clearFiltering();
                        $scope.getModelFilter();
                    } catch (e) {

                    }


                    $scope.FreezeDate = res.data.FREEZEDATE;

                    var schObj = $("#ResourceGroupSchedule").data("ejSchedule");

                    schObj.refresh(); // To refresh the Schedule control within the client side event
                    schObj.refreshAppointments();

                }
            });
        } catch (e) {

        }
        //var schObj = $("#ResourceGroupSchedule").data("ejSchedule");
        //var appointments = schObj.getAppointments();


    }


    $scope.navigation = function (args) {
        $scope.currentDate.year = args.currentDate.getFullYear();
        $scope.currentDate.month = args.currentDate.getMonth();
        $scope.currentDate.day = args.currentDate.getDate();

        var args = { "requestType": "filtering" };
        $scope.filterComplete(args);

    }

    $scope.VWPRODDATA = [];
    $scope.VWCDATA = [];
    $scope.VROWDATA = {};
    $scope.VPRDATA = {};
    $scope.VSTYLEDATA = [];
    $scope.SAMEDAYDATA = [];
    $scope.GetProductionPlanningData = function (id, PRID) {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetNewProductionPlanningData?planrowid=" + id + "&ProductionOrderId=" + PRID + "&processid=" + $scope.PlanningTypeProcessId
            }).then(function successCallback(res) {

                $scope.VWCDATA = res.data.WCDATA;
                $scope.VWPRODDATA = res.data.WPRODDATA;
                $scope.VROWDATA = res.data.ROWDATA[0];
                $scope.VPRDATA = res.data.PRDATA[0];
                $scope.VSTYLEDATA = res.data.WSTYLEDATA;
                $scope.SAMEDAYDATA = res.data.SAMEDAYDATA;


                if (id) {
                    $("#dialogProductionPlanView").ejDialog("setTitle", "Plan Summary for Date: [" + $scope.VROWDATA.ProductionDate + "], Prod. Order [" + $scope.VPRDATA.ProductionOrderID + "]");
                }
                else {
                    $("#dialogProductionPlanView").ejDialog("setTitle", "Plan Summary for Prod. Order [" + PRID + "]");

                }
                var eDialog = $("#dialogProductionPlanView").data("ejDialog");
                eDialog.open();

                getAllDisplayParameters();
            });
        } catch (e) {

        }
        //var schObj = $("#ResourceGroupSchedule").data("ejSchedule");
        //var appointments = schObj.getAppointments();


    }

    $scope.PRODUCTPARAMS = {};
    $scope.PRODUCTIONPARAMS = {};
    $scope.WORKCENTERPARAMS = {};
    $scope.WORKCENTERProductList = [];
    $scope.PRODUCTPARAMSWorkCenterList = [];//entityid HARD CODED, PLEASE REVIEW "4"
    $scope.PRODUCTIONPARAMSWorkCenterList = [];
    function getAllDisplayParameters() {

        try {
            $http({
                method: 'GET',
                url: $scope.path + "getProductMasterParametersDisplay?productionOrderID=" + $scope.VROWDATA.ProductionOrderId + "&entityid=" + $scope.VROWDATA.entityid
            }).then(function successCallback(res) {
                $scope.PRODUCTPARAMS = res.data.PRODUCTPARAMS[0];
                $scope.PRODUCTIONPARAMS = res.data.PRODUCTIONPARAMS[0];
                $scope.PRODUCTPARAMSWorkCenterList = res.data.PRODUCTPARAMSWorkCenterList;
                $scope.PRODUCTIONPARAMSWorkCenterList = res.data.PRODUCTIONPARAMSWorkCenterList;


                $scope.PRODUCTIONPARAMS.LSD = $filter('dateFiltering')($scope.PRODUCTIONPARAMS.LSD, 'dd-MM-yyyy');
                $scope.PRODUCTIONPARAMS.CommitmentDate = $filter('dateFiltering')($scope.PRODUCTIONPARAMS.CommitmentDate, 'dd-MM-yyyy');


            });
        } catch (e) {

        }
    }

    $scope.GetProductPlanningData = function () {
        try {

            $("#dialogProductMasterParameters").ejDialog("setTitle", "Configurations for Product [" + $scope.PRODUCTPARAMS.ProductName + "]");
            var eDialog = $("#dialogProductMasterParameters").data("ejDialog");
            eDialog.open();
        } catch (e) {

        }
    }

    $scope.oncloseprparams = function (args) {
        try {
            $scope.Clear();
            $("#workCenterPopUp").ejDialog({
                title: "Work Center",
                enableModal: true,
                showOnInit: false,
                target: "#entrycontainer"
            });
        } catch (e) {

        }

    }
    $scope.GetProductionPlanningParametersData = function (dialogName, targetName) {
        try {
            //$scope.productionOrderModel.Id = $scope.VROWDATA.ProductionOrderId;
            //$scope.getProductionOrderParameters();
            try {
                $("#workCenterPopUp").ejDialog({
                    title: "Work Center",
                    enableModal: true,
                    showOnInit: false,
                    target: "#" + targetName
                });
            } catch (e) {

            }


            $scope.Get1($scope.VROWDATA.ProductionOrderId);

            $("#" + dialogName).ejDialog("setTitle", "Configurations for Production Order [" + $scope.VROWDATA.ProductionOrderId + "]");
            var eDialog = $("#" + dialogName).data("ejDialog");
            eDialog.open();
        } catch (e) {

        }

    }
    $scope.GetWorkCenterParametersData = function (Id) {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "getWorkcenterParametersDisplay?WorkCenterMasterId=" + Id
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

    $scope.onworkcenterproductrow = function (e) {
        if ($scope.PRODUCTPARAMS.Id == e.data.Id)
            e.row.css("background-color", '#00ff00');
    }

    $scope.workcenterclick = function (args) {
        $scope.GetWorkCenterParametersData(args.data.WorkCenterMasterId);
    }
    $scope.workcenterclickbyid = function (args) {
        $scope.GetWorkCenterParametersData(args.data.Id);


    }

    $scope.graphmaxheight = 10;
    $scope.graphmaxwidth = '200px';
    $scope.dataSourceLineGraph = [];
    $scope.showlinegraph = function (args) {

        try {
            $scope.graphmaxwidth = '200px';
            $http({
                method: 'GET',
                url: $scope.path + "GetProductionPlanGraph?orderid=" + args.data.ProductionOrderID + "&workcentrid=" + args.data.WorkCenterMasterId
            }).then(function successCallback(res) {
                $scope.graphmaxheight = 10;
                for (var i = 0; i < res.data.length; i++) {
                    if (res.data[i].Quantity > $scope.graphmaxheight)
                        $scope.graphmaxheight = res.data[i].Quantity;
                }

                $scope.graphmaxwidth = ((res.data.length * 30) + 200) + 'px';
                $scope.graphmaxheight = $scope.graphmaxheight + ($scope.graphmaxheight * .10);

                $scope.dataSourceLineGraph = res.data;

                $("#graph").ejDialog("setTitle", "Production Plan for Workcenter [" + args.data.WorkCenter + "], Production Order#" + args.data.ProductionOrderID);
                var eDialog = $("#graph").data("ejDialog");
                eDialog.open();
            });



        } catch (e) {

        }
    }
    $scope.showlinegraphPRWise = function () {

        try {
            $scope.graphmaxwidth = '200px';
            $http({
                method: 'GET',
                url: $scope.path + "GetProductionPlanGraphPRWise?orderid=" + $scope.VPRDATA.ProductionOrderID
            }).then(function successCallback(res) {
                $scope.graphmaxheight = 10;
                for (var i = 0; i < res.data.length; i++) {
                    if (res.data[i].Quantity > $scope.graphmaxheight)
                        $scope.graphmaxheight = res.data[i].Quantity;
                }

                $scope.graphmaxwidth = ((res.data.length * 30) + 200) + 'px';
                $scope.graphmaxheight = $scope.graphmaxheight + ($scope.graphmaxheight * .10);

                $scope.dataSourceLineGraph = res.data;

                $("#graph").ejDialog("setTitle", "Production Order#" + $scope.VPRDATA.ProductionOrderID);
                var eDialog = $("#graph").data("ejDialog");
                eDialog.open();
            });



        } catch (e) {

        }
    }
    $scope.StyleGraph = [];
    $scope.GetStyleGraphData = function (styleno) {
        try {
            var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
            $scope.EntityId = DropDownEntityListObj.getSelectedValue();

            if (angular.isUndefinedOrNull($scope.EntityId)) {
                for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                    if (angular.isUndefinedOrNull($scope.EntityId)) {
                        EntityId = + DropDownEntityListObj.popupListItems[i].Id;
                    } else {
                        EntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                    }
                }
            }

            $scope.graphmaxwidth = '200px';
            $http({
                method: 'POST',
                url: $scope.path + "GetStyleData",
                data: { styleName: styleno, entityId: $scope.EntityId }
            }).then(function successCallback(res) {
                $scope.graphmaxheight = 10;
                for (var i = 0; i < res.data.STYLEGRAPH.length; i++) {
                    if (res.data.STYLEGRAPH[i].PlanQty > $scope.graphmaxheight)
                        $scope.graphmaxheight = res.data.STYLEGRAPH[i].PlanQty;

                    if (res.data.STYLEGRAPH[i].ProductionQty > $scope.graphmaxheight)
                        $scope.graphmaxheight = res.data.STYLEGRAPH[i].ProductionQty;
                }
                $scope.graphmaxwidth = ((res.data.STYLEGRAPH.length * 30) + 200) + "px";;
                $scope.graphmaxheight = $scope.graphmaxheight + ($scope.graphmaxheight * .10);

                $scope.StyleGraph = res.data.STYLEGRAPH;
                $("#Stylegraph").ejDialog("setTitle", "Production Plan for Style [" + styleno + "]");
                var eDialog = $("#Stylegraph").data("ejDialog");
                //$("#dialog").ejDialog({ maxWidth: ((res.data.STYLEGRAPH.length * 30) + 200) }).open;
                eDialog.open();

            });



        } catch (e) {

        }
    }
    $scope.dataSourceProductionLineGraph = [];
    $scope.showProductionlinegraph = function (args) {

        try {
            $scope.graphmaxwidth = '200px';
            $http({
                method: 'GET',
                url: $scope.path + "GetProductionGraph?orderid=" + args.data.ProductionOrderID + "&workcentrid=" + args.data.WorkCenterMasterId
            }).then(function successCallback(res) {
                $scope.graphmaxheight = 10;
                for (var i = 0; i < res.data.length; i++) {
                    if (res.data[i].Quantity > $scope.graphmaxheight)
                        $scope.graphmaxheight = res.data[i].Quantity;

                    if (res.data[i].TargetQty > $scope.graphmaxheight)
                        $scope.graphmaxheight = res.data[i].TargetQty;

                }
                $scope.graphmaxwidth = ((res.data.length * 30) + 200) + "px";
                $scope.graphmaxheight = $scope.graphmaxheight + ($scope.graphmaxheight * .10);

                $scope.dataSourceProductionLineGraph = res.data;
                $("#graphProduction").ejDialog("setTitle", "Production Info for Workcenter [" + args.data.WorkCenter + "], Production Order#" + args.data.ProductionOrderID);
                var eDialog = $("#graphProduction").data("ejDialog");
                eDialog.open();


            });


        } catch (e) {

        }
    }
    $scope.showProductionlinegraphPRWise = function () {

        try {
            $scope.graphmaxwidth = '200px';
            $http({
                method: 'GET',
                url: $scope.path + "GetProductionGraphPRWise?orderid=" + $scope.VPRDATA.ProductionOrderID
            }).then(function successCallback(res) {
                $scope.graphmaxheight = 10;
                for (var i = 0; i < res.data.length; i++) {
                    if (res.data[i].Quantity > $scope.graphmaxheight)
                        $scope.graphmaxheight = res.data[i].Quantity;

                    if (res.data[i].TargetQty > $scope.graphmaxheight)
                        $scope.graphmaxheight = res.data[i].TargetQty;

                }
                $scope.graphmaxwidth = ((res.data.length * 30) + 200) + "px";
                $scope.graphmaxheight = $scope.graphmaxheight + ($scope.graphmaxheight * .10);

                $scope.dataSourceProductionLineGraph = res.data;
                $("#graphProduction").ejDialog("setTitle", "Production Info Production Order#" + $scope.VPRDATA.ProductionOrderID);
                var eDialog = $("#graphProduction").data("ejDialog");
                eDialog.open();


            });


        } catch (e) {

        }
    }

    ////////////////////////FREEZE//////////////////////////////////


    $scope.NewFreezeDate = null;
    $scope.FreezeConfig = {};
    $scope.getFreezeConfig = function () {
        var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
        $scope.EntityId = DropDownEntityListObj.getSelectedValue();

        if (angular.isUndefinedOrNull($scope.EntityId)) {
            for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                if (angular.isUndefinedOrNull($scope.EntityId)) {
                    EntityId = + DropDownEntityListObj.popupListItems[i].Id;
                } else {
                    EntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                }
            }
        }

        $("#dialogFreezeDate").data("ejDialog").open();

        $http({
            method: 'GET',
            url: $scope.path + "FreezeConfig?entityid=" + $scope.EntityId

        }).then(function successCallback(response) {
            $scope.FreezeConfig = response.data[0];
        })
    }
    $scope.SaveFreezeConfig = function () {
        var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
        $scope.EntityId = DropDownEntityListObj.getSelectedValue();

        if (angular.isUndefinedOrNull($scope.EntityId)) {
            for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                if (angular.isUndefinedOrNull($scope.EntityId)) {
                    EntityId = + DropDownEntityListObj.popupListItems[i].Id;
                } else {
                    EntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                }
            }
        }

        $http({
            method: 'GET',
            url: $scope.path + "SaveFreezeConfig?entityid=" + $scope.EntityId + "&date=" + $scope.NewFreezeDate

        }).then(function successCallback(response) {
            if (response.data.Error == true)
                ShowResult(response.data.Message, 'failure');
            else {
                $("#dialogFreezeDate").data("ejDialog").close();
                $scope.OpenSimulatedData();
                ShowResult(response.data.Message, 'success');
            }

        })
    }

    ////////////////////////SNAPSHOT//////////////////////////////////
    $scope.snapshotmaster = { ID: null, EntityID: null, ProcessID: null, SnapshotName: null, SnapshotDesc: null };
    $scope.snapshotmasterNew = Object.assign({}, $scope.snapshotmaster);
    $scope.takeSnapshot = function () {
        try {

            $scope.snapshotmasterNew.EntityID = $scope.EntityId;
            $scope.snapshotmasterNew.ProcessID = $scope.PlanningTypeProcessId;



            if (angular.isUndefinedOrNull($scope.snapshotmasterNew.SnapshotName) == true)
                throw 'Please enter snapshot name';
            if (angular.isUndefinedOrNull($scope.snapshotmasterNew.SnapshotDesc) == true)
                throw 'Please enter snapshot description';

            $http({
                method: 'POST',
                data: { t1: $scope.snapshotmasterNew },
                url: $scope.path + "SaveSnapshot"

            }).then(function successCallback(response) {
                $scope.snapshotmasterNew = Object.assign({}, $scope.snapshotmaster);
                ShowResult(response.data.Message, 'success');

                var eDialog = $("#dialogSnapshot").data("ejDialog");
                eDialog.close();


                $scope.getSnapshotList();

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'dialogSnapshot1');
            };
        } catch (e) {

            ShowResult(e, 'failure', 'dialogSnapshot1');
        }
    }
    $scope.showSnapshotPanel = function () {
        $scope.snapshotmasterNew = Object.assign({}, $scope.snapshotmaster);
        var eDialog = $("#dialogSnapshot").data("ejDialog");
        eDialog.open();
    }

    $scope.snapshotList = [];
    $scope.getSnapshotList = function () {
        var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
        $scope.EntityId = DropDownEntityListObj.getSelectedValue();

        if (angular.isUndefinedOrNull($scope.EntityId)) {
            for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                if (angular.isUndefinedOrNull($scope.EntityId)) {
                    EntityId = + DropDownEntityListObj.popupListItems[i].Id;
                } else {
                    EntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                }
            }
        }

        $http({
            method: 'POST',
            url: $scope.path + "LoadNewSnapshotList?entityid=" + $scope.EntityId + "&processid=" + $scope.PlanningTypeProcessId

        }).then(function successCallback(response) {
            $scope.snapshotList = response.data.DATA;

            var eDialog = $("#dialogSnapshotSelect").data("ejDialog");
            eDialog.open();

        })
    }


    $scope.SnapshotRestoreArgs = null;
    $scope.PromtRestoreSnapshot = function (args) {
        $scope.SnapshotRestoreArgs = args;

        var eDialog = $("#dialogSnapshotRestoreConfirm").data("ejDialog");
        eDialog.open();
    }
    $scope.RestoreSnapshot = function () {
        $http({
            method: 'POST',
            url: $scope.path + "RestoreSnapshot?MasterId=" + $scope.SnapshotRestoreArgs.data.ID

        }).then(function successCallback(response) {
            var eDialog = $("#dialogSnapshotRestoreConfirm").data("ejDialog");
            eDialog.close();
            ShowResult(response.data.Message, 'success');

            $scope.OpenSimulatedData();

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'dialogSnapshotRestoreConfirm');
        };
    }

    $scope.appointmentsSnapshot = [];
    $scope.loadSnapshot = function (args) {
        try {
            var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
            $scope.EntityId = DropDownEntityListObj.getSelectedValue();

            if (angular.isUndefinedOrNull($scope.EntityId)) {
                for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                    if (angular.isUndefinedOrNull($scope.EntityId)) {
                        EntityId = + DropDownEntityListObj.popupListItems[i].Id;
                    } else {
                        EntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                    }
                }
            }

            $http({
                method: 'POST',
                url: $scope.path + "LoadSnapshot?id=" + args.data.ID + "&entityid=" + $scope.EntityId + "&processid=" + $scope.PlanningTypeProcessId
            }).then(function successCallback(res) {

                if (res.data.DATA.length > 0) {
                    var eDialog = $("#dialogSnapshotSelect").data("ejDialog");
                    eDialog.close();

                    //$scope.OpenSimulatedData();

                    $scope.resourcedata2 = {
                        dataSource: res.data.GROUPDATA,
                        text: "text", id: "id", groupId: "groupId", color: "color"
                    };
                    for (var i = 0; i < res.data.DATA.length; i++) {
                        res.data.DATA[i].AllDay = true;
                        res.data.DATA[i].Recurrence = false;
                    }
                    $scope.workweek = res.data.WORKDAYDATA;
                    $scope.appointmentsSnapshot = angular.copy(res.data.DATA);

                    var schObj = $("#ResourceGroupScheduleSnapshot").data("ejSchedule");
                    schObj.refresh(); // To refresh the Schedule control within the client side event
                    schObj.refreshAppointments();

                }
            });
        } catch (e) {

        }
        //var schObj = $("#ResourceGroupSchedule").data("ejSchedule");
        //var appointments = schObj.getAppointments();


    }

    $scope.GetSnapshotExcel = function (args) {

        try {
            var file_src = 'OrderManagements/productionOrderReports/OS2Snapshotxls?entityid=' + $scope.EntityId + '&SnapshotId=' + args.data.ID
            $rootScope.report(file_src);

        } catch (e) {

        }
    }
    $scope.DownloadOS2 = function (args) {

        try {
            var file_src = 'OrderManagements/productionOrderReports/OS2xls?entityid=' + $scope.EntityId;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }
    $scope.OrderMaster = function () {

        try {
            var file_src = 'OrderManagements/productionOrderReports/OS3xls?entityid=' + $scope.EntityId
            $rootScope.report(file_src);

        } catch (e) {

        }
    }
    $scope.LineBookingStatus = function () {

        try {
            var file_src = 'OrderManagements/productionOrderReports/LineBookingStatus?entityid=' + $scope.EntityId
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.LadderPlanStatus = function () {
        var reportFormat = "Excel";
        try {
            var file_src = 'OrderManagements/productionOrderReports/LadderPlanStatus?reportFormat=' + reportFormat + '&entityid=' + $scope.EntityId;
            $rootScope.report(file_src);
             
        } catch (e) {

        }
    }
    ////////////////////////CONTINUOUS SNAPSHOT//////////////////////////////////

    $scope.snapshotmaster2 = { ID: null, EntityID: null, ProcessID: null, SnapshotName: null, SnapshotDesc: null, SnapshotTakenBy: null };
    $scope.snapshotmasterNew2 = Object.assign({}, $scope.snapshotmaster2);

    $scope.SaveSnapshot2 = function () {
        try {

            $scope.snapshotmasterNew2.EntityID = $scope.EntityId;
            $scope.snapshotmasterNew2.ProcessID = $scope.PlanningTypeProcessId;


            if (angular.isUndefinedOrNull($scope.snapshotmasterNew2.SnapshotName) == true)
                throw 'Please enter snapshot name';
            if (angular.isUndefinedOrNull($scope.snapshotmasterNew2.SnapshotDesc) == true)
                throw 'Please enter snapshot description';


            $http({
                method: 'POST',
                data: { t1: $scope.snapshotmasterNew2 },
                url: $scope.path + "SaveSnapshot2"

            }).then(function successCallback(response) {
                ShowResult(response.data.Message, 'success');


                var eDialog = $("#dialogSnapshot2").data("ejDialog");
                eDialog.close();

                $scope.snapshotmasterNew2 = Object.assign({}, $scope.snapshotmaster2);

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {

            ShowResult(response.data.Message, 'failure');
        }
    }


    ////////////////////////CONTINUOUS SNAPSHOT//////////////////////////////////


    $scope.showSnapshotPanel2 = function () {
        $scope.snapshotmasterNew = Object.assign({}, $scope.snapshotmaster);
        var eDialog = $("#dialogSnapshot2").data("ejDialog");
        eDialog.open();
    }



    ////////////////////////CONTINUOUS SNAPSHOT//////////////////////////////////

    ////////////////////////////////////////////////REPORT//////////////////////////////////////////////////
    $scope.hrefss = 'about:blank';
    $scope.getos2 = function () {
        //location.href = 'OrderManagements/productionOrderSchedulingParametersType1/OS2xls?entityid=sdsd&processid=process';
        $scope.hrefss = 'about:blank';
        $scope.hrefss = 'OrderManagements/productionOrderSchedulingParametersType1/OS2xls?entityid=sdsd&processid=process';
        //
    }

    $scope.oncreatetab = function () {
        try {
            $("#workCenterPopUp").ejDialog({
                title: "Work Center",
                enableModal: true,
                showOnInit: false,
                target: "#entrycontainer"
            });

            $scope.actionCompleteSelected(); LoadFilterSQL
        } catch (e) {

        }
    }


    $scope.WorkAllCenterPlanList = [];
    $scope.WorkCenterPlanList = [];
    $scope.SelectedWorlcenterForSummary = {};
    $scope.GetAllWorkcenterWisePlanningSummary = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.path + "GetWorkcenterWisePlanningSummary?EntityId=" + $scope.EntityId

            }).then(function successCallback(response) {
                $scope.WorkAllCenterPlanList = response.data;
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {

            ShowResult(response.data.Message, 'failure');
        }

    }
    $scope.GetSingleWorkcenterWisePlanningSummary = function (data) {
        try {
            $scope.SelectedWorlcenterForSummary = data;
            $("#dialogWorkCenterWisePlanning").ejDialog("setTitle", "Plan Summary for Workcenter: [" + data.WorkCenterCode + '-' + data.WorkCenter + "]");

            $scope.openPopup('dialogWorkCenterWisePlanning');
            $http({
                method: 'POST',
                url: $scope.path + "GetSingleWorkcenterWisePlanningSummary?WorkCenterId=" + data.Id

            }).then(function successCallback(response) {
                $scope.WorkCenterPlanList = response.data;
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {

            ShowResult(response.data.Message, 'failure');
        }

    }

    $scope.EditProductionPlanningData = function (args) {


        $scope.VROWDATA.ProductionOrderId = args.data.ProductionOrderID;
        $scope.GetProductionPlanningParametersData('dialogProductionOrderParameters1', 'entrypop1');
    }
    $scope.EditProductionPlanningDataSameDay = function (args) {


        $scope.VROWDATA.ProductionOrderId = args.data.ProductionOrderID;
        $scope.GetProductionPlanningParametersData('dialogProductionOrderParameters', 'entrypop');
    }

    // The functions for the priority Update
    $scope.fileData = [];
    $scope.GetSample = function () {
        var reportFormat = "Excel";

        if (angular.isUndefinedOrNull($scope.EntityId)) {
            ShowResult("Please First Select the Entity!");
            throw ("Invalid");
        }

        try {
            window.open('OrderManagements/productionOrderSchedulingParametersType1/GetSampleReports?reportFormat=' + reportFormat+ '&Entity=' + $scope.EntityId,'_blank');

        } catch (e) {

        }
    }

    $("#uploadFile").change(function () {
        $scope.fileData = this.files[0];
    });
    $scope.ExcelUploadData = [];
    //IMporting The Data From the Excel File

    $scope.ModelNew = {
        FileName: null
    }


    $scope.ImportData = function () {
        try {
            $scope.ExcelUploadData = [];
            $scope.msg = "";
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.fileData.length == 0) {

                throw ("Please Select A File!!");
            }


            var fileData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.fileData)) {
                $scope.ModelNew.FileName = $scope.fileData.name;
            }

            $http({
                method: 'POST',
                url: $scope.path + 'ImportData',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    fileData.append("modelNew", angular.toJson(data.modelNew));
                    if (baseService.isUndefinedOrNull($scope.fileData) === false) {
                        fileData.append('file', data.file);

                    }
                    return fileData;
                },
                data: { 'modelNew': $scope.ModelNew, 'file': $scope.fileData }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");

                }

                else {
                    try {
                        $scope.ExcelUploadData = response.data;
                    }

                    catch (e) {

                        ShowResult(e, "failure");
                    }

                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    //Save the File Data
    $scope.saveFileList = function () {




        $http({
            method: 'POST',
            url: $scope.path + 'SaveFileList',
            data: { 'data': $scope.ExcelUploadData }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                try {
                    ShowResult(response.data.Message, 'success')
                }
                catch (e) {

                    ShowResult(e, "failure");
                }
            }
        });
    }
}