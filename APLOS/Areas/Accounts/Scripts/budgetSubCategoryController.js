"use strict";
budgetSubCategoryController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http"];
function budgetSubCategoryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http) {
    $rootScope.title = "Budget Category";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.budgetSubCategories = [];
    $scope.path = "accounts/budgetsubcategory/";
    $scope.getListUrl = "accounts/companygroupbudgetsubcategory/getlist";
    $scope.getUrl = $scope.path + "get";
    $scope.getSeqUrl = $scope.path + "getautosequence";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.budgetSubCategories = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.budgetSubCategory = {
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
                $scope.budgetSubCategory.Sequence = response.data;
            });
    };

    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.budgetSubCategory = $scope.budgetSubCategories[$scope.index];
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.budgetSubCategoryForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: $scope.budgetSubCategory,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    } else {
                        ShowResult(response.data.Message, "success");
                        $scope.budgetSubCategories.push(response.data.BudgetSubCategory);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                },
                    function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                return true;
            } else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: $scope.budgetSubCategory,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    } else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.budgetSubCategories[$scope.index] = $scope.budgetSubCategory;
                        }
                        ClearFields(response.data.Sequence);
                    }
                },
                    function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                return true;
            }
        }
        return true;
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.budgetSubCategory.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.budgetSubCategory.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                } else {
                    ShowResult(response.data.Message, "success");
                    $scope.budgetSubCategories.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            },
                function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
        } else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.budgetSubCategory = {};
        $scope.budgetSubCategory.Sequence = seq;
        $scope.budgetSubCategory.Active = true;
    }
}