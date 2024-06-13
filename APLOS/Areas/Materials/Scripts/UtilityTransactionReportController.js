'use strict';
UtilityTransactionReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function UtilityTransactionReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Utility Transaction Report';
    $scope.UtilityTransactionList = [];
    $scope.path = 'Materials/UtilityTransactionReport/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
  
    baseService.init($scope.getListUrl);


    $scope.ToDate = null;
    $scope.FromDate = null;
    $scope.UtilityGroupId = null;

   

    $scope.getUtilityTransactionData = function () {
       
        if (angular.isUndefinedOrNull($scope.FromDate) || angular.isUndefinedOrNull($scope.ToDate)) {
            ShowResult("Please Select the Date Range!", 'failure');
            throw ('Invalid Request!!');
        }

        $http({
            method: 'POST',
            url: $scope.path + 'getUtilityTransactionData',
            data: { 'ToDate': $scope.ToDate, 'FromDate': $scope.FromDate, 'UtilityGroupId': $scope.UtilityGroupId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.UtilityTransactionList = resp.data;
        });

    } 

    $scope.UserGroupList = [];
    $scope.GetUtilitygroupMaster = function () {
        $http({
            method: 'GET',
            url: 'Materials/UtilityTransactionReport/GetUserGroup'
        }).then(function successCallback(response) {
            $scope.UserGroupList = [];
            if (baseService.arrayLength(response.data) > 0) {
                $scope.UserGroupList = response.data;
            }
        });
    };
    $scope.GetUtilitygroupMaster();

    $scope.UtilityTransactionReport = function () {
        if (angular.isUndefinedOrNull($scope.FromDate) || angular.isUndefinedOrNull($scope.ToDate)) {
            ShowResult("Please Select the Date Range!", 'failure');
            throw ('Invalid Request!!');
        }

        var dataList = [];
        var g = $("#GridEdit").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {
            dataList = $scope.UtilityTransactionList;
        }

        $scope.fileName = "UtilityTransactionReport.xlsx";

        $http({
            method: 'POST',
            url: $scope.path + "GetUtilityTransactionReport",
            data: {'data': dataList,'reportFileName': $scope.fileName},
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