'use strict';
teamPlanReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function teamPlanReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "TeamPlanReport";
    $scope.Action = 'Save';
    $scope.path = 'Machines/TeamPlanReport/';
    $scope.savePlannedUrl = $scope.path + 'createPlanned';
    $scope.saveResponsibleUrl = $scope.path + 'createResponsible';
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrl = 'GridReports/Download';
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

    function Validation() {
        try {
            CheckField("To Date", $scope.statusNew.ToDate);
        } catch (ex) {
            throw ex;
        }
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] is required.";
            }

        } catch (ex) {
            throw ex;
        }
    }

    $scope.TeamPlanReportList = [];
    $scope.View = function () {
        try {
            Validation();
            $http({

                method: 'Get',
                url: 'Machines/TeamPlanReport/LoadTeamPlanReportList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate
            }).then(function successCallback(response) {
                $scope.TeamPlanReportList = response.data;
                var gridObj = $("#GridTeamPlanReport").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            }
            )
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.TeamPlanReport = function () {
        var dataList = [];
        var g = $("#GridTeamPlanReport").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.TeamPlanReportList;
        }

        $scope.fileName = "Team Plan Report";

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpd,
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
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

    //$scope.TeamPlanReport = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + 'XlsTeamPlanReport?todate=' + $scope.statusNew.ToDate + '&fromDate=' + $scope.statusNew.FromDate + '&teamName=' + $scope.statusNew.TeamName + '&employeeId=' + $scope.statusNew.Employee,
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {

    //            $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
    //        }
    //    }, function errorCallback(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    });

    //};
   
    //#endregion
}

