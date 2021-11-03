'use strict';
EmployeeServiceVariableController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function EmployeeServiceVariableController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Employee Servic (Variable)';
    $scope.Action = 'Save';
    $scope.path = 'Payrolls/EmployeeServiceVariable/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';


    $scope.FromDate = null;
    $scope.ToDate = null;
    $scope.ServiceName = null;


    $scope.EmpServiceTypeList = [];
    $scope.GetEmpServiceTypeCbo = function () {
        $http({
            method: 'GET',
            url: 'Payrolls/EmployeeServiceVariable/GetEmpServiceTypeCbo/'
        }).then(function successCallback(response) {
            $scope.EmpServiceTypeList = response.data;
        });
    };
    $scope.GetEmpServiceTypeCbo();



    $scope.GetReport = function () {
        var reportFormat = "Excel";
        try {
            var file_src = 'Payrolls/EmployeeServiceVariable/GetEmployeeServiceFixedReport?reportFormat=' + reportFormat + '&FromDate=' + $scope.FromDate + '&ToDate=' + $scope.ToDate + '&Service=' + $scope.ServiceName;
            $rootScope.report(file_src);
        } catch (e) {

        }
    };

}