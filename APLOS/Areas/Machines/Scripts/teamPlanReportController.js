'use strict';
teamPlanReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function teamPlanReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "TeamPlanReport";
    $scope.Action = 'Save';
    $scope.path = 'Machines/TeamPlanReport/';
    $scope.savePlannedUrl = $scope.path + 'createPlanned';
    $scope.saveResponsibleUrl = $scope.path + 'createResponsible';
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
   /* date.setDate(date.getDate() + 7);*/
    var firstDay = new Date(y, m, 1);
    $scope.status = {
        Id: null,
        FromDate: $filter('dateFiltering')(firstDay, 'dd-MM-yyyy'),
        ToDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
        TeamName: null,
        Entity: null,
        Employee: null,
        BudgetCode: null,
        TeamCategory: null,
        ActivityCategory: null
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.TeamNameList=[];
    $scope.GetTeamNameList = function () {
        $http({
            method: 'GET',
            url: 'Machines/TeamPlanReport/GetTeamNameList'
        }).then(function successCallback(response) {
            $scope.TeamNameList = response.data;
        });
    }
    $scope.GetTeamNameList();

    $scope.EntityList = [];
    $scope.GetEntityList = function (TeamId) {
        $http({
            method: 'GET',
            url: 'Machines/TeamPlanReport/GetEntityList?TeamId='+TeamId
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        });
    }
    $scope.TeamCategoryList = [];
    $scope.GetTeamCategoryList = function (TeamId) {
        $http({
            method: 'GET',
            url: 'Machines/TeamPlanReport/GetTeamCategoryList?TeamId=' + TeamId
        }).then(function successCallback(response) {
            $scope.TeamCategoryList = response.data;
        });
    }

    $scope.BudgetCodeList = [];
    $scope.GetBudgetCodeList = function (TeamId) {
        $http({
            method: 'GET',
            url: 'Machines/TeamPlanReport/GetBudgetCodeList?TeamId=' + TeamId
        }).then(function successCallback(response) {
            $scope.BudgetCodeList = response.data;
        });
    }

    $scope.EmployeeList = [];
    $scope.GetEmployeeList = function (TeamId) {
        $http({
            method: 'GET',
            url: 'Machines/TeamPlanReport/GetEmployeeList?TeamId=' + TeamId
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;
        });
    }

    $scope.ActivityCategoryList = [];
    $scope.GetActivityCategoryList = function (EmpId) {
        $http({
            method: 'GET',
            url: 'Machines/TeamPlanReport/GetActivityCategoryList?EmpId=' + EmpId
        }).then(function successCallback(response) {
            $scope.ActivityCategoryList = response.data;
        });
    }
   
    //#endregion
}

