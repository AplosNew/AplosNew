'use strict';
manpowerBudgetReportController.$inject = ['cboService', '$scope', '$rootScope', '$routeParams', 'baseService', '$http', '$filter','$window'];
function manpowerBudgetReportController(cboService, $scope, $rootScope, $routeParams, baseService, $http, $filter, $window) {
    $scope.title = 'Manpower Budget Report';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.mbfilters = [];

    $scope.MBWisefilters = function () {
        try {
            $scope.mbfilters = [];
            $http({
                method: 'GET',
                url: 'HumanResource/ManpowerBudgetDashboard/getMBWiseFilters',
                dataType: 'JSON'
            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    $scope.mbfilters = response.data;
                    var columnList = [
                        { field: 'BudgetCode', width: 20, headerText: "BudgetCode", type: "string" },
                        { field: 'Division', width: 20, headerText: "Division", type: "string" },
                        { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                        { field: 'Department', width: 20, headerText: "Department", type: "string" },
                        { field: 'Section', width: 20, headerText: "Section", type: "string" },
                        { field: 'SubSection', width: 20, headerText: "SubSection", type: "string" },
                        { field: 'Designation', width: 20, headerText: "Designation", type: "string" },
                        { field: 'ShiftName', width: 20, headerText: "ShiftName", type: "string" },
                        { field: 'Line', width: 20, headerText: "Line", type: "string" }
                    ];

                    $("#mbfilters").ejGrid({
                        dataSource: $scope.mbfilters,
                        minWidth: 450, minHeight: 400,
                        allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                        filterSettings: { filterType: "excel" },
                        columns: columnList
                    });

                    var gridSumObj = $("#mbfilters").data("ejGrid");
                    gridSumObj.refreshContent(true);
                    gridSumObj.refreshTemplate();
                    $("#mbfilters").children('.e-pager.e-js.e-pager').hide();
                    $("#mbfilters").children('.e-gridcontent.e-droppable.e-js').hide();
                    $("#mbfilters").children('.e-gridcontent').hide();
                }

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.MBWisefilters();


    var getString = function (data, column) {
        var string = "''";
        var collection = [];

        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                /* var replace = data[i][column].replace(",", "','");*/
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }

    $scope.powisemeters = [];
    $scope.POWisefilterComplete = function () {

        var g = $("#mbfilters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.mbfilters;
        }


        var powisemeters = [];
        powisemeters.push({ "Key": "BudgetId", "Value": getString(fl, "BudgetId") });
        powisemeters.push({ "Key": "DivisionId", "Value": getString(fl, "DivisionId") });
        powisemeters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        powisemeters.push({ "Key": "DepartmentId", "Value": getString(fl, "DepartmentId") });
        powisemeters.push({ "Key": "SectionId", "Value": getString(fl, "SectionId") });
        powisemeters.push({ "Key": "SubSectionId", "Value": getString(fl, "SubSectionId") });
        powisemeters.push({ "Key": "DesignationId", "Value": getString(fl, "DesignationId") });
        powisemeters.push({ "Key": "ShiftId", "Value": getString(fl, "ShiftId") });
        powisemeters.push({ "Key": "LineId", "Value": getString(fl, "LineId") });

        $scope.powisemeters = powisemeters;

    }

    //--Filters End---

    $scope.MBWiseList = [];
    $scope.GetMBWiseView = function () {
        $scope.POWisefilterComplete();
        $http({
            method: 'POST',
            url: 'HumanResource/ManpowerBudgetDashboard/MBWiseData',
            data: { 'parameters': $scope.powisemeters },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MBWiseList = response.data.NewData;
        });
    };

    $scope.MBWiseReportExcel = function () {
        var dataList = [];
        var g = $("#GridMB").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.MBWiseList;
        }
        $scope.fileName = 'Manpower Budget Report.xlsx';

        $http({
            method: 'POST',
            url: "HumanResource/ManpowerBudgetDashboard/GetMBWiseReportDataXls",
            data: {
                'data': dataList,
                'reportFileName': $scope.fileName
            },
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
    }


}