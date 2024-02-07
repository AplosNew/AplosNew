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
        , R1Id: null
        , NoOfPlyR1: null
        , UserNameR1: null
        , MarkerIdR1: null
        , PackingTypeIdR1: null
        , R2Id: null
        , NoOfPlyR2: null
        , UserNameR2: null
        , MarkerIdR2: null
        , PackingTypeIdR2: null
        , R3Id: null
        , NoOfPlyR3: null
        , UserNameR3: null
        , MarkerIdR3: null
        , PackingTypeIdR3: null
        , R4Id: null
        , NoOfPlyR4: null
        , UserNameR4: null
        , MarkerIdR4: null
        , PackingTypeIdR4: null
        , R5Id: null
        , NoOfPlyR5: null
        , UserNameR5: null
        , MarkerIdR5: null
        , PackingTypeIdR5: null
        , R6Id: null
        , NoOfPlyR6: null
        , UserNameR6: null
        , MarkerIdR6: null
        , PackingTypeIdR6: null
        , R7Id: null
        , NoOfPlyR7: null
        , UserNameR7: null
        , MarkerIdR7: null
        , PackingTypeIdR7: null
        , R8Id: null
        , NoOfPlyR8: null
        , UserNameR8: null
        , MarkerIdR8: null
        , PackingTypeIdR8: null
        , R9Id: null
        , NoOfPlyR9: null
        , UserNameR9: null
        , MarkerIdR9: null
        , PackingTypeIdR9: null
        , R10Id: null
        , NoOfPlyR10: null
        , UserNameR10: null
        , MarkerIdR10: null
        , PackingTypeIdR10: null
        , TotalRatio: null
        , TotalCAQty: null
        , TotalRatio2: null
        , TotalCAQty2: null
        , TotalRatio3: null
        , TotalCAQty3: null
        , TotalRatio4: null
        , TotalCAQty4: null
        , TotalRatio5: null
        , TotalCAQty5: null
        , TotalRatio6: null
        , TotalCAQty6: null
        , TotalRatio7: null
        , TotalCAQty7: null
        , TotalRatio8: null
        , TotalCAQty8: null
        , TotalRatio9: null
        , TotalCAQty9: null
        , TotalRatio10: null
        , TotalCAQty10: null
        , TotalFinalQty: null
        , TotalCPQ: null
        , TotalEPQ: null
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
        $scope.GetAllotedHeaderCountList();
        $scope.LoadCutPlanDetailsR1List();
        $scope.LoadCutPlanDetailsR2List();
        $scope.LoadCutPlanDetailsR3List();
        $scope.LoadCutPlanDetailsR4List();
        $scope.LoadCutPlanDetailsR5List();
        $scope.LoadCutPlanDetailsR6List();
        $scope.LoadCutPlanDetailsR7List();
        $scope.LoadCutPlanDetailsR8List();
        $scope.LoadCutPlanDetailsR9List();
        $scope.LoadCutPlanDetailsR10List();
        $scope.LoadCutPlanDetailsBalanceList();
    }

    $scope.CutPlanDetailsR1List = [];
    $scope.LoadCutPlanDetailsR1List = function () {
        $scope.CutPlanEditHeaderNew.TotalRatio = 0;
        $scope.CutPlanEditHeaderNew.TotalCAQty = 0;
        $scope.CutPlanDetailsR1List = [];
        $http({
            method: 'Get',
            url: 'Productions/CutPlanEdit/GetCutPlanDetailsR1List?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanDetailsR1List = response.data;
            for (var i = 0; i < $scope.CutPlanDetailsR1List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio = $scope.CutPlanEditHeaderNew.TotalRatio + $scope.CutPlanDetailsR1List[i].Ratio1;
                $scope.CutPlanEditHeaderNew.TotalCAQty = $scope.CutPlanEditHeaderNew.TotalCAQty + $scope.CutPlanDetailsR1List[i].AllotedQtyR1;
            }
            $scope.CutPlanEditHeaderNew.NoOfPlyR1 = $scope.CutPlanDetailsR1List[0].NoOfPlyR1;
        }
        )
    }

    $scope.NoOfPlyR1ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty = 0;
            for (var i = 0; i < $scope.CutPlanDetailsR1List.length; i++) {
                $scope.CutPlanDetailsR1List[i].Ratio1 = Math.floor($scope.CutPlanDetailsR1List[i].FinalQty / $scope.CutPlanEditHeaderNew.NoOfPlyR1);
                $scope.CutPlanDetailsR1List[i].AllotedQtyR1 = $scope.CutPlanDetailsR1List[i].Ratio1 * $scope.CutPlanEditHeaderNew.NoOfPlyR1;
                $scope.CutPlanEditHeaderNew.TotalRatio = $scope.CutPlanEditHeaderNew.TotalRatio + $scope.CutPlanDetailsR1List[i].Ratio1;
                $scope.CutPlanEditHeaderNew.TotalCAQty = $scope.CutPlanEditHeaderNew.TotalCAQty + $scope.CutPlanDetailsR1List[i].AllotedQtyR1;
            }
            $scope.SaveDetailsR1();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Ratio1Value = 0;
    $scope.Ratio1Change = function (data) {
        try {
            $scope.CutPlanEditHeaderNew.TotalRatio = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty = 0;
            $scope.Ratio1Value = 0;
            $scope.Ratio1Value = data.Ratio1;
            if ($scope.Ratio1Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.AllotedQtyR1 = $scope.Ratio1Value * $scope.CutPlanEditHeaderNew.NoOfPlyR1;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR1List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio = $scope.CutPlanEditHeaderNew.TotalRatio + $scope.CutPlanDetailsR1List[i].Ratio1;
                $scope.CutPlanEditHeaderNew.TotalCAQty = $scope.CutPlanEditHeaderNew.TotalCAQty + $scope.CutPlanDetailsR1List[i].AllotedQtyR1;
            }
            $scope.SaveDetailsR1();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveDetailsR1 = function () {
            try {
                $scope.SaveList = [];
                for (var i = 0; i < $scope.CutPlanDetailsR1List.length; i++) {
                    if ($scope.CutPlanDetailsR1List[i].Ratio1 > 0) {
                        $scope.SaveList.push($scope.CutPlanDetailsR1List[i]);
                    }
                    $scope.CutPlanEditHeaderNew.R1Id = $scope.CutPlanDetailsR1List[i].R1Id;
                    $scope.CutPlanEditHeaderNew.UserNameR1 = $scope.CutPlanDetailsR1List[i].UserNameR1;
                    $scope.CutPlanEditHeaderNew.MarkerIdR1 = $scope.CutPlanDetailsR1List[i].MarkerIdR1;
                    $scope.CutPlanEditHeaderNew.PackingTypeIdR1 = $scope.CutPlanDetailsR1List[i].PackingTypeIdR1;
                    $scope.CutPlanDetailsR1List[i].NoOfPlyR1 = $scope.CutPlanEditHeaderNew.NoOfPlyR1;
                }
                $http({
                    method: "POST",
                    url: 'Productions/CutPlanEdit/CreateCutPlanEditR1Data?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId,
                    data: {
                        'data': $scope.CutPlanEditHeaderNew,
                        'DataList': $scope.SaveList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.LoadCutPlanDetailsR1List();
                        $scope.LoadCutPlanDetailsBalanceList();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;

            } catch (e) {
                ShowResult(e, "failure");
            }
    };

    $scope.CutPlanDetailsR2List = [];
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
        }
        )
    }

    $scope.NoOfPlyR2ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio2 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty2 = 0;
            for (var i = 0; i < $scope.CutPlanDetailsR2List.length; i++) {
                $scope.CutPlanDetailsR2List[i].Ratio2 = Math.floor($scope.CutPlanDetailsR2List[i].BalanceToAllotedR1 / $scope.CutPlanEditHeaderNew.NoOfPlyR2);
                $scope.CutPlanDetailsR2List[i].AllotedQtyR2 = $scope.CutPlanDetailsR2List[i].Ratio2 * $scope.CutPlanEditHeaderNew.NoOfPlyR2;
                $scope.CutPlanEditHeaderNew.TotalRatio2 = $scope.CutPlanEditHeaderNew.TotalRatio2 + $scope.CutPlanDetailsR2List[i].Ratio2;
                $scope.CutPlanEditHeaderNew.TotalCAQty2 = $scope.CutPlanEditHeaderNew.TotalCAQty2 + $scope.CutPlanDetailsR2List[i].AllotedQtyR2;
            }
            $scope.SaveDetailsR2(); 
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
            $scope.Ratio2Value = data.Ratio2;
            if ($scope.Ratio2Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.AllotedQtyR2 = $scope.Ratio2Value * $scope.CutPlanEditHeaderNew.NoOfPlyR2;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR2List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio2 = $scope.CutPlanEditHeaderNew.TotalRatio2 + $scope.CutPlanDetailsR2List[i].Ratio2;
                $scope.CutPlanEditHeaderNew.TotalCAQty2 = $scope.CutPlanEditHeaderNew.TotalCAQty2 + $scope.CutPlanDetailsR2List[i].AllotedQtyR2;
            }
            $scope.SaveDetailsR2();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveDetailsR2 = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.CutPlanDetailsR2List.length; i++) {
                if ($scope.CutPlanDetailsR2List[i].Ratio2 > 0) {
                    $scope.SaveList.push($scope.CutPlanDetailsR2List[i]);
                }
                $scope.CutPlanEditHeaderNew.R2Id = $scope.CutPlanDetailsR2List[i].R2Id;
                $scope.CutPlanEditHeaderNew.UserNameR2 = $scope.CutPlanDetailsR2List[i].UserNameR2;
                $scope.CutPlanEditHeaderNew.MarkerIdR2 = $scope.CutPlanDetailsR2List[i].MarkerIdR2;
                $scope.CutPlanEditHeaderNew.PackingTypeIdR2 = $scope.CutPlanDetailsR2List[i].PackingTypeIdR2;
                $scope.CutPlanDetailsR2List[i].NoOfPlyR2 = $scope.CutPlanEditHeaderNew.NoOfPlyR2;
            }
            $http({
                method: "POST",
                url: 'Productions/CutPlanEdit/CreateCutPlanEditR2Data?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId,
                data: {
                    'data': $scope.CutPlanEditHeaderNew,
                    'DataList': $scope.SaveList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadCutPlanDetailsR2List();
                    $scope.LoadCutPlanDetailsBalanceList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.CutPlanDetailsR3List = [];
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
        }
        )
    }

    $scope.NoOfPlyR3ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio3 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty3 = 0;
            for (var i = 0; i < $scope.CutPlanDetailsR3List.length; i++) {
                $scope.CutPlanDetailsR3List[i].Ratio3 = Math.floor($scope.CutPlanDetailsR3List[i].BalanceToAllotedR2 / $scope.CutPlanEditHeaderNew.NoOfPlyR3);
                $scope.CutPlanDetailsR3List[i].AllotedQtyR3 = $scope.CutPlanDetailsR3List[i].Ratio3 * $scope.CutPlanEditHeaderNew.NoOfPlyR3;
                $scope.CutPlanEditHeaderNew.TotalRatio3 = $scope.CutPlanEditHeaderNew.TotalRatio3 + $scope.CutPlanDetailsR3List[i].Ratio3;
                $scope.CutPlanEditHeaderNew.TotalCAQty3 = $scope.CutPlanEditHeaderNew.TotalCAQty3 + $scope.CutPlanDetailsR3List[i].AllotedQtyR3;
            }
            $scope.SaveDetailsR3();
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
            $scope.Ratio3Value = data.Ratio3;
            if ($scope.Ratio3Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.AllotedQtyR3 = $scope.Ratio3Value * $scope.CutPlanEditHeaderNew.NoOfPlyR3;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR3List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio3 = $scope.CutPlanEditHeaderNew.TotalRatio3 + $scope.CutPlanDetailsR3List[i].Ratio3;
                $scope.CutPlanEditHeaderNew.TotalCAQty3 = $scope.CutPlanEditHeaderNew.TotalCAQty3 + $scope.CutPlanDetailsR3List[i].AllotedQtyR3;
            }
            $scope.SaveDetailsR3();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveDetailsR3 = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.CutPlanDetailsR3List.length; i++) {
                if ($scope.CutPlanDetailsR3List[i].Ratio3 > 0) {
                    $scope.SaveList.push($scope.CutPlanDetailsR3List[i]);
                }
                $scope.CutPlanEditHeaderNew.R3Id = $scope.CutPlanDetailsR3List[i].R3Id;
                $scope.CutPlanEditHeaderNew.UserNameR3 = $scope.CutPlanDetailsR3List[i].UserNameR3;
                $scope.CutPlanEditHeaderNew.MarkerIdR3 = $scope.CutPlanDetailsR3List[i].MarkerIdR3;
                $scope.CutPlanEditHeaderNew.PackingTypeIdR3 = $scope.CutPlanDetailsR3List[i].PackingTypeIdR3;
                $scope.CutPlanDetailsR3List[i].NoOfPlyR3 = $scope.CutPlanEditHeaderNew.NoOfPlyR3;
            }
            $http({
                method: "POST",
                url: 'Productions/CutPlanEdit/CreateCutPlanEditR3Data?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId,
                data: {
                    'data': $scope.CutPlanEditHeaderNew,
                    'DataList': $scope.SaveList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadCutPlanDetailsR3List();
                    $scope.LoadCutPlanDetailsBalanceList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.CutPlanDetailsR4List = [];
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
        }
        )
    }

    $scope.NoOfPlyR4ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio4 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty4 = 0;
            for (var i = 0; i < $scope.CutPlanDetailsR4List.length; i++) {
                $scope.CutPlanDetailsR4List[i].Ratio4 = Math.floor($scope.CutPlanDetailsR4List[i].BalanceToAllotedR3 / $scope.CutPlanEditHeaderNew.NoOfPlyR4);
                $scope.CutPlanDetailsR4List[i].AllotedQtyR4 = $scope.CutPlanDetailsR4List[i].Ratio4 * $scope.CutPlanEditHeaderNew.NoOfPlyR4;
                $scope.CutPlanEditHeaderNew.TotalRatio4 = $scope.CutPlanEditHeaderNew.TotalRatio4 + $scope.CutPlanDetailsR4List[i].Ratio4;
                $scope.CutPlanEditHeaderNew.TotalCAQty4 = $scope.CutPlanEditHeaderNew.TotalCAQty4 + $scope.CutPlanDetailsR4List[i].AllotedQtyR4;
            }
            $scope.SaveDetailsR4();
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
            $scope.Ratio4Value = data.Ratio4;
            if ($scope.Ratio4Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.AllotedQtyR4 = $scope.Ratio4Value * $scope.CutPlanEditHeaderNew.NoOfPlyR4;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR4List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio4 = $scope.CutPlanEditHeaderNew.TotalRatio4 + $scope.CutPlanDetailsR4List[i].Ratio4;
                $scope.CutPlanEditHeaderNew.TotalCAQty4 = $scope.CutPlanEditHeaderNew.TotalCAQty4 + $scope.CutPlanDetailsR4List[i].AllotedQtyR4;
            }
            $scope.SaveDetailsR4();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveDetailsR4 = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.CutPlanDetailsR4List.length; i++) {
                if ($scope.CutPlanDetailsR4List[i].Ratio4 > 0) {
                    $scope.SaveList.push($scope.CutPlanDetailsR4List[i]);
                }
                $scope.CutPlanEditHeaderNew.R4Id = $scope.CutPlanDetailsR4List[i].R4Id;
                $scope.CutPlanEditHeaderNew.UserNameR4 = $scope.CutPlanDetailsR4List[i].UserNameR4;
                $scope.CutPlanEditHeaderNew.MarkerIdR4 = $scope.CutPlanDetailsR4List[i].MarkerIdR4;
                $scope.CutPlanEditHeaderNew.PackingTypeIdR4 = $scope.CutPlanDetailsR4List[i].PackingTypeIdR4;
                $scope.CutPlanDetailsR4List[i].NoOfPlyR4 = $scope.CutPlanEditHeaderNew.NoOfPlyR4;
            }
            $http({
                method: "POST",
                url: 'Productions/CutPlanEdit/CreateCutPlanEditR4Data?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId,
                data: {
                    'data': $scope.CutPlanEditHeaderNew,
                    'DataList': $scope.SaveList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadCutPlanDetailsR4List();
                    $scope.LoadCutPlanDetailsBalanceList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.CutPlanDetailsR5List = [];
    $scope.LoadCutPlanDetailsR5List = function () {
        $scope.CutPlanEditHeaderNew.TotalRatio5 = 0;
        $scope.CutPlanEditHeaderNew.TotalCAQty5 = 0;
        $scope.CutPlanDetailsR5List = [];
        $http({
            method: 'Get',
            url: 'Productions/CutPlanEdit/GetCutPlanDetailsR5List?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanDetailsR5List = response.data;
            for (var i = 0; i < $scope.CutPlanDetailsR5List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio5 = $scope.CutPlanEditHeaderNew.TotalRatio5 + $scope.CutPlanDetailsR5List[i].Ratio5;
                $scope.CutPlanEditHeaderNew.TotalCAQty5 = $scope.CutPlanEditHeaderNew.TotalCAQty5 + $scope.CutPlanDetailsR5List[i].AllotedQtyR5;
            }
            $scope.CutPlanEditHeaderNew.NoOfPlyR5 = $scope.CutPlanDetailsR5List[0].NoOfPlyR5;
        }
        )
    }

    $scope.NoOfPlyR5ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio5 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty5 = 0;
            for (var i = 0; i < $scope.CutPlanDetailsR5List.length; i++) {
                $scope.CutPlanDetailsR5List[i].Ratio5 = Math.floor($scope.CutPlanDetailsR5List[i].BalanceToAllotedR4 / $scope.CutPlanEditHeaderNew.NoOfPlyR5);
                $scope.CutPlanDetailsR5List[i].AllotedQtyR5 = $scope.CutPlanDetailsR5List[i].Ratio5 * $scope.CutPlanEditHeaderNew.NoOfPlyR5;
                $scope.CutPlanEditHeaderNew.TotalRatio5 = $scope.CutPlanEditHeaderNew.TotalRatio5 + $scope.CutPlanDetailsR5List[i].Ratio5;
                $scope.CutPlanEditHeaderNew.TotalCAQty5 = $scope.CutPlanEditHeaderNew.TotalCAQty5 + $scope.CutPlanDetailsR5List[i].AllotedQtyR5;
            }
            $scope.SaveDetailsR5();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Ratio5Value = 0;
    $scope.Ratio5Change = function (data) {
        try {
            $scope.CutPlanEditHeaderNew.TotalRatio5 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty5 = 0;
            $scope.Ratio5Value = 0;
            $scope.Ratio5Value = data.Ratio5;
            if ($scope.Ratio5Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.AllotedQtyR5 = $scope.Ratio5Value * $scope.CutPlanEditHeaderNew.NoOfPlyR5;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR5List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio5 = $scope.CutPlanEditHeaderNew.TotalRatio5 + $scope.CutPlanDetailsR5List[i].Ratio5;
                $scope.CutPlanEditHeaderNew.TotalCAQty5 = $scope.CutPlanEditHeaderNew.TotalCAQty5 + $scope.CutPlanDetailsR5List[i].AllotedQtyR5;
            }
            $scope.SaveDetailsR5();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveDetailsR5 = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.CutPlanDetailsR5List.length; i++) {
                if ($scope.CutPlanDetailsR5List[i].Ratio5 > 0) {
                    $scope.SaveList.push($scope.CutPlanDetailsR5List[i]);
                }
                $scope.CutPlanEditHeaderNew.R5Id = $scope.CutPlanDetailsR5List[i].R5Id;
                $scope.CutPlanEditHeaderNew.UserNameR5 = $scope.CutPlanDetailsR5List[i].UserNameR5;
                $scope.CutPlanEditHeaderNew.MarkerIdR5 = $scope.CutPlanDetailsR5List[i].MarkerIdR5;
                $scope.CutPlanEditHeaderNew.PackingTypeIdR5 = $scope.CutPlanDetailsR5List[i].PackingTypeIdR5;
                $scope.CutPlanDetailsR5List[i].NoOfPlyR5 = $scope.CutPlanEditHeaderNew.NoOfPlyR5;
            }
            $http({
                method: "POST",
                url: 'Productions/CutPlanEdit/CreateCutPlanEditR5Data?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId,
                data: {
                    'data': $scope.CutPlanEditHeaderNew,
                    'DataList': $scope.SaveList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadCutPlanDetailsR5List();
                    $scope.LoadCutPlanDetailsBalanceList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.CutPlanDetailsR6List = [];
    $scope.LoadCutPlanDetailsR6List = function () {
        $scope.CutPlanEditHeaderNew.TotalRatio6 = 0;
        $scope.CutPlanEditHeaderNew.TotalCAQty6 = 0;
        $scope.CutPlanDetailsR6List = [];
        $http({
            method: 'Get',
            url: 'Productions/CutPlanEdit/GetCutPlanDetailsR6List?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanDetailsR6List = response.data;
            for (var i = 0; i < $scope.CutPlanDetailsR6List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio6 = $scope.CutPlanEditHeaderNew.TotalRatio6 + $scope.CutPlanDetailsR6List[i].Ratio6;
                $scope.CutPlanEditHeaderNew.TotalCAQty6 = $scope.CutPlanEditHeaderNew.TotalCAQty6 + $scope.CutPlanDetailsR6List[i].AllotedQtyR6;
            }
            $scope.CutPlanEditHeaderNew.NoOfPlyR6 = $scope.CutPlanDetailsR6List[0].NoOfPlyR6;
        }
        )
    }

    $scope.NoOfPlyR6ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio6 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty6 = 0;
            for (var i = 0; i < $scope.CutPlanDetailsR6List.length; i++) {
                $scope.CutPlanDetailsR6List[i].Ratio6 = Math.floor($scope.CutPlanDetailsR6List[i].BalanceToAllotedR5 / $scope.CutPlanEditHeaderNew.NoOfPlyR6);
                $scope.CutPlanDetailsR6List[i].AllotedQtyR6 = $scope.CutPlanDetailsR6List[i].Ratio6 * $scope.CutPlanEditHeaderNew.NoOfPlyR6;
                $scope.CutPlanEditHeaderNew.TotalRatio6 = $scope.CutPlanEditHeaderNew.TotalRatio6 + $scope.CutPlanDetailsR6List[i].Ratio6;
                $scope.CutPlanEditHeaderNew.TotalCAQty6 = $scope.CutPlanEditHeaderNew.TotalCAQty6 + $scope.CutPlanDetailsR6List[i].AllotedQtyR6;
            }
            $scope.SaveDetailsR6();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Ratio6Value = 0;
    $scope.Ratio6Change = function (data) {
        try {
            $scope.CutPlanEditHeaderNew.TotalRatio6 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty6 = 0;
            $scope.Ratio6Value = 0;
            $scope.Ratio6Value = data.Ratio6;
            if ($scope.Ratio6Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.AllotedQtyR6 = $scope.Ratio6Value * $scope.CutPlanEditHeaderNew.NoOfPlyR6;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR6List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio6 = $scope.CutPlanEditHeaderNew.TotalRatio6 + $scope.CutPlanDetailsR6List[i].Ratio6;
                $scope.CutPlanEditHeaderNew.TotalCAQty6 = $scope.CutPlanEditHeaderNew.TotalCAQty6 + $scope.CutPlanDetailsR6List[i].AllotedQtyR6;
            }
            $scope.SaveDetailsR6();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveDetailsR6 = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.CutPlanDetailsR6List.length; i++) {
                if ($scope.CutPlanDetailsR6List[i].Ratio6 > 0) {
                    $scope.SaveList.push($scope.CutPlanDetailsR6List[i]);
                }
                $scope.CutPlanEditHeaderNew.R6Id = $scope.CutPlanDetailsR6List[i].R6Id;
                $scope.CutPlanEditHeaderNew.UserNameR6 = $scope.CutPlanDetailsR6List[i].UserNameR6;
                $scope.CutPlanEditHeaderNew.MarkerIdR6 = $scope.CutPlanDetailsR6List[i].MarkerIdR6;
                $scope.CutPlanEditHeaderNew.PackingTypeIdR6 = $scope.CutPlanDetailsR6List[i].PackingTypeIdR6;
                $scope.CutPlanDetailsR6List[i].NoOfPlyR6 = $scope.CutPlanEditHeaderNew.NoOfPlyR6;
            }
            $http({
                method: "POST",
                url: 'Productions/CutPlanEdit/CreateCutPlanEditR6Data?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId,
                data: {
                    'data': $scope.CutPlanEditHeaderNew,
                    'DataList': $scope.SaveList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadCutPlanDetailsR6List();
                    $scope.LoadCutPlanDetailsBalanceList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.CutPlanDetailsR7List = [];
    $scope.LoadCutPlanDetailsR7List = function () {
        $scope.CutPlanEditHeaderNew.TotalRatio7 = 0;
        $scope.CutPlanEditHeaderNew.TotalCAQty7 = 0;
        $scope.CutPlanDetailsR7List = [];
        $http({
            method: 'Get',
            url: 'Productions/CutPlanEdit/GetCutPlanDetailsR7List?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanDetailsR7List = response.data;
            for (var i = 0; i < $scope.CutPlanDetailsR7List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio7 = $scope.CutPlanEditHeaderNew.TotalRatio7 + $scope.CutPlanDetailsR7List[i].Ratio7;
                $scope.CutPlanEditHeaderNew.TotalCAQty7 = $scope.CutPlanEditHeaderNew.TotalCAQty7 + $scope.CutPlanDetailsR7List[i].AllotedQtyR7;
            }
            $scope.CutPlanEditHeaderNew.NoOfPlyR7 = $scope.CutPlanDetailsR7List[0].NoOfPlyR7;
        }
        )
    }

    $scope.NoOfPlyR7ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio7 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty7 = 0;
            for (var i = 0; i < $scope.CutPlanDetailsR7List.length; i++) {
                $scope.CutPlanDetailsR7List[i].Ratio7 = Math.floor($scope.CutPlanDetailsR7List[i].BalanceToAllotedR6 / $scope.CutPlanEditHeaderNew.NoOfPlyR7);
                $scope.CutPlanDetailsR7List[i].AllotedQtyR7 = $scope.CutPlanDetailsR7List[i].Ratio7 * $scope.CutPlanEditHeaderNew.NoOfPlyR7;
                $scope.CutPlanEditHeaderNew.TotalRatio7 = $scope.CutPlanEditHeaderNew.TotalRatio7 + $scope.CutPlanDetailsR7List[i].Ratio7;
                $scope.CutPlanEditHeaderNew.TotalCAQty7 = $scope.CutPlanEditHeaderNew.TotalCAQty7 + $scope.CutPlanDetailsR7List[i].AllotedQtyR7;
            }
            $scope.SaveDetailsR7();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Ratio7Value = 0;
    $scope.Ratio7Change = function (data) {
        try {
            $scope.CutPlanEditHeaderNew.TotalRatio7 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty7 = 0;
            $scope.Ratio7Value = 0;
            $scope.Ratio7Value = data.Ratio7;
            if ($scope.Ratio7Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.AllotedQtyR7 = $scope.Ratio7Value * $scope.CutPlanEditHeaderNew.NoOfPlyR7;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR7List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio7 = $scope.CutPlanEditHeaderNew.TotalRatio7 + $scope.CutPlanDetailsR7List[i].Ratio7;
                $scope.CutPlanEditHeaderNew.TotalCAQty7 = $scope.CutPlanEditHeaderNew.TotalCAQty7 + $scope.CutPlanDetailsR7List[i].AllotedQtyR7;
            }
            $scope.SaveDetailsR7();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveDetailsR7 = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.CutPlanDetailsR7List.length; i++) {
                if ($scope.CutPlanDetailsR7List[i].Ratio7 > 0) {
                    $scope.SaveList.push($scope.CutPlanDetailsR7List[i]);
                }
                $scope.CutPlanEditHeaderNew.R7Id = $scope.CutPlanDetailsR7List[i].R7Id;
                $scope.CutPlanEditHeaderNew.UserNameR7 = $scope.CutPlanDetailsR7List[i].UserNameR7;
                $scope.CutPlanEditHeaderNew.MarkerIdR7 = $scope.CutPlanDetailsR7List[i].MarkerIdR7;
                $scope.CutPlanEditHeaderNew.PackingTypeIdR7 = $scope.CutPlanDetailsR7List[i].PackingTypeIdR7;
                $scope.CutPlanDetailsR7List[i].NoOfPlyR7 = $scope.CutPlanEditHeaderNew.NoOfPlyR7;
            }
            $http({
                method: "POST",
                url: 'Productions/CutPlanEdit/CreateCutPlanEditR7Data?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId,
                data: {
                    'data': $scope.CutPlanEditHeaderNew,
                    'DataList': $scope.SaveList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadCutPlanDetailsR7List();
                    $scope.LoadCutPlanDetailsBalanceList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.CutPlanDetailsR8List = [];
    $scope.LoadCutPlanDetailsR8List = function () {
        $scope.CutPlanEditHeaderNew.TotalRatio8 = 0;
        $scope.CutPlanEditHeaderNew.TotalCAQty8 = 0;
        $scope.CutPlanDetailsR8List = [];
        $http({
            method: 'Get',
            url: 'Productions/CutPlanEdit/GetCutPlanDetailsR8List?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanDetailsR8List = response.data;
            for (var i = 0; i < $scope.CutPlanDetailsR8List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio8 = $scope.CutPlanEditHeaderNew.TotalRatio8 + $scope.CutPlanDetailsR8List[i].Ratio8;
                $scope.CutPlanEditHeaderNew.TotalCAQty8 = $scope.CutPlanEditHeaderNew.TotalCAQty8 + $scope.CutPlanDetailsR8List[i].AllotedQtyR8;
            }
            $scope.CutPlanEditHeaderNew.NoOfPlyR8 = $scope.CutPlanDetailsR8List[0].NoOfPlyR8;
        }
        )
    }

    $scope.NoOfPlyR8ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio8 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty8 = 0;
            for (var i = 0; i < $scope.CutPlanDetailsR8List.length; i++) {
                $scope.CutPlanDetailsR8List[i].Ratio8 = Math.floor($scope.CutPlanDetailsR8List[i].BalanceToAllotedR7 / $scope.CutPlanEditHeaderNew.NoOfPlyR8);
                $scope.CutPlanDetailsR8List[i].AllotedQtyR8 = $scope.CutPlanDetailsR8List[i].Ratio8 * $scope.CutPlanEditHeaderNew.NoOfPlyR8;
                $scope.CutPlanEditHeaderNew.TotalRatio8 = $scope.CutPlanEditHeaderNew.TotalRatio8 + $scope.CutPlanDetailsR8List[i].Ratio8;
                $scope.CutPlanEditHeaderNew.TotalCAQty8 = $scope.CutPlanEditHeaderNew.TotalCAQty8 + $scope.CutPlanDetailsR8List[i].AllotedQtyR8;
            }
            $scope.SaveDetailsR8();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Ratio8Value = 0;
    $scope.Ratio8Change = function (data) {
        try {
            $scope.CutPlanEditHeaderNew.TotalRatio8 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty8 = 0;
            $scope.Ratio8Value = 0;
            $scope.Ratio8Value = data.Ratio8;
            if ($scope.Ratio8Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.AllotedQtyR8 = $scope.Ratio8Value * $scope.CutPlanEditHeaderNew.NoOfPlyR8;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR8List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio8 = $scope.CutPlanEditHeaderNew.TotalRatio8 + $scope.CutPlanDetailsR8List[i].Ratio8;
                $scope.CutPlanEditHeaderNew.TotalCAQty8 = $scope.CutPlanEditHeaderNew.TotalCAQty8 + $scope.CutPlanDetailsR8List[i].AllotedQtyR8;
            }
            $scope.SaveDetailsR8();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveDetailsR8 = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.CutPlanDetailsR8List.length; i++) {
                if ($scope.CutPlanDetailsR8List[i].Ratio8 > 0) {
                    $scope.SaveList.push($scope.CutPlanDetailsR8List[i]);
                }
                $scope.CutPlanEditHeaderNew.R8Id = $scope.CutPlanDetailsR8List[i].R8Id;
                $scope.CutPlanEditHeaderNew.UserNameR8 = $scope.CutPlanDetailsR8List[i].UserNameR8;
                $scope.CutPlanEditHeaderNew.MarkerIdR8 = $scope.CutPlanDetailsR8List[i].MarkerIdR8;
                $scope.CutPlanEditHeaderNew.PackingTypeIdR8 = $scope.CutPlanDetailsR8List[i].PackingTypeIdR8;
                $scope.CutPlanDetailsR8List[i].NoOfPlyR8 = $scope.CutPlanEditHeaderNew.NoOfPlyR8;
            }
            $http({
                method: "POST",
                url: 'Productions/CutPlanEdit/CreateCutPlanEditR8Data?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId,
                data: {
                    'data': $scope.CutPlanEditHeaderNew,
                    'DataList': $scope.SaveList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadCutPlanDetailsR8List();
                    $scope.LoadCutPlanDetailsBalanceList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.CutPlanDetailsR9List = [];
    $scope.LoadCutPlanDetailsR9List = function () {
        $scope.CutPlanEditHeaderNew.TotalRatio9 = 0;
        $scope.CutPlanEditHeaderNew.TotalCAQty9 = 0;
        $scope.CutPlanDetailsR9List = [];
        $http({
            method: 'Get',
            url: 'Productions/CutPlanEdit/GetCutPlanDetailsR9List?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanDetailsR9List = response.data;
            for (var i = 0; i < $scope.CutPlanDetailsR9List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio9 = $scope.CutPlanEditHeaderNew.TotalRatio9 + $scope.CutPlanDetailsR9List[i].Ratio9;
                $scope.CutPlanEditHeaderNew.TotalCAQty9 = $scope.CutPlanEditHeaderNew.TotalCAQty9 + $scope.CutPlanDetailsR9List[i].AllotedQtyR9;
            }
            $scope.CutPlanEditHeaderNew.NoOfPlyR9 = $scope.CutPlanDetailsR9List[0].NoOfPlyR9;
        }
        )
    }

    $scope.NoOfPlyR9ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio9 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty9 = 0;
            for (var i = 0; i < $scope.CutPlanDetailsR9List.length; i++) {
                $scope.CutPlanDetailsR9List[i].Ratio9 = Math.floor($scope.CutPlanDetailsR9List[i].BalanceToAllotedR8 / $scope.CutPlanEditHeaderNew.NoOfPlyR9);
                $scope.CutPlanDetailsR9List[i].AllotedQtyR9 = $scope.CutPlanDetailsR9List[i].Ratio9 * $scope.CutPlanEditHeaderNew.NoOfPlyR9;
                $scope.CutPlanEditHeaderNew.TotalRatio9 = $scope.CutPlanEditHeaderNew.TotalRatio9 + $scope.CutPlanDetailsR9List[i].Ratio9;
                $scope.CutPlanEditHeaderNew.TotalCAQty9 = $scope.CutPlanEditHeaderNew.TotalCAQty9 + $scope.CutPlanDetailsR9List[i].AllotedQtyR9;
            }
            $scope.SaveDetailsR9();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Ratio9Value = 0;
    $scope.Ratio9Change = function (data) {
        try {
            $scope.CutPlanEditHeaderNew.TotalRatio9 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty9 = 0;
            $scope.Ratio9Value = 0;
            $scope.Ratio9Value = data.Ratio9;
            if ($scope.Ratio9Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.AllotedQtyR9 = $scope.Ratio9Value * $scope.CutPlanEditHeaderNew.NoOfPlyR9;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR9List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio9 = $scope.CutPlanEditHeaderNew.TotalRatio9 + $scope.CutPlanDetailsR9List[i].Ratio9;
                $scope.CutPlanEditHeaderNew.TotalCAQty9 = $scope.CutPlanEditHeaderNew.TotalCAQty9 + $scope.CutPlanDetailsR9List[i].AllotedQtyR9;
            }
            $scope.SaveDetailsR9();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveDetailsR9 = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.CutPlanDetailsR9List.length; i++) {
                if ($scope.CutPlanDetailsR9List[i].Ratio9 > 0) {
                    $scope.SaveList.push($scope.CutPlanDetailsR9List[i]);
                }
                $scope.CutPlanEditHeaderNew.R9Id = $scope.CutPlanDetailsR9List[i].R9Id;
                $scope.CutPlanEditHeaderNew.UserNameR9 = $scope.CutPlanDetailsR9List[i].UserNameR9;
                $scope.CutPlanEditHeaderNew.MarkerIdR9 = $scope.CutPlanDetailsR9List[i].MarkerIdR9;
                $scope.CutPlanEditHeaderNew.PackingTypeIdR9 = $scope.CutPlanDetailsR9List[i].PackingTypeIdR9;
                $scope.CutPlanDetailsR9List[i].NoOfPlyR9 = $scope.CutPlanEditHeaderNew.NoOfPlyR9;
            }
            $http({
                method: "POST",
                url: 'Productions/CutPlanEdit/CreateCutPlanEditR9Data?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId,
                data: {
                    'data': $scope.CutPlanEditHeaderNew,
                    'DataList': $scope.SaveList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadCutPlanDetailsR9List();
                    $scope.LoadCutPlanDetailsBalanceList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.CutPlanDetailsR10List = [];
    $scope.LoadCutPlanDetailsR10List = function () {
        $scope.CutPlanEditHeaderNew.TotalRatio10 = 0;
        $scope.CutPlanEditHeaderNew.TotalCAQty10 = 0;
        $scope.CutPlanDetailsR10List = [];
        $http({
            method: 'Get',
            url: 'Productions/CutPlanEdit/GetCutPlanDetailsR10List?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanDetailsR10List = response.data;
            for (var i = 0; i < $scope.CutPlanDetailsR10List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio10 = $scope.CutPlanEditHeaderNew.TotalRatio10 + $scope.CutPlanDetailsR10List[i].Ratio10;
                $scope.CutPlanEditHeaderNew.TotalCAQty10 = $scope.CutPlanEditHeaderNew.TotalCAQty10 + $scope.CutPlanDetailsR10List[i].AllotedQtyR10;
            }
            $scope.CutPlanEditHeaderNew.NoOfPlyR10 = $scope.CutPlanDetailsR10List[0].NoOfPlyR10;
        }
        )
    }

    $scope.NoOfPlyR10ChangeManual = function () {
        try {

            $scope.CutPlanEditHeaderNew.TotalRatio10 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty10 = 0;
            for (var i = 0; i < $scope.CutPlanDetailsR10List.length; i++) {
                $scope.CutPlanDetailsR10List[i].Ratio10 = Math.floor($scope.CutPlanDetailsR10List[i].BalanceToAllotedR9 / $scope.CutPlanEditHeaderNew.NoOfPlyR10);
                $scope.CutPlanDetailsR10List[i].AllotedQtyR10 = $scope.CutPlanDetailsR10List[i].Ratio10 * $scope.CutPlanEditHeaderNew.NoOfPlyR10;
                $scope.CutPlanEditHeaderNew.TotalRatio10 = $scope.CutPlanEditHeaderNew.TotalRatio10 + $scope.CutPlanDetailsR10List[i].Ratio10;
                $scope.CutPlanEditHeaderNew.TotalCAQty10 = $scope.CutPlanEditHeaderNew.TotalCAQty10 + $scope.CutPlanDetailsR10List[i].AllotedQtyR10;
            }
            $scope.SaveDetailsR10();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Ratio10Value = 0;
    $scope.Ratio10Change = function (data) {
        try {
            $scope.CutPlanEditHeaderNew.TotalRatio10 = 0;
            $scope.CutPlanEditHeaderNew.TotalCAQty10 = 0;
            $scope.Ratio10Value = 0;
            $scope.Ratio10Value = data.Ratio10;
            if ($scope.Ratio10Value === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else {
                data.AllotedQtyR10 = $scope.Ratio10Value * $scope.CutPlanEditHeaderNew.NoOfPlyR10;
            }
            for (var i = 0; i < $scope.CutPlanDetailsR10List.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalRatio10 = $scope.CutPlanEditHeaderNew.TotalRatio10 + $scope.CutPlanDetailsR10List[i].Ratio10;
                $scope.CutPlanEditHeaderNew.TotalCAQty10 = $scope.CutPlanEditHeaderNew.TotalCAQty10 + $scope.CutPlanDetailsR10List[i].AllotedQtyR10;
            }
            $scope.SaveDetailsR10();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveDetailsR10 = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.CutPlanDetailsR10List.length; i++) {
                if ($scope.CutPlanDetailsR10List[i].Ratio10 > 0) {
                    $scope.SaveList.push($scope.CutPlanDetailsR10List[i]);
                }
                $scope.CutPlanEditHeaderNew.R10Id = $scope.CutPlanDetailsR10List[i].R10Id;
                $scope.CutPlanEditHeaderNew.UserNameR10 = $scope.CutPlanDetailsR10List[i].UserNameR10;
                $scope.CutPlanEditHeaderNew.MarkerIdR10 = $scope.CutPlanDetailsR10List[i].MarkerIdR10;
                $scope.CutPlanEditHeaderNew.PackingTypeIdR10 = $scope.CutPlanDetailsR10List[i].PackingTypeIdR10;
                $scope.CutPlanDetailsR10List[i].NoOfPlyR10 = $scope.CutPlanEditHeaderNew.NoOfPlyR10;
            }
            $http({
                method: "POST",
                url: 'Productions/CutPlanEdit/CreateCutPlanEditR10Data?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId,
                data: {
                    'data': $scope.CutPlanEditHeaderNew,
                    'DataList': $scope.SaveList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadCutPlanDetailsR10List();
                    $scope.LoadCutPlanDetailsBalanceList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.CutPlanDetailsBalanceList = [];
    $scope.LoadCutPlanDetailsBalanceList = function () {
        $scope.CutPlanEditHeaderNew.TotalFinalQty = 0;
        $scope.CutPlanEditHeaderNew.TotalCPQ = 0;
        $scope.CutPlanEditHeaderNew.TotalEPQ = 0;
        $scope.CutPlanDetailsBalanceList = [];
        $http({
            method: 'Get',
            url: 'Productions/CutPlanEdit/GetCutPlanDetailsBalanceList?MasterPlanId=' + $scope.CutPlanEditHeaderNew.MasterPlanId + '&ColorId=' + $scope.CutPlanEditHeaderNew.ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanDetailsBalanceList = response.data;
            for (var i = 0; i < $scope.CutPlanDetailsBalanceList.length; i++) {
                $scope.CutPlanEditHeaderNew.TotalFinalQty = $scope.CutPlanEditHeaderNew.TotalFinalQty + $scope.CutPlanDetailsBalanceList[i].FinalQty;
                $scope.CutPlanEditHeaderNew.TotalCPQ = $scope.CutPlanEditHeaderNew.TotalCPQ + $scope.CutPlanDetailsBalanceList[i].AllotedQty;
                $scope.CutPlanEditHeaderNew.TotalEPQ = $scope.CutPlanEditHeaderNew.TotalEPQ + $scope.CutPlanDetailsBalanceList[i].BalanceQty;
            }
        }
        )
    }
}