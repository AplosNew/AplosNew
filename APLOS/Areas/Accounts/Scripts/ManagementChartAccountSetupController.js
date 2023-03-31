'use strict';
ManagementChartAccountSetupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ManagementChartAccountSetupController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
        // #endregion TAB CHANGE

    // #region BudgetGroup
    $scope.ActionBG = "Save";
    $scope.index = -1;
    $scope.budgetGroups = [];
    $scope.pathBG = "accounts/budgetgroup/";
    $scope.saveUrlBG = $scope.pathBG + "create";
    $scope.updateUrlBG = $scope.pathBG + "edit";
    $scope.deleteUrlBG = $scope.pathBG + "delete/";
    $scope.getListUrlBG = "accounts/companygroupbudgetgroup/getlist";
    baseService.init($scope.getListUrlBG, null, 15);
    $scope.getDataBG = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.budgetGroups = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getDataBG();

    $scope.budgetGroup = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), "yyyy-MM-dd"),
        AddedFromIP: null,
        UpdatedDate: $filter("date")(Date.now(), "yyyy-MM-dd"),
        UpdatedFromIP: null
    };

    $scope.GetSequenceBG = function () {
        $http.get("accounts/budgetgroup/getautosequence")
            .then(function (response) {
                $scope.budgetGroup.Sequence = response.data;
            });
    };
    $scope.GetSequenceBG();

    $scope.GetBG = function (id, index) {
        $scope.index = index;
        $scope.budgetGroup = $scope.budgetGroups[$scope.index];
        $scope.ActionBG = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveBG = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.budgetGroupForm.$valid) {
            if ($scope.ActionBG === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrlBG,
                    data: $scope.budgetGroup,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.budgetGroups.push(response.data.BudgetGroup);
                        baseService.paginationAdd();
                        ClearFieldsBG(response.data.Sequence);
                    }
                }), function errorCallback(response) {
                    ShowResult(response.data.Message, "failure");
                };
            }
            else if ($scope.ActionBG === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrlBG,
                    data: $scope.budgetGroup,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.budgetGroups[$scope.index] = $scope.budgetGroup;
                        }
                        ClearFieldsBG(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, "failure");
                });
            }
        }
    };
    $scope.DeleteBG = function () {
        if (!baseService.isUndefinedOrNull($scope.budgetGroup.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrlBG + $scope.budgetGroup.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.budgetGroups.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFieldsBG(response.data.Sequence);
                }
            });
        }
    };

    $scope.ClearBG = function () {
        ClearFieldsBG($scope.GetSequenceBG());
        return true;
    };

    function ClearFieldsBG(seq) {
        $scope.ActionBG = "Save";
        $scope.budgetGroup = {};
        $scope.budgetGroup.Sequence = seq;
        $scope.budgetGroup.Active = true;
    }
    // #endregion BudgetGroup

    // #region BudgetCategory
    $rootScope.titleBC = "Budget Category";
    $scope.ActionBC = "Save";
    $scope.index = -1;
    $scope.budgetCategories = [];
    $scope.pathBC = "accounts/budgetCategory/";
    $scope.getListUrlBC = "accounts/companygroupbudgetcategory/getlist";
    $scope.getUrlBC = $scope.pathBC + "get";
    $scope.getSeqUrlBC = $scope.pathBC + "getautosequence";
    $scope.saveUrlBC = $scope.pathBC + "create";
    $scope.updateUrlBC = $scope.pathBC + "edit";
    $scope.deleteUrlBC = $scope.pathBC + "delete/";
    baseService.init($scope.getListUrlBC);
    $scope.getDataBC = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.budgetCategories = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getDataBC();

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

    $scope.GetSequenceBC = function () {
        $http.get($scope.getSeqUrlBC)
            .then(function (response) {
                $scope.budgetCategory.Sequence = response.data;
            });
    };
    $scope.GetSequenceBC();

    $scope.GetBC = function (id, index) {
        $scope.index = index;
        $scope.budgetCategory = $scope.budgetCategories[$scope.index];
        $scope.ActionBC = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveBC = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.budgetCategoryForm.$valid) {
            if ($scope.ActionBC === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrlBC,
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
                        ClearFieldsBC(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.ActionBC === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrlBC,
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
                        ClearFieldsBC(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }
        return true;
    };

    $scope.DeleteBC = function () {
        if (!baseService.isUndefinedOrNull($scope.budgetCategory.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrlBC + $scope.budgetCategory.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.budgetCategories.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFieldsBC(response.data.Sequence);
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

    $scope.ClearBC = function () {
        ClearFieldsBC($scope.GetSequenceBC());
        return true;
    };

    function ClearFieldsBC(seq) {
        $scope.ActionBC = "Save";
        $scope.budgetCategory = {};
        $scope.budgetCategory.Sequence = seq;
        $scope.budgetCategory.Active = true;
    }
    // #endregion BudgetCategory
}