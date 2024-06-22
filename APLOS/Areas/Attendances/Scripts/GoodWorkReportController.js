'use strict';
GoodWorkReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function GoodWorkReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Good Work Report'; 
   
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.FromDate = null;
    $scope.ToDate = null;

    $scope.GWlist = [];
    $scope.GetGWData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "Select From Date";
            }
            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "Select To Date";
            }
            $http({
                method: 'Get',
                url: "Attendances/GoodWork/GetGWDataInDateRange?fromDate=" + $scope.FromDate + '&toDate=' + $scope.ToDate,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.GWlist = response.data;
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }



    $scope.XGoodWorkReport = function () {
        try {
            $scope.fileName = "GoodWorkReport.xlsx";
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "Select From Date";
            }
            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "Select To Date";
            }
            $http({
                method: 'POST',
                url: "Attendances/GoodWork/GetGoodWorkReportInDateRange",
                data: { 'reportFileName': $scope.fileName, 'fromDate': $scope.FromDate, 'toDate': $scope.ToDate },
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
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.GoodWorkReport = function () {
        try {
            var dataList = [];
            var g = $("#GridEmp").data("ejGrid");
            dataList = g.getFilteredRecords();

            if (dataList.length == 0) {
                dataList = $scope.GWlist;
            }

            if (dataList.length == 0) {
                throw "First click on View button.";
            }

            $scope.fileName = "GoodWorkReport.xlsx";

            $http({
                method: 'POST',
                url: "Attendances/GoodWork/GetGoodWorkReportInDateRange",
                //data: { 'parameters': $scope.parameters },
                data: { 'data': dataList, 'reportFileName': $scope.fileName },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //$window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

}