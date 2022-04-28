'use strict';
ResidenceMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ResidenceMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Residence Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/ResidenceMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Create';    
    $scope.deleteUrl = $scope.path + 'delete/';   
    baseService.init($scope.getListUrl);

    $scope.PlantList = [];
    $scope.ResidenceGroupList = [];
/*
    $scope.getPlant = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getPlant",
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
        });
    }

    $scope.getResidenceGroup = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getResidenceGroup",
            dataType: 'JSON',
        }).then(function successcallback(response) {
            $scope.ResidenceGroupList = response.data;
        })
    }
    */
   /* $scope.PlantId = null;
    $scope.selectPlant = function (e) {
        $scope.PlantId = e.data.Id;
    }

    $scope.ResidenceGroupId = null;
    $scope.selectResidenceGroup = function (e) {
        $scope.ResidenceGroupId = e.data.Id;
    }*/
}