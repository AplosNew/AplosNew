'use strict';
MaterialIssueController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", "$controller"];
function MaterialIssueController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {
    $rootScope.title = "Material Issue";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SOItemList = [];
    $scope.path = 'Materials/MaterialIssueControl/';
    $scope.saveUrl = $scope.path + 'CreateApprove';
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = ['BOM'];
  
    $scope.ModelNew = { Id: null, POId: null, UserCode: null, UserRef: null, PlanPercentage: null, ByWhomId: null, UserName: null, Level: "Costing", LotNo: null, IsApproved: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };

    $scope.modelList = [];
    $scope.GetData = function () {
        $scope.modelList = [];
        $http.get('Materials/MaterialIssueControl/GetApprovedData')
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
        $http.get('Materials/MaterialIssueControl/GetSavedDetailData?masterId=' + $scope.ModelNew.Id)
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
        $scope.ModelNew = { Id: null, POId: null };
        $scope.SOItemList = [];
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
}