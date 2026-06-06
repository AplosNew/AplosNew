'use strict';
EmployeeLastPunchReportController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeLastPunchReportController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Last Punch Report';


    $scope.path = 'Attendances/EmployeeLastPunchReport/';
       
        
    /// --- Grid Show
    $scope.MainData = [];
    $scope.loadGrid = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.MainData.length != 0) {
            $scope.destroy();
        }
       
            var ColumnList = [
                { field: 'EmployeeCode', width: 150, headerText: "EmployeeCode", type: "string" },
                { field: 'EmployeeName', width: 150, headerText: "Employee Name", type: "string" },
                { field: 'Plant', width: 150, headerText: "PlantName", type: "string" },
                { field: 'Entity', width: 100, headerText: "Entity", type: "string" },
                { field: 'DOJ', width: 150, headerText: "DOJ", type: "string" },
                { field: 'Department', width: 150, headerText: "Department", type: "string" },
                { field: 'Section', width: 150, headerText: "Section", type: "string" },
                { field: 'SubSection', width: 150, headerText: "SubSection", type: "string" },
                { field: 'Designation', width: 150, headerText: "Designation", type: "string" },
                { field: 'TenureMonth', width: 150, headerText: "Tenure (In Months)", type: "string" },
                { field: 'EmployeeCurrentStatus', width: 150, headerText: "EmployeeCurrentStatus", type: "string" },
                { field: 'LastWorkDate', width: 150, headerText: "LastPunch Date", type: "string" },
                { field: 'LastIn', width: 150, headerText: "LastPunch Time", type: "string" },
              
            ];

            $http({
                method: 'GET',
                url: $scope.path + 'GetSummaryData',
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
    $scope.loadGrid();

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
        parameters.push({ "Key": "EmpId", "Value": getString(filteredRecords, "EmpId") });
        applyFilters(parameters);

       
    }

 

    function applyFilters(parameters) {
              
        $http({
            method: 'POST',
            url: $scope.path + 'GetPrintReport',
            data: {
                EmpId: parameters[0].Value
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