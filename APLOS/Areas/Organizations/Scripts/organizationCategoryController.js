'use strict';
function OrganizationCategoryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Organization Category';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.organizationCategories = [];
    $scope.path = 'Organizations/organizationcategory/';
    $scope.getListUrl = $scope.path + 'getorganizationcategorylist';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.organizationCategories = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.organizationCategory = {
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
        $http.get('Organizations/organizationcategory/getautosequence')
            .then(function (response) {
                $scope.organizationCategory.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.organizationCategory = $scope.organizationCategories[$scope.index];
        $scope.organizationCategory.AddedDate = $filter('dateFilter')($scope.organizationCategory.AddedDate);
        $scope.organizationCategory.UpdatedDate = $filter('dateFilter')($scope.organizationCategory.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.organizationCategoryForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/organizationcategory/create',
                    data: $scope.organizationCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.organizationCategories.push(response.data.OrganizationCategory);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: 'Organizations/organizationcategory/edit',
                    data: $scope.organizationCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.organizationCategories[$scope.index] = $scope.organizationCategory;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.organizationCategory.Id)) {
            $http({
                method: 'POST',
                url: 'Organizations/organizationcategory/delete/' + $scope.organizationCategory.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.organizationCategories.splice($scope.index, 1);
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
        $scope.organizationCategory = {};
        $scope.organizationCategory.Sequence = seq;
        $scope.organizationCategory.Active = true;
    }
}
OrganizationCategoryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];