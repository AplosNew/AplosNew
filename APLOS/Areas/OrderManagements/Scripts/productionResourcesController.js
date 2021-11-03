'use strict';
productionResourcesController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function productionResourcesController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Production Resources';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.ProductionResources = [];
    $scope.path = 'OrderManagements/ProductionResources/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.ProductionResource = {
        Id: null,
        ResourceName: null,
        Quantity: null,
        UOMId: null,
        PlantId: null,
    };
    $scope.ProductionResourceNew = Object.assign({}, $scope.ProductionResource);
   
    $scope.getData = function () {
        $http.get('OrderManagements/ProductionResources/GetList?PlantId=' + $scope.ProductionResourceNew.PlantId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ProductionResources = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
   
    $scope.recorddoubleclick = function ($event) {
        $scope.ProductionResource  = $event.data;
        $scope.ProductionResourceNew = Object.assign({}, $scope.ProductionResource);
        $scope.Action = 'Update';
    };

    $scope.Save = function () {
        angular.copy($scope.ProductionResourceNew, $scope.ProductionResource);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ProductionResourcesForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.ProductionResource,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ProductionResources.push(response.data.ProductionResource);
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.ProductionResource,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.ProductionResources[$scope.index] = $scope.ProductionResource;
                        }
                        ClearFields();
                        $scope.getData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
       
        if (!baseService.isUndefinedOrNull($scope.ProductionResourceNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ProductionResourceNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ProductionResources.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();

        return true;
    };

    function ClearFields()

    {
        $scope.Action = 'Save';
        $scope.ProductionResource = {};
        $scope.ProductionResourceNew = { PlantId: $scope.ProductionResourceNew.PlantId };
    }

    $scope.UoMList = [];
    cboService.getUoMCbo(function (result) {
        $scope.UoMList = result;
    });

    $scope.PlantList = [];
    cboService.getCboPlantByCompany(null, function (result) {
        $scope.PlantList = result;
    });
}