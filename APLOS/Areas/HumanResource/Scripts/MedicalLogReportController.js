'use strict';
MedicalLogReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$interval'];
function MedicalLogReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $interval) {
    $rootScope.title = 'Medical Log Report';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/MedicalLogReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.openEmpPopUp = function () {
        angular.element(document.querySelector('#empPopUpId')).modal('show');

    }

    $scope.closeEmpPopUp = function () {

        angular.element(document.querySelector('#empPopUpId')).modal('hide');

    }
}