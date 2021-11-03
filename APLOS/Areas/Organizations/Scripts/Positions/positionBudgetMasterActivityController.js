'use strict';
positionBudgetMasterActivityController.$inject = ['commonMessage', '$rootScope', '$scope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$compile', 'cboService', '$window'];
function positionBudgetMasterActivityController(commonMessage, $rootScope, $scope, baseService, $routeParams, $location, $http, $filter, $compile, cboService, $window) {
    $rootScope.title = 'Budget Master Activity';

    $scope.positionAllowance = {
        Id: null,
        PositionId: null,
        PositionName: null,
        BudgetMasterId: null,
        BudgetMasterName: null,
        Active: true
    };

    if (!baseService.isUndefinedOrNull($routeParams.positionId)) {
        $scope.positionAllowance.PositionId = $routeParams.positionId;
        $scope.positionAllowance.PositionName = $routeParams.positionName;
        $scope.positionAllowance.BudgetMasterId = $routeParams.budgetMasterId;
        $scope.positionAllowance.BudgetMasterName = $routeParams.budgetMasterName;
        onChange();
        $routeParams.positionId = null;
        $routeParams.positionName = null;
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
        baseService.init('Organizations/Position/BudgetMasterActivityResponsiblePerson', null, null, null, 'GLGeneralInfoName', 'GLGeneralInfoName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.positionId = $scope.positionAllowance.PositionId;
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
            , PositionId: $scope.positionAllowance.PositionId
            , BudgetMasterId: data.BudgetMasterId
            , BudgetMasterActivityId: data.BudgetMasterActivityId
            , Active: event.currentTarget.checked
        };

        $http({
            method: 'POST',
            url: 'Organizations/Position/SaveBudgetMasterActivity',
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