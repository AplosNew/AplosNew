'use strict';
maintenanceStatusDetailsController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function maintenanceStatusDetailsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "MaintenanceStatusDetails";
    $scope.Action = 'Save';
    $scope.path = 'Machines/MaintenanceStatusDetails/';
    $scope.saveUrl = $scope.path + 'create';

    $scope.status = {
        Id: null,
        ToDate:null
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.MaintenanceStatusDetailsList = [];
    $scope.View = function () {
        $http({

            method: 'Get',
            url: 'Machines/MaintenanceStatusDetails/LoadMaintenanceStatusDetailsList?ToDate=' + $scope.statusNew.ToDate
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusDetailsList = response.data;
            var gridObj = $("#GridMaintenanceStatusDetails").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.MaintenanceStatusSummaryList = [];
    $scope.ViewSummary = function () {
        $http({

            method: 'Get',
            url: 'Machines/MaintenanceStatusDetails/LoadMaintenanceStatusSummaryList?ToDate=' + $scope.statusNew.ToDate
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusSummaryList = response.data;
            var gridObj = $("#GridMaintenanceStatusSummary").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
}