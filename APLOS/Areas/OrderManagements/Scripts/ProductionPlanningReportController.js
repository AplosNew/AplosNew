'use strict';
ProductionPlanningReportController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProductionPlanningReportController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Production Planning Report';


    $scope.path = 'OrderManagements/ProductionPlanningReport/';

    $scope.selectedValues = {
        PlanningType: null,
        SnapShotType: null,
        FromDate: null,
        ToDate: null,
        SnapshotName: null,
        SnapId:null,
    };

 
    $scope.clearFliters = function () {
        $scope.selectedValues.PlanningType = null;
        $scope.selectedValues.SnapShotType = null;
        $scope.selectedValues.FromDate = null;
        $scope.selectedValues.ToDate = null;        
        $scope.selectedValues.SnapId = null;
    }

    $scope.planningTypesList = [];
    cboService.getEnumCbo('Enum/GetEnumEnumPlanningTypes', function (result) {
        $scope.planningTypesList = result;
    });

 
    /// --- Grid Show
    $scope.MainData = [];
    $scope.loadGrid = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.MainData.length != 0) {
            $scope.destroy();
        }
        if ($scope.General.$valid) {

            var ColumnList = [
                { field: 'Company', width: 150, headerText: "Company", type: "string" },
                { field: 'Plant', width: 150, headerText: "Plant", type: "string" },
                { field: 'Entity', width: 150, headerText: "Entity", type: "string" },
                { field: 'SnapshotName', width: 150, headerText: "Snapshot Name", type: "string" },
                { field: 'SnapshotDate', width: 150, headerText: "Snapshot Date", type: "string" },
                { field: 'Customer', width: 150, headerText: "Customer", type: "string" },
                { field: 'ProductionOrderID', width: 150, headerText: "PO", type: "string" },
                { field: 'WorkCenter', width: 150, headerText: "WorkCenter", type: "string" },
                { field: 'Process', width: 150, headerText: "Process", type: "string" },
                { field: 'Quantity', width: 150, headerText: "Quantity", type: "string", allowFiltering: false },
                { field: 'ProductionHours', width: 150, headerText: "Production Hours", type: "string", allowFiltering: false },
               
            ];
           
            $http({
                method: 'GET',
                url: $scope.path + 'GetSnapShotData?From=' + $scope.selectedValues.FromDate + '&To=' + $scope.selectedValues.ToDate + '&SnapShotType='
                    + $scope.selectedValues.SnapShotType + '&SnapId=' + $scope.selectedValues.SnapId,
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
        parameters.push({ "Key": "CompanyId", "Value": getString(filteredRecords, "CompanyId") });
        parameters.push({ "Key": "SnapDate", "Value": getString(filteredRecords, "SnapshotDate") });
        parameters.push({ "Key": "PlantId", "Value": getString(filteredRecords, "PlantId") });
        parameters.push({ "Key": "EntityId", "Value": getString(filteredRecords, "EntityID") });
        parameters.push({ "Key": "ProcessId", "Value": getString(filteredRecords, "ProcessID") });
        parameters.push({ "Key": "SnapName", "Value": getString(filteredRecords, "SnapshotName") });
        parameters.push({ "Key": "WkCenterId", "Value": getString(filteredRecords, "WorkCenterMasterId") });
        parameters.push({ "Key": "CustomerId", "Value": getString(filteredRecords, "CustomerId") });
        parameters.push({ "Key": "POId", "Value": getString(filteredRecords, "ProductionOrderID") });

        applyFilters(parameters);

       
    }

 

    function applyFilters(parameters) {

        $http({
            method: 'POST',
            url: $scope.path + 'GetPrintReport',
            data: {
                From: $scope.selectedValues.FromDate, To: $scope.selectedValues.ToDate,
                SnapShotType: $scope.selectedValues.SnapShotType, CompanyId: parameters[0].Value,
                SnapDate: parameters[1].Value, PlantId: parameters[2].Value, EntityId: parameters[3].Value,
                ProcessId: parameters[4].Value, SnapName: parameters[5].Value, WkCenterId: parameters[6].Value,
                CustomerId: parameters[7].Value, POId: parameters[8].Value
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


    $scope.SnapNamesList = [];
    $scope.SnapPopUp = function () {
        angular.element(document.querySelector("#SnapPopUp")).modal("show");
        $scope.getSnapNames();

    }
    $scope.getSnapNames = function () {
        $scope.SnapNamesList = [];

        $http({
            method: 'GET',         
            url: $scope.path + 'GetSnapShotNames?SnapShotType=' + $scope.selectedValues.SnapShotType
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.SnapNamesList = response.data.DATA;
            }
        });
    }

    $scope.closeSnapPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.setSnapData = function (obj) {

        var data = obj.data;
        $scope.selectedValues.FromDate = data.MinDate;
        $scope.selectedValues.ToDate = data.MaxDate;
        $scope.selectedValues.SnapId = data.SnapId;
        angular.element(document.querySelector('#SnapPopUp')).modal('hide');
    };



}