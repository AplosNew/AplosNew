'use strict';
manpowerBudgetBudgetMasterController.$inject = ['commonMessage', '$rootScope', '$scope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$compile', 'cboService', '$window'];
function manpowerBudgetBudgetMasterController(commonMessage, $rootScope, $scope, baseService, $routeParams, $location, $http, $filter, $compile, cboService, $window) {
    $rootScope.title = 'Budget Master';

    $scope.positionAllowance = {
        Id: null,
        ManpowerBudgetId: null,
        ManpowerBudgetName: null,
        Active: true
    };

    if (!baseService.isUndefinedOrNull($routeParams.id)) {
        $scope.positionAllowance.ManpowerBudgetId = $routeParams.id;
        $scope.positionAllowance.ManpowerBudgetName = $routeParams.name;
        onChange();
        $routeParams.id = null;
        $routeParams.name = null;
    }

    $scope.searchByList = [
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        },
        {
            'name': 'Budget Category',
            'value': 'BudgetCategory'
        },
        {
            'name': 'Budget SubCategory',
            'value': 'BudgetSubCategory'
        },
        {
            'name': 'Budget',
            'value': 'BudgetItem'
        },
        {
            'name': 'Budget Type',
            'value': 'BudgetType'
        }
    ];
    function onChange() {
        baseService.init('Organizations/ManpowerBudget/BudgetMasterResponsiblePerson', null, null, null, 'GLGeneralInfoName', 'GLGeneralInfoName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.manpowerBudgetId = $scope.positionAllowance.ManpowerBudgetId;
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
            , SourceType: 'BudgetMaster'
            , ManpowerBudgetId: $scope.positionAllowance.ManpowerBudgetId
            , BudgetMasterId: data.BudgetMasterId
            , Active: event.currentTarget.checked
        };

        $http({
            method: 'POST',
            url: 'Organizations/ManpowerBudget/SaveBudgetMaster',
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