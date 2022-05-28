'use strict';
generalLedgerVSfixedAssetsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function generalLedgerVSfixedAssetsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'GL VS FA';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.GLFAList = [];
    $scope.path = 'FixedAssets/FixedAssetRegister/';
    $scope.reportParameters = {
        FromDate: null,
        ToDate: null
        //LCType: 'contract'
    };

    $scope.GetGLFAList = function () {
        try {
              $http({
                    method: 'GET',
                    url: $scope.path + 'GetGLFAListList',
                    dataType: 'JSON'
              }).then(function successCallback(response) {
                  $scope.GLFAList = response.data;
              }),
                  function errorCallBack(response) {
                   ShowResult(response.data.Message, 'failure');
                  }
        }
         catch (e) {

         }
    }
    $scope.GetGLFAList();

    $scope.GLVSfaReport = function () {

        try {

            var file_src = $scope.path + "GLVSfaReport"; 
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

}


