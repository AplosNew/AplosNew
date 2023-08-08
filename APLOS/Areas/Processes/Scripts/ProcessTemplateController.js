'use strict';
ProcessTemplateController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function ProcessTemplateController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $scope.title = 'Process Template'
    $scope.path = 'Processes/ProcessTemplate/';
    $scope.Action = 'Save';

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    // #endregion TAB CHANGE
}