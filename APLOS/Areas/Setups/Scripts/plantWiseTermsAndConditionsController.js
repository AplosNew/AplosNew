'use strict';
plantWiseTermsAndConditionsController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService','$window'];
function plantWiseTermsAndConditionsController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService,$window) {
    $rootScope.title = "PlantWiseTermsAndConditions";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.plantWiseTermsAndConditions = [];
    $scope.path = 'setups/plantwisetermsandconditions/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, 'Description1', 'Description1');

    $scope.plantWiseTermsAndCondition = {
        Id: null,
        PlantId: $window.plantId,
        Description1: null,
        Description2: null
    };
    $scope.getData = function (pageno) {
        $rootScope.parameters.PlantId = $scope.plantWiseTermsAndCondition.PlantId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.plantWiseTermsAndConditions = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.searchByList = [
        {
            'name': 'Description1',
            'value': 'Description1'
        },
        {
            'name': 'Description2',
            'value': 'Description2'
        }
    ];

    $scope.plantList = [];
    cboService.getCboPlantByCompany(null, function (result) {
        $scope.plantList = result;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.plantWiseTermsAndCondition = $scope.plantWiseTermsAndConditions[$scope.index];
        $scope.plantWiseTermsAndCondition.AddedDate = $filter('dateFilter')($scope.plantWiseTermsAndCondition.AddedDate);
        $scope.plantWiseTermsAndCondition.UpdatedDate = $filter('dateFilter')($scope.plantWiseTermsAndCondition.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.plantWiseTermsAndConditionForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.plantWiseTermsAndCondition,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.plantWiseTermsAndConditions.push(response.data.PlantWiseTermsAndConditions);
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.getData();
                    }
                }), function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.plantWiseTermsAndCondition,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.plantWiseTermsAndConditions[$scope.index] = $scope.plantWiseTermsAndCondition;
                        }
                        ClearFields();
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.plantWiseTermsAndCondition.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.plantWiseTermsAndCondition.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.plantWiseTermsAndConditions.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                } function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.PlantId = $scope.plantWiseTermsAndCondition.PlantId;
        $scope.plantWiseTermsAndCondition = {};
        $scope.plantWiseTermsAndCondition.PlantId = $scope.PlantId;
    }
}