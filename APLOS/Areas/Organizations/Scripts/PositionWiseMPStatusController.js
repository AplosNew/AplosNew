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


    //#region The Filters 

    $scope.filters = [];
    $scope.getPostionWMPStatusFilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getPostionWMPSFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            console.log($scope.filters);
            for (var i = 0; i < $scope.filters.length; i++) {
                $scope.filters[i].Age = (($scope.filters[i].MPBgt - $scope.filters[i].Deployment) / $scope.filters[i].Deployment) * 100;
            }

            var columnList = [
                { field: 'Plant', width: 20, headerText: "Plant", type: "string" },
                { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                { field: 'Division', width: 20, headerText: "Division", type: "string" },
                { field: 'Department', width: 20, headerText: "Department", type: "string" },
                { field: 'Section', width: 20, headerText: "Section", type: "string" },
                { field: 'SubSection', width: 20, headerText: "Sub-Section", type: "string" },
                { field: 'Designation', width: 20, headerText: "Designation", type: "string" },
                { field: 'Activity', width: 20, headerText: "Activity", type: "string" },
                { field: 'Process', width: 20, headerText: "Process", type: "string" },
                { field: 'Criticality', width: 20, headerText: "Criticality", type: "string" },
                { field: 'PositionCode', width: 20, headerText: "Position Code", type: "string" },
                { field: 'Deployment', width: 20, headerText: "Deployment", type: "string" },
                { field: 'MPBgt', width: 20, headerText: "MP Bgt", type: "string" },
                { field: 'Age', width: 20, headerText: "% Age", type: "string" },
                { field: 'OnRoll', width: 20, headerText: "On Roll", type: "string" },
                { field: 'TBS', width: 20, headerText: "TBS", type: "string" },
                { field: 'LAbs', width: 20, headerText: "L.Abs", type: "string" },
                { field: 'CurrentAvailable', width: 20, headerText: "Current Available", type: "string" },
                { field: 'Excess', width: 20, headerText: "Excess", type: "string" },
                { field: 'Short', width: 20, headerText: "Short", type: "string" },
                { field: 'AdditionalPlan', width: 20, headerText: "Additional Plan", type: "string" },
                { field: 'CurrentPlan', width: 20, headerText: "Current Plan", type: "string" },
                { field: 'ToReallocate', width: 20, headerText: "To Reallocate", type: "string" },
                { field: 'ToRecurit', width: 20, headerText: "To Recurit", type: "string" },

            ];
            $("#filters").ejGrid({
                dataSource: $scope.filters,
                minWidth: 450, minHeight: 400,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                filterSettings: { filterType: "excel" },
                columns: columnList
            });

            var gridObj = $("#filters").data("ejGrid");
            //gridObj.refreshContent(true);
            //gridObj.refreshTemplate();
            $("#filters").children('.e-pager.e-js.e-pager').hide();
            $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#filters").children('.e-gridcontent').hide();
        });
    }
    $scope.getPostionWMPStatusFilters();

    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#PositionEditList").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "Plant", "Value": getString(fl, "Plant") });
        parameters.push({ "Key": "Entity", "Value": getString(fl, "Entity") });
        parameters.push({ "Key": "Division", "Value": getString(fl, "Division") });
        parameters.push({ "Key": "Department", "Value": getString(fl, "Department") });
        parameters.push({ "Key": "Section", "Value": getString(fl, "Section") });
        parameters.push({ "Key": "SubSection", "Value": getString(fl, "SubSection") });
        parameters.push({ "Key": "Designation", "Value": getString(fl, "Designation") });
        parameters.push({ "Key": "Activity", "Value": getString(fl, "Activity") });
        parameters.push({ "Key": "Process", "Value": getString(fl, "Process") });
        parameters.push({ "Key": "Criticality", "Value": getString(fl, "Criticality") });
        parameters.push({ "Key": "PositionCode", "Value": getString(fl, "PositionCode") });
        parameters.push({ "Key": "Deployment", "Value": getString(fl, "Deployment") });
        parameters.push({ "Key": "MPBgt", "Value": getString(fl, "MPBgt") });
        parameters.push({ "Key": "Age", "Value": getString(fl, "Age") });
        parameters.push({ "Key": "OnRoll", "Value": getString(fl, "OnRoll") });
        parameters.push({ "Key": "TBS", "Value": getString(fl, "TBS") });
        parameters.push({ "Key": "LAbs", "Value": getString(fl, "LAbs") });
        parameters.push({ "Key": "CurrentAvailable", "Value": getString(fl, "CurrentAvailable") });
        parameters.push({ "Key": "Excess", "Value": getString(fl, "Excess") });
        parameters.push({ "Key": "Short", "Value": getString(fl, "Short") });
        parameters.push({ "Key": "AdditionalPlan", "Value": getString(fl, "AdditionalPlan") });
        parameters.push({ "Key": "CurrentPlan", "Value": getString(fl, "CurrentPlan") });
        parameters.push({ "Key": "ToReallocate", "Value": getString(fl, "ToReallocate") });
        parameters.push({ "Key": "ToRecurit", "Value": getString(fl, "ToRecurit") });

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

   
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.getPositionWiseMPStatusReport = function () {
        $scope.filterComplete();
        //var dataList = [];
        //var g = $("#filters").data("ejGrid");
        //dataList = g.getFilteredRecords();

        //if (dataList.length == 0) {
        //    dataList = $scope.parameters;
        //}
        $scope.fileName = 'Position Wise MP Status';

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            //url: $scope.path + "GetPositionWiseMPStatusReport",
            //data: { 'parameters': dataList },
            data: { 'data': $scope.parameters, 'reportFileName': $scope.fileName, },
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