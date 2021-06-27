'use strict';
AttendanceFromAppReportController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function AttendanceFromAppReportController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'AttendanceFromApp Report';


    $scope.path = 'Attendances/AttendanceFromAppReport/';

    $scope.selectedValues = {
        FromDate: null,
        ToDate: null,
        AttndType:null,
    };

 
    $scope.clearFliters = function () {
        $scope.selectedValues.FromDate = null;
        $scope.selectedValues.ToDate = null;        
        $scope.selectedValues.AttndType = null;
    }

 
    /// --- Grid Show
    $scope.MainData = [];
    $scope.loadGrid = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.MainData.length != 0) {
            $scope.destroy();
        }
        if ($scope.General.$valid) {

            var ColumnList = [
                { field: 'Plant', width: 150, headerText: "Plant", type: "string" },
                { field: 'Unit', width: 150, headerText: "Unit", type: "string" },
                { field: 'EmployeeCode', width: 150, headerText: "EmpCode", type: "string" },
                { field: 'EmployeeName', width: 150, headerText: "Employee Name", type: "string" },
                { field: 'Department', width: 150, headerText: "Department", type: "string" },
                { field: 'Section', width: 150, headerText: "Section", type: "string" },
                { field: 'SubSection', width: 150, headerText: "SubSection", type: "string" },
                { field: 'Designation', width: 150, headerText: "Designation", type: "string" },
                { field: 'Date', width: 150, headerText: "Date", type: "string", allowFiltering: false },
                { field: 'InTime', width: 150, headerText: "InTime", type: "string", allowFiltering: false },             
                { field: 'OutTime', width: 150, headerText: "OutTime", type: "string", allowFiltering: false },  
            ];
           
            $http({
                method: 'GET',
                url: $scope.path + 'GetAttndData?From=' + $scope.selectedValues.FromDate + '&To=' + $scope.selectedValues.ToDate + '&AttndType='
                    + $scope.selectedValues.AttndType,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.MainData = response.data.DATA;

                    $("#GridData").ejGrid({
                        dataSource: $scope.MainData,
                        minWidth: 450, minHeight: 400,
                        allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                        filterSettings: { filterType: "excel" },
                        columns: ColumnList
                    });


                    var gridObj = $("#GridData").data("ejGrid");
                    gridObj.refreshContent(true);
                    gridObj.refreshTemplate();

                    var x = document.getElementById("but");
                    if ($scope.MainData.length != 0) {
                        x.style.display = "block";
                    }
                    else {
                        x.style.display = "none";
                    }
                }
            });

        }

    }


    //Grid Destroy
    $scope.destroy = function () {
        var grid = $("#GridData").data("ejGrid");
        grid.destroy();
    }


    $scope.filteringData = function () {

        var gridobj = $("#GridData").data("ejGrid");
        var filteredRecords = gridobj.getFilteredRecords();

        if (filteredRecords.length == 0) {
            filteredRecords = $scope.MainData;
        }

        var parameters = [];
        parameters.push({ "Key": "EmpName", "Value": getString(filteredRecords, "EmployeeName") });
        parameters.push({ "Key": "PlantId", "Value": getString(filteredRecords, "PlantId") });
        parameters.push({ "Key": "SubId", "Value": getString(filteredRecords, "SubsectionId") });
        parameters.push({ "Key": "SectionId", "Value": getString(filteredRecords, "SectionId") });
        parameters.push({ "Key": "DesgId", "Value": getString(filteredRecords, "DesignationId") });
        parameters.push({ "Key": "UnitId", "Value": getString(filteredRecords, "UnitId") });
        parameters.push({ "Key": "DeptId", "Value": getString(filteredRecords, "DepartmentId") });
        parameters.push({ "Key": "EmpCode", "Value": getString(filteredRecords, "EmployeeCode") });

        applyFilters(parameters);

       
    }

 

    function applyFilters(parameters) {

        $http({
            method: 'POST',
            url: $scope.path + 'GetPrintReport',
            data: {
                From: $scope.selectedValues.FromDate, To: $scope.selectedValues.ToDate,
                AttndType: $scope.selectedValues.AttndType, EmpName: parameters[0].Value,
                PlantId: parameters[1].Value, SubId: parameters[2].Value,
                SectionId: parameters[3].Value, DesgId: parameters[4].Value, UnitId: parameters[5].Value,
                DeptId: parameters[6].Value, EmpCode: parameters[7].Value
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


    $scope.downloadgriddataUrl = 'GridReports/Download';
   
    var getString = function (data, column) {
        var kk = "";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) === false) {
                if (kk === "") {
                    kk += "'" + data[i][column] + "'";
                }
                else if (data[i][column] == null) {
                }
                else {
                    kk += ",'" + data[i][column] + "'";
                }
            }

        }
        return kk;
    } 


}