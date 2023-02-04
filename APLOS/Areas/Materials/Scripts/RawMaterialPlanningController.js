'use strict';
RawMaterialPlanningController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", "$controller"];
function RawMaterialPlanningController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {
    $rootScope.title = "Raw Material Planning";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.PlanStatusList = [];
    $scope.path = 'Materials/RawMaterialPlanning/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'CreateRMPlan';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.PlanStatusList = [
        {
            'Value': 'Inprogress',
            'Text': 'Inprogress'
        },
        {
            'Value': 'Close',
            'Text': 'Close'
        }
        ,
        {
            'Value': 'OnHold',
            'Text': 'OnHold'
        }
    ];

    $scope.MaterialPlanTypeList = [
        {
            'Value': 'Costing',
            'Text': 'Costing'
        },
        {
            'Value': 'QBOQ',
            'Text': 'QBOQ'
        }
    ];

    $scope.ConsumptionLevelList = [];
    $scope.GetConsumptionLevelList = function () {
        $http({
            method: 'GET',
            url: 'Materials/RawMaterialPlanning/GetConsumptionLevelList' 
        }).then(function successCallback(response) {
            $scope.ConsumptionLevelList = response.data;
        });
    }
    $scope.GetConsumptionLevelList();

    $scope.modelFilterByList = [
        { 'name': 'Prod. Order#', 'value': 'Id' },
        { 'name': 'Prod. Status', 'value': 'ProductionStatus' },
        { 'name': 'Material', 'value': 'Material' },
        { 'name': 'Product', 'value': 'Product' },
        { 'name': 'Product Category', 'value': 'ProductCategory' },
        { 'name': 'Master Order No', 'value': 'MasterOrderId' },
        { 'name': 'Buyer Order#', 'value': 'BuyerRefNo' },
        { 'name': 'Own Order#', 'value': 'OwnRefNo' },
        { 'name': 'Buyer Item#', 'value': 'StyleNo' },
        { 'name': 'Own Item#', 'value': 'OwnStyleNo' },
        { 'name': 'SO No', 'value': 'SONo' },
        { 'name': 'SO Desc', 'value': 'SODesc' },
        { 'name': 'Buyer', 'value': 'buyer' },
        { 'name': 'Customer', 'value': 'Customer' },
    ];

    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = ['BOM'];
    $scope.ModelNew = { Id: null, POId: null, EntityId: null, MOItemId: null, MaterialStorageId: null, IssueDate: null, IssueType: 'Revenue', UserCode: null, UserRef: null, PlanPercentage: null, ByWhomId: null, UserName: null, Level: "QBOQ", LotNo: null, IsApproved: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };
    $scope.RMPlanningNew = { Id: null, POId: null, PlanBy: null, PlanById: null, PlanDate: null, UserPlanName: null, Remarks: null, IsActive: true, PlanStatus: null, MaterialPlanType: "QBOQ"};

    //$scope.entityList = [];
    //$scope.getAllEntities = function () {
    //    $http({
    //        method: 'Get',
    //        url: "Materials/RawMaterialPlanning/EntityList"
    //    }).then(function successCallback(response) {
    //        $scope.entityList = response.data;
    //    });
    //}
    //$scope.getAllEntities();

    //$scope.GetEntityName = function () {
    //    for (var i = 0; i < $scope.entityList.length; i++) {
    //        if ($scope.entityList[i].Value == $scope.ModelNew.EntityId) {
    //            $scope.ModelNew.Entity = $scope.entityList[i].Text;
    //            break;
    //        }
    //    }

    //}

    //$scope.AllTabPrint = function (data) {
    //    location.href = "Materials/RawMaterialPlanning/IssueRequestReport?mId=" + data.data.Id;
    //};

    //$scope.LevelList = [
    //    {
    //        'Value': 'Costing',
    //        'Text': 'Costing'
    //    },
    //    {
    //        'Value': 'QBOQ',
    //        'Text': 'QBOQ'
    //    }
    //];

    //$scope.storageList = [];

    //$scope.Getstorage = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'Materials/MaterialStorage/getcbo'
    //    }).then(function (response) {
    //        $scope.storageList = response.data;
    //    });
    //}
    //$scope.Getstorage();

    //$scope.NotificationSettingStatus = function () {
    //    //debugger;
    //    $http({
    //        method: 'GET',
    //        url: 'Products/GoodsReceiveNote/NotificationSetting',
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.NotificationSetting = response.data;
    //        $scope.CheckedByStatusForNoti = $scope.NotificationSetting[0].RequiredChecking;
    //        $scope.ApprovedByStatusForNoti = $scope.NotificationSetting[0].RequiredApproval;
    //        if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === false) {
    //            $scope.labelCheckAndApproved = 'To be checked by';
    //        }
    //        else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true) {
    //            $scope.labelCheckAndApproved = 'To be approved by';
    //        }
    //        else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true) {
    //            $scope.labelCheckAndApproved = 'To be checked by';
    //        }
    //        //else {
    //        //    $scope.productNew.labelCheckAndApproved = 'To be checked/approved by';
    //        //}

    //    });
    //}
    //$scope.NotificationSettingStatus();

    //$scope.checkedByList = [];
    //$scope.GetSupervisorCboList = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'Products/PurchaseOrder/GetIssueSlipCheckByCbo'
    //    }).then(function successCallback(response) {
    //        $scope.checkedByList = response.data;
    //    });
    //}
    //$scope.GetSupervisorCboList();

    //$scope.CostCenterLoad = function () {
    //    cboService.getCostCenterCbo(function (result) {
    //        $scope.costCenterList = result;
    //        for (var i = 0; i < $scope.costCenterList.length; i++) {
    //            if ($scope.costCenterList[i].Text == 'Mixing') {
    //                $scope.ModelNew.CostCenterId = $scope.costCenterList[i].Value;
    //                break;
    //            }
    //        }
    //    });
    //}
    //$scope.CostCenterLoad();

    //$scope.savedList = [];
    //$scope.GetSavedData = function () {
    //    $scope.savedList = [];
    //    $http.get('Materials/RawMaterialPlanning/GetApprovedData')
    //        .then(
    //            function successCallback(response) {
    //                if (baseService.arrayLength(response.data) > 0) {
    //                    $scope.savedList = response.data;
    //                }
    //            },
    //            function errorCallback(response) {
    //                ShowResult(response, 'failure');
    //            });

    //};
    //$scope.GetSavedData();

    $scope.SOItemList = [];
    $scope.Get = function (obj) {
        $scope.RMPlanningNew.POId = obj.data.Id;
        $scope.LoadPlanDetails($scope.RMPlanningNew.POId);
        $scope.SOItemList = [];
        $http.get('Materials/RawMaterialPlanning/GetSOItemList?ProductionOrderId=' + obj.data.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SOItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    //$scope.GetSaved = function (obj) {
    //    $scope.ModelNew = Object.assign({}, obj.data);
    //    $scope.ModelNew.IssueDate = $filter('dateFiltering')($scope.ModelNew.IssueDate, 'dd-M-yyyy');
    //    $scope.Action = 'Update';
    //    $scope.GetSavedSODetailData();
    //    $scope.GetSavedDetailData();
    //    // $scope.IssueSlipGriddata('ForChecked', 'InventorySlip', $scope.ModelNew.POId);
    //    /*$scope.getdataInventoryIssue($scope.ModelNew.POId);*/
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};

    //$scope.SOItemList = [];
    //$scope.GetSavedSODetailData = function () {
    //    $scope.SOItemList = [];
    //    $http.get('Materials/RawMaterialPlanning/GetSavedSODetailData?masterId=' + $scope.ModelNew.Id)
    //        .then(
    //            function successCallback(response) {
    //                if (baseService.arrayLength(response.data) > 0) {
    //                    $scope.SOItemList = response.data;
    //                }
    //            },
    //            function errorCallback(response) {
    //                ShowResult(response, 'failure');
    //            });

    //};

    //$scope.QBOQCostingList = [];
    //$scope.GetSavedDetailData = function () {
    //    $scope.QBOQCostingList = [];
    //    $http.get('Materials/RawMaterialPlanning/GetSavedDetailData?masterId=' + $scope.RMPlanningNew.Id)
    //        .then(
    //            function successCallback(response) {
    //                if (baseService.arrayLength(response.data) > 0) {
    //                    $scope.QBOQCostingList = response.data;
    //                }
    //            },
    //            function errorCallback(response) {
    //                ShowResult(response, 'failure');
    //            });

    //};

    //$scope.getArticle = function (data) {
    //    $scope.SelectedMaterial = data;
    //    $scope.getArticleSearchList(data.MaterialMasterId);
    //};
    //$scope.selectarticle = function (ob) {
    //    try {

    //        $scope.SelectedMaterial.MaterialMasterId = ob.MaterialMasterId;
    //        $scope.SelectedMaterial.Material = ob.MaterialMasterName;
    //        $scope.SelectedMaterial.ArticleId = ob.Id;
    //        $scope.SelectedMaterial.Article = ob.StandardName;

    //        angular.element(document.querySelector('#articleSearchPop')).modal('hide');
    //        if ($scope.RMPlanningNew.MaterialPlanType == "Costing") {
    //            var gridObj = $("#CGrid").data("ejGrid");
    //            gridObj.refreshContent(true);
    //            gridObj.refreshTemplate();
    //        } else {
    //            var gridObj = $("#BGrid").data("ejGrid");
    //            gridObj.refreshContent(true);
    //            gridObj.refreshTemplate();
    //        }
    //    } catch (e) {
    //        ShowResult(e, '', 'articleSearchPop');
    //    }
    //};

    //$scope.popUpDataList = [];
    //$scope.showEmployeeListPopUp = function () {
    //    try {
    //        $scope.popUpDataList = [];
    //        $http({
    //            method: 'GET',
    //            url: 'OrderManagements/SalesOrderApproval/GetAllActiveEmployeeData'

    //        }).then(function successCallback(response) {
    //            $scope.popUpDataList = response.data;
    //        });

    //        angular.element(document.querySelector('#popUp')).modal('show');

    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //};

    //$scope.SelectEmployee = function (arg) {
    //    $scope.ModelNew.ByWhomId = arg.data.SystemId;
    //    $scope.ModelNew.ByWhom = arg.data.EmployeeName;
    //    $scope.closePopUp();
    //}

    //$scope.closePopUp = function () {
    //    angular.element(document.querySelector('#popUp')).modal('hide');
    //}

    $scope.refreshTemplateSO = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllSO });
    };
    function CheckBoxSelectAllSO(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#SOGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SOItemList.length; i++) {
                $scope.SOItemList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#SOGrid").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.refreshTemplateQBOQ = function (args) {
        $("#Qheadchk").ejCheckBox({ "change": CheckBoxSelectAllQBOQ });
    };
    function CheckBoxSelectAllQBOQ(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#BGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
                $scope.QBOQCostingList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#BGrid").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.refreshTemplateCosting = function (args) {
        $("#Cheadchk").ejCheckBox({ "change": CheckBoxSelectAllCosting });
    };
    function CheckBoxSelectAllCosting(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#CGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
                $scope.QBOQCostingList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#CGrid").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.Action = 'Save';
    $scope.Save = function () {
        $scope.QBOQCostingListNew = [];
        $scope.SaveList = [];
        for (var p = 0; p < $scope.QBOQCostingList.length; p++) {
            if ($scope.QBOQCostingList[p].Flag == true) {
                $scope.QBOQCostingList[p].TransactionUoMId = $scope.QBOQCostingList[p].UoMId;
                $scope.QBOQCostingList[p].BaseUoMId = $scope.QBOQCostingList[p].UoMId;
                $scope.QBOQCostingList[p].CostCenterId = $scope.ModelNew.CostCenterId;
                $scope.QBOQCostingList[p].RequestedQty = $scope.QBOQCostingList[p].PlanConsumption;
                $scope.QBOQCostingListNew.push($scope.QBOQCostingList[p]);
            }
        }
        try {
            if (baseService.isUndefinedOrNull($scope.RMPlanningNew.POId)) {
                throw "Select Production Order.";
            }
            if (baseService.arrayLength($scope.SOItemList) === 0) {
                throw "Select SO Detail.";
            }
            for (var i = 0; i < $scope.SOItemList.length; i++) {
                if ($scope.SOItemList[i].Flag == true) {
                    $scope.SaveList.push($scope.SOItemList[i]);
                }
            }
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.RMPlanningNewForm.$valid) {
                if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'model': $scope.RMPlanningNew
                            , 'soList': $scope.SaveList
                            , 'dataList': $scope.QBOQCostingListNew
                            , 'dataLists': $scope.QBOQCostingListNew
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Clear();
                            $scope.GetSavedData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    
    $scope.Clear = function () {
        //$scope.ModelNew = { Id: null, POId: null, EntityId: null, MaterialStorageId: null, IssueDate: null, IssueType: 'Revenue', UserCode: null, UserRef: null, PlanPercentage: null, ByWhomId: null, UserName: null, Level: "Costing", LotNo: null, IsApproved: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };
        $scope.RMPlanningNew = { Id: null, POId: null, PlanBy: null, PlanById: null, PlanDate: null, UserPlanName: null, Remarks: null, IsActive: true, PlanStatus: null, MaterialPlanType: "Costing"};
        $scope.SOItemList = [];
        $scope.QBOQCostingList = [];
        //$scope.ModelNew.Level = "Costing";
        $scope.RMPlanningNew.MaterialPlanType = "Costing";
        $scope.modelList = [];
        $rootScope.toggle();

    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.QBOQCostingList = [];
    $scope.GetQBOQCostingData = function () {
        if ($scope.SOItemList.length > 0) {
            var uniqueMasterOrderId = removeDuplicates($scope.SOItemList, 'SOId');
            var wcEmpCode = "";
            if (uniqueMasterOrderId.length > 0) {
                wcEmpCode = "IN(";
                wcEmpCode += Array.prototype.map.call(uniqueMasterOrderId, function (item) { return "'" + item.SOId + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcEmpCode;
        }


        $scope.QBOQCostingList = [];
        if ($scope.RMPlanningNew.MaterialPlanType == "Costing") {
            $http({
                method: 'GET',
                url: 'Materials/RawMaterialPlanning/GetCostingDataList?soId=' + $scope.sqlInStatement

            }).then(function successCallback(response) {
                $scope.QBOQCostingList = response.data;
            });
        }
        else {
            $http({
                method: 'GET',
                url: 'Materials/RawMaterialPlanning/GetQBOQDataList?soId=' + $scope.sqlInStatement

            }).then(function successCallback(response) {
                $scope.QBOQCostingList = response.data;
                for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
                    for (var j = 0; j < $scope.ConsumptionLevelList.length; j++) {
                        if ($scope.QBOQCostingList[i].ConsumptionLevel == $scope.ConsumptionLevelList[j].Value) {
                            $scope.QBOQCostingList[i].ConsumptionLevel = $scope.ConsumptionLevelList[j].Value;
                        }
                    }
                }
            });
        }
    }

    $scope.PlanQtyList = [];
    $scope.GetPlanQtyData = function () {
        if ($scope.SOItemList.length > 0) {
            var uniqueMasterOrderId = removeDuplicates($scope.SOItemList, 'SOId');
            var wcEmpCode = "";
            if (uniqueMasterOrderId.length > 0) {
                wcEmpCode = "IN(";
                wcEmpCode += Array.prototype.map.call(uniqueMasterOrderId, function (item) { return "'" + item.SOId + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcEmpCode;
        }


        $scope.PlanQtyList = [];
            $http({
                method: 'GET',
                url: 'Materials/RawMaterialPlanning/GetPlanQtyList?soId=' + $scope.sqlInStatement

            }).then(function successCallback(response) {
                $scope.PlanQtyList = response.data;
            });
    }

    //$scope.BoqCalculation = function (obj) {
    //    try {
    //        if (baseService.isUndefinedOrNull($scope.ModelNew.PlanPercentage) || $scope.ModelNew.PlanPercentage == 0 || $scope.ModelNew.PlanPercentage == 'NaN') {
    //            throw "Input Plan Percentage";
    //        }
    //        var totaPlanlAmount = 0;
    //        obj.data.PlanConsumption = (obj.data.TotalConsumption + obj.data.AdditionReduction) * $scope.ModelNew.PlanPercentage / 100;
    //        obj.data.TotaPlanlAmount = obj.data.PlanConsumption * obj.data.Rate;
    //        obj.data.ActualIssueAmount = obj.data.PlanConsumption * obj.data.StockRate;

    //        if ($scope.RMPlanningNew.MaterialPlanType == "Costing") {
    //            var gridObj = $("#CGrid").data("ejGrid");
    //            gridObj.refreshContent(true);
    //            gridObj.refreshTemplate();
    //        } else {
    //            var gridObj = $("#BGrid").data("ejGrid");
    //            gridObj.refreshContent(true);
    //            gridObj.refreshTemplate();
    //        }

    //        for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
    //            totaPlanlAmount += $scope.QBOQCostingList[i].TotaPlanlAmount;
    //        }

    //        for (var i = 0; i < $scope.SOItemList.length; i++) {
    //            $scope.SOItemList[i].PlanRate = totaPlanlAmount / $scope.SOItemList[i].PlannedQty;
    //            $scope.SOItemList[i].PlantCost = $scope.SOItemList[i].PlanRate * $scope.SOItemList[i].PlannedQty;
    //            $scope.SOItemList[i].TotalSOCostVsTotalPlanCost = $scope.SOItemList[i].SOTotalMaterailCost - $scope.SOItemList[i].PlantCost;
    //        }
    //        var gridObj = $("#SOGrid").data("ejGrid");
    //        gridObj.refreshContent(true);
    //        gridObj.refreshTemplate();

    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //}

    //$scope.CostCalculation = function (obj) {
    //    try {
    //        if (baseService.isUndefinedOrNull($scope.ModelNew.PlanPercentage) || $scope.ModelNew.PlanPercentage == 0 || $scope.ModelNew.PlanPercentage == 'NaN') {
    //            throw "Input Plan Percentage";
    //        }
    //        var totaPlanlAmount = 0;
    //        obj.data.PlanConsumption = (obj.data.TotalConsumption + obj.data.AdditionReduction) * $scope.ModelNew.PlanPercentage / 100;
    //        obj.data.TotaPlanlAmount = obj.data.PlanConsumption * obj.data.Rate;
    //        obj.data.ActualIssueAmount = obj.data.PlanConsumption * obj.data.StockRate;

    //        if ($scope.RMPlanningNew.MaterialPlanType == "Costing") {
    //            var gridObj = $("#CGrid").data("ejGrid");
    //            gridObj.refreshContent(true);
    //            gridObj.refreshTemplate();
    //        } else {
    //            var gridObj = $("#BGrid").data("ejGrid");
    //            gridObj.refreshContent(true);
    //            gridObj.refreshTemplate();
    //        }

    //        for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
    //            totaPlanlAmount += $scope.QBOQCostingList[i].TotaPlanlAmount;
    //        }

    //        for (var i = 0; i < $scope.SOItemList.length; i++) {
    //            $scope.SOItemList[i].PlanRate = totaPlanlAmount / $scope.SOItemList[i].PlannedQty;
    //            $scope.SOItemList[i].PlantCost = $scope.SOItemList[i].PlanRate * $scope.SOItemList[i].PlannedQty;
    //            $scope.SOItemList[i].TotalSOCostVsTotalPlanCost = $scope.SOItemList[i].SOTotalMaterailCost - $scope.SOItemList[i].PlantCost;
    //        }
    //        var gridObj = $("#SOGrid").data("ejGrid");
    //        gridObj.refreshContent(true);
    //        gridObj.refreshTemplate();

    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //}

    //$scope.CalculationByPlan = function () {
    //    try {
    //        if (baseService.isUndefinedOrNull($scope.ModelNew.PlanPercentage) || $scope.ModelNew.PlanPercentage == 0 || $scope.ModelNew.PlanPercentage == 'NaN') {
    //            throw "Input Plan Percentage";
    //        }
    //        var totaPlanlAmount = 0;
    //        for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
    //            $scope.QBOQCostingList[i].PlanConsumption = ($scope.QBOQCostingList[i].TotalConsumption + $scope.QBOQCostingList[i].AdditionReduction) * $scope.ModelNew.PlanPercentage / 100;
    //            $scope.QBOQCostingList[i].TotaPlanlAmount = $scope.QBOQCostingList[i].PlanConsumption * $scope.QBOQCostingList[i].Rate;
    //            $scope.QBOQCostingList[i].ActualIssueAmount = $scope.QBOQCostingList[i].PlanConsumption * $scope.QBOQCostingList[i].StockRate;
    //        }


    //        if ($scope.RMPlanningNew.MaterialPlanType == "Costing") {
    //            var gridObj = $("#CGrid").data("ejGrid");
    //            gridObj.refreshContent(true);
    //            gridObj.refreshTemplate();
    //        } else {
    //            var gridObj = $("#BGrid").data("ejGrid");
    //            gridObj.refreshContent(true);
    //            gridObj.refreshTemplate();
    //        }

    //        for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
    //            totaPlanlAmount += $scope.QBOQCostingList[i].TotaPlanlAmount;
    //        }

    //        for (var i = 0; i < $scope.SOItemList.length; i++) {
    //            $scope.SOItemList[i].PlanRate = totaPlanlAmount / $scope.SOItemList[i].PlannedQty;
    //            $scope.SOItemList[i].PlantCost = $scope.SOItemList[i].PlanRate * $scope.SOItemList[i].PlannedQty;
    //            $scope.SOItemList[i].TotalSOCostVsTotalPlanCost = $scope.SOItemList[i].SOTotalMaterailCost - $scope.SOItemList[i].PlantCost;
    //        }
    //        var gridObj = $("#SOGrid").data("ejGrid");
    //        gridObj.refreshContent(true);
    //        gridObj.refreshTemplate();
    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }

    //}

    $scope.filters = [];
    $scope.getFiltersData = function () {
        try {
            $http({
                method: 'GET',
                url: 'Materials/RawMaterialPlanning/GetFilterList',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filters = response.data;
                var columnList = [
                    { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                    { field: 'Customer', width: 20, headerText: "Customer", type: "string" },
                    { field: 'ProductCode', width: 20, headerText: "Product Code", type: "string" },
                    { field: 'OwnRefNo', width: 20, headerText: "Own Order#", type: "string" },
                    { field: 'BuyerRefNo', width: 20, headerText: "Cust. Order#", type: "string" }
                ];
                $("#filters").ejGrid({
                    dataSource: $scope.filters,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: columnList
                });

                var gridObj = $("#filters").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                $("#filters").children('.e-pager.e-js.e-pager').hide();
                $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#filters").children('.e-gridcontent').hide();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.getFiltersData();
    
    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "Customer", "Value": getString(fl, "Customer") });
        parameters.push({ "Key": "ProductCode", "Value": getString(fl, "ProductCode") });
        parameters.push({ "Key": "OwnRefNo", "Value": getString(fl, "OwnRefNo") });
        parameters.push({ "Key": "BuyerRefNo", "Value": getString(fl, "BuyerRefNo") });


        $scope.parameters = parameters;
    }

    var getString = function (data, column) {
        var string = "''";
        var collection = [];

        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }

    $scope.PRSearchColumn = 'Id';
    $scope.PRSearchValue = null;
    $scope.modelList = [];
    $scope.getData = function () {
        $scope.modelList = [];
        $scope.filterComplete();
            $http({
                method: 'POST',
                data: {
                    'parameters': $scope.parameters, 'column': $scope.PRSearchColumn, 'value': $scope.PRSearchValue
                },
                url: $scope.getListUrl
            }).then(function successCallback(response) {
                $scope.modelList = response.data;
            });
    };

    $scope.selectRMPPlanBy = function () {
        $scope.getRMPPlanBy();
        angular.element(document.querySelector('#RMPPlanByPopup')).modal('show');
    }

    $scope.EmployeeList = [];
    $scope.getRMPPlanBy = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.doubleEmployee = function (e) {
        $scope.RMPlanningNew.PlanById = e.data.SystemId;
        $scope.RMPlanningNew.PlanBy = e.data.EmployeeName;
        angular.element(document.querySelector('#RMPPlanByPopup')).modal('hide');
    }

    $scope.closeRMPPlanByPopUp = function () {
        angular.element(document.querySelector('#RMPPlanByPopup')).modal('hide');
    }

    $scope.PlanList = [];
    $scope.LoadPlanDetails = function (POId) {
        $http({

            method: 'Get',
            url: 'Materials/RawMaterialPlanning/LoadPlanDetails?POID='+ POId
        }).then(function successCallback(response) {
            $scope.PlanList = response.data;
        }
        )
    }

    $scope.GetPlanDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Materials/RawMaterialPlanning/LoadPlanEditData?PlanId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.RMPlanningNew = response.data.plan[0];
            $scope.RMPlanningNew.PlanDate = response.data.plan[0].FormatPlanDate;
            $scope.RMPlanningNew.PlanBy = response.data.plan[0].PlanBy;
            $scope.LoadSODetails($scope.RMPlanningNew.Id);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.LoadSODetails = function (PlanId) {
        $http.get('Materials/RawMaterialPlanning/GetSOPlanWiseList?PlanId=' + PlanId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SOItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
}