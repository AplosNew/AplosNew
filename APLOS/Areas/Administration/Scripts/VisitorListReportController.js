'use strict';
VisitorListReportController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function VisitorListReportController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Visitor List Report';
    $scope.path = 'Administration/VisitorListReport/';

    $scope.selectedValues = {
        FromDate: null,
        ToDate: null,
        InDone: false,
        OutDone:false
    };

 
    $scope.clearFliters = function () {
        $scope.selectedValues = {
            FromDate: null,
            ToDate: null,
            InDone: false,
            OutDone: false
        };
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
                { field: 'Id', width: 150, headerText: "Id", type: "string",visible:false },
                { field: 'VisitorName', width: 150, headerText: "Visitor Name", type: "string" },
                { field: 'VisitorType', width: 120, headerText: "Visitor Type", type: "string" },
                { field: 'VisitorCategory', width: 130, headerText: "Visitor Category", type: "string" },
                { field: 'VisitorLocation', width: 150, headerText: "Visitor Location", type: "string" },
                { field: 'ToMeet', width: 150, headerText: "To Meet", type: "string" },
                { field: 'Purpose', width: 150, headerText: "Purpose", type: "string" },
                { field: 'InDate', width: 120, headerText: "InDate", type: "string" },
                { field: 'InTime', width: 120, headerText: "InTime", type: "string" },             
                { field: 'OutDate', width: 120, headerText: "OutDate", type: "string" },
                { field: 'OutTime', width: 120, headerText: "OutTime", type: "string" },
                { field: 'AddedBy', width: 150, headerText: "AddedBy", type: "string" }
            ];
           
            $http({
                method: 'POST',
                url: $scope.path + 'GetData',
                data: {
                    In: $scope.selectedValues.InDone, Out: $scope.selectedValues.OutDone,
                    FromDate: $scope.selectedValues.FromDate, ToDate: $scope.selectedValues.ToDate
                },
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
        parameters.push({ "Key": "Id", "Value": getString(filteredRecords, "Id") });
        applyFilters(parameters);
              
    } 

    function applyFilters(parameters) {

        $http({
            method: 'POST',
            url: $scope.path + 'GetPrintReport',
            data: {
                In: $scope.selectedValues.InDone, Out: $scope.selectedValues.OutDone,
                FromDate: $scope.selectedValues.FromDate, ToDate: $scope.selectedValues.ToDate
                , Id: parameters[0].Value
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