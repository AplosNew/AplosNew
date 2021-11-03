'use strict';
EmployeeBudgetCategoryDepartmentController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeBudgetCategoryDepartmentController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.employeeBudgetCategoryDepartment = {
        Id: null,
        CompanyGroupId: null,
        DepartmentId: null,
        EmployeeBudgetCategoryId: null,
        AddedDate: new Date(),
        UpdatedBy: null,
        UpdatedDate: new Date()
    };
    /**********CBO*************/
    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });
    $http.get('employees/EmployeeBudgetCategory/GetCbo')
        .then(function (result) {
            $scope.employeeBudgetCategoryCboList = result.data;
            console.log('aaa', result);
        });
    $scope.departmentList = [];
    $scope.getDepartment = function () {
        $http.get('employees/EmployeeBudgetCategoryDepartment/GetDepartmentWithCompanyGroupList')
            .then(function (result) {
                $scope.departmentList = result.data.Rows;
                for (var i = 0; i < $scope.departmentList.length; i++) {
                    if ($scope.departmentList[i].Id !== null) {
                        $scope.departmentList[i].Flag = true;
                    } else {
                        $scope.departmentList[i].Flag = false;
                    }
                }
            });
    }
    /************/
    //Save
    function employeeBudgetCategoryDepartmentSaved(list) {
        $scope.employeeBudgetCategoryDepartmentSavedList = [];
        try {
            for (var i = 0; i < list.length; i++) {
                if (list[i].Flag) {
                    if (list[i].EmployeeBudgetCategoryId === null) {
                        throw 'Budget Category can not be empty!! on <b>( ' + list[i].UserName + ')</b>';
                    }
                    $scope.employeeBudgetCategoryDepartmentSavedList.push(list[i]);
                }
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.Save = function () {
        try {
            employeeBudgetCategoryDepartmentSaved($scope.departmentList);
            if ($scope.employeeBudgetCategoryDepartmentSavedList.length === 0) {
                throw 'Select at least one department.';
            }
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'employees/employeeBudgetCategoryDepartment/create',
                    data: { 'employeeBudgetCategoryDepartment': $scope.employeeBudgetCategoryDepartmentSavedList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getDepartment();
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //Deleting Rows from EmployeeBudgetCategoryDepartmentList
    //
}