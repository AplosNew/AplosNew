'use strict';
VehicleReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function VehicleReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Vehicle Report"
    $scope.path = 'HumanResource/VehicleMovementMaster/'; 
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';


    //$scope.InWardMaterialList = [];
    //$scope.GetInWardMaterialData = function () {
    //    if ($scope.fromDate === null || $scope.fromDate === "") {
    //        ShowResult('Select From Date', 'failure');
    //        return false;
    //    }
    //    else if ($scope.toDate === null || $scope.toDate === "") {
    //        ShowResult('Select To Date', 'failure');
    //        return false;
    //    }
    //    $http({
    //        method: 'POST',
    //        url: 'Materials/MaterialLedger/GetInWardMaterialData',
    //        data: {
    //            fromDate: $scope.fromDate,
    //            toDate: $scope.toDate
    //        },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.InWardMaterialList = response.data;
    //    });
    //};

    $scope.ReportDataList = [];
    $scope.GetReportData = function () {
        if ($scope.fromDate === null || $scope.fromDate === "") {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        else if ($scope.toDate === null || $scope.toDate === "") {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        $http({
            method: 'POST',
            url: $scope.path + "CompleteVehicleMovementCycle",
            data: {fromDate: $scope.fromDate,toDate: $scope.toDate},
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