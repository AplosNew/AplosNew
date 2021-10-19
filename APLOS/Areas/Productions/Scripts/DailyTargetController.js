
'use strict';
DailyTargetController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DailyTargetController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Daily Target";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.costingTypeses = [];
    $scope.path = 'Productions/DailyTarget/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';


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


    $scope.listFromProcessOrSFGInventory = [];
    $scope.GetSFGMovementFromCbo = function (entity) {
        $http({
            method: 'GET',
            url: 'Productions/DailyTarget/GetProcessFromCbo?entity=' + entity,
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
            return item.ProcessId === $scope.DailyProductionTargetNew.ProcessId;
        })[0].Status;

        for (var i = 0; i < $scope.listFromProcessOrSFGInventory.length; i++) {
            if ($scope.DailyProductionTargetNew.ProcessId === $scope.listFromProcessOrSFGInventory[i].ProcessId) {
                $scope.DailyProductionTargetNew.ProductionBookingLevel = $scope.listFromProcessOrSFGInventory[i].ProductionBookingLevel;
                $scope.LotNumberCapture = $scope.listFromProcessOrSFGInventory[i].LotNumberCapture;
                $scope.LotNumberMandatory = $scope.listFromProcessOrSFGInventory[i].LotNumberMandatory;
                $scope.IsFirst = $scope.listFromProcessOrSFGInventory[i].IsFirst;
                $scope.Status = $scope.listFromProcessOrSFGInventory[i].Status;
                $scope.Sequence = $scope.listFromProcessOrSFGInventory[i].Sequence - 1;
                break;
            }
        }
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


    $scope.DailyTargetAllCheck = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        for (var i = 0; i < $scope.DailyTargetList.length; i++) {
            $scope.DailyTargetList[i].Active = ChkOrUnchk;
        }

        var gridObj = $("#GridDailyTargetList").data("ejGrid");
        gridObj.refreshContent();
    };



    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');

            for (var i = 0; i < $scope.DailyTargetList.length; i++) {
                if ($scope.DailyTargetList[i].Active) {
                    if (baseService.isUndefinedOrNull($scope.DailyTargetList[i].PRNo) == true) {
                        throw "Please select Production Order No. for '" + $scope.DailyTargetList[i].Line + "'";
                        if (baseService.isUndefinedOrNull($scope.DailyTargetList[i].Manpower) == true) {
                            throw "Manpower is Empty.";
                        }
                    }
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'DailyTargetData': $scope.DailyTargetList, 'TargetDate': $scope.DailyProductionTargetNew.ProductionDate, 'EntityId': $scope.DailyProductionTargetNew.EntityId, 'ProcessId': $scope.DailyProductionTargetNew.ProcessId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    /*ClearFields(response.data.Sequence);*/
                    $scope.getDailytarget();
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
        $scope.DailyProductionTarget = {}
        $scope.DailyTargetList = [];
        $scope.SOItemList = [];
    }


    //search PR
    $scope.SelectedLineForPR = {};
    $scope.SOItemList = [];
    $scope.SearchPRPopup = function (data) {
        $scope.SelectedLineForPR = data;
        if (baseService.isUndefinedOrNull(data.WorkCenterMasterId)) {
            return ShowResult('Please Work Center.', 'failure');
        }
        $scope.SOItemList = [];
        $http.get($scope.path + 'GetProductionOrderPOPUp?entityid=' + $scope.DailyProductionTargetNew.EntityId + '&processId=' + $scope.DailyProductionTargetNew.ProcessId)
            .then(
                function successCallback(response) {
                    $scope.SOItemList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');

    }

    $scope.selectSOItem = function (args) {
        for (var i = 0; i < $scope.DailyTargetList.length; i++) {
            if ($scope.SelectedLineForPR.WorkCenterMasterId == $scope.DailyTargetList[i].WorkCenterMasterId) {
                $scope.DailyTargetList[i].PRNo = args.data.Id;
                $scope.DailyTargetList[i].Material = args.data.Material;
                $scope.DailyTargetList[i].Article = args.data.Article;
                $scope.DailyTargetList[i].MaterialMasterId = args.data.MaterialMasterId;
                $scope.DailyTargetList[i].MaterialMasterArticleId = args.data.ArticleId;
                $scope.DailyTargetList[i].CustomerPONo = args.data.CustomerPONo;
                $scope.DailyTargetList[i].BuyerItemNo = args.data.BuyerItemNo;
                angular.element(document.querySelector('#POItemPopup')).modal('hide');
                break;
            }
        }
        var gridObj = $("#GridDailyTargetList").data("ejGrid");
        gridObj.refreshContent();
        gridObj.refreshTemplate();
    }
    $scope.CalculateTotalQuantity = function (args) {
        for (var i = 0; i < $scope.DailyTargetList.length; i++) {
            $scope.DailyTargetList[i].Quantity = (dbl($scope.DailyTargetList[i].QuantityPerHour) * dbl($scope.DailyTargetList[i].TotalHour)).toFixed(0);

        }
        var gridObj = $("#GridDailyTargetList").data("ejGrid");
        gridObj.refreshContent();
        //gridObj.refreshTemplate();
    }
    $scope.rowDataBound = function rowDataBound(e) {

        if (e.data.IsManual == true)
            e.row.css("background-color", '#d1e5ff');


    }
}