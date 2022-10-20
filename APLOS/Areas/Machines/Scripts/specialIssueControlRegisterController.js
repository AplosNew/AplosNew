'use strict';
specialIssueControlRegisterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function specialIssueControlRegisterController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Special Issue Control Report';
    $scope.path = 'Machines/SpecialIssueControlRegister/';
    $scope.SpecialIssueRegisters = {
        Shift:null,
        ReportFormat: 'Excel'
    };
   
    $scope.ShiftList = [];
    $scope.GetShiftList = function () {
        $http({
            method: 'GET',
            url: 'Machines/SpecialIssueControlRegister/GetShiftList'
        }).then(function successCallback(response) {
            $scope.ShiftList = response.data;
        });
    }
    $scope.GetShiftList();

    $scope.IssueControlMasterList = [];
    $scope.LoadSpecialIssueMasterList = function () {
        $http({
            method: 'Get',
            url: 'Machines/SpecialIssueControlRegister/LoadSpecialIssueMasterList'
        }).then(function successCallback(response) {
            $scope.IssueControlMasterList = response.data;
            var gridObj = $("#GridSpecialIssueControlMaster").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        });
    }
    //$scope.LoadSpecialIssueMasterList();
}