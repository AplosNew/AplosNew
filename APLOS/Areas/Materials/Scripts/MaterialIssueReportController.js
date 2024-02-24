'use strict';
MaterialIssueReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function MaterialIssueReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Material Control Report';
    $scope.TransactionList = [];
    $scope.path = 'Materials/MaterialIssueReport/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.filters = [];
    $scope.getFiltersData = function () {
        try {
            $http({
                method: 'GET',
                url: 'Materials/MaterialIssueReport/getFiltersData',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filters = response.data;
                var columnList = [
                    { field: 'POStatus', width: 20, headerText: "POStatus", type: "string" },
                    { field: 'PONo', width: 20, headerText: "PONo", type: "string" },

                ];
                $("#filters").ejGrid({
                    dataSource: $scope.filters,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: columnList
                });

                var gridObj = $("#filters").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                $("#filters").children('.e-pager.e-js.e-pager').hide();
                $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#filters").children('.e-gridcontent').hide();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.getFiltersData();
    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "POStatus", "Value": getString(fl, "POStatus") });
        parameters.push({ "Key": "PONo", "Value": getString(fl, "PONo") });

        $scope.parameters = parameters;
    }

    var getString = function (data, column) {
        var string = "''";
        var collection = [];

        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }

    $scope.GetTransactionData = function () {
        $scope.filterComplete();
        $http({
            method: 'POST',
            url: $scope.path + 'GetTransactionData',
            data: { 'parameters': $scope.parameters },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.TransactionList = resp.data;
        });

    }
    


    $scope.PrintReport = function () {
        $scope.fileName = "MaterialIssueReport.xlsx";
        var dataList = [];
       
        var g = $("#GridEdit").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.TransactionList;
        }
   
        $http({
            method: 'POST',
            url: $scope.path + "GetReport",
            data: { 'data': dataList},
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