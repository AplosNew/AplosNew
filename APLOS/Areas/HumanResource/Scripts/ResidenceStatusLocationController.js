'use strict';
ResidenceStatusLocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ResidenceStatusLocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Residence Status Loacation';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/ResidenceStatusLocation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    // All List Variables are here for dropdown
    $scope.PlantList = [];
    $scope.LocationList = [];
    $scope.ResidenceGroupIdList = [];
    $scope.ResidenceCategoryList = [];
    $scope.ResidenceSubCategoryList = [];
    $scope.BlockList = [];

    $scope.getPlant = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getPlant',
        }).then(function success(response) {
            $scope.PlantList = response.data;
        });
    }
    $scope.getPlant();

    $scope.getLocation = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getLocation',
        }).then(function success(response) {
            $scope.LocationList = response.data;
        });
    }
    $scope.getLocation();

    $scope.getResidenceGroup = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getResidenceGroup',
        }).then(function success(response) {
            $scope.ResidenceGroupIdList = response.data;
        });
    }
    $scope.getResidenceGroup();

    $scope.getResidenceCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getResidenceCategory',
        }).then(function success(response) {
            $scope.ResidenceCategoryList = response.data;
        });
    }
    $scope.getResidenceCategory();

    $scope.getResidenceSubCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getResidenceSubCategory',
        }).then(function success(response) {
            $scope.ResidenceSubCategoryList = response.data;
        });
    }
    $scope.getResidenceSubCategory();

    $scope.getBlock = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getBlock',
        }).then(function success(response) {
            $scope.BlockList = response.data;
        });
    }
    $scope.getBlock();

}