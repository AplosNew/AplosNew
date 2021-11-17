'use strict';
MachineLayoutReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MachineLayoutReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Machine Layout Report";

    $scope.DailyProductionTarget = {
        Id: null,
        DailyProductionTargetID: null,
        Line: null,
        PRNo: null,
        MaterialMasterArticleId: null,
        MaterialMasterId: null,
        Manpower: null,
        SMV: null,
        TotalHour: null,
        PlantId: null,
        EntityId: null,
        ProcessId: null,
        ProductionDate: $filter("date")(Date.now(), 'dd-MMM-yyyy'),

    };
    $scope.DailyProductionTargetNew = Object.assign({}, $scope.DailyProductionTarget);

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.DailyProductionTargetNew.EntityId = $scope.entityList[0].Value;
                $scope.loadProcessList($scope.DailyProductionTargetNew.EntityId);
            }
        });
    };
    $scope.getAllEntities();

    $scope.processList = [];
    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
            if (baseService.arrayLength(result) === 1) {
                $scope.DailyProductionTargetNew.ProcessId = $scope.processList[0].Value;

            }
        });
    };

    $scope.DailyTargetList = [];
    $scope.getDailytarget = function () {
        try {
            if (angular.isUndefinedOrNull($scope.DailyProductionTargetNew.EntityId))
                throw 'Plase select entity';

            if (angular.isUndefinedOrNull($scope.DailyProductionTargetNew.ProcessId))
                throw 'Plase select process';

            if (angular.isUndefinedOrNull($scope.DailyProductionTargetNew.ProductionDate))
                throw 'Plase select target date';
            $http({

                method: 'GET',
                url: 'Productions/DailyTarget/GetDailyTarget?EntityId=' + $scope.DailyProductionTargetNew.EntityId + '&ProcessId=' + $scope.DailyProductionTargetNew.ProcessId + '&ProductionDate=' + $scope.DailyProductionTargetNew.ProductionDate,
            }).then(function successCallback(response) {
                $scope.DailyTargetList = response.data;
            }
            )
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.CalculateTotalQuantity = function (args) {
        for (var i = 0; i < $scope.DailyTargetList.length; i++) {
            $scope.DailyTargetList[i].Quantity = (dbl($scope.DailyTargetList[i].QuantityPerHour) * dbl($scope.DailyTargetList[i].TotalHour)).toFixed(0);

        }
        var gridObj = $("#GridDTargetList").data("ejGrid");
        gridObj.refreshContent();
        //gridObj.refreshTemplate();
    }
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.DailyProductionTarget = {}
        $scope.DailyTargetList = [];
        $scope.SOItemList = [];
    }
}