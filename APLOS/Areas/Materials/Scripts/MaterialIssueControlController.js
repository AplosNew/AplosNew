'use strict';
MaterialIssueControlController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", "$controller"];
function MaterialIssueControlController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {
    $rootScope.title = "Material Issue Control";
    $scope.Action = 'Save';
    $scope.index = -1;
    //$rootScope.isLeftMenuHide = true;
    //$rootScope.ShowHideSideBar();
    $scope.baseProcess = { Id: null, UserName: null };
    $scope.modelList = [];
    $scope.productionMaterialList = [];
    $scope.prdProcessSetList = [];
    $scope.productionEntityList = [];
    $scope.productionWorkCenterList = [];
    $scope.runningWorkCenterList = [];
    $scope.productWorkCenterList = [];
    $scope.productionStatusList = [];
    $scope.MaterialImagePath = virtualPath.ProductsImage;
    $scope.sortSettings = { sortedColumns: [{ field: "ProductionStatus", direction: "descending" }, { field: "LSD", direction: "ascending" }] };
    $scope.path = 'Materials/MaterialIssueControl/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.incrementType = [
        { text: "FIXED", contentType: "textonly" },
        { text: "PERCENTAGE", contentType: "textonly", selected: "selected" }];
    cboService.getProductionStatusCboByGroup(function (result) {
        $scope.productionStatusList = result;
    });
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
    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {

        }
    }
    $scope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }
    $scope.openPopupAngular = function (popupName) {
        try {
            angular.element(document.querySelector("#" + popupName + "")).modal("show");
        } catch (e) {

        }

    }

    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = ['BOM'];

    $scope.modelPriorityList = [];
    $scope.loadDataForPriority = function () {
        $scope.modelPriorityList = [];
        try {
            $http({
                method: 'POST',
                data: {
                    'baseprocessid': $scope.baseProcess.Id, 'entityid': $scope.EntityId, 'column': '', 'value': ''
                },
                url: $scope.getListUrl
            }).then(function successCallback(response) {
                for (var i = 0; i < response.data.length; i++) {
                    response.data[i].LSD = new Date(response.data[i].LSD);
                    response.data[i].FirstShipmentDate = new Date(response.data[i].FirstShipmentDate);
                    response.data[i].LastShipmentDate = new Date(response.data[i].LastShipmentDate);
                    response.data[i].LSD = new Date(response.data[i].LSD);
                }

                $scope.modelPriorityList = response.data;
            });
        } catch (e) {

        }
    }

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetAllEntityForPlanningType1"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.getAllEntities();

    $scope.PRSearchColumn = 'Id';
    $scope.PRSearchValue = null;
    $scope.modelList = [];
    $scope.getData = function () {
        $http({
            method: 'POST',
            data: {
                'entityid': $scope.EntityId, 'column': $scope.PRSearchColumn, 'value': $scope.PRSearchValue
            },
            url: $scope.getListUrl
        }).then(function successCallback(response) {
            $scope.modelList = response.data;
        });
    };

    $scope.ModelNew = { Id: null, POId: null, EntityId: null, Level:"Costing" };

    $scope.SOItemList = [];
    $scope.Get = function (obj) {
        $scope.ModelNew.POId = obj.data.Id;
        $scope.SOItemList = [];
        $http.get('Materials/MaterialIssueControl/GetSOItemList?entityid=' + $scope.EntityId + '&ProductionOrderId=' + obj.data.Id)
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
            var gridObj = $("#SOGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
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
            $scope.ModelNew.Id = null;
            $scope.ModelNew.EntityId = $scope.EntityId;
            if (baseService.arrayLength($scope.SOItemList) === 0) {
                throw "Select Production Order.";
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.POId)) {
                throw "Select Production Order.";
            }
            if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'model': $scope.ModelNew
                        , 'soList': $scope.SOItemList
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




}