'use strict';
DailyProductionDisplayController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DailyProductionDisplayController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.path = 'IE/DailyProductionDisplay/'
    $rootScope.title = 'Daily Production Display';
    $rootScope.FormName = 'Daily Production';
    $rootScope.PlantName = 'Sadma Fashion(Line-1)';

    $scope.productionData = [{ hour: 'H-1', quantity: 50 }, { hour: 'H-2', quantity: 100 }, { hour: 'H-3', quantity: 120 },
    { hour: 'H-4', quantity: 50 }, { hour: 'H-5', quantity: 100 }, { hour: 'H-6', quantity: 120 },
    { hour: 'H-7', quantity: 50 }, { hour: 'H-8', quantity: 100 }, { hour: 'H-9', quantity: 120 },];


    $scope.dataHeight = 300;
    $scope.DataRowHeight = 200;
    $scope.FooterRowHeight = 100;
    function calc() {
        var mainBlockRatio = 0.8;//80% of usable area
        var bottomBlockRatio = 0.2;//20% of usable area
        var MainBlockPerRowData = 6;

        var x = $(document);
        var docHeight = $(document).height();
        var templateHeaderHeight = $('#templateHeader').height();
        var usableArea = docHeight - templateHeaderHeight - 10;

        var numberOfRows = 3;
        if ($scope.productionData.length / MainBlockPerRowData > numberOfRows)
            numberOfRows = parseInt($scope.productionData.length / MainBlockPerRowData);

        $scope.DataRowHeight = parseInt((usableArea * mainBlockRatio) / numberOfRows);
        $scope.dataHeight = parseInt(usableArea * mainBlockRatio);
        $scope.FooterRowHeight = parseInt(usableArea * bottomBlockRatio);
    }

    setInterval(function () {
        calc();
    }, 1000);
}