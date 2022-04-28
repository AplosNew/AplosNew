'use strict';
GoalSettingApprovalController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function GoalSettingApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Goal Setting Approval';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/GoalSettingApproval/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.deleteUrl = $scope.path + 'Delete/';
    baseService.init($scope.getListUrl);

    $scope.ModelNew = {
        RoBudget: null,
        PerformancePeriod:null,
    };

    $scope.PerformancePeriodList = [];
    $scope.getPerformncePeriod = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getPerformancePeriod',

        }).then(function success(resp) {
            $scope.PerformancePeriodList = resp.data;
        })
    }
    $scope.getPerformncePeriod();
    // PERFORMANCE PERIOD POP OPEN
    $scope.selectPerfPeriod = function () {

        angular.element(document.querySelector('#performceperiodPop')).modal('show');
    }

   

    $scope.PerformancePeriod = null;
    $scope.selPerformacePeriod = function (e) {
        $scope.PerformancePeriod = e.data.Id;
        angular.element(document.querySelector('#performceperiodPop')).modal('hide');

    }

    $scope.MenPowerList = [];
    $scope.getMenPower = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getMenPower',

        }).then(function success(resp) {
            $scope.MenPowerList = resp.data;
        })
    }
    $scope.getMenPower();

    $scope.selRoBudget = function (e) {
        $scope.RoBudgetCode = e.data.ROBudgetCode;
        angular.element(document.querySelector('#ROBudgetPop')).modal('hide');
    }

    // RO BUDGET POP OPEN
    
    $scope.selectROBudget = function () {

        angular.element(document.querySelector('#ROBudgetPop')).modal('show');
    }
    $scope.SearchROPP = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetROPP",
            data: {
                'ROBudget': $scope.RoBudgetCode,
                'PPId': $scope.PerformancePeriod,
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            //ClearFields(response.data);

        });
        $scope.GetEmployeeGoalData();
    }

    $scope.EGList = [];
    $scope.GetEmployeeGoalData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEmployeeGoalData",
            data: {
                'PerformancePeriod': $scope.PerformancePeriod,
                'Empid': $scope.ModelList.EmployeeId,
            },
            dataType: 'JSON',
        }).then(function successCallback(res) {
            $scope.EGList = res.data;
        });
    }
    /*$scope.GetEmployeeGoalData();*/

    // PERFORMANCE PERIOD POP OPEN
    $scope.OpenEG = function () {

        angular.element(document.querySelector('#ROPop')).modal('show');
    }

   /* $scope.EmployeeId = null;
    $scope.selROPP = function () {

        $http({
            method: 'POST',
            url: $scope.path + "getEG",
            data: {

            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.EmployeeId = response.data[0].EmployeeId
        })
    }
    */

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'GetROPP';
        $scope.RoBudgetCode = null
        $scope.PerformancePeriod = null
        
    }
}