'use strict';
ManualOTReportNewController.$inject = ['$window',"addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ManualOTReportNewController($window, addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.OTReportList = [];
    $scope.SearchedOTManualReportsList = [];
    $scope.PlantList = [];
    $scope.EntityList = [];

    $scope.path = 'Attendances/ManualOTReportNew/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';

    baseService.init($scope.getListUrl);

    $scope.searchBy = "EmployeeCode"; $scope.search = "";


    $scope.searchByList = [{ value: 'EmployeeCode', name: "Employee Code" }, { value: 'OThour', name: "OT hour" }];


    // #region ddl
 

    $scope.GetPlantList = function () {
        $scope.PlantList = [];
        $http({
            method: 'GET',
            url: 'Attendances/ManualOTReportNew/getplant/'
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
            for (var p = 0; p < $scope.PlantList.length; p++) {
                if ($scope.PlantList[p].Value == $window.plantId) {
                    $scope.OTManualReport.PlantId = $scope.PlantList[p].Value;
                }
                    
            }
            $scope.GetEntity();
        });
    }
    $scope.GetPlantList();
   

    $scope.GetEntity = function () {
        $scope.EntityList = [];
        $http({
            method: 'GET',
            url: 'Attendances/ManualOTReportNew/getentity?PlantId=' + $scope.OTManualReport.PlantId
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        });
    }

    // #end region

    var d = new Date();

    var hh = d.getHours();
    var mm = d.getMinutes();
    mm = (mm < 10 ? '0' + mm : mm);
    var ss = d.getSeconds()

    //   var _Time = hh + ":" + mm + ":" + ss;
    var _Time = hh + ":" + mm;

    $scope.ModelTemp = {
        Id: null,
        EmpSystemId: null,
        ToDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        FromDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        APDEmpWorkDate: null,
        PlantId: null,
        EntityId: null,
  
    };
    $scope.OTManualReport = Object.assign({}, $scope.ModelTemp);

    //$scope.ValidateToDate = function () {

    //    try {

    //        if (new Date() < new Date($scope.OTManualReport.ToDate)) {
    //            $scope.OTManualReport.ToDate = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
    //            throw 'To Date should not be greater than Current date.';
    //        }
    //        if (new Date($scope.OTManualReport.ToDate) < new Date($scope.OTManualReport.FromDate)) {
    //            $scope.OTManualReport.ToDate = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
    //            throw 'To Date should not be less than From date.';
    //        }

    //    }
    //    catch (e) {
    //        ShowResult(e, "failure");
    //    }

    //}

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.OTManualReport = Object.assign({}, $scope.ModelTemp);
        $scope.SearchedOTManualReportsList = [];
        $scope.EntityList = [];
        $scope.GetPlantList();
        $scope.EnableDisableShift();
    }

    $scope.SearchOTEmpReport = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
            $scope.SearchedOTManualReportsList = [];
            $http({
                method: 'POST',
                data: { PlantId: $scope.OTManualReport.PlantId, EntityId: $scope.OTManualReport.EntityId, FromDate: $scope.OTManualReport.FromDate, ToDate: $scope.OTManualReport.ToDate },
                url: 'Attendances/ManualOTReportNew/getsearchedotemp/'
            }).then(function successCallback(response) {
                $scope.SearchedOTManualReportsList = response.data;
                $scope.EnableDisableShift();
            });
        }
      

    }

    // OT Manual Report

    // Enable Disable 
    $scope.enable = false;
    $scope.EnableDisableShift = function () {
        if (baseService.arrayLength($scope.SearchedOTManualReportsList) > 0)
            $scope.enable = true;
        else
            $scope.enable = false;
    }

    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.GenerateReport = function () {
        var reportFormat = "Excel";
        var FilteredData = [];
        $scope.GetFilteredData = [];
        var gridobj = $("#SearchedOTManualReportsTab").data("ejGrid");
        FilteredData = gridobj.getFilteredRecords();
        if (FilteredData.length == 0) {
            FilteredData = $scope.SearchedOTManualReportsList;
        }

        var parameters = [];
        
        parameters.push({ "Key": "PlantId", "Value": getString(FilteredData, "PlantId") });
        parameters.push({ "Key": "EntityId", "Value": getString(FilteredData, "EntityId") });
        parameters.push({ "Key": "Code", "Value": getString(FilteredData, "Code") });

        parameters.push({ "Key": "DivisionId", "Value": getString(FilteredData, "DivisionId") });
        parameters.push({ "Key": "DepartmentId", "Value": getString(FilteredData, "DepartmentId") });
        parameters.push({ "Key": "SectionId", "Value": getString(FilteredData, "SectionId") });

        parameters.push({ "Key": "SubSectionId", "Value": getString(FilteredData, "SubSectionId") });
        parameters.push({ "Key": "DesignationId", "Value": getString(FilteredData, "DesignationId") });
        parameters.push({ "Key": "APDEmpWorkDate", "Value": getString(FilteredData, "APDEmpWorkDate") });


        applyFilters(parameters);

    }

    function applyFilters(parameters) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetOTManualReport',
            data: {
                From: $scope.OTManualReport.FromDate, To: $scope.OTManualReport.ToDate,
                PlantId: parameters[0].Value, EntityId: parameters[1].Value, Code: parameters[2].Value, 
                DivisionId: parameters[3].Value, DepartmentId: parameters[4].Value, SectionId: parameters[5].Value, SubSectionId: parameters[6].Value,
                DesignationId: parameters[7].Value, APDEmpWorkDate: parameters[8].Value
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

    //var getString = function (data, column) {
    //    var string = "''";
    //    var collection = [];

    //    for (var i = 0; i < data.length; i++) {
    //        if (collection.includes(data[i][column]) == false) {
    //            string += ",'" + data[i][column] + "'";
    //            collection.push(data[i][column]);
    //        }
    //    }
    //    return string;
    //}

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

                collection.push(data[i][column]);
            }
        }
        return kk;
    };

}