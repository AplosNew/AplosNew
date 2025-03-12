'use strict';
accountStatusBIController.$inject = ['commonMessage', '$scope', '$rootScope', 'cboService', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function accountStatusBIController(commonMessage, $scope, $rootScope, cboService, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Account Status";

    $scope.comInfo = {};
    cboService.getCboInterCompany(null, function (result) {
        $scope.companyList = result;
        $scope.comInfo.CompanyId = $scope.companyList[0].CompanyId;
        $scope.comInfo.CompanyName = $scope.companyList[0].CompanyName;
    });

    $scope.viewCredential = function () {
        angular.element(document.querySelector('#biCredentialPopUp')).modal('show');
    }

    $scope.closeViewCredential = function () {
        angular.element(document.querySelector('#biCredentialPopUp')).modal('hide');
    }

    $scope.copyUserId = function () {
        var copyUserText = "it@apopinternational.com";
        navigator.clipboard.writeText(copyUserText);
        $scope.closeViewCredential();
    }

    $scope.copyPassword = function () {
        var copyPassText = "aPOP@123";
        navigator.clipboard.writeText(copyPassText);
        $scope.closeViewCredential();
    }

}