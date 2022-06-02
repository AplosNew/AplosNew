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

    /*
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
    */

    // POP OPEN
    $scope.selectEmployee = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

    // POP CLOSED
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

    // All List

    $scope.EntityList = [];
    $scope.CategoryList = [];
    $scope.DepartmentList = [];
    $scope.MachineList = [];
    $scope.ObservedByList = [];
    $scope.TagList = [];
    $scope.PersonList = [];
    $scope.ProcessList = [];
    $scope.MachineRefList = [];
    $scope.ResponsiblePersonList = [];
    $scope.ProcessList = [];

    // ALL GET FUNCTIONS
    $scope.getEntity = function () {
        $http({
            method: 'POST',
           
            url: $scope.path + 'getEntity',
        }).then(function success(response) {
            $scope.EntityList = response.data;
        });
    }

    $scope.getEntity();
  
    $scope.getObservedBy = function () {
        $http({
            method: 'POST',
            data: {
                
            },
            url: $scope.path + 'getObservedBy',
        }).then(function success(response) {
            $scope.ObservedByList = response.data;
        });
    }

    $scope.getCategory = function () {
        $http({
            method: 'POST',
            data: {

            },
            url: $scope.path + 'getCategory',
        }).then(function success(response) {
            $scope.CategoryList = response.data;
        });
    }

    $scope.getTag = function () {
        $http({
            method: 'POST',
            data: {

            },
            url: $scope.path + 'getTag',
        }).then(function success(response) {
            $scope.TagList = response.data;
        });
    }

    $scope.getDepartment = function () {
        $http({
            method: 'POST',
            data: {

            },
            url: $scope.path + 'getDepartment',
        }).then(function success(response) {
            $scope.DepartmentList = response.data;
        });
    }

    $scope.getProcess = function () {
        $http({
            method: 'POST',
            data: {

            },
            url: $scope.path + 'getProcess',
        }).then(function success(response) {
            $scope.ProcessList = response.data;
        });
    }

    $scope.getMachine = function () {
        $http({
            method: 'POST',
            data: {

            },
            url: $scope.path + 'getMachine',
        }).then(function success(response) {
            $scope.MachineList = response.data;
        });
    }

    $scope.getMachineRef = function () {
        $http({
            method: 'POST',
            data: {

            },
            url: $scope.path + 'getMachineRef',
        }).then(function success(response) {
            $scope.MachineRefList = response.data;
        });
    }

    $scope.getResponsiblePerson = function () {
        $http({
            method: 'POST',
            data: {

            },
            url: $scope.path + 'getResponsiblePerson',
        }).then(function success(response) {
            $scope.ResponsiblePersonList = response.data;
        });
    }

    
}