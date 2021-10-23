'use strict';
EntireYearPresentDaysSummaryController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EntireYearPresentDaysSummaryController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'PresentDays Summary Report';


    $scope.path = 'Attendances/EntireYearPresentDaysSummary/';

    
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
                { field: 'Department', width: 150, headerText: "Department", type: "string" },
                { field: 'Section', width: 150, headerText: "Section", type: "string" },
                { field: 'SubSection', width: 150, headerText: "SubSection", type: "string" },
                { field: 'DOJ', width: 150, headerText: "DOJ", type: "string" },
                { field: 'Jan', width: 150, headerText: "January", type: "string" },
                { field: 'Feb', width: 150, headerText: "Feburary", type: "string" },             
                { field: 'Mar', width: 150, headerText: "March", type: "string" },
                { field: 'Apr', width: 150, headerText: "April", type: "string" },
                { field: 'May', width: 150, headerText: "May", type: "string" },
                { field: 'June', width: 150, headerText: "June", type: "string" },
                { field: 'July', width: 150, headerText: "Ju;y", type: "string" },
                { field: 'Aug', width: 150, headerText: "August", type: "string" },
                { field: 'Sep', width: 150, headerText: "September", type: "string" },
                { field: 'Oct', width: 150, headerText: "October", type: "string" },
                { field: 'Nov', width: 150, headerText: "November", type: "string" },
                { field: 'Dec', width: 150, headerText: "December", type: "string" },

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
                EmpId: parameters[0].Value,
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