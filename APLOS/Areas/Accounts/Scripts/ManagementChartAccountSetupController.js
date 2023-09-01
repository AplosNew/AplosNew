'use strict';
ManagementChartAccountSetupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ManagementChartAccountSetupController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.searchBy = "Sequence"; $scope.search = "";
    $scope.searchBGBy = "Sequence"; $scope.searchBG = "";
    $scope.searchBCBy = "Sequence"; $scope.searchBC = "";
    $scope.searchBSCBy = "Sequence"; $scope.searchBSC = "";
    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
        $scope.getDataBG();
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
   // baseService.init($scope.getListUrlBG, null, 15);
    $scope.searchByBGList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Defined Name',
            'value': 'UserName'
        }
    ];
    $scope.BGparameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "Sequence",
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getDataBG = function () {
        $scope.budgetGroups = [];
        $http.get('accounts/companygroupbudgetgroup/getlist?column=' + $scope.searchBGBy + '&value=' + $scope.searchBG)
            .then(function (response) {
                $scope.budgetGroups = response.data;
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

    $scope.GetBG = function (args) {
        $scope.budgetGroup = Object.assign({}, args.data);
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
                        $scope.getDataBG();
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
                        $scope.getDataBG();
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
                    $scope.getDataBG();
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
    //baseService.init($scope.getListUrlBC);

    $scope.searchByBCList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Defined Name',
            'value': 'UserName'
        }
    ];
    $scope.BCparameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "Sequence",
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getDataBC = function () {
        $scope.budgetCategories = [];
        $http.get('accounts/companygroupbudget/getlist?column=' + $scope.searchBCBy + '&value=' + $scope.searchBC)
            .then(function (response) {
                $scope.budgetCategories = response.data;
            });
    };
    // #region
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

    $scope.GetBC = function (args) {
        $scope.budgetCategory = Object.assign({}, args.data);
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
                        $scope.getDataBC();
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
                        $scope.getDataBC();
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
                    $scope.getDataBC();
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

    // #endregion
    // #endregion BudgetCategory

    // #region BudgetSubCategory
    $rootScope.titleBSC = "Budget Sub Category";
    $scope.ActionBSC = "Save";
    $scope.index = -1;
    $scope.budgetSubCategories = [];
    $scope.pathBSC = "accounts/budgetsubcategory/";
    $scope.getListUrlBSC = "accounts/companygroupbudgetsubcategory/getlist";
    $scope.getUrl = $scope.pathBSC + "get";
    $scope.getSeqUrlBSC = $scope.pathBSC + "getautosequence";
    $scope.saveUrlBSC = $scope.pathBSC + "create";
    $scope.updateUrlBSC = $scope.pathBSC + "edit";
    $scope.deleteUrlBSC = $scope.pathBSC + "delete/";
   // baseService.init($scope.getListUrlBSC);

    $scope.searchByBSCList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Defined Name',
            'value': 'UserName'
        }
    ];
    $scope.BSCparameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "Sequence",
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getDataBSC = function () {
        $scope.budgetSubCategories = [];
        $http.get('accounts/CompanyGroupBudgetSubCategory/getlist?column=' + $scope.searchBSCBy + '&value=' + $scope.searchBSC)
            .then(function (response) {
                $scope.budgetSubCategories = response.data;
            });
    };
   // $scope.getDataBSC();

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

    $scope.GetSequenceBSC = function () {
        $http.get($scope.getSeqUrlBSC)
            .then(function (response) {
                $scope.budgetSubCategory.Sequence = response.data;
            });
    };

    $scope.GetSequenceBSC();

    $scope.GetBSC = function (obj) {
        $scope.budgetSubCategory = Object.assign({}, obj.data);
        $scope.ActionBSC = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveBSC = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.budgetSubCategoryForm.$valid) {
            if ($scope.ActionBSC === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrlBSC,
                    data: $scope.budgetSubCategory,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    } else {
                        ShowResult(response.data.Message, "success");
                        $scope.budgetSubCategories.push(response.data.BudgetSubCategory);
                        $scope.getDataBSC();
                        ClearFieldsBSC(response.data.Sequence);
                    }
                },
                    function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                return true;
            } else if ($scope.ActionBSC === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrlBSC,
                    data: $scope.budgetSubCategory,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    } else {
                        ShowResult(response.data.Message, "success");
                        $scope.getDataBSC();
                        ClearFieldsBSC(response.data.Sequence);
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

    $scope.DeleteBSC = function () {
        if (!baseService.isUndefinedOrNull($scope.budgetSubCategory.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrlBSC + $scope.budgetSubCategory.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                } else {
                    ShowResult(response.data.Message, "success");
                    $scope.budgetSubCategories.splice($scope.index, 1);
                    $scope.getDataBSC();
                    ClearFieldsBSC(response.data.Sequence);
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

    $scope.ClearBSC = function () {
        ClearFieldsBSC($scope.GetSequenceBSC());
        return true;
    };

    function ClearFieldsBSC(seq) {
        $scope.ActionBSC = "Save";
        $scope.budgetSubCategory = {};
        $scope.budgetSubCategory.Sequence = seq;
        $scope.budgetSubCategory.Active = true;
    }
    // #endregion BudgetSubCategory

    // #region Budget
    // #region
    $rootScope.titleBgt = "Budget";
    $scope.ActionBgt = "Save";
    $scope.index = -1;
    $scope.budgets = [];
    $scope.pathBgt = "accounts/budget/";
    $scope.getListUrlBgt = "accounts/companygroupbudget/getlist";
    $scope.getUrlBgt = $scope.pathBgt + "get";
    $scope.getSeqUrlBgt = $scope.pathBgt + "getautosequence";
    $scope.saveUrlBgt = $scope.pathBgt + "create";
    $scope.updateUrlBgt = $scope.pathBgt + "edit";
    $scope.deleteUrlBgt = $scope.pathBgt + "delete/";
    // #endregion

    
    $scope.searchByBgtList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Defined Name',
            'value': 'UserName'
        }
    ];

    $scope.getDataBgt = function () {
        $scope.budgets = [];
        $http.get('accounts/companygroupbudget/getlist?column='+ $scope.searchBy + '&value='+ $scope.search)
            .then(function (response) {
                $scope.budgets = response.data;
            });
    };
    $scope.getDataBgt();

    $scope.GetBgt = function (args) {
        $scope.budget = Object.assign({}, args.data);
        $scope.budget.AddedDate = $filter("dateFilter")($scope.budget.AddedDate);
        $scope.budget.UpdatedDate = $filter("dateFilter")($scope.budget.UpdatedDate);
        $scope.getDataBgt();
        $scope.ActionItem = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.budget = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };

    $scope.GetSequenceBgt = function () {
        $http.get($scope.getSeqUrlBgt)
            .then(function (response) {
                $scope.budget.Sequence = response.data;
            });
    };
    $scope.GetSequenceBgt();


    $scope.SaveBgt = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.budgetForm.$valid) {
            if ($scope.ActionBgt === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrlBgt,
                    data: $scope.budget,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.budgets.push(response.data.Budget);
                        baseService.paginationAdd();
                        ClearFieldsBgt(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.ActionBgt === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrlBgt,
                    data: $scope.budget,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.budgets[$scope.index] = $scope.budget;
                        }
                        ClearFieldsBgt(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }
        return true;
    };

    $scope.DeleteBgt = function () {
        if (!baseService.isUndefinedOrNull($scope.budget.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrlBgt + $scope.budget.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.budgets.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFieldsBgt(response.data.Sequence);
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

    $scope.ClearBgt = function () {
        ClearFieldsBgt($scope.GetSequenceBgt());
        return true;
    };

    function ClearFieldsBgt(seq) {
        $scope.Action = "Save";
        $scope.budget = {};
        $scope.budget.Sequence = seq;
        $scope.budget.Active = true;
    }
    // #endregion Budget

    //  #region Activity
    $rootScope.title = "Budget";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.activities = [];
    $scope.path = "accounts/activity/";
    $scope.getSeqUrl = $scope.path + "getautosequence";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    $scope.getListUrl = "accounts/CompanyGroupActivity/getlist";
    //baseService.init($scope.getListUrl);

    $scope.searchByActList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Defined Name',
            'value': 'UserName'
        }
    ];

    $scope.Actparameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "Sequence",
        searchBy: "UserName",
        pageSize: 15,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getData = function (pageno) {
        //baseService.paginationBase($scope.getListUrl, pageno, $scope.Actparameters)
        baseService.init($scope.getListUrl, pageno, 15, 'asc', 'Sequence', $scope.Actparameters.search)
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.activities = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    //$scope.getData();

    $scope.activity = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        ActivityType: null,
        ActivityId: null,
        FALinked: null,
        Description: null,
        Remarks: null,
        Active: true
    };

   

    $scope.GetSequenceActivity = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.budget.Sequence = response.data;
            });
    };
    $scope.GetSequenceActivity();

    //cboService.getEnumCbo("enum/GetActivityTypeCbo", function (result) {
    //    $scope.activityTypeList = result;
    //});

    //cboService.getEnumCbo("enum/GetCboFALinked", function (result) {
    //    $scope.fALinkedList = result;
    //});

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.activity = $scope.activities[$scope.index];
        $scope.activity.AddedDate = $filter("dateFilter")($scope.activity.AddedDate);
        $scope.activity.UpdatedDate = $filter("dateFilter")($scope.activity.UpdatedDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.invalidFALink = false;
    $scope.fALinkValidation = function () {
        $scope.invalidFALink = baseService.isUndefinedOrNull($scope.activity.FALinked);
        return manualValidation("div_FA", $scope.invalidFALink, "FA Link is required.");
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.activity.IsFABased) {
            $scope.fALinkValidation();
        }
        if ($scope.activityForm.$valid && !$scope.invalidFALink) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: $scope.activity,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.activities.push(response.data.Activity);
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
                    data: $scope.activity,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.activities[$scope.index] = $scope.activity;
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

    $scope.DeleteActivity = function () {
        if (!baseService.isUndefinedOrNull($scope.activity.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.activity.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.activities.splice($scope.index, 1);
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
        ClearFields($scope.GetSequenceActivity());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.activity = {};
        $scope.activity.Sequence = seq;
        $scope.activity.Active = true;
    }
    //  #endregion Activity

    $scope.message_Detailconfirmation = null;
    $scope.RemoveBudgetGroup = function () {

        if (!baseService.isUndefinedOrNull($scope.budgetGroup.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpBudgetGroup')).modal('show');
    }

    $scope.RemoveBudgetCategory = function () {

        if (!baseService.isUndefinedOrNull($scope.budgetCategory.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpBudgetcategory')).modal('show');
    }

    $scope.RemoveBudgetSubCategory = function () {

        if (!baseService.isUndefinedOrNull($scope.budgetSubCategory.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpBudgetSubCategory')).modal('show');
    }

    $scope.RemoveBudget = function () {

        if (!baseService.isUndefinedOrNull($scope.budget.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpBudget')).modal('show');
    }

    $scope.RemoveActivity = function () {

        if (!baseService.isUndefinedOrNull($scope.activity.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpActivity')).modal('show');
    }
}