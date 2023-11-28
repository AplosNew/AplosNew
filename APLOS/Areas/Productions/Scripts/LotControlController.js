'use strict';
LotControlController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function LotControlController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'LotControl';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/LotControl/';
    $scope.saveUrl = $scope.path + 'create';


    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.getAllEntities();

    $scope.ProductionOrderList = [];

    $scope.PRSearchColumn = 'Id';
    $scope.PRSearchValue = null;
    $scope.getProductionOrderPopUp = function () {
        $scope.ProductionOrderList = [];
        if (!baseService.isUndefinedOrNull($scope.ModelNew.EntityId)) {
            $http({
                method: 'POST',
                data: {
                    'entityid': $scope.ModelNew.EntityId, 'column': $scope.PRSearchColumn, 'value': $scope.PRSearchValue
                },
                url: 'Materials/MaterialIssueControl/getlist'
            }).then(function successCallback(response) {
                $scope.ProductionOrderList = response.data;
            });
        }
        angular.element(document.querySelector('#POItemPopup')).modal('show');
    };

    $scope.SetPrOData = function ($event) {
        $scope.ModelNew.ProductionOrderId = $event.data.Id;
        $scope.GetPOLotControlSettingData();
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
    }

    $scope.lotControlList = [];
    $scope.GetPOLotControlSettingData = function () {
        try {
            $http({
                method: 'GET',
                url: 'OrderManagements/ProductionOrder/GetPOLotContSettingsData?poId=' + $scope.ModelNew.ProductionOrderId + '&entityId=' + $scope.ModelNew.EntityId
            }).then(function (response) {
                $scope.lotControlList = response.data;

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.tempModel = {};
    $scope.SetSufix = function (args) {
        $scope.tempModel = args.data;
        //var str = $scope.tempModel.UserLotNo;
        //var extention = str.substr(str.indexOf('/') + 1);

        //if (!baseService.isUndefinedOrNull($scope.tempModel.Sufix)) {
        //    if ($scope.tempModel.Sufix != extention) {
        //        $scope.tempModel.UserLotNo = $scope.tempModel.UserLotNo + '/' + $scope.tempModel.Sufix;
        //    }
        //}
        $scope.tempModel.UserLotNo = $scope.tempModel.LotNo + '/' + $scope.tempModel.Sufix;
        var gridObj = $("#GridLC").data("ejGrid");
        gridObj.refreshContent();
        gridObj.refreshTemplate();
    }

    $scope.copymessage_detailconfirmation = null;
    $scope.copyDetail = function (obj) {
        $scope.DetailNew = obj.data;

        $scope.copymessage_detailconfirmation = 'Are you sure want to copy Lot No:' + $scope.DetailNew.UserLotNo + ' ?';
        angular.element(document.querySelector('#confirmCopyDetailPopUp')).modal('show');
    }

    function refreshSerial() {
        for (var j = 0; j < $scope.lotControlList.length; j++) {
            $scope.lotControlList[j].Serial = j;
        }
    }
    $scope.CopyDetailData = function (e) {
        let ob = {};
        Object.assign(ob, e);
        ob.Id = null;
        $scope.lotControlList.splice(e.Serial + 1, 0, ob);
        refreshSerial();
    }


    $scope.XXCopyDetailData = function () {
        $scope.DetailNew.LotQty = $scope.DetailNew.LotQty / 2;
        var ob = {};
        ob.Id = null;
        ob.SeqNo = $scope.DetailNew.SeqNo;
        ob.EntityId = $scope.DetailNew.EntityId;
        ob.LotNo = $scope.DetailNew.LotNo;
        ob.UserLotNo = $scope.DetailNew.UserLotNo;
        ob.ProductionBookingLevel = $scope.DetailNew.ProductionBookingLevel;
        ob.ProductionOrderId = $scope.DetailNew.ProductionOrderId;
        ob.MasterOrderItemId = $scope.DetailNew.MasterOrderItemId;
        ob.SalesOrderId = $scope.DetailNew.SalesOrderId;
        ob.ProcessId = $scope.DetailNew.ProcessId;
        ob.LotArticle = $scope.DetailNew.LotArticle;
        ob.Sufix = $scope.DetailNew.Sufix;
        ob.Process = $scope.DetailNew.Process;
        ob.LotQty = $scope.DetailNew.LotQty / 2;
        ob.OrderQty = $scope.DetailNew.OrderQty;
        ob.PlanQty = $scope.DetailNew.PlanQty;
        ob.SchedulePercentage = $scope.DetailNew.SchedulePercentage;
        ob.Remark = $scope.DetailNew.Remark;

        $scope.lotControlList.push(ob);
        $scope.lotControlList.sort('Process');
        ob = {};
        var gridObj = $("#GridLC").data("ejGrid");
        gridObj.refreshContent();
        gridObj.refreshTemplate();
    }

    $scope.XCopyDetailData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'CopyData?ProductionOrderId=' + $scope.DetailNew.ProductionOrderId + '&Id=' + $scope.DetailNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetPOLotControlSettingData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.SaveRowData = function (obj) {
        try {

            $http({
                method: 'POST',
                url: 'Productions/LotControl/SaveTNCRowData',
                data: { 'data': obj.data },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetPOLotControlSettingData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, "failure");
        }
    };



    function checkExistLot(list, ProcessId, UserLotNo) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProcessId === ProcessId && list[i].UserLotNo === UserLotNo && !baseService.isUndefinedOrNull(list[i].Id)) {
                return true;
            }
        }
        return false;
    }

    $scope.SaveLotSettingData = function () {
        try {

            //for (var i = 0; i < $scope.lotControlList.length; i++) {
            //    var getRow = $filter("filter")($scope.lotControlList, { "ProcessId": $scope.lotControlList[i].ProcessId, "UserLotNo": $scope.lotControlList[i].UserLotNo });
            //    if (getRow.length > 1) {
            //        throw "Process wise Lot No should unique.";
            //    }
            //}
            for (var i = 0; i < $scope.lotControlList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.lotControlList[i].Id)) {
                    if (checkExistLot($scope.lotControlList, $scope.lotControlList[i].ProcessId, $scope.lotControlList[i].UserLotNo)) {
                        throw "Process wise Lot No should unique.";
                    }
                }
            }


            $http({
                method: 'POST',
                url: 'Productions/LotControl/SaveLotSettingData',
                data: { 'data': $scope.lotControlList, 'poId': $scope.ModelNew.ProductionOrderId },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetPOLotControlSettingData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.message_deleteconfirmation = null;
    $scope.removeDetail = function (obj) {

        $scope.DetailNew = obj.data;
        $scope.message_deleteconfirmation = 'Are you sure want to delete permanently [ ' + $scope.DetailNew.UserLotNo + ' ]';
        angular.element(document.querySelector('#confirDeletePopUp')).modal('show');
    }

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'Productions/LotControl/Delete?id=' + $scope.DetailNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetPOLotControlSettingData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };



}