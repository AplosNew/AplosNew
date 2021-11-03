'use strict';
function TestingStandardReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    // #region ****Initial****
    $rootScope.title = "Testing Standard Report";
    $scope.path = 'Setups/testingstandard/';
    // #endregion

    // #region ****Scope Testing Standard Report***
    $scope.testingStandardReport = {
        Testing: 'WithTesting'
    };
    $scope.testingStandardReportNew = angular.copy($scope.testingStandardReport);
    // #endregion

    // #region ddl

    // #endregion

    // #region *****Report*******
    $scope.testingStandardReport = function () {
        location.href = 'Setups/testingstandard/testingstandardreport?testing=' + $scope.testingStandardReportNew.Testing;
    };
    // #endregion
};
TestingStandardReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];