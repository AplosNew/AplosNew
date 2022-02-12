'use strict';
FGInventoryStockReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FGInventoryStockReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'FG Inventory Stock Report';
    $scope.path = 'Productions/FGInventoryStockReport/';

    $scope.downloadgriddataUrl = 'GridReports/Download';


    $scope.getStocksReport = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetStocksReport",
            
            data: {
                //'ToDate': $scope.selectedValues.ToDate, 'FromDate': $scope.selectedValues.FromDate,
                //'type': $rootScope.typeVal, 'group': $rootScope.groupVal, 'value': $scope.search, 'column': $scope.searchBy,
                'ToDate': $scope.ToDate, 'FromDate': $scope.FromDate
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
}