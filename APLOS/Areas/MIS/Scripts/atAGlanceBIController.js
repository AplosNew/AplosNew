'use strict';
atAGlanceBIController.$inject = ['commonMessage', '$scope', 'cboService','$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function atAGlanceBIController(commonMessage, $scope, cboService, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "At A Glance";

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

    $scope.copyPassword=function () {
        var copyPassText = "aPOP@123";
        navigator.clipboard.writeText(copyPassText);
        $scope.closeViewCredential();
    }
}