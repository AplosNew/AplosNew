'use strict';
GRNRequisitionSOAllocationController.$inject = ['accountService', 'addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$location'];
function GRNRequisitionSOAllocationController(accountService, addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $location) {

    $rootScope.title = 'GRN SO allocation';

    // #region All Tab Control
    $scope.Action = "Update";
    $scope.GRN = "";
    $scope.tab = 1;
    $scope.setTabpou = function (newTab) {
        $scope.tab = newTab;
        $scope.GetOutSourceGRData();

    };
    $scope.isSetpou = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };


    $scope.setTabpou1 = function (newTab) {
        $scope.tab = newTab;
        $scope.getalldata1();
    };
    $scope.isSetpou1 = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };
    // $scope.tab = 2;

    $scope.setTabpoa = function (newTab) {

        $scope.tab = newTab;
        $scope.GRN = 1;
        $scope.GetOutSourceGRData();
    };
    $scope.isSetpoa = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 1;
    };
    // End PO approve

    $scope.setTabAllocatedList = function (newTab) {

        $scope.tab = newTab;
        $scope.GetOutSourceReceiptAllocatedData();
    };
    $scope.isSetAllocatedList = function (tabNum) {
        return $scope.tab === tabNum;

    };

    $scope.setTabGRNRejectHoldList1 = function (newTab) {

        $scope.tab = newTab;
        //$scope.getListForGRNRejectHoldList();
        //$scope.GRNNGriddataHoldReject();
    };
    $scope.isSetGRNRejectHoldList1 = function (tabNum) {
        return $scope.tab === tabNum;

    };

    $scope.setTabpoApproval = function (newTab) {

        $scope.tab = newTab;
        $scope.GRN = 1;
        $scope.Griddataapprovpo1();

        //alert('Checked Approval');
    };
    $scope.isSetpoApproval = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 1;
    };




    $scope.setTabGRNApproval = function (newTab) {

        $scope.tab = newTab;
        $scope.GRN = 1;
        $scope.Griddataapprovpo1();

        //alert('Checked Approval');
    };
    $scope.isSetGRNApproval = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 1;
    };


    // #endregion

    $scope.OutSourceGRNData = [];
    $scope.GetOutSourceGRData = function () {
        //debugger;
        $http({
            method: 'GET',
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetOutSourceReceiptDataForAllocation',
        }).then(function successCallback(response) {
            $scope.OutSourceGRNData = response.data;
        });
    };
    $scope.GetOutSourceGRData();

    $scope.OutSourceReceiptAllocatedData = [];
    $scope.GetOutSourceReceiptAllocatedData = function () {
        //debugger;
        $http({
            method: 'GET',
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetOutSourceReceiptAllocatedData',
        }).then(function successCallback(response) {
            $scope.OutSourceReceiptAllocatedData = response.data;
        });
    };


    $scope.CalculateBaseQty = function (data) {
        if (data.TransactionQty > data.TransactionQty1) {
            ShowResult('Current Transaction Qty can not grater than Transaction Qty ', 'failure');
            data.TransactionQty = 0;
            data.BaseQty = 0;
            return false;

        }
        if (data.BaseQty > data.BaseQty1) {
            ShowResult('Current Base Qty can not grater than Base Qty ', 'failure');
            data.BaseQty = 0;
            return false;
        }
        else {
            data.BaseQty = (data.TransactionQty * data.BaseUOMFactor);
        }

    }

    $scope.SOAllowcation = function (data) {
        //$scope.Action1 = 'Update'
        $scope.GRNTrnQty = data.TransactionQty
        $scope.GRNUOM = data.TransactionUoM
        $scope.GRNDetailNo = data.InventoryReceiveDetailId
        GRNAllowcationForSOList(data.InventoryReceiveDetailId);
        angular.element(document.querySelector('#ListOfSo')).modal('show');
    };

    $scope.OutSourceReceiptDetailData = [];
    function GRNAllowcationForSOList(inventoryReceiveId) {
        $scope.Action1 = 'Save';
        $http.get('Products/GoodsReceiveNote/GetOutSourceReceiptDetailDataForAllocation?inventoryReceiveDetailId=' + inventoryReceiveId)
            .then(function (response) {
                $scope.OutSourceReceiptDetailData = response.data;
            });
    }
    $scope.GrnRequisitionAllocationSave = function () {

    }
    $scope.ClsoeListOfSo = function () {
        $scope.GRNTrnQty = null;
        $scope.GRNUOM = null;
        $scope.GRNDetailNo = null;
        angular.element(document.querySelector('#ListOfSo')).modal('hide');
    }

    $scope.calculateAllocationQty = function (data) {
        $scope.currentTransactionQty = $filter("sumByKey")($filter("filter")($scope.OutSourceReceiptDetailData), "TransactionQty");
        $scope.currentAllocatedQty = $filter("sumByKey")($filter("filter")($scope.OutSourceReceiptDetailData), "AllocatedQty");
        if ($scope.GRNTrnQty < (parseFloat($scope.currentTransactionQty) + parseFloat($scope.currentAllocatedQty))) {
            data.TransactionQty = 0
            ShowResult('Current  Qty can not grater than GRN Qty ', 'failure', 'ListOfSo');
        }
        data.BaseQty = data.TransactionQty * data.BaseUOMFactor
    }

    $scope.validation = function () {
        if ($scope.detailListNew.length > 0) {
            $scope.currentTransactionQty = $filter("sumByKey")($filter("filter")($scope.detailListNew), "TransactionQty");
            $scope.currentAllocatedQty = $filter("sumByKey")($filter("filter")($scope.detailListNew), "AllocatedQty");
            if ($scope.GRNTrnQty < (parseFloat($scope.currentTransactionQty) + parseFloat($scope.currentAllocatedQty))) {
                ShowResult('Qty can not grater than GRN Qty ', 'failure', 'ListOfSo');
                return true;
            }
            if ($scope.GRNTrnQty > (parseFloat($scope.currentTransactionQty) + parseFloat($scope.currentAllocatedQty))) {
                ShowResult('Qty can not less than GRN Qty ', 'failure', 'ListOfSo');
                return true;
            }
        }
        else {
            ShowResult('Please select  ', 'failure', 'ListOfSo');
            return true;
        }

        return false;
    };

    $scope.UpdateJWSOAllocation = function () {
        $scope.detailListNew = [];
        for (var i = 0; i < $scope.OutSourceReceiptDetailData.length; i++) {
            if ($scope.OutSourceReceiptDetailData[i].Active === true) {
                //var OutSourceGRNData = $filter("filter")($scope.OutSourceGRNData, { "MaterialMasterId": $scope.OutSourceGRNData[i].MaterialMasterId, "ArticleId": $scope.OutSourceGRNData[i].ArticleId, "FirstCharacteristicsValueId": $scope.OutSourceGRNData[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.OutSourceGRNData[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.OutSourceGRNData[i].ThirdCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.OutSourceGRNData[i].InventoryReceiveDetailId, "check": true }).TransactionQty;
                //$scope.OutSourceGRNData[i].TransactionQty1 = $filter('sumByKey')($filter('filter')($scope.OutSourceGRNData, {	MaterialMasterId: $scope.OutSourceGRNData[i].MaterialMasterId, ArticleId: $scope.OutSourceGRNData[i].ArticleId, FirstCharacteristicsValueId: $scope.OutSourceGRNData[i].FirstCharacteristicsValueId, SecondCharacteristicsValueId: $scope.OutSourceGRNData[i].SecondCharacteristicsValueId, ThirdCharacteristicsValueId: $scope.OutSourceGRNData[i].ThirdCharacteristicsValueId, InventoryReceiveDetailId: $scope.OutSourceGRNData[i].InventoryReceiveDetailId, "check": true }), 'TransactionQty1');
                $scope.detailListNew.push($scope.OutSourceReceiptDetailData[i]);
            }
        }
        if (!$scope.validation()) {
            if ($scope.Action === "Update") {
                $http({
                    method: 'POST'
                    , url: 'Products/GoodsReceiveNote/CreateJWSOAllocation'
                    , data: { 'Data': $scope.detailListNew }
                    , dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ClsoeListOfSo();
                        $scope.GetOutSourceGRData();
                        //$scope.Clear();
                        //$scope.getdataInventoryIssue();
                        //$scope.productNew.Id = response.data.inventoryIssue.Id;
                        //$scope.getData();
                        //$scope.GetDataList();
                    }
                }), function (response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else ShowResult('Please issue material', 'failure');
        }
    };

    $scope.UpdateJWSOAllocationFromUI = function () {
        $scope.detailListNew = [];
        for (var i = 0; i < $scope.OutSourceGRNData.length; i++) {
            if ($scope.OutSourceGRNData[i].SOIdCount == 1) {
                $scope.detailListNew.push($scope.OutSourceGRNData[i]);
            }
        }
        $http({
            method: 'POST'
            , url: 'Products/GoodsReceiveNote/CreateJWSOAllocation'
            , data: { 'Data': $scope.detailListNew }
            , dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetOutSourceGRData();
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
}