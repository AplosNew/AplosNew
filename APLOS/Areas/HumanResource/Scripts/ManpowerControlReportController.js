'use strict';
ManpowerControlReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function ManpowerControlReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Manpower Control Report';
    $scope.path = 'HumanResource/ManpowerControlReport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.Dates = new Date();

    $scope.parameters = [];
    $scope.filters = [];
    $scope.loadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + '/getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var columnList = [
                { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                { field: 'Process', width: 20, headerText: "Process", type: "string" },
                { field: 'UserReportGroup', width: 20, headerText: "User Report Group", type: "string" },
                { field: 'EmpType', width: 20, headerText: "Employee Type", type: "string" },

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

    $scope.loadfilters();


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

    $scope.getEmployeeWorkDurationReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEmployeeWorkDurationReport",
            data: {
                'FromDate': $scope.FromDate, 'ToDate': $scope.ToDate,
            },
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

    $scope.GetReport = function (reportType) {
        try {

            //The Filters Code
            var g = $("#filters").data("ejGrid");
            var fl = g.getFilteredRecords();
            if (fl.length == 0) {
                fl = $scope.filters;
            }


            var parameters = [];
            parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
            parameters.push({ "Key": "ProcessId", "Value": getString(fl, "ProcessId") });
            parameters.push({ "Key": "UserReportGroup", "Value": getString(fl, "UserReportGroup") });
            parameters.push({ "Key": "EmpTypeId", "Value": getString(fl, "EmpTypeId") });
            $scope.parameters = parameters;


            // The Report Code
            $http({
                method: 'POST',
                url: $scope.path + '/XlsManpowerControlReport',
                data: {
                    'Parameters': $scope.parameters, 'Dates': $scope.Dates,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

}