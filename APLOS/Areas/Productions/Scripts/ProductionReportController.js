'use strict';
ProductionReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function ProductionReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Production Report';

    $scope.ModelList = [];
    $scope.path = 'Productions/ProductionReport/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.Date = null;

    $scope.ProductionEntityId = null;
    $scope.ProcessId = null;

    $scope.entityList = [];
    $scope.processList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.getAllEntities();
   

    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
            if (baseService.arrayLength(result) === 1) {
                $scope.ProcessId = $scope.processList[0].Value;
                //$scope.getProdLevel();
                ////default
                //$scope.loadWC($scope.ProcessId, $scope.EntityId, $scope.ProductionShiftId);
            }
        });
    };
    
    $scope.DonwloadReport = function () {
        try {
            $scope.fileName = "ProductionReport.xls";

            if (baseService.isUndefinedOrNull($scope.Date)) {
                throw "Select Date plz!";
            }

            $http({
                method: 'POST',
                url: 'Productions/ProductionReport/ProReport',
                data: {
                    'Date': $scope.Date,
                    'Entity': $scope.ProductionEntityId, 'ProcessId': $scope.ProcessId
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}