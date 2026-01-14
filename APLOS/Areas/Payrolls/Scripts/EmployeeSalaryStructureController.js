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

    $scope.model2 = { Adjustment: false, Promotion: false, Increment: false }

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
    $scope.employeeList = [];
    $scope.LoadEmployeeDataForGrid = function () {
        try {
            $http.get($scope.getUnApprovedEmpListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                    else {
                        $scope.employeeList = [];
                        $scope.employeeList = response.data;
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

    $scope.EmpSalaryOpenHeadCurrent = [];
    $scope.EmpSalaryInfo = {};
    $scope.Get = function (data) {
        try {
            $http.get('Payrolls/EmployeeSalaryRuleSetup/GetEmployeeSalaryData?empId=' + data.rowData.SystemId + '&designationId=' + data.rowData.GivenDesignationId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                    else {
                        $scope.EmpSalaryInfo = response.data[0];
                        $scope.EmpSalaryOpenHeadCurrent = response.data;
                        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');

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
            $scope.EmpSalaryInfoNew = {};
            if (baseService.arrayLength($scope.EmpSalaryOpenHeadCurrent) < 0) {
                throw "Define Designation in Salary Rule Setup.";
            }
            $scope.EmpSalaryInfoNew.SystemID = null;
            $scope.EmpSalaryInfoNew.EmpInfoSystemID = $scope.EmpSalaryInfo.EmpInfoSystemID;
            $scope.EmpSalaryInfoNew.PlantId = $scope.EmpSalaryInfo.PlantId;
            $scope.EmpSalaryInfoNew.GroupID = $scope.EmpSalaryInfo.CompanyGroupId;
            $scope.EmpSalaryInfoNew.EffectiveDate = $scope.EmpSalaryInfo.EffectiveDate;
            $scope.EmpSalaryInfoNew.NextDueDate = $scope.EmpSalaryInfo.NextDueDate;
            $scope.EmpSalaryInfoNew.IsApproved = false;
            $scope.EmpSalaryInfoNew.EmployeeSalaryRuleSetupId = $scope.EmpSalaryInfo.EmployeeSalaryRuleSetupId;
            $scope.newList = [];
            var ob = {};
            for (var i = 0; i < $scope.EmpSalaryOpenHeadCurrent.length; i++) {
                ob.SystemID = null;
                ob.SalaryID = null;
                ob.SalaryHeadID = $scope.EmpSalaryOpenHeadCurrent[i].SalaryHeadID;
                ob.EntryCurrencyID = null;
                ob.EntryAmount = $scope.EmpSalaryOpenHeadCurrent[i].Amount;
                ob.DefineCurrencyID = null;
                ob.DefineAmount = $scope.EmpSalaryOpenHeadCurrent[i].Amount;
                ob.AmtDefinitionCurrencyID = null;
                ob.AmtDefinitionRate = null;
                ob.SalaryCategory = null;
                ob.SequenceNo = $scope.EmpSalaryOpenHeadCurrent[i].Sequence;

                $scope.newList.push(ob);
                ob = {};
            }

            $scope.IncrementHistory.SystemID = null;
            $scope.IncrementHistory.EmpSystemID = $scope.EmpSalaryInfo.EmpInfoSystemID;
            $scope.IncrementHistory.FromGivenDesignationId = $scope.EmpSalaryInfo.GivenDesignationId;
            $scope.IncrementHistory.FromBudgetCode = $scope.EmpSalaryInfo.BudgetCode;
            $scope.IncrementHistory.FromLegalDesignationId = $scope.EmpSalaryInfo.LegalDesignationId;
            $scope.IncrementHistory.ToGivenDesignationId = $scope.budgetCodeChangeNew.GivenDesignationId;
            $scope.IncrementHistory.ToBudgetCode = $scope.budgetCodeChangeNew.BudgetCode;
            $scope.IncrementHistory.ToLegalDesignationId = $scope.budgetCodeChangeNew.LegalDesignationId;
            $scope.IncrementHistory.ToEffectiveDate = $scope.EmpSalaryInfo.EffectiveDate;
            $scope.IncrementHistory.IncrementType = "Fresh Entry";

            $http({
                method: 'POST',
                url: 'Payrolls/EmployeeSalaryRuleSetup/CreateSalary',
                data: { 'master': $scope.EmpSalaryInfoNew, 'data': $scope.newList, 'IncrementHistory': $scope.IncrementHistory},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.EmpSalaryOpenHeadCurrent = [];
                    $scope.EmpSalaryInfo = {};
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }




}