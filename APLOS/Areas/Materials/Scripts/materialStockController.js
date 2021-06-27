'use strict';
materialStockController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function materialStockController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Material Stock";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.models = [];
    $scope.path = 'Materials/materialstock/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.GetList = function () {
        baseService.init($scope.getListUrl, null, null, null, 'Code, UserName', 'UserName');
        $scope.getData = function (pageno) {
            var tempParam = [];
            tempParam.push($scope.searchModel.Code);
            tempParam.push($scope.searchModel.ShortName);
            tempParam.push($scope.searchModel.StandardName);
            tempParam.push($scope.searchModel.UserName);
            tempParam.push($scope.searchModel.BaseUoM);
            $rootScope.parameters.tempParam = JSON.stringify(tempParam);
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.buyerStyles = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
        angular.element(document.querySelector('#stockId')).modal('show');
    };
    $scope.model = {
        Id: null
        , MaterialMasterId: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , BaseUoM: null
        , UsageType: null
        , RequirementType: null
        , PurchaseFrequency: null
        , HazardsLevel: null
        , Flammability: null
        , LifeTimeinDays: 0
        , IsLocal: false
        , IsImport: false
        , StandardRate: 0
        , StandardRateCurrencyId: null
        , StandardRateUoMId: null
        , InventoryUoMId: null
        , MinimumOrderQuantity: 0
        , MinimumInventoryLevel: 0
        , ReorderLevel: 0
    };
    $scope.modelNew = Object.assign({}, $scope.model);
    $scope.searchModel = {
        Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , BaseUoM: null
    };
    $scope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.model = $scope.buyerStyles[$scope.index];
        $scope.modelNew = Object.assign({}, $scope.model);
        $scope.Action = "Update";
        angular.element(document.querySelector('#stockId')).modal('hide');
    };

    // #region DDl
    $scope.usageTypeList = [];
    cboService.getEnumCbo('enum/getusagetypelistcbo', function (response) {
        $scope.usageTypeList = response;
    });
    $scope.requirementTypeList = [];
    cboService.getEnumCbo('enum/getrequirementtypelistcbo', function (response) {
        $scope.requirementTypeList = response;
    });
    $scope.hazardsLevelList = [];
    cboService.getEnumCbo('enum/gethazardslistcbo', function (response) {
        $scope.hazardsLevelList = response;
        $scope.modelNew.HazardsLevel = response[0].Value;
    });
    $scope.flammabilityList = [];
    cboService.getEnumCbo('enum/getflammabilitylistcbo', function (response) {
        $scope.flammabilityList = response;
        $scope.modelNew.Flammability = response[0].Value;
    });
    $scope.purchaseFrequencyList = [];
    cboService.getEnumCbo('enum/getpurchasefrequencylistcbo', function (response) {
        $scope.purchaseFrequencyList = response;
    });
    $scope.currencyList = [];
    cboService.getCompanyGroupCurrencyCbo(null, function (response) {
        $scope.currencyList = response;
    });
    function getMaterialMasterUoMCbo() {
        $scope.materialUoMList = [];
        var ids = [];
        ids.push($scope.modelNew.MaterialMasterId);
        cboService.getUomCboByMaterialMaster(JSON.stringify(ids), function (response) {
            $scope.materialUoMList = response;
        });
    }
    // #endregion DDl

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            angular.copy($scope.modelNew, $scope.model);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.model,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, "failure");
                    else {
                        ShowResult(response.data.Message, "success");
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.model,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, "failure");
                    else {
                        ShowResult(response.data.Message, "success");
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.model = {};
        $scope.modelNew = {
            HazardsLevel: $scope.hazardsLevelList[0].Value
            , Flammability: $scope.flammabilityList[0].Value
        };
    }

    // #region MM
    $scope.materialModel = {
        materialTypeId: null
        , materialCategoryId: null
        , materialSubCategoryId: null
        , materialGroupMasterId: null
    };
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        $scope.popUpDataList = [];
        $scope.popUpUrl = $scope.path + 'GetMaterialMasterList';
        baseService.setCurrentPage('popUpDataList');
        $scope.getPopUpData = function (pageno) {
            var paramList = [];
            paramList.push($scope.materialModel.materialTypeId);
            paramList.push($scope.materialModel.materialGroupMasterId);
            paramList.push($scope.materialModel.materialCategoryId);
            paramList.push($scope.materialModel.materialSubCategoryId);
            $scope.popUpParameters.paramList = JSON.stringify(paramList);
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };
    $scope.selectDoubleClick = function (data) {
        ClearFields();
        $scope.modelNew = data;
        getMaterialMasterUoMCbo();
        $scope.closePopUp();
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        $scope.materialModel = {};
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
    // #endregion MM
}