'use strict';
PlantInOutControllReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function PlantInOutControllReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Plant In Out Report';
    $scope.path = 'HumanResource/PlantInOutControllReport/';

    $scope.PlantInOutList = []
    $scope.GetPlantInOutGridData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPlantInOutGridData",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PlantInOutList = response.data;

        });
    }
    $scope.GetPlantInOutGridData();
}