'use strict';
userDefineReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function userDefineReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    $scope.Action = 'Save';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.HrefList = [];
    $scope.UINameList = [];

    $scope.WorkDate = null;
    $scope.WrkDate = null;

    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.filters = [];

    $scope.getClearFiltersData = function () {
        try {
            var gridObj = $("#filters").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();

            if (baseService.isUndefinedOrNull($scope.WorkDate)) {
                throw "Work Date is required.";
            }
            $http({
                method: 'GET',
                url: 'Employees/EmployeeInFoReport/getFiltersData',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filters = response.data;
                var columnList = [
                    { field: 'ShiftName', width: 20, headerText: "Shift Name", type: "string" },
                    { field: 'PositionCategory', width: 20, headerText: "Position Category", type: "string" },
                    { field: 'EntityName', width: 20, headerText: "Entity Name", type: "string" },
                    { field: 'Section', width: 20, headerText: "Section", type: "string" },
                    { field: 'SubSection', width: 20, headerText: "SubSection", type: "string" },
                    { field: 'Department', width: 20, headerText: "Department", type: "string" },
                    { field: 'EmploymentType', width: 20, headerText: "EmploymentType", type: "string" }

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

    $scope.getFiltersData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.WorkDate)) {
                throw "Work Date is required.";
            }
            $http({
                method: 'GET',
                url: 'Employees/EmployeeInFoReport/getFiltersData',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filters = response.data;
                var columnList = [
                    { field: 'ShiftName', width: 20, headerText: "Shift Name", type: "string" },
                    { field: 'PositionCategory', width: 20, headerText: "Position Category", type: "string" },
                    { field: 'EntityName', width: 20, headerText: "Entity Name", type: "string" },
                    { field: 'Section', width: 20, headerText: "Section", type: "string" },
                    { field: 'SubSection', width: 20, headerText: "SubSection", type: "string" },
                    { field: 'Department', width: 20, headerText: "Department", type: "string" },
                    { field: 'EmploymentType', width: 20, headerText: "EmploymentType", type: "string" }

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
                //$scope.getFavouriteFiltersData();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.FavouriteFilters = [];
    function getUniqueValues(data, field) {
        var result = [];

        angular.forEach(data, function (item) {
            if (result.indexOf(item[field]) === -1) {
                result.push(item[field]);
            }
        });

        return result;
    }

    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }
        if (fl.length == 0) {
            fl = $scope.filtersdata;
        }


        var parameters = [];
        parameters.push({ "Key": "ShiftId", "Value": getString(fl, "ShiftId") });
        parameters.push({ "Key": "PositionCategory", "Value": getString(fl, "PositionCategory") });
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "SectionId", "Value": getString(fl, "SectionId") });
        parameters.push({ "Key": "SubSectionId", "Value": getString(fl, "SubSectionId") });
        parameters.push({ "Key": "DepartmentId", "Value": getString(fl, "DepartmentId") });
        parameters.push({ "Key": "EmploymentType", "Value": getString(fl, "EmploymentType") });

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
    

    $scope.DailyHRReportList = [];
    $scope.GetDailyReportData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.WorkDate)) {
                throw "Work Date is required.";
            }

            $scope.DailyHRReportList = [];
            $scope.filterComplete();

            $http({
                method: 'POST',
                url: "Employees/EmployeeInFoReport/GetDailyReportData",
                data: { 'parameters': $scope.parameters, 'date': $scope.WorkDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.DailyHRReportList = response.data;
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope._GetDailyReport = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.WrkDate)) {
                throw "Work Date is required.";
            }

            $scope.filterComplete();
            $scope.fileName = "DailyReport.xlsx";
            $http({
                method: 'POST',
                url: "Employees/EmployeeInFoReport/GetDailyReport",
                data: { 'parameters': $scope.parameters, 'date': $scope.WrkDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.GetDailyReport = function () {
        try {
            var dataList = [];
            var g = $("#GridEmp").data("ejGrid");
            dataList = g.getFilteredRecords();

            if (dataList.length == 0) {
                dataList = $scope.DailyHRReportList;
            }

            if (dataList.length == 0) {
                throw "First click on View button.";
            }

            $scope.fileName = "HRDailyReport.xlsx";

            $http({
                method: 'POST',
                url: "Employees/EmployeeInFoReport/GetDailyReport",
                data: { 'reportFileName': $scope.fileName, 'data': dataList, 'date': $scope.WrkDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    //-----------------------------------

    $scope.docEmployeeCategoryList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.docEmployeeCategoryList = result;
    });


    $scope.gridColumns = [];

    function buildColumnsFromData(data) {
        if (!data || data.length === 0) return [];

        var firstRow = data[0];
        var columns = [];

        angular.forEach(firstRow, function (value, key) {
            columns.push({
                field: key,
                headerText: key,
                width: 80,
                textAlign: (typeof value === 'number') ? 'right' : 'left'
            });
        });

        return columns;
    }

  
    $scope.EmpCat = null;
    $scope.DailyHRAttdnReportList = [];
    $scope.GetDailyAttdnReportData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.WrkDate)) {
                throw "Work Date is required.";
            }
            if (baseService.isUndefinedOrNull($scope.EmpCat)) {
                throw "Employee Category is required.";
            }

            $scope.DailyHRAttdnReportList = [];
           // $scope.filterComplete();

            $http({
                method: 'POST',
                url: "Employees/EmployeeInFoReport/GetDailyAttdnReportData",
                data: { 'date': $scope.WrkDate, 'empCatId': $scope.EmpCat},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.DailyHRAttdnReportList = response.data;
                    $scope.gridColumns = buildColumnsFromData(response.data);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }   

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.GetDailyAttdnReport = function () {
        var dataListUnDisbursed = [];
        var gUnDisbursed = $("#GridAttdnEmp").data("ejGrid");
        dataListUnDisbursed = gUnDisbursed.getFilteredRecords();

        if (dataListUnDisbursed.length == 0) {
            dataListUnDisbursed = $scope.DailyHRAttdnReportList;
        }
        $scope.fileName = 'DailyAttdnReport';
        $http({
            method: "POST",
            url: $scope.exportgriddataUrl,
            data: {
                'data': dataListUnDisbursed,
                'reportFileName': $scope.fileName
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.filtersdata = [];
    $scope.getGraphFiltersData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "From Date is required.";
            }
            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "To Date is required.";
            }

            $http({
                method: 'GET',
                url: 'Employees/EmployeeInFoReport/getFiltersData',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filtersdata = response.data;
                var columnList = [
                    { field: 'ShiftName', width: 20, headerText: "Shift Name", type: "string" },
                    { field: 'PositionCategory', width: 20, headerText: "Position Category", type: "string" },
                    { field: 'EntityName', width: 20, headerText: "Entity Name", type: "string" },
                    { field: 'Section', width: 20, headerText: "Section", type: "string" },
                    { field: 'SubSection', width: 20, headerText: "SubSection", type: "string" },
                    { field: 'Department', width: 20, headerText: "Department", type: "string" },
                    { field: 'EmploymentType', width: 20, headerText: "EmploymentType", type: "string" }

                ];
                $("#filtersdata").ejGrid({
                    dataSource: $scope.filters,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: columnList
                });

                var gridObj = $("#filtersdata").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                $("#filtersdata").children('.e-pager.e-js.e-pager').hide();
                $("#filtersdata").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#filtersdata").children('.e-gridcontent').hide();
                //$scope.getFavouriteFiltersData();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.parameters = [];
    $scope.graphfilterComplete = function () {

        var g = $("#filtersdata").data("ejGrid");
        var fl = g.getFilteredRecords();
       
        if (fl.length == 0) {
            fl = $scope.filtersdata;
        }


        var parameters = [];
        parameters.push({ "Key": "ShiftId", "Value": getString(fl, "ShiftId") });
        parameters.push({ "Key": "PositionCategory", "Value": getString(fl, "PositionCategory") });
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "SectionId", "Value": getString(fl, "SectionId") });
        parameters.push({ "Key": "SubSectionId", "Value": getString(fl, "SubSectionId") });
        parameters.push({ "Key": "DepartmentId", "Value": getString(fl, "DepartmentId") });
        parameters.push({ "Key": "EmploymentType", "Value": getString(fl, "EmploymentType") });

        $scope.parameters = parameters;
    }

    $scope.FromDate = null;
    $scope.ToDate = null;
    $scope.rowsList = [];

    var countChartObj = null;
    var pctChartObj = null;

    $scope.GetDailyGraphData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "From Date is required.";
            }
            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "To Date is required.";
            }

            $scope.rowsList = [];
            $scope.graphfilterComplete();

            $http({
                method: 'POST',
                url: "Employees/EmployeeInFoReport/GetDailyGraphData",
                data: { 'parameters': $scope.parameters, 'fdate': $scope.FromDate, 'tdate': $scope.ToDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.rowsList = response.data;

                    // draw the charts only after the data has arrived
                    countChartObj = drawLine('countChart', countChartObj,
                        buildChartData($scope.rowsList, 'EmployeeCount'), 'Employee Count');
                    pctChartObj = drawLine('pctChart', pctChartObj,
                        buildChartData($scope.rowsList, 'Percentage'), 'Percentage (%)');
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //// 'rows' is a normal parameter name
    //function buildChartData(rows, valueField) {
    //    const labels = [...new Set(rows.map(r => r.ReportDate))].sort();
    //    const particulars = [...new Set(rows.map(r => r.Particular))];

    //    const datasets = particulars.map(p => ({
    //        label: p,
    //        data: labels.map(d => {
    //            const row = rows.find(r => r.ReportDate === d && r.Particular === p);
    //            return row ? row[valueField] : null;
    //        }),
    //        tension: 0.3,
    //        borderWidth: 2,
    //        spanGaps: true
    //    }));

    //    return { labels, datasets };
    //}

    function buildChartData(rows, valueField) {
        // Set keeps insertion order, so labels follow the SQL ORDER BY (25-Oct → 10-Nov)
        const labels = [...new Set(rows.map(r => r.ReportDate))];
        const particulars = [...new Set(rows.map(r => r.Particular))];

        const datasets = particulars.map(p => ({
            label: p,
            data: labels.map(d => {
                const row = rows.find(r => r.ReportDate === d && r.Particular === p);
                return row ? row[valueField] : null;
            }),
            tension: 0.3,
            borderWidth: 2,
            spanGaps: true
        }));

        return { labels, datasets };
    }

    function drawLine(canvasId, oldChart, data, yTitle) {
        if (oldChart) { oldChart.destroy(); }   // remove the previous chart

        return new Chart(document.getElementById(canvasId), {
            type: 'line',
            data: data,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: { legend: { position: 'top' } },
                scales: {
                    x: { title: { display: true, text: 'Report Date' } },
                    y: { beginAtZero: true, title: { display: true, text: yTitle } }
                }
            }
        });
    }




}