'use strict';

GatePassRegisterController.$inject = ['$scope', '$rootScope',  '$http'];
function GatePassRegisterController( $scope,$rootScope, $http) {

    $rootScope.title = "Gate Pass Register";
    $scope.path = "Products/GatePassRegister/";
    $scope.gatePassRegisterList = [];
    $scope.getGatePassRegister = function () {
        $http({
            method: 'GET',
            url: $scope.path + "getGatePassRegister",
        }).then(function successCallback(response) {
            $scope.gatePassRegisterList = [];
            $scope.gatePassRegisterList = response.data;
        });
    }

    $scope.getGatePassRegister();

    //Gate Pass Report Excel
    $scope.gatePassReportExcel = function () {

        try {
            var file_src = $scope.path + "GatePassReportExcel";
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    //Gate Pass Report Pdf
    $scope.gatePassReportPdf = function () {
        debugger;
        try {
            var file_src = $scope.path + "GatePassReportPdf";
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
}