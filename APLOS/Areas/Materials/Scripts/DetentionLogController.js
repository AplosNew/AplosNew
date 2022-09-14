'use strict';
DetentionLogController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function DetentionLogController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Detention Log";
    $scope.Action = 'Save';
    $scope.path = 'Materials/DetentionLog/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getStorage = $scope.path + 'StorageSql';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'Delete';
   

}