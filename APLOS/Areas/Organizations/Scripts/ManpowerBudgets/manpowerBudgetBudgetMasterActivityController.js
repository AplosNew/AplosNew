'use strict';
manpowerBudgetBudgetMasterActivityController.$inject = ['commonMessage', '$rootScope', '$scope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$compile', 'cboService', '$window'];
function manpowerBudgetBudgetMasterActivityController(commonMessage, $rootScope, $scope, baseService, $routeParams, $location, $http, $filter, $compile, cboService, $window) {
    $rootScope.title = 'Budget Master Activity';

    $scope.positionAllowance = {
        Id: null,
        ManpowerBudgetId: null,
        ManpowerBudgetName: null,
        BudgetMasterId: null,
        BudgetMasterName: null,
        Active: true
    };

    if (!baseService.isUndefinedOrNull($routeParams.manpowerBudgetId)) {
        $scope.positionAllowance.ManpowerBudgetId = $routeParams.manpowerBudgetId;
        $scope.positionAllowance.ManpowerBudgetName = $routeParams.manpowerBudgetName;
        $scope.positionAllowance.BudgetMasterId = $routeParams.budgetMasterId;
        $scope.positionAllowance.BudgetMasterName = $routeParams.budgetMasterName;
        onChange();
        $routeParams.manpowerBudgetId = null;
        $routeParams.manpowerBudgetName = null;
    }

    $scope.searchByList = [
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        },
        {
            'name': 'Budget',
            'value': 'BudgetCodeName'
        },
        {
            'name': 'Activity',
            'value': 'ActivityCodeName'
        }
    ];

    function onChange() {
        baseService.init('Organizations/ManpowerBudget/BudgetMasterActivityResponsiblePerson', null, null, null, 'GLGeneralInfoName', 'GLGeneralInfoName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.manpowerBudgetId = $scope.positionAllowance.ManpowerBudgetId;
            $rootScope.parameters.budgetMasterId = $scope.positionAllowance.BudgetMasterId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.positionAllowanceList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    }

    $scope.Save = function (event, data) {
        var entity = {
            Id: data.Id
            , SourceType: 'BudgetMasterActivity'
            , ManpowerBudgetId: $scope.positionAllowance.ManpowerBudgetId
            , BudgetMasterId: data.BudgetMasterId
            , BudgetMasterActivityId: data.BudgetMasterActivityId
            , Active: event.currentTarget.checked
        };

        $http({
            method: 'POST',
            url: 'Organizations/ManpowerBudget/SaveBudgetMasterActivity',
            data: { entity },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else
                ShowResult(response.data.Message, 'success');
        });
    };
}