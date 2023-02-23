'use strict';
InWardMaterialController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function InWardMaterialController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
    $rootScope.title = "In Ward Material";
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.path = 'Materials/MaterialLedger/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    //$scope.fromDate = $filter('dateFiltering')(Date.now());
    $scope.toDate = $filter('dateFiltering')(Date.now());

    $scope.InWardMaterialList = [];
    $scope.GetInWardMaterialData = function () {
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
            url: 'Materials/MaterialLedger/GetInWardMaterialData',
            data: {
                fromDate: $scope.fromDate,
                toDate: $scope.toDate
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.InWardMaterialList = response.data;
        });
    };


    $scope.InWardMaterialReportExcel = function () {
        if (baseService.isUndefinedOrNull($scope.fromDate)) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.toDate)) {
            ShowResult('Select To Date', 'failure');
            return false;
        }

        var dataList = [];
        var g = $("#GridPrint").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.InWardMaterialList;
        }
        try {
            $scope.fileName = 'In Ward Material Report.xlsx';
            $http({
                method: 'POST',
                url: $scope.path + "InWardMaterialDataXls",
                data: {
                    'data': dataList,
                    'reportFileName': $scope.fileName,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

        } catch (e) {

        }
    }

    //End In ward material

}
 

