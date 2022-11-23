'use strict';
MaterialIssueControlApprovalController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", "$controller"];
function MaterialIssueControlApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {
    $rootScope.title = "Material Issue Control Approval";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SOItemList = [];
    $scope.path = 'Materials/MaterialIssueControl/';
    $scope.saveUrl = $scope.path + 'CreateApprove';
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = ['BOM'];
  
    $scope.ModelNew = { Id: null, POId: null, EntityId: null, MaterialStorageId: null, IssueDate: null, IssueType: 'Revenue', UserCode: null, UserRef: null, PlanPercentage: null, ByWhomId: null, UserName: null, Level: "Costing", LotNo: null, IsApproved: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };

    $scope.Getstorage = function () {
        $http({
            method: 'GET',
            url: 'Materials/MaterialStorage/getcbo'
        }).then(function (response) {
            $scope.storageList = response.data;
        });
    }
    $scope.Getstorage();

    $scope.modelList = [];
    $scope.GetData = function () {
        $scope.modelList = [];
        $http.get('Materials/MaterialIssueControl/GetSavedUnApprovedData')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.modelList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
      
    };
    $scope.GetData();

    $scope.SOItemList = [];
    $scope.GetSavedSODetailData = function () {
        $scope.SOItemList = [];
        $http.get('Materials/MaterialIssueControl/GetSavedSODetailData?masterId='+$scope.ModelNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SOItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };

    $scope.QBOQCostingList = [];
    $scope.GetSavedDetailData = function () {
        $scope.QBOQCostingList = [];
        $http.get('Materials/MaterialIssueControl/GetSavedDetailDataToApprove?masterId=' + $scope.ModelNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.QBOQCostingList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };

    $scope.Get = function (obj) {
        $scope.ModelNew = Object.assign({}, obj.data);
        $scope.ModelNew.IssueDate = $filter('dateFiltering')($scope.ModelNew.IssueDate, 'dd-M-yyyy');
        $scope.Action = 'Update';
        $scope.GetSavedSODetailData();
        $scope.GetSavedDetailData();
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
                            $scope.GetData();
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
        $scope.ModelNew = { Id: null, POId: null, EntityId: null, MaterialStorageId: null, IssueDate: null, IssueType: 'Revenue', UserCode: null, UserRef: null, PlanPercentage: null, ByWhomId: null, UserName: null, Level: "Costing", LotNo: null, IsApproved: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };
        $scope.SOItemList = [];
        $scope.QBOQCostingList = [];
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

    $scope.Calculation = function (obj) {
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
    }

    $scope.CalculationByPlan = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.PlanPercentage) || $scope.ModelNew.PlanPercentage == 0 || $scope.ModelNew.PlanPercentage == 'NaN') {
                throw "Input Plan Percentage";
            }
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
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }
}