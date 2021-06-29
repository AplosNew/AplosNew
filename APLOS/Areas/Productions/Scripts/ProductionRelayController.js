
'use strict';
ProductionRelayController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProductionRelayController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Production Relay";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.costingTypeses = [];
    $scope.path = 'Productions/ProductionRelay/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.ProductionRelayList = [];
    $scope.ProductionRelayClosedList = [];

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.ProductionRelay = {
        Id: null,
        EntityId: null,
        ProcessId: null,
    };
    $scope.ProductionRelayNew = Object.assign({}, $scope.ProductionRelay);

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.ProductionRelayNew.EntityId = $scope.entityList[0].Value;
                $scope.loadProcessList($scope.ProductionRelayNew.EntityId);
            }
        });
    };
    $scope.getAllEntities();

    $scope.processList = [];
    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
            if (baseService.arrayLength(result) === 1) {
                $scope.ProductionRelayNew.ProcessId = $scope.processList[0].Value;
            }
        });
    };


    $scope.listFromProcessOrSFGInventory = [];
    $scope.GetSFGMovementFromCbo = function (entity) {
        $http({
            method: 'GET',
            url: 'Productions/ProductionRelay/GetProcessFromCbo?entity=' + entity,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //sccuess 
                $scope.listFromProcessOrSFGInventory = response.data;

            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    $scope.changeProcess = function () {
        $scope.Process = $("#Process option:selected").text();
        $scope.Status = null;
        $scope.Status = $.grep($scope.listFromProcessOrSFGInventory, function (item) {
            return item.ProcessId === $scope.ProductionRelayNew.ProcessId;
        })[0].Status;

        for (var i = 0; i < $scope.listFromProcessOrSFGInventory.length; i++) {
            if ($scope.ProductionRelayNew.ProcessId === $scope.listFromProcessOrSFGInventory[i].ProcessId) {
                $scope.ProductionRelayNew.ProductionBookingLevel = $scope.listFromProcessOrSFGInventory[i].ProductionBookingLevel;
                $scope.LotNumberCapture = $scope.listFromProcessOrSFGInventory[i].LotNumberCapture;
                $scope.LotNumberMandatory = $scope.listFromProcessOrSFGInventory[i].LotNumberMandatory;
                $scope.IsFirst = $scope.listFromProcessOrSFGInventory[i].IsFirst;
                $scope.Status = $scope.listFromProcessOrSFGInventory[i].Status;
                $scope.Sequence = $scope.listFromProcessOrSFGInventory[i].Sequence - 1;
                break;
            }
        }
    };

    $scope.getProductionRelay = function () {
        try {

            if (angular.isUndefinedOrNull($scope.ProductionRelayNew.EntityId))
                throw 'Plase select entity';

            if (angular.isUndefinedOrNull($scope.ProductionRelayNew.ProcessId))
                throw 'Plase select process';

            $http({
                method: 'GET',
                url: 'Productions/ProductionRelay/GetProductionRelay?EntityId=' + $scope.ProductionRelayNew.EntityId + '&ProcessId=' + $scope.ProductionRelayNew.ProcessId,
            }).then(function successCallback(response) {
                $scope.ProductionRelayList = response.data;
            })
            $http({
                method: 'GET',
                url: 'Productions/ProductionRelay/GetProductionRelayClosed?EntityId=' + $scope.ProductionRelayNew.EntityId + '&ProcessId=' + $scope.ProductionRelayNew.ProcessId,
            }).then(function successCallback(response) {
                $scope.ProductionRelayClosedList = response.data;
            })
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ProductionRelayAllCheck = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };
    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        for (var i = 0; i < $scope.ProductionRelayList.length; i++) {
            $scope.ProductionRelayList[i].Checked = ChkOrUnchk;
        }

        var gridObj = $("#GridProductionRelay").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.Save = function () {
        try {
                      $scope.ActiveList = [];

            for (var i = 0; i < $scope.ProductionRelayList.length; i++) {
                if ($scope.ProductionRelayList[i].Checked) {
                    $scope.ActiveList.push($scope.ProductionRelayList[i]);
                }

            }
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'ProductionRelayData': $scope.ActiveList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    /*ClearFields(response.data.Sequence);*/
                    $scope.getProductionRelay();
                    /* $scope.GetDetails({ data: { Id: response.data.Data.Id } });*/
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = "Save";
        $scope.ProductionRelay = {}
        $scope.ProductionRelayList = [];
    }

}