'use strict';
lineEmployeeAssignEditController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function lineEmployeeAssignEditController(fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Line Operator Qty";
    $scope.Action = 'Save';
    $scope.productionList = [];
    $scope.path = 'OrderManagements/LineEmployeeAssign/';
    $scope.getListUrl = $scope.path + 'GetForLineEmpAssignEdit';
    $scope.saveUrl = $scope.path + 'edit';


    $scope.model = {
        Id: null
        , ProductionDate: null
        , LineId: null
        , SalesOrderId: null
        , ShiftId: null
        , ProductionQty: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.lineList = [];
    $scope.getLineCbo = function () {
        $scope.productionList = [];
        cboService.getLineCbo($filter('dateFiltering')($scope.modelNew.ProductionDate, 'dd-MM-yyyy'), function (result) {
            $scope.lineList = result;
        });
    };

    $scope.salesOrderCboList = [];
    $scope.getSalesOrderCbo = function () {
        $scope.productionList = [];
        $scope.modelNew.SalesOrderId = null;
        $scope.modelNew.ShiftId = null;
        cboService.getSalesOrderCbo($filter('dateFiltering')($scope.modelNew.ProductionDate, 'dd-MM-yyyy'), document.getElementById("lineId").options[document.getElementById('lineId').selectedIndex].text, function (result) {
            $scope.salesOrderCboList = result;
        });
    };

    $scope.shiftCboList = [];
    $scope.getShiftCboList = function () {
        $scope.productionList = [];
        $scope.modelNew.ShiftId = null;
        cboService.getShiftCbo($filter('dateFiltering')($scope.modelNew.ProductionDate, 'dd-MM-yyyy'), document.getElementById("lineId").options[document.getElementById('lineId').selectedIndex].text, document.getElementById("salesOrderId").options[document.getElementById('salesOrderId').selectedIndex].text, function (result) {
            $scope.shiftCboList = result;
        });
    };

    $scope.getList = function () {
        if (baseService.isUndefinedOrNull($scope.modelNew.ProductionDate))
            return $scope.productionList = [];
        $http({
            method: "GET"
            , url: $scope.getListUrl
            , params: {
                'date': $filter('dateFiltering')($scope.modelNew.ProductionDate, 'dd-MM-yyyy'),
                'line': document.getElementById("lineId").options[document.getElementById('lineId').selectedIndex].text,
                'salesOrderName': document.getElementById("salesOrderId").options[document.getElementById('salesOrderId').selectedIndex].text,
                'shift': document.getElementById("shiftId").options[document.getElementById('shiftId').selectedIndex].text
            }
            , dataType: "json"
        }).then(function successCallback(response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.modelNew.Id = response.data[0].LineProductionBookingId;
                    $scope.modelNew.ProductionQty = response.data[0].ProductionQty;
                    $scope.productionList = response.data;
                }
                else {
                    $scope.productionList = [];
                    $scope.modelNew.Id = null;
                    $scope.modelNew.ProductionQty = null;
                }
            }
        }), function errorCallBack(response) {
            showResult(response.data.Message, 'failure');
        };
    };


    $scope.Save = function () {
        if (baseService.arrayLength($scope.productionList) === 0)
            return ShowResult('Data not found', 'failure');
        $http({
            method: 'POST'
            , url: $scope.saveUrl
            , data: { 'entities': $scope.productionList }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                ClearFields();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.Clear = function () {
        ClearFields();
        $scope.modelNew = {};
    };
    function ClearFields() {
        $scope.model = {};
        $scope.modelNew = {
            ProductionDate: $scope.modelNew.ProductionDate
            , LineId: $scope.modelNew.LineId
        };
        $scope.productionList = [];
    }
}
