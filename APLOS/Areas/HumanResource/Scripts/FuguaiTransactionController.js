'use strict';
FuguaiTransactionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FuguaiTransactionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Fuguai Transaction';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/FuguaiTransaction/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);


    // ALL POP UPs

     // Observed By
    $scope.OpenEmployeePopUp = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

    
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

    // Entity Master
    $scope.OpenEntityPopUp = function () {

        angular.element(document.querySelector('#EntityPop')).modal('show');
    }

   
    $scope.closeEntityPopUp = function () {
        angular.element(document.querySelector('#EntityPop')).modal('hide');
    }

   // Fuguai Master
    $scope.OpenFuguaiPopUp = function () {

        angular.element(document.querySelector('#FuguaiPop')).modal('show');
    }


    $scope.closeFuguaiPopUp = function () {
        angular.element(document.querySelector('#FuguaiPop')).modal('hide');
    }

}