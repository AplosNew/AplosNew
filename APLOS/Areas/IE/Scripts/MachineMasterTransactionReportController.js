'use strict';
MachineMasterTransactionReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function MachineMasterTransactionReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $scope.title = 'Machine Master Transaction Report';
    $scope.ModelList = [];
    $scope.path = 'IE/MachineMasterTransactionReport/';
    $scope.downloadgriddataUrlPath = 'IE/MachineMasterTransactionReport/DownloadUsingFullPath';
    baseService.init($scope.getListUrl);
    $scope.FromDate = null;
    $scope.ToDate = null;
    //The Filters 
    $scope.filters = [];
    $scope.MachineMasterTransactionloadfilters = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "Select From Date.";
            }
            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "Select To Date.";
            }
            if (new Date($scope.FromDate) > new Date($scope.ToDate)) {
                throw "From date must be below or equal to To Date";
            }
            if (new Date($scope.ToDate) < new Date($scope.FromDate)) {
                throw "To date must be above or equal to From Date.";
            }

            $http({
                method: 'GET',
                url: $scope.path + 'getFilters?fromDate=' + $scope.FromDate + '&toDate=' + $scope.ToDate,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filters = response.data;
                var columnList = [
                    { field: 'From', width: 20, headerText: "From", type: "string" },
                    { field: 'To', width: 20, headerText: "To", type: "string" },
                    { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                    { field: 'Process', width: 20, headerText: "Process", type: "string" },
                    { field: 'Department', width: 20, headerText: "Department", type: "string" },
                    { field: 'DetentionType', width: 20, headerText: "Detention Type", type: "string" },
                    { field: 'Shift', width: 20, headerText: "Shift", type: "string" },
                    { field: 'ResponsiblePerson', width: 20, headerText: "ResponsiblePerson", type: "string" },
                    { field: 'DetentionCategory', width: 20, headerText: "Detention Category", type: "string" },
                    { field: 'DetentionSubCategory', width: 20, headerText: "Detention Sub Category", type: "string" },
                    { field: 'Avoidable', width: 20, headerText: "Avoidable/Unavoidable", type: "string" },
                    { field: 'Criticality', width: 20, headerText: "Criticality", type: "string" },

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
   // $scope.MachineMasterTransactionloadfilters();

    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "ProcessId", "Value": getString(fl, "ProcessId") });
        parameters.push({ "Key": "DepartmentId", "Value": getString(fl, "DepartmentId") });
        parameters.push({ "Key": "DetentionId", "Value": getString(fl, "DetentionId") });
        parameters.push({ "Key": "ShiftId", "Value": getString(fl, "ShiftId") });
        parameters.push({ "Key": "ResponsiblePersonId", "Value": getString(fl, "ResponsiblePersonId") });
        
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
            $scope.fileName = "MachineMasterTransactionReport.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetMachineMasterTransactionReport",
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