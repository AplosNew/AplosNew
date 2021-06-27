'use strict';
function ProcessSetReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    // #region ****Initial****
    $rootScope.title = "Process Set Report";
    $scope.path = 'Processes/processset/';
    // #endregion

    // #region ****Scope Process Set Report***
    $scope.processSetReport = {
        Process: 'Process',
        CompanyId: null,
        EntityId: null
    };
    $scope.processSetReportNew = angular.copy($scope.processSetReport);
    // #endregion

    // #region ddl
    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.entityList = [];
    $scope.getEntityList = function () {
        cboService.getCboEntityByCompanyWise(null, $scope.processSetReport.CompanyId, function (result) {
            $scope.entityList = result;
        });
    };
    // #endregion

    $scope.EntityHideShow = false;
    $scope.CompanyHideShowFn = function () {
        if ($scope.processSetReportNew.Process == 'ProcessSet') {
            $scope.EntityHideShow = true
            $scope.processSetReportNew.CompanyId = null;
            $scope.processSetReportNew.EntityId = null;
        }
        else {
            $scope.EntityHideShow = false;
            $scope.processSetReportNew.CompanyId = null;
            $scope.processSetReportNew.EntityId = null;
        }
    }
    $scope.CompanyHideShowFn();

    // #region *****Report*******
    $scope.processSetReport = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.from.$valid) {
            location.href = 'Processes/processset/processsetreport?companyId=' + $scope.processSetReportNew.CompanyId + ' &entityId=' + $scope.processSetReportNew.EntityId + ' &process=' + $scope.processSetReportNew.Process ;
        }
    };
    // #endregion
};
ProcessSetReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
