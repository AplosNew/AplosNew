'use strict';
ProductionOrderEntitySetupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProductionOrderEntitySetupController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Production Order Entity Setup";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.dmms = [];
    $scope.path = 'Productions/ProductionOrderEntitySetup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.ProductionOrderEntitySetup = {
        Id: null,
        ProductionEntityId: null,
        FromEntityId: null,
        Type: null,
        MasterOrderType: null,
        OrderType: null,
        Applicable: null
    };
    $scope.ProductionOrderEntitySetupNew = Object.assign({}, $scope.ProductionOrderEntitySetup);

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.getAllEntities();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.dmm = $scope.dmms[$scope.index];
        $scope.dmmNew = Object.assign({}, $scope.dmm);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        angular.copy($scope.dmmNew, $scope.dmm);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.dmmNewForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.dmm,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.dmms.push(response.data.DMM);
                        $scope.dmms = $filter('orderBy')($scope.dmms, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.dmm,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.dmms[$scope.index] = $scope.dmm;
                            $scope.dmms = $filter('orderBy')($scope.dmms, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.dmmNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.dmmNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.dmms.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    }
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.ProductionOrderEntitySetup = {};
        $scope.ProductionOrderEntitySetupNew = {};
    }
}