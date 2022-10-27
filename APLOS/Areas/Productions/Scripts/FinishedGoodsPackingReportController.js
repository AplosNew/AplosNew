'use strict';
FinishedGoodsPackingReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FinishedGoodsPackingReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Finished Goods Packing Report';
    $scope.path = 'Productions/FinishedGoodsPackingReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.LocList = [];

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
    $scope.fileName="GetFinishedGoodsPackingReport.xlsx";
    $scope.getFinishedStocksReport = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetReport",
            
            data: {
                'ToDate': $scope.ToDate, 'FromDate': $scope.FromDate
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
}