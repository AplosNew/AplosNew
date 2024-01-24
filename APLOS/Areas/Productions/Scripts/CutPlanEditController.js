'use strict';
CutPlanEditController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function CutPlanEditController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Cut Plan Update';
    $scope.Action = 'Save';
    $scope.path = 'Productions/CutPlanEdit/';

    $scope.CutPlanEditHeader = {
        Id: null
        , MasterPlanId: null
        , ColorId: null
        , HeaderCount: null
        , NoOfPlyR1: null
        , TotalFinalQty: null
        , TotalRatio: null
        , TotalCAQty: null
        , TotalRatio2: null
        , TotalCAQty2: null
        , TotalRatio3: null
        , TotalCAQty3: null
        , TotalRatio4: null
        , TotalCAQty4: null
    };
    $scope.CutPlanEditHeaderNew = Object.assign({}, $scope.CutPlanEditHeader);

    $scope.MasterPlanList = [];
    $scope.GetMasterPlanList = function () {
        $http({
            method: 'GET',
            url: 'Productions/CutPlanEdit/GetMasterPlanList'
        }).then(function successCallback(response) {
            $scope.MasterPlanList = response.data;
        });
    }
    $scope.GetMasterPlanList();

    $scope.ColorLists = [];
    $scope.GetColorLists = function (MPId) {
        $http({
            method: 'GET',
            url: 'Productions/CutPlanEdit/GetColorLists?MasterPlanId=' + MPId
        }).then(function successCallback(response) {
            $scope.ColorLists = response.data;
        });
    }

    $scope.CutPlanSummaryList = [];
    $scope.LoadCutPlanSummaryList = function () {
        $http({
            method: 'Get',
            url: 'Productions/CutPlanEdit/GetCutPlanSummary?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanSummaryList = response.data;
            var gridObj = $("#GridCutPlanSummary").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.AllotedHeaderCountList = [];
    $scope.GetAllotedHeaderCountList = function () {
        $http({
            method: 'GET',
            url: 'Productions/CutPlanEdit/GetAllotedHeaderCountList?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.AllotedHeaderCountList = response.data;
            $scope.CutPlanEditHeaderNew.HeaderCount = $scope.AllotedHeaderCountList[0].HeaderCount;
        });
    }

    $scope.View = function () {
        $scope.LoadCutPlanSummaryList();
        $scope.GetAllotedHeaderCountList();
        $scope.LoadCutPlanDetailsR1List();
        $scope.LoadCutPlanDetailsR2List();
        $scope.LoadCutPlanDetailsR3List();
        $scope.LoadCutPlanDetailsR4List();
    }

    $scope.CutPlanDetailsR1List = [];
    $scope.MinimumQty = 0;
    $scope.LoadCutPlanDetailsR1List = function () {
        $scope.CutPlanEditHeaderNew.TotalFinalQty = 0;
        $scope.CutPlanEditHeaderNew.TotalRatio = 0;
        $scope.CutPlanEditHeaderNew.TotalCAQty = 0;
        $scope.CutPlanDetailsR1List = [];
        $http({
            method: 'Get',
            url: 'Productions/CutPlanEdit/GetCutPlanDetailsR1List?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanDetailsR1List = response.data;
            for (var i = 0; i < $scope.CutPlanDetailsR1List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalFinalQty = $scope.CutPlanEditHeaderNew.TotalFinalQty + $scope.CutPlanDetailsR1List[i].FinalQty;
                $scope.CutPlanEditHeaderNew.TotalRatio = $scope.CutPlanEditHeaderNew.TotalRatio + $scope.CutPlanDetailsR1List[i].Ratio1;
                $scope.CutPlanEditHeaderNew.TotalCAQty = $scope.CutPlanEditHeaderNew.TotalCAQty + $scope.CutPlanDetailsR1List[i].AllotedQtyR1;
            }
            $scope.CutPlanEditHeaderNew.NoOfPlyR1 = $scope.CutPlanDetailsR1List[0].NoOfPlyR1;
            $scope.MinimumQty = $scope.CutPlanEditHeaderNew.NoOfPlyR1;
            var gridObj = $("#GridCutPlanDetailsR1").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.NoOfPlyR1ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty = 0;
            if ($scope.CutPlanEditHeaderNew.NoOfPlyR1 > $scope.MinimumQty) {
                throw "MinimumQty should not be greater than the minimumqty of Balance to be closed";
            }
            for (var i = 0; i < $scope.CutPlanDetailsR1List.length; i++) {
                $scope.CutPlanDetailsR1List[i].Ratio1 = Math.floor($scope.CutPlanDetailsR1List[i].FinalQty / $scope.CutPlanEditHeaderNew.NoOfPlyR1);
                $scope.CutPlanDetailsR1List[i].AllotedQtyR1 = $scope.CutPlanDetailsR1List[i].Ratio1 * $scope.CutPlanEditHeaderNew.NoOfPlyR1;
                $scope.CutPlanEditHeaderNew.TotalRatio = $scope.CutPlanEditHeaderNew.TotalRatio + $scope.CutPlanDetailsR1List[i].Ratio1;
                $scope.CutPlanEditHeaderNew.TotalCAQty = $scope.CutPlanEditHeaderNew.TotalCAQty + $scope.CutPlanDetailsR1List[i].AllotedQtyR1;
            }
            var gridObj = $("#GridCutPlanDetailsR1").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Ratio1Value = 0;
    $scope.Ratio1Change = function (data) {
        try {
            $scope.CutPlanEditHeaderNew.TotalFinalQty = 0;
            $scope.CutPlanEditHeaderNew.TotalRatio = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty = 0;
            $scope.Ratio1Value = 0;
            $scope.Ratio1Value = data.data.Ratio1;
            if ($scope.Ratio1Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.data.AllotedQtyR1 = $scope.Ratio1Value * $scope.CutPlanEditHeaderNew.NoOfPlyR1;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR1List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalFinalQty = $scope.CutPlanEditHeaderNew.TotalFinalQty + $scope.CutPlanDetailsR1List[i].FinalQty;
                $scope.CutPlanEditHeaderNew.TotalRatio = $scope.CutPlanEditHeaderNew.TotalRatio + $scope.CutPlanDetailsR1List[i].Ratio1;
                $scope.CutPlanEditHeaderNew.TotalCAQty = $scope.CutPlanEditHeaderNew.TotalCAQty + $scope.CutPlanDetailsR1List[i].AllotedQtyR1;
            }
            var gridObj = $("#GridCutPlanDetailsR1").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.CutPlanDetailsR2List = [];
    $scope.MinimumQtyR2 = 0;
    $scope.LoadCutPlanDetailsR2List = function () {
        $scope.CutPlanEditHeaderNew.TotalRatio2 = 0;
        $scope.CutPlanEditHeaderNew.TotalCAQty2 = 0;
        $scope.CutPlanDetailsR2List = [];
        $http({
            method: 'Get',
            url: 'Productions/CutPlanEdit/GetCutPlanDetailsR2List?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanDetailsR2List = response.data;
            for (var i = 0; i < $scope.CutPlanDetailsR2List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio2 = $scope.CutPlanEditHeaderNew.TotalRatio2 + $scope.CutPlanDetailsR2List[i].Ratio2;
                $scope.CutPlanEditHeaderNew.TotalCAQty2 = $scope.CutPlanEditHeaderNew.TotalCAQty2 + $scope.CutPlanDetailsR2List[i].AllotedQtyR2;
            }
            $scope.CutPlanEditHeaderNew.NoOfPlyR2 = $scope.CutPlanDetailsR2List[0].NoOfPlyR2;
            $scope.MinimumQtyR2 = $scope.CutPlanEditHeaderNew.NoOfPlyR2;
            var gridObj = $("#GridCutPlanDetailsR2").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.NoOfPlyR2ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio2 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty2 = 0;
            if ($scope.CutPlanEditHeaderNew.NoOfPlyR2 > $scope.MinimumQtyR2) {
                throw "MinimumQty should not be greater than the minimumqty of Balance to be closed";
            }
            for (var i = 0; i < $scope.CutPlanDetailsR2List.length; i++) {
                $scope.CutPlanDetailsR2List[i].Ratio2 = Math.floor($scope.CutPlanDetailsR2List[i].BalanceToAllotedR1 / $scope.CutPlanEditHeaderNew.NoOfPlyR2);
                $scope.CutPlanDetailsR2List[i].AllotedQtyR2 = $scope.CutPlanDetailsR2List[i].Ratio2 * $scope.CutPlanEditHeaderNew.NoOfPlyR2;
                $scope.CutPlanEditHeaderNew.TotalRatio2 = $scope.CutPlanEditHeaderNew.TotalRatio2 + $scope.CutPlanDetailsR2List[i].Ratio2;
                $scope.CutPlanEditHeaderNew.TotalCAQty2 = $scope.CutPlanEditHeaderNew.TotalCAQty2 + $scope.CutPlanDetailsR2List[i].AllotedQtyR2;
            }
            var gridObj = $("#GridCutPlanDetailsR2").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Ratio2Value = 0;
    $scope.Ratio2Change = function (data) {
        try {
            $scope.CutPlanEditHeaderNew.TotalRatio2 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty2 = 0;
            $scope.Ratio2Value = 0;
            $scope.Ratio2Value = data.data.Ratio2;
            if ($scope.Ratio2Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.data.AllotedQtyR2 = $scope.Ratio2Value * $scope.CutPlanEditHeaderNew.NoOfPlyR2;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR2List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio2 = $scope.CutPlanEditHeaderNew.TotalRatio2 + $scope.CutPlanDetailsR2List[i].Ratio2;
                $scope.CutPlanEditHeaderNew.TotalCAQty2 = $scope.CutPlanEditHeaderNew.TotalCAQty2 + $scope.CutPlanDetailsR2List[i].AllotedQtyR2;
            }
            var gridObj = $("#GridCutPlanDetailsR2").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.CutPlanDetailsR3List = [];
    $scope.MinimumQtyR3 = 0;
    $scope.LoadCutPlanDetailsR3List = function () {
        $scope.CutPlanEditHeaderNew.TotalRatio3 = 0;
        $scope.CutPlanEditHeaderNew.TotalCAQty3 = 0;
        $scope.CutPlanDetailsR3List = [];
        $http({
            method: 'Get',
            url: 'Productions/CutPlanEdit/GetCutPlanDetailsR3List?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanDetailsR3List = response.data;
            for (var i = 0; i < $scope.CutPlanDetailsR3List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio3 = $scope.CutPlanEditHeaderNew.TotalRatio3 + $scope.CutPlanDetailsR3List[i].Ratio3;
                $scope.CutPlanEditHeaderNew.TotalCAQty3 = $scope.CutPlanEditHeaderNew.TotalCAQty3 + $scope.CutPlanDetailsR3List[i].AllotedQtyR3;
            }
            $scope.CutPlanEditHeaderNew.NoOfPlyR3 = $scope.CutPlanDetailsR3List[0].NoOfPlyR3;
            $scope.MinimumQtyR3 = $scope.CutPlanEditHeaderNew.NoOfPlyR3;
            var gridObj = $("#GridCutPlanDetailsR3").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.NoOfPlyR3ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio3 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty3 = 0;
            if ($scope.CutPlanEditHeaderNew.NoOfPlyR3 > $scope.MinimumQtyR3) {
                throw "MinimumQty should not be greater than the minimumqty of Balance to be closed";
            }
            for (var i = 0; i < $scope.CutPlanDetailsR3List.length; i++) {
                $scope.CutPlanDetailsR3List[i].Ratio3 = Math.floor($scope.CutPlanDetailsR3List[i].BalanceToAllotedR2 / $scope.CutPlanEditHeaderNew.NoOfPlyR3);
                $scope.CutPlanDetailsR3List[i].AllotedQtyR3 = $scope.CutPlanDetailsR3List[i].Ratio3 * $scope.CutPlanEditHeaderNew.NoOfPlyR3;
                $scope.CutPlanEditHeaderNew.TotalRatio3 = $scope.CutPlanEditHeaderNew.TotalRatio3 + $scope.CutPlanDetailsR3List[i].Ratio3;
                $scope.CutPlanEditHeaderNew.TotalCAQty3 = $scope.CutPlanEditHeaderNew.TotalCAQty3 + $scope.CutPlanDetailsR3List[i].AllotedQtyR3;
            }
            var gridObj = $("#GridCutPlanDetailsR3").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Ratio3Value = 0;
    $scope.Ratio3Change = function (data) {
        try {
            $scope.CutPlanEditHeaderNew.TotalRatio3 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty3 = 0;
            $scope.Ratio3Value = 0;
            $scope.Ratio3Value = data.data.Ratio3;
            if ($scope.Ratio3Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.data.AllotedQtyR3 = $scope.Ratio3Value * $scope.CutPlanEditHeaderNew.NoOfPlyR3;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR3List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio3 = $scope.CutPlanEditHeaderNew.TotalRatio3 + $scope.CutPlanDetailsR3List[i].Ratio3;
                $scope.CutPlanEditHeaderNew.TotalCAQty3 = $scope.CutPlanEditHeaderNew.TotalCAQty3 + $scope.CutPlanDetailsR3List[i].AllotedQtyR3;
            }
            var gridObj = $("#GridCutPlanDetailsR3").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    $scope.CutPlanDetailsR4List = [];
    $scope.MinimumQtyR4 = 0;
    $scope.LoadCutPlanDetailsR4List = function () {
        $scope.CutPlanEditHeaderNew.TotalRatio4 = 0;
        $scope.CutPlanEditHeaderNew.TotalCAQty4 = 0;
        $scope.CutPlanDetailsR4List = [];
        $http({
            method: 'Get',
            url: 'Productions/CutPlanEdit/GetCutPlanDetailsR4List?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanDetailsR4List = response.data;
            for (var i = 0; i < $scope.CutPlanDetailsR4List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio4 = $scope.CutPlanEditHeaderNew.TotalRatio4 + $scope.CutPlanDetailsR4List[i].Ratio4;
                $scope.CutPlanEditHeaderNew.TotalCAQty4 = $scope.CutPlanEditHeaderNew.TotalCAQty4 + $scope.CutPlanDetailsR4List[i].AllotedQtyR4;
            }
            $scope.CutPlanEditHeaderNew.NoOfPlyR4 = $scope.CutPlanDetailsR4List[0].NoOfPlyR4;
            $scope.MinimumQtyR4 = $scope.CutPlanEditHeaderNew.NoOfPlyR4;
            var gridObj = $("#GridCutPlanDetailsR4").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.NoOfPlyR4ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio4 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty4 = 0;
            if ($scope.CutPlanEditHeaderNew.NoOfPlyR4 > $scope.MinimumQtyR4) {
                throw "MinimumQty should not be greater than the minimumqty of Balance to be closed";
            }
            for (var i = 0; i < $scope.CutPlanDetailsR4List.length; i++) {
                $scope.CutPlanDetailsR4List[i].Ratio4 = Math.floor($scope.CutPlanDetailsR4List[i].BalanceToAllotedR3 / $scope.CutPlanEditHeaderNew.NoOfPlyR4);
                $scope.CutPlanDetailsR4List[i].AllotedQtyR4 = $scope.CutPlanDetailsR4List[i].Ratio4 * $scope.CutPlanEditHeaderNew.NoOfPlyR4;
                $scope.CutPlanEditHeaderNew.TotalRatio4 = $scope.CutPlanEditHeaderNew.TotalRatio4 + $scope.CutPlanDetailsR4List[i].Ratio4;
                $scope.CutPlanEditHeaderNew.TotalCAQty4 = $scope.CutPlanEditHeaderNew.TotalCAQty4 + $scope.CutPlanDetailsR4List[i].AllotedQtyR4;
            }
            var gridObj = $("#GridCutPlanDetailsR4").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Ratio4Value = 0;
    $scope.Ratio4Change = function (data) {
        try {
            $scope.CutPlanEditHeaderNew.TotalRatio4 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty4 = 0;
            $scope.Ratio4Value = 0;
            $scope.Ratio4Value = data.data.Ratio4;
            if ($scope.Ratio4Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.data.AllotedQtyR4 = $scope.Ratio4Value * $scope.CutPlanEditHeaderNew.NoOfPlyR4;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR4List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio4 = $scope.CutPlanEditHeaderNew.TotalRatio4 + $scope.CutPlanDetailsR4List[i].Ratio4;
                $scope.CutPlanEditHeaderNew.TotalCAQty4 = $scope.CutPlanEditHeaderNew.TotalCAQty4 + $scope.CutPlanDetailsR4List[i].AllotedQtyR4;
            }
            var gridObj = $("#GridCutPlanDetailsR4").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
}