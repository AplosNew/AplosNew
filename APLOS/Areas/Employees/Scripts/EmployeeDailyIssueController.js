'use strict';
EmployeeDailyIssueController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','$window'];
function EmployeeDailyIssueController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter,$window) {
    $rootScope.title = 'Daily Issue';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.DailyIssues = [];
    $scope.path = 'employees/EmployeeInformation/';
    $scope.saveUrl = $scope.path + 'CreateDailyIssue';
    $scope.deleteUrl = $scope.path + 'DeleteDailyIssue/';
   
    $scope.searchDIBy = "Category"; $scope.searchDI = "";
    $scope.searchByDIList = [{ value: 'Id', name: "Id" }, { value: 'Category', name: "Category" }, { value: 'IssueType', name: "Issue" }, { value: 'CriticalityLevel', name: "CriticalityLevel" }, { value: 'ResponsiblePerson', name: "ResponsiblePerson" }, { value: 'Remarks', name: "Remarks" }];

    $scope.CriticalityLevelList = [{ Value: 'Critical', Text: "Critical" }, { Value: 'Normal', Text: "Normal" }];

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDailyIssueDataList",
            data: { column: $scope.searchDIBy, value: $scope.searchDI },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DailyIssues = response.data;
        });
    }
    $scope.getData();

    $scope.DailyIssue = {
        Id: null,
        Category: null,
        ResolveTime: null,
        CriticalityLevel: null,
        ResponsiblePerson: null,
        ResponsiblePersonId: null,
        IssueType: null,
        HandleTime: null,
        Remarks: null,
        ShiftId:null
    };

    $scope.DailyIssueNew = Object.assign({}, $scope.DailyIssue);

    $scope.searchBy = "EmployeeCode"; $scope.search = "";
    $scope.searchByList = [{ value: 'SystemId', name: "SystemId" }, { value: 'EmployeeCode', name: "Employee Code" }, { value: 'EmployeeName', name: "Employee Name" }, { value: 'Department', name: "Department" }, { value: 'LegalDesignation', name: "Designation" }];


    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $scope.getEmpData();
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.emppath = 'employees/employeeinformation/';
    $scope.getEmpData = function () {
        $http({
            method: 'Get',
            //url: $scope.emppath + "GetEmployeeDataList",
            url: 'employees/authorizationconfig/getallemployeedata',
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
    }

    $scope.setEmpData = function (obj) {
        var data = obj.data;
        $scope.DailyIssueNew.ResponsiblePersonCode = data.EmployeeCode;
        $scope.DailyIssueNew.ResponsiblePersonId = data.SystemId;
        $scope.DailyIssueNew.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');

    }

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.fixedShitList = [];
    $scope.GetShiftCbo = function () {
        $scope.fixedShitList = [];
        $http.get('employees/EmployeeInformation/GetCboShiftDefinationByPlant?plantId=' + $window.plantId)
            .then(function (response) {
                $scope.fixedShitList = response.data;
            });
    };
    $scope.GetShiftCbo();


    $scope.Get = function (data) {
        $scope.DailyIssueNew = Object.assign({}, data.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'employees/EmployeeInformation/CreateDailyIssue',
                data: { 'data': $scope.DailyIssueNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.DailyIssueNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.DailyIssueNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.DailyIssue = {};
        $scope.DailyIssueNew = {};
    }
}