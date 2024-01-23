'use strict';
CutPlanController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function CutPlanController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Cut Plan';
    $scope.Action = 'Save';
    $scope.path = 'Productions/CutPlan/';
   
    $scope.PackingTypeLists = [];
    $scope.GetPackingTypeLists = function () {
        $http({
            method: 'GET',
            url: 'Productions/CutPlan/GetPackingTypeLists'
        }).then(function successCallback(response) {
            $scope.PackingTypeLists = response.data;
        });
    }
    $scope.GetPackingTypeLists();

    $scope.processList = [];
    $scope.GetProcessList = function () {
        $http({
            method: 'GET',
            url: 'Productions/CutPlan/GetMPDProcessList'
        }).then(function successCallback(response) {
            $scope.processList = response.data;
        });
    }
    $scope.GetProcessList();

    $scope.SKU1ColorLists = [];
    $scope.GetSKU1ColorLists = function (MPId) {
        $http({
            method: 'GET',
            url: 'Productions/CutPlan/GetSKU1ColorLists?MasterPlanId=' + MPId
        }).then(function successCallback(response) {
            $scope.SKU1ColorLists = response.data;
        });
    }
    

    $scope.UserNameList = [];
    $scope.GetUserNameList = function (PId) {
        $http({
            method: 'GET',
            url: 'Productions/CutPlan/GetUserNameList?ProcessId=' + PId
        }).then(function successCallback(response) {
            $scope.UserNameList = response.data;
            $scope.CutPlanHeaderNew.UserName = $scope.UserNameList[0].Text;
        });
    }

    $scope.CutPlanHeader = {
        Id: null
        , ProcessId: null
        , UserName: null
    };
    $scope.CutPlanHeaderNew = Object.assign({}, $scope.CutPlanHeader);

    $scope.cutplan = {
        Id: null
        , ProcessId: null
        , EntityId: null
        , Process: null
        , Entity: null
        , PlanName: null
        , UserId: null
        , UserName: null
        , PlanStatus: null
        , ResponsiblePersonId: null
        , ResponsiblePerson: null
        , Remarks: null
        , LineItem: false
        , SKU1: false
        , SKU2: false
        , LineItemTotalQty: null
        , SKU1TotalQty: null
        , SKU2TotalQty: null
        , TotalFinalQty: 0
        , TotalRatio: 0
        , TotalCAQty: 0

    };
    $scope.cutplanNew = Object.assign({}, $scope.cutplan);

    $scope.allotedheader = {
        Id: null
        , MasterPlanId: null
        , PackingTypeId: null
        , UserName: null
        , MarkerId: null
        , Remarks: null
        , NoOfPly: 0
        , SKU1ColorId: null
    };
    $scope.allotedheaderNew = Object.assign({}, $scope.allotedheader);
  
    $scope.MasterPlanList = [];
    $scope.LoadMasterPlanList = function () {
        $http({
            method: 'Get',
            url: 'Productions/CutPlan/GetMasterPlanListForCutPlan?ProcessId=' + $scope.CutPlanHeaderNew.ProcessId
        }).then(function successCallback(response) {
            $scope.MasterPlanList = response.data;
            var gridObj = $("#GridMasterPlanForCutPlan").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.View = function () {
        $scope.LoadMasterPlanList();
    }

    $scope.MaterialID = "";
    $scope.isAlternative = -1;
    $scope.rowDataBound = function rowDataBound(e) {
        if ($scope.MaterialID != e.data.ProductionGrouping + e.data.MaterialMasterId) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.MaterialID = e.data.ProductionGrouping + e.data.MaterialMasterId;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", "#90EE90");
        else
            e.row.css("background-color", '##013220');
    }

    $scope.GetCutPlan = function (args) {
        $scope.MasterPlanId = args.data.Id;
        $scope.LineItem = args.data.LineItem;
        $scope.SKU1 = args.data.SKU1;
        $scope.SKU2 = args.data.SKU2;
        $scope.cutplanNew.Id = args.data.Id;
        $scope.cutplanNew.PlanName = args.data.PlanName;
        $scope.cutplanNew.PlanStatus = args.data.PlanStatus;
        $scope.cutplanNew.UserName = args.data.UserName;
        $scope.cutplanNew.ResponsiblePerson = args.data.ResponsiblePerson;
        $scope.cutplanNew.Process = args.data.Process;
        $scope.cutplanNew.Entity = args.data.Entity;
        $scope.cutplanNew.LineItem = args.data.LineItem;
        $scope.cutplanNew.SKU1 = args.data.SKU1;
        $scope.cutplanNew.SKU2 = args.data.SKU2;
        $scope.cutplanNew.Remarks = args.data.Remarks;
        getCutPlanList();
        $scope.LoadMPDLineItemList();
        $scope.LoadMPDSKU1List();
        $scope.LoadMPDSKU2List();
        $scope.GetSKU1ColorLists($scope.cutplanNew.Id);
        //$scope.LoadCutPlanList();
        angular.element(document.querySelector('#CutPlanPopUp')).modal('show');
    }

    $scope.closeCutPlanPopUp = function () {
        //CutPlanClearFields();
        CutPlanCloseFields();
        angular.element(document.querySelector('#CutPlanPopUp')).modal('hide');
    }

    $scope.CutPlanListSelected = [];
    function getCutPlanList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetCutPlanList?ProcessId=' + $scope.CutPlanHeaderNew.ProcessId + '&MasterPlanId=' + $scope.MasterPlanId,
        }).then(function successCallback(response) {
            $scope.CutPlanListSelected = response.data;
            var gridObj = $("#GridSOItemSelectedForCutPlan").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        });
    }

    $scope.refreshTemplateCutPlan = function (args) {
        $("#Cheadchk").ejCheckBox({ "change": CheckBoxSelectAllCutPlan });
    };
    function CheckBoxSelectAllCutPlan(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSOItemSelectedForCutPlan").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.CutPlanListSelected.length; i++) {
                $scope.CutPlanListSelected[i].Status = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Status = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSOItemSelectedForCutPlan").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };
    $scope.SOPlanPer = 0;
    $scope.SOQty = 0;
    $scope.SOPlanQtyCal = function (data) {
        try {
            $scope.SOPlanPer = 0;
            $scope.SOQty = 0;
            $scope.SOPlanQty = 0;
            $scope.SOPlanPer = data.data.SOPlanPercentage;
            $scope.SOQty = data.data.Qty;
            data.data.SOPlanQty = $scope.SOQty + ($scope.SOPlanPer * $scope.SOQty / 100);
            var gridObj = $("#GridSOItemSelectedForCutPlan").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };



    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.MPDLineItemList = [];
    $scope.LoadMPDLineItemList = function () {
        $http({
            method: 'Get',
            url: 'Productions/CutPlan/GetMPDLineItemList?MasterPlanId=' + $scope.cutplanNew.Id
        }).then(function successCallback(response) {
            $scope.MPDLineItemList = response.data;
            $scope.cutplanNew.LineItemTotalQty = $scope.MPDLineItemList[0].LineItemTotalQty;
            $scope.cutplanNew.SKU1TotalQty = $scope.MPDLineItemList[0].SKU1TotalQty;
            $scope.cutplanNew.SKU2TotalQty = $scope.MPDLineItemList[0].SKU2TotalQty;
            var gridObj = $("#GridCutPlanLineItem").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.MPDSKU1List = [];
    $scope.LoadMPDSKU1List = function () {
        $http({
            method: 'Get',
            url: 'Productions/CutPlan/GetMPDSKU1List?MasterPlanId=' + $scope.cutplanNew.Id
        }).then(function successCallback(response) {
            $scope.MPDSKU1List = response.data;
            var gridObj = $("#GridCutPlanSKU1").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.MPDSKU2List = [];
    $scope.LoadMPDSKU2List = function () {
        $http({
            method: 'Get',
            url: 'Productions/CutPlan/GetMPDSKU2List?MasterPlanId=' + $scope.cutplanNew.Id
        }).then(function successCallback(response) {
            $scope.MPDSKU2List = response.data;
            var gridObj = $("#GridCutPlanSKU2").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.CutPlanList = [];
    $scope.CutPlanMinQtyList = [];
    $scope.MinimumQty = 0;
    $scope.LoadCutPlanList = function () {
        $scope.cutplanNew.TotalFinalQty = 0;
        $scope.cutplanNew.TotalRatio = 0;
        $scope.cutplanNew.TotalCAQty = 0;
        $scope.CutPlanMinQtyList = [];
        $scope.CutPlanList = [];
        $http({
            method: 'Get',
            url: 'Productions/CutPlan/GetCutPlanQtyList?MasterPlanId=' + $scope.cutplanNew.Id + '&LineItem=' + $scope.LineItem + '&SKU1=' + $scope.SKU1 + '&SKU2=' + $scope.SKU2 + '&MinQty=' + $scope.allotedheaderNew.NoOfPly + '&SKU1ColorId=' + $scope.allotedheaderNew.SKU1ColorId
        }).then(function successCallback(response) {
            $scope.CutPlanList = response.data;
            for (var i = 0; i < $scope.CutPlanList.length; i++) {
                if ($scope.CutPlanList[i].BalanceQty == 0) {
                    $scope.CutPlanList[i].Status = false;
                }
                if ($scope.CutPlanList[i].BalanceQty > 0) {
                    $scope.CutPlanMinQtyList.push($scope.CutPlanList[i].BalanceQty);
                }
                $scope.cutplanNew.TotalFinalQty = $scope.cutplanNew.TotalFinalQty + $scope.CutPlanList[i].FinalQty;
                $scope.cutplanNew.TotalRatio = $scope.cutplanNew.TotalRatio + $scope.CutPlanList[i].Ratio;
                $scope.cutplanNew.TotalCAQty = $scope.cutplanNew.TotalCAQty + $scope.CutPlanList[i].CurrentAllotedQty;
            }
            /*if ($scope.allotedheaderNew.NoOfPly == 0) {*/
            $scope.allotedheaderNew.NoOfPly = Math.min.apply(null, $scope.CutPlanMinQtyList);
            $scope.MinimumQty = $scope.allotedheaderNew.NoOfPly;
            $scope.MinQtyChangeManual();
          /*  }*/
            var gridObj = $("#GridCutPlanLineItemQty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridCutPlanSKU1Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridCutPlanSKU2Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }


    $scope.CutPlanMinimumQtyList = [];
    $scope.MinQtyChange = function () {
        try {
            $scope.CutPlanMinimumQtyList = [];
            $scope.CutPlanRatioList = [];
            for (var i = 0; i < $scope.CutPlanList.length; i++) {
                if ($scope.CutPlanList[i].Status == true) {
                    $scope.CutPlanMinimumQtyList.push($scope.CutPlanList[i].BalanceQty);
                }
            }
            $scope.allotedheaderNew.NoOfPly = Math.min.apply(null, $scope.CutPlanMinimumQtyList);
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.MinQtyChangeAuto = function () {
        try {

            $scope.cutplanNew.TotalRatio = 0;
            $scope.cutplanNew.TotalCAQty = 0;
            $scope.CutPlanRatioList = [];
            for (var i = 0; i < $scope.CutPlanList.length; i++) {
                if ($scope.CutPlanList[i].Status == true) {
                    $scope.CutPlanList[i].Ratio = Math.floor($scope.CutPlanList[i].BalanceQty / $scope.allotedheaderNew.NoOfPly);
                    $scope.CutPlanList[i].CurrentAllotedQty = $scope.CutPlanList[i].Ratio * $scope.allotedheaderNew.NoOfPly;
                    $scope.cutplanNew.TotalRatio = $scope.cutplanNew.TotalRatio + $scope.CutPlanList[i].Ratio;
                    $scope.cutplanNew.TotalCAQty = $scope.cutplanNew.TotalCAQty + $scope.CutPlanList[i].CurrentAllotedQty;
                }
            }
            var gridObj = $("#GridCutPlanLineItemQty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridCutPlanSKU1Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridCutPlanSKU2Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.MinQtyChangeManual = function () {
        try {
           
            $scope.cutplanNew.TotalRatio = 0;
            $scope.cutplanNew.TotalCAQty = 0;
            if ($scope.allotedheaderNew.NoOfPly > $scope.MinimumQty)
            {
                throw "MinimumQty should not be greater than the minimumqty of Balance to be closed";
            }
            $scope.CutPlanRatioList = [];
            for (var i = 0; i < $scope.CutPlanList.length; i++) {
                if ($scope.CutPlanList[i].Status == true) {
                    $scope.CutPlanList[i].Ratio = Math.floor($scope.CutPlanList[i].BalanceQty / $scope.allotedheaderNew.NoOfPly);
                    $scope.CutPlanList[i].CurrentAllotedQty = $scope.CutPlanList[i].Ratio * $scope.allotedheaderNew.NoOfPly;
                    $scope.cutplanNew.TotalRatio = $scope.cutplanNew.TotalRatio + $scope.CutPlanList[i].Ratio;
                    $scope.cutplanNew.TotalCAQty = $scope.cutplanNew.TotalCAQty + $scope.CutPlanList[i].CurrentAllotedQty;
                }
            }
            var gridObj = $("#GridCutPlanLineItemQty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridCutPlanSKU1Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridCutPlanSKU2Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.RatioValue = 0;
    $scope.RatioChange = function (data) {
        try {
            $scope.cutplanNew.TotalFinalQty = 0;
            $scope.cutplanNew.TotalRatio = 0;
            $scope.cutplanNew.TotalCAQty = 0;
            $scope.RatioValue = 0;
            $scope.RatioValue = data.data.Ratio;
            if ($scope.RatioValue === "0") {
                throw "O Ratio Value should not be allowed";
            }
            else
            {
                data.data.CurrentAllotedQty = $scope.RatioValue * $scope.allotedheaderNew.NoOfPly;
            }
            for (var i = 0; i < $scope.CutPlanList.length; i++) {
                $scope.cutplanNew.TotalFinalQty = $scope.cutplanNew.TotalFinalQty + $scope.CutPlanList[i].FinalQty;
                $scope.cutplanNew.TotalRatio = $scope.cutplanNew.TotalRatio + $scope.CutPlanList[i].Ratio;
                $scope.cutplanNew.TotalCAQty = $scope.cutplanNew.TotalCAQty + $scope.CutPlanList[i].CurrentAllotedQty;
            }
            var gridObj = $("#GridCutPlanLineItemQty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridCutPlanSKU1Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridCutPlanSKU2Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();

        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.refreshTemplateCPLineItem = function (args) {
        $("#Lheadchk").ejCheckBox({ "change": CheckBoxSelectAllCutPlanLineItem });
    };
    function CheckBoxSelectAllCutPlanLineItem(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridCutPlanLineItemQty").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.CutPlanList.length; i++) {
                if ($scope.CutPlanList[0].BalanceQty > 0) {
                    $scope.CutPlanList[i].Status = ChkOrUnchk;
                }
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                if ($scope.filtered[0].BalanceQty > 0) {
                    filtered[j].Status = ChkOrUnchk;
                }
            }
        }
        var gridObj = $("#GridCutPlanLineItemQty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.refreshTemplateCPSKU1 = function (args) {
        $("#S1headchk").ejCheckBox({ "change": CheckBoxSelectAllCutPlanSKU1 });
    };
    function CheckBoxSelectAllCutPlanSKU1(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridCutPlanSKU1Qty").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.CutPlanList.length; i++) {
                if ($scope.CutPlanList[0].BalanceQty > 0) {
                    $scope.CutPlanList[i].Status = ChkOrUnchk;
                }
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                if ($scope.filtered[0].BalanceQty > 0) {
                    filtered[j].Status = ChkOrUnchk;
                }
            }
        }
        var gridObj = $("#GridCutPlanSKU1Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.refreshTemplateCPSKU2 = function (args) {
        $("#S2headchk").ejCheckBox({ "change": CheckBoxSelectAllCutPlanSKU2 });
    };
    function CheckBoxSelectAllCutPlanSKU2(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridCutPlanSKU2Qty").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.CutPlanList.length; i++) {
                if ($scope.CutPlanList[0].BalanceQty > 0) {
                    $scope.CutPlanList[i].Status = ChkOrUnchk;
                }
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                if ($scope.filtered[0].BalanceQty > 0) {
                    filtered[j].Status = ChkOrUnchk;
                }
            }
        }
        var gridObj = $("#GridCutPlanSKU2Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    
    $scope.SaveAllotedInfo = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.modelForm4.$valid) {
            try {
                $scope.SaveList = [];
                for (var i = 0; i < $scope.CutPlanList.length; i++) {
                    if ($scope.CutPlanList[i].Status == true && $scope.CutPlanList[i].BalanceQty > 0 && $scope.CutPlanList[i].Ratio > 0) {
                        $scope.SaveList.push($scope.CutPlanList[i]);
                    }
                }
                $http({
                    method: "POST",
                    url: 'Productions/CutPlan/CreateCutPlanData?MasterPlanId='+ $scope.cutplanNew.Id,
                    data: {
                        'data': $scope.allotedheaderNew,
                        'DataList': $scope.SaveList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.LoadCutPlanList();
                        CutPlanClearFields();
                        if ($rootScope.isCollapsed) {
                            $rootScope.toggle();
                        }
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;

            } catch (e) {
                ShowResult(e, "failure");
            }
        }
    };

    $scope.ClearCutPlan = function () {
        CutPlanClearFields();
    };

    function CutPlanClearFields() {
        //$scope.Action = "Save";
        //$scope.allotedheader = {
        //    Id: null
        //    , MasterPlanId: null
        //    , PackingTypeId: null
        //    , UserName: null
        //    , MarkerId: null
        //    , Remarks: null
        //    , NoOfPly: 0
        //};
        //$scope.allotedheaderNew = Object.assign({}, $scope.allotedheader);
        $scope.GetSKU1ColorLists($scope.cutplanNew.Id);
        $scope.allotedheaderNew.Id = null;
        $scope.allotedheaderNew.MasterPlanId = null;
        $scope.allotedheaderNew.PackingTypeId = null;
        $scope.allotedheaderNew.UserName = null;
        $scope.allotedheaderNew.MarkerId = null;
        $scope.allotedheaderNew.Remarks = null;
        $scope.allotedheaderNew.NoOfPly = null;
    }

    function CutPlanCloseFields() {
        $scope.Action = "Save";
        $scope.allotedheader = {
            Id: null
            , MasterPlanId: null
            , PackingTypeId: null
            , UserName: null
            , MarkerId: null
            , Remarks: null
            , NoOfPly: 0
            , SKU1ColorId: null
        };
        $scope.allotedheaderNew = Object.assign({}, $scope.allotedheader);
        $scope.CutPlanList = [];
    }
}