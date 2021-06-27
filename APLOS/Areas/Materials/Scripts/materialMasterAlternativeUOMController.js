'use strict';
//Now Not Use
function MaterialMasterAlternativeUOMController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "MaterialMasterAlternativeUOM";
    $scope.Action = 'Save';
    $scope.materialMasterAlternativeUOMs = [];
    $scope.index = -1;
    $scope.path = 'Materials/materialmasteralternativeuom/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "MaterialMasterId", "MaterialMasterId");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.materialMasterAlternativeUOMs = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.searchCharacteristicsByList = [
        {
            'name': 'Material Master',
            'value': 'MaterialMasterId'
        },
        {
            'name': 'Alternative UOM',
            'value': 'AlternativeUOMId'
        },
        {
            'name': 'AlternativeUOM Factor',
            'value': 'AlternativeUOMFactor'
        }
    ];

    $scope.materialMasterAlternativeUOM = {
        Id: null,
        MaterialMasterId: null,
        AlternativeUOMId: null,
        AlternativeUOMFactor: null,
        BaseUOMId: null,
        BaseUOMFactor: null,
        Active: true,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        UpdatedFromIP: null
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $http.get("Materials/characteristics/getcharacteristics/" + id)
            .then(function (response) {
                $scope.materialMasterAlternativeUOM = response.data;
                $scope.Action = "Update";
            });
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.materialMasterAlternativeUOM.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
        $scope.materialMasterAlternativeUOM.UpdatedDate = null;
        if ($scope.Action == "Save") {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.materialMasterAlternativeUOMForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.materialMasterAlternativeUOM,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(data.Message, "success");
                        $scope.materialMasterAlternativeUOM = data.MaterialMasterAlternativeUOM;
                        $scope.materialMasterAlternativeUOM.AddedDate = $filter('dateFilter')($scope.materialMasterAlternativeUOM.AddedDate);
                        $scope.materialMasterAlternativeUOMs.push($scope.materialMasterAlternativeUOM);
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            return true;
        }
        else if ($scope.Action == "Update") {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.materialMasterAlternativeUOMForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.materialMasterAlternativeUOM,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.materialMasterAlternativeUOMs[$scope.index] = $scope.materialMasterAlternativeUOM;
                        }
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            return true;
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.materialMasterAlternativeUOM.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.materialMasterAlternativeUOM.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.materialMasterAlternativeUOMs.splice($scope.index, 1);
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

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.materialMasterAlternativeUOM = {};
        $scope.materialMasterAlternativeUOM.Active = true;
    }
}
MaterialMasterAlternativeUOMController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
