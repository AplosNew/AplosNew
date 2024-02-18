'use strict';
LCReportsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function LCReportsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'LC Reports';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.MasterLCList = [];
    $scope.path = 'Commercial/LCReports/';
    $scope.reportParameters = {
        FromDate: null,
        ToDate: null,
        //LCType: null
        LCType: 'contract'
    };

    $scope.GetMasterLCList = function () {

        try {

            //if (angular.isUndefinedOrNull($scope.reportParameters.FromDate))
            //    throw 'Please enter from date';

            //if (angular.isUndefinedOrNull($scope.reportParameters.ToDate))
            //    throw 'Please enter to date';

            $http({
                method: 'POST',
                url: $scope.path + "GetMasterLCList",
                data: { FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate, lcType: $scope.reportParameters.LCType },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.MasterLCList = response.data;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');

                }
        }
        catch (e) {

        }
    }

    $scope.refreshTemplateLC = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllLC });
    };

    function CheckBoxSelectAllLC(e) {
        var ChkOrUnchkLC = false;
        if (e.model.checkState === "check") {
            ChkOrUnchkLC = true;
        }
        var filtered = $("#GridMasterLC").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MasterLCList.length; i++) {
                $scope.MasterLCList[i].isSelected = ChkOrUnchkLC;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].isSelected = ChkOrUnchkLC;
            }
        }
        var gridObj = $("#GridMasterLC").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.MasterLCReport = function () {
        var dataList = [];
        //var g = $("#GridMasterLC").data("ejGrid");
        //dataList = g.getFilteredRecords();

        for (var i = 0; i < $scope.MasterLCList.length; i++) {
            if ($scope.MasterLCList[i].isSelected == true) {
                dataList.push($scope.MasterLCList[i]);
            }
        }
         
        $scope.fileName = 'Master LC Report.xlsx';
        $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

        if (dataList.length == 0) {
            ShowResult('Please select at least one Item', 'failure');
        }
        else {

            $http({
                method: 'POST',
                url: $scope.path + "MasterLCDataXls",
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



    $scope.MasterOrderReport = function () {

        //var MasterOrderId = "1935";
        try {
            var file_src = $scope.path + "MasterOrderReport?MasterOrderId=1935";
            $rootScope.report(file_src);


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


}


