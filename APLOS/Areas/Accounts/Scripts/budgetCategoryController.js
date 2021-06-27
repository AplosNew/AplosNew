"use strict";
budgetCategoryController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http"];
function budgetCategoryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http) {
    $rootScope.title = "Budget Category";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.budgetCategories = [];
    $scope.path = "accounts/budgetCategory/";
    $scope.getListUrl = "accounts/companygroupbudgetcategory/getlist";
    $scope.getUrl = $scope.path + "get";
    $scope.getSeqUrl = $scope.path + "getautosequence";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.budgetCategories = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.budgetCategory = {
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
                $scope.budgetCategory.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.budgetCategory = $scope.budgetCategories[$scope.index];
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.budgetCategoryForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: $scope.budgetCategory,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.budgetCategories.push(response.data.BudgetCategory);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: $scope.budgetCategory,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.budgetCategories[$scope.index] = $scope.budgetCategory;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }
        return true;
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.budgetCategory.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.budgetCategory.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.budgetCategories.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
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
        $scope.budgetCategory = {};
        $scope.budgetCategory.Sequence = seq;
        $scope.budgetCategory.Active = true;
    }
}