'use strict';
EmployeeSalaryStructureController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeSalaryStructureController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Salary Structure';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    //$scope.path = 'Payrolls/EmployeeSalaryRuleSetup/';

    $scope.path = 'humanresource/employeepromotionNew/';
    $scope.getApprovedEmpListUrl = $scope.path + 'GetSalaryStrcApprovedEmployeeList';
    $scope.getUnApprovedEmpListUrl = $scope.path + 'GetSalaryStrcUnApprovedEmployeeList';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.SetCheckforAdjustment = function () {
        if ($scope.model2.Adjustment === true) {
            $scope.model2.Promotion = false;
            $scope.model2.Increment = false;
        }
        if ($scope.model2.Promotion === true) {
            $scope.model2.Adjustment === false;
        }
        if ($scope.model2.Increment === true) {
            $scope.model2.Adjustment === false;
        }
    }

    $scope.LoadEmployeeDataForGrid = function () {
        try {
            $http.get($scope.getUnApprovedEmpListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                    else {
                        $scope.employees = [];
                        $scope.employees = response.data;
                        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');

                    }
                    function errorCallBack(response) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                });


        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };

    $scope.Save = function () {
        try {
            if ($scope.model2.Promotion === true) {
                $scope.IncrementHistory.IncrementType = "Confirmation with Promotion";
                $scope.IncrementHistory.IsConfirmation = true;
                $scope.Update();
            }
            if ($scope.model2.Increment === true) {

                if ($scope.model2.Promotion === true) {
                    $scope.IncrementHistory.IncrementType = "Confirmation with Increment and Promotion";
                    $scope.IncrementHistory.IsConfirmation = true;

                } else {
                    $scope.IncrementHistory.IncrementType = "Confirmation with Increment";
                    $scope.IncrementHistory.IsConfirmation = true;
                }


                if (baseService.isUndefinedOrNull($scope.EmpSalaryInfo.SalaryRuleMasterSystemID)) {
                    throw "Enter valid Salary Rule Master.";
                }
            }
        } catch (e) {
            $scope.ShowResult(e, "failure");
        }
    }




}