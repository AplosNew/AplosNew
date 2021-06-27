'use strict';
runningOrderParametersController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function runningOrderParametersController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Running Order Parameters';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.runningOrderParameters = [];
    $scope.path = 'OrderManagements/RunningOrderParameters/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.getData = function (pageno) {
        $rootScope.parameters.plantId = $scope.runningOrderParametersNew.PlantId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.runningOrderParameters = result;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.runningOrderParameter = {
        Id: null,
        BlockSize: null,
        PlantId: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
       
    };

    $scope.runningOrderParametersNew = Object.assign({}, $scope.runningOrderParameter);
    
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.runningOrderParameter = $scope.runningOrderParameters[$scope.index];
        $scope.runningOrderParametersNew = Object.assign({}, $scope.runningOrderParameter);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    
    $scope.Save = function () {
        angular.copy($scope.runningOrderParametersNew, $scope.runningOrderParameter);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.runningOrderParametersNew.BlockSize <= 0) {
            ShowResult("BlockSize can't be zero", 'failure');
            return false;
        }
        if ($scope.RunningOrderParametersNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.runningOrderParameter,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.runningOrderParameters.push(response.data.RunningOrderParameter);
                        $scope.runningOrderParameters = $filter('orderBy')($scope.runningOrderParameters, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.runningOrderParameter,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.runningOrderParameters[$scope.index] = $scope.runningOrderParameter;
                            $scope.runningOrderParameters = $filter('orderBy')($scope.runningOrderParameters, 'Sequence');
                        }
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.runningOrderParametersNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.runningOrderParametersNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.runningOrderParameters.splice($scope.index, 1);
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

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.runningOrderParameter = {};
        $scope.runningOrderParametersNew = { PlantId: $scope.runningOrderParametersNew.PlantId };
        $scope.runningOrderParametersNew.Active = true;
        
    }
    $scope.PlantList = [];
    cboService.getCboPlantByCompany(null, function (result) {
        $scope.PlantList = result;
    });
}