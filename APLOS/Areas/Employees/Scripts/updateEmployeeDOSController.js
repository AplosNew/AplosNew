'use strict';
updateEmployeeDOSController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function updateEmployeeDOSController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Update DOS';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.maternityLeaveTransactions = [];
    $scope.path = 'employees/EmployeeDelete/';

    $scope.getEmployeeListUrl = 'Payrolls/FinalSettlement/GetResiginedEmployeelist';

    $scope.EmployeeInformationList = [];
    $scope.LoadEmployeeList = function () {
        try {

            $http.get($scope.getEmployeeListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.EmployeeInformationList = response.data;
                        angular.element(document.querySelector('#dialogEmployeeInfo')).modal('show');
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };   

    $scope.empList = [];
    $scope.resigList = [];
    $scope.GetResiginedEmployee = function (obj) {
        $scope.empList = [];
        $scope.resigList = [];
        $scope.EmpSysId = obj.data.SystemId;
        $http({
            method: 'GET',
            url: 'Payrolls/FinalSettlement/GetResiginedEmployee?empId=' + $scope.EmpSysId
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                $scope.empList = response.data.emp;
                $scope.resigList = response.data.empr;
                angular.element(document.querySelector('#dialogEmployeeInfo')).modal('hide');

            }
        });
    }

    $scope.EmpSysId = null;
    $scope.GetDOSEmployee = function () {
        $scope.empList = [];
        $http({
            method: 'GET',
            url: 'Payrolls/FinalSettlement/GetResiginedEmployee?empId=' + $scope.EmpSysId
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                $scope.empList = response.data.emp;

            }
        });
    }

    $scope.GetDOREmployee = function () {
        $scope.resigList = [];
        $http({
            method: 'GET',
            url: 'Payrolls/FinalSettlement/GetResiginedEmployee?empId=' + $scope.EmpSysId
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                $scope.resigList = response.data.empr;

            }
        });
    }

    $scope.SaveEmployeeDOS = function () {
        try {
            $http({
                method: 'POST',
                url: 'Payrolls/FinalSettlement/UpdateDOS',
                data: { 'data': $scope.empList[0] },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    $scope.GetDOSEmployee();
                }
            }, function errorCallback(response) {
                $scope.ShowResultCustom(response.status.Message, "failure");
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.UpdateEmployeeDOR = function () {
        try {
            $http({
                method: 'POST',
                url: 'Payrolls/FinalSettlement/UpdateDOR',
                data: { 'data': $scope.resigList[0] },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    $scope.GetDOREmployee();
                }
            }, function errorCallback(response) {
                $scope.ShowResultCustom(response.status.Message, "failure");
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Clear = function () {
        $scope.empList = [];
        $scope.resigList = [];
    }


}