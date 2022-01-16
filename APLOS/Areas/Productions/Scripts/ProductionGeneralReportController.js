'use strict';
ProductionGeneralReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProductionGeneralReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Production Report';
    $scope.path = 'Productions/ProductionGeneralReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    //Variables 
    $scope.filtersList = [];
    $scope.masterData = [];
    $scope.masterDetailData = [];
    $scope.ColName = 'Master';



    // Getting the Filters 
    //- $http({}).then(function () { });

    $scope.getFilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters'
        }).then(function (resp) {
            $scope.filtersList = resp.data;
            var gridObj = $("#FilterList").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $("#FilterList").children('.e-pager.e-js.e-pager').hide();
            $("#FilterList").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#FilterList").children('.e-gridcontent').hide();
            $scope.fillFilters();
        });
    }

   
    // Filling in the Filters as Parameters

    $scope.fillFilters = function () {

        var gridObj = $("#FilterList").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        
        if (filteredRecords.length == 0) {
            filteredRecords = $scope.filtersList;
        }

        var parameters = [];
        parameters.push({ "Key": "CustomerId", "Value": getString(filteredRecords, "CustomerId") });
        parameters.push({ "Key": "BuyerRef", "Value": getString(filteredRecords, "BuyerRef") });
        parameters.push({ "Key": "OwnRef", "Value": getString(filteredRecords, "OwnRef") });
        parameters.push({ "Key": "MOId", "Value": getString(filteredRecords, "MOId") });
        parameters.push({ "Key": "LineItem", "Value": getString(filteredRecords, "LineItem") });
        parameters.push({ "Key": "SO", "Value": getString(filteredRecords, "SO") });
        parameters.push({ "Key": "PRStatus", "Value": getString(filteredRecords, "PRStatus") });
        parameters.push({ "Key": "SOOrderId", "Value": getString(filteredRecords, "SOOrderId") });
        parameters.push({ "Key": "PSLibId", "Value": getString(filteredRecords, "PSLibId") });
        parameters.push({ "Key": "ProcessId", "Value": getString(filteredRecords, "ProcessId") });


        $scope.getMaster(parameters);
    }

    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        if (data.length > 0) {
            for (var i = 0; i < data.length; i++) {
                if (collection.includes(data[i][column]) == false) {
                    string += ",'" + data[i][column] + "'";
                    collection.push(data[i][column]);
                }
            }
        }
       
        return string;
    }

    // Getting the Master Grid

    $scope.getMaster = function (param) {
        $http({
            method: 'POST',
            url: $scope.path + 'getMasterGrid',
            data: {'filters': param}
        }).then(function (resp) {
            if (resp.data.Error == false) {
                $scope.masterData = [];
                $scope.masterData = resp.data.Data;
            }
            else {
                ShowResult(resp.data.Message, 'failure');
            }
            
        });
    }

    //Double Click in Master Grid
    $scope.detailClick = function (e) {
        if (e.cellIndex > 7) {
            $scope.ColName = e.columnName;
        }
        else {
            $scope.ColName = 'Master';
        }

        var PRId = e.data.ProductionOrderId;
        $http({
            method: 'POST',
            url: $scope.path + 'masterDetail',
            data: {'PRId':PRId , 'Col':$scope.ColName},
        }).then(function (resp) {
            $scope.masterDetailData = [];
            $scope.masterDetailData = resp.data;
            angular.element(document.querySelector('#masterDetail')).modal('show');
        });
    }

    //Downloading Of the Reports
    $scope.getReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getReports",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    //Initial Loading Functions
    $scope.getFilters();
}