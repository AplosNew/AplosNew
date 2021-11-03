'use strict';
jobCardInformationController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window', 'toaster'];
function jobCardInformationController(fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window, toaster) {
    $rootScope.title = 'Job Card Information';
    $scope.index = -1;
    $scope.path = 'employees/EmployeeInformation/';

      // #region ****Scope Ledger Report***
    $scope.jobCardReport = {
        FromDate: null,
        ToDate: null
    };

    $scope.jobCardReport = function () {
       
        location.href = 'Employees/EmployeeInformation/JobCardReport?fromDate=' + $scope.jobCardReport.FromDate + '&toDate=' + $scope.jobCardReport.ToDate;
        
    };

}