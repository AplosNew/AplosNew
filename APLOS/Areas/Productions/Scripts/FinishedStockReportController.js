'use strict';
FinishedStockReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FinishedStockReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Finished Stock Report';
    $scope.path = 'Productions/FinishedStockReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.LocList = [];
    $scope.ToDate = $filter("dateFiltering")(Date.now());
    function getLocations() {
        $http({
            method: 'GET',
            url:  "Productions/Packing/getLocations"
        }).then(function succ(resp) {
            $scope.LocList = resp.data;
        })
    }
    getLocations();

    $rootScope.LocName = "All";
    $rootScope.LocId = "All";

    $scope.LocChange = function () {
        var obj = $('#listLoc').data("ejDropDownList");
        $rootScope.LocName = obj.option("text");
        $rootScope.LocId = obj.option("value");
    }

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    $scope.getFinishedStocksReport = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetFinishedStocksReport",
            
            data: {
                //'ToDate': $scope.selectedValues.ToDate, 'FromDate': $scope.selectedValues.FromDate,
                //'type': $rootScope.typeVal, 'group': $rootScope.groupVal, 'value': $scope.search, 'column': $scope.searchBy,
                'ToDate': $scope.ToDate, 'FromDate': $scope.FromDate
                , 'Loc': $scope.LocId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //$rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FullPath + "&fileName=" + response.data.FileName);//downloadgriddataUrlPath
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
}