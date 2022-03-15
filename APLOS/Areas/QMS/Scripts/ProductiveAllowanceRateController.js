'use strict';
ProductiveAllowanceRateController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function ProductiveAllowanceRateController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Productive Allowance Rate';
    $scope.Action = 'Save';
    $scope.path = 'QMS/ProductiveAllowanceRate/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.getData = function () {
        $http({
            method: "POST",
            url: "/QMS/getData",
            

        })
    }
    

   

}