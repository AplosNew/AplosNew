'use strict';
userDefineReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function userDefineReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    $scope.Action = 'Save';

    $scope.tab = 2;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.HrefList = [];
    $scope.UINameList = [];

    $scope.WrkDate = null;

    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.filters = [];

    $scope.getClearFiltersData = function () {
        try {
            var gridObj = $("#filters").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();

            if (baseService.isUndefinedOrNull($scope.WrkDate)) {
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
            if (baseService.isUndefinedOrNull($scope.WrkDate)) {
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

    //$scope.getFavouriteFiltersData = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'Employees/EmployeeInFoReport/getFavouriteFiltersData'
    //    }).then(function (response) {

    //        $scope.FavouriteFilters = response.data;

    //        setTimeout(function () {
    //            applyFavouriteFilters();
    //        }, 500);
    //    });
    //};



    //function applyFavouriteFilters() {

    //    var gridObj = $("#filters").ejGrid("instance");
    //    if (!gridObj) return;

    //    var excelFilters = buildExcelFilterCollection();

    //    // ⭐ THIS LINE makes checkboxes ticked
    //    gridObj.model.filterSettings.filteredColumns = excelFilters;

    //    // refresh grid with filters
    //    gridObj.refreshContent();
    //}


    //function buildExcelFilterCollection() {

    //    var fav = $scope.FavouriteFilters;

    //    var filters = [];

    //    function pushFilters(field, values) {
    //        for (var i = 0; i < values.length; i++) {
    //            filters.push({
    //                field: field,
    //                operator: "equal",
    //                value: values[i],
    //                predicate: i == 0 ? "and" : "or",
    //                matchcase: false
    //            });
    //        }
    //    }

    //    pushFilters("ShiftName", getUniqueValues(fav, "ShiftName"));
    //    pushFilters("PositionCategory", getUniqueValues(fav, "PositionCategory"));
    //    pushFilters("EntityName", getUniqueValues(fav, "EntityName"));
    //    pushFilters("Section", getUniqueValues(fav, "Section"));

    //    return filters;
    //}


    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "ShiftId", "Value": getString(fl, "ShiftId") });
        parameters.push({ "Key": "PositionCategory", "Value": getString(fl, "PositionCategory") });
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "SectionId", "Value": getString(fl, "SectionId") })
        parameters.push({ "Key": "EmploymentType", "Value": getString(fl, "EmploymentType") })

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

 

    //$scope.ShiftList = [];
    //$scope.getShiftCbo = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'Employees/EmployeeInFoReport/getShiftCbo'
    //    }).then(function successCallback(response) {
    //        $scope.ShiftList = response.data;
    //    });
    //}
    //$scope.getShiftCbo();


    //$scope.PositionList = [];
    //$scope.getPositionCbo = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'Employees/EmployeeInFoReport/getPositionCbo'
    //    }).then(function successCallback(response) {
    //        $scope.PositionList = response.data;
    //    });
    //}
    //$scope.getPositionCbo();

    //$scope.EntityList = [];
    //$scope.getEntityCbo = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'Employees/EmployeeInFoReport/getEntityCbo'
    //    }).then(function successCallback(response) {
    //        $scope.EntityList = response.data;
    //    });
    //}
    //$scope.getEntityCbo();

    //$scope.SectionList = [];
    //$scope.getSectionCbo = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'Employees/EmployeeInFoReport/getSectionCbo'
    //    }).then(function successCallback(response) {
    //        $scope.SectionList = response.data;
    //    });
    //}
    //$scope.getSectionCbo();

    $scope.DailyHRReportList = [];
    $scope.GetDailyReportData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.WrkDate)) {
                throw "Work Date is required.";
            }

            $scope.DailyHRReportList = [];
            $scope.filterComplete();

            $http({
                method: 'POST',
                url: "Employees/EmployeeInFoReport/GetDailyReportData",
                data: { 'parameters': $scope.parameters, 'date': $scope.WrkDate },
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

}