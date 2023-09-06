'use strict';
StocksAgeingReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function StocksAgeingReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Finished Goods Stock Ageing ';
    $scope.path = 'Productions/StocksAgeingReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    // Variables
    //$scope.FromDate = null;
    //$scope.ToDate = null;
    $scope.masterData = [];
    //Operations
    $scope.getData = function () {
        //if (angular.isUndefinedOrNull($scope.FromDate) == true) {
        //    ShowResult("Please select From Date");
        //    throw ("Please select From Date");
        //}
        //if (angular.isUndefinedOrNull($scope.ToDate) == true) {
        //    ShowResult("Please select To Date");
        //    throw ("Please select To Date");
        //}

        $http({
            method: 'POST',
            url: $scope.path + 'getData',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.masterData = [];
            $scope.masterData = response.data;

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }


    $scope.downloadReport = function () { 
        var dataList = [];
        var g = $("#masterGrid").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.masterData;
        }
        $scope.fileName = 'FinishedStockReport.xlsx';
        $http({
            method: 'POST',
            url: $scope.path + "getReport",
            data: { 'data': dataList,'reportFileName': $scope.fileName},
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