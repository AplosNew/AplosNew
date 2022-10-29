'use strict';
FinishedGoodsPackingReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FinishedGoodsPackingReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Finished Goods Packing Report';
    $scope.path = 'Productions/FinishedGoodsPackingReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    //$scope.PurposeList = [];
    //$scope.getAllEntities = function () {
    //    $http({
    //        method: 'POST',
    //        url: "Productions/MaterialMovementPurpose/GetCbo"
    //    }).then(function successCallback(response) {
    //        $scope.PurposeList = response.data;
    //        var index = 0;
    //        $('#PurposeList').ejDropDownList(
    //            {
    //                dataSource: $scope.entityList,
    //                fields: { text: "UserName", value: "Id" },
    //                selectedIndex: index, showCheckBox: true, multiSelectMode: ej.MultiSelectMode.VisualMode
    //                , width: 250
    //            });
    //        var DropDownEntityListObj = $("#PurposeList").data("ejDropDownList");
    //        var PurposeId = DropDownEntityListObj.getSelectedValue();
    //    });
    //}
    //$scope.getAllEntities();

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.fileName="GetFinishedGoodsPackingReport.xlsx";
    $scope.getFinishedStocksReport = function () {
        //var DropDownEntityListObj = $("#PurposeList").data("ejDropDownList");
        //var Purpose = DropDownEntityListObj.getSelectedValue();

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