'use strict';
VehicleReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function VehicleReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Vehicle In & Out"
    $scope.path = 'HumanResource/VehicleMovementMaster/';
    $scope.saveVehicleReqUrl = $scope.path + 'UpdateVehicleAllocation';
    $scope.saveVehicleMovementReqUrl = $scope.path + 'UpdateVehicleMovement';
    $scope.ActionIn = "Save";
    $scope.ActionOut = "Save";
    $scope.Action = 'Update'; 
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
   
    $scope.ReportDataList = [];
    $scope.GetReportData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "CompleteVehicleMovementCycle",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ReportDataList = response.data;

        });
    }
    $scope.GetReportData();

    $scope.XlsVehicleReport = function () {
        var dataList = [];
        var g = $("#GridEdit").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ReportDataList;
        }
        $scope.fileName = 'VehicleReport.xlsx';

        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleReport",
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

}