'use strict';
UtilityTransactionReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function UtilityTransactionReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Machine Master Transaction Report';
    $scope.ModelList = [];
    $scope.path = 'Materials/UtilityTransactionReport/';
    $scope.downloadgriddataUrlPath = 'Materials/UtilityTransactionReport/DownloadUsingFullPath';
    baseService.init($scope.getListUrl);

    //The Filters 
    $scope.filters = [];
    $scope.UtilityTransactionloadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var columnList = [
                { field: 'Date', width: 20, headerText: "Date", type: "string" },
                { field: 'Quantity', width: 20, headerText: "Quantity", type: "string" },
                { field: 'Remarks', width: 20, headerText: "Remarks", type: "string" },
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
    }
    $scope.UtilityTransactionloadfilters();

    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "Date", "Value": getString(fl, "Date") });
        parameters.push({ "Key": "Quantity", "Value": getString(fl, "Quantity") });
        parameters.push({ "Key": "Remarks", "Value": getString(fl, "Remarks") });

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

    $scope.Report = function () {
        try {

            $scope.filterComplete();
            $scope.fileName = "UtilityTransactionReport.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetUtilityTransactionReport",
                data: { 'parameters': $scope.parameters },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    // $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }
}