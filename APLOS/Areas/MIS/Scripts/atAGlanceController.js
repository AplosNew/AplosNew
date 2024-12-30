'use strict';
atAGlanceController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function atAGlanceController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "At A Glance";
    $scope.index = -1;
    $scope.brands = [];
    $scope.path = 'MIS/brand/';
}