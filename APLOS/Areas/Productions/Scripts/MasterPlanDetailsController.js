'use strict';
MasterPlanDetailsController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function MasterPlanDetailsController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Master Plan Details';
    $scope.Action = 'Save';
    $scope.RoundUpLists = [];
    $scope.path = 'Productions/MasterPlanDetails/';
    $scope.saveUrlMasterPlanQty = $scope.path + 'createMasterPlanQty';

    $scope.RoundUpLists = [
        {
            'Value': '0',
            'Text': '0'
        },
        {
            'Value': '1',
            'Text': '1'
        },
        {
            'Value': '3',
            'Text': '3'
        }
    ];

    $scope.processList = [];
    $scope.GetProcessList = function () {
        $http({
            method: 'GET',
            url: 'Productions/MasterPlanDetails/GetMPDProcessList'
        }).then(function successCallback(response) {
            $scope.processList = response.data;
        });
    }
    $scope.GetProcessList();

    $scope.UserNameList = [];
    $scope.GetUserNameList = function (PId) {
        $http({
            method: 'GET',
            url: 'Productions/MasterPlanDetails/GetUserNameList?ProcessId=' + PId
        }).then(function successCallback(response) {
            $scope.UserNameList = response.data;
            $scope.MasterPlanDetailsNew.UserName = $scope.UserNameList[0].Text;
        });
    }

    $scope.MasterPlanDetails = {
        Id: null
        , ProcessId: null
        , UserName: null
    };
    $scope.MasterPlanDetailsNew = Object.assign({}, $scope.MasterPlanDetails);

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
        , MinQty: null
        , PlanPercentage: null
        , RoundUpApplicable: false
        , RoundUp: null

    };
    $scope.cutplanNew = Object.assign({}, $scope.cutplan);

    $scope.MasterPlanList = [];
    $scope.LoadMasterPlanList = function () {
        $http({
            method: 'Get',
            url: 'Productions/MasterPlanDetails/GetMasterPlanList?ProcessId=' + $scope.MasterPlanDetailsNew.ProcessId
        }).then(function successCallback(response) {
            $scope.MasterPlanList = response.data;
            var gridObj = $("#GridMasterPlan").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
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

    $scope.GetMasterPlanDetails = function (args) {
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
        getMasterPlanDetailsList();
        $scope.LoadMPDLineItemList();
        $scope.LoadMPDSKU1List();
        $scope.LoadMPDSKU2List();
        angular.element(document.querySelector('#MasterPlanDetailsPopUp')).modal('show');
    }

    $scope.closeMasterPlanDetailsPopUp = function () {
        angular.element(document.querySelector('#MasterPlanDetailsPopUp')).modal('hide');
    }

    $scope.MasterPlanDetailsListSelected = [];
    function getMasterPlanDetailsList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMasterPlanDetailsList?ProcessId=' + $scope.MasterPlanDetailsNew.ProcessId + '&MasterPlanId=' + $scope.MasterPlanId,
        }).then(function successCallback(response) {
            $scope.MasterPlanDetailsListSelected = response.data;
            var gridObj = $("#GridSOItemSelected").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
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

        var filtered = $("#GridSOItemSelected").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MasterPlanDetailsListSelected.length; i++) {
                $scope.MasterPlanDetailsListSelected[i].Status = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Status = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSOItemSelected").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
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
            var gridObj = $("#GridSOItemSelected").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.MPQPlanPer = 0;
    $scope.MPQMinQty = 0;
    $scope.MPQQty = 0;
    $scope.MPQAdjQty = 0;
    $scope.MasterPlanQtyCal = function (data) {
        try {
            $scope.MPQPlanPer = 0;
            $scope.MPQMinQty = 0;
            $scope.MPQQty = 0;
            $scope.MPQAdjQty = 0;
            $scope.MPQPlanPer = data.data.PlanPercentage;
            $scope.MPQMinQty = data.data.MinQty;
            $scope.MPQQty = data.data.Qty;
            $scope.MPQAdjQty = data.data.AdjustmentQty;
            if (($scope.MPQPlanPer * $scope.MPQQty / 100) < $scope.MPQMinQty) {
                if ($scope.cutplanNew.RoundUp === "0") {
                    data.data.MasterPlanQty = Math.round($scope.MPQQty + $scope.MPQMinQty);
                    data.data.FinalQty = Math.round(data.data.MasterPlanQty - $scope.MPQAdjQty);
                }
                else if ($scope.cutplanNew.RoundUp === "1") {
                    data.data.MasterPlanQty = Math.round(($scope.MPQQty + $scope.MPQMinQty) * 10) / 10;
                    data.data.FinalQty = Math.round((data.data.MasterPlanQty - $scope.MPQAdjQty) * 10) / 10;
                }
                else if ($scope.cutplanNew.RoundUp === "3") {
                    data.data.MasterPlanQty = Math.round(($scope.MPQQty + $scope.MPQMinQty) * 1000) / 1000;
                    data.data.FinalQty = Math.round((data.data.MasterPlanQty - $scope.MPQAdjQty) * 1000) / 1000;
                }
                else
                {
                    data.data.MasterPlanQty = Math.round($scope.MPQQty + $scope.MPQMinQty);
                    data.data.FinalQty = Math.round(data.data.MasterPlanQty - $scope.MPQAdjQty);
                }
                
            }
            else {
                if ($scope.cutplanNew.RoundUp === "0") {
                    data.data.MasterPlanQty = Math.round($scope.MPQQty + ($scope.MPQPlanPer * $scope.MPQQty / 100));
                    data.data.FinalQty = Math.round(data.data.MasterPlanQty - $scope.MPQAdjQty);
                }
                else if ($scope.cutplanNew.RoundUp === "1") {
                    data.data.MasterPlanQty = Math.round(($scope.MPQQty + ($scope.MPQPlanPer * $scope.MPQQty / 100)) * 10) / 10;
                    data.data.FinalQty = Math.round((data.data.MasterPlanQty - $scope.MPQAdjQty) * 10) / 10;
                }
                else if ($scope.cutplanNew.RoundUp === "3") {
                    data.data.MasterPlanQty = Math.round(($scope.MPQQty + ($scope.MPQPlanPer * $scope.MPQQty / 100)) * 1000) / 1000;
                    data.data.FinalQty = Math.round((data.data.MasterPlanQty - $scope.MPQAdjQty) * 1000) / 1000;
                }
                else
                {
                    data.data.MasterPlanQty = Math.round($scope.MPQQty + ($scope.MPQPlanPer * $scope.MPQQty / 100));
                    data.data.FinalQty = Math.round(data.data.MasterPlanQty - $scope.MPQAdjQty);
                }
            }
            var gridObj = $("#GridMasterPlanLineItemQty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridMasterPlanSKU1Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridMasterPlanSKU2Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };


    $scope.MPQty = 0;
    $scope.MPAdjQty = 0;
    $scope.FinalQtyCal = function (data) {
        try {
            $scope.MPQty = 0;
            $scope.MPAdjQty = 0;
            $scope.MPQty = data.data.MasterPlanQty;
            $scope.MPAdjQty = data.data.AdjustmentQty;
            if ($scope.cutplanNew.RoundUp === "0") {
                data.data.FinalQty = Math.round($scope.MPQty - ($scope.MPAdjQty));
            }
            else if ($scope.cutplanNew.RoundUp === "1") {
                data.data.FinalQty = Math.round((($scope.MPQty - ($scope.MPAdjQty)) * 10 ) / 10);
            }
            else if ($scope.cutplanNew.RoundUp === "3") {
                data.data.FinalQty = Math.round((($scope.MPQty - ($scope.MPAdjQty)) * 1000) / 1000);
            }
            else
            { 
                data.data.FinalQty = Math.round($scope.MPQty - ($scope.MPAdjQty));
            }
            var gridObj = $("#GridMasterPlanLineItemQty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridMasterPlanSKU1Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridMasterPlanSKU2Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
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
            url: 'Productions/MasterPlanDetails/GetMPDLineItemList?MasterPlanId=' + $scope.cutplanNew.Id
        }).then(function successCallback(response) {
            $scope.MPDLineItemList = response.data;
            $scope.cutplanNew.LineItemTotalQty = $scope.MPDLineItemList[0].LineItemTotalQty;
            $scope.cutplanNew.SKU1TotalQty = $scope.MPDLineItemList[0].SKU1TotalQty;
            $scope.cutplanNew.SKU2TotalQty = $scope.MPDLineItemList[0].SKU2TotalQty;
            var gridObj = $("#GridMPDLineItem").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.MPDSKU1List = [];
    $scope.LoadMPDSKU1List = function () {
        $http({
            method: 'Get',
            url: 'Productions/MasterPlanDetails/GetMPDSKU1List?MasterPlanId=' + $scope.cutplanNew.Id
        }).then(function successCallback(response) {
            $scope.MPDSKU1List = response.data;
            var gridObj = $("#GridMPDSKU1").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.MPDSKU2List = [];
    $scope.LoadMPDSKU2List = function () {
        $http({
            method: 'Get',
            url: 'Productions/MasterPlanDetails/GetMPDSKU2List?MasterPlanId=' + $scope.cutplanNew.Id
        }).then(function successCallback(response) {
            $scope.MPDSKU2List = response.data;
            var gridObj = $("#GridMPDSKU1").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.MasterPlanQtyList = [];
    $scope.LoadMasterPlanQtyList = function () {
        $http({
            method: 'Get',
            url: 'Productions/MasterPlanDetails/GetMasterPlanQtyList?MasterPlanId=' + $scope.cutplanNew.Id + '&MinQty=' + $scope.cutplanNew.MinQty + '&PlanPercentage=' + $scope.cutplanNew.PlanPercentage + '&LineItem=' + $scope.LineItem + '&SKU1=' + $scope.SKU1 + '&SKU2=' + $scope.SKU2
        }).then(function successCallback(response) {
            $scope.MasterPlanQtyList = response.data;
            var gridObj = $("#GridMasterPlanLineItemQty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridMasterPlanSKU1Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            var gridObj = $("#GridMasterPlanSKU2Qty").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.ViewMasterPlanQty = function () {
        try {
            if ($scope.LineItem == true || $scope.SKU1 === false || $scope.SKU2 === false) {
                $scope.LoadMasterPlanQtyList();
            }
            if ($scope.LineItem === true || $scope.SKU1 === true || $scope.SKU2 === false) {
                if ($scope.LineItemTotalQty !== $scope.SKU1TotalQty) {
                    throw "LineItem and SKU1 Total is not matching please correct it and proceed...";
                }
                else {
                    $scope.LoadMasterPlanQtyList();
                }
            }
            if ($scope.LineItem === true || $scope.SKU1 === true || $scope.SKU2 === true) {
                if ($scope.LineItemTotalQty !== $scope.SKU1TotalQty && $scope.LineItemTotalQty !== $scope.SKU2TotalQty && $scope.SKU1TotalQty !== $scope.SKU2TotalQty) {
                    throw "LineItem,SKU1 and SKU2 Totals are not matching please correct it and proceed...";
                }
                else {
                    $scope.LoadMasterPlanQtyList();
                }
            }
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    }

    $scope.SaveMasterPlanQty = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.MasterPlanQtyList.length; i++) {
                $scope.SaveList.push($scope.MasterPlanQtyList[i]);
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlMasterPlanQty,
                data: {
                    "DataList": $scope.SaveList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadMasterPlanList();
                    angular.element(document.querySelector('#MasterPlanDetailsPopUp')).modal('hide');
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
}