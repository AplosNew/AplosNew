'use strict';
fixedAssetDepreciationRuleController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function fixedAssetDepreciationRuleController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'DepreciationRule';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.depreciationRules = [];
    $scope.path = 'fixedassets/fixedAssetdepreciationrule/';
    $scope.getListUrl = 'fixedassets/fixedAssetdepreciationrule/getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Depreciation Rule',
            'value': 'DepreciationRules'
        },
        {
            'name': 'Factor',
            'value': 'Factor'
        },
        {
            'name': 'Salvage Value',
            'value': 'SalvageValue'
        },
        {
            'name': 'Life Time',
            'value': 'LifeTime'
        }
    ];
    baseService.init($scope.getListUrl, null, null, null, 'Code', 'Code');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.depreciationRules = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.depreciationRule = {
        Id: null,
        Code: null,
        Description: null,
        Factor: null,
        LifeTime: null,
        SalvageValue: null,
        DepreciationRules: null,
        DepreciationCharge: null,
        DepreciationPurchase: null,
        DepreciationDisposal: null,
        UniformAcross: true,
        Active: true
    };

    /***********cbo*************/
    $scope.FixedAssetCategoryList = [];
    $scope.DepreciationChargeList = [];
    cboService.getFixedAssetCategoryList(function (result) {
        $scope.FixedAssetCategoryList = result;
    });
    cboService.getEnumCbo('Enum/GetJobDescriptionFrequencyListCbo', function (result) {
        $scope.DepreciationChargeList = result;
    });
    /***/

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.getdepreciationRule = angular.copy($scope.depreciationRules[$scope.index]);
        $scope.depreciationRule = $scope.getdepreciationRule;
        $scope.depreciationRule.AddedDate = $filter('dateFilter')($scope.depreciationRule.AddedDate);
        $scope.depreciationRule.UpdatedDate = $filter('dateFilter')($scope.depreciationRule.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.depreciationRuleForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.depreciationRule,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.depreciationRules.push(response.data.DepreciationRule);
                        $scope.depreciationRules = $filter('orderBy')($scope.depreciationRules, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.depreciationRule,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.depreciationRules[$scope.index] = $scope.depreciationRule;
                            $scope.depreciationRules = $filter('orderBy')($scope.depreciationRules, 'Sequence');
                        }
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.depreciationRule.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.depreciationRule.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.depreciationRules.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.depreciationRule = {};
        $scope.depreciationRule.Active = true;
    }
}