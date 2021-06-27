'use strict';
positionBudgetMasterController.$inject = ['commonMessage', '$rootScope', '$scope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$compile', 'cboService', '$window'];
function positionBudgetMasterController(commonMessage, $rootScope, $scope, baseService, $routeParams, $location, $http, $filter, $compile, cboService, $window) {
    $rootScope.title = 'Budget Master';

    $scope.positionAllowance = {
        Id: null,
        PositionId: null,
        PositionName: null,
        Active: true
    };

    if (!baseService.isUndefinedOrNull($routeParams.id)) {
        $scope.positionAllowance.PositionId = $routeParams.id;
        $scope.positionAllowance.PositionName = $routeParams.name;
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
        baseService.init('Organizations/Position/BudgetMasterResponsiblePerson', null, null, null, 'GLGeneralInfoName', 'GLGeneralInfoName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.positionId = $scope.positionAllowance.PositionId;
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
            , PositionId: $scope.positionAllowance.PositionId
            , BudgetMasterId: data.BudgetMasterId
            , Active: event.currentTarget.checked
        };

        $http({
            method: 'POST',
            url: 'Organizations/Position/SaveBudgetMaster',
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