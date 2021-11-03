'use strict';
WeighingScaleReportController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function WeighingScaleReportController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Weighing-Scale Report';


    $scope.path = 'Productions/WeighingScaleReport/';

    $scope.selectedValues = {  
        StatusId: null,
        Purpose: null,
    };

    $scope.StatusList = [];
    $scope.GetStatus = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetStatus',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {

            }
            else {
                $scope.StatusList = response.data;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    $scope.GetStatus();

 
    $scope.clearFliters = function () {
        $scope.selectedValues = {};
    }

 
    /// --- Grid Show
    $scope.MainData = [];
    $scope.loadGrid = function () {
        $scope.$broadcast('show-errors-check-validity');
       if( $scope.MainData.length != 0)
        {
            $scope.destroy();
        }
        if ($scope.General.$valid) {

            if ($scope.selectedValues.Purpose == "For User") {
                $scope.x = 1;

                var ColumnList = [
                    { field: 'ProductCode', width: 120, headerText: "Product Code", type: "string" },
                    { field: 'LotNo', width: 100, headerText: "LotNo", type: "string" },
                    { field: 'PO', width: 100, headerText: "PO", type: "string" },

                ];
            }
            else {
                $scope.x = 0;
                var ColumnList = [
                { field: 'ProductCode', width: 120, headerText: "Product Code", type: "string" },
              ];

            }
         
           
            $http({
                method: 'GET',
                url: $scope.path + 'GetData?Status=' + $scope.selectedValues.StatusId + '&purp=' + $scope.selectedValues.Purpose,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.MainData = response.data.DATA;
                    $scope.ExtraColumns = response.data.Columns;

                    for (var i = 0; i < response.data.Columns.length; i++) {
                        ColumnList.push({ field: response.data.Columns[i], width: 170, headerText: response.data.Columns[i], type: "string", allowFiltering: false });
                    }
                    if ($scope.x == 1) {
                        ColumnList.push(

                            { field: 'Remarks', width: 150, headerText: "Remarks", type: "string", allowFiltering: false },
                            { field: 'DeliveryDate', width: 150, headerText: "Delivery Date", type: "string", allowFiltering: false },
                            { field: 'SOQty', width: 120, headerText: "SOQty", type: "string", allowFiltering: false },
                            { field: 'SO', width: 150, headerText: "SO", type: "string" },
                            { field: 'Product', width: 150, headerText: "Product", type: "string" },
                            { field: 'Material', width: 180, headerText: "Material", type: "string" },
                            { field: 'MaterialCode', width: 120, headerText: "Material Code", type: "string" },
                            { field: 'MasterOrderNo', width: 100, headerText: "MasterOrderNo", type: "string" },
                            { field: 'Customer', width: 150, headerText: "Customer", type: "string" },
                            { field: 'OrderStatus', width: 150, headerText: "Production Status", type: "string", allowFiltering: false });
                    }

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
                    if ($scope.MainData.length != 0)
                    {
                        x.style.display = "block";
                    }
                    else
                    {
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
        parameters.push({ "Key": "SO", "Value": getString(filteredRecords, "SO") });
        parameters.push({ "Key": "ProductCode", "Value": getString(filteredRecords, "ProductCode") });
        parameters.push({ "Key": "PO", "Value": getString(filteredRecords, "PO") });
        parameters.push({ "Key": "ProductId", "Value": getString(filteredRecords, "ProductId") });
        parameters.push({ "Key": "MaterialId", "Value": getString(filteredRecords, "MaterialId") });
        parameters.push({ "Key": "LotNo", "Value": getString(filteredRecords, "LotNo") });
        parameters.push({ "Key": "MaterialCode", "Value": getString(filteredRecords, "MaterialCode") });
        parameters.push({ "Key": "CustomerId", "Value": getString(filteredRecords, "CustomerId") });
        parameters.push({ "Key": "MasterOrderNo", "Value": getString(filteredRecords, "MasterOrderNo") });
       
        applyFilters(parameters);

       
    }

    function applyFilters(parameters) {

        $http({
            method: 'POST',
            url: $scope.path + 'GetPrintReport',
            data: {
                Status: $scope.selectedValues.StatusId, 
                SO: parameters[0].Value, ProductCode: parameters[1].Value,
                PO: parameters[2].Value, Product: parameters[3].Value, Material: parameters[4].Value,
                LotNo: parameters[5].Value, MaterialCode: parameters[6].Value, Customer: parameters[7].Value,
                MasterOrderNo: parameters[8].Value, purp: $scope.selectedValues.Purpose
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


}