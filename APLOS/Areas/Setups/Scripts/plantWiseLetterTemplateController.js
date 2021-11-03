'use strict';
plantWiseLetterTemplateController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService','$window'];
function plantWiseLetterTemplateController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService,$window) {
    $rootScope.title = "PlantWiseLetterTemplate";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.plantWiseLetterTemplates = [];
    $scope.path = 'setups/plantWiseLetterTemplate/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, 'Description1', 'Description1');
    $scope.getData = function (pageno) {
        $rootScope.parameters.PlantId = $scope.plantWiseLetterTemplate.PlantId;
        $rootScope.parameters.LetterType = $scope.plantWiseLetterTemplate.LetterType;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.plantWiseLetterTemplates = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.plantWiseLetterTemplate = {
        Id: null,
        PlantId: $window.plantId,
        Description1: null,
        Description2: null,
        Description3: null,
        LetterType:null
    };

    $scope.searchByList = [
        {
            'name': 'Body 1',
            'value': 'Description1'
        },
        {
            'name': 'Body 2',
            'value': 'Description2'
        },
        {
            'name': 'Body 3',
            'value': 'Description3'
        }
    ];
    $scope.letterTypeList = [];
    cboService.getEnumCbo("enum/GetLetterTypeCbo", function (result) {
        $scope.letterTypeList = result;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.plantWiseLetterTemplate = $scope.plantWiseLetterTemplates[$scope.index];
        $scope.plantWiseLetterTemplate.AddedDate = $filter('dateFilter')($scope.plantWiseLetterTemplate.AddedDate);
        $scope.plantWiseLetterTemplate.UpdatedDate = $filter('dateFilter')($scope.plantWiseLetterTemplate.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.plantWiseLetterTemplateForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.plantWiseLetterTemplate,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.plantWiseLetterTemplates.push(response.data.plantWiseLetterTemplate);
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
                    data: $scope.plantWiseLetterTemplate,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.plantWiseLetterTemplates[$scope.index] = $scope.plantWiseLetterTemplate;
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
        if (!baseService.isUndefinedOrNull($scope.plantWiseLetterTemplate.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.plantWiseLetterTemplate.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.plantWiseLetterTemplates.splice($scope.index, 1);
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
        $scope.PlantId = $scope.plantWiseLetterTemplate.PlantId;
        $scope.plantWiseLetterTemplate.PlantId = $scope.PlantId;
        $scope.LetterType = $scope.plantWiseLetterTemplate.LetterType;
        $scope.plantWiseLetterTemplate.LetterType = $scope.LetterType;
        $scope.plantWiseLetterTemplate = {};
    }
}