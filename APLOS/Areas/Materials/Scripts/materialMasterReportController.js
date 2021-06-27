'use strict';
function MaterialMasterReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    // #region ****Initial****
    $rootScope.title = "Material Master Report";
    $scope.path = 'Materials/materialmaster/';
    // #endregion


    // #region ****Scope Material Master Report***
    $scope.materialMasterReport = {
        MaterialTypeId: null,
        WithArticle: false
    };
    $scope.materialMasterReportNew = angular.copy($scope.materialMasterReport);
    // #endregion


    // #region ddl
    $http({
        method: 'GET',
        url: 'Materials/materialtype/getcbo/',
    }).then(function successCallback(response) {
        $scope.materialTypeList = response.data;
    });
    // #endregion



    $scope.getMasterReport2 = function () {
       
        try {
            
            var file_src = $scope.path + 'MaterialMasterReport2?MaterialTypeId=' + $scope.materialMasterReportNew.MaterialTypeId + '&Article=' + $scope.materialMasterReportNew.WithArticle;;
                $rootScope.report(file_src);

           

        } catch (e) {

        }


    }






    // #region *****Report*******
    $scope.selectMessage = '';
    $scope.materialMasterReport = function () {
        if ($scope.materialMasterReportNew.MaterialTypeId == null) {
            $scope.selectMessage = 'Select Material Type';
        }
        else {
            $scope.selectMessage = '';
            location.href = 'Materials/materialmaster/materialmasterreport?materialTypeId=' + $scope.materialMasterReportNew.MaterialTypeId + '&withSubmaterial=' + $scope.materialMasterReportNew.WithSubmaterial;
        }
    };
    // #endregion
};
MaterialMasterReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
