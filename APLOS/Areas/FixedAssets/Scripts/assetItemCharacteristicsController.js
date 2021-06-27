'use strict';
assetItemCharacteristicsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function assetItemCharacteristicsController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'AssetItem Characteristics';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.fixedAssetCategories = [];
    $scope.path = 'fixedassets/AssetItemCharacteristics/';
    $scope.getListUrl = 'fixedassets/AssetItemCharacteristics/getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.fixedAssetCategories = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.assetItemCharacteristics = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        CharacteristicsProperty: null,
        IsFreeField: true,
        IsPreDefinedField: true,
        IsMandatory: true,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.assetItemCharacteristics.Sequence = response.data;
            });
    };

    $scope.GetSequence();
    $scope.characteristicsPropertyList = [];
    cboService.getEnumCbo("enum/GetAttributePropertiesCbo", function (result) {
        $scope.characteristicsPropertyList = result;
    });
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.getassetItemCharacteristics = angular.copy($scope.fixedAssetCategories[$scope.index]);
        $scope.assetItemCharacteristics = $scope.getassetItemCharacteristics;
        $scope.assetItemCharacteristics.AddedDate = $filter('dateFilter')($scope.assetItemCharacteristics.AddedDate);
        $scope.assetItemCharacteristics.UpdatedDate = $filter('dateFilter')($scope.assetItemCharacteristics.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.assetItemCharacteristicsForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.assetItemCharacteristics,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.fixedAssetCategories.push(response.data.AssetItemCharacteristics);
                        $scope.fixedAssetCategories = $filter('orderBy')($scope.fixedAssetCategories, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
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
                    data: $scope.assetItemCharacteristics,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.fixedAssetCategories[$scope.index] = $scope.assetItemCharacteristics;
                            $scope.fixedAssetCategories = $filter('orderBy')($scope.fixedAssetCategories, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.assetItemCharacteristics.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.assetItemCharacteristics.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.fixedAssetCategories.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
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
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.assetItemCharacteristics = {};
        $scope.assetItemCharacteristics.Sequence = seq;
        $scope.assetItemCharacteristics.Active = true;
    }
}