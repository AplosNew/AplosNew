'use strict';
PositionWiseMPStatusController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', '$window'];
function PositionWiseMPStatusController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $controller, $filter, $cboService, $window, fileReader) {
    $rootScope.title = 'Position Wise MP Status Report';
    $scope.ModelList = [];
    $scope.path = 'Organizations/PositionWiseMPStatus/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.Action = 'Save';
    //$scope.searchBy = "UserName"; $scope.searchBySO = "MasterOrderId"; $scope.searchSO = ''; $scope.search = "";
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.filters = [];
    $scope.getPostionWMPStatusFilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getPostionWMPSData',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
       
            var columnList = [
                { field: 'IsDirect', width: 20, headerText: "Direct/ In-Direct/ All", type: "string" },
                { field: 'EmpoyeeCategory', width: 20, headerText: "Employee Category", type: "string" },
                { field: 'CriticalityLevel', width: 20, headerText: "Criticality Level", type: "string" },
                { field: 'UserReportGroup', width: 20, headerText: "User Group", type: "string" },
                { field: 'Plant', width: 20, headerText: "Plant", type: "string" },
                { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                
            ];
            $("#filters").ejGrid({
                dataSource: $scope.filters,
                minWidth: 450, minHeight: 400,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                filterSettings: { filterType: "excel" },
                columns: columnList
            });

            var gridObj = $("#filters").data("ejGrid");
            $("#filters").children('.e-pager.e-js.e-pager').hide();
            $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#filters").children('.e-gridcontent').hide();
        });
    }
    $scope.getPostionWMPStatusFilters();


    $scope.PositionParameters = [];
    $scope.filterComplete = function () {
        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }
        var parameters = [];

        parameters.push({ "Key": "PlantId", "Value": getString(fl, "PlantId") });
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "UserReportGroup", "Value": getString(fl, "UserReportGroup") });
        parameters.push({ "Key": "CriticalityLevel", "Value": getString(fl, "CriticalityLevel") });
        parameters.push({ "Key": "IsDirect", "Value": getString(fl, "IsDirect") });
        parameters.push({ "Key": "EmpoyeeCategoryId", "Value": getString(fl, "EmpoyeeCategoryId") });

        $scope.PositionParameters = parameters;
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

    $scope.ModelList = [];
    $scope.getData = function () {
      $scope.filterComplete();
        $http({
            method: 'POST',
            url: $scope.path + "getPostionWMPSSqlData",
            data: { 'parameters': $scope.PositionParameters },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;

            for (var i = 0; i < $scope.ModelList.length; i++) {
                $scope.ModelList[i].Age = parseFloat((($scope.ModelList[i].MPBgt - $scope.ModelList[i].Deployment) / $scope.ModelList[i].Deployment) * 100).toFixed(2);
                if ($scope.ModelList[i].Age == 'NaN' || $scope.ModelList[i].Age == 'Infinity') {
                    $scope.ModelList[i].Age = 0;
                }
            }
        });
    }
    
   
   
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.getPositionWiseMPStatusReport = function () {
        var dataList = [];
        var g = $("#PositionEditList").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ModelList;
        }
        $scope.fileName = 'Position Wise MP Status';

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': dataList, 'reportFileName': $scope.fileName, },
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

}