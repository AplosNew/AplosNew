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

    $scope.getUtilityTransactionData = function () {
       
        if (angular.isUndefinedOrNull($scope.FromDate) || angular.isUndefinedOrNull($scope.ToDate)) {
            ShowResult("Please Select the Date Range!", 'failure');
            throw ('Invalid Request!!');
        }

        $http({
            method: 'POST',
            url: $scope.path + 'getUtilityTransactionData',
            data: {'ToDate': $scope.ToDate, 'FromDate': $scope.FromDate },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.UtilityTransactionList = resp.data;
        });

    }
    //$scope.getData();

    ////The Filters 
    //$scope.filters = [];
    //$scope.UtilityTransactionloadfilters = function () {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'getFilters',
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.filters = response.data;
    //        var columnList = [
    //            { field: 'Group', width: 20, headerText: "Group", type: "string" },
    //            { field: 'SubGroup', width: 20, headerText: "Sub Group", type: "string" },
    //            { field: 'Category', width: 20, headerText: "Category", type: "string" },
    //            { field: 'SubCategory', width: 20, headerText: "Sub Category", type: "string" },
    //            { field: 'AddedDate', width: 20, headerText: "Added Date", type: "string" },
    //            { field: 'ResponsiblePerson', width: 20, headerText: "Responsible Person", type: "string" },
    //        ];
    //        $("#filters").ejGrid({
    //            dataSource: $scope.filters,
    //            minWidth: 450, minHeight: 400,
    //            allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
    //            filterSettings: { filterType: "excel" },
    //            columns: columnList
    //        });

    //        var gridObj = $("#filters").data("ejGrid");
    //        //gridObj.refreshContent(true);
    //        //gridObj.refreshTemplate();
    //        $("#filters").children('.e-pager.e-js.e-pager').hide();
    //        $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
    //        $("#filters").children('.e-gridcontent').hide();
    //    });
    //}
    //$scope.UtilityTransactionloadfilters();

    //$scope.parameters = [];
    //$scope.filterComplete = function () {

    //    var g = $("#filters").data("ejGrid");
    //    var fl = g.getFilteredRecords();
    //    if (fl.length == 0) {
    //        fl = $scope.filters;
    //    }


    //    var parameters = [];
    //    parameters.push({ "Key": "Id", "Value": getString(fl, "Id") });
        
    //    $scope.parameters = parameters;
    //}

    //var getString = function (data, column) {
    //    var string = "''";
    //    var collection = [];

    //    for (var i = 0; i < data.length; i++) {
    //        if (collection.includes(data[i][column]) == false) {
    //            string += ",'" + data[i][column] + "'";
    //            collection.push(data[i][column]);
    //        }
    //    }
    //    return string;
    //}


    $scope.UtilityTransactionReport = function () {
        if (angular.isUndefinedOrNull($scope.FromDate) || angular.isUndefinedOrNull($scope.ToDate)) {
            ShowResult("Please Select the Date Range!", 'failure');
            throw ('Invalid Request!!');
        }

        //$scope.filterComplete();
        $scope.fileName = "UtilityTransactionReport.xlsx";

        $http({
            method: 'POST',
            url: $scope.path + "GetUtilityTransactionReport",
            data: {'ToDate': $scope.ToDate, 'FromDate': $scope.FromDate},
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