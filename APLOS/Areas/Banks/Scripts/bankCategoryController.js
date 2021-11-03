"use strict";
bankCategoryController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter"];
function bankCategoryController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Bank Category";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.bankCategories = [];
    $scope.path = "banks/bankcategory/";
    $scope.getListUrl = $scope.path + "getlist";
    $scope.getUrl = $scope.path + "get";
    $scope.getSeqUrl = $scope.path + "getautosequence";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.bankCategories = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.bankCategory = {
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
                $scope.bankCategory.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.bankCategory = $scope.bankCategories[$scope.index];
        $scope.bankCategory.AddedDate = $filter("dateFilter")($scope.bankCategory.AddedDate);
        $scope.bankCategory.UpdatedDate = $filter("dateFilter")($scope.bankCategory.UpdatedDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.bankCategoryForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: $scope.bankCategory,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.bankCategories.push(response.data.BankCategory);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, "failure");
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: $scope.bankCategory,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.bankCategories[$scope.index] = $scope.bankCategory;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, "failure");
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.bankCategory.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.bankCategory.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.bankCategories.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                } function errorCallback(response) {
                    ShowResult(response.data.Message, "failure");
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.bankCategory = {};
        $scope.bankCategory.Sequence = seq;
        $scope.bankCategory.Active = true;
    }
}