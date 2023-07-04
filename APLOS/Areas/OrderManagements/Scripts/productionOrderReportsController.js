'use strict';
productionOrderReportsController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", 'signalR'];
function productionOrderReportsController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, signalR) {
    $rootScope.title = "production Order Reports";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.baseProcess = { Id: null, UserName: null };
    $scope.modelList = [];
    $scope.productionMaterialList = [];
    $scope.prdProcessSetList = [];
    $scope.productionEntityList = [];
    $scope.productionWorkCenterList = [];
    $scope.productWorkCenterList = [];

    $scope.path = 'OrderManagements/productionOrderReports/';
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    var last_date = new Date(y, m + 1, 0);

    $scope.FromDate = $filter('dateFiltering')(firstDay);
    $scope.ToDate = $filter('dateFiltering')(last_date);
    $scope.EntityId = '';
    $scope.prdProcessSetList = [];
    $scope.ProcessID = '';

    $scope.Plants = [];
    $scope.Entities = [];
    $scope.SinglePlantEntity = [];
    $scope.getProcessData = function () {
        //$scope.ProcessID = '';

        //$http({
        //    method: 'GET',
        //    url: "OrderManagements/ProductionCalendar/GetProcessForPlanning?entityid=" + $scope.EntityId
        //}).then(function successCallback(response) {
        //    $scope.prdProcessSetList = response.data;

        //});


        $http({
            method: 'GET',
            url: $scope.path + "GetPlantAndProcess"
        }).then(function successCallback(response) {
            $scope.Plants = response.data.Plants;
            $scope.Entities = response.data.Entities;
        });

        //$http({
        //    method: 'GET',
        //    url: $scope.path + "GetDateRange?entityid=" + $scope.EntityId
        //}).then(function successCallback(response) {
        //    $scope.FromDate = response.data.FromDate;
        //    $scope.ToDate = response.data.ToDate;

        //});

    };
    $scope.getProcessData();

    $scope.showEntityPopUp = function (Id, plantName) {
        $scope.SinglePlantEntity = [];
        for (var i = 0; i < $scope.Entities.length; i++) {
            if ($scope.Entities[i].PlantId == Id) {
                $scope.SinglePlantEntity.push($scope.Entities[i]);
            }
        }



        $("#dialogSelectEntity").ejDialog("setTitle", "Plant: " + plantName);
        var eDialog = $("#dialogSelectEntity").data("ejDialog");
        eDialog.open();
    }
    $scope.SelectPlant = function (Id) {
        //first get the selected plant

        for (var i = 0; i < $scope.Plants.length; i++) {
            if ($scope.Plants[i].Id == Id) {

                if ($scope.Plants[i].isChecked == true) {

                    for (var E = 0; E < $scope.Entities.length; E++) {
                        if ($scope.Entities[E].PlantId == Id) {
                            $scope.Entities[E].isChecked = true;
                        }
                    }
                }
                else {

                    for (var E = 0; E < $scope.Entities.length; E++) {
                        if ($scope.Entities[E].PlantId == Id) {
                            $scope.Entities[E].isChecked = false;
                        }
                    }
                }
            }
        }


        $scope.ConstructSelectedEntities(Id);
    }


    $scope.ConstructSelectedEntities = function (plantId) {


        for (var i = 0; i < $scope.SinglePlantEntity.length; i++) {
            for (var j = 0; j < $scope.Entities.length; j++) {
                if ($scope.SinglePlantEntity[i].Id == $scope.Entities[j].Id) {

                    $scope.Entities[j].isChecked = $scope.SinglePlantEntity[i].isChecked;
                }
            }
        }

        $scope.EntityId = "''";
        var s = "";
        var allSelected = true;
        for (var i = 0; i < $scope.Entities.length; i++) {
            if ($scope.Entities[i].PlantId == plantId) {

                if ($scope.Entities[i].isChecked == true) {
                    if (s == "") {
                        s = $scope.Entities[i].Entity;
                    }
                    else {
                        s += ',' + $scope.Entities[i].Entity;
                    }
                }
                else {
                    allSelected = false;

                }
            }

            if ($scope.Entities[i].isChecked == true) {
                if (s == "") {
                    $scope.EntityId += ",'" + $scope.Entities[i].Id + "'";
                }
                else {
                    $scope.EntityId += ",'" + $scope.Entities[i].Id + "'";
                }
            }

        }


        for (var i = 0; i < $scope.Plants.length; i++) {
            if ($scope.Plants[i].Id == plantId) {
                if (allSelected == false) {
                    $scope.Plants[i].isChecked = false;
                    $scope.Plants[i].SelectedEntities = s;
                }
                else {
                    $scope.Plants[i].isChecked = true;
                    $scope.Plants[i].SelectedEntities = 'All entities';
                }
            }
        }

        var gridObj = $("#gridSelectPlant").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();

        var gridObj = $("#gridSelectEntity").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    }

    $scope.RunProductionTargetScheduler = function () {
        $http({
            method: 'POST',
            data: {},
            url: "OrderManagements/productionOrderReports/RunProductionTargetScheduler"
        }).then(function successCallback(response) {

            if (response.data.Error == true)
                ShowResult(response.data.Message, 'failure');
            else
                ShowResult(response.data.Message, 'success');
        });

    };
    $scope.entityList = null;
    $scope.getAllEntities = function () {

        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetAllEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.getAllEntities();

    $scope.ProductionStatusId = 'All';
    $scope.productionStatusList = [];
    cboService.getProductionStatusCboByGroup(function (result) {
        //result.splice(0, 0, { Text: "All", Value: "All" });
        //var kk = [];
        //for (var i = 0; i < result.length; i++) {
        //    kk.push({ Text: result[i].Text, Value: result[i].Value, Checked: true });
        //}
        $scope.productionStatusList = result;
    });

    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    ////////////////////////////////////////////////REPORT//////////////////////////////////////////////////
    $scope.getMasterOrder = function (status) {

        try {
            var file_src = 'OrderManagements/productionOrderReports/MasterOrder?entityid=' + $scope.EntityId + '&status=' + status
            $rootScope.report(file_src);

        } catch (e) {

        }
    }
    $scope.getos1 = function () {

        try {
            var DropDownOSList = $("#selOS").data("ejDropDownList");
            var osLists = DropDownOSList.getSelectedValue();
            var olist = DropDownOSList._checkedValues;
            if (angular.isUndefinedOrNull(osLists)) {
                throw "Select Production Status.";
            }

            var file_src = 'OrderManagements/productionOrderReports/OS1xls?entityid=' + $scope.EntityId + "&fromDate=" + $scope.FromDate + "&toDate=" + $scope.ToDate + "&productionStatusList=" + olist;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.Bulletin = function () {

        try {

            var file_src = 'OrderManagements/productionOrderReports/BulletinReport?entityid=' + $scope.EntityId
            $rootScope.report(file_src);

        } catch (e) {

        }
    }


    $scope.getos2 = function () {

        try {
            var file_src = 'OrderManagements/productionOrderReports/OS2xls?entityid=' + $scope.EntityId
            $rootScope.report(file_src);

        } catch (e) {

        }
    }
    $scope.getos3 = function () {

        try {
            var file_src = 'OrderManagements/productionOrderReports/OS3xls?entityid=' + $scope.EntityId
            $rootScope.report(file_src);

        } catch (e) {

        }
    }
    $scope.getos4 = function () {

        try {


            var file_src = 'OrderManagements/productionOrderReports/OS4xls?entityid=' + $scope.EntityId + "&fromDate=" + $scope.FromDate + "&toDate=" + $scope.ToDate;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }
    $scope.ProductionData = function () {

        try {


            var file_src = 'OrderManagements/productionOrderReports/ProductionDataXls?entityid=' + $scope.EntityId + "&fromDate=" + $scope.FromDate + "&toDate=" + $scope.ToDate;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.Snapshot2Data = function () {

        try {
            var file_src = 'OrderManagements/productionOrderReports/Snapshot2DataReportXls?fromDate='+ $scope.FromDate + "&toDate=" + $scope.ToDate;
            $rootScope.report(file_src);
        } catch (e) {

        }
    }

    $scope.getos5 = function () {

        try {


            var file_src = 'OrderManagements/productionOrderReports/OS5xls?entityid=' + $scope.EntityId + "&fromDate=" + $scope.FromDate + "&toDate=" + $scope.ToDate;
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


    $scope.dummydata = [];
    $scope.PunchInfoDummy = function () {

        try {

            $http({
                method: 'GET',
                url: 'OrderManagements/productionOrderReports/getOrderMasterDummy'
            }).then(function successCallback(response) {
                $scope.dummydata = response.data;
            });
        } catch (e) {

        }
    }
    $scope.PunchInfoDummy();

    $scope.gg = function (args) {
        args.data.SalesOrderId = args.data.Buyer.length;
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshTemplate();

        //gridObj.refreshContent(); // Refreshes the grid contents only

    }
    $scope.getProductionInfoWithWIP = function () {
        try {
            var DropDownOSList = $("#selOS").data("ejDropDownList");
            var osLists = DropDownOSList.getSelectedValue();
        //    var olist = DropDownOSList._checkedValues;
            if (angular.isUndefinedOrNull(osLists)) {
                throw "Select Production Status.";
            }

            var file_src = 'OrderManagements/productionOrderReports/ProductionReport?entityid=' + $scope.EntityId + "&fromDate=" + $scope.FromDate + "&todate=" + $scope.ToDate + "&ProductionStatus=" + osLists;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }


    }

}