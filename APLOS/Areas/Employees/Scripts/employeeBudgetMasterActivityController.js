'use strict';
employeeBudgetMasterActivityController.$inject = ['commonMessage', '$rootScope', '$scope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$compile', 'cboService', '$window'];
function employeeBudgetMasterActivityController(commonMessage, $rootScope, $scope, baseService, $routeParams, $location, $http, $filter, $compile, cboService, $window) {
    $rootScope.title = 'Budget Master Activity';

    $scope.positionAllowance = {
        Id: null,
        EmployeeId: null,
        EmployeeName: null,
        BudgetMasterId: null,
        BudgetMasterName: null,
        Active: true
    };

    if (!baseService.isUndefinedOrNull($routeParams.employeeId)) {
        $scope.positionAllowance.EmployeeId = $routeParams.employeeId;
        $scope.positionAllowance.EmployeeName = $routeParams.employeeName;
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
        baseService.init('employees/EmployeeInformation/BudgetMasterActivityResponsiblePerson', null, null, null, 'GLGeneralInfoName', 'GLGeneralInfoName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.employeeId = $scope.positionAllowance.EmployeeId;
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
            , EmployeeId: $scope.positionAllowance.EmployeeId
            , BudgetMasterId: data.BudgetMasterId
            , BudgetMasterActivityId: data.BudgetMasterActivityId
            , Active: event.currentTarget.checked
        };

        $http({
            method: 'POST',
            url: 'employees/EmployeeInformation/SaveBudgetMasterActivity',
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