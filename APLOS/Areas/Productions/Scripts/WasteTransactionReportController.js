'use strict';
WasteTransactionReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function WasteTransactionReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Waste Transaction Report';
    $scope.ModelList = [];
    $scope.path = 'Productions/WasteTransactionReport/';

    $scope.EntityId = null;
    $scope.ToDate = null;
    $scope.FromDate = null;

    $scope.EntityList = [];

    $scope.getsE = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEntity',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EntityList = resp.data;
        });
    }
    $scope.getsE();


    $scope.getData = function () {
        if (angular.isUndefinedOrNull($scope.EntityId)) {
            ShowResult("Please Select the Entity!", 'failure');
            throw ('Invalid Request!!');
        }
        if (angular.isUndefinedOrNull($scope.FromDate) || angular.isUndefinedOrNull($scope.ToDate)) {
            ShowResult("Please Select the Date Range!", 'failure');
            throw ('Invalid Request!!');
        }

        $http({
            method: 'POST',
            url: $scope.path + 'getData',
            data: {'EntityId':$scope.EntityId , 'ToDate':$scope.ToDate , 'FromDate':$scope.FromDate},
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ModelList = resp.data;
        });

    }




   

}