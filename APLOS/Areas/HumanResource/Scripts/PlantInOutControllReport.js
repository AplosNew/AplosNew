'use strict';
PlantInOutControllReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function PlantInOutControllReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Plant In Out Report';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.path = 'HumanResource/PlantInOutControllReport/';

    $scope.PlantInOutList = []
    $scope.GetPlantInOutGridData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPlantInOutGridData",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PlantInOutList = response.data;

        });
    }
    $scope.GetPlantInOutGridData();

    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.summaryfileName = "PlantInOut.xlsx";

    $scope.XlsDailyAttendanceReport = function () {
        var dataList = [];
        var g = $("#GridEdit").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {

            dataList = $scope.PlantInOutList;
        }
        $scope.fileName = 'PlantInOut.xlsx';
        $http({
            method: "POST",

            // url: $scope.path + 'GetDailyAttendanceStatusXls',
            url: $scope.exportgriddataUrl,
            data: {              
                'data': dataList,
                'reportFileName': $scope.fileName,
            },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //$rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.summaryfileName);
                    
                    $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };
}