'use strict';
lineEmployeeDateReportController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function lineEmployeeDateReportController(fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Line Operator Date Report";
    $scope.Action = 'Save';
    $scope.lineEmployeeAssigns = [];
    $scope.employeeAssignList = [];
    $scope.lineEmployeeDateOb = {
        FromDate: null,
        ToDate: null
    };
    $scope.getEmpReport = function () {
        location.href = '/OrderManagements/LineEmployeeAssign/ReportEmployee?fromdate=' + $filter('dateFiltering')($scope.lineEmployeeDateOb.FromDate, 'dd-MM-yyyy') + '&todate=' + $filter('dateFiltering')($scope.lineEmployeeDateOb.ToDate, 'dd-MM-yyyy');
    };
}
