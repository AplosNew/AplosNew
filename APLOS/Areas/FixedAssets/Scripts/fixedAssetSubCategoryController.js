'use strict';
fixedAssetSubCategoryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function fixedAssetSubCategoryController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'FixedAsset SubCategory';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.fixedAssetSubCategories = [];
    $scope.path = 'fixedassets/fixedassetsubcategory/';
    $scope.getListUrl = 'fixedassets/CompanyGroupFixedAssetSubCategory/GetList';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.fixedAssetSubCategories = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.fixedAssetSubCategory = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.fixedAssetSubCategory.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.getfixedAssetSubCategory = angular.copy($scope.fixedAssetSubCategories[$scope.index]);
        $scope.fixedAssetSubCategory = $scope.getfixedAssetSubCategory;
        $scope.fixedAssetSubCategory.AddedDate = $filter('dateFilter')($scope.fixedAssetSubCategory.AddedDate);
        $scope.fixedAssetSubCategory.UpdatedDate = $filter('dateFilter')($scope.fixedAssetSubCategory.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.FixedAssetSubCategoryForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.fixedAssetSubCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.fixedAssetSubCategories.push(response.data.FixedAssetSubCategory);
                        $scope.fixedAssetSubCategories = $filter('orderBy')($scope.fixedAssetSubCategories, 'Sequence');
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
                    data: $scope.fixedAssetSubCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.fixedAssetSubCategories[$scope.index] = $scope.fixedAssetSubCategory;
                            $scope.fixedAssetSubCategories = $filter('orderBy')($scope.fixedAssetSubCategories, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.fixedAssetSubCategory.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.fixedAssetSubCategory.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.fixedAssetSubCategories.splice($scope.index, 1);
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
        $scope.fixedAssetSubCategory = {};
        $scope.fixedAssetSubCategory.Sequence = seq;
        $scope.fixedAssetSubCategory.Active = true;
    }
}