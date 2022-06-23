'use strict';
StorageBinAllocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function StorageBinAllocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Storage Bin Allocation';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Materials/StorageBinAllocation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    // All Lists are here
    $scope.StorageLevelList = [];
    $scope.MaterialList = [];
    $scope.MaterialGroupList = [];
    $scope.MaterialTypeList = [];
    $scope.StorageLocationList = [];    
    $scope.MaterialArticleList = [];
    $scope.AccessTypeList = [];

    $scope.getStorageLevel = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getStorageLevel",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.StorageLevelList = response.data;
        })
    }
    $scope.getStorageLevel();

    $scope.getMaterialType = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMaterialType",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialTypeList = response.data;
        })
    }
    $scope.getMaterialType();

    $scope.getMaterialGroup = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMaterialGroup",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialGroupList = response.data;
        })
    }
    $scope.getMaterialGroup();

    $scope.getMaterial = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMaterial",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialList = response.data;
        })
    }
    $scope.getMaterial();

    $scope.getStorageLocation = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getStorageLocation",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.StorageLocationList = response.data;
        })
    }
    $scope.getStorageLocation();

    $scope.getMaterialArticle = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMaterialArticle",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialArticleList = response.data;
        })
    }
    $scope.getMaterialArticle();
    // ALL POP UPs
    $scope.OpenMaterialPopUp = function () {

        angular.element(document.querySelector('#MaterialpopUp')).modal('show');

    }

    $scope.closeMaterialPopUp = function () {
        angular.element(document.querySelector('#MaterialpopUp')).modal('hide');
    }
    // -----------------------------MATERIAL GROUP POPUP------------------------------------------------
    $scope.OpenMaterialGroupPopUp = function () {

        angular.element(document.querySelector('#MaterialGrouppopUp')).modal('show');

    }

    $scope.closeMaterialGroupPopUp = function () {
        angular.element(document.querySelector('#MaterialGrouppopUp')).modal('hide');
    }

    // -----------------------------MATERIAL TYPE POPUP------------------------------------------------
    $scope.OpenMaterialTypePopUp = function () {

        angular.element(document.querySelector('#MaterialTypepopUp')).modal('show');

    }

    $scope.closeMaterialTypePopUp = function () {
        angular.element(document.querySelector('#MaterialTypepopUp')).modal('hide');
    }

    // -----------------------------MATERIAL ARTICLE POPUP------------------------------------------------
    $scope.OpenMaterialArticlePopUp = function () {

        angular.element(document.querySelector('#MaterialArticlepopUp')).modal('show');

    }

    $scope.closeMaterialArticlePopUp = function () {
        angular.element(document.querySelector('#MaterialArticlepopUp')).modal('hide');
    }
}

