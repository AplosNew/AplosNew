'use strict';
ProductionSummaryReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function ProductionSummaryReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Production Report';
    $scope.path = "Productions/ProductionSummary/";


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

    $scope.entityListData = [];
    $scope.getAllEntitiesList = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityListData = response.data;
            //$scope.loadProcessList(EntityId);
        });
    }
    $scope.getAllEntitiesList();

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
            var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
            var EntityId = DropDownEntityListObj.getSelectedValue();
            //$scope.loadProcessList(EntityId);
            //$scope.getProcessDataList(EntityId);
        });
    }
    $scope.getAllEntities();



    $scope.processDataList = [];
    $scope.getProcessDataList = function () {
        var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
        var entityid = DropDownEntityListObj.getSelectedValue();
        $http({
            method: 'POST',
            url: "Productions/Productionsummary/GetProcessData?entityid=" + entityid
        }).then(function successCallback(response) {
            $scope.processDataList = response.data;
            var index = 0;
            $('#process').ejDropDownList(
                {
                    dataSource: $scope.processDataList,
                    fields: { text: "Text", value: "Value" },
                    selectedIndex: index, showCheckBox: true, multiSelectMode: ej.MultiSelectMode.VisualMode
                    , width: 250
                });
        });
    }


    $scope.fromDate = '';
    $scope.toDate = '';
    $scope.EntityId = null;
    $scope.ProcessId = null;
    $scope.ProcessWiseReport = function () {
        try {
            if (angular.isUndefinedOrNull($scope.fromDate)) {
                throw "Select From Date.";
            }

            if (angular.isUndefinedOrNull($scope.toDate)) {
                throw "Select To Date.";
            }

            if (new Date($scope.fromDate) > new Date($scope.toDate)) {
                throw "From date can not be greater than To date.";
            }

            var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
            var EntityId = DropDownEntityListObj.getSelectedValue();

            var DropDownProcessListObj = $("#process").data("ejDropDownList");
            var ProcessId = DropDownProcessListObj.getSelectedValue();

            if (angular.isUndefinedOrNull(EntityId)) {
                for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                    if (angular.isUndefinedOrNull(EntityId)) {
                        EntityId =  + DropDownEntityListObj.popupListItems[i].Id;
                    } else {
                        EntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                    }
                }
            }

            $scope.fileName = "ProductionOrderReportProcessWise.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetProcessWiseOrderReport",
                data: { 'fromDate': $scope.fromDate, 'toDate': $scope.toDate, 'EntityId': EntityId, 'ProcessId': ProcessId },
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

   

    $scope.fromDate2 = '';
    $scope.toDate2 = '';
    $scope.EntityId2 = null;
    $scope.ProcessId2 = null;
    $scope.ProductionShiftId2 = null;
    $scope.PerameterWiseReport = function () {
        try {
            if (angular.isUndefinedOrNull($scope.fromDate2)) {
                throw "Select From Date.";
            }

            if (angular.isUndefinedOrNull($scope.toDate2)) {
                throw "Select To Date.";
            }

            if (new Date($scope.fromDate) > new Date($scope.toDate2)) {
                throw "From date can not be greater than To date.";
            }

            if (angular.isUndefinedOrNull($scope.EntityId2)) {
                throw "Select Entity.";
            }


            if (angular.isUndefinedOrNull($scope.ProcessId2)) {
                throw "Select Process.";
            }

            $scope.fileName = "ProductionOrderReportParameterWise.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetPerametreWiseOrderReport",
                data: { 'fromDate': $scope.fromDate2, 'toDate': $scope.toDate2, 'EntityId': $scope.EntityId2, 'ProcessId': $scope.ProcessId2, 'ShiftId': $scope.ProductionShiftId2 },
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

    $scope.fromDate3 = '';
    $scope.toDate3 = '';
    $scope.EntityId3 = null;
    $scope.ProcessId3 = null;
    $scope.ItemWiseReport = function () {
        try {
            if (angular.isUndefinedOrNull($scope.fromDate3)) {
                throw "Select From Date.";
            }

            if (angular.isUndefinedOrNull($scope.toDate3)) {
                throw "Select To Date.";
            }

            if (new Date($scope.fromDate3) > new Date($scope.toDate3)) {
                throw "From date can not be greater than To date.";
            }


            $scope.fileName = "ProductionOrderReportItemWise.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetItemWiseOrderReport",
                data: { 'fromDate': $scope.fromDate3, 'toDate': $scope.toDate3, 'EntityId': $scope.EntityId3, 'ProcessId': $scope.ProcessId3 },
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

    $scope.fromDate4 = '';
    $scope.toDate4 = '';
    $scope.EntityId4 = null;
    $scope.ProcessId4 = null;
    $scope.SOWiseReport = function () {
        try {
            if (angular.isUndefinedOrNull($scope.fromDate4)) {
                throw "Select From Date.";
            }

            if (angular.isUndefinedOrNull($scope.toDate4)) {
                throw "Select To Date.";
            }

            if (new Date($scope.fromDate4) > new Date($scope.toDate4)) {
                throw "From date can not be greater than To date.";
            }


            $scope.fileName = "ProductionOrderReportSOWise.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetSOWiseOrderReport",
                data: { 'fromDate': $scope.fromDate4, 'toDate': $scope.toDate4, 'EntityId': $scope.EntityId4, 'ProcessId': $scope.ProcessId4 },
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
        });
    };

   

    $scope.shiftList = [];
    $scope.GetShiftList = function () {
        $scope.shiftList = [];
        $http.get('Productions/Productionsummary/GetAllShiftList')
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.shiftList = response.data;
                }
            });
    }
    $scope.GetShiftList();

}



