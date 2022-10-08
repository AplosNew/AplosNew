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
        });
    }
    $scope.getAllEntities();


    $scope.Report = function () {
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

            var DropDownListObj = $("#PlantList").data("ejDropDownList");
            var PlantId = DropDownListObj.getSelectedValue();

            var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
            var EntityId = DropDownEntityListObj.getSelectedValue();

            $scope.fileName = "ProductionOrderReport.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetOrderReport",
                //data: { 'parameters': $scope.parameters, 'fromDate': $scope.fromDate, 'toDate': $scope.toDate, 'dateType': $rootScope.dateCgroup },
                data: { 'fromDate': $scope.fromDate, 'toDate': $scope.toDate, 'PlantId': PlantId, 'EntityId': EntityId},
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

    $scope.ProductionReport = function () {
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

            var DropDownListObj = $("#PlantList").data("ejDropDownList");
            var PlantId = DropDownListObj.getSelectedValue();

            var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
            var EntityId = DropDownEntityListObj.getSelectedValue();

            $scope.fileName = "ProductionOrderReport.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetProductionReport",
                //data: { 'parameters': $scope.parameters, 'fromDate': $scope.fromDate, 'toDate': $scope.toDate, 'dateType': $rootScope.dateCgroup },
                data: { 'fromDate': $scope.fromDate, 'toDate': $scope.toDate, 'PlantId': PlantId, 'EntityId': EntityId },
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


}



   