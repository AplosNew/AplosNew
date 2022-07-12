'use strict';
OTCompensatoryAllocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OTCompensatoryAllocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'OT Compensatory Allocation';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/OTCompensatoryAllocation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    // All Lists Are Here
    $scope.EntityList = [];
    $scope.EmployeeTypeList = [];
    $scope.DepartmentList = [];
    $scope.SectionList = [];
    $scope.SubSectionLIst = [];
    $scope.UserGroup = [];

    $scope.ModelTemp = {
        Id: null,
       Duration:null,

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    // All get function
    $scope.getEntity = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEntity",
            dataType: 'JSON'

        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        })
    }
    $scope.getEntity();

    $scope.getEmployeeType = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEmployeeType",
            dataType:'JSON',
        }).then(function succesCalback(response) {
            $scope.EmployeeTypeList = response.data;
        })
    }
    $scope.getEmployeeType();

    $scope.getDepartment = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getDepartment",
            dataType:'JSON',
        }).then(function succesCalback(response) {
            $scope.DepartmentList = response.data
        })
    }
    $scope.getDepartment();

    $scope.getSection = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getSection",
            dataType: 'JSON',
        }).then(function succesCalback(response) {
            $scope.SectionList = response.data
        })
    }
    $scope.getSection();

    $scope.getSubSection = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getSubSection",
            dataType: 'JSON',
        }).then(function succesCalback(response) {
            $scope.SubSectionList = response.data
        })
    }
    $scope.getSubSection();


   
}