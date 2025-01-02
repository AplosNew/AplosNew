'use strict';
FabricGroupingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FabricGroupingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Fabric Grouping';
    $scope.Action = 'Save';
    $scope.fabricPendingDetailList = [];
    $scope.path = 'Materials/FabricRoll/';
   
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.getPendingData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetFabricRollMaster",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.fabricPendingDetailList = response.data;
        });
    }
    $scope.getPendingData();

    $scope.fabricPendingChildListList = [];
    $scope.GetFabricRollChildList = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetFabricRollChildList?FabricRollManagementMasterId=" + $scope.masterId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.fabricPendingChildListList = response.data;
        });
    }

    $scope.Get = function (args) {
        $scope.masterId = args.data.Id;
       
        $scope.GetFabricRollChildList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.ShowModal = function () {
        angular.element(document.querySelector('#entryPopUp')).modal('show');
    }

    $scope.CloseModal = function () {
        angular.element(document.querySelector('#entryPopUp')).modal('hide');
    }


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.FabricGroupingNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

}