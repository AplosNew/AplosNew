'use strict';
OrderLineCostingItemServiceMasterMappingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function OrderLineCostingItemServiceMasterMappingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Order Line Costing Item Service Master Map";
    $scope.Action = 'Save';
    $scope.FormulaDetails = [];
    $scope.path = 'Costings/OrderLineCostingItem/';
  

    $scope.ModelList = [];
    $scope.GetData = function () {
        $scope.ModelList = [];
        $http.get("Costings/OrderLineCostingItem/GetList")
            .then(
                function successCallback(response) {
                    $scope.ModelList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };
    $scope.GetData();

    $scope.serviceMasterList = [];
    $scope.GetSMData = function (obj) {
        try {
            if (obj.data.EntryState == 'Calculate') {
                throw "Service Master can't add with Calculated Item.";
            }
            $scope.serviceMasterList = [];
            $scope.OrderLineCostingItemId = obj.data.Id;
            $http({
                method: 'GET',
                url: 'Costings/OrderLineCostingItem/GetOLSMMapDataList?OrderLineCostingItemId=' + $scope.OrderLineCostingItemId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    $scope.serviceMasterList = response.data;
                    angular.element(document.querySelector('#SMPopUp')).modal('show');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.GetSavedSMData = function () {
        $scope.serviceMasterList = [];
        $http({
            method: 'GET',
            url: 'Costings/OrderLineCostingItem/GetOLSMMapDataList?OrderLineCostingItemId=' + $scope.OrderLineCostingItemId
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                $scope.serviceMasterList = response.data;
            }
        });
    }

    $scope.refreshTemplate = function (args) {
        $("#headschk").ejCheckBox({ "change": CheckBoxSelectAllItemWise });
    };
    function CheckBoxSelectAllItemWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSM").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.serviceMasterList.length; i++) {
                $scope.serviceMasterList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSM").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SavedserviceMasterList = [];
    $scope.ClosePopUp = function () {
        angular.element(document.querySelector('#SMPopUp')).modal('hide');
    }

    function checkItemExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ServiceMasterId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.saveUrl = 'Costings/OrderLineCostingItem/CreateOLSMMap';
    $scope.SaveData = function () {
        $scope.SavedserviceMasterList = [];
        for (var i = 0; i < $scope.serviceMasterList.length; i++) {
            if ($scope.serviceMasterList[i].Flag == true) {
                if (checkItemExist($scope.SavedserviceMasterList, $scope.serviceMasterList[i].ServiceMasterId) === false) {
                    var obj = {};
                    obj.Id = $scope.serviceMasterList[i].Id == null ? null : $scope.serviceMasterList[i].Id;
                    obj.OrderLineCostingItemId = $scope.OrderLineCostingItemId;
                    obj.ServiceMasterId = $scope.serviceMasterList[i].ServiceMasterId;
                    obj.Flag = $scope.serviceMasterList[i].Flag;
                    $scope.SavedserviceMasterList.push(obj);
                    obj = {};
                }
            }
        }


        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: { 'data': $scope.SavedserviceMasterList, 'masterId': $scope.OrderLineCostingItemId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSavedSMData();
                angular.element(document.querySelector('#EntityPopup')).modal('hide');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };



}
