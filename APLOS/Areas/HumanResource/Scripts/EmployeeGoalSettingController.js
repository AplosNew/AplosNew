'use strict';
EmployeeGoalSettingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeGoalSettingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Goal Setting';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/EmployeeGoalSetting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    
    // All Lists
    $scope.EGSChildList = [];
    
    // ALL GET FUNCTIONS
    
    $scope.PerformanceYearList = [];
    $scope.SelectPerformanceYearId = null;
    $scope.getPerformancePeriod = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getPerformancePeriod',
        }).then(function success(response) {
            $scope.PerformanceYearList = response.data;
            $scope.SelectPerformanceYearId = $scope.PerformanceYearList[0].Value;
        })
    }

    $scope.getPerformancePeriod();


    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEmployee',

        }).then(function success(resp) {
            $scope.EmployeeList = resp.data;
        })
    }

    $scope.getEmployee();

    $scope.PerformanceGroupList = [];
    $scope.getPMSMaster = function () {
        $http({
            method: "POST",
            url: $scope.path + 'getPMSMaster',
            
            dataType: 'JSON',
        }).then(function success(res) {
            $scope.PerformanceGroupList = res.data;
            for (var i = 0; i <= $scope.PerformanceGroupList.length; i++) {
                $scope.getEGSList($scope.PerformanceGroupList[i].PMSId)
            }
            
        })
    }
    

    $scope.selectEmployee = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

    $scope.SelectedEmployeeId = null;
    $scope.EmpId = null;
    $scope.perfYear = null;
    $scope.selEmp = function (e) {
        $scope.SelectedEmployeeId = e.data.SystemId;
        $scope.EmpId = e.data.EmployeeId;
        $scope.perfYear = document.getElementById("ddperfYear").value;
        console.log($scope.EmpId)
        if ($scope.perfYear != "?" && $scope.SelectedEmployeeId != null) {

            document.getElementById("PerformanceGroupList").style.cssText = "dispplay:block";
            $scope.getPMSMaster();
        } 
        
        
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

    // POP CLOSED
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

   

    $scope.getEGSList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEGSList",
            
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EGSChildList = response.data;
        });
    }
    
    // ALL GET FUNCTIONS CLOSED

    // SAVE FUNCTIONS
    $scope.save = function () {

    }

    // Open Popup for Performance group
    $scope.SaveEmployeeGoalSettingChild = function () {
        angular.element(document.querySelector('#PerfGrPop')).modal('show')
    }
    // SAVE FUNCTIONS CLOSED
    
}