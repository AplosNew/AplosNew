'use strict';
employeeBudgetMasterActivityPhoneController.$inject = ['commonMessage', '$rootScope', '$scope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$compile', 'cboService', '$window'];
function employeeBudgetMasterActivityPhoneController(commonMessage, $rootScope, $scope, baseService, $routeParams, $location, $http, $filter, $compile, cboService, $window) {
    $rootScope.title = 'Budget Master Activity Phone';
    $scope.positionAllowance = {
        Id: null,
        EmployeeId: null,
        EmployeeName: null,
        BudgetMasterId: null,
        BudgetMasterName: null,
        ActivityId: null,
        ActivityName: null,
        Active: true
    };

    if (!baseService.isUndefinedOrNull($routeParams.employeeId)) {
        $scope.positionAllowance.EmployeeId = $routeParams.employeeId;
        $scope.positionAllowance.EmployeeName = $routeParams.employeeName;
        $scope.positionAllowance.BudgetMasterId = $routeParams.budgetMasterId;
        $scope.positionAllowance.BudgetMasterName = $routeParams.budgetMasterName;
        $scope.positionAllowance.ActivityId = $routeParams.activityId;
        $scope.positionAllowance.ActivityName = $routeParams.activityName;
        onChange();
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
        baseService.init('employees/EmployeeInformation/BudgetMasterActivityPhoneResponsiblePerson', null, null, null, 'GLGeneralInfoName', 'GLGeneralInfoName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.employeeId = $scope.positionAllowance.EmployeeId;
            $rootScope.parameters.budgetMasterId = $scope.positionAllowance.BudgetMasterId;
            $rootScope.parameters.activityId = $scope.positionAllowance.ActivityId;
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
            , SourceType: 'BudgetMasterActivityPhone'
            , EmployeeId: $scope.positionAllowance.EmployeeId
            , BudgetMasterId: data.BudgetMasterId
            , BudgetMasterActivityId: data.BudgetMasterActivityId
            , ActivityPhoneId: data.ActivityPhoneId
            , Active: event.currentTarget.checked
        };
        $http({
            method: 'POST',
            url: 'employees/EmployeeInformation/SaveBudgetMasterActivityPhone',
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