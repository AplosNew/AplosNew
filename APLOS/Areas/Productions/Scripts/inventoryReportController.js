'use strict';
inventoryReportController.$inject = ["commonMessage", "$scope", "$rootScope", "$http", "$filter", "bankService", "$controller"];
function inventoryReportController(commonMessage, $scope, $rootScope, $http, $filter, bankService, $controller) {
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $scope.detailModel = {
         InventoryMaterialId: null
        , MaterialMasterId: null
        , MaterialMasterName: null
        , ArticleId: null
        , ArticleName: null
        
    };

    $rootScope.title = "Inventory Report";
    $scope.businessProcesses = '';
    $scope.setMaterialMasterData = function (ob) {
        $scope.detailModel.MaterialMasterId = ob.Id;
        $scope.detailModel.MaterialMasterName = ob.UserName;

        angular.element(document.querySelector('#materialmastersearchpopup')).modal('hide');
    };
    $scope.selectarticle = function (ob) {
        try {
            $scope.detailModel.ArticleId = ob.Id;
            $scope.detailModel.ArticleName = ob.StandardName;
            manualValidation('div_ar', false);
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };
   
    $scope.getBankLedgerReport = function () {
        $scope.$broadcast('show-errors-check-validity');    
        location.href = 'productions/InventoryReport/GetInventoryReport?materialId=' + $scope.detailModel.MaterialMasterId + '&articleId=' + $scope.detailModel.ArticleId;
     };
}