
'use strict';
costingTypesController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function costingTypesController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Costing Types";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.costingTypeses = [];
    $scope.path = 'Productions/CostingTypes/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.getData = function () {
        $http.get($scope.getListUrl)
            .then(function (response) {
                $scope.costingTypeses = response.data;
                if (baseService.arrayLength(response.data) > 0) {
                    for (var i = 0; i < $scope.costingTypeses.length; i++) {
                        if ($scope.costingTypeses[i].CostingType ==='CostingType1') {
                            $scope.costingTypeses[i].Description = 'CostingType1';
                        }
                        else if ($scope.costingTypeses[i].CostingType === 'CostingType2') {
                            $scope.costingTypeses[i].Description = 'CostingType2';
                        } else {
                            $scope.costingTypeses[i].Description = $scope.costingTypeses[i].CostingType;
                        }

                    }
                }
            });
    };
    $scope.getData();

    $scope.costingTypes = {
        Id: null,
        UserName: null,
        CostingType: null
    };
    $scope.costingTypesNew = Object.assign({}, $scope.costingTypes);

    $scope.costingTypesList = [];
    cboService.getEnumCbo('Enum/GetCostingTypeEnumCbo/', function (result) {
        $scope.costingTypesList = result;
    });

    $scope.ChangeType = function () {
        if ($scope.costingTypesNew.CostingType === 'CostingType1') {
            $scope.costingTypesNew.Description = 'CostingType1';
        }
        else if ($scope.costingTypesNew.CostingType === 'CostingType2') {
            $scope.costingTypesNew.Description = 'CostingType2';
        } else {
            $scope.costingTypesNew.Description = $scope.costingTypesNew.CostingType;
        }

    }

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.costingTypes = $scope.costingTypeses[$scope.index];
        $scope.costingTypesNew = Object.assign({}, $scope.costingTypes);
        $scope.Action = 'Update';
    };
    $scope.Save = function () {
        angular.copy($scope.costingTypesNew, $scope.costingTypes);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.CostingTypeForm.$valid) {
            if ($scope.Action == "Save" || $scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.costingTypes,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        ClearFields();
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            
        }
    }
    $scope.valuePass = function (index, data) {
        $scope.Id = data.Id;
        $scope.Index = index;
        if (baseService.isUndefinedOrNull($scope.Id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete parmanently [ ' + data.CostingType + ' ]';
        angular.element(document.querySelector('#removePopUp')).modal('show');
    };

    $scope.Delete = function () {
        if (baseService.isUndefinedOrNull($scope.Id)) {
            $scope.costingTypeses.splice($scope.Index, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'Productions/CostingTypes/Delete?id=' + $scope.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.costingTypesNew = {};
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.costingTypeses.splice($scope.Index, 1);
                    $scope.getData();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = "Save";
        $scope.costingTypes = {};
        $scope.costingTypesNew = {};
    }
}