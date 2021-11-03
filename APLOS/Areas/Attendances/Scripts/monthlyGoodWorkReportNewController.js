'use strict';
monthlyGoodWorkReportNewController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function monthlyGoodWorkReportNewController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Monthly Good Work Report';
    $scope.path = 'Attendances/MonthlyGoodWorkReportNew';
    $scope.downloadgriddataUrl = 'GridReports/Download';
   
    $scope.GoodWork = {
        YearNo: null,
        MonthNo: null
    };

    $scope.monthList = [
        {
            Value: 1,
            Text: 'January'
        },
        {
            Value: 2,
            Text: 'February'
        },
        {
            Value: 3,
            Text: 'March'
        },
        {
            Value: 4,
            Text: 'April'
        },
        {
            Value: 5,
            Text: 'May'
        },
        {
            Value: 6,
            Text: 'June'
        },
        {
            Value: 7,
            Text: 'July'
        },
        {
            Value: 8,
            Text: 'August'
        },
        {
            Value: 9,
            Text: 'September'
        },
        {
            Value: 10,
            Text: 'October'
        },
        {
            Value: 11,
            Text: 'November'
        },
        {
            Value: 12,
            Text: 'December'
        }
    ];

    $scope.yearlist = [];
    $scope.GetCbo = function () {
        $http.get('Attendances/AttendanceProcessUI/GetCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.yearlist = [];
                        $scope.yearlist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetCbo();

    $scope.GoodWork.YearNo = new Date().getFullYear().toString();
    $scope.GoodWork.MonthNo = new Date().getMonth().toString();

  
    $scope.parameters = [];
    $scope.filters = [];
    $scope.loadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path+ '/getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var columnList = [
                { field: 'PlantName', width: 20, headerText: "Plant Name", type: "string" },
                { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                { field: 'Department', width: 20, headerText: "Department", type: "string" },
                { field: 'Section', width: 20, headerText: "Section", type: "string" },
                { field: 'SubSection', width: 20, headerText: "Sub Section", type: "string" },
                { field: 'AttnGroup', width: 20, headerText: "Attendance Group", type: "string" },
                { field: 'PayRollGroup', width: 20, headerText: "Pay Group", type: "string" }

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



    $scope.GetGoodWorkReport = function (reportType) {
        try {
            //The Filters Code
            var g = $("#filters").data("ejGrid");
            var fl = g.getFilteredRecords();
            if (fl.length == 0) {
                fl = $scope.filters;
            }


            var parameters = [];
            parameters.push({ "Key": "PlantId", "Value": getString(fl, "PlantId") });
            parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
            parameters.push({ "Key": "DepartmentId", "Value": getString(fl, "DepartmentId") });
            parameters.push({ "Key": "SectionId", "Value": getString(fl, "SectionId") });
            parameters.push({ "Key": "SubSectionId", "Value": getString(fl, "SubSectionId") });
            parameters.push({ "Key": "PayrollGroupId", "Value": getString(fl, "PayrollGroupId") });
            parameters.push({ "Key": "AttndGroupId", "Value": getString(fl, "AttndGroupId") });
            $scope.parameters = parameters;


            // The Report Code
            if (baseService.isUndefinedOrNull($scope.GoodWork.YearNo)) {
                manualValidation('div_FromDate', true, "Year is required.");
                ShowResult("Year is required.", 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.GoodWork.MonthNo)) {
                manualValidation('div_FromDate', true, "Month is required.");
                ShowResult("Month is required.", 'failure');
            }
            else {
                $http({
                    method: 'POST',
                    url: $scope.path + '/XlsGoodWorkReport',
                    data: {
                        'Month': $scope.GoodWork.MonthNo, 'Year': $scope.GoodWork.YearNo , 'Parameters' : $scope.parameters
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



}