'use strict';
GeneralContractReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function GeneralContractReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Contract Report';
    $scope.ModelList = [];
    $scope.path = 'Administration/GeneralContractReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.Action = 'Save';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Update';
    $scope.deleteUrl = 'Administration/GeneralContractItemMaster/Delete'


}