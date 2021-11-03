'use strict';
JobEvaluationReportController.$inject = ['$window',"addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function JobEvaluationReportController($window, addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
  
    $scope.SearchedJobEvalReportsList = [];
    //$scope.PlantList = [];
    //$scope.EntityList = [];

    $scope.path = 'PerformanceManagement/JobEvaluationReport/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';

    baseService.init($scope.getListUrl);

    $scope.searchBy = "EmployeeCode"; $scope.search = "";


    $scope.searchByList = [{ value: 'EmployeeCode', name: "Employee Code" }, { value: 'OThour', name: "OT hour" }];


    // #region ddl
 

    //$scope.GetPlantList = function () {
    //    $scope.PlantList = [];
    //    $http({
    //        method: 'GET',
    //        url: 'Attendances/ManualOTReport/getplant/'
    //    }).then(function successCallback(response) {
    //        $scope.PlantList = response.data;
    //        for (var p = 0; p < $scope.PlantList.length; p++) {
    //            if ($scope.PlantList[p].Value == $window.plantId) {
    //                $scope.OTManualReport.PlantId = $scope.PlantList[p].Value;
    //            }
                    
    //        }
    //        $scope.GetEntity();
    //    });
    //}
    //$scope.GetPlantList();
   

    //$scope.GetEntity = function () {
    //    $scope.EntityList = [];
    //    $http({
    //        method: 'GET',
    //        url: 'Attendances/ManualOTReport/getentity?PlantId=' + $scope.OTManualReport.PlantId
    //    }).then(function successCallback(response) {
    //        $scope.EntityList = response.data;
    //    });
    //}

    // #end region

    //var d = new Date();

    //var hh = d.getHours();
    //var mm = d.getMinutes();
    //mm = (mm < 10 ? '0' + mm : mm);
    //var ss = d.getSeconds()

    ////   var _Time = hh + ":" + mm + ":" + ss;
    //var _Time = hh + ":" + mm;

    $scope.ModelTemp = {
        Id: null,
        PositionCode: null,
        PositionCodeId: null,
        PositionName: null,
        EmpSystemId: null,
        EmployeeStatus: null,
        EmployeeCode: null,
        ResponsiblePerson: null,
  
    };
    $scope.JobEvaluationReport = Object.assign({}, $scope.ModelTemp);

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
        $scope.JobEvaluationReport = Object.assign({}, $scope.ModelTemp);
        $scope.SearchedJobEvalReportsList = [];
        //$scope.EntityList = [];
        //$scope.GetPlantList();
        $scope.EnableDisableShift();
    }

    $scope.Search = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
            $scope.SearchedJobEvalReportsList = [];
            $http({
                method: 'POST',
                data: { PositionCodeId: $scope.JobEvaluationReport.PositionCodeId, EmpSystemId: $scope.JobEvaluationReport.EmpSystemId},
                url: 'PerformanceManagement/JobEvaluationReport/getsearcheddetails/'
            }).then(function successCallback(response) {
                $scope.SearchedJobEvalReportsList = response.data;
                $scope.EnableDisableShift();
            });
        }
      

    }

    // OT Manual Report

    // Enable Disable 
    $scope.enable = false;
    $scope.EnableDisableShift = function () {
        if (baseService.arrayLength($scope.SearchedJobEvalReportsList) > 0)
            $scope.enable = true;
        else
            $scope.enable = false;
    }

    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.GenerateReport = function () {
     //   var reportFormat = "Excel";
        var FilteredData = [];
        $scope.GetFilteredData = [];
        var gridobj = $("#SearchedJobEvalReportsTab").data("ejGrid");
        FilteredData = gridobj.getFilteredRecords();
        if (FilteredData.length == 0) {
            FilteredData = $scope.SearchedJobEvalReportsList;
        }

        var parameters = [];
        
        parameters.push({ "Key": "PositionCodeId", "Value": getString(FilteredData, "PositionCodeId") });
      
        parameters.push({ "Key": "DivisionId", "Value": getString(FilteredData, "DivisionId") });
        parameters.push({ "Key": "SubDivisionId", "Value": getString(FilteredData, "SubDivisionId") });
        parameters.push({ "Key": "DepartmentId", "Value": getString(FilteredData, "DepartmentId") });
        parameters.push({ "Key": "SectionId", "Value": getString(FilteredData, "SectionId") });

        parameters.push({ "Key": "SubSectionId", "Value": getString(FilteredData, "SubSectionId") });
        parameters.push({ "Key": "DesignationId", "Value": getString(FilteredData, "DesignationId") });


        applyFilters(parameters);

    }

    function applyFilters(parameters) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetJobEvaluationReport',
            data: {
                PositionCodeId: parameters[0].Value, DivisionId: parameters[1].Value, 
                SubDivisionId: parameters[2].Value, DepartmentId: parameters[3].Value, SectionId: parameters[4].Value, SubSectionId: parameters[5].Value,
                DesignationId: parameters[6].Value
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

    // #region Position field

    $scope.PositionList = [];
    $scope.PositionPopUp = function () {
        angular.element(document.querySelector("#PosPopUp")).modal("show");
        $scope.getPosDetailsData();

    }
    $scope.getPosDetailsData = function () {
        $scope.PositionList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.JobEvaluationReport.Id },
            url: $scope.path + 'LoadAllPositionDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.PositionList = response.data;
        });
    }

    $scope.PositionClear = function () {
        $scope.JobEvaluationReport.PositionCodeId = null;
        $scope.JobEvaluationReport.PositionName = null;
        $scope.JobEvaluationReport.PositionCode = null;
    };
    $scope.closePositionPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.setPositionData = function (obj) {
        var data = obj.data;
        $scope.JobEvaluationReport.PositionCode = data.Code;
        $scope.JobEvaluationReport.PositionCodeId = data.Id;
        $scope.JobEvaluationReport.PositionName = data.UserName;
        angular.element(document.querySelector('#PosPopUp')).modal('hide');
    };
    // # end region

    // #region field

    $scope.EmpResPersonList = [];
    $scope.ResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpResPerson")).modal("show");
        $scope.getEmpDetailsData();

    }
    $scope.getEmpDetailsData = function () {
        $scope.EmpResPersonList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.JobEvaluationReport.Id },
            url: $scope.path + 'LoadAllEvaluatorDetails'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.JobEvaluationReport.EmpSystemId = null;
        $scope.JobEvaluationReport.ResponsiblePerson = null;
        $scope.JobEvaluationReport.EmployeeCode = null;
        $scope.JobEvaluationReport.EmployeeStatus = null;

    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.JobEvaluationReport.EmployeeCode = data.Code;
        $scope.JobEvaluationReport.EmpSystemId = data.Id;
        $scope.JobEvaluationReport.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    // # end region
}