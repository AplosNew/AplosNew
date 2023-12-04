'use strict';
CutPlanController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function CutPlanController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Cut Plan';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/CutPlan/';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.saveUrlCutPlan = $scope.path + 'createCutPlan';

    $scope.CalculateOn = 'Round';
    $scope.MarkerId = null;
    $scope.CharacteristicsName = null;
    $scope.CharacteristicsId = null;

    $scope.FGCharacteristicsValueList = [];
    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.modelNew.ProductionEntityId = $scope.entityList[0].Value;
                //default                
            }
        });
    }
    $scope.getAllEntities();

    $scope.PlanStatusList = [];
    $scope.getAllPlanStatus = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetPlanStatus"
        }).then(function successCallback(response) {
            $scope.PlanStatusList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.cutplanNew.PlanStatus = $scope.PlanStatusList[0].Value;
            }
        });
    }
    $scope.getAllPlanStatus();

    $scope.modelNew = {
        Id: null,
        ProductionEntityId: null,
        ProductionOrderId: null,
        FromDate: null,
        ToDate: null,
    }

    $scope.cutplan = {
        Id: null
        , PlanName: null
        , UserId: $window.employeeId
        , User: $window.employeeName
        , PlanStatus: null
        , ResponsiblePersonId: null
        , ResponsiblePerson: null
        , Remarks: null
    };
    $scope.cutplanNew = Object.assign({}, $scope.cutplan);

    $scope.Employee = null;
    $scope.ResponsiblePersonList = [];
    $scope.selectResponsiblePerson = function (flag) {
        $scope.Employee = flag;
        $http({
            method: 'POST',
            url: $scope.path + 'GetUserName',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ResponsiblePersonList = resp.data;
        });
        angular.element(document.querySelector('#ResponsiblePersonPopUp')).modal('show');
    }

    $scope.doubleResponsiblePerson = function (e) {
        if ($scope.Employee === 'User') {
            $scope.cutplanNew.UserId = e.data.SystemId;
            $scope.cutplanNew.User = e.data.EmployeeName;
        }
        else {
            $scope.cutplanNew.ResponsiblePersonId = e.data.SystemId;
            $scope.cutplanNew.ResponsiblePerson = e.data.EmployeeName;
        }
        angular.element(document.querySelector('#ResponsiblePersonPopUp')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopUp')).modal('hide');
    }

    $scope.CutPlanList = [];
    $scope.LoadCutPlanList = function () {
        $http({
            method: 'Get',
            url: 'Productions/CutPlan/LoadCutPlanList'
        }).then(function successCallback(response) {
            $scope.CutPlanList = response.data;
            var gridObj = $("#GridCutPlan").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.LoadCutPlanList();

    $scope.GetCutPlanDetails = function (args) {
        $scope.CutPlanId = args.data.Id;
        $http({
            method: 'Get',
            url: 'Productions/CutPlan/LoadCutPlanEditData?CutPlanId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.cutplanNew = response.data.cutplan[0];
            $scope.cutplanNew.ResponsiblePerson = response.data.cutplan[0].ResponsiblePerson;
            $scope.cutplanNew.User = response.data.cutplan[0].UserName;
            getCutPlanDetailsList();
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.View = function () {
        getCutPlanDetailsList();
    }

    $scope.CutPlanListSelected = [];
    function getCutPlanDetailsList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetCutPlanDetailsList?FromDate=' + $scope.modelNew.FromDate + '&ToDate=' + $scope.modelNew.ToDate + '&ProductionEntityId=' + $scope.modelNew.ProductionEntityId + '&PlanId=' + $scope.cutplanNew.Id,
        }).then(function successCallback(response) {
            $scope.CutPlanListSelected = response.data;
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
            for (var i = 0; i < $scope.CutPlanListSelected.length; i++) {
                $scope.CutPlanListSelected[i].Status = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Status = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSOItemSelected").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };
    $scope.PlanPer = 0;
    $scope.SOQty = 0;
    $scope.SOPlanQtyCal = function (data) {
        try {
            $scope.PlanPer = 0;
            $scope.SOQty = 0;
            $scope.SOPlanQty = 0;
            $scope.PlanPer = data.data.PlanPercentage;
            $scope.SOQty = data.data.Qty;
            data.data.SOPlanQty = $scope.SOQty + ($scope.PlanPer * $scope.SOQty / 100);
            var gridObj = $("#GridSOItemSelected").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveCutPlan = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.CutPlanListSelected.length; i++) {
                if ($scope.CutPlanListSelected[i].Status == true || ($scope.CutPlanListSelected[i].Status == false && $scope.CutPlanListSelected[i].Id != null)) {
                    $scope.SaveList.push($scope.CutPlanListSelected[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlCutPlan,
                data: {
                    'CutPlanData': $scope.cutplanNew,
                    'DataList': $scope.SaveList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadCutPlanList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.ClearCutPlan = function () {
        CutPlanClearFields();
    };

    function CutPlanClearFields() {
        $scope.Action = "Save";
        $scope.cutplanNew = Object.assign({}, $scope.cutplan);
        $scope.CutPlanListSelected = [];
    }


    $scope.ProductionOrderList = [];
    $scope.ProdOrderList = [];
    $scope.getProductionOrderPopUp = function () {
        if ($scope.modelNew.ProductionEntityId == null) {
            throw "Select Production Entity.."
        }
        $scope.ProductionOrderList = [];
        $http.get("Productions/CutPlan/GetProductionOrderDataList?entityId=" + $scope.modelNew.ProductionEntityId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ProductionOrderList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');
    };
    $scope.SelectPOItem = function ($event) {
        $scope.modelNew.ProductionOrderId = $event.data.POId;
        //$scope.GetLineItemData();
        getProductionRecipeMaterialList();
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
    }
    $scope.SalesOrderLineItems = [];
    $scope.recipeMaterialListSelected = [];
    $scope.GetLineItemData = function () {
        $http({
            method: 'GET',
            url: 'Productions/CutPlan/GetLineItemData?entityId=' + $scope.modelNew.ProductionEntityId + '&processId=' + $scope.modelNew.ProcessId + '&productionOrderId=' + $scope.modelNew.ProductionOrderId + '&masterId=' + $scope.modelNew.Id
        }).then(function successCallback(response) {
            $scope.SalesOrderLineItems = response.data;
        });
    }
    function getProductionRecipeMaterialList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionRecipeMaterialList?productionOrderId=' + $scope.modelNew.ProductionOrderId
        }).then(function successCallback(response) {
            $scope.recipeMaterialListSelected = response.data;
            GetMarker(response.data[0].MaterialMasterId);

        });
    }


    //#region MarkerList
    $scope.MarkerList = [];
    function GetMarker(MaterialId) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMarker?MaterialId=' + MaterialId
        }).then(function successCallback(response) {
            $scope.MarkerList = response.data;
            //getProductionProcessSetList();
        });
    }
    $scope.CalculationOption = false;
    $scope.getSKU = function () {
        for (var i = 0; i < $scope.MarkerList.length; i++) {
            if ($scope.MarkerList[i].Value == $scope.MarkerId) {
                $scope.CharacteristicsName = $scope.MarkerList[i].SKU;
                $scope.CharacteristicsId = $scope.MarkerList[i].SKUId;
            }
        }
        $scope.getFGCharacteristicsLists($scope.recipeMaterialListSelected[0].MaterialMasterId);
    };
    $scope.totalRatio = 0;
    $scope.getFGCharacteristics = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMarkerDetails?MarkerId=' + $scope.MarkerId
        }).then(function successCallback(response) {
            $scope.FGCharacteristicsValueList = response.data;
            $scope.CalculationOption = true;
            $scope.Clicked = false;
            $scope.SOIDs = "";
            var CharacteristicsValueId = "";
            $scope.totalRatio = 0;
            for (var i = 0; i < $scope.FGCharacteristicsValueList.length; i++) {
                $scope.totalRatio = parseFloat($scope.FGCharacteristicsValueList[i].Ratio) + parseFloat($scope.totalRatio);

                if (CharacteristicsValueId == "") {
                    CharacteristicsValueId = "'" + $scope.FGCharacteristicsValueList[i].CharacteristicsValueId + "'";
                }
                else {
                    CharacteristicsValueId += ",'" + $scope.FGCharacteristicsValueList[i].CharacteristicsValueId + "'";
                }

            }
            for (var i = 0; i < $scope.recipeMaterialListSelected.length; i++) {
                if ($scope.SOIDs === "") {
                    $scope.SOIDs += "'" + $scope.recipeMaterialListSelected[i].SalesOrderId + "'";
                }
                else {
                    $scope.SOIDs += ", '" + $scope.recipeMaterialListSelected[i].SalesOrderId + "'";
                }
            }
            $scope.getOtherFGCharacteristics($scope.characteristicsList[0].Value, $scope.characteristicsList[0].Sequence, $scope.SOIDs, CharacteristicsValueId);
        });
    };
    $scope.IsSelect = false;
    $scope.SOIDs = "";
    $scope.getFGCharacteristicsLists = function (id) {
        //$scope.clearCharNames();
        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.characteristicsList = [];

            $scope.characteristicsList = response.data.charData;
            for (var i = 0; i < $scope.characteristicsList.length; i++) {
                if ($scope.characteristicsList[i].Value === $scope.CharacteristicsId) {
                    $scope.characteristicsList.splice(i, 1);
                }
            }

        });
    };
    $scope.SkuValueList = [];
    $scope.getOtherFGCharacteristics = function (skuId, Sequence, SOIDs, CharacteristicsValueId) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetSkuDetails?OtherSku=' + skuId + '&SOId=' + SOIDs + '&Sequence=' + Sequence + '&CharacteristicsValueId=' + CharacteristicsValueId
        }).then(function successCallback(response) {
            $scope.SkuValueList = [];
            $scope.SkuValueList = response.data;
        });
    };
    $scope.MinimumPlyValue = null;
    $scope.MinimumPlyValueName = null;
    $scope.CalculationArryWithData = [];
    $scope.CalculatedSkuValueList = [];
    $scope.Clicked = false;
    $scope.ErrorThrow = true;

    $scope.CalculatePly = function () {
        try {
            $scope.CalculatedSkuValueList = [];
            for (var i = 0; i < $scope.SkuValueList.length; i++) {
                if ($scope.SkuValueList[i].IsSelect) {
                    var CalculationArry = [];
                    $scope.ErrorThrow = false;
                    $scope.CalculatedSkuValueList.push($scope.SkuValueList[i]);
                    for (var j = 0; j < $scope.SkuValueList[i].Qty.length; j++) {
                        CalculationArry.push(parseFloat($scope.SkuValueList[i].Qty[j].Qty) / parseFloat($scope.SkuValueList[i].Qty[j].Ratio));
                    }

                    $scope.MinimumPlyValue = Math.min.apply(null, CalculationArry);
                    var MiniValue = parseFloat($scope.MinimumPlyValue).toFixed(2);
                    var OptionBasedMinValue = '';
                    if ($scope.CalculateOn == 'Round') {
                        OptionBasedMinValue = parseFloat(Math.round($scope.MinimumPlyValue)).toFixed(2);
                    }
                    else if ($scope.CalculateOn == 'RoundUp') {
                        OptionBasedMinValue = parseFloat(Math.ceil($scope.MinimumPlyValue)).toFixed(2);
                    }
                    else {
                        OptionBasedMinValue = parseFloat(Math.floor($scope.MinimumPlyValue)).toFixed(2);
                    }



                    $scope.Clicked = true;
                    $scope.SkuValueList[i].MinimumPlyActualValue = MiniValue;
                    $scope.SkuValueList[i].MinimumPlyOptionValue = OptionBasedMinValue;
                }
            }

            for (var i = 0; i < $scope.CalculatedSkuValueList.length; i++) {
                var c = 0;
                for (var j = 0; j < $scope.CalculatedSkuValueList[i].Qty.length; j++) {
                    c = parseFloat($scope.CalculatedSkuValueList[i].MinimumPlyOptionValue) * parseFloat($scope.CalculatedSkuValueList[i].Qty[j].Ratio);
                    $scope.CalculatedSkuValueList[i].Qty[j].CalculatedPlyQty = c.toFixed(2);
                }
            }
            if ($scope.ErrorThrow) {
                throw "Select Value For Calculation.. ";
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.CutPlanMarkerDetails = {
        Id: null,
        CutPlanMasterId: null,
        MarkerId: null,
        MarkerCharacteristicsId: null,
        RoundingType: null,
    }
    $scope.Iteration = 1;
    $scope.Save = function () {

        //#region CutPlanMarkerDetails Model 
        $scope.CutPlanMarkerDetails.CutPlanMasterId = $scope.modelNew.Id;
        $scope.CutPlanMarkerDetails.MarkerId = $scope.MarkerId;
        $scope.CutPlanMarkerDetails.MarkerCharacteristicsId = $scope.CharacteristicsId;
        $scope.CutPlanMarkerDetails.RoundingType = $scope.CalculateOn;
        //#endregion

        try {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'CalculatedValueList': $scope.CalculatedSkuValueList, 'FGCharacteristicsValueList': $scope.FGCharacteristicsValueList,
                    'MasterData': $scope.modelNew, 'CPMarkerDetails': $scope.CutPlanMarkerDetails, 'SkuValueList': $scope.SkuValueList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Iteration = 2;
                    $scope.GetSecIteration(response.data.data.Id, $scope.Iteration);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SecenodIteration = [];
    $scope.SecenodIterationHeader = [];
    $scope.SecenodIterationColumn = [];
    $scope.GetSecIteration = function (MasterId, iteration) {
        try {
            if (iteration == 2) {
                for (var i = 0; i < $scope.CalculatedSkuValueList.length; i++) {
                    for (var j = 0; j < $scope.CalculatedSkuValueList[i].Qty.length; j++) {
                        $scope.CalculatedSkuValueList[i].Qty[j].AvailableQty = $scope.CalculatedSkuValueList[i].Qty[j].Qty - parseFloat($scope.CalculatedSkuValueList[i].Qty[j].CalculatedPlyQty);
                    }
                }
            }
            else {
                throw "Save data first..";
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    //$scope.CalculatePly = function () {
    //    try {
    //        $scope.CalculatedSkuValueList = [];
    //        for (var i = 0; i < $scope.SkuValueList.length; i++) {
    //            if ($scope.SkuValueList[i].IsSelect) {
    //                var CalculationArry = [];
    //                $scope.ErrorThrow = false;
    //                $scope.CalculatedSkuValueList.push($scope.SkuValueList[i]);
    //                for (var j = 0; j < $scope.SkuValueList[i].Qty.length; j++) {
    //                    CalculationArry.push(parseFloat($scope.SkuValueList[i].Qty[j].Qty) / parseFloat($scope.SkuValueList[i].Qty[j].Ratio));
    //                }

    //                $scope.MinimumPlyValue = Math.min.apply(null, CalculationArry);
    //                var MiniValue = parseFloat($scope.MinimumPlyValue).toFixed(2);
    //                var OptionBasedMinValue = '';
    //                if ($scope.CalculateOn == 'Round') {
    //                    OptionBasedMinValue = parseFloat(Math.round($scope.MinimumPlyValue)).toFixed(2);
    //                }
    //                else if ($scope.CalculateOn == 'RoundUp') {
    //                    OptionBasedMinValue = parseFloat(Math.ceil($scope.MinimumPlyValue)).toFixed(2);
    //                }
    //                else {
    //                    OptionBasedMinValue = parseFloat(Math.floor($scope.MinimumPlyValue)).toFixed(2);
    //                }



    //                $scope.Clicked = true;
    //                $scope.SkuValueList[i].MinimumPlyActualValue = MiniValue;
    //                $scope.SkuValueList[i].MinimumPlyOptionValue = OptionBasedMinValue;
    //            }
    //        }

    //        for (var i = 0; i < $scope.CalculatedSkuValueList.length; i++) {
    //            var c = 0;
    //            for (var j = 0; j < $scope.CalculatedSkuValueList[i].Qty.length; j++) {
    //                c = parseFloat($scope.CalculatedSkuValueList[i].MinimumPlyOptionValue) * parseFloat($scope.CalculatedSkuValueList[i].Qty[j].Ratio);
    //                $scope.CalculatedSkuValueList[i].Qty[j].CalculatedPlyQty = c.toFixed(2);
    //            }
    //        }
    //        if ($scope.ErrorThrow) {
    //            throw "Select Value For Calculation.. ";
    //        }
    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //};

}