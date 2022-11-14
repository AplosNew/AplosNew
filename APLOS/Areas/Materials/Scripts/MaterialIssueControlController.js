'use strict';
MaterialIssueControlController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", "$controller"];
function MaterialIssueControlController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {
    $rootScope.title = "Material Issue Control";
    $scope.Action = 'Save';
    $scope.index = -1;

    $scope.path = 'Materials/MaterialIssueControl/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

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
    $scope.ModelNew = { Id: null, POId: null, EntityId: null, Entity: null, UserCode: null, UserRef: null, PlanPercentage: null, ByWhomId: null, UserName: null, Level: "Costing", LotNo: null, IsApproved: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'Get',
            url: "Materials/MaterialIssueControl/EntityList"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.getAllEntities();

    $scope.GetEntityName = function () {
        for (var i = 0; i < $scope.entityList.length; i++) {
            if ($scope.entityList[i].Value == $scope.ModelNew.EntityId) {
                $scope.ModelNew.Entity = $scope.entityList[i].Text;
                break;
            }
        }

    }
    $scope.storageList = [];

    $scope.Getstorage = function () {
        $http({
            method: 'GET',
            url: 'Materials/MaterialStorage/getcbo'
        }).then(function (response) {
            $scope.storageList = response.data;
        });
    }
    $scope.Getstorage();

    $scope.PRSearchColumn = 'Id';
    $scope.PRSearchValue = null;
    $scope.modelList = [];
    $scope.getData = function () {
        $scope.modelList = [];
        if (!baseService.isUndefinedOrNull($scope.ModelNew.EntityId)) {
            $http({
                method: 'POST',
                data: {
                    'entityid': $scope.ModelNew.EntityId, 'column': $scope.PRSearchColumn, 'value': $scope.PRSearchValue
                },
                url: $scope.getListUrl
            }).then(function successCallback(response) {
                $scope.modelList = response.data;
            });
        }
    };
    $scope.getData();

    $scope.SOItemList = [];
    $scope.Get = function (obj) {
        $scope.ModelNew.POId = obj.data.Id;

        $scope.SOItemList = [];
        $http.get('Materials/MaterialIssueControl/GetSOItemList?entityid=' + $scope.ModelNew.EntityId + '&ProductionOrderId=' + obj.data.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SOItemList = response.data;
                    }
                    $scope.GetQBOQCostingData();
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getArticle = function (data) {
        $scope.SelectedMaterial = data;
        $scope.getArticleSearchList(data.MaterialMasterId);
    };
    $scope.selectarticle = function (ob) {
        try {

            $scope.SelectedMaterial.MaterialMasterId = ob.MaterialMasterId;
            $scope.SelectedMaterial.Material = ob.MaterialMasterName;
            $scope.SelectedMaterial.ArticleId = ob.Id;
            $scope.SelectedMaterial.Article = ob.StandardName;

            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
            if ($scope.ModelNew.Level == "Costing") {
                var gridObj = $("#CGrid").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            } else {
                var gridObj = $("#BGrid").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            }
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.popUpDataList = [];
    $scope.showEmployeeListPopUp = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/SalesOrderApproval/GetAllActiveEmployeeData'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });

            angular.element(document.querySelector('#popUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        $scope.ModelNew.ByWhomId = arg.data.SystemId;
        $scope.ModelNew.ByWhom = arg.data.EmployeeName;
        $scope.closePopUp();
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    $scope.Action = 'Save';
    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.POId)) {
                throw "Select Production Order.";
            }
            if (baseService.arrayLength($scope.SOItemList) === 0) {
                throw "Select SO Detail.";
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'model': $scope.ModelNew
                            , 'soList': $scope.SOItemList
                            , 'dataList': $scope.QBOQCostingList
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
        $scope.ModelNew = { Id: null, POId: null, UserCode: null, UserRef: null, PlanPercentage: null, ByWhomId: null, UserName: null, Level: "Costing", LotNo: null, IsApproved: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };
        $scope.SOItemList = [];
        $scope.QBOQCostingList = [];
        $scope.ModelNew.Level = "Costing";
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
        if ($scope.ModelNew.Level == "Costing") {
            $http({
                method: 'GET',
                url: 'Materials/MaterialIssueControl/GetCostingDataList?soId=' + $scope.sqlInStatement

            }).then(function successCallback(response) {
                $scope.QBOQCostingList = response.data;
            });
        }
        else {
            $http({
                method: 'GET',
                url: 'Materials/MaterialIssueControl/GetQBOQDataList?soId=' + $scope.sqlInStatement

            }).then(function successCallback(response) {
                $scope.QBOQCostingList = response.data;
            });
        }
    }

    $scope.Calculation = function (obj) {
        var totaPlanlAmount = 0;
        obj.data.PlanConsumption = (obj.data.TotalConsumption + obj.data.AdditionReduction) * $scope.ModelNew.PlanPercentage / 100;
        obj.data.TotaPlanlAmount = obj.data.PlanConsumption * obj.data.Rate;
        obj.data.ActualIssueAmount = obj.data.PlanConsumption * obj.data.StockRate;

        if ($scope.ModelNew.Level == "Costing") {
            var gridObj = $("#CGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        } else {
            var gridObj = $("#BGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        }

        for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
            totaPlanlAmount += $scope.QBOQCostingList[i].TotaPlanlAmount;
        }

        for (var i = 0; i < $scope.SOItemList.length; i++) {
            $scope.SOItemList[i].PlanRate = totaPlanlAmount / $scope.SOItemList[i].PlannedQty;
            $scope.SOItemList[i].PlantCost = $scope.SOItemList[i].PlanRate * $scope.SOItemList[i].PlannedQty;
            $scope.SOItemList[i].TotalSOCostVsTotalPlanCost = $scope.SOItemList[i].SOTotalMaterailCost - $scope.SOItemList[i].PlantCost;
        }
        var gridObj = $("#SOGrid").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();

    }

    $scope.CalculationByPlan = function () {
        var totaPlanlAmount = 0;
        for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
            $scope.QBOQCostingList[i].PlanConsumption = ($scope.QBOQCostingList[i].TotalConsumption + $scope.QBOQCostingList[i].AdditionReduction) * $scope.ModelNew.PlanPercentage / 100;
            $scope.QBOQCostingList[i].TotaPlanlAmount = $scope.QBOQCostingList[i].PlanConsumption * $scope.QBOQCostingList[i].Rate;
            $scope.QBOQCostingList[i].ActualIssueAmount = $scope.QBOQCostingList[i].PlanConsumption * $scope.QBOQCostingList[i].StockRate;
        }


        if ($scope.ModelNew.Level == "Costing") {
            var gridObj = $("#CGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        } else {
            var gridObj = $("#BGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        }

        for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
            totaPlanlAmount += $scope.QBOQCostingList[i].TotaPlanlAmount;
        }

        for (var i = 0; i < $scope.SOItemList.length; i++) {
            $scope.SOItemList[i].PlanRate = totaPlanlAmount / $scope.SOItemList[i].PlannedQty;
            $scope.SOItemList[i].PlantCost = $scope.SOItemList[i].PlanRate * $scope.SOItemList[i].PlannedQty;
            $scope.SOItemList[i].TotalSOCostVsTotalPlanCost = $scope.SOItemList[i].SOTotalMaterailCost - $scope.SOItemList[i].PlantCost;
        }
        var gridObj = $("#SOGrid").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();

    }



}