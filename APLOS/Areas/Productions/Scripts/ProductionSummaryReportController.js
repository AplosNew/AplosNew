'use strict';
ProductionSummaryReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','$window'];
function ProductionSummaryReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Production Report';
    $scope.path = "Productions/ProductionSummary/";

    $scope.fromDate = '';
    $scope.toDate = '';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportJson';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.PlantList = [];
    $scope.getPlant = function () {
        $http({
            method: 'GET',
            url: "humanresource/payrollReports/GetPlantList",
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
            var index = 0;
            for (var i = 0; i < $scope.PlantList.length; i++) {
                if ($scope.PlantList[i].PlantId == $window.plantId) {
                    index = i;
                }
            }

            $('#PlantList').ejDropDownList(
                {
                    dataSource: $scope.PlantList,
                    fields: { text: "PlantName", value: "PlantId" },
                    selectedIndex: index, showCheckBox: true, multiSelectMode: ej.MultiSelectMode.VisualMode
                    , width: 250
                });
        });
    }
    $scope.getPlant();

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            var index = 0;
            $('#entityList').ejDropDownList(
                {
                    dataSource: $scope.entityList,
                    fields: { text: "UserName", value: "Id" },
                    selectedIndex: index, showCheckBox: true, multiSelectMode: ej.MultiSelectMode.VisualMode
                    , width: 250
                });
            $scope.loadProcessList($scope.EntityId);
        });
    }
    $scope.getAllEntities();

    $scope.processDataList = [];
    $scope.getProcessDataList = function () {
        $http({
            method: 'POST',
            url: "Productions/Productionsummary/GetShiftList/GetProcessData"
        }).then(function successCallback(response) {
            $scope.processDataList = response.data;
        });
    }
    $scope.getProcessDataList();


    $scope.EntityId = null;
    $scope.ProcessId = null;
    $scope.ProcessWiseReport = function () {
        try {
            if (angular.isUndefinedOrNull($scope.fromDate)) {
                throw "Select From Date";
            }

            if (angular.isUndefinedOrNull($scope.toDate)) {
                throw "Select To Date";
            }

            if (new Date($scope.fromDate) > new Date($scope.toDate)) {
                throw "From date can not be greater than To date.";
            }

            //var DropDownListObj = $("#PlantList").data("ejDropDownList");
            //var PlantId = DropDownListObj.getSelectedValue();

            //var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
            //var EntityId = DropDownEntityListObj.getSelectedValue();

            $scope.fileName = "ProductionOrderReport.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetProcessWiseOrderReport",
                //data: { 'parameters': $scope.parameters, 'fromDate': $scope.fromDate, 'toDate': $scope.toDate, 'dateType': $rootScope.dateCgroup },
                data: { 'fromDate': $scope.fromDate, 'toDate': $scope.toDate, 'EntityId': $scope.EntityId, 'ProcessId': $scope.ProcessId},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath

                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.PerameterWiseReport = function () {
        try {
            if (angular.isUndefinedOrNull($scope.fromDate)) {
                throw "Select From Date";
            }

            if (angular.isUndefinedOrNull($scope.toDate)) {
                throw "Select To Date";
            }

            if (new Date($scope.fromDate) > new Date($scope.toDate)) {
                throw "From date can not be greater than To date.";
            }

            //var DropDownListObj = $("#PlantList").data("ejDropDownList");
            //var PlantId = DropDownListObj.getSelectedValue();

            //var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
            //var EntityId = DropDownEntityListObj.getSelectedValue();

            $scope.fileName = "ProductionOrderReport.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetPerametreWiseOrderReport",
                data: { 'fromDate': $scope.fromDate, 'toDate': $scope.toDate, 'PlantId': $scope.PlantId, 'EntityId': $scope.EntityId},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath

                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
            if (baseService.arrayLength(result) === 1) {
                $scope.productionSummaryNew.ProcessId = $scope.processList[0].Value;
                $scope.getProdLevel();
                //default
                $scope.loadWC($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.EntityId, $scope.productionSummaryNew.ProductionShiftId);
            }
        });
    };

    $scope.getProdLevel = function () {
        try {
            $scope.PQEnable = false;

            $scope.IsFirst = $.grep($scope.processList, function (item) {
                return item.Value === $scope.ProcessId;
            })[0].IsFirst;

            $scope.ProductionBookingLevel = $.grep($scope.processList, function (item) {
                return item.Value === $scope.ProcessId;
            })[0].ProductionBookingLevel;

            $scope.LotNumberCapture = $.grep($scope.processList, function (item) {
                return item.Value === $scope.ProcessId;
            })[0].LotNumberCapture;

            $scope.LotNumberMandatory = $.grep($scope.processList, function (item) {
                return item.Value === $scope.ProcessId;
            })[0].LotNumberMandatory;

            $scope.IsSKU1 = $.grep($scope.processList, function (item) {
                return item.Value === $scope.ProcessId;
            })[0].IsSKU1;

            $scope.IsSKU2 = $.grep($scope.processList, function (item) {
                return item.Value === $scope.ProcessId;
            })[0].IsSKU2;

            $scope.IsSKU3 = $.grep($scope.processList, function (item) {
                return item.Value === $scope.ProcessId;
            })[0].IsSKU3;

            $scope.IsParameterBased = $.grep($scope.processList, function (item) {
                return item.Value === $scope.ProcessId;
            })[0].IsParameterBased;

            if ($scope.ProductionBookingLevel === 'ProductionOrder') {
                $scope.ProductionLevel = 'Production Order';
                $scope.disGo = false;
            }
            else if ($scope.ProductionBookingLevel === 'SalesOrder') {
                $scope.ProductionLevel = 'Sales Order';
                $scope.disGo = false;
            }
            else if ($scope.ProductionBookingLevel === 'MasterOrderItem') {
                $scope.ProductionLevel = 'Master Order Item';
                $scope.disGo = false;
            }
            else if ($scope.ProductionBookingLevel === 'ProductCode') {
                $scope.ProductionLevel = 'Product Code';
                $scope.disGo = false;
            }
            else {
                $scope.disGo = true;
                $scope.PQEnable = true;
                throw 'Production Booking Level is not defined for selected process.';
            }

            if ($scope.IsSKU1 === true || $scope.IsSKU2 === true || $scope.IsSKU2 === true || $scope.IsParameterBased == true) {
                $scope.PQEnable = true;
                $scope.disGo = false;
            }


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.shiftList = [];
    $scope.GetShiftList = function () {
        $scope.shiftList = [];
        $http.get('Productions/Productionsummary/GetShiftList?processId=' + $scope.ProcessId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.shiftList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.ProductionShiftId = $scope.shiftList[0].Value;
                    }
                }
            });
    }

}



   